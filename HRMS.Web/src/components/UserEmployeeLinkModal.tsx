import React, { useState, useEffect } from 'react';
import { Modal, Form, Select, Switch, Button, Space, Tag, Typography, Popconfirm, notification, Alert, Divider } from 'antd';
import { LinkOutlined, DisconnectOutlined, MobileOutlined, UserOutlined, IdcardOutlined } from '@ant-design/icons';
import api from '../services/api';
import type { SysUserDTO, NhanVienDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

const { Text } = Typography;

interface UserEmployeeLinkModalProps {
  visible: boolean;
  onClose: () => void;
  user: SysUserDTO | null;
  onSuccess?: () => void;
}

export const UserEmployeeLinkModal: React.FC<UserEmployeeLinkModalProps> = ({
  visible,
  onClose,
  user,
  onSuccess,
}) => {
  const { t } = useAppLanguage();
  const [form] = Form.useForm();
  const [employees, setEmployees] = useState<NhanVienDTO[]>([]);
  const [loadingEmployees, setLoadingEmployees] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (visible) {
      fetchEmployees();
      if (user) {
        form.setFieldsValue({
          EmployeeId: user.Manv || user.manv || undefined,
          IsMobileEnabled: user.IsMobileEnabled !== undefined ? user.IsMobileEnabled : true,
        });
      }
    }
  }, [visible, user]);

  const fetchEmployees = async () => {
    try {
      setLoadingEmployees(true);
      const res = await api.get('/nhanvien');
      const data = res.data?.data || res.data || [];
      // Filter only active employees
      const active = Array.isArray(data) ? data.filter((nv: NhanVienDTO) => (nv.DATHOIVIEC ?? 0) !== 1) : [];
      setEmployees(active);
    } catch {
      notification.error({ message: t('common.error'), description: t('user.fetchEmpError') });
    } finally {
      setLoadingEmployees(false);
    }
  };

  const handleLink = async () => {
    if (!user) return;
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      await api.post(`/users/${user.IdUser}/link-employee`, {
        EmployeeId: values.EmployeeId,
        IsMobileEnabled: values.IsMobileEnabled,
      });

      notification.success({
        message: t('common.success'),
        description: t('user.linkSuccessMsg', { username: user.Username }),
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string; message?: string } }; message?: string };
      const msg = errorObj.response?.data?.Message || errorObj.response?.data?.message || t('user.linkError');
      notification.error({ message: t('user.linkError'), description: msg });
    } finally {
      setSubmitting(false);
    }
  };

  const handleUnlink = async () => {
    if (!user) return;
    try {
      setSubmitting(true);
      await api.post(`/users/${user.IdUser}/unlink-employee`);
      notification.success({
        message: t('common.success'),
        description: t('user.unlinkSuccessMsg', { username: user.Username }),
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch {
      notification.error({ message: t('common.error'), description: t('user.unlinkError') });
    } finally {
      setSubmitting(false);
    }
  };

  const currentManv = user?.Manv || user?.manv;
  const currentEmpCode = user?.EmployeeCode || user?.employeeCode;
  const currentEmpName = user?.EmployeeName || user?.employeeName;

  return (
    <Modal
      title={
        <Space wrap>
          <LinkOutlined style={{ color: '#1677ff' }} />
          <span>{t('user.linkModalFullTitle')}</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={[
        <Button key="cancel" onClick={onClose}>
          {t('common.close')}
        </Button>,
        currentManv && (
          <Popconfirm
            key="unlink"
            title={t('user.unlinkConfirmTitle')}
            description={t('user.unlinkConfirmDesc')}
            onConfirm={handleUnlink}
            okText={t('user.btnUnlink')}
            cancelText={t('common.cancel')}
            okButtonProps={{ danger: true }}
          >
            <Button danger icon={<DisconnectOutlined />} loading={submitting}>
              {t('user.btnUnlink')}
            </Button>
          </Popconfirm>
        ),
        <Button key="submit" type="primary" onClick={handleLink} loading={submitting}>
          {currentManv ? t('user.btnUpdateLink') : t('user.btnConfirmLink')}
        </Button>,
      ]}
      width="min(560px, 95vw)"
    >
      <Alert
        message={t('user.identityRuleTitle')}
        description={
          <ul style={{ paddingLeft: 16, margin: 0, fontSize: 13 }}>
            <li>{t('user.identityRule1')}</li>
            <li>{t('user.identityRule2')}</li>
            <li>{t('user.identityRule3')}</li>
          </ul>
        }
        type="info"
        showIcon
        style={{ marginBottom: 16, marginTop: 8 }}
      />

      <div style={{ background: '#fafafa', padding: 12, borderRadius: 8, marginBottom: 16 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text type="secondary">{t('user.loginAccountLabel')} </Text>
            <Text strong style={{ color: '#1677ff', fontSize: 14 }}>
              <UserOutlined style={{ marginRight: 6 }} />
              {user?.Username}
            </Text>
            <span style={{ marginLeft: 8 }}>({user?.FullName})</span>
          </div>

          <div>
            <Text type="secondary">{t('user.linkStatusLabel')} </Text>
            {currentManv ? (
              <Tag color="success" icon={<IdcardOutlined />}>
                {currentEmpCode || `NV#${currentManv}`} - {currentEmpName || t('user.tagUser')}
              </Tag>
            ) : (
              <Tag color="orange">{t('employee360.notLinked')}</Tag>
            )}
          </div>
        </Space>
      </div>

      <Form form={form} layout="vertical">
        <Form.Item
          name="EmployeeId"
          label={t('user.selectEmpLabel')}
          rules={[{ required: true, message: t('user.selectEmpRequired') }]}
        >
          <Select
            showSearch
            placeholder={t('user.selectEmpPlaceholder')}
            loading={loadingEmployees}
            optionFilterProp="children"
            filterOption={(input, option) => {
              const label = String(option?.children ?? '').toLowerCase();
              return label.includes(input.toLowerCase());
            }}
          >
            {employees.map((emp) => {
              const code = emp.EMPLOYEE_CODE || emp.EmployeeCode || `NV${String(emp.MANV).padStart(6, '0')}`;
              const name = emp.HOTEN || '';
              const id = emp.MANV;
              return (
                <Select.Option key={id} value={id}>
                  [{code}] {name} {emp.TENPB ? `(${emp.TENPB})` : ''}
                </Select.Option>
              );
            })}
          </Select>
        </Form.Item>

        <Divider style={{ margin: '12px 0' }} />

        <Form.Item
          name="IsMobileEnabled"
          label={t('employee360.mobileAccess')}
          valuePropName="checked"
          help={t('user.mobileAccessHelp')}
        >
          <Switch
            checkedChildren={<Space><MobileOutlined />{t('employee360.switchOn')}</Space>}
            unCheckedChildren={t('employee360.switchOff')}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default UserEmployeeLinkModal;
