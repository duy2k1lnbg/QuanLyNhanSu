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
    public partial class FrmPhanQuyenBaoCao : DevExpress.XtraEditors.XtraForm
    {
        private MyEntities db = new MyEntities();
        private List<ReportRightItem> _rightList = new List<ReportRightItem>();

        public FrmPhanQuyenBaoCao()
        {
            InitializeComponent();
        }

        private void FrmPhanQuyenBaoCao_Load(object sender, EventArgs e)
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

            // Load all reports
            var allReports = db.TB_SYS_REPORT.OrderBy(r => r.REP_CODE).ToList();

            // Load current user report rights
            var userReportRights = db.TB_SYS_RIGHT_REPORT
                .Where(r => r.IDUSER == selectedUser.IDUSER && r.USER_RIGHT == 1)
                .Select(r => r.REP_CODE)
                .ToList();

            // Map to list item
            _rightList = allReports.Select(r => new ReportRightItem
            {
                REP_CODE = r.REP_CODE,
                DESCRIPTION = r.DESCRIPTION,
                HAS_RIGHT = userReportRights.Contains(r.REP_CODE)
            }).ToList();

            gcRight.DataSource = new BindingList<ReportRightItem>(_rightList);

            // Format right columns
            if (gvRight.Columns["REP_CODE"] != null)
            {
                gvRight.Columns["REP_CODE"].Caption = "Mã báo cáo";
                gvRight.Columns["REP_CODE"].OptionsColumn.AllowEdit = false;
            }
            if (gvRight.Columns["DESCRIPTION"] != null)
            {
                gvRight.Columns["DESCRIPTION"].Caption = "Tên báo cáo";
                gvRight.Columns["DESCRIPTION"].OptionsColumn.AllowEdit = false;
            }
            if (gvRight.Columns["HAS_RIGHT"] != null)
            {
                gvRight.Columns["HAS_RIGHT"].Caption = "Cho phép xem";
                gvRight.Columns["HAS_RIGHT"].OptionsColumn.AllowEdit = true;
            }
        }

        private void gvRight_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "HAS_RIGHT")
            {
                var selectedUser = (TB_SYS_USER)gvUser.GetFocusedRow();
                if (selectedUser == null) return;

                var rightItem = (ReportRightItem)gvRight.GetRow(e.RowHandle);
                if (rightItem == null) return;

                // Find existing report right in DB
                var dbRight = db.TB_SYS_RIGHT_REPORT.FirstOrDefault(r => r.IDUSER == selectedUser.IDUSER && r.REP_CODE == rightItem.REP_CODE);

                if (rightItem.HAS_RIGHT)
                {
                    if (dbRight == null)
                    {
                        var newRight = new TB_SYS_RIGHT_REPORT
                        {
                            IDUSER = selectedUser.IDUSER,
                            REP_CODE = rightItem.REP_CODE,
                            USER_RIGHT = 1
                        };
                        db.TB_SYS_RIGHT_REPORT.Add(newRight);
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
                        dbRight.USER_RIGHT = 0;
                    }
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu phân quyền báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Custom model for GridView mapping
        public class ReportRightItem
        {
            public decimal REP_CODE { get; set; }
            public string DESCRIPTION { get; set; }
            public bool HAS_RIGHT { get; set; }
        }
    }
}
