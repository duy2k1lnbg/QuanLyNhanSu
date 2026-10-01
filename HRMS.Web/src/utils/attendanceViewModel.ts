import type { KyCongChiTietDTO } from '../types/hrms';

export type AttendanceCategory =
  | 'work_full'        // Đi làm đủ ca (X, 1, CD)
  | 'work_half'        // Làm nửa ngày / nửa ca (X/2)
  | 'leave_full'       // Nghỉ phép năm cả ngày (P, F)
  | 'work_leave_mix'   // Nửa ngày làm việc + nửa ngày nghỉ phép (P/X)
  | 'business_trip'    // Công tác (CT)
  | 'insurance'        // Nghỉ chế độ BHXH ốm đau/thai sản (BH)
  | 'unpaid_leave'     // Nghỉ không lương có phép (KL)
  | 'holiday'          // Nghỉ lễ tết theo quy định (L)
  | 'weekend_off'      // Nghỉ tuần / không phân ca (N, CN)
  | 'absent'           // Vắng mặt không lý do (V, RO)
  | 'anomaly'          // Bất thường / thiếu quẹt thẻ / cần xác minh (?)
  | 'resigned'         // Đã thôi việc
  | 'unknown';         // Ký hiệu lạ chưa xác định

export interface AttendanceSymbolDef {
  symbol: string;
  name: string;
  category: AttendanceCategory;
  workUnits: number;
  leaveUnits: number;
  badgeColor: string;
  dotColor: 'green' | 'amber' | 'red' | 'gray' | 'purple' | 'cyan' | 'blue';
  description: string;
}

/**
 * Bản đồ giải mã ký hiệu chấm công chuẩn mực, đối soát trực tiếp từ CSDL Oracle và Backend
 */
export const ATTENDANCE_SYMBOLS: Record<string, AttendanceSymbolDef> = {
  X: {
    symbol: 'X',
    name: 'Đi làm đủ ngày',
    category: 'work_full',
    workUnits: 1.0,
    leaveUnits: 0,
    badgeColor: 'blue',
    dotColor: 'green',
    description: 'Hoàn thành đủ ca chuẩn (1.0 công)',
  },
  'X/2': {
    symbol: 'X/2',
    name: 'Làm nửa ngày',
    category: 'work_half',
    workUnits: 0.5,
    leaveUnits: 0,
    badgeColor: 'cyan',
    dotColor: 'green',
    description: 'Làm việc 0.5 ca thực tế',
  },
  '1': {
    symbol: '1',
    name: 'Đủ công',
    category: 'work_full',
    workUnits: 1.0,
    leaveUnits: 0,
    badgeColor: 'blue',
    dotColor: 'green',
    description: 'Chấm công chuẩn 1 công',
  },
  CD: {
    symbol: 'CD',
    name: 'Ca đêm',
    category: 'work_full',
    workUnits: 1.0,
    leaveUnits: 0,
    badgeColor: 'purple',
    dotColor: 'purple',
    description: 'Làm việc ca đêm có phụ cấp theo luật',
  },
  P: {
    symbol: 'P',
    name: 'Nghỉ phép hưởng lương',
    category: 'leave_full',
    workUnits: 0,
    leaveUnits: 1.0,
    badgeColor: 'gold',
    dotColor: 'amber',
    description: 'Nghỉ phép năm đã được phê duyệt (1.0 ngày phép)',
  },
  F: {
    symbol: 'F',
    name: 'Nghỉ phép',
    category: 'leave_full',
    workUnits: 0,
    leaveUnits: 1.0,
    badgeColor: 'gold',
    dotColor: 'amber',
    description: 'Nghỉ phép theo chế độ',
  },
  'P/X': {
    symbol: 'P/X',
    name: 'Nửa phép + Nửa làm',
    category: 'work_leave_mix',
    workUnits: 0.5,
    leaveUnits: 0.5,
    badgeColor: 'orange',
    dotColor: 'amber',
    description: 'Làm việc 0.5 công + nghỉ phép 0.5 công',
  },
  CT: {
    symbol: 'CT',
    name: 'Công tác',
    category: 'business_trip',
    workUnits: 1.0,
    leaveUnits: 0,
    badgeColor: 'cyan',
    dotColor: 'cyan',
    description: 'Đi công tác theo quyết định của ban giám đốc',
  },
  BH: {
    symbol: 'BH',
    name: 'Nghỉ chế độ BHXH',
    category: 'insurance',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'magenta',
    dotColor: 'purple',
    description: 'Nghỉ ốm đau, thai sản, khám thai do quỹ BHXH chi trả',
  },
  KL: {
    symbol: 'KL',
    name: 'Nghỉ không lương',
    category: 'unpaid_leave',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'volcano',
    dotColor: 'amber',
    description: 'Nghỉ việc riêng không hưởng lương có đơn duyệt',
  },
  L: {
    symbol: 'L',
    name: 'Nghỉ lễ có lương',
    category: 'holiday',
    workUnits: 0,
    leaveUnits: 1.0,
    badgeColor: 'geekblue',
    dotColor: 'blue',
    description: 'Nghỉ ngày lễ tết quốc gia hưởng nguyên lương',
  },
  CN: {
    symbol: 'CN',
    name: 'Chủ nhật / Nghỉ tuần',
    category: 'weekend_off',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'default',
    dotColor: 'gray',
    description: 'Ngày nghỉ cuối tuần theo lịch công ty',
  },
  N: {
    symbol: 'N',
    name: 'Nghỉ theo lịch',
    category: 'weekend_off',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'default',
    dotColor: 'gray',
    description: 'Không có ca phân bổ trong ngày',
  },
  V: {
    symbol: 'V',
    name: 'Vắng không phép',
    category: 'absent',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'red',
    dotColor: 'red',
    description: 'Không đi làm và chưa có đơn xin phép',
  },
  RO: {
    symbol: 'RO',
    name: 'Nghỉ bù',
    category: 'weekend_off',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'default',
    dotColor: 'gray',
    description: 'Nghỉ bù làm thêm giờ',
  },
  '?': {
    symbol: '?',
    name: 'Ngoại lệ / Chưa xác minh',
    category: 'anomaly',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'red',
    dotColor: 'red',
    description: 'Dữ liệu bất thường hoặc thiếu lượt quẹt thẻ',
  },
};

/**
 * Phân tích mã ký hiệu từ cell dữ liệu công
 */
export const parseAttendanceSymbol = (rawSymbol?: string | null): AttendanceSymbolDef => {
  if (!rawSymbol || !rawSymbol.trim()) {
    return {
      symbol: '-',
      name: 'Chưa có dữ liệu',
      category: 'unknown',
      workUnits: 0,
      leaveUnits: 0,
      badgeColor: 'default',
      dotColor: 'gray',
      description: 'Chưa phát sinh ghi nhận công',
    };
  }

  const clean = rawSymbol.trim().toUpperCase();
  if (ATTENDANCE_SYMBOLS[clean]) {
    return ATTENDANCE_SYMBOLS[clean];
  }

  // Fallback nếu có tiền tố / hậu tố
  if (clean.startsWith('X/2')) return ATTENDANCE_SYMBOLS['X/2'];
  if (clean.startsWith('P/X')) return ATTENDANCE_SYMBOLS['P/X'];
  if (clean.startsWith('X')) return ATTENDANCE_SYMBOLS['X'];
  if (clean.startsWith('P')) return ATTENDANCE_SYMBOLS['P'];
  if (clean.startsWith('L')) return ATTENDANCE_SYMBOLS['L'];
  if (clean.startsWith('BH')) return ATTENDANCE_SYMBOLS['BH'];
  if (clean.startsWith('KL')) return ATTENDANCE_SYMBOLS['KL'];
  if (clean.startsWith('CT')) return ATTENDANCE_SYMBOLS['CT'];
  if (clean.startsWith('V')) return ATTENDANCE_SYMBOLS['V'];
  if (clean.includes('?')) return ATTENDANCE_SYMBOLS['?'];

  return {
    symbol: clean,
    name: `Ký hiệu: ${clean}`,
    category: 'unknown',
    workUnits: 0,
    leaveUnits: 0,
    badgeColor: 'default',
    dotColor: 'gray',
    description: `Ký hiệu nguồn (${clean}) chưa được phân loại`,
  };
};

export interface EmployeeDayRecord {
  manv: number;
  hoten: string;
  tenpb?: string;
  symbol: string;
  symbolDef: AttendanceSymbolDef;
  isResigned: boolean;
  workUnits: number;
  leaveUnits: number;
}

export interface DayAttendanceMetrics {
  day: number;
  dateStr: string;
  dayOfWeekName: string;
  isWeekend: boolean;
  isSunday: boolean;
  isSaturday: boolean;
  isToday: boolean;

  // Headcount & Phạm vi
  totalScope: number;
  activeHeadcount: number;
  resignedCount: number;
  scheduledCount: number;

  // Thành phần công thực tế
  workFullCount: number;       // Đi làm đủ ca (X)
  workHalfCount: number;       // Làm nửa ngày (X/2)
  leaveFullCount: number;      // Nghỉ phép cả ngày (P)
  workLeaveMixCount: number;   // Làm nửa ngày + phép nửa ngày (P/X)
  businessTripCount: number;   // Công tác (CT)
  insuranceCount: number;      // Nghỉ BHXH (BH)
  unpaidLeaveCount: number;    // Nghỉ không lương (KL)
  holidayCount: number;        // Nghỉ lễ (L)
  absentCount: number;         // Vắng mặt (V)
  anomalyCount: number;        // Bất thường (?)
  weekendOffCount: number;     // Nghỉ tuần (N, CN)

  // Tổng hợp hợp nhất
  distinctWorkedCount: number; // Tổng số người có tham gia làm việc ngày này
  workEquivalentDays: number;  // Tổng công quy đổi
  leaveEquivalentDays: number; // Tổng ngày phép hưởng lương

  // Tỷ lệ đi làm
  attendanceRate: number | null; // null nếu không có lịch làm việc
  attendanceRateDisplay: string;

  // Trạng thái chung hiển thị chấm ô lịch
  calendarDotColor: 'green' | 'amber' | 'red' | 'gray' | 'blue' | 'purple';
  calendarBadgeText?: string;

  // Danh sách chi tiết nhân viên trong ngày
  records: EmployeeDayRecord[];
}

/**
 * Tính toán thống kê chi tiết cho một ngày từ danh sách bảng công
 */
export const calculateDayAttendance = (
  day: number,
  month: number,
  year: number,
  employeeRows: KyCongChiTietDTO[],
  lang: string = 'vi'
): DayAttendanceMetrics => {
  const dateObj = new Date(year, month - 1, day);
  const dayOfWeek = dateObj.getDay();
  const isSunday = dayOfWeek === 0;
  const isSaturday = dayOfWeek === 6;
  const isWeekend = isSunday || isSaturday;

  const today = new Date();
  const isToday =
    today.getDate() === day &&
    today.getMonth() === month - 1 &&
    today.getFullYear() === year;

  const dayKey = `D${day}` as keyof KyCongChiTietDTO;
  const dateStr = `${day < 10 ? '0' + day : day}/${month < 10 ? '0' + month : month}/${year}`;
  const dayOfWeekName = new Intl.DateTimeFormat(lang === 'zh-CN' ? 'zh-CN' : lang, {
    weekday: 'long',
  }).format(dateObj);

  let activeHeadcount = 0;
  let resignedCount = 0;
  let scheduledCount = 0;

  let workFullCount = 0;
  let workHalfCount = 0;
  let leaveFullCount = 0;
  let workLeaveMixCount = 0;
  let businessTripCount = 0;
  let insuranceCount = 0;
  let unpaidLeaveCount = 0;
  let holidayCount = 0;
  let absentCount = 0;
  let anomalyCount = 0;
  let weekendOffCount = 0;

  const records: EmployeeDayRecord[] = [];

  for (const row of employeeRows) {
    const isResigned = row.DATHOIVIEC === 1 || row.IS_ACTIVE === false;
    if (isResigned) {
      resignedCount++;
    } else {
      activeHeadcount++;
    }

    const rawVal = row[dayKey] as string | undefined;
    const def = parseAttendanceSymbol(rawVal);

    records.push({
      manv: row.MANV,
      hoten: row.HOTEN,
      tenpb: row.TENPB,
      symbol: def.symbol,
      symbolDef: def,
      isResigned,
      workUnits: def.workUnits,
      leaveUnits: def.leaveUnits,
    });

    // Xác định có lịch hay không
    const hasSchedule = !isResigned && def.category !== 'weekend_off';
    if (hasSchedule) {
      scheduledCount++;
    }

    // Phân loại các nhóm
    switch (def.category) {
      case 'work_full':
        workFullCount++;
        break;
      case 'work_half':
        workHalfCount++;
        break;
      case 'leave_full':
        leaveFullCount++;
        break;
      case 'work_leave_mix':
        workLeaveMixCount++;
        break;
      case 'business_trip':
        businessTripCount++;
        break;
      case 'insurance':
        insuranceCount++;
        break;
      case 'unpaid_leave':
        unpaidLeaveCount++;
        break;
      case 'holiday':
        holidayCount++;
        break;
      case 'absent':
        absentCount++;
        break;
      case 'anomaly':
        anomalyCount++;
        break;
      case 'weekend_off':
        weekendOffCount++;
        break;
      default:
        break;
    }
  }

  // Số người duy nhất có tham gia làm việc
  const distinctWorkedCount =
    workFullCount + workHalfCount + workLeaveMixCount + businessTripCount;

  // Tổng công quy đổi
  const workEquivalentDays =
    workFullCount * 1.0 +
    workHalfCount * 0.5 +
    workLeaveMixCount * 0.5 +
    businessTripCount * 1.0;

  // Tổng ngày phép
  const leaveEquivalentDays = leaveFullCount * 1.0 + workLeaveMixCount * 0.5;

  // Tỷ lệ đi làm:
  // Nếu là Chủ nhật không phân ca: tỷ lệ = null
  // Nếu có người phân ca: distinctWorkedCount / scheduledCount
  let attendanceRate: number | null = null;
  let attendanceRateDisplay = '—';

  // Tính mẫu số chuẩn: nếu có lịch thì dùng scheduledCount, nếu không có lịch nhưng là ngày làm việc thường thì dùng activeHeadcount
  const denominator = scheduledCount > 0 ? scheduledCount : (isSunday ? 0 : activeHeadcount);

  if (denominator > 0) {
    attendanceRate = Math.min(100, Math.round((distinctWorkedCount / denominator) * 100));
    attendanceRateDisplay = `${attendanceRate}%`;
  } else if (isSunday) {
    attendanceRateDisplay = 'Nghỉ tuần';
  }

  // Chấm màu ô lịch & nhãn phụ
  let calendarDotColor: DayAttendanceMetrics['calendarDotColor'] = 'green';
  let calendarBadgeText: string | undefined;

  if (isSunday && distinctWorkedCount === 0) {
    calendarDotColor = 'gray';
  } else if (holidayCount > 0 && distinctWorkedCount === 0) {
    calendarDotColor = 'blue';
    calendarBadgeText = 'Lễ';
  } else if (absentCount > 0 || anomalyCount > 0) {
    calendarDotColor = 'red';
    calendarBadgeText = `${absentCount + anomalyCount} ${absentCount > 0 ? 'V' : '?'}`;
  } else if (leaveFullCount > 0 || workLeaveMixCount > 0) {
    calendarDotColor = 'amber';
    const totalPhep = leaveFullCount + workLeaveMixCount;
    calendarBadgeText = `${totalPhep} P`;
  } else if (businessTripCount > 0) {
    calendarDotColor = 'blue';
    calendarBadgeText = `${businessTripCount} CT`;
  } else if (insuranceCount > 0) {
    calendarDotColor = 'purple';
    calendarBadgeText = `${insuranceCount} BH`;
  }

  return {
    day,
    dateStr,
    dayOfWeekName,
    isWeekend,
    isSunday,
    isSaturday,
    isToday,
    totalScope: employeeRows.length,
    activeHeadcount,
    resignedCount,
    scheduledCount,
    workFullCount,
    workHalfCount,
    leaveFullCount,
    workLeaveMixCount,
    businessTripCount,
    insuranceCount,
    unpaidLeaveCount,
    holidayCount,
    absentCount,
    anomalyCount,
    weekendOffCount,
    distinctWorkedCount,
    workEquivalentDays,
    leaveEquivalentDays,
    attendanceRate,
    attendanceRateDisplay,
    calendarDotColor,
    calendarBadgeText,
    records,
  };
};
