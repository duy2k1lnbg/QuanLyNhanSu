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

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.RowCount > 0)
            {
                _MAKYCONG = int.Parse(gvDanhSach.GetFocusedRowCellValue("MAKYCONG").ToString());
                cboNam.Text = gvDanhSach.GetFocusedRowCellValue("NAM").ToString();
                cboThang.Text = gvDanhSach.GetFocusedRowCellValue("THANG").ToString();
                //chkKhoa.Checked = bool.Parse(gvDanhSach.GetFocusedRowCellValue("KHOA").ToString());
                //chkTrangThai.Checked = bool.Parse(gvDanhSach.GetFocusedRowCellValue("TRANGTHAI").ToString());

                // Kiểm tra giá trị của cột KHOA (1 = true, 0 = false)
                chkKhoa.Checked = gvDanhSach.GetFocusedRowCellValue("KHOA").ToString() == "1";

                // Kiểm tra giá trị của cột TRANGTHAI (1 = true, 0 = false)
                chkTrangThai.Checked = gvDanhSach.GetFocusedRowCellValue("TRANGTHAI").ToString() == "1";
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
            //_reset();
            cboNam.Text = DateTime.Now.Year.ToString();
            cboThang.Text= DateTime.Now.Month.ToString();
            chkKhoa.Checked = false;
            chkTrangThai.Checked = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá đi không ?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    TB_KYCONG kc = new TB_KYCONG();
                    kc.MAKYCONG = int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text);
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
                if (e.Value.ToString() == "0")
                {
                    e.DisplayText = "Chưa khoá";
                }
                else if (e.Value.ToString() == "1")
                {
                    e.DisplayText = "Đã khoá";
                }
            }
            if (e.Column.FieldName == "TRANGTHAI")
            {
                if (e.Value.ToString() == "0")
                {
                    e.DisplayText = "Chưa Tạo";
                }
                else if (e.Value.ToString() == "1")
                {
                    e.DisplayText = "Đã Tạo";
                }
            }
        }

        private void btnXemBC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FrmBangCong_ChiTiet frm = new FrmBangCong_ChiTiet();
            frm._MAKYCONG = _MAKYCONG;
            frm._thang = int.Parse(cboThang.Text);
            frm._nam = int.Parse(cboNam.Text);
            frm._macty = 1;
            frm.ShowInTaskbar = false;
            frm.ShowDialog();
        }

        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
        }
    }
}