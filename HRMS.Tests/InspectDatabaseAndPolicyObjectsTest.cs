using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

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
    }
}
