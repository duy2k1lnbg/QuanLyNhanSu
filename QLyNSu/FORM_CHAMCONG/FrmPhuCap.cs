using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
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
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_CC_PHUCAP")) return;

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

            searchMANV.EditValueChanged += searchMANV_EditValueChanged;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_PHUCAP", Bu.DTO.PermissionAction.Edit)) return;
            if (searchMANV.EditValue == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!Functions.FormSecurity.AssertPermission("F_CC_PHUCAP", Bu.DTO.PermissionAction.Edit)) return;

                if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out int manv))
                {
                    MessageBox.Show("Vui lòng chọn một nhân viên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var allowances = new Dictionary<int, decimal>
                {
                    { 1, spP1.Value },
                    { 2, spP2.Value },
                    { 3, spP3.Value },
                    { 4, spP4.Value },
                    { 5, spP5.Value },
                    { 6, spP6.Value },
                    { 7, spP7.Value },
                    { 8, spP8.Value },
                    { 9, spP9.Value },
                    { 10, spP10.Value },
                    { 11, spP11.Value },
                    { 12, spP12.Value },
                    { 13, spP13.Value }
                };

                _phucap.SavePhuCapHopDong(manv, allowances);
                LoadData();

                string hoten = searchMANV.Text;
                MessageBox.Show($"Cập nhật thành công 13 loại phụ cấp theo hợp đồng cho nhân viên: {manv} - {hoten}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                showHide(true);
                splitContainer1.Panel1Collapsed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật phụ cấp: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (!Functions.FormSecurity.AssertPermission("F_CC_PHUCAP", Bu.DTO.PermissionAction.Print)) return;
            if (gvDanhSach.RowCount > 0)
            {
                gvDanhSach.ShowRibbonPrintPreview();
            }
        }

        private void searchMANV_EditValueChanged(object sender, EventArgs e)
        {
            if (searchMANV.EditValue != null && int.TryParse(searchMANV.EditValue.ToString(), out int manv))
            {
                var dict = _phucap.GetPhuCapByNhanVien(manv);
                spP1.EditValue = dict.ContainsKey(1) ? dict[1] : 0;
                spP2.EditValue = dict.ContainsKey(2) ? dict[2] : 0;
                spP3.EditValue = dict.ContainsKey(3) ? dict[3] : 0;
                spP4.EditValue = dict.ContainsKey(4) ? dict[4] : 0;
                spP5.EditValue = dict.ContainsKey(5) ? dict[5] : 0;
                spP6.EditValue = dict.ContainsKey(6) ? dict[6] : 0;
                spP7.EditValue = dict.ContainsKey(7) ? dict[7] : 0;
                spP8.EditValue = dict.ContainsKey(8) ? dict[8] : 0;
                spP9.EditValue = dict.ContainsKey(9) ? dict[9] : 0;
                spP10.EditValue = dict.ContainsKey(10) ? dict[10] : 0;
                spP11.EditValue = dict.ContainsKey(11) ? dict[11] : 0;
                spP12.EditValue = dict.ContainsKey(12) ? dict[12] : 0;
                spP13.EditValue = dict.ContainsKey(13) ? dict[13] : 0;
                CalculateTotal(null, null);
            }
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.FocusedRowHandle >= 0)
            {
                var manvVal = gvDanhSach.GetFocusedRowCellValue("MANV");
                if (manvVal == null) return;
                int manv = Convert.ToInt32(manvVal);

                searchMANV.EditValue = manv;

                spP1.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC1") ?? 0);
                spP2.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC2") ?? 0);
                spP3.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC3") ?? 0);
                spP4.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC4") ?? 0);
                spP5.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC5") ?? 0);
                spP6.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC6") ?? 0);
                spP7.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC7") ?? 0);
                spP8.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC8") ?? 0);
                spP9.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC9") ?? 0);
                spP10.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC10") ?? 0);
                spP11.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC11") ?? 0);
                spP12.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC12") ?? 0);
                spP13.Value = Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIEN_IDPC13") ?? 0);

                CalculateTotal(null, null);

                splitContainer1.Panel1Collapsed = false;
                _them = false;
                showHide(false);
            }
        }

        private void CalculateTotal(object sender, EventArgs e)
        {
            decimal total = spP1.Value + spP2.Value + spP3.Value + spP4.Value +
                            spP5.Value + spP6.Value + spP7.Value + spP8.Value +
                            spP9.Value + spP10.Value + spP11.Value + spP12.Value +
                            spP13.Value;
            spPTong.Value = total;
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
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_CC_PHUCAP", null, btnSua, null, btnIn, kt);

            spP1.Enabled = !kt;
            spP2.Enabled = !kt;
            spP3.Enabled = !kt;
            spP4.Enabled = !kt;
            spP5.Enabled = !kt;
            spP6.Enabled = !kt;
            spP7.Enabled = !kt;
            spP8.Enabled = !kt;
            spP9.Enabled = !kt;
            spP10.Enabled = !kt;
            spP11.Enabled = !kt;
            spP12.Enabled = !kt;
            spP13.Enabled = !kt;
            searchMANV.Enabled = !kt;
        }
    }
}