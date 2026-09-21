import { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Table,
  Tag,
  Button,
  Space,
  Input,
  Select,
  DatePicker,
  Modal,
  Descriptions,
  Typography,
  message,
  Tooltip,
} from 'antd';
import {
  AuditOutlined,
  SearchOutlined,
  ReloadOutlined,
  EyeOutlined,
  LaptopOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import dayjs from 'dayjs';

const { Text } = Typography;
const { RangePicker } = DatePicker;

interface AuditLogItem {
  id: number;
  userId?: number;
  username: string;
  action: string;
  tableName: string;
  recordId: string;
  module: string;
  ipAddress: string;
  computerName?: string;
  timestamp: string;
  changedFields?: string;
  appVersion?: string;
}

interface AuditLogDetail extends AuditLogItem {
  macAddress?: string;
  oldData?: string;
  newData?: string;
  sessionId?: string;
}

export function AuditLogPage() {
  const [logs, setLogs] = useState<AuditLogItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // Filters
  const [search, setSearch] = useState('');
  const [action, setAction] = useState('ALL');
  const [moduleFilter, setModuleFilter] = useState('ALL');
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>(null);

  // Module & Action options
  const [moduleOptions, setModuleOptions] = useState<string[]>([]);
  const [actionOptions, setActionOptions] = useState<string[]>([]);

  // Detail modal
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [selectedLog, setSelectedLog] = useState<AuditLogDetail | null>(null);

  // Load modules & actions
  const fetchFilterOptions = async () => {
    try {
      const res = await api.get('/audit/modules');
      if (res.data && res.data.success) {
        setModuleOptions(res.data.modules || []);
        setActionOptions(res.data.actions || []);
      }
    } catch {
      // ignore
    }
  };

  const fetchLogs = useCallback(async (targetPage = page, targetPageSize = pageSize) => {
    setLoading(true);
    try {
      const params: Record<string, any> = {
        page: targetPage,
        pageSize: targetPageSize,
      };
      if (search.trim()) params.search = search.trim();
      if (action && action !== 'ALL') params.action = action;
      if (moduleFilter && moduleFilter !== 'ALL') params.module = moduleFilter;
      if (dateRange && dateRange[0]) params.fromDate = dateRange[0].format('YYYY-MM-DD');
      if (dateRange && dateRange[1]) params.toDate = dateRange[1].format('YYYY-MM-DD');

      const res = await api.get('/audit', { params });
      if (res.data && res.data.success) {
        setLogs(res.data.data || []);
        setTotal(res.data.total || 0);
      }
    } catch {
      message.error('Không thể tải nhật ký kiểm toán. Vui lòng thử lại!');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, action, moduleFilter, dateRange]);

  useEffect(() => {
    fetchFilterOptions();
  }, []);

  useEffect(() => {
    fetchLogs(1, pageSize);
    setPage(1);
  }, [action, moduleFilter, dateRange]);

  const handleSearch = () => {
    fetchLogs(1, pageSize);
    setPage(1);
  };

  const handleViewDetail = async (record: AuditLogItem) => {
    setDetailLoading(true);
    setDetailModalVisible(true);
    setSelectedLog(null);
    try {
      const res = await api.get(`/audit/${record.id}`);
      if (res.data && res.data.success) {
        setSelectedLog(res.data.data);
      } else {
        setSelectedLog(record as AuditLogDetail);
      }
    } catch {
      setSelectedLog(record as AuditLogDetail);
    } finally {
      setDetailLoading(false);
    }
  };

  const getActionColor = (act: string) => {
    switch (act?.toUpperCase()) {
      case 'INSERT':
      case 'CREATE':
      case 'ADD':
        return 'success';
      case 'UPDATE':
      case 'EDIT':
        return 'processing';
      case 'DELETE':
      case 'REMOVE':
        return 'error';
      case 'LOGIN':
        return 'purple';
      case 'LOGOUT':
        return 'default';
      default:
        return 'blue';
    }
  };

  const columns = [
    {
      title: 'Thời gian',
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 160,
      render: (val: string) => <Text style={{ fontSize: 13, fontFamily: 'monospace' }}>{val}</Text>,
    },
    {
      title: 'Người thực hiện',
      dataIndex: 'username',
      key: 'username',
      width: 140,
      render: (val: string, record: AuditLogItem) => (
        <Space direction="vertical" size={0}>
          <Text strong style={{ fontSize: 13 }}>{val || 'System'}</Text>
          {record.userId ? (
            <Text type="secondary" style={{ fontSize: 11 }}>NV#{record.userId}</Text>
          ) : null}
        </Space>
      ),
    },
    {
      title: 'Hành động',
      dataIndex: 'action',
      key: 'action',
      width: 110,
      render: (val: string) => (
        <Tag color={getActionColor(val)} style={{ fontWeight: 600 }}>
          {val || 'UNKNOWN'}
        </Tag>
      ),
    },
    {
      title: 'Phân hệ / Bảng',
      key: 'moduleTable',
      width: 160,
      render: (_: any, record: AuditLogItem) => (
        <Space direction="vertical" size={0}>
          <Tag color="geekblue" style={{ margin: 0, fontSize: 11 }}>{record.module || 'Hệ thống'}</Tag>
          <Text type="secondary" style={{ fontSize: 12, fontFamily: 'monospace' }}>{record.tableName}</Text>
        </Space>
      ),
    },
    {
      title: 'Mã bản ghi',
      dataIndex: 'recordId',
      key: 'recordId',
      width: 110,
      render: (val: string) => <Tag style={{ fontFamily: 'monospace' }}>{val || '-'}</Tag>,
    },
    {
      title: 'IP / Thiết bị',
      key: 'ipDevice',
      width: 150,
      render: (_: any, record: AuditLogItem) => (
        <Space direction="vertical" size={0}>
          <Text style={{ fontSize: 12 }}>{record.ipAddress || '127.0.0.1'}</Text>
          {record.computerName && (
            <Text type="secondary" style={{ fontSize: 11 }}>
              <LaptopOutlined style={{ marginRight: 4 }} />
              {record.computerName}
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Trường thay đổi',
      dataIndex: 'changedFields',
      key: 'changedFields',
      ellipsis: true,
      render: (val: string) => (
        <Tooltip title={val}>
          <Text type="secondary" style={{ fontSize: 12 }}>{val || '-'}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Thao tác',
      key: 'actionBtn',
      width: 80,
      align: 'center' as const,
      render: (_: any, record: AuditLogItem) => (
        <Button
          type="text"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => handleViewDetail(record)}
        >
          Chi tiết
        </Button>
      ),
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Header card */}
      <Card
        style={{
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0,0,0,0.04)',
        }}
        bodyStyle={{ padding: '16px 20px' }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 12 }}>
          <Space size={12}>
            <div
              style={{
                width: 40,
                height: 40,
                borderRadius: 10,
                background: 'linear-gradient(135deg, #1890ff, #096dd9)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#fff',
                fontSize: 20,
              }}
            >
              <AuditOutlined />
            </div>
            <div>
              <Text strong style={{ fontSize: 18 }}>Nhật ký kiểm toán hệ thống (Audit Trail)</Text>
              <div>
                <Text type="secondary" style={{ fontSize: 13 }}>
                  Theo dõi, truy vết toàn bộ hoạt động thêm, sửa, xóa, đăng nhập và bảo mật trên CSDL Oracle
                </Text>
              </div>
            </div>
          </Space>

          <Button
            icon={<ReloadOutlined spin={loading} />}
            onClick={() => fetchLogs(page, pageSize)}
          >
            Làm mới
          </Button>
        </div>

        {/* Filter bar */}
        <div style={{ marginTop: 16, display: 'flex', flexWrap: 'wrap', gap: 10, alignItems: 'center' }}>
          <Input
            placeholder="Tìm theo người dùng, mã bản ghi, IP..."
            prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
            style={{ width: 260 }}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onPressEnter={handleSearch}
            allowClear
          />

          <Select
            value={action}
            onChange={setAction}
            style={{ width: 140 }}
            placeholder="Hành động"
          >
            <Select.Option value="ALL">Tất cả hành động</Select.Option>
            {actionOptions.map((act) => (
              <Select.Option key={act} value={act}>
                {act}
              </Select.Option>
            ))}
          </Select>

          <Select
            value={moduleFilter}
            onChange={setModuleFilter}
            style={{ width: 170 }}
            placeholder="Phân hệ"
          >
            <Select.Option value="ALL">Tất cả phân hệ</Select.Option>
            {moduleOptions.map((mod) => (
              <Select.Option key={mod} value={mod}>
                {mod}
              </Select.Option>
            ))}
          </Select>

          <RangePicker
            placeholder={['Từ ngày', 'Đến ngày']}
            format="DD/MM/YYYY"
            value={dateRange}
            onChange={(dates) => setDateRange(dates as any)}
            style={{ width: 240 }}
          />

          <Button type="primary" icon={<SearchOutlined />} onClick={handleSearch}>
            Lọc
          </Button>
        </div>
      </Card>

      {/* Table Card */}
      <Card
        style={{
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0,0,0,0.04)',
        }}
        bodyStyle={{ padding: 0 }}
      >
        <Table
          columns={columns}
          dataSource={logs}
          rowKey="id"
          loading={loading}
          pagination={{
            current: page,
            pageSize: pageSize,
            total: total,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
            showTotal: (tot) => `Tổng cộng ${tot} bản ghi kiểm toán`,
            onChange: (p, ps) => {
              setPage(p);
              setPageSize(ps);
              fetchLogs(p, ps);
            },
          }}
          size="middle"
          scroll={{ x: 950 }}
        />
      </Card>

      {/* Detail Modal */}
      <Modal
        title={
          <Space>
            <AuditOutlined style={{ color: '#1890ff' }} />
            <span>Chi tiết nhật ký kiểm toán #{selectedLog?.id}</span>
          </Space>
        }
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={[
          <Button key="close" type="primary" onClick={() => setDetailModalVisible(false)}>
            Đóng
          </Button>,
        ]}
        width={750}
      >
        {detailLoading ? (
          <div style={{ textAlign: 'center', padding: '30px 0' }}>Đang tải chi tiết...</div>
        ) : selectedLog ? (
          <Space direction="vertical" size={16} style={{ width: '100%' }}>
            <Descriptions bordered size="small" column={{ xs: 1, sm: 2 }}>
              <Descriptions.Item label="Thời gian">{selectedLog.timestamp}</Descriptions.Item>
              <Descriptions.Item label="Hành động">
                <Tag color={getActionColor(selectedLog.action)} style={{ fontWeight: 600 }}>
                  {selectedLog.action}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Người thực hiện">
                <Text strong>{selectedLog.username}</Text> {selectedLog.userId ? `(ID: ${selectedLog.userId})` : ''}
              </Descriptions.Item>
              <Descriptions.Item label="Phân hệ / Bảng">
                <Tag color="geekblue">{selectedLog.module}</Tag> / <code>{selectedLog.tableName}</code>
              </Descriptions.Item>
              <Descriptions.Item label="Mã bản ghi">
                <Tag>{selectedLog.recordId || '-'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Địa chỉ IP">{selectedLog.ipAddress || '127.0.0.1'}</Descriptions.Item>
              {selectedLog.macAddress && (
                <Descriptions.Item label="Địa chỉ MAC">{selectedLog.macAddress}</Descriptions.Item>
              )}
              {selectedLog.computerName && (
                <Descriptions.Item label="Tên máy">{selectedLog.computerName}</Descriptions.Item>
              )}
              {selectedLog.appVersion && (
                <Descriptions.Item label="Phiên bản">{selectedLog.appVersion}</Descriptions.Item>
              )}
              {selectedLog.sessionId && (
                <Descriptions.Item label="Session ID">{selectedLog.sessionId}</Descriptions.Item>
              )}
              {selectedLog.changedFields && (
                <Descriptions.Item label="Trường thay đổi" span={2}>
                  <Text code>{selectedLog.changedFields}</Text>
                </Descriptions.Item>
              )}
            </Descriptions>

            {/* Old vs New data */}
            {selectedLog.oldData && (
              <div>
                <Text strong style={{ color: '#d46b08' }}>Dữ liệu cũ (Trước khi sửa / Trước khi xóa):</Text>
                <pre
                  style={{
                    backgroundColor: '#fffbe6',
                    padding: 12,
                    borderRadius: 6,
                    border: '1px solid #ffe58f',
                    maxHeight: 200,
                    overflow: 'auto',
                    fontSize: 12,
                    marginTop: 6,
                  }}
                >
                  {selectedLog.oldData}
                </pre>
              </div>
            )}

            {selectedLog.newData && (
              <div>
                <Text strong style={{ color: '#389e0d' }}>Dữ liệu mới (Sau khi thêm / Sau khi sửa):</Text>
                <pre
                  style={{
                    backgroundColor: '#f6ffed',
                    padding: 12,
                    borderRadius: 6,
                    border: '1px solid #b7eb8f',
                    maxHeight: 200,
                    overflow: 'auto',
                    fontSize: 12,
                    marginTop: 6,
                  }}
                >
                  {selectedLog.newData}
                </pre>
              </div>
            )}
          </Space>
        ) : null}
      </Modal>
    </div>
  );
}

export default AuditLogPage;
