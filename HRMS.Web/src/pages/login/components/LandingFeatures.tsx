import React from 'react';
import { Row, Col, Typography } from 'antd';
import {
  TeamOutlined,
  CalendarOutlined,
  DollarOutlined,
  SafetyCertificateOutlined,
  RobotOutlined,
  TrophyOutlined,
} from '@ant-design/icons';
import { ScrollReveal } from '../../../components/ScrollReveal';

const { Title } = Typography;

interface LandingFeaturesProps {
  tLanding: any;
}

export const LandingFeatures: React.FC<LandingFeaturesProps> = ({ tLanding }) => {
  return (
    <div style={{ marginBottom: 70 }}>
      <ScrollReveal direction="up" duration={550}>
        <div style={{ textAlign: 'center', marginBottom: 36 }}>
          <div style={{ fontSize: 13, fontWeight: 700, color: '#60a5fa', textTransform: 'uppercase', letterSpacing: '1px', marginBottom: 8 }}>
            {tLanding.featuresBadge}
          </div>
          <Title level={2} style={{ color: '#fff', margin: 0, fontWeight: 800 }}>
            {tLanding.featuresTitle}
          </Title>
        </div>
      </ScrollReveal>

      <Row gutter={[20, 20]}>
        {/* ROW 1: TRÁI -> DƯỚI -> PHẢI */}
        <Col xs={24} sm={12} lg={8}>
          <ScrollReveal direction="left" delay={0} duration={600} fullHeight>
            <div style={{ height: '100%', background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
              <TeamOutlined style={{ fontSize: 30, color: '#60a5fa', marginBottom: 12 }} />
              <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f1_title}</div>
              <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                {tLanding.f1_desc}
              </div>
            </div>
          </ScrollReveal>
        </Col>

        <Col xs={24} sm={12} lg={8}>
          <ScrollReveal direction="up" delay={80} duration={600} fullHeight>
            <div style={{ height: '100%', background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
              <CalendarOutlined style={{ fontSize: 30, color: '#4ade80', marginBottom: 12 }} />
              <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f2_title}</div>
              <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                {tLanding.f2_desc}
              </div>
            </div>
          </ScrollReveal>
        </Col>

        <Col xs={24} sm={12} lg={8}>
          <ScrollReveal direction="right" delay={160} duration={600} fullHeight>
            <div style={{ height: '100%', background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
              <DollarOutlined style={{ fontSize: 30, color: '#fb923c', marginBottom: 12 }} />
              <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f3_title}</div>
              <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                {tLanding.f3_desc}
              </div>
            </div>
          </ScrollReveal>
        </Col>

        {/* ROW 2: TRÁI -> DƯỚI -> PHẢI */}
        <Col xs={24} sm={12} lg={8}>
          <ScrollReveal direction="left" delay={80} duration={600} fullHeight>
            <div style={{ height: '100%', background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
              <SafetyCertificateOutlined style={{ fontSize: 30, color: '#c084fc', marginBottom: 12 }} />
              <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f4_title}</div>
              <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                {tLanding.f4_desc}
              </div>
            </div>
          </ScrollReveal>
        </Col>

        <Col xs={24} sm={12} lg={8}>
          <ScrollReveal direction="up" delay={160} duration={600} fullHeight>
            <div style={{ height: '100%', background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
              <RobotOutlined style={{ fontSize: 30, color: '#38bdf8', marginBottom: 12 }} />
              <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f5_title}</div>
              <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                {tLanding.f5_desc}
              </div>
            </div>
          </ScrollReveal>
        </Col>

        <Col xs={24} sm={12} lg={8}>
          <ScrollReveal direction="right" delay={240} duration={600} fullHeight>
            <div style={{ height: '100%', background: 'rgba(30, 41, 59, 0.5)', border: '1px solid rgba(255,255,255,0.06)', borderRadius: 14, padding: '22px' }}>
              <TrophyOutlined style={{ fontSize: 30, color: '#eab308', marginBottom: 12 }} />
              <div style={{ color: '#fff', fontWeight: 700, fontSize: 16, marginBottom: 6 }}>{tLanding.f6_title}</div>
              <div style={{ color: '#94a3b8', fontSize: 13.5, lineHeight: 1.6 }}>
                {tLanding.f6_desc}
              </div>
            </div>
          </ScrollReveal>
        </Col>
      </Row>
    </div>
  );
};
