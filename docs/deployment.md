# Ghi chú triển khai API và Web

Cập nhật nội dung: 01/10/2026.

**Phạm vi:** Tài liệu tham khảo theo mã nguồn hiện có.

## Phạm vi

API .NET Framework chạy qua IIS/IIS Express trên Windows. Web là các file tĩnh sau build. Oracle và các dịch vụ AI cần cấu hình riêng. Tài liệu mô tả cách bố trí hiện có, chưa xác nhận một bản triển khai mới đã hoàn tất.

## Trước khi đóng gói

1. Chốt phiên bản mã nguồn cần triển khai, ghi lại commit và các thay đổi chưa commit liên quan.
2. Kiểm tra các điểm còn mở trong [hiện trạng](current-status.md), nhất là quyền AI/lương.
3. Xác định schema và migration cần áp dụng; chuẩn bị backup và thử khôi phục ở môi trường riêng.
4. Xác định API nằm ở root hay IIS sub-application, URL công khai và biến môi trường frontend.
5. Build API, Web; lưu log, mã thoát và cảnh báo. Không đồng nhất build đạt với nghiệm thu nghiệp vụ.

## Backend trên IIS

- Project cần build/publish là `HRMS.Api/HRMS.Api.csproj`, solution là `HRMS.sln`.
- Chuẩn bị IIS và ASP.NET .NET Framework phù hợp. Application Pool dùng CLR v4 và pipeline Integrated.
- Publish bằng công cụ của Visual Studio/MSBuild theo profile được chuẩn bị cho máy triển khai. Tài liệu không giả định rằng một profile cụ thể đã tồn tại.
- Cấu hình connection string, `HRMS_JWT_SECRET` hoặc `JwtSecret`, quyền đọc/ghi thư mục và DLL phụ thuộc.
- Đặt logging và chi tiết lỗi phù hợp với môi trường. Mã hiện có dùng `IncludeErrorDetailPolicy.Always`; đây là điểm còn cần xử lý, không phải đã được tắt.

## Web và URL API

```powershell
cd HRMS.Web
npm ci
npm run build
```

`VITE_API_BASE_URL` được đưa vào lúc build. Ví dụ đường dẫn là `/api` khi API được đặt ở root với route prefix `api`, hoặc `/api/api` khi IIS thêm sub-application `/api`. Kiểm tra request thực tế trước khi chọn giá trị.

API client hiện thử đổi hai base URL sau 404. Cơ chế này không thay cho cấu hình URL đúng, vì 404 cũng có thể là một bản ghi không tồn tại.

Với Nginx Docker, [nginx.conf](../HRMS.Web/nginx.conf) proxy tới `host.docker.internal:5000`. Khả năng truy cập host cần xác minh trên máy Docker cụ thể. Compose không có container API.

## Script đóng gói hiện có

[build_deploy.ps1](../build_deploy.ps1) chứa đường dẫn cố định `D:\QL_NS`, copy output, dọn assets và tạo gói `deploy_vps`. Đọc các thao tác file trước khi chạy. Gói Desktop/APK có sẵn chưa được xem là kết quả build mới của source hiện tại.

## Kiểm tra sau triển khai

- URL Web/API, đăng nhập/đăng xuất, token hết hạn và phiên bị thu hồi.
- Quyền xem/sửa, phạm vi công ty và dữ liệu cá nhân trên từng endpoint cần sử dụng.
- Luồng chấm công/lương/phê duyệt với dữ liệu kiểm thử đã định nghĩa và kết quả mong đợi.
- Ghi audit đúng người, không trộn lịch sử chat giữa tài khoản.
- HTTPS, CORS với các security header mà client gửi; Mobile đang có URL HTTP trong cấu hình mặc định cần đối chiếu.
- Log lỗi và phương án phục hồi phiên bản ứng dụng/database.

Ghi lại những bước chưa thực hiện. Chưa có căn cứ trong lần cập nhật tài liệu này để xác nhận môi trường triển khai đáp ứng hết danh sách trên.
