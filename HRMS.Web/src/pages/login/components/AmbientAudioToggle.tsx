import React from 'react';
import { CustomerServiceOutlined } from '@ant-design/icons';
import { Tooltip } from 'antd';
import { useAppLanguage } from '../../../services/i18n';

interface AmbientAudioToggleProps {
  isPlaying: boolean;
  onToggle: () => void;
  labelTooltip?: string;
}

export const AmbientAudioToggle: React.FC<AmbientAudioToggleProps> = ({
  isPlaying,
  onToggle,
  labelTooltip = 'Âm thanh tự nhiên (Rừng xanh)',
}) => {
  const { tLanding } = useAppLanguage();
  return (
    <Tooltip title={labelTooltip}>
      <button
        type="button"
        onClick={onToggle}
        aria-pressed={isPlaying}
        aria-label={isPlaying ? tLanding.ambientAudioOff : tLanding.ambientAudioOn}
        className={`ambient-audio-toggle ${isPlaying ? 'is-active' : ''}`}
        style={{
          background: isPlaying ? 'rgba(220, 197, 142, 0.18)' : 'rgba(255, 255, 255, 0.06)',
          border: isPlaying ? '1px solid #DCC58E' : '1px solid rgba(255, 255, 255, 0.14)',
          borderRadius: '50%',
          width: 34,
          height: 34,
          display: 'inline-flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: isPlaying ? '#F4EEDC' : 'rgba(244, 238, 220, 0.7)',
          cursor: 'pointer',
          transition: 'all 0.3s ease',
          padding: 0,
        }}
      >
        <CustomerServiceOutlined style={{ fontSize: 15 }} />
      </button>
    </Tooltip>
  );
};
