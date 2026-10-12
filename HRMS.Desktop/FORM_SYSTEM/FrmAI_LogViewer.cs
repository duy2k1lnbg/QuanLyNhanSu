using DevExpress.XtraEditors;
using QLyNSu.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmAI_LogViewer : DevExpress.XtraEditors.XtraForm
    {
        private static FrmAI_LogViewer _currentInstance;

        public static void ShowViewer(Form parent = null)
        {
            if (_currentInstance == null || _currentInstance.IsDisposed)
            {
                _currentInstance = new FrmAI_LogViewer();
                _currentInstance.Show(parent);
            }
            else
            {
                _currentInstance.BringToFront();
                _currentInstance.WindowState = FormWindowState.Normal;
            }
        }

        public FrmAI_LogViewer()
        {
            InitializeComponent();
        }

        private void FrmAI_LogViewer_Load(object sender, EventArgs e)
        {
            AiLogger.LogAppended += OnLogAppended;
            ReloadLogs();
        }

        private void FrmAI_LogViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            AiLogger.LogAppended -= OnLogAppended;
        }

        private void OnLogAppended(AiLogEntry entry)
        {
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<AiLogEntry>(OnLogAppended), entry);
                }
                catch { }
                return;
            }

            if (!IsDisposed && MatchesFilter(entry))
            {
                AppendEntryToRichTextBox(entry);
                UpdateStatus();
            }
        }

        private bool MatchesFilter(AiLogEntry entry)
        {
            string selectedLevel = cboFilterLevel.Text?.Trim() ?? "Tất cả";
            if (selectedLevel != "Tất cả" && !entry.Level.ToString().Equals(selectedLevel, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string search = txtSearch.Text?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                bool matched = (entry.Message != null && entry.Message.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (entry.Category != null && entry.Category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (entry.Details != null && entry.Details.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (entry.ExceptionMessage != null && entry.ExceptionMessage.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!matched) return false;
            }

            return true;
        }

        private void ReloadLogs()
        {
            rtbLogs.SuspendLayout();
            rtbLogs.Clear();

            var logs = AiLogger.GetAllLogs();
            int count = 0;
            foreach (var entry in logs)
            {
                if (MatchesFilter(entry))
                {
                    AppendEntryToRichTextBox(entry);
                    count++;
                }
            }

            rtbLogs.ResumeLayout();

            if (chkAutoScroll.Checked)
            {
                rtbLogs.SelectionStart = rtbLogs.TextLength;
                rtbLogs.ScrollToCaret();
            }

            UpdateStatus(count, logs.Count);
        }

        private void AppendEntryToRichTextBox(AiLogEntry entry)
        {
            // Timestamp (Gray)
            AppendColoredText($"[{entry.Timestamp:HH:mm:ss.fff}] ", Color.FromArgb(140, 140, 140));

            // Level
            Color levelColor = Color.FromArgb(78, 201, 176); // Green (Info)
            switch (entry.Level)
            {
                case AiLogLevel.Warn:
                    levelColor = Color.FromArgb(206, 145, 120); // Orange
                    break;
                case AiLogLevel.Error:
                    levelColor = Color.FromArgb(244, 71, 71); // Red
                    break;
                case AiLogLevel.Debug:
                    levelColor = Color.FromArgb(156, 220, 254); // Cyan
                    break;
            }
            AppendColoredText($"[{entry.Level.ToString().ToUpperInvariant(),-5}] ", levelColor);

            // Category (Blue)
            AppendColoredText($"[{entry.Category}] ", Color.FromArgb(86, 156, 214));

            // Message (White/Normal)
            AppendColoredText($"{entry.Message}\n", Color.FromArgb(220, 225, 230));

            // Details
            if (!string.IsNullOrEmpty(entry.Details))
            {
                AppendColoredText($"    -> {entry.Details}\n", Color.FromArgb(180, 180, 180));
            }

            // Exception
            if (!string.IsNullOrEmpty(entry.ExceptionMessage))
            {
                AppendColoredText($"    [!] Lỗi: {entry.ExceptionMessage}\n", Color.FromArgb(255, 128, 128));
            }

            if (chkAutoScroll.Checked)
            {
                rtbLogs.SelectionStart = rtbLogs.TextLength;
                rtbLogs.ScrollToCaret();
            }
        }

        private void AppendColoredText(string text, Color color)
        {
            rtbLogs.SelectionStart = rtbLogs.TextLength;
            rtbLogs.SelectionLength = 0;
            rtbLogs.SelectionColor = color;
            rtbLogs.AppendText(text);
            rtbLogs.SelectionColor = rtbLogs.ForeColor;
        }

        private void UpdateStatus(int displayed = -1, int total = -1)
        {
            if (total < 0) total = AiLogger.GetAllLogs().Count;
            if (displayed < 0) displayed = total;
            lblStatus.Text = $"Hiển thị {displayed}/{total} bản ghi log | Đường dẫn file: {AiLogger.LogFilePath}";
        }

        private void cboFilterLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReloadLogs();
        }

        private void txtSearch_EditValueChanged(object sender, EventArgs e)
        {
            ReloadLogs();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ReloadLogs();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (rtbLogs.TextLength > 0)
            {
                Clipboard.SetText(rtbLogs.Text);
                XtraMessageBox.Show("Đã sao chép toàn bộ nội dung log vào bộ nhớ tạm (Clipboard)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Bạn có chắc chắn muốn xóa toàn bộ lịch sử log trên giao diện?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AiLogger.Clear();
                rtbLogs.Clear();
                UpdateStatus(0, 0);
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string path = AiLogger.LogFilePath;
                if (File.Exists(path))
                {
                    Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
                else
                {
                    string dir = Path.GetDirectoryName(path);
                    if (Directory.Exists(dir))
                    {
                        Process.Start("explorer.exe", $"\"{dir}\"");
                    }
                    else
                    {
                        XtraMessageBox.Show("Thư mục lưu trữ log chưa được tạo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Không thể mở thư mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
