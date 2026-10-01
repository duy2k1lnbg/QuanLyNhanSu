using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmCapNhatNgayCong : DevExpress.XtraEditors.XtraForm
    {
        public FrmCapNhatNgayCong()
        {
            InitializeComponent();
        }

        private KYCONGCHITIET _kcct;
        private BANGCONG_NV_CHITIET _bcct_nv;
        private KYCONG _kycong;

        public int _manv;
        public string _hoten;
        public int _MAKYCONG;
        public string _ngay;
        public int _cngay;
        public int thang_f1_bcct;
        public int nam_f_bcct1;

        // Dữ liệu ca làm việc và trạng thái
        private List<ShiftInfo> _allShifts;
        private ShiftInfo _currentShift;
        private DateTime _ngayDate;
        private bool _isLoading = false;

        // Tracking trạng thái giờ tự điền vs đã sửa tay
        private bool _isGioTuDien = true;
        private bool _isManualEdited = false;
        private bool _hasSavedHoursInDb = false;
        private bool _hasDevicePunchLog = false;
        private DateTime? _draftGioVao = null;
        private DateTime? _draftGioRa = null;

        private bool IsKyCongBiKhoa()
        {
            if (_kycong == null) _kycong = new KYCONG();
            var kc = _kycong.getItem(_MAKYCONG);
            return kc != null && ((kc.KHOA ?? 0) == 1 || (kc.TRANGTHAI ?? 0) == 1);
        }

        private void FrmCapNhatNgayCong_Load(object sender, EventArgs e)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(830, 575);
            this.MinimumSize = new Size(846, 614);
            this.MaximumSize = new Size(846, 614);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _kcct = new KYCONGCHITIET();
            _bcct_nv = new BANGCONG_NV_CHITIET();
            _kycong = new KYCONG();

            // Tải danh sách ca làm việc từ hệ thống
            _allShifts = AttendanceCalculationHelper.LoadAllShifts();
            cboCaLam.Properties.Items.Clear();
            foreach (var s in _allShifts)
            {
                cboCaLam.Properties.Items.Add(s.TenPhienBan);
            }

            try
            {
                int nam = 0;
                int thang = 0;
                int ngay = 0;

                // 1. Xác định năm và tháng an toàn
                if (_MAKYCONG >= 100000)
                {
                    nam = _MAKYCONG / 100;
                    thang = _MAKYCONG % 100;
                }
                else if (nam_f_bcct1 > 0 && thang_f1_bcct > 0)
                {
                    nam = nam_f_bcct1;
                    thang = thang_f1_bcct;
                    _MAKYCONG = nam * 100 + thang;
                }
                else
                {
                    nam = DateTime.Now.Year;
                    thang = DateTime.Now.Month;
                    _MAKYCONG = nam * 100 + thang;
                }

                lblKyCong.Text = _MAKYCONG.ToString();

                // 2. Xác định ngày từ _ngay (ví dụ: "D1", "D2", ..., "D31")
                if (!string.IsNullOrEmpty(_ngay))
                {
                    string dayStr = _ngay.StartsWith("D", StringComparison.OrdinalIgnoreCase)
                        ? _ngay.Substring(1)
                        : _ngay;
                    int.TryParse(dayStr, out ngay);
                }

                if (nam <= 0 || thang <= 0 || thang > 12 || ngay <= 0 || ngay > DateTime.DaysInMonth(nam, thang))
                {
                    XtraMessageBox.Show("Ngày hoặc kỳ công không hợp lệ. Vui lòng chọn một ô ngày công hợp lệ (D1-D31).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                _cngay = ngay;
                _ngayDate = new DateTime(nam, thang, ngay);
                cldNgayCong.SetDate(_ngayDate);

                // Tải dữ liệu ngày công
                LoadThongTinNgayCong(_cngay);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Vui lòng chọn đúng ô ngày công để cập nhật.\nLỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
            }
        }

        /// <summary>
        /// Tải thông tin ca làm, giờ quẹt thẻ và trạng thái công cho ngày được chọn
        /// </summary>
        private void LoadThongTinNgayCong(int ngay)
        {
            _isLoading = true;
            try
            {
                int nam = _MAKYCONG / 100;
                int thang = _MAKYCONG % 100;
                if (nam <= 0 || thang <= 0 || ngay <= 0 || ngay > DateTime.DaysInMonth(nam, thang)) return;

                _cngay = ngay;
                _ngayDate = new DateTime(nam, thang, ngay);

                lblMANV.Text = _manv.ToString();
                lblHoTen.Text = _hoten;
                lblNgay.Text = _ngayDate.ToString("dd/MM/yyyy");
                lblKyCong.Text = _MAKYCONG.ToString();

                // 1. Kiểm tra trạng thái khóa kỳ công
                bool isKhoa = IsKyCongBiKhoa();
                var v118 = _bcct_nv.GetChiTietNgayCongV118(_MAKYCONG, _manv, ngay);
                if (v118 != null && v118.KyCongKhoa == 1)
                {
                    isKhoa = true;
                }

                if (isKhoa)
                {
                    lblTrangThaiKhoa.Text = "🔒 Đã khóa (Chỉ xem)";
                    lblTrangThaiKhoa.ForeColor = Color.Crimson;
                    this.Text = $"Chi tiết ngày công [CHỈ XEM - KỲ CÔNG {_MAKYCONG} ĐÃ KHÓA]";
                    btnCapNhat.Enabled = false;
                    btnCapNhat.Text = "Đã Khóa (Chỉ Xem)";
                    btnApDungGioCa.Enabled = false;
                    cboCaLam.ReadOnly = true;
                    radDiLam.Enabled = false;
                    radNghi.Enabled = false;
                    chkCongTac.ReadOnly = true;
                    cboPhanNghi.ReadOnly = true;
                    cboLoaiNghi.ReadOnly = true;
                    txtLyDoNghi.ReadOnly = true;
                    timeEditGioVao.ReadOnly = true;
                    timeEditGioRa.ReadOnly = true;
                }
                else
                {
                    lblTrangThaiKhoa.Text = "🔓 Kỳ công mở (Được sửa)";
                    lblTrangThaiKhoa.ForeColor = Color.ForestGreen;
                    this.Text = "Cập nhật ngày công";
                    btnCapNhat.Enabled = true;
                    btnCapNhat.Text = "Cập Nhật";
                    btnApDungGioCa.Enabled = true;
                    cboCaLam.ReadOnly = false;
                    radDiLam.Enabled = true;
                    radNghi.Enabled = true;
                    chkCongTac.ReadOnly = false;
                    cboPhanNghi.ReadOnly = false;
                    cboLoaiNghi.ReadOnly = false;
                    txtLyDoNghi.ReadOnly = false;
                    timeEditGioVao.ReadOnly = false;
                    timeEditGioRa.ReadOnly = false;
                }

                // 2. Xác định ca làm việc cho ngày này
                _currentShift = AttendanceCalculationHelper.GetShiftForEmployee(_allShifts, _manv, _ngayDate);
                int shiftIndex = _allShifts.FindIndex(s => s.IdCaPhienBan == _currentShift.IdCaPhienBan);
                if (shiftIndex >= 0)
                {
                    cboCaLam.SelectedIndex = shiftIndex;
                }
                lblChiTietCa.Text = _currentShift.DisplayDescription;

                // Cập nhật danh sách phần nghỉ theo loại ca (ngày vs đêm)
                UpdatePhanNghiItems();

                // 3. Đọc dữ liệu từ TB_BANGCONG_CHITIET và TB_BANGCONG
                var currentBcct = _bcct_nv.getItem(_MAKYCONG, _manv, ngay);
                var raw = _bcct_nv.GetBangCongRaw(_manv, nam, thang, ngay);

                _hasDevicePunchLog = false;
                _hasSavedHoursInDb = false;
                _draftGioVao = null;
                _draftGioRa = null;

                // Xác định giờ vào / ra
                if (raw != null && raw.GIOVAO.HasValue && raw.GIORA.HasValue)
                {
                    int gv = (int)raw.GIOVAO.Value;
                    int pv = (int)(raw.PHUTVAO ?? 0);
                    int gr = (int)raw.GIORA.Value;
                    int pr = (int)(raw.PHUTRA ?? 0);
                    timeEditGioVao.Time = new DateTime(nam, thang, ngay, Math.Min(Math.Max(gv, 0), 23), Math.Min(Math.Max(pv, 0), 59), 0);
                    timeEditGioRa.Time = new DateTime(nam, thang, ngay, Math.Min(Math.Max(gr, 0), 23), Math.Min(Math.Max(pr, 0), 59), 0);
                    _hasSavedHoursInDb = true;
                    _hasDevicePunchLog = true;
                    _isGioTuDien = false;
                    _isManualEdited = false;
                    lblNguonDuLieu.Text = $"Nguồn: Máy chấm công ({gv:D2}:{pv:D2} - {gr:D2}:{pr:D2})";
                    lblNguonDuLieu.ForeColor = Color.ForestGreen;
                }
                else if (currentBcct != null && !string.IsNullOrEmpty(currentBcct.GIOVAO) && !string.IsNullOrEmpty(currentBcct.GIORA))
                {
                    if (TimeSpan.TryParse(currentBcct.GIOVAO, out TimeSpan tv))
                    {
                        timeEditGioVao.Time = new DateTime(nam, thang, ngay, tv.Hours, tv.Minutes, 0);
                    }
                    if (TimeSpan.TryParse(currentBcct.GIORA, out TimeSpan tr))
                    {
                        timeEditGioRa.Time = new DateTime(nam, thang, ngay, tr.Hours, tr.Minutes, 0);
                    }
                    _hasSavedHoursInDb = true;
                    _isGioTuDien = false;
                    _isManualEdited = false;
                    lblNguonDuLieu.Text = "Nguồn: Nhập tay đã lưu";
                    lblNguonDuLieu.ForeColor = Color.DarkSlateGray;
                }
                else
                {
                    // Chưa có giờ: tự điền theo ca làm việc
                    _hasSavedHoursInDb = false;
                    _isGioTuDien = true;
                    _isManualEdited = false;
                    ApplyDefaultShiftHoursSilent();
                    lblNguonDuLieu.Text = "Nguồn: Tự điền theo ca làm việc";
                    lblNguonDuLieu.ForeColor = Color.Gray;
                }

                // Xác định trạng thái công và loại nghỉ
                if (currentBcct != null && !string.IsNullOrEmpty(currentBcct.KYHIEU))
                {
                    lblBadgeChuaXacNhan.Visible = false;
                    string kh = currentBcct.KYHIEU.Trim().ToUpper();

                    if (kh == "CT") // Công tác
                    {
                        radDiLam.Checked = true;
                        chkCongTac.Checked = true;
                    }
                    else if (kh == "P" || kh == "V" || kh == "VR") // Nghỉ
                    {
                        radNghi.Checked = true;
                        chkCongTac.Checked = false;
                        cboLoaiNghi.SelectedIndex = (kh == "P" || kh == "VR") ? 0 : 1; // 0: Nghỉ phép, 1: Nghỉ không phép
                        txtLyDoNghi.Text = currentBcct.GHICHU ?? (kh == "VR" ? "Việc riêng" : "");

                        // Xác định phần nghỉ
                        if (currentBcct.NGAYPHEP == 1.0m || (currentBcct.NGAYCONG == 0 && currentBcct.NGAYPHEP == 0))
                        {
                            cboPhanNghi.SelectedIndex = 2; // Nguyên ngày / Cả ca
                        }
                        else if (currentBcct.NGAYPHEP == 0.5m || currentBcct.NGAYCONG == 0.5m)
                        {
                            // Nghỉ nửa ngày
                            cboPhanNghi.SelectedIndex = 0; // Mặc định là Sáng
                        }
                        else
                        {
                            cboPhanNghi.SelectedIndex = 2; // Nguyên ngày
                        }
                    }
                    else // Đi làm (X hoặc CD)
                    {
                        radDiLam.Checked = true;
                        chkCongTac.Checked = false;
                    }
                }
                else
                {
                    // Bản ghi chưa có dữ liệu: Hiển thị nhãn Chưa xác nhận
                    lblBadgeChuaXacNhan.Visible = true;
                    lblBadgeChuaXacNhan.Text = "● Chưa có dữ liệu chấm công";
                    radDiLam.Checked = true;
                    chkCongTac.Checked = false;
                    cboLoaiNghi.SelectedIndex = 0;
                    cboPhanNghi.SelectedIndex = 0;
                    txtLyDoNghi.Text = "";
                }

                // Hiển thị trạng thái chốt công dễ hiểu
                if (v118 != null && v118.LanTinhIdHienHanh.HasValue)
                {
                    if (v118.DuDieuKienChot == 1)
                    {
                        lblTrangThaiBangCong.Text = $"✓ Đã xác nhận — Đủ điều kiện chốt công (Công: {v118.NgayCong ?? 0:N2})";
                        lblTrangThaiBangCong.ForeColor = Color.ForestGreen;
                    }
                    else
                    {
                        lblTrangThaiBangCong.Text = "⚠ Chờ xác minh — Có bất thường chấm công cần xử lý trước khi chốt";
                        lblTrangThaiBangCong.ForeColor = Color.DarkOrange;
                    }
                }
                else
                {
                    lblTrangThaiBangCong.Text = "⏳ Dữ liệu mới/chưa chốt — Sẽ tự động tính toán khi bấm Cập nhật";
                    lblTrangThaiBangCong.ForeColor = Color.Gray;
                }
            }
            catch (Exception ex)
            {
                lblTrangThaiBangCong.Text = "Lỗi tải thông tin: " + ex.Message;
                lblTrangThaiBangCong.ForeColor = Color.Red;
            }
            finally
            {
                _isLoading = false;
                UpdateLayoutAndPreview();
            }
        }

        private void UpdatePhanNghiItems()
        {
            int oldIdx = cboPhanNghi.SelectedIndex;
            cboPhanNghi.Properties.Items.Clear();

            if (_currentShift != null && _currentShift.IsNightShift)
            {
                cboPhanNghi.Properties.Items.Add("Nửa đầu ca (22:00 - 02:00)");
                cboPhanNghi.Properties.Items.Add("Nửa cuối ca (02:00 - 06:00)");
                cboPhanNghi.Properties.Items.Add("Cả ca (22:00 - 06:00)");
            }
            else
            {
                cboPhanNghi.Properties.Items.Add("Sáng");
                cboPhanNghi.Properties.Items.Add("Chiều");
                cboPhanNghi.Properties.Items.Add("Nguyên ngày");
            }

            if (oldIdx >= 0 && oldIdx < cboPhanNghi.Properties.Items.Count)
            {
                cboPhanNghi.SelectedIndex = oldIdx;
            }
            else
            {
                cboPhanNghi.SelectedIndex = 0;
            }

            if (cboLoaiNghi.SelectedIndex < 0)
            {
                cboLoaiNghi.SelectedIndex = 0;
            }
        }

        private string GetPhanNghiCode()
        {
            int idx = cboPhanNghi.SelectedIndex;
            if (idx == 0) return "S";
            if (idx == 1) return "C";
            return "NN";
        }

        private void ApplyDefaultShiftHoursSilent()
        {
            if (_currentShift == null) return;
            string phanNghi = GetPhanNghiCode();
            (TimeSpan? gv, TimeSpan? gr) = AttendanceCalculationHelper.GetDefaultShiftHours(_currentShift, radDiLam.Checked, phanNghi);
            if (gv.HasValue && gr.HasValue)
            {
                timeEditGioVao.Time = new DateTime(_ngayDate.Year, _ngayDate.Month, _ngayDate.Day, gv.Value.Hours, gv.Value.Minutes, 0);
                timeEditGioRa.Time = new DateTime(_ngayDate.Year, _ngayDate.Month, _ngayDate.Day, gr.Value.Hours, gr.Value.Minutes, 0);
            }
        }

        /// <summary>
        /// Căn chỉnh layout linh hoạt không để khoảng trống lớn và chạy tính công dự kiến trực tiếp
        /// </summary>
        private void UpdateLayoutAndPreview()
        {
            if (_isLoading) return;

            bool isDiLam = radDiLam.Checked;
            bool isNghi = radNghi.Checked;
            chkCongTac.Enabled = isDiLam;

            string phanNghi = GetPhanNghiCode();
            bool isNghiNguyenNgay = (isNghi && phanNghi == "NN");

            // 1. Căn chỉnh vị trí các GroupControl
            if (isNghi)
            {
                grThongTinNghi.Visible = true;
                grThongTinNghi.Top = grTrangThai.Bottom + 6;

                if (isNghiNguyenNgay)
                {
                    // Nghỉ nguyên ngày: ẩn cụm nhập giờ, hiển thị thông báo
                    grGioGhiNhan.Visible = true;
                    pnlGioInputs.Visible = false;
                    lblNghiNguyenNgayNotice.Visible = true;
                    btnApDungGioCa.Visible = false;

                    grGioGhiNhan.Top = grThongTinNghi.Bottom + 6;
                    grGioGhiNhan.Height = 65;
                    grKetQuaDuKien.Top = grGioGhiNhan.Bottom + 6;
                }
                else
                {
                    // Nghỉ nửa ngày: hiển thị nhập giờ cho buổi làm còn lại
                    grGioGhiNhan.Visible = true;
                    pnlGioInputs.Visible = true;
                    lblNghiNguyenNgayNotice.Visible = false;
                    btnApDungGioCa.Visible = true;

                    grGioGhiNhan.Top = grThongTinNghi.Bottom + 6;
                    grGioGhiNhan.Height = 85;
                    grKetQuaDuKien.Top = grGioGhiNhan.Bottom + 6;
                }
            }
            else // Đi làm
            {
                grThongTinNghi.Visible = false;
                grGioGhiNhan.Visible = true;
                pnlGioInputs.Visible = true;
                lblNghiNguyenNgayNotice.Visible = false;
                btnApDungGioCa.Visible = true;

                grGioGhiNhan.Top = grTrangThai.Bottom + 6;
                grGioGhiNhan.Height = 85;
                grKetQuaDuKien.Top = grGioGhiNhan.Bottom + 6;
            }

            // 2. Chạy tính toán kết quả dự kiến (Live Preview)
            bool isCongTac = isDiLam && chkCongTac.Checked;
            string loaiNghi = (cboLoaiNghi.SelectedIndex == 1) ? "V" : "P"; // P: Nghỉ phép, V: Nghỉ không phép
            DateTime? gv = isNghiNguyenNgay ? (DateTime?)null : timeEditGioVao.Time;
            DateTime? gr = isNghiNguyenNgay ? (DateTime?)null : timeEditGioRa.Time;

            var res = AttendanceCalculationHelper.Calculate(
                _currentShift,
                isDiLam,
                isCongTac,
                phanNghi,
                loaiNghi,
                gv,
                gr,
                _hasDevicePunchLog,
                _ngayDate
            );

            // 3. Hiển thị lên UI
            lblKetQuaCong.Text = $"{res.NgayCong:N2} công";
            lblKetQuaPhep.Text = $"{res.NgayPhep:N1} ngày";
            lblKetQuaGioHopLe.Text = res.GioLamHopLeText;
            lblKetQuaDiMuon.Text = res.DiMuonVeSomText;
            lblKetQuaDiMuon.ForeColor = (res.DiMuonPhut > 5 || res.VeSomPhut > 5) ? Color.Crimson : Color.ForestGreen;
            lblKetQuaTrangThai.Text = $"{res.TrangThaiHienThi} (Ký hiệu: {res.KyHieu})";

            if (res.HasConflict)
            {
                lblCanhBaoXungDot.Text = "⚠ " + res.ConflictMessage;
                lblCanhBaoXungDot.ForeColor = Color.Crimson;
            }
            else if (res.HasWarning)
            {
                lblCanhBaoXungDot.Text = "⚠ " + res.WarningMessage;
                lblCanhBaoXungDot.ForeColor = Color.DarkOrange;
            }
            else
            {
                lblCanhBaoXungDot.Text = "✓ Dữ liệu hợp lệ, đủ điều kiện xác nhận công.";
                lblCanhBaoXungDot.ForeColor = Color.ForestGreen;
            }
        }

        private void radStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            if (radDiLam.Checked)
            {
                chkCongTac.Enabled = true;
                if (_draftGioVao.HasValue && _draftGioRa.HasValue)
                {
                    timeEditGioVao.Time = _draftGioVao.Value;
                    timeEditGioRa.Time = _draftGioRa.Value;
                }
                else if (_isGioTuDien)
                {
                    ApplyDefaultShiftHoursSilent();
                }
            }
            else if (radNghi.Checked)
            {
                chkCongTac.Checked = false;
                chkCongTac.Enabled = false;

                if (GetPhanNghiCode() == "NN")
                {
                    _draftGioVao = timeEditGioVao.Time;
                    _draftGioRa = timeEditGioRa.Time;
                }
                else if (_isGioTuDien)
                {
                    ApplyDefaultShiftHoursSilent();
                }
            }

            UpdateLayoutAndPreview();
        }

        private void chkCongTac_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdateLayoutAndPreview();
        }

        private void cboCaLam_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            int idx = cboCaLam.SelectedIndex;
            if (idx >= 0 && idx < _allShifts.Count)
            {
                _currentShift = _allShifts[idx];
                lblChiTietCa.Text = _currentShift.DisplayDescription;
                UpdatePhanNghiItems();

                if (_isGioTuDien)
                {
                    ApplyDefaultShiftHoursSilent();
                }
                UpdateLayoutAndPreview();
            }
        }

        private void cboPhanNghi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            string code = GetPhanNghiCode();
            if (code == "NN")
            {
                _draftGioVao = timeEditGioVao.Time;
                _draftGioRa = timeEditGioRa.Time;
            }
            else
            {
                if (_draftGioVao.HasValue && !_isGioTuDien && _isManualEdited)
                {
                    // Người dùng từng sửa giờ tay, khôi phục lại
                    timeEditGioVao.Time = _draftGioVao.Value;
                    timeEditGioRa.Time = _draftGioRa.Value;
                }
                else if (_isGioTuDien)
                {
                    ApplyDefaultShiftHoursSilent();
                }
            }

            UpdateLayoutAndPreview();
        }

        private void cboLoaiNghi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdateLayoutAndPreview();
        }

        private void timeEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            // Người dùng sửa giờ thủ công: ghi nhận không tự động đè lại
            _isGioTuDien = false;
            _isManualEdited = true;
            lblNguonDuLieu.Text = "Nguồn: Nhập tay thủ công (Đã điều chỉnh)";
            lblNguonDuLieu.ForeColor = Color.DarkOrange;

            UpdateLayoutAndPreview();
        }

        private void btnApDungGioCa_Click(object sender, EventArgs e)
        {
            if (IsKyCongBiKhoa()) return;
            if (_currentShift == null) return;

            string phanNghi = GetPhanNghiCode();
            (TimeSpan? gv, TimeSpan? gr) = AttendanceCalculationHelper.GetDefaultShiftHours(_currentShift, radDiLam.Checked, phanNghi);

            if (gv.HasValue && gr.HasValue)
            {
                timeEditGioVao.Time = new DateTime(_ngayDate.Year, _ngayDate.Month, _ngayDate.Day, gv.Value.Hours, gv.Value.Minutes, 0);
                timeEditGioRa.Time = new DateTime(_ngayDate.Year, _ngayDate.Month, _ngayDate.Day, gr.Value.Hours, gr.Value.Minutes, 0);

                _isGioTuDien = true;
                _isManualEdited = false;
                lblNguonDuLieu.Text = "Nguồn: Đã áp dụng giờ chuẩn theo ca";
                lblNguonDuLieu.ForeColor = Color.ForestGreen;

                UpdateLayoutAndPreview();
            }
        }

        private void cldNgayCong_DateSelected(object sender, DateRangeEventArgs e)
        {
            _cngay = cldNgayCong.SelectionRange.Start.Day;
            LoadThongTinNgayCong(_cngay);
        }

        private async void btnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsKyCongBiKhoa())
                {
                    XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khóa, chỉ có thể xem, không thể cập nhật.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cldNgayCong.SelectionRange.Start.Year * 100 + cldNgayCong.SelectionRange.Start.Month != _MAKYCONG)
                {
                    XtraMessageBox.Show("Vui lòng chọn đúng ngày công thuộc tháng hiện tại của kỳ công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int nam = _MAKYCONG / 100;
                int thang = _MAKYCONG % 100;
                int ngayChon = cldNgayCong.SelectionStart.Day;
                _cngay = ngayChon;
                _ngayDate = new DateTime(nam, thang, _cngay);

                bool isDiLam = radDiLam.Checked;
                bool isCongTac = isDiLam && chkCongTac.Checked;
                string phanNghi = GetPhanNghiCode();
                string loaiNghi = (cboLoaiNghi.SelectedIndex == 1) ? "V" : "P";
                bool isNghiNguyenNgay = (!isDiLam && phanNghi == "NN");

                // Validate thông tin nghỉ
                if (!isDiLam)
                {
                    if (cboLoaiNghi.SelectedIndex < 0)
                    {
                        XtraMessageBox.Show("Vui lòng chọn loại nghỉ (Nghỉ phép hoặc Nghỉ không phép).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Validate giờ vào/ra nếu không phải nghỉ nguyên ngày
                DateTime? gv = isNghiNguyenNgay ? (DateTime?)null : timeEditGioVao.Time;
                DateTime? gr = isNghiNguyenNgay ? (DateTime?)null : timeEditGioRa.Time;

                // Tính toán kết quả chốt
                var res = AttendanceCalculationHelper.Calculate(
                    _currentShift,
                    isDiLam,
                    isCongTac,
                    phanNghi,
                    loaiNghi,
                    gv,
                    gr,
                    _hasDevicePunchLog,
                    _ngayDate
                );

                // Cảnh báo xung đột nếu có
                if (res.HasConflict)
                {
                    var dlg = XtraMessageBox.Show(
                        $"{res.ConflictMessage}\n\nBạn có chắc chắn muốn xác nhận nghỉ nguyên ngày và bỏ qua dữ liệu giờ làm này không?",
                        "Xác nhận xung đột",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (dlg != DialogResult.Yes)
                    {
                        return;
                    }
                }

                // Cảnh báo nếu thiếu dữ liệu khi đi làm
                if (!isNghiNguyenNgay && (gv == null || gr == null))
                {
                    XtraMessageBox.Show("Thiếu thông tin giờ vào hoặc giờ ra. Không thể xác nhận đủ công!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int? gioVaoVal = isNghiNguyenNgay ? (int?)null : timeEditGioVao.Time.Hour;
                int? phutVaoVal = isNghiNguyenNgay ? (int?)null : timeEditGioVao.Time.Minute;
                int? gioRaVal = isNghiNguyenNgay ? (int?)null : timeEditGioRa.Time.Hour;
                int? phutRaVal = isNghiNguyenNgay ? (int?)null : timeEditGioRa.Time.Minute;

                string ghiChu = !isDiLam ? txtLyDoNghi.Text?.Trim() : (isCongTac ? "Đi công tác" : "");

                // Khóa giao diện trong khi lưu
                btnCapNhat.Enabled = false;
                btnDong.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                lblTrangThaiBangCong.Text = "⏳ Đang lưu dữ liệu và tính toán công bố lại...";
                lblTrangThaiBangCong.ForeColor = Color.DarkOrange;

                long? idCaPhienBan = _currentShift?.IdCaPhienBan;

                await Task.Run(() =>
                {
                    var bcctWorker = new BANGCONG_NV_CHITIET();
                    bcctWorker.CapNhatNgayCongVaBangCongRaw(
                        _manv,
                        _MAKYCONG,
                        nam,
                        thang,
                        _cngay,
                        gioVaoVal,
                        phutVaoVal,
                        gioRaVal,
                        phutRaVal,
                        res.KyHieu,
                        phanNghi,
                        res.NgayCong,
                        res.NgayPhep,
                        1,
                        ghiChu,
                        idCaPhienBan
                    );
                });

                // Tải lại dữ liệu vừa lưu
                LoadThongTinNgayCong(_cngay);

                string msg = $"Cập nhật thành công ngày {_cngay:D2}/{thang:D2}/{nam}!\n" +
                             $"- Ca làm: {_currentShift?.TenPhienBan}\n" +
                             $"- Trạng thái: {res.TrangThaiHienThi}\n" +
                             $"- Công thực tế: {res.NgayCong:N2} | Phép: {res.NgayPhep:N1}\n";

                if (isNghiNguyenNgay)
                {
                    msg += "- Giờ làm: Không ghi nhận (Nghỉ nguyên ngày)";
                }
                else
                {
                    msg += $"- Giờ vào: {gioVaoVal:D2}:{phutVaoVal:D2} | Giờ ra: {gioRaVal:D2}:{phutRaVal:D2}";
                }

                XtraMessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Lỗi khi cập nhật ngày công: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadThongTinNgayCong(_cngay);
            }
            finally
            {
                btnCapNhat.Enabled = !IsKyCongBiKhoa();
                btnDong.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}