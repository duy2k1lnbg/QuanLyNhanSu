import axios from 'axios';

// Cấu hình URL gọi tới backend: Tự động nhận diện đường dẫn API qua HTTPS của IIS sub-application (/api/api)
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/api';

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
      localStorage.removeItem('hrms_token');
      localStorage.removeItem('hrms_user');
    }
    return Promise.reject(error);
  }
);

export default api;
