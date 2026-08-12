using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bu.DTO;

namespace Bu.CLASS_CHAMCONG
{
    public class BANGLUONG
    {
        MyEntities db = new MyEntities();

        public List<BANGLUONG_DTO> getList(int makycong)
        {
            var query = from bl in db.TB_BANGLUONG
                        where bl.MAKYCONG == makycong
                        join nv in db.TB_NHANVIEN on bl.MANV equals nv.MANV into nvGroup
                        from nv in nvGroup.DefaultIfEmpty()
                        select new BANGLUONG_DTO
                        {
                            IDBL = bl.IDBL,
                            MANV = bl.MANV,
                            HOTEN = nv != null ? nv.HOTEN : "",
                            MAKYCONG = bl.MAKYCONG,
                            THANG = bl.THANG,
                            NAM = bl.NAM,
                            CONG_CHUAN = bl.CONG_CHUAN,
                            CONG_THUCTE = bl.CONG_THUCTE,
                            CONG_LAMDEM = bl.CONG_LAMDEM,
                            DAILY_RATE = bl.DAILY_RATE,
                            DAILY_ALLOWANCE = bl.DAILY_ALLOWANCE,
                            LUONG_CONG_THUCTE = bl.LUONG_CONG_THUCTE,
                            PHUCAP_CONG_THUCTE = bl.PHUCAP_CONG_THUCTE,
                            TIEN_TANGCA = bl.TIEN_TANGCA,
                            TIEN_CHUYENCAN = bl.TIEN_CHUYENCAN,
                            TIEN_AN_CA = bl.TIEN_AN_CA,
                            KHOAN_CONG_KHAC = bl.KHOAN_CONG_KHAC,
                            TIEN_BHXH_TRICH = bl.TIEN_BHXH_TRICH,
                            TIEN_TAMUNG = bl.TIEN_TAMUNG,
                            KHOAN_TRU_KHAC = bl.KHOAN_TRU_KHAC,
                            THUC_LINH = bl.THUC_LINH
                        };
            return query.ToList();
        }

        public void TinhLuongKyCong(int makycong, int iduser)
        {
            try
            {
                // Phân tích tháng và năm từ makycong (ví dụ: 202404 -> năm 2024, tháng 4)
                int nam = makycong / 100;
                int thang = makycong % 100;

                // Lấy kỳ công chi tiết
                var lstKyCongChiTiet = db.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == makycong).ToList();
                if (lstKyCongChiTiet.Count == 0) return;

                // Preload all entities in dictionaries to prevent N+1 queries
                var lstAllNhanVien = db.TB_NHANVIEN.ToList().ToDictionary(x => Convert.ToInt32(x.MANV));
                var lstAllHopDong = db.TB_HOPDONG
                    .Where(x => x.NGAYBATDAU <= DateTime.Now)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.NGAYBATDAU).ToList());

                var lstAllPhuCap = db.TB_NHANVIEN_PHUCAP
                    .Where(x => x.MAKYCONG == makycong)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.ToList());

                var lstAllTangCa = db.TB_TANGCA
                    .Where(x => x.THANG == thang && x.NAM == nam)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.ToList());

                var lstAllLoaiCa = db.TB_LOAICA.ToList().ToDictionary(x => Convert.ToInt32(x.IDLOAICA));

                var lstAllBaoHiem = db.TB_BAOHIEM
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var lstAllUngLuong = db.TB_UNGLUONG
                    .Where(x => x.THANG == thang && x.NAM == nam)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.ToList());

                var lstAllBangLuong = db.TB_BANGLUONG
                    .Where(x => x.MAKYCONG == makycong)
                    .ToList()
                    .ToDictionary(x => Convert.ToInt32(x.MANV));

                // Ngày công chuẩn trong tháng (ví dụ: mặc định là 26 ngày công chuẩn)
                decimal congChuan = 26.0m;

                foreach (var kcct in lstKyCongChiTiet)
                {
                    int manv = Convert.ToInt32(kcct.MANV);

                    // 1. Lấy thông tin nhân viên và Loại nhân viên
                    if (!lstAllNhanVien.TryGetValue(manv, out var nv)) continue;

                    // Lấy loại nhân viên: 1 = Office, 2 = Driver, 3 = Worker (mặc định 1)
                    int loaiNV = nv.LOAI_NV != null ? Convert.ToInt32(nv.LOAI_NV) : 1;

                    // 2. Lấy thông tin Hợp đồng lao động mới nhất còn hiệu lực
                    TB_HOPDONG hd = null;
                    if (lstAllHopDong.TryGetValue(manv, out var hds))
                    {
                        hd = hds.FirstOrDefault();
                    }

                    decimal luongThoaThuan = 0;
                    if (hd != null && hd.LUONG_THOA_THUAN != null)
                    {
                        luongThoaThuan = (decimal)hd.LUONG_THOA_THUAN;
                    }

                    // Nếu lương thỏa thuận trống, thử dùng Hệ số lương nhân với mức lương cơ sở
                    if (luongThoaThuan <= 0 && hd != null && hd.HESOLUONG != null)
                    {
                        if (hd.HESOLUONG > 100)
                        {
                            luongThoaThuan = (decimal)hd.HESOLUONG; // Nếu HESOLUONG lớn hơn 100, đó chính là mức lương thực tế được nhập vào
                        }
                        else
                        {
                            luongThoaThuan = (decimal)hd.HESOLUONG * 1800000m; // ví dụ lương cơ sở 1.8M
                        }
                    }

                    // 3. Phân bổ Lương cơ bản & Phụ cấp cố định theo đối tượng
                    decimal luongCoBan = 0;
                    decimal baseTrachNhiem = 0;
                    decimal baseChuyenCan = 0;
                    decimal baseNhaO = 0;
                    decimal baseNgonNgu = 0;
                    decimal baseThamNien = 0;
                    decimal baseDiLai = 0;
                    decimal baseKhac = 0;

                    // Tính thâm niên (mỗi năm tăng 200,000 đ)
                    if (hd != null && hd.NGAYBATDAU.HasValue)
                    {
                        int soNamLamViec = DateTime.Now.Year - hd.NGAYBATDAU.Value.Year;
                        if (soNamLamViec > 0)
                        {
                            baseThamNien = soNamLamViec * 200000m;
                        }
                    }

                    if (loaiNV == 1) // 💻 NHÂN VIÊN OFFICE
                    {
                        luongCoBan = luongThoaThuan * 0.6m;
                        baseTrachNhiem = luongThoaThuan * 0.1m;
                        baseKhac = luongThoaThuan * 0.14m;
                        baseNhaO = luongThoaThuan * 0.1m;
                        baseChuyenCan = luongThoaThuan * 0.06m;
                    }
                    else if (loaiNV == 2) // 🚚 LÁI XE (DRIVER)
                    {
                        luongCoBan = luongThoaThuan * 0.7m;
                        baseTrachNhiem = luongThoaThuan * 0.1m;
                        baseKhac = luongThoaThuan * 0.07m;
                        baseNhaO = luongThoaThuan * 0.07m;
                        baseChuyenCan = luongThoaThuan * 0.06m;
                    }
                    else // 🛠️ CÔNG NHÂN (WORKER)
                    {
                        luongCoBan = luongThoaThuan > 0 ? luongThoaThuan : 4425000m; 
                        baseChuyenCan = 300000m; 
                        baseKhac = 325000m;
                        baseNhaO = 250000m;
                    }

                    // Đảm bảo và tự động khởi tạo/cập nhật phụ cấp mặc định trong CSDL
                    List<TB_NHANVIEN_PHUCAP> lstPc;
                    if (!lstAllPhuCap.TryGetValue(manv, out lstPc))
                    {
                        lstPc = new List<TB_NHANVIEN_PHUCAP>();
                    }

                    for (int i = 1; i <= 7; i++)
                    {
                        var pcItem = lstPc.FirstOrDefault(x => x.IDPC == i);
                        decimal baseVal = 0;
                        if (i == 1) baseVal = baseTrachNhiem;
                        else if (i == 2) baseVal = baseChuyenCan;
                        else if (i == 3) baseVal = baseNhaO;
                        else if (i == 4) baseVal = baseNgonNgu;
                        else if (i == 5) baseVal = baseThamNien;
                        else if (i == 6) baseVal = baseDiLai;
                        else if (i == 7) baseVal = baseKhac;

                        if (pcItem == null)
                        {
                            pcItem = new TB_NHANVIEN_PHUCAP
                            {
                                MANV = manv,
                                IDPC = i,
                                MAKYCONG = makycong,
                                SOTIEN = baseVal,
                                GHICHU = "Tự động phát sinh khi tính lương"
                            };
                            db.TB_NHANVIEN_PHUCAP.Add(pcItem);
                            lstPc.Add(pcItem);
                        }
                        else if (pcItem.SOTIEN == null || pcItem.SOTIEN == 0)
                        {
                            pcItem.SOTIEN = baseVal;
                        }
                    }

                    // Đọc giá trị phụ cấp từ CSDL sau khi khởi tạo/cập nhật
                    decimal pcTrachNhiem = lstPc.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN ?? 0;
                    decimal pcChuyenCan = lstPc.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN ?? 0;
                    decimal pcNhaO = lstPc.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN ?? 0;
                    decimal pcNgonNgu = lstPc.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN ?? 0;
                    decimal pcThamNien = lstPc.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN ?? 0;
                    decimal pcDiLai = lstPc.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN ?? 0;
                    decimal pcKhac = lstPc.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN ?? 0;

                    // 4. Lấy đơn giá lương ngày (Daily Rate) và phụ cấp ngày (Daily Allowance)
                    // Tiền lương tính BHXH = Lương cơ bản + Phụ cấp trách nhiệm + Ngôn ngữ + Thâm niên
                    decimal luongTinhBHXH = luongCoBan + pcTrachNhiem + pcNgonNgu + pcThamNien;
                    if (loaiNV == 3) // Công nhân
                    {
                        luongTinhBHXH = luongCoBan;
                    }
                    decimal dailyRate = luongTinhBHXH / congChuan;
                    
                    // Phụ cấp ngày gồm: Chuyên cần + nhà ở + đi lại + khác chia cho 26
                    decimal dailyAllowance = (pcChuyenCan + pcNhaO + pcDiLai + pcKhac) / congChuan;

                    // 5. Đọc ngày công thực tế từ Kỳ công chi tiết
                    decimal congThucTe = kcct.TONGNGAYCONG != null ? (decimal)kcct.TONGNGAYCONG : 0;
                    decimal congLamDem = 0; // Công nhân làm đêm (nếu có, tính từ chi tiết bảng chấm công)

                    // Lương công thực tế = công thực tế * Daily Rate (ca đêm phụ trội 130%)
                    decimal luongCongThucTe = (congThucTe * dailyRate) + (congLamDem * dailyRate * 1.3m);
                    decimal phuCapCongThucTe = congThucTe * dailyAllowance;

                    // 6. Tính tiền làm thêm giờ (Overtime)
                    decimal tienTangCa = 0;
                    List<TB_TANGCA> lstTangCa;
                    if (!lstAllTangCa.TryGetValue(manv, out lstTangCa))
                    {
                        lstTangCa = new List<TB_TANGCA>();
                    }
                    
                    // Mức lương cơ sở tính OT
                    decimal otRate = 0;
                    if (loaiNV == 3) // Công nhân: tính OT trên tất cả các khoản cộng lại
                    {
                        otRate = (luongCoBan + pcTrachNhiem + pcNgonNgu + pcThamNien + pcChuyenCan + pcNhaO + pcDiLai + pcKhac) / (congChuan * 8.0m);
                    }
                    else // Nhân viên/Lái xe: tính trên Lương cơ bản + Phụ cấp trách nhiệm + Ngôn ngữ + Thâm niên
                    {
                        otRate = (luongCoBan + pcTrachNhiem + pcNgonNgu + pcThamNien) / (congChuan * 8.0m);
                    }

                    foreach (var tc in lstTangCa)
                    {
                        decimal heSoLoaiCa = 1.5m;
                        if (tc.IDLOAICA != null)
                        {
                            int idLoaiCa = Convert.ToInt32(tc.IDLOAICA);
                            if (lstAllLoaiCa.TryGetValue(idLoaiCa, out var loaiCa) && loaiCa.HESOLOAICA != null)
                            {
                                heSoLoaiCa = (decimal)loaiCa.HESOLOAICA;
                            }
                        }

                        decimal soGio = tc.SOGIO != null ? (decimal)tc.SOGIO : 0;
                        tienTangCa += soGio * otRate * heSoLoaiCa;
                    }

                    // 7. Phụ cấp biến động khác (Không tính vì đã phân bổ trực tiếp vào 7 cột trên)
                    decimal phuCapKhacBienDong = 0;

                    // 8. Chuyên cần tháng (chỉ nhận đủ nếu đi làm đủ công chuẩn)
                    decimal tienChuyenCanNhan = 0;
                    if (congThucTe >= congChuan)
                    {
                        tienChuyenCanNhan = pcChuyenCan;
                    }

                    // 9. Tiền ăn ca đêm (ví dụ: 30,000đ/ngày nếu làm đêm)
                    decimal tienAnCa = 0;

                    // 10. Giảm trừ Bảo hiểm xã hội trích vào lương (10.5% mức lương đóng BHXH)
                    decimal mucLuongDongBH = luongTinhBHXH;
                    TB_BAOHIEM bh = null;
                    if (lstAllBaoHiem.TryGetValue(manv, out bh) && bh != null && bh.LUONG_BHXH != null && bh.LUONG_BHXH > 0)
                    {
                        mucLuongDongBH = (decimal)bh.LUONG_BHXH;
                    }
                    decimal tienBHXHTriCh = mucLuongDongBH * 0.105m; // 8% BHXH + 1.5% BHYT + 1% BHTN

                    // 11. Các khoản tạm ứng lương từ TB_UNGLUONG
                    List<TB_UNGLUONG> lstUng;
                    if (!lstAllUngLuong.TryGetValue(manv, out lstUng))
                    {
                        lstUng = new List<TB_UNGLUONG>();
                    }
                    decimal tienTamUng = 0;
                    foreach (var ul in lstUng)
                    {
                        if (ul.SOTIENUNG != null)
                        {
                            tienTamUng += (decimal)ul.SOTIENUNG;
                        }
                    }

                    // 11b. Tính Thuế Thu Nhập Cá Nhân (PIT / Thuế TNCN)
                    // Tổng thu nhập trước thuế = Lương công thực tế + Phụ cấp công thực tế + Tiền tăng ca + Chuyên cần + Ăn ca + Các khoản cộng khác
                    decimal tongThuNhap = luongCongThucTe + phuCapCongThucTe + tienTangCa + tienChuyenCanNhan + tienAnCa + phuCapKhacBienDong;

                    // Phần tăng ca được miễn thuế (Exempt OT Premium)
                    decimal tangCaMienThue = 0;
                    foreach (var tc in lstTangCa)
                    {
                        decimal heSoLoaiCa = 1.5m;
                        if (tc.IDLOAICA != null)
                        {
                            int idLoaiCa = Convert.ToInt32(tc.IDLOAICA);
                            if (lstAllLoaiCa.TryGetValue(idLoaiCa, out var loaiCa) && loaiCa.HESOLOAICA != null)
                            {
                                heSoLoaiCa = (decimal)loaiCa.HESOLOAICA;
                            }
                        }

                        decimal soGio = tc.SOGIO != null ? (decimal)tc.SOGIO : 0;
                        if (heSoLoaiCa > 1.0m)
                        {
                            tangCaMienThue += soGio * otRate * (heSoLoaiCa - 1.0m);
                        }
                    }

                    // Phần ăn trưa miễn thuế (tối đa 730,000đ)
                    decimal tienAnMienThue = Math.Min(tienAnCa, 730000m);

                    // Lương chịu thuế (Taxable Income)
                    decimal luongChiuThue = tongThuNhap - tangCaMienThue - tienAnMienThue;
                    if (luongChiuThue < 0) luongChiuThue = 0;

                    // Thu nhập tính thuế (Deduction base)
                    decimal giamTruBanThan = 11000000m;
                    decimal giamTruNguoiPhuThuoc = 0;
                    decimal thuNhapTinhThue = luongChiuThue - giamTruBanThan - giamTruNguoiPhuThuoc - tienBHXHTriCh;
                    if (thuNhapTinhThue < 0) thuNhapTinhThue = 0;

                    // Tính thuế TNCN theo biểu lũy tiến từng phần
                    decimal thueTNCN = 0;
                    if (thuNhapTinhThue > 0)
                    {
                        if (thuNhapTinhThue <= 5000000m)
                            thueTNCN = thuNhapTinhThue * 0.05m;
                        else if (thuNhapTinhThue <= 10000000m)
                            thueTNCN = thuNhapTinhThue * 0.1m - 250000m;
                        else if (thuNhapTinhThue <= 18000000m)
                            thueTNCN = thuNhapTinhThue * 0.15m - 750000m;
                        else if (thuNhapTinhThue <= 32000000m)
                            thueTNCN = thuNhapTinhThue * 0.2m - 1650000m;
                        else if (thuNhapTinhThue <= 52000000m)
                            thueTNCN = thuNhapTinhThue * 0.25m - 3250000m;
                        else if (thuNhapTinhThue <= 80000000m)
                            thueTNCN = thuNhapTinhThue * 0.3m - 5850000m;
                        else
                            thueTNCN = thuNhapTinhThue * 0.35m - 9850000m;
                    }
                    thueTNCN = Math.Round(thueTNCN, 0);

                    // 12. Tính toán THỰC LĨNH cuối cùng (trừ thêm cả Thuế TNCN)
                    decimal thucLinh = tongThuNhap - tienBHXHTriCh - tienTamUng - thueTNCN;

                    // 13. Lưu hoặc cập nhật kết quả vào TB_BANGLUONG
                    TB_BANGLUONG bl = null;
                    bool isNew = false;
                    if (!lstAllBangLuong.TryGetValue(manv, out bl) || bl == null)
                    {
                        bl = new TB_BANGLUONG();
                        isNew = true;
                        bl.MANV = manv;
                        bl.MAKYCONG = makycong;
                        bl.THANG = (byte)thang;
                        bl.NAM = (short)nam;
                    }

                    bl.CONG_CHUAN = congChuan;
                    bl.CONG_THUCTE = congThucTe;
                    bl.CONG_LAMDEM = congLamDem;
                    bl.DAILY_RATE = dailyRate;
                    bl.DAILY_ALLOWANCE = dailyAllowance;
                    bl.LUONG_CONG_THUCTE = luongCongThucTe;
                    bl.PHUCAP_CONG_THUCTE = phuCapCongThucTe;
                    bl.TIEN_TANGCA = tienTangCa;
                    bl.TIEN_CHUYENCAN = tienChuyenCanNhan;
                    bl.TIEN_AN_CA = tienAnCa;
                    bl.KHOAN_CONG_KHAC = phuCapKhacBienDong;
                    bl.TIEN_BHXH_TRICH = tienBHXHTriCh;
                    bl.TIEN_TAMUNG = tienTamUng;
                    bl.KHOAN_TRU_KHAC = thueTNCN; // Lưu Thuế TNCN vào cột này
                    bl.THUC_LINH = thucLinh;

                    if (isNew)
                    {
                        db.TB_BANGLUONG.Add(bl);
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tính lương kỳ công: " + ex.Message);
            }
        }
    }
}
