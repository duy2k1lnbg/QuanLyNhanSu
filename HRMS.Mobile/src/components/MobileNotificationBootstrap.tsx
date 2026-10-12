import React, {useEffect, useState} from 'react';
import {AppState} from 'react-native';
import {useNavigation} from '@react-navigation/native';
import {useAuth} from '../hooks/useAuth';
import {useLanguage} from '../hooks/useLanguage';
import {canMobile} from '../utils/mobilePermissions';
import {notifyNotificationsChanged} from '../utils/notificationEvents';
import {notificationService} from '../services/notificationService';
import {NotificationPermissionModal} from './NotificationPermissionModal';

/** Mounted only within the authenticated Main screen. Never request OS permission automatically. */
export function MobileNotificationBootstrap() {
  const {user} = useAuth();
  const {activeLanguage} = useLanguage();
  const navigation = useNavigation();
  const allowed = canMobile(user, 'MOBILE_NOTIFICATION_VIEW');
  const [showPrompt, setShowPrompt] = useState(false);

  useEffect(() => {
    if (!allowed || !notificationService.canRequestPermission()) return;
    notificationService.resumeRegistration();
    let active = true;
    const sync = () => { if (active) void notificationService.syncDeviceRegistration().catch(()=>{}); };
    const unsubscribe = notificationService.subscribeResponse(response => {
      if (!active) return;
      notifyNotificationsChanged();
      notificationService.handleNotificationNavigation(navigation, response.notification.request.content.data);
    });
    void notificationService.checkLastNotificationResponse(navigation);
    sync();
    if (notificationService.isRemotePushConfigured()) {
      Promise.all([notificationService.hasPromptedForPermission(), notificationService.getPermissionStatus()])
        .then(([prompted, permission]) => {
          if (active && !prompted && permission.status !== 'GRANTED' && permission.canAskAgain) setShowPrompt(true);
        }).catch(()=>{});
    }
    const subscription = AppState.addEventListener('change', state => { if (state === 'active') sync(); });
    return () => { active = false; unsubscribe(); subscription.remove(); };
  }, [allowed, user?.idUser, navigation, activeLanguage]);

  const dismiss = () => {
    setShowPrompt(false);
    void notificationService.setPromptedForPermission();
  };
  const accept = async () => {
    dismiss();
    const permission = await notificationService.requestPermission();
    if (permission.status === 'GRANTED') await notificationService.syncDeviceRegistration();
  };
  return <NotificationPermissionModal visible={allowed && showPrompt} onAccept={()=> { void accept().catch(()=>{}); }} onDismiss={dismiss}/>;
}
