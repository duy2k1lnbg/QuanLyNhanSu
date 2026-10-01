import React from 'react';
import { Breadcrumb, Typography, Space } from 'antd';
import { HomeOutlined } from '@ant-design/icons';
import { useAppTheme } from '../ThemeContext';

const { Title, Text } = Typography;

export interface BreadcrumbItem {
  title: React.ReactNode;
  href?: string;
  onClick?: () => void;
}

interface PageHeaderProps {
  title: React.ReactNode;
  subtitle?: React.ReactNode;
  breadcrumbs?: BreadcrumbItem[];
  extra?: React.ReactNode;
  badge?: React.ReactNode;
  onHomeClick?: () => void;
  style?: React.CSSProperties;
}

export const PageHeader: React.FC<PageHeaderProps> = ({
  title,
  subtitle,
  breadcrumbs = [],
  extra,
  badge,
  onHomeClick,
  style,
}) => {
  const { tokens } = useAppTheme();

  const breadcrumbItems = [
    {
      title: (
        <span
          onClick={onHomeClick}
          style={{ cursor: onHomeClick ? 'pointer' : 'default', display: 'inline-flex', alignItems: 'center', gap: 4 }}
        >
          <HomeOutlined style={{ fontSize: 13 }} />
          <span>Trang chủ</span>
        </span>
      ),
    },
    ...breadcrumbs.map((b) => ({
      title: b.onClick ? (
        <span onClick={b.onClick} style={{ cursor: 'pointer' }}>
          {b.title}
        </span>
      ) : (
        b.title
      ),
    })),
  ];

  return (
    <div
      style={{
        marginBottom: 20,
        display: 'flex',
        flexDirection: 'column',
        gap: 8,
        ...style,
      }}
    >
      {breadcrumbs.length > 0 && (
        <Breadcrumb
          items={breadcrumbItems}
          style={{
            fontSize: 12,
            color: tokens.textMuted,
          }}
        />
      )}

      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: 12,
        }}
      >
        <div>
          <Space align="center" size={8}>
            <Title
              level={3}
              style={{
                margin: 0,
                color: tokens.textPrimary,
                fontSize: 22,
                fontWeight: 700,
                letterSpacing: '-0.3px',
              }}
            >
              {title}
            </Title>
            {badge}
          </Space>
          {subtitle && (
            <div style={{ marginTop: 4 }}>
              <Text style={{ color: tokens.textSecondary, fontSize: 13 }}>
                {subtitle}
              </Text>
            </div>
          )}
        </div>

        {extra && (
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
            {extra}
          </div>
        )}
      </div>
    </div>
  );
};
