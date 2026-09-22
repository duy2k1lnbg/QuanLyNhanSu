import { useState, useEffect, useMemo, useCallback } from 'react';
import {
  LOCALES,
  ANTD_LOCALES,
  LANGUAGE_CONFIG,
  type SupportedLanguage,
  type LocaleType,
} from '../locales';

export type AppLanguage = SupportedLanguage;
export { LANGUAGE_CONFIG as I18N_CONFIG };
export { LOCALES as I18N_DICTIONARY };

const LANGUAGE_STORAGE_KEY = 'app_language';
const LANGUAGE_EVENT_NAME = 'hrms_language_change';

export const SUPPORTED_LANGUAGES: AppLanguage[] = ['vi', 'en', 'zh-CN', 'ko', 'ja'];

export const getSavedLanguage = (): AppLanguage => {
  if (typeof window === 'undefined') return 'vi';
  try {
    const saved = localStorage.getItem(LANGUAGE_STORAGE_KEY) as AppLanguage | null;
    if (saved && SUPPORTED_LANGUAGES.includes(saved)) {
      return saved;
    }
  } catch {
    // fallback
  }
  return 'vi';
};

export const setSavedLanguage = (lang: AppLanguage): void => {
  if (typeof window === 'undefined') return;
  try {
    localStorage.setItem(LANGUAGE_STORAGE_KEY, lang);
    window.dispatchEvent(new CustomEvent(LANGUAGE_EVENT_NAME, { detail: lang }));
  } catch (err) {
    console.error('Failed to persist language:', err);
  }
};

/**
 * Universal nested key resolver with interpolation
 * e.g. resolveKey(dict, 'employee.colEmpCode')
 * e.g. resolveKey(dict, 'common.totalRecords', { total: 42 })
 */
export function resolveTranslation(
  dict: any,
  fallbackDict: any,
  key: string,
  params?: Record<string, string | number>
): string {
  if (!key) return '';
  const parts = key.split('.');
  let current: any = dict;
  let fallbackCurrent: any = fallbackDict;

  for (const part of parts) {
    if (current && typeof current === 'object' && part in current) {
      current = current[part];
    } else {
      current = undefined;
    }
    if (fallbackCurrent && typeof fallbackCurrent === 'object' && part in fallbackCurrent) {
      fallbackCurrent = fallbackCurrent[part];
    } else {
      fallbackCurrent = undefined;
    }
  }

  let text = typeof current === 'string' ? current : (typeof fallbackCurrent === 'string' ? fallbackCurrent : key);

  if (import.meta.env?.DEV && text === key) {
    console.warn(`[i18n] Missing translation key: "${key}"`);
  }

  if (params && typeof text === 'string') {
    Object.entries(params).forEach(([paramKey, paramVal]) => {
      text = text.replace(new RegExp(`\\{${paramKey}\\}`, 'g'), String(paramVal));
    });
  }

  return text;
}

/**
 * Hook for components to access language, translations, and Ant Design locale
 */
export function useAppLanguage() {
  const [lang, setLangState] = useState<AppLanguage>(getSavedLanguage);

  useEffect(() => {
    const handleLangChange = (e: Event) => {
      const customEvent = e as CustomEvent<AppLanguage>;
      if (customEvent.detail && SUPPORTED_LANGUAGES.includes(customEvent.detail)) {
        setLangState(customEvent.detail);
      } else {
        setLangState(getSavedLanguage());
      }
    };

    window.addEventListener(LANGUAGE_EVENT_NAME, handleLangChange);
    window.addEventListener('storage', handleLangChange);

    return () => {
      window.removeEventListener(LANGUAGE_EVENT_NAME, handleLangChange);
      window.removeEventListener('storage', handleLangChange);
    };
  }, []);

  const changeLanguage = useCallback((newLang: AppLanguage) => {
    if (SUPPORTED_LANGUAGES.includes(newLang)) {
      setLangState(newLang);
      setSavedLanguage(newLang);
    }
  }, []);

  const dict: LocaleType = useMemo(() => LOCALES[lang] || LOCALES.vi, [lang]);
  const antdLocale = useMemo(() => ANTD_LOCALES[lang] || ANTD_LOCALES.vi, [lang]);

  const t = useCallback(
    (key: string, params?: Record<string, string | number>): string => {
      return resolveTranslation(dict, LOCALES.vi, key, params);
    },
    [dict]
  );

  // Backward-compatible tLanding, tAuth and tApp proxies
  const tLanding = useMemo(() => dict.landing, [dict]);
  const tAuth = useMemo(() => dict.auth, [dict]);

  const tApp = useMemo(() => {
    return {
      ...dict.app,
      systemOnlineTooltip: (periods: number) =>
        resolveTranslation(dict, LOCALES.vi, 'app.systemOnlineTooltip', { periods }),
      userAccount: (username: string) =>
        resolveTranslation(dict, LOCALES.vi, 'app.userAccount', { username }),
      roleStaff: (count: number) =>
        resolveTranslation(dict, LOCALES.vi, 'app.roleStaff', { count }),
    };
  }, [dict]);

  return {
    lang,
    setLang: changeLanguage,
    t,
    dict,
    antdLocale,
    tLanding,
    tAuth,
    tApp,
    config: LANGUAGE_CONFIG[lang],
    allConfigs: LANGUAGE_CONFIG,
  };
}

/**
 * Normalizes backend/system status values to localized labels and Ant Design tag colors
 */
export function getLocalizedStatus(
  statusCodeOrText: string | number | boolean | null | undefined,
  t: (key: string) => string
): { label: string; color: string } {
  if (statusCodeOrText === null || statusCodeOrText === undefined) {
    return { label: t('common.noData'), color: 'default' };
  }

  const raw = String(statusCodeOrText).trim();
  const lower = raw.toLowerCase();

  // Active / Inactive statuses
  if (lower === 'active' || lower === 'đang làm việc' || lower === '1' || statusCodeOrText === true) {
    return { label: t('status.active'), color: 'success' };
  }
  if (lower === 'resigned' || lower === 'đã thôi việc' || lower === 'thôi việc' || lower === '0' || statusCodeOrText === false) {
    return { label: t('status.resigned'), color: 'default' };
  }
  if (lower === 'inactive' || lower === 'ngưng hoạt động') {
    return { label: t('status.inactive'), color: 'default' };
  }
  if (lower === 'suspended' || lower === 'bị tạm khóa') {
    return { label: t('status.suspended'), color: 'warning' };
  }
  if (lower === 'locked' || lower === 'đã khóa') {
    return { label: t('status.locked'), color: 'error' };
  }

  // Approval statuses
  if (lower === 'pending' || lower === 'chờ phê duyệt' || lower === 'chờ duyệt') {
    return { label: t('status.pending'), color: 'processing' };
  }
  if (lower === 'approved' || lower === 'đã phê duyệt' || lower === 'đã duyệt') {
    return { label: t('status.approved'), color: 'success' };
  }
  if (lower === 'rejected' || lower === 'đã từ chối' || lower === 'từ chối') {
    return { label: t('status.rejected'), color: 'error' };
  }

  // Attendance statuses
  if (lower === 'đủ công' || lower === 'fullday' || lower === 'du cong') {
    return { label: t('status.fullDay'), color: 'success' };
  }
  if (lower === 'nửa công' || lower === 'halfday') {
    return { label: t('status.halfDay'), color: 'warning' };
  }
  if (lower === 'vắng mặt' || lower === 'absent') {
    return { label: t('status.absent'), color: 'error' };
  }
  if (lower === 'nghỉ phép' || lower === 'nghỉ phép hưởng lương' || lower === 'leavepaid') {
    return { label: t('status.leavePaid'), color: 'blue' };
  }
  if (lower === 'nghỉ không lương' || lower === 'leaveunpaid') {
    return { label: t('status.leaveUnpaid'), color: 'purple' };
  }

  // Payment statuses
  if (lower === 'đã thanh toán' || lower === 'paid' || lower === 'da thanh toan') {
    return { label: t('status.paid'), color: 'success' };
  }
  if (lower === 'chờ thanh toán' || lower === 'unpaid' || lower === 'cho thanh toan') {
    return { label: t('status.unpaid'), color: 'warning' };
  }

  // Contract statuses
  if (lower === 'đang hiệu lực' || lower === 'hợp lệ' || lower === 'valid') {
    return { label: t('contract.statusActive'), color: 'success' };
  }
  if (lower === 'đã hết hạn' || lower === 'expired') {
    return { label: t('contract.statusExpired'), color: 'error' };
  }
  if (lower === 'sắp hết hạn' || lower === 'nearexpire') {
    return { label: t('contract.statusNearExpire'), color: 'warning' };
  }

  return { label: raw, color: 'default' };
}
