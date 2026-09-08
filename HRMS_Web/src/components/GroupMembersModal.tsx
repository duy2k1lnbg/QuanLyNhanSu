import React, { useState, useEffect } from 'react';
import {
  Modal,
  Table,
  Button,
  Space,
  Tag,
  Typography,
  message,
  Popconfirm,
  Select,
  Row,
  Col,
  Empty,
  Card,
  Alert,
} from 'antd';
import {
  TeamOutlined,
  UserAddOutlined,
  UserDeleteOutlined,
  UserOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../services/api';
import type { GroupMembersResponseDTO, GroupMemberItemDTO } from '../types/hrms';

const { Text } = Typography;

interface GroupMembersModalProps {
  visible: boolean;
  onClose: () => void;
  groupId: number;
  groupName: string;
  groupFullName: string;
  onSuccess?: () => void;
}

export const GroupMembersModal: React.FC<GroupMembersModalProps> = ({
  visible,
  onClose,
  groupId,
  groupName,
  groupFullName,
  onSuccess,
}) => {
  const [data, setData] = useState<GroupMembersResponseDTO | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [selectedUserToAdd, setSelectedUserToAdd] = useState<number | null>(null);
  const [adding, setAdding] = useState<boolean>(false);

  useEffect(() => {
    if (visible && groupId) {
      loadMembers();
    }
  }, [visible, groupId]);

  const loadMembers = async () => {
    setLoading(true);
    try {
      const res = await api.get<GroupMembersResponseDTO>(`/users/groups/${groupId}/members`);
      if (res.data) {
        setData(res.data);
      }
    } catch {
      message.error('Không thể tải danh sách thành viên nhóm.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddMember = async () => {
    if (!selectedUserToAdd) {
      message.warning('Vui lòng chọn tài khoản cần thêm vào nhóm.');
      return;
    }
    setAdding(true);
    try {
      await api.post(`/users/groups/${groupId}/members/${selectedUserToAdd}`);
      message.success('Đã thêm thành viên vào nhóm thành công!');
      setSelectedUserToAdd(null);
      loadMembers();
      if (onSuccess) onSuccess();
    } catch {
      message.error('Lỗi khi thêm thành viên vào nhóm.');
    } finally {
      setAdding(false);
    }
  };

  const handleRemoveMember = async (memberId: number) => {
    try {
      await api.delete(`/users/groups/${groupId}/members/${memberId}`);
      message.success('Đã gỡ thành viên khỏi nhóm thành công!');
      loadMembers();
      if (onSuccess) onSuccess();
    } catch {
      message.error('Lỗi khi gỡ thành viên khỏi nhóm.');
    }
  };

  const columns: ColumnsType<GroupMemberItemDTO> = [
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
      width: 150,
      render: (u: string) => (
        <Space>
          <UserOutlined style={{ color: '#1677ff' }} />
          <Text strong>{u}</Text>
        </Space>
      ),
    },
    {
      title: 'Họ và tên',
      dataIndex: 'FullName',
      key: 'FullName',
      render: (name: string) => name || 'Chưa cập nhật',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'Disabled',
      key: 'Disabled',
      width: 120,
      render: (disabled?: boolean) =>
        disabled ? <Tag color="red">Bị khóa</Tag> : <Tag color="green">Hoạt động</Tag>,
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 110,
      align: 'center',
      render: (_, record) => (
        <Popconfirm
          title="Gỡ khỏi nhóm?"
          description={`Gỡ tài khoản [${record.Username}] khỏi nhóm [${groupName}]?`}
          onConfirm={() => handleRemoveMember(record.IdUser)}
          okText="Gỡ"
          cancelText="Hủy"
        >
          <Button size="small" danger icon={<UserDeleteOutlined />}>
            Gỡ
          </Button>
        </Popconfirm>
      ),
    },
  ];

  const members: GroupMemberItemDTO[] = data?.Members || [];
  const availableUsers: GroupMemberItemDTO[] = data?.AvailableUsers || [];

  return (
    <Modal
      title={
        <Space>
          <TeamOutlined style={{ color: '#722ed1', fontSize: 18 }} />
          <span>
            Thành viên nhóm quyền: <b style={{ color: '#722ed1' }}>{groupFullName || groupName}</b> ({groupName})
          </span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width={720}
      footer={[
        <Button key="close" type="primary" onClick={onClose}>
          Hoàn tất
        </Button>,
      ]}
    >
      <Alert
        message={
          <span>
            Tất cả các thành viên trong nhóm này sẽ tự động kế thừa toàn bộ quyền chức năng được phân cho nhóm <b>[{groupName}]</b>.
          </span>
        }
        type="info"
        showIcon
        style={{ marginBottom: 16, marginTop: 8 }}
      />

      <Card
        size="small"
        style={{ marginBottom: 16, background: '#fafafa' }}
        title={
          <Space>
            <UserAddOutlined style={{ color: '#52c41a' }} />
            <span>Thêm tài khoản vào nhóm</span>
          </Space>
        }
      >
        <Row gutter={12} align="middle">
          <Col flex="auto">
            <Select
              placeholder="Chọn người dùng để thêm vào nhóm..."
              value={selectedUserToAdd}
              onChange={(val) => setSelectedUserToAdd(val)}
              style={{ width: '100%' }}
              showSearch
              filterOption={(input, option) =>
                String(option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              options={availableUsers.map((u: GroupMemberItemDTO) => ({
                value: u.IdUser,
                label: `${u.FullName} (${u.Username})`,
              }))}
            />
          </Col>
          <Col>
            <Button
              type="primary"
              icon={<UserAddOutlined />}
              onClick={handleAddMember}
              loading={adding}
              disabled={!selectedUserToAdd}
              style={{ background: '#52c41a' }}
            >
              Thêm vào nhóm
            </Button>
          </Col>
          <Col>
            <Button icon={<ReloadOutlined />} onClick={loadMembers} />
          </Col>
        </Row>
      </Card>

      <Table
        columns={columns}
        dataSource={members}
        rowKey="IdUser"
        loading={loading}
        pagination={{ pageSize: 6 }}
        size="small"
        locale={{
          emptyText: <Empty description="Nhóm này chưa có thành viên nào. Hãy chọn người dùng ở trên để thêm vào nhóm." />,
        }}
      />
    </Modal>
  );
};

export default GroupMembersModal;
