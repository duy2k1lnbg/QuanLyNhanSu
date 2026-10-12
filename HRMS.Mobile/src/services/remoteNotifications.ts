import {Platform} from 'react-native';
import Constants, {ExecutionEnvironment} from 'expo-constants';

type NotificationsModule = typeof import('expo-notifications');
let nativeNotifications: NotificationsModule | undefined;

export function canUseNativeNotifications(): boolean {
  const expoGo = Constants.executionEnvironment === ExecutionEnvironment.StoreClient
    || Constants.appOwnership === 'expo';
  return (Platform.OS === 'android' || Platform.OS === 'ios') && !expoGo;
}

export function canUseRemoteNotifications(): boolean {
  const projectId = Constants.expoConfig?.extra?.eas?.projectId ?? Constants.easConfig?.projectId;
  return canUseNativeNotifications() && !!projectId;
}

export function getRemoteNotifications(): NotificationsModule | null {
  // Importing the SDK itself attaches token listeners and crashes Android Expo Go.
  // A native build can request OS permission before its EAS project is linked.
  if (!canUseNativeNotifications()) return null;
  if (!nativeNotifications) {
    const notifications: NotificationsModule = require('expo-notifications');
    notifications.setNotificationHandler({
      handleNotification: async () => ({
        shouldShowBanner:true, shouldShowList:true,
        shouldPlaySound:true, shouldSetBadge:true,
      }),
    });
    nativeNotifications=notifications;
  }
  return nativeNotifications;
}
