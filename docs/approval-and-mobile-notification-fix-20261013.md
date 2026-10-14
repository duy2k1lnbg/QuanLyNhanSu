# Sửa duyệt đơn trên VPS và âm thanh/rung Mobile — 13/10/2026

## Kết quả đã áp dụng trên VPS

Constraint HR.SYS_C0012832 là khóa chính HR.TB_THONGBAO.ID trên database XEPDB1.

SEQ_THONGBAO ban đầu LAST_NUMBER=1429, trong khi TB_THONGBAO.MAX(ID)=2570. Các thao tác nộp/duyệt có ghi thông báo nên va chạm ID. Đã tái hiện bằng NotificationService trong transaction rồi rollback.

Đã áp dụng duy nhất DDL:
ALTER SEQUENCE HR.SEQ_THONGBAO RESTART START WITH 2571

Hai lượt probe trước repair tiêu thụ sequence nên receipt BEFORE ghi 1431. Probe sau repair ghi notification thành công rồi rollback; LAST_NUMBER sau kiểm thử=2572. Không xóa/sửa các đơn thật. Người dùng đã xác nhận Web duyệt/từ chối, đổi ảnh lần hai Mobile và nộp tạm ứng hoạt động sau repair.

Script để đối chiếu/tái áp dụng sau restore: database/repair_notification_sequence_vps_20261013.sql. Script idempotent với contract NOCACHE/INCREMENT 1/NOCYCLE và target XEPDB1, chỉ tiến sequence; DDL không rollback được. Không cần chạy lại trên VPS hiện đã sửa.

## Lỗi riêng còn lại: duyệt điều chỉnh công

Đã tái hiện lỗi ArgumentOutOfRangeException trên yêu cầu PENDING ngày 15/11/2026 của VPS. Stack trace:
OracleParameter..ctor(String parameterName, OracleDbType oraType)
OracleParameterCollection.Add(String name, OracleDbType dbType)
AttendanceApprovalService.ApproveAttendanceCorrection(...)

Dòng cũ:
cmdBc.Parameters.Add("p_le", 0m);

Literal 0m chọn overload OracleDbType thay vì giá trị numeric. Đã sửa:
cmdBc.Parameters.Add("p_le", OracleDbType.Decimal).Value = 0m;

Probe sau sửa đi qua toàn bộ luồng SQL tới commit boundary; bản probe ép throw và rollback trước commit, không duyệt đơn thật hoặc giữ thay đổi công/revision/notification. Đây là kiểm thử business/SQL, không thay authenticated HTTP/UI.

Bản sửa này nằm trong DLL backend xuất kèm. Chưa chép DLL lên VPS; lỗi Web sẽ còn trên binary cũ tới khi thay DLL/recycle ứng dụng.

## Transaction nộp tạm ứng

SalaryAdvanceService.CreateRequest trước đây insert đơn rồi tạo thông báo không cùng transaction. Khi notification lỗi, API có thể báo 500 trong khi đơn đã commit.

Đã đổi thành một transaction: lấy ID → insert PENDING → notification/push outbox → commit; lỗi thì rollback tất cả. Notification tạo đơn gửi cá nhân người nộp, không broadcast số tiền cho toàn bộ người dùng. Constructor public một tham số vẫn được giữ tương thích binary; callback nội bộ chỉ phục vụ kiểm thử lỗi transaction.

Không tự hủy các đơn cũ đã lưu ở những lần API báo lỗi. Người dùng phải đối chiếu lịch sử/trạng thái trước khi gửi lại.

## Mobile: âm thanh/rung

Chỉ thay Mobile, không thêm âm thanh Web:
- Kênh giữ nguyên hrms_default_channel để tương thích payload backend và APK cũ.
- Khai báo enableVibrate=true, mẫu [0,250,250,250], sound=default, importance MAX.
- App config khai báo quyền VIBRATE.
- Foreground handler giữ shouldPlaySound=true; OS tự phát cảnh báo, không thêm Vibration.vibrate riêng gây rung hai lần.
- Khi push tới, refresh notification badges/listeners.
- Settings có mục “Âm thanh và rung” ở cả năm ngôn ngữ. Android mở chính kênh thông báo; OEM không hỗ trợ thì fallback app settings. iOS mở app settings.

Kiểm tra VPS: một thiết bị Android 1.0.1 đang GRANTED/PUSH_ENABLED/ACTIVE và có token; 11 outbox jobs RECEIPT_OK. Không in token hoặc tự gửi notification thử cho người dùng. Receipt OK không chứng minh thiết bị đã phát âm thanh/rung.

Android không cho ứng dụng đổi sound/vibration của channel đã tạo bằng cách gọi lại setup. Người dùng vào cài đặt kênh để bật âm thanh/rung nếu đã tắt; không reset/delete channel hoặc tạo channel mới nhằm vượt lựa chọn im lặng của người dùng. Âm lượng/DND/chế độ im lặng/iOS notification settings vẫn do hệ điều hành quyết định.

SDK đã đối chiếu: https://docs.expo.dev/versions/v57.0.0/sdk/notifications/.
Push payload backend hiện sound=default, channelId=hrms_default_channel, priority=high; không cần migration token/channel/server để có nhạc riêng. Chưa thêm custom ringtone.

## Kiểm thử

- Business build .NET Framework 4.7.2 thành công; có 3 warnings có sẵn.
- 7 cases invalid input của SalaryAdvanceTransactionRegressionTests chạy qua NUnit assertions.
- Oracle local live rollback regression: giả lập lỗi sau khi đã insert request + notification; request/notification/push outbox không còn sau rollback. Sequence có thể tăng vì không transactional.
- VPS NotificationService trước repair: ORA-00001; sau repair: PASS, rollback sạch.
- VPS AttendanceApprovalService trước sửa parameter: ArgumentOutOfRangeException; sau sửa: PASS tới commit boundary, ép rollback.
- Mobile: 53/53 tests PASS, gồm 4 tests mới cho sound/vibration/channel settings/fallback/iOS.
- Mobile TypeScript noEmit PASS.
- dotnet test toàn project không hoàn thành trong lượt này; không báo regression suite đầy đủ là xanh. Các cases mới được compile và chạy bằng runner riêng với NUnit.
- Chưa kiểm tra âm thanh/rung trên điện thoại thật; cần cài APK mới và kiểm tra foreground/background/locked screen với cài đặt thiết bị phù hợp.
- Chưa authenticated HTTP/UI cho bản DLL mới trên VPS; chờ người dùng copy file rồi nghiệm thu.

## Hai loại file, hai nơi cài

1. Bu.dll là DLL của backend API, không phải file Mobile. Web và Mobile cùng gọi backend; cần thay DLL để sửa xử lý điều chỉnh công và transaction nộp tạm ứng.
2. APK là ứng dụng Android cài trên điện thoại, chứa thay đổi UI/cấu hình âm thanh-rung.

Gói tại artifacts/approval-notification-hotfix-20261013.
Bu.dll SHA256: 15F99FE673F8C88AFB0A7EA4C827546DB6F07CC5FEDD2D39640503AF3F681CD4.

DLL build từ snapshot mã nguồn hiện tại trong workspace (có các sửa trước của người dùng). Không thay Web.config/HRMS_API.dll/DA.dll hoặc chạy các SQL reset payroll để triển khai hotfix này. Cần đối chiếu snapshot/dependency với bản VPS trước deploy nếu VPS khác bản export hiện có.

Cách thay backend:
- Xác định đúng thư mục ứng dụng API trong IIS, nơi có Web.config và bin/HRMS_API.dll.
- Backup bin/Bu.dll hiện tại ra vị trí có timestamp.
- Dừng đúng application pool API hoặc dùng quy trình app_offline của ứng dụng.
- Chép Bu.dll mới vào bin/Bu.dll; PDB là tùy chọn.
- Khởi động/recycle đúng pool API.
- Nghiệm thu duyệt/từ chối các domain bằng tài khoản hợp lệ; thử điều chỉnh công 09:00–17:00, state không bị đổi hai lần, nguồn công/revision đúng.
- Nếu cần rollback binary, phục hồi DLL backup và recycle pool; không giảm sequence về giá trị cũ đã bị trùng.

Không chép DLL vào điện thoại. APK không sửa binary API đang chạy trên VPS. Không tự reset password Oracle/ứng dụng hoặc phân quyền.

Thông tin đăng nhập VPS chỉ dùng qua file tạm trong quá trình kết nối; file tạm đã được xóa, không đưa vào code/artifact/báo cáo.


## APK đã xuất và kiểm tra

Build :app:assembleDebug thành công ngày 13/10/2026. APK mới đã được chép tới đúng hai vị trí mà build-apk.bat sử dụng:
- D:/QL_NS/QuanLyNhanSu/HRMS.Mobile/HRMS_Mobile.apk
- D:/QL_NS/deploy_vps/mobile/HRMS_Mobile.apk

Bản có tên ngày trong gói hotfix:
- HRMS-Mobile-notification-20261013.apk

APK: 171832280 bytes, khoảng 163.9 MiB; SHA256 FFF76E22A04F50089DEACD68EC74D7FC8185A1027AFA88BC0B477CE4A5430684.
Package com.qlns.hrmsmobile, versionName 1.0.1, versionCode 2. Có đủ arm64-v8a, armeabi-v7a, x86, x86_64.

Đây là APK debug ký bằng debug key hiện có của dự án để cài thử, không phải bản ký phát hành lên store. apksigner verify PASS (v2). aapt xác nhận quyền POST_NOTIFICATIONS và VIBRATE. Đã mở chính APK để xác nhận bundle JavaScript tồn tại, có mục notificationSoundVibration, intent cài đặt kênh mới và API VPS https://tryhardagain.com/api/api. JavaScript/assets đã nhúng trong APK; không cần chạy Metro để cung cấp bundle.

Tất cả ba bản sao APK được đối chiếu SHA256 và trùng nhau. Chi tiết máy đọc được tại artifact-manifest.json trong gói hotfix. Chưa cài/chạy APK trên điện thoại thật trong phiên này.

Quy trình build dùng cùng assembleDebug như build-apk.bat. Các đầu ra Gradle/CMake được tách sang artifacts/mobile-android-build-20261013 vì thư mục build/.cxx cũ gặp AccessDenied trên Windows; file build-apk.bat không bị sửa. Bản app-debug.apk mới nguồn nằm tại artifacts/mobile-android-build-20261013/_app/outputs/apk/debug/app-debug.apk; không lấy nhầm app-debug.apk cũ trong android/app/build.

Cách nghiệm thu Mobile:
1. Cài HRMS_Mobile.apk lên Android. Bản này tiếp tục gọi API VPS.
2. Đăng nhập, kiểm tra quyền thông báo và tùy chọn nhận thông báo đang bật.
3. Trong màn hình Cài đặt chọn “Âm thanh và rung” → “Mở cài đặt”; bật âm thanh/rung cho kênh HRMS Notifications. Kiểm tra âm lượng thông báo và chế độ Không làm phiền.
4. Khi có thông báo nghiệp vụ thật, kiểm tra lúc app đang mở, đang chạy nền và màn hình khóa; badge/danh sách phải cập nhật, không rung hai lần.
5. APK cập nhật giao diện/cấu hình Mobile. Để hết lỗi API duyệt điều chỉnh công trong ảnh, vẫn phải thay Bu.dll ở backend VPS và recycle đúng ứng dụng API theo hướng dẫn phía trên.