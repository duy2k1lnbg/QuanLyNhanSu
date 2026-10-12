---
title: "Bản đồ đối chiếu cấu trúc Database, phân quyền Capability và kiểm soát an toàn AI"
version: "1.2.0"
applicable_system: "HRMS Backend (EF 6.5.1 / Oracle) & AI Core"
status: "ACTIVE_TECHNICAL_SPEC"
created_date: "2026-10-11"
author: "HRMS Core Architecture Team"
---

# BẢN ĐỒ ĐỐI CHIẾU DATABASE, PHÂN QUYỀN CAPABILITY VÀ KIỂM SOÁT AN TOÀN AI

Tài liệu này xác lập bản đồ ánh xạ kỹ thuật chính xác giữa kho tri thức nhân sự (DataDoc), cấu trúc cơ sở dữ liệu Oracle thực tế (Entity Framework 6.5.1 qua file mô hình `QLNhanSu.edmx` và `AIEntities.edmx`) và hệ thống kiểm soát phân quyền của Trợ lý ảo AI (`AiCapabilityCatalog`, `AiScopePolicyResolver`, `AiPolicyProvider`).

---

## 1. Hiện trạng kiến trúc dữ liệu và công nghệ Backend

1. **Công nghệ CSDL & ORM thực tế:**
   - Hệ thống Backend sử dụng **Oracle Managed Data Access** kết hợp **Entity Framework 6.5.1** (không sử dụng EF Core).
   - Mô hình dữ liệu bao gồm các Entity thực tế trong `QLNhanSu.edmx` (tiền tố `TB_*`).
   - Tầng tích hợp AI đọc dữ liệu nghiệp vụ thông qua các View phân quyền thực tế trong `AIEntities.edmx` (tiền tố `V_AI_*`) và được quản lý tập trung qua `AiCapabilityCatalog.cs`.
2. **Tách biệt định danh: UserId vs Mã Nhân viên (MANV):**
   - **UserId (`ctx.UserId`):** Khóa chính tài khoản người dùng đăng nhập hệ thống (`TB_SYS_USER.IDUSER`).
   - **MANV (`ctx.Manv`):** Mã số nhân viên thực tế trong hồ sơ nhân sự (`TB_NHANVIEN.MANV`).
   - **Bảng liên kết thực tế:** `TB_USER_EMPLOYEE_MAPPING.USER_ID` → `TB_SYS_USER.IDUSER`, `EMPLOYEE_ID` → `TB_NHANVIEN.MANV`.
   - **Nguyên tắc kỹ thuật bắt buộc:** `UserId` **KHÔNG PHẢI** là `MANV`. Tài khoản Quản trị / IT có thể có `IDUSER` nhưng `MANV = null`. Khi người dùng tra cứu thông tin cá nhân (`SELF`), hệ thống bắt buộc sử dụng `ctx.Manv` được giải mã từ `TB_USER_EMPLOYEE_MAPPING` hoặc thủ tục bảo mật `PKG_AI_SECURITY.GET_USER_POLICY`, **tuyệt đối không được gán `MANV = UserId`**.

---

## 2. Bản đồ ánh xạ điều khoản tri thức, bảng dữ liệu CSDL và Capability AI

| Nhóm nghiệp vụ / Điều khoản | Dữ liệu nghiệp vụ liên quan | Bảng CSDL thực tế (EF 6.5.1) | View AI thực tế (`AIEntities.edmx`) | Capability Code trong C# | Scope cho phép theo Contract | Trạng thái thực tế trong Source (`AiCapabilityCatalog`) |
|:---|:---|:---|:---|:---|:---:|:---:|
| **Quy định chính sách chung**<br>([PL-01] đến [PL-06], [CT-01] đến [CT-10]) | Điều khoản quy chế, văn bản pháp luật, FAQ | Kho tri thức DataDoc | Vector alias `hrms_knowledge_active` | `POLICY_LOOKUP` | `ALL` (Người dùng xác thực) | **ĐANG BẬT** (`IsEnabled = true`) |
| **Hồ sơ nhân viên**<br>([PL-01], [CT-01]) | Họ tên, phòng ban, chức vụ, ngày vào làm, tình trạng HĐ | `TB_NHANVIEN`, `TB_PHONGBAN`, `TB_BOPHAN`, `TB_CHUCVU` | `V_AI_EMPLOYEE` | `EMPLOYEE_PROFILE` | `SELF`<br>`DEPARTMENT`<br>`COMPANY`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Tra cứu danh tính cơ bản**<br>([HD-01]) | Tìm mã NV, họ tên, vị trí công tác | `TB_NHANVIEN`, `TB_PHONGBAN` | `V_AI_EMPLOYEE_LOOKUP` | `EMPLOYEE_LOOKUP` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Thống kê nhân sự**<br>([CT-01]) | Đếm số lượng nhân viên theo phòng ban | `TB_NHANVIEN`, `TB_PHONGBAN` | `V_AI_EMPLOYEE_COUNT` | `EMPLOYEE_COUNT` | `DEPARTMENT`<br>`COMPANY`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Tổng hợp công & Phép**<br>([PL-01], [CT-02], [BM-01]) | Tổng ngày công, đi muộn, về sớm, nghỉ phép theo kỳ | `TB_BANGCONG`, `TB_BANGCONG_CHITIET`, `TB_KYCONG` | `V_AI_ATTENDANCE_SUMMARY` | `ATTENDANCE_SUMMARY` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Chi tiết quẹt thẻ hàng ngày**<br>([CT-02], [SOP-01]) | Giờ vào/ra từng ngày, bất thường công *(dữ liệu quẹt thẻ thô phần cứng)* | `TB_CHAMCONG_KQ_NGAY`, `TB_CHAMCONG_BATTHUONG` *(Log thô máy chấm công: [DỰ KIẾN / CHƯA MAP])* | `V_AI_ATTENDANCE` | `ATTENDANCE_DETAIL` | `SELF`<br>`DEPARTMENT` | **TẠM KHÓA** (`IsEnabled = false`) |
| **Đăng ký tăng ca / OT**<br>([PL-02], [CT-03], [BM-03]) | Danh sách ca OT, số giờ duyệt, hệ số tăng ca | `TB_TANGCA`, `TB_HESO_TANGCA`, `TB_YEUCAU_TANGCA` | `V_AI_OVERTIME` | `OVERTIME_VIEW` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Thống kê số giờ OT**<br>([PL-02], [CT-03]) | Tổng số giờ tăng ca theo tháng / xếp hạng OT | `TB_TANGCA`, `TB_KYCONG` | `V_AI_OVERTIME_SUMMARY` | `OVERTIME_SUM` | `DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Phụ cấp nhân viên**<br>([CT-05], [CT-09]) | Phụ cấp xăng xe, nhà ở, chuyên cần | `TB_PHUCAP`, `TB_NHANVIEN_PHUCAP` | `V_AI_ALLOWANCE` | `ALLOWANCE_VIEW` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Tạm ứng lương**<br>([CT-05]) | Khoản tiền tạm ứng trong tháng | `TB_UNGLUONG` | `V_AI_ADVANCE` | `ADVANCE_VIEW` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Hợp đồng lao động**<br>([PL-01], [CT-01]) | Loại HĐ, ngày ký, ngày hết hạn | `TB_HOPDONG`, `TB_LOAIHOPDONG` | `V_AI_CONTRACT` | `CONTRACT_VIEW` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Nâng lương nhân viên**<br>([PL-05], [CT-06]) | Quyết định nâng bậc, hệ số lương mới | `TB_NANGLUONG_NHANVIEN` | `V_AI_SALARY_CHANGE` | `SALARY_CHANGE_VIEW` | `SELF`<br>`DEPARTMENT`<br>`ALL` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Bảo hiểm xã hội cá nhân**<br>([PL-03], [SOP-02]) | Số sổ, ngày cấp, nơi cấp và nơi khám; chưa có mức đóng | `TB_BAOHIEM` | `V_AI_INSURANCE` | `INSURANCE_SELF` | `SELF` | **ĐANG BẬT** (`IsEnabled = true`) |
| **Bảo hiểm quản lý**<br>([PL-03], [SOP-02]) | Hồ sơ sổ bảo hiểm nhân viên trong phạm vi AI đã cấp | `TB_BAOHIEM` | `V_AI_INSURANCE` | `INSURANCE_VIEW` | Phạm vi AI đã cấp | **ĐANG BẬT**, cần `F_DM_NHANVIEN`; số bảo hiểm bị che |
| **Phiếu lương cá nhân**<br>([PL-02], [PL-04], [CT-05]) | Lương cơ bản, phụ cấp, OT, thực lĩnh cá nhân | `TB_BANGLUONG`, `TB_BANGLUONG_CT` | `V_AI_PAYROLL` | `PAYROLL_SELF` | `SELF` | **TẠM KHÓA** (`IsEnabled = false` qua `ApplyBranchB2Profile`) |
| **Bảng lương phòng ban**<br>([CT-05], [SOP-01]) | Chi tiết bảng lương nhân sự theo phòng ban | `TB_BANGLUONG`, `TB_BANGLUONG_CT` | `V_AI_PAYROLL` | `PAYROLL_VIEW` | `DEPARTMENT`<br>`ALL` | **TẠM KHÓA** (`IsEnabled = false` qua `ApplyBranchB2Profile`) |
| **Tổng quỹ lương công ty**<br>([CT-05]) | Tổng chi phí quỹ lương theo kỳ công | `TB_BANGLUONG` | `V_AI_PAYROLL_SUMMARY` | `PAYROLL_SUMMARY` | `ALL` | **TẠM KHÓA** (`IsEnabled = false` qua `ApplyBranchB2Profile`) |

> **LƯU Ý QUAN TRỌNG ĐỐI VỚI HỆ THỐNG AI:**
> - Kiểm tra mã nguồn và Oracle ngày 12/10/2026:, 4 capability: `PAYROLL_SELF`, `PAYROLL_VIEW`, `PAYROLL_SUMMARY`, `ATTENDANCE_DETAIL` **đang bị tắt chủ động** (`IsEnabled = false` qua hàm `ApplyBranchB2Profile()`).
> - Khi bất kỳ người dùng nào (kể cả Admin hay HR) hỏi chi tiết phiếu lương cá nhân, bảng lương phòng ban hoặc dữ liệu quẹt thẻ chi tiết, AI **phải từ chối và phản hồi rõ ràng**: *"Chức năng tra cứu dữ liệu lương/chi tiết quẹt thẻ qua AI hiện đang tạm khóa theo chính sách bảo mật của hệ thống. Vui lòng tra cứu trực tiếp trên phân hệ giao diện chuyên môn hoặc liên hệ Phòng Nhân sự."*
> - Tuyệt đối không tự ý giả định các Tool duyệt đơn hay lấy lương như `GetMySalarySlipTool` hay `ApproveRequestTool`. Mọi quy trình phê duyệt (Approval Workflow) phải do con người thao tác trực tiếp trên giao diện phần mềm.

---

## 3. Ma trận kiểm soát phân quyền (RBAC & Dynamic Policy Matrix)

Trong kiến trúc Backend của HRMS, quyền hạn thực tế **KHÔNG ĐƯỢC SUY ĐOÁN ĐƠN THUẦN TỪ TÊN ROLE** mà được giải quyết thông qua bộ phân giải chính sách động `AiScopePolicyResolver.cs`:
- **Chính sách từ chối (DENY):** Có độ ưu tiên tuyệt đối cao nhất (`IsExplicitDeny`). Nếu một capability hoặc người dùng bị cấu hình DENY, mọi quyền hạn từ Role hay Scope đều bị vô hiệu hóa ngay lập tức.
- **Nguồn bị tắt (Disabled Capabilities):** Nếu `AiCapabilityDefinition.IsEnabled == false`, hệ thống từ chối thực thi bất kể người dùng mang Role gì.
- **Phạm vi hiệu lực (EffectiveScopeType):** Tuân thủ contract chuẩn: `"ALL"`, `"DEPARTMENT"`, `"COMPANY"`, `"SELF"`, `"NONE"`.

| Nhóm dữ liệu / Hành động | Nhân viên thường (`Role: EMPLOYEE`) | Quản lý bộ phận (`Role: MANAGER`) | Chuyên viên HR (`Role: HR`) | Quản trị hệ thống (`Role: ADMIN`) |
|:---|:---:|:---:|:---:|:---:|
| **Tra cứu Quy chế & Luật (`POLICY_LOOKUP`)** | ĐƯỢC PHÉP (nếu không có DENY) | ĐƯỢC PHÉP (nếu không có DENY) | ĐƯỢC PHÉP (nếu không có DENY) | ĐƯỢC PHÉP (nếu không có DENY) |
| **Hồ sơ nhân viên (`EMPLOYEE_PROFILE`)** | Chỉ xem chính mình (`Scope: SELF`, `ctx.Manv`) | Xem cấp dưới trong bộ phận được cấp quyền (`Scope: DEPARTMENT`, `EffectiveDepartmentIds`) | Xem theo thẩm quyền quản lý nhân sự (`Scope: COMPANY` hoặc `ALL`) | Xem theo cấu hình quyền (`Scope: ALL`) |
| **Tổng hợp công (`ATTENDANCE_SUMMARY`)** | Chỉ xem chính mình (`Scope: SELF`, `ctx.Manv`) | Xem cấp dưới trong bộ phận được cấp quyền (`Scope: DEPARTMENT`) | Xem toàn công ty theo quyền chức năng `F_CC_BANGCONG` | Chỉ xem theo quyền chức năng và phạm vi AI đã cấp |
| **Tăng ca / OT (`OVERTIME_VIEW`)** | Chỉ xem chính mình (`Scope: SELF`, `ctx.Manv`) | Xem cấp dưới trong bộ phận được cấp quyền (`Scope: DEPARTMENT`) | Xem theo quyền chức năng `F_CC_TANGCA` | Chỉ xem theo quyền chức năng và phạm vi AI đã cấp |
| **Phiếu lương / Chi tiết lương (`PAYROLL_*`)** | **BỊ TỪ CHỐI** (Capability đang tạm khóa) | **BỊ TỪ CHỐI** (Capability đang tạm khóa) | **BỊ TỪ CHỐI** (Capability đang tạm khóa) | **BỊ TỪ CHỐI** (Capability đang tạm khóa) |
| **Chi tiết quẹt thẻ thô (`ATTENDANCE_DETAIL`)** | **BỊ TỪ CHỐI** (Capability đang tạm khóa) | **BỊ TỪ CHỐI** (Capability đang tạm khóa) | **BỊ TỪ CHỐI** (Capability đang tạm khóa) | **BỊ TỪ CHỐI** (Capability đang tạm khóa) |
| **Tra cứu dữ liệu người khác khi chỉ có `Scope: SELF`** | **BỊ TỪ CHỐI (DENY)** | **BỊ TỪ CHỐI (DENY)** nếu người đó ngoài `EffectiveDepartmentIds` | Theo phạm vi phân quyền thực tế | Chỉ theo quyền chức năng và scope thực tế; DENY vẫn thắng |
| **Phê duyệt đơn từ qua Chatbot** | **KHÔNG HỖ TRỢ** (Thao tác trên UI) | **KHÔNG HỖ TRỢ** (Thao tác trên UI) | **KHÔNG HỖ TRỢ** (Thao tác trên UI) | **KHÔNG HỖ TRỢ** (Thao tác trên UI) |

---

## 4. Các chốt chặn an ninh bắt buộc trong mã nguồn C#

1. **Chốt chặn 1 - Identity Binding & MANV Validation:**
   - Mọi truy vấn dữ liệu cá nhân phải lấy `MANV` từ `ctx.Manv` được giải mã từ `TB_USER_EMPLOYEE_MAPPING`.
   - Nếu câu lệnh truy vấn nhận tham số `MANV` khác `ctx.Manv` mà người dùng không có quyền truy cập phạm vi tương ứng (`DEPARTMENT`, `COMPANY`, `ALL`), hệ thống phải ném ngoại lệ bảo mật:
     ```csharp
     throw new SecurityException("Truy cập trái phép dữ liệu cá nhân của người khác");
     ```
2. **Chốt chặn 2 - Scope Evaluation & Department Boundaries:**
   - Khi người dùng có `EffectiveScopeType == "DEPARTMENT"`, bộ phân giải `AiScopeEvaluator.IsEmployeeInScope` bắt buộc phải kiểm tra xem nhân viên mục tiêu có thuộc danh sách `EffectiveDepartmentIds` của người quản lý hay không. Quản lý xưởng A tuyệt đối không được truy vấn nhân sự xưởng B.
3. **Chốt chặn 3 - Kiểm tra Capability IsEnabled trước khi Planner sinh SQL:**
   - Trước khi lập kế hoạch truy vấn dữ liệu nghiệp vụ, `AiExecutionService` bắt buộc phải kiểm tra `AiCapabilityDefinition.IsEnabled`. Nếu capability đang tắt, hệ thống trả về thông báo từ chối an toàn ngay lập tức, hoàn toàn không sinh câu lệnh truy vấn CSDL.
4. **Chốt chặn 4 - Cách ly hoàn toàn nhánh Casual Chit-chat:**
   - Các câu hỏi trò chuyện thông thường ngoài nghiệp vụ nhân sự (thời tiết, chào hỏi, thơ văn) được xử lý hoàn toàn qua module Chat tự nhiên, không cấp phát Connection String CSDL, không nạp công cụ và không truyền ngữ cảnh nhân sự.


## Cập nhật kiểm chứng 12/10/2026

Oracle và catalog có 17 capability: 13 bật, 4 tắt do chưa có protected view `V_AI_PAYROLL`, `V_AI_PAYROLL_SUMMARY`, `V_AI_ATTENDANCE`. `INSURANCE_VIEW` được bật qua migration V1_47; `INSURANCE_SELF` và `INSURANCE_VIEW` có 10 chính sách trường mỗi loại. Chỉ thêm trường chưa có, không sửa scope, DENY hoặc quyền chức năng tài khoản.

`V_AI_INSURANCE` chỉ phục vụ hồ sơ sổ bảo hiểm; không tuyên bố trả được tiền đóng BHXH/BHYT/BHTN. Self dùng MANV được ánh xạ. Quản lý cần `F_DM_NHANVIEN` cùng grant AI; số bảo hiểm được che trong cả câu trả lời và bảng JSON API. Tài khoản chỉ có quyền đăng nhập Mobile chưa mặc nhiên được cấp scope AI.

Nguồn structured dùng Oracle AI_READONLY, proof HMAC và ticket; không nạp bảng lương/công/bảo hiểm trực tiếp vào collection quy chế. Alias tri thức có 112 điểm (83 nội bộ demo công ty 1 + 29 pháp luật); 11 benchmark ngoài công ty chưa được công bố.
