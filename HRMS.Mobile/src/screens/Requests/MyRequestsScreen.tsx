import React, { useState, useCallback } from 'react';
import { View, Text, ScrollView, RefreshControl, TouchableOpacity, StyleSheet } from 'react-native';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { UnifiedRequestItem } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge, BadgeVariant } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppErrorState } from '../../components/AppErrorState';
import { mapApiError } from '../../utils/errorMapper';
import { formatDate,formatCurrency } from '../../utils/formatters';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

const profileTypes = ['PERSONAL_INFO','PROFILE','AVATAR','INSURANCE_INFO','DEPENDENTS'];
export const MyRequestsScreen: React.FC = () => {
  const { t }=useTranslation(); const navigation=useNavigation(); const { colors }=useTheme(); const { activeLanguage }=useLanguage();
  const [items,setItems]=useState<UnifiedRequestItem[]>([]), [filter,setFilter]=useState('ALL');
  const [loading,setLoading]=useState(true), [refreshing,setRefreshing]=useState(false), [error,setError]=useState<string|null>(null);
  const reload=useCallback(async(refresh=false)=>{
    refresh ? setRefreshing(true):setLoading(true); setError(null);
    try { const result=await meApi.getAllRequests(); setItems(result.items || []); }
    catch(e) {setError(mapApiError(e));} finally {setLoading(false);setRefreshing(false);}
  },[]);
  useFocusEffect(useCallback(()=>{reload();},[reload]));
  const filtered=items.filter(item=>filter==='ALL' || (filter==='PROFILE' ? profileTypes.includes(item.requestType) : item.requestType===filter));
  const label=(type:string)=>t(`mobileReview.type${type}`,{defaultValue:t('mobileReview.typePROFILE')});
  const statusVariant=(status:string):BadgeVariant=>status==='REJECTED' ? 'danger' : ['APPLIED','DISBURSED'].includes(status) ? 'success' : status==='APPROVED' ? 'info' : status==='CANCELLED' ? 'default':'warning';
  return <View style={[styles.container,{backgroundColor:colors.background}]}>
    <AppHeader title={t('requests.title')} showBack onBack={()=>navigation.goBack()}/>
    <ScrollView horizontal style={{flexGrow:0}} contentContainerStyle={{padding:spacing.md,gap:spacing.sm}} showsHorizontalScrollIndicator={false}>
      {['ALL','LEAVE','ATTENDANCE','OVERTIME','ADVANCE','PROFILE'].map(type=><TouchableOpacity key={type} onPress={()=>setFilter(type)} style={[styles.filter,{backgroundColor:filter===type ? colors.primary:colors.surfaceCard}]}>
        <Text style={[typography.captionBold,{color:filter===type ? '#fff':colors.text}]}>{type==='ALL' ? t('requests.allRequests'):label(type)}</Text>
      </TouchableOpacity>)}
    </ScrollView>
    <ScrollView contentContainerStyle={styles.content} refreshControl={<RefreshControl refreshing={refreshing} onRefresh={()=>reload(true)} tintColor={colors.primary}/> }>
      {loading ? <AppLoading/> : error ? <AppErrorState message={error} onRetry={()=>reload()}/> : filtered.length===0 ? <AppEmptyState title={t('mobileReview.noRequests')} message={t('mobileReview.requestHistoryHint')} icon="file-tray-outline"/> : filtered.map(item=><AppCard key={`${item.requestType}-${item.id}`} style={{marginBottom:spacing.md}}>
        <View style={styles.row}><Text style={[typography.bodyBold,{color:colors.text,flex:1,marginRight:spacing.sm}]}>{label(item.requestType)}</Text><AppBadge label={t(`mobileReview.status${item.status}`)} variant={statusVariant(item.status)}/></View>
        <Text style={[typography.caption,{color:colors.textSecondary,marginTop:spacing.sm}]}>#{item.id} · {(item.dateRange || item.createdAt || '').split(' - ').map(value=>formatDate(value,activeLanguage)).join(' – ')}</Text>
        {item.amount!=null && <Text style={[typography.bodyBold,{color:colors.text,marginTop:spacing.sm}]}>{formatCurrency(item.amount,activeLanguage)}</Text>}
        {item.hours!=null && <Text style={[typography.body,{color:colors.text}]}>{t('requests.hours')}: {item.hours}</Text>}
        {item.totalDays!=null && <Text style={[typography.body,{color:colors.text}]}>{t('requests.totalDays')}: {item.totalDays}</Text>}
        {item.reason ? <Text style={[typography.body,{color:colors.text,marginTop:spacing.sm}]}>{item.reason}</Text>:null}
        {item.rejectionReason ? <Text style={[typography.caption,{color:colors.warning,marginTop:spacing.sm}]}>{t('requests.note')}: {item.rejectionReason}</Text>:null}
      </AppCard>)}
    </ScrollView>
  </View>;
};
const styles=StyleSheet.create({container:{flex:1},content:{padding:spacing.md,paddingBottom:spacing.xxl},row:{flexDirection:'row',alignItems:'center'},filter:{paddingHorizontal:spacing.md,paddingVertical:spacing.sm,borderRadius:12}});
