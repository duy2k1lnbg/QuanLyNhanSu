using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bu;

namespace QLyNSu
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private NHANVIEN _nhanvien;
        private HOPDONGLAODONG _hopdong;

        private void OpenForm(Type typeForm)
        {
            foreach(var frm in MdiChildren)
                if(frm.GetType() == typeForm)
                {
                    frm.Activate();
                    return;
                }
            Form f = (Form)Activator.CreateInstance(typeForm);
            f.MdiParent = this;
            f.Show();
        }

        private void ribbonControl1_Click(object sender, EventArgs e)
        {

        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmDanToc));
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmTonGiao));
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmTrinhDo));
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmDieuChuyen_NhanVien));
        }

        private void barButtonItem15_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _nhanvien = new NHANVIEN();
            _hopdong = new HOPDONGLAODONG();
            ribbonControl1.SelectedPage = ribbonPage2;
            loadSinhNhat();
            loadLenLuong();
        }

        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmPhongBan));
        }

        private void barButtonItem5_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmBoPhan));
        }

        private void barButtonItem4_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmCongTy));
        }

        private void barButtonItem21_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmChucVu));
        }

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmNhanVien));
        }

        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmHopDongLaoDong));
        }

        private void barButtonItem20_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void barButtonItem22_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void btnKhenThuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmKhenThuong));
        }

        private void btnKyLuat_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmKyLuat));
        }

        private void btnThoiViec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmNhanVien_ThoiViec));
        }

        private void btnNangLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenForm(typeof(FrmNangLuong_NhanVien));
        }

        private void loadSinhNhat()
        {
            lstSinhNhat.DataSource = _nhanvien.getSinhNhat();
            lstSinhNhat.DisplayMember = "HOTEN";
            lstSinhNhat.ValueMember = "MANV";
        }

        private void loadLenLuong()
        {
            lstNangLuong.DataSource = _hopdong.getLenLuong();
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
    }
}
