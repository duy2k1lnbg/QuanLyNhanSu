using DevExpress.XtraEditors;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.Services.AI_Services.Core;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmOllamaConfig : DevExpress.XtraEditors.XtraForm
    {
        private long _currentVersion = 1;
        private Functions.AiClientConfigResult _originalSnapshot;

        public FrmOllamaConfig()
        {
            InitializeComponent();
        }

        private async void FrmOllamaConfig_Load(object sender, EventArgs e)
        {
            Functions.TranslationManager.Translate(this);
            await LoadConfigFromApiAsync();
        }

        private async Task LoadConfigFromApiAsync()
        {
            // Kiểm tra token xác thực
            if (!Functions.AiApiClient.Instance.HasToken)
            {
                btnSave.Enabled = false;
                btnTest.Enabled = false;
                btnTestQdrant.Enabled = false;

                // Nạp giá trị hiển thị mặc định
                txtUrl.Text = "http://localhost:11434";
                txtModel.Text = "qwen2.5:latest";
                txtQdrant.Text = "http://127.0.0.1:6333";
                txtTemp.Text = "0.4";
                txtMaxTokens.Text = "1000";
                txtCtx.Text = "3072";
                txtTopK.Text = "30";
                txtTopP.Text = "0.8";
                txtRepeat.Text = "1.15";

                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Chưa có phiên làm việc xác thực. Bạn cần đăng nhập tài khoản quản trị để xem và lưu cấu hình AI."),
                    Functions.TranslationManager.Translate("Cảnh báo xác thực"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                var res = await Functions.AiApiClient.Instance.GetAiConfigAsync();
                if (res.Success)
                {
                    _originalSnapshot = res;
                    _currentVersion = res.Version;

                    txtUrl.Text = res.OllamaHost ?? "http://localhost:11434";
                    txtModel.Text = res.AiModel ?? "qwen2.5:latest";
                    txtQdrant.Text = res.QdrantUrl ?? "http://127.0.0.1:6333";
                    txtTemp.Text = res.AiTemp.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    txtMaxTokens.Text = res.AiMaxTokens.ToString();
                    txtCtx.Text = res.AiCtx.ToString();
                    txtTopK.Text = res.AiTopK.ToString();
                    txtTopP.Text = res.AiTopP.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    txtRepeat.Text = res.AiRepeat.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

                    btnSave.Enabled = true;
                    btnTest.Enabled = true;
                    btnTestQdrant.Enabled = true;
                }
                else
                {
                    btnSave.Enabled = false;
                    btnTest.Enabled = false;
                    btnTestQdrant.Enabled = false;

                    XtraMessageBox.Show(
                        res.Message ?? Functions.TranslationManager.Translate("Không thể tải cấu hình AI từ máy chủ API."),
                        Functions.TranslationManager.Translate("Quyền truy cập bị từ chối"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                btnSave.Enabled = false;
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Lỗi khi tải cấu hình từ backend:") + " " + ex.Message,
                    Functions.TranslationManager.Translate("Lỗi kết nối"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            if (!Functions.AiApiClient.Instance.HasToken)
            {
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Bạn cần đăng nhập để kiểm tra kết nối từ máy chủ API."),
                    Functions.TranslationManager.Translate("Cảnh báo"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            btnTest.Enabled = false;
            btnTest.Text = Functions.TranslationManager.Translate("Đang thử...");
            try
            {
                var res = await Functions.AiApiClient.Instance.TestAiConfigDraftAsync("OLLAMA", url, txtModel.Text.Trim());

                if (res.Success)
                {
                    XtraMessageBox.Show(
                        (res.Message ?? Functions.TranslationManager.Translate("Kết nối đến Ollama Server thành công!")) +
                        "\n\n(" + Functions.TranslationManager.Translate("Kiểm tra kết nối thực hiện từ máy chủ Backend API.") + ")",
                        Functions.TranslationManager.Translate("Thông báo"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Kết nối Ollama thất bại: ") + (string.IsNullOrEmpty(res.Message) ? "Không thể kết nối đến máy chủ." : res.Message) +
                        "\n\n(" + Functions.TranslationManager.Translate("Kiểm tra kết nối thực hiện từ máy chủ Backend API.") + ")",
                        Functions.TranslationManager.Translate("Lỗi"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Không thể kiểm tra kết nối:") + " " + ex.Message,
                    Functions.TranslationManager.Translate("Lỗi"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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

            if (!Functions.AiApiClient.Instance.HasToken)
            {
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Bạn cần đăng nhập để kiểm tra kết nối từ máy chủ API."),
                    Functions.TranslationManager.Translate("Cảnh báo"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            btnTestQdrant.Enabled = false;
            btnTestQdrant.Text = Functions.TranslationManager.Translate("Đang thử...");
            try
            {
                var res = await Functions.AiApiClient.Instance.TestAiConfigDraftAsync("QDRANT", url);

                if (res.Success)
                {
                    XtraMessageBox.Show(
                        (res.Message ?? Functions.TranslationManager.Translate("Kết nối đến Qdrant Server thành công!")) +
                        "\n\n(" + Functions.TranslationManager.Translate("Kiểm tra kết nối thực hiện từ máy chủ Backend API.") + ")",
                        Functions.TranslationManager.Translate("Thông báo"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Kết nối Qdrant thất bại: ") + (string.IsNullOrEmpty(res.Message) ? "Không thể kết nối đến máy chủ." : res.Message) +
                        "\n\n(" + Functions.TranslationManager.Translate("Kiểm tra kết nối thực hiện từ máy chủ Backend API.") + ")",
                        Functions.TranslationManager.Translate("Lỗi"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Không thể kết nối đến Qdrant:") + " " + ex.Message,
                    Functions.TranslationManager.Translate("Lỗi"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnTestQdrant.Enabled = true;
                btnTestQdrant.Text = Functions.TranslationManager.Translate("Kiểm Tra Kết Nối");
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string url = txtUrl.Text.Trim().TrimEnd('/');
                string model = txtModel.Text.Trim();
                string qdrantUrl = txtQdrant.Text.Trim().TrimEnd('/');

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(model) || string.IsNullOrEmpty(qdrantUrl))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Vui lòng điền đầy đủ thông tin địa chỉ URL và Model."),
                        Functions.TranslationManager.Translate("Cảnh báo"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Kiểm tra số thống nhất, chặn NaN/Infinity, xử lý cả định dạng phẩy và chấm
                if (!AiConfigurationCoordinator.ParseInvariantDouble(txtTemp.Text, 0.4, out double temp, 0.0, 2.0))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Chat Temperature phải là số từ 0.0 đến 2.0."),
                        Functions.TranslationManager.Translate("Lỗi nhập liệu"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtTemp.Focus();
                    return;
                }

                if (!AiConfigurationCoordinator.ParseInvariantInt(txtMaxTokens.Text, 1000, out int maxTokens, 1, 16384))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Max Tokens phải là số nguyên từ 1 đến 16384."),
                        Functions.TranslationManager.Translate("Lỗi nhập liệu"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtMaxTokens.Focus();
                    return;
                }

                if (!AiConfigurationCoordinator.ParseInvariantInt(txtCtx.Text, 3072, out int ctx, 1024, 128000))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Context Window phải là số nguyên từ 1024 đến 128000."),
                        Functions.TranslationManager.Translate("Lỗi nhập liệu"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtCtx.Focus();
                    return;
                }

                if (!AiConfigurationCoordinator.ParseInvariantInt(txtTopK.Text, 30, out int topk, 1, 100))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Top K phải là số nguyên từ 1 đến 100."),
                        Functions.TranslationManager.Translate("Lỗi nhập liệu"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtTopK.Focus();
                    return;
                }

                if (!AiConfigurationCoordinator.ParseInvariantDouble(txtTopP.Text, 0.8, out double topp, 0.0, 1.0))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Top P phải là số từ 0.0 đến 1.0."),
                        Functions.TranslationManager.Translate("Lỗi nhập liệu"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtTopP.Focus();
                    return;
                }

                if (!AiConfigurationCoordinator.ParseInvariantDouble(txtRepeat.Text, 1.15, out double repeat, 0.01, 2.0))
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Repeat Penalty phải là số lớn hơn 0.0 và nhỏ hơn hoặc bằng 2.0."),
                        Functions.TranslationManager.Translate("Lỗi nhập liệu"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtRepeat.Focus();
                    return;
                }

                // Không có token: Bắt buộc từ chối lưu, không ghi trực tiếp CSDL
                if (!Functions.AiApiClient.Instance.HasToken)
                {
                    XtraMessageBox.Show(
                        Functions.TranslationManager.Translate("Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn. Vui lòng đăng nhập lại tài khoản quản trị để lưu cấu hình AI."),
                        Functions.TranslationManager.Translate("Xác thực thất bại"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                btnSave.Enabled = false;

                // Gửi payload chứa ExpectedVersion đến Backend API duy nhất
                var updatePayload = new
                {
                    OllamaHost = url,
                    AiModel = model,
                    QdrantUrl = qdrantUrl,
                    AiTemp = temp,
                    AiMaxTokens = maxTokens,
                    AiCtx = ctx,
                    AiTopK = topk,
                    AiTopP = topp,
                    AiRepeat = repeat,
                    ExpectedVersion = _currentVersion
                };

                var saveRes = await Functions.AiApiClient.Instance.SaveAiConfigAsync(updatePayload);

                if (!saveRes.Success)
                {
                    // Xung đột phiên bản 409
                    if (saveRes.StatusCode == 409)
                    {
                        var confirmReload = XtraMessageBox.Show(
                            (saveRes.Message ?? Functions.TranslationManager.Translate("Cấu hình đã được chỉnh sửa bởi phiên làm việc khác.")) +
                            "\n\n" + Functions.TranslationManager.Translate("Bạn có muốn tải lại phiên bản mới nhất từ máy chủ không? (Lưu ý: Các thay đổi vừa nhập trên form sẽ bị hủy bỏ)."),
                            Functions.TranslationManager.Translate("Xung đột phiên bản"),
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (confirmReload == DialogResult.Yes)
                        {
                            await LoadConfigFromApiAsync();
                        }
                        // Giữ form mở, không ghi đè, không ghi CSDL
                        return;
                    }

                    // Lỗi phân quyền 403 / 401 hoặc lỗi hệ thống khác
                    XtraMessageBox.Show(
                        saveRes.Message ?? Functions.TranslationManager.Translate("Không thể lưu cấu hình qua Backend API."),
                        Functions.TranslationManager.Translate("Lỗi lưu cấu hình"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Giữ nguyên form và bản nháp để người dùng không mất công nhập
                    return;
                }

                // Lưu API thành công: cập nhật version, đóng form
                _currentVersion = saveRes.Version;
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Đã lưu thiết lập cấu hình AI thành công cho toàn hệ thống."),
                    Functions.TranslationManager.Translate("Thông báo"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    Functions.TranslationManager.Translate("Lỗi khi gửi yêu cầu lưu cấu hình:") + " " + ex.Message,
                    Functions.TranslationManager.Translate("Lỗi"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnSave.Enabled = true;
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
                              "   - Số lượng token ngữ cảnh AI có thể nhớ trong một phiên hội thoại.\n\n" +
                              "4. Top K (1 - 100) & Top P (0.0 - 1.0):\n" +
                              "   - Tham số lấy mẫu xác suất từ ngữ cho mô hình ngôn ngữ lớn.\n\n" +
                              "5. Repeat Penalty (0.01 - 2.0):\n" +
                              "   - Hệ số phạt lặp từ để AI không bị lặp lại các cụm từ giống nhau liên tục.\n\n" +
                              "LƯU Ý BẢO MẬT:\n" +
                              "- Mọi thay đổi cấu hình được kiểm soát thẩm quyền qua Backend API (yêu cầu F_SYSTEM_AI_CONFIG).\n" +
                              "- Thao tác lưu có kiểm soát phiên bản (ExpectedVersion) để chống xung đột ghi đè đồng thời.";

            XtraMessageBox.Show(helpText, Functions.TranslationManager.Translate("Trợ giúp thông số"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
