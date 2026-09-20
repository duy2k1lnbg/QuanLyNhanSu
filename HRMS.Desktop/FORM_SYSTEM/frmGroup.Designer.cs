namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmGroup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGroup));
            this.pageGroup = new DevExpress.XtraTab.XtraTabControl();
            this.tapNhom = new DevExpress.XtraTab.XtraTabPage();
            this.lblChonNhom = new System.Windows.Forms.Label();
            this.cboNhom = new DevExpress.XtraEditors.LookUpEdit();
            this.btnNewGroup = new DevExpress.XtraEditors.SimpleButton();
            this.txtMoTa = new DevExpress.XtraEditors.TextEdit();
            this.txtTenNhom = new DevExpress.XtraEditors.TextEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tapThanhVien = new DevExpress.XtraTab.XtraTabPage();
            this.simpleButton4 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton3 = new DevExpress.XtraEditors.SimpleButton();
            this.gcThanhVien = new DevExpress.XtraGrid.GridControl();
            this.gvThanhVien = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.IDUSER = new DevExpress.XtraGrid.Columns.GridColumn();
            this.USERNAME = new DevExpress.XtraGrid.Columns.GridColumn();
            this.FULLNAME = new DevExpress.XtraGrid.Columns.GridColumn();
            this.btnLuu = new DevExpress.XtraEditors.SimpleButton();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            this.btnXoaNhom = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.pageGroup)).BeginInit();
            this.pageGroup.SuspendLayout();
            this.tapNhom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboNhom.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMoTa.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenNhom.Properties)).BeginInit();
            this.tapThanhVien.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcThanhVien)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvThanhVien)).BeginInit();
            this.SuspendLayout();
            // 
            // pageGroup
            // 
            this.pageGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.pageGroup.Location = new System.Drawing.Point(0, 0);
            this.pageGroup.Name = "pageGroup";
            this.pageGroup.SelectedTabPage = this.tapNhom;
            this.pageGroup.Size = new System.Drawing.Size(990, 445);
            this.pageGroup.TabIndex = 0;
            this.pageGroup.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tapNhom,
            this.tapThanhVien});
            // 
            // tapNhom
            // 
            this.tapNhom.Controls.Add(this.lblChonNhom);
            this.tapNhom.Controls.Add(this.cboNhom);
            this.tapNhom.Controls.Add(this.btnNewGroup);
            this.tapNhom.Controls.Add(this.txtMoTa);
            this.tapNhom.Controls.Add(this.txtTenNhom);
            this.tapNhom.Controls.Add(this.label2);
            this.tapNhom.Controls.Add(this.label1);
            this.tapNhom.Name = "tapNhom";
            this.tapNhom.Size = new System.Drawing.Size(988, 415);
            this.tapNhom.Text = "Thông Tin Nhóm";            // 
            // lblChonNhom
            // 
            this.lblChonNhom.AutoSize = false;
            this.lblChonNhom.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblChonNhom.Location = new System.Drawing.Point(145, 52);
            this.lblChonNhom.Name = "lblChonNhom";
            this.lblChonNhom.Size = new System.Drawing.Size(130, 25);
            this.lblChonNhom.TabIndex = 10;
            this.lblChonNhom.Text = "Chọn Nhóm:";
            this.lblChonNhom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboNhom
            // 
            this.cboNhom.Location = new System.Drawing.Point(280, 50);
            this.cboNhom.Name = "cboNhom";
            this.cboNhom.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboNhom.Properties.Appearance.Options.UseFont = true;
            this.cboNhom.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboNhom.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("USERNAME", "Tên nhóm"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("FULLNAME", "Mô tả")});
            this.cboNhom.Properties.DisplayMember = "USERNAME";
            this.cboNhom.Properties.NullText = "-- Chọn nhóm để xem/sửa --";
            this.cboNhom.Properties.ValueMember = "IDUSER";
            this.cboNhom.Size = new System.Drawing.Size(350, 30);
            this.cboNhom.TabIndex = 11;
            this.cboNhom.EditValueChanged += new System.EventHandler(this.cboNhom_EditValueChanged);
            // 
            // btnNewGroup
            // 
            this.btnNewGroup.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNewGroup.Appearance.Options.UseFont = true;
            this.btnNewGroup.Location = new System.Drawing.Point(645, 46);
            this.btnNewGroup.Name = "btnNewGroup";
            this.btnNewGroup.Size = new System.Drawing.Size(155, 38);
            this.btnNewGroup.TabIndex = 12;
            this.btnNewGroup.Text = "Tạo nhóm mới";
            this.btnNewGroup.Click += new System.EventHandler(this.btnNewGroup_Click);
            // 
            // txtMoTa
            // 
            this.txtMoTa.Location = new System.Drawing.Point(280, 142);
            this.txtMoTa.Name = "txtMoTa";
            this.txtMoTa.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMoTa.Properties.Appearance.Options.UseFont = true;
            this.txtMoTa.Size = new System.Drawing.Size(350, 30);
            this.txtMoTa.TabIndex = 5;
            // 
            // txtTenNhom
            // 
            this.txtTenNhom.Location = new System.Drawing.Point(280, 96);
            this.txtTenNhom.Name = "txtTenNhom";
            this.txtTenNhom.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTenNhom.Properties.Appearance.Options.UseFont = true;
            this.txtTenNhom.Size = new System.Drawing.Size(350, 30);
            this.txtTenNhom.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = false;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(145, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Mô Tả:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = false;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(145, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tên Nhóm:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tapThanhVien
            // 
            this.tapThanhVien.Controls.Add(this.simpleButton4);
            this.tapThanhVien.Controls.Add(this.simpleButton3);
            this.tapThanhVien.Controls.Add(this.gcThanhVien);
            this.tapThanhVien.Name = "tapThanhVien";
            this.tapThanhVien.Size = new System.Drawing.Size(988, 415);
            this.tapThanhVien.Text = "Thành Viên";
            // 
            // simpleButton4
            // 
            this.simpleButton4.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.simpleButton4.Appearance.Options.UseFont = true;
            this.simpleButton4.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton4.ImageOptions.SvgImage")));
            this.simpleButton4.Location = new System.Drawing.Point(854, 360);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(123, 40);
            this.simpleButton4.TabIndex = 3;
            this.simpleButton4.Text = "Xoá";
            // 
            // simpleButton3
            // 
            this.simpleButton3.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.simpleButton3.Appearance.Options.UseFont = true;
            this.simpleButton3.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton3.ImageOptions.SvgImage")));
            this.simpleButton3.Location = new System.Drawing.Point(673, 360);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(123, 40);
            this.simpleButton3.TabIndex = 2;
            this.simpleButton3.Text = "Thêm";
            // 
            // gcThanhVien
            // 
            this.gcThanhVien.Dock = System.Windows.Forms.DockStyle.Top;
            this.gcThanhVien.Location = new System.Drawing.Point(0, 0);
            this.gcThanhVien.MainView = this.gvThanhVien;
            this.gcThanhVien.Name = "gcThanhVien";
            this.gcThanhVien.Size = new System.Drawing.Size(988, 274);
            this.gcThanhVien.TabIndex = 0;
            this.gcThanhVien.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvThanhVien});
            // 
            // gvThanhVien
            // 
            this.gvThanhVien.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.IDUSER,
            this.USERNAME,
            this.FULLNAME});
            this.gvThanhVien.GridControl = this.gcThanhVien;
            this.gvThanhVien.Name = "gvThanhVien";
            this.gvThanhVien.OptionsView.ShowGroupPanel = false;
            // 
            // IDUSER
            // 
            this.IDUSER.Caption = "ID";
            this.IDUSER.FieldName = "IDUSER";
            this.IDUSER.MinWidth = 25;
            this.IDUSER.Name = "IDUSER";
            this.IDUSER.Visible = true;
            this.IDUSER.VisibleIndex = 0;
            this.IDUSER.Width = 94;
            // 
            // USERNAME
            // 
            this.USERNAME.Caption = "USERNAME";
            this.USERNAME.FieldName = "USERNAME";
            this.USERNAME.MinWidth = 25;
            this.USERNAME.Name = "USERNAME";
            this.USERNAME.Visible = true;
            this.USERNAME.VisibleIndex = 1;
            this.USERNAME.Width = 94;
            // 
            // FULLNAME
            // 
            this.FULLNAME.Caption = "FULLNAME";
            this.FULLNAME.FieldName = "FULLNAME";
            this.FULLNAME.MinWidth = 25;
            this.FULLNAME.Name = "FULLNAME";
            this.FULLNAME.Visible = true;
            this.FULLNAME.VisibleIndex = 2;
            this.FULLNAME.Width = 94;
            // 
            // btnXoaNhom
            // 
            this.btnXoaNhom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnXoaNhom.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnXoaNhom.Appearance.Options.UseFont = true;
            this.btnXoaNhom.Location = new System.Drawing.Point(20, 451);
            this.btnXoaNhom.Name = "btnXoaNhom";
            this.btnXoaNhom.Size = new System.Drawing.Size(160, 40);
            this.btnXoaNhom.TabIndex = 13;
            this.btnXoaNhom.Text = "Xóa nhóm";
            this.btnXoaNhom.Click += new System.EventHandler(this.btnXoaNhom_Click);
            // 
            // btnLuu
            // 
            this.btnLuu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLuu.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLuu.Appearance.Options.UseFont = true;
            this.btnLuu.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnLuu.ImageOptions.SvgImage")));
            this.btnLuu.Location = new System.Drawing.Point(646, 451);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(160, 40);
            this.btnLuu.TabIndex = 1;
            this.btnLuu.Text = "Lưu";
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // btnDong
            // 
            this.btnDong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDong.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnDong.Appearance.Options.UseFont = true;
            this.btnDong.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDong.ImageOptions.SvgImage")));
            this.btnDong.Location = new System.Drawing.Point(818, 451);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(160, 40);
            this.btnDong.TabIndex = 2;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            //
            // FrmGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 503);
            this.Controls.Add(this.btnXoaNhom);
            this.Controls.Add(this.btnDong);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.pageGroup);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmGroup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nhóm Người Dùng";
            this.Load += new System.EventHandler(this.frmGroup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pageGroup)).EndInit();
            this.pageGroup.ResumeLayout(false);
            this.tapNhom.ResumeLayout(false);
            this.tapNhom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboNhom.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMoTa.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenNhom.Properties)).EndInit();
            this.tapThanhVien.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcThanhVien)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvThanhVien)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl pageGroup;
        private DevExpress.XtraTab.XtraTabPage tapNhom;
        private DevExpress.XtraTab.XtraTabPage tapThanhVien;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit txtMoTa;
        private DevExpress.XtraEditors.TextEdit txtTenNhom;
        private DevExpress.XtraEditors.SimpleButton btnLuu;
        private DevExpress.XtraEditors.SimpleButton btnDong;
        private DevExpress.XtraEditors.SimpleButton simpleButton3;
        private DevExpress.XtraGrid.GridControl gcThanhVien;
        private DevExpress.XtraGrid.Views.Grid.GridView gvThanhVien;
        private DevExpress.XtraEditors.SimpleButton simpleButton4;
        private DevExpress.XtraGrid.Columns.GridColumn IDUSER;
        private DevExpress.XtraGrid.Columns.GridColumn USERNAME;
        private DevExpress.XtraGrid.Columns.GridColumn FULLNAME;
        private System.Windows.Forms.Label lblChonNhom;
        private DevExpress.XtraEditors.LookUpEdit cboNhom;
        private DevExpress.XtraEditors.SimpleButton btnNewGroup;
        private DevExpress.XtraEditors.SimpleButton btnXoaNhom;
    }
}