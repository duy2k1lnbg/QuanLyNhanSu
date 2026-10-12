import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { RootStackParamList } from './types';
import { SplashScreen } from '../screens/Splash/SplashScreen';
import { LanguageSelectScreen } from '../screens/Language/LanguageSelectScreen';
import { LoginScreen } from '../screens/Login/LoginScreen';
import { MainTabNavigator } from './MainTabNavigator';
import { ContractScreen } from '../screens/Contract/ContractScreen';
import { InsuranceScreen } from '../screens/Insurance/InsuranceScreen';
import { DependentsScreen } from '../screens/Dependents/DependentsScreen';
import { SettingsScreen } from '../screens/Settings/SettingsScreen';
import { LanguageSettingsScreen } from '../screens/Settings/LanguageSettingsScreen';
import { ThemeSettingsScreen } from '../screens/Settings/ThemeSettingsScreen';
import { ChangePasswordScreen } from '../screens/Settings/ChangePasswordScreen';
import { NotificationDetailScreen } from '../screens/Notifications/NotificationDetailScreen';
import { LeaveRequestScreen } from '../screens/Requests/LeaveRequestScreen';
import { AttendanceCorrectionScreen } from '../screens/Requests/AttendanceCorrectionScreen';
import { MyOvertimeScreen } from '../screens/Requests/MyOvertimeScreen';
import { SalaryAdvanceScreen } from '../screens/Requests/SalaryAdvanceScreen';
import { MyRequestsScreen } from '../screens/Requests/MyRequestsScreen';
import { useAuth } from '../hooks/useAuth';
import { withMobilePermission } from '../components/MobilePermissionGate';

const GuardedContract=withMobilePermission(ContractScreen, 'MOBILE_CONTRACT_VIEW');
const GuardedInsurance=withMobilePermission(InsuranceScreen, 'MOBILE_INSURANCE_VIEW');
const GuardedDependents=withMobilePermission(DependentsScreen, 'MOBILE_REQUEST_DEPENDENT');
const GuardedNotificationDetail=withMobilePermission(NotificationDetailScreen, 'MOBILE_NOTIFICATION_VIEW');
const GuardedLeaveRequest=withMobilePermission(LeaveRequestScreen, 'MOBILE_REQUEST_LEAVE');
const GuardedAttendanceCorrection=withMobilePermission(AttendanceCorrectionScreen, 'MOBILE_REQUEST_CORRECTION');
const GuardedMyOvertime=withMobilePermission(MyOvertimeScreen, 'MOBILE_REQUEST_OVERTIME');
const GuardedSalaryAdvance=withMobilePermission(SalaryAdvanceScreen, 'MOBILE_REQUEST_ADVANCE');
const GuardedMyRequests=withMobilePermission(MyRequestsScreen, 'REQUESTS');

const GuardedChangePassword=withMobilePermission(ChangePasswordScreen,'DOIMATKHAU','Edit');
const Stack = createNativeStackNavigator<RootStackParamList>();

export const RootNavigator: React.FC = () => {
  const { isAuthenticated }=useAuth();
  return (
    <Stack.Navigator
      initialRouteName="Splash"
      screenOptions={{
        headerShown: false,
        animation: 'slide_from_right',
      }}
    >
      <Stack.Screen name="Splash" component={SplashScreen} />
      {/* Language preferences are available before and after sign-in. */}
      <Stack.Screen name="LanguageSettings" navigationKey={isAuthenticated ? "signed-in" : "signed-out"} component={LanguageSettingsScreen} />
      {!isAuthenticated ? <>
      <Stack.Screen name="LanguageSelect" component={LanguageSelectScreen} />
      <Stack.Screen name="Login" component={LoginScreen} />
      </> : <>
      <Stack.Screen name="Main" component={MainTabNavigator} />
      <Stack.Screen name="Contract" component={GuardedContract} />
      <Stack.Screen name="Insurance" component={GuardedInsurance} />
      <Stack.Screen name="Dependents" component={GuardedDependents} />
      <Stack.Screen name="Settings" component={SettingsScreen} />
      <Stack.Screen name="ThemeSettings" component={ThemeSettingsScreen} />
      <Stack.Screen name="ChangePassword" component={GuardedChangePassword} />
      <Stack.Screen name="NotificationDetail" component={GuardedNotificationDetail} />
      <Stack.Screen name="LeaveRequest" component={GuardedLeaveRequest} />
      <Stack.Screen name="AttendanceCorrection" component={GuardedAttendanceCorrection} />
      <Stack.Screen name="MyOvertime" component={GuardedMyOvertime} />
      <Stack.Screen name="SalaryAdvance" component={GuardedSalaryAdvance} />
      <Stack.Screen name="MyRequests" component={GuardedMyRequests} />
      </>}
    </Stack.Navigator>
  );
};
