import React, { createContext, useContext, useState, useEffect, useMemo, useCallback } from 'react';
import { ConfigProvider, App as AntdApp } from 'antd';
import {
  type AppThemeMode,
  type ThemeColors,
  LIGHT_TOKENS,
  DARK_TOKENS,
  getAntdThemeConfig,
} from './tokens';

export const THEME_STORAGE_KEY = 'hrms_web_theme';

interface ThemeContextValue {
  theme: AppThemeMode;
  mode: AppThemeMode;
  isDark: boolean;
  tokens: ThemeColors;
  toggleTheme: () => void;
  setTheme: (mode: AppThemeMode) => void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

export const getSavedTheme = (): AppThemeMode => {
  if (typeof window === 'undefined') return 'light';
  try {
    const saved = localStorage.getItem(THEME_STORAGE_KEY) as AppThemeMode | null;
    if (saved === 'dark' || saved === 'light') {
      return saved;
    }
  } catch {
    // fallback if localStorage not accessible
  }
  return 'light';
};

interface AppThemeProviderProps {
  children: React.ReactNode;
  locale?: any;
}

export const AppThemeProvider: React.FC<AppThemeProviderProps> = ({ children, locale }) => {
  const [theme, setThemeState] = useState<AppThemeMode>(getSavedTheme);

  const setTheme = useCallback((newTheme: AppThemeMode) => {
    setThemeState(newTheme);
    try {
      localStorage.setItem(THEME_STORAGE_KEY, newTheme);
    } catch {
      // ignore
    }
  }, []);

  const toggleTheme = useCallback(() => {
    setThemeState((prev: AppThemeMode) => {
      const next: AppThemeMode = prev === 'light' ? 'dark' : 'light';
      try {
        localStorage.setItem(THEME_STORAGE_KEY, next);
      } catch {
        // ignore
      }
      return next;
    });
  }, []);

  // Sync with document element and CSS variables
  useEffect(() => {
    const root = document.documentElement;
    root.setAttribute('data-theme', theme);
    root.style.colorScheme = theme;

    const tokens = theme === 'dark' ? DARK_TOKENS : LIGHT_TOKENS;
    // Set CSS variables for non-AntD styling, charts, scrollbar, SVG
    root.style.setProperty('--color-app-bg', tokens.appBg);
    root.style.setProperty('--color-sidebar-bg', tokens.sidebarBg);
    root.style.setProperty('--color-header-bg', tokens.headerBg);
    root.style.setProperty('--color-card-bg', tokens.cardBg);
    root.style.setProperty('--color-card-secondary-bg', tokens.cardSecondaryBg);
    root.style.setProperty('--color-elevated-bg', tokens.elevatedBg);
    root.style.setProperty('--color-text-primary', tokens.textPrimary);
    root.style.setProperty('--color-text-secondary', tokens.textSecondary);
    root.style.setProperty('--color-text-muted', tokens.textMuted);
    root.style.setProperty('--color-border-subtle', tokens.borderSubtle);
    root.style.setProperty('--color-border-medium', tokens.borderMedium);
    root.style.setProperty('--color-hover-bg', tokens.hoverBg);
    root.style.setProperty('--color-active-bg', tokens.activeBg);
    root.style.setProperty('--color-primary', tokens.primary);
    root.style.setProperty('--color-primary-hover', tokens.primaryHover);
    root.style.setProperty('--color-btn-primary-bg', tokens.btnPrimaryBg);
    root.style.setProperty('--color-btn-primary-text', tokens.btnPrimaryText);
  }, [theme]);

  const isDark = theme === 'dark';
  const tokens = isDark ? DARK_TOKENS : LIGHT_TOKENS;
  const antdThemeConfig = useMemo(() => getAntdThemeConfig(theme), [theme]);

  const contextValue = useMemo(
    () => ({
      theme,
      mode: theme,
      isDark,
      tokens,
      toggleTheme,
      setTheme,
    }),
    [theme, isDark, tokens, toggleTheme, setTheme]
  );

  return (
    <ThemeContext.Provider value={contextValue}>
      <ConfigProvider theme={antdThemeConfig} locale={locale}>
        <AntdApp>
          {children}
        </AntdApp>
      </ConfigProvider>
    </ThemeContext.Provider>
  );
};

export const useAppTheme = (): ThemeContextValue => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useAppTheme must be used within an AppThemeProvider');
  }
  return context;
};
