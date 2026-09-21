import React, { useState } from 'react';
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
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import type { SysUserDTO } from '../types/hrms';

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
        width={560}
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
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Title level={4} style={{ margin: 0, color: '#1677ff' }}>
                <UserOutlined style={{ marginRight: 8 }} />
                {user.Username}
              </Title>
              <Space>
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
        <Card title="3. Kênh truy cập hệ thống (Access Channels)" size="small" style={{ marginBottom: 16 }}>
          <Row gutter={12}>
            <Col span={8}>
              <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                <DesktopOutlined style={{ fontSize: 20, color: '#1677ff', marginBottom: 4 }} />
                <div>Desktop Power User</div>
                <Tag color="green" style={{ marginTop: 4 }}>Cho phép</Tag>
              </Card>
            </Col>
            <Col span={8}>
              <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                <GlobalOutlined style={{ fontSize: 20, color: '#722ed1', marginBottom: 4 }} />
                <div>Web Portal</div>
                <Tag color="green" style={{ marginTop: 4 }}>Cho phép</Tag>
              </Card>
            </Col>
            <Col span={8}>
              <Card size="small" style={{ textAlign: 'center', background: '#fafafa' }}>
                <MobileOutlined style={{ fontSize: 20, color: isRootAdmin ? '#d9d9d9' : isMobileEnabled ? '#52c41a' : '#fa8c16', marginBottom: 4 }} />
                <div>Mobile ESS</div>
                {isRootAdmin ? (
                  <Tag color="default" style={{ marginTop: 4 }}>Bị chặn</Tag>
                ) : isMobileEnabled && hasLinked ? (
                  <Tag color="cyan" style={{ marginTop: 4 }}>Đã kích hoạt</Tag>
                ) : isMobileEnabled ? (
                  <Tag color="blue" style={{ marginTop: 4 }}>Chờ liên kết</Tag>
                ) : (
                  <Tag color="default" style={{ marginTop: 4 }}>Đã tắt</Tag>
                )}
              </Card>
            </Col>
          </Row>
        </Card>

        {/* 4. Administrative Actions */}
        {(!canEdit || canEdit('F_SYSTEM_USER')) && !isRootAdmin && (
          <Card title="4. Thao tác Quản trị tài khoản" size="small">
            <Space wrap>
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
    </>
  );
};

export default UserDetailDrawer;
