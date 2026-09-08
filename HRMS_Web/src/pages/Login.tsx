import React, { useState } from 'react';
import { Card, Form, Input, Button, Typography, Space, Tag, Alert, message, Divider } from 'antd';
import { UserOutlined, LockOutlined, LoginOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import api from '../services/api';
import type { CurrentUserDTO } from '../types/hrms';

const { Title, Text, Paragraph } = Typography;

interface LoginProps {
  onLoginSuccess: (user: CurrentUserDTO, token: string) => void;
}

export const Login: React.FC<LoginProps> = ({ onLoginSuccess }) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState<boolean>(false);
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
        onLoginSuccess(normalizedUser, token);
      } else {
        setErrorMessage(
          res.data?.message || res.data?.Message || 'Tên đăng nhập hoặc mật khẩu không chính xác.'
        );
      }
    } catch (err: unknown) {
      const errorObj = err as {
        response?: { data?: { Message?: string; message?: string } | string };
        message?: string;
      };
      let msg = 'Không thể kết nối đến máy chủ đăng nhập.';
      if (typeof errorObj.response?.data === 'string') {
        msg = errorObj.response.data;
      } else if (
        typeof errorObj.response?.data === 'object' &&
        errorObj.response?.data !== null &&
        ('Message' in errorObj.response.data || 'message' in errorObj.response.data)
      ) {
        msg = errorObj.response.data.Message || errorObj.response.data.message || msg;
      } else if (errorObj.message) {
        msg = errorObj.message;
      }
      setErrorMessage(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleQuickLogin = (user: string, pass: string) => {
    form.setFieldsValue({ username: user, password: pass });
    handleLogin({ username: user, password: pass });
  };

  return (
    <div
      style={{
        minHeight: '100vh',
        width: '100%',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'radial-gradient(ellipse at top, #1e3a8a 0%, #0f172a 70%, #020617 100%)',
        padding: '24px',
        position: 'relative',
        overflow: 'hidden',
      }}
    >
      {/* Background ambient glowing circles */}
      <div
        style={{
          position: 'absolute',
          width: 500,
          height: 500,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(59,130,246,0.15) 0%, rgba(0,0,0,0) 70%)',
          top: '-10%',
          left: '10%',
          pointerEvents: 'none',
        }}
      />
      <div
        style={{
          position: 'absolute',
          width: 400,
          height: 400,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(168,85,247,0.12) 0%, rgba(0,0,0,0) 70%)',
          bottom: '-10%',
          right: '15%',
          pointerEvents: 'none',
        }}
      />

      <Card
        style={{
          width: 440,
          maxWidth: '100%',
          borderRadius: 16,
          boxShadow: '0 20px 50px rgba(0, 0, 0, 0.5)',
          background: 'rgba(255, 255, 255, 0.96)',
          backdropFilter: 'blur(16px)',
          border: '1px solid rgba(255, 255, 255, 0.2)',
        }}
        bodyStyle={{ padding: '36px 32px' }}
      >
        {/* Header Branding */}
        <div style={{ textAlign: 'center', marginBottom: 28 }}>
          <div
            style={{
              width: 56,
              height: 56,
              margin: '0 auto 12px',
              borderRadius: 14,
              background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 8px 16px rgba(37,99,235,0.3)',
            }}
          >
            <SafetyCertificateOutlined style={{ fontSize: 28, color: '#fff' }} />
          </div>
          <Title level={3} style={{ margin: 0, fontWeight: 700, color: '#0f172a' }}>
            HRMS ENTERPRISE
          </Title>
          <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 0, fontSize: 13 }}>
            Cổng Quản Trị Nhân Sự & Phân Quyền Doanh Nghiệp
          </Paragraph>
          <Tag color="processing" style={{ marginTop: 8, borderRadius: 10 }}>
            Oracle DB Connected
          </Tag>
        </div>

        {errorMessage && (
          <Alert
            message={errorMessage}
            type="error"
            showIcon
            closable
            onClose={() => setErrorMessage(null)}
            style={{ marginBottom: 20, borderRadius: 8 }}
          />
        )}

        <Form form={form} layout="vertical" onFinish={handleLogin} initialValues={{ username: 'ADMIN', password: '123' }}>
          <Form.Item
            name="username"
            label={<Text strong>Tên đăng nhập</Text>}
            rules={[{ required: true, message: 'Vui lòng nhập tên tài khoản!' }]}
          >
            <Input
              size="large"
              prefix={<UserOutlined style={{ color: '#94a3b8' }} />}
              placeholder="ADMIN / nhansu / chamcong"
              autoFocus
            />
          </Form.Item>

          <Form.Item
            name="password"
            label={<Text strong>Mật khẩu</Text>}
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
          >
            <Input.Password
              size="large"
              prefix={<LockOutlined style={{ color: '#94a3b8' }} />}
              placeholder="Nhập mật khẩu..."
            />
          </Form.Item>

          <Form.Item style={{ marginTop: 24 }}>
            <Button
              type="primary"
              htmlType="submit"
              size="large"
              block
              loading={loading}
              icon={<LoginOutlined />}
              style={{
                height: 46,
                borderRadius: 8,
                fontWeight: 600,
                fontSize: 15,
                background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                border: 'none',
                boxShadow: '0 4px 12px rgba(37,99,235,0.35)',
              }}
            >
              Đăng nhập hệ thống
            </Button>
          </Form.Item>
        </Form>

        <Divider plain style={{ margin: '20px 0 16px', color: '#64748b', fontSize: 12 }}>
          ĐĂNG NHẬP NHANH (TÀI KHOẢN PHÂN QUYỀN WINFORM)
        </Divider>

        {/* Quick Demo Logins corresponding to WinForm role matrix */}
        <Space direction="vertical" style={{ width: '100%' }} size={8}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
            <Button
              size="middle"
              onClick={() => handleQuickLogin('ADMIN', 'ADMIN')}
              style={{ borderRadius: 6, fontSize: 12, textAlign: 'left', borderColor: '#bfdbfe', background: '#eff6ff' }}
            >
              👑 <b>ADMIN</b> (Toàn quyền)
            </Button>
            <Button
              size="middle"
              onClick={() => handleQuickLogin('nhansu', '123')}
              style={{ borderRadius: 6, fontSize: 12, textAlign: 'left', borderColor: '#bbf7d0', background: '#f0fdf4' }}
            >
              👥 <b>Nhân sự</b> (QL Hồ sơ)
            </Button>
            <Button
              size="middle"
              onClick={() => handleQuickLogin('chamcong', '123')}
              style={{ borderRadius: 6, fontSize: 12, textAlign: 'left', borderColor: '#fed7aa', background: '#fff7ed' }}
            >
              🕒 <b>Chấm công</b> (Bảng lương)
            </Button>
            <Button
              size="middle"
              onClick={() => handleQuickLogin('baocao', '123')}
              style={{ borderRadius: 6, fontSize: 12, textAlign: 'left', borderColor: '#e9d5ff', background: '#faf5ff' }}
            >
              📊 <b>Báo cáo</b> (Dashboard)
            </Button>
          </div>
        </Space>
      </Card>
    </div>
  );
};

export default Login;
