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
