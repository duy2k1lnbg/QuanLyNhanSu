using Bu.CLASS_CHAMCONG;
using DA;
using NUnit.Framework;
using System;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class TangCaOvertimeTests
    {
        private TANGCA _tangca;
        private HESO_TANGCA _heSoTangCa;

        [SetUp]
        public void Setup()
        {
            _tangca = new TANGCA();
            _heSoTangCa = new HESO_TANGCA();
        }

        [Test]
        public void Test_TinhSoGio_DayShift_CalculatesCorrectly()
        {
            double hours = _tangca.TinhSoGio("17:00", "21:00");
            Assert.AreEqual(4.0, hours);

            double hours2 = _tangca.TinhSoGio("06:00", "08:00");
            Assert.AreEqual(2.0, hours2);

            double hours3 = _tangca.TinhSoGio("08:00", "17:30");
            Assert.AreEqual(9.5, hours3);
        }

        [Test]
        public void Test_TinhSoGio_Overnight_CalculatesCorrectly()
        {
            // Ca qua đêm: 22:00 -> 02:00 = 4 giờ
            double hours = _tangca.TinhSoGio("22:00", "02:00");
            Assert.AreEqual(4.0, hours);

            // 20:00 -> 04:30 = 8.5 giờ
            double hours2 = _tangca.TinhSoGio("20:00", "04:30");
            Assert.AreEqual(8.5, hours2);

            // 23:00 -> 06:00 = 7 giờ
            double hours3 = _tangca.TinhSoGio("23:00", "06:00");
            Assert.AreEqual(7.0, hours3);
        }

        [Test]
        public void Test_TinhSoGio_Invalid_ReturnsZero()
        {
            Assert.AreEqual(0, _tangca.TinhSoGio("", ""));
            Assert.AreEqual(0, _tangca.TinhSoGio(null, "17:00"));
            Assert.AreEqual(0, _tangca.TinhSoGio("abc", "xyz"));
        }

        [Test]
        public void Test_Database_LoaiCa_And_LoaiCong_Standards()
        {
            using (var db = new MyEntities())
            {
                var loaiCaList = db.TB_LOAICA.ToList();
                Assert.AreEqual(2, loaiCaList.Count, "TB_LOAICA chỉ được có 2 loại ca (Ca ngày và Ca đêm)");

                var caNgay = loaiCaList.FirstOrDefault(x => x.IDLOAICA == 1);
                Assert.IsNotNull(caNgay);
                Assert.AreEqual(1.00m, (decimal)(caNgay.HESOLOAICA ?? 0));

                var caDem = loaiCaList.FirstOrDefault(x => x.IDLOAICA == 2);
                Assert.IsNotNull(caDem);
                Assert.AreEqual(1.30m, (decimal)(caDem.HESOLOAICA ?? 0));

                var loaiCongList = db.TB_LOAICONG.ToList();
                Assert.AreEqual(3, loaiCongList.Count, "TB_LOAICONG chỉ được có 3 loại công (Thường, CN, Lễ)");

                var congThuong = loaiCongList.FirstOrDefault(x => x.IDLOAICONG == 1);
                Assert.IsNotNull(congThuong);
                Assert.AreEqual(1.00m, (decimal)(congThuong.HESOLOAICONG ?? 0));

                var congCN = loaiCongList.FirstOrDefault(x => x.IDLOAICONG == 2);
                Assert.IsNotNull(congCN);
                Assert.AreEqual(1.50m, (decimal)(congCN.HESOLOAICONG ?? 0));

                var congLe = loaiCongList.FirstOrDefault(x => x.IDLOAICONG == 3);
                Assert.IsNotNull(congLe);
                Assert.AreEqual(2.00m, (decimal)(congLe.HESOLOAICONG ?? 0));
            }
        }

        [Test]
        public void Test_Database_HeSoTangCa_Has10Rules()
        {
            var rules = _heSoTangCa.GetList();
            Assert.AreEqual(10, rules.Count, "TB_HESO_TANGCA phải có đủ 10 quy định theo yêu cầu nghiệp vụ");

            // 1. Tăng ca ca ngày 06:00–08:00: 150%, thử việc 127.5%
            var r1 = rules.FirstOrDefault(x => x.IDLOAICONG == 1 && x.IDLOAICA == 1 && x.GIO_BATDAU == "06:00" && x.GIO_KETTHUC == "08:00");
            Assert.IsNotNull(r1);
            Assert.AreEqual(1.50m, r1.HESO_CHINHTHUC);
            Assert.AreEqual(1.275m, r1.HESO_THUVIEC);

            // 2. Tăng ca ca ngày 17:00–22:00: 150%, thử việc 127.5%
            var r2 = rules.FirstOrDefault(x => x.IDLOAICONG == 1 && x.IDLOAICA == 1 && x.GIO_BATDAU == "17:00" && x.GIO_KETTHUC == "22:00");
            Assert.IsNotNull(r2);
            Assert.AreEqual(1.50m, r2.HESO_CHINHTHUC);
            Assert.AreEqual(1.275m, r2.HESO_THUVIEC);

            // 3. Tăng ca ca ngày sau 22:00–06:00: 200%, thử việc 170%
            var r3 = rules.FirstOrDefault(x => x.IDLOAICONG == 1 && x.IDLOAICA == 1 && x.GIO_BATDAU == "22:00" && x.GIO_KETTHUC == "06:00");
            Assert.IsNotNull(r3);
            Assert.AreEqual(2.00m, r3.HESO_CHINHTHUC);
            Assert.AreEqual(1.700m, r3.HESO_THUVIEC);

            // 4. Tăng ca Chủ nhật 08:00–22:00: 200%, thử việc 170%
            var r4 = rules.FirstOrDefault(x => x.IDLOAICONG == 2 && x.GIO_BATDAU == "08:00" && x.GIO_KETTHUC == "22:00");
            Assert.IsNotNull(r4);
            Assert.AreEqual(2.00m, r4.HESO_CHINHTHUC);
            Assert.AreEqual(1.700m, r4.HESO_THUVIEC);

            // 5. Tăng ca Chủ nhật sau 22:00–06:00: 270%, thử việc 229.5%
            var r5 = rules.FirstOrDefault(x => x.IDLOAICONG == 2 && x.IDLOAICA == null && x.GIO_BATDAU == "22:00" && x.GIO_KETTHUC == "06:00");
            Assert.IsNotNull(r5);
            Assert.AreEqual(2.70m, r5.HESO_CHINHTHUC);
            Assert.AreEqual(2.295m, r5.HESO_THUVIEC);

            // 6. Tăng ca ca đêm 05:30–06:00: 200%, thử việc 170%
            var r6 = rules.FirstOrDefault(x => x.IDLOAICONG == 1 && x.IDLOAICA == 2 && x.GIO_BATDAU == "05:30" && x.GIO_KETTHUC == "06:00");
            Assert.IsNotNull(r6);
            Assert.AreEqual(2.00m, r6.HESO_CHINHTHUC);
            Assert.AreEqual(1.700m, r6.HESO_THUVIEC);

            // 7. Tăng ca ca đêm 06:00–08:00: 180%, thử việc 153%
            var r7 = rules.FirstOrDefault(x => x.IDLOAICONG == 1 && x.IDLOAICA == 2 && x.GIO_BATDAU == "06:00" && x.GIO_KETTHUC == "08:00");
            Assert.IsNotNull(r7);
            Assert.AreEqual(1.80m, r7.HESO_CHINHTHUC);
            Assert.AreEqual(1.530m, r7.HESO_THUVIEC);

            // 8. Làm ca đêm Chủ nhật 20:00–08:00: 270%, thử việc 229.5%
            var r8 = rules.FirstOrDefault(x => x.IDLOAICONG == 2 && x.IDLOAICA == 2 && x.GIO_BATDAU == "20:00" && x.GIO_KETTHUC == "08:00");
            Assert.IsNotNull(r8);
            Assert.AreEqual(2.70m, r8.HESO_CHINHTHUC);
            Assert.AreEqual(2.295m, r8.HESO_THUVIEC);

            // 9. Tăng ca ngày lễ 08:00–22:00: 300%, thử việc 255%
            var r9 = rules.FirstOrDefault(x => x.IDLOAICONG == 3 && x.GIO_BATDAU == "08:00" && x.GIO_KETTHUC == "22:00");
            Assert.IsNotNull(r9);
            Assert.AreEqual(3.00m, r9.HESO_CHINHTHUC);
            Assert.AreEqual(2.550m, r9.HESO_THUVIEC);

            // 10. Tăng ca ngày lễ sau 22:00: 390%, thử việc 331.5%
            var r10 = rules.FirstOrDefault(x => x.IDLOAICONG == 3 && x.GIO_BATDAU == "22:00" && x.GIO_KETTHUC == "06:00");
            Assert.IsNotNull(r10);
            Assert.AreEqual(3.90m, r10.HESO_CHINHTHUC);
            Assert.AreEqual(3.315m, r10.HESO_THUVIEC);
        }

        [Test]
        public void Test_TimQuyDinh_MatchesAccurately()
        {
            // Ca ngày thường 17:00-21:00
            var qd1 = _heSoTangCa.TimQuyDinh(1, 1, "17:00", "21:00");
            Assert.IsNotNull(qd1);
            Assert.AreEqual(1.50m, qd1.HESO_CHINHTHUC);

            // Ca ngày thường sau 22h qua đêm 22:00-02:00
            var qd2 = _heSoTangCa.TimQuyDinh(1, 1, "22:00", "02:00");
            Assert.IsNotNull(qd2);
            Assert.AreEqual(2.00m, qd2.HESO_CHINHTHUC);

            // Chủ nhật ban ngày 08:00-17:00
            var qd3 = _heSoTangCa.TimQuyDinh(2, 1, "08:00", "17:00");
            Assert.IsNotNull(qd3);
            Assert.AreEqual(2.00m, qd3.HESO_CHINHTHUC);

            // Chủ nhật sau 22h qua đêm 22:00-02:00
            var qd4 = _heSoTangCa.TimQuyDinh(2, 1, "22:00", "02:00");
            Assert.IsNotNull(qd4);
            Assert.AreEqual(2.70m, qd4.HESO_CHINHTHUC);

            // Làm ca đêm Chủ nhật 20:00-04:00
            var qd5 = _heSoTangCa.TimQuyDinh(2, 2, "20:00", "04:00");
            Assert.IsNotNull(qd5);
            Assert.AreEqual(2.70m, qd5.HESO_CHINHTHUC);

            // Ngày lễ 08:00-17:00
            var qd6 = _heSoTangCa.TimQuyDinh(3, 1, "08:00", "17:00");
            Assert.IsNotNull(qd6);
            Assert.AreEqual(3.00m, qd6.HESO_CHINHTHUC);

            // Ngày lễ sau 22h 22:00-02:00
            var qd7 = _heSoTangCa.TimQuyDinh(3, 1, "22:00", "02:00");
            Assert.IsNotNull(qd7);
            Assert.AreEqual(3.90m, qd7.HESO_CHINHTHUC);
        }

        [Test]
        public void Test_Calculation_Salary_And_Probation_Formula()
        {
            // Kiểm tra nhân viên hợp lệ trong CSDL (ví dụ MANV = 62)
            int testManv = 62;
            DateTime ngay = new DateTime(2026, 9, 13); // Chủ nhật

            decimal otRate = _tangca.GetMucLuong1GioOT(testManv, ngay.Year, ngay.Month);
            Assert.IsTrue(otRate > 0, "Mức 1h OT của nhân viên phải lớn hơn 0");

            // Tăng ca Chủ nhật 08:00-12:00 = 4.0h
            // Chính thức: Hệ số 2.0 (200%)
            var calcChinhThuc = _tangca.TinhChiTietTangCa(testManv, ngay, 1, 2, "08:00", "12:00", false);
            Assert.AreEqual(4.0, calcChinhThuc.SoGio);
            Assert.AreEqual(2.00m, calcChinhThuc.HeSo);
            decimal expectedMoney1 = Math.Round(4.0m * otRate * 2.00m, 0);
            Assert.AreEqual(expectedMoney1, calcChinhThuc.ThanhTien);

            // Thử việc: Hệ số 2.0 * 85% = 1.70 (170%)
            var calcThuViec = _tangca.TinhChiTietTangCa(testManv, ngay, 1, 2, "08:00", "12:00", true);
            Assert.AreEqual(4.0, calcThuViec.SoGio);
            Assert.AreEqual(1.700m, calcThuViec.HeSo);
            decimal expectedMoney2 = Math.Round(4.0m * otRate * 1.700m, 0);
            Assert.AreEqual(expectedMoney2, calcThuViec.ThanhTien);
        }
    }
}
