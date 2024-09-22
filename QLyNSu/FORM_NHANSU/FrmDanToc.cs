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
    public partial class FrmDanToc : DevExpress.XtraEditors.XtraForm
    {
        public FrmDanToc()
        {
            InitializeComponent();
        }

        private DANTOC _dantoc;
        bool _them;
        int _IDDT;

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
        private void labelControl1_Click(object sender, EventArgs e)
        {

        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
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
            if (_IDDT <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _dantoc.Delete(_IDDT);
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

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void FrmDanToc_Load(object sender, EventArgs e)
        {
            _them= false;
            _dantoc = new DANTOC();
            showHide(true);
            LoadData();
        }

        private void LoadData()
        {
            gcDsDT.DataSource = _dantoc.getList();
            FormManager_Functions.CustomView_Colums(gvDsDT);
        }

        private void SaveData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTen.Text))
                {
                    MessageBox.Show("Vui lòng điền tên dân tộc.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_them)
                {
                    TB_DANTOC dt = new TB_DANTOC();
                    //dt.TENDT = txtTen.Text;
                    dt.TENDT = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    _dantoc.Add(dt);
                }
                else
                {
                    var dt = _dantoc.getItem(_IDDT);
                    if (dt != null)
                    {
                        //dt.TENDT = txtTen.Text;
                        dt.TENDT = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        _dantoc.Update(dt);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDDT);
                    }    
                }
            }
            catch (Exception ex)
            {

                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gvDsDT_Click(object sender, EventArgs e)
        {
            //_IDDT = int.Parse(gvDsDT.GetFocusedRowCellValue("IDDT").ToString());
            //txtTen.Text = gvDsDT.GetFocusedRowCellValue("TENDT").ToString();
            // Kiểm tra nếu IDDT có giá trị trước khi gán
            if (gvDsDT.GetFocusedRowCellValue("IDDT") != null)
            {
                _IDDT = int.Parse(gvDsDT.GetFocusedRowCellValue("IDDT").ToString());
            }
            else
            {
                // Gán ID mặc định hoặc xử lý nếu cần
                _IDDT = -1;
            }

            // Kiểm tra nếu TENDT không bị null hoặc rỗng
            var tendtValue = gvDsDT.GetFocusedRowCellValue("TENDT");
            if (tendtValue != null && !string.IsNullOrEmpty(tendtValue.ToString()))
            {
                txtTen.Text = tendtValue.ToString();
            }
            else
            {
                // Gán giá trị mặc định nếu TENDT trống
                txtTen.Text = string.Empty;
            }
        }

        private void barDockControlTop_Click(object sender, EventArgs e)
        {

        }

        private void barDockControlBottom_Click(object sender, EventArgs e)
        {

        }

        private void barDockControlLeft_Click(object sender, EventArgs e)
        {

        }

        private void barDockControlRight_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void txtTen_TextChanged(object sender, EventArgs e)
        {

        }

        private void gcDsDT_Click(object sender, EventArgs e)
        {

        }
    }
}