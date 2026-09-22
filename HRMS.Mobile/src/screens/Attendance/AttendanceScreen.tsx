import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  FlatList,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '../../hooks/useTheme';
import { meApi } from '../../api/meApi';
import { AttendanceDto, AttendanceDailyItemDto } from '../../types/me';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppErrorState } from '../../components/AppErrorState';
import { mapApiError } from '../../utils/errorMapper';
import { getLocalizedAttendanceStatus } from '../../utils/statusMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const AttendanceScreen: React.FC = () => {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();

  // Current date tracking
  const [currentDate, setCurrentDate] = useState(() => {
    const d = new Date();
    return { year: d.getFullYear(), month: d.getMonth() + 1 };
  });

  const [attendance, setAttendance] = useState<AttendanceDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchAttendance = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    const monthParam = `${currentDate.year}-${String(currentDate.month).padStart(2, '0')}`;

    try {
      const data = await meApi.getAttendance(monthParam);
      setAttendance(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [currentDate]);

  useEffect(() => {
    fetchAttendance();
  }, [fetchAttendance]);

  const handlePrevMonth = () => {
    setCurrentDate((prev) => {
      if (prev.month === 1) return { year: prev.year - 1, month: 12 };
      return { year: prev.year, month: prev.month - 1 };
    });
  };

  const handleNextMonth = () => {
    setCurrentDate((prev) => {
      if (prev.month === 12) return { year: prev.year + 1, month: 1 };
      return { year: prev.year, month: prev.month + 1 };
    });
  };

  const summary = attendance?.summary || (attendance as any)?.Summary;
  const dailyList = attendance?.dailyList || (attendance as any)?.DailyList || [];

  const getStatusVariant = (status: string) => {
    if (status.includes('Đủ') || status.toLowerCase().includes('full')) return 'success';
    if (status.includes('muộn') || status.toLowerCase().includes('late')) return 'warning';
    if (status.includes('Vắng') || status.toLowerCase().includes('absent')) return 'danger';
    return 'default';
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {/* Month Navigation Header */}
      <View
        style={[
          styles.monthNav,
          {
            backgroundColor: colors.surfaceCard,
            borderBottomColor: colors.border,
            paddingTop: Math.max(insets.top, spacing.md),
          },
        ]}
      >
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={handlePrevMonth}
          style={styles.navArrow}
        >
          <Ionicons name="chevron-back" size={24} color={colors.text} />
        </TouchableOpacity>

        <Text style={[typography.h3, { color: colors.text }]}>
          {t('attendance.title')}: {String(currentDate.month).padStart(2, '0')}/{currentDate.year}
        </Text>

        <TouchableOpacity
          activeOpacity={0.7}
          onPress={handleNextMonth}
          style={styles.navArrow}
        >
          <Ionicons name="chevron-forward" size={24} color={colors.text} />
        </TouchableOpacity>
      </View>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchAttendance(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {loading && !refreshing ? (
          <AppLoading />
        ) : errorMessage && !attendance ? (
          <AppErrorState message={errorMessage} onRetry={() => fetchAttendance()} />
        ) : (
          <>
            {/* 1. Summary Cards */}
            <AppCard style={styles.summaryContainer}>
              <Text style={[typography.bodyBold, { color: colors.text, marginBottom: spacing.md }]}>
                {t('attendance.summaryTitle')}
              </Text>
              <View style={styles.summaryGrid}>
                <View style={styles.summaryItem}>
                  <Text style={[typography.caption, { color: colors.textSecondary }]}>
                    {t('attendance.workDays')}
                  </Text>
                  <Text style={[typography.h2, { color: colors.primary }]}>
                    {summary ? summary.tongNgayCong : 0}
                  </Text>
                </View>

                <View style={styles.summaryItem}>
                  <Text style={[typography.caption, { color: colors.textSecondary }]}>
                    {t('attendance.standardDays')}
                  </Text>
                  <Text style={[typography.h2, { color: colors.text }]}>
                    {summary ? summary.ngayCongChuan : 0}
                  </Text>
                </View>

                <View style={styles.summaryItem}>
                  <Text style={[typography.caption, { color: colors.textSecondary }]}>
                    {t('attendance.lateCount')}
                  </Text>
                  <Text style={[typography.h2, { color: colors.warning }]}>
                    {summary ? summary.soLanDiMuon : 0}
                  </Text>
                </View>

                <View style={styles.summaryItem}>
                  <Text style={[typography.caption, { color: colors.textSecondary }]}>
                    {t('attendance.overtime')}
                  </Text>
                  <Text style={[typography.h2, { color: colors.info }]}>
                    {summary ? summary.soGioTangCa : 0}h
                  </Text>
                </View>
              </View>
            </AppCard>

            {/* 2. Daily List */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              {t('attendance.dailyDetails')} ({dailyList.length})
            </Text>

            {dailyList.length === 0 ? (
              <AppEmptyState
                icon="calendar-outline"
                title={t('attendance.noData')}
              />
            ) : (
              dailyList.map((item: AttendanceDailyItemDto, index: number) => (
                <AppCard key={index} style={styles.dailyCard}>
                  <View style={styles.dailyTopRow}>
                    <View style={styles.dailyDateGroup}>
                      <View style={[styles.dayPill, { backgroundColor: colors.primary + '20' }]}>
                        <Text style={[typography.captionBold, { color: colors.primary }]}>
                          {item.thu || 'T'}
                        </Text>
                      </View>
                      <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.sm }]}>
                        {item.ngay}
                      </Text>
                    </View>

                    <AppBadge
                      label={getLocalizedAttendanceStatus(item.trangThai) || item.kyHieu || 'OK'}
                      variant={getStatusVariant(item.trangThai || '')}
                    />
                  </View>

                  <View style={[styles.dailyBottomRow, { borderTopColor: colors.border }]}>
                    <View style={styles.timeCol}>
                      <Text style={[typography.caption, { color: colors.textMuted }]}>
                        {t('attendance.timeIn')}
                      </Text>
                      <Text style={[typography.bodyBold, { color: colors.text, marginTop: 2 }]}>
                        {item.gioVao || '--:--'}
                      </Text>
                    </View>
                    <View style={styles.timeCol}>
                      <Text style={[typography.caption, { color: colors.textMuted }]}>
                        {t('attendance.timeOut')}
                      </Text>
                      <Text style={[typography.bodyBold, { color: colors.text, marginTop: 2 }]}>
                        {item.gioRa || '--:--'}
                      </Text>
                    </View>
                    <View style={styles.timeCol}>
                      <Text style={[typography.caption, { color: colors.textMuted }]}>
                        {t('attendance.workDays')}
                      </Text>
                      <Text style={[typography.bodyBold, { color: colors.primary, marginTop: 2 }]}>
                        {item.ngayCong ?? 0}
                      </Text>
                    </View>
                  </View>
                </AppCard>
              ))
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
  monthNav: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.md,
    borderBottomWidth: 1,
  },
  navArrow: {
    padding: spacing.sm,
  },
  scrollContent: {
    padding: spacing.lg,
    paddingBottom: 40,
  },
  summaryContainer: {
    padding: spacing.lg,
  },
  summaryGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
  },
  summaryItem: {
    width: '46%',
    flexGrow: 1,
  },
  dailyCard: {
    marginBottom: spacing.sm,
    padding: spacing.md,
  },
  dailyTopRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingBottom: spacing.sm,
  },
  dailyDateGroup: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  dayPill: {
    paddingHorizontal: spacing.sm,
    paddingVertical: 2,
    borderRadius: spacing.borderRadiusSm,
    minWidth: 32,
    alignItems: 'center',
  },
  dailyBottomRow: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    paddingTop: spacing.sm,
    borderTopWidth: 1,
  },
  timeCol: {
    alignItems: 'center',
    flex: 1,
  },
});
