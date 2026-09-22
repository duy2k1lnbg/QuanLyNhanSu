import React from 'react';
import { Modal, Alert, Button, Tag } from 'antd';
import {
  RocketOutlined,
  SafetyCertificateOutlined,
  InfoCircleOutlined,
  WindowsOutlined,
  CloudDownloadOutlined,
  MobileOutlined,
} from '@ant-design/icons';

interface DownloadModalProps {
  visible: boolean;
  onClose: () => void;
  downloadType: 'windows' | 'mobile' | 'general';
  onDownloadWindows: () => void;
  onDownloadMobile: () => void;
  tLanding: any;
}

export const DownloadModal: React.FC<DownloadModalProps> = ({
  visible,
  onClose,
  downloadType,
  onDownloadWindows,
  onDownloadMobile,
  tLanding,
}) => {
  return (
    <Modal
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <RocketOutlined style={{ color: '#2563eb', fontSize: 20 }} />
          <span style={{ fontWeight: 700, fontSize: 16 }}>
            {tLanding.downloadModalHeader}
          </span>
        </div>
      }
      open={visible}
      onOk={onClose}
      onCancel={onClose}
      okText={tLanding.downloadModalUnderstood}
      cancelButtonProps={{ style: { display: 'none' } }}
      width={560}
      centered
    >
      <div style={{ padding: '12px 4px' }}>
        <Alert
          message={
            downloadType === 'windows'
              ? tLanding.downloadWinAlertTitle
              : downloadType === 'mobile'
              ? tLanding.downloadMobileAlertTitle
              : tLanding.downloadGeneralAlertTitle
          }
          description={
            downloadType === 'windows'
              ? tLanding.downloadWinAlertDesc
              : downloadType === 'mobile'
              ? tLanding.downloadMobileAlertDesc
              : tLanding.downloadGeneralAlertDesc
          }
          type={downloadType === 'windows' ? 'success' : 'info'}
          showIcon
          icon={downloadType === 'windows' ? <SafetyCertificateOutlined style={{ color: '#16a34a' }} /> : <InfoCircleOutlined style={{ color: '#2563eb' }} />}
          style={{
            marginBottom: 20,
            borderRadius: 10,
            background: downloadType === 'windows' ? '#f0fdf4' : '#eff6ff',
            borderColor: downloadType === 'windows' ? '#bbf7d0' : '#bfdbfe',
          }}
        />

        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div
            style={{
              display: 'flex',
              gap: 14,
              padding: '16px',
              borderRadius: 12,
              background: '#f8fafc',
              border: '1px solid #e2e8f0',
              alignItems: 'flex-start',
            }}
          >
            <WindowsOutlined style={{ fontSize: 32, color: '#2563eb', marginTop: 2 }} />
            <div style={{ flex: 1 }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8, flexWrap: 'wrap' }}>
                <span style={{ fontWeight: 700, fontSize: 15, color: '#1e293b' }}>
                  {tLanding.downloadWinSectionTitle}
                </span>
                <Tag color="success" style={{ fontWeight: 600 }}>{tLanding.winTagReady}</Tag>
              </div>
              <div style={{ fontSize: 13, color: '#475569', marginTop: 6, lineHeight: 1.5 }}>
                {tLanding.downloadWinSectionDesc}
              </div>
              <div style={{ marginTop: 12 }}>
                <Button
                  type="primary"
                  icon={<CloudDownloadOutlined />}
                  onClick={onDownloadWindows}
                  style={{
                    background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                    border: 'none',
                    borderRadius: 8,
                    fontWeight: 600,
                  }}
                >
                  {tLanding.downloadWinBtnText}
                </Button>
              </div>
            </div>
          </div>

          <div
            style={{
              display: 'flex',
              gap: 14,
              padding: '16px',
              borderRadius: 12,
              background: '#f8fafc',
              border: '1px solid #e2e8f0',
              alignItems: 'flex-start',
            }}
          >
            <MobileOutlined style={{ fontSize: 32, color: '#a855f7', marginTop: 2 }} />
            <div style={{ flex: 1 }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8, flexWrap: 'wrap' }}>
                <span style={{ fontWeight: 700, fontSize: 15, color: '#1e293b' }}>
                  {tLanding.downloadMobileSectionTitle}
                </span>
                <Tag color="success" style={{ fontWeight: 600 }}>{tLanding.mobileTagComing || 'APK Ready'}</Tag>
              </div>
              <div style={{ fontSize: 13, color: '#475569', marginTop: 6, lineHeight: 1.5 }}>
                {tLanding.downloadMobileSectionDesc}
              </div>
              <div style={{ marginTop: 12 }}>
                <Button
                  type="primary"
                  icon={<CloudDownloadOutlined />}
                  onClick={onDownloadMobile}
                  style={{
                    background: 'linear-gradient(135deg, #9333ea 0%, #7e22ce 100%)',
                    border: 'none',
                    borderRadius: 8,
                    fontWeight: 600,
                  }}
                >
                  {tLanding.downloadMobileBtnText}
                </Button>
              </div>
              <div style={{ marginTop: 10, fontSize: 12, color: '#64748b', lineHeight: 1.6 }}>
                <span>{tLanding.downloadMobileWebNote} </span>
                <Tag color="blue" style={{ fontSize: 12, fontWeight: 600, margin: '2px 4px' }}>
                  https://tryhardagain.com
                </Tag>
                <span>{tLanding.downloadMobileResponsiveNote}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Modal>
  );
};
