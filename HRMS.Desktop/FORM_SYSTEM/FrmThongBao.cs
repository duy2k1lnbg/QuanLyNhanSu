using Bu;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
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

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmThongBao : DevExpress.XtraEditors.XtraForm
    {
        public FrmThongBao()
        {
            InitializeComponent();
        }

        private THONGBAO _thongbao;
        private bool _them;
        private decimal _ID;

        private void FrmThongBao_Load(object sender, EventArgs e)
        {
            _thongbao = new THONGBAO();
            _them = false;
            LoadCombo();
            showHide(true);
            LoadData();
            ApplyAuthorization();
            gvDanhSach.CustomColumnDisplayText += gvDanhSach_CustomColumnDisplayText;
        }

        public void LoadCombo()
        {
            try
            {
                // 1. Static list for cboLoaiTB
                cboLoaiTB.Properties.Items.Clear();
                cboLoaiTB.Properties.Items.Add("Thông báo chung");
                cboLoaiTB.Properties.Items.Add("Chính sách mới");
                cboLoaiTB.Properties.Items.Add("Tin khẩn cấp");
                cboLoaiTB.Properties.Items.Add("Quy chế công ty");
                cboLoaiTB.SelectedIndex = 0;

                // 2. Static list for cboTrangThai
                cboTrangThai.Properties.Items.Clear();
                cboTrangThai.Properties.Items.Add("Bản nháp");
                cboTrangThai.Properties.Items.Add("Đã đăng");
                cboTrangThai.Properties.Items.Add("Đã ẩn");
                cboTrangThai.SelectedIndex = 1; // Default to "Đã đăng"

                // 3. Load cboCongTy list from Database
                var listCongTy = new CONGTY().getList();
                cboCongTy.Properties.DataSource = listCongTy;
                cboCongTy.Properties.DisplayMember = "TENCTY";
                cboCongTy.Properties.ValueMember = "IDCTY";
                cboCongTy.Properties.Columns.Clear();
                cboCongTy.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("TENCTY", "Tên Công ty"));

                // 4. Load cboPhongBan list from Database
                var listPhongBan = new PHONGBAN().getListDTO(QLyNSu.Functions.TranslationManager.GetCurrentLanguageCode());
                cboPhongBan.Properties.DataSource = listPhongBan;
                cboPhongBan.Properties.DisplayMember = "TENPB";
                cboPhongBan.Properties.ValueMember = "IDPB";
                cboPhongBan.Properties.Columns.Clear();
                cboPhongBan.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("TENPB", "Tên Phòng ban"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gvDanhSach_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "TRANGTHAI" && e.Value != null)
            {
                int val = Convert.ToInt32(e.Value);
                switch (val)
                {
                    case 0:
                        e.DisplayText = "Bản nháp";
                        break;
                    case 1:
                        e.DisplayText = "Đã đăng";
                        break;
                    case 2:
                        e.DisplayText = "Đã ẩn";
                        break;
                }
            }
            else if (e.Column.FieldName == "IS_PINNED" && e.Value != null)
            {
                int val = Convert.ToInt32(e.Value);
                e.DisplayText = val == 1 ? "Ghim" : "";
            }
        }

        private void showHide(bool kt)
        {
            // Only allow buttons if user is admin
            bool isAdmin = UserSession.CurrentUser != null && UserSession.CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);

            if (isAdmin)
            {
                btnThem.Enabled = kt;
                btnSua.Enabled = kt;
                btnXoa.Enabled = kt;
                btnLuu.Enabled = !kt;
                btnHuy.Enabled = !kt;
            }
            else
            {
                btnThem.Enabled = false;
                btnSua.Enabled = false;
                btnXoa.Enabled = false;
                btnLuu.Enabled = false;
                btnHuy.Enabled = false;
            }

            btnDong.Enabled = true;

            txtTieuDe.Enabled = !kt && isAdmin;
            txtNoiDung.Properties.ReadOnly = kt || !isAdmin;

            cboLoaiTB.ReadOnly = kt || !isAdmin;
            cboTrangThai.ReadOnly = kt || !isAdmin;
            chkGhim.ReadOnly = kt || !isAdmin;
            dtNgayHetHan.ReadOnly = kt || !isAdmin;
            txtFileDinhKem.ReadOnly = kt || !isAdmin;
            cboCongTy.ReadOnly = kt || !isAdmin;
            cboPhongBan.ReadOnly = kt || !isAdmin;
        }

        private void ApplyAuthorization()
        {
            bool isAdmin = UserSession.CurrentUser != null && UserSession.CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                txtTieuDe.Enabled = false;
                txtNoiDung.Properties.ReadOnly = true;
                cboLoaiTB.ReadOnly = true;
                cboTrangThai.ReadOnly = true;
                chkGhim.ReadOnly = true;
                dtNgayHetHan.ReadOnly = true;
                txtFileDinhKem.ReadOnly = true;
                cboCongTy.ReadOnly = true;
                cboPhongBan.ReadOnly = true;
            }
        }

        public void LoadData()
        {
            try
            {
                gcDanhSach.DataSource = _thongbao.getListFull_DTO(QLyNSu.Functions.TranslationManager.GetCurrentLanguageCode());
                FormManager_Functions.CustomView_Colums(gvDanhSach);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool SaveData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTieuDe.Text))
                {
                    MessageBox.Show("Vui lòng điền tiêu đề thông báo.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
                {
                    MessageBox.Show("Vui lòng điền nội dung thông báo.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (_them)
                {
                    TB_THONGBAO tb = new TB_THONGBAO
                    {
                        TIEUDE = txtTieuDe.Text.Trim(),
                        NOIDUNG = txtNoiDung.Text.Trim(),
                        NGUOIDANG = UserSession.CurrentUser != null ? UserSession.CurrentUser.FULLNAME : "ADMIN",
                        NGAYDANG = DateTime.Now,
                        LOAI_TB = cboLoaiTB.Text,
                        IS_PINNED = chkGhim.Checked,
                        TRANGTHAI = cboTrangThai.SelectedIndex == 1,
                        NGAY_HETHAN = dtNgayHetHan.EditValue != null ? (DateTime?)dtNgayHetHan.DateTime : null,
                        FILE_DINHKEM = txtFileDinhKem.Text.Trim(),
                        MACTY = cboCongTy.EditValue != null ? cboCongTy.EditValue.ToString() : null,
                        MAPB = cboPhongBan.EditValue != null ? cboPhongBan.EditValue.ToString() : null
                    };
                    _thongbao.Add(tb);
                }
                else
                {
                    var tb = _thongbao.getItem(_ID);
                    if (tb != null)
                    {
                        tb.TIEUDE = txtTieuDe.Text.Trim();
                        tb.NOIDUNG = txtNoiDung.Text.Trim();
                        tb.NGUOIDANG = UserSession.CurrentUser != null ? UserSession.CurrentUser.FULLNAME : "ADMIN";
                        tb.NGAYDANG = DateTime.Now;
                        tb.LOAI_TB = cboLoaiTB.Text;
                        tb.IS_PINNED = chkGhim.Checked;
                        tb.TRANGTHAI = cboTrangThai.SelectedIndex == 1;
                        tb.NGAY_HETHAN = dtNgayHetHan.EditValue != null ? (DateTime?)dtNgayHetHan.DateTime : null;
                        tb.FILE_DINHKEM = txtFileDinhKem.Text.Trim();
                        tb.MACTY = cboCongTy.EditValue != null ? cboCongTy.EditValue.ToString() : null;
                        tb.MAPB = cboPhongBan.EditValue != null ? cboPhongBan.EditValue.ToString() : null;
                        _thongbao.Update(tb);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông báo cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu thông báo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = true;
            showHide(false);
            txtTieuDe.Text = string.Empty;
            txtNoiDung.Text = string.Empty;
            txtNguoiDang.Text = UserSession.CurrentUser != null ? UserSession.CurrentUser.FULLNAME : "ADMIN";
            txtNgayDang.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            cboLoaiTB.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 1;
            chkGhim.Checked = false;
            dtNgayHetHan.EditValue = null;
            txtFileDinhKem.Text = string.Empty;
            cboCongTy.EditValue = null;
            cboPhongBan.EditValue = null;

            txtTieuDe.Focus();
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_ID <= 0)
            {
                MessageBox.Show("Vui lòng chọn thông báo muốn sửa.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
            txtTieuDe.Focus();
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_ID <= 0)
            {
                MessageBox.Show("Vui lòng chọn một thông báo để xóa.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa thông báo này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _thongbao.Delete(_ID);
                    ResetInputs();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (SaveData())
            {
                LoadData();
                _them = false;
                showHide(true);
            }
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
            gvDanhSach_Click(null, null);
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            var item = gvDanhSach.GetFocusedRow() as THONGBAO_DTO;
            if (item != null)
            {
                _ID = item.ID;
                txtTieuDe.Text = item.TIEUDE ?? string.Empty;
                txtNoiDung.Text = item.NOIDUNG ?? string.Empty;
                txtNguoiDang.Text = item.NGUOIDANG ?? string.Empty;
                txtNgayDang.Text = item.NGAYDANG.ToString("dd/MM/yyyy HH:mm");

                cboLoaiTB.Text = item.LOAI_TB ?? "Thông báo chung";
                chkGhim.Checked = item.IS_PINNED == 1;

                if (item.TRANGTHAI != null)
                {
                    int idx = Convert.ToInt32(item.TRANGTHAI);
                    if (idx >= 0 && idx < cboTrangThai.Properties.Items.Count)
                        cboTrangThai.SelectedIndex = idx;
                    else
                        cboTrangThai.SelectedIndex = 1;
                }
                else
                {
                    cboTrangThai.SelectedIndex = 1;
                }

                if (item.NGAY_HETHAN != null)
                {
                    dtNgayHetHan.DateTime = item.NGAY_HETHAN.Value;
                }
                else
                {
                    dtNgayHetHan.EditValue = null;
                }

                txtFileDinhKem.Text = item.FILE_DINHKEM ?? string.Empty;

                if (!string.IsNullOrEmpty(item.MACTY))
                {
                    cboCongTy.EditValue = Convert.ToDecimal(item.MACTY);
                }
                else
                {
                    cboCongTy.EditValue = null;
                }

                if (!string.IsNullOrEmpty(item.MAPB))
                {
                    cboPhongBan.EditValue = Convert.ToDecimal(item.MAPB);
                }
                else
                {
                    cboPhongBan.EditValue = null;
                }
            }
            else
            {
                ResetInputs();
            }
        }

        private void ResetInputs()
        {
            _ID = 0;
            txtTieuDe.Text = string.Empty;
            txtNoiDung.Text = string.Empty;
            txtNguoiDang.Text = string.Empty;
            txtNgayDang.Text = string.Empty;
            cboLoaiTB.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 1;
            chkGhim.Checked = false;
            dtNgayHetHan.EditValue = null;
            txtFileDinhKem.Text = string.Empty;
            cboCongTy.EditValue = null;
            cboPhongBan.EditValue = null;
        }

        private void txtFileDinhKem_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn tệp đính kèm";
                ofd.Filter = "Tất cả các tệp (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFileDinhKem.Text = ofd.FileName;
                }
            }
        }
    }
}
