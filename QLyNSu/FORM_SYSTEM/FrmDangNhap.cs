using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
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
    public partial class FrmDangNhap : DevExpress.XtraEditors.XtraForm
    {
        public FrmDangNhap()
        {
            InitializeComponent();
            txtPassword.ButtonClick += TxtPassword_ButtonClick;
        }
        private void TxtPassword_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            txtPassword.Properties.UseSystemPasswordChar = !txtPassword.Properties.UseSystemPasswordChar;
            txtPassword.Properties.PasswordChar = txtPassword.Properties.UseSystemPasswordChar ? '\0' : '●';
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtUser.ErrorText = "Vui lòng nhập tài khoản";
                return;
            }

            // Hiển thị Overlay mờ ảo
            var overlay = SplashScreenManager.ShowOverlayForm(this);

            try
            {
                string u = txtUser.Text;
                string p = txtPassword.Text;

                // Kiểm tra database Oracle qua EF6 Task
                bool isValid = await Task.Run(() => {
                    using (var db = new MyEntities())
                    {
                        // EF6 + Oracle thường cần ép kiểu hoặc ToUpper nếu DB không phân biệt
                        return db.TB_SYS_USER.Any(x => x.USERNAME.ToUpper() == u.ToUpper() && x.PASSWORD == p);
                    }
                });

                if (isValid)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show("Thông tin đăng nhập không chính xác.", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi Oracle đặc thù (ORA-xxxxx)
                XtraMessageBox.Show($"Lỗi Oracle: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SplashScreenManager.CloseOverlayForm(overlay);
            }
        }
    }
}