# Hệ thống phân loại DataDoc HRMS (Taxonomy & Facets)

DataDoc là bộ tài liệu văn bản nghiệp vụ phục vụ hệ thống AI Retrieval-Augmented Generation (RAG).
Hệ thống phân loại này thiết lập ba chiều thuộc tính trực giao, hoàn toàn độc lập với nhau, tránh việc gộp lẫn nguồn gốc pháp lý, hình thức văn bản và chủ đề nghiệp vụ.

---

## 1. Ba trục phân loại độc lập

### Trục 1: Xuất xứ & Nguồn gốc (`source_kind`)
Xác định chủ thể chịu trách nhiệm ban hành và phạm vi ràng buộc pháp lý của tài liệu:
- `LEGAL_REFERENCE`: Văn bản quy phạm pháp luật của Nhà nước Việt Nam (Luật, Nghị định, Thông tư, Văn bản hợp nhất). Mang tính cưỡng chế và là giới hạn pháp lý tối thượng.
- `COMPANY_DOCUMENT`: Văn bản nội bộ do chính doanh nghiệp ban hành hoặc dự thảo (Nội quy lao động, Quy chế, Quyết định, Quy trình, Biểu mẫu).
- `EXTERNAL_COMPANY_REFERENCE`: Tài liệu tham khảo từ doanh nghiệp bên ngoài (ví dụ: Quy tắc ứng xử toàn cầu của Foxconn/Hon Hai). Chỉ có giá trị đối sánh tham khảo, không có hiệu lực áp dụng mặc định tại công ty.
- `SYSTEM_GUIDE`: Hướng dẫn vận hành hệ thống kỹ thuật, phần mềm HRMS, giải thích trường dữ liệu.
- `TEST_FIXTURE`: Dữ liệu kiểm thử tổng hợp (synthetic), ví dụ kịch bản kiểm thử giả định (`is_example: true`, `production_eligible: false`), tuyệt đối không đưa vào corpus sản xuất.

### Trục 2: Hình thức văn bản (`document_type`)
Xác định thể loại và cấu trúc tài liệu theo thể thức quản trị doanh nghiệp:
- `LAW`: Văn bản luật hoặc văn bản hợp nhất luật.
- `INTERNAL_RULE`: Nội quy lao động (theo Điều 118 Bộ luật Lao động).
- `REGULATION`: Quy chế quản lý nội bộ (Quy chế lương thưởng, Quy chế làm việc).
- `POLICY`: Chính sách công ty (Chính sách phúc lợi, Chính sách bảo mật thông tin).
- `PROCEDURE`: Quy trình nghiệp vụ tổng quát.
- `SOP`: Quy trình thao tác chuẩn chi tiết từng bước (Standard Operating Procedure).
- `USER_GUIDE`: Hướng dẫn sử dụng tính năng cho người dùng.
- `JOB_DESCRIPTION`: Bản mô tả công việc và tiêu chuẩn chức danh (JD).
- `FORM`: Biểu mẫu hành chính, phiếu đăng ký, biên bản (Template).
- `NOTICE`: Thông báo nội bộ (Thông báo lịch nghỉ lễ, thông báo kế hoạch).
- `DECISION`: Quyết định hành chính (Quyết định khen thưởng, nâng bậc, kỷ luật).
- `FAQ`: Bộ câu hỏi và trả lời thường gặp đã được phê duyệt.
- `COLLECTIVE_AGREEMENT`: Thỏa ước lao động tập thể (TƯLĐTT).
- `GLOSSARY`: Thuật ngữ và định nghĩa nghiệp vụ chuẩn hóa.

### Trục 3: Chủ đề nghiệp vụ (`topics`)
Danh mục 25 chủ đề nghiệp vụ chuyên môn (cho phép một tài liệu gắn nhiều chủ đề):
1. `SHIFT`: Chế độ ca kíp, phân ca, lịch làm việc, đổi ca, chuyển ca.
2. `ATTENDANCE`: Chấm công, ghi nhận thời gian làm việc, quên quẹt thẻ, sửa công.
3. `OVERTIME`: Làm thêm giờ (OT), đăng ký tăng ca, phê duyệt, giới hạn giờ OT.
4. `PAYROLL`: Tiền lương, tính lương, kỳ hạn trả lương, bảng lương, đối soát lương.
5. `SALARY_REVIEW`: Xét duyệt nâng lương, nâng bậc lương, niên hạn nâng lương.
6. `BONUS`: Tiền thưởng (chuyên cần, năng suất, sáng kiến, thưởng lễ/Tết, tuyển dụng).
7. `DISCIPLINE`: Xử lý kỷ luật lao động, trách nhiệm vật chất, bồi thường thiệt hại.
8. `LEAVE`: Nghỉ phép năm, nghỉ việc riêng, nghỉ không lương, nghỉ ốm đau.
9. `HOLIDAY`: Nghỉ ngày lễ, Tết theo luật định, bố trí lịch nghỉ hoán đổi.
10. `BENEFITS`: Chế độ phúc lợi, ăn ca, xe đưa đón, trợ cấp đời sống, quà tặng.
11. `RECRUITMENT`: Tuyển dụng nhân sự, tiêu chuẩn đầu vào, thi tuyển.
12. `ONBOARDING`: Tiếp nhận nhân viên mới, hội nhập doanh nghiệp.
13. `PROBATION`: Thử việc, hợp đồng thử việc, đánh giá kết thúc thử việc.
14. `TRAINING`: Đào tạo nội bộ, nâng cao tay nghề, cam kết sau đào tạo.
15. `PERFORMANCE`: Đánh giá hiệu quả công việc, KPI, xếp loại nhân viên.
16. `PROMOTION`: Thăng chức, bổ nhiệm, điều động nhân sự.
17. `TERMINATION`: Chấm dứt hợp đồng lao động, thôi việc, sa thải, bồi thường.
18. `HANDOVER`: Bàn giao công việc, tài sản, tài liệu khi thay đổi vị trí/nghỉ việc.
19. `OCCUPATIONAL_SAFETY`: An toàn vệ sinh lao động (ATVSLĐ), bảo hộ, khám sức khỏe.
20. `SECURITY`: An ninh trật tự, bảo vệ nhà máy, kiểm soát ra vào, phòng cháy chữa cháy.
21. `CONDUCT`: Quy tắc ứng xử, đạo đức nghề nghiệp, văn hóa doanh nghiệp.
22. `GRIEVANCE`: Tiếp nhận và giải quyết phản ánh, kiến nghị, khiếu nại, tố cáo.
23. `APPROVAL_AUTHORITY`: Thẩm quyền phê duyệt, phân cấp ký duyệt chứng từ nhân sự.
24. `INSURANCE`: Bảo hiểm xã hội (BHXH), y tế (BHYT), thất nghiệp (BHTN), tai nạn.
25. `TAX`: Thuế thu nhập cá nhân (TNCN), giảm trừ gia cảnh, quyết toán thuế.

---

## 2. Chế độ áp dụng (`applicability_mode`)

Xác định cách thức văn bản áp dụng trong tổ chức:
- `ALL`: Áp dụng cho toàn bộ cán bộ công nhân viên trong công ty mà không phân biệt bộ phận hay vị trí.
- `RESTRICTED`: Áp dụng có giới hạn theo danh sách cụ thể của `site_ids`, `department_ids`, `roles`, hoặc `employment_types`.
- `UNRESOLVED`: Chưa xác định rõ phạm vi áp dụng (yêu cầu công ty làm rõ; **không** được tự động suy diễn thành áp dụng cho tất cả).

---

## 3. Nguyên tắc bất biến khi phân loại

1. **Không trộn lẫn hình thức và chủ đề:** Ví dụ: "Thưởng chuyên cần" không phải là một `document_type`, mà là một điều khoản thuộc `document_type = "REGULATION"`, có `topics = ["BONUS", "ATTENDANCE"]`.
2. **Không trộn lẫn nguồn gốc và hình thức:** Bản tóm tắt Bộ luật Lao động về làm thêm giờ có `source_kind = "LEGAL_REFERENCE"`, `document_type = "LAW"`, `topics = ["OVERTIME"]`; không được phân loại là quy định của công ty.
3. **Phân biệt rõ văn bản quy phạm và dữ liệu giao dịch động:** Văn bản quy định cơ chế tính thưởng thuộc DataDoc RAG. Số tiền thưởng thực tế của nhân viên tháng trước thuộc SQL Database có kiểm soát quyền, không được tìm kiếm qua vector store tài liệu.
