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
import { useAppLanguage } from '../services/i18n';

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
  const { t } = useAppLanguage();
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
      message.error(t('user.loadMembersError'));
    } finally {
      setLoading(false);
    }
  };

  const handleAddMember = async () => {
    if (!selectedUserToAdd) {
      message.warning(t('user.selectUserToAddWarning'));
      return;
    }
    setAdding(true);
    try {
      await api.post(`/users/groups/${groupId}/members/${selectedUserToAdd}`);
      message.success(t('user.addMemberSuccess'));
      setSelectedUserToAdd(null);
      loadMembers();
      if (onSuccess) onSuccess();
    } catch {
      message.error(t('user.addMemberError'));
    } finally {
      setAdding(false);
    }
  };

  const handleRemoveMember = async (memberId: number) => {
    try {
      await api.delete(`/users/groups/${groupId}/members/${memberId}`);
      message.success(t('user.removeMemberSuccess'));
      loadMembers();
      if (onSuccess) onSuccess();
    } catch {
      message.error(t('user.removeMemberError'));
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
      title: t('user.colUsername'),
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
      title: t('user.colFullName'),
      dataIndex: 'FullName',
      key: 'FullName',
      render: (name: string) => name || t('employee360.notUpdated'),
    },
    {
      title: t('user.colStatus'),
      dataIndex: 'Disabled',
      key: 'Disabled',
      width: 120,
      render: (disabled?: boolean) =>
        disabled ? (
          <Tag color="red">{t('employee360.accountSuspended')}</Tag>
        ) : (
          <Tag color="green">{t('employee360.accountActive')}</Tag>
        ),
    },
    {
      title: t('common.actions'),
      key: 'action',
      width: 110,
      align: 'center',
      render: (_, record) => (
        <Popconfirm
          title={t('user.removeMemberConfirmTitle')}
          description={t('user.removeMemberConfirmDesc', { username: record.Username, groupName })}
          onConfirm={() => handleRemoveMember(record.IdUser)}
          okText={t('user.btnRemove')}
          cancelText={t('common.cancel')}
        >
          <Button size="small" danger icon={<UserDeleteOutlined />}>
            {t('user.btnRemove')}
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
            {t('user.groupMembersTitle', { name: groupFullName || groupName })} ({groupName})
          </span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width="min(720px, 95vw)"
      footer={[
        <Button key="close" type="primary" onClick={onClose}>
          {t('user.btnComplete')}
        </Button>,
      ]}
    >
      <Alert
        message={t('user.groupInheritAlert', { groupName })}
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
            <span>{t('user.addMemberCardTitle')}</span>
          </Space>
        }
      >
        <Row gutter={[8, 8]} align="middle">
          <Col xs={24} sm={15} md="auto" flex="auto">
            <Select
              placeholder={t('user.selectUserPrompt')}
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
          <Col xs={18} sm="auto">
            <Button
              type="primary"
              icon={<UserAddOutlined />}
              onClick={handleAddMember}
              loading={adding}
              disabled={!selectedUserToAdd}
              style={{ background: '#52c41a', width: '100%' }}
            >
              {t('user.btnAddMember')}
            </Button>
          </Col>
          <Col xs={6} sm="auto">
            <Button icon={<ReloadOutlined />} onClick={loadMembers} style={{ width: '100%' }} />
          </Col>
        </Row>
      </Card>

      <Table
        columns={columns}
        dataSource={members}
        rowKey="IdUser"
        loading={loading}
        scroll={{ x: 'max-content' }}
        pagination={{ pageSize: 6 }}
        size="small"
        locale={{
          emptyText: <Empty description={t('user.emptyGroupMembers')} />,
        }}
      />
    </Modal>
  );
};

export default GroupMembersModal;
