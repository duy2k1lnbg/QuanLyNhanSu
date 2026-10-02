import React from 'react';
import { AuthorFrame } from './AuthorFrame';
import { ScrollReveal } from '../../../components/ScrollReveal';

interface LandingAboutProps {
  tLanding: any;
}

export const LandingAbout: React.FC<LandingAboutProps> = ({ tLanding }) => {
  return (
    <section
      id="about-me"
      className="landing-section-about"
      style={{
        position: 'relative',
        padding: 'clamp(56px, 8vw, 96px) clamp(20px, 5vw, 64px)',
        scrollMarginTop: '72px',
        overflow: 'hidden',
      }}
    >
      {/* Subtle misty fern silhouette on the edges (sways very slowly) */}
      <div
        className="fern-shadow-left"
        aria-hidden="true"
        style={{
          position: 'absolute',
          left: -40,
          top: '20%',
          width: 220,
          height: 380,
          pointerEvents: 'none',
          opacity: 0.05,
          background: 'radial-gradient(ellipse at center, rgba(160, 200, 175, 0.4) 0%, transparent 70%)',
          filter: 'blur(20px)',
          animation: 'leafSwaySlow 22s ease-in-out infinite alternate',
        }}
      />
      <div
        className="fern-shadow-right"
        aria-hidden="true"
        style={{
          position: 'absolute',
          right: -40,
          bottom: '15%',
          width: 240,
          height: 400,
          pointerEvents: 'none',
          opacity: 0.05,
          background: 'radial-gradient(ellipse at center, rgba(160, 200, 175, 0.4) 0%, transparent 70%)',
          filter: 'blur(24px)',
          animation: 'leafSwaySlow 26s ease-in-out infinite alternate-reverse',
        }}
      />

      <div
        style={{
          maxWidth: 1040,
          margin: '0 auto',
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
          alignItems: 'center',
          gap: 'clamp(36px, 6vw, 72px)',
          position: 'relative',
          zIndex: 2,
        }}
      >
        {/* LEFT COLUMN: AUTHOR MONOGRAM FRAME */}
        <ScrollReveal direction="fade" duration={1400} waitForIntro={true}>
          <div style={{ display: 'flex', justifyContent: 'center' }}>
            <AuthorFrame />
          </div>
        </ScrollReveal>

        {/* RIGHT COLUMN: BIO & INTRO */}
        <ScrollReveal direction="up" delay={450} duration={1500} waitForIntro={true}>
          <div style={{ textAlign: 'left' }}>
            <h2
              style={{
                fontFamily: "'Lora', 'Playfair Display', Georgia, serif",
                fontSize: 'clamp(28px, 4.2vw, 44px)',
                fontWeight: 600,
                color: 'var(--landing-text, #F4EEDC)',
                lineHeight: 1.25,
                margin: 0,
                letterSpacing: '-0.3px',
              }}
            >
              {tLanding?.aboutGreeting || 'Chào bạn, mình là Duy.'}
            </h2>

            <div
              style={{
                fontFamily: "'Plus Jakarta Sans', 'Segoe UI', sans-serif",
                fontSize: 17,
                fontWeight: 600,
                color: 'var(--landing-muted, #C5CCBF)',
                marginTop: 10,
                marginBottom: 24,
                letterSpacing: '0.2px',
              }}
            >
              {tLanding?.aboutAuthorName || 'Nguyễn Thọ Duy'}
            </div>

            <p
              style={{
                fontSize: 'clamp(15px, 1.8vw, 17px)',
                lineHeight: 1.75,
                color: 'var(--landing-muted, #C5CCBF)',
                margin: '0 0 16px 0',
                maxWidth: 580,
              }}
            >
              {tLanding?.aboutBio1 ||
                'Mình phát triển phần mềm với C# .NET và React, tập trung vào hệ thống quản lý nhân sự.'}
            </p>

            <p
              style={{
                fontSize: 'clamp(15px, 1.8vw, 17px)',
                lineHeight: 1.75,
                color: 'var(--landing-muted, #C5CCBF)',
                margin: '0 0 28px 0',
                maxWidth: 580,
              }}
            >
              {tLanding?.aboutBio2 ||
                'Mình quan tâm đến cách dữ liệu, nghiệp vụ và trải nghiệm sử dụng kết nối với nhau.'}
            </p>

            {/* SKILLS TAGLINE */}
            <div
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 12,
                fontSize: 14,
                fontWeight: 500,
                color: 'var(--landing-text, #F4EEDC)',
                letterSpacing: '0.5px',
                paddingTop: 12,
                borderTop: '1px solid var(--landing-border, rgba(239, 232, 212, 0.16))',
              }}
            >
              <span>{tLanding?.aboutSkills || 'C# .NET · React · Cơ sở dữ liệu'}</span>
            </div>
          </div>
        </ScrollReveal>
      </div>
    </section>
  );
};
