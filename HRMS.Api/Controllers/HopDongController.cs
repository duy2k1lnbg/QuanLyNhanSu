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
    [JwtAuthorize(Right = "F_NV_HOPDONG")]
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
        [RateLimit(Policy = RateLimitPolicy.BusinessRead)]
        public IHttpActionResult GetAll([FromUri] string sohd = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(sohd))
                {
                    return GetDetailInternal(sohd);
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                var list = _hopDongBus.getlistFull_DTO();

                // Object-level authorization for contracts list
                if (jwtUser != null && !jwtUser.IsAdmin)
                {
                    if (!string.IsNullOrWhiteSpace(jwtUser.MaCty) && decimal.TryParse(jwtUser.MaCty, out decimal userCtyId) && userCtyId > 0)
                    {
                        using (var db = new MyEntities())
                        {
                            var ctyManvs = new HashSet<decimal>(db.TB_NHANVIEN.Where(e => e.IDCTY == userCtyId).Select(e => e.MANV).ToList());
                            list = list.Where(h => h.MANV.HasValue && ctyManvs.Contains(h.MANV.Value)).ToList();
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(jwtUser.Manv) && decimal.TryParse(jwtUser.Manv, out decimal userManv) && userManv > 0)
                    {
                        list = list.Where(h => h.MANV.HasValue && h.MANV.Value == userManv).ToList();
                    }
                    else
                    {
                        return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Không thể xác định phạm vi dữ liệu hợp lệ cho tài khoản." });
                    }
                }

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
        [RateLimit(Policy = RateLimitPolicy.BusinessRead)]
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

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                var list = _hopDongBus.getItem_FULL(sohd);
                HOPDONG_DTO item = (list != null && list.Count > 0) ? list.FirstOrDefault() : null;

                if (item == null)
                {
                    var fallback = _hopDongBus.getItem(sohd);
                    if (fallback == null) return NotFound();

                    // Object-level authorization for fallback contract
                    if (jwtUser != null && !jwtUser.IsAdmin && fallback.MANV.HasValue)
                    {
                        using (var db = new MyEntities())
                        {
                            var emp = db.TB_NHANVIEN.FirstOrDefault(e => e.MANV == fallback.MANV.Value);
                            if (emp == null) return NotFound();

                            if (!string.IsNullOrWhiteSpace(jwtUser.MaCty) && decimal.TryParse(jwtUser.MaCty, out decimal userCtyId) && userCtyId > 0)
                            {
                                if (emp.IDCTY != userCtyId)
                                {
                                    return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Hợp đồng nằm ngoài phạm vi công ty của bạn." });
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(jwtUser.Manv) && decimal.TryParse(jwtUser.Manv, out decimal userManv) && userManv > 0)
                            {
                                if (fallback.MANV.Value != userManv)
                                {
                                    return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bạn chỉ được xem hợp đồng của chính mình." });
                                }
                            }
                            else
                            {
                                return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Không thể xác định phạm vi dữ liệu hợp lệ." });
                            }
                        }
                    }

                    return Ok(fallback);
                }

                // Object-level authorization for full contract
                if (jwtUser != null && !jwtUser.IsAdmin && item.MANV.HasValue)
                {
                    using (var db = new MyEntities())
                    {
                        var emp = db.TB_NHANVIEN.FirstOrDefault(e => e.MANV == item.MANV.Value);
                        if (emp == null) return NotFound();

                        if (!string.IsNullOrWhiteSpace(jwtUser.MaCty) && decimal.TryParse(jwtUser.MaCty, out decimal userCtyId) && userCtyId > 0)
                        {
                            if (emp.IDCTY != userCtyId)
                            {
                                return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Hợp đồng nằm ngoài phạm vi công ty của bạn." });
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(jwtUser.Manv) && decimal.TryParse(jwtUser.Manv, out decimal userManv) && userManv > 0)
                        {
                            if (item.MANV.Value != userManv)
                            {
                                return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bạn chỉ được xem hợp đồng của chính mình." });
                            }
                        }
                        else
                        {
                            return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Không thể xác định phạm vi dữ liệu hợp lệ." });
                        }
                    }
                }

                return Ok(item);
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
        [RateLimit(Policy = RateLimitPolicy.BusinessWrite)]
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
                if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int currentUserId) || currentUserId <= 0)
                {
                    return Unauthorized();
                }

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
        [RateLimit(Policy = RateLimitPolicy.BusinessWrite)]
        public IHttpActionResult Update(string sohd, [FromBody] TB_HOPDONG hd)
        {
            try
            {
                sohd = Uri.UnescapeDataString(sohd).Trim();
                if (hd == null) return BadRequest("Dữ liệu hợp đồng không hợp lệ.");

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int currentUserId) || currentUserId <= 0)
                {
                    return Unauthorized();
                }

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
        [RateLimit(Policy = RateLimitPolicy.BusinessWrite)]
        public IHttpActionResult Delete(string sohd = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sohd)) return BadRequest("Vui lòng cung cấp số hợp đồng.");
                sohd = Uri.UnescapeDataString(sohd).Trim();
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int currentUserId) || currentUserId <= 0)
                {
                    return Unauthorized();
                }

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
