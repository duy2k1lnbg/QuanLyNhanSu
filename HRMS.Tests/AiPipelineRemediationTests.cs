using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Retrieval.Sql;
using Bu.Services.AI_Services.Security;
using Bu.Services.AI_Services.Vector;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiPipelineRemediationTests
    {
        [Test]
        public void Test_CloneForSubPlan_Preserves_RowLimit_FieldModes_And_Revision()
        {
            var plan = new QueryExecutionPlan
            {
                Strategy = ExecutionStrategy.Hybrid,
                Domain = "OVERTIME",
                Operation = "TOP",
                Metric = "SUM",
                TargetView = "V_AI_OVERTIME",
                SqlStatement = "SELECT MANV, SOGIO FROM AI_OWNER.V_AI_OVERTIME",
                RowLimit = 5,
                SourceRevision = 12345,
                AuthorizationFingerprint = "fingerprint-test",
                RequiredCapability = "OVERTIME_SUM"
            };
            plan.FieldModes["SOGIO"] = "FULL";
            plan.FieldModes["MANV"] = "FULL";
            plan.MaskedFields.Add("HOTEN");

            var subPlan = plan.CloneForSubPlan(ExecutionStrategy.SqlTemplate);

            Assert.That(subPlan.Strategy, Is.EqualTo(ExecutionStrategy.SqlTemplate));
            Assert.That(subPlan.Domain, Is.EqualTo("OVERTIME"));
            Assert.That(subPlan.RowLimit, Is.EqualTo(5), "F03 Fix: RowLimit must be preserved in subplan");
            Assert.That(subPlan.SourceRevision, Is.EqualTo(12345));
            Assert.That(subPlan.FieldModes.Count, Is.EqualTo(2));
            Assert.That(subPlan.MaskedFields.Contains("HOTEN"), Is.True);
        }

        [Test]
        public void Test_QueryUnderstanding_Detects_Policy_Intent_And_Purity()
        {
            var service = new QueryUnderstandingService();
            var ctx = new AiAuthorizationContext { UserId = 1, PolicyLoaded = true };

            // 1. Câu hỏi thuần túy về chính sách quy chế
            var pureResult = service.UnderstandQuery("Quy chế làm thêm giờ và nghỉ phép của công ty như thế nào?", ctx);
            Assert.That(pureResult.HasPolicyIntent, Is.True);
            Assert.That(pureResult.IsPolicyPure, Is.True);
            Assert.That(pureResult.Domain, Is.EqualTo("POLICY"));

            // 2. Câu hỏi kết hợp số liệu thực tế và chính sách
            var hybridResult = service.UnderstandQuery("Tôi đã làm thêm bao nhiêu giờ trong tháng 8/2024 theo quy định công ty?", ctx);
            Assert.That(hybridResult.HasPolicyIntent, Is.True);
            Assert.That(hybridResult.IsPolicyPure, Is.False, "Questions asking for hours/numbers are hybrid, not pure policy");
            Assert.That(hybridResult.Domain, Is.EqualTo("OVERTIME"));
        }

        [Test]
        public void Test_QueryPlanner_Routes_Pure_Policy_To_VectorSearch()
        {
            var planner = new QueryPlanner();
            var ctx = new AiAuthorizationContext
            {
                UserId = 1,
                Manv = 181,
                PolicyLoaded = true,
                FunctionRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "F_SYSTEM_AI" }
            };
            ctx.Capabilities["POLICY_LOOKUP"] = new AiPolicyCapability { CapabilityCode = "POLICY_LOOKUP", Enabled = true, SourceView = "hrms_vectors_v2" };

            var pureUnderstood = new QueryUnderstandingResult
            {
                Domain = "POLICY",
                IsPolicyPure = true,
                HasPolicyIntent = true,
                OriginalQuestion = "Nội quy thời gian làm việc",
                SupportStatus = QuerySupportStatus.Supported
            };

            var plan = planner.CreatePlan(pureUnderstood, ctx);

            Assert.That(plan.Strategy, Is.EqualTo(ExecutionStrategy.VectorSearch));
            Assert.That(plan.VectorCapability, Is.EqualTo("POLICY_LOOKUP"));
        }

        [Test]
        public void Test_Qdrant_GeneratePointId_IsDeterministic_UUIDv5()
        {
            string id1 = QdrantService.GeneratePointId("EMPLOYEE", "181");
            string id2 = QdrantService.GeneratePointId("EMPLOYEE", "181");
            string idDiff = QdrantService.GeneratePointId("EMPLOYEE", "182");

            Assert.That(id1, Is.EqualTo(id2), "UUID v5 must be completely deterministic for identical inputs");
            Assert.That(id1, Is.Not.EqualTo(idDiff));

            // Verify GUID format
            Assert.That(Guid.TryParse(id1, out var parsedGuid), Is.True);

            // Verify RFC 4122 version 5: 13th hex char is '5'
            string rawGuid = parsedGuid.ToString("D");
            Assert.That(rawGuid[14], Is.EqualTo('5'), "Point ID must be UUID version 5");
        }

        [Test]
        public void Test_AiSchemaCatalog_Contains_14_Standard_Views()
        {
            var views = AiSchemaCatalog.GetAllViews().ToList();
            Assert.That(views.Count, Is.GreaterThanOrEqualTo(14));

            var payrollView = AiSchemaCatalog.GetView("V_AI_PAYROLL");
            Assert.That(payrollView, Is.Not.Null);
            Assert.That(payrollView.Domain, Is.EqualTo("PAYROLL"));
            Assert.That(payrollView.RequiredCapability, Is.EqualTo("PAYROLL_VIEW"));
            Assert.That(payrollView.Columns.Any(c => c.Name == "THUCLANH" && c.IsMaskedByDefault), Is.True);

            var doc = AiSchemaCatalog.GenerateFullSchemaDocumentation();
            Assert.That(doc, Does.Contain("V_AI_EMPLOYEE_LOOKUP"));
            Assert.That(doc, Does.Contain("V_AI_CONTRACT"));
            Assert.That(doc, Does.Contain("V_AI_SALARY_CHANGE"));
        }

        [Test]
        public void Test_VectorSearchStatus_Enum_Typing()
        {
            var unavailableResult = new VectorSearchResult
            {
                Status = VectorSearchStatus.Unavailable,
                ErrorMessage = "Cannot contact Qdrant"
            };

            var forbiddenResult = new VectorSearchResult
            {
                Status = VectorSearchStatus.Forbidden,
                ErrorMessage = "Access denied"
            };

            Assert.That(unavailableResult.Status, Is.EqualTo(VectorSearchStatus.Unavailable));
            Assert.That(forbiddenResult.Status, Is.EqualTo(VectorSearchStatus.Forbidden));
            Assert.That(unavailableResult.Status, Is.Not.EqualTo(VectorSearchStatus.NoMatch), "Unavailable must not be treated as NoMatch (F08)");
        }
    }
}
