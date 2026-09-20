import React, { useEffect } from 'react';
import { View, Text, StyleSheet, ActivityIndicator } from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { storage } from '../../utils/storage';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

type Props = NativeStackScreenProps<RootStackParamList, 'Splash'>;

export const SplashScreen: React.FC<Props> = ({ navigation }) => {
  const { isInitializing, isAuthenticated } = useAuth();
  const { colors } = useTheme();

  useEffect(() => {
    if (isInitializing) return;

    const checkNavigation = async () => {
      // Check if user has launched app before
      const isFirstRun = await storage.getFirstRun();
      if (isFirstRun) {
        navigation.replace('LanguageSelect');
        return;
      }

      if (isAuthenticated) {
        navigation.replace('Main');
      } else {
        navigation.replace('Login');
      }
    };

    // Small delay to present branding smoothly
    const timer = setTimeout(() => {
      checkNavigation();
    }, 600);

    return () => clearTimeout(timer);
  }, [isInitializing, isAuthenticated, navigation]);

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.logoCircle, { backgroundColor: colors.primary }]}>
        <Text style={styles.logoText}>HRMS</Text>
      </View>
      <Text style={[typography.h1, { color: colors.text, marginTop: spacing.xl }]}>
        HRMS
      </Text>
      <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.xs }]}>
        Human Resource Management System
      </Text>
      <View style={styles.loadingWrapper}>
        <ActivityIndicator size="small" color={colors.primary} />
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: spacing.xxl,
  },
  logoCircle: {
    width: 96,
    height: 96,
    borderRadius: 24,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15,
    shadowRadius: 8,
  },
  logoText: {
    color: '#FFFFFF',
    fontSize: 26,
    fontWeight: '800',
    letterSpacing: 1.5,
  },
  loadingWrapper: {
    position: 'absolute',
    bottom: 60,
  },
});
