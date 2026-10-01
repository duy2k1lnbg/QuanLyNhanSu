import React from 'react';
import { Button } from 'antd';
import { CloseOutlined, ThunderboltFilled } from '@ant-design/icons';
import { useAppTheme } from '../theme/ThemeContext';
import { useWelcomeIntro } from '../hooks/useWelcomeIntro';

const LETTERS = ['W', 'E', 'L', 'C', 'O', 'M', 'E'];

export const WelcomeIntro: React.FC = () => {
  const { isDark } = useAppTheme();
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
      className={`welcome-intro-overlay ${isDark ? 'theme-dark' : 'theme-light'} ${
        isExiting ? 'exiting' : 'entering'
      }`}
      role="dialog"
      aria-label="Welcome to HRMS Enterprise"
      aria-modal="true"
      onPointerDown={handleContainerPointerDown}
    >
      {/* Background Cyber Starfield Texture */}
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
        WELCOME TO HRMS ENTERPRISE - Smart Workforce Management, Biometric Attendance, and Enterprise Payroll
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
        aria-label="Skip Intro"
      >
        <span style={{ fontWeight: 600 }}>Skip</span>
        <span className="welcome-skip-shortcut">Esc / Any key</span>
      </Button>

      {/* Center Cinematic Container */}
      <div className="welcome-content-container">
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
          <span className="welcome-brand-text">TO HRMS ENTERPRISE</span>
          <span className="welcome-brand-tag">NEXT-GEN HCM</span>
        </div>

        {/* Enterprise English Subtitle */}
        <div className="welcome-subtitle">
          Smart Workforce Management · Biometric Attendance · Enterprise Payroll
        </div>
      </div>

      {/* Bottom Hint / Tap Prompt */}
      <div className="welcome-bottom-hint">
        <span className="welcome-hint-dot" aria-hidden="true">
          <span className="welcome-hint-ping" />
        </span>
        <span className="welcome-hint-text">
          Press any key or click anywhere to continue
        </span>
      </div>
    </div>
  );
};
