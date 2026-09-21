import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  Modal,
  Alert,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { Ionicons } from '@expo/vector-icons';
import { RootStackParamList } from '../../navigation/types';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { ProfileDto } from '../../types/me';
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
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const ProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Edit personal info states
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [editPhone, setEditPhone] = useState('');
  const [editEmail, setEditEmail] = useState('');
  const [editAddress, setEditAddress] = useState('');
  const [updating, setUpdating] = useState(false);

  const fetchProfile = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const data = await meApi.getProfile();
      setProfile(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

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
        <View style={styles.avatarWrapper}>
          <AppAvatar
            name={profile?.hoten}
            avatarBase64={profile?.avatarBase64}
            size="xl"
          />
        </View>
        <Text style={[typography.h2, { color: colors.text, marginTop: spacing.md }]}>
          {profile?.hoten || '---'}
        </Text>
        <View style={styles.badgeRow}>
          <AppBadge label={`${t('profile.employeeId')}: ${profile?.employeeCode || profile?.manv || '---'}`} />
          {profile?.trangThaiLaoDong ? (
            <AppBadge
              label={profile.trangThaiLaoDong}
              variant={profile.trangThaiLaoDong.toLowerCase().includes('làm việc') ? 'success' : 'default'}
            />
          ) : null}
        </View>

        {/* Quick links to Contract, Insurance, Settings */}
        <View style={styles.linksRow}>
          <TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Contract')}
          >
            <Ionicons name="document-text-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]}>
              {t('nav.contract')}
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Insurance')}
          >
            <Ionicons name="medkit-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]}>
              {t('nav.insurance')}
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.linkBtn, { borderColor: colors.border }]}
            onPress={() => navigation.navigate('Settings')}
          >
            <Ionicons name="settings-outline" size={18} color={colors.primary} />
            <Text style={[typography.captionBold, { color: colors.text }]}>
              {t('nav.settings')}
            </Text>
          </TouchableOpacity>
        </View>
      </AppCard>

      {/* 2. Work Information (HR-Owned, Read-Only per Rule 32) */}
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

      {/* 3. Personal Information (Editable for phone, email, address per Rule 32) */}
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.lg, marginBottom: spacing.sm }}>
        <Text style={[typography.h3, { color: colors.text }]}>
          {t('profile.personalInfo')}
        </Text>
        <TouchableOpacity
          style={{ flexDirection: 'row', alignItems: 'center', gap: 4 }}
          onPress={() => {
            setEditPhone(profile?.dienthoai || '');
            setEditEmail(profile?.email || '');
            setEditAddress(profile?.diachi || '');
            setEditModalVisible(true);
          }}
        >
          <Ionicons name="create-outline" size={16} color={colors.primary} />
          <Text style={[typography.captionBold, { color: colors.primary }]}>
            {t('common.save') !== 'Lưu' ? 'Edit' : 'Sửa'}
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

      {/* Edit Personal Info Modal */}
      <Modal
        visible={editModalVisible}
        animationType="slide"
        transparent
        onRequestClose={() => setEditModalVisible(false)}
      >
        <View style={{ flex: 1, justifyContent: 'center', backgroundColor: 'rgba(0,0,0,0.5)', padding: spacing.lg }}>
          <View style={{ backgroundColor: colors.surfaceCard, borderRadius: spacing.borderRadiusLg, padding: spacing.xl }}>
            <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.md }]}>
              {t('profile.editProfile')}
            </Text>

            <AppTextInput
              label={t('profile.phone')}
              value={editPhone}
              onChangeText={setEditPhone}
              keyboardType="phone-pad"
              leftIcon="call-outline"
            />

            <AppTextInput
              label={t('profile.email')}
              value={editEmail}
              onChangeText={setEditEmail}
              keyboardType="email-address"
              autoCapitalize="none"
              leftIcon="mail-outline"
            />

            <AppTextInput
              label={t('profile.address')}
              value={editAddress}
              onChangeText={setEditAddress}
              multiline
              numberOfLines={2}
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
                  onPress={async () => {
                    if (updating) return;
                    setUpdating(true);
                    try {
                      await meApi.updateProfile({
                        dienthoai: editPhone.trim(),
                        email: editEmail.trim(),
                        diachi: editAddress.trim(),
                      });
                      Alert.alert(t('common.success'), t('profile.updateSuccess'));
                      setEditModalVisible(false);
                      fetchProfile(true);
                    } catch (error: any) {
                      Alert.alert(t('common.error'), mapApiError(error));
                    } finally {
                      setUpdating(false);
                    }
                  }}
                  loading={updating}
                  variant="primary"
                />
              </View>
            </View>
          </View>
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
  avatarWrapper: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15,
    shadowRadius: 8,
    elevation: 4,
  },
  badgeRow: {
    flexDirection: 'row',
    gap: spacing.sm,
    marginTop: spacing.sm,
  },
  linksRow: {
    flexDirection: 'row',
    gap: spacing.sm,
    marginTop: spacing.xl,
    width: '100%',
  },
  linkBtn: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 4,
    paddingVertical: spacing.sm,
    paddingHorizontal: 2,
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
});
