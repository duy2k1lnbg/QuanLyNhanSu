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
            var lst = db.TB_BANGLUONG.Where(x => x.MAKYCONG == makycong).ToList();
            List<BANGLUONG_DTO> lstDTO = new List<BANGLUONG_DTO>();
            foreach (var item in lst)
            {
                var dto = new BANGLUONG_DTO();
                dto.IDBL = item.IDBL;
                dto.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == item.MANV);
                dto.HOTEN = nv != null ? nv.HOTEN : "";
                dto.MAKYCONG = item.MAKYCONG;
                dto.THANG = item.THANG;
                dto.NAM = item.NAM;
                dto.CONG_CHUAN = item.CONG_CHUAN;
                dto.CONG_THUCTE = item.CONG_THUCTE;
                dto.CONG_LAMDEM = item.CONG_LAMDEM;
                dto.DAILY_RATE = item.DAILY_RATE;
                dto.DAILY_ALLOWANCE = item.DAILY_ALLOWANCE;
                dto.LUONG_CONG_THUCTE = item.LUONG_CONG_THUCTE;
                dto.PHUCAP_CONG_THUCTE = item.PHUCAP_CONG_THUCTE;
                dto.TIEN_TANGCA = item.TIEN_TANGCA;
                dto.TIEN_CHUYENCAN = item.TIEN_CHUYENCAN;
                dto.TIEN_AN_CA = item.TIEN_AN_CA;
                dto.KHOAN_CONG_KHAC = item.KHOAN_CONG_KHAC;
                dto.TIEN_BHXH_TRICH = item.TIEN_BHXH_TRICH;
                dto.TIEN_TAMUNG = item.TIEN_TAMUNG;
                dto.KHOAN_TRU_KHAC = item.KHOAN_TRU_KHAC;
                dto.THUC_LINH = item.THUC_LINH;
                lstDTO.Add(dto);
            }
            return lstDTO;
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

                // Ngày công chuẩn trong tháng (ví dụ: mặc định là 26 ngày công chuẩn)
                double congChuan = 26.0;

                foreach (var kcct in lstKyCongChiTiet)
                {
                    int manv = (int)kcct.MANV;

                    // 1. Lấy thông tin nhân viên và Loại nhân viên
                    var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                    if (nv == null) continue;

                    // Lấy loại nhân viên: 1 = Office, 2 = Driver, 3 = Worker (mặc định 1)
                    int loaiNV = nv.LOAI_NV != null ? (int)nv.LOAI_NV : 1;

                    // 2. Lấy thông tin Hợp đồng lao động mới nhất còn hiệu lực
                    var hd = db.TB_HOPDONG
                        .Where(x => x.MANV == manv && x.NGAYBATDAU <= DateTime.Now)
                        .OrderByDescending(x => x.NGAYBATDAU)
                        .FirstOrDefault();

                    double luongThoaThuan = 0;
                    if (hd != null && hd.LUONG_THOA_THUAN != null)
                    {
                        luongThoaThuan = (double)hd.LUONG_THOA_THUAN;
                    }

                    // Nếu lương thỏa thuận trống, thử dùng Hệ số lương nhân với mức lương cơ sở
                    if (luongThoaThuan <= 0 && hd != null && hd.HESOLUONG != null)
                    {
                        if (hd.HESOLUONG > 100)
                        {
                            luongThoaThuan = (double)hd.HESOLUONG; // Nếu HESOLUONG lớn hơn 100, đó chính là mức lương thực tế được nhập vào
                        }
                        else
                        {
                            luongThoaThuan = (double)hd.HESOLUONG * 1800000; // ví dụ lương cơ sở 1.8M
                        }
                    }

                    // 3. Phân bổ Lương cơ bản & Phụ cấp cố định theo đối tượng
                    double luongCoBan = 0;
                    double baseTrachNhiem = 0;
                    double baseChuyenCan = 0;
                    double baseNhaO = 0;
                    double baseNgonNgu = 0;
                    double baseThamNien = 0;
                    double baseDiLai = 0;
                    double baseKhac = 0;

                    // Tính thâm niên (mỗi năm tăng 200,000 đ)
                    if (hd != null && hd.NGAYBATDAU.HasValue)
                    {
                        int soNamLamViec = DateTime.Now.Year - hd.NGAYBATDAU.Value.Year;
                        if (soNamLamViec > 0)
                        {
                            baseThamNien = soNamLamViec * 200000;
                        }
                    }

                    if (loaiNV == 1) // 💻 NHÂN VIÊN OFFICE
                    {
                        luongCoBan = luongThoaThuan * 0.6;
                        baseTrachNhiem = luongThoaThuan * 0.1;
                        baseKhac = luongThoaThuan * 0.14;
                        baseNhaO = luongThoaThuan * 0.1;
                        baseChuyenCan = luongThoaThuan * 0.06;
                    }
                    else if (loaiNV == 2) // 🚚 LÁI XE (DRIVER)
                    {
                        luongCoBan = luongThoaThuan * 0.7;
                        baseTrachNhiem = luongThoaThuan * 0.1;
                        baseKhac = luongThoaThuan * 0.07;
                        baseNhaO = luongThoaThuan * 0.07;
                        baseChuyenCan = luongThoaThuan * 0.06;
                    }
                    else // 🛠️ CÔNG NHÂN (WORKER)
                    {
                        luongCoBan = luongThoaThuan > 0 ? luongThoaThuan : 4425000; 
                        baseChuyenCan = 300000; 
                        baseKhac = 325000;
                        baseNhaO = 250000;
                    }

                    // Đảm bảo và tự động khởi tạo/cập nhật phụ cấp mặc định trong CSDL
                    var lstPc = db.TB_NHANVIEN_PHUCAP.Where(x => x.MANV == manv && x.MAKYCONG == makycong).ToList();
                    for (int i = 1; i <= 7; i++)
                    {
                        var pcItem = lstPc.FirstOrDefault(x => x.IDPC == i);
                        double baseVal = 0;
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
                                SOTIEN = (decimal)baseVal,
                                GHICHU = "Tự động phát sinh khi tính lương"
                            };
                            db.TB_NHANVIEN_PHUCAP.Add(pcItem);
                            lstPc.Add(pcItem);
                        }
                        else if (pcItem.SOTIEN == null || pcItem.SOTIEN == 0)
                        {
                            pcItem.SOTIEN = (decimal)baseVal;
                        }
                    }
                    db.SaveChanges();

                    // Đọc giá trị phụ cấp từ CSDL sau khi khởi tạo/cập nhật
                    double pcTrachNhiem = (double)(lstPc.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN ?? 0);
                    double pcChuyenCan = (double)(lstPc.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN ?? 0);
                    double pcNhaO = (double)(lstPc.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN ?? 0);
                    double pcNgonNgu = (double)(lstPc.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN ?? 0);
                    double pcThamNien = (double)(lstPc.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN ?? 0);
                    double pcDiLai = (double)(lstPc.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN ?? 0);
                    double pcKhac = (double)(lstPc.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN ?? 0);

                    // 4. Lấy đơn giá lương ngày (Daily Rate) và phụ cấp ngày (Daily Allowance)
                    // Tiền lương tính BHXH = Lương cơ bản + Phụ cấp trách nhiệm + Ngôn ngữ + Thâm niên
                    double luongTinhBHXH = luongCoBan + pcTrachNhiem + pcNgonNgu + pcThamNien;
                    if (loaiNV == 3) // Công nhân
                    {
                        luongTinhBHXH = luongCoBan;
                    }
                    double dailyRate = luongTinhBHXH / congChuan;
                    
                    // Phụ cấp ngày gồm: Chuyên cần + nhà ở + đi lại + khác chia cho 26
                    double dailyAllowance = (pcChuyenCan + pcNhaO + pcDiLai + pcKhac) / congChuan;

                    // 5. Đọc ngày công thực tế từ Kỳ công chi tiết
                    double congThucTe = kcct.TONGNGAYCONG != null ? (double)kcct.TONGNGAYCONG : 0;
                    double congLamDem = 0; // Công nhân làm đêm (nếu có, tính từ chi tiết bảng chấm công)

                    // Lương công thực tế = công thực tế * Daily Rate (ca đêm phụ trội 130%)
                    double luongCongThucTe = (congThucTe * dailyRate) + (congLamDem * dailyRate * 1.3);
                    double phuCapCongThucTe = congThucTe * dailyAllowance;

                    // 6. Tính tiền làm thêm giờ (Overtime)
                    double tienTangCa = 0;
                    var lstTangCa = db.TB_TANGCA.Where(x => x.MANV == manv && x.THANG == thang && x.NAM == nam).ToList();
                    
                    // Mức lương cơ sở tính OT
                    double otRate = 0;
                    if (loaiNV == 3) // Công nhân: tính OT trên tất cả các khoản cộng lại
                    {
                        otRate = (luongCoBan + pcTrachNhiem + pcNgonNgu + pcThamNien + pcChuyenCan + pcNhaO + pcDiLai + pcKhac) / (congChuan * 8.0);
                    }
                    else // Nhân viên/Lái xe: tính trên Lương cơ bản + Phụ cấp trách nhiệm + Ngôn ngữ + Thâm niên
                    {
                        otRate = (luongCoBan + pcTrachNhiem + pcNgonNgu + pcThamNien) / (congChuan * 8.0);
                    }

                    foreach (var tc in lstTangCa)
                    {
                        double heSoLoaiCa = 1.5;
                        var loaiCa = db.TB_LOAICA.FirstOrDefault(x => x.IDLOAICA == tc.IDLOAICA);
                        if (loaiCa != null && loaiCa.HESOLOAICA != null)
                        {
                            heSoLoaiCa = (double)loaiCa.HESOLOAICA;
                        }

                        double soGio = tc.SOGIO != null ? (double)tc.SOGIO : 0;
                        tienTangCa += soGio * otRate * heSoLoaiCa;
                    }

                    // 7. Phụ cấp biến động khác (Không tính vì đã phân bổ trực tiếp vào 7 cột trên)
                    double phuCapKhacBienDong = 0;

                    // 8. Chuyên cần tháng (chỉ nhận đủ nếu đi làm đủ công chuẩn)
                    double tienChuyenCanNhan = 0;
                    if (congThucTe >= congChuan)
                    {
                        tienChuyenCanNhan = pcChuyenCan;
                    }

                    // 9. Tiền ăn ca đêm (ví dụ: 30,000đ/ngày nếu làm đêm)
                    double tienAnCa = 0;

                    // 10. Giảm trừ Bảo hiểm xã hội trích vào lương (10.5% mức lương đóng BHXH)
                    double mucLuongDongBH = luongTinhBHXH;
                    var bh = db.TB_BAOHIEM.FirstOrDefault(x => x.MANV == manv);
                    if (bh != null && bh.LUONG_BHXH != null && bh.LUONG_BHXH > 0)
                    {
                        mucLuongDongBH = (double)bh.LUONG_BHXH;
                    }
                    double tienBHXHTriCh = mucLuongDongBH * 0.105; // 8% BHXH + 1.5% BHYT + 1% BHTN

                    // 11. Các khoản tạm ứng lương từ TB_UNGLUONG
                    double tienTamUng = 0;
                    var lstUng = db.TB_UNGLUONG.Where(x => x.MANV == manv && x.THANG == thang && x.NAM == nam).ToList();
                    foreach (var ul in lstUng)
                    {
                        if (ul.SOTIENUNG != null)
                        {
                            tienTamUng += (double)ul.SOTIENUNG;
                        }
                    }

                    // 11b. Tính Thuế Thu Nhập Cá Nhân (PIT / Thuế TNCN)
                    // Tổng thu nhập trước thuế = Lương công thực tế + Phụ cấp công thực tế + Tiền tăng ca + Chuyên cần + Ăn ca + Các khoản cộng khác
                    double tongThuNhap = luongCongThucTe + phuCapCongThucTe + tienTangCa + tienChuyenCanNhan + tienAnCa + phuCapKhacBienDong;

                    // Phần tăng ca được miễn thuế (Exempt OT Premium)
                    double tangCaMienThue = 0;
                    foreach (var tc in lstTangCa)
                    {
                        double heSoLoaiCa = 1.5;
                        var loaiCa = db.TB_LOAICA.FirstOrDefault(x => x.IDLOAICA == tc.IDLOAICA);
                        if (loaiCa != null && loaiCa.HESOLOAICA != null)
                        {
                            heSoLoaiCa = (double)loaiCa.HESOLOAICA;
                        }

                        double soGio = tc.SOGIO != null ? (double)tc.SOGIO : 0;
                        if (heSoLoaiCa > 1.0)
                        {
                            tangCaMienThue += soGio * otRate * (heSoLoaiCa - 1.0);
                        }
                    }

                    // Phần ăn trưa miễn thuế (tối đa 730,000đ)
                    double tienAnMienThue = Math.Min(tienAnCa, 730000);

                    // Lương chịu thuế (Taxable Income)
                    double luongChiuThue = tongThuNhap - tangCaMienThue - tienAnMienThue;
                    if (luongChiuThue < 0) luongChiuThue = 0;

                    // Thu nhập tính thuế (Deduction base)
                    double giamTruBanThan = 11000000;
                    double giamTruNguoiPhuThuoc = 0;
                    double thuNhapTinhThue = luongChiuThue - giamTruBanThan - giamTruNguoiPhuThuoc - tienBHXHTriCh;
                    if (thuNhapTinhThue < 0) thuNhapTinhThue = 0;

                    // Tính thuế TNCN theo biểu lũy tiến từng phần
                    double thueTNCN = 0;
                    if (thuNhapTinhThue > 0)
                    {
                        if (thuNhapTinhThue <= 5000000)
                            thueTNCN = thuNhapTinhThue * 0.05;
                        else if (thuNhapTinhThue <= 10000000)
                            thueTNCN = thuNhapTinhThue * 0.1 - 250000;
                        else if (thuNhapTinhThue <= 18000000)
                            thueTNCN = thuNhapTinhThue * 0.15 - 750000;
                        else if (thuNhapTinhThue <= 32000000)
                            thueTNCN = thuNhapTinhThue * 0.2 - 1650000;
                        else if (thuNhapTinhThue <= 52000000)
                            thueTNCN = thuNhapTinhThue * 0.25 - 3250000;
                        else if (thuNhapTinhThue <= 80000000)
                            thueTNCN = thuNhapTinhThue * 0.3 - 5850000;
                        else
                            thueTNCN = thuNhapTinhThue * 0.35 - 9850000;
                    }
                    thueTNCN = Math.Round(thueTNCN, 0);

                    // 12. Tính toán THỰC LĨNH cuối cùng (trừ thêm cả Thuế TNCN)
                    double thucLinh = tongThuNhap - tienBHXHTriCh - tienTamUng - thueTNCN;

                    // 13. Lưu hoặc cập nhật kết quả vào TB_BANGLUONG
                    var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.MANV == manv && x.MAKYCONG == makycong);
                    bool isNew = false;
                    if (bl == null)
                    {
                        bl = new TB_BANGLUONG();
                        isNew = true;
                        bl.MANV = manv;
                        bl.MAKYCONG = makycong;
                        bl.THANG = (byte)thang;
                        bl.NAM = (short)nam;
                    }

                    bl.CONG_CHUAN = (decimal)congChuan;
                    bl.CONG_THUCTE = (decimal)congThucTe;
                    bl.CONG_LAMDEM = (decimal)congLamDem;
                    bl.DAILY_RATE = (decimal)dailyRate;
                    bl.DAILY_ALLOWANCE = (decimal)dailyAllowance;
                    bl.LUONG_CONG_THUCTE = (decimal)luongCongThucTe;
                    bl.PHUCAP_CONG_THUCTE = (decimal)phuCapCongThucTe;
                    bl.TIEN_TANGCA = (decimal)tienTangCa;
                    bl.TIEN_CHUYENCAN = (decimal)tienChuyenCanNhan;
                    bl.TIEN_AN_CA = (decimal)tienAnCa;
                    bl.KHOAN_CONG_KHAC = (decimal)phuCapKhacBienDong;
                    bl.TIEN_BHXH_TRICH = (decimal)tienBHXHTriCh;
                    bl.TIEN_TAMUNG = (decimal)tienTamUng;
                    bl.KHOAN_TRU_KHAC = (decimal)thueTNCN; // Lưu Thuế TNCN vào cột này
                    bl.THUC_LINH = (decimal)thucLinh;

                    if (isNew)
                    {
                        db.TB_BANGLUONG.Add(bl);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tính lương kỳ công: " + ex.Message);
            }
        }
    }
}
