import { useState, useEffect, useCallback, useRef } from 'react';

export type WelcomeIntroState = 'entering' | 'exiting' | 'done';

export const HRMS_INTRO_EVENT = 'hrms_welcome_intro_finished';

// Key for browser tab session storage.
// Stays present during F5 reloads in the same tab (so reload does NOT re-run the intro).
// Automatically cleared when the browser tab is closed and reopened.
const INTRO_TAB_SESSION_KEY = 'hrms_welcome_intro_tab_seen_v2';

let memoryTabSeen = false;

function checkTabSeen(): boolean {
  if (typeof window === 'undefined') return true;
  try {
    return sessionStorage.getItem(INTRO_TAB_SESSION_KEY) === 'true' || memoryTabSeen;
  } catch {
    return memoryTabSeen;
  }
}

function markTabSeen(): void {
  memoryTabSeen = true;
  if (typeof window !== 'undefined') {
    try {
      sessionStorage.setItem(INTRO_TAB_SESSION_KEY, 'true');
    } catch {
      // ignore storage error
    }
  }
}

export function isIntroActiveGlobal(): boolean {
  if (typeof window === 'undefined') return false;
  if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) return false;
  return !checkTabSeen();
}

export function notifyIntroFinished(): void {
  markTabSeen();
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent(HRMS_INTRO_EVENT));
  }
}

export function useWelcomeIntro() {
  const [state, setState] = useState<WelcomeIntroState>(() => {
    if (typeof window === 'undefined') return 'done';

    // Respect user's reduced motion preference
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) {
      markTabSeen();
      return 'done';
    }

    if (checkTabSeen()) {
      return 'done';
    }

    return 'entering';
  });

  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isFinishedRef = useRef(false);

  const finishIntro = useCallback(() => {
    if (isFinishedRef.current) return;
    isFinishedRef.current = true;
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }
    notifyIntroFinished();
    setState('done');
  }, []);

  const skipIntro = useCallback(() => {
    finishIntro();
  }, [finishIntro]);

  // Lock scroll while intro is active
  useEffect(() => {
    if (state === 'done') return;

    const prevBodyOverflow = document.body.style.overflow;
    const prevHtmlOverflow = document.documentElement.style.overflow;
    document.body.style.overflow = 'hidden';
    document.documentElement.style.overflow = 'hidden';

    return () => {
      document.body.style.overflow = prevBodyOverflow;
      document.documentElement.style.overflow = prevHtmlOverflow;
    };
  }, [state]);

  // Handle keys, visibility, preference changes, and animation timer phases
  useEffect(() => {
    if (state === 'done') return;

    // Any key skips immediately
    const handleKeyDown = (e: KeyboardEvent) => {
      // Prevent scrolling the page behind on Space or arrow keys
      if (e.key === ' ' || e.key === 'Spacebar' || e.key.startsWith('Arrow')) {
        e.preventDefault();
      }
      e.stopPropagation();
      skipIntro();
    };

    // If tab switches to background, finish immediately so it doesn't get stuck
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'hidden') {
        skipIntro();
      }
    };

    // If user prefers reduced motion while intro is running
    const motionQuery = window.matchMedia?.('(prefers-reduced-motion: reduce)');
    const handleMotionChange = (e: MediaQueryListEvent) => {
      if (e.matches) {
        skipIntro();
      }
    };

    window.addEventListener('keydown', handleKeyDown, { capture: true });
    document.addEventListener('visibilitychange', handleVisibilityChange);
    motionQuery?.addEventListener?.('change', handleMotionChange);

    // Forest welcome timing (~6.4s total): letters, portrait, shimmer, brand and hold.
    // entering (0 -> 5400ms): staggered letters and portrait, then shimmer at
    // 2800ms, brand at 3400ms, subtitle at 4000ms and a brief hold.
    // exiting (5400ms -> 6400ms): a 1000ms dissolve restores the landing page.
    // done (6400ms): unmount & restore full interaction
    if (state === 'entering') {
      timerRef.current = setTimeout(() => {
        setState('exiting');
      }, 5400);
    } else if (state === 'exiting') {
      timerRef.current = setTimeout(() => {
        finishIntro();
      }, 1000);
    }

    return () => {
      window.removeEventListener('keydown', handleKeyDown, { capture: true });
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      motionQuery?.removeEventListener?.('change', handleMotionChange);
      if (timerRef.current) {
        clearTimeout(timerRef.current);
        timerRef.current = null;
      }
    };
  }, [state, skipIntro, finishIntro]);

  return {
    introState: state,
    isIntroActive: state !== 'done',
    skipIntro,
  };
}
