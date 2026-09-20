import React, { useState } from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
  Alert,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { authApi } from '../../api/authApi';
import { AppHeader } from '../../components/AppHeader';
import { AppTextInput } from '../../components/AppTextInput';
import { AppButton } from '../../components/AppButton';
import { AppCard } from '../../components/AppCard';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';

type Props = NativeStackScreenProps<RootStackParamList, 'ChangePassword'>;

export const ChangePasswordScreen: React.FC<Props> = ({ navigation }) => {
  const { t } = useTranslation();
  const { logout } = useAuth();
  const { colors } = useTheme();

  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handleSubmit = async () => {
    setErrorMsg(null);

    if (!oldPassword.trim()) {
      setErrorMsg(t('auth.validationEmptyPass'));
      return;
    }
    if (!newPassword.trim()) {
      setErrorMsg(t('auth.validationEmptyPass'));
      return;
    }
    if (newPassword === oldPassword) {
      setErrorMsg(t('auth.passwordSameAsOld'));
      return;
    }
    if (newPassword !== confirmPassword) {
      setErrorMsg(t('auth.passwordsDoNotMatch'));
      return;
    }

    setLoading(true);
    try {
      await authApi.changePassword({
        oldPassword,
        newPassword,
      });

      Alert.alert(t('common.success'), t('auth.changePasswordSuccess'), [
        {
          text: 'OK',
          onPress: async () => {
            await logout();
            navigation.reset({
              index: 0,
              routes: [{ name: 'Login' }],
            });
          },
        },
      ]);
    } catch (error: any) {
      setErrorMsg(mapApiError(error));
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={{ flex: 1, backgroundColor: colors.background }}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <AppHeader
        title={t('auth.changePassword')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <AppCard>
          <AppTextInput
            label={t('auth.oldPassword')}
            placeholder={t('auth.oldPassword')}
            isPassword
            value={oldPassword}
            onChangeText={(val) => {
              setOldPassword(val);
              if (errorMsg) setErrorMsg(null);
            }}
          />

          <AppTextInput
            label={t('auth.newPassword')}
            placeholder={t('auth.newPassword')}
            isPassword
            value={newPassword}
            onChangeText={(val) => {
              setNewPassword(val);
              if (errorMsg) setErrorMsg(null);
            }}
          />

          <AppTextInput
            label={t('auth.confirmPassword')}
            placeholder={t('auth.confirmPassword')}
            isPassword
            value={confirmPassword}
            onChangeText={(val) => {
              setConfirmPassword(val);
              if (errorMsg) setErrorMsg(null);
            }}
            error={errorMsg || undefined}
          />

          <AppButton
            title={t('common.save')}
            onPress={handleSubmit}
            variant="primary"
            size="lg"
            loading={loading}
            style={{ marginTop: spacing.md }}
          />
        </AppCard>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  scrollContent: {
    padding: spacing.lg,
  },
});
