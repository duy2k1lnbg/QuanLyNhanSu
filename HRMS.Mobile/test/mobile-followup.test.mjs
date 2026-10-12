import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';
import {createRequire} from 'node:module';
const require=createRequire(import.meta.url), ts=require('typescript');
const root=path.resolve('src');
const plain=value=>JSON.parse(JSON.stringify(value));
const element=(type,props,...children)=>({type,props:{...props,children:children.flat(Infinity)}});
const react={createElement:element,Fragment:'Fragment',useState:value=>[value,()=>{}],useEffect:()=>{},useCallback:fn=>fn};
function load(relative,stubs={},fallback=false) {
  const file=path.resolve(root,relative), module={exports:{}};
  const localRequire=name=>{
    if(Object.hasOwn(stubs,name)) return stubs[name];
    if(name.startsWith('.')) {
      if(fallback) return new Proxy({}, {get:(_,key)=>String(key)});
      const resolved=path.resolve(path.dirname(file),name);
      return load(path.relative(root,resolved+(fs.existsSync(resolved+'.tsx')?'.tsx':'.ts')),stubs);
    }
    return require(name);
  };
  const code=ts.transpileModule(fs.readFileSync(file,'utf8'),{fileName:file,compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2020,jsx:ts.JsxEmit.React}}).outputText;
  vm.runInNewContext(code,{module,exports:module.exports,require:localRequire,Date,Promise,Error,Set,Map,Math,URL,console,setInterval,clearInterval,__DEV__:false},{filename:file});
  return module.exports;
}
const permissions=load('utils/mobilePermissions.ts');
function allNodes(tree) {
  if(!tree || typeof tree!=='object') return [];
  return [tree,...(tree.props?.children||[]).flatMap(allNodes)];
}
function textContent(tree) {
  if(typeof tree==='string') return tree;
  return (tree?.props?.children||[]).map(textContent).join('');
}

test('actual root registers LanguageSettings before login and keeps protected routes unavailable',()=>{
  let authenticated=false;
  const rootNavigator=load('navigation/RootNavigator.tsx',{
    react,
    '@react-navigation/native-stack':{createNativeStackNavigator:()=>({Navigator:'Navigator',Screen:'Screen'})},
    '../hooks/useAuth':{useAuth:()=>({isAuthenticated:authenticated})},
    '../components/MobilePermissionGate':{withMobilePermission:component=>component},
  },true).RootNavigator;
  for(const value of [false,true]) {
    authenticated=value;
    const screens=allNodes(rootNavigator()).filter(node=>node.type==='Screen');
    assert.equal(screens.filter(node=>node.props.name==='LanguageSettings').length,1);
    assert.equal(screens.some(node=>node.props.name==='Login'),!value);
    assert.equal(screens.some(node=>node.props.name==='ChangePassword'),value);
    assert.equal(screens.some(node=>node.props.name==='Main'),value);
  }
});

test('actual language provider persists selection and restores it on a new mount',async()=>{
  let saved='system',changed='',state=[],index=0,effects=[];
  const hooks={...react,createContext:()=>({Provider:'Provider'}),useContext:()=>{},
    useState:initial=>{const i=index++;if(!(i in state))state[i]=initial;return [state[i],value=>{state[i]=value;}];},
    useEffect:fn=>effects.push(fn)};
  const provider=load('i18n/LanguageContext.tsx',{
    react:hooks,'react-i18next':{useTranslation:()=>({i18n:{changeLanguage:async lang=>{changed=lang;}}})},
    '../utils/storage':{storage:{getLanguagePreference:async()=>saved,setLanguagePreference:async value=>{saved=value;}}},
    './i18n':{getSystemLanguage:()=>'en'},
  }).LanguageProvider;
  const render=()=>{index=0;effects=[];return provider({children:null});};
  let tree=render(); await tree.props.value.setLanguage('ja');
  assert.equal(saved,'ja');assert.equal(changed,'ja');
  state=[];tree=render();effects[0]();await new Promise(resolve=>setImmediate(resolve));
  tree=render();assert.equal(tree.props.value.preference,'ja');assert.equal(tree.props.value.activeLanguage,'ja');
  await tree.props.value.setLanguage('system');assert.equal(saved,'system');assert.equal(changed,'en');
});

test('actual Settings keeps change-password entry visible and still checks Edit permission',()=>{
  let user={idUser:21,detailedRights:{DOIMATKHAU:{CAN_EDIT:true}}};
  const navigated=[],alerts=[];
  const settings=load('screens/Settings/SettingsScreen.tsx',{
    react,'react-native':{View:'View',Text:'Text',TouchableOpacity:'TouchableOpacity',ScrollView:'ScrollView',Switch:'Switch',ActivityIndicator:'Spinner',StyleSheet:{create:x=>x},Alert:{alert:(...args)=>alerts.push(args)}},
    'react-i18next':{useTranslation:()=>({t:key=>key})},'@expo/vector-icons':{Ionicons:'Icon'},
    '@react-navigation/native':{useNavigation:()=>({navigate:route=>navigated.push(route)})},
    '../../hooks/useAuth':{useAuth:()=>({user})},'../../utils/mobilePermissions':permissions,
    '../../hooks/useTheme':{useTheme:()=>({colors:{},mode:'dark'})},'../../hooks/useLanguage':{useLanguage:()=>({preference:'vi',activeLanguage:'vi'})},
    '../../config':{APP_CONFIG:{env:'production',apiBaseUrl:'https://tryhardagain.com/api/api'}},
    '../../services/notificationService':{notificationService:{isRemotePushConfigured:()=>false,canRequestPermission:()=>false}},
    '../../constants/spacing':{spacing:{}},'../../constants/typography':{typography:{}},
  },true).SettingsScreen;
  const row=()=>allNodes(settings()).find(node=>node.type==='TouchableOpacity'&&textContent(node).includes('settings.changePassword'));
  assert.ok(row());row().props.onPress();assert.deepEqual(navigated,['ChangePassword']);
  user={idUser:21,rights:['DOIMATKHAU'],detailedRights:{DOIMATKHAU:{CAN_VIEW:true,CAN_EDIT:false}}};
  assert.ok(row());row().props.onPress();assert.equal(navigated.length,1);assert.equal(alerts.length,1);
});

test('actual change-password route guard requires Edit rather than only View',()=>{
  let user={detailedRights:{DOIMATKHAU:{CAN_VIEW:true,CAN_EDIT:false}}};
  const guard=load('components/MobilePermissionGate.tsx',{
    react,'react-i18next':{useTranslation:()=>({t:key=>key})},'../hooks/useAuth':{useAuth:()=>({user})},
    '../utils/mobilePermissions':permissions,'./AppErrorState':{AppErrorState:'Denied'},
  }).withMobilePermission('PasswordForm','DOIMATKHAU','Edit');
  assert.equal(guard({navigation:{goBack:()=>{}}}).type,'Denied');
  user.detailedRights.DOIMATKHAU.CAN_EDIT=true;
  assert.equal(guard({navigation:{goBack:()=>{}}}).type,'PasswordForm');
});

test('photo selection does not require broad library access; camera still requests permission',async()=>{
  let library=0,camera=0,granted=false;
  const picker=load('services/avatarPicker.ts',{'expo-image-picker':{
    requestMediaLibraryPermissionsAsync:()=>{throw new Error('Broad permission should not be requested');},
    requestCameraPermissionsAsync:async()=>({granted}),
    launchImageLibraryAsync:async()=>{library++;return {canceled:false,assets:[{base64:'/9j/AA==',mimeType:'image/heic'}]};},
    launchCameraAsync:async()=>{camera++;return {canceled:true};},
  }});
  const image=await picker.pickAvatar('library');assert.equal(library,1);assert.equal(image.uri,'data:image/jpeg;base64,/9j/AA==');
  await assert.rejects(picker.pickAvatar('camera'),/cameraPermission/);assert.equal(camera,0);
  granted=true;assert.equal(await picker.pickAvatar('camera'),null);assert.equal(camera,1);
  assert.throws(()=>picker.avatarFromResult({canceled:false,assets:[{}]}),/avatarReadFailed/);
  assert.throws(()=>picker.avatarFromResult({canceled:false,assets:[{base64:'A'.repeat(7*1024*1024)}]}),/avatarTooLarge/);
});

test('native permission remains available without an EAS project, without claiming remote push',()=>{
  const native={setNotificationHandler:()=>{}};
  const remote=load('services/remoteNotifications.ts',{
    'react-native':{Platform:{OS:'android'}},'expo-notifications':native,
    'expo-constants':{__esModule:true,default:{executionEnvironment:'standalone'},ExecutionEnvironment:{StoreClient:'storeClient'}},
  });
  assert.equal(remote.canUseNativeNotifications(),true);assert.equal(remote.canUseRemoteNotifications(),false);
  assert.equal(remote.getRemoteNotifications(),native);
});

test('device registration without a token never claims remote push is connected',async()=>{
  let registration;
  const service=load('services/notificationService.ts',{
    './remoteNotifications':{canUseNativeNotifications:()=>true,canUseRemoteNotifications:()=>false,getRemoteNotifications:()=>null},
    'react-native':{Platform:{OS:'android'},Linking:{}},'expo-device':{isDevice:true},
    'expo-constants':{__esModule:true,default:{}},
    '@react-native-async-storage/async-storage':{__esModule:true,default:{getItem:async()=>null}},
    '../api/meApi':{meApi:{registerDevice:async dto=>{registration=dto;}}},
    '../i18n/i18n':{__esModule:true,default:{language:'vi',t:key=>key}},'../utils/deviceIdentity':{getInstallationId:async()=>'installation-1'},
  }).notificationService;
  const status=await service.syncDeviceRegistration();
  assert.equal(registration.expoPushToken,null);assert.equal(status.isRegistered,false);assert.equal(status.pushEnabled,false);
  assert.equal(status.errorMessage,'mobileReview.pushNotConfigured');
});

test('push payload with a notification ID opens the scoped notification detail first',()=>{
  const {notificationDestination}=load('utils/requestContracts.ts');
  assert.deepEqual(plain(notificationDestination({notificationId:123,eventType:'SALARY_ADVANCE_APPROVED'})),{screen:'NotificationDetail',params:{notificationId:123}});
});

test('push build config rejects missing credentials and invalid Firebase app package',()=>{
  const configPath=path.resolve('app.config.js'),module={exports:{}};
  const environment={};let exists=true,packageName='com.wrong.app';
  vm.runInNewContext(fs.readFileSync(configPath,'utf8'),{module,exports:module.exports,__dirname:path.dirname(configPath),process:{env:environment},require:name=>name==='node:fs'?{existsSync:()=>exists,readFileSync:()=>JSON.stringify({client:[{client_info:{android_client_info:{package_name:packageName}}}]})}:require(name)});
  const configure=module.exports,config={android:{package:'com.qlns.hrmsmobile'},extra:{existing:true}};
  assert.equal(configure({config}).extra.existing,true);
  environment.HRMS_REQUIRE_REMOTE_PUSH='true';assert.throws(()=>configure({config}),/requires/);
  environment.EXPO_PUBLIC_EAS_PROJECT_ID='fake';assert.throws(()=>configure({config}),/UUID/);
  environment.EXPO_PUBLIC_EAS_PROJECT_ID='11111111-1111-1111-1111-111111111111';environment.GOOGLE_SERVICES_JSON='local.json';
  assert.throws(()=>configure({config}),/package/);
  packageName='com.qlns.hrmsmobile';assert.equal(configure({config}).android.googleServicesFile,'local.json');
  exists=false;assert.throws(()=>configure({config}),/does not exist/);
});

test('logout waits for an in-flight registration, deactivates last and blocks late registration',async()=>{
  let release,started;
  const began=new Promise(resolve=>{started=resolve;});
  const order=[];
  const service=load('services/notificationService.ts',{
    './remoteNotifications':{canUseNativeNotifications:()=>true,canUseRemoteNotifications:()=>false,getRemoteNotifications:()=>null},
    'react-native':{Platform:{OS:'android'},Linking:{}},'expo-device':{isDevice:true},
    'expo-constants':{__esModule:true,default:{}},
    '@react-native-async-storage/async-storage':{__esModule:true,default:{getItem:async()=>null}},
    '../api/meApi':{meApi:{registerDevice:async()=>{order.push('register-start');started();await new Promise(resolve=>{release=resolve;});order.push('register-finish');},deactivateDevice:async()=>{order.push('deactivate');}}},
    '../i18n/i18n':{__esModule:true,default:{language:'vi',t:key=>key}},'../utils/deviceIdentity':{getInstallationId:async()=>'installation-1'},
  }).notificationService;
  const sync=service.syncDeviceRegistration();await began;
  const logout=service.deactivateOnLogout();
  assert.deepEqual(order,['register-start']);
  assert.equal((await service.syncDeviceRegistration()).isRegistered,false);
  release();await sync;await logout;
  assert.deepEqual(order,['register-start','register-finish','deactivate']);
});
