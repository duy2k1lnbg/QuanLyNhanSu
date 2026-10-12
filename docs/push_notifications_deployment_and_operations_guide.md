# Hướng Dẫn Vận Hành & Triển Khai Hệ Thống Thông Báo Đẩy (In-App & Remote Push Notifications)

Hệ thống thông báo đẩy của HRMS được thiết kế theo kiến trúc **Transactional Outbox**, tuân thủ nghiêm ngặt chuẩn **Expo SDK ~57**, hỗ trợ đầy đủ 5 ngôn ngữ (`vi`, `en`, `ja`, `zh-CN`, `ko`), đồng bộ đa thiết bị an toàn và bảo mật tối đa dữ liệu nhạy cảm trên màn hình khóa.

---

## 1. Tổng Quan Kiến Trúc (Architecture Overview)

```
+-----------------------------------------------------------------------------------+
|                            NGHIỆP VỤ HRMS (BUSINESS)                              |
|  - Duyệt / Từ chối đơn nghỉ phép (Leave Approval / Rejection)                     |
|  - Điều chỉnh công (Attendance Correction)                                        |
|  - Đăng ký tăng ca (Overtime Registration)                                        |
|  - Tạm ứng lương (Salary Advance Approval / Disbursed)                            |
|  - Yêu cầu cập nhật hồ sơ (Employee Profile Request)                              |
|  - Thông báo hành chính nội bộ (Desktop / Web Admin / Mobile)                      |
+-----------------------------------------+-----------------------------------------+
                                          |
                   Cùng 1 Database Transaction (Oracle DB)
                                          |
                                          v
+-----------------------------------------+-----------------------------------------+
|                TB_THONGBAO              |              TB_PUSH_OUTBOX             |
|  - ID (SEQ_THONGBAO.NEXTVAL)            |  - ID (SEQ_PUSH_OUTBOX.NEXTVAL)         |
|  - TIEUDE, NOIDUNG, NGUOIDANG           |  - NOTIFICATION_ID, INSTALLATION_ID     |
|  - TARGET_TYPE (PERSONAL/DEPT/CTY/ALL)  |  - EXPO_PUSH_TOKEN, STATUS (QUEUED)     |
|  - TARGET_MANV, MAPB, MACTY             |  - TITLE, BODY, PAYLOAD_JSON            |
|  - EVENT_TYPE, ENTITY_ID                |  - RETRY_COUNT, NEXT_RETRY_AT           |
+-----------------------------------------+-----------------------------------------+
                                          |
                     Quét ngầm độc lập (Mỗi 15-30 giây)
                                          |
                                          v
+-----------------------------------------------------------------------------------+
|               BACKGROUND WORKER (PushNotificationHostingWorker)                   |
|  1. Khóa nguyên tử hàng đợi (Atomic lock: STATUS='PROCESSING', WORKER_ID, TIMEOUT)|
|  2. Tái xác thực tài khoản (Pre-flight account switch check)                      |
|  3. Đóng gói lô (Chunks tối đa 100 tin nhắn, nén gzip)                           |
|  4. Gửi tới Expo Push API (https://exp.host/--/api/v2/push/send)                  |
|  5. Phân tích phản hồi lỗi:                                                      |
|     - 'DeviceNotRegistered': Vô hiệu hóa thiết bị ngay (TB_USER_DEVICE.IS_ACTIVE=0)|
|     - Lỗi mạng/rate limit: Exponential backoff retry (30s, 60s, 120s, ...)        |
|  6. Kiểm tra biên nhận Expo (Receipt Worker: /--/api/v2/push/getReceipts)        |
+-----------------------------------------------------------------------------------+
                                          |
                                          v
+-----------------------------------------------------------------------------------+
|                   THIẾT BỊ ĐẦU CUỐI (HRMS.Mobile - Expo SDK 57)                   |
|  - Nhận Push Notification hiển thị trên Lock Screen / Notification Center         |
|  - Hiển thị Banner khi App đang chạy Foreground (shouldShowBanner: true)          |
|  - Tự động làm mới danh sách thông báo và số đếm khi nhận tin Foreground          |
|  - Điều hướng sâu thông minh (Deep Linking Navigation) khi người dùng chạm tin   |
+-----------------------------------------------------------------------------------+
```

---

## 2. Cơ Sở Dữ Liệu Oracle (Database Schema & Migration)

### 2.1 Các bảng và Sequence chính
- **`TB_USER_DEVICE`**: Lưu thông tin định danh thiết bị theo `INSTALLATION_ID`, hỗ trợ 1 nhân viên sử dụng nhiều thiết bị (điện thoại cá nhân, máy tính bảng).
- **`TB_PUSH_OUTBOX`**: Hàng đợi thông báo đẩy đảm bảo giao dịch ACID, chống mất mát thông báo khi mạng chập chờn.
- **Sequence**:
  - `SEQ_USER_DEVICE`: Tăng tự động ID thiết bị.
  - `SEQ_PUSH_OUTBOX`: Tăng tự động ID bản ghi outbox.
  - `SEQ_THONGBAO`: Tăng tự động ID thông báo in-app.

### 2.2 Công thức phân giải mục tiêu thông báo (Target Formula)
Toàn bộ hệ thống áp dụng công thức đồng nhất, ngăn chặn triệt để lỗ hổng đoán ID và rò rỉ dữ liệu khi `MANV IS NULL`:
```sql
(
    (UPPER(t.TARGET_TYPE) = 'PERSONAL' AND t.TARGET_MANV = :p_manv)
    OR (UPPER(t.TARGET_TYPE) = 'DEPARTMENT' AND t.MAPB = :p_pb AND :p_pb IS NOT NULL)
    OR (UPPER(t.TARGET_TYPE) = 'COMPANY' AND t.MACTY = :p_cty AND :p_cty IS NOT NULL)
    OR (UPPER(NVL(t.TARGET_TYPE, 'ALL')) = 'ALL')
)
```

### 2.3 Quy trình Rollback cơ sở dữ liệu
Nếu cần rollback về trước phiên bản V1.41:
```bash
sqlplus HR/HR_PASSWORD@localhost:1521/orcl @database/migrations/V1_41_rollback__inapp_and_remote_push_notifications.sql
```

---

## 3. Cấu Hình Mobile (Expo SDK ~57)

### 3.1 Cấu hình Plugin trong `app.json`
```json
{
  "expo": {
    "name": "HRMS Mobile",
    "slug": "hrms-mobile",
    "version": "1.0.0",
    "android": {
      "package": "com.qlns.hrmsmobile",
      "permissions": [
        "INTERNET",
        "ACCESS_NETWORK_STATE",
        "POST_NOTIFICATIONS"
      ]
    },
    "plugins": [
      "expo-secure-store",
      "expo-localization",
      [
        "expo-notifications",
        {
          "icon": "./assets/android-icon-monochrome.png",
          "color": "#1E40AF"
        }
      ]
    ]
  }
}
```

### 3.2 Khởi tạo Android Notification Channel
Trong Android 8.0+ (API 26+), kênh thông báo là bắt buộc để phát âm thanh và hiển thị trên thanh trạng thái:
- **Channel ID:** `hrms_default_channel`
- **Tên:** `HRMS Notifications`
- **Importance:** `MAX`
- **Vibration Pattern:** `[0, 250, 250, 250]`
- **Light Color:** `#1E40AF`

### 3.3 Thiết lập EAS Project ID cho môi trường sản xuất
Khi phát hành ứng dụng qua Expo Application Services (EAS):
1. Chạy lệnh: `npx eas init` trong thư mục `HRMS.Mobile`.
2. Lấy `projectId` gắn vào `app.json` hoặc biến môi trường `EXPO_PUBLIC_EAS_PROJECT_ID`.
3. Khi chạy trên môi trường giả lập (Simulator) hoặc máy thật chưa có EAS project, service kích hoạt chế độ **Zero-Crash Graceful Fallback**, người dùng vẫn sử dụng đầy đủ các tính năng in-app mà không gặp lỗi gián đoạn.

---

## 4. Bảo Mật & Quyền Riêng Tư (Security & Privacy)

### 4.1 Bảo mật dữ liệu màn hình khóa (Lock Screen Privacy)
Theo quy chuẩn an ninh thông tin doanh nghiệp, payload thông báo gửi qua máy chủ trung gian và hiển thị trên màn hình khóa **tuyệt đối không bao gồm**:
- Số CCCD / CMND (`cccd`, `soCccd`)
- Số tiền lương, thực lĩnh, tạm ứng (`salary`, `soTien`, `luong`)
- Mã số thuế (`taxId`, `maSoThue`)
- Mật khẩu, JWT token, Session ID

Nội dung payload chỉ chứa tiêu đề lịch sự, mô tả ngắn gọn và metadata điều hướng an toàn:
```json
{
  "to": "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]",
  "title": "Phê duyệt đơn xin nghỉ phép",
  "body": "Đơn xin nghỉ phép của bạn đã được phê duyệt.",
  "sound": "default",
  "channelId": "hrms_default_channel",
  "data": {
    "notificationId": 105,
    "eventType": "LEAVE_APPROVAL",
    "requestId": 42,
    "status": "APPROVED"
  }
}
```

### 4.2 Tách rời thiết bị khi Đăng xuất (Device Decoupling on Logout)
- Khi người dùng đăng xuất trên thiết bị di động, ứng dụng tự động gửi `X-Installation-Id` qua header và gọi:
  `DELETE api/me/devices/{installationId}`
- Backend cập nhật ngay lập tức `TB_USER_DEVICE.IS_ACTIVE = 0` và hủy liên kết token để đảm bảo nếu người khác đăng nhập vào cùng thiết bị, họ sẽ không bao giờ nhận được thông báo của người trước.

---

## 5. Vận Hành & Quản Trị Hệ Thống (Operations & Admin API)

Backend cung cấp bộ API quản trị thông báo tại `api/notifications/admin/*` (yêu cầu quyền quản trị viên hệ thống):

### 5.1 Kiểm tra trạng thái hàng đợi Outbox
- **Endpoint:** `GET /api/notifications/admin/outbox-status`
- **Kết quả trả về:**
  - `totalQueued`: Số tin nhắn đang chờ gửi.
  - `totalSent`: Số tin nhắn đã gửi thành công.
  - `totalFailed`: Số tin nhắn lỗi đã quá số lần thử lại tối đa.
  - `totalDevices`: Tổng số thiết bị đang đăng ký nhận tin.
  - `activeDevices`: Số thiết bị đang hoạt động.
  - `recentOutbox`: Danh sách 20 tin nhắn gần nhất kèm trạng thái và lỗi (nếu có).

### 5.2 Kích hoạt xử lý Outbox thủ công
- **Endpoint:** `POST /api/notifications/admin/process-outbox`
- **Mục đích:** Dùng khi muốn ép hệ thống quét hàng đợi ngay lập tức mà không cần đợi chu kỳ cron ngầm.

### 5.3 Gửi thông báo thử nghiệm (Test Push)
- **Endpoint:** `POST /api/notifications/admin/test-push`
- **Body JSON:**
```json
{
  "installationId": "inst_android_xxxxxxxx",
  "expoPushToken": "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]",
  "title": "Kiểm tra thông báo đẩy",
  "body": "Tin nhắn thử nghiệm kết nối tổng đài thông báo HRMS.",
  "eventType": "TEST_PUSH"
}
```

---

## 6. Bảng Kiểm Thử & Nghiệm Thu (Verification Matrix)

| Hạng mục kiểm thử | Công cụ / Lệnh | Tiêu chí nghiệm thu | Kết quả thực tế |
|---|---|---|---|
| **Độ khớp 5 ngôn ngữ Mobile** | `npm test` trong `HRMS.Mobile` | `vi`, `en`, `ja`, `zh-CN`, `ko` có 100% cùng số lượng keys, không có chuỗi rỗng | **PASSED (100%)** |
| **Kiểm tra kiểu dữ liệu Mobile** | `npx tsc --noEmit` trong `HRMS.Mobile` | TypeScript build không lỗi cú pháp, tuân thủ Expo SDK 57 | **PASSED (0 errors)** |
| **Bảo mật & Outbox Backend** | `dotnet test --filter "InAppAndPushNotificationSecurityAndOutboxTests"` | Khóa nguyên tử, exponential backoff, che giấu dữ liệu nhạy cảm | **PASSED (7/7 tests)** |
| **Biên dịch toàn bộ Solution C#** | `MSBuild HRMS.sln /p:Configuration=Debug` | `DA.dll`, `Bu.dll`, `HRMS_API.dll`, `QLyNSu.exe`, `HRMS.Tests.dll` | **BUILD SUCCEEDED (0 errors)** |
| **Biên dịch Web Admin Portal** | `npm run build` trong `HRMS.Web` | TypeScript và Vite bundle build hoàn tất | **BUILD SUCCEEDED (0 errors)** |
