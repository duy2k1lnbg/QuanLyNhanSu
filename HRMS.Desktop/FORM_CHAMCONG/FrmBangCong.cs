using System.Threading.Tasks;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using QLyNSu.Functions;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmBangCong : DevExpress.XtraEditors.XtraForm
    {
        public FrmBangCong()
        {
            InitializeComponent();
            _periodLock.Id = barManager1.GetNewItemId();
            barManager1.Items.Add(_periodLock); bar1.AddItem(_periodLock);
            _periodLock.ItemClick += async (sender, args) => await ChangePeriodLock();
            chkKhoa.Text = "Đã khóa"; chkTrangThai.Text = "Đã công bố";
            var periodFields = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12,8,12,8), AutoScroll = true, WrapContents = true };
            splitContainer1.Panel1.Controls.Clear();
            labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Default;
            labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Default;
            labelControl1.Text = "Năm:"; labelControl2.Text = "Tháng:";
            cboNam.Properties.TextEditStyle = cboThang.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cboNam.Properties.AutoHeight = cboThang.Properties.AutoHeight = false;
            cboNam.Height = cboThang.Height = 30;
            cboNam.Properties.Items.Clear(); cboThang.Properties.Items.Clear();
            for (int year = 2000; year <= 2100; year++) cboNam.Properties.Items.Add(year.ToString());
            for (int month = 1; month <= 12; month++) cboThang.Properties.Items.Add(month.ToString());
            cboNam.Width = 120; cboThang.Width = 85;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.SplitterWidth = 1;
            splitContainer1.SplitterDistance = 64;
            foreach (Control field in new Control[] { labelControl1, cboNam, labelControl2, cboThang, chkTrangThai, chkKhoa }) {
                field.Margin = new Padding(4,8,16,8);
                field.Anchor = AnchorStyles.None;
                periodFields.Controls.Add(field);
            }
            splitContainer1.Panel1.Controls.Add(periodFields);
            Functions.DesktopTypography.Apply(this, barManager1);
            periodFields.SizeChanged += (sender,args) => AdjustPeriodHeader(periodFields);
            AdjustPeriodHeader(periodFields);
            ConfigurePeriodColumns();
            gcDanhSach.SizeChanged += (sender,args) => ConfigurePeriodColumns();
            UpdateLockAction();
        }

        private void AdjustPeriodHeader(FlowLayoutPanel fields)
        {
            if (splitContainer1.Height < 160 || fields.Width <= 0) return;
            int preferred = fields.GetPreferredSize(new Size(fields.Width, 0)).Height;
            int height = Math.Max(58, Math.Min(preferred, splitContainer1.Height / 3));
            if (splitContainer1.SplitterDistance != height) splitContainer1.SplitterDistance = height;
        }
        private void ConfigurePeriodColumns()
        {
            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gvDanhSach.Columns) {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
            NGAYTINHCONG.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            NGAYTINHCONG.DisplayFormat.FormatString = "dd/MM/yyyy";
            bool compact = gcDanhSach.ClientSize.Width < 1000;
            DELETED_BY.Width = compact ? 28 : 36; DELETED_BY.OptionsColumn.FixedWidth = true;
            MAKYCONG.Width = compact ? 100 : 130; MAKYCONG.OptionsColumn.FixedWidth = true;
            NAM.Width = compact ? 60 : 80; NAM.OptionsColumn.FixedWidth = true;
            THANG.Width = 70; THANG.OptionsColumn.FixedWidth = true;
            NGAYTINHCONG.Width = compact ? 110 : 135; NGAYTINHCONG.OptionsColumn.FixedWidth = true;
            NGAYCONGTRONGTHANG.Width = compact ? 120 : 165; NGAYCONGTRONGTHANG.OptionsColumn.FixedWidth = true;
            TRANGTHAI.MinWidth = compact ? 120 : 145; KHOA.MinWidth = compact ? 96 : 115;
            gvDanhSach.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            gvDanhSach.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
        }

        private readonly DevExpress.XtraBars.BarButtonItem _periodLock = new DevExpress.XtraBars.BarButtonItem();
        private bool _locking;
        private void UpdateLockAction()
        {
            _periodLock.Caption = chkKhoa.Checked ? "Mở khóa kỳ công" : "Khóa kỳ công";
            _periodLock.Enabled = !_locking && _MAKYCONG > 0 && !btnLuu.Enabled && UserSession.CanEdit("F_CC_BANGCONG");
        }
        private async Task ChangePeriodLock()
        {
            if (_locking || _MAKYCONG <= 0 || !UserSession.CanEdit("F_CC_BANGCONG")) return;
            int period = _MAKYCONG; bool locked = chkKhoa.Checked;
            _locking = true; UpdateLockAction();
            try { if (locked) await ThaoTacMoKhoaKyCong(period); else await ThaoTacKhoaKyCong(period); }
            finally { _locking = false; if (!IsDisposed) UpdateLockAction(); }
        }
        private KYCONG _kycong;
        private bool _them;
        public int InitialPeriod { get; set; }
        private int _MAKYCONG;
        private bool _isSaving = false;

        private async void FrmBangCong_Load(object sender, EventArgs e)
        {
            _them = false;
            _kycong = new KYCONG();
            showHide(true);

            // Hook context menu for Lock / Unlock operations
            gvDanhSach.PopupMenuShowing += gvDanhSach_PopupMenuShowing;

            chkKhoa.Enabled = false;
            chkTrangThai.Enabled = false;

            cboNam.Text = DateTime.Now.Year.ToString();
            cboThang.Text = DateTime.Now.Month.ToString();
            if (Bu.CLASS_PAYROLL.PayrollReadinessService.IsValidPeriod(InitialPeriod)) _MAKYCONG = InitialPeriod;
            await LoadData();
            if (Bu.CLASS_PAYROLL.PayrollReadinessService.IsValidPeriod(InitialPeriod))
            {
                cboNam.Text = (InitialPeriod / 100).ToString();
                cboThang.Text = (InitialPeriod % 100).ToString();
            }

            if (IsDisposed || Disposing) return;
            Functions.TranslationManager.Translate(this);
        }

        private void gvDanhSach_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.HitInfo.InRow || e.HitInfo.InRowCell)
            {
                gvDanhSach.FocusedRowHandle = e.HitInfo.RowHandle;
                SyncFocusedRow();
                int makycong = _MAKYCONG;
                int isKhoa = Convert.ToInt32(gvDanhSach.GetRowCellValue(e.HitInfo.RowHandle, "KHOA") ?? 0);

                if (isKhoa == 1)
                {
                    var itemUnlock = new DXMenuItem("Mở khóa kỳ công...", async (s, args) => await ChangePeriodLock());
                    itemUnlock.Enabled = !_locking && !btnLuu.Enabled && UserSession.CanEdit("F_CC_BANGCONG");
                    e.Menu.Items.Add(itemUnlock);
                }
                else
                {
                    var itemLock = new DXMenuItem("Khóa kỳ công...", async (s, args) => await ChangePeriodLock());
                    itemLock.Enabled = !_locking && !btnLuu.Enabled && UserSession.CanEdit("F_CC_BANGCONG");
                    e.Menu.Items.Add(itemLock);
                }
            }
        }

        private async Task ThaoTacKhoaKyCong(int makycong)
        {
            if (!UserSession.IsLoggedIn || !UserSession.CanEdit("F_CC_BANGCONG"))
            {
                XtraMessageBox.Show("Yêu cầu phiên đăng nhập hợp lệ để khóa kỳ công.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (XtraMessageBox.Show($"Bạn có chắc chắn muốn khóa sổ kỳ công {makycong} không?\nSau khi khóa, dữ liệu công và lương của kỳ sẽ được bảo vệ chống chỉnh sửa.",
                "Xác nhận khóa kỳ công", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int userId = (int)UserSession.CurrentUser.IDUSER;
                var result = await FormDataLoader.ReadAsync(this, () => _kycong.TryLockKyCong(makycong, userId));
                if (IsDisposed || Disposing) return;
                if (!result.Success) { await ShowLockFailure(result); return; }
                XtraMessageBox.Show($"Đã khóa kỳ công {makycong} thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
            catch (OperationCanceledException) { return; }
            catch (BusinessException be)
            {
                XtraMessageBox.Show(be.Message, "Cảnh báo nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "khóa kỳ công");
                XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ShowLockFailure(PeriodLockResult result)
        {
            if (result.ErrorCode == "LOCK_NOT_READY") {
                if (XtraMessageBox.Show(this, result.Message + "\n\nMở quy trình để xử lý kỳ này?", "Kỳ công chưa đủ điều kiện khóa",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    await ChildFormWindow.ShowAsync(this, new FrmPayrollWorkflow(result.Period));
            }
            else XtraMessageBox.Show(this, Functions.TranslationManager.Translate(result.Message), "Không thể đổi trạng thái kỳ " + result.Period,
                MessageBoxButtons.OK, result.ErrorCode == "DATABASE_ERROR" ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
        }

        private async Task ThaoTacMoKhoaKyCong(int makycong)
        {
            if (!UserSession.IsLoggedIn || !UserSession.CanEdit("F_CC_BANGCONG"))
            {
                XtraMessageBox.Show("Yêu cầu phiên đăng nhập hợp lệ để mở khóa kỳ công.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string lyDo = XtraInputBox.Show("Vui lòng nhập lý do mở khóa kỳ công (bắt buộc):", "Lý do mở khóa kỳ công", "");
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                XtraMessageBox.Show("Thao tác mở khóa bị hủy: Lý do mở khóa là bắt buộc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int userId = (int)UserSession.CurrentUser.IDUSER;
                var result = await FormDataLoader.ReadAsync(this, () => _kycong.TryUnlockKyCong(makycong, userId, lyDo.Trim()));
                if (IsDisposed || Disposing) return;
                if (!result.Success) { await ShowLockFailure(result); return; }
                XtraMessageBox.Show($"Đã mở khóa kỳ công {makycong} thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
            catch (OperationCanceledException) { return; }
            catch (BusinessException be)
            {
                XtraMessageBox.Show(be.Message, "Cảnh báo nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "mở khóa kỳ công");
                XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gvDanhSach_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.Name == "DELETED_BY")
            {
                Image img;

                if (e.CellValue != null)
                {
                    img = Properties.Resources.del;
                }
                else
                {
                    img = Properties.Resources.no_del;
                }
                Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.DrawImage(img, rect);
                e.Handled = true;
            }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = true;
            showHide(false);
            cboNam.Text = DateTime.Now.Year.ToString();
            cboThang.Text = DateTime.Now.Month.ToString();
            chkKhoa.Checked = false;
            chkTrangThai.Checked = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_MAKYCONG <= 0)
            {
                XtraMessageBox.Show("Vui lòng chọn một kỳ công để chỉnh sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var kc = _kycong.getItem(_MAKYCONG);
            if (kc == null)
            {
                XtraMessageBox.Show($"Không tìm thấy kỳ công {_MAKYCONG}.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((kc.KHOA ?? 0) == 1)
            {
                XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khóa sổ để bảo vệ số liệu. Không thể sửa trực tiếp kỳ đã khóa.\nĐể mở khóa, nhấp chuột phải vào dòng kỳ công trên danh sách và chọn 'Mở khóa kỳ công...'",
                    "Kỳ công đã khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _them = false;
            showHide(false);
        }

        private async void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_MAKYCONG <= 0)
            {
                XtraMessageBox.Show("Vui lòng chọn một kỳ công để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var kc = _kycong.getItem(_MAKYCONG);
            if (kc == null)
            {
                XtraMessageBox.Show($"Không tìm thấy kỳ công {_MAKYCONG}.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((kc.KHOA ?? 0) == 1)
            {
                XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khoá. Khi đã khoá thì không được chuyển vào thùng rác!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (XtraMessageBox.Show($"Bạn có chắc chắn muốn chuyển kỳ công {_MAKYCONG} (tháng {kc.THANG}/{kc.NAM}) vào Thùng rác không?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int userId = (int)(UserSession.CurrentUser?.IDUSER ?? 0);
                    _kycong.Delete(_MAKYCONG, userId);
                    XtraMessageBox.Show($"Đã chuyển kỳ công {_MAKYCONG} vào Thùng rác thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                    if (IsDisposed || Disposing) return;
                }
                catch (BusinessException be)
                {
                    XtraMessageBox.Show(be.Message, "Cảnh báo nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "xóa kỳ công");
                    XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;
            btnLuu.Enabled = false;

            try
            {
                if (SaveData())
                {
                    _them = false;
                    showHide(true);
                    try
                    {
                        await LoadData();
                        if (IsDisposed || Disposing) return;
                    }
                    catch (Exception loadEx)
                    {
                        XtraMessageBox.Show("Dữ liệu kỳ công đã được lưu thành công nhưng chưa thể làm mới danh sách tự động. Vui lòng bấm nút 'Tải lại' (Refresh).",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            finally
            {
                _isSaving = false;
                if (!_them) btnLuu.Enabled = false;
                else btnLuu.Enabled = true;
            }
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
            SyncFocusedRow();
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnThem.Enabled = kt;
            btnXoa.Enabled = kt;
            btnSua.Enabled = kt;
            btnIn.Enabled = kt;
            btnDong.Enabled = kt;
            cboThang.Properties.ReadOnly = cboNam.Properties.ReadOnly = kt;
            cboThang.Properties.AllowFocused = cboNam.Properties.AllowFocused = !kt;
            cboThang.TabStop = cboNam.TabStop = !kt;
            UpdateLockAction();
            chkKhoa.Enabled = false; // Locking uses the explicit toolbar action
            chkTrangThai.Enabled = false; // Indicator only
        }

        private async Task LoadData()
        {
            try
            {
            int selectedPeriod = _MAKYCONG;
            var reader = new KYCONG();
            var periods = await Functions.FormDataLoader.ReadAsync(this, () => reader.getList());
            gcDanhSach.DataSource = periods;
            if (selectedPeriod > 0)
            {
                int handle = gvDanhSach.LocateByValue("MAKYCONG", selectedPeriod);
                if (handle >= 0) gvDanhSach.FocusedRowHandle = handle;
            }
            TRANGTHAI.Caption = TranslationManager.Translate("Công bố bảng công");
            chkTrangThai.Text = TranslationManager.Translate("Đã công bố công");
            FormManager_Functions.CustomView_Colums(gvDanhSach);
            ConfigurePeriodColumns();
            SyncFocusedRow();

            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private bool SaveData()
        {
            if (!UserSession.IsLoggedIn && UserSession.CurrentUser == null)
            {
                XtraMessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(cboNam.Text, out int nam) || nam < 2000 || nam > 2100)
            {
                XtraMessageBox.Show("Năm không hợp lệ. Vui lòng chọn năm từ 2000 đến 2100.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboNam.Focus();
                return false;
            }

            if (!int.TryParse(cboThang.Text, out int thang) || thang < 1 || thang > 12)
            {
                XtraMessageBox.Show("Tháng không hợp lệ. Vui lòng chọn tháng từ 1 đến 12.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboThang.Focus();
                return false;
            }

            int userId = (int)(UserSession.CurrentUser?.IDUSER ?? 0);

            try
            {
                if (_them)
                {
                    int makycong = nam * 100 + thang;
                    TB_KYCONG kc = new TB_KYCONG
                    {
                        MAKYCONG = makycong,
                        NAM = nam,
                        THANG = thang,
                        KHOA = 0,
                        TRANGTHAI = 0,
                        NGAYCONGTRONGTHANG = ChamCong_Functions.demSoNgayLamViecTrongThang(thang, nam),
                        NGAYTINHCONG = DateTime.Now,
                        CREATED_BY = userId,
                        CREATED_DATE = DateTime.Now
                    };

                    _kycong.Add(kc);
                    XtraMessageBox.Show($"Tạo kỳ công {makycong} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    var kc = _kycong.getItem(_MAKYCONG);
                    if (kc == null)
                    {
                        XtraMessageBox.Show("Không tìm thấy kỳ công với ID: " + _MAKYCONG, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if ((kc.KHOA ?? 0) == 1)
                    {
                        XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khoá. Vui lòng mở khóa trước khi sửa.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    kc.NAM = nam;
                    kc.THANG = thang;
                    kc.NGAYCONGTRONGTHANG = ChamCong_Functions.demSoNgayLamViecTrongThang(thang, nam);
                    kc.NGAYTINHCONG = DateTime.Now;
                    kc.UPDATED_BY = userId;
                    kc.UPDATED_DATE = DateTime.Now;

                    _kycong.Update(kc);
                    XtraMessageBox.Show($"Cập nhật kỳ công {_MAKYCONG} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            catch (BusinessException be)
            {
                XtraMessageBox.Show(be.Message, "Cảnh báo nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "lưu kỳ công");
                XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void gvDanhSach_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "KHOA")
            {
                if (e.Value != null && e.Value.ToString() == "1")
                {
                    e.DisplayText = "Đã khoá";
                }
                else
                {
                    e.DisplayText = "Chưa khoá";
                }
            }
            if (e.Column.FieldName == "TRANGTHAI")
            {
                if (e.Value != null && e.Value.ToString() == "1")
                {
                    e.DisplayText = TranslationManager.Translate("Đã công bố");
                }
                else
                {
                    e.DisplayText = TranslationManager.Translate("Chưa công bố");
                }
            }
        }

        private async void btnXemBC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
            if (_MAKYCONG == 0 && gvDanhSach.RowCount > 0 && gvDanhSach.FocusedRowHandle >= 0)
            {
                object val = gvDanhSach.GetFocusedRowCellValue("MAKYCONG");
                if (val != null) _MAKYCONG = Convert.ToInt32(val);
            }

            if (_MAKYCONG > 0)
            {
                FrmBangCong_ChiTiet frm = new FrmBangCong_ChiTiet();
                frm._MAKYCONG = _MAKYCONG;
                frm._nam = _MAKYCONG / 100;
                frm._thang = _MAKYCONG % 100;
                try { await Functions.ChildFormWindow.ShowAsync(this, frm); } finally { frm.Dispose(); }
                if (IsDisposed || Disposing) return;
                await LoadData();
            }
            else
            {
                XtraMessageBox.Show("Vui lòng chọn một kỳ công để xem chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private async void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await LoadData();
            if (IsDisposed || Disposing) return;
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            SyncFocusedRow();
        }

        private void gvDanhSach_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            SyncFocusedRow();
        }

        private void SyncFocusedRow()
        {
            if (!cboNam.Properties.ReadOnly) return; // Do not replace values while editing a period.
            _MAKYCONG = 0;
            if (gvDanhSach.RowCount > 0 && gvDanhSach.FocusedRowHandle >= 0)
            {
                object val = gvDanhSach.GetFocusedRowCellValue("MAKYCONG");
                if (val != null && int.TryParse(val.ToString(), out int makycong))
                {
                    _MAKYCONG = makycong;
                    cboNam.Text = gvDanhSach.GetFocusedRowCellValue("NAM")?.ToString() ?? "";
                    cboThang.Text = gvDanhSach.GetFocusedRowCellValue("THANG")?.ToString() ?? "";

                    object khoaVal = gvDanhSach.GetFocusedRowCellValue("KHOA");
                    chkKhoa.Checked = khoaVal != null && (khoaVal.ToString() == "1" || khoaVal.ToString().Equals("true", StringComparison.OrdinalIgnoreCase));
                    UpdateLockAction();

                    object ttVal = gvDanhSach.GetFocusedRowCellValue("TRANGTHAI");
                    chkTrangThai.Checked = ttVal != null && (ttVal.ToString() == "1" || ttVal.ToString().Equals("true", StringComparison.OrdinalIgnoreCase));
                }
            }
        }
    }
}
