using Bu;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DevExpress.XtraEditors;
using System;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmChiTietPhatSinh : DevExpress.XtraEditors.XtraForm
    {
        private readonly PayrollOccurrenceService _occurrenceService = new PayrollOccurrenceService();
        private readonly NHANVIEN _nhanVienBus = new NHANVIEN();
        private readonly int _makycong;
        private PayrollOccurrenceDto _existingItem;
        private bool _isDirty = false;
        private bool _isSaving = false;
        private bool _isLoading = false;
        private bool _isProcessing = false;

        public FrmChiTietPhatSinh(int makycong)
        {
            InitializeComponent();
            _makycong = makycong;
            _existingItem = null;
        }

        public FrmChiTietPhatSinh(PayrollOccurrenceDto item, int makycong)
        {
            InitializeComponent();
            _makycong = makycong;
            _existingItem = item;
        }

        private void FrmChiTietPhatSinh_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                // Load danh sách nhân viên
                var listNv = _nhanVienBus.getListFll_DTO();
                lookUpNhanVien.Properties.DataSource = listNv;
                lookUpNhanVien.Properties.ValueMember = "MANV";
                lookUpNhanVien.Properties.DisplayMember = "HOTEN";

                cboLoai.Properties.Items.Clear();
                cboLoai.Properties.Items.Add("1 - Thu nhập phát sinh");
                cboLoai.Properties.Items.Add("2 - Khấu trừ phát sinh");

                txtKyCong.Text = _makycong.ToString();

                if (_existingItem == null)
                {
                    // Thêm mới: KHÔNG mặc định nhân viên là 1, để trống để người dùng chọn
                    lookUpNhanVien.EditValue = null;
                    cboLoai.SelectedIndex = 0;
                    txtTenKhoan.Text = "Thưởng thành tích phát sinh";
                    spSoTien.Value = 500000m;
                    dtNgayPhatSinh.DateTime = DateTime.Today;
                    txtSoChungTu.Text = string.Empty;
                    txtTrangThai.Text = "Bản nháp";
                    txtLyDo.Text = string.Empty;

                    btnDuyet.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                    btnThuHoi.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                }
                else
                {
                    // Sửa hoặc Xem chi tiết
                    lookUpNhanVien.EditValue = _existingItem.MaNV;
                    lookUpNhanVien.Properties.ReadOnly = true;

                    bool isDeduction = _existingItem.Nhom == "Khấu trừ phát sinh";
                    cboLoai.SelectedIndex = isDeduction ? 1 : 0;
                    txtTenKhoan.Text = _existingItem.TenKhoan;
                    spSoTien.Value = _existingItem.SoTien;
                    dtNgayPhatSinh.DateTime = _existingItem.NgayPhatSinh != DateTime.MinValue ? _existingItem.NgayPhatSinh : DateTime.Today;
                    txtSoChungTu.Text = _existingItem.SoChungTu;
                    txtSoChungTu.Properties.ReadOnly = true;
                    txtTrangThai.Text = _existingItem.TrangThai;
                    txtLyDo.Text = _existingItem.LyDo;

                    bool isApproved = _existingItem.TrangThai == "Đã duyệt";
                    bool isDraft = _existingItem.TrangThai.Contains("Bản nháp");
                    bool hasPerm = UserSession.IsAdmin || UserSession.CanEdit("F_CC_BANGLUONG");

                    btnDuyet.Visibility = (isDraft && hasPerm) ? DevExpress.XtraBars.BarItemVisibility.Always : DevExpress.XtraBars.BarItemVisibility.Never;
                    btnThuHoi.Visibility = (isApproved && hasPerm) ? DevExpress.XtraBars.BarItemVisibility.Always : DevExpress.XtraBars.BarItemVisibility.Never;

                    if (isApproved)
                    {
                        // Đã duyệt: Khóa chỉnh sửa trực tiếp, phải thu hồi trước
                        btnLuuNhap.Enabled = false;
                        cboLoai.Properties.ReadOnly = true;
                        txtTenKhoan.Properties.ReadOnly = true;
                        spSoTien.Properties.ReadOnly = true;
                        dtNgayPhatSinh.Properties.ReadOnly = true;
                        txtLyDo.Properties.ReadOnly = true;
                        lblNote.Text = "* Khoản phát sinh này [Đã duyệt]. Để chỉnh sửa thông tin, vui lòng thực hiện 'Thu hồi phê duyệt' trước.";
                    }
                }

                // Gắn sự kiện dirty tracking
                lookUpNhanVien.EditValueChanged += (s, ev) => { if (!_isLoading) _isDirty = true; };
                cboLoai.SelectedIndexChanged += (s, ev) => { if (!_isLoading) _isDirty = true; };
                txtTenKhoan.TextChanged += (s, ev) => { if (!_isLoading) _isDirty = true; };
                spSoTien.ValueChanged += (s, ev) => { if (!_isLoading) _isDirty = true; };
                dtNgayPhatSinh.EditValueChanged += (s, ev) => { if (!_isLoading) _isDirty = true; };
                txtLyDo.TextChanged += (s, ev) => { if (!_isLoading) _isDirty = true; };

                Functions.TranslationManager.Translate(this);
            }
            finally
            {
                _isLoading = false;
                _isDirty = false;
            }
        }

        private bool SaveDraft(bool showSuccessMessage = true)
        {
            if (_isProcessing) return false;

            // 1. Kiểm tra bắt buộc nhân viên (không chấp nhận trống hoặc <= 0)
            if (lookUpNhanVien.EditValue == null || !int.TryParse(lookUpNhanVien.EditValue.ToString(), out int manv) || manv <= 0)
            {
                XtraMessageBox.Show("Vui lòng chọn nhân viên nhận phát sinh lương từ danh sách!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lookUpNhanVien.Focus();
                return false;
            }

            // 2. Kiểm tra số tiền
            if (spSoTien.Value <= 0)
            {
                XtraMessageBox.Show("Số tiền phát sinh phải lớn hơn 0 đồng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spSoTien.Focus();
                return false;
            }

            // 3. Kiểm tra lý do bắt buộc
            if (string.IsNullOrWhiteSpace(txtLyDo.Text))
            {
                XtraMessageBox.Show("Vui lòng nhập lý do / căn cứ phát sinh bắt buộc!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLyDo.Focus();
                return false;
            }

            // 4. Kiểm tra phiên đăng nhập
            int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 0;
            if (userId <= 0)
            {
                XtraMessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int loai = (cboLoai.SelectedIndex == 1) ? 2 : 1;
            string tenKhoan = string.IsNullOrWhiteSpace(txtTenKhoan.Text) ? (loai == 1 ? "Thưởng phát sinh" : "Khấu trừ phát sinh") : txtTenKhoan.Text.Trim();
            DateTime ngayPhatSinh = dtNgayPhatSinh.DateTime != DateTime.MinValue ? dtNgayPhatSinh.DateTime : DateTime.Today;

            var input = new PayrollOccurrenceInput
            {
                MaNV = manv,
                MaKyCong = _makycong,
                Loai = loai,
                TenKhoan = tenKhoan,
                SoTien = spSoTien.Value,
                LyDo = txtLyDo.Text.Trim(),
                NgayPhatSinh = ngayPhatSinh,
                SoChungTu = txtSoChungTu.Text.Trim()
            };

            _isProcessing = true;
            btnLuuNhap.Enabled = false;

            try
            {
                if (_existingItem == null)
                {
                    string soqd = _occurrenceService.SaveDraftOccurrence(input, userId);
                    txtSoChungTu.Text = soqd;
                    _existingItem = new PayrollOccurrenceDto
                    {
                        SourceType = "PHATSINH",
                        SourceId = soqd,
                        MaNV = manv,
                        HoTen = lookUpNhanVien.Text,
                        Nhom = loai == 1 ? "Thu nhập phát sinh" : "Khấu trừ phát sinh",
                        TenKhoan = tenKhoan,
                        NgayPhatSinh = ngayPhatSinh,
                        SoLuong = 1,
                        DonVi = "khoản",
                        DonGia = spSoTien.Value,
                        SoTien = spSoTien.Value,
                        LyDo = input.LyDo,
                        SoChungTu = soqd,
                        TrangThai = "Bản nháp",
                        CanApprove = true,
                        CanRevoke = false,
                        CanDelete = true,
                        DataVersion = 1m
                    };
                    txtTrangThai.Text = "Bản nháp";
                    btnDuyet.Visibility = (UserSession.IsAdmin || UserSession.CanEdit("F_CC_BANGLUONG")) ? DevExpress.XtraBars.BarItemVisibility.Always : DevExpress.XtraBars.BarItemVisibility.Never;
                    if (showSuccessMessage)
                    {
                        XtraMessageBox.Show($"Đã tạo thành công khoản phát sinh nháp [{soqd}].\nKhoản này ở trạng thái [Bản nháp] và chỉ được đưa vào tính lương sau khi được duyệt.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    _occurrenceService.UpdateDraftOccurrence(input, userId, _existingItem.DataVersion);
                    _existingItem.SoTien = spSoTien.Value;
                    _existingItem.DonGia = spSoTien.Value;
                    _existingItem.TenKhoan = tenKhoan;
                    _existingItem.LyDo = input.LyDo;
                    _existingItem.NgayPhatSinh = ngayPhatSinh;
                    _existingItem.Nhom = loai == 1 ? "Thu nhập phát sinh" : "Khấu trừ phát sinh";
                    _existingItem.DataVersion = (_existingItem.DataVersion ?? 1m) + 1m;
                    if (showSuccessMessage)
                    {
                        XtraMessageBox.Show($"Đã cập nhật khoản phát sinh nháp [{input.SoChungTu}] thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                _isDirty = false;
                return true;
            }
            catch (Exception ex)
            {
                // Giữ nguyên dữ liệu đã nhập trên form khi gặp lỗi, không đóng form, không đặt _isDirty = false
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Lưu khoản phát sinh");
                XtraMessageBox.Show(msg, "Lỗi lưu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                _isProcessing = false;
                btnLuuNhap.Enabled = true;
            }
        }

        private void btnLuuNhap_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (SaveDraft(showSuccessMessage: true))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnDuyet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_isProcessing) return;

            // Nếu form có thay đổi chưa lưu, không được duyệt bản cũ trong DB bằng thông tin mới trên màn hình
            if (_isDirty)
            {
                var askSave = XtraMessageBox.Show(
                    $"Dữ liệu khoản phát sinh đã được chỉnh sửa (Số tiền: {spSoTien.Value:N0} đ) nhưng chưa được lưu vào cơ sở dữ liệu.\n\nBạn có muốn lưu các thay đổi này thành bản nháp trước khi phê duyệt không?",
                    "Dữ liệu chưa lưu",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (askSave != DialogResult.Yes)
                {
                    XtraMessageBox.Show("Thao tác phê duyệt đã bị hủy do dữ liệu chưa được lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Lưu nháp trước khi duyệt
                bool saved = SaveDraft(showSuccessMessage: false);
                if (!saved)
                {
                    // Lưu thất bại: đã có thông báo lỗi từ SaveDraft, dừng lại và giữ nguyên form cho người dùng sửa
                    return;
                }
            }

            if (_existingItem == null || string.IsNullOrWhiteSpace(_existingItem.SourceId))
            {
                XtraMessageBox.Show("Không tìm thấy mã chứng từ hợp lệ để phê duyệt!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 0;
            if (userId <= 0)
            {
                XtraMessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Thông tin xác nhận duyệt lấy chính xác từ bản đã lưu
            var confirm = XtraMessageBox.Show(
                $"Xác nhận phê duyệt khoản phát sinh [{_existingItem.TenKhoan}] ({_existingItem.SoTien:N0} đ) của nhân viên [{lookUpNhanVien.Text}]?\n\nSau khi duyệt, khoản này sẽ được tính vào bảng lương chính thức.",
                "Xác nhận phê duyệt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            _isProcessing = true;
            btnDuyet.Enabled = false;

            try
            {
                _occurrenceService.ApproveOccurrence(_existingItem.SourceId, userId, _existingItem.DataVersion);
                _existingItem.DataVersion = (_existingItem.DataVersion ?? 1m) + 1m;
                XtraMessageBox.Show($"Đã phê duyệt thành công chứng từ [{_existingItem.SourceId}] ({_existingItem.SoTien:N0} đ)!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _isDirty = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Phê duyệt phát sinh");
                XtraMessageBox.Show(msg, "Lỗi phê duyệt", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isProcessing = false;
                btnDuyet.Enabled = true;
            }
        }

        private void btnThuHoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_isProcessing) return;

            if (_isDirty)
            {
                XtraMessageBox.Show("Dữ liệu đang có chỉnh sửa chưa lưu. Vui lòng lưu hoặc hủy thay đổi trước khi thu hồi phê duyệt!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_existingItem == null || string.IsNullOrWhiteSpace(_existingItem.SourceId))
            {
                XtraMessageBox.Show("Không tìm thấy mã chứng từ để thu hồi!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int userId = UserSession.CurrentUser != null ? (int)UserSession.CurrentUser.IDUSER : 0;
            if (userId <= 0)
            {
                XtraMessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string lyDo = XtraInputBox.Show("Nhập lý do thu hồi phê duyệt khoản phát sinh bắt buộc:", "Lý do thu hồi duyệt", "");
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                XtraMessageBox.Show("Lý do thu hồi là bắt buộc để lưu vết nghiệp vụ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isProcessing = true;
            btnThuHoi.Enabled = false;

            try
            {
                _occurrenceService.RevokeOccurrence(_existingItem.SourceId, userId, lyDo.Trim(), _existingItem.DataVersion);
                _existingItem.DataVersion = (_existingItem.DataVersion ?? 1m) + 1m;
                XtraMessageBox.Show($"Đã thu hồi phê duyệt khoản phát sinh [{_existingItem.SourceId}]. Khoản này đã chuyển về trạng thái [Đã thu hồi / Hủy] và bị loại khỏi lần tính lương tiếp theo.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _isDirty = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Thu hồi phê duyệt phát sinh");
                XtraMessageBox.Show(msg, "Lỗi thu hồi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isProcessing = false;
                btnThuHoi.Enabled = true;
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void FrmChiTietPhatSinh_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty && this.DialogResult != DialogResult.OK)
            {
                var ask = XtraMessageBox.Show("Dữ liệu có thay đổi chưa được lưu. Bạn có chắc chắn muốn đóng form không?", "Cảnh báo dữ liệu chưa lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
