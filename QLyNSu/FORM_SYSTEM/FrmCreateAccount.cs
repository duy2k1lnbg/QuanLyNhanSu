using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public class FrmCreateAccount : DevExpress.XtraEditors.XtraForm
    {
        private System.ComponentModel.IContainer components = null;
        private DevExpress.XtraTab.XtraTabControl pageAccount;
        private DevExpress.XtraTab.XtraTabPage tapTaiKhoan;
        private DevExpress.XtraTab.XtraTabPage tapNhomCuaUser;
        private DevExpress.XtraEditors.TextEdit txtFullName;
        private DevExpress.XtraEditors.TextEdit txtUsername;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.CheckEdit chkDisabled;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        
        private DevExpress.XtraEditors.SimpleButton btnRemoveGroup;
        private DevExpress.XtraEditors.SimpleButton btnThemNhom;
        private DevExpress.XtraGrid.GridControl gcNhom;
        private DevExpress.XtraGrid.Views.Grid.GridView gvNhom;
        
        private DevExpress.XtraEditors.SimpleButton btnLuu;
        private DevExpress.XtraEditors.SimpleButton btnDong;
        private DevExpress.XtraEditors.SimpleButton btnXoaTK;

        private DevExpress.XtraEditors.LookUpEdit cboTaiKhoan;
        private DevExpress.XtraEditors.SimpleButton btnNewAccount;
        private System.Windows.Forms.Label lblChonTaiKhoan;

        private MyEntities db = new MyEntities();
        private SYS_USER _sysUser = new SYS_USER();
        private TB_SYS_USER _user;
        private bool _them = true;
        private bool _dataSaved = false;

        public FrmCreateAccount()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pageAccount = new DevExpress.XtraTab.XtraTabControl();
            this.tapTaiKhoan = new DevExpress.XtraTab.XtraTabPage();
            this.lblChonTaiKhoan = new System.Windows.Forms.Label();
            this.cboTaiKhoan = new DevExpress.XtraEditors.LookUpEdit();
            this.btnNewAccount = new DevExpress.XtraEditors.SimpleButton();
            this.btnXoaTK = new DevExpress.XtraEditors.SimpleButton();
            this.txtFullName = new DevExpress.XtraEditors.TextEdit();
            this.txtUsername = new DevExpress.XtraEditors.TextEdit();
            this.txtPassword = new DevExpress.XtraEditors.TextEdit();
            this.chkDisabled = new DevExpress.XtraEditors.CheckEdit();
            this.lblFullName = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.tapNhomCuaUser = new DevExpress.XtraTab.XtraTabPage();
            this.btnRemoveGroup = new DevExpress.XtraEditors.SimpleButton();
            this.btnThemNhom = new DevExpress.XtraEditors.SimpleButton();
            this.gcNhom = new DevExpress.XtraGrid.GridControl();
            this.gvNhom = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnLuu = new DevExpress.XtraEditors.SimpleButton();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.pageAccount)).BeginInit();
            this.pageAccount.SuspendLayout();
            this.tapTaiKhoan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtFullName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisabled.Properties)).BeginInit();
            this.tapNhomCuaUser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcNhom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvNhom)).BeginInit();
            this.SuspendLayout();
            // 
            // pageAccount
            // 
            this.pageAccount.Dock = System.Windows.Forms.DockStyle.Top;
            this.pageAccount.Location = new System.Drawing.Point(0, 0);
            this.pageAccount.Name = "pageAccount";
            this.pageAccount.SelectedTabPage = this.tapTaiKhoan;
            this.pageAccount.Size = new System.Drawing.Size(990, 418);
            this.pageAccount.TabIndex = 0;
            this.pageAccount.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tapTaiKhoan,
            this.tapNhomCuaUser});
            // 
            // tapTaiKhoan
            // 
            this.tapTaiKhoan.Controls.Add(this.lblChonTaiKhoan);
            this.tapTaiKhoan.Controls.Add(this.cboTaiKhoan);
            this.tapTaiKhoan.Controls.Add(this.btnNewAccount);
            this.tapTaiKhoan.Controls.Add(this.txtFullName);
            this.tapTaiKhoan.Controls.Add(this.txtUsername);
            this.tapTaiKhoan.Controls.Add(this.txtPassword);
            this.tapTaiKhoan.Controls.Add(this.chkDisabled);
            this.tapTaiKhoan.Controls.Add(this.lblFullName);
            this.tapTaiKhoan.Controls.Add(this.lblUsername);
            this.tapTaiKhoan.Controls.Add(this.lblPassword);
            this.tapTaiKhoan.Name = "tapTaiKhoan";
            this.tapTaiKhoan.Size = new System.Drawing.Size(988, 388);
            this.tapTaiKhoan.Text = "Thông Tin Tài Khoản";
            // 
            // txtFullName
            // 
            this.txtFullName.Location = new System.Drawing.Point(280, 169);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtFullName.Properties.Appearance.Options.UseFont = true;
            this.txtFullName.Size = new System.Drawing.Size(350, 30);
            this.txtFullName.TabIndex = 0;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(280, 109);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsername.Properties.Appearance.Options.UseFont = true;
            this.txtUsername.Size = new System.Drawing.Size(350, 30);
            this.txtUsername.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(280, 229);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPassword.Properties.Appearance.Options.UseFont = true;
            this.txtPassword.Properties.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(350, 30);
            this.txtPassword.TabIndex = 2;
            // 
            // chkDisabled
            // 
            this.chkDisabled.Location = new System.Drawing.Point(280, 280);
            this.chkDisabled.Name = "chkDisabled";
            this.chkDisabled.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.chkDisabled.Properties.Appearance.Options.UseFont = true;
            this.chkDisabled.Properties.Caption = "Khóa tài khoản";
            this.chkDisabled.Size = new System.Drawing.Size(150, 27);
            this.chkDisabled.TabIndex = 3;
            // 
            // lblChonTaiKhoan
            // 
            this.lblChonTaiKhoan.AutoSize = false;
            this.lblChonTaiKhoan.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblChonTaiKhoan.Location = new System.Drawing.Point(145, 52);
            this.lblChonTaiKhoan.Name = "lblChonTaiKhoan";
            this.lblChonTaiKhoan.Size = new System.Drawing.Size(130, 25);
            this.lblChonTaiKhoan.TabIndex = 7;
            this.lblChonTaiKhoan.Text = "Chọn Tài Khoản:";
            this.lblChonTaiKhoan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboTaiKhoan
            // 
            this.cboTaiKhoan.Location = new System.Drawing.Point(280, 50);
            this.cboTaiKhoan.Name = "cboTaiKhoan";
            this.cboTaiKhoan.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboTaiKhoan.Properties.Appearance.Options.UseFont = true;
            this.cboTaiKhoan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboTaiKhoan.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("USERNAME", "Tên đăng nhập"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("FULLNAME", "Họ và tên"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("TRANGTHAI", "Trạng thái")});
            this.cboTaiKhoan.Properties.DisplayMember = "USERNAME";
            this.cboTaiKhoan.Properties.NullText = "-- Chọn tài khoản để xem/sửa --";
            this.cboTaiKhoan.Properties.ValueMember = "IDUSER";
            this.cboTaiKhoan.Size = new System.Drawing.Size(350, 30);
            this.cboTaiKhoan.TabIndex = 8;
            this.cboTaiKhoan.EditValueChanged += new System.EventHandler(this.cboTaiKhoan_EditValueChanged);
            // 
            // btnNewAccount
            // 
            this.btnNewAccount.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNewAccount.Appearance.Options.UseFont = true;
            this.btnNewAccount.Location = new System.Drawing.Point(645, 46);
            this.btnNewAccount.Name = "btnNewAccount";
            this.btnNewAccount.Size = new System.Drawing.Size(155, 38);
            this.btnNewAccount.TabIndex = 9;
            this.btnNewAccount.Text = "Tạo tài khoản mới";
            this.btnNewAccount.Click += new System.EventHandler(this.btnNewAccount_Click);
            // 
            // lblFullName
            // 
            this.lblFullName.AutoSize = false;
            this.lblFullName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFullName.Location = new System.Drawing.Point(145, 172);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new System.Drawing.Size(130, 25);
            this.lblFullName.TabIndex = 4;
            this.lblFullName.Text = "Họ và tên:";
            this.lblFullName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = false;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUsername.Location = new System.Drawing.Point(145, 112);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(130, 25);
            this.lblUsername.TabIndex = 5;
            this.lblUsername.Text = "Tên đăng nhập:";
            this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = false;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPassword.Location = new System.Drawing.Point(145, 232);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(130, 25);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "Mật khẩu:";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tapNhomCuaUser
            // 
            this.tapNhomCuaUser.Controls.Add(this.btnRemoveGroup);
            this.tapNhomCuaUser.Controls.Add(this.btnThemNhom);
            this.tapNhomCuaUser.Controls.Add(this.gcNhom);
            this.tapNhomCuaUser.Name = "tapNhomCuaUser";
            this.tapNhomCuaUser.Size = new System.Drawing.Size(988, 415);
            this.tapNhomCuaUser.Text = "Nhóm Người Dùng";
            // 
            // btnRemoveGroup
            // 
            this.btnRemoveGroup.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRemoveGroup.Appearance.Options.UseFont = true;
            this.btnRemoveGroup.Location = new System.Drawing.Point(873, 360);
            this.btnRemoveGroup.Name = "btnRemoveGroup";
            this.btnRemoveGroup.Size = new System.Drawing.Size(100, 32);
            this.btnRemoveGroup.TabIndex = 3;
            this.btnRemoveGroup.Text = "Xoá";
            // 
            // btnThemNhom
            // 
            this.btnThemNhom.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnThemNhom.Appearance.Options.UseFont = true;
            this.btnThemNhom.Location = new System.Drawing.Point(758, 360);
            this.btnThemNhom.Name = "btnThemNhom";
            this.btnThemNhom.Size = new System.Drawing.Size(100, 32);
            this.btnThemNhom.TabIndex = 2;
            this.btnThemNhom.Text = "Thêm";
            // 
            // gcNhom
            // 
            this.gcNhom.Dock = System.Windows.Forms.DockStyle.Top;
            this.gcNhom.Location = new System.Drawing.Point(0, 0);
            this.gcNhom.MainView = this.gvNhom;
            this.gcNhom.Name = "gcNhom";
            this.gcNhom.Size = new System.Drawing.Size(988, 343);
            this.gcNhom.TabIndex = 0;
            this.gcNhom.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvNhom});
            // 
            // gvNhom
            // 
            this.gvNhom.GridControl = this.gcNhom;
            this.gvNhom.Name = "gvNhom";
            this.gvNhom.OptionsView.ShowGroupPanel = false;
            // 
            // btnXoaTK
            // 
            this.btnXoaTK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnXoaTK.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnXoaTK.Appearance.Options.UseFont = true;
            this.btnXoaTK.Location = new System.Drawing.Point(20, 451);
            this.btnXoaTK.Name = "btnXoaTK";
            this.btnXoaTK.Size = new System.Drawing.Size(160, 40);
            this.btnXoaTK.TabIndex = 13;
            this.btnXoaTK.Text = "Xóa tài khoản";
            this.btnXoaTK.Click += new System.EventHandler(this.btnXoaTK_Click);
            // 
            // btnLuu
            // 
            this.btnLuu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLuu.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLuu.Appearance.Options.UseFont = true;
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
            this.btnDong.Location = new System.Drawing.Point(818, 451);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(160, 40);
            this.btnDong.TabIndex = 2;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // FrmCreateAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 503);
            this.Controls.Add(this.btnXoaTK);
            this.Controls.Add(this.btnDong);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.pageAccount);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCreateAccount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tạo Tài Khoản Người Dùng";
            this.Load += new System.EventHandler(this.FrmCreateAccount_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pageAccount)).EndInit();
            this.pageAccount.ResumeLayout(false);
            this.tapTaiKhoan.ResumeLayout(false);
            this.tapTaiKhoan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtFullName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisabled.Properties)).EndInit();
            this.tapNhomCuaUser.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcNhom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvNhom)).EndInit();
            this.ResumeLayout(false);

        }



        private void FrmCreateAccount_Load(object sender, EventArgs e)
        {
            loadAccounts();
            btnNewAccount_Click(null, null);
            chkDisabled.Enabled = UserSession.HasRight("F_SYSTEM_LOCK_USER");
        }

        private void loadAccounts()
        {
            var accounts = db.TB_SYS_USER.Where(x => (x.ISGROUP ?? 0) == 0)
                .Select(x => new {
                    x.IDUSER,
                    x.USERNAME,
                    x.FULLNAME,
                    TRANGTHAI = x.DISABLED == 1 ? "Đã khóa" : "Hoạt động"
                }).ToList();
            cboTaiKhoan.Properties.DataSource = accounts;
        }

        private void cboTaiKhoan_EditValueChanged(object sender, EventArgs e)
        {
            if (cboTaiKhoan.EditValue != null && cboTaiKhoan.EditValue != DBNull.Value)
            {
                decimal userId = Convert.ToDecimal(cboTaiKhoan.EditValue);
                _user = db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == userId);
                if (_user != null)
                {
                    _them = false;
                    txtUsername.Text = _user.USERNAME;
                    txtUsername.Enabled = false;
                    txtFullName.Text = _user.FULLNAME;
                    txtPassword.Text = string.Empty; // Don't show password hashes
                    txtPassword.Properties.NullValuePrompt = "Để trống nếu không muốn đổi mật khẩu";
                    chkDisabled.Checked = _user.DISABLED == 1;
                    chkDisabled.Enabled = !_user.USERNAME.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) && 
                                          !_user.USERNAME.Equals(UserSession.CurrentUser?.USERNAME, StringComparison.OrdinalIgnoreCase) && 
                                          UserSession.HasRight("F_SYSTEM_LOCK_USER");
                    
                    tapNhomCuaUser.PageEnabled = true;
                    btnXoaTK.Enabled = true;
                    loadUserGroups();
                }
            }
        }

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            _them = true;
            _user = null;
            cboTaiKhoan.EditValue = null;
            txtUsername.Text = string.Empty;
            txtUsername.Enabled = true;
            txtFullName.Text = string.Empty;
            txtPassword.Text = string.Empty;
            txtPassword.Properties.NullValuePrompt = "Nhập mật khẩu cho tài khoản mới";
            chkDisabled.Checked = false;
            chkDisabled.Enabled = UserSession.HasRight("F_SYSTEM_LOCK_USER");
            
            tapNhomCuaUser.PageEnabled = false;
            btnXoaTK.Enabled = false;
            txtUsername.Focus();
        }

        private void loadUserGroups()
        {
            if (_user != null)
            {
                var userGroupIds = db.TB_SYS_GROUP.Where(g => g.MEMBER == _user.IDUSER).Select(g => g.ID_GROUP).ToList();
                var userGroups = db.TB_SYS_USER.Where(u => userGroupIds.Contains(u.IDUSER) && u.ISGROUP == 1).ToList();
                gcNhom.DataSource = userGroups;
                FormManager_Functions.CustomView_Colums(gvNhom);

                if (gvNhom.Columns["PASSWORD"] != null) gvNhom.Columns["PASSWORD"].Visible = false;
                if (gvNhom.Columns["LAST_PWD_CHANGED"] != null) gvNhom.Columns["LAST_PWD_CHANGED"].Visible = false;
                if (gvNhom.Columns["ISGROUP"] != null) gvNhom.Columns["ISGROUP"].Visible = false;
                
                if (gvNhom.Columns["IDUSER"] != null) gvNhom.Columns["IDUSER"].Caption = "Mã số";
                if (gvNhom.Columns["USERNAME"] != null) gvNhom.Columns["USERNAME"].Caption = "Tên nhóm";
                if (gvNhom.Columns["FULLNAME"] != null) gvNhom.Columns["FULLNAME"].Caption = "Mô tả nhóm";
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string fullname = txtFullName.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(fullname))
            {
                MessageBox.Show("Vui lòng nhập họ và tên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (_them)
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu cho tài khoản mới.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (_sysUser.checkUserExist(username))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại trong hệ thống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.SelectAll();
                    txtUsername.Focus();
                    return;
                }

                if (chkDisabled.Checked && !UserSession.HasRight("F_SYSTEM_LOCK_USER"))
                {
                    MessageBox.Show("Bạn không có quyền tạo tài khoản ở trạng thái khóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _user = new TB_SYS_USER
                {
                    USERNAME = username,
                    FULLNAME = fullname,
                    PASSWORD = PasswordHasher.HashPassword(password),
                    DISABLED = chkDisabled.Checked ? 1 : 0,
                    ISGROUP = 0,
                    MACTY = "1",
                    MADVI = "1"
                };

                db.TB_SYS_USER.Add(_user);
                db.SaveChanges();
                _them = false;
                _dataSaved = true;
                
                loadAccounts();
                cboTaiKhoan.EditValue = _user.IDUSER;
                
                tapNhomCuaUser.PageEnabled = true;
                pageAccount.SelectedTabPage = tapNhomCuaUser;
                loadUserGroups();
                
                MessageBox.Show("Tạo tài khoản người dùng thành công. Bây giờ bạn có thể gán tài khoản này vào các nhóm ở tab Nhóm Người Dùng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (_user.DISABLED != (chkDisabled.Checked ? 1 : 0))
                {
                    if (!UserSession.HasRight("F_SYSTEM_LOCK_USER"))
                    {
                        MessageBox.Show("Bạn không có quyền thay đổi trạng thái khóa tài khoản.", "Lỗi phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (_user.USERNAME.Equals(UserSession.CurrentUser?.USERNAME, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Bạn không thể tự khóa tài khoản của chính mình.", "Lỗi bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (_user.USERNAME.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) && chkDisabled.Checked)
                {
                    MessageBox.Show("Không thể khóa tài khoản Quản trị hệ thống (ADMIN).", "Lỗi bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Log audit action if status changed
                if (_user.DISABLED != (chkDisabled.Checked ? 1 : 0))
                {
                    string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
                    try
                    {
                        string action = chkDisabled.Checked ? "LOCKED" : "UNLOCKED";
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - AUDIT: USER [{UserSession.CurrentUser?.USERNAME}] {action} account [{_user.USERNAME}]\r\n");
                    }
                    catch { }
                }

                _user.FULLNAME = fullname;
                _user.DISABLED = chkDisabled.Checked ? 1 : 0;
                
                if (!string.IsNullOrEmpty(password))
                {
                    _user.PASSWORD = PasswordHasher.HashPassword(password);
                }

                db.SaveChanges();
                _dataSaved = true;
                
                loadAccounts();
                cboTaiKhoan.EditValue = _user.IDUSER;
                
                MessageBox.Show("Cập nhật thông tin tài khoản thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnXoaTK_Click(object sender, EventArgs e)
        {
            if (_user == null || _them) return;

            if (_user.USERNAME.Equals(UserSession.CurrentUser?.USERNAME, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không thể tự xóa tài khoản của chính mình.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_user.USERNAME.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể xóa tài khoản Quản trị hệ thống (ADMIN).", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tài khoản [{_user.USERNAME}] không?\nHành động này cũng sẽ xóa toàn bộ phân quyền và liên kết nhóm liên quan.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    // Remove group connections
                    var groups = db.TB_SYS_GROUP.Where(g => g.MEMBER == _user.IDUSER).ToList();
                    db.TB_SYS_GROUP.RemoveRange(groups);

                    // Remove rights
                    var rights = db.TB_SYS_RIGHT.Where(r => r.IDUSER == _user.IDUSER).ToList();
                    db.TB_SYS_RIGHT.RemoveRange(rights);

                    // Remove user
                    var userInDb = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == _user.IDUSER);
                    if (userInDb != null)
                    {
                        db.TB_SYS_USER.Remove(userInDb);
                    }

                    db.SaveChanges();
                    MessageBox.Show("Xóa tài khoản thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _dataSaved = true;

                    // Log audit action for delete
                    string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
                    try
                    {
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - AUDIT: USER [{UserSession.CurrentUser?.USERNAME}] DELETED account [{_user.USERNAME}]\r\n");
                    }
                    catch { }

                    btnNewAccount_Click(null, null);
                    loadAccounts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnThemNhom_Click(object sender, EventArgs e)
        {
            if (_user == null) return;

            // Load groups this user is NOT in
            var userGroupIds = db.TB_SYS_GROUP.Where(g => g.MEMBER == _user.IDUSER).Select(g => g.ID_GROUP).ToList();
            var nonGroups = db.TB_SYS_USER.Where(u => !userGroupIds.Contains(u.IDUSER) && u.ISGROUP == 1).ToList();

            if (nonGroups.Count == 0)
            {
                MessageBox.Show("Tài khoản này đã tham gia tất cả các nhóm hiện có.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var selector = new FrmSelectGroups(nonGroups))
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    foreach (var grp in selector.SelectedGroups)
                    {
                        var groupMember = new TB_SYS_GROUP { ID_GROUP = grp.IDUSER, MEMBER = _user.IDUSER };
                        db.TB_SYS_GROUP.Add(groupMember);
                    }
                    db.SaveChanges();
                    _dataSaved = true;
                    loadUserGroups();
                }
            }
        }

        private void btnRemoveGroup_Click(object sender, EventArgs e)
        {
            if (_user == null) return;

            var selectedGroup = (TB_SYS_USER)gvNhom.GetFocusedRow();
            if (selectedGroup == null)
            {
                MessageBox.Show("Vui lòng chọn nhóm muốn loại bỏ tài khoản khỏi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn rời khỏi nhóm [{selectedGroup.USERNAME}] không?",
                "Xác nhận loại bỏ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var record = db.TB_SYS_GROUP.FirstOrDefault(g => g.ID_GROUP == selectedGroup.IDUSER && g.MEMBER == _user.IDUSER);
                    if (record != null)
                    {
                        db.TB_SYS_GROUP.Remove(record);
                        db.SaveChanges();
                        _dataSaved = true;
                        loadUserGroups();
                        MessageBox.Show("Đã loại bỏ tài khoản khỏi nhóm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.DialogResult = _dataSaved ? DialogResult.OK : DialogResult.Cancel;
            this.Close();
        }
    }

    // Multi-selection dialog for groups
    public class FrmSelectGroups : DevExpress.XtraEditors.XtraForm
    {
        private GridControl gc;
        private GridView gv;
        private SimpleButton btnSelect;
        private SimpleButton btnCancel;
        public List<TB_SYS_USER> SelectedGroups { get; private set; } = new List<TB_SYS_USER>();

        public FrmSelectGroups(List<TB_SYS_USER> groups)
        {
            this.Text = "Chọn nhóm muốn tham gia";
            this.Size = new Size(520, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Appearance.Font = new Font("Segoe UI", 9.5F);

            gc = new GridControl { Dock = DockStyle.Top, Height = 300 };
            gv = new GridView();
            gc.MainView = gv;
            gc.ViewCollection.Add(gv);

            btnSelect = new SimpleButton { Text = "Chọn", Location = new Point(130, 320), Size = new Size(110, 40) };
            btnCancel = new SimpleButton { Text = "Hủy", Location = new Point(270, 320), Size = new Size(110, 40) };

            btnSelect.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            this.Controls.Add(gc);
            this.Controls.Add(btnSelect);
            this.Controls.Add(btnCancel);

            gv.OptionsBehavior.Editable = false;
            gv.OptionsSelection.MultiSelect = true;
            gv.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

            gc.DataSource = groups;

            gv.Columns["IDUSER"].Caption = "Mã nhóm";
            gv.Columns["USERNAME"].Caption = "Tên nhóm";
            gv.Columns["FULLNAME"].Caption = "Mô tả";

            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
            {
                if (col.FieldName != "IDUSER" && col.FieldName != "USERNAME" && col.FieldName != "FULLNAME")
                    col.Visible = false;
            }

            FormManager_Functions.CustomView_Colums(gv);

            btnCancel.Click += (s, e) => this.Close();
            btnSelect.Click += (s, e) => {
                var selectedRowHandles = gv.GetSelectedRows();
                foreach (var rowHandle in selectedRowHandles)
                {
                    if (rowHandle >= 0)
                    {
                        var group = (TB_SYS_USER)gv.GetRow(rowHandle);
                        SelectedGroups.Add(group);
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }
    }
}
