using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmPhatSinhLuong : DevExpress.XtraEditors.XtraForm
    {
        private KYCONG _kycong;
        private readonly PayrollOccurrenceService _occurrenceService = new PayrollOccurrenceService();

        public FrmPhatSinhLuong()
        {
            InitializeComponent();
        }

        private void FrmPhatSinhLuong_Load(object sender, EventArgs e)
        {
            _kycong = new KYCONG();

            cboKyCong.DataSource = _kycong.getList();
            cboKyCong.DisplayMember = "MAKYCONG";
            cboKyCong.ValueMember = "MAKYCONG";

            cboLoaiPhatSinh.Items.Clear();
            cboLoaiPhatSinh.Items.Add("Tất cả");
            cboLoaiPhatSinh.Items.Add("Thu nhập phát sinh");
            cboLoaiPhatSinh.Items.Add("Khấu trừ phát sinh");
            cboLoaiPhatSinh.SelectedIndex = 0;

            gvPhatSinh.PopupMenuShowing += GvPhatSinh_PopupMenuShowing;
            gvPhatSinh.DoubleClick += GvPhatSinh_DoubleClick;

            LoadData();
            Functions.TranslationManager.Translate(this);
        }

        private void GvPhatSinh_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row || e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.User)
            {
                var itemAddDraft = new DevExpress.Utils.Menu.DXMenuItem("Thêm khoản phát sinh nháp mới...", OnAddDraftOccurrenceClicked);
                e.Menu.Items.Add(itemAddDraft);

                var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
                if (selected != null && selected.SourceType == "PHATSINH")
                {
                    var itemEdit = new DevExpress.Utils.Menu.DXMenuItem("Xem / Sửa chi tiết phát sinh...", (s, ev) => OpenDetailDialog(selected));
                    e.Menu.Items.Add(itemEdit);

                    if (selected.TrangThai == "Đã duyệt")
                    {
                        var itemRevoke = new DevExpress.Utils.Menu.DXMenuItem("Thu hồi phê duyệt (chuyển về Bản nháp)...", (s, ev) => RevokeSelectedOccurrence(selected));
                        e.Menu.Items.Add(itemRevoke);
                    }
                }
            }
        }

        private void GvPhatSinh_DoubleClick(object sender, EventArgs e)
        {
            var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
            if (selected != null && selected.SourceType == "PHATSINH")
            {
                OpenDetailDialog(selected);
            }
        }

        private void OpenDetailDialog(PayrollOccurrenceDto selected)
        {
            if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ trước khi thao tác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var frm = new FrmChiTietPhatSinh(selected, makycong))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnAddDraftOccurrenceClicked(sender, EventArgs.Empty);
        }

        private void OnAddDraftOccurrenceClicked(object sender, EventArgs e)
        {
            if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ trước khi thêm phát sinh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var frm = new FrmChiTietPhatSinh(makycong))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void cboKyCong_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void cboLoaiPhatSinh_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
                {
                    gcPhatSinh.DataSource = null;
                    return;
                }

                string filterType = cboLoaiPhatSinh.SelectedItem?.ToString() ?? "Tất cả";
                var items = _occurrenceService.GetOccurrences(makycong, filterType);
                gcPhatSinh.DataSource = items;
            }
            catch (Exception ex)
            {
                string userMsg = ErrorHelper.ResolveUserFriendlyMessage(ex, "tải danh sách phát sinh lương");
                XtraMessageBox.Show(userMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDuyet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
            if (selected == null)
            {
                XtraMessageBox.Show("Vui lòng chọn dòng phát sinh cần duyệt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.SourceType == "PHUCAP")
            {
                XtraMessageBox.Show("Khoản phụ cấp này thuộc dữ liệu tham chiếu từ Hợp đồng lao động (module Phụ cấp). Không thể duyệt trực tiếp tại form này để bảo toàn quy trình quản lý phụ cấp.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.SourceType == "UNGLUONG")
            {
                XtraMessageBox.Show("Khoản tạm ứng này thuộc module Quản lý Tạm ứng (FrmUngLuong). Vui lòng thực hiện phê duyệt tại form Tạm ứng để tuân thủ quy trình kiểm soát tài chính.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.TrangThai == "Đã duyệt")
            {
                var askRevoke = XtraMessageBox.Show(
                    $"Khoản phát sinh [{selected.TenKhoan}] của nhân viên [{selected.HoTen}] đã ở trạng thái [Đã duyệt].\n\nBạn có muốn THU HỒI phê duyệt khoản này về trạng thái [Bản nháp] không?",
                    "Thu hồi phê duyệt",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (askRevoke == DialogResult.Yes)
                {
                    RevokeSelectedOccurrence(selected);
                }
                return;
            }

            int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 0;
            if (userId <= 0)
            {
                XtraMessageBox.Show("Vui lòng đăng nhập để thực hiện phê duyệt!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = XtraMessageBox.Show(
                $"Xác nhận phê duyệt khoản phát sinh [{selected.TenKhoan}] số tiền {selected.SoTien:N0} đ của nhân viên [{selected.HoTen}]?\nSau khi duyệt, khoản này sẽ được tính vào bảng lương chính thức.",
                "Xác nhận phê duyệt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _occurrenceService.ApproveOccurrence(selected.SourceId, userId, selected.DataVersion);
                XtraMessageBox.Show($"Đã phê duyệt thành công khoản phát sinh [{selected.TenKhoan}] của nhân viên [{selected.HoTen}]!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Phê duyệt khoản phát sinh");
                XtraMessageBox.Show(msg, "Lỗi phê duyệt", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThuHoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
            if (selected == null)
            {
                XtraMessageBox.Show("Vui lòng chọn dòng phát sinh cần thu hồi phê duyệt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.SourceType != "PHATSINH")
            {
                XtraMessageBox.Show("Chỉ có thể thu hồi phát sinh được tạo từ quyết định hoặc khoản phát sinh lương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.TrangThai != "Đã duyệt")
            {
                XtraMessageBox.Show("Khoản phát sinh này không ở trạng thái [Đã duyệt], không thể thu hồi phê duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RevokeSelectedOccurrence(selected);
        }

        private void RevokeSelectedOccurrence(PayrollOccurrenceDto selected)
        {
            int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 0;
            if (userId <= 0)
            {
                XtraMessageBox.Show("Vui lòng đăng nhập để thực hiện thao tác!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string lyDo = XtraInputBox.Show(
                $"Nhập lý do thu hồi phê duyệt khoản phát sinh [{selected.TenKhoan}] của nhân viên [{selected.HoTen}]:",
                "Lý do thu hồi duyệt",
                "Sai sót số liệu / Thu hồi theo đề nghị"
            );

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                XtraMessageBox.Show("Lý do thu hồi là bắt buộc để lưu vết kiểm toán!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _occurrenceService.RevokeOccurrence(selected.SourceId, userId, lyDo.Trim(), selected.DataVersion);
                XtraMessageBox.Show($"Đã thu hồi phê duyệt khoản phát sinh [{selected.TenKhoan}]. Khoản này đã chuyển về trạng thái [Đã thu hồi / Hủy] và bị loại khỏi lần tính lương tiếp theo.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Thu hồi phê duyệt phát sinh");
                XtraMessageBox.Show(msg, "Lỗi thu hồi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoaMem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
            if (selected == null)
            {
                XtraMessageBox.Show("Vui lòng chọn dòng phát sinh cần xóa vào Thùng rác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.SourceType == "PHUCAP")
            {
                XtraMessageBox.Show("Không thể xóa khoản phụ cấp hợp đồng từ form này. Vui lòng quản lý phụ cấp tại module Phụ cấp nhân viên để bảo toàn dữ liệu nguồn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selected.SourceType == "UNGLUONG")
            {
                XtraMessageBox.Show("Không thể xóa phiếu tạm ứng từ form này. Vui lòng quản lý tại module Tạm ứng tiền lương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 0;
            if (userId <= 0)
            {
                XtraMessageBox.Show("Vui lòng đăng nhập để thực hiện thao tác xóa!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialog = XtraMessageBox.Show(
                $"Bạn có chắc chắn muốn chuyển khoản phát sinh [{selected.TenKhoan}] ({selected.SoTien:N0} đ) của nhân viên [{selected.HoTen}] vào Thùng rác không?",
                "Xác nhận xóa vào Thùng rác",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dialog != DialogResult.Yes) return;

            try
            {
                _occurrenceService.DeleteOccurrence(selected.SourceId, userId, selected.DataVersion);
                XtraMessageBox.Show("Đã chuyển khoản phát sinh vào Thùng rác thành công. Bạn có thể khôi phục trong Quản trị dữ liệu > Thùng rác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Xóa khoản phát sinh");
                XtraMessageBox.Show(msg, "Lỗi xóa phát sinh", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
