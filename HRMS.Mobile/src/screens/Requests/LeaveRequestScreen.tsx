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
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { LeaveRequestDto } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppButton } from '../../components/AppButton';
import { AppTextInput } from '../../components/AppTextInput';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { formatDate } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const LeaveRequestScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [history, setHistory] = useState<LeaveRequestDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Form states
  const [leaveType, setLeaveType] = useState<string>('Nghỉ phép năm');
  const [fromDate, setFromDate] = useState<string>(() => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  });
  const [toDate, setToDate] = useState<string>(() => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  });
  const [totalDays, setTotalDays] = useState<string>('1');
  const [reason, setReason] = useState<string>('');

  const leaveTypeOptions = [
    { label: t('requests.annualLeave'), value: 'Nghỉ phép năm' },
    { label: t('requests.sickLeave'), value: 'Nghỉ ốm hưởng BHXH' },
    { label: t('requests.unpaidLeave'), value: 'Nghỉ không lương' },
    { label: t('requests.otherLeave'), value: 'Nghỉ việc riêng' },
  ];

  const fetchHistory = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    try {
      const data = await meApi.getLeaveRequests();
      setHistory(data);
    } catch {
      // ignore
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchHistory();
  }, [fetchHistory]);

  const handleSubmit = async () => {
    if (submitting) return;

    if (!fromDate.trim() || !toDate.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nhập ngày bắt đầu và ngày kết thúc.');
      return;
    }

    const days = parseFloat(totalDays);
    if (isNaN(days) || days <= 0) {
      Alert.alert(t('common.error'), 'Số ngày nghỉ không hợp lệ.');
      return;
    }

    if (!reason.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nhập lý do xin nghỉ phép.');
      return;
    }

    setSubmitting(true);
    try {
      await meApi.createLeaveRequest({
        leaveType,
        fromDate: fromDate.trim(),
        toDate: toDate.trim(),
        totalDays: days,
        reason: reason.trim(),
      });

      Alert.alert(t('common.success'), t('requests.submitSuccess'));
      setReason('');
      fetchHistory(true);
    } catch (error: any) {
      Alert.alert(t('common.error'), mapApiError(error));
    } finally {
      setSubmitting(false);
    }
  };

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

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader title={t('requests.leaveTitle')} showBack onBack={() => navigation.goBack()} />

      <ScrollView
        contentContainerStyle={styles.content}
        keyboardShouldPersistTaps="handled"
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchHistory(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {/* Form Create Leave Request */}
        <AppCard style={styles.formCard}>
          <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.xs }]}>
            {t('requests.leaveTitle')}
          </Text>
          <Text style={[typography.caption, { color: colors.textSecondary, marginBottom: spacing.md }]}>
            {t('requests.leaveSubtitle')}
          </Text>

          {/* Leave Type Selector Chips */}
          <Text style={[typography.captionBold, { color: colors.text, marginBottom: spacing.xs }]}>
            {t('requests.leaveType')}
          </Text>
          <View style={styles.chipRow}>
            {leaveTypeOptions.map((opt) => {
              const isSelected = leaveType === opt.value;
              return (
                <TouchableOpacity
                  key={opt.value}
                  activeOpacity={0.7}
                  onPress={() => setLeaveType(opt.value)}
                  style={[
                    styles.chip,
                    {
                      backgroundColor: isSelected ? colors.primary : colors.surfaceCard,
                      borderColor: isSelected ? colors.primary : colors.border,
                    },
                  ]}
                >
                  <Text
                    style={[
                      typography.caption,
                      { color: isSelected ? '#FFFFFF' : colors.text, fontWeight: isSelected ? '700' : '500' },
                    ]}
                  >
                    {opt.label}
                  </Text>
                </TouchableOpacity>
              );
            })}
          </View>

          <View style={styles.rowInputs}>
            <View style={{ flex: 1 }}>
              <AppTextInput
                label={t('requests.fromDate')}
                placeholder="YYYY-MM-DD"
                value={fromDate}
                onChangeText={setFromDate}
                leftIcon="calendar-outline"
              />
            </View>
            <View style={{ width: spacing.md }} />
            <View style={{ flex: 1 }}>
              <AppTextInput
                label={t('requests.toDate')}
                placeholder="YYYY-MM-DD"
                value={toDate}
                onChangeText={setToDate}
                leftIcon="calendar-outline"
              />
            </View>
          </View>

          <AppTextInput
            label={t('requests.totalDays')}
            placeholder="1"
            value={totalDays}
            onChangeText={setTotalDays}
            keyboardType="numeric"
            leftIcon="time-outline"
          />

          <AppTextInput
            label={t('requests.reason')}
            placeholder={t('requests.reasonPlaceholder')}
            value={reason}
            onChangeText={setReason}
            multiline
            numberOfLines={3}
            leftIcon="chatbox-outline"
          />

          <AppButton
            title={submitting ? t('requests.submitting') : t('requests.submit')}
            onPress={handleSubmit}
            loading={submitting}
            variant="primary"
            size="lg"
            style={{ marginTop: spacing.sm }}
          />
        </AppCard>

        {/* History Section */}
        <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.md }]}>
          {t('requests.history')}
        </Text>

        {loading && !refreshing ? (
          <AppLoading />
        ) : history.length === 0 ? (
          <AppEmptyState message={t('requests.emptyRequests')} />
        ) : (
          history.map((item) => (
            <AppCard key={item.id} style={styles.historyCard}>
              <View style={styles.historyHeader}>
                <View style={styles.historyTypeWrapper}>
                  <Ionicons name="calendar" size={18} color={colors.primary} />
                  <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.xs }]}>
                    {item.leaveType}
                  </Text>
                </View>
                <AppBadge label={getStatusLabel(item.status)} variant={getStatusVariant(item.status)} />
              </View>

              <View style={styles.historyMetaRow}>
                <Text style={[typography.caption, { color: colors.textSecondary }]}>
                  {formatDate(item.fromDate, activeLanguage)} ➔ {formatDate(item.toDate, activeLanguage)} ({item.totalDays} ngày)
                </Text>
              </View>

              <Text style={[typography.body, { color: colors.text, marginTop: spacing.xs }]}>
                {item.reason}
              </Text>

              {item.note ? (
                <View style={[styles.noteBox, { backgroundColor: 'rgba(245, 158, 11, 0.08)', borderColor: colors.border }]}>
                  <Text style={[typography.captionBold, { color: colors.warning }]}>
                    {t('requests.note')}: {item.note}
                  </Text>
                </View>
              ) : null}

              {item.approverName ? (
                <Text style={[typography.caption, { color: colors.textMuted, marginTop: spacing.xs }]}>
                  {t('requests.approver')}: {item.approverName} {item.approvedAt ? `• ${formatDate(item.approvedAt, activeLanguage)}` : ''}
                </Text>
              ) : null}
            </AppCard>
          ))
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
  formCard: {
    padding: spacing.lg,
  },
  chipRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.xs,
    marginBottom: spacing.md,
  },
  chip: {
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.md,
    borderRadius: spacing.borderRadiusFull,
    borderWidth: 1,
  },
  rowInputs: {
    flexDirection: 'row',
  },
  historyCard: {
    marginBottom: spacing.md,
    padding: spacing.lg,
  },
  historyHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  historyTypeWrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  historyMetaRow: {
    marginTop: spacing.xs,
  },
  noteBox: {
    marginTop: spacing.sm,
    padding: spacing.sm,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
  },
});
