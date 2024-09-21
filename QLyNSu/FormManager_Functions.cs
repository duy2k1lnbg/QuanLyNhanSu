using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
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
            // Hiển thị SplashScreen khi đang mở form
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
                // Đóng SplashScreen sau khi form đã mở xong
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

            // Khởi tạo form bất đồng bộ
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
                // Hiển thị hộp thoại xác nhận trước khi thoát
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Kết thúc toàn bộ ứng dụng
                    Application.Exit();
                }
            }
        }
    }
}
