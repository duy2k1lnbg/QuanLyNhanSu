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
  Popconfirm,
  notification,
  Typography,
  theme,
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
} from '@ant-design/icons';
import api from '../services/api';
import PhanQuyenModal from '../components/PhanQuyenModal';
import GroupMembersModal from '../components/GroupMembersModal';
import UserEditModal from '../components/UserEditModal';
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
  const [createUserModalVisible, setCreateUserModalVisible] = useState(false);
  const [isCreatingGroup, setIsCreatingGroup] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formCreateUser] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleToggleLock = async (user: SysUserDTO) => {
    try {
      await api.post(`/users/${user.IdUser}/toggle-lock`);
      notification.success({
        message: 'Thành công',
        description: `Đã ${user.Disabled ? 'mở khóa' : 'khóa'} tài khoản [${user.Username}]!`,
      });
      onRefresh();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể thay đổi trạng thái tài khoản.' });
    }
  };

  const handleDeleteUser = async (user: SysUserDTO) => {
    try {
      await api.delete(`/users/${user.IdUser}`);
      notification.success({
        message: 'Thành công',
        description: `Đã xóa ${user.IsGroup ? 'nhóm' : 'tài khoản'} [${user.Username}]!`,
      });
      onRefresh();
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
      onRefresh();
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
    if (!userSearchText) return true;
    const kw = userSearchText.toLowerCase();
    return (
      (u.Username && u.Username.toLowerCase().includes(kw)) ||
      (u.FullName && u.FullName.toLowerCase().includes(kw))
    );
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
      title: 'Thao tác quản trị',
      key: 'actions',
      width: 280,
      render: (_, record) => {
        const isRootAdmin = Boolean(record.IsAdmin || record.Username?.toUpperCase() === 'ADMIN');

        if (isRootAdmin) {
          return (
            <Tag color="gold" icon={<SafetyCertificateOutlined />} style={{ padding: '3px 10px', fontWeight: 600, fontSize: 12 }}>
              Toàn quyền hệ thống (Mặc định)
            </Tag>
          );
        }

        return (
          <Space size="small" wrap>
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
      <Card
        title="🔐 Quản trị Người dùng & Phân quyền Hệ thống"
        extra={
          <Space>
            <Input
              placeholder={userTab === 'users' ? 'Tìm tài khoản, họ tên...' : 'Tìm mã nhóm, tên nhóm...'}
              prefix={<SearchOutlined />}
              value={userSearchText}
              onChange={(e) => setUserSearchText(e.target.value)}
              allowClear
              style={{ width: 240 }}
            />
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
            <Button icon={<ReloadOutlined />} onClick={onRefresh}>
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
                <Table
                  columns={userColumns}
                  dataSource={filteredUsers}
                  rowKey="IdUser"
                  loading={userLoading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 8, showTotal: (t) => `Tổng số ${t} người dùng` }}
                />
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
                <Table
                  columns={groupColumns}
                  dataSource={filteredUsers}
                  rowKey="IdUser"
                  loading={userLoading}
                  scroll={{ x: 'max-content' }}
                  pagination={{ pageSize: 8, showTotal: (t) => `Tổng số ${t} nhóm quyền` }}
                />
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
            onRefresh();
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
          onSuccess={onRefresh}
        />
      )}
    </>
  );
}

export default UserManagementPage;
