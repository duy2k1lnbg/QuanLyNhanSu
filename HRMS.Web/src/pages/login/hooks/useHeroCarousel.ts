import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
import type { CinematicSlideData } from '../data/cinematicSlides';
import {
  advanceCarouselClock, createTypewriterTimeline, visibleCharacterCount, POST_TYPING_WAIT_MS,
} from '../data/carouselTiming';
import type { CarouselClock } from '../data/carouselTiming';
import { isIntroActiveGlobal, HRMS_INTRO_EVENT } from '../../../hooks/useWelcomeIntro';

export type PauseReason = 'hover' | 'focus' | 'drag' | 'modal' | 'documentHidden' | 'offscreen' | 'intro';

interface UseHeroCarouselOptions {
  slides: CinematicSlideData[];
  initialIndex?: number;
  isModalOpen?: boolean;
}

export function useHeroCarousel({ slides, initialIndex = 4, isModalOpen = false }: UseHeroCarouselOptions) {
  const [selection, setSelection] = useState(() => ({
    index: Math.max(0, Math.min(initialIndex, slides.length - 1)), revision: 0,
  }));
  const [reducedMotion, setReducedMotion] = useState(() =>
    window.matchMedia('(prefers-reduced-motion: reduce)').matches);
  const [isManualPaused, setIsManualPaused] = useState(reducedMotion);
  const manualPausedRef = useRef(isManualPaused);
  const typingPausedRef = useRef(false);
  const [pauseReasons, setPauseReasons] = useState<Set<PauseReason>>(() => {
    const reasons = new Set<PauseReason>();
    if (isIntroActiveGlobal()) reasons.add('intro');
    if (document.hidden) reasons.add('documentHidden');
    if (isModalOpen) reasons.add('modal');
    return reasons;
  });
  const pauseReasonsRef = useRef(pauseReasons);
  const clockRef = useRef<CarouselClock>({ typingMs: 0, holdMs: 0 });
  const lastFrameRef = useRef(0);
  const indexRef = useRef(selection.index);
  const [visibleCount, setVisibleCount] = useState(0);
  const displayedCountRef = useRef(-1);
  const progressBarRef = useRef<HTMLDivElement | null>(null);
  const heroRef = useRef<HTMLElement | null>(null);
  const currentSlide = slides[selection.index];
  const timeline = useMemo(() => createTypewriterTimeline(
    currentSlide.quote, currentSlide.quoteSub, reducedMotion,
  ), [currentSlide, reducedMotion]);

  const updateReasons = useCallback((update: (reasons: Set<PauseReason>) => void) => {
    const next = new Set(pauseReasonsRef.current);
    update(next);
    if (next.size === pauseReasonsRef.current.size &&
        [...next].every(reason => pauseReasonsRef.current.has(reason))) return;
    pauseReasonsRef.current = next;
    lastFrameRef.current = performance.now();
    setPauseReasons(next);
  }, []);

  const addPauseReason = useCallback((reason: PauseReason) => {
    updateReasons(reasons => { reasons.add(reason); });
  }, [updateReasons]);
  const removePauseReason = useCallback((reason: PauseReason) => {
    updateReasons(reasons => { reasons.delete(reason); });
  }, [updateReasons]);

  const goToSlide = useCallback((newIndex: number, manual = true) => {
    const normalized = ((newIndex % slides.length) + slides.length) % slides.length;
    indexRef.current = normalized;
    clockRef.current = { typingMs: 0, holdMs: 0 };
    displayedCountRef.current = -1;
    setVisibleCount(0);
    lastFrameRef.current = performance.now();
    if (progressBarRef.current) progressBarRef.current.style.transform = 'scaleX(0)';
    if (manual) {
      typingPausedRef.current = false;
      manualPausedRef.current = true;
      setIsManualPaused(true);
    }
    setSelection(previous => ({ index: normalized, revision: previous.revision + 1 }));
  }, [slides.length]);
  const nextSlide = useCallback((manual = true) => {
    goToSlide(indexRef.current + 1, manual);
  }, [goToSlide]);
  const prevSlide = useCallback((manual = true) => {
    goToSlide(indexRef.current - 1, manual);
  }, [goToSlide]);

  const togglePlayPause = useCallback(() => {
    const paused = manualPausedRef.current;
    typingPausedRef.current = !paused;
    manualPausedRef.current = !paused;
    setIsManualPaused(!paused);
    if (paused) {
      // An explicit Play action must work while its button is hovered/focused.
      updateReasons(reasons => { reasons.delete('hover'); reasons.delete('focus'); });
    }
    lastFrameRef.current = performance.now();
  }, [updateReasons]);

  useEffect(() => {
    if (isModalOpen) addPauseReason('modal');
    else removePauseReason('modal');
  }, [isModalOpen, addPauseReason, removePauseReason]);

  useEffect(() => {
    const finishIntro = () => removePauseReason('intro');
    window.addEventListener(HRMS_INTRO_EVENT, finishIntro);
    if (!isIntroActiveGlobal()) finishIntro();
    return () => window.removeEventListener(HRMS_INTRO_EVENT, finishIntro);
  }, [removePauseReason]);

  useEffect(() => {
    const onVisibility = () => {
      if (document.hidden) addPauseReason('documentHidden');
      else removePauseReason('documentHidden');
    };
    const media = window.matchMedia('(prefers-reduced-motion: reduce)');
    const onMotionChange = () => {
      setReducedMotion(media.matches);
      if (media.matches) {
        manualPausedRef.current = true;
        setIsManualPaused(true);
      }
    };
    document.addEventListener('visibilitychange', onVisibility);
    media.addEventListener('change', onMotionChange);
    const observer = new IntersectionObserver(entries => {
      if (entries[0]?.isIntersecting) removePauseReason('offscreen');
      else addPauseReason('offscreen');
    }, { threshold: 0.1 });
    if (heroRef.current) observer.observe(heroRef.current);
    return () => {
      document.removeEventListener('visibilitychange', onVisibility);
      media.removeEventListener('change', onMotionChange);
      observer.disconnect();
    };
  }, [addPauseReason, removePauseReason]);

  useEffect(() => {
    let frameId = 0;
    let disposed = false;
    lastFrameRef.current = performance.now();
    const tick = (now: number) => {
      if (disposed) return;
      const delta = Math.max(0, now - lastFrameRef.current);
      lastFrameRef.current = now;
      const reasons = pauseReasonsRef.current;
      const blocked = typingPausedRef.current || ['intro', 'modal', 'drag', 'documentHidden', 'offscreen']
        .some(reason => reasons.has(reason as PauseReason));
      const autoplay = !manualPausedRef.current && reasons.size === 0;
      clockRef.current = advanceCarouselClock(
        clockRef.current, delta, timeline.durationMs, blocked, autoplay,
      );
      const count = visibleCharacterCount(timeline, clockRef.current.typingMs);
      if (displayedCountRef.current !== count) {
        displayedCountRef.current = count;
        setVisibleCount(count);
      }
      const duration = timeline.durationMs + POST_TYPING_WAIT_MS;
      const ratio = Math.min(1, (clockRef.current.typingMs + clockRef.current.holdMs) / duration);
      if (progressBarRef.current) progressBarRef.current.style.transform = 'scaleX(' + ratio + ')';
      if (autoplay && clockRef.current.holdMs >= POST_TYPING_WAIT_MS) {
        nextSlide(false);
        return;
      }
      frameId = requestAnimationFrame(tick);
    };
    frameId = requestAnimationFrame(tick);
    return () => { disposed = true; cancelAnimationFrame(frameId); };
  }, [selection.revision, timeline, nextSlide]);

  const isTypingComplete = visibleCount >= timeline.main.length + timeline.sub.length;
  return {
    currentIndex: selection.index, currentSlide, goToSlide, nextSlide, prevSlide,
    isManualPaused, isPlaying: !isManualPaused,
    togglePlayPause, addPauseReason, removePauseReason, progressBarRef, heroRef,
    isIntroPending: pauseReasons.has('intro'),
    isAtmospherePaused: ['intro', 'documentHidden', 'offscreen', 'modal']
      .some(reason => pauseReasons.has(reason as PauseReason)),
    typedQuote: timeline.main.slice(0, visibleCount).join(''),
    typedSubquote: timeline.sub.slice(0, Math.max(0, visibleCount - timeline.main.length)).join(''),
    isTypingComplete,
  };
}
