# Danh mục tổng thể tài liệu DataDoc HRMS (Master Document Catalog)

Ngày lập: 11/10/2026.
Tất cả tài liệu được phân loại theo hệ thống 3 trục độc lập (`source_kind`, `document_type`, `topics`).
Các tài liệu mẫu công ty ở trạng thái dự thảo (`DRAFT_NOT_APPROVED`), có `ingest_enabled: false` để phục vụ xem xét, phê duyệt trước khi nạp vào Qdrant.

---

## 1. Nhóm 01 — Tham chiếu Pháp luật Việt Nam (`01_Phap_luat_Viet_Nam/`)

| Mã tài liệu | Tên văn bản | Thể loại | Chủ đề chính | Nguồn căn cứ | Trạng thái |
|---|---|---|---|---|---|
| `PL-01` | [Quy định pháp luật về thời giờ làm việc và ca kíp](../01_Phap_luat_Viet_Nam/PL-01-Thoi-gio-lam-viec-va-ca.md) | `LAW` | `SHIFT`, `ATTENDANCE` | BLLĐ 2019 (18/VBHN-VPQH), Điều 105–111 | `LEGAL_REFERENCE` |
| `PL-02` | [Quy định pháp luật về làm thêm giờ và làm việc ban đêm](../01_Phap_luat_Viet_Nam/PL-02-Lam-them-gio-va-luong-dem.md) | `LAW` | `OVERTIME`, `PAYROLL` | BLLĐ 2019, Điều 98, 107, 108; NĐ 145/2020 | `LEGAL_REFERENCE` |
| `PL-03` | [Quy định pháp luật về nghỉ phép năm, nghỉ lễ và việc riêng](../01_Phap_luat_Viet_Nam/PL-03-Nghi-phep-va-ngay-le.md) | `LAW` | `LEAVE`, `HOLIDAY` | BLLĐ 2019, Điều 112–115 | `LEGAL_REFERENCE` |
| `PL-04` | [Mức lương tối thiểu vùng tại Bắc Ninh từ năm 2026](../01_Phap_luat_Viet_Nam/PL-04-Luong-toi-thieu-Bac-Ninh.md) | `LAW` | `PAYROLL` | Nghị định 293/2025/NĐ-CP & Phụ lục | `LEGAL_REFERENCE` |
| `PL-05` | [Chế độ nâng lương và tiền thưởng theo pháp luật](../01_Phap_luat_Viet_Nam/PL-05-Che-do-nang-luong-va-thuong.md) | `LAW` | `SALARY_REVIEW`, `BONUS` | BLLĐ 2019, Điều 103, 104 | `LEGAL_REFERENCE` |
| `PL-06` | [Nội quy lao động, trình tự kỷ luật và cấm chế tài phạt tiền](../01_Phap_luat_Viet_Nam/PL-06-Noi-quy-va-ky-luat-lao-dong.md) | `LAW` | `DISCIPLINE`, `CONDUCT` | BLLĐ 2019, Điều 118–128 (Đặc biệt Đ.127) | `LEGAL_REFERENCE` |

---

## 2. Nhóm 02 — Tài liệu tham khảo Foxconn / Hon Hai (`02_Tham_khao_Foxconn/`)

| Mã tài liệu | Tên văn bản | Thể loại | Chủ đề chính | Ghi chú phạm vi | Trạng thái |
|---|---|---|---|---|---|
| `FC-01` | [Tổng quan nguồn và phạm vi tham khảo Foxconn](../02_Tham_khao_Foxconn/FC-01-Tong-quan-nguon-va-pham-vi-tham-khao.md) | `POLICY` | `CONDUCT`, `OVERTIME` | Phân định tham khảo, không áp đặt cho công ty | `EXTERNAL_COMPANY_REFERENCE` |
| `FC-02` | [Quy tắc ứng xử và bảo vệ quyền người lao động Foxconn](../02_Tham_khao_Foxconn/FC-02-Quy-tac-ung-xu-va-quyen-nguoi-lao-dong.md) | `POLICY` | `CONDUCT`, `OCCUPATIONAL_SAFETY` | Tóm lược Global Code of Conduct 2021 | `EXTERNAL_COMPANY_REFERENCE` |
| `FC-03` | [Khoảng trống thông tin chính sách nhà máy Bắc Ninh](../02_Tham_khao_Foxconn/FC-03-Khoang-trong-chinh-sach-nha-may-Bac-Ninh.md) | `FAQ` | `BONUS`, `BENEFITS` | Báo cáo các nội dung chưa xác minh công khai | `EXTERNAL_COMPANY_REFERENCE` |

---

## 3. Nhóm 03 — Mẫu quy định và quy chế công ty (`03_Mau_quy_dinh_cong_ty/`)

| Mã tài liệu | Tên văn bản | Thể loại | Chủ đề chính | Tình trạng dữ liệu | Trạng thái phê duyệt |
|---|---|---|---|---|---|
| `CT-01` | [Nội quy làm việc theo ca và bàn giao ca](../03_Mau_quy_dinh_cong_ty/CT-01-Noi-quy-lam-viec-theo-ca-va-ban-giao.md) | `INTERNAL_RULE` | `SHIFT`, `HANDOVER` | Chờ điền lịch ca cụ thể | `DRAFT_NOT_APPROVED` |
| `CT-02` | [Quy chế chấm công, đi muộn về sớm và xử lý sự cố công](../03_Mau_quy_dinh_cong_ty/CT-02-Quy-che-cham-cong-va-sua-cong.md) | `REGULATION` | `ATTENDANCE` | Chờ duyệt hạn chốt sửa công | `DRAFT_NOT_APPROVED` |
| `CT-03` | [Quy chế làm thêm giờ và quy trình phê duyệt OT](../03_Mau_quy_dinh_cong_ty/CT-03-Quy-che-lam-them-gio-va-phe-duyet-OT.md) | `REGULATION` | `OVERTIME`, `APPROVAL_AUTHORITY` | Tuân thủ trần pháp lý BLLĐ | `DRAFT_NOT_APPROVED` |
| `CT-04` | [Quy chế nghỉ phép năm, đổi ca và nghỉ ngày lễ](../03_Mau_quy_dinh_cong_ty/CT-04-Quy-che-nghi-phep-doi-ca-va-nghi-le.md) | `REGULATION` | `LEAVE`, `HOLIDAY`, `SHIFT` | Chờ duyệt thời hạn báo trước | `DRAFT_NOT_APPROVED` |
| `CT-05` | [Quy chế trả lương và đối soát bảng lương](../03_Mau_quy_dinh_cong_ty/CT-05-Quy-che-tra-luong-va-doi-soat-bang-luong.md) | `REGULATION` | `PAYROLL` | Chờ xác nhận ngày trả lương | `DRAFT_NOT_APPROVED` |
| `CT-06` | [Quy chế nâng lương, chuyển bậc và tiêu chuẩn xét duyệt](../03_Mau_quy_dinh_cong_ty/CT-06-Quy-che-nang-luong-va-xet-bac.md) | `REGULATION` | `SALARY_REVIEW`, `PERFORMANCE` | Mức tiền null, chờ duyệt KPI | `DRAFT_NOT_APPROVED` |
| `CT-07` | [Quy chế khen thưởng và các chương trình tiền thưởng](../03_Mau_quy_dinh_cong_ty/CT-07-Quy-che-khen-thuong-va-phuc-loi.md) | `REGULATION` | `BONUS`, `BENEFITS` | Mức tiền null, tách từng chương trình | `DRAFT_NOT_APPROVED` |
| `CT-08` | [Quy chế xử lý kỷ luật lao động và trách nhiệm vật chất](../03_Mau_quy_dinh_cong_ty/CT-08-Quy-che-ky-luat-va-trach-nhiem-vat-chat.md) | `REGULATION` | `DISCIPLINE`, `CONDUCT` | Cấm phạt tiền; đúng 4 hình thức luật | `DRAFT_NOT_APPROVED` |
| `CT-09` | [Chính sách phúc lợi ăn ca, xe đưa đón và hỗ trợ đời sống](../03_Mau_quy_dinh_cong_ty/CT-09-Chinh-sach-an-ca-xe-va-ho-tro-doi-song.md) | `POLICY` | `BENEFITS` | Chờ công ty quyết định áp dụng | `DRAFT_NOT_APPROVED` |
| `CT-10` | [Quy tắc ứng xử, an toàn thông tin và tiếp nhận phản ánh](../03_Mau_quy_dinh_cong_ty/CT-10-Quy-tac-ung-xu-bao-mat-va-khieu-nai.md) | `INTERNAL_RULE` | `CONDUCT`, `SECURITY`, `GRIEVANCE` | Kênh tiếp nhận ẩn danh bảo mật | `DRAFT_NOT_APPROVED` |

---

## 4. Nhóm 04 — Quy trình, Biểu mẫu và FAQ (`04_Quy_trinh_Bieu_mau_FAQ/`)

| Mã tài liệu | Tên văn bản | Thể loại | Chủ đề chính | Mục đích sử dụng |
|---|---|---|---|---|
| `BM-01` | [Phiếu yêu cầu sửa công và giải trình](../04_Quy_trinh_Bieu_mau_FAQ/BM-01-Phieu-yeu-cau-sua-cong-va-giai-trinh.md) | `FORM_TEMPLATE` | `ATTENDANCE_TIMEKEEPING` | Quên quẹt thẻ, lỗi máy quẹt |
| `BM-02` | [Đơn xin nghỉ phép và hoán đổi ca làm việc](../04_Quy_trinh_Bieu_mau_FAQ/BM-02-Don-xin-nghi-phep-va-hoan-doi-ca.md) | `FORM_TEMPLATE` | `LEAVE_POLICY`, `SHIFT_MANAGEMENT` | Nghỉ phép năm, việc riêng, đổi ca |
| `BM-03` | [Phiếu đăng ký và xác nhận làm thêm giờ](../04_Quy_trinh_Bieu_mau_FAQ/BM-03-Phieu-dang-ky-va-xac-nhan-lam-them-gio.md) | `FORM_TEMPLATE` | `OVERTIME_POLICY`, `SHIFT_MANAGEMENT` | Thỏa thuận tự nguyện OT theo luật |
| `BM-04` | [Phiếu đề nghị khen thưởng và nâng bậc lương](../04_Quy_trinh_Bieu_mau_FAQ/BM-04-Phieu-de-nghi-khen-thuong-va-nang-bac.md) | `FORM_TEMPLATE` | `BONUS_POLICY`, `SALARY_INCREASE` | Đề xuất thành tích định kỳ |
| `BM-05` | [Biên bản sự việc và bản giải trình vi phạm nội quy](../04_Quy_trinh_Bieu_mau_FAQ/BM-05-Bien-ban-su-viec-va-ban-giai-trinh-vi-pham.md) | `FORM_TEMPLATE` | `DISCIPLINE_POLICY` | Ghi nhận sự việc, quyền giải trình |
| `BM-06` | [Biên bản bàn giao ca làm việc và thiết bị](../04_Quy_trinh_Bieu_mau_FAQ/BM-06-Bien-ban-ban-giao-ca-va-thiet-bi.md) | `FORM_TEMPLATE` | `SHIFT_MANAGEMENT`, `OCCUPATIONAL_SAFETY` | Bàn giao khi chuyển ca sản xuất |
| `BM-07` | [Phiếu khiếu nại và phản ánh chế độ nhân sự](../04_Quy_trinh_Bieu_mau_FAQ/BM-07-Phieu-khieu-nai-va-phan-anh-che-do.md) | `FORM_TEMPLATE` | `WHISTLEBLOWING`, `LABOR_DISPUTES` | Kênh phản ánh bảo mật, không trù dập |
| `SOP-01` | [Quy trình chốt bảng công và tính lương hàng tháng](../04_Quy_trinh_Bieu_mau_FAQ/SOP-01-Quy-trinh-chot-bang-cong-va-tinh-luong.md) | `PROCEDURE` | `ATTENDANCE_TIMEKEEPING`, `SALARY_POLICY` | Chu trình đóng kỳ công hàng tháng |
| `SOP-02` | [Quy trình phê duyệt và giám sát làm thêm giờ](../04_Quy_trinh_Bieu_mau_FAQ/SOP-02-Quy-trinh-phe-duyet-va-giam-sat-OT.md) | `PROCEDURE` | `OVERTIME_POLICY`, `SHIFT_MANAGEMENT` | Trình ký và hậu kiểm dữ liệu OT |
| `FAQ-01` | [Bộ câu hỏi thường gặp về chính sách nhân sự](../04_Quy_trinh_Bieu_mau_FAQ/FAQ-01-Hoi-dap-chinh-sach-nhan-su.md) | `FAQ` | Đa chủ đề | Giải đáp thắc mắc người lao động |
| `HD-01` | [Hướng dẫn thao tác tra cứu trên hệ thống HRMS](../04_Quy_trinh_Bieu_mau_FAQ/HD-01-Huong-dan-thao-tac-tren-he-thong-HRMS.md) | `USER_GUIDE` | `SYSTEM_GUIDE` | Hướng dẫn sử dụng Desktop/Web thật |

---

## 5. Nhóm 06 & 07 — RAG & Kiểm thử (`06_RAG/`, `07_Kiem_thu/`)

- `06_RAG/document.schema.json`: JSON Schema xác thực tài liệu cấp độ Document.
- `06_RAG/clause.schema.json`: JSON Schema xác thực từng điều khoản cấp độ Clause/Chunk.
- `06_RAG/manifest.json`: Manifest toàn bộ các tài liệu kèm hash SHA-256.
- `06_RAG/clauses.jsonl`: Dữ liệu điều khoản chi tiết chuẩn hóa theo schema.
- `06_RAG/HUONG_DAN_CHUNK_VA_TRUY_XUAT.md`: Hướng dẫn chunking và prompt RAG.
- `06_RAG/DOI_CHIEU_DATABASE_VA_QUYEN.md`: Bảng đối chiếu điều khoản $\leftrightarrow$ CSDL Oracle $\leftrightarrow$ Qdrant $\leftrightarrow$ Phân quyền AI.
- `06_RAG/KE_HOACH_TICH_HOP_QDRANT.md`: Kế hoạch tích hợp Qdrant, schema migration và cache.
- `07_Kiem_thu/test_cases.jsonl`: 50+ test cases kiểm thử RAG chi tiết dạng JSONL.
- `07_Kiem_thu/CAU_HOI_KIEM_THU.md`: Bảng tra cứu các tình huống kiểm thử.
- `07_Kiem_thu/BAO_CAO_KIEM_TRA.md`: Báo cáo xác thực tĩnh (Static Validation Report).
