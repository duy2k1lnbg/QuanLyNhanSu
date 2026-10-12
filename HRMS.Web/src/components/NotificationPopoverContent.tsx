import React from 'react';
import { List, Button, Typography, Space, Divider, Badge } from 'antd';
import {
  BellOutlined,
  ClockCircleOutlined,
  UserAddOutlined,
  AlertOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';

const { Text } = Typography;

export interface NotificationItem {
  id: string;
  type: 'urgent' | 'warning' | 'info';
  title: string;
  time: string;
  route: string;
  read: boolean;
}

interface NotificationPopoverContentProps {
  onNavigate: (route: string) => void;
  onClose?: () => void;
  notifications: NotificationItem[];
  onMarkAllAsRead?: () => void;
  onMarkItemAsRead?: (id: string, route: string) => void;
}

export const NotificationPopoverContent: React.FC<NotificationPopoverContentProps> = ({
  onNavigate,
  onClose,
  notifications = [],
  onMarkAllAsRead,
  onMarkItemAsRead,
}) => {
  const { t } = useAppLanguage();
  const { tokens } = useAppTheme();
  const unreadCount = notifications.filter((n) => !n.read).length;

  const handleItemClick = (item: NotificationItem) => {
    if (onMarkItemAsRead) {
      onMarkItemAsRead(item.id, item.route);
    } else {
      onNavigate(item.route);
    }
    if (onClose) onClose();
  };

  const getLocalizedTitle = (rawTitle: string): string => {
    if (!rawTitle) return '';
    const tsMatch = rawTitle.match(/Bảng chấm công kỳ Tháng (\d+)\/(\d+) chưa hoàn tất khóa sổ/i);
    if (tsMatch) {
      return t('notification.timesheetUnclosed', { month: tsMatch[1], year: tsMatch[2] });
    }
    const missingMatch = rawTitle.match(/Có (\d+) hồ sơ nhân sự đang thiếu CCCD hoặc SĐT/i);
    if (missingMatch) {
      return t('notification.missingInfo', { count: missingMatch[1] });
    }
    const expMatch = rawTitle.match(/Hợp đồng lao động của (.+) sẽ hết hạn sau (\d+) ngày/i);
    if (expMatch) {
      return t('notification.contractExpiring', { name: expMatch[1], days: expMatch[2] });
    }
    return rawTitle;
  };

  const getLocalizedTime = (rawTime: string): string => {
    if (!rawTime) return '';
    if (rawTime === 'Đang mở') return t('notification.statusOpen');
    if (rawTime === 'Cần bổ sung') return t('notification.statusNeedsUpdate');
    if (rawTime === 'Gần đây') return t('notification.statusRecent');
    return rawTime;
  };

  return (
    <div className="hrms-notifications" style={{ width: 360, maxWidth: 'calc(100vw - 32px)', color: tokens.textPrimary }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '12px 16px 8px 16px' }}>
        <Space>
          <BellOutlined style={{ fontSize: 16, color: '#1677ff' }} />
          <Text strong style={{ fontSize: 15 }}>{t('notification.systemTitle')}</Text>
          {unreadCount > 0 && <Badge count={unreadCount} style={{ backgroundColor: '#ef4444' }} />}
        </Space>
        {unreadCount > 0 && onMarkAllAsRead && (
          <Button type="link" size="small" onClick={onMarkAllAsRead} style={{ padding: 0, color: tokens.primary }}>
            {t('notification.markAllRead')}
          </Button>
        )}
      </div>

      <Divider style={{ margin: '4px 0' }} />

      {notifications.length === 0 ? (
        <div style={{ padding: '36px 16px', textAlign: 'center' }}>
          <CheckCircleOutlined style={{ fontSize: 36, color: tokens.successText, marginBottom: 10 }} />
          <div style={{ fontSize: 14, fontWeight: 600, color: tokens.textPrimary }}>
            {t('notification.emptyTitle')}
          </div>
          <Text type="secondary" style={{ fontSize: 12, color: tokens.textSecondary }}>
            {t('notification.emptyDesc')}
          </Text>
        </div>
      ) : (
        <List
          dataSource={notifications}
          renderItem={(item) => {
            let dotColor = tokens.infoText;
            let iconBackground = tokens.infoBg;
            let icon = <UserAddOutlined style={{ color: tokens.infoText }} />;
            if (item.type === 'urgent') {
              dotColor = tokens.dangerText;iconBackground=tokens.dangerBg;
              icon = <AlertOutlined style={{ color: tokens.dangerText }} />;
            } else if (item.type === 'warning') {
              dotColor = tokens.warningText;iconBackground=tokens.warningBg;
              icon = <ClockCircleOutlined style={{ color: tokens.warningText }} />;
            }

            return (
              <div
                key={item.id}
                onClick={() => handleItemClick(item)}
                style={{
                  padding: '10px 16px',
                  cursor: 'pointer',
                  backgroundColor: item.read ? tokens.elevatedBg : tokens.cardSecondaryBg,
                  borderBottom: `1px solid ${tokens.borderMedium}`,
                  transition: 'background 0.2s',
                  display: 'flex',
                  gap: 12,
                  alignItems: 'flex-start',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = tokens.hoverBg)}
                onMouseLeave={(e) =>
                  (e.currentTarget.style.backgroundColor = item.read ? tokens.elevatedBg : tokens.cardSecondaryBg)
                }
              >
                <div
                  style={{
                    marginTop: 2,
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    backgroundColor: iconBackground,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  {icon}
                </div>

                <div style={{ flex: 1 }}>
                  <div
                    style={{
                      fontSize: 13,
                      fontWeight: item.read ? 400 : 600,
                      color: item.read ? tokens.textSecondary : tokens.textPrimary,
                      lineHeight: 1.4,
                    }}
                  >
                    {getLocalizedTitle(item.title)}
                  </div>
                  <div style={{ fontSize: 11, color: tokens.textSecondary, marginTop: 4 }}>
                    {getLocalizedTime(item.time)}
                  </div>
                </div>

                {!item.read && (
                  <div
                    style={{
                      width: 8,
                      height: 8,
                      borderRadius: '50%',
                      backgroundColor: dotColor,
                      marginTop: 6,
                      flexShrink: 0,
                    }}
                  />
                )}
              </div>
            );
          }}
        />
      )}

      {notifications.length > 0 && (
        <div style={{ padding: '8px 16px', textAlign: 'center', background: tokens.cardSecondaryBg }}>
          <Text type="secondary" style={{ fontSize: 12, color: tokens.textSecondary }}>
            {t('notification.showingCount', { count: notifications.length })}
          </Text>
        </div>
      )}
    </div>
  );
};

export default NotificationPopoverContent;
