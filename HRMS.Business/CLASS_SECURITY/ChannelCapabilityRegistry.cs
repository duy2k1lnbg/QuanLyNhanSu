using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_SECURITY
{
    public enum ChannelAction
    {
        View = 1,
        Add = 2,
        Edit = 3,
        Delete = 4,
        Print = 5
    }

    public class SupportedChannelCapability
    {
        public string FunctionCode { get; set; }
        public string Channel { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public string RestrictionNote { get; set; }
    }

    /// <summary>
    /// Registry định nghĩa khả năng hỗ trợ chức năng và hành động theo kênh
    /// theo đặc tả mục 13 và 14 trong ANTIGRAVITY_PLATFORM_ACCESS_LOGIC_AND_PLAN.md.
    /// Đây là danh mục khả năng sản phẩm (capability registry), không phải nguồn grant.
    /// </summary>
    public static class ChannelCapabilityRegistry
    {
        private static readonly Dictionary<string, SupportedChannelCapability> _capabilities
            = new Dictionary<string, SupportedChannelCapability>(StringComparer.OrdinalIgnoreCase);

        static ChannelCapabilityRegistry()
        {
            RegisterDefaultCapabilities();
        }

        private static void Register(string channel, string funcCode, bool view, bool add, bool edit, bool delete, bool print, string note = null)
        {
            string key = $"{AppChannels.Normalize(channel)}::{funcCode.Trim().ToUpperInvariant()}";
            _capabilities[key] = new SupportedChannelCapability
            {
                Channel = AppChannels.Normalize(channel),
                FunctionCode = funcCode.Trim(),
                CanView = view,
                CanAdd = add,
                CanEdit = edit,
                CanDelete = delete,
                CanPrint = print,
                RestrictionNote = note
            };
        }

        private static void RegisterDefaultCapabilities()
        {
            // =========================================================================
            // 1. KÊNH DESKTOP: Hỗ trợ đầy đủ các module HR, Chấm công, Bảng lương, System
            // =========================================================================
            string[] desktopFullModules = new string[]
            {
                "F_DM_BOPHAN", "F_DM_CHUCVU", "F_DM_CONGTY", "F_DM_DANTOC", "F_DM_NHANVIEN",
                "F_DM_PHONGBAN", "F_DM_TONGIAO", "F_DM_TRINHDO", "F_DM_LOAIPHEP",
                "F_NV_LOAIHOPDONG", "F_NV_DIEUCHUYEN", "F_NV_HOPDONG", "F_NV_KHENTHUONG",
                "F_NV_KYLUAT", "F_NV_NANGLUONG", "F_NV_THOIVIEC", "F_NV_NGHIPHEP",
                "F_CC_BANGCONG", "F_CC_BANGLUONG", "F_CC_BCCT", "F_CC_LOAICA", "F_CC_LOAICONG",
                "F_CC_PHUCAP", "F_CC_TANGCA", "F_CC_UNGLUONG", "F_CC_NGAYLE", "F_CC_BCCT_IN", "F_CC_CAPNHATCONG",
                "F_BC_BAOCAO", "F_DB_LUONG", "F_DB_NHANSU",
                "F_SYSTEM_USER", "F_SYSTEM_GROUP", "F_SYSTEM_PHUCHOI", "F_SYSTEM_SAULUU",
                "F_SYSTEM_LOCK_USER", "F_SYSTEM_SETTING", "F_SYSTEM_GIAMSAT", "F_SYSTEM_AI",
                "F_SYSTEM_AI_CONFIG", "F_SYSTEM_CAPTAIKHOAN", "F_SYSTEM_PQ_CHUCNANG",
                "F_SYSTEM_PQ_BAOCAO", "F_SYSTEM_THONGBAO", "F_SYSTEM_DB_CONFIG", "DOIMATKHAU"
            };

            foreach (var m in desktopFullModules)
            {
                Register(AppChannels.Desktop, m, true, true, true, true, true);
            }

            // =========================================================================
            // 2. KÊNH WEB PORTAL: Hỗ trợ quản trị, báo cáo, hồ sơ, phê duyệt, AI Chat.
            // ĐẶC BIỆT: Bảng lương (F_CC_BANGLUONG) CHỈ hỗ trợ Xem (View), khóa các action tính/ghi lương!
            // =========================================================================
            Register(AppChannels.Web, "F_CC_BANGLUONG", true, false, false, false, false, "Web chỉ được xem bảng lương đã công bố, không được tính/sửa lương.");
            Register(AppChannels.Web, "F_NV_PHEDUYET", true, true, true, false, true, "Phê duyệt yêu cầu trực tuyến.");

            string[] webSupportedModules = new string[]
            {
                "F_DM_BOPHAN", "F_DM_CHUCVU", "F_DM_CONGTY", "F_DM_DANTOC", "F_DM_NHANVIEN",
                "F_DM_PHONGBAN", "F_DM_TONGIAO", "F_DM_TRINHDO", "F_DM_LOAIPHEP",
                "F_NV_LOAIHOPDONG", "F_NV_DIEUCHUYEN", "F_NV_HOPDONG", "F_NV_KHENTHUONG",
                "F_NV_KYLUAT", "F_NV_NANGLUONG", "F_NV_THOIVIEC", "F_NV_NGHIPHEP",
                "F_CC_BANGCONG", "F_CC_BCCT", "F_CC_NGAYLE",
                "F_BC_BAOCAO", "F_DB_LUONG", "F_DB_NHANSU",
                "F_SYSTEM_USER", "F_SYSTEM_GROUP", "F_SYSTEM_LOCK_USER", "F_SYSTEM_CAPTAIKHOAN",
                "F_SYSTEM_PQ_CHUCNANG", "F_SYSTEM_PQ_BAOCAO", "F_SYSTEM_THONGBAO",
                "F_SYSTEM_AI", "F_SYSTEM_AI_CONFIG", "DOIMATKHAU"
            };

            foreach (var m in webSupportedModules)
            {
                Register(AppChannels.Web, m, true, true, true, true, true);
            }

            // =========================================================================
            // 3. KÊNH MOBILE APP: Chuyên biệt cho nhân viên cá nhân (các mã MOBILE_*)
            // =========================================================================
            Register(AppChannels.Mobile, "MOBILE_PROFILE_VIEW", true, false, false, false, false, "Xem hồ sơ cá nhân trên Mobile");
            Register(AppChannels.Mobile, "MOBILE_ATTENDANCE_VIEW", true, false, false, false, false, "Xem bảng công cá nhân trên Mobile");
            Register(AppChannels.Mobile, "MOBILE_PAYROLL_VIEW", true, false, false, false, false, "Xem phiếu lương đã công bố trên Mobile");
            Register(AppChannels.Mobile, "MOBILE_CONTRACT_VIEW", true, false, false, false, false, "Xem hợp đồng lao động trên Mobile");
            Register(AppChannels.Mobile, "MOBILE_INSURANCE_VIEW", true, false, false, false, false, "Xem bảo hiểm trên Mobile");
            Register(AppChannels.Mobile, "MOBILE_NOTIFICATION_VIEW", true, false, false, false, false, "Xem thông báo nội bộ trên Mobile");
            Register(AppChannels.Mobile, "MOBILE_REQUEST_LEAVE", true, true, false, false, false, "Gửi đơn xin nghỉ phép từ Mobile");
            Register(AppChannels.Mobile, "MOBILE_REQUEST_OVERTIME", true, true, false, false, false, "Gửi đơn đăng ký tăng ca từ Mobile");
            Register(AppChannels.Mobile, "MOBILE_REQUEST_ADVANCE", true, true, false, false, false, "Gửi yêu cầu tạm ứng lương từ Mobile");
            Register(AppChannels.Mobile, "DOIMATKHAU", true, false, true, false, false, "Đổi mật khẩu tài khoản");
        }

        /// <summary>
        /// Kiểm tra xem kênh có hỗ trợ hành động cụ thể cho chức năng hay không
        /// </summary>
        public static bool IsActionSupported(string channel, string functionCode, ChannelAction action)
        {
            if (string.IsNullOrWhiteSpace(channel) || string.IsNullOrWhiteSpace(functionCode)) return false;

            string normChannel = AppChannels.Normalize(channel);
            if (normChannel == null) return false;

            // Quyền cha F_LOGIN_* không có action Thêm/Sửa/Xóa/In
            if (PlatformFunctionCodes.IsPlatformFunction(functionCode))
            {
                return action == ChannelAction.View;
            }

            string key = $"{normChannel}::{functionCode.Trim().ToUpperInvariant()}";
            if (!_capabilities.TryGetValue(key, out var cap))
            {
                // Mặc định từ chối: Bất kỳ chức năng nào không được đăng ký cụ thể đều bị cấm trên mọi kênh
                return false;
            }

            switch (action)
            {
                case ChannelAction.View: return cap.CanView;
                case ChannelAction.Add: return cap.CanAdd;
                case ChannelAction.Edit: return cap.CanEdit;
                case ChannelAction.Delete: return cap.CanDelete;
                case ChannelAction.Print: return cap.CanPrint;
                default: return false;
            }
        }

        /// <summary>
        /// Lấy chi tiết khả năng được hỗ trợ cho chức năng trên kênh
        /// </summary>
        public static SupportedChannelCapability GetCapability(string channel, string functionCode)
        {
            string normChannel = AppChannels.Normalize(channel);
            if (normChannel == null || string.IsNullOrWhiteSpace(functionCode)) return null;

            string key = $"{normChannel}::{functionCode.Trim().ToUpperInvariant()}";
            if (_capabilities.TryGetValue(key, out var cap))
            {
                return cap;
            }

            // Mặc định từ chối: Chức năng không được hỗ trợ trên kênh
            return new SupportedChannelCapability
            {
                Channel = normChannel,
                FunctionCode = functionCode.Trim(),
                CanView = false,
                CanAdd = false,
                CanEdit = false,
                CanDelete = false,
                CanPrint = false,
                RestrictionNote = $"Chức năng [{functionCode.Trim()}] không được hỗ trợ trên kênh {normChannel}."
            };
        }
    }
}
