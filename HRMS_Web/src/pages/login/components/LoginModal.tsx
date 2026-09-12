import React, { useState } from 'react';
import { Modal, Form, Input, Button, Typography, Tag, Alert, message } from 'antd';
import {
  SafetyCertificateOutlined,
  UserOutlined,
  LockOutlined,
  LoginOutlined,
} from '@ant-design/icons';
import api from '../../../services/api';
import type { CurrentUserDTO } from '../../../types/hrms';

const { Title, Text, Paragraph } = Typography;

interface LoginModalProps {
  visible: boolean;
  onClose: () => void;
  onLoginSuccess: (user: CurrentUserDTO, token: string) => void;
  tLanding: any;
}

export const LoginModal: React.FC<LoginModalProps> = ({
  visible,
  onClose,
  onLoginSuccess,
  tLanding,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleLogin = async (values: { username: string; password: string }) => {
    setLoading(true);
    setErrorMessage(null);
    try {
      const res = await api.post('/auth/login', {
        Username: values.username.trim(),
        Password: values.password,
      });

      const token = res.data?.token || res.data?.Token;
      const rawUser = res.data?.user || res.data?.User;

      if (token && rawUser) {
        const normalizedUser: CurrentUserDTO = {
          IdUser: rawUser.IdUser ?? rawUser.id ?? 0,
          Username: rawUser.Username || rawUser.username || values.username.trim(),
          FullName: rawUser.FullName || rawUser.fullName || rawUser.Username || values.username.trim(),
          IsAdmin: Boolean(rawUser.IsAdmin ?? rawUser.isAdmin),
          Rights: rawUser.Rights || rawUser.rights || [],
        };

        localStorage.setItem('hrms_token', token);
        localStorage.setItem('hrms_user', JSON.stringify(normalizedUser));
        message.success(`Chào mừng ${normalizedUser.FullName} đăng nhập thành công!`);
        onClose();
        onLoginSuccess(normalizedUser, token);
      } else {
        setErrorMessage(
          res.data?.message || res.data?.Message || 'Tên đăng nhập hoặc mật khẩu không chính xác.'
        );
      }
    } catch (err: unknown) {
      console.error('Chi tiết lỗi đăng nhập:', err);
      const errorObj = err as {
        response?: { data?: { Message?: string; message?: string } | string; status?: number };
        message?: string;
        config?: { url?: string; baseURL?: string };
      };
      let msg = 'Không thể kết nối đến máy chủ đăng nhập.';
      if (typeof errorObj.response?.data === 'string') {
        if (
          errorObj.response.data.includes('<html') ||
          errorObj.response.data.includes('Exception') ||
          errorObj.response.data.includes('ORA-')
        ) {
          msg = `Lỗi máy chủ (${errorObj.response?.status || 500}). Vui lòng kiểm tra dịch vụ Backend và kết nối CSDL Oracle.`;
        } else {
          msg = errorObj.response.data;
        }
      } else if (
        typeof errorObj.response?.data === 'object' &&
        errorObj.response?.data !== null &&
        ('Message' in errorObj.response.data || 'message' in errorObj.response.data)
      ) {
        msg = (errorObj.response.data as any).Message || (errorObj.response.data as any).message || msg;
      } else if (errorObj.message) {
        const target = (errorObj.config?.baseURL || '') + (errorObj.config?.url || '');
        msg = `Không thể kết nối máy chủ API (${target || 'HRMS Backend'}: ${errorObj.message}). Vui lòng đảm bảo Backend HRMS_API đang hoạt động!`;
      }
      setErrorMessage(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      open={visible}
      onCancel={() => {
        setErrorMessage(null);
        onClose();
      }}
      footer={null}
      width={440}
      centered
      destroyOnClose
      bodyStyle={{ padding: '32px 28px' }}
      style={{ borderRadius: 18, overflow: 'hidden' }}
    >
      {/* Header Modal */}
      <div style={{ textAlign: 'center', marginBottom: 24 }}>
        <div
          style={{
            width: 54,
            height: 54,
            margin: '0 auto 12px',
            borderRadius: 14,
            background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: '0 8px 18px rgba(37,99,235,0.35)',
          }}
        >
          <SafetyCertificateOutlined style={{ fontSize: 28, color: '#fff' }} />
        </div>
        <Title level={3} style={{ margin: 0, fontWeight: 800, color: '#0f172a' }}>
          {tLanding.loginModalTitle}
        </Title>
        <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 0, fontSize: 13 }}>
          {tLanding.loginModalSubtitle}
        </Paragraph>
        <Tag color="processing" style={{ marginTop: 8, borderRadius: 10, fontWeight: 500 }}>
          <SafetyCertificateOutlined style={{ marginRight: 4 }} /> {tLanding.loginModalSSLTag}
        </Tag>
      </div>

      {errorMessage && (
        <Alert
          message={errorMessage}
          type="error"
          showIcon
          closable
          onClose={() => setErrorMessage(null)}
          style={{ marginBottom: 18, borderRadius: 8 }}
        />
      )}

      <Form form={form} layout="vertical" onFinish={handleLogin}>
        <Form.Item
          name="username"
          label={<Text strong style={{ color: '#334155' }}>{tLanding.usernameLabel}</Text>}
          rules={[{ required: true, message: tLanding.usernameRequired }]}
        >
          <Input
            size="large"
            prefix={<UserOutlined style={{ color: '#94a3b8' }} />}
            placeholder={tLanding.usernamePlaceholder}
            autoFocus
            style={{ borderRadius: 8 }}
          />
        </Form.Item>

        <Form.Item
          name="password"
          label={<Text strong style={{ color: '#334155' }}>{tLanding.passwordLabel}</Text>}
          rules={[{ required: true, message: tLanding.passwordRequired }]}
        >
          <Input.Password
            size="large"
            prefix={<LockOutlined style={{ color: '#94a3b8' }} />}
            placeholder={tLanding.passwordPlaceholder}
            style={{ borderRadius: 8 }}
          />
        </Form.Item>

        <Form.Item style={{ marginTop: 24, marginBottom: 0 }}>
          <Button
            type="primary"
            htmlType="submit"
            size="large"
            block
            loading={loading}
            icon={<LoginOutlined />}
            style={{
              height: 46,
              borderRadius: 10,
              fontWeight: 700,
              fontSize: 15,
              background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
              border: 'none',
              boxShadow: '0 6px 16px rgba(37,99,235,0.4)',
            }}
          >
            {tLanding.btnLoginSubmit}
          </Button>
        </Form.Item>
      </Form>

      <div
        style={{
          marginTop: 20,
          paddingTop: 14,
          borderTop: '1px solid #f1f5f9',
          textAlign: 'center',
          fontSize: 12,
          color: '#64748b',
        }}
      >
        {tLanding.loginSecurityFooter}
      </div>
    </Modal>
  );
};
