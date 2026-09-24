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
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { PayrollDto } from '../../types/me';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppErrorState } from '../../components/AppErrorState';
import { AppDivider } from '../../components/AppDivider';
import { formatCurrency } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { getLocalizedPayrollStatus } from '../../utils/statusMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const PayrollScreen: React.FC = () => {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const { activeLanguage } = useLanguage();

  const [currentDate, setCurrentDate] = useState(() => {
    const d = new Date();
    return { year: d.getFullYear(), month: d.getMonth() + 1 };
  });

  const [payroll, setPayroll] = useState<PayrollDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchPayroll = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const data = await meApi.getPayroll(currentDate.year, currentDate.month);
      setPayroll(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [currentDate]);

  useEffect(() => {
    fetchPayroll();
  }, [fetchPayroll]);

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

  const renderItemRow = (label: string, amount: number, isDeduction = false) => (
    <View style={styles.detailRow}>
      <Text style={[typography.body, { color: colors.textSecondary }]}>
        {label}
      </Text>
      <Text
        style={[
          typography.bodyBold,
          { color: isDeduction ? colors.danger : colors.text },
        ]}
      >
        {isDeduction && amount > 0 ? '-' : ''}
        {formatCurrency(amount, activeLanguage)}
      </Text>
    </View>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {/* Month Navigation */}
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
        <TouchableOpacity activeOpacity={0.7} onPress={handlePrevMonth} style={styles.navArrow}>
          <Ionicons name="chevron-back" size={24} color={colors.text} />
        </TouchableOpacity>

        <Text style={[typography.h3, { color: colors.text }]}>
          {t('payroll.title')}: {String(currentDate.month).padStart(2, '0')}/{currentDate.year}
        </Text>

        <TouchableOpacity activeOpacity={0.7} onPress={handleNextMonth} style={styles.navArrow}>
          <Ionicons name="chevron-forward" size={24} color={colors.text} />
        </TouchableOpacity>
      </View>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchPayroll(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {loading && !refreshing ? (
          <AppLoading />
        ) : errorMessage && !payroll ? (
          <AppErrorState message={errorMessage} onRetry={() => fetchPayroll()} />
        ) : !payroll ? (
          <AppEmptyState
            icon="wallet-outline"
            title={t('payroll.noData')}
          />
        ) : (
          <>
            {/* 1. Main Net Salary Highlight Card */}
            <AppCard style={[styles.highlightCard, { backgroundColor: colors.primary }]}>
              <Text style={[typography.captionBold, { color: '#E0E7FF' }]}>
                {t('payroll.netSalary')}
              </Text>
              <Text
                style={[typography.h1, { color: '#FFFFFF', fontSize: 28, marginVertical: spacing.sm, paddingHorizontal: spacing.sm }]}
                numberOfLines={1}
                adjustsFontSizeToFit
              >
                {formatCurrency(payroll.thucLinh, activeLanguage)}
              </Text>
              <View style={styles.statusBadgeRow}>
                <AppBadge
                  label={getLocalizedPayrollStatus(payroll.trangThaiChiTra)}
                  variant={payroll.trangThaiChiTra?.toLowerCase().includes('đã') || payroll.trangThaiChiTra?.toLowerCase().includes('paid') ? 'success' : 'warning'}
                />
              </View>
            </AppCard>

            {/* 2. Earnings Section */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              {t('payroll.totalEarnings')}
            </Text>
            <AppCard>
              {renderItemRow(t('payroll.baseSalary'), payroll.luongCoBan)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.actualWorkSalary'), payroll.luongCongThucTe)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.allowance'), payroll.phuCapCongThucTe)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.overtimePay'), payroll.tienTangCa)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.attendanceBonus'), payroll.tienChuyenCan)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.shiftMeal'), payroll.tienAnCa)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.otherBonus'), payroll.khoanCongKhac)}
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>
                  {t('payroll.totalEarnings')}
                </Text>
                <Text style={[typography.bodyBold, { color: colors.success }]}>
                  {formatCurrency(payroll.tongThuNhap, activeLanguage)}
                </Text>
              </View>
            </AppCard>

            {/* 3. Deductions Section */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              {t('payroll.totalDeductions')}
            </Text>
            <AppCard>
              {renderItemRow(t('payroll.insuranceDeduction'), (payroll.tienBhxh || 0) + (payroll.tienBhyt || 0) + (payroll.tienBhtn || 0), true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.unionFee'), payroll.tienCongDoan, true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.advanceSalary'), payroll.tienTamUng, true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.personalTax'), payroll.thueTncn, true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(t('payroll.otherDeduction'), payroll.khoanTruKhac, true)}
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>
                  {t('payroll.totalDeductions')}
                </Text>
                <Text style={[typography.bodyBold, { color: colors.danger }]}>
                  -{formatCurrency(payroll.tongKhauTru, activeLanguage)}
                </Text>
              </View>
            </AppCard>

            {/* 4. Attendance Section */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              Chấm công & Ngày làm việc
            </Text>
            <AppCard>
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Số công chuẩn</Text>
                <Text style={[typography.bodyBold, { color: colors.text }]}>{payroll.congChuan ?? 0} ngày</Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Số công thực tế</Text>
                <Text style={[typography.bodyBold, { color: colors.primary }]}>{payroll.congThucTe ?? 0} ngày</Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Công làm ca ngày</Text>
                <Text style={[typography.bodyBold, { color: colors.text }]}>{payroll.congLamNgay ?? 0} ngày</Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Công làm ca đêm</Text>
                <Text style={[typography.bodyBold, { color: colors.text }]}>{payroll.congLamDem ?? 0} ngày</Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Số giờ tăng ca</Text>
                <Text style={[typography.bodyBold, { color: colors.text }]}>{payroll.soGioTangCa ?? 0} giờ</Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Đơn giá ngày</Text>
                <Text style={[typography.bodyBold, { color: colors.text }]}>{formatCurrency(payroll.dailyRate, activeLanguage)}</Text>
              </View>
            </AppCard>

            {/* 5. Personal Income Tax (PIT) Details */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              Chi tiết thuế TNCN
            </Text>
            <AppCard>
              {renderItemRow('Thu nhập chịu thuế', payroll.thuNhapChiuThue ?? payroll.tongThuNhap)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow('Giảm trừ bản thân', payroll.giamTruBanThan ?? 0, true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow(`Giảm trừ người phụ thuộc (${payroll.soNguoiPhuThuoc ?? 0} người)`, payroll.giamTruPhuThuoc ?? 0, true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow('Giảm trừ bảo hiểm bắt buộc', payroll.giamTruBaoHiem ?? (payroll.tienBhxh + payroll.tienBhyt + payroll.tienBhtn), true)}
              <AppDivider marginVertical={spacing.sm} />
              {renderItemRow('Thu nhập tính thuế (TNTT)', payroll.thuNhapTinhThue ?? 0)}
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>Thuế TNCN khấu trừ</Text>
                <Text style={[typography.bodyBold, { color: colors.danger }]}>
                  {formatCurrency(payroll.thueTncn, activeLanguage)}
                </Text>
              </View>
              {(payroll.hoanThue ?? 0) > 0 && (
                <>
                  <AppDivider marginVertical={spacing.sm} />
                  <View style={styles.detailRow}>
                    <Text style={[typography.bodyBold, { color: colors.text }]}>Hoàn thuế TNCN</Text>
                    <Text style={[typography.bodyBold, { color: colors.success }]}>
                      +{formatCurrency(payroll.hoanThue, activeLanguage)}
                    </Text>
                  </View>
                </>
              )}
            </AppCard>

            {/* 6. Cách tính (Plain Vietnamese Explanation) */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              Cách tính lương & thuế
            </Text>
            <AppCard style={{ backgroundColor: colors.surfaceCard }}>
              <View style={{ marginBottom: spacing.sm }}>
                <Text style={[typography.captionBold, { color: colors.primary, marginBottom: 4 }]}>
                  CÔNG THỨC LƯƠNG
                </Text>
                <Text style={[typography.body, { color: colors.text }]}>
                  {payroll.cachTinhLuong || 'Lương thực tế = Đơn giá ngày × Số công thực tế'}
                </Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View>
                <Text style={[typography.captionBold, { color: colors.primary, marginBottom: 4 }]}>
                  CÔNG THỨC THUẾ TNCN
                </Text>
                <Text style={[typography.body, { color: colors.text }]}>
                  {payroll.cachTinhThue || 'Thuế TNCN tính theo biểu lũy tiến từng phần theo quy định của Luật Thuế TNCN'}
                </Text>
              </View>
            </AppCard>

            {/* 7. Nguồn & Chính sách áp dụng */}
            <Text style={[typography.h3, { color: colors.text, marginTop: spacing.xl, marginBottom: spacing.sm }]}>
              Nguồn & Chính sách áp dụng
            </Text>
            <AppCard>
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Phiên bản tính lương</Text>
                <AppBadge
                  label={payroll.payrollVersion || 'V2026.PROD'}
                  variant={payroll.isLegacy ? 'default' : 'info'}
                />
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Bộ chính sách</Text>
                <Text style={[typography.caption, { color: colors.text, flex: 1, textAlign: 'right' }]} numberOfLines={2}>
                  {payroll.chinhSachApDung || 'Quy chuẩn sản xuất 2026'}
                </Text>
              </View>
              <AppDivider marginVertical={spacing.sm} />
              <View style={styles.detailRow}>
                <Text style={[typography.body, { color: colors.textSecondary }]}>Phân loại dữ liệu</Text>
                <Text style={[typography.bodyBold, { color: payroll.isLegacy ? colors.warning : colors.success }]}>
                  {payroll.isLegacy ? 'Dữ liệu lịch sử (Legacy)' : 'Quy chuẩn sản xuất (Production)'}
                </Text>
              </View>
            </AppCard>
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
  highlightCard: {
    alignItems: 'center',
    paddingVertical: spacing.xxl,
  },
  statusBadgeRow: {
    marginTop: spacing.xs,
  },
  detailRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 2,
  },
});
