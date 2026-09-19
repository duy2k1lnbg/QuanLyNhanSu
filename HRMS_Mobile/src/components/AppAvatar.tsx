import React from 'react';
import { View, Text, Image, StyleSheet } from 'react-native';
import { useTheme } from '../hooks/useTheme';
import { typography } from '../constants/typography';

interface AppAvatarProps {
  name?: string;
  avatarBase64?: string | null;
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

export const AppAvatar: React.FC<AppAvatarProps> = ({
  name = '',
  avatarBase64,
  size = 'md',
}) => {
  const { colors } = useTheme();

  let dimension = 48;
  let fontSize = 18;

  switch (size) {
    case 'sm':
      dimension = 36;
      fontSize = 14;
      break;
    case 'md':
      dimension = 48;
      fontSize = 18;
      break;
    case 'lg':
      dimension = 64;
      fontSize = 24;
      break;
    case 'xl':
      dimension = 88;
      fontSize = 32;
      break;
  }

  const getInitials = (text: string): string => {
    if (!text) return 'NV';
    const parts = text.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  const imageSource = avatarBase64
    ? {
        uri: avatarBase64.startsWith('data:')
          ? avatarBase64
          : `data:image/jpeg;base64,${avatarBase64}`,
      }
    : null;

  if (imageSource) {
    return (
      <Image
        source={imageSource}
        style={{
          width: dimension,
          height: dimension,
          borderRadius: dimension / 2,
        }}
      />
    );
  }

  return (
    <View
      style={{
        width: dimension,
        height: dimension,
        borderRadius: dimension / 2,
        backgroundColor: colors.primaryLight,
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <Text
        style={{
          ...typography.h3,
          fontSize,
          color: '#FFFFFF',
          fontWeight: '700',
        }}
      >
        {getInitials(name)}
      </Text>
    </View>
  );
};
