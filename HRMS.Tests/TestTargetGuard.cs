using System;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Net;
using DA;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace HRMS.Tests
{
    /// <summary>
    /// Fail-closed security guard to ensure test suites NEVER mutate the live application database (HR schema)
    /// or live application services unintentionally.
    /// </summary>
    public static class TestTargetGuard
    {
        public const string DefaultLiveSchema = "HR";
        public const string DefaultExpectedTestSchema = "HR_TEST";
        public const string DefaultTestApiUrl = "http://localhost:55463";

        /// <summary>
        /// Extracts the Oracle User ID / Schema from a connection string.
        /// </summary>
        public static string ExtractUserId(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return string.Empty;

            try
            {
                string providerConnStr = connectionString;
                if (connectionString.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
                    if (builder.ContainsKey("provider connection string"))
                    {
                        providerConnStr = builder["provider connection string"] as string ?? connectionString;
                    }
                }

                var oracleBuilder = new OracleConnectionStringBuilder(providerConnStr);
                return oracleBuilder.UserID?.Trim().ToUpperInvariant() ?? string.Empty;
            }
            catch
            {
                // Fallback manual regex parse if builder fails
                var match = System.Text.RegularExpressions.Regex.Match(
                    connectionString,
                    @"(?:USER\s*ID|UID)\s*=\s*([a-zA-Z0-9_]+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                return match.Success ? match.Groups[1].Value.Trim().ToUpperInvariant() : string.Empty;
            }
        }

        /// <summary>
        /// Reads the currently configured connection string for MyEntities.
        /// </summary>
        public static string GetActiveConnectionString()
        {
            try
            {
                var entry = ConfigurationManager.ConnectionStrings["MyEntities"];
                return entry?.ConnectionString ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convenience property returning true only if database mutation is safe and allowed.
        /// </summary>
        public static bool IsSafeForDatabaseMutation => CheckSafeForDatabaseMutation(out _);

        /// <summary>
        /// Evaluates safety logic with explicit parameters for unit testing.
        /// </summary>
        public static bool EvaluateSafety(string connectionString, bool allowFlag, string liveSchema, string expectedTestSchema)
        {
            if (!allowFlag) return false;
            string currentUserId = ExtractUserId(connectionString);
            string live = (liveSchema ?? DefaultLiveSchema).Trim().ToUpperInvariant();
            string expected = (expectedTestSchema ?? DefaultExpectedTestSchema).Trim().ToUpperInvariant();

            if (string.Equals(currentUserId, live, StringComparison.OrdinalIgnoreCase)) return false;
            return string.Equals(currentUserId, expected, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the current test execution is connected to an isolated test database and has explicit mutation opt-in.
        /// Fails closed if pointing to live 'HR' schema or missing dual opt-in flags.
        /// </summary>
        public static bool CheckSafeForDatabaseMutation(out string failureReason)
        {
            // 1. Dual Opt-in verification: An isolated test environment flag AND explicit write permission flag must be present.
            string envOptIn = Environment.GetEnvironmentVariable("HRMS_ALLOW_TEST_DB_WRITE");
            string envIsolatedSchema = Environment.GetEnvironmentVariable("HRMS_ISOLATED_TEST_SCHEMA");
            string configOptIn = ConfigurationManager.AppSettings["AllowTestDatabaseMutation"];

            bool hasEnvWriteFlag = string.Equals(envOptIn, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(envOptIn, "1", StringComparison.OrdinalIgnoreCase);
            bool hasConfigWriteFlag = string.Equals(configOptIn, "true", StringComparison.OrdinalIgnoreCase);
            bool hasIsolatedSchemaFlag = !string.IsNullOrWhiteSpace(envIsolatedSchema);

            if (!hasEnvWriteFlag && !hasConfigWriteFlag)
            {
                failureReason = "Database mutation is not opted in. (Set HRMS_ALLOW_TEST_DB_WRITE=true and point to isolated test schema to enable).";
                return false;
            }

            // 2. Active schema inspection
            string connStr = GetActiveConnectionString();
            string currentUserId = ExtractUserId(connStr);
            string liveSchema = (ConfigurationManager.AppSettings["LiveAppSchema"] ?? DefaultLiveSchema).Trim().ToUpperInvariant();
            string expectedTestSchema = (ConfigurationManager.AppSettings["ExpectedTestSchema"] ?? DefaultExpectedTestSchema).Trim().ToUpperInvariant();

            // Fail-closed rule: If user ID matches live schema ('HR'), strictly deny mutation!
            if (string.Equals(currentUserId, liveSchema, StringComparison.OrdinalIgnoreCase))
            {
                failureReason = $"Target database user '{currentUserId}' matches live application schema '{liveSchema}'. All test mutations are strictly blocked to protect application state.";
                return false;
            }

            // Must match expected test schema
            if (!string.Equals(currentUserId, expectedTestSchema, StringComparison.OrdinalIgnoreCase) &&
                (!hasIsolatedSchemaFlag || !string.Equals(currentUserId, envIsolatedSchema.Trim().ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)))
            {
                failureReason = $"Target database user '{currentUserId}' does not match expected isolated test schema '{expectedTestSchema}'.";
                return false;
            }

            failureReason = "Target verified as isolated test schema.";
            return true;
        }

        /// <summary>
        /// Guards a test method: Skips execution cleanly if the target database is not an isolated test schema.
        /// </summary>
        public static void EnsureSafeToMutate(string purpose = null)
        {
            if (!CheckSafeForDatabaseMutation(out string reason))
            {
                Assert.Ignore($"[TestTargetGuard] Skipped '{(purpose ?? "Database Mutation Test")}': {reason}");
            }
        }

        /// <summary>
        /// Verifies whether a live HTTP API test endpoint is safe and available.
        /// Fails closed unless the test runner explicitly enables HTTP integration tests.
        /// </summary>
        public static bool IsLiveTestApiAvailable(string baseUrl = DefaultTestApiUrl)
        {
            string envHttpOptIn = Environment.GetEnvironmentVariable("HRMS_ALLOW_HTTP_INTEGRATION_TESTS");
            string configHttpOptIn = ConfigurationManager.AppSettings["AllowHttpIntegrationTests"];

            bool isOptedIn = string.Equals(envHttpOptIn, "true", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(envHttpOptIn, "1", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(configHttpOptIn, "true", StringComparison.OrdinalIgnoreCase);

            if (!isOptedIn) return false;

            try
            {
                var req = (HttpWebRequest)WebRequest.Create((baseUrl ?? DefaultTestApiUrl).TrimEnd('/') + "/api/auth/readiness");
                req.Timeout = 2000;
                req.Method = "GET";
                req.UserAgent = "HRMS-IntegrationTest-Preflight/1.0";
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    return resp.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Guards a live HTTP integration test method: Skips execution cleanly if the test API is not configured or not running.
        /// </summary>
        public static void EnsureLiveTestApiAvailable(string baseUrl = DefaultTestApiUrl)
        {
            if (!IsLiveTestApiAvailable(baseUrl))
            {
                Assert.Ignore($"[TestTargetGuard] Skipped live HTTP test: API at '{baseUrl}' is not preflight-ready or HRMS_ALLOW_HTTP_INTEGRATION_TESTS is not opted in.");
            }
        }

        /// <summary>
        /// Resolves the root ADMIN User ID dynamically from the database, avoiding hardcoded IDs (1, 80, 89).
        /// </summary>
        public static decimal ResolveAdminUserId(MyEntities db = null)
        {
            try
            {
                if (db != null)
                {
                    var id = db.Database.SqlQuery<decimal?>("SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1").FirstOrDefault();
                    if (id.HasValue && id.Value > 0) return id.Value;
                }
                else
                {
                    using (var localDb = new MyEntities())
                    {
                        var id = localDb.Database.SqlQuery<decimal?>("SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1").FirstOrDefault();
                        if (id.HasValue && id.Value > 0) return id.Value;
                    }
                }
            }
            catch { }
            return 80; // Fallback to current snapshot if DB is completely offline
        }
    }
}
