namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmBangLuong
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBangLuong));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.btnTinhLuong = new DevExpress.XtraBars.BarButtonItem();
            this.btnIn = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnTinhLuongCong = new DevExpress.XtraEditors.SimpleButton();
            this.cboKyCong = new System.Windows.Forms.ComboBox();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.gcBangLuong = new DevExpress.XtraGrid.GridControl();
            this.gvBangLuong = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.IDBL = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MANV = new DevExpress.XtraGrid.Columns.GridColumn();
            this.HOTEN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MAKYCONG = new DevExpress.XtraGrid.Columns.GridColumn();
            this.THANG = new DevExpress.XtraGrid.Columns.GridColumn();
            this.NAM = new DevExpress.XtraGrid.Columns.GridColumn();
            this.CONG_CHUAN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.CONG_THUCTE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DAILY_RATE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LUONG_CONG_THUCTE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TIEN_TANGCA = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TIEN_CHUYENCAN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.KHOAN_CONG_KHAC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TIEN_BHXH_TRICH = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TIEN_TAMUNG = new DevExpress.XtraGrid.Columns.GridColumn();
            this.KHOAN_TRU_KHAC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.THUC_LINH = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcBangLuong)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvBangLuong)).BeginInit();
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
            this.btnTinhLuong,
            this.btnIn,
            this.btnDong});
            this.barManager1.MaxItemId = 3;
            // 
            // bar1
            // 
            this.bar1.BarAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bar1.BarAppearance.Normal.Options.UseFont = true;
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnTinhLuong, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnIn, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnDong, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // btnTinhLuong
            // 
            this.btnTinhLuong.Caption = "Tính Lương";
            this.btnTinhLuong.Id = 0;
            this.btnTinhLuong.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnTinhLuong.ImageOptions.SvgImage")));
            this.btnTinhLuong.Name = "btnTinhLuong";
            this.btnTinhLuong.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnTinhLuong_ItemClick);
            // 
            // btnIn
            // 
            this.btnIn.Caption = "In Bảng Lương";
            this.btnIn.Id = 1;
            this.btnIn.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnIn.ImageOptions.SvgImage")));
            this.btnIn.Name = "btnIn";
            this.btnIn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnIn_ItemClick);
            // 
            // btnDong
            // 
            this.btnDong.Caption = "Đóng";
            this.btnDong.Id = 2;
            this.btnDong.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDong.ImageOptions.SvgImage")));
            this.btnDong.Name = "btnDong";
            this.btnDong.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDong_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1280, 30);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 720);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1280, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 30);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 690);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1280, 30);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 690);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 30);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnTinhLuongCong);
            this.splitContainer1.Panel1.Controls.Add(this.cboKyCong);
            this.splitContainer1.Panel1.Controls.Add(this.labelControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gcBangLuong);
            this.splitContainer1.Size = new System.Drawing.Size(1280, 690);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.TabIndex = 4;
            // 
            // btnTinhLuongCong
            // 
            this.btnTinhLuongCong.Appearance.Font = new System.Drawing.Font("Times New Roman", 13.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTinhLuongCong.Appearance.Options.UseFont = true;
            this.btnTinhLuongCong.Location = new System.Drawing.Point(400, 22);
            this.btnTinhLuongCong.Name = "btnTinhLuongCong";
            this.btnTinhLuongCong.Size = new System.Drawing.Size(150, 35);
            this.btnTinhLuongCong.TabIndex = 2;
            this.btnTinhLuongCong.Text = "Tính Lương";
            this.btnTinhLuongCong.Click += new System.EventHandler(this.btnTinhLuongCong_Click);
            // 
            // cboKyCong
            // 
            this.cboKyCong.Font = new System.Drawing.Font("Times New Roman", 13.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboKyCong.FormattingEnabled = true;
            this.cboKyCong.Location = new System.Drawing.Point(150, 22);
            this.cboKyCong.Name = "cboKyCong";
            this.cboKyCong.Size = new System.Drawing.Size(220, 33);
            this.cboKyCong.TabIndex = 1;
            this.cboKyCong.SelectedIndexChanged += new System.EventHandler(this.cboKyCong_SelectedIndexChanged);
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Times New Roman", 13.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(40, 25);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(95, 25);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Kỳ Công:";
            // 
            // gcBangLuong
            // 
            this.gcBangLuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcBangLuong.Location = new System.Drawing.Point(0, 0);
            this.gcBangLuong.MainView = this.gvBangLuong;
            this.gcBangLuong.MenuManager = this.barManager1;
            this.gcBangLuong.Name = "gcBangLuong";
            this.gcBangLuong.Size = new System.Drawing.Size(1280, 606);
            this.gcBangLuong.TabIndex = 0;
            this.gcBangLuong.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvBangLuong});
            // 
            // gvBangLuong
            // 
            this.gvBangLuong.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.IDBL,
            this.MANV,
            this.HOTEN,
            this.MAKYCONG,
            this.THANG,
            this.NAM,
            this.CONG_CHUAN,
            this.CONG_THUCTE,
            this.DAILY_RATE,
            this.LUONG_CONG_THUCTE,
            this.TIEN_TANGCA,
            this.TIEN_CHUYENCAN,
            this.KHOAN_CONG_KHAC,
            this.TIEN_BHXH_TRICH,
            this.TIEN_TAMUNG,
            this.KHOAN_TRU_KHAC,
            this.THUC_LINH});
            this.gvBangLuong.GridControl = this.gcBangLuong;
            this.gvBangLuong.Name = "gvBangLuong";
            this.gvBangLuong.OptionsView.ShowGroupPanel = false;
            // 
            // IDBL
            // 
            this.IDBL.Caption = "ID";
            this.IDBL.FieldName = "IDBL";
            this.IDBL.Name = "IDBL";
            // 
            // MANV
            // 
            this.MANV.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.MANV.AppearanceHeader.Options.UseFont = true;
            this.MANV.Caption = "MÃ NV";
            this.MANV.FieldName = "MANV";
            this.MANV.MaxWidth = 100;
            this.MANV.MinWidth = 25;
            this.MANV.Name = "MANV";
            this.MANV.Visible = true;
            this.MANV.VisibleIndex = 0;
            this.MANV.Width = 80;
            // 
            // HOTEN
            // 
            this.HOTEN.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.HOTEN.AppearanceHeader.Options.UseFont = true;
            this.HOTEN.Caption = "HỌ TÊN";
            this.HOTEN.FieldName = "HOTEN";
            this.HOTEN.MaxWidth = 200;
            this.HOTEN.MinWidth = 150;
            this.HOTEN.Name = "HOTEN";
            this.HOTEN.Visible = true;
            this.HOTEN.VisibleIndex = 1;
            this.HOTEN.Width = 180;
            // 
            // MAKYCONG
            // 
            this.MAKYCONG.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.MAKYCONG.AppearanceHeader.Options.UseFont = true;
            this.MAKYCONG.Caption = "MÃ KỲ CÔNG";
            this.MAKYCONG.FieldName = "MAKYCONG";
            this.MAKYCONG.Name = "MAKYCONG";
            this.MAKYCONG.Visible = true;
            this.MAKYCONG.VisibleIndex = 2;
            this.MAKYCONG.Width = 90;
            // 
            // THANG
            // 
            this.THANG.Caption = "Tháng";
            this.THANG.FieldName = "THANG";
            this.THANG.Name = "THANG";
            // 
            // NAM
            // 
            this.NAM.Caption = "Năm";
            this.NAM.FieldName = "NAM";
            this.NAM.Name = "NAM";
            // 
            // CONG_CHUAN
            // 
            this.CONG_CHUAN.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.CONG_CHUAN.AppearanceHeader.Options.UseFont = true;
            this.CONG_CHUAN.Caption = "CÔNG CHUẨN";
            this.CONG_CHUAN.FieldName = "CONG_CHUAN";
            this.CONG_CHUAN.Name = "CONG_CHUAN";
            this.CONG_CHUAN.Visible = true;
            this.CONG_CHUAN.VisibleIndex = 3;
            this.CONG_CHUAN.Width = 90;
            // 
            // CONG_THUCTE
            // 
            this.CONG_THUCTE.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.CONG_THUCTE.AppearanceHeader.Options.UseFont = true;
            this.CONG_THUCTE.Caption = "CÔNG THỰC TẾ";
            this.CONG_THUCTE.FieldName = "CONG_THUCTE";
            this.CONG_THUCTE.Name = "CONG_THUCTE";
            this.CONG_THUCTE.Visible = true;
            this.CONG_THUCTE.VisibleIndex = 4;
            this.CONG_THUCTE.Width = 100;
            // 
            // DAILY_RATE
            // 
            this.DAILY_RATE.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.DAILY_RATE.AppearanceHeader.Options.UseFont = true;
            this.DAILY_RATE.Caption = "LƯƠNG NGÀY";
            this.DAILY_RATE.DisplayFormat.FormatString = "n0";
            this.DAILY_RATE.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.DAILY_RATE.FieldName = "DAILY_RATE";
            this.DAILY_RATE.Name = "DAILY_RATE";
            this.DAILY_RATE.Visible = true;
            this.DAILY_RATE.VisibleIndex = 5;
            this.DAILY_RATE.Width = 95;
            // 
            // LUONG_CONG_THUCTE
            // 
            this.LUONG_CONG_THUCTE.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.LUONG_CONG_THUCTE.AppearanceHeader.Options.UseFont = true;
            this.LUONG_CONG_THUCTE.Caption = "LƯƠNG CÔNG TT";
            this.LUONG_CONG_THUCTE.DisplayFormat.FormatString = "n0";
            this.LUONG_CONG_THUCTE.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.LUONG_CONG_THUCTE.FieldName = "LUONG_CONG_THUCTE";
            this.LUONG_CONG_THUCTE.Name = "LUONG_CONG_THUCTE";
            this.LUONG_CONG_THUCTE.Visible = true;
            this.LUONG_CONG_THUCTE.VisibleIndex = 6;
            this.LUONG_CONG_THUCTE.Width = 110;
            // 
            // TIEN_TANGCA
            // 
            this.TIEN_TANGCA.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.TIEN_TANGCA.AppearanceHeader.Options.UseFont = true;
            this.TIEN_TANGCA.Caption = "TĂNG CA";
            this.TIEN_TANGCA.DisplayFormat.FormatString = "n0";
            this.TIEN_TANGCA.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.TIEN_TANGCA.FieldName = "TIEN_TANGCA";
            this.TIEN_TANGCA.Name = "TIEN_TANGCA";
            this.TIEN_TANGCA.Visible = true;
            this.TIEN_TANGCA.VisibleIndex = 7;
            this.TIEN_TANGCA.Width = 90;
            // 
            // TIEN_CHUYENCAN
            // 
            this.TIEN_CHUYENCAN.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.TIEN_CHUYENCAN.AppearanceHeader.Options.UseFont = true;
            this.TIEN_CHUYENCAN.Caption = "CHUYÊN CẦN";
            this.TIEN_CHUYENCAN.DisplayFormat.FormatString = "n0";
            this.TIEN_CHUYENCAN.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.TIEN_CHUYENCAN.FieldName = "TIEN_CHUYENCAN";
            this.TIEN_CHUYENCAN.Name = "TIEN_CHUYENCAN";
            this.TIEN_CHUYENCAN.Visible = true;
            this.TIEN_CHUYENCAN.VisibleIndex = 8;
            this.TIEN_CHUYENCAN.Width = 90;
            // 
            // KHOAN_CONG_KHAC
            // 
            this.KHOAN_CONG_KHAC.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.KHOAN_CONG_KHAC.AppearanceHeader.Options.UseFont = true;
            this.KHOAN_CONG_KHAC.Caption = "CỘNG KHÁC";
            this.KHOAN_CONG_KHAC.DisplayFormat.FormatString = "n0";
            this.KHOAN_CONG_KHAC.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.KHOAN_CONG_KHAC.FieldName = "KHOAN_CONG_KHAC";
            this.KHOAN_CONG_KHAC.Name = "KHOAN_CONG_KHAC";
            this.KHOAN_CONG_KHAC.Visible = true;
            this.KHOAN_CONG_KHAC.VisibleIndex = 9;
            this.KHOAN_CONG_KHAC.Width = 90;
            // 
            // TIEN_BHXH_TRICH
            // 
            this.TIEN_BHXH_TRICH.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.TIEN_BHXH_TRICH.AppearanceHeader.Options.UseFont = true;
            this.TIEN_BHXH_TRICH.Caption = "BHXH TRỪ";
            this.TIEN_BHXH_TRICH.DisplayFormat.FormatString = "n0";
            this.TIEN_BHXH_TRICH.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.TIEN_BHXH_TRICH.FieldName = "TIEN_BHXH_TRICH";
            this.TIEN_BHXH_TRICH.Name = "TIEN_BHXH_TRICH";
            this.TIEN_BHXH_TRICH.Visible = true;
            this.TIEN_BHXH_TRICH.VisibleIndex = 10;
            this.TIEN_BHXH_TRICH.Width = 90;
            // 
            // TIEN_TAMUNG
            // 
            this.TIEN_TAMUNG.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.TIEN_TAMUNG.AppearanceHeader.Options.UseFont = true;
            this.TIEN_TAMUNG.Caption = "TẠM ỨNG";
            this.TIEN_TAMUNG.DisplayFormat.FormatString = "n0";
            this.TIEN_TAMUNG.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.TIEN_TAMUNG.FieldName = "TIEN_TAMUNG";
            this.TIEN_TAMUNG.Name = "TIEN_TAMUNG";
            this.TIEN_TAMUNG.Visible = true;
            this.TIEN_TAMUNG.VisibleIndex = 11;
            this.TIEN_TAMUNG.Width = 90;
            // 
            // KHOAN_TRU_KHAC
            // 
            this.KHOAN_TRU_KHAC.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.KHOAN_TRU_KHAC.AppearanceHeader.Options.UseFont = true;
            this.KHOAN_TRU_KHAC.Caption = "THUẾ TNCN";
            this.KHOAN_TRU_KHAC.DisplayFormat.FormatString = "n0";
            this.KHOAN_TRU_KHAC.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.KHOAN_TRU_KHAC.FieldName = "KHOAN_TRU_KHAC";
            this.KHOAN_TRU_KHAC.Name = "KHOAN_TRU_KHAC";
            this.KHOAN_TRU_KHAC.Visible = true;
            this.KHOAN_TRU_KHAC.VisibleIndex = 12;
            this.KHOAN_TRU_KHAC.Width = 90;
            // 
            // THUC_LINH
            // 
            this.THUC_LINH.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.THUC_LINH.AppearanceCell.Options.UseFont = true;
            this.THUC_LINH.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.THUC_LINH.AppearanceHeader.Options.UseFont = true;
            this.THUC_LINH.Caption = "THỰC LĨNH";
            this.THUC_LINH.DisplayFormat.FormatString = "n0";
            this.THUC_LINH.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.THUC_LINH.FieldName = "THUC_LINH";
            this.THUC_LINH.Name = "THUC_LINH";
            this.THUC_LINH.Visible = true;
            this.THUC_LINH.VisibleIndex = 13;
            this.THUC_LINH.Width = 120;
            // 
            // FrmBangLuong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmBangLuong";
            this.Text = "Bảng Lương";
            this.Load += new System.EventHandler(this.FrmBangLuong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcBangLuong)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvBangLuong)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnTinhLuong;
        private DevExpress.XtraBars.BarButtonItem btnIn;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ComboBox cboKyCong;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnTinhLuongCong;
        private DevExpress.XtraGrid.GridControl gcBangLuong;
        private DevExpress.XtraGrid.Views.Grid.GridView gvBangLuong;
        private DevExpress.XtraGrid.Columns.GridColumn IDBL;
        private DevExpress.XtraGrid.Columns.GridColumn MANV;
        private DevExpress.XtraGrid.Columns.GridColumn HOTEN;
        private DevExpress.XtraGrid.Columns.GridColumn MAKYCONG;
        private DevExpress.XtraGrid.Columns.GridColumn THANG;
        private DevExpress.XtraGrid.Columns.GridColumn NAM;
        private DevExpress.XtraGrid.Columns.GridColumn CONG_CHUAN;
        private DevExpress.XtraGrid.Columns.GridColumn CONG_THUCTE;
        private DevExpress.XtraGrid.Columns.GridColumn DAILY_RATE;
        private DevExpress.XtraGrid.Columns.GridColumn LUONG_CONG_THUCTE;
        private DevExpress.XtraGrid.Columns.GridColumn TIEN_TANGCA;
        private DevExpress.XtraGrid.Columns.GridColumn TIEN_CHUYENCAN;
        private DevExpress.XtraGrid.Columns.GridColumn KHOAN_CONG_KHAC;
        private DevExpress.XtraGrid.Columns.GridColumn TIEN_BHXH_TRICH;
        private DevExpress.XtraGrid.Columns.GridColumn TIEN_TAMUNG;
        private DevExpress.XtraGrid.Columns.GridColumn KHOAN_TRU_KHAC;
        private DevExpress.XtraGrid.Columns.GridColumn THUC_LINH;
    }
}