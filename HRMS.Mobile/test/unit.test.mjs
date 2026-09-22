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
});
