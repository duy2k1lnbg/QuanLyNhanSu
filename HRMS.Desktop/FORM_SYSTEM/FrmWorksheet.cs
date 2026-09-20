using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Drawing;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmWorksheet : DevExpress.XtraEditors.XtraForm
    {
        public FrmWorksheet(string title, string content)
        {
            InitializeComponent();
            this.Text = "Worksheet - " + title;
            txtContent.Text = content;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*";
                sfd.DefaultExt = "sql";
                sfd.FileName = "export_worksheet.sql";
                
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(sfd.FileName, txtContent.Text);
                        XtraMessageBox.Show("Lưu file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtContent.Text))
            {
                Clipboard.SetText(txtContent.Text);
                XtraMessageBox.Show("Đã chép nội dung vào Clipboard!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
