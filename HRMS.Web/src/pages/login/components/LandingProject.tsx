import React from 'react';
import { GithubOutlined, LoginOutlined, DownloadOutlined } from '@ant-design/icons';
import { ScrollReveal } from '../../../components/ScrollReveal';
import { AUTHOR_INFO } from '../types';

interface LandingProjectProps {
  tLanding: any;
  onOpenLogin: () => void;
  onOpenDownload: () => void;
}

export const LandingProject: React.FC<LandingProjectProps> = ({
  tLanding,
  onOpenLogin,
  onOpenDownload,
}) => {
  return (
    <section
      id="featured-project"
      className="landing-section-project"
      style={{
        position: 'relative',
        padding: 'clamp(64px, 9vw, 112px) clamp(20px, 5vw, 64px)',
        overflow: 'hidden',
        minHeight: 580,
      }}
    >
      {/* BACKGROUND FOREST IMAGE WITH DARK GRADIENT OVERLAYS */}
      <div
        className="project-bg-layer"
        aria-hidden="true"
        style={{
          position: 'absolute',
          inset: 0,
          backgroundImage: 'url(/images/landing/project-forest.jpg)',
          backgroundSize: 'cover',
          backgroundPosition: 'center 40%',
          opacity: 0.9,
          zIndex: 1,
        }}
      />
      <div
        className="project-gradient-overlay"
        aria-hidden="true"
        style={{
          position: 'absolute',
          inset: 0,
          background: `
            linear-gradient(to right, rgba(18, 30, 24, 0.86) 0%, rgba(18, 30, 24, 0.68) 45%, rgba(18, 30, 24, 0.28) 100%),
            linear-gradient(to bottom, rgba(18, 30, 24, 0.85) 0%, transparent 20%, transparent 80%, rgba(18, 30, 24, 0.95) 100%)
          `,
          zIndex: 2,
        }}
      />

      <div
        style={{
          maxWidth: 1160,
          margin: '0 auto',
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
          alignItems: 'center',
          gap: 'clamp(40px, 6vw, 64px)',
          position: 'relative',
          zIndex: 3,
        }}
      >
        {/* LEFT COLUMN: PROJECT DETAILS & CTAs */}
        <ScrollReveal direction="left" duration={1500} waitForIntro={true}>
          <div style={{ textAlign: 'left' }}>
            {/* SUB-HEADER LABEL */}
            <div
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 12,
                fontSize: 12,
                letterSpacing: '2.5px',
                color: 'var(--slide-accent, #DCC58E)',
                fontWeight: 600,
                textTransform: 'uppercase',
                marginBottom: 16,
              }}
            >
              <span>{tLanding?.projectBadge || 'DỰ ÁN ĐANG PHÁT TRIỂN'}</span>
              <span style={{ width: 42, height: 1, backgroundColor: 'currentColor', opacity: 0.5 }} />
            </div>

            <h2
              style={{
                fontFamily: "'Lora', 'Playfair Display', Georgia, serif",
                fontSize: 'clamp(32px, 4.5vw, 48px)',
                fontWeight: 600,
                color: 'var(--landing-text, #F4EEDC)',
                lineHeight: 1.2,
                margin: '0 0 20px 0',
                letterSpacing: '-0.4px',
              }}
            >
              {tLanding?.projectTitle || 'Hệ thống quản lý nhân sự'}
            </h2>

            <p
              style={{
                fontSize: 'clamp(15px, 1.8vw, 17px)',
                lineHeight: 1.7,
                color: 'var(--landing-muted, #C5CCBF)',
                margin: '0 0 20px 0',
                maxWidth: 480,
              }}
            >
              {tLanding?.projectDesc ||
                'Quản lý hồ sơ nhân viên, chấm công và tính lương trên Desktop, Web và Mobile.'}
            </p>

            <div
              style={{
                fontSize: 14,
                color: 'rgba(244, 238, 220, 0.75)',
                fontWeight: 500,
                marginBottom: 36,
                letterSpacing: '0.4px',
              }}
            >
              {tLanding?.projectTags || 'Hồ sơ nhân viên · Chấm công · Tiền lương'}
            </div>

            {/* THREE ACTION BUTTONS (MATCHING TARGET DESIGN) */}
            <div
              className="project-cta-group"
              style={{
                display: 'flex',
                alignItems: 'center',
                flexWrap: 'wrap',
                gap: 14,
              }}
            >
              {/* BUTTON 1: XEM GITHUB (WHITE PILL) */}
              <a
                href={AUTHOR_INFO.github}
                target="_blank"
                rel="noopener noreferrer"
                className="btn-github-pill"
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 8,
                  padding: '11px 22px',
                  borderRadius: 24,
                  backgroundColor: '#FFFFFF',
                  color: '#15211B',
                  fontWeight: 600,
                  fontSize: 14,
                  textDecoration: 'none',
                  boxShadow: '0 4px 14px rgba(0,0,0,0.25)',
                  transition: 'all 0.3s ease',
                }}
              >
                <GithubOutlined style={{ fontSize: 16 }} />
                <span>{tLanding?.btnViewGithub || 'Xem GitHub'}</span>
              </a>

              {/* BUTTON 2: ĐĂNG NHẬP (DARK SEMI-TRANSPARENT PILL) */}
              <button
                type="button"
                onClick={onOpenLogin}
                className="btn-login-pill"
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 8,
                  padding: '10px 22px',
                  borderRadius: 24,
                  backgroundColor: 'rgba(255, 255, 255, 0.08)',
                  backdropFilter: 'blur(8px)',
                  border: '1px solid rgba(255, 255, 255, 0.22)',
                  color: '#F4EEDC',
                  fontWeight: 600,
                  fontSize: 14,
                  cursor: 'pointer',
                  transition: 'all 0.3s ease',
                }}
              >
                <LoginOutlined style={{ fontSize: 15 }} />
                <span>{tLanding?.signIn || 'Đăng nhập'}</span>
              </button>

              {/* BUTTON 3: TẢI ỨNG DỤNG (SAGE/OLIVE PILL) */}
              <button
                type="button"
                onClick={onOpenDownload}
                className="btn-download-pill"
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 8,
                  padding: '10px 22px',
                  borderRadius: 24,
                  backgroundColor: 'rgba(74, 102, 82, 0.55)',
                  backdropFilter: 'blur(8px)',
                  border: '1px solid rgba(135, 170, 145, 0.4)',
                  color: '#F4EEDC',
                  fontWeight: 600,
                  fontSize: 14,
                  cursor: 'pointer',
                  transition: 'all 0.3s ease',
                }}
              >
                <DownloadOutlined style={{ fontSize: 15 }} />
                <span>{tLanding?.btnDownloadApp || 'Tải ứng dụng'}</span>
              </button>
            </div>
          </div>
        </ScrollReveal>

        {/* RIGHT COLUMN: REALISTIC DEVICE MOCKUP (DESKTOP + MOBILE) */}
        <ScrollReveal direction="right" delay={450} duration={1500} waitForIntro={true}>
          <div
            className="device-mockup-wrap"
            style={{
              position: 'relative',
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
            }}
          >
            {/* DESKTOP MONITOR */}
            <div
              className="monitor-mockup"
              style={{
                width: '100%',
                maxWidth: 460,
                aspectRatio: '16 / 10',
                backgroundColor: '#1E2B23',
                borderRadius: '8px 8px 0 0',
                border: '6px solid #2B3A30',
                borderBottomWidth: 14,
                boxShadow: '0 20px 48px rgba(0, 0, 0, 0.5)',
                position: 'relative',
                overflow: 'hidden',
              }}
            >
              {/* Web Header inside screen */}
              <div
                style={{
                  height: 22,
                  backgroundColor: '#EBE5D6',
                  display: 'flex',
                  alignItems: 'center',
                  padding: '0 10px',
                  gap: 5,
                  borderBottom: '1px solid rgba(0,0,0,0.06)',
                }}
              >
                <span style={{ width: 6, height: 6, borderRadius: '50%', backgroundColor: '#D98282' }} />
                <span style={{ width: 6, height: 6, borderRadius: '50%', backgroundColor: '#E5BF6D' }} />
                <span style={{ width: 6, height: 6, borderRadius: '50%', backgroundColor: '#8BC38F' }} />
                <span style={{ width: 90, height: 8, borderRadius: 4, backgroundColor: '#DDD6C4', marginLeft: 10 }} />
              </div>

              {/* Screen Body */}
              <div
                style={{
                  height: 'calc(100% - 22px)',
                  backgroundColor: '#F4EFE3',
                  padding: '12px 14px',
                  display: 'flex',
                  gap: 12,
                }}
              >
                {/* Mini sidebar */}
                <div style={{ width: 34, display: 'flex', flexDirection: 'column', gap: 6, opacity: 0.55 }}>
                  <div style={{ width: 18, height: 18, borderRadius: '50%', backgroundColor: '#384D3F' }} />
                  <div style={{ width: 24, height: 4, borderRadius: 2, backgroundColor: '#8CA091' }} />
                  <div style={{ width: 20, height: 4, borderRadius: 2, backgroundColor: '#8CA091' }} />
                  <div style={{ width: 22, height: 4, borderRadius: 2, backgroundColor: '#8CA091' }} />
                </div>

                {/* Main content pane */}
                <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 8 }}>
                  {/* Top card */}
                  <div
                    style={{
                      height: 52,
                      backgroundColor: '#FFFFFF',
                      borderRadius: 6,
                      border: '1px solid #E3DCB',
                      padding: '8px 10px',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 10,
                    }}
                  >
                    <div style={{ width: 32, height: 32, borderRadius: '50%', backgroundColor: '#88A28E' }} />
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
                      <div style={{ width: 110, height: 7, borderRadius: 3, backgroundColor: '#384D3F' }} />
                      <div style={{ width: 70, height: 5, borderRadius: 3, backgroundColor: '#98AC9D' }} />
                    </div>
                  </div>

                  {/* Stat cards row */}
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 6 }}>
                    {[1, 2, 3].map((k) => (
                      <div
                        key={k}
                        style={{
                          height: 48,
                          backgroundColor: '#FFFFFF',
                          borderRadius: 6,
                          padding: 6,
                          display: 'flex',
                          flexDirection: 'column',
                          justifyContent: 'center',
                          gap: 4,
                        }}
                      >
                        <div style={{ width: '50%', height: 4, backgroundColor: '#ADC0B2', borderRadius: 2 }} />
                        <div style={{ width: '80%', height: 7, backgroundColor: '#384D3F', borderRadius: 3 }} />
                      </div>
                    ))}
                  </div>

                  {/* Bottom table rows */}
                  <div
                    style={{
                      flex: 1,
                      backgroundColor: '#FFFFFF',
                      borderRadius: 6,
                      padding: '6px 8px',
                      display: 'flex',
                      flexDirection: 'column',
                      gap: 5,
                    }}
                  >
                    <div style={{ width: '100%', height: 6, backgroundColor: '#EDE8D8', borderRadius: 2 }} />
                    <div style={{ width: '90%', height: 5, backgroundColor: '#F0ECE0', borderRadius: 2 }} />
                    <div style={{ width: '95%', height: 5, backgroundColor: '#F0ECE0', borderRadius: 2 }} />
                  </div>
                </div>
              </div>
            </div>

            {/* MONITOR STAND */}
            <div
              style={{
                position: 'absolute',
                bottom: -22,
                left: '42%',
                width: 70,
                height: 22,
                backgroundColor: '#243229',
                borderTop: '1px solid #384A3E',
                zIndex: 1,
              }}
            />
            <div
              style={{
                position: 'absolute',
                bottom: -28,
                left: '32%',
                width: 160,
                height: 6,
                backgroundColor: '#1E2B23',
                borderRadius: '3px 3px 0 0',
                zIndex: 1,
              }}
            />

            {/* OVERLAPPING SMARTPHONE MOCKUP ON BOTTOM RIGHT */}
            <div
              className="phone-mockup"
              style={{
                position: 'absolute',
                right: -10,
                bottom: -20,
                width: 130,
                aspectRatio: '9 / 18.5',
                backgroundColor: '#1A251E',
                borderRadius: 18,
                border: '5px solid #2B3A30',
                boxShadow: '0 16px 36px rgba(0, 0, 0, 0.6)',
                zIndex: 10,
                overflow: 'hidden',
                padding: '6px 6px',
              }}
            >
              {/* Phone screen */}
              <div
                style={{
                  width: '100%',
                  height: '100%',
                  backgroundColor: '#F3EFE4',
                  borderRadius: 12,
                  padding: 8,
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 6,
                }}
              >
                {/* Notch */}
                <div style={{ width: 28, height: 3, backgroundColor: '#2B3A30', borderRadius: 2, margin: '0 auto 4px auto' }} />

                {/* Avatar & Greeting */}
                <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                  <div style={{ width: 20, height: 20, borderRadius: '50%', backgroundColor: '#6C8572' }} />
                  <div style={{ width: 50, height: 5, backgroundColor: '#384D3F', borderRadius: 2 }} />
                </div>

                {/* Card 1: Check-in / Punch */}
                <div
                  style={{
                    backgroundColor: '#FFFFFF',
                    borderRadius: 6,
                    padding: 6,
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 3,
                  }}
                >
                  <div style={{ width: '40%', height: 4, backgroundColor: '#ADC0B2', borderRadius: 2 }} />
                  <div style={{ width: '70%', height: 6, backgroundColor: '#384D3F', borderRadius: 2 }} />
                </div>

                {/* Card 2: Payslip */}
                <div
                  style={{
                    backgroundColor: '#FFFFFF',
                    borderRadius: 6,
                    padding: 6,
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 3,
                  }}
                >
                  <div style={{ width: '50%', height: 4, backgroundColor: '#ADC0B2', borderRadius: 2 }} />
                  <div style={{ width: '85%', height: 6, backgroundColor: '#4F6C56', borderRadius: 2 }} />
                </div>
              </div>
            </div>
          </div>
        </ScrollReveal>
      </div>
    </section>
  );
};
