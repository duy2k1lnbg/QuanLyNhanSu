import React from 'react';
import { Button } from 'antd';
import { WindowsOutlined, CloudDownloadOutlined, MobileOutlined, GlobalOutlined } from '@ant-design/icons';
import { LandingModal } from './LandingModal';
import { WINDOWS_PACKAGE_URL, MOBILE_APK_URL } from '../types';

interface DownloadModalProps {
  visible: boolean;
  onClose: () => void;
  downloadType: 'windows' | 'mobile' | 'general';
  onDownloadWindows: () => void;
  onDownloadMobile: () => void;
  tLanding: any;
}

export const DownloadModal: React.FC<DownloadModalProps> = ({
  visible, onClose, downloadType, onDownloadWindows, onDownloadMobile, tLanding,
}) => {
  const description = downloadType === 'windows' ? tLanding.downloadWinAlertDesc
    : downloadType === 'mobile' ? tLanding.downloadMobileAlertDesc : tLanding.downloadGeneralAlertDesc;
  return (
    <LandingModal open={visible} onCancel={onClose} footer={null} width={740}
      className="landing-download-modal"
      title={
        <div className="landing-modal-visual">
          <span className="landing-modal-kicker">TRYHARDAGAIN · HRMS ENTERPRISE</span>
          <h2>{tLanding.downloadModalHeader}</h2>
          <p>{description}</p>
        </div>
      }>
      <div className="landing-download-grid">
        <article className={'landing-download-card' + (downloadType === 'windows' ? ' is-selected' : '')}>
          <div className="landing-download-card-top">
            <WindowsOutlined className="landing-download-icon" />
            <span className="landing-download-status">v3.5.0 · Windows</span>
          </div>
          <h3>{tLanding.downloadWinSectionTitle}</h3>
          <p>{tLanding.downloadWinSectionDesc}</p>
          <span className="landing-download-file">{WINDOWS_PACKAGE_URL.split('/').at(-1)}</span>
          <Button type="primary" icon={<CloudDownloadOutlined />} onClick={onDownloadWindows}>
            {tLanding.downloadWinBtnText}
          </Button>
        </article>
        <article className={'landing-download-card' + (downloadType === 'mobile' ? ' is-selected' : '')}>
          <div className="landing-download-card-top">
            <MobileOutlined className="landing-download-icon" />
            <span className="landing-download-status">APK · Android</span>
          </div>
          <h3>{tLanding.downloadMobileSectionTitle}</h3>
          <p>{tLanding.downloadMobileSectionDesc}</p>
          <span className="landing-download-file">{MOBILE_APK_URL.split('/').at(-1)}</span>
          <Button type="primary" icon={<CloudDownloadOutlined />} onClick={onDownloadMobile}>
            {tLanding.downloadMobileBtnText}
          </Button>
        </article>
      </div>
      <div className="landing-modal-note">
        <GlobalOutlined />
        <span>{tLanding.downloadMobileWebNote}{' '}
          <a href="https://tryhardagain.com" target="_blank" rel="noopener noreferrer">tryhardagain.com</a>
        </span>
      </div>
      <div className="landing-download-footer">
        <Button onClick={onClose}>{tLanding.downloadModalUnderstood}</Button>
      </div>
    </LandingModal>
  );
};
