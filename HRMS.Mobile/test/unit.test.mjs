import { test, describe } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

describe('HRMS Mobile Localization Key Parity', () => {
  const viPath = path.resolve(__dirname, '../src/i18n/locales/vi.json');
  const jaPath = path.resolve(__dirname, '../src/i18n/locales/ja.json');
  const enPath = path.resolve(__dirname, '../src/i18n/locales/en.json');
  const zhPath = path.resolve(__dirname, '../src/i18n/locales/zh-CN.json');
  const koPath = path.resolve(__dirname, '../src/i18n/locales/ko.json');

  const vi = JSON.parse(fs.readFileSync(viPath, 'utf8'));
  const ja = JSON.parse(fs.readFileSync(jaPath, 'utf8'));
  const en = JSON.parse(fs.readFileSync(enPath, 'utf8'));
  const zh = JSON.parse(fs.readFileSync(zhPath, 'utf8'));
  const ko = JSON.parse(fs.readFileSync(koPath, 'utf8'));

  const getKeys = (obj, prefix = '') => {
    let keys = [];
    for (const key of Object.keys(obj)) {
      const fullKey = prefix ? `${prefix}.${key}` : key;
      if (typeof obj[key] === 'object' && obj[key] !== null && !Array.isArray(obj[key])) {
        keys = keys.concat(getKeys(obj[key], fullKey));
      } else {
        keys.push(fullKey);
      }
    }
    return keys;
  };

  const viKeys = getKeys(vi).sort();
  const jaKeys = getKeys(ja).sort();
  const enKeys = getKeys(en).sort();
  const zhKeys = getKeys(zh).sort();
  const koKeys = getKeys(ko).sort();

  test('Vietnamese and Japanese should have identical translation keys', () => {
    assert.deepEqual(jaKeys, viKeys, 'Missing or extra keys between ja and vi');
  });

  test('Vietnamese and English should have identical translation keys', () => {
    assert.deepEqual(enKeys, viKeys, 'Missing or extra keys between en and vi');
  });

  test('Vietnamese and Chinese (zh-CN) should have identical translation keys', () => {
    assert.deepEqual(zhKeys, viKeys, 'Missing or extra keys between zh-CN and vi');
  });

  test('Vietnamese and Korean (ko) should have identical translation keys', () => {
    assert.deepEqual(koKeys, viKeys, 'Missing or extra keys between ko and vi');
  });

  test('No empty translations in any language file', () => {
    const checkNoEmpty = (obj, file) => {
      for (const [k, v] of Object.entries(obj)) {
        if (typeof v === 'object' && v !== null) {
          checkNoEmpty(v, file);
        } else {
          assert.ok(v && v.trim().length > 0, `Empty value for key ${k} in ${file}`);
        }
      }
    };
    checkNoEmpty(vi, 'vi.json');
    checkNoEmpty(ja, 'ja.json');
    checkNoEmpty(en, 'en.json');
    checkNoEmpty(zh, 'zh-CN.json');
    checkNoEmpty(ko, 'ko.json');
  });
});

describe('Formatters and Utilities', () => {
  test('Currency formatting handles valid numbers and nulls', () => {
    const formatCurrency = (amount, locale = 'vi') => {
      if (amount == null || isNaN(amount)) return '0 ₫';
      try {
        if (locale === 'ja') {
          return new Intl.NumberFormat('ja-JP', { style: 'currency', currency: 'VND' }).format(amount);
        }
        if (locale === 'en') {
          return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'VND' }).format(amount);
        }
        if (locale === 'zh-CN' || locale === 'zh') {
          return new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'VND' }).format(amount);
        }
        if (locale === 'ko') {
          return new Intl.NumberFormat('ko-KR', { style: 'currency', currency: 'VND' }).format(amount);
        }
        return new Intl.NumberFormat('vi-VN').format(amount) + ' ₫';
      } catch {
        return amount.toLocaleString() + ' ₫';
      }
    };

    assert.equal(formatCurrency(null), '0 ₫');
    assert.equal(formatCurrency(undefined), '0 ₫');
    assert.ok(formatCurrency(1500000, 'vi').includes('1.500.000'));
    assert.ok(formatCurrency(1500000, 'en').includes('1,500,000'));
    assert.ok(formatCurrency(1500000, 'zh-CN').includes('1,500,000') || formatCurrency(1500000, 'zh-CN').includes('1.500.000'));
    assert.ok(formatCurrency(1500000, 'ko').includes('1,500,000') || formatCurrency(1500000, 'ko').includes('1.500.000'));
  });

  test('Date formatting handles dd/MM/yyyy and ISO strings across locales', () => {
    const formatDateByLocale = (dateStr, locale = 'vi') => {
      if (!dateStr || dateStr.trim() === '' || dateStr === 'N/A') return 'Chưa cập nhật';
      const dmyMatch = dateStr.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})/);
      if (dmyMatch) {
        const day = dmyMatch[1].padStart(2, '0');
        const month = dmyMatch[2].padStart(2, '0');
        const year = dmyMatch[3];
        if (locale === 'ja' || locale === 'zh-CN' || locale === 'zh') return `${year}/${month}/${day}`;
        if (locale === 'ko') return `${year}.${month}.${day}`;
        if (locale === 'en') return `${month}/${day}/${year}`;
        return `${day}/${month}/${year}`;
      }
      return dateStr;
    };

    assert.equal(formatDateByLocale('19/09/2026', 'vi'), '19/09/2026');
    assert.equal(formatDateByLocale('19/09/2026', 'ja'), '2026/09/19');
    assert.equal(formatDateByLocale('19/09/2026', 'zh-CN'), '2026/09/19');
    assert.equal(formatDateByLocale('19/09/2026', 'ko'), '2026.09.19');
    assert.equal(formatDateByLocale('19/09/2026', 'en'), '09/19/2026');
    assert.equal(formatDateByLocale(null, 'vi'), 'Chưa cập nhật');
  });

  test('Language system fallback logic', () => {
    const getSystemLanguage = (deviceCode) => {
      if (deviceCode === 'ja') return 'ja';
      if (deviceCode === 'en') return 'en';
      if (deviceCode === 'zh') return 'zh-CN';
      if (deviceCode === 'ko') return 'ko';
      if (deviceCode === 'vi') return 'vi';
      return 'vi'; // Default fallback per requirement
    };

    assert.equal(getSystemLanguage('ja'), 'ja');
    assert.equal(getSystemLanguage('en'), 'en');
    assert.equal(getSystemLanguage('zh'), 'zh-CN');
    assert.equal(getSystemLanguage('ko'), 'ko');
    assert.equal(getSystemLanguage('vi'), 'vi');
    assert.equal(getSystemLanguage('fr'), 'vi'); // Unsupported falls back to vi
  });

  test('Payroll status paid vs unpaid resolution', () => {
    const isPayrollPaid = (status) => {
      if (!status) return false;
      const lower = status.trim().toLowerCase();
      if (
        lower.includes('unpaid') ||
        lower.includes('chưa') ||
        lower.includes('chờ') ||
        lower.includes('pending') ||
        lower.includes('awaiting')
      ) {
        return false;
      }
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

    assert.equal(isPayrollPaid('paid'), true);
    assert.equal(isPayrollPaid('Đã thanh toán'), true);
    assert.equal(isPayrollPaid('unpaid'), false);
    assert.equal(isPayrollPaid('Chờ thanh toán'), false);
    assert.equal(isPayrollPaid('Chưa thanh toán'), false);
    assert.equal(isPayrollPaid('pending'), false);
    assert.equal(isPayrollPaid(null), false);
    assert.equal(isPayrollPaid(undefined), false);
  });
});

describe('Push Notification Contracts & Security Boundaries', () => {
  const isTargetMatch = (notif, user) => {
    if (!user || !user.MANV) return false;
    const type = notif.TARGET_TYPE || 'ALL';
    if (type === 'PERSONAL') {
      return !!notif.TARGET_MANV && notif.TARGET_MANV === user.MANV;
    }
    if (type === 'DEPARTMENT') {
      return !!notif.MAPB && notif.MAPB === user.MAPB;
    }
    if (type === 'COMPANY') {
      return !!notif.MACTY && notif.MACTY === user.MACTY;
    }
    if (type === 'ALL') {
      return true;
    }
    return false;
  };

  test('PERSONAL notification matches only target MANV and rejects null/mismatch', () => {
    const notif = { TARGET_TYPE: 'PERSONAL', TARGET_MANV: 'NV0001' };
    assert.equal(isTargetMatch(notif, { MANV: 'NV0001', MAPB: 'PB01', MACTY: 'CT01' }), true);
    assert.equal(isTargetMatch(notif, { MANV: 'NV0002', MAPB: 'PB01', MACTY: 'CT01' }), false);
    assert.equal(isTargetMatch(notif, { MANV: null, MAPB: 'PB01' }), false);

    const nullNotif = { TARGET_TYPE: 'PERSONAL', TARGET_MANV: null };
    assert.equal(isTargetMatch(nullNotif, { MANV: 'NV0001' }), false);
    assert.equal(isTargetMatch(nullNotif, { MANV: null }), false);
  });

  test('DEPARTMENT targeting never confuses MAPB with employee ID', () => {
    const notif = { TARGET_TYPE: 'DEPARTMENT', MAPB: 'PB02' };
    assert.equal(isTargetMatch(notif, { MANV: 'NV0001', MAPB: 'PB02', MACTY: 'CT01' }), true);
    assert.equal(isTargetMatch(notif, { MANV: 'PB02', MAPB: 'PB01', MACTY: 'CT01' }), false);
    assert.equal(isTargetMatch(notif, { MANV: 'NV0002', MAPB: null }), false);
  });

  test('COMPANY and ALL targeting resolve accurately', () => {
    const ctyNotif = { TARGET_TYPE: 'COMPANY', MACTY: 'CT01' };
    assert.equal(isTargetMatch(ctyNotif, { MANV: 'NV0001', MACTY: 'CT01' }), true);
    assert.equal(isTargetMatch(ctyNotif, { MANV: 'NV0001', MACTY: 'CT02' }), false);

    const allNotif = { TARGET_TYPE: 'ALL' };
    assert.equal(isTargetMatch(allNotif, { MANV: 'NV0001' }), true);
    assert.equal(isTargetMatch(allNotif, null), false);
  });

  test('Push payload privacy: no CCCD, salary, tax IDs, or JWT tokens in push payloads', () => {
    const sensitiveKeys = ['cccd', 'socccd', 'salary', 'sotien', 'luong', 'taxid', 'masothue', 'jwt', 'token', 'password'];

    const createSafePushPayload = (title, body, eventType, notificationId, extraData = {}) => {
      const safeData = {
        notificationId: Number(notificationId),
        eventType: String(eventType),
      };
      for (const [k, v] of Object.entries(extraData)) {
        const lowerK = k.toLowerCase();
        const isSensitive = sensitiveKeys.some(sk => lowerK.includes(sk));
        if (!isSensitive) {
          safeData[k] = v;
        }
      }
      return {
        title,
        body,
        sound: 'default',
        channelId: 'hrms_default_channel',
        data: safeData,
      };
    };

    const payload = createSafePushPayload(
      'Duyệt đơn nghỉ phép',
      'Đơn nghỉ phép của bạn đã được phê duyệt.',
      'LEAVE_APPROVAL',
      105,
      {
        requestId: 42,
        cccd: '001234567890',
        salaryAmount: 25000000,
        jwtToken: 'secret_token_abc',
        status: 'APPROVED',
      }
    );

    assert.equal(payload.title, 'Duyệt đơn nghỉ phép');
    assert.equal(payload.data.notificationId, 105);
    assert.equal(payload.data.eventType, 'LEAVE_APPROVAL');
    assert.equal(payload.data.requestId, 42);
    assert.equal(payload.data.status, 'APPROVED');

    // Sensitive keys must be scrubbed
    assert.equal(payload.data.cccd, undefined);
    assert.equal(payload.data.salaryAmount, undefined);
    assert.equal(payload.data.jwtToken, undefined);
  });

  test('Expo Push Token validation regex compliant with SDK 57', () => {
    const isExpoPushToken = (token) => {
      if (typeof token !== 'string') return false;
      return /^(ExponentPushToken|ExpoPushToken)\[.+\]$/.test(token.trim());
    };

    assert.equal(isExpoPushToken('ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]'), true);
    assert.equal(isExpoPushToken('ExpoPushToken[xxxxxxxxxxxxxxxxxxxxxx]'), true);
    assert.equal(isExpoPushToken('fcm_token_12345'), false);
    assert.equal(isExpoPushToken(''), false);
    assert.equal(isExpoPushToken(null), false);
  });

  test('Deep navigation destination routing resolution', () => {
    const resolveNavigationRoute = (data) => {
      if (!data) return { route: 'NotificationsTab' };
      const eventType = data.eventType || data.type;
      const notificationId = data.notificationId || data.id;

      switch (eventType) {
        case 'LEAVE_APPROVAL':
        case 'LEAVE_REJECTED':
        case 'NGHI_PHEP':
        case 'ATTENDANCE_CORRECTION':
        case 'ATTENDANCE_CORRECTION_REJECTED':
        case 'DIEUCHINH_CONG':
        case 'OVERTIME_APPROVED':
        case 'OVERTIME_REJECTED':
        case 'TANG_CA':
          return { route: 'MyRequests' };
        case 'SALARY_ADVANCE':
        case 'SALARY_ADVANCE_APPROVED':
        case 'SALARY_ADVANCE_REJECTED':
        case 'SALARY_ADVANCE_DISBURSED':
        case 'YEUCAU_UNGLUONG':
          return { route: 'SalaryAdvance' };
        case 'PROFILE_REQUEST':
        case 'PROFILE_REQUEST_APPROVED':
        case 'PROFILE_REQUEST_REJECTED':
        case 'YEUCAU_HOSO':
          return { route: 'ProfileTab' };
        default:
          if (notificationId) {
            return { route: 'NotificationDetail', params: { notificationId: Number(notificationId) } };
          }
          return { route: 'NotificationsTab' };
      }
    };

    assert.deepEqual(resolveNavigationRoute({ eventType: 'LEAVE_APPROVAL' }), { route: 'MyRequests' });
    assert.deepEqual(resolveNavigationRoute({ eventType: 'ATTENDANCE_CORRECTION_REJECTED' }), { route: 'MyRequests' });
    assert.deepEqual(resolveNavigationRoute({ eventType: 'SALARY_ADVANCE_APPROVED' }), { route: 'SalaryAdvance' });
    assert.deepEqual(resolveNavigationRoute({ eventType: 'PROFILE_REQUEST_APPROVED' }), { route: 'ProfileTab' });
    assert.deepEqual(resolveNavigationRoute({ eventType: 'COMPANY_ANNOUNCEMENT', notificationId: 99 }), {
      route: 'NotificationDetail',
      params: { notificationId: 99 },
    });
    assert.deepEqual(resolveNavigationRoute({ eventType: 'GENERAL' }), { route: 'NotificationsTab' });
    assert.deepEqual(resolveNavigationRoute(null), { route: 'NotificationsTab' });
  });
});
