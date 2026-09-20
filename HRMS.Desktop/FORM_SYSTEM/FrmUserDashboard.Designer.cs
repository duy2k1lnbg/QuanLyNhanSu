namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmUserDashboard
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
            this.btnRefresh = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colIDUSER = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colUSERNAME = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colFULLNAME = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTRANGTHAI_HOATDONG = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colIP_ADDRESS = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMAC_ADDRESS = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTEN_MAY_TINH = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTHOIGIAN_DANGNHAP = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTRANGTHAI_DANGNHAP = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTRANGTHAI_ONLINE = new DevExpress.XtraGrid.Columns.GridColumn();
            
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
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
            this.btnRefresh});
            this.barManager1.MaxItemId = 1;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnRefresh, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Caption = "Làm Mới (Refresh)";
            this.btnRefresh.Id = 0;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnRefresh_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1200, 24);
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 24);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.MenuManager = this.barManager1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1200, 676);
            this.gridControl1.TabIndex = 4;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colIDUSER,
            this.colUSERNAME,
            this.colFULLNAME,
            this.colTRANGTHAI_HOATDONG,
            this.colIP_ADDRESS,
            this.colMAC_ADDRESS,
            this.colTEN_MAY_TINH,
            this.colTHOIGIAN_DANGNHAP,
            this.colTRANGTHAI_DANGNHAP,
            this.colTRANGTHAI_ONLINE});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            this.gridView1.OptionsView.ShowGroupPanel = true;
            // 
            // colIDUSER
            // 
            this.colIDUSER.Caption = "ID";
            this.colIDUSER.FieldName = "IDUSER";
            this.colIDUSER.Name = "colIDUSER";
            this.colIDUSER.Visible = true;
            this.colIDUSER.VisibleIndex = 0;
            // 
            // colUSERNAME
            // 
            this.colUSERNAME.Caption = "Tên Đăng Nhập";
            this.colUSERNAME.FieldName = "USERNAME";
            this.colUSERNAME.Name = "colUSERNAME";
            this.colUSERNAME.Visible = true;
            this.colUSERNAME.VisibleIndex = 1;
            // 
            // colFULLNAME
            // 
            this.colFULLNAME.Caption = "Họ Tên";
            this.colFULLNAME.FieldName = "FULLNAME";
            this.colFULLNAME.Name = "colFULLNAME";
            this.colFULLNAME.Visible = true;
            this.colFULLNAME.VisibleIndex = 2;
            // 
            // colTRANGTHAI_HOATDONG
            // 
            this.colTRANGTHAI_HOATDONG.Caption = "Trạng Thái Tài Khoản";
            this.colTRANGTHAI_HOATDONG.FieldName = "TRANGTHAI_HOATDONG";
            this.colTRANGTHAI_HOATDONG.Name = "colTRANGTHAI_HOATDONG";
            this.colTRANGTHAI_HOATDONG.Visible = true;
            this.colTRANGTHAI_HOATDONG.VisibleIndex = 3;
            // 
            // colIP_ADDRESS
            // 
            this.colIP_ADDRESS.Caption = "IP Máy (Lần Cuối)";
            this.colIP_ADDRESS.FieldName = "IP_ADDRESS";
            this.colIP_ADDRESS.Name = "colIP_ADDRESS";
            this.colIP_ADDRESS.Visible = true;
            this.colIP_ADDRESS.VisibleIndex = 4;
            // 
            // colMAC_ADDRESS
            // 
            this.colMAC_ADDRESS.Caption = "MAC Address (Lần Cuối)";
            this.colMAC_ADDRESS.FieldName = "MAC_ADDRESS";
            this.colMAC_ADDRESS.Name = "colMAC_ADDRESS";
            this.colMAC_ADDRESS.Visible = true;
            this.colMAC_ADDRESS.VisibleIndex = 5;
            // 
            // colTEN_MAY_TINH
            // 
            this.colTEN_MAY_TINH.Caption = "Tên Máy (Lần Cuối)";
            this.colTEN_MAY_TINH.FieldName = "TEN_MAY_TINH";
            this.colTEN_MAY_TINH.Name = "colTEN_MAY_TINH";
            this.colTEN_MAY_TINH.Visible = true;
            this.colTEN_MAY_TINH.VisibleIndex = 6;
            // 
            // colTHOIGIAN_DANGNHAP
            // 
            this.colTHOIGIAN_DANGNHAP.Caption = "TG Đăng Nhập (Lần Cuối)";
            this.colTHOIGIAN_DANGNHAP.FieldName = "THOIGIAN_DANGNHAP";
            this.colTHOIGIAN_DANGNHAP.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colTHOIGIAN_DANGNHAP.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm:ss";
            this.colTHOIGIAN_DANGNHAP.Name = "colTHOIGIAN_DANGNHAP";
            this.colTHOIGIAN_DANGNHAP.Visible = true;
            this.colTHOIGIAN_DANGNHAP.VisibleIndex = 7;
            // 
            // colTRANGTHAI_DANGNHAP
            // 
            this.colTRANGTHAI_DANGNHAP.Caption = "Trạng Thái (Lần Cuối)";
            this.colTRANGTHAI_DANGNHAP.FieldName = "TRANGTHAI_DANGNHAP";
            this.colTRANGTHAI_DANGNHAP.Name = "colTRANGTHAI_DANGNHAP";
            this.colTRANGTHAI_DANGNHAP.Visible = true;
            this.colTRANGTHAI_DANGNHAP.VisibleIndex = 8;
            // 
            // colTRANGTHAI_ONLINE
            // 
            this.colTRANGTHAI_ONLINE.Caption = "Đang Trực Tuyến";
            this.colTRANGTHAI_ONLINE.FieldName = "TRANGTHAI_ONLINE";
            this.colTRANGTHAI_ONLINE.Name = "colTRANGTHAI_ONLINE";
            this.colTRANGTHAI_ONLINE.Visible = true;
            this.colTRANGTHAI_ONLINE.VisibleIndex = 9;
            // 
            // FrmUserDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmUserDashboard";
            this.Text = "Dashboard Phiên Đăng Nhập Người Dùng";
            this.Load += new System.EventHandler(this.FrmUserDashboard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnRefresh;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        
        private DevExpress.XtraGrid.Columns.GridColumn colIDUSER;
        private DevExpress.XtraGrid.Columns.GridColumn colUSERNAME;
        private DevExpress.XtraGrid.Columns.GridColumn colFULLNAME;
        private DevExpress.XtraGrid.Columns.GridColumn colTRANGTHAI_HOATDONG;
        private DevExpress.XtraGrid.Columns.GridColumn colIP_ADDRESS;
        private DevExpress.XtraGrid.Columns.GridColumn colMAC_ADDRESS;
        private DevExpress.XtraGrid.Columns.GridColumn colTEN_MAY_TINH;
        private DevExpress.XtraGrid.Columns.GridColumn colTHOIGIAN_DANGNHAP;
        private DevExpress.XtraGrid.Columns.GridColumn colTRANGTHAI_DANGNHAP;
        private DevExpress.XtraGrid.Columns.GridColumn colTRANGTHAI_ONLINE;
    }
}
