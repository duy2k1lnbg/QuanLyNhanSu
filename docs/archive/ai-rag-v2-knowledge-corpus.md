# Nhánh tài liệu quy chế — chưa triển khai

Cập nhật 03/10/2026. Luồng V2 hiện trả unsupported cho câu hỏi quy chế, và `QdrantService.SearchScopedAsync` trả kết quả rỗng với trạng thái disabled. Không fallback sang tìm không có quyền. Không có migration tạo corpus giả hay chạy sync Qdrant trong đợt sửa này.

Các hình tham khảo về chunking, embedding, dense/sparse/hybrid, reranking và multi-query là hướng thiết kế có thể áp dụng cho tài liệu văn bản. Chúng không thay thế bước xác định metric/đối tượng/kỳ và quyền khi hỏi dữ liệu bảng. Tổng giờ, tổng lương và số người vẫn được tính bằng SQL có quyền.

## Điều kiện để bật tiếp

1. Chủ dữ liệu xác nhận bảng/view ánh xạ tài liệu gốc được phép cho AI đọc; không thêm nguồn ngoài ý muốn người dùng.
2. Xác nhận document ID, nội dung, phiên bản, ngày hiệu lực, trạng thái xóa và quyền truy cập hiện tại. Không suy quyền từ chỉ metadata Qdrant hoặc target employee trong câu hỏi.
3. Chunk kế thừa ACL tài liệu, cùng source ID/version; có thể dùng parent-child theo cấu trúc. Không mặc định 300–800 token là kích thước tối ưu cho mọi tài liệu.
4. Lọc quyền ở retrieval, rồi xác minh lại nguồn/version/ACL hiện tại trước hydrate nội dung. Fallback chỉ được nới business tag, giữ quyền. Cache retrieval phải bao gồm policy/source revision.
5. Thử BM25 + dense, hợp nhất bằng RRF; rerank sau lọc quyền. Multi-query chỉ chạy khi đã hiểu rõ intent và giữ nguyên constraints.
6. Đánh giá recall/precision/citation/freshness/latency trên bộ câu hỏi có đáp án và kiểm thử prompt injection trước bật tính năng.

## Bảng/view chỉ là đề xuất cho giai đoạn sau

Có thể cần catalogue ánh xạ `V_AI_KNOWLEDGE_DOCUMENT` và `V_AI_KNOWLEDGE_ACL`, hoặc bảng metadata `TB_AI_DOCUMENT`, `TB_AI_DOCUMENT_CHUNK`, `TB_AI_DOCUMENT_ACL` trong schema sở hữu. Chưa có các đối tượng này trong migration hiện tại. Chọn sau khi biết nguồn thực, tránh nhân bản lại bảng gốc vô ích. AI_READONLY chỉ được SELECT view đã lọc, không được ghi document/chunk/index/cache.

Việc chứng minh search bị tắt an toàn không chứng minh hybrid search/reranking hoặc kiểm tra metadata cũ đã được triển khai. Các tiêu chí nghiệm thu nhánh này vẫn để mở.