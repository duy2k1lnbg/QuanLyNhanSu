import { vi, type LocaleType } from './vi';
import { en } from './en';
import { zhCN } from './zh-CN';
import { ko } from './ko';
import { ja } from './ja';

import antdViVN from 'antd/locale/vi_VN';
import antdEnUS from 'antd/locale/en_US';
import antdZhCN from 'antd/locale/zh_CN';
import antdKoKR from 'antd/locale/ko_KR';
import antdJaJP from 'antd/locale/ja_JP';
import type { Locale as AntdLocale } from 'antd/es/locale';

export type SupportedLanguage = 'vi' | 'en' | 'zh-CN' | 'ko' | 'ja';
export type { LocaleType };

export const LOCALES: Record<SupportedLanguage, LocaleType> = {
  vi,
  en,
  'zh-CN': zhCN,
  ko,
  ja,
};

export const ANTD_LOCALES: Record<SupportedLanguage, AntdLocale> = {
  vi: antdViVN,
  en: antdEnUS,
  'zh-CN': antdZhCN,
  ko: antdKoKR,
  ja: antdJaJP,
};

export const LANGUAGE_CONFIG: Record<
  SupportedLanguage,
  { name: string; nativeName: string; short: string; flag: string }
> = {
  vi: {
    name: 'Tiếng Việt',
    nativeName: 'Tiếng Việt',
    short: 'VI',
    flag: '🇻🇳',
  },
  en: {
    name: 'English',
    nativeName: 'English',
    short: 'EN',
    flag: '🇬🇧',
  },
  'zh-CN': {
    name: 'Simplified Chinese',
    nativeName: '简体中文',
    short: 'ZH',
    flag: '🇨🇳',
  },
  ko: {
    name: 'Korean',
    nativeName: '한국어',
    short: 'KO',
    flag: '🇰🇷',
  },
  ja: {
    name: 'Japanese',
    nativeName: '日本語',
    short: 'JA',
    flag: '🇯🇵',
  },
};
