using Bu.CLASS_CHAMCONG;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/chamcong")]
    public class ChamCongController : ApiController
    {
        private readonly KYCONGCHITIET _kyCongCTBus = new KYCONGCHITIET();
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

                    // Danh sách 975 nhân sự chuẩn từ TB_NHANVIEN (loại bỏ bản ghi test 3207)
                    var nvList = db.TB_NHANVIEN
                        .Where(nv => nv.MANV != 3207)
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

                    // Lọc chuẩn xác chỉ giữ các bản ghi thuộc 975 nhân viên chính thức của công ty
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

        /// <summary>
        /// POST: api/chamcong/phatsinh
        /// Phát sinh tự động bảng công chi tiết cho tháng/năm (Yêu cầu quyền F_CHAMCONG_ADD)
        /// </summary>
        [HttpPost]
        [Route("phatsinh")]
        [JwtAuthorize(Right = "F_CHAMCONG_ADD")]
        public IHttpActionResult PhatSinhKyCong([FromBody] PhatSinhKyCongParam param)
        {
            try
            {
                if (param == null || param.Thang < 1 || param.Thang > 12 || param.Nam < 2000)
                {
                    return BadRequest("Thông tin tháng hoặc năm không hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;
                int ctyId = param.MaCty > 0 ? param.MaCty : ((jwtUser != null && int.TryParse(jwtUser.MaCty, out int uc) && uc > 0) ? uc : 1);

                _kyCongCTBus.phatSinhKyCongChiTiet(ctyId, param.Thang, param.Nam, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = $"Đã phát sinh kỳ công chi tiết tháng {param.Thang}/{param.Nam} thành công."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi phát sinh kỳ công chi tiết: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi phát sinh kỳ công chi tiết." });
            }
        }
    }

    public class PhatSinhKyCongParam
    {
        public int MaCty { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int? IdUser { get; set; }
    }
}
