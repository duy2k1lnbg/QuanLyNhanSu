import type { ThemeConfig } from 'antd';
import { theme as antdTheme } from 'antd';

export type AppThemeMode = 'light' | 'dark';

export interface ThemeColors {
  appBg: string;
  sidebarBg: string;
  headerBg: string;
  cardBg: string;
  cardSecondaryBg: string;
  elevatedBg: string;
  textPrimary: string;
  textSecondary: string;
  textMuted: string;
  borderSubtle: string;
  borderMedium: string;
  hoverBg: string;
  activeBg: string;
  primary: string;
  primaryHover: string;
  btnPrimaryBg: string;
  btnPrimaryText: string;
  focusRing: string;
  // Status colors
  successBg: string;
  successText: string;
  warningBg: string;
  warningText: string;
  dangerBg: string;
  dangerText: string;
  infoBg: string;
  infoText: string;
  violetBg: string;
  violetText: string;
  tealBg: string;
  tealText: string;
  // Chart colors
  chartPurple: string;
  chartBlue: string;
  chartGreen: string;
  chartOrange: string;
  chartTeal: string;
  chartGrid: string;
  chartTooltipBg: string;
}

export const LIGHT_TOKENS: ThemeColors = {
  appBg: '#F6F7FB',
  sidebarBg: '#FFFFFF',
  headerBg: '#FFFFFF',
  cardBg: '#FFFFFF',
  cardSecondaryBg: '#F9FAFD',
  elevatedBg: '#FFFFFF',
  textPrimary: '#202332',
  textSecondary: '#626B7E',
  textMuted: '#8C98B0',
  borderSubtle: '#E5E8F0',
  borderMedium: '#CBD5E1',
  hoverBg: '#F3F0FF',
  activeBg: '#EEE9FF',
  primary: '#6D4AFF',
  primaryHover: '#5835EB',
  btnPrimaryBg: '#6D4AFF',
  btnPrimaryText: '#FFFFFF',
  focusRing: 'rgba(109, 74, 255, 0.25)',
  // Status
  successBg: '#EAFBF1',
  successText: '#237A50',
  warningBg: '#FEF6E7',
  warningText: '#96610D',
  dangerBg: '#FDF0F2',
  dangerText: '#C6384F',
  infoBg: '#EFF6FF',
  infoText: '#2E63C4',
  violetBg: '#F3F0FF',
  violetText: '#6D4AFF',
  tealBg: '#E6FFFA',
  tealText: '#0D9488',
  // Chart
  chartPurple: '#6D4AFF',
  chartBlue: '#3B82F6',
  chartGreen: '#10B981',
  chartOrange: '#F59E0B',
  chartTeal: '#14B8A6',
  chartGrid: '#E2E8F0',
  chartTooltipBg: '#FFFFFF',
};

export const DARK_TOKENS: ThemeColors = {
  appBg: '#080B14',
  sidebarBg: '#0C101C',
  headerBg: '#0C101C',
  cardBg: '#101522',
  cardSecondaryBg: '#151B2B',
  elevatedBg: '#1A2235',
  textPrimary: '#F1F3F9',
  textSecondary: '#B3BCD0',
  textMuted: '#8C98B0',
  borderSubtle: '#1E2638',
  borderMedium: '#283148',
  hoverBg: '#1A2033',
  activeBg: '#281D49',
  primary: '#9A79FF',
  primaryHover: '#B398FF',
  btnPrimaryBg: '#7150DA',
  btnPrimaryText: '#FFFFFF',
  focusRing: 'rgba(154, 121, 255, 0.35)',
  // Status
  successBg: 'rgba(35, 122, 80, 0.22)',
  successText: '#68D5A1',
  warningBg: 'rgba(150, 97, 13, 0.25)',
  warningText: '#F2BB62',
  dangerBg: 'rgba(198, 56, 79, 0.25)',
  dangerText: '#FF8C9E',
  infoBg: 'rgba(46, 99, 196, 0.22)',
  infoText: '#8AB9FF',
  violetBg: 'rgba(109, 74, 255, 0.22)',
  violetText: '#B398FF',
  tealBg: 'rgba(13, 148, 136, 0.22)',
  tealText: '#2DD4BF',
  // Chart
  chartPurple: '#9A79FF',
  chartBlue: '#60A5FA',
  chartGreen: '#34D399',
  chartOrange: '#FBBF24',
  chartTeal: '#2DD4BF',
  chartGrid: '#1E2638',
  chartTooltipBg: '#1A2235',
};

export const getAntdThemeConfig = (mode: AppThemeMode): ThemeConfig => {
  const isDark = mode === 'dark';
  const tokens = isDark ? DARK_TOKENS : LIGHT_TOKENS;

  return {
    algorithm: isDark ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
    token: {
      colorPrimary: tokens.primary,
      colorInfo: tokens.primary,
      colorSuccess: isDark ? '#68D5A1' : '#237A50',
      colorWarning: isDark ? '#F2BB62' : '#96610D',
      colorError: isDark ? '#FF8C9E' : '#C6384F',
      colorTextBase: tokens.textPrimary,
      colorBgBase: isDark ? tokens.cardBg : '#FFFFFF',
      colorBgContainer: tokens.cardBg,
      colorBgElevated: tokens.elevatedBg,
      colorBgLayout: tokens.appBg,
      colorBorder: tokens.borderMedium,
      colorBorderSecondary: tokens.borderSubtle,
      borderRadius: 8,
      borderRadiusLG: 12,
      borderRadiusSM: 6,
      fontFamily: "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
      fontSize: 14,
      controlHeight: 36,
    },
    components: {
      Layout: {
        bodyBg: tokens.appBg,
        headerBg: tokens.headerBg,
        siderBg: tokens.sidebarBg,
      },
      Menu: {
        itemBg: 'transparent',
        itemColor: tokens.textSecondary,
        itemHoverBg: tokens.hoverBg,
        itemHoverColor: tokens.primary,
        itemSelectedBg: tokens.activeBg,
        itemSelectedColor: tokens.primary,
        itemBorderRadius: 10,
        subMenuItemBg: 'transparent',
      },
      Card: {
        colorBgContainer: tokens.cardBg,
        colorBorderSecondary: tokens.borderSubtle,
        borderRadiusLG: 12,
      },
      Table: {
        colorBgContainer: tokens.cardBg,
        headerBg: tokens.cardSecondaryBg,
        headerColor: tokens.textSecondary,
        borderColor: tokens.borderSubtle,
        rowHoverBg: tokens.hoverBg,
        headerBorderRadius: 8,
      },
      Button: {
        borderRadius: 8,
        primaryColor: tokens.btnPrimaryText,
        colorBgContainer: tokens.cardBg,
        defaultBorderColor: tokens.borderMedium,
        defaultColor: tokens.textPrimary,
      },
      Input: {
        colorBgContainer: isDark ? tokens.cardSecondaryBg : '#FFFFFF',
        colorBorder: tokens.borderMedium,
        activeBorderColor: tokens.primary,
        hoverBorderColor: tokens.primary,
        borderRadius: 8,
      },
      Select: {
        colorBgContainer: isDark ? tokens.cardSecondaryBg : '#FFFFFF',
        colorBorder: tokens.borderMedium,
        borderRadius: 8,
      },
      DatePicker: {
        colorBgContainer: isDark ? tokens.cardSecondaryBg : '#FFFFFF',
        colorBorder: tokens.borderMedium,
        borderRadius: 8,
      },
      Modal: {
        contentBg: tokens.elevatedBg,
        headerBg: tokens.elevatedBg,
        titleColor: tokens.textPrimary,
        borderRadiusLG: 16,
      },
      Drawer: {
        colorBgElevated: tokens.elevatedBg,
      },
      Dropdown: {
        colorBgElevated: tokens.elevatedBg,
        borderRadiusLG: 10,
      },
      Popover: {
        colorBgElevated: tokens.elevatedBg,
        borderRadiusLG: 12,
      },
      Tooltip: {
        colorBgSpotlight: isDark ? '#1E2638' : '#1E293B',
      },
      Tag: {
        borderRadiusSM: 6,
      },
      Tabs: {
        itemColor: tokens.textSecondary,
        itemSelectedColor: tokens.primary,
        itemHoverColor: tokens.primaryHover,
        inkBarColor: tokens.primary,
      },
      Pagination: {
        itemBg: tokens.cardBg,
      },
    },
  };
};
