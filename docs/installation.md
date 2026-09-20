# Hướng Dẫn Cài Đặt & Phát Triển HRMS Enterprise

Tài liệu này hướng dẫn chi tiết quy trình thiết lập môi trường phát triển (Development Environment) và cài đặt đầy đủ các thành phần của hệ thống HRMS Enterprise.

---

## 1. Yêu Cầu Tiền Đề (Prerequisites)

| Thành phần | Phiên bản tối thiểu | Mục đích |
| :--- | :--- | :--- |
| **Hệ điều hành** | Windows 10/11 x64 hoặc Windows Server 2019+ | Chạy Visual Studio, WinForms, IIS Express |
| **Visual Studio** | 2022 (Community / Professional / Enterprise) | Build .NET Framework 4.7.2, Entity Framework 6 |
| **.NET SDK & Runtimes** | .NET Framework 4.7.2 Developer Pack, .NET 8 SDK | Chạy API Backend và bộ Unit/Benchmark Tests |
| **Node.js & npm** | Node.js 20+ LTS, npm 10+ | Chạy và build frontend React SPA |
| **Oracle Database** | Oracle Database 19c, 21c hoặc 23ai Free (Docker) | CSDL lõi lưu trữ nhân sự, tiền lương, chấm công |
| **Qdrant Vector DB** | v1.10+ | CSDL vector phục vụ tìm kiếm ngữ nghĩa RAG |
| **Ollama** | v0.3+ | Máy chủ LLM cục bộ chạy model `qwen2.5:latest` |

---

## 2. Khởi Động Các Dịch Vụ Phụ Trợ Qua Docker

Cách nhanh nhất để khởi tạo CSDL Oracle, Qdrant và Ollama là dùng `docker-compose`:

```bash
# 1. Sao chép cấu hình mẫu
copy .env.example .env

# 2. Khởi động các container phụ trợ
docker compose up -d oracle-db qdrant ollama

# 3. Tải mô hình Qwen 2.5 vào Ollama
docker exec -it hrms_ollama ollama pull qwen2.5:latest
```

Kiểm tra trạng thái các dịch vụ:
- Oracle Database: `localhost:1521` (User: `HR`, Pass: theo `.env`)
- Qdrant Dashboard: [http://localhost:6333/dashboard](http://localhost:6333/dashboard)
- Ollama API: [http://localhost:11434/api/tags](http://localhost:11434/api/tags)

---

## 3. Khởi Tạo Cơ Sở Dữ Liệu Oracle

Chạy các tập lệnh SQL theo thứ tự trong thư mục `database/migrations/`:
1. `V1_0__init_schema.sql` - Khởi tạo bảng, khóa chính, ràng buộc quan hệ
2. `V1_1__audit_triggers.sql` - Kích hoạt Trigger Audit Log tự động
3. `V1_2__ai_views.sql` - Tạo danh sách View an toàn cho AI RAG (`V_AI_*`)
4. `V1_3__schema_versioning.sql` - Đăng ký bảng đối soát phiên bản schema

---

## 4. Biên Dịch & Chạy Backend Web API

1. Mở giải pháp `QuanLyNhanSu.sln` bằng **Visual Studio 2022**.
2. Restore NuGet packages (hoặc chạy `nuget restore QuanLyNhanSu.sln`).
3. Kiểm tra chuỗi kết nối trong `HRMS_API/Web.config`:
   ```xml
   <connectionStrings>
     <add name="MyEntities" connectionString="metadata=res://*/HRMS_Model.csdl...;provider=Oracle.ManagedDataAccess.Client;provider connection string=&quot;DATA SOURCE=127.0.0.1:1521/xe;PASSWORD=YourPassword;USER ID=HR&quot;" providerName="System.Data.EntityClient" />
   </connectionStrings>
   ```
4. Thiết lập biến môi trường JWT Secret (khuyến nghị cho Production):
   ```powershell
   [System.Environment]::SetEnvironmentVariable("HRMS_JWT_SECRET", "YourSuperSecure256BitSecretKeyHere!", "Process")
   ```
5. Nhấn `F5` hoặc chọn Start `HRMS_API` để chạy qua IIS Express (mặc định cổng 5000 hoặc theo cấu hình IIS).

---

## 5. Chạy Kiểm Thử Tự Động (Unit & Benchmark Tests)

Chạy bộ kiểm thử tự động xác minh toàn diện hệ thống:

```bash
dotnet test Bu.Tests/Bu.Tests.csproj
```

Bộ kiểm thử bao gồm:
- **AiPromptInjectionTests**: Kiểm thử 100% kịch bản tiêm nhiễm Prompt, SQL Injection, gọi hàm DBMS Oracle nguy hiểm.
- **AiRetrievalBenchmarkTests**: Đo kiểm hiệu năng xử lý (AST Parser > 30,000 ops/s, Cache > 450,000 ops/s).
- **ApiSecurityIntegrationTests**: Kiểm tra mã hóa BCrypt, phân quyền RBAC và kiểm soát Data Scope.
- **DbQueryTests**: Kiểm tra tính toàn vẹn câu lệnh Linq EF trên CSDL Oracle.

---

## 6. Khởi Động Frontend React Web SPA

1. Di chuyển vào thư mục `HRMS.Web`:
   ```bash
   cd HRMS.Web
   npm install
   ```
2. Cấu hình file `.env`:
   ```env
   VITE_API_BASE_URL=http://localhost:5000/api
   ```
3. Khởi động môi trường phát triển (Hot Reload):
   ```bash
   npm run dev
   ```
4. Truy cập giao diện ứng dụng tại: [http://localhost:5173](http://localhost:5173)
   - Tài khoản mặc định: `admin` (hoặc tài khoản đã khởi tạo trong DB)
   - Mật khẩu: theo cấu hình đã băm BCrypt.

---

## 7. Khởi Động Client Mobile (React Native / Expo)

1. Di chuyển vào thư mục `HRMS.Mobile`:
   ```bash
   cd HRMS.Mobile
   npm install
   ```
2. Khởi động môi trường phát triển Metro / Expo:
   ```bash
   npm start
   ```
3. Chạy trên thiết bị Android hoặc máy ảo:
   ```bash
   npm run android
   ```

---

## 7. Khởi Động Client Desktop (WinForms)

1. Thiết lập project `QLyNSu` làm StartUp Project trong Visual Studio.
2. Kiểm tra chuỗi kết nối trong `QLyNSu/App.config`.
3. Nhấn `F5` để chạy ứng dụng WinForms với giao diện DevExpress.
