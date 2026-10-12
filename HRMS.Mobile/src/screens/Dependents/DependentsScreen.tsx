import React, { useState, useCallback } from 'react';
import { View, Text, ScrollView, RefreshControl, Modal, Alert, TouchableOpacity, StyleSheet } from 'react-native';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { meApi } from '../../api/meApi';
import { DependentDto, AllProfileQuotasDto } from '../../types/me';
import { canMobile } from '../../utils/mobilePermissions';
import { localDate, dependentPayload } from '../../utils/requestContracts';
import { mapApiError } from '../../utils/errorMapper';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppButton } from '../../components/AppButton';
import { AppTextInput } from '../../components/AppTextInput';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppErrorState } from '../../components/AppErrorState';
import { AppEmptyState } from '../../components/AppEmptyState';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

function displayMonth(value?:string|number) {const raw=String(value || '').replace('-','');return raw.length===6 ? `${raw.slice(0,4)}-${raw.slice(4)}`:raw;}
const emptyDependent=():DependentDto=>({hoTen:'',moiQuanHe:'',loaiGiayTo:'GIAY_KHAI_SINH',thangBatDau:localDate().slice(0,7),action:'ADD'});
export const DependentsScreen:React.FC=()=>{
  const {t}=useTranslation(), navigation=useNavigation(), {colors}=useTheme(), {user}=useAuth();
  const [items,setItems]=useState<DependentDto[]>([]), [quota,setQuota]=useState<AllProfileQuotasDto|null>(null);
  const [loading,setLoading]=useState(true), [refreshing,setRefreshing]=useState(false), [error,setError]=useState<string|null>(null);
  const [draft,setDraft]=useState<DependentDto>(emptyDependent), [reason,setReason]=useState(''), [visible,setVisible]=useState(false), [saving,setSaving]=useState(false);
  const allowed=canMobile(user,'MOBILE_REQUEST_DEPENDENT','Add');
  const reload=useCallback(async(refresh=false)=>{
    refresh ? setRefreshing(true):setLoading(true); setError(null);
    try {
      const results=await Promise.allSettled([meApi.getDependents(),meApi.getProfileQuotas()]);
      if(results[0].status==='rejected') throw results[0].reason;
      setItems(results[0].value); setQuota(results[1].status==='fulfilled' ? results[1].value:null);
    } catch(e) {setError(mapApiError(e));} finally {setLoading(false);setRefreshing(false);}
  },[]);
  useFocusEffect(useCallback(()=>{reload();},[reload]));
  const open=(item?:DependentDto,terminate=false)=>{
    setDraft(item ? {...item,thangBatDau:displayMonth(item.thangBatDau),thangKetThuc:terminate ? localDate().slice(0,7):displayMonth(item.thangKetThuc),action:terminate ? 'TERMINATE':'UPDATE'}:emptyDependent());
    setReason('');setVisible(true);
  };
  const field=(key:keyof DependentDto,label:string,required=false)=><AppTextInput key={key} label={`${t('mobileReview.'+label)}${required ? ' *':''}`} value={String(draft[key] || '')} onChangeText={value=>setDraft({...draft,[key]:value})}/>;
  const save=async()=>{
    if(saving || !allowed) return;
    if(!draft.hoTen.trim() || !draft.moiQuanHe.trim() || !reason.trim()) {Alert.alert(t('common.error'),t('mobileReview.requiredFields'));return;}
    setSaving(true);
    try {dependentPayload(draft); const result=await meApi.proposeDependents([draft],reason.trim());Alert.alert(t('common.success'),t(result.isDirectEdit ? 'mobileReview.savedDirect':'mobileReview.sentForApproval'));setVisible(false);await reload(true);}
    catch(e) {Alert.alert(t('common.error'),mapApiError(e));} finally {setSaving(false);}
  };
  return <View style={[styles.container,{backgroundColor:colors.background}]}>
    <AppHeader title={t('nav.dependents')} showBack onBack={()=>navigation.goBack()}/>
    <ScrollView contentContainerStyle={styles.content} refreshControl={<RefreshControl refreshing={refreshing} onRefresh={()=>reload(true)} tintColor={colors.primary}/>}>
      <AppCard style={styles.card}><Text style={[typography.body,{color:colors.text}]}>{t(quota?.dependents?.isFirstEditAvailable ? 'mobileReview.firstEdit':'mobileReview.approvalRequired')}</Text>
        <Text style={[typography.caption,{color:colors.textSecondary,marginVertical:spacing.md}]}>{t('mobileReview.dependentTaxHint')}</Text>
        <AppButton title={t('mobileReview.addDependent')} disabled={!allowed} onPress={()=>open()}/>
      </AppCard>
      {loading ? <AppLoading/>:error ? <AppErrorState message={error} onRetry={()=>reload()}/>:items.length===0 ? <AppEmptyState icon="people-outline" title={t('mobileReview.noDependents')}/>:items.map(item=><AppCard key={item.id} style={styles.card}>
        <Text style={[typography.bodyBold,{color:colors.text}]}>{item.hoTen}</Text><Text style={[typography.body,{color:colors.textSecondary,marginTop:spacing.sm}]}>{item.moiQuanHe}</Text>
        <Text style={[typography.caption,{color:colors.textSecondary,marginVertical:spacing.sm}]}>{displayMonth(item.thangBatDau)} — {displayMonth(item.thangKetThuc) || t('mobileReview.noExpiry')}</Text>
        <AppBadge label={item.thangKetThuc ? t('mobileReview.hasEndMonth'):t('mobileReview.noExpiry')} variant="info"/>
        <View style={styles.buttons}><View style={styles.grow}><AppButton size="sm" title={t('mobileReview.editDependent')} disabled={!allowed} onPress={()=>open(item)}/></View><View style={styles.grow}><AppButton size="sm" variant="outline" title={t('mobileReview.endDependent')} disabled={!allowed} onPress={()=>open(item,true)}/></View></View>
      </AppCard>)}
    </ScrollView>
    <Modal visible={visible} animationType="slide" onRequestClose={()=>!saving && setVisible(false)}>
      <View style={[styles.container,{backgroundColor:colors.background}]}><AppHeader title={t(draft.action==='TERMINATE' ? 'mobileReview.endDependent':draft.id ? 'mobileReview.editDependent':'mobileReview.addDependent')} showBack onBack={()=>!saving && setVisible(false)}/>
        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <Text style={[typography.caption,{color:colors.textSecondary,marginBottom:spacing.md}]}>{t('mobileReview.dependentTaxHint')}</Text>
          {draft.action!=='TERMINATE' && <>{field('hoTen','dependentName',true)}{field('moiQuanHe','relationship',true)}{field('ngaySinh','birthDate')}<Text style={[typography.caption,{color:colors.textSecondary}]}>{t('mobileReview.documentType')}</Text><View style={[styles.buttons,{flexWrap:'wrap',marginBottom:spacing.md}]}>{['GIAY_KHAI_SINH','CCCD','HO_CHIEU'].map(type=><TouchableOpacity key={type} onPress={()=>setDraft({...draft,loaiGiayTo:type})} style={{padding:spacing.sm,borderRadius:8,backgroundColor:draft.loaiGiayTo===type ? colors.primary:colors.surfaceCard}}><Text style={[typography.caption,{color:draft.loaiGiayTo===type ? '#fff':colors.text}]}>{t('mobileReview.document'+type)}</Text></TouchableOpacity>)}</View>{field('soGiayTo','documentNumber')}{field('cccd','identityNumber')}{field('maSoThueNpt','taxNumber')}{field('thangBatDau','startMonth',true)}</>}
          {field('thangKetThuc','endMonth',draft.action==='TERMINATE')}
          <AppTextInput label={t('mobileReview.reason')+' *'} value={reason} onChangeText={setReason} multiline numberOfLines={3}/>
          <View style={styles.buttons}><View style={styles.grow}><AppButton title={t('common.cancel')} variant="outline" disabled={saving} onPress={()=>setVisible(false)}/></View><View style={styles.grow}><AppButton title={t('common.save')} loading={saving} disabled={!allowed} onPress={save}/></View></View>
        </ScrollView>
      </View>
    </Modal>
  </View>;
};
const styles=StyleSheet.create({container:{flex:1},content:{padding:spacing.md,paddingBottom:spacing.xxl},card:{marginBottom:spacing.md},buttons:{flexDirection:'row',gap:spacing.sm,marginTop:spacing.md},grow:{flex:1}});
