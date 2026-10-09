# Checklist nghiệm thu AI/RAG V2

Cập nhật 03/10/2026. Đây là mục tiêu nghiệm thu, không phải báo cáo tất cả đã hoàn tất. Đã bỏ các đánh dấu hoàn thành không có bằng chứng end-to-end. Kiểm thử mã được ghi riêng trong ai-rag-v2-test-report.md.

Các tiêu chí database/connection pool/phân quyền Oracle phải thử staging; UI phải thử Web/Desktop thực. U20 (LLM hiểu JSON), A13/A14/A19 và C14 (nhánh corpus/vector) chưa nghiệm thu vì tính năng đang tắt. C18 chỉ đạt dùng chung Business execution service và DTO; Desktop chưa dùng HTTP/JWT. A15 yêu cầu đối chiếu payroll source/run/uniqueness thực tế, không được suy từ dữ liệu giả.

Giữ các ô dưới đây chưa đánh dấu cho tới khi có bằng chứng nghiệm thu và tên người xác nhận. Những test unit pass không tự đánh dấu cả tiêu chí sản phẩm là hoàn tất.
## 20 Tiêu chí Hiểu câu hỏi & Hội thoại (U01 — U20)
- [ ] U01: Tăng ca nhân viên mã 10 tháng 9/2026 $\rightarrow$ OVERTIME, MANV=10, 09/2026; không query profile.
- [ ] U02: Phụ cấp dưới 1 triệu $\rightarrow$ ALLOWANCE, SOTIEN < 1000000; không nhầm OVERTIME.
- [ ] U03: Phụ cấp trên 1,5 triệu $\rightarrow$ SOTIEN > 1500000; không parse thành 5 triệu hoặc >=.
- [ ] U04: Nhân viên sinh nhật tháng 12 $\rightarrow$ BirthdayMonth=12; không gộp tháng/năm hiện tại.
- [ ] U05: Tổng quỹ lương tháng này $\rightarrow$ Metric quỹ lương rõ ràng; không trả lịch trả lương.
- [ ] U06: Nghỉ phép năm $\rightarrow$ Không tìm HOTEN chứa "nghỉ phép năm"; policy lookup rõ ràng.
- [ ] U07: Xin chào + hỏi việc $\rightarrow$ Giữ nội dung hỏi, không chỉ trả lời câu chào.
- [ ] U08: Tên người khớp duy nhất $\rightarrow$ Resolve MANV, không hỏi lại dư thừa.
- [ ] U09: Tên người mơ hồ (trùng tên) $\rightarrow$ Trả `needs_clarification`, hỏi người dùng chọn (Runtime Gate hoạt động).
- [ ] U10: Đang pending chọn người + bổ sung filter $\rightarrow$ Bổ sung đồng thời entity và operation.
- [ ] U11: Câu hỏi tiếp nối thời gian "tháng trước đó thì sao?" $\rightarrow$ Giữ domain, lùi về tháng liền trước kỳ vừa hỏi.
- [ ] U12: "Tổng tăng ca tháng trước" với inject clock $\rightarrow$ Tính tháng trước theo injected clock/timezone.
- [ ] U13: Sau khi liệt kê nhiều người, user nói "lương anh ấy" $\rightarrow$ Hỏi rõ người nào, không lấy top 1.
- [ ] U14: User đổi chủ đề giữa chừng $\rightarrow$ Hủy pending cũ, chuyển sang yêu cầu mới.
- [ ] U15: Phòng ban/tổ chức alias mơ hồ $\rightarrow$ Phân giải/hỏi trong phạm vi, không đoán bừa.
- [ ] U16: Tháng 13 hoặc năm thiếu $\rightarrow$ Báo lỗi hoặc hỏi năm, không sinh SQL sai.
- [ ] U17: Lương cơ bản vs Thực lĩnh $\rightarrow$ Phân biệt 2 metric khác nhau, không dùng chung cache.
- [ ] U18: "Ai tăng ca nhiều nhất" $\rightarrow$ Phép tính `SUM(SOGIO)` theo nhân viên rồi `ORDER BY DESC`.
- [ ] U19: Câu hỏi kỹ năng/skills khi CSDL không có $\rightarrow$ Trả `unsupported`, không bịa kinh nghiệm.
- [ ] U20: LLM sinh JSON lỗi/enum lạ $\rightarrow$ Bounded retry hoặc trả lỗi kiểm soát; không đoán fallback.

---

## 20 Tiêu chí Quyền, SQL & Dữ liệu (A01 — A20)
- [ ] A01: Không token hoặc token hết hạn $\rightarrow$ Trả 401 Unauthorized trước mọi thao tác.
- [ ] A02: Có quyền chat nhưng thiếu capability payroll (`F_CC_BANGLUONG`) $\rightarrow$ Trả 403 Forbidden.
- [ ] A03: Self-scope hỏi lương người khác $\rightarrow$ Bị chặn, không nâng scope.
- [ ] A04: Manager phòng ban hỏi công phòng khác $\rightarrow$ Chỉ lọc trong phạm vi phòng mình.
- [ ] A05: Lookup danh sách ứng viên có người ngoài quyền $\rightarrow$ Loại bỏ người ngoài quyền.
- [ ] A06: Field bị cấm (ví dụ điện thoại, lương) $\rightarrow$ Không xuất hiện trong SQL, Context, LLM, Log.
- [ ] A07: Quyền aggregate-only $\rightarrow$ Không cấp row detail cho caller.
- [ ] A08: Yêu cầu nhiều phần, có phần bị cấm $\rightarrow$ Từ chối hoặc tách bạch, không lộ dữ liệu cấm.
- [ ] A09: SQL Injection / DDL / DML / System package $\rightarrow$ Bị chặn 100% bởi Validator.
- [ ] A10: SQL subquery/CTE ngoài whitelist $\rightarrow$ Từ chối an toàn.
- [ ] A11: Lỗi context connection $\rightarrow$ Không trả dữ liệu; reset connection pool sạch sẽ.
- [ ] A12: Timeout / Cancel giữa chừng $\rightarrow$ Hủy tác vụ, không ghi kết quả trễ vào state.
- [ ] A13: Vector search fallback $\rightarrow$ Luôn giữ security filter; không bao giờ search toàn bộ.
- [ ] A14: Metadata cũ sau khi nhân viên chuyển phòng $\rightarrow$ Kiểm tra quyền thời gian thực trước khi hydrate.
- [ ] A15: Nhiều lần tính lương trong kỳ $\rightarrow$ Chỉ lấy bản ghi lương có hiệu lực (active run).
- [ ] A16: Hợp đồng hết hạn trong 30 ngày $\rightarrow$ Tính toán đúng biên ngày tháng.
- [ ] A17: Phân biệt `no_data` (không có bản ghi) với `error` (lỗi kết nối CSDL).
- [ ] A18: View chưa migrate trên DB $\rightarrow$ Báo capability not-ready rõ ràng.
- [ ] A19: Văn bản nguồn chứa Prompt Injection $\rightarrow$ Không thay đổi được chỉ thị hệ thống.
- [ ] A20: Quyền user `AI_READONLY` trên DB $\rightarrow$ Chỉ SELECT các View an toàn, không có quyền bảng gốc.

---

## 20 Tiêu chí Cache, Đồng thời & Client (C01 — C20)
- [ ] C01: Hai câu hỏi ngữ nghĩa tương đương $\rightarrow$ Cùng canonical key, chỉ chạy 1 load.
- [ ] C02: Khác tham số (mã, tháng, metric) $\rightarrow$ Khác cache key.
- [ ] C03: Cùng từ "anh ấy" nhưng khác phiên trò chuyện $\rightarrow$ Khác key, không dùng chung kết quả.
- [ ] C04: Hai user khác quyền/scope $\rightarrow$ Khác security fingerprint, không chia sẻ kết quả.
- [ ] C05: Hết TTL tuyệt đối $\rightarrow$ Tự động hủy và tải lại từ nguồn.
- [ ] C06: Thay đổi quyền của user $\rightarrow$ Vô hiệu hóa cache cũ ngay lập tức.
- [ ] C07: CSDL thay đổi dữ liệu (tăng revision) $\rightarrow$ Invalidate cache tương ứng.
- [ ] C08: Dữ liệu thay đổi trong lúc đang query $\rightarrow$ Không ghi đè kết quả cũ lên revision mới.
- [ ] C09: Đổi schema/prompt/model version $\rightarrow$ Tự động chuyển sang namespace cache mới.
- [ ] C10: 10 requests đồng thời cùng key (Single-flight) $\rightarrow$ Chỉ 1 DB load, không block các key khác.
- [ ] C11: 1 client cancel trong nhóm single-flight $\rightarrow$ Không làm ảnh hưởng các clients còn lại.
- [ ] C12: Lỗi DB/Timeout/Forbidden $\rightarrow$ Không cache như thành công.
- [ ] C13: Dữ liệu nhạy cảm (Lương, Bảo hiểm) $\rightarrow$ Không cache kết quả chi tiết.
- [ ] C14: Vector fallback đổi tag $\rightarrow$ Tái sử dụng embedding vector, giữ nguyên filter.
- [ ] C15: Reset cuộc trò chuyện $\rightarrow$ Tác vụ cũ hoàn thành trễ không được ghi vào phiên mới (Generation token).
- [ ] C16: GET chat / Reset ẩn danh $\rightarrow$ Bị chặn hoàn toàn (yêu cầu xác thực, POST duy nhất).
- [ ] C17: Option token làm rõ quá hạn hoặc của user khác $\rightarrow$ Bị từ chối.
- [ ] C18: Web và Desktop đồng nhất hành vi, hợp đồng dữ liệu và cơ chế xác thực.
- [ ] C19: Bộ nhớ cache vượt ngưỡng capacity $\rightarrow$ Cơ chế Eviction (LRU/FIFO) hoạt động chuẩn.
- [ ] C20: Phép tính đếm/tổng scalar $\rightarrow$ Render trực tiếp, không gọi LLM suy luận.
