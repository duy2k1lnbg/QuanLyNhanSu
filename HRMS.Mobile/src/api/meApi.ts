import { apiClient } from './client';
import { ENDPOINTS } from './endpoints';
import {
  ProfileDto,
  LeaveBalanceDto,
  AttendanceDto,
  PayrollDto,
  ContractDto,
  InsuranceDto,
  NotificationDto,
  DashboardDto,
  AllProfileQuotasDto,
  ProfileOperationResultDto,
  DependentDto,
  ProposeInsuranceDto,
} from '../types/me';
import { notifyNotificationsChanged } from '../utils/notificationEvents';
import { UserSession } from '../types/auth';
import { dependentPayload, normalizeRequests, isoDate, localDate, validClock } from '../utils/requestContracts';

export const meApi = {
  async getLeaveBalance(): Promise<LeaveBalanceDto> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.LEAVE_BALANCE);
    return response.data && Object.prototype.hasOwnProperty.call(response.data,'data') ? response.data.data : response.data;
  },

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

  async getPayrollResult(year?: number, month?: number): Promise<{payroll:PayrollDto|null;availability:string}> {
    const params = year && month ? {year,month} : undefined;
    const response=await apiClient.get<any>(ENDPOINTS.ME.PAYROLL,{params});
    const body=response.data;
    const payroll=body && Object.prototype.hasOwnProperty.call(body,'data') ? body.data : body;
    return {payroll,availability:payroll ? 'AVAILABLE' : (body?.availability ?? 'UNKNOWN')};
  },
  async getPayroll(year?: number, month?: number): Promise<PayrollDto|null> {
    return (await this.getPayrollResult(year,month)).payroll;
  },

  async getContract(): Promise<ContractDto> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.CONTRACT);
    const data = response.data && Object.prototype.hasOwnProperty.call(response.data,'data') ? response.data.data : response.data;
    if (Array.isArray(data)) {
      return data[0] || null;
    }
    return data;
  },

  async getInsurance(): Promise<InsuranceDto> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.INSURANCE);
    const data = response.data && Object.prototype.hasOwnProperty.call(response.data,'data') ? response.data.data : response.data;
    if (Array.isArray(data)) {
      return data[0] || null;
    }
    return data;
  },

  async getUnreadNotificationCount(): Promise<number> {
    try {
      const response = await apiClient.get('/me/notifications/unread-count');
      const count=Number(response.data?.data?.count ?? response.data?.count);
      if (!Number.isInteger(count) || count<0) throw new Error('mobileReview.invalidNotificationResponse');
      return count;
    } catch (error: any) {
      // Older deployed APIs expose the same authoritative count on dashboard.
      // Do not retry authorization, connection or server failures as a missing route.
      if (error.response?.status!==404) throw error;
      const response=await apiClient.get<any>(ENDPOINTS.ME.DASHBOARD);
      const dashboard=response.data?.data ?? response.data;
      const value=dashboard?.unreadNotificationCount ?? dashboard?.UnreadNotificationCount;
      if (value===null || value===undefined) throw error;
      const count=Number(value);
      if (!Number.isInteger(count) || count<0) throw error;
      return count;
    }
  },

  async getNotificationPage(offset = 0, unreadOnly = false): Promise<{items:NotificationDto[];unreadCount:number|null}> {
    const [items,count]=await Promise.allSettled([
      this.getNotifications(offset,unreadOnly), this.getUnreadNotificationCount(),
    ]);
    if (items.status==='rejected') throw items.reason;
    return {items:items.value,unreadCount:count.status==='fulfilled'?count.value:null};
  },

  async getOvertimeShifts(): Promise<{idCa:number;name:string;coefficient:number}[]> {
    const response=await apiClient.get('/me/overtime-shifts');
    return Array.isArray(response.data) ? response.data : response.data.data;
  },

  async getNotifications(offset = 0, unreadOnly = false): Promise<NotificationDto[]> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.NOTIFICATIONS, {params:{offset,limit:50,unreadOnly}});
    const data = response.data;
    if (Array.isArray(data)) {
      return data;
    }
    return data?.data || [];
  },

  async getNotificationDetail(id: number | string): Promise<NotificationDto> {
    const response = await apiClient.get<NotificationDto>(ENDPOINTS.ME.NOTIFICATION_DETAIL(id));
    notifyNotificationsChanged();
    return response.data;
  },

  async markNotificationAsRead(id: number | string): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.NOTIFICATION_READ(id));
    notifyNotificationsChanged();
    return response.data;
  },

  async markAllNotificationsAsRead(): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.NOTIFICATIONS_READ_ALL);
    notifyNotificationsChanged();
    return response.data;
  },

  async getProfileQuotas(): Promise<AllProfileQuotasDto> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.PROFILE_QUOTA);
    return response.data?.data || response.data;
  },

  async updateProfile(data: import('../types/me').UpdateProfileDto): Promise<ProfileOperationResultDto> {
    const response = await apiClient.put(ENDPOINTS.ME.PROFILE, {...data,ngaysinh:data.ngaysinh ? isoDate(data.ngaysinh):undefined});
    return response.data;
  },

  async uploadAvatar(avatarBase64: string): Promise<ProfileOperationResultDto> {
    const response = await apiClient.post(ENDPOINTS.ME.PROFILE_AVATAR, { avatarBase64 });
    return response.data;
  },

  async getProposedAvatar(): Promise<{ success: boolean; hasProposed: boolean; requestId?: number; avatarBase64?: string; createdAt?: string }> {
    const response = await apiClient.get(ENDPOINTS.ME.PROFILE_AVATAR_PROPOSED);
    return response.data;
  },

  async getDependents(): Promise<DependentDto[]> {
    const response = await apiClient.get<any>(ENDPOINTS.ME.DEPENDENTS);
    const data = response.data;
    if (Array.isArray(data)) return data;
    return data?.data || [];
  },

  async proposeDependents(dependents: DependentDto[], reason?: string): Promise<ProfileOperationResultDto> {
    const response = await apiClient.post(ENDPOINTS.ME.DEPENDENTS_PROPOSE, { dependents: dependents.map(dependentPayload), reason });
    return response.data;
  },

  async proposeInsurance(dto: ProposeInsuranceDto): Promise<ProfileOperationResultDto> {
    const response = await apiClient.post(ENDPOINTS.ME.INSURANCE_PROPOSE, dto);
    return response.data;
  },

  async getLeaveRequests(): Promise<import('../types/me').LeaveRequestDto[]> {
    const response = await apiClient.get(ENDPOINTS.ME.LEAVE);
    return Array.isArray(response.data) ? response.data : (response.data?.data || []);
  },

  async createLeaveRequest(dto: import('../types/me').CreateLeaveRequestDto): Promise<{ success: boolean; message?: string }> {
    if (isoDate(dto.toDate)<isoDate(dto.fromDate) || !Number.isFinite(dto.totalDays) || dto.totalDays<=0) throw new Error('mobileReview.invalidLeave');
    if ((dto.startTime || dto.endTime) && (!validClock(dto.startTime) || !validClock(dto.endTime))) throw new Error('mobileReview.invalidTime');
    const payload = {
      ...dto,
      fromDate: isoDate(dto.fromDate),
      tuNgay: isoDate(dto.fromDate),
      toDate: isoDate(dto.toDate),
      denNgay: isoDate(dto.toDate),
      leaveType: dto.leaveType,
      loaiNghi: dto.leaveType,
      totalDays: dto.totalDays,
      soNgay: dto.totalDays,
      reason: dto.reason,
      lyDo: dto.reason,
    };
    const response = await apiClient.post(ENDPOINTS.ME.LEAVE, payload);
    return response.data;
  },

  async getAttendanceCorrections(): Promise<import('../types/me').AttendanceCorrectionDto[]> {
    const response = await apiClient.get(ENDPOINTS.ME.ATTENDANCE_CORRECTIONS);
    return Array.isArray(response.data) ? response.data : (response.data?.data || []);
  },

  async createAttendanceCorrection(dto: import('../types/me').CreateAttendanceCorrectionDto): Promise<{ success: boolean; message?: string }> {
    if ((!dto.requestedCheckIn && !dto.requestedCheckOut) || (dto.requestedCheckIn && !validClock(dto.requestedCheckIn)) || (dto.requestedCheckOut && !validClock(dto.requestedCheckOut))) throw new Error('mobileReview.invalidTime');
    const payload = {
      ...dto,
      workDate: isoDate(dto.workDate),
      ngayCong: isoDate(dto.workDate),
      requestedCheckIn: dto.requestedCheckIn,
      gioVaoMoi: dto.requestedCheckIn,
      requestedCheckOut: dto.requestedCheckOut,
      gioRaMoi: dto.requestedCheckOut,
      reason: dto.reason,
      lyDo: dto.reason,
    };
    const response = await apiClient.post(ENDPOINTS.ME.ATTENDANCE_CORRECTIONS, payload);
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
    if (!Number.isFinite(dto.hours) || dto.hours<=0 || dto.hours>24 || !dto.idCa) throw new Error('mobileReview.invalidOvertime');
    const payload = {
      ...dto,
      otDate: isoDate(dto.otDate),
      ngayTangCa: isoDate(dto.otDate),
      hours: dto.hours,
      soGio: dto.hours,
      shiftType: dto.shiftType,
      tenCa: dto.shiftType,
      reason: dto.reason,
      lyDo: dto.reason,
      noiDung: dto.reason,
    };
    const response = await apiClient.post(ENDPOINTS.ME.OVERTIME, payload);
    return response.data;
  },

  async cancelOvertimeRequest(id: number | string): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.OVERTIME_CANCEL(id));
    return response.data;
  },

  async getAdvanceRequests(): Promise<import('../types/me').AdvanceSalaryDto[]> {
    try {
      const response = await apiClient.get(ENDPOINTS.ME.ADVANCE);
      return Array.isArray(response.data) ? response.data : (response.data?.data || []);
    } catch (err: any) {
      if (err?.response?.status === 404) {
        // Fallback: nếu /me/advance chưa có thì lấy từ danh sách /me/requests
        const all = await meApi.getAllRequests();
        return all.advances || [];
      }
      throw err;
    }
  },

  async createAdvanceRequest(dto: import('../types/me').CreateAdvanceSalaryDto): Promise<{ success: boolean; message?: string }> {
    const amount=dto.amount ?? dto.soTien ?? 0;
    if (!Number.isSafeInteger(amount) || amount<=0 || amount>100000000) throw new Error('mobileReview.invalidAmount');
    const payload = {
      ...dto,
      amount,
      soTien: amount,
      soTienUng: amount,
      advanceDate: isoDate(dto.advanceDate || localDate()),
      ngayUng: isoDate(dto.advanceDate || localDate()),
      reason: dto.reason,
      lyDo: dto.reason,
      ghiChu: dto.reason,
    };
    const response = await apiClient.post(ENDPOINTS.ME.ADVANCE, payload);
    return response.data;
  },

  async getAllRequests(): Promise<import('../types/me').UnifiedRequestsDto> {
    const response = await apiClient.get(ENDPOINTS.ME.REQUESTS);
    return normalizeRequests(response.data);
  },

  async registerDevice(dto: import('../types/me').RegisterDeviceDto): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.post(ENDPOINTS.ME.DEVICES_REGISTER, dto);
    return response.data;
  },

  async updateDevicePreferences(installationId: string, dto: import('../types/me').DevicePreferenceDto): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.put(ENDPOINTS.ME.DEVICE_PREFERENCES(installationId), dto);
    return response.data;
  },

  async deactivateDevice(installationId: string): Promise<{ success: boolean; message?: string }> {
    const response = await apiClient.delete(ENDPOINTS.ME.DEVICE_DELETE(installationId));
    return response.data;
  },

  async getMyDevices(): Promise<import('../types/me').UserDeviceInfoDto[]> {
    const response = await apiClient.get(ENDPOINTS.ME.DEVICES);
    return response.data?.data || response.data || [];
  },
};
