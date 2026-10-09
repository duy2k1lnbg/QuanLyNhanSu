# Tài liệu HRMS

Cập nhật: 01/10/2026. Tài liệu được gộp theo chủ đề để giảm các bản trùng và dễ tra cứu.

## Bắt đầu

1. [README dự án](../README.md): thành phần và cách bắt đầu.
2. [Cài đặt](installation.md): môi trường, cấu hình và URL cục bộ.
3. [Kiến trúc](architecture.md): luồng truy cập và nguồn mã chính.
4. [Hiện trạng](current-status.md): kết quả đã quan sát, giới hạn và việc còn mở.

## Hướng dẫn theo chủ đề

| Tài liệu | Nội dung |
| --- | --- |
| [Triển khai](deployment.md) | IIS/API, Web, cấu hình và các bước cần kiểm tra |
| [Mobile](mobile.md) | Kiến trúc, API, chạy/build, mapping và phạm vi dữ liệu |
| [Công–lương](payroll.md) | Luồng tính, dữ liệu, trạng thái/quyền, chính sách và đối soát giao diện |
| [Database](database.md) | Migration, kiểm tra dữ liệu và các bộ dữ liệu mô phỏng |
| [Web](web.md) | Theme/component, KPI, ký hiệu công và intro |
| [Kiểm thử](testing.md) | Nguồn test, kịch bản cần chạy và bằng chứng cần ghi |
| [Thuộc tính model](model-fields.md) | Danh mục scalar C#; không thay metadata Oracle hoặc kết quả coverage UI |

Script và snapshot dữ liệu nằm trong `database/`.

## Quy ước cập nhật

- Sửa tài liệu chính của chủ đề thay vì thêm báo cáo tổng kết trùng nội dung.
- Kết quả chạy cần ghi ngày, source, lệnh, môi trường và giới hạn; tên test hoặc migration không chứng minh đã chạy.
- Phân biệt mô tả theo source, kết quả đã quan sát, kế hoạch và dữ liệu lịch sử.
- Chỉ thêm tài liệu khi có một chủ đề mới cần hướng dẫn riêng.

Prompt được lưu cục bộ trong `prompts/` ở gốc repository và bị Git bỏ qua. Các báo cáo cũ được sao lưu trong `artifacts/docs-maintenance/before-compaction-*.zip` trước khi bỏ khỏi bộ tài liệu chính; bản sao này cũng bị Git bỏ qua.
