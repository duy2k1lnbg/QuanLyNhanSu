import { useState, useEffect } from 'react';
import {
  Card,
  Tabs,
  Table,
  Tag,
  Space,
  Button,
  Input,
  Select,
  Row,
  Col,
  Modal,
  notification,
  Typography,
  Image,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  SearchOutlined,
  ReloadOutlined,
  ScheduleOutlined,
  CalendarOutlined,
  FieldTimeOutlined,
  SafetyCertificateOutlined,
  DollarOutlined,
  UserOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import { PageHeader } from '../theme/components/PageHeader';
import { MetricCard } from '../theme/components/MetricCard';
import { canEdit } from '../utils/permissionUtils';
import type { CurrentUserDTO } from '../types/hrms';

const { Text } = Typography;

interface LeaveItem {
  id: number;
  manv: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  loaiNghi: string;
  tuNgay: string;
  denNgay: string;
  soNgay: number;
  lyDo: string;
  trangThai: string;
  ngayTao: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
  lyDoTuChoi?: string;
}

interface AttendanceCorrectionItem {
  id: number;
  manv: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  ngayCong: string;
  gioVaoMoi: string;
  gioRaMoi: string;
  lyDo: string;
  trangThai: string;
  ngayTao: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
  lyDoTuChoi?: string;
}

interface OvertimeItem {
  id: number;
  manv: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  ngayTangCa: string;
  soGio: number;
  heSo: number;
  noiDung: string;
  trangThai: string;
  ngayTao: string;
  nguoiDuyet?: string;
  ngayDuyet?: string;
  lyDoTuChoi?: string;
}

interface InsuranceMovementItem {
  id: number;
  manv: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  maKyCong: number;
  loai: string;
  ngayHieuLuc: string;
  lyDo: string;
  trangThai: string;
  nguoiDuyet?: string;
}

interface AdvanceItem {
  id: number;
  manv: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  amount: number;
  advanceDate: string;
  thang: number;
  nam: number;
  reason: string;
  status: string;
  approverName?: string;
  approvedDate?: string;
  rejectReason?: string;
  disbursedDate?: string;
  voucherNo?: string;
  disbursedByName?: string;
  disburseNote?: string;
  idUlLink?: number;
  createdAt: string;
}

interface ProfileChangeItem {
  id: number;
  manv: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  groupCode: string;
  reqType: string;
  currentDataJson?: string;
  proposedDataJson?: string;
  hasProposedAvatarBlob?: boolean;
  status: string;
  reason: string;
  rejectReason?: string;
  resolvedByName?: string;
  resolvedAt?: string;
  appliedAt?: string;
  createdAt: string;
}

interface SummaryData {
  totalPending: number;
  leavePending: number;
  attendancePending: number;
  overtimePending: number;
  insurancePending?: number;
  advancePending?: number;
  profilePending?: number;
}

interface ApprovalCenterPageProps {
  currentUser?: CurrentUserDTO | null;
}

export function ApprovalCenterPage({ currentUser: propUser }: ApprovalCenterPageProps = {}) {
  const { t } = useAppLanguage();
  const { tokens } = useAppTheme();

  const currentUser = propUser || (() => {
    try {
      const saved = localStorage.getItem('hrms_user');
      return saved ? JSON.parse(saved) : null;
    } catch {
      return null;
    }
  })();

  const canApproveGeneral = canEdit(currentUser, 'F_NV_PHEDUYET');
  const canApproveAdvance = canApproveGeneral || canEdit(currentUser, 'F_APPROVE_ADVANCE');
  const canDisburseAdvance = canEdit(currentUser, 'F_PAYROLL_DISBURSE_ADVANCE');
  const canApproveProfile = canApproveGeneral || canEdit(currentUser, 'F_APPROVE_PROFILE');

  const [activeTab, setActiveTab] = useState<string>('leave');
  const [statusFilter, setStatusFilter] = useState<string>('PENDING');
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [profileGroupFilter, setProfileGroupFilter] = useState<string>('ALL');
  const [summary, setSummary] = useState<SummaryData>({
    totalPending: 0,
    leavePending: 0,
    attendancePending: 0,
    overtimePending: 0,
    insurancePending: 0,
    advancePending: 0,
    profilePending: 0,
  });

  // Lists
  const [leaveList, setLeaveList] = useState<LeaveItem[]>([]);
  const [attendanceList, setAttendanceList] = useState<AttendanceCorrectionItem[]>([]);
  const [overtimeList, setOvertimeList] = useState<OvertimeItem[]>([]);
  const [insuranceList, setInsuranceList] = useState<InsuranceMovementItem[]>([]);
  const [advanceList, setAdvanceList] = useState<AdvanceItem[]>([]);
  const [profileList, setProfileList] = useState<ProfileChangeItem[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // Reject Modal
  const [rejectModalVisible, setRejectModalVisible] = useState<boolean>(false);
  const [rejectTarget, setRejectTarget] = useState<{
    type: 'leave' | 'attendance' | 'overtime' | 'insurance' | 'advance' | 'profile';
    id: number;
    name?: string;
  } | null>(null);
  const [rejectReason, setRejectReason] = useState<string>('');
  const [submittingReject, setSubmittingReject] = useState<boolean>(false);

  // Disburse Modal (Kế toán giải ngân tạm ứng)
  const [disburseModalVisible, setDisburseModalVisible] = useState<boolean>(false);
  const [disburseTarget, setDisburseTarget] = useState<AdvanceItem | null>(null);
  const [soChungTu, setSoChungTu] = useState<string>('');
  const [ngayChi, setNgayChi] = useState<string>(new Date().toISOString().slice(0, 10));
  const [ghiChuChi, setGhiChuChi] = useState<string>('');
  const [submittingDisburse, setSubmittingDisburse] = useState<boolean>(false);

  // Profile Diff Modal
  const [diffModalVisible, setDiffModalVisible] = useState<boolean>(false);
  const [diffTarget, setDiffTarget] = useState<ProfileChangeItem | null>(null);

  const fetchSummary = async () => {
    try {
      const res = await api.get('/approvals/summary');
      if (res.data?.success) {
        setSummary(res.data);
      }
    } catch (err) {
      console.error('Error fetching summary:', err);
    }
  };

  const fetchData = async () => {
    try {
      setLoading(true);
      const params = {
        status: statusFilter,
        search: searchKeyword.trim() || undefined,
        pageSize: 50,
      };

      if (activeTab === 'leave') {
        const res = await api.get('/approvals/leave', { params });
        setLeaveList(res.data?.data || []);
      } else if (activeTab === 'attendance') {
        const res = await api.get('/approvals/attendance-corrections', { params });
        setAttendanceList(res.data?.data || []);
      } else if (activeTab === 'overtime') {
        const res = await api.get('/approvals/overtime', { params });
        setOvertimeList(res.data?.data || []);
      } else if (activeTab === 'insurance') {
        const res = await api.get('/approvals/insurance-movements', { params });
        setInsuranceList(res.data?.data || []);
      } else if (activeTab === 'advance') {
        const res = await api.get('/approvals/advance', { params });
        setAdvanceList(res.data?.data || []);
      } else if (activeTab === 'profile') {
        const res = await api.get('/approvals/profile-requests', {
          params: {
            ...params,
            groupCode: profileGroupFilter !== 'ALL' ? profileGroupFilter : undefined,
          },
        });
        setProfileList(res.data?.data || []);
      }
    } catch {
      notification.error({ message: t('common.error'), description: t('common.loading') });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSummary();
  }, []);

  useEffect(() => {
    fetchData();
  }, [activeTab, statusFilter, searchKeyword, profileGroupFilter]);

  const handleApprove = async (
    type: 'leave' | 'attendance' | 'overtime' | 'insurance' | 'advance' | 'profile',
    id: number
  ) => {
    try {
      let endpoint = '';
      if (type === 'leave') endpoint = `/approvals/leave/${id}/approve`;
      else if (type === 'attendance') endpoint = `/approvals/attendance-corrections/${id}/approve`;
      else if (type === 'overtime') endpoint = `/approvals/overtime/${id}/approve`;
      else if (type === 'insurance') endpoint = `/approvals/insurance-movements/${id}/approve`;
      else if (type === 'advance') endpoint = `/approvals/advance/${id}/approve`;
      else if (type === 'profile') endpoint = `/approvals/profile-requests/${id}/approve`;

      const res = await api.post(endpoint);
      notification.success({
        message: t('common.success'),
        description: res.data?.message || t('approval.approveSuccess'),
      });
      fetchSummary();
      fetchData();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { message?: string } } };
      notification.error({
        message: t('common.error'),
        description: errorObj.response?.data?.message || t('common.saveError'),
      });
    }
  };

  const openRejectModal = (
    type: 'leave' | 'attendance' | 'overtime' | 'insurance' | 'advance' | 'profile',
    id: number,
    name?: string
  ) => {
    setRejectTarget({ type, id, name });
    setRejectReason('');
    setRejectModalVisible(true);
  };

  const handleConfirmReject = async () => {
    if (!rejectTarget) return;
    try {
      setSubmittingReject(true);
      let endpoint = '';
      if (rejectTarget.type === 'leave') endpoint = `/approvals/leave/${rejectTarget.id}/reject`;
      else if (rejectTarget.type === 'attendance') endpoint = `/approvals/attendance-corrections/${rejectTarget.id}/reject`;
      else if (rejectTarget.type === 'overtime') endpoint = `/approvals/overtime/${rejectTarget.id}/reject`;
      else if (rejectTarget.type === 'insurance') endpoint = `/approvals/insurance-movements/${rejectTarget.id}/reject`;
      else if (rejectTarget.type === 'advance') endpoint = `/approvals/advance/${rejectTarget.id}/reject`;
      else if (rejectTarget.type === 'profile') endpoint = `/approvals/profile-requests/${rejectTarget.id}/reject`;

      const res = await api.post(endpoint, { reason: rejectReason.trim() || 'Rejected' });
      notification.success({
        message: t('common.success'),
        description: res.data?.message || t('approval.rejectSuccess'),
      });
      setRejectModalVisible(false);
      setRejectTarget(null);
      fetchSummary();
      fetchData();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { message?: string } } };
      notification.error({
        message: t('common.error'),
        description: errorObj.response?.data?.message || t('common.saveError'),
      });
    } finally {
      setSubmittingReject(false);
    }
  };

  const openDisburseModal = (item: AdvanceItem) => {
    setDisburseTarget(item);
    setSoChungTu(`PC-UL-${item.nam}${String(item.thang).padStart(2, '0')}-${item.id}`);
    setNgayChi(new Date().toISOString().slice(0, 10));
    setGhiChuChi('');
    setDisburseModalVisible(true);
  };

  const handleConfirmDisburse = async () => {
    if (!disburseTarget) return;
    if (!soChungTu.trim()) {
      notification.warning({ message: 'Vui lòng nhập số chứng từ chi tiền.' });
      return;
    }
    try {
      setSubmittingDisburse(true);
      const res = await api.post(`/approvals/advance/${disburseTarget.id}/disburse`, {
        soChungTu: soChungTu.trim(),
        ngayChi,
        ghiChu: ghiChuChi.trim(),
      });
      notification.success({
        message: t('common.success'),
        description: res.data?.message || 'Giải ngân tạm ứng lương thành công!',
      });
      setDisburseModalVisible(false);
      setDisburseTarget(null);
      fetchSummary();
      fetchData();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { message?: string } } };
      notification.error({
        message: t('common.error'),
        description: errorObj.response?.data?.message || 'Lỗi khi giải ngân.',
      });
    } finally {
      setSubmittingDisburse(false);
    }
  };

  const renderStatusTag = (status: string) => {
    switch (status) {
      case 'APPROVED':
        return <Tag color="success" icon={<CheckCircleOutlined />}>{t('status.approved')}</Tag>;
      case 'REJECTED':
        return <Tag color="error" icon={<CloseCircleOutlined />}>{t('status.rejected')}</Tag>;
      case 'DISBURSED':
        return <Tag color="cyan" icon={<CheckCircleOutlined />}>Đã chi tiền</Tag>;
      default:
        return <Tag color="processing" icon={<ClockCircleOutlined />}>{t('status.pending')}</Tag>;
    }
  };

  const renderGroupCodeTag = (groupCode: string) => {
    switch (groupCode) {
      case 'PERSONAL_INFO':
        return <Tag color="blue">Thông tin cá nhân</Tag>;
      case 'INSURANCE_INFO':
        return <Tag color="geekblue">Bảo hiểm</Tag>;
      case 'DEPENDENTS':
        return <Tag color="purple">Người phụ thuộc</Tag>;
      case 'AVATAR':
        return <Tag color="gold">Ảnh đại diện</Tag>;
      default:
        return <Tag color="default">{groupCode}</Tag>;
    }
  };

  const leaveColumns: ColumnsType<LeaveItem> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 90,
      render: (c, r) => <Tag color="blue">{c || `#${r.manv}`}</Tag>,
    },
    {
      title: t('approval.colApplicant'),
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 170,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'departmentName',
      key: 'departmentName',
      width: 140,
      render: (tVal) => <Tag color="cyan">{tVal || '-'}</Tag>,
    },
    {
      title: t('approval.colType'),
      dataIndex: 'loaiNghi',
      key: 'loaiNghi',
      width: 130,
      render: (tVal) => <Tag color="purple">{tVal}</Tag>,
    },
    {
      title: t('approval.colTimeSpan'),
      key: 'thoigian',
      width: 200,
      render: (_, r) => (
        <Space direction="vertical" size={0}>
          <Text style={{ fontSize: 12 }}>{r.tuNgay} ~ {r.denNgay}</Text>
          <Text type="secondary" style={{ fontSize: 11 }}>({r.soNgay} {t('attendance.colActualDays')})</Text>
        </Space>
      ),
    },
    {
      title: t('approval.colReason'),
      dataIndex: 'lyDo',
      key: 'lyDo',
      ellipsis: true,
    },
    {
      title: t('approval.colStatus'),
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 130,
      render: renderStatusTag,
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 160,
      fixed: 'right',
      render: (_, r) => {
        if (r.trangThai !== 'PENDING') {
          return <Text type="secondary" style={{ fontSize: 12 }}>{r.ngayDuyet || '-'}</Text>;
        }
        if (!canApproveGeneral) {
          return <Text type="secondary" style={{ fontSize: 12 }}>Chờ duyệt</Text>;
        }
        return (
          <Space>
            <Button
              type="primary"
              size="small"
              style={{ background: '#10b981', borderColor: '#10b981' }}
              onClick={() => handleApprove('leave', r.id)}
            >
              {t('approval.btnApprove')}
            </Button>
            <Button
              danger
              size="small"
              onClick={() => openRejectModal('leave', r.id, r.employeeName)}
            >
              {t('approval.btnReject')}
            </Button>
          </Space>
        );
      },
    },
  ];

  const attendanceColumns: ColumnsType<AttendanceCorrectionItem> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 90,
      render: (c, r) => <Tag color="blue">{c || `#${r.manv}`}</Tag>,
    },
    {
      title: t('approval.colApplicant'),
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 170,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'departmentName',
      key: 'departmentName',
      width: 140,
      render: (tVal) => <Tag color="cyan">{tVal || '-'}</Tag>,
    },
    {
      title: t('common.date'),
      dataIndex: 'ngayCong',
      key: 'ngayCong',
      width: 110,
    },
    {
      title: t('approval.colTimeSpan'),
      key: 'gio',
      width: 150,
      render: (_, r) => (
        <Tag color="geekblue">{r.gioVaoMoi || '--:--'} ~ {r.gioRaMoi || '--:--'}</Tag>
      ),
    },
    {
      title: t('approval.colReason'),
      dataIndex: 'lyDo',
      key: 'lyDo',
      ellipsis: true,
    },
    {
      title: t('approval.colStatus'),
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 130,
      render: renderStatusTag,
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 160,
      fixed: 'right',
      render: (_, r) => {
        if (r.trangThai !== 'PENDING') {
          return <Text type="secondary" style={{ fontSize: 12 }}>{r.ngayDuyet || '-'}</Text>;
        }
        if (!canApproveGeneral) {
          return <Text type="secondary" style={{ fontSize: 12 }}>Chờ duyệt</Text>;
        }
        return (
          <Space>
            <Button
              type="primary"
              size="small"
              style={{ background: '#10b981', borderColor: '#10b981' }}
              onClick={() => handleApprove('attendance', r.id)}
            >
              {t('approval.btnApprove')}
            </Button>
            <Button
              danger
              size="small"
              onClick={() => openRejectModal('attendance', r.id, r.employeeName)}
            >
              {t('approval.btnReject')}
            </Button>
          </Space>
        );
      },
    },
  ];

  const overtimeColumns: ColumnsType<OvertimeItem> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 90,
      render: (c, r) => <Tag color="blue">{c || `#${r.manv}`}</Tag>,
    },
    {
      title: t('approval.colApplicant'),
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 170,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'departmentName',
      key: 'departmentName',
      width: 140,
      render: (tVal) => <Tag color="cyan">{tVal || '-'}</Tag>,
    },
    {
      title: t('common.date'),
      dataIndex: 'ngayTangCa',
      key: 'ngayTangCa',
      width: 110,
    },
    {
      title: t('overtime.colHours'),
      key: 'hours',
      width: 130,
      render: (_, r) => (
        <Space>
          <Text strong>{r.soGio}h</Text>
          <Tag color="orange">x{r.heSo}</Tag>
        </Space>
      ),
    },
    {
      title: t('approval.colReason'),
      dataIndex: 'noiDung',
      key: 'noiDung',
      ellipsis: true,
    },
    {
      title: t('approval.colStatus'),
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 130,
      render: renderStatusTag,
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 160,
      fixed: 'right',
      render: (_, r) => {
        if (r.trangThai !== 'PENDING') {
          return <Text type="secondary" style={{ fontSize: 12 }}>{r.ngayDuyet || '-'}</Text>;
        }
        if (!canApproveGeneral) {
          return <Text type="secondary" style={{ fontSize: 12 }}>Chờ duyệt</Text>;
        }
        return (
          <Space>
            <Button
              type="primary"
              size="small"
              style={{ background: '#10b981', borderColor: '#10b981' }}
              onClick={() => handleApprove('overtime', r.id)}
            >
              {t('approval.btnApprove')}
            </Button>
            <Button
              danger
              size="small"
              onClick={() => openRejectModal('overtime', r.id, r.employeeName)}
            >
              {t('approval.btnReject')}
            </Button>
          </Space>
        );
      },
    },
  ];

  const insuranceColumns: ColumnsType<InsuranceMovementItem> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 90,
      render: (c, r) => <Tag color="blue">{c || `#${r.manv}`}</Tag>,
    },
    {
      title: t('approval.colApplicant'),
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 170,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'departmentName',
      key: 'departmentName',
      width: 140,
    },
    {
      title: 'Kỳ công',
      dataIndex: 'maKyCong',
      key: 'maKyCong',
      width: 100,
      render: (val) => <Tag color="geekblue">{val}</Tag>,
    },
    {
      title: 'Loại biến động',
      dataIndex: 'loai',
      key: 'loai',
      width: 140,
      render: (loai) => {
        let color = 'default';
        let label = loai;
        if (loai === 'TANG') { color = 'green'; label = 'Tăng mới'; }
        else if (loai === 'GIAM') { color = 'orange'; label = 'Giảm lao động'; }
        else if (loai === 'DIEU_CHINH') { color = 'blue'; label = 'Điều chỉnh'; }
        else if (loai === 'TAM_DUNG') { color = 'purple'; label = 'Tạm dừng'; }
        return <Tag color={color}>{label}</Tag>;
      },
    },
    {
      title: 'Ngày hiệu lực',
      dataIndex: 'ngayHieuLuc',
      key: 'ngayHieuLuc',
      width: 120,
    },
    {
      title: t('approval.colReason'),
      dataIndex: 'lyDo',
      key: 'lyDo',
      ellipsis: true,
    },
    {
      title: t('approval.colStatus'),
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 120,
      render: renderStatusTag,
    },
    {
      title: 'Người duyệt',
      dataIndex: 'nguoiDuyet',
      key: 'nguoiDuyet',
      width: 140,
      render: (text) => text || '-',
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 150,
      fixed: 'right',
      render: (_, r) => {
        const isPending = r.trangThai === 'PENDING' || r.trangThai === 'DRAFT';
        if (!isPending) {
          return <Text type="secondary">{t('approval.processed')}</Text>;
        }
        if (!canApproveGeneral) {
          return <Text type="secondary" style={{ fontSize: 12 }}>Chờ duyệt</Text>;
        }
        return (
          <Space>
            <Button
              type="primary"
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={() => handleApprove('insurance', r.id)}
            >
              {t('approval.btnApprove')}
            </Button>
            <Button
              danger
              size="small"
              onClick={() => openRejectModal('insurance', r.id, r.employeeName)}
            >
              {t('approval.btnReject')}
            </Button>
          </Space>
        );
      },
    },
  ];

  const advanceColumns: ColumnsType<AdvanceItem> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 90,
      render: (c, r) => <Tag color="blue">{c || `#${r.manv}`}</Tag>,
    },
    {
      title: t('approval.colApplicant'),
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 170,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'departmentName',
      key: 'departmentName',
      width: 140,
      render: (tVal) => <Tag color="cyan">{tVal || '-'}</Tag>,
    },
    {
      title: 'Số tiền ứng',
      dataIndex: 'amount',
      key: 'amount',
      width: 130,
      render: (amount) => (
        <Text strong style={{ color: '#059669' }}>
          {Number(amount).toLocaleString('vi-VN')} đ
        </Text>
      ),
    },
    {
      title: 'Kỳ áp dụng',
      key: 'period',
      width: 100,
      render: (_, r) => <Tag color="geekblue">{String(r.thang).padStart(2, '0')}/{r.nam}</Tag>,
    },
    {
      title: 'Ngày đề nghị',
      dataIndex: 'advanceDate',
      key: 'advanceDate',
      width: 110,
    },
    {
      title: t('approval.colReason'),
      dataIndex: 'reason',
      key: 'reason',
      ellipsis: true,
    },
    {
      title: t('approval.colStatus'),
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: renderStatusTag,
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 200,
      fixed: 'right',
      render: (_, r) => {
        if (r.status === 'PENDING') {
          if (!canApproveAdvance) {
            return <Text type="secondary" style={{ fontSize: 12 }}>Chờ quản lý duyệt</Text>;
          }
          return (
            <Space>
              <Button
                type="primary"
                size="small"
                style={{ background: '#10b981', borderColor: '#10b981' }}
                onClick={() => handleApprove('advance', r.id)}
              >
                {t('approval.btnApprove')}
              </Button>
              <Button
                danger
                size="small"
                onClick={() => openRejectModal('advance', r.id, r.employeeName)}
              >
                {t('approval.btnReject')}
              </Button>
            </Space>
          );
        }
        if (r.status === 'APPROVED') {
          if (!canDisburseAdvance) {
            return <Text type="secondary" style={{ fontSize: 12 }}>Chờ kế toán chi tiền</Text>;
          }
          return (
            <Button
              type="primary"
              size="small"
              icon={<DollarOutlined />}
              style={{ background: '#2563eb', borderColor: '#2563eb' }}
              onClick={() => openDisburseModal(r)}
            >
              Chi tiền
            </Button>
          );
        }
        if (r.status === 'DISBURSED') {
          return (
            <Space direction="vertical" size={0}>
              <Text type="secondary" style={{ fontSize: 11 }}>Đã chi: #{r.idUlLink || '-'}</Text>
              <Text type="secondary" style={{ fontSize: 11 }}>CT: {r.voucherNo || '-'}</Text>
            </Space>
          );
        }
        return <Text type="secondary" style={{ fontSize: 12 }}>{r.rejectReason || '-'}</Text>;
      },
    },
  ];

  const profileColumns: ColumnsType<ProfileChangeItem> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 90,
      render: (c, r) => <Tag color="blue">{c || `#${r.manv}`}</Tag>,
    },
    {
      title: t('approval.colApplicant'),
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 170,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'departmentName',
      key: 'departmentName',
      width: 140,
      render: (tVal) => <Tag color="cyan">{tVal || '-'}</Tag>,
    },
    {
      title: 'Nhóm hồ sơ',
      dataIndex: 'groupCode',
      key: 'groupCode',
      width: 150,
      render: renderGroupCodeTag,
    },
    {
      title: 'Loại yêu cầu',
      dataIndex: 'reqType',
      key: 'reqType',
      width: 140,
    },
    {
      title: t('approval.colReason'),
      dataIndex: 'reason',
      key: 'reason',
      ellipsis: true,
    },
    {
      title: t('approval.colStatus'),
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: renderStatusTag,
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 220,
      fixed: 'right',
      render: (_, r) => (
        <Space>
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => {
              setDiffTarget(r);
              setDiffModalVisible(true);
            }}
          >
            So sánh
          </Button>
          {r.status === 'PENDING' && (
            canApproveProfile ? (
              <>
                <Button
                  type="primary"
                  size="small"
                  style={{ background: '#10b981', borderColor: '#10b981' }}
                  onClick={() => handleApprove('profile', r.id)}
                >
                  Duyệt
                </Button>
                <Button
                  danger
                  size="small"
                  onClick={() => openRejectModal('profile', r.id, r.employeeName)}
                >
                  Từ chối
                </Button>
              </>
            ) : (
              <Text type="secondary" style={{ fontSize: 12 }}>Chờ quản lý duyệt</Text>
            )
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16, width: '100%' }}>
      {/* PAGE HEADER */}
      <PageHeader
        title={t('approval.pageTitle') || 'Trung Tâm Phê Duyệt'}
        subtitle="Xét duyệt đơn nghỉ phép, chấm công, làm thêm, bảo hiểm, tạm ứng lương và hồ sơ nhân viên"
        breadcrumbs={[
          { title: 'Quản trị nhân sự' },
          { title: 'Trung tâm phê duyệt' },
        ]}
        extra={
          <Button
            icon={<ReloadOutlined spin={loading} />}
            onClick={() => {
              fetchSummary();
              fetchData();
            }}
            style={{ borderRadius: 8 }}
          >
            {t('common.refresh')}
          </Button>
        }
      />

      {/* THẺ THỐNG KÊ NHANH */}
      <Row gutter={[16, 16]}>
        <Col xs={12} sm={8} md={4} style={{ flexGrow: 1 }}>
          <MetricCard
            title={t('status.pending')}
            value={summary.totalPending}
            icon={<ClockCircleOutlined />}
            accent="blue"
          />
        </Col>
        <Col xs={12} sm={8} md={4} style={{ flexGrow: 1 }}>
          <MetricCard
            title={t('approval.tabLeaves', { count: '' })}
            value={summary.leavePending}
            icon={<CalendarOutlined />}
            accent="green"
          />
        </Col>
        <Col xs={12} sm={8} md={4} style={{ flexGrow: 1 }}>
          <MetricCard
            title={t('approval.tabCorrections', { count: '' })}
            value={summary.attendancePending}
            icon={<ScheduleOutlined />}
            accent="purple"
          />
        </Col>
        <Col xs={12} sm={8} md={4} style={{ flexGrow: 1 }}>
          <MetricCard
            title={t('approval.tabOvertimes', { count: '' })}
            value={summary.overtimePending}
            icon={<FieldTimeOutlined />}
            accent="amber"
          />
        </Col>
        <Col xs={12} sm={8} md={4} style={{ flexGrow: 1 }}>
          <MetricCard
            title="Tạm ứng lương"
            value={summary.advancePending ?? 0}
            icon={<DollarOutlined />}
            accent="green"
          />
        </Col>
        <Col xs={12} sm={8} md={4} style={{ flexGrow: 1 }}>
          <MetricCard
            title="Đổi hồ sơ"
            value={summary.profilePending ?? 0}
            icon={<UserOutlined />}
            accent="purple"
          />
        </Col>
      </Row>

      {/* FILTER & TABS */}
      <Card
        bordered={false}
        style={{
          borderRadius: 12,
          background: tokens.cardBg,
          border: `1px solid ${tokens.borderSubtle}`,
          boxShadow: '0 1px 3px rgba(0,0,0,0.04)',
        }}
      >
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: 12,
            marginBottom: 16,
          }}
        >
          <Space wrap>
            <Input
              prefix={<SearchOutlined style={{ color: tokens.textMuted }} />}
              placeholder={t('common.searchPlaceholder')}
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              style={{ minWidth: 220, borderRadius: 8 }}
              allowClear
            />
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ minWidth: 150, borderRadius: 8 }}
              options={[
                { value: 'PENDING', label: t('status.pending') },
                { value: 'APPROVED', label: t('status.approved') },
                { value: 'DISBURSED', label: 'Đã chi tiền' },
                { value: 'REJECTED', label: t('status.rejected') },
                { value: 'ALL', label: t('common.all') },
              ]}
            />
            {activeTab === 'profile' && (
              <Select
                value={profileGroupFilter}
                onChange={setProfileGroupFilter}
                style={{ minWidth: 180, borderRadius: 8 }}
                options={[
                  { value: 'ALL', label: 'Tất cả nhóm hồ sơ' },
                  { value: 'PERSONAL_INFO', label: 'Thông tin cá nhân' },
                  { value: 'INSURANCE_INFO', label: 'Bảo hiểm' },
                  { value: 'DEPENDENTS', label: 'Người phụ thuộc' },
                  { value: 'AVATAR', label: 'Ảnh đại diện' },
                ]}
              />
            )}
          </Space>
        </div>

        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            {
              key: 'leave',
              label: (
                <Space>
                  <CalendarOutlined />
                  <span>{t('approval.tabLeaves', { count: summary.leavePending > 0 ? summary.leavePending : '' })}</span>
                  {summary.leavePending > 0 && <Tag color="green">{summary.leavePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={leaveColumns}
                  dataSource={leaveList}
                  rowKey="id"
                  loading={loading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
            {
              key: 'attendance',
              label: (
                <Space>
                  <ScheduleOutlined />
                  <span>{t('approval.tabCorrections', { count: summary.attendancePending > 0 ? summary.attendancePending : '' })}</span>
                  {summary.attendancePending > 0 && <Tag color="purple">{summary.attendancePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={attendanceColumns}
                  dataSource={attendanceList}
                  rowKey="id"
                  loading={loading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
            {
              key: 'overtime',
              label: (
                <Space>
                  <FieldTimeOutlined />
                  <span>{t('approval.tabOvertimes', { count: summary.overtimePending > 0 ? summary.overtimePending : '' })}</span>
                  {summary.overtimePending > 0 && <Tag color="magenta">{summary.overtimePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={overtimeColumns}
                  dataSource={overtimeList}
                  rowKey="id"
                  loading={loading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
            {
              key: 'advance',
              label: (
                <Space>
                  <DollarOutlined />
                  <span>Tạm ứng lương</span>
                  {(summary.advancePending ?? 0) > 0 && <Tag color="gold">{summary.advancePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={advanceColumns}
                  dataSource={advanceList}
                  rowKey="id"
                  loading={loading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
            {
              key: 'profile',
              label: (
                <Space>
                  <UserOutlined />
                  <span>Thay đổi hồ sơ</span>
                  {(summary.profilePending ?? 0) > 0 && <Tag color="blue">{summary.profilePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={profileColumns}
                  dataSource={profileList}
                  rowKey="id"
                  loading={loading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
            {
              key: 'insurance',
              label: (
                <Space>
                  <SafetyCertificateOutlined />
                  <span>Biến động BHXH</span>
                  {(summary.insurancePending ?? 0) > 0 && <Tag color="cyan">{summary.insurancePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={insuranceColumns}
                  dataSource={insuranceList}
                  rowKey="id"
                  loading={loading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
          ]}
        />
      </Card>

      {/* REJECT MODAL */}
      <Modal
        title={
          <Space>
            <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
            <span>{t('approval.modalRejectTitle', { name: rejectTarget?.name || '' })}</span>
          </Space>
        }
        open={rejectModalVisible}
        onCancel={() => setRejectModalVisible(false)}
        onOk={handleConfirmReject}
        confirmLoading={submittingReject}
        okText={t('approval.btnReject')}
        okButtonProps={{ danger: true }}
        cancelText={t('common.cancel')}
        width="min(500px, 95vw)"
      >
        <div style={{ marginTop: 12, marginBottom: 8 }}>
          <Text>{t('approval.rejectReasonPrompt')}</Text>
        </div>
        <Input.TextArea
          rows={4}
          placeholder={t('approval.rejectReasonPrompt')}
          value={rejectReason}
          onChange={(e) => setRejectReason(e.target.value)}
        />
      </Modal>

      {/* DISBURSE ADVANCE MODAL (Kế toán chi tiền) */}
      <Modal
        title={
          <Space>
            <DollarOutlined style={{ color: '#10b981' }} />
            <span>Xác nhận giải ngân tạm ứng lương</span>
          </Space>
        }
        open={disburseModalVisible}
        onCancel={() => setDisburseModalVisible(false)}
        onOk={handleConfirmDisburse}
        confirmLoading={submittingDisburse}
        okText="Xác nhận chi tiền"
        cancelText={t('common.cancel')}
        width="min(540px, 95vw)"
      >
        {disburseTarget && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 14, marginTop: 12 }}>
            <Card size="small" style={{ background: '#f8fafc', borderRadius: 8 }}>
              <p style={{ margin: 0 }}><strong>Nhân viên:</strong> {disburseTarget.employeeName} ({disburseTarget.employeeCode})</p>
              <p style={{ margin: '4px 0 0' }}><strong>Số tiền tạm ứng:</strong> <span style={{ color: '#059669', fontWeight: 600 }}>{Number(disburseTarget.amount).toLocaleString('vi-VN')} đ</span></p>
              <p style={{ margin: '4px 0 0' }}><strong>Kỳ lương:</strong> Tháng {disburseTarget.thang}/{disburseTarget.nam}</p>
            </Card>
            <div>
              <Text strong>Số chứng từ / Phiếu chi (*):</Text>
              <Input
                style={{ marginTop: 4, borderRadius: 6 }}
                value={soChungTu}
                onChange={(e) => setSoChungTu(e.target.value)}
                placeholder="VD: PC-UL-202610-001"
              />
            </div>
            <div>
              <Text strong>Ngày chi tiền:</Text>
              <Input
                type="date"
                style={{ marginTop: 4, borderRadius: 6 }}
                value={ngayChi}
                onChange={(e) => setNgayChi(e.target.value)}
              />
            </div>
            <div>
              <Text strong>Ghi chú chi tiền:</Text>
              <Input.TextArea
                rows={2}
                style={{ marginTop: 4, borderRadius: 6 }}
                value={ghiChuChi}
                onChange={(e) => setGhiChuChi(e.target.value)}
                placeholder="Ghi chú thêm nếu có..."
              />
            </div>
          </div>
        )}
      </Modal>

      {/* PROFILE DIFF MODAL (So sánh Trước vs Sau / Xem Avatar đề xuất) */}
      <Modal
        title={
          <Space>
            <UserOutlined style={{ color: '#3b82f6' }} />
            <span>Chi tiết đề xuất thay đổi hồ sơ #{diffTarget?.id}</span>
          </Space>
        }
        open={diffModalVisible}
        onCancel={() => setDiffModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setDiffModalVisible(false)}>
            Đóng
          </Button>,
          diffTarget?.status === 'PENDING' && (
            <Button
              key="approve"
              type="primary"
              style={{ background: '#10b981', borderColor: '#10b981' }}
              onClick={() => {
                setDiffModalVisible(false);
                if (diffTarget) handleApprove('profile', diffTarget.id);
              }}
            >
              Phê duyệt & Áp dụng
            </Button>
          ),
        ]}
        width="min(800px, 95vw)"
      >
        {diffTarget && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 14, marginTop: 12 }}>
            <Card size="small" style={{ background: '#f8fafc', borderRadius: 8 }}>
              <p style={{ margin: 0 }}><strong>Nhân viên:</strong> {diffTarget.employeeName} ({diffTarget.employeeCode}) - {diffTarget.departmentName}</p>
              <p style={{ margin: '4px 0 0' }}><strong>Nhóm:</strong> {renderGroupCodeTag(diffTarget.groupCode)} | <strong>Lý do:</strong> {diffTarget.reason}</p>
            </Card>

            {diffTarget.groupCode === 'AVATAR' || diffTarget.hasProposedAvatarBlob ? (
              <div style={{ textAlign: 'center', padding: '16px 0' }}>
                <Text strong style={{ display: 'block', marginBottom: 8 }}>Ảnh đại diện đề xuất mới:</Text>
                <Image
                  width={200}
                  height={200}
                  style={{ objectFit: 'cover', borderRadius: '50%', border: '3px solid #3b82f6' }}
                  src={`/api/approvals/profile-requests/${diffTarget.id}/avatar`}
                  fallback="data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100'><rect width='100' height='100' fill='%23ccc'/><text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='%23666'>No Image</text></svg>"
                />
              </div>
            ) : (
              <Row gutter={12}>
                <Col span={12}>
                  <Text strong style={{ color: '#64748b' }}>DỮ LIỆU HIỆN TẠI (TRƯỚC)</Text>
                  <pre
                    style={{
                      marginTop: 6,
                      background: '#f1f5f9',
                      padding: 10,
                      borderRadius: 6,
                      maxHeight: 300,
                      overflow: 'auto',
                      fontSize: 12,
                      whiteSpace: 'pre-wrap',
                      wordBreak: 'break-word',
                    }}
                  >
                    {diffTarget.currentDataJson
                      ? JSON.stringify(JSON.parse(diffTarget.currentDataJson), null, 2)
                      : '(Chưa có dữ liệu gốc)'}
                  </pre>
                </Col>
                <Col span={12}>
                  <Text strong style={{ color: '#0284c7' }}>DỮ LIỆU ĐỀ XUẤT (SẼ ÁP DỤNG)</Text>
                  <pre
                    style={{
                      marginTop: 6,
                      background: '#ecfdf5',
                      padding: 10,
                      borderRadius: 6,
                      maxHeight: 300,
                      overflow: 'auto',
                      fontSize: 12,
                      whiteSpace: 'pre-wrap',
                      wordBreak: 'break-word',
                      border: '1px solid #a7f3d0',
                    }}
                  >
                    {diffTarget.proposedDataJson
                      ? JSON.stringify(JSON.parse(diffTarget.proposedDataJson), null, 2)
                      : '(Chưa có dữ liệu đề xuất)'}
                  </pre>
                </Col>
              </Row>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}

export default ApprovalCenterPage;
