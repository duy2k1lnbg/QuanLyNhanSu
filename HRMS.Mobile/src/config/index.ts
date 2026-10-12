// Cấu hình máy chủ HRMS API:
// Sử dụng tên miền chính thức, không để lộ IP và Port máy chủ
export const VPS_DOMAIN_URL = 'https://tryhardagain.com/api/api';

export const APP_CONFIG = {
  appName: 'HRMS Mobile',
  version: '1.0.1',
  buildNumber: '2',
  env: 'production',
  apiBaseUrl: VPS_DOMAIN_URL,
  defaultApiBaseUrl: VPS_DOMAIN_URL,
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
