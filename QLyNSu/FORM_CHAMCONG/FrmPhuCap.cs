using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
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

        private bool _them;

        private void FrmPhuCap_Load(object sender, EventArgs e)
        {
            _them = false;
            showHide(true);
            _nhanvien = new NHANVIEN();
            _phucap = new PHUCAP();
            LoadData();
            LoadNhanVien();
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
            gvDanhSach.OptionsFind.AlwaysVisible = true;
            gvDanhSach.OptionsFind.FindDelay = 100;
            splitContainer1.Panel1Collapsed = true;

        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(searchMANV.EditValue?.ToString()))
                {
                    MessageBox.Show("Vui lòng chọn một nhân viên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Dừng lại nếu không có giá trị
                }

                int makc = Convert.ToInt32(spNam.Text) * 100 + Convert.ToInt32(spP1.Text);
                var nv = Convert.ToInt32(searchMANV.EditValue);
                decimal sotienIdpc1 = Convert.ToDecimal(spP1.Text);
                decimal sotienIdpc2 = Convert.ToDecimal(spP2.Text);
                decimal sotienIdpc3 = Convert.ToDecimal(spP3.Text);
                decimal sotienIdpc4 = Convert.ToDecimal(spP4.Text);
                decimal sotienIdpc5 = Convert.ToDecimal(spP5.Text);
                decimal sotienIdpc6 = Convert.ToDecimal(spP6.Text);
                decimal sotienIdpc7 = Convert.ToDecimal(spP7.Text); 
                _phucap.UpdatePhucap(nv, 1, sotienIdpc1, makc);
                _phucap.UpdatePhucap(nv, 2, sotienIdpc2, makc);
                _phucap.UpdatePhucap(nv, 3, sotienIdpc3, makc);
                _phucap.UpdatePhucap(nv, 4, sotienIdpc4, makc);
                _phucap.UpdatePhucap(nv, 5, sotienIdpc5, makc);
                _phucap.UpdatePhucap(nv, 6, sotienIdpc6, makc);
                _phucap.UpdatePhucap(nv, 7, sotienIdpc7, makc);
                LoadData();
                var manv = Convert.ToInt32(searchMANV.EditValue); 
                var row = ((DataRowView)searchMANV.GetSelectedDataRow()).Row; 
                var hoten = row["HOTEN"].ToString();
                MessageBox.Show($"Cập nhật phụ cấp thành công cho nhân viên {manv} - {hoten} có mã công {makc}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }

        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
            splitContainer1.Panel1Collapsed = true;
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
                //spP1.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC1").ToString() ?? "0";
                //spP2.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC2").ToString() ?? "0";
                //spP3.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC3").ToString() ?? "0";
                //spP4.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC4").ToString() ?? "0";
                //spP5.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC5").ToString() ?? "0";
                //spP6.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC6").ToString() ?? "0";
                //spP7.Text = gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC7").ToString() ?? "0";
                spP1.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC1") ?? 0);
                spP2.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC2") ?? 0);
                spP3.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC3") ?? 0);
                spP4.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC4") ?? 0);
                spP5.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC5") ?? 0);
                spP6.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC6") ?? 0);
                spP7.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC7") ?? 0);
                int makc = Convert.ToInt32(gvDanhSach.GetFocusedRowCellValue("MAKYCONG").ToString());
                int year = makc / 100;
                int month = makc % 100;

                spNam.Text = year.ToString();
                spThang.Text = month.ToString();
                var ghiChuValue = gvDanhSach.GetFocusedRowCellValue("GHICHU");
            }
            splitContainer1.Panel1Collapsed = false;
            _them = false;
            showHide(false);
        }

        private void LoadData()
        {
            _phucap = new PHUCAP();
            gcDanhSach.DataSource = _phucap.GetNhanVienSortedByIDPC();
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

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnSua.Enabled = kt;
            btnIn.Enabled = kt;
            btnDong.Enabled = kt;
        }    
    }
}