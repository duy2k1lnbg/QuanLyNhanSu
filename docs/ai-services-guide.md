# Hướng Dẫn Kiến Trúc & Vận Hành AI_Services (HRMS Enterprise)

Tài liệu này cung cấp quy chuẩn thiết kế, cấu trúc tổ chức mã nguồn, phân định trách nhiệm chi tiết cho từng thư mục, hợp đồng dữ liệu (DTO), luồng thực thi (execution pipeline), ma trận phân quyền và sổ tay vận hành cho toàn bộ phân hệ Trợ lý Trí tuệ Nhân tạo & RAG (Retrieval-Augmented Generation) của hệ thống HRMS Enterprise.

- **Ngày cập nhật & rà soát thực tế:** 09/10/2026  
- **Môi trường áp dụng:** ASP.NET Web API 2 (.NET Framework 4.7.2) / Windows Desktop (WinForms) / React Vite Web / Oracle Database 19c / Qdrant Vector Engine 1.19.0 / Ollama Local LLM (`bge-m3:latest`, `qwen2.5:latest`)  
- **Cơ chế xác thực & định danh:** Bearer JWT kết hợp Snapshot Ủy quyền (`AiAuthorizationContext`)  

---

## 1. Phạm Vi, Ranh Giới & Nguyên Tắc Thiết Kế

1. **Bảo tồn toàn vẹn mã nguồn (KHÔNG XÓA FILE C#):**
   - Toàn bộ **53 file C#** nghiệp vụ AI được giữ nguyên vẹn 100% basename và được quy hoạch vào đúng **14 nhóm chức năng nghiệp vụ**.
   - Mọi class, struct, enum và public interface đều được duy trì tính tương thích ngược (backward compatibility) cho cả 3 nền tảng: Web API 2 (`HRMS.Api`), Ứng dụng Desktop (`HRMS.Desktop`), và Công cụ đồng bộ dữ liệu (`HRMS.VectorDataSync`).
2. **Nguyên tắc "Fail-Closed" & Bảo mật đa tầng (Multi-Layer Security):**
   - Danh tính người dùng (`AiAuthorizationContext`) do Server trích xuất từ Token phiên làm việc (JWT Claims); tuyệt đối không tin cậy hoặc nhận vai trò, quyền hạn do Client tự khai báo qua Request Body/Header.
   - Quyền hạn phân định rõ 3 lớp độc lập: **Quyền nền tảng** (`F_SYSTEM_AI`), **Quyền nghiệp vụ chức năng** (16 Capability trong `AiCapabilityCatalog`), và **Phạm vi dữ liệu hiệu lực** (`EffectiveScope`: `SELF`, `DEPARTMENT`, `COMPANY`, `ALL`).
   - Mọi kế hoạch truy vấn (SQL, Vector, Hybrid) bắt buộc phải vượt qua bước kiểm tra quyền (`ValidateCapability` và Scope check) **trước khi** phát sinh bất kỳ I/O nào tới Oracle Database hoặc Qdrant. Nếu thiếu quyền, lập tức từ chối (`ExecutionStrategy.Forbidden`), không thực thi ngầm.
3. **Phân tách ranh giới nguồn sự thật (Source of Truth):**
   - **Oracle Database 19c:** Nguồn sự thật duy nhất và tuyệt đối cho dữ liệu có cấu trúc: hồ sơ nhân sự, dữ liệu chấm công, tính lương, phụ cấp, hợp đồng, tăng ca, bảo hiểm và bảng phân quyền theo kỳ. Không lưu số liệu tài chính hay lương vào Qdrant.
   - **Qdrant Vector Database:** Lưu trữ embeddings ngữ nghĩa của văn bản quy chế, nội quy, chính sách nhân sự và thông tin chỉ mục định danh tối thiểu của nhân viên đang làm việc (`hrms_vectors_v2`). Không chứa PII nhạy cảm (CCCD, tài khoản ngân hàng, lương thỏa thuận).

---

## 2. Cây Thư Mục & Bảng Manifest Chi Tiết 53 File AI

### 2.1. Cây Thư Mục Thực Tế (Phân Nhóm 14 Thư Mục Nghiệp Vụ)

```text
HRMS.Business/Services/AI_Services/
├── Bootstrap/
│   └── AiServiceLocator.cs                      # Service Locator & DI Container (Lazy thread-safe)
├── Configuration/
│   └── AiConfigurationCoordinator.cs            # Quản trị cấu hình Ollama/Qdrant, kiểm tra version CAS, probe SSRF-safe
├── Chat/
│   ├── AiExecutionService.cs                    # Central Orchestrator duy nhất điều phối toàn bộ pipeline chat
│   └── ChatboxManager.cs                        # UI Adapter cho Desktop/Web, quản lý session chat và DTO
├── Understanding/
│   ├── AiRouterService.cs                       # Phân loại intent trợ giúp trong allowlist
│   ├── ClarificationPolicy.cs                   # Chính sách làm rõ câu hỏi mơ hồ, sinh ClarificationToken (TTL 10 phút)
│   ├── EntityResolver.cs                        # Phân giải thực thể nhân viên/phòng ban/kỳ công có kiểm tra Scope
│   ├── QueryPreprocessor.cs                     # Chuẩn hóa Unicode tiếng Việt, khoảng trắng, dấu câu
│   ├── QueryUnderstandingModels.cs              # DTO: Intent, Entity, ClarificationToken, DateRangeToken
│   └── QueryUnderstandingService.cs             # Pipeline hiểu câu hỏi (Preprocessor -> Intent -> Resolver -> Clarification)
├── Planning/
│   ├── ExecutionPlan.cs                         # Model đặc tả kế hoạch: SqlTemplate, DeterministicDirect, VectorSearch, Hybrid
│   └── QueryPlanner.cs                          # Lập ExecutionPlan dựa trên Intent, Entity và phân quyền Fail-Closed
├── Retrieval/
│   ├── Sql/
│   │   ├── AiSchemaService.cs                   # Catalog metadata bảng/cột được phép tra cứu trong AI (chưa Compile)
│   │   ├── OracleSqlAstValidator.cs             # AST validator kiểm tra cú pháp và allowlist câu lệnh SQL Oracle
│   │   ├── SafeSqlExecutor.cs                   # Adapter kiểm tra AST và ủy quyền an toàn cho ScopedSqlExecutor
│   │   ├── ScopedSqlExecutor.cs                 # Điểm truy cập Oracle DB duy nhất trong AI: áp dụng scope ALL/COMPANY/DEPT/SELF
│   │   └── SqlGeneratorService.cs               # Trình chọn mẫu SQL template từ catalog đã duyệt; không sinh SQL tùy ý
│   ├── Vector/
│   │   ├── QdrantService.cs                     # Client HTTP REST Qdrant: SearchScopedAsync có fail-closed ACL
│   │   └── VectorSearchModels.cs                # Model VectorSecurityFilter, VectorHit, VectorSearchResult, Citation
│   └── Hybrid/
│       ├── HybridRagService.cs                  # Service phối hợp đa nguồn (Oracle DB + Qdrant Vector) theo ExecutionPlan
│       └── RagContextRetriever.cs               # Thu thập bằng chứng (evidence) có kiểm soát provenance và lọc quyền
├── Responses/
│   ├── DeterministicResponseRenderer.cs         # Dựng câu trả lời số liệu chuẩn xác từ Oracle SQL, không qua LLM bịa đặt
│   ├── FastResponseService.cs                   # Phản hồi tĩnh tức thì cho câu chào hỏi, trợ giúp chức năng
│   └── RagSynthesizer.cs                        # Tổng hợp văn phong tự nhiên từ bằng chứng đã kiểm duyệt bằng LLM
├── Providers/
│   └── Ollama/
│       └── OllamaService.cs                     # HTTP Client kết nối Ollama API (/api/chat, /api/embeddings) có CancellationToken
├── Prompts/
│   └── JsonPromptManager.cs                     # Quản lý và nạp System Prompt từ tài nguyên runtime JSON có phiên bản
├── Memory/
│   ├── Cache/
│   │   ├── AiCacheCoordinator.cs                # Bộ nhớ đệm thread-safe: bounded cardinality, TTL, single-flight, CAS invalidation
│   │   └── AiCacheService.cs                    # Facade truy xuất cache tiện lợi gắn liền Authorization Fingerprint
│   └── Conversations/
│       ├── AiChatHistory.cs                     # Data model lưu trữ danh sách tin nhắn của phiên hội thoại
│       └── ConversationStateManager.cs          # Quản lý vòng đời phiên chat, pending clarification state (TTL 10 phút)
├── Security/
│   ├── Authorization/
│   │   ├── AiAuthorizationContext.cs            # Snapshot quyền người dùng: UserId, MaNV, MaCty, MaPB, Capabilities, Scopes
│   │   ├── AiAuthorizationService.cs            # Đánh giá quyền sử dụng AI và kiểm duyệt Capability từng chức năng
│   │   ├── AiCapabilityCatalog.cs               # Danh mục 16 Capability chuẩn của HRMS AI và quyền cơ sở nền tảng
│   │   └── AiPolicyProvider.cs                  # Nạp chính sách phân quyền và thông tin tài khoản người dùng từ Oracle DB
│   ├── Scopes/
│   │   ├── AiScopeEvaluator.cs                  # Đánh giá một bản ghi nhân sự có nằm trong phạm vi hiệu lực của người hỏi
│   │   ├── AiScopeGrantManagementService.cs     # Dịch vụ quản trị phân quyền phạm vi AI (Cấp, Sửa, Thu hồi Grant)
│   │   ├── AiScopeGrantModels.cs                # DTO truyền nhận dữ liệu quản trị scope grant
│   │   └── AiScopePolicyResolver.cs             # Hợp nhất grant cá nhân, grant nhóm, quy tắc DENY ưu tiên cao nhất
│   ├── Repositories/
│   │   ├── IAiScopeGrantRepository.cs           # Contract lưu trữ phân quyền AI Scope trên cơ sở dữ liệu
│   │   ├── InMemoryAiScopeGrantRepository.cs    # Mock in-memory repository phục vụ Unit Test cô lập
│   │   └── OracleAiScopeGrantRepository.cs      # Triển khai lưu trữ persistence trên bảng AI_OWNER.TB_AI_SCOPE_GRANT
│   └── Proofs/
│       └── AiHmacProofService.cs                # Sinh và xác thực chữ ký HMAC-SHA256 bảo vệ toàn vẹn phiên trao đổi
├── Indexing/
│   ├── AiDataSyncHub.cs                         # Tiếp nhận sự kiện nghiệp vụ (Thêm/Sửa/Xóa NV) và kích hoạt đồng bộ
│   └── QdrantOutboxManager.cs                   # Hàng đợi Outbox bền vững đồng bộ vector sang Qdrant, ACK khi thành công
├── Interfaces/
│   ├── ILlmService.cs                           # Contract giao tiếp với mô hình ngôn ngữ lớn (Chat, Embedding)
│   ├── IPromptManager.cs                        # Contract quản lý system prompt theo phiên bản
│   ├── IRagContextRetriever.cs                  # Contract thu thập bằng chứng ngữ cảnh cho RAG
│   ├── IRagSynthesizer.cs                       # Contract tổng hợp câu trả lời từ bằng chứng ngữ cảnh
│   ├── ISafeSqlExecutor.cs                      # Contract thực thi SQL an toàn có kiểm định AST
│   ├── IScopedSqlExecutor.cs                    # Contract thực thi SQL có áp đặt phạm vi người dùng (Scoped SQL)
│   ├── ISqlGenerator.cs                         # Contract chọn mẫu SQL có cấu trúc từ template catalog
│   └── IVectorService.cs                        # Contract thao tác với Qdrant Vector Database
└── Runtime/
    └── Time/
        └── IClockProvider.cs                    # Interface cung cấp thời gian: SystemClockProvider và FakeClockProvider (cho test)
```

### 2.2. Bảng Đối Soát 53 File (Old Path -> New Path -> Caller & Trạng Thái)

| STT | File Gốc (Đường Dẫn Cũ) | Vị Trí Mới (14 Nhóm Chức Năng) | Vai Trò & Trách Nhiệm Thực Tế | Caller Chính Trong Ứng Dụng | Trạng Thái Compile |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | `AiServiceLocator.cs` | `Bootstrap/AiServiceLocator.cs` | Service Locator & DI Container; khởi tạo Lazy singleton đa luồng | `AiChatController`, `ChatboxManager`, `AiBootstrap` | Included |
| 2 | `ChatboxManager.cs` | `Chat/ChatboxManager.cs` | Adapter UI Desktop/Web; điều khiển session, DTO và hủy tác vụ | `FrmAI_Chat.cs`, `FrmAI.cs` | Included |
| 3 | `Core/AiConfigurationCoordinator.cs` | `Configuration/AiConfigurationCoordinator.cs` | Quản lý cấu hình Ollama/Qdrant, CAS versioning, chống SSRF | `AiChatController`, `FrmOllamaConfig`, `OllamaService` | Included |
| 4 | `Core/AiExecutionService.cs` | `Chat/AiExecutionService.cs` | Bộ điều phối trung tâm duy nhất xử lý luồng chat toàn hệ thống | `AiChatController.Chat`, `ChatboxManager` | Included |
| 5 | `Core/AiRouterService.cs` | `Understanding/AiRouterService.cs` | Phân loại intent trợ giúp trong danh mục allowlist | `QueryUnderstandingService` | Included |
| 6 | `Core/ClarificationPolicy.cs` | `Understanding/ClarificationPolicy.cs` | Phát hiện câu hỏi mơ hồ, sinh ClarificationToken (TTL 10 phút) | `QueryUnderstandingService`, `AiExecutionService` | Included |
| 7 | `Core/DeterministicResponseRenderer.cs` | `Responses/DeterministicResponseRenderer.cs` | Dựng câu trả lời chính xác từ Oracle SQL, không qua LLM bịa đặt | `AiExecutionService` | Included |
| 8 | `Core/EntityResolver.cs` | `Understanding/EntityResolver.cs` | Phân giải nhân viên/phòng ban/kỳ công có kiểm tra Scope | `QueryUnderstandingService` | Included |
| 9 | `Core/ExecutionPlan.cs` | `Planning/ExecutionPlan.cs` | Model kế hoạch thực thi: SqlTemplate, VectorSearch, Hybrid | `QueryPlanner`, `AiExecutionService` | Included |
| 10 | `Core/FastResponseService.cs` | `Responses/FastResponseService.cs` | Phản hồi tức thì cho câu chào hỏi, trợ giúp cú pháp hệ thống | `AiExecutionService` | Included |
| 11 | `Core/HybridRagService.cs` | `Retrieval/Hybrid/HybridRagService.cs` | Phối hợp đồng thời kết quả giữa Oracle SQL và Qdrant Vector | `AiExecutionService`, `RagContextRetriever` | Included |
| 12 | `Core/IClockProvider.cs` | `Runtime/Time/IClockProvider.cs` | Cung cấp thời gian hệ thống và đồng hồ giả lập cho kiểm thử | `AiCacheCoordinator`, `ConversationStateManager` | Included |
| 13 | `Core/JsonPromptManager.cs` | `Prompts/JsonPromptManager.cs` | Nạp system prompt từ file JSON tài nguyên đóng gói có version | `RagSynthesizer`, `AiExecutionService` | Included |
| 14 | `Core/OracleSqlAstValidator.cs` | `Retrieval/Sql/OracleSqlAstValidator.cs` | Kiểm định cú pháp AST và allowlist câu lệnh Oracle SQL | `ScopedSqlExecutor`, `SafeSqlExecutor` | Included |
| 15 | `Core/QueryPlanner.cs` | `Planning/QueryPlanner.cs` | Phân tích intent, thực thể và quyền hạn để lập ra ExecutionPlan | `AiExecutionService` | Included |
| 16 | `Core/QueryPreprocessor.cs` | `Understanding/QueryPreprocessor.cs` | Tiền xử lý, chuẩn hóa Unicode tiếng Việt, dấu câu, khoảng trắng | `QueryUnderstandingService` | Included |
| 17 | `Core/QueryUnderstandingModels.cs` | `Understanding/QueryUnderstandingModels.cs` | Tập hợp các DTO intent, entity, clarification token, date token | `QueryUnderstandingService`, `EntityResolver` | Included |
| 18 | `Core/QueryUnderstandingService.cs` | `Understanding/QueryUnderstandingService.cs` | Pipeline hiểu câu hỏi: tiền xử lý, phân giải intent và thực thể | `AiExecutionService` | Included |
| 19 | `Core/RagContextRetriever.cs` | `Retrieval/Hybrid/RagContextRetriever.cs` | Thu thập bằng chứng ngữ cảnh có kiểm soát nguồn gốc (provenance) | `HybridRagService`, `AiExecutionService` | Included |
| 20 | `Core/RagSynthesizer.cs` | `Responses/RagSynthesizer.cs` | Dùng LLM tổng hợp văn phong trả lời từ bằng chứng đã lọc quyền | `AiExecutionService` | Included |
| 21 | `Core/SafeSqlExecutor.cs` | `Retrieval/Sql/SafeSqlExecutor.cs` | Adapter kiểm định AST và ủy thác sang ScopedSqlExecutor | `HybridRagService`, `AiServiceLocator` | Included |
| 22 | `Core/ScopedSqlExecutor.cs` | `Retrieval/Sql/ScopedSqlExecutor.cs` | Điểm truy cập Oracle DB duy nhất: áp dụng scope ALL/DEPT/SELF | `AiExecutionService`, `SafeSqlExecutor` | Included |
| 23 | `Core/SqlGeneratorService.cs` | `Retrieval/Sql/SqlGeneratorService.cs` | Chọn lựa mẫu SQL template từ catalog được duyệt; không sinh tự do | `AiExecutionService` | Included |
| 24 | `Core/AiSchemaService.cs` | `Retrieval/Sql/AiSchemaService.cs` | Catalog metadata schema phục vụ tra cứu AI | `SqlGeneratorService` | *Excluded (Baseline)* |
| 25 | `Interfaces/ILlmService.cs` | `Interfaces/ILlmService.cs` | Hợp đồng giao tiếp mô hình LLM (Chat, Embedding) | `OllamaService` | Included |
| 26 | `Interfaces/IPromptManager.cs` | `Interfaces/IPromptManager.cs` | Hợp đồng quản lý System Prompt theo phiên bản | `JsonPromptManager` | Included |
| 27 | `Interfaces/IRagContextRetriever.cs` | `Interfaces/IRagContextRetriever.cs` | Hợp đồng thu thập ngữ cảnh cho RAG có chứng nhận phạm vi | `RagContextRetriever` | Included |
| 28 | `Interfaces/IRagSynthesizer.cs` | `Interfaces/IRagSynthesizer.cs` | Hợp đồng tổng hợp câu trả lời từ bằng chứng ngữ cảnh | `RagSynthesizer` | Included |
| 29 | `Interfaces/ISafeSqlExecutor.cs` | `Interfaces/ISafeSqlExecutor.cs` | Hợp đồng thực thi SQL an toàn có kiểm định cú pháp | `SafeSqlExecutor` | Included |
| 30 | `Interfaces/IScopedSqlExecutor.cs` | `Interfaces/IScopedSqlExecutor.cs` | Hợp đồng thực thi SQL có kiểm soát phạm vi người dùng | `ScopedSqlExecutor` | Included |
| 31 | `Interfaces/ISqlGenerator.cs` | `Interfaces/ISqlGenerator.cs` | Hợp đồng tạo truy vấn SQL có cấu trúc từ template catalog | `SqlGeneratorService` | Included |
| 32 | `Interfaces/IVectorService.cs` | `Interfaces/IVectorService.cs` | Hợp đồng thao tác với Qdrant (Search scoped, Upsert, Count) | `QdrantService` | Included |
| 33 | `LLM/OllamaService.cs` | `Providers/Ollama/OllamaService.cs` | HTTP Client kết nối Ollama API (`/api/chat`, `/api/embeddings`) | `AiExecutionService`, `RagSynthesizer` | Included |
| 34 | `Memory/AiCacheCoordinator.cs` | `Memory/Cache/AiCacheCoordinator.cs` | Bộ nhớ đệm thread-safe: bounded cardinality, TTL, single-flight | `AiCacheService`, `AiExecutionService` | Included |
| 35 | `Memory/AiCacheService.cs` | `Memory/Cache/AiCacheService.cs` | Facade thao tác cache đơn giản theo khóa ủy quyền | `AiExecutionService` | Included |
| 36 | `Memory/AiChatHistory.cs` | `Memory/Conversations/AiChatHistory.cs` | Model lưu trữ tin nhắn hội thoại theo từng người dùng | `ConversationStateManager` | Included |
| 37 | `Memory/ConversationStateManager.cs` | `Memory/Conversations/ConversationStateManager.cs` | Quản lý trạng thái phiên chat, trạng thái chờ làm rõ (TTL 10 phút) | `AiExecutionService` | Included |
| 38 | `Security/AiAuthorizationContext.cs` | `Security/Authorization/AiAuthorizationContext.cs` | Snapshot quyền người dùng: UserId, MaNV, Capabilities, Scopes | `AiExecutionService`, `AiScopeEvaluator` | Included |
| 39 | `Security/AiAuthorizationService.cs` | `Security/Authorization/AiAuthorizationService.cs` | Đánh giá quyền sử dụng AI và quyết định quyền thực thi chức năng | `AiExecutionService` | Included |
| 40 | `Security/AiCapabilityCatalog.cs` | `Security/Authorization/AiCapabilityCatalog.cs` | Danh mục 16 Capability AI và quy tắc kiểm tra quyền nghiệp vụ nền | `AiAuthorizationService`, `QueryPlanner` | Included |
| 41 | `Security/AiPolicyProvider.cs` | `Security/Authorization/AiPolicyProvider.cs` | Nạp chính sách phân quyền AI và thông tin người dùng từ Oracle | `AiExecutionService`, `AiScopePolicyResolver` | Included |
| 42 | `Security/AiScopeEvaluator.cs` | `Security/Scopes/AiScopeEvaluator.cs` | Kiểm tra bản ghi nhân sự có nằm trong phạm vi hiệu lực hay không | `ScopedSqlExecutor`, `EntityResolver` | Included |
| 43 | `Security/AiScopeGrantManagementService.cs` | `Security/Scopes/AiScopeGrantManagementService.cs` | Dịch vụ nghiệp vụ quản trị phân quyền phạm vi AI (CRUD Grant) | `AiScopeAdminController`, `FrmAiScopeGrantDetail` | Included |
| 44 | `Security/AiScopeGrantModels.cs` | `Security/Scopes/AiScopeGrantModels.cs` | DTO truyền nhận cho nghiệp vụ quản lý scope grant | `AiScopeGrantManagementService` | Included |
| 45 | `Security/AiScopePolicyResolver.cs` | `Security/Scopes/AiScopePolicyResolver.cs` | Hợp nhất grant trực tiếp, grant nhóm, xử lý DENY ưu tiên cao nhất | `AiPolicyProvider`, `AiExecutionService` | Included |
| 46 | `Security/IAiScopeGrantRepository.cs` | `Security/Repositories/IAiScopeGrantRepository.cs` | Contract lưu trữ phân quyền AI Scope trên CSDL | `OracleAiScopeGrantRepository`, `InMemoryAiScopeGrantRepository` | Included |
| 47 | `Security/InMemoryAiScopeGrantRepository.cs` | `Security/Repositories/InMemoryAiScopeGrantRepository.cs` | Triển khai mock in-memory của repository dùng cho Unit Test | `HRMS.Tests` | Included |
| 48 | `Security/OracleAiScopeGrantRepository.cs` | `Security/Repositories/OracleAiScopeGrantRepository.cs` | Triển khai lưu trữ phân quyền AI Scope trên `TB_AI_SCOPE_GRANT` | `AiScopeGrantManagementService` | Included |
| 49 | `Security/AiHmacProofService.cs` | `Security/Proofs/AiHmacProofService.cs` | Tạo và kiểm thực HMAC Proof bảo đảm tính toàn vẹn phiên trao đổi | `AiExecutionService` | Included |
| 50 | `Vector/AiDataSyncHub.cs` | `Indexing/AiDataSyncHub.cs` | Tiếp nhận sự kiện nghiệp vụ và kích hoạt hàng đợi đồng bộ | Nghiệp vụ HR (Thêm/Sửa/Xóa NV) | Included |
| 51 | `Vector/QdrantOutboxManager.cs` | `Indexing/QdrantOutboxManager.cs` | Quản lý Outbox bền vững đồng bộ vector sang Qdrant | `HRMS.VectorDataSync`, Background Worker | Included |
| 52 | `Vector/QdrantService.cs` | `Retrieval/Vector/QdrantService.cs` | Client HTTP REST kết nối Qdrant: Scoped Search, Add, Count | `AiExecutionService`, `HRMS.VectorDataSync` | Included |
| 53 | `Vector/VectorSearchModels.cs` | `Retrieval/Vector/VectorSearchModels.cs` | DTO VectorSecurityFilter, VectorHit, Citation provenance | `QdrantService`, `RagContextRetriever` | Included |

---

## 3. Hướng Dẫn Chi Tiết Từng Nhóm Chức Năng (20 Nhóm Trách Nhiệm)

### 3.1. Nhóm Bootstrap (`Bootstrap/`)
- **Trách nhiệm & Ranh giới:** Điểm khởi tạo và đăng ký phụ thuộc (DI / Service Locator) tập trung cho toàn phân hệ AI. Chỉ chịu trách nhiệm kết nối các interface và instance; không trực tiếp thực thi truy vấn hoặc kích hoạt tiến trình nền khi resolve service.
- **File & Class:**
  - `AiServiceLocator.cs` (`class AiServiceLocator`): Triển khai mẫu Singleton thread-safe qua `Lazy<T>`, sở hữu phương thức `Initialize(...)`, `GetService<T>()`, `GetRequiredService<T>()`.
- **Caller chính:** `AiChatController` (Web API), `ChatboxManager` (Desktop), `AiBootstrap` (WinForms startup).
- **Thời điểm chạy & Vòng đời:** Chạy một lần duy nhất lúc ứng dụng khởi động (Application Startup). Các service đăng ký có vòng đời Singleton trong phạm vi tiến trình.
- **Đầu vào / Đầu ra:** Đầu vào là cấu hình chuỗi kết nối và factory khởi tạo; Đầu ra là các instance đã cấu hình sẵn (`AiExecutionService`, `AiAuthorizationService`, `IVectorService`, `ILlmService`).
- **Xử lý lỗi:** Nếu thiếu dependency bắt buộc, quăng lỗi `InvalidOperationException` kèm thông báo rõ ràng về service chưa được đăng ký.

### 3.2. Nhóm Configuration (`Configuration/`)
- **Trách nhiệm & Ranh giới:** Quản lý vòng đời và tính toàn vẹn cấu hình AI (Ollama Host/Port, Chat Model, Embedding Model, Qdrant URL, Timeout). Kiểm tra hợp lệ mạng, chống SSRF (chặn dải IP riêng tư nguy hiểm hoặc hostname không thuộc allowlist).
- **File & Class:**
  - `AiConfigurationCoordinator.cs` (`class AiConfigurationCoordinator`): Quản lý snapshot cấu hình có gắn version CAS (Compare-And-Swap), cung cấp hàm `GetActiveConfiguration()`, `UpdateConfigurationAsync()`, `ProbeOllamaAsync()`.
- **Caller chính:** `AiChatController`, `FrmOllamaConfig` (Desktop Admin), `OllamaService`.
- **Thời điểm chạy & Vòng đời:** Singleton. Nạp cấu hình từ DB lúc khởi động, cập nhật theo sự kiện quản trị.
- **Xử lý lỗi:** Khi phát hiện xung đột phiên bản (version mismatch), trả mã lỗi `409 Conflict` yêu cầu người dùng làm mới dữ liệu trước khi lưu đè.

### 3.3. Nhóm Chat (`Chat/`)
- **Trách nhiệm & Ranh giới:** Tầng ứng dụng tiếp nhận yêu cầu trò chuyện, điều phối toàn bộ chu trình xử lý (Orchestration) và chuyển đổi kết quả cho UI.
- **File & Class:**
  - `AiExecutionService.cs` (`class AiExecutionService`): Bộ điều phối trung tâm (Central Orchestrator) duy nhất. Thực hiện: Nạp Auth Context -> Kiểm tra quyền AI nền tảng -> Hiểu câu hỏi (NLP) -> Lập ExecutionPlan -> Authorize theo nhánh -> Thực thi (SQL, Vector, Hybrid, Fast) -> Cache -> Trả kết quả.
  - `ChatboxManager.cs` (`class ChatboxManager`): Adapter tầng UI cho WinForms và Web frontend; quản lý mã phiên (`conversationId`), hủy yêu cầu qua `CancellationTokenSource`, định dạng tin nhắn lịch sử.
- **Caller chính:** `AiChatController.Chat` (HTTP POST `/api/ai/chat`), `FrmAI_Chat.cs` (Desktop).
- **Xử lý lỗi:** Bắt lỗi dịch vụ (Timeout, 500, Hủy thao tác), chuyển đổi thành `AiChatResponseDto` thân thiện kèm Correlation ID để tra cứu nhật ký.

### 3.4. Nhóm Understanding (`Understanding/`)
- **Trách nhiệm & Ranh giới:** Tiếp nhận câu hỏi dạng văn bản thô, làm sạch, phân giải ngữ nghĩa, trích xuất thực thể nhân sự/kỳ công và phát hiện các trường hợp mơ hồ cần hỏi lại.
- **File & Class:**
  - `QueryPreprocessor.cs`: Chuẩn hóa Unicode tiếng Việt (NFC), loại bỏ ký tự lạ, chuẩn hóa khoảng trắng.
  - `AiRouterService.cs`: Đối chiếu từ khóa với allowlist intent (Tra cứu lương, chấm công, hợp đồng, chính sách quy chế).
  - `EntityResolver.cs`: Phân giải tên nhân viên, mã phòng ban, mã kỳ công dựa trên dữ liệu nhân sự, có áp đặt phạm vi dữ liệu (`EffectiveScope`) của người hỏi.
  - `ClarificationPolicy.cs`: Đánh giá độ mơ hồ (ví dụ: tìm thấy 2 nhân viên trùng họ tên trong cùng phạm vi); sinh danh sách lựa chọn và `ClarificationToken` với thời gian sống (TTL) **10 phút**.
  - `QueryUnderstandingModels.cs`: Chứa các DTO `QueryUnderstandingResult`, `ResolvedEntity`, `ClarificationTokenInfo`.
  - `QueryUnderstandingService.cs`: Pipeline tích hợp toàn bộ các bước tiền xử lý, phân giải intent và thực thể.
- **Caller chính:** `AiExecutionService.ProcessChatAsync`.

### 3.5. Nhóm Planning (`Planning/`)
- **Trách nhiệm & Ranh giới:** Dựa trên kết quả hiểu câu hỏi và snapshot quyền (`AiAuthorizationContext`), xây dựng kế hoạch thực thi tối ưu và an toàn.
- **File & Class:**
  - `ExecutionPlan.cs` (`class QueryExecutionPlan`, `enum ExecutionStrategy`): Chứa các chiến lược thực thi trong C#: `SqlTemplate`, `DeterministicDirect`, `VectorSearch`, `Hybrid`, `NeedsClarification`, `Forbidden`, `Unsupported`, `NoAction`.
  - `QueryPlanner.cs` (`class QueryPlanner`): Quyết định chiến lược. Áp dụng quy tắc bảo mật **Fail-Closed**: nếu câu hỏi yêu cầu chính sách (`POLICY`), phải kiểm tra capability `POLICY_LOOKUP` qua `AiAuthorizationService.ValidateCapability`. Nếu thiếu quyền, lập tức gán chiến lược `Forbidden`.
- **Caller chính:** `AiExecutionService`.

### 3.6. Nhóm Retrieval/Sql (`Retrieval/Sql/`)
- **Trách nhiệm & Ranh giới:** Tương tác an toàn với CSDL Oracle 19c. Nghiêm cấm chạy SQL tự do; chỉ thực thi các truy vấn mẫu (SQL Templates) đã được phê duyệt và bind parameter chặt chẽ.
- **File & Class:**
  - `ScopedSqlExecutor.cs` (`class ScopedSqlExecutor`, `interface IScopedSqlExecutor`): Cổng thực thi Oracle SQL duy nhất trong AI. Bắt buộc nhận `AiAuthorizationContext` và tự động áp dụng mệnh đề WHERE lọc theo phạm vi (`IDPB`, `MANV`, `MACTY`).
  - `SafeSqlExecutor.cs` (`class SafeSqlExecutor`, `interface ISafeSqlExecutor`): Adapter kiểm định AST trước khi chuyển giao lệnh cho `ScopedSqlExecutor`.
  - `OracleSqlAstValidator.cs` (`class OracleSqlAstValidator`): Kiểm định cú pháp SQL, chỉ cho phép các lệnh `SELECT` thuần túy, cấm triệt để DDL/DML (`DROP`, `ALTER`, `INSERT`, `UPDATE`, `DELETE`, `EXEC`).
  - `SqlGeneratorService.cs` (`class SqlGeneratorService`, `interface ISqlGenerator`): Chọn template SQL tương ứng từ catalog dựa vào Intent và Entity đã phân giải.
  - `AiSchemaService.cs` (`class AiSchemaService`): Catalog định nghĩa các bảng và trường được phép truy vấn trong AI (hiện giữ trạng thái loại trừ khỏi Compile theo baseline).
- **Caller chính:** `AiExecutionService`, `RagContextRetriever`.

### 3.7. Nhóm Retrieval/Vector (`Retrieval/Vector/`)
- **Trách nhiệm & Ranh giới:** Tra cứu ngữ nghĩa tài liệu văn bản, quy chế, nội quy công ty từ Qdrant Vector Engine. Tuyệt đối không cho phép tìm kiếm vượt quyền hay bỏ qua filter bảo mật.
- **File & Class:**
  - `QdrantService.cs` (`class QdrantService`, `interface IVectorService`): Client HTTP kết nối REST API Qdrant (`/collections/{name}/points/search`). Sử dụng collection hoạt động `hrms_vectors_v2` (1024 chiều, Cosine). Hàm `SearchScopedAsync` xây dựng `VectorSecurityFilter` chặt chẽ theo CompanyId, DepartmentIds, CallerEmployeeId (với phạm vi `SELF`), kiểm tra mã từ chối (`DenyCodes`) và tái đánh giá quyền trên từng payload trước khi gán `IsAuthorized = true`. Chỉ kiểm tra sự tồn tại của collection qua `GET`, không tự ý tạo lại collection khi đọc.
  - `VectorSearchModels.cs`: DTO `VectorSecurityFilter`, `VectorBusinessFilter`, `VectorHit`, `VectorSearchResult`.
- **Cấu hình Chỉ mục & Dữ liệu Thực tế (`hrms_vectors_v2`):**
  - **Vector Dimension & Distance:** 1024 chiều (`bge-m3:latest`), khoảng cách Cosine.
  - **Payload Indexes đã thiết lập:** `tag` (keyword), `departmentId` (integer), `companyId` (integer), `employeeId` (integer), `domain` (keyword), `documentType` (keyword), `visibility_profile` (keyword).
  - **Dữ liệu hiện hữu:** 197 vector nhân viên (`tag = "EMPLOYEE"`), tất cả đều hợp lệ (hữu hạn, khác 0, không trùng lặp ID).
  - **Quy tắc Nhân viên Chưa phân bổ (`departmentId = 0`):** Có 2 nhân viên (MANV 201 và 2201) trong Oracle `HR.TB_NHANVIEN` có `IDPB IS NULL` (chưa phân bổ phòng ban). Khi đồng bộ sang Qdrant mang giá trị `departmentId = 0`. Phạm vi `DEPARTMENT` trong `QdrantService` bắt buộc kiểm tra `departmentId > 0`, tuyệt đối không xem 0 là wildcard và không cho phép nhân viên chưa phân bổ nhìn thấy nhau.
  - **Trạng thái Văn bản Quy chế (`REGULATION`):** Hiện tại collection chưa có vector văn bản quy chế thật (`tag = "REGULATION"`: 0 point). Khi người dùng hỏi quy chế hoặc chạy Hybrid, hệ thống báo trung thực rằng nguồn tài liệu chưa sẵn sàng hoặc chưa tìm thấy, tuyệt đối không bịa đặt nội dung quy định.
- **Caller chính:** `AiExecutionService`, `RagContextRetriever`, `HRMS.VectorDataSync`.

### 3.8. Nhóm Retrieval/Hybrid (`Retrieval/Hybrid/`)
- **Trách nhiệm & Ranh giới:** Điều phối thu thập dữ liệu đồng thời từ cả hai nguồn: số liệu có cấu trúc từ Oracle DB và tài liệu ngữ nghĩa từ Qdrant Vector.
- **File & Class:**
  - `RagContextRetriever.cs` (`class RagContextRetriever`, `interface IRagContextRetriever`): Thu thập ngữ cảnh và bằng chứng (Evidence) từ các nguồn dữ liệu, chuẩn hóa thông tin trích dẫn (Document ID, Title, Section, UpdatedAt, EffectiveScope).
  - `HybridRagService.cs` (`class HybridRagService`): Phối hợp bằng chứng từ Oracle và Qdrant, bảo đảm nếu một nguồn bị từ chối do thiếu quyền, không để lộ thông tin của nguồn đó sang câu trả lời tổng hợp.
- **Caller chính:** `AiExecutionService`.

### 3.9. Nhóm Responses (`Responses/`)
- **Trách nhiệm & Ranh giới:** Định dạng và tổng hợp câu trả lời cuối cùng gửi tới người dùng.
- **File & Class:**
  - `DeterministicResponseRenderer.cs` (`class DeterministicResponseRenderer`): Dựng câu trả lời chính xác, đáng tin cậy 100% từ kết quả Oracle SQL (bảng số liệu chấm công, lương, số lượng nhân sự), không đưa qua LLM để loại trừ hoàn toàn nguy cơ bịa đặt (hallucination).
  - `RagSynthesizer.cs` (`class RagSynthesizer`, `interface IRagSynthesizer`): Gọi mô hình LLM (`qwen2.5`) để diễn đạt câu trả lời tự nhiên từ các đoạn trích dẫn quy chế đã được kiểm duyệt quyền.
  - `FastResponseService.cs` (`class FastResponseService`): Trả về câu chào hỏi hoặc giới thiệu năng lực hệ thống một cách tức thì, không tiêu tốn tài nguyên DB hoặc GPU.
- **Caller chính:** `AiExecutionService`.

### 3.10. Nhóm Providers/Ollama (`Providers/Ollama/`)
- **Trách nhiệm & Ranh giới:** Tầng giao tiếp vật lý qua HTTP Client với Ollama Engine đang phục vụ cục bộ.
- **File & Class:**
  - `OllamaService.cs` (`class OllamaService`, `interface ILlmService`): Thực hiện gọi endpoint `/api/chat` (mô hình `qwen2.5:latest`) để sinh văn bản và `/api/embeddings` (mô hình `bge-m3:latest`, 1024 chiều) để vector hóa câu hỏi. Hỗ trợ đầy đủ timeout và `CancellationToken`.
- **Caller chính:** `RagSynthesizer`, `QdrantService` (khi cần embed query), `HRMS.VectorDataSync`.

### 3.11. Nhóm Prompts (`Prompts/`)
- **Trách nhiệm & Ranh giới:** Quản lý và cung cấp System Prompt cho mô hình ngôn ngữ lớn từ file JSON tài nguyên đóng gói của ứng dụng.
- **File & Class:**
  - `JsonPromptManager.cs` (`class JsonPromptManager`, `interface IPromptManager`): Đọc và lưu trữ bộ prompt theo phiên bản từ file JSON cấu hình nội bộ; bảo đảm prompt mang tính trung lập, phòng chống prompt injection và không chứa đường dẫn máy cá nhân.
- **Caller chính:** `RagSynthesizer`, `AiExecutionService`.

### 3.12. Nhóm Memory/Cache (`Memory/Cache/`)
- **Trách nhiệm & Ranh giới:** Bộ nhớ đệm tốc độ cao cho kết quả truy vấn và bằng chứng AI, bảo vệ hệ thống trước tình trạng quá tải lặp lại câu hỏi.
- **File & Class:**
  - `AiCacheCoordinator.cs` (`class AiCacheCoordinator`): Bộ nhớ đệm thread-safe trong RAM (in-memory) có giới hạn dung lượng (bounded cardinality: tối đa 500 mục), cơ chế trượt thời gian sống (TTL), chống tháo chạy đồng thời (single-flight execution) và cơ chế hủy cache tức thì (invalidation) dựa trên phiên bản chính sách (`TB_AI_REVISION`). Khóa cache bắt buộc phải gắn liền `AuthorizationFingerprint` của người hỏi để tránh lộ chéo dữ liệu giữa các người dùng khác scope.
  - `AiCacheService.cs` (`class AiCacheService`): Facade cung cấp các phương thức thao tác cache đơn giản cho các service tầng trên.
- **Caller chính:** `AiExecutionService`.

### 3.13. Nhóm Memory/Conversations (`Memory/Conversations/`)
- **Trách nhiệm & Ranh giới:** Quản lý ngữ cảnh và trạng thái phiên hội thoại giữa người dùng và trợ lý AI.
- **File & Class:**
  - `ConversationStateManager.cs` (`class ConversationStateManager`): Quản lý phiên hội thoại theo cặp khóa `(UserId, ConversationId)`. Lưu trữ trạng thái câu hỏi đang chờ người dùng làm rõ (`PendingClarification`) kèm thời hạn hiệu lực **10 phút**. Tự động dọn dẹp các phiên đã hết hạn.
  - `AiChatHistory.cs` (`class AiChatHistory`): Data model lưu trữ danh sách các lượt tin nhắn trong phiên, giới hạn tối đa 20 lượt trao đổi gần nhất để kiểm soát dung lượng bộ nhớ.
- **Caller chính:** `AiExecutionService`.

### 3.14. Nhóm Security/Authorization (`Security/Authorization/`)
- **Trách nhiệm & Ranh giới:** Lớp bảo vệ an ninh trung tâm; xây dựng snapshot quyền và kiểm định quyền hạn thực thi chức năng AI.
- **File & Class:**
  - `AiAuthorizationContext.cs`: Snapshot thông tin quyền của người dùng tại thời điểm gửi câu hỏi, bao gồm: `UserId`, `MaNV`, `MaCty`, `MaPB`, `IsSystemAdmin`, danh sách `GrantedCapabilities`, và từ điển `EffectiveScopes` cho từng capability.
  - `AiCapabilityCatalog.cs`: Danh mục định nghĩa **16 Capability chuẩn** của hệ thống (ví dụ: `EMPLOYEE_COUNT`, `ATTENDANCE_SUMMARY`, `OVERTIME_VIEW`, `POLICY_LOOKUP`,...) và ánh xạ quyền nghiệp vụ cơ sở tương ứng (`F_DM_NHANVIEN`, `F_CC_BANGCONG`, `F_CC_TANGCA`,...).
  - `AiAuthorizationService.cs`: Thực hiện kiểm tra tính hợp lệ của Capability: kiểm tra tài khoản có bị khóa không, có quyền `F_SYSTEM_AI` không, quyền nghiệp vụ cơ sở có được bật không và Capability có nằm trong danh sách cấp phép hay không.
  - `AiPolicyProvider.cs`: Đọc thông tin phân quyền AI và thông tin tổ chức của người dùng từ CSDL Oracle thông qua package `PKG_AI_READER`.
- **Caller chính:** `AiExecutionService`, `QueryPlanner`.

### 3.15. Nhóm Security/Scopes (`Security/Scopes/`)
- **Trách nhiệm & Ranh giới:** Tính toán và thẩm định phạm vi dữ liệu hiệu lực (Data Scope: `SELF`, `DEPARTMENT`, `COMPANY`, `ALL`) cho từng người dùng và từng bản ghi cụ thể.
- **File & Class:**
  - `AiScopePolicyResolver.cs`: Thuật toán hợp nhất các quyền cấp trực tiếp cho User và các quyền kế thừa từ Nhóm (Group); áp dụng nguyên tắc **DENY có độ ưu tiên cao nhất**.
  - `AiScopeEvaluator.cs`: Đánh giá một đối tượng nhân viên cụ thể có nằm trong phạm vi được phép của người hỏi hay không dựa trên mã công ty, mã phòng ban và mã nhân viên.
  - `AiScopeGrantManagementService.cs`: Dịch vụ nghiệp vụ quản trị phân quyền phạm vi AI (Thêm, Sửa, Thu hồi Grant), ghi nhận nhật ký kiểm toán và kích hoạt tăng số hiệu phiên bản (`TB_AI_REVISION`).
  - `AiScopeGrantModels.cs`: DTO phục vụ màn hình quản trị Scope Grant.
- **Caller chính:** `AiPolicyProvider`, `ScopedSqlExecutor`, `AiScopeAdminController`.

### 3.16. Nhóm Security/Repositories (`Security/Repositories/`)
- **Trách nhiệm & Ranh giới:** Tầng lưu trữ bền vững (Persistence) cho dữ liệu phân quyền Scope Grant.
- **File & Class:**
  - `IAiScopeGrantRepository.cs`: Contract định nghĩa các thao tác CRUD với phân quyền Scope Grant.
  - `OracleAiScopeGrantRepository.cs`: Triển khai đọc và ghi dữ liệu trực tiếp vào bảng `AI_OWNER.TB_AI_SCOPE_GRANT` trên Oracle 19c bằng tham số bind an toàn.
  - `InMemoryAiScopeGrantRepository.cs`: Triển khai lưu trữ trên bộ nhớ RAM dùng riêng cho các bài kiểm thử tự động (Unit Test), hoàn toàn cách ly với database production.
- **Caller chính:** `AiScopeGrantManagementService`, `HRMS.Tests`.

### 3.17. Nhóm Security/Proofs (`Security/Proofs/`)
- **Trách nhiệm & Ranh giới:** Bảo vệ tính toàn vẹn và chống giả mạo thông tin trao đổi giữa Client và Server.
- **File & Class:**
  - `AiHmacProofService.cs` (`class AiHmacProofService`): Sinh và thẩm định chữ ký HMAC-SHA256 trên các payload nhạy cảm hoặc token làm rõ lựa chọn (Option Token), ngăn chặn người dùng chỉnh sửa trái phép tham số truy vấn trên đường truyền.
- **Caller chính:** `AiExecutionService`, `ClarificationPolicy`.

### 3.18. Nhóm Indexing (`Indexing/`)
- **Trách nhiệm & Ranh giới:** Cơ chế đồng bộ dữ liệu hồ sơ nhân sự và quy chế sang Qdrant Vector Database theo mô hình Transactional Outbox bền vững.
- **File & Class:**
  - `AiDataSyncHub.cs`: Hub tiếp nhận các sự kiện thay đổi dữ liệu nhân sự từ nghiệp vụ lõi (Thêm mới nhân viên, sửa hồ sơ, chuyển phòng ban, thôi việc) và đẩy bản ghi vào hàng đợi Outbox.
  - `QdrantOutboxManager.cs`: Tiến trình xử lý hàng đợi Outbox: nạp sự kiện -> đọc dữ liệu Oracle mới nhất -> tạo embedding vector -> upsert sang Qdrant -> chỉ xác nhận (ACK) khi Qdrant trả về HTTP 200/201. Nếu lỗi, ghi nhận mã lỗi, giữ bản ghi trong hàng đợi và kích hoạt cơ chế thử lại (exponential backoff).
- **Caller chính:** Nghiệp vụ Nhân sự, `HRMS.VectorDataSync`.

### 3.19. Nhóm Interfaces (`Interfaces/`)
- **Trách nhiệm & Ranh giới:** Nơi lưu trữ tập trung các hợp đồng (Interfaces) dùng chung giữa các tầng nghiệp vụ, bảo đảm tính độc lập và khả năng đảo ngược phụ thuộc (Dependency Inversion).
- **File & Interface:**
  - `ILlmService.cs`: Hợp đồng cho nhà cung cấp mô hình ngôn ngữ.
  - `IPromptManager.cs`: Hợp đồng quản lý mẫu prompt.
  - `IRagContextRetriever.cs`: Hợp đồng thu thập ngữ cảnh RAG có kiểm soát phạm vi.
  - `IRagSynthesizer.cs`: Hợp đồng tổng hợp câu trả lời từ bằng chứng.
  - `ISafeSqlExecutor.cs`: Hợp đồng thực thi SQL an toàn có kiểm định AST.
  - `IScopedSqlExecutor.cs`: Hợp đồng thực thi SQL theo phạm vi ủy quyền.
  - `ISqlGenerator.cs`: Hợp đồng sinh truy vấn SQL từ template.
  - `IVectorService.cs`: Hợp đồng thao tác với kho lưu trữ Vector.

### 3.20. Nhóm Runtime/Time (`Runtime/Time/`)
- **Trách nhiệm & Ranh giới:** Trừu tượng hóa nguồn thời gian hệ thống, phục vụ việc kiểm thử chính xác các logic liên quan đến TTL, hạn sử dụng và trượt cửa sổ thời gian.
- **File & Class:**
  - `IClockProvider.cs` (`interface IClockProvider`): Interface cung cấp thuộc tính `UtcNow` và `Now`.
  - `SystemClockProvider`: Triển khai chuẩn sử dụng đồng hồ hệ điều hành cho môi trường Production.
  - `FakeClockProvider`: Triển khai đồng hồ có thể điều khiển thời gian tự do, phục vụ kiểm thử đơn vị độc lập mà không cần chờ đợi thời gian thực.
- **Caller chính:** `AiCacheCoordinator`, `ConversationStateManager`, `RateLimiterService`.

---

## 4. Các Luồng Thực Thi Nghiệp Vụ Xuyên Suốt (End-to-End Traces)

### Kịch Bản 1: "Có bao nhiêu nhân viên trong phòng của tôi?" (SQL Only)
- **Người hỏi:** Trưởng phòng IT (Mã NV: 105, Mã Phòng: 4, Mã Cty: 1).
- **Luồng xử lý chi tiết:**
  1. `AiChatController` nhận POST request, qua `RateLimiterService` (hạn mức 6 req/phút/user, cấp lease concurrency slot).
  2. `AiExecutionService` gọi `AiPolicyProvider.GetContextAsync`, nhận `AiAuthorizationContext` có Capability `EMPLOYEE_COUNT` với Scope `DEPARTMENT` (Phòng 4).
  3. `QueryUnderstandingService` phân tích câu hỏi -> Intent = `COUNT_EMPLOYEES`, Entity = Phòng của tôi (IDPB = 4).
  4. `QueryPlanner` đánh giá: Người dùng có quyền `EMPLOYEE_COUNT` phạm vi Phòng ban -> Lập `ExecutionPlan` với Strategy = `SqlTemplate`, Template = `TPL_COUNT_EMPLOYEES_BY_DEPT`.
  5. `ScopedSqlExecutor` thực thi câu lệnh SQL với bind parameter an toàn:
     ```sql
     SELECT COUNT(*) FROM HR.V_AI_SRC_EMPLOYEE WHERE IDPB = :p_idpb AND TRANGTHAI = 1
     ```
     (Giá trị `:p_idpb = 4` được trích xuất từ Context bảo mật).
  6. Kết quả trả về: 12 nhân viên.
  7. `DeterministicResponseRenderer` định dạng câu trả lời: *"Phòng IT của bạn hiện có 12 nhân viên đang làm việc."*
  8. Hệ thống lưu kết quả vào `AiCacheCoordinator` kèm khóa phân quyền. Hoàn toàn **không gọi Qdrant hay LLM**.

### Kịch Bản 2: "Tìm quy trình xin nghỉ phép và số ngày nghỉ phép năm" (Vector Only)
- **Người hỏi:** Nhân viên kinh doanh (Mã NV: 202).
- **Luồng xử lý chi tiết:**
  1. `AiExecutionService` xác thực danh tính -> Người dùng có quyền `F_SYSTEM_AI` và Capability `POLICY_LOOKUP` với Scope `COMPANY`.
  2. `QueryUnderstandingService` nhận diện Intent = `POLICY_LOOKUP`, Domain = `LEAVE_POLICY`.
  3. `QueryPlanner` kiểm tra: Có quyền `POLICY_LOOKUP` -> Lập `ExecutionPlan` với Strategy = `VectorSearch`, Collection = `hrms_vectors_v2`.
  4. `QdrantService.SearchScopedAsync` tạo `VectorSecurityFilter` (CompanyId = 1, doc_type = `REGULATION`), gọi Ollama embed câu hỏi bằng `bge-m3:latest` (1024 chiều), gửi Search Request sang Qdrant.
  5. Qdrant trả về 2 chunk văn bản thuộc tài liệu *"Quy chế nghỉ phép năm số 04/2026/QC-HR"*.
  6. `RagSynthesizer` gọi LLM `qwen2.5:latest` tổng hợp câu trả lời từ bằng chứng, trích dẫn rõ nguồn gốc: `[Văn bản: Quy chế nghỉ phép năm, Điều 5: Số ngày phép năm, Cập nhật: 01/2026]`.

### Kịch Bản 3: "Số giờ tăng ca tháng này của tôi và quy định tăng ca đang áp dụng?" (Hybrid Strategy)
- **Người hỏi:** Kỹ sư phần mềm (Mã NV: 305).
- **Luồng xử lý chi tiết:**
  1. `QueryUnderstandingService` nhận diện câu hỏi kép: yêu cầu cả số liệu tăng ca cá nhân (`OVERTIME_VIEW`) và văn bản quy định (`POLICY_LOOKUP`).
  2. `QueryPlanner` thẩm định quyền:
     - Nhánh SQL: Có quyền `OVERTIME_VIEW` phạm vi `SELF` (Mã NV: 305).
     - Nhánh Vector: Có quyền `POLICY_LOOKUP` phạm vi `COMPANY`.
     -> Quyết định Strategy = `ExecutionStrategy.Hybrid`.
  3. `AiExecutionService` thực thi đồng thời hai nhánh:
     - *Nhánh SQL (`ScopedSqlExecutor`):* Truy vấn bảng tăng ca tháng hiện tại của Mã NV 305 -> Kết quả: 14.5 giờ (gồm 10.5 giờ ngày thường, 4.0 giờ ngày nghỉ).
     - *Nhánh Vector (`QdrantService`):* Tìm kiếm quy định tính lương làm thêm giờ từ Qdrant -> Kết quả: Quy định hệ số 150% ngày thường, 200% ngày nghỉ tuần.
  4. `HybridRagService` kết hợp hai nguồn bằng chứng, chuyển sang `RagSynthesizer` tổng hợp câu trả lời đầy đủ và kèm compound citation: `[Nguồn: Dữ liệu Oracle DB Chấm công & Quy định làm thêm giờ số 08/2026/QĐ-HR]`.

### Kịch Bản 4: Xử Lý Nhân Viên Trùng Họ Tên (Mơ Hồ Thực Thể - Clarification Flow)
- **Tình huống:** Quản lý hỏi: *"Xem ngày công tháng này của nhân viên Nguyễn Văn A"*.
- **Luồng xử lý:**
  1. `EntityResolver` tìm kiếm nhân viên có tên "Nguyễn Văn A" trong phạm vi quản lý -> Phát hiện 2 nhân viên:
     - Ứng viên 1: Nguyễn Văn A (Mã NV: 101, Phòng Kế toán).
     - Ứng viên 2: Nguyễn Văn A (Mã NV: 156, Phòng Kinh doanh).
  2. `ClarificationPolicy` phát hiện mơ hồ, tạo `ClarificationToken` có thời hạn hiệu lực **10 phút**, sinh Option Token có mã hóa chữ ký HMAC cho từng ứng viên.
  3. `ConversationStateManager` lưu trạng thái `PendingClarification` vào phiên của User.
  4. Hệ thống phản hồi yêu cầu người dùng làm rõ:
     *"Tôi tìm thấy 2 nhân viên có tên 'Nguyễn Văn A' trong phạm vi của bạn. Vui lòng chọn nhân viên cần tra cứu:*
     *1. Nguyễn Văn A (Mã NV: 101 - Phòng Kế toán)*
     *2. Nguyễn Văn A (Mã NV: 156 - Phòng Kinh doanh)"*
  5. Khi người dùng bấm chọn ứng viên 1, Client gửi kèm Option Token; Server thẩm định token hợp lệ và nạp đúng dữ liệu của Mã NV 101.

### Kịch Bản 5: Từ Chối Truy Cập Do Thiếu Quyền (Fail-Closed Enforcement)
- **Tình huống:** Nhân viên bình thường hỏi: *"Xem tổng quỹ lương của toàn công ty"*.
- **Luồng xử lý:**
  1. `QueryUnderstandingService` nhận diện Intent = `PAYROLL_SUMMARY`.
  2. `QueryPlanner` kiểm tra Capability: `PAYROLL_SUMMARY` yêu cầu quyền `F_CC_BANGLUONG`. Context của nhân viên không có quyền này.
  3. `QueryPlanner` lập tức gán `ExecutionStrategy.Forbidden` kèm mã lỗi `FORBIDDEN_CAPABILITY`.
  4. `AiExecutionService` dừng xử lý ngay lập tức, **tuyệt đối không gọi Oracle SQL, không gọi Qdrant và không gọi LLM**.
  5. Trả về thông báo từ chối truy cập chuẩn mực: *"Bạn không có quyền xem thông tin tổng quỹ lương của công ty (Mã quyền: PAYROLL_SUMMARY bị từ chối)."*

### Kịch Bản 6: Cập Nhật Phân Quyền Scope Grant & Invalidation Cache
- **Tình huống:** Quản trị viên cấp quyền tra cứu phạm vi Phòng ban cho một Trưởng nhóm qua Web / Desktop Admin.
- **Luồng xử lý:**
  1. `AiScopeAdminController` nhận lệnh POST cấp Grant.
  2. `AiScopeGrantManagementService` ghi nhận bản ghi vào `AI_OWNER.TB_AI_SCOPE_GRANT` qua `OracleAiScopeGrantRepository`.
  3. Tự động tăng số hiệu phiên bản trong bảng `AI_OWNER.TB_AI_REVISION` cho khóa `POLICY_GLOBAL` (ví dụ: Rev 6 -> Rev 7).
  4. Gửi tín hiệu thông báo invalidation tới `AiCacheCoordinator`.
  5. Toàn bộ cache cũ gắn với phiên bản trước lập tức hết hiệu lực; các câu hỏi tiếp theo của người dùng sẽ nạp ngay chính sách mới.

### Kịch Bản 7: Đồng Bộ Thay Đổi Dữ Liệu Nhân Sự Sang Qdrant (Durable Outbox)
- **Tình huống:** Nhân viên 105 được điều chuyển từ Phòng Kỹ thuật sang Phòng Quản lý Dự án.
- **Luồng xử lý:**
  1. Nghiệp vụ Quản lý nhân sự lưu thay đổi vào bảng `HR.TB_NHANVIEN`.
  2. `AiDataSyncHub` ghi nhận sự kiện chuyển phòng và ghi vào bảng hàng đợi Outbox.
  3. `QdrantOutboxManager` lấy bản ghi Outbox, đọc lại thông tin cập nhật từ Oracle view `HR.V_AI_SRC_EMPLOYEE`.
  4. Tạo embedding mới qua Ollama `bge-m3:latest` và gọi `QdrantService.AddAsync` với metadata `department_id` mới.
  5. Chỉ sau khi Qdrant phản hồi HTTP 200/201, `QdrantOutboxManager` mới đánh dấu bản ghi Outbox là đã hoàn thành (ACK).
  6. Nếu Qdrant tạm thời gián đoạn, bản ghi được giữ nguyên trong Outbox để thử lại với độ trễ lũy thừa (exponential backoff), không làm mất dữ liệu.

### Kịch Bản 8: Vận Hành Công Cụ CLI `HRMS.VectorDataSync`
- **Tình huống:** Quản trị viên thực hiện kiểm tra và nạp lại toàn bộ dữ liệu vector của nhân viên.
- **Lệnh thực thi:**
  ```powershell
  # 1. Kiểm tra kết nối và tính sẵn sàng của Qdrant & Ollama
  .\HRMS.VectorDataSync.exe preflight

  # 2. Xác minh đối soát điểm dữ liệu hiện có trong Qdrant
  .\HRMS.VectorDataSync.exe verify

  # 3. Nạp lại toàn bộ 197 nhân viên đang làm việc vào collection hrms_vectors_v2
  .\HRMS.VectorDataSync.exe rebuild

  # 4. Kiểm tra đối soát sau khi nạp để bảo đảm chính xác 197 points
  .\HRMS.VectorDataSync.exe verify
  ```

---

## 5. Sổ Tay Vận Hành & Khắc Phục Sự Cố (Troubleshooting Runbook)

| Mã Lỗi / Hiện Tượng | Nguyên Nhân Gốc | Vị Trí Cần Kiểm Tra | Cách Khắc Phục Nhanh |
| :--- | :--- | :--- | :--- |
| **HTTP 429 Too Many Requests** (`RATE_LIMITED`) | Người dùng gửi quá 6 câu chat/phút hoặc vượt giới hạn đồng thời (2 tác vụ toàn hệ thống). | `RateLimiterService.cs`, `RateLimitAttribute.cs` | Chờ hết thời gian đếm ngược (khoảng 20–60 giây). Hệ thống tự giải phóng hạn mức. |
| **HTTP 403 Forbidden** (`FORBIDDEN_CAPABILITY`) | Tài khoản thiếu quyền nghiệp vụ cơ sở hoặc không có quyền `F_SYSTEM_AI`. | `AiAuthorizationService.cs`, `TB_AI_CAPABILITY` | Quản trị viên cấp quyền chức năng tương ứng hoặc cấp Scope Grant trên giao diện Quản lý Phân quyền AI. |
| **ORA-00942: table or view does not exist** | Schema `HR` thiếu Synonym hoặc thiếu quyền `SELECT`/`EXECUTE` từ `AI_OWNER`. | Bảng `AI_OWNER.TB_AI_SCOPE_GRANT`, `TB_AI_REVISION` | Chạy migration [V1_31](file:///d:/QL_NS/QuanLyNhanSu/database/migrations/V1_31__grant_ai_owner_policy_tables_to_hr.sql) dưới quyền `SYS AS SYSDBA` để tạo Synonym và cấp quyền. |
| **Qdrant Connection Timeout / Refused** | Dịch vụ Qdrant tại `localhost:6333` chưa được khởi động. | Container Docker Qdrant hoặc dịch vụ Windows | Chạy lệnh `docker start qdrant` hoặc kiểm tra port 6333 đang lắng nghe. |
| **Ollama Timeout (quá 35 giây)** | Dịch vụ Ollama chưa chạy hoặc máy chủ quá tải khi nạp mô hình LLM. | `http://127.0.0.1:11434`, task `ollama serve` | Khởi động Ollama: `ollama serve`, kiểm tra mô hình qua `ollama list` (`bge-m3:latest`, `qwen2.5:latest`). |
| **HTTP 409 Conflict (CAS Version Mismatch)** | Có hai quản trị viên cùng sửa đổi cấu hình hoặc scope grant cùng thời điểm. | `AiConfigurationCoordinator.cs` | Nhấn "Tải lại dữ liệu mới nhất" trên giao diện, kiểm tra các thay đổi và tiến hành lưu lại. |
| **Clarification Token Expired** | Người dùng để câu hỏi quá 10 phút mới bấm chọn nhân viên làm rõ. | `ConversationStateManager.cs` | Nhập lại câu hỏi mới kèm thông tin cụ thể (ví dụ: nhập kèm mã nhân viên hoặc phòng ban). |

---

## 6. Hướng Dẫn Mở Rộng Tính Năng & Nguồn Dữ Liệu Mới

Khi có yêu cầu bổ sung một chức năng tra cứu thông tin nhân sự mới:
1. **Định nghĩa Capability:** Thêm mã chức năng vào `AiCapabilityCatalog.cs`, khai báo quyền cơ sở và bảng dữ liệu tương ứng.
2. **Khai báo Mẫu Truy Vấn (SQL Template):** Bổ sung mẫu SQL với tham số bind chặt chẽ trong `SqlGeneratorService.cs` và ánh xạ trong `QueryPlanner.cs`.
3. **Khai báo Phạm Vi Hiệu Lực:** Khai báo quy tắc lọc phạm vi trong `ScopedSqlExecutor.cs` và `AiScopeEvaluator.cs`.
4. **Viết Kiểm Thử Hồi Quy:** Thêm các Unit Test trong `HRMS.Tests` kiểm tra:
   - Cho phép người có đủ quyền truy cập đúng phạm vi.
   - Chặn đứng người thiếu quyền (Fail-Closed).
   - Kiểm tra không rò rỉ dữ liệu khi người dùng đổi phạm vi phòng ban/công ty.
