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
import type { UngLuongDTO, TangCaDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

const { Text } = Typography;

interface TangCaUngLuongPageProps {
  ungLuongList: UngLuongDTO[];
  tangCaList: TangCaDTO[];
  tcUlLoading: boolean;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
}

export function TangCaUngLuongPage({
  ungLuongList,
  tangCaList,
  tcUlLoading,
  onRefresh,
  hasRight,
  canAdd,
  canDelete,
}: TangCaUngLuongPageProps) {
  const { t } = useAppLanguage();
  const [ulModalVisible, setUlModalVisible] = useState(false);
  const [tcModalVisible, setTcModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formUl] = Form.useForm();
  const [formTc] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleSaveUl = async () => {
    try {
      const values = await formUl.validateFields();
      setSaving(true);
      await api.post('/ungluong', {
        Nam: values.Nam || dayjs().year(),
        Thang: values.Thang || dayjs().month() + 1,
        Ngay: values.Ngay ? dayjs(values.Ngay).date() : dayjs().date(),
        MaNv: values.MaNv,
        SoTien: values.SoTien,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: t('common.success'), description: t('common.saveSuccess') });
      setUlModalVisible(false);
      formUl.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.saveError') });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteUl = async (id: number) => {
    try {
      await api.delete(`/ungluong/${id}`);
      notification.success({ message: t('common.success'), description: t('common.deleteSuccess') });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.deleteError') });
    }
  };

  const handleSaveTc = async () => {
    try {
      const values = await formTc.validateFields();
      setSaving(true);
      await api.post('/tangca', {
        Nam: values.Nam || dayjs().year(),
        Thang: values.Thang || dayjs().month() + 1,
        Ngay: values.Ngay ? dayjs(values.Ngay).date() : dayjs().date(),
        SoGio: values.SoGio,
        MaNv: values.MaNv,
        IdLoaiCa: values.IdLoaiCa,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: t('common.success'), description: t('common.saveSuccess') });
      setTcModalVisible(false);
      formTc.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.saveError') });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteTc = async (id: number) => {
    try {
      await api.delete(`/tangca/${id}`);
      notification.success({ message: t('common.success'), description: t('common.deleteSuccess') });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.deleteError') });
    }
  };

  return (
    <>
      <Card
        title={`💸 ${t('overtime.pageTitle')}`}
        extra={
          <Space>
            {(canAdd ? canAdd('UNGLUONG', 'TANGCA', 'F_CC_UNGLUONG', 'F_CC_TANGCA') : hasRight('UNGLUONG', 'TANGCA', 'F_CC_UNGLUONG', 'F_CC_TANGCA')) && (
              <>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => {
                    formUl.resetFields();
                    setUlModalVisible(true);
                  }}
                >
                  {t('overtime.btnAddAdvance')}
                </Button>
                <Button
                  type="default"
                  icon={<PlusOutlined />}
                  onClick={() => {
                    formTc.resetFields();
                    setTcModalVisible(true);
                  }}
                >
                  {t('overtime.btnAddOvertime')}
                </Button>
              </>
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
          defaultActiveKey="ungluong"
          items={[
            {
              key: 'ungluong',
              label: `${t('overtime.tabAdvance')} (${ungLuongList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'ID', dataIndex: 'ID', key: 'ID', width: 70 },
                    { title: t('employee.colEmpCode'), dataIndex: 'MANV', key: 'MANV', width: 80, render: (tVal) => <Tag color="blue">#{tVal}</Tag> },
                    { title: t('overtime.colEmployee'), dataIndex: 'HOTEN', key: 'HOTEN', render: (tVal) => <Text strong>{tVal}</Text> },
                    {
                      title: t('overtime.colDate'),
                      key: 'date',
                      render: (_, r) => `${r.NGAY}/${r.THANG}/${r.NAM}`,
                    },
                    {
                      title: t('overtime.colAmount'),
                      dataIndex: 'SOTIEN',
                      key: 'SOTIEN',
                      render: (st: number) => (
                        <Text strong style={{ color: '#fa8c16' }}>
                          {st?.toLocaleString('vi-VN')} đ
                        </Text>
                      ),
                    },
                    { title: t('common.note'), dataIndex: 'GHICHU', key: 'GHICHU' },
                    {
                      title: t('common.actions'),
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('UNGLUONG', 'F_CC_UNGLUONG') : hasRight('UNGLUONG', 'F_CC_UNGLUONG')) && (
                          <Popconfirm
                            title={t('common.confirmDeleteTitle')}
                            onConfirm={() => handleDeleteUl(r.ID)}
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
                  dataSource={ungLuongList}
                  rowKey="ID"
                  loading={tcUlLoading}
                  pagination={{ pageSize: 10, showTotal: (tot) => t('common.totalRecords', { total: tot }) }}
                />
              ),
            },
            {
              key: 'tangca',
              label: `${t('overtime.tabOvertime')} (${tangCaList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: 'ID', dataIndex: 'ID', key: 'ID', width: 70 },
                    { title: t('employee.colEmpCode'), dataIndex: 'MANV', key: 'MANV', width: 80, render: (tVal) => <Tag color="blue">#{tVal}</Tag> },
                    { title: t('overtime.colEmployee'), dataIndex: 'HOTEN', key: 'HOTEN', render: (tVal) => <Text strong>{tVal}</Text> },
                    {
                      title: t('overtime.colDate'),
                      key: 'date',
                      render: (_, r) => `${r.NGAY}/${r.THANG}/${r.NAM}`,
                    },
                    {
                      title: t('overtime.colHours'),
                      dataIndex: 'SOGIO',
                      key: 'SOGIO',
                      render: (g: number) => <Tag color="green">{g}h</Tag>,
                    },
                    {
                      title: t('attendance.tabShifts'),
                      dataIndex: 'TENLOAICA',
                      key: 'TENLOAICA',
                      render: (tVal) => <Tag color="geekblue">{tVal || '-'}</Tag>,
                    },
                    {
                      title: t('overtime.colCoefficient'),
                      dataIndex: 'HESO',
                      key: 'HESO',
                      render: (h: number) => <Tag color="orange">{h ?? 1.5}x</Tag>,
                    },
                    { title: t('common.note'), dataIndex: 'GHICHU', key: 'GHICHU' },
                    {
                      title: t('common.actions'),
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('TANGCA', 'F_CC_TANGCA') : hasRight('TANGCA', 'F_CC_TANGCA')) && (
                          <Popconfirm
                            title={t('common.confirmDeleteTitle')}
                            onConfirm={() => handleDeleteTc(r.ID)}
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
                  dataSource={tangCaList}
                  rowKey="ID"
                  loading={tcUlLoading}
                  pagination={{ pageSize: 10, showTotal: (tot) => t('common.totalRecords', { total: tot }) }}
                />
              ),
            },
          ]}
        />
      </Card>

      {/* Modal Tạm ứng */}
      <Modal
        title={t('overtime.btnAddAdvance')}
        open={ulModalVisible}
        onCancel={() => setUlModalVisible(false)}
        onOk={handleSaveUl}
        confirmLoading={saving}
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width="min(500px, 95vw)"
      >
        <Form form={formUl} layout="vertical">
          <Form.Item name="MaNv" label={t('employee.colEmpCode')} rules={[{ required: true, message: t('employee.colEmpCode') }]}>
            <InputNumber style={{ width: '100%' }} placeholder="10" />
          </Form.Item>
          <Form.Item name="Ngay" label={t('overtime.colDate')} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="SoTien" label={t('overtime.colAmount')} rules={[{ required: true, message: t('overtime.colAmount') }]}>
            <InputNumber
              style={{ width: '100%' }}
              formatter={(val) => `${val}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              placeholder="1,000,000"
            />
          </Form.Item>
          <Form.Item name="GhiChu" label={t('common.note')}>
            <Input placeholder={t('common.note')} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Modal Tăng ca */}
      <Modal
        title={t('overtime.btnAddOvertime')}
        open={tcModalVisible}
        onCancel={() => setTcModalVisible(false)}
        onOk={handleSaveTc}
        confirmLoading={saving}
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width="min(500px, 95vw)"
      >
        <Form form={formTc} layout="vertical">
          <Form.Item name="MaNv" label={t('employee.colEmpCode')} rules={[{ required: true, message: t('employee.colEmpCode') }]}>
            <InputNumber style={{ width: '100%' }} placeholder="10" />
          </Form.Item>
          <Form.Item name="Ngay" label={t('overtime.colDate')} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="SoGio" label={t('overtime.colHours')} rules={[{ required: true, message: t('overtime.colHours') }]}>
            <InputNumber step={0.5} style={{ width: '100%' }} placeholder="2.0" />
          </Form.Item>
          <Form.Item name="IdLoaiCa" label={t('attendance.tabShifts')} initialValue={1}>
            <Select
              options={[
                { value: 1, label: `${t('attendance.tabShifts')} 1 (1.5x)` },
                { value: 2, label: `${t('attendance.tabShifts')} 2 (2.0x)` },
              ]}
            />
          </Form.Item>
          <Form.Item name="GhiChu" label={t('common.note')}>
            <Input placeholder={t('common.note')} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}

export default TangCaUngLuongPage;
