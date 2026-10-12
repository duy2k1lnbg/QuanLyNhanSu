import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {timelineLabels,checklistLabels} from '../src/utils/profileEventLabels.ts';
import {vi} from '../src/locales/vi.ts';
import {en} from '../src/locales/en.ts';
import {ja} from '../src/locales/ja.ts';
import {ko} from '../src/locales/ko.ts';
import {zhCN} from '../src/locales/zh-CN.ts';
const dictionaries={vi,en,ja,ko,'zh-CN':zhCN};
const examples=[
  {Loai:'LUONG',TieuDe:'Chi trả lương kỳ Tháng 03/2026',MoTa:'Thực lĩnh: 12,500,000 đ (Công thực tế: 25.5/26 ngày công).',keep:['03','2026','12,500,000','25.5','26']},
  {Loai:'HOPDONG',TieuDe:'Ký Hợp đồng Lao động (#00132/2026/HDLD)',MoTa:'Ký hợp đồng thời hạn: 24 tháng. Lương thỏa thuận: 12,000,000 đ.',keep:['00132/2026/HDLD','24','12,000,000']},
  {Loai:'NANGLUONG',TieuDe:'Quyết định Nâng lương (#NL/26)',MoTa:'Điều chỉnh mức lương / hệ số từ 3.25 lên 3.50. Ghi chú của HR',keep:['NL/26','3.25','3.50','Ghi chú của HR']},
  {Loai:'KHENTHUONG',TieuDe:'Khen thưởng: Nhân viên xuất sắc (QĐ #KT01)',MoTa:'Lý do: Nội dung do HR nhập',keep:['Nhân viên xuất sắc','KT01','Nội dung do HR nhập']},
  {Loai:'KYLUAT',TieuDe:'Kỷ luật: Vi phạm nội quy (QĐ #KL01)',MoTa:'Lý do: Nội dung do HR nhập',keep:['Vi phạm nội quy','KL01','Nội dung do HR nhập']},
  {Loai:'DIEUCHUYEN',TieuDe:'Quyết định Điều chuyển công tác (#DC/26)',MoTa:'Lý do điều chuyển: Theo đề nghị. Giữ ghi chú.',keep:['DC/26','Theo đề nghị. Giữ ghi chú.']},
  {Loai:'JOIN',TieuDe:'Gia nhập công ty',MoTa:'Khởi tạo hồ sơ nhân sự trên hệ thống HRMS.',keep:[]},
];
const backend=readFileSync(new URL('../../HRMS.API/Controllers/NhanVienController.cs',import.meta.url),'utf8');
const checklistTitles=Array.from(backend.matchAll(/new Profile360ChecklistItemDTO \{ Title = "([^"]+)"/g),m=>m[1]);
assert.equal(checklistTitles.length,8,'Review changed backend checklist templates');
for(const [lang,dict] of Object.entries(dictionaries)) {
  const t=(key:string,params?:Record<string,string|number>)=>{
    let value:any=dict;
    for(const part of key.split('.')) value=value?.[part];
    assert.equal(typeof value,'string',lang+': '+key);
    return String(value).replace(/\{([^}]+)\}/g,(_,p)=>String(params?.[p]??'{'+p+'}'));
  };
  for(const section of ['userDetail','profileEvents'] as const) {
    assert.deepEqual(Object.keys(dict[section]).sort(),Object.keys(vi[section]).sort());
    for(const [key,value] of Object.entries(dict[section])) assert.ok(value.trim(),lang+': '+key);
  }
  for(const item of examples) {
    const result=timelineLabels(item,t);
    const all=result.title+' '+result.description;
    for(const preserved of item.keep) assert.ok(all.includes(preserved),lang+': missing '+preserved);
    if(lang!=='vi') {
      assert.notEqual(result.title,item.TieuDe);
      assert.notEqual(result.description,item.MoTa);
      assert.notEqual(result.label,item.Loai);
    }
  }
  for(const Title of checklistTitles) {
    const result=checklistLabels({Title,Detail:'0987654321'},t);
    assert.equal(result.detail,'0987654321');
    if(lang!=='vi') assert.notEqual(result.title,Title);
  }
  assert.ok(checklistLabels({Title:checklistTitles[0],Detail:'Mã NV #132'},t).detail.includes('132'));
  assert.ok(checklistLabels({Title:checklistTitles[5],Detail:'Số HĐ: 00132/2026/HDLD'},t).detail.includes('00132/2026/HDLD'));
  for(const Detail of ['Chưa cập nhật','Đã phân bổ','Chưa phân bổ','Chưa có hợp đồng','Đã có thông tin BHXH','Chưa đăng ký','Đã có ảnh chân dung','Chưa có ảnh chân dung']) {
    if(lang!=='vi') assert.notEqual(checklistLabels({Title:checklistTitles[0],Detail},t).detail,Detail);
  }
  const unknown={Loai:'CUSTOM',TieuDe:'Nội dung tự nhập',MoTa:'Số liệu 100.25 / ghi chú'};
  assert.deepEqual(timelineLabels(unknown,t),{title:unknown.TieuDe,description:unknown.MoTa,label:'CUSTOM'});
  assert.deepEqual(checklistLabels({Title:'Mục tùy chỉnh',Detail:'Địa chỉ nguyên gốc'},t),{title:'Mục tùy chỉnh',detail:'Địa chỉ nguyên gốc'});
  console.log('PASS '+lang+': 7 event types, 8 checklist templates, metadata and authored content preserved');
}
console.log('User detail and profile localization checks passed in five languages.');
