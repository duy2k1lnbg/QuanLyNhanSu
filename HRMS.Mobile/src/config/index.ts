import { Platform } from 'react-native';

// Cấu hình máy chủ HRMS API
// - Khi chạy trên Android Emulator: 10.0.2.2 đại diện cho localhost của máy tính chủ
// - Khi chạy trên Điện thoại thật: Sử dụng IP LAN của máy tính (192.168.3.178)
// - Khi Production: Domain HTTPS chính thức
const DEFAULT_DEV_HOST = Platform.OS === 'android' ? 'http://10.0.2.2:5000/api' : 'http://localhost:5000/api';
const DEFAULT_LAN_HOST = 'http://192.168.3.178:5000/api';

export const APP_CONFIG = {
  appName: 'HRMS Mobile',
  version: '1.0.0',
  buildNumber: '1',
  env: 'development',
  apiBaseUrl: DEFAULT_LAN_HOST,
  defaultApiBaseUrl: DEFAULT_LAN_HOST,
  devEmulatorApiUrl: DEFAULT_DEV_HOST,
  productionApiUrl: 'https://tryhardagain.com/api/api',
  requestTimeout: 15000,
  storageKeys: {
    token: 'hrms_mobile_token',
    userSession: 'hrms_mobile_session',
    themePreference: 'hrms_theme_preference',
    theme: 'hrms_theme_preference',
    languagePreference: 'hrms_language_preference',
    language: 'hrms_language_preference',
    hasLaunchedBefore: 'hrms_has_launched_before',
    apiBaseUrl: 'hrms_custom_api_url',
  },
};
