// Reveal whole words, preserving the exact server answer. Longer answers gain speed.
export function createAiTypingTimeline(text: string) {
  const ends = Array.from(text.matchAll(/\S+\s*/g), match => match.index! + match[0].length);
  const durationMs = Math.min(6000, Math.max(45, 45 * ends.length / (1 + ends.length / 180)));
  return {
    durationMs,
    wordCount: ends.length,
    visibleLength(elapsedMs: number): number {
      if (!ends.length || elapsedMs >= durationMs) return text.length;
      const count = Math.max(1, Math.floor(ends.length * Math.max(0, elapsedMs) / durationMs));
      return ends[Math.min(count, ends.length) - 1];
    },
  };
}
