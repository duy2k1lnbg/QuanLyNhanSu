import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  Alert,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { AttendanceCorrectionDto } from '../../types/me';
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

export const AttendanceCorrectionScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [history, setHistory] = useState<AttendanceCorrectionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Form states
  const [workDate, setWorkDate] = useState<string>(() => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  });
  const [checkIn, setCheckIn] = useState<string>('08:00');
  const [checkOut, setCheckOut] = useState<string>('17:00');
  const [reason, setReason] = useState<string>('');

  const fetchHistory = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    try {
      const data = await meApi.getAttendanceCorrections();
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

    if (!workDate.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nhập ngày làm việc cần điều chỉnh.');
      return;
    }

    if (!checkIn.trim() && !checkOut.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nhập ít nhất một trong hai: giờ vào hoặc giờ ra.');
      return;
    }

    if (!reason.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nêu rõ lý do điều chỉnh công.');
      return;
    }

    setSubmitting(true);
    try {
      await meApi.createAttendanceCorrection({
        workDate: workDate.trim(),
        requestedCheckIn: checkIn.trim() || undefined,
        requestedCheckOut: checkOut.trim() || undefined,
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
      <AppHeader title={t('requests.correctionTitle')} showBack onBack={() => navigation.goBack()} />

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
        {/* Form Create Correction */}
        <AppCard style={styles.formCard}>
          <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.xs }]}>
            {t('requests.correctionTitle')}
          </Text>
          <Text style={[typography.caption, { color: colors.textSecondary, marginBottom: spacing.md }]}>
            {t('requests.correctionSubtitle')}
          </Text>

          <AppTextInput
            label={t('requests.workDate')}
            placeholder="YYYY-MM-DD"
            value={workDate}
            onChangeText={setWorkDate}
            leftIcon="calendar-outline"
          />

          <View style={styles.rowInputs}>
            <View style={{ flex: 1 }}>
              <AppTextInput
                label={t('requests.checkIn')}
                placeholder="08:00"
                value={checkIn}
                onChangeText={setCheckIn}
                leftIcon="log-in-outline"
              />
            </View>
            <View style={{ width: spacing.md }} />
            <View style={{ flex: 1 }}>
              <AppTextInput
                label={t('requests.checkOut')}
                placeholder="17:00"
                value={checkOut}
                onChangeText={setCheckOut}
                leftIcon="log-out-outline"
              />
            </View>
          </View>

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
                  <Ionicons name="time" size={18} color={colors.primary} />
                  <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.xs }]}>
                    {formatDate(item.workDate, activeLanguage)}
                  </Text>
                </View>
                <AppBadge label={getStatusLabel(item.status)} variant={getStatusVariant(item.status)} />
              </View>

              <View style={styles.timeBadgeRow}>
                <View style={[styles.timeChip, { backgroundColor: 'rgba(59, 130, 246, 0.08)' }]}>
                  <Text style={[typography.captionBold, { color: colors.primary }]}>
                    Vào: {item.requestedCheckIn || '--:--'}
                  </Text>
                </View>
                <View style={[styles.timeChip, { backgroundColor: 'rgba(16, 185, 129, 0.08)' }]}>
                  <Text style={[typography.captionBold, { color: colors.success }]}>
                    Ra: {item.requestedCheckOut || '--:--'}
                  </Text>
                </View>
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
  timeBadgeRow: {
    flexDirection: 'row',
    gap: spacing.sm,
    marginTop: spacing.xs,
  },
  timeChip: {
    paddingVertical: 2,
    paddingHorizontal: spacing.sm,
    borderRadius: spacing.borderRadiusSm,
  },
  noteBox: {
    marginTop: spacing.sm,
    padding: spacing.sm,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
  },
});
