import React, { useState, useRef } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
  Alert,
  TextInput,
  TouchableOpacity,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { RootStackParamList } from '../../navigation/types';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { AppTextInput } from '../../components/AppTextInput';
import { AppButton } from '../../components/AppButton';
import { mapApiError } from '../../utils/errorMapper';
import { APP_CONFIG } from '../../config';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

type Props = NativeStackScreenProps<RootStackParamList, 'Login'>;

export const LoginScreen: React.FC<Props> = ({ navigation }) => {
  const { t } = useTranslation();
  const { login, isLoading } = useAuth();
  const { colors, mode } = useTheme();
  const { activeLanguage } = useLanguage();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const passwordInputRef = useRef<TextInput>(null);

  const handleLogin = async () => {
    setErrorMessage(null);
    const trimmedUser = username.trim();

    if (!trimmedUser) {
      setErrorMessage(t('auth.validationEmptyUser'));
      return;
    }
    if (!password) {
      setErrorMessage(t('auth.validationEmptyPass'));
      return;
    }

    try {
      await login({
        username: trimmedUser,
        password,
        clientType: 'MOBILE',
      });
      // Navigate to Main upon successful authentication
      navigation.replace('Main');
    } catch (error: any) {
      const friendlyMsg = mapApiError(error);
      setErrorMessage(friendlyMsg);
      // UX rule: keep username, clear password on auth fail
      setPassword('');
      passwordInputRef.current?.focus();
    }
  };

  const handleForgotPassword = () => {
    Alert.alert(t('auth.forgotPassword'), t('auth.forgotPasswordAlert'), [
      { text: t('common.close'), style: 'cancel' },
    ]);
  };

  const getLanguageLabel = () => {
    switch (activeLanguage) {
      case 'vi':
        return '🇻🇳 Tiếng Việt';
      case 'ja':
        return '🇯🇵 日本語';
      case 'en':
        return '🇺🇸 English';
      default:
        return '🌐 ' + activeLanguage;
    }
  };

  return (
    <KeyboardAvoidingView
      style={{ flex: 1, backgroundColor: colors.background }}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <View style={styles.brandingHeader}>
          <View style={[styles.brandIcon, { backgroundColor: colors.primary }]}>
            <Ionicons name="shield-checkmark" size={36} color="#FFFFFF" />
          </View>
          <Text style={[typography.h1, { color: colors.text, marginTop: spacing.md }]}>
            HRMS Mobile
          </Text>
          <Text
            style={[
              typography.caption,
              { color: colors.textSecondary, textAlign: 'center', marginTop: spacing.xs, maxWidth: 320 },
            ]}
          >
            {t('auth.subtitle')}
          </Text>
        </View>

        <View style={[styles.card, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
          <Text style={[typography.h3, { color: colors.text, marginBottom: spacing.lg }]}>
            {t('auth.login')}
          </Text>

          {errorMessage ? (
            <View style={[styles.errorBanner, { backgroundColor: 'rgba(239, 68, 68, 0.1)', borderColor: colors.danger }]}>
              <Ionicons name="alert-circle" size={20} color={colors.danger} />
              <Text style={[typography.caption, { color: colors.danger, flex: 1, marginLeft: spacing.sm }]}>
                {errorMessage}
              </Text>
            </View>
          ) : null}

          <AppTextInput
            label={t('auth.username')}
            placeholder={t('auth.usernamePlaceholder')}
            leftIcon="person-outline"
            value={username}
            onChangeText={(val) => {
              setUsername(val);
              if (errorMessage) setErrorMessage(null);
            }}
            autoCapitalize="none"
            autoCorrect={false}
            returnKeyType="next"
            onSubmitEditing={() => passwordInputRef.current?.focus()}
          />

          <AppTextInput
            label={t('auth.password')}
            placeholder={t('auth.passwordPlaceholder')}
            leftIcon="lock-closed-outline"
            isPassword
            value={password}
            onChangeText={(val) => {
              setPassword(val);
              if (errorMessage) setErrorMessage(null);
            }}
            returnKeyType="done"
            onSubmitEditing={handleLogin}
          />

          <TouchableOpacity
            activeOpacity={0.7}
            onPress={handleForgotPassword}
            style={styles.forgotButton}
          >
            <Text style={[typography.captionBold, { color: colors.primary }]}>
              {t('auth.forgotPassword')}
            </Text>
          </TouchableOpacity>

          <AppButton
            title={isLoading ? t('auth.loggingIn') : t('auth.login')}
            onPress={handleLogin}
            variant="primary"
            size="lg"
            loading={isLoading}
            style={{ marginTop: spacing.md }}
          />
        </View>

        <View style={styles.footer}>
          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() => navigation.navigate('LanguageSettings')}
            style={styles.metaRow}
          >
            <Text style={[typography.caption, { color: colors.textSecondary }]}>
              Language: <Text style={{ color: colors.primary, fontWeight: '600' }}>{getLanguageLabel()}</Text>
            </Text>
          </TouchableOpacity>

          <Text style={[typography.caption, { color: colors.textMuted, marginTop: spacing.xs }]}>
            {t('auth.version')} {APP_CONFIG.version} (Build {APP_CONFIG.buildNumber})
          </Text>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: spacing.xl,
  },
  brandingHeader: {
    alignItems: 'center',
    marginBottom: spacing.xxl,
  },
  brandIcon: {
    width: 64,
    height: 64,
    borderRadius: 18,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.15,
    shadowRadius: 6,
    elevation: 3,
  },
  card: {
    padding: spacing.xl,
    borderRadius: spacing.borderRadiusLg,
    borderWidth: 1,
    elevation: 2,
    shadowColor: 'rgba(0, 0, 0, 0.05)',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 1,
    shadowRadius: 12,
  },
  errorBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: spacing.md,
    borderRadius: spacing.borderRadiusSm,
    borderWidth: 1,
    marginBottom: spacing.md,
  },
  forgotButton: {
    alignSelf: 'flex-end',
    marginBottom: spacing.md,
    marginTop: -spacing.xs,
  },
  footer: {
    alignItems: 'center',
    marginTop: spacing.xxl,
  },
  metaRow: {
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.md,
  },
});
