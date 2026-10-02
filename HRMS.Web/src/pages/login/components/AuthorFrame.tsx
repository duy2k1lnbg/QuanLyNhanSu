import React, { useState, useEffect, useRef } from 'react';
import { AUTHOR_INFO } from '../types';
import { useAppLanguage } from '../../../services/i18n';

interface AuthorFrameProps {
  useAvatar?: boolean;
}

export const AuthorFrame: React.FC<AuthorFrameProps> = ({
  useAvatar = AUTHOR_INFO.useRealAvatar,
}) => {
  const { tLanding } = useAppLanguage();
  const [hasAnimated, setHasAnimated] = useState<boolean>(false);
  const frameRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const el = frameRef.current;
    if (!el) return;

    if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) {
      setHasAnimated(true);
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0]?.isIntersecting) {
          setHasAnimated(true);
          observer.disconnect();
        }
      },
      { threshold: 0.2 }
    );

    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return (
    <div
      ref={frameRef}
      className="author-frame-container"
      style={{
        position: 'relative',
        width: '100%',
        maxWidth: 320,
        aspectRatio: '4 / 5',
        borderRadius: 8,
        overflow: 'hidden',
        boxShadow: '0 12px 36px rgba(0, 0, 0, 0.35)',
        backgroundColor: '#1E2D24',
      }}
    >
      {/* BACKGROUND SAGE MOSS TEXTURE */}
      <div
        className="author-frame-inner"
        style={{
          position: 'absolute',
          inset: 0,
          background: 'linear-gradient(145deg, #2D3D32 0%, #1A2820 60%, #15221B 100%)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          transition: 'transform 0.7s cubic-bezier(0.2, 0.8, 0.2, 1)',
        }}
      >
        {/* Subtle mottled noise/texture overlay */}
        <div
          style={{
            position: 'absolute',
            inset: 0,
            opacity: 0.15,
            backgroundImage: `radial-gradient(circle at 30% 20%, rgba(220, 197, 142, 0.4) 0%, transparent 40%),
                              radial-gradient(circle at 80% 80%, rgba(80, 120, 95, 0.6) 0%, transparent 50%)`,
          }}
        />

        {useAvatar ? (
          <img
            src={AUTHOR_INFO.avatarUrl}
            alt={tLanding.aboutAuthorName}
            style={{
              width: '100%',
              height: '100%',
              objectFit: 'cover',
              objectPosition: 'center 42%',
            }}
          />
        ) : (
          /* ARTISTIC MONOGRAM "ND" WITH LEAF STEM (MATCHING TARGET DESIGN) */
          <div
            style={{
              position: 'relative',
              width: '75%',
              height: '75%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            {/* Elegant SVG Botanical Monogram */}
            <svg
              viewBox="0 0 200 240"
              style={{
                width: '100%',
                height: '100%',
                overflow: 'visible',
              }}
            >
              {/* Botanical twig & leaf intertwined through the letters */}
              <g
                stroke="#C5CCBF"
                strokeWidth="1.4"
                fill="none"
                strokeLinecap="round"
                strokeLinejoin="round"
                opacity="0.75"
              >
                {/* Stem starting from bottom left curving upwards */}
                <path d="M 60 215 C 65 170, 95 120, 145 65" />
                {/* Small leaf 1 */}
                <path d="M 62 195 C 45 190, 48 175, 64 185" fill="rgba(197, 204, 191, 0.12)" />
                {/* Small leaf 2 */}
                <path d="M 68 180 C 85 175, 82 160, 70 170" fill="rgba(197, 204, 191, 0.12)" />
                {/* Small leaf 3 */}
                <path d="M 98 122 C 85 110, 98 95, 105 112" fill="rgba(197, 204, 191, 0.12)" />
                {/* Small leaf 4 at top right */}
                <path d="M 132 80 C 148 70, 155 82, 138 90" fill="rgba(197, 204, 191, 0.12)" />
                <path d="M 145 65 C 160 55, 168 68, 150 72" fill="rgba(197, 204, 191, 0.12)" />
              </g>

              {/* Serif ND Letters */}
              <text
                x="50"
                y="145"
                fontFamily="'Lora', 'Playfair Display', Georgia, serif"
                fontSize="92"
                fontWeight="500"
                letterSpacing="-4"
                fill="#F4EEDC"
              >
                N
              </text>
              <text
                x="105"
                y="155"
                fontFamily="'Lora', 'Playfair Display', Georgia, serif"
                fontSize="92"
                fontWeight="500"
                letterSpacing="-4"
                fill="#E8E0CB"
              >
                D
              </text>
            </svg>
          </div>
        )}
      </div>

      {/* DRAWING SVG BORDER OVERLAY (animates once on entering viewport) */}
      <svg
        style={{
          position: 'absolute',
          inset: 0,
          width: '100%',
          height: '100%',
          pointerEvents: 'none',
        }}
      >
        <rect
          x="12"
          y="12"
          width="calc(100% - 24px)"
          height="calc(100% - 24px)"
          rx="4"
          fill="none"
          stroke="rgba(244, 238, 220, 0.45)"
          strokeWidth="1"
          style={{
            strokeDasharray: 900,
            strokeDashoffset: hasAnimated ? 0 : 900,
            transition: 'stroke-dashoffset 1.7s cubic-bezier(0.2, 0.8, 0.2, 1)',
          }}
        />
      </svg>
    </div>
  );
};
