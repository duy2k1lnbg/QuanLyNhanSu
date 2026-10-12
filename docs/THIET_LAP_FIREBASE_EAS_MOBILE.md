# Cập nhật Mobile và hoàn tất thông báo đẩy

Mã Mobile đã có đường dẫn đổi ngôn ngữ trước đăng nhập, mục đổi mật khẩu, chọn/chụp ảnh và xác nhận tải ảnh. Các thao tác ghi vẫn kiểm tra quyền Edit của tài khoản; không dùng quyền View thay cho Edit. Chọn ảnh không đòi quyền truy cập toàn bộ thư viện. Máy ảnh cần quyền CAMERA; có hướng dẫn mở Cài đặt nếu bị từ chối. Ảnh tối đa 5 MB, lần đầu lưu trực tiếp và các lần sau vẫn theo quy trình chờ duyệt của API.

## Firebase đã nối

File `D:\QL_NS\QuanLyNhanSu\HRMS.Mobile\google-services.json` đã kiểm tra: Firebase project `hrms-mobile-698a6`, Android package `com.qlns.hrmsmobile`. `app.json` trỏ tới file này; dự án Android hiện có đã thêm plugin Google Services của Expo SDK 57 và bản sao cấu hình vào `android/app/google-services.json`.

Không cần dán thêm đoạn Gradle trong hướng dẫn Firebase. Không cần thay Oracle bằng Firebase. Firebase ở đây phục vụ đăng ký thiết bị và chuyển thông báo Android.

## EAS đã liên kết; còn cần quyền gửi FCM V1

EAS projectId `2add3fd9-2af9-46e9-ad3c-045d38e5f4f7` do người dùng cung cấp đã được lưu vào `expo.extra.eas.projectId` trong `app.json`. Không cần chạy lại bước liên kết nếu đã đăng nhập đúng project.

1. Mở terminal tại `D:\QL_NS\QuanLyNhanSu\HRMS.Mobile`:

   ```powershell
   npx eas-cli login
   npx eas-cli init
   ```

   Đăng nhập Expo, chọn/tạo project HRMS. Project ID của Expo là UUID, khác `hrms-mobile-698a6` của Firebase. Nếu CLI báo không tự ghi được cấu hình động, lấy UUID mà CLI hiển thị và đặt vào `expo.extra.eas.projectId` trong `app.json`, hoặc biến `EXPO_PUBLIC_EAS_PROJECT_ID`. `app.config.js` đã hỗ trợ cả hai nguồn.

2. Firebase Console → Project settings → Service accounts → Generate new private key. Đây là khóa tài khoản dịch vụ để gửi FCM V1, **khác** `google-services.json` của Android. Lưu khóa riêng tại máy quản trị để tải lên EAS; không đưa khóa riêng vào mã Mobile hay APK.

3. Tại thư mục Mobile:

   ```powershell
   npx eas-cli credentials --platform android
   ```

   Chọn profile production → Google Service Account → Manage your Google Service Account Key for Push Notifications (FCM V1) → Upload a new service account key. Chọn file khóa riêng vừa tạo. Khóa này thuộc cùng Firebase project `hrms-mobile-698a6`.

4. Nếu build trên EAS, trong Expo Dashboard → Environment variables tạo biến **File** tên `GOOGLE_SERVICES_JSON`, tải `google-services.json` lên, áp dụng cho môi trường `preview` và `production`. File này đang được bỏ qua bởi Git, nên EAS cần biến File để nhận nó. Có thể đặt `EXPO_PUBLIC_EAS_PROJECT_ID` là UUID dạng Plain text nếu chưa lưu vào `app.json`.

5. Build bản APK thử push:

   ```powershell
   npx eas-cli build --platform android --profile push-preview
   ```

   Profile này kiểm tra thiếu projectId/file Firebase và dừng build với thông báo rõ. Bản phát hành dùng profile `production`. Cấu hình Android mới cần build/cài APK mới; reload JavaScript không bổ sung được Firebase hoặc quyền native.

## Cách người dùng cấp quyền trên điện thoại

Khi dùng bản cài HRMS có EAS: sau đăng nhập, app mời bật thông báo. Chỉ sau khi bấm **Bật thông báo** mới hiện hộp xin quyền của Android/iOS. Người dùng có thể chọn **Để sau**.

Có thể mở **Cá nhân → Cài đặt → Thông báo → Bật ngay**. Nếu hệ điều hành còn cho phép hỏi lại, app xin quyền; nếu đã từ chối vĩnh viễn, nút **Mở Cài đặt** đưa đến cài đặt ứng dụng. Quay lại app sẽ kiểm tra lại quyền và đồng bộ thiết bị. Chỉ báo kết nối push thành công khi có token, quyền được cấp, push được bật và API nhận đăng ký.

Đăng xuất sẽ ngừng listener, chờ thao tác đăng ký đang chạy rồi vô hiệu hóa thiết bị. Khi bấm thông báo, app mở chi tiết thông báo và API vẫn kiểm tra người nhận. Thông báo trong ứng dụng không cần quyền hiển thị của điện thoại; vẫn cập nhật khi mở app và mỗi 30 giây ở foreground.

Expo Go Android không hỗ trợ remote push, nên app không import phần push SDK trong Expo Go và không làm crash lúc khởi động. Cần bản cài HRMS riêng.

## Kiểm tra sau khi có đủ cấu hình

- Cài APK mới, đăng nhập tài khoản nhân viên, bật quyền và kiểm tra trạng thái đăng ký trong Cài đặt.
- Tạo/duyệt một yêu cầu thử cho đúng tài khoản qua PC/Web; kiểm tra thông báo trong app, badge, đọc/chưa đọc.
- Đưa app về nền, tạo thông báo tiếp theo; kiểm tra âm thanh/banner và bấm mở đúng thông báo.
- Kiểm tra khi màn hình khóa; kiểm tra đăng xuất không nhận tiếp thông báo của tài khoản trước.
- API trên VPS cần có cùng phiên bản với mã hiện tại, bao gồm đăng ký thiết bị và worker xử lý outbox. Gói DLL không ghi đè Web.config hay thông tin kết nối của VPS.

Hiện Firebase và EAS projectId đã nối; chưa xác nhận khóa FCM V1 được tải lên EAS. Vì vậy chưa thể xác nhận gửi push từ đầu đến cuối trên điện thoại thật.

Tài liệu chính thức: [Expo FCM V1](https://docs.expo.dev/push-notifications/fcm-credentials/), [Expo SDK 57 Notifications](https://docs.expo.dev/versions/v57.0.0/sdk/notifications/), [EAS Environment variables](https://docs.expo.dev/eas/environment-variables/).
