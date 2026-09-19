export interface LoginRequest {
  username: string;
  password: string;
  clientType?: string;
}

export interface UserSession {
  idUser: number;
  username: string;
  fullName: string;
  manv?: number | null;
  clientType?: string;
  isAdmin: boolean;
  rights: string[];
}

export interface LoginResponse {
  success: boolean;
  token: string;
  user: {
    idUser: number;
    username: string;
    fullName: string;
    isAdmin: boolean;
    rights: string[];
    manv?: number | null;
    clientType?: string;
  };
  message?: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}
