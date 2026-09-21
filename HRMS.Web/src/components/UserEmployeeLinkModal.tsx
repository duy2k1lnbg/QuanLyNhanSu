import React, { useState, useEffect } from 'react';
import { Modal, Form, Select, Switch, Button, Space, Tag, Typography, Popconfirm, notification, Alert, Divider } from 'antd';
import { LinkOutlined, DisconnectOutlined, MobileOutlined, UserOutlined, IdcardOutlined } from '@ant-design/icons';
import api from '../services/api';
import type { SysUserDTO, NhanVienDTO } from '../types/hrms';

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
      notification.error({ message: 'Lỗi', description: 'Không thể tải danh sách nhân viên.' });
    } finally {
      setLoadingEmployees(false);
    }
  };

  const handleLink = async () => {
    if (!user) return;
    if (user.IsAdmin || user.Username?.toUpperCase() === 'ADMIN') {
      notification.warning({
        message: 'Không khả dụng',
        description: 'Tài khoản Quản trị viên tối cao (ADMIN) là tài khoản hệ thống, không được phép liên kết với hồ sơ nhân viên.',
      });
      return;
    }
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      await api.post(`/users/${user.IdUser}/link-employee`, {
        EmployeeId: values.EmployeeId,
        IsMobileEnabled: values.IsMobileEnabled,
      });

      notification.success({
        message: 'Thành công',
        description: `Đã liên kết tài khoản [${user.Username}] với nhân viên thành công!`,
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string; message?: string } }; message?: string };
      const msg = errorObj.response?.data?.Message || errorObj.response?.data?.message || 'Liên kết thất bại.';
      notification.error({ message: 'Lỗi liên kết', description: msg });
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
        message: 'Thành công',
        description: `Đã hủy liên kết hồ sơ nhân viên cho tài khoản [${user.Username}].`,
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể hủy liên kết.' });
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
        <Space>
          <LinkOutlined style={{ color: '#1677ff' }} />
          <span>Liên kết Tài khoản ↔ Hồ sơ Nhân sự (Quy tắc 9 & 21)</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Đóng
        </Button>,
        currentManv && (
          <Popconfirm
            key="unlink"
            title="Hủy liên kết nhân viên?"
            description="Tài khoản này sẽ không thể đăng nhập trên Mobile cho đến khi được liên kết lại."
            onConfirm={handleUnlink}
            okText="Hủy liên kết"
            cancelText="Không"
            okButtonProps={{ danger: true }}
          >
            <Button danger icon={<DisconnectOutlined />} loading={submitting}>
              Hủy liên kết
            </Button>
          </Popconfirm>
        ),
        <Button key="submit" type="primary" onClick={handleLink} loading={submitting}>
          {currentManv ? 'Cập nhật liên kết' : 'Xác nhận liên kết'}
        </Button>,
      ]}
      width={560}
    >
      <Alert
        message="Nguyên tắc phân định danh tính an toàn"
        description={
          <ul style={{ paddingLeft: 16, margin: 0, fontSize: 13 }}>
            <li>Tên đăng nhập (LoginName) và Mã nhân sự (EmployeeCode) độc lập 100%.</li>
            <li>Quan hệ liên kết chuẩn 1:1. Một tài khoản chỉ liên kết với 1 nhân viên và ngược lại.</li>
            <li>Nhân viên có thể dùng cả LoginName hoặc Mã nhân sự nguyên bản để đăng nhập Mobile.</li>
          </ul>
        }
        type="info"
        showIcon
        style={{ marginBottom: 16, marginTop: 8 }}
      />

      <div style={{ background: '#fafafa', padding: 12, borderRadius: 8, marginBottom: 16 }}>
        <Space orientation="vertical" style={{ width: '100%' }}>
          <div>
            <Text type="secondary">Tài khoản hệ thống (Login Account): </Text>
            <Text strong style={{ color: '#1677ff', fontSize: 14 }}>
              <UserOutlined style={{ marginRight: 6 }} />
              {user?.Username}
            </Text>
            <span style={{ marginLeft: 8 }}>({user?.FullName})</span>
          </div>

          <div>
            <Text type="secondary">Trạng thái liên kết hiện tại: </Text>
            {currentManv ? (
              <Tag color="success" icon={<IdcardOutlined />}>
                {currentEmpCode || `NV#${currentManv}`} - {currentEmpName || 'Hồ sơ nhân sự'}
              </Tag>
            ) : (
              <Tag color="orange">Chưa liên kết hồ sơ</Tag>
            )}
          </div>
        </Space>
      </div>

      <Form form={form} layout="vertical">
        <Form.Item
          name="EmployeeId"
          label="Chọn hồ sơ nhân viên để liên kết (1:1)"
          rules={[{ required: true, message: 'Vui lòng chọn nhân viên cần liên kết' }]}
        >
          <Select
            showSearch
            placeholder="Tìm theo họ tên hoặc mã nhân viên (VD: PX01-KT-TV-2026-001, 01, NV000123)..."
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
          label="Quyền truy cập ứng dụng di động (Mobile Access)"
          valuePropName="checked"
          help="Bật tùy chọn này để cho phép nhân viên đăng nhập vào ứng dụng HRMS Mobile Self-Service."
        >
          <Switch
            checkedChildren={<Space><MobileOutlined />Bật</Space>}
            unCheckedChildren="Tắt"
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default UserEmployeeLinkModal;
