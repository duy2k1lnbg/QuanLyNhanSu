using System;
using System.Collections.Generic;
using System.Linq;
using Bu.CLASS_CHAMCONG;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace HRMS.Tests
{
    [TestFixture]
    public class TimeSegmentationEngineTests
    {
        private TimeSegmentationEngine _engine;
        private ShiftVersionDto _standardShift;
        private AttendancePolicyDto _standardPolicy;

        private static readonly HashSet<string> AllowedDbSegmentTypes = new HashSet<string>
        {
            "LAM_VIEC", "NGHI_HUONG_LUONG", "NGHI_KHONG_LUONG",
            "PHEP_HUONG_LUONG", "PHEP_KHONG_LUONG", "NGHI_BHXH", "NGHI_LE",
            "DI_MUON", "VE_SOM", "VANG_TRONG_CA", "CO_MAT_NGOAI_CA", "CHO_XAC_MINH"
        };

        [SetUp]
        public void Setup()
        {
            _engine = new TimeSegmentationEngine();

            // Ca hành chính 8 tiếng: 08:00 - 12:00, nghỉ 12:00 - 13:00, 13:00 - 17:00
            _standardShift = new ShiftVersionDto
            {
                IdCaPhienBan = 1,
                IdLoaiCa = 1,
                TenPhienBan = "Ca Hành Chính Chuẩn",
                TongGiayChuan = 28800, // 8 hours * 3600
                CongQuyDoi = 1.0m,
                KhungGios = new List<ShiftFrameDto>
                {
                    new ShiftFrameDto { Stt = 1, BatDauPhut = 480, KetThucPhut = 720, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = 1 }, // 08:00 - 12:00
                    new ShiftFrameDto { Stt = 2, BatDauPhut = 720, KetThucPhut = 780, LoaiKhungGio = "NGHI_KHONG_LUONG", BatBuocQuetThe = 0 }, // 12:00 - 13:00
                    new ShiftFrameDto { Stt = 3, BatDauPhut = 780, KetThucPhut = 1020, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = 1 } // 13:00 - 17:00
                }
            };

            _standardPolicy = new AttendancePolicyDto
            {
                IdQuyDinh = 1,
                MaQuyDinh = "QD_CHUAN_CTY",
                GiayDungSaiHuongCong = 300, // 5 phút dung sai
                CachHuongDungSai = "DU_CONG_NEU_TRONG_NGUONG",
                GiayMienViPhamMuon = 300,
                DemBatDauPhut = 1320, // 22:00
                DemKetThucPhut = 360   // 06:00
            };
        }

        private void AssertAllSegmentsAdhereToOracleConstraints(DayCalculationResult res)
        {
            foreach (var seg in res.Segments)
            {
                // 1. Kiểm tra CK18_PD_TYPE
                Assert.IsTrue(AllowedDbSegmentTypes.Contains(seg.LoaiThoiGian),
                    $"Loại phân đoạn '{seg.LoaiThoiGian}' không nằm trong CK18_PD_TYPE của Oracle!");

                // 2. Kiểm tra CK18_PD_TIME: KETTHUC_LUC > BATDAU_LUC và THOILUONG_GIAY = (KETTHUC - BATDAU)*86400
                Assert.IsTrue(seg.KetThucLuc > seg.BatDauLuc);
                long expectedSeconds = (long)Math.Round((seg.KetThucLuc - seg.BatDauLuc).TotalSeconds);
                Assert.AreEqual(expectedSeconds, seg.ThoiLuongGiay,
                    $"Thời lượng giây {seg.ThoiLuongGiay} không khớp độ dài thời gian {expectedSeconds}!");

                // 3. Ranh giới nửa đêm CK18_PD_TIME: KETTHUC_LUC <= TRUNC(BATDAU_LUC) + 1
                Assert.IsTrue(seg.KetThucLuc <= seg.BatDauLuc.Date.AddDays(1),
                    $"Phân đoạn vượt quá ranh giới nửa đêm {seg.BatDauLuc} -> {seg.KetThucLuc}");

                // 4. Ràng buộc quan hệ giây CK18_PD_SECONDS
                Assert.IsTrue(seg.GiayThucTe <= seg.ThoiLuongGiay);
                Assert.IsTrue(seg.GiayHuongCongThuong <= seg.ThoiLuongGiay);
                Assert.IsTrue(seg.GiayDemOt <= seg.GiayOtXacNhan);
                Assert.IsTrue(seg.GiayDemTrongGioThuong + seg.GiayOtXacNhan <= seg.GiayThucTe);
            }
        }

        [Test]
        public void Test1_StandardWorkingDay_FullEightHours_CreditedProperly()
        {
            DateTime workDate = new DateTime(2026, 10, 5); // Monday
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5001,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 201,
                    MaNV = 1,
                    Ngay = workDate,
                    IdCaPhienBan = 1,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC",
                    LoaiNgay = "THUONG"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9001,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(8),
                    ThoiDiemRa = workDate.AddHours(17)
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);
            Assert.AreEqual("DA_XAC_NHAN", res.TrangThaiCong);
            Assert.AreEqual(1, res.DuDieuKienChot);
            Assert.AreEqual(28800, res.GiayHuongCongThuong, "8 hours = 28,800 seconds");
            Assert.AreEqual(1.0m, res.CongThuongQuyDoi);
            Assert.AreEqual(0, res.GiayDiMuonViPham);
            Assert.AreEqual(0, res.GiayVeSomViPham);
            Assert.AreEqual(0, res.Anomalies.Count);
        }

        [Test]
        public void Test2_ClassicUserCase_In0730_Out2100_WithApprovedOT_SplitsSegmentsCorrectly()
        {
            // Case: Lịch 08:00 - 17:00, Vào 07:30, Ra 21:00. Đơn OT duyệt: 17:30 - 20:00 (2.5h)
            DateTime workDate = new DateTime(2026, 10, 5);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5002,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 202,
                    MaNV = 1,
                    Ngay = workDate,
                    IdCaPhienBan = 1,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9002,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(7).AddMinutes(30), // 07:30
                    ThoiDiemRa = workDate.AddHours(21)                  // 21:00
                },
                ApprovedOvertimes = new List<ApprovedOvertimeDto>
                {
                    new ApprovedOvertimeDto
                    {
                        Id = 301,
                        MaNV = 1,
                        BatDauDuyet = workDate.AddHours(17).AddMinutes(30), // 17:30
                        KetThucDuyet = workDate.AddHours(20),                // 20:00
                        GiayOtDuyet = 9000 // 2.5h * 3600
                    }
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);

            // 1. Phải đủ 8h công chuẩn
            Assert.AreEqual(28800, res.GiayHuongCongThuong, "Công chuẩn đủ 8h (08:00 - 17:00 trừ nghỉ trưa)");
            Assert.AreEqual(1.0m, res.CongThuongQuyDoi);

            // 2. OT đã xác nhận đúng 2.5 tiếng (9,000 giây)
            Assert.AreEqual(9000, res.GiayOtXacNhan, "2.5h OT = 9000 giây");

            // 3. Có mặt làm việc thực tế cả ngày (loại trừ 1h nghỉ trưa): 12.5h = 45,000s
            Assert.AreEqual(45000, res.GiayThucTe);

            // 4. Phát hiện bất thường cho 2 khoảng ngoài ca riêng biệt: 17:00-17:30 và 20:00-21:00
            Assert.AreEqual(2, res.Anomalies.Count, "Phải sinh 2 bất thường riêng cho 17:00-17:30 và 20:00-21:00");
            Assert.IsTrue(res.Anomalies.All(a => a.MaLoi == "OT_CHUA_DUYET" && a.MucDo == "CAN_RA_SOAT" && a.ChanChot == 0));

            // Cơ chế thanh toán phần không tranh chấp (đã tính công thường & OT duyệt) nhưng giữ ngày ở CHO_XAC_MINH (DuDieuKienChot = 0) để chờ xác minh theo CK18_KQ_READY
            Assert.AreEqual(0, res.DuDieuKienChot);
            Assert.AreEqual("CHO_XAC_MINH", res.TrangThaiCong);
        }

        [Test]
        public void Test3_MorningLeave_AfternoonWork_CalculatedCorrectlyWithoutFalseLateArrival()
        {
            // Case: Phép hưởng lương 08:00 - 12:00, Làm việc thực tế 13:00 - 17:00
            DateTime workDate = new DateTime(2026, 10, 6);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5003,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 203,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9003,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(13), // Vào 13:00
                    ThoiDiemRa = workDate.AddHours(17)   // Ra 17:00
                },
                ApprovedLeaves = new List<ApprovedLeaveDto>
                {
                    new ApprovedLeaveDto
                    {
                        Id = 401,
                        MaNV = 1,
                        BatDauNghi = workDate.AddHours(8),
                        KetThucNghi = workDate.AddHours(12),
                        GiayNghi = 14400,
                        CoHuongLuong = 1,
                        LoaiHuongCong = "PHEP_NAM"
                    }
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);

            // Được hưởng đủ 8h công: 4h phép sáng + 4h làm việc chiều
            Assert.AreEqual(28800, res.GiayHuongCongThuong, "4h phép sáng + 4h làm chiều = 28,800s");
            Assert.AreEqual(1.0m, res.CongThuongQuyDoi);
            Assert.AreEqual(0, res.GiayDiMuonViPham, "Nghỉ phép có đơn không được ghi nhận đi muộn");
            Assert.AreEqual(0, res.GiayDiMuonThucTe, "Thời gian phép không phải đi muộn");
            Assert.AreEqual(1, res.DuDieuKienChot);
            Assert.AreEqual("DA_XAC_NHAN", res.TrangThaiCong);
        }

        [Test]
        public void Test4_FullDayLeave_MatchesExactSegmentLengthsAcrossFrames()
        {
            // Case: Phép cả ngày 08:00 - 17:00
            DateTime workDate = new DateTime(2026, 10, 7);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5004,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 204,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = null,
                ApprovedLeaves = new List<ApprovedLeaveDto>
                {
                    new ApprovedLeaveDto
                    {
                        Id = 402,
                        MaNV = 1,
                        BatDauNghi = workDate.AddHours(8),
                        KetThucNghi = workDate.AddHours(17),
                        GiayNghi = 28800,
                        CoHuongLuong = 1,
                        LoaiHuongCong = "PHEP_NAM"
                    }
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);

            // Phải có phân đoạn nghỉ trưa 12:00 - 13:00 là NGHI_KHONG_LUONG
            var lunchSegment = res.Segments.FirstOrDefault(s => s.BatDauLuc == workDate.AddHours(12) && s.KetThucLuc == workDate.AddHours(13));
            Assert.IsNotNull(lunchSegment);
            Assert.AreEqual("NGHI_KHONG_LUONG", lunchSegment.LoaiThoiGian);
            Assert.AreEqual(3600, lunchSegment.ThoiLuongGiay);

            // Tổng hưởng công = 28800s (4h sáng + 4h chiều)
            Assert.AreEqual(28800, res.GiayHuongCongThuong);
            Assert.AreEqual(1.0m, res.CongThuongQuyDoi);
        }

        [Test]
        public void Test5_InvertedPunch_GeneratesBlockingAnomaly_PreventsLocking()
        {
            // Case: Vào 17:00, Ra 08:00 cùng ngày (giờ đảo ngược)
            DateTime workDate = new DateTime(2026, 10, 8);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5005,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 205,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9005,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(17),
                    ThoiDiemRa = workDate.AddHours(8)
                }
            };

            var res = _engine.ProcessDay(input);

            Assert.AreEqual(0, res.DuDieuKienChot, "Dữ liệu giờ đảo ngược bắt buộc DU_DIEUKIEN_CHOT = 0");
            Assert.AreEqual("CHO_XAC_MINH", res.TrangThaiCong);
            var anom = res.Anomalies.FirstOrDefault(a => a.MaLoi == "GIO_DAO_NGUOC");
            Assert.IsNotNull(anom);
            Assert.AreEqual(1, anom.ChanChot);
            Assert.AreEqual("NGHIEP_VU_TREO", anom.MucDo);
        }

        [Test]
        public void Test6_OneHourLeave_BlocksLocking_WhenRemainingHoursAreUnexcusedAbsence()
        {
            // Case: Phép 08:00 - 09:00, nhưng 09:00 - 17:00 không chấm công
            DateTime workDate = new DateTime(2026, 10, 9);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5006,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 206,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = null,
                ApprovedLeaves = new List<ApprovedLeaveDto>
                {
                    new ApprovedLeaveDto
                    {
                        Id = 403,
                        MaNV = 1,
                        BatDauNghi = workDate.AddHours(8),
                        KetThucNghi = workDate.AddHours(9),
                        GiayNghi = 3600,
                        CoHuongLuong = 1
                    }
                }
            };

            var res = _engine.ProcessDay(input);

            // 1h phép được ghi nhận
            Assert.AreEqual(3600, res.GiayHuongCongThuong);

            // Nhưng 7h còn lại là vắng trong ca -> Phát hiện bất thường VANG_CHUA_XAC_NHAN chặn chốt
            Assert.AreEqual(0, res.DuDieuKienChot, "7h vắng còn lại phải chặn chốt công");
            Assert.AreEqual("CHO_XAC_MINH", res.TrangThaiCong);
            var anom = res.Anomalies.FirstOrDefault(a => a.MaLoi == "VANG_CHUA_XAC_NHAN");
            Assert.IsNotNull(anom);
            Assert.AreEqual(1, anom.ChanChot);
        }

        [Test]
        public void Test7_PolicyChanges_ProduceDifferentInputHashes()
        {
            DateTime workDate = new DateTime(2026, 10, 10);
            var input1 = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5007,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy, // Dung sai 300s
                Schedule = new ScheduleDto
                {
                    IdLich = 207,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9007,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(8).AddMinutes(4),
                    ThoiDiemRa = workDate.AddHours(17)
                }
            };

            var res1 = _engine.ProcessDay(input1);

            // Policy 2: Dung sai = 0
            var policyNoGrace = new AttendancePolicyDto
            {
                IdQuyDinh = 2,
                MaQuyDinh = "QD_KHONG_DUNG_SAI",
                GiayDungSaiHuongCong = 0,
                CachHuongDungSai = "KHONG_BU"
            };

            var input2 = new DayCalculationInput
            {
                IdLanTinh = 102,
                IdBangCongCt = 5007,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = policyNoGrace,
                Schedule = input1.Schedule,
                RawPunch = input1.RawPunch
            };

            var res2 = _engine.ProcessDay(input2);

            Assert.AreNotEqual(res1.InputHash, res2.InputHash, "Thay đổi chính sách bắt buộc làm thay đổi InputHash");
            Assert.AreNotEqual(res1.GiayHuongCongThuong, res2.GiayHuongCongThuong, "Công hưởng phải khác nhau khi đổi dung sai");
        }

        [Test]
        public void Test8_DaytimeOtFlag_RequiresActualDaytimeOtWorked_NotMerelyRequested()
        {
            // Case: Có đơn OT ngày 17:00 - 18:00, nhưng thực tế chỉ quẹt thẻ OT đêm 22:00 - 23:00
            DateTime workDate = new DateTime(2026, 10, 11);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5008,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 208,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9008,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(22), // Chỉ đến làm lúc 22:00
                    ThoiDiemRa = workDate.AddHours(23)
                },
                ApprovedOvertimes = new List<ApprovedOvertimeDto>
                {
                    new ApprovedOvertimeDto
                    {
                        Id = 305,
                        MaNV = 1,
                        BatDauDuyet = workDate.AddHours(17),
                        KetThucDuyet = workDate.AddHours(18), // Đăng ký OT ngày nhưng không làm
                        GiayOtDuyet = 3600
                    },
                    new ApprovedOvertimeDto
                    {
                        Id = 306,
                        MaNV = 1,
                        BatDauDuyet = workDate.AddHours(22),
                        KetThucDuyet = workDate.AddHours(23), // Làm OT đêm thực tế
                        GiayOtDuyet = 3600
                    }
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);

            var nightOtSegment = res.Segments.FirstOrDefault(s => s.GiayDemOt > 0);
            Assert.IsNotNull(nightOtSegment);
            Assert.AreEqual(0, nightOtSegment.CoOtBanNgayTruoc,
                "Không thực sự làm OT ban ngày thì CoOtBanNgayTruoc = 0 (hưởng 200%, không bị nhận nhầm 210%)");
        }

        [Test]
        public void Test9_ShortShift_AppliesMultiplierProperly()
        {
            // Case: Ca 4 tiếng có CongQuyDoi = 0.5. Làm đủ 4 tiếng phải trả về 0.5 công!
            DateTime workDate = new DateTime(2026, 10, 12);
            var halfDayShift = new ShiftVersionDto
            {
                IdCaPhienBan = 3,
                IdLoaiCa = 3,
                TenPhienBan = "Ca Sáng 4 Tiếng",
                TongGiayChuan = 14400, // 4 hours
                CongQuyDoi = 0.5m,
                KhungGios = new List<ShiftFrameDto>
                {
                    new ShiftFrameDto { Stt = 1, BatDauPhut = 480, KetThucPhut = 720, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = 1 }
                }
            };

            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5009,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = halfDayShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 209,
                    MaNV = 1,
                    Ngay = workDate,
                    IdCaPhienBan = 3,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(12),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9009,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(8),
                    ThoiDiemRa = workDate.AddHours(12)
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);
            Assert.AreEqual(14400, res.GiayHuongCongThuong);
            Assert.AreEqual(0.5m, res.CongThuongQuyDoi, "Ca 4 tiếng hệ số 0.5 làm đủ phải quy đổi ra đúng 0.5 công!");
        }

        [Test]
        public void Test11_MultiPunch_SplitShift_CalculatesCorrectly()
        {
            // Case: Nhân viên có 2 cặp quẹt thẻ trên cùng một ngày (Ca gãy hoặc ra ngoài giữa ca)
            // Lượt 1: 08:00 - 12:00 (MABC = 1001)
            // Lượt 2: 13:00 - 17:00 (MABC = 1002)
            DateTime workDate = new DateTime(2026, 10, 13);
            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5011,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = _standardShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 211,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(8),
                    KetThucKeHoach = workDate.AddHours(17),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunches = new List<RawPunchDto>
                {
                    new RawPunchDto { Mabc = 1001, MaNV = 1, ThoiDiemVao = workDate.AddHours(8), ThoiDiemRa = workDate.AddHours(12) },
                    new RawPunchDto { Mabc = 1002, MaNV = 1, ThoiDiemVao = workDate.AddHours(13), ThoiDiemRa = workDate.AddHours(17) }
                }
            };

            var res = _engine.ProcessDay(input);

            AssertAllSegmentsAdhereToOracleConstraints(res);

            // 1. Phải tính đủ 8h công chuẩn (28,800s), không bị tính đi muộn hay về sớm
            Assert.AreEqual(28800, res.GiayHuongCongThuong);
            Assert.AreEqual(1.0m, res.CongThuongQuyDoi);
            Assert.AreEqual(28800, res.GiayThucTe);
            Assert.AreEqual(0, res.GiayDiMuonThucTe);
            Assert.AreEqual(0, res.GiayVeSomThucTe);

            // 2. Không có bất thường nào
            Assert.AreEqual(0, res.Anomalies.Count);
            Assert.AreEqual("DA_XAC_NHAN", res.TrangThaiCong);
            Assert.AreEqual(1, res.DuDieuKienChot);

            // 3. Nguồn quẹt thẻ được truy vết chính xác vào từng phân đoạn
            var morningSeg = res.Segments.FirstOrDefault(s => s.BatDauLuc == workDate.AddHours(8) && s.KetThucLuc == workDate.AddHours(12));
            var afternoonSeg = res.Segments.FirstOrDefault(s => s.BatDauLuc == workDate.AddHours(13) && s.KetThucLuc == workDate.AddHours(17));
            Assert.IsNotNull(morningSeg);
            Assert.IsNotNull(afternoonSeg);
            Assert.IsTrue(morningSeg.SourceMabcList.Contains(1001));
            Assert.IsTrue(afternoonSeg.SourceMabcList.Contains(1002));
        }

        [Test]
        public void Test12_OvernightShift_CrossMidnight_SlicesStrictlyAtMidnight()
        {
            // Case: Ca đêm từ 22:00 (hôm nay) đến 06:00 (sáng hôm sau)
            // Phân đoạn bắt buộc phải cắt tại đúng 00:00:00 để tuân thủ triệt để CK18_PD_TIME:
            // KETTHUC_LUC <= TRUNC(BATDAU_LUC) + 1
            DateTime workDate = new DateTime(2026, 10, 31);
            var nightShift = new ShiftVersionDto
            {
                IdCaPhienBan = 4,
                IdLoaiCa = 4,
                TenPhienBan = "Ca Đêm 22h-06h",
                TongGiayChuan = 28800,
                CongQuyDoi = 1.0m,
                KhungGios = new List<ShiftFrameDto>
                {
                    // 22:00 = 1320 phút, 06:00 hôm sau = 1800 phút
                    new ShiftFrameDto { Stt = 1, BatDauPhut = 1320, KetThucPhut = 1800, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = 1 }
                }
            };

            var input = new DayCalculationInput
            {
                IdLanTinh = 101,
                IdBangCongCt = 5012,
                MaNV = 1,
                MaKyCong = 202610,
                Ngay = workDate,
                Shift = nightShift,
                Policy = _standardPolicy,
                Schedule = new ScheduleDto
                {
                    IdLich = 212,
                    MaNV = 1,
                    Ngay = workDate,
                    BatDauKeHoach = workDate.AddHours(22),
                    KetThucKeHoach = workDate.AddDays(1).AddHours(6),
                    TrangThaiPhanCong = "LAM_VIEC"
                },
                RawPunch = new RawPunchDto
                {
                    Mabc = 9012,
                    MaNV = 1,
                    ThoiDiemVao = workDate.AddHours(22),
                    ThoiDiemRa = workDate.AddDays(1).AddHours(6)
                }
            };

            var res = _engine.ProcessDay(input);

            // 1. Phải thỏa mãn toàn bộ constraint Oracle (đặc biệt là CK18_PD_TIME không vượt qua nửa đêm)
            AssertAllSegmentsAdhereToOracleConstraints(res);

            // 2. Đúng 8 tiếng công chuẩn (28,800s) và toàn bộ 8 tiếng là giờ làm đêm (GiayDemTrongGioThuong = 28,800s)
            Assert.AreEqual(28800, res.GiayHuongCongThuong);
            Assert.AreEqual(28800, res.GiayDemTrongGioThuong);
            Assert.AreEqual(1.0m, res.CongThuongQuyDoi);

            // 3. Phân đoạn ca đêm phải được tách thành đúng 2 lát cắt tại 00:00:00:
            // Đoạn 1: 22:00 -> 00:00 (7,200s)
            // Đoạn 2: 00:00 -> 06:00 (21,600s)
            var seg1 = res.Segments.FirstOrDefault(s => s.BatDauLuc == workDate.AddHours(22) && s.KetThucLuc == workDate.AddDays(1));
            var seg2 = res.Segments.FirstOrDefault(s => s.BatDauLuc == workDate.AddDays(1) && s.KetThucLuc == workDate.AddDays(1).AddHours(6));

            Assert.IsNotNull(seg1, "Phải có phân đoạn từ 22:00 đến 00:00");
            Assert.IsNotNull(seg2, "Phải có phân đoạn từ 00:00 đến 06:00 hôm sau");
            Assert.AreEqual(7200, seg1.ThoiLuongGiay);
            Assert.AreEqual(21600, seg2.ThoiLuongGiay);
            Assert.AreEqual(1, seg1.LaBanDem);
            Assert.AreEqual(1, seg2.LaBanDem);
        }

        [Test]
        [Explicit("Legacy live-Oracle mutation test. Use database/synthetic200 guarded runners instead.")]
        public void Test10_OracleDatabase_ConstraintsAcceptEngineOutputs()
        {
            string connStr = "DATA SOURCE=localhost:1521/orcl;USER ID=HR;PASSWORD=hr;";
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        long testRunId = 888801;
                        long testBcctId = 162818; // Existing valid row in TB_BANGCONG_CHITIET
                        long testMaNv = 2707;
                        long testMaKyCong = 202603;
                        DateTime workDate = new DateTime(2026, 3, 1);

                        // 1. Insert TB_QUYDINH_CHAMCONG
                        using (var cmd = new OracleCommand(@"
                            INSERT INTO HR.TB_QUYDINH_CHAMCONG (
                                IDQUYDINH, MA_QUYDINH, SO_PHIENBAN, TEN_QUYDINH, TU_NGAY,
                                GIAY_DUNG_SAI_HUONG_CONG, CACH_HUONG_DUNG_SAI, CACH_DEM_MUON,
                                GIOI_HAN_GHEP_GIAY, TAO_BOI
                            ) VALUES (
                                :p_idqd, 'TEST_QD', 1, 'Quy định test', :p_date,
                                300, 'DU_CONG_NEU_TRONG_NGUONG', 'TOAN_BO_NEU_VUOT',
                                7200, 161
                            )", conn))
                        {
                            cmd.Parameters.Add("p_idqd", testRunId);
                            cmd.Parameters.Add("p_date", workDate);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Insert TB_CA_PHIENBAN
                        using (var cmd = new OracleCommand(@"
                            INSERT INTO HR.TB_CA_PHIENBAN (
                                IDCAPHIENBAN, IDLOAICA, SO_PHIENBAN, TEN_PHIENBAN, TU_NGAY,
                                TONG_GIAY_CHUAN, CONG_QUY_DOI, TAO_BOI
                            ) VALUES (
                                :p_idca, 1, 1, 'Ca test 8h', :p_date,
                                28800, 1.0, 161
                            )", conn))
                        {
                            cmd.Parameters.Add("p_idca", testRunId);
                            cmd.Parameters.Add("p_date", workDate);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Insert TB_LICH_LAMVIEC
                        using (var cmd = new OracleCommand(@"
                            INSERT INTO HR.TB_LICH_LAMVIEC (
                                IDLICH, MANV, NGAY, MA_PHANCONG, SO_PHIENBAN, IDCAPHIENBAN, IDQUYDINH,
                                BATDAU_KEHOACH, KETTHUC_KEHOACH, TRANG_THAI_PHAN_CONG, LOAI_NGAY,
                                TRANG_THAI, NGUON_PHAN_CONG, TAO_BOI
                            ) VALUES (
                                :p_idlich, :p_manv, :p_ngay, 'PC_TEST_888', 1, :p_idca, :p_idqd,
                                :p_start, :p_end, 'LAM_VIEC', 'THUONG',
                                'PUBLISHED', 'TEST', 161
                            )", conn))
                        {
                            cmd.Parameters.Add("p_idlich", testRunId);
                            cmd.Parameters.Add("p_manv", testMaNv);
                            cmd.Parameters.Add("p_ngay", workDate);
                            cmd.Parameters.Add("p_idca", testRunId);
                            cmd.Parameters.Add("p_idqd", testRunId);
                            cmd.Parameters.Add("p_start", workDate.AddHours(8));
                            cmd.Parameters.Add("p_end", workDate.AddHours(17));
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Insert TB_CHAMCONG_LANTINH
                        using (var cmd = new OracleCommand(@"
                            INSERT INTO HR.TB_CHAMCONG_LANTINH (
                                IDLANTINH, MA_YEU_CAU, REQUEST_HASH, MAKYCONG, TU_NGAY, DEN_NGAY,
                                INPUT_REV, EXPECTED_PUBLISH_REV, PHIENBAN_ENGINE, TRANG_THAI, NGUOI_THUC_HIEN
                            ) VALUES (
                                :p_idlt, 'REQ_TEST_8888', 'HASH8888', :p_mkc, :p_tu, :p_den,
                                0, 0, 'v1.0.0', 'DANG_CHAY', 161
                            )", conn))
                        {
                            cmd.Parameters.Add("p_idlt", testRunId);
                            cmd.Parameters.Add("p_mkc", testMaKyCong);
                            cmd.Parameters.Add("p_tu", workDate);
                            cmd.Parameters.Add("p_den", workDate.AddDays(1));
                            cmd.ExecuteNonQuery();
                        }

                        // 5. Run TimeSegmentationEngine for In 07:30, Out 21:00 with approved OT 17:30 - 20:00
                        var input = new DayCalculationInput
                        {
                            IdLanTinh = testRunId,
                            IdBangCongCt = testBcctId,
                            MaNV = testMaNv,
                            MaKyCong = testMaKyCong,
                            Ngay = workDate,
                            Shift = _standardShift,
                            Policy = _standardPolicy,
                            Schedule = new ScheduleDto
                            {
                                IdLich = testRunId,
                                MaNV = testMaNv,
                                Ngay = workDate,
                                IdCaPhienBan = testRunId,
                                BatDauKeHoach = workDate.AddHours(8),
                                KetThucKeHoach = workDate.AddHours(17),
                                TrangThaiPhanCong = "LAM_VIEC"
                            },
                            RawPunch = new RawPunchDto
                            {
                                Mabc = 999991,
                                MaNV = testMaNv,
                                ThoiDiemVao = workDate.AddHours(7).AddMinutes(30),
                                ThoiDiemRa = workDate.AddHours(21)
                            },
                            ApprovedOvertimes = new List<ApprovedOvertimeDto>
                            {
                                new ApprovedOvertimeDto
                                {
                                    Id = 301,
                                    MaNV = testMaNv,
                                    BatDauDuyet = workDate.AddHours(17).AddMinutes(30),
                                    KetThucDuyet = workDate.AddHours(20),
                                    GiayOtDuyet = 9000
                                }
                            }
                        };
                        var calcResult = _engine.ProcessDay(input);

                        // 6. Insert into TB_CHAMCONG_KQ_NGAY
                        using (var cmd = new OracleCommand(@"
                            INSERT INTO HR.TB_CHAMCONG_KQ_NGAY (
                                IDLANTINH, IDBANGCONGCT, MANV, MAKYCONG, NGAY, INPUT_HASH, INPUT_SNAPSHOT,
                                TRANG_THAI, DU_DIEUKIEN_CHOT, GIAY_THUC_TE, GIAY_HUONG_CONG_THUONG,
                                GIAY_OT_XAC_NHAN, GIAY_DEM_TRONG_GIO_THUONG, GIAY_DEM_OT,
                                GIAY_DI_MUON_THUC_TE, GIAY_VE_SOM_THUC_TE, GIAY_DI_MUON_VIPHAM, GIAY_VE_SOM_VIPHAM,
                                CONG_THUONG_QUYDOI
                            ) VALUES (
                                :p_idlt, :p_bcct, :p_manv, :p_mkc, :p_ngay, :p_hash, :p_snap,
                                :p_tt, :p_ready, :p_thucte, :p_huongcong,
                                :p_ot, :p_dem_thuong, :p_dem_ot,
                                :p_muon_tt, :p_som_tt, :p_muon_vp, :p_som_vp,
                                :p_cong
                            )", conn))
                        {
                            cmd.Parameters.Add("p_idlt", testRunId);
                            cmd.Parameters.Add("p_bcct", testBcctId);
                            cmd.Parameters.Add("p_manv", testMaNv);
                            cmd.Parameters.Add("p_mkc", testMaKyCong);
                            cmd.Parameters.Add("p_ngay", workDate);
                            cmd.Parameters.Add("p_hash", calcResult.InputHash);
                            cmd.Parameters.Add("p_snap", calcResult.InputSnapshotJson);
                            cmd.Parameters.Add("p_tt", calcResult.TrangThaiCong);
                            cmd.Parameters.Add("p_ready", calcResult.DuDieuKienChot);
                            cmd.Parameters.Add("p_thucte", calcResult.GiayThucTe);
                            cmd.Parameters.Add("p_huongcong", calcResult.GiayHuongCongThuong);
                            cmd.Parameters.Add("p_ot", calcResult.GiayOtXacNhan);
                            cmd.Parameters.Add("p_dem_thuong", calcResult.GiayDemTrongGioThuong);
                            cmd.Parameters.Add("p_dem_ot", calcResult.GiayDemOt);
                            cmd.Parameters.Add("p_muon_tt", calcResult.GiayDiMuonThucTe);
                            cmd.Parameters.Add("p_som_tt", calcResult.GiayVeSomThucTe);
                            cmd.Parameters.Add("p_muon_vp", calcResult.GiayDiMuonViPham);
                            cmd.Parameters.Add("p_som_vp", calcResult.GiayVeSomViPham);
                            cmd.Parameters.Add("p_cong", calcResult.CongThuongQuyDoi);
                            cmd.ExecuteNonQuery();
                        }

                        // 7. Insert all segments into TB_CONG_PHANDOAN
                        foreach (var seg in calcResult.Segments)
                        {
                            using (var cmd = new OracleCommand(@"
                                INSERT INTO HR.TB_CONG_PHANDOAN (
                                    IDLANTINH, IDBANGCONGCT, MANV, IDLICH, IDQUYDINH, BATDAU_LUC, KETTHUC_LUC,
                                    THOILUONG_GIAY, LOAI_THOIGIAN, LOAI_NGAY, LA_BAN_DEM, CO_OT_BAN_NGAY_TRUOC,
                                    TRANGTHAI_XACNHAN, GIAY_THUC_TE, GIAY_HUONG_CONG_THUONG, GIAY_OT_XAC_NHAN,
                                    GIAY_DEM_TRONG_GIO_THUONG, GIAY_DEM_OT, GIAY_DI_MUON_THUC_TE, GIAY_VE_SOM_THUC_TE,
                                    GIAY_DI_MUON_VIPHAM, GIAY_VE_SOM_VIPHAM
                                ) VALUES (
                                    :p_idlt, :p_bcct, :p_manv, :p_idlich, :p_idqd, :p_start, :p_end,
                                    :p_dur, :p_type, :p_daytype, :p_night, :p_otday,
                                    :p_status, :p_thucte, :p_huongcong, :p_ot,
                                    :p_dem_thuong, :p_dem_ot, :p_muon_tt, :p_som_tt,
                                    :p_muon_vp, :p_som_vp
                                )", conn))
                            {
                                cmd.Parameters.Add("p_idlt", testRunId);
                                cmd.Parameters.Add("p_bcct", testBcctId);
                                cmd.Parameters.Add("p_manv", testMaNv);
                                cmd.Parameters.Add("p_idlich", testRunId);
                                cmd.Parameters.Add("p_idqd", testRunId);
                                cmd.Parameters.Add("p_start", seg.BatDauLuc);
                                cmd.Parameters.Add("p_end", seg.KetThucLuc);
                                cmd.Parameters.Add("p_dur", seg.ThoiLuongGiay);
                                cmd.Parameters.Add("p_type", seg.LoaiThoiGian);
                                cmd.Parameters.Add("p_daytype", seg.LoaiNgay);
                                cmd.Parameters.Add("p_night", seg.LaBanDem);
                                cmd.Parameters.Add("p_otday", seg.CoOtBanNgayTruoc);
                                cmd.Parameters.Add("p_status", seg.TrangThaiXacNhan);
                                cmd.Parameters.Add("p_thucte", seg.GiayThucTe);
                                cmd.Parameters.Add("p_huongcong", seg.GiayHuongCongThuong);
                                cmd.Parameters.Add("p_ot", seg.GiayOtXacNhan);
                                cmd.Parameters.Add("p_dem_thuong", seg.GiayDemTrongGioThuong);
                                cmd.Parameters.Add("p_dem_ot", seg.GiayDemOt);
                                cmd.Parameters.Add("p_muon_tt", seg.GiayDiMuonThucTe);
                                cmd.Parameters.Add("p_som_tt", seg.GiayVeSomThucTe);
                                cmd.Parameters.Add("p_muon_vp", seg.GiayDiMuonViPham);
                                cmd.Parameters.Add("p_som_vp", seg.GiayVeSomViPham);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 8. Insert anomalies into TB_CHAMCONG_BATTHUONG
                        foreach (var anom in calcResult.Anomalies)
                        {
                            using (var cmd = new OracleCommand(@"
                                INSERT INTO HR.TB_CHAMCONG_BATTHUONG (
                                    MA_SU_VIEC_HASH, IDLANTINH_PHATHIEN, IDBANGCONGCT, MANV, IDLICH, NGAY,
                                    MA_LOI, BATDAU_LUC, KETTHUC_LUC, MUC_DO, CHAN_CHOT, INPUT_HASH,
                                    INPUT_SNAPSHOT, TRANG_THAI, MO_TA
                                ) VALUES (
                                    :p_hash, :p_idlt, :p_bcct, :p_manv, :p_idlich, :p_ngay,
                                    :p_maloi, :p_start, :p_end, :p_mucdo, :p_chan, :p_inhash,
                                    :p_insnap, :p_tt, :p_mota
                                )", conn))
                            {
                                cmd.Parameters.Add("p_hash", anom.MaSuViecHash);
                                cmd.Parameters.Add("p_idlt", testRunId);
                                cmd.Parameters.Add("p_bcct", testBcctId);
                                cmd.Parameters.Add("p_manv", testMaNv);
                                cmd.Parameters.Add("p_idlich", testRunId);
                                cmd.Parameters.Add("p_ngay", workDate);
                                cmd.Parameters.Add("p_maloi", anom.MaLoi);
                                cmd.Parameters.Add("p_start", (object)anom.BatDauLuc ?? DBNull.Value);
                                cmd.Parameters.Add("p_end", (object)anom.KetThucLuc ?? DBNull.Value);
                                cmd.Parameters.Add("p_mucdo", anom.MucDo);
                                cmd.Parameters.Add("p_chan", anom.ChanChot);
                                cmd.Parameters.Add("p_inhash", anom.InputHash);
                                cmd.Parameters.Add("p_insnap", anom.InputSnapshotJson);
                                cmd.Parameters.Add("p_tt", anom.TrangThai);
                                cmd.Parameters.Add("p_mota", anom.MoTa);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 9. Update TB_BANGCONG_CHITIET with new pointer and metrics (verifying FK18_BCCT_CURRENT & CK18_BCCT_CURRENT)
                        using (var cmd = new OracleCommand(@"
                            UPDATE HR.TB_BANGCONG_CHITIET
                            SET LANTINH_ID_HIENHANH = :p_idlt,
                                TRANGTHAI_CONG = :p_tt,
                                DU_DIEUKIEN_CHOT = :p_ready,
                                GIAY_THUC_TE = :p_thucte,
                                GIAY_HUONG_CONG_THUONG = :p_huongcong,
                                GIAY_OT_XAC_NHAN = :p_ot,
                                GIAY_DEM_TRONG_GIO_THUONG = :p_dem_thuong,
                                GIAY_DEM_OT = :p_dem_ot,
                                GIAY_DI_MUON_THUC_TE = :p_muon_tt,
                                GIAY_VE_SOM_THUC_TE = :p_som_tt,
                                GIAY_DI_MUON_VIPHAM = :p_muon_vp,
                                GIAY_VE_SOM_VIPHAM = :p_som_vp
                            WHERE IDBANGCONGCT = :p_bcct", conn))
                        {
                            cmd.Parameters.Add("p_idlt", testRunId);
                            cmd.Parameters.Add("p_tt", calcResult.TrangThaiCong);
                            cmd.Parameters.Add("p_ready", calcResult.DuDieuKienChot);
                            cmd.Parameters.Add("p_thucte", calcResult.GiayThucTe);
                            cmd.Parameters.Add("p_huongcong", calcResult.GiayHuongCongThuong);
                            cmd.Parameters.Add("p_ot", calcResult.GiayOtXacNhan);
                            cmd.Parameters.Add("p_dem_thuong", calcResult.GiayDemTrongGioThuong);
                            cmd.Parameters.Add("p_dem_ot", calcResult.GiayDemOt);
                            cmd.Parameters.Add("p_muon_tt", calcResult.GiayDiMuonThucTe);
                            cmd.Parameters.Add("p_som_tt", calcResult.GiayVeSomThucTe);
                            cmd.Parameters.Add("p_muon_vp", calcResult.GiayDiMuonViPham);
                            cmd.Parameters.Add("p_som_vp", calcResult.GiayVeSomViPham);
                            cmd.Parameters.Add("p_bcct", testBcctId);
                            int rowsUpdated = cmd.ExecuteNonQuery();
                            Assert.AreEqual(1, rowsUpdated, "Expected exactly 1 row updated in TB_BANGCONG_CHITIET");
                        }
                    }
                    finally
                    {
                        // Luôn rollback để giữ database nguyên vẹn sạch sẽ
                        tx.Rollback();
                    }
                }
            }
        }

        [Test]
        [Explicit("Legacy live-Oracle mutation test. Use database/synthetic200 guarded runners instead.")]
        public void Test13_AttendancePublishingService_PessimisticLockingAndRollup()
        {
            string connStr = "DATA SOURCE=localhost:1521/orcl;USER ID=HR;PASSWORD=hr;";
            var service = new AttendancePublishingService(connStr);

            long testUserId = 80; // Admin user in TB_SYS_USER
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (var cmd = new OracleCommand("SELECT IDUSER FROM HR.TB_SYS_USER WHERE ROWNUM = 1", conn))
                {
                    testUserId = Convert.ToInt64(cmd.ExecuteScalar());
                }
            }

            // 1. Kiểm tra kỳ đã khóa (202602 có KHOA = 1): Bắt buộc từ chối công bố
            var lockReq = new AttendancePublishRequest
            {
                MaKyCong = 202602,
                NguoiThucHien = testUserId,
                ManvList = new List<long> { 2707 },
                MaYeuCau = "TEST_LOCKED_PERIOD"
            };
            var lockRes = service.PublishAttendance(lockReq);
            Assert.IsFalse(lockRes.Success, "Kỳ công đã khóa (KHOA = 1) bắt buộc phải bị từ chối công bố");
            Assert.AreEqual("THAT_BAI", lockRes.TrangThai);
            StringAssert.Contains("đã bị khóa", lockRes.ErrorMessage);

            // 2. Kiểm tra công bố thành công trên kỳ mở (202603 có KHOA = 0) cho nhân viên 2707
            long initialPublishRev = 0;
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (var cmd = new OracleCommand("SELECT NVL(CONG_PUBLISH_REV, 0) FROM HR.TB_KYCONG WHERE MAKYCONG = 202603", conn))
                {
                    initialPublishRev = Convert.ToInt64(cmd.ExecuteScalar());
                }
            }

            string testReqCode = "TEST_PUB_202603_" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var pubReq = new AttendancePublishRequest
            {
                MaKyCong = 202603,
                NguoiThucHien = testUserId,
                ManvList = new List<long> { 2707 },
                MaYeuCau = testReqCode,
                GhiChu = "Test công bố tự động"
            };

            var pubRes = service.PublishAttendance(pubReq);
            try
            {
                Assert.IsTrue(pubRes.Success, "Công bố kỳ mở phải thành công: " + pubRes.ErrorMessage);
                Assert.AreEqual("DA_CONG_BO", pubRes.TrangThai);
                Assert.IsTrue(pubRes.IdLanTinh > 0);
                Assert.IsTrue(pubRes.SoNgayCong > 0);

                // 3. Xác minh trong database:
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // a. TB_CHAMCONG_LANTINH có trạng thái DA_CONG_BO, CONGBO_LUC không null
                    using (var cmd = new OracleCommand(@"
                        SELECT TRANG_THAI, CONGBO_LUC, INPUT_DATA_HASH 
                        FROM HR.TB_CHAMCONG_LANTINH 
                        WHERE IDLANTINH = :p_idlt", conn))
                    {
                        cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                        using (var reader = cmd.ExecuteReader())
                        {
                            Assert.IsTrue(reader.Read(), "Phải tìm thấy dòng run trong TB_CHAMCONG_LANTINH");
                            Assert.AreEqual("DA_CONG_BO", reader["TRANG_THAI"].ToString());
                            Assert.IsNotNull(reader["CONGBO_LUC"]);
                            Assert.IsNotNull(reader["INPUT_DATA_HASH"]);
                        }
                    }

                    // b. TB_BANGCONG_CHITIET đã trỏ LANTINH_ID_HIENHANH về pubRes.IdLanTinh
                    using (var cmd = new OracleCommand(@"
                        SELECT COUNT(*) 
                        FROM HR.TB_BANGCONG_CHITIET 
                        WHERE MAKYCONG = 202603 AND MANV = 2707 AND LANTINH_ID_HIENHANH = :p_idlt", conn))
                    {
                        cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert.IsTrue(count > 0, "TB_BANGCONG_CHITIET phải được gắn con trỏ LANTINH_ID_HIENHANH mới");
                    }

                    // c. TB_KYCONG: CONG_PUBLISH_REV đã được tăng thêm 1
                    using (var cmd = new OracleCommand("SELECT NVL(CONG_PUBLISH_REV, 0) FROM HR.TB_KYCONG WHERE MAKYCONG = 202603", conn))
                    {
                        long newPublishRev = Convert.ToInt64(cmd.ExecuteScalar());
                        Assert.AreEqual(initialPublishRev + 1, newPublishRev, "CONG_PUBLISH_REV phải tăng đúng 1");
                    }
                }
            }
            finally
            {
                // Dọn dẹp dữ liệu test để bảo đảm database sạch sẽ
                if (pubRes.IdLanTinh > 0)
                {
                    using (var conn = new OracleConnection(connStr))
                    {
                        conn.Open();
                        using (var tx = conn.BeginTransaction())
                        {
                            try
                            {
                                using (var cmd = new OracleCommand("UPDATE HR.TB_BANGCONG_CHITIET SET LANTINH_ID_HIENHANH = NULL WHERE LANTINH_ID_HIENHANH = :p_idlt", conn))
                                {
                                    cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new OracleCommand("DELETE FROM HR.TB_CONG_PD_NGUON WHERE IDPHANDOAN IN (SELECT IDPHANDOAN FROM HR.TB_CONG_PHANDOAN WHERE IDLANTINH = :p_idlt)", conn))
                                {
                                    cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new OracleCommand("DELETE FROM HR.TB_CONG_PHANDOAN WHERE IDLANTINH = :p_idlt", conn))
                                {
                                    cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new OracleCommand("DELETE FROM HR.TB_CHAMCONG_BATTHUONG WHERE IDLANTINH_PHATHIEN = :p_idlt", conn))
                                {
                                    cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new OracleCommand("DELETE FROM HR.TB_CHAMCONG_KQ_NGAY WHERE IDLANTINH = :p_idlt", conn))
                                {
                                    cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new OracleCommand("DELETE FROM HR.TB_CHAMCONG_LANTINH WHERE IDLANTINH = :p_idlt", conn))
                                {
                                    cmd.Parameters.Add("p_idlt", pubRes.IdLanTinh);
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new OracleCommand("UPDATE HR.TB_KYCONG SET CONG_PUBLISH_REV = :p_rev WHERE MAKYCONG = 202603", conn))
                                {
                                    cmd.Parameters.Add("p_rev", initialPublishRev);
                                    cmd.ExecuteNonQuery();
                                }
                                tx.Commit();
                            }
                            catch { tx.Rollback(); }
                        }
                    }
                }
            }
        }

        [OneTimeTearDown]
        public void CleanupPools()
        {
            try
            {
                Oracle.ManagedDataAccess.Client.OracleConnection.ClearAllPools();
            }
            catch { }
        }
    }
}
