import { useEffect, useRef, useState } from 'react';

interface UseLandingParallaxOptions {
  speed?: number; // default 0.12 (gentle, slow)
  maxOffset?: number; // default 32px
}

export function useLandingParallax({
  speed = 0.12,
  maxOffset = 32,
}: UseLandingParallaxOptions = {}) {
  const [offsetY, setOffsetY] = useState<number>(0);
  const elementRef = useRef<HTMLDivElement | null>(null);
  const rafIdRef = useRef<number | null>(null);

  useEffect(() => {
    // Disable if reduced motion is requested
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) {
      return;
    }

    const handleScroll = () => {
      if (rafIdRef.current !== null) return;

      rafIdRef.current = requestAnimationFrame(() => {
        rafIdRef.current = null;
        const el = elementRef.current;
        if (!el) return;

        const rect = el.getBoundingClientRect();
        const windowHeight = window.innerHeight || 800;

        // Only compute if element is within or near viewport
        if (rect.bottom >= -100 && rect.top <= windowHeight + 100) {
          const centerDelta = rect.top + rect.height / 2 - windowHeight / 2;
          const targetOffset = Math.max(-maxOffset, Math.min(maxOffset, centerDelta * speed));
          setOffsetY(targetOffset);
        }
      });
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll();

    return () => {
      window.removeEventListener('scroll', handleScroll);
      if (rafIdRef.current !== null) {
        cancelAnimationFrame(rafIdRef.current);
      }
    };
  }, [speed, maxOffset]);

  return { elementRef, offsetY };
}
