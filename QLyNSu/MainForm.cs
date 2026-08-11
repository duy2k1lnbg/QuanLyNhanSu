using Bu;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DevExpress.XtraSplashScreen;
using QLyNSu.FORM_BAOCAO;
using QLyNSu.FORM_CHAMCONG;
using QLyNSu.FORM_SYSTEM;
using QLyNSu.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace QLyNSu
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private FormManager_Functions _formManager;
        public MainForm()
        {
            InitializeComponent();
            _formManager = new FormManager_Functions(this); // Khởi tạo FormManager
            // InitializeNewSystemIcons(); // Vô hiệu hóa tính năng này để giao diện VS và Runtime giống hệt nhau
            
            this.FormClosing += MainForm_FormClosing;
        }

        private async void btnLoginDashboard_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FORM_SYSTEM.FrmUserDashboard));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserSession.CurrentLoginId > 0)
            {
                try
                {
                    using (var db = new DA.MyEntities())
                    {
                        db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_LOGIN_HISTORY SET THOIGIAN_DANGXUAT = CURRENT_TIMESTAMP WHERE ID_LOGIN = :p0", 
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", UserSession.CurrentLoginId));
                    }
                }
                catch { } // Ignore on closing
            }
        }

        private NHANVIEN _nhanvien;
        private HOPDONGLAODONG _hopdong;
        #region Region_OpenForm
        //private SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        //private void OpenForm(Type typeForm)
        //{
        //    foreach (var frm in MdiChildren)
        //        if (frm.GetType() == typeForm)
        //        {
        //            frm.Activate();
        //            return;
        //        }
        //    Form f = (Form)Activator.CreateInstance(typeForm);
        //    f.MdiParent = this;
        //    f.Show();
        //}

        //private async Task OpenFormAsync(Type typeForm)
        //{
        //    foreach (var frm in MdiChildren)
        //    {
        //        if (frm.GetType() == typeForm)
        //        {
        //            frm.Activate();
        //            return;
        //        }
        //    }

        //    // Khởi tạo form bất đồng bộ
        //    await Task.Run(() =>
        //    {
        //        Form f = (Form)Activator.CreateInstance(typeForm);
        //        this.Invoke((MethodInvoker)delegate
        //        {
        //            f.MdiParent = this;
        //            f.Show();
        //        });
        //    });
        //}

        //private async Task OpenFormWithSplashScreen(Type typeForm)
        //{
        //    // Hiển thị SplashScreen khi đang mở form
        //    SplashScreenManager.ShowForm(this, typeof(FrmWaiting), true, true, false);

        //    try
        //    {
        //        await OpenFormWithSemaphore(typeForm);
        //    }
        //    finally
        //    {
        //        // Đóng SplashScreen sau khi form đã mở xong
        //        SplashScreenManager.CloseForm();
        //    }
        //}


        //private async Task OpenFormWithSemaphore(Type typeForm)
        //{
        //    if (!_semaphore.Wait(0))
        //    {
        //        MessageBox.Show("Hệ thống đang bận, vui lòng chờ một chút.");
        //        return;
        //    }

        //    try
        //    {
        //        await OpenFormAsync(typeForm);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Có lỗi xảy ra: {ex.Message}");
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}

        //private void CloseAll()
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.Invoke(new MethodInvoker(CloseAll));
        //    }
        //    else
        //    {
        //        // Hiển thị hộp thoại xác nhận trước khi thoát
        //        DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButtons.YesNo);
        //        if (result == DialogResult.Yes)
        //        {
        //            // Kết thúc toàn bộ ứng dụng
        //            Application.Exit();
        //        }
        //    }
        //}
        #endregion

        private async void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmDanToc));
        }

        private async void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmTonGiao));
        }

        private async void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmTrinhDo));
        }

        private async void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
           await _formManager.OpenFormWithSplashScreen(typeof(FrmDieuChuyen_NhanVien));
        }

        private async void barButtonItem15_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmPhuCap));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Ensure default users and rights are seeded
                new SYS_USER().EnsureSeeded();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo dữ liệu hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            TranslationManager.LoadLanguage();
            TranslationManager.Translate(this);
            // Avoid overriding custom settings/notification icons

            _ = AiBootstrap.EnsureOllama();
            _nhanvien = new NHANVIEN();
            _hopdong = new HOPDONGLAODONG();
            ribbonControl1.SelectedPage = ribbonPage1;
            loadMainFormThongBao();

            // Apply default authorization (lock ui elements) - deferred to avoid Ribbon initialization overwriting it
            this.BeginInvoke(new Action(() => ApplyAuthorization()));
        }

        private void ShowLoginDialog()
        {
            this.Hide();
            using (var loginForm = new FrmDangNhap())
            {
                TranslationManager.Translate(loginForm);
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                    ApplyAuthorization();
                }
                else
                {
                    if (UserSession.CurrentUser == null)
                    {
                        this.Show();
                        ApplyAuthorization();
                    }
                }
            }
        }

        private void ApplyAuthorization()
        {
            if (UserSession.CurrentUser == null)
            {
                // Disconnected state
                btnLogin.Caption = TranslationManager.Translate("Đăng Nhập");
                
                // Disable all controls except login and exit
                btnPass.Enabled = false;
                btnSaoLuu_DB.Enabled = false;
                btnPhucHoi_DB.Enabled = false;
                btnDanToc.Enabled = false;
                btnTonGiao.Enabled = false;
                btnTrinhDo.Enabled = false;
                btnPhongBan.Enabled = false;
                btnNhanVien.Enabled = false;
                btnHopDong.Enabled = false;
                btnKhenThuong.Enabled = false;
                btnDieuChuyen.Enabled = false;
                btnThoiViec.Enabled = false;
                btnLoaiCa.Enabled = false;
                btnLoaiCong.Enabled = false;
                btnPhuCap.Enabled = false;
                btnTangCa.Enabled = false;
                btnUngLuong.Enabled = false;
                btnBangCong.Enabled = false;
                btnBangLuong.Enabled = false;
                btnCongTy.Enabled = false;
                btnBoPhan.Enabled = false;
                btnChucVu.Enabled = false;
                btnKyLuat.Enabled = false;
                btnGiamSat.Enabled = false;
                btnNangLuong.Enabled = false;
                btnBCCT_NV.Enabled = false;
                btnBaoCao.Enabled = false;
                btnGroup.Enabled = false;
                btnUser.Enabled = false;
                btnUser_Update.Enabled = false;
                btnChucNang.Enabled = false;
                btnPQ_BaoCao.Enabled = false;
                BtnAI.Enabled = false;
                
                btnSetting.Enabled = false;
                btnDashboardNhanSu.Enabled = false;
                btnDashboardLuong.Enabled = false;
                
                btnThongBao.Enabled = false;
            }
            else
            {
                // Connected state
                btnLogin.Caption = TranslationManager.Translate("Đăng Xuất") + " (" + UserSession.CurrentUser.FULLNAME + ")";
                if (ribbonControl1 != null) ribbonControl1.Refresh();
                
                // Check rights
                btnPass.Enabled = true;
                btnUser_Update.Enabled = true;
                
                btnGroup.Enabled = UserSession.HasRight("F_SYSTEM_GROUP");
                btnUser.Enabled = UserSession.HasRight("F_SYSTEM_USER");
                btnSaoLuu_DB.Enabled = UserSession.HasRight("F_SYSTEM_SAULUU");
                btnPhucHoi_DB.Enabled = UserSession.HasRight("F_SYSTEM_PHUCHOI");
                BtnAI.Enabled = UserSession.HasRight("F_SYSTEM_AI");
                
                btnSetting.Enabled = UserSession.HasRight("F_SYSTEM_SETTING");
                btnDashboardNhanSu.Enabled = UserSession.HasRight("F_DB_NHANSU");
                btnDashboardLuong.Enabled = UserSession.HasRight("F_DB_LUONG");
                btnGiamSat.Enabled = UserSession.HasRight("F_SYSTEM_GIAMSAT");
                
                btnDanToc.Enabled = UserSession.HasRight("F_DM_DANTOC");
                btnTonGiao.Enabled = UserSession.HasRight("F_DM_TONGIAO");
                btnTrinhDo.Enabled = UserSession.HasRight("F_DM_TRINHDO");
                btnNhanVien.Enabled = UserSession.HasRight("F_DM_NHANVIEN");
                btnPhongBan.Enabled = UserSession.HasRight("F_DM_PHONGBAN");
                btnBoPhan.Enabled = UserSession.HasRight("F_DM_BOPHAN");
                btnCongTy.Enabled = UserSession.HasRight("F_DM_CONGTY");
                btnChucVu.Enabled = UserSession.HasRight("F_DM_CHUCVU");
                
                btnHopDong.Enabled = UserSession.HasRight("F_NV_HOPDONG");
                btnNangLuong.Enabled = UserSession.HasRight("F_NV_NANGLUONG");
                btnKhenThuong.Enabled = UserSession.HasRight("F_NV_KHENTHUONG");
                btnKyLuat.Enabled = UserSession.HasRight("F_NV_KYLUAT");
                btnDieuChuyen.Enabled = UserSession.HasRight("F_NV_DIEUCHUYEN");
                btnThoiViec.Enabled = UserSession.HasRight("F_NV_THOIVIEC");
                
                btnLoaiCa.Enabled = UserSession.HasRight("F_CC_LOAICA");
                btnLoaiCong.Enabled = UserSession.HasRight("F_CC_LOAICONG");
                btnPhuCap.Enabled = UserSession.HasRight("F_CC_PHUCAP");
                btnTangCa.Enabled = UserSession.HasRight("F_CC_TANGCA");
                btnUngLuong.Enabled = UserSession.HasRight("F_CC_UNGLUONG");
                btnBangCong.Enabled = UserSession.HasRight("F_CC_BANGCONG");
                btnBCCT_NV.Enabled = UserSession.HasRight("F_CC_BCCT");
                btnBangLuong.Enabled = UserSession.HasRight("F_CC_BANGLUONG");
                
                btnBaoCao.Enabled = UserSession.HasRight("F_BC_BAOCAO");
                
                btnChucNang.Enabled = UserSession.CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);
                btnPQ_BaoCao.Enabled = UserSession.CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);
                
                btnThongBao.Enabled = true;
            }
            loadMainFormThongBao();
        }

        private async void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmPhongBan));
        }

        private async void barButtonItem5_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmBoPhan));
        }

        private async void barButtonItem4_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmCongTy));
        }

        private async void barButtonItem21_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmChucVu));
        }

        private async void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmNhanVien));
        }

        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _formManager.OpenForm(typeof(FrmHopDongLaoDong));

        }

        private void barButtonItem20_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _formManager.CloseAll();
        }

        private void barButtonItem22_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _formManager.CloseAll();
        }

        private async void btnKhenThuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmKhenThuong));
        }

        private async void btnKyLuat_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmKyLuat));
        }

        private async void btnThoiViec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
             await _formManager.OpenFormWithSplashScreen(typeof(FrmNhanVien_ThoiViec));
        }

        private async void btnNangLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmNangLuong_NhanVien));
        }

        public void ActivateMdiChild(Form frm)
        {
            if (frm == null) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ActivateMdiChild(frm)));
                return;
            }
            documentManager1.View.ActivateDocument(frm);
        }

        private void loadMainFormThongBao()
        {
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
            try
            {
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - loadMainFormThongBao called. CurrentUser: {UserSession.CurrentUser?.USERNAME ?? "null"}\r\n");

                var allNotices = new THONGBAO().getListFull_DTO();
                System.IO.File.AppendAllText(logPath, $"  Total notices in DB: {allNotices.Count}\r\n");

                // Filter active notifications
                var activeNotices = allNotices.Where(x => 
                    x.TRANGTHAI == 1 && // Đã đăng
                    (x.NGAY_HETHAN == null || x.NGAY_HETHAN >= DateTime.Now) // Chưa hết hạn
                ).ToList();
                System.IO.File.AppendAllText(logPath, $"  Active notices (TRANGTHAI=1 and not expired): {activeNotices.Count}\r\n");

                // Filter by company/department if not admin
                if (UserSession.CurrentUser != null)
                {
                    if (!UserSession.CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        string userCty = UserSession.CurrentUser.MACTY;
                        string userPb = UserSession.CurrentUser.MADVI;
                        System.IO.File.AppendAllText(logPath, $"  Filtering for non-admin user. Company: '{userCty}', Dept/Div: '{userPb}'\r\n");

                        activeNotices = activeNotices.Where(x => 
                            (string.IsNullOrEmpty(x.MACTY) || x.MACTY == userCty) &&
                            (string.IsNullOrEmpty(x.MAPB) || x.MAPB == userPb)
                        ).ToList();
                    }
                }
                else
                {
                    // If not logged in, only show notifications with no target company/department (public)
                    System.IO.File.AppendAllText(logPath, "  Filtering for non-logged-in user (public only)\r\n");
                    activeNotices = activeNotices.Where(x => string.IsNullOrEmpty(x.MACTY) && string.IsNullOrEmpty(x.MAPB)).ToList();
                }

                System.IO.File.AppendAllText(logPath, $"  Final notices to bind: {activeNotices.Count}\r\n");

                lstThongBao.DataSource = null;
                lstThongBao.Items.Clear();
                foreach (var notice in activeNotices)
                {
                    lstThongBao.Items.Add(notice);
                }
                lstThongBao.DisplayMember = "DisplayText";
                lstThongBao.ValueMember = "ID";
                grThongKe.Text = TranslationManager.Translate("Thông Báo Mới");
                
                System.IO.File.AppendAllText(logPath, "  Successfully bound data source manually.\r\n");
            }
            catch (Exception ex)
            {
                lstThongBao.DataSource = null;
                grThongKe.Text = TranslationManager.Translate("Thông Báo Mới (Lỗi)");
                System.IO.File.AppendAllText(logPath, $"  ERROR: {ex}\r\n");
            }
        }

        private void lstThongBao_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lstThongBao.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                var item = lstThongBao.SelectedItem as THONGBAO_DTO;
                if (item != null)
                {
                    string message = $"Tiêu đề: {item.TIEUDE}\n" +
                                     $"Loại thông báo: {item.LOAI_TB}\n" +
                                     $"Người đăng: {item.NGUOIDANG} ({item.NGAYDANG:dd/MM/yyyy HH:mm})\n";
                    if (!string.IsNullOrEmpty(item.FILE_DINHKEM))
                    {
                        message += $"File đính kèm: {System.IO.Path.GetFileName(item.FILE_DINHKEM)}\n";
                    }
                    message += $"\nNội dung:\n{item.NOIDUNG}";

                    DevExpress.XtraEditors.XtraMessageBox.Show(message, "Chi Tiết Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }



        private async void btnLoaiCa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmLoaiCa));
        }

        private async void btnLoaiCong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmLoaiCong));
        }

        private void btnThoat2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _formManager.CloseAll();
        }

        private async void btnBangCong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmBangCong));
        }

        private void btnBCCT_NV_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //FrmBangCongNV_IN frm = new FrmBangCongNV_IN();
            //frm.ShowDialog();
            _formManager.OpenForm_NewTap(typeof(FrmBangCongNV_IN));
        }

        private async void btnTangCa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmTangCa));
        }

        private async void btnUngLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmUngLuong));
        }

        private async void btnBangLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmBangLuong));
        }

        private async void btnBaoCao_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmBaoCaoTongHop));
        }

        private async void btnDashboardNhanSu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmDashboardNhanSu));
        }

        private async void btnDashboardLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmDashboardLuong));
        }

        private async void btnThongBao_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmThongBao));
        }

        private void btnExit3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _formManager.CloseAll();
        }

        private void BtnExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _formManager.CloseAll();
        }

        private async void btnGroup_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmGroup));
        }

        private async void btnUser_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmCreateAccount));
        }

        private async void barButtonItem2_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmAI_Chat));
        }

        private void btnLogin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (UserSession.IsLoggedIn)
            {
                var result = MessageBox.Show(TranslationManager.Translate("Bạn có muốn đăng xuất không?"), TranslationManager.Translate("Xác nhận đăng xuất"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    foreach (Form child in this.MdiChildren)
                    {
                        child.Close();
                    }
                    UserSession.Clear();
                    ApplyAuthorization();

                    this.Hide();
                    using (var loginForm = new FrmDangNhap())
                    {
                        TranslationManager.Translate(loginForm);
                        if (loginForm.ShowDialog() == DialogResult.OK)
                        {
                            this.Show();
                            ApplyAuthorization();
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                }
            }
            else
            {
                this.Hide();
                using (var loginForm = new FrmDangNhap())
                {
                    TranslationManager.Translate(loginForm);
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        this.Show();
                        ApplyAuthorization();
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
        }

        private void btnPass_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var frm = new FrmChangePassword())
            {
                TranslationManager.Translate(frm);
                frm.ShowDialog();
            }
        }

        private void btnSetting_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var frm = new FrmSetting())
            {
                TranslationManager.Translate(frm);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    foreach (Form openForm in Application.OpenForms)
                    {
                        TranslationManager.Translate(openForm);
                    }
                    ApplyAuthorization(); // Re-apply dynamic captions that might have been overwritten by TranslationManager
                }
            }
        }

        private void btnUser_Update_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (UserSession.CurrentUser == null) return;
            using (var frm = new FrmUser(UserSession.CurrentUser))
            {
                TranslationManager.Translate(frm);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ApplyAuthorization();
                }
            }
        }

        private async void btnChucNang_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmPhanQuyenChucNang));
        }

        private async void btnPQ_BaoCao_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmPhanQuyenBaoCao));
        }

        private async void btnSaoLuu_DB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmDataExport));
        }
        private async void btnPhucHoi_DB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await _formManager.OpenFormWithSplashScreen(typeof(FrmDataImport));
        }
    }

    public class EFContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(p =>
                p.PropertyType.Namespace != "DA" &&
                (!typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[]))
            ).ToList();
        }
    }

    public class DbBackupData
    {
        public string Version { get; set; } = "1.0";
        public DateTime BackupTime { get; set; } = DateTime.Now;
        public List<DA.TB_CONFIG> TB_CONFIG { get; set; }
        public List<DA.TB_GIOITINH> TB_GIOITINH { get; set; }
        public List<DA.TB_QUOCTICH> TB_QUOCTICH { get; set; }
        public List<DA.TB_CONGTY> TB_CONGTY { get; set; }
        public List<DA.TB_BOPHAN> TB_BOPHAN { get; set; }
        public List<DA.TB_PHONGBAN> TB_PHONGBAN { get; set; }
        public List<DA.TB_CHUCVU> TB_CHUCVU { get; set; }
        public List<DA.TB_DANTOC> TB_DANTOC { get; set; }
        public List<DA.TB_TONGIAO> TB_TONGIAO { get; set; }
        public List<DA.TB_TRINHDO> TB_TRINHDO { get; set; }
        public List<DA.TB_LOAICA> TB_LOAICA { get; set; }
        public List<DA.TB_LOAICONG> TB_LOAICONG { get; set; }
        public List<DA.TB_PHUCAP> TB_PHUCAP { get; set; }
        public List<DA.TB_SYS_USER> TB_SYS_USER { get; set; }
        public List<DA.TB_SYS_FUNCTION> TB_SYS_FUNCTION { get; set; }
        public List<DA.TB_SYS_REPORT> TB_SYS_REPORT { get; set; }
        public List<DA.TB_NHANVIEN> TB_NHANVIEN { get; set; }
        public List<DA.TB_SYS_GROUP> TB_SYS_GROUP { get; set; }
        public List<DA.TB_SYS_RIGHT> TB_SYS_RIGHT { get; set; }
        public List<DA.TB_SYS_RIGHT_REPORT> TB_SYS_RIGHT_REPORT { get; set; }
        public List<DA.TB_HOPDONG> TB_HOPDONG { get; set; }
        public List<DA.TB_BAOHIEM> TB_BAOHIEM { get; set; }
        public List<DA.TB_NHANVIEN_PHUCAP> TB_NHANVIEN_PHUCAP { get; set; }
        public List<DA.TB_TANGCA> TB_TANGCA { get; set; }
        public List<DA.TB_UNGLUONG> TB_UNGLUONG { get; set; }
        public List<DA.TB_DIEUCHUYEN_NHANVIEN> TB_DIEUCHUYEN_NHANVIEN { get; set; }
        public List<DA.TB_KHENTHUONG_KYLUAT> TB_KHENTHUONG_KYLUAT { get; set; }
        public List<DA.TB_NHANVIEN_THOIVIEC> TB_NHANVIEN_THOIVIEC { get; set; }
        public List<DA.TB_NANGLUONG_NHANVIEN> TB_NANGLUONG_NHANVIEN { get; set; }
        public List<DA.TB_BANGCONG> TB_BANGCONG { get; set; }
        public List<DA.TB_BANGCONG_CHITIET> TB_BANGCONG_CHITIET { get; set; }
        public List<DA.TB_KYCONG> TB_KYCONG { get; set; }
        public List<DA.TB_KYCONGCHITIET> TB_KYCONGCHITIET { get; set; }
    }
}
