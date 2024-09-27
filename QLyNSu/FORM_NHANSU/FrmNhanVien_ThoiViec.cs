using Bu;
using DA;
using DevExpress.XtraEditors;
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
    public partial class FrmNhanVien_ThoiViec : DevExpress.XtraEditors.XtraForm
    {
        public FrmNhanVien_ThoiViec()
        {
            InitializeComponent();
        }

        private bool _them;
        private string _SOQD;
        private NHANVIEN_THOIVIEC _nvtv;
        private NHANVIEN _nhanvien;

        private void FrmNhanVien_ThoiViec_Load(object sender, EventArgs e)
        {
            _nvtv = new NHANVIEN_THOIVIEC();
            _nhanvien = new NHANVIEN();
            _them = false;
            showHide(true);
            LoadData();
            loadNhanVien();
            splitContainer1.Panel1Collapsed = true;
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = true;
            showHide(false);
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDsTV.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá đi không ?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _nvtv.Delete(_SOQD, 1);
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
            gcDsTV.Enabled = kt;
            txtLyDoTV.Enabled = !kt;
            txtGhiChuTV.Enabled = !kt;
            txtSoQD.Enabled = !kt;
            dtNgayNopDon.Enabled = !kt;
            dtNgayNghiViec.Enabled = !kt;
            searchMANV.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoQD.Text = string.Empty;
            txtLyDoTV.Text = string.Empty;
            txtGhiChuTV.Text = string.Empty;
            dtNgayNopDon.Value = DateTime.Now;
            dtNgayNghiViec.Value = dtNgayNopDon.Value.AddDays(45);
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
        }

        private void loadNhanVien()
        {
            searchMANV.Properties.DataSource = _nhanvien.getList();
            searchMANV.Properties.ValueMember = "MANV";
            searchMANV.Properties.DisplayMember = "HOTEN";
        }

        private void LoadData()
        {
            gcDsTV.DataSource = _nvtv.getListFull();
            FormManager_Functions.CustomView_Colums(gvDsTV);
        }

        private void SaveData()
        {
            TB_NHANVIEN_THOIVIEC tv;
            try
            {
                if (_them)
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(txtGhiChuTV.Text))
                    {
                        MessageBox.Show("Vui lòng điền ghi chú.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtLyDoTV.Text))
                    {
                        MessageBox.Show("Vui lòng điền lý do.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var maxSoQD = _nvtv.MaxSoQuyetDinh();
                    int so = int.Parse(maxSoQD.Substring(0, 5)) + 1;

                    tv = new TB_NHANVIEN_THOIVIEC();
                    tv.SOQDTV = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/QĐTV";
                    tv.NGAYNOPDON = dtNgayNopDon.Value;
                    tv.NGAYNGHIVIEC = dtNgayNghiViec.Value;
                    tv.LYDOTV = txtLyDoTV.Text;
                    tv.GHICHUTV = txtGhiChuTV.Text;
                    tv.MANV = int.Parse(searchMANV.EditValue.ToString());
                    tv.CREATED_BY = 1;
                    tv.CREATED_DATE = DateTime.Now;
                    _nvtv.Add(tv);
                }
                else
                {
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    tv = _nvtv.getItem(_SOQD);
                    if (tv == null)
                    {
                        MessageBox.Show("Không tìm thấy hợp đồng với số hợp đồng: " + _SOQD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    tv.NGAYNOPDON = dtNgayNopDon.Value;
                    tv.NGAYNGHIVIEC = dtNgayNghiViec.Value;
                    tv.LYDOTV = txtLyDoTV.Text;
                    tv.GHICHUTV = txtGhiChuTV.Text;
                    tv.MANV = int.Parse(searchMANV.EditValue.ToString());
                    tv.UPDATED_BY = 1;
                    tv.UPDATED_DATE = DateTime.Now;
                    _nvtv.Update(tv);
                }
                var nv = _nhanvien.getItem((int)tv.MANV.Value);
                nv.DATHOIVIEC = 1;
                _nhanvien.Update(nv);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gvDsTV_Click(object sender, EventArgs e)
        {
            _SOQD = gvDsTV.GetFocusedRowCellValue("SOQDTV").ToString();
            var tv = _nvtv.getItem(_SOQD);

            txtSoQD.Text = _SOQD;
            dtNgayNopDon.Value = tv.NGAYNOPDON.Value;
            dtNgayNghiViec.Value = tv.NGAYNGHIVIEC.Value;
            searchMANV.EditValue = tv.MANV;
            txtLyDoTV.Text = tv.LYDOTV;
            txtGhiChuTV.Text = tv.GHICHUTV;
            //_lstKL = _ktkl.getItem_FULL(2, _SOQD);
        }

        private void gvDsTV_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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

        private void dtNgayNghiViec_ValueChanged(object sender, EventArgs e)
        {
            //dtNgayNghiViec.Value = dtNgayNopDon.Value.AddDays(45);
        }

        private void dtNgayNopDon_ValueChanged(object sender, EventArgs e)
        {
            dtNgayNghiViec.Value = dtNgayNopDon.Value.AddDays(45);
        }
    }
}