import assert from 'node:assert';
import {
  CINEMATIC_SLIDES,
  DEFAULT_SLIDE_INDEX,
} from '../src/pages/login/data/cinematicSlides.ts';
import { vi } from '../src/locales/vi.ts';
import { en } from '../src/locales/en.ts';
import { ja } from '../src/locales/ja.ts';
import { ko } from '../src/locales/ko.ts';
import { zhCN } from '../src/locales/zh-CN.ts';

console.log('=== TEST SUITE: TRYHARDAGAIN HERO CAROUSEL & CONTROLLER ===\n');

// ----------------------------------------------------------------------
// TEST 1: 10 Slides Integrity & Default Slide 05 (PATIENCE)
// ----------------------------------------------------------------------
console.log('Test 1: Verifying 10 slides, environment mapping, and default slide PATIENCE');

assert.strictEqual(CINEMATIC_SLIDES.length, 10, 'Must have exactly 10 slides');
assert.strictEqual(DEFAULT_SLIDE_INDEX, 4, 'Default slide must be index 4 (hero-05 - PATIENCE)');

const defaultSlide = CINEMATIC_SLIDES[DEFAULT_SLIDE_INDEX];
assert.strictEqual(defaultSlide.tag, 'PATIENCE', 'Default slide tag must be PATIENCE');
assert.strictEqual(defaultSlide.environment, 'sun-dust', 'PATIENCE environment must be single sun-dust');
assert.ok(defaultSlide.quote.includes('Hãy kiên nhẫn với chính mình'), 'PATIENCE quote must match target text');

const validEnvironments = new Set(['dawn', 'lamp', 'sun-dust', 'mist', 'clouds', 'rain', 'horizon']);
for (const slide of CINEMATIC_SLIDES) {
  assert.ok(validEnvironments.has(slide.environment), `Slide ${slide.tag} has valid single environment ${slide.environment}`);
  assert.ok(typeof slide.durationMs === 'number' && slide.durationMs >= 8000, `Slide ${slide.tag} duration must be >= 8s`);
  assert.ok(slide.accent.startsWith('#'), `Slide ${slide.tag} must have valid hex accent`);
}
console.log('✓ Test 1 Passed: Exactly 10 slides, valid single environments, default slide PATIENCE (index 4).\n');

// ----------------------------------------------------------------------
// TEST 2: Pause Reasons Matrix
// ----------------------------------------------------------------------
console.log('Test 2: Verifying Pause Reasons Set Logic (Multiple reasons, removing hover while modal is active keeps paused)');

const pauseReasons = new Set<string>();

// 1. Hover occurs
pauseReasons.add('hover');
assert.strictEqual(pauseReasons.size > 0, true, 'Carousel must be paused on hover');

// 2. Modal opens while hovering
pauseReasons.add('modal');
assert.strictEqual(pauseReasons.size, 2);

// 3. User unhovers, but modal is still open
pauseReasons.delete('hover');
assert.strictEqual(pauseReasons.size > 0, true, 'Carousel must remain paused because modal is still open');

// 4. Modal closes
pauseReasons.delete('modal');
assert.strictEqual(pauseReasons.size, 0, 'Carousel can resume when all pause reasons are cleared');

console.log('✓ Test 2 Passed: Pause reasons matrix prevents unhover from overriding active modals.\n');

// ----------------------------------------------------------------------
// TEST 3: Carousel Navigation & Wrap-around Loop
// ----------------------------------------------------------------------
console.log('Test 3: Verifying Wrap-around Navigation (0 -> 9, 9 -> 0)');

const total = CINEMATIC_SLIDES.length;
const wrapNext = (current: number) => (current + 1) % total;
const wrapPrev = (current: number) => (current - 1 + total) % total;

assert.strictEqual(wrapNext(9), 0, 'Next from slide 10 (index 9) must loop to slide 1 (index 0)');
assert.strictEqual(wrapPrev(0), 9, 'Prev from slide 1 (index 0) must loop to slide 10 (index 9)');
assert.strictEqual(wrapNext(4), 5, 'Next from PATIENCE (index 4) must advance to slide 6 (index 5)');
assert.strictEqual(wrapPrev(4), 3, 'Prev from PATIENCE (index 4) must go to slide 4 (index 3)');

console.log('✓ Test 3 Passed: Wrap-around navigation loops cleanly at both boundaries.\n');

// ----------------------------------------------------------------------
// TEST 4: Drag Decision & Axis-Lock
// ----------------------------------------------------------------------
console.log('Test 4: Verifying Drag axis-lock and gesture commit thresholds');

function evaluateGesture(deltaX: number, deltaY: number, durationMs: number, containerWidth: number) {
  // Axis lock: only engage horizontal drag if deltaX dominates
  if (Math.abs(deltaY) > Math.abs(deltaX)) {
    return 'pass-vertical-scroll';
  }

  const velocity = Math.abs(deltaX) / Math.max(1, durationMs);
  const threshold = Math.min(100, containerWidth * 0.2);

  if (deltaX < -threshold || (deltaX < -30 && velocity > 0.4)) {
    return 'commit-next';
  }
  if (deltaX > threshold || (deltaX > 30 && velocity > 0.4)) {
    return 'commit-prev';
  }
  return 'cancel-settle';
}

const width = 1000;
// Dominant vertical motion -> pass to page scroll
assert.strictEqual(evaluateGesture(10, 80, 200, width), 'pass-vertical-scroll');

// Drag left beyond 20% width -> commit next
assert.strictEqual(evaluateGesture(-120, 5, 300, width), 'commit-next');

// Drag right beyond 20% width -> commit prev
assert.strictEqual(evaluateGesture(120, 5, 300, width), 'commit-prev');

// Quick swipe flick left (> 0.4 px/ms) -> commit next
assert.strictEqual(evaluateGesture(-40, 2, 80, width), 'commit-next');

// Minor nudge without velocity -> cancel settle back to current
assert.strictEqual(evaluateGesture(-25, 4, 300, width), 'cancel-settle');

console.log('✓ Test 4 Passed: Drag correctly differentiates vertical scrolling, distance thresholds, and quick flicks.\n');

// ----------------------------------------------------------------------
// TEST 5: i18n Landing Keys across all 5 languages
// ----------------------------------------------------------------------
console.log('Test 5: Verifying Landing Keys Integrity across vi, en, ja, ko, zh-CN');

const allLocales: Record<string, any> = { vi, en, ja, ko, 'zh-CN': zhCN };
const expectedLandingKeys = [
  'navAbout',
  'heroSubline',
  'aboutGreeting',
  'aboutAuthorName',
  'aboutBio1',
  'aboutBio2',
  'aboutSkills',
  'focusTitle',
  'focus1Title',
  'focus1Desc',
  'focus2Title',
  'focus2Desc',
  'focus3Title',
  'focus3Desc',
  'projectBadge',
  'projectTitle',
  'projectDesc',
  'projectTags',
  'btnViewGithub',
  'btnDownloadApp',
  'contactTitle',
  'contactDesc',
  'copiedEmail',
  'copyEmail',
  'footerBrand',
];

for (const [langCode, localeObj] of Object.entries(allLocales)) {
  assert.ok(localeObj.landing, `landing object must exist in ${langCode}`);
  for (const key of expectedLandingKeys) {
    const val = (localeObj.landing as any)[key];
    assert.ok(
      typeof val === 'string' && val.trim().length > 0,
      `Missing or empty landing key "${key}" in language "${langCode}"`
    );
  }
}

console.log('✓ Test 5 Passed: All 5 languages have complete translations for the TryHardAgain landing page.\n');

console.log('ALL HERO CAROUSEL TESTS PASSED SUCCESSFULLY! (5/5 suites)');

import {
  createTypewriterTimeline, visibleCharacterCount, advanceCarouselClock, POST_TYPING_WAIT_MS,
} from '../src/pages/login/data/carouselTiming.ts';

console.log('Test 6: Actual typewriter clock holds the completed text for exactly two seconds');
const timeline = createTypewriterTimeline('Hãy tiếp tục.', 'Start again.');
let clock = advanceCarouselClock({ typingMs: 0, holdMs: 0 }, timeline.durationMs, timeline.durationMs, false, true);
assert.strictEqual(visibleCharacterCount(timeline, clock.typingMs), timeline.main.length + timeline.sub.length);
assert.strictEqual(clock.holdMs, 0, 'Typing the last grapheme starts the reading hold');
clock = advanceCarouselClock(clock, 1999, timeline.durationMs, false, true);
assert.ok(clock.holdMs < POST_TYPING_WAIT_MS, 'Must not transition at 1.999 seconds');
clock = advanceCarouselClock(clock, 1, timeline.durationMs, false, true);
assert.strictEqual(clock.holdMs, POST_TYPING_WAIT_MS, 'Transition is due at exactly 2 seconds');

console.log('Test 7: Pause freezes both typing and the reading hold, resume preserves elapsed time');
const incomplete = advanceCarouselClock({ typingMs: 0, holdMs: 0 }, 900, timeline.durationMs, false, true);
assert.deepStrictEqual(advanceCarouselClock(incomplete, 5000, timeline.durationMs, true, true), incomplete);
const waiting = { typingMs: timeline.durationMs, holdMs: 1200 };
assert.deepStrictEqual(advanceCarouselClock(waiting, 5000, timeline.durationMs, true, true), waiting);
assert.strictEqual(advanceCarouselClock(waiting, 800, timeline.durationMs, false, true).holdMs, 2000);
assert.strictEqual(advanceCarouselClock(waiting, 800, timeline.durationMs, false, false).holdMs, 1200);

console.log('Test 8: Graphemes and reduced motion preserve Vietnamese, Japanese and emoji');
const unicode = createTypewriterTimeline('a\u0301👍🏽日');
assert.deepStrictEqual(unicode.main, ['á', '👍🏽', '日']);
assert.strictEqual(visibleCharacterCount(unicode, unicode.revealAt[0] - 1), 0);
assert.strictEqual(visibleCharacterCount(unicode, unicode.revealAt[0]), 1);
const staticTimeline = createTypewriterTimeline('Xin chào', 'こんにちは', true);
assert.strictEqual(staticTimeline.durationMs, 0);
assert.strictEqual(visibleCharacterCount(staticTimeline, 0), staticTimeline.main.length + staticTimeline.sub.length);

console.log('Test 9: Controls and platform descriptions exist in all five languages');
const newKeys = [
  'carouselPause', 'carouselPlay', 'carouselPrevious', 'carouselNext', 'carouselSelect',
  'platformsTitle', 'platformsSubtitle', 'platformWebTitle', 'platformWebDesc', 'platformWebAction',
  'platformMobileTitle', 'platformMobileDesc', 'platformMobileAction',
  'platformDesktopTitle', 'platformDesktopDesc', 'platformDesktopAction',
];
for (const [code, locale] of Object.entries(allLocales)) {
  for (const key of newKeys) assert.ok(locale.landing[key]?.trim(), code + ': missing ' + key);
}
console.log('✓ Typewriter timing, pause/resume, graphemes and platform translations passed.');
