import {Platform} from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
let pending: Promise<string> | null=null;
export function getInstallationId(): Promise<string> {
  if (!pending) {
    pending=(async()=>{
      let id=await AsyncStorage.getItem('hrms_mobile_device_id');
      if (!id) {
        id=`inst_${Platform.OS}_${Date.now().toString(36)}_${Math.random().toString(36).slice(2)}${Math.random().toString(36).slice(2)}`;
        await AsyncStorage.setItem('hrms_mobile_device_id',id);
      }
      return id;
    })().catch(error=>{pending=null;throw error;});
  }
  return pending;
}
