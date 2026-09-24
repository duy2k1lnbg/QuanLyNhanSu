using System;
using System.Collections.Generic;

namespace HRMS_API.Models
{
    public class MobileMeDto
    {
        public int IdUser { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public decimal? Manv { get; set; }
        public string EmployeeCode { get; set; }
        public string ClientType { get; set; }
        public string MaCty { get; set; }
        public string MaDvi { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Rights { get; set; } = new List<string>();
    }

    public class MobileProfileDto
    {
        public decimal Manv { get; set; }
        public string EmployeeCode { get; set; }
        public string Hoten { get; set; }
        public string Gioitinh { get; set; }
        public string Ngaysinh { get; set; }
        public string Dienthoai { get; set; }
        public string Cccd { get; set; }
        public string Diachi { get; set; }
        public string TenPhongBan { get; set; }
        public string TenBoPhan { get; set; }
        public string TenChucVu { get; set; }
        public string TenTrinhDo { get; set; }
        public string NgayVaoLam { get; set; }
        public string Email { get; set; }
        public string AvatarBase64 { get; set; }
        public string TrangThaiLaoDong { get; set; }
    }

    public class MobileAttendanceDailyDto
    {
        public string Ngay { get; set; }
        public string Thu { get; set; }
        public string GioVao { get; set; }
        public string GioRa { get; set; }
        public decimal NgayCong { get; set; }
        public string KyHieu { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
    }

    public class MobileAttendanceSummaryDto
    {
        public int Makycong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal TongNgayCong { get; set; }
        public decimal NgayCongChuan { get; set; }
        public decimal CongNgay { get; set; }
        public decimal CongDem { get; set; }
        public decimal NgayPhep { get; set; }
        public decimal CongLe { get; set; }
        public decimal CongChuNhat { get; set; }
        public decimal SoGioTangCa { get; set; }
        public int SoLanDiMuon { get; set; }
    }

    public class MobileAttendanceDto
    {
        public MobileAttendanceSummaryDto Summary { get; set; }
        public List<MobileAttendanceDailyDto> DailyList { get; set; } = new List<MobileAttendanceDailyDto>();
    }

    public class MobilePayrollDto
    {
        public decimal Idbl { get; set; }
        public int Makycong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal CongChuan { get; set; }
        public decimal CongThucTe { get; set; }
        public decimal CongLamNgay { get; set; }
        public decimal CongLamDem { get; set; }
        public decimal SoGioTangCa { get; set; }
        public decimal DailyRate { get; set; }
        public decimal LuongCaNgay { get; set; }
        public decimal LuongCaDem { get; set; }
        public decimal LuongCongThucTe { get; set; }
        public decimal PhuCapCongThucTe { get; set; }
        public decimal TienTangCa { get; set; }
        public decimal TienChuyenCan { get; set; }
        public decimal TienAnCa { get; set; }
        public decimal KhoanCongKhac { get; set; }
        public decimal TongThuNhap { get; set; }
        public decimal TienBhxh { get; set; }
        public decimal TienBhyt { get; set; }
        public decimal TienBhtn { get; set; }
        public decimal TienCongDoan { get; set; }
        public decimal TienTamUng { get; set; }
        public decimal ThueTncn { get; set; }
        public decimal HoanThue { get; set; }
        public decimal KhoanTruKhac { get; set; }
        public decimal TongKhauTru { get; set; }
        public decimal ThucLinh { get; set; }
        public string TrangThaiChiTra { get; set; }

        // Tax breakdown
        public decimal ThuNhapChiuThue { get; set; }
        public decimal ThuNhapTinhThue { get; set; }
        public decimal GiamTruBanThan { get; set; }
        public decimal GiamTruPhuThuoc { get; set; }
        public decimal GiamTruBaoHiem { get; set; }
        public int SoNguoiPhuThuoc { get; set; }

        // Explanations & Policy
        public string CachTinhLuong { get; set; }
        public string CachTinhThue { get; set; }
        public string PayrollVersion { get; set; }
        public string ChinhSachApDung { get; set; }
        public bool IsLegacy { get; set; }
    }

    public class MobileContractDto
    {
        public string Sohd { get; set; }
        public decimal? Loaihd { get; set; }
        public string TenLoaihd { get; set; }
        public string Ngaybatdau { get; set; }
        public string Ngayketthuc { get; set; }
        public string Ngayky { get; set; }
        public decimal? Lanky { get; set; }
        public string Thoihan { get; set; }
        public decimal? LuongThoaThuan { get; set; }
        public decimal? HeSoLuong { get; set; }
        public string NoiDung { get; set; }
        public bool IsExpiringSoon { get; set; }
    }

    public class MobileInsuranceDto
    {
        public decimal Idbh { get; set; }
        public string Sobh { get; set; }
        public string Ngaycap { get; set; }
        public string Noicap { get; set; }
        public string Noikhambenh { get; set; }
        public decimal? LuongBhxh { get; set; }
    }

    public class MobileNotificationDto
    {
        public decimal Id { get; set; }
        public string Tieude { get; set; }
        public string Noidung { get; set; }
        public string Nguoidang { get; set; }
        public string Ngaydang { get; set; }
        public string Loaitb { get; set; }
        public bool IsPinned { get; set; }
        public string FileDinhkem { get; set; }
        public bool IsUnread { get; set; }
    }

    public class MobileDashboardDto
    {
        public MobileProfileDto ProfileSummary { get; set; }
        public MobileAttendanceSummaryDto AttendanceSummary { get; set; }
        public MobilePayrollDto PayrollSummary { get; set; }
        public bool HasExpiringContract { get; set; }
        public string ExpiringContractInfo { get; set; }
        public int UnreadNotificationCount { get; set; }
        public List<MobileNotificationDto> RecentNotifications { get; set; } = new List<MobileNotificationDto>();
    }

    public class UpdateProfileRequest
    {
        public string Dienthoai { get; set; }
        public string Diachi { get; set; }
        public string AvatarBase64 { get; set; }
    }

    public class MobileLeaveRequestDto
    {
        public decimal IdYeuCau { get; set; }
        public decimal Manv { get; set; }
        public string LoaiNghi { get; set; }
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
        public decimal SoNgay { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; }
        public string NgayTao { get; set; }
        public string NguoiDuyet { get; set; }
        public string NgayDuyet { get; set; }
        public string LyDoTuChoi { get; set; }
    }

    public class CreateLeaveRequest
    {
        public string LoaiNghi { get; set; }
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
        public decimal? SoNgay { get; set; }
        public string LyDo { get; set; }
    }

    public class MobileAttendanceCorrectionDto
    {
        public decimal IdYeuCau { get; set; }
        public decimal Manv { get; set; }
        public string NgayCong { get; set; }
        public string GioVaoMoi { get; set; }
        public string GioRaMoi { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; }
        public string NgayTao { get; set; }
        public string NguoiDuyet { get; set; }
        public string NgayDuyet { get; set; }
        public string LyDoTuChoi { get; set; }
    }

    public class CreateAttendanceCorrectionRequest
    {
        public string NgayCong { get; set; }
        public string GioVaoMoi { get; set; }
        public string GioRaMoi { get; set; }
        public string LyDo { get; set; }
    }

    public class MobileOvertimeRequestDto
    {
        public decimal Id { get; set; }
        public decimal IdYeuCau { get; set; }
        public decimal Manv { get; set; }
        public string NgayTangCa { get; set; }
        public string OtDate { get; set; }
        public decimal SoGio { get; set; }
        public decimal Hours { get; set; }
        public decimal? IdCa { get; set; }
        public string TenCa { get; set; }
        public string ShiftType { get; set; }
        public decimal HeSo { get; set; }
        public decimal Coefficient { get; set; }
        public string NoiDung { get; set; }
        public string Reason { get; set; }
        public string TrangThai { get; set; }
        public string Status { get; set; }
        public string NgayTao { get; set; }
        public string CreatedAt { get; set; }
        public string NguoiDuyet { get; set; }
        public string NgayDuyet { get; set; }
        public string LyDoTuChoi { get; set; }
        public string Note { get; set; }
    }

    public class CreateOvertimeRequest
    {
        public string NgayTangCa { get; set; }
        public string OtDate { get; set; }
        public decimal SoGio { get; set; }
        public decimal Hours { get; set; }
        public decimal? IdCa { get; set; }
        public string ShiftType { get; set; }
        public decimal? HeSo { get; set; }
        public string NoiDung { get; set; }
        public string Reason { get; set; }
    }

    public class UnifiedRequestDto
    {
        public decimal Id { get; set; }
        public string RequestType { get; set; } // 'LEAVE', 'ATTENDANCE', 'OVERTIME'
        public string TypeLabel { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string DateRange { get; set; }
        public string Status { get; set; } // 'PENDING', 'APPROVED', 'REJECTED'
        public string CreatedAt { get; set; }
        public string Reason { get; set; }
        public string RejectionReason { get; set; }
    }
}
