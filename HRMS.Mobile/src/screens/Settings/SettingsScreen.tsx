import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
  Switch,
  ActivityIndicator,
  AppState,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { canMobile } from '../../utils/mobilePermissions';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppDivider } from '../../components/AppDivider';
import { AppBadge } from '../../components/AppBadge';
import { notificationService } from '../../services/notificationService';
import { APP_CONFIG } from '../../config';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';
import {mapApiError} from '../../utils/errorMapper';

export const SettingsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { logout,user } = useAuth();
  const { colors, mode } = useTheme();
  const { preference, activeLanguage } = useLanguage();

  const remotePushConfigured=notificationService.isRemotePushConfigured();
  const nativeNotifications=notificationService.canRequestPermission();
  const notificationAllowed=canMobile(user,'MOBILE_NOTIFICATION_VIEW');
  const [canAskAgain,setCanAskAgain]=useState(true);
  const [notifLoading, setNotifLoading] = useState(false);
  const [permStatus, setPermStatus] = useState<'GRANTED' | 'DENIED' | 'UNDETERMINED'>('UNDETERMINED');
  const [pushEnabled, setPushEnabled] = useState(true);
  const [isRegistered, setIsRegistered] = useState(false);
  const [syncError, setSyncError] = useState<string | null>(null);

  const refreshNotificationStatus = useCallback(async () => {
    if (!nativeNotifications) return;
    setNotifLoading(true);
    try {
      const { status, canAskAgain } = await notificationService.getPermissionStatus();
      setPermStatus(status);
      setCanAskAgain(canAskAgain);
      if (!notificationAllowed) return;
      const regResult = await notificationService.syncDeviceRegistration();
      setIsRegistered(regResult.isRegistered);
      setPushEnabled(regResult.pushEnabled);
      setSyncError(regResult.errorMessage);
    } catch (err: any) {
      setSyncError(mapApiError(err));
    } finally {
      setNotifLoading(false);
    }
  }, [nativeNotifications, notificationAllowed]);

  useEffect(() => {
    refreshNotificationStatus();

    const sub = AppState.addEventListener('change', (nextState) => {
      if (nextState === 'active') {
        refreshNotificationStatus();
      }
    });

    return () => {
      sub.remove();
    };
  }, [refreshNotificationStatus]);

  const handleTogglePush = async (value: boolean) => {
    if (notifLoading || !notificationAllowed || !remotePushConfigured) return;
    setNotifLoading(true);
    try {
      if (value && permStatus !== 'GRANTED') {
        if (!canAskAgain) { await notificationService.openAppSettings(); return; }
        const permission=await notificationService.requestPermission();
        setPermStatus(permission.status); setCanAskAgain(permission.canAskAgain);
        if (permission.status !== 'GRANTED') return;
      }
      await notificationService.syncDeviceRegistration();
      if (!await notificationService.setPushEnabled(value)) throw new Error('push preference sync failed');
      setPushEnabled(value);
      await refreshNotificationStatus();
    } catch {
      Alert.alert(t('common.error'), t('settings.pushToggleFailed'));
    } finally { setNotifLoading(false); }
  };

  const handleRequestPermission = async () => {
    if (notifLoading || !nativeNotifications || !notificationAllowed) return;
    setNotifLoading(true);
    try {
      if (permStatus === 'DENIED' && !canAskAgain) {
        await notificationService.openAppSettings(); return;
      }
      const permission=await notificationService.requestPermission();
      setPermStatus(permission.status); setCanAskAgain(permission.canAskAgain);
      await refreshNotificationStatus();
    } catch { Alert.alert(t('common.error'), t('errors.unknown')); }
    finally { setNotifLoading(false); }
  };

  const handleLogout = () => {
    Alert.alert(
      t('auth.logoutConfirmTitle'),
      t('auth.logoutConfirmMessage'),
      [
        { text: t('common.cancel'), style: 'cancel' },
        {
          text: t('auth.logoutButton'),
          style: 'destructive',
          onPress: async () => {
            await logout();

          },
        },
      ]
    );
  };

  const getThemeText = () => {
    switch (mode) {
      case 'light':
        return t('settings.themeLight');
      case 'dark':
        return t('settings.themeDark');
      default:
        return t('settings.themeSystem');
    }
  };

  const getLanguageText = () => {
    if (preference === 'system') return `${t('language.followSystem')} (${activeLanguage.toUpperCase()})`;
    if (preference === 'vi') return 'Tiếng Việt 🇻🇳';
    if (preference === 'ja') return '日本語 🇯🇵';
    if (preference === 'zh-CN') return '中文 🇨🇳';
    if (preference === 'ko') return '한국어 🇰🇷';
    return 'English 🇺🇸';
  };

  const renderActionRow = (
    icon: keyof typeof Ionicons.glyphMap,
    label: string,
    onPress: () => void,
    trailingText?: string,
    isDanger = false
  ) => (
    <TouchableOpacity
      activeOpacity={0.7}
      onPress={onPress}
      style={styles.actionRow}
    >
      <View style={styles.actionLeft}>
        <Ionicons
          name={icon}
          size={22}
          color={isDanger ? colors.danger : colors.primary}
        />
        <Text
          style={[
            typography.bodyBold,
            { color: isDanger ? colors.danger : colors.text, marginLeft: spacing.md },
          ]}
        >
          {label}
        </Text>
      </View>

      <View style={styles.actionRight}>
        {trailingText ? (
          <Text style={[typography.caption, { color: colors.textSecondary, marginRight: spacing.xs }]}>
            {trailingText}
          </Text>
        ) : null}
        <Ionicons
          name="chevron-forward"
          size={20}
          color={isDanger ? colors.danger : colors.textMuted}
        />
      </View>
    </TouchableOpacity>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader
        title={t('settings.title')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {/* Section 1: Account */}
        <Text style={[typography.captionBold, { color: colors.textSecondary, marginBottom: spacing.xs }]}>
          {t('settings.accountSection').toUpperCase()}
        </Text>
        <AppCard style={styles.sectionCard}>
          {renderActionRow('key-outline', t('settings.changePassword'), () => {
            if (canMobile(user,'DOIMATKHAU','Edit')) navigation.navigate('ChangePassword');
            else Alert.alert(t('common.error'), t('mobileReview.permissionRefreshHelp'));
          })}
          <AppDivider marginVertical={spacing.xs} />
          {renderActionRow(
            'log-out-outline',
            t('settings.logout'),
            handleLogout,
            undefined,
            true
          )}
        </AppCard>

        {/* Section 2: Notifications */}
        <Text
          style={[
            typography.captionBold,
            { color: colors.textSecondary, marginTop: spacing.xl, marginBottom: spacing.xs },
          ]}
        >
          {t('settings.notificationsSection', 'THÔNG BÁO').toUpperCase()}
        </Text>
        <AppCard style={styles.sectionCard}>
          <Text style={[typography.bodyBold,{color:colors.text}]}>{t('mobileReview.inAppNotifications')}</Text>
          <Text style={[typography.caption,{color:colors.textSecondary,marginTop:spacing.sm}]}>
            {t('mobileReview.inAppNotificationsDesc')}
          </Text>
          {!remotePushConfigured && <Text style={[typography.caption,{color:colors.textSecondary,marginTop:spacing.md}]}>{t('mobileReview.remotePushDeferred')}</Text>}
          {!nativeNotifications && <Text style={[typography.caption,{color:colors.textSecondary,marginTop:spacing.md}]}>{t("mobileReview.pushNativeBuildRequired")}</Text>}
          {!notificationAllowed && <Text style={[typography.caption,{color:colors.danger,marginTop:spacing.md}]}>{t("mobileReview.permissionRefreshHelp")}</Text>}
          <>
          {/* Push Toggle */}
          <View style={styles.switchRow}>
            <View style={styles.actionLeft}>
              <Ionicons name="notifications-outline" size={22} color={colors.primary} />
              <View style={{ marginLeft: spacing.md, flex: 1 }}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>
                  {t('settings.pushNotification', 'Thông báo đẩy')}
                </Text>
                <Text style={[typography.caption, { color: colors.textSecondary }]}>
                  {t('settings.pushNotificationDesc', 'Nhận thông báo đơn từ và cập nhật công ty')}
                </Text>
              </View>
            </View>
            <Switch
              disabled={!remotePushConfigured || !notificationAllowed || notifLoading}
              value={pushEnabled && permStatus === 'GRANTED'}
              onValueChange={handleTogglePush}
              trackColor={{ false: colors.border, true: colors.primary }}
              thumbColor="#ffffff"
            />
          </View>

          <AppDivider marginVertical={spacing.xs} />

          {/* OS Permission Status */}
          <View style={styles.switchRow}>
            <View style={styles.actionLeft}>
              <Ionicons name="shield-checkmark-outline" size={22} color={colors.primary} />
              <View style={{ marginLeft: spacing.md, flex: 1 }}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>
                  {t('settings.systemPermission', 'Quyền hệ điều hành')}
                </Text>
                <Text style={[typography.caption, { color: colors.textSecondary }]}>
                  {permStatus === 'GRANTED'
                    ? t('settings.permissionGranted', 'Đã cấp quyền thông báo')
                    : permStatus === 'DENIED'
                    ? t('settings.permissionDenied', 'Bị tắt trong Cài đặt thiết bị')
                    : t('settings.permissionUndetermined', 'Chưa cấp quyền')}
                </Text>
              </View>
            </View>
            <View style={{ alignItems: 'flex-end', gap: 4 }}>
              <AppBadge
                label={
                  permStatus === 'GRANTED'
                    ? t('common.active', 'Đã bật')
                    : permStatus === 'DENIED'
                    ? t('common.disabled', 'Bị tắt')
                    : t('common.pending', 'Chưa bật')
                }
                variant={
                  permStatus === 'GRANTED'
                    ? 'success'
                    : permStatus === 'DENIED'
                    ? 'danger'
                    : 'warning'
                }
              />
              {permStatus !== 'GRANTED' && (
                <TouchableOpacity disabled={!nativeNotifications || !notificationAllowed || notifLoading} onPress={handleRequestPermission} style={[styles.openSettingsBtn, {opacity: nativeNotifications && notificationAllowed ? 1 : 0.5}]}>
                  <Text style={[typography.captionBold, { color: colors.primary }]}>
                    {permStatus === 'DENIED' && !canAskAgain
                      ? t('settings.openSettings', 'Mở Cài đặt')
                      : t('settings.enablePermission', 'Bật ngay')}
                  </Text>
                </TouchableOpacity>
              )}
            </View>
          </View>

          <AppDivider marginVertical={spacing.xs} />

          {/* Channel sound/vibration remain under the device owner's control. */}
          <View style={styles.switchRow}>
            <View style={styles.actionLeft}>
              <Ionicons name="volume-high-outline" size={22} color={colors.primary} />
              <View style={{ marginLeft: spacing.md, flex: 1 }}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>
                  {t('settings.notificationSoundVibration')}
                </Text>
                <Text style={[typography.caption, { color: colors.textSecondary }]}>
                  {t('settings.notificationSoundVibrationDesc')}
                </Text>
              </View>
            </View>
            <TouchableOpacity
              disabled={!nativeNotifications || !notificationAllowed || notifLoading}
              onPress={() => {
                void notificationService.openAppSettings().catch(() => Alert.alert(t('common.error'), t('errors.unknown')));
              }}
              style={styles.openSettingsBtn}>
              <Text style={[typography.captionBold, { color: colors.primary }]}>{t('settings.openSettings')}</Text>
            </TouchableOpacity>
          </View>
          <AppDivider marginVertical={spacing.xs} />

          {/* Server Sync / Status */}
          <View style={styles.switchRow}>
            <View style={styles.actionLeft}>
              <Ionicons name="cloud-done-outline" size={22} color={colors.primary} />
              <View style={{ marginLeft: spacing.md, flex: 1 }}>
                <Text style={[typography.bodyBold, { color: colors.text }]}>
                  {t('settings.pushRegistration', 'Đăng ký nhận tin')}
                </Text>
                <Text style={[typography.caption, { color: colors.textSecondary }]}>
                  {isRegistered
                    ? t('settings.registeredSuccess', 'Thiết bị đã kết nối tổng đài thông báo')
                    : syncError
                    ? syncError
                    : t('settings.registeredPending', 'Chưa hoàn tất kết nối')}
                </Text>
              </View>
            </View>
            <TouchableOpacity
              onPress={refreshNotificationStatus}
              disabled={notifLoading || !nativeNotifications || !notificationAllowed}
              style={styles.retryBtn}
            >
              {notifLoading ? (
                <ActivityIndicator size="small" color={colors.primary} />
              ) : (
                <View style={{ flexDirection: 'row', alignItems: 'center', gap: 4 }}>
                  <Ionicons name="refresh-outline" size={16} color={colors.primary} />
                  <Text style={[typography.captionBold, { color: colors.primary }]}>
                    {t('common.retry', 'Thử lại')}
                  </Text>
                </View>
              )}
            </TouchableOpacity>
          </View>
          </>
        </AppCard>

        {/* Section 3: Appearance */}
        <Text style={[typography.captionBold, { color: colors.textSecondary, marginTop: spacing.xl, marginBottom: spacing.xs }]}>
          {t('settings.appearanceSection').toUpperCase()}
        </Text>
        <AppCard style={styles.sectionCard}>
          {renderActionRow(
            'language-outline',
            t('settings.language'),
            () => navigation.navigate('LanguageSettings'),
            getLanguageText()
          )}
          <AppDivider marginVertical={spacing.xs} />
          {renderActionRow(
            'color-palette-outline',
            t('settings.theme'),
            () => navigation.navigate('ThemeSettings'),
            getThemeText()
          )}
        </AppCard>

        {/* Section 4: App Information */}
        <Text style={[typography.captionBold, { color: colors.textSecondary, marginTop: spacing.xl, marginBottom: spacing.xs }]}>
          {t('settings.systemSection').toUpperCase()}
        </Text>
        <AppCard style={styles.sectionCard}>
          <View style={styles.infoRow}>
            <Text style={[typography.body, { color: colors.textSecondary }]}>
              {t('settings.appVersion')}
            </Text>
            <Text style={[typography.bodyBold, { color: colors.text }]}>
              {APP_CONFIG.version} (Build {APP_CONFIG.buildNumber})
            </Text>
          </View>
          <AppDivider marginVertical={spacing.xs} />
          <View style={styles.infoRow}>
            <Text style={[typography.body, { color: colors.textSecondary }]}>
              {t('settings.environment')}
            </Text>
            <Text style={[typography.bodyBold, { color: colors.primary }]}>
              {APP_CONFIG.env.toUpperCase()}
            </Text>
          </View>
          <AppDivider marginVertical={spacing.xs} />
          <View style={styles.infoRow}>
            <Text style={[typography.body, { color: colors.textSecondary }]}>
              {t('settings.apiUrl')}
            </Text>
            <Text style={[typography.caption, { color: colors.textMuted }]} numberOfLines={1}>
              {new URL(APP_CONFIG.apiBaseUrl).hostname}
            </Text>
          </View>
        </AppCard>
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    padding: spacing.lg,
    paddingBottom: 40,
  },
  sectionCard: {
    padding: spacing.sm,
  },
  actionRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
  },
  actionLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  actionRight: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  switchRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
  },
  openSettingsBtn: {
    marginTop: 2,
    paddingVertical: 2,
    paddingHorizontal: 4,
  },
  retryBtn: {
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.sm,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
  },
});
