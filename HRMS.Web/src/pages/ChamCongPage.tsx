import React, { useState, useMemo } from 'react';
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
} from '@ant-design/icons';
import type { KyCongDTO, KyCongChiTietDTO, LoaiCaDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

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

interface DayException {
  manv: number;
  hoten: string;
  tenpb?: string;
  symbol: string;
  reason: string;
}

interface DayStat {
  day: number;
  dateStr: string;
  dayOfWeekName: string;
  isWeekend: boolean;
  present: number;
  late: number;
  absent: number;
  leave: number;
  trip: number;
  exceptions: DayException[];
  status: 'good' | 'late' | 'bad' | 'weekend';
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
  const {
    token: { borderRadiusLG, colorPrimary },
  } = theme.useToken();

  const [activeView, setActiveView] = useState<string>('calendar');
  const [selectedDept, setSelectedDept] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<'active' | 'all' | 'resigned'>('all');
  const [searchText, setSearchText] = useState<string>('');
  const [attendanceFilter, setAttendanceFilter] = useState<string>('all');
  const [selectedDay, setSelectedDay] = useState<number>(() => {
    const today = new Date();
    return today.getDate();
  });
  const [hoveredColKey, setHoveredColKey] = useState<string | null>(null);

  const safeChamCongList = Array.isArray(chamCongList) ? chamCongList : [];
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

  // Dữ liệu đã lọc áp dụng chung cho cả Heatmap và Bảng chi tiết
  const filteredList = useMemo(() => {
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

    if (attendanceFilter === 'leave') {
      result = result.filter((r) => (r.NGAYPHEP ?? 0) > 0);
    } else if (attendanceFilter === 'absent') {
      result = result.filter((r) => (r.NGHIKHONGPHEP ?? 0) > 0);
    } else if (attendanceFilter === 'full') {
      result = result.filter(
        (r) =>
          (r.NGAYPHEP ?? 0) === 0 &&
          (r.NGHIKHONGPHEP ?? 0) === 0 &&
          (r.TONGNGAYCONG ?? 0) >= 24
      );
    }

    return result;
  }, [safeChamCongList, statusFilter, selectedDept, searchText, attendanceFilter]);

  // Thống kê từng ngày từ danh sách nhân viên đã lọc
  const monthDays: DayStat[] = useMemo(() => {
    return Array.from({ length: daysInMonth }, (_, i) => {
      const day = i + 1;
      const dateObj = new Date(year, month - 1, day);
      const dayOfWeek = dateObj.getDay();
      const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
      const dayKey = `D${day}` as keyof KyCongChiTietDTO;
      const dayOfWeekName = new Intl.DateTimeFormat(lang === 'zh-CN' ? 'zh-CN' : lang, { weekday: 'long' }).format(dateObj);

      let presentCount = 0;
      let leaveCount = 0;
      let absentCount = 0;
      let tripCount = 0;
      const exceptions: DayException[] = [];

      if (filteredList.length > 0) {
        filteredList.forEach((row) => {
          const rawVal = (row[dayKey] as string)?.trim()?.toUpperCase();
          if (!rawVal) return;

          if (rawVal === 'X' || rawVal === 'X/2' || rawVal === '1' || rawVal === 'CD') {
            presentCount++;
          } else if (rawVal === 'P' || rawVal === 'F') {
            leaveCount++;
            exceptions.push({
              manv: row.MANV,
              hoten: row.HOTEN,
              tenpb: row.TENPB,
              symbol: rawVal,
              reason: t('status.leavePaid'),
            });
          } else if (rawVal === 'V' || rawVal === 'RO') {
            absentCount++;
            exceptions.push({
              manv: row.MANV,
              hoten: row.HOTEN,
              tenpb: row.TENPB,
              symbol: rawVal,
              reason: t('status.absent'),
            });
          } else if (rawVal === 'CT') {
            tripCount++;
            exceptions.push({
              manv: row.MANV,
              hoten: row.HOTEN,
              tenpb: row.TENPB,
              symbol: rawVal,
              reason: t('common.description'),
            });
          }
        });
      }

      let status: DayStat['status'] = 'good';
      if (isWeekend) status = 'weekend';
      else if (absentCount > 0) status = 'bad';
      else if (leaveCount > 0) status = 'late';

      return {
        day,
        dateStr: `${day < 10 ? '0' + day : day}/${month < 10 ? '0' + month : month}/${year}`,
        dayOfWeekName,
        isWeekend,
        present: presentCount,
        late: 0,
        absent: absentCount,
        leave: leaveCount,
        trip: tripCount,
        exceptions,
        status,
      };
    });
  }, [daysInMonth, year, month, filteredList, lang, t]);

  const currentDayData = useMemo(() => {
    return (
      monthDays.find((d) => d.day === selectedDay) ||
      monthDays[0] || {
        day: 1,
        dateStr: `01/${month}/${year}`,
        dayOfWeekName: '',
        isWeekend: false,
        present: 0,
        late: 0,
        absent: 0,
        leave: 0,
        trip: 0,
        exceptions: [],
        status: 'good',
      }
    );
  }, [monthDays, selectedDay, month, year]);

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
          if (!val) return <span style={{ color: '#d9d9d9' }}>-</span>;
          const u = val.trim().toUpperCase();
          if (u === 'X') return <span style={{ color: '#1677ff', fontWeight: 600 }}>X</span>;
          if (u === 'CN') return <span style={{ color: '#ff4d4f', fontWeight: 600 }}>CN</span>;
          if (u === 'P') return <Tag color="gold" style={{ padding: '0 4px', margin: 0 }}>P</Tag>;
          if (u === 'V' || u === 'RO') return <Tag color="red" style={{ padding: '0 4px', margin: 0 }}>V</Tag>;
          if (u === 'CT') return <Tag color="cyan" style={{ padding: '0 4px', margin: 0 }}>CT</Tag>;
          return <span style={{ fontSize: 12 }}>{val}</span>;
        },
      };
    }),
  ];

  return (
    <Card
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <CalendarOutlined style={{ color: '#1677ff', fontSize: 20 }} />
          <span>{t('attendance.pageTitle')}</span>
        </div>
      }
      extra={
        <Space wrap>
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
          <Button icon={<ReloadOutlined />} onClick={onRefresh} loading={chamCongLoading}>
            {t('common.refresh')}
          </Button>
        </Space>
      }
      bordered={false}
      style={{ borderRadius: borderRadiusLG }}
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
          background: '#f8fafc',
          borderRadius: 8,
          border: '1px solid #e2e8f0',
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
            <FilterOutlined style={{ color: '#64748b' }} />
            <Select
              value={attendanceFilter}
              onChange={setAttendanceFilter}
              size="small"
              style={{ minWidth: 160 }}
              options={[
                { value: 'all', label: t('common.all') },
                { value: 'leave', label: t('attendance.colPaidLeave') },
                { value: 'absent', label: t('status.absent') },
                { value: 'full', label: t('status.fullDay') },
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
                    <div style={{ background: '#fff', borderRadius: 8, border: '1px solid #e2e8f0', padding: 16 }}>
                      {/* Tiêu đề các thứ trong tuần */}
                      <div
                        style={{
                          display: 'grid',
                          gridTemplateColumns: 'repeat(7, 1fr)',
                          textAlign: 'center',
                          fontWeight: 700,
                          color: '#64748b',
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
                              background: '#f8fafc',
                              border: '1px dashed #e2e8f0',
                              opacity: 0.35,
                              minHeight: 74,
                            }}
                          />
                        ))}

                        {monthDays.map((item) => {
                          const isSelected = item.day === selectedDay;
                          let icon = '🟢';
                          let bg = '#f0fdf4';
                          let borderColor = '#bbf7d0';

                          if (item.isWeekend) {
                            icon = '⚪';
                            bg = '#f8fafc';
                            borderColor = '#e2e8f0';
                          } else if (item.absent > 0) {
                            icon = '🔴';
                            bg = '#fef2f2';
                            borderColor = '#fecaca';
                          } else if (item.leave > 0) {
                            icon = '🟠';
                            bg = '#fffbeb';
                            borderColor = '#fde68a';
                          }

                          return (
                            <div
                              key={item.day}
                              onClick={() => setSelectedDay(item.day)}
                              style={{
                                padding: '8px 6px',
                                borderRadius: 8,
                                background: isSelected ? '#eff6ff' : bg,
                                border: isSelected ? '2px solid #1677ff' : `1px solid ${borderColor}`,
                                cursor: 'pointer',
                                textAlign: 'center',
                                transition: 'all 0.15s ease',
                                transform: isSelected ? 'scale(1.02)' : 'none',
                                boxShadow: isSelected ? '0 4px 10px rgba(22,119,255,0.18)' : 'none',
                                minHeight: 74,
                              }}
                            >
                              <div style={{ fontWeight: 700, fontSize: 13, color: isSelected ? '#1677ff' : '#0f172a' }}>
                                {item.day}
                              </div>
                              <div style={{ fontSize: 15, marginTop: 2 }}>{icon}</div>

                              {item.isWeekend ? (
                                <div style={{ fontSize: 11, color: '#94a3b8', marginTop: 2 }}>
                                  -
                                </div>
                              ) : (
                                <div style={{ fontSize: 11, color: '#0f172a', marginTop: 2, fontWeight: 500 }}>
                                  {item.present}
                                  {item.leave > 0 && (
                                    <div style={{ color: '#d97706', fontSize: 10 }}>
                                      {item.leave} P
                                    </div>
                                  )}
                                  {item.absent > 0 && (
                                    <div style={{ color: '#dc2626', fontSize: 10 }}>
                                      {item.absent} V
                                    </div>
                                  )}
                                </div>
                              )}
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  </Col>

                  {/* CHI TIẾT NGÀY ĐANG CHỌN */}
                  <Col xs={24} lg={8}>
                    <Card
                      title={
                        <div>
                          <Text strong style={{ fontSize: 16 }}>
                            {currentDayData.dayOfWeekName}, {currentDayData.dateStr}
                          </Text>
                          <div style={{ fontSize: 12, color: '#64748b' }}>
                            {t('common.totalRecords', { total: filteredList.length })}
                          </div>
                        </div>
                      }
                      size="small"
                      bordered={false}
                      style={{ background: '#fff', borderRadius: 8, border: '1px solid #e2e8f0' }}
                    >
                      <Space direction="vertical" style={{ width: '100%' }} size={12}>
                        <div>
                          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                            <Text style={{ fontSize: 13 }}>{t('dashboard.statAttendanceRate')}:</Text>
                            <Text strong style={{ color: '#1677ff' }}>
                              {filteredList.length > 0 && !currentDayData.isWeekend
                                ? Math.round((currentDayData.present / filteredList.length) * 100)
                                : currentDayData.isWeekend
                                ? 0
                                : 100}
                              %
                            </Text>
                          </div>
                          <Progress
                            percent={
                              filteredList.length > 0 && !currentDayData.isWeekend
                                ? Math.round((currentDayData.present / filteredList.length) * 100)
                                : 0
                            }
                            status={currentDayData.isWeekend ? 'normal' : 'active'}
                            strokeColor={currentDayData.isWeekend ? '#94a3b8' : '#1677ff'}
                          />
                        </div>

                        <div
                          style={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            padding: '8px 12px',
                            background: '#f0fdf4',
                            borderRadius: 6,
                            border: '1px solid #bbf7d0',
                          }}
                        >
                          <Space>
                            <CheckCircleOutlined style={{ color: '#16a34a' }} />
                            <Text>{t('status.active')}</Text>
                          </Space>
                          <Text strong style={{ color: '#16a34a', fontSize: 15 }}>
                            {currentDayData.present}
                          </Text>
                        </div>

                        <div
                          style={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            padding: '8px 12px',
                            background: '#fffbeb',
                            borderRadius: 6,
                            border: '1px solid #fde68a',
                          }}
                        >
                          <Space>
                            <CalendarOutlined style={{ color: '#d97706' }} />
                            <Text>{t('attendance.colPaidLeave')}</Text>
                          </Space>
                          <Text strong style={{ color: '#d97706', fontSize: 15 }}>
                            {currentDayData.leave}
                          </Text>
                        </div>

                        <div
                          style={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            padding: '8px 12px',
                            background: '#fef2f2',
                            borderRadius: 6,
                            border: '1px solid #fecaca',
                          }}
                        >
                          <Space>
                            <CloseCircleOutlined style={{ color: '#dc2626' }} />
                            <Text>{t('status.absent')}</Text>
                          </Space>
                          <Text strong style={{ color: '#dc2626', fontSize: 15 }}>
                            {currentDayData.absent}
                          </Text>
                        </div>

                        {/* DANH SÁCH CHI TIẾT NHÂN VIÊN */}
                        {currentDayData.exceptions.length > 0 ? (
                          <div style={{ marginTop: 6 }}>
                            <Text strong style={{ fontSize: 13 }}>
                              {t('common.info')} ({currentDayData.exceptions.length}):
                            </Text>
                            <div
                              style={{
                                maxHeight: 180,
                                overflowY: 'auto',
                                marginTop: 6,
                                display: 'flex',
                                flexDirection: 'column',
                                gap: 6,
                              }}
                            >
                              {currentDayData.exceptions.map((exc) => (
                                <div
                                  key={exc.manv}
                                  style={{
                                    display: 'flex',
                                    justifyContent: 'space-between',
                                    alignItems: 'center',
                                    padding: '6px 10px',
                                    background: '#f8fafc',
                                    borderRadius: 6,
                                    border: '1px solid #e2e8f0',
                                    fontSize: 12,
                                  }}
                                >
                                  <div>
                                    <Text strong>{exc.hoten}</Text>{' '}
                                    <Text type="secondary">(#{exc.manv})</Text>
                                    <div style={{ color: '#64748b', fontSize: 11 }}>
                                      {exc.tenpb}
                                    </div>
                                  </div>
                                  <Tag
                                    color={
                                      exc.symbol === 'P'
                                        ? 'gold'
                                        : exc.symbol === 'CT'
                                        ? 'cyan'
                                        : 'red'
                                    }
                                  >
                                    {exc.symbol}: {exc.reason}
                                  </Tag>
                                </div>
                              ))}
                            </div>
                          </div>
                        ) : (
                          <div style={{ textAlign: 'center', padding: '10px 0', color: '#94a3b8', fontSize: 12 }}>
                            {currentDayData.isWeekend ? '-' : t('status.active')}
                          </div>
                        )}

                        <Button
                          type="dashed"
                          block
                          icon={<TableOutlined />}
                          onClick={() => setActiveView('bangcong')}
                        >
                          {t('attendance.tabTimesheets')}
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
                      <li><Tag color="blue">X</Tag> : {t('status.fullDay')}</li>
                      <li><Tag color="cyan">X/2</Tag> : {t('status.halfDay')}</li>
                      <li><Tag color="gold">P</Tag> : {t('status.leavePaid')}</li>
                      <li><Tag color="red">V / RO</Tag> : {t('status.leaveUnpaid')}</li>
                      <li><Tag color="cyan">CT</Tag> : {t('common.description')}</li>
                      <li><Tag color="purple">CD</Tag> : {t('common.info')}</li>
                      <li><Tag color="red">CN</Tag> : {t('status.sunday')}</li>
                    </ul>
                  </Card>
                </Col>
              </Row>
            ),
          },
        ]}
      />
    </Card>
  );
};

export default ChamCongPage;
