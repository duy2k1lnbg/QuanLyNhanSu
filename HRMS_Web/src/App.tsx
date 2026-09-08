import { useState, useEffect } from 'react';
import {
  Layout,
  Menu,
  Typography,
  Card,
  Row,
  Col,
  Statistic,
  Table,
  Tag,
  Avatar,
  Space,
  Button,
  Input,
  Drawer,
  Badge,
  Tooltip,
  notification,
  theme,
  Select,
  Modal,
  Form,
  Popconfirm,
  Tabs,
  Spin,
  Dropdown,
  InputNumber,
  DatePicker,
  Segmented,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  DashboardOutlined,
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  FileTextOutlined,
  RobotOutlined,
  BellOutlined,
  UserOutlined,
  SendOutlined,
  SearchOutlined,
  PlusOutlined,
  CheckCircleOutlined,
  CloudSyncOutlined,
  ReloadOutlined,
  CalculatorOutlined,
  DeleteOutlined,
  EditOutlined,
  LogoutOutlined,
  SafetyCertificateOutlined,
  TrophyOutlined,
  SwapOutlined,
  RiseOutlined,
  PrinterOutlined,
  SettingOutlined,
  LockOutlined,
  KeyOutlined,
} from '@ant-design/icons';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip as RechartsTooltip,
  Legend,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import dayjs from 'dayjs';
import api from './services/api';
import Login from './pages/Login';
import PhanQuyenModal from './components/PhanQuyenModal';
import GroupMembersModal from './components/GroupMembersModal';
import ChangePasswordModal from './components/ChangePasswordModal';
import UserEditModal from './components/UserEditModal';
import PhieuLuongModal from './components/PhieuLuongModal';
import type {
  NhanVienDTO,
  DashboardLuongDTO,
  DashboardPhongBanDTO,
  AIChatMessage,
  BangLuongDTO,
  KyCongDTO,
  KyCongChiTietDTO,
  HopDongDTO,
  LoaiCaDTO,
  DanhMucAllDTO,
  CurrentUserDTO,
  SysUserDTO,
  KhenThuongDTO,
  NangLuongDTO,
  DieuChuyenDTO,
  UngLuongDTO,
  TangCaDTO,
} from './types/hrms';

const { Header, Content, Sider } = Layout;
const { Title, Text } = Typography;

const PIE_COLORS = ['#1677ff', '#52c41a', '#fa8c16', '#722ed1', '#13c2c2', '#eb2f96'];

export function App() {
  // Authentication State
  const [currentUser, setCurrentUser] = useState<CurrentUserDTO | null>(() => {
    const saved = localStorage.getItem('hrms_user');
    if (saved) {
      try {
        const u = JSON.parse(saved);
        if (!u) return null;
        return {
          IdUser: u.IdUser ?? u.id ?? 0,
          Username: u.Username || u.username || '',
          FullName: u.FullName || u.fullName || u.Username || u.username || '',
          IsAdmin: Boolean(u.IsAdmin ?? u.isAdmin),
          Rights: u.Rights || u.rights || [],
        };
      } catch {
        return null;
      }
    }
    return null;
  });

  const [collapsed, setCollapsed] = useState(false);
  const [currentMenu, setCurrentMenu] = useState('dashboard');
  const [aiDrawerVisible, setAiDrawerVisible] = useState(false);
  const [chatInput, setChatInput] = useState('');
  const [chatLoading, setChatLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');

  // States dữ liệu chính
  const [nhanVienList, setNhanVienList] = useState<NhanVienDTO[]>([]);
  const [luongStats, setLuongStats] = useState<DashboardLuongDTO[]>([]);
  const [phongBanStats, setPhongBanStats] = useState<DashboardPhongBanDTO[]>([]);
  const [totalEmployees, setTotalEmployees] = useState<number>(0);
  const [totalSalary, setTotalSalary] = useState<number>(0);
  const [isBackendConnected, setIsBackendConnected] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  // States Danh mục & Kỳ công
  const [danhMuc, setDanhMuc] = useState<DanhMucAllDTO | null>(null);
  const [kyCongList, setKyCongList] = useState<KyCongDTO[]>([]);
  const [selectedKyCong, setSelectedKyCong] = useState<number>(0);

  // States Bảng lương & Chấm công
  const [bangLuongList, setBangLuongList] = useState<BangLuongDTO[]>([]);
  const [bangLuongLoading, setBangLuongLoading] = useState<boolean>(false);
  const [tinhLuongLoading, setTinhLuongLoading] = useState<boolean>(false);
  const [chamCongList, setChamCongList] = useState<KyCongChiTietDTO[]>([]);
  const [loaiCaList, setLoaiCaList] = useState<LoaiCaDTO[]>([]);
  const [chamCongLoading, setChamCongLoading] = useState<boolean>(false);

  // States Hợp đồng
  const [hopDongList, setHopDongList] = useState<HopDongDTO[]>([]);
  const [hopDongLoading, setHopDongLoading] = useState<boolean>(false);

  // States Khen thưởng & Kỷ luật
  const [khenThuongList, setKhenThuongList] = useState<KhenThuongDTO[]>([]);
  const [kyLuatList, setKyLuatList] = useState<KhenThuongDTO[]>([]);
  const [ktLoading, setKtLoading] = useState<boolean>(false);
  const [ktModalVisible, setKtModalVisible] = useState<boolean>(false);
  const [formKt] = Form.useForm();

  // States Nâng lương & Điều chuyển
  const [nangLuongList, setNangLuongList] = useState<NangLuongDTO[]>([]);
  const [dieuChuyenList, setDieuChuyenList] = useState<DieuChuyenDTO[]>([]);
  const [nlDcLoading, setNlDcLoading] = useState<boolean>(false);
  const [nlModalVisible, setNlModalVisible] = useState<boolean>(false);
  const [dcModalVisible, setDcModalVisible] = useState<boolean>(false);
  const [formNl] = Form.useForm();
  const [formDc] = Form.useForm();

  // States Tăng ca & Ứng lương
  const [ungLuongList, setUngLuongList] = useState<UngLuongDTO[]>([]);
  const [tangCaList, setTangCaList] = useState<TangCaDTO[]>([]);
  const [tcUlLoading, setTcUlLoading] = useState<boolean>(false);
  const [ulModalVisible, setUlModalVisible] = useState<boolean>(false);
  const [tcModalVisible, setTcModalVisible] = useState<boolean>(false);
  const [formUl] = Form.useForm();
  const [formTc] = Form.useForm();

  // States Quản trị Người dùng & Phân quyền (RBAC WinForms)
  const [userList, setUserList] = useState<SysUserDTO[]>([]);
  const [userLoading, setUserLoading] = useState<boolean>(false);
  const [userTab, setUserTab] = useState<'users' | 'groups'>('users');
  const [userSearchText, setUserSearchText] = useState<string>('');
  const [selectedUserForPerms, setSelectedUserForPerms] = useState<SysUserDTO | null>(null);
  const [phanQuyenModalVisible, setPhanQuyenModalVisible] = useState<boolean>(false);
  const [createUserModalVisible, setCreateUserModalVisible] = useState<boolean>(false);
  const [selectedGroupForMembers, setSelectedGroupForMembers] = useState<SysUserDTO | null>(null);
  const [groupMembersModalVisible, setGroupMembersModalVisible] = useState<boolean>(false);
  const [userEditModalVisible, setUserEditModalVisible] = useState<boolean>(false);
  const [selectedUserForEdit, setSelectedUserForEdit] = useState<SysUserDTO | null>(null);
  const [changePasswordModalVisible, setChangePasswordModalVisible] = useState<boolean>(false);
  const [isCreatingGroup, setIsCreatingGroup] = useState<boolean>(false);
  const [formCreateUser] = Form.useForm();

  // State Phiếu lương Modal
  const [selectedBangLuong, setSelectedBangLuong] = useState<BangLuongDTO | null>(null);
  const [phieuLuongModalVisible, setPhieuLuongModalVisible] = useState<boolean>(false);

  // State Modal Nhân viên
  const [nvModalVisible, setNvModalVisible] = useState<boolean>(false);
  const [editingNv, setEditingNv] = useState<NhanVienDTO | null>(null);
  const [formNv] = Form.useForm();

  // Chat Messages AI
  const [chatMessages, setChatMessages] = useState<AIChatMessage[]>([
    {
      id: '1',
      sender: 'assistant',
      content:
        'Xin chào! Tôi là Trợ lý AI HRMS Copilot (kết nối trực tiếp cơ sở dữ liệu Oracle HR). Bạn cần tra cứu luật lao động, số liệu phòng ban hay chính sách nhân sự nào?',
      timestamp: 'Vừa xong',
      source: 'Oracle_Live_Assistant',
    },
  ]);

  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken();

  // Check quyền tương tự WinForms (hỗ trợ cả mã F_DM_..., F_CC_..., v.v.)
  const hasRight = (...codes: string[]) => {
    if (!currentUser) return false;
    if (currentUser.IsAdmin || currentUser.Rights?.includes('*')) return true;
    return codes.some((code) =>
      currentUser.Rights?.some(
        (r) =>
          r.toUpperCase() === code.toUpperCase() ||
          r.toUpperCase().includes(code.toUpperCase()) ||
          code.toUpperCase().includes(r.toUpperCase())
      )
    );
  };

  // 1. Tải dữ liệu ban đầu
  const fetchInitialData = async () => {
    setLoading(true);
    try {
      const [nvRes, statsRes, dmRes, kcRes] = await Promise.all([
        api.get<NhanVienDTO[]>('/nhanvien'),
        api.get<{
          tongNhanVien: number;
          tongQuyLuong: number;
          phongBanStats: DashboardPhongBanDTO[];
          luongStats: DashboardLuongDTO[];
        }>('/dashboard/stats'),
        api.get<DanhMucAllDTO>('/danhmuc/all'),
        api.get<KyCongDTO[]>('/bangluong/kycong'),
      ]);

      if (nvRes.data) {
        setNhanVienList(nvRes.data);
        setTotalEmployees(nvRes.data.length);
        setIsBackendConnected(true);
      }

      if (statsRes.data) {
        if (statsRes.data.tongNhanVien > 0) setTotalEmployees(statsRes.data.tongNhanVien);
        if (statsRes.data.tongQuyLuong > 0) setTotalSalary(statsRes.data.tongQuyLuong);
        if (statsRes.data.phongBanStats) setPhongBanStats(statsRes.data.phongBanStats);
        if (statsRes.data.luongStats) setLuongStats(statsRes.data.luongStats);
      }

      if (dmRes.data) setDanhMuc(dmRes.data);

      if (kcRes.data && kcRes.data.length > 0) {
        setKyCongList(kcRes.data);
        setSelectedKyCong(kcRes.data[0].MAKYCONG);
      }
    } catch {
      setIsBackendConnected(false);
    } finally {
      setLoading(false);
    }
  };

  // 2. Tải Bảng lương theo kỳ công
  const fetchBangLuong = async (maKc: number) => {
    setBangLuongLoading(true);
    try {
      const res = await api.get<{ makycong: number; total: number; items: BangLuongDTO[] }>(
        `/bangluong?makycong=${maKc}`
      );
      if (res.data && res.data.items) {
        setBangLuongList(res.data.items);
      }
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải bảng lương.' });
    } finally {
      setBangLuongLoading(false);
    }
  };

  // 3. Tải Bảng chấm công
  const fetchChamCong = async (maKc: number) => {
    setChamCongLoading(true);
    try {
      const [ccRes, lcRes] = await Promise.all([
        api.get<{ makycong: number; total: number; items: KyCongChiTietDTO[] }>(
          `/chamcong/kycongchitiet?makycong=${maKc}`
        ),
        api.get<LoaiCaDTO[]>('/chamcong/loaica'),
      ]);

      if (ccRes.data && ccRes.data.items) setChamCongList(ccRes.data.items);
      if (lcRes.data) setLoaiCaList(lcRes.data);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải dữ liệu chấm công.' });
    } finally {
      setChamCongLoading(false);
    }
  };

  // 4. Tải Hợp đồng
  const fetchHopDong = async () => {
    setHopDongLoading(true);
    try {
      const res = await api.get<HopDongDTO[]>('/hopdong');
      if (res.data) setHopDongList(res.data);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải danh sách hợp đồng.' });
    } finally {
      setHopDongLoading(false);
    }
  };

  // 5. Tải Khen thưởng & Kỷ luật
  const fetchKhenThuongKyLuat = async () => {
    setKtLoading(true);
    try {
      const [ktRes, klRes] = await Promise.all([
        api.get<KhenThuongDTO[]>('/khenthuong?loai=1'),
        api.get<KhenThuongDTO[]>('/khenthuong?loai=2'),
      ]);
      if (ktRes.data) setKhenThuongList(ktRes.data);
      if (klRes.data) setKyLuatList(klRes.data);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải dữ liệu khen thưởng/kỷ luật.' });
    } finally {
      setKtLoading(false);
    }
  };

  // 6. Tải Nâng lương & Điều chuyển
  const fetchNangLuongDieuChuyen = async () => {
    setNlDcLoading(true);
    try {
      const [nlRes, dcRes] = await Promise.all([
        api.get<NangLuongDTO[]>('/nangluong'),
        api.get<DieuChuyenDTO[]>('/dieuchuyen'),
      ]);
      if (nlRes.data) setNangLuongList(nlRes.data);
      if (dcRes.data) setDieuChuyenList(dcRes.data);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải dữ liệu nâng lương/điều chuyển.' });
    } finally {
      setNlDcLoading(false);
    }
  };

  // 7. Tải Tăng ca & Ứng lương
  const fetchTangCaUngLuong = async () => {
    setTcUlLoading(true);
    try {
      const [ulRes, tcRes] = await Promise.all([
        api.get<UngLuongDTO[]>('/ungluong'),
        api.get<TangCaDTO[]>('/tangca'),
      ]);
      if (ulRes.data) setUngLuongList(ulRes.data);
      if (tcRes.data) setTangCaList(tcRes.data);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải dữ liệu tăng ca/ứng lương.' });
    } finally {
      setTcUlLoading(false);
    }
  };

  // 8. Tải Danh sách Người dùng hệ thống
  const fetchUsers = async () => {
    setUserLoading(true);
    try {
      const res = await api.get<SysUserDTO[]>('/users');
      if (res.data) setUserList(res.data);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tải danh sách tài khoản.' });
    } finally {
      setUserLoading(false);
    }
  };

  // Khởi chạy khi đăng nhập thành công
  useEffect(() => {
    if (currentUser) {
      fetchInitialData();
    }
  }, [currentUser]);

  // Điều hướng khi chuyển Tab
  useEffect(() => {
    if (!currentUser) return;
    if (currentMenu === 'bangluong' && selectedKyCong) {
      fetchBangLuong(selectedKyCong);
    } else if (currentMenu === 'chamcong' && selectedKyCong) {
      fetchChamCong(selectedKyCong);
    } else if (currentMenu === 'hopdong') {
      fetchHopDong();
    } else if (currentMenu === 'khenthuong') {
      fetchKhenThuongKyLuat();
    } else if (currentMenu === 'nangluong') {
      fetchNangLuongDieuChuyen();
    } else if (currentMenu === 'ungluong') {
      fetchTangCaUngLuong();
    } else if (currentMenu === 'phanquyen') {
      fetchUsers();
    }
  }, [currentMenu, selectedKyCong, currentUser]);

  // Xử lý đăng nhập thành công
  const handleLoginSuccess = (user: CurrentUserDTO) => {
    setCurrentUser(user);
    setCurrentMenu('dashboard');
  };

  // Xử lý đăng xuất
  const handleLogout = async () => {
    try {
      await api.post('/auth/logout');
    } catch {
      // Bỏ qua lỗi khi logout
    }
    localStorage.removeItem('hrms_token');
    localStorage.removeItem('hrms_user');
    setCurrentUser(null);
    notification.info({ message: 'Đã đăng xuất', description: 'Phiên làm việc đã kết thúc an toàn.' });
  };

  // Tính lương kỳ công
  const handleTinhLuong = async () => {
    if (!selectedKyCong) return;
    setTinhLuongLoading(true);
    try {
      const res = await api.post<{ success: boolean; message: string }>('/bangluong/tinhluong', {
        Makycong: selectedKyCong,
        IdUser: currentUser?.IdUser || 1,
      });
      notification.success({
        message: 'Tính lương thành công',
        description: res.data.message || `Đã hoàn tất tính lương cho kỳ công #${selectedKyCong}`,
      });
      fetchBangLuong(selectedKyCong);
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({
        message: 'Lỗi tính lương',
        description: errorObj.response?.data?.Message || errorObj.message || 'Không thể tính lương.',
      });
    } finally {
      setTinhLuongLoading(false);
    }
  };

  // Modal Nhân viên
  const handleOpenNvModal = (nv?: NhanVienDTO) => {
    if (nv) {
      setEditingNv(nv);
      formNv.setFieldsValue({
        HOTEN: nv.HOTEN,
        GIOITINH: nv.GIOITINH === 'Nữ' ? 2 : 1,
        DIENTHOAI: nv.DIENTHOAI,
        CCCD: nv.CCCD,
        DIACHI: nv.DIACHI,
        IDPB: nv.IDPB,
        IDCV: nv.IDCV,
        IDTD: nv.IDTD,
      });
    } else {
      setEditingNv(null);
      formNv.resetFields();
    }
    setNvModalVisible(true);
  };

  const handleSaveNv = async () => {
    try {
      const values = await formNv.validateFields();
      setLoading(true);
      if (editingNv) {
        await api.put(`/nhanvien/${editingNv.MANV}`, values);
        notification.success({ message: 'Thành công', description: `Đã cập nhật nhân viên #${editingNv.MANV}` });
      } else {
        await api.post('/nhanvien', values);
        notification.success({ message: 'Thành công', description: 'Đã thêm nhân viên mới vào hệ thống!' });
      }
      setNvModalVisible(false);
      const nvRes = await api.get<NhanVienDTO[]>('/nhanvien');
      if (nvRes.data) setNhanVienList(nvRes.data);
    } catch (err: unknown) {
      const errorObj = err as { message?: string };
      notification.error({
        message: 'Lỗi lưu nhân viên',
        description: errorObj?.message || 'Vui lòng kiểm tra lại thông tin.',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteNv = async (manv: number) => {
    try {
      await api.delete(`/nhanvien/${manv}`);
      notification.success({ message: 'Thành công', description: `Đã cho thôi việc nhân viên #${manv}` });
      setNhanVienList((prev) => prev.filter((x) => x.MANV !== manv));
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa nhân viên.' });
    }
  };

  // Thêm Khen thưởng / Kỷ luật
  const handleSaveKt = async () => {
    try {
      const values = await formKt.validateFields();
      setKtLoading(true);
      await api.post('/khenthuong', {
        SoQd: values.SoQd,
        Loai: values.Loai,
        MaNv: values.MaNv,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        NoiDung: values.NoiDung,
        LyDo: values.LyDo,
      });
      notification.success({ message: 'Thành công', description: 'Đã lưu quyết định thành công.' });
      setKtModalVisible(false);
      formKt.resetFields();
      fetchKhenThuongKyLuat();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu quyết định.' });
    } finally {
      setKtLoading(false);
    }
  };

  const handleDeleteKt = async (soqd: string) => {
    try {
      await api.delete(`/khenthuong/${encodeURIComponent(soqd)}`);
      notification.success({ message: 'Thành công', description: 'Đã xóa quyết định.' });
      fetchKhenThuongKyLuat();
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể xóa quyết định.' });
    }
  };

  // Thêm Nâng lương
  const handleSaveNl = async () => {
    try {
      const values = await formNl.validateFields();
      setNlDcLoading(true);
      await api.post('/nangluong', {
        SoQd: values.SoQd,
        SoHd: values.SoHd,
        MaNv: values.MaNv,
        HeSoLuongCu: values.HeSoLuongCu,
        HeSoLuongMoi: values.HeSoLuongMoi,
        NgayKy: values.NgayKy ? dayjs(values.NgayKy).format('YYYY-MM-DD') : null,
        NgayLenLuong: values.NgayLenLuong ? dayjs(values.NgayLenLuong).format('YYYY-MM-DD') : null,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã tạo quyết định nâng lương.' });
      setNlModalVisible(false);
      formNl.resetFields();
      fetchNangLuongDieuChuyen();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu nâng lương.' });
    } finally {
      setNlDcLoading(false);
    }
  };

  // Thêm Điều chuyển
  const handleSaveDc = async () => {
    try {
      const values = await formDc.validateFields();
      setNlDcLoading(true);
      await api.post('/dieuchuyen', {
        SoQd: values.SoQd,
        MaNv: values.MaNv,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        IdPb: values.IdPb,
        IdPb2: values.IdPb2,
        LyDo: values.LyDo,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã tạo quyết định điều chuyển phòng ban.' });
      setDcModalVisible(false);
      formDc.resetFields();
      fetchNangLuongDieuChuyen();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu điều chuyển.' });
    } finally {
      setNlDcLoading(false);
    }
  };

  // Thêm Tạm ứng lương
  const handleSaveUl = async () => {
    try {
      const values = await formUl.validateFields();
      setTcUlLoading(true);
      await api.post('/ungluong', {
        Nam: values.Nam,
        Thang: values.Thang,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        SoTien: values.SoTien,
        MaNv: values.MaNv,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã thêm bản ghi tạm ứng lương.' });
      setUlModalVisible(false);
      formUl.resetFields();
      fetchTangCaUngLuong();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu tạm ứng.' });
    } finally {
      setTcUlLoading(false);
    }
  };

  // Thêm Tăng ca (OT)
  const handleSaveTc = async () => {
    try {
      const values = await formTc.validateFields();
      setTcUlLoading(true);
      await api.post('/tangca', {
        Nam: values.Nam,
        Thang: values.Thang,
        Ngay: values.Ngay ? dayjs(values.Ngay).format('YYYY-MM-DD') : null,
        SoGio: values.SoGio,
        MaNv: values.MaNv,
        IdLoaiCa: values.IdLoaiCa,
        GhiChu: values.GhiChu,
      });
      notification.success({ message: 'Thành công', description: 'Đã thêm bản ghi làm thêm giờ (OT).' });
      setTcModalVisible(false);
      formTc.resetFields();
      fetchTangCaUngLuong();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi lưu tăng ca.' });
    } finally {
      setTcUlLoading(false);
    }
  };

  // Thêm Người dùng hệ thống hoặc Nhóm quyền mới
  const handleCreateUser = async () => {
    try {
      const values = await formCreateUser.validateFields();
      setUserLoading(true);
      await api.post('/users', {
        Username: values.Username.trim(),
        FullName: values.FullName.trim(),
        Password: values.Password,
        IsGroup: Boolean(values.IsGroup),
      });
      notification.success({
        message: 'Thành công',
        description: values.IsGroup
          ? `Đã tạo nhóm quyền [${values.Username}]!`
          : `Đã tạo tài khoản [${values.Username}]!`,
      });
      setCreateUserModalVisible(false);
      formCreateUser.resetFields();
      fetchUsers();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi tạo tài khoản/nhóm.' });
    } finally {
      setUserLoading(false);
    }
  };

  // Khóa / Mở khóa tài khoản nhanh
  const handleToggleLock = async (user: SysUserDTO) => {
    try {
      await api.put(`/users/${user.IdUser}/toggle-lock`);
      notification.success({
        message: 'Thành công',
        description: user.Disabled ? `Đã mở khóa tài khoản [${user.Username}]!` : `Đã khóa tài khoản [${user.Username}]!`,
      });
      fetchUsers();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi cập nhật trạng thái.' });
    }
  };

  // Xóa tài khoản người dùng hoặc nhóm quyền
  const handleDeleteUser = async (user: SysUserDTO) => {
    try {
      await api.delete(`/users/${user.IdUser}`);
      notification.success({
        message: 'Thành công',
        description: `Đã xóa ${user.IsGroup ? 'nhóm' : 'tài khoản'} [${user.Username}]!`,
      });
      fetchUsers();
    } catch (err: unknown) {
      const errorObj = err as { response?: { data?: { Message?: string } }; message?: string };
      notification.error({ message: 'Lỗi', description: errorObj.response?.data?.Message || 'Lỗi khi xóa.' });
    }
  };

  // Làm mới quyền hạn của người dùng đang đăng nhập trực tiếp từ CSDL
  const handleRefreshMyRights = async () => {
    try {
      const res = await api.get<{ user: CurrentUserDTO }>('/auth/me');
      if (res.data && res.data.user) {
        const refreshed = res.data.user;
        setCurrentUser(refreshed);
        localStorage.setItem('hrms_user', JSON.stringify(refreshed));
        notification.success({
          message: 'Làm mới quyền hạn',
          description: `Đã đồng bộ ${refreshed.Rights?.length || 0} quyền chức năng mới nhất từ CSDL!`,
        });
      }
    } catch {
      notification.warning({ message: 'Thông báo', description: 'Không thể làm mới quyền hạn lúc này.' });
    }
  };

  // Gửi AI Chat
  const handleSendMessage = async (customPrompt?: string) => {
    const question = customPrompt || chatInput;
    if (!question.trim()) return;

    const userMsg: AIChatMessage = {
      id: Date.now().toString(),
      sender: 'user',
      content: question,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };

    setChatMessages((prev) => [...prev, userMsg]);
    if (!customPrompt) setChatInput('');
    setChatLoading(true);

    try {
      const res = await api.post<{ answer: string; sqlQuery?: string; source?: string }>('/ai/chat', {
        Question: question,
        Lang: 'vi',
      });

      const botReply: AIChatMessage = {
        id: (Date.now() + 1).toString(),
        sender: 'assistant',
        content: res.data?.answer || 'Không nhận được câu trả lời từ hệ thống.',
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        source: res.data?.source,
        sqlQuery: res.data?.sqlQuery,
      };

      setChatMessages((prev) => [...prev, botReply]);
    } catch {
      const errorReply: AIChatMessage = {
        id: (Date.now() + 1).toString(),
        sender: 'assistant',
        content: 'Rất tiếc hiện tại không thể kết nối tới dịch vụ AI. Vui lòng kiểm tra lại kết nối API.',
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      };
      setChatMessages((prev) => [...prev, errorReply]);
    } finally {
      setChatLoading(false);
    }
  };

  // NẾU CHƯA ĐĂNG NHẬP -> HIỂN THỊ MÀN HÌNH LOGIN
  if (!currentUser) {
    return <Login onLoginSuccess={handleLoginSuccess} />;
  }

  // Lọc nhân viên
  const filteredEmployees = nhanVienList.filter((nv) => {
    if (!searchKeyword) return true;
    const kw = searchKeyword.toLowerCase();
    return (
      (nv.HOTEN && nv.HOTEN.toLowerCase().includes(kw)) ||
      (nv.MANV && nv.MANV.toString().includes(kw)) ||
      (nv.TENPB && nv.TENPB.toLowerCase().includes(kw)) ||
      (nv.TENCV && nv.TENCV.toLowerCase().includes(kw)) ||
      (nv.DIENTHOAI && nv.DIENTHOAI.includes(kw))
    );
  });

  // Bảng nhân viên
  const employeeColumns: ColumnsType<NhanVienDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 85,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (text: string, record) => (
        <Space>
          <Avatar style={{ backgroundColor: record.GIOITINH === 'Nữ' ? '#eb2f96' : '#1677ff' }}>
            {text ? text.charAt(0) : 'U'}
          </Avatar>
          <Text strong>{text}</Text>
        </Space>
      ),
    },
    {
      title: 'Giới tính',
      dataIndex: 'GIOITINH',
      key: 'GIOITINH',
      width: 90,
    },
    {
      title: 'Phòng ban',
      dataIndex: 'TENPB',
      key: 'TENPB',
      render: (text: string) => text || 'Chưa phân bổ',
    },
    {
      title: 'Chức vụ',
      dataIndex: 'TENCV',
      key: 'TENCV',
      render: (text: string) => text || 'Nhân viên',
    },
    {
      title: 'Trình độ',
      dataIndex: 'TENTD',
      key: 'TENTD',
      render: (text: string) => text || 'Cơ bản',
    },
    {
      title: 'Điện thoại',
      dataIndex: 'DIENTHOAI',
      key: 'DIENTHOAI',
      render: (text: string) => text || 'Chưa có',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'TRANGTHAI',
      key: 'TRANGTHAI',
      width: 120,
      render: (status?: boolean) => (
        <Tag icon={<CheckCircleOutlined />} color={status !== false ? 'success' : 'default'}>
          {status !== false ? 'Đang làm' : 'Thôi việc'}
        </Tag>
      ),
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Chỉnh sửa">
            <Button
              type="text"
              size="small"
              icon={<EditOutlined style={{ color: '#1677ff' }} />}
              onClick={() => handleOpenNvModal(record)}
            />
          </Tooltip>
          <Popconfirm
            title="Xác nhận thôi việc nhân viên này?"
            onConfirm={() => handleDeleteNv(record.MANV)}
            okText="Đồng ý"
            cancelText="Hủy"
          >
            <Tooltip title="Thôi việc">
              <Button type="text" size="small" icon={<DeleteOutlined style={{ color: '#ff4d4f' }} />} />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // Bảng lương Columns (kèm nút xem phiếu lương)
  const bangLuongColumns: ColumnsType<BangLuongDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 80,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (name: string) => <Text strong>{name}</Text>,
    },
    {
      title: 'Công chuẩn',
      dataIndex: 'CONG_CHUAN',
      key: 'CONG_CHUAN',
      width: 100,
      align: 'right',
      render: (v: number) => v ?? 26,
    },
    {
      title: 'Công TT',
      dataIndex: 'CONG_THUCTE',
      key: 'CONG_THUCTE',
      width: 90,
      align: 'right',
      render: (v: number) => (
        <Tag color="cyan" style={{ fontWeight: 600 }}>
          {v ?? 0}
        </Tag>
      ),
    },
    {
      title: 'Lương thực tế',
      dataIndex: 'LUONG_CONG_THUCTE',
      key: 'LUONG_CONG_THUCTE',
      align: 'right',
      render: (v: number) => (v ? `${Number(v).toLocaleString('vi-VN')} đ` : '0 đ'),
    },
    {
      title: 'Phụ cấp',
      dataIndex: 'PHUCAP_CONG_THUCTE',
      key: 'PHUCAP_CONG_THUCTE',
      align: 'right',
      render: (v: number) => (v ? `${Number(v).toLocaleString('vi-VN')} đ` : '0 đ'),
    },
    {
      title: 'Tăng ca',
      dataIndex: 'TIEN_TANGCA',
      key: 'TIEN_TANGCA',
      align: 'right',
      render: (v: number) => (v ? `${Number(v).toLocaleString('vi-VN')} đ` : '0 đ'),
    },
    {
      title: 'Trừ BHXH',
      dataIndex: 'TIEN_BHXH_TRICH',
      key: 'TIEN_BHXH_TRICH',
      align: 'right',
      render: (v: number) => (v ? `-${Number(v).toLocaleString('vi-VN')} đ` : '0 đ'),
    },
    {
      title: 'Thực lĩnh',
      dataIndex: 'THUC_LINH',
      key: 'THUC_LINH',
      align: 'right',
      render: (v: number) => (
        <Text strong style={{ color: '#52c41a', fontSize: '14px' }}>
          {v ? `${Number(v).toLocaleString('vi-VN')} đ` : '0 đ'}
        </Text>
      ),
    },
    {
      title: 'Phiếu lương',
      key: 'phieuLuong',
      align: 'center',
      width: 110,
      render: (_, record) => (
        <Button
          size="small"
          type="primary"
          ghost
          icon={<PrinterOutlined />}
          onClick={() => {
            setSelectedBangLuong(record);
            setPhieuLuongModalVisible(true);
          }}
        >
          In phiếu
        </Button>
      ),
    },
  ];

  // Bảng Hợp đồng Columns
  const hopDongColumns: ColumnsType<HopDongDTO> = [
    {
      title: 'Số HĐ',
      dataIndex: 'SOHD',
      key: 'SOHD',
      render: (sohd: string) => <Tag color="purple">{sohd}</Tag>,
    },
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 80,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (name: string) => <Text strong>{name}</Text>,
    },
    {
      title: 'Thời hạn',
      dataIndex: 'THOIHAN',
      key: 'THOIHAN',
      render: (text: string) => text || '12 tháng',
    },
    {
      title: 'Bắt đầu',
      dataIndex: 'NGAYBATDAU',
      key: 'NGAYBATDAU',
    },
    {
      title: 'Kết thúc',
      dataIndex: 'NGAYKETTHUC',
      key: 'NGAYKETTHUC',
      render: (text: string) => text || 'Không xác định',
    },
    {
      title: 'Hệ số',
      dataIndex: 'HESOLUONG',
      key: 'HESOLUONG',
      align: 'right',
      render: (v: number) => v ?? 1.0,
    },
    {
      title: 'Lương thỏa thuận',
      dataIndex: 'LUONG_THOA_THUAN',
      key: 'LUONG_THOA_THUAN',
      align: 'right',
      render: (v: number) => (v ? `${Number(v).toLocaleString('vi-VN')} đ` : 'Theo hệ số'),
    },
  ];

  // Bảng Chấm công Columns
  const chamCongColumns: ColumnsType<KyCongChiTietDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      fixed: 'left',
      width: 80,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      fixed: 'left',
      width: 170,
      render: (name: string) => <Text strong>{name}</Text>,
    },
    {
      title: 'Tổng công',
      dataIndex: 'TONGNGAYCONG',
      key: 'TONGNGAYCONG',
      width: 100,
      fixed: 'left',
      render: (v: number) => <Tag color="geekblue" style={{ fontWeight: 'bold' }}>{v ?? 0} ngày</Tag>,
    },
    {
      title: 'Nghỉ phép',
      dataIndex: 'NGAYPHEP',
      key: 'NGAYPHEP',
      width: 90,
      render: (v: number) => <Tag color="gold">{v ?? 0} P</Tag>,
    },
    ...Array.from({ length: 31 }, (_, i) => {
      const dayKey = `D${i + 1}` as keyof KyCongChiTietDTO;
      return {
        title: `${i + 1}`,
        dataIndex: dayKey,
        key: dayKey,
        width: 45,
        align: 'center' as const,
        render: (val: string) => {
          if (!val) return <span style={{ color: '#d9d9d9' }}>-</span>;
          if (val === 'X') return <span style={{ color: '#1677ff', fontWeight: 600 }}>X</span>;
          if (val === 'CN') return <span style={{ color: '#ff4d4f', fontWeight: 600 }}>CN</span>;
          if (val === 'P') return <span style={{ color: '#faad14', fontWeight: 600 }}>P</span>;
          return <span>{val}</span>;
        },
      };
    }),
  ];

  // Bảng Người dùng hệ thống Columns
  const userColumns: ColumnsType<SysUserDTO> = [
    {
      title: 'ID',
      dataIndex: 'IdUser',
      key: 'IdUser',
      width: 70,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Tài khoản',
      dataIndex: 'Username',
      key: 'Username',
      width: 160,
      render: (u: string, record: SysUserDTO) => (
        <Space direction="vertical" size={2}>
          <Space>
            <UserOutlined style={{ color: '#1677ff' }} />
            <Text strong style={{ color: '#1677ff' }}>{u}</Text>
          </Space>
          {record.IsAdmin && (
            <Tag color="red" icon={<SafetyCertificateOutlined />} style={{ fontSize: 11 }}>
              Super Admin
            </Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Họ và tên',
      dataIndex: 'FullName',
      key: 'FullName',
      render: (fn: string) => fn || 'Chưa cập nhật',
    },
    {
      title: 'Nhóm quyền trực thuộc',
      dataIndex: 'Groups',
      key: 'Groups',
      render: (groups?: string[]) =>
        groups && groups.length > 0 ? (
          <Space wrap size={[4, 4]}>
            {groups.map((g, idx) => (
              <Tag color="purple" key={idx} icon={<TeamOutlined />}>
                {g}
              </Tag>
            ))}
          </Space>
        ) : (
          <Text type="secondary" italic>Chưa gán nhóm</Text>
        ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'Disabled',
      key: 'Disabled',
      width: 130,
      render: (disabled?: boolean) =>
        disabled ? (
          <Tag color="red">Bị tạm khóa</Tag>
        ) : (
          <Tag color="green">Đang hoạt động</Tag>
        ),
    },
    {
      title: 'Thao tác quản trị',
      key: 'actions',
      width: 280,
      render: (_, record) => (
        <Space size="small" wrap>
          <Button
            size="small"
            icon={<SettingOutlined />}
            style={{ borderColor: '#fa8c16', color: '#fa8c16' }}
            onClick={() => {
              setSelectedUserForPerms(record);
              setPhanQuyenModalVisible(true);
            }}
          >
            Phân quyền
          </Button>
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => {
              setSelectedUserForEdit(record);
              setUserEditModalVisible(true);
            }}
          >
            Sửa
          </Button>
          {!record.IsAdmin && (
            <Popconfirm
              title={record.Disabled ? 'Mở khóa tài khoản?' : 'Khóa tài khoản?'}
              description={`Bạn có chắc muốn ${record.Disabled ? 'mở khóa' : 'tạm khóa'} tài khoản [${record.Username}]?`}
              onConfirm={() => handleToggleLock(record)}
              okText="Đồng ý"
              cancelText="Hủy"
            >
              <Button size="small" danger={!record.Disabled}>
                {record.Disabled ? 'Mở khóa' : 'Khóa'}
              </Button>
            </Popconfirm>
          )}
          {!record.IsAdmin && (
            <Popconfirm
              title="Xóa tài khoản?"
              description={`Bạn có chắc chắn muốn xóa tài khoản [${record.Username}] không?`}
              onConfirm={() => handleDeleteUser(record)}
              okText="Xóa"
              cancelText="Hủy"
            >
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  // Bảng Nhóm quyền hệ thống Columns (Tương thích WinForms frmGroup)
  const groupColumns: ColumnsType<SysUserDTO> = [
    {
      title: 'ID',
      dataIndex: 'IdUser',
      key: 'IdUser',
      width: 70,
      render: (id: number) => <Tag color="purple">#{id}</Tag>,
    },
    {
      title: 'Mã nhóm quyền',
      dataIndex: 'Username',
      key: 'Username',
      width: 180,
      render: (name: string) => (
        <Tag color="purple" style={{ fontWeight: 600, fontSize: 13, padding: '2px 8px' }}>
          <TeamOutlined style={{ marginRight: 4 }} />
          {name}
        </Tag>
      ),
    },
    {
      title: 'Tên / Mô tả nhóm',
      dataIndex: 'FullName',
      key: 'FullName',
      render: (fn: string) => <Text strong>{fn}</Text>,
    },
    {
      title: 'Số lượng thành viên',
      dataIndex: 'MemberCount',
      key: 'MemberCount',
      width: 180,
      render: (count: number) => (
        <Space>
          <Badge count={count || 0} showZero style={{ backgroundColor: count > 0 ? '#52c41a' : '#d9d9d9' }} />
          <Text type="secondary">thành viên</Text>
        </Space>
      ),
    },
    {
      title: 'Thao tác quản trị nhóm',
      key: 'groupActions',
      width: 320,
      render: (_, record) => (
        <Space size="small" wrap>
          <Button
            size="small"
            icon={<SettingOutlined />}
            style={{ borderColor: '#fa8c16', color: '#fa8c16' }}
            onClick={() => {
              setSelectedUserForPerms(record);
              setPhanQuyenModalVisible(true);
            }}
          >
            Phân quyền nhóm
          </Button>
          <Button
            size="small"
            type="primary"
            ghost
            icon={<TeamOutlined />}
            onClick={() => {
              setSelectedGroupForMembers(record);
              setGroupMembersModalVisible(true);
            }}
          >
            Thành viên ({record.MemberCount || 0})
          </Button>
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => {
              setSelectedUserForEdit(record);
              setUserEditModalVisible(true);
            }}
          >
            Sửa
          </Button>
          <Popconfirm
            title="Xóa nhóm quyền?"
            description={`Bạn có chắc muốn xóa nhóm [${record.Username}] không? Toàn bộ liên kết thành viên và quyền hạn liên quan sẽ bị xóa.`}
            onConfirm={() => handleDeleteUser(record)}
            okText="Xóa"
            cancelText="Hủy"
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // Xây dựng danh sách Menu theo phân quyền WinForms
  const menuItems = [
    ...(hasRight('DASHBOARD', 'BAOCAO', 'F_DB_LUONG', 'F_DB_NHANSU', 'F_BC_BAOCAO')
      ? [{ key: 'dashboard', icon: <DashboardOutlined />, label: 'Bảng điều khiển' }]
      : []),
    ...(hasRight('NV', 'F_DM_NHANVIEN', 'F_NV_NHANVIEN', 'NHANVIEN')
      ? [{ key: 'nhanvien', icon: <TeamOutlined />, label: 'Quản lý Nhân sự' }]
      : []),
    ...(hasRight('CHAMCONG', 'F_CC_BANGCONG', 'F_CC_LOAICA', 'F_CC_KYCONG')
      ? [{ key: 'chamcong', icon: <CalendarOutlined />, label: 'Chấm công & Ca làm' }]
      : []),
    ...(hasRight('BANGLUONG', 'F_CC_BANGLUONG', 'LUONG')
      ? [{ key: 'bangluong', icon: <DollarOutlined />, label: 'Tính lương & Thuế' }]
      : []),
    ...(hasRight('HOPDONG', 'F_NV_HOPDONG')
      ? [{ key: 'hopdong', icon: <FileTextOutlined />, label: 'Hợp đồng lao động' }]
      : []),
    ...(hasRight('KHENTHUONG', 'KYLUAT', 'F_NV_KHENTHUONG', 'F_NV_KYLUAT')
      ? [{ key: 'khenthuong', icon: <TrophyOutlined />, label: 'Khen thưởng & Kỷ luật' }]
      : []),
    ...(hasRight('NANGLUONG', 'DIEUCHUYEN', 'F_NV_NANGLUONG', 'F_NV_DIEUCHUYEN')
      ? [{ key: 'nangluong', icon: <RiseOutlined />, label: 'Nâng lương & Chuyển phòng' }]
      : []),
    ...(hasRight('UNGLUONG', 'TANGCA', 'F_CC_UNGLUONG', 'F_CC_TANGCA')
      ? [{ key: 'ungluong', icon: <SwapOutlined />, label: 'Tăng ca & Ứng lương' }]
      : []),
    ...(currentUser.IsAdmin || hasRight('PHANQUYEN', 'F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'F_SYSTEM_LOCK_USER')
      ? [{ key: 'phanquyen', icon: <SafetyCertificateOutlined />, label: 'Người dùng & Phân quyền' }]
      : []),
    {
      key: 'aichat',
      icon: <RobotOutlined style={{ color: '#52c41a' }} />,
      label: (
        <span>
          AI Copilot <Tag color="purple" style={{ marginLeft: 4, fontSize: 10 }}>Oracle + Qwen</Tag>
        </span>
      ),
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {/* SIDEBAR NAVIGATION */}
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={(val) => setCollapsed(val)}
        width={250}
        theme="dark"
        style={{
          boxShadow: '2px 0 8px 0 rgba(29,35,41,.05)',
        }}
      >
        <div
          style={{
            height: 56,
            margin: '12px 16px',
            background: 'linear-gradient(135deg, #1677ff 0%, #0958d9 100%)',
            borderRadius: 8,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 'bold',
            fontSize: collapsed ? '14px' : '17px',
            letterSpacing: '0.5px',
          }}
        >
          {collapsed ? 'HR' : '⚡ HRMS PORTAL'}
        </div>

        <Menu
          theme="dark"
          selectedKeys={[currentMenu]}
          mode="inline"
          onClick={(e) => {
            if (e.key === 'aichat') {
              setAiDrawerVisible(true);
            } else {
              setCurrentMenu(e.key);
            }
          }}
          items={menuItems}
        />
      </Sider>

      <Layout>
        {/* TOP HEADER */}
        <Header
          style={{
            padding: '0 24px',
            background: colorBgContainer,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid #f0f0f0',
            position: 'sticky',
            top: 0,
            zIndex: 10,
          }}
        >
          <Title level={4} style={{ margin: 0, color: '#1f1f1f', fontWeight: 600 }}>
            {currentMenu === 'dashboard' && '📊 TỔNG QUAN HỆ THỐNG NHÂN SỰ'}
            {currentMenu === 'nhanvien' && '👥 QUẢN LÝ HỒ SƠ NHÂN VIÊN'}
            {currentMenu === 'chamcong' && '🕒 CHẤM CÔNG & QUẢN LÝ CA'}
            {currentMenu === 'bangluong' && '💰 BẢNG LƯƠNG & QUỸ LƯƠNG'}
            {currentMenu === 'hopdong' && '📜 HỢP ĐỒNG LAO ĐỘNG'}
            {currentMenu === 'khenthuong' && '🏆 QUẢN LÝ KHEN THƯỞNG & KỶ LUẬT'}
            {currentMenu === 'nangluong' && '📈 QUẢN LÝ NÂNG LƯƠNG & ĐIỀU CHUYỂN'}
            {currentMenu === 'ungluong' && '💵 QUẢN LÝ TĂNG CA & TẠM ỨNG LƯƠNG'}
            {currentMenu === 'phanquyen' && '🛡️ QUẢN TRỊ TÀI KHOẢN & PHÂN QUYỀN CHỨC NĂNG'}
          </Title>

          <Space size="middle">
            <Tooltip title={isBackendConnected ? 'Backend HRMS_API đã kết nối Oracle DB' : 'Chế độ ngoại tuyến'}>
              <Tag
                icon={<CloudSyncOutlined />}
                color={isBackendConnected ? 'success' : 'warning'}
                style={{ cursor: 'pointer', padding: '4px 10px', borderRadius: 12 }}
                onClick={fetchInitialData}
              >
                {isBackendConnected ? 'Oracle Live DB' : 'Chế độ Ngoại tuyến'}
              </Tag>
            </Tooltip>

            <Tooltip title="Làm mới dữ liệu">
              <Button shape="circle" icon={<ReloadOutlined spin={loading} />} onClick={fetchInitialData} />
            </Tooltip>

            <Button
              type="primary"
              icon={<RobotOutlined />}
              onClick={() => setAiDrawerVisible(true)}
              style={{
                background: 'linear-gradient(135deg, #722ed1 0%, #1677ff 100%)',
                border: 'none',
              }}
            >
              Hỏi AI Copilot
            </Button>

            <Tooltip title="Kỳ công hiện hành">
              <Badge count={kyCongList.length} size="small">
                <Button shape="circle" icon={<BellOutlined />} />
              </Badge>
            </Tooltip>

            {/* Dropdown User & Đăng xuất */}
            <Dropdown
              menu={{
                items: [
                  {
                    key: 'profile',
                    icon: <UserOutlined />,
                    label: `Tài khoản: ${currentUser.Username}`,
                  },
                  {
                    key: 'role',
                    icon: <SafetyCertificateOutlined />,
                    label: currentUser.IsAdmin ? 'Quyền: Super Admin' : `Quyền: ${currentUser.Rights?.length || 0} chức năng`,
                  },
                  {
                    type: 'divider',
                  },
                  {
                    key: 'changePassword',
                    icon: <KeyOutlined style={{ color: '#1677ff' }} />,
                    label: 'Đổi mật khẩu cá nhân',
                    onClick: () => setChangePasswordModalVisible(true),
                  },
                  {
                    key: 'refreshRights',
                    icon: <ReloadOutlined style={{ color: '#52c41a' }} />,
                    label: 'Làm mới quyền hạn CSDL',
                    onClick: handleRefreshMyRights,
                  },
                  {
                    type: 'divider',
                  },
                  {
                    key: 'logout',
                    icon: <LogoutOutlined style={{ color: '#ff4d4f' }} />,
                    label: <span style={{ color: '#ff4d4f' }}>Đăng xuất</span>,
                    onClick: handleLogout,
                  },
                ],
              }}
            >
              <Space style={{ marginLeft: 8, cursor: 'pointer' }}>
                <Avatar
                  style={{
                    backgroundColor: currentUser.IsAdmin ? '#ff4d4f' : '#1677ff',
                  }}
                  icon={<UserOutlined />}
                />
                <div style={{ display: 'flex', flexDirection: 'column', lineHeight: 1.2 }}>
                  <Text strong style={{ fontSize: 13 }}>{currentUser.FullName}</Text>
                  <Tag color={currentUser.IsAdmin ? 'red' : 'blue'} style={{ fontSize: 10, width: 'fit-content', padding: '0 4px', margin: 0 }}>
                    {currentUser.IsAdmin ? 'SUPER ADMIN' : 'NHÂN VIÊN'}
                  </Tag>
                </div>
              </Space>
            </Dropdown>
          </Space>
        </Header>

        {/* MAIN CONTENT AREA */}
        <Content style={{ margin: '20px 24px', minHeight: 400 }}>
          {/* TAB 1: DASHBOARD */}
          {currentMenu === 'dashboard' && (
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
              <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Tổng số Nhân viên</Text>}
                      value={totalEmployees}
                      prefix={<TeamOutlined style={{ color: '#1677ff', marginRight: 8 }} />}
                      suffix={<Tag color="blue" style={{ marginLeft: 8 }}>{isBackendConnected ? 'Trực tiếp' : 'Dữ liệu'}</Tag>}
                      valueStyle={{ color: '#1677ff', fontWeight: 700 }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Hợp đồng Lao động</Text>}
                      value={hopDongList.length || 976}
                      suffix={<Tag color="green" style={{ marginLeft: 8 }}>Có hiệu lực</Tag>}
                      prefix={<FileTextOutlined style={{ color: '#52c41a', marginRight: 8 }} />}
                      valueStyle={{ color: '#52c41a', fontWeight: 700 }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Tổng Quỹ Lương Kỳ Mới</Text>}
                      value={totalSalary}
                      prefix={<DollarOutlined style={{ color: '#fa8c16', marginRight: 8 }} />}
                      formatter={(v) => `${Number(v).toLocaleString('vi-VN')} đ`}
                      valueStyle={{ color: '#fa8c16', fontWeight: 700, fontSize: '1.25rem' }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Phân Quyền WinForm</Text>}
                      value={currentUser.IsAdmin ? 'Full Access' : `${currentUser.Rights?.length} Module`}
                      prefix={<SafetyCertificateOutlined style={{ color: '#722ed1', marginRight: 8 }} />}
                      suffix={<Tag color="purple">Role-Based</Tag>}
                      valueStyle={{ color: '#722ed1', fontWeight: 700 }}
                    />
                  </Card>
                </Col>
              </Row>

              {/* CHARTS ROW */}
              <Row gutter={[16, 16]}>
                <Col xs={24} lg={15}>
                  <Card
                    title="📊 Biểu đồ Biến động Quỹ Lương"
                    bordered={false}
                    style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}
                  >
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={luongStats} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                        <XAxis dataKey="KyCong" />
                        <YAxis tickFormatter={(val) => `${(val / 1000000000).toFixed(1)}B`} />
                        <RechartsTooltip formatter={(val: unknown) => [`${Number(val ?? 0).toLocaleString('vi-VN')} đ`, 'Tổng quỹ lương']} />
                        <Legend />
                        <Bar dataKey="TongLuong" name="Quỹ Lương (VNĐ)" fill="#1677ff" radius={[4, 4, 0, 0]} />
                      </BarChart>
                    </ResponsiveContainer>
                  </Card>
                </Col>

                <Col xs={24} lg={9}>
                  <Card
                    title="🏢 Phân bố Nhân sự theo Phòng ban"
                    bordered={false}
                    style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}
                  >
                    <ResponsiveContainer width="100%" height={300}>
                      <PieChart>
                        <Pie
                          data={phongBanStats}
                          dataKey="SoLuong"
                          nameKey="PhongBan"
                          cx="50%"
                          cy="50%"
                          outerRadius={90}
                          label={(entry: { name?: string; value?: number }) => `${entry.name || ''}: ${entry.value || 0}`}
                        >
                          {phongBanStats.map((_, index) => (
                            <Cell key={`cell-${index}`} fill={PIE_COLORS[index % PIE_COLORS.length]} />
                          ))}
                        </Pie>
                        <RechartsTooltip />
                      </PieChart>
                    </ResponsiveContainer>
                  </Card>
                </Col>
              </Row>

              {/* RECENT EMPLOYEES TABLE */}
              <Card
                title="👥 Danh sách Nhân sự Gần đây"
                extra={
                  <Button type="link" onClick={() => setCurrentMenu('nhanvien')}>
                    Xem tất cả ({nhanVienList.length}) &gt;
                  </Button>
                }
                bordered={false}
                style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}
              >
                <Table
                  columns={employeeColumns}
                  dataSource={filteredEmployees.slice(0, 5)}
                  rowKey="MANV"
                  pagination={false}
                  loading={loading}
                  size="middle"
                />
              </Card>
            </Space>
          )}

          {/* TAB 2: QUẢN LÝ NHÂN SỰ */}
          {currentMenu === 'nhanvien' && (
            <Card
              title={`Danh sách Hồ sơ Nhân viên (${filteredEmployees.length})`}
              extra={
                <Space>
                  <Input
                    placeholder="Tìm theo tên, mã NV, phòng ban..."
                    prefix={<SearchOutlined />}
                    value={searchKeyword}
                    onChange={(e) => setSearchKeyword(e.target.value)}
                    allowClear
                    style={{ width: 280 }}
                  />
                  <Button type="primary" icon={<PlusOutlined />} onClick={() => handleOpenNvModal()}>
                    Thêm nhân viên mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Table
                columns={employeeColumns}
                dataSource={filteredEmployees}
                rowKey="MANV"
                loading={loading}
                pagination={{ pageSize: 8, showTotal: (total) => `Tổng số ${total} nhân viên` }}
              />
            </Card>
          )}

          {/* TAB 3: CHẤM CÔNG & CA LÀM */}
          {currentMenu === 'chamcong' && (
            <Card
              title="🕒 Quản lý Chấm công & Ca làm"
              extra={
                <Space>
                  <Text strong>Chọn Kỳ công:</Text>
                  <Select
                    value={selectedKyCong}
                    onChange={(val) => setSelectedKyCong(val)}
                    style={{ width: 150 }}
                    options={kyCongList.map((kc) => ({
                      value: kc.MAKYCONG,
                      label: `Kỳ ${kc.THANG}/${kc.NAM}`,
                    }))}
                  />
                  <Button icon={<ReloadOutlined />} onClick={() => fetchChamCong(selectedKyCong)}>
                    Làm mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Tabs
                defaultActiveKey="bangcong"
                items={[
                  {
                    key: 'bangcong',
                    label: `Bảng chấm công chi tiết (${chamCongList.length} nhân viên)`,
                    children: (
                      <Table
                        columns={chamCongColumns}
                        dataSource={chamCongList}
                        rowKey="MANV"
                        loading={chamCongLoading}
                        scroll={{ x: 1800 }}
                        pagination={{ pageSize: 10, showTotal: (total) => `Tổng số ${total} bản ghi` }}
                        size="small"
                      />
                    ),
                  },
                  {
                    key: 'loaica',
                    label: 'Danh mục Loại ca & Ca làm',
                    children: (
                      <Row gutter={[16, 16]}>
                        <Col span={12}>
                          <Card title="Danh sách Loại ca làm việc" size="small">
                            <Table
                              columns={[
                                { title: 'Mã', dataIndex: 'IDLOAICA', key: 'IDLOAICA', width: 70 },
                                { title: 'Tên ca làm', dataIndex: 'TENLOAICA', key: 'TENLOAICA' },
                                {
                                  title: 'Hệ số ca',
                                  dataIndex: 'HESOLOAICA',
                                  key: 'HESOLOAICA',
                                  render: (h: number) => <Tag color="blue">{h ?? 1.0}x</Tag>,
                                },
                              ]}
                              dataSource={loaiCaList}
                              rowKey="IDLOAICA"
                              pagination={false}
                            />
                          </Card>
                        </Col>
                        <Col span={12}>
                          <Card title="Ký hiệu quy ước chấm công" size="small">
                            <ul style={{ lineHeight: 2, paddingLeft: 20 }}>
                              <li><Tag color="blue">X</Tag>: Đi làm cả ngày (1.0 công)</li>
                              <li><Tag color="red">CN</Tag>: Ngày nghỉ Chủ nhật hàng tuần</li>
                              <li><Tag color="gold">P</Tag>: Nghỉ phép năm có hưởng lương</li>
                              <li><Tag color="green">L</Tag>: Nghỉ lễ tết có hưởng lương</li>
                              <li><Tag color="default">KP</Tag>: Nghỉ không phép</li>
                            </ul>
                          </Card>
                        </Col>
                      </Row>
                    ),
                  },
                ]}
              />
            </Card>
          )}

          {/* TAB 4: BẢNG LƯƠNG & QUỸ LƯƠNG */}
          {currentMenu === 'bangluong' && (
            <Card
              title="💰 Quản lý Tính lương & Bảng lương"
              extra={
                <Space>
                  <Text strong>Kỳ công:</Text>
                  <Select
                    value={selectedKyCong}
                    onChange={(val) => setSelectedKyCong(val)}
                    style={{ width: 140 }}
                    options={kyCongList.map((kc) => ({
                      value: kc.MAKYCONG,
                      label: `Kỳ ${kc.THANG}/${kc.NAM}`,
                    }))}
                  />
                  <Button
                    type="primary"
                    icon={<CalculatorOutlined />}
                    loading={tinhLuongLoading}
                    onClick={handleTinhLuong}
                    style={{ background: '#52c41a', borderColor: '#52c41a' }}
                  >
                    Tính lương kỳ này
                  </Button>
                  <Button icon={<ReloadOutlined />} onClick={() => fetchBangLuong(selectedKyCong)}>
                    Làm mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Row gutter={[16, 16]}>
                <Col span={8}>
                  <Card size="small" style={{ background: '#f6ffed', borderColor: '#b7eb8f' }}>
                    <Statistic
                      title="Tổng thực lĩnh kỳ"
                      value={bangLuongList.reduce((acc, cur) => acc + (cur.THUC_LINH || 0), 0)}
                      formatter={(v) => `${Number(v).toLocaleString('vi-VN')} đ`}
                      valueStyle={{ color: '#52c41a', fontWeight: 700 }}
                    />
                  </Card>
                </Col>
                <Col span={8}>
                  <Card size="small" style={{ background: '#e6f4ff', borderColor: '#91caff' }}>
                    <Statistic
                      title="Số lượng nhân viên"
                      value={bangLuongList.length}
                      suffix="người"
                      valueStyle={{ color: '#1677ff', fontWeight: 700 }}
                    />
                  </Card>
                </Col>
                <Col span={8}>
                  <Card size="small" style={{ background: '#fff7e6', borderColor: '#ffd591' }}>
                    <Statistic
                      title="Lương bình quân"
                      value={
                        bangLuongList.length > 0
                          ? Math.round(
                              bangLuongList.reduce((acc, cur) => acc + (cur.THUC_LINH || 0), 0) / bangLuongList.length
                            )
                          : 0
                      }
                      formatter={(v) => `${Number(v).toLocaleString('vi-VN')} đ`}
                      valueStyle={{ color: '#fa8c16', fontWeight: 700 }}
                    />
                  </Card>
                </Col>
              </Row>

              <Table
                columns={bangLuongColumns}
                dataSource={bangLuongList}
                rowKey="IDBL"
                loading={bangLuongLoading}
                pagination={{ pageSize: 10, showTotal: (total) => `Tổng số ${total} nhân viên được tính lương` }}
                size="middle"
                style={{ marginTop: 16 }}
              />
            </Card>
          )}

          {/* TAB 5: HỢP ĐỒNG LAO ĐỘNG */}
          {currentMenu === 'hopdong' && (
            <Card
              title={`Danh sách Hợp đồng Lao động (${hopDongList.length})`}
              extra={
                <Space>
                  <Button icon={<ReloadOutlined />} onClick={fetchHopDong}>
                    Làm mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Table
                columns={hopDongColumns}
                dataSource={hopDongList}
                rowKey="SOHD"
                loading={hopDongLoading}
                pagination={{ pageSize: 8, showTotal: (total) => `Tổng cộng ${total} hợp đồng lao động` }}
              />
            </Card>
          )}

          {/* TAB 6: KHEN THƯỞNG & KỶ LUẬT */}
          {currentMenu === 'khenthuong' && (
            <Card
              title="🏆 Quản lý Khen thưởng & Kỷ luật"
              extra={
                <Space>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={() => {
                      formKt.resetFields();
                      setKtModalVisible(true);
                    }}
                  >
                    Tạo Quyết định mới
                  </Button>
                  <Button icon={<ReloadOutlined />} onClick={fetchKhenThuongKyLuat}>
                    Làm mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Tabs
                defaultActiveKey="khenthuong"
                items={[
                  {
                    key: 'khenthuong',
                    label: `Quyết định Khen thưởng (${khenThuongList.length})`,
                    children: (
                      <Table
                        columns={[
                          { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="green">{t}</Tag> },
                          { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                          { title: 'Ngày ban hành', dataIndex: 'NGAY', key: 'NGAY' },
                          { title: 'Nội dung khen thưởng', dataIndex: 'NOIDUNG', key: 'NOIDUNG' },
                          { title: 'Lý do', dataIndex: 'LYDO', key: 'LYDO' },
                          {
                            title: 'Thao tác',
                            key: 'action',
                            width: 90,
                            render: (_, r) => (
                              <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDeleteKt(r.SOQD)}>
                                <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                              </Popconfirm>
                            ),
                          },
                        ]}
                        dataSource={khenThuongList}
                        rowKey="SOQD"
                        loading={ktLoading}
                        pagination={{ pageSize: 8 }}
                      />
                    ),
                  },
                  {
                    key: 'kyluat',
                    label: `Quyết định Kỷ luật (${kyLuatList.length})`,
                    children: (
                      <Table
                        columns={[
                          { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="red">{t}</Tag> },
                          { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                          { title: 'Ngày ban hành', dataIndex: 'NGAY', key: 'NGAY' },
                          { title: 'Nội dung kỷ luật', dataIndex: 'NOIDUNG', key: 'NOIDUNG' },
                          { title: 'Lý do vi phạm', dataIndex: 'LYDO', key: 'LYDO' },
                          {
                            title: 'Thao tác',
                            key: 'action',
                            width: 90,
                            render: (_, r) => (
                              <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDeleteKt(r.SOQD)}>
                                <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                              </Popconfirm>
                            ),
                          },
                        ]}
                        dataSource={kyLuatList}
                        rowKey="SOQD"
                        loading={ktLoading}
                        pagination={{ pageSize: 8 }}
                      />
                    ),
                  },
                ]}
              />
            </Card>
          )}

          {/* TAB 7: NÂNG LƯƠNG & ĐIỀU CHUYỂN */}
          {currentMenu === 'nangluong' && (
            <Card
              title="📈 Quản lý Nâng lương & Điều chuyển"
              extra={
                <Space>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={() => {
                      formNl.resetFields();
                      setNlModalVisible(true);
                    }}
                  >
                    Tạo QĐ Nâng lương
                  </Button>
                  <Button
                    icon={<SwapOutlined />}
                    onClick={() => {
                      formDc.resetFields();
                      setDcModalVisible(true);
                    }}
                  >
                    Tạo QĐ Điều chuyển
                  </Button>
                  <Button icon={<ReloadOutlined />} onClick={fetchNangLuongDieuChuyen}>
                    Làm mới
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
                    label: `Quyết định Nâng lương (${nangLuongList.length})`,
                    children: (
                      <Table
                        columns={[
                          { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="blue">{t}</Tag> },
                          { title: 'Số HĐ', dataIndex: 'SOHD', key: 'SOHD', render: (t) => <Tag color="purple">{t}</Tag> },
                          { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                          { title: 'Hệ số cũ', dataIndex: 'HESOLUONG_CU', key: 'HESOLUONG_CU', align: 'right' },
                          {
                            title: 'Hệ số mới',
                            dataIndex: 'HESOLUONG_MOI',
                            key: 'HESOLUONG_MOI',
                            align: 'right',
                            render: (v) => <Tag color="green" style={{ fontWeight: 600 }}>{v}</Tag>,
                          },
                          { title: 'Ngày ký', dataIndex: 'NGAYKY', key: 'NGAYKY' },
                          { title: 'Ngày hưởng', dataIndex: 'NGAYLENLUONG', key: 'NGAYLENLUONG' },
                          { title: 'Ghi chú', dataIndex: 'GHICHU', key: 'GHICHU' },
                        ]}
                        dataSource={nangLuongList}
                        rowKey="SOQD"
                        loading={nlDcLoading}
                        pagination={{ pageSize: 8 }}
                      />
                    ),
                  },
                  {
                    key: 'dieuchuyen',
                    label: `Điều chuyển Phòng ban (${dieuChuyenList.length})`,
                    children: (
                      <Table
                        columns={[
                          { title: 'Số QĐ', dataIndex: 'SOQD', key: 'SOQD', render: (t) => <Tag color="geekblue">{t}</Tag> },
                          { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                          { title: 'Ngày chuyển', dataIndex: 'NGAY', key: 'NGAY' },
                          { title: 'Phòng ban cũ', dataIndex: 'TENPB', key: 'TENPB', render: (t) => <Tag color="default">{t}</Tag> },
                          { title: 'Phòng ban mới', dataIndex: 'TENPB2', key: 'TENPB2', render: (t) => <Tag color="cyan">{t}</Tag> },
                          { title: 'Lý do chuyển', dataIndex: 'LYDO', key: 'LYDO' },
                          { title: 'Ghi chú', dataIndex: 'GHICHU', key: 'GHICHU' },
                        ]}
                        dataSource={dieuChuyenList}
                        rowKey="SOQD"
                        loading={nlDcLoading}
                        pagination={{ pageSize: 8 }}
                      />
                    ),
                  },
                ]}
              />
            </Card>
          )}

          {/* TAB 8: TĂNG CA & ỨNG LƯƠNG */}
          {currentMenu === 'ungluong' && (
            <Card
              title="💵 Quản lý Tăng ca & Tạm ứng Lương"
              extra={
                <Space>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={() => {
                      formTc.resetFields();
                      setTcModalVisible(true);
                    }}
                  >
                    Báo cáo Tăng ca (OT)
                  </Button>
                  <Button
                    icon={<DollarOutlined />}
                    onClick={() => {
                      formUl.resetFields();
                      setUlModalVisible(true);
                    }}
                  >
                    Đăng ký Tạm ứng
                  </Button>
                  <Button icon={<ReloadOutlined />} onClick={fetchTangCaUngLuong}>
                    Làm mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Tabs
                defaultActiveKey="tangca"
                items={[
                  {
                    key: 'tangca',
                    label: `Làm thêm giờ / Tăng ca (${tangCaList.length})`,
                    children: (
                      <Table
                        columns={[
                          { title: 'ID', dataIndex: 'ID', key: 'ID', width: 70, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                          { title: 'Tháng/Năm', key: 'thangnam', render: (_, r) => `${r.THANG}/${r.NAM}` },
                          { title: 'Ngày OT', dataIndex: 'NGAY', key: 'NGAY' },
                          { title: 'Loại ca', dataIndex: 'TENLOAICA', key: 'TENLOAICA', render: (t) => <Tag color="purple">{t}</Tag> },
                          { title: 'Số giờ', dataIndex: 'SOGIO', key: 'SOGIO', align: 'right', render: (v) => <Tag color="orange">{v} giờ</Tag> },
                          {
                            title: 'Tiền tăng ca',
                            dataIndex: 'SOTIEN',
                            key: 'SOTIEN',
                            align: 'right',
                            render: (v) => <Text strong style={{ color: '#52c41a' }}>{(v || 0).toLocaleString('vi-VN')} đ</Text>,
                          },
                          { title: 'Ghi chú', dataIndex: 'GHICHU', key: 'GHICHU' },
                        ]}
                        dataSource={tangCaList}
                        rowKey="ID"
                        loading={tcUlLoading}
                        pagination={{ pageSize: 8 }}
                      />
                    ),
                  },
                  {
                    key: 'ungluong',
                    label: `Danh sách Tạm ứng Lương (${ungLuongList.length})`,
                    children: (
                      <Table
                        columns={[
                          { title: 'ID', dataIndex: 'ID', key: 'ID', width: 70, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Mã NV', dataIndex: 'MANV', key: 'MANV', width: 80, render: (t) => <Tag color="blue">#{t}</Tag> },
                          { title: 'Họ tên', dataIndex: 'HOTEN', key: 'HOTEN', render: (t) => <Text strong>{t}</Text> },
                          { title: 'Tháng/Năm', key: 'thangnam', render: (_, r) => `${r.THANG}/${r.NAM}` },
                          { title: 'Ngày ứng', dataIndex: 'NGAY', key: 'NGAY' },
                          {
                            title: 'Số tiền ứng',
                            dataIndex: 'SOTIEN',
                            key: 'SOTIEN',
                            align: 'right',
                            render: (v) => <Text strong style={{ color: '#ff4d4f' }}>-{(v || 0).toLocaleString('vi-VN')} đ</Text>,
                          },
                          {
                            title: 'Trạng thái',
                            dataIndex: 'TRANGTHAI',
                            key: 'TRANGTHAI',
                            render: (st) => (
                              <Tag color={st === 1 ? 'success' : 'warning'}>
                                {st === 1 ? 'Đã duyệt chi' : 'Chờ duyệt'}
                              </Tag>
                            ),
                          },
                          { title: 'Lý do ứng', dataIndex: 'GHICHU', key: 'GHICHU' },
                        ]}
                        dataSource={ungLuongList}
                        rowKey="ID"
                        loading={tcUlLoading}
                        pagination={{ pageSize: 8 }}
                      />
                    ),
                  },
                ]}
              />
            </Card>
          )}

          {/* TAB 9: QUẢN TRỊ TÀI KHOẢN & PHÂN QUYỀN (CHUẨN WINFORMS RBAC) */}
          {currentMenu === 'phanquyen' && (
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
              {/* METRICS CARDS */}
              <Row gutter={16}>
                <Col span={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
                    <Statistic
                      title="Tổng số người dùng"
                      value={userList.filter((u) => !u.IsGroup).length}
                      prefix={<UserOutlined style={{ color: '#1677ff' }} />}
                    />
                  </Card>
                </Col>
                <Col span={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
                    <Statistic
                      title="Đang hoạt động"
                      value={userList.filter((u) => !u.IsGroup && !u.Disabled).length}
                      valueStyle={{ color: '#52c41a' }}
                      prefix={<CheckCircleOutlined />}
                    />
                  </Card>
                </Col>
                <Col span={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
                    <Statistic
                      title="Tài khoản tạm khóa"
                      value={userList.filter((u) => !u.IsGroup && u.Disabled).length}
                      valueStyle={{ color: '#ff4d4f' }}
                      prefix={<LockOutlined />}
                    />
                  </Card>
                </Col>
                <Col span={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG }}>
                    <Statistic
                      title="Nhóm quyền (Roles)"
                      value={userList.filter((u) => u.IsGroup).length}
                      valueStyle={{ color: '#722ed1' }}
                      prefix={<TeamOutlined />}
                    />
                  </Card>
                </Col>
              </Row>

              {/* MAIN CONTENT CARD */}
              <Card
                title={
                  <Space size="large">
                    <span style={{ fontWeight: 600, fontSize: 16 }}>🛡️ Quản trị Tài khoản & Phân quyền Hệ thống</span>
                    <Segmented
                      value={userTab}
                      onChange={(val: any) => setUserTab(val as 'users' | 'groups')}
                      options={[
                        {
                          label: `👥 Người dùng (${userList.filter((u) => !u.IsGroup).length})`,
                          value: 'users',
                        },
                        {
                          label: `🏷️ Nhóm quyền (${userList.filter((u) => u.IsGroup).length})`,
                          value: 'groups',
                        },
                      ]}
                    />
                  </Space>
                }
                extra={
                  <Space>
                    <Input
                      placeholder={userTab === 'users' ? 'Tìm tài khoản, họ tên...' : 'Tìm mã hoặc tên nhóm...'}
                      prefix={<SearchOutlined />}
                      value={userSearchText}
                      onChange={(e) => setUserSearchText(e.target.value)}
                      allowClear
                      style={{ width: 240 }}
                    />
                    <Button
                      type="primary"
                      icon={<PlusOutlined />}
                      onClick={() => {
                        formCreateUser.resetFields();
                        setIsCreatingGroup(userTab === 'groups');
                        formCreateUser.setFieldsValue({ IsGroup: userTab === 'groups' });
                        setCreateUserModalVisible(true);
                      }}
                    >
                      {userTab === 'users' ? 'Thêm tài khoản mới' : 'Thêm nhóm quyền mới'}
                    </Button>
                    <Button icon={<ReloadOutlined spin={userLoading} />} onClick={fetchUsers}>
                      Làm mới
                    </Button>
                  </Space>
                }
                bordered={false}
                style={{ borderRadius: borderRadiusLG }}
              >
                {userTab === 'users' ? (
                  <Table
                    columns={userColumns}
                    dataSource={userList.filter(
                      (u) =>
                        !u.IsGroup &&
                        (u.Username.toLowerCase().includes(userSearchText.toLowerCase()) ||
                          u.FullName.toLowerCase().includes(userSearchText.toLowerCase()))
                    )}
                    rowKey="IdUser"
                    loading={userLoading}
                    pagination={{ pageSize: 8, showTotal: (t) => `Tổng số ${t} người dùng` }}
                  />
                ) : (
                  <Table
                    columns={groupColumns}
                    dataSource={userList.filter(
                      (u) =>
                        Boolean(u.IsGroup) &&
                        (u.Username.toLowerCase().includes(userSearchText.toLowerCase()) ||
                          u.FullName.toLowerCase().includes(userSearchText.toLowerCase()))
                    )}
                    rowKey="IdUser"
                    loading={userLoading}
                    pagination={{ pageSize: 8, showTotal: (t) => `Tổng số ${t} nhóm quyền` }}
                  />
                )}
              </Card>
            </Space>
          )}
        </Content>
      </Layout>

      {/* MODAL THÊM / SỬA NHÂN VIÊN */}
      <Modal
        title={editingNv ? `Chỉnh sửa Nhân viên #${editingNv.MANV}` : 'Thêm Nhân viên Mới'}
        open={nvModalVisible}
        onOk={handleSaveNv}
        onCancel={() => setNvModalVisible(false)}
        okText="Lưu thông tin"
        cancelText="Hủy"
        width={700}
      >
        <Form form={formNv} layout="vertical" style={{ marginTop: 16 }}>
          <Row gutter={16}>
            <Col span={16}>
              <Form.Item
                name="HOTEN"
                label="Họ và tên nhân viên"
                rules={[{ required: true, message: 'Vui lòng nhập họ và tên' }]}
              >
                <Input placeholder="Ví dụ: Nguyễn Văn Hoàng" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="GIOITINH" label="Giới tính" initialValue={1}>
                <Select
                  options={[
                    { value: 1, label: 'Nam' },
                    { value: 2, label: 'Nữ' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="DIENTHOAI" label="Số điện thoại">
                <Input placeholder="0988123456" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="CCCD" label="Số CCCD / CMND">
                <Input placeholder="001201012345" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="IDPB" label="Phòng ban" rules={[{ required: true, message: 'Vui lòng chọn phòng ban' }]}>
                <Select
                  placeholder="Chọn phòng ban"
                  options={danhMuc?.phongBan?.map((pb) => ({
                    value: pb.IDPB,
                    label: pb.TENPB,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="IDCV" label="Chức vụ" rules={[{ required: true, message: 'Vui lòng chọn chức vụ' }]}>
                <Select
                  placeholder="Chọn chức vụ"
                  options={danhMuc?.chucVu?.map((cv) => ({
                    value: cv.IDCV,
                    label: cv.TENCV,
                  }))}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="IDTD" label="Trình độ học vấn">
                <Select
                  placeholder="Chọn trình độ"
                  options={danhMuc?.trinhDo?.map((td) => ({
                    value: td.ID,
                    label: td.TEN,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="DIACHI" label="Địa chỉ liên hệ">
                <Input placeholder="Hà Nội, Bắc Giang..." />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>

      {/* MODAL TẠO QUYẾT ĐỊNH KHEN THƯỞNG / KỶ LUẬT */}
      <Modal
        title="Tạo Quyết định Khen thưởng / Kỷ luật"
        open={ktModalVisible}
        onOk={handleSaveKt}
        onCancel={() => setKtModalVisible(false)}
        okText="Lưu Quyết định"
        cancelText="Hủy"
        width={600}
      >
        <Form form={formKt} layout="vertical" style={{ marginTop: 16 }} initialValues={{ Loai: 1 }}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="SoQd"
                label="Số Quyết định"
                rules={[{ required: true, message: 'Vui lòng nhập số QĐ' }]}
              >
                <Input placeholder="Ví dụ: 10/2023/QĐKT" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Loai" label="Hình thức">
                <Select
                  options={[
                    { value: 1, label: 'Khen thưởng' },
                    { value: 2, label: 'Kỷ luật' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="MaNv"
                label="Nhân viên"
                rules={[{ required: true, message: 'Vui lòng chọn nhân viên' }]}
              >
                <Select
                  showSearch
                  placeholder="Chọn nhân viên"
                  optionFilterProp="children"
                  options={nhanVienList.map((nv) => ({
                    value: nv.MANV,
                    label: `${nv.HOTEN} (#${nv.MANV})`,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Ngay" label="Ngày ban hành">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item
            name="NoiDung"
            label="Nội dung quyết định"
            rules={[{ required: true, message: 'Vui lòng nhập nội dung' }]}
          >
            <Input.TextArea rows={2} placeholder="Nội dung khen thưởng hoặc xử lý kỷ luật..." />
          </Form.Item>
          <Form.Item name="LyDo" label="Lý do">
            <Input placeholder="Lập thành tích xuất sắc, vi phạm nội quy..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* MODAL TẠO QUYẾT ĐỊNH NÂNG LƯƠNG */}
      <Modal
        title="Tạo Quyết định Nâng lương"
        open={nlModalVisible}
        onOk={handleSaveNl}
        onCancel={() => setNlModalVisible(false)}
        okText="Lưu Nâng lương"
        cancelText="Hủy"
        width={600}
      >
        <Form form={formNl} layout="vertical" style={{ marginTop: 16 }}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="SoQd"
                label="Số Quyết định"
                rules={[{ required: true, message: 'Vui lòng nhập số QĐ' }]}
              >
                <Input placeholder="Ví dụ: 05/2023/QĐNL" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="SoHd" label="Số Hợp đồng liên quan">
                <Input placeholder="01/2023/HĐLĐ" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="MaNv"
                label="Nhân viên"
                rules={[{ required: true, message: 'Chọn nhân viên' }]}
              >
                <Select
                  showSearch
                  placeholder="Chọn nhân viên"
                  optionFilterProp="children"
                  options={nhanVienList.map((nv) => ({
                    value: nv.MANV,
                    label: `${nv.HOTEN} (#${nv.MANV})`,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="HeSoLuongCu" label="Hệ số cũ" initialValue={1.0}>
                <InputNumber step={0.1} min={0.5} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="HeSoLuongMoi"
                label="Hệ số mới"
                initialValue={1.2}
                rules={[{ required: true, message: 'Hệ số mới' }]}
              >
                <InputNumber step={0.1} min={0.5} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="NgayKy" label="Ngày ký">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="NgayLenLuong" label="Ngày bắt đầu hưởng">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="GhiChu" label="Ghi chú">
            <Input placeholder="Hoàn thành xuất sắc nhiệm vụ năm..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* MODAL TẠO QUYẾT ĐỊNH ĐIỀU CHUYỂN */}
      <Modal
        title="Tạo Quyết định Điều chuyển Phòng ban"
        open={dcModalVisible}
        onOk={handleSaveDc}
        onCancel={() => setDcModalVisible(false)}
        okText="Lưu Điều chuyển"
        cancelText="Hủy"
        width={600}
      >
        <Form form={formDc} layout="vertical" style={{ marginTop: 16 }}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="SoQd"
                label="Số Quyết định"
                rules={[{ required: true, message: 'Vui lòng nhập số QĐ' }]}
              >
                <Input placeholder="Ví dụ: 03/2023/QĐĐC" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="MaNv"
                label="Nhân viên điều chuyển"
                rules={[{ required: true, message: 'Chọn nhân viên' }]}
              >
                <Select
                  showSearch
                  placeholder="Chọn nhân viên"
                  optionFilterProp="children"
                  options={nhanVienList.map((nv) => ({
                    value: nv.MANV,
                    label: `${nv.HOTEN} (#${nv.MANV})`,
                  }))}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="IdPb"
                label="Phòng ban hiện tại"
                rules={[{ required: true, message: 'Chọn phòng ban hiện tại' }]}
              >
                <Select
                  placeholder="Chọn phòng ban cũ"
                  options={danhMuc?.phongBan?.map((pb) => ({
                    value: pb.IDPB,
                    label: pb.TENPB,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="IdPb2"
                label="Phòng ban mới"
                rules={[{ required: true, message: 'Chọn phòng ban chuyển đến' }]}
              >
                <Select
                  placeholder="Chọn phòng ban mới"
                  options={danhMuc?.phongBan?.map((pb) => ({
                    value: pb.IDPB,
                    label: pb.TENPB,
                  }))}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Ngay" label="Ngày điều chuyển">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="LyDo" label="Lý do">
                <Input placeholder="Luân chuyển cán bộ, mở rộng chi nhánh..." />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="GhiChu" label="Ghi chú">
            <Input placeholder="Ghi chú thêm..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* MODAL TẠO BÁO CÁO TĂNG CA */}
      <Modal
        title="Báo cáo Tăng ca / Làm thêm giờ (OT)"
        open={tcModalVisible}
        onOk={handleSaveTc}
        onCancel={() => setTcModalVisible(false)}
        okText="Lưu Tăng ca"
        cancelText="Hủy"
        width={560}
      >
        <Form
          form={formTc}
          layout="vertical"
          style={{ marginTop: 16 }}
          initialValues={{ Nam: new Date().getFullYear(), Thang: new Date().getMonth() + 1, SoGio: 2 }}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Thang" label="Tháng" rules={[{ required: true }]}>
                <InputNumber min={1} max={12} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Nam" label="Năm" rules={[{ required: true }]}>
                <InputNumber min={2020} max={2030} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={14}>
              <Form.Item
                name="MaNv"
                label="Nhân viên"
                rules={[{ required: true, message: 'Chọn nhân viên' }]}
              >
                <Select
                  showSearch
                  placeholder="Chọn nhân viên"
                  optionFilterProp="children"
                  options={nhanVienList.map((nv) => ({
                    value: nv.MANV,
                    label: `${nv.HOTEN} (#${nv.MANV})`,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item name="Ngay" label="Ngày tăng ca">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="IdLoaiCa"
                label="Loại ca làm việc"
                rules={[{ required: true, message: 'Chọn loại ca' }]}
              >
                <Select
                  placeholder="Chọn loại ca"
                  options={loaiCaList.map((lc) => ({
                    value: lc.IDLOAICA,
                    label: `${lc.TENLOAICA} (${lc.HESOLOAICA ?? 1.5}x)`,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="SoGio"
                label="Số giờ làm thêm"
                rules={[{ required: true, message: 'Nhập số giờ' }]}
              >
                <InputNumber min={0.5} max={12} step={0.5} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="GhiChu" label="Ghi chú">
            <Input placeholder="Tăng ca dự án, hoàn thành kiểm kê..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* MODAL TẠM ỨNG LƯƠNG */}
      <Modal
        title="Đăng ký Tạm ứng Lương"
        open={ulModalVisible}
        onOk={handleSaveUl}
        onCancel={() => setUlModalVisible(false)}
        okText="Lưu Tạm ứng"
        cancelText="Hủy"
        width={560}
      >
        <Form
          form={formUl}
          layout="vertical"
          style={{ marginTop: 16 }}
          initialValues={{ Nam: new Date().getFullYear(), Thang: new Date().getMonth() + 1, SoTien: 1000000 }}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Thang" label="Tháng ứng" rules={[{ required: true }]}>
                <InputNumber min={1} max={12} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Nam" label="Năm" rules={[{ required: true }]}>
                <InputNumber min={2020} max={2030} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={14}>
              <Form.Item
                name="MaNv"
                label="Nhân viên tạm ứng"
                rules={[{ required: true, message: 'Chọn nhân viên' }]}
              >
                <Select
                  showSearch
                  placeholder="Chọn nhân viên"
                  optionFilterProp="children"
                  options={nhanVienList.map((nv) => ({
                    value: nv.MANV,
                    label: `${nv.HOTEN} (#${nv.MANV})`,
                  }))}
                />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item name="Ngay" label="Ngày ứng">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item
            name="SoTien"
            label="Số tiền tạm ứng (VNĐ)"
            rules={[{ required: true, message: 'Nhập số tiền' }]}
          >
            <InputNumber
              style={{ width: '100%' }}
              step={500000}
              min={100000}
              formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            />
          </Form.Item>
          <Form.Item name="GhiChu" label="Lý do tạm ứng">
            <Input placeholder="Chi tiêu cá nhân, giải quyết việc gia đình..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* MODAL TẠO TÀI KHOẢN / NHÓM QUYỀN MỚI */}
      <Modal
        title={isCreatingGroup ? 'Thêm Nhóm quyền Hệ thống Mới' : 'Thêm Tài khoản Người dùng Hệ thống'}
        open={createUserModalVisible}
        onOk={handleCreateUser}
        onCancel={() => setCreateUserModalVisible(false)}
        okText={isCreatingGroup ? 'Tạo nhóm quyền' : 'Tạo tài khoản'}
        cancelText="Hủy"
        width={540}
      >
        <Form form={formCreateUser} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="IsGroup" label="Loại đối tượng" initialValue={false}>
            <Select
              onChange={(val) => setIsCreatingGroup(val)}
              options={[
                { value: false, label: '👤 Người dùng cá nhân (Tài khoản đăng nhập)' },
                { value: true, label: '🏷️ Nhóm quyền hệ thống (Role)' },
              ]}
            />
          </Form.Item>

          <Form.Item
            name="Username"
            label={isCreatingGroup ? 'Mã nhóm quyền (Tên nhóm)' : 'Tên đăng nhập'}
            rules={[{ required: true, message: 'Vui lòng nhập trường này' }]}
          >
            <Input placeholder={isCreatingGroup ? 'Ví dụ: NHOM_KE_TOAN, NHOM_KY_THUAT...' : 'Ví dụ: nhansu02, ketoan01...'} />
          </Form.Item>

          <Form.Item
            name="FullName"
            label={isCreatingGroup ? 'Mô tả / Tên đầy đủ của nhóm' : 'Họ và tên người dùng'}
            rules={[{ required: true, message: 'Vui lòng nhập họ tên hoặc mô tả' }]}
          >
            <Input placeholder={isCreatingGroup ? 'Ví dụ: Nhóm Kế toán thanh toán' : 'Ví dụ: Nguyễn Văn Hoàng'} />
          </Form.Item>

          {!isCreatingGroup && (
            <Form.Item
              name="Password"
              label="Mật khẩu khởi tạo"
              rules={[{ required: true, message: 'Vui lòng nhập mật khẩu' }]}
              initialValue="123"
            >
              <Input.Password placeholder="Mật khẩu bảo mật (mặc định: 123)..." />
            </Form.Item>
          )}
        </Form>
      </Modal>

      {/* MODAL PHÂN QUYỀN CHỨC NĂNG (Tương thích WinForms FrmPhanQuyenChucNang) */}
      {selectedUserForPerms && (
        <PhanQuyenModal
          visible={phanQuyenModalVisible}
          onClose={() => setPhanQuyenModalVisible(false)}
          userId={selectedUserForPerms.IdUser}
          username={selectedUserForPerms.Username}
          fullName={selectedUserForPerms.FullName}
          isGroup={selectedUserForPerms.IsGroup}
          onSuccess={() => {
            fetchUsers();
          }}
        />
      )}

      {/* MODAL QUẢN LÝ THÀNH VIÊN NHÓM (Tương thích WinForms frmGroup & FrmShowUser_Group) */}
      {selectedGroupForMembers && (
        <GroupMembersModal
          visible={groupMembersModalVisible}
          onClose={() => setGroupMembersModalVisible(false)}
          groupId={selectedGroupForMembers.IdUser}
          groupName={selectedGroupForMembers.Username}
          groupFullName={selectedGroupForMembers.FullName}
          onSuccess={() => {
            fetchUsers();
          }}
        />
      )}

      {/* MODAL CHỈNH SỬA THÔNG TIN NGƯỜI DÙNG / NHÓM */}
      <UserEditModal
        visible={userEditModalVisible}
        onClose={() => setUserEditModalVisible(false)}
        user={selectedUserForEdit}
        onSuccess={() => {
          fetchUsers();
        }}
      />

      {/* MODAL ĐỔI MẬT KHẨU CÁ NHÂN */}
      <ChangePasswordModal
        visible={changePasswordModalVisible}
        onClose={() => setChangePasswordModalVisible(false)}
        username={currentUser.Username}
      />

      {/* MODAL PHIẾU LƯƠNG CÁ NHÂN */}
      <PhieuLuongModal
        visible={phieuLuongModalVisible}
        onClose={() => setPhieuLuongModalVisible(false)}
        record={selectedBangLuong}
        kyCongLabel={
          selectedKyCong
            ? `Kỳ Lương #${selectedKyCong} (${kyCongList.find((k) => k.MAKYCONG === selectedKyCong)?.THANG}/${
                kyCongList.find((k) => k.MAKYCONG === selectedKyCong)?.NAM
              })`
            : undefined
        }
      />

      {/* AI COPILOT CHAT DRAWER */}
      <Drawer
        title={
          <Space>
            <Avatar style={{ backgroundColor: '#722ed1' }} icon={<RobotOutlined />} />
            <div>
              <Text strong>AI HRMS Copilot</Text>
              <div>
                <Tag color="green" style={{ fontSize: 10 }}>Oracle + Qwen 2.5 RAG</Tag>
              </div>
            </div>
          </Space>
        }
        placement="right"
        width={440}
        onClose={() => setAiDrawerVisible(false)}
        open={aiDrawerVisible}
      >
        <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          <div style={{ flex: 1, overflowY: 'auto', paddingRight: 4, display: 'flex', flexDirection: 'column', gap: 12 }}>
            {chatMessages.map((msg) => (
              <div
                key={msg.id}
                style={{
                  alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  maxWidth: '85%',
                  background: msg.sender === 'user' ? '#1677ff' : '#f0f2f5',
                  color: msg.sender === 'user' ? '#fff' : '#1f1f1f',
                  padding: '10px 14px',
                  borderRadius: msg.sender === 'user' ? '12px 12px 2px 12px' : '12px 12px 12px 2px',
                  fontSize: '13.5px',
                  lineHeight: '1.5',
                }}
              >
                <div style={{ whiteSpace: 'pre-wrap' }}>{msg.content}</div>
                {msg.source && (
                  <div style={{ marginTop: 4 }}>
                    <Tag color={msg.source === 'RAG_Ollama' ? 'purple' : 'blue'} style={{ fontSize: 10 }}>
                      {msg.source === 'RAG_Ollama' ? '🤖 Qwen 2.5 RAG' : '⚡ Oracle DB Live'}
                    </Tag>
                  </div>
                )}
                <div
                  style={{
                    fontSize: '10px',
                    color: msg.sender === 'user' ? 'rgba(255,255,255,0.7)' : '#8c8c8c',
                    marginTop: 4,
                    textAlign: 'right',
                  }}
                >
                  {msg.timestamp}
                </div>
              </div>
            ))}
            {chatLoading && (
              <div style={{ alignSelf: 'flex-start', padding: 8 }}>
                <Spin size="small" /> <Text type="secondary" style={{ fontSize: 12, marginLeft: 8 }}>AI đang tra cứu dữ liệu...</Text>
              </div>
            )}
          </div>

          <div style={{ margin: '12px 0' }}>
            <Text type="secondary" style={{ fontSize: 11 }}>Gợi ý nhanh:</Text>
            <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginTop: 4 }}>
              <Tag
                style={{ cursor: 'pointer' }}
                onClick={() => handleSendMessage('Quy định tính tiền làm thêm giờ (OT)?')}
              >
                💰 Quy định làm thêm giờ
              </Tag>
              <Tag
                style={{ cursor: 'pointer' }}
                onClick={() => handleSendMessage('Hệ thống hiện tại có bao nhiêu nhân viên?')}
              >
                👥 Số lượng nhân sự
              </Tag>
              <Tag
                style={{ cursor: 'pointer' }}
                onClick={() => handleSendMessage('Tổng quỹ lương kỳ hiện tại là bao nhiêu?')}
              >
                💵 Tổng quỹ lương
              </Tag>
              <Tag
                style={{ cursor: 'pointer' }}
                onClick={() => handleSendMessage('Công ty có bao nhiêu phòng ban?')}
              >
                🏢 Danh sách phòng ban
              </Tag>
            </div>
          </div>

          <Space.Compact style={{ width: '100%' }}>
            <Input
              placeholder="Nhập câu hỏi cho AI Copilot..."
              value={chatInput}
              onChange={(e) => setChatInput(e.target.value)}
              onPressEnter={() => handleSendMessage()}
              disabled={chatLoading}
            />
            <Button
              type="primary"
              icon={<SendOutlined />}
              onClick={() => handleSendMessage()}
              loading={chatLoading}
              style={{ background: '#722ed1', borderColor: '#722ed1' }}
            >
              Gửi
            </Button>
          </Space.Compact>
        </div>
      </Drawer>
    </Layout>
  );
}

export default App;
