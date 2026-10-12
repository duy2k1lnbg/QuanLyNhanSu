type Translate = (key: string, params?: Record<string, string | number>) => string;

const statusKeys: Record<string, string> = {
  PUBLISHED: 'statusPublished', DRAFT: 'statusDraft',
  CHO_XU_LY: 'statusPending', DA_XAC_MINH: 'statusVerified',
  BO_QUA: 'statusIgnored', TU_CHOI: 'statusRejected',
  HOAN_TAT: 'statusCompleted', DANG_TINH: 'statusRunning',
  THAT_BAI: 'statusFailed', LOI: 'statusFailed',
  DA_PHAN_CONG: 'statusAssigned', NGAY_THUONG: 'dayNormal',
  NGAY_LE: 'dayHoliday', NGAY_NGHI: 'dayRest',
  NGHIEP_VU_TREO: 'severityBlocking', CANH_BAO: 'severityWarning',
};

/** Translate system codes; preserve unfamiliar codes for diagnostics. */
export function attendanceStatusLabel(value: string, t: Translate): string {
  const key = statusKeys[value];
  return key ? t('attendanceOps.' + key) : value;
}

/** Compatibility with the readiness messages returned by the existing API.
 * Anchored templates preserve numbers and employee IDs; unknown/user-authored
 * messages remain unchanged rather than being guessed or hidden.
 */
export function attendanceReasonLabel(value: string, t: Translate): string {
  let match = /^Kỳ công (\d+) hiện đã được khóa\.$/.exec(value);
  if (match) return t('attendanceOps.reasonLocked', { period: match[1] });
  match = /^Tồn tại (\d+) bất thường có mức độ chặn chốt chưa được giải trình\/xác minh \(Nhân viên: ([\d, ]+)\)\.$/.exec(value);
  if (match) return t('attendanceOps.reasonAnomalies', { count: match[1], employees: match[2] });
  match = /^Kỳ (\d+) có (\d+) ngày công chưa qua công bố TimeSegmentationEngine hoặc chưa đủ điều kiện chốt\.$/.exec(value);
  if (match) return t('attendanceOps.reasonUnpublished', { period: match[1], count: match[2] });
  match = /^Dữ liệu đầu vào đã có thay đổi mới \((\d+) ngày công bị lệch so với lần công bố gần nhất\)\. Cần tính toán công bố lại trước khi chốt\.$/.exec(value);
  if (match) return t('attendanceOps.reasonStale', { count: match[1] });
  return value;
}
