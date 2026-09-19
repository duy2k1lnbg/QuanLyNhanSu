# HRMS Mobile v1 Security Architecture & Compliance

## 1. Nguyên tắc bảo mật cốt lõi

Phiên bản **HRMS Mobile v1** được xây dựng tuân thủ tiêu chuẩn an toàn bảo mật cấp doanh nghiệp, tập trung vào việc bảo vệ dữ liệu nhân sự, thông tin lương thưởng nhạy cảm và ngăn chặn triệt để các lỗ hổng OWASP Mobile Top 10.

---

## 2. Phòng chống IDOR & Cơ chế Self-Scoping tuyệt đối

Lỗ hổng phân quyền dữ liệu đối tượng gián tiếp (Insecure Direct Object Reference - IDOR) là nguy cơ phổ biến nhất trong các ứng dụng di động:

- **Quy tắc bắt buộc**: Server **KHÔNG BAO GIỜ** tin tưởng tham số `manv` được gửi từ Client (thông qua Query String, Request Body, Route URL hay Custom Header).
- **Luồng xác thực**:
  ```
  [Mobile Request] (Chỉ kèm Bearer Token trong Header)
         │
         ▼
  [JwtAuthorize Filter] ──> Giải mã & Xác thực chữ ký bí mật
         │
         ▼
  [ClaimsPrincipal] ──> Trích xuất claim 'id_user' & 'manv'
         │
         ▼
  [Database Query] ──> Lấy dữ liệu CHÍNH XÁC của MANV đã trích xuất
  ```
- **Kiểm thử thực tế**: Nhân viên A (`MANV 10025`) dù gửi request sửa đổi tham số nhằm xem bảng lương hay hồ sơ của nhân viên B (`MANV 10026`) cũng không thể can thiệp được vì Server luôn sử dụng `MANV` trích xuất từ JWT của nhân viên A.

---

## 3. Lưu trữ an toàn trên thiết bị di động (Secure Storage)

- **Lưu trữ Token**: Sử dụng thư viện `expo-secure-store`.
  - Trên Android: Dữ liệu được mã hóa bằng thuật toán AES-GCM với khóa mã hóa lưu trong **Android Keystore System** (khu vực bảo mật phần cứng TEE).
  - Tuyệt đối **KHÔNG dùng AsyncStorage** để lưu trữ: mật khẩu, JWT token, refresh token hay dữ liệu lương.
- **Lưu trữ Cấu hình**: Chỉ sử dụng `AsyncStorage` cho các tùy chọn không nhạy cảm:
  - Ngôn ngữ ưa thích (`hrms_language_preference`).
  - Giao diện người dùng (`hrms_theme_preference`).
  - Đánh dấu đã mở app lần đầu (`hrms_has_launched_before`).

---

## 4. Quản lý Mật khẩu & Xác thực

1. **Mã hóa một chiều**: Mật khẩu được mã hóa và kiểm tra bằng thuật toán **BCrypt** với Salt ngẫu nhiên.
2. **Không ghi log (Zero Logging)**:
   - Tuyệt đối không in mật khẩu ra Console log, Crash log hay Debug stream.
   - Tuyệt đối không log toàn văn token JWT.
3. **Thu hồi phiên làm việc (Revocation)**: Khi người dùng đổi mật khẩu thành công hoặc bấm Đăng xuất, token lưu trên SecureStore sẽ được hủy hoàn toàn và ứng dụng chuyển về màn hình Đăng nhập.

---

## 5. Phân quyền tối thiểu (Least Privilege)

Tài khoản Mobile chỉ được cấp các quyền xem dữ liệu cá nhân (`MOBILE_*_VIEW`):
- Không có quyền Admin hoặc HR Management.
- Không có quyền chỉnh sửa chấm công hoặc phê duyệt lương.
- Không có quyền truy cập dữ liệu của đồng nghiệp hoặc nhân viên khác.

---

## 6. Che giấu thông tin lỗi & Ngăn chặn rò rỉ ngoại lệ (Exception Masking)

Tầng `apiClient` và `errorMapper` trên ứng dụng di động thực hiện lọc toàn bộ thông điệp lỗi:
- Không bao giờ hiển thị lỗi nội bộ máy chủ, Stack Trace, cú pháp Entity Framework hay lỗi Oracle `ORA-xxxxx`.
- Các mã trạng thái HTTP được ánh xạ thành thông điệp thân thiện theo từng ngôn ngữ:
  - `401`: Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.
  - `403`: Bạn không có quyền truy cập chức năng này.
  - `404`: Không tìm thấy dữ liệu yêu cầu.
  - `408`: Kết nối quá thời gian quy định.
  - `500`: Hệ thống đang gặp sự cố. Vui lòng thử lại sau.
  - `Mất mạng`: Không thể kết nối máy chủ. Vui lòng kiểm tra lại mạng.
