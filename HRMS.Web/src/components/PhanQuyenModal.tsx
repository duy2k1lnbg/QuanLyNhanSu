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
  EyeOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  PrinterOutlined,
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
        const mapped: SysRightItemDTO[] = res.data.map((item: any) => {
          const canView = item.CAN_VIEW !== undefined ? Boolean(item.CAN_VIEW) : Boolean(item.CanView ?? item.canView ?? item.HAS_RIGHT ?? item.HasRight ?? item.hasRight);
          const canAdd = item.CAN_ADD !== undefined ? Boolean(item.CAN_ADD) : Boolean(item.CanAdd ?? item.canAdd);
          const canEdit = item.CAN_EDIT !== undefined ? Boolean(item.CAN_EDIT) : Boolean(item.CanEdit ?? item.canEdit);
          const canDelete = item.CAN_DELETE !== undefined ? Boolean(item.CAN_DELETE) : Boolean(item.CanDelete ?? item.canDelete);
          const canPrint = item.CAN_PRINT !== undefined ? Boolean(item.CAN_PRINT) : Boolean(item.CanPrint ?? item.canPrint);

          return {
            FuncCode: item.FUNCTION_CODE || item.FuncCode || item.funcCode || '',
            Description: item.DESCRIPTION || item.Description || item.description || '',
            Parent: item.PARENT || item.Parent || item.parent || 'OTHER',
            Sort: item.SORT ?? item.Sort ?? 999,
            HasRight: canView,
            CanView: canView,
            CanAdd: canAdd,
            CanEdit: canEdit,
            CanDelete: canDelete,
            CanPrint: canPrint,
          };
        });
        setRights(mapped);
      }
    } catch {
      message.error('Không thể tải danh sách quyền.');
    } finally {
      setLoading(false);
    }
  };

  // Toggle từng quyền hành động trên một dòng
  const handleToggleAction = (
    funcCode: string,
    action: 'CanView' | 'CanAdd' | 'CanEdit' | 'CanDelete' | 'CanPrint',
    checked: boolean
  ) => {
    setRights((prev) =>
      prev.map((item) => {
        if (item.FuncCode !== funcCode) return item;
        const updated = { ...item, [action]: checked };

        // Nếu bật Thêm/Sửa/Xóa/In thì tự động kích hoạt quyền Xem
        if (checked && action !== 'CanView') {
          updated.CanView = true;
          updated.HasRight = true;
        }

        // Nếu tắt quyền Xem thì tự động tắt toàn bộ thao tác còn lại
        if (action === 'CanView') {
          updated.HasRight = checked;
          if (!checked) {
            updated.CanAdd = false;
            updated.CanEdit = false;
            updated.CanDelete = false;
            updated.CanPrint = false;
          }
        }
        return updated;
      })
    );
  };

  // Bật/Tắt toàn quyền 5 thao tác cho một dòng chức năng
  const handleToggleRowAll = (funcCode: string, checked: boolean) => {
    setRights((prev) =>
      prev.map((item) =>
        item.FuncCode === funcCode
          ? {
              ...item,
              HasRight: checked,
              CanView: checked,
              CanAdd: checked,
              CanEdit: checked,
              CanDelete: checked,
              CanPrint: checked,
            }
          : item
      )
    );
  };

  // Bật/Tắt toàn bộ 1 cột (Xem, Thêm, Sửa, Xóa, In) cho phân hệ đang lọc
  const handleToggleColumn = (
    action: 'CanView' | 'CanAdd' | 'CanEdit' | 'CanDelete' | 'CanPrint',
    checked: boolean
  ) => {
    setRights((prev) =>
      prev.map((item) => {
        if (categoryFilter !== 'ALL' && item.Parent?.toUpperCase() !== categoryFilter.toUpperCase()) {
          return item;
        }
        const updated = { ...item, [action]: checked };
        if (checked && action !== 'CanView') {
          updated.CanView = true;
          updated.HasRight = true;
        }
        if (action === 'CanView') {
          updated.HasRight = checked;
          if (!checked) {
            updated.CanAdd = false;
            updated.CanEdit = false;
            updated.CanDelete = false;
            updated.CanPrint = false;
          }
        }
        return updated;
      })
    );
  };

  // Cấp/Bỏ toàn bộ quyền của phân hệ đang chọn
  const handleSelectCurrentCategory = (check: boolean) => {
    setRights((prev) =>
      prev.map((item) => {
        if (categoryFilter === 'ALL' || item.Parent?.toUpperCase() === categoryFilter.toUpperCase()) {
          return {
            ...item,
            HasRight: check,
            CanView: check,
            CanAdd: check,
            CanEdit: check,
            CanDelete: check,
            CanPrint: check,
          };
        }
        return item;
      })
    );
  };

  // Cấp/Bỏ toàn bộ quyền hệ thống
  const handleSelectAll = (check: boolean) => {
    setRights((prev) =>
      prev.map((item) => ({
        ...item,
        HasRight: check,
        CanView: check,
        CanAdd: check,
        CanEdit: check,
        CanDelete: check,
        CanPrint: check,
      }))
    );
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const details = rights.map((r) => ({
        FunctionCode: r.FuncCode,
        CanView: r.CanView,
        CanAdd: r.CanAdd,
        CanEdit: r.CanEdit,
        CanDelete: r.CanDelete,
        CanPrint: r.CanPrint,
      }));

      const activeCodes = rights
        .filter((r) => r.CanView || r.CanAdd || r.CanEdit || r.CanDelete || r.CanPrint)
        .map((r) => r.FuncCode);

      await api.post(`/users/${userId}/rights`, {
        FunctionCodes: activeCodes,
        FuncCodes: activeCodes,
        Details: details,
      });

      message.success(`Đã cập nhật ma trận phân quyền cho [${username}] thành công!`);
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

  const totalActionsGranted = rights.reduce(
    (acc, r) =>
      acc +
      (r.CanView ? 1 : 0) +
      (r.CanAdd ? 1 : 0) +
      (r.CanEdit ? 1 : 0) +
      (r.CanDelete ? 1 : 0) +
      (r.CanPrint ? 1 : 0),
    0
  );

  const columns: ColumnsType<SysRightItemDTO> = [
    {
      title: 'Mã chức năng',
      dataIndex: 'FuncCode',
      key: 'FuncCode',
      width: 155,
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
      width: 140,
      render: (parent: string) => {
        const info = getParentLabel(parent);
        return <Tag color={info.color}>{info.label}</Tag>;
      },
    },
    {
      title: (
        <div style={{ textAlign: 'center' }}>
          <div><EyeOutlined style={{ color: '#1677ff' }} /> Xem</div>
          <Button
            type="link"
            size="small"
            style={{ fontSize: 11, padding: 0, height: 18 }}
            onClick={() => handleToggleColumn('CanView', true)}
          >
            Bật tất cả
          </Button>
        </div>
      ),
      dataIndex: 'CanView',
      key: 'CanView',
      width: 85,
      align: 'center',
      render: (canView: boolean, record: SysRightItemDTO) => (
        <Checkbox
          checked={canView}
          onChange={(e) => handleToggleAction(record.FuncCode, 'CanView', e.target.checked)}
        />
      ),
    },
    {
      title: (
        <div style={{ textAlign: 'center' }}>
          <div><PlusOutlined style={{ color: '#52c41a' }} /> Thêm</div>
          <Button
            type="link"
            size="small"
            style={{ fontSize: 11, padding: 0, height: 18 }}
            onClick={() => handleToggleColumn('CanAdd', true)}
          >
            Bật tất cả
          </Button>
        </div>
      ),
      dataIndex: 'CanAdd',
      key: 'CanAdd',
      width: 85,
      align: 'center',
      render: (canAdd: boolean, record: SysRightItemDTO) => (
        <Checkbox
          checked={canAdd}
          onChange={(e) => handleToggleAction(record.FuncCode, 'CanAdd', e.target.checked)}
        />
      ),
    },
    {
      title: (
        <div style={{ textAlign: 'center' }}>
          <div><EditOutlined style={{ color: '#fa8c16' }} /> Sửa</div>
          <Button
            type="link"
            size="small"
            style={{ fontSize: 11, padding: 0, height: 18 }}
            onClick={() => handleToggleColumn('CanEdit', true)}
          >
            Bật tất cả
          </Button>
        </div>
      ),
      dataIndex: 'CanEdit',
      key: 'CanEdit',
      width: 85,
      align: 'center',
      render: (canEdit: boolean, record: SysRightItemDTO) => (
        <Checkbox
          checked={canEdit}
          onChange={(e) => handleToggleAction(record.FuncCode, 'CanEdit', e.target.checked)}
        />
      ),
    },
    {
      title: (
        <div style={{ textAlign: 'center' }}>
          <div><DeleteOutlined style={{ color: '#ff4d4f' }} /> Xóa</div>
          <Button
            type="link"
            size="small"
            style={{ fontSize: 11, padding: 0, height: 18 }}
            onClick={() => handleToggleColumn('CanDelete', true)}
          >
            Bật tất cả
          </Button>
        </div>
      ),
      dataIndex: 'CanDelete',
      key: 'CanDelete',
      width: 85,
      align: 'center',
      render: (canDelete: boolean, record: SysRightItemDTO) => (
        <Checkbox
          checked={canDelete}
          onChange={(e) => handleToggleAction(record.FuncCode, 'CanDelete', e.target.checked)}
        />
      ),
    },
    {
      title: (
        <div style={{ textAlign: 'center' }}>
          <div><PrinterOutlined style={{ color: '#722ed1' }} /> In</div>
          <Button
            type="link"
            size="small"
            style={{ fontSize: 11, padding: 0, height: 18 }}
            onClick={() => handleToggleColumn('CanPrint', true)}
          >
            Bật tất cả
          </Button>
        </div>
      ),
      dataIndex: 'CanPrint',
      key: 'CanPrint',
      width: 85,
      align: 'center',
      render: (canPrint: boolean, record: SysRightItemDTO) => (
        <Checkbox
          checked={canPrint}
          onChange={(e) => handleToggleAction(record.FuncCode, 'CanPrint', e.target.checked)}
        />
      ),
    },
    {
      title: 'Toàn quyền dòng',
      key: 'RowAll',
      width: 115,
      align: 'center',
      render: (_: any, record: SysRightItemDTO) => {
        const isFull =
          record.CanView &&
          record.CanAdd &&
          record.CanEdit &&
          record.CanDelete &&
          record.CanPrint;
        return (
          <Checkbox
            checked={isFull}
            onChange={(e) => handleToggleRowAll(record.FuncCode, e.target.checked)}
          >
            {isFull ? <span style={{ color: '#52c41a', fontWeight: 600 }}>Đủ 5</span> : 'Cấp hết'}
          </Checkbox>
        );
      },
    },
  ];

  return (
    <Modal
      title={
        <Space>
          <KeyOutlined style={{ color: '#fa8c16', fontSize: 20 }} />
          <span>
            Ma trận phân quyền 5 thao tác cho {isGroup ? 'nhóm' : 'tài khoản'}:{' '}
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
      width="min(1060px, 95vw)"
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
          Lưu phân quyền ({totalActionsGranted} / {rights.length * 5} hành động được cấp)
        </Button>,
      ]}
    >
      <Alert
        message={
          <span>
            {isGroup ? (
              <>
                Phân quyền 5 hành động (<b>Xem - Thêm - Sửa - Xóa - In</b>) cho <b>Nhóm quyền</b> sẽ tự động áp dụng cho toàn bộ thành viên trong nhóm theo cấu trúc RBAC đồng bộ với WinForms.
              </>
            ) : (
              <>
                Phân quyền 5 hành động (<b>Xem - Thêm - Sửa - Xóa - In</b>) trực tiếp cho <b>Tài khoản</b>. Người dùng sẽ thừa hưởng quyền từ các nhóm trực thuộc cộng gộp với quyền chỉ định tại đây.
              </>
            )}
          </span>
        }
        type="info"
        showIcon
        style={{ marginBottom: 14, marginTop: 4 }}
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

      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 14, gap: 12, flexWrap: 'wrap' }}>
        <Input
          placeholder="Tìm kiếm mã hoặc tên chức năng..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          allowClear
          style={{ maxWidth: 320 }}
        />
        <Space wrap>
          <Tooltip title="Cấp toàn quyền cả 5 hành động cho toàn bộ chức năng trong nhóm đang chọn">
            <Button
              icon={<CheckSquareOutlined />}
              onClick={() => handleSelectCurrentCategory(true)}
              size="small"
            >
              Cấp hết nhóm này
            </Button>
          </Tooltip>
          <Tooltip title="Bỏ chọn tất cả hành động trong nhóm đang chọn">
            <Button
              icon={<BorderOutlined />}
              onClick={() => handleSelectCurrentCategory(false)}
              size="small"
            >
              Xóa trắng nhóm này
            </Button>
          </Tooltip>
          <Button
            type="dashed"
            icon={<SafetyCertificateOutlined />}
            onClick={() => handleSelectAll(true)}
            size="small"
            style={{ borderColor: '#52c41a', color: '#52c41a' }}
          >
            Cấp toàn quyền hệ thống ({rights.length * 5})
          </Button>
          <Button
            type="dashed"
            danger
            onClick={() => handleSelectAll(false)}
            size="small"
          >
            Bỏ toàn bộ ({rights.length * 5})
          </Button>
        </Space>
      </div>

      <Table
        columns={columns}
        dataSource={filteredData}
        rowKey="FuncCode"
        loading={loading}
        scroll={{ x: 'max-content' }}
        pagination={{ pageSize: 8, showTotal: (total) => `Hiển thị ${total} chức năng` }}
        size="small"
      />
    </Modal>
  );
};

export default PhanQuyenModal;
