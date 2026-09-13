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
                            CONG_LAMNGAY = bl.CONG_LAMNGAY,
                            CONG_LAMDEM = bl.CONG_LAMDEM,
                            DAILY_RATE = bl.DAILY_RATE,
                            DAILY_ALLOWANCE = bl.DAILY_ALLOWANCE,
                            LUONG_CONG_THUCTE = bl.LUONG_CONG_THUCTE,
                            PHUCAP_CONG_THUCTE = bl.PHUCAP_CONG_THUCTE,
                            TIEN_TANGCA = bl.TIEN_TANGCA,
                            TIEN_CHUYENCAN = bl.TIEN_CHUYENCAN,
                            TIEN_AN_CA = bl.TIEN_AN_CA,
                            KHOAN_CONG_KHAC = bl.KHOAN_CONG_KHAC,
                            TONG_CONG = bl.TONG_CONG,
                            LUONG_BHXH = bl.LUONG_BHXH,
                            TIEN_BHXH = bl.TIEN_BHXH,
                            TIEN_BHYT = bl.TIEN_BHYT,
                            TIEN_BHTN = bl.TIEN_BHTN,
                            TIEN_BHXH_TRICH = bl.TIEN_BHXH_TRICH,
                            TIEN_CONG_DOAN = bl.TIEN_CONG_DOAN,
                            TIEN_TAMUNG = bl.TIEN_TAMUNG,
                            THUE_TNCN = bl.THUE_TNCN,
                            KHOAN_TRU_KHAC = bl.KHOAN_TRU_KHAC,
                            HOAN_THUE = bl.HOAN_THUE,
                            THUC_LINH = bl.THUC_LINH,
                            LUONG_CA_NGAY = (bl.DAILY_RATE != null && bl.CONG_LAMNGAY != null) ? (bl.DAILY_RATE * bl.CONG_LAMNGAY) : 0,
                            LUONG_CA_DEM = (bl.DAILY_RATE != null && bl.CONG_LAMDEM != null) ? (bl.DAILY_RATE * bl.CONG_LAMDEM * 1.30m) : 0
                        };
            return query.ToList();
        }

        public void TinhLuongKyCong(int makycong, int iduser)
        {
            TinhLuongKyCong(makycong, iduser, null);
        }

        public void TinhLuongKyCong(int makycong, int iduser, Action<int, int, string> progress)
        {
            try
            {
                progress?.Invoke(0, 100, "Đang kiểm tra thời hạn hợp đồng nhân sự...");

                // Phân tích tháng và năm từ makycong (ví dụ: 202601 -> năm 2026, tháng 1)
                int nam = makycong / 100;
                int thang = makycong % 100;
                DateTime periodStart = new DateTime(nam, thang, 1);
                DateTime periodEnd = new DateTime(nam, thang, DateTime.DaysInMonth(nam, thang));

                // 1. Tự động kiểm tra thời hạn hợp đồng và cập nhật DATHOIVIEC = 1 nếu đã hết hạn
                NHANVIEN nhanvienBus = new NHANVIEN();
                nhanvienBus.KiemTraVaCapNhatTrangThaiHopDong(nam, thang);

                progress?.Invoke(15, 100, "Đang tải dữ liệu kỳ công và danh mục...");

                // Lấy kỳ công chi tiết
                var lstKyCongChiTiet = db.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == makycong).ToList();
                if (lstKyCongChiTiet.Count == 0) return;

                // Preload all entities in dictionaries to prevent N+1 queries
                var lstAllNhanVien = db.TB_NHANVIEN.ToList().ToDictionary(x => Convert.ToInt32(x.MANV));
                
                var lstAllHopDong = db.TB_HOPDONG
                    .Where(x => x.NGAYBATDAU <= periodEnd)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.NGAYBATDAU).ToList());

                // Tải chi tiết từng ngày công trong kỳ công (TB_BANGCONG_CHITIET)
                var lstAllBangCongChiTiet = db.TB_BANGCONG_CHITIET
                    .Where(x => x.MAKYCONG == makycong)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.ToList());

                var lstAllPhuCap = db.TB_NHANVIEN_PHUCAP
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

                var lstAllKtkl = db.TB_KHENTHUONG_KYLUAT
                    .Where(x => x.THANG_APDUNG == thang && x.NAM_APDUNG == nam && x.DELETED_BY == null && x.SOTIEN > 0)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.ToList());

                var lstAllBangLuong = db.TB_BANGLUONG
                    .Where(x => x.MAKYCONG == makycong)
                    .ToList()
                    .ToDictionary(x => Convert.ToInt32(x.MANV));

                // Ngày công chuẩn trong tháng (mặc định là 26 ngày công chuẩn)
                decimal congChuan = 26.0m;

                int totalCount = lstKyCongChiTiet.Count;
                int processedCount = 0;

                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                foreach (var kcct in lstKyCongChiTiet)
                {
                    int manv = Convert.ToInt32(kcct.MANV);
                    processedCount++;

                    // 1. Lấy thông tin nhân viên và Loại nhân viên
                    if (!lstAllNhanVien.TryGetValue(manv, out var nv)) continue;

                    // Chỉ tính lương cho nhân viên chưa thôi việc
                    if (nv.DATHOIVIEC == 1) continue;

                    // 2. Lấy thông tin Hợp đồng lao động mới nhất còn hiệu lực
                    TB_HOPDONG hd = null;
                    if (lstAllHopDong.TryGetValue(manv, out var hds) && hds != null)
                    {
                        hd = hds.FirstOrDefault();
                    }

                    // Không tính lương nếu không có hợp đồng hoặc hợp đồng đã kết thúc trước kỳ công
                    if (hd == null) continue;
                    if (hd.NGAYKETTHUC.HasValue && hd.NGAYKETTHUC.Value < periodStart) continue;

                    // 3. Phân biệt công ca ngày và ca đêm từ TB_BANGCONG_CHITIET
                    decimal congNgay = 0;
                    decimal congDem = 0;
                    decimal congPhep = 0;
                    decimal congLe = 0;
                    decimal congChuNhat = 0;

                    List<TB_BANGCONG_CHITIET> lstBcct;
                    if (lstAllBangCongChiTiet.TryGetValue(manv, out lstBcct) && lstBcct != null && lstBcct.Count > 0)
                    {
                        foreach (var row in lstBcct)
                        {
                            bool isDem = (row.KYHIEU == "CD" || row.KYHIEU == "Đ" || row.KYHIEU == "XĐ" ||
                                         (row.GIOVAO != null && (string.Compare(row.GIOVAO, "18:00") >= 0 || string.Compare(row.GIOVAO, "06:00") < 0)));
                            decimal nc = row.NGAYCONG.HasValue ? (decimal)row.NGAYCONG.Value : 0;
                            if (isDem)
                            {
                                congDem += nc;
                            }
                            else
                            {
                                congNgay += nc;
                            }

                            if (row.NGAYPHEP.HasValue) congPhep += (decimal)row.NGAYPHEP.Value;
                            if (row.CONGNGAYLE.HasValue) congLe += (decimal)row.CONGNGAYLE.Value;
                            if (row.CONGCHUNHAT.HasValue) congChuNhat += (decimal)row.CONGCHUNHAT.Value;
                        }
                    }
                    else
                    {
                        // Fallback nếu chưa có chi tiết từng ngày thì lấy từ TB_KYCONGCHITIET
                        congNgay = kcct.TONGNGAYCONG.HasValue ? (decimal)kcct.TONGNGAYCONG.Value : 0;
                        congDem = 0;
                        congPhep = kcct.NGAYPHEP.HasValue ? (decimal)kcct.NGAYPHEP.Value : 0;
                    }

                    decimal congThucTe = congNgay + congDem;

                    // 4. Lương cơ bản theo hợp đồng
                    decimal luongThoaThuan = 0;
                    if (hd.LUONG_THOA_THUAN.HasValue && hd.LUONG_THOA_THUAN.Value > 0)
                    {
                        luongThoaThuan = (decimal)hd.LUONG_THOA_THUAN.Value;
                    }
                    else if (hd.HESOLUONG.HasValue && hd.HESOLUONG.Value > 100)
                    {
                        luongThoaThuan = (decimal)hd.HESOLUONG.Value;
                    }
                    decimal luongCoBan = luongThoaThuan > 0 ? luongThoaThuan : 4525000m;

                    // 5. Đơn giá lương ngày (DAILY_RATE = Lương cơ bản / Công chuẩn)
                    decimal dailyRate = luongCoBan / congChuan;

                    // 6. Lương ca ngày và Lương ca đêm
                    // Lương ca ngày = Lương cơ bản / Công chuẩn * Công ca ngày
                    decimal luongCaNgay = Math.Round((luongCoBan / congChuan) * congNgay, 0);
                    // Lương ca đêm = Lương cơ bản / Công chuẩn * Công ca đêm * Hệ số ca đêm (1.30)
                    decimal heSoCaDem = 1.30m;
                    decimal luongCaDem = Math.Round((luongCoBan / congChuan) * congDem * heSoCaDem, 0);

                    decimal luongCongThucTe = luongCaNgay + luongCaDem;

                    // 7. Phụ cấp theo Hợp đồng & theo Tháng
                    List<TB_NHANVIEN_PHUCAP> lstPc;
                    if (!lstAllPhuCap.TryGetValue(manv, out lstPc))
                    {
                        lstPc = new List<TB_NHANVIEN_PHUCAP>();
                    }

                    Func<int, decimal> getPc = (id) =>
                    {
                        var item = lstPc.FirstOrDefault(x => x.IDPC == id);
                        return item?.SOTIEN != null ? (decimal)item.SOTIEN : 0m;
                    };

                    decimal pcNhaO = getPc(1);          // 1. Phụ cấp nhà ở
                    decimal pcDiLai = getPc(2);         // 2. Phụ cấp đi lại
                    decimal pcGiaDinh = getPc(3);       // 3. Phụ cấp gia đình
                    decimal pcNguoiPhuThuoc = getPc(4); // 4. Phụ cấp người phụ thuộc
                    decimal pcChucVu = getPc(5);        // 5. Phụ cấp chức vụ
                    decimal pcChungChi = getPc(6);      // 6. Phụ cấp chứng chỉ
                    decimal pcKyNang = getPc(7);        // 7. Phụ cấp kỹ năng
                    decimal pcKhuVuc = getPc(8);        // 8. Phụ cấp khu vực
                    decimal pcChuyenCan = getPc(9);     // 9. Phụ cấp chuyên cần
                    decimal pcThamNien = getPc(10);     // 10. Phụ cấp thâm niên
                    decimal pcLamViecTaiNha = getPc(11);// 11. Phụ cấp làm việc tại nhà
                    decimal pcDacBiet = getPc(12);      // 12. Phụ cấp đặc biệt
                    decimal pcKhac = getPc(13);         // 13. Phụ cấp khác

                    // Tổng các khoản phụ cấp tính theo ngày công (không bao gồm chuyên cần và ăn ca)
                    decimal tongPhuCapThang = pcNhaO + pcDiLai + pcGiaDinh + pcNguoiPhuThuoc + pcChucVu + pcChungChi + pcKyNang + pcKhuVuc + pcThamNien + pcLamViecTaiNha + pcDacBiet + pcKhac;
                    decimal dailyAllowance = congChuan > 0 ? (tongPhuCapThang / congChuan) : 0;
                    decimal phuCapCongThucTe = Math.Round(dailyAllowance * congThucTe, 0);

                    // 8. Tiền chuyên cần: Đủ điều kiện mới nhận
                    decimal tienChuyenCan = 0;
                    if (pcChuyenCan > 0 && congThucTe >= congChuan)
                    {
                        tienChuyenCan = pcChuyenCan;
                    }
                    else if (pcChuyenCan > 0 && congThucTe >= 24) // Linh hoạt ngưỡng chuyên cần nếu đi làm đủ tháng
                    {
                        tienChuyenCan = pcChuyenCan;
                    }

                    // 9. Tiền ăn ca: Số ngày hưởng ăn * đơn giá ăn ca
                    decimal tienAnCa = 0;
                    if (congDem > 0)
                    {
                        tienAnCa = Math.Round(congDem * 20000m, 0); // 20.000đ/ca đêm (12 đêm = 240.000đ)
                    }

                    // 10. Tiền phép: Số ngày phép * Đơn giá phép
                    decimal tienPhep = 0;
                    if (congPhep > 0)
                    {
                        tienPhep = Math.Round(congPhep * 200000m, 0); // Đơn giá phép (hoặc theo cấu hình/dailyRate)
                    }

                    // Lương thử việc (nếu có hợp đồng thử việc riêng biệt trong kỳ)
                    decimal luongThuViec = 0;

                    // Tiền khen thưởng áp dụng trong kỳ lương (cộng vào lương)
                    decimal tienKhenThuong = 0;
                    decimal tienKyLuat = 0;
                    if (lstAllKtkl.TryGetValue(manv, out var listKtkl) && listKtkl != null)
                    {
                        tienKhenThuong = listKtkl.Where(x => x.LOAI == 1 && x.SOTIEN.HasValue).Sum(x => (decimal)x.SOTIEN.Value);
                        tienKyLuat = listKtkl.Where(x => x.LOAI == 2 && x.SOTIEN.HasValue).Sum(x => (decimal)x.SOTIEN.Value);
                    }

                    // Khoản cộng khác: Tiền khen thưởng được duyệt trong kỳ
                    decimal khoanCongKhac = tienKhenThuong;

                    // 11. Tính tiền Tăng ca (Overtime)
                    decimal tienTangCa = 0;
                    List<TB_TANGCA> lstTangCa;
                    if (!lstAllTangCa.TryGetValue(manv, out lstTangCa))
                    {
                        lstTangCa = new List<TB_TANGCA>();
                    }

                    foreach (var tc in lstTangCa)
                    {
                        if (tc.SOTIENTC.HasValue && tc.SOTIENTC.Value > 0)
                        {
                            tienTangCa += (decimal)tc.SOTIENTC.Value;
                        }
                        else
                        {
                            decimal soGio = tc.SOGIO.HasValue ? (decimal)tc.SOGIO.Value : 0;
                            decimal donGiaOT = (tc.DONGIATC.HasValue && tc.DONGIATC.Value > 0)
                                ? (decimal)tc.DONGIATC.Value
                                : 25000m; // Đơn giá 1 giờ chuẩn 25.000đ

                            decimal heSo = tc.HESOTC.HasValue ? (decimal)tc.HESOTC.Value : 1.5m;

                            // Nếu là thử việc (LOAIHD = 1) thì hệ số OT = HỆ SỐ OT * 85%
                            bool isThuViecOT = (tc.IS_THUVIEC == 1) || (hd.LOAIHD == 1);
                            if (isThuViecOT && (tc.IS_THUVIEC != 1))
                            {
                                heSo = heSo * 0.85m;
                            }

                            decimal tienTC = Math.Round(soGio * donGiaOT * heSo, 0);
                            tc.DONGIATC = donGiaOT;
                            tc.HESOTC = heSo;
                            tc.SOTIENTC = tienTC;
                            tienTangCa += tienTC;
                        }
                    }

                    // 12. Tổng tiền công thực tế
                    // = LƯƠNG CA NGÀY + LƯƠNG CA ĐÊM + LƯƠNG THỬ VIỆC + TIỀN PHỤ CẤP + TIỀN PHÉP + TIỀN CHUYÊN CẦN + TIỀN ĂN CA + KHOẢN CỘNG KHÁC
                    decimal tongTienCongThucTe = luongCaNgay + luongCaDem + luongThuViec + phuCapCongThucTe + tienPhep + tienChuyenCan + tienAnCa + khoanCongKhac;

                    // 13. Tổng cộng = TỔNG TIỀN CÔNG THỰC TẾ + TỔNG TIỀN OT
                    decimal tongCong = tongTienCongThucTe + tienTangCa;

                    // 14. Bảo hiểm & Đoàn phí
                    // BHXH = LƯƠNG BHXH * 8%
                    // BHYT = LƯƠNG BHXH * 1.5%
                    // BHTN = LƯƠNG BHXH * 1%
                    // PHÍ CÔNG ĐOÀN = Mức phí công đoàn theo cấu hình
                    decimal luongBHXH = luongCoBan;
                    TB_BAOHIEM bh = null;
                    if (lstAllBaoHiem.TryGetValue(manv, out bh) && bh != null && bh.LUONG_BHXH.HasValue && bh.LUONG_BHXH.Value > 0)
                    {
                        luongBHXH = (decimal)bh.LUONG_BHXH.Value;
                    }

                    decimal tienBHXH = Math.Round(luongBHXH * 0.08m, 0);   // 8% = 362.000đ
                    decimal tienBHYT = Math.Round(luongBHXH * 0.015m, 0);  // 1.5% = 67.875đ
                    decimal tienBHTN = Math.Round(luongBHXH * 0.01m, 0);   // 1% = 45.250đ
                    decimal tienBHXHTriCh = tienBHXH + tienBHYT + tienBHTN;// 10.5% = 475.125đ
                    decimal tienCongDoan = 42000m;                         // Phí công đoàn = 42.000đ

                    // 15. Tạm ứng, Thuế TNCN, Hoàn thuế
                    List<TB_UNGLUONG> lstUng;
                    if (!lstAllUngLuong.TryGetValue(manv, out lstUng))
                    {
                        lstUng = new List<TB_UNGLUONG>();
                    }
                    decimal tienTamUng = 0;
                    foreach (var ul in lstUng)
                    {
                        if (ul.SOTIENUNG.HasValue) tienTamUng += (decimal)ul.SOTIENUNG.Value;
                    }

                    decimal thueTNCN = 0;
                    decimal hoanThue = 0;
                    // Khoản trừ khác: Tiền phạt kỷ luật trong kỳ
                    decimal khoanTruKhac = tienKyLuat;

                    // TỔNG KHẤU TRỪ = BHXH + BHYT + BHTN + PHÍ CÔNG ĐOÀN + THUẾ TNCN + TIỀN TẠM ỨNG + KHOẢN TRỪ KHÁC
                    decimal tongKhauTru = tienBHXH + tienBHYT + tienBHTN + tienCongDoan + thueTNCN + tienTamUng + khoanTruKhac;

                    // 16. THỰC LĨNH = TỔNG CỘNG - TỔNG KHẤU TRỪ + HOÀN THUẾ
                    decimal thucLinh = tongCong - tongKhauTru + hoanThue;

                    // 17. Lưu hoặc cập nhật kết quả vào TB_BANGLUONG
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
                    bl.CONG_LAMNGAY = congNgay;
                    bl.CONG_LAMDEM = congDem;
                    bl.DAILY_RATE = dailyRate;
                    bl.DAILY_ALLOWANCE = dailyAllowance;
                    bl.LUONG_CONG_THUCTE = luongCaNgay + luongCaDem + luongThuViec + tienPhep;
                    bl.PHUCAP_CONG_THUCTE = phuCapCongThucTe;
                    bl.TIEN_TANGCA = tienTangCa;
                    bl.TIEN_CHUYENCAN = tienChuyenCan;
                    bl.TIEN_AN_CA = tienAnCa;
                    bl.KHOAN_CONG_KHAC = khoanCongKhac;
                    bl.TONG_CONG = tongCong;
                    bl.LUONG_BHXH = luongBHXH;
                    bl.TIEN_BHXH = tienBHXH;
                    bl.TIEN_BHYT = tienBHYT;
                    bl.TIEN_BHTN = tienBHTN;
                    bl.TIEN_BHXH_TRICH = tienBHXHTriCh;
                    bl.TIEN_CONG_DOAN = tienCongDoan;
                    bl.THUE_TNCN = thueTNCN;
                    bl.TIEN_TAMUNG = tienTamUng;
                    bl.KHOAN_TRU_KHAC = khoanTruKhac;
                    bl.HOAN_THUE = hoanThue;
                    bl.THUC_LINH = thucLinh;

                    if (isNew)
                    {
                        db.TB_BANGLUONG.Add(bl);
                    }

                    if (processedCount % 50 == 0 || processedCount == totalCount)
                    {
                        int p = 15 + (int)((double)processedCount / totalCount * 75);
                        progress?.Invoke(p, 100, $"Đang tính lương & phụ cấp: {processedCount}/{totalCount} nhân sự");
                    }
                }

                progress?.Invoke(90, 100, "Đang lưu dữ liệu bảng lương vào CSDL...");
                db.SaveChanges();
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = true;
                progress?.Invoke(100, 100, "Hoàn tất tính lương!");
            }
            catch (Exception ex)
            {
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = true;
                throw new Exception("Lỗi khi tính lương kỳ công: " + ex.Message, ex);
            }
        }
    }
}
