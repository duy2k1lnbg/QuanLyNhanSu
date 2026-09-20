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
    public partial class FrmNangLuong_NhanVien : DevExpress.XtraEditors.XtraForm
    {
        public FrmNangLuong_NhanVien()
        {
            InitializeComponent();
        }

        private bool _them;
        private string _SOQD;
        private NANGLUONG_NHANVIEN _nlnv;
        private NHANVIEN _nhanvien_nl;
        private HOPDONGLAODONG _hopdong;

        private void FrmNangLuong_NhanVien_Load(object sender, EventArgs e)
        {
            _nlnv = new NANGLUONG_NHANVIEN();
            _nhanvien_nl = new NHANVIEN();
            _hopdong = new HOPDONGLAODONG();
            _them = false;
            showHide(true);
            LoadData();
            loadHopDong();
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
            gcDsNL.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _nlnv.Delete(_SOQD, 1);
                var hd = _hopdong.getItem(searchHopDong.EditValue.ToString());
                hd.HESOLUONG = decimal.Parse(spHSL_now.EditValue.ToString()); ;
                _hopdong.Update(hd);
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

        private void gvDsNL_Click(object sender, EventArgs e)
        {
            _SOQD = gvDsNL.GetFocusedRowCellValue("SOQDNL").ToString();
            var nl = _nlnv.getItem(_SOQD);

            txtSoQD.Text = _SOQD;
            dtNgayKy.Value = nl.NGAYKYNL.Value;
            dtNgayLenLuong.Value = nl.NGAYLENLUONG.Value;
            searchHopDong.EditValue = nl.SOHD;
            txtGhiChuTV.Text = nl.GHICHUNL;
            spHSL_now.EditValue = nl.HESOLUONG_NOW;
            spHSL_new.EditValue = nl.HESOLUONG_NEW;
            txtNhanVien.Text = gvDsNL.GetFocusedRowCellValue("HOTEN").ToString();
            //_lstKL = _ktkl.getItem_FULL(2, _SOQD);
        }

        private void gvDsNL_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
            gcDsNL.Enabled = kt;
            txtNhanVien.Enabled = !kt;
            txtGhiChuTV.Enabled = !kt;
            txtSoQD.Enabled = !kt;
            dtNgayKy.Enabled = !kt;
            dtNgayLenLuong.Enabled = !kt;
            searchHopDong.Enabled = !kt;
        }

        private void _reset()
        {
            txtSoQD.Text = string.Empty;
            txtNhanVien.Text = string.Empty;
            txtGhiChuTV.Text = string.Empty;
            dtNgayKy.Value = DateTime.Now;
            dtNgayLenLuong.Value = dtNgayKy.Value.AddDays(45);
            searchHopDong.Properties.NullText = "Vui lòng chọn";
        }

        private void loadHopDong()
        {
            searchHopDong.Properties.DataSource = _hopdong.getlistFull_DTO();
            searchHopDong.Properties.ValueMember = "SOHD";
            searchHopDong.Properties.DisplayMember = "SOHD";
        }

        private void LoadData()
        {
            gcDsNL.DataSource = _nlnv.getListFull();
            FormManager_Functions.CustomView_Colums(gvDsNL);
        }

        private void SaveData()
        {
            TB_NANGLUONG_NHANVIEN nl;
            try
            {
                if (_them)
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(txtGhiChuTV.Text))
                    {
                        MessageBox.Show("Vui lòng điền ghi chú.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //Số hợp đồng: 00001/2024/HĐLĐ
                    var maxSoQD = _nlnv.MaxSoQuyetDinh();
                    int so = int.Parse(maxSoQD.Substring(0, 5)) + 1;

                    nl = new TB_NANGLUONG_NHANVIEN();
                    nl.SOQDNL = so.ToString("00000") + @"/" + DateTime.Now.Year.ToString() + @"/QĐNL";
                    nl.SOHD = searchHopDong.EditValue.ToString();
                    nl.NGAYKYNL = dtNgayKy.Value;
                    nl.NGAYLENLUONG = dtNgayLenLuong.Value;
                    nl.HESOLUONG_NOW = _hopdong.getItem(searchHopDong.EditValue.ToString()).HESOLUONG;
                    nl.HESOLUONG_NEW = decimal.Parse(spHSL_new.EditValue.ToString());
                    nl.GHICHUNL = txtGhiChuTV.Text;
                    nl.MANV = _hopdong.getItem(searchHopDong.EditValue.ToString()).MANV;
                    nl.CREATED_BY = 1;
                    nl.CREATED_DATE = DateTime.Now;
                    _nlnv.Add(nl);
                }
                else
                {
                    //Số hợp đồng: 00001/2024/HĐLĐ
                    nl = _nlnv.getItem(_SOQD);
                    if (nl == null)
                    {
                        MessageBox.Show("Không tìm thấy hợp đồng với số hợp đồng: " + _SOQD, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    nl.SOHD = searchHopDong.EditValue.ToString();
                    nl.NGAYKYNL = dtNgayKy.Value;
                    nl.NGAYLENLUONG = dtNgayLenLuong.Value;
                    nl.HESOLUONG_NOW = _hopdong.getItem(searchHopDong.EditValue.ToString()).HESOLUONG;
                    nl.HESOLUONG_NEW = decimal.Parse(spHSL_new.EditValue.ToString());
                    nl.GHICHUNL = txtGhiChuTV.Text;
                    nl.MANV = _hopdong.getItem(searchHopDong.EditValue.ToString()).MANV;
                    nl.UPDATED_BY = 1;
                    nl.UPDATED_DATE = DateTime.Now;
                    _nlnv.Update(nl);
                }
                var hd = _hopdong.getItem(searchHopDong.EditValue.ToString());
                hd.HESOLUONG = decimal.Parse(spHSL_new.EditValue.ToString()); ;
                _hopdong.Update(hd);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchHopDong_EditValueChanged(object sender, EventArgs e)
        {
            var hd = _hopdong.getItem_FULL(searchHopDong.EditValue.ToString());
            if (hd.Count !=  0)
            {
                txtNhanVien.Text = hd[0].MANV + " - " + hd[0].HOTEN;
                spHSL_now.EditValue = hd[0].HESOLUONG;
            }
        }

        private void spHSL_now_EditValueChanged(object sender, EventArgs e)
        {
            if (spHSL_now.EditValue != null)
            {
                decimal currentHSL = Convert.ToDecimal(spHSL_now.EditValue); // Chuyển đổi EditValue sang decimal
                spHSL_new.Value = currentHSL + 0.5m; // Thêm 0.2 và gán lại cho spHSL_new.Value
            }
        }
    }
}