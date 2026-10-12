import React, { useEffect } from 'react';
import { Modal, Form, Input, Switch, message, Space, Tag } from 'antd';
import { EditOutlined, LockOutlined, UserOutlined, TeamOutlined } from '@ant-design/icons';
import api from '../services/api';
import type { SysUserDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

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
  const { t } = useAppLanguage();
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

      message.success(t('user.updateSuccessMsg', { username: user.Username }));
      if (onSuccess) onSuccess();
      onClose();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      message.error(errorObj.response?.data?.Message || t('user.updateErrorMsg'));
    }
  };

  const isGroup = user?.IsGroup;

  return (
    <Modal
      title={
        <Space wrap>
          <EditOutlined style={{ color: '#1677ff' }} />
          <span>
            {isGroup ? t('user.modalEditGroupTitle') : t('user.modalEditUserTitle')}: <b>{user?.Username}</b>
          </span>
          {isGroup ? (
            <Tag color="purple" icon={<TeamOutlined />}>
              {t('user.tagGroup')}
            </Tag>
          ) : (
            <Tag color="blue" icon={<UserOutlined />}>
              {t('user.tagUser')}
            </Tag>
          )}
        </Space>
      }
      open={visible}
      onOk={handleSubmit}
      onCancel={onClose}
      okText={t('common.save')}
      cancelText={t('common.cancel')}
      width="min(500px, 95vw)"
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item name="Username" label={isGroup ? t('user.groupCode') : t('user.colUsername')}>
          <Input disabled />
        </Form.Item>

        <Form.Item
          name="FullName"
          label={isGroup ? t('user.groupDesc') : t('user.colFullName')}
          rules={[{ required: true, message: t('user.requiredField') }]}
        >
          <Input placeholder={isGroup ? 'Ví dụ: Nhóm Nhân sự' : 'Ví dụ: Nguyễn Văn A'} />
        </Form.Item>

        {!isGroup && (
          <Form.Item
            name="NewPassword"
            label={t('user.resetPasswordLabel')}
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder={t('user.resetPasswordPlaceholder')}
            />
          </Form.Item>
        )}

        {!isGroup && (
          <Form.Item
            name="Disabled"
            label={t('user.colStatus')}
            valuePropName="checked"
          >
            <Switch
              checkedChildren={t('user.statusLockedSwitch')}
              unCheckedChildren={t('user.statusActiveSwitch')}
            />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
};

export default UserEditModal;
