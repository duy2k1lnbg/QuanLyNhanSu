import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  TextInput,
  Modal,
  Alert,
  ActivityIndicator,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useTheme } from '../../hooks/useTheme';
import { approvalsApi } from '../../api/approvalsApi';
import {
  ApprovalSummaryDto,
  LeaveApprovalItemDto,
  AttendanceCorrectionApprovalItemDto,
  OvertimeApprovalItemDto,
  InsuranceMovementApprovalItemDto,
} from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppButton } from '../../components/AppButton';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';
import { mapApiError } from '../../utils/errorMapper';

type ApprovalTab = 'LEAVE' | 'CORRECTION' | 'OVERTIME' | 'INSURANCE';
type StatusFilter = 'ALL' | 'PENDING' | 'APPROVED' | 'REJECTED';

export const ManagerApprovalsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { colors } = useTheme();

  const [activeTab, setActiveTab] = useState<ApprovalTab>('LEAVE');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('PENDING');
  const [searchQuery, setSearchQuery] = useState('');

  const [summary, setSummary] = useState<ApprovalSummaryDto>({
    totalPending: 0,
    leavePending: 0,
    attendancePending: 0,
    overtimePending: 0,
  });

  const [leaves, setLeaves] = useState<LeaveApprovalItemDto[]>([]);
  const [corrections, setCorrections] = useState<AttendanceCorrectionApprovalItemDto[]>([]);
  const [overtimes, setOvertimes] = useState<OvertimeApprovalItemDto[]>([]);
  const [insuranceMovements, setInsuranceMovements] = useState<InsuranceMovementApprovalItemDto[]>([]);

  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [submittingId, setSubmittingId] = useState<number | null>(null);

  // Reject Modal State
  const [rejectModalVisible, setRejectModalVisible] = useState(false);
  const [selectedItemForReject, setSelectedItemForReject] = useState<{ id: number; type: ApprovalTab } | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  // Detail Modal State
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [selectedDetail, setSelectedDetail] = useState<any>(null);

  const fetchSummary = useCallback(async () => {
    try {
      const sum = await approvalsApi.getSummary();
      setSummary(sum);
    } catch {
      // ignore
    }
  }, []);

  const fetchData = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    try {
      await fetchSummary();
      const filter = {
        status: statusFilter === 'ALL' ? undefined : statusFilter,
        search: searchQuery.trim() || undefined,
        pageSize: 50,
      };

      if (activeTab === 'LEAVE') {
        const res = await approvalsApi.getLeaveList(filter);
        setLeaves(res.data || []);
      } else if (activeTab === 'CORRECTION') {
        const res = await approvalsApi.getAttendanceCorrections(filter);
        setCorrections(res.data || []);
      } else if (activeTab === 'OVERTIME') {
        const res = await approvalsApi.getOvertimeList(filter);
        setOvertimes(res.data || []);
      } else if (activeTab === 'INSURANCE') {
        const res = await approvalsApi.getInsuranceMovements(filter);
        setInsuranceMovements(res.data || []);
      }
    } catch (err) {
      Alert.alert('Lỗi', mapApiError(err));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [activeTab, statusFilter, searchQuery, fetchSummary]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Handle Approve
  const handleApprove = (id: number, type: ApprovalTab, employeeName: string) => {
    Alert.alert(
      'Xác nhận phê duyệt',
      `Bạn có chắc chắn muốn phê duyệt yêu cầu của nhân viên ${employeeName}?`,
      [
        { text: 'Hủy', style: 'cancel' },
        {
          text: 'Phê duyệt',
          style: 'default',
          onPress: async () => {
            if (submittingId !== null) return;
            setSubmittingId(id);
            try {
              if (type === 'LEAVE') {
                await approvalsApi.approveLeave(id);
              } else if (type === 'CORRECTION') {
                await approvalsApi.approveAttendanceCorrection(id);
              } else if (type === 'OVERTIME') {
                await approvalsApi.approveOvertime(id);
              } else if (type === 'INSURANCE') {
                await approvalsApi.approveInsuranceMovement(id);
              }
              Alert.alert('Thành công', 'Đã phê duyệt yêu cầu thành công!');
              fetchData(true);
            } catch (err) {
              Alert.alert('Thao tác thất bại', mapApiError(err));
            } finally {
              setSubmittingId(null);
            }
          },
        },
      ]
    );
  };

  // Open Reject Modal
  const openRejectModal = (id: number, type: ApprovalTab) => {
    setSelectedItemForReject({ id, type });
    setRejectReason('');
    setRejectModalVisible(true);
  };

  // Confirm Reject
  const handleConfirmReject = async () => {
    if (!selectedItemForReject) return;
    if (!rejectReason.trim()) {
      Alert.alert('Thiếu thông tin', 'Vui lòng nhập lý do từ chối.');
      return;
    }

    setSubmittingId(selectedItemForReject.id);
    try {
      if (selectedItemForReject.type === 'LEAVE') {
        await approvalsApi.rejectLeave(selectedItemForReject.id, rejectReason.trim());
      } else if (selectedItemForReject.type === 'CORRECTION') {
        await approvalsApi.rejectAttendanceCorrection(selectedItemForReject.id, rejectReason.trim());
      } else if (selectedItemForReject.type === 'OVERTIME') {
        await approvalsApi.rejectOvertime(selectedItemForReject.id, rejectReason.trim());
      } else if (selectedItemForReject.type === 'INSURANCE') {
        await approvalsApi.rejectInsuranceMovement(selectedItemForReject.id, rejectReason.trim());
      }
      setRejectModalVisible(false);
      setSelectedItemForReject(null);
      Alert.alert('Thành công', 'Đã từ chối yêu cầu.');
      fetchData(true);
    } catch (err) {
      Alert.alert('Thao tác thất bại', mapApiError(err));
    } finally {
      setSubmittingId(null);
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
        return 'ĐÃ DUYỆT';
      case 'REJECTED':
        return 'TỪ CHỐI';
      default:
        return 'CHỜ DUYỆT';
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader title="Phê duyệt yêu cầu" showBack onBack={() => navigation.goBack()} />

      {/* 1. Summary Cards Header */}
      <View style={[styles.summaryRow, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
        <View style={styles.summaryItem}>
          <Text style={[typography.caption, { color: colors.textSecondary }]}>Chờ duyệt</Text>
          <Text style={[typography.h2, { color: colors.warning }]}>{summary.totalPending}</Text>
        </View>
        <View style={[styles.dividerVertical, { backgroundColor: colors.border }]} />
        <View style={styles.summaryItem}>
          <Text style={[typography.caption, { color: colors.textSecondary }]}>Nghỉ phép</Text>
          <Text style={[typography.h3, { color: colors.primary }]}>{summary.leavePending}</Text>
        </View>
        <View style={[styles.dividerVertical, { backgroundColor: colors.border }]} />
        <View style={styles.summaryItem}>
          <Text style={[typography.caption, { color: colors.textSecondary }]}>Sửa công</Text>
          <Text style={[typography.h3, { color: colors.info }]}>{summary.attendancePending}</Text>
        </View>
        <View style={[styles.dividerVertical, { backgroundColor: colors.border }]} />
        <View style={styles.summaryItem}>
          <Text style={[typography.caption, { color: colors.textSecondary }]}>Tăng ca</Text>
          <Text style={[typography.h3, { color: colors.text }]}>{summary.overtimePending}</Text>
        </View>
        <View style={[styles.dividerVertical, { backgroundColor: colors.border }]} />
        <View style={styles.summaryItem}>
          <Text style={[typography.caption, { color: colors.textSecondary }]}>BHXH</Text>
          <Text style={[typography.h3, { color: '#0ea5e9' }]}>{summary.insurancePending ?? 0}</Text>
        </View>
      </View>

      {/* 2. Type Tabs */}
      <View style={[styles.tabBar, { backgroundColor: colors.surfaceCard, borderBottomColor: colors.border }]}>
        <TouchableOpacity
          style={[styles.tabItem, activeTab === 'LEAVE' && { borderBottomColor: colors.primary, borderBottomWidth: 3 }]}
          onPress={() => setActiveTab('LEAVE')}
        >
          <Text
            style={[
              typography.captionBold,
              { color: activeTab === 'LEAVE' ? colors.primary : colors.textSecondary },
            ]}
          >
            Nghỉ phép ({summary.leavePending})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[
            styles.tabItem,
            activeTab === 'CORRECTION' && { borderBottomColor: colors.primary, borderBottomWidth: 3 },
          ]}
          onPress={() => setActiveTab('CORRECTION')}
        >
          <Text
            style={[
              typography.captionBold,
              { color: activeTab === 'CORRECTION' ? colors.primary : colors.textSecondary },
            ]}
          >
            Sửa công ({summary.attendancePending})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[
            styles.tabItem,
            activeTab === 'OVERTIME' && { borderBottomColor: colors.primary, borderBottomWidth: 3 },
          ]}
          onPress={() => setActiveTab('OVERTIME')}
        >
          <Text
            style={[
              typography.captionBold,
              { color: activeTab === 'OVERTIME' ? colors.primary : colors.textSecondary },
            ]}
          >
            Tăng ca ({summary.overtimePending})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[
            styles.tabItem,
            activeTab === 'INSURANCE' && { borderBottomColor: colors.primary, borderBottomWidth: 3 },
          ]}
          onPress={() => setActiveTab('INSURANCE')}
        >
          <Text
            style={[
              typography.captionBold,
              { color: activeTab === 'INSURANCE' ? colors.primary : colors.textSecondary },
            ]}
          >
            BHXH ({summary.insurancePending ?? 0})
          </Text>
        </TouchableOpacity>
      </View>

      {/* 3. Filter & Search Controls */}
      <View style={styles.filterSection}>
        {/* Search Input */}
        <View style={[styles.searchBox, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
          <Ionicons name="search-outline" size={18} color={colors.textMuted} />
          <TextInput
            placeholder="Tìm theo họ tên hoặc mã nhân viên..."
            placeholderTextColor={colors.textMuted}
            value={searchQuery}
            onChangeText={setSearchQuery}
            style={[styles.searchInput, { color: colors.text }]}
          />
          {searchQuery.length > 0 && (
            <TouchableOpacity onPress={() => setSearchQuery('')}>
              <Ionicons name="close-circle" size={18} color={colors.textMuted} />
            </TouchableOpacity>
          )}
        </View>

        {/* Status Chips */}
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.statusChipsRow}>
          {(['PENDING', 'APPROVED', 'REJECTED', 'ALL'] as StatusFilter[]).map((st) => (
            <TouchableOpacity
              key={st}
              style={[
                styles.chip,
                {
                  backgroundColor: statusFilter === st ? colors.primary : colors.surfaceCard,
                  borderColor: statusFilter === st ? colors.primary : colors.border,
                },
              ]}
              onPress={() => setStatusFilter(st)}
            >
              <Text
                style={[
                  typography.captionBold,
                  { color: statusFilter === st ? '#FFFFFF' : colors.textSecondary },
                ]}
              >
                {st === 'PENDING'
                  ? 'Chờ duyệt'
                  : st === 'APPROVED'
                  ? 'Đã duyệt'
                  : st === 'REJECTED'
                  ? 'Đã từ chối'
                  : 'Tất cả'}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
      </View>

      {/* 4. Requests List */}
      <ScrollView
        contentContainerStyle={styles.listContent}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchData(true)}
            colors={[colors.primary]}
            tintColor={colors.primary}
          />
        }
      >
        {loading && !refreshing ? (
          <AppLoading />
        ) : (
          <>
            {/* TAB LEAVE */}
            {activeTab === 'LEAVE' && (
              <>
                {leaves.length === 0 ? (
                  <AppEmptyState icon="calendar-outline" title="Không có đơn xin nghỉ phép nào" />
                ) : (
                  leaves.map((item) => (
                    <AppCard
                      key={item.id}
                      style={styles.requestCard}
                      onPress={() => {
                        setSelectedDetail({ ...item, _type: 'LEAVE' });
                        setDetailModalVisible(true);
                      }}
                    >
                      <View style={styles.cardHeader}>
                        <View style={styles.empInfo}>
                          <Text style={[typography.bodyBold, { color: colors.text }]}>
                            {item.employeeName}
                          </Text>
                          <Text style={[typography.caption, { color: colors.textMuted }]}>
                            {item.employeeCode || `NV#${item.manv}`} • {item.departmentName}
                          </Text>
                        </View>
                        <AppBadge label={getStatusLabel(item.trangThai)} variant={getStatusVariant(item.trangThai)} />
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="time-outline" size={16} color={colors.primary} />
                        <Text style={[typography.captionBold, { color: colors.text, marginLeft: 6 }]}>
                          {item.tuNgay} {item.tuNgay !== item.denNgay ? `đến ${item.denNgay}` : ''} ({item.soNgay} ngày)
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="pricetag-outline" size={16} color={colors.info} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]}>
                          Loại nghỉ: {item.loaiNghi}
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="document-text-outline" size={16} color={colors.textMuted} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]} numberOfLines={2}>
                          Lý do: {item.lyDo || 'Không có'}
                        </Text>
                      </View>

                      {item.trangThai === 'REJECTED' && item.lyDoTuChoi && (
                        <View style={[styles.rejectReasonBox, { backgroundColor: 'rgba(239, 68, 68, 0.08)' }]}>
                          <Text style={[typography.caption, { color: colors.danger }]}>
                            Lý do từ chối: {item.lyDoTuChoi}
                          </Text>
                        </View>
                      )}

                      {item.trangThai === 'PENDING' && (
                        <View style={styles.actionButtonsRow}>
                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.danger }]}
                            onPress={() => openRejectModal(item.id, 'LEAVE')}
                            disabled={submittingId === item.id}
                          >
                            <Ionicons name="close-circle-outline" size={16} color="#FFF" />
                            <Text style={styles.actionBtnText}>Từ chối</Text>
                          </TouchableOpacity>

                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.success }]}
                            onPress={() => handleApprove(item.id, 'LEAVE', item.employeeName)}
                            disabled={submittingId === item.id}
                          >
                            {submittingId === item.id ? (
                              <ActivityIndicator size="small" color="#FFF" />
                            ) : (
                              <>
                                <Ionicons name="checkmark-circle-outline" size={16} color="#FFF" />
                                <Text style={styles.actionBtnText}>Phê duyệt</Text>
                              </>
                            )}
                          </TouchableOpacity>
                        </View>
                      )}
                    </AppCard>
                  ))
                )}
              </>
            )}

            {/* TAB CORRECTION */}
            {activeTab === 'CORRECTION' && (
              <>
                {corrections.length === 0 ? (
                  <AppEmptyState icon="time-outline" title="Không có yêu cầu điều chỉnh công nào" />
                ) : (
                  corrections.map((item) => (
                    <AppCard
                      key={item.id}
                      style={styles.requestCard}
                      onPress={() => {
                        setSelectedDetail({ ...item, _type: 'CORRECTION' });
                        setDetailModalVisible(true);
                      }}
                    >
                      <View style={styles.cardHeader}>
                        <View style={styles.empInfo}>
                          <Text style={[typography.bodyBold, { color: colors.text }]}>
                            {item.employeeName}
                          </Text>
                          <Text style={[typography.caption, { color: colors.textMuted }]}>
                            {item.employeeCode || `NV#${item.manv}`} • {item.departmentName}
                          </Text>
                        </View>
                        <AppBadge label={getStatusLabel(item.trangThai)} variant={getStatusVariant(item.trangThai)} />
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="calendar-outline" size={16} color={colors.primary} />
                        <Text style={[typography.captionBold, { color: colors.text, marginLeft: 6 }]}>
                          Ngày công: {item.ngayCong}
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="log-in-outline" size={16} color={colors.info} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]}>
                          Giờ đề xuất: Vào {item.gioVaoMoi || '--:--'} — Ra {item.gioRaMoi || '--:--'}
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="document-text-outline" size={16} color={colors.textMuted} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]} numberOfLines={2}>
                          Lý do: {item.lyDo || 'Không có'}
                        </Text>
                      </View>

                      {item.trangThai === 'REJECTED' && item.lyDoTuChoi && (
                        <View style={[styles.rejectReasonBox, { backgroundColor: 'rgba(239, 68, 68, 0.08)' }]}>
                          <Text style={[typography.caption, { color: colors.danger }]}>
                            Lý do từ chối: {item.lyDoTuChoi}
                          </Text>
                        </View>
                      )}

                      {item.trangThai === 'PENDING' && (
                        <View style={styles.actionButtonsRow}>
                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.danger }]}
                            onPress={() => openRejectModal(item.id, 'CORRECTION')}
                            disabled={submittingId === item.id}
                          >
                            <Ionicons name="close-circle-outline" size={16} color="#FFF" />
                            <Text style={styles.actionBtnText}>Từ chối</Text>
                          </TouchableOpacity>

                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.success }]}
                            onPress={() => handleApprove(item.id, 'CORRECTION', item.employeeName)}
                            disabled={submittingId === item.id}
                          >
                            {submittingId === item.id ? (
                              <ActivityIndicator size="small" color="#FFF" />
                            ) : (
                              <>
                                <Ionicons name="checkmark-circle-outline" size={16} color="#FFF" />
                                <Text style={styles.actionBtnText}>Phê duyệt</Text>
                              </>
                            )}
                          </TouchableOpacity>
                        </View>
                      )}
                    </AppCard>
                  ))
                )}
              </>
            )}

            {/* TAB OVERTIME */}
            {activeTab === 'OVERTIME' && (
              <>
                {overtimes.length === 0 ? (
                  <AppEmptyState icon="flash-outline" title="Không có đề xuất tăng ca nào" />
                ) : (
                  overtimes.map((item) => (
                    <AppCard
                      key={item.id}
                      style={styles.requestCard}
                      onPress={() => {
                        setSelectedDetail({ ...item, _type: 'OVERTIME' });
                        setDetailModalVisible(true);
                      }}
                    >
                      <View style={styles.cardHeader}>
                        <View style={styles.empInfo}>
                          <Text style={[typography.bodyBold, { color: colors.text }]}>
                            {item.employeeName}
                          </Text>
                          <Text style={[typography.caption, { color: colors.textMuted }]}>
                            {item.employeeCode || `NV#${item.manv}`} • {item.departmentName}
                          </Text>
                        </View>
                        <AppBadge label={getStatusLabel(item.trangThai)} variant={getStatusVariant(item.trangThai)} />
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="calendar-outline" size={16} color={colors.primary} />
                        <Text style={[typography.captionBold, { color: colors.text, marginLeft: 6 }]}>
                          Ngày: {item.ngayTangCa} • {item.soGio} giờ (Hệ số: {item.heSo})
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="business-outline" size={16} color={colors.info} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]}>
                          Ca làm: {item.tenCa}
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="document-text-outline" size={16} color={colors.textMuted} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]} numberOfLines={2}>
                          Nội dung: {item.noiDung || 'Không có'}
                        </Text>
                      </View>

                      {item.trangThai === 'REJECTED' && item.lyDoTuChoi && (
                        <View style={[styles.rejectReasonBox, { backgroundColor: 'rgba(239, 68, 68, 0.08)' }]}>
                          <Text style={[typography.caption, { color: colors.danger }]}>
                            Lý do từ chối: {item.lyDoTuChoi}
                          </Text>
                        </View>
                      )}

                      {item.trangThai === 'PENDING' && (
                        <View style={styles.actionButtonsRow}>
                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.danger }]}
                            onPress={() => openRejectModal(item.id, 'OVERTIME')}
                            disabled={submittingId === item.id}
                          >
                            <Ionicons name="close-circle-outline" size={16} color="#FFF" />
                            <Text style={styles.actionBtnText}>Từ chối</Text>
                          </TouchableOpacity>

                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.success }]}
                            onPress={() => handleApprove(item.id, 'OVERTIME', item.employeeName)}
                            disabled={submittingId === item.id}
                          >
                            {submittingId === item.id ? (
                              <ActivityIndicator size="small" color="#FFF" />
                            ) : (
                              <>
                                <Ionicons name="checkmark-circle-outline" size={16} color="#FFF" />
                                <Text style={styles.actionBtnText}>Phê duyệt</Text>
                              </>
                            )}
                          </TouchableOpacity>
                        </View>
                      )}
                    </AppCard>
                  ))
                )}
              </>
            )}

            {/* TAB INSURANCE */}
            {activeTab === 'INSURANCE' && (
              <>
                {insuranceMovements.length === 0 ? (
                  <AppEmptyState icon="shield-checkmark-outline" title="Không có biến động BHXH nào" />
                ) : (
                  insuranceMovements.map((item) => (
                    <AppCard
                      key={item.id}
                      style={styles.requestCard}
                      onPress={() => {
                        setSelectedDetail({ ...item, _type: 'INSURANCE' });
                        setDetailModalVisible(true);
                      }}
                    >
                      <View style={styles.cardHeader}>
                        <View style={styles.empInfo}>
                          <Text style={[typography.bodyBold, { color: colors.text }]}>
                            {item.employeeName}
                          </Text>
                          <Text style={[typography.caption, { color: colors.textMuted }]}>
                            {item.employeeCode || `NV#${item.manv}`} • {item.departmentName}
                          </Text>
                        </View>
                        <AppBadge label={getStatusLabel(item.trangThai)} variant={getStatusVariant(item.trangThai)} />
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="calendar-outline" size={16} color={colors.primary} />
                        <Text style={[typography.captionBold, { color: colors.text, marginLeft: 6 }]}>
                          Hiệu lực: {item.ngayHieuLuc} • Kỳ công: {item.maKyCong}
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="swap-horizontal-outline" size={16} color={colors.info} />
                        <Text style={[typography.captionBold, { color: colors.primary, marginLeft: 6 }]}>
                          Loại: {item.loai === 'TANG' ? 'Tăng mới' : item.loai === 'GIAM' ? 'Giảm lao động' : item.loai === 'DIEU_CHINH' ? 'Điều chỉnh' : 'Tạm dừng'}
                        </Text>
                      </View>

                      <View style={styles.detailRow}>
                        <Ionicons name="document-text-outline" size={16} color={colors.textMuted} />
                        <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 6 }]} numberOfLines={2}>
                          Lý do: {item.lyDo || 'Không có'}
                        </Text>
                      </View>

                      {(item.trangThai === 'PENDING' || item.trangThai === 'DRAFT') && (
                        <View style={styles.actionButtonsRow}>
                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.danger }]}
                            onPress={() => openRejectModal(item.id, 'INSURANCE')}
                            disabled={submittingId === item.id}
                          >
                            <Ionicons name="close-circle-outline" size={16} color="#FFF" />
                            <Text style={styles.actionBtnText}>Từ chối</Text>
                          </TouchableOpacity>

                          <TouchableOpacity
                            style={[styles.actionBtn, { backgroundColor: colors.success }]}
                            onPress={() => handleApprove(item.id, 'INSURANCE', item.employeeName)}
                            disabled={submittingId === item.id}
                          >
                            {submittingId === item.id ? (
                              <ActivityIndicator size="small" color="#FFF" />
                            ) : (
                              <>
                                <Ionicons name="checkmark-circle-outline" size={16} color="#FFF" />
                                <Text style={styles.actionBtnText}>Phê duyệt</Text>
                              </>
                            )}
                          </TouchableOpacity>
                        </View>
                      )}
                    </AppCard>
                  ))
                )}
              </>
            )}
          </>
        )}
      </ScrollView>

      {/* Reject Reason Modal */}
      <Modal
        visible={rejectModalVisible}
        transparent
        animationType="fade"
        onRequestClose={() => setRejectModalVisible(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={[styles.modalCard, { backgroundColor: colors.surfaceCard }]}>
            <Text style={[typography.h3, { color: colors.danger, marginBottom: spacing.sm }]}>
              Từ chối yêu cầu
            </Text>
            <Text style={[typography.caption, { color: colors.textSecondary, marginBottom: spacing.md }]}>
              Vui lòng nhập lý do từ chối để nhân viên nắm được thông tin.
            </Text>

            <TextInput
              style={[
                styles.modalTextInput,
                { color: colors.text, borderColor: colors.border, backgroundColor: colors.background },
              ]}
              multiline
              numberOfLines={4}
              placeholder="Nhập lý do từ chối..."
              placeholderTextColor={colors.textMuted}
              value={rejectReason}
              onChangeText={setRejectReason}
            />

            <View style={styles.modalButtonsRow}>
              <TouchableOpacity
                style={[styles.modalBtn, { borderColor: colors.border, borderWidth: 1 }]}
                onPress={() => setRejectModalVisible(false)}
              >
                <Text style={[typography.bodyBold, { color: colors.textSecondary }]}>Hủy</Text>
              </TouchableOpacity>

              <TouchableOpacity
                style={[styles.modalBtn, { backgroundColor: colors.danger }]}
                onPress={handleConfirmReject}
              >
                <Text style={[typography.bodyBold, { color: '#FFF' }]}>Xác nhận từ chối</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>

      {/* Detail Inspection Modal */}
      <Modal
        visible={detailModalVisible}
        transparent
        animationType="slide"
        onRequestClose={() => setDetailModalVisible(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={[styles.modalCard, { backgroundColor: colors.surfaceCard, maxHeight: '80%' }]}>
            <View style={styles.modalHeaderRow}>
              <Text style={[typography.h3, { color: colors.text }]}>Chi tiết yêu cầu</Text>
              <TouchableOpacity onPress={() => setDetailModalVisible(false)}>
                <Ionicons name="close" size={24} color={colors.text} />
              </TouchableOpacity>
            </View>

            <ScrollView style={{ marginTop: spacing.md }}>
              {selectedDetail && (
                <>
                  <View style={styles.detailItem}>
                    <Text style={[typography.caption, { color: colors.textMuted }]}>Nhân viên:</Text>
                    <Text style={[typography.bodyBold, { color: colors.text }]}>
                      {selectedDetail.employeeName} ({selectedDetail.employeeCode || `NV#${selectedDetail.manv}`})
                    </Text>
                  </View>
                  <View style={styles.detailItem}>
                    <Text style={[typography.caption, { color: colors.textMuted }]}>Phòng ban:</Text>
                    <Text style={[typography.body, { color: colors.text }]}>
                      {selectedDetail.departmentName}
                    </Text>
                  </View>
                  <View style={styles.detailItem}>
                    <Text style={[typography.caption, { color: colors.textMuted }]}>Trạng thái:</Text>
                    <AppBadge
                      label={getStatusLabel(selectedDetail.trangThai)}
                      variant={getStatusVariant(selectedDetail.trangThai)}
                    />
                  </View>
                  <View style={styles.detailItem}>
                    <Text style={[typography.caption, { color: colors.textMuted }]}>Ngày tạo đơn:</Text>
                    <Text style={[typography.body, { color: colors.text }]}>
                      {selectedDetail.ngayTao || '---'}
                    </Text>
                  </View>
                  {selectedDetail.nguoiDuyet && (
                    <View style={styles.detailItem}>
                      <Text style={[typography.caption, { color: colors.textMuted }]}>Người duyệt:</Text>
                      <Text style={[typography.body, { color: colors.text }]}>
                        {selectedDetail.nguoiDuyet} ({selectedDetail.ngayDuyet || ''})
                      </Text>
                    </View>
                  )}
                  {selectedDetail.lyDoTuChoi && (
                    <View style={styles.detailItem}>
                      <Text style={[typography.caption, { color: colors.danger }]}>Lý do từ chối:</Text>
                      <Text style={[typography.body, { color: colors.danger }]}>
                        {selectedDetail.lyDoTuChoi}
                      </Text>
                    </View>
                  )}
                </>
              )}
            </ScrollView>
          </View>
        </View>
      </Modal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  summaryRow: {
    flexDirection: 'row',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.md,
    borderBottomWidth: 1,
    alignItems: 'center',
    justifyContent: 'space-around',
  },
  summaryItem: {
    alignItems: 'center',
    flex: 1,
  },
  dividerVertical: {
    width: 1,
    height: 32,
  },
  tabBar: {
    flexDirection: 'row',
    borderBottomWidth: 1,
  },
  tabItem: {
    flex: 1,
    paddingVertical: spacing.md,
    alignItems: 'center',
    justifyContent: 'center',
  },
  filterSection: {
    paddingHorizontal: spacing.md,
    paddingTop: spacing.sm,
    paddingBottom: spacing.xs,
  },
  searchBox: {
    flexDirection: 'row',
    alignItems: 'center',
    borderRadius: 8,
    borderWidth: 1,
    paddingHorizontal: spacing.sm,
    height: 40,
    marginBottom: spacing.xs,
  },
  searchInput: {
    flex: 1,
    marginLeft: spacing.xs,
    fontSize: 14,
  },
  statusChipsRow: {
    paddingVertical: spacing.xs,
    gap: spacing.xs,
  },
  chip: {
    paddingHorizontal: spacing.md,
    paddingVertical: 6,
    borderRadius: 16,
    borderWidth: 1,
    marginRight: spacing.xs,
  },
  listContent: {
    padding: spacing.md,
    paddingBottom: spacing.xxl,
  },
  requestCard: {
    marginBottom: spacing.md,
    padding: spacing.md,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: spacing.sm,
  },
  empInfo: {
    flex: 1,
    marginRight: spacing.sm,
  },
  detailRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 4,
  },
  rejectReasonBox: {
    padding: spacing.sm,
    borderRadius: 6,
    marginTop: spacing.sm,
  },
  actionButtonsRow: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: spacing.sm,
    marginTop: spacing.md,
    paddingTop: spacing.sm,
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: '#E5E7EB',
  },
  actionBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: spacing.md,
    paddingVertical: 8,
    borderRadius: 6,
    gap: 4,
  },
  actionBtnText: {
    color: '#FFFFFF',
    fontWeight: 'bold',
    fontSize: 13,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.lg,
  },
  modalCard: {
    width: '100%',
    borderRadius: 12,
    padding: spacing.lg,
  },
  modalHeaderRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  modalTextInput: {
    borderWidth: 1,
    borderRadius: 8,
    padding: spacing.sm,
    height: 100,
    textAlignVertical: 'top',
    marginBottom: spacing.md,
  },
  modalButtonsRow: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: spacing.sm,
  },
  modalBtn: {
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.sm,
    borderRadius: 6,
  },
  detailItem: {
    marginBottom: spacing.sm,
  },
});
