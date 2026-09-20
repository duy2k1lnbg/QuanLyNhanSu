using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmShowUser_Group : DevExpress.XtraEditors.XtraForm
    {
        private SYS_USER _sysUser;

        public int Mode { get; private set; }

        public FrmShowUser_Group()
        {
            InitializeComponent();
            _sysUser = new SYS_USER();
            this.Mode = 0;
        }

        public FrmShowUser_Group(int mode)
        {
            InitializeComponent();
            _sysUser = new SYS_USER();
            this.Mode = mode;
        }

        private void SetupTopPanel()
        {
            // Hide the default BarManager bar to prevent merging or hiding issues in MDI child form
            if (bar1 != null) bar1.Visible = false;

            PanelControl pnlAction = new PanelControl();
            pnlAction.Dock = DockStyle.Top;
            pnlAction.Height = 55;
            pnlAction.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlAction.BackColor = Color.FromArgb(240, 240, 240);

            SimpleButton btnAdd = new SimpleButton();
            btnAdd.Text = "Thêm mới";
            btnAdd.Size = new Size(120, 35);
            btnAdd.Location = new Point(15, 10);
            btnAdd.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnAdd.ImageOptions.SvgImage = btnThem.ImageOptions.SvgImage;
            btnAdd.Click += (s, e) => btnThem_ItemClick(null, null);

            SimpleButton btnEdit = new SimpleButton();
            btnEdit.Text = "Chỉnh sửa";
            btnEdit.Size = new Size(120, 35);
            btnEdit.Location = new Point(145, 10);
            btnEdit.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnEdit.ImageOptions.SvgImage = btnSua.ImageOptions.SvgImage;
            btnEdit.Click += (s, e) => btnSua_ItemClick(null, null);

            SimpleButton btnDelete = new SimpleButton();
            btnDelete.Text = "Xóa";
            btnDelete.Size = new Size(100, 35);
            btnDelete.Location = new Point(275, 10);
            btnDelete.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnDelete.ImageOptions.SvgImage = btnXoa.ImageOptions.SvgImage;
            btnDelete.Click += (s, e) => btnXoa_ItemClick(null, null);

            SimpleButton btnClose = new SimpleButton();
            btnClose.Text = "Đóng";
            btnClose.Size = new Size(100, 35);
            btnClose.Location = new Point(this.ClientSize.Width - 115, 10);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnClose.ImageOptions.SvgImage = btnDong.ImageOptions.SvgImage;
            btnClose.Click += (s, e) => btnDong_ItemClick(null, null);

            pnlAction.Controls.Add(btnAdd);
            pnlAction.Controls.Add(btnEdit);
            pnlAction.Controls.Add(btnDelete);
            pnlAction.Controls.Add(btnClose);

            this.Controls.Add(pnlAction);
            pnlAction.BringToFront();
        }

        private void FrmShowUser_Group_Load(object sender, EventArgs e)
        {
            this.Text = this.Mode == 1 ? "Danh Sách Nhóm Người Dùng" : "Danh Sách Tài Khoản Người Dùng";

            SetupTopPanel();

            // Support double-clicking and key shortcuts for modern navigation
            gridView1.DoubleClick += gridView1_DoubleClick;
            gridView1.KeyDown += gridView1_KeyDown;

            // Custom drawing for beautiful premium badges in cells
            gridView1.CustomDrawCell += gridView1_CustomDrawCell;

            loadData();
        }

        private void loadData()
        {
            var data = _sysUser.getALL().Where(x => (x.ISGROUP ?? 0) == this.Mode).ToList();
            gridControl1.DataSource = data;
            FormManager_Functions.CustomView_Colums(gridView1);
            
            // Hide password hashes and sensitive change dates
            if (gridView1.Columns["PASSWORD"] != null) gridView1.Columns["PASSWORD"].Visible = false;
            if (gridView1.Columns["LAST_PWD_CHANGED"] != null) gridView1.Columns["LAST_PWD_CHANGED"].Visible = false;

            // Hide ISGROUP column as it's redundant now
            if (gridView1.Columns["ISGROUP"] != null) gridView1.Columns["ISGROUP"].Visible = false;

            // Give beautiful custom widths and headers
            if (gridView1.Columns["IDUSER"] != null) gridView1.Columns["IDUSER"].Caption = "Mã số";
            if (this.Mode == 1)
            {
                if (gridView1.Columns["USERNAME"] != null) gridView1.Columns["USERNAME"].Caption = "Tên nhóm";
                if (gridView1.Columns["FULLNAME"] != null) gridView1.Columns["FULLNAME"].Caption = "Mô tả nhóm";
            }
            else
            {
                if (gridView1.Columns["USERNAME"] != null) gridView1.Columns["USERNAME"].Caption = "Tên đăng nhập";
                if (gridView1.Columns["FULLNAME"] != null) gridView1.Columns["FULLNAME"].Caption = "Họ và tên";
            }
            if (gridView1.Columns["MACTY"] != null) gridView1.Columns["MACTY"].Caption = "Mã Công ty";
            if (gridView1.Columns["MADVI"] != null) gridView1.Columns["MADVI"].Caption = "Mã Đơn vị";
            if (gridView1.Columns["DISABLED"] != null) gridView1.Columns["DISABLED"].Caption = "Trạng thái";
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName == "ISGROUP")
            {
                if (e.CellValue != null)
                {
                    decimal val = Convert.ToDecimal(e.CellValue);
                    string text = val == 1 ? "Nhóm" : "Người dùng";
                    
                    // Web HSL inspired colors
                    Color backColor = val == 1 ? Color.FromArgb(243, 232, 255) : Color.FromArgb(224, 242, 254); // Light Violet vs Light Blue
                    Color foreColor = val == 1 ? Color.FromArgb(107, 33, 168) : Color.FromArgb(3, 105, 161);   // Dark Violet vs Dark Blue
                    
                    DrawBadge(e, text, backColor, foreColor);
                    e.Handled = true;
                }
            }
            else if (e.Column.FieldName == "DISABLED")
            {
                if (e.CellValue != null)
                {
                    decimal val = Convert.ToDecimal(e.CellValue);
                    string text = val == 1 ? "Đã khóa" : "Hoạt động";
                    
                    Color backColor = val == 1 ? Color.FromArgb(254, 226, 226) : Color.FromArgb(220, 252, 231); // Light Red vs Light Green
                    Color foreColor = val == 1 ? Color.FromArgb(185, 28, 28) : Color.FromArgb(21, 128, 61);     // Dark Red vs Dark Green
                    
                    DrawBadge(e, text, backColor, foreColor);
                    e.Handled = true;
                }
            }
        }

        private void DrawBadge(DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e, string text, Color backColor, Color foreColor)
        {
            // Draw default cell background first to clear selection artifact
            e.Cache.FillRectangle(e.Cache.GetSolidBrush(e.Appearance.BackColor), e.Bounds);

            // Compute ideal badge size
            Size textSize = TextRenderer.MeasureText(text, e.Appearance.Font);
            int rectWidth = Math.Max(85, textSize.Width + 16);
            int rectHeight = textSize.Height + 6;

            // Center inside current cell bounds
            int rectX = e.Bounds.X + (e.Bounds.Width - rectWidth) / 2;
            int rectY = e.Bounds.Y + (e.Bounds.Height - rectHeight) / 2;

            Rectangle rect = new Rectangle(rectX, rectY, rectWidth, rectHeight);

            // Draw soft round rectangle path
            using (GraphicsPath path = GetRoundedRectPath(rect, 8))
            {
                e.Cache.FillPath(e.Cache.GetSolidBrush(backColor), path);
            }

            // Draw text centered inside the badge
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(e.Cache.Graphics, text, e.Appearance.Font, rect, foreColor, flags);
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.Mode == 1)
            {
                using (var frm = new FrmGroup())
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        loadData();
                    }
                }
            }
            else
            {
                using (var frm = new FrmUser())
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        loadData();
                    }
                }
            }
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            editSelectedItem();
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selectedUser = (TB_SYS_USER)gridView1.GetFocusedRow();
            if (selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản hoặc nhóm muốn xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedUser.USERNAME.Equals(UserSession.CurrentUser?.USERNAME, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không thể tự xóa tài khoản của chính mình.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (selectedUser.USERNAME.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể xóa tài khoản Quản trị hệ thống (ADMIN).", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa {(selectedUser.ISGROUP == 1 ? "nhóm" : "người dùng")} [{selectedUser.USERNAME}] không?\nHành động này cũng sẽ xóa toàn bộ phân quyền và liên kết nhóm liên quan.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _sysUser.Delete(selectedUser.IDUSER);
                    MessageBox.Show("Xóa thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Log audit action for delete
                    string type = selectedUser.ISGROUP == 1 ? "GROUP" : "USER";
                    string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
                    try
                    {
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - AUDIT: USER [{UserSession.CurrentUser?.USERNAME}] DELETED {type} [{selectedUser.USERNAME}]\r\n");
                    }
                    catch { }

                    loadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            editSelectedItem();
        }

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                editSelectedItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                btnXoa_ItemClick(null, null);
                e.Handled = true;
            }
        }

        private void editSelectedItem()
        {
            var selectedUser = (TB_SYS_USER)gridView1.GetFocusedRow();
            if (selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản hoặc nhóm muốn chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (this.Mode == 1)
            {
                using (var frm = new FrmGroup(selectedUser))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        loadData();
                    }
                }
            }
            else
            {
                using (var frm = new FrmUser(selectedUser))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        loadData();
                    }
                }
            }
        }
    }
}