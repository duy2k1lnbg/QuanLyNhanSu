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
    public partial class FrmDieuChuyen_NhanVien : DevExpress.XtraEditors.XtraForm
    {
        public FrmDieuChuyen_NhanVien()
        {
            InitializeComponent();
        }

        private bool _them;
        private string _SOQD;
        private DIEUCHUYEN_NHANVIEN _dcnv;
        private NHANVIEN _nhanvien;
        private PHONGBAN _phongban;

        private void FrmDieuChuyen_NhanVien_Load(object sender, EventArgs e)
        {
            _dcnv = new DIEUCHUYEN_NHANVIEN();
            _nhanvien = new NHANVIEN();
            _phongban = new PHONGBAN();
            _them = false;
            showHide(true);
            LoadData();
            loadNhanVien();
            LoadDonVi();
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
            gcDsDC.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _dcnv.Delete(_SOQD);
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
            //_lstKL = _ktkl.getItem_FULL(2, _SOQD);
            //rptKyLuat rpt = new rptKyLuat(_lstKL);
            //rpt.ShowRibbonPreview();
        }

        private void gvDsDC_Click(object sender, EventArgs e)
        {
            _SOQD = gvDsDC.GetFocusedRowCellValue("SOQDDIEUCHUYEN").ToString();
            var dc = _dcnv.getItem(_SOQD);

            txtSoQD.Text = _SOQD;
            dtNgayDC.Value = dc.NGAYDC.Value;
            searchMANV.EditValue = dc.MANV;
            txtLyDoDC.Text = dc.LYDODC;
            txtGhiChu.Text = dc.GHICHU;
            cboDieuChuyen.SelectedValue = dc.MAPB2;
            //_lstKL = _ktkl.getItem_FULL(2, _SOQD);
        }

        private void gvDsDC_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
            gcDsDC.Enabled = kt;
            txtLyDoDC.Enabled = !kt;
            txtGhiChu.Enabled = !kt;
            txtSoQD.Enabled = !kt;
            dtNgayDC.Enabled = !kt;
            cboDieuChuyen.Enabled = !kt;
            searchMANV.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoQD.Text = string.Empty;
            txtLyDoDC.Text = string.Empty;
            txtGhiChu.Text = string.Empty;
            dtNgayDC.Value = DateTime.Now;
            cboDieuChuyen.Text = "Vui lòng chọn";
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
            gcDsDC.DataSource = _dcnv.getListFull();
            FormManager_Functions.CustomView_Colums(gvDsDC);
        }

        private void LoadDonVi()
        {
            cboDieuChuyen.DataSource = _phongban.getList();
            cboDieuChuyen.ValueMember = "IDPB";
            cboDieuChuyen.DisplayMember = "TENPB";
        }

        private void SaveData()
        {
            TB_DIEUCHUYEN_NHANVIEN dc;
            try
            {
                if (_them)
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(txtGhiChu.Text))
                    {
                        MessageBox.Show("Vui lòng điền ghi chú.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtLyDoDC.Text))
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
                    var maxSoQD = _dcnv.MaxSoQuyetDinh();
                    int so = int.Parse(maxSoQD.Substring(0, 5)) + 1;

                    dc = new TB_DIEUCHUYEN_NHANVIEN();
                    dc.SOQDDIEUCHUYEN = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/QĐĐC";
                    dc.NGAYDC = dtNgayDC.Value;
                    dc.LYDODC = txtLyDoDC.Text;
                    dc.GHICHU = txtGhiChu.Text;
                    dc.MANV = int.Parse(searchMANV.EditValue.ToString());
                    dc.MAPB = _nhanvien.getItem(int.Parse(searchMANV.EditValue.ToString())).IDPB;
                    dc.MAPB2 = int.Parse(cboDieuChuyen.SelectedValue.ToString());
                    dc.CREATED_BY = 1;
                    dc.CREATED_DATE = DateTime.Now;
                    _dcnv.Add(dc);
                }
                else
                {
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    dc = _dcnv.getItem(_SOQD);
                    if (dc == null)
                    {
                        MessageBox.Show("Không tìm thấy hợp đồng với số hợp đồng: " + _SOQD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    dc.NGAYDC = dtNgayDC.Value;
                    dc.LYDODC = txtLyDoDC.Text;
                    dc.GHICHU = txtGhiChu.Text;
                    dc.MANV = int.Parse(searchMANV.EditValue.ToString());
                    dc.MAPB2 = int.Parse(cboDieuChuyen.SelectedValue.ToString());
                    dc.UPDATED_BY = 1;
                    dc.UPDATED_DATE = DateTime.Now;
                    _dcnv.Update(dc);
                }
                var nv = _nhanvien.getItem((int)dc.MANV.Value);
                nv.IDPB = dc.MAPB2;
                _nhanvien.Update(nv);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gcDsDC_Click(object sender, EventArgs e)
        {

        }
    }
}