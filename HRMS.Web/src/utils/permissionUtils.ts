import type { CurrentUserDTO, UserRightDetail } from '../types/hrms';

export type PermissionAction = 'VIEW' | 'ADD' | 'EDIT' | 'DELETE' | 'PRINT';

/**
 * Bản đồ liên kết phân hệ (Route) tới các mã chức năng hệ thống (TB_SYS_FUNCTION)
 */
export const ROUTE_FUNCTION_CODES: Record<string, string[]> = {
  dashboard: ['F_DB_NHANSU', 'F_DB_LUONG', 'F_BC_BAOCAO', 'BAOCAO', 'DASHBOARD'],
  nhanvien: ['F_DM_NHANVIEN', 'F_NV_NHANVIEN', 'NV', 'NHANVIEN'],
  chamcong: ['F_CC_BANGCONG', 'F_CC_BCCT', 'F_CC_LOAICA', 'F_CC_LOAICONG', 'F_CC_NGAYLE', 'F_CC_PHUCAP', 'CHAMCONG'],
  bangluong: ['F_CC_BANGLUONG', 'BANGLUONG', 'LUONG'],
  hopdong: ['F_NV_HOPDONG', 'F_NV_LOAIHOPDONG', 'HOPDONG'],
  khenthuong: ['F_NV_KHENTHUONG', 'F_NV_KYLUAT', 'KHENTHUONG', 'KYLUAT'],
  nangluong: ['F_NV_NANGLUONG', 'F_NV_DIEUCHUYEN', 'NANGLUONG', 'DIEUCHUYEN'],
  ungluong: ['F_CC_UNGLUONG', 'F_CC_TANGCA', 'UNGLUONG', 'TANGCA'],
  approvals: ['F_CC_BANGCONG', 'F_CC_TANGCA', 'F_NV_NHANVIEN', 'APPROVE', 'CHAMCONG'],
  phanquyen: ['F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'F_SYSTEM_LOCK_USER', 'PHANQUYEN'],
  audit: ['F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'PHANQUYEN', 'AUDIT'],
};

/**
 * Ánh xạ các bí danh phổ biến sang mã chức năng chuẩn trong Oracle CSDL
 */
export const FUNCTION_CODE_ALIASES: Record<string, string[]> = {
  NV: ['F_DM_NHANVIEN', 'F_NV_NHANVIEN'],
  NHANVIEN: ['F_DM_NHANVIEN'],
  HOPDONG: ['F_NV_HOPDONG'],
  CHAMCONG: ['F_CC_BANGCONG', 'F_CC_BCCT'],
  BANGLUONG: ['F_CC_BANGLUONG'],
  LUONG: ['F_CC_BANGLUONG'],
  KHENTHUONG: ['F_NV_KHENTHUONG'],
  KYLUAT: ['F_NV_KYLUAT'],
  NANGLUONG: ['F_NV_NANGLUONG'],
  DIEUCHUYEN: ['F_NV_DIEUCHUYEN'],
  UNGLUONG: ['F_CC_UNGLUONG'],
  TANGCA: ['F_CC_TANGCA'],
  PHANQUYEN: ['F_SYSTEM_USER', 'F_SYSTEM_GROUP'],
  USER: ['F_SYSTEM_USER'],
  DASHBOARD: ['F_DB_NHANSU', 'F_DB_LUONG'],
  BAOCAO: ['F_BC_BAOCAO'],
  AI: ['F_SYSTEM_AI'],
};

/**
 * Kiểm tra xem người dùng có phải là Quản trị viên tối cao (Super Admin) hay không.
 * Super Admin luôn có toàn quyền tuyệt đối đối với mọi chức năng và thao tác.
 */
export function isSuperAdmin(user: CurrentUserDTO | null | undefined): boolean {
  if (!user) return false;
  return Boolean(
    user.IsAdmin ||
    user.Username?.trim().toUpperCase() === 'ADMIN' ||
    user.Rights?.includes('*')
  );
}

/**
 * Tìm kiếm chi tiết phân quyền của một mã chức năng trong DetailedRights của người dùng
 */
function findRightDetail(
  detailedRights: Record<string, UserRightDetail> | undefined,
  funcCode: string
): UserRightDetail | undefined {
  if (!detailedRights || typeof detailedRights !== 'object') return undefined;

  const upperCode = funcCode.trim().toUpperCase();

  // 1. Tìm trực tiếp theo key chính xác
  for (const [key, val] of Object.entries(detailedRights)) {
    if (key.trim().toUpperCase() === upperCode) {
      return val;
    }
  }

  // 2. Tìm theo bí danh nếu có
  const aliases = FUNCTION_CODE_ALIASES[upperCode] || [];
  for (const alias of aliases) {
    for (const [key, val] of Object.entries(detailedRights)) {
      if (key.trim().toUpperCase() === alias.toUpperCase()) {
        return val;
      }
    }
  }

  return undefined;
}

/**
 * Kiểm tra quyền hạn theo 5 thao tác cơ bản: VIEW, ADD, EDIT, DELETE, PRINT
 */
export function checkPermission(
  user: CurrentUserDTO | null | undefined,
  action: PermissionAction,
  ...funcCodes: string[]
): boolean {
  if (!user) return false;
  if (isSuperAdmin(user)) return true;
  if (funcCodes.length === 0) return false;

  return funcCodes.some((code) => {
    if (!code) return false;
    const upperCode = code.trim().toUpperCase();

    // 1. Kiểm tra từ DetailedRights (5 quyền chi tiết từ CSDL)
    if (user.DetailedRights) {
      const detail = findRightDetail(user.DetailedRights, upperCode);
      if (detail) {
        switch (action) {
          case 'VIEW':
            return Boolean(detail.CanView ?? detail.CAN_VIEW);
          case 'ADD':
            return Boolean(detail.CanAdd ?? detail.CAN_ADD);
          case 'EDIT':
            return Boolean(detail.CanEdit ?? detail.CAN_EDIT);
          case 'DELETE':
            return Boolean(detail.CanDelete ?? detail.CAN_DELETE);
          case 'PRINT':
            return Boolean(detail.CanPrint ?? detail.CAN_PRINT);
        }
      }
    }

    // 2. Fallback cho quyền VIEW thông qua mảng Rights truyền thống
    if (action === 'VIEW' && Array.isArray(user.Rights)) {
      const hasLegacyRight = user.Rights.some((r) => {
        const ur = r.trim().toUpperCase();
        if (ur === upperCode) return true;
        const aliases = FUNCTION_CODE_ALIASES[upperCode] || [];
        return aliases.some((a) => a.toUpperCase() === ur);
      });
      if (hasLegacyRight) return true;
    }

    return false;
  });
}

/**
 * Kiểm tra quyền XEM (VIEW)
 */
export function canView(user: CurrentUserDTO | null | undefined, ...funcCodes: string[]): boolean {
  return checkPermission(user, 'VIEW', ...funcCodes);
}

/**
 * Kiểm tra quyền THÊM MỚI (ADD)
 */
export function canAdd(user: CurrentUserDTO | null | undefined, ...funcCodes: string[]): boolean {
  return checkPermission(user, 'ADD', ...funcCodes);
}

/**
 * Kiểm tra quyền SỬA / CHỈNH SỬA / TÍNH TOÁN (EDIT)
 */
export function canEdit(user: CurrentUserDTO | null | undefined, ...funcCodes: string[]): boolean {
  return checkPermission(user, 'EDIT', ...funcCodes);
}

/**
 * Kiểm tra quyền XÓA / THÔI VIỆC / HỦY (DELETE)
 */
export function canDelete(user: CurrentUserDTO | null | undefined, ...funcCodes: string[]): boolean {
  return checkPermission(user, 'DELETE', ...funcCodes);
}

/**
 * Kiểm tra quyền IN / XUẤT EXCEL / BÁO CÁO (PRINT)
 */
export function canPrint(user: CurrentUserDTO | null | undefined, ...funcCodes: string[]): boolean {
  return checkPermission(user, 'PRINT', ...funcCodes);
}

/**
 * Bí danh tương thích ngược cho hasRight (kiểm tra quyền XEM)
 */
export function hasRight(user: CurrentUserDTO | null | undefined, ...funcCodes: string[]): boolean {
  return canView(user, ...funcCodes);
}

/**
 * Kiểm tra quyền truy cập vào một Route cụ thể
 */
export function canAccessRoute(user: CurrentUserDTO | null | undefined, route: string): boolean {
  if (!user) return false;
  if (isSuperAdmin(user)) return true;

  const cleanRoute = route.replace(/^#?\/?/, '').trim().toLowerCase();
  const codes = ROUTE_FUNCTION_CODES[cleanRoute];

  // Nếu route không nằm trong danh sách cần bảo vệ nghiêm ngặt, cho phép truy cập
  if (!codes || codes.length === 0) return true;

  // Cần ít nhất 1 quyền Xem (VIEW) trong danh sách mã chức năng của Route
  return canView(user, ...codes);
}

/**
 * Lấy danh sách các route mà người dùng hiện tại được phép truy cập
 */
export function getPermittedRoutes(user: CurrentUserDTO | null | undefined): string[] {
  if (!user) return [];
  return Object.keys(ROUTE_FUNCTION_CODES).filter((r) => canAccessRoute(user, r));
}

/**
 * Lấy route đầu tiên mà người dùng có quyền xem (dùng để fallback khi truy cập trái phép)
 */
export function getFirstAccessibleRoute(
  user: CurrentUserDTO | null | undefined,
  defaultPreferred: string = 'dashboard'
): string {
  if (!user) return 'dashboard';
  if (canAccessRoute(user, defaultPreferred)) return defaultPreferred;

  const permitted = getPermittedRoutes(user);
  return permitted.length > 0 ? permitted[0] : 'dashboard';
}
