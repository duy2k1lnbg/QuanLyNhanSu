# HRMS Mobile v1 Build & Deployment Guide

## 1. Yêu cầu môi trường phát triển (Prerequisites)

| Công cụ | Phiên bản khuyến nghị | Ghi chú |
| :--- | :--- | :--- |
| **Node.js** | `>= 18.0.0` (Đang dùng v24.20.0) | Quản lý gói JavaScript |
| **Java JDK** | `OpenJDK 17` (LTS) | Bắt buộc cho Gradle & Android SDK |
| **Android SDK** | API Level 34 / 35 | Đi kèm Android Build Tools |
| **Android Studio** | Mới nhất | Dùng quản lý máy ảo (AVD) và SDK Tools |
| **Git** | Mới nhất | Quản lý phiên bản mã nguồn |

### 1.1. Cấu hình biến môi trường hệ thống

Đảm bảo các biến sau đã được thiết lập chính xác trong Windows Environment Variables:
```powershell
JAVA_HOME = "C:\Program Files\Microsoft\jdk-17.0.20.101-hotspot"
ANDROID_HOME = "C:\Users\<Tên_User>\AppData\Local\Android\Sdk"
Path += "%JAVA_HOME%\bin;%ANDROID_HOME%\platform-tools;%ANDROID_HOME%\cmdline-tools\latest\bin"
```

---

## 2. Cài đặt thư viện & Cấu hình Máy chủ API

### 2.1. Cài đặt dependencies

Di chuyển vào thư mục dự án Mobile và cài đặt các thư viện:
```powershell
cd HRMS.Mobile
npm install
```

### 2.2. Cấu hình địa chỉ máy chủ API (`src/config/index.ts`)

- **Khi chạy trên máy ảo Android (Emulator)**:
  Máy ảo Android dùng địa chỉ IP `10.0.2.2` để trỏ về `localhost` của máy tính chạy API.
  ```typescript
  export const APP_CONFIG = {
    apiBaseUrl: 'http://10.0.2.2:5000/api',
  };
  ```
- **Khi chạy trên điện thoại thật (Physical Device qua Wi-Fi)**:
  Sử dụng địa chỉ IP LAN của máy tính (Ví dụ: `192.168.3.178`).
  ```typescript
  export const APP_CONFIG = {
    apiBaseUrl: 'http://192.168.3.178:5000/api',
  };
  ```
- **Khi xuất bản Production**:
  ```typescript
  export const APP_CONFIG = {
    apiBaseUrl: 'https://tryhardagain.com/api/api',
  };
  ```

---

## 3. Khởi chạy trong chế độ Debug (Development)

### 3.1. Chạy với Expo Dev Server
```powershell
cd HRMS.Mobile
npm start
```
- Bấm `a` để mở trên máy ảo Android đang chạy.
- Dùng ứng dụng **Expo Go** trên điện thoại để quét mã QR (nếu dùng chế độ managed Expo).

### 3.2. Chạy trực tiếp Native Debug Build
```powershell
cd HRMS.Mobile
npx expo run:android
```

---

## 4. Quy trình đóng gói APK (Build APK)

### 4.1. Đóng gói Debug APK (Dùng để kiểm thử & cài đặt trực tiếp)

1. Sinh thư mục native Android (nếu chưa có):
   ```powershell
   npx expo prebuild --platform android
   ```
2. Cấu hình đường dẫn Android SDK trong `android/local.properties`:
   ```properties
   sdk.dir=C\:/Users/<Tên_User>/AppData/Local/Android/Sdk
   ```
3. Chạy lệnh Gradle để build APK:
   ```powershell
   cd android
   .\gradlew assembleDebug
   ```
4. **Vị trí file APK sau khi build thành công**:
   `HRMS.Mobile/android/app/build/outputs/apk/debug/app-debug.apk`

---

### 4.2. Cấu hình & Đóng gói Release APK (Chuẩn bị xuất bản Production)

1. **Tạo Keystore ký số (Signing Key)**:
   ```powershell
   keytool -genkeypair -v -storetype PKCS12 -keystore hrms-release-key.keystore -alias hrms-key-alias -keyalg RSA -keysize 2048 -validity 10000
   ```
   *Lưu ý: Không commit file `.keystore` hoặc mật khẩu lên kho mã nguồn công khai.*

2. **Cấu hình thông số ký trong `android/gradle.properties`**:
   ```properties
   HRMS_UPLOAD_STORE_FILE=hrms-release-key.keystore
   HRMS_UPLOAD_KEY_ALIAS=hrms-key-alias
   HRMS_UPLOAD_STORE_PASSWORD=*****
   HRMS_UPLOAD_KEY_PASSWORD=*****
   ```

3. **Cập nhật `android/app/build.gradle`**:
   ```groovy
   android {
       ...
       signingConfigs {
           release {
               if (project.hasProperty('HRMS_UPLOAD_STORE_FILE')) {
                   storeFile file(HRMS_UPLOAD_STORE_FILE)
                   storePassword HRMS_UPLOAD_STORE_PASSWORD
                   keyAlias HRMS_UPLOAD_KEY_ALIAS
                   keyPassword HRMS_UPLOAD_KEY_PASSWORD
               }
           }
       }
       buildTypes {
           release {
               signingConfig signingConfigs.release
               minifyEnabled true
               proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
           }
       }
   }
   ```

4. **Thực thi lệnh đóng gói Release APK**:
   ```powershell
   cd android
   .\gradlew assembleRelease
   ```
5. **Vị trí file APK Release**:
   `HRMS.Mobile/android/app/build/outputs/apk/release/app-release.apk`
