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
  Statistic,
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
} from '@ant-design/icons';
import api from '../services/api';

const { Text, Title } = Typography;

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

interface SummaryData {
  totalPending: number;
  leavePending: number;
  attendancePending: number;
  overtimePending: number;
}

export function ApprovalCenterPage() {
  const [activeTab, setActiveTab] = useState<string>('leave');
  const [statusFilter, setStatusFilter] = useState<string>('PENDING');
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [summary, setSummary] = useState<SummaryData>({
    totalPending: 0,
    leavePending: 0,
    attendancePending: 0,
    overtimePending: 0,
  });

  // Lists
  const [leaveList, setLeaveList] = useState<LeaveItem[]>([]);
  const [attendanceList, setAttendanceList] = useState<AttendanceCorrectionItem[]>([]);
  const [overtimeList, setOvertimeList] = useState<OvertimeItem[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // Reject Modal
  const [rejectModalVisible, setRejectModalVisible] = useState<boolean>(false);
  const [rejectTarget, setRejectTarget] = useState<{ type: 'leave' | 'attendance' | 'overtime'; id: number } | null>(null);
  const [rejectReason, setRejectReason] = useState<string>('');
  const [submittingReject, setSubmittingReject] = useState<boolean>(false);

  useEffect(() => {
    fetchSummary();
  }, []);

  useEffect(() => {
    fetchData();
  }, [activeTab, statusFilter, searchKeyword]);

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
      }
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải danh sách phê duyệt.' });
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (type: 'leave' | 'attendance' | 'overtime', id: number) => {
    try {
      let endpoint = '';
      if (type === 'leave') endpoint = `/approvals/leave/${id}/approve`;
      else if (type === 'attendance') endpoint = `/approvals/attendance-corrections/${id}/approve`;
      else if (type === 'overtime') endpoint = `/approvals/overtime/${id}/approve`;

      const res = await api.post(endpoint);
      notification.success({
        message: 'Thành công',
        description: res.data?.message || 'Phê duyệt thành công!',
      });
      fetchSummary();
      fetchData();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { message?: string } } };
      notification.error({
        message: 'Lỗi phê duyệt',
        description: errorObj.response?.data?.message || 'Không thể thực hiện phê duyệt.',
      });
    }
  };

  const openRejectModal = (type: 'leave' | 'attendance' | 'overtime', id: number) => {
    setRejectTarget({ type, id });
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

      const res = await api.post(endpoint, { reason: rejectReason.trim() || 'Cấp quản lý từ chối yêu cầu.' });
      notification.success({
        message: 'Đã từ chối',
        description: res.data?.message || 'Đã từ chối yêu cầu thành công.',
      });
      setRejectModalVisible(false);
      setRejectTarget(null);
      fetchSummary();
      fetchData();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { message?: string } } };
      notification.error({
        message: 'Lỗi từ chối',
        description: errorObj.response?.data?.message || 'Không thể từ chối yêu cầu.',
      });
    } finally {
      setSubmittingReject(false);
    }
  };

  const renderStatusTag = (status: string) => {
    switch (status) {
      case 'APPROVED':
        return <Tag color="success" icon={<CheckCircleOutlined />}>Đã duyệt</Tag>;
      case 'REJECTED':
        return <Tag color="error" icon={<CloseCircleOutlined />}>Đã từ chối</Tag>;
      default:
        return <Tag color="warning" icon={<ClockCircleOutlined />}>Chờ phê duyệt</Tag>;
    }
  };

  // Leave Table Columns
  const leaveColumns: ColumnsType<LeaveItem> = [
    {
      title: 'Mã NV',
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 130,
      render: (code: string) => <Tag color="blue">{code}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'employeeName',
      key: 'employeeName',
      render: (name: string, r) => (
        <Space direction="vertical" size={0}>
          <Text strong>{name}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{r.departmentName}</Text>
        </Space>
      ),
    },
    {
      title: 'Loại nghỉ',
      dataIndex: 'loaiNghi',
      key: 'loaiNghi',
      render: (type: string) => <Tag color="geekblue">{type}</Tag>,
    },
    {
      title: 'Thời gian nghỉ',
      key: 'period',
      render: (_, r) => (
        <Space direction="vertical" size={0}>
          <Text>{r.tuNgay === r.denNgay ? r.tuNgay : `${r.tuNgay} -> ${r.denNgay}`}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>({r.soNgay} ngày)</Text>
        </Space>
      ),
    },
    {
      title: 'Lý do xin nghỉ',
      dataIndex: 'lyDo',
      key: 'lyDo',
      render: (reason: string) => reason || <Text italic type="secondary">Không có lý do</Text>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 130,
      render: (s: string) => renderStatusTag(s),
    },
    {
      title: 'Ngày gửi',
      dataIndex: 'ngayTao',
      key: 'ngayTao',
      width: 140,
      render: (d: string) => <Text type="secondary" style={{ fontSize: 12 }}>{d}</Text>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 180,
      render: (_, r) =>
        r.trangThai === 'PENDING' ? (
          <Space size="small">
            <Button
              type="primary"
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={() => handleApprove('leave', r.id)}
            >
              Duyệt
            </Button>
            <Button
              danger
              size="small"
              icon={<CloseCircleOutlined />}
              onClick={() => openRejectModal('leave', r.id)}
            >
              Từ chối
            </Button>
          </Space>
        ) : (
          <Text type="secondary" style={{ fontSize: 12 }}>
            {r.nguoiDuyet ? `Bởi: ${r.nguoiDuyet}` : 'Đã xử lý'}
          </Text>
        ),
    },
  ];

  // Attendance Correction Columns
  const attendanceColumns: ColumnsType<AttendanceCorrectionItem> = [
    {
      title: 'Mã NV',
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 130,
      render: (code: string) => <Tag color="blue">{code}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'employeeName',
      key: 'employeeName',
      render: (name: string, r) => (
        <Space direction="vertical" size={0}>
          <Text strong>{name}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{r.departmentName}</Text>
        </Space>
      ),
    },
    {
      title: 'Ngày điều chỉnh',
      dataIndex: 'ngayCong',
      key: 'ngayCong',
      render: (d: string) => <Tag color="purple">{d}</Tag>,
    },
    {
      title: 'Giờ vào / ra đề nghị',
      key: 'times',
      render: (_, r) => (
        <Space>
          <Tag color="cyan">Vào: {r.gioVaoMoi || '--:--'}</Tag>
          <Tag color="geekblue">Ra: {r.gioRaMoi || '--:--'}</Tag>
        </Space>
      ),
    },
    {
      title: 'Lý do giải trình',
      dataIndex: 'lyDo',
      key: 'lyDo',
      render: (reason: string) => reason || <Text italic type="secondary">Không có lý do</Text>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 130,
      render: (s: string) => renderStatusTag(s),
    },
    {
      title: 'Ngày gửi',
      dataIndex: 'ngayTao',
      key: 'ngayTao',
      width: 140,
      render: (d: string) => <Text type="secondary" style={{ fontSize: 12 }}>{d}</Text>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 180,
      render: (_, r) =>
        r.trangThai === 'PENDING' ? (
          <Space size="small">
            <Button
              type="primary"
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={() => handleApprove('attendance', r.id)}
            >
              Duyệt
            </Button>
            <Button
              danger
              size="small"
              icon={<CloseCircleOutlined />}
              onClick={() => openRejectModal('attendance', r.id)}
            >
              Từ chối
            </Button>
          </Space>
        ) : (
          <Text type="secondary" style={{ fontSize: 12 }}>
            {r.nguoiDuyet ? `Bởi: ${r.nguoiDuyet}` : 'Đã xử lý'}
          </Text>
        ),
    },
  ];

  // Overtime Columns
  const overtimeColumns: ColumnsType<OvertimeItem> = [
    {
      title: 'Mã NV',
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 130,
      render: (code: string) => <Tag color="blue">{code}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'employeeName',
      key: 'employeeName',
      render: (name: string, r) => (
        <Space direction="vertical" size={0}>
          <Text strong>{name}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{r.departmentName}</Text>
        </Space>
      ),
    },
    {
      title: 'Ngày tăng ca',
      dataIndex: 'ngayTangCa',
      key: 'ngayTangCa',
      render: (d: string) => <Tag color="orange">{d}</Tag>,
    },
    {
      title: 'Số giờ / Hệ số',
      key: 'hours',
      render: (_, r) => (
        <Space>
          <Tag color="volcano">{r.soGio} giờ</Tag>
          <Tag color="gold">x{r.heSo}</Tag>
        </Space>
      ),
    },
    {
      title: 'Nội dung công việc',
      dataIndex: 'noiDung',
      key: 'noiDung',
      render: (txt: string) => txt || <Text italic type="secondary">Tăng ca dự án</Text>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'trangThai',
      key: 'trangThai',
      width: 130,
      render: (s: string) => renderStatusTag(s),
    },
    {
      title: 'Ngày gửi',
      dataIndex: 'ngayTao',
      key: 'ngayTao',
      width: 140,
      render: (d: string) => <Text type="secondary" style={{ fontSize: 12 }}>{d}</Text>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 180,
      render: (_, r) =>
        r.trangThai === 'PENDING' ? (
          <Space size="small">
            <Button
              type="primary"
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={() => handleApprove('overtime', r.id)}
            >
              Duyệt
            </Button>
            <Button
              danger
              size="small"
              icon={<CloseCircleOutlined />}
              onClick={() => openRejectModal('overtime', r.id)}
            >
              Từ chối
            </Button>
          </Space>
        ) : (
          <Text type="secondary" style={{ fontSize: 12 }}>
            {r.nguoiDuyet ? `Bởi: ${r.nguoiDuyet}` : 'Đã xử lý'}
          </Text>
        ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Title level={3} style={{ margin: 0 }}>
            Trung tâm Phê duyệt Yêu cầu (Approval Center)
          </Title>
          <Text type="secondary">
            Điều phối và xử lý toàn bộ các yêu cầu tự phục vụ (Self-Service) phát sinh từ nhân viên qua ứng dụng Mobile.
          </Text>
        </div>
        <Button icon={<ReloadOutlined />} onClick={() => { fetchSummary(); fetchData(); }}>
          Làm mới
        </Button>
      </div>

      {/* SUMMARY STATISTIC CARDS */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={6}>
          <Card size="small" style={{ borderRadius: 8, borderColor: summary.totalPending > 0 ? '#fa8c16' : '#d9d9d9' }}>
            <Statistic
              title="Tổng chờ phê duyệt"
              value={summary.totalPending}
              valueStyle={{ color: summary.totalPending > 0 ? '#fa8c16' : '#52c41a', fontWeight: 700 }}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card size="small" style={{ borderRadius: 8 }}>
            <Statistic
              title="Nghỉ phép chờ duyệt"
              value={summary.leavePending}
              valueStyle={{ color: '#1890ff', fontWeight: 700 }}
              prefix={<CalendarOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card size="small" style={{ borderRadius: 8 }}>
            <Statistic
              title="Điều chỉnh công chờ duyệt"
              value={summary.attendancePending}
              valueStyle={{ color: '#722ed1', fontWeight: 700 }}
              prefix={<ScheduleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card size="small" style={{ borderRadius: 8 }}>
            <Statistic
              title="Tăng ca chờ duyệt"
              value={summary.overtimePending}
              valueStyle={{ color: '#eb2f96', fontWeight: 700 }}
              prefix={<FieldTimeOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* MAIN CONTENT CARD */}
      <Card style={{ borderRadius: 12 }}>
        {/* FILTER BAR */}
        <Row gutter={[16, 16]} style={{ marginBottom: 20 }}>
          <Col xs={24} sm={10} md={8}>
            <Input
              placeholder="Tìm kiếm theo mã NV hoặc tên nhân viên..."
              prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              allowClear
            />
          </Col>
          <Col xs={24} sm={8} md={6}>
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ width: '100%' }}
              options={[
                { value: 'PENDING', label: '⏳ Đang chờ phê duyệt' },
                { value: 'APPROVED', label: '✅ Đã được phê duyệt' },
                { value: 'REJECTED', label: '❌ Đã bị từ chối' },
                { value: 'ALL', label: '📋 Tất cả trạng thái' },
              ]}
            />
          </Col>
        </Row>

        {/* TABS FOR DIFFERENT WORKFLOWS */}
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          type="card"
          items={[
            {
              key: 'leave',
              label: (
                <Space>
                  <CalendarOutlined />
                  <span>Đơn xin nghỉ phép</span>
                  {summary.leavePending > 0 && <Tag color="warning">{summary.leavePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={leaveColumns}
                  dataSource={leaveList}
                  rowKey="id"
                  loading={loading}
                  pagination={{ pageSize: 15, showTotal: (t) => `Tổng cộng ${t} yêu cầu` }}
                />
              ),
            },
            {
              key: 'attendance',
              label: (
                <Space>
                  <ScheduleOutlined />
                  <span>Yêu cầu điều chỉnh công</span>
                  {summary.attendancePending > 0 && <Tag color="purple">{summary.attendancePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={attendanceColumns}
                  dataSource={attendanceList}
                  rowKey="id"
                  loading={loading}
                  pagination={{ pageSize: 15, showTotal: (t) => `Tổng cộng ${t} yêu cầu` }}
                />
              ),
            },
            {
              key: 'overtime',
              label: (
                <Space>
                  <FieldTimeOutlined />
                  <span>Đăng ký tăng ca (OT)</span>
                  {summary.overtimePending > 0 && <Tag color="magenta">{summary.overtimePending}</Tag>}
                </Space>
              ),
              children: (
                <Table
                  columns={overtimeColumns}
                  dataSource={overtimeList}
                  rowKey="id"
                  loading={loading}
                  pagination={{ pageSize: 15, showTotal: (t) => `Tổng cộng ${t} yêu cầu` }}
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
            <span>Xác nhận từ chối yêu cầu</span>
          </Space>
        }
        open={rejectModalVisible}
        onCancel={() => setRejectModalVisible(false)}
        onOk={handleConfirmReject}
        confirmLoading={submittingReject}
        okText="Xác nhận từ chối"
        okButtonProps={{ danger: true }}
        cancelText="Hủy"
      >
        <div style={{ marginTop: 12, marginBottom: 8 }}>
          <Text>Vui lòng nhập lý do từ chối để thông báo đến nhân viên:</Text>
        </div>
        <Input.TextArea
          rows={4}
          placeholder="Ví dụ: Thiếu tài liệu chứng minh, trùng ca công việc, hoặc kế hoạch nhân sự không đáp ứng..."
          value={rejectReason}
          onChange={(e) => setRejectReason(e.target.value)}
        />
      </Modal>
    </div>
  );
}

export default ApprovalCenterPage;
