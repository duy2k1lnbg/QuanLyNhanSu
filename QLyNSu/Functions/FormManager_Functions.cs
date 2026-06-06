using DevExpress.XtraSplashScreen;
using QLyNSu.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu
{
    public class FormManager_Functions
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Form _parentForm;

        public FormManager_Functions(Form parentForm)
        {
            _parentForm = parentForm;
        }

        public void OpenForm(Type typeForm)
        {
            foreach (var frm in _parentForm.MdiChildren)
            {
                if (frm.GetType() == typeForm)
                {
                    frm.Activate();
                    return;
                }
            }

            Form f = (Form)Activator.CreateInstance(typeForm);
            f.MdiParent = _parentForm;
            f.Show();
        }

        public async Task OpenFormWithSplashScreen(Type typeForm)
        {
            SplashScreenManager.ShowForm(_parentForm, typeof(FrmWaiting), true, true, false);

            try
            {
                if (!_semaphore.Wait(0))
                {
                    MessageBox.Show("Hệ thống đang bận, vui lòng chờ một chút.");
                    return;
                }

                await OpenFormAsync(typeForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}");
            }
            finally
            {
                SplashScreenManager.CloseForm();
                _semaphore.Release();
            }
        }

        private async Task OpenFormAsync(Type typeForm)
        {
            foreach (var frm in _parentForm.MdiChildren)
            {
                if (frm.GetType() == typeForm)
                {
                    frm.Activate();
                    return;
                }
            }

            Form f = (Form)Activator.CreateInstance(typeForm);
            f.MdiParent = _parentForm;
            await Task.Run(() => _parentForm.Invoke((MethodInvoker)(() => f.Show())));
        }

        public void CloseAll()
        {
            if (_parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new MethodInvoker(CloseAll));
            }
            else
            {
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    AiBootstrap.StopOllama();
                    Application.Exit();
                }
            }
        }

        private static void ApplyModernGridStyle(DevExpress.XtraGrid.Views.Grid.GridView gridView)
        {
            // Configure modern grid view appearance
            gridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.True;
            gridView.OptionsView.EnableAppearanceEvenRow = true;

            // Row heights for padding/spacing (web-like padding)
            gridView.RowHeight = 36;
            gridView.ColumnPanelRowHeight = 40;

            // Soft selection and alternating row colors
            gridView.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            gridView.Appearance.OddRow.BackColor = System.Drawing.Color.White;
            gridView.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(222, 236, 253);
            gridView.Appearance.FocusedRow.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            gridView.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(233, 242, 255);
            gridView.Appearance.SelectedRow.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);

            // Hide cell focus border rectangle for cleaner look
            gridView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;

            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridView.Columns)
            {
                column.AppearanceHeader.Options.UseTextOptions = true;
                column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                column.AppearanceHeader.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                column.AppearanceCell.Font = new System.Drawing.Font("Segoe UI", 10);
            }

            gridView.Appearance.HeaderPanel.Options.UseFont = true;
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        public static void CustomView(DevExpress.XtraGrid.Views.Grid.GridView gridView, string layoutPath)
        {
            // Khôi phục layout từ file XML
            gridView.RestoreLayoutFromXml(layoutPath);
            gridView.OptionsBehavior.Editable = false;

            ApplyModernGridStyle(gridView);

            gridView.OptionsView.ColumnAutoWidth = true;
            gridView.BestFitColumns();
        }

        public static void CustomView_Colums(DevExpress.XtraGrid.Views.Grid.GridView gridView)
        {
            gridView.OptionsBehavior.Editable = false;

            gridView.OptionsMenu.EnableColumnMenu = false;
            gridView.OptionsMenu.EnableGroupPanelMenu = false;
            gridView.OptionsCustomization.AllowFilter = false;

            ApplyModernGridStyle(gridView);

            gridView.OptionsView.ColumnAutoWidth = true;
            gridView.BestFitColumns();
        }

        public void OpenForm_NewTap(Type typeForm)
        {
            // Kiểm tra nếu form đã được mở
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm.GetType() == typeForm)
                {
                    openForm.Activate();
                    return;
                }
            }

            // Tạo form mới và hiển thị
            Form form = (Form)Activator.CreateInstance(typeForm);
            form.Show();
        }
    }
}
