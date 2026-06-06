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
    public partial class FrmChangePassword : DevExpress.XtraEditors.XtraForm
    {
        private SYS_USER _sysUser;

        public FrmChangePassword()
        {
            InitializeComponent();
            _sysUser = new SYS_USER();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string oldPwd = txtOldPassword.Text;
            string newPwd = txtNewPassword.Text;
            string confirmPwd = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirmPwd))
            {
                MessageBox.Show("Vui lòng điền đầy đủ tất cả các trường.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPwd.Length < 3)
            {
                MessageBox.Show("Mật khẩu mới phải từ 3 ký tự trở lên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPwd != confirmPwd)
            {
                MessageBox.Show("Mật khẩu mới và Nhập lại mật khẩu không trùng khớp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verify current password
            var currentUser = UserSession.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("Chưa đăng nhập hệ thống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!PasswordHasher.VerifyPassword(oldPwd, currentUser.PASSWORD))
            {
                MessageBox.Show("Mật khẩu cũ không chính xác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldPassword.SelectAll();
                txtOldPassword.Focus();
                return;
            }

            try
            {
                // Update password in DB
                currentUser.PASSWORD = PasswordHasher.HashPassword(newPwd);
                _sysUser.Update(currentUser);

                MessageBox.Show("Đổi mật khẩu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đổi mật khẩu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
