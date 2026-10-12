---
title: "Hướng dẫn chunk và truy xuất điều khoản DataDoc"
version: "1.4.0"
status: "TECHNICAL_ARCHITECTURE_SPECIFICATION"
last_updated: "2026-10-11"
---

# Hướng dẫn chunk và truy xuất điều khoản

Tài liệu mô tả cách nguồn được đưa vào pipeline đang chạy. Kiến trúc collection, payload và công bố xem `KE_HOACH_TICH_HOP_QDRANT.md`. Không dùng tài liệu kỹ thuật này làm nội quy lao động.

## 1. Đơn vị nguồn và chunk

Đơn vị ngữ nghĩa là một điều khoản có đủ đối tượng áp dụng, điều kiện, kết quả, ngoại lệ, đơn vị/công thức và mốc hiệu lực. Không cắt điều kiện “được hưởng” khỏi trường hợp “không được hưởng”. Điều khoản ngắn giữ một chunk; không ghép điều khoản không liên quan để đủ độ dài.

Điều khoản quá dài có thể chia nhiều sub-chunk được chuẩn hóa trong nguồn. Mỗi phần cần giữ tiêu đề, đối tượng, tên cột và đơn vị nếu có bảng; lặp lại ngữ cảnh cần thiết để phần có thể tìm kiếm. Không tự chia theo số ký tự rồi mặc định nội dung còn nguyên nghĩa. CLI hiện kiểm tra chunk đã chuẩn hóa; chưa tự sinh sub-chunk từ Markdown.

```json
{
  "clause_id": "CT-01/DIEU-12_chunk_0",
  "chunk_index": 0,
  "total_chunks": 2,
  "parent_clause_id": "CT-01/DIEU-10",
  "related_clause_ids": ["CT-01/DIEU-13"]
}
```

Ví dụ này chỉ minh họa các trường chunk, không phải một điều khoản đủ schema để nạp. Nguồn phải bổ sung đầy đủ metadata tài liệu, nội dung, tenant, ACL, phê duyệt và hash. Chunk tiếp theo có index 1, cùng tenant/source/version/điều khoản và total_chunks=2. Không gộp hai phiên bản hoặc hai tenant. CLI từ chối index bị thiếu, trùng hoặc total không nhất quán.

## 2. Nạp dữ liệu

1. Đối chiếu Markdown, frontmatter và manifest với điều khoản chuẩn hóa.
2. Chỉ nhận bản chính thức đã xác minh, được bật ingest và xác định áp dụng.
3. Kiểm tra hash, tenant, ACL, ngày hiệu lực và tính đầy đủ nhóm chunk.
4. Tạo embedding chính `text` bằng model cấu hình bge-m3, 1024 chiều.
5. Nạp vào collection mới; verify; công bố alias sau cùng.

Dự thảo, benchmark và tài liệu chưa duyệt không được trộn vào alias chính thức. Cần xem trước dự thảo thì thiết kế luồng preview riêng có kiểm quyền; luồng này chưa triển khai.

## 3. Truy xuất và mở rộng bằng chứng đã triển khai

```text
Câu hỏi + danh tính đã xác thực
  -> hiểu câu hỏi và chọn capability
  -> kiểm quyền + chọn POLICY alias hoặc SQL nghiệp vụ
  -> tạo query embedding đúng model/digest
  -> lọc Qdrant theo tenant, ACL, trạng thái, verification, AsOf
  -> kiểm quyền từng hit lần nữa
  -> lấy đủ sibling chunks + parent/related trực tiếp
  -> chọn các điều khoản nguyên vẹn trong ngân sách
  -> prompt có evidence IDs -> model chat -> kiểm mã dẫn nguồn -> trả lời
```

Qdrant search ban đầu dùng ngưỡng tương đồng; mở rộng lấy bằng ID điều khoản để không làm mất ngoại lệ chỉ vì chunk ngoại lệ có điểm tìm kiếm thấp. Tất cả chunk mở rộng vẫn qua cùng bộ lọc quyền và hiệu lực.

`ExpandPolicyEvidenceAsync` hiện:

- Gom các sibling của hit nhiều chunk bằng tenant/source/version/clause.
- Đọc parent và related trực tiếp được khai báo ở hit; không đi đệ quy toàn đồ thị.
- Scroll có phân trang; kiểm lại quyền và model/digest cho từng điểm.
- Giới hạn 20 điều khoản yêu cầu mở rộng và 1000 chunk cho một điều khoản; quá giới hạn hoặc thiếu một phần làm kết quả không sẵn sàng, không đưa nửa điều khoản vào prompt.
- Không lấy parent/related từ công ty khác, phiên bản khác hoặc tài liệu hết hiệu lực để bù nội dung.

Các giới hạn này là giới hạn kỹ thuật hiện tại, không phải chỉ tiêu chất lượng chứng minh đủ cho mọi bộ tài liệu. Corpus thật hiện có 26 điều khoản một chunk; tình huống multi-chunk cần tiếp tục có dữ liệu đánh giá khi quy chế công ty được duyệt.

## 4. Ngân sách prompt

`SelectPolicyEvidence` nhóm theo tenant/source/version/clause, xếp theo điểm cao nhất trong nhóm, kiểm tra đủ chunk và chọn cả nhóm. Không còn cố định `Take(3)`. Nếu nhóm vượt ngân sách, bỏ cả nhóm; không cắt đoạn cuối làm mất ngoại lệ.

Ngân sách evidence của Ollama dựa trên context window và phần dành cho output/system; hiện ước lượng bằng số ký tự, không dùng tokenizer chính xác. C# trừ thêm câu hỏi và lịch sử; nhánh SQL + tài liệu trừ cả nội dung số liệu SQL. Vì vậy không cam kết chọn toàn bộ corpus hoặc đếm token tuyệt đối. Khi chưa có bằng chứng đủ trong ngân sách, không coi câu trả lời model tự tạo là quy chế xác minh.

Mỗi chunk được gắn nhãn:

```text
[E1] Tên tài liệu | PL-01/DIEU-01 | v1.1.0
Nội dung điều khoản được phép đọc...
```

Đầu vào model gồm câu hỏi, lịch sử phù hợp và bằng chứng đã qua quyền. Không đưa hit bị từ chối vào prompt. Nội dung tài liệu được coi là dữ liệu, không phải lệnh ghi đè system prompt.

## 5. Dẫn nguồn và trả lời

- Prompt yêu cầu dùng các mã `[E1]`, `[E2]` đã cung cấp.
- `HasOnlyKnownEvidenceCitations` từ chối `[E0]` hoặc chỉ số vượt tập bằng chứng; khi không nhận được câu trả lời phù hợp, giữ bằng chứng đã xác minh.
- Kiểm tra hiện tại chỉ xác minh mã dẫn nguồn có tồn tại. Chưa bắt buộc mỗi mệnh đề có dẫn nguồn, chưa xác minh ngữ nghĩa toàn câu trả lời hoặc bảo đảm model không suy diễn sai.
- Nguồn trả lời cần phân biệt luật quốc gia với quy chế công ty; không biến mức thưởng ví dụ/dự thảo thành quyền lợi đang áp dụng.
- Log `QDRANT_RESULT` chứa số hit, điểm, nguồn và phần văn bản được phép sau redaction; `VECTOR_CONTEXT` ghi các bằng chứng được đưa vào prompt. Không log toàn nội dung hit bị từ chối.

Lỗi `Unavailable`/`InvalidIndex` phải hiển thị lỗi dịch vụ; `Timeout` báo quá thời gian chờ; `Cancelled` báo hủy. Chỉ `NoMatch` mới dùng thông báo không có tài liệu phù hợp. Nhánh hybrid có số liệu SQL vẫn có thể trả số liệu đã được phép, kèm ghi chú phần đối chiếu văn bản chưa thực hiện được.

## 6. Phân biệt hai dạng hybrid

| Dạng | Hiện trạng |
|---|---|
| SQL + tài liệu | Đã có: Oracle cung cấp số liệu sau kiểm quyền; Qdrant cung cấp bằng chứng chính sách |
| Dense + sparse/BM25, fusion và rerank | Chưa triển khai; không gọi pipeline hiện tại là đã có RRF hoặc reranker |

Qdrant không phải nguồn số liệu nhân sự cập nhật theo thời gian thực. Câu hỏi “lương tháng này của nhân viên 21” phải đi qua API/Oracle có quyền; câu hỏi “nguyên tắc trả lương ca đêm” dùng tài liệu. Câu hỏi kết hợp cần kiểm quyền ở cả hai nguồn.

## 7. Kiểm thử và giới hạn đã biết

Kiểm thử hiện có bao gồm tenant công ty/GLOBAL, deny, ACL rỗng, nhóm/phòng ban, metadata và thời gian không hợp lệ, vector sai dimension, nhóm chunk không đầy đủ, ngân sách giữ nguyên điều khoản, mã dẫn nguồn không tồn tại và cách báo lỗi truy xuất.

Kiểm thử Qdrant/Ollama thật đã chạy: hỏi giờ làm việc trả điều khoản PL-01/DIEU-01; AsOf trước ngày hiệu lực không có kết quả; câu ngoài corpus không nhận hit vượt ngưỡng. Ma trận 10 câu hỏi đạt 10/10; tiêu chí negative là không có hit, đã bỏ tiêu chí cũ có thể cho qua hit không liên quan. Ma trận này đánh giá retrieval, không chứng minh chất lượng toàn bộ câu trả lời của model.

V1_35 và đọc membership có HMAC đã xác minh. Chưa có tài liệu công ty chính thức ROLE_RESTRICTED để test xuyên suốt tài liệu thật với người dùng có nhóm. Chưa chạy lại toàn bộ giao diện desktop/web trong phiên đăng nhập thực tế. Tra cứu ngày lịch sử từ câu hỏi, sparse retrieval/reranking và kiểm chứng ngữ nghĩa câu trả lời còn là việc mở rộng.
