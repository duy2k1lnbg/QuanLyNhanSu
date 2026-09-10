import React, { useState } from 'react';
import {
  Modal,
  Form,
  Input,
  Button,
  Typography,
  Tag,
  Alert,
  message,
  Row,
  Col,
  Space,
  Card,
  Badge,
} from 'antd';
import {
  UserOutlined,
  LockOutlined,
  LoginOutlined,
  SafetyCertificateOutlined,
  WindowsOutlined,
  CloudDownloadOutlined,
  RocketOutlined,
  DatabaseOutlined,
  MobileOutlined,
  InfoCircleOutlined,
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  RobotOutlined,
  TrophyOutlined,
  CodeOutlined,
  ArrowRightOutlined,
  MailOutlined,
  FacebookOutlined,
  GithubOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import type { CurrentUserDTO } from '../types/hrms';

const { Title, Text, Paragraph } = Typography;

// =============================================================================
// THÔNG TIN TÁC GIẢ & CÁC KÊNH LIÊN HỆ (BẠN CÓ THỂ TỰ THAY ĐỔI TRỰC TIẾP TẠI ĐÂY)
// =============================================================================
const AUTHOR_INFO = {
  name: 'Nguyễn Thọ Duy',
  title: 'Software Engineer',
  badge: 'Tác Giả Dự Án', // Gợi ý: 'Tác Giả Dự Án', 'Sẵn Sàng Hợp Tác', 'Project Creator', 'Open for Work'
  bio: 'Lập trình viên phát triển phần mềm quản trị nhân sự và hệ thống doanh nghiệp với C# .NET, React và CSDL quan hệ.',
  email: 'duythptln2001@gmail.com',
  facebook: 'https://www.facebook.com/duy.nguyentho.7/',
  github: 'https://github.com/duy2k1lnbg/QuanLyNhanSu',
  avatarUrl: '/myavt.png',
};

interface LoginProps {
  onLoginSuccess: (user: CurrentUserDTO, token: string) => void;
}

export const Login: React.FC<LoginProps> = ({ onLoginSuccess }) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // State điều khiển Modal Đăng Nhập
  const [loginModalVisible, setLoginModalVisible] = useState<boolean>(false);

  // State điều khiển Modal Tải Ứng Dụng
  const [downloadModalVisible, setDownloadModalVisible] = useState<boolean>(false);
  const [downloadType, setDownloadType] = useState<'windows' | 'mobile' | 'general'>('windows');

  const WINDOWS_PACKAGE_URL = '/downloads/HRMS_Setup_v3.5.0.zip';

  const handleDownloadWindows = () => {
    message.loading({ content: 'Đang bắt đầu tải xuống HRMS_Setup_v3.5.0.zip...', key: 'dl_win', duration: 2 });
    const link = document.createElement('a');
    link.href = WINDOWS_PACKAGE_URL;
    link.setAttribute('download', 'HRMS_Setup_v3.5.0.zip');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleOpenDownload = (type: 'windows' | 'mobile' | 'general' = 'general') => {
    setDownloadType(type);
    setDownloadModalVisible(true);
  };

  const handleOpenLogin = () => {
    setErrorMessage(null);
    setLoginModalVisible(true);
  };

  const handleLogin = async (values: { username: string; password: string }) => {
    setLoading(true);
    setErrorMessage(null);
    try {
      const res = await api.post('/auth/login', {
        Username: values.username.trim(),
        Password: values.password,
      });

      const token = res.data?.token || res.data?.Token;
      const rawUser = res.data?.user || res.data?.User;

      if (token && rawUser) {
        const normalizedUser: CurrentUserDTO = {
          IdUser: rawUser.IdUser ?? rawUser.id ?? 0,
          Username: rawUser.Username || rawUser.username || values.username.trim(),
          FullName: rawUser.FullName || rawUser.fullName || rawUser.Username || values.username.trim(),
          IsAdmin: Boolean(rawUser.IsAdmin ?? rawUser.isAdmin),
          Rights: rawUser.Rights || rawUser.rights || [],
        };

        localStorage.setItem('hrms_token', token);
        localStorage.setItem('hrms_user', JSON.stringify(normalizedUser));
        message.success(`Chào mừng ${normalizedUser.FullName} đăng nhập thành công!`);
        setLoginModalVisible(false);
        onLoginSuccess(normalizedUser, token);
      } else {
        setErrorMessage(
          res.data?.message || res.data?.Message || 'Tên đăng nhập hoặc mật khẩu không chính xác.'
        );
      }
    } catch (err: unknown) {
      const errorObj = err as {
        response?: { data?: { Message?: string; message?: string } | string };
        message?: string;
      };
      let msg = 'Không thể kết nối đến máy chủ đăng nhập.';
      if (typeof errorObj.response?.data === 'string') {
        if (
          errorObj.response.data.includes('<html') ||
          errorObj.response.data.includes('Exception') ||
          errorObj.response.data.includes('ORA-')
        ) {
          msg = 'Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin hoặc thử lại sau.';
        } else {
          msg = errorObj.response.data;
        }
      } else if (
        typeof errorObj.response?.data === 'object' &&
        errorObj.response?.data !== null &&
        ('Message' in errorObj.response.data || 'message' in errorObj.response.data)
      ) {
        msg = errorObj.response.data.Message || errorObj.response.data.message || msg;
      } else if (errorObj.message) {
        msg = 'Lỗi kết nối máy chủ. Vui lòng thử lại sau.';
      }
      setErrorMessage(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: '100vh',
        width: '100%',
        display: 'flex',
        flexDirection: 'column',
        background: 'radial-gradient(ellipse at top, #0f172a 0%, #020617 100%)',
        position: 'relative',
        overflowX: 'hidden',
        color: '#f8fafc',
        fontFamily: "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
      }}
    >
      {/* Background ambient glowing circles */}
      <div
        style={{
          position: 'absolute',
          width: 700,
          height: 700,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(37,99,235,0.18) 0%, rgba(0,0,0,0) 70%)',
          top: '-15%',
          left: '10%',
          pointerEvents: 'none',
        }}
      />
      <div
        style={{
          position: 'absolute',
          width: 600,
          height: 600,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(168,85,247,0.16) 0%, rgba(0,0,0,0) 70%)',
          top: '30%',
          right: '5%',
          pointerEvents: 'none',
        }}
      />

      {/* TOP NAVIGATION BAR */}
      <header
        style={{
          height: 72,
          padding: '0 clamp(16px, 4vw, 40px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          borderBottom: '1px solid rgba(255, 255, 255, 0.08)',
          background: 'rgba(15, 23, 42, 0.8)',
          backdropFilter: 'blur(16px)',
          position: 'sticky',
          top: 0,
          zIndex: 100,
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <div
            style={{
              width: 42,
              height: 42,
              borderRadius: 12,
              background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 4px 14px rgba(37,99,235,0.45)',
            }}
          >
            <SafetyCertificateOutlined style={{ fontSize: 24, color: '#fff' }} />
          </div>
          <div>
            <div style={{ fontWeight: 800, fontSize: 18, letterSpacing: '0.5px', color: '#fff', lineHeight: 1.2 }}>
              HRMS ENTERPRISE
            </div>
            <div style={{ fontSize: 11, color: '#94a3b8' }}>
              Giải Pháp Quản Trị Nhân Sự & Phân Quyền Doanh Nghiệp
            </div>
          </div>
        </div>

        <Space size="middle">
          <Tag color="success" style={{ padding: '3px 10px', borderRadius: 12, border: 'none', background: 'rgba(34,197,94,0.15)', color: '#4ade80' }}>
            <Badge status="processing" color="#4ade80" /> Hệ Thống Sẵn Sàng
          </Tag>

          <Button
            type="text"
            icon={<CloudDownloadOutlined style={{ color: '#60a5fa' }} />}
            onClick={() => handleOpenDownload('general')}
            style={{ color: '#e2e8f0', fontWeight: 600 }}
          >
            Tải Ứng Dụng
          </Button>

          {/* NÚT ĐĂNG NHẬP NỔI BẬT */}
          <Button
            type="primary"
            icon={<LoginOutlined />}
            onClick={handleOpenLogin}
            style={{
              height: 40,
              padding: '0 20px',
              borderRadius: 10,
              fontWeight: 700,
              fontSize: 14,
              background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
              border: 'none',
              boxShadow: '0 4px 14px rgba(37,99,235,0.4)',
            }}
          >
            Đăng Nhập
          </Button>
        </Space>
      </header>

      {/* MAIN CONTENT BODY */}
      <main style={{ flex: 1, maxWidth: 1280, width: '100%', margin: '0 auto', padding: 'clamp(30px, 5vw, 60px) 24px' }}>
        {/* HERO BANNER */}
        <div style={{ textAlign: 'center', maxWidth: 900, margin: '0 auto 60px' }}>
          <div
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 8,
              padding: '6px 16px',
              borderRadius: 20,
              background: 'rgba(59,130,246,0.12)',
              border: '1px solid rgba(59,130,246,0.3)',
              marginBottom: 20,
            }}
          >
            <RocketOutlined style={{ color: '#60a5fa' }} />
            <span style={{ fontSize: 13, fontWeight: 600, color: '#93c5fd', letterSpacing: '0.5px' }}>
              NỀN TẢNG QUẢN TRỊ NHÂN LỰC THẾ HỆ MỚI • TIÊU CHUẨN DOANH NGHIỆP
            </span>
          </div>

          <Title
            level={1}
            style={{
              color: '#fff',
              fontSize: 'clamp(32px, 5vw, 54px)',
              fontWeight: 800,
              lineHeight: 1.18,
              letterSpacing: '-1px',
              marginBottom: 20,
            }}
          >
            Hệ Thống Quản Trị Nhân Sự, Chấm Công & Tiền Lương{' '}
            <span
              style={{
                background: 'linear-gradient(135deg, #60a5fa 0%, #c084fc 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
              }}
            >
              Toàn Diện
            </span>
          </Title>

          <Paragraph style={{ color: '#94a3b8', fontSize: 'clamp(15px, 2vw, 17px)', lineHeight: 1.7, marginBottom: 36, maxWidth: 780, margin: '0 auto 36px' }}>
            Hệ sinh thái phần mềm kết nối dữ liệu trực tiếp và xuyên suốt giữa ứng dụng <b>Windows Desktop (WinForms)</b> cho khối văn phòng,
            cổng <b>Web Quản Trị Trực Tuyến</b> và <b>Ứng Dụng Di Động</b> dành cho toàn thể cán bộ công nhân viên.
          </Paragraph>

          <Space size="middle" wrap style={{ justifyContent: 'center' }}>
            <Button
              type="primary"
              size="large"
              icon={<LoginOutlined />}
              onClick={handleOpenLogin}
              style={{
                height: 50,
                padding: '0 32px',
                borderRadius: 12,
                fontWeight: 700,
                fontSize: 16,
                background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                border: 'none',
                boxShadow: '0 8px 24px rgba(37,99,235,0.45)',
              }}
            >
              Đăng Nhập Quản Trị
            </Button>

            <Button
              size="large"
              icon={<CloudDownloadOutlined />}
              onClick={() => handleOpenDownload('general')}
              style={{
                height: 50,
                padding: '0 28px',
                borderRadius: 12,
                fontWeight: 600,
                fontSize: 15,
                background: 'rgba(255, 255, 255, 0.08)',
                color: '#fff',
                borderColor: 'rgba(255, 255, 255, 0.2)',
                backdropFilter: 'blur(8px)',
              }}
            >
              Tải Bộ Cài Đặt Ứng Dụng
            </Button>
          </Space>
        </div>

        {/* 4 LIVE SYSTEM CAPABILITIES */}
        <Row gutter={[16, 16]} style={{ marginBottom: 60 }}>
          <Col xs={12} sm={6}>
            <div
              style={{
                background: 'rgba(30, 41, 59, 0.6)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 16,
                padding: '20px',
                textAlign: 'center',
                backdropFilter: 'blur(10px)',
              }}
            >
              <div style={{ fontSize: 'clamp(24px, 3vw, 36px)', fontWeight: 800, color: '#60a5fa', lineHeight: 1.2 }}>
                99.9%
              </div>
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>Độ sẵn sàng dịch vụ</div>
            </div>
          </Col>

          <Col xs={12} sm={6}>
            <div
              style={{
                background: 'rgba(30, 41, 59, 0.6)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 16,
                padding: '20px',
                textAlign: 'center',
                backdropFilter: 'blur(10px)',
              }}
            >
              <div style={{ fontSize: 'clamp(24px, 3vw, 36px)', fontWeight: 800, color: '#4ade80', lineHeight: 1.2 }}>
                100%
              </div>
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>Chuẩn hóa quy trình HR</div>
            </div>
          </Col>

          <Col xs={12} sm={6}>
            <div
              style={{
                background: 'rgba(30, 41, 59, 0.6)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 16,
                padding: '20px',
                textAlign: 'center',
                backdropFilter: 'blur(10px)',
              }}
            >
              <div style={{ fontSize: 'clamp(24px, 3vw, 36px)', fontWeight: 800, color: '#fb923c', lineHeight: 1.2 }}>
                Real-time
              </div>
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>Chấm công & tính lương</div>
            </div>
          </Col>

          <Col xs={12} sm={6}>
            <div
              style={{
                background: 'rgba(30, 41, 59, 0.6)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 16,
                padding: '20px',
                textAlign: 'center',
                backdropFilter: 'blur(10px)',
              }}
            >
              <div style={{ fontSize: 'clamp(24px, 3vw, 36px)', fontWeight: 800, color: '#c084fc', lineHeight: 1.2 }}>
                256-bit
              </div>
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>Mã hóa bảo mật đa tầng</div>
            </div>
          </Col>
        </Row>

        {/* SECTION 2: HỆ SINH THÁI TẢI VỀ (DOWNLOAD MULTI-PLATFORM) */}
        <div style={{ marginBottom: 70 }}>
          <div style={{ textAlign: 'center', marginBottom: 36 }}>
            <div style={{ fontSize: 13, fontWeight: 700, color: '#60a5fa', textTransform: 'uppercase', letterSpacing: '1px', marginBottom: 8 }}>
              HỆ SINH THÁI ĐA NỀN TẢNG
            </div>
            <Title level={2} style={{ color: '#fff', margin: 0, fontWeight: 800 }}>
              Sẵn Sàng Cho Mọi Thiết Bị Của Doanh Nghiệp
            </Title>
            <Paragraph style={{ color: '#94a3b8', marginTop: 8, fontSize: 15 }}>
              Lựa chọn phương thức làm việc linh hoạt, tối ưu năng suất cho từng phòng ban.
            </Paragraph>
          </div>

          <Row gutter={[24, 24]}>
            {/* 1. BẢN WINDOWS DESKTOP */}
            <Col xs={24} md={8}>
              <Card
                hoverable
                style={{
                  height: '100%',
                  background: 'rgba(30, 41, 59, 0.7)',
                  borderColor: 'rgba(59, 130, 246, 0.35)',
                  borderRadius: 18,
                  backdropFilter: 'blur(12px)',
                  display: 'flex',
                  flexDirection: 'column',
                }}
                bodyStyle={{ padding: '28px 24px', display: 'flex', flexDirection: 'column', height: '100%' }}
              >
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 18 }}>
                  <div
                    style={{
                      width: 52,
                      height: 52,
                      borderRadius: 14,
                      background: 'rgba(37,99,235,0.2)',
                      color: '#60a5fa',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: 28,
                    }}
                  >
                    <WindowsOutlined />
                  </div>
                  <Tag color="success" style={{ borderRadius: 6, fontWeight: 600, padding: '2px 8px' }}>
                    v3.5.0 Sẵn sàng
                  </Tag>
                </div>

                <Title level={4} style={{ color: '#fff', marginBottom: 8 }}>
                  Bản Windows Desktop
                </Title>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 14, fontWeight: 500 }}>
                  C# .NET • Windows Desktop Enterprise
                </div>
                <Paragraph style={{ color: '#cbd5e1', fontSize: 13.5, lineHeight: 1.6, flex: 1 }}>
                  Ứng dụng máy tính chuyên dụng cho Ban Giám đốc, Kế toán trưởng và Phòng Nhân sự. Hỗ trợ import/export Excel hàng loạt,
                  in phiếu lương và phân quyền chức năng chi tiết.
                </Paragraph>

                <Button
                  type="primary"
                  icon={<CloudDownloadOutlined />}
                  block
                  onClick={handleDownloadWindows}
                  style={{
                    height: 42,
                    borderRadius: 10,
                    fontWeight: 700,
                    background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                    border: 'none',
                    marginTop: 16,
                  }}
                >
                  Tải Bộ Cài Đặt (v3.5.0 .zip)
                </Button>
              </Card>
            </Col>

            {/* 2. CỔNG THÔNG TIN WEB */}
            <Col xs={24} md={8}>
              <Card
                hoverable
                style={{
                  height: '100%',
                  background: 'rgba(30, 41, 59, 0.7)',
                  borderColor: 'rgba(34, 197, 94, 0.35)',
                  borderRadius: 18,
                  backdropFilter: 'blur(12px)',
                  display: 'flex',
                  flexDirection: 'column',
                }}
                bodyStyle={{ padding: '28px 24px', display: 'flex', flexDirection: 'column', height: '100%' }}
              >
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 18 }}>
                  <div
                    style={{
                      width: 52,
                      height: 52,
                      borderRadius: 14,
                      background: 'rgba(34,197,94,0.2)',
                      color: '#4ade80',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: 28,
                    }}
                  >
                    <DatabaseOutlined />
                  </div>
                  <Tag color="processing" style={{ borderRadius: 6, fontWeight: 600, padding: '2px 8px' }}>
                    Trực Tuyến (Live)
                  </Tag>
                </div>

                <Title level={4} style={{ color: '#fff', marginBottom: 8 }}>
                  Cổng Web Trực Tuyến
                </Title>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 14, fontWeight: 500 }}>
                  React 19 • HTTPS SSL 256-bit
                </div>
                <Paragraph style={{ color: '#cbd5e1', fontSize: 13.5, lineHeight: 1.6, flex: 1 }}>
                  Truy cập từ bất kỳ trình duyệt nào trên máy tính hoặc điện thoại. Xem biểu đồ trực quan, tính lương, quản lý hồ sơ và
                  trò chuyện cùng <b>Trợ lý ảo AI Copilot</b>.
                </Paragraph>

                <Button
                  type="primary"
                  icon={<ArrowRightOutlined />}
                  block
                  onClick={handleOpenLogin}
                  style={{
                    height: 42,
                    borderRadius: 10,
                    fontWeight: 700,
                    background: 'linear-gradient(135deg, #16a34a 0%, #15803d 100%)',
                    border: 'none',
                    marginTop: 16,
                  }}
                >
                  Mở Cổng Quản Trị
                </Button>
              </Card>
            </Col>

            {/* 3. BẢN MOBILE APP */}
            <Col xs={24} md={8}>
              <Card
                hoverable
                style={{
                  height: '100%',
                  background: 'rgba(30, 41, 59, 0.7)',
                  borderColor: 'rgba(168, 85, 247, 0.35)',
                  borderRadius: 18,
                  backdropFilter: 'blur(12px)',
                  display: 'flex',
                  flexDirection: 'column',
                }}
                bodyStyle={{ padding: '28px 24px', display: 'flex', flexDirection: 'column', height: '100%' }}
              >
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 18 }}>
                  <div
                    style={{
                      width: 52,
                      height: 52,
                      borderRadius: 14,
                      background: 'rgba(168,85,247,0.2)',
                      color: '#c084fc',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: 28,
                    }}
                  >
                    <MobileOutlined />
                  </div>
                  <Tag color="purple" style={{ borderRadius: 6, fontWeight: 600, padding: '2px 8px' }}>
                    Sắp ra mắt
                  </Tag>
                </div>

                <Title level={4} style={{ color: '#fff', marginBottom: 8 }}>
                  HRMS Mobile App
                </Title>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 14, fontWeight: 500 }}>
                  iOS & Android (Cross-Platform)
                </div>
                <Paragraph style={{ color: '#cbd5e1', fontSize: 13.5, lineHeight: 1.6, flex: 1 }}>
                  Chấm công bằng định vị vệ tinh GPS, nhận diện khuôn mặt AI, gửi đơn xin nghỉ phép tức thì và nhận thông báo phiếu lương
                  trực tiếp về điện thoại nhân viên.
                </Paragraph>

                <Button
                  ghost
                  icon={<CloudDownloadOutlined />}
                  block
                  onClick={() => handleOpenDownload('mobile')}
                  style={{
                    height: 42,
                    borderRadius: 10,
                    fontWeight: 700,
                    color: '#c084fc',
                    borderColor: '#a855f7',
                    marginTop: 16,
                  }}
                >
                  App Store / CH Play
                </Button>
              </Card>
            </Col>
          </Row>
        </div>

        {/* SECTION 3: TÍNH NĂNG NỔI BẬT */}
        <div style={{ marginBottom: 70 }}>
          <div style={{ textAlign: 'center', marginBottom: 36 }}>
            <div style={{ fontSize: 13, fontWeight: 700, color: '#60a5fa', textTransform: 'uppercase', letterSpacing: '1px', marginBottom: 8 }}>
              TÍNH NĂNG VƯỢT TRỘI
            </div>
            <Title level={2} style={{ color: '#fff', margin: 0, fontWeight: 800 }}>
              Chuẩn Hóa Mọi Nghiệp Vụ Nhân Sự
            </Title>
          </div>

          <Row gutter={[20, 20]}>
            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <TeamOutlined style={{ fontSize: 30, color: '#60a5fa', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>Quản Lý Hồ Sơ 360°</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  Quản lý đầy đủ sơ yếu lý lịch, hợp đồng lao động, bằng cấp, điều chuyển phòng ban, nâng lương và khen thưởng kỷ luật.
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <CalendarOutlined style={{ fontSize: 30, color: '#4ade80', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>Chấm Công Linh Hoạt</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  Hỗ trợ phân ca linh hoạt, chấm công chi tiết 31 ngày trong kỳ, ghi nhận tăng ca (OT) và tạm ứng lương tức thời.
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <DollarOutlined style={{ fontSize: 30, color: '#fb923c', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>Tính Lương Tự Động</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  Động cơ tính lương tự động từ dữ liệu chấm công, khấu trừ BHXH, tạm ứng và hỗ trợ in phiếu thanh toán lương chuẩn.
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <SafetyCertificateOutlined style={{ fontSize: 30, color: '#c084fc', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>Phân Quyền Ma Trận RBAC</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  Đồng bộ chuẩn 100% theo kiến trúc kiểm soát quyền hạn theo vai trò. Hỗ trợ tạo nhóm quyền và khóa tài khoản an toàn.
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <RobotOutlined style={{ fontSize: 30, color: '#38bdf8', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>Trợ Lý AI Copilot</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  Tích hợp AI Copilot thông minh, hỗ trợ tra cứu luật lao động, chính sách nhân sự và tổng hợp số liệu bằng ngôn ngữ tự nhiên.
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <TrophyOutlined style={{ fontSize: 30, color: '#eab308', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>Khen Thưởng & Kỷ Luật</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  Theo dõi lịch sử quyết định nâng lương, ban hành khen thưởng thành tích và xử lý vi phạm kỷ luật chính xác, minh bạch.
                </div>
              </div>
            </Col>
          </Row>
        </div>

        {/* SECTION 4: THÔNG TIN KIẾN TRÚC & NHÀ PHÁT TRIỂN */}
        <div
          style={{
            background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.7) 0%, rgba(15, 23, 42, 0.8) 100%)',
            border: '1px solid rgba(255, 255, 255, 0.08)',
            borderRadius: 20,
            padding: 'clamp(24px, 4vw, 40px)',
            marginBottom: 40,
            backdropFilter: 'blur(12px)',
          }}
        >
          <Row gutter={[32, 24]} align="middle">
            <Col xs={24} lg={16}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10 }}>
                <CodeOutlined style={{ color: '#60a5fa', fontSize: 20 }} />
                <span style={{ fontSize: 13, fontWeight: 700, color: '#93c5fd', textTransform: 'uppercase' }}>
                  KIẾN TRÚC KỸ THUẬT & NHÀ PHÁT TRIỂN
                </span>
              </div>
              <Title level={3} style={{ color: '#fff', margin: '0 0 14px 0' }}>
                Hệ Thống Được Phát Triển Theo Tiêu Chuẩn Doanh Nghiệp
              </Title>
              <Paragraph style={{ color: '#94a3b8', fontSize: 14.5, lineHeight: 1.7, marginBottom: 18 }}>
                Dự án được xây dựng dựa trên kiến trúc phân tầng (Multi-tier Enterprise Architecture), đảm bảo tính toàn vẹn dữ liệu,
                bảo mật tuyệt đối thông tin nhân sự và sẵn sàng mở rộng quy mô cho hàng nghìn nhân sự.
              </Paragraph>

              <Space wrap size={[8, 8]}>
                <Tag color="blue">Enterprise Database</Tag>
                <Tag color="cyan">C# .NET Enterprise</Tag>
                <Tag color="geekblue">ASP.NET Web API</Tag>
                <Tag color="purple">Entity Framework</Tag>
                <Tag color="magenta">React & TypeScript</Tag>
                <Tag color="green">BCrypt & JWT Authentication</Tag>
                <Tag color="gold">Cloud High Availability</Tag>
              </Space>
            </Col>

            <Col xs={24} lg={8} style={{ textAlign: 'center' }}>
              <div
                style={{
                  background: 'rgba(15, 23, 42, 0.6)',
                  border: '1px solid rgba(255, 255, 255, 0.1)',
                  borderRadius: 16,
                  padding: '24px',
                }}
              >
                <div style={{ fontSize: 15, fontWeight: 700, color: '#fff', marginBottom: 8 }}>
                  Sẵn sàng trải nghiệm?
                </div>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 20 }}>
                  Đăng nhập để vào ngay Cổng Quản Trị Hệ Thống.
                </div>
                <Button
                  type="primary"
                  size="large"
                  block
                  icon={<LoginOutlined />}
                  onClick={handleOpenLogin}
                  style={{
                    height: 44,
                    borderRadius: 10,
                    fontWeight: 700,
                    background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                    border: 'none',
                  }}
                >
                  Đăng Nhập Ngay
                </Button>
              </div>
            </Col>
          </Row>
        </div>

        {/* ========================================================================= */}
        {/* SECTION 5: DEVELOPER PROFILE & MARKETING / CONTACT                        */}
        {/* ========================================================================= */}
        <div
          style={{
            background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.75) 0%, rgba(15, 23, 42, 0.9) 100%)',
            border: '1px solid rgba(59, 130, 246, 0.25)',
            borderRadius: 22,
            padding: 'clamp(28px, 4vw, 44px)',
            marginBottom: 40,
            backdropFilter: 'blur(16px)',
            boxShadow: '0 20px 40px rgba(0, 0, 0, 0.35)',
            position: 'relative',
            overflow: 'hidden',
          }}
        >
          {/* Subtle glowing ambient behind card */}
          <div
            style={{
              position: 'absolute',
              width: 320,
              height: 320,
              borderRadius: '50%',
              background: 'radial-gradient(circle, rgba(59,130,246,0.18) 0%, rgba(0,0,0,0) 70%)',
              top: '-50px',
              right: '-50px',
              pointerEvents: 'none',
            }}
          />

          <Row gutter={[36, 32]} align="middle">
            {/* Cột ảnh đại diện + Thông tin tác giả */}
            <Col xs={24} md={9} lg={8} style={{ textAlign: 'center' }}>
              <div style={{ position: 'relative', display: 'inline-block', marginBottom: 18 }}>
                <div
                  style={{
                    width: 140,
                    height: 140,
                    borderRadius: '50%',
                    padding: 4,
                    background: 'linear-gradient(135deg, #2563eb 0%, #a855f7 50%, #06b6d4 100%)',
                    boxShadow: '0 8px 28px rgba(37, 99, 235, 0.45)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    margin: '0 auto',
                  }}
                >
                  <img
                    src={AUTHOR_INFO.avatarUrl}
                    alt={AUTHOR_INFO.name}
                    style={{
                      width: '100%',
                      height: '100%',
                      borderRadius: '50%',
                      objectFit: 'cover',
                      display: 'block',
                    }}
                  />
                </div>
                <div
                  style={{
                    position: 'absolute',
                    bottom: 4,
                    left: '50%',
                    transform: 'translateX(-50%)',
                    background: '#16a34a',
                    color: '#fff',
                    borderRadius: 20,
                    padding: '3px 12px',
                    fontSize: 11,
                    fontWeight: 700,
                    boxShadow: '0 2px 8px rgba(22, 163, 74, 0.5)',
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: 5,
                    whiteSpace: 'nowrap',
                  }}
                >
                  <span style={{ width: 6, height: 6, borderRadius: '50%', background: '#fff', display: 'inline-block' }} />
                  {AUTHOR_INFO.badge}
                </div>
              </div>

              <Title level={4} style={{ color: '#fff', margin: '6px 0 4px 0', fontWeight: 800 }}>
                {AUTHOR_INFO.name}
              </Title>
              <div style={{ color: '#60a5fa', fontSize: 13.5, fontWeight: 600, marginBottom: 10 }}>
                {AUTHOR_INFO.title}
              </div>
              <div style={{ color: '#94a3b8', fontSize: 13, lineHeight: 1.6, maxWidth: 300, margin: '0 auto 16px' }}>
                {AUTHOR_INFO.bio}
              </div>

              {/* Tag kỹ năng nổi bật */}
              <Space wrap size={[6, 6]} style={{ justifyContent: 'center' }}>
                <Tag color="blue" style={{ borderRadius: 6, fontSize: 11, padding: '2px 8px' }}>C# .NET</Tag>
                <Tag color="cyan" style={{ borderRadius: 6, fontSize: 11, padding: '2px 8px' }}>React & TypeScript</Tag>
                <Tag color="geekblue" style={{ borderRadius: 6, fontSize: 11, padding: '2px 8px' }}>Database</Tag>
                <Tag color="purple" style={{ borderRadius: 6, fontSize: 11, padding: '2px 8px' }}>AI Integration</Tag>
              </Space>
            </Col>

            {/* Cột thông điệp marketing + Nút liên hệ Email & Facebook */}
            <Col xs={24} md={15} lg={16}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 10 }}>
                <RocketOutlined style={{ color: '#a855f7', fontSize: 18 }} />
                <span style={{ fontSize: 12.5, fontWeight: 700, color: '#c084fc', textTransform: 'uppercase', letterSpacing: '1px' }}>
                  KẾT NỐI & TRAO ĐỔI DỰ ÁN
                </span>
              </div>

              <Title level={3} style={{ color: '#fff', margin: '0 0 14px 0', fontWeight: 800 }}>
                Liên Hệ Hợp Tác & Trao Đổi Kỹ Thuật
              </Title>

              <Paragraph style={{ color: '#cbd5e1', fontSize: 14, lineHeight: 1.7, marginBottom: 24 }}>
                Dự án HRMS được tôi nghiên cứu và hoàn thiện với đầy đủ các phân hệ quản lý nhân sự, chấm công, tính lương và cổng Web/Desktop. 
                Nếu bạn có nhu cầu trao đổi kỹ thuật, tham khảo mã nguồn hoặc có dự án cần cộng tác phát triển, rất vui lòng được kết nối qua các kênh dưới đây:
              </Paragraph>

              {/* 3 Nút liên hệ nổi bật: Email, Facebook & GitHub */}
              <Row gutter={[14, 14]}>
                {/* 1. EMAIL */}
                <Col xs={24} sm={8}>
                  <a
                    href={`mailto:${AUTHOR_INFO.email}`}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 12,
                      padding: '14px 16px',
                      borderRadius: 14,
                      background: 'rgba(37, 99, 235, 0.14)',
                      border: '1px solid rgba(59, 130, 246, 0.35)',
                      color: '#fff',
                      transition: 'all 0.3s ease',
                      textDecoration: 'none',
                      height: '100%',
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.background = 'rgba(37, 99, 235, 0.28)';
                      e.currentTarget.style.borderColor = '#60a5fa';
                      e.currentTarget.style.transform = 'translateY(-2px)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.background = 'rgba(37, 99, 235, 0.14)';
                      e.currentTarget.style.borderColor = 'rgba(59, 130, 246, 0.35)';
                      e.currentTarget.style.transform = 'translateY(0)';
                    }}
                  >
                    <div
                      style={{
                        width: 42,
                        height: 42,
                        borderRadius: 10,
                        background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: 20,
                        color: '#fff',
                        flexShrink: 0,
                        boxShadow: '0 4px 12px rgba(37,99,235,0.4)',
                      }}
                    >
                      <MailOutlined />
                    </div>
                    <div style={{ overflow: 'hidden' }}>
                      <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                        Email Trực Tiếp
                      </div>
                      <div style={{ fontSize: 13, fontWeight: 700, color: '#93c5fd', marginTop: 2, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        {AUTHOR_INFO.email}
                      </div>
                    </div>
                  </a>
                </Col>

                {/* 2. FACEBOOK */}
                <Col xs={24} sm={8}>
                  <a
                    href={AUTHOR_INFO.facebook}
                    target="_blank"
                    rel="noreferrer"
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 12,
                      padding: '14px 16px',
                      borderRadius: 14,
                      background: 'rgba(24, 119, 242, 0.14)',
                      border: '1px solid rgba(24, 119, 242, 0.4)',
                      color: '#fff',
                      transition: 'all 0.3s ease',
                      textDecoration: 'none',
                      height: '100%',
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.background = 'rgba(24, 119, 242, 0.28)';
                      e.currentTarget.style.borderColor = '#1877f2';
                      e.currentTarget.style.transform = 'translateY(-2px)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.background = 'rgba(24, 119, 242, 0.14)';
                      e.currentTarget.style.borderColor = 'rgba(24, 119, 242, 0.4)';
                      e.currentTarget.style.transform = 'translateY(0)';
                    }}
                  >
                    <div
                      style={{
                        width: 42,
                        height: 42,
                        borderRadius: 10,
                        background: 'linear-gradient(135deg, #1877f2 0%, #0d5cb6 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: 20,
                        color: '#fff',
                        flexShrink: 0,
                        boxShadow: '0 4px 12px rgba(24,119,242,0.4)',
                      }}
                    >
                      <FacebookOutlined />
                    </div>
                    <div style={{ overflow: 'hidden' }}>
                      <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                        Facebook Profile
                      </div>
                      <div style={{ fontSize: 13, fontWeight: 700, color: '#60a5fa', marginTop: 2, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        fb.com/duy.nguyentho.7
                      </div>
                    </div>
                  </a>
                </Col>

                {/* 3. GITHUB */}
                <Col xs={24} sm={8}>
                  <a
                    href={AUTHOR_INFO.github}
                    target="_blank"
                    rel="noreferrer"
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 12,
                      padding: '14px 16px',
                      borderRadius: 14,
                      background: 'rgba(255, 255, 255, 0.08)',
                      border: '1px solid rgba(255, 255, 255, 0.2)',
                      color: '#fff',
                      transition: 'all 0.3s ease',
                      textDecoration: 'none',
                      height: '100%',
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.background = 'rgba(255, 255, 255, 0.16)';
                      e.currentTarget.style.borderColor = '#cbd5e1';
                      e.currentTarget.style.transform = 'translateY(-2px)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.background = 'rgba(255, 255, 255, 0.08)';
                      e.currentTarget.style.borderColor = 'rgba(255, 255, 255, 0.2)';
                      e.currentTarget.style.transform = 'translateY(0)';
                    }}
                  >
                    <div
                      style={{
                        width: 42,
                        height: 42,
                        borderRadius: 10,
                        background: 'linear-gradient(135deg, #334155 0%, #1e293b 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: 20,
                        color: '#fff',
                        flexShrink: 0,
                        boxShadow: '0 4px 12px rgba(0,0,0,0.3)',
                      }}
                    >
                      <GithubOutlined />
                    </div>
                    <div style={{ overflow: 'hidden' }}>
                      <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                        GitHub Mã Nguồn
                      </div>
                      <div style={{ fontSize: 13, fontWeight: 700, color: '#e2e8f0', marginTop: 2, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        github.com/duy2k1lnbg
                      </div>
                    </div>
                  </a>
                </Col>
              </Row>
            </Col>
          </Row>
        </div>
      </main>

      {/* FOOTER */}
      <footer
        style={{
          padding: '24px',
          textAlign: 'center',
          fontSize: 13,
          color: '#64748b',
          borderTop: '1px solid rgba(255, 255, 255, 0.08)',
          background: 'rgba(15, 23, 42, 0.95)',
        }}
      >
        <div>
          © 2026 HRMS ENTERPRISE SOLUTION • HỆ THỐNG QUẢN TRỊ NHÂN SỰ DOANH NGHIỆP
        </div>
        <div style={{ fontSize: 11.5, color: '#475569', marginTop: 4 }}>
          Cổng Thông Tin Doanh Nghiệp Hoạt Động Trên Nền Tảng Đám Mây An Toàn • SSL 256-bit Encrypted
        </div>
      </footer>

      {/* ========================================================================= */}
      {/* MODAL ĐĂNG NHẬP HỆ THỐNG (CHỈ XUẤT HIỆN KHI NGƯỜI DÙNG BẤM "ĐĂNG NHẬP") */}
      {/* ========================================================================= */}
      <Modal
        open={loginModalVisible}
        onCancel={() => setLoginModalVisible(false)}
        footer={null}
        width={440}
        centered
        destroyOnClose
        bodyStyle={{ padding: '32px 28px' }}
        style={{ borderRadius: 18, overflow: 'hidden' }}
      >
        {/* Header Modal */}
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <div
            style={{
              width: 54,
              height: 54,
              margin: '0 auto 12px',
              borderRadius: 14,
              background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 8px 18px rgba(37,99,235,0.35)',
            }}
          >
            <SafetyCertificateOutlined style={{ fontSize: 28, color: '#fff' }} />
          </div>
          <Title level={3} style={{ margin: 0, fontWeight: 800, color: '#0f172a' }}>
            ĐĂNG NHẬP HỆ THỐNG
          </Title>
          <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 0, fontSize: 13 }}>
            Cổng Quản Trị Nhân Sự Trực Tuyến
          </Paragraph>
          <Tag color="processing" style={{ marginTop: 8, borderRadius: 10, fontWeight: 500 }}>
            <SafetyCertificateOutlined style={{ marginRight: 4 }} /> Cổng Xác Thực Bảo Mật SSL
          </Tag>
        </div>

        {errorMessage && (
          <Alert
            message={errorMessage}
            type="error"
            showIcon
            closable
            onClose={() => setErrorMessage(null)}
            style={{ marginBottom: 18, borderRadius: 8 }}
          />
        )}

        <Form form={form} layout="vertical" onFinish={handleLogin}>
          <Form.Item
            name="username"
            label={<Text strong style={{ color: '#334155' }}>Tên đăng nhập</Text>}
            rules={[{ required: true, message: 'Vui lòng nhập tên tài khoản!' }]}
          >
            <Input
              size="large"
              prefix={<UserOutlined style={{ color: '#94a3b8' }} />}
              placeholder="Nhập tên đăng nhập..."
              autoFocus
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item
            name="password"
            label={<Text strong style={{ color: '#334155' }}>Mật khẩu</Text>}
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
          >
            <Input.Password
              size="large"
              prefix={<LockOutlined style={{ color: '#94a3b8' }} />}
              placeholder="Nhập mật khẩu..."
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item style={{ marginTop: 24, marginBottom: 0 }}>
            <Button
              type="primary"
              htmlType="submit"
              size="large"
              block
              loading={loading}
              icon={<LoginOutlined />}
              style={{
                height: 46,
                borderRadius: 10,
                fontWeight: 700,
                fontSize: 15,
                background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                border: 'none',
                boxShadow: '0 6px 16px rgba(37,99,235,0.4)',
              }}
            >
              Đăng nhập hệ thống
            </Button>
          </Form.Item>
        </Form>

        <div
          style={{
            marginTop: 20,
            paddingTop: 14,
            borderTop: '1px solid #f1f5f9',
            textAlign: 'center',
            fontSize: 12,
            color: '#64748b',
          }}
        >
          🔒 Bảo mật mã hóa SSL 256-bit • Tiêu chuẩn RFC 7519 JWT
        </div>
      </Modal>

      {/* ========================================================================= */}
      {/* MODAL THÔNG BÁO TẢI ỨNG DỤNG ĐANG PHÁT TRIỂN & ĐÓNG GÓI                 */}
      {/* ========================================================================= */}
      <Modal
        title={
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <RocketOutlined style={{ color: '#2563eb', fontSize: 20 }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              Thông Báo Bộ Cài Đặt Ứng Dụng
            </span>
          </div>
        }
        open={downloadModalVisible}
        onOk={() => setDownloadModalVisible(false)}
        onCancel={() => setDownloadModalVisible(false)}
        okText="Đã hiểu"
        cancelButtonProps={{ style: { display: 'none' } }}
        width={560}
        centered
      >
        <div style={{ padding: '12px 4px' }}>
          <Alert
            message={
              downloadType === 'windows'
                ? 'Bộ Cài Đặt Windows Desktop v3.5.0 Đã Sẵn Sàng'
                : downloadType === 'mobile'
                ? 'Ứng dụng di động đang trong quá trình phát triển'
                : 'Hệ Sinh Thái Ứng Dụng Doanh Nghiệp HRMS'
            }
            description={
              downloadType === 'windows'
                ? 'Gói cài đặt HRMS_Setup_v3.5.0.zip đã sẵn sàng. Bạn có thể nhấn tải trực tiếp về máy tính làm việc.'
                : downloadType === 'mobile'
                ? 'Phiên bản ứng dụng di động cho iOS & Android đang trong lộ trình phát triển và sẽ sớm có mặt trên App Store & Google Play.'
                : 'Bản cài đặt Windows v3.5.0 đã sẵn sàng tải về. Ứng dụng di động đang trong lộ trình phát triển.'
            }
            type={downloadType === 'windows' ? 'success' : 'info'}
            showIcon
            icon={downloadType === 'windows' ? <SafetyCertificateOutlined style={{ color: '#16a34a' }} /> : <InfoCircleOutlined style={{ color: '#2563eb' }} />}
            style={{ marginBottom: 20, borderRadius: 10, background: downloadType === 'windows' ? '#f0fdf4' : '#eff6ff', borderColor: downloadType === 'windows' ? '#bbf7d0' : '#bfdbfe' }}
          />

          <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            <div
              style={{
                display: 'flex',
                gap: 14,
                padding: '16px',
                borderRadius: 12,
                background: '#f8fafc',
                border: '1px solid #e2e8f0',
                alignItems: 'flex-start',
              }}
            >
              <WindowsOutlined style={{ fontSize: 32, color: '#2563eb', marginTop: 2 }} />
              <div style={{ flex: 1 }}>
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8, flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 700, fontSize: 15, color: '#1e293b' }}>
                    🖥️ Ứng dụng Windows Desktop (.NET Enterprise)
                  </span>
                  <Tag color="success" style={{ fontWeight: 600 }}>v3.5.0 Sẵn sàng</Tag>
                </div>
                <div style={{ fontSize: 13, color: '#475569', marginTop: 6, lineHeight: 1.5 }}>
                  Gói cài đặt chính thức <b>HRMS_Setup_v3.5.0.zip</b> đã sẵn sàng. Tương thích Windows 10, Windows 11 và Windows Server.
                </div>
                <div style={{ marginTop: 12 }}>
                  <Button
                    type="primary"
                    icon={<CloudDownloadOutlined />}
                    onClick={handleDownloadWindows}
                    style={{
                      background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                      border: 'none',
                      borderRadius: 8,
                      fontWeight: 600,
                    }}
                  >
                    Tải Về Ngay: HRMS_Setup_v3.5.0.zip
                  </Button>
                </div>
              </div>
            </div>

            <div
              style={{
                display: 'flex',
                gap: 14,
                padding: '14px 16px',
                borderRadius: 10,
                background: '#f8fafc',
                border: '1px solid #e2e8f0',
              }}
            >
              <MobileOutlined style={{ fontSize: 28, color: '#a855f7', marginTop: 2 }} />
              <div>
                <div style={{ fontWeight: 700, fontSize: 14, color: '#1e293b' }}>
                  📱 Ứng dụng Di Động (HRMS Mobile iOS / Android)
                </div>
                <div style={{ fontSize: 13, color: '#475569', marginTop: 4, lineHeight: 1.5 }}>
                  Ứng dụng di động đang trong giai đoạn xây dựng. Hiện tại bạn có thể truy cập mượt mà trên trình duyệt điện thoại qua địa chỉ:
                  <div style={{ marginTop: 4 }}>
                    <Tag color="blue" style={{ fontSize: 12, fontWeight: 600 }}>
                      https://tryhardagain.com
                    </Tag>
                    <span style={{ fontSize: 12, color: '#64748b' }}>(Giao diện đã tối ưu 100% cho màn hình cảm ứng)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default Login;
