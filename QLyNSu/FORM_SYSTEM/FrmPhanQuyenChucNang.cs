using Bu.CLASS_SYSTEM;
using DA;
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

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmPhanQuyenChucNang : DevExpress.XtraEditors.XtraForm
    {
        private MyEntities db = new MyEntities();
        private List<FunctionRightItem> _rightList = new List<FunctionRightItem>();

        public FrmPhanQuyenChucNang()
        {
            InitializeComponent();
        }

        private void FrmPhanQuyenChucNang_Load(object sender, EventArgs e)
        {
            // Set grid formatting
            FormManager_Functions.CustomView_Colums(gvUser);
            FormManager_Functions.CustomView_Colums(gvRight);

            // Configure event handlers
            gvUser.FocusedRowChanged += gvUser_FocusedRowChanged;
            gvRight.CellValueChanged += gvRight_CellValueChanged;

            loadUsers();
        }

        private void loadUsers()
        {
            gcUser.DataSource = db.TB_SYS_USER.ToList();
            
            // Format User columns
            if (gvUser.Columns["IDUSER"] != null) gvUser.Columns["IDUSER"].Caption = "ID";
            if (gvUser.Columns["USERNAME"] != null) gvUser.Columns["USERNAME"].Caption = "Tài khoản/Nhóm";
            if (gvUser.Columns["FULLNAME"] != null) gvUser.Columns["FULLNAME"].Caption = "Họ và tên / Mô tả";
            
            // Hide other columns
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gvUser.Columns)
            {
                if (col.FieldName != "IDUSER" && col.FieldName != "USERNAME" && col.FieldName != "FULLNAME")
                    col.Visible = false;
            }
        }

        private void gvUser_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            loadRights();
        }

        private void loadRights()
        {
            var selectedUser = (TB_SYS_USER)gvUser.GetFocusedRow();
            if (selectedUser == null)
            {
                gcRight.DataSource = null;
                return;
            }

            // Load all functions
            var allFunctions = db.TB_SYS_FUNCTION.OrderBy(f => f.SORT).ToList();

            // Load current user rights
            var userRights = db.TB_SYS_RIGHT
                .Where(r => r.IDUSER == selectedUser.IDUSER && r.USER_RIGHT == 1)
                .Select(r => r.FUNCTION_CODE)
                .ToList();

            // Map to list item
            _rightList = allFunctions.Select(f => new FunctionRightItem
            {
                FUNCTION_CODE = f.FUNCTION_CODE,
                DESCRIPTION = f.DESCRIPTION,
                HAS_RIGHT = userRights.Contains(f.FUNCTION_CODE)
            }).ToList();

            gcRight.DataSource = new BindingList<FunctionRightItem>(_rightList);

            // Format right columns
            if (gvRight.Columns["FUNCTION_CODE"] != null)
            {
                gvRight.Columns["FUNCTION_CODE"].Caption = "Mã chức năng";
                gvRight.Columns["FUNCTION_CODE"].OptionsColumn.AllowEdit = false;
            }
            if (gvRight.Columns["DESCRIPTION"] != null)
            {
                gvRight.Columns["DESCRIPTION"].Caption = "Tên chức năng";
                gvRight.Columns["DESCRIPTION"].OptionsColumn.AllowEdit = false;
            }
            if (gvRight.Columns["HAS_RIGHT"] != null)
            {
                gvRight.Columns["HAS_RIGHT"].Caption = "Cho phép truy cập";
                gvRight.Columns["HAS_RIGHT"].OptionsColumn.AllowEdit = true; // Editable checkbox column!
            }
        }

        private void gvRight_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "HAS_RIGHT")
            {
                var selectedUser = (TB_SYS_USER)gvUser.GetFocusedRow();
                if (selectedUser == null) return;

                var rightItem = (FunctionRightItem)gvRight.GetRow(e.RowHandle);
                if (rightItem == null) return;

                // Find existing right in DB
                var dbRight = db.TB_SYS_RIGHT.FirstOrDefault(r => r.IDUSER == selectedUser.IDUSER && r.FUNCTION_CODE == rightItem.FUNCTION_CODE);

                if (rightItem.HAS_RIGHT)
                {
                    if (dbRight == null)
                    {
                        var newRight = new TB_SYS_RIGHT
                        {
                            IDUSER = selectedUser.IDUSER,
                            FUNCTION_CODE = rightItem.FUNCTION_CODE,
                            USER_RIGHT = 1
                        };
                        db.TB_SYS_RIGHT.Add(newRight);
                    }
                    else
                    {
                        dbRight.USER_RIGHT = 1;
                    }
                }
                else
                {
                    if (dbRight != null)
                    {
                        dbRight.USER_RIGHT = 0; // or db.TB_SYS_RIGHT.Remove(dbRight);
                    }
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu phân quyền: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Custom model for GridView mapping
        public class FunctionRightItem
        {
            public string FUNCTION_CODE { get; set; }
            public string DESCRIPTION { get; set; }
            public bool HAS_RIGHT { get; set; }
        }
    }
}
