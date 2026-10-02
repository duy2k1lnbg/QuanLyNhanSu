import React, { useState } from 'react';
import { message } from 'antd';
import type { CurrentUserDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import { CinematicHeroGallery } from '../components/CinematicHeroGallery';
import { LandingHeader } from './login/components/LandingHeader';
import { LandingAbout } from './login/components/LandingAbout';
import { LandingFocus } from './login/components/LandingFocus';
import { LandingPlatforms } from './login/components/LandingPlatforms';
import { LandingProject } from './login/components/LandingProject';
import { LandingContact } from './login/components/LandingContact';
import { LandingJourneyLine } from './login/components/LandingJourneyLine';
import { LoginModal } from './login/components/LoginModal';
import { DownloadModal } from './login/components/DownloadModal';
import { WINDOWS_PACKAGE_URL, MOBILE_APK_URL } from './login/types';
import { useAmbientAudio } from './login/hooks/useAmbientAudio';
import './login/LandingPage.css';

interface LoginProps {
  onLoginSuccess: (user: CurrentUserDTO, token: string) => void;
}

export const Login: React.FC<LoginProps> = ({ onLoginSuccess }) => {
  const [loginModalVisible, setLoginModalVisible] = useState<boolean>(false);
  const [downloadModalVisible, setDownloadModalVisible] = useState<boolean>(false);
  const [downloadType, setDownloadType] = useState<'windows' | 'mobile' | 'general'>('windows');

  // Multilingual hook across system
  const { lang: currentLang, setLang: handleLanguageChange, tLanding, tAuth, allConfigs } = useAppLanguage();

  // Natural calm ambient audio hook (woodland atmosphere)
  const { isPlaying: isAudioPlaying, toggleAudio } = useAmbientAudio();

  const handleDownloadWindows = () => {
    message.loading({ content: 'Đang bắt đầu tải xuống HRMS_Setup_v3.5.0.zip...', key: 'dl_win', duration: 2 });
    const link = document.createElement('a');
    link.href = WINDOWS_PACKAGE_URL;
    link.setAttribute('download', 'HRMS_Setup_v3.5.0.zip');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleDownloadMobile = () => {
    message.loading({ content: 'Đang bắt đầu tải xuống HRMS_Mobile.apk...', key: 'dl_apk', duration: 2 });
    const link = document.createElement('a');
    link.href = MOBILE_APK_URL;
    link.setAttribute('download', 'HRMS_Mobile.apk');
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

  const { theme } = useAppTheme();
  const isAnyModalOpen = loginModalVisible || downloadModalVisible;

  return (
    <div className="tryhard-landing" data-landing-theme={theme}>
      {/* 1. TOP STICKY NAVIGATION HEADER */}
      <LandingHeader
        currentLang={currentLang}
        onLanguageChange={handleLanguageChange}
        allConfigs={allConfigs}
        tLanding={tLanding}
        onOpenDownload={handleOpenDownload}
        onOpenLogin={handleOpenLogin}
        isAudioPlaying={isAudioPlaying}
        onToggleAudio={toggleAudio}
      />

      {/* 2. CINEMATIC 10-SLIDE HERO GALLERY (STARTS AT HERO-05 PATIENCE) */}
      <CinematicHeroGallery isModalOpen={isAnyModalOpen} />

      {/* 3. VERTICAL JOURNEY MILESTONE CONNECTOR */}
      <LandingJourneyLine />

      {/* 4. SECTION: VỀ MÌNH (BIO, SKILLS, MONOGRAM ND) */}
      <LandingAbout tLanding={tLanding} />

      {/* 5. SECTION: NHỮNG ĐIỀU MÌNH ĐANG TẬP TRUNG (3 COLUMNS) */}
      <LandingFocus tLanding={tLanding} />

      <LandingPlatforms tLanding={tLanding} onOpenLogin={handleOpenLogin} onOpenDownload={handleOpenDownload} />

      {/* 6. SECTION: DỰ ÁN ĐANG PHÁT TRIỂN (HRMS, CTAs, MOCKUP) */}
      <LandingProject
        tLanding={tLanding}
        onOpenLogin={handleOpenLogin}
        onOpenDownload={handleOpenDownload}
      />

      {/* 7. SECTION: KẾT NỐI & TRAO ĐỔI + FOOTER */}
      <LandingContact tLanding={tLanding} />

      {/* 8. MODAL ĐĂNG NHẬP (PRESERVED) */}
      <LoginModal
        visible={loginModalVisible}
        onClose={() => setLoginModalVisible(false)}
        onLoginSuccess={onLoginSuccess}
        tLanding={tLanding}
        tAuth={tAuth}
      />

      {/* 9. MODAL TẢI BỘ CÀI ĐẶT (PRESERVED) */}
      <DownloadModal
        visible={downloadModalVisible}
        onClose={() => setDownloadModalVisible(false)}
        downloadType={downloadType}
        onDownloadWindows={handleDownloadWindows}
        onDownloadMobile={handleDownloadMobile}
        tLanding={tLanding}
      />
    </div>
  );
};

export default Login;
