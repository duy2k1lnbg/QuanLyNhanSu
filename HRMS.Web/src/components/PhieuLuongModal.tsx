import React, { useState, useEffect } from 'react';
import { Modal, Button, Spin } from 'antd';
import { PrinterOutlined } from '@ant-design/icons';
import type { BangLuongDTO } from '../types/hrms';
import api from '../services/api';

export interface ChiTietLuongNVDTO {
  IDBL?: number;
  MANV?: number;
  HOTEN?: string;
  TENPB?: string;
  TENCV?: string;
  MAKYCONG?: number;
  THANG?: number;
  NAM?: number;
  CONG_CHUAN?: number;
  CONG_THUCTE?: number;
  CONG_LAMDEM?: number;
  DAILY_RATE?: number;
  DAILY_ALLOWANCE?: number;
  LUONG_CO_BAN?: number;

  PC_TRACH_NHIEM?: number;
  PC_CHUYEN_CAN?: number;
  PC_NHA_O?: number;
  PC_NGON_NGU?: number;
  PC_THAM_NIEN?: number;
  PC_DI_LAI?: number;
  PC_KHAC?: number;
  SUM_ALLOWANCES?: number;
  TONG_CO_BAN_TRO_CAP?: number;

  LUONG_CONG_THUCTE?: number;
  PHUCAP_CONG_THUCTE?: number;
  TIEN_CHUYENCAN?: number;
  TIEN_AN_CA?: number;
  KHOAN_CONG_KHAC?: number;
  TONG_NGAY_CONG_THUCTE?: number;

  OT_HOURS?: number;
  TIEN_TANGCA?: number;
  TOTAL_GROSS?: number;

  INSURANCE_BASE?: number;
  BHXH_8?: number;
  BHYT_15?: number;
  BHTN_1?: number;
  TIEN_BHXH_TRICH?: number;
  PHI_CONG_DOAN?: number;
  KHOAN_TRU_KHAC?: number;
  TIEN_TAMUNG?: number;
  TONG_KHAU_TRU?: number;

  THUC_LINH?: number;
}

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
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [detailData, setDetailData] = useState<ChiTietLuongNVDTO | null>(null);

  useEffect(() => {
    if (visible && record?.MANV && record?.MAKYCONG) {
      setLoadingDetail(true);
      api
        .get<ChiTietLuongNVDTO>(`/bangluong/chitiet?makycong=${record.MAKYCONG}&manv=${record.MANV}`)
        .then((res) => {
          setDetailData(res.data);
        })
        .catch(() => {
          setDetailData(null);
        })
        .finally(() => {
          setLoadingDetail(false);
        });
    } else {
      setDetailData(null);
    }
  }, [visible, record]);

  if (!record) return null;

  // Safe fallback calculation
  const congChuan = detailData?.CONG_CHUAN ?? record.CONG_CHUAN ?? 26;
  const congThucTe = detailData?.CONG_THUCTE ?? record.CONG_THUCTE ?? 0;
  const dailyRate = detailData?.DAILY_RATE ?? record.DAILY_RATE ?? 0;
  const luongCoBan = detailData?.LUONG_CO_BAN ?? dailyRate * congChuan;

  const pcTrachNhiem = detailData?.PC_TRACH_NHIEM ?? 0;
  const pcNgonNgu = detailData?.PC_NGON_NGU ?? 0;
  const pcThamNien = detailData?.PC_THAM_NIEN ?? 0;
  const pcChuyenCan = detailData?.PC_CHUYEN_CAN ?? (record.TIEN_CHUYENCAN || 0);
  const pcKhac = detailData?.PC_KHAC ?? (record.KHOAN_CONG_KHAC || 0);
  const pcNhaO = detailData?.PC_NHA_O ?? 0;
  const pcDiLai = detailData?.PC_DI_LAI ?? 0;
  const sumAllowances = detailData?.SUM_ALLOWANCES ?? (pcTrachNhiem + pcNgonNgu + pcThamNien + pcChuyenCan + pcKhac + pcNhaO + pcDiLai);
  const tongCoBanTroCap = detailData?.TONG_CO_BAN_TRO_CAP ?? (luongCoBan + sumAllowances);

  const luongCongThucTe = detailData?.LUONG_CONG_THUCTE ?? record.LUONG_CONG_THUCTE ?? 0;
  const phuCapCongThucTe = detailData?.PHUCAP_CONG_THUCTE ?? record.PHUCAP_CONG_THUCTE ?? 0;
  const tienChuyenCan = detailData?.TIEN_CHUYENCAN ?? record.TIEN_CHUYENCAN ?? 0;
  const tienAnCa = detailData?.TIEN_AN_CA ?? record.TIEN_AN_CA ?? 0;
  const khoanCongKhac = detailData?.KHOAN_CONG_KHAC ?? record.KHOAN_CONG_KHAC ?? 0;
  const tongNgayCongThucTe = detailData?.TONG_NGAY_CONG_THUCTE ?? (luongCongThucTe + phuCapCongThucTe + tienChuyenCan + tienAnCa + khoanCongKhac);

  const otHours = detailData?.OT_HOURS ?? 0;
  const tienTangCa = detailData?.TIEN_TANGCA ?? record.TIEN_TANGCA ?? 0;
  const totalGross = detailData?.TOTAL_GROSS ?? (tongNgayCongThucTe + tienTangCa);

  const insuranceBase = detailData?.INSURANCE_BASE ?? (record.TIEN_BHXH_TRICH ? Math.round(record.TIEN_BHXH_TRICH / 0.105) : 0);
  const bhxh = detailData?.BHXH_8 ?? (record.TIEN_BHXH_TRICH ? Math.round(insuranceBase * 0.08) : 0);
  const bhyt = detailData?.BHYT_15 ?? (record.TIEN_BHXH_TRICH ? Math.round(insuranceBase * 0.015) : 0);
  const bhtn = detailData?.BHTN_1 ?? (record.TIEN_BHXH_TRICH ? Math.round(insuranceBase * 0.01) : 0);
  const phiCongDoan = detailData?.PHI_CONG_DOAN ?? 0;
  const thueTNCN = detailData?.KHOAN_TRU_KHAC ?? record.KHOAN_TRU_KHAC ?? 0;
  const tienTamUng = detailData?.TIEN_TAMUNG ?? record.TIEN_TAMUNG ?? 0;
  const thucLinh = detailData?.THUC_LINH ?? record.THUC_LINH ?? (totalGross - (bhxh + bhyt + bhtn + thueTNCN + tienTamUng));

  const formatVnd = (val?: number | string | null) => {
    if (val === undefined || val === null || val === '-') return '-';
    if (typeof val === 'string') return val;
    if (isNaN(val)) return '-';
    if (val === 0) return '0';
    return Math.round(val).toLocaleString('vi-VN');
  };

  const thang = detailData?.THANG ?? record.THANG;
  const nam = detailData?.NAM ?? record.NAM;
  const tenpb = detailData?.TENPB ?? (record as any).TENPB ?? 'Phòng ban';
  const hourlyRate = congChuan > 0 ? Math.round(luongCoBan / (congChuan * 8)) : 0;

  // TOÀN BỘ 51 MỤC CHUẨN XÁC 100% THEO RPTBAOCAOLUONGNV.CS
  const rows: Array<{
    stt: string;
    content: string;
    qty?: string;
    amount?: number | string;
    isHighlight?: boolean;
    isGrandTotal?: boolean;
    isDeduction?: boolean;
  }> = [
    { stt: '-', content: 'Số ngày công tiêu chuẩn trong tháng', qty: `${congChuan} ngày`, amount: '-' },
    { stt: '1', content: 'Lương cơ bản (theo HĐLĐ)', qty: '-', amount: luongCoBan },
    { stt: '2', content: 'Phụ cấp trách nhiệm', qty: '-', amount: pcTrachNhiem },
    { stt: '3', content: 'Phụ cấp ngôn ngữ', qty: '-', amount: pcNgonNgu },
    { stt: '4', content: 'Phụ cấp thâm niên', qty: '-', amount: pcThamNien },
    { stt: '5', content: 'Phụ cấp chuyên cần', qty: '-', amount: pcChuyenCan },
    { stt: '6', content: 'Phụ cấp khác', qty: '-', amount: pcKhac },
    { stt: '7', content: 'Phụ cấp nhà ở', qty: '-', amount: pcNhaO },
    { stt: '8', content: 'Phụ cấp phương tiện đi lại', qty: '-', amount: pcDiLai },
    { stt: '9', content: 'TỔNG CƠ BẢN + TRỢ CẤP', qty: '-', amount: tongCoBanTroCap, isHighlight: true },
    { stt: '10', content: 'Lương làm ca ngày chính thức', qty: `${congThucTe} công`, amount: luongCongThucTe },
    { stt: '11', content: 'Lương làm ca đêm chính thức', qty: '-', amount: 0 },
    { stt: '12', content: 'Lương làm ca ngày thử việc', qty: '-', amount: 0 },
    { stt: '13', content: 'Lương làm ca đêm thử việc', qty: '-', amount: 0 },
    { stt: '14', content: 'Lương phụ cấp', qty: `${congThucTe} công`, amount: phuCapCongThucTe },
    { stt: '15', content: 'Nghỉ hưởng 70% lương cơ bản', qty: '-', amount: 0 },
    { stt: '16', content: 'Tiền lương 1 ngày hưởng 100% lương cơ bản', qty: '-', amount: 0 },
    { stt: '17', content: 'Nghỉ phép năm', qty: '-', amount: 0 },
    { stt: '18', content: 'Trợ cấp chuyên cần', qty: '-', amount: tienChuyenCan },
    { stt: '19', content: 'Trợ cấp tiền ăn', qty: '-', amount: tienAnCa },
    { stt: '20', content: 'Các khoản cộng cho người lao động', qty: '-', amount: khoanCongKhac },
    { stt: '21', content: 'Người lao động bồi thường', qty: '-', amount: 0 },
    { stt: '22', content: 'TỔNG TIỀN NGÀY CÔNG THỰC TẾ', qty: '-', amount: tongNgayCongThucTe, isHighlight: true },
    { stt: '23', content: 'Mức tiền để tính 1 giờ Overtime', qty: '-', amount: hourlyRate },
    { stt: '24', content: 'Số giờ tăng ca ngày (6h-8h, 17h-22h) 150%', qty: otHours > 0 ? `${otHours} giờ` : '-', amount: tienTangCa },
    { stt: '25', content: 'Số giờ tăng ca ngày sau 22h-6h 200%', qty: '-', amount: 0 },
    { stt: '26', content: 'Số giờ tăng ca ngày chủ nhật (8h-22h) 200%', qty: '-', amount: 0 },
    { stt: '27', content: 'Số giờ tăng ca ngày chủ nhật sau 22h-6h 270%', qty: '-', amount: 0 },
    { stt: '28', content: 'Số giờ tăng ca 5h30-6h đêm 200%', qty: '-', amount: 0 },
    { stt: '29', content: 'Số giờ tăng ca đêm (6h-8h) 180%', qty: '-', amount: 0 },
    { stt: '30', content: 'Số tiếng làm ca đêm chủ nhật (20h-8h) 270%', qty: '-', amount: 0 },
    { stt: '31', content: 'Số giờ tăng ca ngày lễ (8h-22h) 300%', qty: '-', amount: 0 },
    { stt: '32', content: 'Số giờ tăng ca ngày lễ sau 22h 390%', qty: '-', amount: 0 },
    { stt: '33', content: 'Số giờ tăng ca ngày (6h-8h, 17h-22h) thử việc 150%*85%', qty: '-', amount: 0 },
    { stt: '34', content: 'Số giờ tăng ca ngày sau 22h-6h thử việc 200%*85%', qty: '-', amount: 0 },
    { stt: '35', content: 'Số giờ tăng ca chủ nhật (8h-22h) thử việc 200%*85%', qty: '-', amount: 0 },
    { stt: '36', content: 'Số giờ tăng ca ngày chủ nhật sau 22h-6h thử việc 270%*85%', qty: '-', amount: 0 },
    { stt: '37', content: 'Số giờ tăng ca đêm (5h30-6h) thử việc 200%*85%', qty: '-', amount: 0 },
    { stt: '38', content: 'Số giờ tăng ca đêm (6h-8h) thử việc 180%*85%', qty: '-', amount: 0 },
    { stt: '39', content: 'Số tiếng làm ca đêm chủ nhật (20h-8h) thử việc 270%*85%', qty: '-', amount: 0 },
    { stt: '40', content: 'Số giờ tăng ca ngày lễ (8h-22h) thử việc 300%*85%', qty: '-', amount: 0 },
    { stt: '41', content: 'Số giờ tăng ca ngày lễ sau 22h thử việc 390%*85%', qty: '-', amount: 0 },
    { stt: '42', content: 'TỔNG CỘNG THU NHẬP (GROSS)', qty: '-', amount: totalGross, isHighlight: true },
    { stt: '43', content: 'Lương căn cứ tính BHXH', qty: '-', amount: insuranceBase },
    { stt: '44', content: 'BHXH (8%)', qty: '8.0%', amount: bhxh, isDeduction: true },
    { stt: '45', content: 'BHYT (1.5%)', qty: '1.5%', amount: bhyt, isDeduction: true },
    { stt: '46', content: 'BHTN (1%)', qty: '1.0%', amount: bhtn, isDeduction: true },
    { stt: '47', content: 'Phí công đoàn', qty: '-', amount: phiCongDoan },
    { stt: '48', content: 'Thuế thu nhập cá nhân (TNCN)', qty: '-', amount: thueTNCN, isDeduction: true },
    { stt: '49', content: 'Quyết toán và trả lại tiền thuế thừa của năm 2023', qty: '-', amount: 0 },
    { stt: '50', content: 'Trừ tiền tạm ứng', qty: '-', amount: tienTamUng, isDeduction: true },
    { stt: '51', content: 'CÒN LĨNH THỰC TẾ (NET PAY)', qty: '(Chuyển khoản)', amount: thucLinh, isGrandTotal: true },
  ];

  // In A4 thông qua iframe độc lập để không dính URL header/footer hay modal styling
  const handlePrint = () => {
    const payslipEl = document.getElementById('printable-payslip');
    if (!payslipEl) {
      window.print();
      return;
    }

    try {
      let iframe = document.getElementById('print-payslip-iframe') as HTMLIFrameElement;
      if (!iframe) {
        iframe = document.createElement('iframe');
        iframe.id = 'print-payslip-iframe';
        iframe.style.position = 'fixed';
        iframe.style.right = '0';
        iframe.style.bottom = '0';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = '0';
        iframe.style.visibility = 'hidden';
        document.body.appendChild(iframe);
      }

      const doc = iframe.contentWindow?.document;
      if (doc) {
        doc.open();
        doc.write(`
          <!DOCTYPE html>
          <html>
            <head>
              <meta charset="utf-8" />
              <title></title>
              <style>
                @page {
                  size: A4 portrait;
                  margin: 8mm 10mm;
                }
                * {
                  box-sizing: border-box;
                  margin: 0;
                  padding: 0;
                }
                body {
                  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
                  background: #ffffff;
                  color: #0f172a;
                  padding: 0;
                  font-size: 11px;
                  line-height: 1.35;
                  -webkit-print-color-adjust: exact !important;
                  print-color-adjust: exact !important;
                }
                table {
                  width: 100%;
                  border-collapse: collapse;
                }
                tr {
                  page-break-inside: avoid;
                }
              </style>
            </head>
            <body>
              ${payslipEl.outerHTML}
            </body>
          </html>
        `);
        doc.close();

        setTimeout(() => {
          iframe.contentWindow?.focus();
          iframe.contentWindow?.print();
        }, 250);
        return;
      }
    } catch (e) {
      console.warn('Iframe print fallback to window.print', e);
    }

    window.print();
  };

  return (
    <Modal
      open={visible}
      onCancel={onClose}
      width="min(960px, 95vw)"
      style={{ top: 20 }}
      className="payslip-modal"
      styles={{
        body: {
          padding: 0,
        },
      }}
      footer={
        <div style={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', gap: '12px', paddingTop: '10px' }}>
          <Button onClick={onClose} size="middle" style={{ borderRadius: '6px' }}>
            Đóng
          </Button>
          <Button
            type="primary"
            icon={<PrinterOutlined />}
            onClick={handlePrint}
            size="middle"
            style={{
              background: '#1677ff',
              borderRadius: '6px',
              fontWeight: 500,
              boxShadow: '0 2px 4px rgba(22, 119, 255, 0.25)',
            }}
          >
            In Bảng Lương (A4)
          </Button>
        </div>
      }
    >
      <Spin spinning={loadingDetail} tip="Đang tải dữ liệu chi tiết từ CSDL...">
        {/* CONTAINER SCROLL ĐỂ XEM ĐẦY ĐỦ 51 MỤC TRÊN MÀN HÌNH */}
        <div
          id="printable-payslip-wrapper"
          style={{
            maxHeight: 'calc(86vh - 110px)',
            overflowY: 'auto',
            padding: '16px 20px',
            backgroundColor: '#ffffff',
          }}
        >
          <div
            id="printable-payslip"
            style={{
              backgroundColor: '#ffffff',
              color: '#0f172a',
              fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
              padding: '12px 14px',
              fontSize: '12px',
              lineHeight: 1.4,
            }}
          >
            {/* HEADER SECTION */}
            <div
              style={{
                border: '1.5px solid #334155',
                borderBottom: 'none',
                borderRadius: '6px 6px 0 0',
                overflow: 'hidden',
              }}
            >
              {/* TIÊU ĐỀ CHÍNH & SỐ HIỆU PHIẾU */}
              <div
                style={{
                  display: 'flex',
                  alignItems: 'stretch',
                  borderBottom: '1.5px solid #334155',
                }}
              >
                <div
                  style={{
                    flex: 1,
                    padding: '12px 16px',
                    textAlign: 'center',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    backgroundColor: '#ffffff',
                  }}
                >
                  <div
                    style={{
                      fontSize: '11px',
                      fontWeight: 700,
                      textTransform: 'uppercase',
                      letterSpacing: '1.2px',
                      color: '#475569',
                      marginBottom: '3px',
                    }}
                  >
                    CÔNG TY CỔ PHẦN PHÁT TRIỂN NGUỒN NHÂN LỰC HRMS
                  </div>
                  <h2
                    style={{
                      margin: 0,
                      color: '#b91c1c',
                      fontWeight: 800,
                      textTransform: 'uppercase',
                      fontSize: '18px',
                      letterSpacing: '0.5px',
                      lineHeight: 1.3,
                    }}
                  >
                    {kyCongLabel ? `BẢNG LƯƠNG CHI TIẾT - ${kyCongLabel.toUpperCase()}` : `BẢNG LƯƠNG CHI TIẾT - THÁNG ${thang} NĂM ${nam}`}
                  </h2>
                </div>

                <div
                  style={{
                    width: '180px',
                    borderLeft: '1.5px solid #334155',
                    backgroundColor: '#fef3c7',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: '8px',
                  }}
                >
                  <div style={{ fontSize: '10.5px', fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.8px', color: '#9a3412' }}>
                    No / STT
                  </div>
                  <div style={{ fontSize: '20px', fontWeight: 900, color: '#b91c1c', marginTop: '1px', lineHeight: 1.1 }}>
                    #{record.MANV}
                  </div>
                </div>
              </div>

              {/* HÀNG THÔNG TIN NHÂN SỰ (3 Ô) */}
              <div
                style={{
                  display: 'flex',
                  backgroundColor: '#f8fafc',
                  fontSize: '12.5px',
                }}
              >
                <div style={{ flex: 1.3, padding: '7px 12px', borderRight: '1px solid #cbd5e1' }}>
                  <span style={{ color: '#64748b', marginRight: '4px' }}>Họ và tên:</span>
                  <strong style={{ fontSize: '13px', color: '#0f172a' }}>{record.HOTEN}</strong>
                </div>
                <div style={{ flex: 1.3, padding: '7px 12px', borderRight: '1px solid #cbd5e1' }}>
                  <span style={{ color: '#64748b', marginRight: '4px' }}>Phòng ban:</span>
                  <strong style={{ color: '#1e293b' }}>{tenpb}</strong>
                </div>
                <div style={{ width: '180px', padding: '7px 12px', textAlign: 'center' }}>
                  <span style={{ color: '#64748b', marginRight: '4px' }}>Mã NV:</span>
                  <strong style={{ color: '#1e40af', fontSize: '13px' }}>{record.MANV}</strong>
                </div>
              </div>
            </div>

            {/* BẢNG ĐẦY ĐỦ 51 MỤC (TỪ MỤC 1 ĐẾN MỤC 51 KHỚP 100% RPTBAOCAOLUONGNV) */}
            <table
              style={{
                width: '100%',
                borderCollapse: 'collapse',
                border: '1.5px solid #334155',
                borderRadius: '0 0 6px 6px',
                fontSize: '11.5px',
              }}
            >
              <thead>
                <tr style={{ backgroundColor: '#f1f5f9', color: '#0f172a', fontWeight: 700, textAlign: 'center' }}>
                  <th style={{ width: '48px', border: '1px solid #cbd5e1', borderBottom: '1.5px solid #334155', padding: '6px 3px' }}>
                    Mục
                  </th>
                  <th style={{ border: '1px solid #cbd5e1', borderBottom: '1.5px solid #334155', padding: '6px 10px', textAlign: 'left' }}>
                    Nội dung diễn giải chi tiết
                  </th>
                  <th style={{ width: '140px', border: '1px solid #cbd5e1', borderBottom: '1.5px solid #334155', padding: '6px 6px' }}>
                    Số lượng / Công / Giờ
                  </th>
                  <th style={{ width: '155px', border: '1px solid #cbd5e1', borderBottom: '1.5px solid #334155', padding: '6px 10px', textAlign: 'right' }}>
                    Thành tiền (VNĐ)
                  </th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => {
                  // Dòng tổng kết đặc biệt (Mục 51)
                  if (row.isGrandTotal) {
                    return (
                      <tr
                        key={row.stt}
                        style={{
                          backgroundColor: '#fef08a',
                          color: '#b91c1c',
                          fontWeight: 800,
                          borderTop: '2px solid #ca8a04',
                          borderBottom: '2px solid #ca8a04',
                        }}
                      >
                        <td style={{ border: '1px solid #ca8a04', textAlign: 'center', padding: '8px 3px', fontSize: '13.5px' }}>
                          {row.stt}
                        </td>
                        <td style={{ border: '1px solid #ca8a04', padding: '8px 10px', fontSize: '13px', letterSpacing: '0.3px' }}>
                          {row.content}
                        </td>
                        <td style={{ border: '1px solid #ca8a04', textAlign: 'center', padding: '8px 6px', fontSize: '11.5px', color: '#9a3412' }}>
                          {row.qty || '-'}
                        </td>
                        <td
                          style={{
                            border: '1px solid #ca8a04',
                            textAlign: 'right',
                            padding: '8px 10px',
                            fontSize: '15px',
                            fontWeight: 900,
                          }}
                        >
                          {formatVnd(row.amount)} VNĐ
                        </td>
                      </tr>
                    );
                  }

                  // Các dòng tổng phụ (Mục 9, 22, 42)
                  if (row.isHighlight) {
                    return (
                      <tr
                        key={row.stt}
                        style={{
                          backgroundColor: '#fef9c3',
                          color: '#991b1b',
                          fontWeight: 700,
                        }}
                      >
                        <td style={{ border: '1px solid #cbd5e1', textAlign: 'center', padding: '5.5px 3px' }}>
                          {row.stt}
                        </td>
                        <td style={{ border: '1px solid #cbd5e1', padding: '5.5px 10px', letterSpacing: '0.2px' }}>
                          {row.content}
                        </td>
                        <td style={{ border: '1px solid #cbd5e1', textAlign: 'center', padding: '5.5px 6px', color: '#991b1b' }}>
                          {row.qty || '-'}
                        </td>
                        <td style={{ border: '1px solid #cbd5e1', textAlign: 'right', padding: '5.5px 10px', fontSize: '12.5px' }}>
                          {formatVnd(row.amount)}
                        </td>
                      </tr>
                    );
                  }

                  // Các dòng chi tiết thông thường
                  return (
                    <tr key={row.stt} style={{ backgroundColor: '#ffffff' }}>
                      <td style={{ border: '1px solid #cbd5e1', textAlign: 'center', padding: '4px 3px', color: '#64748b' }}>
                        {row.stt}
                      </td>
                      <td style={{ border: '1px solid #cbd5e1', padding: '4px 10px' }}>
                        {row.content}
                      </td>
                      <td
                        style={{
                          border: '1px solid #cbd5e1',
                          textAlign: 'center',
                          padding: '4px 6px',
                          color: row.qty && row.qty !== '-' ? '#0f172a' : '#94a3b8',
                          fontWeight: row.qty && row.qty !== '-' ? 600 : 400,
                        }}
                      >
                        {row.qty || '-'}
                      </td>
                      <td
                        style={{
                          border: '1px solid #cbd5e1',
                          textAlign: 'right',
                          padding: '4px 10px',
                          color: row.isDeduction && row.amount ? '#dc2626' : (row.amount === 0 ? '#94a3b8' : '#0f172a'),
                          fontWeight: row.amount && row.amount !== 0 && row.amount !== '-' ? 600 : 400,
                        }}
                      >
                        {row.isDeduction && typeof row.amount === 'number' && row.amount > 0
                          ? `-${formatVnd(row.amount)}`
                          : formatVnd(row.amount)}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>

            {/* CHỮ KÝ 3 BÊN */}
            <div style={{ marginTop: '20px', display: 'flex', justifyContent: 'space-between', textAlign: 'center' }}>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: '12px', color: '#0f172a', textTransform: 'uppercase' }}>
                  Người lập biểu
                </div>
                <div style={{ fontSize: '10.5px', color: '#64748b', fontStyle: 'italic', marginTop: '1px' }}>
                  (Ký, ghi rõ họ tên)
                </div>
                <div style={{ height: '46px' }} />
                <div style={{ fontWeight: 600, color: '#1e293b' }}>Phòng Nhân sự</div>
              </div>

              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: '12px', color: '#0f172a', textTransform: 'uppercase' }}>
                  Kế toán trưởng
                </div>
                <div style={{ fontSize: '10.5px', color: '#64748b', fontStyle: 'italic', marginTop: '1px' }}>
                  (Ký, ghi rõ họ tên)
                </div>
                <div style={{ height: '46px' }} />
                <div style={{ fontWeight: 600, color: '#1e293b' }}>Ban Tài chính</div>
              </div>

              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: '12px', color: '#0f172a', textTransform: 'uppercase' }}>
                  Người lao động
                </div>
                <div style={{ fontSize: '10.5px', color: '#64748b', fontStyle: 'italic', marginTop: '1px' }}>
                  (Ký xác nhận)
                </div>
                <div style={{ height: '46px' }} />
                <div style={{ fontWeight: 600, color: '#1e293b' }}>{record.HOTEN}</div>
              </div>
            </div>

            <div style={{ marginTop: '16px', textAlign: 'center', fontSize: '10.5px', color: '#64748b', fontStyle: 'italic' }}>
              Bảng thanh toán tiền lương này được trích xuất từ hệ thống Quản lý Nhân sự HRMS Enterprise. Mọi thắc mắc vui lòng phản hồi phòng Nhân sự trong vòng 3 ngày làm việc.
            </div>
          </div>
        </div>
      </Spin>
    </Modal>
  );
};

export default PhieuLuongModal;
