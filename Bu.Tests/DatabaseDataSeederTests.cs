using Bu.CLASS_CHAMCONG;
using Bu.CLASS_NHANSU;
using DA;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class DatabaseDataSeederTests
    {
        [Test]
        public void Fix_All_Mojibake_In_Database()
        {
            using (var db = new MyEntities())
            {
                // 1. Chuẩn hóa TB_LOAICONG
                var loaiCongs = db.TB_LOAICONG.ToList();
                foreach (var lc in loaiCongs)
                {
                    if (lc.IDLOAICONG == 1) lc.TENLC = "Công ngày thường";
                    else if (lc.IDLOAICONG == 2) lc.TENLC = "Công Chủ nhật";
                    else if (lc.IDLOAICONG == 3) lc.TENLC = "Công ngày lễ";
                }

                // 2. Chuẩn hóa TB_LOAICA
                var loaiCas = db.TB_LOAICA.ToList();
                foreach (var ca in loaiCas)
                {
                    if (ca.IDLOAICA == 1) ca.TENLOAICA = "Ca ngày";
                    else if (ca.IDLOAICA == 2) ca.TENLOAICA = "Ca đêm";
                }

                // 3. Chuẩn hóa TB_LOAIHOPDONG
                var loaiHds = db.TB_LOAIHOPDONG.ToList();
                foreach (var hd in loaiHds)
                {
                    if (hd.LOAIHD == 1) hd.TENLOAIHD = "Hợp đồng thử việc";
                    else if (hd.LOAIHD == 2) hd.TENLOAIHD = "Hợp đồng lao động xác định thời hạn";
                    else if (hd.LOAIHD == 3) hd.TENLOAIHD = "Hợp đồng lao động không xác định thời hạn";
                }

                db.SaveChanges();

                // 4. Chuẩn hóa TB_HESO_TANGCA bằng câu lệnh Update thuần với tham số Unicode
                var rules = new[]
                {
                    new { Id = 1, Ten = "Tăng ca ca ngày 06:00–08:00", LoaiCong = "Công ngày thường", IdLoaiCong = 1, LoaiCa = "Ca ngày", IdLoaiCa = (int?)1, Start = "06:00", End = "08:00", HsCt = 1.50, HsTv = 1.275, ThuTu = 1, GhiChu = "Hệ số 150%, thử việc 85% = 127.5%" },
                    new { Id = 2, Ten = "Tăng ca ca ngày 17:00–22:00", LoaiCong = "Công ngày thường", IdLoaiCong = 1, LoaiCa = "Ca ngày", IdLoaiCa = (int?)1, Start = "17:00", End = "22:00", HsCt = 1.50, HsTv = 1.275, ThuTu = 2, GhiChu = "Hệ số 150%, thử việc 85% = 127.5%" },
                    new { Id = 3, Ten = "Tăng ca ca ngày sau 22:00–06:00", LoaiCong = "Công ngày thường", IdLoaiCong = 1, LoaiCa = "Ca ngày", IdLoaiCa = (int?)1, Start = "22:00", End = "06:00", HsCt = 2.00, HsTv = 1.700, ThuTu = 3, GhiChu = "Hệ số 200%, thử việc 85% = 170%" },
                    new { Id = 4, Ten = "Tăng ca Chủ nhật 08:00–22:00", LoaiCong = "Công Chủ nhật", IdLoaiCong = 2, LoaiCa = "Ca ngày", IdLoaiCa = (int?)1, Start = "08:00", End = "22:00", HsCt = 2.00, HsTv = 1.700, ThuTu = 4, GhiChu = "Hệ số 200%, thử việc 85% = 170%" },
                    new { Id = 5, Ten = "Tăng ca Chủ nhật sau 22:00–06:00", LoaiCong = "Công Chủ nhật", IdLoaiCong = 2, LoaiCa = (string)null, IdLoaiCa = (int?)null, Start = "22:00", End = "06:00", HsCt = 2.70, HsTv = 2.295, ThuTu = 5, GhiChu = "Hệ số 270%, thử việc 85% = 229.5%" },
                    new { Id = 6, Ten = "Tăng ca ca đêm 05:30–06:00", LoaiCong = "Công ngày thường", IdLoaiCong = 1, LoaiCa = "Ca đêm", IdLoaiCa = (int?)2, Start = "05:30", End = "06:00", HsCt = 2.00, HsTv = 1.700, ThuTu = 6, GhiChu = "Hệ số 200%, thử việc 85% = 170%" },
                    new { Id = 7, Ten = "Tăng ca ca đêm 06:00–08:00", LoaiCong = "Công ngày thường", IdLoaiCong = 1, LoaiCa = "Ca đêm", IdLoaiCa = (int?)2, Start = "06:00", End = "08:00", HsCt = 1.80, HsTv = 1.530, ThuTu = 7, GhiChu = "Hệ số 180%, thử việc 85% = 153%" },
                    new { Id = 8, Ten = "Làm ca đêm Chủ nhật 20:00–08:00", LoaiCong = "Công Chủ nhật", IdLoaiCong = 2, LoaiCa = "Ca đêm", IdLoaiCa = (int?)2, Start = "20:00", End = "08:00", HsCt = 2.70, HsTv = 2.295, ThuTu = 8, GhiChu = "Hệ số 270%, thử việc 85% = 229.5%" },
                    new { Id = 9, Ten = "Tăng ca ngày lễ 08:00–22:00", LoaiCong = "Công ngày lễ", IdLoaiCong = 3, LoaiCa = (string)null, IdLoaiCa = (int?)null, Start = "08:00", End = "22:00", HsCt = 3.00, HsTv = 2.550, ThuTu = 9, GhiChu = "Hệ số 300%, thử việc 85% = 255%" },
                    new { Id = 10, Ten = "Tăng ca ngày lễ sau 22:00", LoaiCong = "Công ngày lễ", IdLoaiCong = 3, LoaiCa = (string)null, IdLoaiCa = (int?)null, Start = "22:00", End = "06:00", HsCt = 3.90, HsTv = 3.315, ThuTu = 10, GhiChu = "Hệ số 390%, thử việc 85% = 331.5%" }
                };

                foreach (var r in rules)
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE TB_HESO_TANGCA SET TEN_QUYDINH = :p0, LOAICONG = :p1, LOAICA = :p2, GHICHU = :p3 WHERE ID = :p4",
                        r.Ten, r.LoaiCong, r.LoaiCa, r.GhiChu, r.Id
                    );
                }

                // Xác thực lại
                var heSoBus = new HESO_TANGCA();
                var checkRules = heSoBus.GetList();
                Assert.AreEqual(10, checkRules.Count);
                foreach (var rule in checkRules)
                {
                    Assert.IsFalse(rule.TEN_QUYDINH.Contains("Ä") || rule.TEN_QUYDINH.Contains("Ã"), $"Vẫn còn mojibake trong {rule.TEN_QUYDINH}");
                    Console.WriteLine($"[OK] {rule.ID}: {rule.TEN_QUYDINH} | {rule.LOAICONG} | {rule.LOAICA}");
                }
            }
        }

        [Test]
        public void Seed_Realistic_TestData_For_Objective_Testing()
        {
            using (var db = new MyEntities())
            {
                // Lấy danh sách nhân viên đang làm việc để chèn dữ liệu test
                var employees = db.TB_NHANVIEN
                    .Where(x => (x.DATHOIVIEC == null || x.DATHOIVIEC == 0) && x.DELETED_DATE == null)
                    .OrderBy(x => x.MANV)
                    .Take(20)
                    .ToList();

                Assert.IsTrue(employees.Count >= 5, "Cần có ít nhất 5 nhân viên để seed dữ liệu test");

                var ktBus = new KHENTHUONG_KYLUAT();
                var ulBus = new UNGLUONG();
                var tcBus = new TANGCA();

                // Xóa bỏ các mã test không chuẩn trước đó nếu có
                db.Database.ExecuteSqlCommand("DELETE FROM TB_KHENTHUONG_KYLUAT WHERE SOQUYETDINH LIKE 'QD-KT-TEST%' OR SOQUYETDINH LIKE 'QD-KL-TEST%'");

                // 1. SEED KHEN THƯỞNG & KỶ LUẬT (Đủ các tình huống nghiệp vụ, tuân thủ đúng định dạng 00000/NAM/QĐKT)
                // - Khen thưởng có tiền + có tháng áp dụng
                // - Khen thưởng không có tiền (số tiền = 0, không cần tháng áp dụng)
                // - Kỷ luật có phạt tiền + có tháng áp dụng trừ lương
                // - Kỷ luật khiển trách không trừ tiền (số tiền = 0, không cần tháng áp dụng)
                var testKtKlData = new[]
                {
                    new { MaNv = employees[0].MANV, Loai = 1, SoQd = "00015/2026/QĐKT", LyDo = "Hoàn thành xuất sắc dự án quý 1", NoiDung = "Khen thưởng cán bộ xuất sắc", SoTien = 2000000m, Thang = (decimal?)1, Nam = (decimal?)2026 },
                    new { MaNv = employees[1].MANV, Loai = 1, SoQd = "00016/2026/QĐKT", LyDo = "Đạt thành tích nhân viên gương mẫu", NoiDung = "Thưởng thành tích phong trào", SoTien = 1000000m, Thang = (decimal?)1, Nam = (decimal?)2026 },
                    new { MaNv = employees[2].MANV, Loai = 1, SoQd = "00017/2026/QĐKT", LyDo = "Được khen tặng Bằng khen cấp Công ty", NoiDung = "Trao bằng khen lưu niệm (khen thưởng tinh thần)", SoTien = 0m, Thang = (decimal?)null, Nam = (decimal?)null },
                    new { MaNv = employees[3].MANV, Loai = 1, SoQd = "00018/2026/QĐKT", LyDo = "Sáng kiến cải tiến kỹ thuật quy trình sản xuất", NoiDung = "Thưởng sáng kiến kỹ thuật", SoTien = 1500000m, Thang = (decimal?)2, Nam = (decimal?)2026 },
                    new { MaNv = employees[0].MANV, Loai = 2, SoQd = "00015/2026/QĐKL", LyDo = "Đi muộn quá 3 lần không lý do trong tháng", NoiDung = "Phạt trừ lương vi phạm nội quy", SoTien = 300000m, Thang = (decimal?)1, Nam = (decimal?)2026 },
                    new { MaNv = employees[4].MANV, Loai = 2, SoQd = "00016/2026/QĐKL", LyDo = "Không tuân thủ quy định an toàn lao động", NoiDung = "Phạt vi phạm quy trình bảo hộ", SoTien = 500000m, Thang = (decimal?)2, Nam = (decimal?)2026 },
                    new { MaNv = employees[1].MANV, Loai = 2, SoQd = "00017/2026/QĐKL", LyDo = "Nhắc nhở về tác phong văn phòng", NoiDung = "Khiển trách bằng văn bản (không phạt tiền)", SoTien = 0m, Thang = (decimal?)null, Nam = (decimal?)null }
                };

                foreach (var item in testKtKlData)
                {
                    var existing = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == item.SoQd);
                    if (existing != null)
                    {
                        existing.MANV = item.MaNv;
                        existing.LOAI = item.Loai;
                        existing.LYDO = item.LyDo;
                        existing.NOIDUNG = item.NoiDung;
                        existing.SOTIEN = item.SoTien;
                        existing.THANG_APDUNG = item.Thang;
                        existing.NAM_APDUNG = item.Nam;
                        existing.DELETED_BY = null;
                        existing.DELETED_DATE = null;
                    }
                    else
                    {
                        var newKt = new TB_KHENTHUONG_KYLUAT
                        {
                            SOQUYETDINH = item.SoQd,
                            MANV = item.MaNv,
                            LOAI = item.Loai,
                            NGAY = new DateTime(2026, 1, 15),
                            TUNGAY = new DateTime(2026, 1, 1),
                            DENNGAY = new DateTime(2026, 1, 31),
                            LYDO = item.LyDo,
                            NOIDUNG = item.NoiDung,
                            SOTIEN = item.SoTien,
                            THANG_APDUNG = item.Thang,
                            NAM_APDUNG = item.Nam,
                            CREATED_DATE = DateTime.Now
                        };
                        db.TB_KHENTHUONG_KYLUAT.Add(newKt);
                    }
                }
                db.SaveChanges();
                Console.WriteLine("Seed KhenThuong/KyLuat test data: OK");

                // 2. SEED ỨNG LƯƠNG (Chọn ngày linh hoạt: ngày 05, 10, 15, 20...)
                var testUngLuongData = new[]
                {
                    new { MaNv = employees[0].MANV, Ngay = 5, Thang = 1, Nam = 2026, SoTien = 1500000m, GhiChu = "Tạm ứng chi tiêu gia đình đầu tháng" },
                    new { MaNv = employees[1].MANV, Ngay = 12, Thang = 1, Nam = 2026, SoTien = 2000000m, GhiChu = "Tạm ứng tiền khám chữa bệnh" },
                    new { MaNv = employees[2].MANV, Ngay = 20, Thang = 1, Nam = 2026, SoTien = 1000000m, GhiChu = "Tạm ứng tiền xe về quê" },
                    new { MaNv = employees[3].MANV, Ngay = 8, Thang = 2, Nam = 2026, SoTien = 3000000m, GhiChu = "Tạm ứng chi phí chuẩn bị Tết" },
                    new { MaNv = employees[4].MANV, Ngay = 15, Thang = 2, Nam = 2026, SoTien = 1200000m, GhiChu = "Tạm ứng cá nhân" }
                };

                foreach (var ulItem in testUngLuongData)
                {
                    var existingUl = db.TB_UNGLUONG.FirstOrDefault(x => x.MANV == ulItem.MaNv && x.THANG == ulItem.Thang && x.NAM == ulItem.Nam && x.NGAY == ulItem.Ngay);
                    if (existingUl != null)
                    {
                        existingUl.SOTIENUNG = ulItem.SoTien;
                        existingUl.GHICHU = ulItem.GhiChu;
                        existingUl.DELETED_BY = null;
                        existingUl.DELETED_DATE = null;
                    }
                    else
                    {
                        var newUl = new TB_UNGLUONG
                        {
                            MANV = ulItem.MaNv,
                            NAM = ulItem.Nam,
                            THANG = ulItem.Thang,
                            NGAY = ulItem.Ngay,
                            SOTIENUNG = ulItem.SoTien,
                            GHICHU = ulItem.GhiChu,
                            CREATED_DATE = new DateTime(ulItem.Nam, ulItem.Thang, ulItem.Ngay)
                        };
                        db.TB_UNGLUONG.Add(newUl);
                    }
                }
                db.SaveChanges();
                Console.WriteLine("Seed UngLuong test data: OK");

                // 3. SEED TĂNG CA (Phủ nhiều khung giờ quy định: Ca ngày thường, Sau 22h, Chủ nhật, Đêm)
                var testTangCaData = new[]
                {
                    new { MaNv = employees[0].MANV, Nam = 2026, Thang = 1, Ngay = 10, GioVao = "17:00", GioRa = "21:00", IdLoaiCong = 1, IdLoaiCa = 1, IdQuyDinh = 2, GhiChu = "Tăng ca tiến độ xuất hàng" },
                    new { MaNv = employees[0].MANV, Nam = 2026, Thang = 1, Ngay = 18, GioVao = "08:00", GioRa = "17:00", IdLoaiCong = 2, IdLoaiCa = 1, IdQuyDinh = 4, GhiChu = "Tăng ca bảo trì hệ thống Chủ nhật" },
                    new { MaNv = employees[1].MANV, Nam = 2026, Thang = 1, Ngay = 12, GioVao = "06:00", GioRa = "08:00", IdLoaiCong = 1, IdLoaiCa = 2, IdQuyDinh = 7, GhiChu = "Làm thêm ca đêm bàn giao ca sáng" },
                    new { MaNv = employees[2].MANV, Nam = 2026, Thang = 1, Ngay = 15, GioVao = "22:00", GioRa = "02:00", IdLoaiCong = 1, IdLoaiCa = 1, IdQuyDinh = 3, GhiChu = "Xử lý sự cố máy chủ đêm" },
                    new { MaNv = employees[3].MANV, Nam = 2026, Thang = 1, Ngay = 25, GioVao = "20:00", GioRa = "04:00", IdLoaiCong = 2, IdLoaiCa = 2, IdQuyDinh = 8, GhiChu = "Trực đêm Chủ nhật kiểm kê kho" }
                };

                foreach (var tcItem in testTangCaData)
                {
                    var existingTc = db.TB_TANGCA.FirstOrDefault(x => x.MANV == tcItem.MaNv && x.NAM == tcItem.Nam && x.THANG == tcItem.Thang && x.NGAY == tcItem.Ngay);
                    double soGioDouble = tcBus.TinhSoGio(tcItem.GioVao, tcItem.GioRa);
                    decimal soGio = (decimal)soGioDouble;
                    var rule = db.Database.SqlQuery<TB_HESO_TANGCA>("SELECT * FROM TB_HESO_TANGCA WHERE ID = :p0", tcItem.IdQuyDinh).FirstOrDefault();
                    decimal heSo = rule != null ? rule.HESO_CHINHTHUC : 1.5m;
                    decimal donGia = tcBus.GetMucLuong1GioOT((int)tcItem.MaNv, tcItem.Nam, tcItem.Thang);
                    decimal thanhTien = soGio * donGia * heSo;

                    if (existingTc != null)
                    {
                        existingTc.GIOBATDAU = tcItem.GioVao;
                        existingTc.GIOKETTHUC = tcItem.GioRa;
                        existingTc.SOGIO = soGio;
                        existingTc.IDLOAICONG = tcItem.IdLoaiCong;
                        existingTc.IDLOAICA = tcItem.IdLoaiCa;
                        existingTc.IDLOAITANGCA = tcItem.IdQuyDinh;
                        existingTc.HESOTC = heSo;
                        existingTc.DONGIATC = donGia;
                        existingTc.SOTIENTC = thanhTien;
                        existingTc.GHICHU = tcItem.GhiChu;
                        existingTc.DELETED_BY = null;
                        existingTc.DELETED_DATE = null;
                    }
                    else
                    {
                        var newTc = new TB_TANGCA
                        {
                            MANV = tcItem.MaNv,
                            NAM = tcItem.Nam,
                            THANG = tcItem.Thang,
                            NGAY = tcItem.Ngay,
                            GIOBATDAU = tcItem.GioVao,
                            GIOKETTHUC = tcItem.GioRa,
                            SOGIO = soGio,
                            IDLOAICONG = tcItem.IdLoaiCong,
                            IDLOAICA = tcItem.IdLoaiCa,
                            IDLOAITANGCA = tcItem.IdQuyDinh,
                            HESOTC = heSo,
                            DONGIATC = donGia,
                            SOTIENTC = thanhTien,
                            GHICHU = tcItem.GhiChu,
                            CREATED_DATE = DateTime.Now
                        };
                        db.TB_TANGCA.Add(newTc);
                    }
                }
                db.SaveChanges();
                Console.WriteLine("Seed TangCa test data: OK");
            }
        }

        [Test]
        public void Apply_Migration_V1_12_Mobile_User_Employee_Link()
        {
            using (var db = new MyEntities())
            {
                // 1. Add MANV column to TB_SYS_USER if missing
                try
                {
                    int manvCol = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM user_tab_cols WHERE table_name = 'TB_SYS_USER' AND column_name = 'MANV'"
                    ).FirstOrDefault();
                    if (manvCol == 0)
                    {
                        db.Database.ExecuteSqlCommand("ALTER TABLE TB_SYS_USER ADD (MANV NUMBER)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MANV col error: {ex.Message}");
                }

                // 2. Add CLIENT_TYPE column to TB_SYS_USER if missing
                try
                {
                    int clientTypeCol = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM user_tab_cols WHERE table_name = 'TB_SYS_USER' AND column_name = 'CLIENT_TYPE'"
                    ).FirstOrDefault();
                    if (clientTypeCol == 0)
                    {
                        db.Database.ExecuteSqlCommand("ALTER TABLE TB_SYS_USER ADD (CLIENT_TYPE NVARCHAR2(20) DEFAULT 'ALL')");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CLIENT_TYPE col error: {ex.Message}");
                }

                // 3. Link an active employee to test user 'nhansu'
                try
                {
                    var firstEmp = db.TB_NHANVIEN.FirstOrDefault(n => (n.DATHOIVIEC ?? 0) != 1);
                    if (firstEmp != null)
                    {
                        var testUser = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME == "nhansu");
                        if (testUser != null)
                        {
                            testUser.MANV = firstEmp.MANV;
                            testUser.CLIENT_TYPE = "ALL";
                            db.SaveChanges();
                            Console.WriteLine($"Linked user 'nhansu' to MANV {firstEmp.MANV} ({firstEmp.HOTEN})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Link employee error: {ex.Message}");
                }
            }
        }
    }
}
