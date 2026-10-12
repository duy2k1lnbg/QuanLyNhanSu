import React, { useState, useCallback } from 'react';
import { View, Text, ScrollView, RefreshControl, Modal, Alert, Switch, StyleSheet } from 'react-native';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { meApi } from '../../api/meApi';
import { InsuranceDto, AllProfileQuotasDto } from '../../types/me';
import { canMobile } from '../../utils/mobilePermissions';
import { isoDate, localDate } from '../../utils/requestContracts';
import { mapApiError } from '../../utils/errorMapper';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppButton } from '../../components/AppButton';
import { AppTextInput } from '../../components/AppTextInput';
import { AppLoading } from '../../components/AppLoading';
import { AppErrorState } from '../../components/AppErrorState';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const InsuranceScreen:React.FC=()=>{
  const {t}=useTranslation(), navigation=useNavigation(), {colors}=useTheme(), {user}=useAuth();
  const [insurance,setInsurance]=useState<InsuranceDto|null>(null), [quota,setQuota]=useState<AllProfileQuotasDto|null>(null);
  const [loading,setLoading]=useState(true), [refreshing,setRefreshing]=useState(false), [error,setError]=useState<string|null>(null);
  const [visible,setVisible]=useState(false), [saving,setSaving]=useState(false), [salary,setSalary]=useState(''), [region,setRegion]=useState(''), [date,setDate]=useState(localDate), [reason,setReason]=useState('');
  const [flags,setFlags]=useState({thamGiaBhxh:false,thamGiaBhyt:false,thamGiaBhtn:false,thamGiaTnldBnn:false});
  const allowed=canMobile(user,'MOBILE_REQUEST_INSURANCE','Add');
  const reload=useCallback(async(refresh=false)=>{
    refresh ? setRefreshing(true):setLoading(true);setError(null);
    try {const results=await Promise.allSettled([meApi.getInsurance(),meApi.getProfileQuotas()]);if(results[0].status==='rejected') throw results[0].reason;setInsurance(results[0].value);setQuota(results[1].status==='fulfilled' ? results[1].value:null);}
    catch(e) {setError(mapApiError(e));} finally {setLoading(false);setRefreshing(false);}
  },[]);
  useFocusEffect(useCallback(()=>{reload();},[reload]));
  const open=()=>{
    const current=insurance?.participations?.find(item=>isoDate(item.ngayBatDau)<=localDate() && (!item.ngayKetThuc || isoDate(item.ngayKetThuc)>=localDate()));
    setSalary(String(current?.luongDongBhxhRieng || ''));setRegion(String(current?.vungLuong || ''));setDate(localDate());setReason('');
    setFlags({thamGiaBhxh:current?.thamGiaBhxh ?? false,thamGiaBhyt:current?.thamGiaBhyt ?? false,thamGiaBhtn:current?.thamGiaBhtn ?? false,thamGiaTnldBnn:current?.thamGiaTnldBnn ?? false});setVisible(true);
  };
  const save=async()=>{
    if(saving || !allowed) return;
    if(!/^[1-4]$/.test(region) || !reason.trim() || (salary && !/^\d+$/.test(salary))) {Alert.alert(t('common.error'),t('mobileReview.insuranceValidation'));return;}
    setSaving(true);
    try {const result=await meApi.proposeInsurance({...flags,vungLuong:region,luongDongBhxhRieng:salary ? Number(salary):undefined,ngayBatDau:isoDate(date),reason:reason.trim()});Alert.alert(t('common.success'),t(result.isDirectEdit ? 'mobileReview.savedDirect':'mobileReview.sentForApproval'));setVisible(false);await reload(true);}
    catch(e) {Alert.alert(t('common.error'),mapApiError(e));} finally {setSaving(false);}
  };
  return <View style={[styles.container,{backgroundColor:colors.background}]}>
    <AppHeader title={t('nav.insurance')} showBack onBack={()=>navigation.goBack()}/>
    <ScrollView contentContainerStyle={styles.content} refreshControl={<RefreshControl refreshing={refreshing} onRefresh={()=>reload(true)} tintColor={colors.primary}/>}>
      {loading ? <AppLoading/>:error ? <AppErrorState message={error} onRetry={()=>reload()}/>:<>
        <AppCard style={styles.card}><Text style={[typography.bodyBold,{color:colors.text}]}>{insurance?.sobh || t('common.notUpdated')}</Text><Text style={[typography.body,{color:colors.textSecondary,marginTop:spacing.sm}]}>{insurance?.noicap || t('common.notUpdated')}</Text>
          <Text style={[typography.caption,{color:colors.textSecondary,marginVertical:spacing.md}]}>{t(quota?.insuranceInfo?.isFirstEditAvailable ? 'mobileReview.firstEdit':'mobileReview.approvalRequired')}</Text>
          <AppButton title={t('mobileReview.proposeInsurance')} disabled={!allowed} onPress={open}/>
        </AppCard>
        {(insurance?.participations || []).map(item=><AppCard key={item.id} style={styles.card}><Text style={[typography.bodyBold,{color:colors.text}]}>{t('mobileReview.insuranceRegion',{region:item.vungLuong})}</Text><Text style={[typography.caption,{color:colors.textSecondary,marginTop:spacing.sm}]}>{item.ngayBatDau} — {item.ngayKetThuc || t('mobileReview.noExpiry')}</Text>
          {(['thamGiaBhxh','thamGiaBhyt','thamGiaBhtn','thamGiaTnldBnn'] as const).map(key=><Text key={key} style={[typography.body,{color:colors.text,marginTop:spacing.xs}]}>{t('mobileReview.'+key)}: {t(item[key] ? 'common.yes':'common.no')}</Text>)}
        </AppCard>)}
        {insurance?.unionParticipation && <AppCard style={styles.card}><Text style={[typography.bodyBold,{color:colors.text}]}>{t('mobileReview.union')}</Text><Text style={[typography.body,{color:colors.text}]}>{t('mobileReview.unionMember')}: {t(insurance.unionParticipation.laDoanVien ? 'common.yes':'common.no')}</Text><Text style={[typography.caption,{color:colors.textSecondary}]}>{insurance.unionParticipation.ngayGiaNhap} — {insurance.unionParticipation.ngayKetThuc || t('mobileReview.noExpiry')}</Text></AppCard>}
        {(insurance?.movements || []).map(item=><AppCard key={'movement-'+item.id} style={styles.card}><Text style={[typography.bodyBold,{color:colors.text}]}>{t('mobileReview.movement'+item.loai,{defaultValue:t('mobileReview.insuranceMovement')})}</Text><Text style={[typography.body,{color:colors.textSecondary}]}>{item.ngayHieuLuc} · {t('mobileReview.status'+item.trangThai,{defaultValue:item.trangThai})}</Text><Text style={[typography.body,{color:colors.text}]}>{item.lyDo}</Text></AppCard>)}
        {canMobile(user,'MOBILE_REQUEST_DEPENDENT') && <AppButton title={t('nav.dependents')} variant="outline" onPress={()=>navigation.navigate('Dependents' as never)}/>}
      </>}
    </ScrollView>
    <Modal visible={visible} animationType="slide" onRequestClose={()=>!saving && setVisible(false)}><View style={[styles.container,{backgroundColor:colors.background}]}>
      <AppHeader title={t('mobileReview.proposeInsurance')} showBack onBack={()=>!saving && setVisible(false)}/>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <Text style={[typography.caption,{color:colors.textSecondary,marginBottom:spacing.md}]}>{t('mobileReview.insuranceHistoryHint')}</Text>
        <AppTextInput label={t('mobileReview.region')+' *'} value={region} onChangeText={setRegion} keyboardType="numeric" placeholder="1–4"/>
        <AppTextInput label={t('mobileReview.insuranceSalary')} value={salary} onChangeText={setSalary} keyboardType="numeric"/>
        <AppTextInput label={t('mobileReview.effectiveDate')+' *'} value={date} onChangeText={setDate} placeholder="YYYY-MM-DD"/>
        {(Object.keys(flags) as (keyof typeof flags)[]).map(key=><View key={key} style={styles.row}><Text style={[typography.body,{color:colors.text,flex:1}]}>{t('mobileReview.'+key)}</Text><Switch value={flags[key]} onValueChange={value=>setFlags({...flags,[key]:value})}/></View>)}
        <AppTextInput label={t('mobileReview.reason')+' *'} value={reason} onChangeText={setReason} multiline numberOfLines={3}/>
        <AppButton title={t('common.save')} loading={saving} disabled={!allowed} onPress={save}/>
      </ScrollView>
    </View></Modal>
  </View>;
};
const styles=StyleSheet.create({container:{flex:1},content:{padding:spacing.md,paddingBottom:spacing.xxl},card:{marginBottom:spacing.md},row:{flexDirection:'row',alignItems:'center',marginBottom:spacing.md}});
