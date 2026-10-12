import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { getInstallationId } from '../utils/deviceIdentity';
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
        const deviceId = await getInstallationId();
        config.headers['X-Device-Id'] = deviceId;
        config.headers['X-Installation-Id'] = deviceId;
        config.headers['X-Device-Name'] = `${Platform.OS.toUpperCase()} App`;
        config.headers['X-Correlation-Id'] = 'mob_' + Date.now().toString(36) + '_' + Math.random().toString(36).substring(2, 7);
      }
    } catch (e) {
      console.warn('[ApiClient] Request interceptor error:', e);
      return Promise.reject(e);
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

  // Tự động map 2 chiều các alias phổ biến giữa backend Oracle/Vietnamese và DTO Mobile
  if (out.id === undefined) {
    out.id = out.idYeuCau ?? out.id_yeucau ?? out.idUl ?? out.id_ul ?? out.idbh ?? out.id_bh;
  }
  if (out.idYeuCau === undefined && out.id !== undefined) {
    out.idYeuCau = out.id;
  }
  if (out.fromDate === undefined && out.tuNgay !== undefined) out.fromDate = out.tuNgay;
  if (out.tuNgay === undefined && out.fromDate !== undefined) out.tuNgay = out.fromDate;

  if (out.toDate === undefined && out.denNgay !== undefined) out.toDate = out.denNgay;
  if (out.denNgay === undefined && out.toDate !== undefined) out.denNgay = out.toDate;

  if (out.totalDays === undefined && out.soNgay !== undefined) out.totalDays = out.soNgay;
  if (out.soNgay === undefined && out.totalDays !== undefined) out.soNgay = out.totalDays;

  if (out.leaveType === undefined && out.loaiNghi !== undefined) out.leaveType = out.loaiNghi;
  if (out.loaiNghi === undefined && out.leaveType !== undefined) out.loaiNghi = out.leaveType;

  if (out.workDate === undefined && out.ngayCong !== undefined) out.workDate = out.ngayCong;
  if (out.ngayCong === undefined && out.workDate !== undefined) out.ngayCong = out.workDate;

  if (out.requestedCheckIn === undefined && out.gioVaoMoi !== undefined) out.requestedCheckIn = out.gioVaoMoi;
  if (out.gioVaoMoi === undefined && out.requestedCheckIn !== undefined) out.gioVaoMoi = out.requestedCheckIn;

  if (out.requestedCheckOut === undefined && out.gioRaMoi !== undefined) out.requestedCheckOut = out.gioRaMoi;
  if (out.gioRaMoi === undefined && out.requestedCheckOut !== undefined) out.gioRaMoi = out.requestedCheckOut;

  if (out.otDate === undefined && out.ngayTangCa !== undefined) out.otDate = out.ngayTangCa;
  if (out.ngayTangCa === undefined && out.otDate !== undefined) out.ngayTangCa = out.otDate;

  if (out.hours === undefined && out.soGio !== undefined) out.hours = out.soGio;
  if (out.soGio === undefined && out.hours !== undefined) out.soGio = out.hours;

  if (out.shiftType === undefined && out.tenCa !== undefined) out.shiftType = out.tenCa;
  if (out.tenCa === undefined && out.shiftType !== undefined) out.tenCa = out.shiftType;

  if (out.amount === undefined && out.soTienUng !== undefined) out.amount = out.soTienUng;
  if (out.soTienUng === undefined && out.amount !== undefined) out.soTienUng = out.amount;

  if (out.advanceDate === undefined && out.ngayUng !== undefined) out.advanceDate = out.ngayUng;
  if (out.ngayUng === undefined && out.advanceDate !== undefined) out.ngayUng = out.advanceDate;

  if (out.reason === undefined && (out.lyDo !== undefined || out.noiDung !== undefined || out.ghiChu !== undefined)) {
    out.reason = out.lyDo ?? out.noiDung ?? out.ghiChu;
  }
  if (out.lyDo === undefined && out.reason !== undefined) out.lyDo = out.reason;

  if (out.status === undefined && out.trangThai !== undefined) out.status = out.trangThai;
  if (out.trangThai === undefined && out.status !== undefined) out.trangThai = out.status;

  if (out.createdAt === undefined && out.ngayTao !== undefined) out.createdAt = out.ngayTao;
  if (out.ngayTao === undefined && out.createdAt !== undefined) out.ngayTao = out.createdAt;

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

// Migrate any old cached base URL that contains raw IP or port back to official domain
(async () => {
  try {
    const cachedUrl = await AppPreferencesStorage.getItem(APP_CONFIG.storageKeys.apiBaseUrl);
    if (cachedUrl && (cachedUrl.includes('103.200.22.79') || cachedUrl.includes(':5000') || cachedUrl.includes('192.168.') || cachedUrl.startsWith('http://tryhardagain.com'))) {
      await AppPreferencesStorage.setItem(APP_CONFIG.storageKeys.apiBaseUrl, APP_CONFIG.defaultApiBaseUrl);
      apiClient.defaults.baseURL = APP_CONFIG.defaultApiBaseUrl;
    }
  } catch {}
})();

// Response Interceptor: Handle 401 Unauthorized and normalize data
apiClient.interceptors.response.use(
  (response) => {
    if (response.data) {
      response.data = normalizeResponse(response.data);
    }
    return response;
  },
  async (error: AxiosError) => {
    if (error.response && error.response.status === 401 && !error.config?.url?.startsWith('/auth/')) {
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

