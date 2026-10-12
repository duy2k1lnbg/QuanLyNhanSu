# Công bố lương và khóa dữ liệu sau công bố

## Mở chức năng

- PC: **Chấm Công → Bảng Lương → Công bố lương**.
- Web: **Bảng Lương → Công bố lương**, ngay dưới phần chọn kỳ.
- Cần quyền Xem bảng lương và quyền Sửa của **Công bố lương** (`F_CC_CONGBO_LUONG`) trên đúng nền tảng. ADMIN đã được cấp riêng trên Desktop và Web trong CSDL demo cấu hình hiện tại. Đăng xuất và đăng nhập lại để nhận phiên bản quyền mới. Tên ADMIN không tự bỏ qua quyền.

## Quy trình với kỳ tính lương mới

1. Chọn đúng kỳ. Dùng Quy trình tính lương để kiểm tra chính sách, hợp đồng, hồ sơ bảo hiểm/công đoàn/thuế và người phụ thuộc còn hiệu lực; mở nơi xử lý các mục thiếu.
2. Giải quyết yêu cầu và bất thường công, công bố công; đối chiếu tăng ca, phụ cấp, ứng lương và các khoản phát sinh.
3. Tính lương tại PC, xem chi tiết, kiểm tra tổng thu nhập, khấu trừ và thực lĩnh. Kết quả phải lưu phiên bản công lúc tính; nếu đầu vào thay đổi phải tính lại trước công bố.
4. Mở Bảng công để khóa kỳ, sau đó trở lại Công bố lương → Kiểm tra lại.
5. Nhập căn cứ công bố (5–1000 ký tự), đọc cảnh báo bất biến và xác nhận. Một giao dịch ghi trạng thái công bố, khóa kỳ, nhật ký và thông báo cho từng nhân viên.
6. Nhân viên mở lại Bảng lương hoặc tab Thông báo trong Mobile để tải dữ liệu mới. Công bố lương không có nghĩa đã chi trả.

Kỳ 202610 đang có 195 phiếu CALCULATED từ lần tính trước khi bổ sung cột phiên bản công; phải tính lại một lần để lưu phiên bản, sau đó khóa kỳ và công bố. Không tự sửa cột phiên bản để vượt kiểm tra.

## Phiếu lương lịch sử

Các kỳ 202601, 202602, 202603 hiện có 194 phiếu mỗi kỳ, trạng thái LEGACY_READONLY. Chỉ công bố khi đã đối chiếu số liệu lưu trữ, đánh dấu xác nhận hồ sơ lịch sử và nhập căn cứ. Không tính lại lịch sử theo chính sách mới. Công bố lịch sử cũng khóa bất biến toàn kỳ công.

## Sau công bố

Lương và bảng công của kỳ đã công bố chỉ được đọc, kể cả ADMIN. Không được sửa số tiền, chi tiết lương, nguồn công, trạng thái lương, thu hồi công bố, xóa phiếu, thêm phiếu, tính lại hoặc mở khóa kỳ công. Dữ liệu công, kết quả công, lịch làm việc, tăng ca, ứng lương và các yêu cầu liên quan kỳ đã công bố cũng được bảo vệ. Hồ sơ cá nhân và chính sách cho kỳ tương lai vẫn quản lý theo quy trình phiên bản hiện có.

Thông báo khi cố sửa: **“Lương kỳ này đã công bố. Không được sửa, xóa, tính lại, thu hồi công bố, đổi trạng thái lương hoặc sửa/mở khóa bảng công.”** API trả HTTP 409, mã PAYROLL_FROZEN; PC/Web/Mobile dịch thông báo trong 5 ngôn ngữ. Nút thu hồi công bố đã được bỏ.

CSDL dùng chung kỳ công giữa các công ty. Người chỉ có phạm vi một phần kỳ không được công bố cả kỳ; cần quản trị viên có quyền trên toàn kỳ. Kỳ khác chưa công bố tiếp tục thao tác bình thường.

## Trạng thái triển khai ngày 12/10/2026

- V1_44 và V1_45 đã áp dụng vào schema HR của kết nối demo đang cấu hình; 27 trigger khóa bất biến đã bật, không có lỗi biên dịch. Không tự công bố phiếu thật của bộ demo.
- Kiểm thử Oracle: 63 kiểm tra đạt, gồm công bố 194 phiếu lịch sử trong giao dịch thử, các thao tác cố sửa và rollback trạng thái/số tiền/khóa/thông báo. Không còn fixture hay thông báo thử đã commit. Sequence Oracle có thể có khoảng trống sau rollback.
- NUnit: 11 kiểm tra hành vi đạt. Mobile: 39 kiểm tra đạt, TypeScript đạt. Web: bộ kiểm thử, bản dịch 5 ngôn ngữ, TypeScript và build production đạt. PC, API và APK build thành công; còn cảnh báo có sẵn về dependency/Fakes và kích thước bundle.
- Chưa ghi đè chương trình PC đang chạy, chưa triển khai API/Web lên VPS, chưa kiểm tra giao diện thực tế trên điện thoại/PC. Các gói dưới đây là bản build để cập nhật cùng phiên bản.

## Cập nhật các bản ứng dụng

1. **PC**: thoát ứng dụng, sao lưu QLyNSu.exe/Bu.dll/DA.dll hiện có rồi thay bằng ba file trong `HRMS-PC-Payroll-review.zip`. Giữ cấu hình và thư viện DevExpress hiện tại; gói này không phải bộ cài độc lập. Mở lại, đăng nhập lại, kiểm tra nút Công bố lương.
2. **API**: trong thời gian bảo trì, sao lưu bin rồi cập nhật DLL từ `HRMS-API-Mobile-review.zip` vào bin của API và khởi động lại host. Giữ Web.config, khóa bí mật và cấu hình kết nối của máy chủ. Gói không chứa cấu hình/mật khẩu hoặc DLL kiểm thử.
3. **Web**: cập nhật `index.html`, `assets` và các tệp tĩnh từ `HRMS-Web-Payroll-review.zip` vào site, giữ web.config/cấu hình host hiện tại rồi tải lại trình duyệt. Gói có APK Mobile mới tại downloads/HRMS_Mobile.apk, không kèm bộ cài PC cũ. Nếu chỉ cập nhật Web mà chưa cập nhật API thì chức năng Công bố lương sẽ báo cần cập nhật máy chủ.
4. **Mobile**: cài `HRMS-Mobile-review.apk`, đăng nhập tài khoản nhân viên. Đây là APK debug có JavaScript bundle; không cần Metro. Expo Go vẫn chạy chế độ thông báo trong ứng dụng, không nạp push SDK. Chưa triển khai remote push theo yêu cầu trước đó.
5. Nếu API/VPS kết nối **database khác** database demo đã xử lý, DBA cần áp dụng V1_44 rồi V1_45 trong thời gian bảo trì, bằng SQLPlus UTF-8, đúng schema HR; cấp quyền công bố riêng theo nền tảng. Không chạy lại cấp quyền hàng loạt hoặc dùng dữ liệu cấu hình mẫu.

Các script SQL có kiểm tra cấu trúc trước DDL và dừng khi lỗi. Oracle DDL tự commit: phải giữ các thao tác ghi tạm dừng nếu migration lỗi. Script V1_45_rollback chỉ phục hồi trước khi có bất kỳ kỳ lương công bố nào; tự từ chối nếu đã công bố. Không dùng script phục hồi để sửa lương đã công bố.
