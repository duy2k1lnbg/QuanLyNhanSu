# Hệ Thống Quản Lý Nhân Sự (HRMS)

Hệ thống quản lý nhân sự đang được phát triển, tập trung vào quản lý thông tin nhân viên, chấm công, tiền lương và các nghiệp vụ nhân sự liên quan. Project cung cấp ba giao diện sử dụng (Desktop, Web, Mobile) kết nối qua REST API chung đến Oracle Database, kèm tính năng trợ lý AI chạy local.

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

HRMS là một project quản lý nhân sự được tổ chức thành nhiều thành phần:

- **Desktop** — ứng dụng WinForms dùng DevExpress, hiện là giao diện nghiệp vụ chính cho các tác vụ quản lý và xử lý dữ liệu nhân sự.
- **Web** — ứng dụng SPA dùng React + Ant Design + Vite, cung cấp các chức năng quản trị qua trình duyệt.
- **Mobile** — ứng dụng React Native + Expo, cho phép nhân viên tự tra cứu thông tin cá nhân.
- **API** — REST API dùng ASP.NET Web API 2, là cổng kết nối chung cho Web và Mobile.
- **AI** — tính năng hỏi đáp dữ liệu nhân sự bằng tiếng Việt, chạy local qua Ollama + Qdrant.

```text
Desktop ─┐
Web ─────┼──> API ──> Business ──> DataAccess ──> Oracle Database
Mobile ──┘
                         │
                    AI Services
                   (Ollama + Qdrant)
```

---

### Cấu trúc Solution

| Project | Mô tả |
|---------|-------|
| `HRMS.Api` | REST API (ASP.NET Web API 2, .NET Framework 4.7.2) |
| `HRMS.Business` | Business logic, DTO, AI Services |
| `HRMS.DataAccess` | Data access layer (Entity Framework 6.5, Oracle) |
| `HRMS.Desktop` | WinForms client (DevExpress) |
| `HRMS.Web` | Web SPA (React 19, Vite, Ant Design, Recharts) |
| `HRMS.Mobile` | Mobile app (React Native 0.86, Expo SDK 57) |
| `HRMS.Tests` | Automated tests (NUnit) |
| `HRMS.VectorDataSync` | Công cụ console đồng bộ dữ liệu nhân viên vào Qdrant |
| `database/migrations` | SQL migration scripts (V1_0 đến V1_14) |

---

### Desktop

Desktop application hiện là giao diện nghiệp vụ chính, sử dụng DevExpress WinForms. Các chức năng có trong source code:

**Nhân sự:**
- Quản lý nhân viên (`FrmNhanVien`)
- Phòng ban, bộ phận, chức vụ, trình độ, dân tộc, tôn giáo
- Công ty
- Hợp đồng lao động (`FrmHopDongLaoDong`), loại hợp đồng
- Khen thưởng, kỷ luật
- Nâng lương, điều chuyển
- Nghỉ việc
- Phê duyệt yêu cầu (`FrmPheDuyetYeuCau`)

**Chấm công & Lương:**
- Bảng chấm công, chi tiết chấm công theo kỳ
- Loại ca, loại công, ngày lễ
- Tăng ca, hệ số tăng ca
- Phụ cấp, ứng lương
- Bảng lương
- Cập nhật ngày công

**Báo cáo & Dashboard:**
- Dashboard nhân sự (biểu đồ phân bổ theo phòng ban, giới tính, trình độ, độ tuổi)
- Dashboard lương
- Báo cáo tổng hợp, báo cáo chi tiết
- In ấn: phiếu lương, bảng công, danh sách nhân viên, hợp đồng, khen thưởng, kỷ luật, danh sách tăng ca, hợp đồng hết hạn (XtraReports)

**Hệ thống:**
- Đăng nhập, đổi mật khẩu
- Quản lý tài khoản, nhóm quyền
- Phân quyền chức năng, phân quyền báo cáo
- Cấu hình database, cấu hình Ollama
- Đa ngôn ngữ (i18n)
- Import/Export dữ liệu
- Thông báo nội bộ
- AI Chat (hỏi đáp dữ liệu nhân sự)

---

### Web

Web client sử dụng React 19, Vite, Ant Design 6 và Recharts. Các page và component có trong source code:

**Pages:**
- Dashboard (biểu đồ thống kê nhân sự và lương)
- Quản lý nhân viên (với modal Employee360 xem chi tiết)
- Chấm công
- Bảng lương (với phiếu lương chi tiết)
- Hợp đồng
- Khen thưởng / Kỷ luật
- Nâng lương / Điều chuyển
- Tăng ca / Ứng lương
- Quản lý người dùng (bao gồm tạo tài khoản hàng loạt, liên kết user-employee, phân quyền 5 thao tác)
- Trung tâm phê duyệt yêu cầu
- Nhật ký kiểm toán (Audit Log)
- Đăng nhập

**Components:**
- AI Chat Drawer (hỏi đáp AI từ Web)
- Command Palette (tìm kiếm nhanh)
- Phân quyền modal, phiếu lương modal
- Thông báo popover
- Đa ngôn ngữ (i18n tích hợp trong `services/i18n.ts`)

Web có hệ thống phân quyền 5 thao tác (View, Add, Edit, Delete, Print) và route guard dựa trên quyền user.

---

### Mobile

Mobile app sử dụng React Native + Expo SDK 57 + TypeScript, tập trung vào tính năng self-service cho nhân viên.

**Màn hình hiện có trong source code:**
- Splash, chọn ngôn ngữ, đăng nhập
- Home (dashboard cá nhân)
- Chấm công (xem lịch sử chấm công cá nhân)
- Phiếu lương (xem phiếu lương cá nhân)
- Hợp đồng lao động
- Bảo hiểm
- Thông báo (danh sách + chi tiết)
- Hồ sơ cá nhân
- Cài đặt (ngôn ngữ, giao diện, đổi mật khẩu)

**Yêu cầu (Requests):**
- Xin nghỉ phép
- Điều chỉnh chấm công
- Đăng ký tăng ca
- Xem tổng hợp yêu cầu đã gửi

Mobile giao tiếp với API qua các endpoint `/api/me/*` (self-scope) bằng JWT Bearer token lưu trong Expo SecureStore. App hỗ trợ i18n (đa ngôn ngữ) và theme (sáng/tối).

> **Lưu ý:** Mobile tập trung vào các chức năng tra cứu cá nhân (self-service), chưa bao phủ các nghiệp vụ quản trị như Desktop hay Web.

---

### Database

Database sử dụng Oracle Database. Migration scripts trong `database/migrations/` hỗ trợ Oracle 19c / 21c / 23ai Free.

**Các nhóm bảng chính (dựa trên migration V1_0 đến V1_14):**

| Nhóm | Bảng |
|------|------|
| Tổ chức | TB_CONGTY, TB_PHONGBAN, TB_BOPHAN, TB_CHUCVU, TB_TRINHDO, TB_DANTOC, TB_TONGIAO |
| Nhân viên | TB_NHANVIEN, TB_NHANVIEN_THOIVIEC |
| Hợp đồng | TB_HOPDONG, TB_LOAIHOPDONG |
| Chấm công | TB_KYCONG, TB_KYCONGCHITIET, TB_LOAICA, TB_LOAICONG, TB_NGAYLE, TB_BANGCONG_CHITIET |
| Tiền lương | TB_BANGLUONG, TB_PHUCAP, TB_NHANVIEN_PHUCAP |
| Tăng ca & Ứng lương | TB_TANGCA, TB_HESO_TANGCA, TB_UNGLUONG |
| Khen thưởng / Kỷ luật | TB_KHENTHUONG_KYLUAT |
| Nâng lương / Điều chuyển | TB_NANGLUONG, TB_DIEUCHUYEN |
| Bảo hiểm | TB_BAOHIEM |
| Người dùng & Phân quyền | TB_SYS_USER, TB_SYS_GROUP, TB_SYS_RIGHT, TB_SYS_FUNCTION, TB_SYS_CONFIG |
| Hệ thống | TB_SYS_LOG, TB_SYS_LOGIN_HISTORY, TB_SYS_REPORT, TB_SYS_RIGHT_REPORT, TB_THONGBAO, TB_LANGUAGES, TB_TRANSLATIONS |
| Yêu cầu Mobile | TB_YEUCAU_NGHIPHEP, TB_YEUCAU_DIEUCHINHCONG, TB_YEUCAU_TANGCA, TB_USER_EMPLOYEE_MAPPING |
| AI Views | V_AI_EMPLOYEE, V_AI_ATTENDANCE, V_AI_OVERTIME, V_AI_INSURANCE, V_AI_ADVANCE, V_AI_ALLOWANCE |

AI sử dụng `AiEntities` — một EF context riêng kết nối chỉ đọc tới 6 View an toàn (`V_AI_*`), tách biệt với `MyEntities` (context chính có audit logging).

---

### Trạng thái tính năng

| Module | Trạng thái | Ghi chú |
|--------|-----------|---------|
| Quản lý nhân viên | Implemented | Desktop + Web + API |
| Hợp đồng lao động | Implemented | Desktop + Web + API |
| Chấm công / Kỳ công | Implemented | Desktop + Web + API |
| Tăng ca | Implemented | Desktop + Web + API, workflow phê duyệt (migration V1_14) |
| Tiền lương | Implemented | Desktop + Web + API |
| Phụ cấp / Ứng lương | Implemented | Desktop + Web + API |
| Khen thưởng / Kỷ luật | Implemented | Desktop + Web + API |
| Nâng lương / Điều chuyển | Implemented | Desktop + Web + API |
| Phân quyền RBAC | Implemented | Desktop + Web + API (5 thao tác: View, Add, Edit, Delete, Print) |
| Dashboard & Báo cáo | Implemented | Desktop (DevExpress Charts + XtraReports), Web (Recharts) |
| Quản lý tài khoản | Implemented | Desktop + Web + API |
| Phê duyệt yêu cầu | Implemented | Desktop + Web + API |
| Audit Log | Implemented | EF interceptor tự động ghi nhận thay đổi dữ liệu |
| Thông báo nội bộ | Implemented | Desktop + Web + Mobile |
| Import / Export dữ liệu | Implemented | Desktop |
| Đa ngôn ngữ (i18n) | Implemented | Desktop + Web + Mobile |
| Mobile Self-Service | In development | Xem thông tin cá nhân, chấm công, lương, hợp đồng, bảo hiểm, gửi yêu cầu |
| AI Chat (NL2SQL + RAG) | Experimental | Desktop + Web, chạy local qua Ollama + Qdrant |
| Vector Data Sync | Experimental | Console tool đồng bộ dữ liệu nhân viên vào Qdrant |

---

### AI

Tính năng AI hiện đang trong giai đoạn thử nghiệm. Pipeline xử lý như sau:

1. **Tiền xử lý** (`QueryPreprocessor`): phục hồi dấu tiếng Việt, chuẩn hóa câu hỏi.
2. **Phản hồi nhanh** (`FastResponseService`): xử lý câu hỏi xã giao / hướng dẫn mà không cần gọi LLM.
3. **Phân loại ý định** (`AiRouterService`): rule-based trước, fallback sang Ollama nếu không khớp.
4. **Sinh SQL** (`SqlGeneratorService`): hardcode cho các pattern phổ biến, fallback sang Ollama để sinh SQL từ ngôn ngữ tự nhiên.
5. **Kiểm duyệt SQL** (`OracleSqlAstValidator`): tokenize câu lệnh, chặn DDL/DML, chỉ cho phép SELECT/WITH trên whitelist 6 AI Views.
6. **Thực thi an toàn** (`SafeSqlExecutor`): chạy SQL qua `AiEntities` (context chỉ đọc) với command timeout 15 giây.
7. **Tìm kiếm vector** (`QdrantService`): tìm kiếm ngữ nghĩa (semantic search) trong Qdrant, sử dụng embedding từ model `bge-m3` qua Ollama.
8. **Tổng hợp** (`RagSynthesizer`): kết hợp kết quả SQL + Vector context, gửi cho Ollama để tổng hợp câu trả lời.
9. **Cache** (`AiCacheService`): cache kết quả SQL theo câu hỏi (in-memory dictionary).
10. **Outbox** (`QdrantOutboxManager`): hàng đợi retry khi Qdrant không khả dụng.

**Mô hình mặc định:** Qwen 2.5 (qua Ollama), embedding: bge-m3. Có thể cấu hình model khác qua `TB_SYS_CONFIG`.

> Packages NuGet cho LangChain, OpenAI, Anthropic, Google Generative AI, HuggingFace cũng có trong `packages.config` nhưng pipeline AI hiện tại chỉ sử dụng Ollama trực tiếp qua HTTP.

**Giới hạn truy cập AI:**
- AI sử dụng `AiEntities` — một EF context riêng chỉ kết nối tới 6 View (`V_AI_*`), không truy cập trực tiếp các bảng chính.
- `OracleSqlAstValidator` kiểm tra cú pháp, chặn DDL/DML, chặn multi-statement, chặn truy cập package hệ thống Oracle (`DBMS_`, `UTL_`, `SYS.`...), và chỉ cho phép truy vấn các View nằm trong whitelist.
- Đây là các lớp giới hạn được triển khai trong code, không phải bảo đảm về bảo mật tuyệt đối. Cần đánh giá thêm trước khi sử dụng trong môi trường có yêu cầu bảo mật cao.

---

### Security

Các cơ chế bảo mật có trong source code:

| Cơ chế | Implementation |
|--------|---------------|
| Mật khẩu | BCrypt hash (`PasswordHasher.cs`), chỉ chấp nhận hash `$2a$/$2b$/$2y$` |
| Authentication | JWT với HMAC-SHA256 (`JwtService.cs`), hỗ trợ cấu hình qua env hoặc AppSettings |
| Authorization | RBAC qua `JwtAuthorizeAttribute` — kiểm tra role, quyền chức năng, data scope (công ty/chi nhánh) |
| Phân quyền chức năng | 5 thao tác chuẩn (View, Add, Edit, Delete, Print) trên cả Web và Desktop |
| Audit | EF interceptor ghi nhận thao tác thay đổi dữ liệu (user, thời gian, IP, nội dung cũ/mới) |
| AI SQL | AST Validator whitelist View, chặn DDL/DML/system packages |
| AI Connection | `AiEntities` context riêng, chỉ đọc 6 AI Views |
| Mobile token | Lưu trữ qua Expo SecureStore |
| Account lockout | Đếm số lần đăng nhập thất bại (`FAILED_LOGIN_COUNT`, `LOCKOUT_END`) |

> Project có các cơ chế bảo vệ ở tầng authentication, authorization và giới hạn truy cập dữ liệu. Các cơ chế này là một phần của implementation hiện tại và cần được đánh giá thêm trước khi triển khai trong môi trường production.

---

### Tests

Project `HRMS.Tests` sử dụng NUnit, hiện có khoảng 141 test method (đếm bằng `[Test]` và `[TestCase]`).

Các nhóm test có trong source code:

| File test | Nội dung |
|-----------|----------|
| `AiPromptInjectionTests` | Kiểm tra AST Validator chống SQL injection trong AI |
| `AiRetrievalBenchmarkTests` | Đo hiệu năng cache, preprocessor, AST |
| `AiCacheServiceTests` | Kiểm tra cache service |
| `ApiSecurityIntegrationTests` | Kiểm tra JWT, BCrypt |
| `PasswordHasherTests` | Kiểm tra hash và verify mật khẩu |
| `QueryPreprocessorTests` | Kiểm tra tiền xử lý câu hỏi AI |
| `DbQueryTests` | Kiểm tra các truy vấn database |
| `PayrollCalculationTests` | Kiểm tra tính lương |
| `OvertimeWorkflowTests` | Kiểm tra workflow tăng ca |
| `TangCaOvertimeTests` | Kiểm tra nghiệp vụ tăng ca |
| `BulkProvisioningAndSecurityTests` | Kiểm tra tạo tài khoản hàng loạt và bảo mật |
| `MobileSecurityAndApiTests` | Kiểm tra API và bảo mật cho Mobile |
| `PermissionFiveActionsTests` | Kiểm tra phân quyền 5 thao tác |
| `DatabaseDataSeederTests` | Kiểm tra dữ liệu mẫu |
| `LoaiHopDongAndNgayLeTests` | Kiểm tra loại hợp đồng và ngày lễ |
| `PhuCapAndHopDongAsyncTests` | Kiểm tra phụ cấp và hợp đồng |

Mobile có thêm 1 file test riêng (`test/unit.test.mjs`).

---

### Công nghệ

| Thành phần | Công nghệ | Version |
|-----------|-----------|---------|
| Backend framework | .NET Framework | 4.7.2 |
| API | ASP.NET Web API 2 | 5.2.9 |
| Desktop UI | DevExpress WinForms | — |
| Web | React + TypeScript + Vite | React 19, Vite 8.2 |
| Web UI library | Ant Design | 6.6 |
| Web charts | Recharts | 3.10 |
| Mobile | React Native + Expo | RN 0.86, Expo SDK 57 |
| Database | Oracle Database | 19c / 21c / 23ai Free |
| ORM | Entity Framework | 6.5.1 |
| Oracle driver | Oracle.ManagedDataAccess | 23.7.0 |
| AI LLM | Ollama (Qwen 2.5) | — |
| AI Embedding | bge-m3 (qua Ollama) | — |
| Vector DB | Qdrant | 1.12.1 (docker) |
| Password hashing | BCrypt.Net-Next | 4.0.3 |
| Testing | NUnit | 3.14.0 |

---

### Cài đặt nhanh

```text
1. Clone repository
2. Chuẩn bị Oracle Database (19c / 21c / 23ai Free)
   - Chạy các migration scripts trong database/migrations/ theo thứ tự
3. Cấu hình connection string
   - Copy .env.example → .env và điền thông tin kết nối
   - Hoặc cấu hình trực tiếp trong App.config / Web.config
4. Build solution trong Visual Studio (HRMS.sln)
5. Chạy API (HRMS.Api qua IIS Express hoặc start-api-service.bat)
6. Chạy Desktop (HRMS.Desktop)
7. Chạy Web: cd HRMS.Web && npm install && npm run dev
8. Chạy Mobile: cd HRMS.Mobile && npm install && npx expo start
```

**Docker (tùy chọn):** `docker-compose.yml` cung cấp Oracle 23ai Free, Qdrant, Ollama và Web (Nginx build) trong container.

Chi tiết cài đặt xem tại:
- [`docs/installation.md`](docs/installation.md)
- [`docs/deployment_guide.md`](docs/deployment_guide.md)
- [`docs/technical_reference.md`](docs/technical_reference.md)

---

### Current Status

- **Desktop** là client có nhiều chức năng nhất và đang được sử dụng làm giao diện nghiệp vụ chính.
- **Web** đã có nhiều trang quản trị nhưng vẫn đang được phát triển thêm.
- **Mobile** tập trung vào self-service (tra cứu cá nhân, gửi yêu cầu), chưa bao phủ các nghiệp vụ quản trị như Desktop.
- **AI** đang ở giai đoạn thử nghiệm — pipeline Hybrid RAG (NL2SQL + Vector Search) hoạt động nhưng chất lượng phụ thuộc vào model LLM và dữ liệu vector.
- Desktop phụ thuộc vào DevExpress WinForms (thư viện thương mại, cần license riêng).
- AI phụ thuộc vào Ollama (phải cài và chạy model Qwen 2.5 + bge-m3 local) và Qdrant.
- Test coverage hiện bao phủ một số thành phần (API, authentication, AI, payroll, overtime workflow) nhưng chưa bao phủ toàn bộ hệ thống.
- Project hiện đang trong quá trình phát triển; cần kiểm thử, cấu hình và đánh giá thêm trước khi triển khai production.

---

### Hướng phát triển

Dựa trên kiến trúc hiện tại và các phần đang phát triển trong repository:

- Hoàn thiện các chức năng Mobile self-service
- Tiếp tục phát triển Web client
- Mở rộng test coverage
- Cải thiện chất lượng AI retrieval (tuning prompt, mở rộng dữ liệu vector)
- Chuẩn hóa quy trình deployment
- Cải thiện documentation

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

HRMS is a human resource management system under active development. It provides three client interfaces — Desktop (WinForms/DevExpress), Web (React/Ant Design), and Mobile (React Native/Expo) — connected through a shared REST API (ASP.NET Web API 2) to an Oracle Database backend.

**Key areas currently implemented:**
- Employee records management, contracts, departments, positions
- Attendance tracking, shift types, payroll calculation
- Overtime, allowances, salary advances
- Rewards, discipline, salary adjustments, transfers
- RBAC authorization (5-action permission model: View, Add, Edit, Delete, Print)
- Audit logging, notification system, multi-language support (i18n)
- Reporting and dashboards (Desktop: DevExpress Charts + XtraReports, Web: Recharts)
- Approval workflow for employee requests (leave, attendance correction, overtime)
- Mobile self-service: personal profile, attendance, payroll, contracts, insurance, request submission
- Experimental AI assistant: Vietnamese natural language queries using local Ollama (Qwen 2.5) + Qdrant vector search

**Technology stack:** .NET Framework 4.7.2, ASP.NET Web API 2, Entity Framework 6.5, Oracle Database (19c/21c/23ai), React 19, Vite, Ant Design, React Native (Expo SDK 57), Ollama, Qdrant, BCrypt, JWT (HMAC-SHA256), NUnit.

**Current status:** Desktop is the most feature-complete client. Web is actively being developed. Mobile focuses on employee self-service. AI features are experimental. The project is under development and requires further testing and configuration before production deployment.

For detailed documentation, see the `docs/` directory.

---

## 🇯🇵 日本語

### 概要

HRMSは開発中の人事管理システムです。Desktop（WinForms/DevExpress）、Web（React/Ant Design）、Mobile（React Native/Expo）の3つのクライアントを提供し、共通のREST API（ASP.NET Web API 2）を通じてOracle Databaseに接続します。

**主な実装済み機能：**
- 従業員情報管理、契約、部署、役職
- 勤怠管理、シフト、給与計算
- 残業、手当、給与前払い
- 表彰・懲戒、昇給、異動
- RBAC権限管理（5操作モデル）
- 監査ログ、通知、多言語対応
- レポート・ダッシュボード
- モバイルセルフサービス（個人情報照会、申請送信）
- 実験的AI機能：ベトナム語自然言語クエリ（ローカルOllama + Qdrant）

**技術スタック：** .NET Framework 4.7.2、ASP.NET Web API 2、Entity Framework 6.5、Oracle Database、React 19、React Native (Expo SDK 57)、Ollama、Qdrant、BCrypt、JWT、NUnit

**現状：** Desktopが最も機能が充実しています。Webは開発中。Mobileは従業員セルフサービスに特化。AI機能は実験段階です。本番環境へのデプロイ前に、さらなるテストと設定が必要です。

詳細なドキュメントは `docs/` ディレクトリを参照してください。
