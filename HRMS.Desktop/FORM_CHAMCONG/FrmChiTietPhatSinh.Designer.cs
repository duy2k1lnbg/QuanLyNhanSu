namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmChiTietPhatSinh
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
            this.btnLuuNhap = new DevExpress.XtraBars.BarButtonItem();
            this.btnDuyet = new DevExpress.XtraBars.BarButtonItem();
            this.btnThuHoi = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.panelMain = new DevExpress.XtraEditors.PanelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblNhanVien = new DevExpress.XtraEditors.LabelControl();
            this.lookUpNhanVien = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.searchLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colMaNV = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHoTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPhongBan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.lblKyCong = new DevExpress.XtraEditors.LabelControl();
            this.txtKyCong = new DevExpress.XtraEditors.TextEdit();
            this.lblLoai = new DevExpress.XtraEditors.LabelControl();
            this.cboLoai = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblTenKhoan = new DevExpress.XtraEditors.LabelControl();
            this.txtTenKhoan = new DevExpress.XtraEditors.TextEdit();
            this.lblSoTien = new DevExpress.XtraEditors.LabelControl();
            this.spSoTien = new DevExpress.XtraEditors.SpinEdit();
            this.lblNgayPhatSinh = new DevExpress.XtraEditors.LabelControl();
            this.dtNgayPhatSinh = new DevExpress.XtraEditors.DateEdit();
            this.lblSoChungTu = new DevExpress.XtraEditors.LabelControl();
            this.txtSoChungTu = new DevExpress.XtraEditors.TextEdit();
            this.lblTrangThai = new DevExpress.XtraEditors.LabelControl();
            this.txtTrangThai = new DevExpress.XtraEditors.TextEdit();
            this.lblLyDo = new DevExpress.XtraEditors.LabelControl();
            this.txtLyDo = new DevExpress.XtraEditors.MemoEdit();
            this.lblNote = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelMain)).BeginInit();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpNhanVien.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit1View)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtKyCong.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboLoai.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenKhoan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spSoTien.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayPhatSinh.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayPhatSinh.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoChungTu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTrangThai.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLyDo.Properties)).BeginInit();
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
            this.btnLuuNhap,
            this.btnDuyet,
            this.btnThuHoi,
            this.btnDong});
            this.barManager1.MaxItemId = 4;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnLuuNhap, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnDuyet, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnThuHoi, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnDong, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // btnLuuNhap
            // 
            this.btnLuuNhap.Caption = "Lưu nháp";
            this.btnLuuNhap.Id = 0;
            this.btnLuuNhap.Name = "btnLuuNhap";
            this.btnLuuNhap.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLuuNhap_ItemClick);
            // 
            // btnDuyet
            // 
            this.btnDuyet.Caption = "Phê duyệt";
            this.btnDuyet.Id = 1;
            this.btnDuyet.Name = "btnDuyet";
            this.btnDuyet.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            this.btnDuyet.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDuyet_ItemClick);
            // 
            // btnThuHoi
            // 
            this.btnThuHoi.Caption = "Thu hồi phê duyệt";
            this.btnThuHoi.Id = 2;
            this.btnThuHoi.Name = "btnThuHoi";
            this.btnThuHoi.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            this.btnThuHoi.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnThuHoi_ItemClick);
            // 
            // btnDong
            // 
            this.btnDong.Caption = "Hủy / Đóng";
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
            this.barDockControlTop.Size = new System.Drawing.Size(650, 24);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 480);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(650, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 24);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 456);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(650, 24);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 456);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.lblNote);
            this.panelMain.Controls.Add(this.txtLyDo);
            this.panelMain.Controls.Add(this.lblLyDo);
            this.panelMain.Controls.Add(this.txtTrangThai);
            this.panelMain.Controls.Add(this.lblTrangThai);
            this.panelMain.Controls.Add(this.txtSoChungTu);
            this.panelMain.Controls.Add(this.lblSoChungTu);
            this.panelMain.Controls.Add(this.dtNgayPhatSinh);
            this.panelMain.Controls.Add(this.lblNgayPhatSinh);
            this.panelMain.Controls.Add(this.spSoTien);
            this.panelMain.Controls.Add(this.lblSoTien);
            this.panelMain.Controls.Add(this.txtTenKhoan);
            this.panelMain.Controls.Add(this.lblTenKhoan);
            this.panelMain.Controls.Add(this.cboLoai);
            this.panelMain.Controls.Add(this.lblLoai);
            this.panelMain.Controls.Add(this.txtKyCong);
            this.panelMain.Controls.Add(this.lblKyCong);
            this.panelMain.Controls.Add(this.lookUpNhanVien);
            this.panelMain.Controls.Add(this.lblNhanVien);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 24);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(650, 456);
            this.panelMain.TabIndex = 4;
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(325, 23);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "THÔNG TIN PHÁT SINH LƯƠNG NHÂN SỰ";
            // 
            // lblNhanVien
            // 
            this.lblNhanVien.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNhanVien.Appearance.Options.UseFont = true;
            this.lblNhanVien.Location = new System.Drawing.Point(20, 58);
            this.lblNhanVien.Name = "lblNhanVien";
            this.lblNhanVien.Size = new System.Drawing.Size(83, 15);
            this.lblNhanVien.TabIndex = 1;
            this.lblNhanVien.Text = "Nhân viên (*):";
            // 
            // lookUpNhanVien
            // 
            this.lookUpNhanVien.Location = new System.Drawing.Point(130, 55);
            this.lookUpNhanVien.MenuManager = this.barManager1;
            this.lookUpNhanVien.Name = "lookUpNhanVien";
            this.lookUpNhanVien.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpNhanVien.Properties.NullText = "-- Chọn nhân viên từ danh sách --";
            this.lookUpNhanVien.Properties.PopupView = this.searchLookUpEdit1View;
            this.lookUpNhanVien.Size = new System.Drawing.Size(490, 22);
            this.lookUpNhanVien.TabIndex = 2;
            // 
            // searchLookUpEdit1View
            // 
            this.searchLookUpEdit1View.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colMaNV,
            this.colHoTen,
            this.colPhongBan});
            this.searchLookUpEdit1View.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.searchLookUpEdit1View.Name = "searchLookUpEdit1View";
            this.searchLookUpEdit1View.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.searchLookUpEdit1View.OptionsView.ShowGroupPanel = false;
            // 
            // colMaNV
            // 
            this.colMaNV.Caption = "Mã NV";
            this.colMaNV.FieldName = "MANV";
            this.colMaNV.Name = "colMaNV";
            this.colMaNV.Visible = true;
            this.colMaNV.VisibleIndex = 0;
            this.colMaNV.Width = 70;
            // 
            // colHoTen
            // 
            this.colHoTen.Caption = "Họ và Tên";
            this.colHoTen.FieldName = "HOTEN";
            this.colHoTen.Name = "colHoTen";
            this.colHoTen.Visible = true;
            this.colHoTen.VisibleIndex = 1;
            this.colHoTen.Width = 200;
            // 
            // colPhongBan
            // 
            this.colPhongBan.Caption = "Phòng ban";
            this.colPhongBan.FieldName = "TENPB";
            this.colPhongBan.Name = "colPhongBan";
            this.colPhongBan.Visible = true;
            this.colPhongBan.VisibleIndex = 2;
            this.colPhongBan.Width = 150;
            // 
            // lblKyCong
            // 
            this.lblKyCong.Location = new System.Drawing.Point(20, 95);
            this.lblKyCong.Name = "lblKyCong";
            this.lblKyCong.Size = new System.Drawing.Size(63, 13);
            this.lblKyCong.TabIndex = 3;
            this.lblKyCong.Text = "Kỳ áp dụng:";
            // 
            // txtKyCong
            // 
            this.txtKyCong.Location = new System.Drawing.Point(130, 92);
            this.txtKyCong.MenuManager = this.barManager1;
            this.txtKyCong.Name = "txtKyCong";
            this.txtKyCong.Properties.ReadOnly = true;
            this.txtKyCong.Size = new System.Drawing.Size(180, 20);
            this.txtKyCong.TabIndex = 4;
            // 
            // lblLoai
            // 
            this.lblLoai.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLoai.Appearance.Options.UseFont = true;
            this.lblLoai.Location = new System.Drawing.Point(340, 95);
            this.lblLoai.Name = "lblLoai";
            this.lblLoai.Size = new System.Drawing.Size(74, 15);
            this.lblLoai.TabIndex = 5;
            this.lblLoai.Text = "Loại khoản (*):";
            // 
            // cboLoai
            // 
            this.cboLoai.Location = new System.Drawing.Point(430, 92);
            this.cboLoai.MenuManager = this.barManager1;
            this.cboLoai.Name = "cboLoai";
            this.cboLoai.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboLoai.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboLoai.Size = new System.Drawing.Size(190, 20);
            this.cboLoai.TabIndex = 6;
            // 
            // lblTenKhoan
            // 
            this.lblTenKhoan.Location = new System.Drawing.Point(20, 132);
            this.lblTenKhoan.Name = "lblTenKhoan";
            this.lblTenKhoan.Size = new System.Drawing.Size(56, 13);
            this.lblTenKhoan.TabIndex = 7;
            this.lblTenKhoan.Text = "Tên khoản:";
            // 
            // txtTenKhoan
            // 
            this.txtTenKhoan.Location = new System.Drawing.Point(130, 129);
            this.txtTenKhoan.MenuManager = this.barManager1;
            this.txtTenKhoan.Name = "txtTenKhoan";
            this.txtTenKhoan.Size = new System.Drawing.Size(490, 20);
            this.txtTenKhoan.TabIndex = 8;
            // 
            // lblSoTien
            // 
            this.lblSoTien.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSoTien.Appearance.Options.UseFont = true;
            this.lblSoTien.Location = new System.Drawing.Point(20, 169);
            this.lblSoTien.Name = "lblSoTien";
            this.lblSoTien.Size = new System.Drawing.Size(95, 15);
            this.lblSoTien.TabIndex = 9;
            this.lblSoTien.Text = "Số tiền (VNĐ) (*):";
            // 
            // spSoTien
            // 
            this.spSoTien.EditValue = new decimal(new int[] {
            500000,
            0,
            0,
            0});
            this.spSoTien.Location = new System.Drawing.Point(130, 166);
            this.spSoTien.MenuManager = this.barManager1;
            this.spSoTien.Name = "spSoTien";
            this.spSoTien.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spSoTien.Properties.DisplayFormat.FormatString = "n0";
            this.spSoTien.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.spSoTien.Properties.EditFormat.FormatString = "n0";
            this.spSoTien.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.spSoTien.Properties.Increment = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.spSoTien.Properties.Mask.EditMask = "n0";
            this.spSoTien.Properties.MaxValue = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.spSoTien.Size = new System.Drawing.Size(180, 20);
            this.spSoTien.TabIndex = 10;
            // 
            // lblNgayPhatSinh
            // 
            this.lblNgayPhatSinh.Location = new System.Drawing.Point(340, 169);
            this.lblNgayPhatSinh.Name = "lblNgayPhatSinh";
            this.lblNgayPhatSinh.Size = new System.Drawing.Size(76, 13);
            this.lblNgayPhatSinh.TabIndex = 11;
            this.lblNgayPhatSinh.Text = "Ngày phát sinh:";
            // 
            // dtNgayPhatSinh
            // 
            this.dtNgayPhatSinh.EditValue = null;
            this.dtNgayPhatSinh.Location = new System.Drawing.Point(430, 166);
            this.dtNgayPhatSinh.MenuManager = this.barManager1;
            this.dtNgayPhatSinh.Name = "dtNgayPhatSinh";
            this.dtNgayPhatSinh.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayPhatSinh.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayPhatSinh.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.dtNgayPhatSinh.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayPhatSinh.Properties.EditFormat.FormatString = "dd/MM/yyyy";
            this.dtNgayPhatSinh.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayPhatSinh.Properties.Mask.EditMask = "dd/MM/yyyy";
            this.dtNgayPhatSinh.Size = new System.Drawing.Size(190, 20);
            this.dtNgayPhatSinh.TabIndex = 12;
            // 
            // lblSoChungTu
            // 
            this.lblSoChungTu.Location = new System.Drawing.Point(20, 206);
            this.lblSoChungTu.Name = "lblSoChungTu";
            this.lblSoChungTu.Size = new System.Drawing.Size(62, 13);
            this.lblSoChungTu.TabIndex = 13;
            this.lblSoChungTu.Text = "Số chứng từ:";
            // 
            // txtSoChungTu
            // 
            this.txtSoChungTu.Location = new System.Drawing.Point(130, 203);
            this.txtSoChungTu.MenuManager = this.barManager1;
            this.txtSoChungTu.Name = "txtSoChungTu";
            this.txtSoChungTu.Properties.NullValuePrompt = "Hệ thống tự sinh nếu để trống";
            this.txtSoChungTu.Size = new System.Drawing.Size(180, 20);
            this.txtSoChungTu.TabIndex = 14;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.Location = new System.Drawing.Point(340, 206);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(53, 13);
            this.lblTrangThai.TabIndex = 15;
            this.lblTrangThai.Text = "Trạng thái:";
            // 
            // txtTrangThai
            // 
            this.txtTrangThai.EditValue = "Bản nháp";
            this.txtTrangThai.Location = new System.Drawing.Point(430, 203);
            this.txtTrangThai.MenuManager = this.barManager1;
            this.txtTrangThai.Name = "txtTrangThai";
            this.txtTrangThai.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.txtTrangThai.Properties.Appearance.Options.UseFont = true;
            this.txtTrangThai.Properties.ReadOnly = true;
            this.txtTrangThai.Size = new System.Drawing.Size(190, 20);
            this.txtTrangThai.TabIndex = 16;
            // 
            // lblLyDo
            // 
            this.lblLyDo.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLyDo.Appearance.Options.UseFont = true;
            this.lblLyDo.Location = new System.Drawing.Point(20, 243);
            this.lblLyDo.Name = "lblLyDo";
            this.lblLyDo.Size = new System.Drawing.Size(89, 15);
            this.lblLyDo.TabIndex = 17;
            this.lblLyDo.Text = "Lý do / Căn cứ (*):";
            // 
            // txtLyDo
            // 
            this.txtLyDo.Location = new System.Drawing.Point(130, 241);
            this.txtLyDo.MenuManager = this.barManager1;
            this.txtLyDo.Name = "txtLyDo";
            this.txtLyDo.Properties.NullValuePrompt = "Nhập lý do chi tiết cho khoản phát sinh này (bắt buộc)...";
            this.txtLyDo.Size = new System.Drawing.Size(490, 150);
            this.txtLyDo.TabIndex = 18;
            // 
            // lblNote
            // 
            this.lblNote.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            this.lblNote.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.lblNote.Appearance.Options.UseFont = true;
            this.lblNote.Appearance.Options.UseForeColor = true;
            this.lblNote.Location = new System.Drawing.Point(130, 405);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(465, 13);
            this.lblNote.TabIndex = 19;
            this.lblNote.Text = "* Ghi chú: Khoản phát sinh ở trạng thái [Bản nháp] chưa được đưa vào tính lương c" +
    "ho đến khi được duyệt.";
            // 
            // FrmChiTietPhatSinh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 480);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChiTietPhatSinh";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chi tiết khoản phát sinh lương";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmChiTietPhatSinh_FormClosing);
            this.Load += new System.EventHandler(this.FrmChiTietPhatSinh_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelMain)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpNhanVien.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit1View)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtKyCong.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboLoai.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenKhoan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spSoTien.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayPhatSinh.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayPhatSinh.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoChungTu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTrangThai.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLyDo.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnLuuNhap;
        private DevExpress.XtraBars.BarButtonItem btnDuyet;
        private DevExpress.XtraBars.BarButtonItem btnThuHoi;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.PanelControl panelMain;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblNhanVien;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpNhanVien;
        private DevExpress.XtraGrid.Views.Grid.GridView searchLookUpEdit1View;
        private DevExpress.XtraGrid.Columns.GridColumn colMaNV;
        private DevExpress.XtraGrid.Columns.GridColumn colHoTen;
        private DevExpress.XtraGrid.Columns.GridColumn colPhongBan;
        private DevExpress.XtraEditors.LabelControl lblKyCong;
        private DevExpress.XtraEditors.TextEdit txtKyCong;
        private DevExpress.XtraEditors.LabelControl lblLoai;
        private DevExpress.XtraEditors.ComboBoxEdit cboLoai;
        private DevExpress.XtraEditors.LabelControl lblTenKhoan;
        private DevExpress.XtraEditors.TextEdit txtTenKhoan;
        private DevExpress.XtraEditors.LabelControl lblSoTien;
        private DevExpress.XtraEditors.SpinEdit spSoTien;
        private DevExpress.XtraEditors.LabelControl lblNgayPhatSinh;
        private DevExpress.XtraEditors.DateEdit dtNgayPhatSinh;
        private DevExpress.XtraEditors.LabelControl lblSoChungTu;
        private DevExpress.XtraEditors.TextEdit txtSoChungTu;
        private DevExpress.XtraEditors.LabelControl lblTrangThai;
        private DevExpress.XtraEditors.TextEdit txtTrangThai;
        private DevExpress.XtraEditors.LabelControl lblLyDo;
        private DevExpress.XtraEditors.MemoEdit txtLyDo;
        private DevExpress.XtraEditors.LabelControl lblNote;
    }
}
