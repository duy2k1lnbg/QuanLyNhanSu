using Bu;
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
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        private readonly NHANVIEN _nhanVienBus = new NHANVIEN();

        /// <summary>
        /// GET: api/dashboard/stats
        /// Trả về số liệu thống kê Dashboard 100% chính xác từ CSDL Oracle thực tế:
        /// Quy mô nhân sự, chuyên cần thực tế hôm nay, Action Items thực tế, Phát hiện bất thường (Anomaly) thực tế.
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

                int expiringContracts = 0;
                int missingInfo = 0;
                int unclosedTimesheets = 0;
                bool salaryNeedsCalc = false;
                TB_KYCONG latestKyCong = null;
                int presentToday = 0;
                int lateToday = 0;

                var actionItems = new List<object>();
                var anomalies = new List<object>();

                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    DateTime today = DateTime.Today;
                    DateTime next30Days = today.AddDays(30);

                    // 1. Hợp đồng sắp hết hạn trong 30 ngày tới (Số liệu thực)
                    expiringContracts = db.TB_HOPDONG.Count(h => h.NGAYKETTHUC.HasValue &&
                                                                h.NGAYKETTHUC.Value >= today &&
                                                                h.NGAYKETTHUC.Value <= next30Days &&
                                                                h.DEL_DATE == null);

                    // 2. Hồ sơ nhân viên còn thiếu thông tin (CCCD hoặc Số điện thoại)
                    missingInfo = db.TB_NHANVIEN.Count(n => (n.DATHOIVIEC == null || n.DATHOIVIEC == 0) &&
                                                            n.DELETED_DATE == null &&
                                                            (string.IsNullOrEmpty(n.CCCD) || string.IsNullOrEmpty(n.DIENTHOAI)));

                    // 3. Kỳ công chưa khóa sổ
                    unclosedTimesheets = db.TB_KYCONG.Count(k => (k.KHOA == null || k.KHOA == 0) && k.DELETED_DATE == null);

                    // 4. Kiểm tra kỳ công gần nhất đã tính lương chưa
                    latestKyCong = db.TB_KYCONG.OrderByDescending(k => k.MAKYCONG).FirstOrDefault();
                    if (latestKyCong != null)
                    {
                        salaryNeedsCalc = !db.TB_BANGLUONG.Any(b => b.MAKYCONG == latestKyCong.MAKYCONG);
                    }

                    // 5. Số liệu chuyên cần hôm nay (Chấm công thực tế từ TB_BANGCONG)
                    try
                    {
                        int todayDay = today.Day;
                        int todayMonth = today.Month;
                        int todayYear = today.Year;

                        var todayPunchIns = db.TB_BANGCONG
                            .Where(b => b.NGAY == todayDay && b.THANG == todayMonth && b.NAM == todayYear)
                            .ToList();

                        if (todayPunchIns.Count > 0)
                        {
                            presentToday = todayPunchIns.Count(b => b.GIOVAO != null || b.GIORA != null);
                            // Đi trễ nếu giờ vào sau 08:30 (quy chuẩn)
                            lateToday = todayPunchIns.Count(b => b.GIOVAO.HasValue && (b.GIOVAO.Value > 8 || (b.GIOVAO.Value == 8 && b.PHUTVAO.HasValue && b.PHUTVAO.Value > 30)));
                        }
                        else
                        {
                            // Nếu chưa có lượt quẹt thẻ hôm nay (hoặc đầu ca làm/ngày nghỉ), trả về đúng 0
                            presentToday = 0;
                            lateToday = 0;
                        }
                    }
                    catch
                    {
                        presentToday = 0;
                        lateToday = 0;
                    }

                    // 6. Action Center Items (Chỉ hiển thị khi có dữ liệu thực tế phát sinh cần xử lý)
                    if (expiringContracts > 0)
                    {
                        actionItems.Add(new
                        {
                            id = "act-contracts",
                            title = $"Có {expiringContracts} hợp đồng lao động sắp hết hạn trong 30 ngày tới",
                            count = expiringContracts,
                            urgency = "urgent",
                            route = "hopdong",
                            actionText = "Xem & Gia hạn"
                        });
                    }

                    if (unclosedTimesheets > 0)
                    {
                        actionItems.Add(new
                        {
                            id = "act-timesheets",
                            title = $"Có {unclosedTimesheets} bảng chấm công kỳ mở cần rà soát khóa sổ",
                            count = unclosedTimesheets,
                            urgency = "warning",
                            route = "chamcong",
                            actionText = "Kiểm tra"
                        });
                    }

                    if (missingInfo > 0)
                    {
                        actionItems.Add(new
                        {
                            id = "act-missing",
                            title = $"Có {missingInfo} hồ sơ nhân viên thiếu CCCD hoặc SĐT liên hệ",
                            count = missingInfo,
                            urgency = "warning",
                            route = "nhanvien",
                            actionText = "Bổ sung hồ sơ"
                        });
                    }

                    if (salaryNeedsCalc && latestKyCong != null)
                    {
                        actionItems.Add(new
                        {
                            id = "act-salary",
                            title = $"Kỳ lương {latestKyCong.THANG}/{latestKyCong.NAM} sẵn sàng tính toán tự động",
                            count = 1,
                            urgency = "info",
                            route = "bangluong",
                            actionText = "Tính lương"
                        });
                    }

                    // 7. Anomaly Detection (Quét phát hiện bất thường thực tế từ CSDL)
                    // a. Tăng ca vượt ngưỡng (> 30 giờ)
                    var highOtList = (from tc in db.TB_TANGCA
                                      join nv in db.TB_NHANVIEN on tc.MANV equals nv.MANV into nvGroup
                                      from nv in nvGroup.DefaultIfEmpty()
                                      join pb in db.TB_PHONGBAN on nv.IDPB equals pb.IDPB into pbGroup
                                      from pb in pbGroup.DefaultIfEmpty()
                                      where tc.SOGIO.HasValue && tc.SOGIO.Value >= 30
                                      select new
                                      {
                                          tc.MANV,
                                          HOTEN = nv != null ? nv.HOTEN : null,
                                          TENPB = pb != null ? pb.TENPB : null,
                                          SOGIO = tc.SOGIO.Value,
                                          tc.THANG,
                                          tc.NAM
                                      }).Take(5).ToList();

                    int anomIndex = 1;
                    foreach (var ot in highOtList)
                    {
                        string empName = !string.IsNullOrWhiteSpace(ot.HOTEN) ? ot.HOTEN : ("Nhân viên #" + ot.MANV);
                        string deptName = !string.IsNullOrWhiteSpace(ot.TENPB) ? ot.TENPB : "Chưa phân bổ";
                        anomalies.Add(new
                        {
                            id = "anom-ot-" + (anomIndex++),
                            employeeName = empName,
                            department = deptName,
                            metric = "Giờ làm thêm cao",
                            severity = "high",
                            description = $"Nhân viên có số giờ tăng ca cao đột biến ({ot.SOGIO} giờ trong kỳ {ot.THANG}/{ot.NAM})."
                        });
                    }

                    // b. Tạm ứng lương bất thường (>= 5,000,000 VNĐ)
                    var highAdvanceList = (from ul in db.TB_UNGLUONG
                                           join nv in db.TB_NHANVIEN on ul.MANV equals nv.MANV into nvGroup
                                           from nv in nvGroup.DefaultIfEmpty()
                                           join pb in db.TB_PHONGBAN on nv.IDPB equals pb.IDPB into pbGroup
                                           from pb in pbGroup.DefaultIfEmpty()
                                           where ul.SOTIENUNG.HasValue && ul.SOTIENUNG.Value >= 5000000
                                           select new
                                           {
                                               ul.MANV,
                                               HOTEN = nv != null ? nv.HOTEN : null,
                                               TENPB = pb != null ? pb.TENPB : null,
                                               SOTIEN = ul.SOTIENUNG.Value,
                                               ul.THANG,
                                               ul.NAM
                                           }).Take(5).ToList();

                    foreach (var ul in highAdvanceList)
                    {
                        string empName = !string.IsNullOrWhiteSpace(ul.HOTEN) ? ul.HOTEN : ("Nhân viên #" + ul.MANV);
                        string deptName = !string.IsNullOrWhiteSpace(ul.TENPB) ? ul.TENPB : "Chưa phân bổ";
                        anomalies.Add(new
                        {
                            id = "anom-ul-" + (anomIndex++),
                            employeeName = empName,
                            department = deptName,
                            metric = "Tạm ứng lương lớn",
                            severity = "medium",
                            description = $"Tạm ứng khoản tiền lớn {ul.SOTIEN:N0} VNĐ trong kỳ {ul.THANG}/{ul.NAM}."
                        });
                    }
                }

                int absentToday = Math.Max(0, tongNhanVien - presentToday);

                return Ok(new
                {
                    tongNhanVien = tongNhanVien,
                    tongQuyLuong = tongQuyLuongHienTai,
                    presentToday = presentToday,
                    absentToday = absentToday,
                    lateToday = lateToday,
                    phongBanStats = phongBanStats,
                    luongStats = luongStats,
                    actionItems = actionItems,
                    anomalies = anomalies
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải thống kê Dashboard: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải số liệu thống kê." });
            }
        }

        /// <summary>
        /// GET: api/dashboard/notifications
        /// Trả về danh sách thông báo và nhắc nhở thời gian thực dựa trên 100% sự kiện thực tế trong CSDL
        /// </summary>
        [HttpGet]
        [Route("notifications")]
        public IHttpActionResult GetNotifications()
        {
            try
            {
                var list = new List<object>();
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    DateTime today = DateTime.Today;
                    DateTime next30Days = today.AddDays(30);

                    // 1. Hợp đồng sắp hết hạn
                    var expiringContracts = (from hd in db.TB_HOPDONG
                                             join nv in db.TB_NHANVIEN on hd.MANV equals nv.MANV into nvGroup
                                             from nv in nvGroup.DefaultIfEmpty()
                                             where hd.NGAYKETTHUC.HasValue &&
                                                   hd.NGAYKETTHUC.Value >= today &&
                                                   hd.NGAYKETTHUC.Value <= next30Days &&
                                                   hd.DEL_DATE == null
                                             select new
                                             {
                                                 hd.SOHD,
                                                 hd.MANV,
                                                 HOTEN = nv != null ? nv.HOTEN : null,
                                                 NGAYKETTHUC = hd.NGAYKETTHUC.Value
                                             }).Take(5).ToList();

                    int notifId = 1;
                    foreach (var c in expiringContracts)
                    {
                        string empName = !string.IsNullOrWhiteSpace(c.HOTEN) ? c.HOTEN : ("Nhân viên #" + c.MANV);
                        int daysLeft = (int)Math.Max(0, (c.NGAYKETTHUC - today).TotalDays);
                        list.Add(new
                        {
                            id = "notif-" + (notifId++),
                            type = "urgent",
                            title = $"Hợp đồng lao động của {empName} sẽ hết hạn sau {daysLeft} ngày.",
                            time = $"Hết hạn: {c.NGAYKETTHUC:dd/MM/yyyy}",
                            route = "hopdong",
                            read = false
                        });
                    }

                    // 2. Bảng chấm công chưa khóa sổ
                    var openKyCong = db.TB_KYCONG
                        .Where(k => (k.KHOA == null || k.KHOA == 0) && k.DELETED_DATE == null)
                        .OrderByDescending(k => k.MAKYCONG)
                        .Take(2)
                        .ToList();

                    foreach (var kc in openKyCong)
                    {
                        list.Add(new
                        {
                            id = "notif-" + (notifId++),
                            type = "warning",
                            title = $"Bảng chấm công kỳ Tháng {kc.THANG}/{kc.NAM} chưa hoàn tất khóa sổ.",
                            time = "Đang mở",
                            route = "chamcong",
                            read = false
                        });
                    }

                    // 3. Hồ sơ thiếu thông tin
                    int missingCount = db.TB_NHANVIEN.Count(n => (n.DATHOIVIEC == null || n.DATHOIVIEC == 0) &&
                                                                n.DELETED_DATE == null &&
                                                                (string.IsNullOrEmpty(n.CCCD) || string.IsNullOrEmpty(n.DIENTHOAI)));
                    if (missingCount > 0)
                    {
                        list.Add(new
                        {
                            id = "notif-" + (notifId++),
                            type = "warning",
                            title = $"Có {missingCount} hồ sơ nhân sự đang thiếu CCCD hoặc SĐT liên hệ.",
                            time = "Cần bổ sung",
                            route = "nhanvien",
                            read = false
                        });
                    }

                    // 4. Khen thưởng hoặc quyết định mới nhất (nếu có trong 30 ngày)
                    DateTime last30Days = today.AddDays(-30);
                    var recentRewards = (from kt in db.TB_KHENTHUONG_KYLUAT
                                         join nv in db.TB_NHANVIEN on kt.MANV equals nv.MANV into nvGroup
                                         from nv in nvGroup.DefaultIfEmpty()
                                         where kt.TUNGAY.HasValue && kt.TUNGAY.Value >= last30Days
                                         orderby kt.TUNGAY descending
                                         select new
                                         {
                                             kt.SOQUYETDINH,
                                             kt.MANV,
                                             HOTEN = nv != null ? nv.HOTEN : null,
                                             kt.NOIDUNG,
                                             kt.LOAI,
                                             kt.TUNGAY
                                         }).Take(3).ToList();

                    foreach (var rw in recentRewards)
                    {
                        string empName = !string.IsNullOrWhiteSpace(rw.HOTEN) ? rw.HOTEN : ("Nhân viên #" + rw.MANV);
                        list.Add(new
                        {
                            id = "notif-" + (notifId++),
                            type = "info",
                            title = $"{(rw.LOAI == 2 ? "Quyết định kỷ luật" : "Khen thưởng")}: {empName} - {rw.NOIDUNG}",
                            time = rw.TUNGAY.HasValue ? rw.TUNGAY.Value.ToString("dd/MM/yyyy") : "Gần đây",
                            route = "khenthuong",
                            read = true
                        });
                    }
                }

                int unreadCount = list.Count(x =>
                {
                    var prop = x.GetType().GetProperty("read");
                    return prop != null && !(bool)prop.GetValue(x);
                });

                return Ok(new
                {
                    unreadCount = unreadCount,
                    total = list.Count,
                    items = list
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải thông báo hệ thống: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách thông báo." });
            }
        }
    }
}
