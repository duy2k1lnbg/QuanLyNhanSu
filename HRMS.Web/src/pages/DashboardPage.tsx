import React, { useState, useMemo } from 'react';
import {
  Row,
  Col,
  Tag,
  Typography,
  Space,
  Button,
  Progress,
  List,
} from 'antd';
import {
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  ArrowRightOutlined,
  WarningOutlined,
  ClockCircleOutlined,
} from '@ant-design/icons';
import {
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
} from 'recharts';
import type {
  CurrentUserDTO,
  DashboardLuongDTO,
  DashboardPhongBanDTO,
  NhanVienDTO,
  BangLuongDTO,
  HopDongDTO,
  ActionItemDTO,
  AnomalyItemDTO,
} from '../types/hrms';
import { canView } from '../utils/permissionUtils';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import { PageHeader } from '../theme/components/PageHeader';
import { MetricCard } from '../theme/components/MetricCard';
import { SectionCard } from '../theme/components/SectionCard';
import { calculateDashboardMetrics } from '../utils/dashboardMetrics';

const { Text } = Typography;

interface DashboardPageProps {
  totalEmployees: number;
  hopDongCount: number;
  totalSalary: number;
  currentUser: CurrentUserDTO;
  isBackendConnected: boolean;
  luongStats: DashboardLuongDTO[];
  phongBanStats: DashboardPhongBanDTO[];
  nhanVienList: NhanVienDTO[];
  bangLuongList: BangLuongDTO[];
  hopDongList?: HopDongDTO[];
  onNavigate: (key: string) => void;
  presentToday?: number;
  absentToday?: number;
  lateToday?: number;
  actionItems?: ActionItemDTO[];
  anomalies?: AnomalyItemDTO[];
  selectedKyCongName?: string;
}

export const DashboardPage: React.FC<DashboardPageProps> = ({
  totalEmployees,
  totalSalary,
  currentUser,
  luongStats,
  nhanVienList,
  bangLuongList,
  hopDongList,
  onNavigate,
  presentToday: propPresent,
  actionItems: propActionItems,
  selectedKyCongName,
}) => {
  const { t, lang } = useAppLanguage();
  const { tokens, isDark } = useAppTheme();
  const [actionCategory, setActionCategory] = useState<'all' | 'urgent' | 'warning'>('all');

  const canAccessNv = canView(currentUser, 'F_DM_NHANVIEN', 'NV', 'NHANVIEN');
  const canAccessCc = canView(currentUser, 'F_CC_BANGCONG', 'CHAMCONG');
  const canAccessBl = canView(currentUser, 'F_CC_BANGLUONG', 'BANGLUONG', 'LUONG');

  const metrics = useMemo(() => {
    return calculateDashboardMetrics(
      nhanVienList,
      hopDongList || [],
      bangLuongList,
      propPresent,
      selectedKyCongName
    );
  }, [nhanVienList, hopDongList, bangLuongList, propPresent, selectedKyCongName]);

  const countTotal = metrics.totalEmployees || totalEmployees;
  const activeCount = metrics.activeEmployees;
  const present = metrics.distinctPresentToday;
  const absent = Math.max(0, activeCount - present);

  const presentPercentage = activeCount > 0 ? Math.min(100, Math.round((present / activeCount) * 100)) : 0;
  const actionList: ActionItemDTO[] = propActionItems || [];

  const formatPayroll = (amount: number) => {
    if (amount >= 1000000000) {
      return `${(amount / 1000000000).toFixed(2)}B đ`;
    }
    if (amount >= 1000000) {
      return `${(amount / 1000000).toFixed(1)}M đ`;
    }
    return `${amount.toLocaleString('vi-VN')} đ`;
  };

  const urgentCount = actionList.filter((a) => a.urgency === 'urgent').length;
  const warningCount = actionList.filter((a) => a.urgency === 'warning').length;

  const filteredActionList = actionList.filter((item) => {
    if (actionCategory === 'urgent') return item.urgency === 'urgent';
    if (actionCategory === 'warning') return item.urgency === 'warning';
    return true;
  });

  const currentDateStr = new Date().toLocaleDateString(lang === 'zh-CN' ? 'zh-CN' : lang, {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20, width: '100%' }}>
      {/* 1. EXECUTIVE PAGE HEADER */}
      <PageHeader
        title={`Xin chào, ${currentUser.FullName || currentUser.Username} 👋`}
        subtitle={`${t('app.titleDashboard')} • ${currentDateStr}`}
        badge={
          <Tag
            color={currentUser.IsAdmin ? 'red' : 'purple'}
            style={{ borderRadius: 6, fontWeight: 600, padding: '2px 8px' }}
          >
            {currentUser.IsAdmin ? t('app.tagSuperAdmin') : t('app.tagStaff')}
          </Tag>
        }
        extra={
          <Space size="middle" wrap>
            {urgentCount > 0 && (
              <Button
                size="middle"
                danger
                onClick={() => setActionCategory('urgent')}
                style={{ borderRadius: 8, fontWeight: 600 }}
              >
                🔴 {urgentCount} {t('dashboard.actionNeededTitle')}
              </Button>
            )}
            {warningCount > 0 && (
              <Button
                size="middle"
                onClick={() => setActionCategory('warning')}
                style={{
                  borderRadius: 8,
                  fontWeight: 600,
                  borderColor: tokens.warningText,
                  color: tokens.warningText,
                }}
              >
                🟡 {warningCount} {t('common.info')}
              </Button>
            )}
            {canAccessNv && (
              <Button
                type="primary"
                onClick={() => onNavigate('nhanvien')}
                style={{
                  borderRadius: 8,
                  background: tokens.btnPrimaryBg,
                  borderColor: tokens.btnPrimaryBg,
                  fontWeight: 600,
                }}
              >
                {t('employee.listTitle', { count: countTotal })}
              </Button>
            )}
          </Space>
        }
      />

      {/* 2. 4 EXECUTIVE KPI CARDS */}
      <Row gutter={[16, 16]}>
        {/* Total Employees */}
        <Col xs={12} sm={12} lg={6}>
          <MetricCard
            title={t('dashboard.statTotalEmployees')}
            value={countTotal}
            icon={<TeamOutlined />}
            accent="blue"
            subtext={`${activeCount} đang làm việc • ${metrics.resignedEmployees} đã thôi việc`}
            onClick={canAccessNv ? () => onNavigate('nhanvien') : undefined}
            className="stagger-card-1"
          />
        </Col>

        {/* Present Today / Active Headcount */}
        <Col xs={12} sm={12} lg={6}>
          <MetricCard
            title={t('status.active')}
            value={activeCount}
            icon={<CheckCircleOutlined />}
            accent="green"
            subtext={`Có mặt hôm nay: ${present} (${metrics.totalPunchesToday} lượt quẹt)`}
            onClick={canAccessCc ? () => onNavigate('chamcong') : (canAccessNv ? () => onNavigate('nhanvien') : undefined)}
            className="stagger-card-2"
          />
        </Col>

        {/* Salary Fund (Net Pay) */}
        <Col xs={12} sm={12} lg={6}>
          <MetricCard
            title={`Tổng thực lĩnh (${metrics.payrollPeriodLabel})`}
            value={formatPayroll(totalSalary || metrics.totalNetPayroll)}
            icon={<DollarOutlined />}
            accent="purple"
            subtext={metrics.payrollCalculatedCount > 0 ? `${metrics.payrollCalculatedCount}/${countTotal} nhân sự đã tính lương` : 'Chi trả thực tế kỳ công'}
            onClick={canAccessBl ? () => onNavigate('bangluong') : undefined}
            className="stagger-card-3"
          />
        </Col>

        {/* Active Contracts */}
        <Col xs={12} sm={12} lg={6}>
          <MetricCard
            title={t('dashboard.statContracts')}
            value={metrics.activeContracts}
            icon={<CalendarOutlined />}
            accent="teal"
            subtext={metrics.totalContracts > 0 ? `${metrics.activeContracts}/${metrics.totalContracts} hợp đồng còn hạn` : 'Hợp đồng lao động hiệu lực'}
            onClick={canAccessNv ? () => onNavigate('hopdong') : undefined}
            className="stagger-card-4"
          />
        </Col>
      </Row>

      {/* 3. CHARTS ROW */}
      <Row gutter={[16, 16]}>
        {/* Main Payroll Trend Chart */}
        <Col xs={24} lg={16}>
          <SectionCard
            title={t('dashboard.chartPayrollDistribution')}
            subtitle="Biểu đồ quỹ lương thực tế phân bổ qua các kỳ công"
            icon={<DollarOutlined style={{ color: tokens.primary }} />}
          >
            <div style={{ width: '100%', height: 280, paddingTop: 10 }}>
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={luongStats} margin={{ top: 10, right: 20, left: 0, bottom: 0 }}>
                  <defs>
                    <linearGradient id="payrollAreaGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor={tokens.primary} stopOpacity={isDark ? 0.45 : 0.25} />
                      <stop offset="95%" stopColor={tokens.primary} stopOpacity={0.0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke={tokens.chartGrid} vertical={false} />
                  <XAxis
                    dataKey="thang"
                    tick={{ fontSize: 12, fill: tokens.textSecondary }}
                    stroke={tokens.borderSubtle}
                  />
                  <YAxis
                    tick={{ fontSize: 12, fill: tokens.textSecondary }}
                    stroke={tokens.borderSubtle}
                    tickFormatter={(val) => {
                      if (val >= 1000000000) return `${(val / 1000000000).toFixed(1)}B`;
                      if (val >= 1000000) return `${(val / 1000000).toFixed(0)}M`;
                      return val;
                    }}
                  />
                  <RechartsTooltip
                    contentStyle={{
                      backgroundColor: tokens.elevatedBg,
                      borderColor: tokens.borderSubtle,
                      borderRadius: 10,
                      boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
                      color: tokens.textPrimary,
                    }}
                    itemStyle={{ color: tokens.primary, fontWeight: 600 }}
                    formatter={(val: any) => [`${Number(val).toLocaleString('vi-VN')} đ`, t('dashboard.chartSalaryFund')]}
                  />
                  <Area
                    type="monotone"
                    dataKey="tongLuong"
                    stroke={tokens.primary}
                    strokeWidth={2.5}
                    fillOpacity={1}
                    fill="url(#payrollAreaGradient)"
                    name={t('dashboard.chartSalaryFund')}
                  />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </SectionCard>
        </Col>

        {/* Secondary Attendance Rate Radial Chart */}
        <Col xs={24} lg={8}>
          <SectionCard
            title={t('dashboard.statAttendanceRate')}
            subtitle="Tỷ lệ nhân sự đi làm hôm nay"
            icon={<CheckCircleOutlined style={{ color: tokens.successText }} />}
          >
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '16px 0' }}>
              <Progress
                type="dashboard"
                percent={presentPercentage}
                strokeColor={{
                  '0%': tokens.primary,
                  '100%': tokens.successText,
                }}
                trailColor={isDark ? '#1E2638' : '#F1F3F9'}
                size={175}
              />
              <div style={{ marginTop: 20 }}>
                <Space size="middle">
                  <Tag
                    color="green"
                    style={{
                      borderRadius: 6,
                      padding: '4px 10px',
                      fontSize: 12,
                      fontWeight: 600,
                    }}
                  >
                    🟢 {t('dashboard.statPresentToday', { present })}
                  </Tag>
                  <Tag
                    color="red"
                    style={{
                      borderRadius: 6,
                      padding: '4px 10px',
                      fontSize: 12,
                      fontWeight: 600,
                    }}
                  >
                    🔴 {t('dashboard.statAbsentToday', { absent })}
                  </Tag>
                </Space>
              </div>
            </div>
          </SectionCard>
        </Col>
      </Row>

      {/* 4. RECENT EMPLOYEES & ACTION ITEMS ROW */}
      <Row gutter={[16, 16]}>
        {/* Action Items Panel */}
        <Col xs={24} lg={12}>
          <SectionCard
            title="Cần xử lý & Cảnh báo"
            subtitle={`${actionList.length} việc cần lưu ý`}
            icon={<WarningOutlined style={{ color: tokens.warningText }} />}
            extra={
              <Space size={6}>
                <Button
                  size="small"
                  type={actionCategory === 'all' ? 'primary' : 'default'}
                  onClick={() => setActionCategory('all')}
                  style={{ borderRadius: 6, fontSize: 12 }}
                >
                  Tất cả ({actionList.length})
                </Button>
                {urgentCount > 0 && (
                  <Button
                    size="small"
                    type={actionCategory === 'urgent' ? 'primary' : 'default'}
                    danger={actionCategory === 'urgent'}
                    onClick={() => setActionCategory('urgent')}
                    style={{ borderRadius: 6, fontSize: 12 }}
                  >
                    Gấp ({urgentCount})
                  </Button>
                )}
              </Space>
            }
          >
            {filteredActionList.length === 0 ? (
              <div style={{ padding: '36px 16px', textAlign: 'center', color: tokens.textMuted }}>
                <CheckCircleOutlined style={{ fontSize: 32, color: tokens.successText, marginBottom: 8 }} />
                <div>Hệ thống ổn định. Không có việc cần xử lý gấp.</div>
              </div>
            ) : (
              <List
                itemLayout="horizontal"
                dataSource={filteredActionList.slice(0, 5)}
                renderItem={(item) => (
                  <List.Item
                    style={{ padding: '12px 0', borderBottom: `1px solid ${tokens.borderSubtle}` }}
                    actions={[
                      item.route ? (
                        <Button
                          key="nav"
                          type="link"
                          size="small"
                          onClick={() => onNavigate(item.route)}
                          style={{ color: tokens.primary, fontWeight: 600, padding: 0 }}
                        >
                          {item.actionText || 'Xử lý'} <ArrowRightOutlined />
                        </Button>
                      ) : null,
                    ]}
                  >
                    <List.Item.Meta
                      avatar={
                        item.urgency === 'urgent' ? (
                          <div
                            style={{
                              width: 32,
                              height: 32,
                              borderRadius: 8,
                              background: tokens.dangerBg,
                              color: tokens.dangerText,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                            }}
                          >
                            <WarningOutlined />
                          </div>
                        ) : (
                          <div
                            style={{
                              width: 32,
                              height: 32,
                              borderRadius: 8,
                              background: tokens.warningBg,
                              color: tokens.warningText,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                            }}
                          >
                            <ClockCircleOutlined />
                          </div>
                        )
                      }
                      title={
                        <Text strong style={{ fontSize: 13, color: tokens.textPrimary }}>
                          {item.title}
                        </Text>
                      }
                      description={
                        <Text style={{ fontSize: 12, color: tokens.textSecondary }}>
                          {item.count ? `${item.count} bản ghi cần kiểm tra` : item.actionText || '-'}
                        </Text>
                      }
                    />
                  </List.Item>
                )}
              />
            )}
          </SectionCard>
        </Col>

        {/* Recent Employees List */}
        <Col xs={24} lg={12}>
          <SectionCard
            title={t('employee.listTitle', { count: nhanVienList.length })}
            subtitle="Danh sách nhân sự mới cập nhật trong hệ thống"
            icon={<TeamOutlined style={{ color: tokens.primary }} />}
            extra={
              canAccessNv && (
                <Button
                  type="link"
                  onClick={() => onNavigate('nhanvien')}
                  style={{ color: tokens.primary, fontWeight: 600, padding: 0 }}
                >
                  {t('common.view')} <ArrowRightOutlined />
                </Button>
              )
            }
          >
            <List
              itemLayout="horizontal"
              dataSource={nhanVienList.slice(0, 5)}
              renderItem={(item) => (
                <List.Item style={{ padding: '10px 0', borderBottom: `1px solid ${tokens.borderSubtle}` }}>
                  <List.Item.Meta
                    avatar={
                      <div
                        style={{
                          width: 36,
                          height: 36,
                          borderRadius: '50%',
                          background: 'linear-gradient(135deg, #6D4AFF 0%, #4B24DE 100%)',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          color: '#fff',
                          fontWeight: 700,
                          fontSize: 14,
                        }}
                      >
                        {item.HOTEN?.[0] || 'NV'}
                      </div>
                    }
                    title={
                      <Text
                        strong
                        style={{
                          color: tokens.textPrimary,
                          cursor: canAccessNv ? 'pointer' : 'default',
                        }}
                        onClick={() => canAccessNv && onNavigate('nhanvien')}
                      >
                        {item.HOTEN}
                      </Text>
                    }
                    description={
                      <span style={{ color: tokens.textSecondary, fontSize: 12 }}>
                        #{item.MANV} &bull; {item.TENPB || '-'} &bull; {item.TENCV || '-'}
                      </span>
                    }
                  />
                  <Tag
                    color={item.DATHOIVIEC === 1 ? 'default' : 'success'}
                    style={{ borderRadius: 6, fontWeight: 500 }}
                  >
                    {item.DATHOIVIEC === 1 ? t('status.resigned') : t('status.active')}
                  </Tag>
                </List.Item>
              )}
            />
          </SectionCard>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;
