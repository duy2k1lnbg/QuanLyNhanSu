import React, { useState } from 'react';
import { Modal, Form, Input, message } from 'antd';
import { LockOutlined, SafetyOutlined } from '@ant-design/icons';
import api from '../services/api';

interface ChangePasswordModalProps {
  visible: boolean;
  onClose: () => void;
  username: string;
}

export const ChangePasswordModal: React.FC<ChangePasswordModalProps> = ({
  visible,
  onClose,
  username,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (values.newPassword !== values.confirmPassword) {
        message.error('Mật khẩu xác nhận không khớp với mật khẩu mới.');
        return;
      }

      setLoading(true);
      await api.post('/auth/change-password', {
        OldPassword: values.oldPassword,
        NewPassword: values.newPassword,
      });

      message.success('Đổi mật khẩu thành công!');
      form.resetFields();
      onClose();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      message.error(errorObj.response?.data?.Message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra mật khẩu hiện tại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={
        <span>
          <SafetyOutlined style={{ color: '#1677ff', marginRight: 8 }} />
          Đổi mật khẩu tài khoản: <b>{username}</b>
        </span>
      }
      open={visible}
      onOk={handleSubmit}
      onCancel={() => {
        form.resetFields();
        onClose();
      }}
      confirmLoading={loading}
      okText="Xác nhận đổi mật khẩu"
      cancelText="Hủy"
      width={460}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="oldPassword"
          label="Mật khẩu hiện tại"
          rules={[{ required: true, message: 'Vui lòng nhập mật khẩu hiện tại' }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Nhập mật khẩu đang sử dụng" />
        </Form.Item>

        <Form.Item
          name="newPassword"
          label="Mật khẩu mới"
          rules={[
            { required: true, message: 'Vui lòng nhập mật khẩu mới' },
            { min: 3, message: 'Mật khẩu phải từ 3 ký tự trở lên' },
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Nhập mật khẩu mới" />
        </Form.Item>

        <Form.Item
          name="confirmPassword"
          label="Xác nhận mật khẩu mới"
          rules={[{ required: true, message: 'Vui lòng xác nhận mật khẩu mới' }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Nhập lại mật khẩu mới" />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ChangePasswordModal;
