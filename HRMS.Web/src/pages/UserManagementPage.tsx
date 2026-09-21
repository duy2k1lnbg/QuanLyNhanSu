import { useState, useEffect } from 'react';
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
  Popconfirm,
  notification,
  Typography,
  theme,
  Row,
  Col,
  Statistic,
  Select,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  UserOutlined,
  TeamOutlined,
  PlusOutlined,
  SearchOutlined,
  ReloadOutlined,
  SettingOutlined,
  EditOutlined,
  DeleteOutlined,
  SafetyCertificateOutlined,
  LinkOutlined,
  MobileOutlined,
  IdcardOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  UserAddOutlined,
  LockOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import PhanQuyenModal from '../components/PhanQuyenModal';
import GroupMembersModal from '../components/GroupMembersModal';
import UserEditModal from '../components/UserEditModal';
import UserEmployeeLinkModal from '../components/UserEmployeeLinkModal';
import BulkProvisioningModal from '../components/BulkProvisioningModal';
import UserDetailDrawer from '../components/UserDetailDrawer';
import type { SysUserDTO, CurrentUserDTO } from '../types/hrms';

const { Text } = Typography;

interface UserManagementPageProps {
  userList: SysUserDTO[];
  userLoading: boolean;
  currentUser?: CurrentUserDTO;
  onRefresh: () => void;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
}

export function UserManagementPage({
  userList,
  userLoading,
  onRefresh,
  canAdd,
  canEdit,
  canDelete,
}: UserManagementPageProps) {
  const [userTab, setUserTab] = useState<'users' | 'groups'>('users');
  const [userSearchText, setUserSearchText] = useState('');
  const [selectedUserForPerms, setSelectedUserForPerms] = useState<SysUserDTO | null>(null);
  const [phanQuyenModalVisible, setPhanQuyenModalVisible] = useState(false);
  const [selectedGroupForMembers, setSelectedGroupForMembers] = useState<SysUserDTO | null>(null);
  const [groupMembersModalVisible, setGroupMembersModalVisible] = useState(false);
  const [selectedUserForEdit, setSelectedUserForEdit] = useState<SysUserDTO | null>(null);
  const [userEditModalVisible, setUserEditModalVisible] = useState(false);
  const [selectedUserForLink, setSelectedUserForLink] = useState<SysUserDTO | null>(null);
  const [linkModalVisible, setLinkModalVisible] = useState(false);
  const [createUserModalVisible, setCreateUserModalVisible] = useState(false);
  const [isCreatingGroup, setIsCreatingGroup] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formCreateUser] = Form.useForm();

  // Bulk Provisioning & Detail Drawer states
  const [bulkModalVisible, setBulkModalVisible] = useState(false);
  const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
  const [selectedUserForDetail, setSelectedUserForDetail] = useState<SysUserDTO | null>(null);

  // Filters
  const [filterAccountStatus, setFilterAccountStatus] = useState<string>('all');
  const [filterMobileStatus, setFilterMobileStatus] = useState<string>('all');

  // Stats
  const [stats, setStats] = useState({
    TotalEmployees: 0,
    AccountsCreated: 0,
    EmployeesWithoutAccount: 0,
    MobileEnabled: 0,
    MobileDisabled: 0,
    LockedAccounts: 0,
    SystemAccounts: 0,
  });
  const [statsLoading, setStatsLoading] = useState(false);

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const fetchStats = async () => {
    try {
      setStatsLoading(true);
      const res = await api.get('/users/stats');
      if (res.data) {
        setStats(res.data);
      }
    } catch {
      // Keep default stats if fetch fails
    } finally {
      setStatsLoading(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

  const handleRefreshAll = () => {
    fetchStats();
    onRefresh();
  };

  const handleToggleLock = async (user: SysUserDTO) => {
    try {
      await api.post(`/users/${user.IdUser}/toggle-lock`);
      notification.success({
        message: 'Thành công',
        description: `Đã ${user.Disabled ? 'mở khóa' : 'khóa'} tài khoản [${user.Username}]!`,
      });
      handleRefreshAll();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể thay đổi trạng thái tài khoản.' });
    }
  };

  const handleToggleMobile = async (user: SysUserDTO) => {
    if (user.IsAdmin || user.Username?.toUpperCase() === 'ADMIN') {
      notification.warning({
        message: 'Không áp dụng',
        description: 'Tài khoản Quản trị viên tối cao (ADMIN) là tài khoản hệ thống, không áp dụng quyền truy cập ứng dụng di động.',
      });
      return;
    }
    try {
      const current = user.IsMobileEnabled !== undefined ? user.IsMobileEnabled : (user.ClientType !== 'DESKTOP');
      const targetState = !current;
      await api.post(`/users/${user.IdUser}/toggle-mobile`, {
        IsMobileEnabled: targetState,
      });
      notification.success({
        message: 'Thành công',
        description: `Đã ${targetState ? 'kích hoạt' : 'tắt'} Mobile Access cho [${user.Username}]!`,
      });
      handleRefreshAll();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể thay đổi quyền Mobile Access.' });
    }
  };

  const handleDeleteUser = async (user: SysUserDTO) => {
    try {
      await api.delete(`/users/${user.IdUser}`);
      notification.success({
        message: 'Thành công',
        description: `Đã xóa ${user.IsGroup ? 'nhóm' : 'tài khoản'} [${user.Username}]!`,
      });
      handleRefreshAll();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi khi xóa.' });
    }
  };

  const handleCreateUser = async () => {
    try {
      const values = await formCreateUser.validateFields();
      setSaving(true);
      await api.post('/users', {
        Username: values.Username,
        FullName: values.FullName,
        Password: values.Password,
        IsGroup: isCreatingGroup,
        MaCty: values.MaCty || 'CTY01',
        MaDvi: values.MaDvi || 'DVI01',
      });
      notification.success({
        message: 'Thành công',
        description: `Đã tạo ${isCreatingGroup ? 'nhóm quyền' : 'tài khoản'} [${values.Username}] thành công!`,
      });
      setCreateUserModalVisible(false);
      formCreateUser.resetFields();
      handleRefreshAll();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi khi tạo mới.' });
    } finally {
      setSaving(false);
    }
  };

  const safeUserList = Array.isArray(userList) ? userList : [];
  const rawList = safeUserList.filter((u) => (userTab === 'users' ? !u.IsGroup : u.IsGroup));
  const filteredUsers = rawList.filter((u) => {
    if (userSearchText) {
      const kw = userSearchText.toLowerCase();
      const matchUser = Boolean(u.Username && u.Username.toLowerCase().includes(kw));
      const matchName = Boolean(u.FullName && u.FullName.toLowerCase().includes(kw));
      const code = u.EmployeeCode || u.employeeCode;
      const matchCode = Boolean(code && code.toLowerCase().includes(kw));
      if (!matchUser && !matchName && !matchCode) return false;
    }

    if (userTab === 'users') {
      // Account link filter
      if (filterAccountStatus === 'linked') {
        if (!u.Manv && !u.manv) return false;
      } else if (filterAccountStatus === 'unlinked') {
        if (u.Manv || u.manv) return false;
      } else if (filterAccountStatus === 'locked') {
        if (!u.Disabled) return false;
      } else if (filterAccountStatus === 'system') {
        if (!u.IsAdmin && u.Username?.toUpperCase() !== 'ADMIN') return false;
      }

      // Mobile filter
      if (filterMobileStatus === 'enabled') {
        const isEnabled = u.IsMobileEnabled !== undefined ? u.IsMobileEnabled : (u.ClientType !== 'DESKTOP');
        if (!isEnabled || u.IsAdmin || u.Username?.toUpperCase() === 'ADMIN') return false;
      } else if (filterMobileStatus === 'disabled') {
        const isEnabled = u.IsMobileEnabled !== undefined ? u.IsMobileEnabled : (u.ClientType !== 'DESKTOP');
        if (isEnabled || u.IsAdmin || u.Username?.toUpperCase() === 'ADMIN') return false;
      } else if (filterMobileStatus === 'blocked') {
        if (!u.IsAdmin && u.Username?.toUpperCase() === 'ADMIN') return false;
      }
    }

    return true;
  });

  const userColumns: ColumnsType<SysUserDTO> = [
    {
      title: 'ID',
      dataIndex: 'IdUser',
      key: 'IdUser',
      width: 70,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Tài khoản',
      dataIndex: 'Username',
      key: 'Username',
      width: 160,
      render: (u: string, record: SysUserDTO) => (
        <Space direction="vertical" size={2}>
          <Space>
            <UserOutlined style={{ color: '#1677ff' }} />
            <Text strong style={{ color: '#1677ff' }}>{u}</Text>
          </Space>
          {record.IsAdmin && (
            <Tag color="red" icon={<SafetyCertificateOutlined />} style={{ fontSize: 11 }}>
              Super Admin
            </Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Họ và tên',
      dataIndex: 'FullName',
      key: 'FullName',
      render: (fn: string) => fn || 'Chưa cập nhật',
    },
    {
      title: 'Nhóm quyền trực thuộc',
      dataIndex: 'Groups',
      key: 'Groups',
      render: (groups?: string[]) =>
        groups && groups.length > 0 ? (
          <Space wrap size={[4, 4]}>
            {groups.map((g, idx) => (
              <Tag color="purple" key={idx} icon={<TeamOutlined />}>
                {g}
              </Tag>
            ))}
          </Space>
        ) : (
          <Text type="secondary" italic>Chưa gán nhóm</Text>
        ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'Disabled',
      key: 'Disabled',
      width: 130,
      render: (disabled?: boolean) =>
        disabled ? (
          <Tag color="red">Bị tạm khóa</Tag>
        ) : (
          <Tag color="green">Đang hoạt động</Tag>
        ),
    },
    {
      title: 'Hồ sơ liên kết (1:1)',
      key: 'linkedEmployee',
      width: 220,
      render: (_, record: SysUserDTO) => {
        const isRootAdmin = Boolean(record.IsAdmin || record.Username?.toUpperCase() === 'ADMIN');
        if (isRootAdmin) {
          return (
            <Tag color="default" style={{ fontStyle: 'italic', fontSize: 12 }}>
              Không áp dụng (Admin hệ thống)
            </Tag>
          );
        }

        const manv = record.Manv || record.manv;
        const code = record.EmployeeCode || record.employeeCode;
        const name = record.EmployeeName || record.employeeName;

        if (!manv) {
          return (
            <Tag color="default" style={{ fontSize: 12 }}>
              Chưa liên kết
            </Tag>
          );
        }

        return (
          <Space direction="vertical" size={1}>
            <Space size={4}>
              <IdcardOutlined style={{ color: '#52c41a' }} />
              <Text strong style={{ color: '#389e0d' }}>
                {code || `NV#${manv}`}
              </Text>
            </Space>
            {name && <Text type="secondary" style={{ fontSize: 12 }}>{name}</Text>}
          </Space>
        );
      },
    },
    {
      title: 'Mobile Access',
      key: 'mobileAccess',
      width: 140,
      render: (_, record: SysUserDTO) => {
        const isRootAdmin = Boolean(record.IsAdmin || record.Username?.toUpperCase() === 'ADMIN');
        if (isRootAdmin) {
          return (
            <Tag color="default" style={{ fontStyle: 'italic', fontSize: 12 }}>
              Không áp dụng
            </Tag>
          );
        }

        const isEnabled = record.IsMobileEnabled !== undefined ? record.IsMobileEnabled : (record.ClientType !== 'DESKTOP');
        const hasLinked = Boolean(record.Manv || record.manv);

        return (
          <Space>
            <Tag
              color={isEnabled && hasLinked ? 'cyan' : isEnabled ? 'blue' : 'default'}
              icon={<MobileOutlined />}
            >
              {isEnabled ? 'Bật' : 'Tắt'}
            </Tag>
            {(!canEdit || canEdit('F_SYSTEM_USER')) && (
              <Button
                type="link"
                size="small"
                onClick={() => handleToggleMobile(record)}
                style={{ fontSize: 12, padding: 0 }}
              >
                Đổi
              </Button>
            )}
          </Space>
        );
      },
    },
    {
      title: 'Thao tác quản trị',
      key: 'actions',
      width: 380,
      render: (_, record) => {
        const isRootAdmin = Boolean(record.IsAdmin || record.Username?.toUpperCase() === 'ADMIN');

        if (isRootAdmin) {
          return (
            <Space size="small">
              <Tag color="gold" icon={<SafetyCertificateOutlined />} style={{ padding: '3px 10px', fontWeight: 600, fontSize: 12 }}>
                Toàn quyền hệ thống
              </Tag>
              <Button
                size="small"
                icon={<EyeOutlined />}
                onClick={() => {
                  setSelectedUserForDetail(record);
                  setDetailDrawerVisible(true);
                }}
              >
                Chi tiết
              </Button>
            </Space>
          );
        }

        return (
          <Space size="small" wrap>
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => {
                setSelectedUserForDetail(record);
                setDetailDrawerVisible(true);
              }}
            >
              Chi tiết
            </Button>
            {(!canEdit || canEdit('F_SYSTEM_USER')) && (
              <Button
                size="small"
                icon={<LinkOutlined />}
                style={{ borderColor: '#1890ff', color: '#1890ff' }}
                onClick={() => {
                  setSelectedUserForLink(record);
                  setLinkModalVisible(true);
                }}
              >
                Liên kết NV
              </Button>
            )}
            {(!canEdit || canEdit('F_SYSTEM_USER', 'PHANQUYEN')) && (
              <Button
                size="small"
                icon={<SettingOutlined />}
                style={{ borderColor: '#fa8c16', color: '#fa8c16' }}
                onClick={() => {
                  setSelectedUserForPerms(record);
                  setPhanQuyenModalVisible(true);
                }}
              >
                Phân quyền
              </Button>
            )}
            {(!canEdit || canEdit('F_SYSTEM_USER')) && (
              <Button
                size="small"
                icon={<EditOutlined />}
                onClick={() => {
                  setSelectedUserForEdit(record);
                  setUserEditModalVisible(true);
                }}
              >
                Sửa
              </Button>
            )}
            {(!canEdit || canEdit('F_SYSTEM_USER')) && (
              <Popconfirm
                title={record.Disabled ? 'Mở khóa tài khoản?' : 'Khóa tài khoản?'}
                description={`Bạn có chắc muốn ${record.Disabled ? 'mở khóa' : 'tạm khóa'} tài khoản [${record.Username}]?`}
                onConfirm={() => handleToggleLock(record)}
                okText="Đồng ý"
                cancelText="Hủy"
              >
                <Button size="small" danger={!record.Disabled}>
                  {record.Disabled ? 'Mở khóa' : 'Khóa'}
                </Button>
              </Popconfirm>
            )}
            {(!canDelete || canDelete('F_SYSTEM_USER')) && (
              <Popconfirm
                title="Xóa tài khoản?"
                description={`Bạn có chắc chắn muốn xóa tài khoản [${record.Username}] không?`}
                onConfirm={() => handleDeleteUser(record)}
                okText="Xóa"
                cancelText="Hủy"
              >
                <Button size="small" danger icon={<DeleteOutlined />} />
              </Popconfirm>
            )}
          </Space>
        );
      },
    },
  ];

  const groupColumns: ColumnsType<SysUserDTO> = [
    {
      title: 'ID Nhóm',
      dataIndex: 'IdUser',
      key: 'IdUser',
      width: 80,
      render: (id: number) => <Tag color="purple">#{id}</Tag>,
    },
    {
      title: 'Mã nhóm',
      dataIndex: 'Username',
      key: 'Username',
      width: 180,
      render: (u: string) => (
        <Space>
          <TeamOutlined style={{ color: '#722ed1' }} />
          <Text strong style={{ color: '#722ed1' }}>{u}</Text>
        </Space>
      ),
    },
    {
      title: 'Tên nhóm quyền',
      dataIndex: 'FullName',
      key: 'FullName',
      render: (fn: string) => fn || 'Chưa đặt tên',
    },
    {
      title: 'Số thành viên',
      dataIndex: 'MemberCount',
      key: 'MemberCount',
      width: 120,
      render: (cnt: number) => <Tag color="geekblue">{cnt || 0} người</Tag>,
    },
    {
      title: 'Thao tác quản trị nhóm',
      key: 'actions',
      width: 320,
      render: (_, record) => (
        <Space size="small" wrap>
          {(!canEdit || canEdit('F_SYSTEM_GROUP', 'PHANQUYEN')) && (
            <Button
              size="small"
              icon={<SettingOutlined />}
              style={{ borderColor: '#fa8c16', color: '#fa8c16' }}
              onClick={() => {
                setSelectedUserForPerms(record);
                setPhanQuyenModalVisible(true);
              }}
            >
              Phân quyền nhóm
            </Button>
          )}
          {(!canEdit || canEdit('F_SYSTEM_GROUP')) && (
            <Button
              size="small"
              type="primary"
              ghost
              icon={<TeamOutlined />}
              onClick={() => {
                setSelectedGroupForMembers(record);
                setGroupMembersModalVisible(true);
              }}
            >
              Thành viên ({record.MemberCount || 0})
            </Button>
          )}
          {(!canEdit || canEdit('F_SYSTEM_GROUP')) && (
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => {
                setSelectedUserForEdit(record);
                setUserEditModalVisible(true);
              }}
            >
              Sửa
            </Button>
          )}
          {(!canDelete || canDelete('F_SYSTEM_GROUP')) && (
            <Popconfirm
              title="Xóa nhóm quyền?"
              description={`Bạn có chắc muốn xóa nhóm [${record.Username}] không? Toàn bộ liên kết thành viên và quyền hạn liên quan sẽ bị xóa.`}
              onConfirm={() => handleDeleteUser(record)}
              okText="Xóa"
              cancelText="Hủy"
            >
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <>
      {/* Dashboard KPI Summary Cards */}
      <Row gutter={[12, 12]} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={8} md={6} lg={3}>
          <Card size="small" style={{ borderRadius: 8, background: '#fafafa' }} loading={statsLoading}>
            <Statistic
              title="Tổng nhân sự"
              value={stats.TotalEmployees}
              valueStyle={{ color: '#1890ff', fontWeight: 700 }}
              prefix={<TeamOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={3}>
          <Card size="small" style={{ borderRadius: 8, background: '#f6ffed' }} loading={statsLoading}>
            <Statistic
              title="Đã có tài khoản"
              value={stats.AccountsCreated}
              valueStyle={{ color: '#52c41a', fontWeight: 700 }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fff7e6' }} loading={statsLoading}>
            <Statistic
              title="Chưa có tài khoản"
              value={stats.EmployeesWithoutAccount}
              valueStyle={{ color: '#fa8c16', fontWeight: 700 }}
              prefix={<UserAddOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={3}>
          <Card size="small" style={{ borderRadius: 8, background: '#e6f7ff' }} loading={statsLoading}>
            <Statistic
              title="Mobile Bật"
              value={stats.MobileEnabled}
              valueStyle={{ color: '#13c2c2', fontWeight: 700 }}
              prefix={<MobileOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={3}>
          <Card size="small" style={{ borderRadius: 8, background: '#f5f5f5' }} loading={statsLoading}>
            <Statistic
              title="Mobile Tắt"
              value={stats.MobileDisabled}
              valueStyle={{ color: '#8c8c8c', fontWeight: 700 }}
              prefix={<MobileOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fff1f0' }} loading={statsLoading}>
            <Statistic
              title="Tài khoản bị khóa"
              value={stats.LockedAccounts}
              valueStyle={{ color: '#f5222d', fontWeight: 700 }}
              prefix={<LockOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fffbe6' }} loading={statsLoading}>
            <Statistic
              title="Tài khoản hệ thống"
              value={stats.SystemAccounts}
              valueStyle={{ color: '#faad14', fontWeight: 700 }}
              prefix={<SafetyCertificateOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card
        title="🔐 Quản trị Người dùng & Phân quyền Hệ thống"
        extra={
          <Space wrap>
            {userTab === 'users' && (!canAdd || canAdd('F_SYSTEM_USER')) && (
              <Button
                type="primary"
                style={{ background: '#722ed1', borderColor: '#722ed1' }}
                icon={<ThunderboltOutlined />}
                onClick={() => setBulkModalVisible(true)}
              >
                Cấp tài khoản Mobile hàng loạt
              </Button>
            )}
            {(!canAdd || (userTab === 'users' ? canAdd('F_SYSTEM_USER') : canAdd('F_SYSTEM_GROUP'))) && (
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  setIsCreatingGroup(userTab === 'groups');
                  formCreateUser.resetFields();
                  setCreateUserModalVisible(true);
                }}
              >
                {userTab === 'users' ? 'Thêm Người Dùng' : 'Thêm Nhóm Quyền'}
              </Button>
            )}
            <Button icon={<ReloadOutlined />} onClick={handleRefreshAll}>
              Làm mới
            </Button>
          </Space>
        }
        bordered={false}
        style={{ borderRadius: borderRadiusLG }}
      >
        <Tabs
          activeKey={userTab}
          onChange={(k) => {
            setUserTab(k as 'users' | 'groups');
            setUserSearchText('');
          }}
          items={[
            {
              key: 'users',
              label: (
                <span>
                  <UserOutlined /> Người dùng cá nhân ({safeUserList.filter((u) => !u.IsGroup).length})
                </span>
              ),
              children: (
                <>
                  {/* Filter Toolbar */}
                  <Row gutter={[12, 12]} style={{ marginBottom: 16 }} align="middle">
                    <Col xs={24} sm={10} md={8}>
                      <Input
                        placeholder="Tìm tài khoản, họ tên, mã NV..."
                        prefix={<SearchOutlined />}
                        value={userSearchText}
                        onChange={(e) => setUserSearchText(e.target.value)}
                        allowClear
                      />
                    </Col>
                    <Col xs={12} sm={7} md={6}>
                      <Select
                        style={{ width: '100%' }}
                        value={filterAccountStatus}
                        onChange={setFilterAccountStatus}
                        options={[
                          { value: 'all', label: 'Tất cả trạng thái hồ sơ' },
                          { value: 'linked', label: 'Đã liên kết nhân sự' },
                          { value: 'unlinked', label: 'Chưa liên kết nhân sự' },
                          { value: 'locked', label: 'Đang bị tạm khóa' },
                          { value: 'system', label: 'Tài khoản hệ thống (Admin)' },
                        ]}
                      />
                    </Col>
                    <Col xs={12} sm={7} md={6}>
                      <Select
                        style={{ width: '100%' }}
                        value={filterMobileStatus}
                        onChange={setFilterMobileStatus}
                        options={[
                          { value: 'all', label: 'Tất cả Mobile Access' },
                          { value: 'enabled', label: 'Mobile: Đã bật' },
                          { value: 'disabled', label: 'Mobile: Đang tắt' },
                          { value: 'blocked', label: 'Mobile: Bị chặn (Admin)' },
                        ]}
                      />
                    </Col>
                  </Row>

                  <Table
                    columns={userColumns}
                    dataSource={filteredUsers}
                    rowKey="IdUser"
                    loading={userLoading}
                    scroll={{ x: 'max-content' }}
                    pagination={{ pageSize: 8, showTotal: (t) => `Tổng số ${t} người dùng` }}
                  />
                </>
              ),
            },
            {
              key: 'groups',
              label: (
                <span>
                  <TeamOutlined /> Nhóm quyền hạn ({safeUserList.filter((u) => u.IsGroup).length})
                </span>
              ),
              children: (
                <>
                  <div style={{ marginBottom: 16 }}>
                    <Input
                      placeholder="Tìm mã nhóm, tên nhóm..."
                      prefix={<SearchOutlined />}
                      value={userSearchText}
                      onChange={(e) => setUserSearchText(e.target.value)}
                      allowClear
                      style={{ width: 280 }}
                    />
                  </div>
                  <Table
                    columns={groupColumns}
                    dataSource={filteredUsers}
                    rowKey="IdUser"
                    loading={userLoading}
                    scroll={{ x: 'max-content' }}
                    pagination={{ pageSize: 8, showTotal: (t) => `Tổng số ${t} nhóm quyền` }}
                  />
                </>
              ),
            },
          ]}
        />
      </Card>

      {/* Modal Tạo người dùng / Nhóm mới */}
      <Modal
        title={isCreatingGroup ? 'Tạo Nhóm Quyền Mới' : 'Tạo Tài Khoản Người Dùng Mới'}
        open={createUserModalVisible}
        onCancel={() => setCreateUserModalVisible(false)}
        onOk={handleCreateUser}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={formCreateUser} layout="vertical">
          <Form.Item
            name="Username"
            label={isCreatingGroup ? 'Mã Nhóm (Ví dụ: G_NHANSU)' : 'Tên đăng nhập (Username)'}
            rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập/mã nhóm' }]}
          >
            <Input placeholder={isCreatingGroup ? 'G_KETOAN' : 'nguyenvana'} />
          </Form.Item>
          <Form.Item
            name="FullName"
            label={isCreatingGroup ? 'Tên Nhóm quyền' : 'Họ và tên đầy đủ'}
            rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
          >
            <Input placeholder={isCreatingGroup ? 'Nhóm Kế Toán & Tiền Lương' : 'Nguyễn Văn A'} />
          </Form.Item>
          {!isCreatingGroup && (
            <Form.Item
              name="Password"
              label="Mật khẩu khởi tạo"
              rules={[{ required: true, message: 'Vui lòng nhập mật khẩu' }]}
            >
              <Input.Password placeholder="Nhập mật khẩu an toàn..." />
            </Form.Item>
          )}
        </Form>
      </Modal>

      {/* Phân quyền Modal */}
      {selectedUserForPerms && (
        <PhanQuyenModal
          visible={phanQuyenModalVisible}
          onClose={() => {
            setPhanQuyenModalVisible(false);
            setSelectedUserForPerms(null);
          }}
          userId={selectedUserForPerms.IdUser}
          username={selectedUserForPerms.Username}
          fullName={selectedUserForPerms.FullName || selectedUserForPerms.Username}
          isGroup={selectedUserForPerms.IsGroup}
        />
      )}

      {/* Thành viên nhóm Modal */}
      {selectedGroupForMembers && (
        <GroupMembersModal
          visible={groupMembersModalVisible}
          onClose={() => {
            setGroupMembersModalVisible(false);
            setSelectedGroupForMembers(null);
            handleRefreshAll();
          }}
          groupId={selectedGroupForMembers.IdUser}
          groupName={selectedGroupForMembers.Username}
          groupFullName={selectedGroupForMembers.FullName || selectedGroupForMembers.Username}
        />
      )}

      {/* Sửa thông tin User/Group Modal */}
      {selectedUserForEdit && (
        <UserEditModal
          visible={userEditModalVisible}
          onClose={() => {
            setUserEditModalVisible(false);
            setSelectedUserForEdit(null);
          }}
          user={selectedUserForEdit}
          onSuccess={handleRefreshAll}
        />
      )}

      {/* Liên kết User ↔ Employee Mapping Modal */}
      {selectedUserForLink && (
        <UserEmployeeLinkModal
          visible={linkModalVisible}
          onClose={() => {
            setLinkModalVisible(false);
            setSelectedUserForLink(null);
          }}
          user={selectedUserForLink}
          onSuccess={handleRefreshAll}
        />
      )}

      {/* Bulk Employee Account Provisioning Modal */}
      <BulkProvisioningModal
        visible={bulkModalVisible}
        onClose={() => setBulkModalVisible(false)}
        onSuccess={handleRefreshAll}
      />

      {/* User Account Detail Drawer */}
      <UserDetailDrawer
        visible={detailDrawerVisible}
        onClose={() => {
          setDetailDrawerVisible(false);
          setSelectedUserForDetail(null);
        }}
        user={selectedUserForDetail}
        onRefresh={handleRefreshAll}
        canEdit={canEdit}
      />
    </>
  );
}

export default UserManagementPage;

