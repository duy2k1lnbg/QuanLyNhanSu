namespace QLyNSu.FORM_BAOCAO
{
    partial class FrmDashboardLuong
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
            this.tblLayoutLuong = new System.Windows.Forms.TableLayoutPanel();
            this.chartLuong = new DevExpress.XtraCharts.ChartControl();
            this.chartLuongBinhQuan = new DevExpress.XtraCharts.ChartControl();
            this.chartTangCaTrend = new DevExpress.XtraCharts.ChartControl();
            ((System.ComponentModel.ISupportInitialize)(this.chartLuong)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartLuongBinhQuan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTangCaTrend)).BeginInit();
            this.SuspendLayout();
            // 
            // tblLayoutLuong
            // 
            this.tblLayoutLuong.ColumnCount = 2;
            this.tblLayoutLuong.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutLuong.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutLuong.Controls.Add(this.chartLuong, 0, 0);
            this.tblLayoutLuong.Controls.Add(this.chartLuongBinhQuan, 1, 0);
            this.tblLayoutLuong.Controls.Add(this.chartTangCaTrend, 0, 1);
            this.tblLayoutLuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayoutLuong.Location = new System.Drawing.Point(0, 0);
            this.tblLayoutLuong.Name = "tblLayoutLuong";
            this.tblLayoutLuong.RowCount = 2;
            this.tblLayoutLuong.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutLuong.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutLuong.Size = new System.Drawing.Size(1200, 700);
            this.tblLayoutLuong.TabIndex = 0;
            this.tblLayoutLuong.SetColumnSpan(this.chartTangCaTrend, 2);
            // 
            // chartLuong
            // 
            this.chartLuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartLuong.Location = new System.Drawing.Point(3, 3);
            this.chartLuong.Name = "chartLuong";
            this.chartLuong.Size = new System.Drawing.Size(594, 344);
            this.chartLuong.TabIndex = 0;
            // 
            // chartLuongBinhQuan
            // 
            this.chartLuongBinhQuan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartLuongBinhQuan.Location = new System.Drawing.Point(603, 3);
            this.chartLuongBinhQuan.Name = "chartLuongBinhQuan";
            this.chartLuongBinhQuan.Size = new System.Drawing.Size(594, 344);
            this.chartLuongBinhQuan.TabIndex = 1;
            // 
            // chartTangCaTrend
            // 
            this.chartTangCaTrend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTangCaTrend.Location = new System.Drawing.Point(3, 353);
            this.chartTangCaTrend.Name = "chartTangCaTrend";
            this.chartTangCaTrend.Size = new System.Drawing.Size(1194, 344);
            this.chartTangCaTrend.TabIndex = 2;
            // 
            // FrmDashboardLuong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tblLayoutLuong);
            this.Name = "FrmDashboardLuong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard Phân Tích Lương & Chi Phí";
            this.Shown += new System.EventHandler(this.FrmDashboardLuong_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.chartLuong)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartLuongBinhQuan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTangCaTrend)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblLayoutLuong;
        private DevExpress.XtraCharts.ChartControl chartLuong;
        private DevExpress.XtraCharts.ChartControl chartLuongBinhQuan;
        private DevExpress.XtraCharts.ChartControl chartTangCaTrend;
    }
}
