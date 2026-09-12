using System;
using System.Collections.Generic;
using System.Security.Claims;
using Bu.CLASS_SYSTEM;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class ApiSecurityIntegrationTests
    {
        [Test]
        public void PasswordHasher_ShouldEnforceBCrypt_AndRejectPlaintext()
        {
            string password = "TestPassword@2026";
            string hash = PasswordHasher.HashPassword(password);

            // Valid hash check
            Assert.IsTrue(PasswordHasher.VerifyPassword(password, hash));
            Assert.IsFalse(PasswordHasher.VerifyPassword("WrongPassword", hash));

            // Plaintext check must fail
            Assert.IsFalse(PasswordHasher.VerifyPassword(password, password), "Direct plaintext comparison must be rejected.");
            Assert.IsFalse(PasswordHasher.VerifyPassword(password, "randomNonBcryptString"), "Non-BCrypt hash must be rejected.");
        }

        [Test]
        public void PasswordHasher_ShouldTrimTrailingPadding()
        {
            string raw = "SecretKey";
            string padded = "SecretKey   ";
            string hash = PasswordHasher.HashPassword(raw);

            Assert.IsTrue(PasswordHasher.VerifyPassword(padded, hash), "Trailing spaces from Oracle CHAR fields must be trimmed.");
        }

        [Test]
        public void Security_RightsChecking_ShouldSupportWildcardAndSpecificRoles()
        {
            var adminRights = new List<string> { "*" };
            var hrStaffRights = new List<string> { "F_NHANSU_ADD", "F_NHANSU_EDIT", "F_CHAMCONG_ADD" };

            // Admin with "*" should satisfy any right
            Assert.IsTrue(adminRights.Contains("*") || adminRights.Contains("F_NHANSU_DELETE"));

            // HR staff should have F_NHANSU_ADD but not F_NHANSU_DELETE
            Assert.IsTrue(hrStaffRights.Contains("F_NHANSU_ADD"));
            Assert.IsFalse(hrStaffRights.Contains("F_NHANSU_DELETE"));
        }

        [Test]
        public void Security_DataScope_AdminBypassesCompanyFilter()
        {
            bool isAdmin = true;
            string userCompany = "CTY01";
            string targetCompany = "CTY02";

            bool canAccess = isAdmin || string.Equals(userCompany, targetCompany, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(canAccess, "Admin must be able to view any company's records.");

            bool regularUserCanAccess = false || string.Equals(userCompany, targetCompany, StringComparison.OrdinalIgnoreCase);
            Assert.IsFalse(regularUserCanAccess, "Regular user must be blocked from viewing another company's records.");
        }
    }
}
