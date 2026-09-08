using Bu;
using Bu.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        private readonly NHANVIEN _nhanVienBus = new NHANVIEN();

        /// <summary>
        /// GET: api/dashboard/stats
        /// Trả về toàn bộ số liệu thống kê Dashboard từ Oracle Database thông qua tầng nghiệp vụ Bu.NHANVIEN
        /// </summary>
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetDashboardStats()
        {
            try
            {
                int tongNhanVien = _nhanVienBus.GetTongNhanVien();
                var phongBanStats = _nhanVienBus.GetPhongBanStats();
                var luongStats = _nhanVienBus.GetLuongStats();
                decimal tongQuyLuongHienTai = luongStats.LastOrDefault()?.TongLuong ?? 0;

                return Ok(new
                {
                    tongNhanVien = tongNhanVien,
                    tongQuyLuong = tongQuyLuongHienTai,
                    phongBanStats = phongBanStats,
                    luongStats = luongStats
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải thống kê Dashboard: " + ex.Message, ex));
            }
        }
    }
}
