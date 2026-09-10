using Bu.CLASS_CHAMCONG;
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
    [RoutePrefix("api/bangluong")]
    public class BangLuongController : ApiController
    {
        private readonly BANGLUONG _bangLuongBus = new BANGLUONG();

        /// <summary>
        /// GET: api/bangluong?makycong={makycong}
        /// Lấy danh sách bảng lương chi tiết. Nếu không truyền makycong, tự động lấy kỳ công mới nhất.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetBangLuong(int makycong = 0)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (makycong <= 0)
                    {
                        var latestKc = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                        if (latestKc != null)
                        {
                            makycong = (int)latestKc.MAKYCONG;
                        }
                    }

                    var raw = (from bl in db.TB_BANGLUONG
                               where makycong <= 0 || bl.MAKYCONG == makycong
                               join nv in db.TB_NHANVIEN on bl.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               select new { bl, nv.HOTEN }).ToList();

                    var result = raw.Select(x => new BANGLUONG_DTO
                    {
                        IDBL = x.bl.IDBL,
                        MANV = x.bl.MANV,
                        HOTEN = x.HOTEN ?? "",
                        MAKYCONG = x.bl.MAKYCONG,
                        THANG = x.bl.THANG,
                        NAM = x.bl.NAM,
                        CONG_CHUAN = x.bl.CONG_CHUAN,
                        CONG_THUCTE = x.bl.CONG_THUCTE,
                        CONG_LAMDEM = x.bl.CONG_LAMDEM,
                        DAILY_RATE = x.bl.DAILY_RATE,
                        DAILY_ALLOWANCE = x.bl.DAILY_ALLOWANCE,
                        LUONG_CONG_THUCTE = x.bl.LUONG_CONG_THUCTE,
                        PHUCAP_CONG_THUCTE = x.bl.PHUCAP_CONG_THUCTE,
                        TIEN_TANGCA = x.bl.TIEN_TANGCA,
                        TIEN_CHUYENCAN = x.bl.TIEN_CHUYENCAN,
                        TIEN_AN_CA = x.bl.TIEN_AN_CA,
                        KHOAN_CONG_KHAC = x.bl.KHOAN_CONG_KHAC,
                        TIEN_BHXH_TRICH = x.bl.TIEN_BHXH_TRICH,
                        TIEN_TAMUNG = x.bl.TIEN_TAMUNG,
                        KHOAN_TRU_KHAC = x.bl.KHOAN_TRU_KHAC,
                        THUC_LINH = x.bl.THUC_LINH
                    }).ToList();

                    return Ok(new
                    {
                        makycong = makycong,
                        total = result.Count,
                        items = result
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải bảng lương: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/bangluong/kycong
        /// Danh sách các kỳ công tính lương
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
        /// POST: api/bangluong/tinhluong
        /// Kích hoạt tính lương cho kỳ công
        /// </summary>
        [HttpPost]
        [Route("tinhluong")]
        public IHttpActionResult TinhLuong([FromBody] TinhLuongParam param)
        {
            try
            {
                if (param == null || param.Makycong <= 0)
                {
                    return BadRequest("Vui lòng cung cấp mã kỳ công hợp lệ.");
                }

                _bangLuongBus.TinhLuongKyCong(param.Makycong, param.IdUser ?? 1);
                return Ok(new
                {
                    success = true,
                    message = $"Đã tính toán bảng lương thành công cho kỳ công {param.Makycong}."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tính lương kỳ công: " + ex.Message, ex));
            }
        }
    }

    public class TinhLuongParam
    {
        public int Makycong { get; set; }
        public int? IdUser { get; set; }
    }
}
