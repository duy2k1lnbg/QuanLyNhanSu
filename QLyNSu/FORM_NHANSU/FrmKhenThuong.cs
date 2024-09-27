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
    public partial class FrmKhenThuong : DevExpress.XtraEditors.XtraForm
    {
        public FrmKhenThuong()
        {
            InitializeComponent();
        }

        private bool _them;
        private string _SOQD;
        private KHENTHUONG_KYLUAT _ktkl;
        private NHANVIEN _nhanvien;
        public List<KHENTHUONG_KYLUAT_DTO> _lstKT;
        private void FrmKhenThuong_Load(object sender, EventArgs e)
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
            gcDsKT.Enabled = kt;
            txtLyDo.Enabled = !kt;
            txtNoiDung.Enabled = !kt;
            txtSoQD.Enabled = !kt;
            dtNgay.Enabled = !kt;
            searchMANV.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoQD.Text = string.Empty;
            txtLyDo.Text = string.Empty;
            txtNoiDung.Text = string.Empty;
            dtNgay.Value = DateTime.Now;
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
            gcDsKT.DataSource = _ktkl.getListFull(1);
            FormManager_Functions.CustomView_Colums(gvDsKT);
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
            gcDsKT.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _ktkl.Delete(_SOQD, 1);
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
            _lstKT = _ktkl.getItem_FULL(1, _SOQD);
            rptKhenThuong rpt = new rptKhenThuong(_lstKT);
            rpt.ShowRibbonPreview();
        }

        private void gvDsKT_Click(object sender, EventArgs e)
        {
            if (gvDsKT.RowCount > 0)
            {
                _SOQD = gvDsKT.GetFocusedRowCellValue("SOQUYETDINH").ToString();
                var kt = _ktkl.getItem(_SOQD);

                txtSoQD.Text = _SOQD;
                dtNgay.Value = kt.NGAY.Value;
                searchMANV.EditValue = kt.MANV;
                txtLyDo.Text = kt.LYDO;
                txtNoiDung.Text = kt.NOIDUNG;
                _lstKT = _ktkl.getItem_FULL(1,_SOQD);
            }
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
                        MessageBox.Show("Vui lòng chọn điền lý do khen thưởng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var maxSoQD = _ktkl.MaxSoQuyetDinh(1);
                    int so = int.Parse(maxSoQD.Substring(0, 5)) + 1;

                    TB_KHENTHUONG_KYLUAT kt = new TB_KHENTHUONG_KYLUAT();
                    kt.SOQUYETDINH = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/QĐKT";
                    //kt.NGAYBATDAU = dtNgayBatDau.Value;
                    //kt.NGAYKETTHUC = dtNgayKetThuc.Value;
                    kt.NGAY = dtNgay.Value;
                    kt.LYDO = txtLyDo.Text;
                    kt.LOAI = 1;
                    kt.NOIDUNG = txtNoiDung.Text;
                    kt.MANV = int.Parse(searchMANV.EditValue.ToString());
                    kt.CREATED_BY = 1;
                    kt.CREATED_DATE = DateTime.Now;
                    _ktkl.Add(kt);
                }
                else
                {
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var kt = _ktkl.getItem(_SOQD);
                    if (kt == null)
                    {
                        MessageBox.Show("Không tìm thấy hợp đồng với số hợp đồng: " + _SOQD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    kt.NGAY = dtNgay.Value;
                    kt.LYDO = txtLyDo.Text;
                    kt.NOIDUNG = txtNoiDung.Text;
                    kt.MANV = int.Parse(searchMANV.EditValue.ToString());
                    kt.UPDATED_BY = 1;
                    kt.UPDATED_DATE = DateTime.Now;
                    _ktkl.Update(kt);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, " Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}