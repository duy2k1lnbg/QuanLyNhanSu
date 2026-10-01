export interface ProfileDto {
  manv: number;
  employeeCode?: string;
  hoten: string;
  gioitinh: string;
  ngaysinh: string;
  dienthoai: string;
  cccd: string;
  diachi: string;
  tenPhongBan: string;
  tenBoPhan: string;
  tenChucVu: string;
  tenTrinhDo: string;
  ngayVaoLam: string;
  email: string;
  avatarBase64?: string | null;
  trangThaiLaoDong: string;
}

export interface UpdateProfileDto {
  dienthoai?: string;
  email?: string;
  diachi?: string;
  avatarBase64?: string;
}

export interface LeaveRequestDto {
  id: number;
  manv: number;
  leaveType: string;
  fromDate: string;
  toDate: string;
  totalDays: number;
  reason: string;
  status: 'PENDING' | 'APPROVED' | 'REJECTED';
  note?: string;
  createdAt: string;
  approverName?: string;
  approvedAt?: string;
}

export interface CreateLeaveRequestDto {
  leaveType: string;
  fromDate: string;
  toDate: string;
  totalDays: number;
  reason: string;
}

export interface AttendanceCorrectionDto {
  id: number;
  manv: number;
  workDate: string;
  requestedCheckIn?: string;
  requestedCheckOut?: string;
  reason: string;
  status: 'PENDING' | 'APPROVED' | 'REJECTED';
  note?: string;
  createdAt: string;
}

export interface CreateAttendanceCorrectionDto {
  workDate: string;
  requestedCheckIn?: string;
  requestedCheckOut?: string;
  reason: string;
}

export interface OvertimeRequestDto {
  id: number;
  manv: number;
  otDate: string;
  hours: number;
  idCa?: number;
  shiftType: string;
  tenCa?: string;
  coefficient: number;
  reason: string;
  status: 'PENDING' | 'APPROVED' | 'REJECTED' | 'CANCELLED';
  note?: string;
  createdAt: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
}

export interface CreateOvertimeRequestDto {
  otDate: string;
  hours: number;
  idCa?: number;
  shiftType?: string;
  reason: string;
}

export interface UnifiedRequestsDto {
  leaves: LeaveRequestDto[];
  corrections: AttendanceCorrectionDto[];
  overtimes: OvertimeRequestDto[];
}

export interface AttendanceDailyItemDto {
  ngay: string;
  thu: string;
  gioVao: string;
  gioRa: string;
  ngayCong: number;
  kyHieu: string;
  trangThai: string;
  ghiChu?: string | null;
  ngayPhep?: number | null;
  congNgayLe?: number | null;
  congChuNhat?: number | null;
  trangThaiCong?: string | null;
  duDieuKienChot?: boolean | null;
  gioThucTe?: number | null;
  gioHuongCong?: number | null;
  gioOtXacNhan?: number | null;
  gioDem?: number | null;
  phutDiMuonViPham?: number | null;
  phutVeSomViPham?: number | null;
  coBatThuongChuaXacMinh?: boolean;
}

export interface LeaveApprovalItemDto {
  id: number;
  idYeuCau: number;
  manv: number;
  employeeCode?: string;
  employeeName: string;
  departmentName: string;
  loaiNghi: string;
  tuNgay: string;
  denNgay: string;
  soNgay: number;
  lyDo: string;
  trangThai: 'PENDING' | 'APPROVED' | 'REJECTED';
  ngayTao: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
  lyDoTuChoi?: string;
}

export interface AttendanceCorrectionApprovalItemDto {
  id: number;
  idYeuCau: number;
  manv: number;
  employeeCode?: string;
  employeeName: string;
  departmentName: string;
  ngayCong: string;
  gioVaoMoi?: string;
  gioRaMoi?: string;
  lyDo: string;
  trangThai: 'PENDING' | 'APPROVED' | 'REJECTED';
  ngayTao: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
  lyDoTuChoi?: string;
}

export interface OvertimeApprovalItemDto {
  id: number;
  idYeuCau: number;
  manv: number;
  employeeCode?: string;
  employeeName: string;
  departmentName: string;
  ngayTangCa: string;
  soGio: number;
  heSo: number;
  idCa: number;
  tenCa: string;
  noiDung: string;
  trangThai: 'PENDING' | 'APPROVED' | 'REJECTED' | 'CANCELLED';
  ngayTao: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
  lyDoTuChoi?: string;
}

export interface InsuranceMovementApprovalItemDto {
  id: number;
  manv: number;
  employeeCode?: string;
  employeeName: string;
  departmentName: string;
  maKyCong: number;
  loai: 'TANG' | 'GIAM' | 'DIEU_CHINH' | 'TAM_DUNG' | string;
  ngayHieuLuc: string;
  lyDo: string;
  trangThai: 'DRAFT' | 'PENDING' | 'APPROVED' | 'REJECTED';
  nguoiDuyet?: string;
  idNguoiDuyet?: number;
}

export interface LeaveTransactionDto {
  id: number;
  ngay: string;
  loai: 'CAP' | 'SU_DUNG' | 'HOAN' | 'DIEU_CHINH' | 'HET_HAN' | string;
  giayPhep: number;
  soNgay: number;
  idDon?: number;
  lyDo: string;
}

export interface LeaveBalanceDto {
  manv: number;
  tongCapNgay: number;
  daDungNgay: number;
  conLaiNgay: number;
  transactions: LeaveTransactionDto[];
}

export interface ApprovalSummaryDto {
  totalPending: number;
  leavePending: number;
  attendancePending: number;
  overtimePending: number;
  insurancePending?: number;
}

export interface AttendanceSummaryDto {
  makycong: number;
  thang: number;
  nam: number;
  tongNgayCong: number;
  ngayCongChuan: number;
  congNgay: number;
  congDem: number;
  ngayPhep: number;
  congLe: number;
  congChuNhat: number;
  soGioTangCa: number;
  soLanDiMuon: number;
}

export interface AttendanceDto {
  summary: AttendanceSummaryDto;
  dailyList: AttendanceDailyItemDto[];
}

export interface PayrollDto {
  idbl: number;
  makycong: number;
  thang: number;
  nam: number;
  luongCoBan: number;
  congChuan: number;
  congThucTe: number;
  congLamNgay: number;
  congLamDem: number;
  soGioTangCa?: number;
  dailyRate: number;
  luongCaNgay: number;
  luongCaDem: number;
  luongCongThucTe: number;
  phuCapCongThucTe: number;
  tienTangCa: number;
  tienChuyenCan: number;
  tienAnCa: number;
  khoanCongKhac: number;
  tongThuNhap: number;
  tienBhxh: number;
  tienBhyt: number;
  tienBhtn: number;
  tienCongDoan: number;
  tienTamUng: number;
  thueTncn: number;
  hoanThue?: number;
  khoanTruKhac: number;
  tongKhauTru: number;
  thucLinh: number;
  trangThaiChiTra: string;

  // Tax breakdown
  thuNhapChiuThue?: number;
  thuNhapTinhThue?: number;
  giamTruBanThan?: number;
  giamTruPhuThuoc?: number;
  giamTruBaoHiem?: number;
  soNguoiPhuThuoc?: number;

  // Explanations & Policy
  cachTinhLuong?: string;
  cachTinhThue?: string;
  payrollVersion?: string;
  chinhSachApDung?: string;
  isLegacy?: boolean;
}

export interface ContractDto {
  sohd: string;
  loaihd?: number | null;
  tenLoaihd: string;
  ngaybatdau: string;
  ngayketthuc: string;
  ngayky: string;
  lanky: number;
  thoihan: string;
  luongThoaThuan?: number | null;
  heSoLuong?: number | null;
  noiDung?: string | null;
  isExpiringSoon: boolean;
}

export interface InsuranceMovementDto {
  id: number;
  manv: number;
  maKyCong: number;
  loai: string;
  ngayHieuLuc: string;
  lyDo: string;
  trangThai: string;
  nguoiDuyet?: number;
  tenNguoiDuyet?: string;
}

export interface InsuranceParticipationDto {
  id: number;
  vungLuong: number;
  thamGiaBhxh: boolean;
  thamGiaBhyt: boolean;
  thamGiaBhtn: boolean;
  thamGiaTnldBnn: boolean;
  huongTyLeTnldUuDai: boolean;
  luongDongBhxhRieng?: number | null;
  ngayBatDau: string;
  ngayKetThuc?: string | null;
  trangThai: string;
}

export interface UnionParticipationDto {
  id: number;
  laDoanVien: boolean;
  ngayGiaNhap: string;
  ngayKetThuc?: string | null;
  trangThai: string;
}

export interface InsuranceDto {
  idbh: number;
  sobh: string;
  ngaycap: string;
  noicap: string;
  noikhambenh: string;
  luongBhxh?: number | null;
  movements?: InsuranceMovementDto[];
  participations?: InsuranceParticipationDto[];
  unionParticipation?: UnionParticipationDto;
}

export interface NotificationDto {
  id: number;
  tieude: string;
  noidung: string;
  nguoidang: string;
  ngaydang: string;
  loaitb: string;
  isPinned: boolean;
  fileDinhkem?: string | null;
  isUnread: boolean;
}

export interface DashboardDto {
  profileSummary: ProfileDto;
  attendanceSummary: AttendanceSummaryDto;
  payrollSummary: PayrollDto;
  hasExpiringContract: boolean;
  expiringContractInfo?: string | null;
  unreadNotificationCount: number;
  recentNotifications: NotificationDto[];
}
