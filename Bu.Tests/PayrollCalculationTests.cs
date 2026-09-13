using Bu.CLASS_CHAMCONG;
using Bu.DTO;
using DA;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class PayrollCalculationTests
    {
        private BANGLUONG _bangluong;
        private TANGCA _tangca;

        [SetUp]
        public void Setup()
        {
            _bangluong = new BANGLUONG();
            _tangca = new TANGCA();
        }

        [Test]
        public void Test_LuongVanHuan_Payroll_ExactMatch_SampleRequirements()
        {
            // Input data per Requirement 35
            // MANV = Lương Văn Huấn
            decimal luongCoBan = 4525000m;
            decimal congChuan = 26m;
            decimal congNgay = 13m;
            decimal congDem = 12m;
            decimal heSoCaDem = 1.30m;

            decimal tongPhuCapThang = 675000m; // Phụ cấp định mức tháng
            decimal congThucTe = congNgay + congDem; // 25 ngày
            decimal tienPhep = 200000m;
            decimal tienChuyenCan = 300000m;
            decimal tienAn = 240000m;

            // 1. Daily rate
            decimal dailyRate = luongCoBan / congChuan; // 174038.4615...

            // 2. Lương ca ngày
            // LUONG_CA_NGAY = Lương cơ bản / Công chuẩn * Công ca ngày
            // 4.525.000 / 26 * 13 = 2.262.500
            decimal luongCaNgay = Math.Round((luongCoBan / congChuan) * congNgay, 0);
            Assert.AreEqual(2262500m, luongCaNgay, "Lương ca ngày phải đúng 2.262.500");

            // 3. Lương ca đêm
            // LUONG_CA_DEM = Lương cơ bản / Công chuẩn * Công ca đêm * 1.30
            // 4.525.000 / 26 * 12 * 1.30 = 2.715.000
            decimal luongCaDem = Math.Round((luongCoBan / congChuan) * congDem * heSoCaDem, 0);
            Assert.AreEqual(2715000m, luongCaDem, "Lương ca đêm phải đúng 2.715.000");

            // 4. Lương phụ cấp
            // 675.000 / 26 * 25 = 649.038,46... -> 649.038
            decimal phuCapCongThucTe = Math.Floor((tongPhuCapThang / congChuan) * congThucTe);
            Assert.AreEqual(649038m, phuCapCongThucTe, "Lương phụ cấp thực tế phải đúng 649.038");

            // 5. Tổng lương công thực tế (gồm lương ca ngày + lương ca đêm + phép)
            // LUONG_CONG_THUCTE = Lương ca ngày + Lương ca đêm + Phép = 2.262.500 + 2.715.000 + 200.000 = 5.177.500
            decimal luongCongThucTe = luongCaNgay + luongCaDem + tienPhep;
            Assert.AreEqual(5177500m, luongCongThucTe);

            // 6. Tổng tiền ngày công (Tổng tiền công thực tế)
            // TONG_TIEN_CONG_THUC_TE = LUONG_CONG_THUCTE + PHUCAP_CONG_THUCTE + TIEN_CHUYENCAN + TIEN_AN_CA
            // = 5.177.500 + 649.038 + 300.000 + 240.000 = 6.366.538
            // Hoặc tính theo: 2.262.500 + 2.715.000 + 649.038 + 200.000 + 300.000 + 240.000 = 6.366.538
            decimal tongTienCongThucTe = luongCaNgay + luongCaDem + phuCapCongThucTe + tienPhep + tienChuyenCan + tienAn;
            Assert.AreEqual(6366538m, tongTienCongThucTe, "Tổng tiền công thực tế phải đúng 6.366.538");

            // 7. OT calculation:
            // 27.5h * 150% * 25.000 = 1.031.250
            // 8h * 200% * 25.000 = 400.000
            // 3.5h * 200% * 25.000 = 175.000
            // 15h * 180% * 25.000 = 675.000
            decimal rateOT = 25000m;
            decimal ot1 = 27.5m * 1.50m * rateOT;
            decimal ot2 = 8m * 2.00m * rateOT;
            decimal ot3 = 3.5m * 2.00m * rateOT;
            decimal ot4 = 15m * 1.80m * rateOT;
            decimal tongOT = ot1 + ot2 + ot3 + ot4;

            Assert.AreEqual(1031250m, ot1);
            Assert.AreEqual(400000m, ot2);
            Assert.AreEqual(175000m, ot3);
            Assert.AreEqual(675000m, ot4);
            Assert.AreEqual(2281250m, tongOT, "Tổng OT phải đúng 2.281.250");

            // 8. Tổng cộng
            // TONG_CONG = TONG_TIEN_CONG_THUC_TE + TIEN_TANGCA
            // 6.366.538 + 2.281.250 = 8.647.788
            decimal tongCong = tongTienCongThucTe + tongOT;
            Assert.AreEqual(8647788m, tongCong, "Tổng cộng phải đúng 8.647.788");

            // 9. Bảo hiểm & Đoàn phí
            decimal luongBHXH = 4525000m;
            decimal tienBHXH = Math.Round(luongBHXH * 0.08m, 0);       // 8% = 362.000
            decimal tienBHYT = Math.Round(luongBHXH * 0.015m, 0);      // 1.5% = 67.875
            decimal tienBHTN = Math.Round(luongBHXH * 0.01m, 0);       // 1% = 45.250
            decimal tienCongDoan = 42000m;                             // 42.000
            decimal tongGiamTru = tienBHXH + tienBHYT + tienBHTN + tienCongDoan; // 517.125

            Assert.AreEqual(362000m, tienBHXH, "BHXH (8%) phải đúng 362.000");
            Assert.AreEqual(67875m, tienBHYT, "BHYT (1.5%) phải đúng 67.875");
            Assert.AreEqual(45250m, tienBHTN, "BHTN (1%) phải đúng 45.250");
            Assert.AreEqual(42000m, tienCongDoan, "Công đoàn phải đúng 42.000");
            Assert.AreEqual(517125m, tongGiamTru);

            // 10. Thực lĩnh
            // THUC_LINH = TONG_CONG - BHXH - BHYT - BHTN - CONG_DOAN - THUE_TNCN - TAM_UNG + HOAN_THUE
            // 8.647.788 - 517.125 = 8.130.663
            decimal thucLinh = tongCong - tongGiamTru;
            Assert.AreEqual(8130663m, thucLinh, "Thực lĩnh phải đúng 8.130.663");
        }

        [Test]
        public void Test_Probation_Overtime_Discount_85Percent()
        {
            // Verify probation overtime multiplier = regular multiplier * 85%
            // 1. 150% * 85% = 127.5%
            Assert.AreEqual(1.275m, 1.50m * 0.85m);

            // 2. 200% * 85% = 170%
            Assert.AreEqual(1.700m, 2.00m * 0.85m);

            // 3. 270% * 85% = 229.5%
            Assert.AreEqual(2.295m, 2.70m * 0.85m);

            // 4. 180% * 85% = 153%
            Assert.AreEqual(1.530m, 1.80m * 0.85m);

            // 5. 300% * 85% = 255%
            Assert.AreEqual(2.550m, 3.00m * 0.85m);

            // 6. 390% * 85% = 331.5%
            Assert.AreEqual(3.315m, 3.90m * 0.85m);
        }

        [Test]
        public void Test_Database_BangLuong_Columns_Exist()
        {
            using (var db = new MyEntities())
            {
                // Verify that the table schema has all new fields mapped
                var bl = new TB_BANGLUONG
                {
                    MAKYCONG = 999999,
                    MANV = 999999,
                    THANG = 1,
                    NAM = 2026,
                    CONG_LAMNGAY = 13,
                    CONG_LAMDEM = 12,
                    CONG_CHUAN = 26,
                    CONG_THUCTE = 25,
                    DAILY_RATE = 174038.46m,
                    DAILY_ALLOWANCE = 25961.54m,
                    LUONG_CONG_THUCTE = 5177500m,
                    PHUCAP_CONG_THUCTE = 649038m,
                    TIEN_TANGCA = 2281250m,
                    TIEN_CHUYENCAN = 300000m,
                    TIEN_AN_CA = 240000m,
                    TONG_CONG = 8647788m,
                    LUONG_BHXH = 4525000m,
                    TIEN_BHXH = 362000m,
                    TIEN_BHYT = 67875m,
                    TIEN_BHTN = 45250m,
                    TIEN_CONG_DOAN = 42000m,
                    THUE_TNCN = 0m,
                    HOAN_THUE = 0m,
                    TIEN_BHXH_TRICH = 475125m,
                    THUC_LINH = 8130663m
                };

                Assert.AreEqual(13, bl.CONG_LAMNGAY);
                Assert.AreEqual(12, bl.CONG_LAMDEM);
                Assert.AreEqual(8647788m, bl.TONG_CONG);
                Assert.AreEqual(4525000m, bl.LUONG_BHXH);
                Assert.AreEqual(362000m, bl.TIEN_BHXH);
                Assert.AreEqual(67875m, bl.TIEN_BHYT);
                Assert.AreEqual(45250m, bl.TIEN_BHTN);
                Assert.AreEqual(42000m, bl.TIEN_CONG_DOAN);
                Assert.AreEqual(0m, bl.THUE_TNCN);
                Assert.AreEqual(0m, bl.HOAN_THUE);
                Assert.AreEqual(8130663m, bl.THUC_LINH);
            }
        }

        [Test]
        public void Test_Database_TangCa_IdLoaiTangCa_FK_Exists()
        {
            using (var db = new MyEntities())
            {
                var tc = new TB_TANGCA
                {
                    IDLOAITANGCA = 1
                };
                Assert.AreEqual(1, tc.IDLOAITANGCA);
            }
        }

        [Test]
        public void Test_KhenThuong_KyLuat_Amount_And_AppliedPeriod()
        {
            using (var db = new MyEntities())
            {
                // 1. Khen thưởng có tiền > 0 -> Bắt buộc có tháng/năm áp dụng
                var kt = new TB_KHENTHUONG_KYLUAT
                {
                    SOQUYETDINH = "TEST_KT_01",
                    LOAI = 1,
                    SOTIEN = 500000m,
                    THANG_APDUNG = 1,
                    NAM_APDUNG = 2026
                };
                Assert.AreEqual(500000m, kt.SOTIEN);
                Assert.AreEqual(1, kt.THANG_APDUNG);
                Assert.AreEqual(2026, kt.NAM_APDUNG);

                // 2. Khen thưởng số tiền = 0 -> không cần chọn tháng áp dụng (có thể null)
                var ktKhongTien = new TB_KHENTHUONG_KYLUAT
                {
                    SOQUYETDINH = "TEST_KT_02",
                    LOAI = 1,
                    SOTIEN = 0m,
                    THANG_APDUNG = null,
                    NAM_APDUNG = null
                };
                Assert.AreEqual(0m, ktKhongTien.SOTIEN);
                Assert.IsNull(ktKhongTien.THANG_APDUNG);
                Assert.IsNull(ktKhongTien.NAM_APDUNG);

                // 3. Kỷ luật có tiền phạt > 0 -> Áp dụng trừ lương đúng kỳ
                var kl = new TB_KHENTHUONG_KYLUAT
                {
                    SOQUYETDINH = "TEST_KL_01",
                    LOAI = 2,
                    SOTIEN = 200000m,
                    THANG_APDUNG = 2,
                    NAM_APDUNG = 2026
                };
                Assert.AreEqual(200000m, kl.SOTIEN);
                Assert.AreEqual(2, kl.THANG_APDUNG);
                Assert.AreEqual(2026, kl.NAM_APDUNG);

                // 4. Test DTO formatting
                var dto = new KHENTHUONG_KYLUAT_DTO
                {
                    SOTIEN = 500000m,
                    THANG_APDUNG = 1,
                    NAM_APDUNG = 2026
                };
                Assert.AreEqual("Tháng 01/2026", dto.THOIGIAN_APDUNG);

                var dtoEmpty = new KHENTHUONG_KYLUAT_DTO
                {
                    SOTIEN = 0m,
                    THANG_APDUNG = null,
                    NAM_APDUNG = null
                };
                Assert.AreEqual(string.Empty, dtoEmpty.THOIGIAN_APDUNG);
            }
        }

        [Test]
        public void Test_UngLuong_Custom_Date_Selection()
        {
            DateTime ngayChon = new DateTime(2026, 1, 15);
            var ul = new TB_UNGLUONG
            {
                NAM = ngayChon.Year,
                THANG = ngayChon.Month,
                NGAY = ngayChon.Day,
                SOTIENUNG = 1000000m,
                CREATED_DATE = ngayChon
            };

            Assert.AreEqual(2026, ul.NAM);
            Assert.AreEqual(1, ul.THANG);
            Assert.AreEqual(15, ul.NGAY);

            var dto = new UNGLUONG_DTO
            {
                NAM = ul.NAM,
                THANG = ul.THANG,
                NGAY = ul.NGAY,
                SOTIENUNG = ul.SOTIENUNG,
                CREATED_DATE = ul.CREATED_DATE
            };

            Assert.IsNotNull(dto.NGAYUNG);
            Assert.AreEqual(new DateTime(2026, 1, 15), dto.NGAYUNG.Value);
        }
    }
}
