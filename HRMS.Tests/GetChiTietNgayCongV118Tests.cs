using System;
using Bu.CLASS_CHAMCONG;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class GetChiTietNgayCongV118Tests
    {
        [Test]
        public void GetChiTietNgayCongV118_202601_Manv2_Ngay5_ExecutesWithoutOraError()
        {
            var bcctBus = new BANGCONG_NV_CHITIET();
            var res = bcctBus.GetChiTietNgayCongV118(202601, 2, 5);
            Assert.IsNotNull(res, "Kết quả trả về không được null");
            Assert.AreEqual(2, (int)res.MaNV);
            Assert.AreEqual(202601, (int)res.MaKyCong);
            Console.WriteLine($"Status: {res.TrangThaiCong}, DuDieuKienChot: {res.DuDieuKienChot}, BatThuongCount: {res.BatThuongList.Count}");
        }
    }
}
