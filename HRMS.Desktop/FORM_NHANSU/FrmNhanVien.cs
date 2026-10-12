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
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_DM_NHANVIEN", btnThem, btnSua, btnXoa, btnIn, kt);
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
            cboLoaiNV.Enabled = !kt;
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
            cboLoaiNV.SelectedValue = 1;
        }

        private async Task LoadData()
        {
            try
            {
            var employees = await Functions.FormDataLoader.ReadAsync(this, () => _nhanvien.getListFll_DTO());
            gcDsNV.DataSource = employees;
            gvDsNV.OptionsBehavior.Editable = false;
            _lstNVDTO = employees;
            FormManager_Functions.CustomView_Colums(gvDsNV);
        
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
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

            var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
            if (!Functions.FormSecurity.AssertPermission("F_DM_NHANVIEN", permAction)) return;

            try
            {
                byte[] imageBytes = null;
                if (pictureHinhanh.Image != null)
                {
                    imageBytes = ImageToBase64(pictureHinhanh.Image, pictureHinhanh.Image.RawFormat);
                }

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
                    nv.HINHANH = imageBytes;
                    nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
                    nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
                    nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
                    nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
                    nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
                    nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
                    nv.IDQT = int.Parse(cboQuocTich.SelectedValue.ToString());
                    nv.IDCTY = int.Parse(cboCongTy.SelectedValue.ToString());
                    nv.LOAI_NV = int.Parse(cboLoaiNV.SelectedValue.ToString());
                    nv.CREATED_BY = 1;
                    nv.CREATED_DATE = DateTime.Now;
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
                        nv.HINHANH = imageBytes;
                        nv.IDPB = int.Parse(cboPhongBan.SelectedValue.ToString());
                        nv.IDBP = int.Parse(cboBoPhan.SelectedValue.ToString());
                        nv.IDCV = int.Parse(cboChucVu.SelectedValue.ToString());
                        nv.IDTD = int.Parse(cboTrinhDo.SelectedValue.ToString());
                        nv.IDDT = int.Parse(cboDanToc.SelectedValue.ToString());
                        nv.IDTG = int.Parse(cboTonGiao.SelectedValue.ToString());
                        nv.IDQT = int.Parse(cboQuocTich.SelectedValue.ToString());
                        nv.IDCTY = int.Parse(cboCongTy.SelectedValue.ToString());
                        nv.LOAI_NV = int.Parse(cboLoaiNV.SelectedValue.ToString());
                        nv.UPDATED_BY = 1;
                        nv.UPDATED_DATE = DateTime.Now;
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

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_NHANVIEN", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_NHANVIEN", Bu.DTO.PermissionAction.Edit)) return;
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDsNV.Enabled = true;
        }

        private async void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_DM_NHANVIEN", Bu.DTO.PermissionAction.Delete)) return;
            splitContainer1.Panel1Collapsed = false;
            //Kiểm tra xem _IDTG có giá trị hợp lệ không
            if (_MANV <= 0)
            {
                MessageBox.Show("Vui lòng chọn một hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _nhanvien.Delete(_MANV,1);
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
            splitContainer1.Panel1Collapsed = true;
        }

        private async void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
            await LoadData();
            if (IsDisposed || Disposing) return;
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
            if (!Functions.FormSecurity.AssertPermission("F_DM_NHANVIEN", Bu.DTO.PermissionAction.Print)) return;
            rptDSNhanVien rpt = new rptDSNhanVien(_lstNVDTO);
            rpt.ShowRibbonPreview();
        }

        private async void FrmNhanVien_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_DM_NHANVIEN")) return;
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
            await LoadData();
            if (IsDisposed || Disposing) return;
            await LoadCombo();
            if (IsDisposed || Disposing) return;
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
                cboGioiTinh.SelectedValue = (int)(nv?.IDGT ?? -1);
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

                cboBoPhan.SelectedValue = (int)(nv?.IDBP ?? -1);
                cboPhongBan.SelectedValue = nv?.IDPB ?? -1;
                cboTrinhDo.SelectedValue = (int)(nv?.IDTD ?? -1);
                cboChucVu.SelectedValue = nv?.IDCV ?? -1;
                cboDanToc.SelectedValue = (int)(nv?.IDDT ?? -1);
                cboTonGiao.SelectedValue = (int)(nv?.IDTG ?? -1);
                cboQuocTich.SelectedValue = (int)(nv?.IDQT ?? -1);
                cboCongTy.SelectedValue = nv?.IDCTY ?? -1;
                cboLoaiNV.SelectedValue = (int)(nv?.LOAI_NV ?? 1);
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

        private async Task LoadCombo()
        {
            try
            {
            var language = Functions.TranslationManager.GetCurrentLanguageCode();
            var lookups = await Functions.FormDataLoader.ReadAsync(this, () => new {
                Item0 = _bophan.getListDTO(language),
                Item1 = _phongban.getListDTO(language),
                Item2 = _trinhdo.getListDTO(language),
                Item3 = _chucvu.getListDTO(language),
                Item4 = _dantoc.getListDTO(language),
                Item5 = _tongiao.getListDTO(language),
                Item6 = _gioitinh.getListDTO(language),
                Item7 = _quoctich.getListDTO(language),
                Item8 = _congty.getList(),
            });

            cboBoPhan.DataSource = lookups.Item0;
            cboBoPhan.DisplayMember = "TEN";
            cboBoPhan.ValueMember = "ID";

            cboPhongBan.DataSource = lookups.Item1;
            cboPhongBan.DisplayMember = "TENPB";
            cboPhongBan.ValueMember = "IDPB";

            cboTrinhDo.DataSource = lookups.Item2;
            cboTrinhDo.DisplayMember = "TEN";
            cboTrinhDo.ValueMember = "ID";

            cboChucVu.DataSource = lookups.Item3;
            cboChucVu.DisplayMember = "TENCV";
            cboChucVu.ValueMember = "IDCV";

            cboDanToc.DataSource = lookups.Item4;
            cboDanToc.DisplayMember = "TEN";
            cboDanToc.ValueMember = "ID";

            cboTonGiao.DataSource = lookups.Item5;
            cboTonGiao.DisplayMember = "TEN";
            cboTonGiao.ValueMember = "ID";

            cboGioiTinh.DataSource = lookups.Item6;
            cboGioiTinh.DisplayMember = "TEN";
            cboGioiTinh.ValueMember = "ID";

            cboQuocTich.DataSource = lookups.Item7;
            cboQuocTich.DisplayMember = "TEN";
            cboQuocTich.ValueMember = "ID";

            cboCongTy.DataSource = lookups.Item8;
            cboCongTy.DisplayMember = "TENCTY";
            cboCongTy.ValueMember = "IDCTY";

            var loaiNvList = new List<object>
            {
                new { Value = 1, Name = QLyNSu.Functions.TranslationManager.Translate("Nhân viên (Office)") },
                new { Value = 2, Name = QLyNSu.Functions.TranslationManager.Translate("Lái xe (Driver)") },
                new { Value = 3, Name = QLyNSu.Functions.TranslationManager.Translate("Công nhân (Worker)") }
            };
            cboLoaiNV.DataSource = loaiNvList;
            cboLoaiNV.DisplayMember = "Name";
            cboLoaiNV.ValueMember = "Value";
        
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
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

        private void gvDsNV_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
    }
}