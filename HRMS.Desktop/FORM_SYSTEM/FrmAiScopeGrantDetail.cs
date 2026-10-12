using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using Bu.CLASS_SYSTEM;
using Bu.Services.AI_Services.Security;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmAiScopeGrantDetail : XtraForm
    {
        private readonly int _subjectId;
        private readonly string _subjectCode;
        private readonly string _subjectName;
        private readonly bool _isGroup;
        private readonly IAiScopeGrantManagementService _scopeService;

        private BindingList<AiScopeRowViewModel> _bindingList;
        private List<LookupItem> _departmentLookup;
        private List<LookupItem> _companyLookup;
        private Dictionary<string, string> _departmentDict = new Dictionary<string, string>();
        private Dictionary<string, string> _companyDict = new Dictionary<string, string>();

        private long _baseRevision = 0;
        private bool _isDirty = false;
        private bool _isSaving = false;
        private bool _isReady;
        private bool _updatingSimplePermission;
        private DevExpress.XtraGrid.Columns.GridColumn _simplePermissionColumn;
        private int? _actorManv = null;

        public class LookupItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class AiScopeRowViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string CapabilityCode { get; set; }
            public string Domain { get; set; }
            public string Description { get; set; }
            private bool _isAiAllowed;
            public bool IsAiAllowed { get => _isAiAllowed; set { if(_isAiAllowed!=value) {_isAiAllowed=value;OnPropertyChanged(nameof(IsAiAllowed));} } }
            public bool IsCapabilityEnabled { get; set; }
            public string AvailabilityStatus { get; set; }
            public string RequiredFunctionCode { get; set; }
            public bool HasRequiredFunctionRight { get; set; }
            public string FunctionRightDisplay { get; set; }

            // Danh sách các grant trực tiếp (hỗ trợ nhiều grant)
            public List<AiScopeGrantItemDto> DirectGrants { get; set; } = new List<AiScopeGrantItemDto>();

            private string _directEffect = "NONE";
            public string DirectEffect
            {
                get => _directEffect;
                set { if (_directEffect != value) { _directEffect = value; OnPropertyChanged(nameof(DirectEffect)); } }
            }

            private string _directScopeType = "SELF";
            public string DirectScopeType
            {
                get => _directScopeType;
                set { if (_directScopeType != value) { _directScopeType = value; OnPropertyChanged(nameof(DirectScopeType)); } }
            }

            private string _directScopeKey;
            public string DirectScopeKey
            {
                get => _directScopeKey;
                set { if (_directScopeKey != value) { _directScopeKey = value; OnPropertyChanged(nameof(DirectScopeKey)); } }
            }

            private DateTime? _directValidFrom;
            public DateTime? DirectValidFrom
            {
                get => _directValidFrom;
                set { if (_directValidFrom != value) { _directValidFrom = value; OnPropertyChanged(nameof(DirectValidFrom)); } }
            }

            private DateTime? _directValidTo;
            public DateTime? DirectValidTo
            {
                get => _directValidTo;
                set { if (_directValidTo != value) { _directValidTo = value; OnPropertyChanged(nameof(DirectValidTo)); } }
            }

            public List<AiInheritedScopeGrantDto> InheritedGrants { get; set; } = new List<AiInheritedScopeGrantDto>();

            public string InheritedSummary { get; set; }
            public bool HasInheritedGrant { get; set; }
            public string InheritedEffect { get; set; }
            public string InheritedScopeType { get; set; }
            public string InheritedScopeKey { get; set; }
            public string InheritedFromGroup { get; set; }
            public DateTime? InheritedValidTo { get; set; }

            private string _effectiveScopeSummary;
            public string EffectiveScopeSummary
            {
                get => _effectiveScopeSummary;
                set { if (_effectiveScopeSummary != value) { _effectiveScopeSummary = value; OnPropertyChanged(nameof(EffectiveScopeSummary)); } }
            }

            private string _explanationNotes;
            public string ExplanationNotes
            {
                get => _explanationNotes;
                set { if (_explanationNotes != value) { _explanationNotes = value; OnPropertyChanged(nameof(ExplanationNotes)); } }
            }

            public string EffectiveEffect { get; set; }

            protected void OnPropertyChanged(string propName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public FrmAiScopeGrantDetail(int subjectId, string subjectCode, string subjectName, bool isGroup)
        {
            InitializeComponent();
            _subjectId = subjectId;
            _subjectCode = subjectCode;
            _subjectName = subjectName;
            _isGroup = isGroup;
            _scopeService = new AiScopeGrantManagementService();
        }

        public event EventHandler DirtyStateChanged;
        private void SetDirty(bool value) { _isDirty=value; DirtyStateChanged?.Invoke(this,EventArgs.Empty); }
        public bool HasUnsavedChanges => _isDirty;
        public bool TryDiscardChanges()
        {
            if(_isSaving) return false;
            if(!_isDirty) return true;
            if(XtraMessageBox.Show("Có thay đổi quyền AI chưa lưu. Hủy thay đổi để tiếp tục?", "Phân quyền AI", MessageBoxButtons.YesNo, MessageBoxIcon.Question)!=DialogResult.Yes) return false;
            SetDirty(false); return true;
        }
        public void ConfigureEmbedded()
        {
            TopLevel=false; FormBorderStyle=FormBorderStyle.None; Dock=DockStyle.Fill;
            btnDong.Visible=false;
        }
        private void ConfigureResponsiveLayout()
        {
            AutoScaleMode=AutoScaleMode.Dpi; Font=new Font("Segoe UI",9.5F);
            gridViewGrants.OptionsView.ColumnAutoWidth=true;
            gridViewGrants.OptionsDetail.EnableMasterViewMode=false;
            foreach(DevExpress.XtraGrid.Columns.GridColumn column in gridViewGrants.Columns) column.Visible=false;
            colDescription.Visible=true;colDescription.VisibleIndex=0;colDescription.Caption="Chức năng AI";colDescription.Width=420;
            var toggle=new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit { AutoHeight=false };
            toggle.EditValueChanged+=(s,e)=>gridViewGrants.PostEditor();
            gridControlGrants.RepositoryItems.Add(toggle);
            _simplePermissionColumn=gridViewGrants.Columns.AddField("IsAiAllowed");
            _simplePermissionColumn.Caption="Cho phép";_simplePermissionColumn.Visible=true;_simplePermissionColumn.VisibleIndex=1;
            _simplePermissionColumn.Width=120;_simplePermissionColumn.MinWidth=110;_simplePermissionColumn.OptionsColumn.FixedWidth=true;
            _simplePermissionColumn.ColumnEdit=toggle;
            gridViewGrants.OptionsView.RowAutoHeight=true;
            gridViewGrants.RowHeight=34;
            gridViewGrants.ColumnPanelRowHeight=40;
            gridViewGrants.OptionsFind.AlwaysVisible=true;
            gridViewGrants.OptionsFind.FindNullPrompt=QLyNSu.Functions.TranslationManager.Translate("Tìm kiếm chức năng / phân hệ...");
            gridViewGrants.Appearance.Row.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;
            gridViewGrants.Appearance.HeaderPanel.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;
            gridViewGrants.Appearance.HeaderPanel.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;
            foreach(DevExpress.XtraGrid.Columns.GridColumn column in gridViewGrants.Columns)
            {
                column.AppearanceCell.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;
                column.AppearanceCell.TextOptions.HAlignment=(column==colDescription || column==colExplanation || column==colInherited) ? DevExpress.Utils.HorzAlignment.Near : DevExpress.Utils.HorzAlignment.Center;
                column.AppearanceCell.TextOptions.WordWrap=DevExpress.Utils.WordWrap.Wrap;
            }
            var top=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Padding=new Padding(8),AutoScroll=false};
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            for(int i=0;i<2;i++) top.RowStyles.Add(new RowStyle(SizeType.Absolute,28));
            pnlTop.Controls.Clear(); pnlTop.Height=138;
            foreach(var label in new[]{lblSubjectTitle,lblSecurityWarning})
            { label.Dock=DockStyle.Fill; label.AutoSizeMode=DevExpress.XtraEditors.LabelAutoSizeMode.None; label.MinimumSize=new Size(0,28);label.Appearance.Font=Font;label.Appearance.TextOptions.WordWrap=DevExpress.Utils.WordWrap.Wrap;label.Appearance.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;top.Controls.Add(label); }
            pnlTop.Controls.Add(top);
            lblSubjectTitle.Appearance.Font=new Font(Font.FontFamily,11F,FontStyle.Bold);
            lblSubjectTitle.Appearance.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;
            lblAiRightStatus.Appearance.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;
            lblSecurityWarning.Text="Chọn Có để cho phép AI dùng nghiệp vụ, Không để chặn.\r\nGiữ nguyên phạm vi đã cấp; quyền mới chỉ áp dụng cho bản thân.";
            lblSecurityWarning.Appearance.BackColor=Color.Empty;
            lblSecurityWarning.Appearance.ForeColor=SystemColors.ControlText;
            lblSecurityWarning.Appearance.Options.UseBackColor=false;
            int previousWidth=-1;
            Action reflowHeader=()=>{
                int width=Math.Max(240,pnlTop.ClientSize.Width-top.Padding.Horizontal);
                if(width==previousWidth)return; previousWidth=width;
                int total=top.Padding.Vertical;
                var labels=new[]{lblSubjectTitle,lblSecurityWarning};
                for(int i=0;i<labels.Length;i++) {
                    var label=labels[i];
                    int height=Math.Max(28,TextRenderer.MeasureText(label.Text,label.Appearance.Font,
                        new Size(Math.Max(100,width-label.Padding.Horizontal-label.Margin.Horizontal),int.MaxValue),
                        TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+label.Padding.Vertical+8);
                    top.RowStyles[i].Height=height+label.Margin.Vertical; total+=height+label.Margin.Vertical;
                }
                pnlTop.Height=total;
            };
            pnlTop.SizeChanged+=(s,e)=>reflowHeader();
            foreach(var label in new[]{lblSubjectTitle,lblSecurityWarning}) label.TextChanged+=(s,e)=>{previousWidth=-1;reflowHeader();};
            reflowHeader();

            var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,WrapContents=true,Padding=new Padding(6)};
            pnlBottom.Controls.Clear();pnlBottom.Height=66;
            foreach(var button in new[]{btnDong,btnLuu,btnLamMoi}) {
                button.Anchor=AnchorStyles.None;button.Size=new Size(135,38);button.Margin=new Padding(4);
                button.Appearance.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;button.Appearance.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;actions.Controls.Add(button);
            }
            pnlBottom.Controls.Add(actions);
        }
        private List<AiScopeGrantRecord> BuildDirectGrants(AiScopeRowViewModel row)
        {
            var grants=new List<AiScopeGrantRecord>();
            if(row.DirectEffect=="NONE") return grants;
            grants.Add(new AiScopeGrantRecord {CapabilityCode=row.CapabilityCode,Effect=row.DirectEffect,ScopeType=row.DirectScopeType,ScopeKey=row.DirectScopeKey,ValidFrom=row.DirectValidFrom,ValidTo=row.DirectValidTo,IsEnabled=true});
            foreach(var extra in row.DirectGrants.Skip(1)) grants.Add(new AiScopeGrantRecord {CapabilityCode=row.CapabilityCode,Effect=extra.Effect,ScopeType=extra.ScopeType,ScopeKey=extra.ScopeKey,ValidFrom=extra.ValidFrom,ValidTo=extra.ValidTo,IsEnabled=true});
            return grants;
        }
        private static bool IsActiveGrant(AiScopeGrantItemDto grant)
        {
            var now=DateTime.Now;
            return grant.IsActive && (!grant.ValidFrom.HasValue || grant.ValidFrom.Value<=now) && (!grant.ValidTo.HasValue || grant.ValidTo.Value>now);
        }
        private static bool HasInheritedDeny(AiScopeRowViewModel row) => row.InheritedGrants.Any(g=>g.IsActive && g.Effect=="DENY");
        private static bool HasSimplePermission(AiScopeRowViewModel row)
        {
            if(!row.IsCapabilityEnabled || HasInheritedDeny(row) || row.DirectGrants.Any(g=>IsActiveGrant(g) && g.Effect=="DENY")) return false;
            return row.DirectGrants.Any(g=>IsActiveGrant(g) && g.Effect=="ALLOW") || row.InheritedGrants.Any(g=>g.IsActive && g.Effect=="ALLOW") ||
                ((row.CapabilityCode=="INSURANCE_SELF" || row.CapabilityCode=="PAYROLL_SELF") && row.EffectiveEffect=="ALLOW");
        }
        private static void SetSimplePermission(AiScopeRowViewModel row,bool enabled)
        {
            if(!row.IsCapabilityEnabled || (enabled && HasInheritedDeny(row))) {row.IsAiAllowed=HasSimplePermission(row);return;}
            var grants=row.DirectGrants.Where(g=>g.Effect=="ALLOW").Select(g=>new AiScopeGrantItemDto {
                GrantId=g.GrantId,CapabilityCode=row.CapabilityCode,Effect=g.Effect,ScopeType=g.ScopeType,ScopeKey=g.ScopeKey,
                ScopeName=g.ScopeName,ValidFrom=g.ValidFrom,ValidTo=g.ValidTo,IsActive=g.IsActive
            }).ToList();
            if(!enabled) grants.Add(new AiScopeGrantItemDto {CapabilityCode=row.CapabilityCode,Effect="DENY",ScopeType="ALL",IsActive=true});
            else if(!grants.Any(g=>IsActiveGrant(g)) && !row.InheritedGrants.Any(g=>g.IsActive && g.Effect=="ALLOW")) {
                var self=grants.FirstOrDefault(g=>g.ScopeType=="SELF");
                if(self!=null) {self.ValidFrom=null;self.ValidTo=null;self.IsActive=true;}
                else grants.Add(new AiScopeGrantItemDto {CapabilityCode=row.CapabilityCode,Effect="ALLOW",ScopeType="SELF",IsActive=true});
            }
            row.DirectGrants=grants;var first=grants.FirstOrDefault();
            row.DirectEffect=first?.Effect ?? "NONE";row.DirectScopeType=first?.ScopeType ?? "SELF";row.DirectScopeKey=first?.ScopeKey;
            row.DirectValidFrom=first?.ValidFrom;row.DirectValidTo=first?.ValidTo;row.IsAiAllowed=HasSimplePermission(row);
        }
        private async void FrmAiScopeGrantDetail_Load(object sender, EventArgs e)
        {
            ConfigureResponsiveLayout();
            // Kiểm tra quyền quản lý phân quyền trên kênh đang đăng nhập.
            if (!UserSession.CanEdit("F_SYSTEM_PQ_CHUCNANG"))
            {
                XtraMessageBox.Show("Từ chối truy cập: Cần quyền sửa Phân quyền chức năng để quản lý quyền AI.", "Lỗi bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.Text = _isGroup
                ? string.Format(QLyNSu.Functions.TranslationManager.Translate("Quản trị Quyền tra cứu AI - Nhóm quyền: [{0}] - {1}"), _subjectCode, _subjectName)
                : string.Format(QLyNSu.Functions.TranslationManager.Translate("Quản trị Quyền tra cứu AI - Tài khoản: [{0}] - {1}"), _subjectCode, _subjectName);

            lblSubjectTitle.Text = _isGroup
                ? $"Nhóm quyền: {_subjectCode} ({_subjectName})"
                : $"Tài khoản: {_subjectCode} - {_subjectName}";

            // Ẩn cột kế thừa từ nhóm nếu đối tượng chính là nhóm
            colInherited.Visible = false;

            await LoadLookups();
            if (IsDisposed || Disposing) return;
            await LoadData();
            if (IsDisposed || Disposing) return;
            QLyNSu.Functions.TranslationManager.Translate(this);

            // Format hàng theo trạng thái
            gridViewGrants.RowStyle += GridViewGrants_RowStyle;
            gridViewGrants.CellValueChanged += GridViewGrants_CellValueChanged;
            gridViewGrants.ShowingEditor += GridViewGrants_ShowingEditor;
            gridViewGrants.CustomColumnDisplayText+=(s,args)=>{
                if(args.Column!=colDirectEffect || args.ListSourceRowIndex<0 || _bindingList==null || args.ListSourceRowIndex>=_bindingList.Count) return;
                var row=_bindingList[args.ListSourceRowIndex];
                if(row.DirectEffect!="NONE" && row.DirectGrants.Count>1) args.DisplayText+=" ("+row.DirectGrants.Count+" phạm vi)";
            };

        }

        private async Task LoadLookups()
        {
            try
            {
                var depts = (await QLyNSu.Functions.FormDataLoader.ReadAsync(this, () => _scopeService.GetDepartmentOptions())) ?? new List<KeyValuePair<string, string>>();
                _departmentLookup = depts.Select(d => new LookupItem { Id = d.Key, Name = $"{d.Value} (ID: {d.Key})" }).ToList();
                _departmentDict = depts.ToDictionary(k => k.Key, v => v.Value);

                var comps = (await QLyNSu.Functions.FormDataLoader.ReadAsync(this, () => _scopeService.GetCompanyOptions())) ?? new List<KeyValuePair<string, string>>();
                _companyLookup = comps.Select(c => new LookupItem { Id = c.Key, Name = $"{c.Value} (Mã: {c.Key})" }).ToList();
                _companyDict = comps.ToDictionary(k => k.Key, v => v.Value);

                repoLookUpScopeKey.DataSource = _departmentLookup;
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải danh mục phòng ban / công ty: " + ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadData()
        {
            try
            {
                var overview = await QLyNSu.Functions.FormDataLoader.ReadAsync(this, () => _scopeService.GetSubjectScopeOverview(_isGroup ? "GROUP" : "USER", _subjectId));
                _baseRevision = overview.CurrentRevision;
                _isReady=overview.IsReady;
                _actorManv = overview.EmployeeId;

                // Kiểm tra tính sẵn sàng của Schema
                if (!overview.IsReady)
                {
                    lblSecurityWarning.Text = "HỆ THỐNG CHÍNH SÁCH AI CHƯA SẴN SÀNG: " + overview.NotReadyReason;
                    pnlSecurityNotice.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                    btnLuu.Enabled = false;
                    gridViewGrants.OptionsBehavior.Editable = false;
                }
                else
                {
                    btnLuu.Enabled = true;
                    gridViewGrants.OptionsBehavior.Editable = true;
                }

                lblAiRightStatus.Text = overview.HasAiFeatureRight
                    ? "Quyền giao diện AI (F_SYSTEM_AI): ĐÃ CẤP (Được phép mở tính năng AI trên ứng dụng)"
                    : "Quyền giao diện AI (F_SYSTEM_AI): CHƯA CẤP (Cần cấp quyền này để tài khoản mở giao diện AI)";
                lblAiRightStatus.ForeColor = overview.HasAiFeatureRight ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);

                var list = new List<AiScopeRowViewModel>();
                foreach (var c in overview.Capabilities)
                {
                    string inhSummary = c.InheritedSummary ?? "Không có";
                    string funcDisplay = string.IsNullOrEmpty(c.RequiredFunctionCode)
                        ? "Không yêu cầu (Tự tra cứu)"
                        : $"{c.RequiredFunctionCode} - {(c.HasRequiredFunctionRight ? "ĐÃ CÓ" : "CHƯA CÓ")}";

                    var vm = new AiScopeRowViewModel
                    {
                        CapabilityCode = c.CapabilityCode,
                        Domain = c.Domain,
                        Description = c.Description,
                        IsCapabilityEnabled = c.IsCapabilityEnabled,
                        AvailabilityStatus = c.AvailabilityStatus,
                        RequiredFunctionCode = c.RequiredFunctionCode,
                        HasRequiredFunctionRight = c.HasRequiredFunctionRight,
                        FunctionRightDisplay = funcDisplay,
                        DirectGrants = c.DirectGrants ?? new List<AiScopeGrantItemDto>(),
                        DirectEffect = c.DirectEffect ?? "NONE",
                        DirectScopeType = c.DirectScopeType ?? "SELF",
                        DirectScopeKey = c.DirectScopeKey,
                        DirectValidFrom = c.DirectValidFrom,
                        DirectValidTo = c.DirectValidTo,
                        InheritedGrants = c.InheritedGrants ?? new List<AiInheritedScopeGrantDto>(),
                        HasInheritedGrant = c.HasInheritedGrant,
                        InheritedEffect = c.InheritedEffect,
                        InheritedScopeType = c.InheritedScopeType,
                        InheritedScopeKey = c.InheritedScopeKey,
                        InheritedFromGroup = c.InheritedFromGroup,
                        InheritedValidTo = c.InheritedValidTo,
                        InheritedSummary = inhSummary,
                        EffectiveEffect = c.EffectiveEffect,
                        EffectiveScopeSummary = c.EffectiveScopeSummary,
                        ExplanationNotes = c.ExplanationNotes
                    };
                    vm.IsAiAllowed=HasSimplePermission(vm);
                    if(!vm.IsCapabilityEnabled) vm.Description+=" (Chưa hỗ trợ)";
                    list.Add(vm);
                }

                _bindingList = new BindingList<AiScopeRowViewModel>(list);
                gridControlGrants.DataSource = _bindingList;
                SetDirty(false);
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                _isReady=false; gridViewGrants.OptionsBehavior.Editable=false;
                btnLuu.Enabled = false;
                XtraMessageBox.Show("Lỗi khi tải thông tin phân quyền AI: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridViewGrants_ShowingEditor(object sender, CancelEventArgs e)
        {
            var row=gridViewGrants.GetFocusedRow() as AiScopeRowViewModel;
            e.Cancel=row==null || !_isReady || _isSaving || !row.IsCapabilityEnabled || gridViewGrants.FocusedColumn!=_simplePermissionColumn;
            if(!e.Cancel && HasInheritedDeny(row)) {
                e.Cancel=true;
                XtraMessageBox.Show("Nhóm đang chặn chức năng này. Hãy chỉnh quyền của nhóm trước.","Phân quyền AI",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }
        private void GridViewGrants_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if(_updatingSimplePermission || e.Column!=_simplePermissionColumn) return;
            var row=gridViewGrants.GetRow(e.RowHandle) as AiScopeRowViewModel;
            if(row==null) return;
            _updatingSimplePermission=true;
            try { SetSimplePermission(row,row.IsAiAllowed);RecalculateRowEffective(row);SetDirty(true);gridViewGrants.RefreshRow(e.RowHandle); }
            finally {_updatingSimplePermission=false;}
        }
        private void RecalculateRowEffective(AiScopeRowViewModel row)
        {
            var directGrants = BuildDirectGrants(row);

            var inheritedGrants = new List<(AiScopeGrantRecord Grant, string GroupName)>();
            if (row.InheritedGrants != null)
            {
                foreach (var inh in row.InheritedGrants)
                {
                    inheritedGrants.Add((new AiScopeGrantRecord
                    {
                        CapabilityCode = inh.CapabilityCode,
                        Effect = inh.Effect,
                        ScopeType = inh.ScopeType,
                        ScopeKey = inh.ScopeKey,
                        ValidFrom = inh.ValidFrom,
                        ValidTo = inh.ValidTo,
                        IsEnabled = true
                    }, inh.GroupName));
                }
            }

            var input = new AiScopePolicyResolver.ResolutionInput
            {
                CapabilityCode = row.CapabilityCode,
                IsCapabilityEnabled = row.IsCapabilityEnabled,
                RequiredFunctionCode = row.RequiredFunctionCode,
                HasRequiredFunctionRight = row.HasRequiredFunctionRight,
                ActorManv = _actorManv,
                DirectGrants = directGrants,
                InheritedGrants = inheritedGrants,
                DepartmentNames = _departmentDict,
                CompanyNames = _companyDict,
                AsOf = DateTime.Now
            };

            var output = AiScopePolicyResolver.Resolve(input);
            row.EffectiveEffect = output.EffectiveEffect;
            row.EffectiveScopeSummary = output.EffectiveScopeSummary;
            row.ExplanationNotes = output.ExplanationNotes;
        }

        private void GridViewGrants_RowStyle(object sender, RowStyleEventArgs e)
        {
            var row = gridViewGrants.GetRow(e.RowHandle) as AiScopeRowViewModel;
            if (row == null) return;

            if (!row.IsCapabilityEnabled)
            {
                e.Appearance.ForeColor = Color.Gray;
                e.Appearance.BackColor = Color.Empty;
            }
            else if (row.EffectiveEffect == "DENY")
            {
                e.Appearance.BackColor = Color.Empty; // Đỏ nhạt cảnh báo DENY
                e.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
            }
            else if (row.EffectiveEffect == "BLOCKED_NO_FUNCTION" || row.EffectiveEffect == "UNMAPPED_EMPLOYEE")
            {
                e.Appearance.BackColor = Color.Empty; // Cam nhạt thiếu quyền nền hoặc MANV
            }
            else if (row.EffectiveEffect == "ALLOW")
            {
                e.Appearance.BackColor = Color.Empty; // Xanh nhạt cho phép
            }
        }

        private async void btnLuu_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;

            // Kiểm tra quyền người thao tác
            if (!UserSession.CanEdit("F_SYSTEM_PQ_CHUCNANG"))
            {
                XtraMessageBox.Show("Từ chối thao tác: Cần quyền sửa Phân quyền chức năng để quản lý quyền AI.", "Lỗi bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _isSaving = true;
                btnLuu.Enabled = false;

                // Hoàn tất và cập nhật editor đang mở trên GridView trước khi thu thập dữ liệu
                gridViewGrants.CloseEditor();
                gridViewGrants.UpdateCurrentRow();

                var request = new SaveAiSubjectScopeGrantsRequest
                {
                    SubjectType = _isGroup ? "GROUP" : "USER",
                    SubjectId = _subjectId,
                    ClientType = "DESKTOP",
                    BaseRevision = _baseRevision
                };

                foreach (var vm in _bindingList)
                {
                    // Nếu capability bị tắt, không gửi thay đổi
                    if (!vm.IsCapabilityEnabled) continue;

                    foreach(var grant in BuildDirectGrants(vm)) request.Grants.Add(new SaveAiSubjectScopeGrantItemDto
                    {
                        CapabilityCode=vm.CapabilityCode,Effect=grant.Effect,ScopeType=grant.ScopeType,
                        ScopeKey=grant.ScopeKey,ValidFrom=grant.ValidFrom,ValidTo=grant.ValidTo
                    });
                }

                var result = await QLyNSu.Functions.FormDataLoader.ReadAsync(this, () => _scopeService.SaveSubjectScopeGrants(request));
                if (result.Success)
                {
                    XtraMessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetDirty(false);
                    await LoadData();
                }
                else if (result.IsConcurrencyConflict)
                {
                    XtraMessageBox.Show(result.Message + "\nBản sửa được giữ lại. Hãy tải lại để đối chiếu trước khi lưu.", "Xung đột phiên bản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    XtraMessageBox.Show(result.Message, "Lỗi lưu cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Có lỗi xảy ra trong quá trình lưu cấu hình: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                btnLuu.Enabled = _isReady;
            }
        }

        private async void btnLamMoi_Click(object sender, EventArgs e)
        {
            if(_isSaving) return;
            if (_isDirty)
            {
                var cf = XtraMessageBox.Show("Dữ liệu thay đổi chưa được lưu. Bạn có chắc chắn muốn tải lại không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (cf != DialogResult.Yes) return;
            }
            await LoadData();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_isDirty && e.CloseReason == CloseReason.UserClosing)
            {
                var cf = XtraMessageBox.Show("Có thay đổi phân quyền chưa được lưu. Bạn có chắc chắn muốn đóng mà không lưu không?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (cf != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
