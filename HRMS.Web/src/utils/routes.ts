export const VALID_ROUTES: Record<string, string> = {
  dashboard: 'dashboard',
  nhanvien: 'nhanvien',
  chamcong: 'chamcong',
  bangluong: 'bangluong',
  hopdong: 'hopdong',
  khenthuong: 'khenthuong',
  nangluong: 'nangluong',
  ungluong: 'ungluong',
  approvals: 'approvals',
  phanquyen: 'phanquyen',
  audit: 'audit',
};

export const ROUTE_TITLES: Record<string, string> = {
  dashboard: 'Bảng điều khiển | HRMS Enterprise',
  nhanvien: 'Quản lý Nhân sự | HRMS Enterprise',
  chamcong: 'Chấm công & Ca làm | HRMS Enterprise',
  bangluong: 'Tính lương & Thuế | HRMS Enterprise',
  hopdong: 'Hợp đồng lao động | HRMS Enterprise',
  khenthuong: 'Khen thưởng & Kỷ luật | HRMS Enterprise',
  nangluong: 'Nâng lương & Điều chuyển | HRMS Enterprise',
  ungluong: 'Tăng ca & Tạm ứng | HRMS Enterprise',
  approvals: 'Trung tâm Phê duyệt | HRMS Enterprise',
  phanquyen: 'Phân quyền & Hệ thống | HRMS Enterprise',
  audit: 'Nhật ký Kiểm toán | HRMS Enterprise',
};

export const getRouteFromLocation = (): string => {
  if (typeof window === 'undefined') return 'dashboard';
  const hash = window.location.hash.replace(/^#\/?/, '').trim().toLowerCase();
  if (hash && VALID_ROUTES[hash]) return VALID_ROUTES[hash];

  const path = window.location.pathname.replace(/^\//, '').trim().toLowerCase();
  if (path && VALID_ROUTES[path]) return VALID_ROUTES[path];

  return 'dashboard';
};
