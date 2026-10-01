using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Bu.CLASS_CHAMCONG
{
    #region DTOs

    public class ShiftFrameDto
    {
        public int Stt { get; set; }
        public int BatDauPhut { get; set; }
        public int KetThucPhut { get; set; }
        public string LoaiKhungGio { get; set; } // LAM_VIEC, NGHI_HUONG_LUONG, NGHI_KHONG_LUONG
        public int BatBuocQuetThe { get; set; }
    }

    public class ShiftVersionDto
    {
        public long IdCaPhienBan { get; set; }
        public long IdLoaiCa { get; set; }
        public int SoPhienBan { get; set; }
        public string TenPhienBan { get; set; }
        public long TongGiayChuan { get; set; } // e.g. 28800
        public decimal CongQuyDoi { get; set; } = 1.0m;
        public List<ShiftFrameDto> KhungGios { get; set; } = new List<ShiftFrameDto>();
    }

    public class AttendancePolicyDto
    {
        public long IdQuyDinh { get; set; }
        public string MaQuyDinh { get; set; }
        public int SoPhienBan { get; set; } = 1;
        public int GiayMienViPhamMuon { get; set; } = 0;
        public int GiayMienViPhamSom { get; set; } = 0;
        public int GiayDungSaiHuongCong { get; set; } = 300; // 5 mins
        public string CachHuongDungSai { get; set; } = "DU_CONG_NEU_TRONG_NGUONG"; // KHONG_BU, DU_CONG_NEU_TRONG_NGUONG
        public string CachDemMuon { get; set; } = "TOAN_BO_NEU_VUOT"; // TOAN_BO_NEU_VUOT, PHAN_VUOT_NGUONG
        public int CuaSoVaoTruocGiay { get; set; } = 3600;
        public int CuaSoRaSauGiay { get; set; } = 7200;
        public int DemBatDauPhut { get; set; } = 1320; // 22:00
        public int DemKetThucPhut { get; set; } = 360;  // 06:00
        public string KieuLamTron { get; set; } = "EXACT";
        public int BuocLamTronGiay { get; set; } = 1;
    }

    public class ScheduleDto
    {
        public long IdLich { get; set; }
        public long? IdQuyDinh { get; set; }
        public long MaNV { get; set; }
        public DateTime Ngay { get; set; }
        public long? IdCaPhienBan { get; set; }
        public DateTime? BatDauKeHoach { get; set; }
        public DateTime? KetThucKeHoach { get; set; }
        public string TrangThaiPhanCong { get; set; } = "LAM_VIEC"; // LAM_VIEC, NGHI, CHUA_XAC_DINH
        public string LoaiNgay { get; set; } = "THUONG"; // THUONG, NGHI_TUAN, LE, CHUA_XAC_DINH
        public string TrangThai { get; set; } = "PUBLISHED";
    }

    public class RawPunchDto
    {
        public long Mabc { get; set; }
        public long MaNV { get; set; }
        public DateTime? ThoiDiemVao { get; set; }
        public DateTime? ThoiDiemRa { get; set; }
        public string NguonCham { get; set; } = "DEVICE";
        public string MaThietBi { get; set; }
    }

    public class ApprovedOvertimeDto
    {
        public long Id { get; set; }
        public long MaNV { get; set; }
        public DateTime BatDauDuyet { get; set; }
        public DateTime KetThucDuyet { get; set; }
        public long GiayOtDuyet { get; set; }
    }

    public class ApprovedLeaveDto
    {
        public long Id { get; set; }
        public long MaNV { get; set; }
        public DateTime BatDauNghi { get; set; }
        public DateTime KetThucNghi { get; set; }
        public long GiayNghi { get; set; }
        public int CoHuongLuong { get; set; } = 1; // 1: Có lương, 0: Không lương
        public string LoaiHuongCong { get; set; } = "PHEP_NAM"; // PHEP_NAM, NGHI_HUONG_LUONG, NGHI_BHXH, KHONG_LUONG
        public string NguonChiTra { get; set; } = "CONG_TY";
    }

    public class ApprovedAdjustmentDto
    {
        public long Id { get; set; }
        public long MaNV { get; set; }
        public DateTime Ngay { get; set; }
        public DateTime? ThoiDiemVaoMoi { get; set; }
        public DateTime? ThoiDiemRaMoi { get; set; }
        public long? IdBangCongGoc { get; set; }
    }

    public class TimeSegmentDto
    {
        public long IdPhanDoan { get; set; }
        public long IdLanTinh { get; set; }
        public long IdBangCongCt { get; set; }
        public long MaNV { get; set; }
        public long? IdLich { get; set; }
        public long IdQuyDinh { get; set; }
        public DateTime BatDauLuc { get; set; }
        public DateTime KetThucLuc { get; set; }
        public long ThoiLuongGiay { get; set; }
        public string LoaiThoiGian { get; set; }
        public string LoaiNgay { get; set; } = "THUONG";
        public int LaBanDem { get; set; } = 0;
        public int CoOtBanNgayTruoc { get; set; } = 0;
        public string TrangThaiXacNhan { get; set; } = "DA_XAC_NHAN"; // CHO_XAC_MINH, DA_XAC_NHAN, KHONG_TINH
        public long GiayThucTe { get; set; } = 0;
        public long GiayHuongCongThuong { get; set; } = 0;
        public long GiayOtXacNhan { get; set; } = 0;
        public long GiayDemTrongGioThuong { get; set; } = 0;
        public long GiayDemOt { get; set; } = 0;
        public long GiayDiMuonThucTe { get; set; } = 0;
        public long GiayVeSomThucTe { get; set; } = 0;
        public long GiayDiMuonViPham { get; set; } = 0;
        public long GiayVeSomViPham { get; set; } = 0;
        public long? IdYeuCauTangCa { get; set; }
        public long? IdYeuCauNghiPhep { get; set; }
        public long? IdYeuCauDieuChinh { get; set; }
        public List<long> SourceMabcList { get; set; } = new List<long>();
    }

    public class AttendanceAnomalyDto
    {
        public long IdBatThuong { get; set; }
        public string MaSuViecHash { get; set; }
        public long IdLanTinhPhatHien { get; set; }
        public long IdBangCongCt { get; set; }
        public long MaNV { get; set; }
        public long? IdLich { get; set; }
        public DateTime Ngay { get; set; }
        public string MaLoi { get; set; }
        public DateTime? BatDauLuc { get; set; }
        public DateTime? KetThucLuc { get; set; }
        public string MucDo { get; set; } // THONG_TIN, CAN_RA_SOAT, NGHIEP_VU_TREO
        public int ChanChot { get; set; } // 0 or 1
        public string TrangThai { get; set; } = "CHO_XU_LY";
        public string MoTa { get; set; }
        public string InputHash { get; set; }
        public string InputSnapshotJson { get; set; }
    }

    public class DayCalculationInput
    {
        public long IdLanTinh { get; set; }
        public long IdBangCongCt { get; set; }
        public long MaNV { get; set; }
        public long MaKyCong { get; set; }
        public DateTime Ngay { get; set; }
        public ScheduleDto Schedule { get; set; }
        public ShiftVersionDto Shift { get; set; }
        public AttendancePolicyDto Policy { get; set; }
        public RawPunchDto RawPunch { get; set; }
        public List<RawPunchDto> RawPunches { get; set; } = new List<RawPunchDto>();
        public List<ApprovedOvertimeDto> ApprovedOvertimes { get; set; } = new List<ApprovedOvertimeDto>();
        public List<ApprovedLeaveDto> ApprovedLeaves { get; set; } = new List<ApprovedLeaveDto>();
        public ApprovedAdjustmentDto ApprovedAdjustment { get; set; }
    }

    public class DayCalculationResult
    {
        public long IdLanTinh { get; set; }
        public long IdBangCongCt { get; set; }
        public long MaNV { get; set; }
        public long MaKyCong { get; set; }
        public DateTime Ngay { get; set; }
        public string InputHash { get; set; }
        public string InputSnapshotJson { get; set; }
        public string TrangThaiCong { get; set; } = "CHO_XAC_MINH"; // CHO_XAC_MINH, DA_XAC_NHAN
        public int DuDieuKienChot { get; set; } = 0;
        public long GiayThucTe { get; set; } = 0;
        public long GiayHuongCongThuong { get; set; } = 0;
        public long GiayOtXacNhan { get; set; } = 0;
        public long GiayDemTrongGioThuong { get; set; } = 0;
        public long GiayDemOt { get; set; } = 0;
        public long GiayDiMuonThucTe { get; set; } = 0;
        public long GiayVeSomThucTe { get; set; } = 0;
        public long GiayDiMuonViPham { get; set; } = 0;
        public long GiayVeSomViPham { get; set; } = 0;
        public decimal CongThuongQuyDoi { get; set; } = 0m;
        public List<TimeSegmentDto> Segments { get; set; } = new List<TimeSegmentDto>();
        public List<AttendanceAnomalyDto> Anomalies { get; set; } = new List<AttendanceAnomalyDto>();
    }

    #endregion

    /// <summary>
    /// Thuật toán phân đoạn thời gian (TimeSegmentationEngine) độc lập, thuần túy,
    /// tuân thủ đúng Điều 106, 109 BLLĐ 2019, Điều 57, 64 NĐ 145/2020/NĐ-CP và DDL V1_18.
    /// </summary>
    public class TimeSegmentationEngine
    {
        public DayCalculationResult ProcessDay(DayCalculationInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var result = new DayCalculationResult
            {
                IdLanTinh = input.IdLanTinh,
                IdBangCongCt = input.IdBangCongCt,
                MaNV = input.MaNV,
                MaKyCong = input.MaKyCong,
                Ngay = input.Ngay.Date
            };

            // 1. Chuẩn hóa nguồn chấm công: lấy từ RawPunches hoặc RawPunch
            var punches = new List<RawPunchDto>();
            if (input.RawPunches != null && input.RawPunches.Count > 0)
            {
                punches.AddRange(input.RawPunches);
            }
            else if (input.RawPunch != null)
            {
                punches.Add(input.RawPunch);
            }

            // Nếu có điều chỉnh đã duyệt, thay thế các lượt chấm
            if (input.ApprovedAdjustment != null && input.ApprovedAdjustment.ThoiDiemVaoMoi.HasValue && input.ApprovedAdjustment.ThoiDiemRaMoi.HasValue)
            {
                punches.Clear();
                punches.Add(new RawPunchDto
                {
                    Mabc = 0,
                    MaNV = input.MaNV,
                    ThoiDiemVao = input.ApprovedAdjustment.ThoiDiemVaoMoi,
                    ThoiDiemRa = input.ApprovedAdjustment.ThoiDiemRaMoi,
                    NguonCham = "ADJUSTMENT"
                });
            }

            var policy = input.Policy ?? new AttendancePolicyDto();
            var schedule = input.Schedule;
            var shift = input.Shift;

            // 2. Chụp Canonical Snapshot JSON (Bao gồm đầy đủ Chính sách & mọi lượt chấm để phát hiện thay đổi)
            var snapshotObj = new
            {
                MaNV = input.MaNV,
                Ngay = input.Ngay.ToString("yyyy-MM-dd"),
                Schedule = schedule != null ? new { schedule.IdLich, schedule.TrangThaiPhanCong, schedule.BatDauKeHoach, schedule.KetThucKeHoach, schedule.LoaiNgay } : null,
                Shift = shift != null ? new { shift.IdCaPhienBan, shift.TenPhienBan, shift.TongGiayChuan, shift.CongQuyDoi, Frames = shift.KhungGios } : null,
                Policy = policy != null ? new
                {
                    policy.IdQuyDinh,
                    policy.MaQuyDinh,
                    policy.GiayMienViPhamMuon,
                    policy.GiayMienViPhamSom,
                    policy.GiayDungSaiHuongCong,
                    policy.CachHuongDungSai,
                    policy.CachDemMuon,
                    policy.KieuLamTron,
                    policy.BuocLamTronGiay,
                    policy.DemBatDauPhut,
                    policy.DemKetThucPhut
                } : null,
                Punches = punches.OrderBy(p => p.ThoiDiemVao).ThenBy(p => p.ThoiDiemRa).ThenBy(p => p.Mabc).Select(p => new { p.Mabc, p.ThoiDiemVao, p.ThoiDiemRa, p.NguonCham }),
                Overtime = input.ApprovedOvertimes?.OrderBy(o => o.Id).Select(o => new { o.Id, o.BatDauDuyet, o.KetThucDuyet, o.GiayOtDuyet }),
                Leave = input.ApprovedLeaves?.OrderBy(l => l.Id).Select(l => new { l.Id, l.BatDauNghi, l.KetThucNghi, l.CoHuongLuong, l.LoaiHuongCong, l.NguonChiTra, l.GiayNghi })
            };
            result.InputSnapshotJson = JsonConvert.SerializeObject(snapshotObj);
            result.InputHash = ComputeSha256(result.InputSnapshotJson);

            bool isWorkingDay = schedule != null && schedule.TrangThaiPhanCong == "LAM_VIEC";
            if (schedule == null || schedule.TrangThaiPhanCong == "CHUA_XAC_DINH" ||
                (isWorkingDay && (shift == null || input.Policy == null)))
            {
                result.Anomalies.Add(CreateAnomaly(input, "THIEU_CAU_HINH",
                    "Missing published schedule, shift or policy; cannot confirm zero attendance.",
                    null, null, "NGHIEP_VU_TREO", 1));
            }
            if (policy.KieuLamTron != "EXACT" || policy.BuocLamTronGiay != 1)
                result.Anomalies.Add(CreateAnomaly(input, "QUY_TAC_CHUA_HO_TRO",
                    "Non-exact rounding must be implemented at the payment stage before approval.",
                    null, null, "NGHIEP_VU_TREO", 1));


            // 3. Kiểm tra các bất thường dữ liệu vào cơ bản
            // 3.0. Giờ vào/ra đảo ngược trong cùng ngày (punchOut <= punchIn)
            foreach (var p in punches)
            {
                if (p.ThoiDiemVao.HasValue && p.ThoiDiemRa.HasValue && p.ThoiDiemRa.Value <= p.ThoiDiemVao.Value)
                {
                    result.Anomalies.Add(CreateAnomaly(
                        input,
                        "GIO_DAO_NGUOC",
                        $"Giờ ra ({p.ThoiDiemRa.Value:HH:mm:ss}) nhỏ hơn hoặc bằng giờ vào ({p.ThoiDiemVao.Value:HH:mm:ss}) trong cùng ngày.",
                        p.ThoiDiemVao.Value,
                        p.ThoiDiemRa.Value,
                        "NGHIEP_VU_TREO",
                        1
                    ));
                }
            }

            // 3.1. Có lịch làm việc nhưng hoàn toàn không có quẹt thẻ và không có phép nào
            bool hasAnyPunchTime = punches.Any(p => p.ThoiDiemVao.HasValue || p.ThoiDiemRa.HasValue);
            if (isWorkingDay && !hasAnyPunchTime && (input.ApprovedLeaves == null || input.ApprovedLeaves.Count == 0))
            {
                result.Anomalies.Add(CreateAnomaly(
                    input,
                    "VANG_CHUA_XAC_NHAN",
                    "Nhân viên có lịch làm việc nhưng không có dữ liệu quẹt thẻ và không có đơn nghỉ phép.",
                    schedule.BatDauKeHoach,
                    schedule.KetThucKeHoach,
                    "NGHIEP_VU_TREO",
                    1
                ));
            }

            // 3.2 & 3.3. Thiếu lượt ra (chỉ có vào) hoặc thiếu lượt vào (chỉ có ra)
            foreach (var p in punches)
            {
                if (p.ThoiDiemVao.HasValue && !p.ThoiDiemRa.HasValue)
                {
                    result.Anomalies.Add(CreateAnomaly(
                        input,
                        "THIEU_GIO_RA",
                        "Nhân viên có giờ vào lúc " + p.ThoiDiemVao.Value.ToString("HH:mm:ss") + " nhưng thiếu giờ ra.",
                        p.ThoiDiemVao.Value,
                        null,
                        "NGHIEP_VU_TREO",
                        1
                    ));
                }
                else if (!p.ThoiDiemVao.HasValue && p.ThoiDiemRa.HasValue)
                {
                    result.Anomalies.Add(CreateAnomaly(
                        input,
                        "THIEU_GIO_VAO",
                        "Nhân viên có giờ ra lúc " + p.ThoiDiemRa.Value.ToString("HH:mm:ss") + " nhưng thiếu giờ vào.",
                        null,
                        p.ThoiDiemRa.Value,
                        "NGHIEP_VU_TREO",
                        1
                    ));
                }
            }

            // 3.4. Quẹt thẻ ngoài lịch
            if (!isWorkingDay && hasAnyPunchTime)
            {
                var firstPunch = punches.FirstOrDefault(p => p.ThoiDiemVao.HasValue || p.ThoiDiemRa.HasValue);
                result.Anomalies.Add(CreateAnomaly(
                    input,
                    "CHAM_NGOAI_LICH",
                    "Nhân viên quẹt thẻ nhưng không có lịch làm việc được phân công vào ngày này.",
                    firstPunch?.ThoiDiemVao,
                    firstPunch?.ThoiDiemRa,
                    "NGHIEP_VU_TREO",
                    1
                ));
            }

            // 4. Sinh phân đoạn thời gian (nếu là ngày làm việc và có ca hoặc có phép)
            var validPunchPairs = punches.Where(p => p.ThoiDiemVao.HasValue && p.ThoiDiemRa.HasValue && p.ThoiDiemRa.Value > p.ThoiDiemVao.Value).ToList();

            if (isWorkingDay && shift != null && (validPunchPairs.Count > 0 || (input.ApprovedLeaves != null && input.ApprovedLeaves.Count > 0)))
            {
                var segments = GenerateTimeSegments(input, validPunchPairs);
                result.Segments.AddRange(segments);

                // Tổng hợp các chỉ số từ danh sách phân đoạn
                foreach (var seg in segments)
                {
                    result.GiayThucTe += seg.GiayThucTe;
                    result.GiayHuongCongThuong += seg.GiayHuongCongThuong;
                    result.GiayOtXacNhan += seg.GiayOtXacNhan;
                    result.GiayDemTrongGioThuong += seg.GiayDemTrongGioThuong;
                    result.GiayDemOt += seg.GiayDemOt;
                    result.GiayDiMuonThucTe += seg.GiayDiMuonThucTe;
                    result.GiayVeSomThucTe += seg.GiayVeSomThucTe;
                    result.GiayDiMuonViPham += seg.GiayDiMuonViPham;
                    result.GiayVeSomViPham += seg.GiayVeSomViPham;
                }

                // Kiểm tra xem có khoảng vắng trong ca không có phép và không có quẹt thẻ nào không
                var unexcusedAbsence = segments.Where(s => s.LoaiThoiGian == "VANG_TRONG_CA").ToList();
                if (unexcusedAbsence.Count > 0)
                {
                    DateTime minVang = unexcusedAbsence.Min(s => s.BatDauLuc);
                    DateTime maxVang = unexcusedAbsence.Max(s => s.KetThucLuc);
                    long totalVangSeconds = unexcusedAbsence.Sum(s => s.ThoiLuongGiay);

                    result.Anomalies.Add(CreateAnomaly(
                        input,
                        "VANG_CHUA_XAC_NHAN",
                        $"Vắng mặt trong ca làm việc {totalVangSeconds / 60} phút chưa có phép hoặc giải trình.",
                        minVang,
                        maxVang,
                        "NGHIEP_VU_TREO",
                        1
                    ));
                }

                // Kiểm tra xem có thời gian ở lại ngoài giờ sau ca chưa có đơn OT không (Tạo Anomaly cho từng khoảng)
                DateTime shiftEnd = schedule.KetThucKeHoach ?? input.Ngay.Date.AddHours(17);
                var unapprovedExtraSegments = segments
                    .Where(s => s.BatDauLuc >= shiftEnd && s.LoaiThoiGian == "CO_MAT_NGOAI_CA" && s.TrangThaiXacNhan == "CHO_XAC_MINH")
                    .OrderBy(s => s.BatDauLuc)
                    .ToList();

                // Gom nhóm các phân đoạn ngoài ca liên tiếp thành từng đợt để tạo Anomaly độc lập
                if (unapprovedExtraSegments.Count > 0)
                {
                    DateTime blockStart = unapprovedExtraSegments[0].BatDauLuc;
                    DateTime blockEnd = unapprovedExtraSegments[0].KetThucLuc;

                    for (int i = 1; i < unapprovedExtraSegments.Count; i++)
                    {
                        if (unapprovedExtraSegments[i].BatDauLuc == blockEnd)
                        {
                            blockEnd = unapprovedExtraSegments[i].KetThucLuc;
                        }
                        else
                        {
                            result.Anomalies.Add(CreateAnomaly(
                                input,
                                "OT_CHUA_DUYET",
                                $"Có mặt ngoài ca từ {blockStart:HH:mm} đến {blockEnd:HH:mm} chưa có đơn tăng ca được duyệt.",
                                blockStart,
                                blockEnd,
                                "CAN_RA_SOAT",
                                0
                            ));
                            blockStart = unapprovedExtraSegments[i].BatDauLuc;
                            blockEnd = unapprovedExtraSegments[i].KetThucLuc;
                        }
                    }

                    result.Anomalies.Add(CreateAnomaly(
                        input,
                        "OT_CHUA_DUYET",
                        $"Có mặt ngoài ca từ {blockStart:HH:mm} đến {blockEnd:HH:mm} chưa có đơn tăng ca được duyệt.",
                        blockStart,
                        blockEnd,
                        "CAN_RA_SOAT",
                        0
                    ));
                }
            }

            if (result.Segments.Any(x => x.IdYeuCauTangCa.HasValue && x.TrangThaiXacNhan == "CHO_XAC_MINH"))
                result.Anomalies.Add(CreateAnomaly(input, "OT_VUOT_GIAY_DUYET",
                    "Actual overtime exceeds the approved seconds; review the excess.",
                    null, null, "NGHIEP_VU_TREO", 1));

            // 5. Tính công chuẩn quy đổi (áp dụng đúng hệ số ca CongQuyDoi)
            long shiftStandardSeconds = shift?.TongGiayChuan ?? 28800;
            if (shiftStandardSeconds <= 0) shiftStandardSeconds = 28800;
            decimal caMultiplier = shift != null && shift.CongQuyDoi > 0 ? shift.CongQuyDoi : 1.0m;

            result.CongThuongQuyDoi = Math.Round(((decimal)result.GiayHuongCongThuong / shiftStandardSeconds) * caMultiplier, 4);

            // 6. Xác định điều kiện chốt và trạng thái công tuân thủ triệt để CK18_KQ_READY:
            // (DU_DIEUKIEN_CHOT = 0 OR TRANG_THAI = 'DA_XAC_NHAN')
            bool hasPendingAnomaly = result.Anomalies.Any(a => a.TrangThai == "CHO_XU_LY");
            if (hasPendingAnomaly)
            {
                // Có bất thường cần rà soát -> CHO_XAC_MINH và chưa đủ điều kiện chốt
                result.TrangThaiCong = "CHO_XAC_MINH";
                result.DuDieuKienChot = 0;
            }
            else
            {
                // Hoàn toàn sạch bất thường hoặc đã được phê duyệt xử lý -> Đã xác nhận và sẵn sàng chốt
                result.TrangThaiCong = "DA_XAC_NHAN";
                result.DuDieuKienChot = 1;
            }

            return result;
        }

        private List<TimeSegmentDto> GenerateTimeSegments(
            DayCalculationInput input,
            List<RawPunchDto> validPairs)
        {
            var segments = new List<TimeSegmentDto>();
            var policy = input.Policy ?? new AttendancePolicyDto();
            var schedule = input.Schedule;
            var shift = input.Shift;
            DateTime baseDate = input.Ngay.Date;

            // Xây dựng danh sách khung ca chuẩn
            var frames = new List<ShiftFrameResolved>();
            if (shift != null && shift.KhungGios != null && shift.KhungGios.Count > 0)
            {
                foreach (var f in shift.KhungGios.OrderBy(k => k.Stt))
                {
                    DateTime frameStart = baseDate.AddMinutes(f.BatDauPhut);
                    DateTime frameEnd = baseDate.AddMinutes(f.KetThucPhut);
                    frames.Add(new ShiftFrameResolved
                    {
                        Start = frameStart,
                        End = frameEnd,
                        LoaiKhungGio = f.LoaiKhungGio,
                        BatBuocQuetThe = f.BatBuocQuetThe
                    });
                }
            }
            else if (schedule?.BatDauKeHoach != null && schedule?.KetThucKeHoach != null)
            {
                frames.Add(new ShiftFrameResolved
                {
                    Start = schedule.BatDauKeHoach.Value,
                    End = schedule.KetThucKeHoach.Value,
                    LoaiKhungGio = "LAM_VIEC",
                    BatBuocQuetThe = 1
                });
            }

            // Tập hợp các mốc cắt thời gian (Cut Points)
            var cutPoints = new HashSet<DateTime>();

            // 1. Khung ca
            foreach (var f in frames)
            {
                cutPoints.Add(f.Start);
                cutPoints.Add(f.End);
            }

            // 2. Các lượt quẹt thẻ hợp lệ
            if (validPairs != null)
            {
                foreach (var vp in validPairs)
                {
                    cutPoints.Add(vp.ThoiDiemVao.Value);
                    cutPoints.Add(vp.ThoiDiemRa.Value);
                }
            }

            // 3. Tăng ca đã duyệt
            if (input.ApprovedOvertimes != null)
            {
                foreach (var ot in input.ApprovedOvertimes)
                {
                    cutPoints.Add(ot.BatDauDuyet);
                    cutPoints.Add(ot.KetThucDuyet);
                }
            }

            // 4. Nghỉ phép đã duyệt
            if (input.ApprovedLeaves != null)
            {
                foreach (var l in input.ApprovedLeaves)
                {
                    cutPoints.Add(l.BatDauNghi);
                    cutPoints.Add(l.KetThucNghi);
                }
            }

            // 5. Khung đêm (22:00, 06:00, 06:00 hôm sau)
            cutPoints.Add(baseDate.AddMinutes(policy.DemKetThucPhut)); // 06:00
            cutPoints.Add(baseDate.AddMinutes(policy.DemBatDauPhut));  // 22:00
            cutPoints.Add(baseDate.AddDays(1).AddMinutes(policy.DemKetThucPhut)); // 06:00 hôm sau

            // 6. Ranh giới nửa đêm để bảo đảm CK18_PD_TIME: KETTHUC_LUC <= TRUNC(BATDAU_LUC) + 1
            cutPoints.Add(baseDate.AddDays(1));

            // Xác định dải thời gian xem xét [minTime, maxTime]
            DateTime minTime = frames.Count > 0 ? frames.Min(f => f.Start) : baseDate.AddHours(8);
            if (validPairs != null && validPairs.Count > 0)
            {
                DateTime minPunch = validPairs.Min(p => p.ThoiDiemVao.Value);
                if (minPunch < minTime) minTime = minPunch;
            }

            DateTime maxTime = frames.Count > 0 ? frames.Max(f => f.End) : baseDate.AddHours(17);
            if (validPairs != null && validPairs.Count > 0)
            {
                DateTime maxPunch = validPairs.Max(p => p.ThoiDiemRa.Value);
                if (maxPunch > maxTime) maxTime = maxPunch;
            }


            var sortedCuts = cutPoints.Where(c => c >= minTime && c <= maxTime).OrderBy(c => c).Distinct().ToList();

            // Duyệt từng lát cắt [t_i, t_{i+1})
            for (int i = 0; i < sortedCuts.Count - 1; i++)
            {
                DateTime tStart = sortedCuts[i];
                DateTime tEnd = sortedCuts[i + 1];

                long durationSeconds = (long)Math.Round((tEnd - tStart).TotalSeconds);
                if (durationSeconds <= 0) continue;

                int laBanDem = IsNightTime(tStart, tEnd, policy) ? 1 : 0;

                var matchingFrame = frames.FirstOrDefault(f => tStart >= f.Start && tEnd <= f.End);
                var matchingOt = input.ApprovedOvertimes?.FirstOrDefault(ot => tStart >= ot.BatDauDuyet && tEnd <= ot.KetThucDuyet);
                var matchingLeave = input.ApprovedLeaves?.FirstOrDefault(l => tStart >= l.BatDauNghi && tEnd <= l.KetThucNghi);

                var segment = new TimeSegmentDto
                {
                    IdLanTinh = input.IdLanTinh,
                    IdBangCongCt = input.IdBangCongCt,
                    MaNV = input.MaNV,
                    IdLich = schedule?.IdLich,
                    IdQuyDinh = policy.IdQuyDinh,
                    BatDauLuc = tStart,
                    KetThucLuc = tEnd,
                    ThoiLuongGiay = durationSeconds,
                    LoaiNgay = schedule?.LoaiNgay ?? "THUONG",
                    LaBanDem = laBanDem,
                    CoOtBanNgayTruoc = 0, // Sẽ được cập nhật sau khi duyệt toàn bộ
                    TrangThaiXacNhan = "DA_XAC_NHAN"
                };

                // Kiểm tra sự hiện diện thực tế từ các lượt chấm hợp lệ
                bool isCoveredByPunch = false;
                if (validPairs != null && validPairs.Count > 0)
                {
                    var matchedPairs = validPairs.Where(vp => tStart >= vp.ThoiDiemVao.Value && tEnd <= vp.ThoiDiemRa.Value).ToList();
                    if (matchedPairs.Count > 0)
                    {
                        isCoveredByPunch = true;
                        foreach (var mp in matchedPairs)
                        {
                            if (mp.Mabc > 0 && !segment.SourceMabcList.Contains(mp.Mabc))
                            {
                                segment.SourceMabcList.Add(mp.Mabc);
                            }
                        }
                    }
                }

                if (matchingFrame != null)
                {
                    // A. NẰM TRONG KHUNG GIỜ CỦA CA
                    if (matchingFrame.LoaiKhungGio == "LAM_VIEC")
                    {
                        // Ưu tiên 1: Có đơn nghỉ phép đã duyệt bao phủ khoảng này
                        if (matchingLeave != null)
                        {
                            segment.LoaiThoiGian = matchingLeave.LoaiHuongCong == "NGHI_BHXH" ? "NGHI_BHXH" : (IsCompanyPaidLeave(matchingLeave) ? "PHEP_HUONG_LUONG" : "PHEP_KHONG_LUONG");
                            segment.IdYeuCauNghiPhep = matchingLeave.Id;
                            segment.GiayHuongCongThuong = IsCompanyPaidLeave(matchingLeave) ? durationSeconds : 0;

                            // Nếu vừa có phép vừa đi làm
                            if (isCoveredByPunch)
                            {
                                segment.GiayThucTe = durationSeconds;
                            }
                        }
                        // Ưu tiên 2: Làm việc thực tế trong ca
                        else if (isCoveredByPunch)
                        {
                            segment.LoaiThoiGian = "LAM_VIEC";
                            segment.GiayThucTe = durationSeconds;
                            segment.GiayHuongCongThuong = durationSeconds;
                            if (laBanDem == 1)
                            {
                                segment.GiayDemTrongGioThuong = durationSeconds;
                            }
                        }
                        // Ưu tiên 3: Đi muộn (trước lượt quẹt vào đầu tiên)
                        else if (validPairs != null && validPairs.Count > 0 && tEnd <= validPairs.Min(p => p.ThoiDiemVao.Value))
                        {
                            DateTime firstIn = validPairs.Min(p => p.ThoiDiemVao.Value);
                            segment.LoaiThoiGian = "DI_MUON";
                            segment.GiayDiMuonThucTe = durationSeconds;

                            long totalLateToPunchIn = (long)Math.Round((firstIn - matchingFrame.Start).TotalSeconds);
                            if (totalLateToPunchIn <= policy.GiayDungSaiHuongCong && policy.CachHuongDungSai == "DU_CONG_NEU_TRONG_NGUONG")
                            {
                                segment.GiayHuongCongThuong = durationSeconds;
                            }

                            if (totalLateToPunchIn > policy.GiayMienViPhamMuon)
                            {
                                segment.GiayDiMuonViPham = policy.CachDemMuon == "PHAN_VUOT_NGUONG"
                                    ? Math.Max(0, (long)(tEnd - (tStart > matchingFrame.Start.AddSeconds(policy.GiayMienViPhamMuon)
                                        ? tStart : matchingFrame.Start.AddSeconds(policy.GiayMienViPhamMuon))).TotalSeconds)
                                    : durationSeconds;
                            }
                        }
                        // Ưu tiên 4: Về sớm (sau lượt quẹt ra cuối cùng)
                        else if (validPairs != null && validPairs.Count > 0 && tStart >= validPairs.Max(p => p.ThoiDiemRa.Value))
                        {
                            DateTime lastOut = validPairs.Max(p => p.ThoiDiemRa.Value);
                            segment.LoaiThoiGian = "VE_SOM";
                            segment.GiayVeSomThucTe = durationSeconds;

                            long totalEarlyFromPunchOut = (long)Math.Round((matchingFrame.End - lastOut).TotalSeconds);
                            if (totalEarlyFromPunchOut > policy.GiayMienViPhamSom)
                            {
                                segment.GiayVeSomViPham = policy.CachDemMuon == "PHAN_VUOT_NGUONG"
                                    ? Math.Max(0, (long)((tEnd < matchingFrame.End.AddSeconds(-policy.GiayMienViPhamSom)
                                        ? tEnd : matchingFrame.End.AddSeconds(-policy.GiayMienViPhamSom)) - tStart).TotalSeconds)
                                    : durationSeconds;
                            }
                        }
                        // Ưu tiên 5: Không có quẹt thẻ và không có phép -> Vắng trong ca
                        else
                        {
                            segment.LoaiThoiGian = "VANG_TRONG_CA";
                        }
                    }
                    else if (matchingFrame.LoaiKhungGio == "NGHI_HUONG_LUONG")
                    {
                        segment.LoaiThoiGian = "NGHI_HUONG_LUONG";
                        segment.GiayHuongCongThuong = durationSeconds;
                    }
                    else // NGHI_KHONG_LUONG (ví dụ nghỉ trưa)
                    {
                        segment.LoaiThoiGian = "NGHI_KHONG_LUONG";
                    }
                }
                else
                {
                    // B. NẰM NGOÀI KHUNG GIỜ CỦA CA
                    if (matchingLeave != null)
                    {
                        segment.LoaiThoiGian = matchingLeave.LoaiHuongCong == "NGHI_BHXH" ? "NGHI_BHXH" : (IsCompanyPaidLeave(matchingLeave) ? "PHEP_HUONG_LUONG" : "PHEP_KHONG_LUONG");
                        segment.IdYeuCauNghiPhep = matchingLeave.Id;
                        segment.GiayHuongCongThuong = 0;
                    }
                    else if (isCoveredByPunch)
                    {
                        // Có mặt ngoài ca
                        if (matchingOt != null)
                        {
                            // Tăng ca đã duyệt -> Phân loại vật lý là 'LAM_VIEC' theo đúng CK18_PD_TYPE!
                            segment.LoaiThoiGian = "LAM_VIEC";
                            segment.GiayThucTe = durationSeconds;
                            segment.GiayOtXacNhan = durationSeconds;
                            segment.IdYeuCauTangCa = matchingOt.Id;

                            if (laBanDem == 1)
                            {
                                segment.GiayDemOt = durationSeconds;
                            }
                        }
                        else
                        {
                            // Có mặt ngoài ca không đơn OT -> 'CO_MAT_NGOAI_CA', Trạng thái chờ xác minh
                            segment.LoaiThoiGian = "CO_MAT_NGOAI_CA";
                            segment.GiayThucTe = durationSeconds;
                            segment.TrangThaiXacNhan = "CHO_XAC_MINH";
                        }
                    }
                    else
                    {
                        segment.LoaiThoiGian = "CO_MAT_NGOAI_CA";
                        segment.TrangThaiXacNhan = "KHONG_TINH";
                    }
                }

                segments.Add(segment);
            }

            foreach (var ot in input.ApprovedOvertimes ?? new List<ApprovedOvertimeDto>())
            {
                long remaining = ot.GiayOtDuyet;
                foreach (var segment in segments.Where(x => x.IdYeuCauTangCa == ot.Id).OrderBy(x => x.BatDauLuc).ToList())
                {
                    long allowed = Math.Max(0, Math.Min(remaining, segment.GiayOtXacNhan));
                    if (allowed > 0 && allowed < segment.GiayOtXacNhan)
                    {
                        var excess = JsonConvert.DeserializeObject<TimeSegmentDto>(JsonConvert.SerializeObject(segment));
                        excess.BatDauLuc = segment.BatDauLuc.AddSeconds(allowed);
                        excess.ThoiLuongGiay -= allowed;
                        excess.GiayThucTe = excess.ThoiLuongGiay;
                        excess.GiayOtXacNhan = 0;
                        excess.GiayDemOt = 0;
                        excess.TrangThaiXacNhan = "CHO_XAC_MINH";
                        segments.Add(excess);
                        segment.KetThucLuc = excess.BatDauLuc;
                        segment.ThoiLuongGiay = allowed;
                        segment.GiayThucTe = allowed;
                    }
                    else if (allowed == 0 && segment.GiayOtXacNhan > 0)
                        segment.TrangThaiXacNhan = "CHO_XAC_MINH";
                    segment.GiayOtXacNhan = allowed;
                    segment.GiayDemOt = segment.LaBanDem == 1 ? allowed : 0;
                    remaining -= allowed;
                }
            }

            // Cập nhật cờ CoOtBanNgayTruoc cho các phân đoạn OT đêm:
            // Cờ chỉ được bật nếu TRÊN THỰC TẾ đã có một phân đoạn OT ban ngày (LaBanDem == 0 và GiayOtXacNhan > 0)
            // diễn ra trước phân đoạn OT đêm đang xét!
            bool hasActualDaytimeOtPrior = false;
            foreach (var seg in segments.OrderBy(s => s.BatDauLuc))
            {
                if (seg.GiayOtXacNhan > 0 && seg.LaBanDem == 0)
                {
                    hasActualDaytimeOtPrior = true;
                }
                else if (seg.GiayDemOt > 0)
                {
                    seg.CoOtBanNgayTruoc = hasActualDaytimeOtPrior ? 1 : 0;
                }
            }

            return segments;
        }

        private static bool IsCompanyPaidLeave(ApprovedLeaveDto leave)
        {
            return leave.CoHuongLuong == 1 && leave.NguonChiTra == "CONG_TY"
                && (leave.LoaiHuongCong == "PHEP_NAM" || leave.LoaiHuongCong == "NGHI_HUONG_LUONG");
        }

        private bool IsNightTime(DateTime start, DateTime end, AttendancePolicyDto policy)
        {
            DateTime mid = start.AddSeconds((end - start).TotalSeconds / 2.0);
            int midMinutes = mid.Hour * 60 + mid.Minute;

            int demStart = policy.DemBatDauPhut;
            int demEnd = policy.DemKetThucPhut;

            if (demStart > demEnd)
            {
                return midMinutes >= demStart || midMinutes < demEnd;
            }
            else
            {
                return midMinutes >= demStart && midMinutes < demEnd;
            }
        }

        private AttendanceAnomalyDto CreateAnomaly(
            DayCalculationInput input,
            string maLoi,
            string moTa,
            DateTime? batDau,
            DateTime? ketThuc,
            string mucDo,
            int chanChot)
        {
            if (!batDau.HasValue || !ketThuc.HasValue || ketThuc <= batDau)
            {
                batDau = null;
                ketThuc = null;
            }
            string hashInput = $"{input.MaNV}|{input.IdBangCongCt}|{maLoi}|{batDau:yyyyMMddHHmmss}|{ketThuc:yyyyMMddHHmmss}";
            string suViecHash = ComputeSha256(hashInput);

            var evidenceSnapshot = new
            {
                MaNV = input.MaNV,
                IdBangCongCt = input.IdBangCongCt,
                MaLoi = maLoi,
                BatDau = batDau,
                KetThuc = ketThuc,
                MucDo = mucDo,
                ChanChot = chanChot,
                ScheduleId = input.Schedule?.IdLich,
                Punches = (input.RawPunches != null && input.RawPunches.Count > 0)
                    ? input.RawPunches.Select(p => new { p.Mabc, p.ThoiDiemVao, p.ThoiDiemRa, p.NguonCham }).ToArray()
                    : (input.RawPunch != null ? new[] { new { input.RawPunch.Mabc, input.RawPunch.ThoiDiemVao, input.RawPunch.ThoiDiemRa, input.RawPunch.NguonCham } } : null)
            };
            string snapshotJson = JsonConvert.SerializeObject(evidenceSnapshot);
            string inputHash = ComputeSha256(snapshotJson);

            return new AttendanceAnomalyDto
            {
                IdLanTinhPhatHien = input.IdLanTinh,
                IdBangCongCt = input.IdBangCongCt,
                MaNV = input.MaNV,
                IdLich = input.Schedule?.IdLich,
                Ngay = input.Ngay.Date,
                MaLoi = maLoi,
                BatDauLuc = batDau,
                KetThucLuc = ketThuc,
                MucDo = mucDo,
                ChanChot = chanChot,
                MoTa = moTa,
                TrangThai = "CHO_XU_LY",
                MaSuViecHash = suViecHash,
                InputHash = inputHash,
                InputSnapshotJson = snapshotJson
            };
        }

        private static string ComputeSha256(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }

    internal class ShiftFrameResolved
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string LoaiKhungGio { get; set; }
        public int BatBuocQuetThe { get; set; }
    }
}
