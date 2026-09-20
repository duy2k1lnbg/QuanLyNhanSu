namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmThongBao
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmThongBao));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.btnThem = new DevExpress.XtraBars.BarButtonItem();
            this.btnSua = new DevExpress.XtraBars.BarButtonItem();
            this.btnXoa = new DevExpress.XtraBars.BarButtonItem();
            this.btnLuu = new DevExpress.XtraBars.BarButtonItem();
            this.btnHuy = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtNgayDang = new DevExpress.XtraEditors.TextEdit();
            this.txtNguoiDang = new DevExpress.XtraEditors.TextEdit();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txtNoiDung = new DevExpress.XtraEditors.MemoEdit();
            this.txtTieuDe = new System.Windows.Forms.TextBox();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.cboLoaiTB = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.cboTrangThai = new DevExpress.XtraEditors.ComboBoxEdit();
            this.chkGhim = new DevExpress.XtraEditors.CheckEdit();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.dtNgayHetHan = new DevExpress.XtraEditors.DateEdit();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.txtFileDinhKem = new DevExpress.XtraEditors.ButtonEdit();
            this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
            this.cboCongTy = new DevExpress.XtraEditors.LookUpEdit();
            this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
            this.cboPhongBan = new DevExpress.XtraEditors.LookUpEdit();
            this.gcDanhSach = new DevExpress.XtraGrid.GridControl();
            this.gvDanhSach = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTieuDe = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNguoiDang = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNgayDang = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colLoaiTB = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colIsPinned = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTrangThai = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNgayHetHan = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNgayDang.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNguoiDang.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoiDung.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboLoaiTB.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTrangThai.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkGhim.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHetHan.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHetHan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileDinhKem.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCongTy.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPhongBan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcDanhSach)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDanhSach)).BeginInit();
            this.SuspendLayout();
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
            this.btnDong});
            this.barManager1.MaxItemId = 6;
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarAppearance.Disabled.Font = new System.Drawing.Font("Tahoma", 12F);
            this.bar1.BarAppearance.Disabled.Options.UseFont = true;
            this.bar1.BarAppearance.Hovered.Font = new System.Drawing.Font("Tahoma", 12F);
            this.bar1.BarAppearance.Hovered.Options.UseFont = true;
            this.bar1.BarAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 12F);
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
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnDong, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
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
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1008, 30);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 542);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1008, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 30);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 512);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1008, 30);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 512);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 30);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtNgayDang);
            this.splitContainer1.Panel1.Controls.Add(this.txtNguoiDang);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl4);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl3);
            this.splitContainer1.Panel1.Controls.Add(this.txtNoiDung);
            this.splitContainer1.Panel1.Controls.Add(this.txtTieuDe);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl2);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl1);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl5);
            this.splitContainer1.Panel1.Controls.Add(this.cboLoaiTB);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl6);
            this.splitContainer1.Panel1.Controls.Add(this.cboTrangThai);
            this.splitContainer1.Panel1.Controls.Add(this.chkGhim);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl7);
            this.splitContainer1.Panel1.Controls.Add(this.dtNgayHetHan);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl8);
            this.splitContainer1.Panel1.Controls.Add(this.txtFileDinhKem);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl9);
            this.splitContainer1.Panel1.Controls.Add(this.cboCongTy);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl10);
            this.splitContainer1.Panel1.Controls.Add(this.cboPhongBan);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gcDanhSach);
            this.splitContainer1.Size = new System.Drawing.Size(1008, 512);
            this.splitContainer1.SplitterDistance = 260;
            this.splitContainer1.TabIndex = 6;
            // 
            // txtNgayDang
            // 
            this.txtNgayDang.Location = new System.Drawing.Point(750, 57);
            this.txtNgayDang.MenuManager = this.barManager1;
            this.txtNgayDang.Name = "txtNgayDang";
            this.txtNgayDang.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtNgayDang.Properties.Appearance.Options.UseFont = true;
            this.txtNgayDang.Properties.ReadOnly = true;
            this.txtNgayDang.Size = new System.Drawing.Size(200, 30);
            this.txtNgayDang.TabIndex = 7;
            // 
            // txtNguoiDang
            // 
            this.txtNguoiDang.Location = new System.Drawing.Point(750, 17);
            this.txtNguoiDang.MenuManager = this.barManager1;
            this.txtNguoiDang.Name = "txtNguoiDang";
            this.txtNguoiDang.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtNguoiDang.Properties.Appearance.Options.UseFont = true;
            this.txtNguoiDang.Properties.ReadOnly = true;
            this.txtNguoiDang.Size = new System.Drawing.Size(200, 30);
            this.txtNguoiDang.TabIndex = 6;
            // 
            // labelControl4
            // 
            this.labelControl4.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl4.Appearance.Options.UseFont = true;
            this.labelControl4.Location = new System.Drawing.Point(640, 60);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(91, 23);
            this.labelControl4.TabIndex = 5;
            this.labelControl4.Text = "Ngày Đăng:";
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Location = new System.Drawing.Point(640, 20);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(100, 23);
            this.labelControl3.TabIndex = 4;
            this.labelControl3.Text = "Người Đăng:";
            // 
            // txtNoiDung
            // 
            this.txtNoiDung.Location = new System.Drawing.Point(120, 57);
            this.txtNoiDung.MenuManager = this.barManager1;
            this.txtNoiDung.Name = "txtNoiDung";
            this.txtNoiDung.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtNoiDung.Properties.Appearance.Options.UseFont = true;
            this.txtNoiDung.Size = new System.Drawing.Size(480, 70);
            this.txtNoiDung.TabIndex = 3;
            // 
            // txtTieuDe
            // 
            this.txtTieuDe.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtTieuDe.Location = new System.Drawing.Point(120, 17);
            this.txtTieuDe.Name = "txtTieuDe";
            this.txtTieuDe.Size = new System.Drawing.Size(480, 30);
            this.txtTieuDe.TabIndex = 2;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl2.Appearance.Options.UseFont = true;
            this.labelControl2.Location = new System.Drawing.Point(30, 60);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(83, 23);
            this.labelControl2.TabIndex = 1;
            this.labelControl2.Text = "Nội Dung:";
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(30, 20);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(65, 23);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Tiêu Đề:";
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl5.Appearance.Options.UseFont = true;
            this.labelControl5.Location = new System.Drawing.Point(30, 140);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(66, 23);
            this.labelControl5.TabIndex = 8;
            this.labelControl5.Text = "Loại TB:";
            // 
            // cboLoaiTB
            // 
            this.cboLoaiTB.Location = new System.Drawing.Point(120, 137);
            this.cboLoaiTB.MenuManager = this.barManager1;
            this.cboLoaiTB.Name = "cboLoaiTB";
            this.cboLoaiTB.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.cboLoaiTB.Properties.Appearance.Options.UseFont = true;
            this.cboLoaiTB.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboLoaiTB.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboLoaiTB.Size = new System.Drawing.Size(200, 30);
            this.cboLoaiTB.TabIndex = 9;
            // 
            // labelControl6
            // 
            this.labelControl6.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl6.Appearance.Options.UseFont = true;
            this.labelControl6.Location = new System.Drawing.Point(350, 140);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(84, 23);
            this.labelControl6.TabIndex = 10;
            this.labelControl6.Text = "Trạng thái:";
            // 
            // cboTrangThai
            // 
            this.cboTrangThai.Location = new System.Drawing.Point(440, 137);
            this.cboTrangThai.MenuManager = this.barManager1;
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.cboTrangThai.Properties.Appearance.Options.UseFont = true;
            this.cboTrangThai.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboTrangThai.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboTrangThai.Size = new System.Drawing.Size(160, 30);
            this.cboTrangThai.TabIndex = 11;
            // 
            // chkGhim
            // 
            this.chkGhim.Location = new System.Drawing.Point(640, 137);
            this.chkGhim.MenuManager = this.barManager1;
            this.chkGhim.Name = "chkGhim";
            this.chkGhim.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.chkGhim.Properties.Appearance.Options.UseFont = true;
            this.chkGhim.Properties.Caption = "Ghim đầu trang";
            this.chkGhim.Size = new System.Drawing.Size(150, 30);
            this.chkGhim.TabIndex = 12;
            // 
            // labelControl7
            // 
            this.labelControl7.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl7.Appearance.Options.UseFont = true;
            this.labelControl7.Location = new System.Drawing.Point(30, 180);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(65, 23);
            this.labelControl7.TabIndex = 13;
            this.labelControl7.Text = "Hết hạn:";
            // 
            // dtNgayHetHan
            // 
            this.dtNgayHetHan.EditValue = null;
            this.dtNgayHetHan.Location = new System.Drawing.Point(120, 177);
            this.dtNgayHetHan.MenuManager = this.barManager1;
            this.dtNgayHetHan.Name = "dtNgayHetHan";
            this.dtNgayHetHan.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.dtNgayHetHan.Properties.Appearance.Options.UseFont = true;
            this.dtNgayHetHan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayHetHan.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayHetHan.Properties.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm";
            this.dtNgayHetHan.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayHetHan.Properties.EditFormat.FormatString = "dd/MM/yyyy HH:mm";
            this.dtNgayHetHan.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayHetHan.Properties.MaskSettings.Set("mask", "g");
            this.dtNgayHetHan.Size = new System.Drawing.Size(200, 30);
            this.dtNgayHetHan.TabIndex = 14;
            // 
            // labelControl8
            // 
            this.labelControl8.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl8.Appearance.Options.UseFont = true;
            this.labelControl8.Location = new System.Drawing.Point(350, 180);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(81, 23);
            this.labelControl8.TabIndex = 15;
            this.labelControl8.Text = "Đính kèm:";
            // 
            // txtFileDinhKem
            // 
            this.txtFileDinhKem.Location = new System.Drawing.Point(440, 177);
            this.txtFileDinhKem.MenuManager = this.barManager1;
            this.txtFileDinhKem.Name = "txtFileDinhKem";
            this.txtFileDinhKem.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtFileDinhKem.Properties.Appearance.Options.UseFont = true;
            this.txtFileDinhKem.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.txtFileDinhKem.Size = new System.Drawing.Size(510, 30);
            this.txtFileDinhKem.TabIndex = 16;
            this.txtFileDinhKem.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.txtFileDinhKem_ButtonClick);
            // 
            // labelControl9
            // 
            this.labelControl9.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl9.Appearance.Options.UseFont = true;
            this.labelControl9.Location = new System.Drawing.Point(30, 220);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(68, 23);
            this.labelControl9.TabIndex = 17;
            this.labelControl9.Text = "Công ty:";
            // 
            // cboCongTy
            // 
            this.cboCongTy.Location = new System.Drawing.Point(120, 217);
            this.cboCongTy.MenuManager = this.barManager1;
            this.cboCongTy.Name = "cboCongTy";
            this.cboCongTy.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.cboCongTy.Properties.Appearance.Options.UseFont = true;
            this.cboCongTy.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboCongTy.Properties.NullText = "-- Chọn Công ty (Tất cả) --";
            this.cboCongTy.Size = new System.Drawing.Size(300, 30);
            this.cboCongTy.TabIndex = 18;
            // 
            // labelControl10
            // 
            this.labelControl10.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.labelControl10.Appearance.Options.UseFont = true;
            this.labelControl10.Location = new System.Drawing.Point(450, 220);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(91, 23);
            this.labelControl10.TabIndex = 19;
            this.labelControl10.Text = "Phòng ban:";
            // 
            // cboPhongBan
            // 
            this.cboPhongBan.Location = new System.Drawing.Point(550, 217);
            this.cboPhongBan.MenuManager = this.barManager1;
            this.cboPhongBan.Name = "cboPhongBan";
            this.cboPhongBan.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.cboPhongBan.Properties.Appearance.Options.UseFont = true;
            this.cboPhongBan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboPhongBan.Properties.NullText = "-- Chọn Phòng ban (Tất cả) --";
            this.cboPhongBan.Size = new System.Drawing.Size(400, 30);
            this.cboPhongBan.TabIndex = 20;
            // 
            // gcDanhSach
            // 
            this.gcDanhSach.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcDanhSach.Location = new System.Drawing.Point(0, 0);
            this.gcDanhSach.MainView = this.gvDanhSach;
            this.gcDanhSach.MenuManager = this.barManager1;
            this.gcDanhSach.Name = "gcDanhSach";
            this.gcDanhSach.Size = new System.Drawing.Size(1008, 348);
            this.gcDanhSach.TabIndex = 0;
            this.gcDanhSach.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvDanhSach});
            // 
            // gvDanhSach
            // 
            this.gvDanhSach.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colID,
            this.colTieuDe,
            this.colNguoiDang,
            this.colNgayDang,
            this.colLoaiTB,
            this.colIsPinned,
            this.colTrangThai,
            this.colNgayHetHan});
            this.gvDanhSach.GridControl = this.gcDanhSach;
            this.gvDanhSach.Name = "gvDanhSach";
            this.gvDanhSach.OptionsView.ShowGroupPanel = false;
            this.gvDanhSach.Click += new System.EventHandler(this.gvDanhSach_Click);
            // 
            // colID
            // 
            this.colID.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colID.AppearanceHeader.Options.UseFont = true;
            this.colID.Caption = "ID";
            this.colID.FieldName = "ID";
            this.colID.MaxWidth = 60;
            this.colID.MinWidth = 25;
            this.colID.Name = "colID";
            this.colID.Visible = true;
            this.colID.VisibleIndex = 0;
            this.colID.Width = 50;
            // 
            // colTieuDe
            // 
            this.colTieuDe.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colTieuDe.AppearanceHeader.Options.UseFont = true;
            this.colTieuDe.Caption = "Tiêu Đề";
            this.colTieuDe.FieldName = "TIEUDE";
            this.colTieuDe.MinWidth = 200;
            this.colTieuDe.Name = "colTieuDe";
            this.colTieuDe.Visible = true;
            this.colTieuDe.VisibleIndex = 1;
            this.colTieuDe.Width = 350;
            // 
            // colNguoiDang
            // 
            this.colNguoiDang.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colNguoiDang.AppearanceHeader.Options.UseFont = true;
            this.colNguoiDang.Caption = "Người Đăng";
            this.colNguoiDang.FieldName = "NGUOIDANG";
            this.colNguoiDang.MaxWidth = 150;
            this.colNguoiDang.MinWidth = 100;
            this.colNguoiDang.Name = "colNguoiDang";
            this.colNguoiDang.Visible = true;
            this.colNguoiDang.VisibleIndex = 2;
            this.colNguoiDang.Width = 120;
            // 
            // colNgayDang
            // 
            this.colNgayDang.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colNgayDang.AppearanceHeader.Options.UseFont = true;
            this.colNgayDang.Caption = "Ngày Đăng";
            this.colNgayDang.FieldName = "NGAYDANG";
            this.colNgayDang.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm";
            this.colNgayDang.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colNgayDang.MaxWidth = 200;
            this.colNgayDang.MinWidth = 120;
            this.colNgayDang.Name = "colNgayDang";
            this.colNgayDang.Visible = true;
            this.colNgayDang.VisibleIndex = 3;
            this.colNgayDang.Width = 130;
            // 
            // colLoaiTB
            // 
            this.colLoaiTB.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colLoaiTB.AppearanceHeader.Options.UseFont = true;
            this.colLoaiTB.Caption = "Loại Tin";
            this.colLoaiTB.FieldName = "LOAI_TB";
            this.colLoaiTB.MinWidth = 100;
            this.colLoaiTB.Name = "colLoaiTB";
            this.colLoaiTB.Visible = true;
            this.colLoaiTB.VisibleIndex = 4;
            this.colLoaiTB.Width = 120;
            // 
            // colIsPinned
            // 
            this.colIsPinned.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colIsPinned.AppearanceHeader.Options.UseFont = true;
            this.colIsPinned.Caption = "Ghim";
            this.colIsPinned.FieldName = "IS_PINNED";
            this.colIsPinned.MaxWidth = 60;
            this.colIsPinned.MinWidth = 50;
            this.colIsPinned.Name = "colIsPinned";
            this.colIsPinned.Visible = true;
            this.colIsPinned.VisibleIndex = 5;
            this.colIsPinned.Width = 50;
            // 
            // colTrangThai
            // 
            this.colTrangThai.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colTrangThai.AppearanceHeader.Options.UseFont = true;
            this.colTrangThai.Caption = "Trạng Thái";
            this.colTrangThai.FieldName = "TRANGTHAI";
            this.colTrangThai.MinWidth = 80;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.Visible = true;
            this.colTrangThai.VisibleIndex = 6;
            this.colTrangThai.Width = 100;
            // 
            // colNgayHetHan
            // 
            this.colNgayHetHan.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.colNgayHetHan.AppearanceHeader.Options.UseFont = true;
            this.colNgayHetHan.Caption = "Hết Hạn";
            this.colNgayHetHan.FieldName = "NGAY_HETHAN";
            this.colNgayHetHan.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm";
            this.colNgayHetHan.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colNgayHetHan.MinWidth = 120;
            this.colNgayHetHan.Name = "colNgayHetHan";
            this.colNgayHetHan.Visible = true;
            this.colNgayHetHan.VisibleIndex = 7;
            this.colNgayHetHan.Width = 130;
            // 
            // FrmThongBao
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 562);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmThongBao";
            this.Text = "Quản Lý Thông Báo";
            this.Load += new System.EventHandler(this.FrmThongBao_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtNgayDang.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNguoiDang.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoiDung.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboLoaiTB.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTrangThai.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkGhim.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHetHan.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHetHan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileDinhKem.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCongTy.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPhongBan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcDanhSach)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDanhSach)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnThem;
        private DevExpress.XtraBars.BarButtonItem btnSua;
        private DevExpress.XtraBars.BarButtonItem btnXoa;
        private DevExpress.XtraBars.BarButtonItem btnLuu;
        private DevExpress.XtraBars.BarButtonItem btnHuy;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private System.Windows.Forms.TextBox txtTieuDe;
        private DevExpress.XtraEditors.MemoEdit txtNoiDung;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.TextEdit txtNguoiDang;
        private DevExpress.XtraEditors.TextEdit txtNgayDang;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.ComboBoxEdit cboLoaiTB;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.ComboBoxEdit cboTrangThai;
        private DevExpress.XtraEditors.CheckEdit chkGhim;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.DateEdit dtNgayHetHan;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.ButtonEdit txtFileDinhKem;
        private DevExpress.XtraEditors.LabelControl labelControl9;
        private DevExpress.XtraEditors.LookUpEdit cboCongTy;
        private DevExpress.XtraEditors.LabelControl labelControl10;
        private DevExpress.XtraEditors.LookUpEdit cboPhongBan;
        private DevExpress.XtraGrid.GridControl gcDanhSach;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDanhSach;
        private DevExpress.XtraGrid.Columns.GridColumn colID;
        private DevExpress.XtraGrid.Columns.GridColumn colTieuDe;
        private DevExpress.XtraGrid.Columns.GridColumn colNguoiDang;
        private DevExpress.XtraGrid.Columns.GridColumn colNgayDang;
        private DevExpress.XtraGrid.Columns.GridColumn colLoaiTB;
        private DevExpress.XtraGrid.Columns.GridColumn colIsPinned;
        private DevExpress.XtraGrid.Columns.GridColumn colTrangThai;
        private DevExpress.XtraGrid.Columns.GridColumn colNgayHetHan;
    }
}
