// Định nghĩa các Interface TypeScript tương ứng 1-1 với C# DTO trong Bu/DTO

export interface NhanVienDTO {
  MANV: number;
  HOTEN: string;
  GIOITINH?: string;
  TENGT?: string;
  IDGT?: number;
  NGAYSINH?: string;
  DIENTHOAI?: string;
  CCCD?: string;
  DIACHI?: string;
  HINHANH?: string;
  IDPB?: number;
  TENPB?: string;
  IDBP?: number;
  TENBP?: string;
  IDCV?: number;
  TENCV?: string;
  IDTD?: number;
  TENTD?: string;
  DATHOIVIEC?: number;
  TRANGTHAI?: boolean;
}

export interface ActionItemDTO {
  id: string;
  title: string;
  count: number;
  urgency: 'urgent' | 'warning' | 'info';
  route: string;
  actionText: string;
}

export interface AnomalyItemDTO {
  id: string;
  employeeName: string;
  department: string;
  metric: string;
  severity: 'high' | 'medium' | 'low';
  description: string;
}

export interface EmployeeTimelineEventDTO {
  id: string;
  date: string;
  title: string;
  category: 'onboarding' | 'contract' | 'promotion' | 'leave' | 'salary' | 'award' | 'discipline';
  description: string;
  tagColor?: string;
}

export interface DashboardStatsDTO {
  tongNhanVien: number;
  tongQuyLuong: number;
  presentToday: number;
  absentToday: number;
  lateToday: number;
  phongBanStats: DashboardPhongBanDTO[];
  luongStats: DashboardLuongDTO[];
  actionItems: ActionItemDTO[];
  anomalies: AnomalyItemDTO[];
}

export interface DashboardPhongBanDTO {
  PhongBan: string;
  SoLuong: number;
}

export interface DashboardLuongDTO {
  KyCong: string;
  TongLuong: number;
}

export interface UserLoginInfoDTO {
  IDUSER: number;
  USERNAME: string;
  FULLNAME: string;
  ROLE: string;
  TOKEN?: string;
}

export interface AIChatMessage {
  id: string;
  sender: 'user' | 'assistant';
  content: string;
  timestamp: string;
  source?: string;
  sqlQuery?: string;
}

export interface KyCongDTO {
  MAKYCONG: number;
  THANG?: number;
  NAM?: number;
  KHOA?: number;
  NGAYCONGTRONGTHANG?: number;
  TRANGTHAI?: number;
  NGAYTINHCONG?: string;
}

export interface BangLuongDTO {
  IDBL: number;
  MANV: number;
  HOTEN: string;
  TENPB?: string;
  IDPB?: number;
  DATHOIVIEC?: number;
  KHOA?: number;
  TRANGTHAI_CHITRA?: string;
  MAKYCONG: number;
  THANG: number;
  NAM: number;
  CONG_CHUAN?: number;
  CONG_THUCTE?: number;
  CONG_LAMDEM?: number;
  DAILY_RATE?: number;
  DAILY_ALLOWANCE?: number;
  LUONG_CONG_THUCTE?: number;
  PHUCAP_CONG_THUCTE?: number;
  TIEN_TANGCA?: number;
  TIEN_CHUYENCAN?: number;
  TIEN_AN_CA?: number;
  KHOAN_CONG_KHAC?: number;
  TIEN_BHXH_TRICH?: number;
  TIEN_TAMUNG?: number;
  KHOAN_TRU_KHAC?: number;
  THUC_LINH?: number;
}

export interface KyCongChiTietDTO {
  MAKYCONG: number;
  MANV: number;
  HOTEN: string;
  TENPB?: string;
  DATHOIVIEC?: number;
  IS_ACTIVE?: boolean;
  D1?: string;
  D2?: string;
  D3?: string;
  D4?: string;
  D5?: string;
  D6?: string;
  D7?: string;
  D8?: string;
  D9?: string;
  D10?: string;
  D11?: string;
  D12?: string;
  D13?: string;
  D14?: string;
  D15?: string;
  D16?: string;
  D17?: string;
  D18?: string;
  D19?: string;
  D20?: string;
  D21?: string;
  D22?: string;
  D23?: string;
  D24?: string;
  D25?: string;
  D26?: string;
  D27?: string;
  D28?: string;
  D29?: string;
  D30?: string;
  D31?: string;
  NGAYCONG?: number;
  NGAYPHEP?: number;
  NGHIKHONGPHEP?: number;
  CONGNGAYLE?: number;
  CONGCHUNHAT?: number;
  TONGNGAYCONG?: number;
}

export interface HopDongDTO {
  SOHD: string;
  NGAYBATDAU?: string;
  NGAYKETTHUC?: string;
  NGAYKY?: string;
  LANKY?: number;
  HESOLUONG?: number;
  LUONG_THOA_THUAN?: number;
  THOIHAN?: string;
  NOIDUNG?: string;
  MANV?: number;
  HOTEN?: string;
  CCCD?: string;
  DIACHI?: string;
  NGAYSINH?: string;
  TENTD?: string;
  TENQT?: string;
  TENBP?: string;
  TENCV?: string;
  TENCTY?: string;
}

export interface LoaiCaDTO {
  IDLOAICA: number;
  TENLOAICA: string;
  HESOLOAICA?: number;
}

export interface LoaiCongDTO {
  IDLOAICONG: number;
  TENLOAICONG: string;
  HESOLOAICONG?: number;
}

export interface ComboDTO {
  ID: number;
  TEN_VI: string;
  TEN: string;
}

export interface DanhMucAllDTO {
  phongBan: { IDPB: number; TENPB: string; TENPB_VI?: string }[];
  chucVu: { IDCV: number; TENCV: string; TENCV_VI?: string }[];
  boPhan: ComboDTO[];
  trinhDo: ComboDTO[];
  danToc: ComboDTO[];
  tonGiao: ComboDTO[];
  quocTich: ComboDTO[];
}

// Interfaces Authentication & Authorization
export interface CurrentUserDTO {
  IdUser: number;
  Username: string;
  FullName: string;
  IsAdmin: boolean;
  Rights: string[];
}

export interface LoginResponseDTO {
  Success: boolean;
  Message: string;
  Token: string;
  User: CurrentUserDTO;
}

export interface SysUserDTO {
  IdUser: number;
  Username: string;
  FullName: string;
  IsGroup?: boolean;
  Disabled?: boolean;
  IsAdmin?: boolean;
  MemberCount?: number;
  Groups?: string[];
  MACTY?: string;
  MADVI?: string;
}

export interface GroupMemberItemDTO {
  IdUser: number;
  Username: string;
  FullName: string;
  Disabled?: boolean;
}

export interface GroupMembersResponseDTO {
  GroupId: number;
  GroupName: string;
  GroupFullName: string;
  Members: GroupMemberItemDTO[];
  AvailableUsers: GroupMemberItemDTO[];
}

export interface SysRightItemDTO {
  FuncCode: string;
  Description: string;
  Parent?: string;
  Sort?: number;
  HasRight: boolean;
}

export interface SysFunctionDTO {
  FuncCode: string;
  Description: string;
  Parent?: string;
  Sort?: number;
  Menu?: string;
}

// Interfaces Quản lý Nhân sự Nâng cao
export interface KhenThuongDTO {
  SOQD: string;
  LOAI?: number; // 1: Khen thưởng, 2: Kỷ luật
  MANV?: number;
  HOTEN?: string;
  NGAY?: string;
  NOIDUNG?: string;
  LYDO?: string;
  TUNGAY?: string;
  DENNGAY?: string;
}

export interface NangLuongDTO {
  SOQD: string;
  SOHD?: string;
  MANV?: number;
  HOTEN?: string;
  HESOLUONG_CU?: number;
  HESOLUONG_MOI?: number;
  NGAYKY?: string;
  NGAYLENLUONG?: string;
  GHICHU?: string;
}

export interface DieuChuyenDTO {
  SOQD: string;
  MANV?: number;
  HOTEN?: string;
  NGAY?: string;
  IDPB?: number;
  TENPB?: string;
  IDPB2?: number;
  TENPB2?: string;
  LYDO?: string;
  GHICHU?: string;
}

export interface UngLuongDTO {
  ID: number;
  NAM?: number;
  THANG?: number;
  NGAY?: string;
  SOTIEN?: number;
  TRANGTHAI?: number;
  MANV?: number;
  HOTEN?: string;
  GHICHU?: string;
}

export interface TangCaDTO {
  ID: number;
  NAM?: number;
  THANG?: number;
  NGAY?: string;
  SOGIO?: number;
  MANV?: number;
  HOTEN?: string;
  IDLOAICA?: number;
  TENLOAICA?: string;
  HESOLOAICA?: number;
  SOTIEN?: number;
  GHICHU?: string;
}
