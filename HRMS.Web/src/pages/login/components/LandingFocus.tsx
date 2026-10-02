import React from 'react';
import { ScrollReveal } from '../../../components/ScrollReveal';

interface LandingFocusProps {
  tLanding: any;
}

export const LandingFocus: React.FC<LandingFocusProps> = ({ tLanding }) => {
  const items = [
    {
      num: '01',
      title: tLanding?.focus1Title || '01 · Hiểu nghiệp vụ',
      desc: tLanding?.focus1Desc || 'Tìm hiểu cách nhân sự, chấm công và tiền lương vận hành.',
      icon: (
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
          <path d="M11 20A7 7 0 0 1 9.8 6.1C15.5 5 17 4.48 19 2c1 2 2 4.18 2 8 0 5.5-4.78 10-10 10Z" />
          <path d="M2 21c0-3 1.85-5.36 5.08-6C9.5 14.52 12 13 13 12" />
        </svg>
      ),
    },
    {
      num: '02',
      title: tLanding?.focus2Title || '02 · Xây dựng hệ thống',
      desc: tLanding?.focus2Desc || 'Kết nối dữ liệu và các nền tảng sử dụng.',
      icon: (
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
          <circle cx="18" cy="5" r="3" />
          <circle cx="6" cy="12" r="3" />
          <circle cx="18" cy="19" r="3" />
          <line x1="8.59" y1="13.51" x2="15.42" y2="17.49" />
          <line x1="15.41" y1="6.51" x2="8.59" y2="10.49" />
        </svg>
      ),
    },
    {
      num: '03',
      title: tLanding?.focus3Title || '03 · Cải thiện trải nghiệm',
      desc: tLanding?.focus3Desc || 'Làm cho thao tác rõ ràng và dễ sử dụng hơn.',
      icon: (
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
          <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
          <path d="m9 12 2 2 4-4" />
        </svg>
      ),
    },
  ];

  return (
    <section
      id="focus-areas"
      className="landing-section-focus"
      style={{
        position: 'relative',
        padding: 'clamp(56px, 8vw, 88px) clamp(20px, 5vw, 64px)',
        borderTop: '1px solid var(--landing-border, rgba(239, 232, 212, 0.12))',
        borderBottom: '1px solid var(--landing-border, rgba(239, 232, 212, 0.12))',
        background: 'rgba(20, 33, 27, 0.4)',
      }}
    >
      <div style={{ maxWidth: 1120, margin: '0 auto', textAlign: 'center' }}>
        <ScrollReveal direction="up" duration={1400} waitForIntro={true}>
          <h2
            style={{
              fontFamily: "'Lora', 'Playfair Display', Georgia, serif",
              fontSize: 'clamp(26px, 3.8vw, 38px)',
              fontWeight: 600,
              color: 'var(--landing-text, #F4EEDC)',
              margin: '0 auto',
              display: 'inline-block',
            }}
          >
            {tLanding?.focusTitle || 'Những điều mình đang tập trung'}
          </h2>
          {/* Subtle horizontal accent underline */}
          <div
            style={{
              width: 48,
              height: 1.5,
              backgroundColor: '#DCC58E',
              margin: '16px auto 48px auto',
              opacity: 0.65,
            }}
          />
        </ScrollReveal>

        {/* 3 FOCUS COLUMNS WITH THIN DIVIDERS */}
        <div
          className="focus-columns-grid"
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: 0,
            alignItems: 'stretch',
          }}
        >
          {items.map((item, index) => (
            <ScrollReveal
              key={item.num}
              direction="up"
              delay={250 + index * 250}
              duration={1400}
              waitForIntro={true}
              className="focus-col-item"
              style={{
                padding: 'clamp(20px, 3vw, 36px) clamp(16px, 2.5vw, 32px)',
                textAlign: 'left',
                borderRight:
                  index < items.length - 1
                    ? '1px solid var(--landing-border, rgba(239, 232, 212, 0.14))'
                    : 'none',
                display: 'flex',
                alignItems: 'flex-start',
                gap: 18,
              }}
            >
              <div
                style={{
                  color: 'var(--slide-accent, #DCC58E)',
                  opacity: 0.9,
                  flexShrink: 0,
                  marginTop: 3,
                }}
              >
                {item.icon}
              </div>

              <div>
                <h3
                  style={{
                    fontFamily: "'Lora', Georgia, serif",
                    fontSize: 18,
                    fontWeight: 600,
                    color: 'var(--landing-text, #F4EEDC)',
                    margin: '0 0 10px 0',
                    lineHeight: 1.35,
                  }}
                >
                  {item.title}
                </h3>
                <p
                  style={{
                    fontSize: 14.5,
                    lineHeight: 1.65,
                    color: 'var(--landing-muted, #C5CCBF)',
                    margin: 0,
                  }}
                >
                  {item.desc}
                </p>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  );
};
