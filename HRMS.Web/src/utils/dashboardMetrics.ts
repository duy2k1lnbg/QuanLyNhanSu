import type { NhanVienDTO, HopDongDTO, BangLuongDTO } from '../types/hrms';
import dayjs from 'dayjs';

/**
 * Kiểm tra xem một chuỗi ngày kết thúc hợp đồng đã qua chưa (tính theo ngày hiện tại hoặc ngày chỉ định)
 */
export const isContractExpired = (endDateStr?: string | null, targetDate?: dayjs.Dayjs): boolean => {
  if (!endDateStr || !endDateStr.trim()) return false;
  const trimmed = endDateStr.trim();
  const ref = targetDate || dayjs();
  
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(trimmed)) {
    const [day, month, year] = trimmed.split('/').map(Number);
    const end = dayjs(new Date(year, month - 1, day));
    return end.isBefore(ref, 'day');
  }
  const parsed = dayjs(trimmed);
  return parsed.isValid() ? parsed.isBefore(ref, 'day') : false;
};

export interface DashboardMetricsSummary {
  totalEmployees: number;
  activeEmployees: number;
  resignedEmployees: number;
  totalContracts: number;
  activeContracts: number;
  expiredContracts: number;
  distinctPresentToday: number;
  totalPunchesToday: number;
  absentToday: number;
  totalScheduledToday: number;
  attendanceRate: number;
  totalNetPayroll: number;
  payrollCalculatedCount: number;
  payrollPeriodLabel: string;
}

/**
 * Tính toán các chỉ số KPI thống nhất, minh bạch theo BẢNG CÔNG THỰC TẾ
 * (TB_BANGCONG_CHITIET / TB_KYCONGCHITIET), không hardcode số liệu.
 */
export const calculateDashboardMetrics = (
  nhanVienList: NhanVienDTO[],
  hopDongList: HopDongDTO[],
  bangLuongList: BangLuongDTO[],
  rawPresentToday?: number,
  param5?: number | string,
  chamCongList?: any[],
  param7?: string
): DashboardMetricsSummary => {
  // Chuẩn hóa tham số đa hình để tương thích ngược 100% với các hàm gọi cũ
  let rawAbsentToday: number | undefined;
  let customPeriodName: string | undefined;

  if (typeof param5 === 'number') {
    rawAbsentToday = param5;
    customPeriodName = param7;
  } else if (typeof param5 === 'string') {
    customPeriodName = param5;
  }

  // 1. Nhân sự
  const totalEmployees = nhanVienList.length;
  const activeEmployees = nhanVienList.filter(
    (nv) => nv.DATHOIVIEC !== 1 && nv.TRANGTHAI !== false
  ).length;
  const resignedEmployees = nhanVienList.filter(
    (nv) => nv.DATHOIVIEC === 1 || nv.TRANGTHAI === false
  ).length;

  // 2. Hợp đồng
  const totalContracts = hopDongList.length;
  let activeContracts = 0;
  let expiredContracts = 0;

  if (hopDongList.length > 0) {
    for (const hd of hopDongList) {
      if (isContractExpired(hd.NGAYKETTHUC)) {
        expiredContracts++;
      } else {
        activeContracts++;
      }
    }
  } else if (nhanVienList.length > 0) {
    activeContracts = activeEmployees;
  }

  // 3. Có mặt & vắng hôm nay theo BẢNG CÔNG THỰC TẾ
  let distinctPresentToday = 0;
  let absentToday = 0;
  let totalScheduledToday = 0;
  const totalPunchesToday = rawPresentToday ?? 0;

  // Ưu tiên 1: Đọc trực tiếp từ bảng công chi tiết theo ngày nếu đã tải (chamCongList)
  let calculatedFromTimesheet = false;
  if (chamCongList && chamCongList.length > 0) {
    const todayDay = dayjs().date();
    const dayKey = `D${todayDay}`;
    let ccWork = 0;
    let ccLeave = 0;

    for (const row of chamCongList) {
      if (row.DATHOIVIEC === 1 || row.IS_ACTIVE === false) continue;
      const val = (row[dayKey] ?? '').toString().trim().toUpperCase();
      if (val && val !== '-' && val !== 'CN' && val !== 'N') {
        calculatedFromTimesheet = true;
        // Đi làm: X, X/2, CD, CT, 1, P/X
        if (val === 'X' || val === 'X/2' || val === 'CD' || val === 'CT' || val === '1' || val === 'P/X') {
          ccWork++;
        }
        // Vắng / Nghỉ phép / Nghỉ ốm / Việc riêng: P, O, VR, V, BH, KL, F
        else if (['P', 'O', 'VR', 'V', 'BH', 'KL', 'F'].includes(val)) {
          ccLeave++;
        }
      }
    }

    if (calculatedFromTimesheet && (ccWork > 0 || ccLeave > 0)) {
      distinctPresentToday = ccWork;
      absentToday = ccLeave;
      totalScheduledToday = ccWork + ccLeave;
    }
  }

  // Ưu tiên 2: Sử dụng số liệu từ API DashboardController (được tổng hợp từ TB_BANGCONG_CHITIET)
  if (!calculatedFromTimesheet || totalScheduledToday === 0) {
    distinctPresentToday = totalPunchesToday > 0
      ? (activeEmployees > 0 ? Math.min(activeEmployees, totalPunchesToday) : totalPunchesToday)
      : 0;

    if (rawAbsentToday !== undefined && rawAbsentToday !== null) {
      absentToday = rawAbsentToday;
    } else {
      absentToday = Math.max(0, activeEmployees - distinctPresentToday);
    }
    totalScheduledToday = distinctPresentToday + absentToday;
  }

  // Tỷ lệ đi làm theo bảng công (tổng đi làm / tổng nhân sự có lịch làm việc)
  const attendanceRate = totalScheduledToday > 0
    ? Math.min(100, Math.round((distinctPresentToday / totalScheduledToday) * 100))
    : (activeEmployees > 0 ? Math.min(100, Math.round((distinctPresentToday / activeEmployees) * 100)) : 0);

  // 4. Quỹ lương (Tổng thực lĩnh kỳ mới nhất)
  let totalNetPayroll = 0;
  let payrollCalculatedCount = 0;

  if (bangLuongList && bangLuongList.length > 0) {
    for (const bl of bangLuongList) {
      if (bl.THUC_LINH && bl.THUC_LINH > 0) {
        totalNetPayroll += bl.THUC_LINH;
        payrollCalculatedCount++;
      }
    }
  }

  const periodName = customPeriodName || 'Kỳ hiện hành';

  return {
    totalEmployees,
    activeEmployees,
    resignedEmployees,
    totalContracts,
    activeContracts,
    expiredContracts,
    distinctPresentToday,
    totalPunchesToday,
    absentToday,
    totalScheduledToday,
    attendanceRate,
    totalNetPayroll,
    payrollCalculatedCount,
    payrollPeriodLabel: periodName,
  };
};
