import React from 'react';
import { View, Text, StyleSheet, ViewStyle } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';
import { typography } from '../constants/typography';
import { AppButton } from './AppButton';

interface AppErrorStateProps {
  message?: string;
  onRetry?: () => void;
  style?: ViewStyle;
}

export const AppErrorState: React.FC<AppErrorStateProps> = ({
  message,
  onRetry,
  style,
}) => {
  const { t } = useTranslation();
  const { colors } = useTheme();

  return (
    <View style={[styles.container, style]}>
      <View
        style={[
          styles.iconCircle,
          { backgroundColor: 'rgba(239, 68, 68, 0.1)', borderColor: colors.danger },
        ]}
      >
        <Ionicons name="alert-circle-outline" size={48} color={colors.danger} />
      </View>

      <Text
        style={[
          typography.h3,
          { color: colors.text, textAlign: 'center', marginTop: spacing.lg },
        ]}
      >
        {t('common.error')}
      </Text>

      <Text
        style={[
          typography.body,
          {
            color: colors.textSecondary,
            textAlign: 'center',
            marginTop: spacing.xs,
            maxWidth: 300,
          },
        ]}
      >
        {message || t('errors.unknown')}
      </Text>

      {onRetry && (
        <AppButton
          title={t('common.retry')}
          onPress={onRetry}
          variant="primary"
          size="md"
          icon={<Ionicons name="refresh-outline" size={18} color="#FFFFFF" />}
          style={{ marginTop: spacing.xl, minWidth: 160 }}
        />
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
    padding: spacing.xxl,
  },
  iconCircle: {
    width: 88,
    height: 88,
    borderRadius: 44,
    borderWidth: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
