import React from 'react';
import { View, Text, StyleSheet, ViewStyle } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';
import { typography } from '../constants/typography';
import { AppButton } from './AppButton';

interface AppEmptyStateProps {
  title?: string;
  message?: string;
  icon?: keyof typeof Ionicons.glyphMap;
  actionText?: string;
  onAction?: () => void;
  style?: ViewStyle;
}

export const AppEmptyState: React.FC<AppEmptyStateProps> = ({
  title,
  message,
  icon = 'file-tray-outline',
  actionText,
  onAction,
  style,
}) => {
  const { t } = useTranslation();
  const { colors } = useTheme();

  return (
    <View style={[styles.container, style]}>
      <View
        style={[
          styles.iconCircle,
          { backgroundColor: colors.surfaceCard, borderColor: colors.border },
        ]}
      >
        <Ionicons name={icon} size={48} color={colors.textMuted} />
      </View>

      <Text
        style={[
          typography.h3,
          { color: colors.text, textAlign: 'center', marginTop: spacing.lg },
        ]}
      >
        {title || t('common.empty')}
      </Text>

      {message ? (
        <Text
          style={[
            typography.body,
            {
              color: colors.textSecondary,
              textAlign: 'center',
              marginTop: spacing.xs,
              maxWidth: 280,
            },
          ]}
        >
          {message}
        </Text>
      ) : null}

      {actionText && onAction ? (
        <AppButton
          title={actionText}
          onPress={onAction}
          variant="outline"
          size="sm"
          style={{ marginTop: spacing.xl }}
        />
      ) : null}
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
