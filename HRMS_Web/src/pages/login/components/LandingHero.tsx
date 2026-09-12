import React from 'react';
import { Typography, Space, Button, Row, Col } from 'antd';
import {
  RocketOutlined,
  LoginOutlined,
  CloudDownloadOutlined,
} from '@ant-design/icons';

const { Title, Paragraph } = Typography;

interface LandingHeroProps {
  tLanding: any;
  onOpenLogin: () => void;
  onOpenDownload: (type?: 'windows' | 'mobile' | 'general') => void;
}

export const LandingHero: React.FC<LandingHeroProps> = ({
  tLanding,
  onOpenLogin,
  onOpenDownload,
}) => {
  return (
    <>
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
            onClick={onOpenLogin}
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
            onClick={() => onOpenDownload('general')}
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
    </>
  );
};
