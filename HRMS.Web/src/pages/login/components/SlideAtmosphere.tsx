import React, { useMemo } from 'react';
import type { SlideEnvironment } from '../data/cinematicSlides';

interface SlideAtmosphereProps {
  environment: SlideEnvironment;
  accent: string;
}

export const SlideAtmosphere: React.FC<SlideAtmosphereProps> = ({ environment, accent }) => {
  // Generate random stable particle values for dust and rain
  const dustParticles = useMemo(() => {
    return Array.from({ length: 14 }).map((_, i) => ({
      id: i,
      left: 45 + Math.random() * 48, // mostly on the right side
      top: 15 + Math.random() * 70,
      size: 1.5 + Math.random() * 2,
      duration: 12 + Math.random() * 8, // "chậm 1 chút" (12s - 20s)
      delay: Math.random() * 6,
      opacity: 0.25 + Math.random() * 0.45,
    }));
  }, []);

  const rainStreaks = useMemo(() => {
    return Array.from({ length: 12 }).map((_, i) => ({
      id: i,
      left: 10 + Math.random() * 85,
      top: -10 + Math.random() * 20,
      length: 24 + Math.random() * 32,
      duration: 2.2 + Math.random() * 1.4, // serene slow rain
      delay: Math.random() * 3,
      opacity: 0.12 + Math.random() * 0.18,
    }));
  }, []);

  return (
    <div
      className="slide-atmosphere-container"
      aria-hidden="true"
      style={{
        position: 'absolute',
        inset: 0,
        pointerEvents: 'none',
        zIndex: 4,
        overflow: 'hidden',
      }}
    >
      {/* 1. DAWN ENVIRONMENT */}
      {environment === 'dawn' && (
        <div
          className="atmosphere-dawn"
          style={{
            position: 'absolute',
            inset: 0,
            background: `radial-gradient(ellipse 65% 55% at 75% 30%, ${accent}33 0%, ${accent}11 45%, transparent 75%)`,
            animation: 'atmosphereBreathe 9s ease-in-out infinite alternate',
          }}
        />
      )}

      {/* 2. LAMP ENVIRONMENT */}
      {environment === 'lamp' && (
        <div
          className="atmosphere-lamp"
          style={{
            position: 'absolute',
            inset: 0,
            background: `radial-gradient(circle at 68% 45%, ${accent}40 0%, ${accent}15 35%, transparent 70%)`,
            animation: 'atmosphereWarmGlow 10s ease-in-out infinite alternate',
          }}
        />
      )}

      {/* 3. SUN-DUST ENVIRONMENT */}
      {environment === 'sun-dust' && (
        <div className="atmosphere-sundust" style={{ position: 'absolute', inset: 0 }}>
          {/* Subtle godray ambient wash */}
          <div
            style={{
              position: 'absolute',
              inset: 0,
              background: `radial-gradient(ellipse at 78% 25%, ${accent}28 0%, ${accent}08 50%, transparent 75%)`,
            }}
          />
          {dustParticles.map((p) => (
            <span
              key={p.id}
              className="dust-particle"
              style={{
                position: 'absolute',
                left: `${p.left}%`,
                top: `${p.top}%`,
                width: `${p.size}px`,
                height: `${p.size}px`,
                borderRadius: '50%',
                backgroundColor: '#FFF7E6',
                boxShadow: `0 0 6px 1px ${accent}`,
                opacity: p.opacity,
                animation: `dustFloat ${p.duration}s ease-in-out infinite alternate`,
                animationDelay: `${p.delay}s`,
              }}
            />
          ))}
        </div>
      )}

      {/* 4. MIST ENVIRONMENT */}
      {environment === 'mist' && (
        <div className="atmosphere-mist" style={{ position: 'absolute', inset: 0 }}>
          <div
            className="mist-band-1"
            style={{
              position: 'absolute',
              left: '-20%',
              right: '-20%',
              top: '40%',
              height: '35%',
              background: 'radial-gradient(ellipse at 50% 50%, rgba(200, 220, 210, 0.14) 0%, transparent 70%)',
              filter: 'blur(30px)',
              animation: 'mistDriftSlow 32s ease-in-out infinite alternate',
            }}
          />
          <div
            className="mist-band-2"
            style={{
              position: 'absolute',
              left: '-15%',
              right: '-15%',
              top: '55%',
              height: '30%',
              background: 'radial-gradient(ellipse at 50% 50%, rgba(215, 230, 220, 0.10) 0%, transparent 65%)',
              filter: 'blur(35px)',
              animation: 'mistDriftSlow2 40s ease-in-out infinite alternate',
            }}
          />
        </div>
      )}

      {/* 5. CLOUDS / MOUNTAIN MIST ENVIRONMENT */}
      {environment === 'clouds' && (
        <div className="atmosphere-clouds" style={{ position: 'absolute', inset: 0 }}>
          <div
            style={{
              position: 'absolute',
              left: '30%',
              right: '-10%',
              top: '35%',
              height: '25%',
              background: `radial-gradient(ellipse at 60% 50%, ${accent}22 0%, rgba(255,255,255,0.06) 40%, transparent 70%)`,
              filter: 'blur(28px)',
              animation: 'cloudsFloat 28s ease-in-out infinite alternate',
            }}
          />
        </div>
      )}

      {/* 6. RAIN ENVIRONMENT */}
      {environment === 'rain' && (
        <div className="atmosphere-rain" style={{ position: 'absolute', inset: 0 }}>
          {rainStreaks.map((r) => (
            <span
              key={r.id}
              className="rain-streak"
              style={{
                position: 'absolute',
                left: `${r.left}%`,
                top: `${r.top}%`,
                width: '1px',
                height: `${r.length}px`,
                background: 'linear-gradient(to bottom, transparent, rgba(220, 235, 245, 0.45), transparent)',
                transform: 'rotate(12deg)',
                opacity: r.opacity,
                animation: `rainFall ${r.duration}s linear infinite`,
                animationDelay: `${r.delay}s`,
              }}
            />
          ))}
        </div>
      )}

      {/* 7. HORIZON ENVIRONMENT */}
      {environment === 'horizon' && (
        <div
          className="atmosphere-horizon"
          style={{
            position: 'absolute',
            inset: 0,
            background: `radial-gradient(ellipse 90% 40% at 75% 65%, ${accent}30 0%, ${accent}10 50%, transparent 75%)`,
            animation: 'atmosphereHorizonGlow 12s ease-in-out infinite alternate',
          }}
        />
      )}
    </div>
  );
};
