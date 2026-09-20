using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using System.Windows.Forms;

namespace QLyNSu.Functions
{
    public static class FormSecurity
    {
        /// <summary>
        /// Áp dụng quyền 5 hành vi cho các nút DevExpress BarButtonItem (Thêm, Sửa, Xóa, In).
        /// </summary>
        public static void ApplyButtons(
            string functionCode,
            BarButtonItem btnThem,
            BarButtonItem btnSua,
            BarButtonItem btnXoa,
            BarButtonItem btnIn,
            bool isBrowsingState)
        {
            if (btnThem != null)
                btnThem.Enabled = isBrowsingState && UserSession.CanAdd(functionCode);

            if (btnSua != null)
                btnSua.Enabled = isBrowsingState && UserSession.CanEdit(functionCode);

            if (btnXoa != null)
                btnXoa.Enabled = isBrowsingState && UserSession.CanDelete(functionCode);

            if (btnIn != null)
                btnIn.Enabled = isBrowsingState && UserSession.CanPrint(functionCode);
        }

        /// <summary>
        /// Áp dụng quyền cho DevExpress SimpleButton (Thêm, Sửa, Xóa, In).
        /// </summary>
        public static void ApplyButtons(
            string functionCode,
            SimpleButton btnThem,
            SimpleButton btnSua,
            SimpleButton btnXoa,
            SimpleButton btnIn,
            bool isBrowsingState)
        {
            if (btnThem != null)
                btnThem.Enabled = isBrowsingState && UserSession.CanAdd(functionCode);

            if (btnSua != null)
                btnSua.Enabled = isBrowsingState && UserSession.CanEdit(functionCode);

            if (btnXoa != null)
                btnXoa.Enabled = isBrowsingState && UserSession.CanDelete(functionCode);

            if (btnIn != null)
                btnIn.Enabled = isBrowsingState && UserSession.CanPrint(functionCode);
        }

        /// <summary>
        /// Kiểm tra quyền trước khi thực thi logic nghiệp vụ.
        /// Trả về true nếu có quyền, ngược lại hiển thị thông báo cảnh báo và trả về false.
        /// </summary>
        public static bool AssertPermission(string functionCode, PermissionAction action, bool showMessage = true)
        {
            bool allowed = UserSession.CheckPermission(functionCode, action);

            if (!allowed && showMessage)
            {
                string actionName = GetActionDisplayName(action);
                string translatedMsg = TranslationManager.Translate($"Bạn không có quyền [{actionName}] cho chức năng này.");
                string translatedTitle = TranslationManager.Translate("Cảnh Báo Phân Quyền");
                MessageBox.Show(translatedMsg, translatedTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return allowed;
        }

        /// <summary>
        /// Kiểm tra quyền truy cập Xem (VIEW) khi Form được tải.
        /// Nếu không có quyền sẽ hiển thị thông báo và đóng Form.
        /// </summary>
        public static bool CheckViewPermission(Form form, string functionCode)
        {
            if (!UserSession.CanView(functionCode))
            {
                string msg = TranslationManager.Translate("Bạn không có quyền truy cập chức năng này.");
                string title = TranslationManager.Translate("Cảnh Báo Phân Quyền");
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                form.BeginInvoke(new MethodInvoker(form.Close));
                return false;
            }
            return true;
        }

        private static string GetActionDisplayName(PermissionAction action)
        {
            switch (action)
            {
                case PermissionAction.View: return "Xem";
                case PermissionAction.Add: return "Thêm mới";
                case PermissionAction.Edit: return "Chỉnh sửa";
                case PermissionAction.Delete: return "Xóa";
                case PermissionAction.Print: return "In ấn";
                default: return "Thực hiện";
            }
        }
    }
}
