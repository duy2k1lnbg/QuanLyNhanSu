# Quy trình chuẩn bị và tính lương trên Desktop

Mở **Chấm công → Bảng lương**, chọn đúng kỳ, rồi bấm **Quy trình tính lương**. Bấm **Tính lương** cũng mở quy trình này trước. Kỳ trong cửa sổ được giữ cố định; muốn đổi kỳ hãy đóng cửa sổ và chọn lại ở Bảng lương.

Danh sách bên trái là các bước kiểm tra. Chọn bước có trạng thái **Cần xử lý**: phần bên phải cho biết hướng dẫn, nhân viên, ngày công và việc cần làm. Bước không có hồ sơ cần xử lý sẽ hiện thông báo thay cho bảng trống. Bấm **Mở nơi xử lý** để mở màn hình tương ứng. Hoàn tất thao tác, đóng màn hình con rồi bấm **Kiểm tra lại**. Các form tải dữ liệu trên luồng nền; chỉ cửa sổ đang thao tác bị khóa tạm, các cửa sổ nghiệp vụ khác vẫn sử dụng được.

## Các bước và nơi thực hiện

| Bước | Điều kiện cần có | Nơi bổ sung / xử lý |
|---|---|---|
| 1. Kỳ công | Kỳ tồn tại, chưa khóa | Bảng công: tạo kỳ đúng năm/tháng. Kỳ đã khóa cần xử lý theo quyền và quy trình mở khóa hiện có. |
| 2. Chính sách | Đúng một phiên bản có hiệu lực: lương, BHXH, công đoàn, thuế; đủ mức vùng và bậc thuế | Mở nơi xử lý → Chuẩn bị dữ liệu tính lương. Chọn loại chính sách, mức vùng hoặc bậc thuế trong danh sách trên cùng. |
| 3. Hợp đồng | Nhân viên có hợp đồng trong kỳ, mức lương hợp lệ, không có hợp đồng chồng lấn | Hợp đồng lao động. Không tự tính lương cho nhân viên chưa có hợp đồng. |
| 4. Bảo hiểm | Một hồ sơ tham gia có hiệu lực cho mỗi nhân viên | Chuẩn bị dữ liệu → Hồ sơ bảo hiểm: khai báo vùng, từng loại tham gia và thời hạn. Không tham gia cũng cần khai báo Không. |
| 5. Công đoàn | Một hồ sơ có hiệu lực cho mỗi nhân viên | Chuẩn bị dữ liệu → Hồ sơ công đoàn: khai báo Có/Không đoàn viên và thời hạn. |
| 6. Thuế | Một hồ sơ có hiệu lực, thông tin cư trú và giảm trừ có căn cứ | Chuẩn bị dữ liệu → Hồ sơ thuế; **Thao tác khác → Người phụ thuộc** để quản lý người phụ thuộc. Không có người phụ thuộc là dữ liệu hợp lệ. |
| 7. Công và đơn | Có dữ liệu công; các ngày đủ điều kiện chốt; nguồn, phân ca và đơn đã xác minh | Chọn nhân viên/ngày và Mở nơi xử lý; menu **Thao tác khác** có Phân ca, Đơn nghỉ, Điều chỉnh giờ công, Xác minh bất thường và Đơn tăng ca để mở đúng nơi nhập. |
| 8. Công bố công | Công được tính từ nguồn mới nhất và công bố thành công | Công bố lại công. Công bố không tự phê duyệt đơn hoặc bỏ qua bất thường. |
| 9. Hệ số tăng ca | Mỗi khoảng tăng ca có đúng một hệ số hiệu lực theo loại ngày và ban ngày/ban đêm | Chuẩn bị dữ liệu → Hệ số trả lương tăng ca. Nếu có tăng ca, chính sách lương đầu kỳ cũng phải hợp lệ. |
| 10. Phát sinh | Rà soát thu nhập, khấu trừ, phụ cấp và ứng lương | Phát sinh lương: mặc định xem **Khoản phát sinh nhập riêng**, dùng Duyệt/Thu hồi/Xóa theo trạng thái và quyền. Chọn **Phụ cấp hợp đồng** hoặc **Ứng lương** rồi bấm **Quản lý phụ cấp/ứng lương** (hoặc nhấp đúp) để mở dữ liệu nguồn. Phụ cấp hợp đồng không có bước duyệt tại Phát sinh lương; không sao chép nó thành khoản nhập riêng vì sẽ tính trùng tiền. Menu **Thao tác khác** cũng có Phụ cấp và Ứng lương. |

Nếu bảng/cột không tồn tại hoặc tài khoản kết nối không có quyền đọc, quy trình sẽ chặn và báo mã đối chiếu. Mở Cấu hình cơ sở dữ liệu để kiểm tra đúng kết nối; DBA xác minh schema và quyền trước khi áp dụng migration thích hợp. Đây không phải lỗi có thể sửa bằng cách tự tạo một hồ sơ nhân viên.

## Nhập dữ liệu ở màn hình mới

1. Chọn loại dữ liệu trong danh sách trên cùng.
2. Chọn **Thêm bản mới**, hoặc chọn dòng rồi **Sửa dòng đã chọn**.
3. Điền các trường có dấu `*`. Danh sách nhân viên, ca, quy định và chính sách có ô chọn, không yêu cầu nhập SQL hoặc nhớ mã nội bộ.
4. Nhập lý do/căn cứ lưu. Bấm **Kiểm tra dữ liệu** để kiểm tra ràng buộc trong một giao dịch được rollback. Bước này không lưu dữ liệu nghiệp vụ, có thể làm tăng số thứ tự sequence của Oracle.
5. Bấm **Lưu dữ liệu** khi đã đối chiếu đúng. Với đơn yêu cầu, lưu mới tạo trạng thái Chờ duyệt; chọn dòng và nhập căn cứ để **Phê duyệt** hoặc **Từ chối**.

Tỷ lệ nhập dạng `0,08` tương ứng `8%` khi máy dùng dấu phẩy thập phân. Không nhập `8` vào trường tỷ lệ. Ngày kết thúc chính sách/hồ sơ lương tính bao gồm ngày đó; ngày kết thúc phiên bản ca/quy định công là mốc ngừng hiệu lực, không bao gồm mốc này. Ca qua đêm phải có ngày kết thúc kế tiếp; khung giờ của ca dùng phút tính từ 00:00 ngày công, qua đêm có thể lớn hơn 1440.

Phiên bản chính sách/hồ sơ đã được sử dụng để tính lương được bảo vệ. Kết thúc hiệu lực sau kỳ đã sử dụng rồi thêm phiên bản mới. Ca/quy định đã công bố không sửa đè; tạo bản Nháp, thêm đủ khung giờ, sau đó Công bố. Phân ca thay đổi tạo phiên bản thay thế, giữ lịch cũ để đối chiếu.

## Xử lý các ngày công đang chặn tính lương

| Tình trạng | Cách xử lý |
|---|---|
| Vắng chưa xác nhận | Đối chiếu thực tế. Nghỉ thật: lập đơn nghỉ với khoảng nghỉ, loại hưởng công và nguồn chi trả, sau đó phê duyệt. Quên quẹt thẻ: lập đơn Điều chỉnh giờ công có căn cứ. Không tự chuyển vắng thành đủ công. |
| Thiếu giờ vào / ra | Lập hoặc sửa đơn Điều chỉnh giờ công, nhập cả ngày và giờ chính xác; người có quyền phê duyệt đối chiếu trước khi duyệt. |
| Quẹt ngoài lịch | Nếu làm việc thực tế: sửa phân ca/ngày nghỉ cho đúng và đối chiếu đơn tăng ca. Nếu khoảng đó không tính công: dùng Xác minh bất thường, ghi căn cứ và xác nhận Không tính. |
| Tăng ca chưa duyệt / vượt giờ duyệt | Mở Đơn tăng ca, nhập đúng khoảng bắt đầu/kết thúc và số giờ. Nếu đã có đơn cùng người/ngày/ca, sửa đơn còn chờ hoặc xử lý đơn hiện có. Không tạo đơn trùng. |
| Thiếu cấu hình | Bổ sung lịch, chọn ca và quy định đã công bố còn hiệu lực. Nếu chưa có ca phù hợp, thêm phiên bản ca và các khung giờ tại danh sách dữ liệu. |

**Xác minh Không tính** chỉ áp dụng hai loại khoảng ngoài lịch / tăng ca chưa duyệt, giữ nguyên giờ quẹt thẻ và các số giây đã được hưởng công. Các loại lỗi khác cần sửa nguồn hoặc đơn. Mỗi quyết định ghi người thực hiện, căn cứ và lịch sử; dữ liệu nguồn thay đổi làm quyết định cũ không còn áp dụng. Trường hợp có nhiều lỗi trong một ngày phải xử lý đủ tất cả.

Với dữ liệu công được công bố từ bản chương trình cũ: bấm **Công bố lại công** trước khi dùng Xác minh Không tính để tạo chứng cứ nguồn theo định dạng mới. Sau khi sửa nguồn hoặc xác minh, công bố lại lần nữa rồi kiểm tra số ngày còn bị chặn. Công bố và khóa kỳ là hai thao tác khác nhau.

## Phân quyền

- Bảng lương: cần quyền Xem để mở quy trình, quyền Sửa để tính lương.
- Phê duyệt yêu cầu: cần Xem để tra cứu; Thêm để lập đơn; Sửa để sửa đơn chờ, phê duyệt/từ chối hoặc xác minh bất thường. Không tự duyệt đơn/xác minh ngày công của chính nhân viên liên kết tài khoản.
- Bảng công: cần Xem để mở dữ liệu, Thêm/Sửa tương ứng để phân ca; Sửa để công bố lại công.
- Hồ sơ và chính sách lương: dùng quyền Xem/Thêm/Sửa của Bảng lương. Hợp đồng, phụ cấp, ứng lương và cấu hình database dùng quyền nghiệp vụ tương ứng.
- Tên ADMIN không tự cấp quyền. Nếu nút bị khóa do quyền, quản trị viên phải cấp đúng quyền của kênh Desktop, sau đó tải lại quyền/đăng nhập lại theo chức năng hiện có.

## Tính và đối chiếu kết quả

Khi không còn bước bị chặn, bấm **Tiếp tục tính lương**. Hệ thống kiểm tra lại đầu vào trước khi chạy và kiểm tra nguồn/chính sách/hồ sơ/hợp đồng trước khi ghi trong giao dịch. Nếu dữ liệu thay đổi hoặc có lỗi thì không lưu một phần kết quả kỳ lương.

Sau khi tính thành công, mở **Chi tiết lương**, đối chiếu công, tăng ca, thu nhập, bảo hiểm, công đoàn, thuế, tạm ứng và thực lĩnh trước khi sử dụng bảng lương cho phê duyệt/chi trả theo quy trình công ty. Kết quả Đã tính không có nghĩa Đã chi trả. Hướng dẫn này không bổ sung một module chi trả mới.

## Hiện trạng dữ liệu demo ngày 11/10/2026

Kỳ `202609`: có 200 nhân viên đủ hợp đồng, chính sách và ba nhóm hồ sơ đã đủ; còn **394 ngày công** chưa đủ điều kiện. Một ngày có thể có nhiều bất thường, nên số dòng bất thường có thể lớn hơn 394. Cần người dùng đối chiếu và xử lý các ngày này, không bỏ qua bằng cách tăng timeout hoặc ép cờ đủ điều kiện.

Kỳ `202610`: kiểm tra đầu vào đã đạt trên dữ liệu demo hiện tại. Các giao dịch kiểm thử nhập liệu được rollback; không tự phê duyệt các đơn tháng 9 và không sửa giờ quẹt thẻ. Bản triển khai trong lượt này là Desktop.


## Khóa và mở khóa kỳ công

Tại **Bảng công**, chọn kỳ rồi bấm **Khóa kỳ công** trên thanh công cụ. Ô **Đã khóa** chỉ hiển thị trạng thái; không chỉnh bằng nút Lưu. Cần quyền sửa Bảng công, có bảng công chi tiết đã công bố, revision đầu vào còn khớp và không còn bất thường chặn chốt. Khóa kỳ không tự công bố công hoặc thay ngày công bố.

Kỳ đã khóa sẽ hiện nút **Mở khóa kỳ công**; phải có quyền sửa và nhập lý do. Thay đổi trạng thái và nhật ký được ghi trong cùng giao dịch. Hai thao tác đọc/ghi dữ liệu trên luồng nền, chỉ form đang thao tác bận tạm thời.
