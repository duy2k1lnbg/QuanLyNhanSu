import { useEffect, useRef, useState } from 'react';
import { Alert, Button, Card, DatePicker, Descriptions, Drawer, Input, Modal, Select, Space, Table, Tabs, Tag, Typography, message } from 'antd';
import { EyeOutlined, ReloadOutlined, SearchOutlined, StopOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { Dayjs } from 'dayjs';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import { PageHeader } from '../theme/components/PageHeader';
type Kind = 'activity' | 'security' | 'sessions';
interface AuditRow {
  id: string; userId?: number; username: string; channel: string; computerName?: string; deviceId?: string;
  action: string; result: string; timestamp: string; ipAddress?: string; ipSource?: string; userAgent?: string;
  module?: string; tableName?: string; recordId?: string; sessionId?: string; correlationId?: string;
  changedFields?: string; oldData?: string; newData?: string; executor?: string; appVersion?: string;
  lastSeen?: string; expiresAt?: string; canRevoke?: boolean;
}
export default function AuditLogPage() {
  const { t } = useAppLanguage(); const { tokens } = useAppTheme();
  const [kind, setKind] = useState<Kind>('activity');
  const [action,setAction] = useState('ALL'); const [module,setModule] = useState('ALL');
  const [options,setOptions] = useState<{actions:string[];modules:string[]}>({actions:[],modules:[]});
  const [channel,setChannel] = useState('ALL'); const [result,setResult] = useState('ALL');
  const [search,setSearch] = useState(''); const [sessionId,setSessionId] = useState(''); const [correlationId,setCorrelationId] = useState('');
  const [range,setRange] = useState<[Dayjs | null,Dayjs | null] | null>(null);
  const [rows,setRows] = useState<AuditRow[]>([]);const [total,setTotal] = useState(0);
  const [page,setPage] = useState(1);const [pageSize,setPageSize] = useState(20);
  const [loading,setLoading] = useState(false);const [error,setError] = useState('');const [detail,setDetail] = useState<AuditRow | null>(null);
  const serial=useRef(0);const abort=useRef<AbortController | null>(null);
  const label=(key:string)=>t('monitor.'+key);
  const display=(key:string)=>{const value=label(key);return value==='monitor.'+key?key:value;};
  useEffect(()=>{const controller=new AbortController();void api.get('/audit/modules',{signal:controller.signal}).then(response=>{if(response.data?.success)setOptions({actions:response.data.actions??[],modules:response.data.modules??[]});}).catch(()=>{});return()=>controller.abort();},[]);
  const load=async (target=1,size=pageSize,reset=false) => {
    const version=++serial.current;abort.current?.abort();const controller=new AbortController();abort.current=controller;
    setLoading(true);setError('');
    try {
      const response=await api.get('/audit',{signal:controller.signal,params:{kind,page:target,pageSize:size,
        action:reset || kind!=='activity'?'ALL':action,module:reset || kind!=='activity'?'ALL':module,channel:reset?'ALL':channel,result:reset?'ALL':result,search:reset?'':search,sessionId:reset?'':sessionId,correlationId:reset?'':correlationId,
        fromDate:reset?undefined:range?.[0]?.format('YYYY-MM-DD'),toDate:reset?undefined:range?.[1]?.format('YYYY-MM-DD')}});
      if(version!==serial.current)return;
      if(!response.data?.success)throw new Error('Invalid audit response');
      setRows(response.data.data??[]);setTotal(response.data.total??0);setPage(target);setPageSize(size);
    } catch {if(version===serial.current && !controller.signal.aborted){setError(label('loadError'));setRows([]);setTotal(0);}}
    finally {if(version===serial.current)setLoading(false);}
  };
  useEffect(()=>{setRows([]);setDetail(null);void load(1);return()=>{serial.current++;abort.current?.abort();};},[kind]);
  const reset=()=>{setAction('ALL');setModule('ALL');setChannel('ALL');setResult('ALL');setSearch('');setSessionId('');setCorrelationId('');setRange(null);void load(1,pageSize,true);};
  const revoke=(row:AuditRow)=>Modal.confirm({title:label('revoke'),content:label('revokeConfirm'),okText:label('revoke'),cancelText:t('common.cancel'),
    onOk:async()=>{try{const response=await api.post('/audit/sessions/'+encodeURIComponent(row.sessionId! )+'/revoke');if(!response.data?.success)throw new Error();message.success(label('revoked'));await load(page);}catch{message.error(label('revokeError'));throw new Error('Revoke failed');}}});
  const status=(value:string)=> <Tag color={value==='SUCCESS' || value==='ACTIVE'?'green':value==='DENIED' || value==='FAILURE'?'red':'default'}>{display(value || 'UNKNOWN')}</Tag>;
  const pretty=(value?:string)=>{if(!value)return label('unknown');try{return JSON.stringify(JSON.parse(value),null,2);}catch{return value;}};
  const columns:ColumnsType<AuditRow>=[
    {title:label('time'),dataIndex:'timestamp',width:165},
    {title:label('actor'),dataIndex:'username',width:165,render:(value,row)=><div><Typography.Text strong>{value}</Typography.Text><div><Typography.Text type="secondary">{row.userId ? '#'+row.userId : label('system')}</Typography.Text></div></div>},
    {title:label('source'),width:230,render:(_,row)=><div><Tag color={row.channel==='MOBILE'?'green':row.channel==='WEB'?'purple':'blue'}>{label(row.channel)}</Tag><div>{row.computerName || label('unknown')}</div></div>},
    ...(kind==='sessions'?[]:[{title:label('action'),dataIndex:'action',width:190}]),
    {title:label('result'),dataIndex:'result',width:155,render:status},
    ...(kind==='sessions'?[{title:label('lastSeen'),dataIndex:'lastSeen',width:165},{title:label('expires'),dataIndex:'expiresAt',width:165}]:[{title:label('object'),width:220,render:(_:unknown,row:AuditRow)=><div>{row.module}<div>{row.tableName} {row.recordId && '#'+row.recordId}</div></div>}]),
    {title:t('common.actions'),width:110,fixed:'right',render:(_,row)=><Space><Button aria-label={label('details')} icon={<EyeOutlined />} onClick={()=>setDetail(row)}/>{row.canRevoke && <Button danger aria-label={label('revoke')} icon={<StopOutlined />} onClick={()=>revoke(row)}/>}</Space>}
  ];
  return <div style={{display:'flex',flexDirection:'column',gap:16,width:'100%',minWidth:0}}>
    <PageHeader title={label('title')} subtitle={label('subtitle')} extra={<Button icon={<ReloadOutlined spin={loading}/>} onClick={()=>load(page)}>{t('common.refresh')}</Button>}/>
    <Card style={{background:tokens.cardBg,borderColor:tokens.borderSubtle}}>
      <Tabs activeKey={kind} onChange={value=>{setResult('ALL');setKind(value as Kind);}} items={(['activity','security','sessions'] as Kind[]).map(key=>({key,label:label(key)}))}/>
      <Space wrap style={{marginBottom:16}}>
        <Input value={search} allowClear placeholder={label('search')} onChange={event=>setSearch(event.target.value)} onPressEnter={()=>load(1)} style={{width:240}}/>
        <Select value={channel} onChange={setChannel} style={{width:160}} options={['ALL','DESKTOP','WEB','MOBILE','SYSTEM','UNKNOWN'].map(value=>({value,label:label(value)}))}/>
        <Select value={result} onChange={setResult} style={{width:170}} options={['ALL',...(kind==='sessions'?['ACTIVE','EXPIRED','REVOKED']:['SUCCESS','FAILURE','DENIED','UNKNOWN'])].map(value=>({value,label:label(value)}))}/>
        {kind==='activity' && <><Select aria-label={label('action')} value={action} onChange={setAction} showSearch style={{width:190}} options={['ALL',...options.actions].map(value=>({value,label:value==='ALL'?label('action')+' · '+label('ALL'):value}))}/><Select aria-label={label('object')} value={module} onChange={setModule} showSearch style={{width:175}} options={['ALL',...options.modules].map(value=>({value,label:value==='ALL'?label('object')+' · '+label('ALL'):value}))}/></>}
        <DatePicker.RangePicker value={range} onChange={value=>setRange(value)} format="DD/MM/YYYY"/>
        <Input value={sessionId} onChange={event=>setSessionId(event.target.value)} placeholder={label('session')} style={{width:190}} allowClear/>
        <Input value={correlationId} onChange={event=>setCorrelationId(event.target.value)} placeholder={label('correlation')} style={{width:190}} allowClear/>
        <Button type="primary" icon={<SearchOutlined />} onClick={()=>load(1)}>{t('common.search')}</Button><Button onClick={reset}>{label('reset')}</Button>
      </Space>
      <Alert type="info" showIcon title={label(kind==='sessions'?'sessionNote':'legacyNote')} style={{marginBottom:16}}/>
      {error && <Alert type="error" showIcon title={error} action={<Button onClick={()=>load(page)}>{label('retry')}</Button>} style={{marginBottom:16}}/>}
      <Typography.Paragraph type="secondary">{label('total')}: {total}</Typography.Paragraph>
      <Table<AuditRow> columns={columns} dataSource={rows} rowKey="id" loading={loading} scroll={{x:kind==='sessions'?1155:1235}} locale={{emptyText:error?label('loadError'):label('empty')}}
        pagination={{current:page,pageSize,total,showSizeChanger:true,onChange:(next,size)=>load(next,size)}}/>
    </Card>
    <Drawer title={label('details')} open={!!detail} onClose={()=>setDetail(null)} size={720}>
      {detail && <><Descriptions column={1} bordered items={[
        ['actor',detail.username],['source',label(detail.channel)],['action',detail.action],['result',display(detail.result)],['time',detail.timestamp],
        ['device',detail.computerName],['deviceId',detail.deviceId],['ip',detail.ipAddress],['ipSource',detail.ipSource],['userAgent',detail.userAgent],['executor',detail.executor],['version',detail.appVersion],
        ['session',detail.sessionId],['correlation',detail.correlationId],['object',[detail.tableName,detail.recordId].filter(Boolean).join(' #')],['lastSeen',detail.lastSeen],['expires',detail.expiresAt]
      ].map(([key,value])=>({key,label:label(key!),children:value||label('unknown')}))}/>
      {(['changedFields','oldData','newData'] as const).map(key=><div key={key} style={{marginTop:20}}><Typography.Title level={5}>{label(key)}</Typography.Title><pre style={{whiteSpace:'pre-wrap',overflowWrap:'anywhere',background:tokens.cardBg,padding:12,borderRadius:8}}>{pretty(detail[key])}</pre></div>)}</>}
    </Drawer>
  </div>;
}
