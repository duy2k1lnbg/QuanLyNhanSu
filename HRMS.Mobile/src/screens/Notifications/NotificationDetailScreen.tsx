import {notificationText} from '../../utils/notificationText';
import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { NotificationDto } from '../../types/me';
import { AppHeader } from '../../components/AppHeader';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppErrorState } from '../../components/AppErrorState';
import { AppDivider } from '../../components/AppDivider';
import { formatDate } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

type Props = NativeStackScreenProps<RootStackParamList, 'NotificationDetail'>;

export const NotificationDetailScreen: React.FC<Props> = ({ route, navigation }) => {
  const { notificationId, initialData } = route.params;
  const { t } = useTranslation();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [notification, setNotification] = useState<NotificationDto | null>(initialData || null);
  const [loading, setLoading] = useState(!initialData);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [attempt,setAttempt]=useState(0);
  const retry=useCallback(()=>setAttempt(value=>value+1),[]);
  useEffect(() => {
    let active=true;
    setLoading(true);setErrorMessage(null);
    meApi.getNotificationDetail(notificationId)
      .then(data=>{if(active) setNotification({...data,isUnread:false});})
      .catch(error=>{if(active) setErrorMessage(mapApiError(error));})
      .finally(()=>{if(active) setLoading(false);});
    return ()=>{active=false;};
  }, [notificationId,attempt]);

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <AppHeader
        title={t('notifications.detailTitle')}
        showBack
        onBack={() => navigation.goBack()}
      />

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {loading ? (
          <AppLoading />
        ) : errorMessage ? (
          <AppErrorState message={errorMessage} onRetry={retry} />
        ) : notification ? (
          <AppCard>
            <View style={styles.headerRow}>
              {notification.isPinned && (
                <AppBadge label={t('notifications.pinned')} variant="warning" style={{ marginBottom: spacing.sm }} />
              )}
              <Text style={[typography.h2, { color: colors.text }]}>
                {notificationText(notification,t).tieude}
              </Text>

              <View style={styles.metaRow}>
                <Ionicons name="person-outline" size={16} color={colors.textSecondary} />
                <Text style={[typography.captionBold, { color: colors.textSecondary, marginLeft: 4 }]}>
                  {notification.nguoidang || 'HR'}
                </Text>
                <Text style={[typography.caption, { color: colors.textMuted, marginHorizontal: spacing.xs }]}>
                  •
                </Text>
                <Ionicons name="time-outline" size={16} color={colors.textSecondary} />
                <Text style={[typography.caption, { color: colors.textSecondary, marginLeft: 4 }]}>
                  {formatDate(notification.ngaydang, activeLanguage)}
                </Text>
              </View>
            </View>

            <AppDivider marginVertical={spacing.lg} />

            <Text style={[typography.body, { color: colors.text, lineHeight: 24 }]}>
              {notificationText(notification,t).noidung}
            </Text>

            {notification.fileDinhkem ? (
              <View style={[styles.attachmentBox, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
                <Ionicons name="document-attach-outline" size={24} color={colors.primary} />
                <View style={{ flex: 1, marginLeft: spacing.sm }}>
                  <Text style={[typography.captionBold, { color: colors.text }]}>
                    {t('notifications.attachment')}
                  </Text>
                  <Text style={[typography.caption, { color: colors.textSecondary }]}>
                    {notification.fileDinhkem}
                  </Text>
                </View>
              </View>
            ) : null}
          </AppCard>
        ) : null}
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
    paddingBottom: 40,
  },
  headerRow: {
    marginBottom: spacing.xs,
  },
  metaRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    alignItems: 'center',
    marginTop: spacing.sm,
  },
  attachmentBox: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: spacing.md,
    borderRadius: spacing.borderRadiusMd,
    borderWidth: 1,
    marginTop: spacing.xl,
  },
});
