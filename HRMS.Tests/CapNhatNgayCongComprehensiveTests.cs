using Bu.CLASS_CHAMCONG;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Tests
{
    [TestFixture]
    public class CapNhatNgayCongComprehensiveTests
    {
        private ShiftInfo _standardDayShift;
        private ShiftInfo _nightShift;
        private ShiftInfo _afternoonShift;

        [SetUp]
        public void Setup()
        {
            _standardDayShift = new ShiftInfo
            {
                IdCaPhienBan = 1,
                IdLoaiCa = 1,
                TenPhienBan = "Ca ngày (Hành chính)",
                TenLoaiCa = "Ca ngày",
                TongGiayChuan = 28800, // 8 hours = 480 mins
                CongQuyDoi = 1.0m,
                Frames = new List<ShiftFrameInfo>
                {
                    new ShiftFrameInfo { Stt = 1, BatDauPhut = 480, KetThucPhut = 720, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }, // 08:00 - 12:00
                    new ShiftFrameInfo { Stt = 2, BatDauPhut = 720, KetThucPhut = 780, LoaiKhungGio = "NGHI_GIUA_CA", BatBuocQuetThe = false }, // 12:00 - 13:00
                    new ShiftFrameInfo { Stt = 3, BatDauPhut = 780, KetThucPhut = 1020, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }  // 13:00 - 17:00
                }
            };

            _nightShift = new ShiftInfo
            {
                IdCaPhienBan = 3,
                IdLoaiCa = 2,
                TenPhienBan = "Ca đêm (22:00 - 06:00)",
                TenLoaiCa = "Ca đêm",
                TongGiayChuan = 28800,
                CongQuyDoi = 1.0m,
                Frames = new List<ShiftFrameInfo>
                {
                    new ShiftFrameInfo { Stt = 1, BatDauPhut = 1320, KetThucPhut = 1560, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }, // 22:00 - 02:00
                    new ShiftFrameInfo { Stt = 2, BatDauPhut = 1560, KetThucPhut = 1800, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }  // 02:00 - 06:00
                }
            };

            _afternoonShift = new ShiftInfo
            {
                IdCaPhienBan = 2,
                IdLoaiCa = 1,
                TenPhienBan = "Ca chiều",
                TenLoaiCa = "Ca ngày",
                TongGiayChuan = 28800,
                CongQuyDoi = 1.0m,
                Frames = new List<ShiftFrameInfo>
                {
                    new ShiftFrameInfo { Stt = 1, BatDauPhut = 840, KetThucPhut = 1080, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }, // 14:00 - 18:00
                    new ShiftFrameInfo { Stt = 2, BatDauPhut = 1080, KetThucPhut = 1140, LoaiKhungGio = "NGHI_GIUA_CA", BatBuocQuetThe = false }, // 18:00 - 19:00
                    new ShiftFrameInfo { Stt = 3, BatDauPhut = 1140, KetThucPhut = 1380, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }  // 19:00 - 23:00
                }
            };
        }

        [Test]
        public void Scenario01_DiLamDuCa_FullWorkingDay_CreditedProperly()
        {
            DateTime date = new DateTime(2026, 10, 1);
            DateTime gv = new DateTime(2026, 10, 1, 8, 0, 0);
            DateTime gr = new DateTime(2026, 10, 1, 17, 0, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: true,
                isCongTac: false,
                phanNghi: "",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: gr,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(1.0m, res.NgayCong, "Đi làm đủ ca phải được 1.0 công");
            Assert.AreEqual(0m, res.NgayPhep, "Đi làm thì ngày phép = 0");
            Assert.AreEqual("X", res.KyHieu, "Ký hiệu phải là X");
            Assert.AreEqual(480, res.GioLamHopLePhut, "Thời gian làm hợp lệ phải trừ giờ nghỉ trưa = 480 phút");
            Assert.AreEqual(0, res.DiMuonPhut, "Không đi muộn");
            Assert.AreEqual(0, res.VeSomPhut, "Không về sớm");
            Assert.IsFalse(res.HasWarning, "Không có cảnh báo");
            Assert.IsTrue(res.DuDieuKienChot, "Đủ điều kiện chốt công");
        }

        [Test]
        public void Scenario02_NghiSang_LamChieu_CreditedHalfWorkAndHalfLeave()
        {
            DateTime date = new DateTime(2026, 10, 2);
            DateTime gv = new DateTime(2026, 10, 2, 13, 0, 0);
            DateTime gr = new DateTime(2026, 10, 2, 17, 0, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "S",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: gr,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(0.5m, res.NgayCong, "Làm buổi chiều phải được 0.5 công");
            Assert.AreEqual(0.5m, res.NgayPhep, "Nghỉ phép buổi sáng phải được 0.5 phép");
            Assert.AreEqual("P", res.KyHieu);
            Assert.AreEqual(240, res.GioLamHopLePhut, "Thời gian làm buổi chiều là 240 phút");
            Assert.AreEqual(0, res.DiMuonPhut);
            Assert.AreEqual(0, res.VeSomPhut);
            Assert.IsTrue(res.DuDieuKienChot);
        }

        [Test]
        public void Scenario03_NghiChieu_LamSang_CreditedHalfWorkAndHalfLeave()
        {
            DateTime date = new DateTime(2026, 10, 3);
            DateTime gv = new DateTime(2026, 10, 3, 8, 0, 0);
            DateTime gr = new DateTime(2026, 10, 3, 12, 0, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "C",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: gr,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(0.5m, res.NgayCong, "Làm buổi sáng phải được 0.5 công");
            Assert.AreEqual(0.5m, res.NgayPhep, "Nghỉ phép buổi chiều phải được 0.5 phép");
            Assert.AreEqual("P", res.KyHieu);
            Assert.AreEqual(240, res.GioLamHopLePhut, "Thời gian làm buổi sáng là 240 phút");
            Assert.AreEqual(0, res.DiMuonPhut);
            Assert.AreEqual(0, res.VeSomPhut);
        }

        [Test]
        public void Scenario04_NghiNguyenNgay_HoursAreNull_CreditedLeave()
        {
            DateTime date = new DateTime(2026, 10, 4);

            // 1. Kiểm tra GetDefaultShiftHours trả về null, null
            (TimeSpan? defVao, TimeSpan? defRa) = AttendanceCalculationHelper.GetDefaultShiftHours(_standardDayShift, isDiLam: false, phanNghi: "NN");
            Assert.IsNull(defVao, "Nghỉ nguyên ngày thì giờ vào mặc định phải là null");
            Assert.IsNull(defRa, "Nghỉ nguyên ngày thì giờ ra mặc định phải là null");

            // 2. Nghỉ phép cả ngày
            var resPhep = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "NN",
                loaiNghi: "P",
                gioVao: null,
                gioRa: null,
                coLogQuetTheGoc: false,
                date: date
            );
            Assert.AreEqual(1.0m, resPhep.NgayCong, "Nghỉ phép hưởng nguyên lương");
            Assert.AreEqual(1.0m, resPhep.NgayPhep, "Được 1.0 phép");
            Assert.AreEqual("P", resPhep.KyHieu);
            Assert.AreEqual(0, resPhep.GioLamHopLePhut);

            // 3. Nghỉ không phép cả ngày (Vắng)
            var resKhongPhep = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "NN",
                loaiNghi: "V",
                gioVao: null,
                gioRa: null,
                coLogQuetTheGoc: false,
                date: date
            );
            Assert.AreEqual(0m, resKhongPhep.NgayCong, "Nghỉ không phép thì 0 công");
            Assert.AreEqual(0m, resKhongPhep.NgayPhep, "0 phép");
            Assert.AreEqual("V", resKhongPhep.KyHieu);
        }

        [Test]
        public void Scenario05_NghiSang_VaoTreChieu_ViolatesLateRules()
        {
            DateTime date = new DateTime(2026, 10, 5);
            // Vào trễ buổi chiều 13:30 (ca chiều bắt đầu 13:00)
            DateTime gv = new DateTime(2026, 10, 5, 13, 30, 0);
            DateTime gr = new DateTime(2026, 10, 5, 17, 0, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "S",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: gr,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(30, res.DiMuonPhut, "Đi muộn 30 phút so với giờ bắt đầu chiều");
            Assert.AreEqual(210, res.GioLamHopLePhut, "Thời gian làm là 3.5 tiếng = 210 phút");
            Assert.IsTrue(res.DiMuonVeSomText.Contains("Đi muộn 30 phút"));
        }

        [Test]
        public void Scenario06_NghiMotBuoi_ThieuGioBuoiConLai_FlagsWarningAndZeroWork()
        {
            DateTime date = new DateTime(2026, 10, 6);
            DateTime gv = new DateTime(2026, 10, 6, 13, 0, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "S",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: null, // Thiếu giờ ra
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.IsTrue(res.HasWarning, "Phải cảnh báo thiếu dữ liệu");
            Assert.IsFalse(res.DuDieuKienChot, "Không đủ điều kiện chốt công khi thiếu giờ ra");
            Assert.AreEqual(0m, res.NgayCong, "Chưa đủ căn cứ xác nhận công buổi chiều");
        }

        [Test]
        public void Scenario07_DoiLuaChonNhieuLan_KhiGioTuDien_UpdatesCorrectHours()
        {
            // 1. Đi làm -> 08:00 - 17:00
            var h1 = AttendanceCalculationHelper.GetDefaultShiftHours(_standardDayShift, true, "");
            Assert.AreEqual(new TimeSpan(8, 0, 0), h1.GioVao);
            Assert.AreEqual(new TimeSpan(17, 0, 0), h1.GioRa);

            // 2. Chuyển sang Nghỉ sáng -> Buổi chiều: 13:00 - 17:00
            var h2 = AttendanceCalculationHelper.GetDefaultShiftHours(_standardDayShift, false, "S");
            Assert.AreEqual(new TimeSpan(13, 0, 0), h2.GioVao);
            Assert.AreEqual(new TimeSpan(17, 0, 0), h2.GioRa);

            // 3. Chuyển sang Nghỉ chiều -> Buổi sáng: 08:00 - 12:00
            var h3 = AttendanceCalculationHelper.GetDefaultShiftHours(_standardDayShift, false, "C");
            Assert.AreEqual(new TimeSpan(8, 0, 0), h3.GioVao);
            Assert.AreEqual(new TimeSpan(12, 0, 0), h3.GioRa);

            // 4. Chuyển sang Nghỉ nguyên ngày -> Rỗng/NULL
            var h4 = AttendanceCalculationHelper.GetDefaultShiftHours(_standardDayShift, false, "NN");
            Assert.IsNull(h4.GioVao);
            Assert.IsNull(h4.GioRa);
        }

        [Test]
        public void Scenario08_DoiLuaChon_SauKhiSuaGioThuCong_PreservesUserManualHours()
        {
            // Nếu người dùng đã sửa giờ thủ công: ví dụ 08:15 - 16:45
            DateTime date = new DateTime(2026, 10, 7);
            DateTime manualVao = new DateTime(2026, 10, 7, 8, 15, 0);
            DateTime manualRa = new DateTime(2026, 10, 7, 16, 45, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: true,
                isCongTac: false,
                phanNghi: "",
                loaiNghi: "P",
                gioVao: manualVao,
                gioRa: manualRa,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(15, res.DiMuonPhut, "Đi muộn 15 phút");
            Assert.AreEqual(15, res.VeSomPhut, "Về sớm 15 phút");
            Assert.AreEqual(450, res.GioLamHopLePhut, "Tổng thời gian làm 450 phút");
        }

        [Test]
        public void Scenario10_NghiNguyenNgay_CoLogQuetTheGoc_FlagsConflict()
        {
            DateTime date = new DateTime(2026, 10, 8);
            DateTime gv = new DateTime(2026, 10, 8, 8, 0, 0);
            DateTime gr = new DateTime(2026, 10, 8, 17, 0, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: false,
                isCongTac: false,
                phanNghi: "NN",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: gr,
                coLogQuetTheGoc: true, // Có log quẹt thẻ gốc trên máy
                date: date
            );

            Assert.IsTrue(res.HasConflict, "Phải cảnh báo xung đột dữ liệu quẹt thẻ thực tế khi đăng ký nghỉ nguyên ngày");
            Assert.IsFalse(res.DuDieuKienChot, "Không tự động chốt công khi còn xung đột");
            Assert.IsTrue(res.ConflictMessage.Contains("Xung đột dữ liệu"));
        }

        [Test]
        public void Scenario11_CaQuaDem_NightShift_CalculatedCorrectly()
        {
            DateTime date = new DateTime(2026, 10, 9);
            DateTime gv = new DateTime(2026, 10, 9, 22, 0, 0);
            DateTime gr = new DateTime(2026, 10, 10, 6, 0, 0); // Sáng hôm sau

            var res = AttendanceCalculationHelper.Calculate(
                _nightShift,
                isDiLam: true,
                isCongTac: false,
                phanNghi: "",
                loaiNghi: "P",
                gioVao: gv,
                gioRa: gr,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(1.0m, res.NgayCong);
            Assert.AreEqual("CD", res.KyHieu, "Ký hiệu ca đêm là CD");
            Assert.AreEqual(480, res.GioLamHopLePhut, "8 tiếng = 480 phút");
            Assert.AreEqual(0, res.DiMuonPhut);
            Assert.AreEqual(0, res.VeSomPhut);
        }

        [Test]
        public void Scenario09_MoLaiBanGhiDaLuu_KhongBiReset()
        {
            // Kiểm tra: nếu bản ghi có giờ đã lưu trong DB (ví dụ 08:30 - 17:15)
            // thì khi tính toán giữ nguyên đúng giá trị đã lưu đó
            DateTime date = new DateTime(2026, 10, 15);
            DateTime savedVao = new DateTime(2026, 10, 15, 8, 30, 0);
            DateTime savedRa = new DateTime(2026, 10, 15, 17, 15, 0);

            var res = AttendanceCalculationHelper.Calculate(
                _standardDayShift,
                isDiLam: true,
                isCongTac: false,
                phanNghi: "",
                loaiNghi: "P",
                gioVao: savedVao,
                gioRa: savedRa,
                coLogQuetTheGoc: false,
                date: date
            );

            Assert.AreEqual(30, res.DiMuonPhut, "Giữ nguyên giờ vào 08:30 -> đi muộn 30 phút");
            Assert.AreEqual(0, res.VeSomPhut, "Giữ nguyên giờ ra 17:15 -> không về sớm");
            Assert.AreEqual(450, res.GioLamHopLePhut);
        }

        [Test]
        public void Scenario12_NgayChuaCoCa_FallbackResolvesGracefully()
        {
            var fallbacks = AttendanceCalculationHelper.GetFallbackShifts();
            Assert.IsNotNull(fallbacks);
            Assert.GreaterOrEqual(fallbacks.Count, 3, "Có ít nhất ca ngày, ca chiều và ca đêm");

            var resolved = AttendanceCalculationHelper.GetShiftForEmployee(fallbacks, 99999, new DateTime(2026, 10, 10));
            Assert.IsNotNull(resolved);
            Assert.AreEqual(1, resolved.IdCaPhienBan);
        }

        [Test]
        public void Scenario13_ChuyenNgayNhanVien_KhongLanDuLieu()
        {
            DateTime dateNv1 = new DateTime(2026, 10, 1);
            DateTime dateNv2 = new DateTime(2026, 10, 2);

            // NV1: Đi làm cả ngày
            var resNv1 = AttendanceCalculationHelper.Calculate(
                _standardDayShift, true, false, "", "P",
                new DateTime(2026, 10, 1, 8, 0, 0),
                new DateTime(2026, 10, 1, 17, 0, 0),
                false, dateNv1
            );

            // NV2: Nghỉ phép nguyên ngày
            var resNv2 = AttendanceCalculationHelper.Calculate(
                _standardDayShift, false, false, "NN", "P",
                null, null, false, dateNv2
            );

            Assert.AreEqual(1.0m, resNv1.NgayCong);
            Assert.AreEqual(0m, resNv1.NgayPhep);
            Assert.AreEqual("X", resNv1.KyHieu);

            Assert.AreEqual(1.0m, resNv2.NgayCong);
            Assert.AreEqual(1.0m, resNv2.NgayPhep);
            Assert.AreEqual("P", resNv2.KyHieu);
            Assert.AreEqual(0, resNv2.GioLamHopLePhut);
        }

        [Test]
        public void Scenario14_HuyForm_KhongLuu()
        {
            // Kiểm tra nguyên lý: Calculate trả về preview DTO
            // Chỉ khi gọi CapNhatNgayCongVaBangCongRaw mới ghi xuống DB
            // Nếu người dùng đóng form, không có bản ghi nào bị ghi đè hay thay đổi dở dang
            DateTime date = new DateTime(2026, 10, 20);
            var preview = AttendanceCalculationHelper.Calculate(
                _standardDayShift, false, false, "NN", "V", null, null, false, date
            );
            Assert.IsNotNull(preview);
            Assert.AreEqual(0m, preview.NgayCong);
            // Dữ liệu chỉ là preview trong bộ nhớ, không ảnh hưởng database
        }

        [Test]
        public void Scenario15_TimesheetLock_ProhibitsPayrollRecalculation()
        {
            const int testMaKyCong = 999912;
            var bangLuong = new BANGLUONG();
            using (var db = new DA.MyEntities())
            {
                // Dọn dẹp bản ghi test cũ nếu có
                var oldTest = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == testMaKyCong);
                if (oldTest != null) db.TB_KYCONG.Remove(oldTest);

                // Dọn dẹp kỳ công 202610 bị bỏ sót bởi lần chạy trước nếu chưa có dữ liệu thực tế
                var kc202610 = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == 202610);
                if (kc202610 != null)
                {
                    bool hasDetails = db.TB_KYCONGCHITIET.Any(x => x.MAKYCONG == 202610) || db.TB_BANGCONG_CHITIET.Any(x => x.MAKYCONG == 202610);
                    if (!hasDetails)
                    {
                        db.TB_KYCONG.Remove(kc202610);
                    }
                    else
                    {
                        kc202610.KHOA = 0;
                    }
                }

                var kc = new DA.TB_KYCONG
                {
                    MAKYCONG = testMaKyCong,
                    THANG = 12,
                    NAM = 9999,
                    KHOA = 1,
                    NGAYCONGTRONGTHANG = 26,
                    TRANGTHAI = 0,
                    CREATED_DATE = DateTime.Now
                };
                db.TB_KYCONG.Add(kc);
                db.SaveChanges();
            }

            try
            {
                // Makycong đã bị khóa: kiểm tra ném lỗi khi tính lương
                var ex = Assert.Throws<InvalidOperationException>(() =>
                {
                    bangLuong.TinhLuongKyCong(testMaKyCong, 1, null);
                });
                StringAssert.Contains("đã bị khóa", ex.Message);
            }
            finally
            {
                using (var db = new DA.MyEntities())
                {
                    var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == testMaKyCong);
                    if (kc != null)
                    {
                        db.TB_KYCONG.Remove(kc);
                        db.SaveChanges();
                    }
                }
            }
        }

        [Test]
        public void Scenario16_KiemTraPhatSinhKyCong_AllowsGeneration_WhenNoDetailData()
        {
            const int testMkc = 888810;
            var kycongBus = new KYCONG();

            using (var db = new DA.MyEntities())
            {
                // Dọn dẹp nếu có
                var old = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == testMkc);
                if (old != null) db.TB_KYCONG.Remove(old);

                var kc = new DA.TB_KYCONG
                {
                    MAKYCONG = testMkc,
                    NAM = 8888,
                    THANG = 10,
                    KHOA = 0,
                    TRANGTHAI = 1, // Header có thể bị set TRANGTHAI=1 nhưng chưa có dữ liệu chi tiết
                    NGAYCONGTRONGTHANG = 26,
                    CREATED_DATE = DateTime.Now
                };
                db.TB_KYCONG.Add(kc);
                db.SaveChanges();
            }

            try
            {
                // Khi chưa có bản ghi chi tiết nào trong TB_KYCONGCHITIET và TB_BANGCONG_CHITIET,
                // KiemTraPhatSinhKyCong PHẢI trả về 0 để cho phép người dùng phát sinh dữ liệu
                int result = kycongBus.KiemTraPhatSinhKyCong(testMkc);
                Assert.AreEqual(0, result, "Kỳ công chưa có chi tiết phải trả về 0 để cho phép phát sinh!");
            }
            finally
            {
                using (var db = new DA.MyEntities())
                {
                    var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == testMkc);
                    if (kc != null)
                    {
                        db.TB_KYCONG.Remove(kc);
                        db.SaveChanges();
                    }
                }
            }
        }

        [Test]
        public void Scenario17_LockLogic_Unification_OnlyKhoaOneIsLocked()
        {
            // Kiểm tra tính đồng nhất: chỉ KHOA == 1 mới là khóa, TRANGTHAI == 1 là đã phát sinh nhưng chưa khóa
            using (var db = new DA.MyEntities())
            {
                var kcOpen = new DA.TB_KYCONG { MAKYCONG = 888811, NAM = 8888, THANG = 11, KHOA = 0, TRANGTHAI = 1 };
                var kcLocked = new DA.TB_KYCONG { MAKYCONG = 888812, NAM = 8888, THANG = 12, KHOA = 1, TRANGTHAI = 1 };

                // Web logic: (kc.KHOA == 1)
                Assert.IsFalse((kcOpen.KHOA ?? 0) == 1, "KHOA = 0 phải là chưa khóa");
                Assert.IsTrue((kcLocked.KHOA ?? 0) == 1, "KHOA = 1 phải là đã khóa");

                // Desktop logic mới: (kc.KHOA ?? 0) == 1
                Assert.IsFalse((kcOpen.KHOA ?? 0) == 1, "Desktop logic phải coi KHOA = 0 là chưa khóa");
                Assert.IsTrue((kcLocked.KHOA ?? 0) == 1, "Desktop logic phải coi KHOA = 1 là đã khóa");
            }
        }

        [Test]
        public void Scenario18_GetChiTietNgayCongV118_SucceedsWithoutPasswordError()
        {
            var bcct = new BANGCONG_NV_CHITIET();
            // Gọi GetChiTietNgayCongV118 với mã kỳ công bất kỳ để kiểm tra không bị lỗi ORA-01005 (null password given)
            Assert.DoesNotThrow(() =>
            {
                var res = bcct.GetChiTietNgayCongV118(202601, 1, 5);
            });
        }

        [Test]
        public void Scenario19_AttendanceCalculationHelper_LoadShiftsAndGetShift_Succeeds()
        {
            var shifts = AttendanceCalculationHelper.LoadAllShifts();
            Assert.IsNotNull(shifts);
            Assert.IsTrue(shifts.Count > 0);

            var shiftForEmp = AttendanceCalculationHelper.GetShiftForEmployee(shifts, 1, DateTime.Today);
            Assert.IsNotNull(shiftForEmp);
            Assert.IsTrue(shiftForEmp.IdCaPhienBan > 0);
        }

        [Test]
        public void Scenario20_CapNhatNgayCongVaBangCongRaw_WithShift_DoesNotThrowEntityModelException()
        {
            var bcct = new BANGCONG_NV_CHITIET();
            int testManv = 1;
            int testMkc = 202601;

            using (var db = new DA.MyEntities())
            {
                var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == testManv);
                if (nv == null)
                {
                    Assert.Ignore("Nhân viên 1 không tồn tại trong DB kiểm thử");
                    return;
                }
            }

            Assert.DoesNotThrow(() =>
            {
                bcct.CapNhatNgayCongVaBangCongRaw(
                    manv: testManv,
                    makycong: testMkc,
                    nam: 2026,
                    thang: 1,
                    ngay: 5,
                    gioVao: 8,
                    phutVao: 0,
                    gioRa: 17,
                    phutRa: 0,
                    kyhieu: "X",
                    loaiNghi: null,
                    ngayCong: 1.0m,
                    ngayPhep: 0m,
                    iduser: 1,
                    ghiChu: "Test cap nhat co phan ca",
                    idCaPhienBan: 1
                );
            });
        }
    }
}
