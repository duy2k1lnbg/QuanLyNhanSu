using System;
using System.Collections.Generic;

namespace Bu.Services.AI_Services.Security
{
    public class AiCapabilityDefinition
    {
        public string CapabilityCode { get; set; }
        public string Domain { get; set; }
        public string Operation { get; set; }
        public string RequiredFunctionCode { get; set; }
        public string SourceView { get; set; }
        public string FieldProfile { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsAggregateOnly { get; set; } = false;
        public string Description { get; set; }
    }

    /// <summary>
    /// Danh mục khả năng (Capabilities) chuẩn hóa cho AI RAG V2
    /// </summary>
    public static class AiCapabilityCatalog
    {
        private static readonly Dictionary<string, AiCapabilityDefinition> _catalog = new Dictionary<string, AiCapabilityDefinition>(StringComparer.OrdinalIgnoreCase);

        static AiCapabilityCatalog()
        {
            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "EMPLOYEE_LOOKUP",
                Domain = "EMPLOYEE",
                Operation = "LOOKUP",
                RequiredFunctionCode = "F_DM_NHANVIEN",
                SourceView = "V_AI_EMPLOYEE_LOOKUP",
                FieldProfile = "DEFAULT",
                Description = "Tra cứu danh tính và thông tin cơ bản của nhân sự"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "EMPLOYEE_PROFILE",
                Domain = "EMPLOYEE",
                Operation = "PROFILE",
                RequiredFunctionCode = "F_DM_NHANVIEN",
                SourceView = "V_AI_EMPLOYEE",
                FieldProfile = "SENSITIVE",
                Description = "Xem hồ sơ chi tiết (ngày sinh, giới tính, địa chỉ)"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "EMPLOYEE_COUNT",
                Domain = "EMPLOYEE",
                Operation = "COUNT",
                RequiredFunctionCode = "F_DM_NHANVIEN",
                SourceView = "V_AI_EMPLOYEE_COUNT",
                FieldProfile = "DEFAULT",
                IsAggregateOnly = true,
                Description = "Đếm số lượng nhân viên theo phòng ban"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "ATTENDANCE_DETAIL",
                IsEnabled = false, // Raw daily clock events are not mapped yet.
                Domain = "ATTENDANCE",
                Operation = "DETAIL",
                RequiredFunctionCode = "F_CC_BANGCONG",
                SourceView = "V_AI_ATTENDANCE",
                FieldProfile = "DEFAULT",
                Description = "Xem giờ vào ra và chi tiết quẹt thẻ chấm công"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "ATTENDANCE_SUMMARY",
                Domain = "ATTENDANCE",
                Operation = "SUMMARY",
                RequiredFunctionCode = "F_CC_BANGCONG",
                SourceView = "V_AI_ATTENDANCE_SUMMARY",
                FieldProfile = "DEFAULT",
                Description = "Xem tổng hợp ngày công và ngày phép theo kỳ"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "OVERTIME_VIEW",
                Domain = "OVERTIME",
                Operation = "VIEW",
                RequiredFunctionCode = "F_CC_TANGCA",
                SourceView = "V_AI_OVERTIME",
                FieldProfile = "DEFAULT",
                Description = "Xem danh sách và số giờ làm thêm (tăng ca)"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "OVERTIME_SUM",
                Domain = "OVERTIME",
                Operation = "SUM",
                RequiredFunctionCode = "F_CC_TANGCA",
                SourceView = "V_AI_OVERTIME_SUMMARY",
                FieldProfile = "DEFAULT",
                IsAggregateOnly = true,
                Description = "Tính tổng số giờ tăng ca hoặc xếp hạng làm thêm"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "ALLOWANCE_VIEW",
                Domain = "ALLOWANCE",
                Operation = "VIEW",
                RequiredFunctionCode = "F_CC_PHUCAP",
                SourceView = "V_AI_ALLOWANCE",
                FieldProfile = "DEFAULT",
                Description = "Xem danh sách phụ cấp nhân sự"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "INSURANCE_SELF",
                Domain = "INSURANCE",
                Operation = "SELF",
                RequiredFunctionCode = null, // Ai cũng được xem BHXH của chính mình
                SourceView = "V_AI_INSURANCE",
                FieldProfile = "DEFAULT",
                Description = "Xem thông tin bảo hiểm xã hội của chính mình"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "INSURANCE_VIEW",
                Domain = "INSURANCE",
                Operation = "VIEW",
                RequiredFunctionCode = null,
                IsEnabled = false, // Management insurance right has not been mapped to a real system function.
                SourceView = "V_AI_INSURANCE",
                FieldProfile = "SENSITIVE",
                Description = "Quản lý tra cứu bảo hiểm của nhân viên"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "ADVANCE_VIEW",
                Domain = "ADVANCE",
                Operation = "VIEW",
                RequiredFunctionCode = "F_CC_UNGLUONG",
                SourceView = "V_AI_ADVANCE",
                FieldProfile = "DEFAULT",
                Description = "Xem dữ liệu tạm ứng lương"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "PAYROLL_SELF",
                Domain = "PAYROLL",
                Operation = "SELF",
                IsEnabled = true,
                RequiredFunctionCode = null, // Tự tra cứu lương cá nhân
                SourceView = "V_AI_PAYROLL",
                FieldProfile = "DEFAULT",
                Description = "Nhân viên xem phiếu lương cá nhân"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "PAYROLL_VIEW",
                Domain = "PAYROLL",
                Operation = "VIEW",
                IsEnabled = true,
                RequiredFunctionCode = "F_CC_BANGLUONG",
                SourceView = "V_AI_PAYROLL",
                FieldProfile = "SENSITIVE",
                Description = "Quản lý xem bảng lương chi tiết của nhân sự"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "PAYROLL_SUMMARY",
                Domain = "PAYROLL",
                Operation = "SUMMARY",
                IsEnabled = true,
                RequiredFunctionCode = "F_CC_BANGLUONG",
                SourceView = "V_AI_PAYROLL_SUMMARY",
                FieldProfile = "AGGREGATE_ONLY",
                IsAggregateOnly = true,
                Description = "Thống kê tổng quỹ lương theo kỳ công"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "CONTRACT_VIEW",
                Domain = "CONTRACT",
                Operation = "VIEW",
                RequiredFunctionCode = "F_NV_HOPDONG",
                SourceView = "V_AI_CONTRACT",
                FieldProfile = "DEFAULT",
                Description = "Xem danh sách hợp đồng lao động và hạn hết hạn"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "SALARY_CHANGE_VIEW",
                Domain = "SALARY_CHANGE",
                Operation = "VIEW",
                RequiredFunctionCode = "F_NV_NANGLUONG",
                SourceView = "V_AI_SALARY_CHANGE",
                FieldProfile = "DEFAULT",
                Description = "Xem quyết định nâng lương"
            });

            Register(new AiCapabilityDefinition
            {
                CapabilityCode = "POLICY_LOOKUP",
                Domain = "POLICY",
                Operation = "LOOKUP",
                RequiredFunctionCode = null, // Mặc định người dùng xác thực được tra cứu quy chế công khai, trừ khi có DENY hoặc cấu hình riêng
                SourceView = "hrms_vectors_v2",
                FieldProfile = "DEFAULT",
                Description = "Tra cứu quy chế, quy trình và văn bản chính sách nhân sự"
            });
        }

        private static void Register(AiCapabilityDefinition cap)
        {
            _catalog[cap.CapabilityCode] = cap;
        }

        public static void SetEnabled(string capabilityCode, bool enabled)
        {
            if (!string.IsNullOrWhiteSpace(capabilityCode) && _catalog.TryGetValue(capabilityCode, out var def))
            {
                def.IsEnabled = enabled;
            }
        }

        public static void ApplyBranchB2Profile()
        {
            // B2: 11 capabilities enabled, 5 disabled
            SetEnabled("PAYROLL_SELF", false);
            SetEnabled("PAYROLL_VIEW", false);
            SetEnabled("PAYROLL_SUMMARY", false);
            SetEnabled("ATTENDANCE_DETAIL", false);
            SetEnabled("INSURANCE_VIEW", false);
        }

        public static AiCapabilityDefinition Get(string capabilityCode)
        {
            if (string.IsNullOrWhiteSpace(capabilityCode)) return null;
            return _catalog.TryGetValue(capabilityCode, out var def) ? def : null;
        }

        public static bool Exists(string capabilityCode)
        {
            return !string.IsNullOrWhiteSpace(capabilityCode) && _catalog.ContainsKey(capabilityCode);
        }

        public static IReadOnlyList<AiCapabilityDefinition> GetAll()
        {
            return new List<AiCapabilityDefinition>(_catalog.Values);
        }
    }
}
