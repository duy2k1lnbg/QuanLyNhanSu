using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Services.AI_Services.Core
{
    public class JsonPromptManager : IPromptManager
    {
        private readonly Dictionary<string, string> _prompts;

        public JsonPromptManager()
        {
            _prompts = new Dictionary<string, string>();
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? "";
                string[] candidatePaths = new[]
                {
                    Path.Combine(baseDir, "ai_prompts.json"),
                    Path.Combine(baseDir, "bin", "ai_prompts.json"),
                    Path.Combine(baseDir, "..", "QLyNSu", "ai_prompts.json"),
                    Path.Combine(baseDir, "..", "HRMS_API", "ai_prompts.json"),
                    Path.Combine(baseDir, "..", "..", "QLyNSu", "ai_prompts.json"),
                    @"d:\QL_NS\QuanLyNhanSu\QLyNSu\ai_prompts.json",
                    @"d:\QL_NS\QuanLyNhanSu\HRMS_API\ai_prompts.json"
                };

                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        _prompts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) 
                                   ?? new Dictionary<string, string>();
                        if (_prompts.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PromptManager] Loaded {_prompts.Count} prompts from: {path}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PromptManager ERROR]: {ex.Message}");
            }

            // Fallback nếu không có file: Đảm bảo 100% luôn có Schema AI_READONLY
            EnsureDefaultAiReadOnlyPrompts();
        }

        private void EnsureDefaultAiReadOnlyPrompts()
        {
            if (!_prompts.ContainsKey("Schema") || string.IsNullOrWhiteSpace(_prompts["Schema"]))
            {
                _prompts["Schema"] = @"
[AI DATABASE - TÀI KHOẢN AI_READONLY - CHỈ ĐƯỢC PHÉP TRUY VẤN CÁC VIEW DƯỚI ĐÂY]
NGUYÊN TẮC BẢO MẬT BẮT BUỘC:
Tài khoản kết nối của bạn là AI_READONLY. Chỉ có quyền truy vấn các VIEW sau đây.
TUYỆT ĐỐI KHÔNG ĐƯỢC DÙNG TABLE HỆ THỐNG NHƯ TB_NHANVIEN, TB_BANGCONG, TB_PHONGBAN... NẾU DÙNG SẼ BỊ LỖI BẢO MẬT VÀ TỪ CHỐI.

==================================================
1. V_AI_EMPLOYEE (Thông tin nhân viên)
--------------------------------------------------
MANV (NUMBER)          - Mã nhân viên
HOTEN (TEXT)           - Họ tên nhân viên
NGAYSINH (DATE)        - Ngày sinh
DIENTHOAI (TEXT)       - Điện thoại
DIACHI (TEXT)          - Địa chỉ
TEN_PHONGBAN (TEXT)    - Phòng ban
TEN_BOPHAN (TEXT)      - Bộ phận
TEN_CHUCVU (TEXT)      - Chức vụ

==================================================
2. V_AI_ATTENDANCE (Chấm công hàng ngày)
--------------------------------------------------
MANV (NUMBER)
HOTEN (TEXT)
TEN_PHONGBAN (TEXT)
NGAY (NUMBER)
THANG (NUMBER)
NAM (NUMBER)
GIOVAO (NUMBER), PHUTVAO (NUMBER)
GIORA (NUMBER), PHUTRA (NUMBER)
TIME_IN (TEXT), TIME_OUT (TEXT)

==================================================
3. V_AI_OVERTIME (Tăng ca)
--------------------------------------------------
MANV (NUMBER), HOTEN (TEXT), TEN_PHONGBAN (TEXT)
NGAY (NUMBER), THANG (NUMBER), NAM (NUMBER)
SOGIO (NUMBER)

==================================================
4. V_AI_INSURANCE (Bảo hiểm xã hội)
--------------------------------------------------
MANV (NUMBER), HOTEN (TEXT)
SOBH (TEXT), NGAYCAP (DATE), NOICAP (TEXT), NOIKHAMBENH (TEXT)

==================================================
5. V_AI_ADVANCE (Ứng lương)
--------------------------------------------------
MANV (NUMBER), HOTEN (TEXT)
NGAY (NUMBER), THANG (NUMBER), NAM (NUMBER), SOTIEN (NUMBER)

==================================================
6. V_AI_ALLOWANCE (Phụ cấp)
--------------------------------------------------
MANV (NUMBER), HOTEN (TEXT)
TENPC (TEXT), SOTIEN (NUMBER), KYCONG (NUMBER)

==================================================
[SQL RULE - BẮT BUỘC CHO TÀI KHOẢN AI_READONLY]
1. CHỈ trả về câu lệnh SELECT.
2. CHỈ TRUY VẤN TRÊN 6 VIEW: V_AI_EMPLOYEE, V_AI_ATTENDANCE, V_AI_OVERTIME, V_AI_INSURANCE, V_AI_ADVANCE, V_AI_ALLOWANCE.
3. KHÔNG BAO GIỜ dùng bảng bắt đầu bằng TB_ (như TB_NHANVIEN).
4. KHÔNG dùng INSERT, UPDATE, DELETE, DROP, ALTER.
5. KHÔNG dùng dấu ``` markdown hay giải thích thêm.
6. Tra cứu tên: dùng UPPER(HOTEN) LIKE UPPER('%TÊN%') từ V_AI_EMPLOYEE.
7. Nếu không viết được câu lệnh SQL hợp lệ, trả về: NOT_SQL
==================================================";
            }

            if (!_prompts.ContainsKey("SqlPromptTemplate") || string.IsNullOrWhiteSpace(_prompts["SqlPromptTemplate"]))
            {
                _prompts["SqlPromptTemplate"] = "\n{Schema}\n\n[NHIỆM VỤ]\nChuyển câu hỏi người dùng thành 1 câu lệnh SQL Oracle duy nhất truy vấn trên các VIEW của AI_READONLY.\nCâu hỏi: \"{Question}\"\n\n[VÍ DỤ MẪU]\n- User: Tìm thông tin nhân viên tên Duy\n- SQL: SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE UPPER('%DUY%')\n- User: Ai ở phòng kế toán?\n- SQL: SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(TEN_PHONGBAN) LIKE UPPER('%KẾ TOÁN%')\n\nLệnh SQL:";
            }

            if (!_prompts.ContainsKey("ChatPromptTemplate") || string.IsNullOrWhiteSpace(_prompts["ChatPromptTemplate"]))
            {
                _prompts["ChatPromptTemplate"] = "\n[DỮ LIỆU HỆ THỐNG - TỪ TÀI KHOẢN AI_READONLY]\n{Context}\n\n[LỊCH SỬ HỘI THOẠI]\n{History}\n\n[CÂU HỎI HIỆN TẠI]\n{Question}\n\n[YÊU CẦU TRẢ LỜI]\n- Bạn là Trợ lý AI Quản trị Nhân sự kết nối an toàn với tài khoản AI_READONLY.\n- Dựa vào [DỮ LIỆU HỆ THỐNG] ở trên để trả lời đầy đủ, chính xác, súc tích và thân thiện cho người dùng.\n- Trình bày thông tin nhân sự rõ ràng với các gạch đầu dòng (Mã NV, Họ tên, Phòng ban, Chức vụ, Bộ phận, Điện thoại...).\n- Không bịa đặt dữ liệu nếu không có trong hệ thống.\n";
            }
        }

        public string GetPrompt(string key)
        {
            if (_prompts.TryGetValue(key, out string val))
            {
                return val;
            }
            return string.Empty;
        }

        public string GetSchema()
        {
            return GetPrompt("Schema");
        }
    }
}
