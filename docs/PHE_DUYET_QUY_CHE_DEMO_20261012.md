# Phê duyệt bộ tài liệu nội bộ cho HRMS demo — 12/10/2026

Yêu cầu phê duyệt của chủ sở hữu: “phê duyệt tất cả và nạp nó vào `Qdrant` DB”.

Phạm vi: công ty mã `1` trong hệ thống demo. 21 tài liệu, 83 điều khoản nội bộ; cùng 26 điều khoản pháp luật đủ điều kiện, tổng cộng 109 điểm tri thức. Hiệu lực nội bộ demo từ 12/10/2026, phiên bản `2.0.0-demo`.

Người phê duyệt được ghi nhận là chủ sở hữu hệ thống theo yêu cầu trực tiếp trong cuộc trò chuyện. Không giả lập chữ ký Tổng Giám đốc, quyết định đăng ký nội quy với cơ quan nhà nước hoặc địa chỉ nhà máy thực tế.

Quyền đọc: `PUBLIC_INTERNAL`, phạm vi công ty mã 1; mọi truy vấn vẫn phải qua xác thực, capability, tenant và ngày hiệu lực. Tài liệu công ty không mang tenant GLOBAL.

Các định mức vận hành được hoàn thiện cho demo: sửa công trong 48 giờ, cảnh báo quá 3 lần/tháng; bàn giao 10 phút được trả công; OT báo trước 4 giờ ngày thường/24 giờ ngày nghỉ; trả lương ngày 10 tháng kế tiếp. Đây là nội dung quy chế demo, không tự thay đổi cấu hình nghiệp vụ Oracle.

Thưởng, phụ cấp, mức đóng và số tiền cá nhân lấy theo hợp đồng/chính sách/quyết định đã duyệt và còn hiệu lực. Không tự tạo mức thưởng, hotline, email, tuyến xe hoặc xác nhận dịch vụ ngoài đời thực. Địa bàn lương tối thiểu vẫn phải được xác định theo địa chỉ thực tế.

Biểu mẫu có ô nhập thông tin cá nhân là mẫu trống, không phải hồ sơ nhân viên đã hoàn thành. Hướng dẫn chốt công/tính lương được cập nhật theo luồng Công bố lương và khóa bất biến hiện tại. Công bố phiếu lương không có nghĩa đã giải ngân ngân hàng hoặc gửi email.

Không nâng cấp tài liệu benchmark của doanh nghiệp khác thành quy chế công ty. PL-04 vẫn chưa nạp do chưa xác định địa bàn. Không thay đổi quyền người dùng hoặc collection nhân sự trong lượt này.

Bản gốc trước chỉnh sửa: `D:/QL_NS/artifacts/policy-approval-20261012/before/DB_Doc`.
Hồ sơ phê duyệt và các lựa chọn chi tiết: `D:/QL_NS/artifacts/policy-approval-20261012/approval-audit.json`.
Receipt nạp và công bố: `D:/QL_NS/artifacts/policy-approval-20261012/ingestion-approved.json`.

## Danh sách phê duyệt

### CT-01

Chế độ áp dụng trong demo là theo ngày. Lịch phân ca phải bảo đảm nghỉ chuyển ca, nghỉ tuần và giới hạn làm thêm; ca 12 giờ không có nghĩa toàn bộ 12 giờ là giờ làm việc bình thường.

- áp dụng chế độ ngày: 08 giờ làm việc bình thường/ngày, không quá 48 giờ/tuần; thời gian vượt 08 giờ/ngày là làm thêm giờ và phải được duyệt
- 10 phút; thời gian bàn giao là thời gian làm việc, được ghi nhận và trả lương, không yêu cầu làm việc ngoài giờ không trả công

### CT-02

Chỉ sử dụng dữ liệu thiết bị đã được HR xác nhận; demo hỗ trợ dữ liệu quẹt thẻ/vân tay/khuôn mặt theo cấu hình. Đơn sửa công phải được quản lý/HR duyệt trước khi áp dụng. Không được sửa công hoặc mở khóa kỳ đã công bố lương.

- 48 giờ kể từ khi phát sinh ca làm việc; trường hợp có lý do chính đáng nộp muộn phải được HR xác minh
- 03 lần/tháng để cảnh báo và hỗ trợ; không được tự động coi là vắng mặt hay cắt tiền công thực tế đã làm

### CT-03

OT khẩn cấp vẫn phải có sự đồng thuận và phê duyệt của quản lý được phân quyền cùng HR trước khi thực hiện; không tự duyệt. Lịch trả công thực tế phải được xác minh, không dùng trần đăng ký để cắt giờ thực tế đã làm.

- 04 giờ đối với ngày thường và 24 giờ đối với ngày nghỉ tuần
- 02 giờ trở lên

### CT-04

Giới hạn vận hành demo là 04 lần đổi ca/tháng để quản lý lịch, HR có thể xem xét ngoại lệ có lý do; mọi lần đổi phải bảo đảm lịch và nghỉ chuyển ca. Nghỉ đột xuất/khẩn cấp được thông báo sớm nhất có thể để HR xử lý, không bị từ chối tự động chỉ vì thiếu thời gian báo trước.

- 24 giờ / 01 ngày làm việc
- 03 ngày làm việc
- 07 ngày làm việc
- được chuyển tối đa 05 ngày sang quý I năm sau; phần vượt mức phải được HR và người lao động thống nhất lịch nghỉ, không tự động xóa quyền nghỉ còn lại
- 12 giờ/ngày
- 12 giờ
- theo quyết định phúc lợi từng dịp đã được duyệt; chưa có quyết định thì chưa có mức tiền mặc định

### CT-05

Địa bàn lương tối thiểu được xác định từ địa chỉ nơi làm việc và chính sách vùng còn hiệu lực trong Cấu hình lương, không suy ra chỉ từ tên tỉnh Bắc Ninh. Hồ sơ demo chưa xác nhận xã/phường thật nên không chốt một vùng cho mọi nhân viên. Phiếu lương được xem trong HRMS sau khi Công bố lương; gửi email và giải ngân ngân hàng là bước riêng, không được coi là đã thực hiện tự động.

- do người lao động lựa chọn và đăng ký; không bắt buộc một ngân hàng duy nhất
- ngày 10 của tháng kế tiếp
- 03 ngày làm việc; việc nộp sau mốc đối soát không làm mất quyền yêu cầu thanh toán tiền lương còn thiếu
- 02 ngày làm việc để phản hồi ban đầu

### CT-06

Hội đồng lương/HR xác nhận tiêu chí sát hạch theo vị trí và biên bản đánh giá trước khi duyệt quyết định cá nhân. Dữ liệu lương đã công bố được giữ nguyên; quyết định mới chỉ có hiệu lực cho kỳ tiếp theo theo ngày áp dụng.

- tháng 01 hàng năm
- Hoàn thành tốt trở lên
- 95% trở lên; các ngày nghỉ hưởng lương hợp pháp được tính theo chính sách đánh giá đã duyệt
- không có tỷ lệ tăng mặc định; chỉ áp dụng quyết định tăng lương cá nhân đã được duyệt và còn hiệu lực
- 01 năm/lần
- giá trị làm lợi được nghiệm thu bằng biên bản; không có ngưỡng tiền mặc định

### CT-07

Các khoản chuyên cần, thưởng và phụ cấp chỉ tính theo hợp đồng/chính sách/quyết định cá nhân còn hiệu lực đã được duyệt. Việc phê duyệt bộ quy chế này không tự tạo khoản thưởng hoặc tự cập nhật số tiền trong Oracle.

- không trừ chuyên cần chỉ vì người lao động sử dụng ngày phép năm đã được duyệt
- tra cứu khoản chuyên cần theo hợp đồng và chính sách lương đã duyệt, còn hiệu lực của từng nhân viên; chưa có căn cứ thì không có mức mặc định chung
- theo quyết định từng kỳ đã được Ban Giám đốc duyệt; không có mức mặc định
- mức tiền và tỷ lệ trích thưởng phải được duyệt cho từng sáng kiến, không có tỷ lệ mặc định
- công bố theo quyết định thưởng từng năm; không mặc định một tháng lương
- theo quyết định chi từng trường hợp đã được duyệt; không có định mức tiền mặc định
- theo thông báo phúc lợi từng dịp đã được duyệt

### CT-08

HR tổ chức hội đồng cho từng vụ việc gồm đại diện người sử dụng lao động, HR, tổ chức đại diện người lao động liên quan và người lao động/người bào chữa theo thủ tục áp dụng. Địa bàn lương tối thiểu phải được kiểm tra từ địa chỉ thực tế. Phê duyệt tài liệu trong demo không thay thế việc đăng ký nội quy hoặc các thủ tục xử lý kỷ luật ngoài đời thực.


### CT-09

Hành chính tiếp nhận đăng ký suất ăn, tuyến xe và chỗ ở; chỉ xác nhận dịch vụ sau khi có nhà cung cấp/danh sách đang hoạt động. Mô tả tiện ích trong quy chế demo không phải xác nhận nhà máy thật có xe buýt, ký túc xá hoặc trạm y tế. Khoản phụ cấp cá nhân tra cứu tại mục Phụ cấp nhân viên và hợp đồng.

- theo hợp đồng cung cấp suất ăn đã được duyệt; không có mức tiền mặc định
- 02 giờ trở lên
- theo thông báo tuyến và điểm đón do Hành chính công bố; hiện chưa có danh sách tuyến được xác nhận trong bộ dữ liệu này, cần liên hệ Hành chính
- tra cứu phụ cấp đi lại của nhân viên trong hợp đồng và mục Phụ cấp; không áp dụng mức tiền chung
- theo danh sách đăng ký và chỗ ở được Hành chính xác nhận; không có ngưỡng cự ly mặc định
- tra cứu khoản hỗ trợ nhà ở trong hợp đồng và mục Phụ cấp của nhân viên; không có mức tiền mặc định

### CT-10

Trong demo, kênh tiếp nhận đang có là nộp Mẫu BM-07 cho HR hoặc gặp trực tiếp HR; email, hotline và vị trí hòm thư vật lý chưa được cấu hình. Không tạo địa chỉ hoặc số điện thoại giả. Quyết định xử lý vi phạm phải có căn cứ pháp luật và đúng trình tự, không tự động sa thải chỉ vì bị tố cáo.

- chỉ tại khu vực có biển báo và nội quy bảo mật đã được công bố; không tự động áp dụng cho toàn bộ nơi làm việc
- chưa cấu hình trong môi trường demo; tiếp nhận trực tiếp tại Phòng Nhân sự hoặc bằng Mẫu BM-07
- chưa cấu hình trong môi trường demo; liên hệ trực tiếp Phòng Nhân sự

### BM-01

Giải trình trong 48 giờ, cảnh báo từ lần vượt 03 lần/tháng. HR vẫn xác minh công thực tế và lý do chính đáng. Không sửa dữ liệu kỳ đã công bố lương; khoản điều chỉnh được xử lý tại kỳ còn mở.

- 48 giờ
- 03 lần/tháng để cảnh báo, không tự động phủ nhận thời gian làm việc thực tế

### BM-02

Nghỉ 01 ngày báo trước 24 giờ; nghỉ 02-03 ngày báo trước 03 ngày làm việc; trên 03 ngày báo trước 07 ngày làm việc. Đổi ca báo trước 12 giờ, vận hành demo 04 lần/tháng; ngoại lệ khẩn cấp do HR xem xét.


### BM-03

Gửi trước 04 giờ với ngày thường, 24 giờ với ngày nghỉ tuần; quản lý được phân quyền và HR kiểm tra trước ca. Mọi giờ đã làm phải được đối soát thực tế; không tự động duyệt hoặc cắt giờ công.


### BM-04

Người đề nghị lập căn cứ giá trị làm lợi bằng biên bản, chi phí, sản lượng hoặc số liệu đo được. Quản lý xác nhận, HR và Tài chính thẩm tra, người có thẩm quyền hoặc được ủy quyền duyệt mức thưởng; không có hạn mức tiền mặc định.

- người đề nghị điền theo hồ sơ và căn cứ; đây là ô của biểu mẫu, không phải mức thưởng đã được ban hành

### BM-05

Mốc vận hành demo để gửi giải trình là 48 giờ sau khi nhận yêu cầu, có thể gia hạn vì lý do chính đáng; không làm mất quyền bào chữa hoặc quyền được tham gia phiên họp theo luật.


### BM-06

Thời gian bàn giao vận hành demo là 10 phút, thuộc thời gian làm việc được ghi nhận và trả lương. Ca sau chưa đến phải báo quản lý, không tự bỏ vị trí.


### BM-07

Nộp trực tiếp cho HR hoặc quản lý được phân công. Phản hồi ban đầu trong 07 ngày làm việc; chưa có email/hotline/hòm thư vật lý được cấu hình trong demo.

- 07 ngày làm việc để phản hồi ban đầu; vụ việc cần xác minh thêm phải thông báo tiến độ

### FAQ-01

Ngày trả lương vận hành demo là ngày 10 tháng kế tiếp, sớm hơn vào ngày làm việc liền kề nếu trùng ngày nghỉ. Mức chuyên cần và phúc lợi tra cứu theo hợp đồng/chính sách cá nhân đã duyệt; không có mức mặc định. Vùng lương tối thiểu phải tra theo địa chỉ nơi làm việc thực tế. AI phải nói rõ khi chưa có căn cứ về mức tiền hoặc địa bàn.

- 48 giờ
- 03 lần/tháng để cảnh báo và hỗ trợ, không tự động cắt công thực tế
- theo quyết định thưởng từng năm đã được duyệt
- tra cứu mức chuyên cần trong hợp đồng và chính sách lương của nhân viên; không có mức mặc định chung
- chưa cấu hình trong môi trường demo; dùng Mẫu BM-07 hoặc gặp trực tiếp Phòng Nhân sự
- chưa cấu hình trong môi trường demo; gặp trực tiếp Phòng Nhân sự

### HD-01

Mobile: vào Trang chủ để gửi đơn nghỉ phép, điều chỉnh công, tăng ca hoặc tạm ứng; vào Yêu cầu của tôi để theo dõi. Thông báo trong ứng dụng lấy dữ liệu từ backend. Đẩy thông báo khi đóng ứng dụng và email chưa được cấu hình trong demo, không cam kết đã hoạt động. Hồ sơ cá nhân/người phụ thuộc cần đúng quyền và luồng phê duyệt theo chức năng hiện hành.

- Quản trị viên IT/HR cấp tài khoản theo đúng nhân viên và quyền đã duyệt; người dùng đổi mật khẩu được cấp ngay khi nhận tài khoản, không dùng chung tài khoản

### SOP-01

Quy trình hiện hành: hoàn thiện hợp đồng, chính sách lương/BHXH/công đoàn/thuế/người phụ thuộc; duyệt các đơn và khoản phát sinh; xử lý bất thường; Tính toán & Công bố công; Tính lương để kiểm tra; Khóa kỳ công khi đủ điều kiện; tính lại nếu hệ thống yêu cầu cập nhật phiên bản; đối soát rồi Công bố lương. PC: Chấm Công → Bảng Lương → Công bố lương. Web: Bảng lương → Công bố lương. Sau công bố, lương và công của kỳ là bất biến, không sửa, thu hồi hay đổi trạng thái; điều chỉnh có căn cứ ở kỳ còn mở. Công bố phiếu lương không có nghĩa đã chuyển tiền ngân hàng.

- 02
- 02
- 03
- 03 - 05
- 06 - 07
- 08
- 09 - 10

### SOP-02

Hành chính chuẩn bị suất ăn phụ khi OT từ 02 giờ trở lên và xác nhận dịch vụ thực có. HR đối soát giờ thực tế với đăng ký; phần chênh lệch phải xác minh, không tự động cắt lương của thời gian đã làm.

- 04 giờ đối với ngày thường, 24 giờ đối với ngày nghỉ tuần

## Công bố và phục hồi

Nạp vào physical collection mới, kiểm tra toàn bộ ID/payload/hash/vector/index/model và truy xuất. Chỉ sau khi kiểm tra thành công mới đổi alias `hrms_knowledge_active` bằng một thao tác nguyên tử. Collection cũ được giữ lại.

Nếu cần phục hồi, dùng `previousCollection` trong receipt để chuyển alias về snapshot trước bằng API `/collections/aliases` (xóa mapping alias hiện tại và tạo mapping về snapshot cũ trong cùng một request). Không xóa collection mới/cũ tự động.

Bộ nạp: `tools/datadoc_ingestion_cli.py`, các bước `validate`, `apply`, `verify`, `publish`. Đối chiếu receipt để xác nhận thực tế đã công bố, không suy ra trạng thái từ tài liệu hướng dẫn.

Nguồn đối chiếu pháp luật hiện có: https://chinhphu.vn/?docid=215832&pageid=27160 và https://vbpl.vn/bolaodong/Pages/ivbpq-van-ban-goc.aspx?ItemID=139264. Phê duyệt demo không thay thế kiểm tra pháp lý và thủ tục đăng ký khi đưa vào doanh nghiệp thật.

## Kết quả công bố thực tế

Đã công bố lúc `2026-10-12T05:29:31.008933+00:00`. Alias `hrms_knowledge_active` trỏ tới `hrms_knowledge_bgem3_20261012T052439_051df452`; 109 điểm tri thức (83 nội bộ + 26 pháp luật), 297 điểm nhân sự được giữ nguyên.

Snapshot trước: `hrms_knowledge_bgem3_20261010T195635_1a9111c8`, được giữ lại. Năm câu hỏi ngữ nghĩa mẫu và kiểm tra tenant/ngày hiệu lực đều đạt; chưa thay thế kiểm thử toàn bộ giao diện chat.
