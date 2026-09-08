using Bu;
using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [RoutePrefix("api/hopdong")]
    public class HopDongController : ApiController
    {
        private readonly HOPDONGLAODONG _hopDongBus = new HOPDONGLAODONG();

        /// <summary>
        /// GET: api/hopdong
        /// Lấy toàn bộ danh sách hợp đồng lao động đầy đủ thông tin nhân viên
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            try
            {
                var list = _hopDongBus.getlistFull_DTO();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh sách hợp đồng: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/hopdong/{sohd}
        /// Lấy chi tiết hợp đồng lao động theo số hợp đồng
        /// </summary>
        [HttpGet]
        [Route("{sohd}")]
        public IHttpActionResult GetBySoHd(string sohd)
        {
            try
            {
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
                return InternalServerError(new Exception("Lỗi khi lấy chi tiết hợp đồng " + sohd + ": " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/hopdong
        /// Thêm mới hợp đồng lao động
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] TB_HOPDONG hd)
        {
            try
            {
                if (hd == null) return BadRequest("Dữ liệu hợp đồng không hợp lệ.");
                if (string.IsNullOrWhiteSpace(hd.SOHD))
                {
                    hd.SOHD = $"{DateTime.Now:yyyyMMdd}/{hd.MANV}/HĐLĐ";
                }
                hd.CREATED_DATE = DateTime.Now;
                var result = _hopDongBus.Add(hd);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi thêm mới hợp đồng: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// PUT: api/hopdong/{sohd}
        /// Cập nhật hợp đồng lao động
        /// </summary>
        [HttpPut]
        [Route("{sohd}")]
        public IHttpActionResult Update(string sohd, [FromBody] TB_HOPDONG hd)
        {
            try
            {
                if (hd == null) return BadRequest("Dữ liệu hợp đồng không hợp lệ.");
                hd.SOHD = sohd;
                hd.UPDATE_DATE = DateTime.Now;
                var result = _hopDongBus.Update(hd);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi cập nhật hợp đồng: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// DELETE: api/hopdong/{sohd}
        /// Xóa hợp đồng lao động
        /// </summary>
        [HttpDelete]
        [Route("{sohd}")]
        public IHttpActionResult Delete(string sohd, int iduser = 1)
        {
            try
            {
                _hopDongBus.Delete(sohd, iduser);
                return Ok(new { success = true, message = $"Đã xóa hợp đồng {sohd} thành công." });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi xóa hợp đồng: " + ex.Message, ex));
            }
        }
    }
}
