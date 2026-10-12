using Bu.CLASS_SYSTEM;
using Bu.CLASS_SECURITY;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLyNSu.Functions;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmChangePassword : DevExpress.XtraEditors.XtraForm
    {
        private readonly IAuthSecurityService _authService;
        private bool _saving;

        public FrmChangePassword() : this(new AuthSecurityService()) { }

        public FrmChangePassword(IAuthSecurityService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            InitializeComponent();
            FormClosing += (sender, args) => { if (_saving) args.Cancel = true; };
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }

        // Read and verify the current password in Oracle, never in the password-free session DTO.
        internal Task<ChangePasswordResultDto> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            if (!UserSession.IsLoggedIn)
                return Task.FromResult(new ChangePasswordResultDto { Message = "Chưa đăng nhập hệ thống." });
            decimal userId = UserSession.CurrentUser.IDUSER;
            string correlationId = Guid.NewGuid().ToString("N");
            return Task.Run(() => _authService.ChangePasswordWithRevocationAsync(
                userId, oldPassword, newPassword, null, "DESKTOP_WINFORMS", correlationId));
        }

        private void ShowMessage(string message, MessageBoxIcon icon)
        {
            MessageBox.Show(this, TranslationManager.Translate(message), TranslationManager.Translate("Thông báo"),
                MessageBoxButtons.OK, icon);
        }

        private async void btnLuu_Click(object sender, EventArgs e)
        {
            if (_saving) return;
            string oldPwd = txtOldPassword.Text;
            string newPwd = txtNewPassword.Text;
            string confirmPwd = txtConfirmPassword.Text;
            if (string.IsNullOrWhiteSpace(oldPwd) || string.IsNullOrWhiteSpace(newPwd) || string.IsNullOrWhiteSpace(confirmPwd))
            {
                ShowMessage("Vui lòng điền đầy đủ tất cả các trường.", MessageBoxIcon.Warning);
                return;
            }
            if (newPwd.Trim().Length < 3)
            {
                ShowMessage("Mật khẩu mới phải từ 3 ký tự trở lên.", MessageBoxIcon.Warning);
                return;
            }
            if (newPwd != confirmPwd)
            {
                ShowMessage("Mật khẩu mới và Nhập lại mật khẩu không trùng khớp.", MessageBoxIcon.Warning);
                return;
            }
            _saving = true;
            btnLuu.Enabled = btnDong.Enabled = false;
            txtOldPassword.Enabled = txtNewPassword.Enabled = txtConfirmPassword.Enabled = false;
            UseWaitCursor = true;
            try
            {
                var result = await ChangePasswordAsync(oldPwd, newPwd);
                if (IsDisposed) return;
                if (!result.Success)
                {
                    ShowMessage(result.Message, MessageBoxIcon.Warning);
                    txtOldPassword.SelectAll();
                    return;
                }
                // The database has committed the password and revoked the old sessions.
                UserSession.Clear();
                ShowMessage("Đổi mật khẩu thành công. Vui lòng đăng nhập lại bằng mật khẩu mới.", MessageBoxIcon.Information);
                txtOldPassword.Text = txtNewPassword.Text = txtConfirmPassword.Text = string.Empty;
                _saving = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DesktopChangePassword] " + ex);
                if (!IsDisposed) ShowMessage("Không thể đổi mật khẩu. Vui lòng kiểm tra kết nối Oracle và thử lại.", MessageBoxIcon.Error);
            }
            finally
            {
                _saving = false;
                if (!IsDisposed)
                {
                    btnLuu.Enabled = btnDong.Enabled = true;
                    txtOldPassword.Enabled = txtNewPassword.Enabled = txtConfirmPassword.Enabled = true;
                    UseWaitCursor = false;
                    txtOldPassword.Focus();
                }
            }
        }
    }
}
