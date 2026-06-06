using DevExpress.XtraEditors;
using System;
using QLyNSu.Functions;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmSetting : DevExpress.XtraEditors.XtraForm
    {
        private readonly string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_settings.json");

        public FrmSetting()
        {
            InitializeComponent();
        }

        private void FrmSetting_Load(object sender, EventArgs e)
        {
            // Populate Language ComboBox
            cboLanguage.Properties.Items.Clear();
            cboLanguage.Properties.Items.Add("Tiếng Việt");
            cboLanguage.Properties.Items.Add("Tiếng Anh");
            cboLanguage.Properties.Items.Add("Tiếng Nhật");
            cboLanguage.Properties.Items.Add("Tiếng Trung");
            cboLanguage.Properties.Items.Add("Tiếng Hàn");

            // Load saved language
            string currentLang = LoadLanguageSetting();
            cboLanguage.SelectedItem = currentLang;
            if (cboLanguage.SelectedIndex == -1)
            {
                cboLanguage.SelectedIndex = 0; // Default to Vietnamese
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (cboLanguage.SelectedItem == null)
            {
                MessageBox.Show(
                    TranslationManager.Translate("Vui lòng chọn ngôn ngữ."), 
                    TranslationManager.Translate("Thông báo"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            string selectedLang = cboLanguage.SelectedItem.ToString();
            SaveLanguageSetting(selectedLang);

            string translatedLangName = TranslationManager.Translate(selectedLang);
            string saveMsg = TranslationManager.Translate("Đã lưu cài đặt ngôn ngữ hệ thống");
            MessageBox.Show(
                $"{saveMsg}: {translatedLangName}.", 
                TranslationManager.Translate("Thông báo"), 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string LoadLanguageSetting()
        {
            TranslationManager.LoadLanguage();
            return TranslationManager.CurrentLanguage;
        }

        private void SaveLanguageSetting(string language)
        {
            TranslationManager.CurrentLanguage = language;
        }

        public class SystemSettings
        {
            public string Language { get; set; }
        }
    }
}
