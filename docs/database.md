# Database: migration và dữ liệu kiểm thử

Cập nhật nội dung: 01/10/2026.

Hướng dẫn rà soát script và dữ liệu trong repository. Chưa truy vấn lại Oracle hoặc áp dụng migration trong đợt dọn tài liệu.

## Migration công–lương

### Phạm vi

Repository có migration từ V1_0 đến V1_20 và các script rollback/phụ trợ. Sự tồn tại của file không chứng minh migration đã chạy hoặc có thể chạy lại an toàn. Tài liệu này không yêu cầu thực thi SQL trên database đang dùng.

### Các nhóm thay đổi gần đây

| Phiên bản | Nội dung từ script |
| --- | --- |
| V1_15 | Chính sách, phiên và audit xác thực |
| V1_16 | Chính sách lương/bảo hiểm/thuế/công đoàn và chi tiết lương |
| V1_17 | Điều chỉnh khóa ngoại bảng công |
| V1_18 | Lịch làm việc, phiên bản ca, phân đoạn, bất thường và công bố công |
| V1_19 | Ràng buộc tính toàn vẹn công–lương |
| V1_20 | Theo dõi chi trả và audit; đầu file vẫn ghi là draft |

`apply_payroll_v1_16_objects.sql` là script phụ trợ. Cần đọc nội dung và phụ thuộc trước khi đưa vào danh sách chạy. Thứ tự không lấy từ cách sắp xếp tên file theo chữ cái: V1_10 không đứng trước V1_2 về phiên bản nghiệp vụ.

### Chuẩn bị

1. Ghi host/service/schema, phiên bản Oracle và công cụ SQL sẽ dùng. Không lưu mật khẩu vào báo cáo.
2. Kiểm tra bảng/cột/constraint/trigger và `SCHEMA_VERSION` thực tế. Nếu ghi nhận phiên bản thiếu, không tự suy ra đã áp dụng đủ.
3. Chụp metadata và backup; thử khôi phục vào schema riêng.
4. Chọn danh sách forward migration cần thiết và đối chiếu nguồn mã đi kèm.
5. Đánh dấu rollback, draft, seed, repair và cutover thành các nhóm riêng.

### Thử trên schema riêng

Đọc từng script, kiểm tra điều kiện schema (một số script yêu cầu `HR`), đặc quyền và tính chạy lại. Một schema đổi tên không tự phù hợp với SQL hard-code `HR`.

Chạy script từng bước với log đầy đủ. Đối chiếu error, metadata và dữ liệu sau mỗi bước. Không dùng một lệnh wildcard để chạy mọi `.sql`. Không giả định DDL Oracle được hoàn tác cùng transaction ứng dụng.

### Sau khi thử

- Build/chạy API với schema thử nghiệm.
- Kiểm tra chấm công, lương và những bất biến đã định nghĩa bằng fixture.
- Kiểm tra API/DTO dùng cột mới và xử lý dữ liệu lịch sử.
- Ghi script đã chạy, trạng thái, log và phương án khôi phục. Chỉ đánh dấu đã áp dụng khi có bằng chứng trên đích cụ thể.

### Các lưu ý của repository

- [Compose](../docker-compose.yml) mount chung [migration](../database/migrations) vào init directory, gồm cả rollback. Chưa nên dùng thư mục này nguyên trạng làm danh sách init.
- V1_20 cần được rà soát và thử riêng; tài liệu không xác nhận nó đã được áp dụng.
- Các script `synthetic200`/`realistic200` có thể thay đổi dữ liệu, tạo/cutover schema; chúng không thuộc nhiệm vụ dọn tài liệu.

Xem [hiện trạng](current-status.md). Không thực hiện migration chỉ để kiểm tra nội dung của README.

## Kiểm tra và sửa dữ liệu

### Mục tiêu

Xác định dữ liệu thiếu hoặc không nhất quán bằng truy vấn/fixture trước khi sửa. Đợt cập nhật tài liệu không chạy kế hoạch này và không xác nhận dữ liệu hiện tại cần thay toàn bộ.

### Các bước

1. Chốt phạm vi kỳ, nhân viên và schema; sao lưu và thử khôi phục.
2. Kiểm tra hợp đồng nối tiếp/chồng lấn, thời điểm vào/ra và trạng thái nhân viên.
3. Kiểm tra chính sách có hiệu lực, hồ sơ bảo hiểm/thuế/công đoàn và người phụ thuộc.
4. Đối chiếu dữ liệu công thô, công đã công bố, tăng ca/nghỉ phép đã duyệt và bất thường.
5. So sánh nguồn đầu vào với các dòng chi tiết lương và tổng tiền.
6. Lập danh sách từng bản ghi cần sửa, lý do, giá trị trước/sau và người xác nhận.
7. Thử sửa/tính lại trong schema riêng. Chỉ áp dụng vào đích được chọn sau khi có kết quả đối soát và cách phục hồi.

### Kiểm tra tối thiểu

| Trường hợp | Kết quả cần xác định trước |
| --- | --- |
| Tháng có lịch làm việc khác công chuẩn | Cách tính lương theo hợp đồng/chính sách của kỳ |
| Nghỉ hưởng lương và làm đêm | Thành phần nào được tính, tránh đưa cùng khoảng thời gian vào hai khoản |
| Không tham gia bảo hiểm/công đoàn | Căn cứ áp dụng theo hồ sơ và chính sách |
| Tạm ứng đã hủy/xóa mềm | Có bị đưa vào khấu trừ hay không |
| Chính sách/hồ sơ thiếu hoặc chồng lấn | Lỗi hoặc cách giải quyết được xác nhận, không tự tạo giá trị để tính tiếp |
| Kỳ đã khóa/đã trả | Quyền điều chỉnh, dấu vết và chứng từ cần có |

Ghi số bản ghi trước/sau và những chênh lệch được giải thích. Không đặt mục tiêu thay dữ liệu chỉ để tổng tiền trông hợp lý. Xem [luồng tính](payroll.md), [chính sách cần xác nhận](payroll.md) và [test report](testing.md).

## Bộ dữ liệu mô phỏng và file hỗ trợ

| Thư mục/file | Mục đích |
| --- | --- |
| [synthetic200](../database/synthetic200/README.md) | Script tạo schema UAT, seed, kiểm tra và chuyển dữ liệu; có thao tác ghi database |
| [schema-snapshot.json](../database/synthetic200/schema-snapshot.json) | Snapshot schema được lưu từ lần làm dữ liệu trước |
| [realistic200](../database/realistic200) | Script sinh/apply dữ liệu mô phỏng và kiểm tra metadata |
| [manifest](../database/realistic200/realistic200_manifest.json), [preview](../database/realistic200/preview_20_employees.json) | Đầu vào/ảnh chụp dữ liệu của bộ realistic200 |

Ghi chép synthetic200 ngày 28/09/2026 mô tả 200 nhân viên và các kỳ 07–09/2026, gồm trường hợp vào/nghỉ giữa tháng và công chờ xác minh. Đây là dữ liệu fixture lịch sử, chưa xác nhận lại số lượng trên database đang chạy. Kết quả PASS trong báo cáo cũ không thay kiểm tra hiện tại.

Backup JSON và metadata của realistic200 đã được chuyển từ `docs/backup_metadata` sang `database/realistic200/backup_metadata`. Thư mục này được Git bỏ qua. Hai script backup/verify đã được cập nhật đường dẫn; nội dung JSON được giữ nguyên. Không chạy script để kiểm tra việc di chuyển file.
