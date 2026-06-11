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

                    // 3. Try to log in with incorrect password - should return null (not throw lock exception)
                    var loginResult = sysUser.Login(testUser.USERNAME, "wrong_password");
                    Assert.IsNull(loginResult);

                    // Restore password
                    testUser.PASSWORD = originalPassword;
                    db.SaveChanges();
                }
                finally
                {
                    // Restore original disabled state
                    testUser.DISABLED = originalDisabled;
                    db.SaveChanges();
                }
            }
        }
    }
}
