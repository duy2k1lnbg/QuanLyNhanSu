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

        public FrmShowUser_Group()
        {
            InitializeComponent();
            _sysUser = new SYS_USER();
        }

        private void FrmShowUser_Group_Load(object sender, EventArgs e)
        {
            // Register programmatic click event handlers for DevExpress Bar Items
            btnThem.ItemClick += btnThem_ItemClick;
            btnSua.ItemClick += btnSua_ItemClick;
            btnXoa.ItemClick += btnXoa_ItemClick;
            btnDong.ItemClick += btnDong_ItemClick;

            // Hide unused buttons from standard toolbar
            btnLuu.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            btnHuy.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            btnIn.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;

            // Support double-clicking and key shortcuts for modern navigation
            gridView1.DoubleClick += gridView1_DoubleClick;
            gridView1.KeyDown += gridView1_KeyDown;

            // Custom drawing for beautiful premium badges in cells
            gridView1.CustomDrawCell += gridView1_CustomDrawCell;

            loadData();
        }

        private void loadData()
        {
            gridControl1.DataSource = _sysUser.getALL();
            FormManager_Functions.CustomView_Colums(gridView1);
            
            // Hide password hashes and sensitive change dates
            if (gridView1.Columns["PASSWORD"] != null) gridView1.Columns["PASSWORD"].Visible = false;
            if (gridView1.Columns["LAST_PWD_CHANGED"] != null) gridView1.Columns["LAST_PWD_CHANGED"].Visible = false;

            // Give beautiful custom widths and headers
            if (gridView1.Columns["IDUSER"] != null) gridView1.Columns["IDUSER"].Caption = "Mã số";
            if (gridView1.Columns["USERNAME"] != null) gridView1.Columns["USERNAME"].Caption = "Tên đăng nhập";
            if (gridView1.Columns["FULLNAME"] != null) gridView1.Columns["FULLNAME"].Caption = "Họ và tên";
            if (gridView1.Columns["MACTY"] != null) gridView1.Columns["MACTY"].Caption = "Mã Công ty";
            if (gridView1.Columns["MADVI"] != null) gridView1.Columns["MADVI"].Caption = "Mã Đơn vị";
            if (gridView1.Columns["ISGROUP"] != null) gridView1.Columns["ISGROUP"].Caption = "Phân loại";
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
            DialogResult choice = MessageBox.Show(
                "Bạn muốn tạo thêm NGƯỜI DÙNG (Chọn YES) hay NHÓM NGƯỜI DÙNG (Chọn NO)?", 
                "Thêm mới hệ thống", 
                MessageBoxButtons.YesNoCancel, 
                MessageBoxIcon.Question
            );

            if (choice == DialogResult.Yes)
            {
                using (var frm = new FrmUser())
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        loadData();
                    }
                }
            }
            else if (choice == DialogResult.No)
            {
                using (var frm = new FrmGroup())
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

            if (selectedUser.ISGROUP == 1)
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