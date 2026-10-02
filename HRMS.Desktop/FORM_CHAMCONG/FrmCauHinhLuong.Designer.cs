namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmCauHinhLuong
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
            this.btnLuu = new DevExpress.XtraBars.BarButtonItem();
            this.btnLamMoi = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPageKhoanLuong = new DevExpress.XtraTab.XtraTabPage();
            this.gcKhoanLuong = new DevExpress.XtraGrid.GridControl();
            this.gvKhoanLuong = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colMaKhoan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenKhoan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNhom = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCachTinh = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonVi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTinhBHXH = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTinhThue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMienThue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTrangThai = new DevExpress.XtraGrid.Columns.GridColumn();
            this.xtraTabPageQuyTac = new DevExpress.XtraTab.XtraTabPage();
            this.gcQuyTac = new DevExpress.XtraGrid.GridControl();
            this.gvQuyTac = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colQTTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colQTPhamVi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colQTNoiDung = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colQTCapNhat = new DevExpress.XtraGrid.Columns.GridColumn();
            this.xtraTabPageChinhSach = new DevExpress.XtraTab.XtraTabPage();
            this.gcChinhSach = new DevExpress.XtraGrid.GridControl();
            this.gvChinhSach = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colCSTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCSGiaTri = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCSHieuLuc = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCSVanBan = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPageKhoanLuong.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcKhoanLuong)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvKhoanLuong)).BeginInit();
            this.xtraTabPageQuyTac.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcQuyTac)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvQuyTac)).BeginInit();
            this.xtraTabPageChinhSach.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcChinhSach)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvChinhSach)).BeginInit();
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
            this.btnLuu,
            this.btnLamMoi,
            this.btnDong});
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
            new DevExpress.XtraBars.LinkPersistInfo(this.btnLuu),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnLamMoi),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDong)});
            this.bar1.Text = "Tools";
            // 
            // btnLuu
            // 
            this.btnLuu.Caption = "Lưu Cấu Hình";
            this.btnLuu.Id = 0;
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLuu_ItemClick);
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Caption = "Làm Mới";
            this.btnLamMoi.Id = 1;
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLamMoi_ItemClick);
            // 
            // btnDong
            // 
            this.btnDong.Caption = "Đóng";
            this.btnDong.Id = 2;
            this.btnDong.Name = "btnDong";
            this.btnDong.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDong_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(950, 29);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 600);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(950, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 29);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 571);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(950, 29);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 571);
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 29);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPageKhoanLuong;
            this.xtraTabControl1.Size = new System.Drawing.Size(950, 571);
            this.xtraTabControl1.TabIndex = 4;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPageKhoanLuong,
            this.xtraTabPageQuyTac,
            this.xtraTabPageChinhSach});
            // 
            // xtraTabPageKhoanLuong
            // 
            this.xtraTabPageKhoanLuong.Controls.Add(this.gcKhoanLuong);
            this.xtraTabPageKhoanLuong.Name = "xtraTabPageKhoanLuong";
            this.xtraTabPageKhoanLuong.Size = new System.Drawing.Size(944, 543);
            this.xtraTabPageKhoanLuong.Text = "Danh Mục Khoản Lương & Phụ Cấp";
            // 
            // gcKhoanLuong
            // 
            this.gcKhoanLuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcKhoanLuong.Location = new System.Drawing.Point(0, 0);
            this.gcKhoanLuong.MainView = this.gvKhoanLuong;
            this.gcKhoanLuong.MenuManager = this.barManager1;
            this.gcKhoanLuong.Name = "gcKhoanLuong";
            this.gcKhoanLuong.Size = new System.Drawing.Size(944, 543);
            this.gcKhoanLuong.TabIndex = 0;
            this.gcKhoanLuong.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvKhoanLuong});
            // 
            // gvKhoanLuong
            // 
            this.gvKhoanLuong.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colMaKhoan,
            this.colTenKhoan,
            this.colNhom,
            this.colCachTinh,
            this.colDonVi,
            this.colTinhBHXH,
            this.colTinhThue,
            this.colMienThue,
            this.colTrangThai});
            this.gvKhoanLuong.GridControl = this.gcKhoanLuong;
            this.gvKhoanLuong.Name = "gvKhoanLuong";
            this.gvKhoanLuong.OptionsBehavior.Editable = true;
            this.gvKhoanLuong.OptionsView.ShowGroupPanel = false;
            // 
            // colMaKhoan
            // 
            this.colMaKhoan.Caption = "Mã khoản";
            this.colMaKhoan.FieldName = "MaKhoan";
            this.colMaKhoan.Name = "colMaKhoan";
            this.colMaKhoan.Visible = true;
            this.colMaKhoan.VisibleIndex = 0;
            this.colMaKhoan.Width = 110;
            // 
            // colTenKhoan
            // 
            this.colTenKhoan.Caption = "Tên thành phần / Khoản mục";
            this.colTenKhoan.FieldName = "TenKhoan";
            this.colTenKhoan.Name = "colTenKhoan";
            this.colTenKhoan.Visible = true;
            this.colTenKhoan.VisibleIndex = 1;
            this.colTenKhoan.Width = 200;
            // 
            // colNhom
            // 
            this.colNhom.Caption = "Nhóm";
            this.colNhom.FieldName = "Nhom";
            this.colNhom.Name = "colNhom";
            this.colNhom.Visible = true;
            this.colNhom.VisibleIndex = 2;
            this.colNhom.Width = 100;
            // 
            // colCachTinh
            // 
            this.colCachTinh.Caption = "Cách tính";
            this.colCachTinh.FieldName = "CachTinh";
            this.colCachTinh.Name = "colCachTinh";
            this.colCachTinh.Visible = true;
            this.colCachTinh.VisibleIndex = 3;
            this.colCachTinh.Width = 140;
            // 
            // colDonVi
            // 
            this.colDonVi.Caption = "Đơn vị";
            this.colDonVi.FieldName = "DonVi";
            this.colDonVi.Name = "colDonVi";
            this.colDonVi.Visible = true;
            this.colDonVi.VisibleIndex = 4;
            this.colDonVi.Width = 80;
            // 
            // colTinhBHXH
            // 
            this.colTinhBHXH.Caption = "Tính BHXH";
            this.colTinhBHXH.FieldName = "TinhBHXH";
            this.colTinhBHXH.Name = "colTinhBHXH";
            this.colTinhBHXH.Visible = true;
            this.colTinhBHXH.VisibleIndex = 5;
            this.colTinhBHXH.Width = 90;
            // 
            // colTinhThue
            // 
            this.colTinhThue.Caption = "Tính Thuế TNCN";
            this.colTinhThue.FieldName = "TinhThue";
            this.colTinhThue.Name = "colTinhThue";
            this.colTinhThue.Visible = true;
            this.colTinhThue.VisibleIndex = 6;
            this.colTinhThue.Width = 100;
            // 
            // colMienThue
            // 
            this.colMienThue.Caption = "Miễn thuế tối đa";
            this.colMienThue.DisplayFormat.FormatString = "{0:n0}";
            this.colMienThue.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colMienThue.FieldName = "MienThue";
            this.colMienThue.Name = "colMienThue";
            this.colMienThue.Visible = true;
            this.colMienThue.VisibleIndex = 7;
            this.colMienThue.Width = 110;
            // 
            // colTrangThai
            // 
            this.colTrangThai.Caption = "Trạng thái";
            this.colTrangThai.FieldName = "TrangThai";
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.Visible = true;
            this.colTrangThai.VisibleIndex = 8;
            this.colTrangThai.Width = 100;
            // 
            // xtraTabPageQuyTac
            // 
            this.xtraTabPageQuyTac.Controls.Add(this.gcQuyTac);
            this.xtraTabPageQuyTac.Name = "xtraTabPageQuyTac";
            this.xtraTabPageQuyTac.Size = new System.Drawing.Size(944, 543);
            this.xtraTabPageQuyTac.Text = "Quy Tắc & Căn Cứ Tính";
            // 
            // gcQuyTac
            // 
            this.gcQuyTac.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcQuyTac.Location = new System.Drawing.Point(0, 0);
            this.gcQuyTac.MainView = this.gvQuyTac;
            this.gcQuyTac.MenuManager = this.barManager1;
            this.gcQuyTac.Name = "gcQuyTac";
            this.gcQuyTac.Size = new System.Drawing.Size(944, 543);
            this.gcQuyTac.TabIndex = 0;
            this.gcQuyTac.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvQuyTac});
            // 
            // gvQuyTac
            // 
            this.gvQuyTac.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colQTTen,
            this.colQTPhamVi,
            this.colQTNoiDung,
            this.colQTCapNhat});
            this.gvQuyTac.GridControl = this.gcQuyTac;
            this.gvQuyTac.Name = "gvQuyTac";
            this.gvQuyTac.OptionsBehavior.Editable = false;
            this.gvQuyTac.OptionsView.ShowGroupPanel = false;
            // 
            // colQTTen
            // 
            this.colQTTen.Caption = "Quy tắc căn cứ";
            this.colQTTen.FieldName = "TenQuyTac";
            this.colQTTen.Name = "colQTTen";
            this.colQTTen.Visible = true;
            this.colQTTen.VisibleIndex = 0;
            this.colQTTen.Width = 220;
            // 
            // colQTPhamVi
            // 
            this.colQTPhamVi.Caption = "Phạm vi áp dụng";
            this.colQTPhamVi.FieldName = "PhamVi";
            this.colQTPhamVi.Name = "colQTPhamVi";
            this.colQTPhamVi.Visible = true;
            this.colQTPhamVi.VisibleIndex = 1;
            this.colQTPhamVi.Width = 180;
            // 
            // colQTNoiDung
            // 
            this.colQTNoiDung.Caption = "Nội dung / Công thức / Hệ số";
            this.colQTNoiDung.FieldName = "NoiDung";
            this.colQTNoiDung.Name = "colQTNoiDung";
            this.colQTNoiDung.Visible = true;
            this.colQTNoiDung.VisibleIndex = 2;
            this.colQTNoiDung.Width = 320;
            // 
            // colQTCapNhat
            // 
            this.colQTCapNhat.Caption = "Hiệu lực";
            this.colQTCapNhat.FieldName = "HieuLuc";
            this.colQTCapNhat.Name = "colQTCapNhat";
            this.colQTCapNhat.Visible = true;
            this.colQTCapNhat.VisibleIndex = 3;
            this.colQTCapNhat.Width = 150;
            // 
            // xtraTabPageChinhSach
            // 
            this.xtraTabPageChinhSach.Controls.Add(this.gcChinhSach);
            this.xtraTabPageChinhSach.Name = "xtraTabPageChinhSach";
            this.xtraTabPageChinhSach.Size = new System.Drawing.Size(944, 543);
            this.xtraTabPageChinhSach.Text = "Chính Sách Lương & Tham Số Pháp Lý";
            // 
            // gcChinhSach
            // 
            this.gcChinhSach.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcChinhSach.Location = new System.Drawing.Point(0, 0);
            this.gcChinhSach.MainView = this.gvChinhSach;
            this.gcChinhSach.MenuManager = this.barManager1;
            this.gcChinhSach.Name = "gcChinhSach";
            this.gcChinhSach.Size = new System.Drawing.Size(944, 543);
            this.gcChinhSach.TabIndex = 0;
            this.gcChinhSach.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvChinhSach});
            // 
            // gvChinhSach
            // 
            this.gvChinhSach.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colCSTen,
            this.colCSGiaTri,
            this.colCSHieuLuc,
            this.colCSVanBan});
            this.gvChinhSach.GridControl = this.gcChinhSach;
            this.gvChinhSach.Name = "gvChinhSach";
            this.gvChinhSach.OptionsBehavior.Editable = false;
            this.gvChinhSach.OptionsView.ShowGroupPanel = false;
            // 
            // colCSTen
            // 
            this.colCSTen.Caption = "Tham số chính sách";
            this.colCSTen.FieldName = "TenChinhSach";
            this.colCSTen.Name = "colCSTen";
            this.colCSTen.Visible = true;
            this.colCSTen.VisibleIndex = 0;
            this.colCSTen.Width = 240;
            // 
            // colCSGiaTri
            // 
            this.colCSGiaTri.Caption = "Mức / Tỷ lệ quy định";
            this.colCSGiaTri.FieldName = "GiaTri";
            this.colCSGiaTri.Name = "colCSGiaTri";
            this.colCSGiaTri.Visible = true;
            this.colCSGiaTri.VisibleIndex = 1;
            this.colCSGiaTri.Width = 200;
            // 
            // colCSHieuLuc
            // 
            this.colCSHieuLuc.Caption = "Thời điểm áp dụng";
            this.colCSHieuLuc.FieldName = "HieuLuc";
            this.colCSHieuLuc.Name = "colCSHieuLuc";
            this.colCSHieuLuc.Visible = true;
            this.colCSHieuLuc.VisibleIndex = 2;
            this.colCSHieuLuc.Width = 160;
            // 
            // colCSVanBan
            // 
            this.colCSVanBan.Caption = "Căn cứ văn bản";
            this.colCSVanBan.FieldName = "VanBan";
            this.colCSVanBan.Name = "colCSVanBan";
            this.colCSVanBan.Visible = true;
            this.colCSVanBan.VisibleIndex = 3;
            this.colCSVanBan.Width = 280;
            // 
            // FrmCauHinhLuong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmCauHinhLuong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cấu Hình Thành Phần & Chính Sách Lương";
            this.Load += new System.EventHandler(this.FrmCauHinhLuong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPageKhoanLuong.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcKhoanLuong)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvKhoanLuong)).EndInit();
            this.xtraTabPageQuyTac.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcQuyTac)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvQuyTac)).EndInit();
            this.xtraTabPageChinhSach.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcChinhSach)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvChinhSach)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnLuu;
        private DevExpress.XtraBars.BarButtonItem btnLamMoi;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageKhoanLuong;
        private DevExpress.XtraGrid.GridControl gcKhoanLuong;
        private DevExpress.XtraGrid.Views.Grid.GridView gvKhoanLuong;
        private DevExpress.XtraGrid.Columns.GridColumn colMaKhoan;
        private DevExpress.XtraGrid.Columns.GridColumn colTenKhoan;
        private DevExpress.XtraGrid.Columns.GridColumn colNhom;
        private DevExpress.XtraGrid.Columns.GridColumn colCachTinh;
        private DevExpress.XtraGrid.Columns.GridColumn colDonVi;
        private DevExpress.XtraGrid.Columns.GridColumn colTinhBHXH;
        private DevExpress.XtraGrid.Columns.GridColumn colTinhThue;
        private DevExpress.XtraGrid.Columns.GridColumn colMienThue;
        private DevExpress.XtraGrid.Columns.GridColumn colTrangThai;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageQuyTac;
        private DevExpress.XtraGrid.GridControl gcQuyTac;
        private DevExpress.XtraGrid.Views.Grid.GridView gvQuyTac;
        private DevExpress.XtraGrid.Columns.GridColumn colQTTen;
        private DevExpress.XtraGrid.Columns.GridColumn colQTPhamVi;
        private DevExpress.XtraGrid.Columns.GridColumn colQTNoiDung;
        private DevExpress.XtraGrid.Columns.GridColumn colQTCapNhat;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageChinhSach;
        private DevExpress.XtraGrid.GridControl gcChinhSach;
        private DevExpress.XtraGrid.Views.Grid.GridView gvChinhSach;
        private DevExpress.XtraGrid.Columns.GridColumn colCSTen;
        private DevExpress.XtraGrid.Columns.GridColumn colCSGiaTri;
        private DevExpress.XtraGrid.Columns.GridColumn colCSHieuLuc;
        private DevExpress.XtraGrid.Columns.GridColumn colCSVanBan;
    }
}
