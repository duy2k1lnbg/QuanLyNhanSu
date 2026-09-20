using DA;
using DevExpress.Utils;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bu.DTO;

namespace QLyNSu.FORM_BAOCAO
{
    public partial class FrmDashboardNhanSu : DevExpress.XtraEditors.XtraForm
    {
        public FrmDashboardNhanSu()
        {
            InitializeComponent();
        }

        private void FrmDashboardNhanSu_Shown(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                List<DashboardPhongBanDTO> dataPB;
                List<DashboardGioiTinhDTO> dataGT;
                List<DashboardTrinhDoDTO> dataTD;
                List<DashboardTuoiDTO> sortedAgeGroups;

                using (var db = new MyEntities())
                {
                    // Load active employees into memory using simple query projections
                    var employeesRaw = db.TB_NHANVIEN
                                         .Where(nv => (nv.DATHOIVIEC ?? 0) == 0)
                                         .Select(nv => new {
                                             TenPB = nv.TB_PHONGBAN.TENPB,
                                             TenGT = nv.TB_GIOITINH.TENGT,
                                             TenTD = nv.TB_TRINHDO.TENTD,
                                             NgaySinh = nv.NGAYSINH
                                         })
                                         .ToList();

                    var employees = employeesRaw.Select(nv => new {
                                          PhongBan = nv.TenPB ?? "Chưa xếp",
                                          GioiTinh = nv.TenGT ?? "Chưa xác định",
                                          TrinhDo = nv.TenTD ?? "Khác",
                                          NgaySinh = nv.NgaySinh
                                      }).ToList();

                    // 1. Chart 1: Cơ cấu nhân sự theo Phòng ban (Pie)
                    dataPB = employees
                              .GroupBy(x => x.PhongBan)
                              .Select(g => new DashboardPhongBanDTO {
                                  PhongBan = g.Key,
                                  SoLuong = g.Count()
                              }).ToList();

                    // 2. Chart 2: Tỷ lệ Giới tính (Doughnut)
                    dataGT = employees
                              .GroupBy(x => x.GioiTinh)
                              .Select(g => new DashboardGioiTinhDTO {
                                  GioiTinh = g.Key,
                                  SoLuong = g.Count()
                              }).ToList();

                    // 3. Chart 3: Trình độ học vấn (Bar)
                    dataTD = employees
                              .GroupBy(x => x.TrinhDo)
                              .Select(g => new DashboardTrinhDoDTO {
                                  TrinhDo = g.Key,
                                  SoLuong = g.Count()
                              }).ToList();

                    // 4. Chart 4: Phân bố Độ tuổi (Column)
                    var today = DateTime.Today;
                    var ageGroups = employees
                                      .Where(x => x.NgaySinh != null)
                                      .Select(nv => {
                                          int age = today.Year - nv.NgaySinh.Value.Year;
                                          if (nv.NgaySinh.Value.Date > today.AddYears(-age)) age--;
                                          return age;
                                      })
                                      .GroupBy(age => {
                                          if (age < 25) return "< 25";
                                          if (age <= 35) return "25 - 35";
                                          if (age <= 45) return "36 - 45";
                                          return "> 45";
                                      })
                                      .Select(g => new { Range = g.Key, SoLuong = g.Count() })
                                      .ToList();

                    sortedAgeGroups = new List<string> { "< 25", "25 - 35", "36 - 45", "> 45" }
                        .Select(r => new DashboardTuoiDTO { Range = r, SoLuong = ageGroups.FirstOrDefault(x => x.Range == r)?.SoLuong ?? 0 })
                        .ToList();
                }

                // Bind Chart 1: Cơ cấu nhân sự theo Phòng ban (Pie)
                chartPhongBan.Series.Clear();
                Series seriesPB = new Series("Cơ cấu nhân sự theo phòng ban", ViewType.Pie);
                foreach (var item in dataPB)
                {
                    seriesPB.Points.Add(new SeriesPoint(item.PhongBan, item.SoLuong));
                }
                chartPhongBan.Series.Add(seriesPB);
                seriesPB.Label.TextPattern = "{A}: {V} ({VP:P0})";
                seriesPB.LegendTextPattern = "{A}";
                
                ChartTitle titlePB = new ChartTitle();
                titlePB.Text = "Cơ cấu nhân sự theo phòng ban (Số lượng nhân viên)";
                chartPhongBan.Titles.Clear();
                chartPhongBan.Titles.Add(titlePB);

                // Bind Chart 2: Tỷ lệ Giới tính (Doughnut)
                chartGioiTinh.Series.Clear();
                Series seriesGT = new Series("Tỷ lệ Giới tính", ViewType.Doughnut);
                foreach (var item in dataGT)
                {
                    seriesGT.Points.Add(new SeriesPoint(item.GioiTinh, item.SoLuong));
                }
                chartGioiTinh.Series.Add(seriesGT);
                seriesGT.Label.TextPattern = "{A}: {V} ({VP:P0})";
                seriesGT.LegendTextPattern = "{A}";

                ChartTitle titleGT = new ChartTitle();
                titleGT.Text = "Tỷ lệ giới tính nhân viên";
                chartGioiTinh.Titles.Clear();
                chartGioiTinh.Titles.Add(titleGT);

                // Bind Chart 3: Trình độ học vấn (Bar)
                chartTrinhDo.Series.Clear();
                Series seriesTD = new Series("Trình độ học vấn", ViewType.Bar);
                foreach (var item in dataTD)
                {
                    seriesTD.Points.Add(new SeriesPoint(item.TrinhDo, item.SoLuong));
                }
                chartTrinhDo.Series.Add(seriesTD);
                seriesTD.LabelsVisibility = DefaultBoolean.True;
                seriesTD.Label.TextPattern = "{V}";

                ChartTitle titleTD = new ChartTitle();
                titleTD.Text = "Phân tích trình độ học vấn";
                chartTrinhDo.Titles.Clear();
                chartTrinhDo.Titles.Add(titleTD);

                // Bind Chart 4: Phân bố Độ tuổi (Column)
                chartTuoi.Series.Clear();
                Series seriesTuoi = new Series("Độ tuổi", ViewType.Bar);
                foreach (var item in sortedAgeGroups)
                {
                    seriesTuoi.Points.Add(new SeriesPoint(item.Range, item.SoLuong));
                }
                chartTuoi.Series.Add(seriesTuoi);
                seriesTuoi.LabelsVisibility = DefaultBoolean.True;
                seriesTuoi.Label.TextPattern = "{V}";

                ChartTitle titleTuoi = new ChartTitle();
                titleTuoi.Text = "Phân bố độ tuổi nhân viên";
                chartTuoi.Titles.Clear();
                chartTuoi.Titles.Add(titleTuoi);
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errMsg += "\r\nInner Exception: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errMsg += "\r\nDetails: " + ex.InnerException.InnerException.Message;
                    }
                }
                XtraMessageBox.Show("Lỗi tải dữ liệu phân tích nhân sự: " + errMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
