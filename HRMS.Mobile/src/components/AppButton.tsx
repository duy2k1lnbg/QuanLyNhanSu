import React from 'react';
import {
  TouchableOpacity,
  Text,
  ActivityIndicator,
  StyleSheet,
  ViewStyle,
  TextStyle,
} from 'react-native';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';
import { typography } from '../constants/typography';

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg';

interface AppButtonProps {
  title: string;
  onPress: () => void;
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  disabled?: boolean;
  style?: ViewStyle;
  textStyle?: TextStyle;
  icon?: React.ReactNode;
}

export const AppButton: React.FC<AppButtonProps> = ({
  title,
  onPress,
  variant = 'primary',
  size = 'md',
  loading = false,
  disabled = false,
  style,
  textStyle,
  icon,
}) => {
  const { colors } = useTheme();

  const getContainerStyle = (): ViewStyle => {
    let bg = colors.primary;
    let border: string | undefined = undefined;

    switch (variant) {
      case 'primary':
        bg = colors.primary;
        break;
      case 'secondary':
        bg = colors.surfaceCard;
        border = colors.border;
        break;
      case 'outline':
        bg = 'transparent';
        border = colors.primary;
        break;
      case 'danger':
        bg = colors.danger;
        break;
    }

    let paddingVertical = spacing.md;
    let paddingHorizontal = spacing.xl;
    let minHeight = 48;

    if (size === 'sm') {
      paddingVertical = spacing.sm;
      paddingHorizontal = spacing.md;
      minHeight = 36;
    } else if (size === 'lg') {
      paddingVertical = spacing.lg;
      paddingHorizontal = spacing.xxl;
      minHeight = 54;
    }

    return {
      backgroundColor: disabled ? colors.border : bg,
      borderWidth: border ? 1.5 : 0,
      borderColor: disabled ? colors.border : border,
      borderRadius: spacing.borderRadiusMd,
      paddingVertical,
      paddingHorizontal,
      minHeight,
      alignItems: 'center',
      justifyContent: 'center',
      flexDirection: 'row',
      opacity: disabled ? 0.6 : 1,
    };
  };

  const getTextColor = (): string => {
    if (disabled) return colors.textMuted;
    switch (variant) {
      case 'primary':
      case 'danger':
        return '#FFFFFF';
      case 'secondary':
        return colors.text;
      case 'outline':
        return colors.primary;
      default:
        return '#FFFFFF';
    }
  };

  const getTextSizeStyle = (): TextStyle => {
    if (size === 'sm') return typography.captionBold;
    if (size === 'lg') return { ...typography.bodyBold, fontSize: 17 };
    return typography.bodyBold;
  };

  return (
    <TouchableOpacity
      activeOpacity={0.8}
      onPress={onPress}
      disabled={disabled || loading}
      style={[getContainerStyle(), style]}
    >
      {loading ? (
        <ActivityIndicator color={getTextColor()} size="small" />
      ) : (
        <>
          {icon && <>{icon}</>}
          <Text
            style={[
              getTextSizeStyle(),
              { color: getTextColor(), marginLeft: icon ? spacing.sm : 0 },
              textStyle,
            ]}
          >
            {title}
          </Text>
        </>
      )}
    </TouchableOpacity>
  );
};
