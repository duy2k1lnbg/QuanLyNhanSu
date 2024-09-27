using Bu;
using Bu.DTO;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using QLyNSu.Reports;
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
    public partial class FrmKyLuat : DevExpress.XtraEditors.XtraForm
    {
        public FrmKyLuat()
        {
            InitializeComponent();
        }

        private bool _them;
        private string _SOQD;
        private KHENTHUONG_KYLUAT _ktkl;
        private NHANVIEN _nhanvien;
        public List<KHENTHUONG_KYLUAT_DTO> _lstKL;
        private void FrmKyLuat_Load(object sender, EventArgs e)
        {
            _ktkl = new KHENTHUONG_KYLUAT();
            _nhanvien = new NHANVIEN();
            _them = false;
            showHide(true);
            LoadData();
            loadNhanVien();
            splitContainer1.Panel1Collapsed = true;
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
            gcDsKl.Enabled = kt;
            txtLyDo.Enabled = !kt;
            txtNoiDung.Enabled = !kt;
            txtSoQD.Enabled = !kt;
            dtNgay.Enabled = !kt;
            dtTuNgay.Enabled = !kt;
            dtDenNgay.Enabled = !kt;
            searchMANV.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoQD.Text = string.Empty;
            txtLyDo.Text = string.Empty;
            txtNoiDung.Text = string.Empty;
            dtNgay.Value = DateTime.Now;
            dtTuNgay.Value = DateTime.Now;
            dtDenNgay.Value = dtTuNgay.Value.AddDays(7);
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
            gcDsKl.DataSource = _ktkl.getListFull(2);
            FormManager_Functions.CustomView_Colums(gvDsKl);
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
            gcDsKl.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _ktkl.Delete(_SOQD);
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
            _lstKL = _ktkl.getItem_FULL(2, _SOQD);
            rptKyLuat rpt = new rptKyLuat(_lstKL);
            rpt.ShowRibbonPreview();
        }

        private void gvDsKl_Click(object sender, EventArgs e)
        {
            _SOQD = gvDsKl.GetFocusedRowCellValue("SOQUYETDINH").ToString();
            var kl = _ktkl.getItem(_SOQD);

            txtSoQD.Text = _SOQD;
            dtNgay.Value = kl.NGAY.Value;
            dtTuNgay.Value = kl.TUNGAY.Value;
            dtDenNgay.Value = kl.DENNGAY.Value;
            searchMANV.EditValue = kl.MANV;
            txtLyDo.Text = kl.LYDO;
            txtNoiDung.Text = kl.NOIDUNG;

            _lstKL = _ktkl.getItem_FULL(2, _SOQD);
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
                    {
                        MessageBox.Show("Vui lòng chọn điền nội dung.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtLyDo.Text))
                    {
                        MessageBox.Show("Vui lòng chọn điền lý do kỷ luật.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var maxSoQD = _ktkl.MaxSoQuyetDinh(2);
                    int so = int.Parse(maxSoQD.Substring(0, 5)) + 1;

                    TB_KHENTHUONG_KYLUAT kl = new TB_KHENTHUONG_KYLUAT();
                    kl.SOQUYETDINH = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/QĐKL";
                    kl.TUNGAY = dtTuNgay.Value;
                    kl.DENNGAY = dtDenNgay.Value;
                    kl.NGAY = dtNgay.Value;
                    kl.LYDO = txtLyDo.Text;
                    kl.LOAI = 2;
                    kl.NOIDUNG = txtNoiDung.Text;
                    kl.MANV = int.Parse(searchMANV.EditValue.ToString());
                    kl.CREATED_BY = 1;
                    kl.CREATED_DATE = DateTime.Now;
                    _ktkl.Add(kl);
                }
                else
                {
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var kl = _ktkl.getItem(_SOQD);
                    if (kl == null)
                    {
                        MessageBox.Show("Không tìm thấy hợp đồng với số hợp đồng: " + _SOQD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    kl.NGAY = dtNgay.Value;
                    kl.LYDO = txtLyDo.Text;
                    kl.NOIDUNG = txtNoiDung.Text;
                    kl.TUNGAY = dtTuNgay.Value;
                    kl.DENNGAY = dtDenNgay.Value;
                    kl.MANV = int.Parse(searchMANV.EditValue.ToString());
                    kl.CREATED_BY = 1;
                    kl.CREATED_DATE = DateTime.Now;
                    _ktkl.Update(kl);
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