---
doc_id: "CT-02"
title: "Quy chế chấm công và xử lý điều chỉnh công (môi trường demo)"
version: "2.0.0-demo"
status: "ACTIVE_OFFICIAL"
effective_from: "2026-10-12"
effective_to: null
approved_by: "Chủ sở hữu hệ thống demo — yêu cầu phê duyệt qua Codex ngày 12/10/2026"
company_id: "1"
source_kind: "COMPANY_POLICY"
document_type: "REGULATION"
topics: ["ATTENDANCE_TIMEKEEPING", "DISCIPLINE_POLICY"]
jurisdiction: "VN"
applicable_location: "Bắc Ninh"
applicability_mode: "ALL"
access_profile: "PUBLIC_INTERNAL"
applies_to_current_company: true
ingest_enabled: true
requires_hr_approval_for_live: true
legal_basis: ["PL-01: Bộ luật Lao động 2019 (Điều 105)", "PL-06: Bộ luật Lao động 2019 (Điều 124, 127)"]
benchmark_basis: []
source_urls: []
unresolved_fields: []
allowed_role_ids: []
allowed_department_ids: []
---

# QUY CHẾ CHẤM CÔNG VÀ XỬ LÝ ĐIỀU CHỈNH CÔNG — MÔI TRƯỜNG DEMO

> **Phê duyệt trong hệ thống demo:** Công ty mã `1`, hiệu lực `12/10/2026`, theo yêu cầu của chủ sở hữu hệ thống. Không xác nhận đã đăng ký nội quy với cơ quan nhà nước. Mức tiền cá nhân dùng hợp đồng/chính sách còn hiệu lực đã duyệt; thông tin chưa được cấu hình phải liên hệ bộ phận phụ trách.

**Hướng dẫn áp dụng chung:** Chỉ sử dụng dữ liệu thiết bị đã được HR xác nhận; demo hỗ trợ dữ liệu quẹt thẻ/vân tay/khuôn mặt theo cấu hình. Đơn sửa công phải được quản lý/HR duyệt trước khi áp dụng. Không được sửa công hoặc mở khóa kỳ đã công bố lương.

### [CT-02/DIEU-01] Phương thức chấm công điện tử
1. Công ty áp dụng phương thức chấm công tự động thông qua máy nhận diện khuôn mặt / vân tay tại các cổng an ninh và cửa ra vào nhà xưởng.
2. Mỗi nhân viên chỉ được sử dụng một mã định danh duy nhất (Mã NV) gắn liền với dữ liệu sinh trắc học đã đăng ký trong hồ sơ nhân sự.
3. Người lao động có nghĩa vụ quẹt thẻ/nhận diện 02 lần cho mỗi ca làm việc:
   - Quẹt thẻ vào đầu ca: Trước giờ bắt đầu ca làm việc;
   - Quẹt thẻ ra cuối ca: Sau khi kết thúc thời gian làm việc và hoàn tất bàn giao.

---

### [CT-02/DIEU-02] Xác định đi muộn, về sớm và ngày công tiêu chuẩn
1. **Đi muộn:** Được xác định khi thời điểm quẹt thẻ vào ca sau giờ quy định bắt đầu ca từ 01 phút trở lên.
2. **Về sớm:** Được xác định khi thời điểm quẹt thẻ ra ca trước giờ quy định kết thúc ca từ 01 phút trở lên mà không có giấy cho phép về sớm của quản lý.
3. **Tính ngày công:** Công ty không trả tiền lương cho những giờ hoặc phút người lao động không làm việc thực tế do đi muộn hoặc về sớm (tính trừ giờ công thực tế, tuyệt đối không áp dụng phạt tiền theo Điều 127 BLLĐ).

---

### [CT-02/DIEU-03] Xử lý sự cố quên quẹt thẻ và điều chỉnh công (Mẫu BM-01)
1. **Trường hợp áp dụng:** Áp dụng khi người lao động có đi làm thực tế nhưng quên quẹt thẻ đầu ca/cuối ca hoặc máy chấm công gặp sự cố kỹ thuật không nhận diện.
2. **Quy trình giải trình điều chỉnh công:**
   - Người lao động lập Phiếu yêu cầu sửa công và giải trình (Mẫu BM-01) trên phần mềm HRMS hoặc nộp bản giấy;
   - Thời hạn nộp đơn giải trình: 48 giờ kể từ khi phát sinh ca làm việc; trường hợp có lý do chính đáng nộp muộn phải được HR xác minh;
   - Phải có xác nhận của Tổ trưởng/Quản lý trực tiếp về sự có mặt và thời gian làm việc thực tế;
   - Giới hạn số lần giải trình: 03 lần/tháng để cảnh báo và hỗ trợ; không được tự động coi là vắng mặt hay cắt tiền công thực tế đã làm. Nếu vượt quá số lần cho phép mà không có lý do chính đáng, HR xác minh công thực tế, nhắc nhở và hỗ trợ; không tự động coi là vắng mặt.

---

### [CT-02/DIEU-04] Xử lý vi phạm kỷ luật chấm công
1. **Hành vi bị nghiêm cấm:**
   - Nhờ người khác quẹt thẻ hộ hoặc quẹt thẻ hộ cho người khác;
   - Cố tình can thiệp làm sai lệch dữ liệu máy chấm công;
   - Khai báo gian lận giờ công hoặc ký xác nhận khống cho nhân sự không có mặt làm việc.
2. **Chế tài xử lý:**
   - Vi phạm lần đầu: Xử lý kỷ luật bằng hình thức **Khiển trách** bằng văn bản theo Điều 124 BLLĐ;
   - Tái phạm hoặc có hành vi gian lận giờ công nghiêm trọng: Xem xét xử lý kỷ luật bằng hình thức **Kéo dài thời hạn nâng lương không quá 06 tháng**, **Cách chức** hoặc **Sa thải** nếu đủ yếu tố cấu thành theo Điều 125 BLLĐ;
   - **Tuyệt đối cấm phạt tiền trừ lương:** Nghiêm cấm quản lý hoặc công ty phạt trừ tiền lương đối với lỗi đi muộn hoặc quên quẹt thẻ (Điều 127 BLLĐ).
