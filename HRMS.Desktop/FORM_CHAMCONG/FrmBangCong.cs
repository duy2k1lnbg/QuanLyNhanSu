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
        }

        private KYCONG _kycong;
        private bool _them;
        private int _MAKYCONG;
        private bool _isSaving = false;

        private void FrmBangCong_Load(object sender, EventArgs e)
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
            for (int i = 1; i <= 12; i++)
            {
                cboThang.Items.Add(i.ToString());
            }
            for (int i = 2020; i <= 2030; i++)
            {
                cboNam.Items.Add(i.ToString());
            }

            LoadData();
            Functions.TranslationManager.Translate(this);
        }

        private void gvDanhSach_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.HitInfo.InRow || e.HitInfo.InRowCell)
            {
                int makycong = Convert.ToInt32(gvDanhSach.GetRowCellValue(e.HitInfo.RowHandle, "MAKYCONG"));
                int isKhoa = Convert.ToInt32(gvDanhSach.GetRowCellValue(e.HitInfo.RowHandle, "KHOA") ?? 0);

                if (isKhoa == 1)
                {
                    var itemUnlock = new DXMenuItem("Mở khóa kỳ công...", (s, args) => ThaoTacMoKhoaKyCong(makycong));
                    e.Menu.Items.Add(itemUnlock);
                }
                else
                {
                    var itemLock = new DXMenuItem("Khóa kỳ công...", (s, args) => ThaoTacKhoaKyCong(makycong));
                    e.Menu.Items.Add(itemLock);
                }
            }
        }

        private void ThaoTacKhoaKyCong(int makycong)
        {
            if (!UserSession.IsLoggedIn)
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
                _kycong.LockKyCong(makycong, userId);
                XtraMessageBox.Show($"Đã khóa kỳ công {makycong} thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
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

        private void ThaoTacMoKhoaKyCong(int makycong)
        {
            if (!UserSession.IsLoggedIn)
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
                _kycong.UnlockKyCong(makycong, userId, lyDo.Trim());
                XtraMessageBox.Show($"Đã mở khóa kỳ công {makycong} thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
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

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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
                    LoadData();
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

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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
                        LoadData();
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
            cboThang.Enabled = !kt;
            cboNam.Enabled = !kt;
            chkKhoa.Enabled = false; // Indicator only, not directly editable
            chkTrangThai.Enabled = false; // Indicator only
        }

        private void LoadData()
        {
            _kycong = new KYCONG();
            gcDanhSach.DataSource = _kycong.getList();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
            SyncFocusedRow();
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
                    e.DisplayText = "Đã Tạo";
                }
                else
                {
                    e.DisplayText = "Chưa Tạo";
                }
            }
        }

        private void btnXemBC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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
                frm.ShowDialog();
            }
            else
            {
                XtraMessageBox.Show("Vui lòng chọn một kỳ công để xem chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
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

                    object ttVal = gvDanhSach.GetFocusedRowCellValue("TRANGTHAI");
                    chkTrangThai.Checked = ttVal != null && (ttVal.ToString() == "1" || ttVal.ToString().Equals("true", StringComparison.OrdinalIgnoreCase));
                }
            }
        }
    }
}