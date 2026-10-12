using DevExpress.XtraSplashScreen;
using QLyNSu.Functions;
using QLyNSu.FORM_SYSTEM;
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
                    ActivateForm(frm);
                    return;
                }
            }

            Form f = null;
            try
            {
                f = (Form)Activator.CreateInstance(typeForm);
                TranslationManager.Translate(f);
                f.MdiParent = _parentForm;
                f.Show();
                ActivateForm(f);
            }
            catch (Exception ex)
            {
                if (f != null)
                {
                    try { f.Dispose(); } catch { }
                }
                string msg = Bu.CLASS_SYSTEM.ErrorHelper.ResolveUserFriendlyMessage(ex, "mở chức năng");
                DevExpress.XtraEditors.XtraMessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Existing menu entry points stay compatible; opening a child no longer locks the main window.
        public async Task OpenFormWithSplashScreen(Type typeForm)
        {
            try
            {
                var openedForm = await OpenFormAsync(typeForm);
                if (!_parentForm.IsDisposed && !openedForm.IsDisposed) ActivateForm(openedForm);
            }
            catch (Exception ex)
            {
                if (!_parentForm.IsDisposed) QLyNSu.Functions.FormDataLoader.ShowLoadError(_parentForm, ex);
            }
        }

        public async Task OpenShowUserGroupFormWithSplashScreen(int mode)
        {
            await Task.Yield();
            if (_parentForm.IsDisposed) return;
            try
            {
                var existing = _parentForm.MdiChildren.OfType<FrmShowUser_Group>().FirstOrDefault(child => child.Mode == mode);
                if (existing != null) { ActivateForm(existing); return; }
                var form = new FrmShowUser_Group(mode);
                TranslationManager.Translate(form);
                form.MdiParent = _parentForm;
                form.Show();
                ActivateForm(form);
            }
            catch (Exception ex) { QLyNSu.Functions.FormDataLoader.ShowLoadError(_parentForm, ex); }
        }

        private async Task<Form> OpenFormAsync(Type typeForm)
        {
            await Task.Yield();
            if (_parentForm.IsDisposed) throw new OperationCanceledException();
            foreach (var frm in _parentForm.MdiChildren)
            {
                if (frm.GetType() == typeForm)
                {
                    return frm;
                }
            }

            Form f = null;
            try
            {
                f = (Form)Activator.CreateInstance(typeForm);
                TranslationManager.Translate(f);
                f.MdiParent = _parentForm;
                f.Show();
                return f;
            }
            catch
            {
                if (f != null)
                {
                    try { f.Dispose(); } catch { }
                }
                throw;
            }
        }

        private void ActivateForm(Form frm)
        {
            if (_parentForm is MainForm mainForm)
            {
                mainForm.ActivateMdiChild(frm);
            }
            else
            {
                frm.Activate();
            }
        }

        public void CloseAll()
        {
            if (_parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new MethodInvoker(CloseAll));
            }
            else
            {
                string message = QLyNSu.Functions.TranslationManager.Translate("Bạn có chắc chắn muốn thoát không?");
                string title = QLyNSu.Functions.TranslationManager.Translate("Xác nhận thoát");
                DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
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
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            DesktopTypography.Grid(gridView);
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
            TranslationManager.Translate(form);
            form.Show();
        }
    }
}
