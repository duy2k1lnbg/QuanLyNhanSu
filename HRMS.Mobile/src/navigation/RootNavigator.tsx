import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { RootStackParamList } from './types';
import { SplashScreen } from '../screens/Splash/SplashScreen';
import { LanguageSelectScreen } from '../screens/Language/LanguageSelectScreen';
import { LoginScreen } from '../screens/Login/LoginScreen';
import { MainTabNavigator } from './MainTabNavigator';
import { ContractScreen } from '../screens/Contract/ContractScreen';
import { InsuranceScreen } from '../screens/Insurance/InsuranceScreen';
import { SettingsScreen } from '../screens/Settings/SettingsScreen';
import { LanguageSettingsScreen } from '../screens/Settings/LanguageSettingsScreen';
import { ThemeSettingsScreen } from '../screens/Settings/ThemeSettingsScreen';
import { ChangePasswordScreen } from '../screens/Settings/ChangePasswordScreen';
import { NotificationDetailScreen } from '../screens/Notifications/NotificationDetailScreen';
import { LeaveRequestScreen } from '../screens/Requests/LeaveRequestScreen';
import { AttendanceCorrectionScreen } from '../screens/Requests/AttendanceCorrectionScreen';
import { MyOvertimeScreen } from '../screens/Requests/MyOvertimeScreen';
import { MyRequestsScreen } from '../screens/Requests/MyRequestsScreen';

const Stack = createNativeStackNavigator<RootStackParamList>();

export const RootNavigator: React.FC = () => {
  return (
    <Stack.Navigator
      initialRouteName="Splash"
      screenOptions={{
        headerShown: false,
        animation: 'slide_from_right',
      }}
    >
      <Stack.Screen name="Splash" component={SplashScreen} />
      <Stack.Screen name="LanguageSelect" component={LanguageSelectScreen} />
      <Stack.Screen name="Login" component={LoginScreen} />
      <Stack.Screen name="Main" component={MainTabNavigator} />
      <Stack.Screen name="Contract" component={ContractScreen} />
      <Stack.Screen name="Insurance" component={InsuranceScreen} />
      <Stack.Screen name="Settings" component={SettingsScreen} />
      <Stack.Screen name="LanguageSettings" component={LanguageSettingsScreen} />
      <Stack.Screen name="ThemeSettings" component={ThemeSettingsScreen} />
      <Stack.Screen name="ChangePassword" component={ChangePasswordScreen} />
      <Stack.Screen name="NotificationDetail" component={NotificationDetailScreen} />
      <Stack.Screen name="LeaveRequest" component={LeaveRequestScreen} />
      <Stack.Screen name="AttendanceCorrection" component={AttendanceCorrectionScreen} />
      <Stack.Screen name="MyOvertime" component={MyOvertimeScreen} />
      <Stack.Screen name="MyRequests" component={MyRequestsScreen} />
    </Stack.Navigator>
  );
};
