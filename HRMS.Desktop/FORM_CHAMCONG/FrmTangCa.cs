using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmTangCa : DevExpress.XtraEditors.XtraForm
    {
        public FrmTangCa()
        {
            InitializeComponent();
        }

        private TANGCA _tangca;
        private NHANVIEN _nhanvien;
        private LOAICA _loaica;
        private LOAICONG _loaicong;
        private bool _them;
        private int _id;
        private bool _isBinding = false;

        private void FrmTangCa_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_CC_TANGCA")) return;
            _loaica = new LOAICA();
            _loaicong = new LOAICONG();
            _nhanvien = new NHANVIEN();
            _tangca = new TANGCA();
            _them = false;

            showHide(true);
            LoadData();
            LoadNhanVien();
            LoadCombo();
            splitContainer1.Panel1Collapsed = true;
            Functions.TranslationManager.Translate(this);

            // Đăng ký sự kiện tính toán tự động
            searchMANV.EditValueChanged += (s, ev) => { if (!_isBinding) Recalculate(); };
            dtNgay.ValueChanged += (s, ev) => { if (!_isBinding) OnDateChanged(); };
            cbLoaiCa.SelectedIndexChanged += (s, ev) => { if (!_isBinding) Recalculate(); };
            cbLoaiCong.SelectedIndexChanged += (s, ev) => { if (!_isBinding) Recalculate(); };
            txtGioBatDau.TextChanged += (s, ev) => { if (!_isBinding) Recalculate(); };
            txtGioKetThuc.TextChanged += (s, ev) => { if (!_isBinding) Recalculate(); };
        }

        private void OnDateChanged()
        {
            // Tự động nhận diện Loại công theo ngày: Ngày lễ -> Chủ nhật -> Ngày thường
            NGAYLE ngayLeBus = new NGAYLE();
            int idLoaiCong = ngayLeBus.XacDinhLoaiCong(dtNgay.Value);
            cbLoaiCong.SelectedValue = idLoaiCong;

            Recalculate();
        }

        private void Recalculate()
        {
            try
            {
                string gbd = txtGioBatDau.Text.Trim();
                string gkt = txtGioKetThuc.Text.Trim();

                double soGio = _tangca.TinhSoGio(gbd, gkt);
                spSoGio.EditValue = (decimal)soGio;

                if (searchMANV.EditValue != null && int.TryParse(searchMANV.EditValue.ToString(), out int manv) && manv > 0)
                {
                    bool isThuViec = _tangca.KiemTraThuViec(manv);
                    txtTrangThaiNV.Text = isThuViec ? "Thử việc (85%)" : "Chính thức (100%)";
                    txtTrangThaiNV.ForeColor = isThuViec ? Color.DarkOrange : Color.Green;

                    int idLoaiCa = cbLoaiCa.SelectedValue != null ? Convert.ToInt32(cbLoaiCa.SelectedValue) : 1;
                    int idLoaiCong = cbLoaiCong.SelectedValue != null ? Convert.ToInt32(cbLoaiCong.SelectedValue) : 1;

                    var calc = _tangca.TinhChiTietTangCa(manv, dtNgay.Value, idLoaiCa, idLoaiCong, gbd, gkt, isThuViec);

                    txtQuyDinh.Text = calc.TenQuyDinh;
                    spHeSo.EditValue = calc.HeSo;
                    spDonGiaOT.EditValue = calc.DonGia1Gio;
                    spThanhTien.EditValue = calc.ThanhTien;
                }
                else
                {
                    txtTrangThaiNV.Text = "Chưa chọn NV";
                    txtTrangThaiNV.ForeColor = Color.Gray;
                    txtQuyDinh.Text = "Chưa xác định";
                    spHeSo.EditValue = 0m;
                    spDonGiaOT.EditValue = 0m;
                    spThanhTien.EditValue = 0m;
                }
            }
            catch
            {
                // Bỏ qua lỗi tính nháp khi người dùng đang nhập dở
            }
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.RowCount > 0 && gvDanhSach.GetFocusedRowCellValue("IDTCA") != null)
            {
                _isBinding = true;
                try
                {
                    _id = int.Parse(gvDanhSach.GetFocusedRowCellValue("IDTCA").ToString());
                    txtGhiChu.Text = gvDanhSach.GetFocusedRowCellValue("GHICHU")?.ToString() ?? "";
                    searchMANV.EditValue = gvDanhSach.GetFocusedRowCellValue("MANV");

                    var ngayObj = gvDanhSach.GetFocusedRowCellValue("NGAY_FULL");
                    if (ngayObj != null && DateTime.TryParse(ngayObj.ToString(), out var dt))
                    {
                        dtNgay.Value = dt;
                    }

                    if (gvDanhSach.GetFocusedRowCellValue("IDLOAICA") != null)
                    {
                        cbLoaiCa.SelectedValue = Convert.ToInt32(gvDanhSach.GetFocusedRowCellValue("IDLOAICA"));
                    }

                    if (gvDanhSach.GetFocusedRowCellValue("IDLOAICONG") != null)
                    {
                        cbLoaiCong.SelectedValue = Convert.ToInt32(gvDanhSach.GetFocusedRowCellValue("IDLOAICONG"));
                    }
                    else
                    {
                        cbLoaiCong.SelectedValue = 1;
                    }

                    txtGioBatDau.Text = gvDanhSach.GetFocusedRowCellValue("GIOBATDAU")?.ToString() ?? "17:00";
                    txtGioKetThuc.Text = gvDanhSach.GetFocusedRowCellValue("GIOKETTHUC")?.ToString() ?? "21:00";
                    spSoGio.EditValue = gvDanhSach.GetFocusedRowCellValue("SOGIO") != null ? Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOGIO")) : 0m;
                    txtTrangThaiNV.Text = gvDanhSach.GetFocusedRowCellValue("TRANGTHAI_NV")?.ToString() ?? "Chính thức (100%)";
                    spHeSo.EditValue = gvDanhSach.GetFocusedRowCellValue("HESOTC") != null ? Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("HESOTC")) : 1.5m;
                    spDonGiaOT.EditValue = gvDanhSach.GetFocusedRowCellValue("DONGIATC") != null ? Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("DONGIATC")) : 0m;
                    spThanhTien.EditValue = gvDanhSach.GetFocusedRowCellValue("SOTIENTC") != null ? Convert.ToDecimal(gvDanhSach.GetFocusedRowCellValue("SOTIENTC")) : 0m;
                }
                finally
                {
                    _isBinding = false;
                }
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
            if (!Functions.FormSecurity.AssertPermission("F_CC_TANGCA", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_TANGCA", Bu.DTO.PermissionAction.Edit)) return;
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDanhSach.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_TANGCA", Bu.DTO.PermissionAction.Delete)) return;
            splitContainer1.Panel1Collapsed = true;
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa bản ghi tăng ca này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _tangca.Delete(_id, 1);
                LoadData();
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (SaveData())
            {
                LoadData();
                _them = false;
                showHide(true);
                splitContainer1.Panel1Collapsed = true;
            }
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
            if (!Functions.FormSecurity.AssertPermission("F_CC_TANGCA", Bu.DTO.PermissionAction.Print)) return;
            // In danh sách tăng ca nếu cần
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_CC_TANGCA", btnThem, btnSua, btnXoa, btnIn, kt);

            searchMANV.Enabled = !kt;
            dtNgay.Enabled = !kt;
            cbLoaiCa.Enabled = !kt;
            cbLoaiCong.Enabled = !kt;
            txtGioBatDau.Enabled = !kt;
            txtGioKetThuc.Enabled = !kt;
            txtGhiChu.Enabled = !kt;

            // Các ô tính toán tự động luôn khóa không cho nhập tay
            spSoGio.Properties.ReadOnly = true;
            txtTrangThaiNV.ReadOnly = true;
            txtQuyDinh.ReadOnly = true;
            spHeSo.Properties.ReadOnly = true;
            spDonGiaOT.Properties.ReadOnly = true;
            spThanhTien.Properties.ReadOnly = true;
        }

        private void _reset()
        {
            _isBinding = true;
            try
            {
                searchMANV.EditValue = null;
                dtNgay.Value = DateTime.Now;
                cbLoaiCa.SelectedValue = 1; // Ca ngày
                cbLoaiCong.SelectedValue = dtNgay.Value.DayOfWeek == DayOfWeek.Sunday ? 2 : 1;
                txtGioBatDau.Text = "17:00";
                txtGioKetThuc.Text = "21:00";
                txtGhiChu.Text = string.Empty;
            }
            finally
            {
                _isBinding = false;
            }

            Recalculate();
        }

        private void LoadNhanVien()
        {
            searchMANV.Properties.DataSource = _nhanvien.getListFll_DTO();
            searchMANV.Properties.ValueMember = "MANV";
            searchMANV.Properties.DisplayMember = "HOTEN";
        }

        private void LoadData()
        {
            gcDanhSach.DataSource = _tangca.getListFull();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void LoadCombo()
        {
            cbLoaiCa.DataSource = _loaica.getList();
            cbLoaiCa.DisplayMember = "TENLOAICA";
            cbLoaiCa.ValueMember = "IDLOAICA";

            cbLoaiCong.DataSource = _loaicong.getList();
            cbLoaiCong.DisplayMember = "TENLC";
            cbLoaiCong.ValueMember = "IDLOAICONG";
        }

        private bool SaveData()
        {
            var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
            if (!Functions.FormSecurity.AssertPermission("F_CC_TANGCA", permAction)) return false;

            try
            {
                if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out int manv) || manv <= 0)
                {
                    MessageBox.Show("Vui lòng chọn một nhân viên để ghi nhận tăng ca.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    searchMANV.Focus();
                    return false;
                }

                string gbd = txtGioBatDau.Text.Trim();
                string gkt = txtGioKetThuc.Text.Trim();

                double soGio = _tangca.TinhSoGio(gbd, gkt);
                if (soGio <= 0)
                {
                    MessageBox.Show("Số giờ tăng ca phải lớn hơn 0! Vui lòng kiểm tra lại giờ bắt đầu và giờ kết thúc.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGioBatDau.Focus();
                    return false;
                }

                int idLoaiCa = cbLoaiCa.SelectedValue != null ? Convert.ToInt32(cbLoaiCa.SelectedValue) : 1;
                int idLoaiCong = cbLoaiCong.SelectedValue != null ? Convert.ToInt32(cbLoaiCong.SelectedValue) : 1;
                bool isThuViec = _tangca.KiemTraThuViec(manv);

                // Tính toán chính xác toàn bộ theo quy tắc hệ thống
                var calc = _tangca.TinhChiTietTangCa(manv, dtNgay.Value, idLoaiCa, idLoaiCong, gbd, gkt, isThuViec);

                if (_them)
                {
                    TB_TANGCA tc = new TB_TANGCA
                    {
                        MANV = manv,
                        NAM = dtNgay.Value.Year,
                        THANG = dtNgay.Value.Month,
                        NGAY = dtNgay.Value.Day, // Ngày tăng ca là ngày bắt đầu ca
                        GIOBATDAU = gbd,
                        GIOKETTHUC = gkt,
                        SOGIO = (decimal)calc.SoGio,
                        IDLOAICA = idLoaiCa,
                        IDLOAICONG = idLoaiCong,
                        HESOTC = calc.HeSo,
                        DONGIATC = calc.DonGia1Gio,
                        IS_THUVIEC = calc.IsThuViec ? 1 : 0,
                        SOTIENTC = calc.ThanhTien, // Lưu lịch sử chốt kết quả kỳ lương
                        GHICHU = txtGhiChu.Text.Trim(),
                        CREATED_BY = 1,
                        CREATED_DATE = DateTime.Now
                    };

                    _tangca.Add(tc);
                }
                else
                {
                    var tc = _tangca.getItem(_id);
                    if (tc == null)
                    {
                        MessageBox.Show("Không tìm thấy bản ghi tăng ca để sửa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    tc.MANV = manv;
                    tc.NAM = dtNgay.Value.Year;
                    tc.THANG = dtNgay.Value.Month;
                    tc.NGAY = dtNgay.Value.Day;
                    tc.GIOBATDAU = gbd;
                    tc.GIOKETTHUC = gkt;
                    tc.SOGIO = (decimal)calc.SoGio;
                    tc.IDLOAICA = idLoaiCa;
                    tc.IDLOAICONG = idLoaiCong;
                    tc.HESOTC = calc.HeSo;
                    tc.DONGIATC = calc.DonGia1Gio;
                    tc.IS_THUVIEC = calc.IsThuViec ? 1 : 0;
                    tc.SOTIENTC = calc.ThanhTien; // Cập nhật và lưu lịch sử
                    tc.GHICHU = txtGhiChu.Text.Trim();
                    tc.UPDATED_BY = 1;
                    tc.UPDATED_DATE = DateTime.Now;

                    _tangca.Update(tc);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu tăng ca: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}