import { useState, useEffect, useMemo, useCallback } from 'react';
import {
  LOCALES,
  ANTD_LOCALES,
  LANGUAGE_CONFIG,
  type SupportedLanguage,
  type LocaleType,
} from '../locales';

export type LanguagePreference = 'system' | SupportedLanguage;
export type AppLanguage = SupportedLanguage;
export { LANGUAGE_CONFIG as I18N_CONFIG };
export { LOCALES as I18N_DICTIONARY };

const LANGUAGE_STORAGE_KEY = 'app_language';
const LANGUAGE_EVENT_NAME = 'hrms_language_change';

export const SUPPORTED_LANGUAGES: AppLanguage[] = ['vi', 'en', 'zh-CN', 'ko', 'ja'];

/**
 * Detects browser system language prioritizing navigator.languages.
 * Normalizes BCP 47 tags: vi-VN -> vi, en-US/en-GB -> en, ja-JP -> ja, ko-KR -> ko,
 * zh-CN/zh-Hans -> zh-CN.
 * If Traditional Chinese (zh-TW/zh-Hant) is encountered, skips to next preferred
 * language since only Simplified Chinese is supported, and falls back to 'vi'.
 */
export function detectBrowserLanguage(): AppLanguage {
  if (typeof navigator === 'undefined') return 'vi';

  const candidates: string[] = [];
  if (Array.isArray(navigator.languages) && navigator.languages.length > 0) {
    candidates.push(...navigator.languages);
  } else if (navigator.language) {
    candidates.push(navigator.language);
  }

  for (const raw of candidates) {
    if (!raw || typeof raw !== 'string') continue;
    const lower = raw.trim().toLowerCase();

    // Vietnamese: vi, vi-VN, vi-latn, etc.
    if (lower === 'vi' || lower.startsWith('vi-')) {
      return 'vi';
    }
    // English: en, en-US, en-GB, en-AU, etc.
    if (lower === 'en' || lower.startsWith('en-')) {
      return 'en';
    }
    // Japanese: ja, ja-JP, etc.
    if (lower === 'ja' || lower.startsWith('ja-')) {
      return 'ja';
    }
    // Korean: ko, ko-KR, etc.
    if (lower === 'ko' || lower.startsWith('ko-')) {
      return 'ko';
    }
    // Simplified Chinese: zh-cn, zh-sg, zh-hans, zh-hans-cn, or plain zh
    if (
      lower === 'zh-cn' ||
      lower === 'zh-sg' ||
      lower === 'zh-hans' ||
      lower === 'zh-hans-cn' ||
      lower === 'zh'
    ) {
      return 'zh-CN';
    }
    // Traditional Chinese (zh-tw, zh-hk, zh-mo, zh-hant):
    // Application provides Simplified Chinese only; proceed to next user preference.
  }

  return 'vi';
}

/**
 * Retrieves the saved preference from localStorage ('system' | SupportedLanguage).
 * Defaults to 'system' if not set or invalid.
 */
export const getSavedPreference = (): LanguagePreference => {
  if (typeof window === 'undefined') return 'system';
  try {
    const saved = localStorage.getItem(LANGUAGE_STORAGE_KEY);
    if (saved === 'system') return 'system';
    if (saved && (SUPPORTED_LANGUAGES as readonly string[]).includes(saved)) {
      return saved as SupportedLanguage;
    }
  } catch {
    // fallback if localStorage is blocked
  }
  return 'system';
};

/**
 * Computes the effective language from a preference.
 */
export const getEffectiveLanguage = (pref: LanguagePreference = getSavedPreference()): AppLanguage => {
  if (pref === 'system') {
    return detectBrowserLanguage();
  }
  return pref;
};

/**
 * Backward-compatible helper returning the effective language.
 */
export const getSavedLanguage = (): AppLanguage => {
  return getEffectiveLanguage(getSavedPreference());
};

/**
 * Saves language preference ('system' | SupportedLanguage) to localStorage
 * and notifies active listeners & sets document.documentElement.lang.
 */
export const setSavedPreference = (pref: LanguagePreference): void => {
  if (typeof window === 'undefined') return;
  try {
    localStorage.setItem(LANGUAGE_STORAGE_KEY, pref);
    const effective = getEffectiveLanguage(pref);
    if (typeof document !== 'undefined') {
      document.documentElement.lang = effective;
    }
    window.dispatchEvent(
      new CustomEvent(LANGUAGE_EVENT_NAME, {
        detail: { preference: pref, effective },
      })
    );
  } catch (err) {
    console.error('Failed to persist language preference:', err);
  }
};

/**
 * Backward-compatible alias for setSavedPreference.
 */
export const setSavedLanguage = (lang: LanguagePreference): void => {
  setSavedPreference(lang);
};

/**
 * Formats a localized label for the system mode menu entry.
 */
export function getSystemLanguageLabel(
  systemEffectiveLang: AppLanguage = detectBrowserLanguage(),
  uiLang: AppLanguage = systemEffectiveLang
): string {
  const nativeName = LANGUAGE_CONFIG[systemEffectiveLang]?.nativeName || systemEffectiveLang;
  switch (uiLang) {
    case 'en':
      return `Auto — System (${nativeName})`;
    case 'ja':
      return `自動 — システム設定 (${nativeName})`;
    case 'zh-CN':
      return `自动 — 遵循系统 (${nativeName})`;
    case 'ko':
      return `자동 — 시스템 설정 (${nativeName})`;
    case 'vi':
    default:
      return `Tự động — theo hệ thống (${nativeName})`;
  }
}

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
 * Hook for components to access language preference, effective language,
 * translations, and Ant Design locale with multi-tab & languagechange listeners.
 */
export function useAppLanguage() {
  const [preference, setPreferenceState] = useState<LanguagePreference>(getSavedPreference);
  const [lang, setLangState] = useState<AppLanguage>(() => getEffectiveLanguage(getSavedPreference()));

  useEffect(() => {
    if (typeof document !== 'undefined') {
      document.documentElement.lang = lang;
    }
  }, [lang]);

  useEffect(() => {
    const handleCustomChange = (e: Event) => {
      const customEvent = e as CustomEvent<{ preference?: LanguagePreference; effective?: AppLanguage } | LanguagePreference>;
      if (customEvent.detail) {
        if (typeof customEvent.detail === 'object' && customEvent.detail.preference) {
          setPreferenceState(customEvent.detail.preference);
          setLangState(customEvent.detail.effective || getEffectiveLanguage(customEvent.detail.preference));
          return;
        } else if (typeof customEvent.detail === 'string') {
          const pref = customEvent.detail as LanguagePreference;
          setPreferenceState(pref);
          setLangState(getEffectiveLanguage(pref));
          return;
        }
      }
      const pref = getSavedPreference();
      setPreferenceState(pref);
      setLangState(getEffectiveLanguage(pref));
    };

    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === LANGUAGE_STORAGE_KEY) {
        const pref = getSavedPreference();
        setPreferenceState(pref);
        setLangState(getEffectiveLanguage(pref));
      }
    };

    const handleSysLanguageChange = () => {
      if (getSavedPreference() === 'system') {
        const newEffective = detectBrowserLanguage();
        setLangState(newEffective);
        if (typeof document !== 'undefined') {
          document.documentElement.lang = newEffective;
        }
      }
    };

    window.addEventListener(LANGUAGE_EVENT_NAME, handleCustomChange);
    window.addEventListener('storage', handleStorageChange);
    window.addEventListener('languagechange', handleSysLanguageChange);

    return () => {
      window.removeEventListener(LANGUAGE_EVENT_NAME, handleCustomChange);
      window.removeEventListener('storage', handleStorageChange);
      window.removeEventListener('languagechange', handleSysLanguageChange);
    };
  }, []);

  const changePreference = useCallback((newPref: LanguagePreference) => {
    setPreferenceState(newPref);
    const effective = getEffectiveLanguage(newPref);
    setLangState(effective);
    setSavedPreference(newPref);
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

  const systemEffectiveLang = useMemo(() => detectBrowserLanguage(), []);
  const systemLabel = useMemo(
    () => getSystemLanguageLabel(detectBrowserLanguage(), lang),
    [lang]
  );

  return {
    lang,
    preference,
    setLang: changePreference,
    setPreference: changePreference,
    systemEffectiveLang,
    systemLabel,
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
  if (lower === 'chờ thanh toán' || lower === 'unpaid' || lower === 'cho thanh toan' || lower === 'chưa thanh toán') {
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
