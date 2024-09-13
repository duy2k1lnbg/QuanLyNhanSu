namespace QLyNSu
{
    partial class FrmCongTy
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCongTy));
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.btnThem = new DevExpress.XtraBars.BarButtonItem();
            this.btnSua = new DevExpress.XtraBars.BarButtonItem();
            this.btnXoa = new DevExpress.XtraBars.BarButtonItem();
            this.btnLuu = new DevExpress.XtraBars.BarButtonItem();
            this.btnHuy = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.btnIn = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.TENCTY = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IDCTY = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gvDsCT = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.DIENTHOAICTY = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EMAILCTY = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DIACHICTY = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcDsCT = new DevExpress.XtraGrid.GridControl();
            this.txtTen = new System.Windows.Forms.TextBox();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtDC = new System.Windows.Forms.TextBox();
            this.lap4 = new DevExpress.XtraEditors.LabelControl();
            this.txtEMAIL = new System.Windows.Forms.TextBox();
            this.lap3 = new DevExpress.XtraEditors.LabelControl();
            this.txtSDT = new System.Windows.Forms.TextBox();
            this.lap2 = new DevExpress.XtraEditors.LabelControl();
            this.txtDaiDien = new System.Windows.Forms.TextBox();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.txtMaSoThue = new System.Windows.Forms.TextBox();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.DAIDIEN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MASOTHUECTY = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDsCT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcDsCT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1307, 33);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 576);
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1,
            this.bar3});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btnThem,
            this.btnSua,
            this.btnXoa,
            this.btnLuu,
            this.btnHuy,
            this.btnDong,
            this.btnIn});
            this.barManager1.MaxItemId = 7;
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarAppearance.Disabled.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bar1.BarAppearance.Disabled.Options.UseFont = true;
            this.bar1.BarAppearance.Hovered.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bar1.BarAppearance.Hovered.Options.UseFont = true;
            this.bar1.BarAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bar1.BarAppearance.Normal.Options.UseFont = true;
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnThem, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnSua, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnXoa, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnLuu, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnHuy, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnDong, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnIn, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // btnThem
            // 
            this.btnThem.Caption = "Thêm";
            this.btnThem.Id = 0;
            this.btnThem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnThem.ImageOptions.SvgImage")));
            this.btnThem.Name = "btnThem";
            this.btnThem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnThem_ItemClick);
            // 
            // btnSua
            // 
            this.btnSua.Caption = "Sửa";
            this.btnSua.Id = 1;
            this.btnSua.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSua.ImageOptions.SvgImage")));
            this.btnSua.Name = "btnSua";
            this.btnSua.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSua_ItemClick);
            // 
            // btnXoa
            // 
            this.btnXoa.Caption = "Xoá";
            this.btnXoa.Id = 2;
            this.btnXoa.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnXoa.ImageOptions.SvgImage")));
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnXoa_ItemClick);
            // 
            // btnLuu
            // 
            this.btnLuu.Caption = "Lưu";
            this.btnLuu.Id = 3;
            this.btnLuu.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnLuu.ImageOptions.SvgImage")));
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLuu_ItemClick);
            // 
            // btnHuy
            // 
            this.btnHuy.Caption = "Huỷ";
            this.btnHuy.Id = 4;
            this.btnHuy.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnHuy.ImageOptions.SvgImage")));
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnHuy_ItemClick);
            // 
            // btnDong
            // 
            this.btnDong.Caption = "Đóng";
            this.btnDong.Id = 5;
            this.btnDong.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDong.ImageOptions.SvgImage")));
            this.btnDong.Name = "btnDong";
            this.btnDong.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDong_ItemClick);
            // 
            // btnIn
            // 
            this.btnIn.Caption = "In";
            this.btnIn.Id = 6;
            this.btnIn.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnIn.ImageOptions.SvgImage")));
            this.btnIn.Name = "btnIn";
            this.btnIn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnIn_ItemClick);
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockRow = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.Appearance.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barDockControlTop.Appearance.Options.UseFont = true;
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1307, 33);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 609);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1307, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 33);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 576);
            // 
            // TENCTY
            // 
            this.TENCTY.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.TENCTY.AppearanceHeader.Options.UseFont = true;
            this.TENCTY.Caption = "Tên Công Ty";
            this.TENCTY.FieldName = "TENCTY";
            this.TENCTY.MaxWidth = 200;
            this.TENCTY.MinWidth = 200;
            this.TENCTY.Name = "TENCTY";
            this.TENCTY.Visible = true;
            this.TENCTY.VisibleIndex = 1;
            this.TENCTY.Width = 200;
            // 
            // IDCTY
            // 
            this.IDCTY.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.IDCTY.AppearanceHeader.Options.UseFont = true;
            this.IDCTY.Caption = "ID Công Ty";
            this.IDCTY.FieldName = "IDCTY";
            this.IDCTY.MaxWidth = 100;
            this.IDCTY.MinWidth = 25;
            this.IDCTY.Name = "IDCTY";
            this.IDCTY.Visible = true;
            this.IDCTY.VisibleIndex = 0;
            this.IDCTY.Width = 94;
            // 
            // gvDsCT
            // 
            this.gvDsCT.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.IDCTY,
            this.TENCTY,
            this.DIENTHOAICTY,
            this.EMAILCTY,
            this.DIACHICTY,
            this.DAIDIEN,
            this.MASOTHUECTY});
            this.gvDsCT.GridControl = this.gcDsCT;
            this.gvDsCT.Name = "gvDsCT";
            this.gvDsCT.OptionsView.ShowGroupPanel = false;
            this.gvDsCT.Click += new System.EventHandler(this.gvDsCT_Click);
            // 
            // DIENTHOAICTY
            // 
            this.DIENTHOAICTY.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.DIENTHOAICTY.AppearanceHeader.Options.UseFont = true;
            this.DIENTHOAICTY.Caption = "Số Điện Thoại";
            this.DIENTHOAICTY.FieldName = "DIENTHOAICTY";
            this.DIENTHOAICTY.MinWidth = 25;
            this.DIENTHOAICTY.Name = "DIENTHOAICTY";
            this.DIENTHOAICTY.Visible = true;
            this.DIENTHOAICTY.VisibleIndex = 2;
            this.DIENTHOAICTY.Width = 94;
            // 
            // EMAILCTY
            // 
            this.EMAILCTY.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.EMAILCTY.AppearanceHeader.Options.UseFont = true;
            this.EMAILCTY.Caption = "Địa Chỉ Email";
            this.EMAILCTY.FieldName = "EMAILCTY";
            this.EMAILCTY.MaxWidth = 200;
            this.EMAILCTY.MinWidth = 200;
            this.EMAILCTY.Name = "EMAILCTY";
            this.EMAILCTY.Visible = true;
            this.EMAILCTY.VisibleIndex = 3;
            this.EMAILCTY.Width = 200;
            // 
            // DIACHICTY
            // 
            this.DIACHICTY.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.DIACHICTY.AppearanceHeader.Options.UseFont = true;
            this.DIACHICTY.Caption = "Địa Chỉ";
            this.DIACHICTY.FieldName = "DIACHICTY";
            this.DIACHICTY.MaxWidth = 200;
            this.DIACHICTY.MinWidth = 200;
            this.DIACHICTY.Name = "DIACHICTY";
            this.DIACHICTY.Visible = true;
            this.DIACHICTY.VisibleIndex = 4;
            this.DIACHICTY.Width = 200;
            // 
            // gcDsCT
            // 
            this.gcDsCT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcDsCT.Location = new System.Drawing.Point(0, 0);
            this.gcDsCT.MainView = this.gvDsCT;
            this.gcDsCT.MenuManager = this.barManager1;
            this.gcDsCT.Name = "gcDsCT";
            this.gcDsCT.Size = new System.Drawing.Size(1307, 381);
            this.gcDsCT.TabIndex = 0;
            this.gcDsCT.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvDsCT});
            // 
            // txtTen
            // 
            this.txtTen.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTen.Location = new System.Drawing.Point(168, 27);
            this.txtTen.Name = "txtTen";
            this.txtTen.Size = new System.Drawing.Size(465, 32);
            this.txtTen.TabIndex = 1;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(65, 30);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(83, 24);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Tên CTY:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtMaSoThue);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl3);
            this.splitContainer1.Panel1.Controls.Add(this.txtDaiDien);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl2);
            this.splitContainer1.Panel1.Controls.Add(this.txtDC);
            this.splitContainer1.Panel1.Controls.Add(this.lap4);
            this.splitContainer1.Panel1.Controls.Add(this.txtEMAIL);
            this.splitContainer1.Panel1.Controls.Add(this.lap3);
            this.splitContainer1.Panel1.Controls.Add(this.txtSDT);
            this.splitContainer1.Panel1.Controls.Add(this.lap2);
            this.splitContainer1.Panel1.Controls.Add(this.txtTen);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gcDsCT);
            this.splitContainer1.Size = new System.Drawing.Size(1307, 576);
            this.splitContainer1.SplitterDistance = 191;
            this.splitContainer1.TabIndex = 7;
            // 
            // txtDC
            // 
            this.txtDC.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDC.Location = new System.Drawing.Point(809, 65);
            this.txtDC.Name = "txtDC";
            this.txtDC.Size = new System.Drawing.Size(465, 32);
            this.txtDC.TabIndex = 7;
            // 
            // lap4
            // 
            this.lap4.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lap4.Appearance.Options.UseFont = true;
            this.lap4.Location = new System.Drawing.Point(726, 68);
            this.lap4.Name = "lap4";
            this.lap4.Size = new System.Drawing.Size(68, 24);
            this.lap4.TabIndex = 6;
            this.lap4.Text = "Địa chỉ:";
            // 
            // txtEMAIL
            // 
            this.txtEMAIL.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEMAIL.Location = new System.Drawing.Point(809, 27);
            this.txtEMAIL.Name = "txtEMAIL";
            this.txtEMAIL.Size = new System.Drawing.Size(465, 32);
            this.txtEMAIL.TabIndex = 5;
            // 
            // lap3
            // 
            this.lap3.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lap3.Appearance.Options.UseFont = true;
            this.lap3.Location = new System.Drawing.Point(738, 30);
            this.lap3.Name = "lap3";
            this.lap3.Size = new System.Drawing.Size(56, 24);
            this.lap3.TabIndex = 4;
            this.lap3.Text = "Email:";
            // 
            // txtSDT
            // 
            this.txtSDT.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSDT.Location = new System.Drawing.Point(168, 65);
            this.txtSDT.Name = "txtSDT";
            this.txtSDT.Size = new System.Drawing.Size(465, 32);
            this.txtSDT.TabIndex = 3;
            // 
            // lap2
            // 
            this.lap2.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lap2.Appearance.Options.UseFont = true;
            this.lap2.Location = new System.Drawing.Point(44, 68);
            this.lap2.Name = "lap2";
            this.lap2.Size = new System.Drawing.Size(99, 24);
            this.lap2.TabIndex = 2;
            this.lap2.Text = "Điện thoại:";
            // 
            // txtDaiDien
            // 
            this.txtDaiDien.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDaiDien.Location = new System.Drawing.Point(168, 103);
            this.txtDaiDien.Name = "txtDaiDien";
            this.txtDaiDien.Size = new System.Drawing.Size(465, 32);
            this.txtDaiDien.TabIndex = 9;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl2.Appearance.Options.UseFont = true;
            this.labelControl2.Location = new System.Drawing.Point(67, 106);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(81, 24);
            this.labelControl2.TabIndex = 8;
            this.labelControl2.Text = "Đại diện:";
            // 
            // txtMaSoThue
            // 
            this.txtMaSoThue.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMaSoThue.Location = new System.Drawing.Point(809, 103);
            this.txtMaSoThue.Name = "txtMaSoThue";
            this.txtMaSoThue.Size = new System.Drawing.Size(465, 32);
            this.txtMaSoThue.TabIndex = 11;
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Location = new System.Drawing.Point(689, 111);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(105, 24);
            this.labelControl3.TabIndex = 10;
            this.labelControl3.Text = "Mã số thuế:";
            // 
            // DAIDIEN
            // 
            this.DAIDIEN.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.DAIDIEN.AppearanceHeader.Options.UseFont = true;
            this.DAIDIEN.Caption = "Đại Diện";
            this.DAIDIEN.FieldName = "DAIDIEN";
            this.DAIDIEN.MaxWidth = 200;
            this.DAIDIEN.MinWidth = 200;
            this.DAIDIEN.Name = "DAIDIEN";
            this.DAIDIEN.Visible = true;
            this.DAIDIEN.VisibleIndex = 5;
            this.DAIDIEN.Width = 200;
            // 
            // MASOTHUECTY
            // 
            this.MASOTHUECTY.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.MASOTHUECTY.AppearanceHeader.Options.UseFont = true;
            this.MASOTHUECTY.Caption = "Mã Số Thuế";
            this.MASOTHUECTY.FieldName = "MASOTHUECTY";
            this.MASOTHUECTY.MinWidth = 25;
            this.MASOTHUECTY.Name = "MASOTHUECTY";
            this.MASOTHUECTY.Visible = true;
            this.MASOTHUECTY.VisibleIndex = 6;
            this.MASOTHUECTY.Width = 94;
            // 
            // FrmCongTy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1307, 629);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmCongTy";
            this.Text = "Công Ty";
            this.Load += new System.EventHandler(this.FrmCongTy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDsCT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcDsCT)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnThem;
        private DevExpress.XtraBars.BarButtonItem btnSua;
        private DevExpress.XtraBars.BarButtonItem btnXoa;
        private DevExpress.XtraBars.BarButtonItem btnLuu;
        private DevExpress.XtraBars.BarButtonItem btnHuy;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarButtonItem btnIn;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox txtTen;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraGrid.GridControl gcDsCT;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDsCT;
        private DevExpress.XtraGrid.Columns.GridColumn IDCTY;
        private DevExpress.XtraGrid.Columns.GridColumn TENCTY;
        private System.Windows.Forms.TextBox txtSDT;
        private DevExpress.XtraEditors.LabelControl lap2;
        private System.Windows.Forms.TextBox txtDC;
        private DevExpress.XtraEditors.LabelControl lap4;
        private System.Windows.Forms.TextBox txtEMAIL;
        private DevExpress.XtraEditors.LabelControl lap3;
        private DevExpress.XtraGrid.Columns.GridColumn DIENTHOAICTY;
        private DevExpress.XtraGrid.Columns.GridColumn EMAILCTY;
        private DevExpress.XtraGrid.Columns.GridColumn DIACHICTY;
        private System.Windows.Forms.TextBox txtMaSoThue;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private System.Windows.Forms.TextBox txtDaiDien;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraGrid.Columns.GridColumn DAIDIEN;
        private DevExpress.XtraGrid.Columns.GridColumn MASOTHUECTY;
    }
}