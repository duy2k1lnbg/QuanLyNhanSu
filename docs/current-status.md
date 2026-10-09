# Hiện trạng dự án

Cập nhật nội dung: 01/10/2026.

**Phạm vi:** Kết quả đã quan sát và nhận xét từ mã nguồn; không phải biên bản nghiệm thu.

## Cách đọc kết quả

Tài liệu này phân biệt kết quả lệnh đã chạy, nhận xét từ mã nguồn và các ghi chép cũ. Những kết quả dưới đây được quan sát trong phiên rà soát ngày 01/10/2026 trước khi viết lại tài liệu. Không chạy lại các phép tính lương hoặc test ghi database trong đợt chỉnh tài liệu này.

## Kiểm tra đã quan sát

| Kiểm tra | Kết quả | Giới hạn |
| --- | --- | --- |
| Build `HRMS.Api` bằng MSBuild | Thành công | Còn cảnh báo quyền ghi cache và xung đột `System.Memory`; không phải kiểm thử endpoint |
| Web `npm run test` | 7 nhóm kiểm tra đạt | Chủ yếu KPI, diễn giải ký hiệu công, intro và ngôn ngữ; không phải E2E |
| Web `npm run lint` | Mã thoát 0, có cảnh báo | Có cảnh báo React/hooks; mã thoát 0 không có nghĩa không còn vấn đề |
| Web `npm run build` | Build đạt với output riêng | `dist` mặc định gặp lỗi quyền truy cập; output kiểm tra nằm trong `artifacts/project-analysis-web-20261001` |
| Bundle JavaScript Web | Khoảng 2,27 MB, gzip khoảng 684 KB | Một bundle lớn; cần đánh giá tải trang trên thiết bị thực tế |
| Mobile `npm run test` | 8 test đạt | Kiểm tra ngôn ngữ và tiện ích; chưa xác minh ứng dụng trên thiết bị |
| Mobile TypeScript `--noEmit` | Thành công | Không thay cho kiểm thử chức năng |
| `Build-IsolatedTests.ps1 -OnlyPure` | Lỗi biên dịch | Có tham chiếu thiếu; suite còn chứa test dùng database |

## Điểm còn mở trong mã nguồn

| Mức ưu tiên | Điểm cần xử lý | Nguồn và căn cứ |
| --- | --- | --- |
| P0 | GET AI chat cho phép ẩn danh và có đường đọc dữ liệu nhân viên | [AiChatController](../HRMS.Api/Controllers/AiChatController.cs): `ChatGet` có `AllowAnonymous`, gọi `Chat`, fallback đọc các view AI |
| P1 | API danh sách lương và khóa/mở khóa kỳ thiếu kiểm tra quyền riêng | [BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs): các action này chỉ kế thừa kiểm tra đăng nhập; chưa lọc phạm vi công ty ở danh sách |
| P1 | Danh tính audit có thể bị request khác ghi đè | [MyEntities.Audit](../HRMS.DataAccess/MyEntities.Audit.cs) dùng thuộc tính `static`; filter API gán lại chúng |
| P1 | Lịch sử AI đang được chia sẻ trong process | [ChatboxManager](../HRMS.Business/Services/AI_Services/Chat/ChatboxManager.cs) giữ RAG tĩnh; dịch vụ đó có lịch sử hội thoại |
| P1 | Lọc đã chi trả chưa thống nhất với đã duyệt | [BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs) nhận `APPROVED` trong bộ lọc `paid`; V1_20 còn ghi là draft |
| P1 | Test độc lập chưa tách khỏi test database | [Script](../database/synthetic200/Build-IsolatedTests.ps1), [AttendanceRegressionTests](../HRMS.Tests/AttendanceRegressionTests.cs) |
| P2 | Cấu hình CORS/chi tiết lỗi cần đối chiếu môi trường | [CorsHandler](../HRMS.Api/App_Start/CorsHandler.cs), [WebApiConfig](../HRMS.Api/App_Start/WebApiConfig.cs): CORS wildcard và chi tiết lỗi Always |
| P2 | Cấu hình khởi tạo Docker cần rà soát | [Compose](../docker-compose.yml) mount cả thư mục có rollback vào init directory; API chạy riêng |

Đây là nhận xét từ mã nguồn, chưa phải kết quả thử khai thác trên môi trường triển khai. Mức ưu tiên dùng để sắp xếp công việc nội bộ. Các điểm đã khắc phục được ghi chú với ngày xác minh.

## Chưa xác minh trong phiên rà soát

- Build/chạy toàn bộ Desktop và các báo cáo DevExpress.
- Ứng dụng Mobile trên Android/iOS, đăng nhập và các luồng phê duyệt trên thiết bị.
- Test tích hợp và E2E với schema Oracle riêng.
- Phiên bản migration đã áp dụng trên database đang sử dụng.
- Toàn bộ công thức lương và giá trị chính sách theo quy định áp dụng của đơn vị.
- Xác nhận thanh toán thực tế, cấu hình IIS/TLS và khôi phục backup trên máy triển khai.

## Tình trạng tài liệu

`docs` có 11 tài liệu Markdown theo chủ đề. Các hướng dẫn Mobile, Web, công–lương và kiểm thử đã được gộp; báo cáo trùng và ghi chép cũ được sao lưu cục bộ trước khi bỏ khỏi `docs`. Prompt nằm riêng trong `prompts/` và bị Git bỏ qua. Snapshot/backup dữ liệu nằm dưới `database/`; JSON được giữ nguyên khi di chuyển.

Trong đợt dọn tài liệu chỉ kiểm tra đường dẫn, định dạng, bản sao và quy tắc Git ignore. Không chạy lại build/test ứng dụng, SQL, seed hoặc tính lương. Kết quả ở bảng đầu là kết quả của phiên rà soát trước đó.
