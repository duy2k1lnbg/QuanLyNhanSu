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
import { useAppLanguage } from '../services/i18n';

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
  const { t } = useAppLanguage();
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
      // fallback
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, action, moduleFilter, dateRange]);

  useEffect(() => {
    fetchFilterOptions();
    fetchLogs(1, pageSize);
  }, []);

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
      title: t('audit.colTimestamp'),
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 160,
      render: (val: string) => <Text style={{ fontSize: 13, fontFamily: 'monospace' }}>{val}</Text>,
    },
    {
      title: t('audit.colActor'),
      dataIndex: 'username',
      key: 'username',
      width: 140,
      render: (val: string, record: AuditLogItem) => (
        <Space direction="vertical" size={0}>
          <Text strong style={{ fontSize: 13 }}>{val || 'System'}</Text>
          {record.userId ? (
            <Text type="secondary" style={{ fontSize: 11 }}>#{record.userId}</Text>
          ) : null}
        </Space>
      ),
    },
    {
      title: t('audit.colAction'),
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
      title: t('audit.colTarget'),
      key: 'moduleTable',
      width: 160,
      render: (_: any, record: AuditLogItem) => (
        <Space direction="vertical" size={0}>
          <Tag color="geekblue" style={{ margin: 0, fontSize: 11 }}>{record.module || '-'}</Tag>
          <Text type="secondary" style={{ fontSize: 12, fontFamily: 'monospace' }}>{record.tableName}</Text>
        </Space>
      ),
    },
    {
      title: t('audit.colIpAddress'),
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
      title: t('audit.colDetails'),
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
      title: t('common.actions'),
      key: 'actionBtn',
      width: 80,
      fixed: 'right' as const,
      align: 'center' as const,
      render: (_: any, record: AuditLogItem) => (
        <Button
          type="text"
          icon={<EyeOutlined />}
          onClick={() => handleViewDetail(record)}
          size="small"
        />
      ),
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Card
        title={
          <Space>
            <AuditOutlined style={{ color: '#1890ff', fontSize: 20 }} />
            <span>{t('audit.pageTitle')}</span>
          </Space>
        }
        bordered={false}
        style={{ borderRadius: 8 }}
      >
        {/* Filters */}
        <div
          style={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 12,
            marginBottom: 16,
            alignItems: 'center',
          }}
        >
          <Input
            placeholder={t('common.searchPlaceholder')}
            prefix={<SearchOutlined />}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onPressEnter={handleSearch}
            style={{ width: 220 }}
            allowClear
          />

          <Select
            value={action}
            onChange={(val) => setAction(val)}
            style={{ width: 140 }}
            options={[
              { value: 'ALL', label: t('audit.filterAction') },
              ...actionOptions.map((act) => ({ value: act, label: act })),
            ]}
          />

          <Select
            value={moduleFilter}
            onChange={(val) => setModuleFilter(val)}
            style={{ width: 150 }}
            options={[
              { value: 'ALL', label: t('common.all') },
              ...moduleOptions.map((mod) => ({ value: mod, label: mod })),
            ]}
          />

          <RangePicker
            value={dateRange}
            onChange={(val) => setDateRange(val)}
            format="YYYY-MM-DD"
            style={{ minWidth: 230 }}
          />

          <Button type="primary" icon={<SearchOutlined />} onClick={handleSearch}>
            {t('common.search')}
          </Button>

          <Button
            icon={<ReloadOutlined />}
            onClick={() => {
              setSearch('');
              setAction('ALL');
              setModuleFilter('ALL');
              setDateRange(null);
              fetchLogs(1, pageSize);
            }}
          >
            {t('common.reset')}
          </Button>
        </div>

        {/* Audit Log Table */}
        <Table
          columns={columns}
          dataSource={logs}
          rowKey="id"
          loading={loading}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (totalCount) => t('common.totalRecords', { total: totalCount }),
            onChange: (p, ps) => {
              setPage(p);
              setPageSize(ps);
              fetchLogs(p, ps);
            },
          }}
          size="middle"
          scroll={{ x: 'max-content' }}
        />
      </Card>

      {/* Detail Modal */}
      <Modal
        title={
          <Space>
            <AuditOutlined style={{ color: '#1890ff' }} />
            <span>{t('audit.colDetails')} #{selectedLog?.id}</span>
          </Space>
        }
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={[
          <Button key="close" type="primary" onClick={() => setDetailModalVisible(false)}>
            {t('common.close')}
          </Button>,
        ]}
        width="min(750px, 95vw)"
      >
        {detailLoading ? (
          <div style={{ textAlign: 'center', padding: '30px 0' }}>{t('common.loading')}</div>
        ) : selectedLog ? (
          <Space direction="vertical" size={16} style={{ width: '100%' }}>
            <Descriptions bordered size="small" column={{ xs: 1, sm: 2 }}>
              <Descriptions.Item label={t('audit.colTimestamp')}>{selectedLog.timestamp}</Descriptions.Item>
              <Descriptions.Item label={t('audit.colAction')}>
                <Tag color={getActionColor(selectedLog.action)} style={{ fontWeight: 600 }}>
                  {selectedLog.action}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label={t('audit.colActor')}>
                <Text strong>{selectedLog.username}</Text> {selectedLog.userId ? `(ID: ${selectedLog.userId})` : ''}
              </Descriptions.Item>
              <Descriptions.Item label={t('audit.colTarget')}>
                <Tag color="geekblue">{selectedLog.module}</Tag> / <code>{selectedLog.tableName}</code>
              </Descriptions.Item>
              <Descriptions.Item label={t('audit.colIpAddress')}>{selectedLog.ipAddress || '127.0.0.1'}</Descriptions.Item>
              {selectedLog.computerName && (
                <Descriptions.Item label="Host">{selectedLog.computerName}</Descriptions.Item>
              )}
              {selectedLog.changedFields && (
                <Descriptions.Item label={t('audit.colDetails')} span={2}>
                  <Text code>{selectedLog.changedFields}</Text>
                </Descriptions.Item>
              )}
            </Descriptions>

            {/* Old vs New data */}
            {selectedLog.oldData && (
              <div>
                <Text strong style={{ color: '#d46b08' }}>Old Data:</Text>
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
                <Text strong style={{ color: '#389e0d' }}>New Data:</Text>
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
