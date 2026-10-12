import {notificationText} from '../../utils/notificationText';
import React, { useState, useRef, useCallback, useMemo } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  RefreshControl,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import {subscribeNotificationChanges} from '../../utils/notificationEvents';
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
  const insets=useSafeAreaInsets();
  const { activeLanguage } = useLanguage();

  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [filter, setFilter] = useState<'ALL' | 'UNREAD'>('ALL');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [unreadCount,setUnreadCount]=useState<number|null>(null);
  const [hasMore,setHasMore]=useState(false);
  const [loadingMore,setLoadingMore]=useState(false);
  const [markingAll,setMarkingAll]=useState(false);
  const rows=useRef<NotificationDto[]>([]);
  const generation=useRef(0);
  const moreBusy=useRef(false);

  const fetchNotifications = useCallback(async (isRefresh = false, loadMore = false) => {
    if (loadMore && moreBusy.current) return;
    const request=++generation.current;
    const offset=loadMore ? rows.current.length : 0;
    if (loadMore) {moreBusy.current=true; setLoadingMore(true);}
    else if (isRefresh) setRefreshing(true);
    else setLoading(true);
    setErrorMessage(null);
    try {
      const {items:data,unreadCount:count}=await meApi.getNotificationPage(offset,filter==='UNREAD');
      if (request!==generation.current) return;
      const combined=loadMore ? [...rows.current,...data] : data;
      rows.current=Array.from(new Map(combined.map(item=>[item.id,item])).values());
      setNotifications(rows.current);setUnreadCount(count);setHasMore(data.length===50 && (!loadMore || rows.current.length>offset));
    } catch (error: any) {
      if (request===generation.current) setErrorMessage(mapApiError(error));
    } finally {
      if (request===generation.current) {setLoading(false);setRefreshing(false);setLoadingMore(false);moreBusy.current=false;}
    }
  }, [filter]);

  useFocusEffect(useCallback(() => {
    fetchNotifications(true);
    const unsubscribe=subscribeNotificationChanges(()=>fetchNotifications(true));
    return () => {unsubscribe();generation.current++;moreBusy.current=false;};
  },[fetchNotifications]));

  const filteredNotifications=useMemo(()=>filter==='UNREAD' ? notifications.filter(item=>item.isUnread) : notifications,[filter,notifications]);
  const hasUnread=(unreadCount??0)>0 || notifications.some(item=>item.isUnread);
  const handleMarkAllRead = async () => {
    if (markingAll) return;
    setMarkingAll(true);
    try {
      await meApi.markAllNotificationsAsRead();
      await fetchNotifications(true);
    } catch (error) { Alert.alert(t('common.error'),mapApiError(error)); }
    finally {setMarkingAll(false);}
  };

  const handleItemPress = (item: NotificationDto) => {
    // The detail endpoint marks read only after verifying visibility and loading it.
    navigation.navigate('NotificationDetail', {notificationId:item.id,initialData:item});
  };

  const renderItem = ({ item }: { item: NotificationDto }) => (
    <AppCard style={styles.card} onPress={() => handleItemPress(item)}>
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
              style={[typography.bodyBold, { color: colors.text, flex: 1 }]}
              numberOfLines={1}
            >
              {notificationText(item,t).tieude}
            </Text>
            {item.isPinned && (
              <AppBadge
                label={t('notifications.pinned', 'Ghim')}
                variant="warning"
                style={{ marginLeft: spacing.xs }}
              />
            )}
            {item.isUnread && (
              <View
                style={[styles.unreadDot, { backgroundColor: colors.danger }]}
              />
            )}
          </View>

          <Text
            style={[
              typography.caption,
              { color: colors.textSecondary, marginTop: 4 },
            ]}
            numberOfLines={2}
          >
            {notificationText(item,t).noidung}
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
      {/* Filter Tabs & Quick Action Bar */}
      <View
        style={[
          styles.filterBar,
          {
            backgroundColor: colors.surfaceCard,
            paddingTop: insets.top,
            borderBottomColor: colors.border,
          },
        ]}
      >
        <View style={styles.tabGroup}>
          <TouchableOpacity
            style={[
              styles.tabButton,
              filter === 'ALL' && [
                styles.tabButtonActive,
                { borderBottomColor: colors.primary },
              ],
            ]}
            onPress={() => setFilter('ALL')}
          >
            <Text
              style={[
                typography.captionBold,
                { color: filter === 'ALL' ? colors.primary : colors.textSecondary },
              ]}
            >
              {t('notifications.all', 'Tất cả')} ({notifications.length})
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[
              styles.tabButton,
              filter === 'UNREAD' && [
                styles.tabButtonActive,
                { borderBottomColor: colors.primary },
              ],
            ]}
            onPress={() => setFilter('UNREAD')}
          >
            <View style={{ flexDirection: 'row', alignItems: 'center', gap: 4 }}>
              <Text
                style={[
                  typography.captionBold,
                  {
                    color:
                      filter === 'UNREAD' ? colors.primary : colors.textSecondary,
                  },
                ]}
              >
                {t('notifications.unread', 'Chưa đọc')}
              </Text>
              {unreadCount !== null && unreadCount > 0 && (
                <View
                  style={[
                    styles.tabBadge,
                    { backgroundColor: colors.danger },
                  ]}
                >
                  <Text style={styles.tabBadgeText}>{unreadCount}</Text>
                </View>
              )}
            </View>
          </TouchableOpacity>
        </View>

        {hasUnread && (
          <TouchableOpacity disabled={markingAll} onPress={handleMarkAllRead} style={styles.markAllBtn}>
            <Ionicons
              name="checkmark-done-outline"
              size={16}
              color={colors.primary}
            />
            <Text style={[typography.captionBold, { color: colors.primary }]}>
              {markingAll ? t('common.loading') : t('notifications.markAllRead', 'Đọc tất cả')}
            </Text>
          </TouchableOpacity>
        )}
      </View>

      {loading && !refreshing ? (
        <AppLoading />
      ) : errorMessage && notifications.length === 0 ? (
        <AppErrorState message={errorMessage} onRetry={() => fetchNotifications()} />
      ) : (
        <FlatList
          data={filteredNotifications}
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
          onEndReached={() => {if (hasMore && !refreshing && !loading && !loadingMore && !errorMessage) fetchNotifications(true,true);}}
          onEndReachedThreshold={0.3}
          ListHeaderComponent={errorMessage ? <AppErrorState message={errorMessage} onRetry={()=>fetchNotifications(true)}/> : null}
          ListFooterComponent={loadingMore ? <ActivityIndicator color={colors.primary}/> : null}
          ListEmptyComponent={
            <AppEmptyState
              icon={filter === 'UNREAD' ? 'checkmark-circle-outline' : 'notifications-off-outline'}
              title={
                filter === 'UNREAD'
                  ? t('notifications.noUnreadData', 'Bạn đã đọc tất cả thông báo!')
                  : t('notifications.noData', 'Chưa có thông báo nào')
              }
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
  filterBar: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: spacing.md,
    borderBottomWidth: 1,
  },
  tabGroup: {
    flexDirection: 'row',
    gap: spacing.lg,
  },
  tabButton: {
    paddingVertical: spacing.md,
    borderBottomWidth: 2,
    borderBottomColor: 'transparent',
  },
  tabButtonActive: {
    borderBottomWidth: 2,
  },
  tabBadge: {
    paddingHorizontal: 6,
    paddingVertical: 1,
    borderRadius: 10,
    alignItems: 'center',
    justifyContent: 'center',
  },
  tabBadgeText: {
    color: '#ffffff',
    fontSize: 10,
    fontWeight: '700',
  },
  markAllBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.sm,
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
