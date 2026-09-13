using Bu;
using Bu.DTO;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using QLyNSu.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu
{
    public partial class FrmKyLuat : DevExpress.XtraEditors.XtraForm
    {
        public FrmKyLuat()
        {
            InitializeComponent();
        }

        private bool _them;
        private string _SOQD;
        private KHENTHUONG_KYLUAT _ktkl;
        private NHANVIEN _nhanvien;
        public List<KHENTHUONG_KYLUAT_DTO> _lstKL;
        private void FrmKyLuat_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_NV_KYLUAT")) return;

            _ktkl = new KHENTHUONG_KYLUAT();
            _nhanvien = new NHANVIEN();
            _them = false;
            showHide(true);
            LoadData();
            loadNhanVien();
            splitContainer1.Panel1Collapsed = true;
            spSoTien.Enter += (s, ev) => { BeginInvoke((Action)(() => spSoTien.SelectAll())); };
            spNamApDung.Enter += (s, ev) => { BeginInvoke((Action)(() => spNamApDung.SelectAll())); };
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            gcDsKl.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_NV_KYLUAT", btnThem, btnSua, btnXoa, btnIn, kt);
            txtLyDo.Enabled = !kt;
            txtNoiDung.Enabled = !kt;
            txtSoQD.Enabled = !kt;
            dtNgay.Enabled = !kt;
            dtTuNgay.Enabled = !kt;
            dtDenNgay.Enabled = !kt;
            searchMANV.Enabled = !kt;
            spSoTien.Enabled = !kt;
            bool hasAmount = ParseDecimalSafe(spSoTien.EditValue) > 0;
            cbThangApDung.Enabled = !kt && hasAmount;
            spNamApDung.Enabled = !kt && hasAmount;
        }

        private void _reset()
        {
            txtSoQD.Text = string.Empty;
            txtLyDo.Text = string.Empty;
            txtNoiDung.Text = string.Empty;
            dtNgay.Value = DateTime.Now;
            dtTuNgay.Value = DateTime.Now;
            dtDenNgay.Value = dtTuNgay.Value.AddDays(7);
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
            spSoTien.EditValue = 0;
            cbThangApDung.SelectedItem = null;
            cbThangApDung.Text = "";
            spNamApDung.EditValue = 0;
            cbThangApDung.Enabled = false;
            spNamApDung.Enabled = false;
        }

        private void spSoTien_EditValueChanged(object sender, EventArgs e)
        {
            decimal val = ParseDecimalSafe(spSoTien.EditValue);
            bool hasAmount = val > 0;
            cbThangApDung.Enabled = hasAmount && btnLuu.Enabled;
            spNamApDung.Enabled = hasAmount && btnLuu.Enabled;
            if (hasAmount)
            {
                if (cbThangApDung.SelectedItem == null || string.IsNullOrEmpty(cbThangApDung.Text) || cbThangApDung.Text == "0")
                    cbThangApDung.SelectedItem = DateTime.Now.Month.ToString();
                if (ParseIntSafe(spNamApDung.EditValue) <= 0)
                    spNamApDung.EditValue = DateTime.Now.Year;
            }
            else
            {
                cbThangApDung.SelectedItem = null;
                cbThangApDung.Text = "";
                spNamApDung.EditValue = 0;
            }
        }

        private void loadNhanVien()
        {
            searchMANV.Properties.DataSource = _nhanvien.getList();
            searchMANV.Properties.ValueMember = "MANV";
            searchMANV.Properties.DisplayMember = "HOTEN";
        }

        private void LoadData()
        {
            gcDsKl.DataSource = _ktkl.getListFull(2);
            FormManager_Functions.CustomView_Colums(gvDsKl);
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_KYLUAT", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_KYLUAT", Bu.DTO.PermissionAction.Edit)) return;
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDsKl.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_KYLUAT", Bu.DTO.PermissionAction.Delete)) return;
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _ktkl.Delete(_SOQD,1);
                LoadData();
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
            LoadData();
            _them = false;
            showHide(true);
            splitContainer1.Panel1Collapsed = true;
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
            splitContainer1.Panel1Collapsed = true;
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_KYLUAT", Bu.DTO.PermissionAction.Print)) return;
            _lstKL = _ktkl.getItem_FULL(2, _SOQD);
            rptKyLuat rpt = new rptKyLuat(_lstKL);
            rpt.ShowRibbonPreview();
        }

        private void gvDsKl_Click(object sender, EventArgs e)
        {
            if (gvDsKl.RowCount > 0)
            {
                _SOQD = gvDsKl.GetFocusedRowCellValue("SOQUYETDINH").ToString();
                var kl = _ktkl.getItem(_SOQD);

                txtSoQD.Text = _SOQD;
                dtNgay.Value = kl.NGAY.Value;
                dtTuNgay.Value = kl.TUNGAY.Value;
                dtDenNgay.Value = kl.DENNGAY.Value;
                searchMANV.EditValue = kl.MANV;
                txtLyDo.Text = kl.LYDO;
                txtNoiDung.Text = kl.NOIDUNG;
                decimal soTien = ParseDecimalSafe(kl.SOTIEN);
                spSoTien.EditValue = soTien;
                if (kl.THANG_APDUNG.HasValue && kl.THANG_APDUNG.Value > 0)
                    cbThangApDung.SelectedItem = ((int)kl.THANG_APDUNG.Value).ToString();
                else
                {
                    cbThangApDung.SelectedItem = null;
                    cbThangApDung.Text = "";
                }
                spNamApDung.EditValue = kl.NAM_APDUNG ?? 0;
                bool hasAmount = soTien > 0;
                cbThangApDung.Enabled = hasAmount && btnLuu.Enabled;
                spNamApDung.Enabled = hasAmount && btnLuu.Enabled;

                _lstKL = _ktkl.getItem_FULL(2, _SOQD);
            }
        }

        private decimal ParseDecimalSafe(object val)
        {
            if (val == null) return 0;
            if (val is decimal d) return d;
            if (val is int i) return i;
            if (val is long l) return l;
            if (val is double db) return (decimal)db;
            if (val is float f) return (decimal)f;
            string s = val.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return 0;
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var res1))
                return res1;
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var res2))
                return res2;
            string clean = System.Text.RegularExpressions.Regex.Replace(s, @"[^\d]", "");
            if (decimal.TryParse(clean, out var res3))
                return res3;
            return 0;
        }

        private int ParseIntSafe(object val)
        {
            if (val == null) return 0;
            if (val is int i) return i;
            if (val is decimal d) return (int)d;
            if (val is long l) return (int)l;
            if (val is short s) return s;
            string str = val.ToString().Trim();
            if (string.IsNullOrEmpty(str)) return 0;
            if (int.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var res1))
                return res1;
            if (int.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var res2))
                return res2;
            string clean = System.Text.RegularExpressions.Regex.Replace(str, @"[^\d]", "");
            if (int.TryParse(clean, out var res3))
                return res3;
            return 0;
        }

        private void SaveData()
        {
            try
            {
                var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
                if (!Functions.FormSecurity.AssertPermission("F_NV_KYLUAT", permAction)) return;

                if (_them)
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
                    {
                        MessageBox.Show("Vui lòng chọn điền nội dung.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtLyDo.Text))
                    {
                        MessageBox.Show("Vui lòng chọn điền lý do kỷ luật.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    decimal soTien = ParseDecimalSafe(spSoTien.EditValue);

                    int? thangApDung = null;
                    int? namApDung = null;
                    if (soTien > 0)
                    {
                        int thang = ParseIntSafe(cbThangApDung.SelectedItem ?? cbThangApDung.Text);
                        int nam = ParseIntSafe(spNamApDung.EditValue ?? spNamApDung.Text);
                        if (thang < 1 || thang > 12 || nam < 2000)
                        {
                            MessageBox.Show("Số tiền phạt lớn hơn 0, vui lòng chọn tháng (1-12) và năm áp dụng trừ vào lương.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        thangApDung = thang;
                        namApDung = nam;
                    }

                    var maxSoQD = _ktkl.MaxSoQuyetDinh(2);
                    int so = 1;
                    if (!string.IsNullOrEmpty(maxSoQD))
                    {
                        var parts = maxSoQD.Split('/');
                        if (parts.Length > 0 && int.TryParse(parts[0], out int parsedNum))
                        {
                            so = parsedNum + 1;
                        }
                        else if (maxSoQD.Length >= 5 && int.TryParse(maxSoQD.Substring(0, 5), out int num2))
                        {
                            so = num2 + 1;
                        }
                    }

                    TB_KHENTHUONG_KYLUAT kl = new TB_KHENTHUONG_KYLUAT();
                    kl.SOQUYETDINH = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/QĐKL";
                    kl.TUNGAY = dtTuNgay.Value;
                    kl.DENNGAY = dtDenNgay.Value;
                    kl.NGAY = dtNgay.Value;
                    kl.LYDO = txtLyDo.Text;
                    kl.LOAI = 2;
                    kl.NOIDUNG = txtNoiDung.Text;
                    kl.MANV = ParseIntSafe(searchMANV.EditValue);
                    kl.SOTIEN = soTien;
                    kl.THANG_APDUNG = thangApDung;
                    kl.NAM_APDUNG = namApDung;
                    kl.CREATED_BY = 1;
                    kl.CREATED_DATE = DateTime.Now;
                    _ktkl.Add(kl);
                }
                else
                {
                    var kl = _ktkl.getItem(_SOQD);
                    if (kl == null)
                    {
                        MessageBox.Show("Không tìm thấy quyết định với số quyết định: " + _SOQD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    decimal soTien = ParseDecimalSafe(spSoTien.EditValue);

                    int? thangApDung = null;
                    int? namApDung = null;
                    if (soTien > 0)
                    {
                        int thang = ParseIntSafe(cbThangApDung.SelectedItem ?? cbThangApDung.Text);
                        int nam = ParseIntSafe(spNamApDung.EditValue ?? spNamApDung.Text);
                        if (thang < 1 || thang > 12 || nam < 2000)
                        {
                            MessageBox.Show("Số tiền phạt lớn hơn 0, vui lòng chọn tháng (1-12) và năm áp dụng trừ vào lương.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        thangApDung = thang;
                        namApDung = nam;
                    }

                    kl.NGAY = dtNgay.Value;
                    kl.LYDO = txtLyDo.Text;
                    kl.NOIDUNG = txtNoiDung.Text;
                    kl.TUNGAY = dtTuNgay.Value;
                    kl.DENNGAY = dtDenNgay.Value;
                    kl.MANV = ParseIntSafe(searchMANV.EditValue);
                    kl.SOTIEN = soTien;
                    kl.THANG_APDUNG = thangApDung;
                    kl.NAM_APDUNG = namApDung;
                    kl.UPDATED_BY = 1;
                    kl.UPDATED_DATE = DateTime.Now;
                    _ktkl.Update(kl);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}