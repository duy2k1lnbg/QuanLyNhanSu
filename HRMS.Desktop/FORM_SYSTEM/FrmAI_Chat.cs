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
        private int _requestGeneration;
        private readonly List<SimpleButton> _activeOptionButtons = new List<SimpleButton>();

        public FrmAI_Chat()
        {
            InitializeComponent();
            _manager = new ChatboxManager(
                conversationId: null,
                executionService: null,
                remoteChatHandler: async (q, cid, opt, ver, clid, ct) =>
                {
                    var res = await AiApiClient.Instance.SendChatAsync(q, cid, opt, ver, clid, null, ct);
                    return new Bu.Services.AI_Services.Core.AiChatExecutionResult
                    {
                        Status = res.Status,
                        Answer = res.Answer,
                        ErrorCode = res.ErrorCode,
                        ConversationId = res.ConversationId,
                        ConversationVersion = res.ConversationVersion,
                        Clarification = res.Clarification,
                        InterpretedRequest = res.InterpretedRequest,
                        Data = res.Data,
                        BypassedLlm = false
                    };
                },
                remoteResetHandler: async (cid, ct) =>
                {
                    return await AiApiClient.Instance.ResetConversationAsync(cid, ct);
                }
            );
            Bu.CLASS_SYSTEM.UserSession.SessionCleared += OnSessionCleared;
            FormClosed += (s,e) => 
            { 
                Bu.CLASS_SYSTEM.UserSession.SessionCleared -= OnSessionCleared;
                _requestGeneration++; 
                _manager.Reset(); 
            };
        }

        private void OnSessionCleared()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(OnSessionCleared));
                return;
            }
            if (!IsDisposed)
            {
                ResetChatUI();
            }
            }

        private void DisableActiveClarificationButtons()
        {
            foreach (var btn in _activeOptionButtons)
            {
                if (btn != null && !btn.IsDisposed)
                {
                    btn.Enabled = false;
                }
            }
            _activeOptionButtons.Clear();
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
            _requestGeneration++;
            DisableActiveClarificationButtons();
            flpChat.Controls.Clear();
            _manager.Reset();
            txtChatInput.Enabled = true; btnChatSend.Enabled = true;

            AddMessageBubble("AI", "Xin chào! Tôi là Trợ lý AI Quản trị Nhân sự. Tôi có thể giúp gì cho bạn hôm nay?\n\nBạn có thể hỏi bất kỳ câu hỏi nghiệp vụ nào bằng tiếng Việt tự nhiên (ví dụ: 'Danh sách nhân viên sinh nhật tháng này', 'Ai chuẩn bị lên lương', 'Thống kê nhân sự theo phòng ban').");
        }

        private async void btnChatSend_Click(object sender, EventArgs e)
        {
            string query=txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(query) || !btnChatSend.Enabled) return;
            txtChatInput.Text="";
            await SendChatAsync(query);
        }
        private async Task SendChatAsync(string query, string token=null)
        {
            int generation=++_requestGeneration;
            DisableActiveClarificationButtons();
            txtChatInput.Enabled=false; btnChatSend.Enabled=false; btnClearChat.Enabled=true;
            AddMessageBubble("You",string.IsNullOrEmpty(query) ? "Đã chọn phương án làm rõ" : query);
            Panel bubble; var label=AddStreamingAiBubble(out bubble);
            try
            {
                var result=await _manager.ProcessQueryAsync(query,token);
                if (IsDisposed || generation!=_requestGeneration) return;
                UpdateStreamingAiBubble(bubble,label,result.Answer);
                if (result.Clarification != null && result.Clarification.Options != null && result.Clarification.Options.Count > 0)
                {
                    var groupButtons = new List<SimpleButton>();
                    foreach (var option in result.Clarification.Options)
                    {
                        string selectedToken = option.Token;
                        var button = new SimpleButton { Text = option.Label, AutoSize = true };
                        groupButtons.Add(button);
                        button.Click += async (s, e) =>
                        {
                            if (btnChatSend.Enabled && generation == _requestGeneration)
                            {
                                DisableActiveClarificationButtons();
                                await SendChatAsync("", selectedToken);
                            }
                        };
                        flpChat.Controls.Add(button);
                    }
                    _activeOptionButtons.AddRange(groupButtons);
                }
            }
            catch { if (!IsDisposed && generation==_requestGeneration) UpdateStreamingAiBubble(bubble,label,"Không thể xử lý yêu cầu. Vui lòng thử lại."); }
            finally { if (!IsDisposed && generation==_requestGeneration) { txtChatInput.Enabled=true; btnChatSend.Enabled=true; txtChatInput.Focus(); } }
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

        private string GetQuickActionPrompt(SimpleButton btn)
        {
            if (btn == null) return string.Empty;
            string text = btn.Text ?? "";
            if (text.IndexOf("HĐ", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("hợp đồng", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Danh sách nhân viên sắp hết hạn hợp đồng?";
            if (text.IndexOf("lương", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Danh sách nhân viên chuẩn bị tăng lương?";
            if (text.IndexOf("sinh nhật", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Danh sách nhân viên sinh nhật tháng này?";
            if (text.IndexOf("phòng ban", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Thống kê số lượng nhân viên theo từng phòng ban?";
            if (text.IndexOf("nhân viên", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Danh sách tất cả nhân viên trong công ty?";

            switch (btn.Name)
            {
                case "btnActionBirthday": return "Danh sách nhân viên sinh nhật tháng này?";
                case "btnActionSalary": return "Danh sách nhân viên chuẩn bị tăng lương?";
                case "btnActionEmployee": return "Danh sách tất cả nhân viên trong công ty?";
                case "btnActionDepartment": return "Thống kê số lượng nhân viên theo từng phòng ban?";
                default: return string.Empty;
            }
        }

        private void QuickAction_Click(object sender, EventArgs e)
        {
            if (!btnChatSend.Enabled) return; // Prevent clicking while AI is running!

            if (sender is SimpleButton btn)
            {
                string prompt = GetQuickActionPrompt(btn);
                if (string.IsNullOrEmpty(prompt)) switch (btn.Name)
                {
                    case "btnActionBirthday":
                        prompt = "Danh sách nhân viên sắp hết hạn hợp đồng?";
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
            string htmlContent = $"<b>{(isUser ? "BẠN" : "TRỢ LÝ AI")}</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/>{System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br/>")}";

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
                    string htmlContent = $"<b>TRỢ LÝ AI</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/>{System.Net.WebUtility.HtmlEncode(messageToDisplay).Replace("\n", "<br/>")}";
                    lblMsg.Text = htmlContent;

                    flpChat.SuspendLayout();
                    ResizeBubble(container);
                    flpChat.ResumeLayout(true);

                    flpChat.ScrollControlIntoView(container);
                }));
            }
            else
            {
                string htmlContent = $"<b>TRỢ LÝ AI</b> <font color='gray' size='-1'>{DateTime.Now.ToString("HH:mm")}</font><br/>{System.Net.WebUtility.HtmlEncode(messageToDisplay).Replace("\n", "<br/>")}";
                lblMsg.Text = htmlContent;

                flpChat.SuspendLayout();
                ResizeBubble(container);
                flpChat.ResumeLayout(true);

                flpChat.ScrollControlIntoView(container);
            }
        }
    }
}
