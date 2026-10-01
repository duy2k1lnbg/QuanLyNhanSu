using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using QLyNSu.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int _MAKYCONG;

        private void FrmBangCong_Load(object sender, EventArgs e)
        {
            _them = false;
            _kycong = new KYCONG();
            showHide(true);
            LoadData();
            cboNam.Text = DateTime.Now.Year.ToString();
            cboThang.Text = DateTime.Now.Month.ToString();
        }

        private void SyncFocusedRow()
        {
            if (gvDanhSach.RowCount > 0 && gvDanhSach.FocusedRowHandle >= 0)
            {
                var valMkc = gvDanhSach.GetFocusedRowCellValue("MAKYCONG");
                if (valMkc != null && int.TryParse(valMkc.ToString(), out int mkc))
                {
                    _MAKYCONG = mkc;
                }
                cboNam.Text = gvDanhSach.GetFocusedRowCellValue("NAM")?.ToString() ?? "";
                cboThang.Text = gvDanhSach.GetFocusedRowCellValue("THANG")?.ToString() ?? "";

                chkKhoa.Checked = gvDanhSach.GetFocusedRowCellValue("KHOA")?.ToString() == "1";
                chkTrangThai.Checked = gvDanhSach.GetFocusedRowCellValue("TRANGTHAI")?.ToString() == "1";
            }
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            SyncFocusedRow();
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
            //_reset();
            cboNam.Text = DateTime.Now.Year.ToString();
            cboThang.Text= DateTime.Now.Month.ToString();
            chkKhoa.Checked = false;
            chkTrangThai.Checked = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var kc = _kycong.getItem(_MAKYCONG);
            if (kc != null && (kc.KHOA ?? 0) == 1)
            {
                XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khoá. Khi đã khoá thì không được xoá sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var kc = _kycong.getItem(_MAKYCONG);
            if (kc != null && (kc.KHOA ?? 0) == 1)
            {
                XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khoá. Khi đã khoá thì không được xoá sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _kycong.Delete(_MAKYCONG, 1);
                LoadData();
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
            LoadData();
            _them = false;
            showHide(true);
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
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
            chkKhoa.Enabled = !kt;
            //chkTrangThai.Enabled = !kt;
        }

        //private void _reset()
        //{
        //    cboNam.Text = "Vui lòng chọn";
        //    cboThang.Text = "Vui lòng chọn";
        //}

        private void LoadData()
        {
            _kycong = new KYCONG();
            gcDanhSach.DataSource = _kycong.getList();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
            SyncFocusedRow();
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    int makycong = int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text);
                    if (_kycong.KiemTraMaKyCong(makycong) != 0)
                    {
                        XtraMessageBox.Show($"Kỳ công {makycong} đã tồn tại trong hệ thống. Bảng công chỉ được tạo 1 lần!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    TB_KYCONG kc = new TB_KYCONG();
                    kc.MAKYCONG = makycong;
                    kc.NAM = int.Parse(cboNam.Text);
                    kc.THANG = int.Parse(cboThang.Text);
                    kc.KHOA = chkKhoa.Checked ? 1 : 0;
                    kc.TRANGTHAI = chkTrangThai.Checked ? 1 : 0;
                    kc.NGAYCONGTRONGTHANG = ChamCong_Functions.demSoNgayLamViecTrongThang(int.Parse(cboThang.Text), int.Parse(cboNam.Text));
                    kc.NGAYTINHCONG= DateTime.Now;
                    kc.CREATED_BY = 1;
                    kc.CREATED_DATE = DateTime.Now;
                    _kycong.Add(kc);
                }
                else
                {
                    var kc = _kycong.getItem(_MAKYCONG);
                    if (kc != null)
                    {
                        if ((kc.KHOA ?? 0) == 1 && chkKhoa.Checked)
                        {
                            XtraMessageBox.Show($"Kỳ công {_MAKYCONG} đã bị khoá. Khi đã khoá thì không được xoá sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        kc.MAKYCONG = int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text);
                        kc.NAM = int.Parse(cboNam.Text);
                        kc.THANG = int.Parse(cboThang.Text);
                        kc.KHOA = chkKhoa.Checked ? 1 : 0;
                        kc.TRANGTHAI = chkTrangThai.Checked ? 1 : 0;
                        kc.NGAYCONGTRONGTHANG = ChamCong_Functions.demSoNgayLamViecTrongThang(int.Parse(cboThang.Text), int.Parse(cboNam.Text));
                        kc.NGAYTINHCONG = DateTime.Now;
                        kc.UPDATED_BY = 1;
                        kc.UPDATED_DATE = DateTime.Now;
                        _kycong.Update(kc);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _MAKYCONG);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gvDanhSach_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            // Kiểm tra cột có tên "Trạng thái"
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
                if (val != null && int.TryParse(val.ToString(), out int mkc))
                {
                    _MAKYCONG = mkc;
                }
            }
            if (_MAKYCONG == 0 && int.TryParse(cboNam.Text, out int n) && int.TryParse(cboThang.Text, out int t))
            {
                _MAKYCONG = n * 100 + t;
            }

            FrmBangCong_ChiTiet frm = new FrmBangCong_ChiTiet();
            frm._MAKYCONG = _MAKYCONG;
            frm._thang = int.Parse(cboThang.Text);
            frm._nam = int.Parse(cboNam.Text);
            frm._macty = 1;
            frm.ShowInTaskbar = false;
            frm.ShowDialog();
            LoadData();
        }

        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
        }
    }
}