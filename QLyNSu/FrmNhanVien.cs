using Bu;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu
{
    public partial class FrmNhanVien : DevExpress.XtraEditors.XtraForm
    {
        NHANVIEN _nhanvien;
        bool _them;
        int _MANV;

        //Load data

        DANTOC _dantoc;
        TONGIAO _tongiao;
        CHUCVU _chucvu;
        PHONGBAN _phongban;
        BOPHAN _bophan;
        TRINHDO _trinhdo;
        //TB_GIOITINH _gioitinh;

        public FrmNhanVien()
        {
            InitializeComponent();
        }

        void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnThem.Enabled = kt;
            btnXoa.Enabled = kt;
            btnSua.Enabled = kt;
            btnIn.Enabled = kt;
            btnDong.Enabled = kt;
            txtHoTen.Enabled = !kt;
            txtCCCD.Enabled = !kt;
            txtDiaChi.Enabled = !kt;
            txtSDT.Enabled = !kt;

            cboBoPhan.Enabled = !kt;
            cboGioiTinh.Enabled = !kt;
            cboPhongBan.Enabled = !kt;
            cboTonGiao.Enabled = !kt;
            cboTrinhDo.Enabled = !kt;
            cboChucVu.Enabled = !kt;
            cboDanToc.Enabled = !kt;
            btnImage.Enabled = !kt;
            dateNgaySinh.Enabled = !kt;
        }

        void _reset()
        {
            txtHoTen.Text = string.Empty;
            txtCCCD.Text = string.Empty;
            txtSDT.Text = string.Empty;
            txtDiaChi.Text = string.Empty;
        }

        void LoadData()
        {
            gcDsNV.DataSource = _nhanvien.getList();
            gvDsNV.OptionsBehavior.Editable = false;
        }

        void SaveData()
        {
            try
            {
                if (_them)
                {
                    TB_NHANVIEN nv = new TB_NHANVIEN();
                    nv.HOTEN = txtHoTen.Text;
                    nv.GIOITINH = int.Parse(cboGioiTinh.SelectedValue.ToString());
                    nv.NGAYSINH = dateNgaySinh.Value;
                    nv.DIENTHOAI = txtSDT.Text;
                    nv.CCCD = txtCCCD.Text;
                    nv.DIACHI = txtDiaChi.Text;
                    nv.HINHANH = ImageToBase64(pictureHinhanh.Image, pictureHinhanh.Image.RawFormat);
                    nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
                    nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
                    nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
                    nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
                    nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
                    nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
                    nv.IDCTY = 1;
                    _nhanvien.Add(nv);
                }
                else
                {
                    var nv = _nhanvien.getItem(_MANV);
                    if (nv != null)
                    {
                        nv.HOTEN = txtHoTen.Text;
                        nv.GIOITINH = int.Parse(cboGioiTinh.SelectedValue.ToString());
                        nv.NGAYSINH = dateNgaySinh.Value;
                        nv.DIENTHOAI = txtSDT.Text;
                        nv.CCCD = txtCCCD.Text;
                        nv.DIACHI = txtDiaChi.Text;
                        nv.HINHANH = ImageToBase64(pictureHinhanh.Image, pictureHinhanh.Image.RawFormat);
                        nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
                        nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
                        nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
                        nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
                        nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
                        nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
                        nv.IDCTY = 1;
                        _nhanvien.Update(nv);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đối tượng với ID: " + _MANV);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gcDsNV_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

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
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Kiểm tra xem _IDTG có giá trị hợp lệ không
            if (_MANV <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _nhanvien.Delete(_MANV);
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

        private void FrmNhanVien_Load(object sender, EventArgs e)
        {
            _them = false;
            _nhanvien = new NHANVIEN();
            _dantoc = new DANTOC();
            _trinhdo = new TRINHDO();
            _chucvu = new CHUCVU();
            _tongiao = new TONGIAO();
            _phongban = new PHONGBAN();
            _bophan = new BOPHAN();
            //_gioitinh = new TB_GIOITINH();
            showHide(true);
            LoadData();
            LoadCombo();
            splitContainer1.Panel1Collapsed = true;
        }

        private void gvDsNV_Click(object sender, EventArgs e)
        {
            if (gvDsNV.RowCount > 0)
            {
                _MANV = int.Parse(gvDsNV.GetFocusedRowCellValue("MANV").ToString());
                var nv = _nhanvien.getItem(_MANV);
                txtHoTen.Text = nv.HOTEN;
                cboGioiTinh.SelectedValue = nv.GIOITINH;
                dateNgaySinh.Value = nv.NGAYSINH.Value;
                txtSDT.Text = nv.DIENTHOAI;
                txtCCCD.Text = nv.CCCD;
                txtDiaChi.Text = nv.DIACHI;
                pictureHinhanh.Image = Base64ToImage(nv.HINHANH);
                cboBoPhan.SelectedValue = nv.IDBP;
                cboPhongBan.SelectedValue = nv.IDPB;
                cboTrinhDo.SelectedValue = nv.IDTD;
                cboChucVu.SelectedValue = nv.IDCV;
                cboDanToc.SelectedValue = nv.IDDT;
                cboTonGiao.SelectedValue = nv.IDTG;
                //cboCongTy.SelectedValue = nv.IDCTY;
            }
        }

        public byte[] ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] imageBytes  = ms.ToArray();
                return imageBytes;
            }    
        }

        public Image Base64ToImage(byte[] imageBytes) 
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        void LoadCombo()
        {
            cboBoPhan.DataSource = _bophan.getList();
            cboBoPhan.DisplayMember = "TENBP";
            cboBoPhan.ValueMember = "IDBP";

            cboPhongBan.DataSource = _phongban.getList();
            cboPhongBan.DisplayMember = "TENPB";
            cboPhongBan.ValueMember = "IDPB";

            cboTrinhDo.DataSource = _trinhdo.getList();
            cboTrinhDo.DisplayMember = "TENTD";
            cboTrinhDo.ValueMember = "IDTD";

            cboChucVu.DataSource = _chucvu.getList();
            cboChucVu.DisplayMember = "TENCV";
            cboChucVu.ValueMember = "IDCV";

            cboDanToc.DataSource = _dantoc.getList();
            cboDanToc.DisplayMember = "TENDT";
            cboDanToc.ValueMember = "IDDT";

            cboTonGiao.DataSource = _tongiao.getList();
            cboTonGiao.DisplayMember = "TENTG";
            cboTonGiao.ValueMember = "IDTG";

            //cboGioiTinh.DataSource = _gioitinh.getList();
            //cboGioiTinh.DisplayMember = "TENGT";
            //cboGioiTinh.ValueMember = "GIOITINH";
        }
    }
}