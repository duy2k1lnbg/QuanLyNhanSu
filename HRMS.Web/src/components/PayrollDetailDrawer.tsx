import React from 'react';
import {
  Drawer,
  Typography,
  Divider,
  Button,
  Space,
  Tag,
  Card,
} from 'antd';
import {
  PrinterOutlined,
  FilePdfOutlined,
  DollarOutlined,
  UserOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';
import type { BangLuongDTO } from '../types/hrms';

const { Title, Text } = Typography;

interface PayrollDetailDrawerProps {
  visible: boolean;
  onClose: () => void;
  record: BangLuongDTO | null;
  onOpenFullModal?: (record: BangLuongDTO) => void;
}

export const PayrollDetailDrawer: React.FC<PayrollDetailDrawerProps> = ({
  visible,
  onClose,
  record,
  onOpenFullModal,
}) => {
  if (!record) return null;

  const congChuan = record.CONG_CHUAN || 26;
  const congThucTe = record.CONG_THUCTE ?? 0;
  const dailyRate = record.DAILY_RATE ?? 0;
  const luongCoBan = dailyRate * congChuan;
  const luongCongThucTe = record.LUONG_CONG_THUCTE ?? (dailyRate * congThucTe);
  const phuCap = record.PHUCAP_CONG_THUCTE ?? 0;
  const ot = record.TIEN_TANGCA ?? 0;
  const tienChuyenCan = record.TIEN_CHUYENCAN ?? 0;
  const tienAnCa = record.TIEN_AN_CA ?? 0;
  const khoanCongKhac = record.KHOAN_CONG_KHAC ?? 0;
  const luongCaDem = record.LUONG_CA_DEM ?? 0;

  const gross = record.TONG_CONG ?? (luongCongThucTe + phuCap + ot + tienChuyenCan + tienAnCa + khoanCongKhac);

  // Khấu trừ NLĐ
  const luongDongBhxh = record.LUONG_DONG_BHXH ?? record.LUONG_BHXH ?? 0;
  const bhxhTrich = record.TIEN_BHXH_TRICH ?? ((record.TIEN_BHXH ?? 0) + (record.TIEN_BHYT ?? 0) + (record.TIEN_BHTN ?? 0));
  const doanPhi = record.TIEN_DOAN_PHI_NLD ?? record.TIEN_CONG_DOAN ?? 0;
  const thueTncn = record.THUE_TNCN ?? 0;
  const tamUng = record.TIEN_TAMUNG ?? 0;
  const khoanTruKhac = record.KHOAN_TRU_KHAC ?? 0;
  const totalDeductions = bhxhTrich + doanPhi + thueTncn + tamUng + khoanTruKhac;

  const net = record.THUC_LINH ?? (gross - totalDeductions);

  // Chi phí NSDLĐ
  const hasEmployerData = Boolean(
    record.TONG_CHI_PHI_NSDLD ||
    record.TIEN_BHXH_NSDLD ||
    record.TIEN_KINH_PHI_CD_NSDLD
  );
  const bhxhNsdld = record.TIEN_BHXH_NSDLD ?? 0;
  const bhytNsdld = record.TIEN_BHYT_NSDLD ?? 0;
  const bhtnNsdld = record.TIEN_BHTN_NSDLD ?? 0;
  const tnldNsdld = record.TIEN_TNLD_BNN_NSDLD ?? 0;
  const kpcdNsdld = record.TIEN_KINH_PHI_CD_NSDLD ?? 0;
  const tongChiPhiNsdld = record.TONG_CHI_PHI_NSDLD ?? (gross + bhxhNsdld + bhytNsdld + bhtnNsdld + tnldNsdld + kpcdNsdld);

  return (
    <Drawer
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <DollarOutlined style={{ color: '#10b981', fontSize: 20 }} />
          <span>Chi tiết Bảng lương Gross-to-Net</span>
        </div>
      }
      placement="right"
      width="min(520px, 95vw)"
      open={visible}
      onClose={onClose}
      extra={
        <Space>
          <Button
            type="primary"
            icon={<PrinterOutlined />}
            onClick={() => {
              if (onOpenFullModal) onOpenFullModal(record);
            }}
          >
            In Payslip
          </Button>
        </Space>
      }
    >
      {/* HEADER CARD */}
      <Card
        style={{
          background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)',
          color: '#fff',
          borderRadius: 10,
          marginBottom: 20,
          border: 'none',
        }}
        size="small"
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <Text style={{ color: '#94a3b8', fontSize: 12 }}>Kỳ thanh toán</Text>
            <Title level={4} style={{ color: '#fff', margin: 0 }}>
              Tháng {record.THANG}/{record.NAM}
            </Title>
          </div>
          <div>
            {record.IS_LEGACY === 1 ? (
              <Tag color="default" style={{ fontSize: 11, padding: '2px 8px' }}>
                Legacy (Bảo toàn)
              </Tag>
            ) : record.TRANG_THAI === 'APPROVED' ? (
              <Tag color="success" style={{ fontSize: 11, padding: '2px 8px' }}>
                <CheckCircleOutlined /> Đã duyệt & khóa sổ
              </Tag>
            ) : record.TRANG_THAI === 'DRAFT' ? (
              <Tag color="processing" style={{ fontSize: 11, padding: '2px 8px' }}>
                Dự thảo (Draft)
              </Tag>
            ) : (
              <Tag color="success" style={{ fontSize: 11, padding: '2px 8px' }}>
                <CheckCircleOutlined /> {record.TRANG_THAI || 'Đã tính lương'}
              </Tag>
            )}
          </div>
        </div>

        <Divider style={{ borderColor: 'rgba(255,255,255,0.15)', margin: '12px 0' }} />

        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <div
            style={{
              width: 44,
              height: 44,
              borderRadius: '50%',
              backgroundColor: '#3b82f6',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#fff',
              fontWeight: 700,
              fontSize: 16,
            }}
          >
            <UserOutlined />
          </div>
          <div>
            <Text strong style={{ color: '#fff', fontSize: 16 }}>
              {record.HOTEN}
            </Text>
            <div style={{ color: '#94a3b8', fontSize: 12 }}>
              Mã NV: #{record.MANV} &bull; Phòng ban: {record.TENPB || 'Chưa phân bổ'}
            </div>
            {Boolean(record.VUNG_LUONG) && (
              <div style={{ color: '#60a5fa', fontSize: 11, marginTop: 2 }}>
                Vùng lương: Vùng {record.VUNG_LUONG} &bull; LTT vùng: {Number(record.LUONG_TOI_THIEU_VUNG || 0).toLocaleString('vi-VN')} đ
              </div>
            )}
          </div>
        </div>
      </Card>

      {/* 1. GROSS BREAKDOWN */}
      <div style={{ marginBottom: 20 }}>
        <Text strong style={{ fontSize: 13, textTransform: 'uppercase', color: '#64748b' }}>
          1. Thu nhập Gross (Tổng các khoản thu nhập)
        </Text>
        <div style={{ background: '#f8fafc', borderRadius: 8, padding: 14, marginTop: 8, border: '1px solid #e2e8f0' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <div>
              <Text>Lương công thực tế ({congThucTe}/{congChuan} công)</Text>
              {luongCoBan > 0 && (
                <div style={{ fontSize: 11, color: '#64748b' }}>
                  Lương HĐLĐ: {Number(luongCoBan).toLocaleString('vi-VN')} đ
                </div>
              )}
            </div>
            <Text strong>{Number(luongCongThucTe).toLocaleString('vi-VN')} đ</Text>
          </div>
          {luongCaDem > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Lương làm ca đêm (+30%)</Text>
              <Text strong style={{ color: '#6366f1' }}>+{Number(luongCaDem).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Phụ cấp cố định & chức vụ</Text>
            <Text strong>{Number(phuCap).toLocaleString('vi-VN')} đ</Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Làm thêm giờ (OT)</Text>
            <Text strong style={{ color: ot > 0 ? '#3b82f6' : undefined }}>
              {ot > 0 ? `+${Number(ot).toLocaleString('vi-VN')} đ` : '0 đ'}
            </Text>
          </div>
          {tienChuyenCan > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Tiền chuyên cần</Text>
              <Text strong>+{Number(tienChuyenCan).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          {tienAnCa > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Tiền ăn ca</Text>
              <Text strong>+{Number(tienAnCa).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          {khoanCongKhac > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Khoản cộng khác / thưởng</Text>
              <Text strong style={{ color: '#10b981' }}>+{Number(khoanCongKhac).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          <Divider style={{ margin: '8px 0' }} />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 15 }}>
            <Text strong>TỔNG THU NHẬP GROSS</Text>
            <Text strong style={{ color: '#0f172a' }}>{Number(gross).toLocaleString('vi-VN')} đ</Text>
          </div>
        </div>
      </div>

      {/* 2. DEDUCTIONS */}
      <div style={{ marginBottom: 20 }}>
        <Text strong style={{ fontSize: 13, textTransform: 'uppercase', color: '#64748b' }}>
          2. Các khoản trích trừ bắt buộc
        </Text>
        <div style={{ background: '#fef2f2', borderRadius: 8, padding: 14, marginTop: 8, border: '1px solid #fee2e2' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
            <div>
              <Text>Trích bảo hiểm NLĐ (10.5%)</Text>
              {luongDongBhxh > 0 && (
                <div style={{ fontSize: 11, color: '#991b1b' }}>
                  Lương đóng BHXH: {Number(luongDongBhxh).toLocaleString('vi-VN')} đ
                </div>
              )}
            </div>
            <Text strong style={{ color: bhxhTrich > 0 ? '#ef4444' : undefined }}>
              {bhxhTrich > 0 ? `-${Number(bhxhTrich).toLocaleString('vi-VN')} đ` : '0 đ'}
            </Text>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
            <Text>Đoàn phí công đoàn (0.5%)</Text>
            <Text strong style={{ color: doanPhi > 0 ? '#ef4444' : undefined }}>
              {doanPhi > 0 ? `-${Number(doanPhi).toLocaleString('vi-VN')} đ` : '0 đ'}
            </Text>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
            <div>
              <Text>Thuế thu nhập cá nhân (TNCN)</Text>
              {record.THU_NHAP_TINH_THUE !== undefined && record.THU_NHAP_TINH_THUE !== null && (
                <div style={{ fontSize: 11, color: '#991b1b' }}>
                  TNTT: {Number(record.THU_NHAP_TINH_THUE).toLocaleString('vi-VN')} đ &bull; NPT: {record.SO_NGUOI_PHU_THUOC ?? 0}
                </div>
              )}
            </div>
            <Text strong style={{ color: thueTncn > 0 ? '#ef4444' : undefined }}>
              {thueTncn > 0 ? `-${Number(thueTncn).toLocaleString('vi-VN')} đ` : '0 đ'}
            </Text>
          </div>

          {tamUng > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
              <Text>Khấu trừ tạm ứng lương</Text>
              <Text strong style={{ color: '#ef4444' }}>-{Number(tamUng).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}

          {khoanTruKhac > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
              <Text>Khấu trừ kỷ luật / khoản trừ khác</Text>
              <Text strong style={{ color: '#ef4444' }}>-{Number(khoanTruKhac).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}

          <Divider style={{ margin: '8px 0' }} />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 14 }}>
            <Text strong style={{ color: '#991b1b' }}>TỔNG KHOẢN KHẤU TRỪ</Text>
            <Text strong style={{ color: '#ef4444' }}>
              -{Number(totalDeductions).toLocaleString('vi-VN')} đ
            </Text>
          </div>
        </div>
      </div>

      {/* 3. NET RESULT */}
      <Card
        style={{
          background: '#ecfdf5',
          border: '1px solid #a7f3d0',
          borderRadius: 8,
          marginBottom: 20,
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
          <div>
            <Text type="secondary" style={{ fontSize: 13 }}>THỰC LĨNH NHÂN VIÊN</Text>
            <Title level={2} style={{ color: '#059669', margin: '4px 0 0 0' }}>
              {Number(net).toLocaleString('vi-VN')} <span style={{ fontSize: 16 }}>VNĐ</span>
            </Title>
          </div>
          <Tag color="success" style={{ fontSize: 13, padding: '4px 10px' }}>
            NET SALARY
          </Tag>
        </div>
      </Card>

      {/* 4. EMPLOYER COSTS (CHI PHÍ NSDLĐ) */}
      {hasEmployerData && (
        <div style={{ marginBottom: 24 }}>
          <Text strong style={{ fontSize: 13, textTransform: 'uppercase', color: '#64748b' }}>
            3. Chi phí Người sử dụng lao động (Doanh nghiệp chi trả)
          </Text>
          <div style={{ background: '#f0fdf4', borderRadius: 8, padding: 14, marginTop: 8, border: '1px solid #bbf7d0' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
              <Text>BHXH, BHYT, BHTN NSDLĐ đóng</Text>
              <Text strong style={{ color: '#047857' }}>
                +{Number(bhxhNsdld + bhytNsdld + bhtnNsdld + tnldNsdld).toLocaleString('vi-VN')} đ
              </Text>
            </div>
            {kpcdNsdld > 0 && (
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <Text>Kinh phí công đoàn NSDLĐ (2%)</Text>
                <Text strong style={{ color: '#047857' }}>
                  +{Number(kpcdNsdld).toLocaleString('vi-VN')} đ
                </Text>
              </div>
            )}
            <Divider style={{ margin: '8px 0' }} />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 14 }}>
              <Text strong style={{ color: '#065f46' }}>TỔNG CHI PHÍ NSDLĐ</Text>
              <Text strong style={{ color: '#047857' }}>
                {Number(tongChiPhiNsdld).toLocaleString('vi-VN')} đ
              </Text>
            </div>
          </div>
        </div>
      )}

      {/* ACTIONS */}
      <Space style={{ width: '100%' }} direction="vertical">
        <Button
          type="primary"
          block
          icon={<PrinterOutlined />}
          size="large"
          style={{ background: '#0f172a', borderColor: '#0f172a' }}
          onClick={() => {
            if (onOpenFullModal) onOpenFullModal(record);
          }}
        >
          Xem Phiếu lương A4 & In ấn
        </Button>
        <Button
          block
          icon={<FilePdfOutlined />}
          onClick={() => window.print()}
        >
          Xuất tệp PDF bảng lương
        </Button>
      </Space>
    </Drawer>
  );
};

export default PayrollDetailDrawer;
