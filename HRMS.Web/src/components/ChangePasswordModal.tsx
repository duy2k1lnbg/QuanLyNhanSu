import React, { useState } from 'react';
import { Modal, Form, Input, message } from 'antd';
import { LockOutlined, SafetyOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';

interface ChangePasswordModalProps {
  visible: boolean;
  onClose: () => void;
  username: string;
  onPasswordChanged?: () => void;
}

export const ChangePasswordModal: React.FC<ChangePasswordModalProps> = ({
  visible,
  onClose,
  username,
  onPasswordChanged,
}) => {
  const { t } = useAppLanguage();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (values.newPassword !== values.confirmPassword) {
        message.error(t('auth.passwordMismatch'));
        return;
      }

      setLoading(true);
      await api.post('/auth/change-password', {
        OldPassword: values.oldPassword,
        NewPassword: values.newPassword,
      });

      message.success('Đổi mật khẩu thành công! Toàn bộ phiên đăng nhập cũ đã được thu hồi. Vui lòng đăng nhập lại.');
      form.resetFields();
      onClose();
      if (onPasswordChanged) {
        onPasswordChanged();
      }
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string; message?: string } }; message?: string };
      message.error(errorObj.response?.data?.message || errorObj.response?.data?.Message || t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={
        <span>
          <SafetyOutlined style={{ color: '#1677ff', marginRight: 8 }} />
          {t('auth.changePasswordTitle')}: <b>{username}</b>
        </span>
      }
      open={visible}
      onOk={handleSubmit}
      onCancel={() => {
        form.resetFields();
        onClose();
      }}
      confirmLoading={loading}
      okText={t('auth.changePasswordBtn')}
      cancelText={t('common.cancel')}
      width="min(460px, 95vw)"
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="oldPassword"
          label={t('auth.oldPassword')}
          rules={[{ required: true, message: t('auth.oldPassword') }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('auth.oldPassword')} />
        </Form.Item>

        <Form.Item
          name="newPassword"
          label={t('auth.newPassword')}
          rules={[
            { required: true, message: t('auth.newPassword') },
            { min: 3, message: 'Mật khẩu phải từ 3 ký tự trở lên' },
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('auth.newPassword')} />
        </Form.Item>

        <Form.Item
          name="confirmPassword"
          label={t('auth.confirmPassword')}
          rules={[{ required: true, message: t('auth.confirmPassword') }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('auth.confirmPassword')} />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ChangePasswordModal;
