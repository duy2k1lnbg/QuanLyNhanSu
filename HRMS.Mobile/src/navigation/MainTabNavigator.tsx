import React, {useState,useEffect,useCallback} from 'react';
import {MobileNotificationBootstrap} from '../components/MobileNotificationBootstrap';
import {AppState} from 'react-native';
import {notificationService} from '../services/notificationService';
import {useFocusEffect} from '@react-navigation/native';
import {meApi} from '../api/meApi';
import {subscribeNotificationChanges,notifyNotificationsChanged} from '../utils/notificationEvents';
import {startForegroundRefresh} from '../utils/foregroundRefresh';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { MainTabParamList } from './types';
import { HomeScreen } from '../screens/Home/HomeScreen';
import { AttendanceScreen } from '../screens/Attendance/AttendanceScreen';
import { PayrollScreen } from '../screens/Payroll/PayrollScreen';
import { NotificationListScreen } from '../screens/Notifications/NotificationListScreen';
import { ProfileScreen } from '../screens/Profile/ProfileScreen';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';
import { useAuth } from '../hooks/useAuth';
import { canMobile } from '../utils/mobilePermissions';
import { withMobilePermission } from '../components/MobilePermissionGate';

const GuardedAttendance=withMobilePermission(AttendanceScreen, 'MOBILE_ATTENDANCE_VIEW');
const GuardedPayroll=withMobilePermission(PayrollScreen, 'MOBILE_PAYROLL_VIEW');
const GuardedNotifications=withMobilePermission(NotificationListScreen, 'MOBILE_NOTIFICATION_VIEW');
const GuardedProfile=withMobilePermission(ProfileScreen, 'MOBILE_PROFILE_VIEW');

const Tab = createBottomTabNavigator<MainTabParamList>();

export const MainTabNavigator: React.FC = () => {
  const { t } = useTranslation();
  const { user }=useAuth();
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const [unread,setUnread]=useState(0);
  const notificationAllowed=canMobile(user,'MOBILE_NOTIFICATION_VIEW');
  const refreshUnread=useCallback(()=>{
    if (!notificationAllowed) { setUnread(0); return; }
    meApi.getUnreadNotificationCount().then(setUnread).catch(()=>{});
  },[notificationAllowed]);
  useFocusEffect(useCallback(()=>{refreshUnread();},[refreshUnread]));
  useEffect(()=>{
    if (!notificationAllowed) return;
    const changed=subscribeNotificationChanges(refreshUnread);
    const received=notificationService.subscribeReceived(notifyNotificationsChanged);
    const stop=startForegroundRefresh(AppState,notifyNotificationsChanged);
    return ()=>{changed();received();stop();};
  },[refreshUnread,notificationAllowed]);


  const tabHeight = 58 + Math.max(insets.bottom, 6);

  return (
    <>
    <MobileNotificationBootstrap/>
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarStyle: {
          backgroundColor: colors.surfaceCard,
          borderTopColor: colors.border,
          height: tabHeight,
          paddingBottom: Math.max(insets.bottom, 6),
          paddingTop: 6,
        },
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textSecondary,
        tabBarLabelStyle: {
          fontSize: 11,
          fontWeight: '600',
        },
      }}
    >
      <Tab.Screen
        name="HomeTab"
        component={HomeScreen}
        options={{
          tabBarLabel: t('nav.home'),
          tabBarIcon: ({ color, size, focused }) => (
            <Ionicons name={focused ? 'home' : 'home-outline'} size={size} color={color} />
          ),
        }}
      />
      {canMobile(user,'MOBILE_ATTENDANCE_VIEW') && (
      <Tab.Screen
        name="AttendanceTab"
        component={GuardedAttendance}
        options={{
          tabBarLabel: t('nav.attendance'),
          tabBarIcon: ({ color, size, focused }) => (
            <Ionicons name={focused ? 'time' : 'time-outline'} size={size} color={color} />
          ),
        }}
      />
      )}
      {canMobile(user,'MOBILE_PAYROLL_VIEW') && (
      <Tab.Screen
        name="PayrollTab"
        component={GuardedPayroll}
        options={{
          tabBarLabel: t('nav.payroll'),
          tabBarIcon: ({ color, size, focused }) => (
            <Ionicons name={focused ? 'wallet' : 'wallet-outline'} size={size} color={color} />
          ),
        }}
      />
      )}
      {canMobile(user,'MOBILE_NOTIFICATION_VIEW') && (
      <Tab.Screen
        name="NotificationsTab"
        component={GuardedNotifications}
        options={{
          tabBarLabel: t('nav.notifications'),
          tabBarBadge: unread>0 ? (unread>99 ? '99+':unread):undefined,
          tabBarIcon: ({ color, size, focused }) => (
            <Ionicons name={focused ? 'notifications' : 'notifications-outline'} size={size} color={color} />
          ),
        }}
      />
      )}
      {canMobile(user,'MOBILE_PROFILE_VIEW') && (
      <Tab.Screen
        name="ProfileTab"
        component={GuardedProfile}
        options={{
          tabBarLabel: t('nav.profile'),
          tabBarIcon: ({ color, size, focused }) => (
            <Ionicons name={focused ? 'person' : 'person-outline'} size={size} color={color} />
          ),
        }}
      />
      )}
    </Tab.Navigator>
    </>
  );
};
