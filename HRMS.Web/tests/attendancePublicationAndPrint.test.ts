import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { vi } from '../src/locales/vi.ts';
import { en } from '../src/locales/en.ts';
import { ja } from '../src/locales/ja.ts';
import { ko } from '../src/locales/ko.ts';
import { zhCN } from '../src/locales/zh-CN.ts';
import { attendanceReasonLabel, attendanceStatusLabel } from '../src/utils/attendanceLabels.ts';
import { canPrint } from '../src/utils/permissionUtils.ts';
import type { CurrentUserDTO } from '../src/types/hrms.ts';

const page = readFileSync(new URL('../src/pages/ChamCongPage.tsx', import.meta.url), 'utf8');
const fixtures = [
  ['Kỳ công 202610 hiện đã được khóa.', ['202610']],
  ['Tồn tại 178 bất thường có mức độ chặn chốt chưa được giải trình/xác minh (Nhân viên: 5, 6, 17).', ['178','5, 6, 17']],
  ['Kỳ 202609 có 394 ngày công chưa qua công bố TimeSegmentationEngine hoặc chưa đủ điều kiện chốt.', ['202609','394']],
  ['Dữ liệu đầu vào đã có thay đổi mới (27 ngày công bị lệch so với lần công bố gần nhất). Cần tính toán công bố lại trước khi chốt.', ['27']],
] as const;
for (const [lang, dictionary] of Object.entries({ vi, en, ja, ko, 'zh-CN': zhCN })) {
  const t = (key: string, params?: Record<string,string|number>): string => {
    let value: unknown = dictionary;
    for (const part of key.split('.')) value = (value as Record<string, unknown>)?.[part];
    assert.equal(typeof value, 'string', `${lang}: missing key ${key}`);
    return String(value).replace(/\{([^}]+)\}/g, (_, p) => String(params?.[p] ?? '{'+p+'}'));
  };
  assert.deepEqual(Object.keys(dictionary.attendanceOps).sort(), Object.keys(vi.attendanceOps).sort());
  for (const [key,value] of Object.entries(dictionary.attendanceOps)) {
    assert.ok(value.trim(), `${lang}: empty ${key}`);
    const tokens = (value.match(/\{[^}]+\}/g) ?? []).sort();
    assert.deepEqual(tokens, (vi.attendanceOps[key as keyof typeof vi.attendanceOps].match(/\{[^}]+\}/g) ?? []).sort(), `${lang}: parameters ${key}`);
  }
  for (const match of page.matchAll(/\bt\('([^']+)'/g)) t(match[1]);
  for (const [input, preserved] of fixtures) {
    const result = attendanceReasonLabel(input, t);
    for (const token of preserved) assert.ok(result.includes(token), `${lang}: missing ${token}`);
    if (lang !== 'vi') assert.notEqual(result, input);
    assert.ok(!/\{[^}]+\}/.test(result));
  }
  for (const code of ['PUBLISHED','DRAFT','CHO_XU_LY','DA_XAC_MINH','HOAN_TAT','NGHIEP_VU_TREO','NGAY_THUONG']) {
    assert.notEqual(attendanceStatusLabel(code, t), code);
  }
  const custom = 'Ghi chú tự nhập của HR: NV001 / 12.50';
  assert.equal(attendanceReasonLabel(custom,t),custom);
  assert.equal(attendanceStatusLabel('CUSTOM_STATE',t),'CUSTOM_STATE');
  console.log(`PASS ${lang}: dictionary coverage, parameter preservation, readiness messages and statuses`);
}

const admin = { Username:'ADMIN', IsAdmin:true, Rights:['F_CC_BANGLUONG'], DetailedRights:{F_CC_BANGLUONG:{CAN_VIEW:true,CAN_PRINT:false}} } as unknown as CurrentUserDTO;
assert.equal(canPrint(admin,'F_CC_BANGLUONG'),false,'ADMIN name/role must not bypass Print grant');
assert.equal(canPrint({...admin,DetailedRights:undefined},'F_CC_BANGLUONG'),false,'View-only legacy claims must not authorize printing');
const granted = {...admin,DetailedRights:{F_CC_BANGLUONG:{CAN_VIEW:true,CAN_PRINT:true}}} as unknown as CurrentUserDTO;
assert.equal(canPrint(granted,'BANGLUONG','F_CC_BANGLUONG'),true,'Explicit print grant works through payroll alias');
assert.equal(canPrint({...admin,Username:'ordinary',IsAdmin:false,DetailedRights:granted.DetailedRights} as unknown as CurrentUserDTO,'F_CC_BANGLUONG'),true,'Regular account with explicit grant can print');
assert.equal(canPrint(null,'F_CC_BANGLUONG'),false);
console.log('PASS print permission: ADMIN without grant, view-only claims, alias, granted regular user and anonymous user');
