using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLyNSu.Reports;
using DevExpress.XtraReports.UI;

namespace QLyNSu.FORM_BAOCAO
{
    public partial class FrmBaoCaoChiTiet : DevExpress.XtraEditors.XtraForm
    {
        private MyEntities db = new MyEntities();
        private KYCONG _kycong = new KYCONG();

        public FrmBaoCaoChiTiet()
        {
            InitializeComponent();
        }

        private void FrmBaoCaoChiTiet_Load(object sender, EventArgs e)
        {
            LoadCombo();
        }

        private void LoadCombo()
        {
            try
            {
                // Load Kỳ công
                var lstKyCong = _kycong.getList();
                cboKyCong.Properties.DataSource = lstKyCong;
                cboKyCong.Properties.DisplayMember = "MAKYCONG";
                cboKyCong.Properties.ValueMember = "MAKYCONG";
                cboKyCong.Properties.Columns.Clear();
                cboKyCong.Properties.Columns.Add(new LookUpColumnInfo("MAKYCONG", "Kỳ Công"));
                
                // Load Nhân viên
                var lstNhanVien = db.TB_NHANVIEN.Where(x => (x.DATHOIVIEC ?? 0) == 0).ToList();
                cboNhanVien.Properties.DataSource = lstNhanVien;
                cboNhanVien.Properties.DisplayMember = "HOTEN";
                cboNhanVien.Properties.ValueMember = "MANV";
                cboNhanVien.Properties.Columns.Clear();
                cboNhanVien.Properties.Columns.Add(new LookUpColumnInfo("MANV", "Mã NV", 60));
                cboNhanVien.Properties.Columns.Add(new LookUpColumnInfo("HOTEN", "Họ Tên", 150));
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải danh mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboKyCong_EditValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void cboNhanVien_EditValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            if (cboKyCong.EditValue != null && cboNhanVien.EditValue != null)
            {
                try
                {
                    decimal makycong = Convert.ToDecimal(cboKyCong.EditValue);
                    decimal manv = Convert.ToDecimal(cboNhanVien.EditValue);

                    var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                    if (bl != null)
                    {
                        var list = new List<TB_BANGLUONG> { bl };
                        gcChiTiet.DataSource = list;
                        ConfigureGrid();
                    }
                    else
                    {
                        gcChiTiet.DataSource = null;
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                gcChiTiet.DataSource = null;
            }
        }

        private void ConfigureGrid()
        {
            gvChiTiet.Columns.Clear();
            
            AddColumn("MANV", "Mã NV", 80);
            AddColumn("CONG_CHUAN", "Công chuẩn", 80, "{0:N1}");
            AddColumn("CONG_THUCTE", "Công thực tế", 90, "{0:N1}");
            AddColumn("LUONG_CONG_THUCTE", "Lương thực tế", 110, "{0:N0}");
            AddColumn("PHUCAP_CONG_THUCTE", "Phụ cấp thực tế", 110, "{0:N0}");
            AddColumn("TIEN_TANGCA", "Tiền tăng ca", 100, "{0:N0}");
            AddColumn("TIEN_CHUYENCAN", "Chuyên cần", 100, "{0:N0}");
            AddColumn("TIEN_AN_CA", "Ăn ca", 90, "{0:N0}");
            AddColumn("TIEN_BHXH_TRICH", "BHXH trích", 110, "{0:N0}");
            AddColumn("TIEN_TAMUNG", "Tạm ứng", 100, "{0:N0}");
            AddColumn("KHOAN_TRU_KHAC", "Thuế TNCN", 110, "{0:N0}");
            AddColumn("THUC_LINH", "Thực lĩnh", 120, "{0:N0}");
            
            gvChiTiet.OptionsBehavior.Editable = false;
            FormManager_Functions.CustomView_Colums(gvChiTiet);
        }

        private void AddColumn(string fieldName, string caption, int width, string format = "")
        {
            var col = gvChiTiet.Columns.AddField(fieldName);
            col.Caption = caption;
            col.Visible = true;
            col.Width = width;
            if (!string.IsNullOrEmpty(format))
            {
                col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                col.DisplayFormat.FormatString = format;
            }
        }

        private void btnIn_Click(object sender, EventArgs e)
        {
            if (cboKyCong.EditValue == null || cboNhanVien.EditValue == null)
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công và nhân viên để in báo cáo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal makycong = Convert.ToDecimal(cboKyCong.EditValue);
                decimal manv = Convert.ToDecimal(cboNhanVien.EditValue);

                var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                if (bl == null)
                {
                    XtraMessageBox.Show("Không tìm thấy dữ liệu bảng lương cho nhân viên trong kỳ công đã chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                string hoten = nv != null ? nv.HOTEN : "";
                string tenpb = "";
                if (nv != null && nv.IDPB != null)
                {
                    var pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == nv.IDPB);
                    if (pb != null) tenpb = pb.TENPB;
                }

                int nam = (int)makycong / 100;
                int thang = (int)makycong % 100;

                // Query overtime hours
                double otHours = 0;
                var lstTangCa = db.TB_TANGCA.Where(x => x.MANV == manv && x.THANG == thang && x.NAM == nam).ToList();
                if (lstTangCa.Count > 0)
                {
                    otHours = (double)lstTangCa.Sum(x => x.SOGIO ?? 0);
                }

                // Query allowances
                var phucaps = db.TB_NHANVIEN_PHUCAP.Where(x => x.MANV == manv && x.MAKYCONG == makycong).ToList();

                rptBaoCaoLuongNV rpt = new rptBaoCaoLuongNV();
                rpt.BindData(bl, hoten, tenpb, otHours, phucaps);
                rpt.ShowRibbonPreviewDialog();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi hiển thị báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}