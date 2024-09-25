using Bu;
using Bu.CLASS_CHAMCONG;
using DevExpress.DirectX.Common.DirectWrite;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmPhuCap : DevExpress.XtraEditors.XtraForm
    {
        public FrmPhuCap()
        {
            InitializeComponent();
        }
        private PHUCAP _phucap;
        private NHANVIEN _nhanvien;
        private int _id;



        private void FrmPhuCap_Load(object sender, EventArgs e)
        {
            _nhanvien = new NHANVIEN();
            _phucap = new PHUCAP();
            LoadData();
            LoadNhanVien();
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
            gvDanhSach.OptionsFind.AlwaysVisible = true;
            gvDanhSach.OptionsFind.FindDelay = 100;
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.FocusedRowHandle >= 0)
            {
                searchMANV.EditValue = gvDanhSach.GetFocusedRowCellValue("MANV").ToString();
                spP1.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap1").ToString();
                spP2.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap2").ToString();
                spP3.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap3").ToString();
                spP4.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap4").ToString();
                spP5.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap5").ToString();
                spP6.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap6").ToString();
                spP7.Text = gvDanhSach.GetFocusedRowCellValue("PhuCap7").ToString();
                var ghiChuValue = gvDanhSach.GetFocusedRowCellValue("GHICHU");
                txtGhiChu.Text = ghiChuValue?.ToString() ?? string.Empty;
            }
        }

        private void LoadData()
        {
            gcDanhSach.DataSource = _phucap.GetNhanVienPhuCap();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void LoadNhanVien()
        {
            var nhanVienList = _nhanvien.getList();

            DataTable dt = new DataTable();
            dt.Columns.Add("MANV", typeof(int));
            dt.Columns.Add("HOTEN", typeof(string));
            dt.Columns.Add("Display", typeof(string));

            foreach (var nhanVien in nhanVienList)
            {
                var displayText = $"{nhanVien.MANV} - {nhanVien.HOTEN}";
                dt.Rows.Add(nhanVien.MANV, nhanVien.HOTEN, displayText);
            }
            searchMANV.Properties.DataSource = dt;
            searchMANV.Properties.ValueMember = "MANV";
            searchMANV.Properties.DisplayMember = "Display";
        }

        private void LoadPhuCapID()
        {

        }

        private void searchMANV_EditValueChanged(object sender, EventArgs e)
        {
            
        }
    }
}