type Translate = (key: string, params?: Record<string, string | number>) => string;
interface TimelineEntry { Loai: string; TieuDe: string; MoTa: string }
interface ChecklistEntry { Title: string; Detail: string }
const eventKeys: Record<string,string> = {LUONG:'payroll',HOPDONG:'contract',NANGLUONG:'raise',KHENTHUONG:'reward',KYLUAT:'discipline',DIEUCHUYEN:'transfer',JOIN:'join'};
const checklistKeys: Record<string,string> = {
  'Tạo hồ sơ nhân sự trên hệ thống HRMS':'createProfile',
  'Cập nhật số Căn cước công dân (CCCD)':'idCard',
  'Cập nhật số điện thoại liên lạc':'phone',
  'Cập nhật địa chỉ cư trú':'address',
  'Phân bổ Phòng ban & Chức vụ công tác':'departmentRole',
  'Ký Hợp đồng lao động chính thức / thử việc':'signContract',
  'Tham gia Bảo hiểm xã hội (BHXH)':'insurance',
  'Tải lên ảnh chân dung nhân viên':'avatar',
};
const detailKeys: Record<string,string> = {'Chưa cập nhật':'notUpdated','Đã phân bổ':'assigned','Chưa phân bổ':'notAssigned','Chưa có hợp đồng':'noContract','Đã có thông tin BHXH':'insuranceRegistered','Chưa đăng ký':'notRegistered','Đã có ảnh chân dung':'avatarUploaded','Chưa có ảnh chân dung':'noAvatar'};
function contractTerm(raw:string,t:Translate):string {
  if (raw==='Xác định thời hạn') return t('profileEvents.fixedTerm');
  if (raw==='Không xác định thời hạn') return t('profileEvents.indefiniteTerm');
  const match=raw.match(/^(\d+)\s+(tháng|năm)$/i);
  return match ? t('profileEvents.'+(match[2].toLowerCase()==='tháng'?'months':'years'),{count:match[1]}) : raw;
}
// Translate known server-generated templates; retain identifiers, amounts and authored notes.
// Unknown templates remain intact rather than guessing their meaning.
export function timelineLabels(item:TimelineEntry,t:Translate) {
  let title=item.TieuDe,description=item.MoTa;
  const label=eventKeys[item.Loai] ? t('profileEvents.'+eventKeys[item.Loai]) : item.Loai;
  let m:RegExpMatchArray|null;
  if(item.Loai==='LUONG') {
    m=title.match(/^Chi trả lương kỳ Tháng (\d{1,2})\/(\d{4})$/);
    if(m) title=t('profileEvents.salaryTitle',{month:m[1],year:m[2]});
    m=description.match(/^Thực lĩnh: (.+?) đ \(Công thực tế: (.+?)\/(.+?) ngày công\)\.$/);
    if(m) description=t('profileEvents.salaryDescription',{net:m[1],actual:m[2],standard:m[3]});
  } else if(item.Loai==='HOPDONG') {
    m=title.match(/^Ký Hợp đồng Lao động \(#(.*)\)$/);
    if(m) title=t('profileEvents.contractTitle',{number:m[1]});
    m=description.match(/^Ký hợp đồng thời hạn: (.*?)\. Lương thỏa thuận: (.*?) đ\.$/);
    if(m) description=t('profileEvents.contractDescription',{term:contractTerm(m[1],t),salary:m[2]});
  } else if(item.Loai==='NANGLUONG') {
    m=title.match(/^Quyết định Nâng lương \(#(.*)\)$/);
    if(m) title=t('profileEvents.raiseTitle',{number:m[1]});
    m=description.match(/^Điều chỉnh mức lương \/ hệ số từ (.*?) lên (.*?)\. (.*)$/s);
    if(m) description=t('profileEvents.raiseDescription',{from:m[1],to:m[2],note:m[3]});
  } else if(item.Loai==='KHENTHUONG'||item.Loai==='KYLUAT') {
    m=title.match(/^(?:Khen thưởng|Kỷ luật): (.*) \(QĐ #(.*)\)$/s);
    if(m) title=t(item.Loai==='KHENTHUONG'?'profileEvents.rewardTitle':'profileEvents.disciplineTitle',{content:m[1],number:m[2]});
    if(description.startsWith('Lý do: ')) description=t('profileEvents.reason',{reason:description.slice(7)});
  } else if(item.Loai==='DIEUCHUYEN') {
    m=title.match(/^Quyết định Điều chuyển công tác \(#(.*)\)$/);
    if(m) title=t('profileEvents.transferTitle',{number:m[1]});
    const prefix='Lý do điều chuyển: ';
    if(description.startsWith(prefix)) description=t('profileEvents.transferDescription',{reason:description.slice(prefix.length)});
  } else if(item.Loai==='JOIN') {
    if(title==='Gia nhập công ty') title=t('profileEvents.joinTitle');
    if(description==='Khởi tạo hồ sơ nhân sự trên hệ thống HRMS.') description=t('profileEvents.joinDescription');
  }
  return {title,description,label};
}
export function checklistLabels(item:ChecklistEntry,t:Translate) {
  const title=checklistKeys[item.Title] ? t('profileEvents.'+checklistKeys[item.Title]) : item.Title;
  let detail=item.Detail;
  if(detailKeys[detail]) detail=t('profileEvents.'+detailKeys[detail]);
  else if(/^Mã NV #\d+$/.test(detail)) detail=t('profileEvents.employeeId',{id:detail.slice(7)});
  else if(detail.startsWith('Số HĐ: ')) detail=t('profileEvents.contractNumber',{number:detail.slice(7)});
  return {title,detail};
}
