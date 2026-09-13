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
        private RadioButton rdoGroup;
        private RadioButton rdoUser;
        private SimpleButton btnSua;
        private SimpleButton btnLuu;
        private SimpleButton btnHuy;
        private bool _isEditing = false;

        public FrmPhanQuyenChucNang()
        {
            InitializeComponent();
        }

        private void SetupTogglePanel()
        {
            PanelControl pnlToggle = new PanelControl();
            pnlToggle.Dock = DockStyle.Top;
            pnlToggle.Height = 45;
            pnlToggle.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlToggle.BackColor = Color.FromArgb(240, 240, 240);

            rdoGroup = new RadioButton();
            rdoGroup.Text = "Nhóm người dùng";
            rdoGroup.Location = new Point(15, 12);
            rdoGroup.AutoSize = true;
            rdoGroup.Checked = true;
            rdoGroup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rdoGroup.ForeColor = Color.FromArgb(107, 33, 168);
            rdoGroup.CheckedChanged += (s, ev) => { if (rdoGroup.Checked) loadUsers(); };

            rdoUser = new RadioButton();
            rdoUser.Text = "Người dùng";
            rdoUser.Location = new Point(170, 12);
            rdoUser.AutoSize = true;
            rdoUser.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rdoUser.ForeColor = Color.FromArgb(3, 105, 161);
            rdoUser.CheckedChanged += (s, ev) => { if (rdoUser.Checked) loadUsers(); };

            pnlToggle.Controls.Add(rdoGroup);
            pnlToggle.Controls.Add(rdoUser);

            splitContainerControl1.Panel1.Controls.Add(pnlToggle);
            pnlToggle.BringToFront();
        }

        private DevExpress.Utils.Svg.SvgImage GetSafeSvg(string path, string fallbackPath = null)
        {
            try
            {
                return DevExpress.Images.ImageResourceCache.Default.GetSvgImage(path);
            }
            catch
            {
                if (fallbackPath != null)
                {
                    try
                    {
                        return DevExpress.Images.ImageResourceCache.Default.GetSvgImage(fallbackPath);
                    }
                    catch {}
                }
            }
            return null;
        }

        private void SetupActionButtons()
        {
            btnSua = new SimpleButton();
            btnSua.Text = "Sửa quyền";
            btnSua.Size = new Size(120, 40);
            btnSua.Location = new Point(440, 10);
            btnSua.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSua.Appearance.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnSua.ImageOptions.SvgImage = GetSafeSvg("svgimages/actions/edit.svg", "svgimages/icon builder/actions_edit.svg");
            btnSua.Click += btnSua_Click;

            btnLuu = new SimpleButton();
            btnLuu.Text = "Lưu";
            btnLuu.Size = new Size(120, 40);
            btnLuu.Location = new Point(570, 10);
            btnLuu.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLuu.Appearance.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnLuu.ImageOptions.SvgImage = GetSafeSvg("svgimages/save/save.svg", "svgimages/actions/save.svg");
            btnLuu.Click += btnLuu_Click;

            btnHuy = new SimpleButton();
            btnHuy.Text = "Hủy";
            btnHuy.Size = new Size(120, 40);
            btnHuy.Location = new Point(700, 10);
            btnHuy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnHuy.Appearance.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnHuy.ImageOptions.SvgImage = GetSafeSvg("svgimages/actions/cancel.svg", "svgimages/actions/undo.svg");
            btnHuy.Click += btnHuy_Click;

            panelControl1.Controls.Add(btnSua);
            panelControl1.Controls.Add(btnLuu);
            panelControl1.Controls.Add(btnHuy);
        }

        private void SetEditingState(bool editing)
        {
            _isEditing = editing;
            if (btnSua != null) btnSua.Enabled = !editing;
            if (btnLuu != null) btnLuu.Enabled = editing;
            if (btnHuy != null) btnHuy.Enabled = editing;

            gcUser.Enabled = !editing;
            if (rdoGroup != null) rdoGroup.Enabled = !editing;
            if (rdoUser != null) rdoUser.Enabled = !editing;

            gvRight.OptionsBehavior.Editable = editing;
        }

        private void FrmPhanQuyenChucNang_Load(object sender, EventArgs e)
        {
            SetupTogglePanel();
            SetupActionButtons();

            // Set grid formatting
            FormManager_Functions.CustomView_Colums(gvUser);
            FormManager_Functions.CustomView_Colums(gvRight);

            // Configure event handlers
            gvUser.FocusedRowChanged += gvUser_FocusedRowChanged;

            SetEditingState(false);
            loadUsers();
        }

        private void loadUsers()
        {
            int targetMode = (rdoGroup != null && rdoGroup.Checked) ? 1 : 0;
            var data = db.TB_SYS_USER.Where(x => (x.ISGROUP ?? 0) == targetMode).ToList();
            gcUser.DataSource = data;
            
            // Format User columns
            if (gvUser.Columns["IDUSER"] != null) gvUser.Columns["IDUSER"].Caption = "ID";
            if (targetMode == 1)
            {
                if (gvUser.Columns["USERNAME"] != null) gvUser.Columns["USERNAME"].Caption = "Tên nhóm";
                if (gvUser.Columns["FULLNAME"] != null) gvUser.Columns["FULLNAME"].Caption = "Mô tả nhóm";
            }
            else
            {
                if (gvUser.Columns["USERNAME"] != null) gvUser.Columns["USERNAME"].Caption = "Tên tài khoản";
                if (gvUser.Columns["FULLNAME"] != null) gvUser.Columns["FULLNAME"].Caption = "Họ và tên";
            }
            
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
                .Where(r => r.IDUSER == selectedUser.IDUSER)
                .ToList();

            var rightDict = userRights.ToDictionary(r => r.FUNCTION_CODE, StringComparer.OrdinalIgnoreCase);

            // Map to list item
            _rightList = allFunctions.Select(f =>
            {
                rightDict.TryGetValue(f.FUNCTION_CODE, out var r);
                return new FunctionRightItem
                {
                    FUNCTION_CODE = f.FUNCTION_CODE,
                    DESCRIPTION = f.DESCRIPTION,
                    CAN_VIEW = r != null && ((r.CAN_VIEW ?? 0) == 1 || (r.USER_RIGHT ?? 0) == 1),
                    CAN_ADD = r != null && (r.CAN_ADD ?? 0) == 1,
                    CAN_EDIT = r != null && (r.CAN_EDIT ?? 0) == 1,
                    CAN_DELETE = r != null && (r.CAN_DELETE ?? 0) == 1,
                    CAN_PRINT = r != null && (r.CAN_PRINT ?? 0) == 1
                };
            }).ToList();

            gcRight.DataSource = new BindingList<FunctionRightItem>(_rightList);

            // Format right columns
            if (gvRight.Columns["FUNCTION_CODE"] != null)
            {
                gvRight.Columns["FUNCTION_CODE"].Caption = "Mã chức năng";
                gvRight.Columns["FUNCTION_CODE"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["FUNCTION_CODE"].Visible = false;
            }
            if (gvRight.Columns["DESCRIPTION"] != null)
            {
                gvRight.Columns["DESCRIPTION"].Caption = "Chức năng";
                gvRight.Columns["DESCRIPTION"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["DESCRIPTION"].Width = 260;
            }
            if (gvRight.Columns["CAN_VIEW"] != null)
            {
                gvRight.Columns["CAN_VIEW"].Caption = "Xem";
                gvRight.Columns["CAN_VIEW"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_VIEW"].Width = 65;
            }
            if (gvRight.Columns["CAN_ADD"] != null)
            {
                gvRight.Columns["CAN_ADD"].Caption = "Thêm";
                gvRight.Columns["CAN_ADD"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_ADD"].Width = 65;
            }
            if (gvRight.Columns["CAN_EDIT"] != null)
            {
                gvRight.Columns["CAN_EDIT"].Caption = "Sửa";
                gvRight.Columns["CAN_EDIT"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_EDIT"].Width = 65;
            }
            if (gvRight.Columns["CAN_DELETE"] != null)
            {
                gvRight.Columns["CAN_DELETE"].Caption = "Xóa";
                gvRight.Columns["CAN_DELETE"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_DELETE"].Width = 65;
            }
            if (gvRight.Columns["CAN_PRINT"] != null)
            {
                gvRight.Columns["CAN_PRINT"].Caption = "In";
                gvRight.Columns["CAN_PRINT"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_PRINT"].Width = 65;
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (gvUser.GetFocusedRow() == null)
            {
                MessageBox.Show("Vui lòng chọn người dùng hoặc nhóm để sửa quyền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetEditingState(true);
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            var selectedUser = (TB_SYS_USER)gvUser.GetFocusedRow();
            if (selectedUser == null) return;

            try
            {
                foreach (var rightItem in _rightList)
                {
                    var dbRight = db.TB_SYS_RIGHT.FirstOrDefault(r => r.IDUSER == selectedUser.IDUSER && r.FUNCTION_CODE == rightItem.FUNCTION_CODE);

                    if (dbRight == null)
                    {
                        var newRight = new TB_SYS_RIGHT
                        {
                            IDUSER = selectedUser.IDUSER,
                            FUNCTION_CODE = rightItem.FUNCTION_CODE,
                            CAN_VIEW = rightItem.CAN_VIEW ? 1 : 0,
                            CAN_ADD = rightItem.CAN_ADD ? 1 : 0,
                            CAN_EDIT = rightItem.CAN_EDIT ? 1 : 0,
                            CAN_DELETE = rightItem.CAN_DELETE ? 1 : 0,
                            CAN_PRINT = rightItem.CAN_PRINT ? 1 : 0,
                            USER_RIGHT = rightItem.CAN_VIEW ? 1 : 0
                        };
                        db.TB_SYS_RIGHT.Add(newRight);
                    }
                    else
                    {
                        dbRight.CAN_VIEW = rightItem.CAN_VIEW ? 1 : 0;
                        dbRight.CAN_ADD = rightItem.CAN_ADD ? 1 : 0;
                        dbRight.CAN_EDIT = rightItem.CAN_EDIT ? 1 : 0;
                        dbRight.CAN_DELETE = rightItem.CAN_DELETE ? 1 : 0;
                        dbRight.CAN_PRINT = rightItem.CAN_PRINT ? 1 : 0;
                        dbRight.USER_RIGHT = rightItem.CAN_VIEW ? 1 : 0;
                    }
                }

                db.SaveChanges();
                MessageBox.Show("Lưu phân quyền thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                SetEditingState(false);
                loadRights();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu phân quyền: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            SetEditingState(false);
            loadRights();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            if (_isEditing)
            {
                var choice = MessageBox.Show("Dữ liệu phân quyền đang thay đổi chưa được lưu. Bạn có chắc chắn muốn đóng và hủy thay đổi không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (choice == DialogResult.No)
                {
                    return;
                }
            }
            this.Close();
        }

        // Custom model for GridView mapping
        public class FunctionRightItem
        {
            public string FUNCTION_CODE { get; set; }
            public string DESCRIPTION { get; set; }
            public bool CAN_VIEW { get; set; }
            public bool CAN_ADD { get; set; }
            public bool CAN_EDIT { get; set; }
            public bool CAN_DELETE { get; set; }
            public bool CAN_PRINT { get; set; }
        }
    }
}
