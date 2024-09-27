using Bu;
using Bu.DTO;
using DA;
using DevExpress.XtraEditors;
using QLyNSu.Reports;
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
using DevExpress.XtraReports.UI;

namespace QLyNSu
{
    public partial class FrmNhanVien : DevExpress.XtraEditors.XtraForm
    {
        public FrmNhanVien()
        {
            InitializeComponent();
        }

        private NHANVIEN _nhanvien;
        bool _them;
        int _MANV;

        //Load data

        private DANTOC _dantoc;
        private TONGIAO _tongiao;
        private CHUCVU _chucvu;
        private PHONGBAN _phongban;
        private BOPHAN _bophan;
        private TRINHDO _trinhdo;
        private GIOITINH _gioitinh;
        private QUOCTICH _quoctich;
        private CONGTY _congty;

        //private Image _hinh;

        List<NHANVIEN_DTO> _lstNVDTO;

        private void showHide(bool kt)
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
            cboQuocTich.Enabled = !kt;
            btnImage.Enabled = !kt;
            dateNgaySinh.Enabled = !kt;
        }

        private void _reset()
        {
            txtHoTen.Text = string.Empty;
            txtCCCD.Text = string.Empty;
            txtSDT.Text = string.Empty;
            txtDiaChi.Text = string.Empty;

            cboBoPhan.Text = "Vui lòng chọn bộ phận";
            cboGioiTinh.Text = "Vui lòng chọn giới tính";
            cboPhongBan.Text = "Vui lòng chọn phòng ban";
            cboTonGiao.Text = "Vui lòng chọn tôn giáo";
            cboTrinhDo.Text = "Vui lòng chọn trình độ";
            cboChucVu.Text = "Vui lòng chọn chức vụ";
            cboDanToc.Text = "Vui lòng chọn dân tộc";
            cboQuocTich.Text = "Vui lòng chọn quốc tịch";
        }

        private void LoadData()
        {
            gcDsNV.DataSource = _nhanvien.getListFll_DTO();
            gvDsNV.OptionsBehavior.Editable = false;
            _lstNVDTO = _nhanvien.getListFll_DTO();
            FormManager_Functions.CustomView_Colums(gvDsNV);
        }

       private void SaveData()
        {
            #region Comments
            //try
            //{
            //    if (_them)
            //    {
            //        TB_NHANVIEN nv = new TB_NHANVIEN();
            //        nv.HOTEN = txtHoTen.Text;
            //        nv.IDGT = int.Parse(cboGioiTinh.SelectedValue.ToString());
            //        nv.NGAYSINH = dateNgaySinh.Value;
            //        nv.DIENTHOAI = txtSDT.Text;
            //        nv.CCCD = txtCCCD.Text;
            //        nv.DIACHI = txtDiaChi.Text;
            //        nv.HINHANH = ImageToBase64(pictureHinhanh.Image, pictureHinhanh.Image.RawFormat);
            //        nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
            //        nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
            //        nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
            //        nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
            //        nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
            //        nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
            //        nv.IDQT = int.Parse(cboQuocTich.SelectedValue.ToString());
            //        nv.IDCTY = int.Parse(cboCongTy.SelectedValue.ToString());
            //        _nhanvien.Add(nv);
            //    }
            //    else
            //    {
            //        var nv = _nhanvien.getItem(_MANV);
            //        if (nv != null)
            //        {
            //            nv.HOTEN = txtHoTen.Text;
            //            nv.IDGT = int.Parse(cboGioiTinh.SelectedValue.ToString());
            //            nv.NGAYSINH = dateNgaySinh.Value;
            //            nv.DIENTHOAI = txtSDT.Text;
            //            nv.CCCD = txtCCCD.Text;
            //            nv.DIACHI = txtDiaChi.Text;
            //            nv.HINHANH = ImageToBase64(pictureHinhanh.Image, pictureHinhanh.Image.RawFormat);
            //            nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
            //            nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
            //            nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
            //            nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
            //            nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
            //            nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
            //            nv.IDQT = int.Parse(cboQuocTich.SelectedValue.ToString());
            //            nv.IDCTY = int.Parse(cboCongTy.SelectedValue.ToString());
            //            _nhanvien.Update(nv);
            //        }
            //        else
            //        {
            //            throw new Exception("Không tìm thấy đối tượng với ID: " + _MANV);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
            //    MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            #endregion
            try
            {
                // Kiểm tra hình ảnh có null không, nếu null thì gán hình ảnh mặc định
                Image imageToSave = pictureHinhanh.Image ?? Image.FromFile(@"image\NhanVien\noimage.png");

                // Kiểm tra các trường thông tin cần thiết
                if (string.IsNullOrWhiteSpace(txtHoTen.Text) ||
                    string.IsNullOrWhiteSpace(txtCCCD.Text) ||
                    string.IsNullOrWhiteSpace(txtSDT.Text) ||
                    string.IsNullOrWhiteSpace(txtDiaChi.Text) ||
                    cboGioiTinh.SelectedValue == null ||
                    cboPhongBan.SelectedValue == null ||
                    cboBoPhan.SelectedValue == null ||
                    cboChucVu.SelectedValue == null ||
                    cboTrinhDo.SelectedValue == null ||
                    cboDanToc.SelectedValue == null ||
                    cboTonGiao.SelectedValue == null ||
                    cboQuocTich.SelectedValue == null ||
                    cboCongTy.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin trước khi lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_them)
                {
                    TB_NHANVIEN nv = new TB_NHANVIEN();
                    nv.HOTEN = txtHoTen.Text;
                    nv.IDGT = int.Parse(cboGioiTinh.SelectedValue.ToString());
                    nv.NGAYSINH = dateNgaySinh.Value;
                    nv.DIENTHOAI = txtSDT.Text;
                    nv.CCCD = txtCCCD.Text;
                    nv.DIACHI = txtDiaChi.Text;
                    nv.HINHANH = ImageToBase64(imageToSave, imageToSave.RawFormat);
                    nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
                    nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
                    nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
                    nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
                    nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
                    nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
                    nv.IDQT = int.Parse(cboQuocTich.SelectedValue.ToString());
                    nv.IDCTY = int.Parse(cboCongTy.SelectedValue.ToString());
                    _nhanvien.Add(nv);
                }
                else
                {
                    var nv = _nhanvien.getItem(_MANV);
                    if (nv != null)
                    {
                        nv.HOTEN = txtHoTen.Text;
                        nv.IDGT = int.Parse(cboGioiTinh.SelectedValue.ToString());
                        nv.NGAYSINH = dateNgaySinh.Value;
                        nv.DIENTHOAI = txtSDT.Text;
                        nv.CCCD = txtCCCD.Text;
                        nv.DIACHI = txtDiaChi.Text;
                        nv.HINHANH = ImageToBase64(imageToSave, imageToSave.RawFormat);
                        nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
                        nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
                        nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
                        nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
                        nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
                        nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
                        nv.IDQT = int.Parse(cboQuocTich.SelectedValue.ToString());
                        nv.IDCTY = int.Parse(cboCongTy.SelectedValue.ToString());
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
            gcDsNV.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
            //Kiểm tra xem _IDTG có giá trị hợp lệ không
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
            rptDSNhanVien rpt = new rptDSNhanVien(_lstNVDTO);
            rpt.ShowRibbonPreview();
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
            _gioitinh = new GIOITINH();
            _quoctich = new QUOCTICH();
            _congty = new CONGTY();
            showHide(true);
            LoadData();
            LoadCombo();
            splitContainer1.Panel1Collapsed = true;
        }

        private void gvDsNV_Click(object sender, EventArgs e)
        {
            //if (gvDsNV.RowCount > 0)
            //{
            //    _MANV = int.Parse(gvDsNV.GetFocusedRowCellValue("MANV").ToString());
            //    var nv = _nhanvien.getItem(_MANV);
            //    txtHoTen.Text = nv.HOTEN;
            //    cboGioiTinh.SelectedValue = nv.IDGT;
            //    dateNgaySinh.Value = nv.NGAYSINH.Value;
            //    txtSDT.Text = nv.DIENTHOAI;
            //    txtCCCD.Text = nv.CCCD;
            //    txtDiaChi.Text = nv.DIACHI;
            //    pictureHinhanh.Image = Base64ToImage(nv.HINHANH);
            //    cboBoPhan.SelectedValue = nv.IDBP;
            //    cboPhongBan.SelectedValue = nv.IDPB;
            //    cboTrinhDo.SelectedValue = nv.IDTD;
            //    cboChucVu.SelectedValue = nv.IDCV;
            //    cboDanToc.SelectedValue = nv.IDDT;
            //    cboTonGiao.SelectedValue = nv.IDTG;
            //    cboQuocTich.SelectedValue = nv.IDQT;
            //    cboCongTy.SelectedValue = nv.IDCTY;
            //}
            if (gvDsNV.RowCount > 0)
            {
                _MANV = int.Parse(gvDsNV.GetFocusedRowCellValue("MANV").ToString());

                var nv = _nhanvien.getItem(_MANV);

                txtHoTen.Text = string.IsNullOrEmpty(nv?.HOTEN) ? "null" : nv.HOTEN;
                cboGioiTinh.SelectedValue = nv?.IDGT ?? -1;
                dateNgaySinh.Value = nv?.NGAYSINH ?? DateTime.Now;
                txtSDT.Text = string.IsNullOrEmpty(nv?.DIENTHOAI) ? "null" : nv.DIENTHOAI;
                txtCCCD.Text = string.IsNullOrEmpty(nv?.CCCD) ? "null" : nv.CCCD;
                txtDiaChi.Text = string.IsNullOrEmpty(nv?.DIACHI) ? "null" : nv.DIACHI;

                if (nv?.HINHANH != null && nv.HINHANH.Length > 0)
                {
                    pictureHinhanh.Image = Base64ToImage(nv.HINHANH);
                }
                else
                {
                    pictureHinhanh.Image = null; // Hoặc thiết lập hình ảnh mặc định
                }

                cboBoPhan.SelectedValue = nv?.IDBP ?? -1;
                cboPhongBan.SelectedValue = nv?.IDPB ?? -1;
                cboTrinhDo.SelectedValue = nv?.IDTD ?? -1;
                cboChucVu.SelectedValue = nv?.IDCV ?? -1;
                cboDanToc.SelectedValue = nv?.IDDT ?? -1;
                cboTonGiao.SelectedValue = nv?.IDTG ?? -1;
                cboQuocTich.SelectedValue = nv?.IDQT ?? -1;
                cboCongTy.SelectedValue = nv?.IDCTY ?? -1;
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
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                return image;
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            
        }

        private void LoadCombo()
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

            cboGioiTinh.DataSource = _gioitinh.getList();
            cboGioiTinh.DisplayMember = "TENGT";
            cboGioiTinh.ValueMember = "IDGT";

            cboQuocTich.DataSource = _quoctich.getList();
            cboQuocTich.DisplayMember = "TENQT";
            cboQuocTich.ValueMember = "IDQT";

            cboCongTy.DataSource = _congty.getList();
            cboCongTy.DisplayMember = "TENCTY";
            cboCongTy.ValueMember = "IDCTY";
        }

        private void btnImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Picture file (jpg, png)|*.png;*.jpg";
            openFile.Title = "Chọn 1 hình ảnh";
            if(openFile.ShowDialog() == DialogResult.OK)
            {
                pictureHinhanh.Image = Image.FromFile(openFile.FileName);
                pictureHinhanh.SizeMode = PictureBoxSizeMode.StretchImage;
            }    

        }
    }
}