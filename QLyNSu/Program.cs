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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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

            Application.Run(new MainForm());
        }
    }
}
