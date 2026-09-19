export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    CHANGE_PASSWORD: '/auth/change-password',
  },
  ME: {
    BASE: '/me',
    DASHBOARD: '/me/dashboard',
    PROFILE: '/me/profile',
    ATTENDANCE: '/me/attendance',
    PAYROLL: '/me/payroll',
    CONTRACT: '/me/contract',
    INSURANCE: '/me/insurance',
    NOTIFICATIONS: '/me/notifications',
    NOTIFICATION_DETAIL: (id: string | number) => `/me/notifications/${id}`,
  },
};
