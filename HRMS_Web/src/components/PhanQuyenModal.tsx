import React, { useState, useEffect } from 'react';
import {
  Modal,
  Table,
  Checkbox,
  Button,
  Input,
  Space,
  Tag,
  Typography,
  message,
  Alert,
  Segmented,
  Tooltip,
} from 'antd';
import {
  KeyOutlined,
  SaveOutlined,
  CheckSquareOutlined,
  BorderOutlined,
  SearchOutlined,
  SafetyCertificateOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../services/api';
import type { SysRightItemDTO } from '../types/hrms';

const { Text } = Typography;

interface PhanQuyenModalProps {
  visible: boolean;
  onClose: () => void;
  userId: number;
  username: string;
  fullName: string;
  isGroup?: boolean;
  onSuccess?: () => void;
}

export const PhanQuyenModal: React.FC<PhanQuyenModalProps> = ({
  visible,
  onClose,
  userId,
  username,
  fullName,
  isGroup = false,
  onSuccess,
}) => {
  const [rights, setRights] = useState<SysRightItemDTO[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [saving, setSaving] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>('');
  const [categoryFilter, setCategoryFilter] = useState<string>('ALL');

  useEffect(() => {
    if (visible && userId) {
      loadRights();
    }
  }, [visible, userId]);

  const loadRights = async () => {
    setLoading(true);
    try {
      const res = await api.get<any[]>(`/users/${userId}/rights`);
      if (res.data) {
        const mapped: SysRightItemDTO[] = res.data.map((item: any) => ({
          FuncCode: item.FUNCTION_CODE || item.FuncCode || item.funcCode || '',
          Description: item.DESCRIPTION || item.Description || item.description || '',
          Parent: item.PARENT || item.Parent || item.parent || 'OTHER',
          Sort: item.SORT ?? item.Sort ?? 999,
          HasRight: item.HAS_RIGHT !== undefined ? Boolean(item.HAS_RIGHT) : Boolean(item.HasRight ?? item.hasRight),
        }));
        setRights(mapped);
      }
    } catch {
      message.error('Không thể tải danh sách quyền.');
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = (funcCode: string, checked: boolean) => {
    setRights((prev) =>
      prev.map((item) => (item.FuncCode === funcCode ? { ...item, HasRight: checked } : item))
    );
  };

  const handleSelectAll = (check: boolean) => {
    setRights((prev) => prev.map((item) => ({ ...item, HasRight: check })));
  };

  const handleSelectCurrentCategory = (check: boolean) => {
    setRights((prev) =>
      prev.map((item) => {
        if (categoryFilter === 'ALL' || item.Parent?.toUpperCase() === categoryFilter.toUpperCase()) {
          return { ...item, HasRight: check };
        }
        return item;
      })
    );
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const selectedCodes = rights.filter((r) => r.HasRight).map((r) => r.FuncCode);
      await api.post(`/users/${userId}/rights`, {
        FunctionCodes: selectedCodes,
        FuncCodes: selectedCodes,
      });
      message.success(`Đã cập nhật ${selectedCodes.length} quyền cho [${username}]!`);
      if (onSuccess) onSuccess();
      onClose();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      message.error(errorObj.response?.data?.Message || 'Lỗi khi lưu phân quyền.');
    } finally {
      setSaving(false);
    }
  };

  const getParentLabel = (parent?: string) => {
    switch (parent?.toUpperCase()) {
      case 'NV':
        return { label: 'Nhân sự & Hợp đồng', color: 'blue' };
      case 'DM':
        return { label: 'Danh mục', color: 'cyan' };
      case 'CC':
        return { label: 'Chấm công & Lương', color: 'gold' };
      case 'BC':
        return { label: 'Báo cáo', color: 'green' };
      case 'SYSTEM':
        return { label: 'Hệ thống & AI', color: 'purple' };
      case 'TÀI KHOẢN':
      case 'TAIKHOAN':
        return { label: 'Tài khoản', color: 'orange' };
      default:
        return { label: parent || 'Khác', color: 'default' };
    }
  };

  const filteredData = rights.filter((item) => {
    const matchesSearch =
      item.FuncCode.toLowerCase().includes(searchText.toLowerCase()) ||
      item.Description.toLowerCase().includes(searchText.toLowerCase());

    if (!matchesSearch) return false;

    if (categoryFilter === 'ALL') return true;
    return item.Parent?.toUpperCase() === categoryFilter.toUpperCase();
  });

  const activeCount = rights.filter((r) => r.HasRight).length;

  const columns: ColumnsType<SysRightItemDTO> = [
    {
      title: 'Mã chức năng',
      dataIndex: 'FuncCode',
      key: 'FuncCode',
      width: 170,
      render: (code: string) => (
        <Tag color="geekblue" style={{ fontWeight: 600 }}>
          {code}
        </Tag>
      ),
    },
    {
      title: 'Tên chức năng hệ thống',
      dataIndex: 'Description',
      key: 'Description',
      render: (desc: string) => <Text strong>{desc}</Text>,
    },
    {
      title: 'Phân hệ',
      dataIndex: 'Parent',
      key: 'Parent',
      width: 160,
      render: (parent: string) => {
        const info = getParentLabel(parent);
        return <Tag color={info.color}>{info.label}</Tag>;
      },
    },
    {
      title: 'Cấp quyền truy cập',
      dataIndex: 'HasRight',
      key: 'HasRight',
      width: 150,
      align: 'center',
      render: (hasRight: boolean, record: SysRightItemDTO) => (
        <Checkbox
          checked={hasRight}
          onChange={(e) => handleToggle(record.FuncCode, e.target.checked)}
        >
          {hasRight ? (
            <Text type="success" strong>
              Được phép
            </Text>
          ) : (
            <Text type="secondary">Chặn</Text>
          )}
        </Checkbox>
      ),
    },
  ];

  return (
    <Modal
      title={
        <Space>
          <KeyOutlined style={{ color: '#fa8c16', fontSize: 18 }} />
          <span>
            Phân quyền chức năng cho {isGroup ? 'nhóm' : 'tài khoản'}:{' '}
            <b style={{ color: '#1677ff' }}>{fullName}</b> ({username})
          </span>
          {isGroup ? (
            <Tag color="purple" icon={<TeamOutlined />}>
              Nhóm quyền
            </Tag>
          ) : (
            <Tag color="blue" icon={<UserOutlined />}>
              Người dùng
            </Tag>
          )}
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width={860}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Đóng
        </Button>,
        <Button
          key="save"
          type="primary"
          icon={<SaveOutlined />}
          loading={saving}
          onClick={handleSave}
          style={{ background: '#1677ff' }}
        >
          Lưu phân quyền ({activeCount} / {rights.length} quyền được cấp)
        </Button>,
      ]}
    >
      <Alert
        message={
          <span>
            {isGroup ? (
              <>
                Phân quyền cho <b>Nhóm quyền</b> sẽ tự động áp dụng cho toàn bộ thành viên trong nhóm theo mô hình RBAC chuẩn của WinForms (<b>TB_SYS_RIGHT & TB_SYS_GROUP</b>).
              </>
            ) : (
              <>
                Phân quyền trực tiếp cho <b>Tài khoản</b>. Người dùng sẽ hưởng các quyền được chọn ở đây kèm theo toàn bộ quyền của các nhóm mà người dùng tham gia.
              </>
            )}
          </span>
        }
        type="info"
        showIcon
        style={{ marginBottom: 16, marginTop: 8 }}
      />

      <div style={{ marginBottom: 14 }}>
        <Segmented
          options={[
            { label: 'Tất cả', value: 'ALL' },
            { label: '👤 Nhân sự (NV)', value: 'NV' },
            { label: '⏰ Chấm công & Lương (CC)', value: 'CC' },
            { label: '📋 Danh mục (DM)', value: 'DM' },
            { label: '📊 Báo cáo (BC)', value: 'BC' },
            { label: '⚙️ Hệ thống & AI (SYSTEM)', value: 'SYSTEM' },
          ]}
          value={categoryFilter}
          onChange={(val) => setCategoryFilter(val as string)}
        />
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 14, gap: 12 }}>
        <Input
          placeholder="Tìm kiếm mã hoặc tên chức năng..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          allowClear
          style={{ maxWidth: 300 }}
        />
        <Space>
          <Tooltip title="Chọn tất cả chức năng thuộc phân hệ đang lọc">
            <Button
              icon={<CheckSquareOutlined />}
              onClick={() => handleSelectCurrentCategory(true)}
              size="small"
            >
              Chọn nhóm này
            </Button>
          </Tooltip>
          <Tooltip title="Bỏ chọn tất cả chức năng thuộc phân hệ đang lọc">
            <Button
              icon={<BorderOutlined />}
              onClick={() => handleSelectCurrentCategory(false)}
              size="small"
            >
              Bỏ chọn nhóm này
            </Button>
          </Tooltip>
          <Button
            type="dashed"
            icon={<SafetyCertificateOutlined />}
            onClick={() => handleSelectAll(true)}
            size="small"
          >
            Cấp toàn quyền ({rights.length})
          </Button>
        </Space>
      </div>

      <Table
        columns={columns}
        dataSource={filteredData}
        rowKey="FuncCode"
        loading={loading}
        pagination={{ pageSize: 8, showTotal: (total) => `Hiển thị ${total} chức năng` }}
        size="small"
      />
    </Modal>
  );
};

export default PhanQuyenModal;
