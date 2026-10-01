import React from 'react';
import { useAppTheme } from '../ThemeContext';

interface PageToolbarProps {
  left?: React.ReactNode;
  right?: React.ReactNode;
  children?: React.ReactNode;
  style?: React.CSSProperties;
  className?: string;
}

export const PageToolbar: React.FC<PageToolbarProps> = ({
  left,
  right,
  children,
  style,
  className,
}) => {
  const { tokens } = useAppTheme();

  return (
    <div
      className={className}
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        flexWrap: 'wrap',
        gap: 12,
        marginBottom: 16,
        padding: '12px 16px',
        borderRadius: 10,
        background: tokens.cardSecondaryBg,
        border: `1px solid ${tokens.borderSubtle}`,
        ...style,
      }}
    >
      {left && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            flexWrap: 'wrap',
            flex: '1 1 auto',
            minWidth: 0,
          }}
        >
          {left}
        </div>
      )}

      {children && !left && !right && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            flexWrap: 'wrap',
            width: '100%',
          }}
        >
          {children}
        </div>
      )}

      {right && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            flexWrap: 'wrap',
            flexShrink: 0,
          }}
        >
          {right}
        </div>
      )}
    </div>
  );
};
