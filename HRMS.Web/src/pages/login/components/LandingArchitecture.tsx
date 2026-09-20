import React from 'react';
import { Row, Col, Typography, Tag, Space, Button } from 'antd';
import {
  CodeOutlined,
  LoginOutlined,
  RocketOutlined,
  MailOutlined,
  FacebookOutlined,
  GithubOutlined,
} from '@ant-design/icons';
import { AUTHOR_INFO } from '../types';

const { Title, Paragraph } = Typography;

interface LandingArchitectureProps {
  tLanding: any;
  onOpenLogin: () => void;
}

export const LandingArchitecture: React.FC<LandingArchitectureProps> = ({
  tLanding,
  onOpenLogin,
}) => {
  return (
    <>
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
                onClick={onOpenLogin}
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
    </>
  );
};
