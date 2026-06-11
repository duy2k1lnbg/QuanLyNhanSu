using DevExpress.XtraWaitForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QLyNSu
{
    public partial class FrmWaiting : WaitForm
    {
        public FrmWaiting()
        {
            InitializeComponent();
            this.progressPanel1.AutoHeight = true;

            // Apply a modern, professional style programmatically
            this.progressPanel1.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.progressPanel1.AppearanceCaption.ForeColor = System.Drawing.Color.FromArgb(45, 52, 54);
            this.progressPanel1.AppearanceCaption.Options.UseFont = true;
            this.progressPanel1.AppearanceCaption.Options.UseForeColor = true;

            this.progressPanel1.AppearanceDescription.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.progressPanel1.AppearanceDescription.ForeColor = System.Drawing.Color.FromArgb(108, 92, 231); // Premium soft purple
            this.progressPanel1.AppearanceDescription.Options.UseFont = true;
            this.progressPanel1.AppearanceDescription.Options.UseForeColor = true;

            // Load translations dynamically
            this.progressPanel1.Caption = Functions.TranslationManager.Translate("Vui lòng đợi");
            this.progressPanel1.Description = Functions.TranslationManager.Translate("Đang xử lý dữ liệu...");
        }

        #region Overrides

        public override void SetCaption(string caption)
        {
            base.SetCaption(caption);
            this.progressPanel1.Caption = caption;
        }
        public override void SetDescription(string description)
        {
            base.SetDescription(description);
            this.progressPanel1.Description = description;
        }
        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum WaitFormCommand
        {
        }
    }
}