using Bu.Services.AI_Services;
using DA;
using HRMS_API.Filters;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/ai")]
    public class AiChatController : ApiController
    {
        /// <summary>
        /// POST: api/ai/chat
        /// Gửi câu hỏi đến Trợ lý AI HRMS Copilot (kết hợp Qwen 2.5 RAG và Oracle Database)
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

            // 1. Kiểm tra nhanh nếu Ollama đang chạy trên port 11434 (timeout 500ms)
            if (IsOllamaOnline())
            {
                try
                {
                    var manager = new ChatboxManager();
                    var result = await manager.ProcessQuery(question);

                    if (result != null && !string.IsNullOrWhiteSpace(result.Answer))
                    {
                        return Ok(new
                        {
                            answer = result.Answer,
                            sqlQuery = result.SqlQuery ?? "",
                            source = "RAG_Ollama"
                        });
                    }
                }
                catch (Exception)
                {
                    // Chuyển sang Fallback nếu có lỗi xử lý
                }
            }

            // 2. Smart Fallback: Phân tích câu hỏi và truy vấn trực tiếp số liệu từ Oracle Database
            try
            {
                string answer = GenerateSmartResponse(question);
                return Ok(new
                {
                    answer = answer,
                    sqlQuery = "",
                    source = "Oracle_Live_Assistant"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong AiChatController: " + ex.ToString());
                return Ok(new
                {
                    answer = "Xin chào, tôi là AI Copilot HRMS. Rất tiếc hệ thống tạm thời gặp gián đoạn kết nối. Vui lòng thử lại sau.",
                    sqlQuery = "",
                    source = "Fallback_Error"
                });
            }
        }

        private static bool IsOllamaOnline()
        {
            try
            {
                using (var tcp = new TcpClient())
                {
                    var ar = tcp.BeginConnect("127.0.0.1", 11434, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
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

        private string GenerateSmartResponse(string q)
        {
            string lower = q.ToLower();

            using (var db = new MyEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                // Tra cứu số lượng nhân sự
                if (lower.Contains("bao nhiêu nhân viên") || lower.Contains("tổng số nhân viên") || lower.Contains("số lượng nhân sự") || lower.Contains("quy mô nhân sự"))
                {
                    int totalNv = db.TB_NHANVIEN.Count();
                    int dangLamViec = db.TB_NHANVIEN.Count(x => x.DATHOIVIEC != 1);
                    return $"Hiện tại toàn hệ thống có tổng cộng **{totalNv:N0} nhân sự**, trong đó có **{dangLamViec:N0} nhân viên** đang làm việc chính thức.";
                }

                // Tra cứu phòng ban
                if (lower.Contains("phòng ban") || lower.Contains("bao nhiêu phòng"))
                {
                    int totalPb = db.TB_PHONGBAN.Count();
                    var listPb = db.TB_PHONGBAN.Take(5).Select(x => x.TENPB).ToList();
                    string pbStr = string.Join(", ", listPb);
                    return $"Công ty hiện có **{totalPb} phòng ban/bộ phận** chức năng (gồm có: {pbStr}...). Bạn có thể vào tab Quản lý Nhân sự để xem chi tiết danh sách.";
                }

                // Tra cứu làm thêm giờ / tăng ca (Luật lao động 2019)
                if (lower.Contains("làm thêm") || lower.Contains("tăng ca") || lower.Contains("ot") || lower.Contains("ngoài giờ"))
                {
                    return "Theo quy định tại **Điều 98 Bộ luật Lao động 2019** và Quy chế công ty:\n\n" +
                           "• **Ngày thường**: Trả ít nhất bằng **150%** lương giờ thực trả.\n" +
                           "• **Ngày nghỉ hàng tuần**: Trả ít nhất bằng **200%** lương giờ thực trả.\n" +
                           "• **Ngày lễ, tết, ngày nghỉ có hưởng lương**: Trả ít nhất bằng **300%** chưa kể tiền lương ngày lễ.\n" +
                           "• **Làm việc vào ban đêm**: Được trả thêm ít nhất bằng **30%** tiền lương tính theo đơn giá tiền lương công việc ngày bình thường.";
                }

                // Tra cứu quỹ lương
                if (lower.Contains("lương") || lower.Contains("quỹ lương") || lower.Contains("chi phí lương"))
                {
                    var latestKc = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                    if (latestKc != null)
                    {
                        var makycong = latestKc.MAKYCONG;
                        decimal tongLuong = db.TB_BANGLUONG.Where(x => x.MAKYCONG == makycong).Sum(x => (decimal?)x.THUC_LINH) ?? 0;
                        return $"Tổng quỹ lương thực lĩnh kỳ công **{latestKc.THANG}/{latestKc.NAM}** là **{tongLuong:N0} VNĐ**. Bạn có thể vào tab 'Tính lương & Thuế' để tra cứu chi tiết từng nhân viên.";
                    }
                }

                // Tra cứu hợp đồng lao động
                if (lower.Contains("hợp đồng") || lower.Contains("hđlđ") || lower.Contains("thời hạn"))
                {
                    int totalHd = db.TB_HOPDONG.Count();
                    return $"Hệ thống hiện đang quản lý **{totalHd:N0} hợp đồng lao động**. Doanh nghiệp ký kết theo 2 loại hợp đồng chính: Hợp đồng xác định thời hạn (tối đa 36 tháng) và Hợp đồng không xác định thời hạn theo Điều 20 Bộ luật Lao động 2019.";
                }

                // Tra cứu nghỉ phép
                if (lower.Contains("phép") || lower.Contains("nghỉ phép") || lower.Contains("nghỉ phép năm"))
                {
                    return "Theo **Điều 113 Bộ luật Lao động 2019**:\n\n" +
                           "• Người lao động làm việc đủ 12 tháng được nghỉ hằng năm hưởng nguyên lương: **12 ngày làm việc** đối với công việc bình thường.\n" +
                           "• Cứ đủ **05 năm làm việc** thì số ngày nghỉ hằng năm được tăng thêm tương ứng **01 ngày**.\n" +
                           "• Lao động chưa đủ 12 tháng làm việc thì số ngày nghỉ tỷ lệ với số tháng làm việc.";
                }
            }

            // Trả lời mặc định
            return $"Chào bạn, câu hỏi của bạn là: *\"{q}\"*. Hệ thống HRMS đã ghi nhận câu hỏi. Bạn có thể tra cứu nhanh các thông tin về: **tổng số nhân sự**, **phòng ban**, **quy định làm thêm giờ (OT)**, **quỹ lương**, **hợp đồng lao động** hoặc **chế độ nghỉ phép năm**.";
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
