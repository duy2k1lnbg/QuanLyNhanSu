# Mobile: kiến trúc, API và cách chạy

Cập nhật nội dung: 01/10/2026.

Hướng dẫn theo mã nguồn hiện có. Mobile dùng API để tra cứu cá nhân, gửi yêu cầu và phê duyệt theo quyền. Kịch bản kiểm thử nằm trong [kiểm thử](testing.md); kết quả đã quan sát nằm trong [hiện trạng](current-status.md).

## Kiến trúc và phiên đăng nhập

### Phạm vi

Mobile dùng Expo 57, React Native 0.86 và TypeScript. Các màn hình hiện có gồm đăng nhập, hồ sơ, chấm công, lương, hợp đồng, bảo hiểm, thông báo, yêu cầu nghỉ phép/điều chỉnh công/tăng ca và phê duyệt cho quản lý. Quyền cho màn hình quản lý cần được kiểm tra tại API.

### Nguồn mã

| Phần | Nguồn |
| --- | --- |
| Phiên đăng nhập | [AuthContext](../HRMS.Mobile/src/auth/AuthContext.tsx) |
| API client | [client.ts](../HRMS.Mobile/src/api/client.ts), [endpoints.ts](../HRMS.Mobile/src/api/endpoints.ts) |
| URL | [config/index.ts](../HRMS.Mobile/src/config/index.ts) |
| Điều hướng | [RootNavigator](../HRMS.Mobile/src/navigation/RootNavigator.tsx), [MainTabNavigator](../HRMS.Mobile/src/navigation/MainTabNavigator.tsx) |
| Lưu token/tùy chọn | [storage.ts](../HRMS.Mobile/src/utils/storage.ts) |
| Phê duyệt | [ManagerApprovalsScreen](../HRMS.Mobile/src/screens/Approvals/ManagerApprovalsScreen.tsx) |

### Phiên và dữ liệu

Token được lưu bằng Expo SecureStore, tùy chọn ngôn ngữ/theme bằng AsyncStorage. Khi khôi phục phiên, ứng dụng đọc token rồi gọi `/me`. API client gắn Bearer token, thông tin thiết bị và xử lý lỗi 401.

Các API `/me` phân giải hồ sơ từ người dùng đã xác thực và bảng liên kết nhân viên. Nhóm `/approvals` có phạm vi khác; không suy rằng nó chỉ trả về dữ liệu cá nhân. Xem [API](mobile.md) và [bảo mật](mobile.md).

### Giới hạn xác minh

TypeScript và nhóm test tiện ích/ngôn ngữ đã đạt trong lần rà soát trước. Chưa kiểm tra luồng sử dụng trên thiết bị, mất mạng, điều hướng sau thu hồi phiên hoặc phê duyệt đồng thời. URL mặc định trong source hiện trỏ tới môi trường triển khai và dùng HTTP; cần chọn URL phù hợp trước khi thử dữ liệu riêng.

## Các nhóm API

### Cách xác định URL

Các đường dẫn dưới đây có prefix `/api` khi API nằm ở root. IIS sub-application có thể thêm một cấp nữa. Đối chiếu [endpoints.ts](../HRMS.Mobile/src/api/endpoints.ts), [API client](../HRMS.Mobile/src/api/client.ts) và [cấu hình](../HRMS.Mobile/src/config/index.ts).

Request cần xác thực gửi `Authorization: Bearer <token>`. Client hiện gắn `X-Client-Type: MOBILE` cùng một số header nền tảng/thiết bị. Payload và tên trường trả về phải đối chiếu DTO hiện tại; tài liệu không tạo response mẫu để thay hợp đồng API.

### Các nhóm endpoint

| Nhóm | Đường dẫn được client sử dụng | Nguồn |
| --- | --- | --- |
| Đăng nhập/phiên | `/api/auth/login`, `change-password`, `logout`, `logout-all` | [authApi](../HRMS.Mobile/src/api/authApi.ts), [AuthController](../HRMS.Api/Controllers/AuthController.cs) |
| Người dùng và hồ sơ | `/api/me`, `/api/me/dashboard`, `/api/me/profile` | [meApi](../HRMS.Mobile/src/api/meApi.ts), [MeController](../HRMS.Api/Controllers/MeController.cs) |
| Tra cứu | `/api/me/attendance`, `payroll`, `contract`, `insurance` | `meApi` và DTO hiện tại |
| Thông báo | `/api/me/notifications`, `/api/me/notifications/{id}` | `meApi` |
| Nghỉ phép | `/api/me/leave`, `/api/me/leave-balance` | `meApi` |
| Điều chỉnh công | `/api/me/attendance-corrections` | `meApi` |
| Tăng ca | `/api/me/overtime`, `/api/me/overtime/{id}`, `/api/me/overtime/{id}/cancel` | `meApi` |
| Tổng hợp yêu cầu | `/api/me/requests` | `meApi` |
| Phê duyệt | `/api/approvals/summary`, `leave`, `attendance-corrections`, `overtime`, `insurance-movements` và các action approve/reject | [approvalsApi](../HRMS.Mobile/src/api/approvalsApi.ts), [ApprovalController](../HRMS.Api/Controllers/ApprovalController.cs) |

Các mục ngắn trong cùng ô kế thừa prefix của đường dẫn đầu. Đây là danh mục đường dẫn, không tuyên bố mọi method/action đều được phép cho cùng một vai trò. Xem client/controller để xác định GET/POST/PUT và body của từng action.

### Phạm vi người dùng

`/me` phân giải nhân viên qua tài khoản đã xác thực và mapping hiện có. `/approvals` phục vụ phạm vi quản lý; cần kiểm tra quyền riêng. Trạng thái tài khoản, liên kết nhân viên và phiên có thể làm request bị từ chối.

Không tự thêm mã nhân viên vào request để mở rộng phạm vi tra cứu. Không coi token Mobile là bằng chứng tài khoản có quyền quản lý.

### Kiểm tra hợp đồng API

Ghi method, URL, body đã che dữ liệu riêng, mã HTTP, DTO và tài khoản thử nghiệm. Đối chiếu request thực tế khi chạy trực tiếp API với khi đi qua `proxy.js`, vì proxy có biến đổi response. Chưa kiểm thử lại tất cả endpoint trên thiết bị trong đợt cập nhật tài liệu này.

## Chạy và build

### Chuẩn bị

- Dùng phiên bản package trong `HRMS.Mobile/package.json` và lockfile, không tự nâng Expo/React Native trong công việc tài liệu.
- Chuẩn bị Node/npm phù hợp. Build Android cần toolchain Android; build iOS cần môi trường tương ứng. Các toolchain này chưa được kiểm tra trong lần rà soát.
- Build/chạy API và chuẩn bị tài khoản liên kết hồ sơ nhân viên trên schema kiểm thử.

### Chọn URL API

Kiểm tra [config/index.ts](../HRMS.Mobile/src/config/index.ts). Cấu hình hiện có URL triển khai, URL IP dự phòng, emulator và LAN; việc các hằng số tồn tại không có nghĩa ứng dụng sẽ tự chọn đúng môi trường. `APP_CONFIG.apiBaseUrl` hiện ưu tiên URL triển khai.

Nếu dùng `start-api-service.bat`, API chạy tại cổng `5001` và proxy tại `5000`. Android emulator thường cần địa chỉ host phù hợp; thiết bị thật cần IP LAN có thể truy cập. Không dùng `localhost` của điện thoại để chỉ máy tính chạy API.

Không ghi token/mật khẩu thật vào file cấu hình hoặc tài liệu. Kiểm tra HTTPS và quy tắc truy cập mạng của bản build trước khi dùng dữ liệu riêng.

### Lệnh trong package

```powershell
cd HRMS.Mobile
npm ci
npm run start
```

```powershell
npm run android
npm run ios
```

Lệnh thứ hai/ba phụ thuộc toolchain của máy; chưa có kết quả build mới được ghi nhận trong tài liệu này. Không mặc định rằng một APK có sẵn trong thư mục đóng gói tương ứng source hiện tại.

### Kiểm tra trước khi phát hành

```powershell
npm run test
npx tsc --noEmit
```

Sau đó kiểm tra trên thiết bị: URL, đăng nhập, lưu token, đăng xuất, hết hạn phiên, hồ sơ, lương và yêu cầu/phê duyệt theo quyền. Ghi phiên bản source, thiết bị, bản build, cấu hình và lỗi còn mở.

Các lệnh package được đối chiếu với source; không coi chúng là quy trình phát hành Android/iOS đã được thử từ đầu. Xem [kế hoạch kiểm thử](testing.md).

## Dữ liệu và mapping

### Nguồn dữ liệu

Mobile dùng API, không kết nối trực tiếp Oracle. `MeController` phân giải tài khoản đã xác thực, đọc liên kết nhân viên và dùng các bảng nghiệp vụ để trả thông tin cá nhân.

| Nhóm | Bảng/model cần đối chiếu |
| --- | --- |
| Tài khoản và hồ sơ | `TB_SYS_USER`, `TB_NHANVIEN`, `TB_USER_EMPLOYEE_MAPPING` |
| Phiên/bảo mật | `TB_AUTH_SESSION`, `TB_AUTH_POLICY`, `TB_AUTH_LOGIN_ATTEMPT`, `TB_AUTH_AUDIT` |
| Yêu cầu | `TB_YEUCAU_NGHIPHEP`, `TB_YEUCAU_DIEUCHINHCONG`, `TB_YEUCAU_TANGCA` |
| Thông tin tra cứu | Kỳ công, bảng công, bảng lương, hợp đồng, bảo hiểm, thông báo |

Các file entity không thay cho việc kiểm tra bảng/cột thực tế trên schema chạy ứng dụng.

### Migration liên quan

- V1_12: liên kết tài khoản/nhân viên cho Mobile.
- V1_13: mapping và các yêu cầu.
- V1_14: kiểm soát workflow tăng ca/yêu cầu.
- V1_15: phiên, chính sách và audit xác thực.
- Các migration công/lương sau đó có thể ảnh hưởng response Mobile.

Xem [thư mục migration](../database/migrations) và [runbook](database.md). Không tự áp dụng rollback hoặc seed khi chỉ cần kiểm tra liên kết.

### Kiểm tra trên schema thử nghiệm

- Một tài khoản liên kết đúng hồ sơ và trạng thái bật/tắt Mobile được phản ánh như mong đợi.
- Tài khoản chưa liên kết bị từ chối ở endpoint yêu cầu hồ sơ.
- Quyền đọc/sửa yêu cầu được kiểm tra cùng với người tạo và trạng thái.
- Khóa ngoại và dữ liệu soft-delete phù hợp với controller đang chạy.

Chưa xác nhận database đang sử dụng đã áp dụng đầy đủ các migration được liệt kê.

## Xác thực và phạm vi

### Cơ chế hiện có

- Mật khẩu được xử lý bằng BCrypt trong backend.
- API nhận JWT, kiểm tra phiên server và security version qua `AuthSecurityService`.
- Mobile lưu token trong SecureStore và gắn token khi gọi API.
- `/me` phân giải người dùng rồi hồ sơ nhân viên, có kiểm tra vô hiệu hóa và liên kết tài khoản.
- Đăng xuất/đổi mật khẩu và quản lý phiên có endpoint trong `AuthController`.

Đây là mô tả cơ chế trong mã nguồn, không phải kết luận về mức độ an toàn của mọi endpoint.

### Phạm vi cần kiểm tra

1. Tài khoản A không đọc hoặc sửa dữ liệu cá nhân của B qua `/me`.
2. Client không thay đổi phạm vi bằng cách gửi mã nhân viên khác trong query/body.
3. Tài khoản nhân viên không được dùng các endpoint quản lý lương hoặc phê duyệt ngoài quyền.
4. Khi khóa tài khoản, thu hồi phiên hoặc đổi mật khẩu, token cũ bị từ chối theo chính sách.
5. Đơn nghỉ phép/tăng ca/điều chỉnh công chỉ được hủy hoặc phê duyệt theo quyền/trạng thái phù hợp.
6. Dùng HTTPS và kiểm tra URL dự phòng; cấu hình Mobile hiện có URL HTTP.

Không dùng việc menu bị ẩn làm bằng chứng API đã từ chối truy cập. Cần thử gọi endpoint trực tiếp bằng tài khoản có quyền hạn khác nhau.

### Điểm liên quan còn mở

Theo [hiện trạng](current-status.md), API lương thiếu một số kiểm tra quyền riêng và GET AI chat có đường truy cập ẩn danh. Những endpoint này cũng cần được thử với token Mobile. Cơ chế `/me` không tự bảo vệ các controller khác.

Xem [MeController](../HRMS.Api/Controllers/MeController.cs), [JwtAuthorizeAttribute](../HRMS.Api/Filters/JwtAuthorizeAttribute.cs), [AuthSecurityService](../HRMS.Business/CLASS_SECURITY/AuthSecurityService.cs) và [kế hoạch test](testing.md).
