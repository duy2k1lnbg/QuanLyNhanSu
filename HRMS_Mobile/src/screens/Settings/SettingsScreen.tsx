import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppDivider } from '../../components/AppDivider';
import { APP_CONFIG } from '../../config';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const SettingsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { logout } = useAuth();
  const { colors, mode } = useTheme();
  const { preference, activeLanguage } = useLanguage();

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
            navigation.reset({
              index: 0,
              routes: [{ name: 'Login' }],
            });
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
          {renderActionRow('key-outline', t('settings.changePassword'), () =>
            navigation.navigate('ChangePassword')
          )}
          <AppDivider marginVertical={spacing.xs} />
          {renderActionRow(
            'log-out-outline',
            t('settings.logout'),
            handleLogout,
            undefined,
            true
          )}
        </AppCard>

        {/* Section 2: Appearance */}
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

        {/* Section 3: App Information */}
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
              {APP_CONFIG.apiBaseUrl}
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
  },
  actionRight: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
  },
});
