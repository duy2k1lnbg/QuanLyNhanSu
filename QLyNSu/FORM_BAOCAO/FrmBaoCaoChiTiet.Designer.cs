namespace QLyNSu.FORM_BAOCAO
{
    partial class FrmBaoCaoChiTiet
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.lblKyCong = new DevExpress.XtraEditors.LabelControl();
            this.cboKyCong = new DevExpress.XtraEditors.LookUpEdit();
            this.lblNhanVien = new DevExpress.XtraEditors.LabelControl();
            this.cboNhanVien = new DevExpress.XtraEditors.LookUpEdit();
            this.btnIn = new DevExpress.XtraEditors.SimpleButton();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            this.gcChiTiet = new DevExpress.XtraGrid.GridControl();
            this.gvChiTiet = new DevExpress.XtraGrid.Views.Grid.GridView();

            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboKyCong.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboNhanVien.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcChiTiet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvChiTiet)).BeginInit();
            this.SuspendLayout();

            // panelControl1
            this.panelControl1.Controls.Add(this.lblKyCong);
            this.panelControl1.Controls.Add(this.cboKyCong);
            this.panelControl1.Controls.Add(this.lblNhanVien);
            this.panelControl1.Controls.Add(this.cboNhanVien);
            this.panelControl1.Controls.Add(this.btnIn);
            this.panelControl1.Controls.Add(this.btnDong);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(950, 75);
            this.panelControl1.TabIndex = 0;

            // lblKyCong
            this.lblKyCong.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblKyCong.Appearance.Options.UseFont = true;
            this.lblKyCong.Location = new System.Drawing.Point(20, 26);
            this.lblKyCong.Name = "lblKyCong";
            this.lblKyCong.Size = new System.Drawing.Size(100, 23);
            this.lblKyCong.Text = "Chọn kỳ công:";

            // cboKyCong
            this.cboKyCong.Location = new System.Drawing.Point(130, 23);
            this.cboKyCong.Name = "cboKyCong";
            this.cboKyCong.Size = new System.Drawing.Size(150, 28);
            this.cboKyCong.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboKyCong.Properties.Appearance.Options.UseFont = true;
            this.cboKyCong.Properties.NullText = "-- Chọn Kỳ công --";
            this.cboKyCong.EditValueChanged += new System.EventHandler(this.cboKyCong_EditValueChanged);

            // lblNhanVien
            this.lblNhanVien.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblNhanVien.Appearance.Options.UseFont = true;
            this.lblNhanVien.Location = new System.Drawing.Point(300, 26);
            this.lblNhanVien.Name = "lblNhanVien";
            this.lblNhanVien.Size = new System.Drawing.Size(105, 23);
            this.lblNhanVien.Text = "Chọn nhân viên:";

            // cboNhanVien
            this.cboNhanVien.Location = new System.Drawing.Point(415, 23);
            this.cboNhanVien.Name = "cboNhanVien";
            this.cboNhanVien.Size = new System.Drawing.Size(220, 28);
            this.cboNhanVien.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboNhanVien.Properties.Appearance.Options.UseFont = true;
            this.cboNhanVien.Properties.NullText = "-- Chọn Nhân viên --";
            this.cboNhanVien.EditValueChanged += new System.EventHandler(this.cboNhanVien_EditValueChanged);

            // btnIn
            this.btnIn.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnIn.Appearance.Options.UseFont = true;
            this.btnIn.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/actions/print.svg");
            this.btnIn.Location = new System.Drawing.Point(660, 20);
            this.btnIn.Name = "btnIn";
            this.btnIn.Size = new System.Drawing.Size(130, 35);
            this.btnIn.Text = "In Báo Cáo";
            this.btnIn.Click += new System.EventHandler(this.btnIn_Click);

            // btnDong
            this.btnDong.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDong.Appearance.Options.UseFont = true;
            this.btnDong.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/actions/cancel.svg");
            this.btnDong.Location = new System.Drawing.Point(800, 20);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(120, 35);
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            // gcChiTiet
            this.gcChiTiet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcChiTiet.Location = new System.Drawing.Point(0, 75);
            this.gcChiTiet.MainView = this.gvChiTiet;
            this.gcChiTiet.Name = "gcChiTiet";
            this.gcChiTiet.Size = new System.Drawing.Size(950, 425);
            this.gcChiTiet.TabIndex = 1;
            this.gcChiTiet.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gvChiTiet });

            // gvChiTiet
            this.gvChiTiet.GridControl = this.gcChiTiet;
            this.gvChiTiet.Name = "gvChiTiet";
            this.gvChiTiet.OptionsView.ShowGroupPanel = false;

            // FrmBaoCaoChiTiet
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 500);
            this.Controls.Add(this.gcChiTiet);
            this.Controls.Add(this.panelControl1);
            this.Name = "FrmBaoCaoChiTiet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Báo Cáo Chi Tiết Lương 1 Nhân Viên";
            this.Load += new System.EventHandler(this.FrmBaoCaoChiTiet_Load);

            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboKyCong.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboNhanVien.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcChiTiet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvChiTiet)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl lblKyCong;
        private DevExpress.XtraEditors.LookUpEdit cboKyCong;
        private DevExpress.XtraEditors.LabelControl lblNhanVien;
        private DevExpress.XtraEditors.LookUpEdit cboNhanVien;
        private DevExpress.XtraEditors.SimpleButton btnIn;
        private DevExpress.XtraEditors.SimpleButton btnDong;
        private DevExpress.XtraGrid.GridControl gcChiTiet;
        private DevExpress.XtraGrid.Views.Grid.GridView gvChiTiet;
    }
}