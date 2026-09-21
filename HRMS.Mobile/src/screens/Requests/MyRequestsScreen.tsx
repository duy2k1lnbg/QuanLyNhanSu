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
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { UnifiedRequestsDto } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { formatDate } from '../../utils/formatters';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

type RequestTab = 'ALL' | 'LEAVE' | 'CORRECTION' | 'OVERTIME';

export const MyRequestsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [activeTab, setActiveTab] = useState<RequestTab>('ALL');
  const [data, setData] = useState<UnifiedRequestsDto>({ leaves: [], corrections: [], overtimes: [] });
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchAll = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    try {
      const res = await meApi.getAllRequests();
      setData(res);
    } catch {
      // ignore
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  const getStatusVariant = (status: string) => {
    switch (status?.toUpperCase()) {
      case 'APPROVED':
        return 'success';
      case 'REJECTED':
        return 'danger';
      default:
        return 'warning';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status?.toUpperCase()) {
      case 'APPROVED':
        return t('requests.statusApproved');
      case 'REJECTED':
        return t('requests.statusRejected');
      default:
        return t('requests.statusPending');
    }
  };

  const totalLeaves = data.leaves?.length || 0;
  const totalCorrections = data.corrections?.length || 0;
  const totalOvertimes = data.overtimes?.length || 0;
  const totalAll = totalLeaves + totalCorrections + totalOvertimes;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader title={t('requests.title')} showBack onBack={() => navigation.goBack()} />

      {/* Top Quick Actions */}
      <View style={[styles.quickBar, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
        <TouchableOpacity
          style={[styles.quickBtn, { borderColor: colors.primary }]}
          onPress={() => navigation.navigate('LeaveRequest')}
        >
          <Ionicons name="calendar-outline" size={16} color={colors.primary} />
          <Text style={[typography.captionBold, { color: colors.primary }]}>
            + {t('requests.leaveTitle')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickBtn, { borderColor: '#3B82F6' }]}
          onPress={() => navigation.navigate('AttendanceCorrection')}
        >
          <Ionicons name="time-outline" size={16} color="#3B82F6" />
          <Text style={[typography.captionBold, { color: '#3B82F6' }]}>
            + {t('requests.correctionTitle')}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.quickBtn, { borderColor: '#F59E0B' }]}
          onPress={() => navigation.navigate('MyOvertime')}
        >
          <Ionicons name="flash-outline" size={16} color="#F59E0B" />
          <Text style={[typography.captionBold, { color: '#F59E0B' }]}>
            + {t('requests.overtimeTitle')}
          </Text>
        </TouchableOpacity>
      </View>

      {/* Filter Tabs */}
      <View style={[styles.tabBar, { borderBottomColor: colors.border }]}>
        <TouchableOpacity
          style={[styles.tabItem, activeTab === 'ALL' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('ALL')}
        >
          <Text style={[typography.captionBold, { color: activeTab === 'ALL' ? colors.primary : colors.textSecondary }]}>
            {t('requests.allRequests')} ({totalAll})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.tabItem, activeTab === 'LEAVE' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('LEAVE')}
        >
          <Text style={[typography.captionBold, { color: activeTab === 'LEAVE' ? colors.primary : colors.textSecondary }]}>
            {t('nav.leaveRequest')} ({totalLeaves})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.tabItem, activeTab === 'CORRECTION' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('CORRECTION')}
        >
          <Text style={[typography.captionBold, { color: activeTab === 'CORRECTION' ? colors.primary : colors.textSecondary }]}>
            {t('nav.attendanceCorrection')} ({totalCorrections})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.tabItem, activeTab === 'OVERTIME' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('OVERTIME')}
        >
          <Text style={[typography.captionBold, { color: activeTab === 'OVERTIME' ? colors.primary : colors.textSecondary }]}>
            {t('nav.overtime')} ({totalOvertimes})
          </Text>
        </TouchableOpacity>
      </View>

      <ScrollView
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchAll(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {loading && !refreshing ? (
          <AppLoading />
        ) : (
          <>
            {/* LEAVES */}
            {(activeTab === 'ALL' || activeTab === 'LEAVE') &&
              data.leaves?.map((item) => (
                <AppCard key={`leave-${item.id}`} style={styles.card}>
                  <View style={styles.cardHeader}>
                    <View style={styles.cardTypeRow}>
                      <View style={[styles.iconCircle, { backgroundColor: 'rgba(59, 130, 246, 0.1)' }]}>
                        <Ionicons name="calendar" size={16} color={colors.primary} />
                      </View>
                      <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.sm }]}>
                        {item.leaveType}
                      </Text>
                    </View>
                    <AppBadge label={getStatusLabel(item.status)} variant={getStatusVariant(item.status)} />
                  </View>

                  <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.xs }]}>
                    {formatDate(item.fromDate, activeLanguage)} ➔ {formatDate(item.toDate, activeLanguage)} ({item.totalDays} ngày)
                  </Text>
                  <Text style={[typography.body, { color: colors.text, marginTop: spacing.xs }]}>
                    {item.reason}
                  </Text>
                  {item.note ? (
                    <Text style={[typography.captionBold, { color: colors.warning, marginTop: spacing.xs }]}>
                      {t('requests.note')}: {item.note}
                    </Text>
                  ) : null}
                </AppCard>
              ))}

            {/* CORRECTIONS */}
            {(activeTab === 'ALL' || activeTab === 'CORRECTION') &&
              data.corrections?.map((item) => (
                <AppCard key={`corr-${item.id}`} style={styles.card}>
                  <View style={styles.cardHeader}>
                    <View style={styles.cardTypeRow}>
                      <View style={[styles.iconCircle, { backgroundColor: 'rgba(16, 185, 129, 0.1)' }]}>
                        <Ionicons name="time" size={16} color={colors.success} />
                      </View>
                      <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.sm }]}>
                        {t('requests.correctionTitle')}
                      </Text>
                    </View>
                    <AppBadge label={getStatusLabel(item.status)} variant={getStatusVariant(item.status)} />
                  </View>

                  <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.xs }]}>
                    Ngày: {formatDate(item.workDate, activeLanguage)} • Vào: {item.requestedCheckIn || '--:--'} • Ra: {item.requestedCheckOut || '--:--'}
                  </Text>
                  <Text style={[typography.body, { color: colors.text, marginTop: spacing.xs }]}>
                    {item.reason}
                  </Text>
                  {item.note ? (
                    <Text style={[typography.captionBold, { color: colors.warning, marginTop: spacing.xs }]}>
                      {t('requests.note')}: {item.note}
                    </Text>
                  ) : null}
                </AppCard>
              ))}

            {/* OVERTIMES */}
            {(activeTab === 'ALL' || activeTab === 'OVERTIME') &&
              data.overtimes?.map((item) => (
                <AppCard key={`ot-${item.id}`} style={styles.card}>
                  <View style={styles.cardHeader}>
                    <View style={styles.cardTypeRow}>
                      <View style={[styles.iconCircle, { backgroundColor: 'rgba(245, 158, 11, 0.1)' }]}>
                        <Ionicons name="flash" size={16} color={colors.warning} />
                      </View>
                      <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.sm }]}>
                        {t('requests.overtimeTitle')} ({item.hours}h)
                      </Text>
                    </View>
                    <AppBadge label={getStatusLabel(item.status)} variant={getStatusVariant(item.status)} />
                  </View>

                  <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.xs }]}>
                    Ngày: {formatDate(item.otDate, activeLanguage)} • {item.shiftType}
                  </Text>
                  <Text style={[typography.body, { color: colors.text, marginTop: spacing.xs }]}>
                    {item.reason}
                  </Text>
                  {item.note ? (
                    <Text style={[typography.captionBold, { color: colors.warning, marginTop: spacing.xs }]}>
                      {t('requests.note')}: {item.note}
                    </Text>
                  ) : null}
                </AppCard>
              ))}

            {totalAll === 0 && (
              <AppEmptyState message={t('requests.emptyRequests')} />
            )}
          </>
        )}
      </ScrollView>
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
  quickBar: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    padding: spacing.sm,
    borderBottomWidth: 1,
    gap: spacing.xs,
  },
  quickBtn: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 4,
    paddingVertical: spacing.xs,
    paddingHorizontal: 2,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
  },
  tabBar: {
    flexDirection: 'row',
    borderBottomWidth: 1,
    paddingHorizontal: spacing.sm,
  },
  tabItem: {
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.sm,
  },
  card: {
    marginBottom: spacing.md,
    padding: spacing.md,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  cardTypeRow: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  iconCircle: {
    width: 28,
    height: 28,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
