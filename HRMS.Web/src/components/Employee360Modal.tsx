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
} from '@ant-design/icons';
import api from '../services/api';
import type { NhanVienDTO, BangLuongDTO, CurrentUserDTO } from '../types/hrms';
import { canEdit, canPrint } from '../utils/permissionUtils';
import PhieuLuongModal from './PhieuLuongModal';

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
  const [activeTab, setActiveTab] = useState<string>('overview');
  const [uploadingAvatar, setUploadingAvatar] = useState<boolean>(false);
  const [currentAvatar, setCurrentAvatar] = useState<string | undefined>(employee?.HINHANH);
  const [profileData, setProfileData] = useState<Profile360Data | null>(null);
  const [loadingProfile, setLoadingProfile] = useState<boolean>(false);
  const [payslipModalVisible, setPayslipModalVisible] = useState<boolean>(false);
  const [selectedPayslipRecord, setSelectedPayslipRecord] = useState<BangLuongDTO | null>(null);

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
      notification.error({ message: 'Ảnh quá lớn', description: 'Vui lòng chọn ảnh dung lượng dưới 5MB.' });
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
        notification.success({ message: 'Thành công', description: 'Đã cập nhật ảnh chân dung nhân viên thành công!' });
        if (onAvatarUpdated) onAvatarUpdated(employee.MANV, base64);
      } catch {
        notification.error({ message: 'Lỗi tải ảnh', description: 'Không thể cập nhật ảnh chân dung.' });
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
      width={1000}
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
      <div
        style={{
          background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 50%, #1e3a8a 100%)',
          color: '#ffffff',
          padding: '24px 32px',
          position: 'relative',
        }}
      >
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 20,
            paddingRight: 36, // Chừa khoảng trống riêng cho nút X đóng modal, tránh bị che
          }}
        >
          {/* LEFT: AVATAR & EMPLOYEE INFO */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 20, flex: 1, minWidth: 0 }}>
            <div style={{ position: 'relative', flexShrink: 0 }}>
              <Avatar
                size={84}
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
                title={employee.DATHOIVIEC === 1 ? 'Đã thôi việc' : 'Đang làm việc'}
              />
              {canEdit(currentUser, 'F_DM_NHANVIEN') && (
                <Upload
                  showUploadList={false}
                  accept="image/*"
                  beforeUpload={handleDirectAvatarUpload}
                >
                  <Tooltip title="Đổi ảnh chân dung nhân viên">
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

            <div style={{ minWidth: 0, flex: 1 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap', marginBottom: 6 }}>
                <Title level={3} style={{ color: '#fff', margin: 0, fontWeight: 700 }}>
                  {employee.HOTEN}
                </Title>
                <Tag color={employee.DATHOIVIEC === 1 ? 'error' : 'success'} style={{ fontWeight: 600 }}>
                  {employee.DATHOIVIEC === 1 ? '🔴 ĐÃ THÔI VIỆC' : '🟢 ĐANG LÀM VIỆC'}
                </Tag>
                <Tag color="cyan">Mã NV: #{employee.MANV}</Tag>
              </div>

              <div style={{ color: '#94a3b8', display: 'flex', gap: 16, flexWrap: 'wrap', fontSize: 13 }}>
                <span>🏢 <strong>Phòng ban:</strong> {employee.TENPB || 'Chưa phân bổ'}</span>
                <span>💼 <strong>Vị trí:</strong> {employee.TENCV || 'Nhân viên'}</span>
                <span>📅 <strong>Ngày vào công ty:</strong> {summary?.NgayVaoCongTy || 'Chưa ghi nhận'}</span>
                <span>🎓 <strong>Trình độ:</strong> {employee.TENTD || 'Đại học'}</span>
              </div>
            </div>
          </div>

          {/* RIGHT: ACTION BUTTONS (ALWAYS PINNED TO RIGHT, NEVER WRAPS UNDER AVATAR) */}
          <div style={{ flexShrink: 0, display: 'flex', flexDirection: 'column', gap: 8, alignItems: 'flex-end' }}>
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
                Sửa hồ sơ
              </Button>
            )}
            {canPrint(currentUser, 'F_DM_NHANVIEN') && (
              <Button
                ghost
                icon={<PrinterOutlined />}
                onClick={() => window.print()}
                style={{ color: '#cbd5e1', borderColor: '#64748b' }}
              >
                Xuất sơ yếu lý lịch
              </Button>
            )}
          </div>
        </div>
      </div>

      {/* BODY CONTENT WITH TABS */}
      <div style={{ padding: '20px 32px' }}>
        <Spin spinning={loadingProfile} tip="Đang tải dữ liệu thực tế từ hệ thống...">
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={[
              {
                key: 'overview',
                label: '📑 Tổng quan 360°',
                children: (
                  <Space direction="vertical" size="large" style={{ width: '100%' }}>
                    {/* METRIC STRIP */}
                    <Row gutter={16}>
                      <Col xs={24} md={8}>
                        <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text type="secondary">
                              Chuyên cần {summary?.KyCongChuyenCanLabel ? `(${summary.KyCongChuyenCanLabel})` : ''}
                            </Text>
                            <ClockCircleOutlined style={{ color: '#10b981' }} />
                          </div>
                          <div style={{ fontSize: 24, fontWeight: 700, color: '#0f172a', margin: '6px 0' }}>
                            {summary ? summary.TyLeChuyenCan : 0}%
                          </div>
                          <Progress
                            percent={summary ? summary.TyLeChuyenCan : 0}
                            strokeColor="#10b981"
                            showInfo={false}
                            size="small"
                          />
                          <Text type="secondary" style={{ fontSize: 11, marginTop: 4, display: 'block' }}>
                            {summary?.TongNgayCong ?? 0}/{summary?.CongChuan ?? 26} ngày công chuẩn đạt chỉ tiêu
                          </Text>
                        </Card>
                      </Col>

                      <Col xs={24} md={8}>
                        <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text type="secondary">Quỹ phép năm (Leave)</Text>
                            <CalendarOutlined style={{ color: '#3b82f6' }} />
                          </div>
                          <div style={{ fontSize: 24, fontWeight: 700, color: '#0f172a', margin: '6px 0' }}>
                            {summary?.SoNgayNghiPhep ?? 0} / {summary?.TongQuyPhep ?? 12}{' '}
                            <span style={{ fontSize: 14, fontWeight: 400 }}>ngày</span>
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
                            Còn lại {summary?.PhepConLai ?? 12} ngày phép có lương
                          </Text>
                        </Card>
                      </Col>

                      <Col xs={24} md={8}>
                        <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text type="secondary">Thu nhập Net kỳ gần nhất</Text>
                            <DollarOutlined style={{ color: '#f59e0b' }} />
                          </div>
                          <div style={{ fontSize: 22, fontWeight: 700, color: '#0f172a', margin: '6px 0' }}>
                            {summary?.ThuNhapNetKyGanNhat != null
                              ? `${Number(summary.ThuNhapNetKyGanNhat).toLocaleString()} đ`
                              : 'Chưa có bảng lương'}
                          </div>
                          <Tag color={summary?.ThuNhapNetKyGanNhat != null ? 'green' : 'default'}>
                            {summary?.KyLuongNetLabel || 'Chưa phát sinh kỳ lương'}
                          </Tag>
                          <Text type="secondary" style={{ fontSize: 11, marginTop: 4, display: 'block' }}>
                            Trạng thái: {summary?.TrangThaiChiTra || 'Chưa có'}
                          </Text>
                        </Card>
                      </Col>
                    </Row>

                    {/* THÔNG TIN CHI TIẾT */}
                    <Card title="Thông tin nhân sự cơ bản" size="small" style={{ borderColor: '#e2e8f0' }}>
                      <Descriptions column={{ xs: 1, sm: 2, md: 3 }} size="small" bordered>
                        <Descriptions.Item label={<Space><PhoneOutlined /> Điện thoại</Space>}>
                          {employee.DIENTHOAI || <Text type="secondary">Chưa cập nhật</Text>}
                        </Descriptions.Item>
                        <Descriptions.Item label={<Space><IdcardOutlined /> Số CCCD</Space>}>
                          {employee.CCCD || <Text type="danger">Thiếu CCCD ⚠️</Text>}
                        </Descriptions.Item>
                        <Descriptions.Item label="Giới tính">
                          <Tag color={(employee.GIOITINH || (employee as any).TENGT || (employee.IDGT === 2 ? 'Nữ' : 'Nam')) === 'Nữ' ? 'magenta' : 'blue'}>
                            {employee.GIOITINH || (employee as any).TENGT || (employee.IDGT === 2 ? 'Nữ' : (employee.IDGT === 3 ? 'Khác' : 'Nam'))}
                          </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="Ngày sinh">
                          {employee.NGAYSINH ? employee.NGAYSINH.split('T')[0] : 'Chưa cập nhật'}
                        </Descriptions.Item>
                        <Descriptions.Item label={<Space><HomeOutlined /> Địa chỉ cư trú</Space>} span={2}>
                          {employee.DIACHI || 'Chưa cập nhật'}
                        </Descriptions.Item>
                        <Descriptions.Item label="Phòng ban trực thuộc">
                          <Tag color="blue">{employee.TENPB || 'Chưa phân bổ'}</Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="Bộ phận chuyên môn">
                          {employee.TENBP || 'Chưa phân bổ'}
                        </Descriptions.Item>
                        <Descriptions.Item label="Chức vụ đảm nhiệm">
                          <Tag color="geekblue">{employee.TENCV || 'Nhân viên'}</Tag>
                        </Descriptions.Item>
                      </Descriptions>
                    </Card>

                    {/* LỊCH SỬ THU NHẬP THỰC TẾ TỪ CSDL */}
                    <Card
                      title="💵 Lịch sử chi trả lương thực tế"
                      size="small"
                      style={{ borderColor: '#e2e8f0' }}
                    >
                      <Table
                        dataSource={rawPayroll}
                        pagination={false}
                        size="small"
                        rowKey="Key"
                        locale={{
                          emptyText: (
                            <Empty
                              image={Empty.PRESENTED_IMAGE_SIMPLE}
                              description="Chưa có dữ liệu bảng lương cho nhân viên này trong CSDL Oracle"
                            />
                          ),
                        }}
                        columns={[
                          { title: 'Kỳ Lương', dataIndex: 'KyLuong', key: 'KyLuong' },
                          {
                            title: 'Thu nhập Gross',
                            dataIndex: 'Gross',
                            key: 'Gross',
                            render: (v) => `${Number(v || 0).toLocaleString()} đ`,
                          },
                          {
                            title: 'Làm thêm (OT)',
                            dataIndex: 'Ot',
                            key: 'Ot',
                            render: (v) => `${Number(v || 0).toLocaleString()} đ`,
                          },
                          {
                            title: 'Thực lĩnh (NET)',
                            dataIndex: 'Net',
                            key: 'Net',
                            render: (v) => (
                              <strong style={{ color: '#10b981' }}>
                                {Number(v || 0).toLocaleString()} đ
                              </strong>
                            ),
                          },
                          {
                            title: 'Ngày công',
                            key: 'Cong',
                            render: (_, r) => `${r.CongThucTe}/${r.CongChuan} công`,
                          },
                          {
                            title: 'Trạng thái',
                            dataIndex: 'TrangThai',
                            key: 'TrangThai',
                            align: 'center',
                            render: (t) => {
                              const isPaid = t === 'Đã chi trả' || t === 'Đã thanh toán';
                              return (
                                <Tag color={isPaid ? 'success' : 'warning'}>
                                  {isPaid ? 'Đã chi trả' : 'Chờ chi trả'}
                                </Tag>
                              );
                            },
                          },
                          {
                            title: 'Bảng lương chi tiết',
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
                                <Tooltip title={!canPrintPayslip ? 'Bạn không có quyền in phiếu lương' : undefined}>
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
                                    In Phiếu Lương
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
                label: '🕒 Dòng thời gian sự nghiệp (Timeline)',
                children: (
                  <div style={{ padding: '12px 16px' }}>
                    <Paragraph type="secondary">
                      Toàn bộ các mốc sự kiện thực tế: Hợp đồng, nâng lương, khen thưởng, điều chuyển và quyết toán lương ghi nhận trong hệ thống.
                    </Paragraph>
                    {timelineItems.length > 0 ? (
                      <Timeline
                        mode="left"
                        items={timelineItems.map((item, idx) => ({
                          key: idx,
                          label: <span style={{ fontWeight: 600, color: '#64748b', fontSize: 13 }}>{item.Ngay}</span>,
                          color: item.TagColor || 'blue',
                          dot: renderTimelineDot(item.IconType),
                          children: (
                            <div style={{ background: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: 8, padding: '10px 14px', marginBottom: 8 }}>
                              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8, marginBottom: 4, flexWrap: 'wrap' }}>
                                <Text strong style={{ color: '#0f172a' }}>{item.TieuDe}</Text>
                                <Tag color={item.TagColor || 'blue'} style={{ borderRadius: 4, margin: 0 }}>
                                  {item.Loai || 'Sự kiện'}
                                </Tag>
                              </div>
                              <Paragraph type="secondary" style={{ margin: 0, fontSize: 13 }}>
                                {item.MoTa}
                              </Paragraph>
                            </div>
                          ),
                        }))}
                      />
                    ) : (
                      <Empty
                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                        description="Chưa có sự kiện thăng tiến hay khen thưởng nào được ghi nhận cho nhân viên này"
                      />
                    )}
                  </div>
                ),
              },
              {
                key: 'onboarding',
                label: '✅ Onboarding Checklist',
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
                          Tiến độ tiếp nhận & Hoàn thiện Hồ sơ Nhân sự
                        </Title>
                        <Text type="secondary">
                          Kiểm tra tính đầy đủ của thông tin pháp lý, hợp đồng và hồ sơ lưu trữ trong CSDL.
                        </Text>
                      </div>
                      <div style={{ textAlign: 'right' }}>
                        <span
                          style={{
                            fontSize: 20,
                            fontWeight: 700,
                            color: onboardingPercent === 100 ? '#10b981' : '#3b82f6',
                          }}
                        >
                          {onboardingPercent}%
                        </span>
                        <Progress percent={onboardingPercent} style={{ width: 140, display: 'block' }} size="small" />
                      </div>
                    </div>

                    <Card size="small" style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}>
                      <Space direction="vertical" style={{ width: '100%' }} size="middle">
                        {checklistItems.map((item, idx) => (
                          <div
                            key={idx}
                            style={{
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'space-between',
                              padding: '8px 12px',
                              background: '#fff',
                              borderRadius: 6,
                              border: '1px solid #f1f5f9',
                            }}
                          >
                            <Checkbox checked={item.Done} disabled style={{ fontWeight: item.Done ? 500 : 400 }}>
                              <span
                                style={{
                                  textDecoration: item.Done ? 'line-through' : 'none',
                                  color: item.Done ? '#64748b' : '#1e293b',
                                }}
                              >
                                {item.Title}
                              </span>
                            </Checkbox>
                            <Space>
                              <Text type="secondary" style={{ fontSize: 12 }}>
                                {item.Detail}
                              </Text>
                              {item.Done ? (
                                <Tag color="success">Đã hoàn thành</Tag>
                              ) : (
                                <Tag color="warning">Chưa hoàn thành</Tag>
                              )}
                            </Space>
                          </div>
                        ))}
                      </Space>
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
        visible={payslipModalVisible}
        onClose={() => setPayslipModalVisible(false)}
        record={selectedPayslipRecord}
      />
    </>
  );
};

export default Employee360Modal;
