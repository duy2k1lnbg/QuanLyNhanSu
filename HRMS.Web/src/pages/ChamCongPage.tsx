import React, { useState, useMemo, useCallback, useEffect } from 'react';
import {
  Card,
  Table,
  Tag,
  Space,
  Button,
  Select,
  Tabs,
  Row,
  Col,
  Typography,
  Progress,
  theme,
  Input,
  Radio,
  Modal,
  message,
  Popconfirm,
  Badge,
  Alert,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  ReloadOutlined,
  CalendarOutlined,
  TableOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  FilterOutlined,
  SearchOutlined,
  UserOutlined,
  ApartmentOutlined,
  WarningOutlined,
  LockOutlined,
  UnlockOutlined,
  PlayCircleOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import type {
  KyCongDTO,
  KyCongChiTietDTO,
  LoaiCaDTO,
  CaPhienBanDTO,
  LichLamViecDTO,
  BatThuongDTO,
  PeriodReadinessDTO,
  LanTinhDTO,
} from '../types/hrms';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import { PageHeader } from '../theme/components/PageHeader';
import api from '../services/api';
import {
  calculateDayAttendance,
  parseAttendanceSymbol,
  type DayAttendanceMetrics,
  type EmployeeDayRecord,
} from '../utils/attendanceViewModel';

const { Text } = Typography;

interface ChamCongPageProps {
  kyCongList: KyCongDTO[];
  selectedKyCong: number;
  onSelectKyCong: (makycong: number) => void;
  chamCongList: KyCongChiTietDTO[];
  chamCongLoading: boolean;
  loaiCaList: LoaiCaDTO[];
  onRefresh: () => void;
}

export const ChamCongPage: React.FC<ChamCongPageProps> = ({
  kyCongList,
  selectedKyCong,
  onSelectKyCong,
  chamCongList,
  chamCongLoading,
  loaiCaList,
  onRefresh,
}) => {
  const { lang, t } = useAppLanguage();
  const { tokens, isDark } = useAppTheme();
  const {
    token: { borderRadiusLG, colorPrimary },
  } = theme.useToken();

  const [activeView, setActiveView] = useState<string>('calendar');
  const [selectedDept, setSelectedDept] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<'active' | 'all' | 'resigned'>('all');
  const [searchText, setSearchText] = useState<string>('');
  const [attendanceFilter, setAttendanceFilter] = useState<string>('all');
  const [drillDownCategory, setDrillDownCategory] = useState<string>('all');
  const [selectedDay, setSelectedDay] = useState<number>(() => {
    const today = new Date();
    return today.getDate();
  });
  const [hoveredColKey, setHoveredColKey] = useState<string | null>(null);

  // Dữ liệu mở rộng V1_18 / V1_19
  const [caPhienBanList, setCaPhienBanList] = useState<CaPhienBanDTO[]>([]);
  const [lichLamViecList, setLichLamViecList] = useState<LichLamViecDTO[]>([]);
  const [batThuongList, setBatThuongList] = useState<BatThuongDTO[]>([]);
  const [readiness, setReadiness] = useState<PeriodReadinessDTO | null>(null);
  const [lanTinhList, setLanTinhList] = useState<LanTinhDTO[]>([]);
  const [extraLoading, setExtraLoading] = useState<boolean>(false);
  const [publishing, setPublishing] = useState<boolean>(false);

  // Modal xử lý ngoại lệ
  const [resolveModalVisible, setResolveModalVisible] = useState<boolean>(false);
  const [selectedAnomaly, setSelectedAnomaly] = useState<BatThuongDTO | null>(null);
  const [resolveAction, setResolveAction] = useState<string>('VERIFY');
  const [resolveReason, setResolveReason] = useState<string>('');

  // Modal mở khóa kỳ công
  const [unlockModalVisible, setUnlockModalVisible] = useState<boolean>(false);
  const [unlockReason, setUnlockReason] = useState<string>('');

  const fetchExtraAttendanceData = useCallback(async () => {
    if (!selectedKyCong) return;
    setExtraLoading(true);
    try {
      const [cpbRes, lichRes, btRes, rdRes, ltRes] = await Promise.allSettled([
        api.get('/chamcong/ca-phienban'),
        api.get(`/chamcong/lich-lamviec?makycong=${selectedKyCong}&pageSize=200`),
        api.get(`/chamcong/bat-thuong?makycong=${selectedKyCong}`),
        api.get(`/chamcong/kycong/${selectedKyCong}/readiness`),
        api.get(`/chamcong/lan-tinh?makycong=${selectedKyCong}`),
      ]);
      if (cpbRes.status === 'fulfilled' && cpbRes.value.data?.data) {
        setCaPhienBanList(cpbRes.value.data.data);
      }
      if (lichRes.status === 'fulfilled' && lichRes.value.data?.data) {
        setLichLamViecList(lichRes.value.data.data);
      }
      if (btRes.status === 'fulfilled' && btRes.value.data?.data) {
        setBatThuongList(btRes.value.data.data);
      }
      if (rdRes.status === 'fulfilled' && rdRes.value.data) {
        setReadiness(rdRes.value.data);
      }
      if (ltRes.status === 'fulfilled' && ltRes.value.data?.data) {
        setLanTinhList(ltRes.value.data.data);
      }
    } catch {
      // ignore
    } finally {
      setExtraLoading(false);
    }
  }, [selectedKyCong]);

  useEffect(() => {
    fetchExtraAttendanceData();
  }, [fetchExtraAttendanceData]);

  // Hành động: Tính toán & Công bố công
  const handlePublishAttendance = async () => {
    setPublishing(true);
    try {
      const res = await api.post('/chamcong/publish', {
        makycong: selectedKyCong,
        forceRecalculate: true,
        ghiChu: `Tính công bố kỳ ${selectedKyCong} từ giao diện quản trị Web`,
      });
      if (res.data?.success) {
        message.success(res.data.message || 'Công bố bảng chấm công thành công!');
        onRefresh();
        fetchExtraAttendanceData();
      } else {
        message.error(res.data?.message || 'Không thể công bố bảng chấm công.');
      }
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi kích hoạt công bố bảng chấm công.');
    } finally {
      setPublishing(false);
    }
  };

  // Hành động: Chốt & Khóa kỳ công
  const handleLockKyCong = async () => {
    try {
      const res = await api.post(`/chamcong/kycong/${selectedKyCong}/lock`, {
        lyDo: `Chốt công kỳ ${selectedKyCong}`,
      });
      if (res.data?.success) {
        message.success(res.data.message || 'Đã khóa kỳ công thành công!');
        onRefresh();
        fetchExtraAttendanceData();
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Không thể khóa kỳ công.';
      message.error(msg);
    }
  };

  // Hành động: Mở khóa kỳ công
  const handleUnlockKyCong = async () => {
    if (!unlockReason.trim()) {
      message.warning('Vui lòng nhập lý do mở khóa kỳ công.');
      return;
    }
    try {
      const res = await api.post(`/chamcong/kycong/${selectedKyCong}/unlock`, {
        lyDo: unlockReason.trim(),
      });
      if (res.data?.success) {
        message.success(res.data.message || 'Đã mở khóa kỳ công thành công!');
        setUnlockModalVisible(false);
        setUnlockReason('');
        onRefresh();
        fetchExtraAttendanceData();
      }
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Không thể mở khóa kỳ công.');
    }
  };

  // Hành động: Xử lý ngoại lệ công
  const handleResolveAnomaly = async () => {
    if (!selectedAnomaly) return;
    if (!resolveReason.trim()) {
      message.warning('Lý do xử lý bắt buộc phải nhập.');
      return;
    }
    try {
      const res = await api.post(`/chamcong/bat-thuong/${selectedAnomaly.idBatThuong}/resolve`, {
        action: resolveAction,
        reason: resolveReason.trim(),
      });
      if (res.data?.success) {
        message.success(res.data.message || 'Đã cập nhật quyết định xử lý ngoại lệ!');
        setResolveModalVisible(false);
        setSelectedAnomaly(null);
        setResolveReason('');
        fetchExtraAttendanceData();
      }
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi xử lý ngoại lệ công.');
    }
  };

  const safeChamCongList = useMemo(() => (Array.isArray(chamCongList) ? chamCongList : []), [chamCongList]);
  const currentKyCong = kyCongList.find((k) => k.MAKYCONG === selectedKyCong);
  const month = currentKyCong?.THANG || new Date().getMonth() + 1;
  const year = currentKyCong?.NAM || new Date().getFullYear();
  const daysInMonth = new Date(year, month, 0).getDate();

  // Tính ngày bắt đầu tháng rơi vào thứ mấy
  const firstDayWeek = new Date(year, month - 1, 1).getDay();
  const startColOffset = firstDayWeek === 0 ? 6 : firstDayWeek - 1;

  // Localized Day of week headers
  const dayOfWeekHeaders = useMemo(() => {
    // Mon (5) to Sun (11) Jan 2026
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(2026, 0, 5 + i);
      const name = new Intl.DateTimeFormat(lang === 'zh-CN' ? 'zh-CN' : lang, { weekday: 'short' }).format(d);
      return { name, isSat: i === 5, isSun: i === 6 };
    });
  }, [lang]);

  // Danh sách phòng ban thực tế từ CSDL
  const departmentOptions = useMemo(() => {
    const map = new Map<string, number>();
    safeChamCongList.forEach((r) => {
      const pb = r.TENPB || t('employee.labelDepartment');
      map.set(pb, (map.get(pb) || 0) + 1);
    });
    const list = Array.from(map.entries()).map(([name, count]) => ({
      value: name,
      label: `${name} (${count})`,
    }));
    return [{ value: 'all', label: `${t('common.all')} (${safeChamCongList.length})` }, ...list];
  }, [safeChamCongList, t]);

  // Đếm số lượng theo trạng thái làm việc thực tế
  const statusCounts = useMemo(() => {
    let active = 0;
    let resigned = 0;
    safeChamCongList.forEach((r) => {
      if (r.IS_ACTIVE !== false && r.DATHOIVIEC !== 1) active++;
      if (r.DATHOIVIEC === 1) resigned++;
    });
    return {
      active,
      resigned,
      total: safeChamCongList.length,
    };
  }, [safeChamCongList]);

  // Danh sách nhân sự theo bộ lọc phạm vi cơ bản (Trạng thái nhân sự, Phòng ban, Tìm kiếm)
  const baseFilteredList = useMemo(() => {
    let result = safeChamCongList;

    if (statusFilter === 'active') {
      result = result.filter((r) => r.IS_ACTIVE !== false && r.DATHOIVIEC !== 1);
    } else if (statusFilter === 'resigned') {
      result = result.filter((r) => r.DATHOIVIEC === 1);
    }

    if (selectedDept !== 'all') {
      result = result.filter((r) => r.TENPB === selectedDept);
    }

    if (searchText.trim()) {
      const q = searchText.trim().toLowerCase();
      result = result.filter(
        (r) =>
          (r.HOTEN && r.HOTEN.toLowerCase().includes(q)) ||
          (r.MANV && r.MANV.toString().includes(q))
      );
    }

    return result;
  }, [safeChamCongList, statusFilter, selectedDept, searchText]);

  // Dữ liệu đã áp dụng thêm bộ lọc chuyên cần (áp dụng cho Bảng chi tiết và Drill-down)
  const filteredList = useMemo(() => {
    let result = baseFilteredList;

    if (attendanceFilter !== 'all') {
      const dayKey = `D${selectedDay}` as keyof KyCongChiTietDTO;
      result = result.filter((r) => {
        const val = r[dayKey] as string | undefined;
        const def = parseAttendanceSymbol(val);
        if (attendanceFilter === 'work') {
          return def.category === 'work_full' || def.category === 'work_half' || def.category === 'work_leave_mix';
        }
        if (attendanceFilter === 'leave') {
          return def.category === 'leave_full' || def.category === 'work_leave_mix';
        }
        if (attendanceFilter === 'trip') {
          return def.category === 'business_trip';
        }
        if (attendanceFilter === 'insurance') {
          return def.category === 'insurance';
        }
        if (attendanceFilter === 'unpaid') {
          return def.category === 'unpaid_leave';
        }
        if (attendanceFilter === 'holiday') {
          return def.category === 'holiday';
        }
        if (attendanceFilter === 'absent') {
          return def.category === 'absent';
        }
        if (attendanceFilter === 'anomaly') {
          return def.category === 'anomaly';
        }
        return true;
      });
    }

    return result;
  }, [baseFilteredList, attendanceFilter, selectedDay]);

  // Thống kê từng ngày từ danh sách nhân viên đã lọc theo phạm vi (chống biến mẫu số thành 100% khi drill-down)
  const monthDays: DayAttendanceMetrics[] = useMemo(() => {
    return Array.from({ length: daysInMonth }, (_, i) => {
      const day = i + 1;
      return calculateDayAttendance(day, month, year, baseFilteredList, lang);
    });
  }, [daysInMonth, year, month, baseFilteredList, lang]);

  const currentDayData: DayAttendanceMetrics = useMemo(() => {
    const found = monthDays.find((d) => d.day === selectedDay);
    if (found) return found;
    return (
      monthDays[0] || calculateDayAttendance(1, month, year, baseFilteredList, lang)
    );
  }, [monthDays, selectedDay, month, year, baseFilteredList, lang]);

  // Lọc chi tiết nhân viên trong ngày theo nhóm drill-down
  const currentDayRecords: EmployeeDayRecord[] = useMemo(() => {
    if (!currentDayData || !currentDayData.records) return [];
    if (drillDownCategory === 'work') {
      return currentDayData.records.filter(
        (r) =>
          r.symbolDef.category === 'work_full' ||
          r.symbolDef.category === 'work_half' ||
          r.symbolDef.category === 'work_leave_mix'
      );
    }
    if (drillDownCategory === 'leave') {
      return currentDayData.records.filter(
        (r) =>
          r.symbolDef.category === 'leave_full' ||
          r.symbolDef.category === 'work_leave_mix'
      );
    }
    if (drillDownCategory === 'absent') {
      return currentDayData.records.filter(
        (r) => r.symbolDef.category === 'absent' || r.symbolDef.category === 'anomaly'
      );
    }
    if (drillDownCategory === 'trip') {
      return currentDayData.records.filter((r) => r.symbolDef.category === 'business_trip');
    }
    if (drillDownCategory === 'insurance') {
      return currentDayData.records.filter((r) => r.symbolDef.category === 'insurance');
    }
    if (drillDownCategory === 'unpaid') {
      return currentDayData.records.filter((r) => r.symbolDef.category === 'unpaid_leave');
    }
    if (drillDownCategory === 'holiday') {
      return currentDayData.records.filter((r) => r.symbolDef.category === 'holiday');
    }
    return currentDayData.records;
  }, [currentDayData, drillDownCategory]);

  const chamCongColumns: ColumnsType<KyCongChiTietDTO> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'MANV',
      key: 'MANV',
      width: 75,
      fixed: 'left',
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: t('employee.colFullName'),
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      width: 170,
      fixed: 'left',
      render: (name: string, r) => (
        <Space direction="vertical" size={0}>
          <Text strong>{name}</Text>
          {r.TENPB && (
            <Text type="secondary" style={{ fontSize: 11 }}>
              {r.TENPB}
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: t('employee.colStatus'),
      key: 'status',
      fixed: 'left',
      width: 105,
      render: (_, r) => {
        if (r.DATHOIVIEC === 1) return <Tag color="default">{t('status.resigned')}</Tag>;
        return <Tag color="green">{t('status.active')}</Tag>;
      },
    },
    {
      title: t('attendance.colTotalWorkDays'),
      dataIndex: 'TONGNGAYCONG',
      key: 'TONGNGAYCONG',
      width: 100,
      fixed: 'left',
      render: (v: number) => (
        <Tag color="geekblue" style={{ fontWeight: 'bold' }}>
          {v ?? 0}
        </Tag>
      ),
    },
    {
      title: t('attendance.colPaidLeave'),
      dataIndex: 'NGAYPHEP',
      key: 'NGAYPHEP',
      width: 80,
      render: (v: number) => (v && v > 0 ? <Tag color="gold">{v} P</Tag> : <Text type="secondary">0</Text>),
    },
    {
      title: t('status.absent'),
      dataIndex: 'NGHIKHONGPHEP',
      key: 'NGHIKHONGPHEP',
      width: 80,
      render: (v: number) => (v && v > 0 ? <Tag color="red">{v} V</Tag> : <Text type="secondary">0</Text>),
    },
    ...Array.from({ length: 31 }, (_, i) => {
      const dayNum = i + 1;
      const dayKey = `D${dayNum}` as keyof KyCongChiTietDTO;
      const dateObj = new Date(year, month - 1, dayNum);
      const isSun = dateObj.getDay() === 0;
      const isSat = dateObj.getDay() === 6;
      const isHovered = hoveredColKey === dayKey;

      return {
        title: (
          <div
            style={{
              color: isHovered ? '#1d4ed8' : isSun ? '#ff4d4f' : isSat ? '#fa8c16' : undefined,
              fontWeight: isHovered ? 800 : 600,
              transition: 'all 0.1s ease',
            }}
          >
            {dayNum}
          </div>
        ),
        dataIndex: dayKey,
        key: dayKey,
        width: 44,
        align: 'center' as const,
        onCell: () => ({
          onMouseEnter: () => {
            if (hoveredColKey !== dayKey) setHoveredColKey(dayKey);
          },
        }),
        render: (val: string) => {
          if (!val || !val.trim()) return <span style={{ color: tokens.textMuted }}>-</span>;
          const def = parseAttendanceSymbol(val);
          if (def.symbol === 'X') return <span style={{ color: '#1677ff', fontWeight: 600 }}>X</span>;
          if (def.symbol === 'CN') return <span style={{ color: '#ff4d4f', fontWeight: 600 }}>CN</span>;
          return (
            <Tag color={def.badgeColor} style={{ padding: '0 4px', margin: 0, fontSize: 11, fontWeight: 600 }}>
              {def.symbol}
            </Tag>
          );
        },
      };
    }),
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16, width: '100%' }}>
      <PageHeader
        title={t('attendance.pageTitle') || 'Chấm Công & Quản Lý Ca'}
        subtitle={`Theo dõi lịch làm việc, ma trận công D1-D31, tỷ lệ chuyên cần và xử lý ngoại lệ theo kỳ công`}
        breadcrumbs={[
          { title: 'Công & lương' },
          { title: 'Bảng chấm công' },
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
            <Button
              icon={<ReloadOutlined spin={chamCongLoading} />}
              onClick={onRefresh}
              loading={chamCongLoading}
              style={{ borderRadius: 8 }}
            >
              {t('common.refresh')}
            </Button>
          </Space>
        }
      />

      <Card
        bordered={false}
        style={{
          borderRadius: borderRadiusLG,
          background: tokens.cardBg,
          border: `1px solid ${tokens.borderSubtle}`,
        }}
      >
        {/* THANH BỘ LỌC */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: 12,
            marginBottom: 16,
            padding: '12px 16px',
            background: tokens.cardSecondaryBg,
            borderRadius: 8,
            border: `1px solid ${tokens.borderSubtle}`,
          }}
        >
        <Space wrap size="middle">
          {/* Lọc trạng thái */}
          <Space>
            <UserOutlined style={{ color: colorPrimary }} />
            <Text strong>{t('common.status')}:</Text>
            <Radio.Group
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              buttonStyle="solid"
              size="small"
            >
              <Radio.Button value="all">
                {t('common.all')} ({statusCounts.total})
              </Radio.Button>
              <Radio.Button value="active">
                {t('status.active')} ({statusCounts.active})
              </Radio.Button>
              {statusCounts.resigned > 0 && (
                <Radio.Button value="resigned">
                  {t('status.resigned')} ({statusCounts.resigned})
                </Radio.Button>
              )}
            </Radio.Group>
          </Space>

          {/* Lọc phòng ban */}
          <Space>
            <ApartmentOutlined style={{ color: colorPrimary }} />
            <Text strong>{t('employee.labelDepartment')}:</Text>
            <Select
              value={selectedDept}
              onChange={setSelectedDept}
              style={{ minWidth: 180 }}
              size="small"
              options={departmentOptions}
            />
          </Space>

          {/* Tìm kiếm */}
          <Input
            placeholder={t('common.searchPlaceholder')}
            prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            allowClear
            size="small"
            style={{ width: 200 }}
          />

          {/* Lọc tình trạng chuyên cần */}
          <Space>
            <FilterOutlined style={{ color: tokens.textSecondary }} />
            <Select
              value={attendanceFilter}
              onChange={setAttendanceFilter}
              size="small"
              style={{ minWidth: 170 }}
              options={[
                { value: 'all', label: 'Tất cả chuyên cần' },
                { value: 'work', label: 'Đi làm (đủ / nửa ca)' },
                { value: 'leave', label: 'Nghỉ phép (P, P/X)' },
                { value: 'trip', label: 'Công tác (CT)' },
                { value: 'insurance', label: 'Chế độ BHXH (BH)' },
                { value: 'unpaid', label: 'Nghỉ không lương (KL)' },
                { value: 'anomaly', label: 'Bất thường / Ngoại lệ (?)' },
                { value: 'holiday', label: 'Nghỉ lễ (L)' },
                { value: 'absent', label: 'Vắng không phép (V)' },
              ]}
            />
          </Space>
        </Space>
      </div>

      <Tabs
        activeKey={activeView}
        onChange={setActiveView}
        items={[
          {
            key: 'calendar',
            label: (
              <span>
                <CalendarOutlined /> {t('attendance.pageTitle')} ({month < 10 ? '0' + month : month}/{year})
              </span>
            ),
            children: (
              <div>
                <Row gutter={[20, 20]}>
                  {/* CALENDAR HEATMAP GRID */}
                  <Col xs={24} lg={16}>
                    <div style={{ background: tokens.cardBg, borderRadius: 8, border: `1px solid ${tokens.borderSubtle}`, padding: 16 }}>
                      {/* Tiêu đề các thứ trong tuần */}
                      <div
                        style={{
                          display: 'grid',
                          gridTemplateColumns: 'repeat(7, 1fr)',
                          textAlign: 'center',
                          fontWeight: 700,
                          color: tokens.textSecondary,
                          marginBottom: 12,
                          fontSize: 13,
                        }}
                      >
                        {dayOfWeekHeaders.map((dh, idx) => (
                          <div
                            key={idx}
                            style={{
                              color: dh.isSun ? '#ef4444' : dh.isSat ? '#fa8c16' : undefined,
                            }}
                          >
                            {dh.name}
                          </div>
                        ))}
                      </div>

                      {/* Các ô ngày trong tháng */}
                      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 8 }}>
                        {Array.from({ length: startColOffset }).map((_, idx) => (
                          <div
                            key={`empty-${idx}`}
                            style={{
                              padding: '10px 8px',
                              borderRadius: 8,
                              background: isDark ? 'rgba(255,255,255,0.03)' : '#f8fafc',
                              border: `1px dashed ${tokens.borderSubtle}`,
                              opacity: 0.35,
                              minHeight: 74,
                            }}
                          />
                        ))}

                        {monthDays.map((item) => {
                          const isSelected = item.day === selectedDay;
                          const isToday = item.isToday;

                          // Màu nền & viền theo trạng thái nghiệp vụ chuẩn
                          let bg = isDark ? 'rgba(255,255,255,0.03)' : '#f8fafc';
                          let borderColor = tokens.borderSubtle;
                          let dotIcon = '🟢';

                          if (item.isSunday && item.distinctWorkedCount === 0) {
                            dotIcon = '⚪';
                            bg = isDark ? '#151B2B' : '#f8fafc';
                            borderColor = tokens.borderSubtle;
                          } else if (item.calendarDotColor === 'red') {
                            dotIcon = '🔴';
                            bg = isDark ? 'rgba(239, 68, 68, 0.12)' : '#fef2f2';
                            borderColor = isDark ? 'rgba(239, 68, 68, 0.3)' : '#fecaca';
                          } else if (item.calendarDotColor === 'amber') {
                            dotIcon = '🟠';
                            bg = isDark ? 'rgba(245, 158, 11, 0.12)' : '#fffbeb';
                            borderColor = isDark ? 'rgba(245, 158, 11, 0.3)' : '#fde68a';
                          } else if (item.calendarDotColor === 'blue') {
                            dotIcon = '🔵';
                            bg = isDark ? 'rgba(59, 130, 246, 0.12)' : '#eff6ff';
                            borderColor = isDark ? 'rgba(59, 130, 246, 0.3)' : '#dbeafe';
                          } else if (item.calendarDotColor === 'green') {
                            dotIcon = '🟢';
                            bg = isDark ? 'rgba(16, 185, 129, 0.12)' : '#f0fdf4';
                            borderColor = isDark ? 'rgba(16, 185, 129, 0.3)' : '#bbf7d0';
                          }

                          // Viền tím nổi bật cho ngày đang chọn
                          if (isSelected) {
                            bg = isDark ? 'rgba(109, 74, 255, 0.18)' : '#f3f0ff';
                            borderColor = tokens.primary;
                          }

                          return (
                            <div
                              key={item.day}
                              onClick={() => setSelectedDay(item.day)}
                              className="attendance-day-cell"
                              style={{
                                padding: '8px 6px',
                                borderRadius: 10,
                                background: bg,
                                border: isSelected ? `2px solid ${tokens.primary}` : `1px solid ${borderColor}`,
                                cursor: 'pointer',
                                textAlign: 'center',
                                position: 'relative',
                                minHeight: 80,
                                boxShadow: isSelected ? '0 4px 12px rgba(109, 74, 255, 0.22)' : 'none',
                              }}
                            >
                              {/* Số ngày & dấu hiệu hôm nay */}
                              <div
                                style={{
                                  fontWeight: 700,
                                  fontSize: 13,
                                  color: isSelected ? tokens.primary : tokens.textPrimary,
                                  display: 'flex',
                                  alignItems: 'center',
                                  justifyContent: 'center',
                                  gap: 4,
                                }}
                              >
                                <span>{item.day}</span>
                                {isToday && (
                                  <span
                                    style={{
                                      width: 5,
                                      height: 5,
                                      borderRadius: '50%',
                                      backgroundColor: tokens.primary,
                                      display: 'inline-block',
                                    }}
                                    title="Hôm nay"
                                  />
                                )}
                              </div>

                              {/* Icon chấm trạng thái */}
                              <div style={{ fontSize: 13, margin: '2px 0' }}>{dotIcon}</div>

                              {/* Số lượng người ghi nhận */}
                              {item.isSunday && item.distinctWorkedCount === 0 ? (
                                <div style={{ fontSize: 11, color: tokens.textMuted }}>-</div>
                              ) : (
                                <div style={{ fontSize: 12, fontWeight: 600, color: tokens.textPrimary }}>
                                  {item.distinctWorkedCount}
                                </div>
                              )}

                              {/* Badge nhãn phụ (Phep, Bat thuong, Cong tac, Le) */}
                              {item.calendarBadgeText && (
                                <div
                                  style={{
                                    fontSize: 10,
                                    fontWeight: 700,
                                    color:
                                      item.calendarDotColor === 'amber'
                                        ? '#d97706'
                                        : item.calendarDotColor === 'red'
                                        ? '#dc2626'
                                        : item.calendarDotColor === 'blue'
                                        ? '#2563eb'
                                        : tokens.textSecondary,
                                    marginTop: 1,
                                    lineHeight: 1.2,
                                  }}
                                >
                                  {item.calendarBadgeText}
                                </div>
                              )}
                            </div>
                          );
                        })}
                      </div>

                      {/* Chú giải lịch công trực quan ngay dưới lịch */}
                      <div
                        style={{
                          marginTop: 16,
                          padding: '10px 14px',
                          background: tokens.cardSecondaryBg,
                          borderRadius: 8,
                          border: `1px solid ${tokens.borderSubtle}`,
                          display: 'flex',
                          flexWrap: 'wrap',
                          alignItems: 'center',
                          gap: 12,
                          fontSize: 12,
                        }}
                      >
                        <Text strong style={{ color: tokens.textSecondary }}>Chú giải lịch:</Text>
                        <Space size="middle" wrap>
                          <Space size={4}><span>🟢</span><Text>Đi làm</Text></Space>
                          <Space size={4}><span>🟠</span><Text>Nghỉ phép (P, P/X)</Text></Space>
                          <Space size={4}><span>🔴</span><Text>Vắng / Ngoại lệ (V, ?)</Text></Space>
                          <Space size={4}><span>🔵</span><Text>Công tác (CT) / Nghỉ lễ (L)</Text></Space>
                          <Space size={4}><span>🟣</span><Text>BHXH (BH - ốm, thai sản)</Text></Space>
                          <Space size={4}><span>⚪</span><Text>Nghỉ tuần</Text></Space>
                        </Space>
                      </div>
                    </div>
                  </Col>

                  {/* CHI TIẾT NGÀY ĐANG CHỌN (THEO THIẾT KẾ ĐỐI CHIẾU) */}
                  <Col xs={24} lg={8}>
                    <Card
                      title={
                        <div>
                          <Text strong style={{ fontSize: 16, color: tokens.textPrimary }}>
                            {currentDayData.dayOfWeekName}, {currentDayData.dateStr}
                          </Text>
                          <div style={{ fontSize: 12, color: tokens.textSecondary, marginTop: 2 }}>
                            Tổng cộng: {filteredList.length} bản ghi ({currentDayData.activeHeadcount} đang làm việc)
                          </div>
                        </div>
                      }
                      size="small"
                      bordered={false}
                      style={{
                        background: tokens.cardBg,
                        borderRadius: borderRadiusLG,
                        border: `1px solid ${tokens.borderSubtle}`,
                      }}
                    >
                      <Space direction="vertical" style={{ width: '100%' }} size={14}>
                        {/* Thanh tỷ lệ đi làm hôm nay */}
                        <div>
                          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                            <Text style={{ fontSize: 13, color: tokens.textSecondary }}>Tỷ Lệ Đi Làm Hôm Nay:</Text>
                            <Text strong style={{ color: tokens.primary, fontSize: 14 }}>
                              {currentDayData.attendanceRateDisplay}
                            </Text>
                          </div>
                          <Progress
                            percent={currentDayData.attendanceRate ?? 0}
                            status={currentDayData.isSunday && currentDayData.distinctWorkedCount === 0 ? 'normal' : 'active'}
                            strokeColor={
                              currentDayData.isSunday && currentDayData.distinctWorkedCount === 0
                                ? tokens.textMuted
                                : tokens.primary
                            }
                          />
                        </div>

                        {/* Thẻ 1: Đang làm việc */}
                        <div
                          onClick={() => setDrillDownCategory(drillDownCategory === 'work' ? 'all' : 'work')}
                          style={{
                            display: 'flex',
                            flexDirection: 'column',
                            padding: '10px 14px',
                            background: isDark ? 'rgba(16, 185, 129, 0.12)' : '#f0fdf4',
                            borderRadius: 8,
                            border: `1px solid ${isDark ? 'rgba(16, 185, 129, 0.25)' : '#bbf7d0'}`,
                            cursor: 'pointer',
                            transition: 'all 0.15s ease',
                          }}
                        >
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Space>
                              <CheckCircleOutlined style={{ color: '#16a34a' }} />
                              <Text strong style={{ color: isDark ? '#34d399' : '#15803d' }}>Đang làm việc</Text>
                            </Space>
                            <Text strong style={{ color: '#16a34a', fontSize: 16 }}>
                              {currentDayData.distinctWorkedCount}
                            </Text>
                          </div>
                          <div style={{ fontSize: 11, color: isDark ? '#a7f3d0' : '#166534', marginTop: 4 }}>
                            {currentDayData.workFullCount} đủ ngày • {currentDayData.workHalfCount} nửa ngày {currentDayData.workLeaveMixCount > 0 ? `• ${currentDayData.workLeaveMixCount} nửa làm/phép` : ''}
                          </div>
                        </div>

                        {/* Thẻ 2: Nghỉ phép */}
                        <div
                          onClick={() => setDrillDownCategory(drillDownCategory === 'leave' ? 'all' : 'leave')}
                          style={{
                            display: 'flex',
                            flexDirection: 'column',
                            padding: '10px 14px',
                            background: isDark ? 'rgba(245, 158, 11, 0.12)' : '#fffbeb',
                            borderRadius: 8,
                            border: `1px solid ${isDark ? 'rgba(245, 158, 11, 0.25)' : '#fde68a'}`,
                            cursor: 'pointer',
                            transition: 'all 0.15s ease',
                          }}
                        >
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Space>
                              <CalendarOutlined style={{ color: '#d97706' }} />
                              <Text strong style={{ color: isDark ? '#fbbf24' : '#b45309' }}>Nghỉ phép</Text>
                            </Space>
                            <Text strong style={{ color: '#d97706', fontSize: 16 }}>
                              {currentDayData.leaveFullCount + currentDayData.workLeaveMixCount}
                            </Text>
                          </div>
                          <div style={{ fontSize: 11, color: isDark ? '#fde68a' : '#92400e', marginTop: 4 }}>
                            {currentDayData.leaveFullCount} cả ngày {currentDayData.workLeaveMixCount > 0 ? `• ${currentDayData.workLeaveMixCount} nửa ngày (P/X)` : ''}
                          </div>
                        </div>

                        {/* Thẻ 3: Vắng mặt / Ngoại lệ */}
                        <div
                          onClick={() => setDrillDownCategory(drillDownCategory === 'absent' ? 'all' : 'absent')}
                          style={{
                            display: 'flex',
                            flexDirection: 'column',
                            padding: '10px 14px',
                            background: isDark ? 'rgba(239, 68, 68, 0.12)' : '#fef2f2',
                            borderRadius: 8,
                            border: `1px solid ${isDark ? 'rgba(239, 68, 68, 0.25)' : '#fecaca'}`,
                            cursor: 'pointer',
                            transition: 'all 0.15s ease',
                          }}
                        >
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Space>
                              <CloseCircleOutlined style={{ color: '#dc2626' }} />
                              <Text strong style={{ color: isDark ? '#f87171' : '#b91c1c' }}>Vắng mặt / Ngoại lệ</Text>
                            </Space>
                            <Text strong style={{ color: '#dc2626', fontSize: 16 }}>
                              {currentDayData.absentCount + currentDayData.anomalyCount}
                            </Text>
                          </div>
                          <div style={{ fontSize: 11, color: isDark ? '#fca5a5' : '#991b1b', marginTop: 4 }}>
                            {currentDayData.absentCount} không phép • {currentDayData.anomalyCount} bất thường/chờ duyệt
                          </div>
                        </div>

                        {/* Các thành phần bổ sung (Công tác, BHXH, Không lương, Nghỉ lễ) */}
                        {(currentDayData.businessTripCount > 0 || currentDayData.insuranceCount > 0 || currentDayData.unpaidLeaveCount > 0 || currentDayData.holidayCount > 0) && (
                          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                            {currentDayData.businessTripCount > 0 && (
                              <Tag color="cyan" style={{ borderRadius: 4, cursor: 'pointer' }} onClick={() => setDrillDownCategory('trip')}>
                                ✈️ Công tác: {currentDayData.businessTripCount}
                              </Tag>
                            )}
                            {currentDayData.insuranceCount > 0 && (
                              <Tag color="magenta" style={{ borderRadius: 4, cursor: 'pointer' }} onClick={() => setDrillDownCategory('insurance')}>
                                🏥 BHXH: {currentDayData.insuranceCount}
                              </Tag>
                            )}
                            {currentDayData.unpaidLeaveCount > 0 && (
                              <Tag color="volcano" style={{ borderRadius: 4, cursor: 'pointer' }} onClick={() => setDrillDownCategory('unpaid')}>
                                📄 Không lương: {currentDayData.unpaidLeaveCount}
                              </Tag>
                            )}
                            {currentDayData.holidayCount > 0 && (
                              <Tag color="geekblue" style={{ borderRadius: 4, cursor: 'pointer' }} onClick={() => setDrillDownCategory('holiday')}>
                                🎉 Nghỉ lễ: {currentDayData.holidayCount}
                              </Tag>
                            )}
                          </div>
                        )}

                        {/* Danh sách nhân viên trong ngày đang chọn */}
                        <div style={{ marginTop: 4 }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                            <Text strong style={{ fontSize: 13, color: tokens.textPrimary }}>
                              Chi tiết nhân viên ({currentDayRecords.length}):
                            </Text>
                            {drillDownCategory !== 'all' && (
                              <Button size="small" type="link" onClick={() => setDrillDownCategory('all')} style={{ padding: 0, fontSize: 12 }}>
                                Hiện tất cả
                              </Button>
                            )}
                          </div>

                          <div
                            style={{
                              maxHeight: 200,
                              overflowY: 'auto',
                              display: 'flex',
                              flexDirection: 'column',
                              gap: 6,
                              paddingRight: 2,
                            }}
                          >
                            {currentDayRecords.length > 0 ? (
                              currentDayRecords.map((rec, idx) => (
                                <div
                                  key={`${rec.manv}-${rec.symbol}-${idx}`}
                                  style={{
                                    display: 'flex',
                                    justifyContent: 'space-between',
                                    alignItems: 'center',
                                    padding: '8px 10px',
                                    background: tokens.cardSecondaryBg,
                                    borderRadius: 6,
                                    border: `1px solid ${tokens.borderSubtle}`,
                                    fontSize: 12,
                                  }}
                                >
                                  <div>
                                    <Text strong style={{ color: tokens.textPrimary }}>{rec.hoten}</Text>{' '}
                                    <Text type="secondary">(#{rec.manv})</Text>
                                    <div style={{ color: tokens.textSecondary, fontSize: 11 }}>
                                      {rec.tenpb || 'Chưa phân bổ PB'}
                                    </div>
                                  </div>
                                  <div style={{ textAlign: 'right' }}>
                                    <Tag color={rec.symbolDef.badgeColor} style={{ margin: 0, fontWeight: 600 }}>
                                      {rec.symbol}: {rec.symbolDef.name}
                                    </Tag>
                                    {(rec.workUnits > 0 || rec.leaveUnits > 0) && (
                                      <div style={{ fontSize: 10, color: tokens.textSecondary, marginTop: 2 }}>
                                        {rec.workUnits > 0 ? `${rec.workUnits} công ` : ''}
                                        {rec.leaveUnits > 0 ? `${rec.leaveUnits} phép` : ''}
                                      </div>
                                    )}
                                  </div>
                                </div>
                              ))
                            ) : (
                              <div style={{ textAlign: 'center', padding: '16px 0', color: tokens.textMuted, fontSize: 12 }}>
                                {currentDayData.isSunday ? 'Chủ nhật nghỉ tuần' : 'Không có nhân viên trong nhóm đã chọn'}
                              </div>
                            )}
                          </div>
                        </div>

                        <Button
                          type="dashed"
                          block
                          icon={<TableOutlined />}
                          onClick={() => setActiveView('bangcong')}
                          style={{ borderRadius: 8 }}
                        >
                          {t('attendance.tabTimesheets')} ({filteredList.length})
                        </Button>
                      </Space>
                    </Card>
                  </Col>
                </Row>
              </div>
            ),
          },
          {
            key: 'bangcong',
            label: (
              <span>
                <TableOutlined /> {t('attendance.tabTimesheets')} ({filteredList.length})
              </span>
            ),
            children: (
              <div onMouseLeave={() => setHoveredColKey(null)}>
                <Table
                  className="cham-cong-matrix-table"
                  columns={chamCongColumns}
                  dataSource={filteredList}
                  rowKey="MANV"
                  loading={chamCongLoading}
                  scroll={{ x: 'max-content' }}
                  pagination={{
                    pageSize: 15,
                    showSizeChanger: true,
                    pageSizeOptions: ['15', '30', '50', '100'],
                    showTotal: (total) => t('common.totalRecords', { total }),
                  }}
                  size="small"
                />
              </div>
            ),
          },
          {
            key: 'loaica',
            label: `⚙️ ${t('attendance.tabShifts')}`,
            children: (
              <Row gutter={[16, 16]}>
                <Col xs={24} md={12}>
                  <Card title={t('attendance.tabShifts')} size="small">
                    <Table
                      scroll={{ x: 'max-content' }}
                      columns={[
                        { title: t('employee.colEmpCode'), dataIndex: 'IDLOAICA', key: 'IDLOAICA', width: 70 },
                        { title: t('attendance.shiftName'), dataIndex: 'TENLOAICA', key: 'TENLOAICA' },
                        {
                          title: t('attendance.shiftCoefficient'),
                          dataIndex: 'HESOLOAICA',
                          key: 'HESOLOAICA',
                          render: (h: number) => <Tag color="blue">{h ?? 1.0}x</Tag>,
                        },
                      ]}
                      dataSource={loaiCaList}
                      rowKey="IDLOAICA"
                      pagination={false}
                    />
                  </Card>
                </Col>
                <Col xs={24} md={12}>
                  <Card title={t('attendance.tabWorkTypes')} size="small">
                    <ul style={{ lineHeight: '28px', margin: 0, paddingLeft: 20 }}>
                      <li><Tag color="blue">X</Tag> : Đi làm đủ ca chuẩn (1.0 công)</li>
                      <li><Tag color="cyan">X/2</Tag> : Làm nửa ngày (0.5 công)</li>
                      <li><Tag color="gold">P</Tag> : Nghỉ phép năm có lương (1.0 ngày)</li>
                      <li><Tag color="orange">P/X</Tag> : Nửa ngày phép + nửa ngày làm (0.5 + 0.5)</li>
                      <li><Tag color="geekblue">L</Tag> : Nghỉ lễ tết có lương theo luật</li>
                      <li><Tag color="magenta">BH</Tag> : Nghỉ chế độ BHXH (ốm đau, thai sản)</li>
                      <li><Tag color="volcano">KL</Tag> : Nghỉ việc riêng không hưởng lương</li>
                      <li><Tag color="cyan">CT</Tag> : Đi công tác</li>
                      <li><Tag color="purple">CD</Tag> : Ca đêm / chế độ đặc thù</li>
                      <li><Tag color="default">RO</Tag> : Nghỉ bù</li>
                      <li><Tag color="red">V</Tag> : Vắng không phép</li>
                      <li><Tag color="red">?</Tag> : Ngoại lệ / thiếu quẹt thẻ / cần xác minh</li>
                      <li><Tag color="default">N / CN</Tag> : Nghỉ tuần / không phân ca</li>
                    </ul>
                  </Card>
                </Col>
              </Row>
            ),
          },
          {
            key: 'caphienban',
            label: `🗂️ Phiên bản ca (${caPhienBanList.length})`,
            children: (
              <Card title="Cấu hình phiên bản ca làm việc & Khung giờ (V1_18)" size="small">
                <Table
                  scroll={{ x: 'max-content' }}
                  loading={extraLoading}
                  columns={[
                    { title: 'ID', dataIndex: 'idCaPhienBan', key: 'idCaPhienBan', width: 70 },
                    { title: 'Loại ca', dataIndex: 'tenLoaiCa', key: 'tenLoaiCa', width: 120 },
                    {
                      title: 'Phiên bản',
                      dataIndex: 'soPhienBan',
                      key: 'soPhienBan',
                      width: 90,
                      render: (v: number) => <Tag color="geekblue">v{v}</Tag>,
                    },
                    { title: 'Tên phiên bản', dataIndex: 'tenPhienBan', key: 'tenPhienBan' },
                    { title: 'Hiệu lực từ', dataIndex: 'tuNgay', key: 'tuNgay', width: 110 },
                    { title: 'Đến ngày', dataIndex: 'denNgay', key: 'denNgay', width: 110 },
                    {
                      title: 'Giờ chuẩn',
                      dataIndex: 'gioChuan',
                      key: 'gioChuan',
                      width: 100,
                      render: (g: number) => <Tag color="blue">{g}h</Tag>,
                    },
                    {
                      title: 'Công quy đổi',
                      dataIndex: 'congQuyDoi',
                      key: 'congQuyDoi',
                      width: 110,
                      render: (c: number) => <span>{c} công</span>,
                    },
                    {
                      title: 'Trạng thái',
                      dataIndex: 'trangThai',
                      key: 'trangThai',
                      width: 120,
                      render: (st: string) => (
                        <Tag color={st === 'PUBLISHED' ? 'green' : 'orange'}>{st}</Tag>
                      ),
                    },
                  ]}
                  dataSource={caPhienBanList}
                  rowKey="idCaPhienBan"
                  pagination={{ pageSize: 10 }}
                />
              </Card>
            ),
          },
          {
            key: 'lichlamviec',
            label: `📅 Lịch làm việc (${lichLamViecList.length})`,
            children: (
              <Card
                title={`Phân ca & Lịch làm việc nhân viên (Kỳ ${selectedKyCong})`}
                size="small"
                extra={
                  <Button icon={<ReloadOutlined />} onClick={fetchExtraAttendanceData} loading={extraLoading}>
                    Làm mới
                  </Button>
                }
              >
                <Table
                  scroll={{ x: 'max-content' }}
                  loading={extraLoading}
                  columns={[
                    { title: 'Mã NV', dataIndex: 'employeeCode', key: 'employeeCode', width: 100 },
                    { title: 'Họ và tên', dataIndex: 'hoten', key: 'hoten', width: 180 },
                    { title: 'Ngày làm', dataIndex: 'ngay', key: 'ngay', width: 110 },
                    { title: 'Phiên bản ca', dataIndex: 'tenPhienBan', key: 'tenPhienBan' },
                    {
                      title: 'Loại ngày',
                      dataIndex: 'loaiNgay',
                      key: 'loaiNgay',
                      width: 130,
                      render: (ln: string) => (
                        <Tag color={ln === 'NGAY_THUONG' ? 'blue' : ln === 'NGAY_LE' ? 'magenta' : 'orange'}>
                          {ln}
                        </Tag>
                      ),
                    },
                    {
                      title: 'Trạng thái phân công',
                      dataIndex: 'trangThaiPhanCong',
                      key: 'trangThaiPhanCong',
                      width: 150,
                      render: (st: string) => <Tag color="green">{st}</Tag>,
                    },
                    { title: 'Mã phân công', dataIndex: 'maPhanCong', key: 'maPhanCong', width: 180 },
                  ]}
                  dataSource={lichLamViecList}
                  rowKey="idLich"
                  pagination={{
                    pageSize: 15,
                    showSizeChanger: true,
                    pageSizeOptions: ['15', '30', '50', '100'],
                  }}
                />
              </Card>
            ),
          },
          {
            key: 'batthuong',
            label: (
              <span>
                ⚠️ Ngoại lệ công{' '}
                <Badge
                  count={batThuongList.filter((b) => b.chanChot && b.trangThai === 'CHO_XU_LY').length}
                  style={{ backgroundColor: '#ef4444' }}
                />
              </span>
            ),
            children: (
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                {batThuongList.some((b) => b.chanChot && b.trangThai === 'CHO_XU_LY') && (
                  <Alert
                    type="error"
                    showIcon
                    icon={<ExclamationCircleOutlined />}
                    message="Cảnh báo ngoại lệ chặn chốt kỳ công (Intentional Test Exceptions)"
                    description="Kỳ công tháng 09/2026 chứa các ngoại lệ công cố ý chưa được xác minh (Bao gồm MANV 5, 6, 7, 8, 9, 17, 18). Hệ thống kiên quyết CHẶN CHỐT KỲ CÔNG để bảo đảm tính toàn vẹn dữ liệu, không tự ý gán công giả lập!"
                  />
                )}

                <Card
                  title={`Danh sách ngoại lệ & bất thường chấm công (${batThuongList.length})`}
                  size="small"
                  extra={
                    <Button icon={<ReloadOutlined />} onClick={fetchExtraAttendanceData} loading={extraLoading}>
                      Làm mới
                    </Button>
                  }
                >
                  <Table
                    scroll={{ x: 'max-content' }}
                    loading={extraLoading}
                    columns={[
                      { title: 'ID', dataIndex: 'idBatThuong', key: 'idBatThuong', width: 70 },
                      { title: 'Mã NV', dataIndex: 'employeeCode', key: 'employeeCode', width: 90 },
                      { title: 'Họ tên', dataIndex: 'hoten', key: 'hoten', width: 170 },
                      { title: 'Ngày', dataIndex: 'ngay', key: 'ngay', width: 110 },
                      {
                        title: 'Mã lỗi',
                        dataIndex: 'maLoi',
                        key: 'maLoi',
                        render: (ml: string) => <Tag color="volcano">{ml}</Tag>,
                      },
                      {
                        title: 'Mức độ',
                        dataIndex: 'mucDo',
                        key: 'mucDo',
                        width: 140,
                        render: (md: string) => (
                          <Tag color={md === 'NGHIEP_VU_TREO' ? 'red' : 'gold'}>{md}</Tag>
                        ),
                      },
                      {
                        title: 'Chặn chốt',
                        dataIndex: 'chanChot',
                        key: 'chanChot',
                        width: 110,
                        render: (cc: boolean) =>
                          cc ? <Tag color="red">CHẶN CHỐT</Tag> : <Tag color="default">Không chặn</Tag>,
                      },
                      {
                        title: 'Trạng thái',
                        dataIndex: 'trangThai',
                        key: 'trangThai',
                        width: 130,
                        render: (st: string) => (
                          <Tag color={st === 'CHO_XU_LY' ? 'orange' : st === 'DA_XAC_MINH' ? 'green' : 'default'}>
                            {st}
                          </Tag>
                        ),
                      },
                      { title: 'Mô tả ngoại lệ', dataIndex: 'moTa', key: 'moTa' },
                      {
                        title: 'Thao tác',
                        key: 'action',
                        width: 120,
                        render: (_, record: BatThuongDTO) => (
                          <Button
                            size="small"
                            type="primary"
                            ghost
                            onClick={() => {
                              setSelectedAnomaly(record);
                              setResolveAction('VERIFY');
                              setResolveReason('');
                              setResolveModalVisible(true);
                            }}
                          >
                            Xử lý
                          </Button>
                        ),
                      },
                    ]}
                    dataSource={batThuongList}
                    rowKey="idBatThuong"
                    pagination={{ pageSize: 10 }}
                  />
                </Card>
              </Space>
            ),
          },
          {
            key: 'congbo_chot',
            label: '🔒 Công bố & Chốt kỳ công',
            children: (
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                {/* READINESS & ACTIONS BANNER */}
                <Row gutter={[16, 16]}>
                  <Col xs={24} sm={12} md={6}>
                    <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                      <Text type="secondary">Trạng thái khóa kỳ</Text>
                      <div style={{ marginTop: 8 }}>
                        {readiness?.isLocked ? (
                          <Tag color="red" style={{ fontSize: 14, padding: '4px 10px' }}>
                            <LockOutlined /> ĐÃ KHÓA
                          </Tag>
                        ) : (
                          <Tag color="green" style={{ fontSize: 14, padding: '4px 10px' }}>
                            <UnlockOutlined /> ĐANG MỞ
                          </Tag>
                        )}
                      </div>
                    </Card>
                  </Col>

                  <Col xs={24} sm={12} md={6}>
                    <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                      <Text type="secondary">Điều kiện chốt kỳ</Text>
                      <div style={{ marginTop: 8 }}>
                        {readiness?.isReadyToLock ? (
                          <Tag color="green" style={{ fontSize: 14, padding: '4px 10px' }}>
                            <CheckCircleOutlined /> ĐỦ ĐIỀU KIỆN
                          </Tag>
                        ) : (
                          <Tag color="volcano" style={{ fontSize: 14, padding: '4px 10px' }}>
                            <WarningOutlined /> CHƯA ĐỦ ĐIỀU KIỆN
                          </Tag>
                        )}
                      </div>
                    </Card>
                  </Col>

                  <Col xs={24} sm={12} md={6}>
                    <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                      <Text type="secondary">Phiên bản Input / Publish</Text>
                      <div style={{ marginTop: 8, fontWeight: 700, fontSize: 16 }}>
                        Rev {readiness?.inputRev ?? 0} / {readiness?.publishRev ?? 0}
                      </div>
                    </Card>
                  </Col>

                  <Col xs={24} sm={12} md={6}>
                    <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                      <Text type="secondary">Bất thường chặn chốt</Text>
                      <div style={{ marginTop: 8, fontWeight: 700, fontSize: 16, color: '#ef4444' }}>
                        {readiness?.unverifiedCount ?? 0} trường hợp
                      </div>
                    </Card>
                  </Col>
                </Row>

                {readiness?.blockingReasons && readiness.blockingReasons.length > 0 && (
                  <Alert
                    type="warning"
                    showIcon
                    message="Các lý do chưa đủ điều kiện chốt kỳ công:"
                    description={
                      <ul style={{ margin: 0, paddingLeft: 20 }}>
                        {readiness.blockingReasons.map((r, idx) => (
                          <li key={idx}>{r}</li>
                        ))}
                      </ul>
                    }
                  />
                )}

                <Card title="Thao tác vận hành công bố & chốt kỳ" size="small">
                  <Space wrap size="middle">
                    <Button
                      type="primary"
                      icon={<PlayCircleOutlined />}
                      onClick={handlePublishAttendance}
                      loading={publishing}
                    >
                      Tính toán & Công bố công (Publish Attendance)
                    </Button>

                    <Popconfirm
                      title="Xác nhận khóa kỳ công"
                      description="Sau khi khóa kỳ công, không thể thay đổi lịch làm việc hoặc sửa đổi dữ liệu chấm công. Bạn có chắc chắn muốn khóa?"
                      onConfirm={handleLockKyCong}
                      okText="Khóa kỳ công"
                      cancelText="Hủy"
                      disabled={readiness?.isLocked || !readiness?.isReadyToLock}
                    >
                      <Button
                        type="primary"
                        danger
                        icon={<LockOutlined />}
                        disabled={readiness?.isLocked || !readiness?.isReadyToLock}
                      >
                        Chốt & Khóa kỳ công
                      </Button>
                    </Popconfirm>

                    <Button
                      icon={<UnlockOutlined />}
                      onClick={() => setUnlockModalVisible(true)}
                      disabled={!readiness?.isLocked}
                    >
                      Mở khóa kỳ công
                    </Button>

                    <Button icon={<ReloadOutlined />} onClick={fetchExtraAttendanceData} loading={extraLoading}>
                      Kiểm tra lại tính sẵn sàng
                    </Button>
                  </Space>
                </Card>

                {/* CALCULATION RUNS HISTORY */}
                <Card title="Lịch sử các lần tính toán & công bố (TB_CHAMCONG_LANTINH)" size="small">
                  <Table
                    scroll={{ x: 'max-content' }}
                    loading={extraLoading}
                    columns={[
                      { title: 'ID Lần tính', dataIndex: 'idLanTinh', key: 'idLanTinh', width: 90 },
                      { title: 'Mã yêu cầu', dataIndex: 'maYeuCau', key: 'maYeuCau' },
                      { title: 'Khoảng tính', render: (_, r: LanTinhDTO) => `${r.tuNgay} - ${r.denNgay}` },
                      { title: 'Input Rev', dataIndex: 'inputRev', key: 'inputRev', width: 90 },
                      {
                        title: 'Trạng thái',
                        dataIndex: 'trangThai',
                        key: 'trangThai',
                        width: 120,
                        render: (st: string) => (
                          <Tag color={st === 'HOAN_TAT' ? 'green' : 'orange'}>{st}</Tag>
                        ),
                      },
                      { title: 'Bắt đầu tính', dataIndex: 'batDauTinh', key: 'batDauTinh', width: 160 },
                      { title: 'Công bố lúc', dataIndex: 'congBoLuc', key: 'congBoLuc', width: 160 },
                      { title: 'Ghi chú', dataIndex: 'ghiChu', key: 'ghiChu' },
                    ]}
                    dataSource={lanTinhList}
                    rowKey="idLanTinh"
                    pagination={{ pageSize: 5 }}
                  />
                </Card>
              </Space>
            ),
          },
        ]}
      />

      {/* Modal Xử lý ngoại lệ công */}
      <Modal
        title="Quyết định xử lý ngoại lệ công (Audit Trace)"
        open={resolveModalVisible}
        onCancel={() => setResolveModalVisible(false)}
        onOk={handleResolveAnomaly}
        okText="Lưu quyết định"
        cancelText="Hủy"
      >
        {selectedAnomaly && (
          <Space direction="vertical" style={{ width: '100%' }} size="middle">
            <div>
              <Text strong>Nhân viên: </Text>
              <span>{selectedAnomaly.hoten} ({selectedAnomaly.employeeCode})</span>
            </div>
            <div>
              <Text strong>Ngày công: </Text>
              <span>{selectedAnomaly.ngay}</span>
            </div>
            <div>
              <Text strong>Nội dung ngoại lệ: </Text>
              <Tag color="volcano">{selectedAnomaly.maLoi}</Tag>
              <div style={{ marginTop: 4, color: '#64748b' }}>{selectedAnomaly.moTa}</div>
            </div>

            <div>
              <Text strong>Hành động xử lý: </Text>
              <Radio.Group
                value={resolveAction}
                onChange={(e) => setResolveAction(e.target.value)}
                style={{ marginTop: 6 }}
              >
                <Radio value="VERIFY">Xác minh hợp lệ (DA_XAC_MINH)</Radio>
                <Radio value="IGNORE">Bỏ qua ngoại lệ (BO_QUA)</Radio>
                <Radio value="REJECT">Từ chối / Vi phạm (TU_CHOI)</Radio>
              </Radio.Group>
            </div>

            <div>
              <Text strong>Lý do xử lý (Bắt buộc lưu kiểm toán):</Text>
              <Input.TextArea
                rows={3}
                placeholder="Nhập lý do hoặc chứng cứ giải trình..."
                value={resolveReason}
                onChange={(e) => setResolveReason(e.target.value)}
                style={{ marginTop: 6 }}
              />
            </div>
          </Space>
        )}
      </Modal>

      {/* Modal Mở khóa kỳ công */}
      <Modal
        title="Mở khóa kỳ công (Yêu cầu thẩm quyền & Lý do kiểm toán)"
        open={unlockModalVisible}
        onCancel={() => setUnlockModalVisible(false)}
        onOk={handleUnlockKyCong}
        okText="Mở khóa kỳ"
        cancelText="Hủy"
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <Alert
            type="warning"
            message="Lưu ý khi mở khóa kỳ công"
            description="Mở khóa kỳ công cho phép cập nhật lại chấm công và phân ca, tuy nhiên sẽ làm mất tính bất biến của dữ liệu lương đã chốt nếu có. Bắt buộc nhập lý do mở khóa."
          />
          <div style={{ marginTop: 8 }}>
            <Text strong>Lý do mở khóa kỳ công:</Text>
            <Input.TextArea
              rows={3}
              placeholder="Nhập lý do mở khóa..."
              value={unlockReason}
              onChange={(e) => setUnlockReason(e.target.value)}
              style={{ marginTop: 6 }}
            />
          </div>
        </Space>
      </Modal>
    </Card>
    </div>
  );
};

export default ChamCongPage;
