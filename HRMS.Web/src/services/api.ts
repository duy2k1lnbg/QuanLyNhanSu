import axios from 'axios';

// Tự động nhận diện API URL:
// 1. Biến môi trường VITE_API_BASE_URL (nếu có trong .env)
// 2. Môi trường Dev (port 5173 / 3000): sử dụng proxy '/api'
// 3. Môi trường Production / IIS (domain như tryhardagain.com hoặc port 80/443):
//    Vì backend được gắn là IIS sub-application '/api' và các Controller có RoutePrefix 'api/...',
//    nên endpoint chuẩn chính xác qua HTTPS là '/api/api'
const getInitialApiBaseUrl = (): string => {
  const envUrl = import.meta.env.VITE_API_BASE_URL;
  if (envUrl && typeof envUrl === 'string' && envUrl.trim() !== '') {
    return envUrl.trim();
  }

  if (typeof window !== 'undefined' && window.location) {
    const port = window.location.port;

    // Vite Dev proxy
    if (port === '5173' || port === '3000') {
      return '/api';
    }

    // Khi chạy trên Domain hoặc IIS sub-application:
    return '/api/api';
  }

  return '/api/api';
};

const api = axios.create({
  baseURL: getInitialApiBaseUrl(),
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor tự động gán Token vào header Authorization cho mọi request
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('hrms_token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor xử lý lỗi: Tự động chuyển đổi giữa '/api/api' và '/api' nếu gặp 404
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (!originalRequest) return Promise.reject(error);

    // Nếu request bị 404 (sai cấp độ route giữa IIS sub-app và root)
    if (!originalRequest._retry && error.response?.status === 404) {
      originalRequest._retry = true;
      if (originalRequest.baseURL === '/api/api') {
        console.warn(`[HRMS API] 404 tại '/api/api', đang tự động thử lại qua '/api'...`);
        api.defaults.baseURL = '/api';
        originalRequest.baseURL = '/api';
        return api(originalRequest);
      } else if (originalRequest.baseURL === '/api') {
        console.warn(`[HRMS API] 404 tại '/api', đang tự động thử lại qua '/api/api'...`);
        api.defaults.baseURL = '/api/api';
        originalRequest.baseURL = '/api/api';
        return api(originalRequest);
      }
    }

    if (error.response && error.response.status === 401) {
      console.warn('Phiên đăng nhập đã hết hạn hoặc chưa xác thực.');
      localStorage.removeItem('hrms_token');
      localStorage.removeItem('hrms_user');
    }
    return Promise.reject(error);
  }
);

export default api;
