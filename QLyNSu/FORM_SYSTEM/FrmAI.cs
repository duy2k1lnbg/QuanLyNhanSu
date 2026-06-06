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
using Bu.Services.AI_Services;
using QLyNSu.Functions;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmAI : DevExpress.XtraEditors.XtraForm
    {
        private readonly ChatboxManager _manager;

        public FrmAI()
        {
            InitializeComponent();
            _manager = new ChatboxManager();
        }

        private void FrmAI_Load(object sender, EventArgs e)
        {
            // Apply DevExpress grid styling
            FormManager_Functions.CustomView_Colums(gvData);

            // Load KPIs and Charts from Database
            LoadDashboardStats();
        }

        private void LoadDashboardStats()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    // 1. Load KPIs
                    int totalEmp = db.TB_NHANVIEN.Count();
                    int totalDept = db.TB_PHONGBAN.Count();
                    int activeContracts = db.TB_HOPDONG.Count();
                    double avgSalaryCoeff = db.TB_HOPDONG.Average(h => (double?)h.HESOLUONG) ?? 2.5;

                    lblKpiVal1.Text = totalEmp.ToString();
                    lblKpiVal2.Text = totalDept.ToString();
                    lblKpiVal3.Text = activeContracts.ToString();
                    lblKpiVal4.Text = avgSalaryCoeff.ToString("N2");

                    // 2. Load Department Chart Data
                    var deptStats = db.TB_NHANVIEN
                        .GroupBy(n => n.TB_PHONGBAN.TENPB)
                        .Select(g => new 
                        { 
                            Department = g.Key ?? "Chưa phân phòng", 
                            Count = g.Count() 
                        })
                        .ToList();

                    var series = new DevExpress.XtraCharts.Series("Nhân viên theo phòng ban", DevExpress.XtraCharts.ViewType.Bar);
                    series.DataSource = deptStats;
                    series.ArgumentDataMember = "Department";
                    series.ValueDataMembers.AddRange(new string[] { "Count" });
                    
                    chartControl1.Series.Clear();
                    chartControl1.Series.Add(series);

                    // 3. Load Grid with Employee list
                    var empList = db.TB_NHANVIEN.Select(n => new 
                    { 
                        MANV = n.MANV, 
                        HOTEN = n.HOTEN, 
                        PHONGBAN = n.TB_PHONGBAN.TENPB, 
                        CHUCVU = n.TB_CHUCVU.TENCV, 
                        DIENTHOAI = n.DIENTHOAI, 
                        CCCD = n.CCCD 
                    }).ToList();

                    gcData.DataSource = empList;
                    gvData.BestFitColumns();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải số liệu Dashboard: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            txtSearch.Enabled = false;
            btnSearch.Enabled = false;
            
            // Cursor wait
            this.Cursor = Cursors.WaitCursor;

            try
            {
                var result = await _manager.ProcessQuery(query);

                // Show AI explanation dialog
                XtraMessageBox.Show(result.Answer, "Câu trả lời từ AI Copilot", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (result.Data != null && result.Data.Rows.Count > 0)
                {
                    // Bind table to grid
                    gcData.DataSource = result.Data;
                    gvData.BestFitColumns();

                    // Try to plot dynamic query results
                    UpdateChartFromDataTable(result.Data);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                txtSearch.Enabled = true;
                btnSearch.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(sender, EventArgs.Empty);
            }
        }

        private void UpdateChartFromDataTable(DataTable dt)
        {
            if (dt == null || dt.Columns.Count < 2) return;

            // Find first string column for arguments
            string argCol = null;
            foreach (DataColumn col in dt.Columns)
            {
                if (col.DataType == typeof(string))
                {
                    argCol = col.ColumnName;
                    break;
                }
            }

            // Find first numeric column for values
            string valCol = null;
            foreach (DataColumn col in dt.Columns)
            {
                if (col.DataType == typeof(int) || col.DataType == typeof(decimal) || 
                    col.DataType == typeof(double) || col.DataType == typeof(float) ||
                    col.DataType == typeof(long) || col.DataType == typeof(short))
                {
                    valCol = col.ColumnName;
                    break;
                }
            }

            if (argCol != null && valCol != null)
            {
                var series = new DevExpress.XtraCharts.Series("AI Query Visualizer", DevExpress.XtraCharts.ViewType.Pie);
                series.DataSource = dt;
                series.ArgumentDataMember = argCol;
                series.ValueDataMembers.AddRange(new string[] { valCol });

                chartControl1.Series.Clear();
                chartControl1.Series.Add(series);
            }
        }

        private async void btnOpenChat_Click(object sender, EventArgs e)
        {
            var formManager = new FormManager_Functions(this.MdiParent ?? this);
            await formManager.OpenFormWithSplashScreen(typeof(FrmAI_Chat));
        }
    }
}