namespace Bu.Services.AI_Services
{
    public class AiSchemaService
    {
        public string GetFullSchema()
        {
            return @"
[AI DATABASE - READ ONLY VIEWS]

Chỉ sử dụng các VIEW dưới đây để viết câu lệnh SQL. Không dùng TABLE.

==================================================
1. V_AI_EMPLOYEE (Thông tin nhân viên)
--------------------------------------------------
MANV (NUMBER)          - Mã nhân viên
HOTEN (TEXT)           - Họ tên
NGAYSINH (DATE)        - Ngày sinh
DIENTHOAI (TEXT)       - Điện thoại
DIACHI (TEXT)          - Địa chỉ
TEN_PHONGBAN (TEXT)    - Phòng ban
TEN_BOPHAN (TEXT)      - Bộ phận
TEN_CHUCVU (TEXT)      - Chức vụ

==================================================
2. V_AI_ATTENDANCE (Chấm công)
--------------------------------------------------
MANV (NUMBER)
HOTEN (TEXT)
TEN_PHONGBAN (TEXT)
NGAY (NUMBER)
THANG (NUMBER)
NAM (NUMBER)
GIOVAO (NUMBER)
PHUTVAO (NUMBER)
GIORA (NUMBER)
PHUTRA (NUMBER)
TIME_IN (TEXT)     - giờ vào (HH:MM)
TIME_OUT (TEXT)    - giờ ra (HH:MM)

==================================================
3. V_AI_OVERTIME (Tăng ca)
--------------------------------------------------
MANV (NUMBER)
HOTEN (TEXT)
TEN_PHONGBAN (TEXT)
NGAY (NUMBER)
THANG (NUMBER)
NAM (NUMBER)
SOGIO (NUMBER)

==================================================
4. V_AI_INSURANCE (Bảo hiểm)
--------------------------------------------------
MANV (NUMBER)
HOTEN (TEXT)
SOBH (TEXT)
NGAYCAP (DATE)
NOICAP (TEXT)
NOIKHAMBENH (TEXT)

==================================================
5. V_AI_ADVANCE (Ứng lương)
--------------------------------------------------
MANV (NUMBER)
HOTEN (TEXT)
NGAY (NUMBER)
THANG (NUMBER)
NAM (NUMBER)
SOTIEN (NUMBER)

==================================================
6. V_AI_ALLOWANCE (Phụ cấp)
--------------------------------------------------
MANV (NUMBER)
HOTEN (TEXT)
TENPC (TEXT)       - Tên phụ cấp
SOTIEN (NUMBER)
KYCONG (NUMBER)

==================================================
[SQL RULE - BẮT BUỘC]

1. CHỈ trả về câu lệnh SELECT.
2. KHÔNG dùng INSERT, UPDATE, DELETE, DROP.
3. KHÔNG dùng dấu ``` hoặc giải thích.
4. MANV là NUMBER → không dùng dấu nháy đơn.
   Ví dụ đúng: WHERE MANV = 181

5. Tìm tên:
   Dùng: UPPER(HOTEN) LIKE UPPER('%DUY%')

6. Khi có THANG/NAM:
   Ví dụ:
   WHERE THANG = 8 AND NAM = 2024

7. Nếu không thể viết SQL → trả về:
   NOT_SQL

==================================================
";
        }
    }
}