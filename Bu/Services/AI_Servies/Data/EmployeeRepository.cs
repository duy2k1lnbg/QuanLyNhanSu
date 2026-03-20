using DA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Servies
{
    public class EmployeeRepository
    {
        public List<string> Search(string keyword, string dept = null)
        {
            using (var db = new AIEntities())
            {
                var query = db.V_AI_EMP_WITH_DEPT.AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(x => x.HOTEN.ToLower().Contains(keyword));

                if (!string.IsNullOrWhiteSpace(dept))
                    query = query.Where(x => x.TEN_PHONGBAN.ToLower().Contains(dept.ToLower()));

                return query
                    .Select(x => "Mã NV: " + x.MANV +
                                 " | Tên: " + x.HOTEN +
                                 " | Phòng: " + x.TEN_PHONGBAN +
                                 " | Bộ phận: " + x.TEN_BOPHAN)
                    .Take(10)
                    .ToList();
            }
        }

        public int Count()
        {
            using (var db = new AIEntities())
            {
                return db.V_AI_EMP_WITH_DEPT.Count();
            }
        }

        public List<string> GetTop(int n)
        {
            using (var db = new AIEntities())
            {
                return db.V_AI_EMP_WITH_DEPT
                    .Take(n)
                    .ToList()
                    .Select(x => "Mã NV: " + x.MANV +
                                 " | Tên: " + x.HOTEN +
                                 " | Phòng: " + x.TEN_PHONGBAN)
                    .ToList();
            }
        }
    }
}