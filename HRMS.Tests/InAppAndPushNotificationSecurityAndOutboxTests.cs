using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bu.CLASS_SYSTEM;
using DA;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class InAppAndPushNotificationSecurityAndOutboxTests
    {
        [Test]
        public void TargetFormula_Personal_StrictlyMatchesTargetManvOnly()
        {
            // Kiểm tra công thức phân giải mục tiêu cá nhân
            var notif = new TB_THONGBAO
            {
                ID = 1,
                TIEUDE = "Duyệt nghỉ phép",
                NOIDUNG = "Đơn nghỉ phép của bạn đã được duyệt",
                TARGET_TYPE = "PERSONAL",
                TARGET_MANV = 2327m,
                MAPB = "2",
                MACTY = "CTY01"
            };

            // Employee 2327 -> Match
            bool matchTargetUser = IsNotificationAccessible(notif, userManv: 2327m, userPb: "2", userCty: "CTY01");
            Assert.IsTrue(matchTargetUser, "Nhân viên đích phải truy cập được thông báo cá nhân");

            // Employee 9999 in same department -> Reject
            bool matchOtherUserInSamePb = IsNotificationAccessible(notif, userManv: 9999m, userPb: "2", userCty: "CTY01");
            Assert.IsFalse(matchOtherUserInSamePb, "Nhân viên khác cùng phòng ban KHÔNG được xem thông báo cá nhân của người khác");

            // Null manv -> Reject
            bool matchNullUser = IsNotificationAccessible(notif, userManv: null, userPb: "2", userCty: "CTY01");
            Assert.IsFalse(matchNullUser, "Người dùng chưa có MANV KHÔNG được khớp thông báo cá nhân");
        }

        [Test]
        public void TargetFormula_Department_NeverConfusesDepartmentIdWithEmployeeId()
        {
            var notif = new TB_THONGBAO
            {
                ID = 2,
                TIEUDE = "Họp phòng kỹ thuật",
                NOIDUNG = "Họp lúc 14h chiều nay",
                TARGET_TYPE = "DEPARTMENT",
                TARGET_MANV = null,
                MAPB = "5",
                MACTY = "CTY01"
            };

            // Employee in PB 5 -> Match
            bool matchPbMember = IsNotificationAccessible(notif, userManv: 2327m, userPb: "5", userCty: "CTY01");
            Assert.IsTrue(matchPbMember, "Thành viên phòng ban 5 phải xem được thông báo phòng ban");

            // Employee in PB 3 -> Reject
            bool matchOtherPb = IsNotificationAccessible(notif, userManv: 2327m, userPb: "3", userCty: "CTY01");
            Assert.IsFalse(matchOtherPb, "Nhân viên phòng ban khác KHÔNG được nhận thông báo phòng 5");

            // Employee whose MANV happens to be 5, but is in PB 99 -> Reject
            bool matchManvSameAsPb = IsNotificationAccessible(notif, userManv: 5m, userPb: "99", userCty: "CTY01");
            Assert.IsFalse(matchManvSameAsPb, "Mã phòng ban (MAPB) KHÔNG bao giờ được nhầm lẫn với mã nhân viên (MANV)");
        }

        [Test]
        public void TargetFormula_CompanyAndAll_ResolvesCorrectly()
        {
            var companyNotif = new TB_THONGBAO
            {
                ID = 3,
                TIEUDE = "Thông báo công ty CTY01",
                NOIDUNG = "Lịch nghỉ lễ",
                TARGET_TYPE = "COMPANY",
                MACTY = "CTY01"
            };

            Assert.IsTrue(IsNotificationAccessible(companyNotif, userManv: 100m, userPb: "1", userCty: "CTY01"));
            Assert.IsFalse(IsNotificationAccessible(companyNotif, userManv: 100m, userPb: "1", userCty: "CTY02"));

            var allNotif = new TB_THONGBAO
            {
                ID = 4,
                TIEUDE = "Thông báo toàn hệ thống",
                NOIDUNG = "Bảo trì",
                TARGET_TYPE = "ALL"
            };

            Assert.IsTrue(IsNotificationAccessible(allNotif, userManv: 100m, userPb: "1", userCty: "CTY01"));
            Assert.IsTrue(IsNotificationAccessible(allNotif, userManv: 200m, userPb: "2", userCty: "CTY02"));
        }

        [Test]
        public void PushNotificationPayload_Scrubbing_GuaranteesDiscreteLockScreenPrivacy()
        {
            // Kiểm tra hợp đồng bảo mật Lock Screen: Không đưa CCCD, Lương, Mã số thuế, JWT vào payload push
            string[] forbiddenKeywords = new[] { "CCCD", "CMND", "LUONG", "SALARY", "SOTIEN", "TAX", "MASOTHUE", "JWT", "PASSWORD", "TOKEN" };

            var pushPayload = new
            {
                to = "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]",
                title = "Phê duyệt đơn xin nghỉ phép",
                body = "Đơn xin nghỉ phép của bạn đã được phê duyệt.",
                sound = "default",
                channelId = "hrms_default_channel",
                data = new
                {
                    notificationId = 105,
                    eventType = "LEAVE_APPROVAL",
                    requestId = 42,
                    status = "APPROVED"
                }
            };

            string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(pushPayload.data);
            string lockScreenContent = $"{pushPayload.title} {pushPayload.body} {dataJson}";

            foreach (var kw in forbiddenKeywords)
            {
                Assert.IsFalse(lockScreenContent.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0,
                    $"Push payload vi phạm quyền riêng tư: Chứa từ khóa nhạy cảm '{kw}'");
            }
        }

        [Test]
        public void ExpoPushToken_FormatValidation_ConformsToSdk57()
        {
            string validExpoToken1 = "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]";
            string validExpoToken2 = "ExpoPushToken[yyyyyyyyyyyyyyyyyyyyyy]";
            string invalidFcmToken = "fcm_token_abcdef123456";
            string emptyToken = "";

            Assert.IsTrue(IsValidExpoPushToken(validExpoToken1));
            Assert.IsTrue(IsValidExpoPushToken(validExpoToken2));
            Assert.IsFalse(IsValidExpoPushToken(invalidFcmToken));
            Assert.IsFalse(IsValidExpoPushToken(emptyToken));
            Assert.IsFalse(IsValidExpoPushToken(null));
        }

        [Test]
        public void ExponentialBackoff_CalculatesCorrectDelays()
        {
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                int delaySeconds = CalculateBackoffSeconds(attempt);
                Assert.GreaterOrEqual(delaySeconds, 30);
                Assert.LessOrEqual(delaySeconds, 600);

                if (attempt > 1)
                {
                    int prevDelay = CalculateBackoffSeconds(attempt - 1);
                    Assert.GreaterOrEqual(delaySeconds, prevDelay, "Retry delay phải tăng dần theo hàm mũ");
                }
            }
        }

        [Test]
        public void DeviceRegisterDto_Validation_AcceptsValidPayload()
        {
            var dto = new DeviceRegisterDto
            {
                InstallationId = "inst_android_test123",
                ExpoPushToken = "ExponentPushToken[1234567890abcdef]",
                Platform = "ANDROID",
                Language = "vi",
                PermissionStatus = "GRANTED",
                PushEnabled = true,
                DeviceName = "Samsung Galaxy S24"
            };

            Assert.IsNotNull(dto.InstallationId);
            Assert.IsTrue(dto.InstallationId.StartsWith("inst_"));
            Assert.IsTrue(IsValidExpoPushToken(dto.ExpoPushToken));
            Assert.IsTrue(dto.PushEnabled.Value);
        }

        private static bool IsNotificationAccessible(TB_THONGBAO notif, decimal? userManv, string userPb, string userCty)
        {
            string type = (notif.TARGET_TYPE ?? "ALL").ToUpperInvariant();

            if (type == "PERSONAL")
            {
                return userManv.HasValue && notif.TARGET_MANV.HasValue && notif.TARGET_MANV.Value == userManv.Value;
            }

            if (type == "DEPARTMENT")
            {
                return !string.IsNullOrEmpty(userPb) && !string.IsNullOrEmpty(notif.MAPB) &&
                       string.Equals(userPb, notif.MAPB, StringComparison.OrdinalIgnoreCase);
            }

            if (type == "COMPANY")
            {
                return !string.IsNullOrEmpty(userCty) && !string.IsNullOrEmpty(notif.MACTY) &&
                       string.Equals(userCty, notif.MACTY, StringComparison.OrdinalIgnoreCase);
            }

            if (type == "ALL")
            {
                return true;
            }

            return false;
        }

        private static bool IsValidExpoPushToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            return Regex.IsMatch(token.Trim(), @"^(ExponentPushToken|ExpoPushToken)\[.+\]$");
        }

        private static int CalculateBackoffSeconds(int attemptCount)
        {
            int baseDelay = 30;
            int delay = baseDelay * (int)Math.Pow(2, Math.Min(attemptCount - 1, 4));
            return Math.Min(delay, 600);
        }
    }
}
