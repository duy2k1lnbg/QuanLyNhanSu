using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmPhanQuyenChucNang : DevExpress.XtraEditors.XtraForm
    {
        private MyEntities db = new MyEntities();
        private readonly IChannelPermissionResolver _channelResolver = new ChannelPermissionResolver();
        private List<ChannelFunctionRightViewModel> _rightList = new List<ChannelFunctionRightViewModel>();

        // Controls lựa chọn đối tượng
        private RadioButton rdoAllSubjects;
        private RadioButton rdoGroup;
        private RadioButton rdoUser;

        // Controls lựa chọn kênh & quyền cha
        private PanelControl pnlChannelHeader;
        private RadioButton rdoDesktopChannel;
        private RadioButton rdoWebChannel;
        private RadioButton rdoMobileChannel;
        private RadioButton rdoAiPermissions;
        private FrmAiScopeGrantDetail _aiPermissionsForm;
        private bool _switchingPermissionPage;
        private bool IsAiPage => rdoAiPermissions != null && rdoAiPermissions.Checked;
        private CheckEdit chkParentAccess;
        private LabelControl lblParentInherited;
        private LabelControl lblReadinessStatus;

        // Action buttons
        private SimpleButton btnLamMoi;
        private SimpleButton btnChonTatCa;
        private SimpleButton btnBoChonTatCa;
        private SimpleButton btnSua;
        private SimpleButton btnLuu;
        private SimpleButton btnHuy;

        private bool _loadingSubjects;
        private int _rightsLoadVersion;
        private bool _isEditing = false;
        private bool _isSaving = false;

        private string CurrentChannelCode
        {
            get
            {
                if (rdoWebChannel != null && rdoWebChannel.Checked) return AppChannels.Web;
                if (rdoMobileChannel != null && rdoMobileChannel.Checked) return AppChannels.Mobile;
                return AppChannels.Desktop;
            }
        }

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
            rdoGroup.Text = QLyNSu.Functions.TranslationManager.Translate("Nhóm người dùng");
            rdoGroup.Location = new Point(15, 12);
            rdoGroup.AutoSize = true;
            rdoGroup.Checked = false;
            rdoGroup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rdoGroup.ForeColor = Color.FromArgb(107, 33, 168);
            rdoGroup.CheckedChanged += async (s, ev) => { if (rdoGroup.Checked) await loadUsers(); };

            rdoUser = new RadioButton();
            rdoUser.Text = QLyNSu.Functions.TranslationManager.Translate("Người dùng");
            rdoUser.Location = new Point(170, 12);
            rdoUser.AutoSize = true;
            rdoUser.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rdoUser.ForeColor = Color.FromArgb(3, 105, 161);
            rdoUser.CheckedChanged += async (s, ev) => { if (rdoUser.Checked) await loadUsers(); };

            var subjects = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(8) };
            rdoGroup.Margin=new Padding(4);rdoUser.Margin=new Padding(4);
            rdoAllSubjects = new RadioButton { Text = QLyNSu.Functions.TranslationManager.Translate("Tất cả"), AutoSize = true, Checked = true, Font = rdoGroup.Font, Margin = new Padding(4) };
            rdoAllSubjects.CheckedChanged += async (s, ev) => { if (rdoAllSubjects.Checked) await loadUsers(); };
            subjects.Controls.Add(rdoAllSubjects);subjects.Controls.Add(rdoGroup);subjects.Controls.Add(rdoUser);pnlToggle.Controls.Add(subjects);
            subjects.SizeChanged += (s, e) => pnlToggle.Height = Math.Max(45, subjects.Height);

            splitContainerControl1.Panel1.Controls.Add(pnlToggle);
            gcUser.BringToFront();
        }

        private void SetupChannelHeaderPanel()
        {
            pnlChannelHeader = new PanelControl();
            pnlChannelHeader.Dock = DockStyle.Top;
            pnlChannelHeader.Height = 72;
            pnlChannelHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlChannelHeader.BackColor = Color.FromArgb(248, 250, 252);

            // 1. Nhóm chọn kênh nền tảng
            LabelControl lblChannel = new LabelControl();
            lblChannel.Text = QLyNSu.Functions.TranslationManager.Translate("Nền tảng:");
            lblChannel.Location = new Point(12, 10);
            lblChannel.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblChannel.Appearance.ForeColor = Color.FromArgb(15, 23, 42);

            rdoDesktopChannel = new RadioButton();
            rdoDesktopChannel.Text = "DESKTOP";
            rdoDesktopChannel.Location = new Point(85, 8);
            rdoDesktopChannel.AutoSize = true;
            rdoDesktopChannel.Checked = true;
            rdoDesktopChannel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdoDesktopChannel.ForeColor = Color.FromArgb(30, 64, 175);
            rdoDesktopChannel.CheckedChanged += (s, ev) => { if (rdoDesktopChannel.Checked) OnChannelChanged(); };

            rdoWebChannel = new RadioButton();
            rdoWebChannel.Text = "WEB";
            rdoWebChannel.Location = new Point(185, 8);
            rdoWebChannel.AutoSize = true;
            rdoWebChannel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdoWebChannel.ForeColor = Color.FromArgb(5, 150, 105);
            rdoWebChannel.CheckedChanged += (s, ev) => { if (rdoWebChannel.Checked) OnChannelChanged(); };

            rdoMobileChannel = new RadioButton();
            rdoMobileChannel.Text = "MOBILE";
            rdoMobileChannel.Location = new Point(260, 8);
            rdoMobileChannel.AutoSize = true;
            rdoMobileChannel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdoMobileChannel.ForeColor = Color.FromArgb(217, 119, 6);
            rdoMobileChannel.CheckedChanged += (s, ev) => { if (rdoMobileChannel.Checked) OnChannelChanged(); };

            // 2. Quyền cha của kênh (Platform Access Grant)
            chkParentAccess = new CheckEdit();
            chkParentAccess.Text = QLyNSu.Functions.TranslationManager.Translate("Cấp quyền truy cập nền tảng này (Quyền cha)");
            chkParentAccess.Location = new Point(12, 38);
            chkParentAccess.Properties.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            chkParentAccess.Properties.Appearance.ForeColor = Color.FromArgb(2, 132, 199);
            chkParentAccess.CheckedChanged += (s, ev) => OnParentAccessCheckChanged();

            lblParentInherited = new LabelControl();
            lblParentInherited.Text = "";
            lblParentInherited.Location = new Point(360, 42);
            lblParentInherited.Appearance.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblParentInherited.Appearance.ForeColor = Color.FromArgb(100, 116, 139);

            lblReadinessStatus = new LabelControl();
            lblReadinessStatus.Text = "";
            lblReadinessStatus.Location = new Point(360, 10);
            lblReadinessStatus.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            pnlChannelHeader.Controls.Add(lblChannel);
            pnlChannelHeader.Controls.Add(rdoDesktopChannel);
            pnlChannelHeader.Controls.Add(rdoWebChannel);
            pnlChannelHeader.Controls.Add(rdoMobileChannel);
            pnlChannelHeader.Controls.Add(chkParentAccess);
            pnlChannelHeader.Controls.Add(lblParentInherited);
            pnlChannelHeader.Controls.Add(lblReadinessStatus);

            rdoAiPermissions=new RadioButton {Text="AI",AutoSize=true,Font=rdoDesktopChannel.Font,ForeColor=Color.FromArgb(107,33,168)};
            rdoAiPermissions.CheckedChanged+=(s,e)=>{if(rdoAiPermissions.Checked) OnChannelChanged();};
            var channels=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Padding=new Padding(8),MinimumSize=new Size(0,36)};
            var parent=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Padding=new Padding(8),AutoScroll=true};
            pnlChannelHeader.Controls.Clear();pnlChannelHeader.Height=110;
            foreach(var control in new Control[]{lblChannel,rdoDesktopChannel,rdoWebChannel,rdoMobileChannel,rdoAiPermissions,lblReadinessStatus}) {control.Anchor=AnchorStyles.None;control.Margin=new Padding(6);channels.Controls.Add(control);}
            foreach(var control in new Control[]{chkParentAccess,lblParentInherited}) {control.Anchor=AnchorStyles.None;control.Margin=new Padding(6);parent.Controls.Add(control);}
            pnlChannelHeader.Controls.Add(parent);pnlChannelHeader.Controls.Add(channels);
            EventHandler reflow=(s,e)=>pnlChannelHeader.Height=Math.Max(56,channels.Height+parent.Height+4);
            channels.SizeChanged+=reflow;parent.SizeChanged+=reflow;

            splitContainerControl1.Panel2.Controls.Add(pnlChannelHeader);
            gcRight.BringToFront();
        }

        private async void OnChannelChanged()
        {
            if(_switchingPermissionPage) return;
            if(_aiPermissionsForm!=null && !_aiPermissionsForm.TryDiscardChanges())
            { _switchingPermissionPage=true; rdoAiPermissions.Checked=true; _switchingPermissionPage=false;return; }

            if (_isEditing)
            {
                var choice = XtraMessageBox.Show(
                    QLyNSu.Functions.TranslationManager.Translate("Dữ liệu đang sửa đổi chưa được lưu. Bạn có muốn đổi kênh và hủy các thay đổi hiện tại không?"),
                    QLyNSu.Functions.TranslationManager.Translate("Xác nhận"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (choice == DialogResult.No)
                {
                    return;
                }
                SetEditingState(false);
            }

            await loadRights();
        }

        private void OnParentAccessCheckChanged()
        {
            if (!_isEditing) return;

            bool isParentChecked = chkParentAccess.Checked;
            // Nếu quyền cha bị tắt: Khóa toàn bộ quyền con
            foreach (var item in _rightList)
            {
                item.IsDisabledByParent = !isParentChecked;
            }
            gvRight.RefreshData();
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
                    catch { }
                }
            }
            return null;
        }

        private void SetupActionButtons()
        {
            btnLamMoi = new SimpleButton();
            btnLamMoi.Text = QLyNSu.Functions.TranslationManager.Translate("Làm mới");
            btnLamMoi.Size = new Size(105, 40);
            btnLamMoi.Location = new Point(110, 10);
            btnLamMoi.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLamMoi.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            btnLamMoi.ImageOptions.SvgImage = GetSafeSvg("svgimages/dashboards/resetview.svg", "svgimages/icon builder/actions_refresh.svg");
            btnLamMoi.Click += async (s, ev) =>
            {
                try
                {
                    if(_aiPermissionsForm!=null && !_aiPermissionsForm.TryDiscardChanges()) return;
                    await QLyNSu.Functions.FormDataLoader.RunAsync(this, () => new Bu.CLASS_SYSTEM.SYS_USER().EnsureSeeded());
                    db = new MyEntities();
                    await loadUsers();
                    XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Đã làm mới dữ liệu chức năng và phân quyền thành công!"), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Lỗi khi làm mới:") + " " + ex.Message, QLyNSu.Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnChonTatCa = new SimpleButton();
            btnChonTatCa.Text = QLyNSu.Functions.TranslationManager.Translate("Chọn tất cả");
            btnChonTatCa.Size = new Size(115, 40);
            btnChonTatCa.Location = new Point(225, 10);
            btnChonTatCa.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnChonTatCa.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            btnChonTatCa.ImageOptions.SvgImage = GetSafeSvg("svgimages/spreadsheet/selectall.svg", "svgimages/actions/apply.svg");
            btnChonTatCa.Click += (s, ev) =>
            {
                if (!_isEditing || _rightList == null) return;
                if (!chkParentAccess.Checked)
                {
                    XtraMessageBox.Show(
                        QLyNSu.Functions.TranslationManager.Translate("Quyền truy cập nền tảng đang tắt. Vui lòng bật quyền cha trước khi chọn quyền con."),
                        QLyNSu.Functions.TranslationManager.Translate("Thông báo"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Chọn tất cả: CHỈ BẬT CÁC QUYỀN CON ĐƯỢC KÊNH HỖ TRỢ, TUYỆT ĐỐI KHÔNG TỰ BẬT QUYỀN CHA HOẶC KÊNH KHÁC
                foreach (var item in _rightList)
                {
                    if (item.SupportedCanView) item.CAN_VIEW = true;
                    if (item.SupportedCanAdd) item.CAN_ADD = true;
                    if (item.SupportedCanEdit) item.CAN_EDIT = true;
                    if (item.SupportedCanDelete) item.CAN_DELETE = true;
                    if (item.SupportedCanPrint) item.CAN_PRINT = true;
                }
                gcRight.RefreshDataSource();
            };

            btnBoChonTatCa = new SimpleButton();
            btnBoChonTatCa.Text = QLyNSu.Functions.TranslationManager.Translate("Bỏ tất cả");
            btnBoChonTatCa.Size = new Size(105, 40);
            btnBoChonTatCa.Location = new Point(350, 10);
            btnBoChonTatCa.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBoChonTatCa.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            btnBoChonTatCa.ImageOptions.SvgImage = GetSafeSvg("svgimages/actions/delete.svg", "svgimages/dashboards/cleargridfiltering.svg");
            btnBoChonTatCa.Click += (s, ev) =>
            {
                if (!_isEditing || _rightList == null) return;
                foreach (var item in _rightList)
                {
                    item.CAN_VIEW = false;
                    item.CAN_ADD = false;
                    item.CAN_EDIT = false;
                    item.CAN_DELETE = false;
                    item.CAN_PRINT = false;
                }
                gcRight.RefreshDataSource();
            };

            btnSua = new SimpleButton();
            btnSua.Text = QLyNSu.Functions.TranslationManager.Translate("Sửa quyền");
            btnSua.Size = new Size(115, 40);
            btnSua.Location = new Point(465, 10);
            btnSua.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSua.Appearance.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnSua.ImageOptions.SvgImage = GetSafeSvg("svgimages/actions/edit.svg", "svgimages/icon builder/actions_edit.svg");
            btnSua.Click += btnSua_Click;

            btnLuu = new SimpleButton();
            btnLuu.Text = QLyNSu.Functions.TranslationManager.Translate("Lưu");
            btnLuu.Size = new Size(115, 40);
            btnLuu.Location = new Point(590, 10);
            btnLuu.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLuu.Appearance.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnLuu.ImageOptions.SvgImage = GetSafeSvg("svgimages/save/save.svg", "svgimages/actions/save.svg");
            btnLuu.Click += async (s, ev) => await SaveRightsAsync();

            btnHuy = new SimpleButton();
            btnHuy.Text = QLyNSu.Functions.TranslationManager.Translate("Hủy");
            btnHuy.Size = new Size(115, 40);
            btnHuy.Location = new Point(715, 10);
            btnHuy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnHuy.Appearance.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnHuy.ImageOptions.SvgImage = GetSafeSvg("svgimages/actions/cancel.svg", "svgimages/actions/undo.svg");
            btnHuy.Click += btnHuy_Click;

            panelControl1.Controls.Add(btnLamMoi);
            panelControl1.Controls.Add(btnChonTatCa);
            panelControl1.Controls.Add(btnBoChonTatCa);
            panelControl1.Controls.Add(btnSua);
            panelControl1.Controls.Add(btnLuu);
            panelControl1.Controls.Add(btnHuy);
            var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,WrapContents=true,Padding=new Padding(6)};
            panelControl1.Controls.Clear();panelControl1.Height=72;
            foreach(var button in new[]{btnDong,btnLuu,btnHuy,btnSua,btnBoChonTatCa,btnChonTatCa,btnLamMoi}) {
                button.Anchor=AnchorStyles.None;button.Size=new Size(114,38);button.Margin=new Padding(4);
                button.Appearance.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;button.Appearance.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;actions.Controls.Add(button);
            }
            panelControl1.Controls.Add(actions);

        }

        private void SetEditingState(bool editing)
        {
            _isEditing = editing;
            if (btnLamMoi != null) btnLamMoi.Enabled = !editing;
            if (btnChonTatCa != null) btnChonTatCa.Enabled = editing;
            if (btnBoChonTatCa != null) btnBoChonTatCa.Enabled = editing;
            if (btnSua != null) btnSua.Enabled = !editing && UserSession.CanEdit("F_SYSTEM_PQ_CHUCNANG");
            if (btnLuu != null) btnLuu.Enabled = editing;
            if (btnHuy != null) btnHuy.Enabled = editing;

            gcUser.Enabled = !editing;
            if (rdoGroup != null) rdoGroup.Enabled = !editing;
            if (rdoUser != null) rdoUser.Enabled = !editing;
            if (rdoAllSubjects != null) rdoAllSubjects.Enabled = !editing;
            if (pnlChannelHeader != null)
            {
                rdoDesktopChannel.Enabled = !editing;
                rdoWebChannel.Enabled = !editing;
                rdoMobileChannel.Enabled = !editing;
                rdoAiPermissions.Enabled = !editing && UserSession.CanEdit("F_SYSTEM_PQ_CHUCNANG");
                chkParentAccess.Properties.ReadOnly = !editing;
            }

            gvRight.OptionsBehavior.Editable = editing;
        }

        private async void FrmPhanQuyenChucNang_Load(object sender, EventArgs e)
        {
            AutoScaleMode=AutoScaleMode.Dpi; Font=new Font("Segoe UI",9.5F); MinimumSize=new Size(1040,640);
            splitContainerControl1.Panel1.MinimumSize=new Size(250,0);
            splitContainerControl1.Panel2.MinimumSize=new Size(650,0);
            splitContainerControl1.SplitterPosition=300;

            try
            {
                await QLyNSu.Functions.FormDataLoader.RunAsync(this, () => new Bu.CLASS_SYSTEM.SYS_USER().EnsureSeeded());
            }
            catch { }

            db = new MyEntities();

            SetupTogglePanel();
            SetupChannelHeaderPanel();
            SetupActionButtons();

            // Set grid formatting
            FormManager_Functions.CustomView_Colums(gvUser);
            FormManager_Functions.CustomView_Colums(gvRight);

            gvRight.OptionsFind.AlwaysVisible = true;
            gvRight.OptionsFind.FindNullPrompt = QLyNSu.Functions.TranslationManager.Translate("Tìm kiếm chức năng / phân hệ...");

            // Configure event handlers
            gvUser.FocusedRowChanged += gvUser_FocusedRowChanged;
            gvRight.ShowingEditor += gvRight_ShowingEditor;

            SetEditingState(false);
            await loadUsers();

            QLyNSu.Functions.TranslationManager.Translate(this);
        }

        private async Task loadUsers()
        {
            try
            {
            int targetMode = rdoAllSubjects != null && rdoAllSubjects.Checked ? -1 : (rdoGroup != null && rdoGroup.Checked ? 1 : 0);
            // Never hide a subject based on its account name, including ADMIN.
            var data = await QLyNSu.Functions.FormDataLoader.ReadAsync(this, () => {
                using (var readDb = new MyEntities())
                    return readDb.TB_SYS_USER.Where(x => targetMode < 0 || (x.ISGROUP ?? 0) == targetMode).OrderBy(x => x.USERNAME).ToList();
            });
            _loadingSubjects=true;
            try { gcUser.DataSource = data; } finally { _loadingSubjects=false; }

            // Format User columns
            if (gvUser.Columns["IDUSER"] != null) gvUser.Columns["IDUSER"].Caption = "ID";
            if (targetMode == 1)
            {
                if (gvUser.Columns["USERNAME"] != null) gvUser.Columns["USERNAME"].Caption = QLyNSu.Functions.TranslationManager.Translate("Tên nhóm");
                if (gvUser.Columns["FULLNAME"] != null) gvUser.Columns["FULLNAME"].Caption = QLyNSu.Functions.TranslationManager.Translate("Mô tả nhóm");
            }
            else
            {
                if (gvUser.Columns["USERNAME"] != null) gvUser.Columns["USERNAME"].Caption = QLyNSu.Functions.TranslationManager.Translate("Tên tài khoản");
                if (gvUser.Columns["FULLNAME"] != null) gvUser.Columns["FULLNAME"].Caption = QLyNSu.Functions.TranslationManager.Translate("Họ và tên");
            }

            // Hide other columns
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gvUser.Columns)
            {
                if (col.FieldName != "IDUSER" && col.FieldName != "USERNAME" && col.FieldName != "FULLNAME")
                    col.Visible = false;
            }
            await loadRights();
        
            }
            catch (OperationCanceledException) { }
            catch (Exception error) { QLyNSu.Functions.FormDataLoader.ShowLoadError(this, error); }
        }

        private async void gvUser_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (_loadingSubjects) return;
            await loadRights();
        }

        private string GetModuleName(string parent)
        {
            if (string.IsNullOrEmpty(parent)) return "Hệ Thống";
            switch (parent.Trim().ToUpperInvariant())
            {
                case "SYSTEM": return "Hệ Thống";
                case "DASHBOARD": return "Dashboard & Báo Cáo";
                case "DM": return "Danh Mục";
                case "NV": return "Quản Lý Nhân Sự";
                case "CC": return "Chấm Công & Lương";
                case "MOBILE":
                case "MOBILE_ROOT": return "Mobile App";
                default: return parent;
            }
        }

        private async Task loadRights()
        {
            try
            {
            int requestVersion=++_rightsLoadVersion;
            var selectedUser = (TB_SYS_USER)gvUser.GetFocusedRow();
            if (selectedUser == null)
            {
                if(_aiPermissionsForm!=null) {_aiPermissionsForm.Dispose();_aiPermissionsForm=null;}
                gcRight.DataSource = null;
                if (chkParentAccess != null) chkParentAccess.Checked = false;
                if (lblParentInherited != null) lblParentInherited.Text = "";
                if (lblReadinessStatus != null) lblReadinessStatus.Text = "";
                return;
            }

            foreach(var button in new[]{btnSua,btnLuu,btnHuy,btnBoChonTatCa,btnChonTatCa,btnLamMoi}) button.Visible=!IsAiPage;
            chkParentAccess.Visible=!IsAiPage;lblParentInherited.Visible=!IsAiPage;lblReadinessStatus.Visible=!IsAiPage;
            gcRight.Visible=!IsAiPage;
            if(_aiPermissionsForm!=null) {_aiPermissionsForm.Dispose();_aiPermissionsForm=null;}
            if(IsAiPage)
            {
                _aiPermissionsForm=new FrmAiScopeGrantDetail((int)selectedUser.IDUSER,selectedUser.USERNAME,selectedUser.FULLNAME,(selectedUser.ISGROUP ?? 0)==1);
                _aiPermissionsForm.ConfigureEmbedded();
                _aiPermissionsForm.DirtyStateChanged+=(s,e)=>{bool clean=!((FrmAiScopeGrantDetail)s).HasUnsavedChanges;gcUser.Enabled=clean;rdoUser.Enabled=clean;rdoGroup.Enabled=clean;rdoAllSubjects.Enabled=clean;};
                splitContainerControl1.Panel2.Controls.Add(_aiPermissionsForm);_aiPermissionsForm.BringToFront();_aiPermissionsForm.Show();
                return;
            }
            gcRight.BringToFront();
            string channel = CurrentChannelCode;

            // Nạp thông tin cây quyền theo kênh từ Resolver thực
            decimal subjectId=selectedUser.IDUSER;
            string subjectName=selectedUser.USERNAME;
            var tree = await QLyNSu.Functions.FormDataLoader.ReadAsync(this, () => {
                using (var readDb = new MyEntities())
                    return _channelResolver.ResolveChannelTree(readDb, subjectId, channel, subjectName);
            });
            if (requestVersion!=_rightsLoadVersion || IsDisposed || Disposing) return;

            // Cập nhật Quyền cha trên Header
            if (chkParentAccess != null)
            {
                chkParentAccess.Checked = tree.ParentDirectGrant;
            }

            if (lblParentInherited != null)
            {
                if (tree.ParentInheritedGrant)
                {
                    string groupInfo = tree.ParentInheritedGroupNames != null && tree.ParentInheritedGroupNames.Any()
                        ? string.Join(", ", tree.ParentInheritedGroupNames)
                        : "Nhóm";
                    lblParentInherited.Text = $"(Kế thừa quyền cha từ nhóm: {groupInfo})";
                    lblParentInherited.ForeColor = Color.FromArgb(5, 150, 105);
                }
                else
                {
                    lblParentInherited.Text = tree.ParentDirectGrant ? "(Quyền trực tiếp)" : "(Chưa có quyền cha)";
                    lblParentInherited.ForeColor = tree.ParentDirectGrant ? Color.FromArgb(30, 64, 175) : Color.FromArgb(100, 116, 139);
                }
            }

            if (lblReadinessStatus != null)
            {
                string rStatus = tree.ReadinessCode ?? "READY";
                lblReadinessStatus.Text = $"Trạng thái kênh: {rStatus} - {tree.ReadinessMessage}";
                lblReadinessStatus.ForeColor = rStatus == "READY" ? Color.FromArgb(22, 101, 52) : Color.FromArgb(185, 28, 28);
            }

            // Map DTO sang ViewModel của Grid
            _rightList = tree.Functions
                .Where(f => f.SupportedCapabilities != null &&
                    (f.SupportedCapabilities.CanView || f.SupportedCapabilities.CanAdd || f.SupportedCapabilities.CanEdit || f.SupportedCapabilities.CanDelete || f.SupportedCapabilities.CanPrint))
                .Select(f => new ChannelFunctionRightViewModel
            {
                MODULE_NAME = QLyNSu.Functions.TranslationManager.Translate(GetModuleName(f.ParentCode)),
                FUNCTION_CODE = f.FunctionCode,
                DESCRIPTION = QLyNSu.Functions.TranslationManager.Translate(f.FunctionName),
                RIGHT_TYPE = f.RightType ?? "FUNCTION",
                IsDisabledByParent = !tree.ParentIsEffective,
                RestrictionNote = f.RestrictionNote,

                // Khả năng kênh hỗ trợ
                SupportedCanView = f.SupportedCapabilities.CanView,
                SupportedCanAdd = f.SupportedCapabilities.CanAdd,
                SupportedCanEdit = f.SupportedCapabilities.CanEdit,
                SupportedCanDelete = f.SupportedCapabilities.CanDelete,
                SupportedCanPrint = f.SupportedCapabilities.CanPrint,

                // Direct grants
                CAN_VIEW = f.DirectGrant.CanView,
                CAN_ADD = f.DirectGrant.CanAdd,
                CAN_EDIT = f.DirectGrant.CanEdit,
                CAN_DELETE = f.DirectGrant.CanDelete,
                CAN_PRINT = f.DirectGrant.CanPrint,

                // Kế thừa
                InheritedCanView = f.InheritedGrant.CanView,
                InheritedCanAdd = f.InheritedGrant.CanAdd,
                InheritedCanEdit = f.InheritedGrant.CanEdit,
                InheritedCanDelete = f.InheritedGrant.CanDelete,
                InheritedCanPrint = f.InheritedGrant.CanPrint,

                // Hiệu lực
                EffectiveCanView = f.EffectiveGrant.CanView,
                EffectiveCanAdd = f.EffectiveGrant.CanAdd,
                EffectiveCanEdit = f.EffectiveGrant.CanEdit,
                EffectiveCanDelete = f.EffectiveGrant.CanDelete,
                EffectiveCanPrint = f.EffectiveGrant.CanPrint
            }).ToList();

            gcRight.DataSource = new BindingList<ChannelFunctionRightViewModel>(_rightList);

            // Format right columns
            FormatRightColumns();
            gvRight.OptionsView.ColumnAutoWidth=true;
            gvRight.RowHeight=34;
            gvRight.ColumnPanelRowHeight=40;
            gvRight.OptionsView.RowAutoHeight=true;
            gvRight.Appearance.HeaderPanel.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;
            gvRight.Appearance.HeaderPanel.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;
            foreach(DevExpress.XtraGrid.Columns.GridColumn column in gvRight.Columns) {
                column.MinWidth = column.FieldName.StartsWith("CAN_") ? 72 : (column.FieldName=="DESCRIPTION" ? 280 : column.FieldName=="RestrictionNote" ? 220 : 100);
                column.OptionsColumn.FixedWidth=column.FieldName.StartsWith("CAN_");
                column.AppearanceCell.TextOptions.WordWrap=DevExpress.Utils.WordWrap.Wrap;
                column.AppearanceCell.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;
                column.AppearanceCell.TextOptions.HAlignment=(column.FieldName=="DESCRIPTION" || column.FieldName=="RestrictionNote") ? DevExpress.Utils.HorzAlignment.Near : DevExpress.Utils.HorzAlignment.Center;
            }

        
            }
            catch (OperationCanceledException) { }
            catch (Exception error) { QLyNSu.Functions.FormDataLoader.ShowLoadError(this, error); }
        }

        private void FormatRightColumns()
        {
            if (gvRight.Columns["MODULE_NAME"] != null)
            {
                gvRight.Columns["MODULE_NAME"].Caption = QLyNSu.Functions.TranslationManager.Translate("Phân hệ");
                gvRight.Columns["MODULE_NAME"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["MODULE_NAME"].Visible = true;
                gvRight.Columns["MODULE_NAME"].Width = 130;
            }
            if (gvRight.Columns["FUNCTION_CODE"] != null)
            {
                gvRight.Columns["FUNCTION_CODE"].Caption = QLyNSu.Functions.TranslationManager.Translate("Mã chức năng");
                gvRight.Columns["FUNCTION_CODE"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["FUNCTION_CODE"].Visible = true;
                gvRight.Columns["FUNCTION_CODE"].Width = 150;
            }
            if (gvRight.Columns["RIGHT_TYPE"] != null)
            {
                gvRight.Columns["RIGHT_TYPE"].Caption = QLyNSu.Functions.TranslationManager.Translate("Loại quyền");
                gvRight.Columns["RIGHT_TYPE"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["RIGHT_TYPE"].Visible = true;
                gvRight.Columns["RIGHT_TYPE"].Width = 85;
            }
            if (gvRight.Columns["DESCRIPTION"] != null)
            {
                gvRight.Columns["DESCRIPTION"].Caption = QLyNSu.Functions.TranslationManager.Translate("Tên chức năng");
                gvRight.Columns["DESCRIPTION"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["DESCRIPTION"].Width = 340;
            }
            if (gvRight.Columns["CAN_VIEW"] != null)
            {
                gvRight.Columns["CAN_VIEW"].Caption = QLyNSu.Functions.TranslationManager.Translate("Xem");
                gvRight.Columns["CAN_VIEW"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_VIEW"].Width = 60;
            }
            if (gvRight.Columns["CAN_ADD"] != null)
            {
                gvRight.Columns["CAN_ADD"].Caption = QLyNSu.Functions.TranslationManager.Translate("Thêm");
                gvRight.Columns["CAN_ADD"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_ADD"].Width = 60;
            }
            if (gvRight.Columns["CAN_EDIT"] != null)
            {
                gvRight.Columns["CAN_EDIT"].Caption = QLyNSu.Functions.TranslationManager.Translate("Sửa");
                gvRight.Columns["CAN_EDIT"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_EDIT"].Width = 60;
            }
            if (gvRight.Columns["CAN_DELETE"] != null)
            {
                gvRight.Columns["CAN_DELETE"].Caption = QLyNSu.Functions.TranslationManager.Translate("Xóa");
                gvRight.Columns["CAN_DELETE"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_DELETE"].Width = 60;
            }
            if (gvRight.Columns["CAN_PRINT"] != null)
            {
                gvRight.Columns["CAN_PRINT"].Caption = QLyNSu.Functions.TranslationManager.Translate("In");
                gvRight.Columns["CAN_PRINT"].OptionsColumn.AllowEdit = true;
                gvRight.Columns["CAN_PRINT"].Width = 60;
            }
            if (gvRight.Columns["RestrictionNote"] != null)
            {
                gvRight.Columns["RestrictionNote"].Caption = QLyNSu.Functions.TranslationManager.Translate("Giới hạn kênh");
                gvRight.Columns["RestrictionNote"].OptionsColumn.AllowEdit = false;
                gvRight.Columns["RestrictionNote"].Width = 250;
            }

            // Ẩn các cột bổ trợ không cần hiển thị trực tiếp
            string[] hiddenCols = { "IsDisabledByParent", "SupportedCanView", "SupportedCanAdd", "SupportedCanEdit", "SupportedCanDelete", "SupportedCanPrint",
                                    "InheritedCanView", "InheritedCanAdd", "InheritedCanEdit", "InheritedCanDelete", "InheritedCanPrint",
                                    "EffectiveCanView", "EffectiveCanAdd", "EffectiveCanEdit", "EffectiveCanDelete", "EffectiveCanPrint" };
            foreach (var h in hiddenCols)
            {
                if (gvRight.Columns[h] != null) gvRight.Columns[h].Visible = false;
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if(!UserSession.CanEdit("F_SYSTEM_PQ_CHUCNANG")) return;
            if (gvUser.GetFocusedRow() == null)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn người dùng hoặc nhóm để sửa quyền."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetEditingState(true);
        }

        private void gvRight_ShowingEditor(object sender, CancelEventArgs e)
        {
            var item = gvRight.GetFocusedRow() as ChannelFunctionRightViewModel;
            if (item == null) return;

            // 1. Nếu quyền cha đang tắt: Khóa toàn bộ chỉnh sửa quyền con
            if (chkParentAccess != null && !chkParentAccess.Checked)
            {
                e.Cancel = true;
                return;
            }

            // 2. Khóa các thao tác không được kênh hỗ trợ
            string colName = gvRight.FocusedColumn?.FieldName;
            if (colName == "CAN_VIEW" && !item.SupportedCanView) e.Cancel = true;
            if (colName == "CAN_ADD" && !item.SupportedCanAdd) e.Cancel = true;
            if (colName == "CAN_EDIT" && !item.SupportedCanEdit) e.Cancel = true;
            if (colName == "CAN_DELETE" && !item.SupportedCanDelete) e.Cancel = true;
            if (colName == "CAN_PRINT" && !item.SupportedCanPrint) e.Cancel = true;
        }

        private async Task SaveRightsAsync()
        {
            if (IsAiPage || _isSaving) return;
            if(!UserSession.CanEdit("F_SYSTEM_PQ_CHUCNANG")) {XtraMessageBox.Show("Bạn không có quyền sửa Phân quyền chức năng.");return;}
            _isSaving = true;

            var selectedUser = (TB_SYS_USER)gvUser.GetFocusedRow();
            if (selectedUser == null)
            {
                _isSaving = false;
                return;
            }

            try
            {
                string channel = CurrentChannelCode;
                decimal actorId = UserSession.CurrentUser != null ? UserSession.CurrentUser.IDUSER : 0;

                var req = new SaveChannelRightsRequest
                {
                    ActorUserId = actorId,
                    ActorChannel = UserSession.CurrentChannel,
                    TargetUserId = selectedUser.IDUSER,
                    Channel = channel,
                    ParentDirectGrant = chkParentAccess.Checked,
                    Functions = _rightList.Select(item => new SaveChannelFunctionItemRequest
                    {
                        FunctionCode = item.FUNCTION_CODE,
                        CanView = item.CAN_VIEW,
                        CanAdd = item.CAN_ADD,
                        CanEdit = item.CAN_EDIT,
                        CanDelete = item.CAN_DELETE,
                        CanPrint = item.CAN_PRINT
                    }).ToList()
                };

                // Lưu qua ChannelPermissionResolver tập trung (sử dụng transaction, audit, và thu hồi phiên chuẩn)
                var result = await _channelResolver.SaveChannelRightsAsync(req);

                if (!result.Success)
                {
                    XtraMessageBox.Show(
                        QLyNSu.Functions.TranslationManager.Translate("Không thể lưu phân quyền: ") + result.Message,
                        QLyNSu.Functions.TranslationManager.Translate("Lỗi"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                XtraMessageBox.Show(
                    QLyNSu.Functions.TranslationManager.Translate("Lưu phân quyền thành công.") + "\n" + (result.Message ?? ""),
                    QLyNSu.Functions.TranslationManager.Translate("Thông báo"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                SetEditingState(false);
                await loadRights();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    QLyNSu.Functions.TranslationManager.Translate("Lỗi phát sinh khi lưu:") + " " + ex.Message,
                    QLyNSu.Functions.TranslationManager.Translate("Lỗi"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                _isSaving = false;
            }
        }

        private async void btnHuy_Click(object sender, EventArgs e)
        {
            SetEditingState(false);
            await loadRights();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(_isSaving) {e.Cancel=true; return;}
            if(_isEditing && XtraMessageBox.Show("Có thay đổi phân quyền chưa lưu. Hủy thay đổi để đóng?", "Phân quyền", MessageBoxButtons.YesNo, MessageBoxIcon.Question)!=DialogResult.Yes) e.Cancel=true;
            if(_aiPermissionsForm!=null && !_aiPermissionsForm.TryDiscardChanges()) e.Cancel=true;
            base.OnFormClosing(e);
        }

        // ViewModel cho GridView phân quyền chức năng theo kênh
        public class ChannelFunctionRightViewModel
        {
            public string MODULE_NAME { get; set; }
            public string FUNCTION_CODE { get; set; }
            public string DESCRIPTION { get; set; }
            public string RIGHT_TYPE { get; set; }
            public string RestrictionNote { get; set; }
            public bool IsDisabledByParent { get; set; }

            // Supported capabilities
            public bool SupportedCanView { get; set; }
            public bool SupportedCanAdd { get; set; }
            public bool SupportedCanEdit { get; set; }
            public bool SupportedCanDelete { get; set; }
            public bool SupportedCanPrint { get; set; }

            // Direct editable grants
            public bool CAN_VIEW { get; set; }
            public bool CAN_ADD { get; set; }
            public bool CAN_EDIT { get; set; }
            public bool CAN_DELETE { get; set; }
            public bool CAN_PRINT { get; set; }

            // Inherited
            public bool InheritedCanView { get; set; }
            public bool InheritedCanAdd { get; set; }
            public bool InheritedCanEdit { get; set; }
            public bool InheritedCanDelete { get; set; }
            public bool InheritedCanPrint { get; set; }

            // Effective
            public bool EffectiveCanView { get; set; }
            public bool EffectiveCanAdd { get; set; }
            public bool EffectiveCanEdit { get; set; }
            public bool EffectiveCanDelete { get; set; }
            public bool EffectiveCanPrint { get; set; }
        }
    }
}
