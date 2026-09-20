import React, { createContext, useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { storage } from '../utils/storage';
import { getSystemLanguage, SupportedLanguage } from './i18n';

export type LanguagePreference = SupportedLanguage | 'system';

interface LanguageContextType {
  preference: LanguagePreference;
  activeLanguage: SupportedLanguage;
  setLanguage: (pref: LanguagePreference) => Promise<void>;
  isInitialized: boolean;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { i18n } = useTranslation();
  const [preference, setPreference] = useState<LanguagePreference>('system');
  const [activeLanguage, setActiveLanguage] = useState<SupportedLanguage>('vi');
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    const init = async () => {
      const savedPref = await storage.getLanguagePreference();
      let pref: LanguagePreference = 'system';
      if (savedPref === 'vi' || savedPref === 'ja' || savedPref === 'en' || savedPref === 'system') {
        pref = savedPref;
      }
      setPreference(pref);

      let effectiveLang: SupportedLanguage;
      if (pref === 'system') {
        effectiveLang = getSystemLanguage();
      } else {
        effectiveLang = pref;
      }

      setActiveLanguage(effectiveLang);
      await i18n.changeLanguage(effectiveLang);
      setIsInitialized(true);
    };
    init();
  }, [i18n]);

  const setLanguage = async (newPref: LanguagePreference) => {
    setPreference(newPref);
    await storage.setLanguagePreference(newPref);

    let effectiveLang: SupportedLanguage;
    if (newPref === 'system') {
      effectiveLang = getSystemLanguage();
    } else {
      effectiveLang = newPref;
    }

    setActiveLanguage(effectiveLang);
    await i18n.changeLanguage(effectiveLang);
  };

  return (
    <LanguageContext.Provider value={{ preference, activeLanguage, setLanguage, isInitialized }}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = (): LanguageContextType => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};
