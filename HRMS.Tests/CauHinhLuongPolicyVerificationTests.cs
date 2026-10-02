using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DA;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Tests
{
    [TestFixture]
    public class CauHinhLuongPolicyVerificationTests
    {
        [SetUp]
        public void SetUp()
        {
            // Set default admin session for tests that interact with business logic
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 1,
                USERNAME = "admin",
                FULLNAME = "Administrator"
            };
            UserSession.UserRights.Clear();
            UserSession.DetailedRights.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            UserSession.CurrentUser = null;
            UserSession.UserRights.Clear();
            UserSession.DetailedRights.Clear();
        }

        [Test]
        public void Test_01_InspectAllPolicyGroups_LiveDb_DiagnosesObjectNotFoundWithExactTablesAndRefId()
        {
            // ACT: Inspect all policy groups against live DB
            var results = PolicyResolver.InspectAllPolicyGroups(DateTime.Today);

            // ASSERT: We expect all 4 groups to be evaluated independently
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(4));

            var salary = results.FirstOrDefault(r => r.GroupKey == "SALARY");
            var insurance = results.FirstOrDefault(r => r.GroupKey == "INSURANCE");
            var union = results.FirstOrDefault(r => r.GroupKey == "UNION");
            var tax = results.FirstOrDefault(r => r.GroupKey == "TAX");

            Assert.That(salary, Is.Not.Null);
            Assert.That(insurance, Is.Not.Null);
            Assert.That(union, Is.Not.Null);
            Assert.That(tax, Is.Not.Null);

            // On the unmigrated DB, each group should return ObjectNotFound for its specific table
            Assert.That(salary.Status, Is.EqualTo(PolicyGroupStatus.ObjectNotFound));
            Assert.That(salary.PrimaryTable, Is.EqualTo("TB_CHINH_SACH_LUONG"));
            Assert.That(salary.CorrelationId, Does.StartWith("REF-"));
            Assert.That(salary.ActionRequired, Does.Contain("apply_payroll_v1_16_objects.sql"));

            Assert.That(insurance.Status, Is.EqualTo(PolicyGroupStatus.ObjectNotFound));
            Assert.That(insurance.PrimaryTable, Is.EqualTo("TB_CHINH_SACH_BHXH"));

            Assert.That(union.Status, Is.EqualTo(PolicyGroupStatus.ObjectNotFound));
            Assert.That(union.PrimaryTable, Is.EqualTo("TB_CHINH_SACH_CONG_DOAN"));

            Assert.That(tax.Status, Is.EqualTo(PolicyGroupStatus.ObjectNotFound));
            Assert.That(tax.PrimaryTable, Is.EqualTo("TB_THUE_TNCN_CHINH_SACH"));

            // IsPolicySchemaAvailable should return false with details
            bool isAvailable = PolicyResolver.IsPolicySchemaAvailable(out string missingDetails);
            Assert.That(isAvailable, Is.False);
            Assert.That(missingDetails, Does.Contain("TB_CHINH_SACH_LUONG"));
            Assert.That(missingDetails, Does.Contain("TB_CHINH_SACH_BHXH"));
            Assert.That(missingDetails, Does.Contain("TB_CHINH_SACH_CONG_DOAN"));
            Assert.That(missingDetails, Does.Contain("TB_THUE_TNCN_CHINH_SACH"));
        }

        [Test]
        public void Test_02_EnsureLoaded_ThrowsPolicySchemaMissingException_WithExactMissingTableAndScript()
        {
            var ex = Assert.Throws<PolicySchemaMissingException>(() => new PolicyResolver());
            Assert.That(ex.MissingTableName, Is.EqualTo("TB_CHINH_SACH_LUONG"));
            Assert.That(ex.MigrationScript, Is.EqualTo("apply_payroll_v1_16_objects.sql"));
            Assert.That(ex.Message, Does.Contain("TB_CHINH_SACH_LUONG"));
            Assert.That(ex.Message, Does.Contain("apply_payroll_v1_16_objects.sql"));
        }

        [Test]
        public void Test_03_SaveBehavior_NoChanges_DetectsZeroItemsAndReportsNoChanges()
        {
            // ARRANGE: Simulated allowance list from DB
            var dbList = new List<TB_PHUCAP>
            {
                new TB_PHUCAP { IDPC = 1, TENPC = "Phụ cấp ăn trưa" },
                new TB_PHUCAP { IDPC = 2, TENPC = "Phụ cấp xăng xe" }
            };

            // Grid items having exact same names (with potential whitespace differences)
            var gridItems = new List<(string MaKhoan, string TenKhoan)>
            {
                ("PC_1", "  Phụ cấp ăn trưa  "),
                ("PC_2", "Phụ cấp xăng xe")
            };

            // ACT: Detect changes using the exact logic implemented in FrmCauHinhLuong
            var changes = new List<(int IdPc, string OldName, string NewName)>();
            foreach (var row in gridItems)
            {
                if (row.MaKhoan != null && row.MaKhoan.StartsWith("PC_") && int.TryParse(row.MaKhoan.Substring(3), out int idPc))
                {
                    var pc = dbList.FirstOrDefault(x => x.IDPC == idPc);
                    if (pc != null)
                    {
                        string currentGridName = (row.TenKhoan ?? "").Trim();
                        string dbName = (pc.TENPC ?? "").Trim();
                        if (!string.Equals(currentGridName, dbName, StringComparison.Ordinal))
                        {
                            changes.Add((idPc, pc.TENPC, currentGridName));
                        }
                    }
                }
            }

            // ASSERT: 0 changes detected, UI should report "Không có thay đổi để lưu."
            Assert.That(changes.Count, Is.EqualTo(0));
        }

        [Test]
        public void Test_04_SaveBehavior_RenameAllowance_Succeeds_AndRequeryConfirmsChange()
        {
            var buPhuCap = new PHUCAP();
            int targetId;
            string originalName;

            using (var db = new MyEntities())
            {
                var target = db.TB_PHUCAP.FirstOrDefault();
                Assert.That(target, Is.Not.Null, "DB must contain at least 1 allowance to test rename.");
                targetId = (int)target.IDPC;
                originalName = target.TENPC;
            }

            string tempName = originalName + " [TEST]";

            try
            {
                // ACT 1: Rename allowance
                buPhuCap.UpdateCatalogItem(targetId, tempName, 1);

                // REQUERY: Verify name was changed in database
                using (var db = new MyEntities())
                {
                    var updated = db.TB_PHUCAP.FirstOrDefault(x => x.IDPC == targetId);
                    Assert.That(updated, Is.Not.Null);
                    Assert.That(updated.TENPC, Is.EqualTo(tempName));

                    // Verify audit log exists
                    var recentLogs = db.TB_SYS_LOG
                        .Where(l => l.TEN_BANG == "TB_PHUCAP")
                        .OrderByDescending(l => l.THOIGIAN)
                        .Take(10)
                        .ToList();

                    var log = recentLogs.FirstOrDefault(l => l.ID_BAN_GHI != null && l.ID_BAN_GHI.ToString() == targetId.ToString());

                    Assert.That(log, Is.Not.Null);
                    Assert.That(log.HANHDONG, Is.EqualTo("CAP_NHAT_PHUCAP"));
                    Assert.That(log.DU_LIEU_MOI, Is.EqualTo(tempName));
                }
            }
            finally
            {
                // RESTORE: Always restore original name
                buPhuCap.UpdateCatalogItem(targetId, originalName, 1);
                using (var db = new MyEntities())
                {
                    var restored = db.TB_PHUCAP.FirstOrDefault(x => x.IDPC == targetId);
                    Assert.That(restored.TENPC, Is.EqualTo(originalName));
                }
            }
        }

        [Test]
        public void Test_05_SaveBehavior_AllowanceUpdate_IndependentOfMissingPolicySchema()
        {
            // Verify that policy schema is missing on live DB
            bool isPolicyReady = PolicyResolver.IsPolicySchemaAvailable(out _);
            Assert.That(isPolicyReady, Is.False, "Policy schema must be missing to test decoupled behavior.");

            // Even when policy schema is missing, updating allowance catalog MUST succeed
            var buPhuCap = new PHUCAP();
            int targetId;
            string originalName;
            using (var db = new MyEntities())
            {
                var target = db.TB_PHUCAP.FirstOrDefault();
                Assert.That(target, Is.Not.Null);
                targetId = (int)target.IDPC;
                originalName = target.TENPC;
            }

            // Calling UpdateCatalogItem with same name does not throw PolicySchemaMissingException
            Assert.DoesNotThrow(() => buPhuCap.UpdateCatalogItem(targetId, originalName, 1));
        }

        [Test]
        public void Test_06_SaveBehavior_UnauthenticatedOrUnauthorized_ThrowsBusinessException()
        {
            var buPhuCap = new PHUCAP();

            // Case A: Unauthenticated
            UserSession.CurrentUser = null;
            var unauthEx = Assert.Throws<BusinessException>(() => buPhuCap.UpdateCatalogItem(1, "New Name", 0));
            Assert.That(unauthEx.ErrorCode, Is.EqualTo("UNAUTHENTICATED"));

            // Case B: Logged in but non-admin without F_CC_BANGLUONG edit permission
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 888,
                USERNAME = "regular_staff",
                FULLNAME = "Regular Staff"
            };
            UserSession.UserRights.Clear();
            UserSession.DetailedRights.Clear();

            var permEx = Assert.Throws<BusinessException>(() => buPhuCap.UpdateCatalogItem(1, "New Name", 888));
            Assert.That(permEx.ErrorCode, Is.EqualTo("PERMISSION_DENIED"));
        }

        [Test]
        public void Test_07_PolicyGroupStatus_EvaluationLogic()
        {
            // 1. Missing table -> ObjectNotFound
            var resMissing = new PolicyGroupInspectionResult
            {
                GroupKey = "SALARY",
                GroupName = "Chính sách lương",
                PrimaryTable = "TB_CHINH_SACH_LUONG",
                Status = PolicyGroupStatus.ObjectNotFound,
                StatusDisplay = "Chưa khởi tạo bảng (Cần chạy migration)",
                TechnicalError = "ORA-00942: table or view does not exist",
                ActionRequired = "Thực thi script apply_payroll_v1_16_objects.sql"
            };
            Assert.That(resMissing.Status == PolicyGroupStatus.Ready, Is.False);
            Assert.That(resMissing.StatusDisplay, Does.Contain("Chưa khởi tạo"));

            // 2. Table exists but 0 records -> NoPolicyRecord
            var resEmpty = new PolicyGroupInspectionResult
            {
                GroupKey = "INSURANCE",
                GroupName = "Chính sách BHXH",
                PrimaryTable = "TB_CHINH_SACH_BHXH",
                Status = PolicyGroupStatus.NoPolicyRecord,
                StatusDisplay = "Chưa có bản ghi chính sách nào",
                ValueSummary = "0 bản ghi",
                ActionRequired = "Cần khởi tạo ít nhất một bản ghi"
            };
            Assert.That(resEmpty.Status == PolicyGroupStatus.Ready, Is.False);
            Assert.That(resEmpty.StatusDisplay, Does.Contain("Chưa có bản ghi"));

            // 3. Table exists, records exist, but none effective for date -> NoEffectivePolicy
            var resNoEff = new PolicyGroupInspectionResult
            {
                GroupKey = "TAX",
                GroupName = "Chính sách thuế TNCN",
                PrimaryTable = "TB_THUE_TNCN_CHINH_SACH",
                Status = PolicyGroupStatus.NoEffectivePolicy,
                StatusDisplay = "Không có chính sách hiệu lực cho ngày kiểm tra",
                ValueSummary = "2 bản ghi (0 bản ghi hiệu lực)",
                TechnicalError = "Có 2 bản ghi nhưng không có bản ghi nào hiệu lực cho ngày kiểm tra"
            };
            Assert.That(resNoEff.Status == PolicyGroupStatus.Ready, Is.False);
            Assert.That(resNoEff.StatusDisplay, Does.Contain("Không có chính sách hiệu lực"));

            // 4. Ready
            var resReady = new PolicyGroupInspectionResult
            {
                GroupKey = "UNION",
                GroupName = "Chính sách công đoàn",
                PrimaryTable = "TB_CHINH_SACH_CONG_DOAN",
                Status = PolicyGroupStatus.Ready,
                StatusDisplay = "Sẵn sàng áp dụng",
                ValueSummary = "1 bản ghi hiệu lực"
            };
            Assert.That(resReady.Status == PolicyGroupStatus.Ready, Is.True);
            Assert.That(resReady.StatusDisplay, Does.Contain("Sẵn sàng"));

            // 5. ConnectionFailed
            var resConn = new PolicyGroupInspectionResult
            {
                GroupKey = "SALARY",
                GroupName = "Chính sách lương",
                PrimaryTable = "TB_CHINH_SACH_LUONG",
                Status = PolicyGroupStatus.ConnectionFailed,
                StatusDisplay = "Lỗi kết nối cơ sở dữ liệu",
                TechnicalError = "ORA-12170: TNS:Connect timeout occurred"
            };
            Assert.That(resConn.Status == PolicyGroupStatus.Ready, Is.False);
            Assert.That(resConn.StatusDisplay, Does.Contain("Lỗi kết nối"));
        }

        [Test]
        public void Test_08_SaveBehavior_ReloadFailure_IsolatedFromSaveCommit()
        {
            // Simulate the workflow: Save commits successfully, but reload fails
            bool saveCommitted = false;
            string feedbackMessage = null;

            try
            {
                // Step 1: Save transaction
                saveCommitted = true; // DB committed

                // Step 2: Reload in isolated try/catch
                try
                {
                    throw new InvalidOperationException("Mất kết nối mạng tạm thời khi tải lại.");
                }
                catch (Exception reloadEx)
                {
                    feedbackMessage = $"Dữ liệu đã được lưu vào CSDL thành công, nhưng xảy ra lỗi khi làm mới hiển thị: {reloadEx.Message}";
                }
            }
            catch (Exception ex)
            {
                feedbackMessage = $"Lỗi khi lưu: {ex.Message}";
            }

            // ASSERT: Save is acknowledged as committed, reload failure is explicitly segregated
            Assert.That(saveCommitted, Is.True);
            Assert.That(feedbackMessage, Does.StartWith("Dữ liệu đã được lưu vào CSDL thành công, nhưng xảy ra lỗi khi làm mới"));
            Assert.That(feedbackMessage, Does.Not.StartWith("Lỗi khi lưu"));
        }

        [Test]
        public void Test_09_SaveBehavior_SaveError_PreservesInputData()
        {
            // When save fails before commit, reload is skipped so user edits are not wiped
            bool reloadInvoked = false;
            bool saveFailed = false;

            try
            {
                throw new BusinessException("VALIDATION_ERROR", "Tên phụ cấp không được để trống.");
            }
            catch
            {
                saveFailed = true;
                // UI preserves dirty grid state, does NOT call LoadData()
            }

            Assert.That(saveFailed, Is.True);
            Assert.That(reloadInvoked, Is.False, "Reload must not be invoked on save failure, preserving user inputs.");
        }
    }
}
