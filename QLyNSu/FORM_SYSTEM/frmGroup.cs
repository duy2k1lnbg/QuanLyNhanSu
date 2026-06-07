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

        private DevExpress.XtraEditors.LookUpEdit cboNhom;
        private DevExpress.XtraEditors.SimpleButton btnNewGroup;
        private DevExpress.XtraEditors.SimpleButton btnXoaNhom;
        private System.Windows.Forms.Label lblChonNhom;

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

        private void SetupGroupSelection()
        {
            lblChonNhom = new Label();
            lblChonNhom.Text = "Chọn Nhóm:";
            lblChonNhom.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblChonNhom.Location = new Point(168, 53);
            lblChonNhom.AutoSize = true;

            cboNhom = new LookUpEdit();
            cboNhom.Location = new Point(283, 50);
            cboNhom.Size = new Size(344, 30);
            cboNhom.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            cboNhom.Properties.Appearance.Options.UseFont = true;
            cboNhom.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("USERNAME", "Tên nhóm"));
            cboNhom.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("FULLNAME", "Mô tả"));
            cboNhom.Properties.DisplayMember = "USERNAME";
            cboNhom.Properties.ValueMember = "IDUSER";
            cboNhom.Properties.NullText = "-- Chọn nhóm để xem/sửa --";
            cboNhom.EditValueChanged += cboNhom_EditValueChanged;

            btnNewGroup = new SimpleButton();
            btnNewGroup.Text = "Tạo nhóm mới";
            btnNewGroup.Size = new Size(150, 30);
            btnNewGroup.Location = new Point(635, 50);
            btnNewGroup.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnNewGroup.Click += btnNewGroup_Click;

            // Xóa nhóm ở bottom (cùng hàng với btnLuu và btnDong, xếp ở X=585)
            btnXoaNhom = new SimpleButton();
            btnXoaNhom.Text = "Xóa nhóm";
            btnXoaNhom.Size = new Size(120, 38);
            btnXoaNhom.Location = new Point(585, 450);
            btnXoaNhom.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnXoaNhom.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnXoaNhom.ImageOptions.SvgImage = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/actions/del.svg");
            btnXoaNhom.Click += btnXoaNhom_Click;

            tapNhom.Controls.Add(lblChonNhom);
            tapNhom.Controls.Add(cboNhom);
            tapNhom.Controls.Add(btnNewGroup);
            this.Controls.Add(btnXoaNhom);

            // Căn lề pixel-perfect cho các nhãn và ô nhập trong nhóm (khoảng cách 60px)
            label1.Location = new Point(174, 113);
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtTenNhom.Location = new Point(283, 110);
            txtTenNhom.Size = new Size(344, 30);

            label2.Location = new Point(204, 173);
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtMoTa.Location = new Point(283, 170);
            txtMoTa.Size = new Size(344, 30);

            // Cấu hình các nút chức năng ở bottom: căn phải (Right-align)
            btnDong.Location = new Point(855, 450);
            btnDong.Size = new Size(120, 38);
            btnDong.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDong.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            btnLuu.Location = new Point(720, 450);
            btnLuu.Size = new Size(120, 38);
            btnLuu.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLuu.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            // Cấu hình các nút thêm/xóa thành viên trong Tab Thành Viên: căn phải dưới lưới
            simpleButton3.Size = new Size(100, 32);
            simpleButton3.Location = new Point(758, 360);
            simpleButton3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            simpleButton3.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            simpleButton4.Size = new Size(100, 32);
            simpleButton4.Location = new Point(873, 360);
            simpleButton4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            simpleButton4.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private void loadGroups()
        {
            var groups = _sysUser.getALL().Where(x => (x.ISGROUP ?? 0) == 1).ToList();
            cboNhom.Properties.DataSource = groups;
        }

        private void cboNhom_EditValueChanged(object sender, EventArgs e)
        {
            if (cboNhom.EditValue != null && cboNhom.EditValue != DBNull.Value)
            {
                decimal groupId = Convert.ToDecimal(cboNhom.EditValue);
                _user = _sysUser.getItem(Convert.ToInt32(groupId));
                if (_user != null)
                {
                    _them = false;
                    txtTenNhom.Text = _user.USERNAME;
                    txtTenNhom.Enabled = false;
                    txtMoTa.Text = _user.FULLNAME;
                    tapThanhVien.PageEnabled = true;
                    if (btnXoaNhom != null) btnXoaNhom.Enabled = true;
                    loadMembers();
                }
            }
        }

        private void btnNewGroup_Click(object sender, EventArgs e)
        {
            _them = true;
            _user = null;
            cboNhom.EditValue = null;
            txtTenNhom.Text = string.Empty;
            txtTenNhom.Enabled = true;
            txtMoTa.Text = string.Empty;
            tapThanhVien.PageEnabled = false;
            if (btnXoaNhom != null) btnXoaNhom.Enabled = false;
            txtTenNhom.Focus();
        }

        private void btnXoaNhom_Click(object sender, EventArgs e)
        {
            if (_user == null || _them) return;

            if (_user.USERNAME.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể xóa nhóm Quản trị hệ thống (ADMIN).", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhóm [{_user.USERNAME}] không?\nHành động này cũng sẽ xóa toàn bộ phân quyền và liên kết nhóm liên quan.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _sysUser.Delete(_user.IDUSER);
                    MessageBox.Show("Xóa thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _dataSaved = true;
                    
                    btnNewGroup_Click(null, null);
                    loadGroups();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void frmGroup_Load(object sender, EventArgs e)
        {
            // Register button click handlers for members management
            simpleButton3.Click += btnThemThanhVien_Click; // Add member
            simpleButton4.Click += btnXoaThanhVien_Click; // Remove member

            SetupGroupSelection();
            loadGroups();

            if (_them)
            {
                this.Text = "Thêm nhóm mới";
                tapThanhVien.PageEnabled = false; // Cannot add members until group is created and saved
                if (btnXoaNhom != null) btnXoaNhom.Enabled = false;

                if (_user != null)
                {
                    _them = false;
                    cboNhom.EditValue = _user.IDUSER;
                    cboNhom.Enabled = false;
                    btnNewGroup.Enabled = false;
                    if (btnXoaNhom != null) btnXoaNhom.Enabled = true;
                }
            }
            else
            {
                this.Text = "Chỉnh sửa nhóm";
                cboNhom.EditValue = _user.IDUSER;
                cboNhom.Enabled = false;
                btnNewGroup.Enabled = false;
                if (btnXoaNhom != null) btnXoaNhom.Enabled = true;

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
                
                loadGroups();
                if (cboNhom.Enabled)
                {
                    cboNhom.EditValue = _user.IDUSER;
                }
                
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
                
                loadGroups();
                if (cboNhom.Enabled)
                {
                    cboNhom.EditValue = _user.IDUSER;
                }
                
                MessageBox.Show("Cập nhật thông tin nhóm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Only close form if it was opened from list (i.e. cboNhom is disabled)
                if (!cboNhom.Enabled)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
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