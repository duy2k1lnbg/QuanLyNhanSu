import React from 'react';
import { useAppTheme } from '../ThemeContext';

export type BadgeStatusType =
  | 'success'
  | 'warning'
  | 'danger'
  | 'error'
  | 'info'
  | 'violet'
  | 'teal'
  | 'neutral';

interface StatusBadgeProps {
  status: BadgeStatusType;
  text: React.ReactNode;
  icon?: React.ReactNode;
  showDot?: boolean;
  style?: React.CSSProperties;
  className?: string;
  size?: 'small' | 'middle';
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({
  status,
  text,
  icon,
  showDot = true,
  style,
  className,
  size = 'middle',
}) => {
  const { tokens, isDark } = useAppTheme();

  const normalizedStatus = status === 'error' ? 'danger' : status;

  const styleMap = {
    success: {
      bg: tokens.successBg,
      color: tokens.successText,
      dot: isDark ? '#68D5A1' : '#10B981',
      border: isDark ? 'rgba(104, 213, 161, 0.3)' : 'rgba(16, 185, 129, 0.25)',
    },
    warning: {
      bg: tokens.warningBg,
      color: tokens.warningText,
      dot: isDark ? '#F2BB62' : '#F59E0B',
      border: isDark ? 'rgba(242, 187, 98, 0.3)' : 'rgba(245, 158, 11, 0.25)',
    },
    danger: {
      bg: tokens.dangerBg,
      color: tokens.dangerText,
      dot: isDark ? '#FF8C9E' : '#EF4444',
      border: isDark ? 'rgba(255, 140, 158, 0.3)' : 'rgba(239, 68, 68, 0.25)',
    },
    info: {
      bg: tokens.infoBg,
      color: tokens.infoText,
      dot: isDark ? '#8AB9FF' : '#3B82F6',
      border: isDark ? 'rgba(138, 185, 255, 0.3)' : 'rgba(59, 130, 246, 0.25)',
    },
    violet: {
      bg: tokens.violetBg,
      color: tokens.violetText,
      dot: isDark ? '#B398FF' : '#6D4AFF',
      border: isDark ? 'rgba(179, 152, 255, 0.3)' : 'rgba(109, 74, 255, 0.25)',
    },
    teal: {
      bg: tokens.tealBg,
      color: tokens.tealText,
      dot: isDark ? '#2DD4BF' : '#14B8A6',
      border: isDark ? 'rgba(45, 212, 191, 0.3)' : 'rgba(20, 184, 166, 0.25)',
    },
    neutral: {
      bg: tokens.cardSecondaryBg,
      color: tokens.textSecondary,
      dot: tokens.textMuted,
      border: tokens.borderSubtle,
    },
  }[normalizedStatus];

  const isSmall = size === 'small';

  return (
    <span
      className={className}
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 5,
        padding: isSmall ? '1px 7px' : '3px 10px',
        borderRadius: 9999,
        background: styleMap.bg,
        color: styleMap.color,
        border: `1px solid ${styleMap.border}`,
        fontSize: isSmall ? 11 : 12,
        fontWeight: 600,
        lineHeight: 1.4,
        whiteSpace: 'nowrap',
        ...style,
      }}
    >
      {showDot && !icon && (
        <span
          style={{
            width: isSmall ? 5 : 6,
            height: isSmall ? 5 : 6,
            borderRadius: '50%',
            background: styleMap.dot,
            flexShrink: 0,
          }}
        />
      )}
      {icon && <span style={{ display: 'inline-flex', fontSize: isSmall ? 11 : 13 }}>{icon}</span>}
      <span>{text}</span>
    </span>
  );
};
