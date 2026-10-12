# Nhật ký thay đổi bộ DataDoc HRMS (Changelog)

Toàn bộ các phiên bản và quá trình biên soạn, cập nhật tài liệu trong `D:\QL_NS\QuanLyNhanSu\DB_Doc` được ghi nhận tại đây.

---

## [Phiên bản 1.1.0] — 11/10/2026 (Hiệu chỉnh toàn diện theo Báo cáo Kiểm tra Độc lập)

### 1. Chuẩn hóa triệt để kiểu dữ liệu YAML Frontmatter & Chốt chặn Schema
- **Khắc phục 100% lỗi kiểu mảng (Array Typing):** 
  - Toàn bộ các mảng rỗng trong Frontmatter của 30 tài liệu (`legal_basis: []`, `benchmark_basis: []`, `source_urls: []`, `unresolved_fields: []`) được chuyển thành cú pháp mảng YAML rõ ràng `[]`, loại bỏ triệt để việc parse thành `None`/`null`.
  - Khắc phục 45 lỗi kiểu dữ liệu và đưa số lượng trường lệch giữa Frontmatter Markdown và `clauses.jsonl` từ 180 trường về **0 trường sai lệch**.
- **Chốt chặn Schema điều kiện (`allOf` Ingest Guard):**
  - Cập nhật cả `document.schema.json` và `clause.schema.json` với logic `allOf`: Nghiêm cấm mọi tài liệu hoặc điều khoản có trạng thái `DRAFT_NOT_APPROVED` hoặc `BENCHMARK_ONLY` bật `ingest_enabled: true`, gán `applies_to_current_company: true`, hoặc điền `approved_by`/`effective_from`.
  - Bổ sung đầy đủ các trường provenance và subchunk trong `clause.schema.json` và `clauses.jsonl`: `source_pages`, `chunk_index`, `total_chunks`, `content_hash`, `supersedes`.

### 2. Chuẩn hóa Phân vùng Mức lương Tối thiểu Bắc Ninh
- **Nguyên văn danh mục xã/phường ([PL-04/DIEU-02], [FAQ-01/Q-05]):**
  - Áp dụng chính xác Mục 11 Phụ lục Nghị định số 293/2025/NĐ-CP (hiệu lực từ 01/01/2026):
    - **Vùng II (4.730.000 đ/tháng; 22.700 đ/giờ):** Các phường thuộc TP. Bắc Ninh (Kinh Bắc, Võ Cường, Vũ Ninh, Hạp Lĩnh, Nam Sơn, Vân Dương, Khắc Niệm, Đại Phúc, Ninh Xá, Suối Hoa, Tiền An, Vệ An, Đáp Cầu, Thị Cầu, Kim Chân, Hòa Long, Phong Khê) và các xã/phường công nghiệp thuộc danh mục Vùng II của tỉnh Bắc Ninh tại Phụ lục NĐ 293/2025/NĐ-CP.
    - **Vùng III (4.140.000 đ/tháng; 20.000 đ/giờ):** Danh mục các xã cụ thể: **Lạng Giang, Mỹ Thái, Kép, Tân Dĩnh, Tiên Lục, Tân Yên, Ngọc Thiện, Nhã Nam, Phúc Hòa, Quang Trung, Hợp Thịnh, Hiệp Hòa, Hoàng Vân, Xuân Cẩm...**
    - **Vùng IV (3.700.000 đ/tháng; 17.800 đ/giờ):** Các xã, phường còn lại thuộc tỉnh Bắc Ninh không nằm trong danh mục Vùng II và Vùng III.
  - Loại bỏ hoàn toàn việc suy luận địa bàn theo tên huyện cũ Gia Bình/Lương Tài.
  - Quy định bắt buộc: Trợ lý AI khi tư vấn phải đưa ra bảng tra cứu theo xã/phường và **đặt câu hỏi xác nhận làm rõ địa điểm nơi làm việc thực tế** của người lao động tại Bắc Ninh, tuyệt đối không tự ấn định một vùng.

### 3. Đối chiếu Database & Phân quyền theo Source Code EF 6.5.1 / Oracle
- **Ánh xạ thực tế trong `QLNhanSu.edmx` và `AIEntities.edmx` ([DOI_CHIEU_DATABASE_VA_QUYEN.md]):**
  - Tài khoản người dùng lưu tại bảng `TB_SYS_USER` (không dùng tên giả định `TB_USER` hay `SYS_USER`).
  - Liên kết định danh `UserId` (`IDUSER`) và `MANV` qua bảng liên kết thực tế `TB_USER_EMPLOYEE_MAPPING`.
  - View hồ sơ nhân sự là `V_AI_EMPLOYEE` (không dùng tên giả định `V_AI_EMPLOYEE_PROFILE`).
  - Ghi nhận các bảng dữ liệu quẹt thẻ thô phần cứng chấm công là `[DỰ KIẾN / CHƯA MAP TRONG EDMX]`.
  - Chuẩn hóa contract Scope theo C#: `"SELF"`, `"DEPARTMENT"`, `"COMPANY"`, `"ALL"`, `"NONE"` (dùng `DEPARTMENT` thay cho `DEPT`).
- **Phân giải quyền động:**
  - Khẳng định chính sách `DENY` có quyền ưu tiên cao nhất, vượt trên Role.
  - Phản ánh trung thực việc 5 capability liên quan đến lương và quẹt thẻ (`PAYROLL_SELF`, `PAYROLL_VIEW`, `PAYROLL_SUMMARY`, `ATTENDANCE_DETAIL`, `INSURANCE_VIEW`) hiện đang **tạm khóa đối với toàn bộ người dùng (kể cả Admin/HR)** qua hàm `ApplyBranchB2Profile()`.

### 4. Kế hoạch tích hợp Qdrant & Reader Contract
- **Đính chính dẫn chứng mã nguồn ([KE_HOACH_TICH_HOP_QDRANT.md]):**
  - Sửa dẫn chứng dòng 398-405 trong `QdrantService.cs` là cơ chế ép kiểu lọc `companyId` (`int.TryParse` vs chuỗi), không phải lỗi Point ID gRPC.
  - Phân tích chính xác nguy cơ ghi đè điểm (Point Overwrite) trong `AddAsync` (dòng 152) do không truyền `chunkKey`, và đề xuất công thức sinh Point ID tất định chuẩn: `PointId = GeneratePointId(tag, sourceId, "PUBLIC", clauseId + ":" + chunkIndex)`.
  - Cập nhật payload nạp đưa mã điều khoản vào key **`section`** để khớp với Reader C# (`VectorHit.Section` đọc từ `payload["section"]` tại dòng 561).

### 5. Chuẩn hóa Bộ kiểm thử 55 Ca & Báo cáo chất lượng
- **Bộ kiểm thử ([test_cases.jsonl], [CAU_HOI_KIEM_THU.md]):**
  - 100% ca kiểm thử được trang bị Fixture cấu trúc đầy đủ (`actor_id`, `user_id`, `manv`, `role`, `department_id`, `company_id`, `capabilities`, `direct_scope`, `effective_scope`).
  - Phân loại rõ ràng căn cứ kiểm định qua trường `reference_type`:
    - `POLICY_CLAUSE`: Tham chiếu điều khoản pháp luật/quy chế (chỉ chứa `clause_id`);
    - `SYSTEM_SPEC`: Tham chiếu tài liệu đặc tả kỹ thuật CSDL/quyền (`DOI_CHIEU_DATABASE_VA_QUYEN.md`);
    - `HYBRID`: Kết hợp cả điều khoản hướng dẫn và đặc tả kỹ thuật.
  - Sửa nội dung kiểm thử phân vùng Bắc Ninh (TC-13), thưởng chuyên cần dự thảo (TC-20), cấm trừ lương (TC-24) và các ca kiểm thử bảo mật phân quyền.
- **Báo cáo kiểm tra ([BAO_CAO_KIEM_TRA.md]):**
  - Phân định 9 tiêu chí kiểm tra độc lập và ghi nhận đạt **9/9 tiêu chí (100.0%)**.
  - Đồng bộ phiên bản gói tri thức thành **v1.1.0** trong cả `manifest.json`.

---

## [Phiên bản 1.0.0] — 11/10/2026
- Khởi tạo bộ khung DataDoc ban đầu theo hướng dẫn `PROMPT_ANTIGRAVITY.md`.
