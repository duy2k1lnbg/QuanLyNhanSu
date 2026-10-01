import { useState, useEffect, useRef, useCallback } from 'react';
import { isIntroActiveGlobal, HRMS_INTRO_EVENT } from './useWelcomeIntro';

export interface UseScrollRevealOptions {
  threshold?: number;
  rootMargin?: string;
  triggerOnce?: boolean;
}

/**
 * Custom hook for smooth scroll-driven element reveals using IntersectionObserver.
 * Synchronized with the Welcome Intro: elements in the viewport behind the intro
 * will wait until the intro is finished/skipped before triggering their reveal.
 */
export function useScrollReveal(options: UseScrollRevealOptions = {}) {
  const { threshold = 0.15, rootMargin = '0px 0px -20px 0px', triggerOnce = true } = options;

  const [isRevealed, setIsRevealed] = useState<boolean>(() => {
    if (typeof window === 'undefined') return true;
    // Immediately reveal if reduced motion is requested
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) return true;
    // Fallback if IntersectionObserver is not supported
    if (!('IntersectionObserver' in window)) return true;
    return false;
  });

  const elementRef = useRef<HTMLElement | null>(null);
  const isIntersectingRef = useRef<boolean>(false);

  const triggerReveal = useCallback(() => {
    setIsRevealed(true);
  }, []);

  // Keyboard accessibility: if element or any child receives focus, reveal immediately!
  const handleFocus = useCallback(() => {
    if (!isRevealed) {
      triggerReveal();
    }
  }, [isRevealed, triggerReveal]);

  useEffect(() => {
    if (isRevealed && triggerOnce) return;
    if (typeof window === 'undefined') return;

    // If reduced motion is requested
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) {
      setIsRevealed(true);
      return;
    }

    if (!('IntersectionObserver' in window)) {
      setIsRevealed(true);
      return;
    }

    const node = elementRef.current;
    if (!node) return;

    // Handle tall elements that might exceed viewport height
    const rect = node.getBoundingClientRect();
    const isTall = rect.height > (window.innerHeight || document.documentElement.clientHeight);
    const effectiveThreshold = isTall ? 0.05 : threshold;

    const observer = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        if (!entry) return;

        isIntersectingRef.current = entry.isIntersecting;

        if (entry.isIntersecting) {
          // If intro is still active, wait for it to complete
          if (isIntroActiveGlobal()) {
            return;
          }

          setIsRevealed(true);
          if (triggerOnce) {
            observer.unobserve(node);
          }
        } else if (!triggerOnce) {
          setIsRevealed(false);
        }
      },
      {
        threshold: effectiveThreshold,
        rootMargin,
      }
    );

    observer.observe(node);

    // Listen for intro completion to reveal any elements already in the viewport
    const handleIntroFinished = () => {
      if (isIntersectingRef.current) {
        setIsRevealed(true);
        if (triggerOnce) {
          observer.unobserve(node);
        }
      }
    };

    window.addEventListener(HRMS_INTRO_EVENT, handleIntroFinished);

    return () => {
      observer.disconnect();
      window.removeEventListener(HRMS_INTRO_EVENT, handleIntroFinished);
    };
  }, [isRevealed, threshold, rootMargin, triggerOnce]);

  return {
    ref: elementRef,
    isRevealed,
    triggerReveal,
    handleFocus,
  };
}
