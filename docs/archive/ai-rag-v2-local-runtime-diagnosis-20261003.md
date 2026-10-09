# Chẩn đoán AI trên máy local — 03/10/2026

**Chưa nghiệm thu truy vấn dữ liệu.** Ollama hoạt động nhưng schema AI V2 chưa được triển khai trên Oracle mà API local sử dụng. Các test trước đây xác minh mã với policy/executor giả lập và transport Ollama thật; chúng không chứng minh database đã sẵn sàng.

## Bằng chứng trên môi trường thực

- API `http://localhost:55463/api/ai/status` truy cập được. Vite đang mở trên 5173 nhận TCP nhưng timeout cả GET / và /api/ai/status; không xác nhận bản Web đang mở đã tải mã mới. Phiên Vite riêng trên 5174 đã trả 200 cho trang và proxy API. Sau build, API trả `connected=true`, `llmAvailable=true`, `queryReady=false`.
- Kết nối HR và AI_READONLY đều thành công. Metadata cho thấy HR chưa có TB_AI_*, PKG_AI_* hoặc V_AI_*; HRMS_AI_CTX cũng chưa có. Đọc HR.TB_AI_REVISION trả ORA-00942.
- AI_READONLY sở hữu sáu view cũ (EMPLOYEE, OVERTIME, ALLOWANCE, INSURANCE, ADVANCE, ATTENDANCE). Chúng không phải các view bảo vệ HR.V_AI_* mà luồng V2 yêu cầu. Không chuyển mã sang các view cũ để bỏ qua phân quyền.
- So khớp 86 cột nguồn được các view V2 tham chiếu: thiếu TB_BANGLUONG.RUN_ID, TB_BANGLUONG.TRANGTHAI_CHITRA và toàn bộ bảng TB_PAYROLL_CALCULATION_RUN (bốn cột được view dùng: RUN_ID, MAKYCONG, STATUS, FINISHED_AT). Không có constraint unique/primary enabled trên đúng cặp MANV/MAKYCONG. Metadata bổ sung cho thấy TB_KYCONG.TRANGTHAI_CHITRA cũng thiếu; CONG_INPUT_REV/CONG_PUBLISH_REV/KHOA đã có.
- HR có quyền tạo table/view/package/trigger nhưng không có CREATE ANY CONTEXT. Trusted context cần bước DBA trong đúng service/PDB.
- Quyền cũ của AI_READONLY có SELECT ANY TABLE, CREATE VIEW, CONNECT và SELECT trực tiếp trên 10 bảng HR. Đây là đường truy cập rộng hơn thiết kế V2; cutover phải kiểm kê và thu hồi theo kế hoạch, không chỉ thêm view.

## Những sửa mã đã áp dụng

1. Lời chào thuần túy như “xin chào” đọc danh tính hiện tại và quyền F_SYSTEM_AI qua các bảng hệ thống hiện có. Không cần bảng policy AI, không truy vấn nhân viên và không làm mất yêu cầu đang làm rõ. User bị khóa hoặc thiếu quyền vẫn bị từ chối. Câu có nội dung nghiệp vụ sau lời chào vẫn cần đầy đủ policy.
2. Lỗi thiếu schema được phân loại AI_SETUP_REQUIRED. Lỗi nguồn khác là AI_SOURCE_UNAVAILABLE. Client không nhận connection string, SQL, tên object hay stack trace; server ghi stage/mã lỗi tối thiểu.
3. Endpoint status tách API kết nối, Ollama sẵn sàng và nguồn tra cứu sẵn sàng. Web hiện cảnh báo khi thiếu nguồn; không đánh đồng lỗi policy với Ollama offline. Probe schema dùng metadata và cache 10 giây, không đọc dữ liệu nhân sự. queryReady không bảo đảm user cụ thể đã được cấp scope hoặc quyền reader đã đạt kiểm toán.
4. V1_22/V1_23 có guard kiểm tra tất cả cột nguồn và unique MANV/MAKYCONG **trước DDL đầu tiên**. Oracle DDL tự commit nên không thể chỉ trông chờ rollback khi view compile lỗi ở cuối. Có thêm database/ai_rag_v2_local_preflight.sql chỉ đọc metadata để kiểm tra trước triển khai.

## Kiểm tra đã chạy

- Business/API/Tests build thành công. 183/183 regression AI backend đạt; XML TRX xác nhận 183 executed/passed, 0 failed. Filter loại test OllamaLive và benchmark ghi outbox. Sau khi áp dụng guard mới, đã build lại Tests và chạy riêng schema-contract: 1/1 đạt, xác nhận mọi cột view có guard trước CREATE TABLE và unique constraint phải enabled.
- Web `tsc -b`, Vite production build và toàn bộ bốn file `npm test` thành công. Còn cảnh báo bundle lớn; API có cảnh báo ACL state-file và System.Memory. Business build dùng IntermediateOutputPath riêng vì obj cũ bị khóa.
- Probe Business gọi **Oracle thật** với một actor đang hoạt động có quyền AI thật, không tạo JWT hay đọc thông tin nhân viên: LoadIdentity thành công; “xin chào” trả 200/answered; “nhân viên tên Thái?” trả 503/AI_SETUP_REQUIRED; schema probe false. Đây không phải kiểm thử POST qua JWT hoặc browser E2E.
- Kiểm tra HTTP API trực tiếp và qua phiên Vite kiểm tra mới 5174 đều trả 200 với cùng trạng thái nguồn chưa sẵn sàng. Tiến trình Vite 5173 hiện timeout; cần khởi động lại tiến trình đó để dùng bản Web mới. Không dừng tiến trình của người dùng trong đợt kiểm tra này. POST chat không có JWT trả 401; không mở lời chào cho người chưa đăng nhập. Chưa kiểm thử POST với phiên đăng nhập trên browser của người dùng.
- Bằng chứng tại thư mục tạm `C:\Users\duyth\AppData\Local\Temp\hrms-ai-runtime-diagnostic-20261003`: oracle-metadata.log, oracle-business.log, local-api-status.json, runtime-readiness-regression.trx và web-build. Không chứa mật khẩu trong báo cáo/log.

## Kế hoạch Oracle cần duyệt trước khi thực hiện

1. Backup schema DDL, grants, sáu view cũ và trạng thái migration; xác nhận service/PDB và cửa sổ bảo trì. Chưa chạy bất kỳ CREATE/ALTER/GRANT/REVOKE nào trong đợt chẩn đoán này.
2. Rà soát trạng thái thực của V1_16/V1_18/V1_20 và sửa các tiền đề còn thiếu theo schema thực. **Không chạy mù toàn bộ V1_16**: script có nhiều bảng/chính sách lương, seed và kiểm tra fixture nghiệp vụ cố định. Không tạo run SUCCESS giả hoặc backfill RUN_ID giả để đưa lương cũ vào AI. Nếu chưa phê duyệt thay đổi payroll, cần một phương án triển khai giới hạn domain khác được chuẩn bị riêng; migration đầy đủ hiện sẽ dừng tại preflight.
3. Preflight đạt rồi mới áp dụng owner migration V1_22 (database này chưa có foundation). DBA thiết lập HRMS_AI_CTX và quyền đúng trong cùng PDB; HR sở hữu năm bảng policy/ticket/revision, hai package và 15 view ánh xạ bảo vệ. AI_READONLY chỉ đọc whitelist và consume ticket.
4. Sau kiểm kê tác động, loại quyền SELECT ANY TABLE/CREATE VIEW/role hoặc grant rộng và reader-owned view tạo đường đọc ngoài thiết kế. Script DBA hiện tự xử lý direct grants bảng HR; system privilege/roles/reader-owned views cần xử lý riêng sau đánh giá. Lưu DDL trước khi gỡ view. Không phục hồi broad grants như một cách rollback hoạt động AI.
5. Cấp scope rõ ràng cho USER/GROUP và từng capability. Quyền F_SYSTEM_AI hiện có không tự cấp ALL. Cần chọn actor và phạm vi được duyệt (SELF, phòng ban hoặc công ty); không tự cấp toàn bộ dữ liệu cho tài khoản admin.
6. Chạy V1_23_ai_readonly_verify.sql và các ca reader không vé, vé đúng/sai capability, hết hạn/tái sử dụng, pool cleanup, deny/SELF/phòng ban/công ty, thay quyền/source revision và cache. Đối chiếu câu “nhân viên tên Thái?” bằng dữ liệu được phép; nếu trùng tên phải hỏi lại. Kiểm tra cache COUNT bằng hit/miss và cập nhật nguồn thật. Sau đó nghiệm thu POST với phiên đăng nhập thật và giao diện Web.

Các file DBA/owner/verify/rollback và ví dụ cấp scope nằm trong [runbook database](ai-rag-v2-database-runbook.md). Chưa chạy chúng. Hybrid/reranking/chunking/multi-query và RAG corpus vẫn chưa được tích hợp; sửa lỗi local này không thay đổi giới hạn đó.