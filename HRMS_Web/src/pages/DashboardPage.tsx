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
} from 'antd';
import {
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  WarningOutlined,
  ArrowRightOutlined,
  ClockCircleOutlined,
  FireOutlined,
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
  lateToday: propLate,
  actionItems: propActionItems,
  anomalies: propAnomalies,
}) => {
  const [actionCategory, setActionCategory] = useState<string>('all');

  // 100% Real Database Calculations (No Fake Fallbacks)
  const countTotal = totalEmployees;
  const present = propPresent ?? 0;
  const absent = propAbsent ?? Math.max(0, countTotal - present);
  const late = propLate ?? 0;

  const presentPercentage = countTotal > 0 ? Math.round((present / countTotal) * 100) : 0;

  const actionList: ActionItemDTO[] = propActionItems || [];
  const anomalyList: AnomalyItemDTO[] = propAnomalies || [];

  // Format currency
  const formatPayroll = (amount: number) => {
    if (amount >= 1000000000) {
      return `${(amount / 1000000000).toFixed(2)}B đ`;
    }
    if (amount >= 1000000) {
      return `${(amount / 1000000).toFixed(0)}M đ`;
    }
    return `${amount.toLocaleString('vi-VN')} đ`;
  };

  const urgentCount = actionList.filter((a) => a.urgency === 'urgent').length;
  const warningCount = actionList.filter((a) => a.urgency === 'warning').length;
  const infoCount = actionList.filter((a) => a.urgency === 'info').length;

  const filteredActionList = actionList.filter((item) => {
    if (actionCategory === 'all') return true;
    return item.urgency === actionCategory;
  });

  const currentDateStr = new Date().toLocaleDateString('vi-VN', {
    weekday: 'long',
    day: '2-digit',
    month: '2-digit',
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
                Chào {currentUser.FullName} 👋
              </Title>
              <Tag color="cyan" style={{ border: 'none', background: 'rgba(255,255,255,0.15)', color: '#e0f2fe' }}>
                {currentUser.IsAdmin ? 'Quản trị viên Cấp cao' : 'Nhân sự HR'}
              </Tag>
            </Space>
            <Paragraph style={{ color: '#94a3b8', margin: '6px 0 0 0', fontSize: 14 }}>
              Tổng quan nhân sự hôm nay &bull; {currentDateStr}
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
                🔴 {urgentCount} việc khẩn
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
                🟡 {warningCount} cần xử lý
              </div>
            </Space>
          </Col>
        </Row>
      </div>

      {/* 2. 4 CORE EXECUTIVE KPI CARDS */}
      <Row gutter={[16, 16]}>
        {/* Nhân sự */}
        <Col xs={12} sm={12} lg={6}>
          <Card
            hoverable
            onClick={() => onNavigate('nhanvien')}
            style={{
              borderRadius: 10,
              border: '1px solid #e2e8f0',
              background: '#ffffff',
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text type="secondary" strong style={{ fontSize: 13 }}>
                👥 NHÂN SỰ
              </Text>
              <TeamOutlined style={{ color: '#3b82f6', fontSize: 18 }} />
            </div>
            <div style={{ fontSize: 28, fontWeight: 800, color: '#0f172a', margin: '8px 0 4px 0' }}>
              {countTotal}
            </div>
            <Text type="secondary" style={{ fontSize: 12 }}>
              Toàn bộ nhân sự đang quản lý
            </Text>
          </Card>
        </Col>

        {/* Có mặt */}
        <Col xs={12} sm={12} lg={6}>
          <Card
            hoverable
            onClick={() => onNavigate('chamcong')}
            style={{
              borderRadius: 10,
              border: '1px solid #d1fae5',
              background: '#f0fdf4',
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong style={{ color: '#065f46', fontSize: 13 }}>
                🟢 CÓ MẶT
              </Text>
              <CheckCircleOutlined style={{ color: '#10b981', fontSize: 18 }} />
            </div>
            <div style={{ fontSize: 28, fontWeight: 800, color: '#047857', margin: '8px 0 4px 0' }}>
              {present}
            </div>
            <Text style={{ color: '#059669', fontSize: 12 }}>
              {presentPercentage > 0 ? `Đạt ${presentPercentage}% tỷ lệ chuyên cần` : 'Chưa có lượt chấm công hôm nay'}
            </Text>
          </Card>
        </Col>

        {/* Vắng mặt */}
        <Col xs={12} sm={12} lg={6}>
          <Card
            hoverable
            onClick={() => onNavigate('chamcong')}
            style={{
              borderRadius: 10,
              border: '1px solid #fee2e2',
              background: '#fef2f2',
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong style={{ color: '#991b1b', fontSize: 13 }}>
                🔴 VẮNG MẶT
              </Text>
              <CloseCircleOutlined style={{ color: '#ef4444', fontSize: 18 }} />
            </div>
            <div style={{ fontSize: 28, fontWeight: 800, color: '#b91c1c', margin: '8px 0 4px 0' }}>
              {absent}
            </div>
            <Text style={{ color: '#dc2626', fontSize: 12 }}>
              {late > 0 ? `${late} lượt đi trễ ghi nhận` : 'Không có nhân viên đi trễ'}
            </Text>
          </Card>
        </Col>

        {/* Payroll */}
        <Col xs={12} sm={12} lg={6}>
          <Card
            hoverable
            onClick={() => onNavigate('bangluong')}
            style={{
              borderRadius: 10,
              border: '1px solid #fef3c7',
              background: '#fffbeb',
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong style={{ color: '#92400e', fontSize: 13 }}>
                💰 PAYROLL
              </Text>
              <DollarOutlined style={{ color: '#f59e0b', fontSize: 18 }} />
            </div>
            <div style={{ fontSize: 28, fontWeight: 800, color: '#b45309', margin: '8px 0 4px 0' }}>
              {formatPayroll(totalSalary)}
            </div>
            <Text style={{ color: '#d97706', fontSize: 12 }}>
              Quỹ lương kỳ hiện hành
            </Text>
          </Card>
        </Col>
      </Row>

      {/* 3. ACTION CENTER (VIỆC CẦN XỬ LÝ) */}
      <Card
        title={
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <WarningOutlined style={{ color: '#f59e0b', fontSize: 20 }} />
            <span style={{ fontSize: 16, fontWeight: 700 }}>Action Center — Việc cần xử lý ngay</span>
          </div>
        }
        extra={
          <Space>
            <Button
              size="small"
              type={actionCategory === 'all' ? 'primary' : 'default'}
              onClick={() => setActionCategory('all')}
            >
              Tất cả ({actionList.length})
            </Button>
            <Button
              size="small"
              danger
              type={actionCategory === 'urgent' ? 'primary' : 'default'}
              onClick={() => setActionCategory('urgent')}
            >
              🔴 Khẩn ({urgentCount})
            </Button>
            <Button
              size="small"
              type={actionCategory === 'warning' ? 'primary' : 'default'}
              onClick={() => setActionCategory('warning')}
            >
              🟡 Cần xử lý ({warningCount})
            </Button>
            <Button
              size="small"
              type={actionCategory === 'info' ? 'primary' : 'default'}
              onClick={() => setActionCategory('info')}
            >
              🔵 Cần xem ({infoCount})
            </Button>
          </Space>
        }
        style={{
          borderRadius: 10,
          border: '1px solid #e2e8f0',
          boxShadow: '0 2px 8px rgba(0,0,0,0.04)',
        }}
      >
        {actionList.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '24px 16px', background: '#f8fafc', borderRadius: 8 }}>
            <CheckCircleOutlined style={{ fontSize: 32, color: '#10b981', marginBottom: 8 }} />
            <div style={{ fontSize: 14, fontWeight: 600, color: '#0f172a' }}>
              Không có đầu mục cảnh báo hoặc việc khẩn cấp
            </div>
            <Text type="secondary" style={{ fontSize: 12 }}>
              Mọi quy trình hợp đồng, hồ sơ và chấm công hiện tại đều đang ở trạng thái chuẩn mực.
            </Text>
          </div>
        ) : (
          <List
            dataSource={filteredActionList}
            renderItem={(item) => {
              let dotColor = '#3b82f6';
              let badgeBg = '#eff6ff';
              if (item.urgency === 'urgent') {
                dotColor = '#ef4444';
                badgeBg = '#fef2f2';
              } else if (item.urgency === 'warning') {
                dotColor = '#f59e0b';
                badgeBg = '#fffbeb';
              }

              return (
                <div
                  key={item.id}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    padding: '12px 16px',
                    background: badgeBg,
                    borderRadius: 8,
                    marginBottom: 8,
                    border: `1px solid ${dotColor}30`,
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
                    <div
                      style={{
                        width: 32,
                        height: 32,
                        borderRadius: '50%',
                        backgroundColor: dotColor,
                        color: '#fff',
                        fontWeight: 700,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: 14,
                        flexShrink: 0,
                      }}
                    >
                      {item.count}
                    </div>
                    <div>
                      <Text strong style={{ fontSize: 14, color: '#0f172a' }}>
                        {item.title}
                      </Text>
                    </div>
                  </div>

                  <Button
                    type="primary"
                    size="small"
                    onClick={() => onNavigate(item.route)}
                    style={{
                      backgroundColor: dotColor,
                      borderColor: dotColor,
                      fontWeight: 600,
                    }}
                  >
                    {item.actionText} <ArrowRightOutlined />
                  </Button>
                </div>
              );
            }}
          />
        )}
      </Card>

      {/* 4. ANOMALY DETECTION (PHÁT HIỆN BẤT THƯỜNG THÔNG MINH) */}
      <Card
        title={
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <FireOutlined style={{ color: '#ef4444', fontSize: 20 }} />
            <span style={{ fontSize: 16, fontWeight: 700 }}>Hệ thống cảnh báo Bất thường (Anomaly Detection)</span>
          </div>
        }
        style={{
          borderRadius: 10,
          border: '1px solid #fed7aa',
          background: '#fffaf5',
        }}
      >
        {anomalyList.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '24px 16px', background: '#f0fdf4', borderRadius: 8, border: '1px solid #bbf7d0' }}>
            <CheckCircleOutlined style={{ fontSize: 32, color: '#10b981', marginBottom: 8 }} />
            <div style={{ fontSize: 14, fontWeight: 700, color: '#166534' }}>
              Hệ thống vận hành an toàn — Không phát hiện chỉ số bất thường
            </div>
            <Text style={{ fontSize: 12, color: '#15803d' }}>
              Dữ liệu làm thêm giờ (OT) và tạm ứng lương của toàn bộ nhân viên đều nằm trong ngưỡng kiểm soát an toàn.
            </Text>
          </div>
        ) : (
          <Row gutter={[16, 16]}>
            {anomalyList.map((anom) => (
              <Col xs={24} md={12} key={anom.id}>
                <div
                  style={{
                    background: '#ffffff',
                    borderRadius: 8,
                    padding: '14px 16px',
                    border: '1px solid #fed7aa',
                    boxShadow: '0 1px 3px rgba(0,0,0,0.03)',
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'space-between',
                  }}
                >
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                      <Text strong style={{ fontSize: 14, color: '#0f172a' }}>
                        {anom.employeeName}
                      </Text>
                      <Tag color={anom.severity === 'high' ? 'error' : 'warning'}>
                        {anom.severity === 'high' ? 'Nguy cơ cao' : 'Cần lưu ý'}
                      </Tag>
                    </div>
                    <div style={{ fontSize: 12, color: '#64748b', marginBottom: 6 }}>
                      🏢 {anom.department} &bull; <strong>{anom.metric}</strong>
                    </div>
                    <Paragraph style={{ fontSize: 13, color: '#334155', margin: 0 }}>
                      {anom.description}
                    </Paragraph>
                  </div>

                  <div style={{ marginTop: 12, textAlign: 'right' }}>
                    <Button
                      size="small"
                      type="link"
                      style={{ padding: 0, fontWeight: 600 }}
                      onClick={() => {
                        if (anom.metric.includes('OT') || anom.metric.includes('Chấm công') || anom.metric.includes('vắng')) {
                          onNavigate('chamcong');
                        } else {
                          onNavigate('bangluong');
                        }
                      }}
                    >
                      Kiểm tra dữ liệu <ArrowRightOutlined />
                    </Button>
                  </div>
                </div>
              </Col>
            ))}
          </Row>
        )}
      </Card>

      {/* 5. 2-COLUMN: BIẾN ĐỘNG NHÂN SỰ & HÔM NAY */}
      <Row gutter={[16, 16]}>
        {/* Left: Biến động Quỹ lương */}
        <Col xs={24} lg={15}>
          <Card
            title="👥 Biến động Quỹ lương các kỳ gần nhất"
            bordered={false}
            style={{ borderRadius: 10, border: '1px solid #e2e8f0' }}
          >
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={luongStats} margin={{ top: 15, right: 20, left: 10, bottom: 5 }}>
                <XAxis dataKey="KyCong" tick={{ fontSize: 12 }} />
                <YAxis tickFormatter={(val) => `${(val / 1000000000).toFixed(1)}B`} tick={{ fontSize: 12 }} />
                <RechartsTooltip formatter={(val: unknown) => [`${Number(val ?? 0).toLocaleString('vi-VN')} đ`, 'Quỹ lương']} />
                <Bar dataKey="TongLuong" fill="#3b82f6" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </Col>

        {/* Right: 📅 Hôm nay */}
        <Col xs={24} lg={9}>
          <Card
            title={
              <Space>
                <CalendarOutlined style={{ color: '#3b82f6' }} />
                <span>📅 Hôm nay</span>
              </Space>
            }
            bordered={false}
            style={{ borderRadius: 10, border: '1px solid #e2e8f0' }}
          >
            <div style={{ marginBottom: 16 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <Text strong>Tỷ lệ nhân sự có mặt</Text>
                <Text strong style={{ color: '#10b981' }}>{present}/{countTotal} ({presentPercentage}%)</Text>
              </div>
              <Progress percent={presentPercentage} strokeColor="#10b981" showInfo={false} />
            </div>

            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: '10px 14px',
                  background: '#f0fdf4',
                  borderRadius: 6,
                  border: '1px solid #bbf7d0',
                }}
              >
                <Space>
                  <CheckCircleOutlined style={{ color: '#16a34a' }} />
                  <Text>Có mặt làm việc</Text>
                </Space>
                <Text strong style={{ color: '#16a34a' }}>{present} nhân viên</Text>
              </div>

              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: '10px 14px',
                  background: '#fef2f2',
                  borderRadius: 6,
                  border: '1px solid #fecaca',
                }}
              >
                <Space>
                  <CloseCircleOutlined style={{ color: '#dc2626' }} />
                  <Text>Vắng mặt</Text>
                </Space>
                <Text strong style={{ color: '#dc2626' }}>{absent} nhân viên</Text>
              </div>

              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: '10px 14px',
                  background: '#fffbeb',
                  borderRadius: 6,
                  border: '1px solid #fde68a',
                }}
              >
                <Space>
                  <ClockCircleOutlined style={{ color: '#d97706' }} />
                  <Text>Đi muộn / Về sớm</Text>
                </Space>
                <Text strong style={{ color: '#d97706' }}>{late} trường hợp</Text>
              </div>
            </Space>

            <Button
              type="dashed"
              block
              style={{ marginTop: 16 }}
              onClick={() => onNavigate('chamcong')}
            >
              Mở bảng chấm công chi tiết
            </Button>
          </Card>
        </Col>
      </Row>

      {/* 6. HOẠT ĐỘNG GẦN ĐÂY */}
      <Card
        title="🕒 Hoạt động gần đây trong hệ thống"
        bordered={false}
        style={{ borderRadius: 10, border: '1px solid #e2e8f0' }}
        extra={<Button type="link" onClick={() => onNavigate('nhanvien')}>Xem hồ sơ nhân sự <ArrowRightOutlined /></Button>}
      >
        <List
          itemLayout="horizontal"
          dataSource={nhanVienList.slice(0, 4)}
          renderItem={(item) => (
            <List.Item
              actions={[
                <Button
                  key="view"
                  size="small"
                  type="link"
                  onClick={() => onNavigate('nhanvien')}
                >
                  Chi tiết
                </Button>,
              ]}
            >
              <List.Item.Meta
                avatar={
                  <div
                    style={{
                      width: 38,
                      height: 38,
                      borderRadius: '50%',
                      backgroundColor: '#3b82f6',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: '#fff',
                      fontWeight: 600,
                    }}
                  >
                    {item.HOTEN?.[0] || 'N'}
                  </div>
                }
                title={<Text strong>{item.HOTEN}</Text>}
                description={
                  <span>
                    Mã NV #{item.MANV} &bull; {item.TENPB || 'Phòng Kỹ Thuật'} &bull; {item.TENCV || 'Nhân viên'} &bull; Đang hoạt động
                  </span>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    </Space>
  );
};

export default DashboardPage;
