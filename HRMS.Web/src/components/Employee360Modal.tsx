import React, { useState, useEffect } from 'react';
import {
  Modal,
  Tabs,
  Avatar,
  Tag,
  Typography,
  Row,
  Col,
  Card,
  Progress,
  Descriptions,
  Timeline,
  Table,
  Button,
  Space,
  Checkbox,
  Upload,
  Tooltip,
  notification,
  Spin,
  Empty,
  Switch,
  Alert,
} from 'antd';
import {
  UserOutlined,
  PhoneOutlined,
  IdcardOutlined,
  HomeOutlined,
  CalendarOutlined,
  DollarOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  FileProtectOutlined,
  PrinterOutlined,
  EditOutlined,
  RocketOutlined,
  CameraOutlined,
  LoadingOutlined,
  AlertOutlined,
  CloseOutlined,
  MobileOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import type { NhanVienDTO, BangLuongDTO, CurrentUserDTO, SysUserDTO } from '../types/hrms';
import { canEdit, canPrint } from '../utils/permissionUtils';
import PhieuLuongModal from './PhieuLuongModal';
import { useAppTheme } from '../theme/ThemeContext';
import { timelineLabels, checklistLabels } from '../utils/profileEventLabels';
import { useAppLanguage } from '../services/i18n';

const { Title, Text, Paragraph } = Typography;

export interface Profile360Summary {
  NgayVaoCongTy?: string;
  SoHopDong?: string;
  ThoiHanHD?: string;
  HeSoLuong?: number;
  TyLeChuyenCan: number;
  TongNgayCong: number;
  CongChuan: number;
  KyCongChuyenCanLabel?: string;
  SoNgayNghiPhep: number;
  TongQuyPhep: number;
  PhepConLai: number;
  ThuNhapNetKyGanNhat?: number;
  KyLuongNetLabel?: string;
  TrangThaiChiTra?: string;
}

export interface Profile360PayrollItem {
  Key: string;
  Makycong: number;
  KyLuong: string;
  Gross: number;
  Ot: number;
  Net: number;
  CongThucTe: number;
  CongChuan: number;
  TrangThai: string;
}

export interface Profile360TimelineItem {
  Ngay: string;
  Loai: string;
  TieuDe: string;
  MoTa: string;
  TagColor: string;
  IconType: string;
}

export interface Profile360ChecklistItem {
  Title: string;
  Done: boolean;
  Detail: string;
}

export interface Profile360Data {
  Summary: Profile360Summary;
  LichSuLuong: Profile360PayrollItem[];
  Timeline: Profile360TimelineItem[];
  OnboardingChecklist: Profile360ChecklistItem[];
}

interface Employee360ModalProps {
  visible: boolean;
  onClose: () => void;
  employee: NhanVienDTO | null;
  onEdit?: (emp: NhanVienDTO) => void;
  onAvatarUpdated?: (manv: number, newAvatar: string) => void;
  currentUser?: CurrentUserDTO;
}

export const Employee360Modal: React.FC<Employee360ModalProps> = ({
  visible,
  onClose,
  employee,
  onEdit,
  onAvatarUpdated,
  currentUser: propUser,
}) => {
  const { t } = useAppLanguage();
  const { tokens } = useAppTheme();
  const [activeTab, setActiveTab] = useState<string>('overview');
  const [uploadingAvatar, setUploadingAvatar] = useState<boolean>(false);
  const [currentAvatar, setCurrentAvatar] = useState<string | undefined>(employee?.HINHANH);
  const [profileData, setProfileData] = useState<Profile360Data | null>(null);
  const [loadingProfile, setLoadingProfile] = useState<boolean>(false);
  const [payslipModalVisible, setPayslipModalVisible] = useState<boolean>(false);
  const [selectedPayslipRecord, setSelectedPayslipRecord] = useState<BangLuongDTO | null>(null);
  const [linkedAccount, setLinkedAccount] = useState<SysUserDTO | null>(null);
  const [togglingMobile, setTogglingMobile] = useState<boolean>(false);

  // Fallback to localStorage user if not provided in prop
  const currentUser: CurrentUserDTO | undefined = React.useMemo(() => {
    if (propUser) return propUser;
    try {
      const stored = localStorage.getItem('hrms_user');
      return stored ? JSON.parse(stored) : undefined;
    } catch {
      return undefined;
    }
  }, [propUser]);

  // Đồng bộ avatar khi đổi nhân viên
  useEffect(() => {
    if (employee) {
      setCurrentAvatar(employee.HINHANH);
    }
  }, [employee?.MANV, employee?.HINHANH]);

  // Tải dữ liệu hồ sơ 360 thực tế từ Oracle CSDL
  useEffect(() => {
    if (!visible || !employee?.MANV) {
      setProfileData(null);
      return;
    }
    setLoadingProfile(true);
    api.get<Profile360Data>(`/nhanvien/${employee.MANV}/profile360`)
      .then((res) => {
        setProfileData(res.data);
      })
      .catch((err) => {
        console.error('Lỗi tải hồ sơ 360 thực tế:', err);
      })
      .finally(() => {
        setLoadingProfile(false);
      });

    // Tải thông tin tài khoản liên kết
    api.get('/users')
      .then((res) => {
        const users: SysUserDTO[] = Array.isArray(res.data) ? res.data : (res.data?.data || []);
        const found = users.find(
          (u) => (u.Manv === employee.MANV || u.manv === employee.MANV) && !u.IsGroup
        );
        setLinkedAccount(found || null);
      })
      .catch(() => setLinkedAccount(null));
  }, [visible, employee?.MANV]);

  if (!employee) return null;

  const formatAvatarUrl = (img?: string, _manv?: number): string | undefined => {
    if (img && img.trim()) {
      const clean = img.trim();
      if (clean.startsWith('data:') || clean.startsWith('http')) return clean;
      return `data:image/jpeg;base64,${clean}`;
    }
    // Trả về undefined nếu không có ảnh để Avatar render icon UserOutlined mặc định,
    // tránh gửi HTTP request 404 thừa thãi lên server
    return undefined;
  };

  const handleDirectAvatarUpload = (file: File) => {
    if (file.size > 5 * 1024 * 1024) {
      notification.error({
        message: t('employee360.avatarTooLarge'),
        description: t('employee360.avatarTooLargeDesc'),
      });
      return false;
    }
    const reader = new FileReader();
    reader.onload = async (e) => {
      const base64 = e.target?.result as string;
      if (!base64) return;
      try {
        setUploadingAvatar(true);
        await api.post(`/nhanvien/${employee.MANV}/avatar`, { ImageBase64: base64 });
        setCurrentAvatar(base64);
        employee.HINHANH = base64;
        notification.success({
          message: t('employee360.avatarSuccess'),
          description: t('employee360.avatarSuccessDesc'),
        });
        if (onAvatarUpdated) onAvatarUpdated(employee.MANV, base64);
      } catch {
        notification.error({
          message: t('employee360.avatarError'),
          description: t('employee360.avatarErrorDesc'),
        });
      } finally {
        setUploadingAvatar(false);
      }
    };
    reader.readAsDataURL(file);
    return false;
  };

  const renderTimelineDot = (iconType: string) => {
    switch (iconType) {
      case 'DollarOutlined':
        return <DollarOutlined style={{ fontSize: '16px' }} />;
      case 'FileProtectOutlined':
        return <FileProtectOutlined style={{ fontSize: '16px' }} />;
      case 'RocketOutlined':
        return <RocketOutlined style={{ fontSize: '16px' }} />;
      case 'TrophyOutlined':
        return <TrophyOutlined style={{ fontSize: '16px' }} />;
      case 'AlertOutlined':
        return <AlertOutlined style={{ fontSize: '16px' }} />;
      case 'CalendarOutlined':
        return <CalendarOutlined style={{ fontSize: '16px' }} />;
      default:
        return <CheckCircleOutlined style={{ fontSize: '16px' }} />;
    }
  };

  const timelineColor = (kind: string) => {
    if (kind === 'LUONG' || kind === 'JOIN') return { text: tokens.successText, background: tokens.successBg };
    if (kind === 'HOPDONG') return { text: tokens.violetText, background: tokens.violetBg };
    if (kind === 'KYLUAT') return { text: tokens.dangerText, background: tokens.dangerBg };
    if (kind === 'KHENTHUONG' || kind === 'DIEUCHUYEN') return { text: tokens.warningText, background: tokens.warningBg };
    return { text: tokens.infoText, background: tokens.infoBg };
  };
  const summary = profileData?.Summary;
  const rawPayroll = profileData?.LichSuLuong || [];
  const timelineItems = profileData?.Timeline || [];
  const checklistItems = profileData?.OnboardingChecklist || [];
  const doneCount = checklistItems.filter((c) => c.Done).length;
  const onboardingPercent = checklistItems.length > 0 ? Math.round((doneCount / checklistItems.length) * 100) : 0;

  return (
    <>
      <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      width="min(1000px, 95vw)"
      className="employee-360-modal"
      rootClassName="employee-360-modal"
      style={{ top: 20 }}
      styles={{
        container: {
          padding: 0,
          overflow: 'hidden',
          borderRadius: 16,
          boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.45)',
        },
        body: { padding: 0, maxHeight: '86vh', overflowY: 'auto' },
        close: {
          top: 18,
          right: 18,
          color: '#ffffff',
          backgroundColor: 'rgba(255, 255, 255, 0.2)',
          border: '1px solid rgba(255, 255, 255, 0.35)',
          backdropFilter: 'blur(8px)',
          WebkitBackdropFilter: 'blur(8px)',
          borderRadius: '50%',
          width: 34,
          height: 34,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 50,
        },
      }}
      closeIcon={<CloseOutlined style={{ fontSize: 14, color: '#ffffff' }} />}
      title={null}
    >
      {/* GRAND HERO HEADER */}
      <div className="employee-360-header">
        <div className="employee-360-header-inner">
          {/* LEFT: AVATAR & EMPLOYEE INFO */}
          <div className="employee-360-profile-main">
            <div style={{ position: 'relative', flexShrink: 0 }}>
              <Avatar
                size={84}
                className="employee-360-avatar"
                icon={uploadingAvatar ? <LoadingOutlined /> : <UserOutlined />}
                src={formatAvatarUrl(currentAvatar, employee.MANV)}
                style={{
                  backgroundColor: '#3b82f6',
                  border: '3px solid #60a5fa',
                  fontSize: 36,
                }}
              />
              <span
                style={{
                  position: 'absolute',
                  bottom: 2,
                  left: 2,
                  width: 14,
                  height: 14,
                  backgroundColor: employee.DATHOIVIEC === 1 ? '#ef4444' : '#10b981',
                  borderRadius: '50%',
                  border: '2px solid #0f172a',
                  zIndex: 2,
                }}
                title={employee.DATHOIVIEC === 1 ? t('employee360.statusResigned') : t('employee360.statusWorking')}
              />
              {canEdit(currentUser, 'F_DM_NHANVIEN') && (
                <Upload
                  showUploadList={false}
                  accept="image/*"
                  beforeUpload={handleDirectAvatarUpload}
                >
                  <Tooltip title={t('employee360.changeAvatar')}>
                    <Button
                      shape="circle"
                      size="small"
                      icon={<CameraOutlined />}
                      loading={uploadingAvatar}
                      style={{
                        position: 'absolute',
                        bottom: 0,
                        right: -4,
                        backgroundColor: '#1e293b',
                        borderColor: '#3b82f6',
                        color: '#60a5fa',
                        boxShadow: '0 2px 6px rgba(0,0,0,0.4)',
                        zIndex: 2,
                      }}
                    />
                  </Tooltip>
                </Upload>
              )}
            </div>

            <div className="employee-360-info-col">
              <div className="employee-360-name-row">
                <Title level={3} className="employee-360-name-title">
                  {employee.HOTEN}
                </Title>
                <Tag color={employee.DATHOIVIEC === 1 ? 'error' : 'success'} style={{ fontWeight: 600 }}>
                  {employee.DATHOIVIEC === 1 ? `🔴 ${t('employee360.statusResigned')}` : `🟢 ${t('employee360.statusWorking')}`}
                </Tag>
                <Tag color="cyan">{t('employee360.empCode', { id: employee.MANV })}</Tag>
              </div>

              <div className="employee-360-meta-row">
                <span className="employee-360-meta-item">🏢 <strong>{t('employee360.department')}</strong> {employee.TENPB || t('employee360.unassigned')}</span>
                <span className="employee-360-meta-item">💼 <strong>{t('employee360.position')}</strong> {employee.TENCV || t('employee360.defaultPosition')}</span>
                <span className="employee-360-meta-item">📅 <strong>{t('employee360.hireDateLabel')}</strong> {summary?.NgayVaoCongTy || t('employee360.notRecorded')}</span>
                <span className="employee-360-meta-item">🎓 <strong>{t('employee360.education')}</strong> {employee.TENTD || t('employee360.defaultEducation')}</span>
              </div>
            </div>
          </div>

          {/* RIGHT: ACTION BUTTONS (DESKTOP PINNED RIGHT, MOBILE FULL-WIDTH ROW) */}
          <div className="employee-360-actions">
            {canEdit(currentUser, 'F_DM_NHANVIEN') && (
              <Button
                type="primary"
                icon={<EditOutlined />}
                onClick={() => {
                  onClose();
                  if (onEdit) onEdit(employee);
                }}
                style={{ background: '#3b82f6', borderColor: '#3b82f6' }}
              >
                {t('employee360.editProfile')}
              </Button>
            )}
            {canPrint(currentUser, 'F_DM_NHANVIEN') && (
              <Button
                ghost
                icon={<PrinterOutlined />}
                onClick={() => window.print()}
                style={{ color: '#cbd5e1', borderColor: '#64748b' }}
              >
                {t('employee360.exportResume')}
              </Button>
            )}
          </div>
        </div>
      </div>

      {/* BODY CONTENT WITH TABS */}
      <div className="employee-360-body">
        <Spin spinning={loadingProfile} tip={t('employee360.loadingData')}>
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={[
              {
                key: 'overview',
                label: `📑 ${t('employee360.tabOverview')}`,
                children: (
                  <Space direction="vertical" size="large" style={{ width: '100%' }}>
                    {/* METRIC STRIP */}
                    <Row gutter={[12, 12]}>
                      <Col xs={24} md={8}>
                        <Card size="small" style={{ background: tokens.cardSecondaryBg, borderColor: tokens.borderMedium }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text type="secondary">
                              {t('employee360.attendance')} {summary?.KyCongChuyenCanLabel ? `(${summary.KyCongChuyenCanLabel})` : ''}
                            </Text>
                            <ClockCircleOutlined style={{ color: tokens.successText }} />
                          </div>
                          <div style={{ fontSize: 24, fontWeight: 700, color: tokens.textPrimary, margin: '6px 0' }}>
                            {summary ? summary.TyLeChuyenCan : 0}%
                          </div>
                          <Progress
                            percent={summary ? summary.TyLeChuyenCan : 0}
                            strokeColor="#10b981"
                            showInfo={false}
                            size="small"
                          />
                          <Text type="secondary" style={{ fontSize: 11, marginTop: 4, display: 'block' }}>
                            {t('employee360.attendanceTarget', { actual: summary?.TongNgayCong ?? 0, standard: summary?.CongChuan ?? 26 })}
                          </Text>
                        </Card>
                      </Col>

                      <Col xs={24} md={8}>
                        <Card size="small" style={{ background: tokens.cardSecondaryBg, borderColor: tokens.borderMedium }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text type="secondary">{t('employee360.leaveQuota')}</Text>
                            <CalendarOutlined style={{ color: tokens.infoText }} />
                          </div>
                          <div style={{ fontSize: 24, fontWeight: 700, color: tokens.textPrimary, margin: '6px 0' }}>
                            {summary?.SoNgayNghiPhep ?? 0} / {summary?.TongQuyPhep ?? 12}{' '}
                            <span style={{ fontSize: 14, fontWeight: 400 }}>{t('employee360.daysUnit')}</span>
                          </div>
                          <Progress
                            percent={
                              summary
                                ? Math.round(((summary.SoNgayNghiPhep ?? 0) / (summary.TongQuyPhep ?? 12)) * 100)
                                : 0
                            }
                            strokeColor="#3b82f6"
                            showInfo={false}
                            size="small"
                          />
                          <Text type="secondary" style={{ fontSize: 11, marginTop: 4, display: 'block' }}>
                            {t('employee360.leaveRemaining', { days: summary?.PhepConLai ?? 12 })}
                          </Text>
                        </Card>
                      </Col>

                      <Col xs={24} md={8}>
                        <Card size="small" style={{ background: tokens.cardSecondaryBg, borderColor: tokens.borderMedium }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text type="secondary">{t('employee360.latestNetIncome')}</Text>
                            <DollarOutlined style={{ color: tokens.warningText }} />
                          </div>
                          <div style={{ fontSize: 22, fontWeight: 700, color: tokens.textPrimary, margin: '6px 0' }}>
                            {summary?.ThuNhapNetKyGanNhat != null
                              ? `${Number(summary.ThuNhapNetKyGanNhat).toLocaleString()} đ`
                              : t('employee360.noPayroll')}
                          </div>
                          <Tag color={summary?.ThuNhapNetKyGanNhat != null ? 'green' : 'default'}>
                            {summary?.KyLuongNetLabel || t('employee360.noPayrollPeriod')}
                          </Tag>
                          <Text type="secondary" style={{ fontSize: 11, marginTop: 4, display: 'block' }}>
                            {t('employee360.payoutStatus')} {summary?.TrangThaiChiTra || t('employee360.noStatus')}
                          </Text>
                        </Card>
                      </Col>
                    </Row>

                    {/* THÔNG TIN CHI TIẾT */}
                    <Card title={t('employee360.basicInfo')} size="small" style={{ borderColor: tokens.borderMedium }}>
                      <Descriptions column={{ xs: 1, sm: 2, md: 3 }} size="small" bordered>
                        <Descriptions.Item label={<Space><PhoneOutlined /> {t('employee360.phone')}</Space>}>
                          {employee.DIENTHOAI || <Text type="secondary">{t('employee360.notUpdated')}</Text>}
                        </Descriptions.Item>
                        <Descriptions.Item label={<Space><IdcardOutlined /> {t('employee360.idCard')}</Space>}>
                          {employee.CCCD || <Text type="danger">{t('employee360.missingIdCard')}</Text>}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('employee360.gender')}>
                          {(() => {
                            const rawG = employee.GIOITINH || (employee as any).TENGT;
                            const isFem = rawG === 'Nữ' || employee.IDGT === 2;
                            const isOth = rawG === 'Khác' || employee.IDGT === 3;
                            return (
                              <Tag color={isFem ? 'magenta' : 'blue'}>
                                {isFem ? t('employee360.genderFemale') : isOth ? t('employee360.genderOther') : t('employee360.genderMale')}
                              </Tag>
                            );
                          })()}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('employee360.birthDate')}>
                          {employee.NGAYSINH ? employee.NGAYSINH.split('T')[0] : t('employee360.notUpdated')}
                        </Descriptions.Item>
                        <Descriptions.Item label={<Space><HomeOutlined /> {t('employee360.address')}</Space>} span={2}>
                          {employee.DIACHI || t('employee360.notUpdated')}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('employee360.directDept')}>
                          <Tag color="blue">{employee.TENPB || t('employee360.unassigned')}</Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label={t('employee360.subDept')}>
                          {employee.TENBP || t('employee360.unassigned')}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('employee360.jobTitle')}>
                          <Tag color="geekblue">{employee.TENCV || t('employee360.defaultPosition')}</Tag>
                        </Descriptions.Item>
                      </Descriptions>
                    </Card>

                    {/* LỊCH SỬ THU NHẬP THỰC TẾ TỪ CSDL */}
                    <Card
                      title={`💵 ${t('employee360.actualPayrollHistory')}`}
                      size="small"
                      style={{ borderColor: tokens.borderMedium }}
                    >
                      <Table
                        dataSource={rawPayroll}
                        pagination={false}
                        size="small"
                        rowKey="Key"
                        scroll={{ x: 'max-content' }}
                        locale={{
                          emptyText: (
                            <Empty
                              image={Empty.PRESENTED_IMAGE_SIMPLE}
                              description={t('employee360.noPayrollInDb')}
                            />
                          ),
                        }}
                        columns={[
                          { title: t('employee360.colPeriod'), dataIndex: 'KyLuong', key: 'KyLuong' },
                          {
                            title: t('employee360.colGross'),
                            dataIndex: 'Gross',
                            key: 'Gross',
                            render: (v) => `${Number(v || 0).toLocaleString()} đ`,
                          },
                          {
                            title: t('employee360.colOt'),
                            dataIndex: 'Ot',
                            key: 'Ot',
                            render: (v) => `${Number(v || 0).toLocaleString()} đ`,
                          },
                          {
                            title: t('employee360.colNet'),
                            dataIndex: 'Net',
                            key: 'Net',
                            render: (v) => (
                              <strong style={{ color: tokens.successText }}>
                                {Number(v || 0).toLocaleString()} đ
                              </strong>
                            ),
                          },
                          {
                            title: t('employee360.colWorkDays'),
                            key: 'Cong',
                            render: (_, r) => t('employee360.workDaysUnit', { actual: r.CongThucTe, standard: r.CongChuan }),
                          },
                          {
                            title: t('employee360.colStatus'),
                            dataIndex: 'TrangThai',
                            key: 'TrangThai',
                            align: 'center',
                            render: (statusVal) => {
                              const rawStatus = (statusVal || '').toLowerCase();
                              const isPaid = rawStatus.includes('đã chi trả') || rawStatus.includes('đã thanh toán') || rawStatus.includes('paid');
                              const isLocked = rawStatus.includes('chốt sổ') || rawStatus.includes('locked');
                              const isDraft = rawStatus.includes('nháp') || rawStatus.includes('draft');
                              const displayStatus = isPaid
                                ? t('employee360.statusPaid')
                                : isLocked
                                ? t('employee360.statusLockedAwaiting')
                                : isDraft
                                ? t('employee360.statusDraft')
                                : (statusVal ? t(`status.${statusVal}`) || statusVal : t('employee360.statusPending'));
                              return (
                                <Tag color={isPaid ? 'success' : isLocked ? 'processing' : 'warning'}>
                                  {displayStatus}
                                </Tag>
                              );
                            },
                          },
                          {
                            title: t('employee360.colDetail'),
                            key: 'Action',
                            align: 'center',
                            render: (_, r) => {
                              let month = 4;
                              let year = 2026;
                              if (r.KyLuong && r.KyLuong.includes('/')) {
                                const parts = r.KyLuong.replace('T', '').split('/');
                                month = parseInt(parts[0], 10) || 4;
                                year = parseInt(parts[1], 10) || 2026;
                              }
                              const makycong = year * 100 + month;
                              const canPrintPayslip = canPrint(currentUser, 'F_CC_BANGLUONG');
                              return (
                                <Tooltip title={!canPrintPayslip ? t('employee360.noPrintPermission') : undefined}>
                                  <Button
                                    type="link"
                                    size="small"
                                    disabled={!canPrintPayslip}
                                    icon={<PrinterOutlined />}
                                    onClick={() => {
                                      if (!canPrintPayslip) return;
                                      setSelectedPayslipRecord({
                                        MANV: employee.MANV,
                                        HOTEN: employee.HOTEN,
                                        TENPB: employee.TENPB,
                                        MAKYCONG: makycong,
                                        THANG: month,
                                        NAM: year,
                                        CONG_CHUAN: r.CongChuan,
                                        CONG_THUCTE: r.CongThucTe,
                                        THUC_LINH: r.Net,
                                        LUONG_CONG_THUCTE: r.Gross,
                                        TIEN_TANGCA: r.Ot,
                                      } as any);
                                      setPayslipModalVisible(true);
                                    }}
                                    style={{ fontWeight: 600, color: canPrintPayslip ? '#1d4ed8' : '#94a3b8' }}
                                  >
                                    {t('employee360.printPayslip')}
                                  </Button>
                                </Tooltip>
                              );
                            },
                          },
                        ]}
                      />
                    </Card>
                  </Space>
                ),
              },
              {
                key: 'timeline',
                label: `🕒 ${t('employee360.tabTimeline')}`,
                children: (
                  <div style={{ padding: '12px 16px' }}>
                    <Paragraph type="secondary">
                      {t('employee360.timelineDesc')}
                    </Paragraph>
                    {timelineItems.length > 0 ? (
                      <Timeline
                        mode="left"
                        items={timelineItems.map((item, idx) => ({
                          key: idx,
                          label: <span style={{ fontWeight: 600, color: tokens.textSecondary, fontSize: 13 }}>{item.Ngay}</span>,
                          color: timelineColor(item.Loai).text,
                          dot: renderTimelineDot(item.IconType),
                          children: (
                            <div style={{ background: tokens.cardSecondaryBg, border: `1px solid ${tokens.borderMedium}`, borderRadius: 8, padding: '10px 14px', marginBottom: 8 }}>
                              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8, marginBottom: 4, flexWrap: 'wrap' }}>
                                <Text strong style={{ color: tokens.textPrimary }}>{timelineLabels(item, t).title}</Text>
                                <Tag color={item.TagColor || 'blue'} style={{ borderRadius: 4, margin: 0, color: timelineColor(item.Loai).text, background: timelineColor(item.Loai).background, borderColor: tokens.borderMedium }}>
                                  {timelineLabels(item, t).label || t('employee360.eventDefault')}
                                </Tag>
                              </div>
                              <Paragraph type="secondary" style={{ margin: 0, fontSize: 13, color: tokens.textSecondary }}>
                                {timelineLabels(item, t).description}
                              </Paragraph>
                            </div>
                          ),
                        }))}
                      />
                    ) : (
                      <Empty
                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                        description={t('employee360.noTimelineEvents')}
                      />
                    )}
                  </div>
                ),
              },
              {
                key: 'onboarding',
                label: `✅ ${t('employee360.tabOnboarding')}`,
                children: (
                  <div style={{ padding: '8px 12px' }}>
                    <div
                      style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        marginBottom: 16,
                      }}
                    >
                      <div>
                        <Title level={5} style={{ margin: 0 }}>
                          {t('employee360.onboardingTitle')}
                        </Title>
                        <Text type="secondary">
                          {t('employee360.onboardingDesc')}
                        </Text>
                      </div>
                      <div style={{ textAlign: 'right' }}>
                        <span
                          style={{
                            fontSize: 20,
                            fontWeight: 700,
                            color: onboardingPercent === 100 ? tokens.successText : tokens.infoText,
                          }}
                        >
                          {onboardingPercent}%
                        </span>
                        <Progress percent={onboardingPercent} style={{ width: 140, display: 'block' }} size="small" />
                      </div>
                    </div>

                    <Card size="small" style={{ background: tokens.cardSecondaryBg, borderColor: tokens.borderMedium }}>
                      <Space direction="vertical" style={{ width: '100%' }} size="middle">
                        {checklistItems.map((item, idx) => (
                          <div
                            key={idx}
                            style={{
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'space-between',
                              padding: '8px 12px',
                              background: tokens.cardBg,
                              borderRadius: 6,
                              border: `1px solid ${tokens.borderSubtle}`,
                              gap: 8,
                              flexWrap: 'wrap',
                            }}
                          >
                            <Checkbox checked={item.Done} disabled style={{ fontWeight: item.Done ? 500 : 400 }}>
                              <span
                                style={{
                                  textDecoration: item.Done ? 'line-through' : 'none',
                                  color: item.Done ? tokens.textSecondary : tokens.textPrimary,
                                }}
                              >
                                {checklistLabels(item, t).title}
                              </span>
                            </Checkbox>
                            <Space wrap>
                              <Text type="secondary" style={{ fontSize: 12, color: tokens.textSecondary }}>
                                {checklistLabels(item, t).detail}
                              </Text>
                              {item.Done ? (
                                <Tag color="success" style={{ background: tokens.successBg, color: tokens.successText, borderColor: tokens.borderMedium }}>{t('employee360.completed')}</Tag>
                              ) : (
                                <Tag color="warning" style={{ background: tokens.warningBg, color: tokens.warningText, borderColor: tokens.borderMedium }}>{t('employee360.notCompleted')}</Tag>
                              )}
                            </Space>
                          </div>
                        ))}
                      </Space>
                    </Card>
                  </div>
                ),
              },
              {
                key: 'account',
                label: `🔐 ${t('employee360.tabAccount')}`,
                children: (
                  <div style={{ padding: '8px 12px' }}>
                    <Alert
                      message={t('employee360.accountAlertTitle')}
                      description={t('employee360.accountAlertDesc')}
                      type="info"
                      showIcon
                      style={{ marginBottom: 16 }}
                    />

                    <Card size="small" style={{ background: tokens.cardSecondaryBg, borderColor: tokens.borderMedium, borderRadius: 8 }}>
                      <Descriptions bordered size="middle" column={{ xs: 1, sm: 2 }}>
                        <Descriptions.Item label={t('employee360.empCodeLabel')}>
                          <Text strong style={{ color: tokens.infoText, fontSize: 14 }}>
                            <IdcardOutlined style={{ marginRight: 6 }} />
                            {employee?.EMPLOYEE_CODE || employee?.EmployeeCode || `NV${String(employee?.MANV).padStart(6, '0')}`}
                          </Text>
                        </Descriptions.Item>

                        <Descriptions.Item label={t('employee360.fullName')}>
                          <Text strong>{employee?.HOTEN}</Text>
                        </Descriptions.Item>

                        <Descriptions.Item label={t('employee360.linkedAccount')}>
                          {linkedAccount ? (
                            <Space>
                              <Tag color="blue" icon={<UserOutlined />}>
                                {linkedAccount.Username}
                              </Tag>
                              <Text type="secondary">({linkedAccount.FullName})</Text>
                            </Space>
                          ) : (
                            <Tag color="orange">{t('employee360.notLinked')}</Tag>
                          )}
                        </Descriptions.Item>

                        <Descriptions.Item label={t('employee360.accountStatus')}>
                          {linkedAccount ? (
                            linkedAccount.Disabled ? (
                              <Tag color="red">{t('employee360.accountSuspended')}</Tag>
                            ) : (
                              <Tag color="green">{t('employee360.accountActive')}</Tag>
                            )
                          ) : (
                            <Text type="secondary">N/A</Text>
                          )}
                        </Descriptions.Item>

                        <Descriptions.Item label={t('employee360.mobileAccess')} span={2}>
                          {linkedAccount ? (
                            <Space size="middle" wrap>
                              <Switch
                                checked={
                                  linkedAccount.IsMobileEnabled !== undefined
                                    ? linkedAccount.IsMobileEnabled
                                    : linkedAccount.ClientType !== 'DESKTOP'
                                }
                                loading={togglingMobile}
                                checkedChildren={<Space><MobileOutlined />{t('employee360.switchOn')}</Space>}
                                unCheckedChildren={t('employee360.switchOff')}
                                onChange={async (checked) => {
                                  try {
                                    setTogglingMobile(true);
                                    await api.post(`/users/${linkedAccount.IdUser}/toggle-mobile`, {
                                      IsMobileEnabled: checked,
                                    });
                                    setLinkedAccount({
                                      ...linkedAccount,
                                      IsMobileEnabled: checked,
                                    });
                                    notification.success({
                                      message: t('employee360.toggleMobileSuccess'),
                                      description: t('employee360.toggleMobileSuccessDesc', {
                                        action: checked ? t('employee360.actionActivated') : t('employee360.actionDeactivated'),
                                        username: linkedAccount.Username,
                                      }),
                                    });
                                  } catch {
                                    notification.error({
                                      message: t('employee360.toggleMobileError'),
                                      description: t('employee360.toggleMobileErrorDesc'),
                                    });
                                  } finally {
                                    setTogglingMobile(false);
                                  }
                                }}
                              />
                              <Text type="secondary" style={{ fontSize: 13 }}>
                                {linkedAccount.IsMobileEnabled !== false && linkedAccount.ClientType !== 'DESKTOP'
                                  ? t('employee360.mobileEnabledDesc')
                                  : t('employee360.mobileDisabledDesc')}
                              </Text>
                            </Space>
                          ) : (
                            <Text type="secondary" italic>
                              {t('employee360.needLinkFirst')}
                            </Text>
                          )}
                        </Descriptions.Item>
                      </Descriptions>
                    </Card>
                  </div>
                ),
              },
            ]}
          />
        </Spin>
      </div>
    </Modal>

      <PhieuLuongModal
        printAllowed={canPrint(currentUser, 'F_CC_BANGLUONG')}
        visible={payslipModalVisible}
        onClose={() => setPayslipModalVisible(false)}
        record={selectedPayslipRecord}
      />
    </>
  );
};

export default Employee360Modal;
