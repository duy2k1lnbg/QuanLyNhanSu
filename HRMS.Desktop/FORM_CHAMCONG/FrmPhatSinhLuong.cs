using System.Threading.Tasks;
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
        public int InitialPeriod { get; set; }
        private readonly DevExpress.XtraBars.BarButtonItem _manageSource = new DevExpress.XtraBars.BarButtonItem();
        private bool _loadingFilters;
        private int _loadVersion;
        private KYCONG _kycong;
        private readonly Label _sourceNotice = new Label { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(12, 4, 12, 4), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        private readonly PayrollOccurrenceService _occurrenceService = new PayrollOccurrenceService();

        public FrmPhatSinhLuong()
        {
            InitializeComponent();
            Functions.DesktopTypography.Apply(this, barManager1);
            barManager1.Items.Add(_manageSource); bar1.AddItem(_manageSource);
            _manageSource.ItemClick += async (sender, args) => await OpenSource(gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto);
            Functions.DesktopTypography.Apply(this, barManager1);
            Functions.DesktopTypography.Grid(gvPhatSinh);
            _sourceNotice.Font = Font;
            Controls.Add(_sourceNotice);
            _sourceNotice.BringToFront();
            Controls.SetChildIndex(gcPhatSinh, 0); // Fill last, after reserving the bottom source notice.
            lblLoai.Text = "Loại dữ liệu:";
            gvPhatSinh.FocusedRowChanged += (sender, args) => UpdateOccurrenceActions(gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto);
            UpdateOccurrenceActions(null);
        }

        private void UpdateOccurrenceActions(PayrollOccurrenceDto selected)
        {
            bool own = selected != null && selected.SourceType == "PHATSINH";
            bool allowance = selected?.SourceType == "PHUCAP", advance = selected?.SourceType == "UNGLUONG";
            _manageSource.Caption = allowance ? "Quản lý phụ cấp" : advance ? "Quản lý ứng lương" : "Mở nơi quản lý";
            _manageSource.Visibility = allowance || advance ? DevExpress.XtraBars.BarItemVisibility.Always : DevExpress.XtraBars.BarItemVisibility.Never;
            _manageSource.Enabled = selected != null && UserSession.CanView(allowance ? "F_CC_PHUCAP" : "F_CC_UNGLUONG");
            bool edit = UserSession.CanEdit("F_CC_BANGLUONG");
            btnThem.Enabled = UserSession.CanAdd("F_CC_BANGLUONG");
            btnDuyet.Enabled = own && selected.CanApprove && edit;
            btnThuHoi.Enabled = own && selected.CanRevoke && edit;
            btnXoaMem.Enabled = own && selected.CanDelete && UserSession.CanDelete("F_CC_BANGLUONG");
            string hint = selected?.SourceType == "PHUCAP"
                ? "Phụ cấp hợp đồng: quản lý tại mục Phụ cấp nhân viên."
                : selected?.SourceType == "UNGLUONG"
                    ? "Tạm ứng: quản lý tại mục Ứng lương."
                    : "Chọn khoản phát sinh nháp để duyệt; khoản đã duyệt có thể thu hồi.";
            Functions.TranslationManager.SetTranslatedText(_sourceNotice, hint);
            btnDuyet.Hint = btnThuHoi.Hint = btnXoaMem.Hint = _sourceNotice.Text;
        }

        private async void FrmPhatSinhLuong_Load(object sender, EventArgs e)
        {
            try
            {
            _loadingFilters = true;
            _kycong = new KYCONG();

            cboKyCong.DataSource = await Functions.FormDataLoader.ReadAsync(this, () => _kycong.getList());
            cboKyCong.DisplayMember = "MAKYCONG";
            cboKyCong.ValueMember = "MAKYCONG";
            if (PayrollReadinessService.IsValidPeriod(InitialPeriod)) cboKyCong.SelectedValue = InitialPeriod;

            cboLoaiPhatSinh.Items.Clear();
            cboLoaiPhatSinh.Items.Add("Khoản phát sinh nhập riêng");
            cboLoaiPhatSinh.Items.Add("Phụ cấp hợp đồng");
            cboLoaiPhatSinh.Items.Add("Ứng lương");
            cboLoaiPhatSinh.Items.Add("Tất cả nguồn");
            cboLoaiPhatSinh.Items.Add("Thu nhập phát sinh");
            cboLoaiPhatSinh.Items.Add("Khấu trừ phát sinh");
            cboLoaiPhatSinh.SelectedIndex = 0;

            gvPhatSinh.PopupMenuShowing += GvPhatSinh_PopupMenuShowing;
            gvPhatSinh.DoubleClick += GvPhatSinh_DoubleClick;

            _loadingFilters = false;
            await LoadData();

            if (IsDisposed || Disposing) return;
            Functions.TranslationManager.Translate(this);
        
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private void GvPhatSinh_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row || e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.User)
            {
                var itemAddDraft = new DevExpress.Utils.Menu.DXMenuItem("Thêm khoản phát sinh nháp mới...", OnAddDraftOccurrenceClicked);
                itemAddDraft.Enabled = UserSession.CanAdd("F_CC_BANGLUONG");
                e.Menu.Items.Add(itemAddDraft);

                var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
                if (selected != null && selected.SourceType == "PHATSINH")
                {
                    var itemEdit = new DevExpress.Utils.Menu.DXMenuItem("Xem / Sửa chi tiết phát sinh...", async (s, ev) => await OpenDetailDialog(selected));
                    itemEdit.Enabled = UserSession.CanView("F_CC_BANGLUONG");
                    e.Menu.Items.Add(itemEdit);

                    if (selected.TrangThai == "Đã duyệt")
                    {
                        var itemRevoke = new DevExpress.Utils.Menu.DXMenuItem("Thu hồi phê duyệt (chuyển về Bản nháp)...", async (s, ev) => await RevokeSelectedOccurrence(selected));
                        itemRevoke.Enabled = selected.CanRevoke && UserSession.CanEdit("F_CC_BANGLUONG");
                        e.Menu.Items.Add(itemRevoke);
                    }
                }
            }
        }

        private async void GvPhatSinh_DoubleClick(object sender, EventArgs e)
        {
            var selected = gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto;
            if (selected != null && selected.SourceType != "PHATSINH") { await OpenSource(selected); return; }
            if (selected != null && selected.SourceType == "PHATSINH")
            {
                await OpenDetailDialog(selected);
                if (IsDisposed || Disposing) return;
            }
        }

        internal static System.Collections.Generic.List<PayrollOccurrenceDto> FilterBySource(System.Collections.Generic.IEnumerable<PayrollOccurrenceDto> items, string filter)
        {
            string source = filter == "Khoản phát sinh nhập riêng" ? "PHATSINH" : filter == "Phụ cấp hợp đồng" ? "PHUCAP" : filter == "Ứng lương" ? "UNGLUONG" : null;
            return items.Where(x => source == null || x.SourceType == source).ToList();
        }
        private async Task OpenSource(PayrollOccurrenceDto selected)
        {
            if (selected == null || selected.SourceType == "PHATSINH") return;
            string capability = selected.SourceType == "PHUCAP" ? "F_CC_PHUCAP" : "F_CC_UNGLUONG";
            if (!Functions.FormSecurity.AssertPermission(capability, Bu.DTO.PermissionAction.View)) return;
            try {
                using (Form child = selected.SourceType == "PHUCAP" ? (Form)new FrmPhuCap { InitialEmployeeId = selected.MaNV } : new FrmUngLuong { InitialEmployeeId = selected.MaNV, InitialSourceId = int.TryParse(selected.SourceId, out int sourceId) ? sourceId : 0 })
                    await Functions.ChildFormWindow.ShowAsync(this, child);
                if (!IsDisposed) await LoadData();
            } catch (OperationCanceledException) {} catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private async Task OpenDetailDialog(PayrollOccurrenceDto selected)
        {
            try
            {
            if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ trước khi thao tác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var frm = new FrmChiTietPhatSinh(selected, makycong);
            try { await Functions.ChildFormWindow.ShowAsync(this, frm); } finally { frm.Dispose(); }
            if (!IsDisposed && !Disposing) await LoadData();
        
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnAddDraftOccurrenceClicked(sender, EventArgs.Empty);
        }

        private async void OnAddDraftOccurrenceClicked(object sender, EventArgs e)
        {
            try
            {
            if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ trước khi thêm phát sinh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var frm = new FrmChiTietPhatSinh(makycong);
            try { await Functions.ChildFormWindow.ShowAsync(this, frm); } finally { frm.Dispose(); }
            if (!IsDisposed && !Disposing) await LoadData();
        
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private async void cboKyCong_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadData();
            if (IsDisposed || Disposing) return;
        }

        private async void cboLoaiPhatSinh_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadData();
            if (IsDisposed || Disposing) return;
        }

        private async Task LoadData()
        {
            if (_loadingFilters) return;
            int loadVersion = ++_loadVersion;
            try
            {
            try
            {
                if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
                {
                    gcPhatSinh.DataSource = null;
                    UpdateOccurrenceActions(null);
                    return;
                }

                string[] filters = { "Khoản phát sinh nhập riêng", "Phụ cấp hợp đồng", "Ứng lương", "Tất cả nguồn", "Thu nhập phát sinh", "Khấu trừ phát sinh" };
                string filterType = cboLoaiPhatSinh.SelectedIndex >= 0 && cboLoaiPhatSinh.SelectedIndex < filters.Length ? filters[cboLoaiPhatSinh.SelectedIndex] : filters[0];
                var items = await Functions.FormDataLoader.ReadAsync(this, () => _occurrenceService.GetOccurrences(makycong, filterType == "Thu nhập phát sinh" || filterType == "Khấu trừ phát sinh" ? filterType : "Tất cả"));
                if (loadVersion != _loadVersion || IsDisposed) return;
                gcPhatSinh.DataSource = FilterBySource(items, filterType);
                UpdateOccurrenceActions(gvPhatSinh.GetFocusedRow() as PayrollOccurrenceDto);
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                string userMsg = ErrorHelper.ResolveUserFriendlyMessage(ex, "tải danh sách phát sinh lương");
                XtraMessageBox.Show(userMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private async void btnDuyet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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
                    await RevokeSelectedOccurrence(selected);
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
                await Functions.FormDataLoader.RunAsync(this, () => _occurrenceService.ApproveOccurrence(selected.SourceId, userId, selected.DataVersion));
                XtraMessageBox.Show($"Đã phê duyệt thành công khoản phát sinh [{selected.TenKhoan}] của nhân viên [{selected.HoTen}]!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Phê duyệt khoản phát sinh");
                XtraMessageBox.Show(msg, "Lỗi phê duyệt", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnThuHoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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

            await RevokeSelectedOccurrence(selected);
        }

        private async Task RevokeSelectedOccurrence(PayrollOccurrenceDto selected)
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
                await Functions.FormDataLoader.RunAsync(this, () => _occurrenceService.RevokeOccurrence(selected.SourceId, userId, lyDo.Trim(), selected.DataVersion));
                XtraMessageBox.Show($"Đã thu hồi phê duyệt khoản phát sinh [{selected.TenKhoan}]. Khoản này đã chuyển về trạng thái [Đã thu hồi / Hủy] và bị loại khỏi lần tính lương tiếp theo.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Thu hồi phê duyệt phát sinh");
                XtraMessageBox.Show(msg, "Lỗi thu hồi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnXoaMem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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
                await Functions.FormDataLoader.RunAsync(this, () => _occurrenceService.DeleteOccurrence(selected.SourceId, userId, selected.DataVersion));
                XtraMessageBox.Show("Đã chuyển khoản phát sinh vào Thùng rác thành công. Bạn có thể khôi phục trong Quản trị dữ liệu > Thùng rác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Xóa khoản phát sinh");
                XtraMessageBox.Show(msg, "Lỗi xóa phát sinh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLamMoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await LoadData();
            if (IsDisposed || Disposing) return;
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
    }
}
