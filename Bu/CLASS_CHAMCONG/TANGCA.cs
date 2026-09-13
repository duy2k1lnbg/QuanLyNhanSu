using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_CHAMCONG
{
    public class TangCaCalcResult
    {
        public double SoGio { get; set; }
        public decimal DonGia1Gio { get; set; }
        public decimal HeSo { get; set; }
        public decimal ThanhTien { get; set; }
        public string TenQuyDinh { get; set; }
        public bool IsThuViec { get; set; }
    }

    public class TANGCA
    {
        private MyEntities db = new MyEntities();
        private HESO_TANGCA _heSoTangCaBus = new HESO_TANGCA();

        public TB_TANGCA getItem(int id)
        {
            return db.TB_TANGCA.FirstOrDefault(x => x.IDTCA == id);
        }

        public List<TB_TANGCA> getList()
        {
            return db.TB_TANGCA.ToList();
        }

        public List<TANGCA_DTO> getListFull()
        {
            var rawList = (from tc in db.TB_TANGCA
                           join nv in db.TB_NHANVIEN on tc.MANV equals nv.MANV into nvGroup
                           from nv in nvGroup.DefaultIfEmpty()
                           join lc in db.TB_LOAICA on tc.IDLOAICA equals lc.IDLOAICA into lcGroup
                           from lc in lcGroup.DefaultIfEmpty()
                           join lcong in db.TB_LOAICONG on tc.IDLOAICONG equals lcong.IDLOAICONG into lcongGroup
                           from lcong in lcongGroup.DefaultIfEmpty()
                           select new
                           {
                               tc,
                               HOTEN = nv != null ? nv.HOTEN : null,
                               TENLOAICA = lc != null ? lc.TENLOAICA : null,
                               HESOLOAICA = lc != null ? lc.HESOLOAICA : null,
                               TENLOAICONG = lcong != null ? lcong.TENLC : null,
                               HESOLOAICONG = lcong != null ? lcong.HESOLOAICONG : null
                           }).ToList();

            var result = rawList.Select(x =>
            {
                DateTime? ngayFull = null;
                if (x.tc.NAM.HasValue && x.tc.THANG.HasValue && x.tc.NGAY.HasValue)
                {
                    try
                    {
                        ngayFull = new DateTime((int)x.tc.NAM.Value, (int)x.tc.THANG.Value, (int)x.tc.NGAY.Value);
                    }
                    catch { }
                }

                bool isThuViec = x.tc.IS_THUVIEC.HasValue && x.tc.IS_THUVIEC.Value == 1;

                return new TANGCA_DTO
                {
                    IDTCA = x.tc.IDTCA,
                    NAM = x.tc.NAM,
                    THANG = x.tc.THANG,
                    NGAY = x.tc.NGAY,
                    NGAY_FULL = ngayFull,
                    SOGIO = x.tc.SOGIO,
                    GHICHU = x.tc.GHICHU,
                    MANV = x.tc.MANV,
                    HOTEN = x.HOTEN,
                    IDLOAICA = x.tc.IDLOAICA,
                    TENLOAICA = x.TENLOAICA,
                    HESOLOAICA = x.HESOLOAICA,
                    IDLOAICONG = x.tc.IDLOAICONG,
                    TENLOAICONG = x.TENLOAICONG,
                    HESOLOAICONG = x.HESOLOAICONG != null ? (decimal?)Convert.ToDecimal(x.HESOLOAICONG.Value) : null,
                    GIOBATDAU = x.tc.GIOBATDAU,
                    GIOKETTHUC = x.tc.GIOKETTHUC,
                    HESOTC = x.tc.HESOTC != null ? (decimal?)Convert.ToDecimal(x.tc.HESOTC.Value) : null,
                    DONGIATC = x.tc.DONGIATC != null ? (decimal?)Convert.ToDecimal(x.tc.DONGIATC.Value) : null,
                    IS_THUVIEC = x.tc.IS_THUVIEC,
                    TRANGTHAI_NV = isThuViec ? "Thử việc (85%)" : "Chính thức (100%)",
                    SOTIENTC = x.tc.SOTIENTC,
                    CREATED_BY = x.tc.CREATED_BY,
                    CREATED_DATE = x.tc.CREATED_DATE,
                    UPDATED_BY = x.tc.UPDATED_BY,
                    UPDATED_DATE = x.tc.UPDATED_DATE,
                    DELETED_BY = x.tc.DELETED_BY,
                    DELETED_DATE = x.tc.DELETED_DATE
                };
            }).OrderByDescending(x => x.IDTCA).ToList();

            return result;
        }

        /// <summary>
        /// Tự động tính số giờ tăng ca từ giờ bắt đầu đến giờ kết thúc, xử lý chính xác ca qua đêm (ví dụ 22:00 -> 02:00 = 4 giờ)
        /// </summary>
        public double TinhSoGio(string gioBatDau, string gioKetThuc)
        {
            if (string.IsNullOrWhiteSpace(gioBatDau) || string.IsNullOrWhiteSpace(gioKetThuc)) return 0;

            if (TimeSpan.TryParse(gioBatDau.Trim(), out var t1) && TimeSpan.TryParse(gioKetThuc.Trim(), out var t2))
            {
                if (t2 >= t1)
                {
                    return Math.Round((t2 - t1).TotalHours, 2);
                }
                else
                {
                    // Ca qua đêm (qua ngày hôm sau)
                    return Math.Round((t2.Add(TimeSpan.FromHours(24)) - t1).TotalHours, 2);
                }
            }

            return 0;
        }

        /// <summary>
        /// Xác định nhân viên đang là thử việc hay chính thức theo hợp đồng lao động
        /// </summary>
        public bool KiemTraThuViec(int manv)
        {
            var hd = db.TB_HOPDONG
                       .Where(x => x.MANV == manv)
                       .OrderByDescending(x => x.NGAYBATDAU)
                       .FirstOrDefault();

            if (hd != null)
            {
                if (hd.LOAIHD == 1)
                {
                    return true;
                }

                string thoiHan = (hd.THOIHAN ?? "").ToLower();
                if (thoiHan.Contains("thử việc") || thoiHan.Contains("thu viec") || thoiHan.Contains("thử"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Mức tiền tính 1 giờ Overtime theo quy tắc lương của hệ thống (lương cơ bản + phụ cấp theo lương) / (26 ngày * 8 giờ)
        /// </summary>
        public decimal GetMucLuong1GioOT(int manv, int nam, int thang)
        {
            var hd = db.TB_HOPDONG
                       .Where(x => x.MANV == manv && x.NGAYBATDAU <= DateTime.Now)
                       .OrderByDescending(x => x.NGAYBATDAU)
                       .FirstOrDefault();

            decimal luongCoBan = 0;
            if (hd != null && hd.LUONG_THOA_THUAN != null && hd.LUONG_THOA_THUAN > 0)
            {
                luongCoBan = (decimal)hd.LUONG_THOA_THUAN;
            }
            else if (hd != null && hd.HESOLUONG != null && hd.HESOLUONG > 0)
            {
                if (hd.HESOLUONG > 100) luongCoBan = (decimal)hd.HESOLUONG;
                else luongCoBan = (decimal)hd.HESOLUONG * 1800000m;
            }

            if (luongCoBan <= 0)
            {
                luongCoBan = 4425000m;
            }

            var phuCaps = db.TB_NHANVIEN_PHUCAP
                            .Where(x => x.MANV == manv)
                            .ToList();

            Func<int, decimal> getPc = id =>
            {
                var p = phuCaps.FirstOrDefault(x => x.IDPC == id);
                return p?.SOTIEN ?? 0;
            };

            // Phụ cấp tính vào lương làm căn cứ tính OT theo quy định
            decimal pcChucVu = getPc(5);
            decimal pcThamNien = getPc(10);
            decimal pcKyNang = getPc(7);
            decimal pcNhaO = getPc(1);
            decimal pcDiLai = getPc(2);

            decimal congChuan = 26.0m;
            decimal otRate = (luongCoBan + pcChucVu + pcThamNien + pcKyNang + pcNhaO + pcDiLai) / (congChuan * 8.0m);

            return Math.Round(otRate, 2);
        }

        /// <summary>
        /// Tính toán chi tiết tăng ca: số giờ, đơn giá 1 giờ, hệ số tăng ca (từ bảng cấu hình), trạng thái thử việc và thành tiền
        /// </summary>
        public TangCaCalcResult TinhChiTietTangCa(int manv, DateTime ngay, int idLoaiCa, int idLoaiCong, string gioBatDau, string gioKetThuc, bool? isThuViecManual = null)
        {
            var result = new TangCaCalcResult();

            result.SoGio = TinhSoGio(gioBatDau, gioKetThuc);
            result.IsThuViec = isThuViecManual.HasValue ? isThuViecManual.Value : KiemTraThuViec(manv);
            result.DonGia1Gio = GetMucLuong1GioOT(manv, ngay.Year, ngay.Month);

            var quyDinh = _heSoTangCaBus.TimQuyDinh(idLoaiCong, idLoaiCa, gioBatDau, gioKetThuc);
            if (quyDinh != null)
            {
                result.TenQuyDinh = quyDinh.TEN_QUYDINH;
                result.HeSo = result.IsThuViec ? quyDinh.HESO_THUVIEC : quyDinh.HESO_CHINHTHUC;
            }
            else
            {
                result.TenQuyDinh = "Tăng ca tiêu chuẩn";
                result.HeSo = result.IsThuViec ? 1.275m : 1.50m;
            }

            result.ThanhTien = Math.Round((decimal)result.SoGio * result.DonGia1Gio * result.HeSo, 0);

            return result;
        }

        public TB_TANGCA Add(TB_TANGCA tc)
        {
            try
            {
                if (!tc.SOGIO.HasValue || tc.SOGIO.Value <= 0)
                {
                    throw new Exception("Số giờ tăng ca phải lớn hơn 0.");
                }

                db.TB_TANGCA.Add(tc);
                db.SaveChanges();
                return tc;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thêm tăng ca: " + ex.Message);
            }
        }

        public TB_TANGCA Update(TB_TANGCA tc)
        {
            try
            {
                if (!tc.SOGIO.HasValue || tc.SOGIO.Value <= 0)
                {
                    throw new Exception("Số giờ tăng ca phải lớn hơn 0.");
                }

                var _tc = db.TB_TANGCA.FirstOrDefault(x => x.IDTCA == tc.IDTCA);
                if (_tc == null)
                {
                    throw new Exception("Không tìm thấy bản ghi tăng ca ID: " + tc.IDTCA);
                }

                _tc.MANV = tc.MANV;
                _tc.IDLOAICA = tc.IDLOAICA;
                _tc.IDLOAICONG = tc.IDLOAICONG;
                _tc.NAM = tc.NAM;
                _tc.THANG = tc.THANG;
                _tc.NGAY = tc.NGAY;
                _tc.GIOBATDAU = tc.GIOBATDAU;
                _tc.GIOKETTHUC = tc.GIOKETTHUC;
                _tc.SOGIO = tc.SOGIO;
                _tc.HESOTC = tc.HESOTC;
                _tc.DONGIATC = tc.DONGIATC;
                _tc.IS_THUVIEC = tc.IS_THUVIEC;
                _tc.SOTIENTC = tc.SOTIENTC;
                _tc.GHICHU = tc.GHICHU;
                _tc.UPDATED_BY = tc.UPDATED_BY;
                _tc.UPDATED_DATE = tc.UPDATED_DATE;

                db.SaveChanges();
                return tc;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi cập nhật tăng ca: " + ex.Message);
            }
        }

        public void Delete(int idtc, int iduser)
        {
            var _lc = db.TB_TANGCA.FirstOrDefault(x => x.IDTCA == idtc);
            if (_lc != null)
            {
                _lc.DELETED_BY = iduser;
                _lc.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
        }
    }
}
