import React from 'react';
import { List, Button, Typography, Space, Divider, Badge } from 'antd';
import {
  BellOutlined,
  ClockCircleOutlined,
  UserAddOutlined,
  AlertOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';

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
  const unreadCount = notifications.filter((n) => !n.read).length;

  const handleItemClick = (item: NotificationItem) => {
    if (onMarkItemAsRead) {
      onMarkItemAsRead(item.id, item.route);
    } else {
      onNavigate(item.route);
    }
    if (onClose) onClose();
  };

  return (
    <div style={{ width: 360 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '12px 16px 8px 16px' }}>
        <Space>
          <BellOutlined style={{ fontSize: 16, color: '#1677ff' }} />
          <Text strong style={{ fontSize: 15 }}>Thông báo hệ thống</Text>
          {unreadCount > 0 && <Badge count={unreadCount} style={{ backgroundColor: '#ef4444' }} />}
        </Space>
        {unreadCount > 0 && onMarkAllAsRead && (
          <Button type="link" size="small" onClick={onMarkAllAsRead} style={{ padding: 0 }}>
            Đánh dấu đã đọc
          </Button>
        )}
      </div>

      <Divider style={{ margin: '4px 0' }} />

      {notifications.length === 0 ? (
        <div style={{ padding: '36px 16px', textAlign: 'center' }}>
          <CheckCircleOutlined style={{ fontSize: 36, color: '#10b981', marginBottom: 10 }} />
          <div style={{ fontSize: 14, fontWeight: 600, color: '#1e293b' }}>
            Không có thông báo khẩn
          </div>
          <Text type="secondary" style={{ fontSize: 12 }}>
            Hệ thống đang hoạt động an toàn và chuẩn mực.
          </Text>
        </div>
      ) : (
        <List
          dataSource={notifications}
          renderItem={(item) => {
            let dotColor = '#3b82f6';
            let icon = <UserAddOutlined style={{ color: '#3b82f6' }} />;
            if (item.type === 'urgent') {
              dotColor = '#ef4444';
              icon = <AlertOutlined style={{ color: '#ef4444' }} />;
            } else if (item.type === 'warning') {
              dotColor = '#f59e0b';
              icon = <ClockCircleOutlined style={{ color: '#f59e0b' }} />;
            }

            return (
              <div
                key={item.id}
                onClick={() => handleItemClick(item)}
                style={{
                  padding: '10px 16px',
                  cursor: 'pointer',
                  backgroundColor: item.read ? '#fff' : '#f8fafc',
                  borderBottom: '1px solid #f1f5f9',
                  transition: 'background 0.2s',
                  display: 'flex',
                  gap: 12,
                  alignItems: 'flex-start',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = '#f1f5f9')}
                onMouseLeave={(e) =>
                  (e.currentTarget.style.backgroundColor = item.read ? '#fff' : '#f8fafc')
                }
              >
                <div
                  style={{
                    marginTop: 2,
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    backgroundColor: `${dotColor}15`,
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
                      color: item.read ? '#475569' : '#0f172a',
                      lineHeight: 1.4,
                    }}
                  >
                    {item.title}
                  </div>
                  <div style={{ fontSize: 11, color: '#94a3b8', marginTop: 4 }}>
                    {item.time}
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
        <div style={{ padding: '8px 16px', textAlign: 'center', background: '#f8fafc' }}>
          <Text type="secondary" style={{ fontSize: 12 }}>
            Hiển thị {notifications.length} thông báo thực tế từ CSDL
          </Text>
        </div>
      )}
    </div>
  );
};

export default NotificationPopoverContent;
