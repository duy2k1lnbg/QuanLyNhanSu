import React, { useState, useEffect } from 'react';
import {
  Layout,
  Menu,
  Typography,
  Space,
  Button,
  Tag,
  Avatar,
  Tooltip,
  Dropdown,
  Badge,
} from 'antd';
import {
  MenuOutlined,
  DashboardOutlined,
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  FileTextOutlined,
  RobotOutlined,
  BellOutlined,
  UserOutlined,
  ReloadOutlined,
  LogoutOutlined,
  SafetyCertificateOutlined,
  TrophyOutlined,
  SwapOutlined,
  RiseOutlined,
  KeyOutlined,
  SearchOutlined,
  PlusOutlined,
  UserAddOutlined,
  FileAddOutlined,
  HomeOutlined,
  GlobalOutlined,
  DownOutlined,
  CheckOutlined,
  CheckSquareOutlined,
  AuditOutlined,
} from '@ant-design/icons';
import { Popover } from 'antd';
import NotificationPopoverContent, { type NotificationItem } from './NotificationPopoverContent';
import type { CurrentUserDTO } from '../types/hrms';
import { useAppLanguage, type AppLanguage } from '../services/i18n';
import {
  canView,
  canAdd,
  canEdit,
} from '../utils/permissionUtils';

const { Header, Content, Sider } = Layout;
const { Text } = Typography;

interface MainLayoutProps {
  currentUser: CurrentUserDTO;
  currentMenu: string;
  onMenuChange: (key: string) => void;
  onLogout: () => void;
  onRefreshData: () => void;
  onOpenChangePassword: () => void;
  onOpenAiDrawer: () => void;
  onOpenCommandPalette?: () => void;
  isBackendConnected: boolean;
  loading: boolean;
  kyCongCount: number;
  hasRight?: (...codes: string[]) => boolean;
  canView?: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
  canAccessRoute?: (route: string) => boolean;
  notifications?: NotificationItem[];
  onMarkAllNotificationsRead?: () => void;
  onMarkNotificationRead?: (id: string, route: string) => void;
  children: React.ReactNode;
}

export function MainLayout({
  currentUser,
  currentMenu,
  onMenuChange,
  onLogout,
  onRefreshData,
  onOpenChangePassword,
  onOpenAiDrawer,
  onOpenCommandPalette,
  isBackendConnected,
  loading,
  kyCongCount,
  notifications = [],
  onMarkAllNotificationsRead,
  onMarkNotificationRead,
  children,
}: MainLayoutProps) {
  const [isMobile, setIsMobile] = useState<boolean>(() => typeof window !== 'undefined' && window.innerWidth < 768);
  const [collapsed, setCollapsed] = useState<boolean>(() => typeof window !== 'undefined' && window.innerWidth < 992);

  // Hook đa ngôn ngữ toàn hệ thống (Anh, Việt, Nhật)
  const { lang: currentLang, setLang: handleLanguageChange, tApp, allConfigs } = useAppLanguage();

  const unreadNotifCount = notifications ? notifications.filter((n) => !n.read).length : 0;

  useEffect(() => {
    const handleResize = () => {
      const mobile = window.innerWidth < 768;
      setIsMobile(mobile);
      if (window.innerWidth < 992) {
        setCollapsed(true);
      }
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // Danh mục phân hệ Sidebar được bảo vệ chặt chẽ theo quyền XEM
  const menuItems = [
    ...(canView(currentUser, 'DASHBOARD', 'BAOCAO', 'F_DB_LUONG', 'F_DB_NHANSU', 'F_BC_BAOCAO')
      ? [{ key: 'dashboard', icon: <DashboardOutlined />, label: tApp.menuDashboard }]
      : []),
    ...(canView(currentUser, 'NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN', 'NHANVIEN')
      ? [{ key: 'nhanvien', icon: <TeamOutlined />, label: tApp.menuEmployees }]
      : []),
    ...(canView(currentUser, 'CHAMCONG', 'F_CC_BANGCONG', 'F_CC_LOAICA', 'F_CC_KYCONG')
      ? [{ key: 'chamcong', icon: <CalendarOutlined />, label: tApp.menuAttendance }]
      : []),
    ...(canView(currentUser, 'BANGLUONG', 'F_CC_BANGLUONG', 'LUONG')
      ? [{ key: 'bangluong', icon: <DollarOutlined />, label: tApp.menuPayroll }]
      : []),
    ...(canView(currentUser, 'HOPDONG', 'F_NV_HOPDONG')
      ? [{ key: 'hopdong', icon: <FileTextOutlined />, label: tApp.menuContracts }]
      : []),
    ...(canView(currentUser, 'KHENTHUONG', 'KYLUAT', 'F_NV_KHENTHUONG', 'F_NV_KYLUAT')
      ? [{ key: 'khenthuong', icon: <TrophyOutlined />, label: tApp.menuRewards }]
      : []),
    ...(canView(currentUser, 'NANGLUONG', 'DIEUCHUYEN', 'F_NV_NANGLUONG', 'F_NV_DIEUCHUYEN')
      ? [{ key: 'nangluong', icon: <RiseOutlined />, label: tApp.menuPromotions }]
      : []),
    ...(canView(currentUser, 'UNGLUONG', 'TANGCA', 'F_CC_UNGLUONG', 'F_CC_TANGCA')
      ? [{ key: 'ungluong', icon: <SwapOutlined />, label: tApp.menuOvertime }]
      : []),
    ...(canView(currentUser, 'APPROVE', 'CHAMCONG', 'F_CC_BANGCONG', 'F_CC_TANGCA', 'F_NV_NHANVIEN')
      ? [{ key: 'approvals', icon: <CheckSquareOutlined />, label: tApp.menuApprovals }]
      : []),
    ...(currentUser.IsAdmin || canView(currentUser, 'PHANQUYEN', 'F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'F_SYSTEM_LOCK_USER')
      ? [{ key: 'phanquyen', icon: <SafetyCertificateOutlined />, label: tApp.menuPermissions }]
      : []),
    ...(currentUser.IsAdmin || canView(currentUser, 'AUDIT', 'PHANQUYEN', 'F_SYSTEM_USER')
      ? [{ key: 'audit', icon: <AuditOutlined />, label: tApp.menuAudit }]
      : []),
    {
      key: 'ai-drawer',
      icon: <RobotOutlined style={{ color: '#b37feb' }} />,
      label: (
        <span>
          {tApp.menuAiCopilot} <Tag color="purple" style={{ marginLeft: 4, fontSize: 10 }}>{tApp.aiBadge}</Tag>
        </span>
      ),
    },
  ];

  // Danh mục Thao tác nhanh (Quick Add): Chỉ hiển thị các hành động người dùng có quyền Thêm/Sửa
  const quickActionItems = [
    ...(canAdd(currentUser, 'F_DM_NHANVIEN') && canView(currentUser, 'F_DM_NHANVIEN')
      ? [
          {
            key: 'quick-emp',
            icon: <UserAddOutlined style={{ color: '#10b981' }} />,
            label: tApp.addEmployee,
            onClick: () => onMenuChange('nhanvien'),
          },
        ]
      : []),
    ...(canAdd(currentUser, 'F_NV_HOPDONG') && canView(currentUser, 'F_NV_HOPDONG')
      ? [
          {
            key: 'quick-contract',
            icon: <FileAddOutlined style={{ color: '#3b82f6' }} />,
            label: tApp.createContract,
            onClick: () => onMenuChange('hopdong'),
          },
        ]
      : []),
    ...(canAdd(currentUser, 'F_CC_BANGCONG', 'F_NV_NGHIPHEP') && canView(currentUser, 'F_CC_BANGCONG')
      ? [
          {
            key: 'quick-leave',
            icon: <CalendarOutlined style={{ color: '#f59e0b' }} />,
            label: tApp.createLeave,
            onClick: () => onMenuChange('chamcong'),
          },
        ]
      : []),
    ...(canView(currentUser, 'F_CC_BANGCONG')
      ? [
          {
            key: 'quick-timesheet',
            icon: <CalendarOutlined style={{ color: '#8b5cf6' }} />,
            label: tApp.enterTimesheet,
            onClick: () => onMenuChange('chamcong'),
          },
        ]
      : []),
    ...(canEdit(currentUser, 'F_CC_BANGLUONG') || canAdd(currentUser, 'F_CC_BANGLUONG')
      ? [
          {
            key: 'quick-salary',
            icon: <DollarOutlined style={{ color: '#059669' }} />,
            label: tApp.calculateSalary,
            onClick: () => onMenuChange('bangluong'),
          },
        ]
      : []),
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {isMobile && !collapsed && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)',
            zIndex: 99,
          }}
          onClick={() => setCollapsed(true)}
        />
      )}

      <Sider
        trigger={null}
        collapsible
        breakpoint="lg"
        collapsedWidth={0}
        onBreakpoint={(broken) => {
          setIsMobile(broken);
          if (broken) setCollapsed(true);
        }}
        collapsed={collapsed}
        onCollapse={(val) => setCollapsed(val)}
        width={250}
        theme="dark"
        style={{
          boxShadow: '2px 0 8px 0 rgba(29,35,41,.05)',
          zIndex: 100,
          position: isMobile ? 'fixed' : 'relative',
          height: isMobile ? '100vh' : 'auto',
          left: 0,
          top: 0,
          bottom: 0,
        }}
      >
        <div
          onClick={() => onMenuChange('dashboard')}
          title="Về bảng điều khiển chính (Dashboard / Home)"
          style={{
            height: 48,
            margin: '12px 16px',
            background: 'linear-gradient(135deg, #1677ff 0%, #0958d9 100%)',
            borderRadius: 8,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 'bold',
            fontSize: collapsed ? '14px' : '17px',
            letterSpacing: '0.5px',
            cursor: 'pointer',
            userSelect: 'none',
            transition: 'all 0.2s ease',
          }}
        >
          {collapsed ? 'HR' : '⚡ HRMS PORTAL'}
        </div>

        <Menu
          theme="dark"
          selectedKeys={[currentMenu]}
          mode="inline"
          items={menuItems}
          onClick={(info) => {
            if (info.key === 'ai-drawer') {
              onOpenAiDrawer();
            } else {
              onMenuChange(info.key);
            }
            if (isMobile) setCollapsed(true);
          }}
        />
      </Sider>

      <Layout style={{ minWidth: 0 }}>
        <Header
          style={{
            padding: isMobile ? '0 10px' : '0 20px',
            background: '#fff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            boxShadow: '0 1px 4px rgba(0,21,41,.08)',
            zIndex: 10,
            height: 60,
            lineHeight: 'normal',
            overflow: 'hidden',
          }}
        >
          {/* KHỐI TRÁI: HAMBURGER, HOME & TIÊU ĐỀ PHÂN HỆ (KHÔNG BAO GIỜ BỊ VỠ DÒNG XUỐNG DƯỚI) */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 0, flex: '1 1 auto', overflow: 'hidden' }}>
            {isMobile && (
              <Button
                type="text"
                icon={<MenuOutlined />}
                onClick={() => setCollapsed(!collapsed)}
                style={{ fontSize: '16px', width: 36, height: 36, flexShrink: 0, padding: 0 }}
              />
            )}
            <div className="header-hide-mobile" style={{ flexShrink: 0 }}>
              <Tooltip title={tApp.homeTooltip}>
                <Button
                  type="text"
                  icon={<HomeOutlined />}
                  onClick={() => onMenuChange('dashboard')}
                  style={{
                    fontSize: '16px',
                    width: 36,
                    height: 36,
                    color: currentMenu === 'dashboard' ? '#1677ff' : '#8c8c8c',
                    padding: 0,
                  }}
                />
              </Tooltip>
            </div>
            <div style={{ minWidth: 0, flex: 1, overflow: 'hidden' }}>
              <Text
                strong
                style={{
                  fontSize: isMobile ? 15 : 18,
                  whiteSpace: 'nowrap',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  display: 'block',
                  lineHeight: 1.3,
                  margin: 0,
                }}
              >
                {currentMenu === 'dashboard' && tApp.titleDashboard}
                {currentMenu === 'nhanvien' && tApp.titleEmployees}
                {currentMenu === 'chamcong' && tApp.titleAttendance}
                {currentMenu === 'bangluong' && tApp.titlePayroll}
                {currentMenu === 'hopdong' && tApp.titleContracts}
                {currentMenu === 'khenthuong' && tApp.titleRewards}
                {currentMenu === 'nangluong' && tApp.titlePromotions}
                {currentMenu === 'ungluong' && tApp.titleOvertime}
                {currentMenu === 'approvals' && tApp.titleApprovals}
                {currentMenu === 'phanquyen' && tApp.titlePermissions}
                {currentMenu === 'audit' && tApp.titleAudit}
              </Text>
            </div>
          </div>

          {/* KHỐI PHẢI: CÁC NÚT THAO TÁC & PROFILE (TỰ ĐỘNG THU GỌN THEO ĐỘ RỘNG MÀN HÌNH) */}
          <div style={{ display: 'flex', alignItems: 'center', gap: isMobile ? 6 : 10, flexShrink: 0 }}>
            {/* Tag Online/Offline: Chỉ hiện khi màn hình rộng > 1200px */}
            <div className="header-hide-1200">
              <Tooltip title={isBackendConnected ? tApp.systemOnlineTooltip(kyCongCount) : tApp.systemOfflineTooltip}>
                <Tag
                  color={isBackendConnected ? 'success' : 'warning'}
                  style={{ cursor: 'pointer', padding: '2px 8px', borderRadius: 10, margin: 0 }}
                  onClick={onRefreshData}
                >
                  {isBackendConnected ? tApp.online : tApp.offline}
                </Tag>
              </Tooltip>
            </div>

            {/* Ô tìm kiếm Search input dài: Chỉ hiện khi màn hình rộng > 1200px */}
            {onOpenCommandPalette && (
              <div className="header-hide-1200">
                <Tooltip title={tApp.searchTooltip}>
                  <Button
                    icon={<SearchOutlined style={{ color: '#94a3b8' }} />}
                    onClick={onOpenCommandPalette}
                    style={{
                      background: '#f8fafc',
                      borderColor: '#e2e8f0',
                      color: '#64748b',
                      borderRadius: 8,
                      padding: '4px 12px',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 8,
                      minWidth: 165,
                      justifyContent: 'space-between',
                      cursor: 'pointer',
                    }}
                  >
                    <span style={{ fontSize: 13 }}>{tApp.searchPlaceholder}</span>
                    <kbd
                      style={{
                        fontSize: 11,
                        fontWeight: 600,
                        background: '#e2e8f0',
                        color: '#475569',
                        padding: '2px 6px',
                        borderRadius: 4,
                        border: '1px solid #cbd5e1',
                        lineHeight: '1',
                        fontFamily: 'monospace',
                      }}
                    >
                      Ctrl + K
                    </kbd>
                  </Button>
                </Tooltip>
              </div>
            )}

            {/* Nút Thao tác nhanh: Ẩn trên máy tính bảng & mobile < 992px hoặc khi không có quyền */}
            {quickActionItems.length > 0 && (
              <div className="header-hide-tablet">
                <Dropdown
                  menu={{
                    items: quickActionItems,
                  }}
                >
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    style={{
                      background: '#0f172a',
                      borderColor: '#0f172a',
                      fontWeight: 600,
                      borderRadius: 8,
                    }}
                  >
                    {tApp.quickAdd}
                  </Button>
                </Dropdown>
              </div>
            )}

            {/* Nút Hỏi AI Copilot */}
            <Tooltip title={tApp.askAiCopilot}>
              <Button
                type="primary"
                shape={isMobile ? 'circle' : undefined}
                size="middle"
                icon={<RobotOutlined />}
                onClick={onOpenAiDrawer}
                style={{
                  background: 'linear-gradient(135deg, #722ed1 0%, #1677ff 100%)',
                  border: 'none',
                  borderRadius: 8,
                  height: 36,
                  padding: isMobile ? 0 : '0 12px',
                  width: isMobile ? 36 : undefined,
                  display: 'inline-flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                {!isMobile && <span className="header-hide-1200" style={{ marginLeft: 4 }}>{tApp.askAiCopilot}</span>}
              </Button>
            </Tooltip>

            {/* Thông Báo Popover */}
            <Popover
              content={
                <NotificationPopoverContent
                  onNavigate={onMenuChange}
                  notifications={notifications}
                  onMarkAllAsRead={onMarkAllNotificationsRead}
                  onMarkItemAsRead={onMarkNotificationRead}
                />
              }
              trigger="click"
              placement="bottomRight"
            >
              <Tooltip title={tApp.notificationsTooltip}>
                <Badge count={unreadNotifCount} size="small" overflowCount={99}>
                  <Button shape="circle" size="middle" icon={<BellOutlined />} style={{ width: 36, height: 36 }} />
                </Badge>
              </Tooltip>
            </Popover>

            {/* Nút Làm Mới: Ẩn trên mobile < 768px để tiết kiệm chỗ */}
            <div className="header-hide-mobile">
              <Tooltip title={tApp.refreshTooltip}>
                <Button
                  shape="circle"
                  size="middle"
                  icon={<ReloadOutlined spin={loading} />}
                  onClick={onRefreshData}
                  style={{ width: 36, height: 36 }}
                />
              </Tooltip>
            </div>

            {/* BỘ CHUYỂN ĐỔI NGÔN NGỮ (VI, EN, ZH, KO, JA) */}
            <Dropdown
              menu={{
                items: (Object.keys(allConfigs) as AppLanguage[]).map((lKey) => ({
                  key: lKey,
                  label: (
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: 140, padding: '4px 0' }}>
                      <Space size={8}>
                        <span style={{ fontSize: 16 }}>{allConfigs[lKey]?.flag}</span>
                        <span style={{ fontWeight: currentLang === lKey ? 700 : 600, color: currentLang === lKey ? '#1677ff' : '#0f172a', fontSize: 13 }}>
                          {allConfigs[lKey]?.name}
                        </span>
                      </Space>
                      {currentLang === lKey && <CheckOutlined style={{ color: '#1677ff', fontWeight: 700, fontSize: 13 }} />}
                    </div>
                  ),
                })),
                selectedKeys: [currentLang],
                onClick: ({ key }) => handleLanguageChange(key as AppLanguage),
              }}
              placement="bottomRight"
              trigger={['click']}
            >
              <Tooltip title={tApp.languageSwitchTooltip}>
                <Button
                  style={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: 4,
                    borderRadius: 8,
                    borderColor: '#e2e8f0',
                    background: '#f8fafc',
                    fontWeight: 600,
                    padding: isMobile ? '0 6px' : '0 8px',
                    height: 36,
                  }}
                  size="middle"
                >
                  <GlobalOutlined style={{ color: '#1677ff', fontSize: 13 }} />
                  <span style={{ fontSize: 13 }}>{allConfigs[currentLang].flag}</span>
                  <span className="header-hide-mobile" style={{ fontSize: 12 }}>{allConfigs[currentLang].short}</span>
                  <DownOutlined style={{ fontSize: 8, color: '#94a3b8' }} />
                </Button>
              </Tooltip>
            </Dropdown>

            {/* AVATAR & USER PROFILE DROPDOWN */}
            <Dropdown
              menu={{
                items: [
                  {
                    key: 'profile',
                    icon: <UserOutlined />,
                    label: tApp.userAccount(currentUser.Username),
                  },
                  {
                    key: 'role',
                    icon: <SafetyCertificateOutlined />,
                    label: currentUser.IsAdmin ? tApp.roleAdmin : tApp.roleStaff(currentUser.Rights?.length || 0),
                  },
                  {
                    type: 'divider',
                  },
                  {
                    key: 'changePassword',
                    icon: <KeyOutlined style={{ color: '#1677ff' }} />,
                    label: tApp.changePassword,
                    onClick: onOpenChangePassword,
                  },
                  {
                    type: 'divider',
                  },
                  {
                    key: 'logout',
                    icon: <LogoutOutlined style={{ color: '#ff4d4f' }} />,
                    label: <span style={{ color: '#ff4d4f' }}>{tApp.logout}</span>,
                    onClick: onLogout,
                  },
                ],
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', padding: '2px 4px', borderRadius: 8 }}>
                <Avatar style={{ backgroundColor: currentUser.IsAdmin ? '#f5222d' : '#1677ff', flexShrink: 0 }} size={34}>
                  {currentUser.Username?.[0]?.toUpperCase() || 'U'}
                </Avatar>
                {/* Tên và tag Super Admin: Ẩn trên màn hình < 1200px */}
                <div className="header-hide-1200" style={{ display: 'flex', flexDirection: 'column', lineHeight: 1.2 }}>
                  <Text strong style={{ fontSize: 13, maxWidth: 110, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {currentUser.FullName}
                  </Text>
                  <Tag color={currentUser.IsAdmin ? 'red' : 'blue'} style={{ fontSize: 10, width: 'fit-content', padding: '0 4px', margin: 0 }}>
                    {currentUser.IsAdmin ? tApp.tagSuperAdmin : tApp.tagStaff}
                  </Tag>
                </div>
              </div>
            </Dropdown>
          </div>
        </Header>

        <Content style={{ margin: isMobile ? '12px 8px' : '20px 24px', minHeight: 400 }}>
          {children}
        </Content>
      </Layout>
    </Layout>
  );
}

export default MainLayout;
