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

  async updateProfile(data: import('../types/me').UpdateProfileDto): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.put(ENDPOINTS.ME.PROFILE, data);
    return response.data;
  },

  async getLeaveRequests(): Promise<import('../types/me').LeaveRequestDto[]> {
    const response = await apiClient.get(ENDPOINTS.ME.LEAVE);
    return Array.isArray(response.data) ? response.data : (response.data?.data || []);
  },

  async createLeaveRequest(dto: import('../types/me').CreateLeaveRequestDto): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.LEAVE, dto);
    return response.data;
  },

  async getAttendanceCorrections(): Promise<import('../types/me').AttendanceCorrectionDto[]> {
    const response = await apiClient.get(ENDPOINTS.ME.ATTENDANCE_CORRECTIONS);
    return Array.isArray(response.data) ? response.data : (response.data?.data || []);
  },

  async createAttendanceCorrection(dto: import('../types/me').CreateAttendanceCorrectionDto): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.ATTENDANCE_CORRECTIONS, dto);
    return response.data;
  },

  async getOvertimeRequests(): Promise<import('../types/me').OvertimeRequestDto[]> {
    const response = await apiClient.get(ENDPOINTS.ME.OVERTIME);
    return Array.isArray(response.data) ? response.data : (response.data?.data || []);
  },

  async getOvertimeRequestDetail(id: number | string): Promise<import('../types/me').OvertimeRequestDto> {
    const response = await apiClient.get(ENDPOINTS.ME.OVERTIME_DETAIL(id));
    return response.data?.data || response.data;
  },

  async createOvertimeRequest(dto: import('../types/me').CreateOvertimeRequestDto): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.OVERTIME, dto);
    return response.data;
  },

  async cancelOvertimeRequest(id: number | string): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.OVERTIME_CANCEL(id));
    return response.data;
  },

  async getAllRequests(): Promise<import('../types/me').UnifiedRequestsDto> {
    const response = await apiClient.get(ENDPOINTS.ME.REQUESTS);
    return response.data?.data || { leaves: [], corrections: [], overtimes: [] };
  },
};
