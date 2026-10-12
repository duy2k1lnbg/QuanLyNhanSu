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
import type { NhanVienDTO, KyCongDTO, HopDongDTO, CurrentUserDTO } from '../types/hrms';
import { canView, canAdd, canEdit, canAccessRoute } from '../utils/permissionUtils';
import { useAppLanguage } from '../services/i18n';

export interface CommandPaletteModalProps {
  visible: boolean;
  onClose: () => void;
  nhanVienList: NhanVienDTO[];
  kyCongList?: KyCongDTO[];
  hopDongList?: HopDongDTO[];
  onSelectEmployee?: (emp: NhanVienDTO) => void;
  onNavigate: (route: string) => void;
  onOpenAiDrawer?: () => void;
  currentUser?: CurrentUserDTO;
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
  currentUser: propUser,
}) => {
  const { t } = useAppLanguage();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(0);

  // Fallback to localStorage user if not passed in prop
  const currentUser: CurrentUserDTO | undefined = useMemo(() => {
    if (propUser) return propUser;
    try {
      const stored = localStorage.getItem('hrms_user');
      return stored ? JSON.parse(stored) : undefined;
    } catch {
      return undefined;
    }
  }, [propUser]);

  // Global Quick Actions guarded by permissions
  const systemActions: SearchItem[] = useMemo(() => {
    const list: SearchItem[] = [];

    if (canAdd(currentUser, 'F_DM_NHANVIEN')) {
      list.push({
        id: 'act-add-emp',
        type: 'action',
        title: t('commandPalette.actionAddEmpTitle'),
        subtitle: t('commandPalette.actionAddEmpDesc'),
        icon: <UserOutlined style={{ color: '#10b981' }} />,
        tag: t('commandPalette.tagAction'),
        tagColor: 'green',
        action: () => {
          onNavigate('nhanvien');
          onClose();
        },
      });
    }

    if (canEdit(currentUser, 'F_CC_BANGLUONG') || canAdd(currentUser, 'F_CC_BANGLUONG')) {
      list.push({
        id: 'act-tinh-luong',
        type: 'action',
        title: t('commandPalette.actionPayrollTitle'),
        subtitle: t('commandPalette.actionPayrollDesc'),
        icon: <DollarOutlined style={{ color: '#f59e0b' }} />,
        tag: t('commandPalette.tagPayroll'),
        tagColor: 'orange',
        action: () => {
          onNavigate('bangluong');
          onClose();
        },
      });
    }

    list.push({
      id: 'act-ai-copilot',
      type: 'action',
      title: t('commandPalette.actionAiTitle'),
      subtitle: t('commandPalette.actionAiDesc'),
      icon: <RobotOutlined style={{ color: '#8b5cf6' }} />,
      tag: 'AI',
      tagColor: 'purple',
      action: () => {
        onClose();
        if (onOpenAiDrawer) onOpenAiDrawer();
      },
    });

    if (canView(currentUser, 'F_CC_BANGCONG') || canAccessRoute(currentUser, 'chamcong')) {
      list.push({
        id: 'act-cham-cong',
        type: 'navigation',
        title: t('commandPalette.actionAttendanceTitle'),
        subtitle: t('commandPalette.actionAttendanceDesc'),
        icon: <CalendarOutlined style={{ color: '#3b82f6' }} />,
        tag: t('commandPalette.tagNavigation'),
        tagColor: 'blue',
        action: () => {
          onNavigate('chamcong');
          onClose();
        },
      });
    }

    if (canView(currentUser, 'F_NV_HOPDONG') || canAccessRoute(currentUser, 'hopdong')) {
      list.push({
        id: 'act-hop-dong',
        type: 'navigation',
        title: t('commandPalette.actionContractTitle'),
        subtitle: t('commandPalette.actionContractDesc'),
        icon: <FileTextOutlined style={{ color: '#ec4899' }} />,
        tag: t('commandPalette.tagContract'),
        tagColor: 'magenta',
        action: () => {
          onNavigate('hopdong');
          onClose();
        },
      });
    }

    return list;
  }, [currentUser, onNavigate, onClose, onOpenAiDrawer, t]);

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

    // 2. Filter Employees (Only if user has VIEW permission on employee module)
    if (canView(currentUser, 'F_DM_NHANVIEN') || canAccessRoute(currentUser, 'nhanvien')) {
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
            tag: t('commandPalette.tagEmployee'),
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
    }

    // 3. Filter Contracts (Only if user has VIEW permission on contracts)
    if (canView(currentUser, 'F_NV_HOPDONG') || canAccessRoute(currentUser, 'hopdong')) {
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
            tag: t('commandPalette.tagContract'),
            tagColor: 'green',
            action: () => {
              onNavigate('hopdong');
              onClose();
            },
          });
        }
      });
    }

    // 4. Filter Pay Periods (Only if user has VIEW permission on payroll)
    if (canView(currentUser, 'F_CC_BANGLUONG') || canAccessRoute(currentUser, 'bangluong')) {
      kyCongList.forEach((kc) => {
        const termMatch = `kỳ lương ${kc.THANG}/${kc.NAM}`.includes(term) || `tháng ${kc.THANG}`.includes(term);
        if (termMatch) {
          results.push({
            id: `kc-${kc.MAKYCONG}`,
            type: 'payroll',
            title: `Bảng lương Kỳ Tháng ${kc.THANG}/${kc.NAM}`,
            subtitle: `Mã kỳ công #${kc.MAKYCONG} • ${kc.KHOA === 1 ? 'Đã khóa bảng' : 'Chưa khóa'}`,
            icon: <DollarOutlined style={{ color: '#f59e0b' }} />,
            tag: t('commandPalette.tagPayroll'),
            tagColor: 'gold',
            action: () => {
              onNavigate('bangluong');
              onClose();
            },
          });
        }
      });
    }

    return results.slice(0, 12);
  }, [searchTerm, systemActions, nhanVienList, hopDongList, kyCongList, onClose, onSelectEmployee, onNavigate, currentUser, t]);

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
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: 12, color: '#94a3b8', flexWrap: 'wrap', gap: 6 }}>
          <span>{t('commandPalette.footerNavigate')}</span>
          <span>{t('commandPalette.footerShortcut')}</span>
        </div>
      }
      closable={false}
      width="min(640px, 95vw)"
      style={{ top: 80 }}
      bodyStyle={{ padding: '16px 20px 8px 20px' }}
    >
      <Input
        autoFocus
        size="large"
        prefix={<SearchOutlined style={{ color: '#64748b', fontSize: 20, marginRight: 8 }} />}
        placeholder={t('commandPalette.searchPlaceholder')}
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
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('commandPalette.noResults')} />
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
                      <div style={{ color: '#64748b', fontSize: 12 }}>
                        {item.subtitle}
                      </div>
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
