using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu
{
    internal static class Program
    {
        public static System.Drawing.Icon AppIcon { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load application icon from Resources
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "hrm_icon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                try
                {
                    AppIcon = new System.Drawing.Icon(iconPath);
                }
                catch { }
            }

            // Load saved display language at startup
            QLyNSu.Functions.TranslationManager.LoadLanguage();
            QLyNSu.Functions.TranslationManager.InitializeHook();

            // Register DevExpress skins and enable form skinning
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();

            // Set global DevExpress settings for a modern premium aesthetic
            DevExpress.XtraEditors.WindowsFormsSettings.ForceDirectXPaint();
            DevExpress.XtraEditors.WindowsFormsSettings.DefaultFont = new System.Drawing.Font("Segoe UI", 9.5F);
            DevExpress.XtraEditors.WindowsFormsSettings.DefaultMenuFont = new System.Drawing.Font("Segoe UI", 9.5F);
            DevExpress.XtraEditors.WindowsFormsSettings.ScrollUIMode = DevExpress.XtraEditors.ScrollUIMode.Touch;

            // Apply "The Bezier" skin with "Gleam" palette as default
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("The Bezier", "Gleam");

            // 1. Show Splash Form
            using (var splash = new FrmStartup())
            {
                splash.ShowDialog();
            }

            // 2. Show Login Form
            using (var login = new FORM_SYSTEM.FrmDangNhap())
            {
                if (login.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // 3. Login successful, run MainForm
                    Application.Run(new MainForm());
                }
            }

            QLyNSu.Functions.TranslationManager.ShutdownHook();
        }
    }
}
