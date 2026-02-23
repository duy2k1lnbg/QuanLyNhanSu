using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services
{
    public class AiRepository
    {
        #region

        // Sử dụng DbContext của User AI_READONLY
        private readonly AIEntities _db = new AIEntities();

        public object GetEmployeeData(string name)
        {
            return _db.V_AI_EMP_WITH_DEPT
                      .Where(x => string.IsNullOrEmpty(name) || x.HOTEN.Contains(name))
                      .Take(5).ToList();
        }

        public object GetAttendanceData(decimal? maNV)
        {
            return _db.V_AI_CHAMCONG
                      .Where(x => !maNV.HasValue || x.MANV == maNV)
                      .OrderByDescending(x => x.NGAY).Take(10).ToList();
        }

        public object GetInsuranceData(string name)
        {
            return (from b in _db.V_AI_BAOHIEM
                    join e in _db.V_AI_NHANVIEN on b.MANV equals e.MANV
                    where e.HOTEN.Contains(name)
                    select b).FirstOrDefault();
        }

        public object GetPayrollData(decimal maNV)
        {
            return _db.V_AI_UNGLUONG.Where(x => x.MANV == maNV)
                      .OrderByDescending(x => x.NAM).ThenByDescending(x => x.THANG)
                      .Take(3).ToList();
        }

        public object GetOtherData(string type)
        {
            if (type == "DANTOC") return _db.V_AI_DANTOC.ToList();
            if (type == "TRINHDO") return _db.V_AI_TRINHDO.ToList();
            return null;
        }
        #endregion
    }
}
