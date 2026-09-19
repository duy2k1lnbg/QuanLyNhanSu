import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
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
import { formatCurrency, formatDate } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
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

  const fetchDashboard = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const data = await meApi.getDashboard();
      setDashboard(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

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
    <ScrollView
      style={[styles.container, { backgroundColor: colors.background }]}
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
              <AppBadge label={`${t('home.employeeId')}: ${profile?.manv || user?.manv || '---'}`} />
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

        {/* Payroll Summary */}
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
            {payroll?.trangThaiChiTra || t('payroll.statusPending')}
          </Text>
        </AppCard>
      </View>

      {/* 4. Quick Actions Grid */}
      <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.md }]}>
        {t('home.quickActions')}
      </Text>
      <View style={styles.quickGrid}>
        <TouchableOpacity
          style={[styles.quickItem, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Main', { screen: 'ProfileTab' } as any)}
        >
          <View style={[styles.quickIconCircle, { backgroundColor: 'rgba(30, 64, 175, 0.1)' }]}>
            <Ionicons name="person-outline" size={22} color={colors.primary} />
          </View>
          <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs }]}>
            {t('nav.profile')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickItem, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Main', { screen: 'AttendanceTab' } as any)}
        >
          <View style={[styles.quickIconCircle, { backgroundColor: 'rgba(59, 130, 246, 0.1)' }]}>
            <Ionicons name="time-outline" size={22} color="#3B82F6" />
          </View>
          <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs }]}>
            {t('nav.attendance')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickItem, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Main', { screen: 'PayrollTab' } as any)}
        >
          <View style={[styles.quickIconCircle, { backgroundColor: 'rgba(16, 185, 129, 0.1)' }]}>
            <Ionicons name="wallet-outline" size={22} color="#10B981" />
          </View>
          <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs }]}>
            {t('nav.payroll')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickItem, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Contract')}
        >
          <View style={[styles.quickIconCircle, { backgroundColor: 'rgba(245, 158, 11, 0.1)' }]}>
            <Ionicons name="document-text-outline" size={22} color="#F59E0B" />
          </View>
          <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs }]}>
            {t('nav.contract')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickItem, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Insurance')}
        >
          <View style={[styles.quickIconCircle, { backgroundColor: 'rgba(14, 165, 233, 0.1)' }]}>
            <Ionicons name="medkit-outline" size={22} color="#0EA5E9" />
          </View>
          <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs }]}>
            {t('nav.insurance')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickItem, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}
          onPress={() => navigation.navigate('Main', { screen: 'NotificationsTab' } as any)}
        >
          <View style={[styles.quickIconCircle, { backgroundColor: 'rgba(239, 68, 68, 0.1)' }]}>
            <Ionicons name="notifications-outline" size={22} color="#EF4444" />
          </View>
          <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs }]}>
            {t('nav.notifications')}
          </Text>
        </TouchableOpacity>
      </View>

      {/* 5. Recent Announcements */}
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
                    {item.tieude}
                  </Text>
                  {item.isUnread && <View style={[styles.unreadDot, { backgroundColor: colors.danger }]} />}
                </View>
                <Text style={[typography.caption, { color: colors.textSecondary, marginTop: 2 }]} numberOfLines={2}>
                  {item.noidung}
                </Text>
                <Text style={[typography.caption, { color: colors.textMuted, marginTop: 4 }]}>
                  {formatDate(item.ngaydang, activeLanguage)} • {item.nguoidang || 'HR'}
                </Text>
              </View>
            </View>
          </AppCard>
        ))
      )}
    </ScrollView>
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
});
