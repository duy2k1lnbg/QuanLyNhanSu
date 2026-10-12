# Kho tri thức HRMS DataDoc (Quy chế nhân sự & Căn cứ pháp lý)

Ngữ cảnh: Nhà máy sản xuất, chế tạo linh kiện điện tử tại Bắc Ninh, Việt Nam, làm việc theo ca kíp; tài liệu tham khảo đối sánh quốc tế: Foxconn/Hon Hai Code of Conduct 2021. 
Phiên bản kho tri thức: **v1.1.0** (Cập nhật ngày 11/10/2026).

---

## 1. Cấu trúc thư mục kho tri thức DataDoc

- **[00_Quan_tri/](./00_Quan_tri/)**: Quản trị danh mục, phân loại 3 trục (`PHAN_LOAI_DATADOC.md`), báo cáo kiểm kê mã nguồn (`KIEM_KE_VA_KHOANG_TRONG.md`), danh mục nguồn và nhật ký thay đổi (`NHAT_KY_THAY_DOI.md`).
- **[01_Phap_luat_Viet_Nam/](./01_Phap_luat_Viet_Nam/)**: 6 văn bản pháp luật lao động cốt lõi (PL-01 đến PL-06: thời giờ làm việc, OT & làm đêm 2 nhánh Điều 57 NĐ 145, nghỉ phép & ngày lễ, lương tối thiểu Vùng II & Vùng III Bắc Ninh theo NĐ 293/2025/NĐ-CP, chế độ nâng lương/thưởng, nội quy & 4 hình thức kỷ luật hợp pháp, cấm phạt trừ lương).
- **[02_Tham_khao_Foxconn/](./02_Tham_khao_Foxconn/)**: 3 tài liệu tham khảo đối sánh ngoại bộ (FC-01 đến FC-03: Code of Conduct 2021, trần 60h/tuần không được phá vỡ trần luật VN, cấm lao động trẻ em & cưỡng bức, khoảng trống chính sách nhà máy).
- **[03_Mau_quy_dinh_cong_ty/](./03_Mau_quy_dinh_cong_ty/)**: 10 bản dự thảo quy chế công ty (CT-01 đến CT-10: làm việc theo ca, chấm công & sửa công, phê duyệt OT, nghỉ phép & đổi ca, trả lương & đối soát, nâng lương & bậc nghề, khen thưởng & chuyên cần, kỷ luật & bồi thường vật chất, ăn ca & xe đưa đón, quy tắc ứng xử & bảo mật). Toàn bộ ở trạng thái `DRAFT_NOT_APPROVED`, `applies_to_current_company: false`, `ingest_enabled: false`.
- **[04_Quy_trinh_Bieu_mau_FAQ/](./04_Quy_trinh_Bieu_mau_FAQ/)**: 7 biểu mẫu hành chính (BM-01 đến BM-07), 2 SOP chốt công & duyệt OT (SOP-01, SOP-02), sổ tay hỏi đáp chế độ (FAQ-01) và hướng dẫn thao tác theo menu thực tế của HRMS.Web/Desktop (HD-01).
- **[05_Nguon_goc/](./05_Nguon_goc/)**: 4 tệp PDF gốc lưu trữ văn bản pháp luật và quy tắc ứng xử đối sánh kèm mã băm SHA-256.
- **[06_RAG/](./06_RAG/)**: Hệ thống JSON Schema (`document.schema.json`, `clause.schema.json`), `manifest.json`, `clauses.jsonl` (123 điều khoản trích xuất nguyên khối), hướng dẫn chunking & payload tương thích reader C#, đối chiếu database Oracle / EF 6.5.1 (`DOI_CHIEU_DATABASE_VA_QUYEN.md`) và kế hoạch nâng cấp Qdrant (`KE_HOACH_TICH_HOP_QDRANT.md`).
- **[07_Kiem_thu/](./07_Kiem_thu/)**: Bộ 55 ca kiểm thử thiết kế đa chiều (`test_cases.jsonl`, `CAU_HOI_KIEM_THU.md`) và báo cáo kiểm tra chất lượng tĩnh (`BAO_CAO_KIEM_TRA.md`).

---

## 2. Nguyên tắc an toàn dữ liệu và ranh giới thông tin bắt buộc

1. **Chế độ cách ly an toàn (Ingest Disabled):** Toàn bộ 123 điều khoản hiện đều được cấu hình `ingest_enabled: false`. Tuyệt đối không nạp tự động lên collection `hrms_vectors_v2` đang phục vụ live khi chưa có quyết định phê duyệt bằng văn bản của Ban Giám đốc và Giám đốc Nhân sự.
2. **Không tự ý ấn định số liệu chưa xác nhận:** Mọi thông tin công ty chưa chốt (mức thưởng chuyên cần, ngày trả lương cố định, định mức suất ăn ca, phụ cấp xăng xe/nhà ở) đều được giữ `null` trong JSON/JSONL và gắn nhãn `[CẦN CÔNG TY XÁC NHẬN]`. Tuyệt đối không tự bịa mức 300.000 đồng hay bất kỳ con số nào.
3. **Tuân thủ tuyệt đối pháp luật lao động:** Không chấp nhận hình thức phạt tiền trừ lương (vi phạm Điều 127 BLLĐ); không có hình thức kỷ luật "Cảnh cáo" (trái Điều 124 BLLĐ); phân vùng lương tối thiểu Bắc Ninh tuân thủ theo danh mục xã/phường của Nghị định 293/2025/NĐ-CP (Vùng III lương giờ là 20.000 đ/giờ).
4. **Phân quyền và bảo vệ quyền riêng tư:** Chặn tuyệt đối nhân viên tra cứu tiền lương, quẹt thẻ hoặc hồ sơ của người khác; phản ánh trung thực trạng thái backend hiện tại (các capability liên quan đến tiền lương `PAYROLL_SELF`, `PAYROLL_VIEW` đang tạm khóa trong backend C# vì lý do bảo mật).
