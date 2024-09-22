using Bu;
using Bu.CLASS_CHAMCONG;
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

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmCapNhatNgayCong : DevExpress.XtraEditors.XtraForm
    {
        public FrmCapNhatNgayCong()
        {
            InitializeComponent();
        }

        private KYCONGCHITIET _kcct;

        public int _manv;
        public string _hoten;
        public int _MAKYCONG;
        public string _ngay;
        public int _cngay;
        FrmBangCong_ChiTiet frmBCCC = (FrmBangCong_ChiTiet)Application.OpenForms["FrmBangCong_ChiTiet"];  
        private void FrmCapNhatNgayCong_Load(object sender, EventArgs e)
        {
            _kcct = new KYCONGCHITIET();
            lblMANV.Text = _manv.ToString();
            lblHoTen.Text = _hoten;

            string nam = _MAKYCONG.ToString().Substring(0, 4);
            string thang = _MAKYCONG.ToString().Substring(4);
            string ngay = _ngay.ToString().Substring(1);
            DateTime _d = DateTime.Parse(nam + "/" + thang + "/" + ngay);
            cldNgayCong.SetDate(_d);
            lblNgay.Text = _d.ToString("dd/MM/yyyy");
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            string _valueChamCong = radioChamCong.Properties.Items[radioChamCong.SelectedIndex].Value.ToString();
            string _valueNgayNghi = radioNgayNghi.Properties.Items[radioNgayNghi.SelectedIndex].Value.ToString();
            string fieldName = "D" + _cngay.ToString();
            var kcct = _kcct.getItem(_MAKYCONG, _manv);
            double _tongngayphep = kcct.NGAYPHEP.HasValue ? Convert.ToDouble(kcct.NGAYPHEP.Value) : 0.0;
            double _tongngayle = kcct.CONGNGAYLE.HasValue ? Convert.ToDouble(kcct.CONGNGAYLE.Value) : 0.0;
            double _tongngaykhongphep = kcct.NGHIKHONGPHEP.HasValue ? Convert.ToDouble(kcct.NGHIKHONGPHEP.Value) : 0.0;
            double _tongngaycong = kcct.NGAYCONG.HasValue ? Convert.ToDouble(kcct.NGAYCONG.Value) : 0.0;
            

            if (cldNgayCong.SelectionRange.Start.Year * 100 + cldNgayCong.SelectionRange.Start.Month != _MAKYCONG)
            {
                MessageBox.Show("Vui lòng chọn đúng ngày cong tháng hiện tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Cập nhật KYCONGCHITIET => BANGCONG_NV_CHITIET
            GetData_Functions.execQuery("Update TB_KYCONGCHITIET SET " + fieldName+ "='"+_valueChamCong +"' WHERE MAKYCONG="+_MAKYCONG+" AND MANV="+ _manv);
            frmBCCC.loadBangCong();
   
            //MessageBox.Show(_valueChamCong + " -- " + _valueNgayNghi);
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cldNgayCong_DateSelected(object sender, DateRangeEventArgs e)
        {
            _cngay = cldNgayCong.SelectionRange.Start.Day;
            //MessageBox.Show(cldNgayCong.SelectionRange.Start.Day.ToString());
        }
    }
}