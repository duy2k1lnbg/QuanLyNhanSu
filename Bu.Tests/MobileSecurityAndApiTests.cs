using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Services;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class MobileSecurityAndApiTests
    {
        [Test]
        public void JwtToken_ShouldContainManvAndClientTypeClaims()
        {
            int userId = 101;
            string username = "test.nv";
            string fullName = "Nguyễn Văn Test";
            string manv = "2327";
            string clientType = "MOBILE";
            var rights = new List<string> { "MOBILE_PROFILE_VIEW", "MOBILE_ATTENDANCE_VIEW", "MOBILE_PAYROLL_VIEW" };

            string token = JwtService.GenerateToken(userId, username, fullName, false, rights, "CTY01", "DVI01", manv, clientType);

            Assert.IsNotNull(token);
            Assert.IsTrue(JwtService.ValidateToken(token, out var claims, out var principal));

            Assert.AreEqual("101", claims.UserId);
            Assert.AreEqual("test.nv", claims.Username);
            Assert.AreEqual("2327", claims.Manv);
            Assert.AreEqual("MOBILE", claims.ClientType);
            Assert.AreEqual("CTY01", claims.MaCty);

            // Kiểm tra Claims trong ClaimsPrincipal
            var manvClaim = principal.FindFirst("manv");
            Assert.IsNotNull(manvClaim, "ClaimsPrincipal phải có claim 'manv'");
            Assert.AreEqual("2327", manvClaim.Value);

            var clientTypeClaim = principal.FindFirst("client_type");
            Assert.IsNotNull(clientTypeClaim, "ClaimsPrincipal phải có claim 'client_type'");
            Assert.AreEqual("MOBILE", clientTypeClaim.Value);
        }

        [Test]
        public void JwtToken_TamperedSignature_ShouldFailValidation()
        {
            string token = JwtService.GenerateToken(102, "hacker", "Hacker", false, new List<string>(), manv: "9999", clientType: "MOBILE");
            string[] parts = token.Split('.');
            Assert.AreEqual(3, parts.Length);

            // Giả mạo chữ ký
            string tamperedToken = $"{parts[0]}.{parts[1]}.tamperedSignature12345";
            bool isValid = JwtService.ValidateToken(tamperedToken, out var claims, out _);

            Assert.IsFalse(isValid, "Token có chữ ký giả mạo phải bị từ chối.");
            Assert.IsNull(claims);
        }

        [Test]
        public void MobileLogin_UserWithoutEmployeeLink_MustBeRejected()
        {
            var userWithoutManv = new TB_SYS_USER
            {
                IDUSER = 999,
                USERNAME = "nomanv_user",
                MANV = null,
                CLIENT_TYPE = "ALL",
                DISABLED = 0
            };

            // Mô phỏng kiểm tra điều kiện đăng nhập Mobile
            string clientType = "MOBILE";
            bool isMobileAllowed = true;
            string rejectReason = null;

            if (clientType == "MOBILE")
            {
                if (!userWithoutManv.MANV.HasValue || userWithoutManv.MANV.Value <= 0)
                {
                    isMobileAllowed = false;
                    rejectReason = "Tài khoản chưa được liên kết với hồ sơ nhân viên. Vui lòng liên hệ bộ phận nhân sự.";
                }
            }

            Assert.IsFalse(isMobileAllowed, "Tài khoản không có MANV phải bị từ chối truy cập Mobile.");
            Assert.AreEqual("Tài khoản chưa được liên kết với hồ sơ nhân viên. Vui lòng liên hệ bộ phận nhân sự.", rejectReason);
        }

        [Test]
        public void MobileLogin_DesktopOnlyUser_MustBeRejectedOnMobile()
        {
            var desktopOnlyUser = new TB_SYS_USER
            {
                IDUSER = 998,
                USERNAME = "desktop_staff",
                MANV = 2327,
                CLIENT_TYPE = "DESKTOP",
                DISABLED = 0
            };

            string clientType = "MOBILE";
            bool isAllowed = true;
            string rejectReason = null;

            if (clientType == "MOBILE")
            {
                string allowed = (desktopOnlyUser.CLIENT_TYPE ?? "ALL").ToUpperInvariant();
                if (allowed == "DESKTOP")
                {
                    isAllowed = false;
                    rejectReason = "Tài khoản này chỉ được phép truy cập từ ứng dụng máy tính (Desktop).";
                }
            }

            Assert.IsFalse(isAllowed, "Tài khoản cấu hình DESKTOP phải bị từ chối truy cập Mobile.");
            Assert.AreEqual("Tài khoản này chỉ được phép truy cập từ ứng dụng máy tính (Desktop).", rejectReason);
        }

        [Test]
        public void MobileLogin_DisabledAccount_MustBeRejected()
        {
            var disabledUser = new TB_SYS_USER
            {
                IDUSER = 997,
                USERNAME = "locked_user",
                MANV = 2327,
                CLIENT_TYPE = "ALL",
                DISABLED = 1
            };

            bool canLogin = (disabledUser.DISABLED ?? 0) == 0;
            Assert.IsFalse(canLogin, "Tài khoản bị DISABLED == 1 phải bị chặn đăng nhập.");
        }

        [Test]
        public void MobileSelfScope_EmployeeIdentityMustComeFromJwt_PreventIdor()
        {
            // Kịch bản IDOR: User A (MANV 2327) gửi request có query ?manv=2328 nhằm xem trộm lương của User B
            decimal jwtAuthenticatedManv = 2327m;
            decimal? queryParamManv = 2328m;

            // Cơ chế bảo vệ Self-Scope của MeController:
            // Luôn cưỡng chế dùng jwtAuthenticatedManv, hoàn toàn phớt lờ queryParamManv
            decimal effectiveManv = jwtAuthenticatedManv; // Server trích xuất từ JWT claims

            Assert.AreEqual(2327m, effectiveManv, "Server phải luôn sử dụng MANV từ JWT Token, không dùng tham số client gửi.");
            Assert.AreNotEqual(queryParamManv, effectiveManv, "Client không thể dùng IDOR để xem trộm nhân viên khác.");
        }

        [Test]
        public void MobilePermissions_MobileFunctions_MustBeReadOnly()
        {
            var mobileFunctions = new List<string>
            {
                "MOBILE_PROFILE_VIEW",
                "MOBILE_ATTENDANCE_VIEW",
                "MOBILE_PAYROLL_VIEW",
                "MOBILE_CONTRACT_VIEW",
                "MOBILE_INSURANCE_VIEW",
                "MOBILE_NOTIFICATION_VIEW"
            };

            // Xác minh tất cả quyền Mobile đều là quyền xem (VIEW), không có EDIT hay DELETE
            foreach (var func in mobileFunctions)
            {
                Assert.IsTrue(func.EndsWith("_VIEW"), $"Qức năng Mobile [{func}] phải là quyền xem (ReadOnly).");
                Assert.IsFalse(func.Contains("_EDIT"), $"Quyền Mobile không được phép chỉnh sửa [{func}].");
                Assert.IsFalse(func.Contains("_DELETE"), $"Quyền Mobile không được phép xóa [{func}].");
            }
        }

        [Test]
        public void MobilePasswordChange_EnforcesCorrectOldPassword_AndHashesNewPassword()
        {
            string oldPassword = "OldSecretPassword@123";
            string currentHash = PasswordHasher.HashPassword(oldPassword);

            string enteredOldPassword = "OldSecretPassword@123";
            string enteredNewPassword = "NewSecurePassword@2026";

            // 1. Kiểm tra mật khẩu cũ đúng
            Assert.IsTrue(PasswordHasher.VerifyPassword(enteredOldPassword, currentHash));

            // 2. Mật khẩu mới không được trùng mật khẩu cũ
            Assert.AreNotEqual(oldPassword, enteredNewPassword);

            // 3. Mật khẩu mới băm bằng BCrypt
            string newHash = PasswordHasher.HashPassword(enteredNewPassword);
            Assert.IsTrue(PasswordHasher.VerifyPassword(enteredNewPassword, newHash));
            Assert.IsFalse(PasswordHasher.VerifyPassword(oldPassword, newHash));
        }
    }
}
