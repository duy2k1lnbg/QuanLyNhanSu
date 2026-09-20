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

            try
            {
                int nam = 0;
                int thang = 0;
                int ngay = 0;

                // 1. Xác định năm và tháng an toàn (không dùng Substring để tránh lỗi khi _MAKYCONG = 0)
                if (_MAKYCONG >= 100000)
                {
                    nam = _MAKYCONG / 100;
                    thang = _MAKYCONG % 100;
                }
                else if (nam_f_bcct1 > 0 && thang_f1_bcct > 0)
                {
                    nam = nam_f_bcct1;
                    thang = thang_f1_bcct;
                    _MAKYCONG = nam * 100 + thang;
                }
                else
                {
                    nam = DateTime.Now.Year;
                    thang = DateTime.Now.Month;
                    _MAKYCONG = nam * 100 + thang;
                }

                lblKyCong.Text = _MAKYCONG.ToString();

                // 2. Xác định ngày từ _ngay (ví dụ: "D1", "D2", ..., "D31")
                if (!string.IsNullOrEmpty(_ngay))
                {
                    string dayStr = _ngay.StartsWith("D", StringComparison.OrdinalIgnoreCase)
                        ? _ngay.Substring(1)
                        : _ngay;
                    int.TryParse(dayStr, out ngay);
                }

                if (nam <= 0 || thang <= 0 || thang > 12 || ngay <= 0 || ngay > DateTime.DaysInMonth(nam, thang))
                {
                    MessageBox.Show("Ngày hoặc kỳ công không hợp lệ. Vui lòng chọn một ô ngày công hợp lệ (D1-D31).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                DateTime _d = new DateTime(nam, thang, ngay);
                cldNgayCong.SetDate(_d);
                lblNgay.Text = _d.ToString("dd/MM/yyyy");
                _cngay = ngay;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Vui lòng chọn đúng ô ngày công để cập nhật.\nLỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
            }
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioChamCong.SelectedIndex < 0)
                {
                    MessageBox.Show("Vui lòng chọn ký hiệu chấm công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string _valueChamCong = radioChamCong.Properties.Items[radioChamCong.SelectedIndex].Value.ToString();
                string _valueNgayNghi = radioNgayNghi.SelectedIndex >= 0 
                    ? radioNgayNghi.Properties.Items[radioNgayNghi.SelectedIndex].Value.ToString() 
                    : "NN";

                if (cldNgayCong.SelectionRange.Start.Year * 100 + cldNgayCong.SelectionRange.Start.Month != _MAKYCONG)
                {
                    MessageBox.Show("Vui lòng chọn đúng ngày công thuộc tháng hiện tại của kỳ công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int ngayChon = cldNgayCong.SelectionStart.Day;
                _cngay = ngayChon;

                var kcct = _kcct.getItem(_MAKYCONG, _manv);
                if (kcct == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu kỳ công chi tiết của nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                TB_BANGCONG_CHITIET bcctnv = _bcct_nv.getItem(_MAKYCONG, _manv, ngayChon);
                if (bcctnv == null)
                {
                    DateTime dateVal = new DateTime(_MAKYCONG / 100, _MAKYCONG % 100, ngayChon);
                    bcctnv = new TB_BANGCONG_CHITIET
                    {
                        MAKYCONG = _MAKYCONG,
                        MANV = _manv,
                        HOTEN = _hoten,
                        IDCTY = 1,
                        NGAY = dateVal,
                        THU = dateVal.DayOfWeek == DayOfWeek.Sunday ? "Chủ nhật" : ("Thứ " + ((int)dateVal.DayOfWeek + 1)),
                        GIOVAO = "08:00",
                        GIORA = "17:00",
                        NGAYPHEP = 0,
                        CONGNGAYLE = 0,
                        CONGCHUNHAT = dateVal.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                        CREATED_BY = 1,
                        CREATED_DATE = DateTime.Now
                    };
                    _bcct_nv.Add(bcctnv);
                }

                // Cập nhật KYCONGCHITIET => BANGCONG_NV_CHITIET
                _kcct.UpdateChamCong(_MAKYCONG, _manv, _cngay, _valueChamCong);

                bcctnv.KYHIEU = _valueChamCong;
                switch (_valueChamCong)
                {
                    case "X":
                        bcctnv.NGAYCONG = 1;
                        bcctnv.NGAYPHEP = 0;
                        bcctnv.GIOVAO = "08:00";
                        bcctnv.GIORA = "17:00";
                        break;
                    case "CD":
                        bcctnv.NGAYCONG = 1;
                        bcctnv.NGAYPHEP = 0;
                        bcctnv.GIOVAO = "22:00";
                        bcctnv.GIORA = "06:00";
                        break;
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

                // Tính lại các ngày công
                decimal tongngaycong = _bcct_nv.tongNgayCong(_MAKYCONG, _manv);
                decimal tongngayphep = _bcct_nv.tongNgayPhep(_MAKYCONG, _manv);

                kcct.NGAYPHEP = tongngayphep;
                kcct.TONGNGAYCONG = tongngaycong;
                _kcct.Update(kcct, 1);

                if (frmBCCC == null)
                {
                    frmBCCC = Application.OpenForms["FrmBangCong_ChiTiet"] as FrmBangCong_ChiTiet;
                }
                frmBCCC?.loadBangCong();

                MessageBox.Show("Cập nhật ngày công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật ngày công: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cldNgayCong_DateSelected(object sender, DateRangeEventArgs e)
        {
            _cngay = cldNgayCong.SelectionRange.Start.Day;
            lblNgay.Text = cldNgayCong.SelectionRange.Start.ToString("dd/MM/yyyy");
        }
    }
}