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
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraSplashScreen;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmBangLuong : DevExpress.XtraEditors.XtraForm
    {
        private BANGLUONG _bangluong;
        private KYCONG _kycong;

        public FrmBangLuong()
        {
            InitializeComponent();
        }

        private void FrmBangLuong_Load(object sender, EventArgs e)
        {
            _bangluong = new BANGLUONG();
            _kycong = new KYCONG();
            LoadCombo();
            LoadData();
        }

        private void LoadCombo()
        {
            cboKyCong.DataSource = _kycong.getList();
            cboKyCong.DisplayMember = "MAKYCONG";
            cboKyCong.ValueMember = "MAKYCONG";
        }

        private void LoadData()
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                gcBangLuong.DataSource = _bangluong.getList(makycong);
                gvBangLuong.OptionsBehavior.Editable = false;
            }
        }

        private void btnTinhLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TinhLuong();
        }

        private void btnTinhLuongCong_Click(object sender, EventArgs e)
        {
            TinhLuong();
        }

        private async void TinhLuong()
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                try
                {
                    SplashScreenManager.ShowForm(typeof(FrmWaiting), true, true);
                    
                    await Task.Run(() =>
                    {
                        _bangluong.TinhLuongKyCong(makycong, 1);
                    });

                    LoadData();
                    SplashScreenManager.CloseForm();
                    XtraMessageBox.Show("Tính lương thành công cho kỳ công " + makycong + "!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    SplashScreenManager.CloseForm();
                    XtraMessageBox.Show("Lỗi khi tính lương: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gvBangLuong.RowCount > 0)
            {
                gcBangLuong.ShowPrintPreview();
            }
            else
            {
                XtraMessageBox.Show("Không có dữ liệu để in!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void cboKyCong_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                LoadData();
            }
        }
    }
}