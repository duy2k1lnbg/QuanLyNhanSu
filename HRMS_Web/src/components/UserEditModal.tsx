import React, { useEffect } from 'react';
import { Modal, Form, Input, Switch, message, Space, Tag } from 'antd';
import { EditOutlined, LockOutlined, UserOutlined, TeamOutlined } from '@ant-design/icons';
import api from '../services/api';
import type { SysUserDTO } from '../types/hrms';

interface UserEditModalProps {
  visible: boolean;
  onClose: () => void;
  user: SysUserDTO | null;
  onSuccess?: () => void;
}

export const UserEditModal: React.FC<UserEditModalProps> = ({
  visible,
  onClose,
  user,
  onSuccess,
}) => {
  const [form] = Form.useForm();

  useEffect(() => {
    if (visible && user) {
      form.setFieldsValue({
        Username: user.Username,
        FullName: user.FullName,
        Disabled: user.Disabled || false,
        NewPassword: '',
      });
    }
  }, [visible, user, form]);

  const handleSubmit = async () => {
    if (!user) return;
    try {
      const values = await form.validateFields();
      await api.put(`/users/${user.IdUser}`, {
        FullName: values.FullName?.trim(),
        Disabled: values.Disabled,
        NewPassword: values.NewPassword?.trim() ? values.NewPassword.trim() : undefined,
      });

      message.success(`Đã cập nhật thông tin [${user.Username}] thành công!`);
      if (onSuccess) onSuccess();
      onClose();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      message.error(errorObj.response?.data?.Message || 'Cập nhật thất bại.');
    }
  };

  const isGroup = user?.IsGroup;
  const isAdminUser = user?.Username?.toUpperCase() === 'ADMIN';

  return (
    <Modal
      title={
        <Space>
          <EditOutlined style={{ color: '#1677ff' }} />
          <span>
            {isGroup ? 'Chỉnh sửa nhóm quyền' : 'Chỉnh sửa thông tin người dùng'}: <b>{user?.Username}</b>
          </span>
          {isGroup ? (
            <Tag color="purple" icon={<TeamOutlined />}>
              Nhóm
            </Tag>
          ) : (
            <Tag color="blue" icon={<UserOutlined />}>
              Người dùng
            </Tag>
          )}
        </Space>
      }
      open={visible}
      onOk={handleSubmit}
      onCancel={onClose}
      okText="Lưu thay đổi"
      cancelText="Hủy"
      width={500}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item name="Username" label={isGroup ? 'Mã nhóm quyền' : 'Tên tài khoản'}>
          <Input disabled />
        </Form.Item>

        <Form.Item
          name="FullName"
          label={isGroup ? 'Mô tả nhóm quyền' : 'Họ và tên'}
          rules={[{ required: true, message: 'Vui lòng nhập thông tin này' }]}
        >
          <Input placeholder={isGroup ? 'Ví dụ: Nhóm Nhân sự' : 'Ví dụ: Nguyễn Văn A'} />
        </Form.Item>

        {!isGroup && (
          <Form.Item
            name="NewPassword"
            label="Đặt lại mật khẩu mới (Bỏ trống nếu không muốn đổi)"
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="Nhập mật khẩu mới nếu muốn đổi..."
            />
          </Form.Item>
        )}

        {!isGroup && !isAdminUser && (
          <Form.Item
            name="Disabled"
            label="Trạng thái tài khoản"
            valuePropName="checked"
          >
            <Switch
              checkedChildren="Bị khóa"
              unCheckedChildren="Hoạt động"
            />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
};

export default UserEditModal;
