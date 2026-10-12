import PhanQuyenModal from './PhanQuyenModal';
import React, { useState, useEffect } from 'react';
import {
  Drawer,
  Descriptions,
  Tag,
  Space,
  Button,
  Alert,
  Typography,
  Card,
  Popconfirm,
  Modal,
  Form,
  Input,
  Switch,
  Spin,
  notification,
  Row,
  Col,
} from 'antd';
import {
  UserOutlined,
  IdcardOutlined,
  MobileOutlined,
  DesktopOutlined,
  GlobalOutlined,
  LockOutlined,
  UnlockOutlined,
  KeyOutlined,
  DisconnectOutlined,
  SettingOutlined,
  BranchesOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';
import type { SysUserDTO } from '../types/hrms';

interface PlatformItemDto {
  Channel: string;
  FunctionCode: string;
  DirectGrant: boolean;
  InheritedGrant: boolean;
  IsEffective: boolean;
  InheritedFromGroups: string[];
  HasMismatchWarning: boolean;
  ReadinessCode: string;
  ReadinessMessage: string;
}

interface UserPlatformSummaryDto {
  UserId: number;
  Username: string;
  FullName: string;
  IsRootAdmin: boolean;
  IsGroup: boolean;
  IsDisabled: boolean;
  EmployeeId?: number;
  EmployeeCode?: string;
  EmployeeName?: string;
  IsMobileEnabled: boolean;
  Platforms: PlatformItemDto[];
}

const { Text, Title } = Typography;

interface UserDetailDrawerProps {
  visible: boolean;
  onClose: () => void;
  user: SysUserDTO | null;
  onRefresh?: () => void;
  canEdit?: (...codes: string[]) => boolean;
}

export const UserDetailDrawer: React.FC<UserDetailDrawerProps> = ({
  visible,
  onClose,
  user,
  onRefresh,
  canEdit,
}) => {
  const { t } = useAppLanguage();
  const { tokens } = useAppTheme();
  const badgeStyle = (kind: 'info' | 'success' | 'danger' | 'warning' | 'violet' | 'neutral'): React.CSSProperties => ({
    background: kind === 'neutral' ? tokens.cardSecondaryBg : tokens[`${kind}Bg`],
    color: kind === 'neutral' ? tokens.textSecondary : tokens[`${kind}Text`],
    borderColor: tokens.borderMedium,
  });
  const [resetModalVisible, setResetModalVisible] = useState(false);
  const [newPassword, setNewPassword] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [platformSummary, setPlatformSummary] = useState<UserPlatformSummaryDto | null>(null);
  const [loadingPlatform, setLoadingPlatform] = useState(false);
  const [platformModalVisible, setPlatformModalVisible] = useState(false);
  const [editDesktop, setEditDesktop] = useState(true);
  const [editWeb, setEditWeb] = useState(true);
  const [editMobile, setEditMobile] = useState(false);
  const [editMobileEnabled, setEditMobileEnabled] = useState(true);
  const [savingPlatform, setSavingPlatform] = useState(false);

  // Phân quyền chi tiết theo kênh (Mục 13 - 14 Cha - Con)
  const [channelModalVisible, setChannelModalVisible] = useState(false);
  useEffect(() => {
    if (visible && user?.IdUser) {
      loadPlatformAccess();
    }
  }, [visible, user?.IdUser]);

  const loadPlatformAccess = async () => {
    if (!user?.IdUser) return;
    try {
      setLoadingPlatform(true);
      const res = await api.get(`/users/${user.IdUser}/platform-access`);
      if (res.data) {
        setPlatformSummary(res.data);
        const pDesktop = res.data.Platforms?.find((p: PlatformItemDto) => p.Channel === 'DESKTOP');
        const pWeb = res.data.Platforms?.find((p: PlatformItemDto) => p.Channel === 'WEB');
        const pMobile = res.data.Platforms?.find((p: PlatformItemDto) => p.Channel === 'MOBILE');
        setEditDesktop(pDesktop ? pDesktop.DirectGrant : true);
        setEditWeb(pWeb ? pWeb.DirectGrant : true);
        setEditMobile(pMobile ? pMobile.DirectGrant : false);
        setEditMobileEnabled(res.data.IsMobileEnabled !== undefined ? res.data.IsMobileEnabled : true);
      }
    } catch {
      // Fallback nếu API chưa có
    } finally {
      setLoadingPlatform(false);
    }
  };

  const handleSavePlatformAccess = async () => {
    if (!user?.IdUser) return;
    try {
      setSavingPlatform(true);
      await api.post(`/users/${user.IdUser}/platform-access`, {
        TargetUserId: user.IdUser,
        DesktopDirectGrant: editDesktop,
        WebDirectGrant: editWeb,
        MobileDirectGrant: editMobile,
        IsMobileEnabled: editMobileEnabled,
      });
      notification.success({
        message: t('userDetail.success'),
        description: t('userDetail.platformSuccess', { username: user.Username }),
      });
      setPlatformModalVisible(false);
      loadPlatformAccess();
      if (onRefresh) onRefresh();
    } catch {
      notification.error({ message: t('userDetail.error'), description: t('userDetail.platformError') });
    } finally {
      setSavingPlatform(false);
    }
  };

  if (!user) return null;

  const hasLinked = Boolean(user.Manv || user.manv);
  const manv = user.Manv || user.manv;
  const empCode = user.EmployeeCode || user.employeeCode;
  const empName = user.EmployeeName || user.employeeName;
  const isMobileEnabled = user.IsMobileEnabled !== undefined ? user.IsMobileEnabled : (user.ClientType !== 'DESKTOP');
  const isDisabled = Boolean(user.Disabled);

  // Toggle Lock
  const handleToggleLock = async () => {
    try {
      setActionLoading(true);
      await api.post(`/users/${user.IdUser}/toggle-lock`);
      notification.success({
        message: t('userDetail.success'),
        description: t(isDisabled ? 'userDetail.unlockSuccess' : 'userDetail.lockSuccess', { username: user.Username }),
      });
      if (onRefresh) onRefresh();
      onClose();
    } catch {
      notification.error({ message: t('userDetail.error'), description: t('userDetail.lockError') });
    } finally {
      setActionLoading(false);
    }
  };

  // Toggle Mobile
  const handleToggleMobile = async () => {
    try {
      setActionLoading(true);
      const target = !isMobileEnabled;
      await api.post(`/users/${user.IdUser}/toggle-mobile`, { IsMobileEnabled: target });
      notification.success({
        message: t('userDetail.success'),
        description: t(target ? 'userDetail.mobileEnableSuccess' : 'userDetail.mobileDisableSuccess', { username: user.Username }),
      });
      if (onRefresh) onRefresh();
      loadPlatformAccess();
      onClose();
    } catch {
      notification.error({ message: t('userDetail.error'), description: t('userDetail.mobileError') });
    } finally {
      setActionLoading(false);
    }
  };

  // Reset Password
  const handleResetPassword = async () => {
    if (!newPassword.trim()) {
      notification.warning({ message: t('userDetail.error'), description: t('userDetail.passwordRequired') });
      return;
    }
    try {
      setActionLoading(true);
      await api.post(`/users/${user.IdUser}/reset-password`, { NewPassword: newPassword.trim() });
      notification.success({
        message: t('userDetail.success'),
        description: t('userDetail.passwordSuccess', { username: user.Username }),
      });
      setResetModalVisible(false);
      setNewPassword('');
    } catch {
      notification.error({ message: t('userDetail.error'), description: t('userDetail.passwordError') });
    } finally {
      setActionLoading(false);
    }
  };

  // Unlink Employee
  const handleUnlink = async () => {
    try {
      setActionLoading(true);
      await api.post(`/users/${user.IdUser}/unlink-employee`);
      notification.success({
        message: t('userDetail.success'),
        description: t('userDetail.unlinkSuccess', { username: user.Username }),
      });
      if (onRefresh) onRefresh();
      onClose();
    } catch {
      notification.error({ message: t('userDetail.error'), description: t('userDetail.unlinkError') });
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <>
      <Drawer
        title={
          <Space>
            <UserOutlined style={{ color: tokens.infoText }} />
            <span>{t('userDetail.title', { id: user.IdUser })}</span>
          </Space>
        }
        placement="right"
        width="min(560px, 95vw)"
        open={visible}
        onClose={onClose}
        footer={
          <div style={{ textAlign: 'right' }}>
            <Button onClick={onClose}>{t('userDetail.close')}</Button>
          </div>
        }
      >
        {/* Account Summary Header */}
        <div style={{ marginBottom: 20, padding: 16, background: tokens.cardSecondaryBg, color: tokens.textPrimary, border: `1px solid ${tokens.borderMedium}`, borderRadius: 8 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
              <Title level={4} style={{ margin: 0, color: tokens.infoText }}>
                <UserOutlined style={{ marginRight: 8 }} />
                {user.Username}
              </Title>
              <Space wrap>
                {user.IsGroup ? (
                  <Tag color="purple" style={{ ...badgeStyle('violet') }}>{t('userDetail.group')}</Tag>
                ) : (
                  <Tag color="blue" style={{ ...badgeStyle('info') }}>{t('userDetail.user')}</Tag>
                )}
                {isDisabled ? (
                  <Tag color="red" style={{ ...badgeStyle('danger') }}>{t('userDetail.locked')}</Tag>
                ) : (
                  <Tag color="green" style={{ ...badgeStyle('success') }}>{t('userDetail.active')}</Tag>
                )}
              </Space>
            </div>
            <Text type="secondary" style={{ color: tokens.textSecondary }}>{user.FullName}</Text>
          </Space>
        </div>

        {/* 1. Account Information */}
        <Card title={t('userDetail.identity')} size="small" style={{ marginBottom: 16 }}>
          <Descriptions column={1} size="small" bordered>
            <Descriptions.Item label={t('userDetail.userId')}>
              <Text strong>#{user.IdUser}</Text>
            </Descriptions.Item>
            <Descriptions.Item label={t('userDetail.loginName')}>
              <Text code strong>{user.Username}</Text>
            </Descriptions.Item>
            <Descriptions.Item label={t('userDetail.fullName')}>
              {user.FullName}
            </Descriptions.Item>
            <Descriptions.Item label={t('userDetail.companyUnit')}>
              {user.MACTY || '1'} / {user.MADVI || '1'}
            </Descriptions.Item>
            {user.Groups && user.Groups.length > 0 && (
              <Descriptions.Item label={t('userDetail.groups')}>
                <Space wrap size={[0, 4]}>
                  {user.Groups.map((g, idx) => (
                    <Tag key={idx} color="geekblue">{g}</Tag>
                  ))}
                </Space>
              </Descriptions.Item>
            )}
          </Descriptions>
        </Card>

        {/* 2. Employee Mapping */}
        <Card title={t('userDetail.mapping')} size="small" style={{ marginBottom: 16 }}>
          {hasLinked ? (
            <Descriptions column={1} size="small" bordered>
              <Descriptions.Item label={t('userDetail.employeeCode')}>
                <Space>
                  <IdcardOutlined style={{ color: tokens.successText }} />
                  <Text strong style={{ color: tokens.successText }}>{empCode || `NV#${manv}`}</Text>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label={t('userDetail.employeeId')}>
                #{manv}
              </Descriptions.Item>
              <Descriptions.Item label={t('userDetail.employeeName')}>
                <Text strong>{empName || user.FullName}</Text>
              </Descriptions.Item>
            </Descriptions>
          ) : (
            <Alert
              message={t('userDetail.notLinked')}
              description={t('userDetail.notLinkedHelp')}
              type="warning"
              showIcon
            />
          )}
        </Card>

        {/* 3. Access Channels */}
        <Card
          title={
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8, whiteSpace: 'normal' }}>
              <span>{t('userDetail.channels')}</span>
              {canEdit?.('F_SYSTEM_PQ_CHUCNANG') && (
                <Space wrap>
                  <Button
                    size="small"
                    type="default"
                    icon={<SettingOutlined />}
                    onClick={() => setPlatformModalVisible(true)}
                  >
                    {t('userDetail.togglePlatforms')}
                  </Button>
                  <Button
                    size="small"
                    type="primary"
                    ghost
                    icon={<BranchesOutlined />}
                    onClick={() => {
                      setChannelModalVisible(true);
                    }}
                  >
                    {t('userDetail.channelPermissions')}
                  </Button>
                </Space>
              )}
            </div>
          }
          size="small"
          style={{ marginBottom: 16 }}
        >
          {loadingPlatform ? (
            <div style={{ textAlign: 'center', padding: '16px 0' }}>
              <Spin tip={t('userDetail.loading')} />
            </div>
          ) : (
            <Row gutter={[10, 10]}>
              {/* Desktop Channel */}
              {(() => {
                const item = platformSummary?.Platforms?.find(p => p.Channel === 'DESKTOP');
                const isGranted = item?.IsEffective ?? false;
                return (
                  <Col xs={24} sm={8}>
                    <Card size="small" style={{ textAlign: 'center', background: tokens.cardSecondaryBg, color: tokens.textPrimary, borderColor: tokens.borderMedium, height: '100%' }}>
                      <DesktopOutlined style={{ fontSize: 20, color: tokens.infoText, marginBottom: 4 }} />
                      <div style={{ fontWeight: 500 }}>{t('userDetail.desktop')}</div>
                      <Tag color={isGranted ? 'green' : 'red'} style={{ ...badgeStyle(isGranted ? 'success' : 'danger'), marginTop: 6 }}>
                        {t(isGranted ? 'userDetail.allowed' : 'userDetail.denied')}
                      </Tag>
                      <div style={{ fontSize: 11, color: tokens.textSecondary, marginTop: 4 }}>
                        {item?.DirectGrant && <Tag color="blue" style={{ ...badgeStyle('info'),  fontSize: 10  }}>{t('userDetail.direct')}</Tag>}
                        {item?.InheritedGrant && (
                          <Tag color="purple" style={{ ...badgeStyle('violet'),  fontSize: 10  }}>{t('userDetail.inherited', { groups: item.InheritedFromGroups?.join(', ') || t('userDetail.group') })}</Tag>
                        )}
                        {!item?.DirectGrant && !item?.InheritedGrant && <span>{t('userDetail.ungranted')}</span>}
                      </div>
                    </Card>
                  </Col>
                );
              })()}

              {/* Web Portal Channel */}
              {(() => {
                const item = platformSummary?.Platforms?.find(p => p.Channel === 'WEB');
                const isGranted = item?.IsEffective ?? false;
                return (
                  <Col xs={24} sm={8}>
                    <Card size="small" style={{ textAlign: 'center', background: tokens.cardSecondaryBg, color: tokens.textPrimary, borderColor: tokens.borderMedium, height: '100%' }}>
                      <GlobalOutlined style={{ fontSize: 20, color: tokens.violetText, marginBottom: 4 }} />
                      <div style={{ fontWeight: 500 }}>{t('userDetail.web')}</div>
                      <Tag color={isGranted ? 'green' : 'red'} style={{ ...badgeStyle(isGranted ? 'success' : 'danger'), marginTop: 6 }}>
                        {t(isGranted ? 'userDetail.allowed' : 'userDetail.denied')}
                      </Tag>
                      <div style={{ fontSize: 11, color: tokens.textSecondary, marginTop: 4 }}>
                        {item?.DirectGrant && <Tag color="blue" style={{ ...badgeStyle('info'),  fontSize: 10  }}>{t('userDetail.direct')}</Tag>}
                        {item?.InheritedGrant && (
                          <Tag color="purple" style={{ ...badgeStyle('violet'),  fontSize: 10  }}>{t('userDetail.inherited', { groups: item.InheritedFromGroups?.join(', ') || t('userDetail.group') })}</Tag>
                        )}
                        {!item?.DirectGrant && !item?.InheritedGrant && <span>{t('userDetail.ungranted')}</span>}
                      </div>
                    </Card>
                  </Col>
                );
              })()}

              {/* Mobile ESS Channel */}
              {(() => {
                const item = platformSummary?.Platforms?.find(p => p.Channel === 'MOBILE');
                const isGranted = item?.IsEffective ?? false;
                const readiness = item?.ReadinessCode || (isMobileEnabled && hasLinked ? 'READY' : 'MISSING_MAPPING');

                return (
                  <Col xs={24} sm={8}>
                    <Card size="small" style={{ textAlign: 'center', background: tokens.cardSecondaryBg, color: tokens.textPrimary, borderColor: tokens.borderMedium, height: '100%' }}>
                      <MobileOutlined style={{ fontSize: 20, color: (isGranted && readiness === 'READY') ? tokens.successText : tokens.warningText, marginBottom: 4 }} />
                      <div style={{ fontWeight: 500 }}>{t('userDetail.mobile')}</div>
                      {!isGranted ? (
                        <Tag color="red" style={{ ...badgeStyle('danger'),  marginTop: 6  }}>{t('userDetail.ungranted')}</Tag>
                      ) : readiness === 'READY' ? (
                        <Tag color="green" style={{ ...badgeStyle('success'),  marginTop: 6  }}>{t('userDetail.ready')}</Tag>
                      ) : readiness === 'MISSING_MAPPING' ? (
                        <Tag color="orange" style={{ ...badgeStyle('warning'),  marginTop: 6  }}>{t('userDetail.waitingMapping')}</Tag>
                      ) : readiness === 'MOBILE_DISABLED' ? (
                        <Tag color="default" style={{ ...badgeStyle('neutral'),  marginTop: 6  }}>{t('userDetail.paused')}</Tag>
                      ) : readiness === 'EMPLOYEE_TERMINATED' ? (
                        <Tag color="volcano" style={{ ...badgeStyle('danger'),  marginTop: 6  }}>{t('userDetail.terminated')}</Tag>
                      ) : (
                        <Tag color="gold" style={{ ...badgeStyle('warning'),  marginTop: 6  }}>{readiness}</Tag>
                      )}
                      <div style={{ fontSize: 11, color: tokens.textSecondary, marginTop: 4 }}>
                        {item?.DirectGrant && <Tag color="blue" style={{ ...badgeStyle('info'),  fontSize: 10  }}>{t('userDetail.direct')}</Tag>}
                        {item?.InheritedGrant && (
                          <Tag color="purple" style={{ ...badgeStyle('violet'),  fontSize: 10  }}>{t('userDetail.inherited', { groups: item.InheritedFromGroups?.join(', ') || t('userDetail.group') })}</Tag>
                        )}
                      </div>
                    </Card>
                  </Col>
                );
              })()}
            </Row>
          )}
        </Card>

        {/* 4. Administrative Actions */}
        {canEdit?.('F_SYSTEM_USER') && (
          <Card title={t('userDetail.adminActions')} size="small">
            <Space wrap>
              {/* Configure Platforms */}
              <Button
                icon={<SettingOutlined />}
                disabled={!canEdit?.("F_SYSTEM_PQ_CHUCNANG")}
                onClick={() => setPlatformModalVisible(true)}
              >
                {t('userDetail.platformPermissions')}
              </Button>

              {/* Toggle Mobile */}
              <Button
                icon={<MobileOutlined />}
                onClick={handleToggleMobile}
                loading={actionLoading}
              >
                {t(isMobileEnabled ? 'userDetail.disableMobile' : 'userDetail.enableMobile')}
              </Button>

              {/* Reset Password */}
              <Button
                icon={<KeyOutlined />}
                onClick={() => setResetModalVisible(true)}
              >
                {t('userDetail.resetPassword')}
              </Button>

              {/* Toggle Lock */}
              <Popconfirm
                title={t(isDisabled ? 'userDetail.unlockConfirm' : 'userDetail.lockConfirm')}
                description={t(isDisabled ? 'userDetail.unlockHelp' : 'userDetail.lockHelp', { username: user.Username })}
                onConfirm={handleToggleLock}
                okText={t('userDetail.confirm')}
                cancelText={t('userDetail.cancel')}
              >
                <Button
                  danger={!isDisabled}
                  disabled={!canEdit?.("F_SYSTEM_LOCK_USER")}
                icon={isDisabled ? <UnlockOutlined /> : <LockOutlined />}
                  loading={actionLoading}
                >
                  {t(isDisabled ? 'userDetail.unlock' : 'userDetail.lock')}
                </Button>
              </Popconfirm>

              {/* Unlink */}
              {hasLinked && (
                <Popconfirm
                  title={t('userDetail.unlinkConfirm')}
                  description={t('userDetail.unlinkHelp')}
                  onConfirm={handleUnlink}
                  okText={t('userDetail.unlink')}
                  cancelText={t('userDetail.no')}
                  okButtonProps={{ danger: true }}
                >
                  <Button danger icon={<DisconnectOutlined />} loading={actionLoading}>
                    {t('userDetail.unlink')}
                  </Button>
                </Popconfirm>
              )}
            </Space>
          </Card>
        )}
      </Drawer>

      {/* Modal Phân quyền nền tảng (Platform Access Modal) */}
      <Modal
        title={
          <Space>
            <SettingOutlined style={{ color: tokens.infoText }} />
            <span>{t('userDetail.platformTitle', { username: user.Username })}</span>
          </Space>
        }
        open={platformModalVisible}
        onCancel={() => setPlatformModalVisible(false)}
        onOk={handleSavePlatformAccess}
        confirmLoading={savingPlatform}
        okText={t('userDetail.savePlatforms')}
        cancelText={t('userDetail.cancel')}
        width={520}
      >
        <div style={{ marginTop: 8 }}>
          <Alert
            message={t('userDetail.securityTitle')}
            description={t('userDetail.securityHelp')}
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />

          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label={`${t('userDetail.desktop')} (F_LOGIN_DESKTOP)`}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>{t('userDetail.allowDesktop')}</span>
                <Switch
                  checked={editDesktop}
                  onChange={setEditDesktop}
                />
              </div>
            </Descriptions.Item>
            <Descriptions.Item label={`${t('userDetail.web')} (F_LOGIN_WEB)`}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>{t('userDetail.allowWeb')}</span>
                <Switch
                  checked={editWeb}
                  onChange={setEditWeb}
                />
              </div>
            </Descriptions.Item>
            <Descriptions.Item label={`${t('userDetail.mobile')} (F_LOGIN_MOBILE)`}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>{t('userDetail.allowMobile')}</span>
                <Switch
                  checked={editMobile}
                  onChange={setEditMobile}

                />
              </div>
            </Descriptions.Item>
            <Descriptions.Item label={t('userDetail.mobileEnabled')}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>{t('userDetail.mobileEnabledHelp')}</span>
                <Switch
                  checked={editMobileEnabled}
                  onChange={setEditMobileEnabled}

                />
              </div>
            </Descriptions.Item>
          </Descriptions>
        </div>
      </Modal>

      {/* Reset Password Modal */}
      <Modal
        title={
          <Space>
            <KeyOutlined style={{ color: tokens.infoText }} />
            <span>{t('userDetail.resetTitle', { username: user.Username })}</span>
          </Space>
        }
        open={resetModalVisible}
        onCancel={() => setResetModalVisible(false)}
        onOk={handleResetPassword}
        confirmLoading={actionLoading}
        okText={t('userDetail.savePassword')}
        cancelText={t('userDetail.cancel')}
      >
        <Form layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item label={t('userDetail.newPassword')} required>
            <Input.Password
              placeholder={t('userDetail.passwordPlaceholder')}
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
            />
          </Form.Item>
        </Form>
      </Modal>
      <PhanQuyenModal visible={channelModalVisible} onClose={() => setChannelModalVisible(false)}
        userId={user.IdUser} username={user.Username} fullName={user.FullName} isGroup={Boolean(user.IsGroup)}
        onSuccess={() => { void loadPlatformAccess(); onRefresh?.(); }} />
    </>
  );
};

export default UserDetailDrawer;
