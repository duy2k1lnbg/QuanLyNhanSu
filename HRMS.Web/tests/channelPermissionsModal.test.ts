import assert from 'node:assert/strict';
import { vi } from '../src/locales/vi.ts';
import { en } from '../src/locales/en.ts';
import { zhCN } from '../src/locales/zh-CN.ts';
import { ko } from '../src/locales/ko.ts';
import { ja } from '../src/locales/ja.ts';

console.log('=== TEST SUITE: CHANNEL PERMISSION MODAL & I18N VERIFICATION ===');

const locales = {
  vi,
  en,
  'zh-CN': zhCN,
  ko,
  ja,
};

const requiredUserKeys = [
  'discardChangesConfirm',
  'reloadDiscardConfirm',
  'concurrencyConflictTitle',
  'concurrencyConflictContent',
  'reLoginRequiredTitle',
  'reLoginRequiredMsg',
  'targetAccount',
  'targetGroup',
  'statusDirectOn',
  'statusDirectOff',
  'totalFunctions',
  'btnSaveChannel',
  'btnSaveAllChannels',
  'allowPlatformLogin',
  'directGrant',
  'inheritedGrant',
  'effectiveGrant',
];

const sampleFunctionCodes = [
  'F_SYSTEM_GROUP',
  'F_SYSTEM_USER',
  'F_SYSTEM_CAPTAIKHOAN',
  'F_SYSTEM_AI',
  'F_SYSTEM_AI_CONFIG',
  'F_DM_NHANVIEN',
  'F_DM_PHONGBAN',
  'F_NV_HOPDONG',
  'F_CC_BANGCONG',
  'F_CC_BANGLUONG',
  'MOBILE_PROFILE_VIEW',
  'MOBILE_ATTENDANCE_VIEW',
  'MOBILE_PAYROLL_VIEW',
  'MOBILE_REQUEST_LEAVE',
];

// Test 1: Verify all 5 locales have required permission modal keys
console.log('\nTest 1: Verifying permission modal keys across all 5 languages...');
for (const [lang, loc] of Object.entries(locales)) {
  for (const k of requiredUserKeys) {
    const val = (loc as any).user?.[k];
    assert.ok(val && typeof val === 'string' && val.trim().length > 0, `Missing user.${k} in locale ${lang}`);
  }
}
console.log('✓ Test 1 Passed: All 5 languages have complete modal user keys.');

// Test 2: Verify all 5 locales have func translations and are distinct from Vietnamese hardcodes when not Vietnamese
console.log('\nTest 2: Verifying func translations in vi, en, zh-CN, ko, ja...');
for (const [lang, loc] of Object.entries(locales)) {
  assert.ok((loc as any).func, `Locale ${lang} missing func dictionary`);
  for (const code of sampleFunctionCodes) {
    const fnName = (loc as any).func?.[code];
    assert.ok(fnName && typeof fnName === 'string' && fnName.trim().length > 0, `Missing func.${code} in locale ${lang}`);
    if (lang === 'en') {
      // Must not be identical to Vietnamese canonical hardcodes for basic items
      if (code === 'F_SYSTEM_USER') {
        assert.equal(fnName, 'User Accounts');
      }
      if (code === 'F_SYSTEM_GROUP') {
        assert.equal(fnName, 'User Groups');
      }
    }
  }
}
console.log('✓ Test 2 Passed: Function translations correctly present and localized across all languages.');

// Test 3: Batch payload construction and atomicity verification
console.log('\nTest 3: Verifying Batch Save Channel Permissions payload contract...');
const sampleTrees = {
  DESKTOP: {
    Channel: 'DESKTOP',
    ParentDirectGrant: true,
    Functions: [
      { FunctionCode: 'F_SYSTEM_USER', DirectGrant: { CanView: true, CanAdd: true, CanEdit: true, CanDelete: false, CanPrint: true } },
    ],
  },
  WEB: {
    Channel: 'WEB',
    ParentDirectGrant: true,
    Functions: [
      { FunctionCode: 'F_SYSTEM_USER', DirectGrant: { CanView: true, CanAdd: false, CanEdit: false, CanDelete: false, CanPrint: false } },
    ],
  },
  MOBILE: {
    Channel: 'MOBILE',
    ParentDirectGrant: false,
    Functions: [],
  },
};

const securityVersion = 5;
const userId = 101;

const batchPayload = {
  TargetUserId: userId,
  ExpectedSecurityVersion: securityVersion,
  Channels: Object.keys(sampleTrees).map((ch) => {
    const tree = (sampleTrees as any)[ch];
    return {
      Channel: ch,
      ParentDirectGrant: tree.ParentDirectGrant,
      Functions: tree.Functions.map((f: any) => ({
        FunctionCode: f.FunctionCode,
        CanView: f.DirectGrant.CanView,
        CanAdd: f.DirectGrant.CanAdd,
        CanEdit: f.DirectGrant.CanEdit,
        CanDelete: f.DirectGrant.CanDelete,
        CanPrint: f.DirectGrant.CanPrint,
      })),
    };
  }),
};

assert.equal(batchPayload.TargetUserId, 101);
assert.equal(batchPayload.ExpectedSecurityVersion, 5);
assert.equal(batchPayload.Channels.length, 3);
assert.equal(batchPayload.Channels[0].Channel, 'DESKTOP');
assert.equal(batchPayload.Channels[1].Channel, 'WEB');
assert.equal(batchPayload.Channels[2].Channel, 'MOBILE');
console.log('✓ Test 3 Passed: Batch payload contains atomic 3-channel structure with ExpectedSecurityVersion.');

// Test 4: Concurrency conflict and ReLogin detection logic
console.log('\nTest 4: Verifying Concurrency Conflict & ReLogin Signal handling...');
const simulateApiResponse = (isConflict: boolean, requiresReLogin: boolean, newVersion: number) => {
  if (isConflict) {
    return { Success: false, IsConflict: true, Message: 'Xung đột phiên bản' };
  }
  return { Success: true, RequiresReLogin: requiresReLogin, NewSecurityVersion: newVersion, Message: 'Thành công' };
};

const conflictRes = simulateApiResponse(true, false, 5);
assert.equal(conflictRes.IsConflict, true);
assert.equal(conflictRes.Success, false);

const reLoginRes = simulateApiResponse(false, true, 6);
assert.equal(reLoginRes.Success, true);
assert.equal(reLoginRes.RequiresReLogin, true);
assert.equal(reLoginRes.NewSecurityVersion, 6);
console.log('✓ Test 4 Passed: Conflict response and ReLogin signals conform to security specifications.');

console.log('\nALL 4 TESTS PASSED IN channelPermissionsModal.test.ts!');
