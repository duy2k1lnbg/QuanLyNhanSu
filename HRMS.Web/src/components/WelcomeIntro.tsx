import React from 'react';
import { Button } from 'antd';
import { CloseOutlined, ThunderboltFilled } from '@ant-design/icons';
import { useAppTheme } from '../theme/ThemeContext';
import { useAppLanguage } from '../services/i18n';
import { useWelcomeIntro } from '../hooks/useWelcomeIntro';
import { AUTHOR_INFO } from '../pages/login/types';
import './WelcomeIntro.css';

const LETTERS = ['W', 'E', 'L', 'C', 'O', 'M', 'E'];

export const WelcomeIntro: React.FC = () => {
  const { isDark } = useAppTheme();
  const { dict } = useAppLanguage();
  const { introState, isIntroActive, skipIntro } = useWelcomeIntro();

  if (!isIntroActive) {
    return null;
  }

  const isExiting = introState === 'exiting';

  const handleContainerPointerDown = (e: React.PointerEvent<HTMLDivElement>) => {
    // If click originated from skip button, it already handles it
    if ((e.target as HTMLElement).closest('.welcome-skip-btn')) {
      return;
    }
    e.preventDefault();
    e.stopPropagation();
    skipIntro();
  };

  return (
    <div
      className={`welcome-intro-overlay welcome-forest ${isDark ? 'theme-dark' : 'theme-light'} ${
        isExiting ? 'exiting' : 'entering'
      }`}
      role="dialog"
      aria-label={dict.intro.toHrms}
      aria-modal="true"
      onPointerDown={handleContainerPointerDown}
    >
      <img className="welcome-forest-background" src="/images/landing/project-forest.jpg" alt="" aria-hidden="true" />
      <div className="welcome-forest-scrim" aria-hidden="true" />
      {/* Existing letter/particle animation retained over the forest photograph. */}
      <div className="welcome-cyber-grid" aria-hidden="true" />

      {/* Aurora Animated Mesh Gradient Orbs */}
      <div className="welcome-aurora-mesh" aria-hidden="true">
        <div className="aurora-orb orb-cyan" />
        <div className="aurora-orb orb-purple" />
        <div className="aurora-orb orb-pink" />
      </div>

      {/* Floating Glowing Cyber Spark Particles */}
      <div className="welcome-particles-layer" aria-hidden="true">
        <span className="cyber-spark spark-1" />
        <span className="cyber-spark spark-2" />
        <span className="cyber-spark spark-3" />
        <span className="cyber-spark spark-4" />
      </div>

      {/* Screen Reader Only Announcement (English) */}
      <h1 className="welcome-sr-only">
        {dict.intro.toHrms} — {dict.intro.subtitle}
      </h1>

      {/* Top-Right Modern Glassmorphism Skip Button */}
      <Button
        className="welcome-skip-btn"
        size="middle"
        onClick={(e) => {
          e.stopPropagation();
          skipIntro();
        }}
        icon={<CloseOutlined style={{ fontSize: 13 }} />}
        aria-label={dict.intro.skip}
      >
        <span style={{ fontWeight: 600 }}>{dict.intro.skip}</span>
        <span className="welcome-skip-shortcut">{dict.intro.skipShortcut}</span>
      </Button>

      {/* Center Cinematic Container */}
      <div className="welcome-content-container">
        <div className="welcome-author-portrait">
          <img src={AUTHOR_INFO.avatarUrl} alt={dict.landing.aboutAuthorName} />
          <span>{dict.landing.aboutAuthorName} · TryHardAgain</span>
        </div>
        {/* Animated Holographic WELCOME Letters */}
        <div className="welcome-word-row" aria-hidden="true">
          {LETTERS.map((char, index) => (
            <span
              key={`${char}-${index}`}
              className={`welcome-letter welcome-letter-${index}`}
            >
              {char}
            </span>
          ))}

          {/* Shimmer Light Beam that sweeps across the assembled letters */}
          <div className="welcome-shimmer-sweep" />
        </div>

        {/* Cyber Neon Horizon Laser Beam */}
        <div className="welcome-laser-line" aria-hidden="true" />

        {/* Brand Identity / TO HRMS Row with Holographic Badge */}
        <div className="welcome-brand-badge">
          <div className="welcome-logo-icon">
            <ThunderboltFilled style={{ fontSize: 20, color: '#ffffff' }} />
          </div>
          <span className="welcome-brand-text">{dict.intro.toHrms}</span>
          <span className="welcome-brand-tag">NEXT-GEN HCM</span>
        </div>

        {/* Enterprise English Subtitle */}
        <div className="welcome-subtitle">
          {dict.intro.subtitle}
        </div>
      </div>

      {/* Bottom Hint / Tap Prompt */}
      <div className="welcome-bottom-hint">
        <span className="welcome-hint-dot" aria-hidden="true">
          <span className="welcome-hint-ping" />
        </span>
        <span className="welcome-hint-text">
          {dict.intro.hint}
        </span>
      </div>
    </div>
  );
};
