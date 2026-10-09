using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Bu.CLASS_SYSTEM;
using Bu.Services.AI_Services;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using HRMS_API.Controllers;
using HRMS_API.Services;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class AiIntegrationAndClientTests
    {
        private class TestPolicyProvider : IAiPolicyProvider { public AiAuthorizationContext Load(int userId) => HRMS.Tests.AiTestContexts.All(userId); }
        private class TestClockProvider : IClockProvider
        {
            public DateTime UtcNow { get; set; } = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc);
            public TimeZoneInfo BusinessTimeZone => TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, BusinessTimeZone);
            public DateTime Now => LocalNow;
        }

        [SetUp]
        public void Setup()
        {
            ConversationStateManager.Instance.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            ConversationStateManager.Instance.ClearAll();
            UserSession.CurrentUser = null;
            UserSession.UserRights.Clear();
        }

        #region R01 & R02: Controller Activation & JSON Serialization Wire Contract

        [Test]
        public void R01_DefaultHttpControllerActivator_MustInstantiateAiChatController()
        {
            var config = new System.Web.Http.HttpConfiguration();
            var activator = new System.Web.Http.Dispatcher.DefaultHttpControllerActivator();
            var descriptor = new System.Web.Http.Controllers.HttpControllerDescriptor(config, "AiChat", typeof(AiChatController));
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);

            var instance = activator.Create(request, descriptor, typeof(AiChatController));

            Assert.IsNotNull(instance, "DefaultHttpControllerActivator must be able to instantiate AiChatController without DI container.");
            Assert.IsInstanceOf<AiChatController>(instance);
        }

        [Test]
        public void R02_WireJsonSerialization_MustProduceCamelCasePropertiesForClient()
        {
            var dto = new AiChatResponseDto
            {
                Status = "needs_clarification",
                Answer = "Vui lòng chọn nhân viên",
                ConversationId = "conv-123",
                ConversationVersion = 2,
                RequestId = "req-456",
                Clarification = new AiClarificationDto
                {
                    ClarificationId = "clar-1",
                    TargetField = "MANV",
                    Question = "Chọn ai?",
                    Options = new List<AiClarificationOptionDto>
                    {
                        new AiClarificationOptionDto { OptionToken = "opt-1", Label = "Nguyễn Văn An", Value = "1" }
                    }
                },
                InterpretedRequest = new InterpretedRequestSummaryDto
                {
                    Domain = "OVERTIME",
                    Operation = "SUM",
                    Metric = "SOGIO",
                    ResolvedPeriod = "09/2026"
                },
                ResultMetadata = new AiResultMetadataDto
                {
                    Total = 5,
                    HasMore = false,
                    SourcePublicLabel = "Dữ liệu an toàn"
                },
                Source = "Deterministic_Engine",
                SqlQuery = ""
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);

            // Web client (AiChatDrawer.tsx) expects camelCase keys:
            // res.data?.status, res.data?.answer, res.data?.conversationId, res.data?.clarification, res.data?.interpretedRequest
            StringAssert.Contains("\"status\":", json, "JSON must contain camelCase 'status'");
            StringAssert.Contains("\"answer\":", json, "JSON must contain camelCase 'answer'");
            StringAssert.Contains("\"conversationId\":", json, "JSON must contain camelCase 'conversationId'");
            StringAssert.Contains("\"conversationVersion\":", json, "JSON must contain camelCase 'conversationVersion'");
            StringAssert.Contains("\"requestId\":", json, "JSON must contain camelCase 'requestId'");
            StringAssert.Contains("\"clarification\":", json, "JSON must contain camelCase 'clarification'");
            StringAssert.Contains("\"targetField\":", json, "JSON must contain camelCase 'targetField'");
            StringAssert.Contains("\"options\":", json, "JSON must contain camelCase 'options'");
            StringAssert.Contains("\"optionToken\":", json, "JSON must contain camelCase 'optionToken'");
            StringAssert.Contains("\"interpretedRequest\":", json, "JSON must contain camelCase 'interpretedRequest'");
            StringAssert.Contains("\"resultMetadata\":", json, "JSON must contain camelCase 'resultMetadata'");
            StringAssert.Contains("\"sourcePublicLabel\":", json, "JSON must contain camelCase 'sourcePublicLabel'");
        }

        #endregion

        #region R03, R04, R05: Schema Alignment, Parameter Binding, System Rights

        [Test]
        public void R04_OracleCommand_MustEnableBindByName_AndStripColonFromParameterNames()
        {
            var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
            var parameters = new Dictionary<string, object>
            {
                { ":p_scope_macty", "01" },
                { ":p_thang", 9 },
                { ":p_nam", 2026 },
                { ":p_sotien", 1500000m }
            };

            string sql = "SELECT * FROM V_AI_OVERTIME WHERE THANG = :p_thang AND NAM = :p_nam AND MACTY = :p_scope_macty";
            ScopedSqlExecutor.PrepareCommand(cmd, sql, parameters);

            Assert.IsTrue(cmd.BindByName, "OracleCommand.BindByName MUST be true to avoid positional parameter binding order bugs.");
            Assert.AreEqual(4, cmd.Parameters.Count);

            // Verify parameter names are clean without leading colon
            Assert.AreEqual("p_scope_macty", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(Oracle.ManagedDataAccess.Client.OracleDbType.NVarchar2, cmd.Parameters[0].OracleDbType);

            Assert.AreEqual("p_thang", cmd.Parameters[1].ParameterName);
            Assert.AreEqual(Oracle.ManagedDataAccess.Client.OracleDbType.Int32, cmd.Parameters[1].OracleDbType);

            Assert.AreEqual("p_nam", cmd.Parameters[2].ParameterName);
            Assert.AreEqual(Oracle.ManagedDataAccess.Client.OracleDbType.Int32, cmd.Parameters[2].OracleDbType);

            Assert.AreEqual("p_sotien", cmd.Parameters[3].ParameterName);
            Assert.AreEqual(Oracle.ManagedDataAccess.Client.OracleDbType.Decimal, cmd.Parameters[3].OracleDbType);
        }

        [Test]
        public void R05_AiCapabilityCatalog_MustUseAuthenticSystemFunctionCodes()
        {
            var empCap = AiCapabilityCatalog.Get("EMPLOYEE_LOOKUP");
            Assert.AreEqual("F_DM_NHANVIEN", empCap.RequiredFunctionCode, "EMPLOYEE_LOOKUP must require authentic F_DM_NHANVIEN");

            var otCap = AiCapabilityCatalog.Get("OVERTIME_VIEW");
            Assert.AreEqual("F_CC_TANGCA", otCap.RequiredFunctionCode, "OVERTIME_VIEW must require authentic F_CC_TANGCA");

            var attCap = AiCapabilityCatalog.Get("ATTENDANCE_SUMMARY");
            Assert.AreEqual("F_CC_BANGCONG", attCap.RequiredFunctionCode, "ATTENDANCE_SUMMARY must require authentic F_CC_BANGCONG");

            var payCap = AiCapabilityCatalog.Get("PAYROLL_VIEW");
            Assert.AreEqual("F_CC_BANGLUONG", payCap.RequiredFunctionCode, "PAYROLL_VIEW must require authentic F_CC_BANGLUONG");

            var allowCap = AiCapabilityCatalog.Get("ALLOWANCE_VIEW");
            Assert.AreEqual("F_CC_PHUCAP", allowCap.RequiredFunctionCode, "ALLOWANCE_VIEW must require authentic F_CC_PHUCAP");

            var advCap = AiCapabilityCatalog.Get("ADVANCE_VIEW");
            Assert.AreEqual("F_CC_UNGLUONG", advCap.RequiredFunctionCode, "ADVANCE_VIEW must require authentic F_CC_UNGLUONG");

            var contractCap = AiCapabilityCatalog.Get("CONTRACT_VIEW");
            Assert.AreEqual("F_NV_HOPDONG", contractCap.RequiredFunctionCode, "CONTRACT_VIEW must require authentic F_NV_HOPDONG");

            var salaryCap = AiCapabilityCatalog.Get("SALARY_CHANGE_VIEW");
            Assert.AreEqual("F_NV_NANGLUONG", salaryCap.RequiredFunctionCode, "SALARY_CHANGE_VIEW must require authentic F_NV_NANGLUONG");

            var authService = new AiAuthorizationService();

            // User with only overtime right F_CC_TANGCA
            var otCtx = new AiAuthorizationContext
            {
                UserId = 10,
                Username = "ot_user",
                FunctionRights = new HashSet<string> { "F_CC_TANGCA" }
            };

            var otCheck = authService.CheckCapability(otCtx, "OVERTIME_VIEW");
            Assert.IsTrue(otCheck.IsAllowed, "User with F_CC_TANGCA must be allowed OVERTIME_VIEW");

            var payCheck = authService.CheckCapability(otCtx, "PAYROLL_VIEW");
            Assert.IsFalse(payCheck.IsAllowed, "User with only F_CC_TANGCA must NOT be allowed PAYROLL_VIEW");
        }

        [Test]
        public void R03_QueryPlanner_MustAlignWithActualViewAndColumnDefinitions()
        {
            var clock = new TestClockProvider();
            var planner = new QueryPlanner(clock);

            var adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            // 1. Overtime: V_AI_OVERTIME and TEN_PHONGBAN (not V_AI_TANGCA or TENPB)
            var otUnderstood = new QueryUnderstandingResult
            {
                Domain = "OVERTIME",
                Operation = "RANK",
                Time = new TimeResolution { Month = 9, Year = 2026 }
            };
            var otPlan = planner.CreatePlan(otUnderstood, adminCtx);
            Assert.AreEqual("V_AI_OVERTIME_SUMMARY", otPlan.TargetView);
            StringAssert.Contains("AI_OWNER.V_AI_OVERTIME_SUMMARY", otPlan.SqlStatement);
            StringAssert.Contains("TEN_PHONGBAN", otPlan.SqlStatement);
            StringAssert.DoesNotContain("V_AI_TANGCA", otPlan.SqlStatement);
            StringAssert.DoesNotContain("TENPB", otPlan.SqlStatement);

            // 2. Allowance: V_AI_ALLOWANCE (not V_AI_PHUCAP) with actual columns
            var allowUnderstood = new QueryUnderstandingResult
            {
                Domain = "ALLOWANCE",
                Operation = "VIEW",
                Entities = new List<EntityMention>
                {
                    new EntityMention { EntityType = "EMPLOYEE", ResolvedId = 10, Status = "RESOLVED", MentionText = "10" }
                },
                Filters = new List<FilterCondition>
                {
                    new FilterCondition { Field = "SOTIEN", Operator = ">", Value = 1000000m }
                }
            };
            var allowPlan = planner.CreatePlan(allowUnderstood, adminCtx);
            Assert.AreEqual("V_AI_ALLOWANCE", allowPlan.TargetView);
            StringAssert.Contains("V_AI_ALLOWANCE", allowPlan.SqlStatement);
            StringAssert.Contains("TENPC", allowPlan.SqlStatement);
            StringAssert.Contains("MANV = :p_manv", allowPlan.SqlStatement, "MANV predicate must be preserved when SOTIEN filter is present");
            StringAssert.Contains("SOTIEN > :p_sotien", allowPlan.SqlStatement);
            StringAssert.DoesNotContain("V_AI_PHUCAP", allowPlan.SqlStatement);

            // 3. Contract: V_AI_CONTRACT with IS_DELETED (not DELE) and MANV support
            var contractUnderstood = new QueryUnderstandingResult
            {
                Domain = "CONTRACT",
                Operation = "VIEW",
                Entities = new List<EntityMention>
                {
                    new EntityMention { EntityType = "EMPLOYEE", ResolvedId = 10, Status = "RESOLVED", MentionText = "10" }
                }
            };
            var contractPlan = planner.CreatePlan(contractUnderstood, adminCtx);
            Assert.AreEqual("V_AI_CONTRACT", contractPlan.TargetView);
            StringAssert.Contains("V_AI_CONTRACT", contractPlan.SqlStatement);
            StringAssert.Contains("MANV = :p_manv", contractPlan.SqlStatement);
            StringAssert.DoesNotContain("IS_DELETED", contractPlan.SqlStatement);
            StringAssert.DoesNotContain("DELE =", contractPlan.SqlStatement);
        }

        #endregion

        #region C16: Method Not Allowed & Authentication Enforcement

        [Test]
        public async Task C16_GetChat_ReturnsMethodNotAllowed_DirectsToPost()
        {
            // GET /api/ai/chat bị từ chối 405 MethodNotAllowed để tránh lộ thông tin qua URL log
            var controller = new AiChatController
            {
                Request = new HttpRequestMessage(),
                Configuration = new System.Web.Http.HttpConfiguration()
            };

            var actionResult = controller.ChatGet();
            var response = await actionResult.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode, "GET /api/ai/chat must return 405 MethodNotAllowed.");
        }

        [Test]
        public async Task C16_GetReset_ReturnsMethodNotAllowed()
        {
            // GET /api/ai/reset bị từ chối 405 MethodNotAllowed
            var controller = new AiChatController
            {
                Request = new HttpRequestMessage(),
                Configuration = new System.Web.Http.HttpConfiguration()
            };

            var actionResult = controller.ResetGet();
            var response = await actionResult.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode, "GET /api/ai/reset must return 405 MethodNotAllowed.");
        }

        [Test]
        public async Task C16_AnonymousCaller_Chat_ReturnsUnauthorized()
        {
            // Người dùng không có JWT claims hợp lệ bị trả về 401 Unauthorized
            var controller = new AiChatController
            {
                Request = new HttpRequestMessage()
            };

            var result = await controller.Chat(new ChatRequest { Question = "Danh sách nhân viên" });
            var negResult = result as NegotiatedContentResult<AiChatResponseDto>;

            Assert.IsNotNull(negResult);
            Assert.AreEqual(HttpStatusCode.Unauthorized, negResult.StatusCode);
            Assert.AreEqual("forbidden", negResult.Content.Status);
        }

        [Test]
        public async Task C16_UserWithoutAiRight_ReturnsForbidden()
        {
            // Người dùng đăng nhập nhưng không có mã quyền F_SYSTEM_AI và không phải Admin bị trả về 403 Forbidden
            var controller = new AiChatController
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties["JwtUser"] = new JwtUserClaims
            {
                UserId = "99",
                Username = "user_no_ai",
                FullName = "Nhân viên Không AI",
                IsAdmin = false,
                Rights = new List<string> { "F_DM_NHANVIEN" } // Không có F_SYSTEM_AI
            };

            var result = await controller.Chat(new ChatRequest { Question = "Danh sách nhân viên" });
            var negResult = result as NegotiatedContentResult<AiChatResponseDto>;

            Assert.IsNotNull(negResult);
            Assert.AreEqual(HttpStatusCode.Forbidden, negResult.StatusCode);
            Assert.AreEqual("forbidden", negResult.Content.Status);
            StringAssert.Contains("F_SYSTEM_AI", negResult.Content.Answer);
        }

        #endregion

        #region C17: Clarification Option Token Validation & Expiry

        [Test]
        public async Task C17_OptionToken_WithoutPendingClarification_ReturnsError()
        {
            // Gửi option token khi không có clarification nào đang pending -> Bị từ chối
            var clock = new TestClockProvider();
            var execService = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: new HRMS.Tests.RegressionSqlExecutor(), clock: clock);

            var authContext = new AiAuthorizationContext
            {
                UserId = 101,
                Username = "user101",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            var result = await execService.ProcessChatAsync(
                question: "",
                authContext: authContext,
                conversationId: "conv-101",
                optionToken: "invalid_opt_token");

            Assert.AreEqual("error", result.Status);
            StringAssert.Contains("phiên làm rõ", result.Answer);
        }

        [Test]
        public async Task C17_OptionToken_Expired_ReturnsError()
        {
            // Gửi option token khi clarification đã quá 10 phút -> Bị từ chối do quá hạn
            var clock = new TestClockProvider();
            var execService = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: new HRMS.Tests.RegressionSqlExecutor(), clock: clock);

            var authContext = new AiAuthorizationContext
            {
                UserId = 102,
                Username = "user102",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            // Tạo pending clarification cũ (quá 15 phút)
            var session = ConversationStateManager.Instance.GetOrCreateSession(102, "conv-102");
            session.PendingClarification = new ClarificationPrompt
            {
                ClarificationId = "clar-102",
                Field = "TIME_PERIOD",
                Question = "Vui lòng chọn thời gian",
                CreatedAt = clock.UtcNow.AddMinutes(-15), // Quá 10 phút
                Options = new List<ClarificationOption>
                {
                    new ClarificationOption { Token = "tok-sep", Label = "Tháng 09/2026", Value = "09/2026" }
                }
            };

            var result = await execService.ProcessChatAsync(
                question: "",
                authContext: authContext,
                conversationId: "conv-102",
                optionToken: "tok-sep");

            Assert.AreEqual("error", result.Status);
            StringAssert.Contains("hết hạn", result.Answer);
        }

        [Test]
        public async Task C17_OptionToken_FromDifferentUser_CannotResolveSession()
        {
            // Token của User 1 không thể được dùng để giải quyết hội thoại của User 2
            var clock = new TestClockProvider();
            var execService = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: new HRMS.Tests.RegressionSqlExecutor(), clock: clock);

            // User 1 có pending clarification
            var sessionUser1 = ConversationStateManager.Instance.GetOrCreateSession(1, "shared-conv-name");
            sessionUser1.PendingClarification = new ClarificationPrompt
            {
                ClarificationId = "c1",
                Field = "MANV",
                Question = "Chọn nhân viên",
                CreatedAt = DateTime.UtcNow,
                Options = new List<ClarificationOption>
                {
                    new ClarificationOption { Token = "token_user1", Label = "Nguyễn Văn An", Value = "1" }
                }
            };

            // User 2 gọi câu hỏi với optionToken của User 1
            var authContext2 = new AiAuthorizationContext
            {
                UserId = 2,
                Username = "user2",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            var result = await execService.ProcessChatAsync(
                question: "",
                authContext: authContext2,
                conversationId: "shared-conv-name",
                optionToken: "token_user1");

            // User 2 không có pending clarification trong session của mình nên bị báo lỗi
            Assert.AreEqual("error", result.Status);
            StringAssert.Contains("phiên làm rõ", result.Answer);
        }

        #endregion

        #region C18: Web and Desktop Unified Execution & Session Isolation

        [Test]
        public async Task C18_DesktopChatboxManager_UsesExecutionService_WithIsolatedSession()
        {
            // Thiết lập Desktop UserSession
            UserSession.CurrentUser = new DA.TB_SYS_USER
            {
                IDUSER = 55,
                USERNAME = "desktop_user",
                FULLNAME = "Desktop Employee",
                MACTY = "CTY01"
            };
            UserSession.UserRights.Add("F_SYSTEM_AI");
            var fakeExecutor = new FakeScopedSqlExecutor();
            var execService = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: fakeExecutor, clock: new FakeClockProvider(new DateTime(2026, 10, 3)), policyProvider: new TestPolicyProvider());

            var manager1 = new ChatboxManager("conv_desk_1", execService);
            var manager2 = new ChatboxManager("conv_desk_2", execService);

            Assert.AreNotEqual(manager1.ConversationId, manager2.ConversationId, "Mỗi instance ChatboxManager phải có ConversationId độc lập.");

            var res1 = await manager1.ProcessQuery("Xin chào AI");
            Assert.IsNotNull(res1);
            Assert.IsFalse(string.IsNullOrWhiteSpace(res1.Answer));
            Assert.AreEqual(2, manager1.GetMessages().Count, "Manager1 phải ghi nhận tin nhắn User và AI.");
            Assert.AreEqual(0, manager2.GetMessages().Count, "Manager2 không bị nhiễm lịch sử của Manager1.");
        }

        [Test]
        public async Task C18_DesktopChatboxManager_Reset_ClearsOnlyCurrentSession()
        {
            UserSession.CurrentUser = new DA.TB_SYS_USER
            {
                IDUSER = 77,
                USERNAME = "reset_user",
                FULLNAME = "Reset User"
            };
            UserSession.UserRights.Add("F_SYSTEM_AI");

            var fakeExecutor = new FakeScopedSqlExecutor();
            var execService = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: fakeExecutor, clock: new FakeClockProvider(new DateTime(2026, 10, 3)), policyProvider: new TestPolicyProvider());

            var manager1 = new ChatboxManager("conv_77_a", execService);
            var manager2 = new ChatboxManager("conv_77_b", execService);

            await manager1.ProcessQuery("Câu hỏi 1");
            await manager2.ProcessQuery("Câu hỏi 2");

            Assert.AreEqual(2, manager1.GetMessages().Count);
            Assert.AreEqual(2, manager2.GetMessages().Count);

            // Reset manager 1
            manager1.Reset();

            Assert.AreEqual(0, manager1.GetMessages().Count, "Lịch sử của manager1 phải bị xóa.");
            Assert.AreEqual(2, manager2.GetMessages().Count, "Lịch sử của manager2 vẫn còn nguyên vẹn.");
        }

        [Test]
        public async Task C18_UnifiedContract_ReturnsInterpretedRequest_AndPublicMetadata()
        {
            // Web API và Desktop trả về đúng hợp đồng DTO với InterpretedRequest
            var fakeExecutor = new FakeScopedSqlExecutor();
            var execService = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), sqlExecutor: fakeExecutor, clock: new FakeClockProvider(new DateTime(2026, 10, 3)), policyProvider: new TestPolicyProvider());

            var controller = new AiChatController(execService)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties["JwtUser"] = new JwtUserClaims
            {
                UserId = "10",
                Username = "admin",
                FullName = "Quản trị viên",
                IsAdmin = true,
                Rights = new List<string> { "*" }
            };

            var result = await controller.Chat(new ChatRequest
            {
                Question = "Thống kê nhân sự theo từng phòng ban?",
                ConversationId = "conv_test_contract"
            });

            var okResult = result as OkNegotiatedContentResult<AiChatResponseDto>;
            Assert.IsNotNull(okResult, "Must return 200 OK with AiChatResponseDto");

            var dto = okResult.Content;
            Assert.AreEqual("answered", dto.Status);
            Assert.IsNotNull(dto.InterpretedRequest, "Must contain InterpretedRequest summary");
            Assert.AreEqual("EMPLOYEE", dto.InterpretedRequest.Domain);
            Assert.AreEqual("COUNT", dto.InterpretedRequest.Operation);
            Assert.IsEmpty(dto.SqlQuery, "Raw SQL query must never be exposed to clients.");
            Assert.IsNotNull(dto.ResultMetadata, "Must contain public ResultMetadata.");
            Assert.IsNotNull(dto.ResultMetadata.AsOf);
        }

        #endregion

        private class FakeScopedSqlExecutor : IScopedSqlExecutor
        {
            public SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                if (sql.Contains("TONG_SOGIO")) return new HRMS.Tests.RegressionSqlExecutor().ExecuteScopedQuery(sql,parameters,cancellationToken);
            var dt = new System.Data.DataTable();
                dt.Columns.Add("TEN_PHONGBAN", typeof(string));
                dt.Columns.Add("TOTAL_COUNT", typeof(int));
                dt.Rows.Add("Phòng Kỹ thuật", 15);
                dt.Rows.Add("Phòng Nhân sự", 5);

                return new SqlExecutionResult
                {
                    Status = SqlExecutionStatus.SuccessWithData,
                    Data = dt,
                    TotalRecords = 2,
                    SourceProvenance = "V_AI_EMPLOYEE"
                };
            }

            public Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ExecuteScopedQuery(sql, parameters, cancellationToken));
            }
        }

        private class CountingScopedSqlExecutor : IScopedSqlExecutor
        {
            public int CallCount { get; private set; }

            public SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                CallCount++;
                if (sql.Contains("TONG_SOGIO")) return new HRMS.Tests.RegressionSqlExecutor().ExecuteScopedQuery(sql,parameters,cancellationToken);
            var dt = new System.Data.DataTable();
                dt.Columns.Add("TOTAL_COUNT", typeof(int));
                dt.Rows.Add(15);
                return new SqlExecutionResult
                {
                    Status = SqlExecutionStatus.SuccessWithData,
                    Data = dt,
                    TotalRecords = 1,
                    SourceProvenance = "V_AI_EMPLOYEE_LOOKUP"
                };
            }

            public Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ExecuteScopedQuery(sql, parameters, cancellationToken));
            }
        }

        private class DelayableScopedSqlExecutor : IScopedSqlExecutor
        {
            public TaskCompletionSource<bool> BlockTcs = new TaskCompletionSource<bool>();

            public SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public async Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
            {
                await BlockTcs.Task;
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (sql.Contains("TONG_SOGIO")) return new HRMS.Tests.RegressionSqlExecutor().ExecuteScopedQuery(sql,parameters,cancellationToken);
            var dt = new System.Data.DataTable();
                dt.Columns.Add("TOTAL_COUNT", typeof(int));
                dt.Rows.Add(10);
                return new SqlExecutionResult
                {
                    Status = SqlExecutionStatus.SuccessWithData,
                    Data = dt,
                    TotalRecords = 1,
                    SourceProvenance = "V_AI_EMPLOYEE"
                };
            }
        }

        #region R06, R07, R08, R11, R12, R13, R17 Regression Tests

        [Test]
        public void R06_DenyByDefault_UserWithOnlyMaCty_CannotQueryAllCompanyOvertime()
        {
            // Nhân viên thường có MANV=10, MACTY="01" nhưng không có scope grant công ty
            var normalStaffCtx = new AiAuthorizationContext
            {
                UserId = 10,
                Username = "staff10",
                Manv = 10,
                MaCty = "01",
                IsAdmin = false,
                HasCompanyScope = false,
                HasAllScope = false,
                PolicyLoaded = true,
                ScopeGrants = new List<AiScopeGrant> { new AiScopeGrant { CapabilityCode = "OVERTIME_VIEW", ScopeType = "SELF", Effect = "ALLOW" } }
            };
            normalStaffCtx.FunctionRights.Add("F_CC_TANGCA");

            // 1. Khi xem tăng ca cá nhân: được phép và giới hạn về MANV của chính mình
            var selfScope = AiScopeEvaluator.BuildSqlScopeFilter(normalStaffCtx, "OVERTIME_VIEW");
            Assert.IsTrue(selfScope.IsAllowed);
            StringAssert.Contains("MANV =", selfScope.SqlPredicate);
            StringAssert.DoesNotContain("MACTY", selfScope.SqlPredicate);
            CollectionAssert.Contains(selfScope.Parameters.Values, 10);

            // 2. Khi hỏi aggregate toàn công ty: BỊ TỪ CHỐI (Deny-by-default) vì không có grant công ty
            var companySumScope = AiScopeEvaluator.BuildSqlScopeFilter(normalStaffCtx, "OVERTIME_SUM");
            Assert.IsFalse(companySumScope.IsAllowed, "Nhân viên thường không có quyền truy vấn tổng hợp toàn công ty.");

            // Kiểm tra nhân sự khác cùng công ty: Deny by default
            bool canViewColleague = AiScopeEvaluator.IsEmployeeInScope(normalStaffCtx, targetManv: 20, targetDeptId: 2, targetCompany: "01");
            Assert.IsFalse(canViewColleague, "Không được tự ý cho phép xem nhân viên khác chỉ vì chung MaCty.");
        }

        [Test]
        public async Task R08_Clarification_FullRoundtrip_PreservesOriginalQuestionAndSlots()
        {
            var clock = new TestClockProvider();
            var fakeExecutor = new FakeScopedSqlExecutor();
            var understanding = new QueryUnderstandingService(clock, new EntityResolver(new FakeEntityLookupProvider()));
            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: fakeExecutor,
                clock: clock);

            var adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            // Lượt 1: Hỏi câu mơ hồ ("An tăng ca bao nhiêu giờ tháng 9/2026?" có 2 người tên An)
            var res1 = await service.ProcessChatAsync(
                "An tăng ca bao nhiêu giờ tháng 9/2026?", 
                adminCtx, 
                conversationId: "conv_r08_roundtrip");

            Assert.AreEqual("needs_clarification", res1.Status, "Câu hỏi tên mơ hồ phải kích hoạt clarification gate trong luồng runtime.");
            Assert.IsNotNull(res1.Clarification);
            Assert.AreEqual(2, res1.Clarification.Options.Count);

            // Lượt 2: Người dùng chọn option 1 (Nguyễn Văn An, mã 10)
            string chosenToken = res1.Clarification.Options[0].Token; // "opt_emp_10"
            var res2 = await service.ProcessChatAsync(
                "", // Không cần gửi lại câu hỏi
                adminCtx,
                conversationId: "conv_r08_roundtrip",
                optionToken: chosenToken,
                expectedConversationVersion: res1.ConversationVersion, clarificationId: res1.Clarification.ClarificationId);

            Assert.AreEqual("answered", res2.Status, $"Actual status: {res2.Status}, Answer: {res2.Answer}");
            Assert.AreEqual("OVERTIME", res2.InterpretedRequest.Domain, "Phải giữ nguyên Domain OVERTIME ban đầu, không được chuyển sang EMPLOYEE LOOKUP.");
            Assert.AreEqual("SUM", res2.InterpretedRequest.Operation, "Phải giữ nguyên Operation SUM ban đầu.");
            Assert.AreEqual("09/2026", res2.InterpretedRequest.EffectivePeriodDisplay, "Phải giữ nguyên kỳ 09/2026 ban đầu.");
            StringAssert.Contains("Nguyễn Văn An", res2.InterpretedRequest.EntityDisplay);
        }

        [Test]
        public async Task R11_R12_CacheCoordinator_WarmHit_AvoidsCallingSqlExecutor()
        {
            var clock = new TestClockProvider();
            var countingExecutor = new CountingScopedSqlExecutor();
            var cacheCoordinator = new AiCacheCoordinator(clock);
            var understanding = new QueryUnderstandingService(clock, new EntityResolver(new FakeEntityLookupProvider()));
            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: countingExecutor,
                cacheCoordinator: cacheCoordinator,
                clock: clock);

            var adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            // Lần 1: Cold cache -> gọi SQL executor
            var res1 = await service.ProcessChatAsync("Có bao nhiêu nhân viên phòng IT?", adminCtx, conversationId: "conv_cache_1");
            Assert.AreEqual("answered", res1.Status);
            Assert.AreEqual(1, countingExecutor.CallCount, "Lần gọi 1 (cold cache) phải gọi SQL 1 lần.");

            // Lần 2: Warm cache (cùng caller, cùng câu hỏi, cùng kỳ) -> lấy từ ResultCache, không gọi SQL!
            var res2 = await service.ProcessChatAsync("Có bao nhiêu nhân viên phòng IT?", adminCtx, conversationId: "conv_cache_1");
            Assert.AreEqual("answered", res2.Status);
            Assert.AreEqual(1, countingExecutor.CallCount, "Lần gọi 2 (warm cache) phải lấy từ cache, không được gọi lại SQL.");

            // Lần 3: Caller khác (UserId = 2) -> cache key khác nhau, phải gọi SQL lần 2!
            var user2Ctx = new AiAuthorizationContext
            {
                UserId = 2,
                Username = "user2",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };
            var res3 = await service.ProcessChatAsync("Có bao nhiêu nhân viên phòng IT?", user2Ctx, conversationId: "conv_cache_2");
            Assert.AreEqual("answered", res3.Status);
            Assert.AreEqual(2, countingExecutor.CallCount, "Caller khác nhau có cache key khác nhau, phải gọi SQL để đảm bảo an toàn phân quyền.");
        }

        [Test]
        public async Task R13_CancellationDuringWait_DoesNotMutateConversationHistory()
        {
            var clock = new TestClockProvider();
            var delayableExecutor = new DelayableScopedSqlExecutor();
            var service = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), 
                sqlExecutor: delayableExecutor,
                clock: clock);

            var adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            var session = ConversationStateManager.Instance.GetOrCreateSession(1, "conv_cancel_test");
            int initialMsgCount = session.Messages.Count;

            using (var cts = new CancellationTokenSource())
            {
                // Bắt đầu query
                var task = service.ProcessChatAsync("Có bao nhiêu nhân viên phòng IT?", adminCtx, conversationId: "conv_cancel_test", cancellationToken: cts.Token);

                // Hủy bỏ trong lúc query đang chờ SQL
                cts.Cancel();
                delayableExecutor.BlockTcs.SetResult(true);

                var res = await task;
                Assert.AreEqual("error", res.Status);
                StringAssert.Contains("hủy", res.Answer);
                Assert.AreEqual(initialMsgCount, session.Messages.Count, "Khi request bị hủy, không được mutate tin nhắn vào lịch sử hội thoại.");
            }
        }

        [Test]
        public async Task R13_ResetDuringWait_DiscardsLateResponse()
        {
            var clock = new TestClockProvider();
            var delayableExecutor = new DelayableScopedSqlExecutor();
            var service = new AiExecutionService(understandingService: new QueryUnderstandingService(entityResolver: new EntityResolver(new FakeEntityLookupProvider())), 
                sqlExecutor: delayableExecutor,
                clock: clock);

            var adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };

            var session = ConversationStateManager.Instance.GetOrCreateSession(1, "conv_reset_race");

            // Bắt đầu query phiên cũ
            var task = service.ProcessChatAsync("Có bao nhiêu nhân viên phòng IT?", adminCtx, conversationId: "conv_reset_race");

            // Người dùng bấm Reset Chat trong khi SQL đang chạy
            ConversationStateManager.Instance.ClearSession(1, "conv_reset_race");

            // Cho SQL hoàn tất
            delayableExecutor.BlockTcs.SetResult(true);

            var res = await task;
            Assert.AreEqual("error", res.Status);
            StringAssert.Contains("đã thay đổi", res.Answer, "Kết quả từ truy vấn trước đó phải bị từ chối sau khi phiên đã bị reset.");
        }

        [Test]
        public void R17_StatusForbidden_IsPreservedAndNotMaskedAsNoData()
        {
            var plan = new QueryExecutionPlan
            {
                Domain = "OVERTIME",
                Operation = "SUM",
                TargetView = "V_AI_OVERTIME",
                EffectivePeriodDisplay = "09/2026",
                EffectiveScopeDisplay = "Cá nhân"
            };

            // Khi SQL executor trả AuthorizationDenied
            var deniedSql = new SqlExecutionResult
            {
                Status = SqlExecutionStatus.AuthorizationDenied,
                Data = null
            };

            var rendered = DeterministicResponseRenderer.Render(plan, deniedSql);
            Assert.AreEqual("forbidden", rendered.Status, "Lỗi từ chối quyền SQL không được rơi sang no_data!");
            StringAssert.Contains("quyền", rendered.Answer);
            Assert.IsNull(rendered.SourceProvenance, "A denied query must not claim a successful data source.");
            Assert.AreEqual(403, rendered.HttpStatus);

            // Khi SQL executor trả ConnectionError
            var connErrSql = new SqlExecutionResult
            {
                Status = SqlExecutionStatus.ConnectionError,
                Data = null
            };

            var renderedConn = DeterministicResponseRenderer.Render(plan, connErrSql);
            Assert.AreEqual("error", renderedConn.Status, "Lỗi kết nối CSDL không được rơi sang no_data!");
        }

        #endregion
    }
}
