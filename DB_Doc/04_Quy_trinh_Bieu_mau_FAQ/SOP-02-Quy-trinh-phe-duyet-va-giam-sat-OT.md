---
doc_id: "SOP-02"
title: "Quy trình thao tác chuẩn: Phê duyệt và giám sát làm thêm giờ (OT) (môi trường demo)"
version: "2.0.0-demo"
status: "ACTIVE_OFFICIAL"
effective_from: "2026-10-12"
effective_to: null
approved_by: "Chủ sở hữu hệ thống demo — yêu cầu phê duyệt qua Codex ngày 12/10/2026"
company_id: "1"
source_kind: "INTERNAL_PROCEDURE"
document_type: "PROCEDURE"
topics: ["OVERTIME_POLICY", "SHIFT_MANAGEMENT", "OCCUPATIONAL_SAFETY"]
jurisdiction: "VN"
applicable_location: "Bắc Ninh"
applicability_mode: "ALL"
access_profile: "PUBLIC_INTERNAL"
applies_to_current_company: true
ingest_enabled: true
requires_hr_approval_for_live: true
legal_basis: ["PL-02: Bộ luật Lao động 2019 (Điều 107 - Làm thêm giờ; Điều 137, 160 - Đối tượng bảo vệ)", "Nghị định 145/2020/NĐ-CP (Điều 59, 62)", "CT-03: Quy chế làm thêm giờ và phê duyệt OT (Dự thảo)"]
benchmark_basis: ["FC-02: Foxconn Global Code of Conduct 2021 (Giới hạn tối đa 60 giờ làm việc/tuần bao gồm OT, làm thêm hoàn toàn tự nguyện)"]
source_urls: []
unresolved_fields: []
allowed_role_ids: []
allowed_department_ids: []
---

# QUY TRÌNH THAO TÁC CHUẨN: PHÊ DUYỆT VÀ GIÁM SÁT LÀM THÊM GIỜ (SOP-02) — MÔI TRƯỜNG DEMO

> **Phê duyệt trong hệ thống demo:** Công ty mã `1`, hiệu lực `12/10/2026`, theo yêu cầu của chủ sở hữu hệ thống. Không xác nhận đã đăng ký nội quy với cơ quan nhà nước. Mức tiền cá nhân dùng hợp đồng/chính sách còn hiệu lực đã duyệt; thông tin chưa được cấu hình phải liên hệ bộ phận phụ trách.

**Hướng dẫn áp dụng chung:** Hành chính chuẩn bị suất ăn phụ khi OT từ 02 giờ trở lên và xác nhận dịch vụ thực có. HR đối soát giờ thực tế với đăng ký; phần chênh lệch phải xác minh, không tự động cắt lương của thời gian đã làm.

### [SOP-02/MUC-01] Mục đích và nguyên tắc cốt lõi
1. Đảm bảo 100% các ca làm thêm giờ đều xuất phát từ nhu cầu sản xuất thực tế, được lập kế hoạch trước, có sự phê duyệt hợp lệ của quản lý và sự đồng thuận tự nguyện của người lao động.
2. Tuyệt đối không để xảy ra trường hợp làm thêm giờ vượt trần pháp luật (quá 40 giờ/tháng, quá 200 giờ/năm hoặc 300 giờ/năm trong ngành nghề đặc thù đã thông báo cơ quan nhà nước).

---

### [SOP-02/MUC-02] Lưu đồ thực hiện

```
[Tổ trưởng / Giám sát] --> Lập Kế hoạch OT & Lấy ý kiến tự nguyện (BM-03)
         │
         ▼
[Hệ thống HRMS / HR]  --> Kiểm tra tự động giới hạn trần giờ & đối tượng bảo vệ
         │
         ▼
[Quản đốc & HR duyệt] --> Phê duyệt phiếu đăng ký OT trước giờ ca bắt đầu
         │
         ▼
[Thực hiện & Hậu cần] --> Bố trí suất ăn phụ, trang bị bảo hộ, vận hành ca OT
         │
         ▼
[Hậu kiểm C&B]        --> Đối soát giờ quẹt thẻ thực tế với phiếu đăng ký đã duyệt
```

---

### [SOP-02/MUC-03] Chi tiết từng bước thực hiện

#### Bước 1: Khảo sát nhu cầu và lập kế hoạch OT
- Căn cứ tiến độ đơn hàng, Tổ trưởng/Giám sát xác định số lượng nhân sự, số giờ cần huy động và vị trí công đoạn cần làm thêm.
- Thông báo công khai kế hoạch OT cho công nhân trong tổ.

#### Bước 2: Xác nhận sự tự nguyện của người lao động
- Thu thập chữ ký xác nhận đồng ý tự nguyện của từng cá nhân trên Phiếu đăng ký làm thêm giờ (Mẫu BM-03) hoặc qua tính năng "Xác nhận tham gia OT" trên ứng dụng di động HRMS.
- Tuyệt đối nghiêm cấm việc bắt buộc công nhân ký tên hoặc đe dọa đánh tụt thi đua nếu công nhân từ chối làm thêm.

#### Bước 3: Rà soát điều kiện bảo vệ và trần giờ theo luật định
Trước khi trình duyệt, hệ thống HRMS hoặc chuyên viên HR tiến hành kiểm tra 3 điều kiện tiên quyết:
1. **Đối tượng được bảo vệ theo Điều 137, 160 BLLĐ 2019:**
   - **Nghiêm cấm tuyệt đối làm thêm giờ:** Lao động nữ mang thai từ tháng thứ 07 (hoặc từ tháng thứ 06 nếu làm việc ở vùng cao, vùng sâu, vùng xa, biên giới, hải đảo);
   - **Chỉ được làm thêm giờ nếu người lao động đồng ý:** Lao động nữ đang nuôi con dưới 12 tháng tuổi; người lao động là người khuyết tật (Điều 137 khoản 1 điểm b và Điều 160 BLLĐ).
   - Lao động chưa thành niên (dưới 18 tuổi) chỉ được làm thêm giờ trong một số công việc đặc thù theo danh mục do Bộ LĐ-TB&XH ban hành.
2. **Giới hạn ca trong ngày:** Tổng giờ làm việc bình thường + giờ OT trong ngày không được vượt quá 12 giờ.
3. **Giới hạn lũy kế tháng/năm:** Số giờ làm thêm của cá nhân cộng dồn trong tháng không được vượt quá 40 giờ; tổng số giờ làm việc trong tuần (bình thường + OT) không vượt quá 60 giờ (theo chuẩn trách nhiệm xã hội RBA và Foxconn Code of Conduct FC-02).

#### Bước 4: Phê duyệt kế hoạch OT
- Kế hoạch OT sau khi vượt qua kiểm tra hệ thống sẽ được chuyển đến Quản đốc xưởng và Trưởng phòng HR phê duyệt điện tử trước giờ bắt đầu làm thêm ít nhất 04 giờ đối với ngày thường, 24 giờ đối với ngày nghỉ tuần.

#### Bước 5: Tổ chức hậu cần và giám sát an toàn
- Bộ phận Hành chính căn cứ danh sách phiếu OT đã duyệt để đặt suất ăn phụ giữa ca và điều phối xe buýt tăng cường (nếu ca OT tan muộn).
- Cán bộ An toàn - Sức khỏe (EHS) kiểm tra điều kiện thông gió, ánh sáng, nhiệt độ nhà xưởng trong ca làm thêm.

#### Bước 6: Nghiệm thu và chốt giờ công OT thực tế
- Người lao động thực hiện quẹt thẻ vân tay/khuôn mặt khi bắt đầu và kết thúc giờ làm thêm.
- HR đối soát số giờ theo phiếu đăng ký đã duyệt và số giờ thực tế tại máy. Chênh lệch phải được xác minh và phê duyệt bổ sung trước khi tính lương; không tự động lấy giá trị nhỏ hơn để cắt thời gian thực tế đã làm.
