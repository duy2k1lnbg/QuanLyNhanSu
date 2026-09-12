import { useState } from 'react';
import {
  Card,
  Table,
  Tag,
  Space,
  Button,
  Tabs,
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  DatePicker,
  Popconfirm,
  notification,
  Typography,
  theme,
} from 'antd';
import {
  PlusOutlined,
  ReloadOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import type { KhenThuongDTO } from '../types/hrms';

const { Text } = Typography;

interface KhenThuongKyLuatPageProps {
  khenThuongList: KhenThuongDTO[];
  kyLuatList: KhenThuongDTO[];
  ktLoading: boolean;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
}

export function KhenThuongKyLuatPage({
  khenThuongList,
  kyLuatList,
  ktLoading,
  onRefresh,
  hasRight,
}: KhenThuongKyLuatPageProps) {
  const [ktModalVisible, setKtModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formKt] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleSaveKt = async () => {
    try {
      const values = await formKt.validateFields();
      setSaving(true);
      await api.post('/khenthuong', {
        SoQd: values.SoQd,
        Loai: values.Loai,
        MaNv: values.MaNv,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        NoiDung: values.NoiDung,
        LyDo: values.LyDo,
      });
      notification.success({ message: 'Thành công', description: 'Đã lưu quyết định thành công.' });
      setKtModalVisible(false);
      formKt.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu quyết định.' });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteKt = async (soqd: string) => {
    try {
      await api.delete(`/khenthuong/${encodeURIComponent(soqd)}`);
      notification.success({ message: 'Thành công', description: 'Đã xóa quyết định.' });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa quyết định.' });
    }
  };

  return (
    <>
      <Card
        title="🏆 Quản lý Khen thưởng & Kỷ luật"
        extra={
          <Space>
            {hasRight('KHENTHUONG', 'KYLUAT', 'F_NV_KHENTHUONG', 'F_NV_KYLUAT') && (
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  formKt.resetFields();
                  setKtModalVisible(true);
                }}
              >
                Tạo Quyết định mới
              </Button>
            )}
            <Button icon={<ReloadOutlined />} onClick={onRefresh}>
              Làm mới
            </Button>
          </Space>
        }
        bordered={false}
        style={{ borderRadius: borderRadiusLG }}
      >
        <Tabs
          defaultActiveKey="khenthuong"
          items={[
            {
              key: 'khenthuong',
              label: `Quyết định Khen thưởng (${khenThuongList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="green">{t}</Tag> },
                    { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                    { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                    { title: 'Ngày ban hành', dataIndex: 'NGAY', key: 'NGAY' },
                    { title: 'Nội dung khen thưởng', dataIndex: 'NOIDUNG', key: 'NOIDUNG' },
                    { title: 'Lý do', dataIndex: 'LYDO', key: 'LYDO' },
                    {
                      title: 'Thao tác',
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        hasRight('KHENTHUONG', 'F_NV_KHENTHUONG') && (
                          <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDeleteKt(r.SOQD)}>
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={khenThuongList}
                  rowKey="SOQD"
                  loading={ktLoading}
                  pagination={{ pageSize: 8 }}
                />
              ),
            },
            {
              key: 'kyluat',
              label: `Quyết định Kỷ luật (${kyLuatList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="red">{t}</Tag> },
                    { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                    { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                    { title: 'Ngày ban hành', dataIndex: 'NGAY', key: 'NGAY' },
                    { title: 'Nội dung kỷ luật', dataIndex: 'NOIDUNG', key: 'NOIDUNG' },
                    { title: 'Lý do vi phạm', dataIndex: 'LYDO', key: 'LYDO' },
                    {
                      title: 'Thao tác',
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        hasRight('KYLUAT', 'F_NV_KYLUAT') && (
                          <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDeleteKt(r.SOQD)}>
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={kyLuatList}
                  rowKey="SOQD"
                  loading={ktLoading}
                  pagination={{ pageSize: 8 }}
                />
              ),
            },
          ]}
        />
      </Card>

      <Modal
        title="Ban hành Quyết định Khen thưởng / Kỷ luật"
        open={ktModalVisible}
        onCancel={() => setKtModalVisible(false)}
        onOk={handleSaveKt}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={formKt} layout="vertical">
          <Form.Item name="SoQd" label="Số Quyết định" rules={[{ required: true, message: 'Nhập số QĐ' }]}>
            <Input placeholder="Ví dụ: QD-2026-001" />
          </Form.Item>
          <Form.Item name="Loai" label="Loại quyết định" initialValue={1} rules={[{ required: true }]}>
            <Select options={[{ value: 1, label: 'Khen thưởng (+)' }, { value: 2, label: 'Kỷ luật (-)' }]} />
          </Form.Item>
          <Form.Item name="MaNv" label="Mã Nhân viên" rules={[{ required: true, message: 'Nhập mã NV' }]}>
            <InputNumber style={{ width: '100%' }} placeholder="Mã số nhân viên (ví dụ: 10)" />
          </Form.Item>
          <Form.Item name="Ngay" label="Ngày áp dụng" initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="NoiDung" label="Nội dung">
            <Input.TextArea rows={2} placeholder="Chi tiết nội dung quyết định..." />
          </Form.Item>
          <Form.Item name="LyDo" label="Lý do">
            <Input placeholder="Lý do khen thưởng hoặc vi phạm..." />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}

export default KhenThuongKyLuatPage;
