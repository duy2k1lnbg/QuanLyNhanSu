using DA;
using HRMS_API.Filters;
using HRMS_API.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    /// <summary>
    /// API tự phục vụ (Self-Service) dành cho ứng dụng di động Mobile.
    /// Toàn bộ endpoint áp dụng cơ chế Self-Scope nghiêm ngặt: 
    /// Định danh nhân viên (MANV) được trích xuất duy nhất từ JWT Token đã xác thực.
    /// Tuyệt đối không chấp nhận tham số MANV từ query string, request body hoặc header để tránh lỗi IDOR.
    /// </summary>
    [JwtAuthorize]
    [RoutePrefix("api/me")]
    public class MeController : ApiController
    {
        private class PayrollStatusRow
        {
            public string TRANG_THAI { get; set; }
            public string TRANGTHAI_CHITRA { get; set; }
        }

        private static bool IsPayrollPublished(MyEntities db, TB_BANGLUONG bl, out string blStatus, out string trangThaiChiTra)
        {
            var pub = Bu.CLASS_PAYROLL.PayrollPublicationHelper.EvaluatePublication(db, bl);
            blStatus = pub.Status;
            trangThaiChiTra = pub.PaymentStatusText;
            return pub.IsPublished;
        }

        private bool TryGetAuthenticatedEmployee(MyEntities db, out TB_SYS_USER user, out TB_NHANVIEN nv, out IHttpActionResult errorResult)
        {
            user = null;
            nv = null;
            errorResult = null;

            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId))
            {
                errorResult = Content(HttpStatusCode.Unauthorized, new { success = false, message = "Phiên làm việc không hợp lệ hoặc đã hết hạn." });
                return false;
            }

            user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == userId);
            if (user == null)
            {
                errorResult = Content(HttpStatusCode.Unauthorized, new { success = false, message = "Không tìm thấy thông tin tài khoản người dùng." });
                return false;
            }

            if ((user.DISABLED ?? 0) == 1)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new { success = false, message = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ Quản trị viên." });
                return false;
            }

            // Phân giải hồ sơ nhân viên và quyền Mobile từ TB_USER_EMPLOYEE_MAPPING nếu có
            int isMobileEnabled = 1;
            try
            {
                var mapRow = db.Database.SqlQuery<MappingCheckRow>(
                    "SELECT EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                    new OracleParameter("p0", user.IDUSER)
                ).FirstOrDefault();

                if (mapRow != null)
                {
                    if (mapRow.EMPLOYEE_ID.HasValue && mapRow.EMPLOYEE_ID.Value > 0)
                    {
                        user.MANV = mapRow.EMPLOYEE_ID.Value;
                    }
                    if (mapRow.IS_MOBILE_ENABLED.HasValue)
                    {
                        isMobileEnabled = (int)mapRow.IS_MOBILE_ENABLED.Value;
                    }
                }
            }
            catch { }

            string clientType = (jwtUser?.ClientType ?? user.CLIENT_TYPE ?? "ALL").Trim().ToUpperInvariant();
            if (clientType == "MOBILE" && isMobileEnabled == 0)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new { success = false, message = "Tài khoản chưa được kích hoạt quyền truy cập ứng dụng di động (Mobile Access)." });
                return false;
            }

            if (!user.MANV.HasValue || user.MANV.Value <= 0)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new { success = false, message = "Tài khoản chưa được liên kết với hồ sơ nhân viên. Vui lòng liên hệ bộ phận nhân sự." });
                return false;
            }

            decimal manv = user.MANV.Value;
            nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == manv && (n.DELETED_BY == null));
            if (nv == null)
            {
                errorResult = Content(HttpStatusCode.NotFound, new { success = false, message = "Không tìm thấy hồ sơ nhân sự tương ứng với tài khoản này." });
                return false;
            }

            if ((nv.DATHOIVIEC ?? 0) == 1)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new { success = false, message = "Hồ sơ nhân viên liên kết đã thôi việc. Tài khoản tạm dừng hoạt động." });
                return false;
            }

            return true;
        }

        /// <summary>
        /// GET: api/me
        /// Lấy thông tin phiên làm việc của người dùng hiện tại
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetMe()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);

                    string empCode = null;
                    try
                    {
                        empCode = db.Database.SqlQuery<string>(
                            "SELECT EMPLOYEE_CODE FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND ROWNUM = 1",
                            new OracleParameter("p0", nv.MANV)
                        ).FirstOrDefault();
                    }
                    catch { }

                    var result = new MobileMeDto
                    {
                        IdUser = (int)user.IDUSER,
                        Username = user.USERNAME,
                        FullName = nv.HOTEN ?? user.FULLNAME ?? user.USERNAME,
                        Manv = user.MANV,
                        EmployeeCode = empCode,
                        ClientType = user.CLIENT_TYPE ?? "ALL",
                        MaCty = user.MACTY,
                        MaDvi = user.MADVI,
                        IsAdmin = jwtUser != null && jwtUser.IsAdmin,
                        Rights = jwtUser?.Rights ?? new List<string>()
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải thông tin tài khoản." });
            }
        }

        /// <summary>
        /// GET: api/me/dashboard
        /// Tổng hợp dữ liệu tóm tắt phục vụ màn hình Trang chủ Mobile (tối ưu hóa lượt gọi mạng)
        /// </summary>
        [HttpGet]
        [Route("dashboard")]
        public IHttpActionResult GetDashboard()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    var dashboard = new MobileDashboardDto();

                    // 1. Tóm tắt hồ sơ nhân viên
                    var pb = nv.IDPB.HasValue ? db.TB_PHONGBAN.FirstOrDefault(p => p.IDPB == nv.IDPB) : null;
                    var cv = nv.IDCV.HasValue ? db.TB_CHUCVU.FirstOrDefault(c => c.IDCV == nv.IDCV) : null;

                    dashboard.ProfileSummary = new MobileProfileDto
                    {
                        Manv = nv.MANV,
                        Hoten = nv.HOTEN,
                        Gioitinh = nv.IDGT == 1 ? "Nam" : "Nữ",
                        TenPhongBan = pb?.TENPB ?? "Chưa phân bổ",
                        TenChucVu = cv?.TENCV ?? "Nhân viên",
                        AvatarBase64 = nv.HINHANH != null && nv.HINHANH.Length > 0 ? Convert.ToBase64String(nv.HINHANH) : null
                    };

                    // 2. Chấm công kỳ gần nhất
                    var latestKc = db.TB_KYCONG.OrderByDescending(k => k.MAKYCONG).FirstOrDefault();
                    if (latestKc != null)
                    {
                        int makycong = (int)latestKc.MAKYCONG;
                        var kcct = db.TB_KYCONGCHITIET.FirstOrDefault(k => k.MAKYCONG == makycong && k.MANV == manv);

                        dashboard.AttendanceSummary = new MobileAttendanceSummaryDto
                        {
                            Makycong = makycong,
                            Thang = (int)(latestKc.THANG ?? (makycong % 100)),
                            Nam = (int)(latestKc.NAM ?? (makycong / 100)),
                            TongNgayCong = kcct?.TONGNGAYCONG.HasValue == true ? (decimal)kcct.TONGNGAYCONG.Value : 0,
                            NgayCongChuan = 26,
                            NgayPhep = kcct?.NGAYPHEP.HasValue == true ? (decimal)kcct.NGAYPHEP.Value : 0,
                            CongNgay = kcct?.TONGNGAYCONG.HasValue == true ? (decimal)kcct.TONGNGAYCONG.Value : 0
                        };
                    }

                    // 3. Bảng lương kỳ gần nhất đã được phép công bố
                    var allBls = db.TB_BANGLUONG
                        .Where(b => b.MANV == manv)
                        .OrderByDescending(b => b.MAKYCONG)
                        .ToList();

                    TB_BANGLUONG publishedBl = null;
                    string blStatus = null;
                    string trangThaiChiTra = null;

                    foreach (var candidate in allBls)
                    {
                        if (IsPayrollPublished(db, candidate, out blStatus, out trangThaiChiTra))
                        {
                            publishedBl = candidate;
                            break;
                        }
                    }

                    if (publishedBl != null)
                    {
                        dashboard.PayrollSummary = new MobilePayrollDto
                        {
                            Idbl = publishedBl.IDBL,
                            Makycong = (int)publishedBl.MAKYCONG,
                            Thang = publishedBl.THANG,
                            Nam = publishedBl.NAM,
                            LuongCoBan = publishedBl.LUONG_CONG_THUCTE ?? 0,
                            CongThucTe = publishedBl.CONG_THUCTE ?? 0,
                            ThucLinh = publishedBl.THUC_LINH ?? 0,
                            TrangThaiChiTra = trangThaiChiTra
                        };
                    }
                    else
                    {
                        dashboard.PayrollSummary = null;
                    }

                    // 4. Cảnh báo hợp đồng sắp hết hạn (trong vòng 30 ngày)
                    var now = DateTime.Now;
                    var activeContracts = db.TB_HOPDONG
                        .Where(h => h.MANV == manv && h.DEL_DATE == null)
                        .OrderByDescending(h => h.NGAYBATDAU)
                        .ToList();

                    var latestContract = activeContracts.FirstOrDefault();
                    if (latestContract != null && latestContract.NGAYKETTHUC.HasValue)
                    {
                        var daysLeft = (latestContract.NGAYKETTHUC.Value - now).TotalDays;
                        if (daysLeft >= 0 && daysLeft <= 30)
                        {
                            dashboard.HasExpiringContract = true;
                            dashboard.ExpiringContractInfo = $"Hợp đồng số {latestContract.SOHD} sẽ hết hạn sau {Math.Ceiling(daysLeft)} ngày (Ngày hết hạn: {latestContract.NGAYKETTHUC.Value:dd/MM/yyyy})";
                        }
                    }

                    // 5. Thông báo gần đây (top 3 thông báo mới nhất)
                    string userCty = user.MACTY;
                    var notifications = db.TB_THONGBAO
                        .Where(t => (t.TRANGTHAI == null || t.TRANGTHAI == true) &&
                                    (t.MACTY == null || t.MACTY == userCty) &&
                                    (t.NGAY_HETHAN == null || t.NGAY_HETHAN >= now))
                        .OrderByDescending(t => t.IS_PINNED)
                        .ThenByDescending(t => t.NGAYDANG)
                        .Take(3)
                        .ToList();

                    foreach (var n in notifications)
                    {
                        dashboard.RecentNotifications.Add(new MobileNotificationDto
                        {
                            Id = n.ID,
                            Tieude = n.TIEUDE,
                            Noidung = n.NOIDUNG != null && n.NOIDUNG.Length > 120 ? n.NOIDUNG.Substring(0, 120) + "..." : n.NOIDUNG,
                            Nguoidang = n.NGUOIDANG,
                            Ngaydang = n.NGAYDANG.ToString("dd/MM/yyyy"),
                            Loaitb = n.LOAI_TB ?? "Thông báo chung",
                            IsPinned = n.IS_PINNED.HasValue && n.IS_PINNED.Value
                        });
                    }

                    dashboard.UnreadNotificationCount = notifications.Count;

                    return Ok(new { success = true, data = dashboard });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/dashboard Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải dữ liệu trang chủ." });
            }
        }

        /// <summary>
        /// GET: api/me/profile
        /// Lấy thông tin hồ sơ nhân sự 360 của người dùng đăng nhập
        /// </summary>
        [HttpGet]
        [Route("profile")]
        public IHttpActionResult GetProfile()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var pb = nv.IDPB.HasValue ? db.TB_PHONGBAN.FirstOrDefault(p => p.IDPB == nv.IDPB) : null;
                    var bp = nv.IDBP.HasValue ? db.TB_BOPHAN.FirstOrDefault(b => b.IDBP == nv.IDBP) : null;
                    var cv = nv.IDCV.HasValue ? db.TB_CHUCVU.FirstOrDefault(c => c.IDCV == nv.IDCV) : null;
                    var td = nv.IDTD.HasValue ? db.TB_TRINHDO.FirstOrDefault(t => t.IDTD == nv.IDTD) : null;

                    var earliestContract = db.TB_HOPDONG
                        .Where(h => h.MANV == nv.MANV && h.DEL_DATE == null)
                        .OrderBy(h => h.NGAYBATDAU ?? h.NGAYKY)
                        .FirstOrDefault();

                    DateTime? joinDate = earliestContract?.NGAYBATDAU ?? earliestContract?.NGAYKY ?? nv.CREATED_DATE;

                    string empCode = null;
                    try
                    {
                        empCode = db.Database.SqlQuery<string>(
                            "SELECT EMPLOYEE_CODE FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND ROWNUM = 1",
                            new OracleParameter("p0", nv.MANV)
                        ).FirstOrDefault();
                    }
                    catch { }

                    var profile = new MobileProfileDto
                    {
                        Manv = nv.MANV,
                        EmployeeCode = empCode,
                        Hoten = nv.HOTEN,
                        Gioitinh = nv.IDGT == 1 ? "Nam" : "Nữ",
                        Ngaysinh = nv.NGAYSINH.HasValue ? nv.NGAYSINH.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật",
                        Dienthoai = !string.IsNullOrWhiteSpace(nv.DIENTHOAI) ? nv.DIENTHOAI : "Chưa cập nhật",
                        Cccd = !string.IsNullOrWhiteSpace(nv.CCCD) ? nv.CCCD : "Chưa cập nhật",
                        Diachi = !string.IsNullOrWhiteSpace(nv.DIACHI) ? nv.DIACHI : "Chưa cập nhật",
                        TenPhongBan = pb?.TENPB ?? "Chưa cập nhật",
                        TenBoPhan = bp?.TENBP ?? "Chưa cập nhật",
                        TenChucVu = cv?.TENCV ?? "Chưa cập nhật",
                        TenTrinhDo = td?.TENTD ?? "Chưa cập nhật",
                        NgayVaoLam = joinDate.HasValue ? joinDate.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật",
                        Email = !string.IsNullOrWhiteSpace(user.USERNAME) && user.USERNAME.Contains("@") ? user.USERNAME : "Chưa cập nhật",
                        AvatarBase64 = nv.HINHANH != null && nv.HINHANH.Length > 0 ? Convert.ToBase64String(nv.HINHANH) : null,
                        TrangThaiLaoDong = (nv.DATHOIVIEC ?? 0) == 1 ? "Đã thôi việc" : "Đang làm việc"
                    };

                    return Ok(new { success = true, data = profile });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/profile Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải thông tin hồ sơ." });
            }
        }

        /// <summary>
        /// PUT: api/me/profile
        /// Cập nhật thông tin cá nhân tự phục vụ (Số điện thoại, Địa chỉ, Ảnh đại diện)
        /// Các trường HR-owned (Mã NV, Họ tên, Phòng ban, Chức vụ, Lương...) tuyệt đối Readonly (Quy tắc 32)
        /// </summary>
        [HttpPut]
        [Route("profile")]
        public IHttpActionResult UpdateProfile([FromBody] UpdateProfileRequest req)
        {
            try
            {
                if (req == null) return BadRequest("Dữ liệu cập nhật không hợp lệ.");

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    if (!string.IsNullOrWhiteSpace(req.Dienthoai))
                    {
                        nv.DIENTHOAI = req.Dienthoai.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(req.Diachi))
                    {
                        nv.DIACHI = req.Diachi.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(req.AvatarBase64))
                    {
                        try
                        {
                            string base64Data = req.AvatarBase64;
                            if (base64Data.Contains(","))
                            {
                                base64Data = base64Data.Split(',')[1];
                            }
                            nv.HINHANH = Convert.FromBase64String(base64Data);
                        }
                        catch
                        {
                            return BadRequest("Định dạng ảnh đại diện không hợp lệ.");
                        }
                    }

                    nv.UPDATED_BY = (int)user.IDUSER;
                    nv.UPDATED_DATE = DateTime.Now;
                    db.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        message = "Cập nhật hồ sơ cá nhân thành công!"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[PUT /api/me/profile Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi cập nhật hồ sơ." });
            }
        }

        /// <summary>
        /// GET: api/me/attendance?month=YYYY-MM
        /// Lấy bảng chấm công chi tiết của nhân viên trong tháng
        /// </summary>
        [HttpGet]
        [Route("attendance")]
        public IHttpActionResult GetAttendance(string month = null)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    int targetMakycong;

                    if (!string.IsNullOrWhiteSpace(month) && month.Contains("-"))
                    {
                        var parts = month.Split('-');
                        if (int.TryParse(parts[0], out int y) && int.TryParse(parts[1], out int m))
                        {
                            targetMakycong = y * 100 + m;
                        }
                        else
                        {
                            var latest = db.TB_KYCONG.OrderByDescending(k => k.MAKYCONG).FirstOrDefault();
                            targetMakycong = latest != null ? (int)latest.MAKYCONG : 202609;
                        }
                    }
                    else
                    {
                        var latest = db.TB_KYCONG.OrderByDescending(k => k.MAKYCONG).FirstOrDefault();
                        targetMakycong = latest != null ? (int)latest.MAKYCONG : 202609;
                    }

                    int thang = targetMakycong % 100;
                    int nam = targetMakycong / 100;

                    var kcct = db.TB_KYCONGCHITIET.FirstOrDefault(k => k.MAKYCONG == targetMakycong && k.MANV == manv);
                    var dailyRows = db.TB_BANGCONG_CHITIET
                        .Where(b => b.MAKYCONG == targetMakycong && b.MANV == manv)
                        .OrderBy(b => b.NGAY)
                        .ToList();

                    var result = new MobileAttendanceDto
                    {
                        Summary = new MobileAttendanceSummaryDto
                        {
                            Makycong = targetMakycong,
                            Thang = thang,
                            Nam = nam,
                            TongNgayCong = kcct?.TONGNGAYCONG.HasValue == true ? (decimal)kcct.TONGNGAYCONG.Value : 0,
                            NgayCongChuan = 26,
                            NgayPhep = kcct?.NGAYPHEP.HasValue == true ? (decimal)kcct.NGAYPHEP.Value : 0
                        }
                    };

                    decimal congNgay = 0;
                    decimal congDem = 0;
                    decimal congLe = 0;
                    decimal congCn = 0;
                    int diMuonCount = 0;

                    foreach (var row in dailyRows)
                    {
                        decimal nc = row.NGAYCONG.HasValue ? (decimal)row.NGAYCONG.Value : 0;
                        bool isDem = (row.KYHIEU == "CD" || row.KYHIEU == "Đ" || row.KYHIEU == "XĐ" ||
                                     (row.GIOVAO != null && (string.Compare(row.GIOVAO, "18:00") >= 0 || string.Compare(row.GIOVAO, "06:00") < 0)));

                        if (isDem) congDem += nc;
                        else congNgay += nc;

                        if (row.CONGNGAYLE.HasValue) congLe += (decimal)row.CONGNGAYLE.Value;
                        if (row.CONGCHUNHAT.HasValue) congCn += (decimal)row.CONGCHUNHAT.Value;

                        string trangThai = "Đủ công";
                        if (row.KYHIEU == "V" || nc == 0)
                        {
                            trangThai = "Vắng mặt";
                        }
                        else if (row.KYHIEU == "P")
                        {
                            trangThai = "Nghỉ phép";
                        }
                        else if (!string.IsNullOrEmpty(row.GIOVAO) && string.Compare(row.GIOVAO, "08:15") > 0 && !isDem)
                        {
                            trangThai = "Đi muộn";
                            diMuonCount++;
                        }

                        bool coBatThuong = (row.DU_DIEUKIEN_CHOT == false) ||
                                            (!string.IsNullOrEmpty(row.TRANGTHAI_CONG) && (row.TRANGTHAI_CONG.Contains("BAT_THUONG") || row.TRANGTHAI_CONG.Contains("ANOMALY") || row.TRANGTHAI_CONG.Contains("UNVERIFIED")));

                        result.DailyList.Add(new MobileAttendanceDailyDto
                        {
                            Ngay = row.NGAY.HasValue ? row.NGAY.Value.ToString("dd/MM") : "N/A",
                            Thu = row.THU ?? "",
                            GioVao = row.GIOVAO ?? "--:--",
                            GioRa = row.GIORA ?? "--:--",
                            NgayCong = nc,
                            KyHieu = row.KYHIEU ?? "X",
                            TrangThai = trangThai,
                            GhiChu = row.GHICHU,
                            NgayPhep = row.NGAYPHEP,
                            CongNgayLe = row.CONGNGAYLE,
                            CongChuNhat = row.CONGCHUNHAT,
                            TrangThaiCong = row.TRANGTHAI_CONG,
                            DuDieuKienChot = row.DU_DIEUKIEN_CHOT,
                            GioThucTe = row.GIAY_THUC_TE.HasValue ? Math.Round(row.GIAY_THUC_TE.Value / 3600.0, 2) : (double?)null,
                            GioHuongCong = row.GIAY_HUONG_CONG_THUONG.HasValue ? Math.Round(row.GIAY_HUONG_CONG_THUONG.Value / 3600.0, 2) : (double?)null,
                            GioOtXacNhan = row.GIAY_OT_XAC_NHAN.HasValue ? Math.Round(row.GIAY_OT_XAC_NHAN.Value / 3600.0, 2) : (double?)null,
                            GioDem = ((row.GIAY_DEM_TRONG_GIO_THUONG ?? 0) + (row.GIAY_DEM_OT ?? 0)) > 0 ? Math.Round(((row.GIAY_DEM_TRONG_GIO_THUONG ?? 0) + (row.GIAY_DEM_OT ?? 0)) / 3600.0, 2) : (double?)null,
                            PhutDiMuonViPham = row.GIAY_DI_MUON_VIPHAM.HasValue ? (int?)(row.GIAY_DI_MUON_VIPHAM.Value / 60) : null,
                            PhutVeSomViPham = row.GIAY_VE_SOM_VIPHAM.HasValue ? (int?)(row.GIAY_VE_SOM_VIPHAM.Value / 60) : null,
                            CoBatThuongChuaXacMinh = coBatThuong
                        });
                    }

                    result.Summary.CongNgay = congNgay;
                    result.Summary.CongDem = congDem;
                    result.Summary.CongLe = congLe;
                    result.Summary.CongChuNhat = congCn;
                    result.Summary.SoLanDiMuon = diMuonCount;

                    // Tổng giờ tăng ca trong tháng
                    var tangCaList = db.TB_TANGCA.Where(t => t.MANV == manv && t.THANG == thang && t.NAM == nam).ToList();
                    result.Summary.SoGioTangCa = tangCaList.Sum(t => t.SOGIO.HasValue ? (decimal)t.SOGIO.Value : 0);

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/attendance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải dữ liệu chấm công." });
            }
        }

        /// <summary>
        /// GET: api/me/payroll?year=YYYY&month=MM
        /// Lấy bảng lương thực tế chi tiết của nhân viên theo kỳ lương
        /// </summary>
        [HttpGet]
        [Route("payroll")]
        public IHttpActionResult GetPayroll(int? year = null, int? month = null)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    TB_BANGLUONG bl = null;
                    string blStatus = null;
                    string trangThaiChiTra = null;

                    if (year.HasValue && month.HasValue)
                    {
                        int targetMakycong = year.Value * 100 + month.Value;
                        var candidates = db.TB_BANGLUONG
                            .Where(b => b.MANV == manv && b.MAKYCONG == targetMakycong)
                            .ToList();

                        foreach (var c in candidates)
                        {
                            if (IsPayrollPublished(db, c, out blStatus, out trangThaiChiTra))
                            {
                                bl = c;
                                break;
                            }
                        }

                        if (bl == null)
                        {
                            return Ok(new { success = true, data = (MobilePayrollDto)null, message = "Bảng lương kỳ này chưa được công bố hoặc chưa phát sinh dữ liệu." });
                        }
                    }
                    else
                    {
                        var allBls = db.TB_BANGLUONG
                            .Where(b => b.MANV == manv)
                            .OrderByDescending(b => b.MAKYCONG)
                            .ToList();

                        foreach (var c in allBls)
                        {
                            if (IsPayrollPublished(db, c, out blStatus, out trangThaiChiTra))
                            {
                                bl = c;
                                break;
                            }
                        }

                        if (bl == null)
                        {
                            return Ok(new { success = true, data = (MobilePayrollDto)null, message = "Chưa có bảng lương nào được công bố cho bạn." });
                        }
                    }

                    var kc = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == bl.MAKYCONG);
                    bool isPeriodLocked = kc != null && kc.KHOA == 1;

                    Bu.DTO.ModernPayrollSnapshotDto snap = null;
                    if (bl.MAKYCONG >= 202601)
                    {
                        try
                        {
                            var sql = @"SELECT IDBL, IS_LEGACY, TRANG_THAI, VUNG_LUONG, LUONG_TOI_THIEU_VUNG, MUC_THAM_CHIEU_BH,
                                               LUONG_DONG_BHXH, TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD, TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD,
                                               TIEN_DOAN_PHI_NLD, TIEN_KINH_PHI_CD_NSDLD, SO_NGUOI_PHU_THUOC, GIAM_TRU_BAN_THAN,
                                               GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM, TONG_THU_NHAP_CHIU_THUE, THU_NHAP_TINH_THUE,
                                               TONG_CHI_PHI_NSDLD
                                        FROM TB_BANGLUONG WHERE IDBL = :p0";
                            snap = db.Database.SqlQuery<Bu.DTO.ModernPayrollSnapshotDto>(
                                sql,
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL)
                            ).FirstOrDefault();
                        }
                        catch { }
                    }

                    // Trạng thái chi trả đã được xác định chính xác từ dữ liệu thanh toán thực tế trong IsPayrollPublished
                    if (string.IsNullOrEmpty(trangThaiChiTra))
                    {
                        bool isPaid = snap != null && (snap.TRANG_THAI == "PAID" || snap.TRANG_THAI == "DA_CHI_TRA");
                        if (isPaid)
                        {
                            trangThaiChiTra = "Đã chi trả";
                        }
                        else if (snap != null && (snap.TRANG_THAI == "APPROVED" || snap.TRANG_THAI == "DA_DUYET"))
                        {
                            trangThaiChiTra = "Đã duyệt chi";
                        }
                        else if (isPeriodLocked)
                        {
                            trangThaiChiTra = "Đã chốt sổ";
                        }
                        else
                        {
                            trangThaiChiTra = "Dự kiến chi trả";
                        }
                    }

                    decimal tongThuNhap = bl.TONG_CONG ?? ((bl.LUONG_CONG_THUCTE ?? 0) + (bl.PHUCAP_CONG_THUCTE ?? 0) + (bl.TIEN_TANGCA ?? 0) + (bl.TIEN_CHUYENCAN ?? 0) + (bl.TIEN_AN_CA ?? 0) + (bl.KHOAN_CONG_KHAC ?? 0));
                    decimal tongKhauTru = (bl.TIEN_BHXH_TRICH ?? ((bl.TIEN_BHXH ?? 0) + (bl.TIEN_BHYT ?? 0) + (bl.TIEN_BHTN ?? 0))) + (bl.TIEN_CONG_DOAN ?? (snap?.TIEN_DOAN_PHI_NLD ?? 0)) + (bl.TIEN_TAMUNG ?? 0) + (bl.THUE_TNCN ?? 0) + (bl.KHOAN_TRU_KHAC ?? 0);
                    decimal luongCoBan = (bl.DAILY_RATE.HasValue && bl.CONG_CHUAN.HasValue && bl.CONG_CHUAN.Value > 0) ? (bl.DAILY_RATE.Value * bl.CONG_CHUAN.Value) : (bl.LUONG_CONG_THUCTE ?? 0);
                    bool isLegacy = (snap != null && snap.IS_LEGACY == 1) || (bl.MAKYCONG < 202601);

                    decimal soGioTangCa = 0;
                    try
                    {
                        soGioTangCa = db.TB_TANGCA
                            .Where(t => t.MANV == manv && t.NAM == bl.NAM && t.THANG == bl.THANG)
                            .Sum(t => (decimal?)t.SOGIO) ?? 0;
                    }
                    catch { }

                    var result = new MobilePayrollDto
                    {
                        Idbl = bl.IDBL,
                        Makycong = (int)bl.MAKYCONG,
                        Thang = bl.THANG,
                        Nam = bl.NAM,
                        LuongCoBan = luongCoBan,
                        CongChuan = bl.CONG_CHUAN ?? 0,
                        CongThucTe = bl.CONG_THUCTE ?? 0,
                        CongLamNgay = bl.CONG_LAMNGAY ?? 0,
                        CongLamDem = bl.CONG_LAMDEM ?? 0,
                        SoGioTangCa = soGioTangCa,
                        DailyRate = bl.DAILY_RATE ?? 0,
                        LuongCaNgay = (bl.DAILY_RATE.HasValue && bl.CONG_LAMNGAY.HasValue) ? (bl.DAILY_RATE.Value * bl.CONG_LAMNGAY.Value) : 0,
                        LuongCaDem = (bl.DAILY_RATE.HasValue && bl.CONG_LAMDEM.HasValue) ? (bl.DAILY_RATE.Value * bl.CONG_LAMDEM.Value * 1.30m) : 0,
                        LuongCongThucTe = bl.LUONG_CONG_THUCTE ?? 0,
                        PhuCapCongThucTe = bl.PHUCAP_CONG_THUCTE ?? 0,
                        TienTangCa = bl.TIEN_TANGCA ?? 0,
                        TienChuyenCan = bl.TIEN_CHUYENCAN ?? 0,
                        TienAnCa = bl.TIEN_AN_CA ?? 0,
                        KhoanCongKhac = bl.KHOAN_CONG_KHAC ?? 0,
                        TongThuNhap = tongThuNhap,
                        TienBhxh = bl.TIEN_BHXH ?? 0,
                        TienBhyt = bl.TIEN_BHYT ?? 0,
                        TienBhtn = bl.TIEN_BHTN ?? 0,
                        TienCongDoan = bl.TIEN_CONG_DOAN ?? (snap?.TIEN_DOAN_PHI_NLD ?? 0),
                        TienTamUng = bl.TIEN_TAMUNG ?? 0,
                        ThueTncn = bl.THUE_TNCN ?? 0,
                        HoanThue = bl.HOAN_THUE ?? 0,
                        KhoanTruKhac = bl.KHOAN_TRU_KHAC ?? 0,
                        TongKhauTru = tongKhauTru,
                        ThucLinh = bl.THUC_LINH ?? 0,
                        TrangThaiChiTra = trangThaiChiTra,

                        // Tax breakdown
                        ThuNhapChiuThue = snap?.TONG_THU_NHAP_CHIU_THUE ?? (bl.TONG_CONG ?? 0),
                        ThuNhapTinhThue = snap?.THU_NHAP_TINH_THUE ?? 0,
                        GiamTruBanThan = snap?.GIAM_TRU_BAN_THAN ?? 0,
                        GiamTruPhuThuoc = snap?.GIAM_TRU_PHU_THUOC ?? 0,
                        GiamTruBaoHiem = snap?.GIAM_TRU_BAO_HIEM ?? (bl.TIEN_BHXH_TRICH ?? 0),
                        SoNguoiPhuThuoc = snap?.SO_NGUOI_PHU_THUOC ?? 0,

                        // Explanations & Policy
                        CachTinhLuong = "Lương thực tế = Đơn giá ngày × Số công thực tế (" + (bl.DAILY_RATE ?? 0).ToString("N0") + " đ × " + (bl.CONG_THUCTE ?? 0) + " công)",
                        CachTinhThue = (bl.THUE_TNCN > 0) ? "Thuế TNCN = Thu nhập tính thuế (" + (snap?.THU_NHAP_TINH_THUE ?? 0).ToString("N0") + " đ) × Thuế suất theo biểu lũy tiến" : "Thu nhập tính thuế không vượt mức giảm trừ gia cảnh (Miễn thuế)",
                        PayrollVersion = !isLegacy ? "V2026.PROD" : "LEGACY",
                        ChinhSachApDung = !isLegacy ? "Luật BHXH 41/2024, Luật Thuế TNCN 109/2025, NĐ 293/2025/NĐ-CP" : "Chính sách lương lịch sử",
                        IsLegacy = isLegacy
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/payroll Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải bảng lương." });
            }
        }

        /// <summary>
        /// GET: api/me/contract
        /// Lấy danh sách và chi tiết các hợp đồng lao động của nhân viên
        /// </summary>
        [HttpGet]
        [Route("contract")]
        [Route("contracts")]
        public IHttpActionResult GetContract()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    var contracts = db.TB_HOPDONG
                        .Where(h => h.MANV == manv && h.DEL_DATE == null)
                        .OrderByDescending(h => h.NGAYBATDAU ?? h.NGAYKY)
                        .ToList();

                    var result = new List<MobileContractDto>();
                    var now = DateTime.Now;

                    foreach (var h in contracts)
                    {
                        bool isExpiringSoon = false;
                        if (h.NGAYKETTHUC.HasValue)
                        {
                            var daysLeft = (h.NGAYKETTHUC.Value - now).TotalDays;
                            isExpiringSoon = daysLeft >= 0 && daysLeft <= 30;
                        }

                        string tenLoaiHd = "Hợp đồng lao động";
                        if (h.LOAIHD.HasValue)
                        {
                            if (h.LOAIHD.Value == 1) tenLoaiHd = "Hợp đồng thử việc";
                            else if (h.LOAIHD.Value == 2) tenLoaiHd = "Hợp đồng xác định thời hạn (12 tháng)";
                            else if (h.LOAIHD.Value == 3) tenLoaiHd = "Hợp đồng xác định thời hạn (36 tháng)";
                            else if (h.LOAIHD.Value == 4) tenLoaiHd = "Hợp đồng không xác định thời hạn";
                        }

                        result.Add(new MobileContractDto
                        {
                            Sohd = h.SOHD,
                            Loaihd = h.LOAIHD,
                            TenLoaihd = tenLoaiHd,
                            Ngaybatdau = h.NGAYBATDAU.HasValue ? h.NGAYBATDAU.Value.ToString("dd/MM/yyyy") : "Chưa ghi nhận",
                            Ngayketthuc = h.NGAYKETTHUC.HasValue ? h.NGAYKETTHUC.Value.ToString("dd/MM/yyyy") : "Không xác định",
                            Ngayky = h.NGAYKY.HasValue ? h.NGAYKY.Value.ToString("dd/MM/yyyy") : "Chưa ghi nhận",
                            Lanky = h.LANKY ?? 1,
                            Thoihan = h.THOIHAN ?? "Không xác định",
                            LuongThoaThuan = h.LUONG_THOA_THUAN,
                            HeSoLuong = h.HESOLUONG,
                            NoiDung = h.NOIDUNG,
                            IsExpiringSoon = isExpiringSoon
                        });
                    }

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/contract Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải thông tin hợp đồng." });
            }
        }

        /// <summary>
        /// GET: api/me/insurance
        /// Lấy thông tin sổ bảo hiểm xã hội của nhân viên
        /// </summary>
        [HttpGet]
        [Route("insurance")]
        public IHttpActionResult GetInsurance()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    var bh = db.TB_BAOHIEM.FirstOrDefault(b => b.MANV == manv);

                    if (bh == null)
                    {
                        return Ok(new { success = true, data = (MobileInsuranceDto)null, message = "Chưa có thông tin bảo hiểm xã hội." });
                    }

                    var result = new MobileInsuranceDto
                    {
                        Idbh = bh.IDBH,
                        Sobh = bh.SOBH ?? "Chưa cấp số",
                        Ngaycap = bh.NGAYCAP.HasValue ? bh.NGAYCAP.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật",
                        Noicap = !string.IsNullOrWhiteSpace(bh.NOICAP) ? bh.NOICAP : "Chưa cập nhật",
                        Noikhambenh = !string.IsNullOrWhiteSpace(bh.NOIKHAMBENH) ? bh.NOIKHAMBENH : "Chưa cập nhật",
                        LuongBhxh = bh.LUONG_BHXH
                    };

                    try
                    {
                        var movements = db.Database.SqlQuery<InsuranceMovementRow>(
                            @"SELECT B.ID, B.MANV, NV.MANV AS EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME,
                                     NVL(PB.TENPB, 'Chưa phân bổ') AS DEPARTMENT_NAME,
                                     B.MAKYCONG, B.LOAI, B.NGAY_HIEULUC, B.LY_DO, B.TRANG_THAI,
                                     B.NGUOI_DUYET, NVL(U.FULLNAME, U.USERNAME) AS TEN_NGUOI_DUYET
                              FROM HR.TB_BAOHIEM_BIENDONG B
                              JOIN HR.TB_NHANVIEN NV ON B.MANV = NV.MANV
                              LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                              LEFT JOIN HR.TB_SYS_USER U ON B.NGUOI_DUYET = U.IDUSER
                              WHERE B.MANV = :p0
                              ORDER BY B.NGAY_HIEULUC DESC",
                            new OracleParameter("p0", manv)
                        ).ToList();

                        result.Movements = movements.Select(m => new MobileInsuranceMovementDto
                        {
                            Id = m.ID,
                            Manv = m.MANV,
                            MaKyCong = m.MAKYCONG,
                            Loai = m.LOAI,
                            NgayHieuLuc = m.NGAY_HIEULUC.ToString("dd/MM/yyyy"),
                            LyDo = m.LY_DO,
                            TrangThai = m.TRANG_THAI,
                            NguoiDuyet = m.NGUOI_DUYET,
                            TenNguoiDuyet = m.TEN_NGUOI_DUYET
                        }).ToList();
                    }
                    catch (Exception mEx)
                    {
                        System.Diagnostics.Trace.TraceWarning("[GetInsurance Movements Warning]: " + mEx.Message);
                    }

                    try
                    {
                        var parts = db.TB_NHANVIEN_BAOHIEM_THAM_GIA
                            .Where(p => p.MANV == manv)
                            .OrderByDescending(p => p.NGAY_BAT_DAU)
                            .ToList();

                        result.Participations = parts.Select(p => new MobileInsuranceParticipationDto
                        {
                            Id = p.ID,
                            VungLuong = p.VUNG_LUONG,
                            ThamGiaBhxh = p.THAM_GIA_BHXH,
                            ThamGiaBhyt = p.THAM_GIA_BHYT,
                            ThamGiaBhtn = p.THAM_GIA_BHTN,
                            ThamGiaTnldBnn = p.THAM_GIA_TNLD_BNN,
                            HuongTyLeTnldUuDai = p.HUONG_TY_LE_TNLD_UU_DAI,
                            LuongDongBhxhRieng = p.LUONG_DONG_BHXH_RIENG,
                            NgayBatDau = p.NGAY_BAT_DAU.ToString("dd/MM/yyyy"),
                            NgayKetThuc = p.NGAY_KET_THUC.HasValue ? p.NGAY_KET_THUC.Value.ToString("dd/MM/yyyy") : null,
                            TrangThai = p.TRANG_THAI
                        }).ToList();
                    }
                    catch (Exception pEx)
                    {
                        System.Diagnostics.Trace.TraceWarning("[GetInsurance Participations Warning]: " + pEx.Message);
                    }

                    try
                    {
                        var union = db.TB_NHANVIEN_CONG_DOAN_THAM_GIA
                            .Where(u => u.MANV == manv)
                            .OrderByDescending(u => u.NGAY_GIA_NHAP)
                            .FirstOrDefault();

                        if (union != null)
                        {
                            result.UnionParticipation = new MobileUnionParticipationDto
                            {
                                Id = union.ID,
                                LaDoanVien = union.LA_DOAN_VIEN,
                                NgayGiaNhap = union.NGAY_GIA_NHAP.ToString("dd/MM/yyyy"),
                                NgayKetThuc = union.NGAY_KET_THUC.HasValue ? union.NGAY_KET_THUC.Value.ToString("dd/MM/yyyy") : null,
                                TrangThai = union.TRANG_THAI
                            };
                        }
                    }
                    catch (Exception uEx)
                    {
                        System.Diagnostics.Trace.TraceWarning("[GetInsurance Union Warning]: " + uEx.Message);
                    }

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/insurance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải thông tin bảo hiểm." });
            }
        }

        /// <summary>
        /// GET: api/me/notifications
        /// Lấy danh sách thông báo nội bộ có phân loại cho người dùng
        /// </summary>
        [HttpGet]
        [Route("notifications")]
        public IHttpActionResult GetNotifications()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    string userCty = user.MACTY;
                    var now = DateTime.Now;

                    var notifications = db.TB_THONGBAO
                        .Where(t => (t.TRANGTHAI == null || t.TRANGTHAI == true) &&
                                    (t.MACTY == null || t.MACTY == userCty) &&
                                    (t.NGAY_HETHAN == null || t.NGAY_HETHAN >= now))
                        .OrderByDescending(t => t.IS_PINNED)
                        .ThenByDescending(t => t.NGAYDANG)
                        .Take(50)
                        .ToList();

                    var result = new List<MobileNotificationDto>();
                    foreach (var n in notifications)
                    {
                        result.Add(new MobileNotificationDto
                        {
                            Id = n.ID,
                            Tieude = n.TIEUDE,
                            Noidung = n.NOIDUNG,
                            Nguoidang = n.NGUOIDANG,
                            Ngaydang = n.NGAYDANG.ToString("dd/MM/yyyy"),
                            Loaitb = n.LOAI_TB ?? "Thông báo chung",
                            IsPinned = n.IS_PINNED.HasValue && n.IS_PINNED.Value,
                            FileDinhkem = n.FILE_DINHKEM,
                            IsUnread = true // Extension point: có thể mở rộng bảng UserNotificationRead sau này
                        });
                    }

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/notifications Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải danh sách thông báo." });
            }
        }

        /// <summary>
        /// GET: api/me/notifications/{id}
        /// Xem chi tiết một thông báo nội bộ
        /// </summary>
        [HttpGet]
        [Route("notifications/{id:int}")]
        public IHttpActionResult GetNotificationDetail(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var n = db.TB_THONGBAO.FirstOrDefault(t => t.ID == id);
                    if (n == null)
                    {
                        return NotFound();
                    }

                    var result = new MobileNotificationDto
                    {
                        Id = n.ID,
                        Tieude = n.TIEUDE,
                        Noidung = n.NOIDUNG,
                        Nguoidang = n.NGUOIDANG,
                        Ngaydang = n.NGAYDANG.ToString("dd/MM/yyyy HH:mm"),
                        Loaitb = n.LOAI_TB ?? "Thông báo chung",
                        IsPinned = n.IS_PINNED.HasValue && n.IS_PINNED.Value,
                        FileDinhkem = n.FILE_DINHKEM,
                        IsUnread = false
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/notifications/{id} Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải chi tiết thông báo." });
            }
        }

        /// <summary>
        /// GET: api/me/leave-balance
        /// Lấy số dư phép tồn và lịch sử phát sinh phép từ sổ TB_PHEP_SOPHATSINH
        /// </summary>
        [HttpGet]
        [Route("leave-balance")]
        public IHttpActionResult GetLeaveBalance()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    var txRows = db.Database.SqlQuery<LeaveTransactionRow>(
                        @"SELECT ID, MANV, NGAY, LOAI, GIAY_PHEP, IDDON, LY_DO
                          FROM HR.TB_PHEP_SOPHATSINH
                          WHERE MANV = :p0
                          ORDER BY NGAY DESC, ID DESC",
                        new OracleParameter("p0", manv)
                    ).ToList();

                    decimal tongCapSeconds = txRows.Where(x => x.LOAI == "CAP").Sum(x => x.GIAY_PHEP);
                    decimal daDungSeconds = txRows.Where(x => x.LOAI == "SU_DUNG").Sum(x => Math.Abs(x.GIAY_PHEP));
                    decimal hoanSeconds = txRows.Where(x => x.LOAI == "HOAN").Sum(x => x.GIAY_PHEP);
                    decimal conLaiSeconds = txRows.Sum(x => x.GIAY_PHEP);

                    // Nếu chưa có dòng sổ nào, fallback sang mặc định 12 ngày
                    if (!txRows.Any())
                    {
                        tongCapSeconds = 12 * 28800;
                        conLaiSeconds = 12 * 28800;
                    }

                    var result = new MobileLeaveBalanceDto
                    {
                        Manv = manv,
                        TongCapNgay = Math.Round(tongCapSeconds / 28800m, 2),
                        DaDungNgay = Math.Round(daDungSeconds / 28800m, 2),
                        ConLaiNgay = Math.Round(conLaiSeconds / 28800m, 2),
                        Transactions = txRows.Select(r => new MobileLeaveTransactionDto
                        {
                            Id = r.ID,
                            Ngay = r.NGAY.ToString("dd/MM/yyyy"),
                            Loai = r.LOAI,
                            GiayPhep = r.GIAY_PHEP,
                            SoNgay = Math.Round(Math.Abs(r.GIAY_PHEP) / 28800m, 2),
                            IdDon = r.IDDON,
                            LyDo = r.LY_DO
                        }).ToList()
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/leave-balance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải số dư phép." });
            }
        }

        /// <summary>
        /// GET: api/me/leave
        /// Lấy danh sách đơn xin nghỉ phép của nhân viên đang đăng nhập (Quy tắc 26)
        /// </summary>
        [HttpGet]
        [Route("leave")]
        public IHttpActionResult GetMyLeaveRequests()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var list = db.Database.SqlQuery<LeaveRequestRow>(
                        "SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.LOAIPHEP AS LOAI_NGHI, Y.TUNGAY AS TU_NGAY, Y.DENNGAY AS DEN_NGAY, Y.SONGAY AS SO_NGAY, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, " +
                        "NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI " +
                        "FROM HR.TB_YEUCAU_NGHIPHEP Y " +
                        "LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER " +
                        "WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                        new OracleParameter("p0", nv.MANV)
                    ).ToList();

                    var result = list.Select(r => new MobileLeaveRequestDto
                    {
                        IdYeuCau = r.ID_YEUCAU,
                        Manv = r.MANV,
                        LoaiNghi = r.LOAI_NGHI ?? "Nghỉ phép năm",
                        TuNgay = r.TU_NGAY.HasValue ? r.TU_NGAY.Value.ToString("dd/MM/yyyy") : "",
                        DenNgay = r.DEN_NGAY.HasValue ? r.DEN_NGAY.Value.ToString("dd/MM/yyyy") : "",
                        SoNgay = r.SO_NGAY ?? 1,
                        LyDo = r.LYDO,
                        TrangThai = r.TRANGTHAI ?? "PENDING",
                        NgayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        NguoiDuyet = r.NGUOI_DUYET,
                        NgayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        LyDoTuChoi = r.LYDO_TUCHOI
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/leave Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách yêu cầu nghỉ phép." });
            }
        }

        /// <summary>
        /// POST: api/me/leave
        /// Gửi yêu cầu xin nghỉ phép mới (Trạng thái mặc định: PENDING)
        /// Nhân viên không được tự APPROVE (Quy tắc 26)
        /// </summary>
        [HttpPost]
        [Route("leave")]
        public IHttpActionResult CreateLeaveRequest([FromBody] CreateLeaveRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.TuNgay) || string.IsNullOrWhiteSpace(req.DenNgay))
                {
                    return BadRequest("Vui lòng chọn thời gian bắt đầu và kết thúc nghỉ.");
                }

                if (!DateTime.TryParse(req.TuNgay, out var tuNgay) || !DateTime.TryParse(req.DenNgay, out var denNgay))
                {
                    return BadRequest("Định dạng ngày không hợp lệ. Vui lòng nhập định dạng chuẩn YYYY-MM-DD.");
                }

                if (denNgay < tuNgay)
                {
                    return BadRequest("Ngày kết thúc nghỉ không được nhỏ hơn ngày bắt đầu.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    decimal soNgay = req.SoNgay.HasValue && req.SoNgay.Value > 0 ? req.SoNgay.Value : (decimal)(denNgay - tuNgay).TotalDays + 1;

                    string insertSql = @"
                        INSERT INTO HR.TB_YEUCAU_NGHIPHEP 
                        (MANV, LOAIPHEP, TUNGAY, DENNGAY, SONGAY, LYDO, TRANGTHAI, CREATED_DATE)
                        VALUES (:p0, :p1, :p2, :p3, :p4, :p5, 'PENDING', SYSDATE)";

                    db.Database.ExecuteSqlCommand(
                        insertSql,
                        new OracleParameter("p0", nv.MANV),
                        new OracleParameter("p1", req.LoaiNghi ?? "Nghỉ phép năm"),
                        new OracleParameter("p2", tuNgay),
                        new OracleParameter("p3", denNgay),
                        new OracleParameter("p4", soNgay),
                        new OracleParameter("p5", req.LyDo ?? "")
                    );

                    return Ok(new
                    {
                        success = true,
                        message = "Gửi đơn xin nghỉ phép thành công! Đơn của bạn đang chờ phê duyệt."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/leave Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tạo yêu cầu nghỉ phép." });
            }
        }

        /// <summary>
        /// GET: api/me/attendance-corrections
        /// Lấy danh sách yêu cầu điều chỉnh công của nhân viên
        /// </summary>
        [HttpGet]
        [Route("attendance-corrections")]
        public IHttpActionResult GetMyAttendanceCorrections()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var list = db.Database.SqlQuery<AttendanceCorrectionRow>(
                        "SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_CONG, Y.GIO_VAO AS GIO_VAO_MOI, Y.GIO_RA AS GIO_RA_MOI, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, " +
                        "NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI " +
                        "FROM HR.TB_YEUCAU_DIEUCHINHCONG Y " +
                        "LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER " +
                        "WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                        new OracleParameter("p0", nv.MANV)
                    ).ToList();

                    var result = list.Select(r => new MobileAttendanceCorrectionDto
                    {
                        IdYeuCau = r.ID_YEUCAU,
                        Manv = r.MANV,
                        NgayCong = r.NGAY_CONG.HasValue ? r.NGAY_CONG.Value.ToString("dd/MM/yyyy") : "",
                        GioVaoMoi = r.GIO_VAO_MOI ?? "--:--",
                        GioRaMoi = r.GIO_RA_MOI ?? "--:--",
                        LyDo = r.LYDO,
                        TrangThai = r.TRANGTHAI ?? "PENDING",
                        NgayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        NguoiDuyet = r.NGUOI_DUYET,
                        NgayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        LyDoTuChoi = r.LYDO_TUCHOI
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/attendance-corrections Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách điều chỉnh công." });
            }
        }

        /// <summary>
        /// POST: api/me/attendance-corrections
        /// Tạo yêu cầu điều chỉnh chấm công (Quy tắc 27: Không sửa trực tiếp Attendance gốc, gửi request chờ duyệt)
        /// </summary>
        [HttpPost]
        [Route("attendance-corrections")]
        public IHttpActionResult CreateAttendanceCorrection([FromBody] CreateAttendanceCorrectionRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.NgayCong))
                {
                    return BadRequest("Vui lòng chỉ định ngày cần điều chỉnh công.");
                }

                if (!DateTime.TryParse(req.NgayCong, out var ngayCong))
                {
                    return BadRequest("Định dạng ngày công không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    string insertSql = @"
                        INSERT INTO HR.TB_YEUCAU_DIEUCHINHCONG 
                        (MANV, NGAY, LOAIDIEUCHINH, GIO_VAO, GIO_RA, LYDO, TRANGTHAI, CREATED_DATE)
                        VALUES (:p0, :p1, 'Điều chỉnh công', :p2, :p3, :p4, 'PENDING', SYSDATE)";

                    db.Database.ExecuteSqlCommand(
                        insertSql,
                        new OracleParameter("p0", nv.MANV),
                        new OracleParameter("p1", ngayCong),
                        new OracleParameter("p2", req.GioVaoMoi ?? "08:00"),
                        new OracleParameter("p3", req.GioRaMoi ?? "17:00"),
                        new OracleParameter("p4", req.LyDo ?? "")
                    );

                    return Ok(new
                    {
                        success = true,
                        message = "Gửi yêu cầu điều chỉnh chấm công thành công! Vui lòng chờ cấp quản lý phê duyệt."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/attendance-corrections Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi gửi yêu cầu điều chỉnh công." });
            }
        }

        /// <summary>
        /// GET: api/me/overtime
        /// Lấy danh sách yêu cầu đăng ký tăng ca của nhân viên (Quy tắc 28)
        /// </summary>
        [HttpGet]
        [Route("overtime")]
        public IHttpActionResult GetMyOvertimeRequests()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var list = db.Database.SqlQuery<OvertimeRequestRow>(
                        @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 
                                 NVL(L.HESO, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA, 
                                 Y.LYDO AS NOI_DUNG, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, 
                                 NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI 
                          FROM HR.TB_YEUCAU_TANGCA Y 
                          LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER 
                          LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA 
                          WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                        new OracleParameter("p0", nv.MANV)
                    ).ToList();

                    var result = list.Select(r => new MobileOvertimeRequestDto
                    {
                        Id = r.ID_YEUCAU,
                        IdYeuCau = r.ID_YEUCAU,
                        Manv = r.MANV,
                        NgayTangCa = r.NGAY_TANGCA.HasValue ? r.NGAY_TANGCA.Value.ToString("dd/MM/yyyy") : "",
                        OtDate = r.NGAY_TANGCA.HasValue ? r.NGAY_TANGCA.Value.ToString("yyyy-MM-dd") : "",
                        SoGio = r.SO_GIO ?? 0,
                        Hours = r.SO_GIO ?? 0,
                        IdCa = r.IDCA ?? 1,
                        TenCa = r.TEN_CA ?? "Ca ngày",
                        ShiftType = r.TEN_CA ?? "Ca ngày",
                        HeSo = r.HE_SO ?? 1.0m,
                        Coefficient = r.HE_SO ?? 1.0m,
                        NoiDung = r.NOI_DUNG,
                        Reason = r.NOI_DUNG,
                        TrangThai = r.TRANGTHAI ?? "PENDING",
                        Status = r.TRANGTHAI ?? "PENDING",
                        NgayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        CreatedAt = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("yyyy-MM-dd HH:mm") : "",
                        NguoiDuyet = r.NGUOI_DUYET,
                        NgayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        LyDoTuChoi = r.LYDO_TUCHOI,
                        Note = r.LYDO_TUCHOI
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/overtime Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách đăng ký tăng ca." });
            }
        }

        /// <summary>
        /// GET: api/me/overtime/{id}
        /// Xem chi tiết một đề xuất tăng ca của chính nhân viên (Security: Employee không xem được request người khác)
        /// </summary>
        [HttpGet]
        [Route("overtime/{id:decimal}")]
        public IHttpActionResult GetOvertimeRequestDetail(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var r = db.Database.SqlQuery<OvertimeRequestRow>(
                        @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 
                                 NVL(L.HESO, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA, 
                                 Y.LYDO AS NOI_DUNG, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, 
                                 NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI 
                          FROM HR.TB_YEUCAU_TANGCA Y 
                          LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER 
                          LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA 
                          WHERE Y.ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (r == null)
                    {
                        return NotFound();
                    }

                    // Security: Employee chỉ được xem request của chính mình
                    if (r.MANV != nv.MANV)
                    {
                        return Content(HttpStatusCode.Forbidden, new { success = false, message = "Bạn không có quyền xem đề xuất tăng ca của nhân viên khác." });
                    }

                    var dto = new MobileOvertimeRequestDto
                    {
                        Id = r.ID_YEUCAU,
                        IdYeuCau = r.ID_YEUCAU,
                        Manv = r.MANV,
                        NgayTangCa = r.NGAY_TANGCA.HasValue ? r.NGAY_TANGCA.Value.ToString("dd/MM/yyyy") : "",
                        OtDate = r.NGAY_TANGCA.HasValue ? r.NGAY_TANGCA.Value.ToString("yyyy-MM-dd") : "",
                        SoGio = r.SO_GIO ?? 0,
                        Hours = r.SO_GIO ?? 0,
                        IdCa = r.IDCA ?? 1,
                        TenCa = r.TEN_CA ?? "Ca ngày",
                        ShiftType = r.TEN_CA ?? "Ca ngày",
                        HeSo = r.HE_SO ?? 1.0m,
                        Coefficient = r.HE_SO ?? 1.0m,
                        NoiDung = r.NOI_DUNG,
                        Reason = r.NOI_DUNG,
                        TrangThai = r.TRANGTHAI ?? "PENDING",
                        Status = r.TRANGTHAI ?? "PENDING",
                        NgayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        CreatedAt = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("yyyy-MM-dd HH:mm") : "",
                        NguoiDuyet = r.NGUOI_DUYET,
                        NgayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        LyDoTuChoi = r.LYDO_TUCHOI,
                        Note = r.LYDO_TUCHOI
                    };

                    return Ok(new { success = true, data = dto });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/overtime/{id} Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi xem chi tiết đề xuất tăng ca." });
            }
        }

        /// <summary>
        /// POST: api/me/overtime
        /// Gửi yêu cầu đăng ký tăng ca (Tự động lấy MANV từ token xác thực, kiểm tra chống trùng, validate ràng buộc DB)
        /// </summary>
        [HttpPost]
        [Route("overtime")]
        public IHttpActionResult CreateOvertimeRequest([FromBody] CreateOvertimeRequest req)
        {
            try
            {
                if (req == null)
                {
                    return BadRequest("Dữ liệu đề xuất tăng ca không hợp lệ.");
                }

                string dateStr = !string.IsNullOrWhiteSpace(req.NgayTangCa) ? req.NgayTangCa : req.OtDate;
                if (string.IsNullOrWhiteSpace(dateStr))
                {
                    return BadRequest("Vui lòng nhập ngày tăng ca.");
                }

                if (!DateTime.TryParse(dateStr, out var ngayTangCa))
                {
                    return BadRequest("Định dạng ngày tăng ca không hợp lệ.");
                }

                decimal hours = req.SoGio > 0 ? req.SoGio : req.Hours;
                if (hours <= 0)
                {
                    return BadRequest("Số giờ tăng ca phải lớn hơn 0.");
                }
                if (hours > 24)
                {
                    return BadRequest("Số giờ tăng ca không được vượt quá 24 giờ/ngày.");
                }

                string reason = !string.IsNullOrWhiteSpace(req.NoiDung) ? req.NoiDung : req.Reason;
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest("Vui lòng nêu rõ lý do/nội dung tăng ca.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    // 1. Kiểm tra IDCA hợp lệ với bảng TB_LOAICA
                    decimal shiftId = req.IdCa ?? 1;
                    bool shiftExists = db.TB_LOAICA.Any(l => l.IDLOAICA == shiftId);
                    if (!shiftExists)
                    {
                        return BadRequest("Ca làm việc (IDCA) không hợp lệ hoặc không tồn tại trong hệ thống.");
                    }

                    // 2. Chống request trùng lặp (Rule: Một NV không thể có nhiều request PENDING/APPROVED cùng ngày và ca)
                    var hasDuplicate = db.Database.SqlQuery<decimal>(@"
                        SELECT COUNT(*) FROM HR.TB_YEUCAU_TANGCA 
                        WHERE MANV = :p0 
                          AND TRUNC(NGAY) = TRUNC(:p1) 
                          AND NVL(IDCA, 1) = :p2 
                          AND TRANGTHAI IN ('PENDING', 'APPROVED')",
                        new OracleParameter("p0", nv.MANV),
                        new OracleParameter("p1", ngayTangCa),
                        new OracleParameter("p2", shiftId)
                    ).FirstOrDefault() > 0;

                    if (hasDuplicate)
                    {
                        return BadRequest("Bạn đã có đề xuất tăng ca cho ca làm việc ngày này đang chờ duyệt hoặc đã được duyệt.");
                    }

                    string insertSql = @"
                        INSERT INTO HR.TB_YEUCAU_TANGCA 
                        (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE)
                        VALUES (:p0, :p1, :p2, :p3, :p4, 'PENDING', SYSDATE)";

                    db.Database.ExecuteSqlCommand(
                        insertSql,
                        new OracleParameter("p0", nv.MANV),
                        new OracleParameter("p1", ngayTangCa),
                        new OracleParameter("p2", hours),
                        new OracleParameter("p3", shiftId),
                        new OracleParameter("p4", reason.Trim())
                    );

                    return Ok(new
                    {
                        success = true,
                        message = "Gửi đơn đăng ký tăng ca thành công! Đang chờ phê duyệt."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/overtime Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi gửi đơn đăng ký tăng ca: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: api/me/overtime/{id}/cancel
        /// Nhân viên tự hủy đề xuất tăng ca khi còn ở trạng thái PENDING
        /// </summary>
        [HttpPost]
        [Route("overtime/{id:decimal}/cancel")]
        public IHttpActionResult CancelOvertimeRequest(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var reqItem = db.Database.SqlQuery<OvertimeRequestRow>(
                        "SELECT ID AS ID_YEUCAU, MANV, TRANGTHAI FROM HR.TB_YEUCAU_TANGCA WHERE ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (reqItem == null)
                    {
                        return NotFound();
                    }

                    if (reqItem.MANV != nv.MANV)
                    {
                        return Content(HttpStatusCode.Forbidden, new { success = false, message = "Bạn không có quyền hủy đề xuất tăng ca của nhân viên khác." });
                    }

                    if (reqItem.TRANGTHAI != "PENDING")
                    {
                        return BadRequest($"Không thể hủy đề xuất ở trạng thái '{reqItem.TRANGTHAI}'. Chỉ có thể hủy đề xuất đang Chờ duyệt (PENDING).");
                    }

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'CANCELLED', GHICHUDUYET = 'Nhân viên tự hủy đề xuất', NGAYDUYET = SYSDATE WHERE ID = :p0 AND MANV = :p1 AND TRANGTHAI = 'PENDING'",
                        new OracleParameter("p0", id),
                        new OracleParameter("p1", nv.MANV)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Không thể hủy đề xuất tăng ca hoặc đề xuất đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Đã hủy đề xuất tăng ca thành công." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CancelOvertimeRequest Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi hủy đề xuất tăng ca." });
            }
        }

        /// <summary>
        /// GET: api/me/requests
        /// Lấy toàn bộ danh sách yêu cầu (Nghỉ phép, Điều chỉnh công, Tăng ca) của nhân viên gộp chung
        /// </summary>
        [HttpGet]
        [Route("requests")]
        public IHttpActionResult GetMyAllRequests()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var unified = new List<UnifiedRequestDto>();

                    // 1. Nghỉ phép
                    try
                    {
                        var leaves = db.Database.SqlQuery<LeaveRequestRow>(
                            "SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.LOAIPHEP AS LOAI_NGHI, Y.TUNGAY AS TU_NGAY, Y.DENNGAY AS DEN_NGAY, Y.SONGAY AS SO_NGAY, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, " +
                            "NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI " +
                            "FROM HR.TB_YEUCAU_NGHIPHEP Y " +
                            "LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER " +
                            "WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                            new OracleParameter("p0", nv.MANV)
                        ).ToList();

                        foreach (var l in leaves)
                        {
                            string tu = l.TU_NGAY.HasValue ? l.TU_NGAY.Value.ToString("dd/MM/yyyy") : "";
                            string den = l.DEN_NGAY.HasValue ? l.DEN_NGAY.Value.ToString("dd/MM/yyyy") : "";
                            unified.Add(new UnifiedRequestDto
                            {
                                Id = l.ID_YEUCAU,
                                RequestType = "LEAVE",
                                TypeLabel = "Xin nghỉ phép",
                                Title = l.LOAI_NGHI ?? "Nghỉ phép năm",
                                Subtitle = $"{l.SO_NGAY ?? 1} ngày",
                                DateRange = tu == den ? tu : $"{tu} - {den}",
                                Status = l.TRANGTHAI ?? "PENDING",
                                CreatedAt = l.NGAY_TAO.HasValue ? l.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                Reason = l.LYDO,
                                RejectionReason = l.LYDO_TUCHOI
                            });
                        }
                    }
                    catch { }

                    // 2. Điều chỉnh công
                    try
                    {
                        var atts = db.Database.SqlQuery<AttendanceCorrectionRow>(
                            "SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_CONG, Y.GIO_VAO AS GIO_VAO_MOI, Y.GIO_RA AS GIO_RA_MOI, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, " +
                            "NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI " +
                            "FROM HR.TB_YEUCAU_DIEUCHINHCONG Y " +
                            "LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER " +
                            "WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                            new OracleParameter("p0", nv.MANV)
                        ).ToList();

                        foreach (var a in atts)
                        {
                            string dt = a.NGAY_CONG.HasValue ? a.NGAY_CONG.Value.ToString("dd/MM/yyyy") : "";
                            unified.Add(new UnifiedRequestDto
                            {
                                Id = a.ID_YEUCAU,
                                RequestType = "ATTENDANCE",
                                TypeLabel = "Điều chỉnh công",
                                Title = $"Điều chỉnh công ngày {dt}",
                                Subtitle = $"Vào: {a.GIO_VAO_MOI ?? "--:--"} | Ra: {a.GIO_RA_MOI ?? "--:--"}",
                                DateRange = dt,
                                Status = a.TRANGTHAI ?? "PENDING",
                                CreatedAt = a.NGAY_TAO.HasValue ? a.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                Reason = a.LYDO,
                                RejectionReason = a.LYDO_TUCHOI
                            });
                        }
                    }
                    catch { }

                    // 3. Tăng ca
                    try
                    {
                        var ots = db.Database.SqlQuery<OvertimeRequestRow>(
                            "SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 1.5 AS HE_SO, Y.LYDO AS NOI_DUNG, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO, " +
                            "NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI " +
                            "FROM HR.TB_YEUCAU_TANGCA Y " +
                            "LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER " +
                            "WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                            new OracleParameter("p0", nv.MANV)
                        ).ToList();

                        foreach (var o in ots)
                        {
                            string dt = o.NGAY_TANGCA.HasValue ? o.NGAY_TANGCA.Value.ToString("dd/MM/yyyy") : "";
                            unified.Add(new UnifiedRequestDto
                            {
                                Id = o.ID_YEUCAU,
                                RequestType = "OVERTIME",
                                TypeLabel = "Đăng ký tăng ca",
                                Title = $"Tăng ca {o.SO_GIO ?? 0} giờ (Hệ số {o.HE_SO ?? 1.5m}x)",
                                Subtitle = o.NOI_DUNG ?? "Tăng ca theo kế hoạch",
                                DateRange = dt,
                                Status = o.TRANGTHAI ?? "PENDING",
                                CreatedAt = o.NGAY_TAO.HasValue ? o.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                Reason = o.NOI_DUNG,
                                RejectionReason = o.LYDO_TUCHOI
                            });
                        }
                    }
                    catch { }

                    var sorted = unified.OrderByDescending(u => u.CreatedAt).ToList();
                    return Ok(new { success = true, data = sorted });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/requests Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách yêu cầu." });
            }
        }
    }

    public class MappingCheckRow
    {
        public decimal? EMPLOYEE_ID { get; set; }
        public decimal? IS_MOBILE_ENABLED { get; set; }
    }

    public class LeaveRequestRow
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string LOAI_NGHI { get; set; }
        public DateTime? TU_NGAY { get; set; }
        public DateTime? DEN_NGAY { get; set; }
        public decimal? SO_NGAY { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class AttendanceCorrectionRow
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public DateTime? NGAY_CONG { get; set; }
        public string GIO_VAO_MOI { get; set; }
        public string GIO_RA_MOI { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class OvertimeRequestRow
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public DateTime? NGAY_TANGCA { get; set; }
        public decimal? SO_GIO { get; set; }
        public decimal? HE_SO { get; set; }
        public decimal? IDCA { get; set; }
        public string TEN_CA { get; set; }
        public string NOI_DUNG { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class LeaveTransactionRow
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public DateTime NGAY { get; set; }
        public string LOAI { get; set; }
        public decimal GIAY_PHEP { get; set; }
        public decimal? IDDON { get; set; }
        public string LY_DO { get; set; }
    }
}

