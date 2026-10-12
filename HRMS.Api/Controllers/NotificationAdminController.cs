using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Filters;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    public class TestSendRequest
    {
        public string InstallationId { get; set; }
        public string ExpoPushToken { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

    /// <summary>
    /// API Quản trị & Điều phối Outbox Thông báo Push
    /// Yêu cầu phân quyền F_SYSTEM_THONGBAO theo kênh người dùng
    /// </summary>
    [RoutePrefix("api/admin/notifications")]
    public class NotificationAdminController : ApiController
    {
        /// <summary>
        /// POST: api/admin/notifications/outbox/process
        /// Kích hoạt thủ công chu kỳ xử lý Outbox gửi tới Expo và kiểm tra receipts
        /// </summary>
        [HttpPost]
        [Route("outbox/process")]
        [JwtAuthorize(Right = "F_SYSTEM_THONGBAO", Action = ChannelAction.Edit)]
        public async Task<IHttpActionResult> ProcessOutboxManually(int batchSize = 100)
        {
            try
            {
                int sent = await PushNotificationOutboxProcessor.ProcessPendingOutboxAsync(batchSize);
                int receipts = await PushNotificationOutboxProcessor.ProcessPendingReceiptsAsync(300);

                return Ok(new
                {
                    success = true,
                    message = $"Đã xử lý {sent} jobs gửi tới Expo và kiểm tra {receipts} receipts.",
                    dispatchedJobs = sent,
                    checkedReceipts = receipts
                });
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ProcessOutboxManually Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi xử lý Outbox: " + ex.Message });
            }
        }

        /// <summary>
        /// GET: api/admin/notifications/outbox/status
        /// Xem thống kê trạng thái hàng đợi Outbox và số lượng thiết bị đang kết nối
        /// </summary>
        [HttpGet]
        [Route("outbox/status")]
        [JwtAuthorize(Right = "F_SYSTEM_THONGBAO", Action = ChannelAction.View)]
        public IHttpActionResult GetOutboxStatus()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var statusCounts = db.Database.SqlQuery<StatusCountRow>(@"
                        SELECT STATUS, COUNT(*) AS TOTAL
                        FROM TB_PUSH_OUTBOX
                        GROUP BY STATUS"
                    ).ToList();

                    var deviceStats = db.Database.SqlQuery<DeviceStatRow>(@"
                        SELECT 
                            COUNT(*) AS TOTAL_DEVICES,
                            NVL(SUM(CASE WHEN IS_ACTIVE = 1 THEN 1 ELSE 0 END), 0) AS ACTIVE_DEVICES,
                            NVL(SUM(CASE WHEN PERMISSION_STATUS = 'GRANTED' THEN 1 ELSE 0 END), 0) AS GRANTED_DEVICES,
                            NVL(SUM(CASE WHEN PUSH_ENABLED = 1 THEN 1 ELSE 0 END), 0) AS PUSH_ENABLED_DEVICES
                        FROM TB_USER_DEVICE"
                    ).FirstOrDefault();

                    return Ok(new
                    {
                        success = true,
                        outbox = statusCounts,
                        devices = deviceStats
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GetOutboxStatus Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi lấy trạng thái Outbox: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: api/admin/notifications/test-send
        /// Gửi thông báo thử nghiệm tới một thiết bị cụ thể (không spam toàn công ty)
        /// </summary>
        [HttpPost]
        [Route("test-send")]
        [JwtAuthorize(Right = "F_SYSTEM_THONGBAO", Action = ChannelAction.Add)]
        public async Task<IHttpActionResult> SendTestNotification([FromBody] TestSendRequest req)
        {
            try
            {
                if (req == null || (string.IsNullOrWhiteSpace(req.InstallationId) && string.IsNullOrWhiteSpace(req.ExpoPushToken)))
                {
                    return BadRequest("Vui lòng cung cấp InstallationId hoặc ExpoPushToken của thiết bị thử nghiệm.");
                }

                using (var db = new MyEntities())
                {
                    TB_USER_DEVICE dev = null;
                    if (!string.IsNullOrWhiteSpace(req.InstallationId))
                    {
                        dev = db.Database.SqlQuery<TB_USER_DEVICE>(
                            "SELECT * FROM TB_USER_DEVICE WHERE INSTALLATION_ID = :p0",
                            new OracleParameter("p0", req.InstallationId.Trim())
                        ).FirstOrDefault();
                    }
                    else if (!string.IsNullOrWhiteSpace(req.ExpoPushToken))
                    {
                        dev = db.Database.SqlQuery<TB_USER_DEVICE>(
                            "SELECT * FROM TB_USER_DEVICE WHERE EXPO_PUSH_TOKEN = :p0 AND ROWNUM = 1",
                            new OracleParameter("p0", req.ExpoPushToken.Trim())
                        ).FirstOrDefault();
                    }

                    if (dev == null)
                    {
                        return Content(HttpStatusCode.NotFound, new { success = false, message = "Không tìm thấy thiết bị thử nghiệm tương ứng trong CSDL." });
                    }

                    if (string.IsNullOrWhiteSpace(dev.EXPO_PUSH_TOKEN))
                    {
                        return BadRequest("Thiết bị này chưa có Expo Push Token hợp lệ.");
                    }

                    string connStr = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                    long notifId;
                    using (var conn = new OracleConnection(connStr))
                    {
                        conn.Open();
                        notifId = NotificationService.CreateNotification(
                            conn,
                            null,
                            req.Title ?? "Thử nghiệm thông báo HRMS",
                            req.Body ?? "Đây là thông báo thử nghiệm kết nối In-App và Remote Push.",
                            "TEST_NOTIFICATION",
                            "ADMIN_TEST",
                            targetType: "PERSONAL",
                            targetManv: dev.MANV.HasValue ? (long)dev.MANV.Value : (long?)null,
                            eventType: "TEST_PUSH",
                            discretePushBody: "HRMS: Đây là thông báo thử nghiệm."
                        );
                    }

                    // Kích hoạt ngay outbox gửi đi
                    await PushNotificationOutboxProcessor.ProcessPendingOutboxAsync(10);

                    return Ok(new
                    {
                        success = true,
                        message = "Đã xếp hàng và gửi thông báo thử nghiệm thành công.",
                        notificationId = notifId,
                        targetDevice = dev.INSTALLATION_ID,
                        platform = dev.PLATFORM
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[SendTestNotification Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi gửi thông báo thử nghiệm: " + ex.Message });
            }
        }
    }

    public class StatusCountRow
    {
        public string STATUS { get; set; }
        public decimal TOTAL { get; set; }
    }

    public class DeviceStatRow
    {
        public decimal TOTAL_DEVICES { get; set; }
        public decimal ACTIVE_DEVICES { get; set; }
        public decimal GRANTED_DEVICES { get; set; }
        public decimal PUSH_ENABLED_DEVICES { get; set; }
    }
}
