import React from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { canMobile, canViewRequests, MobileAction } from '../utils/mobilePermissions';
import { AppErrorState } from './AppErrorState';
export function withMobilePermission(Screen: React.ComponentType<any>, code: string, action: MobileAction = 'View') {
  return function GuardedScreen(props: any) {
    const { user }=useAuth(); const { t }=useTranslation();
    const allowed=code==='REQUESTS' ? canViewRequests(user) : canMobile(user,code,action);
    return allowed ? <Screen {...props}/> : <AppErrorState message={t('errors.forbidden')} onRetry={()=>props.navigation.goBack()}/>;
  };
}
