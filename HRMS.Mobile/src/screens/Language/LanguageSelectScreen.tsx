import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useLanguage, LanguagePreference } from '../../hooks/useLanguage';
import { useTheme } from '../../hooks/useTheme';
import { storage } from '../../utils/storage';
import { AppButton } from '../../components/AppButton';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

type Props = NativeStackScreenProps<RootStackParamList, 'LanguageSelect'>;

export const LanguageSelectScreen: React.FC<Props> = ({ navigation }) => {
  const { t } = useTranslation();
  const { preference, setLanguage } = useLanguage();
  const { colors } = useTheme();
  const [selected, setSelected] = useState<LanguagePreference>(preference);

  const languages: { id: LanguagePreference; title: string; flag: string; desc: string }[] = [
    { id: 'system', title: t('language.followSystem'), flag: '🌐', desc: 'Auto detect system locale' },
    { id: 'vi', title: 'Tiếng Việt', flag: '🇻🇳', desc: 'Vietnamese' },
    { id: 'en', title: 'English', flag: '🇺🇸', desc: 'English (US)' },
    { id: 'zh-CN', title: '简体中文', flag: '🇨🇳', desc: 'Simplified Chinese' },
    { id: 'ko', title: '한국어', flag: '🇰🇷', desc: 'Korean' },
    { id: 'ja', title: '日本語', flag: '🇯🇵', desc: 'Japanese' },
  ];

  const handleSelect = async (id: LanguagePreference) => {
    setSelected(id);
    await setLanguage(id);
  };

  const handleContinue = async () => {
    await storage.setHasRunBefore();
    navigation.replace('Login');
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.header}>
          <View style={[styles.iconCircle, { backgroundColor: colors.primary }]}>
            <Ionicons name="language-outline" size={32} color="#FFFFFF" />
          </View>
          <Text style={[typography.h1, { color: colors.text, marginTop: spacing.lg }]}>
            {t('language.selectLanguage')}
          </Text>
          <Text style={[typography.caption, { color: colors.textSecondary, marginTop: spacing.xs, textAlign: 'center' }]}>
            {t('auth.subtitle')}
          </Text>
        </View>

        <View style={styles.list}>
          {languages.map((item) => {
            const isSelected = selected === item.id;
            return (
              <TouchableOpacity
                key={item.id}
                activeOpacity={0.75}
                onPress={() => handleSelect(item.id)}
                style={[
                  styles.optionCard,
                  {
                    backgroundColor: colors.surfaceCard,
                    borderColor: isSelected ? colors.primary : colors.border,
                    borderWidth: isSelected ? 2 : 1,
                  },
                ]}
              >
                <View style={styles.leftInfo}>
                  <Text style={styles.flagText}>{item.flag}</Text>
                  <View style={styles.textColumn}>
                    <Text style={[typography.bodyBold, { color: colors.text }]}>
                      {item.title}
                    </Text>
                    <Text style={[typography.caption, { color: colors.textSecondary }]}>
                      {item.desc}
                    </Text>
                  </View>
                </View>

                <View
                  style={[
                    styles.radio,
                    {
                      borderColor: isSelected ? colors.primary : colors.border,
                      backgroundColor: isSelected ? colors.primary : 'transparent',
                    },
                  ]}
                >
                  {isSelected && <Ionicons name="checkmark" size={14} color="#FFFFFF" />}
                </View>
              </TouchableOpacity>
            );
          })}
        </View>
      </ScrollView>

      <View style={[styles.footer, { backgroundColor: colors.surfaceCard, borderTopColor: colors.border }]}>
        <AppButton
          title={t('language.continue')}
          onPress={handleContinue}
          variant="primary"
          size="lg"
        />
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    padding: spacing.xxl,
    paddingBottom: 100,
  },
  header: {
    alignItems: 'center',
    marginTop: spacing.xxxl,
    marginBottom: spacing.xxl,
  },
  iconCircle: {
    width: 64,
    height: 64,
    borderRadius: 32,
    alignItems: 'center',
    justifyContent: 'center',
  },
  list: {
    gap: spacing.md,
  },
  optionCard: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: spacing.lg,
    borderRadius: spacing.borderRadiusLg,
  },
  leftInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  flagText: {
    fontSize: 28,
  },
  textColumn: {
    gap: 2,
  },
  radio: {
    width: 22,
    height: 22,
    borderRadius: 11,
    borderWidth: 2,
    alignItems: 'center',
    justifyContent: 'center',
  },
  footer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: spacing.lg,
    borderTopWidth: 1,
  },
});
