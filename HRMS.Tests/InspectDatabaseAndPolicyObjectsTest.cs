using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HRMS.Tests
{
    [TestFixture]
    public class InspectDatabaseAndPolicyObjectsTest
    {
        private const string ConnectionString = "DATA SOURCE=localhost:1521/orcl;PASSWORD=hr;USER ID=HR";

        [Test]
        public void Execute_ReadOnly_Database_Diagnosis()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("=== 1. DATABASE & SESSION CONTEXT ===");
            Console.WriteLine("================================================================================");

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT SYS_CONTEXT('USERENV', 'SESSION_USER') AS SESSION_USER,
                               SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AS CURRENT_SCHEMA,
                               SYS_CONTEXT('USERENV', 'DB_NAME') AS DB_NAME,
                               SYS_CONTEXT('USERENV', 'SERVICE_NAME') AS SERVICE_NAME
                        FROM DUAL";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"SESSION_USER   : {reader["SESSION_USER"]}");
                            Console.WriteLine($"CURRENT_SCHEMA : {reader["CURRENT_SCHEMA"]}");
                            Console.WriteLine($"DB_NAME        : {reader["DB_NAME"]}");
                            Console.WriteLine($"SERVICE_NAME   : {reader["SERVICE_NAME"]}");
                        }
                    }
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 2. TARGET OBJECTS IN ALL_TABLES (Across all schemas visible to user) ===");
                Console.WriteLine("================================================================================");

                var targetObjects = new List<string>
                {
                    "TB_CHINH_SACH_LUONG",
                    "TB_CHINH_SACH_BHXH",
                    "TB_CHINH_SACH_BHXH_VUNG",
                    "TB_CHINH_SACH_CONG_DOAN",
                    "TB_THUE_TNCN_CHINH_SACH",
                    "TB_THUE_TNCN_BAC",
                    "TB_PAYROLL_CALCULATION_RUN",
                    "TB_BANGLUONG_CT",
                    "TB_BANGLUONG"
                };

                using (var cmd = conn.CreateCommand())
                {
                    string inList = string.Join(",", targetObjects.ConvertAll(t => $"'{t}'"));
                    cmd.CommandText = $"SELECT OWNER, TABLE_NAME, NUM_ROWS FROM ALL_TABLES WHERE TABLE_NAME IN ({inList}) OR TABLE_NAME LIKE '%CHINH_SACH%' ORDER BY OWNER, TABLE_NAME";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Console.WriteLine($"  [ALL_TABLES] OWNER: {reader["OWNER"]}, TABLE: {reader["TABLE_NAME"]}, NUM_ROWS: {reader["NUM_ROWS"]}");
                        }
                        if (count == 0)
                        {
                            Console.WriteLine("  -> ZERO target tables found in ALL_TABLES for any schema visible to this user.");
                        }
                    }
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 3. TARGET OBJECTS IN ALL_VIEWS ===");
                Console.WriteLine("================================================================================");
                using (var cmd = conn.CreateCommand())
                {
                    string inList = string.Join(",", targetObjects.ConvertAll(t => $"'{t}'"));
                    cmd.CommandText = $"SELECT OWNER, VIEW_NAME FROM ALL_VIEWS WHERE VIEW_NAME IN ({inList}) ORDER BY OWNER, VIEW_NAME";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Console.WriteLine($"  [ALL_VIEWS] OWNER: {reader["OWNER"]}, VIEW: {reader["VIEW_NAME"]}");
                        }
                        if (count == 0)
                        {
                            Console.WriteLine("  -> ZERO target views found in ALL_VIEWS.");
                        }
                    }
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 4. TARGET OBJECTS IN ALL_SYNONYMS ===");
                Console.WriteLine("================================================================================");
                using (var cmd = conn.CreateCommand())
                {
                    string inList = string.Join(",", targetObjects.ConvertAll(t => $"'{t}'"));
                    cmd.CommandText = $"SELECT OWNER, SYNONYM_NAME, TABLE_OWNER, TABLE_NAME FROM ALL_SYNONYMS WHERE SYNONYM_NAME IN ({inList}) ORDER BY OWNER, SYNONYM_NAME";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Console.WriteLine($"  [ALL_SYNONYMS] OWNER: {reader["OWNER"]}, SYNONYM: {reader["SYNONYM_NAME"]} -> {reader["TABLE_OWNER"]}.{reader["TABLE_NAME"]}");
                        }
                        if (count == 0)
                        {
                            Console.WriteLine("  -> ZERO target synonyms found in ALL_SYNONYMS.");
                        }
                    }
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 5. CHECK PRIVILEGES IN ALL_TAB_PRIVS / USER_TAB_PRIVS ===");
                Console.WriteLine("================================================================================");
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        string inList = string.Join(",", targetObjects.ConvertAll(t => $"'{t}'"));
                        cmd.CommandText = $"SELECT GRANTEE, TABLE_NAME, PRIVILEGE FROM ALL_TAB_PRIVS WHERE TABLE_NAME IN ({inList}) ORDER BY TABLE_NAME, PRIVILEGE";
                        using (var reader = cmd.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                Console.WriteLine($"  [PRIVILEGE] TABLE: {reader["TABLE_NAME"]}, PRIVILEGE: {reader["PRIVILEGE"]}, GRANTEE: {reader["GRANTEE"]}");
                            }
                            if (count == 0)
                            {
                                Console.WriteLine("  -> NO explicit privileges granted on target objects recorded in ALL_TAB_PRIVS.");
                            }
                        }
                    }
                }
                catch (Exception pex)
                {
                    Console.WriteLine($"  -> ALL_TAB_PRIVS check skipped: {pex.Message}");
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 6. CHECK ALL OBJECTS IN HR SCHEMA LIKE '%CHINH_SACH%' OR '%THUE%' ===");
                Console.WriteLine("================================================================================");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT OBJECT_TYPE, OBJECT_NAME, STATUS 
                        FROM USER_OBJECTS 
                        WHERE OBJECT_NAME LIKE '%CHINH_SACH%' OR OBJECT_NAME LIKE '%THUE%' OR OBJECT_NAME LIKE '%POLICY%'
                        ORDER BY OBJECT_TYPE, OBJECT_NAME";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Console.WriteLine($"  [HR OBJECT] TYPE: {reader["OBJECT_TYPE"]}, NAME: {reader["OBJECT_NAME"]}, STATUS: {reader["STATUS"]}");
                        }
                        if (count == 0)
                        {
                            Console.WriteLine("  -> NO objects matching '%CHINH_SACH%' or '%THUE%' in HR schema.");
                        }
                    }
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 7. EXECUTE INDIVIDUAL POLICYRESOLVER QUERIES (With exact SQL and error) ===");
                Console.WriteLine("================================================================================");

                var queries = new[]
                {
                    new {
                        Group = "Chính sách lương",
                        TargetTable = "TB_CHINH_SACH_LUONG",
                        SQL = "SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, SO_CONG_CHUAN_THANG, SO_GIO_CHUAN_NGAY, HE_SO_LAM_DEM, TRANG_THAI FROM TB_CHINH_SACH_LUONG WHERE TRANG_THAI = 'ACTIVE' ORDER BY NGAY_HIEU_LUC"
                    },
                    new {
                        Group = "Chính sách BHXH",
                        TargetTable = "TB_CHINH_SACH_BHXH",
                        SQL = "SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, MUC_THAM_CHIEU, TY_LE_BHXH_NLD, TY_LE_BHYT_NLD, TY_LE_BHTN_NLD, TY_LE_BHXH_NSDLD, TY_LE_BHYT_NSDLD, TY_LE_BHTN_NSDLD, TY_LE_TNLD_BNN_NSDLD, TY_LE_TNLD_BNN_UU_DAI, AP_DUNG_TRAN_BHXH_BHYT, AP_DUNG_TRAN_BHTN, TRANG_THAI FROM TB_CHINH_SACH_BHXH WHERE TRANG_THAI = 'ACTIVE' ORDER BY NGAY_HIEU_LUC"
                    },
                    new {
                        Group = "Chính sách BHXH Vùng",
                        TargetTable = "TB_CHINH_SACH_BHXH_VUNG",
                        SQL = "SELECT ID, POLICY_BHXH_ID, VUNG_LUONG, LUONG_TOI_THIEU_THANG, LUONG_TOI_THIEU_GIO, HE_SO_SAN_DOANH_NGHIEP FROM TB_CHINH_SACH_BHXH_VUNG ORDER BY POLICY_BHXH_ID, VUNG_LUONG"
                    },
                    new {
                        Group = "Chính sách Công đoàn",
                        TargetTable = "TB_CHINH_SACH_CONG_DOAN",
                        SQL = "SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, TY_LE_DOAN_PHI_NLD, CAP_PERCENT_STATUTORY_BASE_SALARY, KINH_PHI_CONG_DOAN_NSDLD, TRANG_THAI FROM TB_CHINH_SACH_CONG_DOAN WHERE TRANG_THAI = 'ACTIVE' ORDER BY NGAY_HIEU_LUC"
                    },
                    new {
                        Group = "Chính sách thuế TNCN",
                        TargetTable = "TB_THUE_TNCN_CHINH_SACH",
                        SQL = "SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, TAX_YEAR, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, GIAM_TRU_BAN_THAN_THANG, GIAM_TRU_PHU_THUOC_THANG, GIAM_TRU_BAN_THAN_NAM, GIAM_TRU_PHU_THUOC_NAM, TRANG_THAI FROM TB_THUE_TNCN_CHINH_SACH WHERE TRANG_THAI = 'ACTIVE' ORDER BY NGAY_HIEU_LUC"
                    },
                    new {
                        Group = "Biểu bậc thuế TNCN",
                        TargetTable = "TB_THUE_TNCN_BAC",
                        SQL = "SELECT ID, POLICY_THUE_ID, TAX_YEAR, PERIOD_TYPE, BAC_THUE, CAN_DUOI, CAN_TREN, THUE_SUAT FROM TB_THUE_TNCN_BAC ORDER BY POLICY_THUE_ID, PERIOD_TYPE, BAC_THUE"
                    }
                };

                foreach (var q in queries)
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = q.SQL;
                            using (var r = cmd.ExecuteReader())
                            {
                                int rowCount = 0;
                                while (r.Read()) rowCount++;
                                Console.WriteLine($"  [SUCCESS] {q.Group} ({q.TargetTable}): Query succeeded. Row count = {rowCount}.");
                            }
                        }
                    }
                    catch (OracleException oex)
                    {
                        Console.WriteLine($"  [FAILED] {q.Group} ({q.TargetTable}): ORA-{oex.Number:D5}: {oex.Message.Trim()}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  [FAILED] {q.Group} ({q.TargetTable}): {ex.GetType().Name}: {ex.Message.Trim()}");
                    }
                }

                Console.WriteLine("\n================================================================================");
                Console.WriteLine("=== 8. CHECK OTHER USERS/SCHEMAS IN DATABASE ===");
                Console.WriteLine("================================================================================");
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT USERNAME FROM ALL_USERS ORDER BY USERNAME";
                        using (var reader = cmd.ExecuteReader())
                        {
                            var users = new List<string>();
                            while (reader.Read())
                            {
                                users.Add(reader["USERNAME"].ToString());
                            }
                            Console.WriteLine($"  Visible database users ({users.Count}): {string.Join(", ", users)}");
                        }
                    }
                }
                catch (Exception uex)
                {
                    Console.WriteLine($"  -> ALL_USERS query failed: {uex.Message}");
                }
            }
        }

        [Test]
        public void Check_Admin_User_And_Rights()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                Console.WriteLine("================================================================================");
                Console.WriteLine("=== INSPECTING ADMIN ACCOUNT & PERMISSIONS (NO HARD-CODING) ===");
                Console.WriteLine("================================================================================");

                decimal adminId = 0;
                string adminUsername = null;
                long tokenVersion = 0;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION, LOCKOUT_END FROM HR.TB_SYS_USER WHERE LOWER(USERNAME) = 'admin'";
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            adminId = Convert.ToDecimal(r["IDUSER"]);
                            adminUsername = r["USERNAME"].ToString();
                            tokenVersion = Convert.ToInt64(r["TOKEN_VERSION"]);
                            Console.WriteLine($"[USER] ID: {adminId}, USERNAME: '{adminUsername}', FULLNAME: '{r["FULLNAME"]}', DISABLED: {r["DISABLED"]}, TOKEN_VERSION: {tokenVersion}, LOCKOUT: {r["LOCKOUT_END"]}");
                        }
                        else
                        {
                            Console.WriteLine("[ERROR] User 'admin' does not exist in TB_SYS_USER!");
                            return;
                        }
                    }
                }

                // 2. Count total functions
                int totalFunctions = 0;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM HR.TB_SYS_FUNCTION";
                    totalFunctions = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"[TB_SYS_FUNCTION] Total registered functions in system: {totalFunctions}");
                }

                // 3. Count admin direct rights
                int adminRightsCount = 0;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM HR.TB_SYS_RIGHT WHERE IDUSER = :p0 AND (CAN_VIEW = 1 OR USER_RIGHT = 1)";
                    cmd.Parameters.Add("p0", adminId);
                    adminRightsCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"[TB_SYS_RIGHT] Admin active rights: {adminRightsCount} / {totalFunctions}");
                }

                // 4. Missing functions in TB_SYS_RIGHT
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT f.FUNCTION_CODE, f.DESCRIPTION
                        FROM HR.TB_SYS_FUNCTION f
                        WHERE f.FUNCTION_CODE NOT IN (
                            SELECT r.FUNCTION_CODE FROM HR.TB_SYS_RIGHT r 
                            WHERE r.IDUSER = :p0 AND (r.CAN_VIEW = 1 OR r.USER_RIGHT = 1)
                        )
                        ORDER BY f.FUNCTION_CODE";
                    cmd.Parameters.Add("p0", adminId);
                    using (var r = cmd.ExecuteReader())
                    {
                        var missing = new List<string>();
                        while (r.Read())
                        {
                            missing.Add($"{r["FUNCTION_CODE"]} ({r["DESCRIPTION"]})");
                        }
                        Console.WriteLine($"[TB_SYS_RIGHT] Total functions MISSING for admin: {missing.Count}");
                        foreach (var m in missing)
                        {
                            Console.WriteLine($"  - Missing: {m}");
                        }
                    }
                }

                // 5. Check crucial security & AI functions
                Console.WriteLine("\n[CRITICAL FUNCTIONS CHECK]:");
                var criticalCodes = new[] { "F_SYSTEM_AI", "F_SYSTEM_AI_CONFIG", "F_LOGIN_DESKTOP", "F_LOGIN_WEB", "F_LOGIN_MOBILE", "F_SYSTEM_SETTING" };
                foreach (var code in criticalCodes)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE FROM HR.TB_SYS_RIGHT WHERE IDUSER = :p0 AND FUNCTION_CODE = :p1";
                        cmd.Parameters.Add("p0", adminId);
                        cmd.Parameters.Add("p1", code);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                Console.WriteLine($"  * {code}: USER_RIGHT={r["USER_RIGHT"]}, VIEW={r["CAN_VIEW"]}, ADD={r["CAN_ADD"]}, EDIT={r["CAN_EDIT"]}, DEL={r["CAN_DELETE"]}");
                            }
                            else
                            {
                                Console.WriteLine($"  * {code}: [NOT FOUND IN TB_SYS_RIGHT] - NO EXPLICIT RECORD!");
                            }
                        }
                    }
                }

                // 6. Check sessions in TB_AUTH_SESSION
                Console.WriteLine("\n[TB_AUTH_SESSION] Recent sessions for admin:");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT SESSION_ID, JTI, CLIENT_TYPE, EXPIRES_AT, REVOKED_AT, LAST_USED_AT, CREATED_AT
                        FROM (
                            SELECT * FROM HR.TB_AUTH_SESSION
                            WHERE USER_ID = :p0
                            ORDER BY CREATED_AT DESC
                        ) WHERE ROWNUM <= 5";
                    cmd.Parameters.Add("p0", adminId);
                    using (var r = cmd.ExecuteReader())
                    {
                        int sCount = 0;
                        while (r.Read())
                        {
                            sCount++;
                            Console.WriteLine($"  Session #{sCount}: ID={r["SESSION_ID"]}, JTI={r["JTI"]}, Channel={r["CLIENT_TYPE"]}, Created={r["CREATED_AT"]}, Expires={r["EXPIRES_AT"]}, Revoked={r["REVOKED_AT"]}");
                        }
                        if (sCount == 0) Console.WriteLine("  No sessions found in TB_AUTH_SESSION for admin!");
                    }
                }

                // 7. Check PlatformAccessGuard evaluation for admin
                Console.WriteLine("\n[PLATFORM ACCESS GUARD EVALUATION]:");
                bool canDesktop = Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                    adminId, "DESKTOP", "F_LOGIN_DESKTOP", Bu.CLASS_SECURITY.ChannelAction.View, adminUsername
                );
                Console.WriteLine($"  Can Login DESKTOP: {canDesktop}");

                bool canAi = Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                    adminId, "DESKTOP", "F_SYSTEM_AI", Bu.CLASS_SECURITY.ChannelAction.View, adminUsername
                );
                Console.WriteLine($"  Can Access AI on DESKTOP: {canAi}");

                bool canAiConfig = Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                    adminId, "DESKTOP", "F_SYSTEM_AI_CONFIG", Bu.CLASS_SECURITY.ChannelAction.View, adminUsername
                );
                Console.WriteLine($"  Can Access AI_CONFIG on DESKTOP: {canAiConfig}");

                // 8. Check TB_SYS_RIGHT_CHANNEL for Admin
                Console.WriteLine("\n[TB_SYS_RIGHT_CHANNEL CHECK FOR ADMIN]:");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT CLIENT_TYPE, COUNT(*) AS TOTAL_FUNCS,
                               SUM(NVL(CAN_VIEW, 0)) AS TOTAL_VIEW,
                               SUM(NVL(CAN_ADD, 0)) AS TOTAL_ADD,
                               SUM(NVL(CAN_EDIT, 0)) AS TOTAL_EDIT,
                               SUM(NVL(CAN_DELETE, 0)) AS TOTAL_DELETE,
                               SUM(NVL(CAN_PRINT, 0)) AS TOTAL_PRINT
                        FROM HR.TB_SYS_RIGHT_CHANNEL
                        WHERE IDUSER = :p0
                        GROUP BY CLIENT_TYPE
                        ORDER BY CLIENT_TYPE";
                    cmd.Parameters.Add("p0", adminId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Console.WriteLine($"  Channel: {r["CLIENT_TYPE"]} | Funcs: {r["TOTAL_FUNCS"]} | View: {r["TOTAL_VIEW"]} | Add: {r["TOTAL_ADD"]} | Edit: {r["TOTAL_EDIT"]} | Del: {r["TOTAL_DELETE"]} | Print: {r["TOTAL_PRINT"]}");
                        }
                    }
                }

                // 9. Test Session Validation on most recent session
                Console.WriteLine("\n[SESSION VALIDATION TEST VIA AuthSecurityService]:");
                var authSec = new Bu.CLASS_SECURITY.AuthSecurityService();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT JTI, CLIENT_TYPE, SESSION_ID FROM (SELECT JTI, CLIENT_TYPE, SESSION_ID FROM HR.TB_AUTH_SESSION WHERE USER_ID = :p0 ORDER BY CREATED_AT DESC) WHERE ROWNUM = 1";
                    cmd.Parameters.Add("p0", adminId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            string jti = r["JTI"].ToString();
                            string channel = r["CLIENT_TYPE"].ToString();
                            string sessId = r["SESSION_ID"].ToString();
                            bool vCurrent = authSec.ValidateSession(jti, tokenVersion, adminId, channel);
                            bool vOld = authSec.ValidateSession(jti, 1, adminId, channel);
                            Console.WriteLine($"  Latest Session JTI={jti}, Channel={channel}:");
                            Console.WriteLine($"    - Validate with tokenVersion={tokenVersion}: {vCurrent}");
                            Console.WriteLine($"    - Validate with tokenVersion=1 (Old): {vOld}");
                        }
                    }
                }

                // 10. Check AI Scope Grants in AI_OWNER.TB_AI_SCOPE_GRANT
                Console.WriteLine("\n[AI_OWNER.TB_AI_SCOPE_GRANT CHECK]:");
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM AI_OWNER.TB_AI_SCOPE_GRANT WHERE SUBJECT_TYPE = 'USER' AND SUBJECT_ID = :p0";
                        cmd.Parameters.Add("p0", adminId);
                        int gCount = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine($"  Admin direct AI scope grants: {gCount}");
                    }
                }
                catch (Exception gex)
                {
                    Console.WriteLine($"  Cannot query AI_OWNER.TB_AI_SCOPE_GRANT: {gex.Message}");
                }
            }
        }

        [Test]
        public async Task Test_Desktop_Token_Exchange_And_Get_Ai_Config()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("=== TESTING LIVE DESKTOP LOGIN & GET_AI_CONFIG FLOW ===");
            Console.WriteLine("================================================================================");

            // 1. Perform login via AuthSecurityService (Desktop client)
            var authSec = new Bu.CLASS_SECURITY.AuthSecurityService();
            var loginResult = await authSec.AuthenticateAsync("admin", "123", "DESKTOP", "WINDOWS", "dev-1", "PC-Test", "127.0.0.1", "TestClient/1.0", Guid.NewGuid().ToString("N"));
            Console.WriteLine($"[1. LOGIN] Success: {loginResult.Success}, SessionId: {loginResult.SessionId}, JTI: {loginResult.Jti}, TokenVersion: {loginResult.TokenVersion}, Failure: {loginResult.FailureReason}");

            if (!loginResult.Success)
            {
                // Try with different password if 123 failed
                loginResult = await authSec.AuthenticateAsync("admin", "admin", "DESKTOP", "WINDOWS", "dev-1", "PC-Test", "127.0.0.1", "TestClient/1.0", Guid.NewGuid().ToString("N"));
                Console.WriteLine($"[1b. LOGIN with 'admin'] Success: {loginResult.Success}, SessionId: {loginResult.SessionId}, JTI: {loginResult.Jti}, TokenVersion: {loginResult.TokenVersion}, Failure: {loginResult.FailureReason}");
            }

            Assert.IsTrue(loginResult.Success, "Login failed: " + loginResult.ErrorMessage);

            // 2. Test ExchangeDesktopTokenAsync with the live API on http://localhost:55463
            using (var http = new System.Net.Http.HttpClient())
            {
                var exchangeBody = new
                {
                    username = "admin",
                    password = "123",
                    sessionId = loginResult.SessionId,
                    jti = loginResult.Jti
                };
                var jsonContent = new System.Net.Http.StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(exchangeBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var exchResp = await http.PostAsync("http://localhost:55463/api/auth/desktop-token", jsonContent);
                string exchJson = await exchResp.Content.ReadAsStringAsync();
                Console.WriteLine($"[2. DESKTOP-TOKEN API] Status: {exchResp.StatusCode}, Response: {exchJson}");

                Assert.IsTrue(exchResp.IsSuccessStatusCode, "Exchange desktop token failed: " + exchJson);

                dynamic exchObj = Newtonsoft.Json.JsonConvert.DeserializeObject(exchJson);
                string token = exchObj?.token?.ToString() ?? exchObj?.Token?.ToString();
                Assert.IsNotNull(token, "Token is null in exchange response");

                // 3. Call GET /api/ai/config using this token
                var configReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "http://localhost:55463/api/ai/config");
                configReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var configResp = await http.SendAsync(configReq);
                string configJson = await configResp.Content.ReadAsStringAsync();
                Console.WriteLine($"[3. GET /api/ai/config] Status: {configResp.StatusCode}, Response: {configJson}");

                Assert.IsTrue(configResp.IsSuccessStatusCode, "GET /api/ai/config failed: " + configJson);

                // 4. Call POST /api/ai/config/test
                var testBody = new
                {
                    Target = "OLLAMA",
                    Url = "http://localhost:11434",
                    Model = "qwen2.5:latest"
                };
                var testReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://localhost:55463/api/ai/config/test")
                {
                    Content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(testBody), System.Text.Encoding.UTF8, "application/json")
                };
                testReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var testResp = await http.SendAsync(testReq);
                string testJson = await testResp.Content.ReadAsStringAsync();
                Console.WriteLine($"[4. POST /api/ai/config/test] Status: {testResp.StatusCode}, Response: {testJson}");

                // Also call TestTargetEndpointAsync directly in process
                var directResult = await Bu.Services.AI_Services.Core.AiConfigurationCoordinator.Instance.TestTargetEndpointAsync("OLLAMA", "http://localhost:11434", "qwen2.5:latest");
                Console.WriteLine($"[4b. Direct TestTargetEndpointAsync OLLAMA] Success: {directResult.Success}, Message: {directResult.Message}, Latency: {directResult.LatencyMs}ms");

                var qdrantLocal = await Bu.Services.AI_Services.Core.AiConfigurationCoordinator.Instance.TestTargetEndpointAsync("QDRANT", "http://localhost:6333");
                Console.WriteLine($"[4c. Qdrant localhost:6333] Success: {qdrantLocal.Success}, Message: {qdrantLocal.Message}, Latency: {qdrantLocal.LatencyMs}ms");

                // Update TB_CONFIG in Oracle to use localhost:6333 for QdrantUrl
                using (var orclConn = new OracleConnection(ConnectionString))
                {
                    orclConn.Open();
                    using (var cmd = orclConn.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE HR.TB_CONFIG SET VALUE = 'http://localhost:6333' WHERE NAME = 'QdrantUrl'";
                        int updated = cmd.ExecuteNonQuery();
                        Console.WriteLine($"[5. DB UPDATE] Updated QdrantUrl to http://localhost:6333: {updated} row(s)");
                    }
                }

                // 6. Test Qdrant via POST /api/ai/config/test
                var qdrantTestBody = new
                {
                    Target = "QDRANT",
                    Url = "http://localhost:6333"
                };
                var qdrantReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://localhost:55463/api/ai/config/test")
                {
                    Content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(qdrantTestBody), System.Text.Encoding.UTF8, "application/json")
                };
                qdrantReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var qdrantResp = await http.SendAsync(qdrantReq);
                string qdrantJson = await qdrantResp.Content.ReadAsStringAsync();
                Console.WriteLine($"[6. POST /api/ai/config/test QDRANT] Status: {qdrantResp.StatusCode}, Response: {qdrantJson}");

                Assert.IsTrue(qdrantResp.IsSuccessStatusCode, "Qdrant test endpoint failed: " + qdrantJson);
            }
        }
    }
}
