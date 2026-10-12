import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { aiCapabilityLabel, buildAiGrants, createAiDraft, isAiPermissionOn, toggleAiPermission } from '../src/utils/aiPermissions.ts';
import type { AiCapability } from '../src/utils/aiPermissions.ts';
import { canEdit, canAccessRoute } from '../src/utils/permissionUtils.ts';
import type { CurrentUserDTO } from '../src/types/hrms.ts';
import { vi } from '../src/locales/vi.ts';
import { en } from '../src/locales/en.ts';
import { ja } from '../src/locales/ja.ts';
import { ko } from '../src/locales/ko.ts';
import { zhCN } from '../src/locales/zh-CN.ts';

let passed = 0;
function test(name: string, action: () => void) { action(); passed++; console.log('PASS: ' + name); }
const capability: AiCapability = {
  CapabilityCode: 'EMPLOYEE_LOOKUP', Description: 'Tra cứu nhân viên', Domain: 'EMPLOYEE',
  IsCapabilityEnabled: true, HasRequiredFunctionRight: true, EffectiveEffect: 'ALLOW', InheritedGrants: [],
  DirectGrants: [
    { Effect: 'ALLOW', ScopeType: 'DEPARTMENT', ScopeKey: '2', ValidTo: '2030-01-01T00:00:00Z' },
    { Effect: 'ALLOW', ScopeType: 'DEPARTMENT', ScopeKey: '3' },
    { Effect: 'DENY', ScopeType: 'ALL' },
  ],
};
test('Retains every scope and DENY when editing another row', () => {
  const draft = createAiDraft([capability]); draft.EMPLOYEE_LOOKUP[0].ScopeKey = '4';
  const saved = buildAiGrants([capability], draft);
  assert.equal(saved.length, 3); assert.equal(saved[0].ScopeKey, '4');
  assert.equal(saved[1].ScopeKey, '3'); assert.equal(saved[2].Effect, 'DENY');
  assert.equal(saved[0].ValidTo, '2030-01-01T00:00:00Z');
  assert.equal(capability.DirectGrants[0].ScopeKey, '2');
});
test('No direct grant is distinct from an explicit DENY', () => {
  assert.deepEqual(buildAiGrants([capability], { EMPLOYEE_LOOKUP: [] }), []);
  assert.equal(buildAiGrants([capability], { EMPLOYEE_LOOKUP: [{ Effect: 'DENY', ScopeType: 'ALL' }] })[0].Effect, 'DENY');
});
test('SELF and ALL cannot retain a stale department key', () => {
  for (const ScopeType of ['SELF', 'ALL'] as const)
    assert.equal(buildAiGrants([capability], { EMPLOYEE_LOOKUP: [{ Effect: 'ALLOW', ScopeType, ScopeKey: '999' }] })[0].ScopeKey, null);
});
test('Department selection cannot be omitted', () => {
  for (const ScopeKey of [null, undefined, '', '  ']) assert.throws(() => buildAiGrants([capability], { EMPLOYEE_LOOKUP: [{ Effect: 'ALLOW', ScopeType: 'DEPARTMENT', ScopeKey }] }));
});
test('Invalid, equal and reversed validity intervals are rejected', () => {
  for (const [ValidFrom, ValidTo] of [['invalid', null], ['2030-01-01', '2030-01-01'], ['2031-01-01', '2030-01-01']])
    assert.throws(() => buildAiGrants([capability], { EMPLOYEE_LOOKUP: [{ Effect: 'ALLOW', ScopeType: 'ALL', ValidFrom, ValidTo }] }));
});
test('Disabled capabilities are not sent as editable grants', () => {
  assert.deepEqual(buildAiGrants([{ ...capability, IsCapabilityEnabled: false }], createAiDraft([capability])), []);
});
test('ADMIN name, legacy wildcard and IsAdmin flag cannot override an explicit denied action', () => {
  const user = { Username: 'ADMIN', IsAdmin: true, Rights: ['*'], DetailedRights: { F_SYSTEM_PQ_CHUCNANG: { CAN_VIEW: false, CAN_EDIT: false } } } as unknown as CurrentUserDTO;
  assert.equal(canEdit(user, 'F_SYSTEM_PQ_CHUCNANG'), false);
  assert.equal(canAccessRoute(user, 'phanquyen'), false);
});
test('A different account can manage permissions with the actual edit grant', () => {
  const user = { Username: 'permission.manager', IsAdmin: false, Rights: [], DetailedRights: { F_SYSTEM_PQ_CHUCNANG: { CAN_VIEW: true, CAN_EDIT: true } } } as unknown as CurrentUserDTO;
  assert.equal(canEdit(user, 'F_SYSTEM_PQ_CHUCNANG'), true);
  assert.equal(canAccessRoute(user, 'phanquyen'), true);
});
test('Five languages have the complete AI panel vocabulary', () => {
  for (const locale of [vi, en, ja, ko, zhCN]) {
    assert.deepEqual(Object.keys(locale.aiPermissions).sort(), Object.keys(vi.aiPermissions).sort());
    const checkStrings = (value: unknown): void => {
      if (typeof value === 'string') assert.ok(value.trim().length > 0);
      else for (const nested of Object.values(value as Record<string, unknown>)) checkStrings(nested);
    };
    checkStrings(locale.aiPermissions);
  }
});
test('Turning off keeps department scopes and dates, and blocks an inherited allow', () => {
  const cap = { ...capability, InheritedGrants: [{ Effect: 'ALLOW' as const, ScopeType: 'ALL' as const }] };
  const grants = toggleAiPermission(cap, cap.DirectGrants, false);
  assert.equal(grants.filter(g => g.Effect === 'ALLOW').length, 2);
  assert.equal(grants[0].ValidTo, '2030-01-01T00:00:00Z');
  assert.equal(grants.filter(g => g.Effect === 'DENY').length, 1);
  assert.equal(isAiPermissionOn(cap, grants), false);
});
test('Turning back on removes the block without widening existing department scopes', () => {
  const off = toggleAiPermission(capability, capability.DirectGrants, false);
  const on = toggleAiPermission(capability, off, true);
  assert.deepEqual(on.map(g => g.ScopeKey), ['2', '3']);
  assert.equal(on[0].ValidTo, '2030-01-01T00:00:00Z');
  assert.equal(isAiPermissionOn(capability, on), true);
});
test('A new binary permission defaults to SELF, never ALL', () => {
  assert.equal(toggleAiPermission(capability, [], true)[0].ScopeType, 'SELF');
});
test('A binary switch cannot override an active group denial', () => {
  const cap = { ...capability, InheritedGrants: [{ Effect: 'DENY' as const, ScopeType: 'ALL' as const, IsActive: true }] };
  assert.equal(isAiPermissionOn(cap, toggleAiPermission(cap, [], true)), false);
});
test('An expired SELF grant is renewed without duplicate scope rows', () => {
  const on = toggleAiPermission(capability, [{ Effect: 'ALLOW', ScopeType: 'SELF', ValidTo: '2000-01-01', IsActive: false }], true);
  assert.equal(on.length, 1); assert.equal(on[0].ValidTo, null);
  assert.equal(isAiPermissionOn(capability, on), true);
});
test('Every capability in the backend catalog has a name in all five languages', () => {
  const source = readFileSync(new URL('../../HRMS.Business/Services/AI_Services/Security/Authorization/AiCapabilityCatalog.cs', import.meta.url), 'utf8');
  const codes = [...source.matchAll(/CapabilityCode = "([^"\n]+)"/g)].map(match => match[1]).sort();
  assert.equal(codes.length, 17);
  for (const locale of [vi, en, ja, ko, zhCN]) assert.deepEqual(Object.keys(locale.aiPermissions.capabilities).sort(), codes);
});
test('AI rows use localized capability names rather than the backend Vietnamese description', () => {
  for (const locale of [en, ja, ko, zhCN]) {
    const label = aiCapabilityLabel(capability, key => key.split('.').reduce((value: any, part) => value?.[part], locale) || key);
    assert.equal(label, locale.aiPermissions.capabilities.EMPLOYEE_LOOKUP);
    assert.notEqual(label, capability.Description);
  }
  assert.equal(aiCapabilityLabel({...capability, CapabilityCode: 'NEW_CAPABILITY'}, key => key), capability.Description);
});
console.log(`${passed} AI permission tests passed.`);
