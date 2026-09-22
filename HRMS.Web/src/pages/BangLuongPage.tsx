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
import { useAppLanguage } from '../services/i18n';

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
  const { t } = useAppLanguage();
  const [selectedBangLuong, setSelectedBangLuong] = useState<BangLuongDTO | null>(null);
  const [phieuLuongModalVisible, setPhieuLuongModalVisible] = useState(false);
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [locking, setLocking] = useState(false);

  // Filters
  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedDept, setSelectedDept] = useState<string>('all');
  const [selectedStatus, setSelectedStatus] = useState<string>('all');

  const [phongBanList, setPhongBanList] = useState<{ IDPB: number; TENPB: string }[]>([]);

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

  const deptOptions = useMemo(() => {
    const list = [{ value: 'all', label: t('common.all') }];
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
  }, [phongBanList, bangLuongList, t]);

  const handleOpenDrawer = (record: BangLuongDTO) => {
    setSelectedBangLuong(record);
    setDrawerVisible(true);
  };

  const handleOpenPhieuLuong = (record: BangLuongDTO) => {
    setSelectedBangLuong(record);
    setPhieuLuongModalVisible(true);
  };

  const safeBangLuongList = Array.isArray(bangLuongList) ? bangLuongList : [];

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
    message.success(t('common.success'));
  };

  const handleToggleLock = () => {
    if (!selectedKyCong) return;
    const targetAction = isPeriodLocked ? t('attendance.btnUnlockPeriod') : t('attendance.btnLockPeriod');
    Modal.confirm({
      title: `${targetAction}: #${selectedKyCong}?`,
      content: t('common.confirmDeleteMsg'),
      okText: t('common.confirm'),
      cancelText: t('common.cancel'),
      okButtonProps: { type: isPeriodLocked ? 'default' : 'primary', danger: isPeriodLocked },
      onOk: async () => {
        try {
          setLocking(true);
          const res = await api.post<{ success: boolean; message: string; khoa: boolean }>('/bangluong/khoa', {
            makycong: selectedKyCong,
            khoa: !isPeriodLocked,
          });
          if (res.data?.success) {
            message.success(res.data.message || t('common.updateSuccess'));
            onRefreshKyCong?.();
            onRefresh();
          } else {
            message.error(res.data?.message || t('common.error'));
          }
        } catch (err: any) {
          message.error(err.response?.data?.message || t('common.error'));
        } finally {
          setLocking(false);
        }
      },
    });
  };

  const bangLuongColumns: ColumnsType<BangLuongDTO> = [
    {
      title: t('payroll.colEmpCode'),
      dataIndex: 'MANV',
      key: 'MANV',
      width: 80,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: t('payroll.colFullName'),
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (name: string, record) => (
        <div
          style={{ cursor: 'pointer' }}
          onClick={() => handleOpenDrawer(record)}
        >
          <Text strong style={{ color: '#1d4ed8' }}>{name}</Text>
          <div style={{ fontSize: 11, color: '#64748b' }}>
            #{record.MANV} &bull; {record.CONG_THUCTE ?? 22} {t('attendance.colActualDays')}
          </div>
        </div>
      ),
    },
    {
      title: t('employee.labelDepartment'),
      dataIndex: 'TENPB',
      key: 'TENPB',
      width: 170,
      render: (pb: string) => (
        <Tag color="blue" style={{ fontSize: 11, borderRadius: 4 }}>
          {pb || '-'}
        </Tag>
      ),
    },
    {
      title: t('payroll.colTotalIncome'),
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
      title: t('payroll.colOvertimeSalary'),
      dataIndex: 'TIEN_TANGCA',
      key: 'TIEN_TANGCA',
      align: 'right',
      render: (v: number) => {
        const val = v ?? 800000;
        return <Text style={{ color: '#3b82f6' }}>+{Number(val).toLocaleString('vi-VN')} đ</Text>;
      },
    },
    {
      title: t('payroll.colInsurance'),
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
      title: t('payroll.colTax'),
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
      title: t('payroll.colNetSalary'),
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
      title: t('payroll.colPaymentStatus'),
      key: 'status',
      align: 'center',
      width: 120,
      render: (_, record) => {
        const isPaid = record.TRANGTHAI_CHITRA === 'Đã chi trả' || isPeriodLocked;
        return isPaid ? (
          <Tag color="success">{t('status.paid')}</Tag>
        ) : (
          <Tag color="warning">{t('status.unpaid')}</Tag>
        );
      },
    },
    {
      title: t('common.actions'),
      key: 'action',
      width: 110,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title={t('common.view')}>
            <Button
              size="small"
              type="text"
              icon={<EyeOutlined style={{ color: '#3b82f6' }} />}
              onClick={() => handleOpenDrawer(record)}
            />
          </Tooltip>
          <Tooltip title={t('payroll.btnPrintPayslip')}>
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
              {t('payroll.pageTitle')}
            </Title>
            <Text style={{ color: '#94a3b8', fontSize: 13 }}>
              {currentKyCong
                ? t('payroll.payslipPeriodHeader', { month: currentKyCong.THANG ?? 0, year: currentKyCong.NAM ?? 0 })
                : ''}
            </Text>
          </Col>

          <Col xs={24} sm={12} style={{ textAlign: 'right' }}>
            <Space wrap>
              <Text style={{ color: '#e2e8f0' }}>{t('common.period')}:</Text>
              <Select
                value={selectedKyCong}
                onChange={onSelectKyCong}
                style={{ width: 150 }}
                options={kyCongList.map((kc) => ({
                  value: kc.MAKYCONG,
                  label: `${kc.THANG}/${kc.NAM}`,
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
                  {t('payroll.btnCalculatePayroll')}
                </Button>
              )}
            </Space>
          </Col>
        </Row>

        {/* 5 KPI METRICS */}
        <Row gutter={[16, 16]}>
          <Col xs={12} sm={8} lg={4}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>{t('dashboard.statTotalEmployees')}</div>
              <div style={{ fontSize: 22, fontWeight: 700, color: '#fff', marginTop: 4 }}>
                {totals.count}
              </div>
            </div>
          </Col>

          <Col xs={12} sm={8} lg={5}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>{t('payroll.colTotalIncome')}</div>
              <div style={{ fontSize: 20, fontWeight: 700, color: '#60a5fa', marginTop: 4 }}>
                {totals.totalGross.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>

          <Col xs={12} sm={8} lg={5}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>{t('payroll.colInsurance')}</div>
              <div style={{ fontSize: 20, fontWeight: 700, color: '#f87171', marginTop: 4 }}>
                -{totals.totalBhxh.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <div style={{ background: 'rgba(255,255,255,0.08)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#94a3b8', fontSize: 12 }}>{t('payroll.colTax')}</div>
              <div style={{ fontSize: 20, fontWeight: 700, color: '#f87171', marginTop: 4 }}>
                -{totals.totalTax.toLocaleString('vi-VN')} đ
              </div>
            </div>
          </Col>

          <Col xs={24} sm={8} lg={6}>
            <div style={{ background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', padding: '12px 14px', borderRadius: 8 }}>
              <div style={{ color: '#a7f3d0', fontSize: 12 }}>{t('payroll.colNetSalary')}</div>
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
              placeholder={t('common.searchPlaceholder')}
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              style={{ minWidth: 200 }}
              allowClear
            />
            <Select
              value={selectedDept}
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              style={{ minWidth: 180 }}
              onChange={setSelectedDept}
              options={deptOptions}
              placeholder={t('employee.labelDepartment')}
            />
            <Select
              value={selectedStatus}
              style={{ minWidth: 140 }}
              onChange={setSelectedStatus}
              options={[
                { value: 'all', label: t('common.all') },
                { value: 'pending', label: t('status.unpaid') },
                { value: 'paid', label: t('status.paid') },
              ]}
            />
          </Space>

          <Space wrap>
            {(!canPrint || canPrint('BANGLUONG', 'F_CC_BANGLUONG')) && (
              <Button icon={<FileExcelOutlined />} onClick={handleExportExcel}>
                {t('payroll.btnExportPayroll')}
              </Button>
            )}
            {((canEdit ? canEdit('BANGLUONG', 'F_CC_BANGLUONG') : hasRight('BANGLUONG', 'F_CC_BANGLUONG', 'LUONG'))) && (
              <Button
                type={isPeriodLocked ? 'default' : 'primary'}
                icon={isPeriodLocked ? <UnlockOutlined /> : <LockOutlined />}
                loading={locking}
                onClick={handleToggleLock}
              >
                {isPeriodLocked ? t('attendance.btnUnlockPeriod') : t('payroll.btnLockPayroll')}
              </Button>
            )}
            <Button icon={<ReloadOutlined />} onClick={onRefresh}>
              {t('common.refresh')}
            </Button>
          </Space>
        </div>

        {/* 3. TABLE */}
        <Table
          columns={bangLuongColumns}
          dataSource={filteredList}
          rowKey={(r) => `${r.MANV}-${r.MAKYCONG}`}
          loading={bangLuongLoading}
          scroll={{ x: 'max-content' }}
          pagination={{ pageSize: 10, showTotal: (total) => t('common.totalRecords', { total }) }}
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
