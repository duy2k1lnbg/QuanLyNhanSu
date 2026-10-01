import React from 'react';
import { useScrollReveal, type UseScrollRevealOptions } from '../hooks/useScrollReveal';

export type RevealDirection = 'up' | 'left' | 'right' | 'fade';

export interface ScrollRevealProps extends UseScrollRevealOptions {
  children: React.ReactNode;
  direction?: RevealDirection;
  delay?: number; // Milliseconds
  duration?: number; // Milliseconds
  className?: string;
  style?: React.CSSProperties;
  as?: React.ElementType;
  fullWidth?: boolean;
  fullHeight?: boolean;
}

export const ScrollReveal: React.FC<ScrollRevealProps> = ({
  children,
  direction = 'up',
  delay = 0,
  duration = 600,
  threshold = 0.15,
  rootMargin,
  triggerOnce = true,
  className = '',
  style = {},
  as: Component = 'div',
  fullWidth = false,
  fullHeight = false,
}) => {
  const { ref, isRevealed, handleFocus } = useScrollReveal({
    threshold,
    rootMargin,
    triggerOnce,
  });

  const combinedStyle: React.CSSProperties = {
    ...style,
    transitionDuration: `${duration}ms`,
    transitionDelay: isRevealed && delay > 0 ? `${delay}ms` : '0ms',
    ...(fullWidth ? { width: '100%' } : {}),
    ...(fullHeight ? { height: '100%' } : {}),
  };

  return (
    <Component
      ref={ref as any}
      onFocusCapture={handleFocus}
      className={`scroll-reveal-box reveal-${direction} ${isRevealed ? 'is-revealed' : ''} ${className}`}
      style={combinedStyle}
    >
      {children}
    </Component>
  );
};
