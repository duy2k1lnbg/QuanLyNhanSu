using Bu;
using Bu.CLASS_NHANSU;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLyNSu.FORM_NHANSU
{
    public partial class FrmLoaiHopDong : DevExpress.XtraEditors.XtraForm
    {
        public FrmLoaiHopDong()
        {
            InitializeComponent();
        }

        private LOAIHOPDONG _loaiHopDong;
        private bool _them;
        private int _IDLOAIHD;

        private void FrmLoaiHopDong_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_NV_LOAIHOPDONG")) return;
            _them = false;
            _loaiHopDong = new LOAIHOPDONG();
            showHide(true);
            LoadData();
            Functions.TranslationManager.Translate(this);
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_NV_LOAIHOPDONG", btnThem, btnSua, btnXoa, null, kt);
            txtTen.Enabled = !kt;
            gcDanhSach.Enabled = kt;
        }

        private void LoadData()
        {
            gcDanhSach.DataSource = _loaiHopDong.getAll();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_LOAIHOPDONG", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            txtTen.Text = string.Empty;
            _IDLOAIHD = 0;
            txtTen.Focus();
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_LOAIHOPDONG", Bu.DTO.PermissionAction.Edit)) return;
            if (_IDLOAIHD <= 0)
            {
                MessageBox.Show("Vui lòng chọn một loại hợp đồng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
            txtTen.Focus();
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_LOAIHOPDONG", Bu.DTO.PermissionAction.Delete)) return;
            if (_IDLOAIHD <= 0)
            {
                MessageBox.Show("Vui lòng chọn một loại hợp đồng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa loại hợp đồng đã chọn không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _loaiHopDong.Delete(_IDLOAIHD, 1);
                    LoadData();
                    txtTen.Text = string.Empty;
                    _IDLOAIHD = 0;
                    MessageBox.Show("Xóa loại hợp đồng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!Functions.FormSecurity.AssertPermission("F_NV_LOAIHOPDONG", permAction)) return;

            try
            {
                string ten = txtTen.Text.Trim();
                if (string.IsNullOrEmpty(ten))
                {
                    MessageBox.Show("Vui lòng nhập tên loại hợp đồng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTen.Focus();
                    return;
                }

                if (_them)
                {
                    TB_LOAIHOPDONG lhd = new TB_LOAIHOPDONG();
                    lhd.TENLOAIHD = ten;
                    lhd.CREATED_BY = 1;
                    _loaiHopDong.Add(lhd);
                    MessageBox.Show("Thêm loại hợp đồng mới thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var lhd = _loaiHopDong.getItem(_IDLOAIHD);
                    if (lhd == null)
                    {
                        MessageBox.Show("Không tìm thấy dữ liệu loại hợp đồng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    lhd.TENLOAIHD = ten;
                    lhd.UPDATED_BY = 1;
                    _loaiHopDong.Update(lhd);
                    MessageBox.Show("Cập nhật loại hợp đồng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.RowCount > 0 && gvDanhSach.FocusedRowHandle >= 0)
            {
                var valId = gvDanhSach.GetFocusedRowCellValue("LOAIHD");
                if (valId != null && int.TryParse(valId.ToString(), out int id))
                {
                    _IDLOAIHD = id;
                }

                var valTen = gvDanhSach.GetFocusedRowCellValue("TENLOAIHD");
                txtTen.Text = valTen != null ? valTen.ToString() : string.Empty;
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
