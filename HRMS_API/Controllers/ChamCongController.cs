using Bu.CLASS_CHAMCONG;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/chamcong")]
    public class ChamCongController : ApiController
    {
        private readonly KYCONGCHITIET _kyCongCTBus = new KYCONGCHITIET();
        private readonly BANGCONG_NV_CHITIET _bcChiTietBus = new BANGCONG_NV_CHITIET();

        /// <summary>
        /// GET: api/chamcong/kycong
        /// Lấy danh sách toàn bộ các kỳ công chấm công
        /// </summary>
        [HttpGet]
        [Route("kycong")]
        public IHttpActionResult GetKyCong()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_KYCONG
                        .OrderByDescending(x => x.MAKYCONG)
                        .Select(x => new
                        {
                            MAKYCONG = (int)x.MAKYCONG,
                            THANG = (int?)x.THANG,
                            NAM = (int?)x.NAM,
                            KHOA = (int?)x.KHOA,
                            NGAYCONGTRONGTHANG = (int?)x.NGAYCONGTRONGTHANG,
                            TRANGTHAI = (int?)x.TRANGTHAI,
                            NGAYTINHCONG = x.NGAYTINHCONG
                        })
                        .ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh sách kỳ công: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/chamcong/kycongchitiet?makycong={makycong}
        /// Lấy bảng chấm công chi tiết theo ngày (D1..D31) của tất cả nhân viên trong kỳ
        /// </summary>
        [HttpGet]
        [Route("kycongchitiet")]
        public IHttpActionResult GetKyCongChiTiet(int makycong = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (makycong <= 0)
                    {
                        var latest = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                        if (latest != null)
                        {
                            makycong = (int)latest.MAKYCONG;
                        }
                    }

                    var list = db.TB_KYCONGCHITIET
                        .Where(x => makycong <= 0 || x.MAKYCONG == makycong)
                        .ToList();

                    return Ok(new
                    {
                        makycong = makycong,
                        total = list.Count,
                        items = list
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải bảng chấm công chi tiết: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/chamcong/nhanvien?makycong={makycong}&manv={manv}
        /// Lấy chi tiết chấm công từng ngày của một nhân viên trong kỳ công
        /// </summary>
        [HttpGet]
        [Route("nhanvien")]
        public IHttpActionResult GetBangCongNhanVien(int makycong, int manv)
        {
            try
            {
                var list = _bcChiTietBus.getBangCongCT(makycong, manv);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải chi tiết chấm công nhân viên #" + manv + ": " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/chamcong/loaica
        /// Danh mục các loại ca làm việc
        /// </summary>
        [HttpGet]
        [Route("loaica")]
        public IHttpActionResult GetLoaiCa()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_LOAICA.Select(x => new
                    {
                        IDLOAICA = (int)x.IDLOAICA,
                        TENLOAICA = x.TENLOAICA,
                        HESOLOAICA = x.HESOLOAICA
                    }).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh mục loại ca: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/chamcong/loaicong
        /// Danh mục các loại ngày công
        /// </summary>
        [HttpGet]
        [Route("loaicong")]
        public IHttpActionResult GetLoaiCong()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_LOAICONG.Select(x => new
                    {
                        IDLOAICONG = (int)x.IDLOAICONG,
                        TENLOAICONG = x.TENLC,
                        HESOLOAICONG = x.HESOLOAICONG
                    }).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh mục loại công: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/chamcong/phatsinh
        /// Phát sinh tự động bảng công chi tiết cho tháng/năm
        /// </summary>
        [HttpPost]
        [Route("phatsinh")]
        public IHttpActionResult PhatSinhKyCong([FromBody] PhatSinhKyCongParam param)
        {
            try
            {
                if (param == null || param.Thang < 1 || param.Thang > 12 || param.Nam < 2000)
                {
                    return BadRequest("Thông tin tháng hoặc năm không hợp lệ.");
                }

                _kyCongCTBus.phatSinhKyCongChiTiet(param.MaCty > 0 ? param.MaCty : 1, param.Thang, param.Nam, param.IdUser ?? 1);
                return Ok(new
                {
                    success = true,
                    message = $"Đã phát sinh kỳ công chi tiết tháng {param.Thang}/{param.Nam} thành công."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi phát sinh kỳ công chi tiết: " + ex.Message, ex));
            }
        }
    }

    public class PhatSinhKyCongParam
    {
        public int MaCty { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int? IdUser { get; set; }
    }
}
