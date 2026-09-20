# HRMS Mobile v1 Quality Assurance & Test Plan

## 1. Mục tiêu kiểm thử

Kế hoạch kiểm thử nhằm xác minh toàn bộ các tiêu chuẩn an toàn, độ tin cậy và sự nhất quán giữa **HRMS_API** (.NET 4.7.2 / Oracle) và **HRMS Mobile** (React Native / Android).

---

## 2. Kịch bản kiểm thử Tự động Backend (Automated Backend Tests)

Các test case được triển khai bằng **NUnit 3** trong dự án `Bu.Tests/MobileSecurityAndApiTests.cs`, kết nối trực tiếp vào cơ sở dữ liệu Oracle:

| Mã kiểm thử | Tên test case | Mục đích | Kết quả kỳ vọng |
| :--- | :--- | :--- | :--- |
| **SEC-01** | `Mobile_Login_With_Valid_Credentials_Returns_Token_With_Manv_And_ClientType` | Kiểm tra JWT Claims | Trả về 200, JWT chứa đúng `id_user`, `manv`, `client_type=MOBILE` |
| **SEC-02** | `Mobile_Login_Rejects_Invalid_Signature_Token` | Kiểm tra tính toàn vẹn chữ ký | Token bị sửa payload hoặc sai Secret bị từ chối xác thực |
| **SEC-03** | `Mobile_Login_Rejects_User_Without_Linked_Employee` | Kiểm tra yêu cầu Requirement 56 | Bị từ chối kèm thông báo thân thiện: *"Tài khoản chưa được liên kết với hồ sơ nhân viên. Vui lòng liên hệ bộ phận nhân sự."* |
| **SEC-04** | `Mobile_Login_Rejects_Desktop_Only_Account` | Kiểm tra hạn chế nền tảng | Tài khoản có `CLIENT_TYPE == "DESKTOP"` không được phép đăng nhập trên Mobile |
| **SEC-05** | `Mobile_Login_Rejects_Disabled_Account` | Kiểm tra tài khoản bị khóa | Tài khoản có `IS_DISABLED == 1` bị từ chối đăng nhập |
| **SEC-06** | `SelfScope_MeController_Extracts_Manv_From_Token_Only_And_Prevents_IDOR` | Phòng chống IDOR tuyệt đối | Bất kể query/body gửi gì, controller chỉ lấy dữ liệu của `MANV` trích xuất từ JWT |
| **SEC-07** | `Mobile_Role_Has_Read_Only_Self_Service_Permissions_Only` | Kiểm tra đặc quyền tối thiểu | Quyền của user Mobile chỉ gồm `MOBILE_*_VIEW`, không có quyền Admin/HR sửa dữ liệu |
| **AUTH-01** | `Mobile_ChangePassword_Validates_Old_Password_And_Updates_BCrypt_Hash` | Kiểm tra đổi mật khẩu | Mật khẩu cũ được kiểm tra qua BCrypt; đổi thành công cập nhật hash mới |

### Kết quả chạy kiểm thử Backend thực tế:
- **Lệnh thực thi**: `dotnet test Bu.Tests\Bu.Tests.csproj`
- **Kết quả**: **101 passed, 0 failed, 0 skipped** (Bao gồm 94 test nghiệp vụ cốt lõi và 7 test bảo mật Mobile chuyên sâu).

---

## 3. Kịch bản kiểm thử Tự động Mobile (Automated Mobile Tests)

Triển khai tại `HRMS.Mobile/test/unit.test.mjs` chạy qua Node.js Test Runner:

| Mã kiểm thử | Nội dung kiểm thử | Kết quả |
| :--- | :--- | :--- |
| **UI-I18N-01** | Kiểm tra độ đồng nhất key dịch giữa `vi.json` và `ja.json` | **PASS** (100% khớp key) |
| **UI-I18N-02** | Kiểm tra độ đồng nhất key dịch giữa `vi.json` và `en.json` | **PASS** (100% khớp key) |
| **UI-I18N-03** | Đảm bảo không có chuỗi dịch nào bị rỗng trong cả 3 ngôn ngữ | **PASS** |
| **UI-FMT-01** | Định dạng tiền tệ VND chính xác theo locale (`vi`, `ja`, `en`) | **PASS** |
| **UI-FMT-02** | Định dạng ngày tháng chuẩn hóa theo chuẩn quốc tế và Việt Nam | **PASS** |
| **UI-SYS-01** | Fallback ngôn ngữ hệ thống thiết bị về Tiếng Việt khi gặp locale lạ | **PASS** |

### Kết quả chạy kiểm thử Mobile thực tế:
- **Lệnh thực thi**: `node --test test/unit.test.mjs`
- **Kết quả**: **6 passed, 0 failed** (100% tỷ lệ thành công).

---

## 4. Kịch bản kiểm thử Giao diện & Trải nghiệm người dùng (Manual UX Test Cases)

1. **Khởi động ứng dụng lần đầu**:
   - Hiển thị SplashScreen kèm Logo HRMS chuyên nghiệp.
   - Tự động chuyển hướng vào `LanguageSelectScreen` để chọn ngôn ngữ.
   - Nhấn "Tiếp tục" chuyển đến màn hình Đăng nhập.
2. **Đăng nhập**:
   - Nhập tên đăng nhập/mã nhân viên và mật khẩu.
   - Bấm icon 👁 để xem/ẩn mật khẩu an toàn.
   - Thử nhập sai mật khẩu: Form giữ nguyên tên đăng nhập, xóa mật khẩu và focus lại ô mật khẩu.
3. **Trang chủ & Lối tắt**:
   - Hiển thị Avatar (ảnh hoặc viết tắt), thông tin nhân viên, thẻ tóm tắt chấm công & lương.
   - Kéo xuống (Pull-to-refresh) để cập nhật dữ liệu mới nhất.
   - Nhấn các thẻ lối tắt chuyển đúng đến màn hình tương ứng.
4. **Chấm công & Bảng lương**:
   - Điều hướng chuyển đổi tháng bằng nút mũi tên `[ < ] Tháng XX/XXXX [ > ]`.
   - Xem chi tiết từng ngày công và phân tích các khoản thu nhập / khấu trừ.
5. **Cài đặt & Giao diện**:
   - Chuyển đổi qua lại giữa Sáng / Tối / Theo hệ thống. Toàn bộ các thành phần UI cập nhật màu sắc ngay lập tức.
   - Đổi ngôn ngữ giữa Tiếng Việt, 日本語, English: Giao diện dịch 100%, không bị lẫn tiếng.
6. **Đăng xuất**:
   - Hiển thị hộp thoại xác nhận. Khi xác nhận, token trong SecureStore được xóa sạch, giữ lại cài đặt ngôn ngữ và theme, quay về màn hình Đăng nhập.
