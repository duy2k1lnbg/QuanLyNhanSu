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
  SwapOutlined,
  ReloadOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import type { NangLuongDTO, DieuChuyenDTO, DanhMucAllDTO } from '../types/hrms';

const { Text } = Typography;

interface NangLuongDieuChuyenPageProps {
  nangLuongList: NangLuongDTO[];
  dieuChuyenList: DieuChuyenDTO[];
  nlDcLoading: boolean;
  danhMuc: DanhMucAllDTO | null;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
}

export function NangLuongDieuChuyenPage({
  nangLuongList,
  dieuChuyenList,
  nlDcLoading,
  danhMuc,
  onRefresh,
  hasRight,
}: NangLuongDieuChuyenPageProps) {
  const [nlModalVisible, setNlModalVisible] = useState(false);
  const [dcModalVisible, setDcModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formNl] = Form.useForm();
  const [formDc] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleSaveNl = async () => {
    try {
      const values = await formNl.validateFields();
      setSaving(true);
      await api.post('/nangluong', {
        SoQd: values.SoQd,
        SoHd: values.SoHd,
        MaNv: values.MaNv,
        HeSoLuongHienTai: values.HeSoLuongHienTai,
        HeSoLuongMoi: values.HeSoLuongMoi,
        NgayKy: values.NgayKy ? dayjs(values.NgayKy).format('YYYY-MM-DD') : null,
        NgayLenLuong: values.NgayLenLuong ? dayjs(values.NgayLenLuong).format('YYYY-MM-DD') : null,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã lưu quyết định nâng lương.' });
      setNlModalVisible(false);
      formNl.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu nâng lương.' });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteNl = async (soqd: string) => {
    try {
      await api.delete(`/nangluong/${encodeURIComponent(soqd)}`);
      notification.success({ message: 'Thành công', description: 'Đã xóa quyết định nâng lương.' });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa quyết định.' });
    }
  };

  const handleSaveDc = async () => {
    try {
      const values = await formDc.validateFields();
      setSaving(true);
      await api.post('/dieuchuyen', {
        SoQd: values.SoQd,
        MaNv: values.MaNv,
        IdPb2: values.IdPb2,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        LyDo: values.LyDo,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã lưu quyết định điều chuyển phòng ban.' });
      setDcModalVisible(false);
      formDc.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu điều chuyển.' });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteDc = async (soqd: string) => {
    try {
      await api.delete(`/dieuchuyen/${encodeURIComponent(soqd)}`);
      notification.success({ message: 'Thành công', description: 'Đã xóa quyết định điều chuyển.' });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa quyết định.' });
    }
  };

  return (
    <>
      <Card
        title="📈 Quản lý Nâng lương & Điều chuyển"
        extra={
          <Space>
            {hasRight('NANGLUONG', 'F_NV_NANGLUONG') && (
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  formNl.resetFields();
                  setNlModalVisible(true);
                }}
              >
                Tạo QĐ Nâng lương
              </Button>
            )}
            {hasRight('DIEUCHUYEN', 'F_NV_DIEUCHUYEN') && (
              <Button
                icon={<SwapOutlined />}
                onClick={() => {
                  formDc.resetFields();
                  setDcModalVisible(true);
                }}
              >
                Tạo QĐ Điều chuyển
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
          defaultActiveKey="nangluong"
          items={[
            {
              key: 'nangluong',
              label: `Quyết định Nâng lương (${nangLuongList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="blue">{t}</Tag> },
                    { title: 'Số HĐ', dataIndex: 'SOHD', key: 'SOHD' },
                    { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="cyan">#{t}</Tag> },
                    { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                    { title: 'Hệ số cũ', dataIndex: 'HESOLUONG_HIENTAI', key: 'HESOLUONG_HIENTAI', render: (t) => `${t ?? 1.0}x` },
                    {
                      title: 'Hệ số mới',
                      dataIndex: 'HESOLUONG_MOI',
                      key: 'HESOLUONG_MOI',
                      render: (t) => <Tag color="green" style={{ fontWeight: 'bold' }}>{t}x</Tag>,
                    },
                    { title: 'Ngày ký', dataIndex: 'NGAYKY', key: 'NGAYKY' },
                    { title: 'Ngày áp dụng', dataIndex: 'NGAYLENLUONG', key: 'NGAYLENLUONG' },
                    {
                      title: 'Thao tác',
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        hasRight('NANGLUONG', 'F_NV_NANGLUONG') && (
                          <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDeleteNl(r.SOQD)}>
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={nangLuongList}
                  rowKey="SOQD"
                  loading={nlDcLoading}
                  pagination={{ pageSize: 8 }}
                />
              ),
            },
            {
              key: 'dieuchuyen',
              label: `Quyết định Điều chuyển Phòng ban (${dieuChuyenList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="purple">{t}</Tag> },
                    { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="cyan">#{t}</Tag> },
                    { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                    { title: 'Phòng ban cũ', dataIndex: 'TENPB', key: 'TENPB', render: (t) => <Tag color="default">{t}</Tag> },
                    {
                      title: 'Phòng ban mới',
                      dataIndex: 'TENPB2',
                      key: 'TENPB2',
                      render: (t) => <Tag color="success" style={{ fontWeight: 'bold' }}>{t}</Tag>,
                    },
                    { title: 'Ngày điều chuyển', dataIndex: 'NGAY', key: 'NGAY' },
                    { title: 'Lý do', dataIndex: 'LYDO', key: 'LYDO' },
                    {
                      title: 'Thao tác',
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        hasRight('DIEUCHUYEN', 'F_NV_DIEUCHUYEN') && (
                          <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDeleteDc(r.SOQD)}>
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={dieuChuyenList}
                  rowKey="SOQD"
                  loading={nlDcLoading}
                  pagination={{ pageSize: 8 }}
                />
              ),
            },
          ]}
        />
      </Card>

      {/* Modal Nâng lương */}
      <Modal
        title="Tạo Quyết định Nâng lương"
        open={nlModalVisible}
        onCancel={() => setNlModalVisible(false)}
        onOk={handleSaveNl}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={formNl} layout="vertical">
          <Form.Item name="SoQd" label="Số Quyết định" rules={[{ required: true, message: 'Nhập số QĐ' }]}>
            <Input placeholder="Ví dụ: NL-2026-001" />
          </Form.Item>
          <Form.Item name="SoHd" label="Số Hợp đồng liên quan" rules={[{ required: true, message: 'Nhập số HĐ' }]}>
            <Input placeholder="Ví dụ: 00001/2026/HĐLĐ" />
          </Form.Item>
          <Form.Item name="MaNv" label="Mã Nhân viên" rules={[{ required: true, message: 'Nhập mã NV' }]}>
            <InputNumber style={{ width: '100%' }} placeholder="Mã NV" />
          </Form.Item>
          <Space style={{ width: '100%' }}>
            <Form.Item name="HeSoLuongHienTai" label="Hệ số cũ" initialValue={1.0}>
              <InputNumber step={0.1} min={1.0} style={{ width: 140 }} />
            </Form.Item>
            <Form.Item name="HeSoLuongMoi" label="Hệ số mới" initialValue={1.2} rules={[{ required: true }]}>
              <InputNumber step={0.1} min={1.0} style={{ width: 140 }} />
            </Form.Item>
          </Space>
          <Space style={{ width: '100%' }}>
            <Form.Item name="NgayKy" label="Ngày ký" initialValue={dayjs()}>
              <DatePicker format="DD/MM/YYYY" style={{ width: 180 }} />
            </Form.Item>
            <Form.Item name="NgayLenLuong" label="Ngày áp dụng" initialValue={dayjs()}>
              <DatePicker format="DD/MM/YYYY" style={{ width: 180 }} />
            </Form.Item>
          </Space>
          <Form.Item name="GhiChu" label="Ghi chú">
            <Input placeholder="Ghi chú thêm..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* Modal Điều chuyển */}
      <Modal
        title="Tạo Quyết định Điều chuyển Phòng ban"
        open={dcModalVisible}
        onCancel={() => setDcModalVisible(false)}
        onOk={handleSaveDc}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={formDc} layout="vertical">
          <Form.Item name="SoQd" label="Số Quyết định" rules={[{ required: true, message: 'Nhập số QĐ' }]}>
            <Input placeholder="Ví dụ: DC-2026-001" />
          </Form.Item>
          <Form.Item name="MaNv" label="Mã Nhân viên" rules={[{ required: true, message: 'Nhập mã NV' }]}>
            <InputNumber style={{ width: '100%' }} placeholder="Mã NV" />
          </Form.Item>
          <Form.Item name="IdPb2" label="Phòng ban chuyển tới" rules={[{ required: true, message: 'Chọn phòng ban mới' }]}>
            <Select
              placeholder="Chọn phòng ban đích"
              options={danhMuc?.phongBan?.map((pb: { IDPB: number; TENPB: string }) => ({ value: pb.IDPB, label: pb.TENPB })) || []}
            />
          </Form.Item>
          <Form.Item name="Ngay" label="Ngày điều chuyển" initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="LyDo" label="Lý do điều chuyển">
            <Input placeholder="Điều động nhân sự theo nhu cầu..." />
          </Form.Item>
          <Form.Item name="GhiChu" label="Ghi chú">
            <Input placeholder="Ghi chú thêm..." />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}

export default NangLuongDieuChuyenPage;
