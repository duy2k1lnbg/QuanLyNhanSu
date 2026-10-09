import React, { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import {
  Modal,
  Table,
  Checkbox,
  Button,
  Input,
  Space,
  Tag,
  Typography,
  message,
  Alert,
  Segmented,
  Tooltip,
  Tabs,
  Badge,
  Card,
  Row,
  Col,
} from 'antd';
import {
  DesktopOutlined,
  GlobalOutlined,
  MobileOutlined,
  SaveOutlined,
  SearchOutlined,
  CheckCircleOutlined,
  SafetyCertificateOutlined,
  TeamOutlined,
  InfoCircleOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';
import type {
  PlatformChannelTreeDTO,
  ChannelFunctionRightItemDTO,
  UserFullChannelPermissionsDTO,
} from '../types/hrms';

const { Text, Title } = Typography;

interface PhanQuyenModalProps {
  visible: boolean;
  onClose: () => void;
  userId: number;
  username: string;
  fullName: string;
  isGroup?: boolean;
  onSuccess?: () => void;
}

const CANONICAL_FUNCTION_NAMES: Record<string, string> = {
  F_SYSTEM_GROUP: 'Nhóm Người Dùng',
  F_SYSTEM_USER: 'Quản Lý Tài Khoản',
  F_SYSTEM_CAPTAIKHOAN: 'Cấp Tài Khoản Hàng Loạt',
  F_SYSTEM_LOCK_USER: 'Khóa/Mở Khóa Tài Khoản',
  F_SYSTEM_PQ_CHUCNANG: 'Phân Quyền Chức Năng',
  F_SYSTEM_PQ_BAOCAO: 'Phân Quyền Báo Cáo',
  F_SYSTEM_THONGBAO: 'Thông Báo Hệ Thống',
  F_SYSTEM_SAULUU: 'Sao Lưu Dữ Liệu',
  F_SYSTEM_PHUCHOI: 'Phục Hồi Dữ Liệu',
  F_SYSTEM_GIAMSAT: 'Giám Sát Đăng Nhập',
  F_SYSTEM_AI: 'Trợ Lý AI & Chatbot',
  F_SYSTEM_SETTING: 'Cấu Hình Ngôn Ngữ & Hệ Thống',
  F_SYSTEM_AI_CONFIG: 'Cấu Hình AI Server (Ollama)',
  F_SYSTEM_DB_CONFIG: 'Cấu Hình Kết Nối CSDL',
  F_DB_NHANSU: 'Dashboard Nhân Sự',
  F_DB_LUONG: 'Dashboard Lương',
  F_BC_BAOCAO: 'Báo Cáo Tổng Hợp & Chi Tiết',
  F_DM_DANTOC: 'Dân Tộc',
  F_DM_TONGIAO: 'Tôn Giáo',
  F_DM_TRINHDO: 'Trình Độ',
  F_DM_NHANVIEN: 'Hồ Sơ Nhân Viên',
  F_DM_PHONGBAN: 'Phòng Ban',
  F_DM_BOPHAN: 'Bộ Phận',
  F_DM_CONGTY: 'Công Ty',
  F_DM_CHUCVU: 'Chức Vụ',
  F_NV_HOPDONG: 'Hợp Đồng Lao Động',
  F_NV_LOAIHOPDONG: 'Loại Hợp Đồng',
  F_NV_NANGLUONG: 'Lên Lương Nhân Viên',
  F_NV_KHENTHUONG: 'Khen Thưởng',
  F_NV_KYLUAT: 'Kỷ Luật',
  F_NV_DIEUCHUYEN: 'Điều Chuyển Nhân Viên',
  F_NV_THOIVIEC: 'Thôi Việc',
  F_NV_PHEDUYET: 'Phê Duyệt Yêu Cầu (Online)',
  F_CC_LOAICA: 'Loại Ca',
  F_CC_LOAICONG: 'Loại Công',
  F_CC_NGAYLE: 'Ngày Lễ',
  F_CC_PHUCAP: 'Phụ Cấp Nhân Viên',
  F_CC_TANGCA: 'Tăng Ca',
  F_CC_UNGLUONG: 'Ứng Lương',
  F_CC_BANGCONG: 'Quản Lý Bảng Công',
  F_CC_BCCT: 'Bảng Công Chi Tiết',
  F_CC_BCCT_IN: 'In Bảng Công Nhân Viên',
  F_CC_CAPNHATCONG: 'Cập Nhật Ngày Công',
  F_CC_BANGLUONG: 'Bảng Lương',
  MOBILE_ROOT: 'Phân Hệ Mobile App',
  MOBILE_PROFILE_VIEW: 'Xem Hồ Sơ Cá Nhân Mobile',
  MOBILE_ATTENDANCE_VIEW: 'Xem Bảng Công Mobile',
  MOBILE_PAYROLL_VIEW: 'Xem Bảng Lương Mobile',
  MOBILE_CONTRACT_VIEW: 'Xem Hợp Đồng Lao Động Mobile',
  MOBILE_INSURANCE_VIEW: 'Xem Bảo Hiểm Xã Hội Mobile',
  MOBILE_NOTIFICATION_VIEW: 'Xem Thông Báo Nội Bộ Mobile',
  MOBILE_REQUEST_LEAVE: 'Gửi Đơn Nghỉ Phép Mobile',
  MOBILE_REQUEST_OVERTIME: 'Gửi Đơn Tăng Ca Mobile',
  MOBILE_REQUEST_ADVANCE: 'Gửi Yêu Cầu Ứng Lương Mobile',
};

const hasMojibake = (str: string): boolean => {
  if (!str) return false;
  return /[\u00C4\u00C5\u00C6\u00C0\u00C1\u00C2\u00C3\u00E0\u00E1\u00E2\u00E3\u00E4\u00C8\u00C9\u00CA\u00CB\u00CC\u00CD\u00CE\u00CF\u00D2\u00D3\u00D4\u00D5\u00D6\u00D9\u00DA\u00DB\u00DC\u00DD]/.test(str) &&
         /(?:Ä|á»|áº|Ã|Æ|Â)/.test(str);
};

export const PhanQuyenModal: React.FC<PhanQuyenModalProps> = ({
  visible,
  onClose,
  userId,
  username,
  fullName,
  isGroup = false,
  onSuccess,
}) => {
  const { t } = useAppLanguage();

  const [activeChannelTab, setActiveChannelTab] = useState<'DESKTOP' | 'WEB' | 'MOBILE'>('DESKTOP');
  const [channelTrees, setChannelTrees] = useState<Record<string, PlatformChannelTreeDTO>>({});
  const [loading, setLoading] = useState<boolean>(false);
  const [status, setStatus] = useState<'idle' | 'loading' | 'ready' | 'saving' | 'load-error' | 'conflict'>('idle');
  const [isDirty, setIsDirty] = useState<boolean>(false);
  const [securityVersion, setSecurityVersion] = useState<number>(1);
  const latestRequestId = useRef<number>(0);
  const [savingChannel, setSavingChannel] = useState<string | null>(null);
  const [savingAll, setSavingAll] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>('');
  const [subsystemFilter, setSubsystemFilter] = useState<string>('ALL');
  const [onlyGrantedFilter, setOnlyGrantedFilter] = useState<boolean>(false);

  const loadChannelPermissions = useCallback(async () => {
    if (!userId) return;
    const reqId = ++latestRequestId.current;
    setLoading(true);
    setStatus('loading');
    try {
      const res = await api.get<UserFullChannelPermissionsDTO>(`/users/${userId}/channel-permissions`);
      if (reqId !== latestRequestId.current) return;
      if (res.data && res.data.Channels) {
        const treeMap: Record<string, PlatformChannelTreeDTO> = {};
        res.data.Channels.forEach((ch: PlatformChannelTreeDTO) => {
          treeMap[ch.Channel] = JSON.parse(JSON.stringify(ch));
        });
        setChannelTrees(treeMap);
        if (res.data.SecurityVersion) {
          setSecurityVersion(res.data.SecurityVersion);
        }
        setIsDirty(false);
        setStatus('ready');
      }
    } catch {
      if (reqId !== latestRequestId.current) return;
      setStatus('load-error');
      message.error(t('common.error') || 'Không thể tải ma trận phân quyền theo kênh.');
    } finally {
      if (reqId === latestRequestId.current) {
        setLoading(false);
      }
    }
  }, [userId]);

  useEffect(() => {
    if (visible && userId) {
      loadChannelPermissions();
    } else {
      setIsDirty(false);
      setStatus('idle');
    }
  }, [visible, userId, loadChannelPermissions]);

  const handleRequestClose = () => {
    if (isDirty) {
      Modal.confirm({
        title: t('common.confirm') || 'Xác nhận',
        content: t('user.discardChangesConfirm') || 'Bạn có thay đổi phân quyền chưa lưu. Bạn có chắc chắn muốn hủy các thay đổi này?',
        onOk: () => {
          setIsDirty(false);
          onClose();
        },
      });
    } else {
      onClose();
    }
  };

  const handleRequestReload = () => {
    if (isDirty) {
      Modal.confirm({
        title: t('common.confirm') || 'Xác nhận',
        content: t('user.reloadDiscardConfirm') || 'Bạn có thay đổi phân quyền chưa lưu. Tải lại sẽ làm mất các thay đổi này, bạn có muốn tiếp tục?',
        onOk: () => {
          loadChannelPermissions();
        },
      });
    } else {
      loadChannelPermissions();
    }
  };

  const currentTree: PlatformChannelTreeDTO | undefined = channelTrees[activeChannelTab];

  const handleToggleChannelParent = (checked: boolean) => {
    if (!currentTree) return;
    setIsDirty(true);
    const updated = { ...currentTree };
    updated.ParentDirectGrant = checked;
    updated.ParentIsEffective = checked || updated.ParentInheritedGrant;

    // Cập nhật trạng thái con nhưng bảo toàn DirectGrant draft
    updated.Functions.forEach((fn) => {
      fn.IsDisabledByParent = !updated.ParentIsEffective;
      if (!updated.ParentIsEffective) {
        fn.EffectiveGrant = {
          CanView: false,
          CanAdd: false,
          CanEdit: false,
          CanDelete: false,
          CanPrint: false,
        };
      } else {
        fn.EffectiveGrant = {
          CanView: (fn.DirectGrant.CanView || fn.InheritedGrant.CanView) && fn.SupportedCapabilities.CanView,
          CanAdd: (fn.DirectGrant.CanAdd || fn.InheritedGrant.CanAdd) && fn.SupportedCapabilities.CanAdd,
          CanEdit: (fn.DirectGrant.CanEdit || fn.InheritedGrant.CanEdit) && fn.SupportedCapabilities.CanEdit,
          CanDelete: (fn.DirectGrant.CanDelete || fn.InheritedGrant.CanDelete) && fn.SupportedCapabilities.CanDelete,
          CanPrint: (fn.DirectGrant.CanPrint || fn.InheritedGrant.CanPrint) && fn.SupportedCapabilities.CanPrint,
        };
      }
    });

    setChannelTrees((prev) => ({ ...prev, [activeChannelTab]: updated }));
  };

  const handleToggleAction = (
    funcCode: string,
    action: 'CanView' | 'CanAdd' | 'CanEdit' | 'CanDelete' | 'CanPrint',
    checked: boolean
  ) => {
    if (!currentTree) return;
    setIsDirty(true);
    const updated = { ...currentTree };
    const fn = updated.Functions.find((f) => f.FunctionCode === funcCode);
    if (!fn) return;

    if (!fn.SupportedCapabilities[action]) return;

    fn.DirectGrant[action] = checked;

    // Quy tắc phụ thuộc hành động:
    // Nếu bật Thêm/Sửa/Xóa/In thì tự động bật Xem
    if (checked && action !== 'CanView' && fn.SupportedCapabilities.CanView) {
      fn.DirectGrant.CanView = true;
    }
    // Nếu tắt Xem thì tự động tắt các thao tác sửa đổi
    if (action === 'CanView' && !checked) {
      fn.DirectGrant.CanAdd = false;
      fn.DirectGrant.CanEdit = false;
      fn.DirectGrant.CanDelete = false;
      fn.DirectGrant.CanPrint = false;
    }

    // Tính toán quyền hiệu lực nếu quyền cha đang bật
    if (updated.ParentIsEffective) {
      fn.EffectiveGrant = {
        CanView: (fn.DirectGrant.CanView || fn.InheritedGrant.CanView) && fn.SupportedCapabilities.CanView,
        CanAdd: (fn.DirectGrant.CanAdd || fn.InheritedGrant.CanAdd) && fn.SupportedCapabilities.CanAdd,
        CanEdit: (fn.DirectGrant.CanEdit || fn.InheritedGrant.CanEdit) && fn.SupportedCapabilities.CanEdit,
        CanDelete: (fn.DirectGrant.CanDelete || fn.InheritedGrant.CanDelete) && fn.SupportedCapabilities.CanDelete,
        CanPrint: (fn.DirectGrant.CanPrint || fn.InheritedGrant.CanPrint) && fn.SupportedCapabilities.CanPrint,
      };
    }

    setChannelTrees((prev) => ({ ...prev, [activeChannelTab]: updated }));
  };

  const handleToggleRowAll = (funcCode: string, checked: boolean) => {
    if (!currentTree) return;
    setIsDirty(true);
    const updated = { ...currentTree };
    const fn = updated.Functions.find((f) => f.FunctionCode === funcCode);
    if (!fn) return;

    const caps = fn.SupportedCapabilities;
    fn.DirectGrant = {
      CanView: checked && caps.CanView,
      CanAdd: checked && caps.CanAdd,
      CanEdit: checked && caps.CanEdit,
      CanDelete: checked && caps.CanDelete,
      CanPrint: checked && caps.CanPrint,
    };

    if (updated.ParentIsEffective) {
      fn.EffectiveGrant = {
        CanView: (fn.DirectGrant.CanView || fn.InheritedGrant.CanView) && caps.CanView,
        CanAdd: (fn.DirectGrant.CanAdd || fn.InheritedGrant.CanAdd) && caps.CanAdd,
        CanEdit: (fn.DirectGrant.CanEdit || fn.InheritedGrant.CanEdit) && caps.CanEdit,
        CanDelete: (fn.DirectGrant.CanDelete || fn.InheritedGrant.CanDelete) && caps.CanDelete,
        CanPrint: (fn.DirectGrant.CanPrint || fn.InheritedGrant.CanPrint) && caps.CanPrint,
      };
    }

    setChannelTrees((prev) => ({ ...prev, [activeChannelTab]: updated }));
  };

  const handleToggleSubsystemAll = (check: boolean) => {
    if (!currentTree) return;
    setIsDirty(true);
    const updated = { ...currentTree };

    updated.Functions.forEach((fn) => {
      const matchCategory =
        subsystemFilter === 'ALL' ||
        (fn.ParentCode || 'OTHER').toUpperCase() === subsystemFilter.toUpperCase();

      if (!matchCategory) return;

      const caps = fn.SupportedCapabilities;
      fn.DirectGrant = {
        CanView: check && caps.CanView,
        CanAdd: check && caps.CanAdd,
        CanEdit: check && caps.CanEdit,
        CanDelete: check && caps.CanDelete,
        CanPrint: check && caps.CanPrint,
      };

      if (updated.ParentIsEffective) {
        fn.EffectiveGrant = {
          CanView: (fn.DirectGrant.CanView || fn.InheritedGrant.CanView) && caps.CanView,
          CanAdd: (fn.DirectGrant.CanAdd || fn.InheritedGrant.CanAdd) && caps.CanAdd,
          CanEdit: (fn.DirectGrant.CanEdit || fn.InheritedGrant.CanEdit) && caps.CanEdit,
          CanDelete: (fn.DirectGrant.CanDelete || fn.InheritedGrant.CanDelete) && caps.CanDelete,
          CanPrint: (fn.DirectGrant.CanPrint || fn.InheritedGrant.CanPrint) && caps.CanPrint,
        };
      }
    });

    setChannelTrees((prev) => ({ ...prev, [activeChannelTab]: updated }));
  };

  const handleGrantViewAll = () => {
    if (!currentTree) return;
    setIsDirty(true);
    const updated = { ...currentTree };

    updated.Functions.forEach((fn) => {
      const matchCategory =
        subsystemFilter === 'ALL' ||
        (fn.ParentCode || 'OTHER').toUpperCase() === subsystemFilter.toUpperCase();

      if (!matchCategory) return;

      if (fn.SupportedCapabilities.CanView) {
        fn.DirectGrant.CanView = true;
      }

      if (updated.ParentIsEffective) {
        fn.EffectiveGrant.CanView =
          (fn.DirectGrant.CanView || fn.InheritedGrant.CanView) && fn.SupportedCapabilities.CanView;
      }
    });

    setChannelTrees((prev) => ({ ...prev, [activeChannelTab]: updated }));
  };

  const handleSaveChannel = async (channelKey: string): Promise<boolean> => {
    const tree = channelTrees[channelKey];
    if (!tree) return false;

    setSavingChannel(channelKey);
    setStatus('saving');
    try {
      const payload = {
        TargetUserId: userId,
        Channel: channelKey,
        ExpectedSecurityVersion: securityVersion,
        ParentDirectGrant: tree.ParentDirectGrant,
        Functions: tree.Functions.map((f) => ({
          FunctionCode: f.FunctionCode,
          CanView: f.DirectGrant.CanView,
          CanAdd: f.DirectGrant.CanAdd,
          CanEdit: f.DirectGrant.CanEdit,
          CanDelete: f.DirectGrant.CanDelete,
          CanPrint: f.DirectGrant.CanPrint,
        })),
      };

      const res = await api.post(`/users/${userId}/channel-permissions`, payload);
      if (res.data?.Success) {
        if (res.data.NewSecurityVersion) {
          setSecurityVersion(res.data.NewSecurityVersion);
        }
        if (res.data.UpdatedChannelTree) {
          setChannelTrees((prev) => ({ ...prev, [channelKey]: res.data.UpdatedChannelTree }));
        }
        setIsDirty(false);
        setStatus('ready');

        if (res.data.RequiresReLogin) {
          Modal.info({
            title: t('user.reLoginRequiredTitle') || 'Yêu cầu đăng nhập lại',
            content:
              t('user.reLoginRequiredMsg') ||
              'Bạn vừa cập nhật quyền của chính tài khoản mình hoặc nhóm chứa mình. Phiên làm việc hiện tại cần được đăng nhập lại để các quyền mới có hiệu lực.',
            onOk: () => {
              onClose();
              if (onSuccess) onSuccess();
            },
          });
          return true;
        }

        message.success(
          t('user.saveChannelSuccess', { channel: tree.ChannelLabel }) ||
            `Đã lưu phân quyền kênh [${tree.ChannelLabel}] thành công!`
        );
        if (onSuccess) onSuccess();
        return true;
      } else {
        if (res.data?.IsConflict) {
          setStatus('conflict');
          Modal.confirm({
            title: t('user.concurrencyConflictTitle') || 'Xung đột phiên bản',
            content:
              t('user.concurrencyConflictContent') ||
              'Dữ liệu phân quyền đã được quản trị viên khác cập nhật. Bạn có muốn tải lại phiên bản mới nhất?',
            onOk: () => loadChannelPermissions(),
          });
        } else {
          message.error(res.data?.Message || 'Lỗi khi lưu phân quyền kênh.');
          setStatus('ready');
        }
        return false;
      }
    } catch (err: any) {
      if (err?.response?.status === 409 || err?.response?.data?.IsConflict) {
        setStatus('conflict');
        Modal.confirm({
          title: t('user.concurrencyConflictTitle') || 'Xung đột phiên bản',
          content:
            t('user.concurrencyConflictContent') ||
            'Dữ liệu phân quyền đã được quản trị viên khác cập nhật. Bạn có muốn tải lại phiên bản mới nhất?',
          onOk: () => loadChannelPermissions(),
        });
      } else {
        message.error(err?.response?.data?.Message || 'Lỗi kết nối khi lưu phân quyền.');
        setStatus('ready');
      }
      return false;
    } finally {
      setSavingChannel(null);
    }
  };

  const handleSaveAll = async () => {
    setSavingAll(true);
    setStatus('saving');
    try {
      const channelPayloads = ['DESKTOP', 'WEB', 'MOBILE']
        .map((ch) => {
          const tree = channelTrees[ch];
          if (!tree) return null;
          return {
            Channel: ch,
            ParentDirectGrant: tree.ParentDirectGrant,
            Functions: tree.Functions.map((f) => ({
              FunctionCode: f.FunctionCode,
              CanView: f.DirectGrant.CanView,
              CanAdd: f.DirectGrant.CanAdd,
              CanEdit: f.DirectGrant.CanEdit,
              CanDelete: f.DirectGrant.CanDelete,
              CanPrint: f.DirectGrant.CanPrint,
            })),
          };
        })
        .filter(Boolean);

      const batchPayload = {
        TargetUserId: userId,
        ExpectedSecurityVersion: securityVersion,
        Channels: channelPayloads,
      };

      const res = await api.post(`/users/${userId}/channel-permissions/batch`, batchPayload);
      if (res.data?.Success) {
        if (res.data.NewSecurityVersion) {
          setSecurityVersion(res.data.NewSecurityVersion);
        }
        if (res.data.UpdatedChannelTrees && Array.isArray(res.data.UpdatedChannelTrees)) {
          const treeMap: Record<string, PlatformChannelTreeDTO> = {};
          res.data.UpdatedChannelTrees.forEach((t: PlatformChannelTreeDTO) => {
            treeMap[t.Channel] = t;
          });
          setChannelTrees((prev) => ({ ...prev, ...treeMap }));
        }
        setIsDirty(false);
        setStatus('ready');

        if (res.data.RequiresReLogin) {
          Modal.info({
            title: t('user.reLoginRequiredTitle') || 'Yêu cầu đăng nhập lại',
            content:
              t('user.reLoginRequiredMsg') ||
              'Bạn vừa cập nhật quyền của chính tài khoản mình hoặc nhóm chứa mình. Phiên làm việc hiện tại cần được đăng nhập lại để các quyền mới có hiệu lực.',
            onOk: () => {
              onClose();
              if (onSuccess) onSuccess();
            },
          });
          return;
        }

        message.success(t('user.saveAllSuccess') || 'Đã lưu phân quyền cho tất cả các nền tảng thành công!');
        if (onSuccess) onSuccess();
      } else {
        if (res.data?.IsConflict) {
          setStatus('conflict');
          Modal.confirm({
            title: t('user.concurrencyConflictTitle') || 'Xung đột phiên bản',
            content:
              t('user.concurrencyConflictContent') ||
              'Dữ liệu phân quyền đã được quản trị viên khác cập nhật. Bạn có muốn tải lại phiên bản mới nhất?',
            onOk: () => loadChannelPermissions(),
          });
        } else {
          message.error(res.data?.Message || 'Lỗi khi lưu phân quyền toàn bộ các kênh.');
          setStatus('ready');
        }
      }
    } catch (err: any) {
      if (err?.response?.status === 409 || err?.response?.data?.IsConflict) {
        setStatus('conflict');
        Modal.confirm({
          title: t('user.concurrencyConflictTitle') || 'Xung đột phiên bản',
          content:
            t('user.concurrencyConflictContent') ||
            'Dữ liệu phân quyền đã được quản trị viên khác cập nhật. Bạn có muốn tải lại phiên bản mới nhất?',
          onOk: () => loadChannelPermissions(),
        });
      } else {
        message.error(err?.response?.data?.Message || 'Lỗi kết nối khi lưu phân quyền toàn bộ các kênh.');
        setStatus('ready');
      }
    } finally {
      setSavingAll(false);
    }
  };

  const getSubsystemMeta = (parent?: string) => {
    switch (parent?.toUpperCase()) {
      case 'NV':
        return { label: t('user.subsystemNV') || 'Nhân sự & Hợp đồng', color: 'blue' };
      case 'DM':
        return { label: t('user.subsystemDM') || 'Danh mục', color: 'cyan' };
      case 'CC':
        return { label: t('user.subsystemCC') || 'Chấm công & Lương', color: 'gold' };
      case 'BC':
        return { label: t('user.subsystemBC') || 'Báo cáo', color: 'green' };
      case 'SYSTEM':
        return { label: t('user.subsystemSYSTEM') || 'Hệ thống & AI', color: 'purple' };
      default:
        return { label: parent || t('user.subsystemOTHER') || 'Khác', color: 'default' };
    }
  };

  const getCleanFunctionName = (code: string, rawName: string): string => {
    const i18nKey = `func.${code}`;
    const translated = t(i18nKey);
    if (translated && translated !== i18nKey) {
      return translated;
    }
    if (CANONICAL_FUNCTION_NAMES[code]) {
      return CANONICAL_FUNCTION_NAMES[code];
    }
    if (hasMojibake(rawName)) {
      return CANONICAL_FUNCTION_NAMES[code] || code;
    }
    return rawName || code;
  };

  const filteredFunctions = useMemo(() => {
    if (!currentTree) return [];
    return currentTree.Functions.filter((fn) => {
      const cleanName = getCleanFunctionName(fn.FunctionCode, fn.FunctionName);
      const matchesSearch =
        fn.FunctionCode.toLowerCase().includes(searchText.toLowerCase()) ||
        cleanName.toLowerCase().includes(searchText.toLowerCase());

      if (!matchesSearch) return false;

      if (subsystemFilter !== 'ALL') {
        if ((fn.ParentCode || 'OTHER').toUpperCase() !== subsystemFilter.toUpperCase()) {
          return false;
        }
      }

      if (onlyGrantedFilter) {
        const hasDirect =
          fn.DirectGrant.CanView ||
          fn.DirectGrant.CanAdd ||
          fn.DirectGrant.CanEdit ||
          fn.DirectGrant.CanDelete ||
          fn.DirectGrant.CanPrint;
        const hasInherited =
          fn.InheritedGrant.CanView ||
          fn.InheritedGrant.CanAdd ||
          fn.InheritedGrant.CanEdit ||
          fn.InheritedGrant.CanDelete ||
          fn.InheritedGrant.CanPrint;
        if (!hasDirect && !hasInherited) return false;
      }

      return true;
    });
  }, [currentTree, searchText, subsystemFilter, onlyGrantedFilter]);

  const renderActionCell = (
    item: ChannelFunctionRightItemDTO,
    action: 'CanView' | 'CanAdd' | 'CanEdit' | 'CanDelete' | 'CanPrint'
  ) => {
    const isSupported = item.SupportedCapabilities[action];
    const isDirect = item.DirectGrant[action];
    const isInherited = item.InheritedGrant[action];
    const isEffective = item.EffectiveGrant[action];
    const isParentOff = !currentTree?.ParentIsEffective;

    if (!isSupported) {
      return (
        <Tooltip title={item.RestrictionNote || t('user.notSupportedOnChannel') || 'Không hỗ trợ trên nền tảng này'}>
          <Text type="secondary" style={{ color: '#d9d9d9', userSelect: 'none' }}>
            —
          </Text>
        </Tooltip>
      );
    }

    return (
      <Space orientation="horizontal" size={4} align="center">
        <Checkbox
          checked={isDirect}
          disabled={isParentOff}
          onChange={(e) => handleToggleAction(item.FunctionCode, action, e.target.checked)}
        />
        {isInherited && (
          <Tooltip
            title={
              item.InheritedFromGroupNames?.length
                ? t('user.inheritedFromGroup', { groups: item.InheritedFromGroupNames.join(', ') })
                : 'Kế thừa từ nhóm'
            }
          >
            <Tag color="geekblue" style={{ fontSize: 10, padding: '0 4px', margin: 0, lineHeight: '16px' }}>
              <TeamOutlined />
            </Tag>
          </Tooltip>
        )}
        {isEffective && !isDirect && !isInherited && (
          <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 12 }} />
        )}
      </Space>
    );
  };

  const columns: ColumnsType<ChannelFunctionRightItemDTO> = [
    {
      title: t('user.colFunctionCode') || 'Mã chức năng',
      dataIndex: 'FunctionCode',
      key: 'FunctionCode',
      width: 175,
      render: (code: string) => (
        <Tag color="geekblue" style={{ fontWeight: 600, fontFamily: 'monospace' }}>
          {code}
        </Tag>
      ),
    },
    {
      title: t('user.colFunctionName') || 'Tên chức năng',
      dataIndex: 'FunctionName',
      key: 'FunctionName',
      render: (name: string, record) => {
        const cleanName = getCleanFunctionName(record.FunctionCode, name);
        return (
          <Space direction="vertical" size={0}>
            <Text strong>{cleanName}</Text>
            {record.RestrictionNote && (
              <Text type="secondary" style={{ fontSize: 11, color: '#fa8c16' }}>
                <InfoCircleOutlined /> {record.RestrictionNote}
              </Text>
            )}
          </Space>
        );
      },
    },
    {
      title: t('user.colSubsystem') || 'Phân hệ',
      dataIndex: 'ParentCode',
      key: 'ParentCode',
      width: 140,
      render: (parent: string) => {
        const meta = getSubsystemMeta(parent);
        return <Tag color={meta.color}>{meta.label}</Tag>;
      },
    },
    {
      title: (
        <Tooltip title={t('common.selectAll') || 'Chọn toàn bộ dòng'}>
          <Text style={{ fontSize: 12 }}>{t('user.colRowAll') || 'Dòng'}</Text>
        </Tooltip>
      ),
      key: 'RowToggle',
      width: 65,
      align: 'center',
      render: (_, record) => {
        const caps = record.SupportedCapabilities;
        const anySupported = caps.CanView || caps.CanAdd || caps.CanEdit || caps.CanDelete || caps.CanPrint;
        if (!anySupported) return <Text type="secondary">—</Text>;

        const isRowAll =
          (!caps.CanView || record.DirectGrant.CanView) &&
          (!caps.CanAdd || record.DirectGrant.CanAdd) &&
          (!caps.CanEdit || record.DirectGrant.CanEdit) &&
          (!caps.CanDelete || record.DirectGrant.CanDelete) &&
          (!caps.CanPrint || record.DirectGrant.CanPrint);

        return (
          <Checkbox
            checked={isRowAll}
            disabled={!currentTree?.ParentIsEffective}
            onChange={(e) => handleToggleRowAll(record.FunctionCode, e.target.checked)}
          />
        );
      },
    },
    {
      title: (
        <Space size={4}>
          <CheckCircleOutlined style={{ color: '#1677ff' }} />
          <span>{t('user.colView') || 'Xem'}</span>
        </Space>
      ),
      key: 'CanView',
      width: 85,
      align: 'center',
      render: (_, record) => renderActionCell(record, 'CanView'),
    },
    {
      title: (
        <Space size={4}>
          <SafetyCertificateOutlined style={{ color: '#52c41a' }} />
          <span>{t('user.colAdd') || 'Thêm'}</span>
        </Space>
      ),
      key: 'CanAdd',
      width: 85,
      align: 'center',
      render: (_, record) => renderActionCell(record, 'CanAdd'),
    },
    {
      title: (
        <Space size={4}>
          <SafetyCertificateOutlined style={{ color: '#fa8c16' }} />
          <span>{t('user.colEdit') || 'Sửa'}</span>
        </Space>
      ),
      key: 'CanEdit',
      width: 85,
      align: 'center',
      render: (_, record) => renderActionCell(record, 'CanEdit'),
    },
    {
      title: (
        <Space size={4}>
          <SafetyCertificateOutlined style={{ color: '#ff4d4f' }} />
          <span>{t('user.colDelete') || 'Xóa'}</span>
        </Space>
      ),
      key: 'CanDelete',
      width: 85,
      align: 'center',
      render: (_, record) => renderActionCell(record, 'CanDelete'),
    },
    {
      title: (
        <Space size={4}>
          <SafetyCertificateOutlined style={{ color: '#722ed1' }} />
          <span>{t('user.colPrint') || 'In'}</span>
        </Space>
      ),
      key: 'CanPrint',
      width: 85,
      align: 'center',
      render: (_, record) => renderActionCell(record, 'CanPrint'),
    },
  ];

  return (
    <Modal
      title={
        <Space align="center" size={8}>
          <SafetyCertificateOutlined style={{ color: '#1677ff', fontSize: 20 }} />
          <div>
            <Title level={5} style={{ margin: 0 }}>
              {t('user.modalPermissionTitle', { target: fullName || username }) ||
                `PHÂN QUYỀN HỆ THỐNG • ${fullName || username}`}
            </Title>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {isGroup
                ? `${t('user.targetGroup') || 'Nhóm người dùng'}: [${username}]`
                : `${t('user.targetAccount') || 'Tài khoản'}: [${username}]`}
            </Text>
          </div>
        </Space>
      }
      open={visible}
      onCancel={handleRequestClose}
      width={1120}
      destroyOnClose
      footer={
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Space>
            <Button
              icon={<ReloadOutlined spin={loading} />}
              onClick={handleRequestReload}
              disabled={loading || savingAll || savingChannel !== null}
            >
              {t('common.refresh') || 'Tải lại'}
            </Button>
            {isDirty && (
              <Tag color="warning" style={{ margin: 0 }}>
                {t('common.warning') || 'Chưa lưu thay đổi'}
              </Tag>
            )}
          </Space>
          <Space>
            <Button onClick={handleRequestClose} disabled={savingAll || savingChannel !== null}>
              {t('common.close') || 'Đóng'}
            </Button>
            <Button
              type="primary"
              icon={<SaveOutlined />}
              loading={savingChannel === activeChannelTab}
              disabled={savingAll || loading || status === 'load-error'}
              onClick={() => handleSaveChannel(activeChannelTab)}
            >
              {t('user.btnSaveChannel', { channel: currentTree?.ChannelLabel || activeChannelTab }) ||
                `Lưu kênh [${currentTree?.ChannelLabel || activeChannelTab}]`}
            </Button>
            <Button
              type="primary"
              style={{ background: '#722ed1', borderColor: '#722ed1' }}
              icon={<SafetyCertificateOutlined />}
              loading={savingAll}
              disabled={loading || savingChannel !== null || status === 'load-error'}
              onClick={handleSaveAll}
            >
              {t('user.btnSaveAllChannels') || 'Lưu tất cả các kênh'}
            </Button>
          </Space>
        </div>
      }
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 8 }}>
        {/* PLATFORM TABS */}
        <Tabs
          activeKey={activeChannelTab}
          onChange={(k) => setActiveChannelTab(k as 'DESKTOP' | 'WEB' | 'MOBILE')}
          items={[
            {
              key: 'DESKTOP',
              label: (
                <Space>
                  <DesktopOutlined />
                  <span>{t('user.tabDesktop') || 'Desktop'}</span>
                  {channelTrees['DESKTOP']?.ParentIsEffective ? (
                    <Badge status="success" />
                  ) : (
                    <Badge status="default" />
                  )}
                </Space>
              ),
            },
            {
              key: 'WEB',
              label: (
                <Space>
                  <GlobalOutlined />
                  <span>{t('user.tabWeb') || 'Website'}</span>
                  {channelTrees['WEB']?.ParentIsEffective ? (
                    <Badge status="success" />
                  ) : (
                    <Badge status="default" />
                  )}
                </Space>
              ),
            },
            {
              key: 'MOBILE',
              label: (
                <Space>
                  <MobileOutlined />
                  <span>{t('user.tabMobile') || 'Mobile'}</span>
                  {channelTrees['MOBILE']?.ParentIsEffective ? (
                    <Badge status="success" />
                  ) : (
                    <Badge status="default" />
                  )}
                </Space>
              ),
            },
          ]}
        />

        {/* CHANNEL HEADER CARD */}
        {currentTree && (
          <Card
            size="small"
            style={{
              background: currentTree.ParentIsEffective ? '#f6ffed' : '#fffbe6',
              borderColor: currentTree.ParentIsEffective ? '#b7eb8f' : '#ffe58f',
            }}
          >
            <Row align="middle" justify="space-between" gutter={[12, 8]}>
              <Col xs={24} md={12}>
                <Space align="center" size={12}>
                  <Checkbox
                    checked={currentTree.ParentDirectGrant}
                    onChange={(e) => handleToggleChannelParent(e.target.checked)}
                  >
                    <Text strong style={{ fontSize: 14 }}>
                      {t('user.allowPlatformLogin') || 'Cho phép đăng nhập trên nền tảng này'}{' '}
                      <Tag color="blue">{currentTree.ParentFunctionCode}</Tag>
                    </Text>
                  </Checkbox>
                </Space>
              </Col>
              <Col xs={24} md={12} style={{ textAlign: 'right' }}>
                <Space wrap size={8}>
                  <Tag color={currentTree.ParentDirectGrant ? 'green' : 'default'}>
                    {t('user.directGrant') || 'Trực tiếp'}:{' '}
                    {currentTree.ParentDirectGrant
                      ? t('common.active') || 'Bật'
                      : t('common.inactive') || 'Tắt'}
                  </Tag>
                  <Tag color={currentTree.ParentInheritedGrant ? 'geekblue' : 'default'}>
                    {t('user.inheritedGrant') || 'Từ nhóm'}:{' '}
                    {currentTree.ParentInheritedGrant
                      ? currentTree.ParentInheritedGroupNames?.join(', ') || t('common.active') || 'Bật'
                      : t('common.no') || 'Không'}
                  </Tag>
                  <Tag color={currentTree.ParentIsEffective ? 'success' : 'error'}>
                    {t('user.effectiveGrant') || 'Có hiệu lực'}:{' '}
                    {currentTree.ParentIsEffective
                      ? t('common.yes') || 'Sẵn sàng'
                      : t('common.no') || 'Vô hiệu'}
                  </Tag>
                  {currentTree.ReadinessMessage && (
                    <Tag color="cyan">
                      {currentTree.ReadinessMessage}
                    </Tag>
                  )}
                </Space>
              </Col>
            </Row>

            {!currentTree.ParentIsEffective && (
              <Alert
                type="warning"
                showIcon
                style={{ marginTop: 8 }}
                message={
                  t('user.parentDisabledWarning') ||
                  'Quyền đăng nhập nền tảng đang tắt. Các quyền chức năng bên dưới tạm thời không có hiệu lực trên nền tảng này.'
                }
              />
            )}
          </Card>
        )}

        {/* FILTER & TOOLBAR */}
        <Row justify="space-between" align="middle" gutter={[8, 8]}>
          <Col xs={24} md={14}>
            <Space wrap size={8}>
              <Segmented
                value={subsystemFilter}
                onChange={(v) => setSubsystemFilter(v as string)}
                options={[
                  { label: t('user.filterAll') || 'Tất cả', value: 'ALL' },
                  { label: 'NV', value: 'NV' },
                  { label: 'CC', value: 'CC' },
                  { label: 'DM', value: 'DM' },
                  { label: 'BC', value: 'BC' },
                  { label: 'SYSTEM', value: 'SYSTEM' },
                ]}
              />
              <Input
                placeholder={t('common.searchPlaceholder') || 'Tìm kiếm mã hoặc tên chức năng...'}
                prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                allowClear
                style={{ width: 220 }}
              />
              <Checkbox
                checked={onlyGrantedFilter}
                onChange={(e) => setOnlyGrantedFilter(e.target.checked)}
              >
                {t('user.filterOnlyGranted') || 'Chỉ hiện quyền đã cấp'}
              </Checkbox>
            </Space>
          </Col>
          <Col xs={24} md={10} style={{ textAlign: 'right' }}>
            <Space size={6}>
              <Button
                size="small"
                onClick={() => handleToggleSubsystemAll(true)}
                disabled={!currentTree?.ParentIsEffective}
              >
                {t('user.btnSelectSubsystem') || 'Bật phân hệ'}
              </Button>
              <Button
                size="small"
                onClick={() => handleToggleSubsystemAll(false)}
                disabled={!currentTree?.ParentIsEffective}
              >
                {t('user.btnDeselectSubsystem') || 'Tắt phân hệ'}
              </Button>
              <Button
                size="small"
                type="dashed"
                onClick={handleGrantViewAll}
                disabled={!currentTree?.ParentIsEffective}
              >
                {t('user.btnGrantViewAll') || 'Bật Xem toàn bộ'}
              </Button>
            </Space>
          </Col>
        </Row>

        {/* TABLE */}
        <Table
          size="small"
          rowKey="FunctionCode"
          loading={loading}
          columns={columns}
          dataSource={filteredFunctions}
          pagination={false}
          scroll={{ y: 380 }}
          bordered
        />
      </div>
    </Modal>
  );
};

export default PhanQuyenModal;
