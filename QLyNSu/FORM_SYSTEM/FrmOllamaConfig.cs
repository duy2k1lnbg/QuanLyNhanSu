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
            txtQdrant.Text = _sysConfig.getValue("QdrantUrl", "http://127.0.0.1:6333");
            txtTemp.Text = _sysConfig.getValue("AiTemp", "0.4");
            txtMaxTokens.Text = _sysConfig.getValue("AiMaxTokens", "1000");
            txtCtx.Text = _sysConfig.getValue("AiCtx", "3072");
            txtTopK.Text = _sysConfig.getValue("AiTopK", "30");
            txtTopP.Text = _sysConfig.getValue("AiTopP", "0.8");
            txtRepeat.Text = _sysConfig.getValue("AiRepeat", "1.15");
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

        private async void btnTestQdrant_Click(object sender, EventArgs e)
        {
            string url = txtQdrant.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            url = url.TrimEnd('/');
            string testUrl = $"{url}/collections"; // API endpoint cơ bản của Qdrant

            btnTestQdrant.Enabled = false;
            btnTestQdrant.Text = Functions.TranslationManager.Translate("Đang thử...");
            try
            {
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                {
                    var response = await client.GetAsync(testUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        XtraMessageBox.Show(Functions.TranslationManager.Translate("Kết nối đến Qdrant Server thành công!"), Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show(Functions.TranslationManager.Translate("Kết nối Qdrant thất bại. Mã lỗi:") + " " + response.StatusCode, Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(Functions.TranslationManager.Translate("Không thể kết nối đến Qdrant. Vui lòng kiểm tra lại địa chỉ IP, port, mạng.") + "\n" + Functions.TranslationManager.Translate("Chi tiết lỗi:") + " " + ex.Message, Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestQdrant.Enabled = true;
                btnTestQdrant.Text = Functions.TranslationManager.Translate("Kiểm Tra Kết Nối");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string url = txtUrl.Text.Trim().TrimEnd('/');
                string model = txtModel.Text.Trim();
                string qdrantUrl = txtQdrant.Text.Trim().TrimEnd('/');

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(model) || string.IsNullOrEmpty(qdrantUrl))
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Vui lòng điền đầy đủ thông tin."), Functions.TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!double.TryParse(txtTemp.Text.Trim(), out double temp) || temp < 0.0 || temp > 2.0)
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Chat Temperature phải là số từ 0.0 đến 2.0."), Functions.TranslationManager.Translate("Lỗi nhập liệu"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTemp.Focus();
                    return;
                }
                if (!int.TryParse(txtMaxTokens.Text.Trim(), out int maxTokens) || maxTokens < 1 || maxTokens > 16384)
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Max Tokens phải là số nguyên từ 1 đến 16384."), Functions.TranslationManager.Translate("Lỗi nhập liệu"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMaxTokens.Focus();
                    return;
                }
                if (!int.TryParse(txtCtx.Text.Trim(), out int ctx) || ctx < 1024 || ctx > 128000)
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Context Window phải là số nguyên từ 1024 đến 128000."), Functions.TranslationManager.Translate("Lỗi nhập liệu"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCtx.Focus();
                    return;
                }
                if (!int.TryParse(txtTopK.Text.Trim(), out int topk) || topk < 1 || topk > 100)
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Top K phải là số nguyên từ 1 đến 100."), Functions.TranslationManager.Translate("Lỗi nhập liệu"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTopK.Focus();
                    return;
                }
                if (!double.TryParse(txtTopP.Text.Trim(), out double topp) || topp < 0.0 || topp > 1.0)
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Top P phải là số từ 0.0 đến 1.0."), Functions.TranslationManager.Translate("Lỗi nhập liệu"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTopP.Focus();
                    return;
                }
                if (!double.TryParse(txtRepeat.Text.Trim(), out double repeat) || repeat <= 0.0 || repeat > 2.0)
                {
                    XtraMessageBox.Show(Functions.TranslationManager.Translate("Repeat Penalty phải là số lớn hơn 0.0 và nhỏ hơn hoặc bằng 2.0."), Functions.TranslationManager.Translate("Lỗi nhập liệu"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtRepeat.Focus();
                    return;
                }

                _sysConfig.setItem("OllamaHost", url);
                _sysConfig.setItem("AiModel", model);
                _sysConfig.setItem("QdrantUrl", qdrantUrl);
                _sysConfig.setItem("AiTemp", txtTemp.Text.Trim());
                _sysConfig.setItem("AiMaxTokens", txtMaxTokens.Text.Trim());
                _sysConfig.setItem("AiCtx", txtCtx.Text.Trim());
                _sysConfig.setItem("AiTopK", txtTopK.Text.Trim());
                _sysConfig.setItem("AiTopP", txtTopP.Text.Trim());
                _sysConfig.setItem("AiRepeat", txtRepeat.Text.Trim());

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

        private void btnHelp_Click(object sender, EventArgs e)
        {
            string helpText = "HƯỚNG DẪN CÁC THÔNG SỐ AI:\n\n" +
                              "1. Chat Temperature (0.0 - 2.0):\n" +
                              "   - Độ sáng tạo của AI. \n" +
                              "   - Càng gần 0.0: AI trả lời cực kỳ khô khan, rập khuôn, chỉ nói sự thật.\n" +
                              "   - Càng cao: AI trả lời bay bổng, sáng tạo, nói nhiều hơn.\n\n" +
                              "2. Max Tokens (1 - 16384):\n" +
                              "   - Giới hạn độ dài tối đa của câu trả lời. Tránh việc AI trả lời quá dài hoặc bị cụt lủn.\n\n" +
                              "3. Context Window (1024 - 128000):\n" +
                              "   - 'Trí nhớ ngắn hạn' của AI. Số lượng từ vựng tối đa AI nhớ được trong 1 cuộc hội thoại. Để quá cao sẽ hao tốn RAM.\n\n" +
                              "4. Top K (1 - 100):\n" +
                              "   - Giới hạn không gian chọn từ. Giúp AI phản xạ nhanh hơn bằng cách chỉ xét các từ có khả năng cao nhất.\n\n" +
                              "5. Top P (0.0 - 1.0):\n" +
                              "   - Độ tập trung của câu trả lời. Giảm xuống giúp câu trả lời đi thẳng vào vấn đề, bớt lan man.\n\n" +
                              "6. Repeat Penalty (1.0 - 2.0):\n" +
                              "   - Hình phạt chống lặp từ. Ngăn AI bị kẹt trong vòng lặp vô tận (nói đi nói lại 1 câu). Giá trị 1.0 là không phạt.";

            XtraMessageBox.Show(helpText, "Hướng Dẫn Cấu Hình AI", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
