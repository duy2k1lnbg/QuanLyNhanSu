using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmUser : DevExpress.XtraEditors.XtraForm
    {
        private SYS_USER _sysUser;
        private TB_SYS_USER _selectedUser;
        private bool _isEditMode;

        public FrmUser()
        {
            InitializeComponent();
            _sysUser = new SYS_USER();
            _isEditMode = false;
        }

        public FrmUser(TB_SYS_USER user) : this()
        {
            _selectedUser = user;
            _isEditMode = true;
        }

        private void FrmUser_Load(object sender, EventArgs e)
        {
            if (_isEditMode && _selectedUser != null)
            {
                this.Text = "Chỉnh sửa người dùng";
                txtUsername.Text = _selectedUser.USERNAME;
                txtUsername.Enabled = false; // Cannot edit username
                txtFullName.Text = _selectedUser.FULLNAME;
                chkDisabled.Checked = _selectedUser.DISABLED == 1;
                txtPassword.Properties.NullValuePrompt = "Để trống nếu không muốn đổi mật khẩu";
                txtPassword.Properties.NullValuePromptShowForEmptyValue = true;
            }
            else
            {
                this.Text = "Thêm người dùng mới";
                lblPasswordRequired.Visible = true;
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
                MessageBox.Show("Vui lòng nhập họ tên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (!_isEditMode)
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

                var newUser = new TB_SYS_USER
                {
                    USERNAME = username,
                    FULLNAME = fullname,
                    PASSWORD = PasswordHasher.HashPassword(password),
                    DISABLED = chkDisabled.Checked ? 1 : 0,
                    ISGROUP = 0,
                    MACTY = "1",
                    MADVI = "1"
                };

                _sysUser.Add(newUser);
                MessageBox.Show("Thêm người dùng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _selectedUser.FULLNAME = fullname;
                _selectedUser.DISABLED = chkDisabled.Checked ? 1 : 0;
                
                if (!string.IsNullOrEmpty(password))
                {
                    _selectedUser.PASSWORD = PasswordHasher.HashPassword(password);
                }

                _sysUser.Update(_selectedUser);
                MessageBox.Show("Cập nhật thông tin người dùng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
