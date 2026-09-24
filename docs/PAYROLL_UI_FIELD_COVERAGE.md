# MA TRẬN PHỦ TRƯỜNG DỮ LIỆU GIAO DIỆN (PAYROLL UI FIELD COVERAGE)

> **Nguyên tắc cốt lõi**: Không bắt buộc mọi trường kỹ thuật phải nằm trên lưới hiển thị chính (Main Grid), nhưng **tuyệt đối không có trường nghiệp vụ nào bị mất đường truy cập**. Mọi trường đều phải truy xuất được từ một trong các vị trí: Lưới chính, Bảng chi tiết (Detail Modal/Drawer), Truy vết công thức (Calculation Trace), hoặc Khu vực quản trị/chính sách (Policy Snapshot).

---

## 1. BẢNG CHI TIẾT PHỦ TRƯỜNG TOÀN HỆ THỐNG

| Business Field | Oracle Database Column | API DTO Property | Desktop UI (`FrmBangLuong` / Report) | Web UI (`BangLuongPage`) | Mobile App (`PayrollScreen`) | Modal / Detail Drawer | Trace Window | Trạng thái hiển thị (Coverage Status) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Mã nhân viên** | `TB_BANGLUONG.MANV` | `MANV` / `manv` | Grid chính | Grid chính | Header | Detail Header | Trace Header | **FULL_COVERAGE** |
| **Họ và tên** | `TB_NHANVIEN.HOTEN` | `HOTEN` / `hoten` | Grid chính | Grid chính | Header | Detail Header | Trace Header | **FULL_COVERAGE** |
| **Phòng ban** | `TB_PHONGBAN.TENPB` | `TENPB` / `tenpb` | Grid chính | Grid chính | Sub-header | Detail Header | N/A | **FULL_COVERAGE** |
| **Kỳ công / Tháng / Năm**| `TB_BANGLUONG.MAKYCONG`, `THANG`, `NAM` | `MAKYCONG`, `THANG`, `NAM` | Header & Filter | Header & Filter | Month Nav Bar | Modal Header | Trace Header | **FULL_COVERAGE** |
| **Số công chuẩn** | `TB_BANGLUONG.CONG_CHUAN` | `CONG_CHUAN` / `congChuan` | Grid chính | Grid chính | Card Chấm công | Section Chấm công | Step 1 Trace | **FULL_COVERAGE** |
| **Số công thực tế** | `TB_BANGLUONG.CONG_THUCTE` | `CONG_THUCTE` / `congThucTe` | Grid chính | Grid chính | Card Chấm công | Section Chấm công | Step 2 Trace | **FULL_COVERAGE** |
| **Công làm ca ngày** | `TB_BANGLUONG.CONG_LAMNGAY` | `CONG_LAMNGAY` / `congLamNgay` | Grid chính | Detail Drawer | Card Chấm công | Section Chấm công | Step 2 Trace | **FULL_COVERAGE** |
| **Công làm ca đêm** | `TB_BANGLUONG.CONG_LAMDEM` | `CONG_LAMDEM` / `congLamDem` | Grid chính | Detail Drawer | Card Chấm công | Section Chấm công | Step 2 Trace | **FULL_COVERAGE** |
| **Đơn giá ngày** | `TB_BANGLUONG.DAILY_RATE` | `DAILY_RATE` / `dailyRate` | Grid chính | Detail Drawer | Card Chấm công | Section Chấm công | Step 1 Trace | **FULL_COVERAGE** |
| **Lương cơ bản tháng** | Hợp đồng / `DAILY_RATE * CONG_CHUAN` | `luongCoBan` | Báo cáo chi tiết | Cột Lương CB | Card Thu nhập | Section Lương | Step 1 Trace | **FULL_COVERAGE** |
| **Lương công thực tế** | `TB_BANGLUONG.LUONG_CONG_THUCTE` | `LUONG_CONG_THUCTE` | Grid chính | Grid chính | Card Thu nhập | Section Lương | Step 2 Trace | **FULL_COVERAGE** |
| **Tổng phụ cấp** | `TB_BANGLUONG.PHUCAP_CONG_THUCTE` | `PHUCAP_CONG_THUCTE` | Grid chính | Grid chính | Card Thu nhập | Section Phụ cấp | Step 3 Trace | **FULL_COVERAGE** |
| **Chi tiết 13 khoản phụ cấp** | `TB_BANGLUONG_CT.ITEM_TYPE='ALLOWANCE'` | `allowances` list | Report chi tiết | Detail Drawer | Section Thu nhập | Section Phụ cấp | Step 3 Trace | **FULL_COVERAGE** |
| **Tiền làm thêm giờ (OT)** | `TB_BANGLUONG.TIEN_TANGCA` | `TIEN_TANGCA` / `tienTangCa` | Grid chính | Grid chính | Card Thu nhập | Section Tăng ca | Step 4 Trace | **FULL_COVERAGE** |
| **Số giờ làm thêm** | `TB_TANGCA` / `SOGIO_TANGCA` | `soGioTangCa` | Grid chính | Detail Drawer | Card Chấm công | Section Tăng ca | Step 4 Trace | **FULL_COVERAGE** |
| **Chi tiết từng ca OT** | `TB_TANGCA` & `TB_BANGLUONG_CT` | `overtimes` list | Report chi tiết | Detail Drawer | Ẩn trên mobile | Section Tăng ca | Step 4 Trace | **FULL_COVERAGE** |
| **Thưởng chuyên cần** | `TB_BANGLUONG.TIEN_CHUYENCAN` | `TIEN_CHUYENCAN` / `tienChuyenCan`| Grid chính | Detail Drawer | Card Thu nhập | Section Thưởng | Step 3 Trace | **FULL_COVERAGE** |
| **Tiền ăn ca** | `TB_BANGLUONG.TIEN_AN_CA` | `TIEN_AN_CA` / `tienAnCa` | Grid chính | Detail Drawer | Card Thu nhập | Section Thưởng | Step 3 Trace | **FULL_COVERAGE** |
| **Khoản cộng khác** | `TB_BANGLUONG.KHOAN_CONG_KHAC` | `KHOAN_CONG_KHAC` / `khoanCongKhac` | Grid chính | Detail Drawer | Card Thu nhập | Section Thưởng | Step 3 Trace | **FULL_COVERAGE** |
| **Tổng thu nhập (GROSS)**| `TB_BANGLUONG.TONG_CONG` | `TONG_CONG` / `tongThuNhap` | Grid chính | Grid chính | Card Thu nhập | Section Tổng quan | Step 5 Trace | **FULL_COVERAGE** |
| **Mức lương đóng BHXH**| `TB_BANGLUONG.LUONG_BHXH` | `LUONG_BHXH` | Report chi tiết | Detail Drawer | Ẩn trên mobile | Section Bảo hiểm | Step 6 Trace | **FULL_COVERAGE** |
| **Trích nộp BHXH (8%)**| `TB_BANGLUONG.TIEN_BHXH` | `TIEN_BHXH` / `tienBhxh` | Report chi tiết | Detail Drawer | Card Khấu trừ | Section Bảo hiểm | Step 6 Trace | **FULL_COVERAGE** |
| **Trích nộp BHYT (1.5%)**| `TB_BANGLUONG.TIEN_BHYT` | `TIEN_BHYT` / `tienBhyt` | Report chi tiết | Detail Drawer | Card Khấu trừ | Section Bảo hiểm | Step 6 Trace | **FULL_COVERAGE** |
| **Trích nộp BHTN (1%)**| `TB_BANGLUONG.TIEN_BHTN` | `TIEN_BHTN` / `tienBhtn` | Report chi tiết | Detail Drawer | Card Khấu trừ | Section Bảo hiểm | Step 6 Trace | **FULL_COVERAGE** |
| **Tổng trừ BHXH (10.5%)**| `TB_BANGLUONG.TIEN_BHXH_TRICH` | `TIEN_BHXH_TRICH` | Grid chính | Grid chính | Card Khấu trừ | Section Bảo hiểm | Step 6 Trace | **FULL_COVERAGE** |
| **Đoàn phí công đoàn** | `TB_BANGLUONG.TIEN_CONG_DOAN` | `TIEN_CONG_DOAN` / `tienCongDoan` | Grid chính | Grid chính | Card Khấu trừ | Section Công đoàn | Step 7 Trace | **FULL_COVERAGE** |
| **Tạm ứng lương** | `TB_BANGLUONG.TIEN_TAMUNG` | `TIEN_TAMUNG` / `tienTamUng` | Grid chính | Grid chính | Card Khấu trừ | Section Khấu trừ | Step 9 Trace | **FULL_COVERAGE** |
| **Khấu trừ kỷ luật/khác**| `TB_BANGLUONG.KHOAN_TRU_KHAC` | `KHOAN_TRU_KHAC` / `khoanTruKhac` | Grid chính | Grid chính | Card Khấu trừ | Section Khấu trừ | Step 9 Trace | **FULL_COVERAGE** |
| **Thu nhập chịu thuế** | `TB_BANGLUONG.TONG_THU_NHAP_CHIU_THUE` | `TONG_THU_NHAP_CHIU_THUE` / `thuNhapChiuThue` | Detail Form | Detail Drawer | Card Thuế TNCN | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Giảm trừ bản thân** | `TB_BANGLUONG.GIAM_TRU_BAN_THAN` | `GIAM_TRU_BAN_THAN` / `giamTruBanThan` | Detail Form | Detail Drawer | Card Thuế TNCN | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Số người phụ thuộc** | `TB_BANGLUONG.SO_NGUOI_PHU_THUOC` | `SO_NGUOI_PHU_THUOC` / `soNguoiPhuThuoc` | Detail Form | Detail Drawer | Card Thuế TNCN | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Giảm trừ người phụ thuộc**| `TB_BANGLUONG.GIAM_TRU_PHU_THUOC` | `GIAM_TRU_PHU_THUOC` / `giamTruPhuThuoc` | Detail Form | Detail Drawer | Card Thuế TNCN | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Thu nhập tính thuế (TNTT)**| `TB_BANGLUONG.THU_NHAP_TINH_THUE` | `THU_NHAP_TINH_THUE` / `thuNhapTinhThue` | Detail Form | Detail Drawer | Card Thuế TNCN | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Thuế TNCN khấu trừ** | `TB_BANGLUONG.THUE_TNCN` | `THUE_TNCN` / `thueTncn` | Grid chính | Grid chính | Card Khấu trừ | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Chi tiết bậc thuế TNCN**| `TB_BANGLUONG_THUE_CT` | `taxBrackets` list | Report chi tiết | Detail Drawer | Ẩn trên mobile | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Hoàn thuế TNCN** | `TB_BANGLUONG.HOAN_THUE` | `HOAN_THUE` / `hoanThue` | Grid chính | Detail Drawer | Card Thuế TNCN | Section Thuế TNCN | Step 8 Trace | **FULL_COVERAGE** |
| **Thực lĩnh (NET)** | `TB_BANGLUONG.THUC_LINH` | `THUC_LINH` / `thucLinh` | Grid chính | Grid chính | Highlight Card | Section Tổng quan | Step 10 Trace | **FULL_COVERAGE** |
| **BHXH công ty đóng (17%)**| `TB_BANGLUONG.TIEN_BHXH_NSDLD` | `TIEN_BHXH_NSDLD` | Report chi tiết | Detail Drawer | Phân quyền bảo mật | Section Doanh nghiệp | Step 11 Trace | **ADMIN_COVERED** |
| **BHYT công ty đóng (3%)** | `TB_BANGLUONG.TIEN_BHYT_NSDLD` | `TIEN_BHYT_NSDLD` | Report chi tiết | Detail Drawer | Phân quyền bảo mật | Section Doanh nghiệp | Step 11 Trace | **ADMIN_COVERED** |
| **BHTN công ty đóng (1%)** | `TB_BANGLUONG.TIEN_BHTN_NSDLD` | `TIEN_BHTN_NSDLD` | Report chi tiết | Detail Drawer | Phân quyền bảo mật | Section Doanh nghiệp | Step 11 Trace | **ADMIN_COVERED** |
| **Kinh phí công đoàn (2%)**| `TB_BANGLUONG.TIEN_KINH_PHI_CD_NSDLD` | `TIEN_KINH_PHI_CD_NSDLD` | Report chi tiết | Detail Drawer | Phân quyền bảo mật | Section Doanh nghiệp | Step 11 Trace | **ADMIN_COVERED** |
| **Tổng chi phí Doanh nghiệp**| `TB_BANGLUONG.TONG_CHI_PHI_NSDLD` | `TONG_CHI_PHI_NSDLD` | Report chi tiết | Detail Drawer | Phân quyền bảo mật | Section Doanh nghiệp | Step 11 Trace | **ADMIN_COVERED** |
| **Tuân thủ giới hạn OT** | `TB_BANGLUONG_OT_COMPLIANCE` | `otCompliance` object | Cảnh báo Desktop | Detail Drawer | Card Chấm công | Section Tuân thủ | Trace Audit | **FULL_COVERAGE** |
| **Snapshot chính sách**| `POLICY_*_ID` FKs | `policySnapshot` object | Tab Chính sách | Detail Drawer | Card Chính sách | Section Chính sách | Trace Policy | **FULL_COVERAGE** |
| **Trạng thái bảng lương**| `TB_BANGLUONG.TRANG_THAI` | `TRANG_THAI` / `trangThaiChiTra` | Cột Trạng thái | Badge Trạng thái | Badge Trạng thái | Header Status | N/A | **FULL_COVERAGE** |
| **Phân loại lịch sử/Legacy**| `TB_BANGLUONG.IS_LEGACY` | `IS_LEGACY` / `isLegacy` | Badge Legacy | Badge cam Legacy | Badge xám Legacy | Header Badge | Trace Audit | **FULL_COVERAGE** |

---

## 2. KẾT LUẬN ĐÁNH GIÁ MỨC ĐỘ PHỦ TRƯỜNG (COVERAGE VERDICT)

1. **Tỷ lệ bao phủ trường nghiệp vụ**: **100%**.
   - Không có bất kỳ trường nghiệp vụ nào trong CSDL bị "mồ côi" hay thiếu endpoint cung cấp ra giao diện người dùng.
2. **Nguyên tắc "No Fake UI"**:
   - Loại bỏ hoàn toàn các trường tự sinh ở phía frontend (`?? 26`, `?? 10000000`, `?? 800000`, `* 0.03`).
   - Mọi số liệu hiển thị trên Web, Desktop và Mobile đều được cấp phát đồng nhất từ backend thông qua `PayrollEngine`.
3. **Phân quyền hiển thị (Security & Scope)**:
   - Các trường thuộc "Chi phí doanh nghiệp" (BHXH doanh nghiệp nộp, Kinh phí công đoàn 2%, Tổng chi phí NSDLĐ) được giới hạn cho quyền Admin/HR trên Desktop và Web Drawer, không tự tiện hiển thị trên màn hình Self-service của nhân viên trên Mobile nhằm bảo đảm tính bảo mật kinh doanh.
