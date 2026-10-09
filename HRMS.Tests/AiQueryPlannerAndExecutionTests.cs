using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using NUnit.Framework;
using Bu.Tests;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiQueryPlannerAndExecutionTests
    {
        private FakeClockProvider _fakeClock;
        private QueryPlanner _planner;
        private AiAuthorizationContext _adminCtx;
        private AiAuthorizationContext _managerItCtx;
        private AiAuthorizationContext _employeeSelfCtx;

        [SetUp]
        public void SetUp()
        {
            _fakeClock = new FakeClockProvider(new DateTime(2026, 10, 3, 10, 0, 0));
            _planner = new QueryPlanner(_fakeClock);

            // 1. Quản trị viên
            _adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            // 2. Trưởng phòng IT (Quản lý phòng 2)
            _managerItCtx = new AiAuthorizationContext
            {
                UserId = 2,
                Username = "manager_it",
                Manv = 100,
                IsAdmin = false,
                AllowedDepartmentIds = new HashSet<int> { 2 }
            };
            _managerItCtx.FunctionRights.Add("F_DM_NHANVIEN");
            _managerItCtx.FunctionRights.Add("F_CC_TANGCA");
            _managerItCtx.FunctionRights.Add("F_CC_PHUCAP");
            _managerItCtx.FunctionRights.Add("F_CC_BANGLUONG");
            _managerItCtx.FunctionRights.Add("F_NV_HOPDONG");

            // 3. Nhân viên bình thường chỉ có quyền SELF
            _employeeSelfCtx = new AiAuthorizationContext
            {
                UserId = 3,
                Username = "staff_an",
                Manv = 10,
                IsAdmin = false
            };
            _employeeSelfCtx.FunctionRights.Add("F_CC_TANGCA");
        }

        #region Planner Tests

        [Test]
        public void CreatePlan_U01_OvertimeWithEmployeeAndMonth_GeneratesParameterizedSql()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "OVERTIME",
                Operation = "VIEW",
                SupportStatus = QuerySupportStatus.Supported,
                Entities = new List<EntityMention>
                {
                    new EntityMention { EntityType = "EMPLOYEE", MentionText = "nhân viên mã 10", ResolvedId = 10, Status = "RESOLVED", ResolvedName = "Nguyễn Văn An" }
                },
                Time = new TimeResolution { Month = 9, Year = 2026 }
            };

            var plan = _planner.CreatePlan(understood, _adminCtx);

            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy);
            Assert.AreEqual("V_AI_OVERTIME", plan.TargetView);
            Assert.IsTrue(plan.SqlStatement.Contains("MANV = :p_manv"));
            Assert.IsTrue(plan.SqlStatement.Contains("THANG = :p_thang"));
            Assert.IsTrue(plan.SqlStatement.Contains("NAM = :p_nam"));
            Assert.AreEqual(10, plan.Parameters[":p_manv"]);
            Assert.AreEqual(9, plan.Parameters[":p_thang"]);
            Assert.AreEqual(2026, plan.Parameters[":p_nam"]);
        }

        [Test]
        public void CreatePlan_U02_AllowanceUnder1Million_GeneratesLessThanOperator()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "ALLOWANCE",
                Operation = "VIEW",
                SupportStatus = QuerySupportStatus.Supported,
                Filters = new List<FilterCondition>
                {
                    new FilterCondition { Field = "SOTIEN", Operator = "<", Value = 1000000m }
                }
            };

            var plan = _planner.CreatePlan(understood, _managerItCtx);

            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy);
            Assert.AreEqual("V_AI_ALLOWANCE", plan.TargetView);
            Assert.IsTrue(plan.SqlStatement.Contains("SOTIEN < :p_sotien"));
            Assert.IsFalse(plan.SqlStatement.Contains("SOTIEN >="));
            Assert.AreEqual(1000000m, plan.Parameters[":p_sotien"]);
        }

        [Test]
        public void CreatePlan_U03_AllowanceAbove1Point5Million_GeneratesGreaterThanOperator()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "ALLOWANCE",
                Operation = "VIEW",
                SupportStatus = QuerySupportStatus.Supported,
                Filters = new List<FilterCondition>
                {
                    new FilterCondition { Field = "SOTIEN", Operator = ">", Value = 1500000m }
                }
            };

            var plan = _planner.CreatePlan(understood, _managerItCtx);

            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy);
            Assert.AreEqual("V_AI_ALLOWANCE", plan.TargetView);
            Assert.IsTrue(plan.SqlStatement.Contains("SOTIEN > :p_sotien"));
            Assert.AreEqual(1500000m, plan.Parameters[":p_sotien"]);
        }

        [Test]
        public void CreatePlan_U04_BirthdayMonth12_GeneratesMonthExtraction()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "EMPLOYEE",
                Operation = "LIST",
                Metric = "BIRTHDAY",
                SupportStatus = QuerySupportStatus.Supported,
                Time = new TimeResolution { BirthdayMonth = 12 }
            };

            var plan = _planner.CreatePlan(understood, _adminCtx);

            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy);
            Assert.AreEqual("V_AI_EMPLOYEE", plan.TargetView);
            Assert.IsTrue(plan.SqlStatement.Contains("BIRTHDAY_MONTH = :p_bday_month"));
            Assert.AreEqual(12, plan.Parameters[":p_bday_month"]);
        }

        [Test]
        public void CreatePlan_U05_PayrollSummary_RequiresAdminOrPayrollView()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "PAYROLL",
                Operation = "SUM",
                SupportStatus = QuerySupportStatus.Supported,
                Time = new TimeResolution { Month = 9, Year = 2026 }
            };

            // Manager IT có F_BANGLUONG_VIEW -> Được phép
            var planManager = _planner.CreatePlan(understood, _managerItCtx);
            Assert.AreEqual(ExecutionStrategy.SqlTemplate, planManager.Strategy);
            Assert.AreEqual("V_AI_PAYROLL_SUMMARY", planManager.TargetView);
            Assert.IsTrue(planManager.IsScalar);

            // Staff An chỉ có SELF, không có F_BANGLUONG_VIEW -> Bị từ chối (403)
            var planStaff = _planner.CreatePlan(understood, _employeeSelfCtx);
            Assert.AreEqual(ExecutionStrategy.Forbidden, planStaff.Strategy);
        }

        [Test]
        public void CreatePlan_U18_TopOvertime_GeneratesSumGroupAndRank()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "OVERTIME",
                Operation = "TOP",
                SupportStatus = QuerySupportStatus.Supported,
                Time = new TimeResolution { Month = 9, Year = 2026 }
            };

            var plan = _planner.CreatePlan(understood, _adminCtx);

            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy);
            Assert.IsTrue(plan.SqlStatement.Contains("GROUP BY MANV,HOTEN,TEN_PHONGBAN"));
            Assert.IsTrue(plan.SqlStatement.Contains("ORDER BY TONG_SOGIO DESC"));
            Assert.IsTrue(plan.SqlStatement.Contains("FETCH FIRST 6 ROWS ONLY"));
        }

        [Test]
        public void CreatePlan_A03_SelfQueriesOtherPayroll_IsForbidden()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "PAYROLL",
                Operation = "LOOKUP",
                SupportStatus = QuerySupportStatus.Supported,
                Entities = new List<EntityMention>
                {
                    new EntityMention { EntityType = "EMPLOYEE", MentionText = "Nguyễn Văn B", ResolvedId = 20, Status = "RESOLVED" }
                },
                Time = new TimeResolution { Month = 9, Year = 2026 }
            };

            // Staff An (MANV = 10) tra cứu lương của MANV = 20 -> Bị chặn ngay lập tức
            var plan = _planner.CreatePlan(understood, _employeeSelfCtx);

            Assert.AreEqual(ExecutionStrategy.Forbidden, plan.Strategy);
            Assert.IsNull(plan.SqlStatement);
        }

        [Test]
        public void CreatePlan_A16_ContractExpiring30Days_GeneratesCorrectBoundary()
        {
            var understood = new QueryUnderstandingResult
            {
                Domain = "CONTRACT",
                Operation = "EXPIRING",
                OriginalQuestion = "Hợp đồng sắp hết hạn trong 30 ngày",
                SupportStatus = QuerySupportStatus.Supported
            };

            var plan = _planner.CreatePlan(understood, _adminCtx);

            Assert.AreEqual(ExecutionStrategy.SqlTemplate, plan.Strategy);
            Assert.AreEqual("V_AI_CONTRACT", plan.TargetView);
            Assert.IsFalse(plan.SqlStatement.Contains("IS_DELETED"), "Deletion filtering belongs to the protected view over DEL_DATE.");
            Assert.IsTrue(plan.SqlStatement.Contains("NGAYKETTHUC >= :p_today"));
            Assert.IsTrue(plan.SqlStatement.Contains("NGAYKETTHUC < :p_limit_date"));

            var today = (DateTime)plan.Parameters[":p_today"];
            var limitDate = (DateTime)plan.Parameters[":p_limit_date"];
            Assert.AreEqual(31, (limitDate - today).TotalDays, "The exclusive end includes the full 30th day after today.");
        }

        #endregion

        #region Deterministic Renderer Tests

        [Test]
        public void Render_ScalarOvertimeSum_FormatsVietnameseHoursAndProvenance_C20()
        {
            var plan = new QueryExecutionPlan
            {
                Domain = "OVERTIME",
                Operation = "SUM",
                IsScalar = true,
                ScalarUnit = "giờ",
                SelectedEntityDisplay = "Nguyễn Văn An",
                EffectivePeriodDisplay = "09/2026",
                TargetView = "V_AI_TANGCA"
            };

            var dt = new DataTable();
            dt.Columns.Add("MANV", typeof(int));
            dt.Columns.Add("HOTEN", typeof(string));
            dt.Columns.Add("TENPB", typeof(string));
            dt.Columns.Add("TONG_SOGIO", typeof(decimal));
            dt.Rows.Add(10, "Nguyễn Văn An", "Phòng IT", 18.5m);

            var sqlResult = new SqlExecutionResult
            {
                Status = SqlExecutionStatus.SuccessWithData,
                Data = dt
            };

            var response = DeterministicResponseRenderer.Render(plan, sqlResult);

            Assert.AreEqual("answered", response.Status);
            Assert.IsTrue(response.BypassedLlm);
            Assert.IsTrue(response.Answer.Contains("18,5 giờ"));
            Assert.IsTrue(response.Answer.Contains("Nguyễn Văn An"));
            Assert.IsTrue(response.Answer.Contains("09/2026"));
            Assert.IsTrue(response.Answer.Contains("Đăng ký tăng ca"));
            Assert.IsFalse(response.Answer.Contains("V_AI_"));
        }

        [Test]
        public void Render_PayrollDetail_DoesNotCallApprovedAsPaid()
        {
            var plan = new QueryExecutionPlan { Domain = "PAYROLL", Operation = "VIEW", EffectivePeriodDisplay = "09/2026", TargetView = "V_AI_PAYROLL" };
            var dt = new DataTable();
            dt.Columns.Add("THUCLANH", typeof(decimal));
            dt.Columns.Add("TRANGTHAI_CHITRA", typeof(string));
            dt.Columns.Add("IS_LOCKED", typeof(int));
            dt.Rows.Add(5100000m, "APPROVED", 1);
            var response = DeterministicResponseRenderer.Render(plan, new SqlExecutionResult { Status = SqlExecutionStatus.SuccessWithData, Data = dt });
            Assert.AreEqual("answered", response.Status);
            StringAssert.Contains("Đã duyệt; chưa xác nhận chi trả", response.Answer);
            StringAssert.Contains("Đã khóa sổ", response.Answer);
            StringAssert.DoesNotContain("Đã chi trả", response.Answer);
        }
        [Test]
        public void Render_SqlError_ReturnsErrorAndNeverRegurgitatesUnrelatedFaq_A17()
        {
            var plan = new QueryExecutionPlan
            {
                Domain = "OVERTIME",
                Operation = "VIEW",
                TargetView = "V_AI_TANGCA"
            };

            var sqlResult = new SqlExecutionResult
            {
                Status = SqlExecutionStatus.ExecutionError,
                ErrorMessage = "ORA-00942: table or view does not exist"
            };

            var response = DeterministicResponseRenderer.Render(plan, sqlResult);

            Assert.AreEqual("error", response.Status);
            Assert.AreEqual(500, response.HttpStatus);
            Assert.IsFalse(response.Answer.Contains("quy chế"));
            Assert.IsFalse(response.Answer.Contains("ORA-00942")); // Không để lộ raw SQL error
        }

        [Test]
        public void Render_SuccessEmpty_ReturnsNoDataStatus_A17()
        {
            var plan = new QueryExecutionPlan
            {
                Domain = "OVERTIME",
                Operation = "VIEW",
                SelectedEntityDisplay = "Nguyễn Văn An",
                EffectivePeriodDisplay = "09/2026",
                TargetView = "V_AI_TANGCA"
            };

            var sqlResult = new SqlExecutionResult
            {
                Status = SqlExecutionStatus.SuccessEmpty,
                Data = new DataTable()
            };

            var response = DeterministicResponseRenderer.Render(plan, sqlResult);

            Assert.AreEqual("no_data", response.Status);
            Assert.AreEqual(0, response.TotalRecords);
            Assert.IsTrue(response.Answer.Contains("Nguyễn Văn An"));
        }

        #endregion

        #region End-to-End AiExecutionService Tests

        [Test]
        public async Task ProcessChatAsync_ScalarOvertimeQuery_ExecutesEndToEndWithoutLlm()
        {
            var fakeExecutor = new FakeScopedSqlExecutor();
            var fakeUnderstanding = new QueryUnderstandingService(_fakeClock, new EntityResolver(new FakeEntityLookupProvider()));
            var service = new AiExecutionService(
                understandingService: fakeUnderstanding,
                sqlExecutor: fakeExecutor,
                clock: _fakeClock);

            var res = await service.ProcessChatAsync(
                "Nguyễn Văn An tăng ca bao nhiêu giờ tháng 9/2026?", 
                _adminCtx,
                conversationId: "conv_test_1");

            Assert.IsNotNull(res);
            Assert.AreEqual("answered", res.Status);
            Assert.IsTrue(res.BypassedLlm);
            Assert.IsTrue(res.Answer.Contains("giờ"));
            Assert.AreEqual("conv_test_1", res.ConversationId);
            Assert.IsTrue(res.ConversationVersion >= 1);
        }

        [Test]
        public async Task ProcessChatAsync_CancelledToken_ThrowsOrReturnsErrorWithoutMutatingState()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            var service = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: new HRMS.Tests.RegressionSqlExecutor(), clock: _fakeClock);

            var res = await service.ProcessChatAsync(
                "Ai ở phòng IT?", 
                _adminCtx, 
                conversationId: "conv_cancel_test",
                cancellationToken: cts.Token);

            Assert.AreEqual("error", res.Status);
            Assert.IsTrue(res.Answer.Contains("hủy"));
        }

        #endregion

        private class FakeScopedSqlExecutor : IScopedSqlExecutor
        {
            public SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                var dt = new DataTable();
                dt.Columns.Add("MANV", typeof(int));
                dt.Columns.Add("HOTEN", typeof(string));
                dt.Columns.Add("TENPB", typeof(string));
                dt.Columns.Add("TONG_SOGIO", typeof(decimal));
                dt.Rows.Add(10, "Nguyễn Văn An", "Phòng IT", 12.0m);

                return new SqlExecutionResult
                {
                    Status = SqlExecutionStatus.SuccessWithData,
                    Data = dt,
                    TotalRecords = 1,
                    SourceProvenance = "V_AI_TANGCA"
                };
            }

            public Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ExecuteScopedQuery(sql, parameters, cancellationToken));
            }
        }
    }
}
