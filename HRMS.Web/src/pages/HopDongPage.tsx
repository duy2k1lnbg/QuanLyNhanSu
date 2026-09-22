import { useState, useMemo } from 'react';
import {
  Card,
  Table,
  Tag,
  Space,
  Button,
  Typography,
  theme,
  Input,
  Select,
  Row,
  Col,
  Statistic,
  Drawer,
  Descriptions,
  Divider,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  ReloadOutlined,
  SearchOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  EyeOutlined,
  PrinterOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import type { HopDongDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

const { Text } = Typography;

interface HopDongPageProps {
  hopDongList: HopDongDTO[];
  hopDongLoading: boolean;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
}

export const formatContractDate = (d?: string | null): string => {
  if (!d || !d.trim()) return '-';
  const trimmed = d.trim();
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(trimmed)) {
    return trimmed;
  }
  const parsed = dayjs(trimmed);
  return parsed.isValid() ? parsed.format('YYYY-MM-DD') : trimmed;
};

export const checkIsContractExpired = (endDateStr?: string | null): boolean => {
  if (!endDateStr || !endDateStr.trim()) return false;
  const trimmed = endDateStr.trim();
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(trimmed)) {
    const [day, month, year] = trimmed.split('/').map(Number);
    const end = dayjs(new Date(year, month - 1, day));
    return end.isBefore(dayjs(), 'day');
  }
  const parsed = dayjs(trimmed);
  return parsed.isValid() ? parsed.isBefore(dayjs(), 'day') : false;
};

export function HopDongPage({
  hopDongList,
  hopDongLoading,
  onRefresh,
  canPrint,
}: HopDongPageProps) {
  const { t } = useAppLanguage();
  const {
    token: { borderRadiusLG, colorPrimary },
  } = theme.useToken();

  const [searchText, setSearchText] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [selectedHopDong, setSelectedHopDong] = useState<HopDongDTO | null>(null);

  const stats = useMemo(() => {
    let active = 0;
    let expired = 0;
    let indefinite = 0;

    for (const hd of hopDongList) {
      if (!hd.NGAYKETTHUC || !hd.NGAYKETTHUC.trim()) {
        indefinite++;
        active++;
      } else if (checkIsContractExpired(hd.NGAYKETTHUC)) {
        expired++;
      } else {
        active++;
      }
    }

    return { total: hopDongList.length, active, expired, indefinite };
  }, [hopDongList]);

  const filteredList = useMemo(() => {
    let result = hopDongList;

    if (searchText.trim()) {
      const q = searchText.trim().toLowerCase();
      result = result.filter(
        (hd) =>
          (hd.SOHD && hd.SOHD.toLowerCase().includes(q)) ||
          (hd.HOTEN && hd.HOTEN.toLowerCase().includes(q)) ||
          (hd.MANV && hd.MANV.toString().includes(q)) ||
          (hd.CCCD && hd.CCCD.toLowerCase().includes(q))
      );
    }

    if (statusFilter === 'active') {
      result = result.filter((hd) => !checkIsContractExpired(hd.NGAYKETTHUC));
    } else if (statusFilter === 'expired') {
      result = result.filter(
        (hd) => hd.NGAYKETTHUC && hd.NGAYKETTHUC.trim() && checkIsContractExpired(hd.NGAYKETTHUC)
      );
    } else if (statusFilter === 'indefinite') {
      result = result.filter((hd) => !hd.NGAYKETTHUC || !hd.NGAYKETTHUC.trim());
    }

    return result;
  }, [hopDongList, searchText, statusFilter]);

  const hopDongColumns: ColumnsType<HopDongDTO> = [
    {
      title: t('contract.colContractNo'),
      dataIndex: 'SOHD',
      key: 'SOHD',
      render: (sohd: string) => <Tag color="blue" style={{ fontWeight: 600 }}>{sohd}</Tag>,
    },
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'MANV',
      key: 'MANV',
      width: 80,
      render: (id: number) => <Tag color="cyan">#{id}</Tag>,
    },
    {
      title: t('contract.colEmployee'),
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (text: string, r) => (
        <Space direction="vertical" size={0}>
          <Text strong>{text || '-'}</Text>
          {r.CCCD && <Text type="secondary" style={{ fontSize: 11 }}>CCCD: {r.CCCD}</Text>}
        </Space>
      ),
    },
    {
      title: t('contract.colStartDate'),
      dataIndex: 'NGAYBATDAU',
      key: 'NGAYBATDAU',
      render: (d: string) => formatContractDate(d),
    },
    {
      title: t('contract.colEndDate'),
      dataIndex: 'NGAYKETTHUC',
      key: 'NGAYKETTHUC',
      render: (d: string) => {
        if (!d || !d.trim()) {
          return <Tag color="green">{t('contract.statusActive')}</Tag>;
        }
        return formatContractDate(d);
      },
    },
    {
      title: t('contract.labelDuration'),
      dataIndex: 'THOIHAN',
      key: 'THOIHAN',
      render: (th: string) => th || '-',
    },
    {
      title: t('contract.colSalary'),
      dataIndex: 'HESOLUONG',
      key: 'HESOLUONG',
      render: (h: number) => {
        if (!h) return '-';
        if (h > 1000) {
          return <Tag color="geekblue">{h.toLocaleString('vi-VN')} đ</Tag>;
        }
        return <Tag color="geekblue">{h}x</Tag>;
      },
    },
    {
      title: t('contract.colSignTimes'),
      dataIndex: 'LANKY',
      key: 'LANKY',
      width: 75,
      align: 'center',
      render: (l: number) => <Tag color="default">#{l ?? 1}</Tag>,
    },
    {
      title: t('contract.colStatus'),
      key: 'status',
      width: 120,
      render: (_, r) => {
        if (!r.NGAYKETTHUC || !r.NGAYKETTHUC.trim()) {
          return <Tag color="success">{t('contract.statusActive')}</Tag>;
        }
        const expired = checkIsContractExpired(r.NGAYKETTHUC);
        return expired ? (
          <Tag color="error">{t('contract.statusExpired')}</Tag>
        ) : (
          <Tag color="processing">{t('contract.statusActive')}</Tag>
        );
      },
    },
    {
      title: t('common.actions'),
      key: 'action',
      width: 90,
      align: 'center',
      render: (_, r) => (
        <Button
          type="link"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => setSelectedHopDong(r)}
        >
          {t('common.view')}
        </Button>
      ),
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* 4 Thẻ chỉ số tổng quan */}
      <Row gutter={[16, 16]}>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title={t('dashboard.statContracts')}
              value={stats.total}
              prefix={<FileTextOutlined style={{ color: colorPrimary }} />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title={t('contract.statusActive')}
              value={stats.active}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title={t('contract.statusExpired')}
              value={stats.expired}
              valueStyle={{ color: stats.expired > 0 ? '#ff4d4f' : '#8c8c8c' }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title={t('contract.statusActive')}
              value={stats.indefinite}
              valueStyle={{ color: '#1677ff' }}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Bảng danh sách hợp đồng */}
      <Card
        title={`${t('contract.pageTitle')} (${filteredList.length}/${hopDongList.length})`}
        extra={
          <Space wrap>
            <Input
              placeholder={t('contract.searchPlaceholder')}
              prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              allowClear
              style={{ minWidth: 220 }}
            />
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ minWidth: 150 }}
              options={[
                { value: 'all', label: t('common.all') },
                { value: 'active', label: t('contract.statusActive') },
                { value: 'expired', label: t('contract.statusExpired') },
                { value: 'indefinite', label: t('contract.statusActive') },
              ]}
            />
            <Button icon={<ReloadOutlined />} onClick={onRefresh} loading={hopDongLoading}>
              {t('common.refresh')}
            </Button>
          </Space>
        }
        bordered={false}
        style={{ borderRadius: borderRadiusLG }}
      >
        <Table
          columns={hopDongColumns}
          dataSource={filteredList}
          rowKey="SOHD"
          loading={hopDongLoading}
          scroll={{ x: 'max-content' }}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
            showTotal: (total) => t('common.totalRecords', { total }),
          }}
        />
      </Card>

      {/* Drawer Chi tiết hợp đồng lao động */}
      <Drawer
        title={`${t('contract.pageTitle')}: ${selectedHopDong?.SOHD || ''}`}
        placement="right"
        width="min(540px, 95vw)"
        open={!!selectedHopDong}
        onClose={() => setSelectedHopDong(null)}
        extra={
          (!canPrint || canPrint('NV', 'F_NV_HOPDONG')) && (
            <Button
              icon={<PrinterOutlined />}
              onClick={() => window.print()}
              size="small"
            >
              {t('common.print')}
            </Button>
          )
        }
      >
        {selectedHopDong && (
          <div>
            <Descriptions title={t('contract.pageTitle')} bordered size="small" column={1}>
              <Descriptions.Item label={t('contract.colContractNo')}>
                <Tag color="blue" style={{ fontSize: 13, padding: '2px 8px' }}>
                  {selectedHopDong.SOHD}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.colSignTimes')}>
                #{selectedHopDong.LANKY ?? 1}
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.labelSignDate')}>
                {formatContractDate(selectedHopDong.NGAYKY)}
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.colStartDate')}>
                {formatContractDate(selectedHopDong.NGAYBATDAU)}
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.colEndDate')}>
                {selectedHopDong.NGAYKETTHUC && selectedHopDong.NGAYKETTHUC.trim() ? (
                  formatContractDate(selectedHopDong.NGAYKETTHUC)
                ) : (
                  <Tag color="green">{t('contract.statusActive')}</Tag>
                )}
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.labelDuration')}>
                {selectedHopDong.THOIHAN || '-'}
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.colStatus')}>
                {!selectedHopDong.NGAYKETTHUC || !selectedHopDong.NGAYKETTHUC.trim() ? (
                  <Tag color="success">{t('contract.statusActive')}</Tag>
                ) : checkIsContractExpired(selectedHopDong.NGAYKETTHUC) ? (
                  <Tag color="error">{t('contract.statusExpired')}</Tag>
                ) : (
                  <Tag color="processing">{t('contract.statusActive')}</Tag>
                )}
              </Descriptions.Item>
              <Descriptions.Item label={t('contract.colSalary')}>
                {selectedHopDong.HESOLUONG
                  ? selectedHopDong.HESOLUONG > 1000
                    ? `${selectedHopDong.HESOLUONG.toLocaleString('vi-VN')} đ`
                    : `${selectedHopDong.HESOLUONG}x`
                  : '-'}
              </Descriptions.Item>
              {selectedHopDong.NOIDUNG && (
                <Descriptions.Item label={t('contract.labelContent')}>
                  {selectedHopDong.NOIDUNG}
                </Descriptions.Item>
              )}
            </Descriptions>

            <Divider style={{ margin: '20px 0' }} />

            <Descriptions title={t('employee360.tabOverview')} bordered size="small" column={1}>
              <Descriptions.Item label={t('employee.colEmpCode')}>
                <Tag color="cyan">#{selectedHopDong.MANV}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label={t('employee.colFullName')}>
                <Text strong>{selectedHopDong.HOTEN || '-'}</Text>
              </Descriptions.Item>
              <Descriptions.Item label={t('employee.colIdCard')}>
                {selectedHopDong.CCCD || '-'}
              </Descriptions.Item>
              <Descriptions.Item label={t('employee.colBirthDate')}>
                {formatContractDate(selectedHopDong.NGAYSINH)}
              </Descriptions.Item>
              <Descriptions.Item label={t('employee.labelAddress')}>
                {selectedHopDong.DIACHI || '-'}
              </Descriptions.Item>
              <Descriptions.Item label={t('employee.labelEducation')}>
                {selectedHopDong.TENTD || '-'}
              </Descriptions.Item>
              <Descriptions.Item label={t('common.info')}>
                {selectedHopDong.TENQT || '-'}
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}
      </Drawer>
    </div>
  );
}

export default HopDongPage;
