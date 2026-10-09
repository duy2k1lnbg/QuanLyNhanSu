# Triển khai database AI V2 — script đã chuẩn bị, chưa chạy

Cập nhật 03/10/2026. Đợt sửa mã không mở kết nối để chạy migration, không thay mật khẩu/quyền thực tế, không sync Qdrant. Các bước dưới đây phải thực hiện trên Oracle staging trước khi phê duyệt production.

## Phân chia tài khoản và đối tượng

Không tạo bảng nghiệp vụ mới trong AI_READONLY. Schema HR sở hữu:

| Đối tượng | Vai trò |
|---|---|
| TB_AI_CAPABILITY | Domain, operation, quyền chức năng, source view, bật/tắt |
| TB_AI_SCOPE_GRANT | USER/GROUP, capability, SELF/DEPARTMENT/COMPANY/ALL, ALLOW/DENY, thời hạn |
| TB_AI_FIELD_POLICY | FULL/MASK/DENY và operation được phép theo trường |
| TB_AI_REVISION | POLICY_GLOBAL và DATA_* để kiểm tra freshness |
| TB_AI_READ_TICKET | Vé ngẫu nhiên một lần, hạn 40 giây, gắn actor/capability/revision/session; không lưu câu hỏi |

AI_READONLY chỉ cần CREATE SESSION, SELECT trên 15 view đã bảo vệ, EXECUTE trên HR.PKG_AI_READER để bind/clear vé. Không cấp SELECT bảng gốc/policy/ticket, DML trực tiếp, SELECT ANY TABLE, RESOURCE, CREATE VIEW/SYNONYM hoặc EXECUTE trên PKG_AI_TICKET. HR application connection cấp vé; vé không được tạo bởi LLM hoặc user client.

Views: V_AI_EMPLOYEE_LOOKUP, V_AI_EMPLOYEE, V_AI_EMPLOYEE_COUNT, V_AI_ORG_LOOKUP, V_AI_PERIOD, V_AI_OVERTIME, V_AI_OVERTIME_SUMMARY, V_AI_ALLOWANCE, V_AI_INSURANCE, V_AI_ADVANCE, V_AI_ATTENDANCE_SUMMARY, V_AI_PAYROLL, V_AI_PAYROLL_SUMMARY, V_AI_CONTRACT, V_AI_SALARY_CHANGE.

EMPLOYEE_COUNT chỉ thấy số đếm theo tổ chức sau lọc scope, không có MANV/HOTEN. OVERTIME_SUM dùng summary theo nhân viên/tháng, không có ngày/lượt tăng ca. PAYROLL_SUMMARY tính trực tiếp từ nguồn gốc có quyền, không mở khóa view lương chi tiết hay lookup nhân viên. Trường của summary và chi tiết không được coi là một capability.

## Chẩn đoán local mới

Ngày 03/10/2026 đã kết nối kiểm tra metadata Oracle thật, không chạy migration. Database local chưa có foundation AI và thiếu tiền đề payroll. Xem [bằng chứng và kế hoạch local](ai-rag-v2-local-runtime-diagnosis-20261003.md). Chạy database/ai_rag_v2_local_preflight.sql chỉ đọc trước mọi DDL; V1_22/V1_23 cũng kiểm tra source contract trước DDL đầu tiên. Không chạy nguyên V1_16 chỉ để mở chatbot khi chưa rà soát seed/fixture và tác động nghiệp vụ.

## Preflight

1. Xác nhận service/PDB/schema/tài khoản đích thực tế và phiên bản Oracle hỗ trợ identity/fetch-first. Không dùng tên PDB ví dụ làm mặc định triển khai.
2. Dừng luồng AI hoặc API/Desktop trong cửa sổ bảo trì. Backup DDL/grants/context và các bảng policy đang có; lưu schema version đang chạy. Oracle DDL có auto-commit: SQLERROR ROLLBACK không hoàn tác toàn bộ DDL.
3. Xác nhận schema thực khớp EDMX hiện tại và các migration tiền đề V1_16/V1_18/V1_20. Một số bảng trong V1_0 cũ không khớp model hiện tại, đặc biệt nâng lương phải là TB_NANGLUONG_NHANVIEN.
4. Kiểm tra USER/ALL_TAB_COLUMNS cho các source. Nâng lương: SOQDNL, SOHD, NGAYKYNL, NGAYLENLUONG, HESOLUONG_NOW, HESOLUONG_NEW, DELETED_DATE; hợp đồng dùng DEL_DATE. Bảng lương cần THUC_LINH, LUONG_CONG_THUCTE, CONG_THUCTE, RUN_ID, TRANGTHAI_CHITRA. TB_PAYROLL_CALCULATION_RUN cần RUN_ID, MAKYCONG, STATUS, FINISHED_AT. Kỳ công cần KHOA, CONG_PUBLISH_REV, TRANGTHAI và DELETED_DATE.
5. Xác nhận constraint UQ_BANGLUONG_MANV_KYCONG enabled và không có bản ghi trùng MANV/kỳ. Xác minh payroll rows hiện hành gắn đúng successful run; các view AI lương chỉ đọc kỳ đã khóa và run SUCCESS có FINISHED_AT. Legacy rows không có RUN_ID, kỳ chưa khóa, FAILED/PARTIAL_SUCCESS/IN_PROGRESS không được đưa vào kết quả AI. Không tự backfill giá trị giả để qua kiểm tra.
6. Xác nhận các bảng policy cũ có đủ cột, constraint và không trùng CAPABILITY_CODE/LOGICAL_FIELD. Migration fail nếu unique index không thể tạo; không chọn một bản policy ngẫu nhiên.
7. Kiểm kê trực tiếp/qua role/PUBLIC, reader-owned views và quyền ANY. Không chạy lại database/setup_ai_readonly.sql cũ như bước mặc định; file đó có thể mở quyền bảng gốc. File cũ được giữ để đối chiếu.

## Thứ tự thực hiện trong staging

1. DBA chạy V1_23_ai_readonly_dba_setup.sql trong PDB đã xác minh: tạo trusted context HRMS_AI_CTX USING HR.PKG_AI_READER, lưu inventory, thu hồi các direct raw-table grants và alias/package grants cũ nêu trong script. Các role, system privilege, PUBLIC hoặc reader-owned view còn lại phải xử lý sau đánh giá tác động; script không tự xóa chúng.
2. HR chạy V1_22__ai_rag_v2_foundation_and_policies.sql nếu mới; chạy V1_23__ai_rag_v2_security_and_correctness.sql nếu V1_22 cũ đã được áp dụng. Cả hai chứa cùng repair idempotent; migrator có thể chạy tiếp V1_23 sau V1_22. Không bỏ qua SQLERROR/invalid objects.
3. Kiểm tra USER_ERRORS và USER_OBJECTS: packages, package bodies, views, triggers phải VALID; đối chiếu tất cả column contract. Preflight constraints ở cuối script phải đạt. Script tăng POLICY_GLOBAL để vô hiệu các vé/snapshot cũ.
4. Quản trị nạp quyền chức năng thật và grants rõ ràng. Không có seed tự cấp ALL cho admin hay cho tất cả user.
5. DBA chạy V1_23_ai_readonly_verify.sql. Nó chặn raw HR table grants cho reader/PUBLIC, ticket issuer bị lộ, roles/system privileges, reader-owned views, HR objects ngoài whitelist và context sai. DBA đọc kỹ diagnostics; không coi SELECT DBA_ERRORS in ra lỗi là thành công.
6. Đăng nhập AI_READONLY không có vé: truy vấn mọi view phải trả 0 row; gọi issuer phải bị từ chối; SELECT bảng gốc phải bị từ chối. Sau đó thử các vé hợp lệ do HR cấp trong kiểm thử có kiểm soát.
7. Triển khai build API/Business/Desktop/Web cùng bản, khởi động lại để loại state/cache cũ. Xác nhận connection AI thực dùng AI_READONLY, không fallback credential HR khi gặp lỗi.

SQL*Plus/SQLcl nên nhập mật khẩu qua phương thức bảo mật được DBA chấp thuận; không đưa credential vào command line, tài liệu, log hoặc lịch sử shell. Dùng WHENEVER SQLERROR EXIT FAILURE của script và lưu output không chứa secret.

## Cấp scope: mẫu cần thay bằng ID đã xác minh

Ví dụ dưới đây chỉ là mẫu cho HR/DBA, không chạy tự động. Xác minh SUBJECT_ID là IDUSER hoặc ID_GROUP thực, không phải MANV.

```sql
INSERT INTO HR.TB_AI_SCOPE_GRANT
 (SUBJECT_TYPE,SUBJECT_ID,CAPABILITY_CODE,SCOPE_TYPE,SCOPE_KEY,EFFECT,IS_ENABLED)
 VALUES ('USER', :verified_user_id, 'PAYROLL_SELF', 'SELF', NULL, 'ALLOW', 1);

INSERT INTO HR.TB_AI_SCOPE_GRANT
 (SUBJECT_TYPE,SUBJECT_ID,CAPABILITY_CODE,SCOPE_TYPE,SCOPE_KEY,EFFECT,IS_ENABLED)
 VALUES ('GROUP', :verified_group_id, 'OVERTIME_SUM', 'DEPARTMENT', :verified_idpb, 'ALLOW', 1);
COMMIT;
```

COMPANY key phải khớp TO_CHAR(TB_NHANVIEN.IDCTY,'TM9'), không lấy chuỗi login MACTY làm suy luận. Grant VIEW không cấp SUM/COUNT, và ngược lại. Muốn quản trị tra cứu rộng cần quyền chức năng thật và grant ALL cho từng capability cụ thể. DENY thắng ALLOW. Tự xem lương/bảo hiểm vẫn cần F_SYSTEM_AI và grant SELF; không tự mở quyền thu nhập.

ALLOWED_OPERATIONS dùng danh sách phân cách dấu phẩy theo enum trong mã: COUNT, LOOKUP, LIST, PROFILE, VIEW, SUM, SUMMARY, TOP/RANK, EXPIRING, FILTER. Nếu khác rỗng, ghi đầy đủ operation muốn cho phép; FILTER là quyền dùng trường làm điều kiện giá trị. Không có hỗ trợ wildcard `*`. MASK_PATTERN hiện chưa dùng: MASK trả `***`.

## Các ca bắt buộc trước production

- User không grant, admin không grant, thiếu quyền chức năng: không có dữ liệu/candidate, HTTP đúng.
- SELF, DEPARTMENT, COMPANY, ALL và DENY chồng nhau; lương người khác không được mở bởi quyền SELF.
- Hai Oracle sessions: vé không dùng chéo session, không bind lần hai, không dùng quá hạn; cleanup sau thành công/error/timeout/cancel, pool reuse không giữ actor cũ.
- Sau chuyển phòng, đổi nhóm, thu hồi quyền hoặc field policy: candidate, data, conversation và cache cũ không trả kết quả ngoài quyền.
- Query dữ liệu trong lúc revision thay đổi: không publish/cache dữ liệu cũ; cập nhật revision rollback cùng transaction nguồn.
- COUNT chỉ trả aggregates; OVERTIME_SUM không đọc V_AI_OVERTIME raw; PAYROLL_SUMMARY không mở employee lookup/detail; vé khác capability không mở view.
- Lương đối chiếu số thật: đúng đơn vị, chỉ run/kỳ đủ điều kiện, trạng thái khóa tách biệt trạng thái chi trả, tổng nhiều phòng không chỉ lấy dòng đầu.
- List/Top có giới hạn, total/hasMore đúng; no_data khác lỗi source/schema thiếu.
- Web/Desktop: làm rõ đầy đủ, stale option, đổi tài khoản, reset/cancel, và response đến trễ. Desktop chưa có HTTP/JWT parity: chỉ nghiệm thu phần dùng chung Business engine.

## Rollback an toàn

V1_22_rollback__revert_ai_rag_v2.sql tắt capability, tăng policy revision và xóa vé; không xóa bảng nghiệp vụ, không khôi phục raw grants hoặc view chưa lọc. Tắt luồng AI và restart trước khi rollback binary. Không deploy binary cũ với giả định SQL tự do sẽ vẫn hoạt động.

Nếu cần khôi phục DDL, DBA dùng bản backup đã duyệt trong cửa sổ bảo trì nhưng giữ AI tắt tới khi security verification lại đạt. Chưa có Oracle integration run trong đợt sửa này; compile/build .NET không xác nhận PL/SQL compile hoặc quyền DB.