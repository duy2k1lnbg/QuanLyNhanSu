import {notificationText} from '../../utils/notificationText';
import { useFocusEffect } from '@react-navigation/native';
import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  Alert,
  TouchableOpacity,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { RootStackParamList } from '../../navigation/types';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { DashboardDto, NotificationDto } from '../../types/me';
import { AppAvatar } from '../../components/AppAvatar';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppErrorState } from '../../components/AppErrorState';
import { NotificationPermissionModal } from '../../components/NotificationPermissionModal';
import { notificationService } from '../../services/notificationService';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { getLocalizedPayrollStatus } from '../../utils/statusMapper';
import { spacing } from '../../constants/spacing';
import { canMobile, canViewRequests } from '../../utils/mobilePermissions';
import { pendingRequestCount } from '../../utils/requestContracts';
import {subscribeNotificationChanges} from '../../utils/notificationEvents';
import { typography } from '../../constants/typography';

export const HomeScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const insets = useSafeAreaInsets();
  const { user } = useAuth();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [dashboard, setDashboard] = useState<DashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [pendingRequestsCount, setPendingRequestsCount] = useState<number>(0);
  const [requestsUnavailable,setRequestsUnavailable]=useState(false);
  const [permissionModalVisible, setPermissionModalVisible] = useState(false);

  const fetchDashboard = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const [dashData, reqData] = await Promise.allSettled([
        meApi.getDashboard(),
        meApi.getAllRequests(),
      ]);

      if (dashData.status === 'rejected') throw dashData.reason;
      if (dashData.status === 'fulfilled') {
        setDashboard(dashData.value);
      }
      setRequestsUnavailable(reqData.status === 'rejected');
      if (reqData.status === 'fulfilled' && reqData.value) {
        setPendingRequestsCount(pendingRequestCount(reqData.value));
      }
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useFocusEffect(useCallback(() => {
    fetchDashboard();
    return subscribeNotificationChanges(() => fetchDashboard(true));
  }, [fetchDashboard]));

  // Kiểm tra quyền thông báo sau khi đăng nhập và thiết lập listeners
  useEffect(() => {
    let isMounted = true;

    const checkPermissionOnReady = async () => {
      // 1. Kiểm tra cold-start notification click
      await notificationService.checkLastNotificationResponse(navigation);

      if (!notificationService.isRemotePushConfigured()) return;

      // 2. Kiểm tra xem đã từng hỏi người dùng chưa
      const hasPrompted = await notificationService.hasPromptedForPermission();
      const { status } = await notificationService.getPermissionStatus();

      if (!hasPrompted && status === 'UNDETERMINED') {
        // Hiện modal mời bật thông báo lần đầu (non-blocking)
        if (isMounted) {
          setPermissionModalVisible(true);
        }
      } else if (status === 'GRANTED') {
        // Đã cấp quyền -> đồng bộ đăng ký thiết bị ngầm
        await notificationService.syncDeviceRegistration();
      }
    };

    checkPermissionOnReady().catch(error=>console.warn('[Notifications] Background registration failed:',error?.message));

    // 4. Listener khi người dùng click vào notification banner -> điều hướng
    const responseSub = notificationService.subscribeResponse((response) => {
      const data = response.notification?.request?.content?.data;
      if (data) {
        notificationService.handleNotificationNavigation(navigation, data);
      }
    });

    return () => {
      isMounted = false;
      responseSub();
    };
  }, [navigation, fetchDashboard]);

  const handleAcceptPermission = async () => {
    setPermissionModalVisible(false);
    await notificationService.setPromptedForPermission();
    try {
      await notificationService.requestPermission();
      const status=await notificationService.syncDeviceRegistration();
      if (status.errorMessage) Alert.alert(t('common.error'), status.errorMessage.startsWith('mobileReview.') ? t(status.errorMessage) : t('errors.serverError'));
    } catch(error) { Alert.alert(t('common.error'),mapApiError(error)); }
  };

  const handleDismissPermission = async () => {
    setPermissionModalVisible(false);
    await notificationService.setPromptedForPermission();
  };

  if (loading && !refreshing) {
    return <AppLoading />;
  }

  if (errorMessage && !dashboard) {
    return <AppErrorState message={errorMessage} onRetry={() => fetchDashboard()} />;
  }

  const profile = dashboard?.profileSummary || (dashboard as any)?.ProfileSummary;
  const attendance = dashboard?.attendanceSummary || (dashboard as any)?.AttendanceSummary;
  const payroll = dashboard?.payrollSummary || (dashboard as any)?.PayrollSummary;
  const notifications = dashboard?.recentNotifications || (dashboard as any)?.RecentNotifications || [];

  return (
    <View style={{ flex: 1, backgroundColor: colors.background }}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={[
          styles.content,
          { paddingTop: Math.max(insets.top, spacing.md) },
        ]}
      refreshControl={
        <RefreshControl
          refreshing={refreshing}
          onRefresh={() => fetchDashboard(true)}
          colors={[colors.primary]}
          tintColor={colors.primary}
        />
      }
    >
      {/* 1. Header Profile Banner */}
      <View style={[styles.headerBanner, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
        <View style={styles.headerLeft}>
          <AppAvatar
            name={profile?.hoten || user?.fullName}
            avatarBase64={profile?.avatarBase64}
            size="lg"
          />
          <View style={styles.headerMeta}>
            <Text style={[typography.caption, { color: colors.textSecondary }]}>
              {t('home.greeting')} 👋
            </Text>
            <Text style={[typography.h3, { color: colors.text }]} numberOfLines={1}>
              {profile?.hoten || user?.fullName || '---'}
            </Text>
            <View style={styles.tagsRow}>
              <AppBadge label={`${t('home.employeeId')}: ${profile?.employeeCode || profile?.manv || user?.manv || '---'}`} />
              {profile?.tenChucVu ? (
                <AppBadge label={profile.tenChucVu} variant="info" />
              ) : null}
            </View>
            {profile?.tenPhongBan ? (
              <Text style={[typography.caption, { color: colors.textMuted, marginTop: 2 }]}>
                {profile.tenPhongBan}
              </Text>
            ) : null}
          </View>
        </View>
      </View>

      {/* 2. Contract Warning Alert (if any) */}
      {dashboard?.hasExpiringContract && (
        <TouchableOpacity
          activeOpacity={0.8}
          onPress={() => navigation.navigate('Contract')}
          style={[styles.alertCard, { backgroundColor: 'rgba(245, 158, 11, 0.12)', borderColor: colors.warning }]}
        >
          <Ionicons name="warning-outline" size={24} color={colors.warning} />
          <View style={styles.alertTextWrapper}>
            <Text style={[typography.bodyBold, { color: colors.warning }]}>
              {t('home.contractAlertTitle')}
            </Text>
            <Text style={[typography.caption, { color: colors.text }]}>
              {dashboard.expiringContractInfo || t('contract.expiringWarning')}
            </Text>
          </View>
          <Ionicons name="chevron-forward" size={20} color={colors.warning} />
        </TouchableOpacity>
      )}

      {/* 3. Summary Cards Row */}
      <View style={styles.cardsRow}>
        {/* Attendance Summary */}
        {canMobile(user,'MOBILE_ATTENDANCE_VIEW') && (
        <AppCard
          style={styles.summaryCard}
          onPress={() => navigation.navigate('Main', { screen: 'AttendanceTab' } as any)}
        >
          <View style={styles.cardHeaderRow}>
            <View style={[styles.iconBox, { backgroundColor: 'rgba(59, 130, 246, 0.1)' }]}>
              <Ionicons name="calendar-outline" size={20} color={colors.primary} />
            </View>
            <AppBadge label={attendance ? `${attendance.thang}/${attendance.nam}` : '---'} />
          </View>
          <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.md }]}>
            {t('home.monthlyAttendance')}
          </Text>
          <Text style={[typography.h2, { color: colors.text, marginTop: spacing.xs }]} numberOfLines={1} adjustsFontSizeToFit>
            {attendance ? attendance.tongNgayCong : 0}{' '}
            <Text style={[typography.caption, { color: colors.textSecondary }]}>
              {t('home.attendanceUnit')}
            </Text>
          </Text>
          <Text style={[typography.caption, { color: colors.textMuted, marginTop: spacing.xs }]} numberOfLines={1}>
            {t('attendance.lateCount')}: {attendance ? attendance.soLanDiMuon : 0}
          </Text>
        </AppCard>
        )}

        {/* Payroll Summary */}
        {canMobile(user,'MOBILE_PAYROLL_VIEW') && (
        <AppCard
          style={styles.summaryCard}
          onPress={() => navigation.navigate('Main', { screen: 'PayrollTab' } as any)}
        >
          <View style={styles.cardHeaderRow}>
            <View style={[styles.iconBox, { backgroundColor: 'rgba(16, 185, 129, 0.1)' }]}>
              <Ionicons name="cash-outline" size={20} color={colors.success} />
            </View>
            <AppBadge
              label={payroll ? `${payroll.thang}/${payroll.nam}` : '---'}
              variant="success"
            />
          </View>
          <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.md }]} numberOfLines={1}>
            {t('home.recentPayroll')}
          </Text>
          <Text style={[typography.h3, { color: colors.success, marginTop: spacing.xs }]} numberOfLines={1} adjustsFontSizeToFit>
            {payroll ? formatCurrency(payroll.thucLinh, activeLanguage) : '---'}
          </Text>
          <Text style={[typography.caption, { color: colors.textMuted, marginTop: spacing.xs }]} numberOfLines={1}>
            {payroll ? getLocalizedPayrollStatus(payroll.trangThaiChiTra) : t('payroll.statusPending')}
          </Text>
        </AppCard>
        )}
      </View>

      {/* 4. Pending Requests Status Banner (Rule 24) */}
      {canViewRequests(user) && (      <TouchableOpacity
        activeOpacity={0.8}
        onPress={() => navigation.navigate('MyRequests')}
        style={[
          styles.alertCard,
          {
            backgroundColor: pendingRequestsCount > 0 ? 'rgba(59, 130, 246, 0.08)' : 'rgba(16, 185, 129, 0.08)',
            borderColor: pendingRequestsCount > 0 ? colors.primary : colors.success,
            marginTop: spacing.md,
          },
        ]}
      >
        <Ionicons
          name={pendingRequestsCount > 0 ? 'hourglass-outline' : 'checkmark-circle-outline'}
          size={24}
          color={pendingRequestsCount > 0 ? colors.primary : colors.success}
        />
        <View style={styles.alertTextWrapper}>
          <Text style={[typography.bodyBold, { color: pendingRequestsCount > 0 ? colors.primary : colors.success }]}>
            {t('home.pendingRequests')}
          </Text>
          <Text style={[typography.caption, { color: colors.text }]}>
            {requestsUnavailable ? t('mobileReview.requestsUnavailable'):pendingRequestsCount > 0
              ? t('mobileReview.pendingCount',{count:pendingRequestsCount})
              : t('home.noPendingRequests')}
          </Text>
        </View>
        <Ionicons name="chevron-forward" size={20} color={colors.textSecondary} />
      </TouchableOpacity>)}


      {/* 5. Quick Actions Grid (Rule 24 & 25: Chấm công, Xin nghỉ, Xem lương, Điều chỉnh công, Tăng ca, Yêu cầu) */}
      <View style={styles.sectionHeaderRow}>
        <Text style={[typography.h3, { color: colors.text }]}>
          {t('home.quickActions')}
        </Text>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.navigate('MyRequests')}
        >
          <Text style={[typography.captionBold, { color: colors.primary }]}>
            {t('requests.allRequests')}
          </Text>
        </TouchableOpacity>
      </View>

      <View style={styles.quickGrid}>
        {[
          {code:'MOBILE_ATTENDANCE_VIEW',label:'nav.attendance',icon:'time-outline',route:'Main',params:{screen:'AttendanceTab'}},
          {code:'MOBILE_REQUEST_LEAVE',label:'nav.leaveRequest',icon:'calendar-outline',route:'LeaveRequest'},
          {code:'MOBILE_PAYROLL_VIEW',label:'nav.payroll',icon:'wallet-outline',route:'Main',params:{screen:'PayrollTab'}},
          {code:'MOBILE_REQUEST_CORRECTION',label:'nav.attendanceCorrection',icon:'build-outline',route:'AttendanceCorrection'},
          {code:'MOBILE_REQUEST_OVERTIME',label:'nav.overtime',icon:'flash-outline',route:'MyOvertime'},
          {code:'MOBILE_REQUEST_ADVANCE',label:'requests.advanceTitle',icon:'cash-outline',route:'SalaryAdvance'},
          {code:'MOBILE_REQUEST_DEPENDENT',label:'nav.dependents',icon:'people-outline',route:'Dependents'},
          {code:'MOBILE_INSURANCE_VIEW',label:'nav.insurance',icon:'shield-checkmark-outline',route:'Insurance'},
          {code:'MOBILE_CONTRACT_VIEW',label:'nav.contract',icon:'document-text-outline',route:'Contract'},
          {code:'REQUESTS',label:'requests.title',icon:'file-tray-outline',route:'MyRequests'},
        ].filter(item=>item.code==='REQUESTS' ? canViewRequests(user) : canMobile(user,item.code)).map(item=>(
          <TouchableOpacity key={item.code} style={[styles.quickItem,{backgroundColor:colors.surfaceCard,borderColor:colors.border}]} onPress={()=>navigation.navigate(item.route as any,item.params as any)}>
            <Ionicons name={item.icon as any} size={24} color={colors.primary}/>
            <Text style={[typography.captionBold,{color:colors.text,marginTop:spacing.sm,textAlign:'center'}]}>{t(item.label)}</Text>
          </TouchableOpacity>
        ))}
        <TouchableOpacity style={[styles.quickItem,{backgroundColor:colors.surfaceCard,borderColor:colors.border}]} onPress={()=>navigation.navigate('Settings')}>
          <Ionicons name="settings-outline" size={24} color={colors.primary}/><Text style={[typography.captionBold,{color:colors.text,marginTop:spacing.sm}]}>{t('nav.settings')}</Text>
        </TouchableOpacity>
      </View>

      {/* Secondary Navigation Row: Hợp đồng lao động */}
      {canMobile(user,'MOBILE_CONTRACT_VIEW') && <View style={{ marginTop: spacing.md }}>
        <TouchableOpacity
          style={[styles.secondaryCard, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Contract')}
        >
          <Ionicons name="document-text-outline" size={22} color={colors.primary} />
          <View style={{ marginLeft: spacing.sm, flex: 1 }}>
            <Text style={[typography.bodyBold, { color: colors.text }]}>
              {t('nav.contract')}
            </Text>
            <Text style={[typography.caption, { color: colors.textSecondary }]}>
              {dashboard?.expiringContractInfo || t('mobileReview.contractHint')}
            </Text>
          </View>
          <Ionicons name="chevron-forward" size={18} color={colors.textMuted} />
        </TouchableOpacity>
      </View>}

      {/* 5. Recent Announcements */}
      {canMobile(user,'MOBILE_NOTIFICATION_VIEW') && <>
      <View style={styles.sectionHeaderRow}>
        <Text style={[typography.h3, { color: colors.text }]}>
          {t('home.recentNotifications')}
        </Text>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.navigate('Main', { screen: 'NotificationsTab' } as any)}
        >
          <Text style={[typography.captionBold, { color: colors.primary }]}>
            {t('home.viewAll')}
          </Text>
        </TouchableOpacity>
      </View>

      {notifications.length === 0 ? (
        <AppCard style={{ alignItems: 'center', padding: spacing.xl }}>
          <Text style={[typography.body, { color: colors.textSecondary }]}>
            {t('home.noNotifications')}
          </Text>
        </AppCard>
      ) : (
        notifications.slice(0, 3).map((item: NotificationDto) => (
          <AppCard
            key={item.id}
            style={styles.notifCard}
            onPress={() =>
              navigation.navigate('NotificationDetail', {
                notificationId: item.id,
                initialData: item,
              })
            }
          >
            <View style={styles.notifRow}>
              <View style={[styles.notifIcon, { backgroundColor: item.isPinned ? 'rgba(245, 158, 11, 0.15)' : 'rgba(59, 130, 246, 0.1)' }]}>
                <Ionicons
                  name={item.isPinned ? 'pin' : 'newspaper-outline'}
                  size={18}
                  color={item.isPinned ? colors.warning : colors.primary}
                />
              </View>
              <View style={styles.notifContent}>
                <View style={styles.notifTitleRow}>
                  <Text style={[typography.bodyBold, { color: colors.text, flex: 1 }]} numberOfLines={1}>
                    {notificationText(item,t).tieude}
                  </Text>
                  {item.isUnread && <View style={[styles.unreadDot, { backgroundColor: colors.danger }]} />}
                </View>
                <Text style={[typography.caption, { color: colors.textSecondary, marginTop: 2 }]} numberOfLines={2}>
                  {notificationText(item,t).noidung}
                </Text>
                <Text style={[typography.caption, { color: colors.textMuted, marginTop: 4 }]}>
                  {formatDate(item.ngaydang, activeLanguage)} • {item.nguoidang || 'HR'}
                </Text>
              </View>
            </View>
          </AppCard>
        ))
      )}
      </>}
      </ScrollView>
      <NotificationPermissionModal
        visible={permissionModalVisible}
        onAccept={handleAcceptPermission}
        onDismiss={handleDismissPermission}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    padding: spacing.lg,
    paddingBottom: 40,
  },
  headerBanner: {
    padding: spacing.lg,
    borderRadius: spacing.borderRadiusLg,
    borderWidth: 1,
    marginBottom: spacing.md,
  },
  headerLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  headerMeta: {
    flex: 1,
  },
  tagsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.xs,
    marginTop: spacing.xs,
  },
  alertCard: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: spacing.md,
    borderRadius: spacing.borderRadiusMd,
    borderWidth: 1,
    marginBottom: spacing.md,
    gap: spacing.md,
  },
  alertTextWrapper: {
    flex: 1,
  },
  cardsRow: {
    flexDirection: 'row',
    gap: spacing.md,
  },
  summaryCard: {
    flex: 1,
    minWidth: 0,
    overflow: 'hidden',
  },
  cardHeaderRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  iconBox: {
    width: 36,
    height: 36,
    borderRadius: 18,
    alignItems: 'center',
    justifyContent: 'center',
  },
  quickGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
    rowGap: spacing.md,
  },
  quickItem: {
    width: '31%',
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: spacing.md,
    paddingHorizontal: 2,
    borderRadius: spacing.borderRadiusMd,
    borderWidth: 1,
  },
  quickIconCircle: {
    width: 44,
    height: 44,
    borderRadius: 22,
    alignItems: 'center',
    justifyContent: 'center',
  },
  sectionHeaderRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: spacing.xl,
    marginBottom: spacing.md,
  },
  notifCard: {
    marginBottom: spacing.sm,
    padding: spacing.md,
  },
  notifRow: {
    flexDirection: 'row',
    gap: spacing.md,
  },
  notifIcon: {
    width: 36,
    height: 36,
    borderRadius: 18,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 2,
  },
  notifContent: {
    flex: 1,
  },
  notifTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  unreadDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    marginLeft: spacing.xs,
  },
  secondaryRow: {
    flexDirection: 'row',
    gap: spacing.md,
    marginTop: spacing.md,
  },
  secondaryCard: {
    width: '100%',
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.md,
    borderRadius: spacing.borderRadiusMd,
    borderWidth: 1,
  },
});
