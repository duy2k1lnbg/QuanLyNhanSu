---
doc_id: "CT-05"
title: "Quy chế trả lương và đối soát bảng lương (môi trường demo)"
version: "2.0.0-demo"
status: "ACTIVE_OFFICIAL"
effective_from: "2026-10-12"
effective_to: null
approved_by: "Chủ sở hữu hệ thống demo — yêu cầu phê duyệt qua Codex ngày 12/10/2026"
company_id: "1"
source_kind: "COMPANY_POLICY"
document_type: "REGULATION"
topics: ["SALARY_POLICY", "PAYROLL_RECONCILIATION", "MINIMUM_WAGE"]
jurisdiction: "VN"
applicable_location: "Bắc Ninh"
applicability_mode: "ALL"
access_profile: "PUBLIC_INTERNAL"
applies_to_current_company: true
ingest_enabled: true
requires_hr_approval_for_live: true
legal_basis: ["PL-02: Bộ luật Lao động 2019 (Điều 98 - Tiền lương làm thêm giờ, làm việc ban đêm)", "PL-04: Nghị định 293/2025/NĐ-CP (Mức lương tối thiểu vùng từ ngày 01/01/2026)", "Bộ luật Lao động 2019 (Điều 94, 95, 96, 102)"]
benchmark_basis: ["FC-01: Foxconn Global Code of Conduct 2021"]
source_urls: []
unresolved_fields: []
allowed_role_ids: []
allowed_department_ids: []
---

# QUY CHẾ TRẢ LƯƠNG VÀ ĐỐI SOÁT BẢNG LƯƠNG — MÔI TRƯỜNG DEMO

> **Phê duyệt trong hệ thống demo:** Công ty mã `1`, hiệu lực `12/10/2026`, theo yêu cầu của chủ sở hữu hệ thống. Không xác nhận đã đăng ký nội quy với cơ quan nhà nước. Mức tiền cá nhân dùng hợp đồng/chính sách còn hiệu lực đã duyệt; thông tin chưa được cấu hình phải liên hệ bộ phận phụ trách.

**Hướng dẫn áp dụng chung:** Địa bàn lương tối thiểu được xác định từ địa chỉ nơi làm việc và chính sách vùng còn hiệu lực trong Cấu hình lương, không suy ra chỉ từ tên tỉnh Bắc Ninh. Hồ sơ demo chưa xác nhận xã/phường thật nên không chốt một vùng cho mọi nhân viên. Phiếu lương được xem trong HRMS sau khi Công bố lương; gửi email và giải ngân ngân hàng là bước riêng, không được coi là đã thực hiện tự động.

### [CT-05/DIEU-01] Nguyên tắc trả lương và hình thức thanh toán
1. **Nguyên tắc trả lương (Điều 94 BLLĐ 2019):**
   - Người sử dụng lao động trả lương trực tiếp, đầy đủ, đúng hạn cho người lao động;
   - Không được hạn chế hoặc can thiệp vào quyền tự do chi tiêu lương của người lao động; không được ép buộc người lao động mua hàng hóa, sử dụng dịch vụ của người sử dụng lao động.
2. **Hình thức trả lương (Điều 96 BLLĐ 2019):**
   - Lương được chi trả thông qua tài khoản cá nhân của người lao động mở tại ngân hàng thương mại do người lao động lựa chọn và đăng ký; không bắt buộc một ngân hàng duy nhất;
   - Các loại phí liên quan đến việc mở tài khoản và chuyển tiền lương do người sử dụng lao động chi trả.

---

### [CT-05/DIEU-02] Kỳ hạn trả lương và mức sàn lương tối thiểu
1. **Kỳ hạn trả lương:**
   - Kỳ tính lương tính từ ngày 01 đến ngày cuối cùng của tháng dương lịch;
   - Ngày thanh toán lương định kỳ hàng tháng: ngày 10 của tháng kế tiếp. Nếu ngày trả lương trùng vào ngày nghỉ cuối tuần hoặc ngày lễ tết thì được thanh toán vào ngày làm việc liền kề trước đó.
2. **Tuân thủ mức lương tối thiểu vùng theo địa bàn hoạt động:**
   - Mức lương ghi trong hợp đồng lao động không được thấp hơn mức lương tối thiểu vùng theo Nghị định 293/2025/NĐ-CP tương ứng với địa bàn xã/phường nơi người lao động làm việc (Nếu thuộc địa bàn Vùng II: 4.730.000 đồng/tháng hoặc 22.700 đồng/giờ; nếu thuộc địa bàn Vùng III: 4.140.000 đồng/tháng hoặc 20.000 đồng/giờ);
   - Do công ty chưa xác nhận địa chỉ nhà máy cụ thể nên sàn lương áp dụng là **điều kiện tra cứu theo địa chỉ thực tế nơi người lao động làm việc**.

---

### [CT-05/DIEU-03] Cơ cấu thu nhập và Bảng kê trả lương (Phiếu lương)
1. **Cơ cấu thu nhập trong tháng:**
   - Lương cơ bản theo hợp đồng lao động;
   - Tiền lương làm việc thực tế theo ngày công;
   - Tiền lương làm thêm giờ (OT ngày thường $\ge 150\%$, nghỉ tuần $\ge 200\%$, lễ tết $\ge 300\%$);
   - Tiền lương làm việc vào ban đêm và làm thêm giờ ban đêm theo đúng 2 nhánh Điều 57 Nghị định 145/2020/NĐ-CP (nhánh không làm thêm ban ngày $\ge 200\%$, nhánh đã làm thêm ban ngày $\ge 210\%$);
   - Các khoản phụ cấp lương và hỗ trợ phúc lợi (nếu có);
   - Các khoản trích trừ bắt buộc theo pháp luật: Bảo hiểm xã hội (8%), Bảo hiểm y tế (1,5%), Bảo hiểm thất nghiệp (1%), Thuế thu nhập cá nhân (nếu có).
2. **Phiếu lương minh bạch (Điều 95 BLLĐ 2019):**
   - Mỗi lần trả lương, công ty có trách nhiệm thông báo bảng kê trả lương (phiếu lương điện tử trên HRMS hoặc gửi qua email nội bộ) cho người lao động.
   - Bảng kê ghi rõ: Tiền lương cơ bản, tiền làm thêm giờ, tiền lương ban đêm, các khoản trợ cấp, số tiền bị khấu trừ (BHXH, BHYT, BHTN, thuế TNCN) và số tiền thực lĩnh.

---

### [CT-05/DIEU-04] Giới hạn khấu trừ tiền lương
1. **Cấm tuyệt đối phạt trừ lương:** Nghiêm cấm dùng hình thức khấu trừ tiền lương để xử phạt vi phạm kỷ luật lao động (Điều 127 BLLĐ 2019).
2. **Khấu trừ hợp pháp theo luật (Điều 102 BLLĐ 2019):**
   - Việc khấu trừ tiền lương chỉ được thực hiện để bồi thường thiệt hại do làm hư hỏng dụng cụ, thiết bị, tài sản của công ty theo quy định tại Điều 129 của BLLĐ 2019;
   - Mức khấu trừ tiền lương hàng tháng không được vượt quá **30%** tiền lương thực lĩnh tháng đó của người lao động (sau khi đã trích nộp các khoản BHXH, BHYT, BHTN và thuế TNCN).

---

### [CT-05/DIEU-05] Quy trình đối soát và giải quyết khiếu nại bảng lương
1. **Thời hạn kiểm tra và khiếu nại:** Sau khi nhận phiếu lương qua hệ thống HRMS, người lao động có thời hạn 03 ngày làm việc; việc nộp sau mốc đối soát không làm mất quyền yêu cầu thanh toán tiền lương còn thiếu để đối soát số giờ công, số giờ làm thêm, các khoản phụ cấp và khấu trừ.
2. **Quy trình giải quyết:** Bộ phận C&B có trách nhiệm kiểm tra đối chiếu dữ liệu chấm công và phản hồi trong vòng 02 ngày làm việc để phản hồi ban đầu. Các khoản sai sót có căn cứ sẽ được chi trả truy lĩnh bổ sung vào kỳ lương kế tiếp.
