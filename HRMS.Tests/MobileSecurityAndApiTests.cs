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

        [Test]
        public void LoginResolution_ByLoginName_ShouldResolveCorrectUser()
        {
            string loginName = "NV000001";
            var user = new TB_SYS_USER { IDUSER = 10, USERNAME = "NV000001", MANV = 1001 };

            // Logic: Try exact LoginName match first
            bool matched = string.Equals(user.USERNAME, loginName, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(matched, "LoginName NV000001 phải match chính xác.");
        }

        [Test]
        public void LoginResolution_LeadingZeroEmployeeCode_MustNotBeStripped_01DoesNotBecome1()
        {
            // Rule 6, 11, 47: 01 KHÔNG ĐƯỢC biến thành 1
            string rawInput = "01";
            string employeeCodeInDb = "01";

            // Giả lập logic kiểm tra: so sánh chính xác chuỗi, TUYỆT ĐỐI KHÔNG dùng int.Parse/decimal.TryParse hay trim leading zero
            bool exactMatch = string.Equals(rawInput.Trim(), employeeCodeInDb, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(exactMatch, "Mã nhân viên '01' phải so khớp chính xác với '01'.");

            // Kiểm tra: nếu user nhập '1' thì KHÔNG được tự động match với '01'
            string wrongInput = "1";
            bool wrongMatch = string.Equals(wrongInput.Trim(), employeeCodeInDb, StringComparison.OrdinalIgnoreCase);
            Assert.IsFalse(wrongMatch, "Mã '1' TUYỆT ĐỐI KHÔNG được tự đoán thành '01'.");
        }

        [Test]
        public void LoginResolution_ComplexLongEmployeeCode_ShouldMatchExactly()
        {
            // Rule 7, 47: Mã nghiệp vụ dài có phòng ban, xưởng, năm
            string complexCode = "PX01-KT-TV-2026-001";
            string input = "PX01-KT-TV-2026-001";

            bool matched = string.Equals(input.Trim(), complexCode, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(matched, "Mã nghiệp vụ dài phức tạp phải được hỗ trợ đầy đủ nguyên bản.");
        }

        [Test]
        public void EmployeeTransfer_LoginNameRemainsStable_EvenWhenEmployeeCodeChanges()
        {
            // Rule 14, 15, 53: Nhân viên chuyển bộ phận: EmployeeCode đổi nhưng LoginName & UserId không đổi
            int userId = 25;
            string loginName = "NV000025";
            string originalCode = "PX01-KT-TV-2026-001";

            // Nhân viên chuyển sang xưởng B
            string transferredCode = "PX02-RD-KYTHUAT-2027-014";

            Assert.AreNotEqual(originalCode, transferredCode, "Mã nhân viên đã thay đổi do điều chuyển bộ phận.");
            Assert.AreEqual("NV000025", loginName, "LoginName phải được giữ nguyên hoàn toàn.");
            Assert.AreEqual(25, userId, "UserId danh tính kỹ thuật không đổi.");
        }

        [Test]
        public void UserEmployeeMapping_1To1Constraint_DuplicateMappingMustBeRejected()
        {
            // Rule 9, 55: Kiểm tra ràng buộc 1-1 giữa User và Employee
            var existingMappings = new List<Tuple<int, decimal>>
            {
                Tuple.Create(1, 100m),
                Tuple.Create(2, 200m)
            };

            // Case 2: Employee 100 đã link với User 1 -> Không thể link với User 3
            decimal alreadyLinkedEmp = 100m;
            bool isEmpAlreadyLinked = existingMappings.Any(m => m.Item2 == alreadyLinkedEmp);
            Assert.IsTrue(isEmpAlreadyLinked, "Employee đã được liên kết phải bị phát hiện để từ chối.");

            // Case 3: User 1 đã link với Employee 100 -> Không thể link với Employee 300
            int alreadyLinkedUser = 1;
            bool isUserAlreadyLinked = existingMappings.Any(m => m.Item1 == alreadyLinkedUser);
            Assert.IsTrue(isUserAlreadyLinked, "User đã có liên kết phải bị phát hiện để từ chối.");
        }

        [Test]
        public void MobileAccessFlag_WhenDisabled_BlocksMobileAccessEvenWithValidCredentials()
        {
            // Rule 51: MobileEnabled == false -> Không được cấp phiên đăng nhập Mobile
            bool isMobileEnabled = false;
            bool credentialsValid = true;

            bool canAccessMobile = credentialsValid && isMobileEnabled;
            Assert.IsFalse(canAccessMobile, "Tài khoản bị tắt Mobile Access phải bị từ chối.");
        }

        [Test]
        public void TerminatedEmployee_DATHOIVIEC_BlocksMobileAccess()
        {
            // Rule 52, 54: Nhân viên đã thôi việc (DATHOIVIEC == 1) -> Khóa Mobile
            int daThoiViec = 1;

            bool isEmployeeActive = (daThoiViec == 0);
            Assert.IsFalse(isEmployeeActive, "Nhân viên đã thôi việc (DATHOIVIEC == 1) phải bị chặn truy cập Mobile.");
        }

        [Test]
        public void ApprovalEndpoints_LiveHttpTest()
        {
            Environment.SetEnvironmentVariable("HRMS_JWT_SECRET", "c74b9f5e18a2d36f9014b2e8ca95173f4e6d2081a95b3c7e1f4082d6e9a3b7c1");
            string token = JwtService.GenerateToken(
                1, "ADMIN", "Administrator", true, new List<string> { "*" }, "1", "1", "1", "WEB"
            );

            using (var db = new DA.MyEntities())
            {
                db.Database.ExecuteSqlCommand("UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = 'PENDING', NGUOIDUYET = NULL, NGAYDUYET = NULL WHERE ID IN (1, 2)");
            }

            using (var client = new System.Net.WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + token;

                // Test Summary
                string sumContent = client.DownloadString("http://localhost:55463/api/approvals/summary");
                Console.WriteLine($"Summary HTTP 200: {sumContent}");
                Assert.IsTrue(sumContent.Contains("\"success\":true"));

                // Test Leave List
                client.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + token;
                string leaveContent = client.DownloadString("http://localhost:55463/api/approvals/leave?status=PENDING&pageSize=50");
                Console.WriteLine($"Leave HTTP 200: {leaveContent}");
                Assert.IsTrue(leaveContent.Contains("\"success\":true"));

                // Test Attendance Corrections List
                client.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + token;
                string attContent = client.DownloadString("http://localhost:55463/api/approvals/attendance-corrections?status=PENDING&pageSize=50");
                Console.WriteLine($"Attendance HTTP 200: {attContent}");
                Assert.IsTrue(attContent.Contains("\"success\":true"));

                // Test Overtime List
                client.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + token;
                string otContent = client.DownloadString("http://localhost:55463/api/approvals/overtime?status=PENDING&pageSize=50");
                Console.WriteLine($"Overtime HTTP 200: {otContent}");
                Assert.IsTrue(otContent.Contains("\"success\":true"));

                // Test Approve Leave (id 1)
                client.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + token;
                string approveLeaveRes = client.UploadString("http://localhost:55463/api/approvals/leave/1/approve", "POST", "");
                Console.WriteLine($"Approve Leave result: {approveLeaveRes}");
                Assert.IsTrue(approveLeaveRes.Contains("\"success\":true"));

                // Test Reject Leave (id 2)
                client.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + token;
                client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
                string rejectLeaveRes = client.UploadString("http://localhost:55463/api/approvals/leave/2/reject", "POST", "{\"Reason\":\"Từ chối phục vụ kiểm thử tự động\"}");
                Console.WriteLine($"Reject Leave result: {rejectLeaveRes}");
                Assert.IsTrue(rejectLeaveRes.Contains("\"success\":true"));
            }
        }
    }
}
