using Bu;
using Bu.DTO;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/hopdong")]
    public class HopDongController : ApiController
    {
        private readonly HOPDONGLAODONG _hopDongBus = new HOPDONGLAODONG();

        /// <summary>
        /// GET: api/hopdong
        /// Lấy toàn bộ danh sách hợp đồng lao động đầy đủ thông tin nhân viên
        /// Hoặc lấy chi tiết nếu cung cấp query string ?sohd=...
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll([FromUri] string sohd = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(sohd))
                {
                    return GetDetailInternal(sohd);
                }
                var list = _hopDongBus.getlistFull_DTO();
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách hợp đồng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách hợp đồng." });
            }
        }

        /// <summary>
        /// GET: api/hopdong/detail?sohd=... hoặc api/hopdong/detail/{*sohd}
        /// Lấy chi tiết hợp đồng lao động theo số hợp đồng
        /// </summary>
        [HttpGet]
        [Route("detail")]
        [Route("detail/{*sohd}")]
        public IHttpActionResult GetBySoHd([FromUri] string sohd = null)
        {
            return GetDetailInternal(sohd);
        }

        private IHttpActionResult GetDetailInternal(string sohd)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sohd)) return BadRequest("Vui lòng cung cấp số hợp đồng.");
                sohd = Uri.UnescapeDataString(sohd).Trim();
                var list = _hopDongBus.getItem_FULL(sohd);
                if (list == null || list.Count == 0)
                {
                    var fallback = _hopDongBus.getItem(sohd);
                    if (fallback == null) return NotFound();
                    return Ok(fallback);
                }
                return Ok(list.FirstOrDefault());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi lấy chi tiết hợp đồng " + sohd + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin hợp đồng." });
            }
        }

        /// <summary>
        /// POST: api/hopdong
        /// Thêm mới hợp đồng lao động (Yêu cầu quyền F_HOPDONG_ADD)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Right = "F_HOPDONG_ADD")]
        public IHttpActionResult Create([FromBody] TB_HOPDONG hd)
        {
            try
            {
                if (hd == null) return BadRequest("Dữ liệu hợp đồng không hợp lệ.");
                if (string.IsNullOrWhiteSpace(hd.SOHD))
                {
                    hd.SOHD = $"{DateTime.Now:yyyyMMdd}/{hd.MANV}/HĐLĐ";
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                hd.CREATED_BY = currentUserId;
                hd.CREATED_DATE = DateTime.Now;
                var result = _hopDongBus.Add(hd);
                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thêm mới hợp đồng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tạo mới hợp đồng." });
            }
        }

        /// <summary>
        /// PUT: api/hopdong/{sohd}
        /// Cập nhật hợp đồng lao động (Yêu cầu quyền F_HOPDONG_EDIT)
        /// </summary>
        [HttpPut]
        [Route("{*sohd}")]
        [JwtAuthorize(Right = "F_HOPDONG_EDIT")]
        public IHttpActionResult Update(string sohd, [FromBody] TB_HOPDONG hd)
        {
            try
            {
                sohd = Uri.UnescapeDataString(sohd).Trim();
                if (hd == null) return BadRequest("Dữ liệu hợp đồng không hợp lệ.");

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                hd.SOHD = sohd;
                hd.UPDATE_BY = currentUserId;
                hd.UPDATE_DATE = DateTime.Now;
                var result = _hopDongBus.Update(hd);
                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi cập nhật hợp đồng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi cập nhật hợp đồng." });
            }
        }

        /// <summary>
        /// DELETE: api/hopdong/{sohd}
        /// Xóa hợp đồng lao động (Yêu cầu quyền F_HOPDONG_DELETE)
        /// </summary>
        [HttpDelete]
        [Route("{*sohd}")]
        [Route("")]
        [JwtAuthorize(Right = "F_HOPDONG_DELETE")]
        public IHttpActionResult Delete(string sohd = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sohd)) return BadRequest("Vui lòng cung cấp số hợp đồng.");
                sohd = Uri.UnescapeDataString(sohd).Trim();
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                _hopDongBus.Delete(sohd, currentUserId);
                return Ok(new { success = true, message = $"Đã xóa hợp đồng {sohd} thành công." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa hợp đồng " + sohd + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa hợp đồng." });
            }
        }
    }
}
