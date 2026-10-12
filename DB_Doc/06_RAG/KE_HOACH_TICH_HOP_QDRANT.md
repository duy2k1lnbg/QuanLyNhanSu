---
title: "Kiến trúc lưu trữ và công bố DataDoc trong Qdrant"
version: "1.6.0"
status: "TECHNICAL_ARCHITECTURE_SPECIFICATION"
last_updated: "2026-10-12"
---

# Kiến trúc lưu trữ DataDoc trong Qdrant

Tài liệu này mô tả phần đã triển khai và kiểm tra ngày 12/10/2026. Các tính năng chưa triển khai được liệt kê riêng ở cuối. Tài liệu kỹ thuật này không phải quy chế công ty và không được đưa vào kho trả lời chính sách.

## 1. Trách nhiệm từng nguồn dữ liệu

| Nguồn | Trách nhiệm |
|---|---|
| Oracle và API nghiệp vụ | Danh tính, quyền, nhóm, dữ liệu nhân viên, lương, chấm công và số liệu cập nhật |
| Markdown trong DB_Doc | Tài liệu nguồn, phiên bản, hiệu lực và phê duyệt |
| `clauses.jsonl`, schema và manifest | Điều khoản đã chuẩn hóa, hợp đồng metadata, kiểm tra nội dung và nguồn trước khi nạp |
| Qdrant | Bản chỉ mục phục vụ tìm kiếm; không thay thế Oracle hoặc tài liệu nguồn |
| Ollama | `bge-m3:latest` tạo embedding; model chat được cấu hình riêng để diễn giải bằng chứng |

Không nạp bảng lương hoặc toàn bộ dữ liệu cá nhân vào collection tài liệu chỉ để AI trả lời. Quyền đọc dữ liệu nghiệp vụ và quyền đọc văn bản là hai bước kiểm tra riêng.

## 2. Collection và alias đang chạy

```text
POLICY / REGULATION -> hrms_knowledge_active
                        -> hrms_knowledge_bgem3_20261012T054201_5cfefe2a
EMPLOYEE            -> hrms_vectors_v2
```

- Collection tri thức mới: **112 điểm**, embedding **1024 chiều**, khoảng cách **Cosine**.
- Collection tri thức cũ `hrms_knowledge_bgem3_v1` được giữ lại sau công bố.
- Collection nhân viên có **297 điểm**; pipeline DataDoc không ghi vào collection này.
- Corpus nguồn: **123 điều khoản**, **112 đủ điều kiện**, **11 bị loại**. Có 29 điều khoản pháp luật thuộc PL-01 đến PL-06 và 83 điều khoản nội bộ thuộc 21 tài liệu CT/BM/FAQ/HD/SOP. Bộ nội bộ đã được chủ sở hữu phê duyệt cho **công ty demo mã 1**, hiệu lực 12/10/2026, phiên bản `2.0.0-demo`; không phải xác nhận đăng ký nội quy với cơ quan nhà nước. FC-01/02/03 vẫn là benchmark ngoài công ty. PL-04 đã đối chiếu phụ lục chính thức và được nạp dưới dạng bảng tra pháp luật; phải hỏi nơi làm việc thực tế khi kết luận vùng áp dụng cho công ty.
- C# đọc alias qua cấu hình `Qdrant:KnowledgeCollection`; dữ liệu nhân viên dùng collection riêng. Kiểm tra readiness của POLICY phải kiểm tra collection tri thức, không phụ thuộc trạng thái writer nhân viên.
- `QdrantService.AddAsync` từ chối ghi POLICY/REGULATION theo đường writer nhân viên. CLI là đường nạp tri thức hiện tại.

Tên physical collection trong tài liệu là snapshot đã công bố ở lần kiểm tra này. Các lần nạp sau sinh tên mới; đọc receipt và `/aliases` để biết snapshot hiện hành.

## 3. Điều kiện được công bố

Một điều khoản phải đồng thời có:

1. `status = ACTIVE_OFFICIAL`, `verification_status = OFFICIAL_VERIFIED`.
2. `ingest_enabled = true`, `applies_to_current_company = true`.
3. Không còn `unresolved_fields`; `applicability_mode` khác `UNRESOLVED`.
4. Nội dung không rỗng, `content == text`, SHA-256 nội dung đúng.
5. Có phiên bản, người duyệt, tiêu đề và ngày hiệu lực hợp lệ.
6. Frontmatter nguồn và manifest cho phép nạp; SHA-256 tệp nguồn đúng; metadata điều khoản và tài liệu không lệch nhau, bao gồm ACL.
7. Tenant và ACL hợp lệ; nhóm chunk phải đầy đủ, liên tục và không trùng định danh.

Dự thảo, bản đối chiếu/benchmark và tài liệu chưa giải quyết phạm vi áp dụng không được phục vụ qua alias chính thức. Metadata sai ở điều khoản đủ điều kiện phải làm dừng ingestion, không âm thầm nạp một phần.

## 4. Định danh và hợp đồng payload

Đơn vị lưu là một chunk thuộc một điều khoản, trong một phiên bản, của một tenant:

```text
logicalIdentity = tenantId:sourceId:version:clauseId:chunkIndex
pointId = SHA1("HRMS_QDRANT_NAMESPACE:" + logicalIdentity)
          lấy 16 byte đầu, gán bit version 5 / RFC4122 variant,
          format byte theo thứ tự hex thành UUID
```

Đây là công thức namespace tùy chỉnh hiện có; không thay bằng UUIDv5 với một UUID namespace khác. C# và Python phải cho cùng kết quả. `clauseId` được chuẩn hóa bỏ hậu tố `_chunk_N`; chỉ số nằm ở `chunkIndex`. Point ID ổn định giữa các lần rebuild cùng phiên bản; đổi nội dung phải cập nhật hash và phiên bản theo quy trình tài liệu.

| Nhóm payload | Trường chính |
|---|---|
| Định danh | `tenantId`, `sourceId`, `clauseId`, `section`, `chunkId`, `chunkIndex`, `totalChunks`, `version` |
| Nội dung và dẫn nguồn | `text`, `title`, `documentTitle`, `sourceKind`, `documentType`, `sourceUrls`, `sourcePages`, `legalBasis` |
| Liên kết điều khoản | `parentClauseId`, `relatedClauseIds` |
| Trạng thái và phạm vi | `domain=POLICY`, `tag=REGULATION`, `status`, `verificationStatus`, `topics`, `jurisdiction`, `applicableLocation`, `applicabilityMode`, `appliesToCurrentCompany`, `companyId` |
| Quyền | `accessProfile`, `allowedRoleIds`, `allowedDepartmentIds` |
| Hiệu lực | `effectiveFrom`, `effectiveTo`, `approvedBy`, `supersedes`, `unresolvedFields` |
| Kiểm chứng | `contentHash`, `metadataHash`, `embeddingModel`, `embeddingDigest`, `embeddingDimension`, `embeddingInputVersion`, `corpusRevision`, `ingestionRunId`, `importedAt`, `updatedAt` |

`section` giữ cùng mã với `clauseId` để tương thích reader. Payload dùng camelCase; nguồn có thể dùng snake_case theo schema. Nếu cả hai tên ACL xuất hiện mà khác giá trị, ingestion từ chối.

### Tenant và quyền

- `GLOBAL` chỉ dành cho `source_kind = VIETNAM_LAW` không thuộc một công ty.
- Tài liệu công ty, kể cả áp dụng pháp luật Việt Nam, phải có `company_id` khác GLOBAL; `tenantId` bằng mã công ty đó.
- `PUBLIC_INTERNAL` vẫn phải qua danh tính đã xác thực, capability, tenant, hiệu lực và trạng thái.
- Tài liệu giới hạn theo nhóm/phòng ban phải có ACL không rỗng. ACL rỗng không được hiểu là toàn quyền, kể cả ADMIN.
- `allowedRoleIds` là ID nhóm Oracle dưới dạng chuỗi, không phải tên nhóm tự đặt. Vai trò do API lấy qua thủ tục có HMAC; không nhận vai trò do client tự khai báo.
- Deny được ưu tiên; ADMIN không bỏ qua tenant, tình trạng tài liệu và ngày hiệu lực. ADMIN có scope ALL có thể đọc tài liệu giới hạn có ACL hợp lệ.

### Hiệu lực thời gian

Ngày trong nguồn là ngày Việt Nam. Ví dụ `2021-01-01` bắt đầu lúc 00:00 UTC+7, lưu thành `2020-12-31T17:00:00Z`.

Khoảng áp dụng là `[effectiveFrom, effectiveTo)`: đầu bao gồm, cuối không bao gồm; null/chuỗi rỗng ở ngày cuối nghĩa là chưa có ngày kết thúc. Ngày sai định dạng bị từ chối. C# so sánh bằng `AsOf`; luồng chat hiện truyền giờ UTC của máy chủ. Tra cứu một thời điểm lịch sử đã được kiểm thử ở tầng service; việc tự hiểu ngày lịch sử từ câu hỏi còn cần tích hợp riêng.

## 5. Model và payload index

Descriptor lần công bố này:

```json
{
  "model": "bge-m3:latest",
  "digest": "7907646426070047a77226ac3e684fbbe8410524f7b4a74d02837e43f2146bab",
  "dimension": 1024,
  "distance": "Cosine",
  "inputVersion": "clause-text-v1"
}
```

Embedding đầu vào hiện là `text` của điều khoản. Không âm thầm đổi sang nối title/header hoặc model khác vì sẽ thay không gian vector. CLI kiểm tra model được yêu cầu tồn tại bằng `/api/tags`, kiểm tra dimension, số hữu hạn và vector khác zero. Reader so sánh model và digest khi dùng Ollama thật; đổi model/digest cần rebuild snapshot phù hợp trước khi công bố.

Đã tạo và kiểm tra **22 payload index**:

- Keyword: `tenantId`, `sourceId`, `clauseId`, `section`, `domain`, `tag`, `documentType`, `sourceKind`, `status`, `accessProfile`, `topics`, `version`, `corpusRevision`, `companyId`, `allowedRoleIds`, `verificationStatus`, `applicabilityMode`, `embeddingModel`, `embeddingDigest`.
- Datetime: `effectiveFrom`, `effectiveTo`.
- Integer: `allowedDepartmentIds`.

Qdrant đang cài đặt nhận lọc datetime dưới key `range` với giá trị ISO-8601; `datetime_range` đã thử và trả 422. Không thay cú pháp chỉ dựa vào ví dụ của phiên bản khác.

## 6. Pipeline CLI đã triển khai

Chạy từ thư mục `D:\QL_NS\QuanLyNhanSu`, sử dụng Python có sẵn:

```powershell
python tools/datadoc_ingestion_cli.py validate
python tools/datadoc_ingestion_cli.py dry-run
python tools/datadoc_ingestion_cli.py apply --receipt scratch/new_knowledge_run.json
python tools/datadoc_ingestion_cli.py verify --receipt scratch/new_knowledge_run.json
python tools/datadoc_ingestion_cli.py publish --receipt scratch/new_knowledge_run.json
```

| Bước | Tác động |
|---|---|
| validate | Kiểm tra nguồn/schema/manifest/điều khoản, không ghi Qdrant |
| dry-run | Ghi báo cáo điều khoản đủ điều kiện và lý do bị loại, không ghi Qdrant |
| apply | Tạo physical collection mới, tạo index, embedding/upsert, checkpoint receipt; giữ nguyên serving alias |
| verify | So tập ID chính xác, toàn payload, hash, vector, index, model digest, count nhân viên và smoke retrieval |
| publish | Bắt buộc receipt đã verify; kiểm tra lại ngay trước công bố; đổi alias nguyên tử rồi đọc lại để xác nhận |

Receipt mặc định hiện là `scratch/datadoc_ingestion_receipt_v2.json`. Receipt cũ `datadoc_ingestion_receipt.json` giữ lại làm bằng chứng trước sửa, không dùng làm trạng thái hiện hành. Receipt công bố hiện hành: `D:\QL_NS\artifacts\qdrant-extra-rights-20261012\ingestion-extra.json`. Hồ sơ phê duyệt: `docs/PHE_DUYET_QUY_CHE_DEMO_20261012.md`. Receipt ngày 11/10/2026 được giữ làm bằng chứng lịch sử.

Đây là rebuild toàn bộ corpus vào snapshot bất biến. CLI từ chối ghi collection nhân viên, collection đang được alias trỏ tới hoặc collection staging đã tồn tại. Checkpoint phục vụ kiểm tra lỗi; chưa hỗ trợ tự tiếp tục một staging dở dang. Thất bại trước publish giữ nguyên alias đang chạy. Không xóa snapshot cũ tự động.

Rollback bằng một lệnh alias nguyên tử trỏ `hrms_knowledge_active` về physical cũ trong receipt, sau khi kiểm tra reader vẫn tương thích. Snapshot cũ trước sửa thiếu một số metadata mới; không xem việc giữ snapshot đó là bảo đảm rollback không cần kiểm tra. Không hạ điều kiện kiểm quyền để ép snapshot cũ trả kết quả.

## 7. Luồng đọc và Oracle V1_35

1. API xác thực người dùng, nạp capability và quyền từ Oracle.
2. Với POLICY, C# xây bộ lọc trạng thái/verification/tenant/ACL/AsOf trước truy vấn Qdrant.
3. Sau khi Qdrant trả kết quả, kiểm lại metadata và quyền, model/digest.
4. Mở rộng toàn điều khoản và liên kết trực tiếp theo hướng dẫn chunking.
5. Chọn bằng chứng nguyên vẹn trong ngân sách rồi chuyển cho model chat.

V1_35 đã được người vận hành chạy và xác minh: `AI_OWNER.GET_ACTOR_ROLE_IDS` ở trạng thái VALID, có EXECUTE cho AI_READONLY; proof hợp lệ được nhận, chữ ký sai và nonce phát lại bị từ chối. Provider C# đã đọc thành công tài khoản ADMIN ID 80; tài khoản này trả danh sách nhóm rỗng trong lần kiểm tra. Chưa có tài liệu ROLE_RESTRICTED trong corpus hiện hành để kiểm thử xuyên suốt với người dùng có nhóm thật.

Thủ tục dùng `PKG_AI_AUTH.GET_ACTOR_SNAPSHOT` xác thực proof trước khi đọc membership qua view nguồn bên trong ranh giới AI_OWNER. Không cấp AI_READONLY quyền SELECT trực tiếp trên HR. Nếu thiếu thủ tục/quyền gọi, provider giữ roles rỗng; tài liệu cần nhóm không được tự mở quyền.

## 8. Cache, kiểm thử và phần chưa triển khai

Embedding cache POLICY có model và host/digest trong khóa; membership đã xác thực được đưa vào fingerprint quyền. POLICY chưa dùng result cache để phát lại câu trả lời chính sách. Cache số liệu COUNT nhân viên hiện có là nhánh khác. Không mô tả cache descriptor/corpusRevision cho POLICY như tính năng đã có.

Đã kiểm tra validation ingestion, bộ lọc tenant/ACL/date, chọn toàn điều khoản, mã dẫn nguồn, transport desktop và lỗi truy xuất; kiểm thử đọc Qdrant/Ollama thật và ma trận 10 câu hỏi. Kết quả chi tiết nằm trong báo cáo sửa tại thư mục artifacts.

Những phần còn là hướng mở rộng: sparse/BM25 + dense, RRF, reranker, incremental ingestion/resume, công cụ biên tập/phê duyệt, tự hiểu ngày lịch sử trong chat, kiểm tra đúng sai ngữ nghĩa của toàn câu trả lời và bộ đánh giá đầy đủ quy chế công ty. Luồng hiện tại là SQL nghiệp vụ kết hợp dense retrieval tài liệu; chưa phải dense+sparse hybrid retrieval.
