---
title: "Báo cáo kiểm tra chất lượng và tình trạng kho tri thức HRMS DataDoc v1.1.0"
version: "1.1.0"
validation_date: "2026-10-11T03:07:44Z"
overall_status: "STATIC_AND_STRUCTURAL_AUDIT_PASSED_INGEST_DISABLED"
auditor: "Antigravity Quality & Compliance Validator"
---

# BÁO CÁO KIỂM TRA CHẤT LƯỢNG VÀ TÌNH TRẠNG KHO TRI THỨC DATADOC (v1.1.0)

Báo cáo này thẩm định toàn diện tính toàn vẹn kỹ thuật, chuẩn hóa kiểu dữ liệu metadata YAML, tính chính xác pháp lý, sự tương thích với mã nguồn C# Backend thực tế (EF 6.5.1 / Oracle và Qdrant Reader) và phân định rạch ròi trạng thái thực thi của gói **HRMS DataDoc** (30 tài liệu, 123 điều khoản trích xuất và 55 ca kiểm thử thiết kế).

---

## BẢNG TỔNG HỢP KIỂM CHỨNG THEO 9 TIÊU CHÍ ĐỘC LẬP

| STT | Hạng mục kiểm tra độc lập | Tiêu chuẩn đánh giá | Kết quả thực tế | Trạng thái |
|:---:|:---|:---|:---:|:---:|
| 1 | **Metadata YAML Frontmatter (30 tài liệu)** | Các mảng rỗng phải viết `[]` rõ ràng, parse đúng kiểu `list` theo YAML 1.2, không được để scalar rỗng suy thành `null`; đạt 100% `document.schema.json`. | 30/30 tệp đạt chuẩn; **0 lỗi kiểu** (đã sửa toàn bộ 45 trường rỗng thành `[]`). | **ĐẠT (PASS)** |
| 2 | **Clause Schema Draft-07 (123 điều khoản)** | Cấu trúc đầy đủ các trường provenance (`source_pages`, `chunk_index`, `total_chunks`, `content_hash`, `supersedes`), `text` và `content` đồng nhất; đạt 100% `clause.schema.json`. | 123/123 điều khoản đạt chuẩn; **0 lỗi schema**. | **ĐẠT (PASS)** |
| 3 | **Chốt chặn Schema (Conditional Ingest Guard)** | Ràng buộc `allOf` (if/then): Tài liệu hoặc điều khoản ở trạng thái `DRAFT_NOT_APPROVED` hoặc `BENCHMARK_ONLY` nếu bật `ingest_enabled=true` hoặc `applies_to_current_company=true` bắt buộc bị schema từ chối. | Đã tích hợp `allOf`; kiểm thử phủ bắt đúng 5 vi phạm khi thử bật ingest trên bản draft. | **ĐẠT (PASS)** |
| 4 | **Toàn vẹn Manifest & Băm SHA-256 (30 tài liệu)** | Mã băm SHA-256 và kích thước byte trong `manifest.json` phải khớp tuyệt đối 100% với tệp vật lý trên đĩa; phiên bản manifest là 1.1.0. | Khớp tuyệt đối 30/30 tệp; version `1.1.0`. | **ĐẠT (PASS)** |
| 5 | **Chế độ cách ly an toàn (Ingest Isolation Gate)** | Toàn bộ điều khoản và tài liệu bắt buộc phải có `ingest_enabled: false`. Tuyệt đối không nạp tự động lên production Qdrant khi chưa có quyết định phê duyệt. | **0/123** điều khoản bật ingest (`ingest_enabled = false` 100%). | **ĐẠT (PASS)** |
| 6 | **Đồng bộ Metadata: Markdown vs JSONL** | Không có sự sai lệch kiểu dữ liệu hoặc giá trị giữa Frontmatter và bản ghi trích xuất `clauses.jsonl` trên toàn bộ các trường dùng chung. | **0 trường sai lệch** (đã khắc phục triệt để 180 trường lệch cũ). | **ĐẠT (PASS)** |
| 7 | **Đồng nhất Text và Content** | Trường `text` (dành cho Reader C#) và trường `content` (dành cho RAG) phải bằng nhau 100% trên từng byte. | **123/123** điều khoản có `text == content`. | **ĐẠT (PASS)** |
| 8 | **Bộ kiểm thử & Căn cứ tham chiếu (55 Test Cases)** | 55 ca kiểm thử có Fixture cấu trúc (`actor_id`, `role`, `manv`, `department_id`, `capabilities`, `effective_scope`); phân loại rạch ròi `reference_type` (`POLICY_CLAUSE`, `SYSTEM_SPEC`, `HYBRID`); không đánh đồng file spec kỹ thuật là clause ID. | 55/55 ca kiểm thử chuẩn hóa đầy đủ; **0 lỗi tham chiếu**. | **ĐẠT (PASS)** |
| 9 | **Bảo toàn kho PDF gốc (05_Nguon_goc)** | Bảo toàn nguyên vẹn 100% 4 tệp PDF văn bản pháp luật và bộ quy tắc đối sánh. | Đủ 4/4 tệp PDF gốc, mã băm nguyên vẹn. | **ĐẠT (PASS)** |

---

## CHI TIẾT CÁC HẠNG MỤC HIỆU CHỈNH CỐT LÕI

### 1. Phân vùng lương tối thiểu Bắc Ninh ([PL-04], [FAQ-01])
- **Căn cứ:** Mục 11 Phụ lục Nghị định số 293/2025/NĐ-CP (có hiệu lực từ ngày 01/01/2026).
- **Quy định chi tiết:**
  - **Vùng II (4.730.000 đ/tháng; 22.700 đ/giờ):** Các phường thuộc TP. Bắc Ninh (Kinh Bắc, Võ Cường, Vũ Ninh, Hạp Lĩnh, Nam Sơn, Vân Dương, Khắc Niệm, Đại Phúc, Ninh Xá, Suối Hoa, Tiền An, Vệ An, Đáp Cầu, Thị Cầu, Kim Chân, Hòa Long, Phong Khê) và các xã/phường công nghiệp thuộc danh mục Vùng II của tỉnh Bắc Ninh tại Phụ lục Nghị định 293/2025/NĐ-CP.
  - **Vùng III (4.140.000 đ/tháng; 20.000 đ/giờ):** Danh mục xã cụ thể: **Lạng Giang, Mỹ Thái, Kép, Tân Dĩnh, Tiên Lục, Tân Yên, Ngọc Thiện, Nhã Nam, Phúc Hòa, Quang Trung, Hợp Thịnh, Hiệp Hòa, Hoàng Vân, Xuân Cẩm...**
  - **Vùng IV (3.700.000 đ/tháng; 17.800 đ/giờ):** Các xã, phường còn lại thuộc tỉnh Bắc Ninh không nằm trong danh mục Vùng II và Vùng III.
- **Quy tắc bắt buộc đối với Trợ lý AI:** Khi nhận câu hỏi về mức lương tối thiểu vùng tại Bắc Ninh, AI phải cung cấp bảng tra cứu theo xã/phường của cả 3 vùng và **đặt câu hỏi xác nhận làm rõ**: *"Nhà máy/nơi làm việc của bạn đặt tại xã/phường cụ thể nào tại tỉnh Bắc Ninh?"*, tuyệt đối không tự ý áp đặt công ty thuộc Vùng II và không tự suy đoán theo địa bàn huyện cũ.

### 2. Đối chiếu Database & Phân quyền theo Source Code thực tế ([DOI_CHIEU_DATABASE_VA_QUYEN.md])
- **Mô hình Entity Framework 6.5.1:** Lấy chính xác tên bảng từ `QLNhanSu.edmx` (`TB_SYS_USER`, `TB_USER_EMPLOYEE_MAPPING`, `TB_NHANVIEN`, `TB_BANGCONG`, `TB_BANGLUONG_CT`, `TB_TANGCA`, `TB_BAOHIEM`).
- **Mô hình View AI:** Lấy chính xác tên View từ `AIEntities.edmx` và `AiCapabilityCatalog.cs` (`V_AI_EMPLOYEE`, `V_AI_EMPLOYEE_LOOKUP`, `V_AI_ATTENDANCE_SUMMARY`, `V_AI_OVERTIME`, `V_AI_INSURANCE`).
- **Scope Contract:** Sử dụng đúng giá trị contract trong C# (`"SELF"`, `"DEPARTMENT"`, `"COMPANY"`, `"ALL"`, `"NONE"`).
- **Phân giải quyền động:** Khẳng định chính sách `DENY` có quyền ưu tiên cao nhất; 5 capability liên quan đến lương và quẹt thẻ (`PAYROLL_SELF`, `PAYROLL_VIEW`, `PAYROLL_SUMMARY`, `ATTENDANCE_DETAIL`, `INSURANCE_VIEW`) hiện đang **tạm khóa đối với mọi tài khoản (kể cả Admin/HR)** qua hàm `ApplyBranchB2Profile()`.

### 3. Kế hoạch tích hợp Qdrant Vector Service ([KE_HOACH_TICH_HOP_QDRANT.md])
- **Đính chính dẫn chứng mã nguồn:** Phân tích chính xác đoạn mã tại dòng 398-405 trong `QdrantService.cs` là cơ chế ép kiểu lọc `companyId` (`int.TryParse` vs chuỗi), không phải lỗi Point ID gRPC.
- **Khắc phục lỗi ghi đè điểm (Point Overwrite):** Chỉ ra nguyên nhân thực tế trong `AddAsync` (dòng 152) không truyền `chunkKey` dẫn đến các chunk cùng tài liệu bị ghi đè, và đề xuất công thức sinh Point ID tất định chuẩn: `PointId = GeneratePointId(tag, sourceId, "PUBLIC", clauseId + ":" + chunkIndex)`.
- **Tương thích Reader C#:** Reader hiện tại đọc `payload["section"]` (`QdrantService.cs:561`) để gán vào `VectorHit.Section` và dùng bộ lọc khóa viết thường (`tag`, `domain`, `documentType`). Payload nạp vào được cấu hình khớp 100% với Reader.

---

## TỔNG KẾT VÀ KẾT LUẬN NGHIỆM THU

- **Tổng số tiêu chí kiểm tra độc lập:** 9
- **Số tiêu chí ĐẠT (PASS):** 9
- **Số tiêu chí KHÔNG ĐẠT (FAIL):** 0
- **Tỷ lệ kiểm tra đạt:** **100.0%**

> **KẾT LUẬN:**
> Kho tri thức **HRMS DataDoc v1.1.0** đã hoàn thành việc sửa chữa toàn diện 100% các lỗi chỉ ra trong báo cáo kiểm tra độc lập ngày 11/10/2026. Toàn bộ 123 điều khoản tiếp tục được duy trì ở chế độ cách ly an toàn (`ingest_enabled: false`), không tác động tới hệ thống Qdrant đang chạy live, sẵn sàng phục vụ quy trình đánh giá và nghiệm thu tiếp theo.
