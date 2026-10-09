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
            _manager = new ChatboxManager(
                conversationId: null,
                executionService: null,
                remoteChatHandler: async (q, cid, opt, ver, clid, ct) =>
                {
                    var res = await AiApiClient.Instance.SendChatAsync(q, cid, opt, ver, clid, null, ct);
                    return new Bu.Services.AI_Services.Core.AiChatExecutionResult
                    {
                        Status = res.Status,
                        Answer = res.Answer,
                        ErrorCode = res.ErrorCode,
                        ConversationId = res.ConversationId,
                        ConversationVersion = res.ConversationVersion,
                        Clarification = res.Clarification,
                        InterpretedRequest = res.InterpretedRequest,
                        Data = res.Data,
                        BypassedLlm = false
                    };
                },
                remoteResetHandler: async (cid, ct) =>
                {
                    return await AiApiClient.Instance.ResetConversationAsync(cid, ct);
                }
            );
            Bu.CLASS_SYSTEM.UserSession.SessionCleared += OnSessionCleared;
            FormClosed += (s,e) => 
            { 
                Bu.CLASS_SYSTEM.UserSession.SessionCleared -= OnSessionCleared;
                _manager.Reset(); 
            };
        }

        private void OnSessionCleared()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(OnSessionCleared));
                return;
            }
            if (!IsDisposed)
            {
                _manager.Reset();
                gcData.DataSource = null;
                chartControl1.Series.Clear();
                lblKpiVal1.Text = "—";
            }
        }


        private void FrmAI_Load(object sender, EventArgs e)
        {
            // Apply DevExpress grid styling
            FormManager_Functions.CustomView_Colums(gvData);

            // Load KPIs and Charts from Database
            LoadDashboardStats();
        }

        private async void LoadDashboardStats()
        {
            lblKpiVal1.Text="—"; lblKpiVal2.Text="—"; lblKpiVal3.Text="—"; lblKpiVal4.Text="—";
            gcData.DataSource=null; chartControl1.Series.Clear();
            if (!Bu.CLASS_SYSTEM.UserSession.IsLoggedIn) return;
            int actor=(int)Bu.CLASS_SYSTEM.UserSession.CurrentUser.IDUSER;
            try
            {
                var dashboard = await AiApiClient.Instance.GetDashboardAsync();
                if (IsDisposed || !Bu.CLASS_SYSTEM.UserSession.IsLoggedIn || Bu.CLASS_SYSTEM.UserSession.CurrentUser.IDUSER != actor) return;
                if (dashboard != null)
                {
                    if (dashboard.CountStatus == "ok" && dashboard.HasCountData)
                    {
                        lblKpiVal1.Text = dashboard.TotalEmployees.ToString("N0");
                    }
                    else if (dashboard.CountStatus == "forbidden")
                    {
                        lblKpiVal1.Text = "Không có quyền";
                    }
                    else
                    {
                        lblKpiVal1.Text = "—";
                    }
                    if (dashboard.Data != null && dashboard.Data.Rows.Count > 0)
                    {
                        gcData.DataSource = dashboard.Data;
                        gvData.BestFitColumns();
                        UpdateChartFromDataTable(dashboard.Data);
                    }
                }
            }
            catch { /* Backend unavailability leaves dashboard empty */ }
        }
        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            gcData.DataSource = null; chartControl1.Series.Clear();
            txtSearch.Enabled = false;
            btnSearch.Enabled = false;
            
            // Cursor wait
            this.Cursor = Cursors.WaitCursor;

            try
            {
                var result = await _manager.ProcessQuery(query);
                if (IsDisposed || Disposing) return;

                while (result.Status=="needs_clarification" && result.Clarification!=null)
                {
                    using (var dialog=new XtraForm { Text=result.Clarification.Question,Width=540,Height=240,StartPosition=FormStartPosition.CenterParent })
                    {
                        var panel=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown };
                        dialog.Controls.Add(panel); string selected=null;
                        foreach (var option in result.Clarification.Options) { var value=option.Token; var button=new SimpleButton { Text=option.Label,AutoSize=true }; button.Click+=(s,args)=>{selected=value;dialog.DialogResult=DialogResult.OK;dialog.Close();}; panel.Controls.Add(button); }
                        if (result.Clarification.Options.Count==0) { var input=new TextBox { Width=460 }; var send=new SimpleButton { Text="Gửi bổ sung" }; send.Click+=(s,args)=>{selected=input.Text;dialog.DialogResult=DialogResult.OK;dialog.Close();}; panel.Controls.Add(input);panel.Controls.Add(send); }
                        if (dialog.ShowDialog(this)!=DialogResult.OK || string.IsNullOrWhiteSpace(selected)) break;
                        result=result.Clarification.Options.Count>0 ? await _manager.ProcessClarificationAsync(selected) : await _manager.ProcessQuery(selected);
                    }
                }
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
            catch (Exception)
            {
                if (!IsDisposed && !Disposing) XtraMessageBox.Show("Không thể hoàn tất yêu cầu. Vui lòng thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed && !Disposing) {
                txtSearch.Enabled = true;
                btnSearch.Enabled = true;
                this.Cursor = Cursors.Default;
                }
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