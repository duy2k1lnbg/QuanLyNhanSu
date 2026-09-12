# Cẩm Nang Triển Khai Môi Trường Sản Xuất (Production Deployment Guide)

Tài liệu này cung cấp hướng dẫn vận hành và triển khai chuẩn Enterprise cho hệ thống HRMS trên môi trường Production (IIS Server, Docker, SSL/TLS, Bảo mật hạ tầng và Giám sát).

---

## 1. Kiến Trúc Triển Khai Tổng Thể

```
                      [ INTERNET / INTRANET ]
                                 |
                                 v
                 [ Reverse Proxy: Nginx / Cloudflare ]
                        (SSL/TLS Offloading 443)
                       /                       \
                      /                         \
       [ Static SPA: HRMS_Web ]       [ REST API: HRMS_API ]
         (Nginx Docker / IIS)             (IIS Application)
                                                |
                               +----------------+----------------+
                               |                |                |
                               v                v                v
                        [ Oracle 19c/23ai ]  [ Qdrant ]     [ Ollama LLM ]
                          (Database Core)   (Vector Search)  (Qwen 2.5)
```

---

## 2. Triển Khai Backend Web API Trên IIS

### Bước 2.1: Chuẩn Bị Máy Chủ Windows Server
1. Cài đặt các tính năng Windows:
   - **Internet Information Services (IIS)**
   - **.NET Framework 4.7.2 Advanced Services** (ASP.NET 4.7)
   - **IIS URL Rewrite Module 2.1**
   - **Application Request Routing (ARR 3.0)** (nếu cần reverse proxy)

### Bước 2.2: Publish Ứng Dụng Web API
Sử dụng Visual Studio MSBuild để đóng gói bản phát hành:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" "d:\QL_NS\QuanLyNhanSu\HRMS_API\HRMS_API.csproj" /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Release /p:OutputPath="C:\inetpub\wwwroot\hrms-api"
```

### Bước 2.3: Thiết Lập IIS Application Pool
1. Mở **IIS Manager** (`inetmgr`).
2. Tạo mới Application Pool:
   - **Name**: `HRMS_Api_Pool`
   - **.NET CLR Version**: `.NET CLR Version v4.0.30319`
   - **Managed pipeline mode**: `Integrated`
3. Advanced Settings của Pool:
   - **Identity**: Chọn tài khoản có quyền truy cập Network / Oracle Client (ví dụ: `ApplicationPoolIdentity` hoặc tài khoản Service riêng).
   - **Idle Time-out (minutes)**: `0` (ngăn Pool tự tắt khi không có request).
   - **Recycling Time**: Đặt lịch tái khởi động vào ban đêm (ví dụ: 03:00 AM).

### Bước 2.4: Bảo Mật Cấu Hình Sản Xuất trong `Web.config`
Đảm bảo các cờ bảo mật đã được bật:
```xml
<system.web>
  <!-- BẮT BUỘC: Tắt chế độ Debug -->
  <compilation debug="false" targetFramework="4.7.2" />
  <!-- BẮT BUỘC: Ẩn chi tiết lỗi với client ngoài -->
  <customErrors mode="RemoteOnly" defaultRedirect="~/Error.html" />
  <httpRuntime targetFramework="4.7.2" enableVersionHeader="false" />
</system.web>
```

---

## 3. Quản Trị Khóa Bí Mật & Xoay Vòng Thông Số (Secrets Rotation)

### 3.1: Xoay Vòng JWT Secret (JWT Rotation Policy)
Không lưu JWT Secret cố định trong `Web.config`. Đặt thông qua biến môi trường hệ điều hành:

```powershell
# Thiết lập biến môi trường cấp Máy chủ (Machine level)
[Environment]::SetEnvironmentVariable("HRMS_JWT_SECRET", "Chuoi_Bi_Mat_Cuc_Ky_Dai_Toi_Thieu_64_Ky_Tu_Bao_Mat_Cao_2026_Enterprise!", "Machine")

# Khởi động lại IIS để nhận biến môi trường
iisreset
```

### 3.2: Bảo Vệ Chuỗi Kết Nối Cơ Sở Dữ Liệu
Sử dụng công cụ `aspnet_regiis` để mã hóa phần `<connectionStrings>` trong `Web.config`:

```powershell
cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319
.\aspnet_regiis.exe -pe "connectionStrings" -app "/hrms-api"
```

---

## 4. Triển Khai Frontend React Web SPA

### Cách 1: Triển Khai Bằng Nginx Docker Container
Sử dụng file `docker-compose.yml` có sẵn:
```bash
docker compose up -d --build hrms-web
```

### Cách 2: Triển Khai Trực Tiếp Trên IIS
1. Build gói bundle tĩnh:
   ```bash
   cd HRMS_Web
   npm run build
   ```
2. Sao chép toàn bộ thư mục `HRMS_Web/dist` vào thư mục web: `C:\inetpub\wwwroot\hrms-web`.
3. Thêm file `web.config` hỗ trợ định tuyến SPA (HTML5 PushState fallback):
   ```xml
   <?xml version="1.0" encoding="UTF-8"?>
   <configuration>
     <system.webServer>
       <rewrite>
         <rules>
           <rule name="SPA Routes" stopProcessing="true">
             <match url=".*" />
             <conditions logicalGrouping="MatchAll">
               <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
               <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
             </conditions>
             <action type="Rewrite" url="/" />
           </rule>
         </rules>
       </rewrite>
     </system.webServer>
   </configuration>
   ```

---

## 5. Vận Hành & Giám Sát Hệ Thống AI Copilot (Qdrant & Ollama)

### 5.1: Giám Sát Hàng Đợi Đồng Bộ (Qdrant Outbox Queue)
- Hệ thống áp dụng mẫu thiết kế **Outbox Pattern** với khả năng tự phục hồi (Self-Healing).
- Quản trị viên có thể kiểm tra trạng thái hàng đợi:
  - **API**: `GET /api/ai/outbox-status` (Yêu cầu quyền Admin)
  - **Kích hoạt đối soát**: `POST /api/ai/reconcile` (Đồng bộ toàn bộ dữ liệu nhân viên từ Oracle sang Qdrant).

### 5.2: Sao Lưu Dữ Liệu Vector (Qdrant Snapshots)
Tạo snapshot định kỳ cho collection `hrms_vectors`:
```bash
curl -X POST "http://localhost:6333/collections/hrms_vectors/snapshots"
```
File snapshot sẽ được lưu tại volume `qdrant_data`.
