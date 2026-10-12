import assert from 'node:assert/strict';
import { AiRequestGuard, buildAiChatPayload, aiConversationStorageKey, getAiAvailability } from '../src/utils/aiConversation.ts';

const prompt = { clarificationId: 'c1', targetField: 'MANV', question: 'Chọn nhân viên', options: [{optionToken:'opaque-1',label:'An',value:'10'}] };
const payload = buildAiChatPayload('An', 'conv1', 'request1', 2, prompt, 'opaque-1');
assert.equal(payload.question, '');
assert.equal(payload.expectedConversationVersion, 2);
assert.equal(payload.clientRequestId, 'request1');
assert.equal(payload.clarification?.clarificationId, 'c1');
assert.equal(payload.clarification?.optionToken, 'opaque-1');
assert.equal(payload.clarification?.freeTextAnswer, undefined);
assert.equal(buildAiChatPayload('năm 2026', 'conv1','r2',3,prompt).clarification?.freeTextAnswer,'năm 2026');
assert.notEqual(aiConversationStorageKey(1),aiConversationStorageKey(2));

const guard = new AiRequestGuard();
let finish!: (value: string) => void;
const delayed = new Promise<string>(resolve => { finish = resolve; });
const generation = guard.begin();
const delivered: string[] = [];
const pending = delayed.then(text => { if (guard.accepts(generation)) delivered.push(text); });
guard.invalidate(); // reset/unmount/logout in the real Drawer
finish('late private answer');
await pending;
assert.deepEqual(delivered, []);
const older = guard.begin();
const current = guard.begin();
assert.equal(guard.accepts(older),false);
assert.equal(guard.accepts(current),true);
console.log('AI Web behavior: payload, clarification identity, version, actor key and late-response guards passed.');

// API connectivity and Oracle query readiness must not be conflated with Ollama availability.
assert.deepEqual(getAiAvailability({ connected: true, llmAvailable: true, queryReady: false }), {connected:true,queryReady:false});
assert.deepEqual(getAiAvailability({ connected: true, llmAvailable: false, queryReady: true }), {connected:true,queryReady:true});
assert.deepEqual(getAiAvailability({ connected: true }), {connected:true,queryReady:null});
assert.deepEqual(getAiAvailability({ connected: false, queryReady: true }), {connected:false,queryReady:null});


// Word animation must never lose accents, punctuation, whitespace or the end of a long answer.
const { createAiTypingTimeline } = await import('../src/utils/aiTyping.ts');
for (const text of ['', ' \n\t', '  Xin chào 👩‍💻!\r\nDòng cuối: 1.234.567 ₫.  ', 'Từ '.repeat(15000) + 'KẾT THÚC']) {
  const timeline = createAiTypingTimeline(text);
  let previous = '';
  let reconstructed = '';
  for (let elapsed = 0; elapsed <= timeline.durationMs + 30; elapsed += 30) {
    const shown = text.slice(0, timeline.visibleLength(elapsed));
    assert.ok(shown.startsWith(previous));
    reconstructed += shown.slice(previous.length);
    previous = shown;
  }
  assert.equal(reconstructed, text);
}
const speeds = [10, 100, 500, 1000, 10000].map(words => {
  const timeline = createAiTypingTimeline('từ '.repeat(words));
  assert.ok(timeline.durationMs <= 6000);
  return words / timeline.durationMs;
});
assert.ok(speeds.every((speed, i) => i === 0 || speed > speeds[i - 1]));
console.log('AI typing: exact full text, monotonic prefixes and increasing long-answer speed passed.');
