# AI/RAG V2 — trạng thái sau sửa ngày 03/10/2026

Tài liệu này mô tả mã hiện tại. Các kết quả kiểm thử nằm trong `ai-rag-v2-test-report.md`; không coi thiết kế hay build thành công là bằng chứng database đã triển khai.

## Luồng đang chạy

Web gửi POST có JWT tới `AiChatController`. Backend lấy ID đã xác thực rồi nạp lại người dùng còn hoạt động, quyền chức năng, capability, phạm vi, chính sách trường và revision bằng `OracleAiPolicyProvider`. Không lấy phạm vi từ câu hỏi, claim công ty, tên nhóm, `IsAdmin` hoặc ký tự `*`.

`AiExecutionService` xử lý: snapshot hội thoại → hiểu câu hỏi → cổng làm rõ → lập SQL từ template → kiểm tra quyền → đọc view qua `AI_READONLY` → kết xuất trực tiếp → kiểm tra lại quyền/revision → commit hội thoại. Số đếm, tổng tiền, tổng giờ không cần LLM.

Desktop dùng cùng Business execution service với ID từ `UserSession`, nạp lại cùng chính sách. Desktop hiện chưa gọi HTTP/JWT của API; chưa thể đánh dấu hoàn thành yêu cầu hai client dùng cùng cơ chế xác thực. Không phát sinh token JWT giả để che lấp khác biệt này.

## Làm rõ đầu vào

Câu hỏi được phân tích theo domain, operation, metric, đối tượng, bộ lọc và thời gian. Hiện dùng bộ quy tắc xác định trong mã, chưa có bộ hiểu ngôn ngữ bằng LLM tổng quát. Câu ngoài các mẫu hỗ trợ được trả `unsupported`.

- Trùng tên nhân viên/phòng ban: hỏi chọn trong các ứng viên có quyền; không tự lấy ứng viên đầu tiên.
- Thiếu năm hoặc kỳ: hỏi bổ sung, không tự gán năm hiện tại cho một tháng được nhập rõ.
- “Quỹ lương/lương” chưa rõ chỉ tiêu: hỏi thực lĩnh hay tiền lương theo công thực tế. Chưa có ánh xạ lương cơ bản tháng được xác nhận.
- Lựa chọn dùng token ngẫu nhiên của đúng prompt; token không được giải mã thành MANV. Phải gửi clarification ID và version hiện tại.
- Trả lời bổ sung giữ các slot trước đó. Token khác user, khác conversation, sai version hoặc quá 10 phút bị từ chối.
- Tháng/năm/biên ngày không hợp lệ bị chặn; nhân viên hay phòng không tìm thấy không được biến thành truy vấn rộng hơn.
- Không dùng hồ sơ nhân sự hiện tại để trả số liệu tháng/năm khi chưa có snapshot lịch sử.
- Bộ lọc một ngưỡng tiền theo đơn vị triệu/tr áp dụng cho phụ cấp và tạm ứng; khoảng tiền hoặc điều kiện tiền chưa được ánh xạ trả unsupported, không âm thầm bỏ bộ lọc.

## Quyền và nguồn dữ liệu

Quyền chức năng dùng các mã thực có trong dự án: `F_SYSTEM_AI`, `F_DM_NHANVIEN`, `F_CC_BANGCONG`, `F_CC_TANGCA`, `F_CC_PHUCAP`, `F_CC_UNGLUONG`, `F_CC_BANGLUONG`, `F_NV_HOPDONG`, `F_NV_NANGLUONG`.

Production bắt buộc có capability đang bật, quyền chức năng cần thiết và grant phạm vi theo từng capability. ALLOW hợp nhất; DENY được trừ sau đó. Trusted reader context giữ thêm SELF_ONLY cho yêu cầu SELF, gồm COUNT được tổng hợp trong database; cache key bao gồm RequestedScope để không trộn SELF với ALL. Nhóm bị disabled hoặc không phải principal ISGROUP=1 không cấp quyền/grant. SELF luôn bị giao với MANV của người gọi, kể cả khi có grant ALL. FIELD_POLICY thiếu thì DENY; MASK được kết xuất thành `***`. ALLOWED_OPERATIONS khác rỗng được kiểm tra theo operation và FILTER; null/rỗng nghĩa là không thêm hạn chế operation ngoài template/capability, không có nghĩa cấp quyền cho trường chưa khai báo.

Quyền COUNT dùng V_AI_EMPLOYEE_COUNT chỉ có tổng theo tổ chức, không có danh tính nhân viên. SUM/TOP tăng ca dùng V_AI_OVERTIME_SUMMARY đã tổng hợp theo người/tháng, không mở lượt tăng ca/ngày. Scope của COUNT được Oracle lọc trước khi tổng hợp. PAYROLL_SUMMARY không được mở employee lookup hay payroll detail. Bảng lương AI chỉ lấy kỳ khóa sổ và run SUCCESS có FINISHED_AT; khi chưa đủ điều kiện, trả no_data với giải thích điều kiện nguồn.

SQL nghiệp vụ chỉ đọc một view `HR.V_AI_*` theo template đã kiểm tra. Executor từ chối lời gọi SQL tự do thiếu kế hoạch/ngữ cảnh. OracleCommand dùng BindByName và tham số đúng kiểu. Cổng legacy `HybridRagService.Ask` thiếu danh tính trả unsupported trước retrieval/LLM.

Ứng dụng dùng kết nối HR tin cậy để nạp chính sách và cấp vé một lần. Vé chỉ mang actor/capability/revision, không mang nội dung câu hỏi. Kết nối AI_READONLY tiêu thụ vé bằng trusted package, có context theo Oracle session; cuối yêu cầu luôn dọn context. Nếu dọn thất bại, connection pool tương ứng bị clear. Các package/view/trigger mới phải được compile và kiểm thử trên Oracle staging.

## Kết quả và đồng thời

Các trạng thái giữ riêng: answered, needs_clarification, forbidden, unsupported, no_data, error. HTTP 401/403/409/503/504 phản ánh lỗi xác thực, quyền, phiên, nguồn và timeout. Không công bố SQL, tên view nội bộ, lỗi Oracle hoặc credential trong câu trả lời.

Danh sách có thứ tự ổn định, giới hạn 20 dòng; xếp hạng giới hạn 5. Đọc thêm một dòng để biết còn kết quả. Total records từ COUNT OVER có thể là null khi không có phép đếm, còn không dữ liệu là 0. `asOf` là thời điểm đọc của ứng dụng, không phải thời điểm cập nhật từng bản ghi.

Mỗi request dùng lease chứa generation/sequence/version và bản sao state. Reset, hết TTL, eviction hoặc request mới làm request cũ không được commit. Hội thoại giới hạn 5.000 phiên, TTL 60 phút, lịch sử 20 message. Bộ nhớ nằm trong từng process, không đồng bộ nhiều API instance.

Web tạo conversation mới khi mount/reload vì không lưu lịch sử nhạy cảm ở browser; conversation ID được ghi theo user, hủy/loại response trễ khi reset/unmount/đổi tài khoản. Desktop có nút chọn làm rõ, reset/cancel; dashboard đọc danh sách tối thiểu và số người từ view có quyền. KPI chưa có ánh xạ để trống.

## Chưa bật

Nguồn quy chế chưa được ánh xạ và có ACL tin cậy: trả unsupported; SearchScopedAsync không truy cập Qdrant. Hybrid BM25/vector, RRF, reranking, multi-query, chunking và embedding cache chưa phải tính năng chạy của luồng này. Chi tiết trong `ai-rag-v2-knowledge-corpus.md`.

ATTENDANCE_DETAIL tắt vì chưa có ánh xạ sự kiện vào/ra ngày. INSURANCE_VIEW quản lý tắt vì chưa xác định mã quyền chức năng thực; INSURANCE_SELF vẫn cần grant SELF rõ ràng. Mọi thay đổi này không thay thế việc kiểm thử quyền/database khi triển khai.