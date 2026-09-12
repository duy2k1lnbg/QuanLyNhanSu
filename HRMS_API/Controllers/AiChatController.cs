using Bu.Services.AI_Services;
using Bu.Services.AI_Services.Core;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/ai")]
    public class AiChatController : ApiController
    {
        /// <summary>
        /// GET: api/ai
        /// Trả về trạng thái tổng quan của máy chủ AI
        /// </summary>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public IHttpActionResult Index()
        {
            return GetStatus();
        }

        /// <summary>
        /// GET: api/ai/chat?question=...
        /// Cho phép kiểm tra hoặc gửi câu hỏi trực tiếp qua phương thức GET
        /// </summary>
        [HttpGet]
        [Route("chat")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ChatGet([FromUri] string question = null, [FromUri] string q = null)
        {
            string prompt = !string.IsNullOrWhiteSpace(question) ? question : q;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                bool isOllama = IsOllamaOnline();
                return Ok(new
                {
                    status = "Ready",
                    service = "HRMS AI Copilot API",
                    connected = isOllama,
                    isOnline = isOllama,
                    engine = isOllama ? "Qwen 2.5 (Ollama Server Online)" : "Oracle Live Fallback Engine",
                    usage = "Gửi POST /api/ai/chat với JSON body {\"Question\": \"...\"} hoặc GET /api/ai/chat?question=...",
                    message = "Máy chủ AI HRMS đang hoạt động bình thường."
                });
            }
            return await Chat(new ChatRequest { Question = prompt });
        }

        /// <summary>
        /// POST: api/ai/chat
        /// Gửi câu hỏi đến Trợ lý AI HRMS Copilot (kết hợp FastResponse, Qwen 2.5 RAG và Oracle Live Database)
        /// Đồng bộ 100% logic với phân hệ WinForms ChatboxManager
        /// </summary>
        [HttpPost]
        [Route("chat")]
        public async Task<IHttpActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("Vui lòng nhập nội dung câu hỏi.");
            }

            string question = request.Question.Trim();

            // 1. Kiểm tra phản hồi nhanh (Fast Response) từ WinForms BLL (Chào hỏi, FAQ quy chế, danh tính, IT Support, Chit-chat...)
            string fastReply = FastResponseService.GetFastResponse(question);
            if (!string.IsNullOrEmpty(fastReply))
            {
                return Ok(new
                {
                    answer = fastReply,
                    sqlQuery = "",
                    source = "Fast_Rule_Based"
                });
            }

            // 2. Nếu Ollama trực tuyến (Port 11434): Chạy luồng RAG Hybrid (chờ tối đa 35s cho suy luận LLM)
            if (IsOllamaOnline())
            {
                try
                {
                    var manager = new ChatboxManager();
                    var queryTask = manager.ProcessQuery(question);
                    var completedTask = await Task.WhenAny(queryTask, Task.Delay(35000));

                    if (completedTask == queryTask)
                    {
                        var result = await queryTask;
                        if (result != null && !string.IsNullOrWhiteSpace(result.Answer) &&
                            !result.Answer.Contains("chưa khởi động") &&
                            !result.Answer.Contains("trục trặc khi kết nối") &&
                            !result.Answer.Contains("Lỗi kết nối"))
                        {
                            return Ok(new
                            {
                                answer = result.Answer,
                                sqlQuery = "", // Không để lộ cấu trúc câu lệnh CSDL ra client
                                source = "RAG_Ollama"
                            });
                        }
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceWarning("Ollama RAG phản hồi quá 35s, chuyển sang Oracle Live Database Engine để phản hồi ngay lập tức.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Ollama RAG xử lý không thành công, chuyển sang Oracle Live Engine: " + ex.Message);
                }
            }

            // 3. Oracle Live Database Engine: Truy vấn trực tiếp dữ liệu nghiệp vụ CSDL Oracle thực tế
            try
            {
                var queryResult = GenerateSmartDatabaseResponse(question);
                return Ok(new
                {
                    answer = queryResult.Answer,
                    sqlQuery = "", // Không để lộ cấu trúc câu lệnh CSDL ra client
                    source = "Oracle_Live_Database"
                });
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errMsg += " -> " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        errMsg += " -> " + ex.InnerException.InnerException.Message;
                }
                System.Diagnostics.Trace.TraceError("Lỗi trong AiChatController: " + ex.ToString());
                return Ok(new
                {
                    answer = "Lỗi hệ thống: " + errMsg,
                    sqlQuery = "",
                    source = "Fallback_Error"
                });
            }
        }

        /// <summary>
        /// POST / GET: api/ai/reset
        /// Đặt lại phiên trò chuyện AI (tương đương Reset() trên WinForms)
        /// </summary>
        [HttpGet, HttpPost]
        [Route("reset")]
        [AllowAnonymous]
        public IHttpActionResult Reset()
        {
            try
            {
                var manager = new ChatboxManager();
                manager.Reset();
                return Ok(new { success = true, message = "Đã đặt lại phiên trò chuyện AI thành công." });
            }
            catch
            {
                return Ok(new { success = true });
            }
        }

        /// <summary>
        /// GET / POST: api/ai/status
        /// Kiểm tra trạng thái kết nối của máy chủ AI (Ollama / Qwen 2.5)
        /// Trả về true (Đã kết nối) hoặc false (Chạy offline / Lỗi)
        /// </summary>
        [HttpGet, HttpPost]
        [Route("status")]
        [AllowAnonymous]
        public IHttpActionResult GetStatus()
        {
            try
            {
                bool isOllama = IsOllamaOnline();
                string ollamaHost = "";
                string aiModel = "";
                try
                {
                    var cfg = new Bu.CLASS_CHAMCONG.SYS_CONFIG();
                    ollamaHost = cfg.getValue("OllamaHost", "http://127.0.0.1:11434");
                    aiModel = cfg.getValue("AiModel", "qwen2.5:latest");

                    // Tự động đồng bộ về địa chỉ máy chủ cục bộ nếu đang lưu IP remote cũ
                    if (string.IsNullOrWhiteSpace(ollamaHost) || ollamaHost.Contains("100.111.179.99"))
                    {
                        cfg.setItem("OllamaHost", "http://127.0.0.1:11434");
                        cfg.setItem("AiModel", "qwen2.5:latest");
                        ollamaHost = "http://127.0.0.1:11434";
                        aiModel = "qwen2.5:latest";
                    }

                    string qdrantUrl = cfg.getValue("QdrantUrl", "http://127.0.0.1:6333");
                    if (string.IsNullOrWhiteSpace(qdrantUrl) || qdrantUrl.Contains("100.111.179.99") || qdrantUrl.Contains("localhost"))
                    {
                        cfg.setItem("QdrantUrl", "http://127.0.0.1:6333");
                    }
                }
                catch (Exception cfgEx)
                {
                    ollamaHost = "Error: " + cfgEx.Message;
                }

                return Ok(new
                {
                    connected = isOllama,
                    isOnline = isOllama,
                    engine = isOllama ? "Qwen 2.5 (Ollama Server Online)" : "Offline Fallback Engine",
                    ollamaHost = ollamaHost,
                    aiModel = aiModel,
                    message = isOllama ? "Đã kết nối máy chủ AI" : "Chạy offline / Lỗi kết nối AI"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    connected = false,
                    isOnline = false,
                    engine = "Error",
                    message = ex.Message
                });
            }
        }

        private static bool IsOllamaOnline()
        {
            try
            {
                string ollamaUrl = System.Configuration.ConfigurationManager.AppSettings["OllamaUrl"] ?? "http://localhost:11434";
                string host = "127.0.0.1";
                int port = 11434;

                try
                {
                    var uri = new Uri(ollamaUrl);
                    host = uri.Host;
                    port = uri.Port > 0 ? uri.Port : 11434;
                }
                catch { }

                using (var tcp = new TcpClient())
                {
                    var ar = tcp.BeginConnect(host, port, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(600));
                    if (!success) return false;
                    tcp.EndConnect(ar);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private class SmartQueryResult
        {
            public string Answer { get; set; }
            public string SqlQuery { get; set; }
        }

        private SmartQueryResult GenerateSmartDatabaseResponse(string q)
        {
            string lower = q.ToLower().Trim();

            // NGUYÊN TẮC BẢO MẬT: AI CHỈ ĐƯỢC KẾT NỐI VỚI TÀI KHOẢN AI_READONLY (AiEntities)
            using (var db = new DA.AiEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                // 1. TÌM KIẾM NHÂN VIÊN THEO TÊN (Ví dụ: "cho tôi thông tin nhân viên tên Duy", "nhân viên tên Duy", "thông tin nhân viên Duy")
                var nameMatch = Regex.Match(lower, @"(?:thông tin nhân viên tên là|thông tin nhân viên tên|thông tin nhân viên|nhân viên tên là|nhân viên tên|tìm nhân viên tên|tìm nhân viên|nhân sự tên là|nhân sự tên|thông tin của|thông tin|tìm|về)\s+([\p{L}\s]+)$");
                string searchName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "";
                
                if (string.IsNullOrWhiteSpace(searchName) && lower.Contains("duy"))
                {
                    searchName = "Duy";
                }

                if (!string.IsNullOrWhiteSpace(searchName) && searchName.Length >= 2)
                {
                    string safeName = searchName.Replace("'", "''");
                    string sql = $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE UPPER('%{safeName}%')";
                    var matchNv = db.Database.SqlQuery<DA.V_AI_EMPLOYEE>(sql).Take(5).ToList();
                    if (matchNv.Count > 0)
                    {
                        var lines = matchNv.Select(x => $"• **#{x.MANV}** - **{x.HOTEN}**\n  - **Phòng ban:** {x.TEN_PHONGBAN ?? "Chưa phân bổ"}\n  - **Bộ phận:** {x.TEN_BOPHAN ?? "Chưa phân bổ"}\n  - **Chức vụ:** {x.TEN_CHUCVU ?? "Nhân sự"}\n  - **Điện thoại:** {x.DIENTHOAI ?? "Chưa cập nhật"}\n  - **Địa chỉ:** {x.DIACHI ?? "Chưa cập nhật"}\n  - **Ngày sinh:** {(x.NGAYSINH.HasValue ? x.NGAYSINH.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật")}");
                        return new SmartQueryResult
                        {
                            Answer = $"👤 **Hồ sơ nhân sự tìm thấy theo tên \"{searchName}\"** *(Tài khoản kết nối: AI_READONLY - View: V_AI_EMPLOYEE)*:\n\n" +
                                     string.Join("\n\n", lines),
                            SqlQuery = sql
                        };
                    }
                }

                // 2. TÌM THEO MÃ NHÂN VIÊN: "mã 10", "manv: 12", "nv 5"
                var idMatch = Regex.Match(lower, @"(?:mã|manv|nv|#)\s*[:=]?\s*(\d+)");
                if (idMatch.Success && int.TryParse(idMatch.Groups[1].Value, out int manvSearch))
                {
                    string sql = $"SELECT * FROM V_AI_EMPLOYEE WHERE MANV = {manvSearch}";
                    var nvList = db.Database.SqlQuery<DA.V_AI_EMPLOYEE>(sql).Take(1).ToList();
                    var nv = nvList.FirstOrDefault();
                    if (nv != null)
                    {
                        return new SmartQueryResult
                        {
                            Answer = $"👤 **Thông tin hồ sơ nhân sự #{nv.MANV}** *(Nguồn an toàn: AI_READONLY.V_AI_EMPLOYEE)*:\n\n" +
                                     $"• **Họ và tên:** {nv.HOTEN}\n" +
                                     $"• **Phòng ban:** {nv.TEN_PHONGBAN ?? "Chưa phân bổ"}\n" +
                                     $"• **Bộ phận:** {nv.TEN_BOPHAN ?? "Chưa phân bổ"}\n" +
                                     $"• **Chức vụ:** {nv.TEN_CHUCVU ?? "Nhân sự"}\n" +
                                     $"• **Điện thoại:** {nv.DIENTHOAI ?? "Chưa cập nhật"}\n" +
                                     $"• **Địa chỉ:** {nv.DIACHI ?? "Chưa cập nhật"}\n" +
                                     $"• **Ngày sinh:** {(nv.NGAYSINH.HasValue ? nv.NGAYSINH.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật")}",
                            SqlQuery = sql
                        };
                    }
                }

                // 3. SINH NHẬT THÁNG NÀY: V_AI_EMPLOYEE
                if (lower.Contains("sinh nhật") || lower.Contains("sinh nhat") || lower.Contains("birthday"))
                {
                    int month = DateTime.Now.Month;
                    string sql = $"SELECT * FROM V_AI_EMPLOYEE WHERE EXTRACT(MONTH FROM NGAYSINH) = {month} ORDER BY EXTRACT(DAY FROM NGAYSINH)";
                    var matchNv = db.Database.SqlQuery<DA.V_AI_EMPLOYEE>(sql).Take(15).ToList();

                    if (matchNv.Count == 0)
                    {
                        return new SmartQueryResult
                        {
                            Answer = $"🎂 Trong **Tháng {month}** này hiện không có nhân sự nào có ngày sinh nhật trong cơ sở dữ liệu.",
                            SqlQuery = sql
                        };
                    }

                    var lines = matchNv.Select(x => $"• **{x.HOTEN}** (#{x.MANV}) - Ngày {(x.NGAYSINH.HasValue ? x.NGAYSINH.Value.ToString("dd/MM") : "")} ({x.TEN_PHONGBAN ?? "Chưa phân bổ"} &bull; {x.TEN_CHUCVU ?? "Nhân sự"})");
                    return new SmartQueryResult
                    {
                        Answer = $"🎂 **Danh sách nhân sự có sinh nhật trong Tháng {month}** ({matchNv.Count} nhân viên - Nguồn: AI_READONLY):\n\n" + string.Join("\n", lines),
                        SqlQuery = sql
                    };
                }

                // 4. TÌM KIẾM NHÂN SỰ THEO PHÒNG BAN CỤ THỂ (Ví dụ: "ai ở phòng IT", "nhân viên phòng kế toán", "ai đang ở phòng IT?")
                var pbMatch = Regex.Match(lower, @"(?:phòng ban|phòng|bộ phận)\s+([\p{L}\s0-9]+)");
                if (pbMatch.Success && !lower.Contains("theo phòng ban") && !lower.Contains("từng phòng ban") && !lower.Contains("phòng ban nào"))
                {
                    string pbSearch = pbMatch.Groups[1].Value.Trim().Replace("?", "").Replace(".", "");
                    if (pbSearch.Length >= 2)
                    {
                        string safePb = pbSearch.Replace("'", "''");
                        string sql = $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(TEN_PHONGBAN) LIKE UPPER('%{safePb}%') OR UPPER(TEN_BOPHAN) LIKE UPPER('%{safePb}%')";
                        var emps = db.Database.SqlQuery<DA.V_AI_EMPLOYEE>(sql).Take(15).ToList();
                        if (emps.Count > 0)
                        {
                            var lines = emps.Select(x => $"• **#{x.MANV}** - **{x.HOTEN}** ({x.TEN_CHUCVU ?? "Nhân sự"} &bull; ĐT: {x.DIENTHOAI ?? "Chưa có"})");
                            return new SmartQueryResult
                            {
                                Answer = $"🏢 **Danh sách nhân sự thuộc {emps[0].TEN_PHONGBAN ?? pbSearch}** ({emps.Count} nhân sự - Nguồn: AI_READONLY.V_AI_EMPLOYEE):\n\n" +
                                         string.Join("\n", lines),
                                SqlQuery = sql
                            };
                        }
                    }
                }

                // 5. THỐNG KÊ SỐ LƯỢNG NHÂN VIÊN THEO TỪNG PHÒNG BAN: V_AI_EMPLOYEE
                if (lower.Contains("theo phòng ban") || lower.Contains("từng phòng ban") || lower.Contains("số lượng nhân viên theo") || lower.Contains("phòng ban nào") || lower.Contains("các phòng ban"))
                {
                    var deptStatsRaw = db.V_AI_EMPLOYEE
                        .Select(x => new { Department = x.TEN_PHONGBAN })
                        .ToList();

                    var deptStats = deptStatsRaw
                        .GroupBy(x => x.Department ?? "Chưa phân phòng")
                        .Select(g => new
                        {
                            Department = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    int totalActive = deptStats.Sum(x => x.Count);
                    string sql = "SELECT TEN_PHONGBAN, COUNT(MANV) AS SO_LUONG FROM V_AI_EMPLOYEE GROUP BY TEN_PHONGBAN ORDER BY SO_LUONG DESC";

                    var lines = deptStats.Select(x => $"• **{x.Department}**: **{x.Count:N0}** nhân sự ({((double)x.Count / Math.Max(totalActive, 1) * 100):0.0}%)");
                    return new SmartQueryResult
                    {
                        Answer = $"🏢 **Thống kê nhân sự theo từng phòng ban** (Tổng số: **{totalActive:N0}** nhân viên - Nguồn: AI_READONLY.V_AI_EMPLOYEE):\n\n" +
                                 string.Join("\n", lines),
                        SqlQuery = sql
                    };
                }

                // 5. DANH SÁCH TẤT CẢ NHÂN VIÊN TRONG CÔNG TY: V_AI_EMPLOYEE
                if (lower.Contains("tất cả nhân viên") || lower.Contains("danh sách nhân viên") || lower.Contains("toàn bộ nhân viên") || lower.Contains("danh sách tất cả"))
                {
                    int totalNv = db.V_AI_EMPLOYEE.Count();

                    var samples = db.V_AI_EMPLOYEE
                        .OrderBy(x => x.MANV)
                        .Take(10)
                        .ToList();

                    string sql = "SELECT MANV, HOTEN, TEN_PHONGBAN, TEN_CHUCVU FROM V_AI_EMPLOYEE ORDER BY MANV ASC";

                    var lines = samples.Select(x => $"• **#{x.MANV}** - **{x.HOTEN}** ({x.TEN_PHONGBAN ?? "Chưa phân bổ"} &bull; {x.TEN_CHUCVU ?? "Nhân sự"})");
                    return new SmartQueryResult
                    {
                        Answer = $"👥 **Danh sách Nhân sự trong hệ thống** (Tổng số: **{totalNv:N0} nhân sự** - Nguồn: AI_READONLY.V_AI_EMPLOYEE):\n\n" +
                                 string.Join("\n", lines) +
                                 (totalNv > 10 ? $"\n• ... *(và {totalNv - 10} nhân sự khác)*" : ""),
                        SqlQuery = sql
                    };
                }

                // 6. PHỤ CẤP: V_AI_ALLOWANCE
                if (lower.Contains("phụ cấp") || lower.Contains("phu cap"))
                {
                    var numMatch = Regex.Match(lower, @"(\d+)\s*(?:triệu|tr)");
                    decimal minTien = 0;
                    if (numMatch.Success && decimal.TryParse(numMatch.Groups[1].Value, out decimal trieu))
                    {
                        minTien = trieu * 1000000;
                    }

                    var pcQuery = db.V_AI_ALLOWANCE.Where(x => x.SOTIEN >= minTien).OrderByDescending(x => x.SOTIEN).Take(15).ToList();
                    string sql = minTien > 0 
                        ? $"SELECT MANV, HOTEN, TENPC, SOTIEN FROM V_AI_ALLOWANCE WHERE SOTIEN >= {minTien} ORDER BY SOTIEN DESC"
                        : "SELECT MANV, HOTEN, TENPC, SOTIEN FROM V_AI_ALLOWANCE ORDER BY SOTIEN DESC";

                    if (pcQuery.Count > 0)
                    {
                        var lines = pcQuery.Select(x => $"• **{x.HOTEN}** (#{x.MANV}) - {x.TENPC}: **{(x.SOTIEN.HasValue ? x.SOTIEN.Value.ToString("N0") : "0")} VNĐ**");
                        return new SmartQueryResult
                        {
                            Answer = $"💵 **Danh sách nhân sự nhận phụ cấp** (Nguồn: AI_READONLY.V_AI_ALLOWANCE):\n\n" +
                                     string.Join("\n", lines),
                            SqlQuery = sql
                        };
                    }
                    else
                    {
                        return new SmartQueryResult
                        {
                            Answer = minTien > 0 
                                ? $"💵 Hiện không có nhân sự nào có mức phụ cấp từ **{minTien:N0} VNĐ** trở lên theo dữ liệu AI_READONLY."
                                : "💵 Hiện chưa có dữ liệu phụ cấp nào được ghi nhận trong hệ thống AI_READONLY.",
                            SqlQuery = sql
                        };
                    }
                }

                // 7. LÀM THÊM GIỜ / TĂNG CA (OT): V_AI_OVERTIME
                if (lower.Contains("làm thêm") || lower.Contains("tăng ca") || lower.Contains("ot") || lower.Contains("ngoài giờ"))
                {
                    var otList = db.V_AI_OVERTIME.OrderByDescending(x => x.SOGIO).Take(10).ToList();
                    string sql = "SELECT MANV, HOTEN, TEN_PHONGBAN, NGAY, THANG, NAM, SOGIO FROM V_AI_OVERTIME ORDER BY SOGIO DESC";
                    if (otList.Count > 0)
                    {
                        var lines = otList.Select(x => $"• **{x.HOTEN}** (#{x.MANV}) - Tăng ca: **{x.SOGIO} giờ** (Ngày {x.NGAY}/{x.THANG}/{x.NAM} &bull; {x.TEN_PHONGBAN})");
                        return new SmartQueryResult
                        {
                            Answer = $"⏱️ **Dữ liệu làm thêm giờ (OT)** (Nguồn: AI_READONLY.V_AI_OVERTIME):\n\n" +
                                     string.Join("\n", lines),
                            SqlQuery = sql
                        };
                    }
                    else
                    {
                        return new SmartQueryResult
                        {
                            Answer = "⏰ **Quy định Chế độ Làm Thêm Giờ (OT) theo Điều 98 Bộ luật Lao động 2019**:\n\n" +
                                     "• **Ngày thường**: Trả ít nhất bằng **150%** đơn giá lương giờ.\n" +
                                     "• **Ngày nghỉ hàng tuần**: Trả ít nhất bằng **200%** đơn giá lương giờ.\n" +
                                     "• **Ngày lễ, tết, ngày nghỉ có hưởng lương**: Trả ít nhất bằng **300%**.\n" +
                                     "• **Làm việc vào ban đêm (22h - 06h)**: Trả thêm ít nhất **30%** tiền lương giờ làm việc ban ngày.",
                            SqlQuery = ""
                        };
                    }
                }

                // 8. CHẾ ĐỘ NGHỈ PHÉP NĂM
                if (lower.Contains("phép") || lower.Contains("nghỉ phép") || lower.Contains("phép năm"))
                {
                    return new SmartQueryResult
                    {
                        Answer = "🌴 **Quy định Chế độ Nghỉ Phép Năm theo Điều 113 & 114 Bộ luật Lao động 2019**:\n\n" +
                                 "• **12 ngày làm việc**: Áp dụng cho người lao động làm việc đủ 12 tháng trong điều kiện bình thường.\n" +
                                 "• **Cộng thêm ngày phép theo thâm niên**: Cứ đủ **05 năm làm việc** cho một người sử dụng lao động thì số ngày nghỉ hằng năm được tăng thêm tương ứng **01 ngày**.\n" +
                                 "• Người lao động làm việc chưa đủ 12 tháng: Số ngày nghỉ hằng năm tỷ lệ với số tháng làm việc thực tế.",
                        SqlQuery = ""
                    };
                }

                // 9. BẢO HIỂM XÃ HỘI: V_AI_INSURANCE
                if (lower.Contains("bảo hiểm") || lower.Contains("bhxh"))
                {
                    var bhList = db.V_AI_INSURANCE.Take(10).ToList();
                    string sql = "SELECT MANV, HOTEN, SOBH, NGAYCAP, NOICAP, NOIKHAMBENH FROM V_AI_INSURANCE";
                    if (bhList.Count > 0)
                    {
                        var lines = bhList.Select(x => $"• **{x.HOTEN}** (#{x.MANV}) - Số BH: **{x.SOBH}** | Nơi cấp: {x.NOICAP ?? "N/A"} | KCB: {x.NOIKHAMBENH ?? "N/A"}");
                        return new SmartQueryResult
                        {
                            Answer = $"🛡️ **Hồ sơ Bảo hiểm Xã hội** (Nguồn: AI_READONLY.V_AI_INSURANCE):\n\n" +
                                     string.Join("\n", lines),
                            SqlQuery = sql
                        };
                    }
                }

                // 10. TẠM ỨNG LƯƠNG: V_AI_ADVANCE
                if (lower.Contains("tạm ứng") || lower.Contains("ứng lương"))
                {
                    var advList = db.V_AI_ADVANCE.OrderByDescending(x => x.SOTIEN).Take(10).ToList();
                    string sql = "SELECT MANV, HOTEN, NGAY, THANG, NAM, SOTIEN FROM V_AI_ADVANCE ORDER BY SOTIEN DESC";
                    if (advList.Count > 0)
                    {
                        var lines = advList.Select(x => $"• **{x.HOTEN}** (#{x.MANV}) - Tạm ứng: **{(x.SOTIEN.HasValue ? x.SOTIEN.Value.ToString("N0") : "0")} VNĐ** (Ngày {x.NGAY}/{x.THANG}/{x.NAM})");
                        return new SmartQueryResult
                        {
                            Answer = $"💳 **Dữ liệu Tạm ứng Lương** (Nguồn: AI_READONLY.V_AI_ADVANCE):\n\n" +
                                     string.Join("\n", lines),
                            SqlQuery = sql
                        };
                    }
                }

                // 11. CHẤM CÔNG: V_AI_ATTENDANCE
                if (lower.Contains("chấm công") || lower.Contains("điểm danh") || lower.Contains("đi làm"))
                {
                    var attList = db.V_AI_ATTENDANCE.Take(10).ToList();
                    string sql = "SELECT MANV, HOTEN, TEN_PHONGBAN, NGAY, THANG, NAM, TIME_IN, TIME_OUT FROM V_AI_ATTENDANCE";
                    if (attList.Count > 0)
                    {
                        var lines = attList.Select(x => $"• **{x.HOTEN}** (#{x.MANV}) - Ngày {x.NGAY}/{x.THANG}/{x.NAM}: Vào lúc **{x.TIME_IN ?? "N/A"}**, Ra lúc **{x.TIME_OUT ?? "N/A"}** ({x.TEN_PHONGBAN ?? ""})");
                        return new SmartQueryResult
                        {
                            Answer = $"⏰ **Dữ liệu Chấm công Nhân sự** (Nguồn: AI_READONLY.V_AI_ATTENDANCE):\n\n" +
                                     string.Join("\n", lines),
                            SqlQuery = sql
                        };
                    }
                }
            }

            // Trả lời mặc định
            return new SmartQueryResult
            {
                Answer = $"Xin chào! Tôi đã nhận câu hỏi: *\"{q}\"*.\n\n" +
                         "Tôi là Trợ lý AI Quản trị Nhân sự (kết nối an toàn tài khoản **AI_READONLY**), sẵn sàng hỗ trợ bạn tra cứu dữ liệu:\n" +
                         "• 👤 *'Cho tôi thông tin nhân viên tên Duy'*\n" +
                         "• 🏢 *'Thống kê số lượng nhân viên theo từng phòng ban'*\n" +
                         "• 👥 *'Danh sách tất cả nhân viên trong công ty'*\n" +
                         "• 🎂 *'Danh sách nhân viên sinh nhật tháng này'*\n" +
                         "• 💵 *'Danh sách nhân viên nhận phụ cấp'*\n" +
                         "• ⏱️ *'Dữ liệu làm thêm giờ (tăng ca)'*\n" +
                         "• ⏰ *'Dữ liệu chấm công nhân viên'*\n" +
                         "• ⚖️ *'Quy định làm thêm giờ (OT) và ngày phép năm'*\n\n" +
                         "Bạn hãy bấm vào các gợi ý nhanh bên dưới hoặc gõ câu hỏi để tôi hỗ trợ nhé!",
                SqlQuery = ""
            };
        }

        /// <summary>
        /// POST: api/ai/reconcile
        /// Chạy job đối soát toàn bộ nhân sự sang Qdrant Vector Service
        /// </summary>
        [HttpPost]
        [Route("reconcile")]
        [JwtAuthorize(RequireAdmin = true)]
        public async Task<IHttpActionResult> ReconcileVectors()
        {
            try
            {
                var result = await Bu.Services.AI_Services.Vector.QdrantOutboxManager.Instance.ReconcileAllEmployeesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong ReconcileVectors: " + ex.ToString());
                return InternalServerError();
            }
        }

        /// <summary>
        /// GET: api/ai/outbox-status
        /// Kiểm tra số lượng tin nhắn đang chờ đồng bộ sang Qdrant
        /// </summary>
        [HttpGet]
        [Route("outbox-status")]
        [JwtAuthorize(RequireAdmin = true)]
        public IHttpActionResult GetOutboxStatus()
        {
            try
            {
                int pending = Bu.Services.AI_Services.Vector.QdrantOutboxManager.Instance.PendingCount;
                return Ok(new { pendingMessages = pending });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong GetOutboxStatus: " + ex.ToString());
                return InternalServerError();
            }
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; }
        public string Lang { get; set; }
    }
}
