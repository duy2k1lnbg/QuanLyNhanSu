using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/khenthuong")]
    public class KhenThuongKyLuatController : ApiController
    {
        /// <summary>
        /// GET: api/khenthuong?loai={1|2}
        /// Lấy danh sách khen thưởng (loại = 1) hoặc kỷ luật (loại = 2)
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetList(int loai = 1)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var raw = (from kt in db.TB_KHENTHUONG_KYLUAT
                               where kt.LOAI == loai
                               join nv in db.TB_NHANVIEN on kt.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               select new
                               {
                                   kt,
                                   nv.HOTEN
                               }).ToList();

                    var result = raw.Select(x => new
                    {
                        SOQUYETDINH = x.kt.SOQUYETDINH,
                        MANV = x.kt.MANV,
                        HOTEN = x.HOTEN ?? "",
                        NOIDUNG = x.kt.NOIDUNG,
                        LYDO = x.kt.LYDO,
                        TUNGAY = x.kt.TUNGAY.HasValue ? x.kt.TUNGAY.Value.ToString("dd/MM/yyyy") : "",
                        DENNGAY = x.kt.DENNGAY.HasValue ? x.kt.DENNGAY.Value.ToString("dd/MM/yyyy") : "",
                        LOAI = x.kt.LOAI
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách khen thưởng/kỷ luật: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách khen thưởng/kỷ luật." });
            }
        }

        /// <summary>
        /// POST: api/khenthuong
        /// Thêm mới quyết định khen thưởng hoặc kỷ luật (Yêu cầu quyền F_KHENTHUONG_ADD)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Right = "F_KHENTHUONG_ADD")]
        public IHttpActionResult Create([FromBody] TB_KHENTHUONG_KYLUAT kt)
        {
            try
            {
                if (kt == null || !kt.MANV.HasValue)
                {
                    return BadRequest("Thông tin quyết định không hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                using (var db = new MyEntities())
                {
                    if (string.IsNullOrWhiteSpace(kt.SOQUYETDINH))
                    {
                        string prefix = kt.LOAI == 2 ? "QĐ-KL" : "QĐ-KT";
                        kt.SOQUYETDINH = $"{DateTime.Now:yyyyMMdd}/{kt.MANV}/{prefix}";
                    }

                    kt.CREATED_DATE = DateTime.Now;
                    kt.CREATED_BY = currentUserId;
                    if (!kt.TUNGAY.HasValue) kt.TUNGAY = DateTime.Now;
                    if (!kt.LOAI.HasValue) kt.LOAI = 1;

                    db.TB_KHENTHUONG_KYLUAT.Add(kt);
                    db.SaveChanges();
                    return Ok(kt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thêm quyết định khen thưởng/kỷ luật: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thêm quyết định khen thưởng/kỷ luật." });
            }
        }

        /// <summary>
        /// DELETE: api/khenthuong/{*soqd}
        /// Xóa quyết định khen thưởng hoặc kỷ luật (Yêu cầu quyền F_KHENTHUONG_DELETE)
        /// </summary>
        [HttpDelete]
        [Route("{*soqd}")]
        [Route("")]
        [JwtAuthorize(Right = "F_KHENTHUONG_DELETE")]
        public IHttpActionResult Delete(string soqd = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(soqd)) return BadRequest("Vui lòng cung cấp số quyết định cần xóa.");
                soqd = Uri.UnescapeDataString(soqd).Trim();

                using (var db = new MyEntities())
                {
                    var item = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(k => k.SOQUYETDINH == soqd);
                    if (item != null)
                    {
                        db.TB_KHENTHUONG_KYLUAT.Remove(item);
                        db.SaveChanges();
                    }
                    return Ok(new { success = true, message = $"Đã xóa quyết định {soqd}." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa quyết định: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa quyết định." });
            }
        }
    }
}
