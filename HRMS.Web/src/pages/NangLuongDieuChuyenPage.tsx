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
  SwapOutlined,
  ReloadOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import type { NangLuongDTO, DieuChuyenDTO, DanhMucAllDTO } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';

const { Text } = Typography;

interface NangLuongDieuChuyenPageProps {
  nangLuongList: NangLuongDTO[];
  dieuChuyenList: DieuChuyenDTO[];
  nlDcLoading: boolean;
  danhMuc: DanhMucAllDTO | null;
  onRefresh: () => void;
  hasRight: (...codes: string[]) => boolean;
  canAdd?: (...codes: string[]) => boolean;
  canEdit?: (...codes: string[]) => boolean;
  canDelete?: (...codes: string[]) => boolean;
  canPrint?: (...codes: string[]) => boolean;
}

export function NangLuongDieuChuyenPage({
  nangLuongList,
  dieuChuyenList,
  nlDcLoading,
  danhMuc,
  onRefresh,
  hasRight,
  canAdd,
  canDelete,
}: NangLuongDieuChuyenPageProps) {
  const { t } = useAppLanguage();
  const [nlModalVisible, setNlModalVisible] = useState(false);
  const [dcModalVisible, setDcModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formNl] = Form.useForm();
  const [formDc] = Form.useForm();

  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const handleSaveNl = async () => {
    try {
      const values = await formNl.validateFields();
      setSaving(true);
      await api.post('/nangluong', {
        SoQd: values.SoQd,
        SoHd: values.SoHd,
        MaNv: values.MaNv,
        HeSoLuongHienTai: values.HeSoLuongHienTai,
        HeSoLuongMoi: values.HeSoLuongMoi,
        NgayKy: values.NgayKy ? dayjs(values.NgayKy).format('YYYY-MM-DD') : null,
        NgayLenLuong: values.NgayLenLuong ? dayjs(values.NgayLenLuong).format('YYYY-MM-DD') : null,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: t('common.success'), description: t('common.saveSuccess') });
      setNlModalVisible(false);
      formNl.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.saveError') });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteNl = async (soqd: string) => {
    try {
      await api.delete(`/nangluong/${encodeURIComponent(soqd)}`);
      notification.success({ message: t('common.success'), description: t('common.deleteSuccess') });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.deleteError') });
    }
  };

  const handleSaveDc = async () => {
    try {
      const values = await formDc.validateFields();
      setSaving(true);
      await api.post('/dieuchuyen', {
        SoQd: values.SoQd,
        MaNv: values.MaNv,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        IdPb: values.IdPb,
        IdPbMoi: values.IdPbMoi,
        GhiChu: values.GhiChu,
        LyDo: values.LyDo,
      });
      notification.success({ message: t('common.success'), description: t('common.saveSuccess') });
      setDcModalVisible(false);
      formDc.resetFields();
      onRefresh();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: t('common.error'), description: errorObj.response?.data?.Message || t('common.saveError') });
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteDc = async (soqd: string) => {
    try {
      await api.delete(`/dieuchuyen/${encodeURIComponent(soqd)}`);
      notification.success({ message: t('common.success'), description: t('common.deleteSuccess') });
      onRefresh();
    } catch {
      notification.error({ message: t('common.error'), description: t('common.deleteError') });
    }
  };

  return (
    <>
      <Card
        title={`📈 ${t('promotion.pageTitle')}`}
        extra={
          <Space>
            {(canAdd ? canAdd('NANGLUONG', 'DIEUCHUYEN', 'F_NV_NANGLUONG', 'F_NV_DIEUCHUYEN') : hasRight('NANGLUONG', 'DIEUCHUYEN', 'F_NV_NANGLUONG', 'F_NV_DIEUCHUYEN')) && (
              <>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => {
                    formNl.resetFields();
                    setNlModalVisible(true);
                  }}
                >
                  {t('promotion.btnAddSalaryIncrease')}
                </Button>
                <Button
                  type="default"
                  icon={<SwapOutlined />}
                  onClick={() => {
                    formDc.resetFields();
                    setDcModalVisible(true);
                  }}
                >
                  {t('promotion.btnAddTransfer')}
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
          defaultActiveKey="nangluong"
          items={[
            {
              key: 'nangluong',
              label: `${t('promotion.tabSalaryIncrease')} (${nangLuongList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: t('promotion.colDecisionNo'), dataIndex: 'SOQD', key: 'SOQD', render: (tVal) => <Tag color="blue">{tVal}</Tag> },
                    { title: t('employee.colEmpCode'), dataIndex: 'MANV', key: 'MANV', width: 80, render: (tVal) => <Tag color="cyan">#{tVal}</Tag> },
                    { title: t('promotion.colEmployee'), dataIndex: 'HOTEN', key: 'HOTEN', render: (tVal) => <Text strong>{tVal}</Text> },
                    { title: t('contract.colContractNo'), dataIndex: 'SOHD', key: 'SOHD' },
                    {
                      title: t('promotion.colOldSalary'),
                      dataIndex: 'HESOLUONGHIENTAI',
                      key: 'HESOLUONGHIENTAI',
                      render: (v) => <Tag color="default">{v}x</Tag>,
                    },
                    {
                      title: t('promotion.colNewSalary'),
                      dataIndex: 'HESOLUONGMOI',
                      key: 'HESOLUONGMOI',
                      render: (v) => <Tag color="green">{v}x</Tag>,
                    },
                    { title: t('contract.labelSignDate'), dataIndex: 'NGAYKY', key: 'NGAYKY' },
                    { title: t('promotion.colEffectiveDate'), dataIndex: 'NGAYLENLUONG', key: 'NGAYLENLUONG' },
                    { title: t('common.note'), dataIndex: 'GHICHU', key: 'GHICHU' },
                    {
                      title: t('common.actions'),
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('NANGLUONG', 'F_NV_NANGLUONG') : hasRight('NANGLUONG', 'F_NV_NANGLUONG')) && (
                          <Popconfirm
                            title={t('common.confirmDeleteTitle')}
                            onConfirm={() => handleDeleteNl(r.SOQD)}
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
                  dataSource={nangLuongList}
                  rowKey="SOQD"
                  loading={nlDcLoading}
                  pagination={{ pageSize: 10, showTotal: (tot) => t('common.totalRecords', { total: tot }) }}
                />
              ),
            },
            {
              key: 'dieuchuyen',
              label: `${t('promotion.tabTransfer')} (${dieuChuyenList.length})`,
              children: (
                <Table
                  scroll={{ x: 'max-content' }}
                  columns={[
                    { title: t('promotion.colDecisionNo'), dataIndex: 'SOQD', key: 'SOQD', render: (tVal) => <Tag color="purple">{tVal}</Tag> },
                    { title: t('employee.colEmpCode'), dataIndex: 'MANV', key: 'MANV', width: 80, render: (tVal) => <Tag color="cyan">#{tVal}</Tag> },
                    { title: t('promotion.colEmployee'), dataIndex: 'HOTEN', key: 'HOTEN', render: (tVal) => <Text strong>{tVal}</Text> },
                    { title: t('promotion.colEffectiveDate'), dataIndex: 'NGAY', key: 'NGAY' },
                    { title: t('promotion.colOldDepartment'), dataIndex: 'TENPB', key: 'TENPB', render: (tVal) => <Tag color="orange">{tVal || '-'}</Tag> },
                    { title: t('promotion.colNewDepartment'), dataIndex: 'TENPB_MOI', key: 'TENPB_MOI', render: (tVal) => <Tag color="green">{tVal || '-'}</Tag> },
                    { title: t('reward.colReason'), dataIndex: 'LYDO', key: 'LYDO' },
                    { title: t('common.note'), dataIndex: 'GHICHU', key: 'GHICHU' },
                    {
                      title: t('common.actions'),
                      key: 'action',
                      width: 90,
                      render: (_, r) => (
                        (canDelete ? canDelete('DIEUCHUYEN', 'F_NV_DIEUCHUYEN') : hasRight('DIEUCHUYEN', 'F_NV_DIEUCHUYEN')) && (
                          <Popconfirm
                            title={t('common.confirmDeleteTitle')}
                            onConfirm={() => handleDeleteDc(r.SOQD)}
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
                  dataSource={dieuChuyenList}
                  rowKey="SOQD"
                  loading={nlDcLoading}
                  pagination={{ pageSize: 10, showTotal: (tot) => t('common.totalRecords', { total: tot }) }}
                />
              ),
            },
          ]}
        />
      </Card>

      {/* Modal Nâng lương */}
      <Modal
        title={t('promotion.btnAddSalaryIncrease')}
        open={nlModalVisible}
        onCancel={() => setNlModalVisible(false)}
        onOk={handleSaveNl}
        confirmLoading={saving}
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width="min(540px, 95vw)"
      >
        <Form form={formNl} layout="vertical">
          <Form.Item name="SoQd" label={t('promotion.colDecisionNo')} rules={[{ required: true, message: t('promotion.colDecisionNo') }]}>
            <Input placeholder="QDNL-2026-001" />
          </Form.Item>
          <Form.Item name="SoHd" label={t('contract.colContractNo')} rules={[{ required: true, message: t('contract.colContractNo') }]}>
            <Input placeholder="HDLD-2026-001" />
          </Form.Item>
          <Form.Item name="MaNv" label={t('employee.colEmpCode')} rules={[{ required: true, message: t('employee.colEmpCode') }]}>
            <InputNumber style={{ width: '100%' }} placeholder="10" />
          </Form.Item>
          <Form.Item name="HeSoLuongHienTai" label={t('promotion.colOldSalary')} initialValue={1.0}>
            <InputNumber step={0.1} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="HeSoLuongMoi" label={t('promotion.colNewSalary')} rules={[{ required: true, message: t('promotion.colNewSalary') }]}>
            <InputNumber step={0.1} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="NgayKy" label={t('contract.labelSignDate')} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="NgayLenLuong" label={t('promotion.colEffectiveDate')} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="GhiChu" label={t('common.note')}>
            <Input placeholder={t('common.note')} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Modal Điều chuyển */}
      <Modal
        title={t('promotion.btnAddTransfer')}
        open={dcModalVisible}
        onCancel={() => setDcModalVisible(false)}
        onOk={handleSaveDc}
        confirmLoading={saving}
        destroyOnClose
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width="min(540px, 95vw)"
      >
        <Form form={formDc} layout="vertical">
          <Form.Item name="SoQd" label={t('promotion.colDecisionNo')} rules={[{ required: true, message: t('promotion.colDecisionNo') }]}>
            <Input placeholder="QDDC-2026-001" />
          </Form.Item>
          <Form.Item name="MaNv" label={t('employee.colEmpCode')} rules={[{ required: true, message: t('employee.colEmpCode') }]}>
            <InputNumber style={{ width: '100%' }} placeholder="10" />
          </Form.Item>
          <Form.Item name="Ngay" label={t('promotion.colEffectiveDate')} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="IdPb" label={t('promotion.colOldDepartment')} rules={[{ required: true, message: t('promotion.colOldDepartment') }]}>
            <Select
              placeholder={t('promotion.colOldDepartment')}
              options={danhMuc?.phongBan?.map((pb) => ({ value: pb.IDPB, label: pb.TENPB })) || []}
            />
          </Form.Item>
          <Form.Item name="IdPbMoi" label={t('promotion.colNewDepartment')} rules={[{ required: true, message: t('promotion.colNewDepartment') }]}>
            <Select
              placeholder={t('promotion.colNewDepartment')}
              options={danhMuc?.phongBan?.map((pb) => ({ value: pb.IDPB, label: pb.TENPB })) || []}
            />
          </Form.Item>
          <Form.Item name="LyDo" label={t('reward.colReason')}>
            <Input placeholder={t('reward.colReason')} />
          </Form.Item>
          <Form.Item name="GhiChu" label={t('common.note')}>
            <Input placeholder={t('common.note')} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}

export default NangLuongDieuChuyenPage;
