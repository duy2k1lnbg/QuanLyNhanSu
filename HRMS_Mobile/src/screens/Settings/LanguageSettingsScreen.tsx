import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView } from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useLanguage, LanguagePreference } from '../../hooks/useLanguage';
import { useTheme } from '../../hooks/useTheme';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppDivider } from '../../components/AppDivider';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const LanguageSettingsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { preference, setLanguage } = useLanguage();
  const { colors } = useTheme();

  const options: { id: LanguagePreference; label: string; flag: string }[] = [
    { id: 'system', label: t('language.followSystem'), flag: '🌐' },
    { id: 'vi', label: t('language.vietnamese'), flag: '🇻🇳' },
    { id: 'ja', label: t('language.japanese'), flag: '🇯🇵' },
    { id: 'en', label: t('language.english'), flag: '🇺🇸' },
  ];

  const handleSelect = async (id: LanguagePreference) => {
    await setLanguage(id);
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader
        title={t('settings.language')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView contentContainerStyle={styles.scrollContent}>
        <AppCard style={styles.card}>
          {options.map((opt, index) => {
            const isSelected = preference === opt.id;
            return (
              <React.Fragment key={opt.id}>
                {index > 0 && <AppDivider marginVertical={spacing.xs} />}
                <TouchableOpacity
                  activeOpacity={0.7}
                  onPress={() => handleSelect(opt.id)}
                  style={styles.optionRow}
                >
                  <View style={styles.optionLeft}>
                    <Text style={styles.flagText}>{opt.flag}</Text>
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
  flagText: {
    fontSize: 24,
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
