namespace QLyNSu.FORM_BAOCAO
{
    partial class FrmBaoCaoTongHop
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new DevExpress.XtraTab.XtraTabControl();
            this.tabBaoCao = new DevExpress.XtraTab.XtraTabPage();
            this.splitContainer = new DevExpress.XtraEditors.SplitContainerControl();
            this.lstBaoCao = new DevExpress.XtraEditors.ListBoxControl();
            this.pnlFilters = new DevExpress.XtraEditors.PanelControl();
            this.lblKyCong = new DevExpress.XtraEditors.LabelControl();
            this.cboKyCong = new DevExpress.XtraEditors.LookUpEdit();
            this.lblNhanVien = new DevExpress.XtraEditors.LabelControl();
            this.cboNhanVien = new DevExpress.XtraEditors.LookUpEdit();
            this.lblMonth = new DevExpress.XtraEditors.LabelControl();
            this.cboMonth = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblDays = new DevExpress.XtraEditors.LabelControl();
            this.spinDays = new DevExpress.XtraEditors.SpinEdit();
            this.btnXem = new DevExpress.XtraEditors.SimpleButton();
            this.btnIn = new DevExpress.XtraEditors.SimpleButton();
            this.btnExcel = new DevExpress.XtraEditors.SimpleButton();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            this.gcData = new DevExpress.XtraGrid.GridControl();
            this.gvData = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.tabDashboard = new DevExpress.XtraTab.XtraTabPage();
            this.splitDashboard = new DevExpress.XtraEditors.SplitContainerControl();
            this.chartPhongBan = new DevExpress.XtraCharts.ChartControl();
            this.chartLuong = new DevExpress.XtraCharts.ChartControl();

            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabBaoCao.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstBaoCao)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFilters)).BeginInit();
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboKyCong.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboNhanVien.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMonth.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinDays.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvData)).BeginInit();
            this.tabDashboard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDashboard)).BeginInit();
            this.splitDashboard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhongBan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartLuong)).BeginInit();
            this.SuspendLayout();

            // tabControl
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedTabPage = this.tabBaoCao;
            this.tabControl.Size = new System.Drawing.Size(1200, 700);
            this.tabControl.TabIndex = 0;
            this.tabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabBaoCao,
            this.tabDashboard});
            this.tabControl.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.tabControl_SelectedPageChanged);

            // tabBaoCao
            this.tabBaoCao.Controls.Add(this.splitContainer);
            this.tabBaoCao.Name = "tabBaoCao";
            this.tabBaoCao.Size = new System.Drawing.Size(1198, 665);
            this.tabBaoCao.Text = "Báo Cáo & Thống Kê";

            // splitContainer
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Panel1.Controls.Add(this.lstBaoCao);
            this.splitContainer.Panel1.Text = "ListBaoCaoPanel";
            this.splitContainer.Panel2.Controls.Add(this.gcData);
            this.splitContainer.Panel2.Controls.Add(this.pnlFilters);
            this.splitContainer.Panel2.Text = "DataPanel";
            this.splitContainer.SplitterPosition = 280;
            this.splitContainer.Size = new System.Drawing.Size(1198, 665);
            this.splitContainer.TabIndex = 0;

            // lstBaoCao
            this.lstBaoCao.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lstBaoCao.Appearance.Options.UseFont = true;
            this.lstBaoCao.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstBaoCao.ItemHeight = 40;
            this.lstBaoCao.Items.AddRange(new object[] {
            "Báo cáo Hợp đồng sắp hết hạn",
            "Danh sách Sinh nhật nhân viên",
            "Báo cáo Tổng hợp giờ tăng ca",
            "Phiếu chi tiết lương nhân viên"});
            this.lstBaoCao.Location = new System.Drawing.Point(0, 0);
            this.lstBaoCao.Name = "lstBaoCao";
            this.lstBaoCao.Size = new System.Drawing.Size(280, 665);
            this.lstBaoCao.TabIndex = 0;
            this.lstBaoCao.SelectedIndexChanged += new System.EventHandler(this.lstBaoCao_SelectedIndexChanged);

            // pnlFilters
            this.pnlFilters.Controls.Add(this.lblKyCong);
            this.pnlFilters.Controls.Add(this.cboKyCong);
            this.pnlFilters.Controls.Add(this.lblNhanVien);
            this.pnlFilters.Controls.Add(this.cboNhanVien);
            this.pnlFilters.Controls.Add(this.lblMonth);
            this.pnlFilters.Controls.Add(this.cboMonth);
            this.pnlFilters.Controls.Add(this.lblDays);
            this.pnlFilters.Controls.Add(this.spinDays);
            this.pnlFilters.Controls.Add(this.btnXem);
            this.pnlFilters.Controls.Add(this.btnIn);
            this.pnlFilters.Controls.Add(this.btnExcel);
            this.pnlFilters.Controls.Add(this.btnDong);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(908, 90);
            this.pnlFilters.TabIndex = 0;

            // lblKyCong
            this.lblKyCong.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblKyCong.Appearance.Options.UseFont = true;
            this.lblKyCong.Location = new System.Drawing.Point(20, 18);
            this.lblKyCong.Name = "lblKyCong";
            this.lblKyCong.Size = new System.Drawing.Size(55, 21);
            this.lblKyCong.Text = "Kỳ công:";

            // cboKyCong
            this.cboKyCong.Location = new System.Drawing.Point(90, 15);
            this.cboKyCong.Name = "cboKyCong";
            this.cboKyCong.Size = new System.Drawing.Size(120, 28);
            this.cboKyCong.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboKyCong.Properties.Appearance.Options.UseFont = true;
            this.cboKyCong.Properties.NullText = "-- Chọn Kỳ công --";
            this.cboKyCong.EditValueChanged += new System.EventHandler(this.Filter_ValueChanged);

            // lblNhanVien
            this.lblNhanVien.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNhanVien.Appearance.Options.UseFont = true;
            this.lblNhanVien.Location = new System.Drawing.Point(230, 18);
            this.lblNhanVien.Name = "lblNhanVien";
            this.lblNhanVien.Size = new System.Drawing.Size(73, 21);
            this.lblNhanVien.Text = "Nhân viên:";

            // cboNhanVien
            this.cboNhanVien.Location = new System.Drawing.Point(315, 15);
            this.cboNhanVien.Name = "cboNhanVien";
            this.cboNhanVien.Size = new System.Drawing.Size(180, 28);
            this.cboNhanVien.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboNhanVien.Properties.Appearance.Options.UseFont = true;
            this.cboNhanVien.Properties.NullText = "-- Chọn Nhân viên --";
            this.cboNhanVien.EditValueChanged += new System.EventHandler(this.Filter_ValueChanged);

            // lblMonth
            this.lblMonth.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblMonth.Appearance.Options.UseFont = true;
            this.lblMonth.Location = new System.Drawing.Point(20, 53);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(44, 21);
            this.lblMonth.Text = "Tháng:";

            // cboMonth
            this.cboMonth.Location = new System.Drawing.Point(90, 50);
            this.cboMonth.Name = "cboMonth";
            this.cboMonth.Size = new System.Drawing.Size(120, 28);
            this.cboMonth.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboMonth.Properties.Appearance.Options.UseFont = true;
            this.cboMonth.Properties.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });
            this.cboMonth.EditValueChanged += new System.EventHandler(this.Filter_ValueChanged);

            // lblDays
            this.lblDays.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDays.Appearance.Options.UseFont = true;
            this.lblDays.Location = new System.Drawing.Point(230, 53);
            this.lblDays.Name = "lblDays";
            this.lblDays.Size = new System.Drawing.Size(56, 21);
            this.lblDays.Text = "Số ngày:";

            // spinDays
            this.spinDays.EditValue = new decimal(new int[] { 30, 0, 0, 0 });
            this.spinDays.Location = new System.Drawing.Point(315, 50);
            this.spinDays.Name = "spinDays";
            this.spinDays.Size = new System.Drawing.Size(100, 28);
            this.spinDays.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.spinDays.Properties.Appearance.Options.UseFont = true;
            this.spinDays.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinDays.Properties.MaxValue = new decimal(new int[] { 365, 0, 0, 0 });
            this.spinDays.Properties.MinValue = new decimal(new int[] { 1, 0, 0, 0 });
            this.spinDays.EditValueChanged += new System.EventHandler(this.Filter_ValueChanged);

            // btnXem
            this.btnXem.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnXem.Appearance.Options.UseFont = true;
            this.btnXem.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/actions/show.svg");
            this.btnXem.Location = new System.Drawing.Point(520, 12);
            this.btnXem.Name = "btnXem";
            this.btnXem.Size = new System.Drawing.Size(140, 30);
            this.btnXem.Text = "XEM BÁO CÁO";
            this.btnXem.Click += new System.EventHandler(this.btnXem_Click);

            // btnIn
            this.btnIn.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnIn.Appearance.Options.UseFont = true;
            this.btnIn.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/actions/print.svg");
            this.btnIn.Location = new System.Drawing.Point(670, 12);
            this.btnIn.Name = "btnIn";
            this.btnIn.Size = new System.Drawing.Size(120, 30);
            this.btnIn.Text = "IN BÁO CÁO";
            this.btnIn.Click += new System.EventHandler(this.btnIn_Click);

            // btnExcel
            this.btnExcel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnExcel.Appearance.Options.UseFont = true;
            this.btnExcel.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/export/exporttoxlsx.svg");
            this.btnExcel.Location = new System.Drawing.Point(520, 48);
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.Size = new System.Drawing.Size(140, 30);
            this.btnExcel.Text = "XUẤT EXCEL";
            this.btnExcel.Click += new System.EventHandler(this.btnExcel_Click);

            // btnDong
            this.btnDong.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDong.Appearance.Options.UseFont = true;
            this.btnDong.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/actions/cancel.svg");
            this.btnDong.Location = new System.Drawing.Point(670, 48);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(120, 30);
            this.btnDong.Text = "ĐÓNG";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            // gcData
            this.gcData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcData.Location = new System.Drawing.Point(0, 90);
            this.gcData.MainView = this.gvData;
            this.gcData.Name = "gcData";
            this.gcData.Size = new System.Drawing.Size(908, 575);
            this.gcData.TabIndex = 1;
            this.gcData.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvData});

            // gvData
            this.gvData.GridControl = this.gcData;
            this.gvData.Name = "gvData";
            this.gvData.OptionsView.ShowGroupPanel = false;

            // tabDashboard
            this.tabDashboard.Controls.Add(this.splitDashboard);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Size = new System.Drawing.Size(1198, 665);
            this.tabDashboard.Text = "Dashboard Phân Tích";

            // splitDashboard
            this.splitDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDashboard.Horizontal = false;
            this.splitDashboard.Location = new System.Drawing.Point(0, 0);
            this.splitDashboard.Name = "splitDashboard";
            this.splitDashboard.Panel1.Controls.Add(this.chartPhongBan);
            this.splitDashboard.Panel1.Text = "PanelChartPB";
            this.splitDashboard.Panel2.Controls.Add(this.chartLuong);
            this.splitDashboard.Panel2.Text = "PanelChartLuong";
            this.splitDashboard.SplitterPosition = 320;
            this.splitDashboard.Size = new System.Drawing.Size(1198, 665);
            this.splitDashboard.TabIndex = 0;

            // chartPhongBan
            this.chartPhongBan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartPhongBan.Location = new System.Drawing.Point(0, 0);
            this.chartPhongBan.Name = "chartPhongBan";
            this.chartPhongBan.Size = new System.Drawing.Size(1198, 320);
            this.chartPhongBan.TabIndex = 0;

            // chartLuong
            this.chartLuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartLuong.Location = new System.Drawing.Point(0, 0);
            this.chartLuong.Name = "chartLuong";
            this.chartLuong.Size = new System.Drawing.Size(1198, 335);
            this.chartLuong.TabIndex = 0;

            // FrmBaoCaoTongHop
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tabControl);
            this.Name = "FrmBaoCaoTongHop";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trung Tâm Báo Cáo & Phân Tích Nhân Sự";
            this.Load += new System.EventHandler(this.FrmBaoCaoTongHop_Load);

            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabBaoCao.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lstBaoCao)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFilters)).EndInit();
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboKyCong.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboNhanVien.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMonth.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinDays.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvData)).EndInit();
            this.tabDashboard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDashboard)).EndInit();
            this.splitDashboard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartPhongBan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartLuong)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl tabControl;
        private DevExpress.XtraTab.XtraTabPage tabBaoCao;
        private DevExpress.XtraTab.XtraTabPage tabDashboard;
        private DevExpress.XtraEditors.SplitContainerControl splitContainer;
        private DevExpress.XtraEditors.ListBoxControl lstBaoCao;
        private DevExpress.XtraEditors.PanelControl pnlFilters;
        private DevExpress.XtraGrid.GridControl gcData;
        private DevExpress.XtraGrid.Views.Grid.GridView gvData;

        private DevExpress.XtraEditors.LabelControl lblKyCong;
        private DevExpress.XtraEditors.LookUpEdit cboKyCong;
        private DevExpress.XtraEditors.LabelControl lblNhanVien;
        private DevExpress.XtraEditors.LookUpEdit cboNhanVien;
        private DevExpress.XtraEditors.LabelControl lblMonth;
        private DevExpress.XtraEditors.ComboBoxEdit cboMonth;
        private DevExpress.XtraEditors.LabelControl lblDays;
        private DevExpress.XtraEditors.SpinEdit spinDays;

        private DevExpress.XtraEditors.SimpleButton btnXem;
        private DevExpress.XtraEditors.SimpleButton btnIn;
        private DevExpress.XtraEditors.SimpleButton btnExcel;
        private DevExpress.XtraEditors.SimpleButton btnDong;

        private DevExpress.XtraEditors.SplitContainerControl splitDashboard;
        private DevExpress.XtraCharts.ChartControl chartPhongBan;
        private DevExpress.XtraCharts.ChartControl chartLuong;
    }
}