![image](https://github.com/user-attachments/assets/7a3468ce-931b-42fa-91e4-5c9898c5aaba)# My .NET Framework Project

## Mô tả dự án

Dự án này được khởi tạo vào ngày 01/07/2024, sử dụng các công nghệ sau:
- .NET Framework 4.7.2
- Oracle Database 19c
- Entity Framework 6 (EF6) 23.7.0
- Oracle.ManagedDataAccess.EntityFramework
- DevExpress cho Windows

## Yêu cầu hệ thống

- Visual Studio (phiên bản Community, Professional, hoặc Enterprise)
- Oracle Data Access Components (ODAC) 19c
- DevExpress

## Cài đặt và thiết lập

### 1. Cài đặt Visual Studio

Tải và cài đặt [Visual Studio](https://visualstudio.microsoft.com/downloads/). Chọn phiên bản phù hợp (Community, Professional, hoặc Enterprise).

### 2. Cài đặt Oracle Data Access Components (ODAC)

Tải và cài đặt [ODAC 19c](https://www.oracle.com/database/technologies/dotnet-odacdeploy-downloads.html).

### 3. Cài đặt DevExpress

Tải và cài đặt [DevExpress](https://www.devexpress.com/Downloads/NET/).

### 4. Cài đặt các gói NuGet cần thiết

Mở `NuGet Package Manager Console` và chạy các lệnh sau để cài đặt các gói:

```sh
Install-Package Oracle.ManagedDataAccess.EntityFramework -Version 23.7.0
