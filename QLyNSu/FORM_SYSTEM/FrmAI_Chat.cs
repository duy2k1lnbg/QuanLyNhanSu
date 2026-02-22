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
        public FrmAI_Chat()
        {
            InitializeComponent();
            this.Text = "AI Query Assistant";
            this.WindowState = FormWindowState.Maximized;

            memoQuestion.Properties.ScrollBars = ScrollBars.Vertical;
            memoQuestion.Properties.NullValuePrompt = "Nhập câu hỏi nhân sự...";

            memoSql.Properties.ReadOnly = true;
            memoSql.Properties.Appearance.Font = new Font("Consolas", 10);

            gridViewAI.OptionsBehavior.Editable = false;
            gridViewAI.OptionsView.ShowGroupPanel = false;

            btnAsk.Text = "Ask AI";
        }

        private readonly OllamaService _ollama = new OllamaService();
        private readonly AIQueryService _aiQuery = new AIQueryService();

        private void FrmAI_Chat_Load(object sender, EventArgs e)
        {

        }

        private async void btnAsk_Click(object sender, EventArgs e)
        {
            try
            {
                btnAsk.Enabled = false;
                btnAsk.Text = "Đang xử lý...";

                string question = memoQuestion.Text;

                if (string.IsNullOrWhiteSpace(question))
                {
                    XtraMessageBox.Show("Vui lòng nhập câu hỏi.");
                    return;
                }

                // 1️⃣ Gọi Ollama
                string sql = await _ollama.GenerateSqlAsync(question);

                memoSql.Text = sql;

                // 2️⃣ Validate
                if (!_aiQuery.IsSafeSelect(sql))
                {
                    XtraMessageBox.Show("SQL không hợp lệ hoặc không an toàn.");
                    return;
                }

                // 3️⃣ Execute bằng AI_READONLY
                var data = _aiQuery.Execute(sql);

                gridControlAI.DataSource = data;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
            finally
            {
                btnAsk.Enabled = true;
                btnAsk.Text = "Ask AI";
            }
        }
    }
}