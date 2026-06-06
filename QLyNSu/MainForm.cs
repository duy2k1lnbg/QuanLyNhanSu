using Bu;
using Bu.CLASS_SYSTEM;
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

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

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

        private async void MainForm_Load(object sender, EventArgs e)
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
            try
            {
                btnSetting.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/icon builder/actions_settings.svg");
            }
            catch { }

            _ = AiBootstrap.EnsureOllama();
            _nhanvien = new NHANVIEN();
            _hopdong = new HOPDONGLAODONG();
            ribbonControl1.SelectedPage = ribbonPage1;
            await loadSinhNhat();
            await loadLenLuong();

            // Wire system menu events dynamically
            btnPass.ItemClick += btnPass_ItemClick;
            btnUser_Update.ItemClick += btnUser_Update_ItemClick;
            btnChucNang.ItemClick += btnChucNang_ItemClick;
            btnPQ_BaoCao.ItemClick += btnPQ_BaoCao_ItemClick;
            btnSaoLuu_DB.ItemClick += btnSaoLuu_DB_ItemClick;
            btnPhucHoi_DB.ItemClick += btnPhucHoi_DB_ItemClick;

            // Apply default authorization (lock ui elements)
            ApplyAuthorization();

            // Prompt user login immediately after main form displays
            this.BeginInvoke(new MethodInvoker(ShowLoginDialog));
        }

        private void ShowLoginDialog()
        {
            using (var loginForm = new FrmDangNhap())
            {
                TranslationManager.Translate(loginForm);
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    ApplyAuthorization();
                }
                else
                {
                    if (UserSession.CurrentUser == null)
                    {
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
                btnNangLuong.Enabled = false;
                btnBCCT_NV.Enabled = false;
                btnBaoCao.Enabled = false;
                btnGroup.Enabled = false;
                btnUser.Enabled = false;
                btnUser_Update.Enabled = false;
                btnChucNang.Enabled = false;
                btnPQ_BaoCao.Enabled = false;
                BtnAI.Enabled = false;
                
                lstSinhNhat.Enabled = false;
                lstNangLuong.Enabled = false;
            }
            else
            {
                // Connected state
                btnLogin.Caption = TranslationManager.Translate("Đăng Xuất") + " (" + UserSession.CurrentUser.FULLNAME + ")";
                
                // Check rights
                btnPass.Enabled = true;
                btnUser_Update.Enabled = true;
                
                btnGroup.Enabled = UserSession.HasRight("F_SYSTEM_GROUP");
                btnUser.Enabled = UserSession.HasRight("F_SYSTEM_USER");
                btnSaoLuu_DB.Enabled = UserSession.HasRight("F_SYSTEM_SAULUU");
                btnPhucHoi_DB.Enabled = UserSession.HasRight("F_SYSTEM_PHUCHOI");
                BtnAI.Enabled = UserSession.HasRight("F_SYSTEM_AI");
                
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
                
                lstSinhNhat.Enabled = true;
                lstNangLuong.Enabled = true;
            }
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

        private async Task loadSinhNhat()
        {
            lstSinhNhat.DataSource = await Task.Run(() => _nhanvien.getSinhNhat());
            //lstSinhNhat.DataSource = _nhanvien.getSinhNhat();
            lstSinhNhat.DisplayMember = "HOTEN";
            lstSinhNhat.ValueMember = "MANV";
        }

        private async Task loadLenLuong()
        {
            //lstNangLuong.DataSource =  _hopdong.getLenLuong();
            lstNangLuong.DataSource = await Task.Run(() => _hopdong.getLenLuong());
            lstNangLuong.DisplayMember = "HOTEN";
            lstNangLuong.ValueMember = "MANV";
        }

        private void lstSinhNhat_CustomizeItem(object sender, DevExpress.XtraEditors.CustomizeTemplatedItemEventArgs e)
        {
            DateTime parsedDate;

            // Thử chuyển đổi chuỗi từ Elements[1].Text sang kiểu DateTime
            if (DateTime.TryParse(e.TemplatedItem.Elements[1].Text, out parsedDate))
            {
                // Định dạng lại chuỗi theo định dạng dd/MM/yyyy
                e.TemplatedItem.Elements[1].Text = parsedDate.ToString("dd/MM/yyyy");

                // Kiểm tra nếu ngày hiện tại và sinh nhật trùng nhau
                if (parsedDate.Day == DateTime.Now.Day && parsedDate.Month == DateTime.Now.Month)
                {
                    e.TemplatedItem.AppearanceItem.Normal.ForeColor = Color.DeepPink;
                }
            }
        }

        private void lstNangLuong_CustomizeItem(object sender, DevExpress.XtraEditors.CustomizeTemplatedItemEventArgs e)
        {
            if (e.TemplatedItem.Elements[1].Text.Substring(0,2) == DateTime.Now.Day.ToString())
            {
                e.TemplatedItem.AppearanceItem.Normal.ForeColor = Color.Blue;
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
            await _formManager.OpenFormWithSplashScreen(typeof(FrmBaoCaoChiTiet));
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
            await _formManager.OpenFormWithSplashScreen(typeof(FrmShowUser_Group));
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
                    UserSession.Clear();
                    ApplyAuthorization();
                    ShowLoginDialog();
                }
            }
            else
            {
                ShowLoginDialog();
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

        private void btnSaoLuu_DB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON Files (*.json)|*.json";
                sfd.FileName = "backup_hr_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
                sfd.Title = "Chọn nơi lưu tệp tin sao lưu dữ liệu";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SplashScreenManager.ShowForm(this, typeof(FrmWaiting), true, true, false);
                    try
                    {
                        using (var db = new DA.MyEntities())
                        {
                            db.Configuration.ProxyCreationEnabled = false;
                            db.Configuration.LazyLoadingEnabled = false;

                            var backupData = new DbBackupData
                            {
                                TB_CONFIG = db.TB_CONFIG.ToList(),
                                TB_GIOITINH = db.TB_GIOITINH.ToList(),
                                TB_QUOCTICH = db.TB_QUOCTICH.ToList(),
                                TB_CONGTY = db.TB_CONGTY.ToList(),
                                TB_BOPHAN = db.TB_BOPHAN.ToList(),
                                TB_PHONGBAN = db.TB_PHONGBAN.ToList(),
                                TB_CHUCVU = db.TB_CHUCVU.ToList(),
                                TB_DANTOC = db.TB_DANTOC.ToList(),
                                TB_TONGIAO = db.TB_TONGIAO.ToList(),
                                TB_TRINHDO = db.TB_TRINHDO.ToList(),
                                TB_LOAICA = db.TB_LOAICA.ToList(),
                                TB_LOAICONG = db.TB_LOAICONG.ToList(),
                                TB_PHUCAP = db.TB_PHUCAP.ToList(),
                                TB_SYS_USER = db.TB_SYS_USER.ToList(),
                                TB_SYS_FUNCTION = db.TB_SYS_FUNCTION.ToList(),
                                TB_SYS_REPORT = db.TB_SYS_REPORT.ToList(),
                                TB_NHANVIEN = db.TB_NHANVIEN.ToList(),
                                TB_SYS_GROUP = db.TB_SYS_GROUP.ToList(),
                                TB_SYS_RIGHT = db.TB_SYS_RIGHT.ToList(),
                                TB_SYS_RIGHT_REPORT = db.TB_SYS_RIGHT_REPORT.ToList(),
                                TB_HOPDONG = db.TB_HOPDONG.ToList(),
                                TB_BAOHIEM = db.TB_BAOHIEM.ToList(),
                                TB_NHANVIEN_PHUCAP = db.TB_NHANVIEN_PHUCAP.ToList(),
                                TB_TANGCA = db.TB_TANGCA.ToList(),
                                TB_UNGLUONG = db.TB_UNGLUONG.ToList(),
                                TB_DIEUCHUYEN_NHANVIEN = db.TB_DIEUCHUYEN_NHANVIEN.ToList(),
                                TB_KHENTHUONG_KYLUAT = db.TB_KHENTHUONG_KYLUAT.ToList(),
                                TB_NHANVIEN_THOIVIEC = db.TB_NHANVIEN_THOIVIEC.ToList(),
                                TB_NANGLUONG_NHANVIEN = db.TB_NANGLUONG_NHANVIEN.ToList(),
                                TB_BANGCONG = db.TB_BANGCONG.ToList(),
                                TB_BANGCONG_CHITIET = db.TB_BANGCONG_CHITIET.ToList(),
                                TB_KYCONG = db.TB_KYCONG.ToList(),
                                TB_KYCONGCHITIET = db.TB_KYCONGCHITIET.ToList()
                            };

                            var settings = new JsonSerializerSettings
                            {
                                ContractResolver = new EFContractResolver(),
                                Formatting = Formatting.Indented,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            };

                            string json = JsonConvert.SerializeObject(backupData, settings);
                            File.WriteAllText(sfd.FileName, json, Encoding.UTF8);
                        }

                        SplashScreenManager.CloseForm();
                        MessageBox.Show("Sao lưu dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        SplashScreenManager.CloseForm();
                        MessageBox.Show("Lỗi khi sao lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnPhucHoi_DB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var confirmResult = MessageBox.Show(
                "CẢNH BÁO CỰC KỲ QUAN TRỌNG:\n\nHành động này sẽ XÓA TOÀN BỘ dữ liệu hiện tại trong cơ sở dữ liệu và ghi đè bằng dữ liệu khôi phục.\n\nBạn có chắc chắn muốn tiếp tục không?",
                "Cảnh báo khôi phục dữ liệu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (confirmResult != DialogResult.Yes) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON Files (*.json)|*.json";
                ofd.Title = "Chọn tệp tin sao lưu dữ liệu để khôi phục";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SplashScreenManager.ShowForm(this, typeof(FrmWaiting), true, true, false);
                    try
                    {
                        string json = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                        var backupData = JsonConvert.DeserializeObject<DbBackupData>(json);

                        if (backupData == null)
                        {
                            throw new Exception("Tệp tin sao lưu không hợp lệ hoặc trống.");
                        }

                        using (var db = new DA.MyEntities())
                        {
                            using (var trans = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    // 1. Delete in child-to-parent order
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_KYCONGCHITIET");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_KYCONG");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGCONG_CHITIET");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGCONG");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_NANGLUONG_NHANVIEN");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_NHANVIEN_THOIVIEC");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_KHENTHUONG_KYLUAT");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_DIEUCHUYEN_NHANVIEN");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_UNGLUONG");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_TANGCA");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_NHANVIEN_PHUCAP");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_BAOHIEM");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_HOPDONG");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_SYS_RIGHT_REPORT");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_SYS_RIGHT");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_SYS_GROUP");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_NHANVIEN");

                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_SYS_USER");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_SYS_FUNCTION");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_SYS_REPORT");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_CONFIG");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_QUOCTICH");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_GIOITINH");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_PHUCAP");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_LOAICONG");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_LOAICA");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_TRINHDO");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_TONGIAO");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_DANTOC");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_CHUCVU");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_PHONGBAN");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_BOPHAN");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TB_CONGTY");

                                    // Save deletions
                                    db.SaveChanges();

                                    // 2. Insert in parent-to-child order
                                    RestoreTable(db, "TB_CONGTY", backupData.TB_CONGTY);
                                    RestoreTable(db, "TB_BOPHAN", backupData.TB_BOPHAN);
                                    RestoreTable(db, "TB_PHONGBAN", backupData.TB_PHONGBAN);
                                    RestoreTable(db, "TB_CHUCVU", backupData.TB_CHUCVU);
                                    RestoreTable(db, "TB_DANTOC", backupData.TB_DANTOC);
                                    RestoreTable(db, "TB_TONGIAO", backupData.TB_TONGIAO);
                                    RestoreTable(db, "TB_TRINHDO", backupData.TB_TRINHDO);
                                    RestoreTable(db, "TB_LOAICA", backupData.TB_LOAICA);
                                    RestoreTable(db, "TB_LOAICONG", backupData.TB_LOAICONG);
                                    RestoreTable(db, "TB_PHUCAP", backupData.TB_PHUCAP);
                                    RestoreTable(db, "TB_GIOITINH", backupData.TB_GIOITINH);
                                    RestoreTable(db, "TB_QUOCTICH", backupData.TB_QUOCTICH);
                                    RestoreTable(db, "TB_CONFIG", backupData.TB_CONFIG);
                                    RestoreTable(db, "TB_SYS_USER", backupData.TB_SYS_USER);
                                    RestoreTable(db, "TB_SYS_FUNCTION", backupData.TB_SYS_FUNCTION);
                                    RestoreTable(db, "TB_SYS_REPORT", backupData.TB_SYS_REPORT);
                                    RestoreTable(db, "TB_NHANVIEN", backupData.TB_NHANVIEN);
                                    RestoreTable(db, "TB_SYS_GROUP", backupData.TB_SYS_GROUP);
                                    RestoreTable(db, "TB_SYS_RIGHT", backupData.TB_SYS_RIGHT);
                                    RestoreTable(db, "TB_SYS_RIGHT_REPORT", backupData.TB_SYS_RIGHT_REPORT);
                                    RestoreTable(db, "TB_HOPDONG", backupData.TB_HOPDONG);
                                    RestoreTable(db, "TB_BAOHIEM", backupData.TB_BAOHIEM);
                                    RestoreTable(db, "TB_NHANVIEN_PHUCAP", backupData.TB_NHANVIEN_PHUCAP);
                                    RestoreTable(db, "TB_TANGCA", backupData.TB_TANGCA);
                                    RestoreTable(db, "TB_UNGLUONG", backupData.TB_UNGLUONG);
                                    RestoreTable(db, "TB_DIEUCHUYEN_NHANVIEN", backupData.TB_DIEUCHUYEN_NHANVIEN);
                                    RestoreTable(db, "TB_KHENTHUONG_KYLUAT", backupData.TB_KHENTHUONG_KYLUAT);
                                    RestoreTable(db, "TB_NHANVIEN_THOIVIEC", backupData.TB_NHANVIEN_THOIVIEC);
                                    RestoreTable(db, "TB_NANGLUONG_NHANVIEN", backupData.TB_NANGLUONG_NHANVIEN);
                                    RestoreTable(db, "TB_BANGCONG", backupData.TB_BANGCONG);
                                    RestoreTable(db, "TB_BANGCONG_CHITIET", backupData.TB_BANGCONG_CHITIET);
                                    RestoreTable(db, "TB_KYCONG", backupData.TB_KYCONG);
                                    RestoreTable(db, "TB_KYCONGCHITIET", backupData.TB_KYCONGCHITIET);

                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.Rollback();
                                    throw new Exception("Lỗi trong quá trình khôi phục giao dịch: " + ex.Message, ex);
                                }
                            }
                        }

                        SplashScreenManager.CloseForm();
                        MessageBox.Show("Phục hồi dữ liệu thành công! Ứng dụng sẽ tự động khởi động lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Application.Restart();
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        SplashScreenManager.CloseForm();
                        MessageBox.Show("Lỗi khi phục hồi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RestoreTable<T>(System.Data.Entity.DbContext db, string tableName, List<T> list) where T : class
        {
            if (list == null) return;
            foreach (var item in list)
            {
                InsertEntityDirectly(db, tableName, item);
            }
        }

        private void InsertEntityDirectly(System.Data.Entity.DbContext db, string tableName, object entity)
        {
            var type = entity.GetType();
            var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && 
                            p.PropertyType.Namespace != "DA" &&
                            (!typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[])))
                .ToList();

            var colNames = new List<string>();
            var paramNames = new List<string>();
            var parameters = new List<object>();

            foreach (var prop in props)
            {
                var val = prop.GetValue(entity);
                colNames.Add(prop.Name);
                paramNames.Add(":" + prop.Name);

                var paramVal = val ?? DBNull.Value;
                var param = new Oracle.ManagedDataAccess.Client.OracleParameter(prop.Name, paramVal);
                parameters.Add(param);
            }

            string sql = $"INSERT INTO {tableName} ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";
            db.Database.ExecuteSqlCommand(sql, parameters.ToArray());
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
