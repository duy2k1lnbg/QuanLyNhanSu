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
    public partial class FrmTonGiao : DevExpress.XtraEditors.XtraForm
    {
        public FrmTonGiao()
        {
            InitializeComponent();
        }

        private TONGIAO _tongiao;
        bool _them;
        int _IDTG;
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
            gcDsTG.DataSource = _tongiao.getList();
            gvDsTG.OptionsBehavior.Editable = false;
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    TB_TONGIAO tg = new TB_TONGIAO();
                    //tg.TENTG = txtTen.Text;
                    tg.TENTG = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    _tongiao.Add(tg);
                }
                else
                {
                    var tg = _tongiao.getItem(_IDTG);
                    if (tg != null)
                    {
                        //tg.TENTG = txtTen.Text;
                        tg.TENTG = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        _tongiao.Update(tg);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDTG);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void FrmTonGiao_Load_1(object sender, EventArgs e)
        {
            _them = false;
            _tongiao = new TONGIAO();
            showHide(true);
            LoadData();
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
            if (_IDTG <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _tongiao.Delete(_IDTG);
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

        private void gvDsTG_Click(object sender, EventArgs e)
        {
            //_IDTG = int.Parse(gvDsTG.GetFocusedRowCellValue("IDTG").ToString());
            //txtTen.Text = gvDsTG.GetFocusedRowCellValue("TENTG").ToString();

            if (gvDsTG.GetFocusedRowCellValue("IDTG") != null)
            {
                _IDTG = int.Parse(gvDsTG.GetFocusedRowCellValue("IDTG").ToString());
            }
            else
            {
                _IDTG = -1;
            }

            var tendtValue = gvDsTG.GetFocusedRowCellValue("TENTG");
            if (tendtValue != null && !string.IsNullOrEmpty(tendtValue.ToString()))
            {
                txtTen.Text = tendtValue.ToString();
            }
            else
            {
                txtTen.Text = string.Empty;
            }
        }
    }
}