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
        public FrmSetting()
        {
            InitializeComponent();
        }

        private void FrmSetting_Load(object sender, EventArgs e)
        {
            // Populate Language ComboBox with native names (industry best practice)
            cboLanguage.Properties.Items.Clear();
            cboLanguage.Properties.Items.Add("Tiếng Việt");
            cboLanguage.Properties.Items.Add("English");
            cboLanguage.Properties.Items.Add("日本語");
            cboLanguage.Properties.Items.Add("中文");
            cboLanguage.Properties.Items.Add("한국어");

            // Load saved language
            string currentLang = LoadLanguageSetting();

            // Select the item corresponding to the current language
            if (currentLang == "Tiếng Việt") cboLanguage.SelectedItem = "Tiếng Việt";
            else if (currentLang == "Tiếng Anh") cboLanguage.SelectedItem = "English";
            else if (currentLang == "Tiếng Nhật") cboLanguage.SelectedItem = "日本語";
            else if (currentLang == "Tiếng Trung") cboLanguage.SelectedItem = "中文";
            else if (currentLang == "Tiếng Hàn") cboLanguage.SelectedItem = "한국어";
            else cboLanguage.SelectedIndex = 0;
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

            string selectedDisplay = cboLanguage.SelectedItem.ToString();
            string selectedLang = TranslationManager.GetCanonicalLanguage(selectedDisplay);
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
            this.DialogResult = DialogResult.Cancel;
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
