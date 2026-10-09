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
  Table,
  Tabs,
  Checkbox,
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
  SafetyCertificateOutlined,
  SettingOutlined,
  BranchesOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import type {
  SysUserDTO,
  UserFullChannelPermissionsDTO,
  PlatformChannelTreeDTO,
  ChannelFunctionRightItemDTO,
} from '../types/hrms';

interface PlatformItemDto {
  Channel: string;
  FunctionCode: string;
  DirectGrant: boolean;
  InheritedGrant: boolean;
  EffectiveGrant: boolean;
  InheritedFromGroupNames: string[];
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
  const [, setChannelPerms] = useState<UserFullChannelPermissionsDTO | null>(null);
  const [loadingChannelPerms, setLoadingChannelPerms] = useState(false);
  const [activeChannelTab, setActiveChannelTab] = useState<'DESKTOP' | 'WEB' | 'MOBILE'>('DESKTOP');
  const [savingChannelPerms, setSavingChannelPerms] = useState(false);
  const [editableChannelTree, setEditableChannelTree] = useState<Record<string, PlatformChannelTreeDTO>>({});

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
        message: 'Thành công',
        description: `Đã cập nhật quyền nền tảng cho tài khoản [${user.Username}] thành công!`,
      });
      setPlatformModalVisible(false);
      loadPlatformAccess();
      if (onRefresh) onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể cập nhật quyền nền tảng.' });
    } finally {
      setSavingPlatform(false);
    }
  };

  const loadChannelPermissions = async () => {
    if (!user?.IdUser) return;
    try {
      setLoadingChannelPerms(true);
      const res = await api.get(`/users/${user.IdUser}/channel-permissions`);
      if (res.data) {
        setChannelPerms(res.data);
        const treeMap: Record<string, PlatformChannelTreeDTO> = {};
        res.data.Channels?.forEach((ch: PlatformChannelTreeDTO) => {
          treeMap[ch.Channel] = JSON.parse(JSON.stringify(ch));
        });
        setEditableChannelTree(treeMap);
      }
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải phân quyền chi tiết theo kênh.' });
    } finally {
      setLoadingChannelPerms(false);
    }
  };

  const handleToggleChannelAction = (
    channel: string,
    funcCode: string,
    action: 'CanView' | 'CanAdd' | 'CanEdit' | 'CanDelete' | 'CanPrint',
    checked: boolean
  ) => {
    const current = editableChannelTree[channel];
    if (!current) return;
    const fn = current.Functions.find(f => f.FunctionCode === funcCode);
    if (!fn) return;

    fn.DirectGrant[action] = checked;
    // Tự động tính lại quyền hiệu lực nếu cha đang bật
    if (current.ParentIsEffective) {
      fn.EffectiveGrant[action] = (fn.DirectGrant[action] || fn.InheritedGrant[action]) && fn.SupportedCapabilities[action];
    }
    setEditableChannelTree({ ...editableChannelTree, [channel]: { ...current } });
  };

  const handleToggleParentChannel = (channel: string, checked: boolean) => {
    const current = editableChannelTree[channel];
    if (!current) return;

    current.ParentDirectGrant = checked;
    current.ParentIsEffective = checked || current.ParentInheritedGrant;

    // Cập nhật trạng thái khóa các quyền con
    current.Functions.forEach(fn => {
      fn.IsDisabledByParent = !current.ParentIsEffective;
      if (!current.ParentIsEffective) {
        fn.EffectiveGrant = { CanView: false, CanAdd: false, CanEdit: false, CanDelete: false, CanPrint: false };
      } else {
        fn.EffectiveGrant = {
          CanView: (fn.DirectGrant.CanView || fn.InheritedGrant.CanView) && fn.SupportedCapabilities.CanView,
          CanAdd: (fn.DirectGrant.CanAdd || fn.InheritedGrant.CanAdd) && fn.SupportedCapabilities.CanAdd,
          CanEdit: (fn.DirectGrant.CanEdit || fn.InheritedGrant.CanEdit) && fn.SupportedCapabilities.CanEdit,
          CanDelete: (fn.DirectGrant.CanDelete || fn.InheritedGrant.CanDelete) && fn.SupportedCapabilities.CanDelete,
          CanPrint: (fn.DirectGrant.CanPrint || fn.InheritedGrant.CanPrint) && fn.SupportedCapabilities.CanPrint,
        };
      }
    });

    setEditableChannelTree({ ...editableChannelTree, [channel]: { ...current } });
  };

  const handleSaveChannelPermissions = async (channel: string) => {
    if (!user?.IdUser) return;
    const tree = editableChannelTree[channel];
    if (!tree) return;

    try {
      setSavingChannelPerms(true);
      const payload = {
        TargetUserId: user.IdUser,
        Channel: channel,
        ParentDirectGrant: tree.ParentDirectGrant,
        Functions: tree.Functions.map(f => ({
          FunctionCode: f.FunctionCode,
          CanView: f.DirectGrant.CanView,
          CanAdd: f.DirectGrant.CanAdd,
          CanEdit: f.DirectGrant.CanEdit,
          CanDelete: f.DirectGrant.CanDelete,
          CanPrint: f.DirectGrant.CanPrint,
        })),
      };

      const res = await api.post(`/users/${user.IdUser}/channel-permissions`, payload);
      if (res.data?.Success) {
        notification.success({
          message: 'Thành công',
          description: `Đã lưu phân quyền kênh [${tree.ChannelLabel}] thành công! ${res.data.SessionsRevokedCount ? `(Thu hồi ${res.data.SessionsRevokedCount} phiên cũ)` : ''}`,
        });
        loadChannelPermissions();
        loadPlatformAccess();
        if (onRefresh) onRefresh();
      } else {
        notification.error({ message: 'Lỗi', description: res.data?.Message || 'Không thể lưu quyền kênh.' });
      }
    } catch {
      notification.error({ message: 'Lỗi', description: 'Đã xảy ra lỗi khi lưu quyền kênh.' });
    } finally {
      setSavingChannelPerms(false);
    }
  };

  if (!user) return null;

  const isRootAdmin = Boolean(user.IsAdmin || user.Username?.toUpperCase() === 'ADMIN');
  const hasLinked = Boolean(user.Manv || user.manv);
  const manv = user.Manv || user.manv;
  const empCode = user.EmployeeCode || user.employeeCode;
  const empName = user.EmployeeName || user.employeeName;
  const isMobileEnabled = user.IsMobileEnabled !== undefined ? user.IsMobileEnabled : (user.ClientType !== 'DESKTOP');
  const isDisabled = Boolean(user.Disabled);

  // Toggle Lock
  const handleToggleLock = async () => {
    if (isRootAdmin) {
      notification.warning({ message: 'Không thể khóa', description: 'Tài khoản Quản trị hệ thống (ADMIN) không thể bị khóa.' });
      return;
    }
    try {
      setActionLoading(true);
      await api.post(`/users/${user.IdUser}/toggle-lock`);
      notification.success({
        message: 'Thành công',
        description: `Đã ${isDisabled ? 'mở khóa' : 'khóa'} tài khoản [${user.Username}]!`,
      });
      if (onRefresh) onRefresh();
      onClose();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể thay đổi trạng thái tài khoản.' });
    } finally {
      setActionLoading(false);
    }
  };

  // Toggle Mobile
  const handleToggleMobile = async () => {
    if (isRootAdmin) {
      notification.warning({ message: 'Không áp dụng', description: 'Tài khoản ADMIN là tài khoản quản trị hệ thống, không áp dụng Mobile Access.' });
      return;
    }
    try {
      setActionLoading(true);
      const target = !isMobileEnabled;
      await api.post(`/users/${user.IdUser}/toggle-mobile`, { IsMobileEnabled: target });
      notification.success({
        message: 'Thành công',
        description: `Đã ${target ? 'kích hoạt' : 'tắt'} quyền Mobile Access cho [${user.Username}]!`,
      });
      if (onRefresh) onRefresh();
      loadPlatformAccess();
      onClose();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể cập nhật quyền Mobile Access.' });
    } finally {
      setActionLoading(false);
    }
  };

  // Reset Password
  const handleResetPassword = async () => {
    if (!newPassword.trim()) {
      notification.warning({ message: 'Lỗi', description: 'Vui lòng nhập mật khẩu mới.' });
      return;
    }
    try {
      setActionLoading(true);
      await api.post(`/users/${user.IdUser}/reset-password`, { NewPassword: newPassword.trim() });
      notification.success({
        message: 'Thành công',
        description: `Đã đặt lại mật khẩu cho tài khoản [${user.Username}] thành công!`,
      });
      setResetModalVisible(false);
      setNewPassword('');
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể đặt lại mật khẩu.' });
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
        message: 'Thành công',
        description: `Đã hủy liên kết hồ sơ nhân viên cho [${user.Username}].`,
      });
      if (onRefresh) onRefresh();
      onClose();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể hủy liên kết.' });
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <>
      <Drawer
        title={
          <Space>
            <UserOutlined style={{ color: '#1677ff' }} />
            <span>Chi tiết Tài khoản & Quyền hạn [#{user.IdUser}]</span>
          </Space>
        }
        placement="right"
        width="min(560px, 95vw)"
        open={visible}
        onClose={onClose}
        footer={
          <div style={{ textAlign: 'right' }}>
            <Button onClick={onClose}>Đóng</Button>
          </div>
        }
      >
        {/* Account Summary Header */}
        <div style={{ marginBottom: 20, padding: 16, background: '#f5f5f5', borderRadius: 8 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
              <Title level={4} style={{ margin: 0, color: '#1677ff' }}>
                <UserOutlined style={{ marginRight: 8 }} />
                {user.Username}
              </Title>
              <Space wrap>
                {isRootAdmin ? (
                  <Tag color="gold" icon={<SafetyCertificateOutlined />}>Super Admin</Tag>
                ) : user.IsGroup ? (
                  <Tag color="purple">Nhóm quyền</Tag>
                ) : (
                  <Tag color="blue">Người dùng</Tag>
                )}
                {isDisabled ? (
                  <Tag color="red">Bị tạm khóa</Tag>
                ) : (
                  <Tag color="green">Đang hoạt động</Tag>
                )}
              </Space>
            </div>
            <Text type="secondary">{user.FullName}</Text>
          </Space>
        </div>

        {/* 1. Account Information */}
        <Card title="1. Thông tin Tài khoản (Account Identity)" size="small" style={{ marginBottom: 16 }}>
          <Descriptions column={1} size="small" bordered>
            <Descriptions.Item label="ID kỹ thuật (UserId)">
              <Text strong>#{user.IdUser}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="Tên đăng nhập (LoginName)">
              <Text code strong>{user.Username}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="Họ và tên hiển thị">
              {user.FullName}
            </Descriptions.Item>
            <Descriptions.Item label="Mã Công ty / Đơn vị">
              {user.MACTY || '1'} / {user.MADVI || '1'}
            </Descriptions.Item>
            {user.Groups && user.Groups.length > 0 && (
              <Descriptions.Item label="Nhóm quyền trực thuộc">
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
        <Card title="2. Hồ sơ Nhân sự liên kết (Employee Mapping 1:1)" size="small" style={{ marginBottom: 16 }}>
          {isRootAdmin ? (
            <Alert
              message="Tài khoản Quản trị hệ thống (ADMIN)"
              description="Tài khoản Quản trị viên tối cao là tài khoản kỹ thuật quản trị hệ thống, độc lập và tuyệt đối không liên kết với hồ sơ nhân sự."
              type="info"
              showIcon
            />
          ) : hasLinked ? (
            <Descriptions column={1} size="small" bordered>
              <Descriptions.Item label="Mã nhân sự (EmployeeCode)">
                <Space>
                  <IdcardOutlined style={{ color: '#52c41a' }} />
                  <Text strong style={{ color: '#389e0d' }}>{empCode || `NV#${manv}`}</Text>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Mã hệ thống (MANV)">
                #{manv}
              </Descriptions.Item>
              <Descriptions.Item label="Họ và tên nhân viên">
                <Text strong>{empName || user.FullName}</Text>
              </Descriptions.Item>
            </Descriptions>
          ) : (
            <Alert
              message="Chưa liên kết hồ sơ nhân sự"
              description="Tài khoản này chưa được gắn với hồ sơ nhân sự nào trong hệ thống. Bạn có thể sử dụng nút 'Liên kết NV' ở danh sách để gán."
              type="warning"
              showIcon
            />
          )}
        </Card>

        {/* 3. Access Channels */}
        <Card
          title={
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>3. Kênh truy cập hệ thống (Access Channels)</span>
              {(!canEdit || canEdit('F_SYSTEM_USER')) && !isRootAdmin && (
                <Space>
                  <Button
                    size="small"
                    type="default"
                    icon={<SettingOutlined />}
                    onClick={() => setPlatformModalVisible(true)}
                  >
                    Bật/Tắt nền tảng
                  </Button>
                  <Button
                    size="small"
                    type="primary"
                    ghost
                    icon={<BranchesOutlined />}
                    onClick={() => {
                      setChannelModalVisible(true);
                      loadChannelPermissions();
                    }}
                  >
                    Phân quyền Cha - Con
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
              <Spin tip="Đang tải trạng thái phân quyền 3 nền tảng..." />
            </div>
          ) : (
            <Row gutter={[10, 10]}>
              {/* Desktop Channel */}
              {(() => {
                const item = platformSummary?.Platforms?.find(p => p.Channel === 'DESKTOP');
                const isGranted = item ? item.EffectiveGrant : true;
                return (
                  <Col xs={24} sm={8}>
                    <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                      <DesktopOutlined style={{ fontSize: 20, color: '#1677ff', marginBottom: 4 }} />
                      <div style={{ fontWeight: 500 }}>Desktop WinForms</div>
                      <Tag color={isGranted ? 'green' : 'red'} style={{ marginTop: 6 }}>
                        {isGranted ? 'Cho phép truy cập' : 'Bị từ chối'}
                      </Tag>
                      <div style={{ fontSize: 11, color: '#8c8c8c', marginTop: 4 }}>
                        {item?.DirectGrant && <Tag color="blue" style={{ fontSize: 10 }}>Trực tiếp</Tag>}
                        {item?.InheritedGrant && (
                          <Tag color="purple" style={{ fontSize: 10 }}>Kế thừa ({item.InheritedFromGroupNames?.join(', ') || 'Nhóm'})</Tag>
                        )}
                        {!item?.DirectGrant && !item?.InheritedGrant && !isRootAdmin && <span>Chưa cấp quyền</span>}
                        {isRootAdmin && <span>Đặc quyền Quản trị viên</span>}
                      </div>
                    </Card>
                  </Col>
                );
              })()}

              {/* Web Portal Channel */}
              {(() => {
                const item = platformSummary?.Platforms?.find(p => p.Channel === 'WEB');
                const isGranted = item ? item.EffectiveGrant : true;
                return (
                  <Col xs={24} sm={8}>
                    <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                      <GlobalOutlined style={{ fontSize: 20, color: '#722ed1', marginBottom: 4 }} />
                      <div style={{ fontWeight: 500 }}>Web Portal</div>
                      <Tag color={isGranted ? 'green' : 'red'} style={{ marginTop: 6 }}>
                        {isGranted ? 'Cho phép truy cập' : 'Bị từ chối'}
                      </Tag>
                      <div style={{ fontSize: 11, color: '#8c8c8c', marginTop: 4 }}>
                        {item?.DirectGrant && <Tag color="blue" style={{ fontSize: 10 }}>Trực tiếp</Tag>}
                        {item?.InheritedGrant && (
                          <Tag color="purple" style={{ fontSize: 10 }}>Kế thừa ({item.InheritedFromGroupNames?.join(', ') || 'Nhóm'})</Tag>
                        )}
                        {!item?.DirectGrant && !item?.InheritedGrant && !isRootAdmin && <span>Chưa cấp quyền</span>}
                        {isRootAdmin && <span>Đặc quyền Quản trị viên</span>}
                      </div>
                    </Card>
                  </Col>
                );
              })()}

              {/* Mobile ESS Channel */}
              {(() => {
                const item = platformSummary?.Platforms?.find(p => p.Channel === 'MOBILE');
                const isGranted = item ? item.EffectiveGrant : false;
                const readiness = item?.ReadinessCode || (isMobileEnabled && hasLinked ? 'READY' : 'MISSING_MAPPING');

                return (
                  <Col xs={24} sm={8}>
                    <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                      <MobileOutlined style={{ fontSize: 20, color: isRootAdmin ? '#d9d9d9' : (isGranted && readiness === 'READY') ? '#52c41a' : '#fa8c16', marginBottom: 4 }} />
                      <div style={{ fontWeight: 500 }}>Mobile App</div>
                      {isRootAdmin ? (
                        <Tag color="default" style={{ marginTop: 6 }}>Cấm Root Admin</Tag>
                      ) : !isGranted ? (
                        <Tag color="red" style={{ marginTop: 6 }}>Chưa cấp quyền</Tag>
                      ) : readiness === 'READY' ? (
                        <Tag color="green" style={{ marginTop: 6 }}>Sẵn sàng hoạt động</Tag>
                      ) : readiness === 'MISSING_MAPPING' ? (
                        <Tag color="orange" style={{ marginTop: 6 }}>Chờ liên kết NV</Tag>
                      ) : readiness === 'MOBILE_DISABLED' ? (
                        <Tag color="default" style={{ marginTop: 6 }}>Đang tạm dừng</Tag>
                      ) : readiness === 'EMPLOYEE_TERMINATED' ? (
                        <Tag color="volcano" style={{ marginTop: 6 }}>Đã thôi việc</Tag>
                      ) : (
                        <Tag color="gold" style={{ marginTop: 6 }}>{readiness}</Tag>
                      )}
                      <div style={{ fontSize: 11, color: '#8c8c8c', marginTop: 4 }}>
                        {item?.DirectGrant && <Tag color="blue" style={{ fontSize: 10 }}>Trực tiếp</Tag>}
                        {item?.InheritedGrant && (
                          <Tag color="purple" style={{ fontSize: 10 }}>Kế thừa ({item.InheritedFromGroupNames?.join(', ') || 'Nhóm'})</Tag>
                        )}
                        {isRootAdmin && <span>Tài khoản kỹ thuật cấm dùng</span>}
                      </div>
                    </Card>
                  </Col>
                );
              })()}
            </Row>
          )}
        </Card>

        {/* 4. Administrative Actions */}
        {(!canEdit || canEdit('F_SYSTEM_USER')) && !isRootAdmin && (
          <Card title="4. Thao tác Quản trị tài khoản" size="small">
            <Space wrap>
              {/* Configure Platforms */}
              <Button
                icon={<SettingOutlined />}
                onClick={() => setPlatformModalVisible(true)}
              >
                Phân quyền nền tảng
              </Button>

              {/* Toggle Mobile */}
              <Button
                icon={<MobileOutlined />}
                onClick={handleToggleMobile}
                loading={actionLoading}
              >
                {isMobileEnabled ? 'Tắt Mobile Access' : 'Bật Mobile Access'}
              </Button>

              {/* Reset Password */}
              <Button
                icon={<KeyOutlined />}
                onClick={() => setResetModalVisible(true)}
              >
                Đặt lại mật khẩu
              </Button>

              {/* Toggle Lock */}
              <Popconfirm
                title={isDisabled ? 'Mở khóa tài khoản?' : 'Khóa tài khoản?'}
                description={`Bạn có chắc muốn ${isDisabled ? 'mở khóa' : 'tạm khóa'} tài khoản [${user.Username}]?`}
                onConfirm={handleToggleLock}
                okText="Đồng ý"
                cancelText="Hủy"
              >
                <Button
                  danger={!isDisabled}
                  icon={isDisabled ? <UnlockOutlined /> : <LockOutlined />}
                  loading={actionLoading}
                >
                  {isDisabled ? 'Mở khóa tài khoản' : 'Khóa tài khoản'}
                </Button>
              </Popconfirm>

              {/* Unlink */}
              {hasLinked && (
                <Popconfirm
                  title="Hủy liên kết hồ sơ nhân sự?"
                  description="Tài khoản này sẽ không thể đăng nhập trên ứng dụng Mobile cho đến khi được liên kết lại."
                  onConfirm={handleUnlink}
                  okText="Hủy liên kết"
                  cancelText="Không"
                  okButtonProps={{ danger: true }}
                >
                  <Button danger icon={<DisconnectOutlined />} loading={actionLoading}>
                    Hủy liên kết NV
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
            <SettingOutlined style={{ color: '#1677ff' }} />
            <span>Phân quyền nền tảng đăng nhập: [{user.Username}]</span>
          </Space>
        }
        open={platformModalVisible}
        onCancel={() => setPlatformModalVisible(false)}
        onOk={handleSavePlatformAccess}
        confirmLoading={savingPlatform}
        okText="Lưu quyền nền tảng"
        cancelText="Hủy"
        width={520}
      >
        <div style={{ marginTop: 8 }}>
          <Alert
            message="Chính sách bảo mật Zero-Trust"
            description="Quyền đăng nhập 3 nền tảng (F_LOGIN_DESKTOP, F_LOGIN_WEB, F_LOGIN_MOBILE) được phân tách độc lập. Thu hồi quyền trên một nền tảng sẽ lập tức chấm dứt các phiên làm việc của nền tảng đó mà không ảnh hưởng nền tảng khác."
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />

          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Desktop WinForms (F_LOGIN_DESKTOP)">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>Cho phép đăng nhập Desktop Client</span>
                <Switch
                  checked={editDesktop}
                  onChange={setEditDesktop}
                />
              </div>
            </Descriptions.Item>
            <Descriptions.Item label="Web Portal (F_LOGIN_WEB)">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>Cho phép đăng nhập Web Portal</span>
                <Switch
                  checked={editWeb}
                  onChange={setEditWeb}
                />
              </div>
            </Descriptions.Item>
            <Descriptions.Item label="Mobile App (F_LOGIN_MOBILE)">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>Cấp quyền chức năng Mobile</span>
                <Switch
                  checked={editMobile}
                  onChange={setEditMobile}
                  disabled={isRootAdmin}
                />
              </div>
            </Descriptions.Item>
            <Descriptions.Item label="Kích hoạt Mobile (IS_MOBILE_ENABLED)">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>Bật sử dụng Mobile trên hồ sơ NV</span>
                <Switch
                  checked={editMobileEnabled}
                  onChange={setEditMobileEnabled}
                  disabled={isRootAdmin}
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
            <KeyOutlined style={{ color: '#1677ff' }} />
            <span>Đặt lại mật khẩu cho [{user.Username}]</span>
          </Space>
        }
        open={resetModalVisible}
        onCancel={() => setResetModalVisible(false)}
        onOk={handleResetPassword}
        confirmLoading={actionLoading}
        okText="Lưu mật khẩu mới"
        cancelText="Hủy"
      >
        <Form layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item label="Mật khẩu mới" required>
            <Input.Password
              placeholder="Nhập mật khẩu mới..."
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
            />
          </Form.Item>
        </Form>
      </Modal>
      {/* Modal Phân quyền Cha - Con theo Kênh (Mục 13 - 14) */}
      <Modal
        title={
          <Space>
            <BranchesOutlined style={{ color: '#1677ff' }} />
            <span>Phân quyền Cha - Con theo Kênh: [{user.Username}]</span>
          </Space>
        }
        open={channelModalVisible}
        onCancel={() => setChannelModalVisible(false)}
        footer={null}
        width={960}
      >
        {loadingChannelPerms ? (
          <div style={{ textAlign: 'center', padding: '32px 0' }}>
            <Spin tip="Đang tải dữ liệu quyền cha - con và khả năng kênh..." />
          </div>
        ) : (
          <div>
            <Tabs
              activeKey={activeChannelTab}
              onChange={(k) => setActiveChannelTab(k as 'DESKTOP' | 'WEB' | 'MOBILE')}
              items={[
                {
                  key: 'DESKTOP',
                  label: (
                    <Space>
                      <DesktopOutlined />
                      <span>Desktop WinForms</span>
                    </Space>
                  ),
                },
                {
                  key: 'WEB',
                  label: (
                    <Space>
                      <GlobalOutlined />
                      <span>Web Portal</span>
                    </Space>
                  ),
                },
                {
                  key: 'MOBILE',
                  label: (
                    <Space>
                      <MobileOutlined />
                      <span>Mobile App</span>
                    </Space>
                  ),
                },
              ]}
            />

            {(() => {
              const currentTree = editableChannelTree[activeChannelTab];
              if (!currentTree) return <div>Chưa có dữ liệu kênh.</div>;

              const isParentOn = currentTree.ParentIsEffective;

              return (
                <div>
                  {/* Banner Trạng thái Quyền Cha */}
                  <Card size="small" style={{ marginBottom: 16, background: isParentOn ? '#f6ffed' : '#fff1f0', borderColor: isParentOn ? '#b7eb8f' : '#ffa39e' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Space direction="vertical" size={2}>
                        <div style={{ fontWeight: 600, fontSize: 14 }}>
                          {isParentOn ? (
                            <Space><CheckCircleOutlined style={{ color: '#52c41a' }} /> Quyền cha [Cho phép sử dụng {currentTree.ChannelLabel}] đang BẬT</Space>
                          ) : (
                            <Space><StopOutlined style={{ color: '#ff4d4f' }} /> Quyền cha [Cho phép sử dụng {currentTree.ChannelLabel}] đang TẮT</Space>
                          )}
                        </div>
                        <div style={{ fontSize: 12, color: '#595959' }}>
                          Mã quyền cha: <code>{currentTree.ParentFunctionCode}</code> | Kế thừa nhóm: {currentTree.ParentInheritedGrant ? currentTree.ParentInheritedGroupNames?.join(', ') || 'Có' : 'Không'}
                        </div>
                      </Space>
                      <Space>
                        <span>Cấp trực tiếp:</span>
                        <Switch
                          checked={currentTree.ParentDirectGrant}
                          onChange={(checked) => handleToggleParentChannel(activeChannelTab, checked)}
                          disabled={isRootAdmin && activeChannelTab === 'MOBILE'}
                        />
                      </Space>
                    </div>
                  </Card>

                  {!isParentOn && (
                    <Alert
                      message="Quyền sử dụng nền tảng đang tắt"
                      description="Toàn bộ các quyền con bên dưới bị vô hiệu hóa khi người dùng truy cập. Cấu hình quyền con được giữ nguyên để rà soát khi bật lại quyền cha."
                      type="warning"
                      showIcon
                      style={{ marginBottom: 16 }}
                    />
                  )}

                  {/* Bảng phân quyền chức năng con */}
                  <Table
                    size="small"
                    pagination={{ pageSize: 8, showSizeChanger: false }}
                    rowKey="FunctionCode"
                    dataSource={currentTree.Functions}
                    columns={[
                      {
                        title: 'Chức năng',
                        key: 'func',
                        render: (_, r: ChannelFunctionRightItemDTO) => (
                          <div>
                            <div style={{ fontWeight: 500 }}>{r.FunctionName}</div>
                            <div style={{ fontSize: 11, color: '#8c8c8c' }}>
                              <code>{r.FunctionCode}</code>
                              {r.RestrictionNote && (
                                <Tag color="orange" style={{ marginLeft: 6, fontSize: 10 }}>
                                  {r.RestrictionNote}
                                </Tag>
                              )}
                            </div>
                          </div>
                        ),
                      },
                      {
                        title: 'Loại quyền',
                        dataIndex: 'RightType',
                        width: 100,
                        render: (val: string) => (
                          <Tag color={val === 'LOGIN' ? 'blue' : 'default'} style={{ fontSize: 11 }}>
                            {val === 'LOGIN' ? 'Nền tảng' : 'Chức năng'}
                          </Tag>
                        ),
                      },
                      {
                        title: 'Xem',
                        width: 70,
                        align: 'center',
                        render: (_, r: ChannelFunctionRightItemDTO) => (
                          <Checkbox
                            checked={r.DirectGrant.CanView}
                            disabled={!r.SupportedCapabilities.CanView || !isParentOn}
                            onChange={(e) => handleToggleChannelAction(activeChannelTab, r.FunctionCode, 'CanView', e.target.checked)}
                          />
                        ),
                      },
                      {
                        title: 'Thêm',
                        width: 70,
                        align: 'center',
                        render: (_, r: ChannelFunctionRightItemDTO) => (
                          <Checkbox
                            checked={r.DirectGrant.CanAdd}
                            disabled={!r.SupportedCapabilities.CanAdd || !isParentOn}
                            onChange={(e) => handleToggleChannelAction(activeChannelTab, r.FunctionCode, 'CanAdd', e.target.checked)}
                          />
                        ),
                      },
                      {
                        title: 'Sửa',
                        width: 70,
                        align: 'center',
                        render: (_, r: ChannelFunctionRightItemDTO) => (
                          <Checkbox
                            checked={r.DirectGrant.CanEdit}
                            disabled={!r.SupportedCapabilities.CanEdit || !isParentOn}
                            onChange={(e) => handleToggleChannelAction(activeChannelTab, r.FunctionCode, 'CanEdit', e.target.checked)}
                          />
                        ),
                      },
                      {
                        title: 'Xóa',
                        width: 70,
                        align: 'center',
                        render: (_, r: ChannelFunctionRightItemDTO) => (
                          <Checkbox
                            checked={r.DirectGrant.CanDelete}
                            disabled={!r.SupportedCapabilities.CanDelete || !isParentOn}
                            onChange={(e) => handleToggleChannelAction(activeChannelTab, r.FunctionCode, 'CanDelete', e.target.checked)}
                          />
                        ),
                      },
                      {
                        title: 'In/Xuất',
                        width: 80,
                        align: 'center',
                        render: (_, r: ChannelFunctionRightItemDTO) => (
                          <Checkbox
                            checked={r.DirectGrant.CanPrint}
                            disabled={!r.SupportedCapabilities.CanPrint || !isParentOn}
                            onChange={(e) => handleToggleChannelAction(activeChannelTab, r.FunctionCode, 'CanPrint', e.target.checked)}
                          />
                        ),
                      },
                    ]}
                  />

                  <div style={{ textAlign: 'right', marginTop: 16 }}>
                    <Space>
                      <Button onClick={() => setChannelModalVisible(false)}>Đóng</Button>
                      <Button
                        type="primary"
                        loading={savingChannelPerms}
                        onClick={() => handleSaveChannelPermissions(activeChannelTab)}
                      >
                        Lưu quyền kênh {currentTree.ChannelLabel}
                      </Button>
                    </Space>
                  </div>
                </div>
              );
            })()}
          </div>
        )}
      </Modal>
    </>
  );
};

export default UserDetailDrawer;
