# Bổ sung cấu hình và hồ sơ demo tính lương — 11/10/2026

Đã áp dụng trực tiếp trên Oracle HR/orcl, không chỉ soạn script. Người dùng xác nhận dữ liệu mang tính demo nhưng cần gần thực tế.

## Kết quả tính lương thực tế

- Engine C# hiện tại tính thành công kỳ **202610: 195/195 nhân viên đủ hợp đồng**, run ID 1, SUCCESS, 0 lỗi.
- Đã lưu 195 bảng lương trạng thái CALCULATED và 639 dòng khoản mục chi tiết. Đây là kết quả tính, chưa duyệt/chốt/chi trả.
- Tổng thực lĩnh: **3.690.102.875 đồng**; thuế TNCN: **79.559.625 đồng**; bảo hiểm NLĐ: **361.260.000 đồng**.
- Công bố lại tháng 10 từ lịch, quẹt thẻ và đơn đã có qua AttendancePublishingService/TimeSegmentationEngine: 6.076/6.076 ngày sẵn sàng, 0 ngày lệch revision; input revision 11371, publish revision 3.
- 582 dòng lương lịch sử tháng 1–3 được giữ nguyên: tổng ID 861651, tổng thực lĩnh 10.707.408.907,67 đồng, checksum 1202980788383.

## Dữ liệu đã nạp

| Nhóm | Số bản ghi | Cách chọn dữ liệu |
|---|---:|---|
| Chính sách lương | 5 | Các kỳ 202601, 202602, 202603, 202609, 202610; công chuẩn lấy từ TB_KYCONG: 27/24/26/26/27; cấu hình ca chuẩn 8 giờ và phụ trội đêm 30% |
| Chính sách bảo hiểm | 2 | Hai khoảng không chồng lấn: 01/01–30/06 và 01/07–31/12/2026 |
| Mức lương tối thiểu | 8 | Đủ 4 vùng cho mỗi chính sách BH; địa điểm công ty Yên Trung, Yên Phong thuộc vùng II |
| Chính sách công đoàn | 1 | Kịch bản doanh nghiệp ngoài nhà nước: đoàn phí 0,5%, giới hạn 10% mức lương cơ sở; kinh phí DN 2% |
| Chính sách thuế | 1 | Năm 2026; giảm trừ bản thân 15,5 triệu/tháng, phụ thuộc 6,2 triệu/tháng |
| Bậc thuế | 10 | 5 bậc tháng và 5 bậc năm: 5/10/20/30/35%; ngưỡng tháng 10/30/60/100 triệu |
| Hồ sơ bảo hiểm | 200 | Dùng ngày cấp/mức đóng từ TB_BAOHIEM, vùng II; 196 người Việt Nam có BHTN, 4 người nước ngoài không có BHTN; không áp ưu đãi TNLĐ khi chưa có căn cứ |
| Hồ sơ công đoàn | 200 | 159 đoàn viên, 41 không đoàn viên; đây là tình huống demo đa dạng, không phải tình trạng đoàn viên đã được xác minh |
| Hồ sơ thuế | 200 | Kịch bản demo cá nhân cư trú; 196 định danh VN lấy từ CCCD đã có, 4 hồ sơ nước ngoài để trống MST; ủy quyền quyết toán = 0 |
| Người phụ thuộc | 57 | 48 nhân viên có 1 hoặc 2 người phụ thuộc mẫu, tên có tiền tố [DEMO], không tự sinh CCCD/MST của người phụ thuộc |

Hồ sơ mới có **DATA_ORIGIN=DEMO** và **SOURCE_NOTE** để phân biệt nguồn cũ và giả định mẫu. Không coi những cờ tham gia/cư trú/giảm trừ mẫu là hồ sơ đăng ký thật. Hồ sơ chính sách công đoàn dùng cơ sở lương đóng BHXH cho doanh nghiệp ngoài nhà nước; nếu thay loại hình công ty hoặc quy chế thu đoàn phí, cần cấu hình lại căn cứ tính tương ứng.

Một hợp đồng **“Nhân viên test 2201”** có lương 0 được đánh dấu DEL_DATE để không trộn fixture vào danh sách trả lương. Giữ nguyên nhân viên, hợp đồng và lịch sử công; không xóa dữ liệu.

## Tháng 9 còn chặn vì đầu vào công

Đã công bố lại tháng 9: input revision 130, publish revision 100, không còn lệch revision; **394/6.030 ngày vẫn cần xác minh**. Bất thường gắn với các ngày này gồm chấm ngoài lịch, tăng ca chưa duyệt và vắng mặt chưa có phép/giải trình. Các loại bất thường có thể trùng ngày, nên không cộng các số loại lỗi để suy ra số ngày.

Tháng 9 không còn thiếu chính sách hay hồ sơ bảo hiểm/công đoàn/thuế. Cần xử lý đúng lịch/đơn/giải trình rồi công bố lại trước khi tính kỳ 202609. Không đổi trực tiếp DU_DIEUKIEN_CHOT để ép qua bước này. Trong lượt này chưa tính bảng lương tháng 9.

## Kiểm chứng

- Cả 4 nhóm PolicyResolver.InspectAllPolicyGroups đều Ready tại 31/10/2026 và 30/09/2026.
- Chạy engine tính lương thật thành công tháng 10; đối chiếu độc lập bằng SQL: BHXH/BHYT, BHTN của người nước ngoài, đoàn phí theo tư cách đoàn viên và trần, kinh phí DN, 5 bậc thuế/giảm trừ, chi tiết khoản mục và checksum lịch sử đều PASS.
- Đối chiếu gross-to-net: 0 sai lệch; 0 dòng thiếu liên kết chính sách/hồ sơ.
- Chạy lại script nạp: thêm 0 hồ sơ, 0 hợp đồng bị đánh dấu thêm; các số lượng chính sách/người phụ thuộc/bảng lương không đổi.
- Toàn bộ SQL, Python, PowerShell, C# probe và tài liệu mới được lưu UTF-8.

## Sử dụng ngay

Đóng rồi mở lại Desktop nếu ứng dụng đã nạp cache chính sách lúc bảng còn rỗng. Mở Bảng lương, chọn **202610** và **Làm mới** để xem kết quả đã lưu. Kỳ 202610 có thể chạy Tính lương lại khi chưa khóa; không cần chạy lại migration V1_16 hoặc script có MST mẫu.

Chỉ có dữ liệu và script cấu hình mới trong lượt này; không sửa mã chạy của Desktop/API. Kết quả kiểm tra dùng bộ business assembly đã build ở lượt sửa lỗi trước.

## Nguồn chính thức đã đối chiếu

1. [Nghị định 293/2025/NĐ-CP và phụ lục địa bàn](https://xaydungchinhsach.chinhphu.vn/nghi-dinh-so-293-2025-nd-cp-quy-dinh-muc-luong-toi-thieu-doi-voi-nguoi-lao-dong-lam-viec-theo-hop-dong-lao-dong-119251110172808433.htm): mức tối thiểu vùng 2026, Yên Trung/Yên Phong vùng II.
2. [BHXH Việt Nam — điều chỉnh mức cơ sở từ 01/7/2026](https://baohiemxahoi.gov.vn/tintuc/Pages/chuyen-doi-so.aspx?CateID=52&ItemID=26780): tham chiếu 2,34 triệu trước tháng 7, 2,53 triệu từ tháng 7 theo NĐ 161/2026.
3. [Luật BHXH 2024 trên BHXH Việt Nam](https://baohiemxahoi.gov.vn/tintuc/Pages/linh-vuc-bao-hiem-y-te.aspx?CateID=0&ItemID=23352&OtItem=date): mức BHXH NLĐ 8%; DN 3% ốm đau/thai sản + 14% hưu trí/tử tuất.
4. [BHXH Việt Nam — tỷ lệ BHYT](https://baohiemxahoi.gov.vn/tintuc/pages/cai-cach-chinh-sach-bhxh.aspx?CateID=0&ItemID=27087&OtItem=date): NLĐ 1,5%, DN 3%.
5. [NĐ 374/2025 — mức BHTN](https://baochinhphu.vn/quy-dinh-moi-ve-muc-dong-va-huong-bao-hiem-that-nghiep-102260107171705469.htm): NLĐ và DN 1%.
6. [NĐ 58/2020 — TNLĐ/BNN](https://vbpl.vn/bolaodong/Pages/vbpq-toanvan.aspx?ItemID=142815): thông thường 0,5%; ưu đãi 0,3% chỉ khi có đủ căn cứ.
7. [Tổng Liên đoàn — Quyết định 61/QĐ-TLĐ](https://congdoanvietnam.vn/hoi-dap/giam-muc-dong-doan-phi-cong-doan-theo-quyet-dinh-moi-9520.tld) và [Luật Công đoàn](https://xaydungchinhsach.chinhphu.vn/toan-van-luat-cong-doan-119241217155547243.htm): phân biệt đoàn phí NLĐ với kinh phí DN.
8. [Cục Thuế — giảm trừ và áp dụng thuế năm 2026](https://cms.baochinhphu.vn/cuc-thue-luu-y-khi-quyet-toan-va-hoan-thue-tu-dong-102260305191148461.htm) và [biểu thuế 5 bậc](https://xaydungchinhsach.chinhphu.vn/chi-tiet-bieu-thue-thu-nhap-ca-nhan-luy-tien-tung-phan-119260327091407544.htm).
9. [Ngành Thuế — số định danh thay MST từ 01/7/2025](https://xaydungchinhsach.chinhphu.vn/so-dinh-danh-ca-nhan-thay-cho-ma-so-thue-tu-1-7-2025-thong-tin-moi-nhat-tu-nganh-thue-119250624135234807.htm): với dữ liệu thật phải đối chiếu đăng ký thuế/dân cư; việc dùng CCCD hiện tại trong bộ demo không xác nhận đối chiếu thật.
10. [Lương làm việc theo ca và ban đêm](https://baochinhphu.vn/lam-viec-theo-ca-tinh-luong-them-gio-the-nao-102292566.htm): phụ trội đêm ít nhất 30%.

## File đã lưu trong dự án

- database/migrations/V1_38__payroll_profile_source_metadata.sql: schema metadata nguồn, đã ghi SCHEMA_VERSION 1.38.
- database/configuration/2026/seed_payroll_policies_2026.sql: cấu hình tham số chính sách.
- database/configuration/2026/seed_payroll_demo_profiles_2026.sql: hồ sơ demo, yêu cầu CLIENT_IDENTIFIER=HRMS_DEMO_CONFIRMED, có kiểm tra schema/địa bàn và không ghi đè hồ sơ đã tồn tại.
- Log/SQL đối chiếu: calculate-202610-result.txt, verification-result.txt, payroll-assertions-result.txt, september-blockers-result.txt trong thư mục báo cáo này.
