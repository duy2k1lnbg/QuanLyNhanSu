# Web: giao diện, KPI và chuyển động

Cập nhật nội dung: 01/10/2026.

Ghi chú tham khảo từ mã nguồn React/TypeScript. Chưa kiểm tra trực quan mọi màn hình. Xem [README Web](../HRMS.Web/README.md) để chạy ứng dụng và [kiểm thử](testing.md) để lập lượt kiểm tra.

## Theme và thành phần dùng chung

### Nguồn hiện có

[tokens.ts](../HRMS.Web/src/theme/tokens.ts) định nghĩa token light/dark, [ThemeContext](../HRMS.Web/src/theme/ThemeContext.tsx) quản lý theme và [motion.css](../HRMS.Web/src/theme/motion.css) khai báo chuyển động. Màu dưới đây được đọc từ token trong workspace; CSS và từng component có thể có lớp ghi đè cần đối chiếu riêng.

### Một số token

| Token | Light | Dark |
| --- | --- | --- |
| `appBg` | `#F6F7FB` | `#080B14` |
| `sidebarBg` | `#FFFFFF` | `#0C101C` |
| `headerBg` | `#FFFFFF` | `#0C101C` |
| `cardBg` | `#FFFFFF` | `#101522` |
| `elevatedBg` | `#FFFFFF` | `#1A2235` |
| `textPrimary` | `#202332` | `#F1F3F9` |
| `textSecondary` | `#626B7E` | `#B3BCD0` |
| `textMuted` | `#8C98B0` | `#8C98B0` |
| `borderSubtle` | `#E5E8F0` | `#1E2638` |
| `primary` | `#6D4AFF` | `#9A79FF` |
| `focusRing` | `rgba(109, 74, 255, 0.25)` | `rgba(154, 121, 255, 0.35)` |

### Thành phần dùng chung

Các component trong [theme/components](../HRMS.Web/src/theme/components) gồm PageHeader, PageToolbar, MetricCard, SectionCard, StatusBadge và ThemeToggle. Dùng lại chúng khi phù hợp để thống nhất khoảng cách, tiêu đề và trạng thái; không cần thêm một component mới cho mọi biến thể nhỏ.

### Kiểm tra khi sửa giao diện

- Text/nền, focus bàn phím, hover/selected/disabled/error ở cả hai theme.
- Bảng dài, số tiền/ngày, drawer/modal và màn hình hẹp.
- Năm ngôn ngữ, chữ dài và dữ liệu thiếu.
- Print nếu có luồng in, cùng lựa chọn giảm chuyển động.
- Theme khi tải lại trang và thay đổi ngôn ngữ/phiên.

Chưa đo lại độ tương phản hoặc kiểm thử accessibility cho mọi màn hình. Không gán nhãn WCAG đạt từ việc có bảng token. Xem [ghi chú QA](testing.md).

## KPI và bảng công

### Nguồn mã

[dashboardMetrics.ts](../HRMS.Web/src/utils/dashboardMetrics.ts) và [attendanceViewModel.ts](../HRMS.Web/src/utils/attendanceViewModel.ts) cung cấp các phép tổng hợp ở Web. [kpiAndAttendance.test.ts](../HRMS.Web/tests/kpiAndAttendance.test.ts) có fixture kiểm tra số lượt, nhân sự, hợp đồng và cách diễn giải ký hiệu công.

### Các số cần phân biệt

- Số lượt chấm công không đồng nghĩa số nhân viên duy nhất.
- Số nhân viên đang làm việc khác tổng hồ sơ có trong dữ liệu.
- Số hợp đồng không đồng nghĩa số nhân viên có hợp đồng còn hiệu lực.
- Tỷ lệ chuyên cần cần có mẫu số/lịch được định nghĩa, không lấy số dòng đang lọc làm mẫu số một cách tùy ý.
- Tổng lương cần xác định kỳ, người thuộc phạm vi và dữ liệu nguồn.

Các số 215/195/200 trong ghi chép và test là trường hợp để kiểm tra cách đếm; không phải số liệu vận hành mới đã truy vấn trong nhiệm vụ này.

### Kết quả đã quan sát

Trong phiên rà soát ngày 01/10/2026, nhóm test KPI/chấm công đạt 4 nhóm kiểm tra. Kết quả này xác nhận các assertion của fixture đã chạy, không chứng minh mọi lịch/chính sách hoặc dữ liệu thực tế đều được diễn giải đúng.

### Phần cần thử thêm

Đối chiếu request/API thực tế với màn hình theo từng kỳ/ngày/phòng ban, trường hợp thiếu lịch, ngày không có lịch, nghỉ nửa ngày, nhân viên nghỉ việc và dữ liệu trùng. Kiểm tra ý nghĩa nhãn/ký hiệu với nghiệp vụ sử dụng.

Xem [hiện trạng](current-status.md) và [nguồn Dashboard](../HRMS.Web/src/pages/DashboardPage.tsx), [ChamCongPage](../HRMS.Web/src/pages/ChamCongPage.tsx).

## Intro và chuyển động

### Cơ chế đọc từ source hiện tại

[useWelcomeIntro](../HRMS.Web/src/hooks/useWelcomeIntro.ts) dùng các trạng thái `entering`, `exiting`, `done` và key `hrms_welcome_intro_tab_seen_v2` trong sessionStorage, có biến bộ nhớ dự phòng. Source hiện tại khác key/state được mô tả trong bản ghi chép cũ.

Hook kiểm tra lựa chọn giảm chuyển động; xử lý kết thúc intro khi có thao tác bàn phím hoặc chuyển tab ra nền. [WelcomeIntro](../HRMS.Web/src/components/WelcomeIntro.tsx), [useScrollReveal](../HRMS.Web/src/hooks/useScrollReveal.ts) và [motion.css](../HRMS.Web/src/theme/motion.css) là nguồn cần xem cùng nhau.

### Kết quả đã quan sát

Trong phiên rà soát, nhóm [welcomeAndScrollReveal.test.ts](../HRMS.Web/tests/welcomeAndScrollReveal.test.ts) đạt 3 nhóm kiểm tra về ngôn ngữ, trạng thái phiên tab và lựa chọn giảm chuyển động. Chưa thử lại hoạt ảnh/timing, scroll và các thao tác thực tế trên mọi trình duyệt.

### Việc cần thử trực tiếp

- Mở trang mới, reload và mở tab theo các cách trình duyệt hỗ trợ.
- Bỏ qua bằng bàn phím/nút, chuyển tab nền, thay đổi reduced-motion.
- Khi intro kết thúc, scroll và focus được phục hồi như mong đợi.
- Đổi theme/ngôn ngữ/phiên, lỗi storage và màn hình hẹp.
- Thành phần scroll-reveal đã ở viewport, thay dữ liệu và thao tác nhanh.

Ghi cách mở tab và trạng thái storage trong test, không chỉ kết luận một key sessionStorage bảo đảm mọi trường hợp vòng đời trình duyệt. Xem [hiện trạng](current-status.md) và [QA Web](testing.md).

## Khi sửa một trang

Xác định trang trong [src/pages](../HRMS.Web/src/pages) và component dùng chung bị ảnh hưởng. Chuẩn bị dữ liệu có dòng, trống, thiếu, tải chậm và lỗi; thử quyền bằng request tới server. Sau đó kiểm tra light/dark, năm ngôn ngữ, viewport hẹp và bàn phím, rồi chạy build/lint/test phù hợp. Ghi rõ màn hình chưa thử.
