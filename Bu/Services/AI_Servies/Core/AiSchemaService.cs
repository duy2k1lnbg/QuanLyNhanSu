using System.Collections.Generic;
using System.Text;

namespace Bu.Services.AI_Servies
{
    public class AiSchemaService
    {
        public string GetFullSchema()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== DATABASE SCHEMA ===");

            sb.AppendLine(@"
[EMPLOYEE]
Table: V_AI_NHANVIEN
Columns:
- MANV: Mã nhân viên
- HOTEN: Họ tên
- NGAYSINH: Ngày sinh
- DIENTHOAI: Điện thoại
- DIACHI: Địa chỉ
- IDPB: Phòng ban
- IDBP: Bộ phận
- IDCV: Chức vụ
");

            sb.AppendLine(@"
[EMPLOYEE_WITH_DEPT]
Table: V_AI_EMP_WITH_DEPT
Columns:
- MANV
- HOTEN
- TEN_PHONGBAN
- TEN_BOPHAN
");

            sb.AppendLine(@"
[ATTENDANCE]
Table: V_AI_CHAMCONG
Columns:
- MANV
- NGAY, THANG, NAM
- GIOVAO, PHUTVAO
- GIORA, PHUTRA
- IDLOAICONG
");

            sb.AppendLine(@"
[INSURANCE]
Table: V_AI_BAOHIEM
Columns:
- MANV
- SOBH: Số bảo hiểm
- NGAYCAP: Ngày cấp
- NOICAP: Nơi cấp
- NOIKHAMBENH: Nơi khám
");

            sb.AppendLine(@"
[DEPARTMENT]
Table: V_AI_PHONGBAN
Columns:
- IDPB
- TENPB
");

            sb.AppendLine(@"
[POSITION]
Table: V_AI_CHUCVU
Columns:
- IDCV
- TENCV
");

            sb.AppendLine(@"
[EDUCATION]
Table: V_AI_TRINHDO
Columns:
- IDTD
- TENTD
");

            sb.AppendLine(@"
[RELIGION]
Table: V_AI_TONGIAO
Columns:
- IDTG
- TENTG
");

            sb.AppendLine(@"
[ETHNIC]
Table: V_AI_DANTOC
Columns:
- IDDT
- TENDT
");

            sb.AppendLine(@"
[ALLOWANCE]
Table: V_AI_PHUCAP
Columns:
- IDPC
- TENPC
");

            sb.AppendLine(@"
[ADVANCE_SALARY]
Table: V_AI_UNGLUONG
Columns:
- MANV
- SOTIENUNG
- NGAY, THANG, NAM
");

            sb.AppendLine(@"
[OVERTIME]
Table: V_AI_TANGCA
Columns:
- MANV
- SOGIO
- NGAY, THANG, NAM
");

            return sb.ToString();
        }
    }
}