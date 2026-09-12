using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api")]
    public class PhuCapUngLuongController : ApiController
    {
        /// <summary>
        /// GET: api/ungluong?thang={thang}&nam={nam}
        /// Danh sách tạm ứng lương
        /// </summary>
        [HttpGet]
        [Route("ungluong")]
        public IHttpActionResult GetUngLuong(int thang = 0, int nam = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var raw = (from ul in db.TB_UNGLUONG
                               where (thang <= 0 || ul.THANG == thang) && (nam <= 0 || ul.NAM == nam)
                               join nv in db.TB_NHANVIEN on ul.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               select new
                               {
                                   ul,
                                   nv.HOTEN
                               }).ToList();

                    var result = raw.Select(x => new
                    {
                        IDUL = (int)x.ul.IDUL,
                        MANV = x.ul.MANV,
                        HOTEN = x.HOTEN ?? "",
                        THANG = x.ul.THANG,
                        NAM = x.ul.NAM,
                        NGAY = x.ul.NGAY,
                        SOTIENUNG = x.ul.SOTIENUNG,
                        GHICHU = x.ul.GHICHU
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách tạm ứng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách tạm ứng lương." });
            }
        }

        /// <summary>
        /// POST: api/ungluong
        /// Thêm mới phiếu tạm ứng lương (Yêu cầu quyền F_UNGLUONG_ADD)
        /// Hỗ trợ cả Entity TB_UNGLUONG lẫn DTO linh hoạt từ Frontend
        /// </summary>
        [HttpPost]
        [Route("ungluong")]
        [JwtAuthorize(Right = "F_UNGLUONG_ADD")]
        public IHttpActionResult CreateUngLuong([FromBody] UngLuongInput input)
        {
            try
            {
                if (input == null)
                {
                    return BadRequest("Thông tin tạm ứng lương không hợp lệ.");
                }

                int? manv = input.MaNv ?? (input.MANV.HasValue ? (int?)input.MANV.Value : null);
                decimal? soTien = input.SoTien ?? input.SOTIENUNG;

                if (!manv.HasValue || !soTien.HasValue || soTien.Value <= 0)
                {
                    return BadRequest("Vui lòng chọn nhân viên và nhập số tiền tạm ứng hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                using (var db = new MyEntities())
                {
                    var ul = new TB_UNGLUONG
                    {
                        MANV = manv.Value,
                        SOTIENUNG = soTien.Value,
                        THANG = input.Thang ?? input.THANG ?? DateTime.Now.Month,
                        NAM = input.Nam ?? input.NAM ?? DateTime.Now.Year,
                        NGAY = input.Ngay ?? input.NGAY ?? DateTime.Now.Day,
                        GHICHU = input.GhiChu ?? input.GHICHU ?? "",
                        CREATED_DATE = DateTime.Now,
                        CREATED_BY = currentUserId
                    };

                    db.TB_UNGLUONG.Add(ul);
                    db.SaveChanges();
                    return Ok(ul);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tạo phiếu tạm ứng lương: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tạo phiếu tạm ứng lương." });
            }
        }

        /// <summary>
        /// DELETE: api/ungluong/{id}
        /// Xóa phiếu tạm ứng lương (Yêu cầu quyền F_UNGLUONG_DELETE)
        /// </summary>
        [HttpDelete]
        [Route("ungluong/{id:int}")]
        [Route("ungluong")]
        [JwtAuthorize(Right = "F_UNGLUONG_DELETE")]
        public IHttpActionResult DeleteUngLuong(int id = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var item = db.TB_UNGLUONG.FirstOrDefault(u => u.IDUL == id);
                    if (item != null)
                    {
                        db.TB_UNGLUONG.Remove(item);
                        db.SaveChanges();
                    }
                    return Ok(new { success = true, message = $"Đã hủy phiếu tạm ứng #{id}." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa tạm ứng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa tạm ứng." });
            }
        }

        /// <summary>
        /// GET: api/tangca?thang={thang}&nam={nam}
        /// Danh sách làm thêm giờ / tăng ca
        /// </summary>
        [HttpGet]
        [Route("tangca")]
        public IHttpActionResult GetTangCa(int thang = 0, int nam = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var raw = (from tc in db.TB_TANGCA
                               where (thang <= 0 || tc.THANG == thang) && (nam <= 0 || tc.NAM == nam)
                               join nv in db.TB_NHANVIEN on tc.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               join lc in db.TB_LOAICA on tc.IDLOAICA equals lc.IDLOAICA into lcGroup
                               from lc in lcGroup.DefaultIfEmpty()
                               select new
                               {
                                   tc,
                                   HOTEN = nv.HOTEN,
                                   TENLOAICA = lc.TENLOAICA,
                                   HESOLOAICA = lc.HESOLOAICA
                               }).ToList();

                    var result = raw.Select(x => new
                    {
                        IDTCA = (int)x.tc.IDTCA,
                        MANV = x.tc.MANV,
                        HOTEN = x.HOTEN ?? "",
                        THANG = x.tc.THANG,
                        NAM = x.tc.NAM,
                        NGAY = x.tc.NGAY,
                        SOGIO = x.tc.SOGIO,
                        IDLOAICA = x.tc.IDLOAICA,
                        TENLOAICA = x.TENLOAICA ?? "Ca tiêu chuẩn",
                        HESOLOAICA = x.HESOLOAICA ?? 1.5m,
                        SOTIENTC = x.tc.SOTIENTC,
                        GHICHU = x.tc.GHICHU
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách tăng ca: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách tăng ca." });
            }
        }

        /// <summary>
        /// POST: api/tangca
        /// Đăng ký làm thêm giờ / tăng ca (Yêu cầu quyền F_TANGCA_ADD)
        /// Hỗ trợ cả Entity TB_TANGCA lẫn DTO linh hoạt từ Frontend
        /// </summary>
        [HttpPost]
        [Route("tangca")]
        [JwtAuthorize(Right = "F_TANGCA_ADD")]
        public IHttpActionResult CreateTangCa([FromBody] TangCaInput input)
        {
            try
            {
                if (input == null)
                {
                    return BadRequest("Thông tin đăng ký tăng ca không hợp lệ.");
                }

                int? manv = input.MaNv ?? (input.MANV.HasValue ? (int?)input.MANV.Value : null);
                decimal? soGio = input.SoGio ?? input.SOGIO;

                if (!manv.HasValue || !soGio.HasValue || soGio.Value <= 0)
                {
                    return BadRequest("Vui lòng chọn nhân viên và nhập số giờ tăng ca hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                using (var db = new MyEntities())
                {
                    int idLoaiCa = input.IdLoaiCa ?? (input.IDLOAICA.HasValue ? (int)input.IDLOAICA.Value : 1);
                    decimal? soTien = input.SoTien ?? input.SOTIENTC;
                    if (!soTien.HasValue || soTien <= 0)
                    {
                        soTien = soGio.Value * 50000 * 1.5m; // Mức mẫu 75,000đ/giờ
                    }

                    var tc = new TB_TANGCA
                    {
                        MANV = manv.Value,
                        SOGIO = soGio.Value,
                        IDLOAICA = idLoaiCa,
                        SOTIENTC = soTien,
                        THANG = input.Thang ?? input.THANG ?? DateTime.Now.Month,
                        NAM = input.Nam ?? input.NAM ?? DateTime.Now.Year,
                        NGAY = input.Ngay ?? input.NGAY ?? DateTime.Now.Day,
                        GHICHU = input.GhiChu ?? input.GHICHU ?? "",
                        CREATED_DATE = DateTime.Now,
                        CREATED_BY = currentUserId
                    };

                    db.TB_TANGCA.Add(tc);
                    db.SaveChanges();
                    return Ok(tc);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tạo đăng ký tăng ca: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tạo đăng ký tăng ca." });
            }
        }

        /// <summary>
        /// DELETE: api/tangca/{id}
        /// Hủy bản ghi tăng ca (Yêu cầu quyền F_TANGCA_DELETE)
        /// </summary>
        [HttpDelete]
        [Route("tangca/{id:int}")]
        [Route("tangca")]
        [JwtAuthorize(Right = "F_TANGCA_DELETE")]
        public IHttpActionResult DeleteTangCa(int id = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var item = db.TB_TANGCA.FirstOrDefault(t => t.IDTCA == id);
                    if (item != null)
                    {
                        db.TB_TANGCA.Remove(item);
                        db.SaveChanges();
                    }
                    return Ok(new { success = true, message = $"Đã hủy bản ghi tăng ca #{id}." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa tăng ca: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa tăng ca." });
            }
        }
    }

    public class UngLuongInput
    {
        public int? IdUl { get; set; }
        public int? MaNv { get; set; }
        public decimal? MANV { get; set; }
        public decimal? SoTien { get; set; }
        public decimal? SOTIENUNG { get; set; }
        public int? Thang { get; set; }
        public int? THANG { get; set; }
        public int? Nam { get; set; }
        public int? NAM { get; set; }
        public int? Ngay { get; set; }
        public int? NGAY { get; set; }
        public string GhiChu { get; set; }
        public string GHICHU { get; set; }
    }

    public class TangCaInput
    {
        public int? IdTca { get; set; }
        public int? MaNv { get; set; }
        public decimal? MANV { get; set; }
        public decimal? SoGio { get; set; }
        public decimal? SOGIO { get; set; }
        public int? IdLoaiCa { get; set; }
        public decimal? IDLOAICA { get; set; }
        public decimal? SoTien { get; set; }
        public decimal? SOTIENTC { get; set; }
        public int? Thang { get; set; }
        public int? THANG { get; set; }
        public int? Nam { get; set; }
        public int? NAM { get; set; }
        public int? Ngay { get; set; }
        public int? NGAY { get; set; }
        public string GhiChu { get; set; }
        public string GHICHU { get; set; }
    }
}
