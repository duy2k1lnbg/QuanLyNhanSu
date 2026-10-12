import { useAuth } from '../../hooks/useAuth';
import { canMobile } from '../../utils/mobilePermissions';
import { useFocusEffect } from '@react-navigation/native';
import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  Modal,
  Alert,
  Image,
  Platform,
  Linking,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { pickAvatar, avatarFromResult, AvatarPickerAction } from '../../services/avatarPicker';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { Ionicons } from '@expo/vector-icons';
import { RootStackParamList } from '../../navigation/types';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { ProfileDto, AllProfileQuotasDto } from '../../types/me';
import { AppAvatar } from '../../components/AppAvatar';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppButton } from '../../components/AppButton';
import { AppTextInput } from '../../components/AppTextInput';
import { AppLoading } from '../../components/AppLoading';
import { AppErrorState } from '../../components/AppErrorState';
import { AppDivider } from '../../components/AppDivider';
import { formatDate } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { getLocalizedEmploymentStatus } from '../../utils/statusMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const ProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [quotas, setQuotas] = useState<AllProfileQuotasDto | null>(null);
  const [proposedAvatar, setProposedAvatar] = useState<{ hasProposed: boolean; avatarBase64?: string; createdAt?: string } | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Edit personal info states
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [editName, setEditName] = useState('');
  const [editGender, setEditGender] = useState('Nam');
  const [editBirthDate, setEditBirthDate] = useState('');
  const [editCccd, setEditCccd] = useState('');
  const [editPhone, setEditPhone] = useState('');
  const [editEmail, setEditEmail] = useState('');
  const [editAddress, setEditAddress] = useState('');
  const [updating, setUpdating] = useState(false);

  // Avatar states
  const [avatarActionModalVisible, setAvatarActionModalVisible] = useState(false);
  const [avatarViewModalVisible, setAvatarViewModalVisible] = useState(false);
  const [avatarPreviewModalVisible, setAvatarPreviewModalVisible] = useState(false);
  const [previewAvatarBase64, setPreviewAvatarBase64] = useState<string | null>(null);
  const [newAvatarRawBase64, setNewAvatarRawBase64] = useState<string | null>(null);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);
  const [pendingAvatarAction, setPendingAvatarAction] = useState<AvatarPickerAction | 'view' | null>(null);
  const pendingAvatarActionRef = useRef<AvatarPickerAction | 'view' | null>(null);

  const fetchProfile = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const [profileData, quotaData, proposedData] = await Promise.allSettled([
        meApi.getProfile(),
        meApi.getProfileQuotas(),
        meApi.getProposedAvatar(),
      ]);

      if (profileData.status === 'rejected') throw profileData.reason;
      if (profileData.status === 'fulfilled') {
        setProfile(profileData.value);
      }
      if (quotaData.status === 'fulfilled') {
        setQuotas(quotaData.value);
      }
      if (proposedData.status === 'fulfilled') {
        setProposedAvatar(proposedData.value);
      }
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { fetchProfile(); }, [fetchProfile]));

  const showAvatar = useCallback((image: ReturnType<typeof avatarFromResult>) => {
    if (!image) return;
    setNewAvatarRawBase64(image.base64);
    setPreviewAvatarBase64(image.uri);
    setAvatarPreviewModalVisible(true);
  }, []);

  const finishAvatarAction = async () => {
    const action = pendingAvatarActionRef.current;
    if (!action) return;
    pendingAvatarActionRef.current = null;
    setPendingAvatarAction(null);
    if (action === 'view') { setAvatarViewModalVisible(true); return; }
    if (!canMobile(user,'MOBILE_PROFILE_EDIT','Edit')) {
      Alert.alert(t('common.error'), t('mobileReview.permissionRefreshHelp')); return;
    }
    try { showAvatar(await pickAvatar(action)); }
    catch (error: any) {
      const message = error?.message?.startsWith('mobileReview.')
        ? t(error.message) : t('mobileReview.avatarReadFailed');
      Alert.alert(t('common.error'), message, [
        {text:t('common.cancel'), style:'cancel'},
        ...(error?.message === 'mobileReview.cameraPermission' ? [{
          text:t('settings.openSettings'), onPress:()=> { Linking.openSettings().catch(()=>{}); },
        }] : []),
      ]);
    }
  };

  const queueAvatarAction = (action: AvatarPickerAction | 'view') => {
    if (action !== 'view' && !canMobile(user,'MOBILE_PROFILE_EDIT','Edit')) {
      Alert.alert(t('common.error'), t('mobileReview.permissionRefreshHelp')); return;
    }
    pendingAvatarActionRef.current = action;
    setPendingAvatarAction(action);
    setAvatarActionModalVisible(false);
  };

  useEffect(() => {
    if (Platform.OS === 'ios' || avatarActionModalVisible || !pendingAvatarAction) return;
    const frame = requestAnimationFrame(() => { void finishAvatarAction(); });
    return () => cancelAnimationFrame(frame);
  }, [avatarActionModalVisible, pendingAvatarAction]);

  useEffect(() => {
    let active = true;
    if (Platform.OS === 'android' && canMobile(user,'MOBILE_PROFILE_EDIT','Edit')) {
      ImagePicker.getPendingResultAsync().then(result => {
        if (active) showAvatar(avatarFromResult(result));
      }).catch(()=> { if (active) Alert.alert(t('common.error'), t('mobileReview.avatarReadFailed')); });
    }
    return () => { active = false; };
  }, [user?.idUser, showAvatar]);

  const handleUploadAvatarConfirm = async () => {
    if (!canMobile(user,'MOBILE_PROFILE_EDIT','Edit')) return;
    if (!newAvatarRawBase64 || uploadingAvatar) return;
    setUploadingAvatar(true);
    try {
      const res = await meApi.uploadAvatar(newAvatarRawBase64);
      if (res.isDirectEdit) {
        Alert.alert(t('common.success'), t('mobileReview.savedDirect'));
      } else {
        Alert.alert(
          t('common.success'),
          t('mobileReview.sentForApproval')
        );
      }
      setAvatarPreviewModalVisible(false);
      setPreviewAvatarBase64(null);
      setNewAvatarRawBase64(null);
      fetchProfile(true);
    } catch (error: any) {
      Alert.alert(t('common.error'), mapApiError(error));
    } finally {
      setUploadingAvatar(false);
    }
  };

  const handleSavePersonalInfo = async () => {
    if (!canMobile(user,'MOBILE_PROFILE_EDIT','Edit')) return;
    if (updating) return;
    setUpdating(true);
    try {
      const res = await meApi.updateProfile({
        hoten: editName.trim() || undefined,
        gioitinh: editGender,
        ngaysinh: editBirthDate.trim() || undefined,
        cccd: editCccd.trim() || undefined,
        dienthoai: editPhone.trim() || undefined,
        email: editEmail.trim() || undefined,
        diachi: editAddress.trim() || undefined,
      });

      Alert.alert(t('common.success'), res.isDirectEdit ? t('mobileReview.savedDirect') : t('mobileReview.sentForApproval'));
      setEditModalVisible(false);
      fetchProfile(true);
    } catch (error: any) {
      Alert.alert(t('common.error'), mapApiError(error));
    } finally {
      setUpdating(false);
    }
  };

  if (loading && !refreshing) {
    return <AppLoading />;
  }

  if (errorMessage && !profile) {
    return <AppErrorState message={errorMessage} onRetry={() => fetchProfile()} />;
  }

  const renderInfoRow = (
    icon: keyof typeof Ionicons.glyphMap,
    label: string,
    value?: string | number | null
  ) => {
    const displayValue = value !== undefined && value !== null && value !== ''
      ? String(value)
      : t('common.notUpdated');

    return (
      <View style={styles.infoRow}>
        <Ionicons name={icon} size={20} color={colors.primary} style={styles.infoIcon} />
        <View style={styles.infoTextWrapper}>
          <Text style={[typography.caption, { color: colors.textSecondary }]}>
            {label}
          </Text>
          <Text style={[typography.bodyBold, { color: colors.text, marginTop: 2 }]}>
            {displayValue}
          </Text>
        </View>
      </View>
    );
  };

  const avatarSource = profile?.avatarBase64
    ? { uri: profile.avatarBase64.startsWith('data:') ? profile.avatarBase64 : `data:image/jpeg;base64,${profile.avatarBase64}` }
    : null;

  return (
    <ScrollView
      style={[styles.container, { backgroundColor: colors.background }]}
      contentContainerStyle={[
        styles.content,
        { paddingTop: Math.max(insets.top, spacing.md) },
      ]}
      refreshControl={
        <RefreshControl
          refreshing={refreshing}
          onRefresh={() => fetchProfile(true)}
          colors={[colors.primary]}
          tintColor={colors.primary}
        />
      }
    >
      {/* 1. Header Card with Avatar */}
      <AppCard style={styles.heroCard}>
        <TouchableOpacity
          activeOpacity={0.8}
          onPress={() => setAvatarActionModalVisible(true)}
          style={styles.avatarTouchWrapper}
        >
          <View style={styles.avatarWrapper}>
            <AppAvatar
              name={profile?.hoten}
              avatarBase64={profile?.avatarBase64}
              size="xl"
            />
            <View style={[styles.avatarCameraBadge, { backgroundColor: colors.primary }]}>
              <Ionicons name="camera" size={14} color="#FFFFFF" />
            </View>
          </View>
        </TouchableOpacity>

        {proposedAvatar?.hasProposed && (
          <TouchableOpacity
            style={[styles.pendingAvatarBanner, { backgroundColor: 'rgba(245, 158, 11, 0.1)', borderColor: colors.warning }]}
            onPress={() => setAvatarActionModalVisible(true)}
          >
            <Ionicons name="hourglass-outline" size={16} color={colors.warning} />
            <Text style={[typography.captionBold, { color: colors.warning, marginLeft: 6 }]}>
              {t('mobileReview.avatarPending')}
            </Text>
          </TouchableOpacity>
        )}

        <Text style={[typography.h2, { color: colors.text, marginTop: spacing.md }]}>
          {profile?.hoten || '---'}
        </Text>
        <View style={styles.badgeRow}>
          <AppBadge label={`${t('profile.employeeId')}: ${profile?.employeeCode || profile?.manv || '---'}`} />
          {profile?.trangThaiLaoDong ? (
            <AppBadge
              label={getLocalizedEmploymentStatus(profile.trangThaiLaoDong)}
              variant={profile.trangThaiLaoDong.toLowerCase().includes('làm việc') || profile.trangThaiLaoDong.toLowerCase().includes('active') ? 'success' : 'default'}
            />
          ) : null}
        </View>

        {/* Quick links to Contract, Insurance, Dependents, Settings */}
        <View style={styles.linksRow}>
          {canMobile(user,'MOBILE_CONTRACT_VIEW') && (<TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Contract')}
          >
            <Ionicons name="document-text-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]} numberOfLines={1}>
              {t('nav.contract')}
            </Text>
          </TouchableOpacity>)}

          {canMobile(user,'MOBILE_INSURANCE_VIEW') && (<TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Insurance')}
          >
            <Ionicons name="medkit-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]} numberOfLines={1}>
              {t('nav.insurance')}
            </Text>
          </TouchableOpacity>)}

          {canMobile(user,'MOBILE_REQUEST_DEPENDENT') && (<TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Dependents' as any)}
          >
            <Ionicons name="people-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]} numberOfLines={1}>
              {t('nav.dependents')}
            </Text>
          </TouchableOpacity>)}

          <TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Settings')}
          >
            <Ionicons name="settings-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]} numberOfLines={1}>
              {t('nav.settings')}
            </Text>
          </TouchableOpacity>
        </View>
      </AppCard>

      {/* 3. Work Information (HR-Owned, Read-Only per Rule 32) */}
      <Text style={[typography.h3, { color: colors.text, marginTop: spacing.lg, marginBottom: spacing.sm }]}>
        {t('profile.generalInfo')}
      </Text>
      <AppCard>
        {renderInfoRow('business-outline', t('profile.department'), profile?.tenPhongBan)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('layers-outline', t('profile.division'), profile?.tenBoPhan)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('ribbon-outline', t('profile.position'), profile?.tenChucVu)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('school-outline', t('profile.education'), profile?.tenTrinhDo)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('calendar-outline', t('profile.hireDate'), formatDate(profile?.ngayVaoLam, activeLanguage))}
      </AppCard>

      {/* 4. Personal Information (Editable for phone, email, address per Rule 32) */}
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.lg, marginBottom: spacing.sm }}>
        <Text style={[typography.h3, { color: colors.text }]}>
          {t('profile.personalInfo')}
        </Text>
        <TouchableOpacity
          style={{ flexDirection: 'row', alignItems: 'center', gap: 4 }}
          onPress={() => {
            const cleanVal = (val?: string | null) => (val && val !== 'Chưa cập nhật' ? val : '');
            setEditName(cleanVal(profile?.hoten));
            setEditGender(profile?.gioitinh === 'Nữ' ? 'Nữ' : 'Nam');
            setEditBirthDate(cleanVal(profile?.ngaysinh));
            setEditCccd(cleanVal(profile?.cccd));
            setEditPhone(cleanVal(profile?.dienthoai));
            setEditEmail(cleanVal(profile?.email));
            setEditAddress(cleanVal(profile?.diachi));
            setEditModalVisible(true);
          }}
        >
          <Ionicons name="create-outline" size={16} color={colors.primary} />
          <Text style={[typography.captionBold, { color: colors.primary }]}>
            {t('common.edit')}
          </Text>
        </TouchableOpacity>
      </View>

      <AppCard>
        {renderInfoRow('male-female-outline', t('profile.gender'), profile?.gioitinh)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('gift-outline', t('profile.birthDate'), formatDate(profile?.ngaysinh, activeLanguage))}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('card-outline', t('profile.idCard'), profile?.cccd)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('call-outline', t('profile.phone'), profile?.dienthoai)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('mail-outline', t('profile.email'), profile?.email)}
        <AppDivider marginVertical={spacing.sm} />
        {renderInfoRow('home-outline', t('profile.address'), profile?.diachi)}
      </AppCard>

      {/* Modal: Avatar Actions (Chọn ảnh, xem to) */}
      <Modal
        visible={avatarActionModalVisible}
        animationType={Platform.OS === "ios" ? "fade" : "none"}
        onDismiss={() => { void finishAvatarAction(); }}
        transparent
        onRequestClose={() => setAvatarActionModalVisible(false)}
      >
        <TouchableOpacity
          style={styles.modalOverlay}
          activeOpacity={1}
          onPress={() => setAvatarActionModalVisible(false)}
        >
          <View style={[styles.actionModalSheet, { backgroundColor: colors.surfaceCard }]}>
            <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.md }]}>
              {t('mobileReview.avatarTitle')}
            </Text>

            {avatarSource && (
              <TouchableOpacity
                style={styles.actionRow}
                onPress={() => queueAvatarAction('view')}
              >
                <Ionicons name="eye-outline" size={22} color={colors.primary} />
                <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.md }]}>
                  {t('mobileReview.avatarView')}
                </Text>
              </TouchableOpacity>
            )}

            <TouchableOpacity style={styles.actionRow} onPress={() => queueAvatarAction('library')}>
              <Ionicons name="images-outline" size={22} color={colors.primary} />
              <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.md }]}>
                {t('mobileReview.avatarChoose')}
              </Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.actionRow} onPress={() => queueAvatarAction('camera')}>
              <Ionicons name="camera-outline" size={22} color={colors.primary} />
              <Text style={[typography.bodyBold, { color: colors.text, marginLeft: spacing.md }]}>
                {t('mobileReview.avatarTake')}
              </Text>
            </TouchableOpacity>

            <View style={{ marginTop: spacing.md }}>
              <AppButton
                title={t('common.cancel')}
                onPress={() => setAvatarActionModalVisible(false)}
                variant="outline"
              />
            </View>
          </View>
        </TouchableOpacity>
      </Modal>

      {/* Modal: Xem Avatar lớn */}
      <Modal
        visible={avatarViewModalVisible}
        animationType="fade"
        transparent
        onRequestClose={() => setAvatarViewModalVisible(false)}
      >
        <View style={styles.fullImageModal}>
          <TouchableOpacity
            style={styles.closeFullImageBtn}
            onPress={() => setAvatarViewModalVisible(false)}
          >
            <Ionicons name="close" size={28} color="#FFFFFF" />
          </TouchableOpacity>
          {avatarSource ? (
            <Image
              source={avatarSource}
              style={styles.fullImageView}
              resizeMode="contain"
            />
          ) : (
            <Text style={{ color: '#FFFFFF' }}>{t('mobileReview.avatarNone')}</Text>
          )}
        </View>
      </Modal>

      {/* Modal: Preview ảnh trước khi lưu / gửi duyệt */}
      <Modal
        visible={avatarPreviewModalVisible}
        animationType="slide"
        transparent
        onRequestClose={() => setAvatarPreviewModalVisible(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={[styles.previewModalCard, { backgroundColor: colors.surfaceCard }]}>
            <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.sm }]}>
              {t('mobileReview.avatarConfirm')}
            </Text>

            {previewAvatarBase64 && (
              <Image
                source={{ uri: previewAvatarBase64 }}
                style={styles.previewAvatarImage}
                resizeMode="cover"
              />
            )}

            <View style={[styles.quotaNoteBox, { backgroundColor: colors.background, borderColor: colors.border }]}>
              <Text style={[typography.captionBold, { color: colors.text }]}>
                {t('mobileReview.quotaTitle')}
              </Text>
              <Text style={[typography.caption, { color: colors.textSecondary, marginTop: 2 }]}>
                {quotas?.avatar?.isFirstEditAvailable
                  ? t('mobileReview.firstEdit')
                  : t('mobileReview.approvalRequired')}
              </Text>
            </View>

            <View style={{ flexDirection: 'row', gap: spacing.md, marginTop: spacing.lg }}>
              <View style={{ flex: 1 }}>
                <AppButton
                  title={t('common.cancel')}
                  onPress={() => {
                    setAvatarPreviewModalVisible(false);
                    setPreviewAvatarBase64(null);
                    setNewAvatarRawBase64(null);
                  }}
                  variant="outline"
                />
              </View>
              <View style={{ flex: 1 }}>
                <AppButton
                  title={uploadingAvatar ? t('common.loading') : t('common.confirm')}
                  onPress={handleUploadAvatarConfirm}
                  loading={uploadingAvatar} disabled={!canMobile(user,'MOBILE_PROFILE_EDIT','Edit')}
                  variant="primary"
                />
              </View>
            </View>
          </View>
        </View>
      </Modal>

      {/* Modal: Chỉnh sửa thông tin cá nhân */}
      <Modal
        visible={editModalVisible}
        animationType="slide"
        transparent
        onRequestClose={() => setEditModalVisible(false)}
      >
        <View style={styles.modalOverlay}>
          <ScrollView contentContainerStyle={{ justifyContent: 'center', flexGrow: 1 }}>
            <View style={[styles.editInfoModalCard, { backgroundColor: colors.surfaceCard }]}>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: spacing.xs }}>
                <Text style={[typography.h3, { color: colors.text }]}>
                  {t('profile.editProfile')}
                </Text>
                <TouchableOpacity onPress={() => setEditModalVisible(false)}>
                  <Ionicons name="close-circle-outline" size={24} color={colors.textSecondary} />
                </TouchableOpacity>
              </View>

              <View style={[styles.quotaNoteBox, { backgroundColor: colors.background, borderColor: colors.border, marginBottom: spacing.md }]}>
                <View style={{ flexDirection: 'row', alignItems: 'center', gap: 6 }}>
                  <Ionicons name="checkmark-circle" size={16} color={colors.primary} />
                  <Text style={[typography.captionBold, { color: colors.text }]}>
                    {t('mobileReview.quotaTitle')}
                  </Text>
                </View>
                <Text style={[typography.caption, { color: colors.textSecondary, marginTop: 2 }]}>
                  {quotas?.personalInfo?.isFirstEditAvailable ? t('mobileReview.firstEdit') : t('mobileReview.approvalRequired')}
                </Text>
              </View>

              <AppTextInput
                label={t('profile.fullName')+' *'}
                value={editName}
                onChangeText={setEditName}
                placeholder={t('profile.fullName')}
                leftIcon="person-outline"
              />

              <Text style={[typography.captionBold, { color: colors.text, marginTop: spacing.xs, marginBottom: 6 }]}>
                {t('profile.gender')}
              </Text>
              <View style={{ flexDirection: 'row', gap: spacing.sm, marginBottom: spacing.sm }}>
                {['Nam', 'Nữ'].map((g) => (
                  <TouchableOpacity
                    key={t(g==='Nam' ? 'mobileReview.male' : 'mobileReview.female')}
                    style={{
                      flex: 1,
                      paddingVertical: 10,
                      alignItems: 'center',
                      borderRadius: spacing.borderRadiusSm,
                      borderWidth: 1,
                      backgroundColor: editGender === g ? colors.primary : colors.background,
                      borderColor: editGender === g ? colors.primary : colors.border,
                    }}
                    onPress={() => setEditGender(g)}
                  >
                    <Text
                      style={[
                        typography.captionBold,
                        { color: editGender === g ? '#FFFFFF' : colors.text },
                      ]}
                    >
                      {t(g==='Nam' ? 'mobileReview.male' : 'mobileReview.female')}
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>

              <AppTextInput
                label={t('profile.birthDate')+' (YYYY-MM-DD)'}
                value={editBirthDate}
                onChangeText={setEditBirthDate}
                placeholder=" 1990-05-20"
                leftIcon="calendar-outline"
              />

              <AppTextInput
                label={t('profile.idCard')}
                value={editCccd}
                onChangeText={setEditCccd}
                keyboardType="numeric"
                placeholder=" 001201012345"
                leftIcon="card-outline"
              />

              <AppTextInput
                label={t('profile.phone')}
                value={editPhone}
                onChangeText={setEditPhone}
                keyboardType="phone-pad"
                placeholder=" 0915013734"
                leftIcon="call-outline"
              />

              <AppTextInput
                label={t('profile.email')}
                value={editEmail}
                onChangeText={setEditEmail}
                keyboardType="email-address"
                autoCapitalize="none"
                placeholder=" an.nguyen@company.com"
                leftIcon="mail-outline"
              />

              <AppTextInput
                label={t('profile.address')}
                value={editAddress}
                onChangeText={setEditAddress}
                multiline
                numberOfLines={2}
                placeholder={t('profile.address')}
                leftIcon="home-outline"
              />

              <View style={{ flexDirection: 'row', gap: spacing.md, marginTop: spacing.md }}>
                <View style={{ flex: 1 }}>
                  <AppButton
                    title={t('common.cancel')}
                    onPress={() => setEditModalVisible(false)}
                    variant="outline"
                  />
                </View>
                <View style={{ flex: 1 }}>
                  <AppButton
                    title={updating ? t('common.loading') : t('common.save')}
                    onPress={handleSavePersonalInfo}
                    loading={updating} disabled={!canMobile(user,'MOBILE_PROFILE_EDIT','Edit')}
                    variant="primary"
                  />
                </View>
              </View>
            </View>
          </ScrollView>
        </View>
      </Modal>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    padding: spacing.lg,
    paddingBottom: 40,
  },
  heroCard: {
    alignItems: 'center',
    paddingVertical: spacing.xl,
  },
  avatarTouchWrapper: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarWrapper: {
    position: 'relative',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15,
    shadowRadius: 8,
    elevation: 4,
  },
  avatarCameraBadge: {
    position: 'absolute',
    bottom: 2,
    right: 2,
    width: 26,
    height: 26,
    borderRadius: 13,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 2,
    borderColor: '#FFFFFF',
  },
  pendingAvatarBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: spacing.sm,
    paddingHorizontal: spacing.sm,
    paddingVertical: 4,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
  },
  badgeRow: {
    flexDirection: 'row',
    gap: spacing.sm,
    marginTop: spacing.sm,
  },
  linksRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
    marginTop: spacing.xl,
    width: '100%',
  },
  linkBtn: {
    width: '48%',
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 6,
    paddingVertical: spacing.sm,
    paddingHorizontal: 4,
    borderRadius: spacing.borderRadiusMd,
    borderWidth: 1,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.xs,
  },
  infoIcon: {
    width: 28,
  },
  infoTextWrapper: {
    flex: 1,
    marginLeft: spacing.sm,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'center',
    padding: spacing.lg,
  },
  actionModalSheet: {
    borderRadius: spacing.borderRadiusLg,
    padding: spacing.xl,
  },
  actionRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.md,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: 'rgba(150,150,150,0.2)',
  },
  fullImageModal: {
    flex: 1,
    backgroundColor: '#000000',
    justifyContent: 'center',
    alignItems: 'center',
  },
  closeFullImageBtn: {
    position: 'absolute',
    top: 50,
    right: 20,
    zIndex: 10,
    padding: 8,
  },
  fullImageView: {
    width: '90%',
    height: '70%',
  },
  previewModalCard: {
    borderRadius: spacing.borderRadiusLg,
    padding: spacing.xl,
    alignItems: 'center',
  },
  previewAvatarImage: {
    width: 180,
    height: 180,
    borderRadius: 90,
    marginVertical: spacing.md,
  },
  quotaNoteBox: {
    width: '100%',
    padding: spacing.sm,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
    marginTop: spacing.xs,
  },
  editInfoModalCard: {
    borderRadius: spacing.borderRadiusLg,
    padding: spacing.xl,
  },
});
