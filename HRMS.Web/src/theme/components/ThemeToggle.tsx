import React from 'react';
import { Button, Tooltip } from 'antd';
import { SunOutlined, MoonOutlined } from '@ant-design/icons';
import { useAppTheme } from '../ThemeContext';

interface ThemeToggleProps {
  mode?: 'light' | 'dark';
  onToggle?: () => void;
  style?: React.CSSProperties;
  className?: string;
  size?: 'small' | 'middle' | 'large';
  labels?: { light: string; dark: string };
}

export const ThemeToggle: React.FC<ThemeToggleProps> = ({
  mode: propMode,
  onToggle: propOnToggle,
  style,
  className,
  size = 'middle',
  labels,
}) => {
  const { mode: contextMode, toggleTheme: contextToggleTheme } = useAppTheme();
  const currentMode = propMode || contextMode;
  const isDark = currentMode === 'dark';
  const handleToggle = propOnToggle || contextToggleTheme;

  const localizedLabel = isDark ? labels?.light : labels?.dark;
  const tooltipTitle = localizedLabel || (isDark ? 'Chuyển sang chế độ Sáng' : 'Chuyển sang chế độ Tối');
  const ariaLabel = localizedLabel || (isDark ? 'Kích hoạt giao diện sáng' : 'Kích hoạt giao diện tối');

  return (
    <Tooltip title={tooltipTitle} placement="bottom">
      <Button
        type="text"
        aria-label={ariaLabel}
        icon={
          isDark ? (
            <SunOutlined style={{ color: '#FBBF24', fontSize: size === 'small' ? 14 : 16 }} />
          ) : (
            <MoonOutlined style={{ color: '#6D4AFF', fontSize: size === 'small' ? 14 : 16 }} />
          )
        }
        onClick={handleToggle}
        className={className}
        style={{
          display: 'inline-flex',
          alignItems: 'center',
          justifyContent: 'center',
          width: size === 'small' ? 32 : 36,
          height: size === 'small' ? 32 : 36,
          borderRadius: 8,
          background: isDark ? 'rgba(255, 255, 255, 0.06)' : 'rgba(109, 74, 255, 0.08)',
          border: isDark ? '1px solid #283148' : '1px solid #E5E8F0',
          transition: 'all 0.2s ease',
          padding: 0,
          ...style,
        }}
      />
    </Tooltip>
  );
};
