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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmLoaiCa : DevExpress.XtraEditors.XtraForm
    {
        public FrmLoaiCa()
        {
            InitializeComponent();
        }

        private LOAICA _loaica;
        bool _them;
        int _IDLOAICA;

        private async void FrmLoaiCa_Load(object sender, EventArgs e)
        {
            _them = false;
            _loaica = new LOAICA();
            showHide(true);
            await LoadData();
            if (IsDisposed || Disposing) return;
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if(gvDanhSach.RowCount > 0)
            {
                _IDLOAICA = int.Parse(gvDanhSach.GetFocusedRowCellValue("IDLOAICA").ToString());
                txtTen.Text = gvDanhSach.GetFocusedRowCellValue("TENLOAICA").ToString();
                spHeSo.Text = gvDanhSach.GetFocusedRowCellValue("HESOLOAICA").ToString();
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
            txtTen.Text = string.Empty;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
        }

        private async void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _loaica.Delete(_IDLOAICA, 1);
                await LoadData();
            if (IsDisposed || Disposing) return;
            }
        }

        private async void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
            await LoadData();
            if (IsDisposed || Disposing) return;
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
            txtTen.Enabled = !kt;
            spHeSo.Enabled = !kt;
        }

        private async Task LoadData()
        {
            try
            {
            gcDanhSach.DataSource = await Functions.FormDataLoader.ReadAsync(this, () => _loaica.getList());
            FormManager_Functions.CustomView_Colums(gvDanhSach);
                    }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
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
                    TB_LOAICA lc = new TB_LOAICA();
                    lc.TENLOAICA = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                    lc.HESOLOAICA = decimal.Parse(spHeSo.EditValue.ToString());
                    lc.CREATED_BY = 1;
                    lc.CREATED_DATE = DateTime.Now;
                    _loaica.Add(lc);
                }
                else
                {
                    var lc = _loaica.getItem(_IDLOAICA);
                    if (lc != null)
                    {
                        lc.TENLOAICA = string.IsNullOrEmpty(txtTen.Text) ? null : txtTen.Text;
                        lc.HESOLOAICA = decimal.Parse(spHeSo.EditValue.ToString());
                        lc.UPDATED_BY = 1;
                        lc.UPDATED_DATE = DateTime.Now;
                        _loaica.Update(lc);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _IDLOAICA);
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