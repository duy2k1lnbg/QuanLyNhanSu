using Bu;
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
using DA;

namespace QLyNSu
{
    public partial class FrmPhongBan : DevExpress.XtraEditors.XtraForm
    {
        public FrmPhongBan()
        {
            InitializeComponent();
        }

        private PHONGBAN _phongban;
        bool _them;
        int _IDPB;

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_DM_PHONGBAN", btnThem, btnSua, btnXoa, btnIn, kt);
            txtTen.Enabled = !kt;
        }

        public async Task LoadData()
        {
            try
            {
            gcDsPB.DataSource = await Functions.FormDataLoader.ReadAsync(this, () => _phongban.getListDTO(QLyNSu.Functions.TranslationManager.GetCurrentLanguageCode()));
            FormManager_Functions.CustomView_Colums(gvDsPB);
                    }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private void SaveData()
        {
            var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
            if (!Functions.FormSecurity.AssertPermission("F_DM_PHONGBAN", permAction)) return;

            try
            {
                if (string.IsNullOrWhiteSpace(txtTen.Text))
                {
                    MessageBox.Show("Vui lòng điền tên phòng ban.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_them)
                {
                    TB_PHONGBAN pb = new TB_PHONGBAN();
                    //pb.TENPB = txtTen.Text;
                    pb.TENPB = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    _phongban.Add(pb);
                }
                else
                {
                    var pb = _phongban.getItem(_IDPB);
                    if (pb != null)
                    {
                        //pb.TENPB = txtTen.Text;
                        pb.TENPB = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        _phongban.Update(pb);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDPB);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_PHONGBAN", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            txtTen.Text = string.Empty;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_PHONGBAN", Bu.DTO.PermissionAction.Edit)) return;
            _them = false;
            showHide(false);
        }

        private async void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_PHONGBAN", Bu.DTO.PermissionAction.Delete)) return;
            // Kiểm tra xem _IDTG có giá trị hợp lệ không
            if (_IDPB <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _phongban.Delete(_IDPB);
                await LoadData();
            if (IsDisposed || Disposing) return;
            }
        }

        private async void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
            await LoadData();
            if (IsDisposed || Disposing) return;
            _them = false;
            showHide(true);
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

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_PHONGBAN", Bu.DTO.PermissionAction.Print)) return;
        }

        private void gvDsPB_Click(object sender, EventArgs e)
        {
            //_IDPB = int.Parse(gvDsPB.GetFocusedRowCellValue("IDPB").ToString());
            //txtTen.Text = gvDsPB.GetFocusedRowCellValue("TENPB").ToString();

            if (gvDsPB.GetFocusedRowCellValue("IDPB") != null)
            {
                _IDPB = int.Parse(gvDsPB.GetFocusedRowCellValue("IDPB").ToString());
            }
            else
            {
                _IDPB = -1;
            }

            var tendtValue = gvDsPB.GetFocusedRowCellValue("TENPB");
            if (tendtValue != null && !string.IsNullOrEmpty(tendtValue.ToString()))
            {
                txtTen.Text = tendtValue.ToString();
            }
            else
            {
                txtTen.Text = string.Empty;
            }
        }

        private async void FrmPhongBan_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_DM_PHONGBAN")) return;
            _them = false;
            _phongban = new PHONGBAN();
            showHide(true);
            await LoadData();
            if (IsDisposed || Disposing) return;
        }
    }
}