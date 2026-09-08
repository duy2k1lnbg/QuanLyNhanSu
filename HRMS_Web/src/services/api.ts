import axios from 'axios';

// Cấu hình URL gọi tới backend HRMS_API (ASP.NET Web API 2)
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44376/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
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

// Interceptor xử lý lỗi chung (401 hết hạn phiên, 500 lỗi máy chủ)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      console.warn('Phiên đăng nhập đã hết hạn hoặc chưa xác thực.');
    }
    return Promise.reject(error);
  }
);

export default api;
