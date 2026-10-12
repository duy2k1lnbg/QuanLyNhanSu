import i18n from '../i18n/i18n';

/**
 * Map raw request status from backend to localized string
 */
export const getLocalizedRequestStatus = (status: string | null | undefined): string => {
  if (!status) return i18n.t('requests.statusPending');
  const upper = status.trim().toUpperCase();

  if (upper === 'APPROVED' || upper === 'ĐÃ DUYỆT' || upper === 'DA_DUYET') {
    return i18n.t('requests.statusApproved');
  }
  if (upper === 'REJECTED' || upper === 'TỪ CHỐI' || upper === 'TU_CHOI') {
    return i18n.t('requests.statusRejected');
  }
  return i18n.t('requests.statusPending');
};

/**
 * Check if a payroll status represents a completed/paid payment.
 * Explicitly guards against 'unpaid', 'chưa thanh toán', 'chờ thanh toán', 'pending'.
 */
export const isPayrollPaid = (status: string | null | undefined): boolean => {
  if (!status) return false;
  const lower = status.trim().toLowerCase();

  // Explicit negative guards: unpaid, pending, awaiting, not yet paid
  if (
    lower.includes('unpaid') ||
    lower.includes('chưa') ||
    lower.includes('chờ') ||
    lower.includes('pending') ||
    lower.includes('awaiting')
  ) {
    return false;
  }

  // Positive markers for paid status
  return (
    lower === 'paid' ||
    lower === 'đã thanh toán' ||
    lower === 'da thanh toan' ||
    lower === 'hoàn tất' ||
    lower === 'hoan tat' ||
    lower === 'đã chi trả' ||
    lower === 'da chi tra' ||
    (lower.startsWith('đã') && !lower.includes('chưa'))
  );
};

/**
 * Map raw payroll payment status to localized string
 */
export const getLocalizedPayrollStatus = (status: string | null | undefined): string => {
  if (!status) return i18n.t('payroll.statusPending');
  if (isPayrollPaid(status)) {
    return i18n.t('payroll.statusPaid');
  }
  return i18n.t('payroll.statusPending');
};

/**
 * Map attendance status to localized string
 */
export const getLocalizedAttendanceStatus = (status: string | null | undefined): string => {
  if (!status) return '';
  const lower = status.trim().toLowerCase();

  if (lower.includes('đủ') || lower.includes('full')) {
    return i18n.t('attendance.statusFull');
  }
  if (lower.includes('muộn') || lower.includes('late')) {
    return i18n.t('attendance.statusLate');
  }
  if (lower.includes('sớm') || lower.includes('early')) {
    return i18n.t('attendance.statusEarly');
  }
  if (lower.includes('phép') || lower.includes('leave')) {
    return i18n.t('attendance.statusLeave');
  }
  if (lower.includes('vắng') || lower.includes('absent')) {
    return i18n.t('attendance.statusAbsent');
  }
  return status;
};

/**
 * Map employment status to localized string
 */
export const getLocalizedEmploymentStatus = (status: string | number | null | undefined): string => {
  if (status === 1 || status === '1' || status === 'active' || String(status).toLowerCase().includes('đang')) {
    return i18n.t('profile.active');
  }
  return i18n.t('profile.resigned');
};
