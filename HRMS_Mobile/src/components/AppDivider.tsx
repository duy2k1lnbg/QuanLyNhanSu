import React from 'react';
import { View, ViewStyle } from 'react-native';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';

interface AppDividerProps {
  style?: ViewStyle;
  marginVertical?: number;
}

export const AppDivider: React.FC<AppDividerProps> = ({
  style,
  marginVertical = spacing.md,
}) => {
  const { colors } = useTheme();

  return (
    <View
      style={[
        {
          height: 1,
          backgroundColor: colors.divider,
          marginVertical,
        },
        style,
      ]}
    />
  );
};
