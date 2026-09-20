export interface ProfileDto {
  manv: number;
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

export interface AttendanceDailyItemDto {
  ngay: string;
  thu: string;
  gioVao: string;
  gioRa: string;
  ngayCong: number;
  kyHieu: string;
  trangThai: string;
  ghiChu?: string | null;
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
  khoanTruKhac: number;
  tongKhauTru: number;
  thucLinh: number;
  trangThaiChiTra: string;
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

export interface InsuranceDto {
  idbh: number;
  sobh: string;
  ngaycap: string;
  noicap: string;
  noikhambenh: string;
  luongBhxh?: number | null;
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
