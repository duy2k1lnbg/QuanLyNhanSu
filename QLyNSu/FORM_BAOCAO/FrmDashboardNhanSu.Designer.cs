namespace QLyNSu.FORM_BAOCAO
{
    partial class FrmDashboardNhanSu
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
            this.tblLayoutNhanSu = new System.Windows.Forms.TableLayoutPanel();
            this.chartPhongBan = new DevExpress.XtraCharts.ChartControl();
            this.chartGioiTinh = new DevExpress.XtraCharts.ChartControl();
            this.chartTrinhDo = new DevExpress.XtraCharts.ChartControl();
            this.chartTuoi = new DevExpress.XtraCharts.ChartControl();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhongBan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartGioiTinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTrinhDo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTuoi)).BeginInit();
            this.SuspendLayout();
            // 
            // tblLayoutNhanSu
            // 
            this.tblLayoutNhanSu.ColumnCount = 2;
            this.tblLayoutNhanSu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutNhanSu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutNhanSu.Controls.Add(this.chartPhongBan, 0, 0);
            this.tblLayoutNhanSu.Controls.Add(this.chartGioiTinh, 1, 0);
            this.tblLayoutNhanSu.Controls.Add(this.chartTrinhDo, 0, 1);
            this.tblLayoutNhanSu.Controls.Add(this.chartTuoi, 1, 1);
            this.tblLayoutNhanSu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayoutNhanSu.Location = new System.Drawing.Point(0, 0);
            this.tblLayoutNhanSu.Name = "tblLayoutNhanSu";
            this.tblLayoutNhanSu.RowCount = 2;
            this.tblLayoutNhanSu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutNhanSu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutNhanSu.Size = new System.Drawing.Size(1200, 700);
            this.tblLayoutNhanSu.TabIndex = 0;
            // 
            // chartPhongBan
            // 
            this.chartPhongBan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartPhongBan.Location = new System.Drawing.Point(3, 3);
            this.chartPhongBan.Name = "chartPhongBan";
            this.chartPhongBan.Size = new System.Drawing.Size(594, 344);
            this.chartPhongBan.TabIndex = 0;
            // 
            // chartGioiTinh
            // 
            this.chartGioiTinh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartGioiTinh.Location = new System.Drawing.Point(603, 3);
            this.chartGioiTinh.Name = "chartGioiTinh";
            this.chartGioiTinh.Size = new System.Drawing.Size(594, 344);
            this.chartGioiTinh.TabIndex = 1;
            // 
            // chartTrinhDo
            // 
            this.chartTrinhDo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTrinhDo.Location = new System.Drawing.Point(3, 353);
            this.chartTrinhDo.Name = "chartTrinhDo";
            this.chartTrinhDo.Size = new System.Drawing.Size(594, 344);
            this.chartTrinhDo.TabIndex = 2;
            // 
            // chartTuoi
            // 
            this.chartTuoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTuoi.Location = new System.Drawing.Point(603, 353);
            this.chartTuoi.Name = "chartTuoi";
            this.chartTuoi.Size = new System.Drawing.Size(594, 344);
            this.chartTuoi.TabIndex = 3;
            // 
            // FrmDashboardNhanSu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tblLayoutNhanSu);
            this.Name = "FrmDashboardNhanSu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard Phân Tích Nhân Sự & Cơ Cấu";
            this.Shown += new System.EventHandler(this.FrmDashboardNhanSu_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.chartPhongBan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartGioiTinh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTrinhDo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTuoi)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblLayoutNhanSu;
        private DevExpress.XtraCharts.ChartControl chartPhongBan;
        private DevExpress.XtraCharts.ChartControl chartGioiTinh;
        private DevExpress.XtraCharts.ChartControl chartTrinhDo;
        private DevExpress.XtraCharts.ChartControl chartTuoi;
    }
}
