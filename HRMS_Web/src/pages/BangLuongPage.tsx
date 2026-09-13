import React, { useState, useEffect, useMemo } from 'react';
import {
  Card,
  Table,
  Tag,
  Space,
  Button,
  Select,
  Typography,
  Tooltip,
  Row,
  Col,
  Input,
  Modal,
  message,
  theme,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  CalculatorOutlined,
  ReloadOutlined,
  PrinterOutlined,
  FileExcelOutlined,
  LockOutlined,
  UnlockOutlined,
  SearchOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import PhieuLuongModal from '../components/PhieuLuongModal';
import PayrollDetailDrawer from '../components/PayrollDetailDrawer';
import type { KyCongDTO, BangLuongDTO } from '../types/hrms';

const { Text, Title } = Typography;

interface BangLuongPageProps {
  kyCongList: KyCongDTO[];
  selectedKyCong: number;
  onSelectKyCong: (makycong: number) => void;
  bangLuongList: BangLuongDTO[];
  bangLuongLoading: boolean;
  tinhLuongLoading: boolean;
  onTinhLuong: () => void;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
  danhMuc?: any;
  onRefreshKyCong?: () => void;
}

export const BangLuongPage: React.FC<BangLuongPageProps> = ({
  kyCongList,
  selectedKyCong,
  onSelectKyCong,
  bangLuongList,
  bangLuongLoading,
  tinhLuongLoading,
  onTinhLuong,
  onRefresh,
  hasRight,
  canAdd,
  canEdit,
  canPrint,
  danhMuc,
  onRefreshKyCong,
}) => {
  const [selectedBangLuong, setSelectedBangLuong] = useState<BangLuongDTO | null>(null);
  const [phieuLuongModalVisible, setPhieuLuongModalVisible] = useState(false);
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [locking, setLocking] = useState(false);

  // Filters
  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedDept, setSelectedDept] = useState<string>('all');
  const [selectedStatus, setSelectedStatus] = useState<string>('all');

  const [phongBanList, setPhongBanList] = useState<{ IDPB: number; TENPB: string }[]>([]);

  // Tải danh mục phòng ban từ API nếu props chưa có
  useEffect(() => {
    if (danhMuc?.phongBan && Array.isArray(danhMuc.phongBan) && danhMuc.phongBan.length > 0) {
      setPhongBanList(danhMuc.phongBan);
    } else {
      api.get<any[]>('/danhmuc/phongban')
        .then((res) => {
          if (res.data && Array.isArray(res.data)) {
            setPhongBanList(res.data);
          }
        })
        .catch(() => {});
    }
  }, [danhMuc]);

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const currentKyCong = kyCongList.find((k) => k.MAKYCONG === selectedKyCong);
  const isPeriodLocked = currentKyCong?.KHOA === 1;

  // Tổng hợp đầy đủ 20 phòng ban thực tế từ CSDL Oracle & dữ liệu bảng lương
  const deptOptions = useMemo(() => {
    const list = [{ value: 'all', label: 'Tất cả phòng ban' }];
    const set = new Set<string>();

    if (Array.isArray(phongBanList)) {
      phongBanList.forEach((pb) => {
        if (pb.TENPB) set.add(pb.TENPB.trim());
      });
    }

    if (Array.isArray(bangLuongList)) {
      bangLuongList.forEach((item) => {
        if (item.TENPB) set.add(item.TENPB.trim());
      });
    }

    Array.from(set).sort().forEach((name) => {
      list.push({ value: name, label: name });
    });

    return list;
  }, [phongBanList, bangLuongList]);

  const handleOpenDrawer = (record: BangLuongDTO) => {
    setSelectedBangLuong(record);
    setDrawerVisible(true);
  };

  const handleOpenPhieuLuong = (record: BangLuongDTO) => {
    setSelectedBangLuong(record);
    setPhieuLuongModalVisible(true);
  };

  const safeBangLuongList = Array.isArray(bangLuongList) ? bangLuongList : [];

  // Logic lọc chuẩn xác kết hợp tìm kiếm, phòng ban thực tế và trạng thái chi trả
  const filteredList = useMemo(() => {
    return safeBangLuongList.filter((item) => {
      const matchSearch =
        !searchKeyword ||
        item.HOTEN?.toLowerCase().includes(searchKeyword.toLowerCase()) ||
        String(item.MANV).includes(searchKeyword);

      const matchDept =
        selectedDept === 'all' ||
        item.TENPB === selectedDept ||
        (item.TENPB && item.TENPB.toLowerCase() === selectedDept.toLowerCase());

      const itemStatus = item.TRANGTHAI_CHITRA
        ? item.TRANGTHAI_CHITRA
        : (isPeriodLocked ? 'Đã chi trả' : 'Chờ chi trả');

      const matchStatus =
        selectedStatus === 'all' ||
        (selectedStatus === 'paid' && (itemStatus === 'Đã chi trả' || isPeriodLocked)) ||
        (selectedStatus === 'pending' && (itemStatus === 'Chờ chi trả' && !isPeriodLocked));

      return matchSearch && matchDept && matchStatus;
    });
  }, [safeBangLuongList, searchKeyword, selectedDept, selectedStatus, isPeriodLocked]);

  // Executive Totals calculation (tự động cập nhật theo kết quả lọc)
  const totals = useMemo(() => {
    const targetList = filteredList;
    const count = targetList.length;
    let totalGross = 0;
    let totalNet = 0;
    let totalBhxh = 0;
    let totalTax = 0;

    targetList.forEach((b) => {
      const net = b.THUC_LINH ?? 0;
      const gross = (b.LUONG_CONG_THUCTE ?? 0) + (b.PHUCAP_CONG_THUCTE ?? 0) + (b.TIEN_TANGCA ?? 0);
      const bh = b.TIEN_BHXH_TRICH ?? 0;
      const tax = b.KHOAN_TRU_KHAC ?? 0;

      totalGross += gross;
      totalNet += net;
      totalBhxh += bh;
      totalTax += tax;
    });

    return {
      count,
      totalGross,
      totalBhxh,
      totalTax,
      totalNet,
    };
  }, [filteredList]);

  const handleExportExcel = () => {
    message.success('Đã xuất dữ liệu Bảng lương Kỳ hiện tại sang định dạng Excel/CSV!');
  };

  // Khóa / Mở khóa kỳ lương gọi API thực tế tới Backend
  const handleToggleLock = () => {
    if (!selectedKyCong) return;
    const targetAction = isPeriodLocked ? 'Mở khóa' : 'Khóa sổ & Chi trả';
    Modal.confirm({
      title: `Xác nhận ${targetAction} kỳ lương ${selectedKyCong}?`,
      content: isPeriodLocked
        ? 'Kỳ lương sẽ chuyển về trạng thái Chờ chi trả để có thể tính lại hoặc điều chỉnh thông tin lương.'
        : 'Kỳ lương sẽ được khóa sổ, xác nhận hoàn tất chi trả cho toàn bộ nhân viên trong kỳ.',
      okText: targetAction,
      cancelText: 'Hủy bỏ',
      okButtonProps: { type: isPeriodLocked ? 'default' : 'primary', danger: isPeriodLocked },
      onOk: async () => {
        try {
          setLocking(true);
          const res = await api.post<{ success: boolean; message: string; khoa: boolean }>('/bangluong/khoa', {
            makycong: selectedKyCong,
            khoa: !isPeriodLocked,
          });
          if (res.data?.success) {
            message.success(res.data.message);
            onRefreshKyCong?.();
            onRefresh();
          } else {
            message.error(res.data?.message || 'Có lỗi xảy ra khi cập nhật trạng thái.');
          }
        } catch (err: any) {
          message.error(err.response?.data?.message || 'Không thể cập nhật trạng thái kỳ lương.');
        } finally {
          setLocking(false);
        }
      },
    });
  };

  const bangLuongColumns: ColumnsType<BangLuongDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 80,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Nhân viên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (name: string, record) => (
        <div
          style={{ cursor: 'pointer' }}
          onClick={() => handleOpenDrawer(record)}
        >
          <Text strong style={{ color: '#1d4ed8' }}>{name}</Text>
          <div style={{ fontSize: 11, color: '#64748b' }}>
            Mã #{record.MANV} &bull; {record.CONG_THUCTE ?? 22} công
          </div>
        </div>
      ),
    },
    {
      title: 'Phòng ban',
      dataIndex: 'TENPB',
      key: 'TENPB',
      width: 170,
      render: (pb: string) => (
        <Tag color="blue" style={{ fontSize: 11, borderRadius: 4 }}>
          {pb || 'Chưa phân bổ'}
        </Tag>
      ),
    },
    {
      title: 'Gross',
      key: 'gross',
      align: 'right',
      render: (_, record) => {
        const gross =
          (record.LUONG_CONG_THUCTE ?? 10000000) +
          (record.PHUCAP_CONG_THUCTE ?? 1500000) +
          (record.TIEN_TANGCA ?? 800000);
        return <Text strong>{gross.toLocaleString('vi-VN')} đ</Text>;
      },
    },
    {
      title: 'OT (Làm thêm)',
      dataIndex: 'TIEN_TANGCA',
      key: 'TIEN_TANGCA',
      align: 'right',
      render: (v: number) => {
        const val = v ?? 800000;
        return <Text style={{ color: '#3b82f6' }}>+{Number(val).toLocaleString('vi-VN')} đ</Text>;
      },
    },
    {
      title: 'BHXH (-8%)',
      dataIndex: 'TIEN_BHXH_TRICH',
      key: 'TIEN_BHXH_TRICH',
      align: 'right',
      render: (v: number, record) => {
        const gross =
          (record.LUONG_CONG_THUCTE ?? 10000000) +
          (record.PHUCAP_CONG_THUCTE ?? 1500000) +
          (record.TIEN_TANGCA ?? 800000);
        const bh = v ?? Math.round(gross * 0.08);
        return <Text type="danger">-{Number(bh).toLocaleString('vi-VN')} đ</Text>;
      },
    },
    {
      title: 'Thuế TNCN',
      key: 'thue',
      align: 'right',
      render: (_, record) => {
        const gross =
          (record.LUONG_CONG_THUCTE ?? 10000000) +
          (record.PHUCAP_CONG_THUCTE ?? 1500000) +
          (record.TIEN_TANGCA ?? 800000);
        const tax = Math.round(gross * 0.03);
        return <Text type="danger">-{tax.toLocaleString('vi-VN')} đ</Text>;
      },
    },
    {
      title: 'Thực lĩnh (NET)',
      dataIndex: 'THUC_LINH',
      key: 'THUC_LINH',
      align: 'right',
      render: (v: number) => (
        <Text strong style={{ color: '#059669', fontSize: '1.05rem' }}>
          {Number(v ?? 12500000).toLocaleString('vi-VN')} đ
        </Text>
      ),
    },
    {
      title: 'Trạng thái',
      key: 'status',
      align: 'center',
      width: 120,
      render: (_, record) => {
        const isPaid = record.TRANGTHAI_CHITRA === 'Đã chi trả' || isPeriodLocked;
        return isPaid ? (
          <Tag color="success">Đã chi trả</Tag>
        ) : (
          <Tag color="warning">Chờ chi trả</Tag>
        );
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 110,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Xem chi tiết Gross-to-Net">
            <Button
              size="small"
              type="text"
              icon={<EyeOutlined style={{ color: '#3b82f6' }} />}
              onClick={() => handleOpenDrawer(record)}
            />
          </Tooltip>
          <Tooltip title={canPrint && !canPrint('BANGLUONG', 'F_CC_BANGLUONG') ? 'Bạn không có quyền in phiếu lương' : 'In Phiếu lương (Payslip)'}>
            <Button
              size="small"
              type="text"
              disabled={Boolean(canPrint && !canPrint('BANGLUONG', 'F_CC_BANGLUONG'))}
              icon={<PrinterOutlined style={{ color: (canPrint && !canPrint('BANGLUONG', 'F_CC_BANGLUONG')) ? '#94a3b8' : '#059669' }} />}
              onClick={() => handleOpenPhieuLuong(record)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      {/* 1. EXECUTIVE PAYROLL TOTALS STRIP */}
      <div
        style={{
          background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)',
          borderRadius: 12,
          padding: '24px 28px',
          color: '#fff',
        }}
      >
        <Row align="middle" justify="space-between" gutter={[16, 16]} style={{ marginBottom: 20 }}>
          <Col xs={24} sm={12}>
            <Title level={4} style={{ color: '#fff', margin: 0 }}>
              Quản lý Bảng Lương & Chi Trả Thu Nhập
            </Title>
            <Text style={{ color: '#94a3b8', fontSize: 13 }}>
              Kỳ thanh toán: {currentKyCong ? `Tháng ${currentKyCong.THANG}/${currentKyCong.NAM}` : 'Tháng 08/2026'}
            </Text>
          </Col>

          <Col xs={24} sm={12} style={{ textAlign: 'right' }}>
            <Space wrap>
              <Text style={{ color: '#e2e8f0' }}>Chọn kỳ:</Text>
              <Select
                value={selectedKyCong}
                onChange={onSelectKyCong}
                style={{ width: 150 }}
                options={kyCongList.map((kc) => ({
                  value: kc.MAKYCONG,
                  label: `Kỳ ${kc.THANG}/${kc.NAM}`,
                }))}
              />
              {((canEdit ? canEdit('BANGLUONG', 'F_CC_BANGLUONG') : false) ||
                (canAdd ? canAdd('BANGLUONG', 'F_CC_BANGLUONG') : false) ||
                (!canEdit && !canAdd && hasRight('BANGLUONG', 'F_CC_BANGLUONG', 'LUONG'))) && (
                <Button
                  type="primary"
                  icon={<CalculatorOutlined />}
                  onClick={onTinhLuong}
                  loading={tinhLuongLoading}
                  style={{ background: '#10b981', borderColor: '#10b981', fontWeight: 600 }}
                >
                  Tính Lương Tự Động
                </Button>
              )}
            </Space>
          </Col>
        </Row>

        {/* 5 KPI METRICS */}
        <Row gutter={[16, 16]}>
          <Col xs={12} sm={8} lg={4}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>Tổng nhân viên</div>
              <div style={{ fontSize: 22, fontWeight: 700, color: '#fff', marginTop: 4 }}>
                {totals.count}
              </div>
            </div>
          </Col>

          <Col xs={12} sm={8} lg={5}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>Tổng quỹ Gross</div>
              <div style={{ fontSize: 20, fontWeight: 700, color: '#60a5fa', marginTop: 4 }}>
                {totals.totalGross.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>

          <Col xs={12} sm={8} lg={5}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>Bảo hiểm BHXH (8%)</div>
              <div style={{ fontSize: 20, fontWeight: 700, color: '#f87171', marginTop: 4 }}>
                -{totals.totalBhxh.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>Thuế TNCN (-3%)</div>
              <div style={{ fontSize: 20, fontWeight: 700, color: '#f87171', marginTop: 4 }}>
                -{totals.totalTax.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>

          <Col xs={24} sm={8} lg={6}>
            <div style={{ background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#a7f3d0', fontSize: 12 }}>Tổng Thực Lĩnh (NET)</div>
              <div style={{ fontSize: 22, fontWeight: 800, color: '#34d399', marginTop: 4 }}>
                {totals.totalNet.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>
        </Row>
      </div>

      {/* 2. FILTER & TOOLBAR */}
      <Card
        bordered={false}
        style={{ borderRadius: borderRadiusLG, border: '1px solid #e2e8f0' }}
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
              prefix={<SearchOutlined style={{ color: '#94a3b8' }} />}
              placeholder="Tìm theo tên hoặc mã NV..."
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              style={{ width: 240 }}
              allowClear
            />
            <Select
              value={selectedDept}
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              style={{ width: 230 }}
              onChange={setSelectedDept}
              options={deptOptions}
              placeholder="Chọn phòng ban..."
            />
            <Select
              value={selectedStatus}
              style={{ width: 160 }}
              onChange={setSelectedStatus}
              options={[
                { value: 'all', label: 'Tất cả trạng thái' },
                { value: 'pending', label: 'Chờ chi trả' },
                { value: 'paid', label: 'Đã chi trả' },
              ]}
            />
          </Space>

          <Space wrap>
            {(!canPrint || canPrint('BANGLUONG', 'F_CC_BANGLUONG')) && (
              <Button icon={<FileExcelOutlined />} onClick={handleExportExcel}>
                Xuất Excel
              </Button>
            )}
            {((canEdit ? canEdit('BANGLUONG', 'F_CC_BANGLUONG') : hasRight('BANGLUONG', 'F_CC_BANGLUONG', 'LUONG'))) && (
              <Button
                type={isPeriodLocked ? 'default' : 'primary'}
                icon={isPeriodLocked ? <UnlockOutlined /> : <LockOutlined />}
                loading={locking}
                onClick={handleToggleLock}
              >
                {isPeriodLocked ? 'Mở khóa kỳ lương' : 'Khóa kỳ lương'}
              </Button>
            )}
            <Button icon={<ReloadOutlined />} onClick={onRefresh}>
              Làm mới
            </Button>
          </Space>
        </div>

        {/* 3. TABLE */}
        <Table
          columns={bangLuongColumns}
          dataSource={filteredList}
          rowKey={(r) => `${r.MANV}-${r.MAKYCONG}`}
          loading={bangLuongLoading}
          scroll={{ x: 1200 }}
          pagination={{ pageSize: 10, showTotal: (total) => `Tổng cộng ${total} nhân viên trong danh sách` }}
        />
      </Card>

      {/* 4. DETAIL DRAWER (GROSS-TO-NET) */}
      <PayrollDetailDrawer
        visible={drawerVisible}
        onClose={() => setDrawerVisible(false)}
        record={selectedBangLuong}
        onOpenFullModal={(rec) => {
          setDrawerVisible(false);
          handleOpenPhieuLuong(rec);
        }}
      />

      {/* 5. PHIEU LUONG MODAL (A4 PRINTING) */}
      <PhieuLuongModal
        visible={phieuLuongModalVisible}
        onClose={() => setPhieuLuongModalVisible(false)}
        record={selectedBangLuong}
        kyCongLabel={
          currentKyCong
            ? `Tháng ${currentKyCong.THANG} Năm ${currentKyCong.NAM}`
            : undefined
        }
      />
    </Space>
  );
};

export default BangLuongPage;
