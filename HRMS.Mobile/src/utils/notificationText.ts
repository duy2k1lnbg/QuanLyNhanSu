import type {NotificationDto} from '../types/me';
export function notificationText(item:NotificationDto,t:(key:string,params?:any)=>string): {tieude:string;noidung:string} {
  const event=String(item.eventType??'').toUpperCase();
  const match=item.noidung?.match(/^Phiếu lương kỳ (\d{6}) /);
  if(!match || !['PAYROLL_PUBLISHED','PAYROLL_PUBLICATION_REVOKED'].includes(event)) return {tieude:item.tieude,noidung:item.noidung};
  const period=match[1].slice(4)+'/'+match[1].slice(0,4);
  const prefix=event==='PAYROLL_PUBLISHED'?'publishedNotice':'retractedNotice';
  return {tieude:t('mobileReview.'+prefix+'Title'),noidung:t('mobileReview.'+prefix+'Body',{period})};
}
