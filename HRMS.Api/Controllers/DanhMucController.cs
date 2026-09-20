using Bu;
using Bu.DTO;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/danhmuc")]
    public class DanhMucController : ApiController
    {
        private readonly PHONGBAN _pbBus = new PHONGBAN();
        private readonly CHUCVU _cvBus = new CHUCVU();
        private readonly BOPHAN _bpBus = new BOPHAN();
        private readonly TRINHDO _tdBus = new TRINHDO();
        private readonly DANTOC _dtBus = new DANTOC();
        private readonly TONGIAO _tgBus = new TONGIAO();
        private readonly QUOCTICH _qtBus = new QUOCTICH();

        /// <summary>
        /// GET: api/danhmuc/phongban?lang=vi
        /// Danh mục phòng ban
        /// </summary>
        [HttpGet]
        [Route("phongban")]
        public IHttpActionResult GetPhongBan(string lang = "vi")
        {
            try
            {
                var list = _pbBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục phòng ban: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục phòng ban." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/chucvu?lang=vi
        /// Danh mục chức vụ
        /// </summary>
        [HttpGet]
        [Route("chucvu")]
        public IHttpActionResult GetChucVu(string lang = "vi")
        {
            try
            {
                var list = _cvBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục chức vụ: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục chức vụ." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/bophan?lang=vi
        /// Danh mục bộ phận
        /// </summary>
        [HttpGet]
        [Route("bophan")]
        public IHttpActionResult GetBoPhan(string lang = "vi")
        {
            try
            {
                var list = _bpBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục bộ phận: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục bộ phận." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/trinhdo?lang=vi
        /// Danh mục trình độ học vấn
        /// </summary>
        [HttpGet]
        [Route("trinhdo")]
        public IHttpActionResult GetTrinhDo(string lang = "vi")
        {
            try
            {
                var list = _tdBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục trình độ: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục trình độ." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/dantoc?lang=vi
        /// Danh mục dân tộc
        /// </summary>
        [HttpGet]
        [Route("dantoc")]
        public IHttpActionResult GetDanToc(string lang = "vi")
        {
            try
            {
                var list = _dtBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục dân tộc: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục dân tộc." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/tongiao?lang=vi
        /// Danh mục tôn giáo
        /// </summary>
        [HttpGet]
        [Route("tongiao")]
        public IHttpActionResult GetTonGiao(string lang = "vi")
        {
            try
            {
                var list = _tgBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục tôn giáo: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục tôn giáo." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/quoctich?lang=vi
        /// Danh mục quốc tịch
        /// </summary>
        [HttpGet]
        [Route("quoctich")]
        public IHttpActionResult GetQuocTich(string lang = "vi")
        {
            try
            {
                var list = _qtBus.getListDTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục quốc tịch: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục quốc tịch." });
            }
        }

        /// <summary>
        /// GET: api/danhmuc/all?lang=vi
        /// Tổng hợp toàn bộ danh mục phục vụ Combobox/Dropdowns nhanh chóng trong 1 request
        /// </summary>
        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAll(string lang = "vi")
        {
            try
            {
                var phongban = _pbBus.getListDTO(lang);
                var chucvu = _cvBus.getListDTO(lang);
                var bophan = _bpBus.getListDTO(lang);
                var trinhdo = _tdBus.getListDTO(lang);
                var dantoc = _dtBus.getListDTO(lang);
                var tongiao = _tgBus.getListDTO(lang);
                var quoctich = _qtBus.getListDTO(lang);

                return Ok(new
                {
                    phongBan = phongban,
                    chucVu = chucvu,
                    boPhan = bophan,
                    trinhDo = trinhdo,
                    danToc = dantoc,
                    tonGiao = tongiao,
                    quocTich = quoctich
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tổng hợp danh mục hệ thống: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tổng hợp danh mục hệ thống." });
            }
        }
    }
}
