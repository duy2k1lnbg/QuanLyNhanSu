using Bu;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_NHANSU;
using DA;
using NUnit.Framework;
using System;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class LoaiHopDongAndNgayLeTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            using (var db = new MyEntities())
            {
                var hd1 = db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == 1);
                if (hd1 != null) hd1.TENLOAIHD = "Hợp đồng thử việc";
                var hd2 = db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == 2);
                if (hd2 != null) hd2.TENLOAIHD = "Hợp đồng lao động xác định thời hạn";
                var hd3 = db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == 3);
                if (hd3 != null) hd3.TENLOAIHD = "Hợp đồng lao động không xác định thời hạn";

                var nl1 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 1);
                if (nl1 != null) nl1.TENLE = "Tết Dương lịch";
                var nl2 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 2);
                if (nl2 != null) nl2.TENLE = "Tết Âm lịch (29 Tết)";
                var nl3 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 3);
                if (nl3 != null) nl3.TENLE = "Tết Âm lịch (Mùng 1)";
                var nl4 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 4);
                if (nl4 != null) nl4.TENLE = "Tết Âm lịch (Mùng 2)";
                var nl5 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 5);
                if (nl5 != null) nl5.TENLE = "Tết Âm lịch (Mùng 3)";
                var nl6 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 6);
                if (nl6 != null) nl6.TENLE = "Tết Âm lịch (Mùng 4)";
                var nl7 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 7);
                if (nl7 != null) nl7.TENLE = "Giỗ tổ Hùng Vương";
                var nl8 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 8);
                if (nl8 != null) nl8.TENLE = "Ngày Chiến thắng (30/4)";
                var nl9 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 9);
                if (nl9 != null) nl9.TENLE = "Ngày Quốc tế Lao động (01/5)";
                var nl10 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 10);
                if (nl10 != null) nl10.TENLE = "Quốc khánh (02/9)";
                var nl11 = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == 11);
                if (nl11 != null) nl11.TENLE = "Nghỉ liền kề Quốc khánh";

                db.SaveChanges();
            }
        }

        [Test]
        public void Test_TB_LOAIHOPDONG_ContainsThreeStandardTypes()
        {
            LOAIHOPDONG loaiHdBus = new LOAIHOPDONG();
            var list = loaiHdBus.getList();

            Assert.IsNotNull(list, "Danh sách loại hợp đồng không được null");
            Assert.IsTrue(list.Count >= 3, "Danh mục loại hợp đồng phải có ít nhất 3 loại chuẩn");

            var tv = list.FirstOrDefault(x => x.LOAIHD == 1);
            Assert.IsNotNull(tv, "Thiếu Hợp đồng thử việc (LOAIHD = 1)");
            StringAssert.Contains("thử việc", tv.TENLOAIHD.ToLower());

            var xd = list.FirstOrDefault(x => x.LOAIHD == 2);
            Assert.IsNotNull(xd, "Thiếu Hợp đồng xác định thời hạn (LOAIHD = 2)");

            var kxd = list.FirstOrDefault(x => x.LOAIHD == 3);
            Assert.IsNotNull(kxd, "Thiếu Hợp đồng không xác định thời hạn (LOAIHD = 3)");
        }

        [Test]
        public void Test_LOAIHOPDONG_AddUpdateDelete()
        {
            LOAIHOPDONG loaiHdBus = new LOAIHOPDONG();

            // Thêm mới
            var newItem = new TB_LOAIHOPDONG
            {
                TENLOAIHD = "Hợp đồng thử nghiệm Unit Test",
                CREATED_BY = 9999
            };
            var added = loaiHdBus.Add(newItem);
            Assert.IsTrue(added.LOAIHD > 0);

            // Sửa
            added.TENLOAIHD = "Hợp đồng thử nghiệm Đã Sửa";
            added.UPDATED_BY = 9999;
            var updated = loaiHdBus.Update(added);
            Assert.AreEqual("Hợp đồng thử nghiệm Đã Sửa", updated.TENLOAIHD);

            // Soft delete
            loaiHdBus.Delete((int)added.LOAIHD, 9999);
            var itemAfterDel = loaiHdBus.getItem((int)added.LOAIHD);
            Assert.IsNotNull(itemAfterDel.DELETED_BY, "DELETED_BY phải được gán khi xóa mềm");

            // Hard cleanup test data
            using (var db = new MyEntities())
            {
                var del = db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == added.LOAIHD);
                if (del != null)
                {
                    db.TB_LOAIHOPDONG.Remove(del);
                    db.SaveChanges();
                }
            }
        }

        [Test]
        public void Test_TB_NGAYLE_SeededHolidaysExist()
        {
            NGAYLE ngayLeBus = new NGAYLE();
            var list2026 = ngayLeBus.getListByNam(2026);

            Assert.IsNotNull(list2026);
            Assert.IsTrue(list2026.Count >= 5, "Năm 2026 phải có danh sách ngày lễ");

            // 01/01/2026 Tết Dương Lịch
            var tetDL = ngayLeBus.KiemTraNgayLe(new DateTime(2026, 1, 1));
            Assert.IsNotNull(tetDL, "Phải nhận diện ngày 01/01/2026 là ngày lễ");
            StringAssert.Contains("Tết Dương lịch", tetDL.TENLE);

            // 02/09/2026 Quốc khánh
            var quocKhanh = ngayLeBus.KiemTraNgayLe(new DateTime(2026, 9, 2));
            Assert.IsNotNull(quocKhanh, "Phải nhận diện ngày 02/09/2026 là ngày lễ");
        }

        [Test]
        public void Test_XacDinhLoaiCong_HolidaySundayWeekdayPriority()
        {
            NGAYLE ngayLeBus = new NGAYLE();

            // 1. Ngày 01/01/2026 là Tết Dương lịch (thứ Năm) -> Ngày Lễ = 3
            DateTime tetDL = new DateTime(2026, 1, 1);
            int loaiCongTet = ngayLeBus.XacDinhLoaiCong(tetDL);
            Assert.AreEqual(3, loaiCongTet, "Ngày lễ phải xác định LOAICONG = 3");

            // 2. Ngày 26/04/2026 là Giỗ tổ Hùng Vương rơi vào Chủ nhật -> Vẫn phải ưu tiên Ngày Lễ (3)
            DateTime gioTo = new DateTime(2026, 4, 26);
            Assert.AreEqual(DayOfWeek.Sunday, gioTo.DayOfWeek);
            int loaiCongGioTo = ngayLeBus.XacDinhLoaiCong(gioTo);
            Assert.AreEqual(3, loaiCongGioTo, "Ngày lễ rơi vào Chủ nhật phải ưu tiên là Ngày lễ (3)");

            // 3. Ngày Chủ nhật bình thường (không phải lễ) -> 2 (Công Chủ nhật)
            // Ví dụ: 11/01/2026 là Chủ nhật
            DateTime cnBinhThuong = new DateTime(2026, 1, 11);
            Assert.AreEqual(DayOfWeek.Sunday, cnBinhThuong.DayOfWeek);
            Assert.IsNull(ngayLeBus.KiemTraNgayLe(cnBinhThuong), "11/01/2026 không phải là ngày lễ");
            int loaiCongCN = ngayLeBus.XacDinhLoaiCong(cnBinhThuong);
            Assert.AreEqual(2, loaiCongCN, "Chủ nhật không phải lễ phải xác định LOAICONG = 2");

            // 4. Ngày thường bình thường (không phải lễ, không phải CN) -> 1 (Công ngày thường)
            // Ví dụ: 12/01/2026 là Thứ hai
            DateTime thuHai = new DateTime(2026, 1, 12);
            Assert.AreEqual(DayOfWeek.Monday, thuHai.DayOfWeek);
            int loaiCongThuHai = ngayLeBus.XacDinhLoaiCong(thuHai);
            Assert.AreEqual(1, loaiCongThuHai, "Ngày thường không phải lễ phải xác định LOAICONG = 1");
        }

        [Test]
        public void Test_NGAYLE_DuplicateDatePrevention()
        {
            NGAYLE ngayLeBus = new NGAYLE();
            DateTime existingHoliday = new DateTime(2026, 1, 1);

            // Kiểm tra trùng ngày
            bool isDuplicate = ngayLeBus.KiemTraTrungNgay(existingHoliday);
            Assert.IsTrue(isDuplicate, "KiemTraTrungNgay phải trả về true khi ngày đã tồn tại");

            // Cố thêm trùng ngày phải ném Exception
            Assert.Throws<Exception>(() =>
            {
                ngayLeBus.Add(new TB_NGAYLE
                {
                    TENLE = "Tết Dương Lịch Trùng Lặp",
                    NGAY = existingHoliday,
                    NAM = 2026,
                    HESO = 3.0m
                });
            });
        }

        [Test]
        public void Test_ProbationCheck_WithLoaiHD()
        {
            TANGCA tangCaBus = new TANGCA();

            // Thử nghiệm tìm nhân viên có hợp đồng
            using (var db = new MyEntities())
            {
                var hdThuViec = db.TB_HOPDONG.FirstOrDefault(x => x.LOAIHD == 1);
                if (hdThuViec != null && hdThuViec.MANV.HasValue)
                {
                    bool isThuViec = tangCaBus.KiemTraThuViec((int)hdThuViec.MANV.Value);
                    Assert.IsTrue(isThuViec, "Nhân viên có LOAIHD = 1 phải được xác định là Thử việc");
                }
            }
        }
    }
}
