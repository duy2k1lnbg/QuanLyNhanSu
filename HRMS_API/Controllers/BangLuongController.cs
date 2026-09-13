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
        /// <summary>
        /// GET: api/bangluong?makycong={makycong}&dept={dept}&status={status}
        /// Lấy danh sách bảng lương chi tiết có hỗ trợ lọc theo phòng ban và trạng thái chi trả.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetBangLuong(int makycong = 0, string dept = null, string status = null)
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

                    var currentKc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                    bool isPeriodLocked = currentKc != null && (currentKc.KHOA == 1);
                    string defaultStatus = isPeriodLocked ? "Đã chi trả" : "Chờ chi trả";

                    var raw = (from bl in db.TB_BANGLUONG
                               where makycong <= 0 || bl.MAKYCONG == makycong
                               join nv in db.TB_NHANVIEN on bl.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               select new { bl, nv.HOTEN, nv.IDPB, nv.DATHOIVIEC }).ToList();

                    var pbMap = db.TB_PHONGBAN.ToDictionary(p => p.IDPB, p => p.TENPB);

                    var result = raw.Select(x => new BANGLUONG_DTO
                    {
                        IDBL = x.bl.IDBL,
                        MANV = x.bl.MANV,
                        HOTEN = x.HOTEN ?? "",
                        TENPB = (x.IDPB != null && pbMap.ContainsKey(x.IDPB.Value)) ? pbMap[x.IDPB.Value] : "",
                        IDPB = x.IDPB,
                        DATHOIVIEC = x.DATHOIVIEC,
                        KHOA = currentKc != null ? currentKc.KHOA : null,
                        TRANGTHAI_CHITRA = defaultStatus,
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

                    // Lọc theo phòng ban nếu có
                    if (!string.IsNullOrWhiteSpace(dept) && dept != "all")
                    {
                        dept = System.Web.HttpUtility.UrlDecode(dept).Trim();
                        if (int.TryParse(dept, out int idpb))
                        {
                            result = result.Where(x => x.IDPB == idpb).ToList();
                        }
                        else
                        {
                            result = result.Where(x => !string.IsNullOrEmpty(x.TENPB) && (x.TENPB.Equals(dept, StringComparison.OrdinalIgnoreCase) || x.TENPB.IndexOf(dept, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
                        }
                    }

                    // Lọc theo trạng thái chi trả nếu có
                    if (!string.IsNullOrWhiteSpace(status) && status != "all")
                    {
                        if (status == "paid" || status == "da_chi_tra")
                        {
                            result = result.Where(x => x.TRANGTHAI_CHITRA == "Đã chi trả").ToList();
                        }
                        else if (status == "pending" || status == "cho_chi_tra")
                        {
                            result = result.Where(x => x.TRANGTHAI_CHITRA == "Chờ chi trả").ToList();
                        }
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải bảng lương: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải bảng lương." });
            }
        }

        /// <summary>
        /// GET: api/bangluong/chitiet?makycong={makycong}&manv={manv}
        /// Chi tiết bảng lương của 1 nhân viên theo cấu trúc rptBaoCaoLuongNV
        /// </summary>
        [HttpGet]
        [Route("chitiet")]
        public IHttpActionResult GetChiTietLuongNV(int makycong, int manv)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                    if (bl == null)
                    {
                        return NotFound();
                    }

                    var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                    string hoten = nv != null ? nv.HOTEN : "";
                    string tenpb = "";
                    string tencv = "";
                    if (nv != null && nv.IDPB != null)
                    {
                        var pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == nv.IDPB);
                        if (pb != null) tenpb = pb.TENPB;
                    }
                    if (nv != null && nv.IDCV != null)
                    {
                        var cv = db.TB_CHUCVU.FirstOrDefault(x => x.IDCV == nv.IDCV);
                        if (cv != null) tencv = cv.TENCV;
                    }

                    int nam = (int)bl.NAM;
                    int thang = (int)bl.THANG;

                    // Lấy số giờ tăng ca
                    decimal otHours = 0;
                    var lstTangCa = db.TB_TANGCA.Where(x => x.MANV == manv && x.THANG == thang && x.NAM == nam).ToList();
                    if (lstTangCa.Count > 0)
                    {
                        otHours = (decimal)lstTangCa.Sum(x => x.SOGIO ?? 0);
                    }

                    // Lấy phụ cấp chi tiết
                    var phucaps = db.TB_NHANVIEN_PHUCAP.Where(x => x.MANV == manv).ToList();
                    decimal pcTrachNhiem = phucaps.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN ?? 0;
                    decimal pcChuyenCan = phucaps.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN ?? 0;
                    decimal pcNhaO = phucaps.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN ?? 0;
                    decimal pcNgonNgu = phucaps.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN ?? 0;
                    decimal pcThamNien = phucaps.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN ?? 0;
                    decimal pcDiLai = phucaps.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN ?? 0;
                    decimal pcKhac = phucaps.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN ?? 0;
                    decimal sumAllowances = pcTrachNhiem + pcChuyenCan + pcNhaO + pcNgonNgu + pcThamNien + pcDiLai + pcKhac;

                    decimal dailyRate = bl.DAILY_RATE ?? 0;
                    decimal congChuan = bl.CONG_CHUAN ?? 26;
                    decimal luongCoBan = dailyRate * congChuan;
                    decimal tongCoBanTroCap = luongCoBan + sumAllowances;

                    decimal luongCongThucTe = bl.LUONG_CONG_THUCTE ?? 0;
                    decimal phuCapCongThucTe = bl.PHUCAP_CONG_THUCTE ?? 0;
                    decimal tienChuyenCan = bl.TIEN_CHUYENCAN ?? 0;
                    decimal tienAnCa = bl.TIEN_AN_CA ?? 0;
                    decimal khoanCongKhac = bl.KHOAN_CONG_KHAC ?? 0;
                    decimal tongNgayCongThucTe = luongCongThucTe + phuCapCongThucTe + tienChuyenCan + tienAnCa + khoanCongKhac;

                    decimal tienTangCa = bl.TIEN_TANGCA ?? 0;
                    decimal totalGross = tongNgayCongThucTe + tienTangCa;

                    decimal insuranceBase = 0;
                    decimal bhxh = 0;
                    decimal bhyt = 0;
                    decimal bhtn = 0;
                    if (bl.TIEN_BHXH_TRICH != null && bl.TIEN_BHXH_TRICH > 0)
                    {
                        insuranceBase = Math.Round(bl.TIEN_BHXH_TRICH.Value / 0.105m, 0);
                        bhxh = Math.Round(insuranceBase * 0.08m, 0);
                        bhyt = Math.Round(insuranceBase * 0.015m, 0);
                        bhtn = Math.Round(insuranceBase * 0.01m, 0);
                    }

                    decimal thueTNCN = bl.KHOAN_TRU_KHAC ?? 0;
                    decimal tienTamUng = bl.TIEN_TAMUNG ?? 0;
                    decimal thucLinh = bl.THUC_LINH ?? (totalGross - (bhxh + bhyt + bhtn + thueTNCN + tienTamUng));

                    var result = new
                    {
                        IDBL = bl.IDBL,
                        MANV = bl.MANV,
                        HOTEN = hoten,
                        TENPB = tenpb,
                        TENCV = tencv,
                        MAKYCONG = bl.MAKYCONG,
                        THANG = thang,
                        NAM = nam,
                        CONG_CHUAN = congChuan,
                        CONG_THUCTE = bl.CONG_THUCTE ?? 0,
                        CONG_LAMDEM = bl.CONG_LAMDEM ?? 0,
                        DAILY_RATE = dailyRate,
                        DAILY_ALLOWANCE = bl.DAILY_ALLOWANCE ?? 0,
                        LUONG_CO_BAN = luongCoBan,

                        // Phụ cấp chi tiết
                        PC_TRACH_NHIEM = pcTrachNhiem,
                        PC_CHUYEN_CAN = pcChuyenCan,
                        PC_NHA_O = pcNhaO,
                        PC_NGON_NGU = pcNgonNgu,
                        PC_THAM_NIEN = pcThamNien,
                        PC_DI_LAI = pcDiLai,
                        PC_KHAC = pcKhac,
                        SUM_ALLOWANCES = sumAllowances,
                        TONG_CO_BAN_TRO_CAP = tongCoBanTroCap,

                        // Chi tiết ngày công thực tế
                        LUONG_CONG_THUCTE = luongCongThucTe,
                        PHUCAP_CONG_THUCTE = phuCapCongThucTe,
                        TIEN_CHUYENCAN = tienChuyenCan,
                        TIEN_AN_CA = tienAnCa,
                        KHOAN_CONG_KHAC = khoanCongKhac,
                        TONG_NGAY_CONG_THUCTE = tongNgayCongThucTe,

                        // Tăng ca
                        OT_HOURS = otHours,
                        TIEN_TANGCA = tienTangCa,
                        TOTAL_GROSS = totalGross,

                        // Bảo hiểm & Khấu trừ
                        INSURANCE_BASE = insuranceBase,
                        BHXH_8 = bhxh,
                        BHYT_15 = bhyt,
                        BHTN_1 = bhtn,
                        TIEN_BHXH_TRICH = bl.TIEN_BHXH_TRICH ?? (bhxh + bhyt + bhtn),
                        PHI_CONG_DOAN = 0,
                        KHOAN_TRU_KHAC = thueTNCN,
                        TIEN_TAMUNG = tienTamUng,
                        TONG_KHAU_TRU = (bhxh + bhyt + bhtn + thueTNCN + tienTamUng),

                        // Thực lĩnh
                        THUC_LINH = thucLinh
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải chi tiết bảng lương nhân viên: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải chi tiết bảng lương." });
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
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách kỳ công." });
            }
        }

        /// <summary>
        /// POST: api/bangluong/tinhluong
        /// Kích hoạt tính lương cho kỳ công (Yêu cầu quyền F_BANGLUONG_CALC)
        /// </summary>
        [HttpPost]
        [Route("tinhluong")]
        [JwtAuthorize(Right = "F_BANGLUONG_CALC")]
        public IHttpActionResult TinhLuong([FromBody] TinhLuongParam param)
        {
            try
            {
                if (param == null || param.Makycong <= 0)
                {
                    return BadRequest("Vui lòng cung cấp mã kỳ công hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                _bangLuongBus.TinhLuongKyCong(param.Makycong, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = $"Đã tính toán bảng lương thành công cho kỳ công {param.Makycong}."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tính lương kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi trong quá trình tính toán lương." });
            }
        }

        /// <summary>
        /// POST: api/bangluong/khoa
        /// Khóa hoặc mở khóa kỳ lương (chi trả / chờ chi trả)
        /// </summary>
        [HttpPost]
        [Route("khoa")]
        public IHttpActionResult ToggleKhoa([FromBody] KhoaKyCongParam param)
        {
            try
            {
                if (param == null || param.Makycong <= 0)
                {
                    return BadRequest("Vui lòng cung cấp mã kỳ công hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == param.Makycong);
                    if (kc == null)
                    {
                        return NotFound();
                    }

                    var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                    int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                    kc.KHOA = param.Khoa ? 1 : 0;
                    kc.UPDATED_BY = currentUserId;
                    kc.UPDATED_DATE = DateTime.Now;

                    db.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        makycong = kc.MAKYCONG,
                        khoa = kc.KHOA == 1,
                        trangthai = kc.KHOA == 1 ? "Đã chi trả" : "Chờ chi trả",
                        message = param.Khoa ? $"Đã khóa sổ và xác nhận chi trả cho kỳ công {param.Makycong}." : $"Đã mở khóa kỳ công {param.Makycong} về trạng thái chờ chi trả."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi khóa/mở khóa kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi cập nhật trạng thái kỳ công: " + ex.Message });
            }
        }
    }

    public class TinhLuongParam
    {
        public int Makycong { get; set; }
        public int? IdUser { get; set; }
    }

    public class KhoaKyCongParam
    {
        public int Makycong { get; set; }
        public bool Khoa { get; set; }
    }
}
