using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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

        private void FrmAiScopeGrantDetail_Load(object sender, EventArgs e)
        {
            // 1. Kiểm tra quyền Quản trị viên tối cao (Chỉ Admin mới có quyền mở form)
            if (!UserSession.IsAdmin)
            {
                XtraMessageBox.Show("Từ chối truy cập: Chỉ Quản trị viên hệ thống (Admin) mới có quyền quản trị phân quyền tra cứu AI.", "Lỗi bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.Text = _isGroup
                ? $"Quản trị Quyền tra cứu AI - Nhóm quyền: [{_subjectCode}] - {_subjectName}"
                : $"Quản trị Quyền tra cứu AI - Tài khoản: [{_subjectCode}] - {_subjectName}";

            lblSubjectTitle.Text = _isGroup
                ? $"Nhóm quyền: {_subjectCode} ({_subjectName})"
                : $"Tài khoản: {_subjectCode} - {_subjectName}";

            // Ẩn cột kế thừa từ nhóm nếu đối tượng chính là nhóm
            colInherited.Visible = !_isGroup;

            LoadLookups();
            LoadData();

            // Format hàng theo trạng thái
            gridViewGrants.RowStyle += GridViewGrants_RowStyle;
            gridViewGrants.CellValueChanged += GridViewGrants_CellValueChanged;
            gridViewGrants.ShowingEditor += GridViewGrants_ShowingEditor;
        }

        private void LoadLookups()
        {
            try
            {
                var depts = _scopeService.GetDepartmentOptions() ?? new List<KeyValuePair<string, string>>();
                _departmentLookup = depts.Select(d => new LookupItem { Id = d.Key, Name = $"{d.Value} (ID: {d.Key})" }).ToList();
                _departmentDict = depts.ToDictionary(k => k.Key, v => v.Value);

                var comps = _scopeService.GetCompanyOptions() ?? new List<KeyValuePair<string, string>>();
                _companyLookup = comps.Select(c => new LookupItem { Id = c.Key, Name = $"{c.Value} (Mã: {c.Key})" }).ToList();
                _companyDict = comps.ToDictionary(k => k.Key, v => v.Value);

                repoLookUpScopeKey.DataSource = _departmentLookup;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải danh mục phòng ban / công ty: " + ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadData()
        {
            try
            {
                var overview = _scopeService.GetSubjectScopeOverview(_isGroup ? "GROUP" : "USER", _subjectId);
                _baseRevision = overview.CurrentRevision;
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
                    list.Add(vm);
                }

                _bindingList = new BindingList<AiScopeRowViewModel>(list);
                gridControlGrants.DataSource = _bindingList;
                _isDirty = false;
            }
            catch (Exception ex)
            {
                btnLuu.Enabled = false;
                XtraMessageBox.Show("Lỗi khi tải thông tin phân quyền AI: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridViewGrants_ShowingEditor(object sender, CancelEventArgs e)
        {
            var row = gridViewGrants.GetFocusedRow() as AiScopeRowViewModel;
            if (row == null) return;

            // Nếu Capability đang bị tắt ở mức hệ thống -> Không cho phép sửa đổi
            if (!row.IsCapabilityEnabled)
            {
                e.Cancel = true;
                return;
            }

            var focusedCol = gridViewGrants.FocusedColumn;

            // Tùy biến datasource cho repoLookUpScopeKey theo loại phạm vi đang chọn
            if (focusedCol == colDirectScopeKey)
            {
                if (row.DirectScopeType == "DEPARTMENT")
                {
                    repoLookUpScopeKey.DataSource = _departmentLookup;
                    repoLookUpScopeKey.NullText = "-- Chọn phòng ban --";
                }
                else if (row.DirectScopeType == "COMPANY")
                {
                    repoLookUpScopeKey.DataSource = _companyLookup;
                    repoLookUpScopeKey.NullText = "-- Chọn công ty --";
                }
                else
                {
                    // SELF hoặc ALL không cần chọn ScopeKey
                    e.Cancel = true;
                }
            }
        }

        private void GridViewGrants_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var row = gridViewGrants.GetRow(e.RowHandle) as AiScopeRowViewModel;
            if (row == null) return;

            _isDirty = true;

            // Nếu đổi sang SELF hoặc ALL, tự động xóa ScopeKey
            if (e.Column == colDirectScopeType)
            {
                if (row.DirectScopeType == "SELF" || row.DirectScopeType == "ALL")
                {
                    row.DirectScopeKey = null;
                }
            }

            // Tự động tính toán lại Effective và Explanation ngay trên giao diện bằng AiScopePolicyResolver
            RecalculateRowEffective(row);
        }

        private void RecalculateRowEffective(AiScopeRowViewModel row)
        {
            var directGrants = new List<AiScopeGrantRecord>();
            if (row.DirectEffect != "NONE")
            {
                directGrants.Add(new AiScopeGrantRecord
                {
                    CapabilityCode = row.CapabilityCode,
                    Effect = row.DirectEffect,
                    ScopeType = row.DirectScopeType,
                    ScopeKey = row.DirectScopeKey,
                    ValidFrom = row.DirectValidFrom,
                    ValidTo = row.DirectValidTo,
                    IsEnabled = true
                });
            }

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
                e.Appearance.BackColor = Color.FromArgb(245, 245, 245);
            }
            else if (row.EffectiveEffect == "DENY")
            {
                e.Appearance.BackColor = Color.FromArgb(255, 235, 238); // Đỏ nhạt cảnh báo DENY
                e.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
            }
            else if (row.EffectiveEffect == "BLOCKED_NO_FUNCTION" || row.EffectiveEffect == "UNMAPPED_EMPLOYEE")
            {
                e.Appearance.BackColor = Color.FromArgb(255, 243, 224); // Cam nhạt thiếu quyền nền hoặc MANV
            }
            else if (row.EffectiveEffect == "ALLOW")
            {
                e.Appearance.BackColor = Color.FromArgb(232, 245, 233); // Xanh nhạt cho phép
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;

            // Kiểm tra quyền người thao tác
            if (!UserSession.IsAdmin)
            {
                XtraMessageBox.Show("Từ chối thao tác: Chỉ Quản trị viên hệ thống (Admin) mới có quyền cấu hình phân quyền tra cứu AI.", "Lỗi bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    request.Grants.Add(new SaveAiSubjectScopeGrantItemDto
                    {
                        CapabilityCode = vm.CapabilityCode,
                        Effect = vm.DirectEffect,
                        ScopeType = vm.DirectScopeType,
                        ScopeKey = vm.DirectScopeKey,
                        ValidFrom = vm.DirectValidFrom,
                        ValidTo = vm.DirectValidTo
                    });
                }

                var result = _scopeService.SaveSubjectScopeGrants(request);
                if (result.Success)
                {
                    XtraMessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _isDirty = false;
                    LoadData();
                }
                else if (result.IsConcurrencyConflict)
                {
                    XtraMessageBox.Show(result.Message, "Xung đột phiên bản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData();
                }
                else
                {
                    XtraMessageBox.Show(result.Message, "Lỗi lưu cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Có lỗi xảy ra trong quá trình lưu cấu hình: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                btnLuu.Enabled = true;
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            if (_isDirty)
            {
                var cf = XtraMessageBox.Show("Dữ liệu thay đổi chưa được lưu. Bạn có chắc chắn muốn tải lại không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (cf != DialogResult.Yes) return;
            }
            LoadData();
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
