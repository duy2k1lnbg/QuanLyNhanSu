# Bộ dữ liệu giả lập 200 nhân viên

Phạm vi: một công ty, Oracle 19c, kỳ 07–09/2026. Đây là ghi chép của đợt dữ liệu ngày 28/09/2026; không phải chỉ dẫn chạy lại hoặc xác nhận trạng thái database hiện tại. Không dùng tỷ lệ tiền của fixture làm căn cứ trả lương thật.

## Tệp và thứ tự đã sử dụng

1. backup_hr.sql: export HR bằng DBMS_DATAPUMP ở cùng SCN.
2. verify_backup_restore.sql: khôi phục bản sao vào HR_RESTORE_200 (account locked).
3. New-UatSchema.ps1: tạo HR_UAT_200 từ metadata, không sao chép dữ liệu cá nhân; mật khẩu ngẫu nhiên lưu DPAPI trong artifacts/synthetic200.
4. Invoke-UatSql.ps1 lần lượt áp dụng repair_schema.sql, payroll_alignment.sql, region_mapping.sql và seed.sql trên schema UAT rỗng. Các script cấu trúc không phải tất cả đều idempotent; không chạy lại mù quáng trên schema đã có.
5. Build-IsolatedTests.ps1, Run-UatAttendance.ps1: kiểm thử engine và công bố công. Mã request V2 cố định phục vụ idempotency; muốn tính phiên bản mới phải dùng khóa mới.
6. verify_scenarios.sql, Run-UatChecks.ps1: đối chiếu 23 tình huống và kiểm tra phạm vi/idempotency/khóa kỳ.
7. Build-UatPayroll.ps1, Run-UatPayroll.ps1: chạy engine lương thực trên UAT. Có thể đặt HRMS_UAT_MONTH=7/8/9. HRMS_UAT_CHECK_ONLY=1 chỉ kiểm tra loader nhận công mới/từ chối công cũ; thay đổi revision được rollback.
8. input_revisions.sql: trigger UAT, cài sau seed và công bố.
9. V1_19__attendance_payroll_data_integrity.sql: bổ sung cấu trúc HR đã thử trên UAT.
10. cutover_hr.sql: thay 63 bảng nghiệp vụ theo thứ tự khóa ngoại trong một transaction; xóa con trỏ ngày trước để phá vòng tham chiếu, phục hồi con trỏ sau khi copy; bảo toàn tài khoản và từng dòng phân quyền cũ. Tạo actor audit giả lập ID=1 bị vô hiệu hóa, không mật khẩu.
11. finalize_hr.sql: đồng bộ named sequence/identity, vô hiệu hóa trigger tự bịa hồ sơ bảo hiểm, cài hr_input_revisions.sql, ghi SCHEMA_VERSION 1.19.
12. postflight_hr.sql: kiểm tra sau chuyển, gồm phép thử revision và khóa kỳ có rollback.

cutover_hr.sql là tác vụ một lần, chỉ chấp nhận baseline 976 nhân viên và revision cũ. Không chạy lại sau khi đã chuyển thành công. DDL Oracle tự commit; migration cấu trúc tách khỏi transaction thay dữ liệu.

Các script dùng đường dẫn Oracle trên máy hiện tại D:/Oracle. Script UAT tự xác minh SELECT USER. Không đưa password/chuỗi kết nối vào log hoặc repository.

## Bảo toàn dữ liệu

Dump gốc:
D:/app/nguyenduy/admin/orcl/dpdump/HR_before_synthetic200_20260928.dmp

SHA-256:
DBED4AEEDD4917C57870A7C3D516F19C9F1388DB69E79E75754BE5C8B0989248

Schema HR_RESTORE_200 đã khôi phục đầy đủ dữ liệu bảng, kiểm tra có 976 nhân viên; account khóa. Bản import kiểm thử chủ động không nhập procedure/trigger/grant. Dump đầy đủ vẫn là nguồn khôi phục các đối tượng đó.

Khôi phục phải là một tác vụ riêng có phạm vi rõ ràng: ngừng các writer, chọn SCN/bản sao, kiểm tra schema đích trống hoặc quy trình thay có kiểm soát, import schema remap, xác minh số lượng/quan hệ/quyền/sequence, sau đó mới đổi ứng dụng. Không chạy TABLE_EXISTS_ACTION=REPLACE lên HR đang dùng theo thói quen. Không xóa bản sao này trong quá trình hoàn thiện UI.

Không thực hiện git reset/restore hàng loạt khi muốn quay lại mã nguồn: repository đã có nhiều sửa đổi của người dùng từ trước.

## Dữ liệu có chủ đích

- 200 nhân viên, 200 hợp đồng, 200 hồ sơ bảo hiểm, thuế, công đoàn và lịch sử lương ban đầu.
- Sáu người vào 15/09; năm người hết hợp đồng giữa tháng 9. Các ngày ngoài hiệu lực không hưởng công.
- Tháng 9 có bảy người chờ xác minh công; không tạo bảng lương cho họ.
- Bảng quyết toán năm và lịch sử quyết định bất thường có thể rỗng khi chưa có giao dịch thật tương ứng. Không tạo quyết định phê duyệt giả chỉ để bảng nào cũng có dòng.
- TB_TANGCA cũ không được seed độc lập để tránh trả trùng OT; OT chuẩn lấy từ đơn được duyệt và phân đoạn hiện hành. Cần đối chiếu nguồn đọc của từng giao diện với công đã công bố.
- TB_LUONG_HIEULUC, TB_BAOHIEM_BIENDONG, TB_PHEP_SOPHATSINH, TB_CHAMCONG_SUKIEN là phần nền; không đồng nghĩa tất cả các workflow đã nối đầy đủ.

Xem [tài liệu database](../../docs/database.md), [kiểm thử](../../docs/testing.md) và [hiện trạng](../../docs/current-status.md) để biết phạm vi và giới hạn đã quan sát.
