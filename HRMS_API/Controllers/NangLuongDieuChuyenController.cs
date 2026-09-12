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
    public class NangLuongDieuChuyenController : ApiController
    {
        /// <summary>
        /// GET: api/nangluong
        /// Danh sách lịch sử và quyết định nâng lương
        /// </summary>
        [HttpGet]
        [Route("nangluong")]
        public IHttpActionResult GetNangLuong()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var raw = (from nl in db.TB_NANGLUONG_NHANVIEN
                               join nv in db.TB_NHANVIEN on nl.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               select new
                               {
                                   nl,
                                   nv.HOTEN
                               }).ToList();

                    var result = raw.Select(x => new
                    {
                        SOQDNL = x.nl.SOQDNL,
                        SOHD = x.nl.SOHD,
                        MANV = x.nl.MANV,
                        HOTEN = x.HOTEN ?? "",
                        HESOLUONG_NOW = x.nl.HESOLUONG_NOW,
                        HESOLUONG_NEW = x.nl.HESOLUONG_NEW,
                        NGAYLENLUONG = x.nl.NGAYLENLUONG.HasValue ? x.nl.NGAYLENLUONG.Value.ToString("dd/MM/yyyy") : "",
                        NGAYKYNL = x.nl.NGAYKYNL.HasValue ? x.nl.NGAYKYNL.Value.ToString("dd/MM/yyyy") : "",
                        GHICHUNL = x.nl.GHICHUNL
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách nâng lương: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách nâng lương." });
            }
        }

        /// <summary>
        /// POST: api/nangluong
        /// Thêm quyết định nâng lương mới (Yêu cầu quyền F_NANGLUONG_ADD)
        /// Hỗ trợ cả Entity TB_NANGLUONG_NHANVIEN lẫn DTO linh hoạt từ Frontend
        /// </summary>
        [HttpPost]
        [Route("nangluong")]
        [JwtAuthorize(Right = "F_NANGLUONG_ADD")]
        public IHttpActionResult CreateNangLuong([FromBody] NangLuongInput input)
        {
            try
            {
                if (input == null) return BadRequest("Thông tin quyết định nâng lương không hợp lệ.");

                int? manv = input.MaNv ?? (input.MANV.HasValue ? (int?)input.MANV.Value : null);
                if (!manv.HasValue) return BadRequest("Vui lòng chọn nhân viên được nâng lương.");

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                using (var db = new MyEntities())
                {
                    string soqd = !string.IsNullOrWhiteSpace(input.SoQd) ? input.SoQd : (!string.IsNullOrWhiteSpace(input.SOQDNL) ? input.SOQDNL : $"{DateTime.Now:yyyyMMdd}/{manv.Value}/QĐ-NL");
                    
                    // Tìm hợp đồng của nhân viên để lấy số hợp đồng và hệ số lương hiện tại nếu chưa có
                    var latestHd = db.TB_HOPDONG.Where(h => h.MANV == manv.Value).OrderByDescending(h => h.NGAYBATDAU).FirstOrDefault();
                    string sohd = !string.IsNullOrWhiteSpace(input.SoHd) ? input.SoHd : (!string.IsNullOrWhiteSpace(input.SOHD) ? input.SOHD : (latestHd != null ? latestHd.SOHD : ""));
                    decimal? hsNow = input.HeSoLuongHienTai ?? input.HESOLUONG_NOW ?? (latestHd != null ? latestHd.HESOLUONG : 1.0m);
                    decimal? hsNew = input.HeSoLuongMoi ?? input.HESOLUONG_NEW ?? ((hsNow ?? 1.0m) + 0.3m);

                    DateTime? ngayKy = null;
                    if (input.NgayKy.HasValue) ngayKy = input.NgayKy;
                    else if (!string.IsNullOrWhiteSpace(input.NgayKyStr) && DateTime.TryParse(input.NgayKyStr, out var nk)) ngayKy = nk;
                    else if (input.NGAYKYNL.HasValue) ngayKy = input.NGAYKYNL;
                    else ngayKy = DateTime.Now;

                    DateTime? ngayLenLuong = null;
                    if (input.NgayLenLuong.HasValue) ngayLenLuong = input.NgayLenLuong;
                    else if (!string.IsNullOrWhiteSpace(input.NgayLenLuongStr) && DateTime.TryParse(input.NgayLenLuongStr, out var nl)) ngayLenLuong = nl;
                    else if (input.NGAYLENLUONG.HasValue) ngayLenLuong = input.NGAYLENLUONG;
                    else ngayLenLuong = DateTime.Now;

                    var record = new TB_NANGLUONG_NHANVIEN
                    {
                        SOQDNL = soqd,
                        SOHD = sohd,
                        MANV = manv.Value,
                        HESOLUONG_NOW = hsNow,
                        HESOLUONG_NEW = hsNew,
                        NGAYKYNL = ngayKy,
                        NGAYLENLUONG = ngayLenLuong,
                        GHICHUNL = input.GhiChu ?? input.GHICHUNL ?? "",
                        CREATED_DATE = DateTime.Now,
                        CREATED_BY = currentUserId
                    };

                    db.TB_NANGLUONG_NHANVIEN.Add(record);

                    // Đồng bộ hệ số lương mới vào hợp đồng hiện tại
                    if (latestHd != null && hsNew.HasValue)
                    {
                        latestHd.HESOLUONG = hsNew;
                    }

                    db.SaveChanges();
                    return Ok(record);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thêm quyết định nâng lương: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thêm quyết định nâng lương." });
            }
        }

        /// <summary>
        /// DELETE: api/nangluong/{soqd}
        /// Xóa quyết định nâng lương (Yêu cầu quyền F_NANGLUONG_DELETE)
        /// </summary>
        [HttpDelete]
        [Route("nangluong/{*soqd}")]
        [Route("nangluong")]
        [JwtAuthorize(Right = "F_NANGLUONG_DELETE")]
        public IHttpActionResult DeleteNangLuong(string soqd = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(soqd)) return BadRequest("Vui lòng cung cấp số quyết định cần xóa.");

                soqd = Uri.UnescapeDataString(soqd).Trim();
                using (var db = new MyEntities())
                {
                    var item = db.TB_NANGLUONG_NHANVIEN.FirstOrDefault(n => n.SOQDNL == soqd);
                    if (item != null)
                    {
                        db.TB_NANGLUONG_NHANVIEN.Remove(item);
                        db.SaveChanges();
                    }
                    return Ok(new { success = true, message = $"Đã xóa quyết định nâng lương {soqd}." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa quyết định nâng lương: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa quyết định nâng lương." });
            }
        }

        /// <summary>
        /// GET: api/dieuchuyen
        /// Danh sách quyết định điều chuyển phòng ban / chức vụ
        /// </summary>
        [HttpGet]
        [Route("dieuchuyen")]
        public IHttpActionResult GetDieuChuyen()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var raw = (from dc in db.TB_DIEUCHUYEN_NHANVIEN
                               join nv in db.TB_NHANVIEN on dc.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               join pb1 in db.TB_PHONGBAN on dc.MAPB equals pb1.IDPB into pb1Group
                               from pb1 in pb1Group.DefaultIfEmpty()
                               join pb2 in db.TB_PHONGBAN on dc.MAPB2 equals pb2.IDPB into pb2Group
                               from pb2 in pb2Group.DefaultIfEmpty()
                               select new
                               {
                                   dc,
                                   HOTEN = nv.HOTEN,
                                   TENPB_CU = pb1.TENPB,
                                   TENPB_MOI = pb2.TENPB
                               }).ToList();

                    var result = raw.Select(x => new
                    {
                        SOQDDIEUCHUYEN = x.dc.SOQDDIEUCHUYEN,
                        MANV = x.dc.MANV,
                        HOTEN = x.HOTEN ?? "",
                        MAPB = x.dc.MAPB,
                        TENPB_CU = x.TENPB_CU ?? "Chưa phân bổ",
                        MAPB2 = x.dc.MAPB2,
                        TENPB_MOI = x.TENPB_MOI ?? "Chưa phân bổ",
                        NGAYDC = x.dc.NGAYDC.HasValue ? x.dc.NGAYDC.Value.ToString("dd/MM/yyyy") : "",
                        LYDODC = x.dc.LYDODC,
                        GHICHU = x.dc.GHICHU
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách điều chuyển: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách điều chuyển." });
            }
        }

        /// <summary>
        /// POST: api/dieuchuyen
        /// Thêm quyết định điều chuyển nhân sự (Yêu cầu quyền F_DIEUCHUYEN_ADD)
        /// Hỗ trợ cả Entity TB_DIEUCHUYEN_NHANVIEN lẫn DTO linh hoạt từ Frontend
        /// </summary>
        [HttpPost]
        [Route("dieuchuyen")]
        [JwtAuthorize(Right = "F_DIEUCHUYEN_ADD")]
        public IHttpActionResult CreateDieuChuyen([FromBody] DieuChuyenInput input)
        {
            try
            {
                if (input == null) return BadRequest("Thông tin điều chuyển không hợp lệ.");

                int? manv = input.MaNv ?? (input.MANV.HasValue ? (int?)input.MANV.Value : null);
                if (!manv.HasValue) return BadRequest("Vui lòng chọn nhân viên cần điều chuyển.");

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                using (var db = new MyEntities())
                {
                    string soqd = !string.IsNullOrWhiteSpace(input.SoQd) ? input.SoQd : (!string.IsNullOrWhiteSpace(input.SOQDDIEUCHUYEN) ? input.SOQDDIEUCHUYEN : $"{DateTime.Now:yyyyMMdd}/{manv.Value}/QĐ-ĐC");

                    var nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == manv.Value);
                    decimal? mapbCu = input.IdPb.HasValue ? (decimal?)input.IdPb.Value : (input.MAPB ?? (nv != null ? nv.IDPB : null));
                    decimal? mapbMoi = input.IdPb2.HasValue ? (decimal?)input.IdPb2.Value : input.MAPB2;

                    DateTime? ngayDc = null;
                    if (input.Ngay.HasValue) ngayDc = input.Ngay;
                    else if (!string.IsNullOrWhiteSpace(input.NgayStr) && DateTime.TryParse(input.NgayStr, out var nd)) ngayDc = nd;
                    else if (input.NGAYDC.HasValue) ngayDc = input.NGAYDC;
                    else ngayDc = DateTime.Now;

                    var dc = new TB_DIEUCHUYEN_NHANVIEN
                    {
                        SOQDDIEUCHUYEN = soqd,
                        MANV = manv.Value,
                        MAPB = mapbCu,
                        MAPB2 = mapbMoi,
                        NGAYDC = ngayDc,
                        LYDODC = input.LyDo ?? input.LYDODC ?? "Điều động nhân sự theo nhu cầu tổ chức",
                        GHICHU = input.GhiChu ?? input.GHICHU ?? "",
                        CREATED_DATE = DateTime.Now,
                        CREATED_BY = currentUserId
                    };

                    db.TB_DIEUCHUYEN_NHANVIEN.Add(dc);

                    // Cập nhật ngay phòng ban mới trong bảng nhân viên
                    if (nv != null && mapbMoi.HasValue)
                    {
                        nv.IDPB = mapbMoi.Value;
                        nv.UPDATED_DATE = DateTime.Now;
                        nv.UPDATED_BY = currentUserId;
                    }

                    db.SaveChanges();
                    return Ok(dc);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thêm quyết định điều chuyển: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thêm quyết định điều chuyển." });
            }
        }

        /// <summary>
        /// DELETE: api/dieuchuyen/{soqd}
        /// Xóa quyết định điều chuyển nhân sự (Yêu cầu quyền F_DIEUCHUYEN_DELETE)
        /// </summary>
        [HttpDelete]
        [Route("dieuchuyen/{*soqd}")]
        [Route("dieuchuyen")]
        [JwtAuthorize(Right = "F_DIEUCHUYEN_DELETE")]
        public IHttpActionResult DeleteDieuChuyen(string soqd = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(soqd)) return BadRequest("Vui lòng cung cấp số quyết định cần xóa.");

                soqd = Uri.UnescapeDataString(soqd).Trim();
                using (var db = new MyEntities())
                {
                    var item = db.TB_DIEUCHUYEN_NHANVIEN.FirstOrDefault(d => d.SOQDDIEUCHUYEN == soqd);
                    if (item != null)
                    {
                        db.TB_DIEUCHUYEN_NHANVIEN.Remove(item);
                        db.SaveChanges();
                    }
                    return Ok(new { success = true, message = $"Đã xóa quyết định điều chuyển {soqd}." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa quyết định điều chuyển: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa quyết định điều chuyển." });
            }
        }
    }

    public class NangLuongInput
    {
        public string SoQd { get; set; }
        public string SOQDNL { get; set; }
        public string SoHd { get; set; }
        public string SOHD { get; set; }
        public int? MaNv { get; set; }
        public decimal? MANV { get; set; }
        public decimal? HeSoLuongHienTai { get; set; }
        public decimal? HESOLUONG_NOW { get; set; }
        public decimal? HeSoLuongMoi { get; set; }
        public decimal? HESOLUONG_NEW { get; set; }
        public DateTime? NgayKy { get; set; }
        public string NgayKyStr { get; set; }
        public DateTime? NGAYKYNL { get; set; }
        public DateTime? NgayLenLuong { get; set; }
        public string NgayLenLuongStr { get; set; }
        public DateTime? NGAYLENLUONG { get; set; }
        public string GhiChu { get; set; }
        public string GHICHUNL { get; set; }
    }

    public class DieuChuyenInput
    {
        public string SoQd { get; set; }
        public string SOQDDIEUCHUYEN { get; set; }
        public int? MaNv { get; set; }
        public decimal? MANV { get; set; }
        public int? IdPb { get; set; }
        public decimal? MAPB { get; set; }
        public int? IdPb2 { get; set; }
        public decimal? MAPB2 { get; set; }
        public DateTime? Ngay { get; set; }
        public string NgayStr { get; set; }
        public DateTime? NGAYDC { get; set; }
        public string LyDo { get; set; }
        public string LYDODC { get; set; }
        public string GhiChu { get; set; }
        public string GHICHU { get; set; }
    }
}
