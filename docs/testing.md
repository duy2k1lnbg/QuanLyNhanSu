# Kiểm thử và cách ghi nhận kết quả

Cập nhật nội dung: 01/10/2026.

Kết quả build/test đã quan sát ngày 01/10/2026 được tập trung trong [hiện trạng](current-status.md). Các bảng kịch bản dưới đây là phần cần thực hiện, không phải danh sách đã nghiệm thu.

## Backend công–lương

### Kết quả đã quan sát ngày 01/10/2026

- API/Business/DataAccess build được, còn cảnh báo.
- Web có 7 nhóm kiểm tra đạt; phần KPI/ký hiệu công không thay kiểm thử công thức lương backend.
- Script `Build-IsolatedTests.ps1 -OnlyPure` lỗi biên dịch do thiếu tham chiếu các lớp mà `AttendanceRegressionTests` dùng.
- Chưa chạy toàn bộ test tính lương, test tích hợp hoặc tính lại một kỳ trên Oracle trong phiên rà soát đó.

Xem [hiện trạng](current-status.md). Các con số và log test trong báo cáo trước không được dùng làm kết quả chạy mới.

### Nguồn test hiện có

| Nhóm | Nguồn |
| --- | --- |
| Lương/chính sách | [PayrollEngineProductionTests](../HRMS.Tests/PayrollEngineProductionTests.cs), [VietnamPayrollRemediationGoldenTests](../HRMS.Tests/VietnamPayrollRemediationGoldenTests.cs) |
| Giao dịch | [TransactionalIntegrityTests](../HRMS.Tests/TransactionalIntegrityTests.cs) |
| Phân đoạn công | [TimeSegmentationEngineTests](../HRMS.Tests/TimeSegmentationEngineTests.cs) |
| Hồi quy công và dữ liệu | [AttendanceRegressionTests](../HRMS.Tests/AttendanceRegressionTests.cs) |
| Runner hỗ trợ | [synthetic200](../database/synthetic200) |

Tên test, assertion hoặc runner không chứng minh test đã chạy. Một số fixture chứa thao tác database; phân loại bằng nội dung trước khi thực thi.

### Việc cần làm trước khi chạy lại

1. Tách test độc lập khỏi test kết nối/ghi database, gắn nhóm hoặc runner rõ ràng.
2. Build API trước vì project test tham chiếu DLL API.
3. Dùng schema riêng, dữ liệu đầu vào và chính sách cố định đã được xác nhận cho fixture.
4. Chọn test có giá trị: nghỉ hưởng lương, lịch vượt/thiếu công chuẩn, làm đêm, OT, đối tượng tham gia bảo hiểm, người phụ thuộc, tạm ứng đã hủy và kỳ khóa.
5. Thử tính lại/lỗi giữa chừng/cạnh tranh giao dịch và đối chiếu bản ghi trước/sau.
6. Lưu lệnh, mã thoát, source, schema, tổng test thực chạy, skipped/failed và log.

Không chạy `dotnet test` cho toàn project với cấu hình database đang dùng chỉ để lấy số test đạt. Script mang tên `OnlyPure` hiện chưa bảo đảm giới hạn đó.

## Mobile

### Kết quả hiện có

Trong phiên rà soát ngày 01/10/2026, `npm run test` đạt 8 test và TypeScript `--noEmit` đạt. Các test chủ yếu kiểm tra khóa ngôn ngữ/giá trị dịch và tiện ích. Không coi đó là bằng chứng các luồng đăng nhập, lương hoặc phê duyệt đã chạy đúng trên thiết bị.

### Chuẩn bị dữ liệu

Tạo schema thử nghiệm và tài khoản cho nhân viên A/B, quản lý trong/ngoài phạm vi và người quản trị. Ghi trước kết quả mong đợi. Không dùng tài khoản thật hoặc chạy test seed trên database đang dùng.

### Kịch bản cần thực hiện

| Nhóm | Kịch bản | Bằng chứng cần lưu |
| --- | --- | --- |
| Phiên | Đúng/sai mật khẩu, tài khoản khóa/chưa liên kết, hết hạn và thu hồi token | Mã HTTP, thông báo và trạng thái app |
| Phạm vi | A thử truy cập dữ liệu B; nhân viên thử gọi endpoint quản lý | Request trực tiếp và phản hồi server |
| Tra cứu | Hồ sơ, công, lương, hợp đồng, bảo hiểm | So sánh dữ liệu fixture với response/màn hình |
| Yêu cầu | Tạo/hủy nghỉ phép, tăng ca, điều chỉnh công theo trạng thái | Bản ghi trước/sau, người tạo và audit |
| Phê duyệt | Người đúng/sai quyền, duyệt lặp và thao tác đồng thời | Kết quả HTTP và trạng thái cuối trong DB |
| Thiết bị | Android/iOS dự kiến sử dụng, mạng chậm/mất mạng, mở lại app | Thiết bị, bản build, ảnh/log lỗi |
| Hiển thị | Năm ngôn ngữ, light/dark, số tiền/ngày và chữ dài | Danh sách màn hình đã kiểm tra |

### Cách ghi nhận

Mỗi lượt test ghi commit/source, schema fixture, thiết bị, lệnh, mã thoát và log. Dùng ba trạng thái: đã đạt với bằng chứng, chưa đạt, chưa thực hiện. Không đánh dấu đạt chỉ vì tên test hoặc màn hình tồn tại.

Trước khi chạy backend suite, đọc test để phân biệt kiểm tra độc lập và test thay đổi dữ liệu. Script `OnlyPure` hiện có lỗi và chứa test database; xem [hiện trạng](current-status.md).

## Giao diện Web

### Kết quả đã quan sát

Trong phiên rà soát ngày 01/10/2026, Web test đạt 7 nhóm, lint trả mã 0 với cảnh báo và build đạt với output riêng. Build mặc định gặp lỗi quyền truy cập thư mục `dist`. Các phép kiểm tra này không thay việc xem từng màn hình hoặc kiểm tra API với vai trò khác nhau.

Không lặp lại tuyên bố nghiệm thu mọi màn hình hoặc tỷ lệ tương phản của bản báo cáo trước khi chưa có lượt đo/kiểm tra mới.

### Danh sách cần kiểm tra trực quan/chức năng

| Nhóm | Nội dung |
| --- | --- |
| Layout | Sidebar/header, đường điều hướng, viewport hẹp, bảng cuộn |
| Theme | Light/dark, đọc số/text, focus/hover/disabled, reload |
| Dữ liệu | Trống/thiếu/trùng, tải chậm, lỗi API và nhiều dòng |
| Quyền | Mục menu/nút và kết quả server khi gọi trực tiếp |
| Nghiệp vụ | Kỳ công, tổng KPI, lương/chi trả và phê duyệt |
| Ngôn ngữ | Năm ngôn ngữ, chuỗi dài, số/ngày/tiền |
| In và chuyển động | Phiếu lương/print, intro, giảm chuyển động và scroll |

### Cách ghi kết quả

Ghi source, trình duyệt, viewport, theme/ngôn ngữ, tài khoản thử, dữ liệu fixture và màn hình đã kiểm tra. Chụp ảnh/log phù hợp, che dữ liệu riêng. Chia đạt/chưa đạt/chưa thực hiện; không tick toàn bộ danh mục từ kết quả build.

Xem [danh mục trang](current-status.md), [token/component](web.md), [intro](web.md) và [hiện trạng](current-status.md).
