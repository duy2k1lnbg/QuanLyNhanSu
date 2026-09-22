import React from 'react';
import { Row, Col, Card, Typography, Tag, Button } from 'antd';
import {
  WindowsOutlined,
  DatabaseOutlined,
  MobileOutlined,
  CloudDownloadOutlined,
  ArrowRightOutlined,
} from '@ant-design/icons';

const { Title, Paragraph } = Typography;

interface LandingEcosystemProps {
  tLanding: any;
  onDownloadWindows: () => void;
  onDownloadMobile?: () => void;
  onOpenLogin: () => void;
  onOpenDownload: (type?: 'windows' | 'mobile' | 'general') => void;
}

export const LandingEcosystem: React.FC<LandingEcosystemProps> = ({
  tLanding,
  onDownloadWindows,
  onDownloadMobile,
  onOpenLogin,
  onOpenDownload,
}) => {
  return (
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
              onClick={onDownloadWindows}
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
              onClick={onOpenLogin}
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
              <Tag color="success" style={{ borderRadius: 6, fontWeight: 600, padding: '2px 8px' }}>
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
              type="primary"
              icon={<CloudDownloadOutlined />}
              block
              onClick={() => onDownloadMobile ? onDownloadMobile() : onOpenDownload('mobile')}
              style={{
                height: 42,
                borderRadius: 10,
                fontWeight: 700,
                background: 'linear-gradient(135deg, #a855f7 0%, #7e22ce 100%)',
                border: 'none',
                color: '#fff',
                marginTop: 16,
              }}
            >
              {tLanding.mobileCardBtn}
            </Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
