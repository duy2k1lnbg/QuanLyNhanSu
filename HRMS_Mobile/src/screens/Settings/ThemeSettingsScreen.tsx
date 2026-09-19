import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView } from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useTheme, ThemeMode } from '../../hooks/useTheme';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppDivider } from '../../components/AppDivider';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const ThemeSettingsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { mode, setThemeMode, colors } = useTheme();

  const options: { id: ThemeMode; label: string; icon: keyof typeof Ionicons.glyphMap }[] = [
    { id: 'system', label: t('settings.themeSystem'), icon: 'phone-portrait-outline' },
    { id: 'light', label: t('settings.themeLight'), icon: 'sunny-outline' },
    { id: 'dark', label: t('settings.themeDark'), icon: 'moon-outline' },
  ];

  const handleSelect = async (id: ThemeMode) => {
    await setThemeMode(id);
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader
        title={t('settings.theme')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView contentContainerStyle={styles.scrollContent}>
        <AppCard style={styles.card}>
          {options.map((opt, index) => {
            const isSelected = mode === opt.id;
            return (
              <React.Fragment key={opt.id}>
                {index > 0 && <AppDivider marginVertical={spacing.xs} />}
                <TouchableOpacity
                  activeOpacity={0.7}
                  onPress={() => handleSelect(opt.id)}
                  style={styles.optionRow}
                >
                  <View style={styles.optionLeft}>
                    <Ionicons
                      name={opt.icon}
                      size={24}
                      color={isSelected ? colors.primary : colors.textSecondary}
                    />
                    <Text
                      style={[
                        typography.bodyBold,
                        { color: isSelected ? colors.primary : colors.text, marginLeft: spacing.md },
                      ]}
                    >
                      {opt.label}
                    </Text>
                  </View>
                  <View
                    style={[
                      styles.radioCircle,
                      {
                        borderColor: isSelected ? colors.primary : colors.border,
                        backgroundColor: isSelected ? colors.primary : 'transparent',
                      },
                    ]}
                  >
                    {isSelected && <Ionicons name="checkmark" size={14} color="#FFFFFF" />}
                  </View>
                </TouchableOpacity>
              </React.Fragment>
            );
          })}
        </AppCard>
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    padding: spacing.lg,
  },
  card: {
    padding: spacing.sm,
  },
  optionRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
  },
  optionLeft: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  radioCircle: {
    width: 22,
    height: 22,
    borderRadius: 11,
    borderWidth: 2,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
