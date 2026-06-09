using NUnit.Framework;
using System;
using System.Linq;
using DA;

namespace Bu.Tests
{
    [TestFixture]
    public class DbQueryTests
    {
        [Test]
        public void TestEFQuery()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    int totalEmp = db.TB_NHANVIEN.Count(x => (x.DATHOIVIEC == null || x.DATHOIVIEC == 0) && x.DELETED_DATE == null);
                    Console.WriteLine($"Total active employees: {totalEmp}");

                    DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    DateTime nextMonth = startOfMonth.AddMonths(1);

                    int newEmp = db.TB_NHANVIEN.Count(x => x.CREATED_DATE >= startOfMonth && x.CREATED_DATE < nextMonth && x.DELETED_DATE == null);
                    Console.WriteLine($"New employees this month: {newEmp}");

                    int resignedEmp = db.TB_NHANVIEN_THOIVIEC.Count(x => x.NGAYNGHIVIEC >= startOfMonth && x.NGAYNGHIVIEC < nextMonth && x.DELETED_DATE == null);
                    Console.WriteLine($"Resigned employees this month: {resignedEmp}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("QUERY FAILED WITH EXCEPTION:");
                Console.WriteLine(ex.ToString());
                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER EXCEPTION:");
                    Console.WriteLine(ex.InnerException.ToString());
                }
                Assert.Fail(ex.Message);
            }
        }
    }
}
