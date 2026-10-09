# Cache AI V2 — cơ chế đang dùng

Cập nhật 03/10/2026. Cache ở RAM từng process, không lưu vào schema AI_READONLY và không tồn tại qua restart.

## Cache nào thực sự có tác dụng

Luồng SQL hiện chỉ cache kết quả EMPLOYEE + COUNT khi có source revision đáng tin cậy lớn hơn 0. Đây gồm tổng số người và thống kê theo phòng ban trong phạm vi đã cấp. TTL tuyệt đối 60 giây, tối đa 1.000 entry. Các phép tra cứu cá nhân, lương, bảo hiểm, phụ cấp, tạm ứng, hợp đồng, nâng lương và tăng ca đều bypass result cache.

PlanCache, EntityCache và EmbeddingCache đã có utility, nhưng chưa được nối vào luồng SQL production hiện tại. Không dùng sự tồn tại của các class đó để khẳng định toàn bộ cache nhiều tầng hoạt động. Entity lookup hiện đọc lại view có quyền; nhánh vector đang tắt.

## Khóa và kiểm tra freshness

Khóa res-v3 bao gồm user ID, fingerprint quyền/phạm vi/chính sách trường/operation policy, RequestedScope (SELF/ALL), domain, operation, metric, source view, SQL template, tham số đã chuẩn hóa cùng kiểu dữ liệu, row limit, capability và source revision. Không dùng câu hỏi thô hoặc token làm rõ làm khóa kết quả.

Hai cách hỏi cùng số đếm có thể dùng chung khóa sau khi hiểu thành cùng kế hoạch. Bộ lọc, kỳ, đối tượng, kiểu tham số, quyền hoặc revision khác thì khóa khác. Khóa được SHA256; không log dữ liệu câu hỏi hay giá trị nhạy cảm.

Production nạp quyền trước request và kiểm tra lại trước khi publish/commit/trả cache hit. Nếu policy hoặc data revision liên quan đổi trong lúc đang đọc, trả conflict 409 và không publish kết quả cũ. Trigger revision tham gia transaction nguồn; thay đổi từ đường cập nhật nào không qua trigger phải được phát hiện trong nghiệm thu DBA. Thay đổi quyền MANV/phòng/công ty được đưa vào fingerprint và POLICY_GLOBAL.

Entry cũ vẫn có thể nằm trong RAM tới TTL/eviction nhưng không được truy cập bằng khóa mới. Không có bộ xóa cache xuyên nhiều process.

## Đồng thời và lỗi

Các request cùng key dùng single-flight trong cùng process. Cleanup chỉ xóa chính flight của nó; waiter cũ không thể xóa flight mới. Một waiter hủy không hủy việc đọc chia sẻ với waiter khác. Truy vấn chia sẻ vẫn bị giới hạn timeout ở executor.

Chỉ answered/no_data được cache. Lỗi, timeout, forbidden, unsupported và prompt làm rõ không được cache. Nếu request khởi tạo flight bị reset/hủy, kết quả không publish dù waiter khác có thể nhận câu trả lời hợp lệ; có thể phát sinh lần load nữa sau đó. Đây là đánh đổi có chủ ý để ngăn công bố từ phiên đã vô hiệu.

Eviction và insert được khóa để giữ giới hạn capacity. Prune hết hạn xóa theo chính entry đã đọc, tránh xóa bản mới cùng key. TTL không gia hạn bởi hit.

## Đo hiệu quả

`AiCacheCoordinator.GetResultMetrics()` trả số entries, hits, misses, bypasses, loads, published; chỉ là số tổng, không chứa dữ liệu người dùng. Hiện chưa có endpoint dashboard mới cho các số này. Có thể đọc trong debugger/diagnostic nội bộ có quyền.

Hit ratio cho request đủ điều kiện: hits / (hits + misses). Misses có thể lớn hơn loads do single-flight. Bypasses gồm các câu không được cache, không tính chúng thành lỗi cache.

Nghiệm thu: hỏi một số đếm hai lần → loads tăng 1, hits tăng 1; thay revision/đổi scope → load mới hoặc forbidden; hết 60 giây → load mới; hỏi lương hai lần → bypass, không giữ dữ liệu lương. Các test có fake clock/executor kiểm tra hành vi này. Chưa có benchmark latency trên Oracle thật: việc nạp policy vẫn tốn truy vấn ngay cả ở cache hit, nên chưa khẳng định tốc độ cải thiện bao nhiêu phần trăm.