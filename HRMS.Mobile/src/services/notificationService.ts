import { Platform, Linking } from 'react-native';
import {canUseNativeNotifications,canUseRemoteNotifications,getRemoteNotifications} from './remoteNotifications';
import type {NotificationResponse} from 'expo-notifications';
import * as Device from 'expo-device';
import Constants from 'expo-constants';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { meApi } from '../api/meApi';
import { RegisterDeviceDto } from '../types/me';
import i18n from '../i18n/i18n';
import { getInstallationId } from '../utils/deviceIdentity';
import { notificationDestination } from '../utils/requestContracts';

let tokenError: string|null=null;
let registrationAllowed=true;
const registrations=new Set<Promise<DeviceStatusInfo>>();
const ASYNC_KEY_PERMISSION_PROMPTED = 'HRMS_NOTIFICATION_PERMISSION_PROMPTED';
export const HRMS_NOTIFICATION_CHANNEL_ID = 'hrms_default_channel';
const ASYNC_KEY_PUSH_ENABLED = 'HRMS_NOTIFICATION_PUSH_ENABLED';

export interface DeviceStatusInfo {
  isRegistered: boolean;
  pushToken: string | null;
  permissionStatus: 'GRANTED' | 'DENIED' | 'UNDETERMINED';
  canAskAgain: boolean;
  pushEnabled: boolean;
  isPhysicalDevice: boolean;
  errorMessage: string | null;
}

export const notificationService = {
  canRequestPermission: canUseNativeNotifications,
  resumeRegistration(): void { registrationAllowed=true; },
  isRemotePushConfigured(): boolean {
    return canUseRemoteNotifications();
  },
  subscribeReceived(listener: () => void): () => void {
    const subscription=getRemoteNotifications()?.addNotificationReceivedListener(listener);
    return () => subscription?.remove();
  },
  subscribeResponse(listener: (response: NotificationResponse) => void): () => void {
    const subscription=getRemoteNotifications()?.addNotificationResponseReceivedListener(listener);
    return () => subscription?.remove();
  },
  /**
   * Lấy hoặc tạo Installation ID duy nhất của thiết bị
   */
  getInstallationId,

  /**
   * Khởi tạo Notification Channel cho Android (Android 8.0+)
   * Cần gọi trước khi xin quyền hoặc hiển thị notification
   */
  async setupAndroidChannel(): Promise<void> {
    const Notifications=getRemoteNotifications();
    if (!Notifications) return;
    if (Platform.OS === 'android') {
      try {
        await Notifications.setNotificationChannelAsync(HRMS_NOTIFICATION_CHANNEL_ID, {
          name: 'HRMS Notifications',
          importance: Notifications.AndroidImportance.MAX,
          enableVibrate: true,
          vibrationPattern: [0, 250, 250, 250],
          lightColor: '#1E40AF',
          sound: 'default',
          showBadge: true,
        });
      } catch (err) {
        console.warn('[setupAndroidChannel Error]:', err);
      }
    }
  },

  /**
   * Kiểm tra xem đã từng hiện lời mời xin quyền Remote Push chưa
   */
  async hasPromptedForPermission(): Promise<boolean> {
    try {
      const val = await AsyncStorage.getItem(ASYNC_KEY_PERMISSION_PROMPTED);
      return val === 'true';
    } catch {
      return false;
    }
  },

  /**
   * Đánh dấu đã hiện lời mời xin quyền Remote Push
   */
  async setPromptedForPermission(): Promise<void> {
    try {
      await AsyncStorage.setItem(ASYNC_KEY_PERMISSION_PROMPTED, 'true');
    } catch {}
  },

  /**
   * Lấy trạng thái quyền hệ điều hành hiện tại
   */
  async getPermissionStatus(): Promise<{ status: 'GRANTED' | 'DENIED' | 'UNDETERMINED'; canAskAgain: boolean }> {
    const Notifications=getRemoteNotifications();
    if (!Notifications) return {status:'UNDETERMINED',canAskAgain:false};
    try {
      const settings = await Notifications.getPermissionsAsync();
      const granted = settings.granted || [Notifications.IosAuthorizationStatus.AUTHORIZED, Notifications.IosAuthorizationStatus.PROVISIONAL, Notifications.IosAuthorizationStatus.EPHEMERAL].includes(settings.ios?.status as number);
      const denied = !granted && settings.status === Notifications.PermissionStatus.DENIED;

      return {
        status: granted ? 'GRANTED' : denied ? 'DENIED' : 'UNDETERMINED',
        canAskAgain: settings.canAskAgain,
      };
    } catch {
      return { status: 'UNDETERMINED', canAskAgain: true };
    }
  },

  /**
   * Yêu cầu quyền hệ điều hành (chỉ gọi khi người dùng bấm "Bật thông báo" trên modal hoặc Settings)
   */
  async requestPermission(): Promise<{ status: 'GRANTED' | 'DENIED' | 'UNDETERMINED'; canAskAgain: boolean }> {
    const Notifications=getRemoteNotifications();
    if (!Notifications) return {status:'UNDETERMINED',canAskAgain:false};
    await this.setupAndroidChannel();

    try {
      const settings = await Notifications.requestPermissionsAsync({
        ios: {
          allowAlert: true,
          allowBadge: true,
          allowSound: true,
        },
      });

      const granted = settings.granted || [Notifications.IosAuthorizationStatus.AUTHORIZED, Notifications.IosAuthorizationStatus.PROVISIONAL, Notifications.IosAuthorizationStatus.EPHEMERAL].includes(settings.ios?.status as number);
      const denied = !granted && settings.status === Notifications.PermissionStatus.DENIED;

      return {
        status: granted ? 'GRANTED' : denied ? 'DENIED' : 'UNDETERMINED',
        canAskAgain: settings.canAskAgain,
      };
    } catch (err) {
      console.warn('[requestPermission Error]:', err);
      return { status: 'DENIED', canAskAgain: false };
    }
  },

  /**
   * Lấy Expo Push Token theo chuẩn Expo SDK 57
   */
  async getExpoPushToken(): Promise<string | null> {
    tokenError=null;
    const Notifications=getRemoteNotifications();
    if (!Notifications) return null;
    if (!Device.isDevice) {
      console.warn('[getExpoPushToken]: Đang chạy trên thiết bị giả lập, không hỗ trợ nhận Remote Push Token.');
      return null;
    }

    try {
      const projectId = Constants?.expoConfig?.extra?.eas?.projectId ?? Constants?.easConfig?.projectId;
      if (!projectId) { tokenError=i18n.t('mobileReview.pushNotConfigured'); return null; }
      const tokenResult = await Notifications.getExpoPushTokenAsync({projectId});
      return tokenResult.data;
    } catch (err: any) {
      console.warn('[getExpoPushToken Warning]: Không lấy được Expo Push Token:', err?.message || err);
      tokenError=i18n.t('mobileReview.pushTokenFailed');
      return null;
    }
  },

  /**
   * Đăng ký thiết bị và Token lên máy chủ HRMS API
   */
  async syncDeviceRegistration(): Promise<DeviceStatusInfo> {
    if (!registrationAllowed) return {
      isRegistered:false, pushToken:null, permissionStatus:'UNDETERMINED',
      canAskAgain:false, pushEnabled:false, isPhysicalDevice:Device.isDevice, errorMessage:null,
    };
    const operation=this.registerDevice();
    registrations.add(operation);
    try { return await operation; }
    finally { registrations.delete(operation); }
  },

  async registerDevice(): Promise<DeviceStatusInfo> {
    tokenError=null;
    const isPhysical = Device.isDevice;
    const { status: permStatus, canAskAgain } = await this.getPermissionStatus();
    await this.setupAndroidChannel();
    const installationId = await this.getInstallationId();

    let pushToken: string | null = null;
    let syncError: string | null = null;

    if (this.isRemotePushConfigured() && permStatus === 'GRANTED' && isPhysical) {
      pushToken = await this.getExpoPushToken();
    }

    // Đọc trạng thái toggle push cá nhân từ local storage (mặc định true)
    let pushEnabled = this.isRemotePushConfigured();
    try {
      const savedEn = await AsyncStorage.getItem(ASYNC_KEY_PUSH_ENABLED);
      if (savedEn !== null) pushEnabled = this.isRemotePushConfigured() && savedEn === 'true';
    } catch {}

    const projectId = Constants?.expoConfig?.extra?.eas?.projectId ?? Constants?.easConfig?.projectId ?? null;
    const deviceName = Device.modelName || Device.deviceName || `${Platform.OS.toUpperCase()} Device`;
    const appVersion = Constants?.expoConfig?.version || '1.0.0';
    const lang = i18n.language || 'vi';

    const dto: RegisterDeviceDto = {
      installationId,
      expoPushToken: pushToken,
      platform: Platform.OS.toUpperCase(),
      projectId,
      environment: __DEV__ ? 'DEVELOPMENT' : 'PRODUCTION',
      language: lang,
      appVersion,
      permissionStatus: permStatus,
      pushEnabled,
      deviceName,
    };

    try {
      await meApi.registerDevice(dto);
      return {
        isRegistered: !!pushToken && permStatus === 'GRANTED' && pushEnabled,
        pushToken,
        permissionStatus: permStatus,
        canAskAgain,
        pushEnabled,
        isPhysicalDevice: isPhysical,
        errorMessage: tokenError || (!this.isRemotePushConfigured() ? i18n.t('mobileReview.pushNotConfigured') : null),
      };
    } catch (err: any) {
      syncError = err?.response?.data?.message || err?.message || 'Không thể đồng bộ với máy chủ.';
      return {
        isRegistered: false,
        pushToken,
        permissionStatus: permStatus,
        canAskAgain,
        pushEnabled,
        isPhysicalDevice: isPhysical,
        errorMessage: syncError,
      };
    }
  },

  /**
   * Cập nhật tùy chọn Bật/Tắt push trong HRMS
   */
  async setPushEnabled(enabled: boolean): Promise<boolean> {
    const installationId = await this.getInstallationId();
    try {
      await meApi.updateDevicePreferences(installationId, { pushEnabled: enabled });
      await AsyncStorage.setItem(ASYNC_KEY_PUSH_ENABLED, enabled ? 'true' : 'false');
      return true;
    } catch (err) {
      console.warn('[setPushEnabled Error]:', err);
      return false;
    }
  },

  /**
   * Vô hiệu hóa thiết bị khi Đăng xuất để chống rò rỉ thông báo
   */
  async deactivateOnLogout(): Promise<void> {
    registrationAllowed=false;
    await Promise.allSettled([...registrations]);
    try {
      const installationId = await this.getInstallationId();
      await meApi.deactivateDevice(installationId);
    } catch (err) {
      console.warn('[deactivateOnLogout Warning]:', err);
    }
  },

  /**
   * Mở Cài đặt hệ điều hành khi bị Denied vĩnh viễn
   */
  async openAppSettings(): Promise<void> {
    if (Platform.OS === 'android') {
      try {
        await Linking.sendIntent('android.settings.CHANNEL_NOTIFICATION_SETTINGS', [
          {key: 'android.provider.extra.APP_PACKAGE', value: Constants.expoConfig?.android?.package ?? 'com.qlns.hrmsmobile'},
          {key: 'android.provider.extra.CHANNEL_ID', value: HRMS_NOTIFICATION_CHANNEL_ID},
        ]);
        return;
      } catch {
        // Some Android versions/OEMs do not expose the channel settings intent.
      }
    }
    await Linking.openSettings();
  },

  /**
   * Điều hướng thông minh khi người dùng bấm vào một thông báo Push hoặc In-App
   */
  handleNotificationNavigation(navigation: any, responseData: any): void {
    if (!responseData) return;

    const destination=notificationDestination(responseData);
    navigation.navigate(destination.screen, destination.params);

  },

  /**
   * Kiểm tra thông báo cold-start (khi app được mở từ trạng thái tắt hoàn toàn qua notification)
   */
  async checkLastNotificationResponse(navigation: any): Promise<void> {
    const Notifications=getRemoteNotifications();
    if (!Notifications) return;
    try {
      const lastResponse = Notifications.getLastNotificationResponse();
      if (lastResponse) {
        const data = lastResponse.notification?.request?.content?.data;
        if (data) {
          this.handleNotificationNavigation(navigation, data);
          Notifications.clearLastNotificationResponse();
        }
      }
    } catch (err) {
      console.warn('[checkLastNotificationResponse Error]:', err);
    }
  },
};
