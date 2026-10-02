namespace QLyNSu.FORM_NHANSU
{
    partial class FrmPhuLucHopDong
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
            this.btnLuuPhuLuc = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.groupHopDong = new DevExpress.XtraEditors.GroupControl();
            this.txtThoiHanHD = new DevExpress.XtraEditors.TextEdit();
            this.lblThoiHanHD = new DevExpress.XtraEditors.LabelControl();
            this.txtNgayKyHD = new DevExpress.XtraEditors.TextEdit();
            this.lblNgayKyHD = new DevExpress.XtraEditors.LabelControl();
            this.txtNhanVien = new DevExpress.XtraEditors.TextEdit();
            this.lblNhanVien = new DevExpress.XtraEditors.LabelControl();
            this.txtSoHD = new DevExpress.XtraEditors.TextEdit();
            this.lblSoHD = new DevExpress.XtraEditors.LabelControl();
            this.groupPhuLuc = new DevExpress.XtraEditors.GroupControl();
            this.txtNoiDung = new DevExpress.XtraEditors.TextEdit();
            this.lblNoiDung = new DevExpress.XtraEditors.LabelControl();
            this.txtCanCu = new DevExpress.XtraEditors.TextEdit();
            this.lblCanCu = new DevExpress.XtraEditors.LabelControl();
            this.dtDenNgay = new DevExpress.XtraEditors.DateEdit();
            this.lblDenNgay = new DevExpress.XtraEditors.LabelControl();
            this.dtNgayHieuLuc = new DevExpress.XtraEditors.DateEdit();
            this.lblNgayHieuLuc = new DevExpress.XtraEditors.LabelControl();
            this.dtNgayKy = new DevExpress.XtraEditors.DateEdit();
            this.lblNgayKy = new DevExpress.XtraEditors.LabelControl();
            this.txtSoPhuLuc = new DevExpress.XtraEditors.TextEdit();
            this.lblSoPhuLuc = new DevExpress.XtraEditors.LabelControl();
            this.groupDieuKhoan = new DevExpress.XtraEditors.GroupControl();
            this.gcDieuKhoan = new DevExpress.XtraGrid.GridControl();
            this.gvDieuKhoan = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colKhoanMuc = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCachHuong = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMucCu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMucMoi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colChenhLech = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colGhiChu = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupHopDong)).BeginInit();
            this.groupHopDong.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtThoiHanHD.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNgayKyHD.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNhanVien.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoHD.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupPhuLuc)).BeginInit();
            this.groupPhuLuc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoiDung.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCanCu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenNgay.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenNgay.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHieuLuc.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHieuLuc.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayKy.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayKy.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoPhuLuc.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupDieuKhoan)).BeginInit();
            this.groupDieuKhoan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcDieuKhoan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDieuKhoan)).BeginInit();
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
            this.btnLuuPhuLuc,
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
            new DevExpress.XtraBars.LinkPersistInfo(this.btnLuuPhuLuc),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDong)});
            this.bar1.Text = "Tools";
            // 
            // btnLuuPhuLuc
            // 
            this.btnLuuPhuLuc.Caption = "Lưu && Áp Dụng Hiệu Lực";
            this.btnLuuPhuLuc.Id = 0;
            this.btnLuuPhuLuc.Name = "btnLuuPhuLuc";
            this.btnLuuPhuLuc.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLuuPhuLuc_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(920, 29);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 560);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(920, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 29);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 531);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(920, 29);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 531);
            // 
            // groupHopDong
            // 
            this.groupHopDong.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.groupHopDong.AppearanceCaption.Options.UseFont = true;
            this.groupHopDong.Controls.Add(this.txtThoiHanHD);
            this.groupHopDong.Controls.Add(this.lblThoiHanHD);
            this.groupHopDong.Controls.Add(this.txtNgayKyHD);
            this.groupHopDong.Controls.Add(this.lblNgayKyHD);
            this.groupHopDong.Controls.Add(this.txtNhanVien);
            this.groupHopDong.Controls.Add(this.lblNhanVien);
            this.groupHopDong.Controls.Add(this.txtSoHD);
            this.groupHopDong.Controls.Add(this.lblSoHD);
            this.groupHopDong.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupHopDong.Location = new System.Drawing.Point(0, 29);
            this.groupHopDong.Name = "groupHopDong";
            this.groupHopDong.Size = new System.Drawing.Size(920, 80);
            this.groupHopDong.TabIndex = 4;
            this.groupHopDong.Text = "Thông Tin Hợp Đồng Gốc (Chỉ đọc)";
            // 
            // txtThoiHanHD
            // 
            this.txtThoiHanHD.Location = new System.Drawing.Point(740, 36);
            this.txtThoiHanHD.Name = "txtThoiHanHD";
            this.txtThoiHanHD.Properties.ReadOnly = true;
            this.txtThoiHanHD.Size = new System.Drawing.Size(150, 22);
            this.txtThoiHanHD.TabIndex = 7;
            // 
            // lblThoiHanHD
            // 
            this.lblThoiHanHD.Location = new System.Drawing.Point(670, 39);
            this.lblThoiHanHD.Name = "lblThoiHanHD";
            this.lblThoiHanHD.Size = new System.Drawing.Size(56, 16);
            this.lblThoiHanHD.TabIndex = 6;
            this.lblThoiHanHD.Text = "Thời hạn:";
            // 
            // txtNgayKyHD
            // 
            this.txtNgayKyHD.Location = new System.Drawing.Point(520, 36);
            this.txtNgayKyHD.Name = "txtNgayKyHD";
            this.txtNgayKyHD.Properties.ReadOnly = true;
            this.txtNgayKyHD.Size = new System.Drawing.Size(130, 22);
            this.txtNgayKyHD.TabIndex = 5;
            // 
            // lblNgayKyHD
            // 
            this.lblNgayKyHD.Location = new System.Drawing.Point(450, 39);
            this.lblNgayKyHD.Name = "lblNgayKyHD";
            this.lblNgayKyHD.Size = new System.Drawing.Size(51, 16);
            this.lblNgayKyHD.TabIndex = 4;
            this.lblNgayKyHD.Text = "Ngày ký:";
            // 
            // txtNhanVien
            // 
            this.txtNhanVien.Location = new System.Drawing.Point(260, 36);
            this.txtNhanVien.Name = "txtNhanVien";
            this.txtNhanVien.Properties.ReadOnly = true;
            this.txtNhanVien.Size = new System.Drawing.Size(170, 22);
            this.txtNhanVien.TabIndex = 3;
            // 
            // lblNhanVien
            // 
            this.lblNhanVien.Location = new System.Drawing.Point(185, 39);
            this.lblNhanVien.Name = "lblNhanVien";
            this.lblNhanVien.Size = new System.Drawing.Size(62, 16);
            this.lblNhanVien.TabIndex = 2;
            this.lblNhanVien.Text = "Nhân viên:";
            // 
            // txtSoHD
            // 
            this.txtSoHD.Location = new System.Drawing.Point(70, 36);
            this.txtSoHD.Name = "txtSoHD";
            this.txtSoHD.Properties.ReadOnly = true;
            this.txtSoHD.Size = new System.Drawing.Size(100, 22);
            this.txtSoHD.TabIndex = 1;
            // 
            // lblSoHD
            // 
            this.lblSoHD.Location = new System.Drawing.Point(15, 39);
            this.lblSoHD.Name = "lblSoHD";
            this.lblSoHD.Size = new System.Drawing.Size(43, 16);
            this.lblSoHD.TabIndex = 0;
            this.lblSoHD.Text = "Số HĐ:";
            // 
            // groupPhuLuc
            // 
            this.groupPhuLuc.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.groupPhuLuc.AppearanceCaption.Options.UseFont = true;
            this.groupPhuLuc.Controls.Add(this.txtNoiDung);
            this.groupPhuLuc.Controls.Add(this.lblNoiDung);
            this.groupPhuLuc.Controls.Add(this.txtCanCu);
            this.groupPhuLuc.Controls.Add(this.lblCanCu);
            this.groupPhuLuc.Controls.Add(this.dtDenNgay);
            this.groupPhuLuc.Controls.Add(this.lblDenNgay);
            this.groupPhuLuc.Controls.Add(this.dtNgayHieuLuc);
            this.groupPhuLuc.Controls.Add(this.lblNgayHieuLuc);
            this.groupPhuLuc.Controls.Add(this.dtNgayKy);
            this.groupPhuLuc.Controls.Add(this.lblNgayKy);
            this.groupPhuLuc.Controls.Add(this.txtSoPhuLuc);
            this.groupPhuLuc.Controls.Add(this.lblSoPhuLuc);
            this.groupPhuLuc.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupPhuLuc.Location = new System.Drawing.Point(0, 109);
            this.groupPhuLuc.Name = "groupPhuLuc";
            this.groupPhuLuc.Size = new System.Drawing.Size(920, 120);
            this.groupPhuLuc.TabIndex = 5;
            this.groupPhuLuc.Text = "Thông Tin Phụ Lục && Hiệu Lực Mới";
            // 
            // txtNoiDung
            // 
            this.txtNoiDung.Location = new System.Drawing.Point(520, 75);
            this.txtNoiDung.Name = "txtNoiDung";
            this.txtNoiDung.Size = new System.Drawing.Size(370, 22);
            this.txtNoiDung.TabIndex = 11;
            // 
            // lblNoiDung
            // 
            this.lblNoiDung.Location = new System.Drawing.Point(450, 78);
            this.lblNoiDung.Name = "lblNoiDung";
            this.lblNoiDung.Size = new System.Drawing.Size(56, 16);
            this.lblNoiDung.TabIndex = 10;
            this.lblNoiDung.Text = "Nội dung:";
            // 
            // txtCanCu
            // 
            this.txtCanCu.Location = new System.Drawing.Point(100, 75);
            this.txtCanCu.Name = "txtCanCu";
            this.txtCanCu.Size = new System.Drawing.Size(330, 22);
            this.txtCanCu.TabIndex = 9;
            // 
            // lblCanCu
            // 
            this.lblCanCu.Location = new System.Drawing.Point(15, 78);
            this.lblCanCu.Name = "lblCanCu";
            this.lblCanCu.Size = new System.Drawing.Size(46, 16);
            this.lblCanCu.TabIndex = 8;
            this.lblCanCu.Text = "Căn cứ:";
            // 
            // dtDenNgay
            // 
            this.dtDenNgay.EditValue = null;
            this.dtDenNgay.Location = new System.Drawing.Point(760, 36);
            this.dtDenNgay.Name = "dtDenNgay";
            this.dtDenNgay.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDenNgay.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDenNgay.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.dtDenNgay.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtDenNgay.Properties.EditFormat.FormatString = "dd/MM/yyyy";
            this.dtDenNgay.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtDenNgay.Properties.Mask.EditMask = "dd/MM/yyyy";
            this.dtDenNgay.Size = new System.Drawing.Size(130, 22);
            this.dtDenNgay.TabIndex = 7;
            // 
            // lblDenNgay
            // 
            this.lblDenNgay.Location = new System.Drawing.Point(680, 39);
            this.lblDenNgay.Name = "lblDenNgay";
            this.lblDenNgay.Size = new System.Drawing.Size(59, 16);
            this.lblDenNgay.TabIndex = 6;
            this.lblDenNgay.Text = "Đến ngày:";
            // 
            // dtNgayHieuLuc
            // 
            this.dtNgayHieuLuc.EditValue = null;
            this.dtNgayHieuLuc.Location = new System.Drawing.Point(530, 36);
            this.dtNgayHieuLuc.Name = "dtNgayHieuLuc";
            this.dtNgayHieuLuc.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayHieuLuc.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayHieuLuc.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.dtNgayHieuLuc.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayHieuLuc.Properties.EditFormat.FormatString = "dd/MM/yyyy";
            this.dtNgayHieuLuc.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayHieuLuc.Properties.Mask.EditMask = "dd/MM/yyyy";
            this.dtNgayHieuLuc.Size = new System.Drawing.Size(130, 22);
            this.dtNgayHieuLuc.TabIndex = 5;
            // 
            // lblNgayHieuLuc
            // 
            this.lblNgayHieuLuc.Location = new System.Drawing.Point(440, 39);
            this.lblNgayHieuLuc.Name = "lblNgayHieuLuc";
            this.lblNgayHieuLuc.Size = new System.Drawing.Size(81, 16);
            this.lblNgayHieuLuc.TabIndex = 4;
            this.lblNgayHieuLuc.Text = "Hiệu lực từ (*):";
            // 
            // dtNgayKy
            // 
            this.dtNgayKy.EditValue = null;
            this.dtNgayKy.Location = new System.Drawing.Point(290, 36);
            this.dtNgayKy.Name = "dtNgayKy";
            this.dtNgayKy.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayKy.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtNgayKy.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.dtNgayKy.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayKy.Properties.EditFormat.FormatString = "dd/MM/yyyy";
            this.dtNgayKy.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtNgayKy.Properties.Mask.EditMask = "dd/MM/yyyy";
            this.dtNgayKy.Size = new System.Drawing.Size(130, 22);
            this.dtNgayKy.TabIndex = 3;
            // 
            // lblNgayKy
            // 
            this.lblNgayKy.Location = new System.Drawing.Point(225, 39);
            this.lblNgayKy.Name = "lblNgayKy";
            this.lblNgayKy.Size = new System.Drawing.Size(51, 16);
            this.lblNgayKy.TabIndex = 2;
            this.lblNgayKy.Text = "Ngày ký:";
            // 
            // txtSoPhuLuc
            // 
            this.txtSoPhuLuc.Location = new System.Drawing.Point(100, 36);
            this.txtSoPhuLuc.Name = "txtSoPhuLuc";
            this.txtSoPhuLuc.Size = new System.Drawing.Size(110, 22);
            this.txtSoPhuLuc.TabIndex = 1;
            // 
            // lblSoPhuLuc
            // 
            this.lblSoPhuLuc.Location = new System.Drawing.Point(15, 39);
            this.lblSoPhuLuc.Name = "lblSoPhuLuc";
            this.lblSoPhuLuc.Size = new System.Drawing.Size(76, 16);
            this.lblSoPhuLuc.TabIndex = 0;
            this.lblSoPhuLuc.Text = "Số phụ lục (*):";
            // 
            // groupDieuKhoan
            // 
            this.groupDieuKhoan.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.groupDieuKhoan.AppearanceCaption.Options.UseFont = true;
            this.groupDieuKhoan.Controls.Add(this.gcDieuKhoan);
            this.groupDieuKhoan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupDieuKhoan.Location = new System.Drawing.Point(0, 229);
            this.groupDieuKhoan.Name = "groupDieuKhoan";
            this.groupDieuKhoan.Size = new System.Drawing.Size(920, 331);
            this.groupDieuKhoan.TabIndex = 6;
            this.groupDieuKhoan.Text = "Điều Chỉnh Mức Lương && Phụ Cấp Theo Hiệu Lực Mới";
            // 
            // gcDieuKhoan
            // 
            this.gcDieuKhoan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcDieuKhoan.Location = new System.Drawing.Point(2, 28);
            this.gcDieuKhoan.MainView = this.gvDieuKhoan;
            this.gcDieuKhoan.MenuManager = this.barManager1;
            this.gcDieuKhoan.Name = "gcDieuKhoan";
            this.gcDieuKhoan.Size = new System.Drawing.Size(916, 301);
            this.gcDieuKhoan.TabIndex = 0;
            this.gcDieuKhoan.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvDieuKhoan});
            // 
            // gvDieuKhoan
            // 
            this.gvDieuKhoan.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colKhoanMuc,
            this.colCachHuong,
            this.colMucCu,
            this.colMucMoi,
            this.colChenhLech,
            this.colGhiChu});
            this.gvDieuKhoan.GridControl = this.gcDieuKhoan;
            this.gvDieuKhoan.Name = "gvDieuKhoan";
            this.gvDieuKhoan.OptionsView.ShowGroupPanel = false;
            // 
            // colKhoanMuc
            // 
            this.colKhoanMuc.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.colKhoanMuc.AppearanceHeader.Options.UseFont = true;
            this.colKhoanMuc.Caption = "Khoản Mục Điều Chỉnh";
            this.colKhoanMuc.FieldName = "KhoanMuc";
            this.colKhoanMuc.Name = "colKhoanMuc";
            this.colKhoanMuc.OptionsColumn.AllowEdit = false;
            this.colKhoanMuc.Visible = true;
            this.colKhoanMuc.VisibleIndex = 0;
            this.colKhoanMuc.Width = 220;
            // 
            // colCachHuong
            // 
            this.colCachHuong.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.colCachHuong.AppearanceHeader.Options.UseFont = true;
            this.colCachHuong.Caption = "Cách Hưởng";
            this.colCachHuong.FieldName = "CachHuong";
            this.colCachHuong.Name = "colCachHuong";
            this.colCachHuong.OptionsColumn.AllowEdit = false;
            this.colCachHuong.Visible = true;
            this.colCachHuong.VisibleIndex = 1;
            this.colCachHuong.Width = 120;
            // 
            // colMucCu
            // 
            this.colMucCu.AppearanceCell.Options.UseTextOptions = true;
            this.colMucCu.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colMucCu.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.colMucCu.AppearanceHeader.Options.UseFont = true;
            this.colMucCu.AppearanceHeader.Options.UseTextOptions = true;
            this.colMucCu.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colMucCu.Caption = "Mức Cũ (VNĐ)";
            this.colMucCu.DisplayFormat.FormatString = "N0";
            this.colMucCu.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colMucCu.FieldName = "MucCu";
            this.colMucCu.Name = "colMucCu";
            this.colMucCu.OptionsColumn.AllowEdit = false;
            this.colMucCu.Visible = true;
            this.colMucCu.VisibleIndex = 2;
            this.colMucCu.Width = 140;
            // 
            // colMucMoi
            // 
            this.colMucMoi.AppearanceCell.Options.UseTextOptions = true;
            this.colMucMoi.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colMucMoi.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.colMucMoi.AppearanceHeader.Options.UseFont = true;
            this.colMucMoi.AppearanceHeader.Options.UseTextOptions = true;
            this.colMucMoi.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colMucMoi.Caption = "Mức Mới (VNĐ)";
            this.colMucMoi.DisplayFormat.FormatString = "N0";
            this.colMucMoi.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colMucMoi.FieldName = "MucMoi";
            this.colMucMoi.Name = "colMucMoi";
            this.colMucMoi.Visible = true;
            this.colMucMoi.VisibleIndex = 3;
            this.colMucMoi.Width = 140;
            // 
            // colChenhLech
            // 
            this.colChenhLech.AppearanceCell.Options.UseTextOptions = true;
            this.colChenhLech.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colChenhLech.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.colChenhLech.AppearanceHeader.Options.UseFont = true;
            this.colChenhLech.AppearanceHeader.Options.UseTextOptions = true;
            this.colChenhLech.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colChenhLech.Caption = "Chênh Lệch (+/-)";
            this.colChenhLech.DisplayFormat.FormatString = "N0";
            this.colChenhLech.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colChenhLech.FieldName = "ChenhLech";
            this.colChenhLech.Name = "colChenhLech";
            this.colChenhLech.OptionsColumn.AllowEdit = false;
            this.colChenhLech.Visible = true;
            this.colChenhLech.VisibleIndex = 4;
            this.colChenhLech.Width = 140;
            // 
            // colGhiChu
            // 
            this.colGhiChu.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.colGhiChu.AppearanceHeader.Options.UseFont = true;
            this.colGhiChu.Caption = "Ghi Chú";
            this.colGhiChu.FieldName = "GhiChu";
            this.colGhiChu.Name = "colGhiChu";
            this.colGhiChu.Visible = true;
            this.colGhiChu.VisibleIndex = 5;
            this.colGhiChu.Width = 150;
            // 
            // FrmPhuLucHopDong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 560);
            this.Controls.Add(this.groupDieuKhoan);
            this.Controls.Add(this.groupPhuLuc);
            this.Controls.Add(this.groupHopDong);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPhuLucHopDong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Phụ Lục Hợp Đồng Lao Động - Điều Chỉnh Mức Lương & Hiệu Lực";
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupHopDong)).EndInit();
            this.groupHopDong.ResumeLayout(false);
            this.groupHopDong.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtThoiHanHD.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNgayKyHD.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNhanVien.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoHD.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupPhuLuc)).EndInit();
            this.groupPhuLuc.ResumeLayout(false);
            this.groupPhuLuc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoiDung.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCanCu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenNgay.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenNgay.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHieuLuc.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayHieuLuc.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayKy.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtNgayKy.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoPhuLuc.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupDieuKhoan)).EndInit();
            this.groupDieuKhoan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcDieuKhoan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDieuKhoan)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnLuuPhuLuc;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.GroupControl groupHopDong;
        private DevExpress.XtraEditors.TextEdit txtThoiHanHD;
        private DevExpress.XtraEditors.LabelControl lblThoiHanHD;
        private DevExpress.XtraEditors.TextEdit txtNgayKyHD;
        private DevExpress.XtraEditors.LabelControl lblNgayKyHD;
        private DevExpress.XtraEditors.TextEdit txtNhanVien;
        private DevExpress.XtraEditors.LabelControl lblNhanVien;
        private DevExpress.XtraEditors.TextEdit txtSoHD;
        private DevExpress.XtraEditors.LabelControl lblSoHD;
        private DevExpress.XtraEditors.GroupControl groupPhuLuc;
        private DevExpress.XtraEditors.TextEdit txtNoiDung;
        private DevExpress.XtraEditors.LabelControl lblNoiDung;
        private DevExpress.XtraEditors.TextEdit txtCanCu;
        private DevExpress.XtraEditors.LabelControl lblCanCu;
        private DevExpress.XtraEditors.DateEdit dtDenNgay;
        private DevExpress.XtraEditors.LabelControl lblDenNgay;
        private DevExpress.XtraEditors.DateEdit dtNgayHieuLuc;
        private DevExpress.XtraEditors.LabelControl lblNgayHieuLuc;
        private DevExpress.XtraEditors.DateEdit dtNgayKy;
        private DevExpress.XtraEditors.LabelControl lblNgayKy;
        private DevExpress.XtraEditors.TextEdit txtSoPhuLuc;
        private DevExpress.XtraEditors.LabelControl lblSoPhuLuc;
        private DevExpress.XtraEditors.GroupControl groupDieuKhoan;
        private DevExpress.XtraGrid.GridControl gcDieuKhoan;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDieuKhoan;
        private DevExpress.XtraGrid.Columns.GridColumn colKhoanMuc;
        private DevExpress.XtraGrid.Columns.GridColumn colCachHuong;
        private DevExpress.XtraGrid.Columns.GridColumn colMucCu;
        private DevExpress.XtraGrid.Columns.GridColumn colMucMoi;
        private DevExpress.XtraGrid.Columns.GridColumn colChenhLech;
        private DevExpress.XtraGrid.Columns.GridColumn colGhiChu;
    }
}
