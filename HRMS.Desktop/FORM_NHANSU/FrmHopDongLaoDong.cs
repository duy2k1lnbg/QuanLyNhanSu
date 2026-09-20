using Bu;
using Bu.CLASS_CHAMCONG;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu
{
    public partial class FrmHopDongLaoDong : DevExpress.XtraEditors.XtraForm
    {
        public FrmHopDongLaoDong()
        {
            InitializeComponent();
        }

        private HOPDONGLAODONG _hdld;
        private NHANVIEN _nhanvien;
        private PHUCAP _phucap;
        private Bu.CLASS_NHANSU.LOAIHOPDONG _loaiHopDong;
        public List<HOPDONG_DTO> _lstHD;
        private bool _them;
        private string _SOHD;

        private async void FrmHopDongLaoDong_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_NV_HOPDONG")) return;

            _hdld = new HOPDONGLAODONG();
            _nhanvien = new NHANVIEN();
            _phucap = new PHUCAP();
            _loaiHopDong = new Bu.CLASS_NHANSU.LOAIHOPDONG();
            _them = false;
            txtNoiDung.Properties.MaxLength = 4000;
            showHide(true);
            splitContainer1.Panel1Collapsed = true;

            searchMANV.EditValueChanged += searchMANV_EditValueChanged;

            await LoadInitialDataAsync();
            Functions.TranslationManager.Translate(this);
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            gcDsHDLD.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_NV_HOPDONG", btnThem, btnSua, btnXoa, btnIn, kt);

            dtNgayKetThuc.Enabled = !kt && cbThoiHan.Text != "Vô thời hạn";
            dtNgayBatDau.Enabled = !kt;
            dtNgayKy.Enabled = !kt;
            cbThoiHan.Enabled = !kt;
            cbLoaiHD.Enabled = !kt;
            spHeSoLuong.Enabled = !kt;
            spLuongThoaThuan.Enabled = !kt;
            spLanKy.Enabled = !kt;
            txtSoHD.Enabled = !kt;
            searchMANV.Enabled = !kt;
            txtNoiDung.Enabled = !kt;

            // Chế độ 13 loại phụ cấp
            spPC_NhaO.Enabled = !kt;
            spPC_DiLai.Enabled = !kt;
            spPC_GiaDinh.Enabled = !kt;
            spPC_NguoiPhuThuoc.Enabled = !kt;
            spPC_ChucVu.Enabled = !kt;
            spPC_ChungChi.Enabled = !kt;
            spPC_KyNang.Enabled = !kt;
            spPC_KhuVuc.Enabled = !kt;
            spPC_ChuyenCan.Enabled = !kt;
            spPC_ThamNien.Enabled = !kt;
            spPC_LamViecTaiNha.Enabled = !kt;
            spPC_DacBiet.Enabled = !kt;
            spPC_Khac.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoHD.Text = string.Empty;
            dtNgayBatDau.Value = DateTime.Now;
            dtNgayKy.Value = DateTime.Now;
            dtNgayKetThuc.Value = DateTime.Now.AddYears(1);
            cbThoiHan.Text = "1 Năm";
            if (cbLoaiHD.Items.Count > 0) cbLoaiHD.SelectedIndex = 0;
            spLanKy.EditValue = 1;
            spHeSoLuong.EditValue = 1;
            spLuongThoaThuan.EditValue = 0;
            searchMANV.EditValue = null;
            txtNoiDung.Text = string.Empty;

            // Reset 13 loại phụ cấp
            spPC_NhaO.EditValue = 0;
            spPC_DiLai.EditValue = 0;
            spPC_GiaDinh.EditValue = 0;
            spPC_NguoiPhuThuoc.EditValue = 0;
            spPC_ChucVu.EditValue = 0;
            spPC_ChungChi.EditValue = 0;
            spPC_KyNang.EditValue = 0;
            spPC_KhuVuc.EditValue = 0;
            spPC_ChuyenCan.EditValue = 0;
            spPC_ThamNien.EditValue = 0;
            spPC_LamViecTaiNha.EditValue = 0;
            spPC_DacBiet.EditValue = 0;
            spPC_Khac.EditValue = 0;
            spPC_Tong.EditValue = 0;
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var taskHd = Task.Run(() => _hdld.getlistFull_DTO());
                var taskNv = Task.Run(() => _nhanvien.getList());
                var taskLhd = Task.Run(() => _loaiHopDong.getList());

                await Task.WhenAll(taskHd, taskNv, taskLhd);

                gcDsHDLD.DataSource = taskHd.Result;
                FormManager_Functions.CustomView_Colums(gvDsHDLD);

                searchMANV.Properties.DataSource = taskNv.Result;
                searchMANV.Properties.ValueMember = "MANV";
                searchMANV.Properties.DisplayMember = "HOTEN";

                cbLoaiHD.DataSource = taskLhd.Result;
                cbLoaiHD.DisplayMember = "TENLOAIHD";
                cbLoaiHD.ValueMember = "LOAIHD";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var list = await Task.Run(() => _hdld.getlistFull_DTO());
                gcDsHDLD.DataSource = list;
                FormManager_Functions.CustomView_Colums(gvDsHDLD);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi làm mới dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void searchMANV_EditValueChanged(object sender, EventArgs e)
        {
            if (searchMANV.EditValue != null && int.TryParse(searchMANV.EditValue.ToString(), out int manv))
            {
                await LoadPhuCapForNhanVienAsync(manv);
            }
        }

        private async Task LoadPhuCapForNhanVienAsync(int manv)
        {
            if (_phucap == null) _phucap = new PHUCAP();
            var dict = await Task.Run(() => _phucap.GetPhuCapByNhanVien(manv));

            spPC_NhaO.EditValue = dict.ContainsKey(1) ? dict[1] : 0;
            spPC_DiLai.EditValue = dict.ContainsKey(2) ? dict[2] : 0;
            spPC_GiaDinh.EditValue = dict.ContainsKey(3) ? dict[3] : 0;
            spPC_NguoiPhuThuoc.EditValue = dict.ContainsKey(4) ? dict[4] : 0;
            spPC_ChucVu.EditValue = dict.ContainsKey(5) ? dict[5] : 0;
            spPC_ChungChi.EditValue = dict.ContainsKey(6) ? dict[6] : 0;
            spPC_KyNang.EditValue = dict.ContainsKey(7) ? dict[7] : 0;
            spPC_KhuVuc.EditValue = dict.ContainsKey(8) ? dict[8] : 0;
            spPC_ChuyenCan.EditValue = dict.ContainsKey(9) ? dict[9] : 0;
            spPC_ThamNien.EditValue = dict.ContainsKey(10) ? dict[10] : 0;
            spPC_LamViecTaiNha.EditValue = dict.ContainsKey(11) ? dict[11] : 0;
            spPC_DacBiet.EditValue = dict.ContainsKey(12) ? dict[12] : 0;
            spPC_Khac.EditValue = dict.ContainsKey(13) ? dict[13] : 0;

            CalculateTotalAllowance(null, null);
        }

        private void CalculateTotalAllowance(object sender, EventArgs e)
        {
            decimal total = Convert.ToDecimal(spPC_NhaO.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_DiLai.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_GiaDinh.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_NguoiPhuThuoc.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_ChucVu.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_ChungChi.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_KyNang.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_KhuVuc.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_ChuyenCan.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_ThamNien.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_LamViecTaiNha.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_DacBiet.EditValue ?? 0)
                          + Convert.ToDecimal(spPC_Khac.EditValue ?? 0);
            spPC_Tong.EditValue = total;
        }

        private string StripRtfIfNeeded(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.TrimStart().StartsWith("{\\rtf"))
            {
                try
                {
                    using (var rtb = new System.Windows.Forms.RichTextBox())
                    {
                        rtb.Rtf = text;
                        return rtb.Text;
                    }
                }
                catch
                {
                    return Regex.Replace(text, @"\\[a-zA-Z0-9]+ ?", "").Replace("{", "").Replace("}", "").Trim();
                }
            }
            return text;
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_HOPDONG", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_HOPDONG", Bu.DTO.PermissionAction.Edit)) return;
            if (string.IsNullOrEmpty(_SOHD))
            {
                MessageBox.Show("Vui lòng chọn hợp đồng cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDsHDLD.Enabled = true;
        }

        private async void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_HOPDONG", Bu.DTO.PermissionAction.Delete)) return;
            if (string.IsNullOrEmpty(_SOHD))
            {
                MessageBox.Show("Vui lòng chọn hợp đồng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa hợp đồng: " + _SOHD + " không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string sohd = _SOHD;
                await Task.Run(() => _hdld.Delete(sohd, 1));
                await LoadDataAsync();
                splitContainer1.Panel1Collapsed = true;
            }
        }

        private async void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await SaveDataAsync();
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

        private async void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_NV_HOPDONG", Bu.DTO.PermissionAction.Print)) return;
            if (string.IsNullOrEmpty(_SOHD))
            {
                MessageBox.Show("Vui lòng chọn hợp đồng để in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string sohd = _SOHD;
            _lstHD = await Task.Run(() => _hdld.getItem_FULL(sohd));
            rptHopDongLaoDong rpt = new rptHopDongLaoDong(_lstHD);
            rpt.ShowRibbonPreview();
        }

        private async Task SaveDataAsync()
        {
            try
            {
                var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
                if (!Functions.FormSecurity.AssertPermission("F_NV_HOPDONG", permAction)) return;
                if (string.IsNullOrWhiteSpace(cbThoiHan.Text))
                {
                    MessageBox.Show("Vui lòng chọn thời hạn hợp đồng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cbLoaiHD.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn loại hợp đồng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out int manv))
                {
                    MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool isVoThoiHan = cbThoiHan.Text.Trim().Equals("Vô thời hạn", StringComparison.OrdinalIgnoreCase);

                if (!isVoThoiHan)
                {
                    if (dtNgayBatDau.Value > dtNgayKetThuc.Value)
                    {
                        MessageBox.Show("Ngày bắt đầu không thể lớn hơn ngày kết thúc.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtNgayKy.Value < dtNgayBatDau.Value || dtNgayKy.Value > dtNgayKetThuc.Value)
                    {
                        MessageBox.Show("Ngày ký phải nằm trong khoảng thời gian hợp đồng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                bool isThem = _them;
                string currentSoHD = _SOHD;
                DateTime ngayBatDau = dtNgayBatDau.Value;
                DateTime? ngayKetThuc = isVoThoiHan ? (DateTime?)null : dtNgayKetThuc.Value;
                DateTime ngayKy = dtNgayKy.Value;
                string thoiHan = cbThoiHan.Text;
                decimal loaiHd = Convert.ToDecimal(cbLoaiHD.SelectedValue);
                decimal heSoLuong = Convert.ToDecimal(spHeSoLuong.EditValue ?? 1);
                decimal luongThoaThuan = Convert.ToDecimal(spLuongThoaThuan.EditValue ?? 0);
                int lanKy = Convert.ToInt32(spLanKy.EditValue ?? 1);
                string rawNoiDung = txtNoiDung.Text ?? string.Empty;
                string noiDung = rawNoiDung.Length > 4000 ? rawNoiDung.Substring(0, 4000) : rawNoiDung;

                var allowances = new Dictionary<int, decimal>
                {
                    { 1, Convert.ToDecimal(spPC_NhaO.EditValue ?? 0) },
                    { 2, Convert.ToDecimal(spPC_DiLai.EditValue ?? 0) },
                    { 3, Convert.ToDecimal(spPC_GiaDinh.EditValue ?? 0) },
                    { 4, Convert.ToDecimal(spPC_NguoiPhuThuoc.EditValue ?? 0) },
                    { 5, Convert.ToDecimal(spPC_ChucVu.EditValue ?? 0) },
                    { 6, Convert.ToDecimal(spPC_ChungChi.EditValue ?? 0) },
                    { 7, Convert.ToDecimal(spPC_KyNang.EditValue ?? 0) },
                    { 8, Convert.ToDecimal(spPC_KhuVuc.EditValue ?? 0) },
                    { 9, Convert.ToDecimal(spPC_ChuyenCan.EditValue ?? 0) },
                    { 10, Convert.ToDecimal(spPC_ThamNien.EditValue ?? 0) },
                    { 11, Convert.ToDecimal(spPC_LamViecTaiNha.EditValue ?? 0) },
                    { 12, Convert.ToDecimal(spPC_DacBiet.EditValue ?? 0) },
                    { 13, Convert.ToDecimal(spPC_Khac.EditValue ?? 0) }
                };

                await Task.Run(() =>
                {
                    TB_HOPDONG hd;
                    if (isThem)
                    {
                        var maxSoHD = _hdld.MaxSoHopDong();
                        int so = 1;
                        if (!string.IsNullOrEmpty(maxSoHD) && maxSoHD.Length >= 5 && int.TryParse(maxSoHD.Substring(0, 5), out int parseSo))
                        {
                            so = parseSo + 1;
                        }

                        hd = new TB_HOPDONG();
                        hd.SOHD = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/HĐLĐ";
                        hd.CREATED_BY = 1;
                        hd.CREATED_DATE = DateTime.Now;
                    }
                    else
                    {
                        hd = _hdld.getItem(currentSoHD);
                        if (hd == null)
                        {
                            throw new Exception("Không tìm thấy hợp đồng: " + currentSoHD);
                        }
                        hd.UPDATE_BY = 1;
                        hd.UPDATE_DATE = DateTime.Now;
                    }

                    hd.MANV = manv;
                    hd.NGAYBATDAU = ngayBatDau;
                    hd.NGAYKETTHUC = ngayKetThuc;
                    hd.NGAYKY = ngayKy;
                    hd.THOIHAN = thoiHan;
                    hd.LOAIHD = loaiHd;
                    hd.HESOLUONG = heSoLuong;
                    hd.LUONG_THOA_THUAN = luongThoaThuan;
                    hd.LANKY = lanKy;
                    hd.NOIDUNG = noiDung;
                    hd.IDCTY = 1;

                    if (isThem)
                    {
                        _hdld.Add(hd);
                    }
                    else
                    {
                        _hdld.Update(hd);
                    }

                    _phucap.SavePhuCapHopDong(manv, allowances);
                });

                MessageBox.Show("Lưu hợp đồng và 13 loại phụ cấp thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadDataAsync();
                _them = false;
                showHide(true);
                splitContainer1.Panel1Collapsed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void gvDsHDLD_Click(object sender, EventArgs e)
        {
            if (gvDsHDLD.RowCount > 0 && gvDsHDLD.FocusedRowHandle >= 0)
            {
                var rowVal = gvDsHDLD.GetFocusedRowCellValue("SOHD");
                if (rowVal == null) return;
                _SOHD = rowVal.ToString();
                string sohd = _SOHD;
                var hd = await Task.Run(() => _hdld.getItem(sohd));
                if (hd == null) return;

                txtSoHD.Text = _SOHD;
                dtNgayBatDau.Value = hd.NGAYBATDAU ?? DateTime.Now;
                dtNgayKy.Value = hd.NGAYKY ?? DateTime.Now;
                cbThoiHan.Text = hd.THOIHAN ?? "1 Năm";

                bool isVoThoiHan = (hd.THOIHAN == "Vô thời hạn") || !hd.NGAYKETTHUC.HasValue;
                if (isVoThoiHan)
                {
                    cbThoiHan.Text = "Vô thời hạn";
                    dtNgayKetThuc.Enabled = false;
                }
                else
                {
                    dtNgayKetThuc.Value = hd.NGAYKETTHUC.Value;
                    dtNgayKetThuc.Enabled = true;
                }

                spHeSoLuong.EditValue = hd.HESOLUONG ?? 1;
                spLuongThoaThuan.EditValue = hd.LUONG_THOA_THUAN ?? 0;
                spLanKy.EditValue = hd.LANKY ?? 1;
                searchMANV.EditValue = hd.MANV;
                if (hd.LOAIHD.HasValue)
                {
                    cbLoaiHD.SelectedValue = hd.LOAIHD.Value;
                }
                else
                {
                    cbLoaiHD.SelectedIndex = -1;
                }
                txtNoiDung.Text = StripRtfIfNeeded(hd.NOIDUNG);

                if (hd.MANV.HasValue)
                {
                    await LoadPhuCapForNhanVienAsync(Convert.ToInt32(hd.MANV.Value));
                }

                _lstHD = await Task.Run(() => _hdld.getItem_FULL(sohd));
            }
        }

        private void cbThoiHan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbThoiHan.SelectedItem == null) return;
            string selected = cbThoiHan.SelectedItem.ToString();

            if (selected == "Vô thời hạn")
            {
                dtNgayKetThuc.Enabled = false;
                return;
            }

            dtNgayKetThuc.Enabled = true;
            DateTime startDate = dtNgayBatDau.Value;

            switch (selected)
            {
                case "3 Tháng":
                    dtNgayKetThuc.Value = startDate.AddMonths(3);
                    break;
                case "6 Tháng":
                    dtNgayKetThuc.Value = startDate.AddMonths(6);
                    break;
                case "9 Tháng":
                    dtNgayKetThuc.Value = startDate.AddMonths(9);
                    break;
                case "1 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(1);
                    break;
                case "2 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(2);
                    break;
                case "3 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(3);
                    break;
                case "4 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(4);
                    break;
                case "5 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(5);
                    break;
                case "6 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(6);
                    break;
            }
        }

        private void gvDsHDLD_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.Name == "DEL_BY")
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
    }
}