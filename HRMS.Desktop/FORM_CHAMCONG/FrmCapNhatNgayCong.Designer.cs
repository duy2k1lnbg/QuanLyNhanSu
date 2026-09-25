namespace QLyNSu.FORM_CHAMCONG
{
    partial class FrmCapNhatNgayCong
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCapNhatNgayCong));
            this.btnCapNhat = new DevExpress.XtraEditors.SimpleButton();
            this.cldNgayCong = new System.Windows.Forms.MonthCalendar();
            this.grChamCong = new DevExpress.XtraEditors.GroupControl();
            this.radioChamCong = new DevExpress.XtraEditors.RadioGroup();
            this.grTG = new DevExpress.XtraEditors.GroupControl();
            this.radioNgayNghi = new DevExpress.XtraEditors.RadioGroup();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.label1 = new System.Windows.Forms.Label();
            this.lblNgay = new System.Windows.Forms.Label();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblMANV = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblKyCong = new System.Windows.Forms.Label();
            this.grGioVaoRa = new DevExpress.XtraEditors.GroupControl();
            this.lblGioVao = new System.Windows.Forms.Label();
            this.timeEditGioVao = new DevExpress.XtraEditors.TimeEdit();
            this.lblGioRa = new System.Windows.Forms.Label();
            this.timeEditGioRa = new DevExpress.XtraEditors.TimeEdit();
            this.btnGioMacDinh = new DevExpress.XtraEditors.SimpleButton();
            this.lblTrangThaiBangCong = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grChamCong)).BeginInit();
            this.grChamCong.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radioChamCong.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grTG)).BeginInit();
            this.grTG.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radioNgayNghi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grGioVaoRa)).BeginInit();
            this.grGioVaoRa.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeEditGioVao.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeEditGioRa.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCapNhat
            // 
            this.btnCapNhat.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCapNhat.Appearance.Options.UseFont = true;
            this.btnCapNhat.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnCapNhat.ImageOptions.SvgImage")));
            this.btnCapNhat.Location = new System.Drawing.Point(340, 380);
            this.btnCapNhat.Name = "btnCapNhat";
            this.btnCapNhat.Size = new System.Drawing.Size(155, 46);
            this.btnCapNhat.TabIndex = 1;
            this.btnCapNhat.Text = "Cập Nhật";
            this.btnCapNhat.Click += new System.EventHandler(this.btnCapNhat_Click);
            // 
            // cldNgayCong
            // 
            this.cldNgayCong.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cldNgayCong.Location = new System.Drawing.Point(35, 12);
            this.cldNgayCong.Name = "cldNgayCong";
            this.cldNgayCong.TabIndex = 2;
            this.cldNgayCong.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.cldNgayCong_DateSelected);
            // 
            // grChamCong
            // 
            this.grChamCong.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grChamCong.AppearanceCaption.ForeColor = System.Drawing.Color.Blue;
            this.grChamCong.AppearanceCaption.Options.UseFont = true;
            this.grChamCong.AppearanceCaption.Options.UseForeColor = true;
            this.grChamCong.Controls.Add(this.radioChamCong);
            this.grChamCong.Location = new System.Drawing.Point(340, 10);
            this.grChamCong.Name = "grChamCong";
            this.grChamCong.Size = new System.Drawing.Size(325, 136);
            this.grChamCong.TabIndex = 3;
            this.grChamCong.Text = "Chấm Công";
            // 
            // radioChamCong
            // 
            this.radioChamCong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioChamCong.EditValue = "X";
            this.radioChamCong.Location = new System.Drawing.Point(2, 28);
            this.radioChamCong.Name = "radioChamCong";
            this.radioChamCong.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioChamCong.Properties.Appearance.Options.UseFont = true;
            this.radioChamCong.Properties.Columns = 2;
            this.radioChamCong.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem("X", "Đi làm"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("CD", "Ca đêm"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("P", "Nghỉ phép"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("V", "Vắng"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("VR", "Việc riêng"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("CT", "Công tác")});
            this.radioChamCong.Size = new System.Drawing.Size(321, 106);
            this.radioChamCong.TabIndex = 0;
            this.radioChamCong.SelectedIndexChanged += new System.EventHandler(this.radioChamCong_SelectedIndexChanged);
            // 
            // grTG
            // 
            this.grTG.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grTG.AppearanceCaption.ForeColor = System.Drawing.Color.Blue;
            this.grTG.AppearanceCaption.Options.UseFont = true;
            this.grTG.AppearanceCaption.Options.UseForeColor = true;
            this.grTG.Controls.Add(this.radioNgayNghi);
            this.grTG.Location = new System.Drawing.Point(340, 150);
            this.grTG.Name = "grTG";
            this.grTG.Size = new System.Drawing.Size(325, 94);
            this.grTG.TabIndex = 4;
            this.grTG.Text = "Thời Gian Nghỉ";
            // 
            // radioNgayNghi
            // 
            this.radioNgayNghi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioNgayNghi.EditValue = "KHONG";
            this.radioNgayNghi.Location = new System.Drawing.Point(2, 28);
            this.radioNgayNghi.Name = "radioNgayNghi";
            this.radioNgayNghi.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioNgayNghi.Properties.Appearance.Options.UseFont = true;
            this.radioNgayNghi.Properties.Columns = 2;
            this.radioNgayNghi.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem("KHONG", "Không nghỉ"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("NN", "Nguyên ngày"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("S", "Sáng"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem("C", "Chiều")});
            this.radioNgayNghi.Size = new System.Drawing.Size(321, 64);
            this.radioNgayNghi.TabIndex = 0;
            // 
            // grGioVaoRa
            // 
            this.grGioVaoRa.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grGioVaoRa.AppearanceCaption.ForeColor = System.Drawing.Color.Blue;
            this.grGioVaoRa.AppearanceCaption.Options.UseFont = true;
            this.grGioVaoRa.AppearanceCaption.Options.UseForeColor = true;
            this.grGioVaoRa.Controls.Add(this.lblGioVao);
            this.grGioVaoRa.Controls.Add(this.timeEditGioVao);
            this.grGioVaoRa.Controls.Add(this.lblGioRa);
            this.grGioVaoRa.Controls.Add(this.timeEditGioRa);
            this.grGioVaoRa.Controls.Add(this.btnGioMacDinh);
            this.grGioVaoRa.Controls.Add(this.lblTrangThaiBangCong);
            this.grGioVaoRa.Location = new System.Drawing.Point(340, 244);
            this.grGioVaoRa.Name = "grGioVaoRa";
            this.grGioVaoRa.Size = new System.Drawing.Size(325, 128);
            this.grGioVaoRa.TabIndex = 7;
            this.grGioVaoRa.Text = "Giờ Vào / Ra Thực Tế (TB_BANGCONG)";
            // 
            // lblGioVao
            // 
            this.lblGioVao.AutoSize = true;
            this.lblGioVao.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGioVao.Location = new System.Drawing.Point(10, 32);
            this.lblGioVao.Name = "lblGioVao";
            this.lblGioVao.Size = new System.Drawing.Size(65, 18);
            this.lblGioVao.TabIndex = 0;
            this.lblGioVao.Text = "Giờ vào:";
            // 
            // timeEditGioVao
            // 
            this.timeEditGioVao.EditValue = new System.DateTime(2026, 1, 1, 8, 0, 0, 0);
            this.timeEditGioVao.Location = new System.Drawing.Point(78, 28);
            this.timeEditGioVao.Name = "timeEditGioVao";
            this.timeEditGioVao.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeEditGioVao.Properties.Appearance.Options.UseFont = true;
            this.timeEditGioVao.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.timeEditGioVao.Properties.Mask.EditMask = "HH:mm";
            this.timeEditGioVao.Size = new System.Drawing.Size(82, 28);
            this.timeEditGioVao.TabIndex = 1;
            // 
            // lblGioRa
            // 
            this.lblGioRa.AutoSize = true;
            this.lblGioRa.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGioRa.Location = new System.Drawing.Point(168, 32);
            this.lblGioRa.Name = "lblGioRa";
            this.lblGioRa.Size = new System.Drawing.Size(56, 18);
            this.lblGioRa.TabIndex = 2;
            this.lblGioRa.Text = "Giờ ra:";
            // 
            // timeEditGioRa
            // 
            this.timeEditGioRa.EditValue = new System.DateTime(2026, 1, 1, 17, 0, 0, 0);
            this.timeEditGioRa.Location = new System.Drawing.Point(228, 28);
            this.timeEditGioRa.Name = "timeEditGioRa";
            this.timeEditGioRa.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeEditGioRa.Properties.Appearance.Options.UseFont = true;
            this.timeEditGioRa.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.timeEditGioRa.Properties.Mask.EditMask = "HH:mm";
            this.timeEditGioRa.Size = new System.Drawing.Size(82, 28);
            this.timeEditGioRa.TabIndex = 3;
            // 
            // btnGioMacDinh
            // 
            this.btnGioMacDinh.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGioMacDinh.Appearance.Options.UseFont = true;
            this.btnGioMacDinh.Location = new System.Drawing.Point(78, 62);
            this.btnGioMacDinh.Name = "btnGioMacDinh";
            this.btnGioMacDinh.Size = new System.Drawing.Size(232, 24);
            this.btnGioMacDinh.TabIndex = 4;
            this.btnGioMacDinh.Text = "Khôi phục giờ chuẩn (08:00 - 17:00)";
            this.btnGioMacDinh.Click += new System.EventHandler(this.btnGioMacDinh_Click);
            // 
            // lblTrangThaiBangCong
            // 
            this.lblTrangThaiBangCong.AutoSize = true;
            this.lblTrangThaiBangCong.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrangThaiBangCong.ForeColor = System.Drawing.Color.Green;
            this.lblTrangThaiBangCong.Location = new System.Drawing.Point(10, 96);
            this.lblTrangThaiBangCong.Name = "lblTrangThaiBangCong";
            this.lblTrangThaiBangCong.Size = new System.Drawing.Size(200, 17);
            this.lblTrangThaiBangCong.TabIndex = 5;
            this.lblTrangThaiBangCong.Text = "✓ Đang liên kết TB_BANGCONG...";
            // 
            // btnDong
            // 
            this.btnDong.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDong.Appearance.Options.UseFont = true;
            this.btnDong.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDong.ImageOptions.SvgImage")));
            this.btnDong.Location = new System.Drawing.Point(510, 380);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(155, 46);
            this.btnDong.TabIndex = 5;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // groupControl1
            // 
            this.groupControl1.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupControl1.AppearanceCaption.ForeColor = System.Drawing.Color.Blue;
            this.groupControl1.AppearanceCaption.Options.UseFont = true;
            this.groupControl1.AppearanceCaption.Options.UseForeColor = true;
            this.groupControl1.Controls.Add(this.label3);
            this.groupControl1.Controls.Add(this.lblKyCong);
            this.groupControl1.Controls.Add(this.label1);
            this.groupControl1.Controls.Add(this.lblNgay);
            this.groupControl1.Controls.Add(this.lblHoTen);
            this.groupControl1.Controls.Add(this.label2);
            this.groupControl1.Controls.Add(this.lblMANV);
            this.groupControl1.Location = new System.Drawing.Point(15, 200);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(310, 172);
            this.groupControl1.TabIndex = 6;
            this.groupControl1.Text = "Thông Tin";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 22);
            this.label1.TabIndex = 12;
            this.label1.Text = "ID:";
            // 
            // lblNgay
            // 
            this.lblNgay.AutoSize = true;
            this.lblNgay.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNgay.Location = new System.Drawing.Point(48, 133);
            this.lblNgay.Name = "lblNgay";
            this.lblNgay.Size = new System.Drawing.Size(56, 22);
            this.lblNgay.TabIndex = 11;
            this.lblNgay.Text = "Ngày";
            // 
            // lblHoTen
            // 
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoTen.Location = new System.Drawing.Point(94, 81);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(44, 22);
            this.lblHoTen.TabIndex = 10;
            this.lblHoTen.Text = "Tên";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 22);
            this.label2.TabIndex = 9;
            this.label2.Text = "Họ Tên:";
            // 
            // lblMANV
            // 
            this.lblMANV.AutoSize = true;
            this.lblMANV.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMANV.Location = new System.Drawing.Point(53, 39);
            this.lblMANV.Name = "lblMANV";
            this.lblMANV.Size = new System.Drawing.Size(64, 22);
            this.lblMANV.TabIndex = 8;
            this.lblMANV.Text = "MANV";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(144, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 22);
            this.label3.TabIndex = 14;
            this.label3.Text = "KC_";
            // 
            // lblKyCong
            // 
            this.lblKyCong.AutoSize = true;
            this.lblKyCong.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKyCong.Location = new System.Drawing.Point(190, 39);
            this.lblKyCong.Name = "lblKyCong";
            this.lblKyCong.Size = new System.Drawing.Size(64, 22);
            this.lblKyCong.TabIndex = 13;
            this.lblKyCong.Text = "MANV";
            // 
            // FrmCapNhatNgayCong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 440);
            this.Controls.Add(this.grGioVaoRa);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.btnDong);
            this.Controls.Add(this.grTG);
            this.Controls.Add(this.grChamCong);
            this.Controls.Add(this.cldNgayCong);
            this.Controls.Add(this.btnCapNhat);
            this.MinimizeBox = false;
            this.Name = "FrmCapNhatNgayCong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cập nhật ngày công";
            this.Load += new System.EventHandler(this.FrmCapNhatNgayCong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grChamCong)).EndInit();
            this.grChamCong.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radioChamCong.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grTG)).EndInit();
            this.grTG.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radioNgayNghi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grGioVaoRa)).EndInit();
            this.grGioVaoRa.ResumeLayout(false);
            this.grGioVaoRa.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeEditGioVao.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeEditGioRa.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnCapNhat;
        private System.Windows.Forms.MonthCalendar cldNgayCong;
        private DevExpress.XtraEditors.GroupControl grChamCong;
        private DevExpress.XtraEditors.RadioGroup radioChamCong;
        private DevExpress.XtraEditors.GroupControl grTG;
        private DevExpress.XtraEditors.RadioGroup radioNgayNghi;
        private DevExpress.XtraEditors.SimpleButton btnDong;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private System.Windows.Forms.Label lblNgay;
        private System.Windows.Forms.Label lblHoTen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblMANV;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblKyCong;
        private DevExpress.XtraEditors.GroupControl grGioVaoRa;
        private System.Windows.Forms.Label lblGioVao;
        private DevExpress.XtraEditors.TimeEdit timeEditGioVao;
        private System.Windows.Forms.Label lblGioRa;
        private DevExpress.XtraEditors.TimeEdit timeEditGioRa;
        private DevExpress.XtraEditors.SimpleButton btnGioMacDinh;
        private System.Windows.Forms.Label lblTrangThaiBangCong;
    }
}