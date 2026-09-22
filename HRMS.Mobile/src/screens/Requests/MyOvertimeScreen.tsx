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
import { OvertimeRequestDto } from '../../types/me';
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

export const MyOvertimeScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [history, setHistory] = useState<OvertimeRequestDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Form states
  const [otDate, setOtDate] = useState<string>(() => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  });
  const [selectedIdCa, setSelectedIdCa] = useState<number>(1);
  const [shiftType, setShiftType] = useState<string>('Ca ngày');
  const [hours, setHours] = useState<string>('2');
  const [reason, setReason] = useState<string>('');
  const [cancellingId, setCancellingId] = useState<number | null>(null);

  const shiftOptions = [
    { label: t('requests.dayShift'), value: 'Ca ngày', idCa: 1 },
    { label: t('requests.nightShift'), value: 'Ca đêm', idCa: 2 },
  ];

  const fetchHistory = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    try {
      const data = await meApi.getOvertimeRequests();
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

    if (!otDate.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nhập ngày làm thêm giờ.');
      return;
    }

    const numHours = parseFloat(hours);
    if (isNaN(numHours) || numHours <= 0) {
      Alert.alert(t('common.error'), 'Số giờ làm thêm phải lớn hơn 0.');
      return;
    }

    if (numHours > 24) {
      Alert.alert(t('common.error'), 'Số giờ làm thêm không được vượt quá 24 giờ/ngày.');
      return;
    }

    if (!reason.trim()) {
      Alert.alert(t('common.error'), 'Vui lòng nêu rõ nội dung và lý do tăng ca.');
      return;
    }

    setSubmitting(true);
    try {
      await meApi.createOvertimeRequest({
        otDate: otDate.trim(),
        hours: numHours,
        idCa: selectedIdCa,
        shiftType,
        reason: reason.trim(),
      });

      Alert.alert(t('common.success'), 'Gửi đề xuất tăng ca thành công! Vui lòng chờ cấp quản lý xét duyệt.');
      setReason('');
      fetchHistory(true);
    } catch (error: any) {
      Alert.alert(t('common.error'), mapApiError(error));
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancelRequest = (reqId: number) => {
    Alert.alert(
      'Xác nhận hủy đề xuất',
      'Bạn có chắc chắn muốn hủy đề xuất tăng ca này không?',
      [
        { text: 'Đóng', style: 'cancel' },
        {
          text: 'Xác nhận hủy',
          style: 'destructive',
          onPress: async () => {
            setCancellingId(reqId);
            try {
              await meApi.cancelOvertimeRequest(reqId);
              Alert.alert(t('common.success'), 'Đã hủy đề xuất tăng ca thành công.');
              fetchHistory(true);
            } catch (error: any) {
              Alert.alert(t('common.error'), mapApiError(error));
            } finally {
              setCancellingId(null);
            }
          },
        },
      ]
    );
  };

  const getStatusVariant = (status: string) => {
    switch (status?.toUpperCase()) {
      case 'APPROVED':
        return 'success';
      case 'REJECTED':
        return 'danger';
      case 'CANCELLED':
        return 'default';
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
      case 'CANCELLED':
        return t('common.cancel');
      default:
        return t('requests.statusPending');
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader title={t('requests.overtimeTitle')} showBack onBack={() => navigation.goBack()} />

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
        {/* Form Register OT */}
        <AppCard style={styles.formCard}>
          <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.xs }]}>
            {t('requests.overtimeTitle')}
          </Text>
          <Text style={[typography.caption, { color: colors.textSecondary, marginBottom: spacing.md }]}>
            {t('requests.overtimeSubtitle')}
          </Text>

          <Text style={[typography.captionBold, { color: colors.text, marginBottom: spacing.xs }]}>
            {t('requests.shiftType')}
          </Text>
          <View style={styles.chipRow}>
            {shiftOptions.map((opt) => {
              const isSelected = selectedIdCa === opt.idCa;
              return (
                <TouchableOpacity
                  key={opt.idCa}
                  activeOpacity={0.7}
                  onPress={() => {
                    setSelectedIdCa(opt.idCa);
                    setShiftType(opt.value);
                  }}
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
                label={t('requests.workDate')}
                placeholder="YYYY-MM-DD"
                value={otDate}
                onChangeText={setOtDate}
                leftIcon="calendar-outline"
              />
            </View>
            <View style={{ width: spacing.md }} />
            <View style={{ flex: 1 }}>
              <AppTextInput
                label={t('requests.hours')}
                placeholder="2.0"
                value={hours}
                onChangeText={setHours}
                keyboardType="numeric"
                leftIcon="time-outline"
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
          history.map((item) => {
            const reqId = item.id || (item as any).idYeuCau;
            const isPending = item.status === 'PENDING';
            const isRejected = item.status === 'REJECTED';
            const isApproved = item.status === 'APPROVED';
            const rejectReason = item.note || (item as any).lyDoTuChoi;

            return (
              <AppCard key={reqId} style={styles.historyCard}>
                <View style={styles.historyHeader}>
                  <View style={styles.historyTypeWrapper}>
                    <Ionicons name="flash-outline" size={18} color="#F59E0B" />
                    <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.xs }]}>
                      {formatDate(item.otDate || (item as any).ngayTangCa, activeLanguage)} • {item.hours || (item as any).soGio}h ({item.shiftType || (item as any).tenCa || 'Ca ngày'})
                    </Text>
                  </View>
                  <AppBadge label={getStatusLabel(item.status)} variant={getStatusVariant(item.status)} />
                </View>

                <Text style={[typography.body, { color: colors.text, marginTop: spacing.xs }]}>
                  {item.reason || (item as any).noiDung}
                </Text>

                {isRejected && rejectReason ? (
                  <View style={[styles.noteBox, { backgroundColor: 'rgba(239, 68, 68, 0.08)', borderColor: '#EF4444' }]}>
                    <Text style={[typography.captionBold, { color: '#EF4444' }]}>
                      Lý do từ chối: {rejectReason}
                    </Text>
                  </View>
                ) : null}

                {isApproved && item.nguoiDuyet ? (
                  <View style={[styles.noteBox, { backgroundColor: 'rgba(16, 185, 129, 0.08)', borderColor: '#10B981' }]}>
                    <Text style={[typography.captionBold, { color: '#10B981' }]}>
                      Đã duyệt bởi: {item.nguoiDuyet} {item.ngayDuyet ? `• ${item.ngayDuyet}` : ''}
                    </Text>
                  </View>
                ) : null}

                {isPending ? (
                  <View style={{ marginTop: spacing.md, alignItems: 'flex-end' }}>
                    <TouchableOpacity
                      activeOpacity={0.7}
                      onPress={() => handleCancelRequest(reqId)}
                      disabled={cancellingId === reqId}
                      style={[styles.cancelBtn, { borderColor: colors.border }]}
                    >
                      <Ionicons name="close-circle-outline" size={16} color={colors.textSecondary} />
                      <Text style={[typography.captionBold, { color: colors.textSecondary, marginLeft: 4 }]}>
                        {cancellingId === reqId ? 'Đang hủy...' : 'Hủy đề xuất'}
                      </Text>
                    </TouchableOpacity>
                  </View>
                ) : null}
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
  noteBox: {
    marginTop: spacing.sm,
    padding: spacing.sm,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
  },
  cancelBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.sm,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
  },
});
