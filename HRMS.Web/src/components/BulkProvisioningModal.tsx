import React, { useState, useEffect } from 'react';
import {
  Modal,
  Steps,
  Form,
  Select,
  Checkbox,
  Input,
  Button,
  Space,
  Table,
  Tag,
  Row,
  Col,
  Statistic,
  Card,
  Alert,
  Typography,
  message,
  notification,
  Divider,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  UserAddOutlined,
  FilterOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  SyncOutlined,
  IdcardOutlined,
  CopyOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import api from '../services/api';

const { Text, Paragraph } = Typography;

interface BulkCandidate {
  EmployeeId: number;
  EmployeeCode: string;
  FullName: string;
  DepartmentId?: number;
  DepartmentName: string;
  PositionName: string;
  ExistingAccount?: string;
  HasAccount: boolean;
  MobileStatus: string;
  SuggestedLoginName: string;
  IsEligible: boolean;
  Reason: string;
}

interface BulkResultItem {
  EmployeeId: number;
  EmployeeCode: string;
  FullName: string;
  LoginName: string;
  Result: string;
  Message: string;
}

interface BulkProvisioningModalProps {
  visible: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

export const BulkProvisioningModal: React.FC<BulkProvisioningModalProps> = ({
  visible,
  onClose,
  onSuccess,
}) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [departments, setDepartments] = useState<{ id: number; name: string }[]>([]);

  // Step 1: Filters
  const [selectedDept, setSelectedDept] = useState<number | undefined>(undefined);
  const [onlyWithoutAccount, setOnlyWithoutAccount] = useState(true);
  const [onlyMobileDisabled, setOnlyMobileDisabled] = useState(false);
  const [defaultPassword, setDefaultPassword] = useState('123456');
  const [enableMobile, setEnableMobile] = useState(true);

  // Step 2 & 3: Preview Data
  const [previewMetrics, setPreviewMetrics] = useState({
    totalEligible: 0,
    alreadyHaveAccount: 0,
    alreadyEnabled: 0,
    systemAdmin: 0,
    inactive: 0,
    readyToCreate: 0,
  });
  const [candidates, setCandidates] = useState<BulkCandidate[]>([]);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

  // Step 4: Execution Results
  const [resultSummary, setResultSummary] = useState({
    total: 0,
    success: 0,
    alreadyExists: 0,
    skipped: 0,
    failed: 0,
  });
  const [resultsList, setResultsList] = useState<BulkResultItem[]>([]);
  const [resultFilter, setResultFilter] = useState<'ALL' | 'SUCCESS' | 'ALREADY_EXISTS' | 'FAILED'>('ALL');
  const [resultSearch, setResultSearch] = useState('');

  useEffect(() => {
    if (visible) {
      setCurrentStep(0);
      setSelectedRowKeys([]);
      setCandidates([]);
      fetchDepartments();
    }
  }, [visible]);

  const fetchDepartments = async () => {
    try {
      const res = await api.get('/danhmuc/all');
      if (res.data?.phongBan) {
        setDepartments(
          res.data.phongBan.map((p: any) => ({
            id: p.IDPB || p.id,
            name: p.TENPB || p.TENPB_VI || p.name,
          }))
        );
      }
    } catch {
      // Fallback
    }
  };

  // Move from Step 1 to Step 2/3 (Preview)
  const handleFetchPreview = async () => {
    try {
      setLoading(true);
      const res = await api.post('/users/bulk-provision-preview', {
        departmentId: selectedDept,
        onlyWithoutAccount: onlyWithoutAccount,
        onlyMobileDisabled: onlyMobileDisabled,
        status: 'ACTIVE',
      });

      const data = res.data;
      setPreviewMetrics({
        totalEligible: data.TotalEligible || 0,
        alreadyHaveAccount: data.AlreadyHaveAccount || 0,
        alreadyEnabled: data.AlreadyEnabled || 0,
        systemAdmin: data.SystemAdmin || 0,
        inactive: data.Inactive || 0,
        readyToCreate: data.ReadyToCreate || 0,
      });

      const candList: BulkCandidate[] = data.Candidates || [];
      setCandidates(candList);

      // Default select all eligible candidates
      const eligibleKeys = candList
        .filter((c) => c.IsEligible)
        .map((c) => c.EmployeeId);
      setSelectedRowKeys(eligibleKeys);

      setCurrentStep(1);
    } catch (err: any) {
      notification.error({
        message: 'Lỗi nạp dữ liệu',
        description: err.response?.data?.message || 'Không thể xem trước danh sách ứng viên.',
      });
    } finally {
      setLoading(false);
    }
  };

  // Show Confirmation Dialog before executing
  const handleConfirmExecute = () => {
    const selectedCount = selectedRowKeys.length;
    if (selectedCount === 0) {
      message.warning('Vui lòng chọn ít nhất 1 nhân viên để cấp tài khoản.');
      return;
    }

    Modal.confirm({
      title: (
        <Space>
          <SafetyCertificateOutlined style={{ color: '#1677ff', fontSize: 20 }} />
          <span>Xác nhận Cấp phát Tài khoản Hàng loạt</span>
        </Space>
      ),
      width: 520,
      content: (
        <div style={{ marginTop: 12 }}>
          <Paragraph>
            Hệ thống chuẩn bị thực hiện cấp phát tự động cho:{' '}
            <Text strong style={{ color: '#1677ff', fontSize: 16 }}>
              {selectedCount} nhân sự
            </Text>
          </Paragraph>
          <Alert
            message="Chi tiết hoạt động"
            description={
              <ul style={{ paddingLeft: 16, margin: 0, fontSize: 13 }}>
                <li>
                  Tạo tài khoản đăng nhập (LoginName chuẩn hóa <code>NVxxxxxx</code>)
                </li>
                <li>
                  Mật khẩu khởi tạo: <code>{defaultPassword}</code> (mã hóa chuẩn an toàn BCrypt)
                </li>
                <li>
                  Quyền Mobile Access: <Text strong>{enableMobile ? 'Bật (Kích hoạt)' : 'Tắt'}</Text>
                </li>
                <li>
                  Ghi nhật ký kiểm toán hệ thống (Audit Log) lưu vết tự động.
                </li>
              </ul>
            }
            type="info"
            showIcon
            style={{ marginBottom: 12 }}
          />
          <Text type="secondary" style={{ fontSize: 12 }}>
            Lưu ý: Quá trình này an toàn và có tính Bất biến (Idempotency). Các nhân sự đã có tài khoản sẽ không bị ghi đè trùng lặp.
          </Text>
        </div>
      ),
      okText: `Xác nhận cấp phát (${selectedCount})`,
      cancelText: 'Hủy bỏ',
      okButtonProps: { type: 'primary' },
      onOk: () => executeBulkProvision(),
    });
  };

  // Execute Bulk Provisioning API
  const executeBulkProvision = async () => {
    try {
      setLoading(true);
      const res = await api.post('/users/bulk-provision', {
        employeeIds: selectedRowKeys,
        defaultPassword: defaultPassword,
        enableMobile: enableMobile,
        overwriteExisting: false,
      });

      const data = res.data;
      setResultSummary({
        total: data.Total || 0,
        success: data.Success || 0,
        alreadyExists: data.AlreadyExists || 0,
        skipped: data.Skipped || 0,
        failed: data.Failed || 0,
      });

      setResultsList(data.Results || []);
      setCurrentStep(2);
      if (onSuccess) onSuccess();
      notification.success({
        message: 'Hoàn tất Cấp phát Tài khoản',
        description: `Đã cấp thành công ${data.Success} tài khoản mới!`,
      });
    } catch (err: any) {
      notification.error({
        message: 'Lỗi thực thi',
        description: err.response?.data?.message || 'Có lỗi xảy ra trong quá trình cấp phát tài khoản.',
      });
    } finally {
      setLoading(false);
    }
  };

  // Candidate Table Columns (Step 2)
  const candidateColumns: ColumnsType<BulkCandidate> = [
    {
      title: 'Mã NV',
      dataIndex: 'EmployeeCode',
      key: 'EmployeeCode',
      width: 130,
      render: (code: string) => (
        <Space size={4}>
          <IdcardOutlined style={{ color: '#1677ff' }} />
          <Text strong>{code}</Text>
        </Space>
      ),
    },
    {
      title: 'Họ và tên',
      dataIndex: 'FullName',
      key: 'FullName',
      width: 170,
    },
    {
      title: 'Phòng ban',
      dataIndex: 'DepartmentName',
      key: 'DepartmentName',
      width: 150,
      render: (d: string) => <Tag color="blue">{d}</Tag>,
    },
    {
      title: 'Tên đăng nhập gợi ý',
      dataIndex: 'SuggestedLoginName',
      key: 'SuggestedLoginName',
      width: 150,
      render: (login: string, record: BulkCandidate) =>
        record.ExistingAccount ? (
          <Tag color="cyan">{record.ExistingAccount} (Đã có)</Tag>
        ) : (
          <Text code strong>{login}</Text>
        ),
    },
    {
      title: 'Mobile Access',
      dataIndex: 'MobileStatus',
      key: 'MobileStatus',
      width: 130,
      render: (status: string) => {
        if (status === 'ENABLED') return <Tag color="green">Đã bật</Tag>;
        if (status === 'BLOCKED') return <Tag color="red">Bị chặn</Tag>;
        return <Tag color="default">Chưa bật</Tag>;
      },
    },
    {
      title: 'Tình trạng hợp lệ',
      key: 'eligibility',
      width: 220,
      render: (_, record: BulkCandidate) =>
        record.IsEligible ? (
          <Tag color="success" icon={<CheckCircleOutlined />}>
            {record.Reason}
          </Tag>
        ) : (
          <Tag color="warning" icon={<ExclamationCircleOutlined />}>
            {record.Reason}
          </Tag>
        ),
    },
  ];

  // Result Table Columns (Step 3)
  const resultColumns: ColumnsType<BulkResultItem> = [
    {
      title: 'Mã NV',
      dataIndex: 'EmployeeCode',
      key: 'EmployeeCode',
      width: 130,
      render: (code: string) => <Text strong>{code}</Text>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'FullName',
      key: 'FullName',
      width: 170,
    },
    {
      title: 'Tên đăng nhập',
      dataIndex: 'LoginName',
      key: 'LoginName',
      width: 140,
      render: (name: string) => (name ? <Text code strong>{name}</Text> : '-'),
    },
    {
      title: 'Kết quả',
      dataIndex: 'Result',
      key: 'Result',
      width: 140,
      render: (res: string) => {
        if (res === 'SUCCESS') return <Tag color="success" icon={<CheckCircleOutlined />}>Thành công</Tag>;
        if (res === 'ALREADY_EXISTS') return <Tag color="cyan" icon={<SyncOutlined />}>Đã tồn tại</Tag>;
        if (res === 'SKIPPED_INACTIVE' || res === 'SKIPPED_SYSTEM') return <Tag color="orange" icon={<ExclamationCircleOutlined />}>Bỏ qua</Tag>;
        return <Tag color="error" icon={<CloseCircleOutlined />}>Thất bại</Tag>;
      },
    },
    {
      title: 'Thông báo chi tiết',
      dataIndex: 'Message',
      key: 'Message',
    },
  ];

  // Filtered Results
  const filteredResults = resultsList.filter((item) => {
    if (resultFilter !== 'ALL' && item.Result !== resultFilter) return false;
    if (resultSearch) {
      const q = resultSearch.toLowerCase();
      return (
        item.EmployeeCode.toLowerCase().includes(q) ||
        item.FullName.toLowerCase().includes(q) ||
        (item.LoginName && item.LoginName.toLowerCase().includes(q))
      );
    }
    return true;
  });

  return (
    <Modal
      title={
        <Space>
          <UserAddOutlined style={{ color: '#1677ff' }} />
          <span>Cấp phát Tài khoản Nhân viên Hàng loạt (Bulk Provisioning)</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width="min(960px, 95vw)"
      footer={null}
      destroyOnClose
    >
      <Steps
        current={currentStep}
        style={{ marginBottom: 24 }}
        items={[
          { title: 'Tiêu chí lọc', icon: <FilterOutlined /> },
          { title: 'Xem trước & Chọn', icon: <IdcardOutlined /> },
          { title: 'Báo cáo kết quả', icon: <CheckCircleOutlined /> },
        ]}
      />

      {/* STEP 1: FILTERS */}
      {currentStep === 0 && (
        <div>
          <Alert
            message="Chính sách Cấp phát Tài khoản Mobile ESS Tự động"
            description="Chức năng này cho phép Quản trị viên khởi tạo tài khoản đăng nhập hàng loạt cho toàn bộ nhân sự chưa có tài khoản, tự động kích hoạt quyền Mobile Access và đảm bảo tính bất biến (Idempotency)."
            type="info"
            showIcon
            style={{ marginBottom: 20 }}
          />

          <Form layout="vertical">
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item label="Phòng ban trực thuộc">
                  <Select
                    placeholder="Tất cả phòng ban"
                    allowClear
                    value={selectedDept}
                    onChange={(val) => setSelectedDept(val)}
                    options={departments.map((d) => ({ value: d.id, label: d.name }))}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="Mật khẩu khởi tạo mặc định">
                  <Input.Password
                    value={defaultPassword}
                    onChange={(e) => setDefaultPassword(e.target.value)}
                    placeholder="Nhập mật khẩu mặc định (ví dụ: 123456)"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Divider style={{ margin: '12px 0 16px 0' }} />

            <Form.Item label="Điều kiện lọc nâng cao">
              <Space direction="vertical" size="small">
                <Checkbox
                  checked={onlyWithoutAccount}
                  onChange={(e) => setOnlyWithoutAccount(e.target.checked)}
                >
                  <Text strong>Chỉ cấp cho nhân viên chưa có tài khoản</Text>{' '}
                  <Text type="secondary">(Khuyến nghị để tránh trùng lặp)</Text>
                </Checkbox>
                <Checkbox
                  checked={onlyMobileDisabled}
                  onChange={(e) => setOnlyMobileDisabled(e.target.checked)}
                >
                  Bao gồm nhân viên đã có tài khoản nhưng chưa bật Mobile Access
                </Checkbox>
                <Checkbox
                  checked={enableMobile}
                  onChange={(e) => setEnableMobile(e.target.checked)}
                >
                  Tự động kích hoạt quyền Mobile Access cho các tài khoản được tạo
                </Checkbox>
              </Space>
            </Form.Item>
          </Form>

          <div style={{ textAlign: 'right', marginTop: 24 }}>
            <Button onClick={onClose} style={{ marginRight: 8 }}>
              Hủy bỏ
            </Button>
            <Button type="primary" onClick={handleFetchPreview} loading={loading}>
              Tiếp tục: Xem trước ứng viên
            </Button>
          </div>
        </div>
      )}

      {/* STEP 2: PREVIEW & SELECTION */}
      {currentStep === 1 && (
        <div>
          <Row gutter={12} style={{ marginBottom: 16 }}>
            <Col span={4}>
              <Card size="small" style={{ textAlign: 'center', background: '#f6ffed', borderColor: '#b7eb8f' }}>
                <Statistic title="Đủ điều kiện" value={previewMetrics.totalEligible} valueStyle={{ color: '#389e0d' }} />
              </Card>
            </Col>
            <Col span={4}>
              <Card size="small" style={{ textAlign: 'center', background: '#e6f4ff', borderColor: '#91caff' }}>
                <Statistic title="Sẵn sàng tạo" value={previewMetrics.readyToCreate} valueStyle={{ color: '#1677ff' }} />
              </Card>
            </Col>
            <Col span={4}>
              <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                <Statistic title="Đã có TK" value={previewMetrics.alreadyHaveAccount} />
              </Card>
            </Col>
            <Col span={4}>
              <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                <Statistic title="Đã bật Mobile" value={previewMetrics.alreadyEnabled} />
              </Card>
            </Col>
            <Col span={4}>
              <Card size="small" style={{ textAlign: 'center', background: '#fff2e8', borderColor: '#ffbb96' }}>
                <Statistic title="Admin/System" value={previewMetrics.systemAdmin} valueStyle={{ color: '#fa541c' }} />
              </Card>
            </Col>
            <Col span={4}>
              <Card size="small" style={{ textAlign: 'center', background: '#fff1f0', borderColor: '#ffa39e' }}>
                <Statistic title="Thôi việc" value={previewMetrics.inactive} valueStyle={{ color: '#cf1322' }} />
              </Card>
            </Col>
          </Row>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
            <Text strong>
              Đã chọn: <Text style={{ color: '#1677ff' }}>{selectedRowKeys.length}</Text> / {candidates.length} nhân sự
            </Text>
            <Space>
              <Button
                size="small"
                onClick={() => {
                  const eligibleKeys = candidates.filter((c) => c.IsEligible).map((c) => c.EmployeeId);
                  setSelectedRowKeys(eligibleKeys);
                }}
              >
                Chọn tất cả hợp lệ
              </Button>
              <Button size="small" onClick={() => setSelectedRowKeys([])}>
                Bỏ chọn tất cả
              </Button>
            </Space>
          </div>

          <Table
            size="small"
            rowKey="EmployeeId"
            columns={candidateColumns}
            dataSource={candidates}
            rowSelection={{
              selectedRowKeys,
              onChange: (keys) => setSelectedRowKeys(keys),
              getCheckboxProps: (record) => ({
                disabled: !record.IsEligible,
              }),
            }}
            pagination={{ pageSize: 8, showSizeChanger: false }}
            scroll={{ y: 320 }}
          />

          <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 20 }}>
            <Button onClick={() => setCurrentStep(0)}>Quay lại bước 1</Button>
            <Space>
              <Button onClick={onClose}>Đóng</Button>
              <Button
                type="primary"
                onClick={handleConfirmExecute}
                loading={loading}
                disabled={selectedRowKeys.length === 0}
              >
                Tiến hành cấp phát ({selectedRowKeys.length} tài khoản)
              </Button>
            </Space>
          </div>
        </div>
      )}

      {/* STEP 3: RESULTS SUMMARY */}
      {currentStep === 2 && (
        <div>
          <Alert
            message="Quá trình cấp phát đã hoàn tất!"
            description={`Đã xử lý tổng cộng ${resultSummary.total} nhân sự. Thành công: ${resultSummary.success}, Đã có sẵn: ${resultSummary.alreadyExists}, Bỏ qua: ${resultSummary.skipped}, Lỗi: ${resultSummary.failed}.`}
            type={resultSummary.failed === 0 ? 'success' : 'warning'}
            showIcon
            style={{ marginBottom: 16 }}
          />

          <Row gutter={12} style={{ marginBottom: 16 }}>
            <Col span={6}>
              <Card size="small" style={{ textAlign: 'center', background: '#f6ffed', borderColor: '#b7eb8f' }}>
                <Statistic title="Thành công" value={resultSummary.success} valueStyle={{ color: '#389e0d' }} />
              </Card>
            </Col>
            <Col span={6}>
              <Card size="small" style={{ textAlign: 'center', background: '#e6f4ff', borderColor: '#91caff' }}>
                <Statistic title="Đã có sẵn" value={resultSummary.alreadyExists} valueStyle={{ color: '#1677ff' }} />
              </Card>
            </Col>
            <Col span={6}>
              <Card size="small" style={{ textAlign: 'center', background: '#fffbe6', borderColor: '#ffe58f' }}>
                <Statistic title="Bỏ qua" value={resultSummary.skipped} valueStyle={{ color: '#d48806' }} />
              </Card>
            </Col>
            <Col span={6}>
              <Card size="small" style={{ textAlign: 'center', background: '#fff1f0', borderColor: '#ffa39e' }}>
                <Statistic title="Thất bại" value={resultSummary.failed} valueStyle={{ color: '#cf1322' }} />
              </Card>
            </Col>
          </Row>

          <Space style={{ marginBottom: 12, width: '100%', justifyContent: 'space-between' }}>
            <Space>
              <Select
                value={resultFilter}
                onChange={(val) => setResultFilter(val)}
                style={{ width: 160 }}
                options={[
                  { value: 'ALL', label: 'Tất cả kết quả' },
                  { value: 'SUCCESS', label: 'Chỉ thành công' },
                  { value: 'ALREADY_EXISTS', label: 'Chỉ đã tồn tại' },
                  { value: 'FAILED', label: 'Chỉ thất bại' },
                ]}
              />
              <Input
                placeholder="Tìm mã NV, tên, login..."
                value={resultSearch}
                onChange={(e) => setResultSearch(e.target.value)}
                style={{ width: 220 }}
              />
            </Space>

            <Button
              icon={<CopyOutlined />}
              onClick={() => {
                const text = resultsList
                  .map((r) => `${r.EmployeeCode}\t${r.FullName}\t${r.LoginName}\t${r.Result}\t${r.Message}`)
                  .join('\n');
                navigator.clipboard.writeText(`Mã NV\tHọ tên\tLoginName\tKết quả\tChi tiết\n${text}`);
                message.success('Đã sao chép danh sách kết quả vào clipboard!');
              }}
            >
              Sao chép kết quả
            </Button>
          </Space>

          <Table
            size="small"
            rowKey="EmployeeId"
            columns={resultColumns}
            dataSource={filteredResults}
            pagination={{ pageSize: 8, showSizeChanger: false }}
            scroll={{ y: 280 }}
          />

          <div style={{ textAlign: 'right', marginTop: 20 }}>
            <Button type="primary" onClick={onClose}>
              Đóng cửa sổ
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
};

export default BulkProvisioningModal;
