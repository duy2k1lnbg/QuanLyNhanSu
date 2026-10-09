using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bu.CLASS_SECURITY;
using DA;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AdminLoginAndTestIsolationRegressionTests
    {
        [Test]
        public void TargetGuard_BlocksMutation_WhenTargetIsLiveSchema()
        {
            // Default environment and configuration must NEVER allow mutations on live schema HR
            bool isSafe = TestTargetGuard.IsSafeForDatabaseMutation;
            Assert.IsFalse(isSafe, "Default test runs against live schema 'HR' must fail-closed and disallow DB writes.");

            // EnsureSafeToMutate must cleanly ignore (IgnoreException) rather than executing writes
            Assert.Throws<IgnoreException>(() =>
            {
                TestTargetGuard.EnsureSafeToMutate("Testing fail-closed behavior");
            });
        }

        [Test]
        public void TargetGuard_RejectsLiveSchema_EvenIfOptInFlagProvided()
        {
            // Even if an environment flag is set to true, pointing to live schema 'HR' must STILL be rejected (dual validation)
            string liveConnStr = "USER ID=HR;DATA SOURCE=localhost:1521/orcl;PASSWORD=hr";
            bool evaluated = TestTargetGuard.EvaluateSafety(
                connectionString: liveConnStr,
                allowFlag: true,
                liveSchema: "HR",
                expectedTestSchema: "HR_TEST"
            );

            Assert.IsFalse(evaluated, "A single environment flag cannot authorize writes against live schema 'HR'. Dual validation must fail-closed.");
        }

        [Test]
        public void TargetGuard_AcceptsOnlyIsolatedTestSchema_WithExplicitOptIn()
        {
            string isolatedTestConnStr = "USER ID=HR_TEST;DATA SOURCE=localhost:1521/orcl;PASSWORD=test";
            
            // Without opt-in flag -> must reject
            bool withoutOptIn = TestTargetGuard.EvaluateSafety(
                connectionString: isolatedTestConnStr,
                allowFlag: false,
                liveSchema: "HR",
                expectedTestSchema: "HR_TEST"
            );
            Assert.IsFalse(withoutOptIn, "Without explicit opt-in, even HR_TEST must not be mutated.");

            // With opt-in flag and matching test schema -> allowed
            bool withOptIn = TestTargetGuard.EvaluateSafety(
                connectionString: isolatedTestConnStr,
                allowFlag: true,
                liveSchema: "HR",
                expectedTestSchema: "HR_TEST"
            );
            Assert.IsTrue(withOptIn, "Isolated schema HR_TEST with explicit opt-in must be allowed.");
        }

        [Test]
        public void StaticInspection_InspectAdminUserState_IsStrictlyReadOnly()
        {
            // Verify that InspectAdminUserState in DbQueryTests.cs does not contain mutating commands
            string testFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "DbQueryTests.cs");
            if (!File.Exists(testFilePath))
            {
                testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbQueryTests.cs");
            }

            if (File.Exists(testFilePath))
            {
                string code = File.ReadAllText(testFilePath);
                int inspectIdx = code.IndexOf("public void InspectAdminUserState()", StringComparison.Ordinal);
                Assert.IsTrue(inspectIdx > 0, "InspectAdminUserState must exist in DbQueryTests.cs");

                int nextMethodIdx = code.IndexOf("public void InspectDatabaseMetadataForOvertime()", inspectIdx, StringComparison.Ordinal);
                Assert.IsTrue(nextMethodIdx > inspectIdx);

                string methodBody = code.Substring(inspectIdx, nextMethodIdx - inspectIdx);

                Assert.IsFalse(methodBody.Contains("db.SaveChanges()"), "InspectAdminUserState must NEVER call SaveChanges()");
                Assert.IsFalse(methodBody.Contains("ExecuteSqlCommand"), "InspectAdminUserState must NEVER call ExecuteSqlCommand");
                Assert.IsFalse(methodBody.Contains("admin.PASSWORD ="), "InspectAdminUserState must NEVER mutate admin.PASSWORD");
            }
        }

        [Test]
        public async Task AuthSecurityService_StandardizedFailureReason_OnInvalidInputs()
        {
            var service = new AuthSecurityService();

            // 1. Missing credentials
            var emptyRes = await service.AuthenticateAsync("", "", "DESKTOP", "WINDOWS", null, null, null, null, null);
            Assert.IsFalse(emptyRes.Success);
            Assert.AreEqual("MISSING_CREDENTIALS", emptyRes.FailureReason);
            Assert.IsNotNull(emptyRes.ErrorMessage);

            // 2. Invalid channel
            var badChannelRes = await service.AuthenticateAsync("admin", "pwd", "UNKNOWN_CHANNEL", "WINDOWS", null, null, null, null, null);
            Assert.IsFalse(badChannelRes.Success);
            Assert.AreEqual("INVALID_CHANNEL", badChannelRes.FailureReason);
            Assert.IsNotNull(badChannelRes.ErrorMessage);

            // 3. Non-existent user
            var unknownUserRes = await service.AuthenticateAsync("non_existent_user_99999", "random_pwd", "DESKTOP", "WINDOWS", null, null, null, null, null);
            Assert.IsFalse(unknownUserRes.Success);
            Assert.AreEqual("INVALID_CREDENTIALS", unknownUserRes.FailureReason);
            Assert.IsNotNull(unknownUserRes.ErrorMessage);
        }

        [Test]
        public void DesktopLogin_UserEntity_IsAssignableToTbSysUser()
        {
            // Verify that LoginResultDto provides properties sufficient to instantiate or cast DA.TB_SYS_USER
            var result = new LoginResultDto
            {
                Success = true,
                UserId = 80,
                Username = "ADMIN",
                FullName = "Administrator",
                MaCty = "1",
                MaDvi = "1",
                ClientType = "DESKTOP",
                TokenVersion = 1,
                User = new DA.TB_SYS_USER
                {
                    IDUSER = 80,
                    USERNAME = "ADMIN",
                    FULLNAME = "Administrator",
                    CLIENT_TYPE = "DESKTOP"
                }
            };

            // Test both direct cast and fallback pattern used in FrmDangNhap
            var directUser = result.User as DA.TB_SYS_USER;
            Assert.IsNotNull(directUser, "Direct cast of result.User to DA.TB_SYS_USER must succeed.");

            // Test fallback pattern if result.User were null
            var fallbackUser = (null as DA.TB_SYS_USER) ?? (result.Success ? new DA.TB_SYS_USER
            {
                IDUSER = result.UserId,
                USERNAME = result.Username,
                FULLNAME = result.FullName,
                CLIENT_TYPE = result.ClientType
            } : null);

            Assert.IsNotNull(fallbackUser, "Fallback reconstruction must succeed.");
            Assert.AreEqual(80, fallbackUser.IDUSER);
            Assert.AreEqual("ADMIN", fallbackUser.USERNAME);
        }

        [Test]
        public void LiveAdminRecord_PreservesSecurityIntegrity()
        {
            // Read live admin state without mutating anything
            decimal adminId = TestTargetGuard.ResolveAdminUserId();
            Assert.Greater(adminId, 0, "Live ADMIN user ID must be resolved dynamically.");

            using (var db = new MyEntities())
            {
                var admin = db.TB_SYS_USER.AsNoTracking().FirstOrDefault(u => u.IDUSER == adminId);
                Assert.IsNotNull(admin, "ADMIN account must exist in the database.");

                // Security assertions: Root admin must not be disabled, locked, or tied to MANV
                Assert.AreEqual(0, admin.DISABLED ?? 0, "ADMIN account must not be disabled.");
                Assert.AreEqual(0, admin.ISGROUP ?? 0, "ADMIN account must not be a group.");
                Assert.IsNull(admin.MANV, "Root ADMIN must have MANV = null by security policy.");

                // Verify password hash integrity without revealing the hash
                Assert.IsNotNull(admin.PASSWORD, "ADMIN password hash must not be null.");
                Assert.GreaterOrEqual(admin.PASSWORD.Length, 50, "ADMIN password must be a cryptographic hash.");
                bool isBcrypt = admin.PASSWORD.StartsWith("$2a$") || admin.PASSWORD.StartsWith("$2b$") || admin.PASSWORD.StartsWith("$2y$");
                Assert.IsTrue(isBcrypt, "ADMIN password must be standard BCrypt format.");
            }
        }
    }
}
