import React from 'react';
import { View, Text, StyleSheet, ViewStyle } from 'react-native';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';
import { typography } from '../constants/typography';

export type BadgeVariant = 'success' | 'warning' | 'danger' | 'info' | 'default';

interface AppBadgeProps {
  label: string | number;
  variant?: BadgeVariant;
  style?: ViewStyle;
}

export const AppBadge: React.FC<AppBadgeProps> = ({
  label,
  variant = 'default',
  style,
}) => {
  const { colors, isDark } = useTheme();

  const getBadgeColors = () => {
    switch (variant) {
      case 'success':
        return {
          bg: isDark ? 'rgba(52, 211, 153, 0.15)' : '#D1FAE5',
          text: colors.success,
        };
      case 'warning':
        return {
          bg: isDark ? 'rgba(251, 191, 36, 0.15)' : '#FEF3C7',
          text: colors.warning,
        };
      case 'danger':
        return {
          bg: isDark ? 'rgba(248, 113, 113, 0.15)' : '#FEE2E2',
          text: colors.danger,
        };
      case 'info':
        return {
          bg: isDark ? 'rgba(56, 189, 248, 0.15)' : '#E0F2FE',
          text: colors.info,
        };
      default:
        return {
          bg: isDark ? colors.border : '#F1F5F9',
          text: colors.textSecondary,
        };
    }
  };

  const badgeColors = getBadgeColors();

  return (
    <View
      style={[
        styles.badge,
        { backgroundColor: badgeColors.bg },
        style,
      ]}
    >
      <Text style={[typography.badge, { color: badgeColors.text }]}>
        {label}
      </Text>
    </View>
  );
};

const styles = StyleSheet.create({
  badge: {
    paddingHorizontal: spacing.sm,
    paddingVertical: 3,
    borderRadius: spacing.borderRadiusFull,
    alignSelf: 'flex-start',
    alignItems: 'center',
    justifyContent: 'center',
  },
});
