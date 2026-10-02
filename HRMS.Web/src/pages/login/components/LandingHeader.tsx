import React, { useState, useEffect } from 'react';
import { Dropdown, Drawer } from 'antd';
import {
  DownOutlined,
  CheckOutlined,
  MenuOutlined,
  CloseOutlined,
} from '@ant-design/icons';
import type { LocaleType } from '../../../locales/vi';
import type { AppLanguage } from '../../../services/i18n';
import { ThemeToggle } from '../../../theme/components/ThemeToggle';
import { AmbientAudioToggle } from './AmbientAudioToggle';

interface LandingHeaderProps {
  currentLang: AppLanguage;
  onLanguageChange: (lang: AppLanguage) => void;
  allConfigs: Record<AppLanguage, { name: string; short: string; flag: string }>;
  tLanding: LocaleType['landing'];
  onOpenDownload: (type?: 'windows' | 'mobile' | 'general') => void;
  onOpenLogin: () => void;
  isAudioPlaying?: boolean;
  onToggleAudio?: () => void;
}

export const LandingHeader: React.FC<LandingHeaderProps> = ({
  currentLang,
  onLanguageChange,
  allConfigs,
  tLanding,
  onOpenDownload,
  onOpenLogin,
  isAudioPlaying = false,
  onToggleAudio,
}) => {
  const [isScrolled, setIsScrolled] = useState<boolean>(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState<boolean>(false);

  useEffect(() => {
    const handleScroll = () => {
      // The app scrolls inside its content panel as well as the document.
      const heroTop = document.getElementById('cinematic-hero-gallery')?.getBoundingClientRect().top ?? 0;
      setIsScrolled(heroTop < -20);
    };
    handleScroll();
    window.addEventListener('scroll', handleScroll, { passive: true, capture: true });
    document.body.addEventListener('scroll', handleScroll, { passive: true });
    return () => {
      window.removeEventListener('scroll', handleScroll, true);
      document.body.removeEventListener('scroll', handleScroll);
    };
  }, []);

  const scrollToSection = (id: string) => {
    setMobileMenuOpen(false);
    const el = document.getElementById(id);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth' });
    }
  };

  const languageMenuItems = Object.entries(allConfigs).map(([code, config]) => ({
    key: code,
    label: (
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 12,
          padding: '4px 6px',
        }}
      >
        <span style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: 16 }}>{config.flag}</span>
          <span style={{ fontWeight: currentLang === code ? 600 : 400 }}>{config.name}</span>
        </span>
        {currentLang === code && <CheckOutlined style={{ color: '#DCC58E', fontSize: 12 }} />}
      </div>
    ),
    onClick: () => onLanguageChange(code as AppLanguage),
  }));

  return (
    <header
      className={`landing-header-bar ${isScrolled ? 'is-scrolled' : ''}`}
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        height: 'clamp(56px, 6vw, 66px)',
        zIndex: 100,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 clamp(16px, 4vw, 44px)',
        transition: 'background-color 0.35s ease, border-color 0.35s ease, backdrop-filter 0.35s ease',
        backgroundColor: isScrolled ? 'rgba(20, 33, 27, 0.97)' : 'rgba(20, 33, 27, 0.88)',
        backdropFilter: 'blur(16px)',
        borderBottom: '1px solid rgba(220, 197, 142, 0.22)',
        boxShadow: '0 4px 24px rgba(10, 20, 14, 0.18)',
      }}
    >
      {/* BRAND (LEFT) */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 12,
          cursor: 'pointer',
        }}
        onClick={() => scrollToSection('cinematic-hero-gallery')}
      >
        <span
          style={{
            fontFamily: "'Lora', 'Playfair Display', Georgia, serif",
            fontSize: 'clamp(19px, 2.2vw, 23px)',
            fontWeight: 600,
            color: '#F4EEDC',
            letterSpacing: '0.4px',
          }}
        >
          HRMS Enterprise
        </span>

        {/* Small pulsing ready indicator dot */}
        <div
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 6,
            fontSize: 12,
            color: '#D5DFCF',
            fontWeight: 400,
          }}
        >
          <span
            style={{
              width: 7,
              height: 7,
              borderRadius: '50%',
              backgroundColor: '#4ADE80',
              boxShadow: '0 0 8px #4ADE80',
            }}
          />
          <span className="header-status-text" style={{ fontSize: 12 }}>
            {tLanding?.systemReady || 'Hệ thống sẵn sàng'}
          </span>
        </div>
      </div>

      {/* DESKTOP NAV & ACTIONS (RIGHT) */}
      <div
        className="header-desktop-nav"
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 'clamp(12px, 1.8vw, 22px)',
        }}
      >
        {/* NAV LINKS */}
        <button
          type="button"
          onClick={() => scrollToSection('cinematic-hero-gallery')}
          className="header-nav-link"
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 14,
            fontWeight: 500,
            cursor: 'pointer',
            padding: '4px 6px',
            transition: 'color 0.25s',
          }}
        >
          {tLanding?.galleryFilm || 'Gallery Phim'}
        </button>

        <button
          type="button"
          onClick={() => scrollToSection('about-me')}
          className="header-nav-link"
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 14,
            fontWeight: 500,
            cursor: 'pointer',
            padding: '4px 6px',
            transition: 'color 0.25s',
          }}
        >
          {tLanding?.navAbout || 'Về mình'}
        </button>

        <button
          type="button"
          onClick={() => onOpenDownload('general')}
          className="header-nav-link"
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 14,
            fontWeight: 500,
            cursor: 'pointer',
            padding: '4px 6px',
            transition: 'color 0.25s',
          }}
        >
          {tLanding?.downloadApp || 'Tải ứng dụng'}
        </button>

        {/* LANGUAGE SELECTOR */}
        <Dropdown menu={{ items: languageMenuItems }} trigger={['click']} placement="bottomRight">
          <button
            type="button"
            className="header-lang-btn"
            style={{
              background: 'none',
              border: 'none',
              color: '#F4EEDC',
              fontSize: 13.5,
              fontWeight: 600,
              cursor: 'pointer',
              display: 'inline-flex',
              alignItems: 'center',
              gap: 4,
              padding: '4px 8px',
            }}
          >
            <span>{allConfigs[currentLang]?.short || 'VI'}</span>
            <DownOutlined style={{ fontSize: 10, opacity: 0.7 }} />
          </button>
        </Dropdown>

        {/* VERTICAL DIVIDER */}
        <span
          style={{
            width: 1,
            height: 16,
            backgroundColor: 'rgba(244, 238, 220, 0.25)',
          }}
        />

        {/* THEME TOGGLE */}
        <ThemeToggle className="landing-theme-toggle" labels={{ light: tLanding.themeLight, dark: tLanding.themeDark }} />

        {/* AMBIENT AUDIO TOGGLE */}
        {onToggleAudio && (
          <AmbientAudioToggle
            isPlaying={isAudioPlaying}
            onToggle={onToggleAudio}
            labelTooltip={tLanding?.ambientAudio || 'Âm thanh tự nhiên (Rừng xanh)'}
          />
        )}

        {/* LOGIN BUTTON (OLIVE/SAGE PILL AS IN TARGET DESIGN) */}
        <button
          type="button"
          onClick={onOpenLogin}
          className="btn-header-login"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 6,
            padding: '7px 18px',
            borderRadius: 20,
            backgroundColor: 'rgba(56, 77, 63, 0.75)',
            border: '1px solid rgba(135, 170, 145, 0.35)',
            backdropFilter: 'blur(8px)',
            color: '#F4EEDC',
            fontWeight: 500,
            fontSize: 13.5,
            cursor: 'pointer',
            transition: 'all 0.3s ease',
          }}
        >
          <span>{tLanding?.signIn || 'Đăng nhập'}</span>
        </button>
      </div>

      {/* MOBILE HAMBURGER BUTTON */}
      <div className="header-mobile-trigger" style={{ display: 'none' }}>
        <button
          type="button"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          aria-label={tLanding.navMenu}
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 20,
            cursor: 'pointer',
            padding: 6,
          }}
        >
          {mobileMenuOpen ? <CloseOutlined /> : <MenuOutlined />}
        </button>
      </div>

      {/* MOBILE DRAWER MENU */}
      <Drawer
        open={mobileMenuOpen}
        onClose={() => setMobileMenuOpen(false)}
        placement="right"
        size={280}
        styles={{
          body: {
            backgroundColor: '#14211B',
            color: '#F4EEDC',
            padding: '24px 20px',
            display: 'flex',
            flexDirection: 'column',
            gap: 20,
          },
          header: {
            backgroundColor: '#14211B',
            borderBottom: '1px solid rgba(239, 232, 212, 0.12)',
            color: '#F4EEDC',
          },
        }}
        title={<span style={{ color: '#F4EEDC', fontFamily: "'Lora', serif" }}>{tLanding.navMenu}</span>}
      >
        <button
          type="button"
          onClick={() => scrollToSection('cinematic-hero-gallery')}
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 16,
            textAlign: 'left',
            padding: '8px 0',
            cursor: 'pointer',
          }}
        >
          {tLanding?.galleryFilm || 'Gallery Phim'}
        </button>

        <button
          type="button"
          onClick={() => scrollToSection('about-me')}
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 16,
            textAlign: 'left',
            padding: '8px 0',
            cursor: 'pointer',
          }}
        >
          {tLanding?.navAbout || 'Về mình'}
        </button>

        <button
          type="button"
          onClick={() => {
            setMobileMenuOpen(false);
            onOpenDownload('general');
          }}
          style={{
            background: 'none',
            border: 'none',
            color: '#F4EEDC',
            fontSize: 16,
            textAlign: 'left',
            padding: '8px 0',
            cursor: 'pointer',
          }}
        >
          {tLanding?.downloadApp || 'Tải ứng dụng'}
        </button>

        <div style={{ height: 1, backgroundColor: 'rgba(239, 232, 212, 0.15)', margin: '8px 0' }} />

        {/* Theme and Audio in Mobile */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <span>{tLanding.navAppearance}</span>
          <ThemeToggle className="landing-theme-toggle" labels={{ light: tLanding.themeLight, dark: tLanding.themeDark }} />
        </div>

        {onToggleAudio && (
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <span>{tLanding.ambientAudio}</span>
            <AmbientAudioToggle isPlaying={isAudioPlaying} onToggle={onToggleAudio} />
          </div>
        )}

        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <span>{tLanding.navLanguage}</span>
          <Dropdown menu={{ items: languageMenuItems }} trigger={['click']}>
            <button
              type="button"
              style={{
                background: 'rgba(255, 255, 255, 0.08)',
                border: '1px solid rgba(255, 255, 255, 0.15)',
                borderRadius: 4,
                color: '#F4EEDC',
                padding: '4px 10px',
                cursor: 'pointer',
              }}
            >
              <span>{allConfigs[currentLang]?.name}</span> <DownOutlined style={{ fontSize: 10 }} />
            </button>
          </Dropdown>
        </div>

        <button
          type="button"
          onClick={() => {
            setMobileMenuOpen(false);
            onOpenLogin();
          }}
          style={{
            marginTop: 20,
            padding: '12px',
            borderRadius: 20,
            backgroundColor: '#384D3F',
            border: '1px solid rgba(135, 170, 145, 0.4)',
            color: '#F4EEDC',
            fontWeight: 600,
            fontSize: 15,
            cursor: 'pointer',
          }}
        >
          {tLanding?.signIn || 'Đăng nhập'}
        </button>
      </Drawer>
    </header>
  );
};
