using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.Services.AI_Services;
using QLyNSu.Functions;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmAI_Chat : DevExpress.XtraEditors.XtraForm
    {
        private readonly ChatboxManager _manager;

        public FrmAI_Chat()
        {
            InitializeComponent();
            _manager = new ChatboxManager();
        }

        private void FrmAI_Chat_Load(object sender, EventArgs e)
        {
            LoadHistoryOrInit();
        }

        private void LoadHistoryOrInit()
        {
            flpChat.Controls.Clear();
            var history = _manager.GetMessages();

            if (history == null || history.Count == 0)
            {
                AddMessageBubble("AI", "Xin chào! Tôi là Trợ lý AI Quản trị Nhân sự. Tôi có thể giúp gì cho bạn hôm nay?\n\nBạn có thể hỏi bất kỳ câu hỏi nghiệp vụ nào bằng tiếng Việt tự nhiên (ví dụ: 'Danh sách nhân viên sinh nhật tháng này', 'Ai chuẩn bị lên lương', 'Thống kê nhân sự theo phòng ban').");
            }
            else
            {
                foreach (var msg in history)
                {
                    string sender = msg.Role == "User" ? "You" : "AI";
                    AddMessageBubble(sender, msg.Content);
                }
            }
        }

        private void ResetChatUI()
        {
            flpChat.Controls.Clear();
            _manager.Reset();

            AddMessageBubble("AI", "Xin chào! Tôi là Trợ lý AI Quản trị Nhân sự. Tôi có thể giúp gì cho bạn hôm nay?\n\nBạn có thể hỏi bất kỳ câu hỏi nghiệp vụ nào bằng tiếng Việt tự nhiên (ví dụ: 'Danh sách nhân viên sinh nhật tháng này', 'Ai chuẩn bị lên lương', 'Thống kê nhân sự theo phòng ban').");
        }

        private async void btnChatSend_Click(object sender, EventArgs e)
        {
            string query = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            // Display user message bubble
            AddMessageBubble("You", query);
            txtChatInput.Text = string.Empty;

            // Lock UI during AI execution
            txtChatInput.Enabled = false;
            btnChatSend.Enabled = false;
            btnClearChat.Enabled = false;
            
            // Create the streaming AI response bubble with initial loading state
            Panel aiBubbleContainer;
            var aiLabelMsg = AddStreamingAiBubble(out aiBubbleContainer);
            string accumMessage = "";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            long lastUpdateMs = 0;

            try
            {
                // Process query via BLL, streaming tokens via callback
                var result = await _manager.ProcessQuery(query, token =>
                {
                    accumMessage += token;
                    long elapsed = sw.ElapsedMilliseconds;
                    // Throttle UI update: update at most every 80ms, or when token is empty (end of stream)
                    if (elapsed - lastUpdateMs >= 80 || string.IsNullOrEmpty(token))
                    {
                        lastUpdateMs = elapsed;
                        string msgToDisplay = accumMessage;
                        UpdateStreamingAiBubble(aiBubbleContainer, aiLabelMsg, msgToDisplay);
                    }
                });

                // Final UI update to ensure all buffered tokens are rendered
                UpdateStreamingAiBubble(aiBubbleContainer, aiLabelMsg, accumMessage);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(accumMessage))
                {
                    accumMessage = "Rất tiếc! Đã xảy ra lỗi khi kết nối và xử lý yêu cầu của bạn: " + ex.Message;
                    UpdateStreamingAiBubble(aiBubbleContainer, aiLabelMsg, accumMessage);
                }
                else
                {
                    accumMessage += $"\n[Lỗi dòng dữ liệu: {ex.Message}]";
                    UpdateStreamingAiBubble(aiBubbleContainer, aiLabelMsg, accumMessage);
                }
            }
            finally
            {
                txtChatInput.Enabled = true;
                btnChatSend.Enabled = true;
                btnClearChat.Enabled = true;
                txtChatInput.Focus();
            }
        }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnChatSend_Click(sender, EventArgs.Empty);
            }
        }

        private void btnClearChat_Click(object sender, EventArgs e)
        {
            ResetChatUI();
        }

        private async void btnOpenDashboard_Click(object sender, EventArgs e)
        {
            var formManager = new FormManager_Functions(this.MdiParent ?? this);
            await formManager.OpenFormWithSplashScreen(typeof(FrmAI));
        }

        private void QuickAction_Click(object sender, EventArgs e)
        {
            if (!btnChatSend.Enabled) return; // Prevent clicking while AI is running!

            if (sender is SimpleButton btn)
            {
                string prompt = "";
                switch (btn.Name)
                {
                    case "btnActionBirthday":
                        prompt = "Ai sinh nhật trong tháng này?";
                        break;
                    case "btnActionSalary":
                        prompt = "Danh sách nhân viên chuẩn bị tăng lương?";
                        break;
                    case "btnActionEmployee":
                        prompt = "Danh sách tất cả nhân viên trong công ty?";
                        break;
                    case "btnActionDepartment":
                        prompt = "Thống kê số lượng nhân viên theo từng phòng ban?";
                        break;
                }

                if (!string.IsNullOrEmpty(prompt))
                {
                    txtChatInput.Text = prompt;
                    btnChatSend_Click(sender, EventArgs.Empty);
                }
            }
        }

        private void flpChat_SizeChanged(object sender, EventArgs e)
        {
            flpChat.SuspendLayout();
            int width = flpChat.ClientSize.Width - 30;
            foreach (Control ctrl in flpChat.Controls)
            {
                if (ctrl is Panel container)
                {
                    container.Width = width;
                    ResizeBubble(container);
                }
            }
            flpChat.ResumeLayout(true);
        }

        // ================= HELPER METHODS FOR CHAT BUBBLES =================

        private void ResizeBubble(Panel container)
        {
            if (container == null || container.Controls.Count == 0) return;
            var lblMsg = container.Controls[0] as LabelControl;
            if (lblMsg == null) return;

            bool isUser = "User".Equals(lblMsg.Tag as string);

            // Calculate label width (82% of container width)
            int targetWidth = (int)(container.Width * 0.82);
            if (targetWidth < 100) targetWidth = 100;
            lblMsg.Width = targetWidth;

            // Recalculate preferred height based on the new width
            int preferredHeight = lblMsg.GetPreferredSize(new Size(targetWidth, 10000)).Height + 24;
            lblMsg.Height = preferredHeight;

            if (isUser)
            {
                lblMsg.Location = new Point(container.Width - lblMsg.Width - 10, 5);
            }
            else
            {
                lblMsg.Location = new Point(10, 5);
            }

            container.Height = lblMsg.Height + 10;
        }

        private void AddMessageBubble(string sender, string message)
        {
            bool isUser = sender.Equals("You", StringComparison.OrdinalIgnoreCase);

            var container = new Panel
            {
                Width = flpChat.ClientSize.Width - 30,
                Height = 50,
                Padding = new Padding(5)
            };

            // Format message content
            string htmlContent = $"<b>{(isUser ? "BẠN" : "TRỢ LÝ AI")}</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/>{message.Replace("\n", "<br/>")}";

            var lblMsg = new LabelControl
            {
                Text = htmlContent,
                AllowHtmlString = true,
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Width = (int)(container.Width * 0.82),
                Tag = isUser ? "User" : "AI"
            };

            lblMsg.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblMsg.Appearance.Font = new Font("Segoe UI", 10F);
            lblMsg.Padding = new Padding(12);

            // Style and align bubble
            if (isUser)
            {
                lblMsg.Appearance.BackColor = Color.FromArgb(220, 235, 252); // Soft blue
                lblMsg.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            }
            else
            {
                lblMsg.Appearance.BackColor = Color.FromArgb(241, 245, 249); // Soft gray
                lblMsg.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            }

            container.Controls.Add(lblMsg);

            flpChat.SuspendLayout();
            flpChat.Controls.Add(container);
            ResizeBubble(container);
            flpChat.ResumeLayout(true);

            flpChat.ScrollControlIntoView(container);
        }

        private Panel AddThinkingBubble()
        {
            var container = new Panel
            {
                Width = flpChat.ClientSize.Width - 30,
                Height = 50,
                Padding = new Padding(5)
            };

            var lblMsg = new LabelControl
            {
                Text = "<i>AI đang phân tích và truy vấn dữ liệu...</i>",
                AllowHtmlString = true,
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Width = 250,
                Tag = "AI"
            };

            lblMsg.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblMsg.Appearance.Font = new Font("Segoe UI", 9.5f);
            lblMsg.Padding = new Padding(12);
            lblMsg.Appearance.BackColor = Color.FromArgb(254, 243, 199); // Soft gold/yellow
            lblMsg.Appearance.ForeColor = Color.FromArgb(146, 64, 14);

            container.Controls.Add(lblMsg);

            flpChat.SuspendLayout();
            flpChat.Controls.Add(container);
            ResizeBubble(container);
            flpChat.ResumeLayout(true);

            flpChat.ScrollControlIntoView(container);

            return container;
        }

        private string FormatSql(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return "";
            // Clean up basic string formatting
            return sql.Replace("SELECT", "SELECT\n   ")
                      .Replace("FROM", "\nFROM\n   ")
                      .Replace("LEFT JOIN", "\nLEFT JOIN\n   ")
                      .Replace("WHERE", "\nWHERE\n   ")
                      .Replace("AND", "\n   AND")
                      .Replace("ORDER BY", "\nORDER BY\n   ");
        }

        // ================= STREAMING CHAT HELPER METHODS =================

        private LabelControl AddStreamingAiBubble(out Panel container)
        {
            container = new Panel
            {
                Width = flpChat.ClientSize.Width - 30,
                Height = 50,
                Padding = new Padding(5)
            };

            string htmlContent = $"<b>TRỢ LÝ AI</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/><i>AI đang phân tích và truy vấn dữ liệu...</i>";

            var lblMsg = new LabelControl
            {
                Text = htmlContent,
                AllowHtmlString = true,
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Width = (int)(container.Width * 0.82),
                Tag = "AI"
            };

            lblMsg.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblMsg.Appearance.Font = new Font("Segoe UI", 10F);
            lblMsg.Padding = new Padding(12);
            lblMsg.Appearance.BackColor = Color.FromArgb(241, 245, 249); // Soft gray
            lblMsg.Appearance.ForeColor = Color.FromArgb(15, 23, 42);

            container.Controls.Add(lblMsg);

            flpChat.SuspendLayout();
            flpChat.Controls.Add(container);
            ResizeBubble(container);
            flpChat.ResumeLayout(true);

            flpChat.ScrollControlIntoView(container);

            return lblMsg;
        }

        private void UpdateStreamingAiBubble(Panel container, LabelControl lblMsg, string messageToDisplay)
        {
            if (lblMsg.InvokeRequired)
            {
                lblMsg.BeginInvoke(new Action(() =>
                {
                    string htmlContent = $"<b>TRỢ LÝ AI</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/>{messageToDisplay.Replace("\n", "<br/>")}";
                    lblMsg.Text = htmlContent;

                    flpChat.SuspendLayout();
                    ResizeBubble(container);
                    flpChat.ResumeLayout(true);

                    flpChat.ScrollControlIntoView(container);
                }));
            }
            else
            {
                string htmlContent = $"<b>TRỢ LÝ AI</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/>{messageToDisplay.Replace("\n", "<br/>")}";
                lblMsg.Text = htmlContent;

                flpChat.SuspendLayout();
                ResizeBubble(container);
                flpChat.ResumeLayout(true);

                flpChat.ScrollControlIntoView(container);
            }
        }
    }
}
