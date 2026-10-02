using Bu;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmThungRacNghiepVu : DevExpress.XtraEditors.XtraForm
    {
        private RecycleBinService _recycleBinService;

        public FrmThungRacNghiepVu()
        {
            InitializeComponent();
        }

        private void FrmThungRacNghiepVu_Load(object sender, EventArgs e)
        {
            _recycleBinService = new RecycleBinService();

            cboLoaiDoiTuong.Items.Clear();
            cboLoaiDoiTuong.Items.Add(new ComboBoxItem { Text = "Tất cả", Value = "" });
            cboLoaiDoiTuong.Items.Add(new ComboBoxItem { Text = "Hợp đồng lao động", Value = "HOPDONG" });
            cboLoaiDoiTuong.Items.Add(new ComboBoxItem { Text = "Phụ cấp nhân viên", Value = "PHUCAP" });
            cboLoaiDoiTuong.Items.Add(new ComboBoxItem { Text = "Kỳ công", Value = "KYCONG" });
            cboLoaiDoiTuong.Items.Add(new ComboBoxItem { Text = "Tạm ứng lương", Value = "UNGLUONG" });
            cboLoaiDoiTuong.Items.Add(new ComboBoxItem { Text = "Khoản phát sinh lương", Value = "PHATSINH" });
            cboLoaiDoiTuong.SelectedIndex = 0;

            LoadData();
            Functions.TranslationManager.Translate(this);
        }

        private void cboLoaiDoiTuong_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string selectedType = null;
                if (cboLoaiDoiTuong.SelectedItem is ComboBoxItem item && !string.IsNullOrEmpty(item.Value))
                {
                    selectedType = item.Value;
                }

                var list = _recycleBinService.GetDeletedItems(selectedType);
                gcThungRac.DataSource = list;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải danh sách thùng rác: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnKhoiPhuc_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selectedItem = gvThungRac.GetFocusedRow() as RecycleBinItemDTO;
            if (selectedItem == null)
            {
                XtraMessageBox.Show("Vui lòng chọn đối tượng cần khôi phục!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dialog = XtraMessageBox.Show(
                $"Bạn có chắc chắn muốn khôi phục đối tượng [{selectedItem.DisplayName}]?",
                "Xác nhận khôi phục",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 1;
                var result = _recycleBinService.Restore(selectedItem.ObjectType, selectedItem.ObjectId, userId);
                if (result.Success)
                {
                    XtraMessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                else
                {
                    XtraMessageBox.Show(result.Message, "Khôi phục thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnXoaVinhVien_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selectedItem = gvThungRac.GetFocusedRow() as RecycleBinItemDTO;
            if (selectedItem == null)
            {
                XtraMessageBox.Show("Vui lòng chọn đối tượng cần xóa vĩnh viễn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!UserSession.IsAdmin && !UserSession.HasRight("F_SYSTEM_PURGE"))
            {
                XtraMessageBox.Show("Bạn không có quyền thực hiện xóa vĩnh viễn (yêu cầu quyền F_SYSTEM_PURGE hoặc Quản trị viên)!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!selectedItem.CanPurge)
            {
                XtraMessageBox.Show("Đối tượng này không được phép xóa vĩnh viễn theo chính sách bảo toàn dữ liệu (đã có dữ liệu phát sinh hoặc ràng buộc nghiệp vụ)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialog = XtraMessageBox.Show(
                $"CẢNH BÁO: Thao tác xóa vĩnh viễn đối tượng [{selectedItem.DisplayName}] không thể hoàn tác!\nBạn có chắc chắn muốn xóa vĩnh viễn không?",
                "Cảnh báo xóa vĩnh viễn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dialog == DialogResult.Yes)
            {
                int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 1;
                var result = _recycleBinService.Purge(selectedItem.ObjectType, selectedItem.ObjectId, userId);
                if (result.Success)
                {
                    XtraMessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                else
                {
                    XtraMessageBox.Show(result.Message, "Không thể xóa vĩnh viễn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLamMoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
