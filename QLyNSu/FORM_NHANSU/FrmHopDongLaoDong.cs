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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QLyNSu
{
    public partial class FrmHopDongLaoDong : DevExpress.XtraEditors.XtraForm
    {
        public FrmHopDongLaoDong()
        {
            InitializeComponent();
        }

        private HOPDONGLAODONG _hdld;
        private NHANVIEN _nhanvien;
        public List<HOPDONG_DTO> _lstHD;
        private bool _them;
        private string _SOHD;
        //private string _MaxSHD;
        private void FrmHopDongLaoDong_Load(object sender, EventArgs e)
        {
            _hdld = new HOPDONGLAODONG();
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
            gcDsHDLD.Enabled = kt;
            dtNgayKetThuc.Enabled = !kt;
            dtNgayBatDau.Enabled = !kt;
            dtNgayKy.Enabled = !kt;
            spHeSoLuong.Enabled = !kt;
            spLanKy.Enabled = !kt;
            txtSoHD.Enabled =! kt;
            searchMANV.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoHD.Text = string.Empty;
            dtNgayBatDau.Value = DateTime.Now;
            dtNgayKy.Value = DateTime.Now;
            spLanKy.Text = "1";
            spHeSoLuong.Text = "1";
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
            gcDsHDLD.DataSource = _hdld.getlistFull_DTO();
            gvDsHDLD.OptionsBehavior.Editable = false;
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
            gcDsHDLD.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Mày có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _hdld.Delete(_SOHD,1);
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
            _lstHD = _hdld.getItem_FULL(_SOHD);
            rptHopDongLaoDong rpt = new rptHopDongLaoDong(_lstHD);
            rpt.ShowRibbonPreview();
        }

        private void SaveData()
        {
            try
            {
                if (_them)
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(cbThoiHan.Text))
                    {
                        MessageBox.Show("Vui lòng chọn thời hạn hợp đồng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (spHeSoLuong.EditValue == null || !decimal.TryParse(spHeSoLuong.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng nhập hệ số lương hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (spLanKy.EditValue == null || !int.TryParse(spLanKy.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng nhập số lần ký hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchMANV.EditValue == null || !int.TryParse(searchMANV.EditValue.ToString(), out _))
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Kiểm tra ngày bắt đầu, ngày kết thúc và ngày ký
                    if (dtNgayBatDau.Value > dtNgayKetThuc.Value)
                    {
                        MessageBox.Show("Ngày bắt đầu không thể lớn hơn ngày kết thúc.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtNgayKy.Value < dtNgayBatDau.Value || dtNgayKy.Value > dtNgayKetThuc.Value)
                    {
                        MessageBox.Show("Ngày ký phải nằm trong khoảng thời gian hợp đồng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var maxSoHD = _hdld.MaxSoHopDong();
                    int so = int.Parse(maxSoHD.Substring(0, 5)) + 1;

                    TB_HOPDONG hd = new TB_HOPDONG();

                    hd.SOHD = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/HĐLĐ";
                    hd.NGAYBATDAU = dtNgayBatDau.Value;
                    hd.NGAYKETTHUC = dtNgayKetThuc.Value;
                    hd.NGAYKY = dtNgayKy.Value;
                    hd.THOIHAN = cbThoiHan.Text; 
                    hd.HESOLUONG = decimal.Parse(spHeSoLuong.EditValue.ToString());
                    hd.LANKY = int.Parse(spLanKy.EditValue.ToString());
                    hd.MANV = int.Parse(searchMANV.EditValue.ToString());
                    hd.NOIDUNG = txtNoiDung.RtfText;
                    hd.IDCTY = 1;
                    hd.CREATED_BY = 1;
                    hd.CREATED_DATE = DateTime.Now;
                    _hdld.Add(hd);
                }
                else
                {
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var hd = _hdld.getItem(_SOHD);
                    if (hd == null)
                    {
                        MessageBox.Show("Không tìm thấy hợp đồng với số hợp đồng: " + _SOHD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    hd.NGAYBATDAU = dtNgayBatDau.Value;
                    hd.NGAYKETTHUC = dtNgayKetThuc.Value;
                    hd.NGAYKY = dtNgayKy.Value;
                    hd.THOIHAN = cbThoiHan.Text;
                    hd.HESOLUONG = decimal.Parse(spHeSoLuong.EditValue.ToString());
                    hd.LANKY = int.Parse(spLanKy.EditValue.ToString());
                    hd.MANV = int.Parse(searchMANV.EditValue.ToString());
                    hd.NOIDUNG = txtNoiDung.RtfText;
                    hd.IDCTY = 1;
                    hd.CREATED_BY = 1;
                    hd.CREATED_DATE = DateTime.Now;
                    _hdld.Update(hd);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gvDsHDLD_Click(object sender, EventArgs e)
        {
            if (gvDsHDLD.RowCount > 0)
            {
                _SOHD = gvDsHDLD.GetFocusedRowCellValue("SOHD").ToString();
                var hd = _hdld.getItem(_SOHD);

                txtSoHD.Text = _SOHD;
                dtNgayBatDau.Value = hd.NGAYBATDAU.Value;
                dtNgayKetThuc.Value = hd.NGAYKETTHUC.Value ;
                dtNgayKy.Value = hd.NGAYKY.Value;
                cbThoiHan.Text = hd.THOIHAN;
                spHeSoLuong.Text= hd.HESOLUONG.ToString();
                spLanKy.Text = hd.LANKY.ToString();
                searchMANV.EditValue = hd.MANV;
                txtNoiDung.RtfText = hd.NOIDUNG;

                _lstHD = _hdld.getItem_FULL(_SOHD);
            }
        }

        private void cbThoiHan_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Lấy giá trị ngày bắt đầu từ DateTimePicker
            DateTime startDate = dtNgayBatDau.Value;

            // Cập nhật ngày kết thúc dựa trên lựa chọn trong ComboBox
            switch (cbThoiHan.SelectedItem.ToString())
            {
                case "3 Tháng":
                    dtNgayKetThuc.Value = startDate.AddMonths(3);
                    break;
                case "6 Tháng":
                    dtNgayKetThuc.Value = startDate.AddMonths(6);
                    break;
                case "9 Tháng":
                    dtNgayKetThuc.Value = startDate.AddMonths(9);
                    break;
                case "1 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(1);
                    break;
                case "2 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(2);
                    break;
                case "3 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(3);
                    break;
                case "4 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(4);
                    break;
                case "5 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(5);
                    break;
                case "6 Năm":
                    dtNgayKetThuc.Value = startDate.AddYears(6);
                    break;
                default:
                    MessageBox.Show("Lựa chọn không hợp lệ!");
                    break;
            }
        }
    }
}