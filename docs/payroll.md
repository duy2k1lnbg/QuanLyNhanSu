# Chấm công và tính lương

Cập nhật nội dung: 01/10/2026.

Tài liệu mô tả luồng, dữ liệu và các bước đối soát. Giá trị chính sách cần được xác nhận cho kỳ áp dụng; phần mô tả mã nguồn không thay kết quả kiểm thử hoặc xác nhận nghiệp vụ.

## Luồng tính và đối soát

### Phạm vi

Tài liệu giải thích luồng trong mã nguồn. Các mức tiền, tỷ lệ bảo hiểm, biểu thuế và điều kiện áp dụng cần lấy từ chính sách được xác nhận cho kỳ tính. Không dùng ví dụ cũ trong báo cáo để thay chính sách của đơn vị.

### Luồng chính

1. Chuẩn bị kỳ công, hồ sơ nhân viên, hợp đồng và chính sách có ngày hiệu lực.
2. Kiểm tra lịch, giờ vào/ra, nghỉ phép và tăng ca đã duyệt.
3. Phân đoạn thời gian và xử lý bất thường; công bố kết quả đủ điều kiện.
4. `BANGLUONG.TinhLuongKyCong` gọi engine lương để tính các thành phần.
5. Engine tính theo từng nhân viên, lưu chi tiết/nguồn cùng thông tin lượt tính.
6. Đối soát, chốt số liệu và xác nhận chi trả theo quy trình được kiểm soát.

Việc khóa kỳ và xác nhận chi trả là hai việc khác nhau. Mã hiện tại còn chỗ lọc `APPROVED` như đã chi trả; không suy trạng thái thanh toán chỉ từ nhãn trên UI.

### Các thành phần

- Thu nhập: lương theo công, nghỉ hưởng lương, làm đêm, tăng ca, phụ cấp và các khoản thưởng được tính vào kỳ.
- Khoản khấu trừ: phần bảo hiểm/công đoàn của người lao động, thuế, tạm ứng và khoản khác được xác định rõ.
- Chi phí doanh nghiệp: có thể gồm các khoản đóng của người sử dụng lao động; không gộp tất cả vào tiền trừ của nhân viên.

Ở mức diễn giải, thực lĩnh bằng tổng thu nhập được đưa vào kỳ trừ các khoản khấu trừ của người lao động, cộng khoản hoàn/điều chỉnh nếu có. Đây không thay cho công thức chi tiết của từng loại khoản.

### Nguồn tham khảo

| Phần | Mã nguồn |
| --- | --- |
| Điều phối | [PayrollEngine](../HRMS.Business/CLASS_PAYROLL/PayrollEngine.cs) |
| Chính sách | [PolicyResolver](../HRMS.Business/CLASS_PAYROLL/PolicyResolver.cs) |
| Hồ sơ theo ngày hiệu lực | [EmployeeProfileResolver](../HRMS.Business/CLASS_PAYROLL/EmployeeProfileResolver.cs) |
| Các khoản | [InsuranceEngine](../HRMS.Business/CLASS_PAYROLL/InsuranceEngine.cs), [TaxEngine](../HRMS.Business/CLASS_PAYROLL/TaxEngine.cs), [UnionEngine](../HRMS.Business/CLASS_PAYROLL/UnionEngine.cs), [OtComplianceEngine](../HRMS.Business/CLASS_PAYROLL/OtComplianceEngine.cs) |
| Chi tiết/API | [BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs) |

### Đối soát trước khi chốt

- Kỳ công và nhân viên nằm trong phạm vi cần tính; công đầu vào đã công bố và không có bất thường chặn chốt.
- Ngày hiệu lực hợp đồng/chính sách và hồ sơ bảo hiểm/thuế/phụ thuộc phù hợp với kỳ.
- Các khoản nghỉ phép, làm đêm và tăng ca không bị tính trùng.
- Tạm ứng đã hủy/xóa mềm không tiếp tục khấu trừ ngoài quy tắc được xác định.
- Tổng các dòng chi tiết khớp tổng thu nhập, khấu trừ và thực lĩnh.
- Lượt tính có trạng thái rõ, nguồn và lỗi được lưu để giải thích kết quả.

Kiểm tra một vài trường hợp biên trước khi tính cả kỳ. Các báo cáo cũ không đủ để xác nhận toàn bộ kết quả hiện tại; xem [test report](testing.md) và [hiện trạng](current-status.md).

## Dữ liệu đầu vào và kết quả

### Phạm vi và đơn vị

Danh mục dưới đây được đối chiếu với model, resolver và migration trong repository. Chưa kiểm tra lại schema đang chạy. Đơn vị của từng cột phải kiểm tra cùng DTO và phép tính; không mặc định mọi giá trị số là tiền.

| Nhóm | Bảng hoặc nguồn | Vai trò |
| --- | --- | --- |
| Kỳ và đầu vào công | `TB_KYCONG`, `TB_BANGCONG`, `TB_BANGCONG_CHITIET` | Kỳ, trạng thái khóa, dữ liệu công và revision |
| Công đã xử lý | `TB_CHAMCONG_LANTINH`, `TB_CHAMCONG_KQ_NGAY`, `TB_CONG_PHANDOAN`, `TB_CONG_PD_NGUON` | Lượt tính, kết quả ngày, phân đoạn và nguồn |
| Tổng lương | `TB_BANGLUONG` | Thành phần/tổng tiền theo nhân viên và kỳ |
| Chi tiết và nguồn | `TB_BANGLUONG_CT`, `TB_BANGLUONG_CT_SOURCE` | Dòng khoản lương và dữ liệu dùng để tính |
| Lượt tính | `TB_PAYROLL_CALCULATION_RUN` | Trạng thái, số lượng và lỗi lượt tính |
| Chính sách | `TB_CHINH_SACH_LUONG`, `TB_CHINH_SACH_BHXH`, `TB_CHINH_SACH_BHXH_VUNG`, `TB_CHINH_SACH_CONG_DOAN`, `TB_THUE_TNCN_CHINH_SACH`, `TB_THUE_TNCN_BAC` | Chính sách theo hiệu lực và bậc thuế |
| Hồ sơ cá nhân | `TB_NHANVIEN_BAOHIEM_THAM_GIA`, `TB_NHANVIEN_CONG_DOAN_THAM_GIA`, `TB_NHANVIEN_THUE`, `TB_NGUOI_PHU_THUOC` | Trạng thái tham gia và căn cứ theo nhân viên |
| Dấu vết tính khoản | `TB_BANGLUONG_BAOHIEM`, `TB_BANGLUONG_CONG_DOAN`, `TB_BANGLUONG_THUE_CT`, `TB_BANGLUONG_OT_COMPLIANCE` | Kết quả thành phần và thông tin giải thích |
| Quyết toán năm | `TB_QUYET_TOAN_THUE_NAM`, `TB_QUYET_TOAN_THUE_NAM_CT` | Tổng hợp quyết toán và chi tiết |
| Chi trả dự kiến trong V1_20 | Cột chi trả và `TB_PAYROLL_PAYMENT_LOG` | Migration đang ghi là draft; chưa xác nhận áp dụng |

### Những trường nên đọc cùng nhau

- `MAKYCONG`, `MANV`, `IDBL`: khóa nhận diện trong luồng; ràng buộc thực tế cần kiểm tra schema.
- `KHOA`: khóa kỳ, không tự đồng nghĩa đã trả tiền.
- `CONG_INPUT_REV`, `CONG_PUBLISH_REV`: phiên bản công đầu vào/công bố.
- `TRANG_THAI`, `IS_LEGACY`: trạng thái tính và dấu dữ liệu lịch sử; không suy quyền chỉnh sửa chỉ từ một trường.
- Tổng thu nhập, khấu trừ, `THUC_LINH`: đối chiếu với các dòng chi tiết thay vì chỉ xem số tổng.

Xem [PolicyResolver](../HRMS.Business/CLASS_PAYROLL/PolicyResolver.cs), [PayrollModels](../HRMS.Business/CLASS_PAYROLL/PayrollModels.cs) và [ma trận model](model-fields.md). Một phần dữ liệu được đọc bằng SQL trực tiếp; EDMX có thể không biểu diễn hết các cột dùng trong service.

## Trạng thái và quyền

### Trình tự nghiệp vụ

Chuẩn bị hồ sơ/chính sách → kiểm tra và công bố công → tính lương → đối soát → khóa kỳ → xác nhận chi trả. Trình tự này dùng để rà soát; các bước duyệt/chứng từ chi tiết cần xác định theo quy trình của đơn vị.

### Trạng thái hiện có và giới hạn

- `KHOA` khóa kỳ; không nên dùng nó làm bằng chứng đã chi trả.
- `TRANG_THAI` của lương thể hiện các trạng thái tính/duyệt trong luồng hiện tại.
- V1_20 đề xuất trạng thái và audit chi trả riêng, nhưng script vẫn ghi draft.
- Bộ lọc `paid` trong API còn nhận `APPROVED`. Chưa xác nhận toàn bộ luồng chuyển trạng thái thống nhất.

### Quyền cần kiểm tra tại server

| Thao tác | Hiện trạng từ mã | Việc cần đối chiếu |
| --- | --- | --- |
| Xem danh sách/chi tiết lương | Controller yêu cầu đăng nhập; danh sách chưa kiểm tra quyền xem riêng hoặc phạm vi công ty | Quyền xem lương, phạm vi đơn vị và dữ liệu cá nhân |
| Tính lương | Có `F_BANGLUONG_CALC` tại action | Người được tính, kỳ/phạm vi được xử lý |
| Khóa/mở khóa kỳ | Action chưa có kiểm tra quyền riêng ngoài đăng nhập | Quyền khóa/mở, lý do và audit |
| Xem phiếu lương cá nhân | Qua `/me` và mapping nhân viên | Token A không đọc dữ liệu B |
| Xác nhận chi trả | Chưa xác minh luồng hoàn chỉnh | Người xác nhận, chứng từ, chống lặp và điều chỉnh |

Đây là danh sách cần đối chiếu, không phải ma trận RBAC đã được triển khai đầy đủ. Quyền `VIEW/ADD/EDIT/DELETE/PRINT` ở giao diện không thay quyền ở endpoint.

Nguồn: [BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs), [JwtAuthorizeAttribute](../HRMS.Api/Filters/JwtAuthorizeAttribute.cs), [MeController](../HRMS.Api/Controllers/MeController.cs), [KYCONG](../HRMS.Business/CLASS_CHAMCONG/KYCONG.cs).

### Bằng chứng cần lưu

Tài khoản/vai trò thử, scope, request, mã HTTP, trạng thái trước/sau, người thao tác và log. Kiểm tra trực tiếp API bằng tài khoản không có quyền, không chỉ bấm nút trên UI. Xem [hiện trạng](current-status.md).

## Chính sách cần xác nhận

### Phạm vi

Tài liệu này là bảng công việc để đối chiếu cấu hình/phép tính với chính sách áp dụng. Không xác nhận các mức tiền, tỷ lệ hoặc văn bản pháp luật trong bản cũ còn hiệu lực. Đợt cập nhật này không rà soát pháp lý và không chạy lại tính lương trên database.

Các tham chiếu pháp lý và giá trị cố định trong bản trước không được dùng làm mặc định triển khai khi chưa có người phụ trách nghiệp vụ xác nhận nguồn và ngày hiệu lực.

### Bảng đối chiếu cần hoàn thành

| Nội dung | Nguồn triển khai | Thông tin cần xác nhận | Hiện trạng xác minh |
| --- | --- | --- | --- |
| Công chuẩn, giờ chuẩn và làm đêm | `PolicyResolver`, `PayrollEngine`, chính sách lương | Cách trả lương, lịch, đơn vị và ngày hiệu lực | Chưa đối chiếu chính sách đơn vị |
| Làm thêm giờ | `OtComplianceEngine`, hệ số tăng ca và dữ liệu duyệt | Loại ngày/giờ, điều kiện áp dụng và giới hạn | Chưa xác nhận nguồn pháp lý hiện hành |
| Bảo hiểm | `InsuranceEngine`, bảng chính sách/hồ sơ tham gia | Đối tượng, căn cứ, vùng, trần và tỷ lệ | Chưa xác nhận theo kỳ tính |
| Thuế theo tháng | `TaxEngine`, chính sách và bậc thuế | Cư trú, giảm trừ, khoản miễn/không miễn và hiệu lực | Chưa xác nhận theo kỳ tính |
| Người phụ thuộc/quyết toán | `AnnualTaxFinalizationService` | Số tháng đủ điều kiện, nguồn chứng từ và số đã khấu trừ | Cần fixture theo trường hợp |
| Công đoàn | `UnionEngine` và hồ sơ tham gia | Khoản của nhân viên/doanh nghiệp, căn cứ và hiệu lực | Chưa đối chiếu chính sách áp dụng |
| Thưởng, kỷ luật và khấu trừ | `PayrollEngine` và dữ liệu quyết định | Khoản được đưa vào kỳ, căn cứ và cách xử lý | Cần người phụ trách xác nhận |
| Chi trả | API, trạng thái kỳ/lương và V1_20 | Chứng từ, người được xác nhận, ngày và trạng thái | Chưa xác minh luồng hoàn chỉnh |

### Cách bổ sung bằng chứng

Mỗi chính sách cần ghi nguồn chính thức, ngày ban hành/hiệu lực, đối tượng áp dụng, người xác nhận, giá trị cấu hình và test fixture. Nếu có nhiều chính sách cùng thời kỳ, ghi quy tắc chọn. Không đánh dấu tuân thủ chỉ vì engine có một nhánh xử lý hoặc test dùng giá trị giả lập đạt.

Nguồn mã: [CLASS_PAYROLL](../HRMS.Business/CLASS_PAYROLL). Xem thêm [từ điển dữ liệu](payroll.md), [luồng tính](payroll.md) và [kế hoạch sửa dữ liệu](database.md).

## Đối chiếu API và giao diện

### Cách sử dụng

Danh sách dùng để kiểm tra các nhóm dữ liệu cần đọc/giải thích trên giao diện. Không yêu cầu mọi trường kỹ thuật nằm trên lưới chính; drawer, phiếu lương hoặc trace có thể phù hợp hơn. Việc trường tồn tại trong DTO chưa chứng minh dữ liệu đúng hoặc tài khoản có quyền đọc.

### Các nhóm cần đối chiếu

| Nhóm | Nguồn cần kiểm tra | Kiểm tra cần thực hiện |
| --- | --- | --- |
| Nhận diện và kỳ | Model/DTO, danh sách API | Nhân viên, kỳ và trạng thái đúng |
| Công/lương thời gian | Công công bố và engine | Đơn vị, công chuẩn/thực tế, nghỉ hưởng lương |
| Thu nhập thêm | Chi tiết phụ cấp, OT, làm đêm, thưởng | Phân loại, giá trị và nguồn, không trùng |
| Khấu trừ | Bảo hiểm/công đoàn/thuế/tạm ứng | Khoản của người lao động, căn cứ và hiệu lực |
| Tổng tiền | Tổng thu nhập, khấu trừ, thực lĩnh | Tổng khớp các dòng chi tiết |
| Chi phí doanh nghiệp | Kết quả phần người sử dụng lao động | Không hiển thị thành khoản trừ nhân viên |
| Chính sách và trace | Snapshot/trace/nguồn | Có thể giải thích đầu vào được dùng |
| Chi trả | Trạng thái kỳ/lương và chứng từ | Không suy từ khóa kỳ hoặc `APPROVED` |
| Quyền | Endpoint và UI | Người không có quyền bị server từ chối |

### Nguồn mã

[BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs), [BANGLUONG_DTO](../HRMS.Business/DTO/BANGLUONG_DTO.cs), [PayrollModels](../HRMS.Business/CLASS_PAYROLL/PayrollModels.cs), [BangLuongPage](../HRMS.Web/src/pages/BangLuongPage.tsx), [FrmBangLuong](../HRMS.Desktop/FORM_CHAMCONG/FrmBangLuong.cs) và [PayrollScreen](../HRMS.Mobile/src/screens/Payroll/PayrollScreen.tsx).

### Giới hạn

Chưa thử round-trip từng trường hoặc mọi vai trò trên ba ứng dụng. Các kết quả coverage trong bảng cũ không được giữ như xác nhận hiện tại. [Danh mục model](model-fields.md) chỉ mô tả khai báo C#, [hiện trạng](current-status.md) nêu các điểm quyền/trạng thái còn mở.
