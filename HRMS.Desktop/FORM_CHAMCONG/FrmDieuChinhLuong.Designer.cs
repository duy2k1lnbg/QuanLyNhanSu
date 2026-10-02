namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmDieuChinhLuong
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
            this.components = new System.ComponentModel.Container();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.btnLuuDieuChinh = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.panelHeader = new DevExpress.XtraEditors.PanelControl();
            this.txtChungTu = new DevExpress.XtraEditors.TextEdit();
            this.lblChungTu = new DevExpress.XtraEditors.LabelControl();
            this.txtLyDo = new DevExpress.XtraEditors.TextEdit();
            this.lblLyDo = new DevExpress.XtraEditors.LabelControl();
            this.lblThongTin = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.gcDieuChinh = new DevExpress.XtraGrid.GridControl();
            this.gvDieuChinh = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colKhoanMuc = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colGiaTriCu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colGiaTriMoi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colChenhLech = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colGhiChu = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).BeginInit();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtChungTu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLyDo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcDieuChinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDieuChinh)).BeginInit();
            this.SuspendLayout();
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btnLuuDieuChinh,
            this.btnDong});
            this.barManager1.MaxItemId = 2;
            // 
            // bar1
            // 
            this.bar1.BarAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F);
            this.bar1.BarAppearance.Normal.Options.UseFont = true;
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btnLuuDieuChinh),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDong)});
            this.bar1.Text = "Tools";
            // 
            // btnLuuDieuChinh
            // 
            this.btnLuuDieuChinh.Caption = "Lưu Điều Chỉnh";
            this.btnLuuDieuChinh.Id = 0;
            this.btnLuuDieuChinh.Name = "btnLuuDieuChinh";
            this.btnLuuDieuChinh.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLuuDieuChinh_ItemClick);
            // 
            // btnDong
            // 
            this.btnDong.Caption = "Đóng";
            this.btnDong.Id = 1;
            this.btnDong.Name = "btnDong";
            this.btnDong.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDong_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(850, 29);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 520);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(850, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 29);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 491);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(850, 29);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 491);
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.txtChungTu);
            this.panelHeader.Controls.Add(this.lblChungTu);
            this.panelHeader.Controls.Add(this.txtLyDo);
            this.panelHeader.Controls.Add(this.lblLyDo);
            this.panelHeader.Controls.Add(this.lblThongTin);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 29);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(850, 120);
            this.panelHeader.TabIndex = 4;
            // 
            // txtChungTu
            // 
            this.txtChungTu.Location = new System.Drawing.Point(540, 78);
            this.txtChungTu.Name = "txtChungTu";
            this.txtChungTu.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtChungTu.Properties.Appearance.Options.UseFont = true;
            this.txtChungTu.Size = new System.Drawing.Size(280, 22);
            this.txtChungTu.TabIndex = 5;
            // 
            // lblChungTu
            // 
            this.lblChungTu.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblChungTu.Appearance.Options.UseFont = true;
            this.lblChungTu.Location = new System.Drawing.Point(440, 81);
            this.lblChungTu.Name = "lblChungTu";
            this.lblChungTu.Size = new System.Drawing.Size(94, 16);
            this.lblChungTu.TabIndex = 4;
            this.lblChungTu.Text = "Số quyết định/CT:";
            // 
            // txtLyDo
            // 
            this.txtLyDo.Location = new System.Drawing.Point(120, 78);
            this.txtLyDo.Name = "txtLyDo";
            this.txtLyDo.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtLyDo.Properties.Appearance.Options.UseFont = true;
            this.txtLyDo.Size = new System.Drawing.Size(300, 22);
            this.txtLyDo.TabIndex = 3;
            // 
            // lblLyDo
            // 
            this.lblLyDo.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblLyDo.Appearance.Options.UseFont = true;
            this.lblLyDo.Location = new System.Drawing.Point(16, 81);
            this.lblLyDo.Name = "lblLyDo";
            this.lblLyDo.Size = new System.Drawing.Size(95, 16);
            this.lblLyDo.TabIndex = 2;
            this.lblLyDo.Text = "Lý do điều chỉnh:";
            // 
            // lblThongTin
            // 
            this.lblThongTin.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblThongTin.Appearance.Options.UseFont = true;
            this.lblThongTin.Location = new System.Drawing.Point(16, 45);
            this.lblThongTin.Name = "lblThongTin";
            this.lblThongTin.Size = new System.Drawing.Size(262, 16);
            this.lblThongTin.TabIndex = 1;
            this.lblThongTin.Text = "Mã NV: --- | Họ tên: --- | Kỳ công: ---";
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(325, 19);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ĐIỀU CHỈNH KHOẢN MỤC BẢNG LƯƠNG";
            // 
            // gcDieuChinh
            // 
            this.gcDieuChinh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcDieuChinh.Location = new System.Drawing.Point(0, 149);
            this.gcDieuChinh.MainView = this.gvDieuChinh;
            this.gcDieuChinh.MenuManager = this.barManager1;
            this.gcDieuChinh.Name = "gcDieuChinh";
            this.gcDieuChinh.Size = new System.Drawing.Size(850, 371);
            this.gcDieuChinh.TabIndex = 5;
            this.gcDieuChinh.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvDieuChinh});
            // 
            // gvDieuChinh
            // 
            this.gvDieuChinh.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colKhoanMuc,
            this.colGiaTriCu,
            this.colGiaTriMoi,
            this.colChenhLech,
            this.colGhiChu});
            this.gvDieuChinh.GridControl = this.gcDieuChinh;
            this.gvDieuChinh.Name = "gvDieuChinh";
            this.gvDieuChinh.OptionsView.ShowGroupPanel = false;
            // 
            // colKhoanMuc
            // 
            this.colKhoanMuc.Caption = "Khoản mục lương";
            this.colKhoanMuc.FieldName = "KhoanMuc";
            this.colKhoanMuc.Name = "colKhoanMuc";
            this.colKhoanMuc.OptionsColumn.AllowEdit = false;
            this.colKhoanMuc.Visible = true;
            this.colKhoanMuc.VisibleIndex = 0;
            this.colKhoanMuc.Width = 200;
            // 
            // colGiaTriCu
            // 
            this.colGiaTriCu.Caption = "Số tiền gốc (đ)";
            this.colGiaTriCu.DisplayFormat.FormatString = "{0:n0}";
            this.colGiaTriCu.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colGiaTriCu.FieldName = "GiaTriCu";
            this.colGiaTriCu.Name = "colGiaTriCu";
            this.colGiaTriCu.OptionsColumn.AllowEdit = false;
            this.colGiaTriCu.Visible = true;
            this.colGiaTriCu.VisibleIndex = 1;
            this.colGiaTriCu.Width = 140;
            // 
            // colGiaTriMoi
            // 
            this.colGiaTriMoi.Caption = "Số tiền điều chỉnh (đ)";
            this.colGiaTriMoi.DisplayFormat.FormatString = "{0:n0}";
            this.colGiaTriMoi.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colGiaTriMoi.FieldName = "GiaTriMoi";
            this.colGiaTriMoi.Name = "colGiaTriMoi";
            this.colGiaTriMoi.Visible = true;
            this.colGiaTriMoi.VisibleIndex = 2;
            this.colGiaTriMoi.Width = 140;
            // 
            // colChenhLech
            // 
            this.colChenhLech.Caption = "Chênh lệch (+/-)";
            this.colChenhLech.DisplayFormat.FormatString = "{0:n0}";
            this.colChenhLech.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colChenhLech.FieldName = "ChenhLech";
            this.colChenhLech.Name = "colChenhLech";
            this.colChenhLech.OptionsColumn.AllowEdit = false;
            this.colChenhLech.Visible = true;
            this.colChenhLech.VisibleIndex = 3;
            this.colChenhLech.Width = 130;
            // 
            // colGhiChu
            // 
            this.colGhiChu.Caption = "Căn cứ điều chỉnh";
            this.colGhiChu.FieldName = "GhiChu";
            this.colGhiChu.Name = "colGhiChu";
            this.colGhiChu.Visible = true;
            this.colGhiChu.VisibleIndex = 4;
            this.colGhiChu.Width = 220;
            // 
            // FrmDieuChinhLuong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 520);
            this.Controls.Add(this.gcDieuChinh);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmDieuChinhLuong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hộp Thoại Điều Chỉnh Lương";
            this.Load += new System.EventHandler(this.FrmDieuChinhLuong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).EndInit();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtChungTu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLyDo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcDieuChinh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDieuChinh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnLuuDieuChinh;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.PanelControl panelHeader;
        private DevExpress.XtraEditors.TextEdit txtChungTu;
        private DevExpress.XtraEditors.LabelControl lblChungTu;
        private DevExpress.XtraEditors.TextEdit txtLyDo;
        private DevExpress.XtraEditors.LabelControl lblLyDo;
        private DevExpress.XtraEditors.LabelControl lblThongTin;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraGrid.GridControl gcDieuChinh;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDieuChinh;
        private DevExpress.XtraGrid.Columns.GridColumn colKhoanMuc;
        private DevExpress.XtraGrid.Columns.GridColumn colGiaTriCu;
        private DevExpress.XtraGrid.Columns.GridColumn colGiaTriMoi;
        private DevExpress.XtraGrid.Columns.GridColumn colChenhLech;
        private DevExpress.XtraGrid.Columns.GridColumn colGhiChu;
    }
}
