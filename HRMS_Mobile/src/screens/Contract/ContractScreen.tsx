import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { ContractDto } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppErrorState } from '../../components/AppErrorState';
import { AppDivider } from '../../components/AppDivider';
import { formatDate, formatCurrency } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const ContractScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [contract, setContract] = useState<ContractDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchContract = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const data = await meApi.getContract();
      setContract(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchContract();
  }, [fetchContract]);

  const renderRow = (icon: keyof typeof Ionicons.glyphMap, label: string, value?: string | number | null) => (
    <View style={styles.infoRow}>
      <Ionicons name={icon} size={20} color={colors.primary} style={styles.rowIcon} />
      <View style={styles.textColumn}>
        <Text style={[typography.caption, { color: colors.textSecondary }]}>{label}</Text>
        <Text style={[typography.bodyBold, { color: colors.text, marginTop: 2 }]}>
          {value || t('common.notUpdated')}
        </Text>
      </View>
    </View>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader
        title={t('contract.title')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchContract(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {loading && !refreshing ? (
          <AppLoading />
        ) : errorMessage && !contract ? (
          <AppErrorState message={errorMessage} onRetry={() => fetchContract()} />
        ) : !contract ? (
          <AppEmptyState
            icon="document-text-outline"
            title={t('contract.noData')}
          />
        ) : (
          <>
            {/* Expiring Alert Banner */}
            {contract.isExpiringSoon && (
              <View style={[styles.alertBanner, { backgroundColor: 'rgba(245, 158, 11, 0.12)', borderColor: colors.warning }]}>
                <Ionicons name="alert-circle" size={24} color={colors.warning} />
                <Text style={[typography.bodyBold, { color: colors.warning, flex: 1, marginLeft: spacing.sm }]}>
                  {t('contract.expiringWarning')}
                </Text>
              </View>
            )}

            <AppCard>
              <View style={styles.cardHeader}>
                <View>
                  <Text style={[typography.caption, { color: colors.textSecondary }]}>
                    {t('contract.contractNumber')}
                  </Text>
                  <Text style={[typography.h3, { color: colors.text, marginTop: 2 }]}>
                    {contract.sohd || '---'}
                  </Text>
                </View>
                <AppBadge label={contract.tenLoaihd || 'Chính thức'} variant="info" />
              </View>

              <AppDivider marginVertical={spacing.md} />

              {renderRow('calendar-outline', t('contract.startDate'), formatDate(contract.ngaybatdau, activeLanguage))}
              <AppDivider marginVertical={spacing.sm} />
              {renderRow('calendar-outline', t('contract.endDate'), formatDate(contract.ngayketthuc, activeLanguage))}
              <AppDivider marginVertical={spacing.sm} />
              {renderRow('create-outline', t('contract.signDate'), formatDate(contract.ngayky, activeLanguage))}
              <AppDivider marginVertical={spacing.sm} />
              {renderRow('time-outline', t('contract.duration'), contract.thoihan)}
              <AppDivider marginVertical={spacing.sm} />
              {renderRow('repeat-outline', t('contract.signTimes'), contract.lanky ? `${contract.lanky}` : '1')}
              {contract.luongThoaThuan ? (
                <>
                  <AppDivider marginVertical={spacing.sm} />
                  {renderRow('cash-outline', t('contract.salary'), formatCurrency(contract.luongThoaThuan, activeLanguage))}
                </>
              ) : null}
            </AppCard>

            {contract.noiDung ? (
              <>
                <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
                  {t('contract.content')}
                </Text>
                <AppCard>
                  <Text style={[typography.body, { color: colors.text }]}>
                    {contract.noiDung}
                  </Text>
                </AppCard>
              </>
            ) : null}
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
  scrollContent: {
    padding: spacing.lg,
    paddingBottom: 40,
  },
  alertBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: spacing.md,
    borderRadius: spacing.borderRadiusMd,
    borderWidth: 1,
    marginBottom: spacing.md,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.xs,
  },
  rowIcon: {
    width: 28,
  },
  textColumn: {
    flex: 1,
    marginLeft: spacing.sm,
  },
});
