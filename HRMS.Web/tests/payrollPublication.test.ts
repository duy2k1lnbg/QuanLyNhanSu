import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import {createRequire} from 'node:module';
import {vi} from '../src/locales/vi.ts';
import {en} from '../src/locales/en.ts';
import {ja} from '../src/locales/ja.ts';
import {ko} from '../src/locales/ko.ts';
import {zhCN} from '../src/locales/zh-CN.ts';
const require=createRequire(import.meta.url);
const ts=require('typescript');
const module={exports:{} as any};
const locales={vi,en,ja,ko,'zh-CN':zhCN};
const code=ts.transpileModule(fs.readFileSync(new URL('../src/services/i18n.ts',import.meta.url),'utf8'),{compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2022}}).outputText;
vm.runInNewContext(code.replaceAll('import.meta.env','({DEV:false})'),{module,exports:module.exports,require:(name:string)=>name==='react'?{}:{LOCALES:locales,LANGUAGE_CONFIG:{},ANTD_LOCALES:{}}});
for(const [language,locale] of Object.entries({vi,en,ja,ko,zhCN})) {
  assert.deepEqual(Object.keys(locale.payrollPublication).sort(),Object.keys(vi.payrollPublication).sort());
  for(const value of Object.values(locale.payrollPublication)) assert.ok(value.trim(),language);
  const output=module.exports.resolveTranslation(locale,vi,'payrollPublication.summary',{period:202601,total:194,pending:194,published:0});
  assert.ok(output.includes('202601') && output.includes('194'),language);
  assert.ok(!/[{}]|undefined/.test(output),language+': unresolved placeholders');
}
console.log('Payroll publication translations and interpolation passed for 5 languages.');
