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
    public partial class FrmCongTy : DevExpress.XtraEditors.XtraForm
    {
        public FrmCongTy()
        {
            InitializeComponent();
        }

        private CONGTY _congty;
        bool _them;
        int _IDCTY;

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnThem.Enabled = kt;
            btnXoa.Enabled = kt;
            btnSua.Enabled = kt;
            btnIn.Enabled = kt;
            btnDong.Enabled = kt;
            txtTen.Enabled = !kt;
            txtSDT.Enabled = !kt;
            txtEMAIL.Enabled = !kt;
            txtDC.Enabled = !kt;
            txtDaiDien.Enabled = !kt;
            txtMaSoThue.Enabled = !kt;
        }

        private void LoadData()
        {
            gcDsCT.DataSource = _congty.getList();
            gvDsCT.OptionsBehavior.Editable = false;
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    TB_CONGTY cty = new TB_CONGTY();
                    cty.TENCTY = txtTen.Text;
                    cty.DIENTHOAICTY = txtSDT.Text;
                    cty.EMAILCTY = txtEMAIL.Text;
                    cty.DIACHICTY = txtDC.Text;
                    cty.DAIDIEN = txtDaiDien.Text;
                    cty.MASOTHUECTY = txtMaSoThue.Text;
                    _congty.Add(cty);
                }
                else
                {
                    var cty = _congty.getItem(_IDCTY);
                    if (cty != null)
                    {
                        cty.TENCTY = txtTen.Text;
                        cty.DIENTHOAICTY = txtSDT.Text;
                        cty.EMAILCTY = txtEMAIL.Text;
                        cty.DIACHICTY = txtDC.Text;
                        cty.DAIDIEN = txtDaiDien.Text;
                        cty.MASOTHUECTY = txtMaSoThue.Text;
                        _congty.Update(cty);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDCTY);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = true;
            showHide(false);
            txtTen.Text = string.Empty;
            txtSDT.Text = string.Empty;
            txtEMAIL.Text = string.Empty;
            txtDC.Text = string.Empty;
            txtMaSoThue.Text = string.Empty;
            txtDaiDien.Text = string.Empty;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Kiểm tra xem _IDTG có giá trị hợp lệ không
            if (_IDCTY <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _congty.Delete(_IDCTY);
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

        private void gvDsCT_Click(object sender, EventArgs e)
        {
            if(gvDsCT.RowCount > 0)
            {
                _IDCTY = int.Parse(gvDsCT.GetFocusedRowCellValue("IDCTY").ToString());
                txtTen.Text = gvDsCT.GetFocusedRowCellValue("TENCTY").ToString();
                txtSDT.Text = gvDsCT.GetFocusedRowCellValue("DIENTHOAICTY").ToString();
                txtEMAIL.Text = gvDsCT.GetFocusedRowCellValue("EMAILCTY").ToString();
                txtDC.Text = gvDsCT.GetFocusedRowCellValue("DIACHICTY").ToString();
                txtDaiDien.Text = gvDsCT.GetFocusedRowCellValue("DAIDIEN").ToString();
                txtMaSoThue.Text = gvDsCT.GetFocusedRowCellValue("MASOTHUECTY").ToString();
            }     
        }

        private void FrmCongTy_Load(object sender, EventArgs e)
        {
            _them = false;
            _congty = new CONGTY();
            showHide(true);
            LoadData();
        }
    }
}