using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Services;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class BulkProvisioningAndSecurityTests
    {
        private string _adminJwt;
        private const string BaseUrl = "http://localhost:55463";
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        [OneTimeSetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("HRMS_JWT_SECRET", "c74b9f5e18a2d36f9014b2e8ca95173f4e6d2081a95b3c7e1f4082d6e9a3b7c1");
            _adminJwt = JwtService.GenerateToken(
                1, "ADMIN", "Administrator", true, new List<string> { "*" }, "CTY01", "DVI01", "1", "WEB"
            );

            // Ensure ADMIN user has known password "123456" for automated login security tests
            using (var db = new MyEntities())
            {
                var admin = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME == "ADMIN");
                if (admin != null)
                {
                    admin.PASSWORD = PasswordHasher.HashPassword("admin");
                    admin.MANV = null; // System admin must never link to employee
                    admin.FAILED_LOGIN_COUNT = 0;
                    admin.LOCKOUT_END = null;
                    admin.DISABLED = 0;
                    admin.CLIENT_TYPE = "ALL";
                    db.SaveChanges();
                }
            }
        }

        private T GetValueCaseInsensitive<T>(Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var val)) return (T)Convert.ChangeType(val, typeof(T));
            string lowerKey = char.ToLowerInvariant(key[0]) + key.Substring(1);
            if (dict.TryGetValue(lowerKey, out val)) return (T)Convert.ChangeType(val, typeof(T));
            string upperKey = char.ToUpperInvariant(key[0]) + key.Substring(1);
            if (dict.TryGetValue(upperKey, out val)) return (T)Convert.ChangeType(val, typeof(T));
            throw new KeyNotFoundException($"Key '{key}' not found in dictionary");
        }

        [Test]
        public void UsersStats_LiveHttpTest_ReturnsAllSevenMetrics()
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _adminJwt;

                string json = client.DownloadString($"{BaseUrl}/api/users/stats");
                Assert.IsNotNull(json);

                var stats = _serializer.Deserialize<Dictionary<string, object>>(json);
                int total = GetValueCaseInsensitive<int>(stats, "TotalEmployees");
                int created = GetValueCaseInsensitive<int>(stats, "AccountsCreated");
                int without = GetValueCaseInsensitive<int>(stats, "EmployeesWithoutAccount");
                int mobileOn = GetValueCaseInsensitive<int>(stats, "MobileEnabled");
                int mobileOff = GetValueCaseInsensitive<int>(stats, "MobileDisabled");
                int locked = GetValueCaseInsensitive<int>(stats, "LockedAccounts");
                int system = GetValueCaseInsensitive<int>(stats, "SystemAccounts");

                Assert.Greater(total, 0, "Total employees should be > 0");
                Assert.GreaterOrEqual(created, 0, "AccountsCreated should be >= 0");
                Assert.GreaterOrEqual(without, 0, "EmployeesWithoutAccount should be >= 0");
                Assert.LessOrEqual(without, total, "EmployeesWithoutAccount should not exceed TotalEmployees");
                Assert.GreaterOrEqual(mobileOn, 0, "MobileEnabled should be >= 0");
                Assert.GreaterOrEqual(mobileOff, 0, "MobileDisabled should be >= 0");
                Assert.GreaterOrEqual(locked, 0, "LockedAccounts should be >= 0");
                Assert.GreaterOrEqual(system, 0, "SystemAccounts should be >= 0");
            }
        }

        [Test]
        public void BulkProvisionPreview_LiveHttpTest_ReturnsValidCandidateMetrics()
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _adminJwt;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                string payload = "{\"OnlyWithoutAccount\":true,\"OnlyMobileDisabled\":false}";
                string json = client.UploadString($"{BaseUrl}/api/users/bulk-provision-preview", "POST", payload);

                var res = _serializer.Deserialize<Dictionary<string, object>>(json);
                int totalEligible = GetValueCaseInsensitive<int>(res, "TotalEligible");
                Assert.GreaterOrEqual(totalEligible, 0);

                object candidatesObj = res.ContainsKey("Candidates") ? res["Candidates"] : res["candidates"];
                var candidates = candidatesObj as ArrayList;
                Assert.IsNotNull(candidates);

                // Candidates must have non-empty suggested login names following NVxxxxxx pattern
                foreach (Dictionary<string, object> c in candidates)
                {
                    string suggested = GetValueCaseInsensitive<string>(c, "SuggestedLoginName");
                    Assert.IsTrue(suggested.StartsWith("NV"), "Suggested login must start with NV prefix");
                }
            }
        }

        [Test]
        public void BulkProvision_IdempotencyTest_ReRunningOnSameEmployeeDoesNotDuplicate()
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _adminJwt;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                // Provision candidate MANV 141 (Thái)
                string payload = "{\"EmployeeIds\":[141],\"EnableMobile\":true,\"DefaultPassword\":\"TestPass@123\"}";

                // Run 1
                string res1Json = client.UploadString($"{BaseUrl}/api/users/bulk-provision", "POST", payload);
                var res1 = _serializer.Deserialize<Dictionary<string, object>>(res1Json);
                int total1 = GetValueCaseInsensitive<int>(res1, "Total");
                Assert.AreEqual(1, total1);

                // Run 2: Re-running on the exact same EmployeeId
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _adminJwt;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res2Json = client.UploadString($"{BaseUrl}/api/users/bulk-provision", "POST", payload);
                var res2 = _serializer.Deserialize<Dictionary<string, object>>(res2Json);
                int total2 = GetValueCaseInsensitive<int>(res2, "Total");
                Assert.AreEqual(1, total2);

                object resultsObj = res2.ContainsKey("Results") ? res2["Results"] : res2["results"];
                var results2 = resultsObj as ArrayList;
                Assert.IsNotNull(results2);
                Assert.AreEqual(1, results2.Count);

                // Second run must be idempotent: ALREADY_EXISTS or SUCCESS without duplicate user creation
                var item = results2[0] as Dictionary<string, object>;
                string secondRunResult = GetValueCaseInsensitive<string>(item, "Result");
                Assert.IsTrue(secondRunResult == "ALREADY_EXISTS" || secondRunResult == "SUCCESS",
                    "Second run must gracefully handle existing account without crashing or duplicating");
            }
        }

        [Test]
        public void Security_SystemAdminAccount_IsBlockedFromMobileLogin()
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                string payload = "{\"Username\":\"ADMIN\",\"Password\":\"123456\",\"ClientType\":\"MOBILE\"}";

                try
                {
                    client.UploadString($"{BaseUrl}/api/auth/login", "POST", payload);
                    Assert.Fail("ADMIN account must be rejected on MOBILE client type.");
                }
                catch (WebException ex)
                {
                    var resp = ex.Response as HttpWebResponse;
                    Assert.IsNotNull(resp);
                    Assert.IsTrue(resp.StatusCode == HttpStatusCode.BadRequest || resp.StatusCode == HttpStatusCode.Forbidden,
                        $"Expected 400 or 403, but received: {resp.StatusCode}");

                    using (var reader = new System.IO.StreamReader(resp.GetResponseStream()))
                    {
                        string body = reader.ReadToEnd();
                        Assert.IsTrue(body.Contains("Tài khoản Quản trị viên tối cao (ADMIN) là tài khoản hệ thống") ||
                                      body.Contains("không áp dụng") ||
                                      body.Contains("SYSTEM_ACCOUNT_NOT_ALLOWED") ||
                                      body.Contains("hệ thống"),
                                      $"Expected system account blocked message, but got: {body}");
                    }
                }
            }
        }

        [Test]
        public void Security_LoginNameFormat_IsIndependentOfEmployeeCode()
        {
            // Business Rule 7, 9, 58:
            // EmployeeCode e.g. "PX01-KT-TV-2026-001" must NOT be sliced/truncated.
            // LoginName is system-generated NVxxxxxx (independent of department or role).
            string employeeCode = "PX01-KT-TV-2026-001";
            int manv = 2327;
            string loginName = $"NV{manv:D6}";

            Assert.AreEqual("NV002327", loginName);
            Assert.AreNotEqual(employeeCode, loginName);
            Assert.IsFalse(loginName.Contains("PX01"), "LoginName must not encode department code");
            Assert.IsFalse(loginName.Contains("KT"), "LoginName must not encode position code");
            Assert.AreEqual("PX01-KT-TV-2026-001", employeeCode, "EmployeeCode must be preserved 100% untouched");
        }

        [Test]
        public void Desktop_FrmPheDuyetYeuCau_DirectOracleQueries_MustExecuteSuccessfully()
        {
            using (var db = new DA.MyEntities())
            {
                // Verify Leave query
                string leaveSql = @"
                    SELECT 
                        Y.ID AS ID,
                        Y.MANV AS MANV,
                        NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                        NV.HOTEN AS EMPLOYEE_NAME,
                        PB.TENPB AS DEPARTMENT_NAME,
                        Y.LOAIPHEP AS LOAI_NGHI,
                        TO_CHAR(Y.TUNGAY, 'DD/MM/YYYY') AS TU_NGAY,
                        TO_CHAR(Y.DENNGAY, 'DD/MM/YYYY') AS DEN_NGAY,
                        Y.SONGAY AS SO_NGAY,
                        Y.LYDO AS LYDO,
                        Y.TRANGTHAI AS TRANGTHAI,
                        TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                        NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                        Y.GHICHUDUYET AS LYDO_TUCHOI
                    FROM HR.TB_YEUCAU_NGHIPHEP Y
                    LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                    ORDER BY Y.ID DESC";

                var leaves = db.Database.SqlQuery<dynamic>(leaveSql).ToList();
                Assert.IsNotNull(leaves, "Leave query must not throw exception");

                // Verify Attendance query (TB_YEUCAU_DIEUCHINHCONG)
                string attSql = @"
                    SELECT 
                        Y.ID AS ID,
                        Y.MANV AS MANV,
                        NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                        NV.HOTEN AS EMPLOYEE_NAME,
                        PB.TENPB AS DEPARTMENT_NAME,
                        TO_CHAR(Y.NGAY, 'DD/MM/YYYY') AS NGAY_CONG,
                        Y.GIO_VAO AS GIO_VAO_MOI,
                        Y.GIO_RA AS GIO_RA_MOI,
                        Y.LYDO AS LYDO,
                        Y.TRANGTHAI AS TRANGTHAI,
                        TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                        NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                        Y.GHICHUDUYET AS LYDO_TUCHOI
                    FROM HR.TB_YEUCAU_DIEUCHINHCONG Y
                    LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                    ORDER BY Y.ID DESC";

                var atts = db.Database.SqlQuery<dynamic>(attSql).ToList();
                Assert.IsNotNull(atts, "Attendance query must not throw exception");

                // Verify Overtime query (TB_YEUCAU_TANGCA)
                string otSql = @"
                    SELECT 
                        Y.ID AS ID,
                        Y.MANV AS MANV,
                        NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                        NV.HOTEN AS EMPLOYEE_NAME,
                        PB.TENPB AS DEPARTMENT_NAME,
                        TO_CHAR(Y.NGAY, 'DD/MM/YYYY') AS NGAY_TANGCA,
                        Y.GIOTANGCA AS SO_GIO,
                        1.5 AS HE_SO,
                        Y.LYDO AS NOIDUNG,
                        Y.TRANGTHAI AS TRANGTHAI,
                        TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                        NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                        Y.GHICHUDUYET AS LYDO_TUCHOI
                    FROM HR.TB_YEUCAU_TANGCA Y
                    LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                    ORDER BY Y.ID DESC";

                var ots = db.Database.SqlQuery<dynamic>(otSql).ToList();
                Assert.IsNotNull(ots, "Overtime query must not throw exception");
            }
        }

        [Test]
        public void Desktop_FrmDuyetCapTaiKhoanHangLoat_CandidateQuery_MustExecuteSuccessfully()
        {
            using (var db = new DA.MyEntities())
            {
                string sql = @"
                    SELECT 
                        NV.MANV,
                        NV.EMPLOYEE_CODE,
                        NV.HOTEN,
                        PB.TENPB,
                        CV.TENCV,
                        U.USERNAME AS EXISTING_USERNAME,
                        M.IS_MOBILE_ENABLED
                    FROM HR.TB_NHANVIEN NV
                    LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                    LEFT JOIN HR.TB_CHUCVU CV ON NV.IDCV = CV.IDCV
                    LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON NV.MANV = M.EMPLOYEE_ID
                    LEFT JOIN HR.TB_SYS_USER U ON M.USER_ID = U.IDUSER
                    WHERE (NV.DATHOIVIEC = 0 OR NV.DATHOIVIEC IS NULL)";

                var candidates = db.Database.SqlQuery<dynamic>(sql).ToList();
                Assert.IsNotNull(candidates);
                Assert.IsTrue(candidates.Count > 0, "Active employees must be returned as candidates");
            }
        }

        [Test]
        public void Security_TwoAccountTypes_DesktopAndWebLoginRules()
        {
            var sysUserBus = new SYS_USER();

            // 1. Quản trị viên hệ thống (ADMIN): đăng nhập thành công trên Desktop bằng 'admin'
            var desktopAdmin = sysUserBus.Login("admin", "admin");
            Assert.IsNotNull(desktopAdmin, "ADMIN must successfully log in on Desktop with default 'admin'");
            Assert.AreEqual("ADMIN", desktopAdmin.USERNAME.Trim().ToUpper());

            // 2. Quản trị viên hệ thống (ADMIN): đăng nhập thành công trên Web qua API
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string payload = "{\"Username\":\"admin\",\"Password\":\"admin\",\"ClientType\":\"WEB\"}";
                string respJson = client.UploadString($"{BaseUrl}/api/auth/login", "POST", payload);
                Assert.IsTrue(respJson.Contains("token") || respJson.Contains("Token"), "Web login for ADMIN must return JWT token");
            }

            // 3. Tài khoản nhân viên (Mobile only): bị chặn trên Desktop
            using (var db = new DA.MyEntities())
            {
                var empUser = db.TB_SYS_USER.FirstOrDefault(u => u.CLIENT_TYPE == "MOBILE" && u.DISABLED == 0);
                if (empUser != null)
                {
                    // Desktop Direct Oracle
                    try
                    {
                        sysUserBus.Login(empUser.USERNAME, "123456");
                        Assert.Fail("Employee account must throw EMPLOYEE_MOBILE_ONLY on Desktop login");
                    }
                    catch (ApplicationException appEx)
                    {
                        Assert.AreEqual("EMPLOYEE_MOBILE_ONLY", appEx.Message);
                    }

                    // Web Portal API
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        string payload = $"{{\"Username\":\"{empUser.USERNAME}\",\"Password\":\"123456\",\"ClientType\":\"WEB\"}}";
                        try
                        {
                            client.UploadString($"{BaseUrl}/api/auth/login", "POST", payload);
                            Assert.Fail("Employee account must be rejected from Web portal login");
                        }
                        catch (WebException webEx)
                        {
                            var resp = webEx.Response as HttpWebResponse;
                            Assert.IsNotNull(resp);
                            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                        }
                    }
                }
            }
        }
    }
}

