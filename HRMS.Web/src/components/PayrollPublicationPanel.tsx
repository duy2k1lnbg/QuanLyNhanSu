import React, {useCallback, useEffect, useRef, useState} from 'react';
import {Alert, Button, Card, Checkbox, Input, Modal, Space, Spin, Typography, message} from 'antd';
import {ReloadOutlined, SendOutlined} from '@ant-design/icons';
import api from '../services/api';
import {useAppLanguage} from '../services/i18n';

interface Review {
  Period:number; Total:number; Pending:number; Published:number; LegacyPending:number;
  PeriodLocked:boolean; HasPermission:boolean; CanPublish:boolean; Frozen:boolean;
  Stamp:string; BlockerCodes:string[];
}
const routeConfig={params:undefined,_skipBaseUrlFallback:true};

export const PayrollPublicationPanel: React.FC<{period:number;onRefresh:()=>void;onOpenAttendance:()=>void;onBusyChange:(busy:boolean)=>void}> = ({period,onRefresh,onOpenAttendance,onBusyChange}) => {
  const {t}=useAppLanguage();
  const [review,setReview]=useState<Review|null>(null);
  const [loading,setLoading]=useState(false);
  const [error,setError]=useState<string|null>(null);
  const [action,setAction]=useState<'publish'|null>(null);
  const [reason,setReason]=useState('');
  const [legacy,setLegacy]=useState(false);
  const [saving,setSaving]=useState(false);
  const generation=useRef(0);
  const mounted=useRef(true);
  useEffect(()=>{mounted.current=true;return ()=>{mounted.current=false;};},[]);
  const refresh=useCallback(async()=>{
    const request=++generation.current;
    if(period<=0){setReview(null);return;}
    setLoading(true);setError(null);
    try {
      const result=await api.get(`/bangluong/${period}/publication`,routeConfig);
      if(request===generation.current)setReview(result.data.data);
    } catch(err:any) {
      if(request===generation.current){setReview(null);setError(err.response?.status===404?t('payrollPublication.updateServer'):t(err.response?.status===403?'payrollPublication.permission':'payrollPublication.loadFailed'));}
    } finally {if(request===generation.current)setLoading(false);}
  },[period,t]);
  useEffect(()=>{setAction(null);setReason('');setLegacy(false);void refresh();return ()=>{generation.current++;};},[refresh]);
  const submit=async()=>{
    if(!review || !action || saving)return;
    setSaving(true);onBusyChange(true);
    try {
      const result=await api.post(`/bangluong/${review.Period}/${action}`,{expectedStamp:review.Stamp,reason:reason.trim(),confirmLegacy:legacy},routeConfig);
      if(!mounted.current)return;
      message.success(t('payrollPublication.changed',{count:result.data.data.Changed}));
      setAction(null);onRefresh();await refresh();
    } catch(err:any) {
      if(!mounted.current)return;
      message.error(t(err.response?.data?.code==='PAYROLL_FROZEN'?'payrollPublication.frozen':err.response?.status===409?'payrollPublication.conflict':err.response?.status===403?'payrollPublication.permission':'payrollPublication.saveFailed'));
      if(err.response?.status===409){setAction(null);await refresh();}
    } finally {if(mounted.current){setSaving(false);onBusyChange(false);}}
  };
  const open=(value:'publish')=>{setReason('');setLegacy(false);setAction(value);};
  return <Card size="small" title={t('payrollPublication.title')} extra={<Button icon={<ReloadOutlined/>} loading={loading} disabled={saving} onClick={()=>void refresh()}>{t('payrollPublication.check')}</Button>}>
    <Typography.Paragraph>{t('payrollPublication.description')}</Typography.Paragraph>
    {loading ? <Spin/> : error ? <Alert type="error" message={error} showIcon/> : review ? <Space direction="vertical" style={{width:'100%'}}>
      <Typography.Text>{t('payrollPublication.summary',{period:review.Period,total:review.Total,pending:review.Pending,published:review.Published})}</Typography.Text>
      {review.Frozen && <Alert type="info" showIcon message={t('payrollPublication.frozen')}/>}
      <Typography.Text>{t(review.PeriodLocked?'payrollPublication.locked':'payrollPublication.open')}</Typography.Text>
      {review.LegacyPending>0 && <Alert type="warning" showIcon message={t('payrollPublication.legacyWarning',{count:review.LegacyPending})}/>}
      {review.BlockerCodes.map(code=><Alert key={code} type="warning" showIcon message={t(`payrollPublication.rule${code}`)}/>)}
      {!review.HasPermission && <Alert type="info" showIcon message={t('payrollPublication.permission')}/>}
      <Space wrap>
        <Button type="primary" icon={<SendOutlined/>} disabled={!review.CanPublish || saving} onClick={()=>open('publish')}>{t('payrollPublication.publish')}</Button>
        {review.BlockerCodes.includes('LOCK_PERIOD') && <Button disabled={saving} onClick={onOpenAttendance}>{t('payrollPublication.attendance')}</Button>}
      </Space>
    </Space> : <Typography.Text>{t('payrollPublication.selectPeriod')}</Typography.Text>}
    <Modal open={action!==null} title={t('payrollPublication.publish')}
      onCancel={()=>{if(!saving)setAction(null);}} onOk={()=>void submit()} confirmLoading={saving}
      okText={t('payrollPublication.confirm')} cancelText={t('payrollPublication.cancel')}
      okButtonProps={{disabled:reason.trim().length<5 || (action==='publish' && (review?.LegacyPending??0)>0 && !legacy)}} cancelButtonProps={{disabled:saving}}
      maskClosable={!saving} keyboard={!saving} closable={!saving}>
      <Typography.Paragraph>{t('payrollPublication.freezeWarning')}</Typography.Paragraph>
      {action==='publish' && (review?.LegacyPending??0)>0 && <Checkbox checked={legacy} onChange={event=>setLegacy(event.target.checked)}>{t('payrollPublication.legacyConfirm')}</Checkbox>}
      <Typography.Paragraph style={{marginTop:16,marginBottom:6}}>{t('payrollPublication.reason')}</Typography.Paragraph>
      <Input.TextArea value={reason} onChange={event=>setReason(event.target.value)} maxLength={1000} showCount rows={3}/>
    </Modal>
  </Card>;
};
