# HRMS.Web

Giao diện Web của dự án quản lý nhân sự, dùng React, TypeScript, Vite và Ant Design. Ứng dụng gọi `HRMS.Api` để đọc và cập nhật dữ liệu; không kết nối trực tiếp Oracle.

Cập nhật nội dung: 01/10/2026.

## Chạy cục bộ

Build/chạy backend trước theo [hướng dẫn cài đặt](../docs/installation.md). Vite proxy mặc định tới `http://localhost:55463`.

```powershell
npm ci
npm run dev
```

Vite dùng cổng `5173`. `VITE_API_BASE_URL` trong `.env` được ưu tiên hơn proxy mặc định. File `.env.example` hiện dùng cổng `5000`; kiểm tra cấu hình backend trước khi sao chép.

## Lệnh kiểm tra

```powershell
npm run test
npm run lint
npm run build
```

Test hiện tập trung vào KPI/chấm công và intro/ngôn ngữ, chưa phải bộ kiểm thử giao diện E2E. Lint còn cảnh báo. Build đã được kiểm tra với output riêng do lỗi quyền truy cập ở `dist` hiện tại; xem [hiện trạng](../docs/current-status.md).

## Nguồn mã

- `src/App.tsx`: trạng thái ứng dụng, tải dữ liệu và điều hướng.
- `src/pages`, `src/components`: trang và thành phần giao diện.
- `src/services/api.ts`: Axios, token và URL API.
- `src/utils/permissionUtils.ts`: quyền ở giao diện; server vẫn cần kiểm tra quyền riêng.
- `src/theme`, `src/locales`: theme và năm ngôn ngữ.
- `tests`: kiểm tra KPI/chấm công và welcome intro.

## Khi triển khai

Đặt `VITE_API_BASE_URL` theo cách bố trí IIS/API trước lúc build. Không coi cơ chế đổi `/api` ↔ `/api/api` sau 404 là cách cấu hình chính. Docker Web dùng Nginx và gọi API chạy riêng trên host.

Xem [tài liệu triển khai](../docs/deployment.md) và [ghi chú giao diện](../docs/web.md). `build:vps` gọi script đóng gói có thao tác file ngoài thư mục Web; đọc script trước khi chạy.
