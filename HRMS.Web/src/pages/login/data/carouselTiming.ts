export const TYPEWRITER_START_DELAY_MS = 650;
export const TYPEWRITER_CHARACTER_MS = 55;
export const POST_TYPING_WAIT_MS = 2000;

export interface TypewriterTimeline {
  main: string[];
  sub: string[];
  revealAt: number[];
  durationMs: number;
}

export interface CarouselClock {
  typingMs: number;
  holdMs: number;
}

export function createTypewriterTimeline(quote: string, quoteSub = '', reducedMotion = false): TypewriterTimeline {
  const segmenter = new Intl.Segmenter(undefined, { granularity: 'grapheme' });
  const split = (text: string) => Array.from(segmenter.segment(text.normalize('NFC')), item => item.segment);
  const main = split(quote);
  const sub = split(quoteSub);
  let elapsed = reducedMotion ? 0 : TYPEWRITER_START_DELAY_MS;
  const revealAt = [...main, ...sub].map((character, index) => {
    if (!reducedMotion) {
      if (index === main.length && sub.length) elapsed += 350;
      elapsed += /\s/u.test(character) ? 35 : TYPEWRITER_CHARACTER_MS;
    }
    const revealTime = elapsed;
    if (!reducedMotion) {
      if (/[.!?。！？]/u.test(character)) elapsed += 180;
      else if (/[,;:、，]/u.test(character)) elapsed += 95;
    }
    return revealTime;
  });
  // The reading hold starts when the last grapheme actually appears.
  return { main, sub, revealAt, durationMs: revealAt.at(-1) ?? elapsed };
}

export function visibleCharacterCount(timeline: TypewriterTimeline, elapsedMs: number): number {
  let low = 0;
  let high = timeline.revealAt.length;
  while (low < high) {
    const middle = (low + high) >>> 1;
    if (timeline.revealAt[middle] <= elapsedMs) low = middle + 1;
    else high = middle;
  }
  return low;
}

export function advanceCarouselClock(
  clock: CarouselClock,
  deltaMs: number,
  typingDurationMs: number,
  blocked: boolean,
  autoplay: boolean,
): CarouselClock {
  if (blocked) return clock;
  const delta = Math.max(0, deltaMs);
  const typingRemaining = Math.max(0, typingDurationMs - clock.typingMs);
  const typingDelta = Math.min(delta, typingRemaining);
  return {
    typingMs: clock.typingMs + typingDelta,
    holdMs: clock.holdMs + (autoplay ? Math.max(0, delta - typingDelta) : 0),
  };
}
