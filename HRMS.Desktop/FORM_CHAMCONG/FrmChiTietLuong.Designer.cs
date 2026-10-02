namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmChiTietLuong
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
            this.btnIn = new DevExpress.XtraBars.BarButtonItem();
            this.btnDieuChinh = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.panelHeader = new DevExpress.XtraEditors.PanelControl();
            this.panelSummary = new DevExpress.XtraEditors.PanelControl();
            this.lblThucLinh = new DevExpress.XtraEditors.LabelControl();
            this.lblTongKhauTru = new DevExpress.XtraEditors.LabelControl();
            this.lblTongThuNhap = new DevExpress.XtraEditors.LabelControl();
            this.lblTrangThai = new DevExpress.XtraEditors.LabelControl();
            this.lblThongTin = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPageKhoanMuc = new DevExpress.XtraTab.XtraTabPage();
            this.gcKhoanMuc = new DevExpress.XtraGrid.GridControl();
            this.gvKhoanMuc = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colNhom = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenKhoan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonVi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoLuong = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonGia = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colThanhTien = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colGhiChu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.xtraTabPageCanCu = new DevExpress.XtraTab.XtraTabPage();
            this.gcCanCu = new DevExpress.XtraGrid.GridControl();
            this.gvCanCu = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colCCThanhPhan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCCNguon = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCCGiaTri = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCCTrangThai = new DevExpress.XtraGrid.Columns.GridColumn();
            this.xtraTabPageBaoHiemThue = new DevExpress.XtraTab.XtraTabPage();
            this.gcBaoHiemThue = new DevExpress.XtraGrid.GridControl();
            this.gvBaoHiemThue = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colBHChiTieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colBHTyLe = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colBHTienNLD = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colBHTienNSDLD = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colBHGhiChu = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).BeginInit();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelSummary)).BeginInit();
            this.panelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPageKhoanMuc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcKhoanMuc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvKhoanMuc)).BeginInit();
            this.xtraTabPageCanCu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcCanCu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCanCu)).BeginInit();
            this.xtraTabPageBaoHiemThue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcBaoHiemThue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvBaoHiemThue)).BeginInit();
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
            this.btnIn,
            this.btnDong,
            this.btnDieuChinh});
            this.barManager1.MaxItemId = 3;
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
            new DevExpress.XtraBars.LinkPersistInfo(this.btnIn),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDieuChinh),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDong)});
            this.bar1.Text = "Tools";
            // 
            // btnDieuChinh
            // 
            this.btnDieuChinh.Caption = "Điều Chỉnh Lương";
            this.btnDieuChinh.Id = 2;
            this.btnDieuChinh.Name = "btnDieuChinh";
            this.btnDieuChinh.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDieuChinh_ItemClick);
            // 
            // btnIn
            // 
            this.btnIn.Caption = "In Phiếu Lương";
            this.btnIn.Id = 0;
            this.btnIn.Name = "btnIn";
            this.btnIn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnIn_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(984, 29);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 661);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(984, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 29);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 632);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(984, 29);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 632);
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.panelSummary);
            this.panelHeader.Controls.Add(this.lblTrangThai);
            this.panelHeader.Controls.Add(this.lblThongTin);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 29);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(984, 115);
            this.panelHeader.TabIndex = 4;
            // 
            // panelSummary
            // 
            this.panelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSummary.Controls.Add(this.lblThucLinh);
            this.panelSummary.Controls.Add(this.lblTongKhauTru);
            this.panelSummary.Controls.Add(this.lblTongThuNhap);
            this.panelSummary.Location = new System.Drawing.Point(540, 10);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(430, 95);
            this.panelSummary.TabIndex = 3;
            // 
            // lblThucLinh
            // 
            this.lblThucLinh.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lblThucLinh.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.lblThucLinh.Appearance.Options.UseFont = true;
            this.lblThucLinh.Appearance.Options.UseForeColor = true;
            this.lblThucLinh.Location = new System.Drawing.Point(15, 62);
            this.lblThucLinh.Name = "lblThucLinh";
            this.lblThucLinh.Size = new System.Drawing.Size(186, 19);
            this.lblThucLinh.TabIndex = 2;
            this.lblThucLinh.Text = "Thực lĩnh: 0 đ";
            // 
            // lblTongKhauTru
            // 
            this.lblTongKhauTru.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblTongKhauTru.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblTongKhauTru.Appearance.Options.UseFont = true;
            this.lblTongKhauTru.Appearance.Options.UseForeColor = true;
            this.lblTongKhauTru.Location = new System.Drawing.Point(15, 36);
            this.lblTongKhauTru.Name = "lblTongKhauTru";
            this.lblTongKhauTru.Size = new System.Drawing.Size(147, 16);
            this.lblTongKhauTru.TabIndex = 1;
            this.lblTongKhauTru.Text = "Tổng khấu trừ: 0 đ";
            // 
            // lblTongThuNhap
            // 
            this.lblTongThuNhap.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblTongThuNhap.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblTongThuNhap.Appearance.Options.UseFont = true;
            this.lblTongThuNhap.Appearance.Options.UseForeColor = true;
            this.lblTongThuNhap.Location = new System.Drawing.Point(15, 10);
            this.lblTongThuNhap.Name = "lblTongThuNhap";
            this.lblTongThuNhap.Size = new System.Drawing.Size(150, 16);
            this.lblTongThuNhap.TabIndex = 0;
            this.lblTongThuNhap.Text = "Tổng thu nhập: 0 đ";
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.Appearance.Font = new System.Drawing.Font("Tahoma", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblTrangThai.Appearance.Options.UseFont = true;
            this.lblTrangThai.Location = new System.Drawing.Point(16, 75);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(175, 16);
            this.lblTrangThai.TabIndex = 2;
            this.lblTrangThai.Text = "Trạng thái: Bảng lương hiện hành";
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
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(350, 22);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "CHI TIẾT BẢNG LƯƠNG NHÂN VIÊN";
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 144);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPageKhoanMuc;
            this.xtraTabControl1.Size = new System.Drawing.Size(984, 517);
            this.xtraTabControl1.TabIndex = 5;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPageKhoanMuc,
            this.xtraTabPageCanCu,
            this.xtraTabPageBaoHiemThue});
            // 
            // xtraTabPageKhoanMuc
            // 
            this.xtraTabPageKhoanMuc.Controls.Add(this.gcKhoanMuc);
            this.xtraTabPageKhoanMuc.Name = "xtraTabPageKhoanMuc";
            this.xtraTabPageKhoanMuc.Size = new System.Drawing.Size(978, 489);
            this.xtraTabPageKhoanMuc.Text = "Khoản Mục Lương";
            // 
            // gcKhoanMuc
            // 
            this.gcKhoanMuc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcKhoanMuc.Location = new System.Drawing.Point(0, 0);
            this.gcKhoanMuc.MainView = this.gvKhoanMuc;
            this.gcKhoanMuc.MenuManager = this.barManager1;
            this.gcKhoanMuc.Name = "gcKhoanMuc";
            this.gcKhoanMuc.Size = new System.Drawing.Size(978, 489);
            this.gcKhoanMuc.TabIndex = 0;
            this.gcKhoanMuc.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvKhoanMuc});
            // 
            // gvKhoanMuc
            // 
            this.gvKhoanMuc.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colNhom,
            this.colTenKhoan,
            this.colDonVi,
            this.colSoLuong,
            this.colDonGia,
            this.colThanhTien,
            this.colGhiChu});
            this.gvKhoanMuc.GridControl = this.gcKhoanMuc;
            this.gvKhoanMuc.Name = "gvKhoanMuc";
            this.gvKhoanMuc.OptionsBehavior.Editable = false;
            this.gvKhoanMuc.OptionsView.ShowGroupPanel = false;
            // 
            // colNhom
            // 
            this.colNhom.Caption = "Nhóm";
            this.colNhom.FieldName = "Nhom";
            this.colNhom.Name = "colNhom";
            this.colNhom.Visible = true;
            this.colNhom.VisibleIndex = 0;
            this.colNhom.Width = 120;
            // 
            // colTenKhoan
            // 
            this.colTenKhoan.Caption = "Thành phần / Khoản mục";
            this.colTenKhoan.FieldName = "TenKhoan";
            this.colTenKhoan.Name = "colTenKhoan";
            this.colTenKhoan.Visible = true;
            this.colTenKhoan.VisibleIndex = 1;
            this.colTenKhoan.Width = 220;
            // 
            // colDonVi
            // 
            this.colDonVi.Caption = "Đơn vị";
            this.colDonVi.FieldName = "DonVi";
            this.colDonVi.Name = "colDonVi";
            this.colDonVi.Visible = true;
            this.colDonVi.VisibleIndex = 2;
            this.colDonVi.Width = 80;
            // 
            // colSoLuong
            // 
            this.colSoLuong.Caption = "Số lượng / Căn cứ";
            this.colSoLuong.FieldName = "SoLuong";
            this.colSoLuong.Name = "colSoLuong";
            this.colSoLuong.Visible = true;
            this.colSoLuong.VisibleIndex = 3;
            this.colSoLuong.Width = 110;
            // 
            // colDonGia
            // 
            this.colDonGia.Caption = "Đơn giá / Mức gốc";
            this.colDonGia.DisplayFormat.FormatString = "{0:n0}";
            this.colDonGia.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colDonGia.FieldName = "DonGia";
            this.colDonGia.Name = "colDonGia";
            this.colDonGia.Visible = true;
            this.colDonGia.VisibleIndex = 4;
            this.colDonGia.Width = 120;
            // 
            // colThanhTien
            // 
            this.colThanhTien.Caption = "Thành tiền (VND)";
            this.colThanhTien.DisplayFormat.FormatString = "{0:n0}";
            this.colThanhTien.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colThanhTien.FieldName = "ThanhTien";
            this.colThanhTien.Name = "colThanhTien";
            this.colThanhTien.Visible = true;
            this.colThanhTien.VisibleIndex = 5;
            this.colThanhTien.Width = 130;
            // 
            // colGhiChu
            // 
            this.colGhiChu.Caption = "Diễn giải / Căn cứ";
            this.colGhiChu.FieldName = "GhiChu";
            this.colGhiChu.Name = "colGhiChu";
            this.colGhiChu.Visible = true;
            this.colGhiChu.VisibleIndex = 6;
            this.colGhiChu.Width = 190;
            // 
            // xtraTabPageCanCu
            // 
            this.xtraTabPageCanCu.Controls.Add(this.gcCanCu);
            this.xtraTabPageCanCu.Name = "xtraTabPageCanCu";
            this.xtraTabPageCanCu.Size = new System.Drawing.Size(978, 489);
            this.xtraTabPageCanCu.Text = "Căn Cứ & Nguồn Tính";
            // 
            // gcCanCu
            // 
            this.gcCanCu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcCanCu.Location = new System.Drawing.Point(0, 0);
            this.gcCanCu.MainView = this.gvCanCu;
            this.gcCanCu.MenuManager = this.barManager1;
            this.gcCanCu.Name = "gcCanCu";
            this.gcCanCu.Size = new System.Drawing.Size(978, 489);
            this.gcCanCu.TabIndex = 0;
            this.gcCanCu.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvCanCu});
            // 
            // gvCanCu
            // 
            this.gvCanCu.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colCCThanhPhan,
            this.colCCNguon,
            this.colCCGiaTri,
            this.colCCTrangThai});
            this.gvCanCu.GridControl = this.gcCanCu;
            this.gvCanCu.Name = "gvCanCu";
            this.gvCanCu.OptionsBehavior.Editable = false;
            this.gvCanCu.OptionsView.ShowGroupPanel = false;
            // 
            // colCCThanhPhan
            // 
            this.colCCThanhPhan.Caption = "Thành phần căn cứ";
            this.colCCThanhPhan.FieldName = "ThanhPhan";
            this.colCCThanhPhan.Name = "colCCThanhPhan";
            this.colCCThanhPhan.Visible = true;
            this.colCCThanhPhan.VisibleIndex = 0;
            this.colCCThanhPhan.Width = 240;
            // 
            // colCCNguon
            // 
            this.colCCNguon.Caption = "Nguồn dữ liệu";
            this.colCCNguon.FieldName = "Nguon";
            this.colCCNguon.Name = "colCCNguon";
            this.colCCNguon.Visible = true;
            this.colCCNguon.VisibleIndex = 1;
            this.colCCNguon.Width = 240;
            // 
            // colCCGiaTri
            // 
            this.colCCGiaTri.Caption = "Giá trị áp dụng";
            this.colCCGiaTri.FieldName = "GiaTri";
            this.colCCGiaTri.Name = "colCCGiaTri";
            this.colCCGiaTri.Visible = true;
            this.colCCGiaTri.VisibleIndex = 2;
            this.colCCGiaTri.Width = 240;
            // 
            // colCCTrangThai
            // 
            this.colCCTrangThai.Caption = "Trạng thái nạp";
            this.colCCTrangThai.FieldName = "TrangThai";
            this.colCCTrangThai.Name = "colCCTrangThai";
            this.colCCTrangThai.Visible = true;
            this.colCCTrangThai.VisibleIndex = 3;
            this.colCCTrangThai.Width = 200;
            // 
            // xtraTabPageBaoHiemThue
            // 
            this.xtraTabPageBaoHiemThue.Controls.Add(this.gcBaoHiemThue);
            this.xtraTabPageBaoHiemThue.Name = "xtraTabPageBaoHiemThue";
            this.xtraTabPageBaoHiemThue.Size = new System.Drawing.Size(978, 489);
            this.xtraTabPageBaoHiemThue.Text = "Bảo Hiểm & Thuế";
            // 
            // gcBaoHiemThue
            // 
            this.gcBaoHiemThue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcBaoHiemThue.Location = new System.Drawing.Point(0, 0);
            this.gcBaoHiemThue.MainView = this.gvBaoHiemThue;
            this.gcBaoHiemThue.MenuManager = this.barManager1;
            this.gcBaoHiemThue.Name = "gcBaoHiemThue";
            this.gcBaoHiemThue.Size = new System.Drawing.Size(978, 489);
            this.gcBaoHiemThue.TabIndex = 0;
            this.gcBaoHiemThue.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvBaoHiemThue});
            // 
            // gvBaoHiemThue
            // 
            this.gvBaoHiemThue.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colBHChiTieu,
            this.colBHTyLe,
            this.colBHTienNLD,
            this.colBHTienNSDLD,
            this.colBHGhiChu});
            this.gvBaoHiemThue.GridControl = this.gcBaoHiemThue;
            this.gvBaoHiemThue.Name = "gvBaoHiemThue";
            this.gvBaoHiemThue.OptionsBehavior.Editable = false;
            this.gvBaoHiemThue.OptionsView.ShowGroupPanel = false;
            // 
            // colBHChiTieu
            // 
            this.colBHChiTieu.Caption = "Chỉ tiêu pháp lý";
            this.colBHChiTieu.FieldName = "ChiTieu";
            this.colBHChiTieu.Name = "colBHChiTieu";
            this.colBHChiTieu.Visible = true;
            this.colBHChiTieu.VisibleIndex = 0;
            this.colBHChiTieu.Width = 240;
            // 
            // colBHTyLe
            // 
            this.colBHTyLe.Caption = "Tỷ lệ (%) / Mức chuẩn";
            this.colBHTyLe.FieldName = "TyLe";
            this.colBHTyLe.Name = "colBHTyLe";
            this.colBHTyLe.Visible = true;
            this.colBHTyLe.VisibleIndex = 1;
            this.colBHTyLe.Width = 140;
            // 
            // colBHTienNLD
            // 
            this.colBHTienNLD.Caption = "NLĐ đóng / Giảm trừ (đ)";
            this.colBHTienNLD.DisplayFormat.FormatString = "{0:n0}";
            this.colBHTienNLD.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colBHTienNLD.FieldName = "TienNLD";
            this.colBHTienNLD.Name = "colBHTienNLD";
            this.colBHTienNLD.Visible = true;
            this.colBHTienNLD.VisibleIndex = 2;
            this.colBHTienNLD.Width = 160;
            // 
            // colBHTienNSDLD
            // 
            this.colBHTienNSDLD.Caption = "NSD LĐ đóng / Thu nhập (đ)";
            this.colBHTienNSDLD.DisplayFormat.FormatString = "{0:n0}";
            this.colBHTienNSDLD.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colBHTienNSDLD.FieldName = "TienNSDLD";
            this.colBHTienNSDLD.Name = "colBHTienNSDLD";
            this.colBHTienNSDLD.Visible = true;
            this.colBHTienNSDLD.VisibleIndex = 3;
            this.colBHTienNSDLD.Width = 160;
            // 
            // colBHGhiChu
            // 
            this.colBHGhiChu.Caption = "Căn cứ pháp lý";
            this.colBHGhiChu.FieldName = "GhiChu";
            this.colBHGhiChu.Name = "colBHGhiChu";
            this.colBHGhiChu.Visible = true;
            this.colBHGhiChu.VisibleIndex = 4;
            this.colBHGhiChu.Width = 240;
            // 
            // FrmChiTietLuong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmChiTietLuong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chi Tiết Bảng Lương Nhân Viên";
            this.Load += new System.EventHandler(this.FrmChiTietLuong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).EndInit();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelSummary)).EndInit();
            this.panelSummary.ResumeLayout(false);
            this.panelSummary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPageKhoanMuc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcKhoanMuc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvKhoanMuc)).EndInit();
            this.xtraTabPageCanCu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcCanCu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCanCu)).EndInit();
            this.xtraTabPageBaoHiemThue.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcBaoHiemThue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvBaoHiemThue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnIn;
        private DevExpress.XtraBars.BarButtonItem btnDieuChinh;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.PanelControl panelHeader;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblThongTin;
        private DevExpress.XtraEditors.LabelControl lblTrangThai;
        private DevExpress.XtraEditors.PanelControl panelSummary;
        private DevExpress.XtraEditors.LabelControl lblThucLinh;
        private DevExpress.XtraEditors.LabelControl lblTongKhauTru;
        private DevExpress.XtraEditors.LabelControl lblTongThuNhap;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageKhoanMuc;
        private DevExpress.XtraGrid.GridControl gcKhoanMuc;
        private DevExpress.XtraGrid.Views.Grid.GridView gvKhoanMuc;
        private DevExpress.XtraGrid.Columns.GridColumn colNhom;
        private DevExpress.XtraGrid.Columns.GridColumn colTenKhoan;
        private DevExpress.XtraGrid.Columns.GridColumn colDonVi;
        private DevExpress.XtraGrid.Columns.GridColumn colSoLuong;
        private DevExpress.XtraGrid.Columns.GridColumn colDonGia;
        private DevExpress.XtraGrid.Columns.GridColumn colThanhTien;
        private DevExpress.XtraGrid.Columns.GridColumn colGhiChu;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageCanCu;
        private DevExpress.XtraGrid.GridControl gcCanCu;
        private DevExpress.XtraGrid.Views.Grid.GridView gvCanCu;
        private DevExpress.XtraGrid.Columns.GridColumn colCCThanhPhan;
        private DevExpress.XtraGrid.Columns.GridColumn colCCNguon;
        private DevExpress.XtraGrid.Columns.GridColumn colCCGiaTri;
        private DevExpress.XtraGrid.Columns.GridColumn colCCTrangThai;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageBaoHiemThue;
        private DevExpress.XtraGrid.GridControl gcBaoHiemThue;
        private DevExpress.XtraGrid.Views.Grid.GridView gvBaoHiemThue;
        private DevExpress.XtraGrid.Columns.GridColumn colBHChiTieu;
        private DevExpress.XtraGrid.Columns.GridColumn colBHTyLe;
        private DevExpress.XtraGrid.Columns.GridColumn colBHTienNLD;
        private DevExpress.XtraGrid.Columns.GridColumn colBHTienNSDLD;
        private DevExpress.XtraGrid.Columns.GridColumn colBHGhiChu;
    }
}
