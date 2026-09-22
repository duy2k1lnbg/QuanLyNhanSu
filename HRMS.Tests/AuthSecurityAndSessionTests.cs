using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Services;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Tests
{
    [TestFixture]
    public class AuthSecurityAndSessionTests
    {
        private IAuthSecurityService _authSecurityService;
        private ISessionPolicyService _policyService;
        private IAuthAuditService _auditService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _authSecurityService = new AuthSecurityService();
            _policyService = new SessionPolicyService();
            _auditService = new AuthAuditService();
        }

        [Test]
        public void Test01_JwtService_GeneratesAndValidates_JtiAndTokenVersion()
        {
            int userId = 1;
            string username = "admin";
            string fullName = "System Administrator";
            bool isAdmin = true;
            var rights = new List<string> { "*", "F_NHANSU_VIEW" };
            string jti = Guid.NewGuid().ToString("N");
            long tokenVersion = 5;

            string token = JwtService.GenerateToken(
                userId, username, fullName, isAdmin, rights,
                maCty: "CTY01", maDvi: "DVI01", manv: "1", clientType: "ALL",
                jti: jti, tokenVersion: tokenVersion
            );

            Assert.IsNotNull(token);
            Assert.IsTrue(token.Split('.').Length == 3, "JWT must consist of 3 parts: header.payload.signature");

            bool isValid = JwtService.ValidateToken(token, out var claims, out var principal);
            Assert.IsTrue(isValid, "Token signature and exp must validate");
            Assert.IsNotNull(claims);
            Assert.AreEqual(userId.ToString(), claims.UserId);
            Assert.AreEqual(username, claims.Username);
            Assert.AreEqual(jti, claims.Jti, "JTI must match generated ID");
            Assert.AreEqual(tokenVersion, claims.TokenVersion, "Token version must match");
            Assert.AreEqual("HRMS.Api", claims.Issuer);
            Assert.AreEqual("HRMS.Clients", claims.Audience);
            Assert.IsTrue(claims.IsAdmin);
        }

        [Test]
        public void Test02_JwtService_RejectsTamperedToken()
        {
            string token = JwtService.GenerateToken(1, "admin", "Admin", true, new List<string> { "*" });
            string[] parts = token.Split('.');

            // Tamper with payload
            string tamperedPayload = parts[1].Substring(0, parts[1].Length - 4) + "AAAA";
            string tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

            bool isValid = JwtService.ValidateToken(tamperedToken, out _, out _);
            Assert.IsFalse(isValid, "Tampered token must fail HMAC-SHA256 signature verification");
        }

        [Test]
        public void Test03_PasswordHasher_BCryptVerification()
        {
            string rawPassword = "SecurePassword@2026";
            string hash = PasswordHasher.HashPassword(rawPassword);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"), "Hash must follow BCrypt standard");

            bool isMatch = PasswordHasher.VerifyPassword(rawPassword, hash);
            Assert.IsTrue(isMatch, "Correct password must verify successfully");

            bool isWrong = PasswordHasher.VerifyPassword("WrongPassword", hash);
            Assert.IsFalse(isWrong, "Incorrect password must be rejected");
        }

        [Test]
        public async Task Test04_PolicyService_ResolvesHierarchicalLimits()
        {
            // Admin role should resolve to 4 max sessions by default seed
            var adminPolicy = await _policyService.GetEffectivePolicyAsync(1, "admin", true);
            Assert.IsNotNull(adminPolicy);
            Assert.AreEqual(4, adminPolicy.MAX_ACTIVE_SESSIONS, "Admin policy must allow 4 concurrent sessions");
            Assert.AreEqual(SessionLimitStrategies.RevokeOldest, adminPolicy.SESSION_LIMIT_STRATEGY);

            // Regular user should resolve to global policy (2 max sessions)
            var userPolicy = await _policyService.GetEffectivePolicyAsync(99999, "regular_user", false);
            Assert.IsNotNull(userPolicy);
            Assert.AreEqual(2, userPolicy.MAX_ACTIVE_SESSIONS, "Standard user policy must allow 2 concurrent sessions");
        }

        [Test]
        public async Task Test05_Authentication_SuccessAndSessionCreation()
        {
            // Seed / ensure a dedicated test user exists in DB
            decimal testUserId = 88881;
            string testUsername = "authtest_user";
            string rawPassword = "TestPassword@123";
            string hashedPassword = PasswordHasher.HashPassword(rawPassword);

            using (var db = new MyEntities())
            {
                db.Database.ExecuteSqlCommand(@"
                    MERGE INTO HR.TB_SYS_USER t
                    USING (SELECT :p0 AS IDUSER FROM DUAL) s
                    ON (t.IDUSER = s.IDUSER)
                    WHEN MATCHED THEN
                        UPDATE SET USERNAME = :p1, PASSWORD = :p2, DISABLED = 0, FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL, TOKEN_VERSION = 1
                    WHEN NOT MATCHED THEN
                        INSERT (IDUSER, USERNAME, PASSWORD, FULLNAME, DISABLED, ISGROUP, CLIENT_TYPE, FAILED_LOGIN_COUNT, TOKEN_VERSION)
                        VALUES (:p0, :p1, :p2, 'Auth Test User', 0, 0, 'ALL', 0, 1)",
                    new OracleParameter("p0", testUserId),
                    new OracleParameter("p1", testUsername),
                    new OracleParameter("p2", hashedPassword)
                );

                // Clean up any existing sessions
                db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_AUTH_SESSION WHERE USER_ID = :p0", new OracleParameter("p0", testUserId));
            }

            // 1. First Login -> 1 active session
            var login1 = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_1", "Test Browser 1", "127.0.0.1", "TestAgent", "corr_1"
            );
            Assert.IsTrue(login1.Success, "Login 1 must succeed: " + login1.ErrorMessage);
            Assert.IsNotNull(login1.SessionId);
            Assert.IsNotNull(login1.Jti);

            // Validate session 1 in DB
            bool isValid1 = _authSecurityService.ValidateSession(login1.Jti, login1.TokenVersion, testUserId);
            Assert.IsTrue(isValid1, "Session 1 must be valid");

            // 2. Second Login -> 2 active sessions
            var login2 = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_2", "Test Browser 2", "127.0.0.1", "TestAgent", "corr_2"
            );
            Assert.IsTrue(login2.Success, "Login 2 must succeed");
            bool isValid2 = _authSecurityService.ValidateSession(login2.Jti, login2.TokenVersion, testUserId);
            Assert.IsTrue(isValid2, "Session 2 must be valid");

            // 3. Third Login -> Exceeds max active sessions (2) -> Revoke Oldest (Session 1 must be revoked)
            var login3 = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_3", "Test Browser 3", "127.0.0.1", "TestAgent", "corr_3"
            );
            Assert.IsTrue(login3.Success, "Login 3 must succeed");

            // Verify Session 1 is NOW IMMEDIATELY REVOKED
            bool isStillValid1 = _authSecurityService.ValidateSession(login1.Jti, login1.TokenVersion, testUserId);
            Assert.IsFalse(isStillValid1, "Session 1 must be REVOKED due to quota limit (REVOKE_OLDEST)");

            // Verify Session 2 and Session 3 are still valid
            Assert.IsTrue(_authSecurityService.ValidateSession(login2.Jti, login2.TokenVersion, testUserId), "Session 2 must remain valid");
            Assert.IsTrue(_authSecurityService.ValidateSession(login3.Jti, login3.TokenVersion, testUserId), "Session 3 must remain valid");
        }

        [Test]
        public async Task Test06_PasswordChange_RevokesAllActiveSessionsImmediately()
        {
            decimal testUserId = 88882;
            string testUsername = "pwdtest_user";
            string initialPassword = "OldPassword@123";
            string newPassword = "NewPassword@456";
            string hashedInit = PasswordHasher.HashPassword(initialPassword);

            using (var db = new MyEntities())
            {
                db.Database.ExecuteSqlCommand(@"
                    MERGE INTO HR.TB_SYS_USER t
                    USING (SELECT :p0 AS IDUSER FROM DUAL) s
                    ON (t.IDUSER = s.IDUSER)
                    WHEN MATCHED THEN
                        UPDATE SET USERNAME = :p1, PASSWORD = :p2, DISABLED = 0, FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL, TOKEN_VERSION = 1
                    WHEN NOT MATCHED THEN
                        INSERT (IDUSER, USERNAME, PASSWORD, FULLNAME, DISABLED, ISGROUP, CLIENT_TYPE, FAILED_LOGIN_COUNT, TOKEN_VERSION)
                        VALUES (:p0, :p1, :p2, 'Pwd Test User', 0, 0, 'ALL', 0, 1)",
                    new OracleParameter("p0", testUserId),
                    new OracleParameter("p1", testUsername),
                    new OracleParameter("p2", hashedInit)
                );

                db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_AUTH_SESSION WHERE USER_ID = :p0", new OracleParameter("p0", testUserId));
            }

            // Log in to get active session
            var login = await _authSecurityService.AuthenticateAsync(
                testUsername, initialPassword, "WEB", "WEB", "dev_pwd", "Browser", "127.0.0.1", "Agent", "corr_pwd"
            );
            Assert.IsTrue(login.Success);
            Assert.IsTrue(_authSecurityService.ValidateSession(login.Jti, login.TokenVersion, testUserId), "Session must be valid before password change");

            // Change Password
            var changeRes = await _authSecurityService.ChangePasswordWithRevocationAsync(
                testUserId, initialPassword, newPassword, "127.0.0.1", "Agent", "corr_change"
            );
            Assert.IsTrue(changeRes.Success, "Password change must succeed: " + changeRes.Message);
            Assert.GreaterOrEqual(changeRes.SessionsRevokedCount, 1, "At least 1 active session must be revoked");

            // Verify old session is IMMEDIATELY invalid (both by session revoked and token_version mismatch)
            bool isOldSessionValid = _authSecurityService.ValidateSession(login.Jti, login.TokenVersion, testUserId);
            Assert.IsFalse(isOldSessionValid, "Old session MUST be rejected immediately after password change");

            // Verify login with old password fails
            var failedLogin = await _authSecurityService.AuthenticateAsync(
                testUsername, initialPassword, "WEB", "WEB", "dev_pwd2", "Browser", "127.0.0.1", "Agent", "corr_old"
            );
            Assert.IsFalse(failedLogin.Success, "Login with old password must fail");

            // Verify login with new password succeeds
            var successLogin = await _authSecurityService.AuthenticateAsync(
                testUsername, newPassword, "WEB", "WEB", "dev_pwd3", "Browser", "127.0.0.1", "Agent", "corr_new"
            );
            Assert.IsTrue(successLogin.Success, "Login with new password must succeed");
            Assert.Greater(successLogin.TokenVersion, login.TokenVersion, "Token version must have incremented");
        }

        [Test]
        public async Task Test07_BruteForceProtection_LocksAccountAfterFiveFailedAttempts()
        {
            decimal testUserId = 88883;
            string testUsername = "brutetest_user";
            string rawPassword = "CorrectPassword@123";
            string hashed = PasswordHasher.HashPassword(rawPassword);

            using (var db = new MyEntities())
            {
                db.Database.ExecuteSqlCommand(@"
                    MERGE INTO HR.TB_SYS_USER t
                    USING (SELECT :p0 AS IDUSER FROM DUAL) s
                    ON (t.IDUSER = s.IDUSER)
                    WHEN MATCHED THEN
                        UPDATE SET USERNAME = :p1, PASSWORD = :p2, DISABLED = 0, FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL, TOKEN_VERSION = 1
                    WHEN NOT MATCHED THEN
                        INSERT (IDUSER, USERNAME, PASSWORD, FULLNAME, DISABLED, ISGROUP, CLIENT_TYPE, FAILED_LOGIN_COUNT, TOKEN_VERSION)
                        VALUES (:p0, :p1, :p2, 'Brute Test User', 0, 0, 'ALL', 0, 1)",
                    new OracleParameter("p0", testUserId),
                    new OracleParameter("p1", testUsername),
                    new OracleParameter("p2", hashed)
                );
            }

            // Simulate 5 wrong password attempts
            for (int i = 1; i <= 4; i++)
            {
                var failRes = await _authSecurityService.AuthenticateAsync(
                    testUsername, "WrongPass" + i, "WEB", "WEB", "dev_bf", "Browser", "127.0.0.1", "Agent", "corr_bf_" + i
                );
                Assert.IsFalse(failRes.Success);
                Assert.IsFalse(failRes.IsLockedOut, $"Attempt {i} should not be locked out yet");
            }

            // 5th attempt triggers lockout
            var fifthAttempt = await _authSecurityService.AuthenticateAsync(
                testUsername, "WrongPass5", "WEB", "WEB", "dev_bf", "Browser", "127.0.0.1", "Agent", "corr_bf_5"
            );
            Assert.IsFalse(fifthAttempt.Success);
            Assert.IsTrue(fifthAttempt.IsLockedOut, "5th attempt must trigger account lockout");
            Assert.Greater(fifthAttempt.LockoutRemainingMinutes, 0, "Lockout remaining minutes must be positive");

            // Subsequent attempt with CORRECT password is still locked out
            var blockedAttempt = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_bf", "Browser", "127.0.0.1", "Agent", "corr_bf_6"
            );
            Assert.IsFalse(blockedAttempt.Success);
            Assert.IsTrue(blockedAttempt.IsLockedOut, "Account must remain locked out even if correct password is provided");

            // Admin unlocks account
            bool unlocked = await _authSecurityService.UnlockUserAsync(testUserId, 1, "admin_corr");
            Assert.IsTrue(unlocked, "Admin unlock must succeed");

            // After unlock, login succeeds
            var afterUnlock = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_bf", "Browser", "127.0.0.1", "Agent", "corr_bf_7"
            );
            Assert.IsTrue(afterUnlock.Success, "Login must succeed after account is unlocked");
        }

        [Test]
        public async Task Test08_LogoutAndLogoutAll_Revocation()
        {
            decimal testUserId = 88884;
            string testUsername = "logouttest_user";
            string rawPassword = "Password@123";
            string hashed = PasswordHasher.HashPassword(rawPassword);

            using (var db = new MyEntities())
            {
                db.Database.ExecuteSqlCommand(@"
                    MERGE INTO HR.TB_SYS_USER t
                    USING (SELECT :p0 AS IDUSER FROM DUAL) s
                    ON (t.IDUSER = s.IDUSER)
                    WHEN MATCHED THEN
                        UPDATE SET USERNAME = :p1, PASSWORD = :p2, DISABLED = 0, FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL, TOKEN_VERSION = 1
                    WHEN NOT MATCHED THEN
                        INSERT (IDUSER, USERNAME, PASSWORD, FULLNAME, DISABLED, ISGROUP, CLIENT_TYPE, FAILED_LOGIN_COUNT, TOKEN_VERSION)
                        VALUES (:p0, :p1, :p2, 'Logout Test User', 0, 0, 'ALL', 0, 1)",
                    new OracleParameter("p0", testUserId),
                    new OracleParameter("p1", testUsername),
                    new OracleParameter("p2", hashed)
                );
                db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_AUTH_SESSION WHERE USER_ID = :p0", new OracleParameter("p0", testUserId));
            }

            // 1. Single session logout
            var login1 = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_s1", "Browser", "127.0.0.1", "Agent", "c1"
            );
            Assert.IsTrue(login1.Success);
            Assert.IsTrue(_authSecurityService.ValidateSession(login1.Jti, login1.TokenVersion, testUserId));

            bool revoked = await _authSecurityService.RevokeSessionAsync(login1.Jti, AuthRevokeReasons.Logout, testUserId, "c_rev");
            Assert.IsTrue(revoked, "Revoke session must return true");
            Assert.IsFalse(_authSecurityService.ValidateSession(login1.Jti, login1.TokenVersion, testUserId), "Revoked session must fail validation immediately");

            // 2. Logout All
            var login2 = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_s2", "Browser", "127.0.0.1", "Agent", "c2"
            );
            var login3 = await _authSecurityService.AuthenticateAsync(
                testUsername, rawPassword, "WEB", "WEB", "dev_s3", "Browser", "127.0.0.1", "Agent", "c3"
            );
            Assert.IsTrue(login2.Success && login3.Success);

            int revokedCount = await _authSecurityService.RevokeAllSessionsAsync(testUserId, AuthRevokeReasons.LogoutAll, true, testUserId, "c_all");
            Assert.GreaterOrEqual(revokedCount, 2, "Logout all must revoke both sessions");

            Assert.IsFalse(_authSecurityService.ValidateSession(login2.Jti, login2.TokenVersion, testUserId));
            Assert.IsFalse(_authSecurityService.ValidateSession(login3.Jti, login3.TokenVersion, testUserId));
        }
    }
}
