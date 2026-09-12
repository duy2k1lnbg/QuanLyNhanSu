import React, { useState } from 'react';
import { message } from 'antd';
import type { CurrentUserDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';
import { CinematicHeroGallery } from '../components/CinematicHeroGallery';
import { LandingHeader } from './login/components/LandingHeader';
import { LandingHero } from './login/components/LandingHero';
import { LandingEcosystem } from './login/components/LandingEcosystem';
import { LandingFeatures } from './login/components/LandingFeatures';
import { LandingArchitecture } from './login/components/LandingArchitecture';
import { LoginModal } from './login/components/LoginModal';
import { DownloadModal } from './login/components/DownloadModal';
import { WINDOWS_PACKAGE_URL } from './login/types';

interface LoginProps {
  onLoginSuccess: (user: CurrentUserDTO, token: string) => void;
}

export const Login: React.FC<LoginProps> = ({ onLoginSuccess }) => {
  const [loginModalVisible, setLoginModalVisible] = useState<boolean>(false);
  const [downloadModalVisible, setDownloadModalVisible] = useState<boolean>(false);
  const [downloadType, setDownloadType] = useState<'windows' | 'mobile' | 'general'>('windows');

  // Hook đa ngôn ngữ toàn hệ thống (Việt, Anh, Nhật)
  const { lang: currentLang, setLang: handleLanguageChange, tLanding, allConfigs } = useAppLanguage();

  const handleDownloadWindows = () => {
    message.loading({ content: 'Đang bắt đầu tải xuống HRMS_Setup_v3.5.0.zip...', key: 'dl_win', duration: 2 });
    const link = document.createElement('a');
    link.href = WINDOWS_PACKAGE_URL;
    link.setAttribute('download', 'HRMS_Setup_v3.5.0.zip');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleOpenDownload = (type: 'windows' | 'mobile' | 'general' = 'general') => {
    setDownloadType(type);
    setDownloadModalVisible(true);
  };

  const handleOpenLogin = () => {
    setLoginModalVisible(true);
  };

  return (
    <div
      style={{
        minHeight: '100vh',
        width: '100%',
        display: 'flex',
        flexDirection: 'column',
        background: 'radial-gradient(ellipse at top, #0f172a 0%, #020617 100%)',
        position: 'relative',
        overflowX: 'hidden',
        color: '#f8fafc',
        fontFamily: "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
      }}
    >
      {/* Background ambient glowing circles */}
      <div
        style={{
          position: 'absolute',
          width: 700,
          height: 700,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(37,99,235,0.18) 0%, rgba(0,0,0,0) 70%)',
          top: '-15%',
          left: '10%',
          pointerEvents: 'none',
        }}
      />
      <div
        style={{
          position: 'absolute',
          width: 600,
          height: 600,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(168,85,247,0.16) 0%, rgba(0,0,0,0) 70%)',
          top: '30%',
          right: '5%',
          pointerEvents: 'none',
        }}
      />

      {/* TOP NAVIGATION BAR */}
      <LandingHeader
        currentLang={currentLang}
        onLanguageChange={handleLanguageChange}
        allConfigs={allConfigs}
        tLanding={tLanding}
        onOpenDownload={handleOpenDownload}
        onOpenLogin={handleOpenLogin}
      />

      {/* 1. CINEMATIC 9-IMAGE HERO HORIZONTAL GALLERY (9 CÂU GIỮ NGUYÊN 100%) */}
      <div id="cinematic-hero-gallery">
        <CinematicHeroGallery />
      </div>

      {/* MAIN CONTENT BODY */}
      <main style={{ flex: 1, maxWidth: 1280, width: '100%', margin: '0 auto', padding: 'clamp(30px, 5vw, 60px) 24px' }}>
        {/* HERO BANNER & 4 CAPABILITIES */}
        <LandingHero
          tLanding={tLanding}
          onOpenLogin={handleOpenLogin}
          onOpenDownload={handleOpenDownload}
        />

        {/* SECTION 2: HỆ SINH THÁI TẢI VỀ */}
        <LandingEcosystem
          tLanding={tLanding}
          onDownloadWindows={handleDownloadWindows}
          onOpenLogin={handleOpenLogin}
          onOpenDownload={handleOpenDownload}
        />

        {/* SECTION 3: TÍNH NĂNG NỔI BẬT */}
        <LandingFeatures tLanding={tLanding} />

        {/* SECTION 4 & 5: THÔNG TIN KIẾN TRÚC & NHÀ PHÁT TRIỂN NGUYỄN THỌ DUY */}
        <LandingArchitecture
          tLanding={tLanding}
          onOpenLogin={handleOpenLogin}
        />
      </main>

      {/* FOOTER */}
      <footer
        style={{
          padding: '24px',
          textAlign: 'center',
          fontSize: 13,
          color: '#64748b',
          borderTop: '1px solid rgba(255, 255, 255, 0.08)',
          background: 'rgba(15, 23, 42, 0.95)',
        }}
      >
        <div>{tLanding.footerLine1}</div>
        <div style={{ fontSize: 11.5, color: '#475569', marginTop: 4 }}>
          {tLanding.footerLine2}
        </div>
      </footer>

      {/* MODAL ĐĂNG NHẬP */}
      <LoginModal
        visible={loginModalVisible}
        onClose={() => setLoginModalVisible(false)}
        onLoginSuccess={onLoginSuccess}
        tLanding={tLanding}
      />

      {/* MODAL TẢI BỘ CÀI ĐẶT */}
      <DownloadModal
        visible={downloadModalVisible}
        onClose={() => setDownloadModalVisible(false)}
        downloadType={downloadType}
        onDownloadWindows={handleDownloadWindows}
        tLanding={tLanding}
      />
    </div>
  );
};

export default Login;
