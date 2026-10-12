---
doc_id: "SOP-01"
title: "Quy trình thao tác chuẩn: Chốt bảng công và tính lương hàng tháng (môi trường demo)"
version: "2.0.0-demo"
status: "ACTIVE_OFFICIAL"
effective_from: "2026-10-12"
effective_to: null
approved_by: "Chủ sở hữu hệ thống demo — yêu cầu phê duyệt qua Codex ngày 12/10/2026"
company_id: "1"
source_kind: "INTERNAL_PROCEDURE"
document_type: "PROCEDURE"
topics: ["ATTENDANCE_TIMEKEEPING", "SALARY_POLICY", "PAYROLL_RECONCILIATION"]
jurisdiction: "VN"
applicable_location: "Bắc Ninh"
applicability_mode: "ALL"
access_profile: "PUBLIC_INTERNAL"
applies_to_current_company: true
ingest_enabled: true
requires_hr_approval_for_live: true
legal_basis: ["PL-01: Bộ luật Lao động 2019 (Thời giờ làm việc)", "PL-02: Bộ luật Lao động 2019 (Tiền lương làm thêm giờ)", "CT-02: Quy chế chấm công và sửa công (Dự thảo)", "CT-05: Quy chế trả lương và đối soát bảng lương (Dự thảo)"]
benchmark_basis: []
source_urls: []
unresolved_fields: []
allowed_role_ids: []
allowed_department_ids: []
---

# QUY TRÌNH THAO TÁC CHUẨN: CHỐT BẢNG CÔNG VÀ TÍNH LƯƠNG (SOP-01) — MÔI TRƯỜNG DEMO

> **Phê duyệt trong hệ thống demo:** Công ty mã `1`, hiệu lực `12/10/2026`, theo yêu cầu của chủ sở hữu hệ thống. Không xác nhận đã đăng ký nội quy với cơ quan nhà nước. Mức tiền cá nhân dùng hợp đồng/chính sách còn hiệu lực đã duyệt; thông tin chưa được cấu hình phải liên hệ bộ phận phụ trách.

**Hướng dẫn áp dụng chung:** Quy trình hiện hành: hoàn thiện hợp đồng, chính sách lương/BHXH/công đoàn/thuế/người phụ thuộc; duyệt các đơn và khoản phát sinh; xử lý bất thường; Tính toán & Công bố công; Tính lương để kiểm tra; Khóa kỳ công khi đủ điều kiện; tính lại nếu hệ thống yêu cầu cập nhật phiên bản; đối soát rồi Công bố lương. PC: Chấm Công → Bảng Lương → Công bố lương. Web: Bảng lương → Công bố lương. Sau công bố, lương và công của kỳ là bất biến, không sửa, thu hồi hay đổi trạng thái; điều chỉnh có căn cứ ở kỳ còn mở. Công bố phiếu lương không có nghĩa đã chuyển tiền ngân hàng.

### [SOP-01/MUC-01] Mục đích và phạm vi
1. Chuẩn hóa quy trình thu thập, đối soát dữ liệu chấm công từ hệ thống máy quẹt thẻ vân tay/nhận diện khuôn mặt, kết nối với dữ liệu nghỉ phép, làm thêm giờ và tính toán bảng lương chính xác, minh bạch.
2. Áp dụng cho toàn bộ các khối phòng ban, nhà xưởng sản xuất trực thuộc nhà máy Bắc Ninh.

---

### [SOP-01/MUC-02] Lưu đồ và các bước thực hiện

```
[Người lao động]     --> Kiểm tra công cá nhân & gửi đơn sửa công (BM-01)
         │
         ▼
[Quản lý bộ phận]   --> Phê duyệt đơn công & Khóa dữ liệu ca/chuyền
         │
         ▼
[Phòng C&B]         --> Đồng bộ dữ liệu chấm công + Xử lý bất thường
         │
         ▼
[Phòng C&B]         --> Tính toán lương, OT, phụ cấp & các khoản trích nộp
         │
         ▼
[Cấp phê duyệt]     --> Trưởng phòng HR & Giám đốc Tài chính phê duyệt
         │
         ▼
[Phát hành & Chi trả]--> Gửi Phiếu lương điện tử & Chuyển khoản ngân hàng
```

---

### [SOP-01/MUC-03] Chi tiết từng bước thực hiện

#### Bước 1: Nhân viên tự kiểm tra và hoàn thiện đơn từ (Từ ngày 01 đến ngày 02 hàng tháng)
- Người lao động đăng nhập ứng dụng HRMS, kiểm tra bảng công cá nhân của tháng trước liền kề.
- Nếu có lỗi quên quẹt thẻ hoặc sai lệch giờ công, người lao động tạo yêu cầu sửa công (BM-01) trực tuyến kèm theo lý do giải trình.
- Hạn chót tiếp nhận đơn giải trình bổ sung công tháng: Trước 17h00 ngày 02.

#### Bước 2: Quản lý bộ phận xét duyệt công (Trước 12h00 ngày 03 hàng tháng)
- Tổ trưởng/Quản lý trực tiếp kiểm tra các đơn xin sửa công, đơn nghỉ phép, phiếu OT của nhân viên dưới quyền.
- Thực hiện phê duyệt hoặc từ chối có nêu rõ lý do trên hệ thống HRMS.
- Hoàn tất phê duyệt các đơn theo quyền và chuyển kết quả đối soát cho HR/C&B; chỉ khóa kỳ công ở chức năng Khóa kỳ công khi hệ thống xác nhận đủ điều kiện.

#### Bước 3: Phòng C&B tổng hợp và rà soát bất thường (Ngày 03 - 05)
- Chuyên viên C&B chạy tiến trình đồng bộ dữ liệu chấm công tổng thể từ tất cả các cổng quẹt thẻ.
- Lọc danh sách bất thường: Quẹt 1 lần, vắng mặt không lý do, làm việc liên tục vượt giới hạn an toàn.
- Phối hợp với nhân sự phụ trách xưởng để xác minh dứt điểm các ca bất thường.

#### Bước 4: Chạy tiến trình tính toán lương và khấu trừ (Ngày 06 - 07)
- Hệ thống tự động tính:
  1. Lương thời gian theo ngày công thực tế;
  2. Lương làm thêm giờ (tách riêng OT ngày thường $\times 1.5$, ngày nghỉ tuần $\times 2.0$, lễ tết $\times 3.0$);
  3. Phụ cấp ca đêm và OT đêm (thêm $30\%$ và $20\%$ theo Điều 98 BLLĐ);
  4. Phụ cấp chức danh, phụ cấp trách nhiệm, chuyên cần (nếu đủ điều kiện);
  5. Trích nộp BHXH ($8\%$), BHYT ($1.5\%$), BHTN ($1\%$) trên mức đóng bảo hiểm;
  6. Khấu trừ thuế TNCN theo biểu thuế lũy tiến từng phần;
  7. Khấu trừ bồi thường thiệt hại (nếu có quyết định bồi thường, bảo đảm không quá $30\%$ lương thực lĩnh).

#### Bước 5: Trình ký và phê duyệt bảng lương (Ngày 08)
- Bảng tổng hợp chi lương được gửi cho:
  - Trưởng phòng Nhân sự thẩm tra nghiệp vụ;
  - Giám đốc Tài chính / Kế toán trưởng thẩm tra ngân sách quỹ lương;
  - Tổng Giám đốc (hoặc Người được ủy quyền) ký duyệt chi.

#### Bước 6: Phát hành phiếu lương và giải ngân (Ngày 09 - 10)
- Người có quyền thực hiện Công bố lương trên PC hoặc Web; nhân viên xem phiếu lương đã công bố của mình trong HRMS.
- Phòng Kế toán gửi lệnh chuyển tiền đến ngân hàng để giải ngân vào tài khoản người lao động đúng ngày thanh toán lương quy định tại [CT-05/DIEU-02].

#### Bước 7: Tiếp nhận tra soát và giải quyết khiếu nại (Trong vòng 03 ngày làm việc sau phát hành)
- Bộ phận C&B trực tiếp tiếp nhận các yêu cầu tra soát qua HRMS hoặc Phiếu đối soát.
- Mọi khiếu nại có căn cứ sẽ được cập nhật bổ sung truy lĩnh vào kỳ lương tháng tiếp theo.
