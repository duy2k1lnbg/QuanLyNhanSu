import { apiClient } from './client';
import { ENDPOINTS } from './endpoints';
import {
  ProfileDto,
  AttendanceDto,
  PayrollDto,
  ContractDto,
  InsuranceDto,
  NotificationDto,
  DashboardDto,
} from '../types/me';
import { UserSession } from '../types/auth';

export const meApi = {
  async getMe(): Promise<UserSession> {
    const response = await apiClient.get<UserSession>(ENDPOINTS.ME.BASE);
    return response.data;
  },

  async getDashboard(): Promise<DashboardDto> {
    const response = await apiClient.get<DashboardDto>(ENDPOINTS.ME.DASHBOARD);
    return response.data;
  },

  async getProfile(): Promise<ProfileDto> {
    const response = await apiClient.get<ProfileDto>(ENDPOINTS.ME.PROFILE);
    return response.data;
  },

  async getAttendance(month?: string): Promise<AttendanceDto> {
    const params = month ? { month } : undefined;
    const response = await apiClient.get<AttendanceDto>(ENDPOINTS.ME.ATTENDANCE, { params });
    return response.data;
  },

  async getPayroll(year?: number, month?: number): Promise<PayrollDto> {
    const params = year && month ? { year, month } : undefined;
    const response = await apiClient.get<PayrollDto>(ENDPOINTS.ME.PAYROLL, { params });
    return response.data;
  },

  async getContract(): Promise<ContractDto> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.CONTRACT);
    const data = response.data;
    if (Array.isArray(data)) {
      return data[0] || null;
    }
    return data;
  },

  async getInsurance(): Promise<InsuranceDto> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.INSURANCE);
    const data = response.data;
    if (Array.isArray(data)) {
      return data[0] || null;
    }
    return data;
  },

  async getNotifications(): Promise<NotificationDto[]> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.NOTIFICATIONS);
    const data = response.data;
    if (Array.isArray(data)) {
      return data;
    }
    return data?.data || [];
  },

  async getNotificationDetail(id: number | string): Promise<NotificationDto> {
    const response = await apiClient.get<NotificationDto>(ENDPOINTS.ME.NOTIFICATION_DETAIL(id));
    return response.data;
  },
};
