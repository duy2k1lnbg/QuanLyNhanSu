using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bu.Services.AI_Services.Core
{
    /// <summary>
    /// Xử lý các câu hỏi cố định, xã giao (Rule-based) để trả lời ngay lập tức, 
    /// giảm tải việc gọi LLM và tăng tốc độ phản hồi.
    /// </summary>
    public static class FastResponseService
    {
        public static string GetFastResponse(string question)
        {
            if (string.IsNullOrWhiteSpace(question)) return null;

            string lowerQ = question.ToLower().Trim();
            lowerQ = Regex.Replace(lowerQ, @"\s+", " "); // Normalize whitespace

            // 1. Chào hỏi cơ bản
            string[] greetings = { "xin chào", "chào bạn", "hi", "hello", "chào", "chào buổi sáng", "chào buổi chiều", "chào ad", "alo", "ê", "chào buổi tối" };
            if (greetings.Any(g => lowerQ == g || lowerQ.StartsWith(g + " ")))
            {
                return "Xin chào! Tôi là Trợ lý Nhân sự AI (HRM Assistant). Tôi có thể giúp bạn tra cứu thông tin nhân viên, tính lương, bảo hiểm, ngày phép và thống kê dữ liệu. Bạn cần tôi giúp gì?";
            }

            // 2. Hỏi về danh tính / người tạo
            string[] identityQs = { "bạn là ai", "mày là ai", "who are you", "tên bạn là gì", "cậu là ai", "anh là ai", "chị là ai", "bạn tên gì" };
            if (identityQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tôi là Hệ thống Trợ lý Nhân sự thông minh (HRM AI RAG) được tích hợp trực tiếp vào phần mềm Quản lý Nhân sự. Chức năng của tôi là giúp bạn truy xuất dữ liệu nhân sự một cách nhanh chóng bằng ngôn ngữ tự nhiên.";
            }

            string[] makerQs = { "do ai tạo ra", "ai phát triển", "ai viết ra", "tác giả của bạn", "người tạo ra bạn", "ba bạn là ai", "mẹ bạn là ai", "ai làm ra bạn" };
            if (makerQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tôi được nghiên cứu và phát triển bởi Admin để hỗ trợ nghiệp vụ quản lý nhân sự tại công ty một cách tự động và thông minh hơn.";
            }

            // 3. Hỏi về khả năng / hướng dẫn sử dụng
            string[] capabilityQs = { "bạn làm được gì", "bạn có thể làm gì", "chức năng của bạn", "hướng dẫn sử dụng", "giúp tôi với", "help", "có thể giúp gì" };
            if (capabilityQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tôi có thể thực hiện các nghiệp vụ sau:\n- Thống kê nhân sự (ví dụ: 'Có bao nhiêu nhân viên ở phòng Kế toán?')\n- Tra cứu thông tin cá nhân ('Tìm số điện thoại của Trần Thanh Tâm')\n- Lọc dữ liệu theo điều kiện ('Ai sinh nhật trong tháng này?', 'Ai có hệ số lương lớn hơn 3?')\n- Cung cấp thông tin về bảo hiểm, phụ cấp, tăng ca, ứng lương.\n\nBạn cứ hỏi tôi bằng tiếng Việt tự nhiên nhé!";
            }

            // 4. Hỏi thăm sức khỏe / cảm xúc / chitchat
            string[] healthQs = { "bạn khỏe không", "khỏe không", "how are you", "dạo này sao rồi", "ổn không" };
            if (healthQs.Any(q => lowerQ.Contains(q)))
            {
                return "Cảm ơn bạn đã hỏi thăm! Tôi là AI nên lúc nào cũng tràn đầy năng lượng 100% để hỗ trợ bạn. Bạn cần tra cứu thông tin gì nào?";
            }
            
            string[] eatQs = { "ăn cơm chưa", "ăn sáng chưa", "đói không", "ăn gì" };
            if (eatQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tôi là AI nên thức ăn của tôi là Dữ liệu (Data) và Điện! Tôi luôn sẵn sàng 24/7 để phục vụ bạn.";
            }

            string[] jokeQs = { "kể chuyện", "chuyện cười", "hát", "nói đùa" };
            if (jokeQs.Any(q => lowerQ.Contains(q)))
            {
                return "Xin lỗi, tôi là AI chuyên ngành Nhân sự nên hơi nghiêm túc một chút. Tôi chỉ rành về hợp đồng, lương bổng và bảo hiểm thôi. Bạn có câu hỏi nào về các lĩnh vực đó không?";
            }

            // 5. Khen ngợi
            string[] praiseQs = { "giỏi quá", "thông minh quá", "tuyệt vời", "tốt", "good", "xuất sắc", "đỉnh", "vip", "ngon", "rất tốt", "giỏi" };
            if (praiseQs.Any(q => lowerQ == q || lowerQ.StartsWith(q + " ")))
            {
                return "Cảm ơn bạn rất nhiều! Đó là động lực để tôi học hỏi và phục vụ bạn tốt hơn. Nếu có gì cần cải thiện, bạn cứ góp ý nhé!";
            }

            // 6. Cảm ơn
            string[] thanksQs = { "cảm ơn", "thanks", "thank you", "cảm ơn bạn", "tuyệt", "ok", "dạ", "vâng", "được rồi", "hiểu rồi" };
            if (thanksQs.Any(q => lowerQ == q || lowerQ.StartsWith(q + " ")))
            {
                return "Không có gì! Rất vui được hỗ trợ bạn. Chúc bạn một ngày làm việc hiệu quả!";
            }

            // 7. Chặn từ ngữ thiếu văn minh / tiêu cực
            string[] toxicQs = { "ngu ngốc", "dở tệ", "đồ ngốc", "ngu quá", "chậm quá", "tệ quá", "óc chó" };
            string[] toxicWords = { "ngu", "chó", "đần" }; // Các từ đơn lẻ phải đứng tách biệt để tránh dính vào "Nguyễn", "Người", "Chóng"
            
            bool hasToxicPhrase = toxicQs.Any(q => lowerQ.Contains(q));
            bool hasToxicWord = toxicWords.Any(w => lowerQ == w || lowerQ.StartsWith(w + " ") || lowerQ.EndsWith(" " + w) || lowerQ.Contains(" " + w + " "));

            if (hasToxicPhrase || hasToxicWord)
            {
                return "Xin lỗi nếu tôi chưa đáp ứng được yêu cầu của bạn. Hệ thống AI vẫn đang trong quá trình học hỏi thêm từ dữ liệu thực tế. Bạn có thể diễn đạt câu hỏi rõ ràng và lịch sự hơn được không?";
            }
            
            // 8. Tạm biệt
            string[] byeQs = { "tạm biệt", "bye", "hẹn gặp lại", "chào nhé", "đi ngủ", "good night", "mai gặp" };
            if (byeQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tạm biệt bạn! Nếu cần hỗ trợ thêm thông tin nhân sự, hãy quay lại bất cứ lúc nào nhé.";
            }

            // ================= CÁC CHỦ ĐỀ MỚI (HR FAQ & IT Support & Chit-chat mở rộng) =================

            // 9. FAQ: Lịch trả lương
            string[] salaryDateQs = { "bao giờ có lương", "khi nào có lương", "ngày mấy có lương", "lịch phát lương", "ngày trả lương", "lương tháng này" };
            if (salaryDateQs.Any(q => lowerQ.Contains(q)))
            {
                return "Thông thường công ty sẽ chốt công vào cuối tháng và chi trả lương vào khoảng mùng 5 đến mùng 10 tháng sau. Bạn cứ yên tâm làm việc, lương sẽ về đúng hạn nhé!";
            }

            // 10. FAQ: Lịch làm việc
            string[] workHourQs = { "mấy giờ làm", "mấy giờ tan", "giờ làm việc", "lịch làm việc", "thời gian làm việc" };
            if (workHourQs.Any(q => lowerQ.Contains(q)))
            {
                return "Giờ hành chính cơ bản thường là từ 8h00 sáng đến 17h00 chiều. Tuy nhiên giờ giấc cụ thể có thể linh động tùy thuộc vào ca làm việc và bộ phận của bạn.";
            }

            // 11. FAQ: Nghỉ phép
            string[] leaveQs = { "xin nghỉ phép", "quy trình nghỉ", "cách xin nghỉ", "đơn xin nghỉ", "làm sao để nghỉ" };
            if (leaveQs.Any(q => lowerQ.Contains(q)))
            {
                return "Để xin nghỉ phép, bạn cần thông báo và được sự đồng ý của Quản lý trực tiếp. Sau đó làm thủ tục/đơn xin nghỉ gửi về phòng Nhân sự (HR) để ghi nhận trên hệ thống chấm công.";
            }

            // 12. IT Support / Quên mật khẩu
            string[] itQs = { "quên mật khẩu", "đổi mật khẩu", "lỗi hệ thống", "không đăng nhập được", "không vào được", "mất tài khoản" };
            if (itQs.Any(q => lowerQ.Contains(q)))
            {
                return "Nếu gặp vấn đề về tài khoản, quên mật khẩu hoặc lỗi phần mềm, bạn vui lòng liên hệ bộ phận IT hoặc Quản trị viên (Admin) của công ty để được reset và hỗ trợ nhanh nhất nhé.";
            }

            // 13. Chit-chat: Tình cảm
            string[] loveQs = { "có người yêu chưa", "bạn trai", "bạn gái", "yêu tôi không", "thích tôi không", "cưới không" };
            if (loveQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tôi là AI nên chỉ biết yêu công việc và yêu người dùng thôi! Trái tim tôi làm bằng Code và Database nên không biết yêu đương là gì đâu.";
            }

            // 14. Chit-chat: Tuổi tác
            string[] ageQs = { "bao nhiêu tuổi", "sinh năm bao nhiêu", "tuổi con gì", "mấy tuổi" };
            if (ageQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tôi được tạo ra gần đây nên tuổi đời còn rất trẻ. Nhưng tuổi 'nghề' và lượng dữ liệu nhân sự tôi nắm giữ thì đủ để giúp bạn mọi nghiệp vụ đấy!";
            }

            // 15. Chit-chat: Thời tiết ngoài lề
            string[] weatherQs = { "thời tiết", "trời mưa", "trời nắng", "nhiệt độ" };
            if (weatherQs.Any(q => lowerQ.Contains(q)))
            {
                return "Tiếc quá, tôi chưa được kết nối với đài khí tượng thủy văn. Tôi chỉ dự báo được tình hình nhân sự của công ty thôi! Bạn hãy xem thời tiết trên điện thoại nhé.";
            }

            // Nếu không khớp luật nào, trả về null để nhường quyền cho AI (Ollama) xử lý
            return null;
        }
    }
}
