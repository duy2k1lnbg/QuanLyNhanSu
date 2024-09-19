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
using DA;
using Bu;

namespace QLyNSu
{
    public partial class FrmChucVu : DevExpress.XtraEditors.XtraForm
    {
        public FrmChucVu()
        {
            InitializeComponent();
        }

        private CHUCVU _chucvu;
        bool _them;
        int _IDCV;

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
            gcDsCV.DataSource = _chucvu.getList();
            gvDsCV.OptionsBehavior.Editable = false;
        }

        private void SaveData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTen.Text))
                {
                    MessageBox.Show("Vui lòng điền tên chức vụ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_them)
                {
                    TB_CHUCVU cv = new TB_CHUCVU();
                    //cv.TENCV = txtTen.Text;
                    cv.TENCV = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    _chucvu.Add(cv);
                }
                else
                {
                    var cv = _chucvu.getItem(_IDCV);
                    if (cv != null)
                    {
                        //cv.TENCV = txtTen.Text;
                        cv.TENCV = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        _chucvu.Update(cv);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDCV);
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

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Kiểm tra xem _IDTG có giá trị hợp lệ không
            if (_IDCV <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _chucvu.Delete(_IDCV);
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

        private void gvDsCV_Click(object sender, EventArgs e)
        {
            //_IDCV = int.Parse(gvDsCV.GetFocusedRowCellValue("IDCV").ToString());
            //txtTen.Text = gvDsCV.GetFocusedRowCellValue("TENCV").ToString();

            if (gvDsCV.GetFocusedRowCellValue("IDCV") != null)
            {
                _IDCV = int.Parse(gvDsCV.GetFocusedRowCellValue("IDCV").ToString());
            }
            else
            {
                _IDCV = -1;
            }

            var tendtValue = gvDsCV.GetFocusedRowCellValue("TENCV");
            if (tendtValue != null && !string.IsNullOrEmpty(tendtValue.ToString()))
            {
                txtTen.Text = tendtValue.ToString();
            }
            else
            {
                txtTen.Text = string.Empty;
            }
        }

        private void FrmChucVu_Load(object sender, EventArgs e)
        {
            _them = false;
            _chucvu = new CHUCVU();
            showHide(true);
            LoadData();
        }
    }
}