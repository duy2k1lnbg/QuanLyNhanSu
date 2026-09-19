# HRMS Mobile v1 Database Changes & Migration Reference

## 1. Mục tiêu kiến trúc

Để phục vụ phiên bản Mobile mà **không phá vỡ hệ thống hiện tại** (gồm WinForms Desktop và React Web), cơ sở dữ liệu Oracle 21c được nâng cấp theo cơ chế Migration an toàn, có khả năng Rollback hoàn toàn:
1. Liên kết tài khoản hệ thống (`TB_SYS_USER`) với hồ sơ nhân viên (`TB_NHANVIEN`).
2. Giới hạn loại thiết bị được phép đăng nhập (`CLIENT_TYPE`).
3. Khởi tạo danh mục phân quyền nghiệp vụ dành riêng cho Mobile (`MOBILE_*`).
4. Đảm bảo toàn vẹn dữ liệu cho các tài khoản Quản trị, HR đã tồn tại.

---

## 2. Chi tiết Migration `V1_12__mobile_user_employee_link.sql`

File thực thi: `database/migrations/V1_12__mobile_user_employee_link.sql`

### 2.1. Cấu trúc bảng `TB_SYS_USER`

Bổ sung 2 trường mới:
- **`MANV`** (`NUMBER(10, 0) NULL`):
  - Khóa ngoại liên kết tới `TB_NHANVIEN(MANV)`.
  - Được cấu hình `NULLABLE` để các tài khoản hệ thống đặc biệt (như `admin`, tài khoản tích hợp nội bộ) không bị ảnh hưởng hoặc ép buộc phải có nhân viên.
- **`CLIENT_TYPE`** (`VARCHAR2(20) DEFAULT 'ALL' NOT NULL`):
  - Giá trị cho phép: `'ALL'` (cho phép mọi nền tảng), `'DESKTOP'` (chỉ WinForms), `'WEB'` (chỉ Web), `'MOBILE'` (chỉ Ứng dụng di động).
  - Giá trị mặc định là `'ALL'` giúp toàn bộ tài khoản hiện tại tiếp tục hoạt động bình thường trên cả Desktop và Web.

### 2.2. Khóa ngoại và Chỉ mục (Foreign Key & Index)

```sql
ALTER TABLE TB_SYS_USER
  ADD CONSTRAINT FK_SYS_USER_NHANVIEN
  FOREIGN KEY (MANV)
  REFERENCES TB_NHANVIEN (MANV)
  ON DELETE SET NULL;

CREATE INDEX IX_SYS_USER_MANV ON TB_SYS_USER(MANV);
```

### 2.3. Cấp quyền Mobile (Mobile Functions Seed)

Bổ sung các mã chức năng tự phục vụ vào bảng danh mục chức năng `TB_SYS_FUNCTION`:
- `MOBILE_PROFILE_VIEW`: Quyền xem hồ sơ nhân viên cá nhân.
- `MOBILE_ATTENDANCE_VIEW`: Quyền xem bảng chấm công cá nhân.
- `MOBILE_PAYROLL_VIEW`: Quyền xem phiếu lương cá nhân.
- `MOBILE_CONTRACT_VIEW`: Quyền xem hợp đồng lao động cá nhân.
- `MOBILE_INSURANCE_VIEW`: Quyền xem thông tin sổ bảo hiểm xã hội cá nhân.
- `MOBILE_NOTIFICATION_VIEW`: Quyền xem và nhận thông báo nội bộ.

---

## 3. Bản Rollback `V1_12_rollback__revert_mobile_user_employee_link.sql`

File thực thi: `database/migrations/V1_12_rollback__revert_mobile_user_employee_link.sql`

Khi cần khôi phục lại trạng thái ban đầu:
1. Xóa các quyền Mobile đã cấp trong `TB_SYS_USER_PERM`.
2. Xóa các chức năng Mobile trong `TB_SYS_FUNCTION`.
3. Xóa index `IX_SYS_USER_MANV`.
4. Drop constraint `FK_SYS_USER_NHANVIEN`.
5. Drop 2 cột `MANV` và `CLIENT_TYPE` khỏi bảng `TB_SYS_USER`.

---

## 4. Tương thích Entity Framework & Data Access Layer (`DA`)

1. Model EDMX `DA/QLNhanSu.edmx` đã được đồng bộ cả 3 tầng:
   - SSDL (Store Schema Definition Language).
   - CSDL (Conceptual Schema Definition Language).
   - MSL (Mapping Specification Language).
2. Entity class `DA/TB_SYS_USER.cs` bổ sung:
   ```csharp
   public Nullable<decimal> MANV { get; set; }
   public string CLIENT_TYPE { get; set; }
   public virtual TB_NHANVIEN TB_NHANVIEN { get; set; }
   ```
3. Lớp nghiệp vụ `Bu/CLASS_SYSTEM/SYS_USER.cs` hỗ trợ cập nhật và đọc `MANV` và `CLIENT_TYPE` trong các phương thức `getItem()`, `AddNew()`, `Update()`, `EnsureSeeded()`.
