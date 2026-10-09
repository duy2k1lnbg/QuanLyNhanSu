using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using Bu.Tests;
using NUnit.Framework;

namespace HRMS.Tests
{
    public class MockEntityLookupForDuy : IEntityLookupProvider
    {
        private readonly List<EntityCandidate> _candidates;

        public MockEntityLookupForDuy(List<EntityCandidate> candidates = null)
        {
            _candidates = candidates ?? new List<EntityCandidate>
            {
                new EntityCandidate { Id = 12, Code = "12", Name = "Nguyễn Văn Duy", DepartmentId = 1, CompanyCode = "1", DepartmentName = "Phòng IT", PositionName = "Kỹ sư phần mềm" }
            };
        }

        public List<EntityCandidate> FindEmployees(string query, AiAuthorizationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<EntityCandidate>();
            return _candidates.Where(c => c.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public EntityCandidate FindEmployeeById(int manv, AiAuthorizationContext ctx)
        {
            return _candidates.FirstOrDefault(c => c.Id == manv);
        }

        public EntityCandidate FindDepartmentByName(string deptName, AiAuthorizationContext ctx) => null;
    }

    public class MockPayrollSqlExecutor : IScopedSqlExecutor, IAuthorizedSqlExecutor
    {
        private readonly bool _hasPayrollData;
        private readonly decimal _totalSalary;
        private readonly int _empCount;

        public MockPayrollSqlExecutor(bool hasPayrollData = true, decimal totalSalary = 150000000m, int empCount = 10)
        {
            _hasPayrollData = hasPayrollData;
            _totalSalary = totalSalary;
            _empCount = empCount;
        }

        public Task<SqlExecutionResult> ExecutePlanAsync(QueryExecutionPlan plan, AiAuthorizationContext ctx, CancellationToken cancellationToken = default)
        {
            if (plan.TargetView == "V_AI_PAYROLL_SUMMARY")
            {
                var dt = new DataTable();
                dt.Columns.Add("TONG_THUCLANH", typeof(decimal));
                dt.Columns.Add("SO_NHANVIEN", typeof(int));

                if (_hasPayrollData)
                {
                    dt.Rows.Add(_totalSalary, _empCount);
                    return Task.FromResult(new SqlExecutionResult
                    {
                        Status = SqlExecutionStatus.SuccessWithData,
                        Data = dt,
                        TotalRecords = 1,
                        SourceProvenance = "V_AI_PAYROLL_SUMMARY"
                    });
                }
                else
                {
                    return Task.FromResult(new SqlExecutionResult
                    {
                        Status = SqlExecutionStatus.SuccessEmpty,
                        Data = dt,
                        TotalRecords = 0,
                        SourceProvenance = "V_AI_PAYROLL_SUMMARY"
                    });
                }
            }

            if (plan.TargetView == "V_AI_EMPLOYEE_LOOKUP")
            {
                var dt = new DataTable();
                dt.Columns.Add("MANV", typeof(int));
                dt.Columns.Add("HOTEN", typeof(string));
                dt.Columns.Add("TEN_PHONGBAN", typeof(string));
                dt.Columns.Add("TEN_CHUCVU", typeof(string));

                if (plan.Parameters.ContainsKey(":p_manv"))
                {
                    dt.Rows.Add(12, "Nguyễn Văn Duy", "Phòng IT", "Kỹ sư phần mềm");
                }
                else
                {
                    // Multi-employee list (e.g. 15 employees)
                    for (int i = 1; i <= 15; i++)
                    {
                        dt.Rows.Add(i, $"Nhân viên {i}", "Phòng Kỹ Thuật", "Nhân viên");
                    }
                }

                return Task.FromResult(new SqlExecutionResult
                {
                    Status = SqlExecutionStatus.SuccessWithData,
                    Data = dt,
                    TotalRecords = dt.Rows.Count,
                    SourceProvenance = "V_AI_EMPLOYEE_LOOKUP"
                });
            }

            return Task.FromResult(new SqlExecutionResult { Status = SqlExecutionStatus.ExecutionError });
        }

        public Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SqlExecutionResult { Status = SqlExecutionStatus.ExecutionError });

        public SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default) =>
            new SqlExecutionResult { Status = SqlExecutionStatus.ExecutionError };
    }

    [TestFixture]
    public class AntigravityUnifiedAiAndPermissionsVerificationTests
    {
        private FakeClockProvider _clock;

        [SetUp]
        public void Setup()
        {
            _clock = new FakeClockProvider(new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc));
        }

        // =========================================================================
        // QUESTION 1: "có nhân viên nào tên duy không"
        // =========================================================================
        [Test]
        public async Task Query1_CoNhanVienNaoTenDuyKhong_ResolvesEmployeeEntityDuy_WhenFound()
        {
            var lookup = new MockEntityLookupForDuy();
            var resolver = new EntityResolver(lookup);
            var understanding = new QueryUnderstandingService(_clock, resolver);
            var sqlExec = new MockPayrollSqlExecutor();
            var sessions = new ConversationStateManager(_clock);
            var cache = new AiCacheCoordinator(_clock);

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExec,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: _clock
            );

            var ctx = AiTestContexts.All();
            var res = await service.ProcessChatAsync("có nhân viên nào tên duy không", ctx, "conv_duy_test");

            Assert.IsNotNull(res);
            Assert.AreEqual("answered", res.Status);
            StringAssert.Contains("Nguyễn Văn Duy", res.Answer, "Câu trả lời phải xác nhận thông tin nhân viên Nguyễn Văn Duy tìm thấy.");
            Assert.IsNotNull(res.InterpretedRequest);
            Assert.AreEqual("EMPLOYEE", res.InterpretedRequest.Domain);
            StringAssert.Contains("Duy", res.InterpretedRequest.EntityDisplay);
        }

        [Test]
        public async Task Query1_CoNhanVienNaoTenDuyKhong_ReturnsClearMessage_WhenNotFound()
        {
            // Trường hợp không có nhân viên nào tên Duy trong công ty
            var emptyLookup = new MockEntityLookupForDuy(new List<EntityCandidate>());
            var resolver = new EntityResolver(emptyLookup);
            var understanding = new QueryUnderstandingService(_clock, resolver);
            var sqlExec = new MockPayrollSqlExecutor();
            var sessions = new ConversationStateManager(_clock);
            var cache = new AiCacheCoordinator(_clock);

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExec,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: _clock
            );

            var ctx = AiTestContexts.All();
            var res = await service.ProcessChatAsync("có nhân viên nào tên duy không", ctx, "conv_duy_empty");

            Assert.IsNotNull(res);
            Assert.AreEqual("unsupported", res.Status);
            StringAssert.Contains("duy", res.Answer.ToLower(), "Phải thông báo rõ không tìm thấy nhân viên tên Duy trong phạm vi được phép.");
        }

        // =========================================================================
        // QUESTION 2: "Danh sách tất cả nhân viên trong công ty?"
        // =========================================================================
        [Test]
        public async Task Query2_DanhSachTatCaNhanVien_ReturnsTableData_And_BypassesLlm_ToAvoidTimeout()
        {
            var lookup = new MockEntityLookupForDuy();
            var resolver = new EntityResolver(lookup);
            var understanding = new QueryUnderstandingService(_clock, resolver);
            var sqlExec = new MockPayrollSqlExecutor();
            var sessions = new ConversationStateManager(_clock);
            var cache = new AiCacheCoordinator(_clock);
            var fakeLlm = new MockFakeLlmService("Bản tóm tắt văn xuôi nhân sự...");

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExec,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: _clock,
                llmService: fakeLlm
            );

            var ctx = AiTestContexts.All();
            var res = await service.ProcessChatAsync("Danh sách tất cả nhân viên trong công ty?", ctx, "conv_list_test");

            Assert.IsNotNull(res);
            Assert.AreEqual("answered", res.Status);
            Assert.IsTrue(res.BypassedLlm, "Danh sách bảng nhiều dòng phải bỏ qua LLM (BypassedLlm = true) để ngăn ngừa timeout 35s.");
            Assert.IsNotNull(res.Data, "Phải trả về DataTable dữ liệu nguồn cho giao diện.");
            Assert.Greater(res.Data.Rows.Count, 1, "Bảng nhân sự phải chứa danh sách các nhân viên.");
            StringAssert.Contains("Nhân viên 1", res.Answer);
        }

        // =========================================================================
        // QUESTION 3: "Tổng quỹ lương tháng này là bao nhiêu?"
        // =========================================================================
        [Test]
        public async Task Query3_TongQuyLuongThangNay_ReturnsSum_WhenAdminHasScopeAndData()
        {
            var understanding = new QueryUnderstandingService(_clock, new EntityResolver(new MockEntityLookupForDuy()));
            var sqlExec = new MockPayrollSqlExecutor(hasPayrollData: true, totalSalary: 150000000m, empCount: 10);
            var sessions = new ConversationStateManager(_clock);
            var cache = new AiCacheCoordinator(_clock);

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExec,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: _clock
            );

            // Context có đầy đủ quyền F_CC_BANGLUONG và Scope PAYROLL_SUMMARY
            var ctx = AiTestContexts.All();
            ctx.Capabilities["PAYROLL_SUMMARY"] = new AiPolicyCapability
            {
                CapabilityCode = "PAYROLL_SUMMARY",
                RequiredFunctionCode = "F_CC_BANGLUONG",
                SourceView = "V_AI_PAYROLL_SUMMARY",
                Enabled = true
            };
            ctx.FieldPolicies["PAYROLL_SUMMARY:TONG_THUCLANH"] = "FULL";
            ctx.FieldPolicies["PAYROLL_SUMMARY:SO_NHANVIEN"] = "FULL";
            ctx.ScopeGrants.Add(new AiScopeGrant { CapabilityCode = "PAYROLL_SUMMARY", ScopeType = "ALL", Effect = "ALLOW" });

            // Turn 1: Hỏi "Tổng quỹ lương tháng này là bao nhiêu?" -> AI yêu cầu làm rõ khoản lương nào
            var promptRes = await service.ProcessChatAsync("Tổng quỹ lương tháng này là bao nhiêu?", ctx, "conv_payroll_test");
            Assert.IsNotNull(promptRes);
            Assert.AreEqual("needs_clarification", promptRes.Status, "Khi hỏi chung chung về quỹ lương, AI yêu cầu làm rõ chỉ số (Thực lĩnh hay Ngày công).");
            Assert.IsNotNull(promptRes.Clarification);
            var opt = promptRes.Clarification.Options.FirstOrDefault(o => o.Value == "THUCLANH");
            Assert.IsNotNull(opt, "Phải có lựa chọn Thực lĩnh kỳ công.");

            // Turn 2: Chọn Thực lĩnh kỳ công bằng optionToken
            var res = await service.ProcessChatAsync("", ctx, "conv_payroll_test", optionToken: opt.Token, expectedConversationVersion: promptRes.ConversationVersion, clarificationId: promptRes.Clarification.ClarificationId);

            Assert.IsNotNull(res);
            Assert.AreEqual("answered", res.Status);
            StringAssert.Contains("150000000", res.Answer, "Phải trả về tổng thực lĩnh quỹ lương chính xác.");
            StringAssert.Contains("VNĐ", res.Answer);

            // Truy vấn trực tiếp nói rõ thực lĩnh "Tổng quỹ lương thực lĩnh tháng này là bao nhiêu?" -> trả lời ngay không cần hỏi lại
            var directRes = await service.ProcessChatAsync("Tổng quỹ lương thực lĩnh tháng này là bao nhiêu?", ctx, "conv_payroll_direct");
            Assert.IsNotNull(directRes);
            Assert.AreEqual("answered", directRes.Status);
            StringAssert.Contains("150000000", directRes.Answer);
        }

        [Test]
        public async Task Query3_TongQuyLuongThangNay_ReturnsNoData_WhenLockedPeriodHasNoRecords()
        {
            var understanding = new QueryUnderstandingService(_clock, new EntityResolver(new MockEntityLookupForDuy()));
            var sqlExec = new MockPayrollSqlExecutor(hasPayrollData: false);
            var sessions = new ConversationStateManager(_clock);
            var cache = new AiCacheCoordinator(_clock);

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExec,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: _clock
            );

            var ctx = AiTestContexts.All();
            ctx.Capabilities["PAYROLL_SUMMARY"] = new AiPolicyCapability
            {
                CapabilityCode = "PAYROLL_SUMMARY",
                RequiredFunctionCode = "F_CC_BANGLUONG",
                SourceView = "V_AI_PAYROLL_SUMMARY",
                Enabled = true
            };
            ctx.FieldPolicies["PAYROLL_SUMMARY:TONG_THUCLANH"] = "FULL";
            ctx.FieldPolicies["PAYROLL_SUMMARY:SO_NHANVIEN"] = "FULL";
            ctx.ScopeGrants.Add(new AiScopeGrant { CapabilityCode = "PAYROLL_SUMMARY", ScopeType = "ALL", Effect = "ALLOW" });

            // Turn 1
            var promptRes = await service.ProcessChatAsync("Tổng quỹ lương tháng này là bao nhiêu?", ctx, "conv_payroll_nodata");
            Assert.AreEqual("needs_clarification", promptRes.Status);
            var opt = promptRes.Clarification.Options.FirstOrDefault(o => o.Value == "THUCLANH");
            Assert.IsNotNull(opt);

            // Turn 2
            var res = await service.ProcessChatAsync("", ctx, "conv_payroll_nodata", optionToken: opt.Token, expectedConversationVersion: promptRes.ConversationVersion, clarificationId: promptRes.Clarification.ClarificationId);

            Assert.IsNotNull(res);
            Assert.AreEqual("no_data", res.Status);
            StringAssert.Contains("Không có bảng lương đã khóa sổ", res.Answer);
        }

        [Test]
        public async Task Query3_TongQuyLuongThangNay_ReturnsForbidden403_WhenLackingScopeOrRight()
        {
            var understanding = new QueryUnderstandingService(_clock, new EntityResolver(new MockEntityLookupForDuy()));
            var sqlExec = new MockPayrollSqlExecutor(hasPayrollData: true);
            var sessions = new ConversationStateManager(_clock);
            var cache = new AiCacheCoordinator(_clock);

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExec,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: _clock
            );

            // Context chỉ có F_SYSTEM_AI và F_DM_NHANVIEN, KHÔNG có F_CC_BANGLUONG hay PAYROLL_SUMMARY
            var ctx = new AiAuthorizationContext
            {
                UserId = 10,
                Manv = 10,
                HasAllScope = false,
                FunctionRights = new HashSet<string>(new[] { "F_SYSTEM_AI", "F_DM_NHANVIEN" }, StringComparer.OrdinalIgnoreCase),
                PolicyVersion = 1,
                PolicyLoaded = false
            };

            var res = await service.ProcessChatAsync("Tổng quỹ lương tháng này là bao nhiêu?", ctx, "conv_payroll_forbidden");

            Assert.IsNotNull(res);
            Assert.AreEqual("forbidden", res.Status);
            Assert.AreEqual(403, res.HttpStatus, "Phải trả về HTTP 403 Forbidden khi thiếu quyền/scope tra cứu quỹ lương.");
            StringAssert.Contains("quyền", res.Answer.ToLower());
        }

        // =========================================================================
        // AI CONFIGURATION COORDINATOR TESTS
        // =========================================================================
        [Test]
        public void AiConfigCoordinator_NormalizeBaseUrl_StripsEndpointsAndUserinfo()
        {
            string url1 = AiConfigurationCoordinator.NormalizeBaseUrl("http://localhost:11434/api/generate");
            Assert.AreEqual("http://localhost:11434", url1);

            string url2 = AiConfigurationCoordinator.NormalizeBaseUrl("http://127.0.0.1:11434/api/tags/");
            Assert.AreEqual("http://127.0.0.1:11434", url2);

            string url3 = AiConfigurationCoordinator.NormalizeBaseUrl("http://localhost:6333/collections");
            Assert.AreEqual("http://localhost:6333", url3);

            string url4 = AiConfigurationCoordinator.NormalizeBaseUrl("192.168.1.50:11434", "http", 11434);
            Assert.AreEqual("http://192.168.1.50:11434", url4);

            Assert.Throws<ArgumentException>(() => AiConfigurationCoordinator.NormalizeBaseUrl("ftp://localhost:11434"));
            Assert.Throws<ArgumentException>(() => AiConfigurationCoordinator.NormalizeBaseUrl("http://admin:secret@localhost:11434"));
        }

        [Test]
        public void AiConfigCoordinator_ParseInvariantDoubleAndInt_HandlesCultureCorrectly()
        {
            // Xử lý dấu phẩy hoặc chấm
            Assert.IsTrue(AiConfigurationCoordinator.ParseInvariantDouble("0,7", 0.4, out double d1));
            Assert.AreEqual(0.7, d1, 0.001);

            Assert.IsTrue(AiConfigurationCoordinator.ParseInvariantDouble("0.85", 0.4, out double d2));
            Assert.AreEqual(0.85, d2, 0.001);

            // Chặn NaN và số ngoài cận
            Assert.IsFalse(AiConfigurationCoordinator.ParseInvariantDouble("NaN", 0.4, out double _));
            Assert.IsFalse(AiConfigurationCoordinator.ParseInvariantDouble("5.5", 0.4, out double _, 0.0, 2.0));

            // Parse số nguyên
            Assert.IsTrue(AiConfigurationCoordinator.ParseInvariantInt("4096", 3072, out int i1, 1024, 128000));
            Assert.AreEqual(4096, i1);

            Assert.IsFalse(AiConfigurationCoordinator.ParseInvariantInt("500", 3072, out int _, 1024, 128000));
        }

        [Test]
        public void AiConfigCoordinator_Snapshot_HasThreadSafeDefault_And_Version()
        {
            var snap = AiConfigurationCoordinator.Instance.CurrentSnapshot;
            Assert.IsNotNull(snap);
            Assert.IsNotNull(snap.OllamaHost);
            Assert.IsNotNull(snap.AiModel);
            Assert.IsNotNull(snap.QdrantUrl);
            Assert.GreaterOrEqual(snap.Version, 1);
        }
    }
}
