using System;
using System.IO;
using System.Text;
using DevExpress.Utils.Svg;

namespace QLyNSu
{
    public partial class MainForm
    {
        private SvgImage LoadSvgFromString(string svgContent)
        {
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent)))
                {
                    return SvgImage.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private void InitializeNewSystemIcons()
        {
            // Set modern vector SvgImages for all 44 ribbon buttons dynamically
            if (btnPass != null) btnPass.ImageOptions.SvgImage = LoadSvgFromString(SvgPass);
            if (btnSaoLuu_DB != null) btnSaoLuu_DB.ImageOptions.SvgImage = LoadSvgFromString(SvgSaoLuu);
            if (btnPhucHoi_DB != null) btnPhucHoi_DB.ImageOptions.SvgImage = LoadSvgFromString(SvgPhucHoi);
            if (btnDanToc != null) btnDanToc.ImageOptions.SvgImage = LoadSvgFromString(SvgDanToc);
            if (btnTonGiao != null) btnTonGiao.ImageOptions.SvgImage = LoadSvgFromString(SvgTonGiao);
            if (btnTrinhDo != null) btnTrinhDo.ImageOptions.SvgImage = LoadSvgFromString(SvgTrinhDo);
            if (btnPhongBan != null) btnPhongBan.ImageOptions.SvgImage = LoadSvgFromString(SvgPhongBan);
            if (btnNhanVien != null) btnNhanVien.ImageOptions.SvgImage = LoadSvgFromString(SvgNhanVien);
            if (btnHopDong != null) btnHopDong.ImageOptions.SvgImage = LoadSvgFromString(SvgHopDong);
            if (btnKhenThuong != null) btnKhenThuong.ImageOptions.SvgImage = LoadSvgFromString(SvgKhenThuong);
            if (btnKyLuat != null) btnKyLuat.ImageOptions.SvgImage = LoadSvgFromString(SvgKyLuat);
            if (btnDieuChuyen != null) btnDieuChuyen.ImageOptions.SvgImage = LoadSvgFromString(SvgDieuChuyen);
            if (btnThoiViec != null) btnThoiViec.ImageOptions.SvgImage = LoadSvgFromString(SvgThoiViec);
            if (btnLoaiCa != null) btnLoaiCa.ImageOptions.SvgImage = LoadSvgFromString(SvgLoaiCa);
            if (btnLoaiCong != null) btnLoaiCong.ImageOptions.SvgImage = LoadSvgFromString(SvgLoaiCong);
            if (btnPhuCap != null) btnPhuCap.ImageOptions.SvgImage = LoadSvgFromString(SvgPhuCap);
            if (btnTangCa != null) btnTangCa.ImageOptions.SvgImage = LoadSvgFromString(SvgTangCa);
            if (btnUngLuong != null) btnUngLuong.ImageOptions.SvgImage = LoadSvgFromString(SvgUngLuong);
            if (btnBangCong != null) btnBangCong.ImageOptions.SvgImage = LoadSvgFromString(SvgBangCong);
            if (btnBangLuong != null) btnBangLuong.ImageOptions.SvgImage = LoadSvgFromString(SvgBangLuong);
            if (btnCongTy != null) btnCongTy.ImageOptions.SvgImage = LoadSvgFromString(SvgCongTy);
            if (btnBoPhan != null) btnBoPhan.ImageOptions.SvgImage = LoadSvgFromString(SvgBoPhan);
            if (btnChucVu != null) btnChucVu.ImageOptions.SvgImage = LoadSvgFromString(SvgChucVu);
            if (btnNangLuong != null) btnNangLuong.ImageOptions.SvgImage = LoadSvgFromString(SvgNangLuong);
            if (btnBCCT_NV != null) btnBCCT_NV.ImageOptions.SvgImage = LoadSvgFromString(SvgBCCT);
            if (btnBaoCao != null) btnBaoCao.ImageOptions.SvgImage = LoadSvgFromString(SvgBaoCao);
            if (btnGroup != null) btnGroup.ImageOptions.SvgImage = LoadSvgFromString(SvgGroup);
            if (btnUser != null) btnUser.ImageOptions.SvgImage = LoadSvgFromString(SvgUser);
            if (btnUser_Update != null) btnUser_Update.ImageOptions.SvgImage = LoadSvgFromString(SvgUserUpdate);
            if (btnChucNang != null) btnChucNang.ImageOptions.SvgImage = LoadSvgFromString(SvgChucNang);
            if (btnPQ_BaoCao != null) btnPQ_BaoCao.ImageOptions.SvgImage = LoadSvgFromString(SvgPQBaoCao);
            if (BtnAI != null) BtnAI.ImageOptions.SvgImage = LoadSvgFromString(SvgAI);
            if (btnLogin != null) btnLogin.ImageOptions.SvgImage = LoadSvgFromString(SvgLogin);
            if (btnSetting != null) btnSetting.ImageOptions.SvgImage = LoadSvgFromString(SvgSetting);
            if (btnDashboardNhanSu != null) btnDashboardNhanSu.ImageOptions.SvgImage = LoadSvgFromString(SvgDashboardNhanSu);
            if (btnDashboardLuong != null) btnDashboardLuong.ImageOptions.SvgImage = LoadSvgFromString(SvgDashboardLuong);
            if (btnThongBao != null) btnThongBao.ImageOptions.SvgImage = LoadSvgFromString(SvgThongBao);
            if (barButtonItem1 != null) barButtonItem1.ImageOptions.SvgImage = LoadSvgFromString(SvgAdd);
            if (barButtonItem4 != null) barButtonItem4.ImageOptions.SvgImage = LoadSvgFromString(SvgRefresh);

            // Redundant Exit / Quit Buttons (Apply rose shutdown symbol)
            var exitSvg = LoadSvgFromString(SvgExit);
            if (BtnExit != null) BtnExit.ImageOptions.SvgImage = exitSvg;
            if (btnExit1 != null) btnExit1.ImageOptions.SvgImage = exitSvg;
            if (btnExit2 != null) btnExit2.ImageOptions.SvgImage = exitSvg;
            if (btnExit3 != null) btnExit3.ImageOptions.SvgImage = exitSvg;
            if (btnThoat2 != null) btnThoat2.ImageOptions.SvgImage = exitSvg;
        }

        #region SVG XML Constants

        private const string SvgPass = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='6' y='12' width='20' height='14' rx='3' stroke='#6366f1' stroke-width='2'/>
  <path d='M11 12V8C11 5.23858 13.2386 3 16 3C18.7614 3 21 5.23858 21 8V12' stroke='#6366f1' stroke-width='2'/>
  <circle cx='16' cy='18' r='2' fill='#6366f1'/>
  <path d='M16 20V23' stroke='#6366f1' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgSaoLuu = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <ellipse cx='16' cy='7' rx='10' ry='4' stroke='#10b981' stroke-width='2'/>
  <path d='M6 7V15C6 17.2 10.5 19 16 19C21.5 19 26 17.2 26 15V7' stroke='#10b981' stroke-width='2'/>
  <path d='M6 15V23C6 25.2 10.5 27 16 27C21.5 27 26 25.2 26 23V15' stroke='#10b981' stroke-width='2'/>
  <path d='M16 11V18M16 18L13 15M16 18L19 15' stroke='#6366f1' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgPhucHoi = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <ellipse cx='16' cy='7' rx='10' ry='4' stroke='#10b981' stroke-width='2'/>
  <path d='M6 7V15C6 17.2 10.5 19 16 19C21.5 19 26 17.2 26 15V7' stroke='#10b981' stroke-width='2'/>
  <path d='M6 15V23C6 25.2 10.5 27 16 27C21.5 27 26 25.2 26 23V15' stroke='#10b981' stroke-width='2'/>
  <path d='M16 18V11M16 11L13 14M16 11L19 14' stroke='#6366f1' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgDanToc = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='16' r='11' stroke='#64748b' stroke-width='2'/>
  <path d='M5 16H27' stroke='#64748b' stroke-width='1.5'/>
  <path d='M16 5V27' stroke='#64748b' stroke-width='1.5'/>
  <path d='M8.5 7.5C12.5 10 12.5 22 8.5 24.5' stroke='#64748b' stroke-width='1.5'/>
  <path d='M23.5 7.5C19.5 10 19.5 22 23.5 24.5' stroke='#64748b' stroke-width='1.5'/>
</svg>";

        private const string SvgTonGiao = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M16 4C16 4 11 9 11 13C11 15.7614 13.2386 18 16 18C18.7614 18 21 15.7614 21 13C21 9 16 4 16 4Z' fill='#f59e0b'/>
  <path d='M8 26C8 21 12 20 16 20C20 20 24 21 24 26' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <path d='M16 20V28' stroke='#64748b' stroke-width='1.5'/>
</svg>";

        private const string SvgTrinhDo = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M16 5L4 11L16 17L28 11L16 5Z' stroke='#64748b' stroke-width='2' stroke-linejoin='round' fill='#cbd5e1'/>
  <path d='M8 15V22C8 23.5 11.5 25 16 25C20.5 25 24 23.5 24 22V15' stroke='#64748b' stroke-width='2' stroke-linejoin='round'/>
  <path d='M28 11V20' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <circle cx='28' cy='21' r='1.5' fill='#64748b'/>
</svg>";

        private const string SvgPhongBan = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='5' y='5' width='22' height='22' rx='2' stroke='#64748b' stroke-width='2'/>
  <path d='M12 5V27' stroke='#64748b' stroke-width='2'/>
  <path d='M20 5V27' stroke='#64748b' stroke-width='2'/>
  <path d='M5 12H27' stroke='#64748b' stroke-width='2'/>
  <path d='M5 20H27' stroke='#64748b' stroke-width='2'/>
</svg>";

        private const string SvgNhanVien = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='4' y='6' width='24' height='20' rx='3' stroke='#f59e0b' stroke-width='2'/>
  <circle cx='10' cy='13' r='3' stroke='#f59e0b' stroke-width='2'/>
  <path d='M5 21C5 18.5 7 17.5 10 17.5C13 17.5 15 18.5 15 21' stroke='#f59e0b' stroke-width='2' stroke-linecap='round'/>
  <line x1='18' y1='11' x2='25' y2='11' stroke='#f59e0b' stroke-width='2' stroke-linecap='round'/>
  <line x1='18' y1='15' x2='25' y2='15' stroke='#f59e0b' stroke-width='2' stroke-linecap='round'/>
  <line x1='18' y1='19' x2='23' y2='19' stroke='#f59e0b' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgHopDong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M6 4H20L26 10V26C26 27.1046 25.1046 28 24 28H6C4.89543 28 4 27.1046 4 26V6C4 4.89543 4.89543 4 6 4Z' stroke='#f59e0b' stroke-width='2'/>
  <path d='M20 4V10H26' stroke='#f59e0b' stroke-width='2'/>
  <line x1='8' y1='14' x2='22' y2='14' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <line x1='8' y1='18' x2='22' y2='18' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <path d='M9 23L12 25L18 21' stroke='#10b981' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgKhenThuong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M6 7H26V14C26 19.5 21.5 24 16 24C10.5 24 6 19.5 6 14V7Z' stroke='#eab308' stroke-width='2' fill='#fef08a'/>
  <path d='M6 10H3C1.89543 10 1 10.8954 1 12C1 14.5 2.5 16 5 16H6' stroke='#eab308' stroke-width='2'/>
  <path d='M26 10H29C30.1046 10 31 10.8954 31 12C31 14.5 29.5 16 27 16H26' stroke='#eab308' stroke-width='2'/>
  <path d='M16 24V28' stroke='#eab308' stroke-width='2' stroke-linecap='round'/>
  <path d='M10 28H22' stroke='#eab308' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgKyLuat = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M16 4L28 10V18C28 23.5 23 27 16 29C9 27 4 23.5 4 18V10L16 4Z' stroke='#f43f5e' stroke-width='2' fill='#ffe4e6'/>
  <line x1='16' y1='10' x2='16' y2='18' stroke='#f43f5e' stroke-width='2' stroke-linecap='round'/>
  <circle cx='16' cy='22' r='1.5' fill='#f43f5e'/>
</svg>";

        private const string SvgDieuChuyen = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M5 10H23M23 10L18 5M23 10L18 15' stroke='#f59e0b' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
  <path d='M27 22H9M9 22L14 17M9 22L14 27' stroke='#64748b' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgThoiViec = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M18 7H6C4.89543 7 4 7.89543 4 9V25C4 26.1046 4.89543 27 6 27H18' stroke='#f43f5e' stroke-width='2' stroke-linecap='round'/>
  <path d='M13 17H28M28 17L23 12M28 17L23 22' stroke='#f43f5e' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
  <circle cx='10' cy='12' r='2' fill='#64748b'/>
</svg>";

        private const string SvgLoaiCa = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='16' r='10' stroke='#3b82f6' stroke-width='2'/>
  <path d='M16 6V26C10.4772 26 6 21.5228 6 16C6 10.4772 10.4772 6 16 6Z' fill='#3b82f6'/>
  <path d='M16 6C21.5228 6 26 10.4772 26 16C26 21.5228 21.5228 26 16 26' stroke='#f59e0b' stroke-width='2'/>
  <line x1='16' y1='2' x2='16' y2='4' stroke='#f59e0b' stroke-width='2' stroke-linecap='round'/>
  <line x1='16' y1='28' x2='16' y2='30' stroke='#3b82f6' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgLoaiCong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='12' cy='13' r='5' stroke='#64748b' stroke-width='2'/>
  <circle cx='12' cy='13' r='1.5' fill='#64748b'/>
  <path d='M12 7V8M12 18V19M6 13H7M17 13H18M8 9L9 10M15 16L16 17M15 9L14 10M8 16L7 17' stroke='#64748b' stroke-width='1.5'/>
  <circle cx='21' cy='21' r='4' stroke='#475569' stroke-width='2'/>
  <circle cx='21' cy='21' r='1' fill='#475569'/>
</svg>";

        private const string SvgPhuCap = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <ellipse cx='16' cy='10' rx='9' ry='3' stroke='#3b82f6' stroke-width='2' fill='#93c5fd'/>
  <path d='M7 10V16C7 17.65 11 19 16 19C21 19 25 17.65 25 16V10' stroke='#3b82f6' stroke-width='2'/>
  <path d='M7 16V22C7 23.65 11 25 16 25C21 25 25 23.65 25 22V16' stroke='#3b82f6' stroke-width='2'/>
</svg>";

        private const string SvgTangCa = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='14' cy='16' r='10' stroke='#3b82f6' stroke-width='2'/>
  <path d='M14 10V16H20' stroke='#3b82f6' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
  <path d='M23 11L27 15L23 19' stroke='#f59e0b' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgUngLuong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M3 23H13C14.5 23 17 21 18 20L27 16' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <rect x='14' y='6' width='14' height='8' rx='1' stroke='#10b981' stroke-width='2' fill='#a7f3d0'/>
  <circle cx='21' cy='10' r='1.5' fill='#10b981'/>
</svg>";

        private const string SvgBangCong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='4' y='6' width='24' height='22' rx='3' stroke='#0d9488' stroke-width='2'/>
  <line x1='4' y1='12' x2='28' y2='12' stroke='#0d9488' stroke-width='2'/>
  <line x1='10' y1='6' x2='10' y2='12' stroke='#0d9488' stroke-width='2'/>
  <line x1='22' y1='6' x2='22' y2='12' stroke='#0d9488' stroke-width='2'/>
  <circle cx='9' cy='17' r='1.5' fill='#0d9488'/>
  <circle cx='16' cy='17' r='1.5' fill='#0d9488'/>
  <circle cx='23' cy='17' r='1.5' fill='#0d9488'/>
  <circle cx='9' cy='23' r='1.5' fill='#0d9488'/>
  <circle cx='16' cy='23' r='1.5' fill='#0d9488'/>
</svg>";

        private const string SvgBangLuong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='4' y='4' width='14' height='24' rx='2' stroke='#0d9488' stroke-width='2'/>
  <rect x='7' y='7' width='8' height='5' stroke='#0d9488' stroke-width='2'/>
  <circle cx='8' cy='16' r='1' fill='#0d9488'/>
  <circle cx='12' cy='16' r='1' fill='#0d9488'/>
  <circle cx='8' cy='20' r='1' fill='#0d9488'/>
  <circle cx='12' cy='20' r='1' fill='#0d9488'/>
  <circle cx='8' cy='24' r='1' fill='#0d9488'/>
  <circle cx='12' cy='24' r='1' fill='#0d9488'/>
  <path d='M22 10H28M22 15H28M22 20H26' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgCongTy = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='6' y='4' width='20' height='24' rx='2' stroke='#64748b' stroke-width='2' fill='#e2e8f0'/>
  <rect x='10' y='8' width='4' height='4' stroke='#64748b' stroke-width='1.5'/>
  <rect x='18' y='8' width='4' height='4' stroke='#64748b' stroke-width='1.5'/>
  <rect x='10' y='15' width='4' height='4' stroke='#64748b' stroke-width='1.5'/>
  <rect x='18' y='15' width='4' height='4' stroke='#64748b' stroke-width='1.5'/>
  <rect x='13' y='22' width='6' height='6' stroke='#64748b' stroke-width='2' fill='#cbd5e1'/>
</svg>";

        private const string SvgBoPhan = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='11' y='4' width='10' height='6' rx='1' stroke='#64748b' stroke-width='2'/>
  <rect x='4' y='20' width='10' height='6' rx='1' stroke='#64748b' stroke-width='2'/>
  <rect x='18' y='20' width='10' height='6' rx='1' stroke='#64748b' stroke-width='2'/>
  <path d='M16 10V15M16 15H9V20M16 15H23V20' stroke='#64748b' stroke-width='2'/>
</svg>";

        private const string SvgChucVu = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='13' r='8' stroke='#64748b' stroke-width='2' fill='#cbd5e1'/>
  <path d='M12 20L9 29L16 26L23 29L20 20' stroke='#64748b' stroke-width='2' stroke-linejoin='round' fill='#e2e8f0'/>
  <path d='M16 9L17.5 12H21L18 14L19.5 17L16 15L12.5 17L14 14L11 12H14.5L16 9Z' fill='#f59e0b'/>
</svg>";

        private const string SvgNangLuong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M4 27H28' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <path d='M8 22H11V27H8V22Z' fill='#a7f3d0' stroke='#10b981' stroke-width='2'/>
  <path d='M14 16H17V27H14V16Z' fill='#6ee7b7' stroke='#10b981' stroke-width='2'/>
  <path d='M20 10H23V27H20V10Z' fill='#34d399' stroke='#10b981' stroke-width='2'/>
  <path d='M7 17L13 11L19 5L26 2M26 2H21M26 2V7' stroke='#3b82f6' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgBCCT = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M6 4H18L24 10V26C24 27.1046 23.1046 28 22 28H6C4.89543 28 4 27.1046 4 26V6C4 4.89543 4.89543 4 6 4Z' stroke='#0d9488' stroke-width='2'/>
  <line x1='8' y1='12' x2='16' y2='12' stroke='#64748b' stroke-width='2'/>
  <line x1='8' y1='17' x2='14' y2='17' stroke='#64748b' stroke-width='2'/>
  <circle cx='21' cy='21' r='4' stroke='#0d9488' stroke-width='2' fill='#ccfbf1'/>
  <line x1='24' y1='24' x2='28' y2='28' stroke='#0d9488' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgBaoCao = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M6 4H18L24 10V26C24 27.1046 23.1046 28 22 28H6C4.89543 28 4 27.1046 4 26V6C4 4.89543 4.89543 4 6 4Z' stroke='#0d9488' stroke-width='2'/>
  <path d='M14 18C14 15 16.5 13.5 19 14.5C19 14.5 19 18 19 18H14Z' fill='#0d9488'/>
  <circle cx='15' cy='19' r='5' stroke='#0d9488' stroke-width='2'/>
</svg>";

        private const string SvgGroup = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='11' cy='11' r='4' stroke='#6366f1' stroke-width='2'/>
  <path d='M4 21.5C4 18.5 7 18 11 18C15 18 18 18.5 18 21.5' stroke='#6366f1' stroke-width='2' stroke-linecap='round'/>
  <circle cx='21' cy='14' r='3' stroke='#64748b' stroke-width='2'/>
  <path d='M17 23.5C17 21 19 20.5 22 20.5C25 20.5 27 21 27 23.5' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgUser = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='11' r='6' stroke='#6366f1' stroke-width='2' fill='#c7d2fe'/>
  <path d='M6 25C6 20 10 19 16 19C22 19 26 20 26 25' stroke='#6366f1' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgUserUpdate = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='12' cy='11' r='5' stroke='#6366f1' stroke-width='2'/>
  <path d='M4 23C4 19.5 7.5 19 12 19' stroke='#6366f1' stroke-width='2' stroke-linecap='round'/>
  <path d='M19 23L27 15L24 12L16 20V23H19Z' stroke='#f59e0b' stroke-width='2' stroke-linejoin='round' fill='#fef3c7'/>
</svg>";

        private const string SvgChucNang = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='5' y='8' width='22' height='18' rx='2' stroke='#6366f1' stroke-width='2'/>
  <path d='M10 14H22M10 20H18' stroke='#64748b' stroke-width='2' stroke-linecap='round'/>
  <circle cx='23' cy='20' r='3.5' stroke='#eab308' stroke-width='2' fill='#fef08a'/>
  <line x1='25' y1='22.5' x2='28' y2='25.5' stroke='#eab308' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgPQBaoCao = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M6 4H18L24 10V26C24 27.1046 23.1046 28 22 28H6C4.89543 28 4 27.1046 4 26V6C4 4.89543 4.89543 4 6 4Z' stroke='#6366f1' stroke-width='2'/>
  <path d='M11 12C11 12 11 19 15 21C19 19 19 12 19 12' stroke='#3b82f6' stroke-width='2'/>
</svg>";

        private const string SvgAI = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M16 4L27 10V22L16 28L5 22V10L16 4Z' stroke='#10b981' stroke-width='2'/>
  <circle cx='16' cy='16' r='3' stroke='#10b981' stroke-width='2' fill='#a7f3d0'/>
  <circle cx='16' cy='7' r='2' fill='#10b981'/>
  <circle cx='24' cy='12' r='2' fill='#10b981'/>
  <circle cx='24' cy='20' r='2' fill='#10b981'/>
  <circle cx='16' cy='25' r='2' fill='#10b981'/>
  <circle cx='8' cy='20' r='2' fill='#10b981'/>
  <circle cx='8' cy='12' r='2' fill='#10b981'/>
  <line x1='16' y1='9' x2='16' y2='13' stroke='#10b981' stroke-width='1.5'/>
  <line x1='22' y1='13.5' x2='18' y2='15' stroke='#10b981' stroke-width='1.5'/>
  <line x1='22' y1='18.5' x2='18' y2='17' stroke='#10b981' stroke-width='1.5'/>
  <line x1='16' y1='23' x2='16' y2='19' stroke='#10b981' stroke-width='1.5'/>
  <line x1='10' y1='18.5' x2='14' y2='17' stroke='#10b981' stroke-width='1.5'/>
  <line x1='10' y1='13.5' x2='14' y2='15' stroke='#10b981' stroke-width='1.5'/>
</svg>";

        private const string SvgLogin = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M12 7H24C25.1046 7 26 7.89543 26 9V23C26 24.1046 25.1046 25 24 25H12' stroke='#6366f1' stroke-width='2' stroke-linecap='round'/>
  <path d='M20 16H4M4 16L9 11M4 16L9 21' stroke='#6366f1' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgSetting = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='16' r='5' stroke='#475569' stroke-width='3'/>
  <path d='M16 8V5M16 27V24M8 16H5M27 16H24M10.3 10.3L8.2 8.2M23.8 23.8L21.7 21.7M21.7 10.3L23.8 8.2M8.2 23.8L10.3 21.7' stroke='#475569' stroke-width='3' stroke-linecap='round'/>
</svg>";

        private const string SvgDashboardNhanSu = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='4' y='4' width='24' height='24' rx='2' stroke='#10b981' stroke-width='2'/>
  <line x1='4' y1='12' x2='28' y2='12' stroke='#10b981' stroke-width='2'/>
  <line x1='14' y1='12' x2='14' y2='28' stroke='#10b981' stroke-width='2'/>
  <rect x='7' y='15' width='4' height='6' fill='#a7f3d0'/>
  <rect x='17' y='15' width='8' height='9' stroke='#10b981' stroke-dasharray='2 2' stroke-width='1.5'/>
</svg>";

        private const string SvgDashboardLuong = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <rect x='4' y='4' width='24' height='24' rx='2' stroke='#10b981' stroke-width='2'/>
  <line x1='4' y1='12' x2='28' y2='12' stroke='#10b981' stroke-width='2'/>
  <line x1='14' y1='12' x2='14' y2='28' stroke='#10b981' stroke-width='2'/>
  <path d='M7 23C7 23 10 17 12 18C14 19 18 14 20 15L25 9' stroke='#10b981' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgThongBao = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M16 4C12.5 4 10 7 10 11V18L7 22H25L22 18V11C22 7 19.5 4 16 4Z' stroke='#475569' stroke-width='2' stroke-linejoin='round'/>
  <path d='M13 25C13 26.65 14.35 28 16 28C17.65 28 19 26.65 19 25' stroke='#475569' stroke-width='2' stroke-linecap='round'/>
</svg>";

        private const string SvgAdd = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='16' r='11' stroke='#10b981' stroke-width='2' fill='#a7f3d0'/>
  <line x1='16' y1='10' x2='16' y2='22' stroke='#10b981' stroke-width='2.5' stroke-linecap='round'/>
  <line x1='10' y1='16' x2='22' y2='16' stroke='#10b981' stroke-width='2.5' stroke-linecap='round'/>
</svg>";

        private const string SvgRefresh = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <path d='M25 12C23.5 8.5 20 6 16 6C10.5 6 6 10.5 6 16C6 21.5 10.5 26 16 26C20.5 26 24.5 22.5 25.5 18' stroke='#3b82f6' stroke-width='2' stroke-linecap='round'/>
  <path d='M26 8V13H21' stroke='#3b82f6' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
</svg>";

        private const string SvgExit = @"<svg viewBox='0 0 32 32' fill='none' xmlns='http://www.w3.org/2000/svg'>
  <circle cx='16' cy='16' r='10' stroke='#f43f5e' stroke-width='2.5'/>
  <path d='M16 6V16' stroke='#f43f5e' stroke-width='3' stroke-linecap='round'/>
</svg>";

        #endregion
    }
}
