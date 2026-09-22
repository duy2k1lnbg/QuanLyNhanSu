import { Platform } from 'react-native';

// Cấu hình máy chủ HRMS API:
// 1. VPS Domain (chuẩn Port 80 IIS sub-application /api & RoutePrefix 'api/...'):
export const VPS_DOMAIN_URL = 'http://tryhardagain.com/api/api';
// 2. VPS Direct IP (Port 5000 Backend ASP.NET):
export const VPS_DIRECT_IP_URL = 'http://103.200.22.79:5000/api';

const DEFAULT_DEV_HOST = Platform.OS === 'android' ? 'http://10.0.2.2:5000/api' : 'http://localhost:5000/api';
const DEFAULT_LAN_HOST = 'http://192.168.3.178:5000/api';

export const APP_CONFIG = {
  appName: 'HRMS Mobile',
  version: '1.0.0',
  buildNumber: '1',
  env: 'production',
  apiBaseUrl: VPS_DOMAIN_URL,
  defaultApiBaseUrl: VPS_DOMAIN_URL,
  fallbackApiBaseUrl: VPS_DIRECT_IP_URL,
  devEmulatorApiUrl: DEFAULT_DEV_HOST,
  productionApiUrl: VPS_DOMAIN_URL,
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
