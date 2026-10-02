import React, { useState, useRef, useEffect } from 'react';
import { MailOutlined, FacebookOutlined, GithubOutlined, ArrowRightOutlined, CopyOutlined, CheckOutlined } from '@ant-design/icons';
import { ScrollReveal } from '../../../components/ScrollReveal';
import { AUTHOR_INFO } from '../types';

interface LandingContactProps {
  tLanding: any;
}

export const LandingContact: React.FC<LandingContactProps> = ({ tLanding }) => {
  const [copied, setCopied] = useState<boolean>(false);
  const copyTimeoutRef = useRef<number | null>(null);

  const handleCopyEmail = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    if (navigator.clipboard?.writeText) {
      navigator.clipboard
        .writeText(AUTHOR_INFO.email)
        .then(() => {
          setCopied(true);
          if (copyTimeoutRef.current) clearTimeout(copyTimeoutRef.current);
          copyTimeoutRef.current = window.setTimeout(() => {
            setCopied(false);
          }, 2000);
        })
        .catch(() => {
          setCopied(false);
        });
    }
  };

  useEffect(() => {
    return () => {
      if (copyTimeoutRef.current) clearTimeout(copyTimeoutRef.current);
    };
  }, []);

  return (
    <section
      id="contact-connect"
      className="landing-section-contact"
      style={{
        position: 'relative',
        padding: 'clamp(64px, 9vw, 110px) clamp(20px, 5vw, 64px) clamp(48px, 6vw, 72px) clamp(20px, 5vw, 64px)',
        overflow: 'hidden',
        minHeight: 520,
      }}
    >
      {/* WOODLAND STREAM BACKGROUND IMAGE */}
      <div
        className="contact-bg-layer"
        aria-hidden="true"
        style={{
          position: 'absolute',
          inset: 0,
          backgroundImage: 'url(/images/landing/contact-stream.jpg)',
          backgroundSize: 'cover',
          backgroundPosition: 'center bottom',
          opacity: 0.78,
          zIndex: 1,
        }}
      />
      <div
        className="contact-gradient-overlay"
        aria-hidden="true"
        style={{
          position: 'absolute',
          inset: 0,
          background: `
            radial-gradient(ellipse at 50% 30%, rgba(18, 30, 24, 0.64) 0%, rgba(18, 30, 24, 0.87) 75%),
            linear-gradient(to bottom, rgba(18, 30, 24, 0.95) 0%, transparent 20%, transparent 75%, rgba(18, 30, 24, 0.98) 100%)
          `,
          zIndex: 2,
        }}
      />

      <div
        style={{
          maxWidth: 720,
          margin: '0 auto',
          textAlign: 'center',
          position: 'relative',
          zIndex: 3,
        }}
      >
        <ScrollReveal direction="up" duration={1500} waitForIntro={true}>
          <h2
            style={{
              fontFamily: "'Lora', 'Playfair Display', Georgia, serif",
              fontSize: 'clamp(28px, 4vw, 42px)',
              fontWeight: 600,
              color: 'var(--landing-text, #F4EEDC)',
              margin: '0 auto',
            }}
          >
            {tLanding?.contactTitle || 'Kết nối & trao đổi'}
          </h2>
          <div
            style={{
              width: 44,
              height: 1.5,
              backgroundColor: '#DCC58E',
              margin: '14px auto 18px auto',
              opacity: 0.65,
            }}
          />
          <p
            style={{
              fontSize: 'clamp(14.5px, 1.8vw, 16px)',
              lineHeight: 1.65,
              color: 'var(--landing-muted, #C5CCBF)',
              margin: '0 auto 42px auto',
              maxWidth: 520,
            }}
          >
            {tLanding?.contactDesc ||
              'Bạn muốn trao đổi kỹ thuật hoặc cùng phát triển một dự án? Mình rất vui được kết nối.'}
          </p>
        </ScrollReveal>

        {/* THREE CONTACT ROWS (MATCHING TARGET DESIGN) */}
        <div
          className="contact-rows-list"
          style={{
            maxWidth: 620,
            margin: '0 auto',
            borderTop: '1px solid rgba(244, 238, 220, 0.15)',
          }}
        >
          {/* ROW 1: EMAIL */}
          <ScrollReveal direction="up" delay={250} duration={1400} waitForIntro={true}>
            <div
              className="contact-row-item"
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '18px 8px',
                borderBottom: '1px solid rgba(244, 238, 220, 0.15)',
                color: 'var(--landing-text, #F4EEDC)',
                transition: 'background-color 0.25s ease',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 18 }}>
                <MailOutlined style={{ fontSize: 18, color: '#DCC58E', opacity: 0.9 }} />
                <span style={{ fontSize: 15, fontWeight: 500, minWidth: 70, textAlign: 'left' }}>Email</span>
              </div>

              <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <a
                  href={`mailto:${AUTHOR_INFO.email}`}
                  style={{
                    color: 'var(--landing-text, #F4EEDC)',
                    textDecoration: 'none',
                    fontSize: 14.5,
                    letterSpacing: '0.2px',
                  }}
                >
                  {AUTHOR_INFO.email}
                </a>

                <button
                  type="button"
                  onClick={handleCopyEmail}
                  title={copied ? tLanding.copiedEmail : tLanding.copyEmail}
                  aria-label={tLanding.copyEmail}
                  style={{
                    background: copied ? 'rgba(100, 180, 120, 0.2)' : 'rgba(255, 255, 255, 0.08)',
                    border: '1px solid rgba(255, 255, 255, 0.15)',
                    borderRadius: 4,
                    padding: '3px 8px',
                    color: copied ? '#86EFAC' : '#C5CCBF',
                    fontSize: 12,
                    cursor: 'pointer',
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: 4,
                    transition: 'all 0.25s ease',
                  }}
                >
                  {copied ? <CheckOutlined /> : <CopyOutlined />}
                  <span role="status" aria-live="polite">
                    {copied ? (tLanding?.copiedEmail || 'Đã sao chép') : ''}
                  </span>
                </button>
              </div>

              <a
                href={`mailto:${AUTHOR_INFO.email}`}
                aria-label={tLanding.sendEmail}
                style={{ color: 'rgba(244, 238, 220, 0.5)', transition: 'color 0.25s' }}
              >
                <ArrowRightOutlined style={{ fontSize: 14 }} />
              </a>
            </div>
          </ScrollReveal>

          {/* ROW 2: FACEBOOK */}
          <ScrollReveal direction="up" delay={500} duration={1400} waitForIntro={true}>
            <a
              href={AUTHOR_INFO.facebook}
              target="_blank"
              rel="noopener noreferrer"
              className="contact-row-item"
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '18px 8px',
                borderBottom: '1px solid rgba(244, 238, 220, 0.15)',
                color: 'var(--landing-text, #F4EEDC)',
                textDecoration: 'none',
                transition: 'background-color 0.25s ease',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 18 }}>
                <FacebookOutlined style={{ fontSize: 18, color: '#DCC58E', opacity: 0.9 }} />
                <span style={{ fontSize: 15, fontWeight: 500, minWidth: 70, textAlign: 'left' }}>Facebook</span>
              </div>

              <div style={{ fontSize: 14.5, letterSpacing: '0.2px', color: 'var(--landing-text, #F4EEDC)' }}>
                {AUTHOR_INFO.facebookUsername || 'duy.nguyentho.7'}
              </div>

              <div style={{ color: 'rgba(244, 238, 220, 0.5)' }}>
                <ArrowRightOutlined style={{ fontSize: 14 }} />
              </div>
            </a>
          </ScrollReveal>

          {/* ROW 3: GITHUB */}
          <ScrollReveal direction="up" delay={750} duration={1400} waitForIntro={true}>
            <a
              href={AUTHOR_INFO.githubProfile || 'https://github.com/duy2k1lnbg'}
              target="_blank"
              rel="noopener noreferrer"
              className="contact-row-item"
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '18px 8px',
                borderBottom: '1px solid rgba(244, 238, 220, 0.15)',
                color: 'var(--landing-text, #F4EEDC)',
                textDecoration: 'none',
                transition: 'background-color 0.25s ease',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 18 }}>
                <GithubOutlined style={{ fontSize: 18, color: '#DCC58E', opacity: 0.9 }} />
                <span style={{ fontSize: 15, fontWeight: 500, minWidth: 70, textAlign: 'left' }}>GitHub</span>
              </div>

              <div style={{ fontSize: 14.5, letterSpacing: '0.2px', color: 'var(--landing-text, #F4EEDC)' }}>
                {AUTHOR_INFO.githubUsername || 'duy2k1lnbg'}
              </div>

              <div style={{ color: 'rgba(244, 238, 220, 0.5)' }}>
                <ArrowRightOutlined style={{ fontSize: 14 }} />
              </div>
            </a>
          </ScrollReveal>
        </div>

        {/* MINIMALIST FOOTER: TryHardAgain · Nguyễn Thọ Duy */}
        <div
          style={{
            marginTop: 64,
            fontSize: 13,
            letterSpacing: '1px',
            color: 'rgba(244, 238, 220, 0.55)',
            fontFamily: "'Lora', Georgia, serif",
          }}
        >
          {tLanding?.footerBrand || 'TryHardAgain · Nguyễn Thọ Duy'}
        </div>
      </div>
    </section>
  );
};
