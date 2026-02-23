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
using Bu.Services;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmAI_Chat : DevExpress.XtraEditors.XtraForm
    {
        private readonly ChatboxManager _manager;

        public FrmAI_Chat()
        {
            InitializeComponent();
            _manager = new ChatboxManager();

            // Set giao diện ban đầu
            AppendLog("Trợ lý ", "Xin chào! Tôi có thể giúp gì cho bạn trong việc quản lý nhân sự hôm nay?");
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string query = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            // Hiển thị câu hỏi của User
            AppendLog("You", query);
            txtInput.Text = string.Empty;

            // Khóa UI chờ AI trả lời
            btnSend.Enabled = false;
            lblStatus.Text = "AI đang suy nghĩ...";

            try
            {
                // Gọi sang tầng BUS để xử lý 14 Views và Ollama
                string result = await _manager.ProcessQuery(query);

                // Hiển thị câu trả lời
                AppendLog("AI", result);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSend.Enabled = true;
                lblStatus.Text = "Sẵn sàng";
                txtInput.Focus();
            }
        }

        private void AppendLog(string sender, string msg)
        {
            string time = DateTime.Now.ToString("HH:mm");
            string formattedMsg = $"[{time}] {sender.ToUpper()}: {msg}{Environment.NewLine}{Environment.NewLine}";

            memChatLog.Text += formattedMsg;

            // Tự động cuộn xuống dòng cuối cùng
            memChatLog.SelectionStart = memChatLog.Text.Length;
            memChatLog.ScrollToCaret();
        }
    }
}
