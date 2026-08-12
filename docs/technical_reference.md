# 📚 Technical Reference (Tài Liệu Kỹ Thuật)

### ✨ Tính Năng Nghiệp Vụ

#### 1. 👤 Quản Lý Nhân Sự (HR Module)

| Form | Chức năng |
|------|-----------|
| `FrmNhanVien` | Hồ sơ nhân viên: thông tin cá nhân, ảnh BLOB, phòng ban, chức vụ, trình độ, dân tộc, tôn giáo |
| `FrmDieuChuyen_NhanVien` | Lập quyết định và lưu lịch sử điều chuyển phòng ban / bộ phận / chức vụ |
| `FrmHopDongLaoDong` | Quản lý hợp đồng lao động: số lần ký, thời hạn, hệ số lương cơ bản |
| `FrmKhenThuong` | Quyết định khen thưởng: nội dung, ngày ban hành, đính kèm |
| `FrmKyLuat` | Quyết định kỷ luật: hình thức, mức độ, ngày hiệu lực |
| `FrmNangLuong_NhanVien` | Lộ trình tăng lương: lưu vết từng lần điều chỉnh hệ số lương |
| `FrmNhanVien_ThoiViec` | Hồ sơ nghỉ việc: lý do, ngày nghỉ, ghi chú |
| `FrmCongTy`, `FrmPhongBan`, `FrmBoPhan`, `FrmChucVu`, `FrmTrinhDo`, `FrmDanToc`, `FrmTonGiao` | Danh mục hệ thống (CRUD chuẩn) |

#### 2. ⏱ Chấm Công & Tính Lương (Timekeeping & Payroll)

| Form | Chức năng |
|------|-----------|
| `FrmLoaiCa` | Danh mục ca làm việc (ca ngày, ca đêm, ca gãy) + hệ số lương ca |
| `FrmLoaiCong` | Định nghĩa loại ngày công (thường, nghỉ phép, nghỉ ốm, lễ) |
| `FrmBangCong` | Bảng công tháng: tổng hợp ngày công thực tế, nghỉ phép của nhân viên |
| `FrmBangCong_ChiTiet` | Chi tiết giờ check-in / check-out hàng ngày từng nhân viên |
| `FrmCapNhatNgayCong` | Cập nhật, chỉnh sửa ngày công riêng lẻ |
| `FrmTangCa` | Theo dõi giờ tăng ca theo ngày/tháng, gắn với hệ số ca |
| `FrmPhuCap` | Danh mục phụ cấp (ăn trưa, xăng xe, điện thoại, trách nhiệm) + phân bổ cho nhân viên |
| `FrmUngLuong` | Ghi nhận và duyệt yêu cầu tạm ứng lương giữa kỳ |
| `FrmBangLuong` | **Tự động tính lương**: Lương CB × Hệ số ngày công + Lương tăng ca + Phụ cấp − Tạm ứng |

> **Công thức tính lương:**
> ```
> Thực lĩnh = (Lương_CB_HĐ × Ngày_Công_Thực_Tế / Ngày_Công_Chuẩn)
>            + (Giờ_Tăng_Ca × Hệ_Số_Ca × Lương_Giờ)
>            + Tổng_Phụ_Cấp
>            − Tổng_Tạm_Ứng
> ```

#### 3. 📄 In Ấn & Báo Cáo (Reports)

| Report | Nội dung |
|--------|----------|
| `rptBangCongTongHop` | Bảng công tổng hợp toàn công ty theo tháng |
| `rptBangCongCTNV` | Bảng công chi tiết từng nhân viên |
| `rptBaoCaoLuongNV` | Phiếu lương cá nhân (hỗ trợ xuất PDF/Excel) |
| `rptDSNhanVien` | Danh sách nhân viên theo phòng ban |
| `rptHopDongLaoDong` | In hợp đồng lao động theo mẫu |
| `rptKhenThuong` / `rptKyLuat` | In quyết định khen thưởng / kỷ luật |
| `rptDSHopDongHetHan` | Danh sách hợp đồng lao động sắp hết hạn |
| `rptDSTangCa` | Báo cáo tổng hợp giờ tăng ca theo kỳ công |
| `FrmDashboardNhanSu` | Dashboard biểu đồ: cơ cấu phòng ban, giới tính, trình độ, độ tuổi |
| `FrmDashboardLuong` | Dashboard biểu đồ lương theo phòng ban, kỳ công |
| `FrmBaoCaoTongHop` | Trung tâm báo cáo tổng hợp (chọn loại báo cáo, lọc kỳ công/nhân viên) |
| `FrmBaoCaoChiTiet` | Báo cáo chi tiết theo kỳ công và nhân viên |

#### 4. 🔐 Quản Trị Hệ Thống (System Administration)

| Form | Chức năng |
|------|-----------|
| `FrmDangNhap` | Đăng nhập với xác thực BCrypt + phân quyền theo nhóm |
| `FrmUser` / `FrmGroup` | Quản lý tài khoản người dùng và nhóm quyền |
| `FrmShowUser_Group` | Xem và gán thành viên vào nhóm |
| `FrmPhanQuyenChucNang` | Phân quyền truy cập từng chức năng (menu) theo nhóm |
| `FrmPhanQuyenBaoCao` | Phân quyền xem báo cáo theo nhóm |
| `FrmChangePassword` | Đổi mật khẩu cá nhân |
| `FrmSetting` | Cấu hình kết nối Ollama và model AI |
| `FrmCreateAccount` | Tạo tài khoản mới kèm thiết lập quyền ban đầu |
| `FrmDatabaseConfig` | Cấu hình kết nối Oracle đa profile (Server IP, Port, SID/Service Name, Auth) |
| `FrmOllamaConfig` | Cấu hình Ollama server (URL, tên model) với nút kiểm tra kết nối |
| `FrmThongBao` | Quản lý thông báo nội bộ (ghim, loại, trạng thái, hạn, phân theo công ty/phòng ban) |
| `FrmLanguages` | Quản lý danh mục ngôn ngữ hệ thống (thêm/sửa/xóa, bật/tắt active) |
| `FrmDataExport` | Xuất cấu trúc và dữ liệu Oracle (SQL/JSON/XML, chọn bảng, DDL/Data, nén ZIP) |
| `FrmDataImport` | Nhập dữ liệu từ file SQL/JSON vào Oracle (Truncate, Disable Constraints/Triggers) |
| `FrmUserDashboard` | Dashboard giám sát phiên đăng nhập, thiết bị, IP người dùng |

#### 5. 🤖 Trợ Lý AI Nhân Sự Thông Minh (AI Copilot)

| Service | Vai trò | Trạng thái |
|---------|---------|------------|
| `QdrantService` | Tìm kiếm ngữ nghĩa | **Đang sử dụng** |
| `OllamaService` | LLM local | **Đang sử dụng** |
| `SqlGeneratorService` | NL → Oracle SQL | **Đang duy trì / tạm bypass trong chế độ test Qdrant** |
| `HybridRagService` | Orchestrator | **Đang phát triển** |
| `AiRouterService` | Routing intent | **Đang sử dụng** |

*(Xem thêm danh sách đầy đủ các service và form trong [docs/technical_reference.md](docs/technical_reference.md))*

---

### 🗄️ Cơ Sở Dữ Liệu (Database Schema)

> File backup đầy đủ: [HR_backup.sql](./HR_backup.sql)

#### Nhóm Bảng Nhân Sự & Tổ Chức

```
TB_NHANVIEN          — Bảng trung tâm, lưu toàn bộ hồ sơ nhân viên
TB_CONGTY            — Thông tin công ty
TB_PHONGBAN          — Danh mục phòng ban
TB_BOPHAN            — Danh mục bộ phận (con của phòng ban)
TB_CHUCVU            — Danh mục chức vụ
TB_TRINHDO           — Danh mục trình độ học vấn
TB_DANTOC            — Danh mục dân tộc
TB_TONGIAO           — Danh mục tôn giáo
TB_GIOITINH          — Danh mục giới tính
TB_QUOCTICH          — Danh mục quốc tịch
```

#### Nhóm Bảng Biến Động Nhân Sự

```
TB_HOPDONG           — Hợp đồng lao động (số lần ký, hệ số lương)
TB_DIEUCHUYEN_NHANVIEN — Lịch sử điều chuyển nội bộ
TB_NANGLUONG_NHANVIEN  — Lộ trình tăng lương
TB_KHENTHUONG_KYLUAT — Quyết định khen thưởng và kỷ luật
TB_NHANVIEN_THOIVIEC — Hồ sơ thôi việc
```

#### Nhóm Bảng Chấm Công & Tài Chính

```
TB_LOAICA            — Danh mục ca làm việc + hệ số lương ca
TB_LOAICONG          — Danh mục loại ngày công
TB_BANGCONG          — Bảng công tháng (tổng hợp)
TB_BANGCONG_CHITIET  — Chi tiết check-in/check-out hàng ngày
TB_KYCONG            — Kỳ công (chu kỳ tính lương)
TB_KYCONGCHITIET     — Chi tiết kỳ công từng nhân viên
TB_TANGCA            — Giờ tăng ca
TB_PHUCAP            — Danh mục phụ cấp
TB_NHANVIEN_PHUCAP   — Phân bổ phụ cấp cho nhân viên
TB_UNGLUONG          — Tạm ứng lương
TB_BANGLUONG         — Bảng lương tổng hợp
TB_BAOHIEM           — Thông tin bảo hiểm xã hội
```

#### Nhóm Bảng Hệ Thống (System Tables)

```
TB_SYS_USER          — Tài khoản người dùng (mật khẩu BCrypt)
TB_SYS_GROUP         — Nhóm quyền và mối quan hệ thành viên
TB_SYS_FUNCTION      — Danh mục chức năng trong hệ thống
TB_SYS_RIGHT         — Phân quyền chức năng theo người dùng/nhóm
TB_SYS_REPORT        — Danh mục báo cáo
TB_SYS_RIGHT_REPORT  — Phân quyền xem báo cáo
TB_CONFIG            — Cấu hình hệ thống (Ollama URL, Qdrant URL, model name, ...)
TB_SYS_LOG           — Nhật ký hoạt động hệ thống
TB_SYS_LOGIN_HISTORY — Lịch sử đăng nhập (thiết bị, IP, thời gian)
TB_THONGBAO          — Thông báo nội bộ (tiêu đề, nội dung, ghim, trạng thái, hết hạn)
TB_LANGUAGES         — Danh mục ngôn ngữ giao diện
TB_TRANSLATIONS      — Từ điển dịch UI (key-value theo ngôn ngữ)
```

> **Trigger quan trọng:** [SYS_USER_triggers.sql](./DA/SYS_USER_triggers.sql) — Cascade delete khi xóa tài khoản người dùng (tự động dọn sạch nhóm, quyền hàm, quyền báo cáo).

---

