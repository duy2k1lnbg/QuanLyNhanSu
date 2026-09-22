import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { APP_CONFIG } from '../config';
import { Platform } from 'react-native';
import { storage, AppPreferencesStorage } from '../utils/storage';
import i18n from '../i18n/i18n';

export const apiClient = axios.create({
  baseURL: APP_CONFIG.apiBaseUrl,
  timeout: APP_CONFIG.requestTimeout,
  headers: {
    'Content-Type': 'application/json',
    'X-Client-Type': 'MOBILE',
  },
});

let cachedDeviceId: string | null = null;
async function getMobileDeviceId(): Promise<string> {
  if (cachedDeviceId) return cachedDeviceId;
  try {
    let id = await AppPreferencesStorage.getItem('hrms_mobile_device_id');
    if (!id) {
      id = 'mob_' + Platform.OS + '_' + Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
      await AppPreferencesStorage.setItem('hrms_mobile_device_id', id);
    }
    cachedDeviceId = id;
    return id;
  } catch {
    return 'mob_' + Platform.OS + '_default';
  }
}

let onUnauthorizedCallback: (() => void) | null = null;

export const setUnauthorizedHandler = (callback: () => void) => {
  onUnauthorizedCallback = callback;
};

// Request Interceptor: Attach JWT Token, Language and Device Security Headers
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    try {
      const token = await storage.getToken();
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }

      // Map language code to HTTP Accept-Language
      const lang = i18n.language || 'vi';
      let acceptLang = 'vi-VN';
      if (lang === 'ja') acceptLang = 'ja-JP';
      if (lang === 'en') acceptLang = 'en-US';
      if (lang === 'zh-CN') acceptLang = 'zh-CN';
      if (lang === 'ko') acceptLang = 'ko-KR';

      if (config.headers) {
        config.headers['Accept-Language'] = acceptLang;
        config.headers['X-Platform'] = Platform.OS.toUpperCase();
        config.headers['X-Device-Id'] = await getMobileDeviceId();
        config.headers['X-Device-Name'] = `${Platform.OS.toUpperCase()} App`;
        config.headers['X-Correlation-Id'] = 'mob_' + Date.now().toString(36) + '_' + Math.random().toString(36).substring(2, 7);
      }
    } catch (e) {
      console.warn('[ApiClient] Request interceptor error:', e);
    }
    return config;
  },
  (error) => Promise.reject(error)
);

function deepNormalize(obj: any): any {
  if (obj === null || obj === undefined || typeof obj !== 'object') return obj;
  if (Array.isArray(obj)) return obj.map(deepNormalize);

  const out: Record<string, any> = {};
  for (const key of Object.keys(obj)) {
    const camel = key.charAt(0).toLowerCase() + key.slice(1);
    const val = deepNormalize(obj[key]);
    out[camel] = val;
    out[key] = val;
  }
  return out;
}

function normalizeResponse(data: any): any {
  if (!data || typeof data !== 'object') return data;
  if ('data' in data) {
    if (Array.isArray(data.data)) {
      const normArr = data.data.map(deepNormalize);
      data.data = normArr;
      if (normArr.length > 0 && typeof normArr[0] === 'object') {
        Object.assign(data, normArr[0]);
      }
    } else if (data.data && typeof data.data === 'object') {
      const normObj = deepNormalize(data.data);
      data.data = normObj;
      Object.assign(data, normObj);
    }
  }
  return deepNormalize(data);
}

// Response Interceptor: Handle 401 Unauthorized, Auto-Failover on 404/Network Error, and normalize data
apiClient.interceptors.response.use(
  (response) => {
    if (response.data) {
      response.data = normalizeResponse(response.data);
    }
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as (InternalAxiosRequestConfig & { _retryCount?: number }) | undefined;

    // Tự động chuyển đổi giữa Domain (tryhardagain.com/api/api) và VPS IP (103.200.22.79:5000/api) khi gặp 404 hoặc lỗi mạng
    const isFailoverCandidate =
      !error.response ||
      error.response.status === 404 ||
      error.response.status === 502 ||
      error.response.status === 503 ||
      error.code === 'ECONNABORTED' ||
      error.code === 'ERR_NETWORK';

    if (originalRequest && isFailoverCandidate && (!originalRequest._retryCount || originalRequest._retryCount < 2)) {
      originalRequest._retryCount = (originalRequest._retryCount || 0) + 1;
      const currentUrl = (originalRequest.baseURL || apiClient.defaults.baseURL || '').toString();

      let targetUrl = APP_CONFIG.fallbackApiBaseUrl;
      if (currentUrl.includes('5000') || currentUrl.includes(APP_CONFIG.fallbackApiBaseUrl)) {
        targetUrl = APP_CONFIG.defaultApiBaseUrl;
      }

      console.warn(`[ApiClient] Request tới ${currentUrl} thất bại (${error.response?.status || error.code || 'lỗi kết nối'}). Tự động thử lại với ${targetUrl}`);
      apiClient.defaults.baseURL = targetUrl;
      originalRequest.baseURL = targetUrl;

      return apiClient(originalRequest);
    }

    if (error.response && error.response.status === 401) {
      console.warn('[ApiClient] 401 Unauthorized response received');
      await storage.removeToken();
      if (onUnauthorizedCallback) {
        onUnauthorizedCallback();
      }
    }
    return Promise.reject(error);
  }
);

export const getActiveApiUrl = (): string => {
  return (apiClient.defaults.baseURL as string) || APP_CONFIG.apiBaseUrl;
};

export const setActiveApiUrl = (url: string) => {
  apiClient.defaults.baseURL = url.trim().replace(/\/+$/, '');
};

