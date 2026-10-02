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
  message,
  theme,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
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
import { useAppTheme } from '../theme/ThemeContext';
import { PageHeader } from '../theme/components/PageHeader';

const { Text } = Typography;

interface BangLuongPageProps {
  kyCongList: KyCongDTO[];
  selectedKyCong: number;
  onSelectKyCong: (makycong: number) => void;
  bangLuongList: BangLuongDTO[];
  bangLuongLoading: boolean;
  onRefresh: () => void;
  canPrint?: (...codes: string[]) => boolean;
  danhMuc?: any;
}

export const BangLuongPage: React.FC<BangLuongPageProps> = ({
  kyCongList,
  selectedKyCong,
  onSelectKyCong,
  bangLuongList,
  bangLuongLoading,
  onRefresh,
  canPrint,
  danhMuc,
}) => {
  const { t } = useAppLanguage();
  const { tokens, isDark } = useAppTheme();
  const [selectedBangLuong, setSelectedBangLuong] = useState<BangLuongDTO | null>(null);
  const [phieuLuongModalVisible, setPhieuLuongModalVisible] = useState(false);
  const [drawerVisible, setDrawerVisible] = useState(false);

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
        : (isPeriodLocked ? 'Đã chốt sổ (Chờ chi trả)' : 'Chờ chi trả');

      const matchStatus =
        selectedStatus === 'all' ||
        (selectedStatus === 'paid' && itemStatus === 'Đã chi trả') ||
        (selectedStatus === 'pending' && itemStatus !== 'Đã chi trả');

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
      const gross = b.TONG_CONG ?? (
        (b.LUONG_CONG_THUCTE ?? 0) +
        (b.PHUCAP_CONG_THUCTE ?? 0) +
        (b.TIEN_TANGCA ?? 0) +
        (b.TIEN_CHUYENCAN ?? 0) +
        (b.TIEN_AN_CA ?? 0) +
        (b.KHOAN_CONG_KHAC ?? 0)
      );
      const bh = b.TIEN_BHXH_TRICH ?? ((b.TIEN_BHXH ?? 0) + (b.TIEN_BHYT ?? 0) + (b.TIEN_BHTN ?? 0));
      const tax = b.THUE_TNCN ?? 0;

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
          <Space size="small">
            <Text strong style={{ color: '#1d4ed8' }}>{name}</Text>
            {record.IS_LEGACY === 1 && (
              <Tag color="default" style={{ fontSize: 10, padding: '0 4px', borderRadius: 4 }}>
                Legacy
              </Tag>
            )}
          </Space>
          <div style={{ fontSize: 11, color: '#64748b' }}>
            #{record.MANV} &bull; {record.CONG_THUCTE ?? 0} {t('attendance.colActualDays')}
            {Boolean(record.CONG_CHUAN) && ` / ${record.CONG_CHUAN}`}
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
        const gross = record.TONG_CONG ?? (
          (record.LUONG_CONG_THUCTE ?? 0) +
          (record.PHUCAP_CONG_THUCTE ?? 0) +
          (record.TIEN_TANGCA ?? 0) +
          (record.TIEN_CHUYENCAN ?? 0) +
          (record.TIEN_AN_CA ?? 0) +
          (record.KHOAN_CONG_KHAC ?? 0)
        );
        return <Text strong>{gross.toLocaleString('vi-VN')} đ</Text>;
      },
    },
    {
      title: t('payroll.colOvertimeSalary'),
      dataIndex: 'TIEN_TANGCA',
      key: 'TIEN_TANGCA',
      align: 'right',
      render: (v: number) => {
        const val = v ?? 0;
        return val > 0 ? (
          <Text style={{ color: '#3b82f6' }}>+{Number(val).toLocaleString('vi-VN')} đ</Text>
        ) : (
          <Text type="secondary">0 đ</Text>
        );
      },
    },
    {
      title: t('payroll.colInsurance'),
      dataIndex: 'TIEN_BHXH_TRICH',
      key: 'TIEN_BHXH_TRICH',
      align: 'right',
      render: (v: number, record) => {
        const bh = v ?? ((record.TIEN_BHXH ?? 0) + (record.TIEN_BHYT ?? 0) + (record.TIEN_BHTN ?? 0));
        return bh > 0 ? (
          <Text type="danger">-{Number(bh).toLocaleString('vi-VN')} đ</Text>
        ) : (
          <Text type="secondary">0 đ</Text>
        );
      },
    },
    {
      title: t('payroll.colTax'),
      key: 'thue',
      align: 'right',
      render: (_, record) => {
        const tax = record.THUE_TNCN ?? 0;
        return tax > 0 ? (
          <Text type="danger">-{tax.toLocaleString('vi-VN')} đ</Text>
        ) : (
          <Text type="secondary">0 đ</Text>
        );
      },
    },
    {
      title: t('payroll.colNetSalary'),
      dataIndex: 'THUC_LINH',
      key: 'THUC_LINH',
      align: 'right',
      render: (v: number) => (
        <Text strong style={{ color: '#059669', fontSize: '1.05rem' }}>
          {Number(v ?? 0).toLocaleString('vi-VN')} đ
        </Text>
      ),
    },
    {
      title: t('payroll.colPaymentStatus'),
      key: 'status',
      align: 'center',
      width: 130,
      render: (_, record) => {
        if (record.TRANGTHAI_CHITRA === 'Đã chi trả') {
          return <Tag color="success">Đã chi trả</Tag>;
        }
        if (record.TRANG_THAI === 'APPROVED') {
          return <Tag color="blue">Đã duyệt</Tag>;
        }
        if (record.TRANG_THAI === 'LEGACY_READONLY') {
          return <Tag color="default">Legacy</Tag>;
        }
        if (isPeriodLocked) {
          return <Tag color="processing">Đã chốt sổ (Chờ chi trả)</Tag>;
        }
        return <Tag color="warning">{t('status.unpaid')}</Tag>;
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
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16, width: '100%' }}>
      {/* 1. EXECUTIVE PAGE HEADER */}
      <PageHeader
        title={t('payroll.pageTitle') || 'Bảng Lương & Chi Trả Thu Nhập'}
        subtitle={
          currentKyCong
            ? t('payroll.payslipPeriodHeader', { month: currentKyCong.THANG ?? 0, year: currentKyCong.NAM ?? 0 })
            : 'Tổng hợp thu nhập, phụ cấp, tăng ca, bảo hiểm, thuế TNCN và thực lĩnh theo kỳ công'
        }
        breadcrumbs={[
          { title: 'Công & lương' },
          { title: 'Bảng lương' },
        ]}
        extra={
          <Space wrap size="middle">
            <Space>
              <Text strong>{t('common.period')}:</Text>
              <Select
                value={selectedKyCong}
                onChange={onSelectKyCong}
                style={{ width: 140 }}
                options={kyCongList.map((kc) => ({
                  value: kc.MAKYCONG,
                  label: `${kc.THANG}/${kc.NAM}`,
                }))}
              />
            </Space>
          </Space>
        }
      />

      {/* 2. 5 KPI METRICS */}
      <Row gutter={[14, 14]}>
        <Col xs={12} sm={8} lg={4}>
          <div
            style={{
              background: tokens.cardBg,
              border: `1px solid ${tokens.borderSubtle}`,
              padding: '14px 16px',
              borderRadius: 10,
              boxShadow: isDark ? 'none' : '0 1px 3px rgba(0,0,0,0.02)',
            }}
          >
            <div style={{ color: tokens.textSecondary, fontSize: 12, fontWeight: 500 }}>{t('dashboard.statTotalEmployees')}</div>
            <div style={{ fontSize: 22, fontWeight: 700, color: tokens.textPrimary, marginTop: 4 }}>
              {totals.count}
            </div>
          </div>
        </Col>

        <Col xs={12} sm={8} lg={5}>
          <div
            style={{
              background: tokens.cardBg,
              border: `1px solid ${tokens.borderSubtle}`,
              padding: '14px 16px',
              borderRadius: 10,
              boxShadow: isDark ? 'none' : '0 1px 3px rgba(0,0,0,0.02)',
            }}
          >
            <div style={{ color: tokens.textSecondary, fontSize: 12, fontWeight: 500 }}>{t('payroll.colTotalIncome')}</div>
            <div style={{ fontSize: 20, fontWeight: 700, color: tokens.chartBlue, marginTop: 4 }}>
              {totals.totalGross.toLocaleString('vi-VN')} đ
            </div>
          </div>
        </Col>

        <Col xs={12} sm={8} lg={5}>
          <div
            style={{
              background: tokens.cardBg,
              border: `1px solid ${tokens.borderSubtle}`,
              padding: '14px 16px',
              borderRadius: 10,
              boxShadow: isDark ? 'none' : '0 1px 3px rgba(0,0,0,0.02)',
            }}
          >
            <div style={{ color: tokens.textSecondary, fontSize: 12, fontWeight: 500 }}>{t('payroll.colInsurance')}</div>
            <div style={{ fontSize: 20, fontWeight: 700, color: tokens.dangerText, marginTop: 4 }}>
              -{totals.totalBhxh.toLocaleString('vi-VN')} đ
            </div>
          </div>
        </Col>

        <Col xs={12} sm={8} lg={4}>
          <div
            style={{
              background: tokens.cardBg,
              border: `1px solid ${tokens.borderSubtle}`,
              padding: '14px 16px',
              borderRadius: 10,
              boxShadow: isDark ? 'none' : '0 1px 3px rgba(0,0,0,0.02)',
            }}
          >
            <div style={{ color: tokens.textSecondary, fontSize: 12, fontWeight: 500 }}>{t('payroll.colTax')}</div>
            <div style={{ fontSize: 20, fontWeight: 700, color: tokens.warningText, marginTop: 4 }}>
              -{totals.totalTax.toLocaleString('vi-VN')} đ
            </div>
          </div>
        </Col>

        <Col xs={24} sm={8} lg={6}>
          <div
            style={{
              background: tokens.successBg,
              border: `1px solid ${tokens.borderSubtle}`,
              padding: '14px 16px',
              borderRadius: 10,
              boxShadow: isDark ? 'none' : '0 1px 3px rgba(0,0,0,0.02)',
            }}
          >
            <div style={{ color: tokens.successText, fontSize: 12, fontWeight: 600 }}>{t('payroll.colNetSalary')}</div>
            <div style={{ fontSize: 22, fontWeight: 800, color: tokens.successText, marginTop: 4 }}>
              {totals.totalNet.toLocaleString('vi-VN')} đ
            </div>
          </div>
        </Col>
      </Row>

      {/* 3. FILTER & TOOLBAR */}
      <Card
        bordered={false}
        style={{ borderRadius: borderRadiusLG, background: tokens.cardBg, border: `1px solid ${tokens.borderSubtle}` }}
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
            <Tag
              icon={isPeriodLocked ? <LockOutlined /> : <UnlockOutlined />}
              color={isPeriodLocked ? 'error' : 'processing'}
              style={{ padding: '4px 10px', fontSize: 13, borderRadius: 6, display: 'inline-flex', alignItems: 'center' }}
            >
              {isPeriodLocked ? (t('payroll.statusLocked') || 'Đã khóa sổ') : (t('payroll.statusOpen') || 'Đang mở')}
            </Tag>
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
    </div>
  );
};

export default BangLuongPage;
