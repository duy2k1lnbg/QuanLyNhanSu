import React, { useEffect, useState } from 'react';

export const LandingJourneyLine: React.FC = () => {
  const [scrollProgress, setScrollProgress] = useState<number>(0);

  useEffect(() => {
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) {
      setScrollProgress(1);
      return;
    }

    const handleScroll = () => {
      const totalScroll = document.documentElement.scrollHeight - window.innerHeight;
      if (totalScroll > 0) {
        const current = window.scrollY / totalScroll;
        setScrollProgress(Math.max(0, Math.min(1, current)));
      }
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll();
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <div
      className="landing-journey-line-container"
      aria-hidden="true"
      style={{
        position: 'absolute',
        left: 'clamp(12px, 3.5vw, 40px)',
        top: '680px',
        bottom: '120px',
        width: 16,
        pointerEvents: 'none',
        zIndex: 5,
        opacity: 0.35,
      }}
    >
      <svg
        style={{
          width: '100%',
          height: '100%',
          overflow: 'visible',
        }}
      >
        <line
          x1="8"
          y1="0"
          x2="8"
          y2="100%"
          stroke="rgba(244, 238, 220, 0.15)"
          strokeWidth="1"
          strokeDasharray="4 6"
        />
        <line
          x1="8"
          y1="0"
          x2="8"
          y2={`${scrollProgress * 100}%`}
          stroke="rgba(220, 197, 142, 0.65)"
          strokeWidth="1.5"
          style={{ transition: 'y2 0.2s ease-out' }}
        />
        {/* Seed nodes at milestones (25%, 50%, 75%, 100%) */}
        {[0.25, 0.5, 0.75, 0.98].map((pct, idx) => (
          <circle
            key={idx}
            cx="8"
            cy={`${pct * 100}%`}
            r={scrollProgress >= pct ? 3.5 : 2.5}
            fill={scrollProgress >= pct ? '#DCC58E' : 'rgba(244, 238, 220, 0.3)'}
            style={{
              transition: 'all 0.5s ease',
              filter: scrollProgress >= pct ? 'drop-shadow(0 0 4px #DCC58E)' : 'none',
            }}
          />
        ))}
      </svg>
    </div>
  );
};
