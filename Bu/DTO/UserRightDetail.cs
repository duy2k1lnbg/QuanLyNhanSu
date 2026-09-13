using System;

namespace Bu.DTO
{
    public enum PermissionAction
    {
        View,
        Add,
        Edit,
        Delete,
        Print
    }

    public class UserRightDetail
    {
        public string FUNCTION_CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public bool CAN_VIEW { get; set; }
        public bool CAN_ADD { get; set; }
        public bool CAN_EDIT { get; set; }
        public bool CAN_DELETE { get; set; }
        public bool CAN_PRINT { get; set; }

        public string FunctionCode { get => FUNCTION_CODE; set => FUNCTION_CODE = value; }
        public string FunctionName { get => DESCRIPTION; set => DESCRIPTION = value; }
        public bool CanView { get => CAN_VIEW; set => CAN_VIEW = value; }
        public bool CanAdd { get => CAN_ADD; set => CAN_ADD = value; }
        public bool CanEdit { get => CAN_EDIT; set => CAN_EDIT = value; }
        public bool CanDelete { get => CAN_DELETE; set => CAN_DELETE = value; }
        public bool CanPrint { get => CAN_PRINT; set => CAN_PRINT = value; }

        public bool HasAction(PermissionAction action)
        {
            switch (action)
            {
                case PermissionAction.View: return CAN_VIEW;
                case PermissionAction.Add: return CAN_ADD;
                case PermissionAction.Edit: return CAN_EDIT;
                case PermissionAction.Delete: return CAN_DELETE;
                case PermissionAction.Print: return CAN_PRINT;
                default: return false;
            }
        }
    }
}
