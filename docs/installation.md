# Cài đặt và chạy dự án cục bộ

Cập nhật nội dung: 01/10/2026.

**Phạm vi:** Tài liệu tham khảo theo mã nguồn hiện có.

## Chuẩn bị

| Thành phần | Cần cho |
| --- | --- |
| Windows, Visual Studio/MSBuild và .NET Framework 4.7.2 Developer Pack | API, Business, DataAccess và Desktop |
| DevExpress 24.1 theo tham chiếu project | Build/chạy Desktop và báo cáo |
| Oracle có schema phù hợp | Các chức năng dữ liệu |
| Node.js/npm tương thích package đã khóa | Web/Mobile |
| Ollama, Qdrant | Các luồng AI có sử dụng dịch vụ này |

Vite cài trong workspace khai báo Node `^20.19.0 || >=22.12.0`. Kiểm tra thêm yêu cầu package Mobile trước khi chọn runtime chung. Không suy từ việc API dùng .NET Framework rằng `dotnet run` sẽ khởi động được API này.

## Database

1. Xác định host, port, service name và schema thực tế. Service name Oracle không được suy từ tên thư mục hay giá trị mẫu `xe`.
2. Sao lưu cấu hình hiện có trước khi chỉnh connection string.
3. Đối chiếu [migration runbook](database.md). Repository có V1_0 đến V1_20, nhưng không có xác nhận trong tài liệu này rằng tất cả đã được áp dụng.
4. Dùng schema riêng cho test. Không tự chạy seed/cutover hoặc toàn bộ thư mục migration trên dữ liệu đang dùng.

`docker-compose.yml` có service Oracle thử nghiệm. Nó mount cả thư mục migration vào thư mục init; cần tách forward/rollback/draft trước khi dùng cơ chế đó. Việc tạo `.env` không tự cấu hình các connection string của ứng dụng .NET.

## Backend

- Mở `HRMS.sln` tại gốc repository, restore NuGet khi cần.
- Cấu hình connection string trong `HRMS.Api/Web.config` theo `MyEntities` và `AiEntities`. Không tự thay metadata EDMX bằng chuỗi ví dụ rút gọn.
- Desktop/console/test có file cấu hình riêng; kiểm tra chúng khi chạy thành phần tương ứng.
- Build `HRMS.Api` bằng Visual Studio hoặc MSBuild của Visual Studio.
- Chạy `start_local_backend.bat`: IIS Express được gọi với `/path:...HRMS.Api` và `/port:55463`.

Đường dẫn cài IIS Express/MSBuild khác nhau theo máy; sửa lệnh cục bộ cho đúng môi trường.

## Web

```powershell
cd HRMS.Web
npm ci
npm run dev
```

Vite mở cổng `5173`, proxy `/api` tới `http://localhost:55463`. Kiểm tra `HRMS.Web/.env` nếu request đi sang cổng khác: `VITE_API_BASE_URL` được ưu tiên và file mẫu hiện đặt `http://localhost:5000/api`.

Để dùng proxy mặc định, bỏ override URL không phù hợp hoặc đặt `VITE_API_BASE_URL=/api` trong cấu hình cục bộ. Khởi động lại Vite sau khi đổi biến môi trường.

## Mobile

Xem [build Mobile](mobile.md). `start-api-service.bat` là cách chạy khác: IIS Express cổng `5001` cùng `proxy.js` ở cổng `5000`. Proxy còn biến đổi response cho Mobile; không xem hai cách chạy là tương đương nếu chưa thử endpoint.

## Kiểm tra tối thiểu

- Web tải được trang và request đến đúng API.
- Đăng nhập bằng tài khoản được cấp trên database kiểm thử; không dùng tài khoản/mật khẩu viết trong tài liệu cũ.
- Kiểm tra dữ liệu cá nhân, quyền xem và trường hợp bị từ chối truy cập.
- Chạy các lệnh kiểm tra frontend trong README. Không chạy mọi test backend trước khi tách nhóm có ghi database.

Kết quả build/test đã quan sát nằm trong [hiện trạng](current-status.md). Hướng dẫn này chưa được thử từ đầu trên một máy mới.
