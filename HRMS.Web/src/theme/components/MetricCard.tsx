import React from 'react';
import { Card, Typography } from 'antd';
import { ArrowUpOutlined, ArrowDownOutlined } from '@ant-design/icons';
import { useAppTheme } from '../ThemeContext';

const { Text } = Typography;

export type MetricAccent = 'purple' | 'blue' | 'green' | 'orange' | 'teal' | 'amber' | 'cyan';

interface MetricCardProps {
  title: string;
  value: React.ReactNode;
  icon?: React.ReactNode;
  accent?: MetricAccent;
  change?: string | number;
  changeType?: 'positive' | 'negative' | 'neutral';
  changeText?: string;
  extra?: React.ReactNode;
  subtext?: React.ReactNode;
  onClick?: () => void;
  style?: React.CSSProperties;
  className?: string;
}

export const MetricCard: React.FC<MetricCardProps> = ({
  title,
  value,
  icon,
  accent = 'purple',
  change,
  changeType = 'positive',
  changeText,
  extra,
  subtext,
  onClick,
  style,
  className,
}) => {
  const { tokens, isDark } = useAppTheme();

  // Color schemes for icon badge based on accent
  const accentStyles = {
    purple: {
      bg: isDark ? 'rgba(154, 121, 255, 0.16)' : '#F3F0FF',
      color: isDark ? '#B398FF' : '#6D4AFF',
      border: isDark ? 'rgba(154, 121, 255, 0.25)' : '#E9E3FF',
    },
    blue: {
      bg: isDark ? 'rgba(59, 130, 246, 0.16)' : '#EFF6FF',
      color: isDark ? '#60A5FA' : '#2563EB',
      border: isDark ? 'rgba(59, 130, 246, 0.25)' : '#DBEAFE',
    },
    green: {
      bg: isDark ? 'rgba(16, 185, 129, 0.16)' : '#ECFDF5',
      color: isDark ? '#34D399' : '#059669',
      border: isDark ? 'rgba(16, 185, 129, 0.25)' : '#D1FAE5',
    },
    orange: {
      bg: isDark ? 'rgba(245, 158, 11, 0.16)' : '#FFFBEB',
      color: isDark ? '#FBBF24' : '#D97706',
      border: isDark ? 'rgba(245, 158, 11, 0.25)' : '#FEF3C7',
    },
    teal: {
      bg: isDark ? 'rgba(20, 184, 166, 0.16)' : '#F0FDFA',
      color: isDark ? '#2DD4BF' : '#0D9488',
      border: isDark ? 'rgba(20, 184, 166, 0.25)' : '#CCFBF1',
    },
    amber: {
      bg: isDark ? 'rgba(245, 158, 11, 0.16)' : '#FFFBEB',
      color: isDark ? '#FBBF24' : '#D97706',
      border: isDark ? 'rgba(245, 158, 11, 0.25)' : '#FEF3C7',
    },
    cyan: {
      bg: isDark ? 'rgba(6, 182, 212, 0.16)' : '#ECFEFF',
      color: isDark ? '#22D3EE' : '#0891B2',
      border: isDark ? 'rgba(6, 182, 212, 0.25)' : '#CFFAFE',
    },
  }[accent];

  return (
    <Card
      onClick={onClick}
      className={className}
      hoverable={Boolean(onClick)}
      style={{
        borderRadius: 12,
        background: tokens.cardBg,
        borderColor: tokens.borderSubtle,
        boxShadow: isDark
          ? '0 2px 8px rgba(0, 0, 0, 0.35)'
          : '0 1px 3px rgba(15, 23, 42, 0.04), 0 4px 12px rgba(15, 23, 42, 0.03)',
        transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
        cursor: onClick ? 'pointer' : 'default',
        overflow: 'hidden',
        ...style,
      }}
      bodyStyle={{
        padding: '18px 20px',
        display: 'flex',
        flexDirection: 'column',
        gap: 12,
      }}
    >
      {/* Top Header Row */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          {icon && (
            <div
              style={{
                width: 38,
                height: 38,
                borderRadius: 10,
                background: accentStyles.bg,
                border: `1px solid ${accentStyles.border}`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: accentStyles.color,
                fontSize: 18,
                flexShrink: 0,
              }}
            >
              {icon}
            </div>
          )}
          <Text
            style={{
              fontSize: 13,
              fontWeight: 600,
              color: tokens.textSecondary,
              letterSpacing: '0.1px',
            }}
          >
            {title}
          </Text>
        </div>
        {extra && <div>{extra}</div>}
      </div>

      {/* Center Value */}
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 8 }}>
        <span
          style={{
            fontSize: 28,
            fontWeight: 700,
            lineHeight: 1.15,
            color: tokens.textPrimary,
            fontVariantNumeric: 'tabular-nums',
            fontFeatureSettings: '"tnum"',
            letterSpacing: '-0.5px',
          }}
        >
          {value}
        </span>
      </div>

      {/* Bottom Trend / Subtext Row */}
      {(change !== undefined || subtext) && (
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, flexWrap: 'wrap' }}>
          {change !== undefined && (
            <span
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 3,
                fontWeight: 600,
                color:
                  changeType === 'positive'
                    ? tokens.successText
                    : changeType === 'negative'
                    ? tokens.dangerText
                    : tokens.textSecondary,
                background:
                  changeType === 'positive'
                    ? tokens.successBg
                    : changeType === 'negative'
                    ? tokens.dangerBg
                    : tokens.cardSecondaryBg,
                padding: '2px 7px',
                borderRadius: 6,
                fontSize: 11,
              }}
            >
              {changeType === 'positive' && <ArrowUpOutlined style={{ fontSize: 10 }} />}
              {changeType === 'negative' && <ArrowDownOutlined style={{ fontSize: 10 }} />}
              <span>{change}</span>
            </span>
          )}
          {changeText && (
            <Text style={{ color: tokens.textMuted, fontSize: 12 }}>
              {changeText}
            </Text>
          )}
          {subtext && !changeText && (
            <Text style={{ color: tokens.textSecondary, fontSize: 12 }}>
              {subtext}
            </Text>
          )}
        </div>
      )}
    </Card>
  );
};
