import React, { useState } from 'react';
import { Form, Input, Button, Alert, message } from 'antd';
import {
  SafetyCertificateOutlined,
  UserOutlined,
  LockOutlined,
  LoginOutlined,
} from '@ant-design/icons';
import api from '../../../services/api';
import type { CurrentUserDTO } from '../../../types/hrms';
import { useAppLanguage } from '../../../services/i18n';
import { LandingModal } from './LandingModal';



interface LoginModalProps {
  visible: boolean;
  onClose: () => void;
  onLoginSuccess: (user: CurrentUserDTO, token: string) => void;
  tLanding?: any;
  tAuth?: any;
}

export const LoginModal: React.FC<LoginModalProps> = ({
  visible,
  onClose,
  onLoginSuccess,
  tLanding: propTLanding,
  tAuth: propTAuth,
}) => {
  const { dict, tLanding: hookTLanding, tAuth: hookTAuth } = useAppLanguage();

  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const auth = propTAuth || hookTAuth || dict?.auth || {};
  const landing = propTLanding || hookTLanding || dict?.landing || {};

  // Comprehensive fallbacks so labels and placeholders are NEVER empty
  const title = auth.loginModalTitle || landing.loginModalTitle || 'ĐĂNG NHẬP HỆ THỐNG';
  const subtitle = auth.loginModalSubtitle || landing.loginModalSubtitle || 'Cổng Quản Trị Nhân Sự Trực Tuyến';
  const usernameLabel = auth.usernameLabel || landing.usernameLabel || 'Tên đăng nhập';
  const usernamePlaceholder = auth.usernamePlaceholder || landing.usernamePlaceholder || 'Nhập tên đăng nhập...';
  const usernameRequired = auth.usernameRequired || landing.usernameRequired || 'Vui lòng nhập tên tài khoản!';
  const passwordLabel = auth.passwordLabel || landing.passwordLabel || 'Mật khẩu';
  const passwordPlaceholder = auth.passwordPlaceholder || landing.passwordPlaceholder || 'Nhập mật khẩu...';
  const passwordRequired = auth.passwordRequired || landing.passwordRequired || 'Vui lòng nhập mật khẩu!';
  const btnSubmitText = auth.btnLoginSubmit || landing.btnLoginSubmit || 'Đăng nhập hệ thống';
  const loggingInText = auth.loggingIn || 'Đang xác thực...';
  const securityFooter = auth.loginSecurityFooter || landing.loginSecurityFooter || '🔒 Bảo mật mã hóa SSL 256-bit • Tiêu chuẩn RFC 7519 JWT';

  const handleLogin = async (values: { username: string; password: string }) => {
    setLoading(true);
    setErrorMessage(null);
    try {
      const res = await api.post('/auth/login', {
        Username: values.username.trim(),
        Password: values.password,
        ClientType: 'WEB',
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
          DetailedRights: rawUser.DetailedRights || rawUser.detailedRights,
        };

        localStorage.setItem('hrms_token', token);
        localStorage.setItem('hrms_user', JSON.stringify(normalizedUser));
        message.success(`Chào mừng ${normalizedUser.FullName} đăng nhập thành công!`);
        onClose();
        onLoginSuccess(normalizedUser, token);
      } else {
        setErrorMessage(
          res.data?.message || res.data?.Message || auth.loginError || 'Tên đăng nhập hoặc mật khẩu không chính xác.'
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
    <LandingModal
      open={visible}
      onCancel={() => { setErrorMessage(null); onClose(); }}
      footer={null}
      width={500}
      className="landing-login-modal"
      title={
        <div className="landing-modal-visual">
          <span className="landing-modal-kicker">TRYHARDAGAIN · HRMS ENTERPRISE</span>
          <h2>{title}</h2>
          <p>{subtitle}</p>
        </div>
      }
    >
      {errorMessage && (
        <Alert message={errorMessage} type="error" showIcon closable
          onClose={() => setErrorMessage(null)} />
      )}
      <Form form={form} layout="vertical" onFinish={handleLogin} preserve={false}>
        <Form.Item name="username" label={usernameLabel}
          rules={[{ required: true, message: usernameRequired }]}>
          <Input size="large" prefix={<UserOutlined />} placeholder={usernamePlaceholder}
            autoComplete="username" autoFocus />
        </Form.Item>
        <Form.Item name="password" label={passwordLabel}
          rules={[{ required: true, message: passwordRequired }]}>
          <Input.Password size="large" prefix={<LockOutlined />}
            placeholder={passwordPlaceholder} autoComplete="current-password" />
        </Form.Item>
        <Form.Item style={{ marginBottom: 0 }}>
          <Button type="primary" htmlType="submit" size="large" block loading={loading}
            icon={<LoginOutlined />} className="landing-login-submit">
            {loading ? loggingInText : btnSubmitText}
          </Button>
        </Form.Item>
      </Form>
      <div className="landing-modal-note">
        <SafetyCertificateOutlined />
        <span>{securityFooter}</span>
      </div>
    </LandingModal>
  );
};
