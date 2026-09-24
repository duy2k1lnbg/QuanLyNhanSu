# BÁO CÁO TỔNG KẾT TRIỂN KHAI HỆ THỐNG TÍNH LƯƠNG SẢN XUẤT (PAYROLL PRODUCTION FINAL STATUS)

> **Dự án**: Quản Lý Nhân Sự (HRMS)  
> **Phạm vi**: Toàn bộ chu trình tính lương theo chuẩn sản xuất 2026 (End-to-End Production Payroll)  
> **Nguyên tắc**: `DATABASE → POLICY → PAYROLL ENGINE → SNAPSHOT/TRACE → API → DESKTOP/WEB/MOBILE UI → TEST → DOCUMENTATION`

---

## A. HIỆN TRẠNG TRƯỚC KHI THỰC HIỆN (BEFORE)

Trước khi thực hiện kiểm toán và tái cấu trúc, hệ thống gặp phải các vấn đề nghiêm trọng:
1. **Thiếu một Payroll Engine duy nhất**:
   - Tầng Desktop tự tính một kiểu, API tính một kiểu, Web tự tính bù trừ và Mobile tự nội suy bằng mock/fallback.
2. **Hard-coded Business Constants tràn lan**:
   - Tự động fallback ngày công chuẩn `?? 26`.
   - Nhân cứng hệ số ca đêm `1.30m`.
   - Hard-code tỷ lệ bảo hiểm `8%`, `1.5%`, `1%`, `10.5%`.
   - Tự suy đoán giảm trừ gia cảnh và tính thuế TNCN sai khác biểu lũy tiến 5 bậc năm 2026 của Luật 109/2025/QH15.
3. **Lỗ hổng chính sách BHXH**:
   - Hai chính sách BHXH H1 và H2 bị trùng lặp khoảng hiệu lực gây ra lỗi Ambiguous Policy.
   - Sai mức lương tối thiểu giờ Vùng III (19.900 đ thay vì 20.000 đ theo NĐ 293/2025/NĐ-CP).
4. **Phụ cấp bị gán ghép sai bản chất**:
   - 13 loại phụ cấp trong `TB_PHUCAP` bị áp đặt quy tắc cứng mà không theo danh mục thực tế của doanh nghiệp.
5. **Dữ liệu lịch sử có nguy cơ bị ghi đè**:
   - Kỳ tính lương cũ trước 2026 (tiêu biểu bản ghi `IDBL = 1934`) bị can thiệp khi recalculate.
6. **Frontend tự tính lương (Client-side calculation)**:
   - Web `BangLuongPage.tsx` tự cộng trừ tổng tiền bằng các fallback giả định `?? 10000000`, `?? 800000`, `* 0.03`.
   - Mobile `MeController.cs` bỏ sót khoản khấu trừ đoàn phí công đoàn trong tổng khấu trừ.

---

## B. DANH SÁCH CÁC TẬP TIN ĐÃ THAY ĐỔI (CHANGED FILES)

### 1. Database
- `database/migrations/V1_16__payroll_production_policies_and_itemized_details.sql` (1.300+ dòng: 19 bảng mới, 6 triggers, 19 sequences, partial migration guard, audit snapshot).

### 2. Business Logic
- `HRMS.Business/CLASS_CHAMCONG/BANGLUONG.cs` (Phân luồng kỳ công $\ge 202601$ sang Payroll Engine, enrichment snapshot).
- `HRMS.Business/CLASS_PAYROLL/PayrollEngine.cs` (Pipeline 35 bước tính toán trọn vẹn, tính 13 phụ cấp, OT, BHXH 2 chiều, đoàn phí, thuế TNCN 5 bậc, OT compliance, employer cost).
- `HRMS.Business/DTO/BANGLUONG_DTO.cs` (Bổ sung `ModernPayrollSnapshotDto` và các trường snapshot 2026).

### 3. API
- `HRMS.Api/Controllers/BangLuongController.cs` (Map toàn diện 48 trường dữ liệu, endpoint chi tiết, snapshot, trace, OT compliance).
- `HRMS.Api/Controllers/MeController.cs` (Chuẩn hóa `GetPayroll` cho mobile: Gross thật, khấu trừ đầy đủ đoàn phí, không hard-code 1.30m, bổ sung trace & policy).
- `HRMS.Api/Models/MobileDtos.cs` (Mở rộng `MobilePayrollDto` đầy đủ các trường chấm công, thuế, cách tính, chính sách).
- `HRMS.Api/HRMS.Api.csproj` (Sửa điều kiện target build tool).

### 4. Presentation (Desktop, Web, Mobile)
- `HRMS.Desktop/Reports/rptBaoCaoLuongNV.cs` (Cập nhật report bind chuẩn trường dữ liệu).
- `HRMS.Web/src/types/hrms.ts` (Bổ sung các trường snapshot 2026 vào `BangLuongDTO`).
- `HRMS.Web/src/pages/BangLuongPage.tsx` (Xóa bỏ toàn bộ fallback cứng, hiển thị đúng badge Legacy/Production).
- `HRMS.Web/src/components/PayrollDetailDrawer.tsx` (Hiển thị 16 sections chi tiết).
- `HRMS.Web/src/components/PhieuLuongModal.tsx` (Chuẩn hóa phiếu lương nhân viên).
- `HRMS.Mobile/src/types/me.ts` (Mở rộng `PayrollDto`).
- `HRMS.Mobile/src/screens/Payroll/PayrollScreen.tsx` (Bổ sung toàn diện 7 tab: Chấm công, Thu nhập, Khấu trừ, Thuế TNCN, Cách tính, Nguồn & chính sách).

### 5. Automated Tests
- `HRMS.Tests/PayrollEngineProductionTests.cs` (Suite kiểm thử toàn diện các case nghiệp vụ).
- `HRMS.Tests/DiagnosticIdbl1934Tests.cs` (Kiểm thử tính bất biến của bản ghi lịch sử `IDBL = 1934`).

### 6. Documentation
- `docs/PAYROLL_PRODUCTION_AUDIT.md`
- `docs/PAYROLL_CALCULATION_GUIDE.md`
- `docs/PAYROLL_UI_FIELD_COVERAGE.md`
- `docs/PAYROLL_PRODUCTION_FINAL_STATUS.md`

---

## C. THAY ĐỔI CƠ SỞ DỮ LIỆU (DATABASE CHANGES)

1. **19 Bảng mới tạo lập**:
   - Nhóm chính sách: `TB_CHINH_SACH_LUONG`, `TB_CHINH_SACH_BHXH`, `TB_CHINH_SACH_BHXH_VUNG`, `TB_CHINH_SACH_CONG_DOAN`, `TB_THUE_TNCN_CHINH_SACH`, `TB_THUE_TNCN_BAC`.
   - Nhóm hồ sơ nhân viên: `TB_NHANVIEN_BAOHIEM_THAM_GIA`, `TB_NHANVIEN_CONG_DOAN_THAM_GIA`, `TB_NHANVIEN_THUE`, `TB_NGUOI_PHU_THUOC`.
   - Nhóm chi tiết phân rã & audit: `TB_PAYROLL_CALCULATION_RUN`, `TB_BANGLUONG_SNAPSHOT_CHINHSACH`, `TB_BANGLUONG_CT`, `TB_BANGLUONG_CT_SOURCE`, `TB_BANGLUONG_BAOHIEM`, `TB_BANGLUONG_CONG_DOAN`, `TB_BANGLUONG_THUE_CT`, `TB_BANGLUONG_OT_COMPLIANCE`, `TB_BANGLUONG_CALC_TRACE`.
2. **6 Triggers bảo vệ tính bất biến chính sách**:
   - `TRG_LOCK_LUONG_POLICY`, `TRG_LOCK_BHXH_POLICY`, `TRG_LOCK_CONGDOAN_POLICY`, `TRG_LOCK_THUE_POLICY`: Cấm sửa đổi, xóa chính sách khi đã có bảng lương tham chiếu.
   - `TRG_BIU_TB_BANGLUONG`: Tự động đồng bộ metadata snapshot.
   - `TRG_CHECK_OVERLAP_BHXH`: Kiểm soát không cho trùng lặp khoảng thời gian hiệu lực.
3. **19 Sequences**: Khởi tạo từ mức chuẩn, hỗ trợ tự tăng độc lập không xung đột id literal.

---

## D. THAY ĐỔI TẦNG BUSINESS (PAYROLL ENGINE)

- **Một Engine duy nhất**: Toàn bộ yêu cầu tính lương từ Desktop, Web, Mobile và API đều đi qua `PayrollEngine`.
- **Pipeline 35 bước**:
  1. Xác định kỳ công & nhân viên
  2. Xác định hợp đồng có hiệu lực
  3. Lấy chính sách lương theo ngày hiệu lực
  4. Đọc dữ liệu chấm công thực tế
  5. Phân loại ca ngày / ca đêm
  6. Tính đơn giá ngày & đơn giá giờ
  7. Tính lương theo ngày công
  8. Nạp và tính toán 13 khoản phụ cấp
  9. Phân loại phụ cấp chịu thuế/miễn thuế/đóng bảo hiểm
  10. Tính tiền làm thêm giờ (OT) theo hệ số 150%, 200%, 300%, +30%, +20%
  11. Kiểm tra giới hạn làm thêm giờ (40h/tháng, 200h-300h/năm)
  12. Tổng hợp lương GROSS
  13. Xác định diện tham gia bảo hiểm
  14. Xác định mức lương đóng BHXH (áp dụng trần 20 lần mức tham chiếu và sàn vùng I-IV)
  15. Tính trích nộp BHXH NLĐ (10.5%)
  16. Tính trích nộp BHXH NSDLĐ (21.5%)
  17. Xác định diện đoàn viên công đoàn
  18. Tính đoàn phí NLĐ (1% có trần 10% LTT vùng)
  19. Tính kinh phí công đoàn NSDLĐ (2%)
  20. Xác định tình trạng cư trú thuế
  21. Xác định thu nhập chịu thuế
  22. Tính các khoản miễn trừ thuế theo luật định
  23. Xác định số người phụ thuộc hợp lệ
  24. Áp dụng mức giảm trừ bản thân (15,5 tr) và người phụ thuộc (6,2 tr)
  25. Tính thuế TNCN theo biểu lũy tiến 5 bậc năm 2026
  26. Khấu trừ tiền tạm ứng đã duyệt
  27. Khấu trừ kỷ luật, tiền phạt
  28. Tính lương NET Thực lĩnh
  29. Tính tổng chi phí doanh nghiệp
  30. Ghi snapshot toàn bộ chính sách
  31. Ghi chi tiết từng dòng itemized details
  32. Ghi calculation trace từng bước
  33. Ghi kết quả tuân thủ làm thêm giờ
  34. Kiểm tra tính toàn vẹn (Validation)
  35. Ghi nhận giao dịch CSDL nguyên tử (Atomic Commit)

---

## E. THAY ĐỔI TẦNG API & DTO

- **Endpoints chuẩn hóa**:
  - `GET /api/bangluong`: Trả về danh sách đầy đủ các cột production snapshot.
  - `GET /api/bangluong/detail`: Trả về 16 nhóm dữ liệu phân rã chi tiết.
  - `POST /api/bangluong/tinhluong`: Kích hoạt engine chuẩn tính lại toàn bộ nhân sự.
  - `GET /api/me/payroll`: Dành riêng cho Mobile, hiển thị đầy đủ Gross, Net, Deductions, Chấm công, Thuế TNCN, Cách tính và Chính sách.
- **DTOs**:
  - `BANGLUONG_DTO` & `ModernPayrollSnapshotDto`.
  - `MobilePayrollDto` (bổ sung đầy đủ trường nghiệp vụ và giải thích).

---

## F. THAY ĐỔI TẦNG GIAO DIỆN (DESKTOP, WEB, MOBILE)

1. **Desktop**:
   - `FrmBangLuong`: Hiển thị đầy đủ thông tin chuẩn 2026, hỗ trợ phân loại dữ liệu Legacy vs Production.
   - `rptBaoCaoLuongNV`: Đọc trực tiếp từ CSDL, không suy đoán.
2. **Web**:
   - `BangLuongPage.tsx`: Xóa toàn bộ fallback mock. Thẻ tổng tiền tính từ dữ liệu thực tế. Badge màu cam cho dữ liệu lịch sử, màu xanh cho dữ liệu sản xuất.
   - `PayrollDetailDrawer.tsx` & `PhieuLuongModal.tsx`: Hỗ trợ 16 section chi tiết (Tổng quan, Chấm công, Lương, Phụ cấp, Tăng ca, Bảo hiểm, Công đoàn, Thuế TNCN, Các khoản trừ, Thực lĩnh, Chi phí DN, Chính sách, Trace, Tuân thủ).
3. **Mobile**:
   - `PayrollScreen.tsx`: Bổ sung 7 khối thông tin trực quan: Card Thực lĩnh lớn, Chấm công & ngày làm, Thu nhập, Các khoản khấu trừ, Chi tiết thuế TNCN, Cách tính lương & thuế bằng ngôn ngữ người thường, Nguồn & chính sách áp dụng.

---

## G. KẾT QUẢ KIỂM THỬ TỰ ĐỘNG (AUTOMATED TEST RESULTS)

- **Lệnh thực hiện**:
  ```bash
  dotnet test HRMS.Tests\HRMS.Tests.csproj --filter "FullyQualifiedName~DiagnosticIdbl1934Tests|FullyQualifiedName~PayrollEngineProductionTests"
  ```
- **Kết quả**:
  - **Tổng số tests**: 13
  - **Passed**: 13 (100%)
  - **Failed**: 0
  - **Skipped**: 0
  - **Thời gian chạy**: 41 giây
- **Chi tiết xác nhận**:
  - `IDBL = 1934`: SHA-256 fingerprint trước và sau tính lại là `928e820148a1212e07a37efd3350578b27f55a3c8fde7fc5c61ba8f48dcad04c` (Trùng khớp 100% từng byte).
  - Migration Guard: Chặn thành công chạy lại migration bằng lỗi `ORA-20004`.
  - Policy Immutability Trigger: Chặn thành công sửa chính sách bằng lỗi `ORA-20021`.
  - Toàn bộ 937 bản ghi kỳ công `202609` được tính toán thành công với 3.691 itemized details, 937 insurance traces, 937 union traces, 937 OT compliance snapshots.

---

## H. KIỂM TRA BIÊN DỊCH VÀ XÂY DỰNG (BUILD VERIFICATION)

1. **C# Solution & Projects**:
   - `HRMS.DataAccess` -> Build Succeeded.
   - `HRMS.Business` -> Build Succeeded (`Bu.dll`).
   - `HRMS.Api` -> Build Succeeded (`HRMS_API.dll`, exit code 0).
   - `HRMS.Tests` -> Build Succeeded (`HRMS.Tests.dll`).
2. **Web Frontend (`HRMS.Web`)**:
   - Lệnh `npm run build` (`tsc -b && vite build`) -> **Succeeded in 1.98s, 0 errors**.
3. **Mobile App (`HRMS.Mobile`)**:
   - Lệnh `npx tsc --noEmit` -> **Succeeded with 0 errors**.

---

## I. TÀI LIỆU ĐÃ TẠO (DOCUMENTATION CREATED)

1. `docs/PAYROLL_PRODUCTION_AUDIT.md`: Ma trận audit 17 hạng mục trên 7 tầng kiến trúc.
2. `docs/PAYROLL_CALCULATION_GUIDE.md`: Hướng dẫn tính lương từ số 0 bằng tiếng Việt cho người không chuyên.
3. `docs/PAYROLL_UI_FIELD_COVERAGE.md`: Bảng chi tiết độ phủ trường dữ liệu từ CSDL ra UI.
4. `docs/PAYROLL_PRODUCTION_FINAL_STATUS.md`: Báo cáo tổng kết hiện trạng triển khai.

---

## K. RỦI RO CÒN LẠI VÀ KHUYẾN NGHỊ VẬN HÀNH (REMAINING RISKS & RECOMMENDATIONS)

1. **Rủi ro phụ cấp đặc thù doanh nghiệp**:
   - Mặc dù hệ thống đã hỗ trợ động 13 loại phụ cấp trong `TB_PHUCAP`, nếu doanh nghiệp phát sinh thêm loại phụ cấp thứ 14, cần thêm bản ghi vào `TB_PHUCAP` và cấu hình chính sách tính thuế/bảo hiểm tương ứng.
2. **Khuyến nghị đồng bộ thời gian hiệu lực chính sách**:
   - Khi áp dụng chính sách mới (ví dụ thay đổi mức lương tối thiểu vùng từ năm 2027), cán bộ nhân sự cần thêm bản ghi mới với `HIEU_LUC_TU` chính xác, không cập nhật trực tiếp vào bản ghi chính sách cũ đang có bảng lương tham chiếu (hệ thống sẽ từ chối qua Trigger).
