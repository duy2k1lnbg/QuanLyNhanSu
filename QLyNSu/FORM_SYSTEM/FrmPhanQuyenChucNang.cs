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
                            dbRight.USER_RIGHT = 0;
                        }
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
            public bool HAS_RIGHT { get; set; }
        }
    }
}
