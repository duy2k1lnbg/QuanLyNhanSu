import React from 'react';
import { Button, Tag, Dropdown, Space } from 'antd';
import {
  SafetyCertificateOutlined,
  CloudDownloadOutlined,
  GlobalOutlined,
  DownOutlined,
  CheckOutlined,
  LoginOutlined,
} from '@ant-design/icons';
import type { AppLanguage } from '../../../services/i18n';

interface LandingHeaderProps {
  currentLang: AppLanguage;
  onLanguageChange: (lang: AppLanguage) => void;
  allConfigs: Record<AppLanguage, { name: string; short: string; flag: string }>;
  tLanding: any;
  onOpenDownload: (type?: 'windows' | 'mobile' | 'general') => void;
  onOpenLogin: () => void;
}

export const LandingHeader: React.FC<LandingHeaderProps> = ({
  currentLang,
  onLanguageChange,
  allConfigs,
  tLanding,
  onOpenDownload,
  onOpenLogin,
}) => {
  return (
    <header
      style={{
        height: 'clamp(58px, 8vw, 72px)',
        padding: '0 clamp(12px, 3vw, 36px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        borderBottom: '1px solid rgba(255, 255, 255, 0.08)',
        background: 'rgba(15, 23, 42, 0.88)',
        backdropFilter: 'blur(16px)',
        position: 'sticky',
        top: 0,
        zIndex: 100,
        gap: 12,
        overflow: 'hidden',
      }}
    >
      {/* BRAND LOGO & TITLE */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0, flexShrink: 1 }}>
        <div
          style={{
            width: 38,
            height: 38,
            borderRadius: 10,
            background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: '0 4px 14px rgba(37,99,235,0.45)',
            flexShrink: 0,
          }}
        >
          <SafetyCertificateOutlined style={{ fontSize: 20, color: '#fff' }} />
        </div>
        <div style={{ minWidth: 0 }}>
          <div style={{ fontWeight: 800, fontSize: 16, letterSpacing: '0.5px', color: '#fff', lineHeight: 1.2, whiteSpace: 'nowrap' }}>
            HRMS ENTERPRISE
          </div>
          <div
            className="header-hide-tablet"
            style={{
              fontSize: 11,
              color: '#94a3b8',
              whiteSpace: 'nowrap',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              maxWidth: 280,
            }}
          >
            {tLanding.subHeader}
          </div>
        </div>
      </div>

      {/* RIGHT ACTION BUTTONS */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexShrink: 0 }}>
        {/* System Ready Tag: Chỉ hiện trên màn hình rộng > 1200px */}
        <Tag
          className="header-hide-1200"
          color="success"
          style={{
            padding: '3px 10px',
            borderRadius: 12,
            border: 'none',
            background: 'rgba(34,197,94,0.15)',
            color: '#4ade80',
            margin: 0,
            fontWeight: 600,
          }}
        >
          ● {tLanding.systemReady}
        </Tag>

        {/* Gallery Phim: Ẩn trên máy tính bảng và điện thoại < 992px */}
        <Button
          className="header-hide-tablet"
          type="text"
          style={{ color: '#cbd5e1', fontWeight: 500 }}
          onClick={() => {
            const el = document.getElementById('cinematic-hero-gallery');
            if (el) el.scrollIntoView({ behavior: 'smooth' });
          }}
        >
          {tLanding.galleryFilm}
        </Button>

        {/* Tải Ứng Dụng: Ẩn trên điện thoại < 768px (người dùng đã có nút tải to ở Hero) */}
        <Button
          className="header-hide-mobile"
          type="text"
          icon={<CloudDownloadOutlined />}
          onClick={() => onOpenDownload('general')}
          style={{ color: '#cbd5e1', fontWeight: 500 }}
        >
          {tLanding.downloadApp}
        </Button>

        {/* BỘ CHỌN ĐA NGÔN NGỮ: VI / EN / JA (Co giãn tự thích ứng) */}
        <Dropdown
          menu={{
            items: [
              {
                key: 'vi',
                label: (
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 130, padding: '4px 0' }}>
                    <Space size={8}>
                      <span style={{ fontSize: 16 }}>{allConfigs.vi.flag}</span>
                      <span style={{ fontWeight: currentLang === 'vi' ? 700 : 600, color: currentLang === 'vi' ? '#0284c7' : '#0f172a', fontSize: 13 }}>
                        {allConfigs.vi.name}
                      </span>
                    </Space>
                    {currentLang === 'vi' && <CheckOutlined style={{ color: '#0284c7', fontWeight: 700, fontSize: 13 }} />}
                  </div>
                ),
              },
              {
                key: 'en',
                label: (
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 130, padding: '4px 0' }}>
                    <Space size={8}>
                      <span style={{ fontSize: 16 }}>{allConfigs.en.flag}</span>
                      <span style={{ fontWeight: currentLang === 'en' ? 700 : 600, color: currentLang === 'en' ? '#0284c7' : '#0f172a', fontSize: 13 }}>
                        {allConfigs.en.name}
                      </span>
                    </Space>
                    {currentLang === 'en' && <CheckOutlined style={{ color: '#0284c7', fontWeight: 700, fontSize: 13 }} />}
                  </div>
                ),
              },
              {
                key: 'ja',
                label: (
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 130, padding: '4px 0' }}>
                    <Space size={8}>
                      <span style={{ fontSize: 16 }}>{allConfigs.ja.flag}</span>
                      <span style={{ fontWeight: currentLang === 'ja' ? 700 : 600, color: currentLang === 'ja' ? '#0284c7' : '#0f172a', fontSize: 13 }}>
                        {allConfigs.ja.name}
                      </span>
                    </Space>
                    {currentLang === 'ja' && <CheckOutlined style={{ color: '#0284c7', fontWeight: 700, fontSize: 13 }} />}
                  </div>
                ),
              },
            ],
            selectedKeys: [currentLang],
            onClick: ({ key }) => onLanguageChange(key as AppLanguage),
          }}
          placement="bottomRight"
          trigger={['click']}
        >
          <Button
            type="text"
            style={{
              color: '#f8fafc',
              fontWeight: 600,
              fontSize: 13,
              background: 'rgba(255, 255, 255, 0.08)',
              backdropFilter: 'blur(12px)',
              border: '1px solid rgba(255, 255, 255, 0.18)',
              borderRadius: 8,
              padding: '0 8px',
              height: 36,
              display: 'inline-flex',
              alignItems: 'center',
              gap: 4,
              cursor: 'pointer',
            }}
          >
            <GlobalOutlined style={{ color: '#38bdf8', fontSize: 14 }} />
            <span style={{ fontSize: 14 }}>{allConfigs[currentLang].flag}</span>
            <span className="header-hide-compact" style={{ fontSize: 12 }}>{allConfigs[currentLang].short}</span>
            <DownOutlined style={{ fontSize: 9, color: '#94a3b8' }} />
          </Button>
        </Dropdown>

        {/* NÚT ĐĂNG NHẬP NỔI BẬT */}
        <Button
          type="primary"
          icon={<LoginOutlined />}
          onClick={onOpenLogin}
          style={{
            height: 38,
            padding: '0 16px',
            borderRadius: 8,
            fontWeight: 700,
            fontSize: 13,
            background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
            border: 'none',
            boxShadow: '0 4px 14px rgba(37,99,235,0.4)',
            whiteSpace: 'nowrap',
          }}
        >
          {tLanding.signIn}
        </Button>
      </div>
    </header>
  );
};
