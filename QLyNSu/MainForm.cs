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
using Bu;
using DevExpress.XtraSplashScreen;
using QLyNSu.FORM_BAOCAO;
using QLyNSu.FORM_CHAMCONG;
using QLyNSu.FORM_SYSTEM;

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
            _nhanvien = new NHANVIEN();
            _hopdong = new HOPDONGLAODONG();
            ribbonControl1.SelectedPage = ribbonPage3;
            await loadSinhNhat();
            await loadLenLuong();
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
            await _formManager.OpenFormWithSplashScreen(typeof(frmGroup));
        }
    }
}
