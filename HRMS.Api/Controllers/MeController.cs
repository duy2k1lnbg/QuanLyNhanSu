using DA;
using HRMS_API.Filters;
using HRMS_API.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (pub.StatusQueryFailed) throw new InvalidOperationException(pub.Message);
            blStatus = pub.Status;
            trangThaiChiTra = pub.PaymentStatusText;
            return pub.IsPublished;
        }

        private bool TryGetAuthenticatedEmployee(
            MyEntities db, 
            out TB_SYS_USER user, 
            out TB_NHANVIEN nv, 
            out IHttpActionResult errorResult,
            string requiredFunctionCode = null,
            Bu.CLASS_SECURITY.ChannelAction requiredAction = Bu.CLASS_SECURITY.ChannelAction.View)
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

            // Tài khoản Quản trị viên tối cao (ADMIN) không sử dụng cổng nhân viên tự phục vụ
            if (user.USERNAME != null && user.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                errorResult = Content(HttpStatusCode.Forbidden, new { success = false, message = "Tài khoản Quản trị viên tối cao (ADMIN) không áp dụng cổng dịch vụ cá nhân nhân viên." });
                return false;
            }

            // Phân giải hồ sơ nhân viên và quyền Mobile từ TB_USER_EMPLOYEE_MAPPING (Nguồn chính xác duy nhất)
            MappingCheckRow mapRow = null;
            try
            {
                mapRow = db.Database.SqlQuery<MappingCheckRow>(
                    "SELECT EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                    new OracleParameter("p0", user.IDUSER)
                ).FirstOrDefault();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[MeController] Error reading TB_USER_EMPLOYEE_MAPPING: " + ex);
                errorResult = Content(HttpStatusCode.ServiceUnavailable, new 
                { 
                    success = false, 
                    code = Bu.CLASS_SECURITY.PlatformErrorCodes.AuthServiceUnavailable,
                    message = Bu.CLASS_SECURITY.PlatformErrorCodes.GetFriendlyMessage(Bu.CLASS_SECURITY.PlatformErrorCodes.AuthServiceUnavailable) 
                });
                return false;
            }

            if (mapRow == null || !mapRow.EMPLOYEE_ID.HasValue || mapRow.EMPLOYEE_ID.Value <= 0)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new 
                { 
                    success = false, 
                    code = Bu.CLASS_SECURITY.PlatformErrorCodes.EmployeeLinkRequired,
                    message = Bu.CLASS_SECURITY.PlatformErrorCodes.GetFriendlyMessage(Bu.CLASS_SECURITY.PlatformErrorCodes.EmployeeLinkRequired) 
                });
                return false;
            }

            // Kiểm tra xung đột MANV giữa TB_SYS_USER và TB_USER_EMPLOYEE_MAPPING
            if (user.MANV.HasValue && user.MANV.Value > 0 && user.MANV.Value != mapRow.EMPLOYEE_ID.Value)
            {
                errorResult = Content(HttpStatusCode.Conflict, new 
                { 
                    success = false, 
                    code = Bu.CLASS_SECURITY.PlatformErrorCodes.EmployeeLinkConflict,
                    message = Bu.CLASS_SECURITY.PlatformErrorCodes.GetFriendlyMessage(Bu.CLASS_SECURITY.PlatformErrorCodes.EmployeeLinkConflict) 
                });
                return false;
            }

            decimal manv = mapRow.EMPLOYEE_ID.Value;
            user.MANV = manv;

            int isMobileEnabled = (int)(mapRow.IS_MOBILE_ENABLED ?? 1);
            string clientType = (jwtUser?.ClientType ?? "ALL").Trim().ToUpperInvariant();
            if (clientType == Bu.CLASS_SECURITY.AppChannels.Mobile && isMobileEnabled == 0)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new 
                { 
                    success = false, 
                    code = Bu.CLASS_SECURITY.PlatformErrorCodes.MobileDisabled,
                    message = Bu.CLASS_SECURITY.PlatformErrorCodes.GetFriendlyMessage(Bu.CLASS_SECURITY.PlatformErrorCodes.MobileDisabled) 
                });
                return false;
            }

            nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == manv && n.DELETED_BY == null);
            if (nv == null)
            {
                errorResult = Content(HttpStatusCode.NotFound, new { success = false, message = "Không tìm thấy hồ sơ nhân sự tương ứng với tài khoản này." });
                return false;
            }

            if (nv.DATHOIVIEC.HasValue && nv.DATHOIVIEC.Value == 1)
            {
                errorResult = Content(HttpStatusCode.Forbidden, new { success = false, message = "Hồ sơ nhân sự đã thôi việc. Quyền tự phục vụ cá nhân đã bị ngừng kích hoạt." });
                return false;
            }

            // Kiểm tra quyền con cụ thể qua PlatformAccessGuard (Zero-Trust theo đặc tả mục 13-14)
            if (!string.IsNullOrWhiteSpace(requiredFunctionCode))
            {
                string normChannel = Bu.CLASS_SECURITY.AppChannels.Normalize(clientType) ?? Bu.CLASS_SECURITY.AppChannels.Mobile;
                bool isActionAllowed = Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                    db, user.IDUSER, normChannel, requiredFunctionCode, requiredAction, user.USERNAME
                );

                if (!isActionAllowed)
                {
                    errorResult = Content(HttpStatusCode.Forbidden, new
                    {
                        success = false,
                        code = "PERMISSION_DENIED",
                        message = $"Tài khoản không có quyền [{requiredAction}] đối với chức năng [{requiredFunctionCode}] trên kênh [{normChannel}]."
                    });
                    return false;
                }
            }

            return true;
        }

        private HashSet<string> SelfServiceViewRights(MyEntities db, TB_SYS_USER user)
        {
            var jwt=JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            var resolver=new Bu.CLASS_SECURITY.ChannelPermissionResolver();
            var tree=resolver.ResolveChannelTree(db,user.IDUSER,jwt.ClientType,user.USERNAME);
            resolver.ProjectEffectiveRights(tree,out var detailed,out var views);
            return new HashSet<string>(views,StringComparer.OrdinalIgnoreCase);
        }

        private static DateTime RequestDate(string value)
        {
            DateTime result;
            return DateTime.TryParseExact(value,new[] { "dd/MM/yyyy HH:mm", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy", "yyyy-MM-dd" },CultureInfo.InvariantCulture,DateTimeStyles.None,out result) ? result:DateTime.MinValue;
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

                    var resolver = new Bu.CLASS_SECURITY.ChannelPermissionResolver();
                    var channelTree = resolver.ResolveChannelTree(db, user.IDUSER, jwtUser.ClientType, user.USERNAME);
                    resolver.ProjectEffectiveRights(channelTree, out var detailedRights, out var viewableRights);
                    var result = new MobileMeDto
                    {
                        IdUser = (int)user.IDUSER,
                        Username = user.USERNAME,
                        FullName = nv.HOTEN ?? user.FULLNAME ?? user.USERNAME,
                        Manv = user.MANV,
                        EmployeeCode = empCode,
                        ClientType = jwtUser.ClientType,
                        MaCty = user.MACTY,
                        MaDvi = user.MADVI,
                        IsAdmin = jwtUser != null && jwtUser.IsAdmin,
                        Rights = viewableRights,
                        DetailedRights = detailedRights
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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

                    var views = SelfServiceViewRights(db,user);
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
                    if (latestKc != null && views.Contains("MOBILE_ATTENDANCE_VIEW"))
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
                    var allBls = views.Contains("MOBILE_PAYROLL_VIEW") ? db.TB_BANGLUONG
                        .Where(b => b.MANV == manv)
                        .OrderByDescending(b => b.MAKYCONG)
                        .ToList() : new List<TB_BANGLUONG>();

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
                    if (views.Contains("MOBILE_CONTRACT_VIEW") && latestContract != null && latestContract.NGAYKETTHUC.HasValue)
                    {
                        var daysLeft = (latestContract.NGAYKETTHUC.Value - now).TotalDays;
                        if (daysLeft >= 0 && daysLeft <= 30)
                        {
                            dashboard.HasExpiringContract = true;
                            dashboard.ExpiringContractInfo = $"Hợp đồng số {latestContract.SOHD} sẽ hết hạn sau {Math.Ceiling(daysLeft)} ngày (Ngày hết hạn: {latestContract.NGAYKETTHUC.Value:dd/MM/yyyy})";
                        }
                    }

                    // 5. Thông báo gần đây và số chưa đọc thật từ NotificationService
                    if (views.Contains("MOBILE_NOTIFICATION_VIEW"))
                    {
                    string pbStr = nv.IDPB?.ToString();
                    var notifList = Bu.CLASS_SYSTEM.NotificationService.GetNotificationsForUser((long)user.IDUSER, (long)nv.MANV, nv.IDCTY?.ToString(), pbStr, 3);
                    foreach (var n in notifList)
                    {
                        dashboard.RecentNotifications.Add(new MobileNotificationDto
                        {
                            Id = n.Id,
                            Tieude = n.Title,
                            Noidung = n.Content != null && n.Content.Length > 120 ? n.Content.Substring(0, 120) + "..." : n.Content,
                            Nguoidang = n.Sender,
                            Ngaydang = n.CreatedAt,
                            Loaitb = n.Type,
                            IsPinned = n.IsPinned,
                            FileDinhkem = n.AttachedFile,
                            IsUnread = n.IsUnread,
                            TargetType = n.TargetType,
                            EventType = n.EventType,
                            EntityId = n.EntityId
                        });
                    }

                    dashboard.UnreadNotificationCount = Bu.CLASS_SYSTEM.NotificationService.GetUnreadCount((long)user.IDUSER, (long)nv.MANV, nv.IDCTY?.ToString(), pbStr);

                    }
                    return Ok(new { success = true, data = dashboard });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_PROFILE_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
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

                    string empEmail = null;
                    try
                    {
                        empEmail = db.Database.SqlQuery<string>(
                            "SELECT EMAIL FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND ROWNUM = 1",
                            new OracleParameter("p0", nv.MANV)
                        ).FirstOrDefault();
                    }
                    catch { }

                    if (string.IsNullOrWhiteSpace(empEmail) && !string.IsNullOrWhiteSpace(user.USERNAME) && user.USERNAME.Contains("@"))
                    {
                        empEmail = user.USERNAME;
                    }

                    var profile = new MobileProfileDto
                    {
                        Manv = nv.MANV,
                        EmployeeCode = empCode,
                        Hoten = nv.HOTEN,
                        Gioitinh = nv.IDGT == 1 ? "Nam" : "Nữ",
                        Ngaysinh = nv.NGAYSINH.HasValue ? nv.NGAYSINH.Value.ToString("yyyy-MM-dd") : null,
                        Dienthoai = !string.IsNullOrWhiteSpace(nv.DIENTHOAI) ? nv.DIENTHOAI : null,
                        Cccd = !string.IsNullOrWhiteSpace(nv.CCCD) ? nv.CCCD : null,
                        Diachi = !string.IsNullOrWhiteSpace(nv.DIACHI) ? nv.DIACHI : null,
                        TenPhongBan = pb?.TENPB ?? "Chưa cập nhật",
                        TenBoPhan = bp?.TENBP ?? "Chưa cập nhật",
                        TenChucVu = cv?.TENCV ?? "Chưa cập nhật",
                        TenTrinhDo = td?.TENTD ?? "Chưa cập nhật",
                        NgayVaoLam = joinDate.HasValue ? joinDate.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật",
                        Email = empEmail,
                        AvatarBase64 = nv.HINHANH != null && nv.HINHANH.Length > 0 ? Convert.ToBase64String(nv.HINHANH) : null,
                        TrangThaiLaoDong = (nv.DATHOIVIEC ?? 0) == 1 ? "Đã thôi việc" : "Đang làm việc"
                    };

                    return Ok(new { success = true, data = profile });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/profile Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải thông tin hồ sơ." });
            }
        }

        /// <summary>
        /// <summary>
        /// GET: api/me/profile/quota
        /// Lấy số lần sửa trực tiếp và trạng thái quota cho từng nhóm thông tin
        /// </summary>
        [HttpGet]
        [Route("profile/quota")]
        public IHttpActionResult GetProfileQuotas()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_PROFILE_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    var svc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    var qPersonal = svc.GetQuota((long)nv.MANV, "PERSONAL_INFO");
                    var qInsurance = svc.GetQuota((long)nv.MANV, "INSURANCE_INFO");
                    var qDependents = svc.GetQuota((long)nv.MANV, "DEPENDENTS");
                    var qAvatar = svc.GetQuota((long)nv.MANV, "AVATAR");

                    var result = new MobileAllQuotasDto
                    {
                        PersonalInfo = new MobileProfileQuotaDto
                        {
                            GroupCode = "PERSONAL_INFO",
                            DirectEditCount = qPersonal.DirectEditCount,
                            IsFirstEditAvailable = qPersonal.IsFirstEditAvailable,
                            FirstEditDate = qPersonal.FirstEditDate,
                            LastEditDate = qPersonal.LastEditDate
                        },
                        InsuranceInfo = new MobileProfileQuotaDto
                        {
                            GroupCode = "INSURANCE_INFO",
                            DirectEditCount = qInsurance.DirectEditCount,
                            IsFirstEditAvailable = qInsurance.IsFirstEditAvailable,
                            FirstEditDate = qInsurance.FirstEditDate,
                            LastEditDate = qInsurance.LastEditDate
                        },
                        Dependents = new MobileProfileQuotaDto
                        {
                            GroupCode = "DEPENDENTS",
                            DirectEditCount = qDependents.DirectEditCount,
                            IsFirstEditAvailable = qDependents.IsFirstEditAvailable,
                            FirstEditDate = qDependents.FirstEditDate,
                            LastEditDate = qDependents.LastEditDate
                        },
                        Avatar = new MobileProfileQuotaDto
                        {
                            GroupCode = "AVATAR",
                            DirectEditCount = qAvatar.DirectEditCount,
                            IsFirstEditAvailable = qAvatar.IsFirstEditAvailable,
                            FirstEditDate = qAvatar.FirstEditDate,
                            LastEditDate = qAvatar.LastEditDate
                        }
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/profile/quota Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải thông tin hạn mức chỉnh sửa." });
            }
        }

        /// <summary>
        /// PUT: api/me/profile
        /// Cập nhật thông tin cá nhân tự phục vụ: Lần đầu lưu trực tiếp, lần sau tạo đơn PENDING
        /// </summary>
        [HttpPut]
        [Route("profile")]
        public IHttpActionResult UpdateProfile([FromBody] UpdateProfileRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (req == null) return BadRequest("Dữ liệu cập nhật không hợp lệ.");

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_PROFILE_EDIT", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return error;
                    }

                    var svc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    var payload = new Bu.CLASS_NHANSU.PersonalInfoPayload
                    {
                        Hoten = req.Hoten,
                        Gioitinh = req.Gioitinh,
                        Ngaysinh = req.Ngaysinh,
                        Cccd = req.Cccd,
                        Dienthoai = req.Dienthoai,
                        Email = req.Email,
                        Diachi = req.Diachi
                    };

                    var result = svc.SubmitPersonalInfo((long)nv.MANV, payload, "Nhân viên cập nhật thông tin cá nhân", (long)user.IDUSER);

                    // Xử lý avatar nếu có gửi kèm
                    if (!string.IsNullOrWhiteSpace(req.AvatarBase64))
                    {
                        try
                        {
                            string base64Data = req.AvatarBase64;
                            if (base64Data.Contains(","))
                            {
                                base64Data = base64Data.Split(',')[1];
                            }
                            byte[] imgBytes = Convert.FromBase64String(base64Data);
                            svc.SubmitAvatar((long)nv.MANV, imgBytes, "Cập nhật ảnh đại diện", (long)user.IDUSER);
                        }
                        catch (Exception exImg) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(exImg)) { throw; }
                        catch (Exception exImg)
                        {
                            System.Diagnostics.Trace.TraceWarning("[UpdateProfile Avatar Warning]: " + exImg.Message);
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        isDirectEdit = result.IsDirectEdit,
                        requestId = result.RequestId,
                        message = result.Message
                    });
                }
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Content(HttpStatusCode.Conflict,new { success=false, message=ex.Message }); }
            catch (KeyNotFoundException ex) { return Content(HttpStatusCode.NotFound,new { success=false, message=ex.Message }); }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[PUT /api/me/profile Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Không thể lưu yêu cầu. Vui lòng thử lại hoặc liên hệ quản trị viên." });
            }
        }

        /// <summary>
        /// POST: api/me/profile/avatar
        /// Cập nhật ảnh đại diện: Lần đầu lưu trực tiếp, lần sau tạo đơn PENDING kèm BLOB
        /// </summary>
        [HttpPost]
        [Route("profile/avatar")]
        public IHttpActionResult UpdateAvatar([FromBody] UpdateProfileRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (req == null || string.IsNullOrWhiteSpace(req.AvatarBase64))
                {
                    return BadRequest("Vui lòng cung cấp dữ liệu ảnh đại diện.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_PROFILE_EDIT", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return error;
                    }

                    string base64Data = req.AvatarBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    byte[] imgBytes = Convert.FromBase64String(base64Data);

                    var svc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    var result = svc.SubmitAvatar((long)nv.MANV, imgBytes, "Đề xuất đổi ảnh đại diện", (long)user.IDUSER);

                    return Ok(new
                    {
                        success = true,
                        isDirectEdit = result.IsDirectEdit,
                        requestId = result.RequestId,
                        message = result.Message
                    });
                }
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Content(HttpStatusCode.Conflict,new { success=false, message=ex.Message }); }
            catch (KeyNotFoundException ex) { return Content(HttpStatusCode.NotFound,new { success=false, message=ex.Message }); }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/profile/avatar Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Không thể lưu yêu cầu. Vui lòng thử lại hoặc liên hệ quản trị viên." });
            }
        }

        /// <summary>
        /// GET: api/me/profile/avatar/proposed
        /// Xem ảnh đại diện đề xuất đang chờ duyệt (nếu có)
        /// </summary>
        [HttpGet]
        [Route("profile/avatar/proposed")]
        public IHttpActionResult GetProposedAvatar()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_PROFILE_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    var svc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    var reqs = svc.GetRequests("AVATAR", "PENDING", null, nv.MANV, 1, 1, out int _);
                    var latest = reqs.FirstOrDefault();
                    if (latest != null)
                    {
                        var bytes = svc.GetProposedAvatar(latest.Id);
                        if (bytes != null && bytes.Length > 0)
                        {
                            return Ok(new
                            {
                                success = true,
                                hasProposed = true,
                                requestId = latest.Id,
                                avatarBase64 = Convert.ToBase64String(bytes),
                                createdAt = latest.CreatedAt
                            });
                        }
                    }

                    return Ok(new { success = true, hasProposed = false });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/profile/avatar/proposed Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi lấy ảnh đề xuất." });
            }
        }

        /// <summary>
        /// GET: api/me/dependents
        /// Lấy danh sách người phụ thuộc hiện tại của nhân viên
        /// </summary>
        [HttpGet]
        [Route("dependents")]
        public IHttpActionResult GetMyDependents()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_DEPENDENT", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    var deps = db.Database.SqlQuery<MobileDependentItemDto>(@"
                        SELECT ID, MANV, HO_TEN AS HoTen, MOI_QUAN_HE AS MoiQuanHe, CCCD, MA_SO_THUE_NPT AS MaSoThueNpt,
                               TO_CHAR(NGAY_SINH, 'YYYY-MM-DD') AS NgaySinh, LOAI_GIAY_TO AS LoaiGiayTo, SO_GIAY_TO AS SoGiayTo,
                               THANG_BAT_DAU_GIAM_TRU AS ThangBatDau, THANG_KET_THUC_GIAM_TRU AS ThangKetThuc, TRANG_THAI AS TrangThai
                        FROM HR.TB_NGUOI_PHU_THUOC
                        WHERE MANV = :p_manv
                        ORDER BY ID",
                        new OracleParameter("p_manv", nv.MANV)
                    ).ToList();

                    return Ok(new { success = true, data = deps });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/dependents Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách người phụ thuộc." });
            }
        }

        /// <summary>
        /// POST: api/me/dependents/propose
        /// Đề xuất thay đổi người phụ thuộc (thêm/sửa/kết thúc)
        /// </summary>
        [HttpPost]
        [Route("dependents/propose")]
        public IHttpActionResult ProposeDependents([FromBody] ProposeDependentsRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (req == null || req.Dependents == null || req.Dependents.Count == 0)
                {
                    return BadRequest("Vui lòng cung cấp danh sách người phụ thuộc.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_DEPENDENT", Bu.CLASS_SECURITY.ChannelAction.Add))
                    {
                        return error;
                    }

                    var svc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    string reason = req.Reason ?? req.LyDo ?? "Nhân viên đề xuất thay đổi người phụ thuộc";
                    var result = svc.SubmitDependentsProposal((long)nv.MANV, req.Dependents, reason, (long)user.IDUSER);

                    return Ok(new
                    {
                        success = true,
                        isDirectEdit = result.IsDirectEdit,
                        requestId = result.RequestId,
                        message = result.Message
                    });
                }
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Content(HttpStatusCode.Conflict,new { success=false, message=ex.Message }); }
            catch (KeyNotFoundException ex) { return Content(HttpStatusCode.NotFound,new { success=false, message=ex.Message }); }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/dependents/propose Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Không thể lưu yêu cầu. Vui lòng thử lại hoặc liên hệ quản trị viên." });
            }
        }

        /// <summary>
        /// POST: api/me/insurance/propose
        /// Đề xuất điều chỉnh thông tin bảo hiểm
        /// </summary>
        [HttpPost]
        [Route("insurance/propose")]
        public IHttpActionResult ProposeInsurance([FromBody] ProposeInsuranceRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (req == null) return BadRequest("Dữ liệu đề xuất bảo hiểm không hợp lệ.");

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_INSURANCE", Bu.CLASS_SECURITY.ChannelAction.Add))
                    {
                        return error;
                    }

                    var svc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    var payload = new Bu.CLASS_NHANSU.InsuranceProposalPayload
                    {
                        VungLuong = req.VungLuong,
                        ThamGiaBhxh = req.ThamGiaBhxh,
                        ThamGiaBhyt = req.ThamGiaBhyt,
                        ThamGiaBhtn = req.ThamGiaBhtn,
                        ThamGiaTnldBnn = req.ThamGiaTnldBnn,
                        LuongDongBhxhRieng = req.LuongDongBhxhRieng,
                        NgayBatDau = req.NgayBatDau,
                        GhiChu = req.Reason ?? req.LyDo
                    };

                    string reason = req.Reason ?? req.LyDo ?? "Nhân viên đề xuất thông tin bảo hiểm";
                    var result = svc.SubmitInsuranceProposal((long)nv.MANV, payload, reason, (long)user.IDUSER);

                    return Ok(new
                    {
                        success = true,
                        isDirectEdit = result.IsDirectEdit,
                        requestId = result.RequestId,
                        message = result.Message
                    });
                }
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Content(HttpStatusCode.Conflict,new { success=false, message=ex.Message }); }
            catch (KeyNotFoundException ex) { return Content(HttpStatusCode.NotFound,new { success=false, message=ex.Message }); }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/insurance/propose Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Không thể lưu yêu cầu. Vui lòng thử lại hoặc liên hệ quản trị viên." });
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

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_ATTENDANCE_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_PAYROLL_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
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
                            return Ok(new { success = true, data = (MobilePayrollDto)null, availability = candidates.Count>0 ? "NOT_PUBLISHED" : "NOT_CREATED", message = "Bảng lương kỳ này chưa được công bố hoặc chưa phát sinh dữ liệu." });
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
                            return Ok(new { success = true, data = (MobilePayrollDto)null, availability = allBls.Count>0 ? "NOT_PUBLISHED" : "NOT_CREATED", message = "Chưa có bảng lương nào được công bố cho bạn." });
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_CONTRACT_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_INSURANCE_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    decimal manv = nv.MANV;
                    var bh = db.TB_BAOHIEM.FirstOrDefault(b => b.MANV == manv);

                    var result = new MobileInsuranceDto
                    {
                        Idbh = bh?.IDBH ?? 0,
                        Sobh = bh?.SOBH,
                        Ngaycap = bh?.NGAYCAP?.ToString("dd/MM/yyyy"),
                        Noicap = bh?.NOICAP,
                        Noikhambenh = bh?.NOIKHAMBENH,
                        LuongBhxh = bh?.LUONG_BHXH
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
                    catch (Exception mEx) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(mEx)) { throw; }
                    catch (Exception mEx)
                    {
                        System.Diagnostics.Trace.TraceWarning("[GetInsurance Movements Warning]: " + mEx.Message);
                    }

                    try
                    {
                        var parts = db.Database.SqlQuery<InsuranceParticipationReadRow>(
                            @"SELECT ID,VUNG_LUONG,THAM_GIA_BHXH,THAM_GIA_BHYT,THAM_GIA_BHTN,
                                     THAM_GIA_TNLD_BNN,HUONG_TY_LE_TNLD_UU_DAI,LUONG_DONG_BHXH_RIENG,
                                     NGAY_BAT_DAU,NGAY_KET_THUC,TRANG_THAI
                              FROM HR.TB_NHANVIEN_BAOHIEM_THAM_GIA WHERE MANV=:p0
                              ORDER BY NGAY_BAT_DAU DESC,ID DESC",new OracleParameter("p0",manv)).ToList();

                        result.Participations = parts.Select(p => new MobileInsuranceParticipationDto
                        {
                            Id = p.ID,
                            VungLuong = Convert.ToInt16(p.VUNG_LUONG),
                            ThamGiaBhxh = p.THAM_GIA_BHXH == 1,
                            ThamGiaBhyt = p.THAM_GIA_BHYT == 1,
                            ThamGiaBhtn = p.THAM_GIA_BHTN == 1,
                            ThamGiaTnldBnn = p.THAM_GIA_TNLD_BNN == 1,
                            HuongTyLeTnldUuDai = p.HUONG_TY_LE_TNLD_UU_DAI == 1,
                            LuongDongBhxhRieng = p.LUONG_DONG_BHXH_RIENG,
                            NgayBatDau = p.NGAY_BAT_DAU.ToString("dd/MM/yyyy"),
                            NgayKetThuc = p.NGAY_KET_THUC.HasValue ? p.NGAY_KET_THUC.Value.ToString("dd/MM/yyyy") : null,
                            TrangThai = p.TRANG_THAI
                        }).ToList();
                    }
                    catch (Exception pEx) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(pEx)) { throw; }
                    catch (Exception pEx)
                    {
                        throw new InvalidOperationException("Không thể tải hồ sơ tham gia bảo hiểm.",pEx);
                    }

                    try
                    {
                        var union = db.Database.SqlQuery<UnionParticipationReadRow>(
                            @"SELECT ID,LA_DOAN_VIEN,NGAY_GIA_NHAP,NGAY_KET_THUC,TRANG_THAI
                              FROM HR.TB_NHANVIEN_CONG_DOAN_THAM_GIA
                              WHERE MANV=:p0 AND TRANG_THAI=N'ACTIVE' AND NGAY_GIA_NHAP<=TRUNC(SYSDATE)
                                AND (NGAY_KET_THUC IS NULL OR NGAY_KET_THUC>=TRUNC(SYSDATE))
                              ORDER BY NGAY_GIA_NHAP DESC,ID DESC",new OracleParameter("p0",manv)).FirstOrDefault();

                        if (union != null)
                        {
                            result.UnionParticipation = new MobileUnionParticipationDto
                            {
                                Id = union.ID,
                                LaDoanVien = union.LA_DOAN_VIEN == 1,
                                NgayGiaNhap = union.NGAY_GIA_NHAP.ToString("dd/MM/yyyy"),
                                NgayKetThuc = union.NGAY_KET_THUC.HasValue ? union.NGAY_KET_THUC.Value.ToString("dd/MM/yyyy") : null,
                                TrangThai = union.TRANG_THAI
                            };
                        }
                    }
                    catch (Exception uEx) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(uEx)) { throw; }
                    catch (Exception uEx)
                    {
                        System.Diagnostics.Trace.TraceWarning("[GetInsurance Union Warning]: " + uEx.Message);
                    }

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/insurance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải thông tin bảo hiểm." });
            }
        }

        /// <summary>
        /// GET: api/me/notifications
        /// Lấy danh sách thông báo nội bộ kèm trạng thái đã đọc/chưa đọc thật theo User ID
        /// </summary>
        [HttpGet]
        [Route("notifications")]
        public IHttpActionResult GetNotifications(int limit = 50, int offset = 0, bool unreadOnly = false)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_NOTIFICATION_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    string pbStr = nv.IDPB?.ToString();
                    var list = Bu.CLASS_SYSTEM.NotificationService.GetNotificationsForUser((long)user.IDUSER, (long)nv.MANV, nv.IDCTY?.ToString(), pbStr, limit, offset, unreadOnly);
                    var result = list.Select(n => new MobileNotificationDto
                    {
                        Id = n.Id,
                        Tieude = n.Title,
                        Noidung = n.Content,
                        Nguoidang = n.Sender,
                        Ngaydang = n.CreatedAt,
                        Loaitb = n.Type,
                        IsPinned = n.IsPinned,
                        FileDinhkem = n.AttachedFile,
                        IsUnread = n.IsUnread,
                        TargetType = n.TargetType,
                        EventType = n.EventType,
                        EntityId = n.EntityId
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/notifications Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải danh sách thông báo." });
            }
        }

        /// <summary>
        /// GET: api/me/notifications/{id}
        /// Xem chi tiết một thông báo nội bộ và tự động đánh dấu đã đọc (kiểm tra chặt chẽ quyền truy cập)
        /// </summary>
        [HttpGet]
        [Route("notifications/unread-count")]
        public IHttpActionResult GetUnreadNotificationCount()
        {
            using (var db=new MyEntities())
            {
                if (!TryGetAuthenticatedEmployee(db,out var user,out var nv,out var error,"MOBILE_NOTIFICATION_VIEW",Bu.CLASS_SECURITY.ChannelAction.View)) return error;
                int count=Bu.CLASS_SYSTEM.NotificationService.GetUnreadCount((long)user.IDUSER,(long)nv.MANV,nv.IDCTY?.ToString(),nv.IDPB?.ToString());
                return Ok(new {success=true,data=new {count}});
            }
        }

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

                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_NOTIFICATION_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    string pbStr = nv.IDPB?.ToString();
                    var n = Bu.CLASS_SYSTEM.NotificationService.GetNotificationDetailForUser(id, (long)user.IDUSER, (long)nv.MANV, nv.IDCTY?.ToString(), pbStr);
                    if (n == null)
                    {
                        return NotFound();
                    }

                    var result = new MobileNotificationDto
                    {
                        Id = n.Id,
                        Tieude = n.Title,
                        Noidung = n.Content,
                        Nguoidang = n.Sender,
                        Ngaydang = n.CreatedAt,
                        Loaitb = n.Type,
                        IsPinned = n.IsPinned,
                        FileDinhkem = n.AttachedFile,
                        IsUnread = false,
                        TargetType = n.TargetType,
                        EventType = n.EventType,
                        EntityId = n.EntityId
                    };

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/notifications/{id} Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Hệ thống đang gặp sự cố khi tải chi tiết thông báo." });
            }
        }

        /// <summary>
        /// POST: api/me/notifications/{id}/read
        /// Đánh dấu một thông báo là đã đọc (kiểm tra quyền truy cập chống ID guessing)
        /// </summary>
        [HttpPost]
        [Route("notifications/{id:int}/read")]
        public IHttpActionResult MarkNotificationAsRead(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_NOTIFICATION_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    string pbStr = nv.IDPB?.ToString();
                    bool success = Bu.CLASS_SYSTEM.NotificationService.MarkAsRead(id, (long)user.IDUSER, (long)nv.MANV, nv.IDCTY?.ToString(), pbStr);
                    if (!success)
                    {
                        return Content(HttpStatusCode.NotFound, new { success = false, message = "Thông báo không tồn tại hoặc không thuộc quyền truy cập của bạn." });
                    }

                    return Ok(new { success = true, message = "Đã đánh dấu thông báo là đã đọc." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/notifications/{id}/read Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi cập nhật trạng thái thông báo." });
            }
        }

        /// <summary>
        /// POST: api/me/notifications/read-all
        /// Đánh dấu toàn bộ thông báo thuộc phạm vi của người dùng là đã đọc
        /// </summary>
        [HttpPost]
        [Route("notifications/read-all")]
        public IHttpActionResult MarkAllNotificationsAsRead()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_NOTIFICATION_VIEW", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    string pbStr = nv.IDPB?.ToString();
                    Bu.CLASS_SYSTEM.NotificationService.MarkAllAsRead((long)user.IDUSER, (long)nv.MANV, nv.IDCTY?.ToString(), pbStr);
                    return Ok(new { success = true, message = "Đã đánh dấu tất cả thông báo là đã đọc." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/notifications/read-all Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi cập nhật trạng thái thông báo." });
            }
        }

        #region Device Management (Đăng ký thiết bị & Quản lý Push Token)

        /// <summary>
        /// POST: api/me/devices/register
        /// Đăng ký hoặc cập nhật token/thiết bị của người dùng hiện tại
        /// </summary>
        [HttpPost]
        [Route("devices/register")]
        public IHttpActionResult RegisterDevice([FromBody] Bu.CLASS_SYSTEM.DeviceRegisterDto req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.InstallationId))
                {
                    return BadRequest("Thiếu thông tin InstallationId của thiết bị.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    Bu.CLASS_SYSTEM.DeviceRegistrationService.RegisterOrUpdateDevice((long)user.IDUSER, (long)nv.MANV, req);
                    return Ok(new { success = true, message = "Đăng ký thiết bị thành công." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/devices/register Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi đăng ký thiết bị." });
            }
        }

        /// <summary>
        /// PUT: api/me/devices/{installationId}/preferences
        /// Cập nhật cài đặt nhận thông báo của thiết bị
        /// </summary>
        [HttpPut]
        [Route("devices/{installationId}/preferences")]
        public IHttpActionResult UpdateDevicePreferences(string installationId, [FromBody] Bu.CLASS_SYSTEM.DevicePreferenceDto req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(installationId) || req == null)
                {
                    return BadRequest("Dữ liệu cài đặt không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    bool updated = Bu.CLASS_SYSTEM.DeviceRegistrationService.UpdatePreferences((long)user.IDUSER, installationId, req);
                    if (!updated)
                    {
                        return Content(HttpStatusCode.NotFound, new { success = false, message = "Không tìm thấy thiết bị hoặc bạn không có quyền cập nhật thiết bị này." });
                    }

                    return Ok(new { success = true, message = "Cập nhật cài đặt thông báo thành công." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[PUT /api/me/devices/{installationId}/preferences Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi cập nhật cài đặt thiết bị." });
            }
        }

        /// <summary>
        /// DELETE: api/me/devices/{installationId}
        /// Hủy liên kết hoặc vô hiệu hóa nhận push trên thiết bị (khi đăng xuất)
        /// </summary>
        [HttpDelete]
        [Route("devices/{installationId}")]
        public IHttpActionResult DeactivateDevice(string installationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(installationId))
                {
                    return BadRequest("Mã thiết bị không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    bool deactivated = Bu.CLASS_SYSTEM.DeviceRegistrationService.DeactivateDevice((long)user.IDUSER, installationId);
                    return Ok(new { success = true, message = "Đã hủy liên kết thiết bị thành công.", deactivated = deactivated });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DELETE /api/me/devices/{installationId} Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi hủy liên kết thiết bị." });
            }
        }

        /// <summary>
        /// GET: api/me/devices
        /// Lấy danh sách thiết bị đang liên kết của người dùng hiện tại
        /// </summary>
        [HttpGet]
        [Route("devices")]
        public IHttpActionResult GetMyDevices()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error))
                    {
                        return error;
                    }

                    var list = Bu.CLASS_SYSTEM.DeviceRegistrationService.GetDevicesForUser((long)user.IDUSER);
                    return Ok(new { success = true, data = list });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/devices Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi lấy danh sách thiết bị." });
            }
        }

        #endregion

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
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_LEAVE", Bu.CLASS_SECURITY.ChannelAction.View))
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_LEAVE", Bu.CLASS_SECURITY.ChannelAction.View))
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
                        Id = r.ID_YEUCAU,
                        IdYeuCau = r.ID_YEUCAU,
                        Manv = r.MANV,
                        LoaiNghi = r.LOAI_NGHI ?? "Nghỉ phép năm",
                        LeaveType = r.LOAI_NGHI ?? "Nghỉ phép năm",
                        TuNgay = r.TU_NGAY.HasValue ? r.TU_NGAY.Value.ToString("dd/MM/yyyy") : "",
                        FromDate = r.TU_NGAY.HasValue ? r.TU_NGAY.Value.ToString("yyyy-MM-dd") : "",
                        DenNgay = r.DEN_NGAY.HasValue ? r.DEN_NGAY.Value.ToString("dd/MM/yyyy") : "",
                        ToDate = r.DEN_NGAY.HasValue ? r.DEN_NGAY.Value.ToString("yyyy-MM-dd") : "",
                        SoNgay = r.SO_NGAY ?? 1,
                        TotalDays = r.SO_NGAY ?? 1,
                        LyDo = r.LYDO,
                        Reason = r.LYDO,
                        TrangThai = r.TRANGTHAI ?? "PENDING",
                        Status = r.TRANGTHAI ?? "PENDING",
                        NgayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        CreatedAt = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("yyyy-MM-dd HH:mm") : "",
                        NguoiDuyet = r.NGUOI_DUYET,
                        ApproverName = r.NGUOI_DUYET,
                        NgayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        ApprovedAt = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("yyyy-MM-dd HH:mm") : "",
                        LyDoTuChoi = r.LYDO_TUCHOI,
                        Note = r.LYDO_TUCHOI
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                if (req == null)
                {
                    return BadRequest("Dữ liệu yêu cầu nghỉ phép không hợp lệ.");
                }

                string fromStr = !string.IsNullOrWhiteSpace(req.FromDate) ? req.FromDate : req.TuNgay;
                string toStr = !string.IsNullOrWhiteSpace(req.ToDate) ? req.ToDate : req.DenNgay;

                if (string.IsNullOrWhiteSpace(fromStr) || string.IsNullOrWhiteSpace(toStr))
                {
                    return BadRequest("Vui lòng chọn thời gian bắt đầu và kết thúc nghỉ.");
                }

                var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "d/M/yyyy", "dd-MM-yyyy" };
                DateTime tuNgay, denNgay;

                if (!DateTime.TryParseExact(fromStr.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out tuNgay) &&
                    !DateTime.TryParse(fromStr.Trim(), out tuNgay))
                {
                    return BadRequest("Định dạng ngày bắt đầu không hợp lệ. Vui lòng nhập chuẩn YYYY-MM-DD hoặc DD/MM/YYYY.");
                }

                if (!DateTime.TryParseExact(toStr.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out denNgay) &&
                    !DateTime.TryParse(toStr.Trim(), out denNgay))
                {
                    return BadRequest("Định dạng ngày kết thúc không hợp lệ. Vui lòng nhập chuẩn YYYY-MM-DD hoặc DD/MM/YYYY.");
                }

                if (denNgay < tuNgay)
                {
                    return BadRequest("Ngày kết thúc nghỉ không được nhỏ hơn ngày bắt đầu.");
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_LEAVE", Bu.CLASS_SECURITY.ChannelAction.Add))
                    {
                        return error;
                    }

                    decimal? inputDays = req.TotalDays ?? req.SoNgay;
                    decimal soNgay = inputDays.HasValue && inputDays.Value > 0 ? inputDays.Value : (decimal)(denNgay - tuNgay).TotalDays + 1;
                    string leaveType = !string.IsNullOrWhiteSpace(req.LeaveType) ? req.LeaveType : (!string.IsNullOrWhiteSpace(req.LoaiNghi) ? req.LoaiNghi : "Nghỉ phép năm");
                    string reason = !string.IsNullOrWhiteSpace(req.Reason) ? req.Reason : (req.LyDo ?? "");

                    if (string.IsNullOrWhiteSpace(reason)) return BadRequest("Nhập lý do xin nghỉ.");
                    if (soNgay <= 0 || soNgay > (decimal)(denNgay.Date-tuNgay.Date).TotalDays+1) return BadRequest("Số ngày nghỉ không phù hợp với khoảng ngày.");
                    // This date-only form creates full-day leave. Partial-day leave must carry an explicit time interval.
                    TimeSpan fromTime,toTime;
                    bool partial=!string.IsNullOrWhiteSpace(req.StartTime) || !string.IsNullOrWhiteSpace(req.EndTime);
                    DateTime startTime=tuNgay.Date,endTime=denNgay.Date.AddDays(1);
                    if (partial)
                    {
                        if (!TimeSpan.TryParseExact(req.StartTime ?? "",@"hh\:mm",CultureInfo.InvariantCulture,out fromTime) || !TimeSpan.TryParseExact(req.EndTime ?? "",@"hh\:mm",CultureInfo.InvariantCulture,out toTime)) return BadRequest("Nhập cả giờ bắt đầu và kết thúc theo HH:mm.");
                        startTime=tuNgay.Date.Add(fromTime);endTime=denNgay.Date.Add(toTime);
                        if (endTime<=startTime) return BadRequest("Thời điểm kết thúc nghỉ phải sau thời điểm bắt đầu.");
                    }
                    else if (soNgay != decimal.Truncate(soNgay)) return BadRequest("Đơn nghỉ một phần ngày cần khai báo khoảng giờ nghỉ.");
                    string benefit=leaveType.IndexOf("BHXH",StringComparison.OrdinalIgnoreCase)>=0 ? "NGHI_BHXH" : leaveType=="Nghỉ phép năm" ? "PHEP_NAM" : leaveType=="Nghỉ không lương" ? "KHONG_LUONG":leaveType=="Nghỉ hưởng lương khác" ? "NGHI_HUONG_LUONG":"LEGACY_UNCLASSIFIED";
                    string payer=benefit=="NGHI_BHXH" ? "BHXH":(benefit=="PHEP_NAM" || benefit=="NGHI_HUONG_LUONG") ? "CONG_TY":benefit=="KHONG_LUONG" ? "KHONG_CHI_TRA":"CHUA_XAC_DINH";
                    using (var transaction=db.Database.BeginTransaction())
                    {
                    db.Database.SqlQuery<decimal>("SELECT MANV FROM HR.TB_NHANVIEN WHERE MANV=:manv FOR UPDATE",new OracleParameter("manv",nv.MANV)).ToList();
                    var duplicate=db.Database.SqlQuery<decimal>("SELECT COUNT(*) FROM HR.TB_YEUCAU_NGHIPHEP WHERE MANV=:manv AND TRANGTHAI IN ('PENDING','APPROVED') AND TUNGAY<=:endDay AND DENNGAY>=:startDay",new OracleParameter("manv",nv.MANV),new OracleParameter("endDay",denNgay.Date),new OracleParameter("startDay",tuNgay.Date)).Single()>0;
                    if (duplicate) return Content(HttpStatusCode.Conflict,new {success=false,message="Đã có đơn nghỉ chờ duyệt hoặc đã duyệt trùng khoảng ngày."});
                    string insertSql = @"
                        INSERT INTO HR.TB_YEUCAU_NGHIPHEP 
                        (MANV, LOAIPHEP, TUNGAY, DENNGAY, SONGAY, LYDO, TRANGTHAI, CREATED_DATE, BATDAU_NGHI, KETTHUC_NGHI, GIAY_NGHI, NGUON_CHI_TRA, LOAI_HUONG_CONG)
                        VALUES (:p0, :p1, :p2, :p3, :p4, :p5, 'PENDING', SYSDATE, :startTime, :endTime, :seconds, :payer, :benefit)";

                    db.Database.ExecuteSqlCommand(
                        insertSql,
                        new OracleParameter("p0", nv.MANV),
                        new OracleParameter("p1", leaveType),
                        new OracleParameter("p2", tuNgay),
                        new OracleParameter("p3", denNgay),
                        new OracleParameter("p4", soNgay),
                        new OracleParameter("p5", reason.Trim()),
                        new OracleParameter("startTime",startTime),new OracleParameter("endTime",endTime),
                        new OracleParameter("seconds",soNgay*28800m),new OracleParameter("payer",payer),new OracleParameter("benefit",benefit)
                    );
                    transaction.Commit();
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Gửi đơn xin nghỉ phép thành công! Đơn của bạn đang chờ phê duyệt."
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_CORRECTION", Bu.CLASS_SECURITY.ChannelAction.View))
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
                        Id = r.ID_YEUCAU,
                        IdYeuCau = r.ID_YEUCAU,
                        Manv = r.MANV,
                        NgayCong = r.NGAY_CONG.HasValue ? r.NGAY_CONG.Value.ToString("dd/MM/yyyy") : "",
                        WorkDate = r.NGAY_CONG.HasValue ? r.NGAY_CONG.Value.ToString("yyyy-MM-dd") : "",
                        GioVaoMoi = r.GIO_VAO_MOI ?? "--:--",
                        RequestedCheckIn = r.GIO_VAO_MOI ?? "--:--",
                        GioRaMoi = r.GIO_RA_MOI ?? "--:--",
                        RequestedCheckOut = r.GIO_RA_MOI ?? "--:--",
                        LyDo = r.LYDO,
                        Reason = r.LYDO,
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                if (req == null)
                {
                    return BadRequest("Dữ liệu điều chỉnh công không hợp lệ.");
                }

                string dateStr = !string.IsNullOrWhiteSpace(req.WorkDate) ? req.WorkDate : req.NgayCong;
                if (string.IsNullOrWhiteSpace(dateStr))
                {
                    return BadRequest("Vui lòng chỉ định ngày cần điều chỉnh công.");
                }

                var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "d/M/yyyy", "dd-MM-yyyy" };
                DateTime ngayCong;
                if (!DateTime.TryParseExact(dateStr.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayCong) &&
                    !DateTime.TryParse(dateStr.Trim(), out ngayCong))
                {
                    return BadRequest("Định dạng ngày công không hợp lệ. Vui lòng nhập chuẩn YYYY-MM-DD hoặc DD/MM/YYYY.");
                }

                string gioVao = !string.IsNullOrWhiteSpace(req.RequestedCheckIn) ? req.RequestedCheckIn : req.GioVaoMoi;
                string gioRa = !string.IsNullOrWhiteSpace(req.RequestedCheckOut) ? req.RequestedCheckOut : req.GioRaMoi;
                string reason = !string.IsNullOrWhiteSpace(req.Reason) ? req.Reason : req.LyDo;

                if (string.IsNullOrWhiteSpace(gioVao) && string.IsNullOrWhiteSpace(gioRa))
                {
                    return BadRequest("Vui lòng nhập giờ vào hoặc giờ ra cần điều chỉnh.");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest("Vui lòng nêu rõ lý do điều chỉnh công.");
                }

                TimeSpan parsed;
                if ((!string.IsNullOrWhiteSpace(gioVao) && !TimeSpan.TryParseExact(gioVao.Trim(),@"hh\:mm",CultureInfo.InvariantCulture,out parsed)) ||
                    (!string.IsNullOrWhiteSpace(gioRa) && !TimeSpan.TryParseExact(gioRa.Trim(),@"hh\:mm",CultureInfo.InvariantCulture,out parsed)))
                    return BadRequest("Giờ vào/ra phải theo định dạng HH:mm (00:00–23:59).");
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_CORRECTION", Bu.CLASS_SECURITY.ChannelAction.Add))
                    {
                        return error;
                    }

                    DateTime? input=string.IsNullOrWhiteSpace(gioVao) ? (DateTime?)null : ngayCong.Date.Add(TimeSpan.Parse(gioVao.Trim(),CultureInfo.InvariantCulture));
                    DateTime? output=string.IsNullOrWhiteSpace(gioRa) ? (DateTime?)null : ngayCong.Date.Add(TimeSpan.Parse(gioRa.Trim(),CultureInfo.InvariantCulture));
                    if (input.HasValue && output.HasValue && output.Value<input.Value) output=output.Value.AddDays(1);
                    using (var transaction=db.Database.BeginTransaction())
                    {
                    db.Database.SqlQuery<decimal>("SELECT MANV FROM HR.TB_NHANVIEN WHERE MANV=:manv FOR UPDATE",new OracleParameter("manv",nv.MANV)).ToList();
                    var duplicate=db.Database.SqlQuery<decimal>("SELECT COUNT(*) FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE MANV=:manv AND TRUNC(NGAY)=:day AND TRANGTHAI='PENDING'",new OracleParameter("manv",nv.MANV),new OracleParameter("day",ngayCong.Date)).Single()>0;
                    if (duplicate) return Content(HttpStatusCode.Conflict,new {success=false,message="Ngày này đã có đơn điều chỉnh công chờ duyệt."});
                    string insertSql = @"
                        INSERT INTO HR.TB_YEUCAU_DIEUCHINHCONG 
                        (MANV, NGAY, LOAIDIEUCHINH, GIO_VAO, GIO_RA, LYDO, TRANGTHAI, CREATED_DATE, THOIDIEM_VAO_MOI, THOIDIEM_RA_MOI)
                        VALUES (:p0, :p1, 'Điều chỉnh công', :p2, :p3, :p4, 'PENDING', SYSDATE, :input, :output)";

                    db.Database.ExecuteSqlCommand(
                        insertSql,
                        new OracleParameter("p0", nv.MANV),
                        new OracleParameter("p1", ngayCong),
                        new OracleParameter("p2", string.IsNullOrWhiteSpace(gioVao) ? (object)DBNull.Value:gioVao.Trim()),
                        new OracleParameter("p3", string.IsNullOrWhiteSpace(gioRa) ? (object)DBNull.Value:gioRa.Trim()),
                        new OracleParameter("p4", reason.Trim()),new OracleParameter("input",(object)input ?? DBNull.Value),new OracleParameter("output",(object)output ?? DBNull.Value)
                    );
                    transaction.Commit();
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Gửi yêu cầu điều chỉnh chấm công thành công! Vui lòng chờ cấp quản lý phê duyệt."
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
        [Route("overtime-shifts")]
        public IHttpActionResult GetOvertimeShifts()
        {
            using (var db=new MyEntities())
            {
                if (!TryGetAuthenticatedEmployee(db,out var user,out var nv,out var error,"MOBILE_REQUEST_OVERTIME",Bu.CLASS_SECURITY.ChannelAction.View)) return error;
                var rows=db.TB_LOAICA.OrderBy(x=>x.IDLOAICA).Select(x=>new { idCa=x.IDLOAICA,name=x.TENLOAICA,coefficient=x.HESOLOAICA }).ToList();
                return Ok(new {success=true,data=rows});
            }
        }

        [HttpGet]
        [Route("overtime")]
        public IHttpActionResult GetMyOvertimeRequests()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_OVERTIME", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    var list = db.Database.SqlQuery<OvertimeRequestRow>(
                        @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 
                                 NVL(L.HESOLOAICA, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA,
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_OVERTIME", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    var r = db.Database.SqlQuery<OvertimeRequestRow>(
                        @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 
                                 NVL(L.HESOLOAICA, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA,
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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

                var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "d/M/yyyy", "dd-MM-yyyy" };
                DateTime ngayTangCa;
                if (!DateTime.TryParseExact(dateStr.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayTangCa) &&
                    !DateTime.TryParse(dateStr.Trim(), out ngayTangCa))
                {
                    return BadRequest("Định dạng ngày tăng ca không hợp lệ. Vui lòng nhập chuẩn YYYY-MM-DD hoặc DD/MM/YYYY.");
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
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_OVERTIME", Bu.CLASS_SECURITY.ChannelAction.Add))
                    {
                        return error;
                    }

                    // 1. Kiểm tra IDCA hợp lệ với bảng TB_LOAICA
                    decimal shiftId = req.IdCa ?? 1;
                    bool shiftExists = db.TB_LOAICA.Any(l => l.IDLOAICA == shiftId);
                    if (!shiftExists)
                    {
                        return BadRequest("Ca tăng ca không tồn tại. Vui lòng tải lại danh sách ca.");
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                string correlationId = Request.Headers.Contains("X-Correlation-Id")
                    ? Request.Headers.GetValues("X-Correlation-Id").FirstOrDefault()
                    : Guid.NewGuid().ToString("N");
                System.Diagnostics.Trace.TraceError($"[POST /api/me/overtime Error - CorrelationId: {correlationId}]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi gửi đơn đăng ký tăng ca. Vui lòng thử lại sau.", correlationId = correlationId });
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
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_OVERTIME", Bu.CLASS_SECURITY.ChannelAction.Edit))
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CancelOvertimeRequest Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi hủy đề xuất tăng ca." });
            }
        }

        /// <summary>
        /// GET: api/me/advance
        /// Lấy danh sách phiếu đề xuất tạm ứng lương của nhân viên với trạng thái thực tế
        /// </summary>
        [HttpGet]
        [Route("advance")]
        public IHttpActionResult GetMyAdvanceSalary()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_ADVANCE", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return error;
                    }

                    var advSvc = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                    var list = advSvc.GetList(null, null, "ALL", null, nv.MANV, 1, 100, out int _);

                    var result = list.Select(r => new MobileAdvanceSalaryDto
                    {
                        Id = r.Id,
                        IdUl = r.IdUlLink ?? r.Id,
                        Manv = r.Manv,
                        Thang = r.Thang,
                        Nam = r.Nam,
                        Ngay = !string.IsNullOrEmpty(r.AdvanceDate) && DateTime.TryParse(r.AdvanceDate, out var dt) ? dt.Day : 1,
                        AdvanceDate = r.AdvanceDate,
                        NgayUng = !string.IsNullOrEmpty(r.AdvanceDate) && DateTime.TryParse(r.AdvanceDate, out var dt2) ? dt2.ToString("dd/MM/yyyy") : r.AdvanceDate,
                        SoTienUng = r.Amount,
                        Amount = r.Amount,
                        GhiChu = r.Reason,
                        Reason = r.Reason,
                        TrangThai = r.Status,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt,
                        NgayTao = r.CreatedAt
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/advance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách tạm ứng lương." });
            }
        }

        /// <summary>
        /// POST: api/me/advance
        /// Gửi đề xuất xin tạm ứng lương của nhân viên: Lưu PENDING, tuyệt đối không INSERT vào TB_UNGLUONG
        /// </summary>
        [HttpPost]
        [Route("advance")]
        public IHttpActionResult CreateAdvanceSalary([FromBody] CreateAdvanceSalaryRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (req == null)
                {
                    return BadRequest("Dữ liệu đề xuất tạm ứng lương không hợp lệ.");
                }

                decimal amount = req.Amount ?? req.SoTien ?? req.SoTienUng ?? 0;
                if (amount <= 0)
                {
                    return BadRequest("Số tiền tạm ứng phải lớn hơn 0 đồng.");
                }

                string reason = !string.IsNullOrWhiteSpace(req.Reason) ? req.Reason : (!string.IsNullOrWhiteSpace(req.LyDo) ? req.LyDo : req.GhiChu);
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest("Vui lòng nêu rõ lý do tạm ứng lương.");
                }

                DateTime ngayUng = DateTime.Now;
                string dateStr = !string.IsNullOrWhiteSpace(req.AdvanceDate) ? req.AdvanceDate : req.NgayUng;
                if (!string.IsNullOrWhiteSpace(dateStr))
                {
                    var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "d/M/yyyy", "dd-MM-yyyy" };
                    if (!DateTime.TryParseExact(dateStr.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayUng))
                    {
                        return BadRequest("Ngày tạm ứng không hợp lệ. Nhập YYYY-MM-DD hoặc DD/MM/YYYY.");
                    }
                }

                using (var db = new MyEntities())
                {
                    if (!TryGetAuthenticatedEmployee(db, out var user, out var nv, out var error, "MOBILE_REQUEST_ADVANCE", Bu.CLASS_SECURITY.ChannelAction.Add))
                    {
                        return error;
                    }

                    int thang = req.Thang ?? ngayUng.Month;
                    int nam = req.Nam ?? ngayUng.Year;

                    var advSvc = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                    var createdDto = advSvc.CreateRequest((long)nv.MANV, amount, ngayUng, thang, nam, reason, (long)user.IDUSER);

                    return Ok(new
                    {
                        success = true,
                        data = createdDto,
                        message = "Gửi đề xuất tạm ứng lương thành công! Đơn đang ở trạng thái chờ duyệt (PENDING) và chưa ghi vào bảng lương."
                    });
                }
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Content(HttpStatusCode.Conflict,new { success=false, message=ex.Message }); }
            catch (KeyNotFoundException ex) { return Content(HttpStatusCode.NotFound,new { success=false, message=ex.Message }); }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[POST /api/me/advance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Không thể lưu yêu cầu. Vui lòng thử lại hoặc liên hệ quản trị viên." });
            }
        }

        /// <summary>
        /// GET: api/me/requests
        /// Lấy toàn bộ danh sách yêu cầu (Nghỉ phép, Chấm công, Tăng ca, Tạm ứng, Đổi hồ sơ) của nhân viên gộp chung
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

                    var views = SelfServiceViewRights(db,user);
                    var unified = new List<UnifiedRequestDto>();
                    var leavesDtoList = new List<MobileLeaveRequestDto>();
                    var correctionsDtoList = new List<MobileAttendanceCorrectionDto>();
                    var overtimesDtoList = new List<MobileOvertimeRequestDto>();
                    var advancesDtoList = new List<MobileAdvanceSalaryDto>();

                    // 1. Nghỉ phép
                    if (views.Contains("MOBILE_REQUEST_LEAVE"))
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
                            leavesDtoList.Add(new MobileLeaveRequestDto
                            {
                                Id = l.ID_YEUCAU,
                                IdYeuCau = l.ID_YEUCAU,
                                Manv = l.MANV,
                                LoaiNghi = l.LOAI_NGHI ?? "Nghỉ phép năm",
                                LeaveType = l.LOAI_NGHI ?? "Nghỉ phép năm",
                                TuNgay = tu,
                                FromDate = l.TU_NGAY.HasValue ? l.TU_NGAY.Value.ToString("yyyy-MM-dd") : "",
                                DenNgay = den,
                                ToDate = l.DEN_NGAY.HasValue ? l.DEN_NGAY.Value.ToString("yyyy-MM-dd") : "",
                                SoNgay = l.SO_NGAY ?? 1,
                                TotalDays = l.SO_NGAY ?? 1,
                                LyDo = l.LYDO,
                                Reason = l.LYDO,
                                TrangThai = l.TRANGTHAI ?? "PENDING",
                                Status = l.TRANGTHAI ?? "PENDING",
                                NgayTao = l.NGAY_TAO.HasValue ? l.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                CreatedAt = l.NGAY_TAO.HasValue ? l.NGAY_TAO.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                NguoiDuyet = l.NGUOI_DUYET,
                                ApproverName = l.NGUOI_DUYET,
                                NgayDuyet = l.NGAY_DUYET.HasValue ? l.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                ApprovedAt = l.NGAY_DUYET.HasValue ? l.NGAY_DUYET.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                LyDoTuChoi = l.LYDO_TUCHOI,
                                Note = l.LYDO_TUCHOI
                            });

                            unified.Add(new UnifiedRequestDto
                            {
                                Id = l.ID_YEUCAU,
                                RequestType = "LEAVE",
                                TotalDays=l.SO_NGAY,
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
                    catch { throw; }

                    // 2. Điều chỉnh công
                    if (views.Contains("MOBILE_REQUEST_CORRECTION"))
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
                            correctionsDtoList.Add(new MobileAttendanceCorrectionDto
                            {
                                Id = a.ID_YEUCAU,
                                IdYeuCau = a.ID_YEUCAU,
                                Manv = a.MANV,
                                NgayCong = dt,
                                WorkDate = a.NGAY_CONG.HasValue ? a.NGAY_CONG.Value.ToString("yyyy-MM-dd") : "",
                                GioVaoMoi = a.GIO_VAO_MOI ?? "--:--",
                                RequestedCheckIn = a.GIO_VAO_MOI ?? "--:--",
                                GioRaMoi = a.GIO_RA_MOI ?? "--:--",
                                RequestedCheckOut = a.GIO_RA_MOI ?? "--:--",
                                LyDo = a.LYDO,
                                Reason = a.LYDO,
                                TrangThai = a.TRANGTHAI ?? "PENDING",
                                Status = a.TRANGTHAI ?? "PENDING",
                                NgayTao = a.NGAY_TAO.HasValue ? a.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                CreatedAt = a.NGAY_TAO.HasValue ? a.NGAY_TAO.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                NguoiDuyet = a.NGUOI_DUYET,
                                NgayDuyet = a.NGAY_DUYET.HasValue ? a.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                LyDoTuChoi = a.LYDO_TUCHOI,
                                Note = a.LYDO_TUCHOI
                            });

                            unified.Add(new UnifiedRequestDto
                            {
                                Id = a.ID_YEUCAU,
                                RequestType = "ATTENDANCE",
                                TypeLabel = "Điều chỉnh công",
                                Title = $"Điều chỉnh công ngày {dt}",
                                Subtitle = $"{a.GIO_VAO_MOI ?? "--:--"} - {a.GIO_RA_MOI ?? "--:--"}",
                                DateRange = dt,
                                Status = a.TRANGTHAI ?? "PENDING",
                                CreatedAt = a.NGAY_TAO.HasValue ? a.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                Reason = a.LYDO,
                                RejectionReason = a.LYDO_TUCHOI
                            });
                        }
                    }
                    catch { throw; }

                    // 3. Tăng ca
                    if (views.Contains("MOBILE_REQUEST_OVERTIME"))
                    try
                    {
                        var ots = db.Database.SqlQuery<OvertimeRequestRow>(
                            @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO,
                                     NVL(L.HESOLOAICA, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA,
                                     Y.LYDO AS NOI_DUNG, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                                     NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                              FROM HR.TB_YEUCAU_TANGCA Y
                              LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                              LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA
                              WHERE Y.MANV = :p0 ORDER BY Y.CREATED_DATE DESC",
                            new OracleParameter("p0", nv.MANV)
                        ).ToList();

                        foreach (var o in ots)
                        {
                            string dt = o.NGAY_TANGCA.HasValue ? o.NGAY_TANGCA.Value.ToString("dd/MM/yyyy") : "";
                            overtimesDtoList.Add(new MobileOvertimeRequestDto
                            {
                                Id = o.ID_YEUCAU,
                                IdYeuCau = o.ID_YEUCAU,
                                Manv = o.MANV,
                                NgayTangCa = dt,
                                OtDate = o.NGAY_TANGCA.HasValue ? o.NGAY_TANGCA.Value.ToString("yyyy-MM-dd") : "",
                                SoGio = o.SO_GIO ?? 0,
                                Hours = o.SO_GIO ?? 0,
                                IdCa = o.IDCA ?? 1,
                                TenCa = o.TEN_CA ?? "Ca ngày",
                                ShiftType = o.TEN_CA ?? "Ca ngày",
                                HeSo = o.HE_SO ?? 1.5m,
                                Coefficient = o.HE_SO ?? 1.5m,
                                NoiDung = o.NOI_DUNG,
                                Reason = o.NOI_DUNG,
                                TrangThai = o.TRANGTHAI ?? "PENDING",
                                Status = o.TRANGTHAI ?? "PENDING",
                                NgayTao = o.NGAY_TAO.HasValue ? o.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                CreatedAt = o.NGAY_TAO.HasValue ? o.NGAY_TAO.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                NguoiDuyet = o.NGUOI_DUYET,
                                NgayDuyet = o.NGAY_DUYET.HasValue ? o.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                                LyDoTuChoi = o.LYDO_TUCHOI,
                                Note = o.LYDO_TUCHOI
                            });

                            unified.Add(new UnifiedRequestDto
                            {
                                Id = o.ID_YEUCAU,
                                RequestType = "OVERTIME",
                                Hours=o.SO_GIO,
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
                    catch { throw; }

                    // 4. Tạm ứng lương (Từ TB_YEUCAU_UNGLUONG thực tế)
                    if (views.Contains("MOBILE_REQUEST_ADVANCE"))
                    try
                    {
                        var advSvc = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                        var advs = advSvc.GetList(null, null, "ALL", null, nv.MANV, 1, 50, out int _);

                        foreach (var adv in advs)
                        {
                            advancesDtoList.Add(new MobileAdvanceSalaryDto
                            {
                                Id = adv.Id,
                                IdUl = adv.IdUlLink ?? adv.Id,
                                Manv = adv.Manv,
                                Thang = adv.Thang,
                                Nam = adv.Nam,
                                Ngay = 1,
                                AdvanceDate = adv.AdvanceDate,
                                NgayUng = adv.AdvanceDate,
                                SoTienUng = adv.Amount,
                                Amount = adv.Amount,
                                GhiChu = adv.Reason,
                                Reason = adv.Reason,
                                TrangThai = adv.Status,
                                Status = adv.Status,
                                CreatedAt = adv.CreatedAt,
                                NgayTao = adv.CreatedAt
                            });

                            unified.Add(new UnifiedRequestDto
                            {
                                Id = adv.Id,
                                RequestType = "ADVANCE",
                                Amount=adv.Amount,
                                TypeLabel = "Tạm ứng lương",
                                Title = $"Tạm ứng {adv.Amount:N0} đ",
                                Subtitle = adv.Reason ?? $"Tạm ứng lương kỳ {adv.Thang}/{adv.Nam}",
                                DateRange = adv.AdvanceDate,
                                Status = adv.Status,
                                CreatedAt = adv.CreatedAt,
                                Reason = adv.Reason,
                                RejectionReason = adv.RejectReason
                            });
                        }
                    }
                    catch { throw; }

                    // 5. Yêu cầu thay đổi hồ sơ (TB_YEUCAU_THAYDOI_HOSO)
                    if (views.Contains("MOBILE_PROFILE_EDIT") || views.Contains("MOBILE_REQUEST_INSURANCE") || views.Contains("MOBILE_REQUEST_DEPENDENT"))
                    try
                    {
                        var profSvc = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                        var profReqs = profSvc.GetRequests(null, "ALL", null, nv.MANV, 1, 50, out int _);

                        foreach (var pr in profReqs)
                        {
                            string code=pr.GroupCode=="DEPENDENTS" ? "MOBILE_REQUEST_DEPENDENT" : pr.GroupCode=="INSURANCE_INFO" ? "MOBILE_REQUEST_INSURANCE" : "MOBILE_PROFILE_EDIT";
                            if (!views.Contains(code)) continue;
                            string typeLabel = "Hồ sơ cá nhân";
                            if (pr.GroupCode == "AVATAR") typeLabel = "Ảnh đại diện";
                            else if (pr.GroupCode == "INSURANCE_INFO") typeLabel = "Thông tin bảo hiểm";
                            else if (pr.GroupCode == "DEPENDENTS") typeLabel = "Người phụ thuộc";

                            unified.Add(new UnifiedRequestDto
                            {
                                Id = pr.Id,
                                RequestType = pr.GroupCode,
                                TypeLabel = typeLabel,
                                Title = $"Đề xuất thay đổi {typeLabel}",
                                Subtitle = pr.Reason ?? "Cập nhật thông tin tự phục vụ",
                                DateRange = pr.CreatedAt,
                                Status = pr.Status,
                                CreatedAt = pr.CreatedAt,
                                Reason = pr.Reason,
                                RejectionReason = pr.RejectReason
                            });
                        }
                    }
                    catch { throw; }

                    var sorted = unified.OrderByDescending(u => RequestDate(u.CreatedAt)).ToList();
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            leaves = leavesDtoList,
                            corrections = correctionsDtoList,
                            overtimes = overtimesDtoList,
                            advances = advancesDtoList,
                            items = sorted
                        }
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/me/requests Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách yêu cầu." });
            }
        }
    }

    public class InsuranceParticipationReadRow
    {
        public decimal ID { get; set; }
        public decimal VUNG_LUONG { get; set; }
        public decimal THAM_GIA_BHXH { get; set; }
        public decimal THAM_GIA_BHYT { get; set; }
        public decimal THAM_GIA_BHTN { get; set; }
        public decimal THAM_GIA_TNLD_BNN { get; set; }
        public decimal HUONG_TY_LE_TNLD_UU_DAI { get; set; }
        public decimal? LUONG_DONG_BHXH_RIENG { get; set; }
        public DateTime NGAY_BAT_DAU { get; set; }
        public DateTime? NGAY_KET_THUC { get; set; }
        public string TRANG_THAI { get; set; }
    }
    public class UnionParticipationReadRow
    {
        public decimal ID { get; set; }
        public decimal LA_DOAN_VIEN { get; set; }
        public DateTime NGAY_GIA_NHAP { get; set; }
        public DateTime? NGAY_KET_THUC { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class AdvanceRow
    {
        public decimal IDUL { get; set; }
        public decimal MANV { get; set; }
        public decimal? THANG { get; set; }
        public decimal? NAM { get; set; }
        public decimal? NGAY { get; set; }
        public decimal? SOTIENUNG { get; set; }
        public string GHICHU { get; set; }
        public DateTime? CREATED_DATE { get; set; }
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

