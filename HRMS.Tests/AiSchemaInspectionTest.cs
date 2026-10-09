using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiSchemaInspectionTest
    {
        private const string HrConnStr = "DATA SOURCE=localhost:1521/orcl;PASSWORD=hr;USER ID=HR";
        private const string AiReadOnlyConnStr = "DATA SOURCE=localhost:1521/orcl;PASSWORD=AI;USER ID=AI_READONLY";

        [Test]
        public void Inspect_Ai_Objects_Exact()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("=== INSPECTION FROM HR ===");
            Console.WriteLine("================================================================================");
            InspectOwner(HrConnStr, "HR");

            Console.WriteLine("\n================================================================================");
            Console.WriteLine("=== INSPECTION FROM AI_READONLY ===");
            Console.WriteLine("================================================================================");
            InspectOwner(AiReadOnlyConnStr, "AI_READONLY");
        }

        private void InspectOwner(string connStr, string user)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                Console.WriteLine($"[{user}] Connected successfully.");

                // Check all tables matching AI or owned by AI_OWNER
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT OWNER, TABLE_NAME, NUM_ROWS
                        FROM ALL_TABLES
                        WHERE TABLE_NAME LIKE '%AI%' OR OWNER = 'AI_OWNER'
                        ORDER BY OWNER, TABLE_NAME";
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine($"[{user}] ALL_TABLES matching AI or OWNER=AI_OWNER:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"  {reader["OWNER"]}.{reader["TABLE_NAME"]} (rows: {reader["NUM_ROWS"]})");
                        }
                    }
                }

                // Check privileges on TB_AI_SCOPE_GRANT and TB_AI_CAPABILITY
                using (var cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = @"
                            SELECT GRANTEE, TABLE_NAME, PRIVILEGE
                            FROM ALL_TAB_PRIVS
                            WHERE TABLE_NAME LIKE 'TB_AI%' OR TABLE_NAME LIKE 'PKG_AI%'
                            ORDER BY TABLE_NAME, GRANTEE, PRIVILEGE";
                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"\n[{user}] Privileges on TB_AI% / PKG_AI%:");
                            while (reader.Read())
                            {
                                Console.WriteLine($"  TABLE: {reader["TABLE_NAME"]} PRIV: {reader["PRIVILEGE"]} GRANTEE: {reader["GRANTEE"]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{user}] Privs check failed: {ex.Message}");
                    }
                }

                // Try SELECT on AI_OWNER.TB_AI_CAPABILITY
                using (var cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT CAPABILITY_CODE, REQUIRED_FUNCTION_CODE, IS_ENABLED FROM AI_OWNER.TB_AI_CAPABILITY ORDER BY CAPABILITY_CODE";
                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"\n[{user}] AI_OWNER.TB_AI_CAPABILITY contents:");
                            while (reader.Read())
                            {
                                Console.WriteLine($"  CAP={reader["CAPABILITY_CODE"]}, REQ={reader["REQUIRED_FUNCTION_CODE"]}, EN={reader["IS_ENABLED"]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{user}] Cannot query AI_OWNER.TB_AI_CAPABILITY: {ex.Message}");
                    }
                }

                // Try SELECT on AI_OWNER.TB_AI_SCOPE_GRANT
                using (var cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM AI_OWNER.TB_AI_SCOPE_GRANT";
                        var cnt = cmd.ExecuteScalar();
                        Console.WriteLine($"\n[{user}] AI_OWNER.TB_AI_SCOPE_GRANT count: {cnt}");

                        cmd.CommandText = "SELECT GRANT_ID, SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, SCOPE_KEY, EFFECT, IS_ENABLED, VALID_FROM, VALID_TO FROM AI_OWNER.TB_AI_SCOPE_GRANT ORDER BY GRANT_ID";
                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"[{user}] AI_OWNER.TB_AI_SCOPE_GRANT contents:");
                            while (reader.Read())
                            {
                                Console.WriteLine($"  ID={reader["GRANT_ID"]}, SUBJ={reader["SUBJECT_TYPE"]}:{reader["SUBJECT_ID"]}, CAP={reader["CAPABILITY_CODE"]}, SCOPE={reader["SCOPE_TYPE"]}({reader["SCOPE_KEY"]}), EFFECT={reader["EFFECT"]}, EN={reader["IS_ENABLED"]}, FROM={reader["VALID_FROM"]}, TO={reader["VALID_TO"]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{user}] Cannot query AI_OWNER.TB_AI_SCOPE_GRANT: {ex.Message}");
                    }
                }

                // Check TB_AI_REVISION
                using (var cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT REVISION_KEY, REVISION_NUMBER, UPDATED_AT FROM AI_OWNER.TB_AI_REVISION";
                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"\n[{user}] AI_OWNER.TB_AI_REVISION contents:");
                            while (reader.Read())
                            {
                                Console.WriteLine($"  KEY={reader["REVISION_KEY"]}, REV={reader["REVISION_NUMBER"]}, UPDATED={reader["UPDATED_AT"]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{user}] Cannot query AI_OWNER.TB_AI_REVISION: {ex.Message}");
                    }
                }
            }
        }
    }
}
