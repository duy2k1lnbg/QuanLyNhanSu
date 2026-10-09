# Kết quả rà soát và sửa AI/RAG V2 — 03/10/2026

Đã sửa trực tiếp mã nguồn trong D:\QL_NS\QuanLyNhanSu và chuẩn bị lại các script database. Kết quả kiểm tra mã cuối cùng: **171/171 kiểm thử AI backend đạt**, API/Business/Tests build thành công, Web typecheck/build và toàn bộ 4 file kiểm thử của npm test thành công. Desktop đã build riêng bằng MSBuild AnyCPU; chưa chạy kiểm thử giao diện thực tế.

Báo cáo này thay thế các khẳng định 122/122 và “đã hoàn thành toàn bộ 60 tiêu chí” trước đó. Số test đạt không đồng nghĩa đã nghiệm thu database, UI, xác thực HTTP/JWT của Desktop hoặc RAG tài liệu.

## Những vấn đề đã sửa

| Phần | Sửa trong mã và tác dụng |
|---|---|
| Làm rõ đầu vào | Giữ yêu cầu gốc và các slot đối tượng/kỳ/bộ lọc sau khi chọn hoặc trả lời bổ sung; hỏi lại khi trùng người/phòng hoặc thiếu năm/chỉ tiêu; không tự lấy ứng viên đầu tiên. Token lựa chọn là giá trị ngẫu nhiên, gắn user, conversation, prompt ID và version, hết hạn sau 10 phút. |
| Hiểu đúng phạm vi | Không tự cấp toàn công ty từ MaCty, admin hoặc dấu *. Capability cần quyền chức năng thật và grant phạm vi cụ thể. Mã của chính người hỏi dùng PAYROLL_SELF; mã người khác cần PAYROLL_VIEW. Nhóm disabled hoặc không phải ISGROUP=1 không được cấp quyền/grant. |
| Chặn truy vấn sai ý | Tháng 13, khoảng hợp đồng ngoài giới hạn, người/phòng không tìm thấy, so sánh nhiều kỳ và hồ sơ nhân sự lịch sử chưa có snapshot không được biến thành truy vấn hiện tại/rộng hơn. Lương cơ bản tháng chưa được ánh xạ được trả unsupported. |
| Điều kiện số tiền | Phụ cấp và tạm ứng truyền đồng thời số tiền, nhân viên và kỳ vào SQL tham số. Hỗ trợ một ngưỡng với <, >, <=, >=, =; khoảng tiền/nhiều ngưỡng hoặc bộ lọc tiền cho nghiệp vụ chưa ánh xạ được từ chối, tránh âm thầm bỏ điều kiện. Hiện bộ đọc câu tiền nhận đơn vị triệu/tr; chưa khẳng định hiểu mọi cách viết VND. |
| Policy theo trường | Thiếu field policy thì DENY; MASK kết xuất ***. Cấm tổng hợp/lọc trường bị MASK/DENY, kiểm tra ALLOWED_OPERATIONS khi có cấu hình. Policy không thể bỏ quyền chức năng bắt buộc đã khai báo trong capability. |
| Đọc dữ liệu | SQL nghiệp vụ chỉ từ template và các view HR.V_AI_*; lời gọi SQL tự do thiếu ngữ cảnh/kế hoạch bị chặn. BindByName, tham số có kiểu, timeout và giới hạn đọc được áp dụng. Legacy RAG thiếu danh tính không được gọi retrieval/LLM. |
| Nguồn tổng hợp | COUNT dùng view tổng theo tổ chức không có MANV/HOTEN. SUM/TOP tăng ca dùng nguồn đã tổng hợp theo người/tháng, không mở lượt tăng ca/ngày. Quỹ lương tổng hợp tách khỏi payroll detail và employee lookup. |
| Nguồn payroll/schema | Đối chiếu EDMX và migration V1_16/V1_18/V1_20; sửa cột và bảng thực, gồm TB_NANGLUONG_NHANVIEN, soft delete, kỳ khóa sổ và run SUCCESS có FINISHED_AT. Không gán “đã trả” từ một trạng thái giả. Kiểm tra này là contract offline, chưa chứng minh Oracle compile. |
| Cache thực | Nối result cache vào luồng chat cho EMPLOYEE + COUNT có source revision đáng tin cậy. TTL 60 giây, tối đa 1.000 entry/process. Khóa gồm actor, quyền/scope/field policy, RequestedScope, template, tham số có kiểu, source revision. Lương và dữ liệu cá nhân/thu nhập bypass. |
| Cache và quyền thay đổi | Kiểm tra lại policy/data revision trước trả cache hit, publish và commit; thay đổi trong lúc đọc trả 409. Gộp truy vấn đồng thời bằng single flight; lỗi không giữ task hỏng, reset/cancel không publish kết quả trễ. Có bộ đếm hit/miss/bypass/load/publish nội bộ. |
| Hội thoại và client | Lease generation/sequence/version chặn response trễ sau reset, expiry, eviction hoặc request mới. Web gửi đúng camelCase/prompt ID/version, hủy và loại response trễ khi reset/unmount/đổi actor, tạo conversation mới khi mount/reload. Desktop dùng cùng Business service và policy, có lựa chọn làm rõ/reset/cancel. |
| Kết quả và dashboard | Giữ riêng forbidden/unsupported/no_data/error; HTTP status phù hợp; không trả SQL, tên view nội bộ hoặc lỗi Oracle thô. Dashboard Desktop bỏ đường đọc raw HR/CCCD/số điện thoại, dùng danh sách tối thiểu và count có quyền; KPI chưa có ánh xạ để trống. |

Bộ hiểu câu hỏi hiện vẫn dựa trên quy tắc và template trong mã, chưa phải LLM hiểu yêu cầu JSON tổng quát. OracleSqlAstValidator là bộ kiểm tra token/whitelist có giới hạn, không phải một parser AST Oracle đầy đủ; nguồn bảo vệ chính là template cố định, policy, tham số và view có context.

## Database và AI_READONLY

Các object quản trị đặt ở schema HR, gồm 5 bảng: TB_AI_CAPABILITY, TB_AI_SCOPE_GRANT, TB_AI_FIELD_POLICY, TB_AI_REVISION, TB_AI_READ_TICKET. AI_READONLY không sở hữu bảng policy/cache và không được quyền ghi vào các bảng này.

Script chuẩn bị 15 view được bảo vệ, package cấp vé chỉ dành kết nối HR tin cậy và package reader/context dành AI_READONLY. Vé một lần, gắn Oracle session, có thời hạn và policy revision; SELF_ONLY giới hạn MANV trước khi tổng hợp. Package đọc kiểm tra quyền/phạm vi hiện tại; cuối request dọn context, clear pool nếu dọn thất bại. Chỉ cấp SELECT các view được duyệt và EXECUTE package reader, không cấp bảng gốc.

- V1_22__ai_rag_v2_foundation_and_policies.sql: thiết lập nền cho database chưa áp dụng V1_22.
- V1_23__ai_rag_v2_security_and_correctness.sql: bản repair/idempotent cho database đã có V1_22 cũ.
- V1_23_ai_readonly_dba_setup.sql: thiết lập trusted context và xử lý direct grant cũ bằng quyền DBA.
- V1_23_ai_readonly_verify.sql: phát hiện grant bảng gốc, object không được duyệt, PUBLIC/role/system privilege và object thuộc reader có thể tạo đường truy cập khác.
- V1_22_rollback__revert_ai_rag_v2.sql: tắt capability, đổi policy revision/dọn ticket, giữ dữ liệu nghiệp vụ.
- Chi tiết thứ tự và điều kiện chạy: [runbook database](ai-rag-v2-database-runbook.md).

**Chưa chạy migration/GRANT/revoke trên Oracle thật, chưa tạo context thật, chưa xác nhận Oracle compile hoặc row isolation thực tế.** Script database/setup_ai_readonly.sql cũ được giữ nguyên; nó chưa phải đường triển khai mới. Không dùng script cũ để mở lại quyền đọc bảng gốc.

Việc chặn quyền phải được kiểm thử trên staging với database đã áp dụng các migration prerequisite, grant cụ thể và pool thật trước khi triển khai. Không tự seed ALL scope cho mọi người dùng.

## Bằng chứng kiểm thử

Backend chạy ngày 03/10/2026 bằng MSBuild/VSTest của Visual Studio 2022, target .NET Framework 4.7.2.

| Fixture | Số case đã chạy | Kết quả |
|---|---:|---|
| Bu.Tests.AiCacheServiceTests | 4 | Đạt |
| Bu.Tests.AiIntegrationAndClientTests | 21 | Đạt |
| Bu.Tests.AiPromptInjectionTests | 33 | Đạt |
| Bu.Tests.AiQueryUnderstandingTests | 20 | Đạt |
| Bu.Tests.AiRetrievalBenchmarkTests | 3 | Đạt, utility offline |
| Bu.Tests.AiSecurityAndAuthorizationTests | 11 | Đạt |
| HRMS.Tests.AiCacheAndVectorV2Tests | 11 | Đạt |
| HRMS.Tests.AiProductionFlowRegressionTests | 54 | Đạt |
| HRMS.Tests.AiQueryPlannerAndExecutionTests | 14 | Đạt |
| **Tổng** | **171** | **171 đạt, 0 thất bại, 0 không chạy trong tập đã chọn** |

Lệnh build và filter kiểm thử:

~~~powershell
MSBuild.exe HRMS.Api\HRMS.Api.csproj /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo
MSBuild.exe HRMS.Tests\HRMS.Tests.csproj /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo
vstest.console.exe HRMS.Tests\bin\Debug\net472\HRMS.Tests.dll "/TestCaseFilter:FullyQualifiedName~Ai&FullyQualifiedName!~Benchmark_QdrantOutbox_Enqueue&TestCategory!=OllamaLive"
~~~

File kết quả gốc: C:\Users\duyth\AppData\Local\Temp\hrms-ai-fix-work\TestResults\ai-fixes-final-4.trx. Đã đọc XML Counters để xác nhận 171 executed/passed và 0 failed.

Các test mới gọi luồng AiExecutionService thực với executor/entity/policy provider giả lập có kiểm soát; kiểm tra SQL/parameters/status/cache/state thực được tạo bởi production code. Chúng không đọc/ghi dữ liệu nhân sự trên Oracle. Test controller và JSON serialization không thay thế kiểm thử HTTP đầy đủ qua IIS/JWT.

Benchmark_QdrantOutbox_Enqueue được **loại khỏi tập chạy** vì có thể ghi Oracle/outbox. Ba benchmark utility còn lại không đo độ trễ chat thực tế, Oracle hoặc Qdrant; không dùng chúng để tuyên bố hệ thống đạt hiệu năng production.

Web đã chạy:

~~~powershell
npm run build -- --outDir C:\Users\duyth\AppData\Local\Temp\hrms-ai-web-verified-20261003-final
npm test
~~~

Build bao gồm tsc -b và Vite. Toàn bộ 4 file test script của npm test thành công: kpiAndAttendance, welcomeAndScrollReveal, heroCarousel và aiChatbot. Test AI gọi module payload/guard thực, kiểm tra token/prompt/version, actor key và loại response trễ. Chưa chạy browser E2E hay lint trong đợt xác minh này; các con số lint của báo cáo cũ không được dùng làm bằng chứng mới.

Desktop build sử dụng AnyCPU, dependency DLL Business vừa build và IntermediateOutputPath/FakesOutputPath/OutputPath/PublishDir trong thư mục tạm riêng để tránh obj/bin cũ bị khóa. Artifact build: C:\Users\duyth\AppData\Local\Temp\hrms-ai-desktop-verified-20261003\bin\QLyNSu.exe. Chưa chạy UI hoặc triển khai executable.

Các cảnh báo còn có: API state-file ACL và xung đột System.Memory; Vite cảnh báo bundle JS khoảng 2,3 MB; Desktop cảnh báo một số fake không sinh được và biến không dùng. Build vẫn thành công, chưa coi cảnh báo dependency/runtime là đã nghiệm thu.

git diff --check đạt đối với các file tracked; có cảnh báo đổi LF/CRLF. Các file untracked mới vẫn được giữ trong workspace, chưa commit.

## Phần chưa hoàn tất/nghiệm thu

1. Chạy compile/migration trên Oracle staging và kiểm tra quyền SELECT trực tiếp bằng AI_READONLY, deny/SELF/company/department, thu hồi quyền/disabled group, ticket reuse/expiry, reset context/connection pool, source revisions và tính đúng dữ liệu payroll thật.
2. Kiểm thử Web/Desktop bằng UI thực và HTTP API qua cơ chế xác thực thật. Desktop hiện dùng Business service + UserSession; **chưa dùng HTTP/JWT như Web**.
3. Corpus tài liệu có nguồn ánh xạ và ACL được duyệt chưa có. SearchScopedAsync cố ý không gọi Qdrant cho tới khi có cấu hình này. Hybrid/BM25/RRF, reranking, chunking, multi-query và embedding cache chưa được triển khai trong luồng mới.
4. ATTENDANCE_DETAIL và INSURANCE_VIEW quản lý đang tắt vì chưa đủ ánh xạ nguồn/quyền; lương cơ bản tháng và hồ sơ lịch sử chưa được xác nhận. Những câu hỏi này trả unsupported.
5. PlanCache/EntityCache/EmbeddingCache mới là utility, chưa tích hợp production SQL. Result cache và conversation state ở RAM từng process, chưa đồng bộ nhiều instance.
6. Hiệu năng chat/Oracle và số liệu cache hit rate thực tế chưa được đo trên môi trường chạy thật.

[Checklist 60 tiêu chí](ai-rag-v2-checklist.md) được giữ dưới dạng mục tiêu nghiệm thu chưa ký xác nhận. Không đánh dấu toàn bộ hoàn thành từ unit test.

## Kiểm thử lại khi Ollama đã bật

Đã build lại API/Business/Tests và chạy lại ngày 03/10/2026: 171/171 regression đạt, 9/9 test Ollama thật đạt, npm test Web đạt. Test Ollama có category/opt-in riêng, dùng cấu hình và context giả lập nhưng transport/model thật. Kết quả này chưa xác nhận kết nối database hay tích hợp LLM vào cổng làm rõ V2. Chi tiết, thời gian và bằng chứng: [báo cáo Ollama local](ai-ollama-live-test-report-20261003.md).


## Kiểm tra runtime local sau phản hồi “chưa hoạt động”

Đã xác định trên Oracle thật: kết nối thành công nhưng HR chưa có schema AI V2, queryReady=false và full policy gặp ORA-00942. Đã sửa lời chào/trạng thái nguồn; 183/183 backend regression và Web build/test đạt. Probe Business dùng danh tính Oracle thật trả lời chào 200, câu hỏi dữ liệu vẫn 503/AI_SETUP_REQUIRED. Chưa áp dụng migration/phân quyền và chưa nghiệm thu truy vấn dữ liệu qua UI/JWT. Xem [chẩn đoán và kế hoạch Oracle local](ai-rag-v2-local-runtime-diagnosis-20261003.md).
