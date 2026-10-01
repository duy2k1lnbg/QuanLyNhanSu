import React from 'react';
import { Card, Typography } from 'antd';
import { useAppTheme } from '../ThemeContext';

const { Title, Text } = Typography;

interface SectionCardProps {
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  icon?: React.ReactNode;
  extra?: React.ReactNode;
  children: React.ReactNode;
  style?: React.CSSProperties;
  bodyStyle?: React.CSSProperties;
  className?: string;
  bordered?: boolean;
}

export const SectionCard: React.FC<SectionCardProps> = ({
  title,
  subtitle,
  icon,
  extra,
  children,
  style,
  bodyStyle,
  className,
  bordered = true,
}) => {
  const { tokens, isDark } = useAppTheme();

  return (
    <Card
      className={className}
      bordered={bordered}
      style={{
        borderRadius: 12,
        background: tokens.cardBg,
        borderColor: tokens.borderSubtle,
        boxShadow: isDark
          ? '0 2px 10px rgba(0, 0, 0, 0.3)'
          : '0 1px 3px rgba(15, 23, 42, 0.04), 0 4px 12px rgba(15, 23, 42, 0.02)',
        marginBottom: 20,
        overflow: 'hidden',
        ...style,
      }}
      bodyStyle={{
        padding: '20px 22px',
        ...bodyStyle,
      }}
    >
      {(title || extra) && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: 12,
            marginBottom: 16,
            paddingBottom: 12,
            borderBottom: `1px solid ${tokens.borderSubtle}`,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            {icon && (
              <div
                style={{
                  color: tokens.primary,
                  fontSize: 18,
                  display: 'flex',
                  alignItems: 'center',
                }}
              >
                {icon}
              </div>
            )}
            <div>
              {typeof title === 'string' ? (
                <Title
                  level={4}
                  style={{
                    margin: 0,
                    fontSize: 16,
                    fontWeight: 600,
                    color: tokens.textPrimary,
                  }}
                >
                  {title}
                </Title>
              ) : (
                title
              )}
              {subtitle && (
                <Text style={{ fontSize: 12, color: tokens.textMuted, display: 'block', marginTop: 2 }}>
                  {subtitle}
                </Text>
              )}
            </div>
          </div>

          {extra && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
              {extra}
            </div>
          )}
        </div>
      )}

      {children}
    </Card>
  );
};
