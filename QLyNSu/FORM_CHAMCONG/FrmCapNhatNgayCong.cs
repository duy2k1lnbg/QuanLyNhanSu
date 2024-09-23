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
        private BANGCONG_NV_CHITIET _bcct_nv;

        public int _manv;
        public string _hoten;
        public int _MAKYCONG;
        public string _ngay;
        public int _cngay;
        public int thang_f1_bcct;
        public int nam_f_bcct1;

        FrmBangCong_ChiTiet frmBCCC = (FrmBangCong_ChiTiet)Application.OpenForms["FrmBangCong_ChiTiet"];  
        private void FrmCapNhatNgayCong_Load(object sender, EventArgs e)
        {
            _kcct = new KYCONGCHITIET();
            _bcct_nv = new BANGCONG_NV_CHITIET();
            lblMANV.Text = _manv.ToString();
            lblHoTen.Text = _hoten;
            lblKyCong.Text = _MAKYCONG.ToString();

            try
            {
                string nam = _MAKYCONG.ToString().Substring(0, 4);
                string thang = _MAKYCONG.ToString().Substring(4);
                string ngay = _ngay.ToString().Substring(1);
                DateTime _d = DateTime.Parse(nam + "/" + thang + "/" + ngay);
                cldNgayCong.SetDate(_d);
                lblNgay.Text = _d.ToString("dd/MM/yyyy");
                _cngay = int.Parse(ngay);
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Vui lòng chọn 1 ô để cập nhật.\nLỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //throw new Exception("Có lỗi xảy ra: " + ex.Message, ex);
                this.Close();
            }
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            string _valueChamCong = radioChamCong.Properties.Items[radioChamCong.SelectedIndex].Value.ToString();
            string _valueNgayNghi = radioNgayNghi.Properties.Items[radioNgayNghi.SelectedIndex].Value.ToString();
            string fieldName = "D" + _cngay.ToString();
            var kcct = _kcct.getItem(_MAKYCONG, _manv);
            //double _tongngayphep = kcct.NGAYPHEP.HasValue ? Convert.ToDouble(kcct.NGAYPHEP.Value) : 0.0;
            //double _tongngayle = kcct.CONGNGAYLE.HasValue ? Convert.ToDouble(kcct.CONGNGAYLE.Value) : 0.0;
            //double _tongngaykhongphep = kcct.NGHIKHONGPHEP.HasValue ? Convert.ToDouble(kcct.NGHIKHONGPHEP.Value) : 0.0;
            //double _tongngaycong = kcct.NGAYCONG.HasValue ? Convert.ToDouble(kcct.NGAYCONG.Value) : 0.0;

            if (cldNgayCong.SelectionRange.Start.Year * 100 + cldNgayCong.SelectionRange.Start.Month != _MAKYCONG)
            {
                MessageBox.Show("Vui lòng chọn đúng ngày công tháng hiện tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Cập nhật KYCONGCHITIET => BANGCONG_NV_CHITIET
            GetData_Functions.execQuery("Update TB_KYCONGCHITIET SET " + fieldName+ "='"+_valueChamCong +"' WHERE MAKYCONG="+_MAKYCONG+" AND MANV="+ _manv);

            TB_BANGCONG_CHITIET bcctnv = _bcct_nv.getItem(_MAKYCONG, _manv, cldNgayCong.SelectionStart.Day);
            //if (bcctnv == null)
            //{
            //    MessageBox.Show("Không tìm thấy dữ liệu cho ngày: " + cldNgayCong.SelectionStart.Day +
            //                    " với MAKYCONG: " + _MAKYCONG +
            //                    " và MANV: " + _manv,
            //                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            bcctnv.KYHIEU = _valueChamCong;
            switch (_valueChamCong)
            {
                case "P":
                    if (_valueNgayNghi == "NN")
                    {
                        bcctnv.NGAYPHEP = 1;
                        bcctnv.NGAYCONG = 1;
                    }
                    else
                    {
                        bcctnv.NGAYPHEP = (decimal)0.5;
                        bcctnv.NGAYCONG = (decimal)0.5;
                    }
                    break;
                case "CT":
                    if (_valueNgayNghi == "NN")
                    {
                        bcctnv.NGAYCONG = 1;
                    }
                    else
                    {
                        bcctnv.NGAYPHEP = (decimal)0.5;
                        bcctnv.NGAYCONG = (decimal)0.5;
                    }
                    break;
                case "V":
                    if (_valueNgayNghi == "NN")
                    {
                        bcctnv.NGAYCONG = 0;
                        bcctnv.NGAYPHEP = 0;
                    }
                    else
                    {
                        bcctnv.NGAYPHEP = (decimal)0.5;
                        bcctnv.NGAYCONG = (decimal)0.5;
                    }
                    break;
                case "VR":
                    if (_valueNgayNghi == "NN")
                    {
                        bcctnv.NGAYCONG = 0;
                        bcctnv.NGAYPHEP = 1;
                    }
                    else
                    {
                        bcctnv.NGAYPHEP = (decimal)0.5;
                        bcctnv.NGAYCONG = (decimal)0.5;
                    }
                    break;
                default:
                    break;
            }
            _bcct_nv.Update(bcctnv);

            //Tính lại các ngày công
            decimal tongngaycong = _bcct_nv.tongNgayCong(_MAKYCONG, _manv);
            decimal tongngayphep = _bcct_nv.tongNgayPhep(_MAKYCONG, _manv);

            kcct.NGAYPHEP = tongngayphep;
            kcct.TONGNGAYCONG = tongngaycong;
            _kcct.Update(kcct,1);
            frmBCCC.loadBangCong();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cldNgayCong_DateSelected(object sender, DateRangeEventArgs e)
        {
            _cngay = cldNgayCong.SelectionRange.Start.Day;
            //lblNgay.Text = cldNgayCong.SelectionRange.Start.ToString("dd/MM/yyyy");
            //lblNgay.Text = 
            //MessageBox.Show(cldNgayCong.SelectionRange.Start.Day.ToString());
        }
    }
}