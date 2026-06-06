using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
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
    public partial class FrmGroup : DevExpress.XtraEditors.XtraForm
    {
        private SYS_USER _sysUser;
        private TB_SYS_USER _user;
        private bool _them;
        private bool _dataSaved; // Track if database was modified to notify parent on Close

        public FrmGroup()
        {
            InitializeComponent();
            _sysUser = new SYS_USER();
            _them = true;
            _dataSaved = false;
        }

        public FrmGroup(TB_SYS_USER groupUser) : this()
        {
            _user = groupUser;
            _them = false;
        }

        private void frmGroup_Load(object sender, EventArgs e)
        {
            // Register button click handlers for members management
            simpleButton3.Click += btnThemThanhVien_Click; // Add member
            simpleButton4.Click += btnXoaThanhVien_Click; // Remove member

            if (_them)
            {
                this.Text = "Thêm nhóm mới";
                tapThanhVien.PageEnabled = false; // Cannot add members until group is created and saved
            }
            else
            {
                this.Text = "Chỉnh sửa nhóm";
                txtTenNhom.Text = _user.USERNAME;
                txtTenNhom.Enabled = false; // Group name is permanent
                txtMoTa.Text = _user.FULLNAME;
                tapThanhVien.PageEnabled = true;
                loadMembers();
            }
        }

        private void loadMembers()
        {
            if (_user != null)
            {
                gcThanhVien.DataSource = _sysUser.GetGroupMembers(_user.IDUSER);
                FormManager_Functions.CustomView_Colums(gvThanhVien);
                
                // Hide system audit/config columns in members grid
                if (gvThanhVien.Columns["PASSWORD"] != null) gvThanhVien.Columns["PASSWORD"].Visible = false;
                if (gvThanhVien.Columns["LAST_PWD_CHANGED"] != null) gvThanhVien.Columns["LAST_PWD_CHANGED"].Visible = false;
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.DialogResult = _dataSaved ? DialogResult.OK : DialogResult.Cancel;
            this.Close();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string groupName = txtTenNhom.Text.Trim();
            string description = txtMoTa.Text.Trim();

            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Chưa nhập tên nhóm. Vui lòng viết liền không dấu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenNhom.SelectAll();
                txtTenNhom.Focus();
                return;
            }

            if (_them)
            {
                bool checkedUser = _sysUser.checkUserExist(groupName);
                if (checkedUser)
                {
                    MessageBox.Show("Nhóm đã tồn tại. Vui lòng kiểm tra lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTenNhom.SelectAll();
                    txtTenNhom.Focus();
                    return;
                }

                _user = new TB_SYS_USER
                {
                    USERNAME = groupName,
                    FULLNAME = description,
                    PASSWORD = PasswordHasher.HashPassword("123"), // Default dummy password for groups
                    ISGROUP = 1,
                    DISABLED = 0,
                    MACTY = "1",
                    MADVI = "1"
                };

                var savedGroup = _sysUser.Add(_user);
                _user = savedGroup;
                _them = false;
                _dataSaved = true;
                
                // Enable members tab for adding users now that ID is generated
                tapThanhVien.PageEnabled = true;
                pageGroup.SelectedTabPage = tapThanhVien;
                loadMembers();
                
                MessageBox.Show("Tạo nhóm thành công. Hãy thêm thành viên vào nhóm ở tab Thành Viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _user.FULLNAME = description;
                _sysUser.Update(_user);
                _dataSaved = true;
                
                MessageBox.Show("Cập nhật thông tin nhóm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnThemThanhVien_Click(object sender, EventArgs e)
        {
            if (_user == null) return;

            var nonMembers = _sysUser.GetUsersNotInGroup(_user.IDUSER);
            if (nonMembers.Count == 0)
            {
                MessageBox.Show("Tất cả người dùng đều đã là thành viên của nhóm này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var selector = new FrmSelectUsers(nonMembers))
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    foreach (var user in selector.SelectedUsers)
                    {
                        _sysUser.AddUserToGroup(_user.IDUSER, user.IDUSER);
                    }
                    _dataSaved = true;
                    loadMembers();
                }
            }
        }

        private void btnXoaThanhVien_Click(object sender, EventArgs e)
        {
            if (_user == null) return;

            var selectedMember = (TB_SYS_USER)gvThanhVien.GetFocusedRow();
            if (selectedMember == null)
            {
                MessageBox.Show("Vui lòng chọn thành viên muốn loại bỏ khỏi nhóm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn loại bỏ [{selectedMember.USERNAME}] khỏi nhóm này?",
                "Xác nhận loại bỏ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _sysUser.RemoveUserFromGroup(_user.IDUSER, selectedMember.IDUSER);
                    _dataSaved = true;
                    loadMembers();
                    MessageBox.Show("Đã loại bỏ thành viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Lightweight User Multi-Selection Dialog Class
    public class FrmSelectUsers : DevExpress.XtraEditors.XtraForm
    {
        private GridControl gc;
        private GridView gv;
        private SimpleButton btnSelect;
        private SimpleButton btnCancel;
        public List<TB_SYS_USER> SelectedUsers { get; private set; } = new List<TB_SYS_USER>();

        public FrmSelectUsers(List<TB_SYS_USER> users)
        {
            this.Text = "Chọn thành viên thêm vào nhóm";
            this.Size = new Size(520, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Apply Segoe UI font to this popup form
            this.Appearance.Font = new Font("Segoe UI", 9.5F);

            gc = new GridControl { Dock = DockStyle.Top, Height = 300 };
            gv = new GridView();
            gc.MainView = gv;
            gc.ViewCollection.Add(gv);

            btnSelect = new SimpleButton { Text = "Chọn", Location = new Point(130, 320), Size = new Size(110, 40) };
            btnCancel = new SimpleButton { Text = "Hủy", Location = new Point(270, 320), Size = new Size(110, 40) };

            btnSelect.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            this.Controls.Add(gc);
            this.Controls.Add(btnSelect);
            this.Controls.Add(btnCancel);

            gv.OptionsBehavior.Editable = false;
            gv.OptionsSelection.MultiSelect = true;
            gv.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

            gc.DataSource = users;

            // Format columns
            gv.Columns["IDUSER"].Caption = "ID";
            gv.Columns["USERNAME"].Caption = "Tên tài khoản";
            gv.Columns["FULLNAME"].Caption = "Họ và tên";

            // Hide audit/mapping fields
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
            {
                if (col.FieldName != "IDUSER" && col.FieldName != "USERNAME" && col.FieldName != "FULLNAME")
                    col.Visible = false;
            }

            // Apply modern grid padding and styling
            FormManager_Functions.CustomView_Colums(gv);

            btnCancel.Click += (s, e) => this.Close();
            btnSelect.Click += (s, e) => {
                var selectedRowHandles = gv.GetSelectedRows();
                foreach (var rowHandle in selectedRowHandles)
                {
                    if (rowHandle >= 0)
                    {
                        var user = (TB_SYS_USER)gv.GetRow(rowHandle);
                        SelectedUsers.Add(user);
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }
    }
}