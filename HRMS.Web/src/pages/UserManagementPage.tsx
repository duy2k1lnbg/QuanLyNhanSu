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
import { useAppLanguage } from '../services/i18n';

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
  const { t } = useAppLanguage();
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
      // Keep default stats
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
        message: t('common.success'),
        description: t('common.updateSuccess'),
      });
      handleRefreshAll();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.saveError') });
    }
  };

  const handleToggleMobile = async (user: SysUserDTO) => {
    if (user.IsAdmin || user.Username?.toUpperCase() === 'ADMIN') {
      notification.warning({
        message: t('common.warning'),
        description: 'ADMIN',
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
        message: t('common.success'),
        description: t('common.updateSuccess'),
      });
      handleRefreshAll();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.saveError') });
    }
  };

  const handleDeleteUser = async (user: SysUserDTO) => {
    try {
      await api.delete(`/users/${user.IdUser}`);
      notification.success({
        message: t('common.success'),
        description: t('common.deleteSuccess'),
      });
      handleRefreshAll();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.deleteError') });
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
        message: t('common.success'),
        description: t('common.saveSuccess'),
      });
      setCreateUserModalVisible(false);
      handleRefreshAll();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.saveError') });
    } finally {
      setSaving(false);
    }
  };

  const safeUserList = Array.isArray(userList) ? userList : [];

  const filteredUsers = safeUserList.filter((u) => {
    if (userTab === 'users' && u.IsGroup) return false;
    if (userTab === 'groups' && !u.IsGroup) return false;

    if (userTab === 'users') {
      if (filterAccountStatus === 'linked' && (!u.Manv && !u.manv)) return false;
      if (filterAccountStatus === 'unlinked' && (u.Manv || u.manv || u.IsAdmin || u.Username?.toUpperCase() === 'ADMIN')) return false;
      if (filterAccountStatus === 'locked' && !u.Disabled) return false;
      if (filterAccountStatus === 'system' && !u.IsAdmin && u.Username?.toUpperCase() !== 'ADMIN') return false;

      const isEnabled = u.IsMobileEnabled !== undefined ? u.IsMobileEnabled : (u.ClientType !== 'DESKTOP');
      if (filterMobileStatus === 'enabled' && !isEnabled) return false;
      if (filterMobileStatus === 'disabled' && isEnabled) return false;
    }

    if (!userSearchText.trim()) return true;
    const q = userSearchText.toLowerCase();
    const matchU = u.Username?.toLowerCase().includes(q);
    const matchN = u.FullName?.toLowerCase().includes(q);
    const matchE = (u.Manv || u.manv)?.toString().includes(q);
    const matchD = u.TenPb?.toLowerCase().includes(q);
    return matchU || matchN || matchE || matchD;
  });

  const userColumns: ColumnsType<SysUserDTO> = [
    {
      title: t('user.colUsername'),
      dataIndex: 'Username',
      key: 'Username',
      width: 140,
      render: (u: string, record) => (
        <Space>
          <UserOutlined style={{ color: record.IsAdmin ? '#f5222d' : '#1890ff' }} />
          <Text strong style={{ color: record.IsAdmin ? '#cf1322' : '#1890ff' }}>{u}</Text>
          {record.IsAdmin && <Tag color="red" style={{ fontSize: 10 }}>ADMIN</Tag>}
        </Space>
      ),
    },
    {
      title: t('user.colFullName'),
      dataIndex: 'FullName',
      key: 'FullName',
      width: 170,
      render: (name: string, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{name || record.Username}</Text>
          {record.TenCv && <Text type="secondary" style={{ fontSize: 11 }}>{record.TenCv}</Text>}
        </Space>
      ),
    },
    {
      title: t('user.colEmployeeCode'),
      key: 'linkedEmployee',
      width: 170,
      render: (_, record) => {
        const manv = record.Manv || record.manv;
        const isRootAdmin = Boolean(record.IsAdmin || record.Username?.toUpperCase() === 'ADMIN');

        if (isRootAdmin) {
          return (
            <Tag color="gold" icon={<SafetyCertificateOutlined />}>
              ADMIN
            </Tag>
          );
        }

        if (manv) {
          return (
            <Space direction="vertical" size={0}>
              <Space>
                <Tag color="cyan" icon={<IdcardOutlined />}>#{manv}</Tag>
                <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 13 }} />
              </Space>
              {record.TenPb && <Text type="secondary" style={{ fontSize: 11 }}>{record.TenPb}</Text>}
            </Space>
          );
        }

        return (
          <Tag color="default">
            {t('status.draft')}
          </Tag>
        );
      },
    },
    {
      title: t('user.colStatus'),
      key: 'status',
      width: 110,
      render: (_, record) => {
        if (record.Disabled) {
          return <Tag color="error">{t('status.locked')}</Tag>;
        }
        return <Tag color="success">{t('status.active')}</Tag>;
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
              -
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
              {isEnabled ? t('common.yes') : t('common.no')}
            </Tag>
            {(!canEdit || canEdit('F_SYSTEM_USER')) && (
              <Button
                type="link"
                size="small"
                onClick={() => handleToggleMobile(record)}
                style={{ fontSize: 12, padding: 0 }}
              >
                {t('common.edit')}
              </Button>
            )}
          </Space>
        );
      },
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 320,
      fixed: 'right',
      render: (_, record) => {
        const isRootAdmin = Boolean(record.IsAdmin || record.Username?.toUpperCase() === 'ADMIN');

        if (isRootAdmin) {
          return (
            <Space size="small">
              <Tag color="gold" icon={<SafetyCertificateOutlined />} style={{ padding: '3px 10px', fontWeight: 600, fontSize: 12 }}>
                SUPER ADMIN
              </Tag>
              <Button
                size="small"
                icon={<EyeOutlined />}
                onClick={() => {
                  setSelectedUserForDetail(record);
                  setDetailDrawerVisible(true);
                }}
              >
                {t('common.view')}
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
              {t('common.view')}
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
                {t('user.colEmployeeCode')}
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
                {t('user.btnPermissions')}
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
                {t('common.edit')}
              </Button>
            )}
            {(!canEdit || canEdit('F_SYSTEM_USER')) && (
              <Popconfirm
                title={record.Disabled ? t('user.btnUnlock') : t('user.btnLock')}
                description={`${record.Disabled ? t('user.btnUnlock') : t('user.btnLock')} [${record.Username}]?`}
                onConfirm={() => handleToggleLock(record)}
                okText={t('common.confirm')}
                cancelText={t('common.cancel')}
              >
                <Button size="small" danger={!record.Disabled}>
                  {record.Disabled ? t('user.btnUnlock') : t('user.btnLock')}
                </Button>
              </Popconfirm>
            )}
            {(!canDelete || canDelete('F_SYSTEM_USER')) && (
              <Popconfirm
                title={t('common.confirmDeleteTitle')}
                description={`${t('common.delete')}: [${record.Username}]?`}
                onConfirm={() => handleDeleteUser(record)}
                okText={t('common.confirm')}
                cancelText={t('common.cancel')}
                okButtonProps={{ danger: true }}
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
      title: 'ID',
      dataIndex: 'IdUser',
      key: 'IdUser',
      width: 80,
      render: (id: number) => <Tag color="purple">#{id}</Tag>,
    },
    {
      title: t('user.colUsername'),
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
      title: t('user.colRoles'),
      dataIndex: 'FullName',
      key: 'FullName',
      render: (fn: string) => fn || '-',
    },
    {
      title: t('user.colRoles'),
      dataIndex: 'MemberCount',
      key: 'MemberCount',
      width: 120,
      render: (cnt: number) => <Tag color="geekblue">{cnt || 0}</Tag>,
    },
    {
      title: t('common.actions'),
      key: 'actions',
      width: 280,
      fixed: 'right',
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
              {t('user.btnPermissions')}
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
              ({record.MemberCount || 0})
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
              {t('common.edit')}
            </Button>
          )}
          {(!canDelete || canDelete('F_SYSTEM_GROUP')) && (
            <Popconfirm
              title={t('common.confirmDeleteTitle')}
              description={`${t('common.delete')}: [${record.Username}]?`}
              onConfirm={() => handleDeleteUser(record)}
              okText={t('common.confirm')}
              cancelText={t('common.cancel')}
              okButtonProps={{ danger: true }}
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
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fafafa' }} loading={statsLoading}>
            <Statistic
              title={t('dashboard.statTotalEmployees')}
              value={stats.TotalEmployees}
              valueStyle={{ fontWeight: 700 }}
              prefix={<UserOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#f6ffed' }} loading={statsLoading}>
            <Statistic
              title={t('user.tabUsers', { count: '' })}
              value={stats.AccountsCreated}
              valueStyle={{ color: '#52c41a', fontWeight: 700 }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fff2e8' }} loading={statsLoading}>
            <Statistic
              title={t('common.noData')}
              value={stats.EmployeesWithoutAccount}
              valueStyle={{ color: '#fa541c', fontWeight: 700 }}
              prefix={<ThunderboltOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#e6f7ff' }} loading={statsLoading}>
            <Statistic
              title="Mobile Enabled"
              value={stats.MobileEnabled}
              valueStyle={{ color: '#1890ff', fontWeight: 700 }}
              prefix={<MobileOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fff1f0' }} loading={statsLoading}>
            <Statistic
              title={t('status.locked')}
              value={stats.LockedAccounts}
              valueStyle={{ color: '#f5222d', fontWeight: 700 }}
              prefix={<LockOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card size="small" style={{ borderRadius: 8, background: '#fffbe6' }} loading={statsLoading}>
            <Statistic
              title="Super Admin"
              value={stats.SystemAccounts}
              valueStyle={{ color: '#faad14', fontWeight: 700 }}
              prefix={<SafetyCertificateOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card
        title={t('user.pageTitle')}
        extra={
          <Space wrap>
            {userTab === 'users' && (!canAdd || canAdd('F_SYSTEM_USER')) && (
              <Button
                type="primary"
                style={{ background: '#722ed1', borderColor: '#722ed1' }}
                icon={<ThunderboltOutlined />}
                onClick={() => setBulkModalVisible(true)}
              >
                {t('user.btnBulkProvision')}
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
                {userTab === 'users' ? t('user.btnAddUser') : t('common.add')}
              </Button>
            )}
            <Button icon={<ReloadOutlined />} onClick={handleRefreshAll}>
              {t('common.refresh')}
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
                  <UserOutlined /> {t('user.tabUsers', { count: safeUserList.filter((u) => !u.IsGroup).length })}
                </span>
              ),
              children: (
                <>
                  <Row gutter={[12, 12]} style={{ marginBottom: 16 }} align="middle">
                    <Col xs={24} sm={10} md={8}>
                      <Input
                        placeholder={t('common.searchPlaceholder')}
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
                          { value: 'all', label: t('common.all') },
                          { value: 'linked', label: t('employee.colStatus') },
                          { value: 'unlinked', label: t('status.draft') },
                          { value: 'locked', label: t('status.locked') },
                          { value: 'system', label: 'Admin' },
                        ]}
                      />
                    </Col>
                    <Col xs={12} sm={7} md={6}>
                      <Select
                        style={{ width: '100%' }}
                        value={filterMobileStatus}
                        onChange={setFilterMobileStatus}
                        options={[
                          { value: 'all', label: 'Mobile Access (All)' },
                          { value: 'enabled', label: 'Mobile (On)' },
                          { value: 'disabled', label: 'Mobile (Off)' },
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
                    pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                  />
                </>
              ),
            },
            {
              key: 'groups',
              label: (
                <span>
                  <TeamOutlined /> {t('user.tabRoles')} ({safeUserList.filter((u) => u.IsGroup).length})
                </span>
              ),
              children: (
                <Table
                  columns={groupColumns}
                  dataSource={filteredUsers}
                  rowKey="IdUser"
                  loading={userLoading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 15, showTotal: (tVal) => t('common.totalRecords', { total: tVal }) }}
                />
              ),
            },
          ]}
        />
      </Card>

      {/* Tạo Người Dùng / Nhóm Quyền Modal */}
      <Modal
        title={isCreatingGroup ? t('common.create') : t('user.btnAddUser')}
        open={createUserModalVisible}
        onCancel={() => setCreateUserModalVisible(false)}
        onOk={handleCreateUser}
        confirmLoading={saving}
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width="min(500px, 95vw)"
      >
        <Form form={formCreateUser} layout="vertical">
          <Form.Item
            name="Username"
            label={isCreatingGroup ? t('common.description') : t('auth.usernameLabel')}
            rules={[{ required: true, message: t('auth.usernameRequired') }]}
          >
            <Input placeholder={t('auth.usernamePlaceholder')} />
          </Form.Item>
          <Form.Item
            name="FullName"
            label={t('employee.labelFullName')}
            rules={[{ required: true, message: t('employee.reqFullName') }]}
          >
            <Input placeholder={t('employee.labelFullName')} />
          </Form.Item>
          {!isCreatingGroup && (
            <Form.Item
              name="Password"
              label={t('auth.passwordLabel')}
              rules={[{ required: true, message: t('auth.passwordRequired') }]}
            >
              <Input.Password placeholder={t('auth.passwordPlaceholder')} />
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
