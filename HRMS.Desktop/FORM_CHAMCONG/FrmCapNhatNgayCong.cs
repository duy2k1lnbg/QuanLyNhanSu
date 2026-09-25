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

                // Tải dữ liệu quẹt thẻ thực tế từ TB_BANGCONG và Bảng công chi tiết
                LoadThongTinNgayCong(_cngay);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Vui lòng chọn đúng ô ngày công để cập nhật.\nLỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
            }
        }

        /// <summary>
        /// Tải thông tin giờ vào/ra từ TB_BANGCONG và trạng thái công từ TB_BANGCONG_CHITIET
        /// </summary>
        private void LoadThongTinNgayCong(int ngay)
        {
            try
            {
                int nam = _MAKYCONG / 100;
                int thang = _MAKYCONG % 100;
                if (nam <= 0 || thang <= 0 || ngay <= 0 || ngay > DateTime.DaysInMonth(nam, thang)) return;

                DateTime _d = new DateTime(nam, thang, ngay);
                lblNgay.Text = _d.ToString("dd/MM/yyyy");
                _cngay = ngay;

                // 1. Kiểm tra và tải dữ liệu quẹt thẻ thực tế từ TB_BANGCONG
                var raw = _bcct_nv.GetBangCongRaw(_manv, nam, thang, ngay);
                if (raw != null)
                {
                    int gv = (int)(raw.GIOVAO ?? 8);
                    int pv = (int)(raw.PHUTVAO ?? 0);
                    int gr = (int)(raw.GIORA ?? 17);
                    int pr = (int)(raw.PHUTRA ?? 0);
                    timeEditGioVao.Time = new DateTime(nam, thang, ngay, Math.Min(Math.Max(gv, 0), 23), Math.Min(Math.Max(pv, 0), 59), 0);
                    timeEditGioRa.Time = new DateTime(nam, thang, ngay, Math.Min(Math.Max(gr, 0), 23), Math.Min(Math.Max(pr, 0), 59), 0);
                    lblTrangThaiBangCong.Text = $"✓ Đã liên kết TB_BANGCONG (MABC: #{raw.MABC})";
                    lblTrangThaiBangCong.ForeColor = Color.ForestGreen;
                }
                else
                {
                    // Fallback sang TB_BANGCONG_CHITIET nếu đã có
                    var bcctnv = _bcct_nv.getItem(_MAKYCONG, _manv, ngay);
                    if (bcctnv != null && !string.IsNullOrEmpty(bcctnv.GIOVAO) && !string.IsNullOrEmpty(bcctnv.GIORA))
                    {
                        if (TimeSpan.TryParse(bcctnv.GIOVAO, out TimeSpan tv))
                        {
                            timeEditGioVao.Time = new DateTime(nam, thang, ngay, tv.Hours, tv.Minutes, 0);
                        }
                        else
                        {
                            timeEditGioVao.Time = new DateTime(nam, thang, ngay, 8, 0, 0);
                        }

                        if (TimeSpan.TryParse(bcctnv.GIORA, out TimeSpan tr))
                        {
                            timeEditGioRa.Time = new DateTime(nam, thang, ngay, tr.Hours, tr.Minutes, 0);
                        }
                        else
                        {
                            timeEditGioRa.Time = new DateTime(nam, thang, ngay, 17, 0, 0);
                        }
                        lblTrangThaiBangCong.Text = "Chưa có TB_BANGCONG gốc (sẽ tự động tạo mới)";
                        lblTrangThaiBangCong.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        timeEditGioVao.Time = new DateTime(nam, thang, ngay, 8, 0, 0);
                        timeEditGioRa.Time = new DateTime(nam, thang, ngay, 17, 0, 0);
                        lblTrangThaiBangCong.Text = "Chưa có dữ liệu quẹt thẻ (sẽ tự động tạo mới)";
                        lblTrangThaiBangCong.ForeColor = Color.Gray;
                    }
                }

                // 2. Tải ký hiệu chấm công & thời gian nghỉ tương ứng từ TB_BANGCONG_CHITIET
                var currentBcct = _bcct_nv.getItem(_MAKYCONG, _manv, ngay);
                if (currentBcct != null && !string.IsNullOrEmpty(currentBcct.KYHIEU))
                {
                    string kh = currentBcct.KYHIEU.Trim();
                    radioChamCong.EditValue = kh;

                    if (currentBcct.NGAYPHEP == 1m)
                    {
                        radioNgayNghi.EditValue = "NN";
                    }
                    else if (currentBcct.NGAYPHEP == 0.5m)
                    {
                        radioNgayNghi.EditValue = "S";
                    }
                    else
                    {
                        radioNgayNghi.EditValue = "KHONG";
                    }
                }
                else
                {
                    // Mặc định là Đi làm (1 ngày công) và Không nghỉ
                    radioChamCong.EditValue = "X";
                    radioNgayNghi.EditValue = "KHONG";
                }
            }
            catch (Exception ex)
            {
                lblTrangThaiBangCong.Text = "Lỗi tải thông tin: " + ex.Message;
                lblTrangThaiBangCong.ForeColor = Color.Red;
            }
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioChamCong.SelectedIndex < 0)
                {
                    MessageBox.Show("Vui lòng chọn trạng thái chấm công (Đi làm, Ca đêm, Nghỉ phép...).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string _valueChamCong = radioChamCong.Properties.Items[radioChamCong.SelectedIndex].Value.ToString();
                string _valueNgayNghi = radioNgayNghi.SelectedIndex >= 0 
                    ? radioNgayNghi.Properties.Items[radioNgayNghi.SelectedIndex].Value.ToString() 
                    : "KHONG";

                if (cldNgayCong.SelectionRange.Start.Year * 100 + cldNgayCong.SelectionRange.Start.Month != _MAKYCONG)
                {
                    MessageBox.Show("Vui lòng chọn đúng ngày công thuộc tháng hiện tại của kỳ công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int nam = _MAKYCONG / 100;
                int thang = _MAKYCONG % 100;
                int ngayChon = cldNgayCong.SelectionStart.Day;
                _cngay = ngayChon;

                int gioVao = timeEditGioVao.Time.Hour;
                int phutVao = timeEditGioVao.Time.Minute;
                int gioRa = timeEditGioRa.Time.Hour;
                int phutRa = timeEditGioRa.Time.Minute;

                // Gọi method ACID cập nhật đồng thời TB_BANGCONG, TB_BANGCONG_CHITIET và TB_KYCONGCHITIET
                _bcct_nv.CapNhatNgayCongVaBangCongRaw(
                    _manv,
                    _MAKYCONG,
                    nam,
                    thang,
                    _cngay,
                    gioVao,
                    phutVao,
                    gioRa,
                    phutRa,
                    _valueChamCong,
                    _valueNgayNghi,
                    1
                );

                if (frmBCCC == null)
                {
                    frmBCCC = Application.OpenForms["FrmBangCong_ChiTiet"] as FrmBangCong_ChiTiet;
                }
                frmBCCC?.loadBangCong();

                // Cập nhật lại UI hiển thị thông tin vừa lưu
                LoadThongTinNgayCong(_cngay);

                string tenCong = (_valueChamCong == "X") ? "Đi làm (1 ngày công)" :
                                 (_valueChamCong == "CD") ? "Ca đêm (1 ngày công)" :
                                 (_valueChamCong == "P") ? "Nghỉ phép" :
                                 (_valueChamCong == "V") ? "Vắng" :
                                 (_valueChamCong == "VR") ? "Việc riêng" :
                                 (_valueChamCong == "CT") ? "Công tác" : _valueChamCong;

                MessageBox.Show($"Cập nhật thành công ngày {_cngay:D2}/{thang:D2}/{nam}!\n" +
                                $"- Giờ vào: {gioVao:D2}:{phutVao:D2}\n" +
                                $"- Giờ ra: {gioRa:D2}:{phutRa:D2}\n" +
                                $"- Trạng thái: {tenCong}\n" +
                                $"Dữ liệu đã được liên kết và cập nhật đồng bộ vào TB_BANGCONG.",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            LoadThongTinNgayCong(_cngay);
        }

        private void radioChamCong_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioChamCong.SelectedIndex < 0) return;
            string val = radioChamCong.Properties.Items[radioChamCong.SelectedIndex].Value?.ToString();

            if (val == "X") // Đi làm
            {
                radioNgayNghi.EditValue = "KHONG";
                if (timeEditGioVao.Time.Hour == 22 && timeEditGioRa.Time.Hour == 6)
                {
                    timeEditGioVao.Time = new DateTime(timeEditGioVao.Time.Year, timeEditGioVao.Time.Month, timeEditGioVao.Time.Day, 8, 0, 0);
                    timeEditGioRa.Time = new DateTime(timeEditGioRa.Time.Year, timeEditGioRa.Time.Month, timeEditGioRa.Time.Day, 17, 0, 0);
                }
            }
            else if (val == "CD") // Ca đêm
            {
                radioNgayNghi.EditValue = "KHONG";
                if (timeEditGioVao.Time.Hour == 8 && timeEditGioRa.Time.Hour == 17)
                {
                    timeEditGioVao.Time = new DateTime(timeEditGioVao.Time.Year, timeEditGioVao.Time.Month, timeEditGioVao.Time.Day, 22, 0, 0);
                    timeEditGioRa.Time = new DateTime(timeEditGioRa.Time.Year, timeEditGioRa.Time.Month, timeEditGioRa.Time.Day, 6, 0, 0);
                }
            }
            else if (val == "P" || val == "V" || val == "VR")
            {
                // Khi chuyển sang nghỉ phép / vắng, nếu đang chọn "Không nghỉ" thì tự động chuyển sang "Nguyên ngày"
                if (radioNgayNghi.EditValue?.ToString() == "KHONG")
                {
                    radioNgayNghi.EditValue = "NN";
                }
            }
        }

        private void btnGioMacDinh_Click(object sender, EventArgs e)
        {
            string val = radioChamCong.SelectedIndex >= 0
                ? radioChamCong.Properties.Items[radioChamCong.SelectedIndex].Value?.ToString()
                : "X";

            if (val == "CD")
            {
                timeEditGioVao.Time = new DateTime(timeEditGioVao.Time.Year, timeEditGioVao.Time.Month, timeEditGioVao.Time.Day, 22, 0, 0);
                timeEditGioRa.Time = new DateTime(timeEditGioRa.Time.Year, timeEditGioRa.Time.Month, timeEditGioRa.Time.Day, 6, 0, 0);
            }
            else
            {
                timeEditGioVao.Time = new DateTime(timeEditGioVao.Time.Year, timeEditGioVao.Time.Month, timeEditGioVao.Time.Day, 8, 0, 0);
                timeEditGioRa.Time = new DateTime(timeEditGioRa.Time.Year, timeEditGioRa.Time.Month, timeEditGioRa.Time.Day, 17, 0, 0);
            }
        }
    }
}