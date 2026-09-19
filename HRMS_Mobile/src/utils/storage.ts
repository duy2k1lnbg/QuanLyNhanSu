import * as SecureStore from 'expo-secure-store';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { APP_CONFIG } from '../config';

// 1. Quản lý Token và Dữ liệu nhạy cảm bằng SecureStore (mã hóa chuẩn phần cứng Keystore)
export const SecureTokenStorage = {
  async saveToken(token: string): Promise<void> {
    try {
      await SecureStore.setItemAsync(APP_CONFIG.storageKeys.token, token);
    } catch (e) {
      console.warn('[SecureStore] Save token error:', e);
    }
  },

  async getToken(): Promise<string | null> {
    try {
      return await SecureStore.getItemAsync(APP_CONFIG.storageKeys.token);
    } catch (e) {
      console.warn('[SecureStore] Get token error:', e);
      return null;
    }
  },

  async removeToken(): Promise<void> {
    try {
      await SecureStore.deleteItemAsync(APP_CONFIG.storageKeys.token);
    } catch (e) {
      console.warn('[SecureStore] Delete token error:', e);
    }
  },
};

// 2. Quản lý Tùy chọn cấu hình không nhạy cảm (Theme, Ngôn ngữ, Cache) bằng AsyncStorage
export const AppPreferencesStorage = {
  async setItem(key: string, value: string): Promise<void> {
    try {
      await AsyncStorage.setItem(key, value);
    } catch (e) {
      console.warn('[AsyncStorage] setItem error:', e);
    }
  },

  async getItem(key: string): Promise<string | null> {
    try {
      return await AsyncStorage.getItem(key);
    } catch (e) {
      console.warn('[AsyncStorage] getItem error:', e);
      return null;
    }
  },

  async removeItem(key: string): Promise<void> {
    try {
      await AsyncStorage.removeItem(key);
    } catch (e) {
      console.warn('[AsyncStorage] removeItem error:', e);
    }
  },
};

// 3. Unified storage facade
export const storage = {
  saveToken: SecureTokenStorage.saveToken,
  getToken: SecureTokenStorage.getToken,
  removeToken: SecureTokenStorage.removeToken,

  async getLanguagePreference(): Promise<string | null> {
    return await AppPreferencesStorage.getItem(APP_CONFIG.storageKeys.language);
  },

  async setLanguagePreference(lang: string): Promise<void> {
    await AppPreferencesStorage.setItem(APP_CONFIG.storageKeys.language, lang);
  },

  async getThemePreference(): Promise<string | null> {
    return await AppPreferencesStorage.getItem(APP_CONFIG.storageKeys.theme);
  },

  async setThemePreference(theme: string): Promise<void> {
    await AppPreferencesStorage.setItem(APP_CONFIG.storageKeys.theme, theme);
  },

  async getFirstRun(): Promise<boolean> {
    const val = await AppPreferencesStorage.getItem('hrms_has_run_before');
    return val !== 'true';
  },

  async setHasRunBefore(): Promise<void> {
    await AppPreferencesStorage.setItem('hrms_has_run_before', 'true');
  },
};
