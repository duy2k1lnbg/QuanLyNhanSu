using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.DirectX.Common.DirectWrite;
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
    public partial class FrmTangCa : DevExpress.XtraEditors.XtraForm
    {
        public FrmTangCa()
        {
            InitializeComponent();
        }

        private TANGCA _tangca;
        private NHANVIEN _nhanvien;
        private LOAICA _loaica;
        private bool _them;
        private int _id;
        private SYS_CONFIG _config;

        private void FrmTangCa_Load(object sender, EventArgs e)
        {
            _loaica = new LOAICA();
            _nhanvien = new NHANVIEN();
            _tangca = new TANGCA();
            _config = new SYS_CONFIG();
            _them = false;
            showHide(true);
            LoadData();
            LoadNhanVien();
            LoadCombo();
            splitContainer1.Panel1Collapsed = true;
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if(gvDanhSach.RowCount > 0)
            {
                _id = int.Parse(gvDanhSach.GetFocusedRowCellValue("IDTCA").ToString());
                txtGhiChu.Text = gvDanhSach.GetFocusedRowCellValue("GHICHU").ToString();
                searchMANV.EditValue = gvDanhSach.GetFocusedRowCellValue("MANV").ToString();
                cbLoaiCa.SelectedValue = gvDanhSach.GetFocusedRowCellValue("IDLOAICA").ToString();
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
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDanhSach.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _tangca.Delete(_id, 1);
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
            cbLoaiCa.Enabled = !kt;
            txtGhiChu.Enabled = !kt;
            spSoGio.Enabled = !kt;
            searchMANV.Enabled = !kt;
        }

        private void _reset()
        {
            txtGhiChu.Text = string.Empty;
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
            cbLoaiCa.SelectedIndex = 0;
            spSoGio.EditValue = 1;
            searchMANV.EditValue = 0;
        }

        private void LoadNhanVien()
        {
            searchMANV.Properties.DataSource = _nhanvien.getListFll_DTO();
            searchMANV.Properties.ValueMember = "MANV";
            searchMANV.Properties.DisplayMember = "HOTEN";
        }
        private void LoadData()
        {
            gcDanhSach.DataSource = _tangca.getListFull();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void LoadCombo()
        {
            cbLoaiCa.DataSource = _loaica.getList();
            cbLoaiCa.DisplayMember = "TENLOAICA";
            cbLoaiCa.ValueMember = "IDLOAICA";
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    TB_TANGCA tc = new TB_TANGCA();
                    tc.IDLOAICA = int.Parse(cbLoaiCa.SelectedValue.ToString());
                    tc.SOGIO = decimal.Parse(spSoGio.EditValue.ToString());
                    tc.MANV = int.Parse(searchMANV.EditValue.ToString());
                    tc.GHICHU = txtGhiChu.Text;

                    var lc = _loaica.getItem(int.Parse(cbLoaiCa.SelectedValue.ToString()));
                    var cg = _config.getItem("TANGCA");
                    tc.SOTIENTC = tc.SOGIO * lc.HESOLOAICA * int.Parse(cg.VALUE);

                    tc.NAM = DateTime.Now.Year;
                    tc.THANG = DateTime.Now.Month;
                    tc.NGAY = DateTime.Now.Day;
                    tc.CREATED_BY = 1;
                    tc.CREATED_DATE = DateTime.Now;
                    _tangca.Add(tc);
                }
                else 
                {
                    var tc = _tangca.getItem(_id);
                    tc.IDLOAICA = int.Parse(cbLoaiCa.SelectedValue.ToString());
                    tc.SOGIO = decimal.Parse(spSoGio.EditValue.ToString());
                    tc.MANV = int.Parse(searchMANV.EditValue.ToString());
                    tc.GHICHU = txtGhiChu.Text;

                    var lc = _loaica.getItem(int.Parse(cbLoaiCa.SelectedValue.ToString()));
                    var cg = _config.getItem("TANGCA");
                    tc.SOTIENTC = tc.SOGIO * lc.HESOLOAICA * int.Parse(cg.VALUE);

                    tc.NAM = DateTime.Now.Year;
                    tc.THANG = DateTime.Now.Month;
                    tc.NGAY = DateTime.Now.Day;
                    tc.UPDATED_BY = 1;
                    tc.UPDATED_DATE = DateTime.Now;
                    _tangca.Update(tc);
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}