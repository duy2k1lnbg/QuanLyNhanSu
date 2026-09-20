using DA;
using HRMS_API.Filters;
using HRMS_API.Models;
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

                    var result = new MobileMeDto
                    {
                        IdUser = (int)user.IDUSER,
                        Username = user.USERNAME,
                        FullName = nv.HOTEN ?? user.FULLNAME ?? user.USERNAME,
                        Manv = user.MANV,
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

                    // 3. Bảng lương kỳ gần nhất
                    var latestBl = db.TB_BANGLUONG
                        .Where(b => b.MANV == manv)
                        .OrderByDescending(b => b.MAKYCONG)
                        .FirstOrDefault();

                    if (latestBl != null)
                    {
                        var kcForBl = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == latestBl.MAKYCONG);
                        bool isPaid = kcForBl != null && kcForBl.KHOA == 1;

                        dashboard.PayrollSummary = new MobilePayrollDto
                        {
                            Idbl = latestBl.IDBL,
                            Makycong = (int)latestBl.MAKYCONG,
                            Thang = latestBl.THANG,
                            Nam = latestBl.NAM,
                            LuongCoBan = latestBl.LUONG_CONG_THUCTE ?? 0,
                            CongThucTe = latestBl.CONG_THUCTE ?? 0,
                            ThucLinh = latestBl.THUC_LINH ?? 0,
                            TrangThaiChiTra = isPaid ? "Đã chi trả" : "Dự kiến chi trả"
                        };
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

                    var profile = new MobileProfileDto
                    {
                        Manv = nv.MANV,
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

                        result.DailyList.Add(new MobileAttendanceDailyDto
                        {
                            Ngay = row.NGAY.HasValue ? row.NGAY.Value.ToString("dd/MM") : "N/A",
                            Thu = row.THU ?? "",
                            GioVao = row.GIOVAO ?? "--:--",
                            GioRa = row.GIORA ?? "--:--",
                            NgayCong = nc,
                            KyHieu = row.KYHIEU ?? "X",
                            TrangThai = trangThai,
                            GhiChu = row.GHICHU
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

                    if (year.HasValue && month.HasValue)
                    {
                        int targetMakycong = year.Value * 100 + month.Value;
                        bl = db.TB_BANGLUONG.FirstOrDefault(b => b.MANV == manv && b.MAKYCONG == targetMakycong);
                    }
                    else
                    {
                        bl = db.TB_BANGLUONG.Where(b => b.MANV == manv).OrderByDescending(b => b.MAKYCONG).FirstOrDefault();
                    }

                    if (bl == null)
                    {
                        return Ok(new { success = true, data = (MobilePayrollDto)null, message = "Chưa phát sinh dữ liệu bảng lương cho kỳ này." });
                    }

                    var kc = db.TB_KYCONG.FirstOrDefault(k => k.MAKYCONG == bl.MAKYCONG);
                    bool isPaid = kc != null && kc.KHOA == 1;

                    decimal tongThuNhap = (bl.LUONG_CONG_THUCTE ?? 0) + (bl.PHUCAP_CONG_THUCTE ?? 0) + (bl.TIEN_TANGCA ?? 0) + (bl.TIEN_CHUYENCAN ?? 0) + (bl.TIEN_AN_CA ?? 0) + (bl.KHOAN_CONG_KHAC ?? 0);
                    decimal tongKhauTru = (bl.TIEN_BHXH_TRICH ?? 0) + (bl.TIEN_TAMUNG ?? 0) + (bl.THUE_TNCN ?? 0) + (bl.KHOAN_TRU_KHAC ?? 0);

                    var result = new MobilePayrollDto
                    {
                        Idbl = bl.IDBL,
                        Makycong = (int)bl.MAKYCONG,
                        Thang = bl.THANG,
                        Nam = bl.NAM,
                        LuongCoBan = bl.LUONG_CONG_THUCTE ?? 0,
                        CongChuan = bl.CONG_CHUAN ?? 26,
                        CongThucTe = bl.CONG_THUCTE ?? 0,
                        CongLamNgay = bl.CONG_LAMNGAY ?? 0,
                        CongLamDem = bl.CONG_LAMDEM ?? 0,
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
                        TienCongDoan = bl.TIEN_CONG_DOAN ?? 0,
                        TienTamUng = bl.TIEN_TAMUNG ?? 0,
                        ThueTncn = bl.THUE_TNCN ?? 0,
                        KhoanTruKhac = bl.KHOAN_TRU_KHAC ?? 0,
                        TongKhauTru = tongKhauTru,
                        ThucLinh = bl.THUC_LINH ?? 0,
                        TrangThaiChiTra = isPaid ? "Đã chi trả" : "Dự kiến chi trả"
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
    }
}
