namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmPhatSinhLuong
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
            this.btnThem = new DevExpress.XtraBars.BarButtonItem();
            this.btnDuyet = new DevExpress.XtraBars.BarButtonItem();
            this.btnThuHoi = new DevExpress.XtraBars.BarButtonItem();
            this.btnXoaMem = new DevExpress.XtraBars.BarButtonItem();
            this.btnLamMoi = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.panelControlTop = new DevExpress.XtraEditors.PanelControl();
            this.cboLoaiPhatSinh = new System.Windows.Forms.ComboBox();
            this.lblLoai = new DevExpress.XtraEditors.LabelControl();
            this.cboKyCong = new System.Windows.Forms.ComboBox();
            this.lblKyCong = new DevExpress.XtraEditors.LabelControl();
            this.gcPhatSinh = new DevExpress.XtraGrid.GridControl();
            this.gvPhatSinh = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colMaNV = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHoTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNhom = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenKhoan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNgay = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoLuong = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonVi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonGia = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoTien = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colLyDo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colChungTu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTrangThai = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlTop)).BeginInit();
            this.panelControlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcPhatSinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvPhatSinh)).BeginInit();
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
            this.btnThem,
            this.btnDuyet,
            this.btnThuHoi,
            this.btnXoaMem,
            this.btnLamMoi,
            this.btnDong});
            this.barManager1.MaxItemId = 6;
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
            new DevExpress.XtraBars.LinkPersistInfo(this.btnThem),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDuyet),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnThuHoi),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnXoaMem),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnLamMoi),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDong)});
            this.bar1.Text = "Tools";
            // 
            // btnThem
            // 
            this.btnThem.Caption = "Thêm Phát Sinh Nháp";
            this.btnThem.Id = 4;
            this.btnThem.Name = "btnThem";
            this.btnThem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnThem_ItemClick);
            // 
            // btnDuyet
            // 
            this.btnDuyet.Caption = "Duyệt Khoản Phát Sinh";
            this.btnDuyet.Id = 0;
            this.btnDuyet.Name = "btnDuyet";
            this.btnDuyet.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDuyet_ItemClick);
            // 
            // btnThuHoi
            // 
            this.btnThuHoi.Caption = "Thu Hồi Duyệt";
            this.btnThuHoi.Id = 5;
            this.btnThuHoi.Name = "btnThuHoi";
            this.btnThuHoi.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnThuHoi_ItemClick);
            // 
            // btnXoaMem
            // 
            this.btnXoaMem.Caption = "Xóa Vào Thùng Rác";
            this.btnXoaMem.Id = 1;
            this.btnXoaMem.Name = "btnXoaMem";
            this.btnXoaMem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnXoaMem_ItemClick);
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Caption = "Làm Mới";
            this.btnLamMoi.Id = 2;
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLamMoi_ItemClick);
            // 
            // btnDong
            // 
            this.btnDong.Caption = "Đóng";
            this.btnDong.Id = 3;
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
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 600);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(984, 0);
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
            this.barDockControlRight.Location = new System.Drawing.Point(984, 29);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 571);
            // 
            // panelControlTop
            // 
            this.panelControlTop.Controls.Add(this.cboLoaiPhatSinh);
            this.panelControlTop.Controls.Add(this.lblLoai);
            this.panelControlTop.Controls.Add(this.cboKyCong);
            this.panelControlTop.Controls.Add(this.lblKyCong);
            this.panelControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlTop.Location = new System.Drawing.Point(0, 29);
            this.panelControlTop.Name = "panelControlTop";
            this.panelControlTop.Size = new System.Drawing.Size(984, 55);
            this.panelControlTop.TabIndex = 4;
            // 
            // cboLoaiPhatSinh
            // 
            this.cboLoaiPhatSinh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoaiPhatSinh.Font = new System.Drawing.Font("Tahoma", 10F);
            this.cboLoaiPhatSinh.FormattingEnabled = true;
            this.cboLoaiPhatSinh.Location = new System.Drawing.Point(380, 16);
            this.cboLoaiPhatSinh.Name = "cboLoaiPhatSinh";
            this.cboLoaiPhatSinh.Size = new System.Drawing.Size(220, 24);
            this.cboLoaiPhatSinh.TabIndex = 3;
            this.cboLoaiPhatSinh.SelectedIndexChanged += new System.EventHandler(this.cboLoaiPhatSinh_SelectedIndexChanged);
            // 
            // lblLoai
            // 
            this.lblLoai.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblLoai.Appearance.Options.UseFont = true;
            this.lblLoai.Location = new System.Drawing.Point(280, 19);
            this.lblLoai.Name = "lblLoai";
            this.lblLoai.Size = new System.Drawing.Size(91, 16);
            this.lblLoai.TabIndex = 2;
            this.lblLoai.Text = "Nhóm phát sinh:";
            // 
            // cboKyCong
            // 
            this.cboKyCong.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboKyCong.Font = new System.Drawing.Font("Tahoma", 10F);
            this.cboKyCong.FormattingEnabled = true;
            this.cboKyCong.Location = new System.Drawing.Point(85, 16);
            this.cboKyCong.Name = "cboKyCong";
            this.cboKyCong.Size = new System.Drawing.Size(160, 24);
            this.cboKyCong.TabIndex = 1;
            this.cboKyCong.SelectedIndexChanged += new System.EventHandler(this.cboKyCong_SelectedIndexChanged);
            // 
            // lblKyCong
            // 
            this.lblKyCong.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblKyCong.Appearance.Options.UseFont = true;
            this.lblKyCong.Location = new System.Drawing.Point(18, 19);
            this.lblKyCong.Name = "lblKyCong";
            this.lblKyCong.Size = new System.Drawing.Size(53, 16);
            this.lblKyCong.TabIndex = 0;
            this.lblKyCong.Text = "Kỳ công:";
            // 
            // gcPhatSinh
            // 
            this.gcPhatSinh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcPhatSinh.Location = new System.Drawing.Point(0, 84);
            this.gcPhatSinh.MainView = this.gvPhatSinh;
            this.gcPhatSinh.MenuManager = this.barManager1;
            this.gcPhatSinh.Name = "gcPhatSinh";
            this.gcPhatSinh.Size = new System.Drawing.Size(984, 516);
            this.gcPhatSinh.TabIndex = 5;
            this.gcPhatSinh.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvPhatSinh});
            // 
            // gvPhatSinh
            // 
            this.gvPhatSinh.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colMaNV,
            this.colHoTen,
            this.colNhom,
            this.colTenKhoan,
            this.colNgay,
            this.colSoLuong,
            this.colDonVi,
            this.colDonGia,
            this.colSoTien,
            this.colLyDo,
            this.colChungTu,
            this.colTrangThai});
            this.gvPhatSinh.GridControl = this.gcPhatSinh;
            this.gvPhatSinh.Name = "gvPhatSinh";
            this.gvPhatSinh.OptionsBehavior.Editable = false;
            this.gvPhatSinh.OptionsView.ShowGroupPanel = false;
            // 
            // colMaNV
            // 
            this.colMaNV.Caption = "Mã NV";
            this.colMaNV.FieldName = "MaNV";
            this.colMaNV.Name = "colMaNV";
            this.colMaNV.Visible = true;
            this.colMaNV.VisibleIndex = 0;
            this.colMaNV.Width = 70;
            // 
            // colHoTen
            // 
            this.colHoTen.Caption = "Họ tên nhân viên";
            this.colHoTen.FieldName = "HoTen";
            this.colHoTen.Name = "colHoTen";
            this.colHoTen.Visible = true;
            this.colHoTen.VisibleIndex = 1;
            this.colHoTen.Width = 160;
            // 
            // colNhom
            // 
            this.colNhom.Caption = "Nhóm";
            this.colNhom.FieldName = "Nhom";
            this.colNhom.Name = "colNhom";
            this.colNhom.Visible = true;
            this.colNhom.VisibleIndex = 2;
            this.colNhom.Width = 120;
            // 
            // colTenKhoan
            // 
            this.colTenKhoan.Caption = "Khoản phát sinh";
            this.colTenKhoan.FieldName = "TenKhoan";
            this.colTenKhoan.Name = "colTenKhoan";
            this.colTenKhoan.Visible = true;
            this.colTenKhoan.VisibleIndex = 3;
            this.colTenKhoan.Width = 150;
            // 
            // colNgay
            // 
            this.colNgay.Caption = "Ngày phát sinh";
            this.colNgay.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.colNgay.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colNgay.FieldName = "NgayPhatSinh";
            this.colNgay.Name = "colNgay";
            this.colNgay.Visible = true;
            this.colNgay.VisibleIndex = 4;
            this.colNgay.Width = 90;
            // 
            // colSoLuong
            // 
            this.colSoLuong.Caption = "Số lượng";
            this.colSoLuong.FieldName = "SoLuong";
            this.colSoLuong.Name = "colSoLuong";
            this.colSoLuong.Visible = true;
            this.colSoLuong.VisibleIndex = 5;
            this.colSoLuong.Width = 65;
            // 
            // colDonVi
            // 
            this.colDonVi.Caption = "Đơn vị";
            this.colDonVi.FieldName = "DonVi";
            this.colDonVi.Name = "colDonVi";
            this.colDonVi.Visible = true;
            this.colDonVi.VisibleIndex = 6;
            this.colDonVi.Width = 60;
            // 
            // colDonGia
            // 
            this.colDonGia.Caption = "Đơn giá (đ)";
            this.colDonGia.DisplayFormat.FormatString = "{0:n0}";
            this.colDonGia.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colDonGia.FieldName = "DonGia";
            this.colDonGia.Name = "colDonGia";
            this.colDonGia.Visible = true;
            this.colDonGia.VisibleIndex = 7;
            this.colDonGia.Width = 100;
            // 
            // colSoTien
            // 
            this.colSoTien.Caption = "Số tiền (đ)";
            this.colSoTien.DisplayFormat.FormatString = "{0:n0}";
            this.colSoTien.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colSoTien.FieldName = "SoTien";
            this.colSoTien.Name = "colSoTien";
            this.colSoTien.Visible = true;
            this.colSoTien.VisibleIndex = 8;
            this.colSoTien.Width = 110;
            // 
            // colLyDo
            // 
            this.colLyDo.Caption = "Lý do / Căn cứ";
            this.colLyDo.FieldName = "LyDo";
            this.colLyDo.Name = "colLyDo";
            this.colLyDo.Visible = true;
            this.colLyDo.VisibleIndex = 9;
            this.colLyDo.Width = 170;
            // 
            // colChungTu
            // 
            this.colChungTu.Caption = "Số chứng từ";
            this.colChungTu.FieldName = "SoChungTu";
            this.colChungTu.Name = "colChungTu";
            this.colChungTu.Visible = true;
            this.colChungTu.VisibleIndex = 10;
            this.colChungTu.Width = 100;
            // 
            // colTrangThai
            // 
            this.colTrangThai.Caption = "Trạng thái";
            this.colTrangThai.FieldName = "TrangThai";
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.Visible = true;
            this.colTrangThai.VisibleIndex = 11;
            this.colTrangThai.Width = 90;
            // 
            // FrmPhatSinhLuong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 600);
            this.Controls.Add(this.gcPhatSinh);
            this.Controls.Add(this.panelControlTop);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmPhatSinhLuong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản Lý Khoản Phát Sinh Lương Trong Kỳ";
            this.Load += new System.EventHandler(this.FrmPhatSinhLuong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlTop)).EndInit();
            this.panelControlTop.ResumeLayout(false);
            this.panelControlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcPhatSinh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvPhatSinh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnThem;
        private DevExpress.XtraBars.BarButtonItem btnDuyet;
        private DevExpress.XtraBars.BarButtonItem btnThuHoi;
        private DevExpress.XtraBars.BarButtonItem btnXoaMem;
        private DevExpress.XtraBars.BarButtonItem btnLamMoi;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.PanelControl panelControlTop;
        private System.Windows.Forms.ComboBox cboLoaiPhatSinh;
        private DevExpress.XtraEditors.LabelControl lblLoai;
        private System.Windows.Forms.ComboBox cboKyCong;
        private DevExpress.XtraEditors.LabelControl lblKyCong;
        private DevExpress.XtraGrid.GridControl gcPhatSinh;
        private DevExpress.XtraGrid.Views.Grid.GridView gvPhatSinh;
        private DevExpress.XtraGrid.Columns.GridColumn colMaNV;
        private DevExpress.XtraGrid.Columns.GridColumn colHoTen;
        private DevExpress.XtraGrid.Columns.GridColumn colNhom;
        private DevExpress.XtraGrid.Columns.GridColumn colTenKhoan;
        private DevExpress.XtraGrid.Columns.GridColumn colNgay;
        private DevExpress.XtraGrid.Columns.GridColumn colSoLuong;
        private DevExpress.XtraGrid.Columns.GridColumn colDonVi;
        private DevExpress.XtraGrid.Columns.GridColumn colDonGia;
        private DevExpress.XtraGrid.Columns.GridColumn colSoTien;
        private DevExpress.XtraGrid.Columns.GridColumn colLyDo;
        private DevExpress.XtraGrid.Columns.GridColumn colChungTu;
        private DevExpress.XtraGrid.Columns.GridColumn colTrangThai;
    }
}
