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

                    int activeUsers = db.TB_SYS_USER.Count(x => (x.DISABLED ?? 0) == 0);
                    Console.WriteLine($"Active sys users: {activeUsers}");

                    // Inspect HopDong
                    int totalHd = db.TB_HOPDONG.Count(x => x.DEL_DATE == null);
                    DateTime today = DateTime.Today;
                    DateTime next30 = today.AddDays(30);
                    var expHd = db.TB_HOPDONG.Where(x => x.NGAYKETTHUC.HasValue && x.NGAYKETTHUC.Value >= today && x.NGAYKETTHUC.Value <= next30 && x.DEL_DATE == null).ToList();
                    Console.WriteLine($"Total active contracts: {totalHd}, Expiring next 30 days: {expHd.Count}");

                    var hdBus = new HOPDONGLAODONG();
                    var hdList = hdBus.getlistFull_DTO();
                    Console.WriteLine($"hdBus.getlistFull_DTO count: {hdList.Count}");

                    // Inspect missing info
                    int missingCccdOrPhone = db.TB_NHANVIEN.Count(x => (x.DATHOIVIEC == null || x.DATHOIVIEC == 0) && x.DELETED_DATE == null && (string.IsNullOrEmpty(x.CCCD) || string.IsNullOrEmpty(x.DIENTHOAI)));
                    Console.WriteLine($"Employees missing CCCD or Phone: {missingCccdOrPhone}");

                    // Inspect KyCong
                    var kyCongs = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).Take(5).ToList();
                    Console.WriteLine($"Total KyCong count: {db.TB_KYCONG.Count()}");
                    foreach (var kc in kyCongs)
                    {
                        Console.WriteLine($"KyCong {kc.MAKYCONG} - Month: {kc.THANG}/{kc.NAM}, Khoa: {kc.KHOA}, TrangThai: {kc.TRANGTHAI}");
                    }

                    // Inspect BangLuong
                    Console.WriteLine($"Total BangLuong records: {db.TB_BANGLUONG.Count()}");

                    // Inspect KyCongChiTiet
                    Console.WriteLine($"Total KyCongChiTiet records: {db.TB_KYCONGCHITIET.Count()}");
                    var kcGroups = db.TB_KYCONGCHITIET.GroupBy(x => x.MAKYCONG).Select(g => new { Makycong = g.Key, Count = g.Count() }).ToList();
                    foreach (var g in kcGroups)
                    {
                        Console.WriteLine($"KyCongChiTiet for MAKYCONG {g.Makycong}: {g.Count} records");
                    }

                    // Inspect BangCong
                    Console.WriteLine($"Total BangCong records: {db.TB_BANGCONG.Count()}");

                    // Inspect TangCa
                    Console.WriteLine($"Total TangCa records: {db.TB_TANGCA.Count()}");

                    // Inspect UngLuong
                    Console.WriteLine($"Total UngLuong records: {db.TB_UNGLUONG.Count()}");

                    // Inspect KhenThuong / KyLuat
                    Console.WriteLine($"Total KhenThuong records: {db.TB_KHENTHUONG_KYLUAT.Count()}");

                    // Inspect NangLuong
                    Console.WriteLine($"Total NangLuong records: {db.TB_NANGLUONG_NHANVIEN.Count()}");

                    // Inspect DieuChuyen
                    Console.WriteLine($"Total DieuChuyen records: {db.TB_DIEUCHUYEN_NHANVIEN.Count()}");

                    // TEST CHITIET QUERY DIRECTLY
                    int testMkc = 202602;
                    var raw = db.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == testMkc).Take(5).ToList();
                    Console.WriteLine($"Direct TB_KYCONGCHITIET query count for 202602: {raw.Count}");
                    foreach (var r in raw)
                    {
                        Console.WriteLine($"NV #{r.MANV} ({r.HOTEN}) - D1: '{r.D1}', D2: '{r.D2}', D3: '{r.D3}', D4: '{r.D4}', D5: '{r.D5}', TONGNGAYCONG: {r.TONGNGAYCONG}");
                    }
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

        [Test]
        public void TestDisabledUserLoginThrowsAccountLocked()
        {
            using (var db = new MyEntities())
            {
                // Find a non-admin user to test with
                var testUser = db.TB_SYS_USER.FirstOrDefault(u => !u.USERNAME.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) && (u.ISGROUP ?? 0) == 0);
                if (testUser == null)
                {
                    Assert.Ignore("No non-admin user found to test disabled login.");
                    return;
                }

                var sysUser = new Bu.CLASS_SYSTEM.SYS_USER();
                decimal originalDisabled = testUser.DISABLED ?? 0;

                try
                {
                    // 0. Ensure no temporary lockout is active
                    db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_USER SET FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL WHERE IDUSER = :id",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("id", testUser.IDUSER));

                    // 1. Set to disabled
                    testUser.DISABLED = 1;
                    db.SaveChanges();

                    // We need a password to test. Let's set a temporary known password hash.
                    string originalPassword = testUser.PASSWORD;
                    testUser.PASSWORD = Bu.CLASS_SYSTEM.PasswordHasher.HashPassword("temp_test_123");
                    db.SaveChanges();

                    // 2. Try to log in with correct password - should throw ApplicationException
                    var ex = Assert.Throws<ApplicationException>(() => sysUser.Login(testUser.USERNAME, "temp_test_123"));
                    Assert.AreEqual("ACCOUNT_LOCKED", ex.Message);

                    // 3. Try to log in with incorrect password - should throw WRONG_PASSWORD or ACCOUNT_LOCKED_NOW
                    var wrongPwEx = Assert.Throws<ApplicationException>(() => sysUser.Login(testUser.USERNAME, "wrong_password"));
                    Assert.That(wrongPwEx.Message, Is.EqualTo("WRONG_PASSWORD").Or.EqualTo("ACCOUNT_LOCKED_NOW"));

                    // Restore password & reset failed count
                    testUser.PASSWORD = originalPassword;
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_USER SET FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL WHERE IDUSER = :id",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("id", testUser.IDUSER));
                }
                finally
                {
                    // Restore original disabled state
                    testUser.DISABLED = originalDisabled;
                    db.SaveChanges();
                }
            }
        }

        [Test]
        public void TestKyCongChiTietEmployeeData()
        {
            using (var db = new MyEntities())
            {
                var kcList = db.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == 202601).ToList();
                Console.WriteLine($"Total rows in 202601: {kcList.Count}");
                
                var manvList = kcList.Select(x => x.MANV).Distinct().ToList();
                var nvList = db.TB_NHANVIEN.Where(x => manvList.Contains(x.MANV)).ToList();
                Console.WriteLine($"Total distinct NV matched: {nvList.Count}");

                int activeNvCount = nvList.Count(x => (x.DATHOIVIEC == null || x.DATHOIVIEC == 0) && x.DELETED_DATE == null);
                int resignedNvCount = nvList.Count(x => x.DATHOIVIEC == 1 || x.DELETED_DATE != null);
                Console.WriteLine($"Active NV in KyCong: {activeNvCount}, Resigned/Deleted NV in KyCong: {resignedNvCount}");

                var pbList = db.TB_PHONGBAN.ToList();
                Console.WriteLine($"Total departments in TB_PHONGBAN: {pbList.Count}");
                foreach (var pb in pbList)
                {
                    int empCount = nvList.Count(n => n.IDPB == pb.IDPB && (n.DATHOIVIEC == null || n.DATHOIVIEC == 0) && n.DELETED_DATE == null);
                    Console.WriteLine($"  IDPB {pb.IDPB}: '{pb.TENPB}' - Active Emp: {empCount}");
                }

                var manvCounts = kcList.GroupBy(x => x.MANV).Where(g => g.Count() > 1).ToList();
                Console.WriteLine($"Duplicate MANV count in KyCong: {manvCounts.Count}");
                foreach (var g in manvCounts.Take(5))
                {
                    Console.WriteLine($"MANV {g.Key} appears {g.Count()} times");
                }

                var unmatchedManv = manvList.Where(m => !nvList.Any(n => n.MANV == m)).ToList();
                Console.WriteLine($"Unmatched MANV (not in TB_NHANVIEN): {unmatchedManv.Count}");
                foreach (var m in unmatchedManv.Take(5))
                {
                    var kc = kcList.FirstOrDefault(k => k.MANV == m);
                    Console.WriteLine($"Unmatched MANV: {m}, KC.HOTEN: '{kc?.HOTEN}'");
                }

                Assert.AreEqual(971, activeNvCount);
            }
        }
    }
}
