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
import { InsuranceDto } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppErrorState } from '../../components/AppErrorState';
import { AppDivider } from '../../components/AppDivider';
import { formatDate, formatCurrency } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const InsuranceScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [insurance, setInsurance] = useState<InsuranceDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchInsurance = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const data = await meApi.getInsurance();
      setInsurance(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchInsurance();
  }, [fetchInsurance]);

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
        title={t('insurance.title')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchInsurance(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {loading && !refreshing ? (
          <AppLoading />
        ) : errorMessage && !insurance ? (
          <AppErrorState message={errorMessage} onRetry={() => fetchInsurance()} />
        ) : !insurance ? (
          <AppEmptyState
            icon="medkit-outline"
            title={t('insurance.noData')}
          />
        ) : (
          <AppCard>
            <View style={styles.cardHeader}>
              <View style={[styles.iconCircle, { backgroundColor: 'rgba(14, 165, 233, 0.1)' }]}>
                <Ionicons name="shield-checkmark-outline" size={28} color="#0EA5E9" />
              </View>
              <View style={{ flex: 1, marginLeft: spacing.md }}>
                <Text style={[typography.caption, { color: colors.textSecondary }]}>
                  {t('insurance.bookNumber')}
                </Text>
                <Text style={[typography.h2, { color: colors.text }]}>
                  {insurance.sobh || '---'}
                </Text>
              </View>
            </View>

            <AppDivider marginVertical={spacing.lg} />

            {renderRow('calendar-outline', t('insurance.issuedDate'), formatDate(insurance.ngaycap, activeLanguage))}
            <AppDivider marginVertical={spacing.sm} />
            {renderRow('location-outline', t('insurance.issuedPlace'), insurance.noicap)}
            <AppDivider marginVertical={spacing.sm} />
            {renderRow('business-outline', t('insurance.hospital'), insurance.noikhambenh)}
            {insurance.luongBhxh ? (
              <>
                <AppDivider marginVertical={spacing.sm} />
                {renderRow('cash-outline', t('insurance.insuranceSalary'), formatCurrency(insurance.luongBhxh, activeLanguage))}
              </>
            ) : null}
          </AppCard>
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
  cardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  iconCircle: {
    width: 56,
    height: 56,
    borderRadius: 28,
    alignItems: 'center',
    justifyContent: 'center',
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
