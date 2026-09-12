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
  Dropdown,
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
  FireOutlined,
  GlobalOutlined,
  DownOutlined,
  CheckOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import type { CurrentUserDTO } from '../types/hrms';
import { CinematicHeroGallery } from '../components/CinematicHeroGallery';
import { useAppLanguage, type AppLanguage } from '../services/i18n';

const { Title, Text, Paragraph } = Typography;

// =============================================================================
// THÔNG TIN TÁC GIẢ & CÁC KÊNH LIÊN HỆ
// =============================================================================
const AUTHOR_INFO = {
  name: 'Nguyễn Thọ Duy',
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

  // Hook đa ngôn ngữ toàn hệ thống (Anh, Việt, Nhật)
  const { lang: currentLang, setLang: handleLanguageChange, tLanding, allConfigs } = useAppLanguage();

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
      console.error('Chi tiết lỗi đăng nhập:', err);
      const errorObj = err as {
        response?: { data?: { Message?: string; message?: string } | string; status?: number };
        message?: string;
        config?: { url?: string; baseURL?: string };
      };
      let msg = 'Không thể kết nối đến máy chủ đăng nhập.';
      if (typeof errorObj.response?.data === 'string') {
        if (
          errorObj.response.data.includes('<html') ||
          errorObj.response.data.includes('Exception') ||
          errorObj.response.data.includes('ORA-')
        ) {
          msg = `Lỗi máy chủ (${errorObj.response?.status || 500}). Vui lòng kiểm tra dịch vụ Backend và kết nối CSDL Oracle.`;
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
        const target = (errorObj.config?.baseURL || '') + (errorObj.config?.url || '');
        msg = `Không thể kết nối máy chủ API (${target || 'HRMS Backend'}: ${errorObj.message}). Vui lòng đảm bảo Backend HRMS_API đang hoạt động!`;
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
              {tLanding.subHeader}
            </div>
          </div>
        </div>

        <Space size="middle">
          <Tag color="success" style={{ padding: '3px 10px', borderRadius: 12, border: 'none', background: 'rgba(34,197,94,0.15)', color: '#4ade80' }}>
            <Badge status="processing" color="#4ade80" /> {tLanding.systemReady}
          </Tag>

          <Button
            type="text"
            icon={<FireOutlined style={{ color: '#fbbf24' }} />}
            onClick={() => {
              const el = document.getElementById('cinematic-hero-gallery');
              if (el) el.scrollIntoView({ behavior: 'smooth' });
            }}
            style={{ color: '#fef08a', fontWeight: 600 }}
          >
            {tLanding.galleryFilm}
          </Button>

          <Button
            type="text"
            icon={<CloudDownloadOutlined style={{ color: '#60a5fa' }} />}
            onClick={() => handleOpenDownload('general')}
            style={{ color: '#e2e8f0', fontWeight: 600 }}
          >
            {tLanding.downloadApp}
          </Button>

          {/* NÚT CHỌN NGÔN NGỮ: ANH, VIỆT, NHẬT */}
          <Dropdown
            menu={{
              items: [
                {
                  key: 'vi',
                  label: (
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 140, padding: '4px 0' }}>
                      <Space size={10}>
                        <span style={{ fontSize: 18 }}>{allConfigs.vi.flag}</span>
                        <span style={{ fontWeight: currentLang === 'vi' ? 700 : 600, color: currentLang === 'vi' ? '#0284c7' : '#0f172a', fontSize: 14 }}>
                          {allConfigs.vi.name}
                        </span>
                      </Space>
                      {currentLang === 'vi' && <CheckOutlined style={{ color: '#0284c7', fontWeight: 700, fontSize: 14 }} />}
                    </div>
                  ),
                },
                {
                  key: 'en',
                  label: (
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 140, padding: '4px 0' }}>
                      <Space size={10}>
                        <span style={{ fontSize: 18 }}>{allConfigs.en.flag}</span>
                        <span style={{ fontWeight: currentLang === 'en' ? 700 : 600, color: currentLang === 'en' ? '#0284c7' : '#0f172a', fontSize: 14 }}>
                          {allConfigs.en.name}
                        </span>
                      </Space>
                      {currentLang === 'en' && <CheckOutlined style={{ color: '#0284c7', fontWeight: 700, fontSize: 14 }} />}
                    </div>
                  ),
                },
                {
                  key: 'ja',
                  label: (
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 140, padding: '4px 0' }}>
                      <Space size={10}>
                        <span style={{ fontSize: 18 }}>{allConfigs.ja.flag}</span>
                        <span style={{ fontWeight: currentLang === 'ja' ? 700 : 600, color: currentLang === 'ja' ? '#0284c7' : '#0f172a', fontSize: 14 }}>
                          {allConfigs.ja.name}
                        </span>
                      </Space>
                      {currentLang === 'ja' && <CheckOutlined style={{ color: '#0284c7', fontWeight: 700, fontSize: 14 }} />}
                    </div>
                  ),
                },
              ],
              selectedKeys: [currentLang],
              onClick: ({ key }) => handleLanguageChange(key as AppLanguage),
            }}
            placement="bottomRight"
            trigger={['click']}
          >
            <Button
              type="text"
              style={{
                color: '#f8fafc',
                fontWeight: 600,
                fontSize: 13,
                background: 'rgba(255, 255, 255, 0.08)',
                backdropFilter: 'blur(12px)',
                border: '1px solid rgba(255, 255, 255, 0.18)',
                borderRadius: 10,
                padding: '0 12px',
                height: 38,
                display: 'inline-flex',
                alignItems: 'center',
                gap: 6,
                cursor: 'pointer',
              }}
            >
              <GlobalOutlined style={{ color: '#38bdf8', fontSize: 15 }} />
              <span>{allConfigs[currentLang].flag} {allConfigs[currentLang].short}</span>
              <DownOutlined style={{ fontSize: 10, color: '#94a3b8' }} />
            </Button>
          </Dropdown>

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
            {tLanding.signIn}
          </Button>
        </Space>
      </header>

      {/* ========================================================================= */}
      {/* 1. CINEMATIC 9-IMAGE HERO HORIZONTAL GALLERY (9 CÂU GIỮ NGUYÊN 100%)      */}
      {/* ========================================================================= */}
      <div id="cinematic-hero-gallery">
        <CinematicHeroGallery />
      </div>

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
              {tLanding.badge}
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
            {tLanding.heroTitle}{' '}
            <span
              style={{
                background: 'linear-gradient(135deg, #60a5fa 0%, #c084fc 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
              }}
            >
              {tLanding.heroTitleHighlight}
            </span>
          </Title>

          <Paragraph style={{ color: '#94a3b8', fontSize: 'clamp(15px, 2vw, 17px)', lineHeight: 1.7, marginBottom: 36, maxWidth: 780, margin: '0 auto 36px' }}>
            {tLanding.heroDesc}
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
              {tLanding.btnSignIn}
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
              {tLanding.btnDownload}
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
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>{tLanding.stat1_label}</div>
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
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>{tLanding.stat2_label}</div>
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
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>{tLanding.stat3_label}</div>
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
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>{tLanding.stat4_label}</div>
            </div>
          </Col>
        </Row>

        {/* SECTION 2: HỆ SINH THÁI TẢI VỀ (DOWNLOAD MULTI-PLATFORM) */}
        <div style={{ marginBottom: 70 }}>
          <div style={{ textAlign: 'center', marginBottom: 36 }}>
            <div style={{ fontSize: 13, fontWeight: 700, color: '#60a5fa', textTransform: 'uppercase', letterSpacing: '1px', marginBottom: 8 }}>
              {tLanding.ecoBadge}
            </div>
            <Title level={2} style={{ color: '#fff', margin: 0, fontWeight: 800 }}>
              {tLanding.ecoTitle}
            </Title>
            <Paragraph style={{ color: '#94a3b8', marginTop: 8, fontSize: 15 }}>
              {tLanding.ecoSubtitle}
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
                    {tLanding.winTagReady}
                  </Tag>
                </div>

                <Title level={4} style={{ color: '#fff', marginBottom: 8 }}>
                  {tLanding.winCardTitle}
                </Title>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 14, fontWeight: 500 }}>
                  {tLanding.winCardSub}
                </div>
                <Paragraph style={{ color: '#cbd5e1', fontSize: 13.5, lineHeight: 1.6, flex: 1 }}>
                  {tLanding.winCardDesc}
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
                  {tLanding.winCardBtn}
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
                    {tLanding.webTagLive}
                  </Tag>
                </div>

                <Title level={4} style={{ color: '#fff', marginBottom: 8 }}>
                  {tLanding.webCardTitle}
                </Title>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 14, fontWeight: 500 }}>
                  {tLanding.webCardSub}
                </div>
                <Paragraph style={{ color: '#cbd5e1', fontSize: 13.5, lineHeight: 1.6, flex: 1 }}>
                  {tLanding.webCardDesc}
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
                  {tLanding.webCardBtn}
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
                    {tLanding.mobileTagComing}
                  </Tag>
                </div>

                <Title level={4} style={{ color: '#fff', marginBottom: 8 }}>
                  {tLanding.mobileCardTitle}
                </Title>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 14, fontWeight: 500 }}>
                  {tLanding.mobileCardSub}
                </div>
                <Paragraph style={{ color: '#cbd5e1', fontSize: 13.5, lineHeight: 1.6, flex: 1 }}>
                  {tLanding.mobileCardDesc}
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
                  {tLanding.mobileCardBtn}
                </Button>
              </Card>
            </Col>
          </Row>
        </div>

        {/* SECTION 3: TÍNH NĂNG NỔI BẬT */}
        <div style={{ marginBottom: 70 }}>
          <div style={{ textAlign: 'center', marginBottom: 36 }}>
            <div style={{ fontSize: 13, fontWeight: 700, color: '#60a5fa', textTransform: 'uppercase', letterSpacing: '1px', marginBottom: 8 }}>
              {tLanding.featuresBadge}
            </div>
            <Title level={2} style={{ color: '#fff', margin: 0, fontWeight: 800 }}>
              {tLanding.featuresTitle}
            </Title>
          </div>

          <Row gutter={[20, 20]}>
            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <TeamOutlined style={{ fontSize: 30, color: '#60a5fa', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f1_title}</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  {tLanding.f1_desc}
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <CalendarOutlined style={{ fontSize: 30, color: '#4ade80', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f2_title}</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  {tLanding.f2_desc}
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <DollarOutlined style={{ fontSize: 30, color: '#fb923c', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f3_title}</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  {tLanding.f3_desc}
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <SafetyCertificateOutlined style={{ fontSize: 30, color: '#c084fc', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f4_title}</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  {tLanding.f4_desc}
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <RobotOutlined style={{ fontSize: 30, color: '#38bdf8', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f5_title}</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  {tLanding.f5_desc}
                </div>
              </div>
            </Col>

            <Col xs={24} sm={12} lg={8}>
              <div style={{ background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
                <TrophyOutlined style={{ fontSize: 30, color: '#eab308', marginBottom: 12 }} />
                <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f6_title}</div>
                <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                  {tLanding.f6_desc}
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
                  {tLanding.archBadge}
                </span>
              </div>
              <Title level={3} style={{ color: '#fff', margin: '0 0 14px 0' }}>
                {tLanding.archTitle}
              </Title>
              <Paragraph style={{ color: '#94a3b8', fontSize: 14.5, lineHeight: 1.7, marginBottom: 18 }}>
                {tLanding.archDesc}
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
                  {tLanding.archReadyTitle}
                </div>
                <div style={{ fontSize: 13, color: '#94a3b8', marginBottom: 20 }}>
                  {tLanding.archReadyDesc}
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
                  {tLanding.archReadyBtn}
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
                  {tLanding.authorBadgeText}
                </div>
              </div>

              <Title level={4} style={{ color: '#fff', margin: '6px 0 4px 0', fontWeight: 800 }}>
                {AUTHOR_INFO.name}
              </Title>
              <div style={{ color: '#60a5fa', fontSize: 13.5, fontWeight: 600, marginBottom: 10 }}>
                {tLanding.authorRoleTitle}
              </div>
              <div style={{ color: '#94a3b8', fontSize: 13, lineHeight: 1.6, maxWidth: 300, margin: '0 auto 16px' }}>
                {tLanding.authorBio}
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
                  {tLanding.authorConnectBadge}
                </span>
              </div>

              <Title level={3} style={{ color: '#fff', margin: '0 0 14px 0', fontWeight: 800 }}>
                {tLanding.authorConnectTitle}
              </Title>

              <Paragraph style={{ color: '#cbd5e1', fontSize: 14, lineHeight: 1.7, marginBottom: 24 }}>
                {tLanding.authorConnectDesc}
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
                        {tLanding.authorEmailDirect}
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
                        {tLanding.authorFbProfile}
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
                        {tLanding.authorGithubSource}
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
          {tLanding.footerLine1}
        </div>
        <div style={{ fontSize: 11.5, color: '#475569', marginTop: 4 }}>
          {tLanding.footerLine2}
        </div>
      </footer>

      {/* ========================================================================= */}
      {/* MODAL ĐĂNG NHẬP HỆ THỐNG                                                  */}
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
            {tLanding.loginModalTitle}
          </Title>
          <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 0, fontSize: 13 }}>
            {tLanding.loginModalSubtitle}
          </Paragraph>
          <Tag color="processing" style={{ marginTop: 8, borderRadius: 10, fontWeight: 500 }}>
            <SafetyCertificateOutlined style={{ marginRight: 4 }} /> {tLanding.loginModalSSLTag}
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
            label={<Text strong style={{ color: '#334155' }}>{tLanding.usernameLabel}</Text>}
            rules={[{ required: true, message: tLanding.usernameRequired }]}
          >
            <Input
              size="large"
              prefix={<UserOutlined style={{ color: '#94a3b8' }} />}
              placeholder={tLanding.usernamePlaceholder}
              autoFocus
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item
            name="password"
            label={<Text strong style={{ color: '#334155' }}>{tLanding.passwordLabel}</Text>}
            rules={[{ required: true, message: tLanding.passwordRequired }]}
          >
            <Input.Password
              size="large"
              prefix={<LockOutlined style={{ color: '#94a3b8' }} />}
              placeholder={tLanding.passwordPlaceholder}
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
              {tLanding.btnLoginSubmit}
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
          {tLanding.loginSecurityFooter}
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
              {tLanding.downloadModalHeader}
            </span>
          </div>
        }
        open={downloadModalVisible}
        onOk={() => setDownloadModalVisible(false)}
        onCancel={() => setDownloadModalVisible(false)}
        okText={tLanding.downloadModalUnderstood}
        cancelButtonProps={{ style: { display: 'none' } }}
        width={560}
        centered
      >
        <div style={{ padding: '12px 4px' }}>
          <Alert
            message={
              downloadType === 'windows'
                ? tLanding.downloadWinAlertTitle
                : downloadType === 'mobile'
                ? tLanding.downloadMobileAlertTitle
                : tLanding.downloadGeneralAlertTitle
            }
            description={
              downloadType === 'windows'
                ? tLanding.downloadWinAlertDesc
                : downloadType === 'mobile'
                ? tLanding.downloadMobileAlertDesc
                : tLanding.downloadGeneralAlertDesc
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
                    {tLanding.downloadWinSectionTitle}
                  </span>
                  <Tag color="success" style={{ fontWeight: 600 }}>{tLanding.winTagReady}</Tag>
                </div>
                <div style={{ fontSize: 13, color: '#475569', marginTop: 6, lineHeight: 1.5 }}>
                  {tLanding.downloadWinSectionDesc}
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
                    {tLanding.downloadWinBtnText}
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
                  {tLanding.downloadMobileSectionTitle}
                </div>
                <div style={{ fontSize: 13, color: '#475569', marginTop: 4, lineHeight: 1.5 }}>
                  {tLanding.downloadMobileSectionDesc}
                  <div style={{ marginTop: 4 }}>
                    <Tag color="blue" style={{ fontSize: 12, fontWeight: 600 }}>
                      https://tryhardagain.com
                    </Tag>
                    <span style={{ fontSize: 12, color: '#64748b' }}>{tLanding.downloadMobileResponsiveNote}</span>
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
