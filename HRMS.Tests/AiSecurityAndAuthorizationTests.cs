using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Bu.Services.AI_Services.Security;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Tests
{
    [TestFixture]
    public class AiSecurityAndAuthorizationTests
    {
        private AiAuthorizationService _authService;

        [SetUp]
        public void SetUp()
        {
            _authService = new AiAuthorizationService();
        }

        [Test]
        public void CheckCapability_ShouldDeny_WhenContextIsAnonymous_A01()
        {
            var anon = AiAuthorizationContext.CreateAnonymous();
            var result = _authService.CheckCapability(anon, "EMPLOYEE_LOOKUP");

            Assert.IsFalse(result.IsAllowed, "Khách ẩn danh không được phép tra cứu capability.");
            Assert.IsNotEmpty(result.DenialReason);
        }

        [Test]
        public void CheckCapability_ShouldDeny_WhenFunctionRightIsMissing_A02()
        {
            var ctx = new AiAuthorizationContext
            {
                UserId = 10,
                Username = "user_no_payroll",
                IsAdmin = false,
                FunctionRights = new HashSet<string> { "F_CC_BANGCONG" } // Không có F_CC_BANGLUONG
            };

            var result = _authService.CheckCapability(ctx, "PAYROLL_VIEW");

            Assert.IsFalse(result.IsAllowed, "Tài khoản thiếu quyền F_CC_BANGLUONG phải bị từ chối.");
            Assert.IsNotEmpty(result.DenialReason);
        }

        [Test]
        public void CheckCapability_ShouldAllow_WhenUserHasRequiredRight()
        {
            var ctx = new AiAuthorizationContext
            {
                UserId = 12,
                Username = "hr_manager",
                IsAdmin = false,
                FunctionRights = new HashSet<string> { "F_DM_NHANVIEN" }
            };

            var result = _authService.CheckCapability(ctx, "EMPLOYEE_LOOKUP");

            Assert.IsTrue(result.IsAllowed);
            Assert.AreEqual("V_AI_EMPLOYEE_LOOKUP", result.SourceView);
        }

        [Test]
        public void CheckCapability_ShouldDeny_DataAccessWithoutExplicitAdminRights()
        {
            var ctx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true
            };

            var result = _authService.CheckCapability(ctx, "PAYROLL_VIEW");
            Assert.IsFalse(result.IsAllowed);
        }

        [Test]
        public void EvaluateScope_ShouldEnforceSelfScope_ForSelfOperations_A03()
        {
            var ctx = new AiAuthorizationContext
            {
                UserId = 20,
                Username = "emp20",
                Manv = 15,
                IsAdmin = false
            };

            var scopeResult = AiScopeEvaluator.BuildSqlScopeFilter(ctx, "PAYROLL_SELF");

            Assert.IsTrue(scopeResult.IsAllowed);
            StringAssert.Contains("MANV =", scopeResult.SqlPredicate);
            Assert.That(scopeResult.Parameters.Values, Does.Contain(15));
        }

        [Test]
        public void EvaluateScope_ShouldEnforceDepartmentScope_A04()
        {
            var ctx = new AiAuthorizationContext
            {
                UserId = 21,
                Username = "dept_manager",
                Manv = 10,
                AllowedDepartmentIds = new HashSet<int> { 2, 5 },
                IsAdmin = false
            };

            var scopeResult = AiScopeEvaluator.BuildSqlScopeFilter(ctx, "ATTENDANCE_DETAIL");

            Assert.IsTrue(scopeResult.IsAllowed);
            Assert.That(scopeResult.SqlPredicate, Does.Contain("IDPB ="));
            Assert.That(scopeResult.Parameters.Values, Does.Contain(2));
            Assert.That(scopeResult.Parameters.Values, Does.Contain(5));
        }

        [Test]
        public void IsEmployeeInScope_ShouldReturnFalse_WhenEmployeeNotInScope_A05()
        {
            var ctx = new AiAuthorizationContext
            {
                UserId = 22,
                Username = "emp22",
                Manv = 18,
                AllowedDepartmentIds = new HashSet<int> { 3 },
                IsAdmin = false
            };

            // Nhân viên #99 ở phòng ban 5 (không thuộc phòng 3, không phải #18)
            bool inScope = AiScopeEvaluator.IsEmployeeInScope(ctx, targetManv: 99, targetDeptId: 5);
            Assert.IsFalse(inScope, "Nhân sự ngoài phòng ban và khác MANV phải bị từ chối ngoài phạm vi.");
        }

        [Test]
        public void GetFieldAccessMode_ShouldDeny_PasswordsAndSecrets_A06()
        {
            Assert.AreEqual("DENY", _authService.GetFieldAccessMode("EMPLOYEE_LOOKUP", "PASSWORD"));
            Assert.AreEqual("DENY", _authService.GetFieldAccessMode("EMPLOYEE_LOOKUP", "MATKHAU"));
            Assert.AreEqual("DENY", _authService.GetFieldAccessMode("EMPLOYEE_LOOKUP", "TOKEN"));
            Assert.AreEqual("DENY", _authService.GetFieldAccessMode("EMPLOYEE_LOOKUP", "REFRESH_TOKEN"));
        }

        [Test]
        public void GetFieldAccessMode_ShouldMask_SensitiveFieldsForDefaultProfile()
        {
            // Hồ sơ cơ bản thì điện thoại/bảo hiểm bị MASK
            Assert.AreEqual("MASK", _authService.GetFieldAccessMode("EMPLOYEE_LOOKUP", "DIENTHOAI"));
            Assert.AreEqual("MASK", _authService.GetFieldAccessMode("EMPLOYEE_LOOKUP", "SOBH"));

            // Hồ sơ nhạy cảm (PROFILE) được FULL
            Assert.AreEqual("MASK", _authService.GetFieldAccessMode("EMPLOYEE_PROFILE", "DIENTHOAI"), "A profile name alone must not grant sensitive fields.");
        }

        [Test]
        public void OracleSqlAstValidator_ShouldAllow_All14V2Views()
        {
            string[] v2Views = new[]
            {
                "SELECT * FROM V_AI_EMPLOYEE",
                "SELECT * FROM V_AI_ATTENDANCE",
                "SELECT * FROM V_AI_OVERTIME",
                "SELECT * FROM V_AI_INSURANCE",
                "SELECT * FROM V_AI_ADVANCE",
                "SELECT * FROM V_AI_ALLOWANCE",
                "SELECT * FROM V_AI_EMPLOYEE_LOOKUP",
                "SELECT * FROM V_AI_ORG_LOOKUP",
                "SELECT * FROM V_AI_PERIOD",
                "SELECT * FROM V_AI_ATTENDANCE_SUMMARY",
                "SELECT * FROM V_AI_PAYROLL",
                "SELECT * FROM V_AI_PAYROLL_SUMMARY",
                "SELECT * FROM V_AI_CONTRACT",
                "SELECT * FROM V_AI_SALARY_CHANGE"
            };

            foreach (var q in v2Views)
            {
                var val = OracleSqlAstValidator.Validate(q);
                Assert.IsTrue(val.IsValid, $"View hợp lệ phải được thông qua: {q}. Lý do từ chối: {val.RejectionReason}");
            }
        }

        [Test]
        public void OracleSqlAstValidator_ShouldSupport_ColonParameters()
        {
            string query = "SELECT MANV, HOTEN FROM V_AI_EMPLOYEE_LOOKUP WHERE MANV = :p_scope_manv AND IDPB = :p_scope_dept";
            var val = OracleSqlAstValidator.Validate(query);

            Assert.IsTrue(val.IsValid, $"Câu lệnh chứa tham số :param phải hợp lệ. Lý do: {val.RejectionReason}");
            Assert.AreEqual(query, val.CleanedSql);
        }

        [Test]
        public void OracleSqlAstValidator_ShouldAllow_AiOwnerSchemaPrefix()
        {
            string query = "SELECT MANV, HOTEN FROM AI_OWNER.V_AI_EMPLOYEE_LOOKUP WHERE MANV = :p_scope_manv";
            var val = OracleSqlAstValidator.Validate(query);

            Assert.IsTrue(val.IsValid, $"Schema AI_OWNER phải được chấp thuận. Lý do từ chối: {val.RejectionReason}");
        }

        [Test]
        public void OracleSqlAstValidator_ShouldReject_HrSchemaPrefix()
        {
            string query = "SELECT MANV, HOTEN FROM HR.V_AI_EMPLOYEE_LOOKUP WHERE MANV = :p_scope_manv";
            var val = OracleSqlAstValidator.Validate(query);

            Assert.IsFalse(val.IsValid, "Schema HR cũ phải bị từ chối.");
            StringAssert.Contains("AI_OWNER", val.RejectionReason);
        }

        [Test]
        public void AiHmacProofService_ExplicitEmptyKey_ThrowsConfigurationErrorsException()
        {
            Assert.Throws<System.Configuration.ConfigurationErrorsException>(() => new AiHmacProofService("   "));
        }

        [Test]
        public void HmacProof_ValidProof_Generates64CharHexSignatureAndVerifiesSuccessfully()
        {
            var proofService = new AiHmacProofService("TEST_SECRET_KEY_FOR_UNIT_TESTING_12345");
            var proof = proofService.GenerateProof(141, "EMPLOYEE_LOOKUP", 30);

            Assert.AreEqual(141, proof.ActorUserId);
            Assert.AreEqual("EMPLOYEE_LOOKUP", proof.CapabilityOrAction);
            Assert.AreEqual("HRMS_AI_ORACLE", proof.Audience);
            Assert.IsNotEmpty(proof.Nonce);
            Assert.AreEqual(64, proof.SignatureHex.Length, "HMAC-SHA256 signature must be 64 uppercase hex characters");
            Assert.IsTrue(proofService.VerifyProof(proof));
        }

        [Test]
        public void HmacProof_TamperedActorOrCapability_FailsVerification()
        {
            var proofService = new AiHmacProofService("TEST_SECRET_KEY_FOR_UNIT_TESTING_12345");
            var proof = proofService.GenerateProof(141, "EMPLOYEE_LOOKUP", 30);

            // Mạo danh sang Actor khác
            proof.ActorUserId = 999;
            Assert.IsFalse(proofService.VerifyProof(proof), "Proof đã bị sửa actorId phải bị từ chối.");

            // Mạo danh sang Capability khác
            proof.ActorUserId = 141;
            proof.CapabilityOrAction = "PAYROLL_VIEW";
            Assert.IsFalse(proofService.VerifyProof(proof), "Proof đã bị sửa capability phải bị từ chối.");
        }

        [Test]
        public void HmacProof_ExpiredProof_FailsVerification()
        {
            var proofService = new AiHmacProofService("TEST_SECRET_KEY_FOR_UNIT_TESTING_12345");
            // Sinh proof với thời hạn -5 giây (đã hết hạn)
            var proof = proofService.GenerateProof(141, "EMPLOYEE_LOOKUP", -5);

            Assert.IsFalse(proofService.VerifyProof(proof), "Proof đã quá hạn phải bị từ chối.");
        }

        [Test]
        public void InsuranceResponse_MasksBothAnswerAndPublicApiTable_WithoutMutatingSource()
        {
            var plan = new QueryExecutionPlan { Domain = "INSURANCE", Strategy = ExecutionStrategy.SqlTemplate };
            plan.FieldModes["SOBH"] = "MASK"; plan.FieldModes["NOICAP"] = "DENY";
            var data = new System.Data.DataTable();
            data.Columns.Add("MANV", typeof(int)); data.Columns.Add("SOBH", typeof(string)); data.Columns.Add("NOICAP", typeof(string));
            data.Rows.Add(15, "DEMO_PRIVATE_NUMBER", "DEMO_PRIVATE_LOCATION");
            var response = DeterministicResponseRenderer.Render(plan, new SqlExecutionResult { Status = SqlExecutionStatus.SuccessWithData, Data = data });
            Assert.That(response.Answer, Does.Not.Contain("DEMO_PRIVATE_NUMBER"));
            Assert.AreEqual("***", response.Data.Rows[0]["SOBH"]);
            Assert.IsFalse(response.Data.Columns.Contains("NOICAP"));
            Assert.AreEqual("DEMO_PRIVATE_NUMBER", data.Rows[0]["SOBH"], "Internal evidence must remain unchanged.");
        }

        private static AiAuthorizationContext InsuranceContext()
        {
            var ctx = new AiAuthorizationContext { UserId = 20, Manv = 15, PolicyLoaded = true, IsAdmin = true };
            foreach (string code in new[] { "INSURANCE_SELF", "INSURANCE_VIEW" })
            {
                ctx.Capabilities[code] = new AiPolicyCapability { Enabled = true, SourceView = "V_AI_INSURANCE", RequiredFunctionCode = code == "INSURANCE_VIEW" ? "F_DM_NHANVIEN" : null };
                ctx.ScopeGrants.Add(new AiScopeGrant { CapabilityCode = code, ScopeType = "ALL", Effect = "ALLOW" });
                foreach (string field in new[] { "MANV", "HOTEN", "SOBH", "NGAYCAP", "NOICAP", "NOIKHAMBENH" })
                {
                    ctx.FieldPolicies[code + ":" + field] = code == "INSURANCE_VIEW" && field == "SOBH" ? "MASK" : "FULL";
                    ctx.FieldOperations[code + ":" + field] = "LOOKUP,VIEW,LIST,SELF,SELECT";
                }
            }
            return ctx;
        }

        [Test]
        public void InsuranceManagement_RequiresFunctionEvenForAdmin_AndPreservesExplicitDeny()
        {
            var ctx = InsuranceContext();
            Assert.IsFalse(_authService.CheckCapability(ctx, "INSURANCE_VIEW").IsAllowed);
            ctx.Capabilities["INSURANCE_VIEW"].RequiredFunctionCode = null;
            Assert.IsFalse(_authService.CheckCapability(ctx, "INSURANCE_VIEW").IsAllowed, "Database policy cannot remove the compiled function requirement.");
            ctx.FunctionRights.Add("F_DM_NHANVIEN");
            Assert.IsTrue(_authService.CheckCapability(ctx, "INSURANCE_VIEW").IsAllowed);
            ctx.ScopeGrants.Add(new AiScopeGrant { CapabilityCode = "INSURANCE_VIEW", ScopeType = "ALL", Effect = "DENY" });
            Assert.IsFalse(_authService.CheckCapability(ctx, "INSURANCE_VIEW").IsAllowed);
            Assert.IsFalse(AiScopeEvaluator.BuildSqlScopeFilter(ctx, "INSURANCE_VIEW").IsAllowed);
        }

        [Test]
        public void InsuranceManagement_RequiresExplicitScope_AndMasksInsuranceNumberInPlan()
        {
            var ctx = InsuranceContext(); ctx.FunctionRights.Add("F_DM_NHANVIEN"); ctx.ScopeGrants.Clear();
            var question = new QueryUnderstandingResult { Domain = "INSURANCE", Operation = "LOOKUP", RequestedScope = "ALL" };
            question.Entities.Add(new EntityMention { EntityType = "EMPLOYEE", ResolvedId = 99, Status = "RESOLVED" });
            var planner = new QueryPlanner();
            Assert.AreEqual(ExecutionStrategy.Forbidden, planner.CreatePlan(question, ctx).Strategy);
            ctx.ScopeGrants.Add(new AiScopeGrant { CapabilityCode = "INSURANCE_VIEW", ScopeType = "DEPARTMENT", ScopeKey = "2", Effect = "ALLOW" });
            question.Entities[0].DepartmentId = 3;
            Assert.AreEqual(ExecutionStrategy.Forbidden, planner.CreatePlan(question, ctx).Strategy, "Other departments remain denied.");
            question.Entities[0].DepartmentId = 2;
            var plan = planner.CreatePlan(question, ctx);
            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy, plan.DenialOrUnsupportedReason);
            Assert.That(plan.MaskedFields, Does.Contain("SOBH"));
        }

        [Test]
        public void InsuranceSelf_NeverExpandsBeyondMappedEmployee_AndDeniesMissingIdentity()
        {
            var ctx = InsuranceContext();
            Assert.IsTrue(AiScopeEvaluator.IsEmployeeInScope(ctx, 15, capabilityCode: "INSURANCE_SELF"));
            Assert.IsFalse(AiScopeEvaluator.IsEmployeeInScope(ctx, 99, capabilityCode: "INSURANCE_SELF"));
            var scope = AiScopeEvaluator.BuildSqlScopeFilter(ctx, "INSURANCE_SELF");
            Assert.That(scope.SqlPredicate, Does.Contain("MANV = :p_scope_self"));
            Assert.AreEqual(15, scope.Parameters[":p_scope_self"]);
            ctx.Manv = null;
            Assert.IsFalse(AiScopeEvaluator.BuildSqlScopeFilter(ctx, "INSURANCE_SELF").IsAllowed);
        }

        [Test]
        public void AvailableSourceProfile_Enables13CapabilitiesAndKeeps4UnavailableCapabilitiesDisabled()
        {
            var previous = AiCapabilityCatalog.GetAll().ToDictionary(c => c.CapabilityCode, c => c.IsEnabled);
            try
            {
                AiCapabilityCatalog.ApplyBranchB2Profile();

                // 13 available capabilities
                string[] b2Active = new[]
                {
                    "EMPLOYEE_LOOKUP", "EMPLOYEE_PROFILE", "EMPLOYEE_COUNT",
                    "ATTENDANCE_SUMMARY", "OVERTIME_VIEW", "OVERTIME_SUM",
                    "ALLOWANCE_VIEW", "INSURANCE_SELF", "ADVANCE_VIEW",
                    "CONTRACT_VIEW", "SALARY_CHANGE_VIEW", "INSURANCE_VIEW", "POLICY_LOOKUP"
                };

                // 4 unavailable protected sources
                string[] b2Disabled = new[]
                {
                    "PAYROLL_SELF", "PAYROLL_VIEW", "PAYROLL_SUMMARY",
                    "ATTENDANCE_DETAIL"
                };

                foreach (var code in b2Active)
                {
                    Assert.IsTrue(AiCapabilityCatalog.Get(code)?.IsEnabled, $"{code} phải bật trong Nhánh B2.");
                }

                foreach (var code in b2Disabled)
                {
                    Assert.IsFalse(AiCapabilityCatalog.Get(code)?.IsEnabled, $"{code} phải tắt trong Nhánh B2.");
                }
            }
            finally
            {
                // Restore for other regression suites
                foreach (var state in previous) AiCapabilityCatalog.SetEnabled(state.Key, state.Value);
            }
        }
    }
}
