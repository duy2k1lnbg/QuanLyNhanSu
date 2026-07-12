using DevExpress.XtraEditors;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.CLASS_CHAMCONG;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmOllamaConfig : DevExpress.XtraEditors.XtraForm
    {
        private SYS_CONFIG _sysConfig;

        public FrmOllamaConfig()
        {
            InitializeComponent();
            _sysConfig = new SYS_CONFIG();
        }

        private void FrmOllamaConfig_Load(object sender, EventArgs e)
        {
            Functions.TranslationManager.Translate(this);
            txtUrl.Text = _sysConfig.getValue("OllamaHost", "http://localhost:11434");
            txtModel.Text = _sysConfig.getValue("AiModel", "qwen2.5:latest");
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            url = url.TrimEnd('/');
            string testUrl = $"{url}/api/tags";

            btnTest.Enabled = false;
            btnTest.Text = Functions.TranslationManager.Translate("Đang thử...");
            try
            {
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                {
                    var response = await client.GetAsync(testUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        XtraMessageBox.Show(Functions.TranslationManager.Translate("Kết nối đến Ollama Server thành công!"), Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show(Functions.TranslationManager.Translate("Kết nối thất bại. Mã lỗi:") + " " + response.StatusCode, Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(Functions.TranslationManager.Translate("Không thể kết nối. Vui lòng kiểm tra lại địa chỉ IP, port, mạng.") + "\n" + Functions.TranslationManager.Translate("Chi tiết lỗi:") + " " + ex.Message, Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTest.Enabled = true;
                btnTest.Text = Functions.TranslationManager.Translate("Kiểm Tra Kết Nối");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string url = txtUrl.Text.Trim().TrimEnd('/');
                string model = txtModel.Text.Trim();

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(model))
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Vui lòng điền đầy đủ thông tin."), Functions.TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _sysConfig.setItem("OllamaHost", url);
                _sysConfig.setItem("AiModel", model);

                XtraMessageBox.Show(Functions.TranslationManager.Translate("Đã lưu thiết lập cấu hình AI."), Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(Functions.TranslationManager.Translate("Lỗi khi lưu cấu hình:") + " " + ex.Message, Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
