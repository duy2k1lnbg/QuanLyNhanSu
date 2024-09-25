using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using QLyNSu.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmBangCongNV_IN : DevExpress.XtraEditors.XtraForm
    {
        public FrmBangCongNV_IN()
        {
            InitializeComponent();
        }

        private NHANVIEN _nhanvien;
        private KYCONG _kycong;
        private BANGCONG_NV_CHITIET _bcct_nv;

        private void FrmBangCongNV_IN_Load(object sender, EventArgs e)
        {
            _nhanvien = new NHANVIEN();
            _kycong = new KYCONG();
            _bcct_nv = new BANGCONG_NV_CHITIET();
            LoadNhanVien();
            LoadKyCong();
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
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

        private void LoadKyCong()
        {
            cboKyCong.DataSource = _kycong.getList();
            cboKyCong.DisplayMember = "MAKYCONG";
            cboKyCong.ValueMember = "MAKYCONG";
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnIn2_Click(object sender, EventArgs e)
        {
            if (searchMANV.EditValue == null || string.IsNullOrEmpty(searchMANV.EditValue.ToString()))
            {
                MessageBox.Show($"Vui lòng chọn 1 nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboKyCong.SelectedIndex == -1)
            {
                MessageBox.Show($"Vui lòng chọn Kỳ Công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var lst = _bcct_nv.getBangCongCT(Convert.ToInt32(cboKyCong.SelectedValue), (int)searchMANV.EditValue);
            rptBangCongCTNV frm = new rptBangCongCTNV(lst);
            frm.ShowRibbonPreviewDialog();
        }
    }
}