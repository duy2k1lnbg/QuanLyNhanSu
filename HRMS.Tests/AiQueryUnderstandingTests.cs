using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Security;

namespace Bu.Tests
{
    public class FakeEntityLookupProvider : IEntityLookupProvider
    {
        private readonly List<EntityCandidate> _employees = new List<EntityCandidate>
        {
            new EntityCandidate { Id = 10, Code = "NV010", Name = "Nguyễn Văn An", DepartmentId = 2, CompanyCode = "1", DepartmentName = "Kỹ thuật", PositionName = "Lập trình viên" },
            new EntityCandidate { Id = 15, Code = "NV015", Name = "Trần Thị Thu An", DepartmentId = 1, CompanyCode = "1", DepartmentName = "Nhân sự", PositionName = "Chuyên viên tuyển dụng" },
            new EntityCandidate { Id = 18, Code = "NV018", Name = "Nguyễn Thọ Duy", DepartmentId = 2, CompanyCode = "1", DepartmentName = "Kỹ thuật", PositionName = "Trưởng nhóm" }
        };

        public List<EntityCandidate> FindEmployees(string query, AiAuthorizationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<EntityCandidate>();
            string q = query.Trim().ToLowerInvariant();
            return _employees.Where(e => e.Name.ToLowerInvariant().Contains(q)).ToList();
        }

        public EntityCandidate FindEmployeeById(int manv, AiAuthorizationContext ctx)
        {
            return _employees.FirstOrDefault(e => e.Id == manv);
        }

        public EntityCandidate FindDepartmentByName(string deptName, AiAuthorizationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(deptName)) return null;
            string lower = deptName.Trim().ToLowerInvariant();
            if (lower == "it" || lower == "cntt" || lower == "kỹ thuật")
            {
                return new EntityCandidate { Id = 2, Name = "Phòng Kỹ thuật" };
            }
            if (lower == "kế toán" || lower == "ke toan")
            {
                return new EntityCandidate { Id = 3, Name = "Phòng Kế toán" };
            }
            if (lower == "nhân sự" || lower == "nhan su")
            {
                return new EntityCandidate { Id = 1, Name = "Phòng Nhân sự" };
            }
            return null;
        }
    }

    [TestFixture]
    public class AiQueryUnderstandingTests
    {
        private QueryUnderstandingService _service;
        private FakeClockProvider _clock;
        private AiAuthorizationContext _adminCtx;
        private ClarificationPolicy _clarificationPolicy;

        [SetUp]
        public void SetUp()
        {
            // Clock cố định: ngày 03/10/2026 (theo đặc tả kiểm thử)
            _clock = new FakeClockProvider(new DateTime(2026, 10, 3, 10, 0, 0));
            var fakeResolver = new EntityResolver(new FakeEntityLookupProvider());
            _service = new QueryUnderstandingService(_clock, fakeResolver);
            _clarificationPolicy = new ClarificationPolicy();

            _adminCtx = new AiAuthorizationContext
            {
                UserId = 1,
                Username = "admin",
                IsAdmin = true,
                HasAllScope = true,
                FunctionRights = HRMS.Tests.AiTestContexts.Rights(),
                SourceRevisions = new Dictionary<string,long> { {"EMPLOYEE",1} }
            };
        }

        [Test]
        public void U01_Overtime_ForEmployeeId10_InSeptember2026()
        {
            string q = "Tăng ca của nhân viên mã 10 tháng 9 năm 2026";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("OVERTIME", result.Domain, "Domain phải là OVERTIME, không phải EMPLOYEE profile.");
            Assert.AreEqual(9, result.Time.Month);
            Assert.AreEqual(2026, result.Time.Year);
            var emp = result.Entities.FirstOrDefault();
            Assert.IsNotNull(emp);
            Assert.AreEqual(10, emp.ResolvedId);
            Assert.AreEqual("RESOLVED", emp.Status);
        }

        [Test]
        public void U02_Allowance_LessThanOneMillion_NotOvertime()
        {
            string q = "Phụ cấp dưới 1 triệu";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("ALLOWANCE", result.Domain, "Domain phải là ALLOWANCE, không phải OVERTIME vì chứa 'sotien'.");
            Assert.AreEqual(1, result.Filters.Count);
            var f = result.Filters[0];
            Assert.AreEqual("SOTIEN", f.Field);
            Assert.AreEqual("<", f.Operator, "'Dưới' phải là toán tử '<'.");
            Assert.AreEqual(1000000m, (decimal)f.Value);
        }

        [Test]
        public void U03_Allowance_GreaterThanOnePointFiveMillion()
        {
            string q = "Phụ cấp trên 1,5 triệu";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("ALLOWANCE", result.Domain);
            Assert.AreEqual(1, result.Filters.Count);
            var f = result.Filters[0];
            Assert.AreEqual(">", f.Operator, "'Trên' phải là toán tử '>'.");
            Assert.AreEqual(1500000m, (decimal)f.Value, "1,5 triệu phải parse thành đúng 1,500,000 VNĐ.");
        }

        [Test]
        public void U04_Birthday_InDecember_DoesNotInjectCurrentMonth()
        {
            string q = "Nhân viên sinh nhật tháng 12";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual(12, result.Time.BirthdayMonth);
            Assert.AreEqual("BIRTHDAY", result.Time.AnchorKind);
            // Không được nhầm lẫn sang tháng hiện tại (10)
            Assert.IsNull(result.Time.Month, "Birthday query không được gán tháng lịch Calendar Month.");
        }

        [Test]
        public void U05_PayrollSummary_DoesNotReturnFaqDate()
        {
            string q = "Tổng quỹ lương tháng này là bao nhiêu?";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("PAYROLL", result.Domain);
            Assert.AreEqual("SUMMARY", result.Operation);
            Assert.AreEqual("PAYROLL_UNSPECIFIED", result.Metric);
            var gate = new ClarificationPolicy().EvaluateClarificationNeeded(result);
            Assert.IsTrue(gate, "Quỹ lương must ask which measure is intended.");
            Assert.IsFalse(result.IsGreetingOnly);
        }

        [Test]
        public void U06_LeavePolicy_DoesNotQueryHotenContainsLeave()
        {
            string q = "Quy định nghỉ phép năm";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("POLICY", result.Domain);
            Assert.IsEmpty(result.Entities, "Không được đoán 'nghỉ phép năm' là tên người để tìm HOTEN!");
        }

        [Test]
        public void U07_GreetingWithQuery_PreservesOvertimeRequest()
        {
            string q = "Xin chào, An tăng ca bao nhiêu giờ tháng 9/2026?";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.IsFalse(result.IsGreetingOnly, "Phải giữ câu hỏi nghiệp vụ, không chỉ trả lời chào hỏi.");
            Assert.AreEqual("OVERTIME", result.Domain);
            Assert.AreEqual("SUM", result.Operation);
            Assert.AreEqual(9, result.Time.Month);
            Assert.AreEqual(2026, result.Time.Year);
        }

        [Test]
        public void U08_SingleMatch_NguyenVanAn_ResolvesWithoutAsking()
        {
            string q = "Thông tin nhân viên Nguyễn Văn An";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual(1, result.Entities.Count);
            var emp = result.Entities[0];
            Assert.AreEqual("RESOLVED", emp.Status);
            Assert.AreEqual(10, emp.ResolvedId);
            Assert.IsFalse(_clarificationPolicy.EvaluateClarificationNeeded(result));
        }

        [Test]
        public void U09_AmbiguousMatch_An_RequiresClarification()
        {
            string q = "Lương của An";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual(1, result.Entities.Count);
            var emp = result.Entities[0];
            Assert.AreEqual("AMBIGUOUS", emp.Status);
            Assert.AreEqual(2, emp.Candidates.Count, "Tìm thấy 2 người tên An.");

            bool needsClar = _clarificationPolicy.EvaluateClarificationNeeded(result);
            Assert.IsTrue(needsClar, "Phải yêu cầu làm rõ khi có 2 người trùng tên.");
            Assert.IsNotNull(result.Clarification);
            Assert.AreEqual("MANV", result.Clarification.Field);
            Assert.AreEqual(2, result.Clarification.Options.Count);
        }

        [Test]
        public void U10_PendingSelection_UserProvidesDeptAndOperation()
        {
            var pending = new ClarificationPrompt { Field = "MANV" };
            string followUp = "người phòng IT, tổng giờ";

            var result = _service.UnderstandQuery(followUp, _adminCtx, null, pending);

            Assert.AreEqual("SUM", result.Operation, "Phải bổ sung operation SUM.");
            Assert.AreEqual("DEPARTMENT", result.RequestedScope, "Phải bổ sung scope phòng ban IT.");
        }

        [Test]
        public void U11_FollowUpTime_PreviousSeptember_NextPreviousIsAugust()
        {
            // Lượt trước: hỏi tháng 9/2026
            var prev = new QueryUnderstandingResult
            {
                Domain = "OVERTIME",
                Operation = "SUM",
                Metric = "SOGIO",
                Time = new TimeResolution { Month = 9, Year = 2026 }
            };

            // Lượt này: "tháng trước đó thì sao?"
            string q = "tháng trước đó thì sao?";
            var result = _service.UnderstandQuery(q, _adminCtx, prev);

            Assert.AreEqual("OVERTIME", result.Domain);
            Assert.AreEqual("SUM", result.Operation);
            Assert.AreEqual(8, result.Time.Month, "Tháng liền trước của tháng 9/2026 phải là tháng 8/2026.");
            Assert.AreEqual(2026, result.Time.Year);
        }

        [Test]
        public void U12_RelativePreviousMonth_UsesInjectedClock()
        {
            // Clock đang là 03/10/2026 -> tháng trước phải là 09/2026
            string q = "Tổng tăng ca tháng trước";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("OVERTIME", result.Domain);
            Assert.AreEqual("SUM", result.Operation);
            Assert.AreEqual(9, result.Time.Month, "Tháng trước của tháng 10/2026 phải là tháng 9.");
            Assert.AreEqual(2026, result.Time.Year);
        }

        [Test]
        public void U14_UserCancelsAndSwitchesTopic()
        {
            var pending = new ClarificationPrompt { Field = "MANV", Question = "Chọn người tăng ca" };
            string q = "Thôi, tìm phụ cấp của Duy";

            var result = _service.UnderstandQuery(q, _adminCtx, null, pending);

            Assert.AreEqual("ALLOWANCE", result.Domain, "Phải chuyển sang domain ALLOWANCE.");
            var emp = result.Entities.FirstOrDefault();
            Assert.IsNotNull(emp);
            Assert.AreEqual(18, emp.ResolvedId);
            Assert.AreEqual("Nguyễn Thọ Duy", emp.ResolvedName);
        }

        [Test]
        public void U16_MonthThirteen_MarksInvalid()
        {
            string q = "Tăng ca tháng 13 năm 2026";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("INVALID", result.Time.AnchorKind, "Tháng 13 phải bị đánh dấu là INVALID.");
        }

        [Test]
        public void U17_BasicSalaryVsTakeHome_DistinctMetrics()
        {
            var qBasic = _service.UnderstandQuery("Lương cơ bản của nhân viên mã 10", _adminCtx);
            var qTakeHome = _service.UnderstandQuery("Thực lĩnh của nhân viên mã 10", _adminCtx);

            Assert.AreEqual("LUONG_COBAN", qBasic.Metric);
            Assert.AreEqual("THUCLANH", qTakeHome.Metric);
            Assert.AreNotEqual(qBasic.Metric, qTakeHome.Metric, "Lương cơ bản và thực lĩnh phải có metric riêng biệt.");
        }

        [Test]
        public void U18_OvertimeRank_AiTangCaNhieuNhat()
        {
            string q = "Ai tăng ca nhiều nhất?";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual("OVERTIME", result.Domain);
            Assert.AreEqual("RANK", result.Operation, "'Nhiều nhất' phải là phép xếp hạng RANK.");
            Assert.AreEqual("SOGIO", result.Metric);
        }

        [Test]
        public void U19_UnsupportedSkillSearch_ReturnsClearReason()
        {
            string q = "Tìm lập trình viên Java trong công ty";
            var result = _service.UnderstandQuery(q, _adminCtx);

            Assert.AreEqual(QuerySupportStatus.Unsupported, result.SupportStatus);
            Assert.That(result.UnsupportedReason, Does.Contain("skills").Or.Contain("kỹ năng"));
        }

        [Test]
        public void U13_AmbiguousPronoun_AnhAy_RequiresClarification()
        {
            var prev = new QueryUnderstandingResult
            {
                Domain = "EMPLOYEE",
                Entities = new List<EntityMention>
                {
                    new EntityMention { EntityType = "EMPLOYEE", ResolvedId = 10, ResolvedName = "Nguyễn Văn An", Status = "RESOLVED" },
                    new EntityMention { EntityType = "EMPLOYEE", ResolvedId = 15, ResolvedName = "Trần Thị Thu An", Status = "RESOLVED" },
                    new EntityMention { EntityType = "EMPLOYEE", ResolvedId = 18, ResolvedName = "Nguyễn Thọ Duy", Status = "RESOLVED" }
                }
            };

            var q = _service.UnderstandQuery("lương anh ấy", _adminCtx, prev);
            bool needsClar = _clarificationPolicy.EvaluateClarificationNeeded(q);
            Assert.IsTrue(needsClar, "Khi lượt trước có nhiều người, đại từ 'anh ấy' phải yêu cầu làm rõ.");
        }

        [Test]
        public void U15_OrgMentionAmbiguous_HandlesGracefully()
        {
            var q = _service.UnderstandQuery("Thống kê nhân sự phòng ban", _adminCtx);
            Assert.AreEqual("EMPLOYEE", q.Domain);
            Assert.AreEqual("COUNT", q.Operation);
        }

        [Test]
        public void U20_MalformedOrEmptyInput_HandledSafely()
        {
            var qNull = _service.UnderstandQuery(null, _adminCtx);
            Assert.AreEqual("GENERAL", qNull.Domain);
            Assert.IsTrue(qNull.IsGreetingOnly);

            var qEmpty = _service.UnderstandQuery("    ", _adminCtx);
            Assert.AreEqual("GENERAL", qEmpty.Domain);
            Assert.IsTrue(qEmpty.IsGreetingOnly);
        }
    }
}
