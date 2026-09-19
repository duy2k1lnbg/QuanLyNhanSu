import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { APP_CONFIG } from '../config';
import { storage } from '../utils/storage';
import i18n from '../i18n/i18n';

export const apiClient = axios.create({
  baseURL: APP_CONFIG.apiBaseUrl,
  timeout: APP_CONFIG.requestTimeout,
  headers: {
    'Content-Type': 'application/json',
    'X-Client-Type': 'MOBILE',
  },
});

let onUnauthorizedCallback: (() => void) | null = null;

export const setUnauthorizedHandler = (callback: () => void) => {
  onUnauthorizedCallback = callback;
};

// Request Interceptor: Attach JWT Token and Language
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

      if (config.headers) {
        config.headers['Accept-Language'] = acceptLang;
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

// Response Interceptor: Handle 401 Unauthorized and normalize data
apiClient.interceptors.response.use(
  (response) => {
    if (response.data) {
      response.data = normalizeResponse(response.data);
    }
    return response;
  },
  async (error: AxiosError) => {
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
