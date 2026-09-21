# Hệ Thống Quản Lý Nhân Sự (HRMS)

Hệ thống quản lý nhân sự đang được phát triển, tập trung vào quản lý thông tin nhân viên, chấm công, tiền lương và các nghiệp vụ nhân sự liên quan. Project cung cấp ba giao diện sử dụng (Desktop, Web, Mobile) với hai kiến trúc truy cập dữ liệu khác nhau, kết nối đến Oracle Database.

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61dafb?logo=react)](https://react.dev/)
[![Expo](https://img.shields.io/badge/Expo-SDK%2057-black?logo=expo)](https://expo.dev/)
[![Oracle DB](https://img.shields.io/badge/Oracle%20Database-19c%2F23ai-red?logo=oracle)](https://www.oracle.com/database/)

---

## Ngôn ngữ / Languages

- [🇻🇳 Tiếng Việt (Chi tiết)](#-tiếng-việt)
- [🇺🇸 English (Overview)](#-english)
- [🇯🇵 日本語 (概要)](#-日本語)

---

## 🇻🇳 Tiếng Việt

### Giới thiệu

HRMS là một project quản lý nhân sự được tổ chức thành nhiều thành phần. Ba ứng dụng client (Desktop, Web, Mobile) phục vụ các đối tượng và mục đích sử dụng khác nhau, chia sẻ chung tầng Business Logic và Data Access nhưng với luồng truy cập dữ liệu khác nhau:

- **Desktop** truy cập trực tiếp Business và DataAccess (không qua API).
- **Web** và **Mobile** giao tiếp qua REST API.

---

### Kiến trúc hệ thống

Desktop và Web/Mobile có kiến trúc truy cập dữ liệu khác nhau:

```text
┌─────────────────┐
│  HRMS.Desktop   │   WinForms / DevExpress
└────────┬────────┘
         │  (project reference)
         ├──────────────────────────┐
         ▼                          ▼
┌─────────────────┐       ┌─────────────────┐
│ HRMS.Business   │──────▶│ HRMS.DataAccess │
│  (Bu namespace) │       │  (DA namespace) │
└─────────────────┘       └────────┬────────┘
                                   │  Entity Framework 6.5
                                   ▼
                              Oracle Database


┌─────────────────┐       ┌─────────────────┐
│    HRMS.Web     │       │  HRMS.Mobile    │
│  React / Vite   │       │ React Native    │
└────────┬────────┘       └────────┬────────┘
         │  HTTP/REST              │  HTTP/REST
         └──────────┬──────────────┘
                    ▼
           ┌─────────────────┐
           │    HRMS.Api     │   ASP.NET Web API 2
           └────────┬────────┘
                    │  (project reference)
                    ├──────────────────────────┐
                    ▼                          ▼
           ┌─────────────────┐       ┌─────────────────┐
           │ HRMS.Business   │──────▶│ HRMS.DataAccess │
           └─────────────────┘       └────────┬────────┘
                                              │
                                              ▼
                                         Oracle Database
```

**Dependency graph (xác minh từ `.csproj` ProjectReference):**

| Project | Tham chiếu đến |
|---------|---------------|
| `HRMS.Desktop` | `HRMS.Business`, `HRMS.DataAccess` |
| `HRMS.Api` | `HRMS.Business`, `HRMS.DataAccess` |
| `HRMS.Business` | `HRMS.DataAccess` |
| `HRMS.DataAccess` | — (chỉ dùng NuGet: Oracle.ManagedDataAccess, EntityFramework) |
| `HRMS.Tests` | `HRMS.Business` (ProjectReference), `HRMS.Api` (DLL reference) |
| `HRMS.VectorDataSync` | `HRMS.Business`, `HRMS.DataAccess` |

> **Lưu ý:** Desktop không gọi REST API. Desktop tham chiếu trực tiếp `HRMS.Business` (namespace `Bu`) và `HRMS.DataAccess` (namespace `DA`) trong code. Duy nhất `HttpClient` trong Desktop được sử dụng cho kết nối Ollama AI, không phải cho HRMS.Api.

---

### Cấu trúc project

```text
QuanLyNhanSu/
├── HRMS.Api/              ← REST API (ASP.NET Web API 2) phục vụ Web và Mobile
├── HRMS.Business/         ← Business logic layer (namespace: Bu)
├── HRMS.DataAccess/       ← Data access layer, Entity Framework 6.5 + Oracle (namespace: DA)
├── HRMS.Desktop/          ← WinForms client, DevExpress 24.1
├── HRMS.Web/              ← Web SPA (React 19, Vite, Ant Design 6)
├── HRMS.Mobile/           ← Mobile app (React Native 0.86, Expo SDK 57)
├── HRMS.Tests/            ← Automated tests (NUnit)
├── HRMS.VectorDataSync/   ← Console tool đồng bộ dữ liệu nhân viên vào Qdrant
├── database/
│   └── migrations/        ← SQL migration scripts (V1_0 đến V1_14)
├── docs/                  ← Tài liệu kỹ thuật
├── HRMS.sln               ← Visual Studio Solution
├── docker-compose.yml     ← Docker: Oracle 23ai Free, Qdrant, Ollama, Web
├── build_deploy.ps1       ← Script build frontend + backend cho deployment
├── HRMS_SetupScript.iss   ← Inno Setup installer cho Desktop
├── start-api-service.bat  ← Script khởi động API
├── start_local_backend.bat
├── proxy.js               ← CORS proxy cho development
└── .env.example           ← Template cấu hình environment
```

---

### HRMS.Desktop

Ứng dụng WinForms sử dụng DevExpress 24.1, là giao diện nghiệp vụ chính dành cho tác vụ quản lý và xử lý dữ liệu nhân sự. Desktop truy cập trực tiếp `HRMS.Business` và `HRMS.DataAccess` qua project reference — không thông qua REST API.

**Kiến trúc truy cập dữ liệu:**
- Desktop import trực tiếp namespace `Bu` (Business) và `DA` (DataAccess).
- Các form gọi business class (ví dụ: `Bu.CLASS_NHANSU.NHANVIEN`, `Bu.CLASS_CHAMCONG.BANGLUONG`) để thực hiện CRUD.
- Business class sử dụng `DA.MyEntities` (EF DbContext) để truy vấn Oracle Database.
- Authentication được xử lý local qua `Bu.CLASS_SYSTEM.SYS_USER` và `Bu.CLASS_SYSTEM.PasswordHasher` (BCrypt), không qua JWT.
- Phân quyền sử dụng `Bu.CLASS_SYSTEM.UserSession` lưu trạng thái đăng nhập trong bộ nhớ process.

**Các nhóm chức năng (xác minh từ source code):**

| Nhóm | Forms | Chức năng |
|------|-------|-----------|
| Nhân sự | `FrmNhanVien`, `FrmPhongBan`, `FrmBoPhan`, `FrmChucVu`, `FrmTrinhDo`, `FrmDanToc`, `FrmTonGiao`, `FrmCongTy` | Quản lý nhân viên, danh mục tổ chức |
| Hợp đồng | `FrmHopDongLaoDong`, `FrmLoaiHopDong` | Hợp đồng lao động, loại hợp đồng |
| Khen thưởng / Kỷ luật | `FrmKhenThuong`, `FrmKyLuat` | Quyết định khen thưởng, kỷ luật |
| Nâng lương / Điều chuyển | `FrmNangLuong_NhanVien`, `FrmDieuChuyen_NhanVien` | Nâng lương, điều chuyển nhân viên |
| Nghỉ việc | `FrmNhanVien_ThoiViec` | Xử lý nghỉ việc |
| Phê duyệt | `FrmPheDuyetYeuCau` | Phê duyệt yêu cầu từ Mobile (nghỉ phép, tăng ca, điều chỉnh công) |
| Chấm công | `FrmBangCong`, `FrmBangCong_ChiTiet`, `FrmCapNhatNgayCong` | Bảng chấm công theo kỳ, chi tiết từng nhân viên |
| Ca / Công | `FrmLoaiCa`, `FrmLoaiCong`, `FrmNgayLe` | Danh mục ca làm, loại công, ngày lễ |
| Tăng ca | `FrmTangCa` | Quản lý tăng ca |
| Phụ cấp / Ứng lương | `FrmPhuCap`, `FrmUngLuong` | Quản lý phụ cấp, tạm ứng lương |
| Tiền lương | `FrmBangLuong` | Bảng lương theo kỳ |
| Dashboard | `FrmDashboardNhanSu`, `FrmDashboardLuong` | Biểu đồ thống kê phân bổ nhân sự, lương |
| Báo cáo | `FrmBaoCaoTongHop`, `FrmBaoCaoChiTiet` | Báo cáo tổng hợp và chi tiết |
| In ấn | `rptBaoCaoLuongNV`, `rptBangCongTongHop`, `rptDSNhanVien`, `rptHopDongLaoDong`, `rptKhenThuong`, `rptKyLuat`, v.v. | XtraReports: phiếu lương, bảng công, hợp đồng, khen thưởng |
| Tài khoản | `FrmDangNhap`, `FrmQuanLyTaiKhoan`, `FrmCreateAccount`, `FrmChangePassword`, `FrmUser`, `frmGroup`, `FrmShowUser_Group` | Đăng nhập, quản lý user/group |
| Phân quyền | `FrmPhanQuyenChucNang`, `FrmPhanQuyenBaoCao` | RBAC theo chức năng và báo cáo |
| Hệ thống | `FrmDatabaseConfig`, `FrmOllamaConfig`, `FrmSetting`, `FrmLanguages` | Cấu hình database, Ollama, i18n |
| Import/Export | `FrmDataImport`, `FrmDataExport` | Nhập xuất dữ liệu |
| Thông báo | `FrmThongBao` | Thông báo nội bộ |
| AI | `FrmAI`, `FrmAI_Chat` | Chatbot AI hỏi đáp dữ liệu nhân sự |

---

### HRMS.Api

REST API dùng ASP.NET Web API 2 (.NET Framework 4.7.2), **phục vụ Web và Mobile** (Desktop không sử dụng API này).

API tham chiếu trực tiếp `HRMS.Business` và `HRMS.DataAccess` qua ProjectReference. Controllers sử dụng namespace `Bu` (Business) và `DA` (DataAccess) để thực hiện nghiệp vụ.

**Authentication & Authorization:**
- JWT với HMAC-SHA256 (`JwtService.cs`) — tạo và xác thực token.
- `JwtAuthorizeAttribute` — filter kiểm tra JWT, role (Admin/User), quyền chức năng (`Right`), và đồng bộ audit user.
- Data scoping theo `MaCty`/`MaDvi` (công ty/đơn vị) được embed trong JWT claims.

**Controllers (17 controllers):**

| Controller | Chức năng | Endpoint chính |
|-----------|-----------|----------------|
| `AuthController` | Đăng nhập, đăng ký, đổi mật khẩu | `/api/auth/*` |
| `MeController` | Self-service cho Mobile (profile, chấm công, lương, hợp đồng, bảo hiểm, yêu cầu nghỉ phép/tăng ca/điều chỉnh công, dashboard, thông báo) | `/api/me/*` |
| `NhanVienController` | CRUD nhân viên | `/api/nhanvien/*` |
| `ChamCongController` | Chấm công, kỳ công | `/api/chamcong/*` |
| `BangLuongController` | Bảng lương | `/api/bangluong/*` |
| `HopDongController` | Hợp đồng lao động | `/api/hopdong/*` |
| `KhenThuongKyLuatController` | Khen thưởng, kỷ luật | `/api/khenthuongkyluat/*` |
| `NangLuongDieuChuyenController` | Nâng lương, điều chuyển | `/api/nangluong-dieuchuyen/*` |
| `PhuCapUngLuongController` | Phụ cấp, ứng lương | `/api/phucap-ungluong/*` |
| `TangCaUngLuongController` | (gộp trong controller khác) | |
| `DanhMucController` | Danh mục (phòng ban, chức vụ, v.v.) | `/api/danhmuc/*` |
| `DashboardController` | Dashboard thống kê | `/api/dashboard/*` |
| `UserController` | Quản lý user, group, phân quyền, bulk provisioning | `/api/user/*` |
| `ApprovalController` | Phê duyệt yêu cầu | `/api/approval/*` |
| `AuditController` | Nhật ký kiểm toán | `/api/audit/*` |
| `AiChatController` | AI Chat qua API | `/api/ai/*` |

> `MeController` là controller lớn nhất (~75KB), cung cấp toàn bộ endpoint self-service cho Mobile app.

---

### HRMS.Business

Business logic layer (namespace: `Bu`), chứa các class nghiệp vụ và AI services. Tham chiếu `HRMS.DataAccess` qua ProjectReference.

**CLASS_NHANSU (16 class):** Nghiệp vụ nhân sự — NHANVIEN, PHONGBAN, BOPHAN, CHUCVU, TRINHDO, DANTOC, TONGIAO, CONGTY, HOPDONGLAODONG, LOAIHOPDONG, KHENTHUONG_KYLUAT, NANGLUONG_NHANVIEN, DIEUCHUYEN_NHANVIEN, NHANVIEN_THOIVIEC, GIOITINH, QUOCTICH.

**CLASS_CHAMCONG (12 class):** Nghiệp vụ chấm công và lương — KYCONG, KYCONGCHITIET, BANGCONG_NV_CHITIET, BANGLUONG, TANGCA, HESO_TANGCA, PHUCAP, UNGLUONG, LOAICA, LOAICONG, NGAYLE, SYS_CONFIG.

**CLASS_SYSTEM (8 class):** Hệ thống — SYS_USER (quản lý user/group/phân quyền, ~35KB), PasswordHasher (BCrypt), UserSession, SYS_LANGUAGE, SYS_FUNCTION, SYS_PARAM, THONGBAO, AI_READONLY.

**DTO (20+ class):** Data Transfer Objects cho API response — NHANVIEN_DTO, BANGLUONG_DTO, HOPDONG_DTO, TANGCA_DTO, DASHBOARD_DTO, v.v.

**AI_Services:** Xem phần [AI / RAG](#ai--rag).

---

### HRMS.DataAccess

Data access layer (namespace: `DA`), sử dụng Entity Framework 6.5 với Oracle.ManagedDataAccess 23.7.0.

Có hai EF DbContext:

| Context | Mục đích | Entities |
|---------|----------|----------|
| `MyEntities` | Context chính, đọc/ghi toàn bộ bảng nghiệp vụ | ~40 entity classes (TB_NHANVIEN, TB_BANGLUONG, TB_SYS_USER, v.v.) |
| `AiEntities` | Context chỉ đọc dành cho AI | 6 View entities (V_AI_EMPLOYEE, V_AI_ATTENDANCE, V_AI_OVERTIME, V_AI_INSURANCE, V_AI_ADVANCE, V_AI_ALLOWANCE) |

`MyEntities` có **audit logging** tự động (`MyEntities.Audit.cs`) — EF interceptor ghi nhận mọi thay đổi dữ liệu (INSERT/UPDATE/DELETE) vào `TB_SYS_LOG` bao gồm: user ID, username, thời gian, IP, module, bảng, giá trị cũ/mới.

Cả hai context đều sử dụng EDMX model (Database-First approach).

---

### HRMS.Web

Web SPA sử dụng React 19, Vite 8.2, Ant Design 6.6, Recharts 3.10 và TypeScript. Giao tiếp với `HRMS.Api` qua HTTP/REST bằng Axios, đính kèm JWT Bearer token tự động.

**Pages (12 pages):**

| Page | Chức năng |
|------|-----------|
| `DashboardPage` | Biểu đồ thống kê nhân sự và lương (Recharts) |
| `NhanVienPage` | Quản lý nhân viên, modal Employee360 xem chi tiết 360° |
| `ChamCongPage` | Chấm công theo kỳ |
| `BangLuongPage` | Bảng lương, phiếu lương chi tiết (PhieuLuongModal, PayrollDetailDrawer) |
| `HopDongPage` | Hợp đồng lao động |
| `KhenThuongKyLuatPage` | Khen thưởng, kỷ luật |
| `NangLuongDieuChuyenPage` | Nâng lương, điều chuyển |
| `TangCaUngLuongPage` | Tăng ca, ứng lương |
| `UserManagementPage` | Quản lý user (tạo hàng loạt, liên kết user-employee, phân quyền 5 thao tác) |
| `ApprovalCenterPage` | Trung tâm phê duyệt yêu cầu |
| `AuditLogPage` | Nhật ký kiểm toán |
| `Login` | Đăng nhập |

**Components (16 components):** MainLayout, AiChatDrawer (chatbot AI), CommandPaletteModal (tìm kiếm nhanh), BulkProvisioningModal, PhanQuyenModal, PhieuLuongModal, Employee360Modal, UserDetailDrawer, UserEditModal, UserEmployeeLinkModal, GroupMembersModal, ChangePasswordModal, NotificationPopoverContent, PayrollDetailDrawer, CinematicHeroGallery.

**Phân quyền:** Hệ thống phân quyền 5 thao tác (View, Add, Edit, Delete, Print) và route guard — `permissionUtils.ts` kiểm tra quyền trước khi hiển thị menu/trang.

**i18n:** Đa ngôn ngữ tích hợp trong `services/i18n.ts`.

**API communication:** `services/api.ts` — Axios client tự động attach JWT token từ localStorage, base URL cấu hình qua `.env`.

---

### HRMS.Mobile

Mobile app sử dụng React Native 0.86 + Expo SDK 57 + TypeScript, tập trung vào **self-service cho nhân viên** — không phải ứng dụng quản trị. Giao tiếp với `HRMS.Api` qua HTTP/REST, sử dụng chủ yếu các endpoint `/api/me/*`.

**Authentication:** JWT token lưu trong Expo SecureStore (`expo-secure-store`), Axios interceptor tự động attach Bearer token.

**Màn hình (xác minh từ navigation và screen source code):**

| Nhóm | Screens | Chức năng |
|------|---------|-----------|
| Khởi động | `SplashScreen`, `LanguageSelectScreen`, `LoginScreen` | Splash, chọn ngôn ngữ, đăng nhập |
| Tab chính | `HomeScreen`, `AttendanceScreen`, `PayrollScreen`, `NotificationListScreen`, `ProfileScreen` | Dashboard cá nhân, xem chấm công, phiếu lương, thông báo, hồ sơ |
| Chi tiết | `ContractScreen`, `InsuranceScreen`, `NotificationDetailScreen` | Hợp đồng, bảo hiểm, chi tiết thông báo |
| Yêu cầu | `LeaveRequestScreen`, `AttendanceCorrectionScreen`, `MyOvertimeScreen`, `MyRequestsScreen` | Gửi yêu cầu nghỉ phép, điều chỉnh chấm công, đăng ký tăng ca, xem tổng hợp yêu cầu |
| Cài đặt | `SettingsScreen`, `LanguageSettingsScreen`, `ThemeSettingsScreen`, `ChangePasswordScreen` | Ngôn ngữ, giao diện sáng/tối, đổi mật khẩu |

**API calls (meApi.ts):** getMe, getDashboard, getProfile, updateProfile, getAttendance, getPayroll, getContract, getInsurance, getNotifications, getLeaveRequests, createLeaveRequest, getAttendanceCorrections, createAttendanceCorrection, getOvertimeRequests, createOvertimeRequest, cancelOvertimeRequest, getAllRequests.

**i18n:** Đa ngôn ngữ qua `i18next` + `react-i18next` + `expo-localization`.

> Mobile tập trung vào tra cứu thông tin cá nhân và gửi yêu cầu. Các chức năng quản trị (quản lý nhân viên, phân quyền, tính lương, v.v.) chỉ có trên Desktop và Web.

---

### HRMS.Tests

Automated tests sử dụng NUnit 3.14.0, target .NET Framework 4.7.2. Tham chiếu `HRMS.Business` (ProjectReference) và `HRMS.Api` (DLL reference).

Hiện có **khoảng 141 test method** (đếm `[Test]` và `[TestCase]` attributes).

| File test | Nội dung | Loại |
|-----------|----------|------|
| `PasswordHasherTests` | BCrypt hash/verify | Unit |
| `AiCacheServiceTests` | AI cache service | Unit |
| `QueryPreprocessorTests` | Tiền xử lý câu hỏi AI | Unit |
| `AiPromptInjectionTests` | AST Validator chống SQL injection trong AI | Unit/Security |
| `AiRetrievalBenchmarkTests` | Đo hiệu năng cache, preprocessor, AST | Benchmark |
| `ApiSecurityIntegrationTests` | JWT, BCrypt | Integration |
| `BulkProvisioningAndSecurityTests` | Tạo tài khoản hàng loạt, bảo mật | Integration |
| `MobileSecurityAndApiTests` | API và bảo mật cho Mobile | Integration |
| `PermissionFiveActionsTests` | Phân quyền 5 thao tác | Integration |
| `DbQueryTests` | Truy vấn database | Integration |
| `PayrollCalculationTests` | Tính lương | Business logic |
| `OvertimeWorkflowTests` | Workflow tăng ca | Business logic |
| `TangCaOvertimeTests` | Nghiệp vụ tăng ca | Business logic |
| `LoaiHopDongAndNgayLeTests` | Loại hợp đồng, ngày lễ | Business logic |
| `PhuCapAndHopDongAsyncTests` | Phụ cấp, hợp đồng | Business logic |
| `DatabaseDataSeederTests` | Dữ liệu mẫu | Data |

Mobile có thêm 1 file test riêng (`test/unit.test.mjs`) chạy qua Node.js test runner.

---

### HRMS.VectorDataSync

Console application (.NET Framework 4.7.2) để đồng bộ dữ liệu nhân viên từ Oracle vào Qdrant vector database. Tham chiếu `HRMS.Business` và `HRMS.DataAccess` qua ProjectReference.

Chức năng:
1. Xóa collection cũ trong Qdrant.
2. Đọc dữ liệu nhân viên từ `V_AI_EMPLOYEE` qua `AiEntities` context.
3. Chuyển mỗi nhân viên thành text description (họ tên, ngày sinh, phòng ban, chức vụ, bộ phận, SĐT, địa chỉ).
4. Thêm vector embedding vào Qdrant qua `IVectorService`.

Đây là công cụ seeding dữ liệu ban đầu, chạy thủ công khi cần.

---

### Các module nghiệp vụ

#### Nhân sự cơ bản
- Quản lý thông tin nhân viên (hồ sơ, ảnh, liên hệ, CCCD)
- Danh mục tổ chức: công ty, phòng ban, bộ phận, chức vụ, trình độ, dân tộc, tôn giáo
- Trạng thái nhân viên (đang làm / đã nghỉ việc)

#### Hợp đồng & Nhân sự
- Hợp đồng lao động (số HĐ, thời hạn, hệ số lương, lương thỏa thuận)
- Loại hợp đồng
- Khen thưởng, kỷ luật (quyết định, lý do, nội dung)
- Nâng lương (hệ số cũ → mới)
- Điều chuyển nhân viên (phòng ban cũ → mới)
- Xử lý nghỉ việc

#### Chấm công
- Kỳ công theo tháng/năm
- Chi tiết chấm công 31 ngày/tháng (D1-D31)
- Ngày công, ngày phép, nghỉ không phép, công ngày lễ, công chủ nhật
- Danh mục: loại ca, loại công, ngày lễ

#### Tăng ca & Lương
- Tăng ca (ngày, số giờ, loại ca, hệ số, số tiền)
- Phụ cấp nhân viên
- Tạm ứng lương
- Bảng lương (lương cơ bản × hệ số + phụ cấp + tăng ca − ứng lương = thực lĩnh)

#### Yêu cầu & Phê duyệt
- Yêu cầu nghỉ phép (từ Mobile)
- Yêu cầu điều chỉnh chấm công (từ Mobile)
- Yêu cầu tăng ca (từ Mobile)
- Workflow phê duyệt: PENDING → APPROVED / REJECTED / CANCELLED
- Mapping user → nhân viên (`TB_USER_EMPLOYEE_MAPPING`)

#### Quản trị hệ thống
- User, group (nhóm quyền)
- RBAC: phân quyền chức năng (5 thao tác: View, Add, Edit, Delete, Print)
- Phân quyền báo cáo
- Audit log tự động
- Lịch sử đăng nhập, account lockout
- Thông báo nội bộ
- Đa ngôn ngữ (i18n)
- Cấu hình hệ thống (`TB_SYS_CONFIG`)

---

### Feature Matrix

| Module | Desktop | Web | Mobile |
|--------|---------|-----|--------|
| Quản lý nhân viên | ✓ | ✓ | Xem hồ sơ cá nhân |
| Hợp đồng lao động | ✓ | ✓ | Xem hợp đồng cá nhân |
| Chấm công / Kỳ công | ✓ | ✓ | Xem chấm công cá nhân |
| Tăng ca | ✓ | ✓ | Gửi yêu cầu tăng ca |
| Tiền lương | ✓ | ✓ | Xem phiếu lương cá nhân |
| Phụ cấp / Ứng lương | ✓ | ✓ | — |
| Khen thưởng / Kỷ luật | ✓ | ✓ | — |
| Nâng lương / Điều chuyển | ✓ | ✓ | — |
| Bảo hiểm | — | — | Xem bảo hiểm cá nhân |
| Nghỉ phép | — | — | Gửi yêu cầu |
| Điều chỉnh chấm công | — | — | Gửi yêu cầu |
| Phê duyệt yêu cầu | ✓ | ✓ | — |
| Dashboard / Biểu đồ | ✓ | ✓ | Dashboard cá nhân |
| Báo cáo / In ấn | ✓ (XtraReports) | — | — |
| Quản lý User / Group | ✓ | ✓ | — |
| Phân quyền RBAC | ✓ | ✓ | — |
| Import / Export | ✓ | — | — |
| Audit Log | Tự động (EF) | ✓ (xem) | — |
| Thông báo | ✓ | ✓ | ✓ |
| Đa ngôn ngữ | ✓ | ✓ | ✓ |
| AI Chat | ✓ (experimental) | ✓ (experimental) | — |
| Cấu hình Ollama | ✓ | — | — |
| Cấu hình Database | ✓ | — | — |

---

### Database

Oracle Database. Migration scripts trong `database/migrations/` tương thích Oracle 19c / 21c / 23ai Free (comment trong migration V1_0 ghi rõ "Oracle 19c/21c/23ai").

**19 migration files** (V1_0 đến V1_14, bao gồm rollback scripts):

| Migration | Nội dung |
|-----------|----------|
| V1_0 | Schema khởi tạo: tổ chức, nhân viên, hợp đồng, chấm công, lương, tăng ca, ứng lương, khen thưởng/kỷ luật, nâng lương, điều chuyển, user/group/right, config |
| V1_1 | Audit triggers |
| V1_2 | 6 AI Views (V_AI_EMPLOYEE, V_AI_ATTENDANCE, V_AI_OVERTIME, V_AI_INSURANCE, V_AI_ADVANCE, V_AI_ALLOWANCE) |
| V1_3 | Schema versioning |
| V1_4 | Fix triggers và encoding |
| V1_5 | 13 loại phụ cấp, mở rộng model hợp đồng |
| V1_6 | Redesign module tăng ca |
| V1_7 | Loại hợp đồng và ngày lễ |
| V1_8 | Cải thiện module lương |
| V1_9 | Khen thưởng/kỷ luật, ngày ứng lương |
| V1_10 | Loại bỏ mã kỳ công, sửa nội dung |
| V1_11 | Bổ sung 5 quyền (View/Add/Edit/Delete/Print) |
| V1_12 | Liên kết user-employee cho Mobile |
| V1_13 | Mapping user-employee, bảng yêu cầu Mobile (nghỉ phép, điều chỉnh công, tăng ca) |
| V1_14 | Hardening workflow tăng ca (ràng buộc CHECK, FK, unique index chống trùng) |

**Nhóm bảng chính:**

| Nhóm | Bảng |
|------|------|
| Tổ chức | TB_CONGTY, TB_PHONGBAN, TB_BOPHAN, TB_CHUCVU, TB_TRINHDO, TB_DANTOC, TB_TONGIAO |
| Nhân viên | TB_NHANVIEN, TB_NHANVIEN_THOIVIEC |
| Hợp đồng | TB_HOPDONG, TB_LOAIHOPDONG |
| Chấm công | TB_KYCONG, TB_KYCONGCHITIET, TB_LOAICA, TB_LOAICONG, TB_NGAYLE, TB_BANGCONG_CHITIET |
| Lương & Phụ cấp | TB_BANGLUONG, TB_PHUCAP, TB_NHANVIEN_PHUCAP |
| Tăng ca & Ứng lương | TB_TANGCA, TB_HESO_TANGCA, TB_UNGLUONG |
| Khen thưởng / Kỷ luật | TB_KHENTHUONG_KYLUAT |
| Nâng lương / Điều chuyển | TB_NANGLUONG, TB_DIEUCHUYEN |
| Bảo hiểm | TB_BAOHIEM |
| Người dùng & Quyền | TB_SYS_USER, TB_SYS_GROUP, TB_SYS_RIGHT, TB_SYS_FUNCTION, TB_SYS_CONFIG |
| Hệ thống | TB_SYS_LOG, TB_SYS_LOGIN_HISTORY, TB_SYS_REPORT, TB_SYS_RIGHT_REPORT, TB_THONGBAO, TB_LANGUAGES, TB_TRANSLATIONS |
| Yêu cầu Mobile | TB_YEUCAU_NGHIPHEP, TB_YEUCAU_DIEUCHINHCONG, TB_YEUCAU_TANGCA, TB_USER_EMPLOYEE_MAPPING |
| AI Views | V_AI_EMPLOYEE, V_AI_ATTENDANCE, V_AI_OVERTIME, V_AI_INSURANCE, V_AI_ADVANCE, V_AI_ALLOWANCE |

---

### AI / RAG

Tính năng AI đang trong giai đoạn **thử nghiệm** (experimental). Có trên Desktop (`FrmAI`, `FrmAI_Chat`) và Web (`AiChatDrawer`), cũng expose qua API (`AiChatController`).

**Pipeline xử lý (xác minh từ `HybridRagService.Ask()`):**

```text
Câu hỏi người dùng
        │
        ▼
QueryPreprocessor ──── phục hồi dấu tiếng Việt, chuẩn hóa
        │
        ▼
FastResponseService ── xử lý câu hỏi xã giao (rule-based, không gọi LLM)
        │
        ▼ (nếu không match)
AiRouterService ────── phân loại ý định (rule-based → fallback Ollama)
        │               Intents: EMPLOYEE, ATTENDANCE, OVERTIME, INSURANCE, ADVANCE, ALLOWANCE, GENERAL
        ▼
RagContextRetriever
        ├── SqlGeneratorService ── hardcode patterns phổ biến → fallback Ollama sinh SQL
        │       │
        │       ▼
        │   OracleSqlAstValidator ── tokenize, chặn DDL/DML, whitelist 6 AI Views
        │       │
        │       ▼
        │   SafeSqlExecutor ── chạy qua AiEntities (read-only), timeout 15s
        │
        └── QdrantService ── semantic search (embedding: bge-m3 qua Ollama)
                │
                ▼
        Kết hợp SQL result + Vector matches
                │
                ▼
RagSynthesizer ──── gửi context + câu hỏi cho Ollama để tổng hợp câu trả lời (streaming)
                │
                ▼
        Câu trả lời (hiển thị token-by-token)
```

**Components AI (source code trong `HRMS.Business/Services/AI_Services/`):**

| Component | Vị trí | Chức năng |
|-----------|--------|-----------|
| `HybridRagService` | Core | Orchestrator điều phối toàn bộ pipeline |
| `AiRouterService` | Core | Phân loại ý định (rule-based + LLM fallback) |
| `SqlGeneratorService` | Core | Sinh SQL từ câu hỏi (hardcode + LLM fallback) |
| `OracleSqlAstValidator` | Core | Kiểm duyệt SQL: tokenize, chặn DDL/DML/system packages, whitelist Views |
| `SafeSqlExecutor` | Core | Thực thi SQL qua AiEntities với timeout |
| `RagContextRetriever` | Core | Kết hợp kết quả SQL + Vector search thành context |
| `RagSynthesizer` | Core | Gọi LLM để tổng hợp câu trả lời |
| `FastResponseService` | Core | Phản hồi nhanh cho câu hỏi xã giao |
| `QueryPreprocessor` | Core | Chuẩn hóa câu hỏi, phục hồi dấu tiếng Việt |
| `AiSchemaService` | Core | Cung cấp schema cho SQL prompt |
| `JsonPromptManager` | Core | Quản lý prompt templates từ `ai_prompts.json` |
| `OllamaService` | LLM | Gọi Ollama API (generate, embeddings) — model mặc định: qwen2.5:latest |
| `QdrantService` | Vector | Tương tác Qdrant: tạo collection, add, search, remove vectors |
| `QdrantOutboxManager` | Vector | Outbox pattern — retry queue khi Qdrant không khả dụng |
| `AiDataSyncHub` | Vector | Event hub đồng bộ khi nhân viên thay đổi/xóa |
| `AiCacheService` | Memory | In-memory dictionary cache kết quả SQL |
| `AiChatHistory` | Memory | Lưu lịch sử hội thoại trong session |

**Mô hình mặc định:** Qwen 2.5 (qua Ollama), embedding: bge-m3. Model có thể cấu hình qua `TB_SYS_CONFIG` (key: `AiModel`, `OllamaHost`).

> Packages NuGet cho LangChain, OpenAI, Anthropic, Google Generative AI, HuggingFace có trong `packages.config` của Business nhưng pipeline hiện tại chỉ sử dụng `OllamaService` gọi Ollama trực tiếp qua HTTP.

**Giới hạn truy cập AI:**
- AI sử dụng `AiEntities` — EF context riêng kết nối chỉ đọc tới 6 View (`V_AI_*`), tách biệt với `MyEntities`.
- `OracleSqlAstValidator` chặn DDL/DML, multi-statement, truy cập package hệ thống Oracle (`DBMS_`, `UTL_`, `SYS.`, `V$`...), và chỉ cho phép SELECT/WITH trên whitelist Views.
- Đây là các lớp giới hạn được triển khai trong code. Chúng giảm phạm vi truy cập AI nhưng không phải security guarantee — cần đánh giá thêm trước khi sử dụng trong môi trường có yêu cầu bảo mật cao.

---

### Security

Các cơ chế bảo mật có trong source code:

| Cơ chế | Implementation | Sử dụng bởi |
|--------|---------------|-------------|
| Mật khẩu | BCrypt hash (`PasswordHasher.cs`), chỉ chấp nhận prefix `$2a$/$2b$/$2y$` | Desktop, API |
| Authentication (Desktop) | BCrypt verify trực tiếp qua `SYS_USER` → `PasswordHasher` | Desktop |
| Authentication (API) | JWT HMAC-SHA256 (`JwtService.cs`), secret từ env/config/ephemeral fallback | Web, Mobile |
| Authorization (API) | `JwtAuthorizeAttribute` — kiểm tra JWT, Admin role, function rights, data scope | Web, Mobile |
| Authorization (Desktop) | `UserSession` + `FormSecurity` — kiểm tra quyền trước khi mở form | Desktop |
| Phân quyền | 5 thao tác (View, Add, Edit, Delete, Print) per function code | Desktop, Web, API |
| Audit | EF interceptor ghi INSERT/UPDATE/DELETE vào `TB_SYS_LOG` (user, thời gian, IP, old/new values) | Tự động |
| Account lockout | `FAILED_LOGIN_COUNT`, `LOCKOUT_END` trong `TB_SYS_USER` | Desktop, API |
| AI SQL safety | AST Validator: whitelist Views, chặn DDL/DML/system packages | AI services |
| AI connection | `AiEntities` context riêng, chỉ đọc 6 Views | AI services |
| Mobile token | Lưu trữ qua Expo SecureStore | Mobile |
| JWT timing-safe compare | `CryptographicEquals` chống timing attack | API |

> Project có các cơ chế bảo vệ ở tầng authentication, authorization và giới hạn truy cập dữ liệu. Các cơ chế này là phần của implementation hiện tại và cần được đánh giá thêm trước khi triển khai production.

---

### Công nghệ

| Thành phần | Công nghệ | Version (từ project files) |
|-----------|-----------|---------------------------|
| Backend framework | .NET Framework | 4.7.2 |
| API | ASP.NET Web API 2 | 5.2.9 |
| Desktop UI | DevExpress WinForms | 24.1.4 |
| Web | React + TypeScript + Vite | React 19.2, Vite 8.2, TypeScript 6.0 |
| Web UI | Ant Design | 6.6.3 |
| Web charts | Recharts | 3.10.1 |
| Mobile | React Native + Expo | RN 0.86.3, Expo SDK 57 |
| Database | Oracle Database | 19c / 21c / 23ai Free |
| ORM | Entity Framework | 6.5.1 |
| Oracle driver | Oracle.ManagedDataAccess | 23.7.0 |
| AI LLM | Ollama (Qwen 2.5) | — |
| AI Embedding | bge-m3 (qua Ollama) | — |
| Vector DB | Qdrant | 1.12.1 (docker-compose) |
| Password hash | BCrypt.Net-Next | 4.0.3 |
| Testing | NUnit | 3.14.0 |
| Installer | Inno Setup | — |
| Containerization | Docker Compose | Oracle 23ai Free, Qdrant, Ollama, Web (Nginx) |

---

### Cài đặt nhanh

```text
1. Clone repository
2. Chuẩn bị Oracle Database (19c / 21c / 23ai Free)
   - Chạy migration scripts trong database/migrations/ theo thứ tự V1_0 → V1_14
3. Cấu hình connection string
   - Copy .env.example → .env và điền thông tin
   - Hoặc cấu hình trong App.config / Web.config
4. Build solution trong Visual Studio (HRMS.sln)
   - Yêu cầu DevExpress 24.1 license cho Desktop
5. Chạy API: start-api-service.bat hoặc chạy HRMS.Api qua IIS Express
6. Chạy Desktop: chạy HRMS.Desktop từ Visual Studio (không cần API)
7. Chạy Web: cd HRMS.Web && npm install && npm run dev
8. Chạy Mobile: cd HRMS.Mobile && npm install && npx expo start
9. (Tùy chọn) AI: cài Ollama, pull model qwen2.5:latest và bge-m3, chạy Qdrant
```

**Docker (tùy chọn):** `docker-compose.yml` cung cấp Oracle 23ai Free, Qdrant 1.12.1, Ollama và Web (Nginx build).

```bash
docker-compose up -d
```

Chi tiết:
- [`docs/installation.md`](docs/installation.md)
- [`docs/deployment_guide.md`](docs/deployment_guide.md)
- [`docs/technical_reference.md`](docs/technical_reference.md)

---

### Current Status

| Thành phần | Trạng thái | Ghi chú |
|-----------|-----------|---------|
| Desktop | Implemented | Client có nhiều chức năng nhất, giao diện nghiệp vụ chính |
| Web | Implemented, in development | Có 12 pages quản trị, vẫn đang được phát triển thêm |
| Mobile | In development | Self-service cho nhân viên, chưa bao phủ nghiệp vụ quản trị |
| API | Implemented | 17 controllers, phục vụ Web và Mobile |
| Database | Implemented | 14 migration versions, schema ổn định |
| AI / RAG | Experimental | Pipeline hoạt động, chất lượng phụ thuộc model và dữ liệu |
| VectorDataSync | Experimental | Console tool, chạy thủ công |
| Tests | Partial coverage | ~141 test methods, bao phủ một số module |
| Documentation | Partial | 9 tài liệu trong docs/, cần cập nhật thêm |

---

### Limitations

- **Desktop không đi qua API** — Desktop truy cập trực tiếp Business/DataAccess, do đó Desktop và Web/Mobile có thể có logic xử lý khác nhau cho cùng một nghiệp vụ.
- **Mobile chỉ hỗ trợ self-service** — không có chức năng quản trị.
- **AI đang thử nghiệm** — chất lượng sinh SQL phụ thuộc vào model Ollama, có thể sinh ra truy vấn không chính xác.
- **DevExpress license** — Desktop yêu cầu DevExpress 24.1 (thư viện thương mại).
- **Ollama + Qdrant** — AI yêu cầu cài đặt và chạy Ollama (model qwen2.5 + bge-m3) và Qdrant riêng.
- **Test coverage chưa đầy đủ** — một số module chưa có automated test.
- **LangChain packages** — nhiều NuGet packages trong Business không được sử dụng bởi pipeline hiện tại.
- **Deployment** — chưa có quy trình deployment chuẩn hóa cho production.

---

### Hướng phát triển

Dựa trên tình trạng thực tế trong repository:

- Hoàn thiện Mobile self-service
- Tiếp tục phát triển Web client
- Mở rộng test coverage
- Cải thiện chất lượng AI retrieval (tuning prompt, mở rộng vector data)
- Chuẩn hóa deployment
- Cải thiện documentation
- Dọn dẹp unused NuGet packages

---

### Tài liệu

| Tài liệu | Đường dẫn |
|-----------|-----------|
| Hướng dẫn cài đặt | [`docs/installation.md`](docs/installation.md) |
| Hướng dẫn triển khai | [`docs/deployment_guide.md`](docs/deployment_guide.md) |
| Tham khảo kỹ thuật | [`docs/technical_reference.md`](docs/technical_reference.md) |
| Mobile API | [`docs/mobile_v1_api.md`](docs/mobile_v1_api.md) |
| Mobile Architecture | [`docs/mobile_v1_architecture.md`](docs/mobile_v1_architecture.md) |
| Mobile Security | [`docs/mobile_v1_security.md`](docs/mobile_v1_security.md) |
| Mobile Build | [`docs/mobile_v1_build.md`](docs/mobile_v1_build.md) |
| Mobile Database | [`docs/mobile_v1_database.md`](docs/mobile_v1_database.md) |
| Mobile Test Plan | [`docs/mobile_v1_test_plan.md`](docs/mobile_v1_test_plan.md) |

---

## 🇺🇸 English

### Overview

HRMS is a human resource management system under active development. It provides three client applications with different data access architectures:

**Desktop** (WinForms/DevExpress) accesses Business and DataAccess layers directly via project references — it does **not** go through the REST API.

**Web** (React/Ant Design) and **Mobile** (React Native/Expo) communicate with the backend through `HRMS.Api` (ASP.NET Web API 2), which in turn references Business and DataAccess layers.

```text
Desktop → Business → DataAccess → Oracle
Web     → API → Business → DataAccess → Oracle
Mobile  → API → Business → DataAccess → Oracle
```

**Key implemented features:**
- Employee records, contracts, departments, positions, organizational catalogs
- Attendance tracking (monthly timesheet with D1-D31), shift types, payroll calculation
- Overtime, allowances, salary advances
- Rewards, discipline, salary adjustments, transfers
- RBAC authorization with 5-action permission model (View/Add/Edit/Delete/Print)
- Audit logging (automatic EF interceptor), notifications, multi-language (i18n)
- Dashboards and reports (Desktop: DevExpress Charts + XtraReports, Web: Recharts)
- Approval workflow for Mobile requests (leave, attendance correction, overtime)
- Mobile self-service: profile, attendance, payslip, contracts, insurance, request submission
- Experimental AI assistant: Vietnamese NL2SQL + Hybrid RAG using local Ollama (Qwen 2.5) + Qdrant vector search

**Technology stack:** .NET Framework 4.7.2, ASP.NET Web API 2, Entity Framework 6.5, DevExpress 24.1, Oracle Database (19c/21c/23ai), React 19, Vite 8.2, Ant Design 6, React Native 0.86 (Expo SDK 57), Ollama, Qdrant, BCrypt, JWT (HMAC-SHA256), NUnit.

**Current status:** Desktop is the most feature-rich client with direct database access. Web provides administration features via API. Mobile focuses on employee self-service. AI features are experimental. The project is under development and requires further testing before production deployment.

For detailed documentation, see the `docs/` directory.

---

## 🇯🇵 日本語

### 概要

HRMSは開発中の人事管理システムです。3つのクライアントアプリケーションがあり、異なるデータアクセスアーキテクチャを持っています：

**Desktop**（WinForms/DevExpress）はBusiness層とDataAccess層に直接アクセスします（REST APIを経由しません）。**Web**（React）と**Mobile**（React Native/Expo）はREST API経由でバックエンドと通信します。

```text
Desktop → Business → DataAccess → Oracle
Web     → API → Business → DataAccess → Oracle
Mobile  → API → Business → DataAccess → Oracle
```

**主な実装済み機能：** 従業員情報管理、契約、勤怠管理、給与計算、残業、手当、表彰・懲戒、昇給・異動、RBAC権限管理（5操作モデル）、監査ログ、通知、多言語対応、ダッシュボード、レポート、モバイルセルフサービス（個人情報照会・申請送信）、実験的AI機能（ベトナム語NL2SQL + ローカルOllama + Qdrant）。

**技術スタック：** .NET Framework 4.7.2、ASP.NET Web API 2、Entity Framework 6.5、DevExpress 24.1、Oracle Database、React 19、React Native (Expo SDK 57)、Ollama、Qdrant、BCrypt、JWT、NUnit。

**現状：** Desktopが最も機能が充実（直接DBアクセス）。Webは管理機能をAPI経由で提供。Mobileは従業員セルフサービスに特化。AI機能は実験段階。本番環境へのデプロイ前に追加のテストが必要です。

詳細は `docs/` ディレクトリを参照してください。
