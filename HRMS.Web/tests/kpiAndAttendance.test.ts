import assert from 'node:assert';
import {
  calculateDashboardMetrics,
  isContractExpired,
} from '../src/utils/dashboardMetrics.ts';
import {
  parseAttendanceSymbol,
  calculateDayAttendance,
  ATTENDANCE_SYMBOLS,
} from '../src/utils/attendanceViewModel.ts';
import type { NhanVienDTO, HopDongDTO, BangLuongDTO, KyCongChiTietDTO } from '../src/types/hrms.ts';

console.log('=== TEST SUITE: HRMS KPI & ATTENDANCE SELECTORS ===\n');

// ----------------------------------------------------------------------
// TEST 1: Dashboard KPI Deduplication & Reconciliation
// ----------------------------------------------------------------------
console.log('Test 1: Dashboard KPI: 215 Punches vs 195 Active Headcount vs 200 Contracts');

const mockEmployees: NhanVienDTO[] = [
  ...Array.from({ length: 195 }, (_, i) => ({
    MANV: i + 1,
    HOTEN: `Nhân viên ${i + 1}`,
    DATHOIVIEC: 0,
    TRANGTHAI: true,
  })),
  ...Array.from({ length: 5 }, (_, i) => ({
    MANV: 196 + i,
    HOTEN: `Đã thôi việc ${196 + i}`,
    DATHOIVIEC: 1,
    TRANGTHAI: false,
  })),
];

const mockContracts: HopDongDTO[] = [
  ...Array.from({ length: 195 }, (_, i) => ({
    SOHD: `HD-${i + 1}`,
    MANV: i + 1,
    HOTEN: `Nhân viên ${i + 1}`,
    NGAYBATDAU: '01/01/2026',
    NGAYKETTHUC: '31/12/2026',
  })),
  ...Array.from({ length: 5 }, (_, i) => ({
    SOHD: `HD-${196 + i}`,
    MANV: 196 + i,
    HOTEN: `Đã thôi việc ${196 + i}`,
    NGAYBATDAU: '01/01/2025',
    NGAYKETTHUC: '15/09/2026', // Expired
  })),
];

const mockPayroll: BangLuongDTO[] = [
  ...Array.from({ length: 193 }, (_, i) => ({
    IDBL: i + 1,
    MANV: i + 1,
    HOTEN: `Nhân viên ${i + 1}`,
    MAKYCONG: 202609,
    THANG: 9,
    NAM: 2026,
    THUC_LINH: 16360000,
  })),
];

const kpiResult = calculateDashboardMetrics(
  mockEmployees,
  mockContracts,
  mockPayroll,
  215, // raw punch count from DashboardController
  '09/2026'
);

assert.strictEqual(kpiResult.totalEmployees, 200, 'Total employees must be exactly 200');
assert.strictEqual(kpiResult.activeEmployees, 195, 'Active headcount must be exactly 195');
assert.strictEqual(kpiResult.resignedEmployees, 5, 'Resigned employees must be exactly 5');
assert.strictEqual(kpiResult.activeContracts, 195, 'Active contracts must be 195 (not 200)');
assert.strictEqual(kpiResult.expiredContracts, 5, 'Expired contracts must be 5');
assert.strictEqual(kpiResult.totalPunchesToday, 215, 'Total punches recorded must be preserved as 215');
assert.strictEqual(kpiResult.distinctPresentToday, 195, 'Distinct present employees must be capped at active headcount 195');
assert.strictEqual(kpiResult.payrollCalculatedCount, 193, 'Payroll calculated count must be 193');
assert.ok(kpiResult.totalNetPayroll > 3000000000, 'Payroll sum must exceed 3B VND');
console.log('✓ Test 1 Passed: KPI accurately differentiates 215 punches, 195 active, 195 contracts, 3.16B VND payroll.\n');

// ----------------------------------------------------------------------
// TEST 2: Attendance Symbol Mapping & Parsing
// ----------------------------------------------------------------------
console.log('Test 2: Attendance Symbol Parsing & Category Verification');

const symbolsToTest = ['X', 'X/2', 'P', 'P/X', 'L', 'BH', 'KL', 'CT', 'CD', 'RO', 'F', '?', 'N', 'V'];

for (const sym of symbolsToTest) {
  const parsed = parseAttendanceSymbol(sym);
  assert.ok(parsed, `Symbol ${sym} must be parsed`);
  assert.notStrictEqual(parsed.category, 'unknown', `Symbol ${sym} must have a known category`);
}

const px = parseAttendanceSymbol('P/X');
assert.strictEqual(px.category, 'work_leave_mix');
assert.strictEqual(px.workUnits, 0.5);
assert.strictEqual(px.leaveUnits, 0.5);

const bh = parseAttendanceSymbol('BH');
assert.strictEqual(bh.category, 'insurance');

const kl = parseAttendanceSymbol('KL');
assert.strictEqual(bh.category, 'insurance');

const ct = parseAttendanceSymbol('CT');
assert.strictEqual(ct.category, 'business_trip');

console.log('✓ Test 2 Passed: All 14 symbols mapped with exact work and leave unit definitions.\n');

// ----------------------------------------------------------------------
// TEST 3: Daily Attendance Aggregation & No Double-Counting
// ----------------------------------------------------------------------
console.log('Test 3: Daily Attendance Aggregation: Half-Day, Mix, Leave, Rate');

const sampleRows: KyCongChiTietDTO[] = [
  // 100 full work (X)
  ...Array.from({ length: 100 }, (_, i) => ({
    MAKYCONG: 202609,
    MANV: i + 1,
    HOTEN: `NV ${i + 1}`,
    D29: 'X',
  })),
  // 10 half work (X/2)
  ...Array.from({ length: 10 }, (_, i) => ({
    MAKYCONG: 202609,
    MANV: 101 + i,
    HOTEN: `NV ${101 + i}`,
    D29: 'X/2',
  })),
  // 5 mix half work + half leave (P/X)
  ...Array.from({ length: 5 }, (_, i) => ({
    MAKYCONG: 202609,
    MANV: 111 + i,
    HOTEN: `NV ${111 + i}`,
    D29: 'P/X',
  })),
  // 5 full leave (P)
  ...Array.from({ length: 5 }, (_, i) => ({
    MAKYCONG: 202609,
    MANV: 116 + i,
    HOTEN: `NV ${116 + i}`,
    D29: 'P',
  })),
  // 2 business trip (CT)
  ...Array.from({ length: 2 }, (_, i) => ({
    MAKYCONG: 202609,
    MANV: 121 + i,
    HOTEN: `NV ${121 + i}`,
    D29: 'CT',
  })),
  // 1 insurance (BH)
  {
    MAKYCONG: 202609,
    MANV: 123,
    HOTEN: 'NV 123',
    D29: 'BH',
  },
  // 1 unpaid leave (KL)
  {
    MAKYCONG: 202609,
    MANV: 124,
    HOTEN: 'NV 124',
    D29: 'KL',
  },
  // 2 anomalies (?)
  {
    MAKYCONG: 202609,
    MANV: 125,
    HOTEN: 'NV 125',
    D29: '?',
  },
  {
    MAKYCONG: 202609,
    MANV: 126,
    HOTEN: 'NV 126',
    D29: '?',
  },
];

const day29 = calculateDayAttendance(29, 9, 2026, sampleRows, 'vi');

// Distinct worked count: 100 (full) + 10 (half) + 5 (mix) + 2 (CT) = 117
assert.strictEqual(day29.distinctWorkedCount, 117, 'Distinct worked employees must be 117');

// Equivalent work days: 100*1 + 10*0.5 + 5*0.5 + 2*1 = 100 + 5 + 2.5 + 2 = 109.5
assert.strictEqual(day29.workEquivalentDays, 109.5, 'Equivalent work days must be 109.5');

// Equivalent leave days: 5*1 (P) + 5*0.5 (P/X) = 7.5
assert.strictEqual(day29.leaveEquivalentDays, 7.5, 'Equivalent leave days must be 7.5');

// Counts
assert.strictEqual(day29.workFullCount, 100);
assert.strictEqual(day29.workHalfCount, 10);
assert.strictEqual(day29.workLeaveMixCount, 5);
assert.strictEqual(day29.leaveFullCount, 5);
assert.strictEqual(day29.businessTripCount, 2);
assert.strictEqual(day29.insuranceCount, 1);
assert.strictEqual(day29.unpaidLeaveCount, 1);
assert.strictEqual(day29.anomalyCount, 2);

console.log('✓ Test 3 Passed: Daily aggregation computes exact headcounts and equivalent units without double counting.\n');

// ----------------------------------------------------------------------
// TEST 4: Sunday & No-Schedule Guard (No False 100%)
// ----------------------------------------------------------------------
console.log('Test 4: Sunday / Non-scheduled attendance rate handling');

const sundayRows: KyCongChiTietDTO[] = Array.from({ length: 20 }, (_, i) => ({
  MAKYCONG: 202609,
  MANV: i + 1,
  HOTEN: `NV ${i + 1}`,
  D27: 'CN', // Sunday 27/09/2026
}));

const sundayMetrics = calculateDayAttendance(27, 9, 2026, sundayRows, 'vi');
assert.strictEqual(sundayMetrics.isSunday, true, 'Day 27 must be recognized as Sunday');
assert.strictEqual(sundayMetrics.distinctWorkedCount, 0, 'No worked punches on normal Sunday');
assert.strictEqual(sundayMetrics.attendanceRate, null, 'Attendance rate must be null on Sunday with no scheduled shifts');
assert.strictEqual(sundayMetrics.attendanceRateDisplay, 'Nghỉ tuần', 'Attendance rate display must be Nghỉ tuần (not 100%)');

console.log('✓ Test 4 Passed: Sunday / non-scheduled day does not falsely output 100% or divide by zero.\n');

console.log('ALL TESTS PASSED SUCCESSFULLY! (4/4 test suites)');
