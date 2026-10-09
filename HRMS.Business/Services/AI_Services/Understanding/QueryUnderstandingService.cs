using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bu.Services.AI_Services.Security;

namespace Bu.Services.AI_Services.Core
{
    public interface IQueryUnderstandingService
    {
        QueryUnderstandingResult UnderstandQuery(
            string question, 
            AiAuthorizationContext ctx, 
            QueryUnderstandingResult previousRequest = null, 
            ClarificationPrompt pendingClarification = null);
    }

    public class QueryUnderstandingService : IQueryUnderstandingService
    {
        private readonly IClockProvider _clock;
        private readonly EntityResolver _entityResolver;

        public QueryUnderstandingService(IClockProvider clock = null, EntityResolver entityResolver = null)
        {
            _clock = clock ?? new SystemClockProvider();
            _entityResolver = entityResolver ?? new EntityResolver();
        }

        public QueryUnderstandingResult UnderstandQuery(
            string question, 
            AiAuthorizationContext ctx, 
            QueryUnderstandingResult previousRequest = null, 
            ClarificationPrompt pendingClarification = null)
        {
            var result = new QueryUnderstandingResult
            {
                OriginalQuestion = question ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(question))
            {
                result.Domain = "GENERAL";
                result.IsGreetingOnly = true;
                return result;
            }

            // 1. Chuẩn hóa chuỗi ký tự (giữ nguyên không nối hint)
            string normalized = NormalizeKeywords(question.Trim());
            result.NormalizedQuestion = normalized;

            string lower = Regex.Replace(normalized.ToLowerInvariant(), @"[?.!,;:]+$", "").Trim();

            if(IsPureGreeting(lower)) { result.Domain="GENERAL"; result.IsGreetingOnly=true; return result; }
            // 2. Kiểm tra câu chào hỏi
            string[] greetings = { "xin chào", "chào bạn", "chào em", "chào bot", "chào ad", "hello", "hi", "hey" };
            foreach (var g in greetings)
            {
                if (lower == g || lower == g + "!" || lower == g + ".")
                {
                    result.Domain = "GENERAL";
                    result.IsGreetingOnly = true;
                    return result;
                }
                if (lower.StartsWith(g + " ") || lower.StartsWith(g + ", ") || lower.StartsWith(g + "! "))
                {
                    // Tách câu chào ra khỏi câu hỏi chính để hiểu đúng nghiệp vụ phía sau (U07)
                    int commaIdx = g.Length - 1;
                    if (commaIdx > 0 && commaIdx < lower.Length - 1)
                    {
                        lower = lower.Substring(commaIdx + 1).TrimStart(',', '!', ' ');
                    }
                    break;
                }
            }

            // 3. Kiểm tra đổi chủ đề khi đang trong trạng thái chờ làm rõ (U14)
            if (pendingClarification != null)
            {
                string[] cancelWords = { "thôi", "hủy", "bỏ qua", "chuyển sang", "không cần", "tìm cái khác" };
                if (cancelWords.Any(w => lower.StartsWith(w)))
                {
                    // Hủy pending cũ, xóa từ hủy rồi tiếp tục phân tích yêu cầu mới
                    pendingClarification = null;
                    foreach (var cw in cancelWords)
                    {
                        if (lower.StartsWith(cw))
                        {
                            lower = lower.Substring(cw.Length).TrimStart(',', ':', ' ');
                            break;
                        }
                    }
                }
            }

            // 4. Phân giải thời gian (Time Resolution)
            ResolveTime(lower, result.Time, previousRequest?.Time);

            // 5. Xác định Domain, Operation & Metric
            ResolveDomainAndMetric(lower, result);

            // 6. Xử lý các điều kiện lọc (Filters: số tiền phụ cấp, toán tử...)
            ResolveFilters(lower, result);

            // 7. Xử lý câu hỏi tiếp nối (Conversational resolution):
            // Ví dụ U11: "tháng trước đó thì sao?"
            bool isPureTimeFollowUp = lower.Contains("tháng trước đó") || lower.Contains("kỳ trước đó") || lower.Contains("tháng sau đó") || (lower.StartsWith("thế còn") && result.Time.Month.HasValue);
            if (previousRequest != null && isPureTimeFollowUp)
            {
                result.Domain = previousRequest.Domain;
                result.Operation = previousRequest.Operation;
                result.Metric = previousRequest.Metric;
                result.Entities = Bu.Services.AI_Services.Memory.ConversationStateManager.Clone(previousRequest.Entities);
                result.Filters = Bu.Services.AI_Services.Memory.ConversationStateManager.Clone(previousRequest.Filters);
                result.RequestedScope = previousRequest.RequestedScope;
                result.Assumptions.Add($"Kế thừa nghiệp vụ {previousRequest.Domain} từ lượt hỏi trước.");
            }

            if (lower.Contains("của tôi") || lower.Contains("của mình") || lower.Contains("cá nhân")) result.RequestedScope = "SELF";
            if (result.Metric == "BIRTHDAY" && result.Time.Month.HasValue) result.Time.BirthdayMonth = result.Time.Month;
            if (lower.Contains("so sánh") || Regex.Matches(lower,@"tháng\s+\d+").Count > 1) { result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Truy vấn so sánh hoặc nhiều kỳ chưa được hỗ trợ. Hãy chọn một kỳ cụ thể."; }
            if (result.Domain == "EMPLOYEE" && result.Metric != "BIRTHDAY" && (result.Time.Month.HasValue || result.Time.Year.HasValue)) { result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Nguồn hồ sơ chỉ phản ánh hiện tại, chưa có snapshot nhân sự theo tháng/năm. Vui lòng hỏi nhân sự hiện tại hoặc chọn nghiệp vụ theo kỳ đã có nguồn."; }
            if (result.Filters.Count > 0 && result.Domain != "ALLOWANCE" && result.Domain != "ADVANCE") { result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Điều kiện lọc số tiền hiện chỉ hỗ trợ phụ cấp hoặc tạm ứng. Chưa có bộ lọc tiền được xác nhận cho nghiệp vụ này."; }
            // 8. Phân giải Thực thể (Entities: nhân viên, mã nhân viên)
            if (result.SupportStatus == QuerySupportStatus.Supported && result.Domain != "GENERAL" && result.Domain != "POLICY") {
                ctx.LookupCapability = QueryPlanner.DetermineCapability(result, ctx, lower);
                if (AiCapabilityCatalog.Get(ctx.LookupCapability)?.IsEnabled == false) { result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Nguồn dữ liệu hoặc ánh xạ quyền cho chức năng này chưa sẵn sàng."; }
                else if (!AiAuthorizationService.ValidateCapability(ctx, ctx.LookupCapability).IsAllowed) { result.SupportStatus = QuerySupportStatus.Forbidden; result.UnsupportedReason = "Bạn chưa được cấp quyền tra cứu nghiệp vụ này."; }
                else if (Regex.IsMatch(lower,@"\b(anh ấy|cô ấy|người đó|nhân viên đó)\b"))
                {
                    var previousEmployees = previousRequest?.Entities.Where(e => e.EntityType == "EMPLOYEE").ToList() ?? new List<EntityMention>();
                    var ids = previousEmployees.SelectMany(e => e.ResolvedId.HasValue ? new[] { e.ResolvedId.Value } : e.Candidates.Select(c => c.Id)).Distinct().ToList();
                    var currentCandidates = ids.Select(id => _entityResolver.ResolveEmployee(id.ToString(),ctx)).Where(e => e.Status == "RESOLVED").SelectMany(e => e.Candidates).ToList();
                    result.Entities.Add(new EntityMention { MentionText="người đó", EntityType="EMPLOYEE", Status=currentCandidates.Count==1 ? "RESOLVED" : currentCandidates.Count>1 ? "AMBIGUOUS" : "NOT_FOUND", ResolvedId=currentCandidates.Count==1 ? (int?)currentCandidates[0].Id : null, ResolvedName=currentCandidates.Count==1 ? currentCandidates[0].Name : null, DepartmentId=currentCandidates.Count==1 ? currentCandidates[0].DepartmentId : null, CompanyCode=currentCandidates.Count==1 ? currentCandidates[0].CompanyCode : null, Candidates=currentCandidates });
                }
                else ResolveEntities(lower, result, ctx);
            }

            // 9. Xử lý phản hồi bổ sung cho lượt đang chờ làm rõ (U10):
            // Ví dụ: user nói "người phòng IT, tổng giờ" khi đang chờ chọn người
            if (pendingClarification != null)
            {
                if (lower.Contains("tổng giờ") || lower.Contains("tổng số giờ") || lower.Contains("tính tổng"))
                {
                    result.Operation = "SUM";
                }
                if (lower.Contains("phòng it") || lower.Contains("kỹ thuật") || lower.Contains("cntt"))
                {
                    result.RequestedScope = "DEPARTMENT";
                }
            }

            if (result.Domain == "EMPLOYEE" && result.Operation == "LOOKUP" && result.Entities.Count == 0 && !lower.Contains("nhân viên") && !lower.Contains("nhân sự")) { result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Chưa xác định được nghiệp vụ hoặc đối tượng cần tra cứu."; }
            if (Regex.IsMatch(lower,@"\bngày\s+\d{1,2}/\d{1,2}") && result.Domain != "CONTRACT") { result.SupportStatus=QuerySupportStatus.Unsupported; result.UnsupportedReason="Truy vấn theo ngày cụ thể chưa được ánh xạ. Hãy chọn tháng và năm."; }
            // 10. Hoàn thiện ResolvedQuestion
            result.ResolvedQuestion = BuildResolvedQuestion(result);

            return result;
        }

        public static bool IsPureGreeting(string question)
        {
            return !string.IsNullOrWhiteSpace(question) && Regex.IsMatch(question.Trim(),@"^(xin chào|chào bạn|chào em|chào bot|chào ad|hello|hi|hey)[.!?]*$",RegexOptions.IgnoreCase);
        }
        private void ResolveTime(string lower, TimeResolution time, TimeResolution previousTime)
        {
            DateTime now = _clock.Now;

            // 1. Sinh nhật theo tháng (U04)
            var birthMatch = Regex.Match(lower, @"sinh nhật(?:\s+vào)?\s+tháng\s+(\d+)");
            if (birthMatch.Success && int.TryParse(birthMatch.Groups[1].Value, out int bMonth))
            {
                if (bMonth >= 1 && bMonth <= 12)
                {
                    time.BirthdayMonth = bMonth;
                    time.AnchorKind = "BIRTHDAY";
                    return;
                }
                time.AnchorKind = "INVALID"; return;
            }

            // 2. Tháng trước đó / kỳ trước đó (tiếp nối lượt trước U11)
            if (lower.Contains("tháng trước đó") || lower.Contains("kỳ trước đó"))
            {
                int refMonth = previousTime?.Month ?? now.Month;
                int refYear = previousTime?.Year ?? now.Year;

                if (refMonth == 1)
                {
                    time.Month = 12;
                    time.Year = refYear - 1;
                }
                else
                {
                    time.Month = refMonth - 1;
                    time.Year = refYear;
                }
                time.IsRelativeTime = true;
                time.RelativeDescription = "tháng trước đó";
                time.AnchorKind = "CALENDAR_MONTH";
                return;
            }

            // 3. Tháng trước (U12 - tính theo injected clock)
            if (lower.Contains("tháng trước") || lower.Contains("kỳ trước"))
            {
                if (now.Month == 1)
                {
                    time.Month = 12;
                    time.Year = now.Year - 1;
                }
                else
                {
                    time.Month = now.Month - 1;
                    time.Year = now.Year;
                }
                time.IsRelativeTime = true;
                time.RelativeDescription = "tháng trước";
                time.AnchorKind = "CALENDAR_MONTH";
                return;
            }

            // 4. Tháng này / kỳ này
            if (lower.Contains("tháng này") || lower.Contains("kỳ này") || lower.Contains("tháng hiện tại"))
            {
                time.Month = now.Month;
                time.Year = now.Year;
                time.IsRelativeTime = true;
                time.RelativeDescription = "tháng này";
                time.AnchorKind = "CALENDAR_MONTH";
                return;
            }

            // 5. Tháng X năm Y rõ ràng (ví dụ: tháng 9 năm 2026, tháng 9/2026)
            var monthYearMatch = Regex.Match(lower, @"tháng\s+(\d+)(?:\s+(?:năm|/)\s*|\s*/\s*)(\d{4})");
            if (monthYearMatch.Success)
            {
                if (int.TryParse(monthYearMatch.Groups[1].Value, out int m) && int.TryParse(monthYearMatch.Groups[2].Value, out int y))
                {
                    if (m >= 1 && m <= 12 && y >= 1900 && y <= 2100)
                    {
                        time.Month = m;
                        time.Year = y;
                        time.AnchorKind = "CALENDAR_MONTH";
                        return;
                    }
                }
            }

            if (monthYearMatch.Success) { time.AnchorKind = "INVALID"; return; }
            var yearOnly = Regex.Match(lower, @"năm\s+(\d{4})\b");
            if (yearOnly.Success) time.Year = int.Parse(yearOnly.Groups[1].Value);
            // 6. Tháng X đơn lẻ (U16: nếu tháng > 12 là không hợp lệ)
            var singleMonthMatch = Regex.Match(lower, @"tháng\s+(\d+)");
            if (singleMonthMatch.Success && int.TryParse(singleMonthMatch.Groups[1].Value, out int sm))
            {
                if (sm >= 1 && sm <= 12)
                {
                    time.Month = sm;
                    time.Year = yearOnly.Success ? (int?)int.Parse(yearOnly.Groups[1].Value) : null;
                    time.AnchorKind = "CALENDAR_MONTH";
                }
                else
                {
                    // Tháng 13 không hợp lệ
                    time.AnchorKind = "INVALID";
                }
            }
        }

        private void ResolveDomainAndMetric(string lower, QueryUnderstandingResult result)
        {
            if (lower.Contains("sinh nhật")) { result.Domain = "EMPLOYEE"; result.Operation = "LIST"; result.Metric = "BIRTHDAY"; return; }
            // Kiểm tra câu hỏi skills/lập trình viên ngoài CSDL (U19)
            if (lower.Contains("java") || lower.Contains("developer") || lower.Contains("lập trình") || lower.Contains("kỹ năng") || lower.Contains("react"))
            {
                result.Domain = "EMPLOYEE";
                result.SupportStatus = QuerySupportStatus.Unsupported;
                result.UnsupportedReason = "Hệ thống CSDL hiện tại không lưu trữ kỹ năng chuyên môn / lập trình (skills) của nhân sự.";
                return;
            }

            // Nghỉ phép năm (U06) -> không tìm tên người!
            if (lower.Contains("nghỉ phép") || lower.Contains("phép năm") || lower.Contains("ngày phép"))
            {
                if (lower.Contains("quy định") || lower.Contains("chế độ") || lower.Contains("luật"))
                {
                    result.Domain = "POLICY";
                    result.Operation = "VIEW";
                    return;
                }
                result.Domain = "ATTENDANCE";
                result.Operation = "SUMMARY";
                result.Metric = "NGAYPHEP";
                return;
            }

            // Tăng ca / Làm thêm giờ (U01, U18 - Sử dụng Regex token biên từ \bot\b để tránh lỗi chứa "ot" như "sotien")
            bool hasOt = lower.Contains("tăng ca") || lower.Contains("làm thêm") || Regex.IsMatch(lower, @"\bot\b", RegexOptions.IgnoreCase);
            if (hasOt)
            {
                result.Domain = "OVERTIME";
                result.Metric = "SOGIO";

                if (lower.Contains("nhiều nhất") || lower.Contains("cao nhất") || lower.Contains("top"))
                {
                    result.Operation = "RANK"; // U18: aggregate SUM theo người rồi rank
                }
                else if (lower.Contains("tổng") || lower.Contains("bao nhiêu giờ") || lower.Contains("mấy giờ"))
                {
                    result.Operation = "SUM";
                }
                else
                {
                    result.Operation = lower == "xem tăng ca" || lower == "tăng ca" ? null : "VIEW";
                }
                return;
            }

            // Phụ cấp / Trợ cấp (U02, U03)
            if (lower.Contains("phụ cấp") || lower.Contains("trợ cấp"))
            {
                result.Domain = "ALLOWANCE";
                result.Metric = "SOTIEN";
                result.Operation = "VIEW";
                return;
            }

            // Quỹ lương / Tổng lương (U05, U17)
            if (lower.Contains("quỹ lương") || lower.Contains("tổng quỹ lương"))
            {
                result.Domain = "PAYROLL";
                result.Operation = "SUMMARY";
                if (lower.Contains("thực lĩnh") || lower.Contains("thực nhận"))
                {
                    result.Metric = "THUCLANH";
                }
                else if (lower.Contains("ngày công") || lower.Contains("công thực tế"))
                {
                    result.Metric = "LUONG_CONG_THUCTE";
                }
                else
                {
                    result.Metric = "PAYROLL_UNSPECIFIED";
                }
                return;
            }

            // Lương cơ bản vs Thực lĩnh (U17)
            if (lower.Contains("lương cơ bản"))
            {
                result.Domain = "PAYROLL";
                result.Operation = "VIEW";
                result.Metric = "LUONG_COBAN";
                result.SupportStatus = QuerySupportStatus.Unsupported;
                result.UnsupportedReason = "Nguồn bảng lương lưu tiền theo ngày công thực tế; chưa có ánh xạ được xác nhận cho lương cơ bản tháng.";
                return;
            }
            if (lower.Contains("thực lĩnh") || lower.Contains("thực nhận"))
            {
                result.Domain = "PAYROLL";
                result.Operation = "VIEW";
                result.Metric = "THUCLANH";
                return;
            }
            if (lower.Contains("bảng lương") || lower.Contains("phiếu lương") || lower.Contains("tiền lương") || lower.Contains("lương của"))
            {
                result.Domain = "PAYROLL";
                result.Operation = "VIEW";
                result.Metric = "THUCLANH";
                return;
            }

            if (lower.Contains("lương") && !lower.Contains("tăng lương") && !lower.Contains("lên lương") && !lower.Contains("nâng lương") && !lower.Contains("ứng lương")) { result.Domain = "PAYROLL"; result.Operation = "VIEW"; result.Metric = "PAYROLL_UNSPECIFIED"; return; }
            // Chấm công
            if (lower.Contains("chấm công") || lower.Contains("điểm danh") || lower.Contains("giờ vào") || lower.Contains("giờ ra"))
            {
                result.Domain = "ATTENDANCE";
                result.Operation = "DETAIL";
                return;
            }

            // Bảo hiểm (U06)
            if (lower.Contains("bảo hiểm") || lower.Contains("bhxh") || lower.Contains("bhyt"))
            {
                result.Domain = "INSURANCE";
                result.Operation = "VIEW";
                return;
            }

            // Tạm ứng lương
            if (lower.Contains("tạm ứng") || lower.Contains("ứng lương"))
            {
                result.Domain = "ADVANCE";
                result.Metric = "SOTIEN";
                result.Operation = "VIEW";
                return;
            }

            // Hợp đồng
            if (lower.Contains("hợp đồng") || lower.Contains("hết hạn"))
            {
                result.Domain = "CONTRACT";
                result.Operation = lower.Contains("hết hạn") ? "EXPIRING" : "VIEW";
                if (result.Operation == "EXPIRING") {
                    var days = Regex.Match(lower, @"(\d+)\s+ngày");
                    int horizon = days.Success ? int.Parse(days.Groups[1].Value) : 30;
                    if (horizon < 1 || horizon > 366) { result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Khoảng hết hạn phải từ 1 đến 366 ngày."; }
                    else { result.Time.StartDate = _clock.Now.Date; result.Time.EndDate = _clock.Now.Date.AddDays(horizon + 1); if (!days.Success) result.Assumptions.Add("Mặc định xét 30 ngày tới."); }
                }
                return;
            }

            // Tăng lương / Nâng lương
            if (lower.Contains("tăng lương") || lower.Contains("lên lương") || lower.Contains("nâng lương"))
            {
                result.Domain = "SALARY_CHANGE";
                result.Operation = "VIEW";
                if (lower.Contains("chuẩn bị") || lower.Contains("sắp")) result.Time.StartDate = _clock.Now.Date;
                return;
            }

            // Đếm số lượng nhân viên
            if (lower.Contains("có bao nhiêu nhân viên") || lower.Contains("số lượng nhân viên") || lower.Contains("thống kê nhân sự"))
            {
                result.Domain = "EMPLOYEE";
                result.Operation = "COUNT";
                result.Metric = lower.Contains("theo từng phòng ban") || lower.Contains("theo phòng ban") ? "HEADCOUNT_BY_DEPT" : "HEADCOUNT";
                return;
            }

            // Mặc định tra cứu nhân sự
            result.Domain = "EMPLOYEE";
            result.Operation = "LOOKUP";
        }

        private void ResolveFilters(string lower, QueryUnderstandingResult result)
        {
            var amounts = Regex.Matches(lower, @"(không quá|không vượt quá|không dưới|không ít hơn|không nhỏ hơn|tối đa|tối thiểu|ít nhất|dưới|nhỏ hơn|ít hơn|trên|lớn hơn|từ)?\s*(\d+(?:[.,]\d+)?)\s*(?:triệu|\btr\b)");
            if (amounts.Count == 0) return;
            if (amounts.Count != 1)
            {
                result.SupportStatus = QuerySupportStatus.Unsupported;
                result.UnsupportedReason = "Khoảng tiền hoặc nhiều điều kiện số tiền chưa được hỗ trợ. Hãy chọn một ngưỡng và toán tử cụ thể.";
                return;
            }
            var match = amounts[0];
            if (!decimal.TryParse(match.Groups[2].Value.Replace(',','.'),System.Globalization.NumberStyles.AllowDecimalPoint,System.Globalization.CultureInfo.InvariantCulture,out var millions) || millions > decimal.MaxValue / 1000000m)
            {
                result.SupportStatus = QuerySupportStatus.Unsupported; result.UnsupportedReason = "Số tiền không hợp lệ."; return;
            }
            string opWord = match.Groups[1].Value.Trim();
            string op = new[] { "dưới","nhỏ hơn","ít hơn" }.Contains(opWord) ? "<"
                : new[] { "trên","lớn hơn" }.Contains(opWord) ? ">"
                : new[] { "không quá","không vượt quá","tối đa" }.Contains(opWord) ? "<="
                : new[] { "từ","ít nhất","tối thiểu","không dưới","không ít hơn","không nhỏ hơn" }.Contains(opWord) ? ">=" : "=";
            decimal amount = millions * 1000000m;
            result.Filters.Add(new FilterCondition { Field="SOTIEN",Operator=op,Value=amount,DisplayText=$"Số tiền {op} {amount:N0} VNĐ" });
        }

        private void ResolveEntities(string lower, QueryUnderstandingResult result, AiAuthorizationContext ctx)
        {
            // 1. Tìm theo mã nhân viên: "mã 10", "manv: 12", "mã nv 10" (U01)
            var idMatch = Regex.Match(lower, @"(?:mã nhân viên|mã nv|manv|nv|mã)\s*[:=]?\s*(\d+)");
            if (idMatch.Success && int.TryParse(idMatch.Groups[1].Value, out int manv))
            {
                var cand = _entityResolver.ResolveEmployee(manv.ToString(), ctx);
                result.Entities.Add(cand);
            }
            else
            {
                // 2. Bóc tách tên riêng có cấu trúc
                // Loại trừ các câu hỏi nghiệp vụ tổng quan để không đoán tên bừa
                if (result.Domain != "POLICY" && result.Operation != "COUNT" && result.Operation != "SUMMARY" && !result.Time.BirthdayMonth.HasValue)
                {
                    string candidateName = null;
                    var patternName = Regex.Match(lower, @"(?:có\s+(?:nhân viên|ai)\s+(?:nào\s+)?tên\s*(?:là)?|nhân viên\s+(?:nào\s+)?tên\s*(?:là)?|thông tin\s+(?:về\s+)?nhân viên\s*(?:tên\s*(?:là)?)?|tìm\s+(?:nhân viên|thông tin)\s*(?:tên\s*(?:là)?)?|nhân viên tên là|nhân viên tên|thông tin nhân viên|lương của|phụ cấp của|tăng ca của)\s+([\p{L}\s]+?)(?:\s+(?:không|hông|ko|k|ạ|nhỉ|vậy|tháng|năm|vào|trong|trên|dưới)|$)");
                    if (patternName.Success)
                    {
                        candidateName = patternName.Groups[1].Value.Trim();
                        candidateName = Regex.Replace(candidateName, @"\b(không|hông|ko|k|ạ|nhỉ|vậy)$", "", RegexOptions.IgnoreCase).Trim();
                    }
                    else
                    {
                        var startNameMatch = Regex.Match(lower, @"^([\p{L}]+(?:\s+[\p{L}]+){0,3})\s+(?:tăng ca|làm thêm|lương|phụ cấp|sinh nhật)");
                        if (startNameMatch.Success)
                        {
                            candidateName = startNameMatch.Groups[1].Value.Trim();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(candidateName) && candidateName.Length >= 2)
                    {
                        string[] excludeKeywords = { "ai", "tổng", "tổng giờ", "xem", "tính tổng", "tất cả", "danh sách", "phòng", "bộ phận", "công ty", "nghỉ phép", "hợp đồng", "nào", "gì", "không", "ko" };
                        if (!excludeKeywords.Any(k => candidateName.Equals(k, StringComparison.OrdinalIgnoreCase)))
                        {
                            var mention = _entityResolver.ResolveEmployee(candidateName, ctx);
                            result.Entities.Add(mention);
                        }
                    }
                }
            }

            // 3. R10 fix: Tìm thực thể Phòng ban (Department): "phòng IT", "phòng ban IT", "phòng kế toán", v.v.
            var deptMatch = Regex.Match(lower, @"(?:phòng ban|phòng|bộ phận|ban)\s+([\p{L}\d]+(?:\s+[\p{L}\d]+)*?)(?=\s+(?:tháng|năm|có|trong|trên|dưới|và|tổng|tăng ca|theo)|[,.?]|$)");
            if (deptMatch.Success && !(result.Metric == "HEADCOUNT_BY_DEPT" && deptMatch.Groups[1].Value.Trim() == "ban"))
            {
                string deptCandidate = deptMatch.Groups[1].Value.Trim();
                string[] excludeDeptWords = { "nào", "gì", "mấy", "tháng", "năm", "của", "và" };
                if (!excludeDeptWords.Any(w => deptCandidate.Equals(w, StringComparison.OrdinalIgnoreCase)))
                {
                    var deptMention = _entityResolver.ResolveDepartment(deptCandidate, ctx);
                    if (deptMention != null)
                    {
                        result.Entities.Add(deptMention);
                        result.RequestedScope = "DEPARTMENT";
                    }
                }
            }
        }

        private string NormalizeKeywords(string text)
        {
            // Chuẩn hóa dấu tiếng Việt trên các từ khóa (giữ nguyên không ghép hint!)
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "nhan vien", "nhân viên" },
                { "phong ban", "phòng ban" },
                { "ke toan", "kế toán" },
                { "nhan su", "nhân sự" },
                { "sinh nhat", "sinh nhật" },
                { "tang ca", "tăng ca" },
                { "lam them", "làm thêm" },
                { "ung luong", "ứng lương" },
                { "tam ung", "tạm ứng" },
                { "phu cap", "phụ cấp" },
                { "bao hiem", "bảo hiểm" },
                { "hop dong", "hợp đồng" },
                { "thoi viec", "thôi việc" }
            };

            string res = text;
            foreach (var kvp in map)
            {
                res = Regex.Replace(res, @"\b" + Regex.Escape(kvp.Key) + @"\b", kvp.Value, RegexOptions.IgnoreCase);
            }
            return res;
        }

        private string BuildResolvedQuestion(QueryUnderstandingResult r)
        {
            var parts = new List<string>();
            parts.Add($"Domain: {r.Domain}");
            if (!string.IsNullOrEmpty(r.Operation)) parts.Add($"Op: {r.Operation}");
            if (!string.IsNullOrEmpty(r.Metric)) parts.Add($"Metric: {r.Metric}");

            var emp = r.Entities.FirstOrDefault(e => e.EntityType == "EMPLOYEE");
            if (emp != null)
            {
                parts.Add(emp.Status == "RESOLVED" ? $"Employee: #{emp.ResolvedId} ({emp.ResolvedName})" : $"Employee: {emp.MentionText} ({emp.Status})");
            }

            if (r.Time.BirthdayMonth.HasValue)
            {
                parts.Add($"BirthdayMonth: {r.Time.BirthdayMonth}");
            }
            else if (r.Time.Month.HasValue)
            {
                parts.Add($"Period: {r.Time.Month:D2}/{r.Time.Year}");
            }

            if (r.Filters.Count > 0)
            {
                parts.Add(string.Join(", ", r.Filters.Select(f => f.DisplayText)));
            }

            return string.Join(" | ", parts);
        }
    }
}
