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
} from '@ant-design/icons';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import { PageHeader } from '../theme/components/PageHeader';
import { MetricCard } from '../theme/components/MetricCard';

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

interface SummaryData {
  totalPending: number;
  leavePending: number;
  attendancePending: number;
  overtimePending: number;
  insurancePending?: number;
}

export function ApprovalCenterPage() {
  const { t } = useAppLanguage();
  const { tokens } = useAppTheme();
  const [activeTab, setActiveTab] = useState<string>('leave');
  const [statusFilter, setStatusFilter] = useState<string>('PENDING');
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [summary, setSummary] = useState<SummaryData>({
    totalPending: 0,
    leavePending: 0,
    attendancePending: 0,
    overtimePending: 0,
    insurancePending: 0,
  });

  // Lists
  const [leaveList, setLeaveList] = useState<LeaveItem[]>([]);
  const [attendanceList, setAttendanceList] = useState<AttendanceCorrectionItem[]>([]);
  const [overtimeList, setOvertimeList] = useState<OvertimeItem[]>([]);
  const [insuranceList, setInsuranceList] = useState<InsuranceMovementItem[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // Reject Modal
  const [rejectModalVisible, setRejectModalVisible] = useState<boolean>(false);
  const [rejectTarget, setRejectTarget] = useState<{ type: 'leave' | 'attendance' | 'overtime' | 'insurance'; id: number; name?: string } | null>(null);
  const [rejectReason, setRejectReason] = useState<string>('');
  const [submittingReject, setSubmittingReject] = useState<boolean>(false);

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
  }, [activeTab, statusFilter, searchKeyword]);

  const handleApprove = async (type: 'leave' | 'attendance' | 'overtime' | 'insurance', id: number) => {
    try {
      let endpoint = '';
      if (type === 'leave') endpoint = `/approvals/leave/${id}/approve`;
      else if (type === 'attendance') endpoint = `/approvals/attendance-corrections/${id}/approve`;
      else if (type === 'overtime') endpoint = `/approvals/overtime/${id}/approve`;
      else if (type === 'insurance') endpoint = `/approvals/insurance-movements/${id}/approve`;

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

  const openRejectModal = (type: 'leave' | 'attendance' | 'overtime' | 'insurance', id: number, name?: string) => {
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

  const renderStatusTag = (status: string) => {
    switch (status) {
      case 'APPROVED':
        return <Tag color="success" icon={<CheckCircleOutlined />}>{t('status.approved')}</Tag>;
      case 'REJECTED':
        return <Tag color="error" icon={<CloseCircleOutlined />}>{t('status.rejected')}</Tag>;
      default:
        return <Tag color="processing" icon={<ClockCircleOutlined />}>{t('status.pending')}</Tag>;
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
      render: (st) => renderStatusTag(st),
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

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16, width: '100%' }}>
      {/* PAGE HEADER */}
      <PageHeader
        title={t('approval.pageTitle') || 'Trung Tâm Phê Duyệt'}
        subtitle="Xét duyệt nhanh các đơn xin nghỉ phép, giải trình chấm công, làm thêm giờ và biến động BHXH"
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

      {/* 5 THẺ THỐNG KÊ NHANH */}
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
            title="Biến động BHXH"
            value={summary.insurancePending ?? 0}
            icon={<SafetyCertificateOutlined />}
            accent="cyan"
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
                { value: 'REJECTED', label: t('status.rejected') },
                { value: 'ALL', label: t('common.all') },
              ]}
            />
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
    </div>
  );
}

export default ApprovalCenterPage;
