using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
        private bool _resizingBubbles;
        private sealed class BubbleState
        {
            public bool IsUser;
            public string Time = DateTime.Now.ToString("HH:mm");
            public readonly List<Panel> Panels = new List<Panel>();
        }
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
            ClearChatControls();
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
            ClearChatControls();
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
            AiLogger.Info("CHAT_UI", $"Người dùng gửi câu hỏi: '{(string.IsNullOrEmpty(query) ? $"Làm rõ option [{token}]" : query)}'");
            try
            {
                var result=await _manager.ProcessQueryAsync(query,token);
                if (IsDisposed || Disposing || generation!=_requestGeneration) return;
                AiLogger.Info("CHAT_UI", $"Nhận phản hồi AI: Status={result.Status}, Trả lời={result.Answer?.Length ?? 0} ký tự");
                if (!await StreamTypingEffectAsync(bubble, label, result.Answer, generation)) return;
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
                    ScrollChatToBottom();
                }
            }
            catch (Exception ex)
            {
                AiLogger.Error("CHAT_UI", "Lỗi xử lý yêu cầu chat trên giao diện: " + ex.Message, ex);
                if (!IsDisposed && generation==_requestGeneration) UpdateStreamingAiBubble(bubble,label,"Không thể xử lý yêu cầu. Vui lòng bấm 'Nhật ký AI' ở góc trên để xem chi tiết lỗi kết nối."); 
            }
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

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            FrmAI_LogViewer.ShowViewer(this);
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
            if (_resizingBubbles) return;
            _resizingBubbles = true;
            bool wasAtBottom = flpChat.VerticalScroll.Value >= flpChat.VerticalScroll.Maximum - flpChat.VerticalScroll.LargeChange - 4;
            try
            {
                flpChat.SuspendLayout();
                foreach (var panel in flpChat.Controls.OfType<Panel>()) ResizeBubble(panel);
            }
            finally
            {
                flpChat.ResumeLayout(true);
                _resizingBubbles = false;
            }
            if (!btnChatSend.Enabled || wasAtBottom) ScrollChatToBottom();
        }

        private void ClearChatControls()
        {
            // Clear() detaches controls without disposing their fonts and native handles.
            foreach (Control control in flpChat.Controls.Cast<Control>().ToArray()) control.Dispose();
            flpChat.AutoScrollMinSize = Size.Empty;
            flpChat.AutoScrollPosition = Point.Empty;
        }

        private void ScrollChatToBottom()
        {
            if (IsDisposed || Disposing || !flpChat.IsHandleCreated) return;
            flpChat.PerformLayout();
            // FlowLayoutPanel can cache a shorter extent after labels grow or wrap.
            // Include the actual last control and its margins in the virtual scroll area.
            int contentBottom = flpChat.Controls.Cast<Control>()
                .Select(control => control.Bottom + control.Margin.Bottom - flpChat.AutoScrollPosition.Y)
                .DefaultIfEmpty(0).Max() + flpChat.Padding.Bottom;
            flpChat.AutoScrollMinSize = new Size(0, contentBottom);
            flpChat.PerformLayout();
            // Scrollbar range includes padding, including at non-default DPI.
            int bottom = Math.Max(0, flpChat.VerticalScroll.Maximum - flpChat.VerticalScroll.LargeChange + 1);
            flpChat.AutoScrollPosition = new Point(0, bottom);
        }

        private void ResizeBubble(Panel container)
        {
            if (container == null || container.Controls.Count == 0) return;
            var label = container.Controls[0] as LabelControl;
            if (label == null) return;
            // Reserve the scrollbar width even before it appears, avoiding width/height oscillation.
            container.Width = Math.Max(140, flpChat.Width - flpChat.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 8);
            int targetWidth = Math.Max(100, (int)(container.Width * 0.82));
            label.Width = targetWidth;
            label.Height = label.GetPreferredSize(new Size(targetWidth, 0)).Height;
            bool isUser = "User".Equals(label.Tag as string);
            label.Location = new Point(isUser ? container.Width - label.Width - 10 : 10, 5);
            container.Height = label.Height + 10;
        }

        private static string FormatDevExpressHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("\n", "<br/>");
        }


        private Panel CreateBubblePanel(BubbleState state)
        {
            var panel = new Panel { Height = 50, Padding = new Padding(5), Tag = state };
            var label = new LabelControl
            {
                AllowHtmlString = true,
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Tag = state.IsUser ? "User" : "AI",
                Padding = new Padding(12)
            };
            label.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            label.Appearance.Font = new Font("Segoe UI", 10F);
            label.Appearance.BackColor = state.IsUser ? Color.FromArgb(220, 235, 252) : Color.FromArgb(241, 245, 249);
            label.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            panel.Controls.Add(label);
            state.Panels.Add(panel);
            flpChat.Controls.Add(panel);
            return panel;
        }

        private void RenderBubble(BubbleState state, string text)
        {
            var chunks = AiTypingTimeline.SplitForDisplay(text);
            flpChat.SuspendLayout();
            try
            {
                while (state.Panels.Count < chunks.Count) CreateBubblePanel(state);
                while (state.Panels.Count > chunks.Count)
                {
                    var last = state.Panels[state.Panels.Count - 1];
                    state.Panels.RemoveAt(state.Panels.Count - 1);
                    last.Dispose();
                }
                for (int i = 0; i < chunks.Count; i++)
                {
                    var panel = state.Panels[i];
                    var label = (LabelControl)panel.Controls[0];
                    string header = i == 0
                        ? $"<b>{(state.IsUser ? "BẠN" : "TRỢ LÝ AI")}</b> <font color='gray' size='-1'>{state.Time}</font><br/>"
                        : string.Empty;
                    string html = header + FormatDevExpressHtml(chunks[i]);
                    if (label.Text != html)
                    {
                        label.Text = html;
                        ResizeBubble(panel);
                    }
                }
            }
            finally { flpChat.ResumeLayout(true); }
            ScrollChatToBottom();
        }

        private void AddMessageBubble(string sender, string message)
        {
            RenderBubble(new BubbleState { IsUser = sender.Equals("You", StringComparison.OrdinalIgnoreCase) }, message);
        }

        private async Task<bool> StreamTypingEffectAsync(Panel container, LabelControl label, string fullText, int generation)
        {
            fullText = fullText ?? string.Empty;
            var timeline = new AiTypingTimeline(fullText);
            var clock = Stopwatch.StartNew();
            int previousLength = -1;
            while (IsCurrentChatBubble(container, label, generation))
            {
                int length = timeline.VisibleLength(clock.Elapsed.TotalMilliseconds);
                if (length != previousLength)
                {
                    UpdateStreamingAiBubble(container, label, fullText.Substring(0, length));
                    previousLength = length;
                }
                if (length == fullText.Length) return true;
                await Task.Delay(30);
            }
            return false;
        }

        private bool IsCurrentChatBubble(Panel container, LabelControl label, int generation)
        {
            return !IsDisposed && !Disposing && generation == _requestGeneration
                && !container.IsDisposed && !label.IsDisposed && container.Parent == flpChat;
        }

        private LabelControl AddStreamingAiBubble(out Panel container)
        {
            var state = new BubbleState();
            RenderBubble(state, "AI đang phân tích và truy vấn dữ liệu...");
            container = state.Panels[0];
            return (LabelControl)container.Controls[0];
        }

        private void UpdateStreamingAiBubble(Panel container, LabelControl label, string text)
        {
            if (container.IsDisposed || label.IsDisposed || IsDisposed || Disposing) return;
            if (label.InvokeRequired)
            {
                label.BeginInvoke(new Action(() => UpdateStreamingAiBubble(container, label, text)));
                return;
            }
            if (container.Tag is BubbleState state) RenderBubble(state, text);
        }
    }
}
