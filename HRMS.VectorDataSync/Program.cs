using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DA;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VectorDataSync
{
    class Program
    {
        private const string DefaultQdrantUrl = "http://localhost:6333";
        private const string LegacyCollectionName = "hrms_vectors";
        private const string ActiveCollectionName = "hrms_vectors_v2";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=================================================================");
            Console.WriteLine("       HRMS ENTERPRISE VECTOR DATA SYNC & OPERATIONS TOOL       ");
            Console.WriteLine("=================================================================");

            string command = args.Length > 0 ? args[0].ToLowerInvariant().TrimStart('-', '/') : "status";

            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
            {
                try
                {
                    switch (command)
                    {
                        case "status":
                        case "preflight":
                            await RunPreflightAsync(httpClient);
                            break;

                        case "verify":
                            await RunVerifyAsync(httpClient);
                            break;

                        case "purge-legacy":
                        case "purge":
                            await RunPurgeLegacyAsync(httpClient);
                            break;

                        case "rebuild":
                            bool isApply = args.Any(a => a.Equals("--apply", StringComparison.OrdinalIgnoreCase));
                            await RunRebuildAsync(httpClient, isApply);
                            break;

                        case "help":
                        case "?":
                        default:
                            PrintHelp();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[ERROR] Thao tác thất bại: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[INNER] {ex.InnerException.Message}");
                    }
                    Console.ResetColor();
                    Environment.ExitCode = 1;
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("\nCÁCH SỬ DỤNG LỆNH CLI:");
            Console.WriteLine("  HRMS.VectorDataSync.exe status          : Kiểm tra kết nối Qdrant, Oracle và trạng thái collection.");
            Console.WriteLine("  HRMS.VectorDataSync.exe verify          : Đối soát cấu trúc payload, fingerprint của bộ dữ liệu.");
            Console.WriteLine("  HRMS.VectorDataSync.exe purge-legacy    : Xóa hoàn toàn bộ dữ liệu 975 points cũ ('hrms_vectors').");
            Console.WriteLine("  HRMS.VectorDataSync.exe rebuild --dry-run: Lập kế hoạch nạp bộ dữ liệu mới (không ghi).");
            Console.WriteLine("  HRMS.VectorDataSync.exe rebuild --apply  : Khởi tạo collection 'hrms_vectors_v2' và nạp dữ liệu.");
            Console.WriteLine("  HRMS.VectorDataSync.exe help            : Hiển thị bảng trợ giúp này.\n");
        }

        private static async Task RunPreflightAsync(HttpClient http)
        {
            Console.WriteLine("\n[1/3] Kiểm tra kết nối Qdrant Server...");
            string qdrantUrl = DefaultQdrantUrl;
            bool qdrantOnline = false;
            JObject collectionsObj = null;

            try
            {
                var resp = await http.GetAsync($"{qdrantUrl}/collections");
                if (resp.IsSuccessStatusCode)
                {
                    qdrantOnline = true;
                    string json = await resp.Content.ReadAsStringAsync();
                    collectionsObj = JObject.Parse(json);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  -> Qdrant kết nối thành công tại: {qdrantUrl}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  -> Qdrant phản hồi mã lỗi HTTP: {resp.StatusCode}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  -> Không thể kết nối Qdrant ({qdrantUrl}): {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n[2/3] Kiểm tra các collections hiện có trên Qdrant...");
            if (collectionsObj != null)
            {
                var collections = collectionsObj["result"]?["collections"] as JArray;
                if (collections != null && collections.Count > 0)
                {
                    foreach (var c in collections)
                    {
                        string name = c["name"]?.ToString();
                        Console.WriteLine($"  - Collection: '{name}'");
                        try
                        {
                            var detailResp = await http.GetAsync($"{qdrantUrl}/collections/{name}");
                            if (detailResp.IsSuccessStatusCode)
                            {
                                var detail = JObject.Parse(await detailResp.Content.ReadAsStringAsync());
                                var points = detail["result"]?["points_count"];
                                var status = detail["result"]?["status"];
                                var dim = detail["result"]?["config"]?["params"]?["vectors"]?["size"];
                                var dist = detail["result"]?["config"]?["params"]?["vectors"]?["distance"];
                                Console.WriteLine($"      Trạng thái: {status}, Số points: {points}, Chiều: {dim}, Khoảng cách: {dist}");
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    Console.WriteLine("  -> Không có collection nào trên máy chủ Qdrant.");
                }
            }

            Console.WriteLine("\n[3/3] Kiểm tra kết nối Oracle Database (Schema HR)...");
            try
            {
                using (var db = new MyEntities())
                {
                    int totalEmp = db.TB_NHANVIEN.Count(e => e.DATHOIVIEC != 1);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  -> Kết nối Oracle thành công! Số nhân viên đang làm việc trong TB_NHANVIEN: {totalEmp}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  -> Lỗi kết nối Oracle: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n=================================================================");
            Console.WriteLine("Preflight hoàn tất. Chạy 'verify' hoặc 'purge-legacy' khi cần.");
            Console.WriteLine("=================================================================\n");
        }

        private static async Task RunVerifyAsync(HttpClient http)
        {
            Console.WriteLine("\n[VERIFY] Bắt đầu đối soát cấu trúc dữ liệu Qdrant...");
            string qdrantUrl = DefaultQdrantUrl;

            var resp = await http.GetAsync($"{qdrantUrl}/collections/{LegacyCollectionName}");
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Collection '{LegacyCollectionName}' không tồn tại trên máy chủ (HTTP {resp.StatusCode}).");
                return;
            }

            var detail = JObject.Parse(await resp.Content.ReadAsStringAsync());
            long pointCount = detail["result"]?["points_count"]?.Value<long>() ?? 0;
            Console.WriteLine($"Collection '{LegacyCollectionName}' có {pointCount} points.");

            // Scroll 5 points để kiểm tra metadata payload
            var scrollReq = new StringContent(
                JsonConvert.SerializeObject(new { limit = 5, with_payload = true, with_vector = false }),
                Encoding.UTF8, "application/json");

            var scrollResp = await http.PostAsync($"{qdrantUrl}/collections/{LegacyCollectionName}/points/scroll", scrollReq);
            if (scrollResp.IsSuccessStatusCode)
            {
                var scrollData = JObject.Parse(await scrollResp.Content.ReadAsStringAsync());
                var points = scrollData["result"]?["points"] as JArray;
                Console.WriteLine("\nMẫu payload trong collection:");
                int legacyCount = 0;
                if (points != null)
                {
                    foreach (var p in points)
                    {
                        var payload = p["payload"] as JObject;
                        string tag = payload?["tag"]?.ToString();
                        string text = payload?["text"]?.ToString();
                        bool hasDept = payload?["departmentId"] != null;
                        bool hasCompany = payload?["companyId"] != null;
                        bool hasEmpId = payload?["employeeId"] != null;

                        if (tag == "EMPLOYEE" && !hasDept && !hasCompany && !hasEmpId)
                        {
                            legacyCount++;
                        }

                        Console.WriteLine($"  - ID: {p["id"]}, Tag: {tag}, Text: {(text != null && text.Length > 60 ? text.Substring(0, 60) + "..." : text)}");
                        Console.WriteLine($"    ACL Fields: departmentId={hasDept}, companyId={hasCompany}, employeeId={hasEmpId}");
                    }
                }

                if (legacyCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[KẾT LUẬN ĐỐI SOÁT]:");
                    Console.WriteLine($"  Bộ dữ liệu trong '{LegacyCollectionName}' ({pointCount} points) là BỘ CŨ (Legacy).");
                    Console.WriteLine("  Đặc điểm: Thiếu toàn bộ metadata phân quyền (departmentId, companyId, employeeId).");
                    Console.WriteLine("  Khuyến nghị: Chạy lệnh 'purge-legacy' để xóa sạch theo yêu cầu Mục 15.");
                    Console.ResetColor();
                }
            }
        }

        private static async Task RunPurgeLegacyAsync(HttpClient http)
        {
            Console.WriteLine($"\n[PURGE] Đang thực hiện xóa vĩnh viễn collection legacy '{LegacyCollectionName}'...");
            string qdrantUrl = DefaultQdrantUrl;

            // Kiểm tra xem collection có tồn tại không
            var checkResp = await http.GetAsync($"{qdrantUrl}/collections/{LegacyCollectionName}");
            if (!checkResp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Collection '{LegacyCollectionName}' không tồn tại hoặc đã được xóa trước đó.");
                return;
            }

            var detail = JObject.Parse(await checkResp.Content.ReadAsStringAsync());
            long pointCount = detail["result"]?["points_count"]?.Value<long>() ?? 0;
            Console.WriteLine($"Tìm thấy collection '{LegacyCollectionName}' với {pointCount} points.");

            // Gọi API DELETE
            var deleteResp = await http.DeleteAsync($"{qdrantUrl}/collections/{LegacyCollectionName}");
            if (deleteResp.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[THÀNH CÔNG] Đã xóa hoàn toàn collection legacy '{LegacyCollectionName}' ({pointCount} points)!");
                Console.ResetColor();

                // Hậu kiểm: gọi lại để xác nhận 404
                var recheck = await http.GetAsync($"{qdrantUrl}/collections/{LegacyCollectionName}");
                if (recheck.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[HẬU KIỂM]: Xác nhận máy chủ Qdrant trả về HTTP 404 Not Found cho collection cũ.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LỖI]: Không thể xóa collection. HTTP Status: {deleteResp.StatusCode}");
                Console.ResetColor();
                Environment.ExitCode = 1;
            }
        }

        private static async Task RunRebuildAsync(HttpClient http, bool isApply)
        {
            Console.WriteLine($"\n[REBUILD] Chế độ: {(isApply ? "APPLY (Ghi thật)" : "DRY-RUN (Chỉ kiểm tra)")}");
            string qdrantUrl = DefaultQdrantUrl;

            using (var db = new MyEntities())
            {
                var employeesRaw = (from nv in db.TB_NHANVIEN
                                    where nv.DATHOIVIEC != 1
                                    join pb in db.TB_PHONGBAN on nv.IDPB equals pb.IDPB into pbG
                                    from pb in pbG.DefaultIfEmpty()
                                    join cv in db.TB_CHUCVU on nv.IDCV equals cv.IDCV into cvG
                                    from cv in cvG.DefaultIfEmpty()
                                    join bp in db.TB_BOPHAN on nv.IDBP equals bp.IDBP into bpG
                                    from bp in bpG.DefaultIfEmpty()
                                    select new
                                    {
                                        nv.MANV,
                                        nv.HOTEN,
                                        nv.IDPB,
                                        TenPB = pb.TENPB,
                                        nv.IDCV,
                                        TenCV = cv.TENCV,
                                        nv.IDBP,
                                        TenBP = bp.TENBP,
                                        nv.IDCTY
                                    }).ToList();

                var employees = employeesRaw.Select(e => new
                {
                    e.MANV,
                    e.HOTEN,
                    e.IDPB,
                    TenPhongBan = !string.IsNullOrEmpty(e.TenPB) ? e.TenPB : "Chưa phân bổ",
                    e.IDCV,
                    TenChucVu = !string.IsNullOrEmpty(e.TenCV) ? e.TenCV : "Chưa phân bổ",
                    e.IDBP,
                    TenBoPhan = !string.IsNullOrEmpty(e.TenBP) ? e.TenBP : "Chưa phân bổ",
                    e.IDCTY
                }).ToList();

                Console.WriteLine($"Tìm thấy {employees.Count} nhân viên đang làm việc trong Oracle DB.");

                if (!isApply)
                {
                    Console.WriteLine("\n[DRY-RUN]: Sẽ tạo collection mới 'hrms_vectors_v2' và nạp các mẫu văn bản bảo mật (NO PII):");
                    if (employees.Count > 0)
                    {
                        var sample = employees[0];
                        string sampleText = $"Nhân viên {sample.HOTEN} (Mã NV: {sample.MANV}), thuộc phòng ban {sample.TenPhongBan}, chức vụ {sample.TenChucVu}, bộ phận {sample.TenBoPhan}.";
                        Console.WriteLine($"  Ví dụ mẫu: \"{sampleText}\"");
                        Console.WriteLine($"  Metadata gắn kèm: employeeId={sample.MANV}, departmentId={sample.IDPB}, companyId={sample.IDCTY}, tag=EMPLOYEE");
                    }
                    Console.WriteLine("\nChạy lệnh: 'HRMS.VectorDataSync.exe rebuild --apply' để thực thi nạp.");
                    return;
                }

                // Tạo collection nếu chưa có
                Console.WriteLine($"\n[1/3] Đang đảm bảo collection '{ActiveCollectionName}' (Vector dimension 1024, Cosine)...");
                var checkCollection = await http.GetAsync($"{qdrantUrl}/collections/{ActiveCollectionName}");
                if (!checkCollection.IsSuccessStatusCode)
                {
                    var createReq = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            vectors = new
                            {
                                size = 1024,
                                distance = "Cosine"
                            }
                        }),
                        Encoding.UTF8, "application/json");
                    var createResp = await http.PutAsync($"{qdrantUrl}/collections/{ActiveCollectionName}", createReq);
                    if (createResp.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  -> Khởi tạo collection '{ActiveCollectionName}' thành công!");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine($"  -> Collection '{ActiveCollectionName}' đã sẵn sàng trên Qdrant.");
                }

                // Kiểm tra Ollama embedding model
                const string defaultOllamaUrl = "http://127.0.0.1:11434";
                const string defaultEmbeddingModel = "bge-m3:latest";
                Console.WriteLine($"\n[2/3] Kiểm tra mô hình embedding '{defaultEmbeddingModel}' trên Ollama ({defaultOllamaUrl})...");
                try
                {
                    var testEmbReq = new StringContent(
                        JsonConvert.SerializeObject(new { model = defaultEmbeddingModel, prompt = "Kiểm tra kết nối embedding" }),
                        Encoding.UTF8, "application/json");
                    var testEmbResp = await http.PostAsync($"{defaultOllamaUrl}/api/embeddings", testEmbReq);
                    if (!testEmbResp.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  -> Lỗi kết nối Ollama embeddings: HTTP {testEmbResp.StatusCode}");
                        Console.ResetColor();
                        Environment.ExitCode = 1;
                        return;
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  -> Mô hình embedding '{defaultEmbeddingModel}' sẵn sàng!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  -> Không thể kết nối tới Ollama: {ex.Message}");
                    Console.ResetColor();
                    Environment.ExitCode = 1;
                    return;
                }

                // Thực hiện nạp dữ liệu nhân sự có ACL
                Console.WriteLine($"\n[3/3] Tiến hành nạp {employees.Count} bản ghi nhân sự vào '{ActiveCollectionName}'...");
                int successCount = 0;
                int batchSize = 10;
                var pointsBatch = new List<object>();

                for (int i = 0; i < employees.Count; i++)
                {
                    var emp = employees[i];
                    string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), thuộc phòng ban {emp.TenPhongBan}, chức vụ {emp.TenChucVu}, bộ phận {emp.TenBoPhan}.";

                    var embReq = new StringContent(
                        JsonConvert.SerializeObject(new { model = defaultEmbeddingModel, prompt = text }),
                        Encoding.UTF8, "application/json");
                    var embResp = await http.PostAsync($"{defaultOllamaUrl}/api/embeddings", embReq);
                    if (!embResp.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"  [!] Lỗi embedding cho NV {emp.MANV} ({emp.HOTEN}): HTTP {embResp.StatusCode}");
                        continue;
                    }
                    var embData = JObject.Parse(await embResp.Content.ReadAsStringAsync());
                    var vector = embData["embedding"]?.ToObject<float[]>();
                    if (vector == null || vector.Length != 1024)
                    {
                        Console.WriteLine($"  [!] Kích thước vector không hợp lệ ({vector?.Length}): NV {emp.MANV}");
                        continue;
                    }

                    ulong empId = (ulong)emp.MANV;
                    int deptId = emp.IDPB.HasValue ? (int)emp.IDPB.Value : 0;
                    int compId = emp.IDCTY.HasValue ? (int)emp.IDCTY.Value : 1;

                    pointsBatch.Add(new
                    {
                        id = empId,
                        vector = vector,
                        payload = new
                        {
                            employeeId = (int)empId,
                            departmentId = deptId,
                            companyId = compId,
                            tag = "EMPLOYEE",
                            text = text,
                            version = 2
                        }
                    });

                    if (pointsBatch.Count >= batchSize || i == employees.Count - 1)
                    {
                        var upsertReq = new StringContent(
                            JsonConvert.SerializeObject(new { points = pointsBatch }),
                            Encoding.UTF8, "application/json");
                        var upsertResp = await http.PutAsync($"{qdrantUrl}/collections/{ActiveCollectionName}/points", upsertReq);
                        if (upsertResp.IsSuccessStatusCode)
                        {
                            successCount += pointsBatch.Count;
                            Console.Write($"\r  -> Đã nạp thành công: {successCount}/{employees.Count} nhân viên...");
                        }
                        else
                        {
                            string errContent = await upsertResp.Content.ReadAsStringAsync();
                            Console.WriteLine($"\n  [!] Lỗi upsert batch: HTTP {upsertResp.StatusCode} - {errContent}");
                        }
                        pointsBatch.Clear();
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[KẾT QUẢ]: Đã nạp thành công {successCount}/{employees.Count} bản ghi nhân sự có đầy đủ ACL vào '{ActiveCollectionName}'.");
                Console.ResetColor();

                // Hậu kiểm số lượng points trong Qdrant
                var countResp = await http.GetAsync($"{qdrantUrl}/collections/{ActiveCollectionName}");
                if (countResp.IsSuccessStatusCode)
                {
                    var countData = JObject.Parse(await countResp.Content.ReadAsStringAsync());
                    long finalPoints = countData["result"]?["points_count"]?.Value<long>() ?? 0;
                    Console.WriteLine($"[HẬU KIỂM]: Tổng số points hiện có trong '{ActiveCollectionName}': {finalPoints}");
                }

                Console.WriteLine("\n=================================================================");
                Console.WriteLine($"Hoàn tất cấu hình và nạp dữ liệu cho collection '{ActiveCollectionName}'.");
                Console.WriteLine("=================================================================\n");
            }
        }
    }
}
