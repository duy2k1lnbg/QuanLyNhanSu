# HRMS Mobile v1 Architecture Documentation

## 1. Giới thiệu tổng quan

Phiên bản **HRMS Mobile v1** là ứng dụng di động dành cho toàn thể nhân viên doanh nghiệp, phục vụ tra cứu thông tin nhân sự, chấm công, bảng lương, hợp đồng, bảo hiểm xã hội và nhận thông báo nội bộ. Ứng dụng được thiết kế theo tiêu chuẩn Enterprise: giao diện hiện đại, trực quan, bảo mật cao và vận hành ổn định trên nền tảng Android (APK).

## 2. Kiến trúc tổng thể hệ thống

Hệ thống tuân thủ nghiêm ngặt nguyên tắc **KHÔNG tạo backend riêng** cho Mobile, sử dụng chung hạ tầng API và Cơ sở dữ liệu hiện có:

```
                  ┌─────────────────────────────────────────┐
                  │          Oracle Database 21c XE         │
                  └────────────────────┬────────────────────┘
                                       │
                                       ▼
                  ┌─────────────────────────────────────────┐
                  │                HRMS_API                 │
                  │   (ASP.NET Web API 2 / .NET 4.7.2)      │
                  │      JWT Bearer / RBAC / Self-Scope     │
                  └──────┬─────────────┼─────────────┬──────┘
                         │             │             │
                         ▼             ▼             ▼
                   ┌──────────┐  ┌───────────┐ ┌───────────────┐
                   │  QLyNSu  │  │ HRMS.Web  │ │  HRMS.Mobile │
                   │ WinForms │  │ React+TS  │ │ React Native  │
                   │ Desktop  │  │ Dashboard │ │ Android (APK) │
                   └──────────┘  └───────────┘ └───────────────┘
```

## 3. Cấu trúc thư mục Mobile (`/HRMS.Mobile`)

```
HRMS.Mobile/
├── App.tsx                    # Điểm khởi động ứng dụng & bọc Providers
├── app.json                   # Cấu hình Expo, Android package, permissions
├── package.json               # Dependencies & scripts
├── android/                   # Native Android project (Gradle, AndroidManifest)
└── src/
    ├── api/                   # Tầng giao tiếp HTTP với HRMS_API
    │   ├── client.ts          # Axios client, interceptors (Bearer token, Accept-Language, 401 handler)
    │   ├── endpoints.ts       # Định nghĩa URI endpoints
    │   ├── authApi.ts         # Login, change-password
    │   └── meApi.ts           # /api/me/* endpoints tự trích xuất MANV từ JWT
    ├── auth/
    │   └── AuthContext.tsx    # Context quản trị phiên đăng nhập an toàn
    ├── components/            # Design System Reusable Components
    │   ├── AppButton.tsx      # Nút bấm đa kích thước, biến thể, trạng thái loading
    │   ├── AppTextInput.tsx   # Ô nhập văn bản kèm icon, toggle ẩn/hiện mật khẩu
    │   ├── AppCard.tsx        # Khung viền thẻ bo tròn có đổ bóng theo theme
    │   ├── AppHeader.tsx      # Thanh tiêu đề màn hình kèm nút quay lại
    │   ├── AppAvatar.tsx      # Hiển thị ảnh chân dung hoặc chữ viết tắt họ tên
    │   ├── AppBadge.tsx       # Huy hiệu trạng thái (Success, Warning, Danger, Info)
    │   ├── AppDivider.tsx     # Đường kẻ phân cách
    │   ├── AppLoading.tsx     # Trạng thái đang tải dữ liệu
    │   ├── AppEmptyState.tsx  # Trạng thái rỗng không có dữ liệu
    │   └── AppErrorState.tsx  # Trạng thái lỗi kèm nút thử lại
    ├── constants/             # Hệ thống tokens thiết kế
    │   ├── colors.ts          # Bảng màu Light / Dark tương thích chuẩn WCAG
    │   ├── spacing.ts         # Quy chuẩn khoảng cách và bo góc
    │   └── typography.ts      # Kiểu chữ tiêu chuẩn doanh nghiệp
    ├── hooks/                 # Custom hooks truy xuất Contexts
    │   ├── useAuth.ts
    │   ├── useTheme.ts
    │   └── useLanguage.ts
    ├── i18n/                  # Hệ thống đa ngôn ngữ
    │   ├── i18n.ts            # Khởi tạo i18next & nhận diện locale Android
    │   ├── LanguageContext.tsx# Quản lý trạng thái chuyển đổi ngôn ngữ
    │   └── locales/
    │       ├── vi.json        # Tiếng Việt (Mặc định)
    │       ├── ja.json        # 日本語 (Tiếng Nhật)
    │       └── en.json        # English (Tiếng Anh)
    ├── navigation/            # Điều hướng màn hình
    │   ├── types.ts           # Type definitions cho Stack & Tab
    │   ├── RootNavigator.tsx  # Native Stack điều phối toàn bộ luồng
    │   └── MainTabNavigator.tsx # Bottom Tabs (Home, Attendance, Payroll, Notifications, Profile)
    ├── screens/               # 12 Màn hình nghiệp vụ chính
    │   ├── Splash/
    │   ├── Language/
    │   ├── Login/
    │   ├── Home/
    │   ├── Profile/
    │   ├── Attendance/
    │   ├── Payroll/
    │   ├── Contract/
    │   ├── Insurance/
    │   ├── Notifications/
    │   └── Settings/
    ├── theme/
    │   └── ThemeContext.tsx   # Cung cấp theme động (System / Light / Dark)
    ├── types/                 # TypeScript interfaces chuẩn hóa cùng Backend DTOs
    │   ├── auth.ts
    │   └── me.ts
    └── utils/
        ├── storage.ts         # SecureStore cho Token & AsyncStorage cho Preferences
        ├── formatters.ts      # Định dạng tiền tệ VND & Ngày tháng theo Locale
        └── errorMapper.ts     # Ánh xạ mã lỗi HTTP thân thiện không rò rỉ exception
```

## 4. Hệ thống Đa ngôn ngữ (Localization)

- Hỗ trợ 3 ngôn ngữ đầy đủ 100%: 🇻🇳 Tiếng Việt, 🇯🇵 日本語, 🇺🇸 English.
- **Quy trình khởi động:**
  1. Kiểm tra cấu hình ngôn ngữ đã lưu trong `storage.getLanguagePreference()`.
  2. Nếu là `system` (hoặc lần đầu mở app): tự động đọc ngôn ngữ thiết bị Android qua `expo-localization`.
  3. Nếu mã ngôn ngữ là `vi`, `ja` hoặc `en`: áp dụng ngôn ngữ tương ứng.
  4. Nếu là ngôn ngữ chưa được hỗ trợ: tự động fallback về Tiếng Việt (`vi`).
- Khi người dùng thay đổi ngôn ngữ: giao diện cập nhật ngay lập tức mà không làm mất phiên đăng nhập hay phải restart ứng dụng.

## 5. Hệ thống Giao diện (Theming System)

- Hỗ trợ 3 chế độ: **Theo hệ thống (System Default)**, **Sáng (Light Mode)**, **Tối (Dark Mode)**.
- Giao diện sử dụng 100% token trừu tượng từ `useTheme()`, đảm bảo không bị hardcode mã màu rời rạc, hỗ trợ chuyển đổi mượt mà giữa chế độ Sáng và Tối.

## 6. Luồng Điều hướng (Navigation Flow)

```
[Khởi động App]
       │
       ▼
 [SplashScreen]
       │
       ├─ Lần đầu mở app ──> [LanguageSelectScreen] ──> [LoginScreen]
       │
       ├─ Đã có Token & Session hợp lệ ──> [MainTabNavigator]
       │                                         ├── [HomeTab]
       │                                         ├── [AttendanceTab]
       │                                         ├── [PayrollTab]
       │                                         ├── [NotificationsTab]
       │                                         └── [ProfileTab]
       │
       └─ Chưa có Token / Hết hạn ──> [LoginScreen]
```
