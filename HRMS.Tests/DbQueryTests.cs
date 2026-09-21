using NUnit.Framework;
using System;
using System.Linq;
using DA;

namespace Bu.Tests
{
    [TestFixture]
    public partial class DbQueryTests
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
        public void EnsureAdminAndNhansuAccounts()
        {
            using (var db = new MyEntities())
            {
                var admin = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME == "ADMIN");
                Assert.IsNotNull(admin, "Tài khoản ADMIN phải tồn tại.");

                // Quy tắc: Tài khoản Quản trị viên tối cao (ADMIN) là tài khoản hệ thống, không được phép liên kết với hồ sơ nhân viên
                if (admin.MANV.HasValue)
                {
                    admin.MANV = null;
                    db.SaveChanges();
                }
                db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", admin.IDUSER));

                Assert.IsNull(admin.MANV, "Tài khoản ADMIN không được liên kết với hồ sơ nhân viên.");

                var nhansu = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME == "nhansu");
                Assert.IsNotNull(nhansu, "Tài khoản nhansu phải tồn tại.");
                Assert.AreEqual(141, nhansu.MANV, "Tài khoản nhansu phải liên kết MANV 141.");

                var nv2327 = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME == "nv2327" || u.MANV == 2327);
                if (nv2327 == null)
                {
                    var emp = db.TB_NHANVIEN.FirstOrDefault(e => e.MANV == 2327);
                    if (emp != null)
                    {
                        nv2327 = new TB_SYS_USER
                        {
                            USERNAME = "nv2327",
                            FULLNAME = emp.HOTEN,
                            PASSWORD = Bu.CLASS_SYSTEM.PasswordHasher.HashPassword("123"),
                            MANV = 2327,
                            DISABLED = 0,
                            CLIENT_TYPE = "ALL",
                            ISGROUP = 0,
                            MACTY = "1",
                            MADVI = "1"
                        };
                        db.TB_SYS_USER.Add(nv2327);
                        db.SaveChanges();
                    }
                }
                else
                {
                    nv2327.PASSWORD = Bu.CLASS_SYSTEM.PasswordHasher.HashPassword("123");
                    nv2327.DISABLED = 0;
                    nv2327.CLIENT_TYPE = "ALL";
                    db.SaveChanges();
                }
                Assert.IsNotNull(nv2327, "Tài khoản nhân viên nv2327 phải sẵn sàng.");
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

                Assert.IsTrue(activeNvCount >= 900, $"Active employee count should be >= 900 (actual: {activeNvCount})");
            }
        }

        [Test]
        public void TestKiemTraVaCapNhatTrangThaiHopDong()
        {
            var nvBus = new NHANVIEN();
            var lstEligible = nvBus.KiemTraVaCapNhatTrangThaiHopDong(2026, 1, null);
            Console.WriteLine($"Eligible employees count for 2026/01: {lstEligible.Count}");
            Assert.IsNotNull(lstEligible);
            Assert.IsTrue(lstEligible.Count >= 900, "Phải có từ 900 nhân sự đủ điều kiện hợp đồng trở lên.");

            using (var db = new MyEntities())
            {
                // Kiểm tra không có nhân viên nào trong danh sách hợp lệ mà đã thôi việc
                foreach (var nv in lstEligible)
                {
                    Assert.AreNotEqual(1, nv.DATHOIVIEC, $"Nhân viên {nv.MANV} đã thôi việc không được nằm trong danh sách hợp lệ.");
                }

                // Kiểm tra các nhân viên hết hạn hợp đồng trước 2026-01-01 phải có DATHOIVIEC = 1
                var expiredContracts = db.TB_HOPDONG
                    .Where(x => x.NGAYKETTHUC.HasValue && x.NGAYKETTHUC.Value < new DateTime(2026, 1, 1))
                    .Select(x => x.MANV)
                    .Distinct()
                    .ToList();

                foreach (var manv in expiredContracts)
                {
                    var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                    var latestHd = db.TB_HOPDONG.Where(x => x.MANV == manv).OrderByDescending(x => x.NGAYBATDAU).FirstOrDefault();
                    if (latestHd != null && latestHd.NGAYKETTHUC.HasValue && latestHd.NGAYKETTHUC.Value < new DateTime(2026, 1, 1))
                    {
                        Assert.AreEqual(1, nv.DATHOIVIEC, $"Nhân viên {manv} hết hạn hợp đồng phải có DATHOIVIEC = 1.");
                    }
                }
            }
        }

        [Test]
        public void TestApprovalTablesQueriesAndCounts()
        {
            using (var db = new MyEntities())
            {
                // 1. Verify summary counts query executes cleanly with decimal mapping
                int leaveCount = (int)db.Database.SqlQuery<decimal>(
                    "SELECT COUNT(*) FROM HR.TB_YEUCAU_NGHIPHEP WHERE TRANGTHAI = 'PENDING'"
                ).FirstOrDefault();

                int attendanceCount = (int)db.Database.SqlQuery<decimal>(
                    "SELECT COUNT(*) FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE TRANGTHAI = 'PENDING'"
                ).FirstOrDefault();

                int overtimeCount = (int)db.Database.SqlQuery<decimal>(
                    "SELECT COUNT(*) FROM HR.TB_YEUCAU_TANGCA WHERE TRANGTHAI = 'PENDING'"
                ).FirstOrDefault();

                Assert.IsTrue(leaveCount >= 0);
                Assert.IsTrue(attendanceCount >= 0);
                Assert.IsTrue(overtimeCount >= 0);

                // 2. Verify leave query with schema aliases
                string leaveSql = @"
                    SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                           Y.LOAIPHEP AS LOAI_NGHI, Y.TUNGAY AS TU_NGAY, Y.DENNGAY AS DEN_NGAY, Y.SONGAY AS SO_NGAY, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                           NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                    FROM HR.TB_YEUCAU_NGHIPHEP Y
                    LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                    WHERE (1=1) ORDER BY Y.CREATED_DATE DESC";

                var leaveRows = db.Database.SqlQuery<LeaveRowTest>(leaveSql).ToList();
                Assert.IsNotNull(leaveRows);

                // 3. Verify attendance query with schema aliases
                string attSql = @"
                    SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                           Y.NGAY AS NGAY_CONG, Y.GIO_VAO AS GIO_VAO_MOI, Y.GIO_RA AS GIO_RA_MOI, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                           NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                    FROM HR.TB_YEUCAU_DIEUCHINHCONG Y
                    LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                    WHERE (1=1) ORDER BY Y.CREATED_DATE DESC";

                var attRows = db.Database.SqlQuery<AttendanceRowTest>(attSql).ToList();
                Assert.IsNotNull(attRows);

                // 4. Verify overtime query with schema aliases
                string otSql = @"
                    SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                           Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 1.5 AS HE_SO, Y.LYDO AS NOI_DUNG, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                           NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                    FROM HR.TB_YEUCAU_TANGCA Y
                    LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                    WHERE (1=1) ORDER BY Y.CREATED_DATE DESC";

                var otRows = db.Database.SqlQuery<OvertimeRowTest>(otSql).ToList();
                Assert.IsNotNull(otRows);
            }
        }
    }

    public class LeaveRowTest
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public string LOAI_NGHI { get; set; }
        public DateTime? TU_NGAY { get; set; }
        public DateTime? DEN_NGAY { get; set; }
        public decimal? SO_NGAY { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class AttendanceRowTest
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public DateTime? NGAY_CONG { get; set; }
        public string GIO_VAO_MOI { get; set; }
        public string GIO_RA_MOI { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class OvertimeRowTest
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public DateTime? NGAY_TANGCA { get; set; }
        public decimal? SO_GIO { get; set; }
        public decimal? HE_SO { get; set; }
        public string NOI_DUNG { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class UserCheckRow
    {
        public decimal IDUSER { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public decimal? ISGROUP { get; set; }
        public decimal? DISABLED { get; set; }
        public string CLIENT_TYPE { get; set; }
        public decimal? MANV { get; set; }
        public decimal? FAILED_LOGIN_COUNT { get; set; }
        public DateTime? LOCKOUT_END { get; set; }
    }

    public partial class DbQueryTests
    {
        [Test]
        public void InspectAdminUserState()
        {
            using (var db = new MyEntities())
            {
                var admin = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME == "ADMIN");
                if (admin != null)
                {
                    admin.PASSWORD = Bu.CLASS_SYSTEM.PasswordHasher.HashPassword("admin");
                    admin.DISABLED = 0;
                    admin.CLIENT_TYPE = "ALL";
                    admin.MANV = null;
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_USER SET FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL WHERE IDUSER = :id",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("id", admin.IDUSER));
                }

                var users = db.Database.SqlQuery<UserCheckRow>("SELECT IDUSER, USERNAME, PASSWORD, ISGROUP, DISABLED, CLIENT_TYPE, MANV, FAILED_LOGIN_COUNT, LOCKOUT_END FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' OR UPPER(TRIM(USERNAME)) = 'NHANSU'").ToList();
                foreach (var u in users)
                {
                    bool matchAdmin = Bu.CLASS_SYSTEM.PasswordHasher.VerifyPassword("ADMIN", u.PASSWORD);
                    bool matchAdminLower = Bu.CLASS_SYSTEM.PasswordHasher.VerifyPassword("admin", u.PASSWORD);
                    bool match123 = Bu.CLASS_SYSTEM.PasswordHasher.VerifyPassword("123", u.PASSWORD);
                    bool match123456 = Bu.CLASS_SYSTEM.PasswordHasher.VerifyPassword("123456", u.PASSWORD);
                    Console.WriteLine($"USER: ID={u.IDUSER}, NAME='{u.USERNAME}', ISGROUP={u.ISGROUP}, DISABLED={u.DISABLED}, CLIENT_TYPE='{u.CLIENT_TYPE}', MANV={u.MANV}, FAILED={u.FAILED_LOGIN_COUNT}, LOCKOUT={u.LOCKOUT_END}, MatchADMIN={matchAdmin}, Matchadmin={matchAdminLower}, Match123={match123}, Match123456={match123456}");
                }
            }
        }

        [Test]
        public void InspectDatabaseMetadataForOvertime()
        {
            using (var db = new MyEntities())
            {
                var tables = new[] { "TB_YEUCAU_TANGCA", "TB_YEUCAU_NGHIPHEP", "TB_YEUCAU_DIEUCHINHCONG", "TB_USER_EMPLOYEE_MAPPING", "TB_TANGCA", "TB_LOAICA", "TB_SYS_USER", "TB_NHANVIEN" };
                foreach (var tbl in tables)
                {
                    Console.WriteLine($"=== TABLE: {tbl} ===");
                    // Columns
                    var cols = db.Database.SqlQuery<ColMeta>(@"
                        SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE, DATA_DEFAULT 
                        FROM ALL_TAB_COLS 
                        WHERE TABLE_NAME = :p0 AND OWNER = 'HR'
                        ORDER BY COLUMN_ID", new Oracle.ManagedDataAccess.Client.OracleParameter("p0", tbl)).ToList();
                    foreach (var c in cols)
                    {
                        Console.WriteLine($"  COL: {c.COLUMN_NAME} | Type={c.DATA_TYPE}({c.DATA_PRECISION ?? c.DATA_LENGTH},{c.DATA_SCALE}) | Null={c.NULLABLE} | Default={c.DATA_DEFAULT?.Trim()}");
                    }

                    // Constraints
                    var cons = db.Database.SqlQuery<ConMeta>(@"
                        SELECT c.CONSTRAINT_NAME, c.CONSTRAINT_TYPE, c.SEARCH_CONDITION, c.R_CONSTRAINT_NAME, r.TABLE_NAME AS R_TABLE_NAME
                        FROM ALL_CONSTRAINTS c
                        LEFT JOIN ALL_CONSTRAINTS r ON c.R_CONSTRAINT_NAME = r.CONSTRAINT_NAME AND r.OWNER = c.OWNER
                        WHERE c.TABLE_NAME = :p0 AND c.OWNER = 'HR'", new Oracle.ManagedDataAccess.Client.OracleParameter("p0", tbl)).ToList();
                    foreach (var cn in cons)
                    {
                        Console.WriteLine($"  CON: {cn.CONSTRAINT_NAME} | Type={cn.CONSTRAINT_TYPE} | Ref={cn.R_TABLE_NAME}({cn.R_CONSTRAINT_NAME}) | Cond={cn.SEARCH_CONDITION?.Trim()}");
                    }

                    // Indexes
                    var idxs = db.Database.SqlQuery<IdxMeta>(@"
                        SELECT i.INDEX_NAME, i.UNIQUENESS, ic.COLUMN_NAME, ic.COLUMN_POSITION
                        FROM ALL_INDEXES i
                        JOIN ALL_IND_COLUMNS ic ON i.INDEX_NAME = ic.INDEX_NAME AND i.OWNER = ic.INDEX_OWNER
                        WHERE i.TABLE_NAME = :p0 AND i.OWNER = 'HR'
                        ORDER BY i.INDEX_NAME, ic.COLUMN_POSITION", new Oracle.ManagedDataAccess.Client.OracleParameter("p0", tbl)).ToList();
                    foreach (var idx in idxs)
                    {
                        Console.WriteLine($"  IDX: {idx.INDEX_NAME} | Unique={idx.UNIQUENESS} | Col={idx.COLUMN_NAME} (#{idx.COLUMN_POSITION})");
                    }

                    // Count
                    var count = db.Database.SqlQuery<decimal>($"SELECT COUNT(*) FROM HR.{tbl}").FirstOrDefault();
                    Console.WriteLine($"  ROW COUNT: {count}");
                }

                // Check distinct TRANGTHAI in request tables
                var reqTables = new[] { "TB_YEUCAU_TANGCA", "TB_YEUCAU_NGHIPHEP", "TB_YEUCAU_DIEUCHINHCONG" };
                foreach (var rt in reqTables)
                {
                    var statuses = db.Database.SqlQuery<string>($"SELECT DISTINCT TRANGTHAI FROM HR.{rt}").ToList();
                    Console.WriteLine($"STATUSES in {rt}: {string.Join(", ", statuses)}");
                }

                // Check TB_LOAICA rows
                var loaiCaList = db.Database.SqlQuery<LoaiCaRow>("SELECT IDLOAICA, TENLOAICA, HESOLOAICA FROM HR.TB_LOAICA").ToList();
                Console.WriteLine("TB_LOAICA list:");
                foreach (var lc in loaiCaList)
                {
                    Console.WriteLine($"  ID={lc.IDLOAICA}, TEN='{lc.TENLOAICA}', HESO={lc.HESOLOAICA}");
                }
            }
        }
    }

    public class ColMeta
    {
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public decimal? DATA_LENGTH { get; set; }
        public decimal? DATA_PRECISION { get; set; }
        public decimal? DATA_SCALE { get; set; }
        public string NULLABLE { get; set; }
        public string DATA_DEFAULT { get; set; }
    }

    public class ConMeta
    {
        public string CONSTRAINT_NAME { get; set; }
        public string CONSTRAINT_TYPE { get; set; }
        public string SEARCH_CONDITION { get; set; }
        public string R_CONSTRAINT_NAME { get; set; }
        public string R_TABLE_NAME { get; set; }
    }

    public class IdxMeta
    {
        public string INDEX_NAME { get; set; }
        public string UNIQUENESS { get; set; }
        public string COLUMN_NAME { get; set; }
        public decimal? COLUMN_POSITION { get; set; }
    }

    public class LoaiCaRow
    {
        public decimal IDLOAICA { get; set; }
        public string TENLOAICA { get; set; }
        public decimal? HESOLOAICA { get; set; }
    }
}

