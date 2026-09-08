using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
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
                return InternalServerError(new Exception("Lỗi khi tải danh sách tạm ứng: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/ungluong
        /// Thêm mới phiếu tạm ứng lương
        /// </summary>
        [HttpPost]
        [Route("ungluong")]
        public IHttpActionResult CreateUngLuong([FromBody] TB_UNGLUONG ul)
        {
            try
            {
                if (ul == null || !ul.MANV.HasValue || !ul.SOTIENUNG.HasValue)
                {
                    return BadRequest("Thông tin tạm ứng lương không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    ul.CREATED_DATE = DateTime.Now;
                    ul.CREATED_BY = 1;
                    if (!ul.THANG.HasValue) ul.THANG = DateTime.Now.Month;
                    if (!ul.NAM.HasValue) ul.NAM = DateTime.Now.Year;
                    if (!ul.NGAY.HasValue) ul.NGAY = DateTime.Now.Day;

                    db.TB_UNGLUONG.Add(ul);
                    db.SaveChanges();
                    return Ok(ul);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tạo phiếu tạm ứng lương: " + ex.Message, ex));
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
                return InternalServerError(new Exception("Lỗi khi tải danh sách tăng ca: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/tangca
        /// Đăng ký làm thêm giờ / tăng ca
        /// </summary>
        [HttpPost]
        [Route("tangca")]
        public IHttpActionResult CreateTangCa([FromBody] TB_TANGCA tc)
        {
            try
            {
                if (tc == null || !tc.MANV.HasValue || !tc.SOGIO.HasValue)
                {
                    return BadRequest("Thông tin đăng ký tăng ca không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    tc.CREATED_DATE = DateTime.Now;
                    tc.CREATED_BY = 1;
                    if (!tc.THANG.HasValue) tc.THANG = DateTime.Now.Month;
                    if (!tc.NAM.HasValue) tc.NAM = DateTime.Now.Year;
                    if (!tc.NGAY.HasValue) tc.NGAY = DateTime.Now.Day;
                    if (!tc.IDLOAICA.HasValue) tc.IDLOAICA = 1;

                    // Tính ước lượng số tiền tăng ca nếu chưa có
                    if (!tc.SOTIENTC.HasValue || tc.SOTIENTC <= 0)
                    {
                        tc.SOTIENTC = tc.SOGIO * 50000 * 1.5m; // Mức mẫu 75,000đ/giờ
                    }

                    db.TB_TANGCA.Add(tc);
                    db.SaveChanges();
                    return Ok(tc);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tạo đăng ký tăng ca: " + ex.Message, ex));
            }
        }
    }
}
