import { useState } from 'react';
import {
  Card,
  Table,
  Tag,
  Space,
  Button,
  Input,
  Modal,
  Form,
  Select,
  DatePicker,
  Popconfirm,
  notification,
  Typography,
  Tooltip,
  theme,
  Upload,
  Avatar,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  IdcardOutlined,
  UploadOutlined,
  UserOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import Employee360Modal from '../components/Employee360Modal';
import type { NhanVienDTO, DanhMucAllDTO } from '../types/hrms';

const { Text } = Typography;

export const getAvatarUrl = (img?: string, manv?: number): string | undefined => {
  if (img && img.trim()) {
    const clean = img.trim();
    if (clean.startsWith('data:') || clean.startsWith('http')) return clean;
    return `data:image/jpeg;base64,${clean}`;
  }
  if (manv) {
    const base = api.defaults.baseURL ? api.defaults.baseURL.replace(/\/$/, '') : '/api';
    return `${base}/nhanvien/${manv}/avatar`;
  }
  return undefined;
};

interface NhanVienPageProps {
  nhanVienList: NhanVienDTO[];
  danhMuc: DanhMucAllDTO | null;
  loading: boolean;
  hasRight: (...codes: string[]) => boolean;
  onRefresh: () => void;
}

export function NhanVienPage({
  nhanVienList,
  danhMuc,
  loading,
  hasRight,
  onRefresh,
}: NhanVienPageProps) {
  const [searchKeyword, setSearchKeyword] = useState('');
  const [nvModalVisible, setNvModalVisible] = useState(false);
  const [editingNv, setEditingNv] = useState<NhanVienDTO | null>(null);
  const [saving, setSaving] = useState(false);
  const [selectedNv360, setSelectedNv360] = useState<NhanVienDTO | null>(null);
  const [modal360Visible, setModal360Visible] = useState(false);
  const [avatarBase64, setAvatarBase64] = useState<string | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [formNv] = Form.useForm();

  const handleOpen360 = (nv: NhanVienDTO) => {
    setSelectedNv360(nv);
    setModal360Visible(true);
  };

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleOpenNvModal = (nv?: NhanVienDTO) => {
    if (nv) {
      setEditingNv(nv);
      setAvatarBase64(nv.HINHANH || null);
      setAvatarPreview(getAvatarUrl(nv.HINHANH, nv.MANV) || null);
      formNv.setFieldsValue({
        HOTEN: nv.HOTEN,
        GIOITINH: nv.GIOITINH || (nv as any).TENGT || (nv.IDGT === 2 ? 'Nữ' : 'Nam'),
        NGAYSINH: nv.NGAYSINH ? dayjs(nv.NGAYSINH) : null,
        DIENTHOAI: nv.DIENTHOAI,
        CCCD: nv.CCCD,
        DIACHI: nv.DIACHI,
        IDPB: nv.IDPB,
        IDBP: nv.IDBP,
        IDCV: nv.IDCV,
        IDTD: nv.IDTD,
      });
    } else {
      setEditingNv(null);
      setAvatarBase64(null);
      setAvatarPreview(null);
      formNv.resetFields();
    }
    setNvModalVisible(true);
  };

  const handleAvatarFileSelect = (file: File) => {
    if (file.size > 5 * 1024 * 1024) {
      notification.error({ message: 'Ảnh quá lớn', description: 'Vui lòng chọn file ảnh dung lượng dưới 5MB.' });
      return false;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      const base64 = e.target?.result as string;
      if (base64) {
        setAvatarBase64(base64);
        setAvatarPreview(base64);
      }
    };
    reader.readAsDataURL(file);
    return false;
  };

  const handleSaveNv = async () => {
    try {
      const values = await formNv.validateFields();
      setSaving(true);
      const idgt = values.GIOITINH === 'Nữ' ? 2 : (values.GIOITINH === 'Khác' ? 3 : 1);
      const payload: Record<string, unknown> = {
        ...values,
        IDGT: idgt,
        GIOITINH: values.GIOITINH,
        NGAYSINH: values.NGAYSINH ? dayjs(values.NGAYSINH).format('YYYY-MM-DD') : null,
      };

      if (avatarBase64) {
        payload.HINHANH = avatarBase64;
      } else if (avatarPreview === null && editingNv?.HINHANH) {
        payload.HINHANH = 'REMOVE';
      }

      if (editingNv) {
        await api.put(`/nhanvien/${editingNv.MANV}`, payload);
        notification.success({ message: 'Thành công', description: `Đã cập nhật nhân viên #${editingNv.MANV}` });
      } else {
        await api.post('/nhanvien', payload);
        notification.success({ message: 'Thành công', description: 'Đã thêm nhân viên mới vào hệ thống!' });
      }
      setNvModalVisible(false);
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { message?: string };
      notification.error({
        message: 'Lỗi lưu nhân viên',
        description: errorObj?.message || 'Vui lòng kiểm tra lại thông tin.',
      });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteNv = async (manv: number) => {
    try {
      await api.delete(`/nhanvien/${manv}`);
      notification.success({ message: 'Thành công', description: `Đã cho thôi việc nhân viên #${manv}` });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa nhân viên.' });
    }
  };

  const safeList = Array.isArray(nhanVienList) ? nhanVienList : [];
  const filteredEmployees = safeList.filter((nv) => {
    if (!searchKeyword) return true;
    const kw = searchKeyword.toLowerCase();
    return (
      (nv.HOTEN && nv.HOTEN.toLowerCase().includes(kw)) ||
      (nv.MANV && nv.MANV.toString().includes(kw)) ||
      (nv.TENPB && nv.TENPB.toLowerCase().includes(kw)) ||
      (nv.TENCV && nv.TENCV.toLowerCase().includes(kw)) ||
      (nv.DIENTHOAI && nv.DIENTHOAI.includes(kw))
    );
  });

  const employeeColumns: ColumnsType<NhanVienDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 85,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (text: string, r) => (
        <Space
          align="center"
          size={10}
          style={{ cursor: 'pointer' }}
          onClick={() => handleOpen360(r)}
        >
          <Avatar
            size={36}
            icon={<UserOutlined />}
            src={getAvatarUrl(r.HINHANH, r.MANV)}
            style={{ backgroundColor: '#1677ff', flexShrink: 0 }}
          />
          <div>
            <Text strong style={{ color: '#1d4ed8' }}>
              {text}
            </Text>
            <div style={{ fontSize: 11, color: '#64748b' }}>
              Xem hồ sơ 360° &bull; {r.TENCV || 'Nhân sự'}
            </div>
          </div>
        </Space>
      ),
    },
    {
      title: 'Giới tính',
      key: 'GIOITINH',
      width: 90,
      render: (_, r: NhanVienDTO) => {
        const gt = r.GIOITINH || (r as any).TENGT || (r.IDGT === 2 ? 'Nữ' : (r.IDGT === 3 ? 'Khác' : 'Nam'));
        const isNu = gt === 'Nữ';
        const isKhac = gt === 'Khác';
        return <Tag color={isNu ? 'magenta' : isKhac ? 'purple' : 'blue'}>{gt}</Tag>;
      },
    },
    {
      title: 'Ngày sinh',
      dataIndex: 'NGAYSINH',
      key: 'NGAYSINH',
      width: 110,
      render: (d: string) => (d ? dayjs(d).format('DD/MM/YYYY') : '-'),
    },
    {
      title: 'Điện thoại',
      dataIndex: 'DIENTHOAI',
      key: 'DIENTHOAI',
      width: 120,
    },
    {
      title: 'Phòng ban',
      dataIndex: 'TENPB',
      key: 'TENPB',
      render: (t: string) => <Tag color="cyan">{t}</Tag>,
    },
    {
      title: 'Chức vụ',
      dataIndex: 'TENCV',
      key: 'TENCV',
      render: (t: string) => <Tag color="geekblue">{t}</Tag>,
    },
    {
      title: 'Địa chỉ',
      dataIndex: 'DIACHI',
      key: 'DIACHI',
      ellipsis: true,
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 130,
      fixed: 'right',
      render: (_, r) => (
        <Space size="small">
          <Tooltip title="Xem Hồ Sơ 360°">
            <Button
              type="text"
              icon={<IdcardOutlined style={{ color: '#10b981', fontSize: 16 }} />}
              onClick={() => handleOpen360(r)}
              size="small"
            />
          </Tooltip>
          {hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN') && (
            <Button
              type="text"
              icon={<EditOutlined style={{ color: '#1677ff' }} />}
              onClick={() => handleOpenNvModal(r)}
              size="small"
            />
          )}
          {hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN') && (
            <Popconfirm
              title="Cho thôi việc nhân viên này?"
              description={`Bạn có chắc muốn cho thôi việc nhân viên [${r.HOTEN}] không?`}
              onConfirm={() => handleDeleteNv(r.MANV)}
              okText="Đồng ý"
              cancelText="Hủy"
            >
              <Button type="text" danger icon={<DeleteOutlined />} size="small" />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <>
      <Card
        title={`Danh sách Hồ sơ Nhân viên (${filteredEmployees.length})`}
        extra={
          <Space>
            <Input
              placeholder="Tìm theo tên, mã NV, phòng ban..."
              prefix={<SearchOutlined />}
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              allowClear
              style={{ width: 280 }}
            />
            {hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN') && (
              <Button type="primary" icon={<PlusOutlined />} onClick={() => handleOpenNvModal()}>
                Thêm nhân viên mới
              </Button>
            )}
          </Space>
        }
        bordered={false}
        style={{ borderRadius: borderRadiusLG }}
      >
        <Table
          columns={employeeColumns}
          dataSource={filteredEmployees}
          rowKey="MANV"
          loading={loading}
          scroll={{ x: 'max-content' }}
          pagination={{ pageSize: 8, showTotal: (total) => `Tổng số ${total} nhân viên` }}
        />
      </Card>

      <Modal
        title={editingNv ? `Cập nhật thông tin: #${editingNv.MANV} - ${editingNv.HOTEN}` : 'Thêm Nhân Viên Mới'}
        open={nvModalVisible}
        onCancel={() => setNvModalVisible(false)}
        onOk={handleSaveNv}
        confirmLoading={saving}
        width={700}
        destroyOnClose
      >
        <Form form={formNv} layout="vertical">
          {/* KHUNG TẢI ẢNH CHÂN DUNG / AVATAR */}
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 20,
              marginBottom: 20,
              padding: '14px 18px',
              background: '#f8fafc',
              borderRadius: 8,
              border: '1px solid #e2e8f0',
            }}
          >
            <Avatar
              size={76}
              icon={<UserOutlined />}
              src={avatarPreview || undefined}
              style={{ backgroundColor: '#1677ff', border: '2px solid #91caff', flexShrink: 0 }}
            />
            <Space direction="vertical" size={4}>
              <Space>
                <Upload
                  beforeUpload={handleAvatarFileSelect}
                  showUploadList={false}
                  accept="image/*"
                >
                  <Button icon={<UploadOutlined />} type="dashed">
                    {avatarPreview ? 'Đổi ảnh chân dung' : 'Tải ảnh chân dung'}
                  </Button>
                </Upload>
                {avatarPreview && (
                  <Button
                    danger
                    type="text"
                    size="small"
                    onClick={() => {
                      setAvatarPreview(null);
                      setAvatarBase64(null);
                    }}
                  >
                    Xóa ảnh
                  </Button>
                )}
              </Space>
              <Text type="secondary" style={{ fontSize: 12 }}>
                Hỗ trợ ảnh định dạng JPG, PNG, WEBP (Dung lượng tối đa 5MB)
              </Text>
            </Space>
          </div>

          <Form.Item name="HOTEN" label="Họ và tên" rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}>
            <Input placeholder="Ví dụ: Nguyễn Văn A" />
          </Form.Item>

          <Space style={{ width: '100%' }} size="large">
            <Form.Item name="GIOITINH" label="Giới tính" initialValue="Nam" style={{ width: 140 }}>
              <Select options={[{ value: 'Nam', label: 'Nam' }, { value: 'Nữ', label: 'Nữ' }, { value: 'Khác', label: 'Khác' }]} />
            </Form.Item>
            <Form.Item name="NGAYSINH" label="Ngày sinh" style={{ width: 200 }}>
              <DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item name="DIENTHOAI" label="Số điện thoại" style={{ width: 260 }}>
              <Input placeholder="0987654321" />
            </Form.Item>
          </Space>

          <Space style={{ width: '100%' }} size="large">
            <Form.Item name="CCCD" label="Số CCCD / CMND" style={{ width: 200 }}>
              <Input placeholder="12 chữ số CCCD" />
            </Form.Item>
            <Form.Item name="IDPB" label="Phòng ban" rules={[{ required: true, message: 'Chọn phòng ban' }]} style={{ width: 210 }}>
              <Select
                placeholder="Chọn phòng ban"
                options={danhMuc?.phongBan?.map((pb: { IDPB: number; TENPB: string }) => ({ value: pb.IDPB, label: pb.TENPB })) || []}
              />
            </Form.Item>
            <Form.Item name="IDCV" label="Chức vụ" rules={[{ required: true, message: 'Chọn chức vụ' }]} style={{ width: 210 }}>
              <Select
                placeholder="Chọn chức vụ"
                options={danhMuc?.chucVu?.map((cv: { IDCV: number; TENCV: string }) => ({ value: cv.IDCV, label: cv.TENCV })) || []}
              />
            </Form.Item>
          </Space>

          <Space style={{ width: '100%' }} size="large">
            <Form.Item name="IDBP" label="Bộ phận" style={{ width: 315 }}>
              <Select
                placeholder="Chọn bộ phận"
                allowClear
                options={danhMuc?.boPhan?.map((bp) => ({ value: bp.ID, label: bp.TEN })) || []}
              />
            </Form.Item>
            <Form.Item name="IDTD" label="Trình độ học vấn" style={{ width: 315 }}>
              <Select
                placeholder="Chọn trình độ"
                allowClear
                options={danhMuc?.trinhDo?.map((td) => ({ value: td.ID, label: td.TEN })) || []}
              />
            </Form.Item>
          </Space>

          <Form.Item name="DIACHI" label="Địa chỉ thường trú">
            <Input placeholder="Số nhà, đường, phường/xã, tỉnh/thành..." />
          </Form.Item>
        </Form>
      </Modal>

      <Employee360Modal
        visible={modal360Visible}
        onClose={() => setModal360Visible(false)}
        employee={selectedNv360}
        onEdit={(emp) => handleOpenNvModal(emp)}
        onAvatarUpdated={() => onRefresh()}
      />
    </>
  );
}

export default NhanVienPage;
