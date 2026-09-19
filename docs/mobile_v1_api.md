# HRMS Mobile v1 API Reference & Specifications

## 1. Nguyên tắc thiết kế & Tự bảo mật (Self-Scope)

Tất cả các API dành cho ứng dụng di động đều tuân thủ nguyên tắc bảo mật tối cao:
- **Tự xác định danh tính (Strict Self-Scope)**: Server chỉ trích xuất thông tin nhân viên (`MANV`) từ token JWT đã được ký mật của người dùng hiện tại (`ClaimsPrincipal`). Tuyệt đối không chấp nhận tham số `manv` trong query string, request body, header tùy chỉnh hoặc route parameter để quyết định phạm vi dữ liệu.
- **Header xác thực**: `Authorization: Bearer <token>`
- **Client Type Header**: `X-Client-Type: MOBILE`
- **Ngôn ngữ**: `Accept-Language: vi-VN` (hoặc `ja-JP`, `en-US`)

---

## 2. Danh sách Endpoints

### 2.1. Đăng nhập (`POST /api/auth/login`)

- **Mô tả**: Xác thực tài khoản nhân viên dành cho Mobile.
- **Request Body**:
```json
{
  "username": "10025",
  "password": "Password123!",
  "clientType": "MOBILE"
}
```
- **Quy tắc nghiệp vụ**:
  - Kiểm tra mật khẩu bằng BCrypt hash.
  - Kiểm tra tài khoản bị vô hiệu (`IS_DISABLED == 1`) -> Từ chối đăng nhập.
  - Kiểm tra phân quyền truy cập Mobile (`CLIENT_TYPE == "DESKTOP"` bị từ chối trên Mobile).
  - Kiểm tra liên kết nhân viên: Nếu tài khoản chưa được liên kết với `MANV` -> Trả về mã lỗi 400 kèm thông báo: *"Tài khoản chưa được liên kết với hồ sơ nhân viên. Vui lòng liên hệ bộ phận nhân sự."* (Requirement 56).
- **Response thành công (200 OK)**:
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "idUser": 42,
    "username": "10025",
    "fullName": "Nguyễn Văn A",
    "isAdmin": false,
    "rights": ["MOBILE_PROFILE_VIEW", "MOBILE_ATTENDANCE_VIEW", "MOBILE_PAYROLL_VIEW", "MOBILE_CONTRACT_VIEW", "MOBILE_INSURANCE_VIEW", "MOBILE_NOTIFICATION_VIEW"],
    "manv": 10025,
    "clientType": "MOBILE"
  }
}
```

---

### 2.2. Đổi mật khẩu (`POST /api/auth/change-password`)

- **Mô tả**: Thay đổi mật khẩu của tài khoản hiện tại.
- **Request Body**:
```json
{
  "oldPassword": "CurrentPassword123!",
  "newPassword": "NewPassword456!"
}
```
- **Quy tắc nghiệp vụ**:
  - Yêu cầu xác thực JWT.
  - Kiểm tra mật khẩu cũ phải chính xác theo BCrypt.
  - Mật khẩu mới phải khác mật khẩu cũ và có độ dài tối thiểu 6 ký tự.

---

### 2.3. Thông tin phiên đăng nhập (`GET /api/me`)

- **Mô tả**: Trả về thông tin session hiện tại của người dùng.
- **Response (200 OK)**:
```json
{
  "idUser": 42,
  "username": "10025",
  "fullName": "Nguyễn Văn A",
  "manv": 10025,
  "clientType": "MOBILE",
  "isAdmin": false,
  "rights": ["MOBILE_PROFILE_VIEW", "MOBILE_ATTENDANCE_VIEW", "..."]
}
```

---

### 2.4. Tổng quan trang chủ (`GET /api/me/dashboard`)

- **Mô tả**: Cung cấp dữ liệu tóm tắt cho trang chủ (hồ sơ vắn tắt, chấm công tháng này, phiếu lương gần nhất, cảnh báo hợp đồng và thông báo mới nhất).
- **Response (200 OK)**:
```json
{
  "profileSummary": { ... },
  "attendanceSummary": {
    "makycong": 202609,
    "thang": 9,
    "nam": 2026,
    "tongNgayCong": 22.0,
    "ngayCongChuan": 22.0,
    "soLanDiMuon": 1
  },
  "payrollSummary": {
    "thang": 9,
    "nam": 2026,
    "thucLinh": 15800000.0,
    "trangThaiChiTra": "Đã chi trả"
  },
  "hasExpiringContract": false,
  "expiringContractInfo": null,
  "unreadNotificationCount": 2,
  "recentNotifications": [ ... ]
}
```

---

### 2.5. Hồ sơ nhân viên (`GET /api/me/profile`)

- **Mô tả**: Trả về chi tiết hồ sơ cá nhân của nhân viên đang đăng nhập.
- **Response (200 OK)**:
```json
{
  "manv": 10025,
  "hoten": "Nguyễn Văn A",
  "gioitinh": "Nam",
  "ngaysinh": "1995-08-15",
  "dienthoai": "0912345678",
  "cccd": "001095012345",
  "diachi": "Hà Nội, Việt Nam",
  "tenPhongBan": "Phòng Kỹ thuật",
  "tenBoPhan": "Bộ phận Phát triển Phần mềm",
  "tenChucVu": "Kỹ sư Phần mềm",
  "tenTrinhDo": "Đại học",
  "ngayVaoLam": "2021-03-01",
  "email": "nguyenvana@company.com",
  "avatarBase64": null,
  "trangThaiLaoDong": "Đang làm việc"
}
```

---

### 2.6. Bảng chấm công (`GET /api/me/attendance?month=2026-09`)

- **Mô tả**: Trả về tổng hợp ngày công và bảng chi tiết chấm công từng ngày trong tháng.
- **Response (200 OK)**:
```json
{
  "summary": {
    "makycong": 202609,
    "thang": 9,
    "nam": 2026,
    "tongNgayCong": 21.5,
    "ngayCongChuan": 22.0,
    "congNgay": 21.5,
    "congDem": 0.0,
    "ngayPhep": 0.5,
    "soLanDiMuon": 1
  },
  "dailyList": [
    {
      "ngay": "01/09",
      "thu": "T2",
      "gioVao": "08:02",
      "gioRa": "17:31",
      "ngayCong": 1.0,
      "kyHieu": "X",
      "trangThai": "Đủ công"
    }
  ]
}
```

---

### 2.7. Bảng lương cá nhân (`GET /api/me/payroll?year=2026&month=09`)

- **Mô tả**: Trả về chi tiết phiếu lương hàng tháng.
- **Response (200 OK)**:
```json
{
  "idbl": 105,
  "thang": 9,
  "nam": 2026,
  "luongCoBan": 12000000.0,
  "congChuan": 22.0,
  "congThucTe": 22.0,
  "luongCongThucTe": 12000000.0,
  "phuCapCongThucTe": 1500000.0,
  "tienTangCa": 500000.0,
  "tienChuyenCan": 500000.0,
  "tienAnCa": 730000.0,
  "tongThuNhap": 15230000.0,
  "tienBhxh": 960000.0,
  "tienBhyt": 180000.0,
  "tienBhtn": 120000.0,
  "tienCongDoan": 100000.0,
  "tienTamUng": 0.0,
  "thueTncn": 120000.0,
  "tongKhauTru": 1480000.0,
  "thucLinh": 13750000.0,
  "trangThaiChiTra": "Đã chi trả"
}
```

---

### 2.8. Hợp đồng lao động (`GET /api/me/contract`)

- **Mô tả**: Trả về hợp đồng lao động đang có hiệu lực.
- **Response (200 OK)**:
```json
{
  "sohd": "001/2024/HDLD",
  "tenLoaihd": "Hợp đồng không xác định thời hạn",
  "ngaybatdau": "2024-01-01",
  "ngayketthuc": "2027-01-01",
  "ngayky": "2023-12-25",
  "lanky": 2,
  "thoihan": "36 tháng",
  "luongThoaThuan": 15000000.0,
  "isExpiringSoon": false
}
```

---

### 2.9. Bảo hiểm xã hội (`GET /api/me/insurance`)

- **Mô tả**: Trả về thông tin sổ bảo hiểm xã hội và nơi đăng ký khám chữa bệnh ban đầu.
- **Response (200 OK)**:
```json
{
  "idbh": 50,
  "sobh": "0123456789",
  "ngaycap": "2021-04-10",
  "noicap": "BHXH TP. Hà Nội",
  "noikhambenh": "Bệnh viện Đa khoa Quốc tế Bạch Mai",
  "luongBhxh": 6000000.0
}
```

---

### 2.10. Thông báo nội bộ (`GET /api/me/notifications` & `GET /api/me/notifications/{id}`)

- **Mô tả**: Trả về danh sách thông báo và chi tiết thông báo nội bộ.
- **Response (200 OK)**:
```json
[
  {
    "id": 1,
    "tieude": "Thông báo lịch nghỉ lễ Quốc khánh",
    "noidung": "Toàn thể cán bộ nhân viên được nghỉ lễ theo quy định...",
    "nguoidang": "Phòng Nhân sự",
    "ngaydang": "2026-08-28",
    "loaitb": "Chung",
    "isPinned": true,
    "isUnread": false
  }
]
```
