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
    public partial class FrmDashboardLuong : DevExpress.XtraEditors.XtraForm
    {
        public FrmDashboardLuong()
        {
            InitializeComponent();
        }

        private void FrmDashboardLuong_Shown(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                List<DashboardLuongDTO> dataLuong;
                List<DashboardLuongBinhQuanDTO> dataLBP;
                List<DashboardTangCaDTO> dataTC;

                using (var db = new MyEntities())
                {
                    // 1. Query 1: Get actual salary trend (last 6 months)
                    var dataLuongRaw = db.TB_BANGLUONG
                                         .Select(bl => new {
                                             bl.MAKYCONG,
                                             bl.THUC_LINH
                                         })
                                         .ToList()
                                         .GroupBy(bl => bl.MAKYCONG)
                                         .Select(g => new {
                                             KyCong = g.Key,
                                             TongLuong = g.Sum(x => x.THUC_LINH ?? 0)
                                         })
                                         .OrderByDescending(x => x.KyCong)
                                         .Take(6)
                                         .ToList();

                    dataLuong = dataLuongRaw.Select(x => new DashboardLuongDTO {
                                         KyCong = x.KyCong.ToString(),
                                         TongLuong = (double)x.TongLuong
                                     })
                                     .OrderBy(x => x.KyCong)
                                     .ToList();

                    // 2. Query 2: Average wage by department
                    var dataLBPRaw = (from bl in db.TB_BANGLUONG
                                      join nv in db.TB_NHANVIEN on bl.MANV equals nv.MANV
                                      join pb in db.TB_PHONGBAN on nv.IDPB equals pb.IDPB
                                      select new {
                                          TenPB = pb.TENPB,
                                          ThucLinh = bl.THUC_LINH
                                      })
                                      .ToList()
                                      .GroupBy(x => x.TenPB)
                                      .Select(g => new {
                                          PhongBan = g.Key,
                                          LuongBinhQuan = g.Average(x => x.ThucLinh ?? 0)
                                      })
                                      .ToList();

                    dataLBP = dataLBPRaw.Select(x => new DashboardLuongBinhQuanDTO {
                                      PhongBan = x.PhongBan,
                                      LuongBinhQuan = (double)x.LuongBinhQuan
                                  }).ToList();

                    // 3. Query 3: Overtime trend (last 6 months)
                    var dataTCRaw = db.TB_TANGCA
                                      .Select(tc => new {
                                          tc.NAM,
                                          tc.THANG,
                                          tc.SOGIO
                                      })
                                      .ToList()
                                      .Select(tc => new {
                                          KyCongKey = (tc.NAM ?? 0) * 100 + (tc.THANG ?? 0),
                                          Sogio = tc.SOGIO ?? 0
                                      })
                                      .GroupBy(tc => tc.KyCongKey)
                                      .Select(g => new {
                                          KyCong = g.Key,
                                          TongGio = g.Sum(x => x.Sogio)
                                      })
                                      .OrderByDescending(x => x.KyCong)
                                      .Take(6)
                                      .ToList();

                    dataTC = dataTCRaw.Select(x => new DashboardTangCaDTO {
                                      KyCong = x.KyCong.ToString(),
                                      TongGio = (double)x.TongGio
                                  })
                                  .OrderBy(x => x.KyCong)
                                  .ToList();
                }

                // Bind Chart 1: Xu hướng quỹ lương thực tế (6 tháng gần nhất)
                chartLuong.Series.Clear();
                Series seriesLuong = new Series("Quỹ lương (VND)", ViewType.Bar);
                foreach (var item in dataLuong)
                {
                    seriesLuong.Points.Add(new SeriesPoint(item.KyCong, item.TongLuong));
                }
                chartLuong.Series.Add(seriesLuong);
                seriesLuong.LabelsVisibility = DefaultBoolean.True;
                seriesLuong.Label.TextPattern = "{V:N0}";

                ChartTitle titleLuong = new ChartTitle();
                titleLuong.Text = "Xu hướng chi phí quỹ lương (6 tháng gần nhất)";
                chartLuong.Titles.Clear();
                chartLuong.Titles.Add(titleLuong);

                // Bind Chart 2: Lương Bình Quân theo Phòng Ban (Horizontal Bar)
                chartLuongBinhQuan.Series.Clear();
                Series seriesLBP = new Series("Lương bình quân (VND)", ViewType.Bar);
                foreach (var item in dataLBP)
                {
                    seriesLBP.Points.Add(new SeriesPoint(item.PhongBan, item.LuongBinhQuan));
                }
                chartLuongBinhQuan.Series.Add(seriesLBP);

                var diagramLBP = chartLuongBinhQuan.Diagram as XYDiagram;
                if (diagramLBP != null)
                {
                    diagramLBP.Rotated = true;
                }

                seriesLBP.LabelsVisibility = DefaultBoolean.True;
                seriesLBP.Label.TextPattern = "{V:N0}";

                ChartTitle titleLBP = new ChartTitle();
                titleLBP.Text = "Lương bình quân theo phòng ban";
                chartLuongBinhQuan.Titles.Clear();
                chartLuongBinhQuan.Titles.Add(titleLBP);

                // Bind Chart 3: Xu hướng giờ tăng ca (6 tháng gần nhất - Line)
                chartTangCaTrend.Series.Clear();
                Series seriesTC = new Series("Tổng giờ tăng ca", ViewType.Line);
                foreach (var item in dataTC)
                {
                    decimal makycong = Convert.ToDecimal(item.KyCong);
                    int month = (int)makycong % 100;
                    int year = (int)makycong / 100;
                    string label = string.Format("{0:D2}/{1}", month, year);
                    seriesTC.Points.Add(new SeriesPoint(label, item.TongGio));
                }
                chartTangCaTrend.Series.Add(seriesTC);
                seriesTC.LabelsVisibility = DefaultBoolean.True;
                seriesTC.Label.TextPattern = "{V:N1}";

                ChartTitle titleTC = new ChartTitle();
                titleTC.Text = "Xu hướng giờ tăng ca (6 tháng gần nhất)";
                chartTangCaTrend.Titles.Clear();
                chartTangCaTrend.Titles.Add(titleTC);
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
                XtraMessageBox.Show("Lỗi tải dữ liệu phân tích lương: " + errMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
