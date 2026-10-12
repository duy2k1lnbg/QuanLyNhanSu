---
doc_id: "BM-02"
title: "Đơn xin nghỉ phép và hoán đổi ca làm việc (môi trường demo)"
version: "2.0.0-demo"
status: "ACTIVE_OFFICIAL"
effective_from: "2026-10-12"
effective_to: null
approved_by: "Chủ sở hữu hệ thống demo — yêu cầu phê duyệt qua Codex ngày 12/10/2026"
company_id: "1"
source_kind: "INTERNAL_PROCEDURE"
document_type: "FORM_TEMPLATE"
topics: ["LEAVE_POLICY", "SHIFT_MANAGEMENT"]
jurisdiction: "VN"
applicable_location: "Bắc Ninh"
applicability_mode: "ALL"
access_profile: "PUBLIC_INTERNAL"
applies_to_current_company: true
ingest_enabled: true
requires_hr_approval_for_live: true
legal_basis: ["PL-03: Bộ luật Lao động 2019 (Điều 113, 115 - Nghỉ phép năm, nghỉ việc riêng)", "CT-04: Quy chế nghỉ phép, đổi ca và nghỉ ngày Lễ Tết (Dự thảo)"]
benchmark_basis: []
source_urls: []
unresolved_fields: []
allowed_role_ids: []
allowed_department_ids: []
---

# BIỂU MẪU: ĐƠN XIN NGHỈ PHÉP VÀ HOÁN ĐỔI CA (BM-02) — MÔI TRƯỜNG DEMO

> **Phê duyệt trong hệ thống demo:** Công ty mã `1`, hiệu lực `12/10/2026`, theo yêu cầu của chủ sở hữu hệ thống. Không xác nhận đã đăng ký nội quy với cơ quan nhà nước. Mức tiền cá nhân dùng hợp đồng/chính sách còn hiệu lực đã duyệt; thông tin chưa được cấu hình phải liên hệ bộ phận phụ trách.

**Hướng dẫn áp dụng chung:** Nghỉ 01 ngày báo trước 24 giờ; nghỉ 02-03 ngày báo trước 03 ngày làm việc; trên 03 ngày báo trước 07 ngày làm việc. Đổi ca báo trước 12 giờ, vận hành demo 04 lần/tháng; ngoại lệ khẩn cấp do HR xem xét.

### [BM-02/MUC-01] Mẫu đơn đề nghị nghỉ phép / hoán đổi ca

```markdown
CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
----------------------------

ĐƠN ĐỀ NGHỊ NGHỈ PHÉP / HOÁN ĐỔI CA LÀM VIỆC
(Ban hành kèm theo Quy chế CT-04)

Kính gửi: 
  - Tổ trưởng / Trưởng bộ phận: ................................................
  - Phòng Quản lý Sản xuất: ....................................................
  - Phòng Nhân sự & Tiền lương: ................................................

I. THÔNG TIN NGƯỜI LÀM ĐƠN:
- Họ và tên: ............................................ Mã NV: ................
- Bộ phận / Chuyền sản xuất: ........................... Chức vụ: ...............

II. NỘI DUNG ĐỀ NGHỊ (Chọn 1 trong 2 mục A hoặc B):

[ ] MỤC A: ĐỀ NGHỊ NGHỈ PHÉP
1. Loại nghỉ phép:
   [ ] Nghỉ phép hàng năm (Phép năm)
   [ ] Nghỉ việc riêng có hưởng lương (Kết hôn, tang chế theo Điều 115 BLLĐ)
   [ ] Nghỉ việc riêng không hưởng lương (Theo thỏa thuận)
   [ ] Nghỉ ốm đau / Thai sản (Kèm theo giấy chứng nhận của cơ sở y tế)
2. Thời gian nghỉ:
   - Từ: ..... giờ ..... phút, ngày ...../...../202...
   - Đến: ..... giờ ..... phút, ngày ...../...../202...
   - Tổng số ngày nghỉ đề nghị: ........... ngày.
3. Lý do nghỉ: .................................................................
4. Bàn giao công việc cho nhân sự: (Họ tên - Mã NV) .............................

[ ] MỤC B: ĐỀ NGHỊ HOÁN ĐỔI CA LÀM VIỆC
1. Thông tin người đổi ca (Người B đồng ý đổi):
   - Họ và tên người B: ................................... Mã NV: ................
   - Vị trí / Kỹ năng đảm nhiệm: ................................................
2. Lịch đổi chi tiết:
   - Ngày đổi 1: Ngày ...../...../202...
     + Ca phân công gốc của Người A: Ca ..... -> Người B làm thay.
   - Ngày đổi 2 (Trả ca): Ngày ...../...../202...
     + Ca phân công gốc của Người B: Ca ..... -> Người A làm thay.
3. Lý do đổi ca: ...............................................................

III. Ý KIẾN VÀ KÝ DUYỆT:

Người đề nghị           Người đổi ca (Mục B)     Quản lý trực tiếp        Phòng Nhân sự
(Ký, ghi rõ họ tên)     (Ký, ghi rõ họ tên)      (Ký, ghi rõ họ tên)      (Ký, duyệt)
  
....................    ....................     ....................     .............
Ngày: ..../..../202.    Ngày: ..../..../202.     Ngày: ..../..../202.     Ngày: .../...
```

---

### [BM-02/MUC-02] Quy định áp dụng và kiểm soát
1. **Kiểm tra quỹ phép năm:** Trước khi nộp đơn nghỉ phép năm, người lao động phải tra cứu số dư phép năm khả dụng trên HRMS. Nghỉ quá số ngày phép tích lũy sẽ chuyển sang hình thức nghỉ không lương.
2. **Bảo đảm an toàn giãn cách ca khi đổi ca:** Việc đổi ca phải bảo đảm thời gian nghỉ giữa 02 ca làm việc liên tiếp tối thiểu 12 giờ theo Điều 110 BLLĐ 2019. Tổ trưởng có trách nhiệm kiểm tra lịch đổi ca, không phê duyệt nếu việc đổi ca tạo ra lịch làm việc quá 12 giờ liên tục.
