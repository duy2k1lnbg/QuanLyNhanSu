using DevExpress.XtraEditors;
using System;
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
                MessageBox.Show("Vui lòng chọn ngôn ngữ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedLang = cboLanguage.SelectedItem.ToString();
            SaveLanguageSetting(selectedLang);

            MessageBox.Show($"Đã lưu cài đặt ngôn ngữ hệ thống: {selectedLang}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string LoadLanguageSetting()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var settings = JsonConvert.DeserializeObject<SystemSettings>(json);
                    if (settings != null && !string.IsNullOrEmpty(settings.Language))
                    {
                        return settings.Language;
                    }
                }
            }
            catch
            {
                // Fallback to default
            }
            return "Tiếng Việt";
        }

        private void SaveLanguageSetting(string language)
        {
            try
            {
                var settings = new SystemSettings { Language = language };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu cài đặt: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class SystemSettings
        {
            public string Language { get; set; }
        }
    }
}
