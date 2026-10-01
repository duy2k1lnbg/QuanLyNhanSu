using Bu.CLASS_CHAMCONG;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    public class KhungGioInputDto
    {
        public int Stt { get; set; }
        public int BatDauPhut { get; set; }
        public int KetThucPhut { get; set; }
        public string LoaiKhungGio { get; set; }
        public bool BatBuocQuetThe { get; set; }
    }

    public class CreateCaPhienBanRequest
    {
        public decimal IdLoaiCa { get; set; }
        public string TenPhienBan { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public long TongGiayChuan { get; set; }
        public decimal CongQuyDoi { get; set; }
        public List<KhungGioInputDto> KhungGioList { get; set; }
    }

    public class BulkScheduleItemInput
    {
        public decimal Manv { get; set; }
        public DateTime Ngay { get; set; }
        public long IdCaPhienBan { get; set; }
        public long? IdQuyDinh { get; set; }
        public string LoaiNgay { get; set; } = "NGAY_THUONG";
        public string GhiChu { get; set; }
    }

    public class BulkScheduleRequest
    {
        public int MaKyCong { get; set; }
        public List<BulkScheduleItemInput> Schedules { get; set; }
    }

    public class ScheduleLineValidationResult
    {
        public int LineIndex { get; set; }
        public decimal Manv { get; set; }
        public string EmployeeName { get; set; }
        public string Ngay { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PublishAttendanceApiRequest
    {
        public int MaKyCong { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public List<long> ManvList { get; set; }
        public string GhiChu { get; set; }
        public bool ForceRecalculate { get; set; }
    }

    public class ResolveAnomalyApiRequest
    {
        public string Action { get; set; }
        public string Reason { get; set; }
    }

    public class LockKyCongRequest
    {
        public string LyDo { get; set; }
    }

    [JwtAuthorize]
    [RoutePrefix("api/chamcong")]
    public class ChamCongController : ApiController
    {
        private readonly BANGCONG_NV_CHITIET _bcChiTietBus = new BANGCONG_NV_CHITIET();

        /// <summary>
        /// GET: api/chamcong/kycong
        /// Lấy danh sách toàn bộ các kỳ công chấm công
        /// </summary>
        [HttpGet]
        [Route("kycong")]
        public IHttpActionResult GetKyCong()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_KYCONG
                        .OrderByDescending(x => x.MAKYCONG)
                        .Select(x => new
                        {
                            MAKYCONG = (int)x.MAKYCONG,
                            THANG = (int?)x.THANG,
                            NAM = (int?)x.NAM,
                            KHOA = (int?)x.KHOA,
                            NGAYCONGTRONGTHANG = (int?)x.NGAYCONGTRONGTHANG,
                            TRANGTHAI = (int?)x.TRANGTHAI,
                            NGAYTINHCONG = x.NGAYTINHCONG
                        })
                        .ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách kỳ công." });
            }
        }

        /// <summary>
        /// GET: api/chamcong/chitiet?makycong={makycong} hoặc api/chamcong/kycongchitiet
        /// Lấy bảng chấm công chi tiết theo ngày (D1..D31) của tất cả nhân viên trong kỳ
        /// </summary>
        [HttpGet]
        [Route("chitiet")]
        [Route("kycongchitiet")]
        public IHttpActionResult GetKyCongChiTiet(int makycong = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (makycong <= 0)
                    {
                        var latest = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                        if (latest != null)
                        {
                            makycong = (int)latest.MAKYCONG;
                        }
                    }

                    // Danh sách nhân sự từ TB_NHANVIEN
                    var nvList = db.TB_NHANVIEN
                        .Where(nv => nv.DELETED_DATE == null)
                        .Select(nv => new
                        {
                            nv.MANV,
                            nv.HOTEN,
                            nv.IDPB,
                            nv.DATHOIVIEC,
                            nv.DELETED_DATE
                        })
                        .ToList();

                    var nvDict = nvList.ToDictionary(k => k.MANV, v => v);

                    var list = db.TB_KYCONGCHITIET
                        .Where(kc => makycong <= 0 || kc.MAKYCONG == makycong)
                        .OrderBy(kc => kc.MANV)
                        .ToList();

                    // Lọc bản ghi thuộc phạm vi nhân viên hợp lệ
                    list = list.Where(kc => nvDict.ContainsKey(kc.MANV)).ToList();

                    var pbDict = db.TB_PHONGBAN
                        .Select(pb => new { pb.IDPB, pb.TENPB })
                        .ToDictionary(k => k.IDPB, v => v.TENPB);

                    var result = list.Select(item =>
                    {
                        string hoTen = item.HOTEN;
                        string tenPb = "Chưa phân phòng";
                        decimal? daThoiViec = null;
                        bool isActive = false;

                        if (nvDict.TryGetValue(item.MANV, out var nv))
                        {
                            if (!string.IsNullOrWhiteSpace(nv.HOTEN)) hoTen = nv.HOTEN;
                            daThoiViec = nv.DATHOIVIEC;
                            isActive = (nv.DATHOIVIEC == null || nv.DATHOIVIEC == 0) && nv.DELETED_DATE == null;
                            if (nv.IDPB.HasValue && pbDict.TryGetValue(nv.IDPB.Value, out var pbName))
                            {
                                tenPb = pbName;
                            }
                        }

                        return new
                        {
                            item.MAKYCONG,
                            item.MANV,
                            HOTEN = hoTen,
                            TENPB = tenPb,
                            DATHOIVIEC = daThoiViec,
                            IS_ACTIVE = isActive,
                            item.D1, item.D2, item.D3, item.D4, item.D5, item.D6, item.D7, item.D8, item.D9, item.D10,
                            item.D11, item.D12, item.D13, item.D14, item.D15, item.D16, item.D17, item.D18, item.D19, item.D20,
                            item.D21, item.D22, item.D23, item.D24, item.D25, item.D26, item.D27, item.D28, item.D29, item.D30,
                            item.D31,
                            item.NGAYCONG,
                            item.NGAYPHEP,
                            item.NGHIKHONGPHEP,
                            item.CONGNGAYLE,
                            item.CONGCHUNHAT,
                            item.TONGNGAYCONG
                        };
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải bảng chấm công chi tiết: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải bảng chấm công chi tiết." });
            }
        }

        /// <summary>
        /// GET: api/chamcong/nhanvien?makycong={makycong}&manv={manv}
        /// Lấy chi tiết chấm công từng ngày của một nhân viên trong kỳ công
        /// </summary>
        [HttpGet]
        [Route("nhanvien")]
        public IHttpActionResult GetBangCongNhanVien(int makycong, int manv)
        {
            try
            {
                var list = _bcChiTietBus.getBangCongCT(makycong, manv);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải chi tiết chấm công nhân viên #" + manv + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải chi tiết chấm công của nhân viên." });
            }
        }

        /// <summary>
        /// GET: api/chamcong/loaica
        /// Danh mục các loại ca làm việc
        /// </summary>
        [HttpGet]
        [Route("loaica")]
        public IHttpActionResult GetLoaiCa()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_LOAICA.Select(x => new
                    {
                        IDLOAICA = (int)x.IDLOAICA,
                        TENLOAICA = x.TENLOAICA,
                        HESOLOAICA = x.HESOLOAICA
                    }).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục loại ca: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục loại ca." });
            }
        }

        /// <summary>
        /// GET: api/chamcong/loaicong
        /// Danh mục các loại ngày công
        /// </summary>
        [HttpGet]
        [Route("loaicong")]
        public IHttpActionResult GetLoaiCong()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_LOAICONG.Select(x => new
                    {
                        IDLOAICONG = (int)x.IDLOAICONG,
                        TENLOAICONG = x.TENLC,
                        HESOLOAICONG = x.HESOLOAICONG
                    }).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục loại công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục loại công." });
            }
        }

        #region Helper Identity

        private decimal GetCurrentUserId(MyEntities db)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                if (jwtUser != null && int.TryParse(jwtUser.UserId, out int uId))
                {
                    return (decimal)uId;
                }
                string uname = jwtUser?.Username ?? "admin";
                var user = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME.ToLower() == uname.ToLower());
                return user?.IDUSER ?? 1;
            }
            catch
            {
                return 1;
            }
        }

        #endregion

        #region Ca và Phiên bản Ca (Shift Versions & Time Intervals)

        /// <summary>
        /// GET: api/chamcong/ca-phienban
        /// Lấy danh sách các phiên bản cấu hình ca làm việc
        /// </summary>
        [HttpGet]
        [Route("ca-phienban")]
        public IHttpActionResult GetCaPhienBan()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = (from pb in db.TB_CA_PHIENBAN
                                join ca in db.TB_LOAICA on pb.IDLOAICA equals ca.IDLOAICA
                                orderby pb.IDLOAICA, pb.SO_PHIENBAN descending
                                select new
                                {
                                    idCaPhienBan = pb.IDCAPHIENBAN,
                                    idLoaiCa = pb.IDLOAICA,
                                    tenLoaiCa = ca.TENLOAICA,
                                    soPhienBan = pb.SO_PHIENBAN,
                                    tenPhienBan = pb.TEN_PHIENBAN,
                                    tuNgay = pb.TU_NGAY,
                                    denNgay = pb.DEN_NGAY,
                                    tongGiayChuan = pb.TONG_GIAY_CHUAN,
                                    gioChuan = Math.Round(pb.TONG_GIAY_CHUAN / 3600.0, 2),
                                    congQuyDoi = pb.CONG_QUY_DOI,
                                    trangThai = pb.TRANG_THAI,
                                    taoLuc = pb.TAO_LUC
                                }).ToList();

                    return Ok(new
                    {
                        success = true,
                        data = list.Select(x => new
                        {
                            x.idCaPhienBan,
                            x.idLoaiCa,
                            x.tenLoaiCa,
                            x.soPhienBan,
                            x.tenPhienBan,
                            tuNgay = x.tuNgay.ToString("dd/MM/yyyy"),
                            denNgay = x.denNgay.HasValue ? x.denNgay.Value.ToString("dd/MM/yyyy") : "Vô thời hạn",
                            x.tongGiayChuan,
                            x.gioChuan,
                            x.congQuyDoi,
                            x.trangThai,
                            taoLuc = x.taoLuc.ToString("dd/MM/yyyy HH:mm")
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi GetCaPhienBan: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải phiên bản ca." });
            }
        }

        /// <summary>
        /// GET: api/chamcong/ca-phienban/{id}
        /// Chi tiết phiên bản ca kèm các khung giờ (làm việc, nghỉ)
        /// </summary>
        [HttpGet]
        [Route("ca-phienban/{id:long}")]
        public IHttpActionResult GetCaPhienBanDetail(long id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var pb = db.TB_CA_PHIENBAN.FirstOrDefault(p => p.IDCAPHIENBAN == id);
                    if (pb == null) return NotFound();

                    var loaica = db.TB_LOAICA.FirstOrDefault(c => c.IDLOAICA == pb.IDLOAICA);
                    var khungGio = db.TB_CA_KHUNGGIO
                        .Where(kg => kg.IDCAPHIENBAN == id)
                        .OrderBy(kg => kg.STT)
                        .Select(kg => new
                        {
                            idKhungGio = kg.IDKHUNGGIO,
                            stt = kg.STT,
                            batDauPhut = kg.BATDAU_PHUT,
                            ketThucPhut = kg.KETTHUC_PHUT,
                            gioBatDau = string.Format("{0:D2}:{1:D2}", kg.BATDAU_PHUT / 60, kg.BATDAU_PHUT % 60),
                            gioKetThuc = string.Format("{0:D2}:{1:D2}", kg.KETTHUC_PHUT / 60, kg.KETTHUC_PHUT % 60),
                            loaiKhungGio = kg.LOAI_KHUNGGIO,
                            batBuocQuetThe = kg.BAT_BUOC_QUET_THE
                        })
                        .ToList();

                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            idCaPhienBan = pb.IDCAPHIENBAN,
                            idLoaiCa = pb.IDLOAICA,
                            tenLoaiCa = loaica?.TENLOAICA,
                            soPhienBan = pb.SO_PHIENBAN,
                            tenPhienBan = pb.TEN_PHIENBAN,
                            tuNgay = pb.TU_NGAY.ToString("dd/MM/yyyy"),
                            denNgay = pb.DEN_NGAY.HasValue ? pb.DEN_NGAY.Value.ToString("dd/MM/yyyy") : "Vô thời hạn",
                            tongGiayChuan = pb.TONG_GIAY_CHUAN,
                            gioChuan = Math.Round(pb.TONG_GIAY_CHUAN / 3600.0, 2),
                            congQuyDoi = pb.CONG_QUY_DOI,
                            trangThai = pb.TRANG_THAI,
                            khungGio = khungGio
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi GetCaPhienBanDetail: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải chi tiết phiên bản ca." });
            }
        }

        /// <summary>
        /// POST: api/chamcong/ca-phienban
        /// Thêm mới phiên bản ca (tự sinh số version tiếp theo, chống sửa đè lịch sử)
        /// </summary>
        [HttpPost]
        [Route("ca-phienban")]
        public IHttpActionResult CreateCaPhienBan([FromBody] CreateCaPhienBanRequest req)
        {
            if (req == null) return BadRequest("Dữ liệu cấu hình ca không hợp lệ.");
            if (string.IsNullOrWhiteSpace(req.TenPhienBan)) return BadRequest("Tên phiên bản ca không được để trống.");
            if (req.KhungGioList == null || req.KhungGioList.Count == 0) return BadRequest("Ca phải có ít nhất một khung giờ làm việc.");

            var sorted = req.KhungGioList.OrderBy(k => k.BatDauPhut).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].KetThucPhut <= sorted[i].BatDauPhut)
                    return BadRequest($"Khung giờ thứ {i + 1} có phút kết thúc ({sorted[i].KetThucPhut}) phải lớn hơn phút bắt đầu ({sorted[i].BatDauPhut}).");
                if (i > 0 && sorted[i].BatDauPhut < sorted[i - 1].KetThucPhut)
                    return BadRequest($"Khung giờ thứ {i + 1} bị chồng lấn với khung giờ trước đó.");
            }

            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);
                    var ca = db.TB_LOAICA.FirstOrDefault(c => c.IDLOAICA == req.IdLoaiCa);
                    if (ca == null) return BadRequest("Loại ca không tồn tại trong danh mục.");

                    int maxVer = db.TB_CA_PHIENBAN.Where(p => p.IDLOAICA == req.IdLoaiCa).Select(p => (int?)p.SO_PHIENBAN).Max() ?? 0;
                    int nextVer = maxVer + 1;

                    using (var trans = db.Database.BeginTransaction())
                    {
                        var newPb = new TB_CA_PHIENBAN
                        {
                            IDLOAICA = req.IdLoaiCa,
                            SO_PHIENBAN = nextVer,
                            TEN_PHIENBAN = req.TenPhienBan.Trim(),
                            TU_NGAY = req.TuNgay.Date,
                            DEN_NGAY = req.DenNgay?.Date,
                            TONG_GIAY_CHUAN = req.TongGiayChuan > 0 ? req.TongGiayChuan : 28800,
                            CONG_QUY_DOI = req.CongQuyDoi > 0 ? req.CongQuyDoi : 1.0m,
                            TRANG_THAI = "PUBLISHED",
                            TAO_LUC = DateTime.Now,
                            TAO_BOI = userId
                        };
                        db.TB_CA_PHIENBAN.Add(newPb);
                        db.SaveChanges();

                        int stt = 1;
                        foreach (var kg in sorted)
                        {
                            db.TB_CA_KHUNGGIO.Add(new TB_CA_KHUNGGIO
                            {
                                IDCAPHIENBAN = newPb.IDCAPHIENBAN,
                                STT = stt++,
                                BATDAU_PHUT = kg.BatDauPhut,
                                KETTHUC_PHUT = kg.KetThucPhut,
                                LOAI_KHUNGGIO = string.IsNullOrWhiteSpace(kg.LoaiKhungGio) ? "LAM_VIEC" : kg.LoaiKhungGio,
                                BAT_BUOC_QUET_THE = kg.BatBuocQuetThe
                            });
                        }
                        db.SaveChanges();
                        trans.Commit();

                        return Ok(new { success = true, message = $"Đã tạo mới phiên bản ca v{nextVer} thành công!", idCaPhienBan = newPb.IDCAPHIENBAN });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi CreateCaPhienBan: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi lưu cấu hình phiên bản ca." });
            }
        }

        #endregion

        #region Lịch làm việc & Phân ca hàng loạt (Work Scheduling)

        /// <summary>
        /// GET: api/chamcong/lich-lamviec
        /// Tra cứu lịch làm việc theo kỳ công, nhân viên, hoặc khoảng ngày
        /// </summary>
        [HttpGet]
        [Route("lich-lamviec")]
        public IHttpActionResult GetLichLamViec(int? makycong = null, decimal? manv = null, DateTime? tuNgay = null, DateTime? denNgay = null, int page = 1, int pageSize = 100)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var query = db.TB_LICH_LAMVIEC.AsQueryable();

                    if (manv.HasValue)
                    {
                        query = query.Where(l => l.MANV == manv.Value);
                    }

                    if (tuNgay.HasValue && denNgay.HasValue)
                    {
                        var from = tuNgay.Value.Date;
                        var to = denNgay.Value.Date;
                        query = query.Where(l => l.NGAY >= from && l.NGAY <= to);
                    }
                    else if (makycong.HasValue)
                    {
                        int y = makycong.Value / 100;
                        int m = makycong.Value % 100;
                        var from = new DateTime(y, m, 1);
                        var to = from.AddMonths(1).AddDays(-1);
                        query = query.Where(l => l.NGAY >= from && l.NGAY <= to);
                    }

                    int total = query.Count();
                    var paged = (from l in query
                                 join nv in db.TB_NHANVIEN on l.MANV equals nv.MANV
                                 join pb in db.TB_CA_PHIENBAN on l.IDCAPHIENBAN equals pb.IDCAPHIENBAN into pbJoin
                                 from pb in pbJoin.DefaultIfEmpty()
                                 orderby l.NGAY descending, l.MANV
                                 select new
                                 {
                                     idLich = l.IDLICH,
                                     manv = l.MANV,
                                     employeeCode = nv.EMPLOYEE_CODE,
                                     hoten = nv.HOTEN,
                                     ngay = l.NGAY,
                                     maPhanCong = l.MA_PHANCONG,
                                     soPhienBan = l.SO_PHIENBAN,
                                     idCaPhienBan = l.IDCAPHIENBAN,
                                     tenPhienBan = pb != null ? pb.TEN_PHIENBAN : "N/A",
                                     loaiNgay = l.LOAI_NGAY,
                                     trangThaiPhanCong = l.TRANG_THAI_PHAN_CONG,
                                     trangThai = l.TRANG_THAI,
                                     lyDo = l.LY_DO
                                 })
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = paged.Select(x => new
                        {
                            x.idLich,
                            x.manv,
                            x.employeeCode,
                            x.hoten,
                            ngay = x.ngay.ToString("dd/MM/yyyy"),
                            x.maPhanCong,
                            x.soPhienBan,
                            x.idCaPhienBan,
                            x.tenPhienBan,
                            x.loaiNgay,
                            x.trangThaiPhanCong,
                            x.trangThai,
                            x.lyDo
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi GetLichLamViec: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải lịch làm việc." });
            }
        }

        /// <summary>
        /// POST: api/chamcong/lich-lamviec/preview
        /// Xem trước và kiểm tra tính hợp lệ của phân ca hàng loạt từng dòng (Dry-run validation)
        /// </summary>
        [HttpPost]
        [Route("lich-lamviec/preview")]
        public IHttpActionResult PreviewBulkSchedule([FromBody] BulkScheduleRequest req)
        {
            if (req == null || req.Schedules == null || req.Schedules.Count == 0)
                return BadRequest("Không có dữ liệu phân ca cần kiểm tra.");

            try
            {
                using (var db = new MyEntities())
                {
                    var kyCong = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == req.MaKyCong);
                    if (kyCong == null)
                        return BadRequest($"Không tìm thấy kỳ công {req.MaKyCong}.");

                    bool isPeriodLocked = (kyCong.KHOA ?? 0) != 0;

                    var activeEmployees = db.TB_NHANVIEN
                        .Where(nv => nv.DELETED_DATE == null)
                        .Select(nv => new { nv.MANV, nv.HOTEN })
                        .ToDictionary(k => k.MANV, v => v.HOTEN);

                    var shiftVersions = db.TB_CA_PHIENBAN
                        .Select(p => new { p.IDCAPHIENBAN, p.TU_NGAY, p.DEN_NGAY, p.TRANG_THAI })
                        .ToDictionary(k => k.IDCAPHIENBAN, v => v);

                    var lineResults = new List<ScheduleLineValidationResult>();
                    int lineIdx = 1;

                    foreach (var item in req.Schedules)
                    {
                        var res = new ScheduleLineValidationResult
                        {
                            LineIndex = lineIdx++,
                            Manv = item.Manv,
                            Ngay = item.Ngay.ToString("dd/MM/yyyy"),
                            IsValid = true
                        };

                        if (isPeriodLocked)
                        {
                            res.IsValid = false;
                            res.ErrorMessage = $"Kỳ công {req.MaKyCong} đã bị khóa, không thể phân ca.";
                        }
                        else if (!activeEmployees.TryGetValue(item.Manv, out var empName))
                        {
                            res.IsValid = false;
                            res.ErrorMessage = $"Nhân viên #{item.Manv} không tồn tại hoặc đã bị xóa.";
                        }
                        else
                        {
                            res.EmployeeName = empName;
                            if (!shiftVersions.TryGetValue(item.IdCaPhienBan, out var shift))
                            {
                                res.IsValid = false;
                                res.ErrorMessage = $"Phiên bản ca #{item.IdCaPhienBan} không tồn tại.";
                            }
                            else if (item.Ngay.Date < shift.TU_NGAY.Date || (shift.DEN_NGAY.HasValue && item.Ngay.Date >= shift.DEN_NGAY.Value.Date))
                            {
                                res.IsValid = false;
                                res.ErrorMessage = $"Ngày phân ca ({res.Ngay}) nằm ngoài hiệu lực của phiên bản ca.";
                            }
                            else
                            {
                                var existing = db.TB_LICH_LAMVIEC.Any(l => l.MANV == item.Manv && DbFunctions.TruncateTime(l.NGAY) == item.Ngay.Date);
                                if (existing)
                                {
                                    res.IsValid = false;
                                    res.ErrorMessage = $"Đã tồn tại lịch làm việc cho nhân viên này vào ngày {res.Ngay}.";
                                }
                            }
                        }

                        lineResults.Add(res);
                    }

                    int validCount = lineResults.Count(l => l.IsValid);
                    int invalidCount = lineResults.Count(l => !l.IsValid);

                    return Ok(new
                    {
                        success = true,
                        totalRows = lineResults.Count,
                        validCount = validCount,
                        invalidCount = invalidCount,
                        canProceed = invalidCount == 0,
                        lineDetails = lineResults
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi PreviewBulkSchedule: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi kiểm tra dữ liệu phân ca." });
            }
        }

        /// <summary>
        /// POST: api/chamcong/lich-lamviec/bulk
        /// Phân ca hàng loạt chính thức, bảo đảm nguyên tử và ghi vết kiểm toán
        /// </summary>
        [HttpPost]
        [Route("lich-lamviec/bulk")]
        public IHttpActionResult ApplyBulkSchedule([FromBody] BulkScheduleRequest req)
        {
            if (req == null || req.Schedules == null || req.Schedules.Count == 0)
                return BadRequest("Không có dữ liệu phân ca để thực hiện.");

            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);
                    var kyCong = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == req.MaKyCong);
                    if (kyCong == null)
                        return BadRequest($"Không tìm thấy kỳ công {req.MaKyCong}.");

                    if ((kyCong.KHOA ?? 0) != 0)
                        return BadRequest($"Kỳ công {req.MaKyCong} đã bị khóa, không thể phân ca.");

                    using (var trans = db.Database.BeginTransaction())
                    {
                        int inserted = 0;
                        foreach (var item in req.Schedules)
                        {
                            var existing = db.TB_LICH_LAMVIEC.FirstOrDefault(l => l.MANV == item.Manv && DbFunctions.TruncateTime(l.NGAY) == item.Ngay.Date);
                            if (existing != null)
                            {
                                existing.IDCAPHIENBAN = item.IdCaPhienBan;
                                existing.LOAI_NGAY = item.LoaiNgay ?? "NGAY_THUONG";
                                existing.LY_DO = item.GhiChu;
                                existing.SO_PHIENBAN += 1;
                            }
                            else
                            {
                                string maPhanCong = $"SCHED_{item.Manv}_{item.Ngay:yyyyMMdd}_{DateTime.Now:HHmmss}";
                                db.TB_LICH_LAMVIEC.Add(new TB_LICH_LAMVIEC
                                {
                                    MANV = item.Manv,
                                    NGAY = item.Ngay.Date,
                                    MA_PHANCONG = maPhanCong.Length > 100 ? maPhanCong.Substring(0, 100) : maPhanCong,
                                    SO_PHIENBAN = 1,
                                    IDCAPHIENBAN = item.IdCaPhienBan,
                                    IDQUYDINH = item.IdQuyDinh,
                                    TRANG_THAI_PHAN_CONG = "DA_PHAN",
                                    LOAI_NGAY = item.LoaiNgay ?? "NGAY_THUONG",
                                    TRANG_THAI = "PUBLISHED",
                                    NGUON_PHAN_CONG = "PORTAL",
                                    TAO_LUC = DateTime.Now,
                                    TAO_BOI = userId,
                                    LY_DO = item.GhiChu
                                });
                            }
                            inserted++;
                        }

                        // Tăng revision đầu vào của kỳ công để công bố nhận biết dữ liệu mới
                        kyCong.CONG_INPUT_REV = kyCong.CONG_INPUT_REV + 1;

                        db.SaveChanges();
                        trans.Commit();

                        return Ok(new
                        {
                            success = true,
                            message = $"Đã cập nhật lịch làm việc thành công cho {inserted} dòng phân công.",
                            count = inserted
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi ApplyBulkSchedule: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phân ca hàng loạt." });
            }
        }

        #endregion

        #region Công bố công & Quản lý lần tính (Attendance Publishing & Recalculation)

        /// <summary>
        /// POST: api/chamcong/publish
        /// Kích hoạt AttendancePublishingService để tính toán, phân đoạn công và công bố kết quả
        /// </summary>
        [HttpPost]
        [Route("publish")]
        public IHttpActionResult PublishAttendance([FromBody] PublishAttendanceApiRequest req)
        {
            if (req == null || req.MaKyCong <= 0)
                return BadRequest("Mã kỳ công không hợp lệ.");

            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);

                    var pubService = new AttendancePublishingService();
                    var pubResult = pubService.PublishAttendance(new AttendancePublishRequest
                    {
                        MaKyCong = req.MaKyCong,
                        TuNgay = req.TuNgay,
                        DenNgay = req.DenNgay,
                        ManvList = req.ManvList,
                        GhiChu = req.GhiChu,
                        ForceRecalculate = req.ForceRecalculate,
                        NguoiThucHien = (long)userId
                    });

                    if (!pubResult.Success)
                    {
                        return Content(HttpStatusCode.BadRequest, new
                        {
                            success = false,
                            message = pubResult.ErrorMessage,
                            details = pubResult.Details
                        });
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Công bố bảng chấm công thành công!",
                        data = pubResult
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi PublishAttendance: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi công bố bảng chấm công: " + ex.Message });
            }
        }

        /// <summary>
        /// GET: api/chamcong/lan-tinh
        /// Lịch sử các lần tính và công bố bảng chấm công
        /// </summary>
        [HttpGet]
        [Route("lan-tinh")]
        public IHttpActionResult GetLanTinhList(int? makycong = null)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var query = db.TB_CHAMCONG_LANTINH.AsQueryable();
                    if (makycong.HasValue)
                    {
                        query = query.Where(l => l.MAKYCONG == makycong.Value);
                    }

                    var list = query
                        .OrderByDescending(l => l.IDLANTINH)
                        .Take(50)
                        .Select(l => new
                        {
                            idLanTinh = l.IDLANTINH,
                            maYeuCau = l.MA_YEU_CAU,
                            maKyCong = l.MAKYCONG,
                            tuNgay = l.TU_NGAY,
                            denNgay = l.DEN_NGAY,
                            inputRev = l.INPUT_REV,
                            expectedPublishRev = l.EXPECTED_PUBLISH_REV,
                            inputDataHash = l.INPUT_DATA_HASH,
                            trangThai = l.TRANG_THAI,
                            batDauTinh = l.BATDAU_TINH,
                            ketThucTinh = l.KETTHUC_TINH,
                            congBoLuc = l.CONGBO_LUC,
                            ghiChu = l.GHI_CHU
                        })
                        .ToList();

                    return Ok(new
                    {
                        success = true,
                        data = list.Select(x => new
                        {
                            x.idLanTinh,
                            x.maYeuCau,
                            x.maKyCong,
                            tuNgay = x.tuNgay.ToString("dd/MM/yyyy"),
                            denNgay = x.denNgay.ToString("dd/MM/yyyy"),
                            x.inputRev,
                            x.expectedPublishRev,
                            x.inputDataHash,
                            x.trangThai,
                            batDauTinh = x.batDauTinh.ToString("dd/MM/yyyy HH:mm:ss"),
                            ketThucTinh = x.ketThucTinh.HasValue ? x.ketThucTinh.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                            congBoLuc = x.congBoLuc.HasValue ? x.congBoLuc.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                            x.ghiChu
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi GetLanTinhList: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải lịch sử tính công." });
            }
        }

        #endregion

        #region Bất thường & Lịch sử quyết định (Anomalies & Historical Decisions)

        /// <summary>
        /// GET: api/chamcong/bat-thuong
        /// Danh sách các bất thường công cần xác minh hoặc giải trình
        /// </summary>
        [HttpGet]
        [Route("bat-thuong")]
        public IHttpActionResult GetBatThuongList(int? makycong = null, decimal? manv = null, string status = null)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var query = db.TB_CHAMCONG_BATTHUONG.AsQueryable();

                    if (manv.HasValue)
                    {
                        query = query.Where(b => b.MANV == manv.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
                    {
                        query = query.Where(b => b.TRANG_THAI == status.ToUpper());
                    }

                    if (makycong.HasValue)
                    {
                        int y = makycong.Value / 100;
                        int m = makycong.Value % 100;
                        var from = new DateTime(y, m, 1);
                        var to = from.AddMonths(1).AddDays(-1);
                        query = query.Where(b => b.NGAY >= from && b.NGAY <= to);
                    }

                    var raw = (from b in query
                               join nv in db.TB_NHANVIEN on b.MANV equals nv.MANV
                               orderby b.NGAY descending, b.MANV
                               select new
                               {
                                   idBatThuong = b.IDBATTHUONG,
                                   manv = b.MANV,
                                   employeeCode = nv.EMPLOYEE_CODE,
                                   hoten = nv.HOTEN,
                                   ngay = b.NGAY,
                                   maLoi = b.MA_LOI,
                                   mucDo = b.MUC_DO,
                                   chanChot = b.CHAN_CHOT,
                                   trangThai = b.TRANG_THAI,
                                   moTa = b.MO_TA,
                                   taoLuc = b.TAO_LUC
                               }).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = raw.Count,
                        data = raw.Select(x => new
                        {
                            x.idBatThuong,
                            x.manv,
                            x.employeeCode,
                            x.hoten,
                            ngay = x.ngay.ToString("dd/MM/yyyy"),
                            x.maLoi,
                            x.mucDo,
                            x.chanChot,
                            x.trangThai,
                            x.moTa,
                            taoLuc = x.taoLuc.ToString("dd/MM/yyyy HH:mm")
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi GetBatThuongList: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách bất thường công." });
            }
        }

        /// <summary>
        /// POST: api/chamcong/bat-thuong/{id}/resolve
        /// Xử lý quyết định bất thường công (Append-only vào TB_CHAMCONG_BT_LICHSU)
        /// </summary>
        [HttpPost]
        [Route("bat-thuong/{id:long}/resolve")]
        public IHttpActionResult ResolveBatThuong(long id, [FromBody] ResolveAnomalyApiRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Action))
                return BadRequest("Hành động xử lý không hợp lệ.");

            if (string.IsNullOrWhiteSpace(req.Reason))
                return BadRequest("Lý do xử lý bắt buộc phải nhập để lưu vết kiểm toán.");

            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);
                    var bt = db.TB_CHAMCONG_BATTHUONG.FirstOrDefault(b => b.IDBATTHUONG == id);
                    if (bt == null) return NotFound();

                    // Xác định trạng thái mới
                    string newStatus = "DA_XAC_MINH";
                    if (req.Action.ToUpper() == "IGNORE" || req.Action.ToUpper() == "BO_QUA")
                        newStatus = "BO_QUA";
                    else if (req.Action.ToUpper() == "REJECT" || req.Action.ToUpper() == "TU_CHOI")
                        newStatus = "TU_CHOI";

                    string oldStatus = bt.TRANG_THAI;

                    using (var trans = db.Database.BeginTransaction())
                    {
                        // 1. Cập nhật trạng thái bất thường
                        bt.TRANG_THAI = newStatus;

                        // 2. Ghi append-only vào TB_CHAMCONG_BT_LICHSU
                        int nextSeq = (db.TB_CHAMCONG_BT_LICHSU.Where(h => h.IDBATTHUONG == id).Select(h => (int?)h.THUTU).Max() ?? 0) + 1;
                        db.TB_CHAMCONG_BT_LICHSU.Add(new TB_CHAMCONG_BT_LICHSU
                        {
                            IDBATTHUONG = id,
                            THUTU = nextSeq,
                            TU_TRANGTHAI = oldStatus,
                            DEN_TRANGTHAI = newStatus,
                            LY_DO = req.Reason.Trim(),
                            NGUOI_XULY = userId,
                            XULY_LUC = DateTime.Now
                        });

                        // 3. Tăng revision kỳ công liên quan
                        int makycong = bt.NGAY.Year * 100 + bt.NGAY.Month;
                        var kyCong = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == makycong);
                        if (kyCong != null)
                        {
                            kyCong.CONG_INPUT_REV = kyCong.CONG_INPUT_REV + 1;
                        }

                        db.SaveChanges();
                        trans.Commit();

                        return Ok(new
                        {
                            success = true,
                            message = $"Đã cập nhật quyết định xử lý bất thường sang trạng thái '{newStatus}'."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi ResolveBatThuong: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi lưu quyết định xử lý bất thường." });
            }
        }

        #endregion

        #region Kiểm tra Sẵn sàng Chốt & Khóa Kỳ công (Period Readiness & Lock)

        /// <summary>
        /// GET: api/chamcong/kycong/{makycong}/readiness
        /// Đánh giá điều kiện sẵn sàng chốt kỳ công (Kiểm tra chặn chốt, bất thường chưa xử lý)
        /// </summary>
        [HttpGet]
        [Route("kycong/{makycong:int}/readiness")]
        public IHttpActionResult CheckPeriodReadiness(int makycong)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var kyCong = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == makycong);
                    if (kyCong == null) return NotFound();

                    bool isLocked = (kyCong.KHOA ?? 0) != 0;
                    int y = makycong / 100;
                    int m = makycong % 100;
                    var from = new DateTime(y, m, 1);
                    var to = from.AddMonths(1).AddDays(-1);

                    // Kiểm tra bất thường chặn chốt chưa xác minh
                    var unverifiedAnomalies = db.TB_CHAMCONG_BATTHUONG
                        .Where(b => b.NGAY >= from && b.NGAY <= to && b.CHAN_CHOT == true && b.TRANG_THAI == "CHO_XU_LY")
                        .Select(b => new
                        {
                            b.IDBATTHUONG,
                            b.MANV,
                            b.MA_LOI,
                            b.MUC_DO,
                            b.MO_TA
                        })
                        .ToList();

                    var blockingReasons = new List<string>();

                    if (isLocked)
                    {
                        blockingReasons.Add($"Kỳ công {makycong} hiện đã được khóa.");
                    }

                    if (unverifiedAnomalies.Count > 0)
                    {
                        var manvs = unverifiedAnomalies.Select(a => a.MANV).Distinct().ToList();
                        blockingReasons.Add($"Tồn tại {unverifiedAnomalies.Count} bất thường có mức độ chặn chốt chưa được giải trình/xác minh (Nhân viên: {string.Join(", ", manvs)}).");
                    }

                    // Kiểm tra trạng thái công bố và độ tươi của Revision so với CONG_INPUT_REV hiện hành
                    long inputRev = kyCong.CONG_INPUT_REV;
                    long publishRev = kyCong.CONG_PUBLISH_REV;

                    var unpublishedDays = db.Database.SqlQuery<decimal>(@"
                        SELECT COUNT(*) FROM TB_BANGCONG_CHITIET
                        WHERE MAKYCONG = :p0 AND (LANTINH_ID_HIENHANH IS NULL OR NVL(DU_DIEUKIEN_CHOT, 0) = 0)",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)).FirstOrDefault();
                    if (unpublishedDays > 0)
                    {
                        blockingReasons.Add($"Kỳ {makycong} có {unpublishedDays} ngày công chưa qua công bố TimeSegmentationEngine hoặc chưa đủ điều kiện chốt.");
                    }

                    var staleDays = db.Database.SqlQuery<decimal>(@"
                        SELECT COUNT(*) FROM TB_BANGCONG_CHITIET b
                        JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH = b.LANTINH_ID_HIENHANH
                        WHERE b.MAKYCONG = :p0 AND r.INPUT_REV <> :p1",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", inputRev)).FirstOrDefault();
                    if (staleDays > 0)
                    {
                        blockingReasons.Add($"Dữ liệu đầu vào đã có thay đổi mới ({staleDays} ngày công bị lệch so với lần công bố gần nhất). Cần tính toán công bố lại trước khi chốt.");
                    }

                    bool isReady = (blockingReasons.Count == 0 && !isLocked);

                    return Ok(new
                    {
                        success = true,
                        makycong = makycong,
                        isLocked = isLocked,
                        isReadyToLock = isReady,
                        inputRev = inputRev,
                        publishRev = publishRev,
                        unverifiedCount = unverifiedAnomalies.Count,
                        unverifiedAnomalies = unverifiedAnomalies,
                        blockingReasons = blockingReasons
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi CheckPeriodReadiness: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi kiểm tra tính sẵn sàng chốt kỳ." });
            }
        }

        /// <summary>
        /// POST: api/chamcong/kycong/{makycong}/lock
        /// Chốt và khóa kỳ công (Bảo vệ giao dịch: Khóa dòng kỳ công bằng SELECT FOR UPDATE, kiểm tra toàn vẹn công và bất thường)
        /// </summary>
        [HttpPost]
        [Route("kycong/{makycong:int}/lock")]
        public IHttpActionResult LockKyCong(int makycong, [FromBody] LockKyCongRequest req)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                var kyCongBus = new Bu.CLASS_CHAMCONG.KYCONG();
                kyCongBus.LockKyCong(makycong, currentUserId);

                return Ok(new
                {
                    success = true,
                    message = $"Kỳ công {makycong} đã được khóa thành công."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Content(HttpStatusCode.Conflict, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi LockKyCong: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi khóa kỳ công: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: api/chamcong/kycong/{makycong}/unlock
        /// Mở khóa kỳ công kèm lý do và ghi nhận audit
        /// </summary>
        [HttpPost]
        [Route("kycong/{makycong:int}/unlock")]
        public IHttpActionResult UnlockKyCong(int makycong, [FromBody] LockKyCongRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.LyDo))
                return BadRequest("Lý do mở khóa kỳ công bắt buộc phải nhập.");

            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                var kyCongBus = new Bu.CLASS_CHAMCONG.KYCONG();
                kyCongBus.UnlockKyCong(makycong, currentUserId, req.LyDo);

                return Ok(new
                {
                    success = true,
                    message = $"Đã mở khóa kỳ công {makycong} thành công."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi UnlockKyCong: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi mở khóa kỳ công: " + ex.Message });
            }
        }

        #endregion
    }
}
