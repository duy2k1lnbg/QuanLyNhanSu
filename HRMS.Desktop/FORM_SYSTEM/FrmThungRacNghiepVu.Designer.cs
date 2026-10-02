namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmThungRacNghiepVu
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
            this.btnKhoiPhuc = new DevExpress.XtraBars.BarButtonItem();
            this.btnXoaVinhVien = new DevExpress.XtraBars.BarButtonItem();
            this.btnLamMoi = new DevExpress.XtraBars.BarButtonItem();
            this.btnDong = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.panelControlTop = new DevExpress.XtraEditors.PanelControl();
            this.cboLoaiDoiTuong = new System.Windows.Forms.ComboBox();
            this.lblLoai = new DevExpress.XtraEditors.LabelControl();
            this.gcThungRac = new DevExpress.XtraGrid.GridControl();
            this.gvThungRac = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colObjectType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colObjectId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDisplayName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colEmployeeName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDeletedDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDeletedBy = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colReason = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlTop)).BeginInit();
            this.panelControlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcThungRac)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvThungRac)).BeginInit();
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
            this.btnKhoiPhuc,
            this.btnXoaVinhVien,
            this.btnLamMoi,
            this.btnDong});
            this.barManager1.MaxItemId = 4;
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
            new DevExpress.XtraBars.LinkPersistInfo(this.btnKhoiPhuc),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnXoaVinhVien),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnLamMoi),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnDong)});
            this.bar1.Text = "Tools";
            // 
            // btnKhoiPhuc
            // 
            this.btnKhoiPhuc.Caption = "Khôi Phục";
            this.btnKhoiPhuc.Id = 0;
            this.btnKhoiPhuc.Name = "btnKhoiPhuc";
            this.btnKhoiPhuc.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnKhoiPhuc_ItemClick);
            // 
            // btnXoaVinhVien
            // 
            this.btnXoaVinhVien.Caption = "Xóa Vĩnh Viễn";
            this.btnXoaVinhVien.Id = 1;
            this.btnXoaVinhVien.Name = "btnXoaVinhVien";
            this.btnXoaVinhVien.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnXoaVinhVien_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(950, 30);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 520);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(950, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 30);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 490);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(950, 30);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 490);
            // 
            // panelControlTop
            // 
            this.panelControlTop.Controls.Add(this.cboLoaiDoiTuong);
            this.panelControlTop.Controls.Add(this.lblLoai);
            this.panelControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlTop.Location = new System.Drawing.Point(0, 30);
            this.panelControlTop.Name = "panelControlTop";
            this.panelControlTop.Size = new System.Drawing.Size(950, 45);
            this.panelControlTop.TabIndex = 4;
            // 
            // cboLoaiDoiTuong
            // 
            this.cboLoaiDoiTuong.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoaiDoiTuong.Font = new System.Drawing.Font("Tahoma", 9F);
            this.cboLoaiDoiTuong.FormattingEnabled = true;
            this.cboLoaiDoiTuong.Location = new System.Drawing.Point(120, 11);
            this.cboLoaiDoiTuong.Name = "cboLoaiDoiTuong";
            this.cboLoaiDoiTuong.Size = new System.Drawing.Size(260, 22);
            this.cboLoaiDoiTuong.TabIndex = 1;
            this.cboLoaiDoiTuong.SelectedIndexChanged += new System.EventHandler(this.cboLoaiDoiTuong_SelectedIndexChanged);
            // 
            // lblLoai
            // 
            this.lblLoai.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.lblLoai.Appearance.Options.UseFont = true;
            this.lblLoai.Location = new System.Drawing.Point(15, 14);
            this.lblLoai.Name = "lblLoai";
            this.lblLoai.Size = new System.Drawing.Size(91, 14);
            this.lblLoai.TabIndex = 0;
            this.lblLoai.Text = "Loại đối tượng:";
            // 
            // gcThungRac
            // 
            this.gcThungRac.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcThungRac.Location = new System.Drawing.Point(0, 75);
            this.gcThungRac.MainView = this.gvThungRac;
            this.gcThungRac.Name = "gcThungRac";
            this.gcThungRac.Size = new System.Drawing.Size(950, 445);
            this.gcThungRac.TabIndex = 5;
            this.gcThungRac.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvThungRac});
            // 
            // gvThungRac
            // 
            this.gvThungRac.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colObjectType,
            this.colObjectId,
            this.colDisplayName,
            this.colEmployeeName,
            this.colDeletedDate,
            this.colDeletedBy,
            this.colReason});
            this.gvThungRac.GridControl = this.gcThungRac;
            this.gvThungRac.Name = "gvThungRac";
            this.gvThungRac.OptionsBehavior.Editable = false;
            this.gvThungRac.OptionsView.ShowAutoFilterRow = true;
            this.gvThungRac.OptionsView.ShowFooter = true;
            // 
            // colObjectType
            // 
            this.colObjectType.Caption = "Loại Đối Tượng";
            this.colObjectType.FieldName = "ObjectType";
            this.colObjectType.Name = "colObjectType";
            this.colObjectType.Visible = true;
            this.colObjectType.VisibleIndex = 0;
            this.colObjectType.Width = 110;
            // 
            // colObjectId
            // 
            this.colObjectId.Caption = "Mã Đối Tượng";
            this.colObjectId.FieldName = "ObjectId";
            this.colObjectId.Name = "colObjectId";
            this.colObjectId.Visible = true;
            this.colObjectId.VisibleIndex = 1;
            this.colObjectId.Width = 120;
            // 
            // colDisplayName
            // 
            this.colDisplayName.Caption = "Mô Tả / Nội Dung";
            this.colDisplayName.FieldName = "DisplayName";
            this.colDisplayName.Name = "colDisplayName";
            this.colDisplayName.Visible = true;
            this.colDisplayName.VisibleIndex = 2;
            this.colDisplayName.Width = 250;
            // 
            // colEmployeeName
            // 
            this.colEmployeeName.Caption = "Nhân Viên";
            this.colEmployeeName.FieldName = "EmployeeName";
            this.colEmployeeName.Name = "colEmployeeName";
            this.colEmployeeName.Visible = true;
            this.colEmployeeName.VisibleIndex = 3;
            this.colEmployeeName.Width = 150;
            // 
            // colDeletedDate
            // 
            this.colDeletedDate.Caption = "Ngày Xóa";
            this.colDeletedDate.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm";
            this.colDeletedDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colDeletedDate.FieldName = "DeletedDate";
            this.colDeletedDate.Name = "colDeletedDate";
            this.colDeletedDate.Visible = true;
            this.colDeletedDate.VisibleIndex = 4;
            this.colDeletedDate.Width = 120;
            // 
            // colDeletedBy
            // 
            this.colDeletedBy.Caption = "Người Xóa";
            this.colDeletedBy.FieldName = "DeletedBy";
            this.colDeletedBy.Name = "colDeletedBy";
            this.colDeletedBy.Visible = true;
            this.colDeletedBy.VisibleIndex = 5;
            this.colDeletedBy.Width = 90;
            // 
            // colReason
            // 
            this.colReason.Caption = "Ghi Chú Trạng Thái";
            this.colReason.FieldName = "Reason";
            this.colReason.Name = "colReason";
            this.colReason.Visible = true;
            this.colReason.VisibleIndex = 6;
            this.colReason.Width = 150;
            // 
            // FrmThungRacNghiepVu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 520);
            this.Controls.Add(this.gcThungRac);
            this.Controls.Add(this.panelControlTop);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FrmThungRacNghiepVu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thùng Rác Nghiệp Vụ - Quản Trị Dữ Liệu Đã Xóa";
            this.Load += new System.EventHandler(this.FrmThungRacNghiepVu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlTop)).EndInit();
            this.panelControlTop.ResumeLayout(false);
            this.panelControlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcThungRac)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvThungRac)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnKhoiPhuc;
        private DevExpress.XtraBars.BarButtonItem btnXoaVinhVien;
        private DevExpress.XtraBars.BarButtonItem btnLamMoi;
        private DevExpress.XtraBars.BarButtonItem btnDong;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.PanelControl panelControlTop;
        private System.Windows.Forms.ComboBox cboLoaiDoiTuong;
        private DevExpress.XtraEditors.LabelControl lblLoai;
        private DevExpress.XtraGrid.GridControl gcThungRac;
        private DevExpress.XtraGrid.Views.Grid.GridView gvThungRac;
        private DevExpress.XtraGrid.Columns.GridColumn colObjectType;
        private DevExpress.XtraGrid.Columns.GridColumn colObjectId;
        private DevExpress.XtraGrid.Columns.GridColumn colDisplayName;
        private DevExpress.XtraGrid.Columns.GridColumn colEmployeeName;
        private DevExpress.XtraGrid.Columns.GridColumn colDeletedDate;
        private DevExpress.XtraGrid.Columns.GridColumn colDeletedBy;
        private DevExpress.XtraGrid.Columns.GridColumn colReason;
    }
}
