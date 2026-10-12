import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';
import {createRequire} from 'node:module';
const require=createRequire(import.meta.url);
const root=path.resolve('src');
const ts=require('typescript');
const plain=value=>JSON.parse(JSON.stringify(value));
function load(relative,stubs={},cache=new Map()) {
  const file=path.resolve(root,relative);
  if(cache.has(file)) return cache.get(file);
  const module={exports:{}};cache.set(file,module.exports);
  const localRequire=name=>{
    if(Object.hasOwn(stubs,name)) return stubs[name];
    if(name.startsWith('.')) return load(path.relative(root,path.resolve(path.dirname(file),name+'.ts')),stubs,cache);
    return require(name);
  };
  const code=ts.transpileModule(fs.readFileSync(file,'utf8'),{compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2020}}).outputText;
  vm.runInNewContext(code,{module,exports:module.exports,require:localRequire,Date,Error,Promise,Set,Map,console,Math,setInterval,clearInterval},{filename:file});
  return module.exports;
}
const contracts=load('utils/requestContracts.ts');
const rights=load('utils/mobilePermissions.ts');
const calls=[];
let response={success:true};
const client={get:async(...args)=>{calls.push(['GET',...args]);return {data:response};},post:async(...args)=>{calls.push(['POST',...args]);return {data:response};},put:async(...args)=>{calls.push(['PUT',...args]);return {data:response};}};
const api=load('api/meApi.ts',{'./client':{apiClient:client}}).meApi;

test('real date helper rejects rollover, ambiguous dates and preserves local date',()=>{
  assert.equal(contracts.isoDate('29/02/2024'),'2024-02-29');
  for(const input of ['2026-02-29','2026-04-31','10/12/26','2026-13-01','wrong']) assert.throws(()=>contracts.isoDate(input),/invalidDate/);
  assert.equal(contracts.localDate(new Date(2026,9,12,0,15)),'2026-10-12');
});
test('dependent contract normalizes YYYY-MM to numeric Oracle/API months',()=>{
  const item=contracts.dependentPayload({id:5,hoTen:'A',moiQuanHe:'Con',ngaySinh:'12/10/2020',thangBatDau:'2026-10',thangKetThuc:'2027-01'});
  assert.equal(item.thangBatDau,202610);assert.equal(item.thangKetThuc,202701);assert.equal(item.ngaySinh,'2020-10-12');assert.equal(item.action,'UPDATE');
  assert.throws(()=>contracts.dependentPayload({...item,thangKetThuc:'2026-09'}),/invalidMonthRange/);
  assert.throws(()=>contracts.dependentPayload({...item,action:'TERMINATE',thangKetThuc:''}),/endMonthRequired/);
  assert.throws(()=>contracts.monthNumber('2026-00'),/invalidMonth/);
});
test('all request types and pending counts survive real response normalization',()=>{
  const result=contracts.normalizeRequests({success:true,data:{items:[{id:1,requestType:'AVATAR',status:'pending'},{id:2,requestType:'DEPENDENTS',status:'APPROVED'},{id:3,requestType:'INSURANCE_INFO',status:'PENDING'},{id:4,requestType:'ADVANCE',status:'DISBURSED',amount:1000}]}});
  assert.equal(result.items.length,4);assert.equal(contracts.pendingRequestCount(result),2);assert.equal(result.items[3].amount,1000);
  assert.equal(contracts.normalizeRequests({leaves:[{idYeuCau:9,trangThai:'PENDING',tuNgay:'2026-10-12'}]}).items[0].id,9);
});
test('permissions distinguish View/Add/Edit and never grant an admin bypass',()=>{
  assert.equal(rights.canMobile({isAdmin:true,rights:[]},'MOBILE_REQUEST_ADVANCE','Add'),false);
  assert.equal(rights.canMobile({rights:['MOBILE_REQUEST_LEAVE']},'MOBILE_REQUEST_LEAVE'),true);
  assert.equal(rights.canMobile({rights:['MOBILE_REQUEST_LEAVE']},'MOBILE_REQUEST_LEAVE','Add'),false);
  const user={detailedRights:{mOBILE_REQUEST_LEAVE:{CAN_VIEW:true,CAN_ADD:false,CAN_EDIT:true}}};
  assert.equal(rights.canMobile(user,'MOBILE_REQUEST_LEAVE'),true);assert.equal(rights.canMobile(user,'MOBILE_REQUEST_LEAVE','Add'),false);assert.equal(rights.canViewRequests(user),true);
  assert.equal(rights.canMobile({detailedRights:{MOBILE_REQUEST_LEAVE:{canView:'false'}}},'MOBILE_REQUEST_LEAVE'),false);
});
test('notification destinations target real root routes and nested notification tab',()=>{
  assert.equal(contracts.notificationDestination({eventType:'PROFILE_REQUEST_APPROVED'}).screen,'MyRequests');
  assert.equal(contracts.notificationDestination({eventType:'ATTENDANCE_CORRECTION'}).screen,'MyRequests');
  assert.equal(contracts.notificationDestination({eventType:'SALARY_ADVANCE_APPROVED'}).screen,'SalaryAdvance');
  assert.deepEqual(plain(contracts.notificationDestination({notificationId:123})),{screen:'NotificationDetail',params:{notificationId:123}});
  assert.deepEqual(plain(contracts.notificationDestination({})),{screen:'Main',params:{screen:'NotificationsTab'}});
});
test('actual API adapter posts correct dependent payload and avatar verb',async()=>{
  response={success:true};calls.length=0;
  await api.proposeDependents([{id:3,hoTen:'A',moiQuanHe:'Con',action:'TERMINATE',thangBatDau:'2026-01',thangKetThuc:'2026-10'}],'test');
  assert.equal(calls[0][2].dependents[0].thangBatDau,202601);assert.equal(calls[0][2].dependents[0].thangKetThuc,202610);
  await api.uploadAvatar('image');assert.equal(calls.at(-1)[0],'POST');assert.match(calls.at(-1)[1],/profile\/avatar$/);
});
test('actual API adapter does not turn unpublished payroll or no contract into a fake record',async()=>{
  response={success:true,data:null};assert.equal(await api.getPayroll(2026,10),null);assert.equal(await api.getContract(),null);assert.equal(await api.getInsurance(),null);
});
test('actual API adapter validates amount, hours, time and dates before sending',async()=>{
  response={success:true};calls.length=0;
  for(const amount of [-1,0,100.1,100000001,NaN]) await assert.rejects(api.createAdvanceRequest({amount,reason:'test'}),/invalidAmount/);
  await assert.rejects(api.createOvertimeRequest({hours:25,idCa:1,otDate:'2026-10-12',reason:'test'}),/invalidOvertime/);
  await assert.rejects(api.createAttendanceCorrection({workDate:'2026-10-12',requestedCheckIn:'25:00',reason:'test'}),/invalidTime/);
  await assert.rejects(api.createLeaveRequest({fromDate:'2026-10-13',toDate:'2026-10-12',totalDays:1,reason:'test',leaveType:'Nghỉ phép năm'}),/invalidLeave/);
  assert.equal(calls.length,0);
  await api.createLeaveRequest({fromDate:'12/10/2026',toDate:'12/10/2026',totalDays:0.5,startTime:'08:00',endTime:'12:00',reason:'test',leaveType:'Nghỉ phép năm'});
  assert.equal(calls[0][2].fromDate,'2026-10-12');assert.equal(calls[0][2].startTime,'08:00');assert.equal(calls[0][2].endTime,'12:00');
});
test('device identity uses a single persistent ID across concurrent API and notification calls',async()=>{
  let writes=0,value=null;
  const identity=load('utils/deviceIdentity.ts',{'react-native':{Platform:{OS:'android'}},'@react-native-async-storage/async-storage':{__esModule:true,default:{getItem:async()=>value,setItem:async(key,id)=>{writes++;value=id;}}}});
  const ids=await Promise.all(Array.from({length:10},()=>identity.getInstallationId()));
  assert.equal(new Set(ids).size,1);assert.equal(writes,1);assert.equal(await identity.getInstallationId(),value);
});
test('device identity fails explicitly when persistence fails, then allows retry',async()=>{
  let fail=true;
  const identity=load('utils/deviceIdentity.ts',{'react-native':{Platform:{OS:'android'}},'@react-native-async-storage/async-storage':{__esModule:true,default:{getItem:async()=>null,setItem:async()=>{if(fail) throw new Error('store failed');}}}});
  await assert.rejects(identity.getInstallationId(),/store failed/);fail=false;assert.match(await identity.getInstallationId(),/^inst_android_/);
});
test('new Mobile labels are translated in all five locales, including dynamic statuses',()=>{
  const locales=['vi','en','ja','zh-CN','ko'].map(name=>JSON.parse(fs.readFileSync(path.join(root,'i18n/locales',name+'.json'),'utf8')));
  const expected=Object.keys(locales[0].mobileReview).sort();
  for(const locale of locales) {assert.deepEqual(Object.keys(locale.mobileReview).sort(),expected);for(const value of Object.values(locale.mobileReview)) assert.ok(value.trim());}
  for(const type of ['LEAVE','ATTENDANCE','OVERTIME','ADVANCE','PROFILE','PERSONAL_INFO','AVATAR','INSURANCE_INFO','DEPENDENTS']) assert.ok(locales.every(locale=>locale.mobileReview['type'+type]));
});

test('leave balance comes from the actual ledger endpoint and unwraps its response',async()=>{
  response={success:true,data:{tongCapNgay:10,daDungNgay:3,conLaiNgay:7,transactions:[{id:1,loai:'CAP'}]}};calls.length=0;
  const balance=await api.getLeaveBalance();assert.equal(balance.conLaiNgay,7);assert.equal(balance.transactions.length,1);assert.equal(calls[0][1],'/me/leave-balance');
});

test('in-app polling runs only in foreground and removes timer/listener on logout',()=>{
  const {startForegroundRefresh}=load('utils/foregroundRefresh.ts');
  let stateListener,tick,polls=0,cancels=0,removed=false;
  const state={currentState:'background',addEventListener:(event,fn)=>{stateListener=fn;return {remove:()=>removed=true};}};
  const stop=startForegroundRefresh(state,()=>polls++,(fn,delay)=>{assert.equal(delay,30000);tick=fn;return 7;},()=>cancels++);
  assert.equal(polls,0);stateListener('active');assert.equal(polls,1);tick();assert.equal(polls,2);
  stateListener('background');assert.equal(cancels,1);stateListener('active');assert.equal(polls,3);
  stop();assert.equal(cancels,2);assert.equal(removed,true);
});
test('notification API preserves pagination, unread filter and server failures',async()=>{
  response={success:true,data:[]};calls.length=0;
  await api.getNotifications(50,true);
  assert.deepEqual(plain(calls[0][2]),{params:{offset:50,limit:50,unreadOnly:true}});
  response={success:true,data:{count:0}};
  assert.equal(await api.getUnreadNotificationCount(),0);
  const get=client.get;
  try {
    client.get=async()=>{throw new Error('server unavailable');};
    await assert.rejects(api.getNotifications(),/server unavailable/);
    await assert.rejects(api.getUnreadNotificationCount(),/server unavailable/);
    await assert.rejects(api.getNotificationDetail(3),/server unavailable/);
  } finally {client.get=get;}
});

test('Expo Go never imports push SDK even when an EAS projectId is present',()=>{
  let nativeLoads=0;
  const stubs={
    'react-native':{Platform:{OS:'android'}},
    'expo-constants':{__esModule:true,default:{executionEnvironment:'storeClient',expoConfig:{extra:{eas:{projectId:'configured-project'}}}},ExecutionEnvironment:{StoreClient:'storeClient'}},
  };
  Object.defineProperty(stubs,'expo-notifications',{get:()=>{nativeLoads++;throw new Error('Expo Go push module imported');}});
  const remote=load('services/remoteNotifications.ts',stubs);
  assert.equal(remote.canUseRemoteNotifications(),false);
  assert.equal(remote.getRemoteNotifications(),null);assert.equal(nativeLoads,0);
});
test('Web and Expo Go never load native push module',()=>{
  for(const [platform,constants] of [
    ['ios',{executionEnvironment:'storeClient',appOwnership:'expo'}],
    ['web',{executionEnvironment:'standalone',expoConfig:{extra:{eas:{projectId:'configured-project'}}}}],
    ['android',{executionEnvironment:'standalone',appOwnership:'expo',expoConfig:{extra:{eas:{projectId:'configured-project'}}}}],
  ]) {
    const stubs={'react-native':{Platform:{OS:platform}},'expo-constants':{__esModule:true,default:constants,ExecutionEnvironment:{StoreClient:'storeClient'}}};
    Object.defineProperty(stubs,'expo-notifications',{get:()=>{throw new Error('native push module imported');}});
    const remote=load('services/remoteNotifications.ts',stubs);
    assert.equal(remote.getRemoteNotifications(),null);
  }
});
test('configured native build loads push SDK lazily and initializes it once',()=>{
  let nativeLoads=0,handlers=0;
  const native={setNotificationHandler:()=>handlers++};
  const stubs={
    'react-native':{Platform:{OS:'android'}},
    'expo-constants':{__esModule:true,default:{executionEnvironment:'standalone',expoConfig:{extra:{eas:{projectId:'configured-project'}}}},ExecutionEnvironment:{StoreClient:'storeClient'}},
  };
  Object.defineProperty(stubs,'expo-notifications',{get:()=>{nativeLoads++;return native;}});
  const remote=load('services/remoteNotifications.ts',stubs);
  assert.equal(nativeLoads,0);assert.equal(remote.canUseRemoteNotifications(),true);assert.equal(nativeLoads,0);
  assert.equal(remote.getRemoteNotifications(),native);assert.equal(remote.getRemoteNotifications(),native);
  assert.equal(nativeLoads,1);assert.equal(handlers,1);
});

test('notification service starts in Expo Go and all push hooks stay inactive',async()=>{
  let nativeLoads=0;
  const stubs={
    'react-native':{Platform:{OS:'android'},Linking:{}},
    'expo-constants':{__esModule:true,default:{executionEnvironment:'storeClient',expoConfig:{extra:{eas:{projectId:'configured-project'}}}},ExecutionEnvironment:{StoreClient:'storeClient'}},
    'expo-device':{isDevice:true},
    '@react-native-async-storage/async-storage':{__esModule:true,default:{}},
    '../api/meApi':{meApi:{}},
    '../i18n/i18n':{__esModule:true,default:{t:key=>key}},
    '../utils/deviceIdentity':{getInstallationId:async()=>'test-installation'},
  };
  Object.defineProperty(stubs,'expo-notifications',{get:()=>{nativeLoads++;throw new Error('push SDK imported during startup');}});
  const service=load('services/notificationService.ts',stubs).notificationService;
  assert.equal(service.isRemotePushConfigured(),false);
  service.subscribeReceived(()=>{throw new Error('received hook fired');})();
  service.subscribeResponse(()=>{throw new Error('response hook fired');})();
  await service.setupAndroidChannel();
  assert.deepEqual(plain(await service.requestPermission()),{status:'UNDETERMINED',canAskAgain:false});
  assert.deepEqual(plain(await service.getPermissionStatus()),{status:'UNDETERMINED',canAskAgain:false});
  assert.equal(await service.getExpoPushToken(),null);
  await service.checkLastNotificationResponse({navigate:()=>{throw new Error('unexpected navigation');}});
  assert.equal(nativeLoads,0);
});

test('missing unread-count route falls back to dashboard count, including real zero',async()=>{
  const get=client.get;let urls=[];
  try {
    for(const count of [0,7]) {
      urls=[];client.get=async(url)=>{urls.push(url);if(url.includes('unread-count'))throw {response:{status:404}};return {data:{success:true,data:{unreadNotificationCount:count}}};};
      assert.equal(await api.getUnreadNotificationCount(),count);
      assert.deepEqual(urls,['/me/notifications/unread-count','/me/dashboard']);
    }
  } finally {client.get=get;}
});
test('notification counter never retries 401, 403 or server failure as missing route',async()=>{
  const get=client.get;
  try {
    for(const status of [401,403,500]) {
      let calls=0;const error={response:{status}};
      client.get=async()=>{calls++;throw error;};
      await assert.rejects(api.getUnreadNotificationCount(),err=>err===error);assert.equal(calls,1);
    }
    client.get=async(url)=>{if(url.includes('unread-count'))throw {response:{status:404}};return {data:{data:{}}};};
    await assert.rejects(api.getUnreadNotificationCount(),err=>err.response.status===404);
  } finally {client.get=get;}
});
test('notification list remains usable when only unread counter fails',async()=>{
  const get=client.get;
  try {
    client.get=async(url)=>{if(url.includes('unread-count'))throw {response:{status:500}};return {data:{data:[{id:1,isUnread:true}]}};};
    const page=await api.getNotificationPage();
    assert.equal(page.items[0].id,1);assert.equal(page.unreadCount,null);
    client.get=async(url)=>{if(url.endsWith('/notifications'))throw {response:{status:403}};return {data:{data:{count:0}}};};
    await assert.rejects(api.getNotificationPage(),err=>err.response.status===403);
  } finally {client.get=get;}
});
test('payroll availability distinguishes unpublished, missing and old API response',async()=>{
  for(const availability of ['NOT_PUBLISHED','NOT_CREATED']){
    response={success:true,data:null,availability};const result=await api.getPayrollResult(2026,1);
    assert.equal(result.payroll,null);assert.equal(result.availability,availability);
  }
  response={success:true,data:null,message:'not published'};
  assert.equal((await api.getPayrollResult(2026,1)).availability,'UNKNOWN');
  response={success:true,data:{idbl:1,makycong:202601}};
  assert.equal((await api.getPayrollResult(2026,1)).availability,'AVAILABLE');
});

test('publication notices translate their period and preserve ordinary announcement content',()=>{
  const {notificationText}=load('utils/notificationText.ts');
  const t=(key,params)=>key+(params?.period ? ':'+params.period : '');
  const item={id:1,tieude:'server title',noidung:'Phiếu lương kỳ 202601 đã được công bố.',eventType:'PAYROLL_PUBLISHED'};
  assert.equal(notificationText(item,t).tieude,'mobileReview.publishedNoticeTitle');
  assert.equal(notificationText(item,t).noidung,'mobileReview.publishedNoticeBody:01/2026');
  assert.equal(notificationText({...item,eventType:'PAYROLL_PUBLICATION_REVOKED'},t).tieude,'mobileReview.retractedNoticeTitle');
  assert.equal(notificationText({...item,eventType:'ANNOUNCEMENT'},t).noidung,item.noidung);
});

test('published-period rejection is localized before any backend text',()=>{
  const {mapApiError}=load('utils/errorMapper.ts',{'../i18n/i18n':{__esModule:true,default:{t:key=>key}}});
  assert.equal(mapApiError({response:{status:409,data:{code:'PAYROLL_FROZEN',message:'Server text'}}}),'errors.payrollFrozen');
  for(const locale of ['vi','en','ja','ko','zh-CN'])assert.ok(JSON.parse(fs.readFileSync(`src/i18n/locales/${locale}.json`,'utf8')).errors.payrollFrozen);
});
