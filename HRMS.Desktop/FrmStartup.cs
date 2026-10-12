using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace QLyNSu
{
    public partial class FrmStartup : Form
    {
        private Timer closeTimer;
        private Image _loadedBackground;

        public FrmStartup()
        {
            InitializeComponent();
            this.labelCopyright.Text = "Copyright © 1998-" + DateTime.Now.Year.ToString() + " HRM OC System. All rights reserved.";
            
            // Set Form Icon if loaded
            if (Program.AppIcon != null)
            {
                this.Icon = Program.AppIcon;
            }

            // Apply a premium color palette and custom fonts programmatically
            this.labelStatus.Text = "Đang khởi tạo hệ thống quản trị nhân sự...";
            this.labelStatus.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            this.labelStatus.Appearance.ForeColor = Color.FromArgb(108, 92, 231); // Soft Purple
            this.labelStatus.Appearance.Options.UseFont = true;
            this.labelStatus.Appearance.Options.UseForeColor = true;

            this.labelCopyright.Appearance.Font = new Font("Segoe UI", 8F);
            this.labelCopyright.Appearance.ForeColor = Color.DarkGray;
            this.labelCopyright.Appearance.Options.UseFont = true;
            this.labelCopyright.Appearance.Options.UseForeColor = true;

            Shown += FrmStartup_Shown;
            FormClosed += (s, e) =>
            {
                closeTimer.Stop();
                closeTimer.Dispose();
                peImage.EditValue = null;
                _loadedBackground?.Dispose();
                _loadedBackground = null;
            };

            // Set up timer to close the form after 2.5 seconds (2500ms)
            closeTimer = new Timer();
            closeTimer.Interval = 2500;
            closeTimer.Tick += CloseTimer_Tick;
            closeTimer.Start();
        }

        private async void FrmStartup_Shown(object sender, EventArgs e)
        {
            string resources = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var background = await Task.Run(() =>
            {
                foreach (string name in new[] { "hrm_splash.png", "splash_bg.png" })
                {
                    try
                    {
                        string path = System.IO.Path.Combine(resources, name);
                        if (System.IO.File.Exists(path))
                            using (var source = Image.FromFile(path)) return (Image)new Bitmap(source);
                    }
                    catch { }
                }
                return null;
            });
            if (IsDisposed || Disposing) { background?.Dispose(); return; }
            _loadedBackground = background;
            peImage.EditValue = background;
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            closeTimer.Stop();
            this.Close();
        }
    }
}