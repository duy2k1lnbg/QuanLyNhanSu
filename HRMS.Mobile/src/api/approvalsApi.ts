import { apiClient } from './client';
import { ENDPOINTS } from './endpoints';
import {
  ApprovalSummaryDto,
  LeaveApprovalItemDto,
  AttendanceCorrectionApprovalItemDto,
  OvertimeApprovalItemDto,
  InsuranceMovementApprovalItemDto,
} from '../types/me';

export interface ApprovalQueryFilter {
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedApprovalResult<T> {
  success: boolean;
  total: number;
  page: number;
  pageSize: number;
  data: T[];
}

export const approvalsApi = {
  async getSummary(): Promise<ApprovalSummaryDto> {
    const response = await apiClient.get<ApprovalSummaryDto>(ENDPOINTS.APPROVALS.SUMMARY);
    return response.data;
  },

  async getLeaveList(filter?: ApprovalQueryFilter): Promise<PagedApprovalResult<LeaveApprovalItemDto>> {
    const response = await apiClient.get<PagedApprovalResult<LeaveApprovalItemDto>>(
      ENDPOINTS.APPROVALS.LEAVE,
      { params: filter }
    );
    return response.data;
  },

  async approveLeave(id: number): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.LEAVE_APPROVE(id)
    );
    return response.data;
  },

  async rejectLeave(id: number, reason: string): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.LEAVE_REJECT(id),
      { reason }
    );
    return response.data;
  },

  async getAttendanceCorrections(
    filter?: ApprovalQueryFilter
  ): Promise<PagedApprovalResult<AttendanceCorrectionApprovalItemDto>> {
    const response = await apiClient.get<PagedApprovalResult<AttendanceCorrectionApprovalItemDto>>(
      ENDPOINTS.APPROVALS.CORRECTIONS,
      { params: filter }
    );
    return response.data;
  },

  async approveAttendanceCorrection(id: number): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.CORRECTIONS_APPROVE(id)
    );
    return response.data;
  },

  async rejectAttendanceCorrection(id: number, reason: string): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.CORRECTIONS_REJECT(id),
      { reason }
    );
    return response.data;
  },

  async getOvertimeList(filter?: ApprovalQueryFilter): Promise<PagedApprovalResult<OvertimeApprovalItemDto>> {
    const response = await apiClient.get<PagedApprovalResult<OvertimeApprovalItemDto>>(
      ENDPOINTS.APPROVALS.OVERTIME,
      { params: filter }
    );
    return response.data;
  },

  async approveOvertime(id: number): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.OVERTIME_APPROVE(id)
    );
    return response.data;
  },

  async rejectOvertime(id: number, reason: string): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.OVERTIME_REJECT(id),
      { reason }
    );
    return response.data;
  },

  async getInsuranceMovements(
    filter?: ApprovalQueryFilter
  ): Promise<PagedApprovalResult<InsuranceMovementApprovalItemDto>> {
    const response = await apiClient.get<PagedApprovalResult<InsuranceMovementApprovalItemDto>>(
      ENDPOINTS.APPROVALS.INSURANCE_MOVEMENTS,
      { params: filter }
    );
    return response.data;
  },

  async approveInsuranceMovement(id: number): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.INSURANCE_MOVE_APPROVE(id)
    );
    return response.data;
  },

  async rejectInsuranceMovement(id: number, reason: string): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      ENDPOINTS.APPROVALS.INSURANCE_MOVE_REJECT(id),
      { reason }
    );
    return response.data;
  },
};
