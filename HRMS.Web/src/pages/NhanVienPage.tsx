import { useState, useMemo } from 'react';
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
  Radio,
  Row,
  Col,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  IdcardOutlined,
  UploadOutlined,
  UserOutlined,
  CheckCircleOutlined,
  StopOutlined,
  UndoOutlined,
  UserDeleteOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import Employee360Modal from '../components/Employee360Modal';
import type { NhanVienDTO, DanhMucAllDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

const { Text } = Typography;

export const getAvatarUrl = (img?: string, _manv?: number): string | undefined => {
  if (img && img.trim()) {
    const clean = img.trim();
    if (clean.startsWith('data:') || clean.startsWith('http')) return clean;
    return `data:image/jpeg;base64,${clean}`;
  }
  return undefined;
};

interface NhanVienPageProps {
  nhanVienList: NhanVienDTO[];
  danhMuc: DanhMucAllDTO | null;
  loading: boolean;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
  onRefresh: () => void;
}

export function NhanVienPage({
  nhanVienList,
  danhMuc,
  loading,
  hasRight,
  canAdd,
  canEdit,
  canDelete,
  onRefresh,
}: NhanVienPageProps) {
  const { t } = useAppLanguage();
  const [searchKeyword, setSearchKeyword] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'resigned'>('all');
  const [departmentFilter, setDepartmentFilter] = useState<number | 'all'>('all');
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

  const safeList = useMemo(() => (Array.isArray(nhanVienList) ? nhanVienList : []), [nhanVienList]);
  const activeCount = useMemo(() => safeList.filter((nv) => nv.DATHOIVIEC !== 1).length, [safeList]);
  const resignedCount = useMemo(() => safeList.filter((nv) => nv.DATHOIVIEC === 1).length, [safeList]);

  const filteredEmployees = useMemo(() => {
    return safeList.filter((nv) => {
      // 1. Lọc theo trạng thái
      if (statusFilter === 'active' && nv.DATHOIVIEC === 1) return false;
      if (statusFilter === 'resigned' && nv.DATHOIVIEC !== 1) return false;

      // 2. Lọc theo phòng ban
      if (departmentFilter !== 'all' && nv.IDPB !== departmentFilter) return false;

      // 3. Tìm kiếm theo từ khóa
      if (!searchKeyword.trim()) return true;
      const kw = searchKeyword.toLowerCase();
      const hoTenMatch = nv.HOTEN?.toLowerCase().includes(kw);
      const maNvMatch = nv.MANV?.toString().includes(kw);
      const cccdMatch = nv.CCCD?.toLowerCase().includes(kw);
      const sdtMatch = nv.DIENTHOAI?.toLowerCase().includes(kw);
      const chucVuMatch = nv.TENCV?.toLowerCase().includes(kw);
      const phongBanMatch = nv.TENPB?.toLowerCase().includes(kw);

      return hoTenMatch || maNvMatch || cccdMatch || sdtMatch || chucVuMatch || phongBanMatch;
    });
  }, [safeList, statusFilter, departmentFilter, searchKeyword]);

  const handleOpenNvModal = (nv?: NhanVienDTO) => {
    if (nv) {
      setEditingNv(nv);
      setAvatarBase64(null);
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
        DATHOIVIEC: nv.DATHOIVIEC === 1 ? 1 : 0,
      });
    } else {
      setEditingNv(null);
      setAvatarBase64(null);
      setAvatarPreview(null);
      formNv.resetFields();
      formNv.setFieldsValue({
        GIOITINH: 'Nam',
        DATHOIVIEC: 0,
      });
    }
    setNvModalVisible(true);
  };

  const handleAvatarFileSelect = (file: File) => {
    if (file.size > 5 * 1024 * 1024) {
      notification.error({ message: t('common.error'), description: t('employee.avatarFormatNote') });
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
        DATHOIVIEC: values.DATHOIVIEC !== undefined ? values.DATHOIVIEC : (editingNv?.DATHOIVIEC ?? 0),
      };

      if (avatarBase64) {
        payload.HINHANH = avatarBase64;
      } else if (avatarPreview === null && editingNv?.HINHANH) {
        payload.HINHANH = 'REMOVE';
      }

      if (editingNv) {
        await api.put(`/nhanvien/${editingNv.MANV}`, payload);
        notification.success({ message: t('common.success'), description: t('common.updateSuccess') });
      } else {
        await api.post('/nhanvien', payload);
        notification.success({ message: t('common.success'), description: t('common.saveSuccess') });
      }
      setNvModalVisible(false);
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { message?: string };
      notification.error({
        message: t('common.error'),
        description: errorObj?.message || t('common.saveError'),
      });
    } finally {
      setSaving(false);
    }
  };

  // Thôi việc nhân viên
  const handleDeleteNv = async (nv: NhanVienDTO) => {
    try {
      await api.delete(`/nhanvien/${nv.MANV}`);
      notification.success({
        message: t('common.success'),
        description: t('common.updateSuccess'),
      });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.deleteError') });
    }
  };

  // Khôi phục nhân viên đi làm lại
  const handleRestoreNv = async (nv: NhanVienDTO) => {
    try {
      await api.post(`/nhanvien/${nv.MANV}/restore`);
      notification.success({
        message: t('common.success'),
        description: t('common.updateSuccess'),
      });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.saveError') });
    }
  };

  const employeeColumns: ColumnsType<NhanVienDTO> = [
    {
      title: t('employee.colEmpCode'),
      dataIndex: 'MANV',
      key: 'MANV',
      width: 85,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: t('employee.colFullName'),
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
              {t('employee.view360Tooltip')} &bull; {r.TENCV || t('employee.labelPosition')}
            </div>
          </div>
        </Space>
      ),
    },
    {
      title: t('employee.colStatus'),
      key: 'DATHOIVIEC',
      width: 140,
      render: (_, r: NhanVienDTO) => {
        const isResigned = r.DATHOIVIEC === 1;
        return isResigned ? (
          <Tag color="error" icon={<StopOutlined />} style={{ borderRadius: 12, padding: '2px 10px' }}>
            {t('status.resigned')}
          </Tag>
        ) : (
          <Tag color="success" icon={<CheckCircleOutlined />} style={{ borderRadius: 12, padding: '2px 10px' }}>
            {t('status.active')}
          </Tag>
        );
      },
    },
    {
      title: t('employee.colGender'),
      key: 'GIOITINH',
      width: 90,
      render: (_, r: NhanVienDTO) => {
        const gt = r.GIOITINH || (r as any).TENGT || (r.IDGT === 2 ? 'Nữ' : (r.IDGT === 3 ? 'Khác' : 'Nam'));
        const isNu = gt === 'Nữ';
        const isKhac = gt === 'Khác';
        const label = isNu ? 'Nữ' : isKhac ? 'Khác' : 'Nam';
        return <Tag color={isNu ? 'magenta' : isKhac ? 'purple' : 'blue'}>{label}</Tag>;
      },
    },
    {
      title: t('employee.colBirthDate'),
      dataIndex: 'NGAYSINH',
      key: 'NGAYSINH',
      width: 110,
      render: (d: string) => (d ? dayjs(d).format('YYYY-MM-DD') : '-'),
    },
    {
      title: t('employee.colPhone'),
      dataIndex: 'DIENTHOAI',
      key: 'DIENTHOAI',
      width: 120,
    },
    {
      title: t('employee.colDepartment'),
      dataIndex: 'TENPB',
      key: 'TENPB',
      render: (tVal: string) => <Tag color="cyan">{tVal || '-'}</Tag>,
    },
    {
      title: t('employee.colPosition'),
      dataIndex: 'TENCV',
      key: 'TENCV',
      render: (tVal: string) => <Tag color="geekblue">{tVal || '-'}</Tag>,
    },
    {
      title: t('employee.labelAddress'),
      dataIndex: 'DIACHI',
      key: 'DIACHI',
      ellipsis: true,
    },
    {
      title: t('employee.colActions'),
      key: 'action',
      width: 130,
      fixed: 'right',
      render: (_, r: NhanVienDTO) => {
        const isResigned = r.DATHOIVIEC === 1;
        return (
          <Space size="small">
            <Tooltip title={t('employee.view360Tooltip')}>
              <Button
                type="text"
                icon={<IdcardOutlined style={{ color: '#10b981', fontSize: 16 }} />}
                onClick={() => handleOpen360(r)}
                size="small"
              />
            </Tooltip>
            {(canEdit ? canEdit('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN') : hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN')) && (
              <Tooltip title={t('employee.editTooltip')}>
                <Button
                  type="text"
                  icon={<EditOutlined style={{ color: '#1677ff', fontSize: 16 }} />}
                  onClick={() => handleOpenNvModal(r)}
                  size="small"
                />
              </Tooltip>
            )}
            {(canDelete ? canDelete('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN') : hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN')) && (
              <>
                {!isResigned ? (
                  <Popconfirm
                    title={t('common.confirmDeleteTitle')}
                    description={`${t('status.resigned')}: ${r.HOTEN} (#${r.MANV})?`}
                    onConfirm={() => handleDeleteNv(r)}
                    okText={t('common.confirm')}
                    cancelText={t('common.cancel')}
                    okButtonProps={{ danger: true }}
                  >
                    <Tooltip title={t('status.resigned')}>
                      <Button
                        type="text"
                        danger
                        icon={<UserDeleteOutlined style={{ fontSize: 16 }} />}
                        size="small"
                      />
                    </Tooltip>
                  </Popconfirm>
                ) : (
                  <Popconfirm
                    title={t('common.confirm')}
                    description={`${t('status.active')}: ${r.HOTEN} (#${r.MANV})?`}
                    onConfirm={() => handleRestoreNv(r)}
                    okText={t('common.confirm')}
                    cancelText={t('common.cancel')}
                  >
                    <Tooltip title={t('status.active')}>
                      <Button
                        type="text"
                        style={{ color: '#52c41a' }}
                        icon={<UndoOutlined style={{ fontSize: 16 }} />}
                        size="small"
                      />
                    </Tooltip>
                  </Popconfirm>
                )}
              </>
            )}
          </Space>
        );
      },
    },
  ];

  return (
    <>
      {/* THANH CÔNG CỤ TÌM KIẾM, LỌC VÀ THÊM MỚI */}
      <Card
        bordered={false}
        style={{ marginBottom: 16, borderRadius: borderRadiusLG }}
        bodyStyle={{ padding: '16px 24px' }}
      >
        <div
          style={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 16,
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <Space wrap size="middle">
            <Radio.Group
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              buttonStyle="solid"
            >
              <Radio.Button value="all">
                {t('common.all')} ({safeList.length})
              </Radio.Button>
              <Radio.Button value="active">
                {t('status.active')} ({activeCount})
              </Radio.Button>
              <Radio.Button value="resigned">
                {t('status.resigned')} ({resignedCount})
              </Radio.Button>
            </Radio.Group>

            <Select
              value={departmentFilter}
              onChange={setDepartmentFilter}
              style={{ minWidth: 180 }}
              placeholder={t('employee.filterDepartment')}
              options={[
                { value: 'all', label: t('employee.filterDepartment') },
                ...(danhMuc?.phongBan?.map((pb) => ({
                  value: pb.IDPB,
                  label: pb.TENPB,
                })) || []),
              ]}
            />
          </Space>

          <Space wrap size="middle">
            <Input
              placeholder={t('employee.searchPlaceholder')}
              prefix={<SearchOutlined />}
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              allowClear
              style={{ minWidth: 240, maxWidth: 320 }}
            />
            {(canAdd ? canAdd('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN') : hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN')) && (
              <Button type="primary" icon={<PlusOutlined />} onClick={() => handleOpenNvModal()}>
                {t('employee.btnAddEmployee')}
              </Button>
            )}
          </Space>
        </div>
      </Card>

      {/* BẢNG DANH SÁCH NHÂN VIÊN */}
      <Card
        title={t('employee.listTitle', { count: filteredEmployees.length })}
        bordered={false}
        style={{ borderRadius: borderRadiusLG }}
      >
        <Table
          columns={employeeColumns}
          dataSource={filteredEmployees}
          rowKey="MANV"
          loading={loading}
          scroll={{ x: 'max-content' }}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
            showTotal: (total) => t('common.totalRecords', { total }),
          }}
        />
      </Card>

      {/* MODAL THÊM / CHỈNH SỬA HỒ SƠ NHÂN VIÊN - HOÀN TOÀN RESPONSIVE */}
      <Modal
        title={editingNv ? t('employee.modalEditTitle', { id: editingNv.MANV, name: editingNv.HOTEN }) : t('employee.modalAddTitle')}
        open={nvModalVisible}
        onCancel={() => setNvModalVisible(false)}
        onOk={handleSaveNv}
        confirmLoading={saving}
        width="min(720px, 95vw)"
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
      >
        <Form form={formNv} layout="vertical">
          {/* KHUNG TẢI ẢNH CHÂN DUNG / AVATAR */}
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              flexWrap: 'wrap',
              gap: 16,
              padding: '16px 20px',
              backgroundColor: '#f8fafc',
              borderRadius: 8,
              border: '1px dashed #cbd5e1',
              marginBottom: 20,
            }}
          >
            <Avatar
              size={72}
              icon={<UserOutlined />}
              src={avatarPreview || undefined}
              style={{ backgroundColor: '#1890ff', border: '2px solid #e8e8e8', flexShrink: 0 }}
            />
            <div style={{ flex: 1, minWidth: 200 }}>
              <div style={{ fontWeight: 600, marginBottom: 6 }}>{t('employee.avatarTitle')}</div>
              <Space wrap size="small">
                <Upload
                  showUploadList={false}
                  accept="image/*"
                  beforeUpload={handleAvatarFileSelect}
                >
                  <Button icon={<UploadOutlined />} size="small">
                    {avatarPreview ? t('employee.changeAvatarBtn') : t('employee.uploadAvatarBtn')}
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
                    {t('employee.removeAvatarBtn')}
                  </Button>
                )}
              </Space>
              <div style={{ fontSize: 11, color: '#64748b', marginTop: 4 }}>
                {t('employee.avatarFormatNote')}
              </div>
            </div>
          </div>

          <Form.Item name="HOTEN" label={t('employee.labelFullName')} rules={[{ required: true, message: t('employee.reqFullName') }]}>
            <Input placeholder={t('employee.labelFullName')} />
          </Form.Item>

          <Row gutter={[16, 0]}>
            <Col xs={24} sm={8}>
              <Form.Item name="GIOITINH" label={t('employee.labelGender')} initialValue="Nam">
                <Select
                  options={[
                    { value: 'Nam', label: 'Nam' },
                    { value: 'Nữ', label: 'Nữ' },
                    { value: 'Khác', label: 'Khác' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item name="NGAYSINH" label={t('employee.labelBirthDate')}>
                <DatePicker format="YYYY-MM-DD" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item name="DIENTHOAI" label={t('employee.labelPhone')}>
                <Input placeholder="0987654321" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={[16, 0]}>
            <Col xs={24} sm={8}>
              <Form.Item name="CCCD" label={t('employee.labelIdCard')}>
                <Input placeholder="CCCD / CMND" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item name="IDPB" label={t('employee.labelDepartment')} rules={[{ required: true, message: t('employee.reqDepartment') }]}>
                <Select
                  placeholder={t('employee.labelDepartment')}
                  options={danhMuc?.phongBan?.map((pb: { IDPB: number; TENPB: string }) => ({ value: pb.IDPB, label: pb.TENPB })) || []}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item name="IDCV" label={t('employee.labelPosition')} rules={[{ required: true, message: t('employee.reqPosition') }]}>
                <Select
                  placeholder={t('employee.labelPosition')}
                  options={danhMuc?.chucVu?.map((cv: { IDCV: number; TENCV: string }) => ({ value: cv.IDCV, label: cv.TENCV })) || []}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={[16, 0]}>
            <Col xs={24} sm={8}>
              <Form.Item name="IDBP" label={t('employee.labelDivision')}>
                <Select
                  placeholder={t('employee.labelDivision')}
                  allowClear
                  options={danhMuc?.boPhan?.map((bp) => ({ value: bp.ID, label: bp.TEN })) || []}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item name="IDTD" label={t('employee.labelEducation')}>
                <Select
                  placeholder={t('employee.labelEducation')}
                  allowClear
                  options={danhMuc?.trinhDo?.map((td) => ({ value: td.ID, label: td.TEN })) || []}
                />
              </Form.Item>
            </Col>
            {editingNv && (
              <Col xs={24} sm={8}>
                <Form.Item name="DATHOIVIEC" label={t('employee.labelStatus')}>
                  <Select
                    options={[
                      { value: 0, label: `🟢 ${t('status.active')}` },
                      { value: 1, label: `🔴 ${t('status.resigned')}` },
                    ]}
                  />
                </Form.Item>
              </Col>
            )}
          </Row>

          <Form.Item name="DIACHI" label={t('employee.labelAddress')}>
            <Input placeholder={t('employee.labelAddress')} />
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
