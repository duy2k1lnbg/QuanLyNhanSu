---
doc_id: "HD-01"
title: "Hướng dẫn thao tác giao diện người dùng trên hệ thống HRMS (môi trường demo)"
version: "2.0.0-demo"
status: "ACTIVE_OFFICIAL"
effective_from: "2026-10-12"
effective_to: null
approved_by: "Chủ sở hữu hệ thống demo — yêu cầu phê duyệt qua Codex ngày 12/10/2026"
company_id: "1"
source_kind: "SYSTEM_SPEC"
document_type: "USER_GUIDE"
topics: ["HRMS_PORTAL", "ATTENDANCE_TIMEKEEPING", "LEAVE_POLICY", "OVERTIME_POLICY", "SALARY_POLICY", "AI_ASSISTANT"]
jurisdiction: "VN"
applicable_location: "Bắc Ninh"
applicability_mode: "ALL"
access_profile: "PUBLIC_INTERNAL"
applies_to_current_company: true
ingest_enabled: true
requires_hr_approval_for_live: true
legal_basis: []
benchmark_basis: []
source_urls: []
unresolved_fields: []
allowed_role_ids: []
allowed_department_ids: []
---

# HƯỚNG DẪN THAO TÁC GIAO DIỆN NGƯỜI DÙNG TRÊN HỆ THỐNG HRMS (HD-01) — MÔI TRƯỜNG DEMO

> **Phê duyệt trong hệ thống demo:** Công ty mã `1`, hiệu lực `12/10/2026`, theo yêu cầu của chủ sở hữu hệ thống. Không xác nhận đã đăng ký nội quy với cơ quan nhà nước. Mức tiền cá nhân dùng hợp đồng/chính sách còn hiệu lực đã duyệt; thông tin chưa được cấu hình phải liên hệ bộ phận phụ trách.

**Hướng dẫn áp dụng chung:** Mobile: vào Trang chủ để gửi đơn nghỉ phép, điều chỉnh công, tăng ca hoặc tạm ứng; vào Yêu cầu của tôi để theo dõi. Thông báo trong ứng dụng lấy dữ liệu từ backend. Đẩy thông báo khi đóng ứng dụng và email chưa được cấu hình trong demo, không cam kết đã hoạt động. Hồ sơ cá nhân/người phụ thuộc cần đúng quyền và luồng phê duyệt theo chức năng hiện hành.

### [HD-01/MUC-01] Đăng nhập và quản lý tài khoản người dùng
1. **Truy cập hệ thống:**
   - Phiên bản Web (`HRMS.Web`): Truy cập qua trình duyệt web theo địa chỉ máy chủ mạng nội bộ của công ty;
   - Phiên bản Desktop (`HRMS.Desktop`): Khởi chạy ứng dụng HRMS trên máy trạm Windows nội bộ.
2. **Quy trình đăng nhập:**
   - Tên đăng nhập: Nhập tên tài khoản (Username) được cấp bởi Quản trị viên hệ thống (IT/HR);
   - Mật khẩu: Mật khẩu khởi tạo theo quy trình bàn giao tài khoản nội bộ Quản trị viên IT/HR cấp tài khoản theo đúng nhân viên và quyền đã duyệt; người dùng đổi mật khẩu được cấp ngay khi nhận tài khoản, không dùng chung tài khoản;
   - Sau khi đăng nhập thành công, hệ thống xác thực JWT Token và nạp danh sách quyền chức năng (`FunctionRights`) tương ứng với vai trò của người dùng.

---

### [HD-01/MUC-02] Thao tác tra cứu Chấm công và Bảng công
1. **Trên giao diện Web (`HRMS.Web`):**
   - Vào menu bên trái: chọn phân hệ **Chấm công** (`chamcong`);
   - Màn hình hỗ trợ: Xem bảng công theo kỳ công (`F_CC_BANGCONG`), danh mục loại ca (`F_CC_LOAICA`), kỳ công tháng (`F_CC_KYCONG`);
   - Thao tác nhanh (Quick Action): Bấm biểu tượng Lịch trên thanh tiêu đề hoặc chọn "Tạo nghỉ phép" (hệ thống điều hướng về phân hệ `chamcong` để lập đề nghị nghỉ);
   - Nhân viên xem số ngày công thực tế, ngày nghỉ phép và số giờ tăng ca của mình theo kỳ công được chọn.
2. **Trên giao diện Desktop (`HRMS.Desktop`):**
   - Mở màn hình `Chấm công` $\rightarrow$ `Bảng công nhân viên`;
   - Chọn tháng, năm và phòng ban để xem chi tiết quẹt thẻ và kết quả tổng hợp công.

---

### [HD-01/MUC-03] Thao tác Đăng ký Làm thêm giờ (Tăng ca) và Ứng lương
1. **Trên giao diện Web (`HRMS.Web`):**
   - Vào menu bên trái: chọn phân hệ **Tăng ca / Ứng lương** (`ungluong`);
   - Phân hệ này quản lý các tính năng: Đăng ký làm thêm giờ (`F_CC_TANGCA`) và Đề nghị tạm ứng lương (`F_CC_UNGLUONG`);
   - Người lao động tạo phiếu đăng ký tăng ca: Chọn ngày, khung giờ làm thêm dự kiến, lý do tăng ca và xác nhận tính tự nguyện;
   - Bấm `Lưu` / `Gửi đề nghị`. Phiếu sẽ chuyển đến hàng đợi phê duyệt của cấp quản lý.
2. **Quy trình Phê duyệt của cấp Quản lý:**
   - Quản lý bộ phận có quyền truy cập menu **Phê duyệt** (`approvals`);
   - Kiểm tra danh sách các đề nghị tăng ca, nghỉ phép của nhân viên trong bộ phận và thực hiện duyệt (Approve) hoặc từ chối (Reject).

---

### [HD-01/MUC-04] Thao tác tra cứu Bảng lương và Phiếu lương
1. **Trên giao diện Web (`HRMS.Web`):**
   - Người dùng có quyền xem tiền lương truy cập menu **Bảng lương** (`bangluong`);
   - Màn hình hiển thị bảng tính lương theo kỳ: Lương cơ bản, phụ cấp, tiền làm thêm giờ, các khoản trích nộp BHXH, BHYT, BHTN, thuế TNCN và số tiền thực lĩnh;
   - Đối với chuyên viên C&B/HR: Thực hiện tính lương thông qua nút thao tác chuyên môn trên màn hình bảng lương.
2. **Quyền tra cứu lương qua AI Copilot:** Backend kiểm tra capability và phạm vi dữ liệu thực tế của tài khoản. Nếu không có quyền, AI hướng dẫn xem Bảng lương/Phiếu lương theo quyền được cấp; tài liệu không tự mở quyền đọc lương.

---

### [HD-01/MUC-05] Sử dụng Trợ lý ảo AI Copilot
1. **Khởi chạy cửa sổ AI Copilot trên Web:**
   - Bấm vào mục **AI Copilot** trên menu Sidebar (`ai-drawer`) hoặc biểu tượng Robot trên thanh tiêu đề Header;
   - Cửa sổ Drawer AI sẽ trượt ra từ cạnh phải màn hình.
2. **Phạm vi hỗ trợ của AI Copilot hiện tại:**
   - **Hỏi đáp chính sách & luật lao động:** Tra cứu quy định pháp luật về làm thêm giờ, thời giờ làm việc, ngày nghỉ phép, ngày lễ tết, lương tối thiểu vùng;
   - **Tra cứu hồ sơ cá nhân:** Xem thông tin mã nhân viên, phòng ban, chức vụ của chính tài khoản đăng nhập;
   - **Tra cứu tổng hợp công:** Hỏi số ngày công trong kỳ công đã chốt của cá nhân;
   - **Trò chuyện hỗ trợ thông thường:** Giao tiếp thân thiện, giải đáp thông tin sử dụng phần mềm.
3. **Các giới hạn an toàn bảo mật:**
   - AI chỉ cung cấp dữ liệu trong phạm vi được backend cấp; nhân viên tự phục vụ chỉ xem dữ liệu của mình;
   - AI không có chức năng tự phê duyệt đơn từ thay cho quản lý.
