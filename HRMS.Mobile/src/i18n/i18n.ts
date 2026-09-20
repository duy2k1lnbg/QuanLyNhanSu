import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import { getLocales } from 'expo-localization';
import vi from './locales/vi.json';
import ja from './locales/ja.json';
import en from './locales/en.json';
import { storage } from '../utils/storage';

export const SUPPORTED_LANGUAGES = ['vi', 'ja', 'en'] as const;
export type SupportedLanguage = typeof SUPPORTED_LANGUAGES[number];

export const getSystemLanguage = (): SupportedLanguage => {
  try {
    const locales = getLocales();
    if (locales && locales.length > 0) {
      const languageCode = locales[0].languageCode?.toLowerCase();
      if (languageCode === 'ja') return 'ja';
      if (languageCode === 'en') return 'en';
      if (languageCode === 'vi') return 'vi';
    }
  } catch (error) {
    console.warn('Error detecting system language:', error);
  }
  return 'vi'; // Default fallback
};

export const initializeI18n = async () => {
  let savedPref = await storage.getLanguagePreference();
  let activeLang: SupportedLanguage;

  if (savedPref && savedPref !== 'system' && (SUPPORTED_LANGUAGES as readonly string[]).includes(savedPref)) {
    activeLang = savedPref as SupportedLanguage;
  } else {
    activeLang = getSystemLanguage();
  }

  await i18n.use(initReactI18next).init({
    resources: {
      vi: { translation: vi },
      ja: { translation: ja },
      en: { translation: en },
    },
    lng: activeLang,
    fallbackLng: 'vi',
    interpolation: {
      escapeValue: false,
    },
    compatibilityJSON: 'v4',
  });

  return activeLang;
};

export default i18n;
