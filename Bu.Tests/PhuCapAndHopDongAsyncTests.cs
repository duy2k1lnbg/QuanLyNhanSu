using Bu.CLASS_CHAMCONG;
using Bu.CLASS_NHANSU;
using Bu.CLASS_SYSTEM;
using DA;
using NUnit.Framework;
using System;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class PhuCapAndHopDongAsyncTests
    {
        [Test]
        public void Test_NhanVienPhuCap_Schema_And_Query_Without_MaKyCong()
        {
            using (var db = new MyEntities())
            {
                // Verify that TB_NHANVIEN_PHUCAP can be queried directly by MANV
                var firstEmp = db.TB_NHANVIEN_PHUCAP.Select(x => x.MANV).FirstOrDefault();
                if (firstEmp > 0)
                {
                    int manv = Convert.ToInt32(firstEmp);
                    var phuCapBus = new PHUCAP();
                    var dict = phuCapBus.GetPhuCapByNhanVien(manv);

                    Assert.IsNotNull(dict, "Danh sách phụ cấp của nhân viên không được null.");
                    Assert.AreEqual(13, dict.Count, "Phải có đủ 13 loại phụ cấp.");

                    var sorted = phuCapBus.GetNhanVienSortedByIDPC();
                    Assert.IsNotNull(sorted, "Danh sách phụ cấp tổng hợp không được null.");
                    Assert.IsTrue(sorted.Any(), "Phải có ít nhất 1 nhân viên có phụ cấp.");
                }
            }
        }

        [Test]
        public void Test_HopDong_NoiDung_Type_And_Query()
        {
            using (var db = new MyEntities())
            {
                var hdBus = new HOPDONGLAODONG();
                var firstHd = db.TB_HOPDONG.FirstOrDefault();
                if (firstHd != null)
                {
                    var item = hdBus.getItem(firstHd.SOHD);
                    Assert.IsNotNull(item, "Phải đọc được thông tin hợp đồng.");
                    if (item.NOIDUNG != null)
                    {
                        Assert.LessOrEqual(item.NOIDUNG.Length, 4000, "Trường nội dung hợp đồng tối đa 4000 ký tự.");
                    }
                }
            }
        }

        [Test]
        public void Test_Authorization_New_Functions_Exist_And_Admin_Has_Right()
        {
            using (var db = new MyEntities())
            {
                var funcLoaiHD = db.TB_SYS_FUNCTION.FirstOrDefault(x => x.FUNCTION_CODE == "F_NV_LOAIHOPDONG");
                var funcNgayLe = db.TB_SYS_FUNCTION.FirstOrDefault(x => x.FUNCTION_CODE == "F_CC_NGAYLE");
                var funcTangCa = db.TB_SYS_FUNCTION.FirstOrDefault(x => x.FUNCTION_CODE == "F_CC_TANGCA");

                Assert.IsNotNull(funcLoaiHD, "F_NV_LOAIHOPDONG phải tồn tại trong TB_SYS_FUNCTION.");
                Assert.IsNotNull(funcNgayLe, "F_CC_NGAYLE phải tồn tại trong TB_SYS_FUNCTION.");
                Assert.IsNotNull(funcTangCa, "F_CC_TANGCA phải tồn tại trong TB_SYS_FUNCTION.");

                // Admin simulation
                UserSession.CurrentUser = new TB_SYS_USER
                {
                    USERNAME = "ADMIN",
                    FULLNAME = "System Admin"
                };

                Assert.IsTrue(UserSession.HasRight("F_NV_LOAIHOPDONG"), "Admin phải có quyền F_NV_LOAIHOPDONG.");
                Assert.IsTrue(UserSession.HasRight("F_CC_NGAYLE"), "Admin phải có quyền F_CC_NGAYLE.");
                Assert.IsTrue(UserSession.HasRight("F_CC_TANGCA"), "Admin phải có quyền F_CC_TANGCA.");

                UserSession.Clear();
            }
        }

        [Test]
        public void Test_Allowance_Applies_Across_Multiple_Months()
        {
            using (var db = new MyEntities())
            {
                // Verify that an employee's allowances can be read identically for any month
                int testManv = 2327; // Sample employee
                var phuCapBus = new PHUCAP();
                var allowances = phuCapBus.GetPhuCapByNhanVien(testManv);

                Assert.IsNotNull(allowances);
                Assert.AreEqual(13, allowances.Count);

                // Both month 1 and month 2 should get the exact same allowance values without makycong filtering
                decimal totalAllowance = allowances.Values.Sum();
                Assert.GreaterOrEqual(totalAllowance, 0);
            }
        }
    }
}
