# Kiểm thử lại với Ollama trên máy thật — 03/10/2026

**Kết quả: 171/171 backend regression đạt, 9/9 Ollama live integration đạt, npm test Web đạt.** API/Business/Tests đã build lại thành công sau khi thêm kiểm thử model thật.

Ollama trả HTTP thành công tại http://127.0.0.1:11434, version 0.34.0. Các model có sẵn: qwen2.5:latest (7.6B), qwen2.5:3b (3.1B), bge-m3:latest (566.70M). Không tải thêm model.

## Phạm vi đã thử

Các kiểm thử gọi trực tiếp DLL Business vừa build, qua lớp OllamaService thật, gửi HTTP thật tới Ollama và dùng model đã cài. Không giả lập HTTP hay model.

Cấu hình host/model và prompt fixture được truyền vào bằng constructor hai tham số mới để kiểm thử không cần đọc TB_CONFIG trên Oracle. Constructor một tham số dùng trong production vẫn đọc SYS_CONFIG như trước. Bỏ log toàn bộ embedding response để tránh xuất mảng vector dài vào log.

Dữ liệu thử là nhân viên thử nghiệm A (#10), kỳ 09/2026, tổng tăng ca 24,5 giờ. Không dùng hồ sơ hay số liệu người thật. SQL sinh ra chỉ được kiểm tra, không được gửi Oracle thực thi. Không chạy migration, cấp/thu hồi GRANT, ghi bảng nghiệp vụ, ingest corpus hoặc đồng bộ Qdrant.

## Kết quả từng case

Đã chạy bộ 9 case, sau đó đổi test SQL sang câu hỏi tự nhiên và chạy lại riêng case đó thành công (1/1). Bảng dùng kết quả mới nhất cho SQL và kết quả bộ 9 case cho các case còn lại. Thời gian lấy từ từng UnitTestResult trong hai file TRX; gồm phần xử lý test/request, có thể gồm nạp model. Đây là một lượt smoke test, không phải benchmark tải.

| Case | Kết quả | Thời gian (giây) |
|---|---|---:|
| CancelledEmbedding_DoesNotReturnSuccessfulVector | Passed | 0.192 |
| Embedding_Has1024FiniteDimensionsAndStableRepeatedInput | Passed | 8.864 |
| Embedding_VietnameseMeaningRanksRelatedTextAboveUnrelatedText | Passed | 0.378 |
| MissingModel_ReturnsConnectionErrorWithoutPretendingToAnswer | Passed | 0.030 |
| PrimaryModel_ClassifiesOvertimeWithRealTransport | Passed | 39.404 |
| PrimaryModel_GeneratesViewOnlySqlButDoesNotExecuteIt | Passed | 46.037 |
| PrimaryModel_StreamingTokensMatchFinalAnswer | Passed | 13.789 |
| PrimaryModel_SummarizesSyntheticAuthorizedContext | Passed | 12.384 |
| SmallerInstalledModel_ClassifiesWithSameTransport | Passed | 12.324 |

- qwen2.5:latest phân loại câu hỏi “Tổng số giờ làm thêm của mã nhân viên 10 tháng 9 năm 2026?” thành OVERTIME.
- Test SQL cuối dùng câu tự nhiên “Nhân viên mã 10 làm thêm tổng bao nhiêu giờ trong tháng 9 năm 2026?”, cung cấp schema và tên bind, không cung cấp nguyên câu SQL. Model sinh đúng SUM(SOGIO), MANV/THANG/NAM và bind tương ứng trên HR.V_AI_OVERTIME_SUMMARY. Model vẫn bọc kết quả trong Markdown; validator hiện có loại wrapper rồi kiểm tra, test không khẳng định model tuân thủ tuyệt đối định dạng. Không thực thi câu SQL và chưa đánh giá NL2SQL tổng quát.
- Chat trả: “Trong kỳ 09/2026, nhân viên thử nghiệm A tăng ca tổng cộng 24,5 giờ.”
- Streaming trả: “Tổng số giờ tăng ca là 24,5 giờ.” Các token nhận được ghép thành đúng kết quả cuối.
- qwen2.5:3b cũng trả OVERTIME cho mẫu phân loại đã thử; chưa có bộ đánh giá đủ lớn để kết luận 3B tốt hơn hay thay 7B trong production.
- bge-m3 trả vector 1.024 chiều, hữu hạn, độ dài khác 0. Cùng đầu vào lặp lại có cosine 1,000.
- Với mẫu “tổng số giờ làm thêm”, câu gần nghĩa “báo cáo tổng giờ tăng ca” có cosine khoảng 0,840; câu về nấu phở có cosine khoảng 0,380. Một cặp mẫu chưa thay thế đo recall/precision retrieval trên corpus.
- Model không tồn tại trả lỗi kết nối, không bị coi là câu trả lời thành công. Embedding đã hủy trả null theo contract hiện tại; case này không cần model sinh output.

## Cách chạy lại

Từ D:\QL_NS\QuanLyNhanSu, build API trước vì tests tham chiếu DLL API:

~~~powershell
MSBuild.exe HRMS.Api\HRMS.Api.csproj /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo
MSBuild.exe HRMS.Tests\HRMS.Tests.csproj /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo

# Regression offline, không gọi outbox benchmark hay Ollama.
vstest.console.exe HRMS.Tests\bin\Debug\net472\HRMS.Tests.dll "/TestCaseFilter:FullyQualifiedName~Ai&FullyQualifiedName!~Benchmark_QdrantOutbox_Enqueue&TestCategory!=OllamaLive"

# Chỉ chạy khi chủ động bật test local.
$env:HRMS_OLLAMA_LIVE_TESTS='1'
vstest.console.exe HRMS.Tests\bin\Debug\net472\HRMS.Tests.dll "/TestCaseFilter:TestCategory=OllamaLive"
~~~

Ollama live tests có category OllamaLive và biến opt-in. Khi chưa bật HRMS_OLLAMA_LIVE_TESTS=1, chúng bị skip trước khi gọi network. Biến môi trường của đợt này chỉ đặt trong process shell chạy test, không sửa cấu hình Windows hoặc cấu hình ứng dụng.

File bằng chứng:

- [Backend regression TRX](C:/Users/duyth/AppData/Local/Temp/hrms-ai-ollama-retest-20261003/backend-regression.trx)
- [Ollama live TRX](C:/Users/duyth/AppData/Local/Temp/hrms-ai-ollama-retest-20261003/ollama-live.trx)
- [Ollama SQL từ câu hỏi tự nhiên TRX](C:/Users/duyth/AppData/Local/Temp/hrms-ai-ollama-retest-20261003/ollama-natural-query.trx)
- [Kết quả case dạng JSON](C:/Users/duyth/AppData/Local/Temp/hrms-ai-ollama-retest-20261003/ollama-live-results.json)
- [Model inventory](C:/Users/duyth/AppData/Local/Temp/hrms-ai-ollama-retest-20261003/models.json)
- [Log kiểm thử Web](C:/Users/duyth/AppData/Local/Temp/hrms-ai-ollama-retest-20261003/web-tests.log)

## Kết luận và giới hạn

Ollama/generation/streaming/embedding local hoạt động qua transport của dự án ở các mẫu đã thử. Generation 7B trong các lượt đã đo mất khoảng 12–46 giây/request; nạp model và trạng thái máy ảnh hưởng kết quả. Result cache hiện chỉ cache COUNT; không vì Ollama đã bật mà có cache câu trả lời LLM/embedding trong luồng V2.

**AiExecutionService/QueryUnderstandingService V2 hiện vẫn dùng quy tắc + SQL template, không gọi Ollama để hiểu yêu cầu.** Chạy thành công các test model độc lập không có nghĩa câu hỏi trong Web/Desktop đã được Ollama phân tích. Chưa nối bộ phân tích JSON bằng LLM vào cổng làm rõ, chưa bật RAG corpus/vector.

Chưa đọc cấu hình Ollama thực trong TB_CONFIG của database; host/model được cố định trong fixture local. Chưa kiểm thử HTTP/JWT/UI thật, quyền view trên Oracle thật, retrieval Qdrant, hybrid/reranking/multi-query hoặc chất lượng trên bộ câu hỏi lớn. Các giới hạn nghiệm thu trong [báo cáo V2](ai-rag-v2-test-report.md) vẫn còn hiệu lực.

Đợt này chỉ build lại API/Business/Tests và chạy lại npm test; không build lại Desktop/Web bundle vì các thay đổi chỉ ở lớp OllamaService và file test. Các build Desktop/Web đã có bằng chứng ở đợt trước; constructor cũ của OllamaService vẫn được giữ.