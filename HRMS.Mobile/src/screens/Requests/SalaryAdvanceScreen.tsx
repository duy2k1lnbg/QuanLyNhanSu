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
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { AdvanceSalaryDto } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppButton } from '../../components/AppButton';
import { AppTextInput } from '../../components/AppTextInput';
import { AppLoading } from '../../components/AppLoading';
import { useAuth } from '../../hooks/useAuth';
import { canMobile } from '../../utils/mobilePermissions';
import { localDate } from '../../utils/requestContracts';
import { AppErrorState } from '../../components/AppErrorState';
import { AppEmptyState } from '../../components/AppEmptyState';
import { formatDate, formatCurrency } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

const QUICK_AMOUNTS = [1000000, 2000000, 3000000, 5000000];

export const SalaryAdvanceScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const { user } = useAuth();
  const [history, setHistory] = useState<AdvanceSalaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Form states
  const [amount, setAmount] = useState<string>('2000000');
  const [advanceDate, setAdvanceDate] = useState<string>(() => {
    const today = new Date();
    return localDate(today);
  });
  const [reason, setReason] = useState<string>('');

  const fetchHistory = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    setErrorMessage(null);
    try {
      const data = await meApi.getAdvanceRequests();
      setHistory(data);
    } catch (error) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  const getAdvanceStatusBadge = (status?: string) => {
    const s = status?.toUpperCase();
    if (s === 'DISBURSED') {
      return { label: t('mobileReview.statusDISBURSED'), variant: 'success' as const };
    }
    if (s === 'APPROVED') {
      return { label: t('requests.statusApproved', 'Đã duyệt'), variant: 'info' as const };
    }
    if (s === 'REJECTED') {
      return { label: t('requests.statusRejected', 'Từ chối'), variant: 'danger' as const };
    }
    return { label: t('requests.statusPending', 'Chờ duyệt'), variant: 'warning' as const };
  };

  useFocusEffect(useCallback(() => { fetchHistory(); }, [fetchHistory]));

  const handleSubmit = async () => {
    if (submitting) return;

    const numAmount = /^\d+$/.test(amount.trim()) ? Number(amount):NaN;
    if (isNaN(numAmount) || numAmount <= 0) {
      Alert.alert(t('common.error'), t('mobileReview.invalidAmount'));
      return;
    }

    if (numAmount > 100000000) {
      Alert.alert(t('common.error'), t('mobileReview.invalidAmount'));
      return;
    }

    if (!advanceDate.trim()) {
      Alert.alert(t('common.error'), t('mobileReview.invalidDate'));
      return;
    }

    if (!reason.trim()) {
      Alert.alert(t('common.error'), t('mobileReview.requiredFields'));
      return;
    }

    setSubmitting(true);
    try {
      await meApi.createAdvanceRequest({
        amount: numAmount,
        advanceDate: advanceDate.trim(),
        reason: reason.trim(),
      });

      Alert.alert(
        t('common.success'),
        t('mobileReview.sentForApproval')
      );
      setReason('');
      fetchHistory(true);
    } catch (error: any) {
      Alert.alert(t('common.error'), mapApiError(error));
    } finally {
      setSubmitting(false);
    }
  };

  if (errorMessage) return <AppErrorState message={errorMessage} onRetry={() => fetchHistory()} />;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader
        title={t('requests.advanceTitle')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchHistory(true)}
            tintColor={colors.primary}
            colors={[colors.primary]}
          />
        }
      >
        {/* Banner giới thiệu */}
        <AppCard style={styles.bannerCard}>
          <View style={styles.bannerHeader}>
            <View style={[styles.iconBox, { backgroundColor: colors.warning + '20' }]}>
              <Ionicons name="cash-outline" size={26} color={colors.warning} />
            </View>
            <View style={styles.bannerText}>
              <Text style={[styles.bannerTitle, { color: colors.text }]}>
                {t('requests.advanceTitle')}
              </Text>
              <Text style={[styles.bannerSub, { color: colors.textSecondary }]}>
                {t('requests.advanceSubtitle')}
              </Text>
            </View>
          </View>
        </AppCard>

        {/* Form nộp đơn */}
        <AppCard style={styles.formCard}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>
            {t('mobileReview.createAdvance')}
          </Text>

          <Text style={[styles.label, { color: colors.text }]}>
            {t('requests.advanceAmount')}
          </Text>
          <AppTextInput
            placeholder={t('requests.advanceAmountPlaceholder')}
            value={amount}
            onChangeText={(txt) => setAmount(txt)}
            keyboardType="numeric"
            leftIcon="wallet-outline"
          />

          {/* Quick Amount Chips */}
          <View style={styles.quickChipContainer}>
            {QUICK_AMOUNTS.map((amt) => {
              const isSelected = amount === amt.toString();
              return (
                <TouchableOpacity
                  key={amt}
                  style={[
                    styles.quickChip,
                    {
                      borderColor: isSelected ? colors.primary : colors.border,
                      backgroundColor: isSelected ? colors.primary + '15' : colors.surfaceCard,
                    },
                  ]}
                  onPress={() => setAmount(amt.toString())}
                >
                  <Text
                    style={[
                      styles.quickChipText,
                      { color: isSelected ? colors.primary : colors.textSecondary },
                    ]}
                  >
                    {(amt / 1000000).toLocaleString()} tr
                  </Text>
                </TouchableOpacity>
              );
            })}
          </View>

          <Text style={[styles.label, { color: colors.text }]}>
            {t('requests.advanceDate')} (YYYY-MM-DD)
          </Text>
          <AppTextInput
            placeholder="YYYY-MM-DD"
            value={advanceDate}
            onChangeText={setAdvanceDate}
            leftIcon="calendar-outline"
          />

          <Text style={[styles.label, { color: colors.text }]}>
            {t('requests.reason')}
          </Text>
          <AppTextInput
            placeholder={t('requests.reasonPlaceholder')}
            value={reason}
            onChangeText={setReason}
            multiline
            numberOfLines={3}
            style={styles.textArea}
            leftIcon="chatbox-ellipses-outline"
          />

          <AppButton
            title={submitting ? t('requests.submitting') : t('requests.submit')}
            onPress={handleSubmit}
            loading={submitting}
            disabled={!canMobile(user, 'MOBILE_REQUEST_ADVANCE', 'Add')}
            style={styles.submitBtn}
          />
        </AppCard>

        {/* Lịch sử */}
        <View style={styles.historyHeader}>
          <Text style={[styles.historyTitle, { color: colors.text }]}>
            {t('requests.history')}
          </Text>
        </View>

        {loading && !refreshing ? (
          <AppLoading message={t('common.loading')} />
        ) : history.length === 0 ? (
          <AppEmptyState
            icon="receipt-outline"
            title={t('requests.emptyRequests')}
            message={t('requests.emptyRequests')}
          />
        ) : (
          history.map((item, index) => {
            const displayDate = item.advanceDate
              ? formatDate(item.advanceDate, activeLanguage)
              : item.ngayUng
              ? formatDate(item.ngayUng, activeLanguage)
              : item.thang && item.nam
              ? (item.ngay ? `${item.ngay}/${item.thang}/${item.nam}` : `${item.thang}/${item.nam}`)
              : '--/--/----';
            const displayAmount = formatCurrency(item.soTienUng || item.amount || 0, activeLanguage);

            const reasonText = item.ghiChu || item.reason || (item as any).lyDo;

            return (
              <AppCard key={item.idUl || item.id || (item as any).id_ul || `adv-${index}`} style={styles.historyCard}>
                <View style={styles.cardHeader}>
                  <View style={styles.cardHeaderLeft}>
                    <View style={[styles.amountIconCircle, { backgroundColor: colors.primary + '15' }]}>
                      <Ionicons name="cash" size={20} color={colors.primary} />
                    </View>
                    <View>
                      <Text style={[styles.amountText, { color: colors.text }]}>
                        {displayAmount}
                      </Text>
                      <Text style={[styles.dateText, { color: colors.textSecondary }]}>
                        {displayDate}
                      </Text>
                    </View>
                  </View>
                  {(() => {
                    const badge = getAdvanceStatusBadge(item.status);
                    return <AppBadge label={badge.label} variant={badge.variant} />;
                  })()}
                </View>

                {!!reasonText && (
                  <View style={[styles.reasonBox, { backgroundColor: colors.background }]}>
                    <Text style={[styles.reasonText, { color: colors.text }]}>
                      {reasonText}
                    </Text>
                  </View>
                )}

                {!!item.createdAt && (
                  <Text style={[styles.metaText, { color: colors.textSecondary }]}>
                    {t('mobileReview.createdOn')}: {formatDate(item.createdAt, activeLanguage)}
                  </Text>
                )}
              </AppCard>
            );
          })
        )}
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    padding: spacing.md,
    paddingBottom: spacing.xxl,
  },
  bannerCard: {
    marginBottom: spacing.md,
  },
  bannerHeader: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  iconBox: {
    width: 48,
    height: 48,
    borderRadius: 24,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: spacing.md,
  },
  bannerText: {
    flex: 1,
  },
  bannerTitle: {
    ...typography.h3,
    marginBottom: 4,
  },
  bannerSub: {
    ...typography.body,
  },
  formCard: {
    marginBottom: spacing.lg,
  },
  sectionTitle: {
    ...typography.h3,
    marginBottom: spacing.md,
  },
  label: {
    ...typography.bodyBold,
    marginBottom: spacing.xs,
    marginTop: spacing.sm,
  },
  quickChipContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
    marginBottom: spacing.sm,
  },
  quickChip: {
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.md,
    borderRadius: 16,
    borderWidth: 1,
  },
  quickChipText: {
    ...typography.body,
    fontWeight: '600',
  },
  textArea: {
    height: 80,
    textAlignVertical: 'top',
  },
  submitBtn: {
    marginTop: spacing.lg,
  },
  historyHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.md,
  },
  historyTitle: {
    ...typography.h3,
  },
  historyCard: {
    marginBottom: spacing.sm,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  cardHeaderLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  amountIconCircle: {
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
  },
  amountText: {
    ...typography.h3,
    fontWeight: '700',
  },
  dateText: {
    ...typography.caption,
    marginTop: 2,
  },
  reasonBox: {
    marginTop: spacing.sm,
    padding: spacing.sm,
    borderRadius: 8,
  },
  reasonText: {
    ...typography.body,
  },
  metaText: {
    ...typography.caption,
    marginTop: spacing.xs,
  },
});
