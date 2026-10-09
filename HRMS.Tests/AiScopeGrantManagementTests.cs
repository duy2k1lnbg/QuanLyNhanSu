using System;
using System.Collections.Generic;
using System.Linq;
using Bu.Services.AI_Services.Security;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiScopeGrantManagementTests
    {
        private InMemoryAiScopeGrantRepository _repo;
        private MockAdminSecurityContext _adminSecCtx;
        private AiScopeGrantManagementService _service;

        public class MockAdminSecurityContext : IAdminSecurityContext
        {
            public bool IsAdminUser { get; set; } = true;
            public int ActorId { get; set; } = 1;
            public string ActorName { get; set; } = "admin";
            public string Client { get; set; } = "TEST";

            public bool IsAdmin() => IsAdminUser;
            public int GetActorUserId() => ActorId;
            public string GetActorUsername() => ActorName;
            public string GetClientType() => Client;
        }

        [SetUp]
        public void Setup()
        {
            _repo = new InMemoryAiScopeGrantRepository();
            _adminSecCtx = new MockAdminSecurityContext();
            _service = new AiScopeGrantManagementService(_repo, _adminSecCtx);

            // Cấu hình dữ liệu mẫu ban đầu trong in-memory repository
            _repo.AddUser(101, "admin", "Quản trị viên", manv: 1, isDisabled: false,
                rights: new[] { "F_DM_NHANVIEN", "F_CC_BANGLUONG", "F_CC_PHUCAP", "F_CC_TANGCA", "F_SYSTEM_AI" });

            _repo.AddUser(102, "nv_test", "Nhân viên Test", manv: 10, isDisabled: false,
                rights: new[] { "F_DM_NHANVIEN", "F_CC_BANGLUONG", "F_SYSTEM_AI" });

            _repo.AddUser(103, "user_no_manv", "Tài khoản hệ thống", manv: null, isDisabled: false,
                rights: new[] { "F_SYSTEM_AI" });

            _repo.AddGroup(201, "GRP_IT", "Phòng IT", isDisabled: false,
                rights: new[] { "F_DM_NHANVIEN", "F_CC_TANGCA" });

            _repo.AddGroup(202, "GRP_BOD", "Ban Giám Đốc", isDisabled: false,
                rights: new[] { "F_DM_NHANVIEN", "F_CC_BANGLUONG", "F_CC_PHUCAP" });

            _repo.AddMembership(102, 201); // nv_test thuộc GRP_IT
        }

        /// <summary>
        /// Ca 1: Oracle mất kết nối / ORA-00942 / ORA-01031:
        /// Tải báo chưa sẵn sàng; lưu thất bại; không tạo JSON, không báo thành công, không mất chính sách cũ.
        /// </summary>
        [Test]
        public void Case01_SchemaNotReady_MustReportNotReady_SaveMustFail_NoJsonFallback()
        {
            _repo.SimulateSchemaNotReady = true;
            _repo.SimulatedNotReadyMessage = "Hệ thống chính sách AI chưa sẵn sàng: Bảng TB_AI_SCOPE_GRANT chưa được khởi tạo (ORA-00942).";

            // 1. Tải dữ liệu: Phải thông báo IsReady = false kèm lý do rõ ràng
            var overview = _service.GetSubjectScopeOverview("USER", 102);
            Assert.IsFalse(overview.IsReady, "Khi schema chưa sẵn sàng, IsReady phải là false.");
            StringAssert.Contains("ORA-00942", overview.NotReadyReason);

            // 2. Lưu dữ liệu: Phải thất bại, trả về IsSchemaNotReady = true
            var saveReq = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto
                    {
                        CapabilityCode = "EMPLOYEE_LOOKUP",
                        Effect = "ALLOW",
                        ScopeType = "DEPARTMENT",
                        ScopeKey = "2"
                    }
                }
            };

            var saveRes = _service.SaveSubjectScopeGrants(saveReq);
            Assert.IsFalse(saveRes.Success, "Lưu dữ liệu phải thất bại khi schema chưa sẵn sàng.");
            Assert.IsTrue(saveRes.IsSchemaNotReady, "Phải đánh dấu cờ IsSchemaNotReady.");
            StringAssert.Contains("ORA-00942", saveRes.Message);

            // 3. Đảm bảo không có bản ghi nào bị ghi vào bộ nhớ
            Assert.AreEqual(0, _repo.GetGrants("USER", 102).Count, "Không được lưu dữ liệu giả vào storage.");
            Assert.AreEqual(0, _repo.Audits.Count, "Không được ghi nhận audit thành công giả.");
        }

        /// <summary>
        /// Ca 2: Người chỉ có F_SYSTEM_AI không được đọc/ghi quản trị;
        /// Thiếu session hoặc giả actor bị từ chối; API và Desktop cùng kết quả.
        /// </summary>
        [Test]
        public void Case02_NonAdmin_MustBeRejected_ForgedActorMustNotBeTrusted()
        {
            // Thiết lập Actor không phải là Admin (chỉ là người dùng thường)
            var nonAdminCtx = new MockAdminSecurityContext
            {
                IsAdminUser = false,
                ActorId = 102,
                ActorName = "nv_test"
            };
            var nonAdminService = new AiScopeGrantManagementService(_repo, nonAdminCtx);

            // 1. Đọc: Phải bị từ chối với UnauthorizedAccessException
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                nonAdminService.GetSubjectScopeOverview("USER", 102);
            }, "Non-admin không được phép đọc ma trận phân quyền AI.");

            // 2. Ghi: Dù request cố tình giả mạo ActorUserId = 1 / admin
            var forgedReq = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                ActorUserId = 1, // Giả mạo admin
                ActorUsername = "admin", // Giả mạo admin
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto
                    {
                        CapabilityCode = "EMPLOYEE_LOOKUP",
                        Effect = "ALLOW",
                        ScopeType = "ALL"
                    }
                }
            };

            var result = nonAdminService.SaveSubjectScopeGrants(forgedReq);
            Assert.IsFalse(result.Success, "Thao tác lưu của non-admin phải bị từ chối.");
            StringAssert.Contains("Quản trị viên", result.Message);
            Assert.AreEqual(0, _repo.Audits.Count, "Không được ghi audit khi lưu bị từ chối.");
        }

        /// <summary>
        /// Ca 3: Direct SELF + group ALL:
        /// Kết quả hiệu lực ALL; dữ liệu ngoài bản thân được xem theo chính sách; UI giải thích nguồn nhóm.
        /// </summary>
        [Test]
        public void Case03_DirectSelf_Plus_GroupAll_MustYieldAll_AndExplainExpansion()
        {
            // Direct: ALLOW SELF
            _repo.GrantsStore["USER:102"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "ALLOW",
                    ScopeType = "SELF",
                    IsEnabled = true
                }
            };

            // Group 201 (mà user 102 tham gia): ALLOW ALL
            _repo.GrantsStore["GROUP:201"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "ALLOW",
                    ScopeType = "ALL",
                    IsEnabled = true
                }
            };

            var overview = _service.GetSubjectScopeOverview("USER", 102);
            var item = overview.Capabilities.First(c => c.CapabilityCode == "EMPLOYEE_LOOKUP");

            // Kết quả hiệu lực phải là ALL (do hợp phạm vi)
            Assert.AreEqual("ALLOW", item.EffectiveEffect);
            Assert.AreEqual("ALL", item.EffectiveScopeType);
            Assert.IsTrue(item.EffectiveIsAll);
            StringAssert.Contains("Toàn bộ hệ thống", item.EffectiveScopeSummary);

            // UI giải thích rõ quyền nhóm mở rộng phạm vi, direct SELF không thu hẹp
            StringAssert.Contains("mở rộng", item.ExplanationNotes);
            StringAssert.Contains("không thu hẹp", item.ExplanationNotes);

            // Kiểm tra Runtime tại AiScopeEvaluator: Phải cho phép nhân viên khác (targetManv = 999)
            var authCtx = new AiAuthorizationContext
            {
                UserId = 102,
                Manv = 10,
                FunctionRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "F_DM_NHANVIEN", "F_SYSTEM_AI" },
                ScopeGrants = new List<AiScopeGrant>
                {
                    new AiScopeGrant { CapabilityCode = "EMPLOYEE_LOOKUP", ScopeType = "SELF", Effect = "ALLOW" },
                    new AiScopeGrant { CapabilityCode = "EMPLOYEE_LOOKUP", ScopeType = "ALL", Effect = "ALLOW" }
                }
            };

            bool inScopeOtherEmp = AiScopeEvaluator.IsEmployeeInScope(authCtx, targetManv: 999, capabilityCode: "EMPLOYEE_LOOKUP");
            Assert.IsTrue(inScopeOtherEmp, "Runtime phải cho phép nhân viên khác khi có grant ALL từ nhóm.");
        }

        /// <summary>
        /// Ca 4: Nhiều nhóm ALLOW phòng 2 và phòng 5: hợp cả hai.
        /// Một DENY hết hạn hoặc tương lai không che mất ALLOW hiện hành; DENY hiện hành chặn toàn capability.
        /// </summary>
        [Test]
        public void Case04_UnionMultipleGroups_ExpiredOrFutureDenyDoesNotBlock_ActiveDenyBlocksAll()
        {
            var now = DateTime.Now;

            // Nhóm 201: ALLOW phòng 2
            _repo.GrantsStore["GROUP:201"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "ALLOW",
                    ScopeType = "DEPARTMENT",
                    ScopeKey = "2",
                    IsEnabled = true
                }
            };

            // Thêm user 102 vào Nhóm 202: ALLOW phòng 5
            _repo.AddMembership(102, 202);
            _repo.GrantsStore["GROUP:202"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "ALLOW",
                    ScopeType = "DEPARTMENT",
                    ScopeKey = "5",
                    IsEnabled = true
                }
            };

            // 1. Kiểm tra hợp cả 2 phòng ban 2 và 5
            var overview = _service.GetSubjectScopeOverview("USER", 102, asOf: now);
            var item = overview.Capabilities.First(c => c.CapabilityCode == "EMPLOYEE_LOOKUP");
            Assert.AreEqual("ALLOW", item.EffectiveEffect);
            Assert.AreEqual("DEPARTMENT", item.EffectiveScopeType);
            Assert.That(item.EffectiveDepartmentIds, Is.EquivalentTo(new[] { 2, 5 }));
            StringAssert.Contains("Phòng IT", item.EffectiveScopeSummary);
            StringAssert.Contains("Phòng Kế toán", item.EffectiveScopeSummary);

            // 2. Thêm DENY đã hết hạn (hết hạn hôm qua) -> Không được chặn
            _repo.GrantsStore["USER:102"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "DENY",
                    ScopeType = "ALL",
                    ValidTo = now.AddDays(-1),
                    IsEnabled = true
                }
            };
            var overviewWithExpiredDeny = _service.GetSubjectScopeOverview("USER", 102, asOf: now);
            var itemExpired = overviewWithExpiredDeny.Capabilities.First(c => c.CapabilityCode == "EMPLOYEE_LOOKUP");
            Assert.AreEqual("ALLOW", itemExpired.EffectiveEffect, "DENY đã hết hạn không được chặn ALLOW hiện hành.");

            // 3. Thêm DENY tương lai (bắt đầu sau 3 ngày) -> Không được chặn
            _repo.GrantsStore["USER:102"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "DENY",
                    ScopeType = "ALL",
                    ValidFrom = now.AddDays(3),
                    IsEnabled = true
                }
            };
            var overviewWithFutureDeny = _service.GetSubjectScopeOverview("USER", 102, asOf: now);
            var itemFuture = overviewWithFutureDeny.Capabilities.First(c => c.CapabilityCode == "EMPLOYEE_LOOKUP");
            Assert.AreEqual("ALLOW", itemFuture.EffectiveEffect, "DENY tương lai không được chặn ALLOW hiện hành.");

            // 4. Thêm DENY hiện hành còn hiệu lực -> Chặn TOÀN BỘ capability
            _repo.GrantsStore["USER:102"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord
                {
                    CapabilityCode = "EMPLOYEE_LOOKUP",
                    Effect = "DENY",
                    ScopeType = "ALL",
                    ValidTo = now.AddDays(10),
                    IsEnabled = true
                }
            };
            var overviewWithActiveDeny = _service.GetSubjectScopeOverview("USER", 102, asOf: now);
            var itemActiveDeny = overviewWithActiveDeny.Capabilities.First(c => c.CapabilityCode == "EMPLOYEE_LOOKUP");
            Assert.AreEqual("DENY", itemActiveDeny.EffectiveEffect, "DENY hiện hành phải chặn toàn bộ capability.");
            StringAssert.Contains("Bị CHẶN toàn bộ capability", itemActiveDeny.EffectiveScopeSummary);
        }

        /// <summary>
        /// Ca 5: Nhiều grant trực tiếp cùng capability, gồm ALLOW và DENY:
        /// Tổng hợp đúng, không bỏ DENY do thứ tự row DB.
        /// </summary>
        [Test]
        public void Case05_DirectGrants_AllowAndDeny_DenyMustAlwaysPrevailRegardlessOfOrder()
        {
            var now = DateTime.Now;

            // Thứ tự 1: ALLOW trước, DENY sau
            var input1 = new AiScopePolicyResolver.ResolutionInput
            {
                CapabilityCode = "EMPLOYEE_LOOKUP",
                IsCapabilityEnabled = true,
                RequiredFunctionCode = "F_DM_NHANVIEN",
                HasRequiredFunctionRight = true,
                ActorManv = 10,
                DirectGrants = new List<AiScopeGrantRecord>
                {
                    new AiScopeGrantRecord { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "ALL", IsEnabled = true },
                    new AiScopeGrantRecord { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "DENY", ScopeType = "ALL", IsEnabled = true }
                },
                AsOf = now
            };
            var output1 = AiScopePolicyResolver.Resolve(input1);
            Assert.AreEqual("DENY", output1.EffectiveEffect, "Khi có DENY, kết quả phải luôn là DENY dù ALLOW đứng trước.");

            // Thứ tự 2: DENY trước, ALLOW sau
            var input2 = new AiScopePolicyResolver.ResolutionInput
            {
                CapabilityCode = "EMPLOYEE_LOOKUP",
                IsCapabilityEnabled = true,
                RequiredFunctionCode = "F_DM_NHANVIEN",
                HasRequiredFunctionRight = true,
                ActorManv = 10,
                DirectGrants = new List<AiScopeGrantRecord>
                {
                    new AiScopeGrantRecord { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "DENY", ScopeType = "ALL", IsEnabled = true },
                    new AiScopeGrantRecord { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "ALL", IsEnabled = true }
                },
                AsOf = now
            };
            var output2 = AiScopePolicyResolver.Resolve(input2);
            Assert.AreEqual("DENY", output2.EffectiveEffect, "Khi có DENY, kết quả phải luôn là DENY dù DENY đứng trước.");
        }

        /// <summary>
        /// Ca 6: Hai phòng ban trên một capability vẫn còn đủ sau vòng đọc-sửa-lưu;
        /// Grant không sửa/không khả dụng không biến mất.
        /// </summary>
        [Test]
        public void Case06_MultipleDepartmentsOnOneCapability_MustSurviveSaveCycle()
        {
            // Thiết lập user 102 có 2 direct grant: Phòng 2 và Phòng 5
            _repo.GrantsStore["USER:102"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord { GrantId = 1, CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2", IsEnabled = true },
                new AiScopeGrantRecord { GrantId = 2, CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "5", IsEnabled = true },
                new AiScopeGrantRecord { GrantId = 3, CapabilityCode = "OVERTIME_VIEW", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2", IsEnabled = true }
            };

            // Đọc dữ liệu lên
            var overview = _service.GetSubjectScopeOverview("USER", 102);
            var empItem = overview.Capabilities.First(c => c.CapabilityCode == "EMPLOYEE_LOOKUP");
            Assert.AreEqual(2, empItem.DirectGrants.Count, "Phải tải đủ 2 grant phòng ban cho EMPLOYEE_LOOKUP.");

            // Gửi lưu với 2 grant phòng ban cho EMPLOYEE_LOOKUP và thêm grant cho PAYROLL_VIEW
            var saveReq = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                BaseRevision = overview.CurrentRevision,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { GrantId = 1, CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2" },
                    new SaveAiSubjectScopeGrantItemDto { GrantId = 2, CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "5" },
                    new SaveAiSubjectScopeGrantItemDto { GrantId = 3, CapabilityCode = "OVERTIME_VIEW", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2" },
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "PAYROLL_VIEW", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2" }
                }
            };

            var saveRes = _service.SaveSubjectScopeGrants(saveReq);
            Assert.IsTrue(saveRes.Success, "Lưu danh sách grant thành công.");

            // Kiểm tra storage sau khi lưu
            var storedGrants = _repo.GetGrants("USER", 102);
            var empGrantsAfter = storedGrants.Where(g => g.CapabilityCode == "EMPLOYEE_LOOKUP").ToList();
            Assert.AreEqual(2, empGrantsAfter.Count, "Cả 2 grant phòng ban cho EMPLOYEE_LOOKUP phải còn nguyên.");
            Assert.That(empGrantsAfter.Select(g => g.ScopeKey), Is.EquivalentTo(new[] { "2", "5" }));
        }

        /// <summary>
        /// Ca 7: Revision/audit lỗi giữa transaction: rollback toàn bộ;
        /// Commit thành công mới trả revision thật. Phiên AI ở tiến trình khác không tiếp tục dùng quyền vừa bị thu hồi.
        /// </summary>
        [Test]
        public void Case07_TransactionFailure_MustRollbackCompletely_SuccessfulCommitIncrementsRevision()
        {
            long initialRev = _repo.Revision;

            // 1. Mô phỏng lỗi giao dịch commit
            _repo.SimulateTransactionFailure = true;

            var req = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                BaseRevision = initialRev,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "ALL" }
                }
            };

            var failRes = _service.SaveSubjectScopeGrants(req);
            Assert.IsFalse(failRes.Success, "Lưu phải thất bại khi transaction lỗi.");
            Assert.AreEqual(initialRev, _repo.Revision, "Revision không được tăng khi transaction rollback.");
            Assert.AreEqual(0, _repo.Audits.Count, "Audit log không được ghi nhận khi transaction rollback.");

            // 2. Khôi phục trạng thái bình thường -> Lưu thành công
            _repo.SimulateTransactionFailure = false;
            var successRes = _service.SaveSubjectScopeGrants(req);

            Assert.IsTrue(successRes.Success, "Lưu phải thành công khi không có lỗi.");
            Assert.AreEqual(initialRev + 1, successRes.NewPolicyRevision, "Revision phải tăng đúng 1.");
            Assert.AreEqual(initialRev + 1, _repo.Revision, "Revision trong storage phải khớp với kết quả trả về.");
            Assert.AreEqual(1, _repo.Audits.Count, "Audit log phải được ghi nhận.");
        }

        /// <summary>
        /// Ca 8: Hai admin lưu đồng thời: phát hiện version conflict, không lost update.
        /// </summary>
        [Test]
        public void Case08_OptimisticConcurrency_TwoAdminsSaveConcurrently_MustDetectConflict()
        {
            // Admin A và Admin B cùng mở form tại Revision = 10
            _repo.Revision = 10;
            var overviewA = _service.GetSubjectScopeOverview("USER", 102);
            var overviewB = _service.GetSubjectScopeOverview("USER", 102);

            Assert.AreEqual(10, overviewA.CurrentRevision);
            Assert.AreEqual(10, overviewB.CurrentRevision);

            // Admin A lưu trước -> Thành công, Revision tăng lên 11
            var saveReqA = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                BaseRevision = overviewA.CurrentRevision,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2" }
                }
            };
            var resA = _service.SaveSubjectScopeGrants(saveReqA);
            Assert.IsTrue(resA.Success);
            Assert.AreEqual(11, resA.NewPolicyRevision);
            Assert.AreEqual(11, _repo.Revision);

            // Admin B lưu sau với BaseRevision cũ (10) -> Phải phát hiện xung đột
            var saveReqB = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                BaseRevision = overviewB.CurrentRevision, // 10 (cũ)
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "5" }
                }
            };
            var resB = _service.SaveSubjectScopeGrants(saveReqB);

            Assert.IsFalse(resB.Success, "Admin B lưu với revision cũ phải bị từ chối.");
            Assert.IsTrue(resB.IsConcurrencyConflict, "Phải đánh dấu cờ IsConcurrencyConflict.");
            StringAssert.Contains("Xung đột phiên bản", resB.Message);

            // Xác minh dữ liệu của Admin A không bị Admin B ghi đè (No lost update)
            var currentGrants = _repo.GetGrants("USER", 102);
            Assert.AreEqual(1, currentGrants.Count);
            Assert.AreEqual("2", currentGrants[0].ScopeKey, "Dữ liệu của Admin A (Phòng 2) phải được bảo toàn.");
        }

        /// <summary>
        /// Ca 9: Capability DB tắt dù catalog bật: UI chưa khả dụng, backend từ chối; không tự bật.
        /// </summary>
        [Test]
        public void Case09_DisabledCapability_MustRemainDisabled_BackendMustRejectGrantConfig()
        {
            // ATTENDANCE_DETAIL đang có IsEnabled = false trong AiCapabilityCatalog
            var catalogItem = AiCapabilityCatalog.Get("ATTENDANCE_DETAIL");
            Assert.IsNotNull(catalogItem);
            Assert.IsFalse(catalogItem.IsEnabled);

            // 1. Kiểm tra Resolver trả về DISABLED
            var input = new AiScopePolicyResolver.ResolutionInput
            {
                CapabilityCode = "ATTENDANCE_DETAIL",
                IsCapabilityEnabled = false,
                RequiredFunctionCode = "F_CC_BANGCONG",
                HasRequiredFunctionRight = true,
                ActorManv = 10
            };
            var output = AiScopePolicyResolver.Resolve(input);
            Assert.AreEqual("DISABLED", output.EffectiveEffect);
            StringAssert.Contains("chưa khả dụng", output.EffectiveScopeSummary);

            // 2. Backend từ chối thêm grant cho capability bị disabled
            var req = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto
                    {
                        CapabilityCode = "ATTENDANCE_DETAIL",
                        Effect = "ALLOW",
                        ScopeType = "ALL"
                    }
                }
            };

            var res = _service.SaveSubjectScopeGrants(req);
            Assert.IsFalse(res.Success, "Backend phải từ chối cấp quyền cho capability đang bị vô hiệu hóa.");
            Assert.That(res.ValidationErrors.Any(e => e.Contains("vô hiệu hóa")), Is.True);
        }

        /// <summary>
        /// Ca 10: Quyền nền thiếu dù có F_SYSTEM_AI và ALLOW: từ chối.
        /// SELF thiếu MANV hoặc xem người khác: từ chối.
        /// </summary>
        [Test]
        public void Case10_MissingFunctionRight_Or_UnmappedEmployee_MustBeBlocked()
        {
            // 1. Tài khoản có F_SYSTEM_AI và có ALLOW ALL cho PAYROLL_VIEW, nhưng thiếu F_CC_BANGLUONG
            var inputNoFunc = new AiScopePolicyResolver.ResolutionInput
            {
                CapabilityCode = "PAYROLL_VIEW",
                IsCapabilityEnabled = true,
                RequiredFunctionCode = "F_CC_BANGLUONG",
                HasRequiredFunctionRight = false, // Thiếu quyền nền
                ActorManv = 10,
                DirectGrants = new List<AiScopeGrantRecord>
                {
                    new AiScopeGrantRecord { CapabilityCode = "PAYROLL_VIEW", Effect = "ALLOW", ScopeType = "ALL", IsEnabled = true }
                }
            };
            var outputNoFunc = AiScopePolicyResolver.Resolve(inputNoFunc);
            Assert.AreEqual("BLOCKED_NO_FUNCTION", outputNoFunc.EffectiveEffect);
            StringAssert.Contains("Thiếu quyền nghiệp vụ nền", outputNoFunc.EffectiveScopeSummary);

            // 2. Tài khoản hệ thống thiếu MANV (manv = null) với nghiệp vụ INSURANCE_SELF
            var inputNoManv = new AiScopePolicyResolver.ResolutionInput
            {
                CapabilityCode = "INSURANCE_SELF",
                IsCapabilityEnabled = true,
                RequiredFunctionCode = null,
                HasRequiredFunctionRight = true,
                ActorManv = null // Chưa map nhân viên
            };
            var outputNoManv = AiScopePolicyResolver.Resolve(inputNoManv);
            Assert.AreEqual("UNMAPPED_EMPLOYEE", outputNoManv.EffectiveEffect);
            StringAssert.Contains("Thiếu MANV", outputNoManv.EffectiveScopeSummary);

            // 3. User có MANV = 10 cố gắng xem nhân viên khác (targetManv = 20) trên INSURANCE_SELF
            var authCtx = new AiAuthorizationContext
            {
                UserId = 102,
                Manv = 10,
                FunctionRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "F_SYSTEM_AI" },
                ScopeGrants = new List<AiScopeGrant>
                {
                    new AiScopeGrant { CapabilityCode = "INSURANCE_SELF", ScopeType = "ALL", Effect = "ALLOW" }
                }
            };
            bool selfCheckOther = AiScopeEvaluator.IsEmployeeInScope(authCtx, targetManv: 20, capabilityCode: "INSURANCE_SELF");
            Assert.IsFalse(selfCheckOther, "Nghiệp vụ _SELF không được phép xem nhân viên khác dù có grant ALL.");

            bool selfCheckOwn = AiScopeEvaluator.IsEmployeeInScope(authCtx, targetManv: 10, capabilityCode: "INSURANCE_SELF");
            Assert.IsTrue(selfCheckOwn, "Nghiệp vụ _SELF phải cho phép xem chính nhân viên của mình.");
        }

        /// <summary>
        /// Ca 11: Subject/phòng ban/công ty không tồn tại; ScopeKey còn từ loại cũ; null grant; payload thiếu; ngày không hợp lệ: không ghi.
        /// </summary>
        [Test]
        public void Case11_ValidationErrors_InvalidSubjectOrScopeOrDates_MustNotSave()
        {
            // 1. Subject không tồn tại
            var reqInvalidSubject = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 99999, // Không tồn tại
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "SELF" }
                }
            };
            var resInvalidSubject = _service.SaveSubjectScopeGrants(reqInvalidSubject);
            Assert.IsFalse(resInvalidSubject.Success);
            StringAssert.Contains("Không tìm thấy", resInvalidSubject.Message);

            // 2. Phòng ban không tồn tại trong danh mục
            var reqInvalidDept = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "9999" }
                }
            };
            var resInvalidDept = _service.SaveSubjectScopeGrants(reqInvalidDept);
            Assert.IsFalse(resInvalidDept.Success);
            Assert.That(resInvalidDept.ValidationErrors.Any(e => e.Contains("không tồn tại")), Is.True);

            // 3. Công ty không tồn tại trong danh mục
            var reqInvalidComp = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "COMPANY", ScopeKey = "NON_EXISTING" }
                }
            };
            var resInvalidComp = _service.SaveSubjectScopeGrants(reqInvalidComp);
            Assert.IsFalse(resInvalidComp.Success);
            Assert.That(resInvalidComp.ValidationErrors.Any(e => e.Contains("không tồn tại")), Is.True);

            // 4. Ngày hết hạn nhỏ hơn ngày bắt đầu
            var reqInvalidDates = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto
                    {
                        CapabilityCode = "EMPLOYEE_LOOKUP",
                        Effect = "ALLOW",
                        ScopeType = "DEPARTMENT",
                        ScopeKey = "2",
                        ValidFrom = DateTime.Now.AddDays(10),
                        ValidTo = DateTime.Now.AddDays(5) // Hết hạn trước bắt đầu!
                    }
                }
            };
            var resInvalidDates = _service.SaveSubjectScopeGrants(reqInvalidDates);
            Assert.IsFalse(resInvalidDates.Success);
            Assert.That(resInvalidDates.ValidationErrors.Any(e => e.Contains("sau ngày hiệu lực")), Is.True);

            // 5. Payload null
            var reqNullGrants = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                Grants = null
            };
            var resNullGrants = _service.SaveSubjectScopeGrants(reqNullGrants);
            Assert.IsFalse(resNullGrants.Success);
            StringAssert.Contains("null", resNullGrants.Message);
        }

        /// <summary>
        /// Ca 12: No-Op detection: Khi dữ liệu không có thay đổi, không tăng revision và không ghi audit giả.
        /// </summary>
        [Test]
        public void Case12_NoOpDetection_IdenticalGrants_MustNotIncrementRevision_AndNotWriteAudit()
        {
            // Thiết lập cấu hình ban đầu
            _repo.GrantsStore["USER:102"] = new List<AiScopeGrantRecord>
            {
                new AiScopeGrantRecord { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2", IsEnabled = true }
            };
            long initialRev = _repo.Revision;

            // Gửi yêu cầu lưu với đúng dữ liệu y hệt không có thay đổi
            var noOpReq = new SaveAiSubjectScopeGrantsRequest
            {
                SubjectType = "USER",
                SubjectId = 102,
                BaseRevision = initialRev,
                Grants = new List<SaveAiSubjectScopeGrantItemDto>
                {
                    new SaveAiSubjectScopeGrantItemDto { CapabilityCode = "EMPLOYEE_LOOKUP", Effect = "ALLOW", ScopeType = "DEPARTMENT", ScopeKey = "2" }
                }
            };

            var res = _service.SaveSubjectScopeGrants(noOpReq);
            Assert.IsTrue(res.Success);
            StringAssert.Contains("Không có thay đổi", res.Message);
            Assert.AreEqual(0, res.SavedCount, "Số lượng bản ghi thay đổi phải là 0.");
            Assert.AreEqual(initialRev, _repo.Revision, "Revision không được tăng khi không có thay đổi.");
            Assert.AreEqual(0, _repo.Audits.Count, "Không được ghi audit khi không có thay đổi.");
        }
    }
}
