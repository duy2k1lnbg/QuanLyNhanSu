import { useState, useEffect } from 'react';
import { notification } from 'antd';
import api from './services/api';
import Login from './pages/Login';
import MainLayout from './components/MainLayout';
import AiChatDrawer from './components/AiChatDrawer';
import ChangePasswordModal from './components/ChangePasswordModal';
import CommandPaletteModal from './components/CommandPaletteModal';
import Employee360Modal from './components/Employee360Modal';
import type { NotificationItem } from './components/NotificationPopoverContent';

// Modular Page Components
import DashboardPage from './pages/DashboardPage';
import NhanVienPage from './pages/NhanVienPage';
import ChamCongPage from './pages/ChamCongPage';
import BangLuongPage from './pages/BangLuongPage';
import HopDongPage from './pages/HopDongPage';
import KhenThuongKyLuatPage from './pages/KhenThuongKyLuatPage';
import NangLuongDieuChuyenPage from './pages/NangLuongDieuChuyenPage';
import TangCaUngLuongPage from './pages/TangCaUngLuongPage';
import UserManagementPage from './pages/UserManagementPage';

import type {
  NhanVienDTO,
  DashboardLuongDTO,
  DashboardPhongBanDTO,
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
  ActionItemDTO,
  AnomalyItemDTO,
} from './types/hrms';

// Bản đồ định tuyến URL hỗ trợ đầy đủ các nút Back, Forward, Reload của trình duyệt
import { VALID_ROUTES, ROUTE_TITLES, getRouteFromLocation } from './utils/routes';


export function App() {
  // 1. Trạng thái xác thực người dùng
  const [currentUser, setCurrentUser] = useState<CurrentUserDTO | null>(() => {
    const saved = localStorage.getItem('hrms_user');
    const token = localStorage.getItem('hrms_token');
    if (saved && token) {
      try {
        const u = JSON.parse(saved);
        if (u && u.Username) {
          return {
            IdUser: u.IdUser ?? u.id ?? 0,
            Username: u.Username || u.username || '',
            FullName: u.FullName || u.fullName || u.Username,
            IsAdmin: Boolean(u.IsAdmin ?? u.isAdmin),
            Rights: u.Rights || u.rights || [],
          };
        }
      } catch {
        // ignore invalid saved user
      }
    }
    return null;
  });

  // Khởi tạo menu từ URL hash để khi Reload (F5) không bị mất trang hiện tại
  const [currentMenu, setCurrentMenu] = useState<string>(() => getRouteFromLocation());
  const [aiDrawerVisible, setAiDrawerVisible] = useState(false);
  const [changePasswordModalVisible, setChangePasswordModalVisible] = useState(false);
  const [isBackendConnected, setIsBackendConnected] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  // 2. Dữ liệu ứng dụng
  const [nhanVienList, setNhanVienList] = useState<NhanVienDTO[]>([]);
  const [luongStats, setLuongStats] = useState<DashboardLuongDTO[]>([]);
  const [phongBanStats, setPhongBanStats] = useState<DashboardPhongBanDTO[]>([]);
  const [totalEmployees, setTotalEmployees] = useState<number>(0);
  const [totalSalary, setTotalSalary] = useState<number>(0);
  const [presentToday, setPresentToday] = useState<number>(0);
  const [absentToday, setAbsentToday] = useState<number>(0);
  const [lateToday, setLateToday] = useState<number>(0);
  const [actionItems, setActionItems] = useState<ActionItemDTO[]>([]);
  const [anomalies, setAnomalies] = useState<AnomalyItemDTO[]>([]);
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);

  // 3. Global Search & Employee 360
  const [commandPaletteVisible, setCommandPaletteVisible] = useState<boolean>(false);
  const [globalEmployee360, setGlobalEmployee360] = useState<NhanVienDTO | null>(null);
  const [global360Visible, setGlobal360Visible] = useState<boolean>(false);

  const [danhMuc, setDanhMuc] = useState<DanhMucAllDTO | null>(null);
  const [kyCongList, setKyCongList] = useState<KyCongDTO[]>([]);
  const [selectedKyCong, setSelectedKyCong] = useState<number>(0);

  // Bảng lương & Chấm công
  const [bangLuongList, setBangLuongList] = useState<BangLuongDTO[]>([]);
  const [bangLuongLoading, setBangLuongLoading] = useState<boolean>(false);
  const [tinhLuongLoading, setTinhLuongLoading] = useState<boolean>(false);
  const [chamCongList, setChamCongList] = useState<KyCongChiTietDTO[]>([]);
  const [loaiCaList, setLoaiCaList] = useState<LoaiCaDTO[]>([]);
  const [chamCongLoading, setChamCongLoading] = useState<boolean>(false);

  // Hợp đồng, Khen thưởng, Nâng lương, Tăng ca, Người dùng
  const [hopDongList, setHopDongList] = useState<HopDongDTO[]>([]);
  const [hopDongLoading, setHopDongLoading] = useState<boolean>(false);
  const [khenThuongList, setKhenThuongList] = useState<KhenThuongDTO[]>([]);
  const [kyLuatList, setKyLuatList] = useState<KhenThuongDTO[]>([]);
  const [ktLoading, setKtLoading] = useState<boolean>(false);
  const [nangLuongList, setNangLuongList] = useState<NangLuongDTO[]>([]);
  const [dieuChuyenList, setDieuChuyenList] = useState<DieuChuyenDTO[]>([]);
  const [nlDcLoading, setNlDcLoading] = useState<boolean>(false);
  const [ungLuongList, setUngLuongList] = useState<UngLuongDTO[]>([]);
  const [tangCaList, setTangCaList] = useState<TangCaDTO[]>([]);
  const [tcUlLoading, setTcUlLoading] = useState<boolean>(false);
  const [userList, setUserList] = useState<SysUserDTO[]>([]);
  const [userLoading, setUserLoading] = useState<boolean>(false);

  // Lắng nghe phím tắt tìm kiếm toàn cục (Ctrl + K, Alt + K hoặc / khi không ở ô nhập)
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const isInput = ['INPUT', 'TEXTAREA'].includes((e.target as HTMLElement)?.tagName);
      if (((e.ctrlKey || e.metaKey || e.altKey) && (e.key === 'k' || e.key === 'K')) || (!isInput && e.key === '/')) {
        e.preventDefault();
        setCommandPaletteVisible((prev) => !prev);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Check quyền tương tự WinForms: Admin và tài khoản ADMIN luôn có toàn quyền tuyệt đối
  const hasRight = (...codes: string[]) => {
    if (!currentUser) return false;
    if (currentUser.IsAdmin || currentUser.Username?.toUpperCase() === 'ADMIN' || currentUser.Rights?.includes('*')) return true;
    return codes.some((code) =>
      currentUser.Rights?.some(
        (r) =>
          r.toUpperCase() === code.toUpperCase() ||
          r.toUpperCase().includes(code.toUpperCase()) ||
          code.toUpperCase().includes(r.toUpperCase())
      )
    );
  };

  // Tải dữ liệu ban đầu
  const fetchInitialData = async () => {
    setLoading(true);
    try {
      const [dashRes, nvRes, dmRes, kcRes, notifRes] = await Promise.allSettled([
        api.get('/dashboard/stats'),
        api.get<NhanVienDTO[]>('/nhanvien'),
        api.get<DanhMucAllDTO>('/danhmuc/all'),
        api.get<KyCongDTO[]>('/bangluong/kycong'),
        api.get('/dashboard/notifications'),
      ]);

      if (dashRes.status === 'fulfilled' && dashRes.value.data) {
        setIsBackendConnected(true);
        const data = dashRes.value.data;
        setTotalEmployees(data.tongNhanVien ?? data.TotalEmployees ?? 0);
        setTotalSalary(data.tongQuyLuong ?? data.TotalSalary ?? 0);
        setPresentToday(data.presentToday ?? 0);
        setAbsentToday(data.absentToday ?? 0);
        setLateToday(data.lateToday ?? 0);
        setLuongStats(data.luongStats ?? data.SalaryTrends ?? []);
        setPhongBanStats(data.phongBanStats ?? data.DepartmentDistribution ?? []);
        setActionItems(data.actionItems ?? []);
        setAnomalies(data.anomalies ?? []);
      }

      if (notifRes.status === 'fulfilled' && notifRes.value.data) {
        const notifData = notifRes.value.data as any;
        const items = Array.isArray(notifData) ? notifData : (notifData.items ?? []);
        setNotifications(items);
      }

      if (nvRes.status === 'fulfilled' && nvRes.value.data) {
        setNhanVienList(nvRes.value.data);
      }

      if (dmRes.status === 'fulfilled' && dmRes.value.data) {
        setDanhMuc(dmRes.value.data);
      }

      if (kcRes.status === 'fulfilled' && kcRes.value.data && kcRes.value.data.length > 0) {
        setKyCongList(kcRes.value.data);
        const latest = kcRes.value.data[0].MAKYCONG;
        setSelectedKyCong(latest);
        fetchBangLuong(latest);
        fetchChamCong(latest);
      }
    } catch {
      setIsBackendConnected(false);
    } finally {
      setLoading(false);
    }
  };

  const fetchKyCongList = async () => {
    try {
      const res = await api.get<KyCongDTO[]>('/bangluong/kycong');
      if (res.data && Array.isArray(res.data)) {
        setKyCongList(res.data);
      }
    } catch {
      // ignore
    }
  };

  const fetchBangLuong = async (makycong: number) => {
    if (!makycong) return;
    setBangLuongLoading(true);
    try {
      const res = await api.get<any>(`/bangluong?makycong=${makycong}`);
      if (res.data) {
        const raw = Array.isArray(res.data) ? res.data : (res.data?.items ?? []);
        setBangLuongList(raw);
      }
    } catch {
      // ignore
    } finally {
      setBangLuongLoading(false);
    }
  };

  const handleTinhLuong = async () => {
    if (!selectedKyCong) return;
    setTinhLuongLoading(true);
    try {
      await api.post(`/bangluong/tinhluong?makycong=${selectedKyCong}`);
      notification.success({ message: 'Thành công', description: 'Đã tính toán bảng lương tự động cho toàn bộ nhân sự!' });
      fetchBangLuong(selectedKyCong);
    } catch {
      notification.error({ message: 'Lỗi', description: 'Không thể tính lương lúc này.' });
    } finally {
      setTinhLuongLoading(false);
    }
  };

  const fetchChamCong = async (makycong: number) => {
    if (!makycong) return;
    setChamCongLoading(true);
    try {
      const [ccRes, lcRes] = await Promise.allSettled([
        api.get<any>(`/chamcong/chitiet?makycong=${makycong}`),
        api.get<LoaiCaDTO[]>('/chamcong/loaica'),
      ]);
      if (ccRes.status === 'fulfilled' && ccRes.value.data) {
        const raw = Array.isArray(ccRes.value.data) ? ccRes.value.data : (ccRes.value.data?.items ?? []);
        setChamCongList(raw);
      }
      if (lcRes.status === 'fulfilled' && lcRes.value.data) setLoaiCaList(lcRes.value.data);
    } catch {
      // ignore
    } finally {
      setChamCongLoading(false);
    }
  };

  const fetchHopDong = async () => {
    setHopDongLoading(true);
    try {
      const res = await api.get<HopDongDTO[]>('/hopdong');
      if (res.data) setHopDongList(res.data);
    } catch {
      // ignore
    } finally {
      setHopDongLoading(false);
    }
  };

  const fetchKhenThuongKyLuat = async () => {
    setKtLoading(true);
    try {
      const [ktRes, klRes] = await Promise.allSettled([
        api.get<KhenThuongDTO[]>('/khenthuong?loai=1'),
        api.get<KhenThuongDTO[]>('/khenthuong?loai=2'),
      ]);
      if (ktRes.status === 'fulfilled' && ktRes.value.data) setKhenThuongList(ktRes.value.data);
      if (klRes.status === 'fulfilled' && klRes.value.data) setKyLuatList(klRes.value.data);
    } catch {
      // ignore
    } finally {
      setKtLoading(false);
    }
  };

  const fetchNangLuongDieuChuyen = async () => {
    setNlDcLoading(true);
    try {
      const [nlRes, dcRes] = await Promise.allSettled([
        api.get<NangLuongDTO[]>('/nangluong'),
        api.get<DieuChuyenDTO[]>('/dieuchuyen'),
      ]);
      if (nlRes.status === 'fulfilled' && nlRes.value.data) setNangLuongList(nlRes.value.data);
      if (dcRes.status === 'fulfilled' && dcRes.value.data) setDieuChuyenList(dcRes.value.data);
    } catch {
      // ignore
    } finally {
      setNlDcLoading(false);
    }
  };

  const fetchTangCaUngLuong = async () => {
    setTcUlLoading(true);
    try {
      const [ulRes, tcRes] = await Promise.allSettled([
        api.get<UngLuongDTO[]>('/ungluong'),
        api.get<TangCaDTO[]>('/tangca'),
      ]);
      if (ulRes.status === 'fulfilled' && ulRes.value.data) setUngLuongList(ulRes.value.data);
      if (tcRes.status === 'fulfilled' && tcRes.value.data) setTangCaList(tcRes.value.data);
    } catch {
      // ignore
    } finally {
      setTcUlLoading(false);
    }
  };

  const fetchUsers = async () => {
    setUserLoading(true);
    try {
      const res = await api.get<SysUserDTO[]>('/users');
      if (res.data) setUserList(res.data);
    } catch {
      // ignore
    } finally {
      setUserLoading(false);
    }
  };

  // Tự động đồng bộ quyền hạn mới nhất từ CSDL trong nền mà không cần người dùng thao tác thủ công
  const syncUserRights = async () => {
    try {
      const res = await api.get<{ user: CurrentUserDTO }>('/auth/me');
      if (res.data && res.data.user) {
        const refreshed = res.data.user;
        setCurrentUser((prev) => {
          if (!prev) return refreshed;
          const prevRights = JSON.stringify(prev.Rights || []);
          const newRights = JSON.stringify(refreshed.Rights || []);
          if (prevRights !== newRights || prev.IsAdmin !== refreshed.IsAdmin || prev.FullName !== refreshed.FullName) {
            localStorage.setItem('hrms_user', JSON.stringify(refreshed));
            return refreshed;
          }
          return prev;
        });
      }
    } catch {
      // chạy nền êm dịu, không gián đoạn người dùng
    }
  };

  useEffect(() => {
    if (currentUser) {
      fetchInitialData();
      syncUserRights();
    }
  }, [currentUser?.Username]);

  // Điều hướng đồng bộ với URL hash và lịch sử trình duyệt (Hỗ trợ Back, Forward, Reload)
  const handleNavigate = (route: string, replace: boolean = false) => {
    const cleanRoute = route.replace(/^#?\/?/, '').trim().toLowerCase();
    const targetRoute = VALID_ROUTES[cleanRoute] || 'dashboard';

    setCurrentMenu(targetRoute);

    if (typeof window !== 'undefined') {
      const targetHash = `#/${targetRoute}`;
      if (window.location.hash !== targetHash) {
        if (replace) {
          window.history.replaceState({ route: targetRoute }, '', targetHash);
        } else {
          window.history.pushState({ route: targetRoute }, '', targetHash);
        }
      }
      document.title = ROUTE_TITLES[targetRoute] || 'HRMS Enterprise';
    }
  };

  // Đồng bộ hai chiều với các nút trình duyệt: Back (<-), Forward (->), Reload (F5)
  useEffect(() => {
    const handleLocationChange = () => {
      const route = getRouteFromLocation();
      setCurrentMenu(route);
      document.title = ROUTE_TITLES[route] || 'HRMS Enterprise';
    };

    window.addEventListener('popstate', handleLocationChange);
    window.addEventListener('hashchange', handleLocationChange);

    // Khi đã đăng nhập, đảm bảo URL có hash để khi F5 / Reload luôn giữ nguyên phân hệ hiện tại
    if (currentUser) {
      const initialRoute = getRouteFromLocation();
      const targetHash = `#/${initialRoute}`;
      if (window.location.hash !== targetHash) {
        window.history.replaceState({ route: initialRoute }, '', targetHash);
      }
      document.title = ROUTE_TITLES[initialRoute] || 'HRMS Enterprise';
    }

    return () => {
      window.removeEventListener('popstate', handleLocationChange);
      window.removeEventListener('hashchange', handleLocationChange);
    };
  }, [currentUser]);

  // Lazy load dữ liệu theo tab được chọn
  useEffect(() => {
    if (!currentUser) return;
    if (currentMenu === 'hopdong') fetchHopDong();
    if (currentMenu === 'khenthuong') fetchKhenThuongKyLuat();
    if (currentMenu === 'nangluong') fetchNangLuongDieuChuyen();
    if (currentMenu === 'ungluong') fetchTangCaUngLuong();
    if (currentMenu === 'phanquyen') fetchUsers();
  }, [currentMenu, currentUser]);

  const handleLoginSuccess = (user: CurrentUserDTO) => {
    setCurrentUser(user);
    const targetRoute = getRouteFromLocation();
    handleNavigate(targetRoute, true);
    notification.success({
      message: 'Đăng nhập thành công',
      description: `Chào mừng ${user.FullName} quay trở lại HRMS Enterprise!`,
    });
  };

  const handleLogout = () => {
    localStorage.removeItem('hrms_token');
    localStorage.removeItem('hrms_user');
    setCurrentUser(null);
    if (typeof window !== 'undefined') {
      window.history.replaceState(null, '', window.location.pathname);
      document.title = 'Đăng nhập | HRMS Enterprise';
    }
    notification.info({ message: 'Đã đăng xuất', description: 'Hẹn gặp lại bạn!' });
  };

  const handleMarkAllNotificationsRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  };

  const handleMarkNotificationRead = (id: string, route: string) => {
    setNotifications((prev) => prev.map((n) => (n.id === id ? { ...n, read: true } : n)));
    handleNavigate(route);
  };

  // Chưa đăng nhập -> hiển thị màn hình Login
  if (!currentUser) {
    return <Login onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <>
      <MainLayout
        currentUser={currentUser}
        currentMenu={currentMenu}
        onMenuChange={(key) => handleNavigate(key)}
        onLogout={handleLogout}
        onRefreshData={fetchInitialData}
        onOpenChangePassword={() => setChangePasswordModalVisible(true)}
        onOpenAiDrawer={() => setAiDrawerVisible(true)}
        onOpenCommandPalette={() => setCommandPaletteVisible(true)}
        isBackendConnected={isBackendConnected}
        loading={loading}
        kyCongCount={kyCongList.length}
        hasRight={hasRight}
        notifications={notifications}
        onMarkAllNotificationsRead={handleMarkAllNotificationsRead}
        onMarkNotificationRead={handleMarkNotificationRead}
      >
        {currentMenu === 'dashboard' && (
          <DashboardPage
            totalEmployees={totalEmployees}
            hopDongCount={hopDongList.length}
            totalSalary={totalSalary}
            currentUser={currentUser}
            isBackendConnected={isBackendConnected}
            luongStats={luongStats}
            phongBanStats={phongBanStats}
            nhanVienList={nhanVienList}
            bangLuongList={bangLuongList}
            onNavigate={(key) => handleNavigate(key)}
            presentToday={presentToday}
            absentToday={absentToday}
            lateToday={lateToday}
            actionItems={actionItems}
            anomalies={anomalies}
          />
        )}

        {currentMenu === 'nhanvien' && (
          <NhanVienPage
            nhanVienList={nhanVienList}
            danhMuc={danhMuc}
            loading={loading}
            hasRight={hasRight}
            onRefresh={fetchInitialData}
          />
        )}

        {currentMenu === 'chamcong' && (
          <ChamCongPage
            kyCongList={kyCongList}
            selectedKyCong={selectedKyCong}
            onSelectKyCong={(val) => {
              setSelectedKyCong(val);
              fetchChamCong(val);
            }}
            chamCongList={chamCongList}
            chamCongLoading={chamCongLoading}
            loaiCaList={loaiCaList}
            onRefresh={() => fetchChamCong(selectedKyCong)}
          />
        )}

        {currentMenu === 'bangluong' && (
          <BangLuongPage
            kyCongList={kyCongList}
            selectedKyCong={selectedKyCong}
            onSelectKyCong={(val) => {
              setSelectedKyCong(val);
              fetchBangLuong(val);
            }}
            bangLuongList={bangLuongList}
            bangLuongLoading={bangLuongLoading}
            tinhLuongLoading={tinhLuongLoading}
            onTinhLuong={handleTinhLuong}
            onRefresh={() => fetchBangLuong(selectedKyCong)}
            hasRight={hasRight}
            danhMuc={danhMuc}
            onRefreshKyCong={fetchKyCongList}
          />
        )}

        {currentMenu === 'hopdong' && (
          <HopDongPage
            hopDongList={hopDongList}
            hopDongLoading={hopDongLoading}
            onRefresh={fetchHopDong}
            hasRight={hasRight}
          />
        )}

        {currentMenu === 'khenthuong' && (
          <KhenThuongKyLuatPage
            khenThuongList={khenThuongList}
            kyLuatList={kyLuatList}
            ktLoading={ktLoading}
            onRefresh={fetchKhenThuongKyLuat}
            hasRight={hasRight}
          />
        )}

        {currentMenu === 'nangluong' && (
          <NangLuongDieuChuyenPage
            nangLuongList={nangLuongList}
            dieuChuyenList={dieuChuyenList}
            nlDcLoading={nlDcLoading}
            danhMuc={danhMuc}
            onRefresh={fetchNangLuongDieuChuyen}
            hasRight={hasRight}
          />
        )}

        {currentMenu === 'ungluong' && (
          <TangCaUngLuongPage
            ungLuongList={ungLuongList}
            tangCaList={tangCaList}
            tcUlLoading={tcUlLoading}
            onRefresh={fetchTangCaUngLuong}
            hasRight={hasRight}
          />
        )}

        {currentMenu === 'phanquyen' && (
          <UserManagementPage
            userList={userList}
            userLoading={userLoading}
            currentUser={currentUser}
            onRefresh={fetchUsers}
          />
        )}
      </MainLayout>

      {/* Drawer AI Copilot */}
      <AiChatDrawer
        open={aiDrawerVisible}
        onClose={() => setAiDrawerVisible(false)}
        isMobile={typeof window !== 'undefined' && window.innerWidth < 768}
        onNavigate={(route) => handleNavigate(route)}
      />

      {/* Modal đổi mật khẩu cá nhân */}
      <ChangePasswordModal
        visible={changePasswordModalVisible}
        onClose={() => setChangePasswordModalVisible(false)}
        username={currentUser.Username}
      />

      {/* Command Palette Global Search (Ctrl + K) */}
      <CommandPaletteModal
        visible={commandPaletteVisible}
        onClose={() => setCommandPaletteVisible(false)}
        nhanVienList={nhanVienList}
        kyCongList={kyCongList}
        hopDongList={hopDongList}
        onSelectEmployee={(emp) => {
          setGlobalEmployee360(emp);
          setGlobal360Visible(true);
        }}
        onNavigate={(route) => handleNavigate(route)}
        onOpenAiDrawer={() => setAiDrawerVisible(true)}
      />

      {/* Global Employee 360° Profile & Timeline */}
      <Employee360Modal
        visible={global360Visible}
        onClose={() => setGlobal360Visible(false)}
        employee={globalEmployee360}
      />
    </>
  );
}

export default App;
