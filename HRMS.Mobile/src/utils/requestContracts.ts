import type { DependentDto, UnifiedRequestsDto, UnifiedRequestItem } from '../types/me';
export function localDate(now = new Date()): string {
  return `${now.getFullYear()}-${String(now.getMonth()+1).padStart(2,'0')}-${String(now.getDate()).padStart(2,'0')}`;
}
export function isoDate(value: string): string {
  const raw = value.trim();
  // Parse explicitly; JavaScript's date rollover must not accept 31 February.
  const dmy = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(raw);
  const iso = /^(\d{4})-(\d{2})-(\d{2})$/.exec(raw);
  if (!dmy && !iso) throw new Error('mobileReview.invalidDate');
  const year=Number(iso ? iso[1] : dmy![3]), month=Number(iso ? iso[2] : dmy![2]), day=Number(iso ? iso[3] : dmy![1]);
  const date=new Date(year,month-1,day);
  if(year<1900 || year>2100 || date.getFullYear()!==year || date.getMonth()!==month-1 || date.getDate()!==day) throw new Error('mobileReview.invalidDate');
  return `${year}-${String(month).padStart(2,'0')}-${String(day).padStart(2,'0')}`;
}
export function monthNumber(value: string | number | null | undefined, fallback?: number): number | undefined {
  if (value===undefined || value===null || String(value).trim()==='') return fallback;
  const raw=String(value).trim().replace('-', '');
  if(!/^\d{6}$/.test(raw)) throw new Error('mobileReview.invalidMonth');
  const period=Number(raw), month=period%100, year=Math.floor(period/100);
  if(month<1 || month>12 || year<1900 || year>2100) throw new Error('mobileReview.invalidMonth');
  return period;
}
export function dependentPayload(item: DependentDto) {
  const now = new Date();
  const start=monthNumber(item.thangBatDau, now.getFullYear()*100+now.getMonth()+1)!;
  const end=monthNumber(item.thangKetThuc);
  if(end!==undefined && end<start) throw new Error('mobileReview.invalidMonthRange');
  if(item.action==='TERMINATE' && (!item.id || !end)) throw new Error('mobileReview.endMonthRequired');
  return {...item, action:item.action || (item.id ? 'UPDATE':'ADD'), ngaySinh:item.ngaySinh ? isoDate(item.ngaySinh):undefined, thangBatDau:start, thangKetThuc:end};
}
export function requestStatus(status?: string): string { return String(status || 'PENDING').trim().toUpperCase(); }
export function normalizeRequests(raw: any): UnifiedRequestsDto {
  const value=raw?.data || raw || {};
  const leaves=value.leaves || [], corrections=value.corrections || [], overtimes=value.overtimes || [], advances=value.advances || [];
  const source=Array.isArray(value) ? value : (Array.isArray(value.items) ? value.items : [
    ...leaves.map((x:any)=>({...x,requestType:'LEAVE'})), ...corrections.map((x:any)=>({...x,requestType:'ATTENDANCE'})),
    ...overtimes.map((x:any)=>({...x,requestType:'OVERTIME'})), ...advances.map((x:any)=>({...x,requestType:'ADVANCE'}))]);
  const items: UnifiedRequestItem[]=source.map((item:any)=>({...item, id:item.id ?? item.idYeuCau, requestType:item.requestType || item.groupCode,
    status:requestStatus(item.status ?? item.trangThai), reason:item.reason ?? item.lyDo ?? '', rejectionReason:item.rejectionReason ?? item.lyDoTuChoi ?? item.note,
    createdAt:item.createdAt ?? item.ngayTao ?? '', dateRange:item.dateRange ?? item.fromDate ?? item.workDate ?? item.otDate ?? item.advanceDate ?? '',
    title:item.title, subtitle:item.subtitle, amount:item.amount ?? item.soTienUng, hours:item.hours ?? item.soGio, totalDays:item.totalDays ?? item.soNgay}));
  return {leaves,corrections,overtimes,advances,items};
}
export function pendingRequestCount(data: UnifiedRequestsDto): number { return (data.items || []).filter(item=>requestStatus(item.status)==='PENDING').length; }
export function notificationDestination(data: any): {screen:string; params?:any} {
  const notificationId=Number(data?.notificationId);
  if (Number.isSafeInteger(notificationId) && notificationId>0) return {screen:'NotificationDetail',params:{notificationId}};
  const type=String(data?.eventType || data?.type || '').toUpperCase();
  if(/LEAVE|NGHI_PHEP|ATTENDANCE_CORRECTION|DIEUCHINH_CONG|OVERTIME|TANG_CA|PROFILE_REQUEST|YEUCAU_HOSO|DEPENDENT|INSURANCE/.test(type)) return {screen:'MyRequests'};
  if(/SALARY_ADVANCE|YEUCAU_UNGLUONG/.test(type)) return {screen:'SalaryAdvance'};
  const id=Number(data?.notificationId || data?.id);
  return Number.isFinite(id) && id>0 ? {screen:'NotificationDetail',params:{notificationId:id}} : {screen:'Main',params:{screen:'NotificationsTab'}};
}

export function validClock(value?: string): boolean { return typeof value==='string' && /^([01]\d|2[0-3]):[0-5]\d$/.test(value.trim()); }
