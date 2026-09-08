// Định nghĩa các Interface TypeScript tương ứng 1-1 với C# DTO trong Bu/DTO

export interface NhanVienDTO {
  MANV: number;
  HOTEN: string;
  GIOITINH: string;
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
  TRANGTHAI?: boolean;
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
}
