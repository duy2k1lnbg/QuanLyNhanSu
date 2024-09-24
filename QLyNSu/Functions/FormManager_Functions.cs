using DevExpress.XtraSplashScreen;
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
                    Application.Exit();
                }
            }
        }

        public static void CustomView(DevExpress.XtraGrid.Views.Grid.GridView gridView, string layoutPath)
        {
            // Khôi phục layout từ file XML
            gridView.RestoreLayoutFromXml(layoutPath);
            //gridView.GridControl.DataSource = dataSource;
            gridView.OptionsBehavior.Editable = false;

            // Căn giữa các tiêu đề cột
            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridView.Columns)
            {
                column.AppearanceHeader.Options.UseTextOptions = true;
                column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
        }

        public static void CustomView_Colums(DevExpress.XtraGrid.Views.Grid.GridView gridView)
        {
            //gridView.GridControl.DataSource = dataSource;
            gridView.OptionsBehavior.Editable = false;

            gridView.OptionsMenu.EnableColumnMenu = false;
            gridView.OptionsMenu.EnableGroupPanelMenu = false;
            gridView.OptionsCustomization.AllowFilter = false;
            //gridView.OptionsCustomization.AllowSort = false;

            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridView.Columns)
            {
                column.AppearanceHeader.Options.UseTextOptions = true;
                column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                column.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold);
                column.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 10);
            }
            gridView.Appearance.HeaderPanel.Options.UseFont = true;
            gridView.Appearance.HeaderPanel.Font = new Font("Tahoma", 12, FontStyle.Bold);
            gridView.RowHeight = 28;
            gridView.OptionsView.ColumnAutoWidth = false;
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
