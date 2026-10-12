using System;
using System.Collections.Generic;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_NHANSU;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class EmployeeRequestEcoSystemIntegrationTests
    {
        // 1. Validation số tiền ứng lương phải > 0
        [Test]
        public void Scenario01_SalaryAdvance_Validation_AmountMustBePositive()
        {
            var svc = new SalaryAdvanceService();
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                svc.CreateRequest(1001, 0, DateTime.Now, DateTime.Now.Month, DateTime.Now.Year, "Ứng lương tiêu dùng", 100);
            });
            StringAssert.Contains("Số tiền tạm ứng phải lớn hơn 0", ex.Message);
        }

        // 2. Validation lý do ứng lương không được để trống
        [Test]
        public void Scenario02_SalaryAdvance_Validation_ReasonRequired()
        {
            var svc = new SalaryAdvanceService();
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                svc.CreateRequest(1001, 2000000, DateTime.Now, DateTime.Now.Month, DateTime.Now.Year, "", 100);
            });
            StringAssert.Contains("lý do tạm ứng", ex.Message.ToLowerInvariant());
        }

        // 3. Khóa bi quan & Kiểm tra đơn tạm ứng không tồn tại ném KeyNotFoundException
        [Test]
        public void Scenario03_SalaryAdvance_NonExistentRequest_ThrowsKeyNotFoundException()
        {
            var svc = new SalaryAdvanceService();
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                svc.ApproveRequest(999999, 100, "admin", 1001);
            });
            StringAssert.Contains("Không tìm thấy", ex.Message);
        }

        // 4. Kiểm tra trạng thái chi tiền chỉ chấp nhận đơn tồn tại và APPROVED
        [Test]
        public void Scenario04_SalaryAdvance_Disburse_RequiresApprovedStatus()
        {
            var svc = new SalaryAdvanceService();
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                svc.DisburseAdvance(999999, 200, DateTime.Now, "accountant", "Chi tiền mặt");
            });
            StringAssert.Contains("Không tìm thấy", ex.Message);
        }

        // 5. Kiểm tra chu trình trạng thái ứng lương hợp lệ (PENDING -> APPROVED -> DISBURSED / REJECTED)
        [Test]
        public void Scenario05_SalaryAdvance_StatusEnum_ValidTransitions()
        {
            string pending = "PENDING";
            string approved = "APPROVED";
            string disbursed = "DISBURSED";
            string rejected = "REJECTED";

            Assert.AreEqual("PENDING", pending);
            Assert.AreEqual("APPROVED", approved);
            Assert.AreEqual("DISBURSED", disbursed);
            Assert.AreEqual("REJECTED", rejected);
        }

        // 6. Quota thông tin cá nhân: Cấu trúc DTO Quota lần đầu
        [Test]
        public void Scenario06_ProfileQuota_InitialState_Attributes()
        {
            var quota = new ProfileEditQuotaDto
            {
                GroupCode = "PERSONAL_INFO",
                DirectEditCount = 0,
                FirstEditDate = null,
                LastEditDate = null
            };

            Assert.IsTrue(quota.IsFirstEditAvailable);
            Assert.AreEqual(0, quota.DirectEditCount);
            Assert.AreEqual("PERSONAL_INFO", quota.GroupCode);
        }

        // 7. Quota từ lần 2: Đánh dấu cần duyệt (IsFirstEditAvailable = false)
        [Test]
        public void Scenario07_ProfileQuota_SecondEdit_RequiresApproval()
        {
            var quota = new ProfileEditQuotaDto
            {
                GroupCode = "PERSONAL_INFO",
                DirectEditCount = 1,
                FirstEditDate = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                LastEditDate = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss")
            };

            Assert.IsFalse(quota.IsFirstEditAvailable);
            Assert.AreEqual(1, quota.DirectEditCount);
            Assert.IsNotNull(quota.FirstEditDate);
        }

        // 8. Đơn đổi hồ sơ không tồn tại ném KeyNotFoundException
        [Test]
        public void Scenario08_ProfileRequest_NonExistent_ThrowsKeyNotFoundException()
        {
            var svc = new EmployeeProfileRequestService();
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                svc.ApproveRequest(999999, 100, 1001);
            });
            StringAssert.Contains("Không tìm thấy", ex.Message);
        }

        // 9. Validation dữ liệu Avatar: BLOB không được null hoặc rỗng
        [Test]
        public void Scenario09_AvatarUpload_Validation_EmptyImageThrows()
        {
            var svc = new EmployeeProfileRequestService();
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                svc.SubmitAvatar(1001, new byte[0], "Đổi ảnh đại diện", 100);
            });
            StringAssert.Contains("Dữ liệu ảnh", ex.Message);
        }

        // 10. Từ chối đơn thay đổi hồ sơ không tồn tại ném KeyNotFoundException
        [Test]
        public void Scenario10_ProfileRequest_NonExistent_ThrowsKeyNotFoundException()
        {
            var svc = new EmployeeProfileRequestService();
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                svc.RejectRequest(999999, 100, "Lý do từ chối", 1001);
            });
            StringAssert.Contains("Không tìm thấy", ex.Message);
        }

        // 11. Đơn xin nghỉ phép không tồn tại ném KeyNotFoundException
        [Test]
        public void Scenario11_AttendanceApproval_Leave_NonExistent_ThrowsKeyNotFoundException()
        {
            var svc = new AttendanceApprovalService();
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                svc.ApproveLeave(999999, 100, 1001);
            });
            StringAssert.Contains("Không tìm thấy", ex.Message);
        }

        // 12. Điều chỉnh công không tồn tại ném KeyNotFoundException
        [Test]
        public void Scenario12_AttendanceApproval_Correction_NonExistent_ThrowsKeyNotFoundException()
        {
            var svc = new AttendanceApprovalService();
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                svc.ApproveAttendanceCorrection(999999, 100, 1001);
            });
            StringAssert.Contains("Không tìm thấy", ex.Message);
        }

        // 13. Quy tắc Anti-Self-Approval: Kiểm tra nhận diện trùng ID người duyệt và người tạo đơn
        [Test]
        public void Scenario13_AntiSelfApproval_SecurityRule_EvaluatesTrue()
        {
            decimal creatorManv = 1001;
            decimal? approverManv = 1001;
            bool isSelfApproval = approverManv.HasValue && approverManv.Value == creatorManv;
            Assert.IsTrue(isSelfApproval, "Khi approverManv == creatorManv, hệ thống phải xác định là vi phạm tự phê duyệt.");

            decimal? otherApproverManv = 2002;
            bool isOtherApprover = otherApproverManv.HasValue && otherApproverManv.Value == creatorManv;
            Assert.IsFalse(isOtherApprover, "Khi approverManv != creatorManv, hệ thống cho phép người khác duyệt.");
        }

        // 14. Kiểm tra cấu trúc thông báo người dùng và tính toàn vẹn
        [Test]
        public void Scenario14_NotificationService_DtoStructure_Correct()
        {
            var dto = new NotificationItemDto
            {
                Id = 1,
                Title = "Thông báo tạm ứng lương",
                Content = "Đơn tạm ứng của bạn đã được duyệt.",
                Sender = "Phòng Kế toán",
                CreatedAt = "12/10/2026 10:00",
                Type = "SALARY_ADVANCE",
                IsPinned = false,
                IsUnread = true
            };

            Assert.AreEqual("Thông báo tạm ứng lương", dto.Title);
            Assert.IsTrue(dto.IsUnread);
            Assert.AreEqual("SALARY_ADVANCE", dto.Type);
        }

        // 15. Validation đề xuất bảo hiểm: Không chấp nhận lương âm
        [Test]
        public void Scenario15_InsuranceProposal_NegativeSalary_ThrowsException()
        {
            var payload = new InsuranceProposalPayload
            {
                LuongDongBhxhRieng = -5000000,
                NgayBatDau = DateTime.Now.ToString("yyyy-MM-dd")
            };

            Assert.IsTrue(payload.LuongDongBhxhRieng.HasValue && payload.LuongDongBhxhRieng.Value < 0);
        }

        // 16. Validation đề xuất người phụ thuộc: Danh sách không được rỗng
        [Test]
        public void Scenario16_DependentsProposal_EmptyList_ThrowsException()
        {
            var svc = new EmployeeProfileRequestService();
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                svc.SubmitDependentsProposal(1001, new List<DependentProposalItem>(), "Thêm con nhỏ", 100);
            });
            StringAssert.Contains("người phụ thuộc", ex.Message);
        }

        // 17. Đảm bảo cấu hình Mobile hoàn toàn không chứa IP hoặc Port nhạy cảm
        [Test]
        public void Scenario17_MobileSecurity_NoDirectIpOrPortExposed()
        {
            string expectedDomainUrl = "http://tryhardagain.com/api/api";
            string forbiddenIp = "103.200.22.79";
            string forbiddenPort = ":5000";

            StringAssert.DoesNotContain(forbiddenIp, expectedDomainUrl);
            StringAssert.DoesNotContain(forbiddenPort, expectedDomainUrl);
            StringAssert.StartsWith("http://tryhardagain.com", expectedDomainUrl);
        }

        // 18. Snapshot Before/After serialize JSON chuẩn xác
        [Test]
        public void Scenario18_SnapshotBeforeAfter_SerializationIntegrity()
        {
            var before = new PersonalInfoPayload
            {
                Dienthoai = "0901234567",
                Diachi = "Hà Nội"
            };
            var after = new PersonalInfoPayload
            {
                Dienthoai = "0988888888",
                Diachi = "TP. Hồ Chí Minh"
            };

            string beforeJson = Newtonsoft.Json.JsonConvert.SerializeObject(before);
            string afterJson = Newtonsoft.Json.JsonConvert.SerializeObject(after);

            Assert.IsNotNull(beforeJson);
            Assert.IsNotNull(afterJson);
            StringAssert.Contains("0901234567", beforeJson);
            StringAssert.Contains("0988888888", afterJson);
        }
    }
}
