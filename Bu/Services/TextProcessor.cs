//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bu.Services
//{
//    public class TextProcessor
//    {
//        private readonly string[] Keywords = { "nhân viên", "lương", "công", "giờ", "bảo hiểm", "ai", "tăng ca" };

//        public QueryIntent Analyze(string input)
//        {
//            input = input.ToLower();
//            var intent = new QueryIntent { IsDatabaseQuery = Keywords.Any(k => input.Contains(k)) };

//            if (!intent.IsDatabaseQuery) { intent.Intent = "CHAT"; return intent; }

//            if (input.Contains("lương") || input.Contains("ứng")) intent.Intent = "LUONG";
//            else if (input.Contains("công") || input.Contains("giờ")) intent.Intent = "CHAMCONG";
//            else intent.Intent = "NHANVIEN";

//            // Lấy từ cuối cùng làm từ khóa tìm kiếm (Tên hoặc Mã)
//            intent.SearchTerm = input.Split(' ').Last();
//            return intent;
//        }
//    }
//}
