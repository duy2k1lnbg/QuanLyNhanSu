import React from 'react';
import { Modal, Typography, Row, Col, Divider, Tag, Button, Space, Card } from 'antd';
import { PrinterOutlined, DollarOutlined, UserOutlined, CalendarOutlined } from '@ant-design/icons';
import type { BangLuongDTO } from '../types/hrms';

const { Title, Text } = Typography;

interface PhieuLuongModalProps {
  visible: boolean;
  onClose: () => void;
  record: BangLuongDTO | null;
  kyCongLabel?: string;
}

export const PhieuLuongModal: React.FC<PhieuLuongModalProps> = ({
  visible,
  onClose,
  record,
  kyCongLabel,
}) => {
  if (!record) return null;

  const handlePrint = () => {
    window.print();
  };

  const tongThuNhap =
    (record.LUONG_CONG_THUCTE || 0) +
    (record.PHUCAP_CONG_THUCTE || 0) +
    (record.TIEN_TANGCA || 0) +
    (record.TIEN_CHUYENCAN || 0) +
    (record.TIEN_AN_CA || 0) +
    (record.KHOAN_CONG_KHAC || 0);

  const tongKhauTru =
    (record.TIEN_BHXH_TRICH || 0) + (record.TIEN_TAMUNG || 0) + (record.KHOAN_TRU_KHAC || 0);

  return (
    <Modal
      title={
        <Space>
          <DollarOutlined style={{ color: '#52c41a' }} />
          <span>Chi Tiết Phiếu Lương Cá Nhân</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width={680}
      footer={[
        <Button key="close" onClick={onClose}>
          Đóng
        </Button>,
        <Button key="print" type="primary" icon={<PrinterOutlined />} onClick={handlePrint}>
          In Phiếu Lương
        </Button>,
      ]}
    >
      <div id="printable-payslip" style={{ padding: '8px 4px' }}>
        {/* Header phiếu */}
        <div style={{ textAlign: 'center', marginBottom: 20 }}>
          <Text type="secondary" style={{ fontSize: 12, textTransform: 'uppercase', letterSpacing: 1 }}>
            CÔNG TY CỔ PHẦN PHÁT TRIỂN NGUỒN NHÂN LỰC HRMS
          </Text>
          <Title level={3} style={{ margin: '4px 0 2px', color: '#1677ff' }}>
            PHIẾU THANH TOÁN LƯƠNG
          </Title>
          <Tag color="blue" icon={<CalendarOutlined />}>
            {kyCongLabel || `Kỳ Lương Tháng ${record.THANG}/${record.NAM}`}
          </Tag>
        </div>

        {/* Thông tin nhân viên */}
        <Card size="small" style={{ marginBottom: 16, background: '#f8fafc', borderRadius: 8 }}>
          <Row gutter={[16, 8]}>
            <Col span={12}>
              <Space>
                <UserOutlined style={{ color: '#64748b' }} />
                <Text type="secondary">Mã nhân viên:</Text>
                <Tag color="geekblue">#{record.MANV}</Tag>
              </Space>
            </Col>
            <Col span={12}>
              <Text type="secondary">Họ và tên: </Text>
              <Text strong style={{ fontSize: 15 }}>{record.HOTEN}</Text>
            </Col>
            <Col span={12}>
              <Text type="secondary">Số ngày công chuẩn: </Text>
              <Text strong>{record.CONG_CHUAN ?? 26} ngày</Text>
            </Col>
            <Col span={12}>
              <Text type="secondary">Số ngày công thực tế: </Text>
              <Tag color="cyan" style={{ fontWeight: 600 }}>
                {record.CONG_THUCTE ?? 0} ngày
              </Tag>
            </Col>
          </Row>
        </Card>

        {/* Chi tiết 2 cột thu nhập và khấu trừ */}
        <Row gutter={16}>
          {/* CỘT THU NHẬP */}
          <Col span={12}>
            <Card
              size="small"
              title={<span style={{ color: '#1677ff', fontWeight: 600 }}>1. CÁC KHOẢN THU NHẬP (+)</span>}
              style={{ height: '100%', borderRadius: 8, borderColor: '#bfdbfe' }}
            >
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Lương công thực tế:</Text>
                  <Text strong>{(record.LUONG_CONG_THUCTE || 0).toLocaleString('vi-VN')} đ</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Phụ cấp công việc:</Text>
                  <Text strong>{(record.PHUCAP_CONG_THUCTE || 0).toLocaleString('vi-VN')} đ</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Tiền làm thêm (OT):</Text>
                  <Text strong>{(record.TIEN_TANGCA || 0).toLocaleString('vi-VN')} đ</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Phụ cấp ăn ca:</Text>
                  <Text strong>{(record.TIEN_AN_CA || 0).toLocaleString('vi-VN')} đ</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Tiền chuyên cần:</Text>
                  <Text strong>{(record.TIEN_CHUYENCAN || 0).toLocaleString('vi-VN')} đ</Text>
                </div>
                {Boolean(record.KHOAN_CONG_KHAC) && (
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text>Khoản cộng khác:</Text>
                    <Text strong>{(record.KHOAN_CONG_KHAC || 0).toLocaleString('vi-VN')} đ</Text>
                  </div>
                )}
                <Divider style={{ margin: '8px 0' }} />
                <div style={{ display: 'flex', justifyContent: 'space-between', color: '#1677ff' }}>
                  <Text strong style={{ color: '#1677ff' }}>TỔNG THU NHẬP:</Text>
                  <Text strong style={{ color: '#1677ff' }}>{tongThuNhap.toLocaleString('vi-VN')} đ</Text>
                </div>
              </div>
            </Card>
          </Col>

          {/* CỘT KHẤU TRỪ */}
          <Col span={12}>
            <Card
              size="small"
              title={<span style={{ color: '#ff4d4f', fontWeight: 600 }}>2. CÁC KHOẢN KHẤU TRỪ (-)</span>}
              style={{ height: '100%', borderRadius: 8, borderColor: '#fecaca' }}
            >
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Trích đóng BHXH (10.5%):</Text>
                  <Text strong type="danger">
                    -{(record.TIEN_BHXH_TRICH || 0).toLocaleString('vi-VN')} đ
                  </Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Tạm ứng trong tháng:</Text>
                  <Text strong type="danger">
                    -{(record.TIEN_TAMUNG || 0).toLocaleString('vi-VN')} đ
                  </Text>
                </div>
                {Boolean(record.KHOAN_TRU_KHAC) && (
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text>Khấu trừ khác:</Text>
                    <Text strong type="danger">
                      -{(record.KHOAN_TRU_KHAC || 0).toLocaleString('vi-VN')} đ
                    </Text>
                  </div>
                )}
                <div style={{ height: 62 }} />
                <Divider style={{ margin: '8px 0' }} />
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text strong style={{ color: '#ff4d4f' }}>TỔNG KHẤU TRỪ:</Text>
                  <Text strong style={{ color: '#ff4d4f' }}>-{tongKhauTru.toLocaleString('vi-VN')} đ</Text>
                </div>
              </div>
            </Card>
          </Col>
        </Row>

        {/* THỰC LĨNH BANNER */}
        <Card
          size="small"
          style={{
            marginTop: 16,
            background: 'linear-gradient(135deg, #f6ffed 0%, #d9f7be 100%)',
            borderColor: '#b7eb8f',
            borderRadius: 8,
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '6px 12px' }}>
            <div>
              <Text strong style={{ fontSize: 16, color: '#237804' }}>
                THỰC LĨNH (NET PAY):
              </Text>
              <div style={{ fontSize: 11, color: '#52c41a' }}>
                Số tiền được chuyển khoản vào tài khoản cá nhân
              </div>
            </div>
            <Title level={2} style={{ margin: 0, color: '#52c41a', fontWeight: 800 }}>
              {(record.THUC_LINH || 0).toLocaleString('vi-VN')} VNĐ
            </Title>
          </div>
        </Card>

        <div style={{ marginTop: 20, textAlign: 'center' }}>
          <Text type="secondary" style={{ fontSize: 12, fontStyle: 'italic' }}>
            Mọi thắc mắc về bảng tính lương vui lòng phản hồi phòng Nhân sự - Kế toán trong vòng 3 ngày làm việc.
          </Text>
        </div>
      </div>
    </Modal>
  );
};

export default PhieuLuongModal;
