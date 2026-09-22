import React, { useState } from 'react';
import {
  Card,
  Row,
  Col,
  Tag,
  Typography,
  Space,
  Button,
  Progress,
  List,
  Tooltip,
} from 'antd';
import {
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  ArrowRightOutlined,
  LockOutlined,
} from '@ant-design/icons';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip as RechartsTooltip,
} from 'recharts';
import type {
  CurrentUserDTO,
  DashboardLuongDTO,
  DashboardPhongBanDTO,
  NhanVienDTO,
  BangLuongDTO,
  ActionItemDTO,
  AnomalyItemDTO,
} from '../types/hrms';
import { canView } from '../utils/permissionUtils';
import { useAppLanguage } from '../services/i18n';

const { Title, Text, Paragraph } = Typography;

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
  onNavigate: (key: string) => void;
  presentToday?: number;
  absentToday?: number;
  lateToday?: number;
  actionItems?: ActionItemDTO[];
  anomalies?: AnomalyItemDTO[];
}

export const DashboardPage: React.FC<DashboardPageProps> = ({
  totalEmployees,
  totalSalary,
  currentUser,
  luongStats,
  nhanVienList,
  onNavigate,
  presentToday: propPresent,
  absentToday: propAbsent,
  actionItems: propActionItems,
}) => {
  const { t, lang } = useAppLanguage();
  const [, setActionCategory] = useState<string>('all');

  const canAccessNv = canView(currentUser, 'F_DM_NHANVIEN', 'NV', 'NHANVIEN');
  const canAccessCc = canView(currentUser, 'F_CC_BANGCONG', 'CHAMCONG');
  const canAccessBl = canView(currentUser, 'F_CC_BANGLUONG', 'BANGLUONG', 'LUONG');

  const countTotal = totalEmployees;
  const present = propPresent ?? 0;
  const absent = propAbsent ?? Math.max(0, countTotal - present);

  const presentPercentage = countTotal > 0 ? Math.round((present / countTotal) * 100) : 0;

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

  const currentDateStr = new Date().toLocaleDateString(lang === 'zh-CN' ? 'zh-CN' : lang, {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      {/* 1. GREETING & STATUS BANNER */}
      <div
        style={{
          background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 60%, #1e3a8a 100%)',
          borderRadius: 12,
          padding: '24px 30px',
          color: '#ffffff',
          boxShadow: '0 4px 20px rgba(15, 23, 42, 0.15)',
        }}
      >
        <Row align="middle" justify="space-between" gutter={[16, 16]}>
          <Col xs={24} md={16}>
            <Space align="center" size="middle">
              <Title level={3} style={{ color: '#fff', margin: 0 }}>
                {currentUser.FullName} 👋
              </Title>
              <Tag color="cyan" style={{ border: 'none', background: 'rgba(255,255,255,0.15)', color: '#e0f2fe' }}>
                {currentUser.IsAdmin ? t('app.tagSuperAdmin') : t('app.tagStaff')}
              </Tag>
            </Space>
            <Paragraph style={{ color: '#94a3b8', margin: '6px 0 0 0', fontSize: 14 }}>
              {t('app.titleDashboard')} &bull; {currentDateStr}
            </Paragraph>
          </Col>

          <Col xs={24} md={8} style={{ textAlign: 'right' }}>
            <Space size="middle" wrap>
              <div
                style={{
                  background: 'rgba(239, 68, 68, 0.2)',
                  border: '1px solid rgba(239, 68, 68, 0.4)',
                  padding: '6px 14px',
                  borderRadius: 20,
                  fontSize: 13,
                  fontWeight: 600,
                  color: '#fca5a5',
                  cursor: 'pointer',
                }}
                onClick={() => setActionCategory('urgent')}
              >
                🔴 {urgentCount} {t('dashboard.actionNeededTitle')}
              </div>
              <div
                style={{
                  background: 'rgba(245, 158, 11, 0.2)',
                  border: '1px solid rgba(245, 158, 11, 0.4)',
                  padding: '6px 14px',
                  borderRadius: 20,
                  fontSize: 13,
                  fontWeight: 600,
                  color: '#fcd34d',
                  cursor: 'pointer',
                }}
                onClick={() => setActionCategory('warning')}
              >
                🟡 {warningCount} {t('common.info')}
              </div>
            </Space>
          </Col>
        </Row>
      </div>

      {/* 2. 4 CORE EXECUTIVE KPI CARDS */}
      <Row gutter={[16, 16]}>
        {/* Nhân sự */}
        <Col xs={12} sm={12} lg={6}>
          <Tooltip title={!canAccessNv ? t('common.error') : t('employee.listTitle', { count: countTotal })}>
            <Card
              hoverable={canAccessNv}
              onClick={() => canAccessNv && onNavigate('nhanvien')}
              style={{
                borderRadius: 10,
                border: '1px solid #e2e8f0',
                background: '#ffffff',
                cursor: canAccessNv ? 'pointer' : 'not-allowed',
                opacity: canAccessNv ? 1 : 0.75,
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text type="secondary" strong style={{ fontSize: 13 }}>
                  👥 {t('dashboard.statTotalEmployees')}
                </Text>
                {!canAccessNv ? (
                  <LockOutlined style={{ color: '#94a3b8', fontSize: 16 }} />
                ) : (
                  <TeamOutlined style={{ color: '#3b82f6', fontSize: 18 }} />
                )}
              </div>
              <div style={{ fontSize: 28, fontWeight: 800, color: '#0f172a', margin: '8px 0 4px 0' }}>
                {countTotal}
              </div>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('dashboard.statTotalEmployeesSub')}
              </Text>
            </Card>
          </Tooltip>
        </Col>

        {/* Có mặt */}
        <Col xs={12} sm={12} lg={6}>
          <Tooltip title={!canAccessCc ? t('common.error') : t('attendance.pageTitle')}>
            <Card
              hoverable={canAccessCc}
              onClick={() => canAccessCc && onNavigate('chamcong')}
              style={{
                borderRadius: 10,
                border: '1px solid #d1fae5',
                background: '#f0fdf4',
                cursor: canAccessCc ? 'pointer' : 'not-allowed',
                opacity: canAccessCc ? 1 : 0.75,
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text strong style={{ color: '#065f46', fontSize: 13 }}>
                  🟢 {t('status.active')}
                </Text>
                {!canAccessCc ? (
                  <LockOutlined style={{ color: '#059669', fontSize: 16 }} />
                ) : (
                  <CheckCircleOutlined style={{ color: '#10b981', fontSize: 18 }} />
                )}
              </div>
              <div style={{ fontSize: 28, fontWeight: 800, color: '#047857', margin: '8px 0 4px 0' }}>
                {present}
              </div>
              <Text style={{ color: '#059669', fontSize: 12 }}>
                {t('dashboard.statPresentToday', { present })}
              </Text>
            </Card>
          </Tooltip>
        </Col>

        {/* Quỹ lương */}
        <Col xs={12} sm={12} lg={6}>
          <Tooltip title={!canAccessBl ? t('common.error') : t('payroll.pageTitle')}>
            <Card
              hoverable={canAccessBl}
              onClick={() => canAccessBl && onNavigate('bangluong')}
              style={{
                borderRadius: 10,
                border: '1px solid #e2e8f0',
                background: '#ffffff',
                cursor: canAccessBl ? 'pointer' : 'not-allowed',
                opacity: canAccessBl ? 1 : 0.75,
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text type="secondary" strong style={{ fontSize: 13 }}>
                  💵 {t('dashboard.statSalaryFund')}
                </Text>
                {!canAccessBl ? (
                  <LockOutlined style={{ color: '#94a3b8', fontSize: 16 }} />
                ) : (
                  <DollarOutlined style={{ color: '#10b981', fontSize: 18 }} />
                )}
              </div>
              <div style={{ fontSize: 26, fontWeight: 800, color: '#0f172a', margin: '8px 0 4px 0' }}>
                {formatPayroll(totalSalary)}
              </div>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('dashboard.statSalaryFundSub')}
              </Text>
            </Card>
          </Tooltip>
        </Col>

        {/* Hợp đồng */}
        <Col xs={12} sm={12} lg={6}>
          <Tooltip title={!canAccessNv ? t('common.error') : t('contract.pageTitle')}>
            <Card
              hoverable={canAccessNv}
              onClick={() => canAccessNv && onNavigate('hopdong')}
              style={{
                borderRadius: 10,
                border: '1px solid #e2e8f0',
                background: '#ffffff',
                cursor: canAccessNv ? 'pointer' : 'not-allowed',
                opacity: canAccessNv ? 1 : 0.75,
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text type="secondary" strong style={{ fontSize: 13 }}>
                  📄 {t('dashboard.statContracts')}
                </Text>
                {!canAccessNv ? (
                  <LockOutlined style={{ color: '#94a3b8', fontSize: 16 }} />
                ) : (
                  <CalendarOutlined style={{ color: '#8b5cf6', fontSize: 18 }} />
                )}
              </div>
              <div style={{ fontSize: 28, fontWeight: 800, color: '#0f172a', margin: '8px 0 4px 0' }}>
                {countTotal}
              </div>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('dashboard.statContractsSub')}
              </Text>
            </Card>
          </Tooltip>
        </Col>
      </Row>

      {/* 3. CHARTS ROW */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={16}>
          <Card
            title={t('dashboard.chartPayrollDistribution')}
            bordered={false}
            style={{ borderRadius: 10, border: '1px solid #e2e8f0' }}
          >
            <div style={{ width: '100%', height: 260 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={luongStats}>
                  <XAxis dataKey="thang" tick={{ fontSize: 12 }} />
                  <YAxis tick={{ fontSize: 12 }} />
                  <RechartsTooltip />
                  <Bar dataKey="tongLuong" fill="#3b82f6" radius={[4, 4, 0, 0]} name={t('dashboard.chartSalaryFund')} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card
            title={t('dashboard.statAttendanceRate')}
            bordered={false}
            style={{ borderRadius: 10, border: '1px solid #e2e8f0' }}
          >
            <div style={{ textAlign: 'center', padding: '10px 0' }}>
              <Progress
                type="dashboard"
                percent={presentPercentage}
                strokeColor={{ '0%': '#10b981', '100%': '#3b82f6' }}
                size={160}
              />
              <div style={{ marginTop: 16 }}>
                <Space size="middle">
                  <Tag color="green">{t('dashboard.statPresentToday', { present })}</Tag>
                  <Tag color="red">{t('dashboard.statAbsentToday', { absent })}</Tag>
                </Space>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      {/* 4. RECENT EMPLOYEES */}
      <Card
        title={t('employee.listTitle', { count: nhanVienList.length })}
        bordered={false}
        style={{ borderRadius: 10, border: '1px solid #e2e8f0' }}
        extra={
          <Button type="link" onClick={() => onNavigate('nhanvien')}>
            {t('common.view')} <ArrowRightOutlined />
          </Button>
        }
      >
        <List
          itemLayout="horizontal"
          dataSource={nhanVienList.slice(0, 5)}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta
                avatar={
                  <div
                    style={{
                      width: 36,
                      height: 36,
                      borderRadius: '50%',
                      backgroundColor: '#1677ff',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: '#fff',
                      fontWeight: 600,
                    }}
                  >
                    {item.HOTEN?.[0] || 'NV'}
                  </div>
                }
                title={<Text strong>{item.HOTEN}</Text>}
                description={`#${item.MANV} • ${item.TENPB || '-'} • ${item.TENCV || '-'}`}
              />
              <Tag color={item.DATHOIVIEC === 1 ? 'default' : 'success'}>
                {item.DATHOIVIEC === 1 ? t('status.resigned') : t('status.active')}
              </Tag>
            </List.Item>
          )}
        />
      </Card>
    </Space>
  );
};

export default DashboardPage;
