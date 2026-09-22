import { apiClient } from './client';
import { ENDPOINTS } from './endpoints';
import { LoginRequest, LoginResponse, ChangePasswordRequest } from '../types/auth';

export const authApi = {
  async login(request: LoginRequest): Promise<LoginResponse> {
    const payload = {
      ...request,
      clientType: 'MOBILE',
    };
    const response = await apiClient.post<LoginResponse>(ENDPOINTS.AUTH.LOGIN, payload);
    return response.data;
  },

  async changePassword(request: ChangePasswordRequest): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.AUTH.CHANGE_PASSWORD,
      request
    );
    return response.data;
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post(ENDPOINTS.AUTH.LOGOUT);
    } catch (e) {
      console.warn('[authApi] logout network error:', e);
    }
  },

  async logoutAll(): Promise<{ success: boolean; revokedSessionsCount: number; message: string }> {
    const response = await apiClient.post<{ success: boolean; revokedSessionsCount: number; message: string }>(
      ENDPOINTS.AUTH.LOGOUT_ALL
    );
    return response.data;
  },
};
