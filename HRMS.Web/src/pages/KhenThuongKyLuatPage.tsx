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
  ReloadOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import type { KhenThuongDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

const { Text } = Typography;

interface KhenThuongKyLuatPageProps {
  khenThuongList: KhenThuongDTO[];
  kyLuatList: KhenThuongDTO[];
  ktLoading: boolean;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
}

export function KhenThuongKyLuatPage({
  khenThuongList,
  kyLuatList,
  ktLoading,
  onRefresh,
  hasRight,
  canAdd,
  canDelete,
}: KhenThuongKyLuatPageProps) {
  const { t } = useAppLanguage();
  const [ktModalVisible, setKtModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formKt] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleSaveKt = async () => {
    try {
      const values = await formKt.validateFields();
      setSaving(true);
      await api.post('/khenthuong', {
        SoQd: values.SoQd,
        Loai: values.Loai,
        MaNv: values.MaNv,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        NoiDung: values.NoiDung,
        LyDo: values.LyDo,
      });
      notification.success({ message: t('common.success'), description: t('common.saveSuccess') });
      setKtModalVisible(false);
      formKt.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.saveError') });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteKt = async (soqd: string) => {
    try {
      await api.delete(`/khenthuong/${encodeURIComponent(soqd)}`);
      notification.success({ message: t('common.success'), description: t('common.deleteSuccess') });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.deleteError') });
    }
  };

  return (
    <>
      <Card
        title={`🏆 ${t('reward.pageTitle')}`}
        extra={
          <Space>
            {(canAdd ? canAdd('KHENTHUONG', 'KYLUAT', 'F_NV_KHENTHUONG', 'F_NV_KYLUAT') : hasRight('KHENTHUONG', 'KYLUAT', 'F_NV_KHENTHUONG', 'F_NV_KYLUAT')) && (
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  formKt.resetFields();
                  setKtModalVisible(true);
                }}
              >
                {t('reward.btnAddReward')}
              </Button>
            )}
            <Button icon={<ReloadOutlined />} onClick={onRefresh}>
              {t('common.refresh')}
            </Button>
          </Space>
        }
        bordered={false}
        style={{ borderRadius: borderRadiusLG }}
      >
        <Tabs
          defaultActiveKey="khenthuong"
          items={[
            {
              key: 'khenthuong',
              label: `${t('reward.tabRewards')} (${khenThuongList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: t('reward.colDecisionNo'), dataIndex: 'SOQD', key: 'SOQD', render: (tVal) => <Tag color="green">{tVal}</Tag> },
                    { title: t('employee.colEmpCode'), dataIndex: 'MANV', key: 'MANV', width: 80, render: (tVal) => <Tag color="blue">#{tVal}</Tag> },
                    { title: t('reward.colEmployee'), dataIndex: 'HOTEN', key: 'HOTEN', render: (tVal) => <Text strong>{tVal}</Text> },
                    { title: t('reward.colDate'), dataIndex: 'NGAY', key: 'NGAY' },
                    { title: t('common.description'), dataIndex: 'NOIDUNG', key: 'NOIDUNG' },
                    { title: t('reward.colReason'), dataIndex: 'LYDO', key: 'LYDO' },
                    {
                      title: t('common.actions'),
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('KHENTHUONG', 'F_NV_KHENTHUONG') : hasRight('KHENTHUONG', 'F_NV_KHENTHUONG')) && (
                          <Popconfirm
                            title={t('common.confirmDeleteTitle')}
                            onConfirm={() => handleDeleteKt(r.SOQD)}
                            okText={t('common.confirm')}
                            cancelText={t('common.cancel')}
                            okButtonProps={{ danger: true }}
                          >
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={khenThuongList}
                  rowKey="SOQD"
                  loading={ktLoading}
                  pagination={{ pageSize: 10, showTotal: (tot) => t('common.totalRecords', { total: tot }) }}
                />
              ),
            },
            {
              key: 'kyluat',
              label: `${t('reward.tabDisciplines')} (${kyLuatList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: t('reward.colDecisionNo'), dataIndex: 'SOQD', key: 'SOQD', render: (tVal) => <Tag color="red">{tVal}</Tag> },
                    { title: t('employee.colEmpCode'), dataIndex: 'MANV', key: 'MANV', width: 80, render: (tVal) => <Tag color="blue">#{tVal}</Tag> },
                    { title: t('reward.colEmployee'), dataIndex: 'HOTEN', key: 'HOTEN', render: (tVal) => <Text strong>{tVal}</Text> },
                    { title: t('reward.colDate'), dataIndex: 'NGAY', key: 'NGAY' },
                    { title: t('common.description'), dataIndex: 'NOIDUNG', key: 'NOIDUNG' },
                    { title: t('reward.colReason'), dataIndex: 'LYDO', key: 'LYDO' },
                    {
                      title: t('common.actions'),
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('KYLUAT', 'F_NV_KYLUAT') : hasRight('KYLUAT', 'F_NV_KYLUAT')) && (
                          <Popconfirm
                            title={t('common.confirmDeleteTitle')}
                            onConfirm={() => handleDeleteKt(r.SOQD)}
                            okText={t('common.confirm')}
                            cancelText={t('common.cancel')}
                            okButtonProps={{ danger: true }}
                          >
                            <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          </Popconfirm>
                        )
                      ),
                    },
                  ]}
                  dataSource={kyLuatList}
                  rowKey="SOQD"
                  loading={ktLoading}
                  pagination={{ pageSize: 10, showTotal: (tot) => t('common.totalRecords', { total: tot }) }}
                />
              ),
            },
          ]}
        />
      </Card>

      <Modal
        title={t('reward.btnAddReward')}
        open={ktModalVisible}
        onCancel={() => setKtModalVisible(false)}
        onOk={handleSaveKt}
        confirmLoading={saving}
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width="min(540px, 95vw)"
      >
        <Form form={formKt} layout="vertical">
          <Form.Item name="SoQd" label={t('reward.colDecisionNo')} rules={[{ required: true, message: t('reward.colDecisionNo') }]}>
            <Input placeholder="QD-2026-001" />
          </Form.Item>
          <Form.Item name="Loai" label={t('common.actions')} initialValue={1} rules={[{ required: true }]}>
            <Select
              options={[
                { value: 1, label: `${t('reward.tabRewards')} (+)` },
                { value: 2, label: `${t('reward.tabDisciplines')} (-)` },
              ]}
            />
          </Form.Item>
          <Form.Item name="MaNv" label={t('employee.colEmpCode')} rules={[{ required: true, message: t('employee.colEmpCode') }]}>
            <InputNumber style={{ width: '100%' }} placeholder="10" />
          </Form.Item>
          <Form.Item name="Ngay" label={t('reward.colDate')} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="NoiDung" label={t('common.description')}>
            <Input.TextArea rows={2} placeholder={t('common.description')} />
          </Form.Item>
          <Form.Item name="LyDo" label={t('reward.colReason')}>
            <Input placeholder={t('reward.colReason')} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}

export default KhenThuongKyLuatPage;
