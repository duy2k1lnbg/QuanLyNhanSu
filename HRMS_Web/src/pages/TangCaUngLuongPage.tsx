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
import type { UngLuongDTO, TangCaDTO } from '../types/hrms';

const { Text } = Typography;

interface TangCaUngLuongPageProps {
  ungLuongList: UngLuongDTO[];
  tangCaList: TangCaDTO[];
  tcUlLoading: boolean;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
}

export function TangCaUngLuongPage({
  ungLuongList,
  tangCaList,
  tcUlLoading,
  onRefresh,
  hasRight,
  canAdd,
  canDelete,
}: TangCaUngLuongPageProps) {
  const [ulModalVisible, setUlModalVisible] = useState(false);
  const [tcModalVisible, setTcModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formUl] = Form.useForm();
  const [formTc] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleSaveUl = async () => {
    try {
      const values = await formUl.validateFields();
      setSaving(true);
      await api.post('/ungluong', {
        Nam: values.Nam || dayjs().year(),
        Thang: values.Thang || dayjs().month() + 1,
        Ngay: values.Ngay ? dayjs(values.Ngay).date() : dayjs().date(),
        MaNv: values.MaNv,
        SoTien: values.SoTien,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã lập phiếu tạm ứng lương.' });
      setUlModalVisible(false);
      formUl.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu tạm ứng.' });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteUl = async (id: number) => {
    try {
      await api.delete(`/ungluong/${id}`);
      notification.success({ message: 'Thành công', description: 'Đã hủy phiếu tạm ứng.' });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa tạm ứng.' });
    }
  };

  const handleSaveTc = async () => {
    try {
      const values = await formTc.validateFields();
      setSaving(true);
      await api.post('/tangca', {
        Nam: values.Nam || dayjs().year(),
        Thang: values.Thang || dayjs().month() + 1,
        Ngay: values.Ngay ? dayjs(values.Ngay).date() : dayjs().date(),
        MaNv: values.MaNv,
        SoGio: values.SoGio,
        IdLoaiCa: values.IdLoaiCa,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã ghi nhận tăng ca thành công.' });
      setTcModalVisible(false);
      formTc.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu tăng ca.' });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteTc = async (id: number) => {
    try {
      await api.delete(`/tangca/${id}`);
      notification.success({ message: 'Thành công', description: 'Đã hủy bản ghi tăng ca.' });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa tăng ca.' });
    }
  };

  return (
    <>
      <Card
        title="💸 Quản lý Tăng ca & Tạm ứng Lương"
        extra={
          <Space>
            {(canAdd ? canAdd('TANGCA', 'F_CC_TANGCA') : hasRight('TANGCA', 'F_CC_TANGCA')) && (
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  formTc.resetFields();
                  setTcModalVisible(true);
                }}
              >
                Ghi nhận Tăng ca
              </Button>
            )}
            {(canAdd ? canAdd('UNGLUONG', 'F_CC_UNGLUONG') : hasRight('UNGLUONG', 'F_CC_UNGLUONG')) && (
              <Button
                icon={<PlusOutlined />}
                onClick={() => {
                  formUl.resetFields();
                  setUlModalVisible(true);
                }}
              >
                Lập Phiếu Ứng lương
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
          defaultActiveKey="tangca"
          items={[
            {
              key: 'tangca',
              label: `Hồ sơ Tăng ca (${tangCaList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'ID', dataIndex: 'ID', key: 'ID', width: 70, render: (t) => <Tag color="blue">#{t}</Tag> },
                    { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="cyan">#{t}</Tag> },
                    { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                    { title: 'Phòng ban', dataIndex: 'TENPB', key: 'TENPB' },
                    { title: 'Loại ca', dataIndex: 'TENLOAICA', key: 'TENLOAICA', render: (t) => <Tag color="geekblue">{t}</Tag> },
                    {
                      title: 'Số giờ',
                      dataIndex: 'SOGIO',
                      key: 'SOGIO',
                      render: (t) => <Text strong style={{ color: '#1677ff' }}>{t} giờ</Text>,
                    },
                    {
                      title: 'Thành tiền',
                      dataIndex: 'SOTIEN',
                      key: 'SOTIEN',
                      render: (t) => <Text style={{ color: '#52c41a', fontWeight: 600 }}>{Number(t ?? 0).toLocaleString('vi-VN')} đ</Text>,
                    },
                    {
                      title: 'Thời gian',
                      key: 'time',
                      render: (_, r) => `${r.NGAY}/${r.THANG}/${r.NAM}`,
                    },
                    {
                      title: 'Thao tác',
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('TANGCA', 'F_CC_TANGCA') : hasRight('TANGCA', 'F_CC_TANGCA')) && (
                          <Popconfirm title="Hủy bản ghi tăng ca này?" onConfirm={() => handleDeleteTc(r.ID)}>
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={tangCaList}
                  rowKey="ID"
                  loading={tcUlLoading}
                  pagination={{ pageSize: 8 }}
                />
              ),
            },
            {
              key: 'ungluong',
              label: `Hồ sơ Tạm ứng Lương (${ungLuongList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'ID', dataIndex: 'ID', key: 'ID', width: 70, render: (t) => <Tag color="blue">#{t}</Tag> },
                    { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="cyan">#{t}</Tag> },
                    { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                    { title: 'Phòng ban', dataIndex: 'TENPB', key: 'TENPB' },
                    {
                      title: 'Số tiền ứng',
                      dataIndex: 'SOTIEN',
                      key: 'SOTIEN',
                      render: (t) => <Text strong style={{ color: '#fa8c16' }}>{Number(t ?? 0).toLocaleString('vi-VN')} đ</Text>,
                    },
                    {
                      title: 'Kỳ ứng',
                      key: 'ky',
                      render: (_, r) => `${r.NGAY}/${r.THANG}/${r.NAM}`,
                    },
                    { title: 'Ghi chú', dataIndex: 'GHICHU', key: 'GHICHU' },
                    {
                      title: 'Thao tác',
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('UNGLUONG', 'F_CC_UNGLUONG') : hasRight('UNGLUONG', 'F_CC_UNGLUONG')) && (
                          <Popconfirm title="Hủy phiếu tạm ứng này?" onConfirm={() => handleDeleteUl(r.ID)}>
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={ungLuongList}
                  rowKey="ID"
                  loading={tcUlLoading}
                  pagination={{ pageSize: 8 }}
                />
              ),
            },
          ]}
        />
      </Card>

      {/* Modal Tăng ca */}
      <Modal
        title="Ghi nhận Tăng ca (Overtime)"
        open={tcModalVisible}
        onCancel={() => setTcModalVisible(false)}
        onOk={handleSaveTc}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={formTc} layout="vertical">
          <Form.Item name="MaNv" label="Mã Nhân viên" rules={[{ required: true, message: 'Nhập mã NV' }]}>
            <InputNumber style={{ width: '100%' }} placeholder="Mã NV" />
          </Form.Item>
          <Form.Item name="IdLoaiCa" label="Loại ca làm việc" initialValue={1} rules={[{ required: true }]}>
            <Select
              options={[
                { value: 1, label: 'Ca ngày thường (Hệ số 1.5x)' },
                { value: 2, label: 'Ca ban đêm (Hệ số 2.0x)' },
                { value: 3, label: 'Ca ngày nghỉ / Chủ nhật (Hệ số 2.0x)' },
                { value: 4, label: 'Ca ngày lễ, tết (Hệ số 3.0x)' },
              ]}
            />
          </Form.Item>
          <Form.Item name="SoGio" label="Số giờ làm thêm" initialValue={2} rules={[{ required: true }]}>
            <InputNumber min={0.5} max={12} step={0.5} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="Ngay" label="Ngày tăng ca" initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="GhiChu" label="Ghi chú">
            <Input placeholder="Chi tiết dự án / công việc làm thêm..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* Modal Tạm ứng */}
      <Modal
        title="Lập Phiếu Tạm ứng Lương"
        open={ulModalVisible}
        onCancel={() => setUlModalVisible(false)}
        onOk={handleSaveUl}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={formUl} layout="vertical">
          <Form.Item name="MaNv" label="Mã Nhân viên" rules={[{ required: true, message: 'Nhập mã NV' }]}>
            <InputNumber style={{ width: '100%' }} placeholder="Mã NV" />
          </Form.Item>
          <Form.Item name="SoTien" label="Số tiền tạm ứng (VNĐ)" rules={[{ required: true, message: 'Nhập số tiền ứng' }]}>
            <InputNumber<number>
              style={{ width: '100%' }}
              min={100000}
              step={500000}
              formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              parser={(v) => Number(v?.replace(/\$\s?|(,*)/g, '') || 0)}
              placeholder="Ví dụ: 2,000,000"
            />
          </Form.Item>
          <Form.Item name="Ngay" label="Ngày ứng" initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="GhiChu" label="Lý do tạm ứng">
            <Input placeholder="Ứng tiền khám bệnh, việc gia đình..." />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}

export default TangCaUngLuongPage;
