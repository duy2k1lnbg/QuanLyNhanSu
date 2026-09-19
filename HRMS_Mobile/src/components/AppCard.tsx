import React from 'react';
import {
  View,
  TouchableOpacity,
  StyleSheet,
  ViewStyle,
  StyleProp,
} from 'react-native';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';

interface AppCardProps {
  children: React.ReactNode;
  style?: StyleProp<ViewStyle>;
  onPress?: () => void;
  bordered?: boolean;
}

export const AppCard: React.FC<AppCardProps> = ({
  children,
  style,
  onPress,
  bordered = true,
}) => {
  const { colors, isDark } = useTheme();

  const cardStyle: ViewStyle = {
    backgroundColor: colors.surfaceCard,
    borderRadius: spacing.borderRadiusLg,
    padding: spacing.lg,
    borderWidth: bordered ? 1 : 0,
    borderColor: colors.border,
    shadowColor: isDark ? '#000000' : colors.cardShadow,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: isDark ? 0.4 : 0.08,
    shadowRadius: 8,
    elevation: isDark ? 2 : 3,
  };

  if (onPress) {
    return (
      <TouchableOpacity
        activeOpacity={0.75}
        onPress={onPress}
        style={[cardStyle, style]}
      >
        {children}
      </TouchableOpacity>
    );
  }

  return <View style={[cardStyle, style]}>{children}</View>;
};
