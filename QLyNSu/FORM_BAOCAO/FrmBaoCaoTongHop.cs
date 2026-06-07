using Bu;
using Bu.CLASS_CHAMCONG;
using Bu.DTO;
using DA;
using DevExpress.Utils;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraReports.UI;
using QLyNSu.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_BAOCAO
{
    public partial class FrmBaoCaoTongHop : DevExpress.XtraEditors.XtraForm
    {
        private MyEntities db = new MyEntities();
        private KYCONG _kycong = new KYCONG();
        private HOPDONGLAODONG _hdld = new HOPDONGLAODONG();

        public FrmBaoCaoTongHop()
        {
            InitializeComponent();
        }

        private void FrmBaoCaoTongHop_Load(object sender, EventArgs e)
        {
            LoadCombo();
            lstBaoCao.SelectedIndex = 0; // Default selection
            UpdateFilterVisibility();
            LoadDashboard();
        }

        private void LoadCombo()
        {
            try
            {
                // Load Kỳ công
                var lstKyCong = _kycong.getList();
                cboKyCong.Properties.DataSource = lstKyCong;
                cboKyCong.Properties.DisplayMember = "MAKYCONG";
                cboKyCong.Properties.ValueMember = "MAKYCONG";
                cboKyCong.Properties.Columns.Clear();
                cboKyCong.Properties.Columns.Add(new LookUpColumnInfo("MAKYCONG", "Kỳ Công"));

                // Load Nhân viên
                var lstNhanVien = db.TB_NHANVIEN.Where(x => (x.DATHOIVIEC ?? 0) == 0).ToList();
                cboNhanVien.Properties.DataSource = lstNhanVien;
                cboNhanVien.Properties.DisplayMember = "HOTEN";
                cboNhanVien.Properties.ValueMember = "MANV";
                cboNhanVien.Properties.Columns.Clear();
                cboNhanVien.Properties.Columns.Add(new LookUpColumnInfo("MANV", "Mã NV", 60));
                cboNhanVien.Properties.Columns.Add(new LookUpColumnInfo("HOTEN", "Họ Tên", 150));

                // Default month selection
                cboMonth.Text = DateTime.Now.Month.ToString();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải danh mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstBaoCao_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFilterVisibility();
            gcData.DataSource = null;
        }

        private void UpdateFilterVisibility()
        {
            int index = lstBaoCao.SelectedIndex;
            
            // Hide all first
            lblKyCong.Visible = false;
            cboKyCong.Visible = false;
            lblNhanVien.Visible = false;
            cboNhanVien.Visible = false;
            lblMonth.Visible = false;
            cboMonth.Visible = false;
            lblDays.Visible = false;
            spinDays.Visible = false;

            if (index == 0) // Hợp đồng sắp hết hạn
            {
                lblDays.Visible = true;
                spinDays.Visible = true;
                lblDays.Location = new Point(20, 33);
                spinDays.Location = new Point(90, 30);
            }
            else if (index == 1) // Sinh nhật nhân viên
            {
                lblMonth.Visible = true;
                cboMonth.Visible = true;
                lblMonth.Location = new Point(20, 33);
                cboMonth.Location = new Point(90, 30);
            }
            else if (index == 2) // Tổng hợp giờ tăng ca
            {
                lblKyCong.Visible = true;
                cboKyCong.Visible = true;
                lblKyCong.Location = new Point(20, 33);
                cboKyCong.Location = new Point(90, 30);
            }
            else if (index == 3) // Phiếu chi tiết lương
            {
                lblKyCong.Visible = true;
                cboKyCong.Visible = true;
                lblNhanVien.Visible = true;
                cboNhanVien.Visible = true;
                
                lblKyCong.Location = new Point(20, 33);
                cboKyCong.Location = new Point(90, 30);
                lblNhanVien.Location = new Point(230, 33);
                cboNhanVien.Location = new Point(315, 30);
            }
        }

        private void Filter_ValueChanged(object sender, EventArgs e)
        {
            // Auto reload data when filter value changes
            LoadData();
        }

        private void btnXem_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            int index = lstBaoCao.SelectedIndex;
            try
            {
                if (index == 0) // Hợp đồng sắp hết hạn
                {
                    int days = Convert.ToInt32(spinDays.Value);
                    DateTime targetDate = DateTime.Now.AddDays(days);
                    
                    var lstHD = _hdld.getlistFull_DTO()
                        .Where(x => {
                            if (string.IsNullOrEmpty(x.NGAYKETTHUC)) return false;
                            DateTime dt;
                            if (DateTime.TryParse(x.NGAYKETTHUC, out dt)) {
                                return dt >= DateTime.Today && dt <= targetDate;
                            }
                            return false;
                        })
                        .ToList();

                    gcData.DataSource = lstHD;
                    ConfigureGridContract();
                }
                else if (index == 1) // Sinh nhật nhân viên
                {
                    if (string.IsNullOrEmpty(cboMonth.Text)) return;
                    int month = Convert.ToInt32(cboMonth.Text);

                    var lstNV = db.TB_NHANVIEN
                        .Where(x => (x.DATHOIVIEC ?? 0) == 0 && x.NGAYSINH != null && x.NGAYSINH.Value.Month == month)
                        .Select(x => new {
                            x.MANV,
                            x.HOTEN,
                            GIOITINH = x.TB_GIOITINH != null ? x.TB_GIOITINH.TENGT : "",
                            x.NGAYSINH,
                            x.DIENTHOAI,
                            x.CCCD
                        })
                        .ToList();

                    gcData.DataSource = lstNV;
                    ConfigureGridBirthday();
                }
                else if (index == 2) // Tổng hợp giờ tăng ca
                {
                    if (cboKyCong.EditValue == null) return;
                    decimal makycong = Convert.ToDecimal(cboKyCong.EditValue);
                    int nam = (int)makycong / 100;
                    int thang = (int)makycong % 100;

                    var lstTC = (from tc in db.TB_TANGCA
                                 join nv in db.TB_NHANVIEN on tc.MANV equals nv.MANV
                                 join lc in db.TB_LOAICA on tc.IDLOAICA equals lc.IDLOAICA
                                 where tc.NAM == nam && tc.THANG == thang && (nv.DATHOIVIEC ?? 0) == 0
                                 select new {
                                     tc.IDTCA,
                                     tc.MANV,
                                     nv.HOTEN,
                                     tc.NGAY,
                                     tc.SOGIO,
                                     lc.TENLOAICA,
                                     lc.HESOLOAICA,
                                     tc.SOTIENTC,
                                     tc.GHICHU
                                 }).ToList();

                    gcData.DataSource = lstTC;
                    ConfigureGridOvertime();
                }
                else if (index == 3) // Phiếu chi tiết lương
                {
                    if (cboKyCong.EditValue == null || cboNhanVien.EditValue == null) return;
                    decimal makycong = Convert.ToDecimal(cboKyCong.EditValue);
                    decimal manv = Convert.ToDecimal(cboNhanVien.EditValue);

                    var bl = db.TB_BANGLUONG.Where(x => x.MAKYCONG == makycong && x.MANV == manv).ToList();
                    gcData.DataSource = bl;
                    ConfigureGridSalary();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridContract()
        {
            gvData.Columns.Clear();
            AddColumn("SOHD", "Số hợp đồng", 120);
            AddColumn("MANV", "Mã NV", 80);
            AddColumn("HOTEN", "Họ tên", 180);
            AddColumn("NGAYKY", "Ngày ký", 110, "{0:dd/MM/yyyy}");
            AddColumn("NGAYKETTHUC", "Ngày kết thúc", 110, "{0:dd/MM/yyyy}");
            AddColumn("LANKY", "Lần ký", 80);
            AddColumn("HESOLUONG", "Hệ số lương", 100, "{0:N2}");
            AddColumn("LUONG_THOA_THUAN", "Lương thỏa thuận", 130, "{0:N0}");
            gvData.OptionsBehavior.Editable = false;
            FormManager_Functions.CustomView_Colums(gvData);
        }

        private void ConfigureGridBirthday()
        {
            gvData.Columns.Clear();
            AddColumn("MANV", "Mã NV", 80);
            AddColumn("HOTEN", "Họ tên", 180);
            AddColumn("GIOITINH", "Giới tính", 80);
            AddColumn("NGAYSINH", "Ngày sinh", 110, "{0:dd/MM/yyyy}");
            AddColumn("DIENTHOAI", "Điện thoại", 110);
            AddColumn("EMAIL", "Email", 160);
            AddColumn("CCCD", "CCCD / CMND", 130);
            gvData.OptionsBehavior.Editable = false;
            FormManager_Functions.CustomView_Colums(gvData);
        }

        private void ConfigureGridOvertime()
        {
            gvData.Columns.Clear();
            AddColumn("MANV", "Mã NV", 80);
            AddColumn("HOTEN", "Họ tên", 180);
            AddColumn("NGAY", "Ngày công", 90);
            AddColumn("SOGIO", "Số giờ tăng ca", 110, "{0:N1}");
            AddColumn("TENLOAICA", "Loại ca", 110);
            AddColumn("HESOLOAICA", "Hệ số ca", 90, "{0:N1}");
            AddColumn("SOTIENTC", "Số tiền trả (VND)", 140, "{0:N0}");
            AddColumn("GHICHU", "Ghi chú", 160);
            gvData.OptionsBehavior.Editable = false;
            FormManager_Functions.CustomView_Colums(gvData);
        }

        private void ConfigureGridSalary()
        {
            gvData.Columns.Clear();
            AddColumn("MANV", "Mã NV", 80);
            AddColumn("HOTEN", "Họ tên", 180);
            AddColumn("CONG_CHUAN", "Công chuẩn", 90, "{0:N1}");
            AddColumn("CONG_THUCTE", "Công thực tế", 100, "{0:N1}");
            AddColumn("LUONG_CONG_THUCTE", "Lương công thực", 130, "{0:N0}");
            AddColumn("PHUCAP_CONG_THUCTE", "Phụ cấp thực tế", 130, "{0:N0}");
            AddColumn("TIEN_TANGCA", "Tiền tăng ca", 120, "{0:N0}");
            AddColumn("TIEN_CHUYENCAN", "Chuyên cần", 110, "{0:N0}");
            AddColumn("TIEN_AN_CA", "Ăn ca", 100, "{0:N0}");
            AddColumn("TIEN_BHXH_TRICH", "BHXH trích", 120, "{0:N0}");
            AddColumn("TIEN_TAMUNG", "Tạm ứng", 120, "{0:N0}");
            AddColumn("THUC_LINH", "Thực lĩnh", 140, "{0:N0}");
            gvData.OptionsBehavior.Editable = false;
            FormManager_Functions.CustomView_Colums(gvData);
        }

        private void AddColumn(string fieldName, string caption, int width, string format = "")
        {
            var col = gvData.Columns.AddField(fieldName);
            col.Caption = caption;
            col.Visible = true;
            col.Width = width;
            if (!string.IsNullOrEmpty(format))
            {
                if (format.Contains("dd/MM/yyyy"))
                {
                    col.DisplayFormat.FormatType = FormatType.DateTime;
                }
                else
                {
                    col.DisplayFormat.FormatType = FormatType.Numeric;
                }
                col.DisplayFormat.FormatString = format;
            }
        }

        private void btnIn_Click(object sender, EventArgs e)
        {
            int index = lstBaoCao.SelectedIndex;
            try
            {
                if (index == 0) // Hợp đồng sắp hết hạn
                {
                    int days = Convert.ToInt32(spinDays.Value);
                    DateTime targetDate = DateTime.Now.AddDays(days);
                    var lstHD = _hdld.getlistFull_DTO()
                        .Where(x => {
                            if (string.IsNullOrEmpty(x.NGAYKETTHUC)) return false;
                            DateTime dt;
                            if (DateTime.TryParse(x.NGAYKETTHUC, out dt)) {
                                return dt >= DateTime.Today && dt <= targetDate;
                            }
                            return false;
                        })
                        .ToList();

                    if (lstHD.Count == 0)
                    {
                        XtraMessageBox.Show("Không có dữ liệu hợp đồng sắp hết hạn trong khoảng ngày đã chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var rpt = new rptDSHopDongHetHan(lstHD);
                    rpt.ShowRibbonPreviewDialog();
                }
                else if (index == 1) // Sinh nhật nhân viên
                {
                    if (gvData.RowCount == 0)
                    {
                        XtraMessageBox.Show("Không có dữ liệu sinh nhật để in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    gcData.ShowRibbonPrintPreview();
                }
                else if (index == 2) // Tổng hợp giờ tăng ca
                {
                    if (cboKyCong.EditValue == null)
                    {
                        XtraMessageBox.Show("Vui lòng chọn kỳ công để in báo cáo tăng ca.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    decimal makycong = Convert.ToDecimal(cboKyCong.EditValue);
                    int nam = (int)makycong / 100;
                    int thang = (int)makycong % 100;

                    var lstTC = (from tc in db.TB_TANGCA
                                 join nv in db.TB_NHANVIEN on tc.MANV equals nv.MANV
                                 join lc in db.TB_LOAICA on tc.IDLOAICA equals lc.IDLOAICA
                                 where tc.NAM == nam && tc.THANG == thang && (nv.DATHOIVIEC ?? 0) == 0
                                 select new {
                                     tc.IDTCA,
                                     tc.MANV,
                                     nv.HOTEN,
                                     tc.NGAY,
                                     tc.SOGIO,
                                     lc.TENLOAICA,
                                     lc.HESOLOAICA,
                                     tc.SOTIENTC,
                                     tc.GHICHU
                                 }).ToList();

                    if (lstTC.Count == 0)
                    {
                        XtraMessageBox.Show("Không có dữ liệu tăng ca trong kỳ công này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable dt = new DataTable();
                    dt.Columns.Add("MANV", typeof(int));
                    dt.Columns.Add("HOTEN", typeof(string));
                    dt.Columns.Add("NGAY", typeof(int));
                    dt.Columns.Add("SOGIO", typeof(double));
                    dt.Columns.Add("TENLOAICA", typeof(string));
                    dt.Columns.Add("HESOLOAICA", typeof(double));
                    dt.Columns.Add("SOTIENTC", typeof(double));
                    dt.Columns.Add("GHICHU", typeof(string));

                    foreach (var item in lstTC)
                    {
                        dt.Rows.Add(item.MANV, item.HOTEN, item.NGAY ?? 0, (double)(item.SOGIO ?? 0), item.TENLOAICA, (double)(item.HESOLOAICA ?? 0), (double)(item.SOTIENTC ?? 0), item.GHICHU);
                    }

                    var rpt = new rptDSTangCa(dt, (int)makycong);
                    rpt.ShowRibbonPreviewDialog();
                }
                else if (index == 3) // Phiếu chi tiết lương
                {
                    if (cboKyCong.EditValue == null || cboNhanVien.EditValue == null)
                    {
                        XtraMessageBox.Show("Vui lòng chọn kỳ công và nhân viên để in phiếu lương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    decimal makycong = Convert.ToDecimal(cboKyCong.EditValue);
                    decimal manv = Convert.ToDecimal(cboNhanVien.EditValue);

                    var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                    if (bl == null)
                    {
                        XtraMessageBox.Show("Không tìm thấy dữ liệu bảng lương cho nhân viên trong kỳ công đã chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                    string hoten = nv != null ? nv.HOTEN : "";
                    string tenpb = "";
                    if (nv != null && nv.IDPB != null)
                    {
                        var pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == nv.IDPB);
                        if (pb != null) tenpb = pb.TENPB;
                    }

                    int nam = (int)makycong / 100;
                    int thang = (int)makycong % 100;

                    double otHours = 0;
                    var lstTangCa = db.TB_TANGCA.Where(x => x.MANV == manv && x.THANG == thang && x.NAM == nam).ToList();
                    if (lstTangCa.Count > 0)
                    {
                        otHours = (double)lstTangCa.Sum(x => x.SOGIO ?? 0);
                    }

                    var phucaps = db.TB_NHANVIEN_PHUCAP.Where(x => x.MANV == manv && x.MAKYCONG == makycong).ToList();

                    rptBaoCaoLuongNV rpt = new rptBaoCaoLuongNV();
                    rpt.BindData(bl, hoten, tenpb, otHours, phucaps);
                    rpt.ShowRibbonPreviewDialog();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi hiển thị báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            if (gvData.RowCount == 0)
            {
                XtraMessageBox.Show("Không có dữ liệu hiển thị để xuất Excel.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files (.xlsx)|*.xlsx";
                sfd.FileName = "BaoCao_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        gcData.ExportToXlsx(sfd.FileName);
                        XtraMessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tabControl_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (tabControl.SelectedTabPage == tabDashboard)
            {
                LoadDashboard();
            }
        }

        private void LoadDashboard()
        {
            try
            {
                // Chart 1: Cơ cấu nhân sự theo Phòng ban
                chartPhongBan.Series.Clear();
                var dataPB = (from nv in db.TB_NHANVIEN
                              join pb in db.TB_PHONGBAN on nv.IDPB equals pb.IDPB
                              where (nv.DATHOIVIEC ?? 0) == 0
                              group nv by pb.TENPB into g
                              select new {
                                  PhongBan = g.Key,
                                  SoLuong = g.Count()
                              }).ToList();

                Series seriesPB = new Series("Cơ cấu nhân sự theo phòng ban", ViewType.Pie);
                foreach (var item in dataPB)
                {
                    seriesPB.Points.Add(new SeriesPoint(item.PhongBan, item.SoLuong));
                }
                chartPhongBan.Series.Add(seriesPB);
                seriesPB.Label.TextPattern = "{A}: {V} ({VP:P0})";
                seriesPB.LegendTextPattern = "{A}";
                
                // Set Pie Chart Title
                ChartTitle titlePB = new ChartTitle();
                titlePB.Text = "Cơ cấu nhân sự theo phòng ban (Số lượng nhân viên)";
                chartPhongBan.Titles.Clear();
                chartPhongBan.Titles.Add(titlePB);

                // Chart 2: Xu hướng quỹ lương thực tế (6 tháng gần nhất)
                chartLuong.Series.Clear();
                var dataLuong = (from bl in db.TB_BANGLUONG
                                 group bl by bl.MAKYCONG into g
                                 orderby g.Key descending
                                 select new {
                                     KyCong = g.Key,
                                     TongLuong = g.Sum(x => x.THUC_LINH ?? 0)
                                 }).Take(6).ToList();

                dataLuong = dataLuong.OrderBy(x => x.KyCong).ToList();

                Series seriesLuong = new Series("Quỹ lương (VND)", ViewType.Bar);
                foreach (var item in dataLuong)
                {
                    seriesLuong.Points.Add(new SeriesPoint(item.KyCong.ToString(), (double)item.TongLuong));
                }
                chartLuong.Series.Add(seriesLuong);
                
                // Set Bar Chart Title
                ChartTitle titleLuong = new ChartTitle();
                titleLuong.Text = "Xu hướng chi phí quỹ lương (6 tháng gần nhất)";
                chartLuong.Titles.Clear();
                chartLuong.Titles.Add(titleLuong);

                // Enable labels
                seriesLuong.LabelsVisibility = DefaultBoolean.True;
                seriesLuong.Label.TextPattern = "{V:N0}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi load Dashboard: " + ex.Message);
            }
        }
    }
}