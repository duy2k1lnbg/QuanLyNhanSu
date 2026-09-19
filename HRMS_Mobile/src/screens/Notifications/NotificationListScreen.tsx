import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  RefreshControl,
  TouchableOpacity,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../navigation/types';
import { useTheme } from '../../hooks/useTheme';
import { useLanguage } from '../../hooks/useLanguage';
import { meApi } from '../../api/meApi';
import { NotificationDto } from '../../types/me';
import { AppCard } from '../../components/AppCard';
import { AppBadge } from '../../components/AppBadge';
import { AppLoading } from '../../components/AppLoading';
import { AppEmptyState } from '../../components/AppEmptyState';
import { AppErrorState } from '../../components/AppErrorState';
import { formatDate } from '../../utils/formatters';
import { mapApiError } from '../../utils/errorMapper';
import { spacing } from '../../constants/spacing';
import { typography } from '../../constants/typography';

export const NotificationListScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { colors } = useTheme();
  const { activeLanguage } = useLanguage();

  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchNotifications = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);

    try {
      const data = await meApi.getNotifications();
      setNotifications(data);
    } catch (error: any) {
      setErrorMessage(mapApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchNotifications();
  }, [fetchNotifications]);

  const renderItem = ({ item }: { item: NotificationDto }) => (
    <AppCard
      style={styles.card}
      onPress={() =>
        navigation.navigate('NotificationDetail', {
          notificationId: item.id,
          initialData: item,
        })
      }
    >
      <View style={styles.cardRow}>
        <View
          style={[
            styles.iconWrapper,
            {
              backgroundColor: item.isPinned
                ? 'rgba(245, 158, 11, 0.15)'
                : 'rgba(59, 130, 246, 0.1)',
            },
          ]}
        >
          <Ionicons
            name={item.isPinned ? 'pin' : 'newspaper-outline'}
            size={22}
            color={item.isPinned ? colors.warning : colors.primary}
          />
        </View>

        <View style={styles.contentColumn}>
          <View style={styles.headerLine}>
            <Text
              style={[
                typography.bodyBold,
                { color: colors.text, flex: 1 },
              ]}
              numberOfLines={1}
            >
              {item.tieude}
            </Text>
            {item.isPinned && (
              <AppBadge label={t('notifications.pinned')} variant="warning" style={{ marginLeft: spacing.xs }} />
            )}
            {item.isUnread && <View style={[styles.unreadDot, { backgroundColor: colors.danger }]} />}
          </View>

          <Text
            style={[
              typography.caption,
              { color: colors.textSecondary, marginTop: 4 },
            ]}
            numberOfLines={2}
          >
            {item.noidung}
          </Text>

          <View style={styles.footerRow}>
            <Text style={[typography.caption, { color: colors.textMuted }]}>
              {formatDate(item.ngaydang, activeLanguage)} • {item.nguoidang || 'HR'}
            </Text>
            {item.fileDinhkem ? (
              <View style={styles.attachmentBadge}>
                <Ionicons name="attach" size={14} color={colors.textSecondary} />
              </View>
            ) : null}
          </View>
        </View>
      </View>
    </AppCard>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {loading && !refreshing ? (
        <AppLoading />
      ) : errorMessage && notifications.length === 0 ? (
        <AppErrorState message={errorMessage} onRetry={() => fetchNotifications()} />
      ) : (
        <FlatList
          data={notifications}
          keyExtractor={(item) => String(item.id)}
          renderItem={renderItem}
          contentContainerStyle={styles.listContent}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => fetchNotifications(true)}
              colors={[colors.primary]}
              tintColor={colors.primary}
            />
          }
          ListEmptyComponent={
            <AppEmptyState
              icon="notifications-off-outline"
              title={t('notifications.noData')}
            />
          }
        />
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  listContent: {
    padding: spacing.lg,
    paddingBottom: 40,
    flexGrow: 1,
  },
  card: {
    marginBottom: spacing.md,
    padding: spacing.md,
  },
  cardRow: {
    flexDirection: 'row',
    gap: spacing.md,
  },
  iconWrapper: {
    width: 44,
    height: 44,
    borderRadius: 22,
    alignItems: 'center',
    justifyContent: 'center',
  },
  contentColumn: {
    flex: 1,
  },
  headerLine: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  unreadDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    marginLeft: spacing.sm,
  },
  footerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: spacing.sm,
  },
  attachmentBadge: {
    padding: 2,
  },
});
