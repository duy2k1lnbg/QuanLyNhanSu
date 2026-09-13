using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmNgayLe : DevExpress.XtraEditors.XtraForm
    {
        public FrmNgayLe()
        {
            InitializeComponent();
        }

        private NGAYLE _ngayLe;
        private bool _them;
        private int _IDLE;

        private void FrmNgayLe_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_CC_NGAYLE")) return;
            _them = false;
            _ngayLe = new NGAYLE();
            showHide(true);
            InitFilterNam();
            LoadData();
            Functions.TranslationManager.Translate(this);
        }

        private void InitFilterNam()
        {
            cboFilterNam.Items.Clear();
            cboFilterNam.Items.Add("Tất cả các năm");
            int currentYear = DateTime.Now.Year;
            for (int y = currentYear - 2; y <= currentYear + 3; y++)
            {
                cboFilterNam.Items.Add(y.ToString());
            }
            cboFilterNam.SelectedItem = currentYear.ToString();
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_CC_NGAYLE", btnThem, btnSua, btnXoa, null, kt);

            txtTenLe.Enabled = !kt;
            dtNgay.Enabled = !kt;
            spNam.Enabled = !kt;
            spHeSo.Enabled = !kt;
            gcDanhSach.Enabled = kt;
        }

        private void LoadData()
        {
            if (cboFilterNam.SelectedItem != null && int.TryParse(cboFilterNam.SelectedItem.ToString(), out int nam))
            {
                gcDanhSach.DataSource = _ngayLe.getListByNam(nam);
            }
            else
            {
                gcDanhSach.DataSource = _ngayLe.getAll();
            }
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_NGAYLE", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            _IDLE = 0;
            txtTenLe.Text = string.Empty;
            dtNgay.Value = DateTime.Now;
            spNam.EditValue = DateTime.Now.Year;
            spHeSo.EditValue = 3.00m;
            txtTenLe.Focus();
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_NGAYLE", Bu.DTO.PermissionAction.Edit)) return;
            if (_IDLE <= 0)
            {
                MessageBox.Show("Vui lòng chọn một ngày lễ để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
            txtTenLe.Focus();
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_NGAYLE", Bu.DTO.PermissionAction.Delete)) return;
            if (_IDLE <= 0)
            {
                MessageBox.Show("Vui lòng chọn một ngày lễ để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa ngày lễ đã chọn không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _ngayLe.Delete(_IDLE, 1);
                    LoadData();
                    _IDLE = 0;
                    txtTenLe.Text = string.Empty;
                    MessageBox.Show("Xóa ngày lễ thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
        }

        private void SaveData()
        {
            var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
            if (!Functions.FormSecurity.AssertPermission("F_CC_NGAYLE", permAction)) return;

            try
            {
                string ten = txtTenLe.Text.Trim();
                if (string.IsNullOrEmpty(ten))
                {
                    MessageBox.Show("Vui lòng nhập tên ngày lễ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTenLe.Focus();
                    return;
                }

                DateTime ngay = dtNgay.Value.Date;
                int nam = Convert.ToInt32(spNam.EditValue ?? ngay.Year);
                decimal heSo = Convert.ToDecimal(spHeSo.EditValue ?? 2.00m);

                // Kiểm tra trùng ngày
                if (_ngayLe.KiemTraTrungNgay(ngay, _them ? (int?)null : _IDLE))
                {
                    MessageBox.Show($"Ngày {ngay:dd/MM/yyyy} đã được khai báo là ngày lễ trong hệ thống. Vui lòng chọn ngày khác.", "Cảnh báo trùng ngày", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtNgay.Focus();
                    return;
                }

                if (_them)
                {
                    TB_NGAYLE nl = new TB_NGAYLE();
                    nl.TENLE = ten;
                    nl.NGAY = ngay;
                    nl.NAM = nam;
                    nl.HESO = heSo;
                    nl.CREATED_BY = 1;
                    _ngayLe.Add(nl);
                    MessageBox.Show("Thêm ngày lễ mới thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var nl = _ngayLe.getItem(_IDLE);
                    if (nl == null)
                    {
                        MessageBox.Show("Không tìm thấy thông tin ngày lễ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    nl.TENLE = ten;
                    nl.NGAY = ngay;
                    nl.NAM = nam;
                    nl.HESO = heSo;
                    nl.UPDATED_BY = 1;
                    _ngayLe.Update(nl);
                    MessageBox.Show("Cập nhật ngày lễ thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadData();
                _them = false;
                showHide(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void dtNgay_ValueChanged(object sender, EventArgs e)
        {
            spNam.EditValue = dtNgay.Value.Year;
        }

        private void cboFilterNam_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.RowCount > 0 && gvDanhSach.FocusedRowHandle >= 0)
            {
                var valId = gvDanhSach.GetFocusedRowCellValue("IDLE");
                if (valId != null && int.TryParse(valId.ToString(), out int id))
                {
                    _IDLE = id;
                }

                var valTen = gvDanhSach.GetFocusedRowCellValue("TENLE");
                txtTenLe.Text = valTen != null ? valTen.ToString() : string.Empty;

                var valNgay = gvDanhSach.GetFocusedRowCellValue("NGAY");
                if (valNgay != null && DateTime.TryParse(valNgay.ToString(), out DateTime ngay))
                {
                    dtNgay.Value = ngay;
                    spNam.EditValue = ngay.Year;
                }

                var valNam = gvDanhSach.GetFocusedRowCellValue("NAM");
                if (valNam != null && decimal.TryParse(valNam.ToString(), out decimal nam))
                {
                    spNam.EditValue = nam;
                }

                var valHeSo = gvDanhSach.GetFocusedRowCellValue("HESO");
                if (valHeSo != null && decimal.TryParse(valHeSo.ToString(), out decimal heso))
                {
                    spHeSo.EditValue = heso;
                }
            }
        }

        private void gvDanhSach_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.Name == "DELETED_BY")
            {
                Image img = e.CellValue != null ? Properties.Resources.del : Properties.Resources.no_del;
                Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.DrawImage(img, rect);
                e.Handled = true;
            }
        }
    }
}
