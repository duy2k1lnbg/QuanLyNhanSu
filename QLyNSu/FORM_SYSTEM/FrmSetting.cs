using DevExpress.XtraEditors;
using System;
using System.Linq;
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
            LoadLanguagesToCombo();

            // Load saved language code (e.g. "en", "vi")
            string currentLangCode = LoadLanguageSetting();

            // Select the item corresponding to the current language
            bool found = false;
            foreach (LangItem item in cboLanguage.Properties.Items)
            {
                if (TranslationManager.GetCanonicalLanguage(item.Name) == currentLangCode)
                {
                    cboLanguage.SelectedItem = item;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // Fallback to select first item if present
                if (cboLanguage.Properties.Items.Count > 0)
                    cboLanguage.SelectedIndex = 0;
            }
        }

        private void LoadLanguagesToCombo()
        {
            cboLanguage.Properties.Items.Clear();
            try
            {
                var sysLang = new Bu.CLASS_SYSTEM.SYS_LANGUAGE();
                var dbLangs = sysLang.getList()
                                     .Where(l => l.IS_ACTIVE)
                                     .GroupBy(l => l.NAME)
                                     .Select(g => g.First())
                                     .ToList();
                if (dbLangs.Count > 0)
                {
                    foreach (var lang in dbLangs)
                    {
                        cboLanguage.Properties.Items.Add(new LangItem { Name = lang.NAME });
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FrmSetting DB Load Error]: " + ex.Message);
            }

            // Mặc định dự phòng nếu lỗi CSDL
            cboLanguage.Properties.Items.Add(new LangItem { Name = "Tiếng Việt" });
            cboLanguage.Properties.Items.Add(new LangItem { Name = "Tiếng Anh" });
            cboLanguage.Properties.Items.Add(new LangItem { Name = "Tiếng Nhật" });
            cboLanguage.Properties.Items.Add(new LangItem { Name = "Tiếng Trung" });
            cboLanguage.Properties.Items.Add(new LangItem { Name = "Tiếng Hàn" });
        }

        private void btnManage_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmLanguages())
            {
                TranslationManager.Translate(frm);
                frm.ShowDialog();
            }
            LoadLanguagesToCombo();
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

            string selectedDisplay = ((LangItem)cboLanguage.SelectedItem).Name;
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
            TranslationManager.TranslateAllOpenForms();
        }

        public class SystemSettings
        {
            public string Language { get; set; }
        }

        private class LangItem
        {
            public string Name { get; set; }
            public override string ToString()
            {
                return TranslationManager.Translate(Name);
            }
        }
    }
}
