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

// Hàm chuẩn hóa & định dạng ngày tháng hợp đồng chính xác
export const formatContractDate = (d?: string | null): string => {
  if (!d || !d.trim()) return '-';
  const trimmed = d.trim();
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(trimmed)) {
    return trimmed;
  }
  const parsed = dayjs(trimmed);
  return parsed.isValid() ? parsed.format('DD/MM/YYYY') : trimmed;
};

// Hàm kiểm tra hợp đồng hết hạn dựa trên ngày kết thúc thực tế
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
  const {
    token: { borderRadiusLG, colorPrimary },
  } = theme.useToken();

  const [searchText, setSearchText] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [selectedHopDong, setSelectedHopDong] = useState<HopDongDTO | null>(null);

  // Thống kê nhanh số liệu từ CSDL
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

  // Bộ lọc dữ liệu danh sách
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
      title: 'Số Hợp đồng',
      dataIndex: 'SOHD',
      key: 'SOHD',
      render: (sohd: string) => <Tag color="blue" style={{ fontWeight: 600 }}>{sohd}</Tag>,
    },
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 80,
      render: (id: number) => <Tag color="cyan">#{id}</Tag>,
    },
    {
      title: 'Họ tên Nhân viên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (text: string, r) => (
        <Space direction="vertical" size={0}>
          <Text strong>{text || 'Chưa cập nhật'}</Text>
          {r.CCCD && <Text type="secondary" style={{ fontSize: 11 }}>CCCD: {r.CCCD}</Text>}
        </Space>
      ),
    },
    {
      title: 'Ngày bắt đầu',
      dataIndex: 'NGAYBATDAU',
      key: 'NGAYBATDAU',
      render: (d: string) => formatContractDate(d),
    },
    {
      title: 'Ngày kết thúc',
      dataIndex: 'NGAYKETTHUC',
      key: 'NGAYKETTHUC',
      render: (d: string) => {
        if (!d || !d.trim()) {
          return <Tag color="green">Vô thời hạn</Tag>;
        }
        return formatContractDate(d);
      },
    },
    {
      title: 'Thời hạn',
      dataIndex: 'THOIHAN',
      key: 'THOIHAN',
      render: (th: string) => th || 'Không xác định',
    },
    {
      title: 'Hệ số / Lương',
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
      title: 'Lần ký',
      dataIndex: 'LANKY',
      key: 'LANKY',
      width: 75,
      align: 'center',
      render: (l: number) => <Tag color="default">Lần {l ?? 1}</Tag>,
    },
    {
      title: 'Trạng thái',
      key: 'status',
      width: 120,
      render: (_, r) => {
        if (!r.NGAYKETTHUC || !r.NGAYKETTHUC.trim()) {
          return <Tag color="success">Vô thời hạn</Tag>;
        }
        const expired = checkIsContractExpired(r.NGAYKETTHUC);
        return expired ? (
          <Tag color="error">Hết hạn</Tag>
        ) : (
          <Tag color="processing">Đang hiệu lực</Tag>
        );
      },
    },
    {
      title: 'Thao tác',
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
          Chi tiết
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
              title="Tổng hợp đồng"
              value={stats.total}
              prefix={<FileTextOutlined style={{ color: colorPrimary }} />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title="Đang hiệu lực"
              value={stats.active}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title="Đã hết hạn"
              value={stats.expired}
              valueStyle={{ color: stats.expired > 0 ? '#ff4d4f' : '#8c8c8c' }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
            <Statistic
              title="HĐ Vô thời hạn"
              value={stats.indefinite}
              valueStyle={{ color: '#1677ff' }}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Bảng danh sách hợp đồng với tìm kiếm & lọc */}
      <Card
        title={`Danh sách Hợp đồng Lao động (${filteredList.length}/${hopDongList.length})`}
        extra={
          <Space wrap>
            <Input
              placeholder="Tìm kiếm mã NV, tên, số HĐ, CCCD..."
              prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              allowClear
              style={{ width: 260 }}
            />
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ width: 150 }}
              options={[
                { value: 'all', label: 'Tất cả trạng thái' },
                { value: 'active', label: 'Đang hiệu lực' },
                { value: 'expired', label: 'Hết hạn' },
                { value: 'indefinite', label: 'Vô thời hạn' },
              ]}
            />
            <Button icon={<ReloadOutlined />} onClick={onRefresh} loading={hopDongLoading}>
              Làm mới
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
          scroll={{ x: 1000 }}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
            showTotal: (total) => `Tổng cộng ${total} hợp đồng lao động`,
          }}
        />
      </Card>

      {/* Drawer Chi tiết hợp đồng lao động */}
      <Drawer
        title={`Chi tiết Hợp đồng: ${selectedHopDong?.SOHD || ''}`}
        placement="right"
        width={typeof window !== 'undefined' && window.innerWidth < 640 ? '100%' : 540}
        open={!!selectedHopDong}
        onClose={() => setSelectedHopDong(null)}
        extra={
          (!canPrint || canPrint('NV', 'F_NV_HOPDONG')) && (
            <Button
              icon={<PrinterOutlined />}
              onClick={() => window.print()}
              size="small"
            >
              In hợp đồng
            </Button>
          )
        }
      >
        {selectedHopDong && (
          <div>
            <Descriptions title="Thông tin Hợp đồng" bordered size="small" column={1}>
              <Descriptions.Item label="Số hợp đồng">
                <Tag color="blue" style={{ fontSize: 13, padding: '2px 8px' }}>
                  {selectedHopDong.SOHD}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Lần ký">
                Lần {selectedHopDong.LANKY ?? 1}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày ký">
                {formatContractDate(selectedHopDong.NGAYKY)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày bắt đầu">
                {formatContractDate(selectedHopDong.NGAYBATDAU)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày kết thúc">
                {selectedHopDong.NGAYKETTHUC && selectedHopDong.NGAYKETTHUC.trim() ? (
                  formatContractDate(selectedHopDong.NGAYKETTHUC)
                ) : (
                  <Tag color="green">Vô thời hạn</Tag>
                )}
              </Descriptions.Item>
              <Descriptions.Item label="Thời hạn">
                {selectedHopDong.THOIHAN || 'Không xác định'}
              </Descriptions.Item>
              <Descriptions.Item label="Trạng thái">
                {!selectedHopDong.NGAYKETTHUC || !selectedHopDong.NGAYKETTHUC.trim() ? (
                  <Tag color="success">Vô thời hạn (Hiệu lực)</Tag>
                ) : checkIsContractExpired(selectedHopDong.NGAYKETTHUC) ? (
                  <Tag color="error">Hết hạn</Tag>
                ) : (
                  <Tag color="processing">Đang hiệu lực</Tag>
                )}
              </Descriptions.Item>
              <Descriptions.Item label="Mức lương / Hệ số">
                {selectedHopDong.HESOLUONG
                  ? selectedHopDong.HESOLUONG > 1000
                    ? `${selectedHopDong.HESOLUONG.toLocaleString('vi-VN')} đ`
                    : `${selectedHopDong.HESOLUONG}x`
                  : '-'}
              </Descriptions.Item>
              {selectedHopDong.NOIDUNG && (
                <Descriptions.Item label="Nội dung">
                  {selectedHopDong.NOIDUNG}
                </Descriptions.Item>
              )}
            </Descriptions>

            <Divider style={{ margin: '20px 0' }} />

            <Descriptions title="Thông tin Nhân viên" bordered size="small" column={1}>
              <Descriptions.Item label="Mã nhân viên">
                <Tag color="cyan">#{selectedHopDong.MANV}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Họ tên">
                <Text strong>{selectedHopDong.HOTEN || 'Chưa cập nhật'}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Số CCCD">
                {selectedHopDong.CCCD || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày sinh">
                {formatContractDate(selectedHopDong.NGAYSINH)}
              </Descriptions.Item>
              <Descriptions.Item label="Địa chỉ">
                {selectedHopDong.DIACHI || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Trình độ">
                {selectedHopDong.TENTD || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Quốc tịch">
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
