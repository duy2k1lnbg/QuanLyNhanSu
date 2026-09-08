using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
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
                return InternalServerError(new Exception("Lỗi khi tải danh sách nâng lương: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/nangluong
        /// Thêm quyết định nâng lương mới
        /// </summary>
        [HttpPost]
        [Route("nangluong")]
        public IHttpActionResult CreateNangLuong([FromBody] TB_NANGLUONG_NHANVIEN nl)
        {
            try
            {
                if (nl == null || !nl.MANV.HasValue) return BadRequest("Thông tin quyết định nâng lương không hợp lệ.");

                using (var db = new MyEntities())
                {
                    if (string.IsNullOrWhiteSpace(nl.SOQDNL))
                    {
                        nl.SOQDNL = $"{DateTime.Now:yyyyMMdd}/{nl.MANV}/QĐ-NL";
                    }

                    nl.CREATED_DATE = DateTime.Now;
                    nl.CREATED_BY = 1;
                    db.TB_NANGLUONG_NHANVIEN.Add(nl);

                    // Cập nhật hệ số lương mới trong hợp đồng gần nhất của nhân viên nếu có
                    if (nl.HESOLUONG_NEW.HasValue)
                    {
                        var hd = db.TB_HOPDONG.Where(h => h.MANV == nl.MANV).OrderByDescending(h => h.NGAYBATDAU).FirstOrDefault();
                        if (hd != null)
                        {
                            hd.HESOLUONG = nl.HESOLUONG_NEW;
                        }
                    }

                    db.SaveChanges();
                    return Ok(nl);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi thêm quyết định nâng lương: " + ex.Message, ex));
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
                return InternalServerError(new Exception("Lỗi khi tải danh sách điều chuyển: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/dieuchuyen
        /// Thêm quyết định điều chuyển nhân sự
        /// </summary>
        [HttpPost]
        [Route("dieuchuyen")]
        public IHttpActionResult CreateDieuChuyen([FromBody] TB_DIEUCHUYEN_NHANVIEN dc)
        {
            try
            {
                if (dc == null || !dc.MANV.HasValue) return BadRequest("Thông tin điều chuyển không hợp lệ.");

                using (var db = new MyEntities())
                {
                    if (string.IsNullOrWhiteSpace(dc.SOQDDIEUCHUYEN))
                    {
                        dc.SOQDDIEUCHUYEN = $"{DateTime.Now:yyyyMMdd}/{dc.MANV}/QĐ-ĐC";
                    }

                    dc.CREATED_DATE = DateTime.Now;
                    dc.CREATED_BY = 1;
                    db.TB_DIEUCHUYEN_NHANVIEN.Add(dc);

                    // Tự động cập nhật phòng ban mới cho nhân viên trong bảng TB_NHANVIEN
                    if (dc.MAPB2.HasValue)
                    {
                        var nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == dc.MANV);
                        if (nv != null)
                        {
                            nv.IDPB = dc.MAPB2;
                            nv.UPDATED_DATE = DateTime.Now;
                        }
                    }

                    db.SaveChanges();
                    return Ok(dc);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi thêm quyết định điều chuyển: " + ex.Message, ex));
            }
        }
    }
}
