import React, { useState, useEffect, useMemo } from 'react';
import { Modal, Input, List, Tag, Empty } from 'antd';
import {
  SearchOutlined,
  UserOutlined,
  DollarOutlined,
  CalendarOutlined,
  FileTextOutlined,
  RobotOutlined,
  ArrowRightOutlined,
} from '@ant-design/icons';
import type { NhanVienDTO, KyCongDTO, HopDongDTO } from '../types/hrms';

export interface CommandPaletteModalProps {
  visible: boolean;
  onClose: () => void;
  nhanVienList: NhanVienDTO[];
  kyCongList?: KyCongDTO[];
  hopDongList?: HopDongDTO[];
  onSelectEmployee?: (emp: NhanVienDTO) => void;
  onNavigate: (route: string) => void;
  onOpenAiDrawer?: () => void;
}

interface SearchItem {
  id: string;
  type: 'employee' | 'action' | 'navigation' | 'contract' | 'payroll';
  title: string;
  subtitle: string;
  icon: React.ReactNode;
  tag?: string;
  tagColor?: string;
  action: () => void;
}

export const CommandPaletteModal: React.FC<CommandPaletteModalProps> = ({
  visible,
  onClose,
  nhanVienList,
  kyCongList = [],
  hopDongList = [],
  onSelectEmployee,
  onNavigate,
  onOpenAiDrawer,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(0);

  // Global Quick Actions
  const systemActions: SearchItem[] = useMemo(
    () => [
      {
        id: 'act-add-emp',
        type: 'action',
        title: 'Thêm hồ sơ nhân viên mới',
        subtitle: 'Mở biểu mẫu tiếp nhận và cấp mã nhân sự',
        icon: <UserOutlined style={{ color: '#10b981' }} />,
        tag: 'Hành động',
        tagColor: 'green',
        action: () => {
          onNavigate('nhanvien');
          onClose();
        },
      },
      {
        id: 'act-tinh-luong',
        type: 'action',
        title: 'Tính toán bảng lương tự động',
        subtitle: 'Chạy công thức lương Gross-to-Net cho kỳ hiện tại',
        icon: <DollarOutlined style={{ color: '#f59e0b' }} />,
        tag: 'Bảng lương',
        tagColor: 'orange',
        action: () => {
          onNavigate('bangluong');
          onClose();
        },
      },
      {
        id: 'act-ai-copilot',
        type: 'action',
        title: 'Trợ lý AI Copilot phân tích dữ liệu',
        subtitle: 'Hỏi đáp báo cáo, dự báo biến động lương và bất thường',
        icon: <RobotOutlined style={{ color: '#8b5cf6' }} />,
        tag: 'AI',
        tagColor: 'purple',
        action: () => {
          onClose();
          if (onOpenAiDrawer) onOpenAiDrawer();
        },
      },
      {
        id: 'act-cham-cong',
        type: 'navigation',
        title: 'Bảng chấm công & Ca làm việc',
        subtitle: 'Xem bảng công chi tiết và phân ca nhân sự',
        icon: <CalendarOutlined style={{ color: '#3b82f6' }} />,
        tag: 'Điều hướng',
        tagColor: 'blue',
        action: () => {
          onNavigate('chamcong');
          onClose();
        },
      },
      {
        id: 'act-hop-dong',
        type: 'navigation',
        title: 'Hợp đồng lao động & Tái ký',
        subtitle: 'Quản lý thời hạn và quyết định gia hạn hợp đồng',
        icon: <FileTextOutlined style={{ color: '#ec4899' }} />,
        tag: 'Hợp đồng',
        tagColor: 'magenta',
        action: () => {
          onNavigate('hopdong');
          onClose();
        },
      },
    ],
    [onNavigate, onClose, onOpenAiDrawer]
  );

  // Search filter
  const filteredItems = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) return systemActions;

    const results: SearchItem[] = [];

    // 1. Filter System Actions
    systemActions.forEach((item) => {
      if (item.title.toLowerCase().includes(term) || item.subtitle.toLowerCase().includes(term)) {
        results.push(item);
      }
    });

    // 2. Filter Employees
    nhanVienList.forEach((emp) => {
      const matchName = emp.HOTEN?.toLowerCase().includes(term);
      const matchId = String(emp.MANV).includes(term);
      const matchPb = emp.TENPB?.toLowerCase().includes(term);
      const matchCv = emp.TENCV?.toLowerCase().includes(term);
      const matchPhone = emp.DIENTHOAI?.includes(term);

      if (matchName || matchId || matchPb || matchCv || matchPhone) {
        results.push({
          id: `emp-${emp.MANV}`,
          type: 'employee',
          title: emp.HOTEN,
          subtitle: `Mã NV #${emp.MANV} • ${emp.TENPB || 'Chưa phân phòng'} • ${emp.TENCV || 'Nhân viên'}`,
          icon: <UserOutlined style={{ color: '#3b82f6' }} />,
          tag: 'Nhân sự 360°',
          tagColor: 'cyan',
          action: () => {
            onClose();
            if (onSelectEmployee) {
              onSelectEmployee(emp);
            } else {
              onNavigate('nhanvien');
            }
          },
        });
      }
    });

    // 3. Filter Contracts
    hopDongList.forEach((hd) => {
      const matchSoHd = hd.SOHD?.toLowerCase().includes(term);
      const matchTen = hd.HOTEN?.toLowerCase().includes(term);
      if (matchSoHd || matchTen) {
        results.push({
          id: `hd-${hd.SOHD}`,
          type: 'contract',
          title: `Hợp đồng #${hd.SOHD}`,
          subtitle: `Nhân sự: ${hd.HOTEN || 'N/A'} • Lương: ${(hd.LUONG_THOA_THUAN ?? 0).toLocaleString('vi-VN')} đ`,
          icon: <FileTextOutlined style={{ color: '#10b981' }} />,
          tag: 'Hợp đồng',
          tagColor: 'green',
          action: () => {
            onNavigate('hopdong');
            onClose();
          },
        });
      }
    });

    // 4. Filter Pay Periods (Ky Cong)
    kyCongList.forEach((kc) => {
      const termMatch = `kỳ lương ${kc.THANG}/${kc.NAM}`.includes(term) || `tháng ${kc.THANG}`.includes(term);
      if (termMatch) {
        results.push({
          id: `kc-${kc.MAKYCONG}`,
          type: 'payroll',
          title: `Bảng lương Kỳ Tháng ${kc.THANG}/${kc.NAM}`,
          subtitle: `Mã kỳ công #${kc.MAKYCONG} • ${kc.KHOA === 1 ? 'Đã khóa bảng' : 'Chưa khóa'}`,
          icon: <DollarOutlined style={{ color: '#f59e0b' }} />,
          tag: 'Bảng lương',
          tagColor: 'gold',
          action: () => {
            onNavigate('bangluong');
            onClose();
          },
        });
      }
    });

    return results.slice(0, 12);
  }, [searchTerm, systemActions, nhanVienList, hopDongList, kyCongList, onClose, onSelectEmployee, onNavigate]);

  useEffect(() => {
    setSelectedIndex(0);
  }, [searchTerm]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setSelectedIndex((prev) => (prev < filteredItems.length - 1 ? prev + 1 : 0));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setSelectedIndex((prev) => (prev > 0 ? prev - 1 : filteredItems.length - 1));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (filteredItems[selectedIndex]) {
        filteredItems[selectedIndex].action();
      }
    }
  };

  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: 12, color: '#94a3b8' }}>
          <span>
            Dùng phím <kbd style={{ padding: '2px 6px', background: '#f1f5f9', borderRadius: 4 }}>↑</kbd>{' '}
            <kbd style={{ padding: '2px 6px', background: '#f1f5f9', borderRadius: 4 }}>↓</kbd> để di chuyển,{' '}
            <kbd style={{ padding: '2px 6px', background: '#f1f5f9', borderRadius: 4 }}>Enter</kbd> để chọn
          </span>
          <span>Phím tắt toàn cục: <kbd style={{ padding: '2px 6px', background: '#f1f5f9', borderRadius: 4 }}>Ctrl + K</kbd></span>
        </div>
      }
      closable={false}
      width={640}
      style={{ top: 80 }}
      bodyStyle={{ padding: '16px 20px 8px 20px' }}
    >
      <Input
        autoFocus
        size="large"
        prefix={<SearchOutlined style={{ color: '#64748b', fontSize: 20, marginRight: 8 }} />}
        placeholder="Tìm kiếm nhân viên, phòng ban, số hợp đồng, kỳ lương... (Ctrl + K)"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        onKeyDown={handleKeyDown}
        style={{
          borderRadius: 8,
          marginBottom: 12,
          border: '1px solid #cbd5e1',
          boxShadow: '0 2px 6px rgba(0,0,0,0.04)',
        }}
        allowClear
      />

      <div style={{ maxHeight: 380, overflowY: 'auto' }}>
        {filteredItems.length === 0 ? (
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không tìm thấy kết quả phù hợp" />
        ) : (
          <List
            dataSource={filteredItems}
            renderItem={(item, index) => {
              const isSelected = index === selectedIndex;
              return (
                <div
                  key={item.id}
                  onClick={() => item.action()}
                  onMouseEnter={() => setSelectedIndex(index)}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    padding: '10px 14px',
                    borderRadius: 6,
                    cursor: 'pointer',
                    background: isSelected ? '#f1f5f9' : 'transparent',
                    transition: 'all 0.15s ease',
                    marginBottom: 2,
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: 12, overflow: 'hidden' }}>
                    <div
                      style={{
                        width: 34,
                        height: 34,
                        borderRadius: 6,
                        backgroundColor: '#fff',
                        border: '1px solid #e2e8f0',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: 16,
                      }}
                    >
                      {item.icon}
                    </div>
                    <div>
                      <div style={{ fontWeight: 600, color: '#0f172a', fontSize: 14 }}>
                        {item.title}
                      </div>
                      <div
                        style={{ color: '#64748b', fontSize: 12 }}
                        dangerouslySetInnerHTML={{ __html: item.subtitle }}
                      />
                    </div>
                  </div>

                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    {item.tag && <Tag color={item.tagColor || 'default'}>{item.tag}</Tag>}
                    {isSelected && <ArrowRightOutlined style={{ color: '#3b82f6', fontSize: 12 }} />}
                  </div>
                </div>
              );
            }}
          />
        )}
      </div>
    </Modal>
  );
};

export default CommandPaletteModal;
