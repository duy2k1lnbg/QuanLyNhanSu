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
    public partial class FrmBoPhan : DevExpress.XtraEditors.XtraForm
    {
        public FrmBoPhan()
        {
            InitializeComponent();
        }

        private BOPHAN _bophan;
        bool _them;
        int _IDBP;

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
        }

        private void LoadData()
        {
            gcDsBP.DataSource = _bophan.getList();
            FormManager_Functions.CustomView_Colums(gvDsBP);
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    TB_BOPHAN bp = new TB_BOPHAN();
                    //bp.TENBP = txtTen.Text;
                    bp.TENBP = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    _bophan.Add(bp);
                }
                else
                {
                    var bp = _bophan.getItem(_IDBP);
                    if (bp != null)
                    {
                        //bp.TENBP = txtTen.Text;
                        bp.TENBP = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        _bophan.Update(bp);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDBP);
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
        }

        private void gvDsBP_Click(object sender, EventArgs e)
        {
            //_IDBP = int.Parse(gvDsBP.GetFocusedRowCellValue("IDBP").ToString());
            //txtTen.Text = gvDsBP.GetFocusedRowCellValue("TENBP").ToString();

            if (gvDsBP.GetFocusedRowCellValue("IDBP") != null)
            {
                _IDBP = int.Parse(gvDsBP.GetFocusedRowCellValue("IDBP").ToString());
            }
            else
            {
                _IDBP = -1;
            }

            var tendtValue = gvDsBP.GetFocusedRowCellValue("TENBP");
            if (tendtValue != null && !string.IsNullOrEmpty(tendtValue.ToString()))
            {
                txtTen.Text = tendtValue.ToString();
            }
            else
            {
                txtTen.Text = string.Empty;
            }
        }

        private void FrmBoPhan_Load(object sender, EventArgs e)
        {
            _them = false;
            _bophan = new BOPHAN();
            showHide(true);
            LoadData();

        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Kiểm tra xem _IDTG có giá trị hợp lệ không
            if (_IDBP <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _bophan.Delete(_IDBP);
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
    }
}