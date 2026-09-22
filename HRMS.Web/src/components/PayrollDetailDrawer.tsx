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

  const luongCoBan = (record.DAILY_RATE ?? 0) * (record.CONG_CHUAN || 22);
  const phuCap = record.PHUCAP_CONG_THUCTE ?? 1500000;
  const ot = record.TIEN_TANGCA ?? 800000;
  const gross = (record.LUONG_CONG_THUCTE ?? luongCoBan) + phuCap + ot + (record.TIEN_CHUYENCAN ?? 0) + (record.TIEN_AN_CA ?? 0);
  const bhxh = record.TIEN_BHXH_TRICH ?? Math.round(gross * 0.08);
  const thue = Math.round(gross * 0.03);
  const tamUng = record.TIEN_TAMUNG ?? 0;
  const net = record.THUC_LINH ?? (gross - bhxh - thue - tamUng);

  return (
    <Drawer
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <DollarOutlined style={{ color: '#10b981', fontSize: 20 }} />
          <span>Chi tiết Bảng lương Gross-to-Net</span>
        </div>
      }
      placement="right"
      width="min(480px, 95vw)"
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
          borderRadius: 8,
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
          <Tag color="success" style={{ fontSize: 12, padding: '4px 8px' }}>
            <CheckCircleOutlined /> Đã tính lương
          </Tag>
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
              Mã NV: #{record.MANV} &bull; Mã kỳ công: {record.MAKYCONG}
            </div>
          </div>
        </div>
      </Card>

      {/* GROSS BREAKDOWN */}
      <div style={{ marginBottom: 20 }}>
        <Text strong style={{ fontSize: 13, textTransform: 'uppercase', color: '#64748b' }}>
          1. Thu nhập Gross (Tổng các khoản thu nhập)
        </Text>
        <div style={{ background: '#f8fafc', borderRadius: 8, padding: 14, marginTop: 8, border: '1px solid #e2e8f0' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Lương công thực tế ({record.CONG_THUCTE || 22} công)</Text>
            <Text strong>{(record.LUONG_CONG_THUCTE ?? luongCoBan).toLocaleString('vi-VN')} đ</Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Phụ cấp cố định & chức vụ</Text>
            <Text strong>{phuCap.toLocaleString('vi-VN')} đ</Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Làm thêm giờ (OT)</Text>
            <Text strong style={{ color: '#3b82f6' }}>+{ot.toLocaleString('vi-VN')} đ</Text>
          </div>
          {Boolean(record.TIEN_CHUYENCAN) && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Tiền chuyên cần</Text>
              <Text strong>{(record.TIEN_CHUYENCAN ?? 0).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          {Boolean(record.TIEN_AN_CA) && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Tiền ăn ca</Text>
              <Text strong>{(record.TIEN_AN_CA ?? 0).toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          <Divider style={{ margin: '8px 0' }} />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 15 }}>
            <Text strong>TỔNG THU NHẬP GROSS</Text>
            <Text strong style={{ color: '#0f172a' }}>{gross.toLocaleString('vi-VN')} đ</Text>
          </div>
        </div>
      </div>

      {/* DEDUCTIONS */}
      <div style={{ marginBottom: 20 }}>
        <Text strong style={{ fontSize: 13, textTransform: 'uppercase', color: '#64748b' }}>
          2. Các khoản trích trừ bắt buộc
        </Text>
        <div style={{ background: '#fef2f2', borderRadius: 8, padding: 14, marginTop: 8, border: '1px solid #fee2e2' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Trích bảo hiểm xã hội, BHYT, BHTN</Text>
            <Text strong style={{ color: '#ef4444' }}>-{bhxh.toLocaleString('vi-VN')} đ</Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text>Thuế thu nhập cá nhân (TNCN)</Text>
            <Text strong style={{ color: '#ef4444' }}>-{thue.toLocaleString('vi-VN')} đ</Text>
          </div>
          {tamUng > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Text>Khấu trừ tạm ứng lương</Text>
              <Text strong style={{ color: '#ef4444' }}>-{tamUng.toLocaleString('vi-VN')} đ</Text>
            </div>
          )}
          <Divider style={{ margin: '8px 0' }} />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 14 }}>
            <Text strong style={{ color: '#991b1b' }}>TỔNG KHOẢN KHẤU TRỪ</Text>
            <Text strong style={{ color: '#ef4444' }}>
              -{(bhxh + thue + tamUng).toLocaleString('vi-VN')} đ
            </Text>
          </div>
        </div>
      </div>

      {/* NET RESULT */}
      <Card
        style={{
          background: '#ecfdf5',
          border: '1px solid #a7f3d0',
          borderRadius: 8,
          marginBottom: 24,
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
          <div>
            <Text type="secondary" style={{ fontSize: 13 }}>THỰC LĨNH NHÂN VIÊN</Text>
            <Title level={2} style={{ color: '#059669', margin: '4px 0 0 0' }}>
              {net.toLocaleString('vi-VN')} <span style={{ fontSize: 16 }}>VNĐ</span>
            </Title>
          </div>
          <Tag color="success" style={{ fontSize: 13, padding: '4px 10px' }}>
            NET SALARY
          </Tag>
        </div>
      </Card>

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
