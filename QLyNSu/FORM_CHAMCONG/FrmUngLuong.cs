using Bu.CLASS_CHAMCONG;
using Bu;
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
using DA;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmUngLuong : DevExpress.XtraEditors.XtraForm
    {
        public FrmUngLuong()
        {
            InitializeComponent();
        }

        private UNGLUONG _ungluong;
        private NHANVIEN _nhanvien;
        private bool _them;
        private int _id;

        private void FrmUngLuong_Load(object sender, EventArgs e)
        {
            if (!Functions.FormSecurity.CheckViewPermission(this, "F_CC_UNGLUONG")) return;

            _ungluong = new UNGLUONG();
            _nhanvien = new NHANVIEN();
            _them = false;
            showHide(true);
            LoadData();
            LoadNhanVien();
            splitContainer1.Panel1Collapsed = true;
        }

        private void gvDanhSach_Click(object sender, EventArgs e)
        {
            if (gvDanhSach.RowCount > 0)
            {
                _id = int.Parse(gvDanhSach.GetFocusedRowCellValue("IDUL").ToString());
                txtGhiChu.Text = gvDanhSach.GetFocusedRowCellValue("GHICHU") != null ? gvDanhSach.GetFocusedRowCellValue("GHICHU").ToString() : string.Empty;
                searchMANV.EditValue = gvDanhSach.GetFocusedRowCellValue("MANV") != null ? gvDanhSach.GetFocusedRowCellValue("MANV").ToString() : null;
                spSoTien.Text = gvDanhSach.GetFocusedRowCellValue("SOTIENUNG") != null ? gvDanhSach.GetFocusedRowCellValue("SOTIENUNG").ToString() : "0";
                
                var rowNgay = gvDanhSach.GetFocusedRowCellValue("NGAYUNG");
                if (rowNgay != null && DateTime.TryParse(rowNgay.ToString(), out var dtVal))
                {
                    dtNgayUng.Value = dtVal;
                }
                else
                {
                    var created = gvDanhSach.GetFocusedRowCellValue("CREATED_DATE");
                    if (created != null && DateTime.TryParse(created.ToString(), out var crVal))
                    {
                        dtNgayUng.Value = crVal;
                    }
                }
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
            if (!Functions.FormSecurity.AssertPermission("F_CC_UNGLUONG", Bu.DTO.PermissionAction.Add)) return;
            _them = true;
            showHide(false);
            _reset();
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_UNGLUONG", Bu.DTO.PermissionAction.Edit)) return;
            _them = false;
            showHide(false);
            splitContainer1.Panel1Collapsed = false;
            gcDanhSach.Enabled = true;
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!Functions.FormSecurity.AssertPermission("F_CC_UNGLUONG", Bu.DTO.PermissionAction.Delete)) return;
            splitContainer1.Panel1Collapsed = true;
            // Hiển thị hộp thoại xác nhận
            if (MessageBox.Show("Bạn có chắc là xoá nó đi không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Thực hiện xóa và tải lại dữ liệu
                _ungluong.Delete(_id, 1);
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
            if (!Functions.FormSecurity.AssertPermission("F_CC_UNGLUONG", Bu.DTO.PermissionAction.Print)) return;
        }

        private void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;
            Functions.FormSecurity.ApplyButtons("F_CC_UNGLUONG", btnThem, btnSua, btnXoa, btnIn, kt);
            txtGhiChu.Enabled = !kt;
            spSoTien.Enabled = !kt;
            searchMANV.Enabled = !kt;
            dtNgayUng.Enabled = !kt;
        }

        private void _reset()
        {
            txtGhiChu.Text = string.Empty;
            searchMANV.Properties.NullText = "Vui lòng chọn 1 nhân viên";
            spSoTien.EditValue = 0;
            searchMANV.EditValue = 0;
            dtNgayUng.Value = DateTime.Now;
        }

        private void LoadNhanVien()
        {
            searchMANV.Properties.DataSource = _nhanvien.getListFll_DTO();
            searchMANV.Properties.ValueMember = "MANV";
            searchMANV.Properties.DisplayMember = "HOTEN";
        }
        private void LoadData()
        {
            gcDanhSach.DataSource = _ungluong.getListFull();
            FormManager_Functions.CustomView_Colums(gvDanhSach);
        }

        private void SaveData()
        {
            try
            {
                var permAction = _them ? Bu.DTO.PermissionAction.Add : Bu.DTO.PermissionAction.Edit;
                if (!Functions.FormSecurity.AssertPermission("F_CC_UNGLUONG", permAction)) return;

                DateTime ngayChon = dtNgayUng.Value;
                if (_them)
                {
                    TB_UNGLUONG ul = new TB_UNGLUONG();
                    ul.SOTIENUNG = int.Parse(spSoTien.EditValue.ToString());
                    ul.MANV = int.Parse(searchMANV.EditValue.ToString());
                    ul.GHICHU = txtGhiChu.Text;

                    ul.NAM = ngayChon.Year;
                    ul.THANG = ngayChon.Month;
                    ul.NGAY = ngayChon.Day;
                    ul.CREATED_BY = 1;
                    ul.CREATED_DATE = ngayChon;
                    _ungluong.Add(ul);
                }
                else
                {
                    var ul = _ungluong.getItem(_id);
                    ul.SOTIENUNG = int.Parse(spSoTien.EditValue.ToString());
                    ul.MANV = int.Parse(searchMANV.EditValue.ToString());
                    ul.GHICHU = txtGhiChu.Text;

                    ul.NAM = ngayChon.Year;
                    ul.THANG = ngayChon.Month;
                    ul.NGAY = ngayChon.Day;
                    ul.UPDATED_BY = 1;
                    ul.UPDATED_DATE = DateTime.Now;
                    _ungluong.Update(ul);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}