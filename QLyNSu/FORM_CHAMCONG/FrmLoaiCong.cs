using Bu.CLASS_CHAMCONG;
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

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmLoaiCong : DevExpress.XtraEditors.XtraForm
    {
        public FrmLoaiCong()
        {
            InitializeComponent();
        }

        private LOAICONG _loaicong;
        bool _them;
        int _IDLOAICONG;

        private void FrmLoaiCong_Load(object sender, EventArgs e)
        {
            _them = false;
            _loaicong = new LOAICONG();
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
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _loaicong.Delete(_IDLOAICONG, 1);
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

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.RowCount > 0)
            {
                _IDLOAICONG = int.Parse(gvDanhSach.GetFocusedRowCellValue("IDLOAICONG").ToString());
                txtTen.Text = gvDanhSach.GetFocusedRowCellValue("TENLC").ToString();
                spHeSo.Text = gvDanhSach.GetFocusedRowCellValue("HESOLOAICONG").ToString();
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
            spHeSo.Enabled = !kt;
        }

        private void LoadData()
        {
            gcDanhSach.DataSource = _loaicong.getList();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void SaveData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTen.Text))
                {
                    MessageBox.Show("Vui lòng điền loại ca.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_them)
                {
                    TB_LOAICONG lc = new TB_LOAICONG();
                    lc.TENLC = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    lc.HESOLOAICONG = decimal.Parse(spHeSo.EditValue.ToString());
                    lc.CREATED_BY = 1;
                    lc.CREATED_DATE = DateTime.Now;
                    _loaicong.Add(lc);
                }
                else
                {
                    var lc = _loaicong.getItem(_IDLOAICONG);
                    if (lc != null)
                    {
                        lc.TENLC = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        lc.HESOLOAICONG = decimal.Parse(spHeSo.EditValue.ToString());
                        lc.UPDATED_BY = 1;
                        lc.UPDATED_DATE = DateTime.Now;
                        _loaicong.Update(lc);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDLOAICONG);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}