import { NavigatorScreenParams } from '@react-navigation/native';
import { NotificationDto } from '../types/me';

export type MainTabParamList = {
  HomeTab: undefined;
  AttendanceTab: undefined;
  PayrollTab: undefined;
  NotificationsTab: undefined;
  ProfileTab: undefined;
};

export type RootStackParamList = {
  Splash: undefined;
  LanguageSelect: undefined;
  Login: undefined;
  Main: NavigatorScreenParams<MainTabParamList> | undefined;
  Contract: undefined;
  Insurance: undefined;
  Settings: undefined;
  LanguageSettings: undefined;
  ThemeSettings: undefined;
  ChangePassword: undefined;
  NotificationDetail: { notificationId: number; initialData?: NotificationDto };
};
