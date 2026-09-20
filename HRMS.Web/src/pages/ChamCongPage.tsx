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

const DOW_NAMES = ['Chủ nhật', 'Thứ hai', 'Thứ ba', 'Thứ tư', 'Thứ năm', 'Thứ sáu', 'Thứ bảy'];

export const ChamCongPage: React.FC<ChamCongPageProps> = ({
  kyCongList,
  selectedKyCong,
  onSelectKyCong,
  chamCongList,
  chamCongLoading,
  loaiCaList,
  onRefresh,
}) => {
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
  const month = currentKyCong?.THANG || (new Date().getMonth() + 1);
  const year = currentKyCong?.NAM || new Date().getFullYear();
  const daysInMonth = new Date(year, month, 0).getDate();

  // Tính ngày bắt đầu tháng rơi vào thứ mấy để căn chỉnh lịch Heatmap chuẩn
  // 0 = Sun, 1 = Mon, ..., 6 = Sat
  // Cột bắt đầu từ Thứ 2 (Mon) -> Chủ nhật (Sun)
  const firstDayWeek = new Date(year, month - 1, 1).getDay();
  const startColOffset = firstDayWeek === 0 ? 6 : firstDayWeek - 1;

  // Danh sách phòng ban thực tế từ CSDL
  const departmentOptions = useMemo(() => {
    const map = new Map<string, number>();
    safeChamCongList.forEach((r) => {
      const pb = r.TENPB || 'Chưa phân phòng';
      map.set(pb, (map.get(pb) || 0) + 1);
    });
    const list = Array.from(map.entries()).map(([name, count]) => ({
      value: name,
      label: `${name} (${count})`,
    }));
    return [{ value: 'all', label: `Tất cả phòng ban (${safeChamCongList.length})` }, ...list];
  }, [safeChamCongList]);

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

    // 1. Lọc theo trạng thái làm việc (Mặc định: Chỉ hiện nhân viên đang làm việc ~971 người)
    if (statusFilter === 'active') {
      result = result.filter((r) => r.IS_ACTIVE !== false && r.DATHOIVIEC !== 1);
    } else if (statusFilter === 'resigned') {
      result = result.filter((r) => r.DATHOIVIEC === 1);
    }

    // 2. Lọc theo phòng ban
    if (selectedDept !== 'all') {
      result = result.filter((r) => r.TENPB === selectedDept);
    }

    // 3. Lọc theo tìm kiếm từ khóa
    if (searchText.trim()) {
      const q = searchText.trim().toLowerCase();
      result = result.filter(
        (r) =>
          (r.HOTEN && r.HOTEN.toLowerCase().includes(q)) ||
          (r.MANV && r.MANV.toString().includes(q))
      );
    }

    // 4. Lọc theo chỉ số chuyên cần trong tháng
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

  // Thống kê từng ngày 100% từ danh sách nhân viên đã lọc theo các tiêu chí
  const monthDays: DayStat[] = useMemo(() => {
    return Array.from({ length: daysInMonth }, (_, i) => {
      const day = i + 1;
      const dateObj = new Date(year, month - 1, day);
      const dayOfWeek = dateObj.getDay(); // 0 = Chủ nhật, 6 = Thứ 7
      const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
      const dayKey = `D${day}` as keyof KyCongChiTietDTO;
      const dayOfWeekName = DOW_NAMES[dayOfWeek];

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
              reason: 'Nghỉ phép năm',
            });
          } else if (rawVal === 'V' || rawVal === 'KP' || rawVal === 'RO') {
            absentCount++;
            exceptions.push({
              manv: row.MANV,
              hoten: row.HOTEN,
              tenpb: row.TENPB,
              symbol: rawVal,
              reason: 'Vắng không phép / việc riêng',
            });
          } else if (rawVal === 'CT') {
            tripCount++;
            exceptions.push({
              manv: row.MANV,
              hoten: row.HOTEN,
              tenpb: row.TENPB,
              symbol: rawVal,
              reason: 'Đi công tác',
            });
          }
        });
      }

      let status: 'good' | 'late' | 'bad' | 'weekend' = 'good';
      if (isWeekend) {
        status = 'weekend';
      } else if (absentCount > 5) {
        status = 'bad';
      } else if (leaveCount > 10) {
        status = 'late';
      }

      const dateStr = `${day < 10 ? '0' + day : day}/${month < 10 ? '0' + month : month}/${year}`;

      return {
        day,
        dateStr,
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
  }, [filteredList, daysInMonth, month, year]);

  const currentDayData = monthDays.find((d) => d.day === selectedDay) || monthDays[0] || {
    day: 1,
    dateStr: `01/${month < 10 ? '0' + month : month}/${year}`,
    dayOfWeekName: 'Thứ hai',
    isWeekend: false,
    present: 0,
    late: 0,
    absent: 0,
    leave: 0,
    trip: 0,
    exceptions: [],
    status: 'good',
  };

  const chamCongColumns: ColumnsType<KyCongChiTietDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      fixed: 'left',
      width: 75,
      onCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      onHeaderCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      fixed: 'left',
      width: 170,
      onCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      onHeaderCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
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
      title: 'Trạng thái',
      key: 'status',
      fixed: 'left',
      width: 105,
      onCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      onHeaderCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      render: (_, r) => {
        if (r.DATHOIVIEC === 1) return <Tag color="default">Đã thôi việc</Tag>;
        return <Tag color="green">Đang làm việc</Tag>;
      },
    },
    {
      title: 'Tổng công',
      dataIndex: 'TONGNGAYCONG',
      key: 'TONGNGAYCONG',
      width: 95,
      fixed: 'left',
      onCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      onHeaderCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      render: (v: number) => (
        <Tag color="geekblue" style={{ fontWeight: 'bold' }}>
          {v ?? 0} công
        </Tag>
      ),
    },
    {
      title: 'Phép (P)',
      dataIndex: 'NGAYPHEP',
      key: 'NGAYPHEP',
      width: 80,
      onCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      onHeaderCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      render: (v: number) => (v && v > 0 ? <Tag color="gold">{v} P</Tag> : <Text type="secondary">0</Text>),
    },
    {
      title: 'Vắng (V)',
      dataIndex: 'NGHIKHONGPHEP',
      key: 'NGHIKHONGPHEP',
      width: 80,
      onCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
      onHeaderCell: () => ({
        onMouseEnter: () => {
          if (hoveredColKey !== null) setHoveredColKey(null);
        },
      }),
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
        className: isHovered ? 'matrix-col-hovered' : undefined,
        onCell: () => ({
          onMouseEnter: () => {
            if (hoveredColKey !== dayKey) setHoveredColKey(dayKey);
          },
          className: isHovered ? 'matrix-col-hovered' : undefined,
        }),
        onHeaderCell: () => ({
          onMouseEnter: () => {
            if (hoveredColKey !== dayKey) setHoveredColKey(dayKey);
          },
          className: isHovered ? 'matrix-header-col-hovered' : undefined,
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
          <span>Theo dõi Chuyên cần & Bảng Chấm công Nhân sự</span>
        </div>
      }
      extra={
        <Space wrap>
          <Text strong>Kỳ công:</Text>
          <Select
            value={selectedKyCong}
            onChange={onSelectKyCong}
            style={{ width: 140 }}
            options={kyCongList.map((kc) => ({
              value: kc.MAKYCONG,
              label: `Kỳ ${kc.THANG}/${kc.NAM}`,
            }))}
          />
          <Button icon={<ReloadOutlined />} onClick={onRefresh} loading={chamCongLoading}>
            Làm mới
          </Button>
        </Space>
      }
      bordered={false}
      style={{ borderRadius: borderRadiusLG }}
    >
      {/* THANH BỘ LỌC ĐẦY ĐỦ TIÊU CHÍ (PHÒNG BAN, TRẠNG THÁI LÀM VIỆC, TÌM KIẾM, CHUYÊN CẦN) */}
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
          {/* Lọc trạng thái nhân sự */}
          <Space>
            <UserOutlined style={{ color: colorPrimary }} />
            <Text strong>Trạng thái:</Text>
            <Radio.Group
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              buttonStyle="solid"
              size="small"
            >
              <Radio.Button value="all">
                Tất cả ({statusCounts.total})
              </Radio.Button>
              <Radio.Button value="active">
                Đang làm việc ({statusCounts.active})
              </Radio.Button>
              {statusCounts.resigned > 0 && (
                <Radio.Button value="resigned">
                  Đã thôi việc ({statusCounts.resigned})
                </Radio.Button>
              )}
            </Radio.Group>
          </Space>

          {/* Lọc phòng ban thực tế */}
          <Space>
            <ApartmentOutlined style={{ color: colorPrimary }} />
            <Text strong>Phòng ban:</Text>
            <Select
              value={selectedDept}
              onChange={setSelectedDept}
              style={{ width: 230 }}
              size="small"
              options={departmentOptions}
            />
          </Space>

          {/* Tìm kiếm họ tên, mã NV */}
          <Input
            placeholder="Tìm theo tên NV, mã NV..."
            prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            allowClear
            size="small"
            style={{ width: 200 }}
          />

          {/* Lọc tình trạng chấm công */}
          <Space>
            <FilterOutlined style={{ color: '#64748b' }} />
            <Select
              value={attendanceFilter}
              onChange={setAttendanceFilter}
              size="small"
              style={{ width: 170 }}
              options={[
                { value: 'all', label: 'Tất cả chấm công' },
                { value: 'leave', label: 'Có nghỉ phép (P)' },
                { value: 'absent', label: 'Có vắng mặt (V)' },
                { value: 'full', label: 'Đi làm đủ (≥24 công)' },
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
                <CalendarOutlined /> Lịch Chuyên cần Heatmap (Tháng {month < 10 ? '0' + month : month}/{year})
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
                        <div>Thứ 2 (Mon)</div>
                        <div>Thứ 3 (Tue)</div>
                        <div>Thứ 4 (Wed)</div>
                        <div>Thứ 5 (Thu)</div>
                        <div>Thứ 6 (Fri)</div>
                        <div style={{ color: '#fa8c16' }}>Thứ 7 (Sat)</div>
                        <div style={{ color: '#ef4444' }}>Chủ nhật (Sun)</div>
                      </div>

                      {/* Các ô ngày trong tháng (CÓ CĂN CHỈNH START OFFSET THEO THỨ THỰC TẾ) */}
                      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 8 }}>
                        {/* Các ô trống bù đầu tháng để ngày 1 rơi đúng thứ */}
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

                        {/* Danh sách ngày 1..daysInMonth */}
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
                                  Nghỉ
                                </div>
                              ) : (
                                <div style={{ fontSize: 11, color: '#0f172a', marginTop: 2, fontWeight: 500 }}>
                                  {item.present} có mặt
                                  {item.leave > 0 && (
                                    <div style={{ color: '#d97706', fontSize: 10 }}>
                                      {item.leave} phép
                                    </div>
                                  )}
                                  {item.absent > 0 && (
                                    <div style={{ color: '#dc2626', fontSize: 10 }}>
                                      {item.absent} vắng
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

                  {/* CHI TIẾT NGÀY ĐANG CHỌN (MINH BẠCH 100% AI NGHỈ / AI VẮNG) */}
                  <Col xs={24} lg={8}>
                    <Card
                      title={
                        <div>
                          <Text strong style={{ fontSize: 16 }}>
                            {currentDayData.dayOfWeekName}, Ngày {currentDayData.dateStr}
                          </Text>
                          <div style={{ fontSize: 12, color: '#64748b' }}>
                            Đang theo dõi: {filteredList.length} nhân sự
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
                            <Text style={{ fontSize: 13 }}>Tỷ lệ đi làm:</Text>
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
                            <Text>Có mặt đi làm</Text>
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
                            <Text>Nghỉ phép năm (P)</Text>
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
                            <Text>Vắng mặt (V)</Text>
                          </Space>
                          <Text strong style={{ color: '#dc2626', fontSize: 15 }}>
                            {currentDayData.absent}
                          </Text>
                        </div>

                        {currentDayData.trip > 0 && (
                          <div
                            style={{
                              display: 'flex',
                              justifyContent: 'space-between',
                              padding: '8px 12px',
                              background: '#eff6ff',
                              borderRadius: 6,
                              border: '1px solid #bfdbfe',
                            }}
                          >
                            <Space>
                              <CheckCircleOutlined style={{ color: '#2563eb' }} />
                              <Text>Công tác (CT)</Text>
                            </Space>
                            <Text strong style={{ color: '#2563eb', fontSize: 15 }}>
                              {currentDayData.trip}
                            </Text>
                          </div>
                        )}

                        {/* DANH SÁCH CHI TIẾT NHÂN VIÊN CÓ NGHỈ PHÉP / VẮNG / CÔNG TÁC TRONG NGÀY */}
                        {currentDayData.exceptions.length > 0 ? (
                          <div style={{ marginTop: 6 }}>
                            <Text strong style={{ fontSize: 13 }}>
                              Danh sách nhân sự nghỉ / công tác ({currentDayData.exceptions.length}):
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
                            {currentDayData.isWeekend
                              ? 'Ngày nghỉ cuối tuần hàng tuần'
                              : '100% nhân viên có mặt đi làm đầy đủ'}
                          </div>
                        )}

                        <Button
                          type="dashed"
                          block
                          icon={<TableOutlined />}
                          onClick={() => setActiveView('bangcong')}
                        >
                          Xem chi tiết toàn bộ bảng chấm công
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
                <TableOutlined /> Bảng chấm công chi tiết ({filteredList.length} nhân sự)
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
                scroll={{ x: 2200 }}
                pagination={{
                  pageSize: 15,
                  showSizeChanger: true,
                  pageSizeOptions: ['15', '30', '50', '100'],
                  showTotal: (total) =>
                    `Tổng cộng ${total} nhân sự (Đang lọc: ${
                      statusFilter === 'active'
                        ? 'Đang làm việc'
                        : statusFilter === 'resigned'
                        ? 'Đã thôi việc'
                        : 'Tất cả'
                    })`,
                }}
                size="small"
              />
            </div>
          ),
          },
          {
            key: 'loaica',
            label: '⚙️ Danh mục Loại ca & Ca làm việc',
            children: (
              <Row gutter={[16, 16]}>
                <Col xs={24} md={12}>
                  <Card title="Danh sách Loại ca làm việc" size="small">
                    <Table
                      scroll={{ x: 'max-content' }}
                      columns={[
                        { title: 'Mã', dataIndex: 'IDLOAICA', key: 'IDLOAICA', width: 70 },
                        { title: 'Tên ca làm', dataIndex: 'TENLOAICA', key: 'TENLOAICA' },
                        {
                          title: 'Hệ số ca',
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
                  <Card title="Ký hiệu quy ước chấm công" size="small">
                    <ul style={{ lineHeight: '28px', margin: 0, paddingLeft: 20 }}>
                      <li><Tag color="blue">X</Tag> : Đi làm cả ngày (1.0 công chuẩn)</li>
                      <li><Tag color="cyan">X/2</Tag> : Đi làm nửa ngày (0.5 công)</li>
                      <li><Tag color="gold">P</Tag> : Nghỉ phép năm có hưởng lương</li>
                      <li><Tag color="red">V / Ro</Tag> : Nghỉ việc riêng / vắng không phép</li>
                      <li><Tag color="cyan">CT</Tag> : Đi công tác bên ngoài</li>
                      <li><Tag color="purple">CD</Tag> : Chế độ (thai sản / ốm đau)</li>
                      <li><Tag color="red">CN</Tag> : Ngày nghỉ Chủ nhật hàng tuần</li>
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
