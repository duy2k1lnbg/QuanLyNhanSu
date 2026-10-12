using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using QLyNSu.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public sealed class FrmPayrollDataEditor : XtraForm
    {
        private readonly int _period;
        private readonly decimal? _employee;
        private readonly DateTime? _day;
        private readonly ComboBoxEdit _sections = new ComboBoxEdit { Dock = DockStyle.Fill };
        private readonly GridControl _grid = new GridControl { Dock = DockStyle.Fill };
        private readonly GridView _view;
        private readonly TableLayoutPanel _fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(6), BackColor = Color.White };
        private readonly MemoEdit _reason = new MemoEdit { Dock = DockStyle.Fill };
        private readonly SimpleButton _save = new SimpleButton { Text = "Lưu dữ liệu", Width = 128, Height = 36 };
        private readonly SimpleButton _approve = new SimpleButton { Text = "Phê duyệt", Width = 130, Height = 36 };
        private readonly SimpleButton _reject = new SimpleButton { Text = "Từ chối", Width = 110, Height = 36 };
        private readonly SimpleButton _add = new SimpleButton { Text = "Thêm bản mới", Width = 128, Height = 36 };
        private readonly SimpleButton _edit = new SimpleButton { Text = "Sửa dòng đã chọn", Width = 142, Height = 36 };
        private readonly SimpleButton _preview = new SimpleButton { Text = "Kiểm tra dữ liệu", Width = 140, Height = 36 };
        private readonly LabelControl _notice = new LabelControl { Dock = DockStyle.Top, AutoSizeMode = LabelAutoSizeMode.None, Padding = new Padding(12, 8, 12, 8) };
        private readonly LabelControl _feedback = new LabelControl { Dock = DockStyle.Top, AutoSizeMode = LabelAutoSizeMode.None, Padding = new Padding(12, 8, 12, 8), Visible = false };
        private readonly GroupControl _listGroup = new GroupControl { Text = "Danh sách", Dock = DockStyle.Fill, Padding = new Padding(8) };
        private readonly GroupControl _editorGroup = new GroupControl { Text = "Thông tin hồ sơ", Dock = DockStyle.Fill, Padding = new Padding(8) };
        private readonly SplitContainer _split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, Size = new Size(1100, 570), Panel1MinSize = 280, Panel2MinSize = 440, SplitterWidth = 8 };
        private readonly Panel _scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        private readonly Dictionary<string, BaseEdit> _editors = new Dictionary<string, BaseEdit>();
        private readonly Dictionary<string, string> _employeeNames = new Dictionary<string, string>();
        private readonly List<TableLayoutPanel> _cards = new List<TableLayoutPanel>();
        private PayrollDataPage _page;
        private Dictionary<string, object> _original;
        private bool _loading, _dirty, _busy;
        private int _selectedRow = -1;
        private sealed class Option { public object Value; public string Caption; public override string ToString() => TranslationManager.Translate(Caption); }

        public FrmPayrollDataEditor(int period, string initialTable, decimal? employee = null, DateTime? day = null)
        {
            _period = period; _employee = employee; _day = day;
            Text = TranslationManager.Translate("Chuẩn bị dữ liệu tính lương") + " - " + period;
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1180, 800); MinimumSize = new Size(900, 680);
            Padding = new Padding(12); StartPosition = FormStartPosition.CenterParent;
            _sections.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            foreach (var section in PayrollPreparationDataService.Sections.Where(s => UserSession.CanView(s.Capability))) _sections.Properties.Items.Add(new Option { Value = section, Caption = section.Title });
            _sections.SelectedItem = _sections.Properties.Items.Cast<Option>().FirstOrDefault(s => ((PayrollDataSection)s.Value).Table == initialTable);
            var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = 54, ColumnCount = 2, Padding = new Padding(0, 4, 0, 10) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            var title = new LabelControl { Text = TranslationManager.Translate("Kỳ công") + ": " + (period % 100).ToString("00") + "/" + (period / 100), Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None };
            title.Appearance.Font = new Font(Font, FontStyle.Bold); title.Appearance.Options.UseFont = true;
            title.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            header.Controls.Add(title, 0, 0); header.Controls.Add(_sections, 1, 0);

            _view = new GridView(_grid); _grid.MainView = _view;
            _view.OptionsBehavior.Editable = false; _view.OptionsView.ShowGroupPanel = false;
            _view.OptionsView.ShowAutoFilterRow = false; _view.OptionsView.RowAutoHeight = true;
            _view.OptionsView.ColumnAutoWidth = true; _view.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            _view.OptionsSelection.EnableAppearanceFocusedCell = false;
            _view.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            _view.Appearance.Row.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            _view.Appearance.Row.Options.UseTextOptions = true;
            DesktopTypography.Grid(_view);
            _view.CustomColumnDisplayText += DisplayCell;
            _view.FocusedRowChanged += (s, e) => {
                if (_loading || _page == null) return;
                if (!Discard()) { _loading = true; try { _view.FocusedRowHandle = _selectedRow; } finally { _loading = false; } return; }
                BindSelected();
            };
            _view.DoubleClick += (s, e) => { if (Discard()) BindSelected(); };
            _listGroup.Controls.Add(_grid); _scroll.Controls.Add(_fields); _editorGroup.Controls.Add(_scroll);
            _split.Panel1.Controls.Add(_listGroup); _split.Panel2.Controls.Add(_editorGroup);
            _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(0, 10, 0, 0) };
            var refresh = new SimpleButton { Text = "Tải lại", Width = 94, Height = 36 };
            var close = new SimpleButton { Text = "Đóng", Width = 88, Height = 36 };
            footer.Controls.AddRange(new Control[] { _save, _preview, _add, _approve, _reject, refresh, close });
            _save.Appearance.Font = new Font(Font, FontStyle.Bold); _save.Appearance.Options.UseFont = true;
            Controls.Add(_split); Controls.Add(_feedback); Controls.Add(_notice); Controls.Add(header); Controls.Add(footer);
            _notice.Appearance.TextOptions.WordWrap = _feedback.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            _notice.Appearance.TextOptions.VAlignment = _feedback.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            _notice.Appearance.BackColor = Color.FromArgb(235, 243, 253); _notice.Appearance.Options.UseBackColor = true;
            _notice.Height = 66;
            _reason.Properties.NullValuePrompt = TranslationManager.Translate("Nhập căn cứ hoặc lý do thay đổi...");
            _reason.EditValueChanged += (s, e) => { if (!_loading) _dirty = true; };
            Load += async (s, e) => await Guard(LoadSection);
            _sections.SelectedIndexChanged += async (s, e) => {
                if (_loading) return;
                if (!Discard()) { _loading = true; try { _sections.SelectedItem = _sections.Properties.Items.Cast<Option>().FirstOrDefault(o => o.Value == _page?.Section); } finally { _loading = false; } return; }
                await Guard(LoadSection);
            };
            _add.Click += (s, e) => { if (Discard()) Bind(null); };
            _edit.Click += (s, e) => { if (Discard()) BindSelected(); };
            refresh.Click += async (s, e) => { if (Discard()) await Guard(LoadSection); };
            close.Click += (s, e) => Close();
            _preview.Click += async (s, e) => await Guard(() => Save(true), "kiểm tra dữ liệu");
            _save.Click += async (s, e) => await Guard(() => Save(), "lưu dữ liệu");
            _approve.Click += async (s, e) => await Guard(() => Decide(true), "phê duyệt yêu cầu");
            _reject.Click += async (s, e) => await Guard(() => Decide(false), "từ chối yêu cầu");
            FormClosing += (s, e) => { if (!Discard()) e.Cancel = true; };
            _scroll.SizeChanged += (s, e) => FitFields();
            _split.SizeChanged += (s, e) => { if (_split.Width >= 760) _split.SplitterDistance = Math.Max(280, Math.Min(_split.Width - 448, (int)(_split.Width * .40))); };
            _grid.SizeChanged += (s, e) => ConfigureColumns();
            TranslationManager.Translate(this); DesktopTypography.Apply(this);
        }

        private bool Discard() => !_dirty || XtraMessageBox.Show(this, TranslationManager.Translate("Có thay đổi chưa lưu. Bỏ thay đổi để tiếp tục?"), TranslationManager.Translate("Dữ liệu chưa lưu"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private async Task Guard(Func<Task> action, string context = "tải dữ liệu")
        {
            if (_busy) return;
            _busy = true; _feedback.Visible = false;
            try { await action(); }
            catch (OperationCanceledException) { }
            catch (ArgumentException e) { ShowValidation(e); }
            catch (BusinessException e) { Feedback(e.Message, false); }
            catch (Exception e) { XtraMessageBox.Show(this, ErrorHelper.ResolveUserFriendlyMessage(e, context), TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { _busy = false; }
        }

        private void Feedback(string message, bool success)
        {
            _feedback.Text = TranslationManager.Translate(message);
            _feedback.Appearance.BackColor = success ? Color.FromArgb(234, 247, 237) : Color.FromArgb(255, 239, 238);
            _feedback.Appearance.ForeColor = success ? Color.FromArgb(30, 95, 48) : Color.FromArgb(159, 39, 39);
            _feedback.Appearance.Options.UseBackColor = _feedback.Appearance.Options.UseForeColor = true;
            _feedback.Height = Math.Max(44, TextRenderer.MeasureText(_feedback.Text, Font, new Size(Math.Max(300, ClientSize.Width - 48), int.MaxValue), TextFormatFlags.WordBreak).Height + 20);
            _feedback.Visible = true;
        }

        private void ShowValidation(ArgumentException error)
        {
            // ParamName targets the editor; exclude the framework's technical parameter suffix.
            var message = error.Message;
            var suffix = message.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (!string.IsNullOrEmpty(error.ParamName) && suffix >= 0) message = message.Substring(0, suffix);
            Feedback(message, false);
            BaseEdit editor;
            if (error.ParamName == "LY_DO_LUU") editor = _reason;
            else _editors.TryGetValue(error.ParamName ?? "", out editor);
            if (editor != null) { _scroll.ScrollControlIntoView(editor); editor.Focus(); }
        }

        private async Task LoadSection()
        {
            var section = (_sections.SelectedItem as Option)?.Value as PayrollDataSection;
            if (section == null) return;
            var data = await FormDataLoader.ReadAsync(this, () => {
                var service = new PayrollPreparationDataService();
                var page = service.Load(section.Table, _period, _employee);
                return new { Page = page, Lookups = page.Fields.ToDictionary(f => f.Name, f => service.Lookup(f.Name)) };
            });
            ApplyPage(data.Page, data.Lookups);
        }

        private void ApplyPage(PayrollDataPage page, Dictionary<string, DataTable> lookups)
        {
            _loading = true;
            try {
                _page = page; _employeeNames.Clear();
                DataTable employeeLookup;
                if (lookups.TryGetValue("MANV", out employeeLookup) && employeeLookup != null)
                    foreach (DataRow employee in employeeLookup.Rows) _employeeNames[Convert.ToString(employee["ID"], CultureInfo.InvariantCulture)] = Convert.ToString(employee["LABEL"]);
                _grid.DataSource = page.Rows; _view.PopulateColumns();
                foreach (DevExpress.XtraEditors.Repository.RepositoryItem old in _grid.RepositoryItems.Cast<DevExpress.XtraEditors.Repository.RepositoryItem>().ToArray()) old.Dispose();
                _grid.RepositoryItems.Clear();
                foreach (DevExpress.XtraGrid.Columns.GridColumn column in _view.Columns) {
                    column.Caption = Caption(column.FieldName);
                    DataTable lookupData;
                    if (lookups.TryGetValue(column.FieldName, out lookupData) && lookupData != null) {
                        DevExpress.XtraEditors.Repository.RepositoryItem lookup = column.FieldName == "MANV"
                            ? (DevExpress.XtraEditors.Repository.RepositoryItem)new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit { WordWrap = true }
                            : new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit { DataSource = lookupData, DisplayMember = "LABEL", ValueMember = "ID", NullText = "" };
                        _grid.RepositoryItems.Add(lookup); column.ColumnEdit = lookup;
                    }
                    if (page.Fields.Any(f => f.Name == column.FieldName && f.Type == "DATE")) { column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime; column.DisplayFormat.FormatString = "dd/MM/yyyy"; }
                }
                ConfigureColumns();
                _reason.Parent?.Controls.Remove(_reason);
                foreach (Control old in _fields.Controls.Cast<Control>().ToArray()) old.Dispose();
                _fields.Controls.Clear(); _fields.RowStyles.Clear(); _fields.RowCount = 0; _editors.Clear(); _cards.Clear();
                foreach (var field in page.Fields) {
                    if (page.Section.IsRequest && new[] { "THOIDIEM_VAO_MOI", "THOIDIEM_RA_MOI", "BATDAU_DANGKY", "KETTHUC_DANGKY", "BATDAU_NGHI", "KETTHUC_NGHI", "GIAY_NGHI", "NGUON_CHI_TRA", "LOAI_HUONG_CONG" }.Contains(field.Name)) field.Required = true;
                    BaseEdit editor; DataTable lookupData;
                    if (lookups.TryGetValue(field.Name, out lookupData) && lookupData != null) {
                        var lookup = new LookUpEdit { Dock = DockStyle.Fill };
                        lookup.Properties.DataSource = lookupData; lookup.Properties.DisplayMember = "LABEL"; lookup.Properties.ValueMember = "ID";
                        lookup.Properties.NullText = TranslationManager.Translate("Chọn...");
                        lookup.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("ID") { Visible = false });
                        lookup.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("LABEL", TranslationManager.Translate("Lựa chọn")));
                        editor = lookup;
                    } else if (field.Type == "DATE") {
                        var date = new DateEdit { Dock = DockStyle.Fill };
                        date.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                        date.Properties.Mask.EditMask = IsDateTime(field.Name) ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy";
                        date.Properties.Mask.UseMaskAsDisplayFormat = true;
                        date.Properties.VistaEditTime = IsDateTime(field.Name) ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.False;
                        editor = date;
                    } else if (Options(field.Name, page.Section) != null) {
                        var combo = new ComboBoxEdit { Dock = DockStyle.Fill };
                        combo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                        combo.Properties.Items.AddRange(Options(field.Name, page.Section)); editor = combo;
                    } else editor = new TextEdit { Dock = DockStyle.Fill };
                    editor.Properties.Appearance.Font = Font; editor.Properties.Appearance.Options.UseFont = true;
                    editor.EditValueChanged += (s, e) => { if (!_loading) _dirty = true; };
                    _editors[field.Name] = editor;
                    AddField(Caption(field.Name) + (field.Required ? " *" : ""), editor);
                }
                AddReason();
                _notice.Text = TranslationManager.Translate(page.Section.IsAnomaly ? "Chọn bất thường để xem căn cứ. Chỉ xác nhận Không tính đối với khoảng ngoài ca/tăng ca chưa duyệt; các lỗi khác cần sửa dữ liệu nguồn rồi công bố lại." : page.Section.IsAttendanceConfiguration ? "Giữ nguyên phiên bản đã công bố. Tạo bản Nháp, nhập khung giờ rồi Công bố. Khoảng qua đêm dùng phút lớn hơn 1440." : page.Section.IsRequest ? "Đơn mới chờ duyệt. Nhập căn cứ trước khi duyệt hoặc từ chối; không tự duyệt đơn của mình. Sau đó công bố lại công." : page.Section.IsSchedule ? "Sửa phân ca tạo phiên bản thay thế và giữ lịch cũ. Sau khi lưu cần công bố lại cả kỳ." : PayrollPreparationDataService.IsReusableEmployeeProfile(page.Section) || page.Section.Table=="TB_NGUOI_PHU_THUOC" || page.Section.Table=="TB_DANG_KY_GIAM_TRU_NPT" ? "Hồ sơ dùng chung toàn hệ thống, không nhập lại mỗi tháng. Để trống ngày hết hiệu lực nếu chưa hết hạn. Sửa hồ sơ của kỳ còn mở cần tính lại lương; kỳ đã chốt được bảo vệ. Dấu * là bắt buộc." : "Dấu * là bắt buộc. Hồ sơ đã dùng tính lương được bảo vệ; thêm phiên bản mới khi cần thay đổi. Tỷ lệ 0,08 tương ứng 8%.");
                _listGroup.Text = TranslationManager.Translate("Danh sách") + " (" + page.Rows.Rows.Count + ")";
                _view.FocusedRowHandle = page.Rows.Rows.Count > 0 ? 0 : -1;
                if (page.Rows.Rows.Count > 0) BindSelected(); else Bind(null);
                _scroll.AutoScrollPosition = Point.Empty; FitFields();
            } finally { _loading = false; }
        }

        private void ConfigureColumns()
        {
            if (_page == null) return;
            string[] preferred = _page.Section.IsAnomaly ? new[] { "MANV", "NGAY", "MA_LOI" }
                : _page.Section.IsRequest ? new[] { "MANV", "NGAY", "TUNGAY", "TRANGTHAI" }
                : new[] { "MANV", "HO_TEN", "TEN_CHINH_SACH", "TEN_PHIENBAN", "NGAY_HIEU_LUC", "NGAY", "TRANG_THAI_HO_SO", "TRANG_THAI" };
            var visible = preferred.Where(f => _view.Columns.ColumnByFieldName(f) != null).Take(_grid.Width < 460 ? 3 : 4).ToList();
            if (visible.Count == 0) visible = _page.Fields.Select(f => f.Name).Take(3).ToList();
            foreach (DevExpress.XtraGrid.Columns.GridColumn column in _view.Columns) {
                column.Visible = visible.Contains(column.FieldName);
                column.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
                column.AppearanceCell.Options.UseTextOptions = true;
                if (visible.Contains(column.FieldName) && column.ColumnEdit == null && _page.Rows.Columns[column.FieldName].DataType == typeof(string)) {
                    var memo = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit { WordWrap = true };
                    _grid.RepositoryItems.Add(memo); column.ColumnEdit = memo;
                }
                column.MinWidth = column.FieldName == "MANV" || column.FieldName.StartsWith("TEN_") ? 125 : 80;
                column.Width = column.FieldName == "MANV" || column.FieldName.StartsWith("TEN_") ? 165 : 105;
            }
        }

        private void DisplayCell(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs args)
        {
            if (args.Value == null || args.Value == DBNull.Value || _page == null) return;
            if (args.Column.FieldName == "MANV") {
                string name; if (_employeeNames.TryGetValue(Convert.ToString(args.Value, CultureInfo.InvariantCulture), out name)) args.DisplayText = name;
                return;
            }
            var options = Options(args.Column.FieldName, _page.Section);
            var option = options?.FirstOrDefault(o => Convert.ToString(o.Value, CultureInfo.InvariantCulture) == Convert.ToString(args.Value, CultureInfo.InvariantCulture));
            if (option != null) args.DisplayText = option.ToString();
        }

        private void AddField(string caption, Control editor)
        {
            int index = _cards.Count, row = index / 2;
            if (index % 2 == 0) { _fields.RowCount++; _fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 74)); }
            var card = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Margin = new Padding(6, 3, 6, 5) };
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var label = new LabelControl { Text = caption, Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None };
            label.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            label.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
            label.Appearance.Font = Font; label.Appearance.Options.UseFont = true;
            editor.Margin = new Padding(0, 4, 0, 2);
            card.Controls.Add(label, 0, 0); card.Controls.Add(editor, 0, 1);
            _fields.Controls.Add(card, index % 2, row); _cards.Add(card);
        }

        private void AddReason()
        {
            int row = _fields.RowCount++; _fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            var card = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Margin = new Padding(6, 8, 6, 5) };
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.Controls.Add(new LabelControl { Text = TranslationManager.Translate("Căn cứ / lý do lưu *"), Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None }, 0, 0);
            card.Controls.Add(_reason, 0, 1); _fields.Controls.Add(card, 0, row); _fields.SetColumnSpan(card, 2);
        }

        private void FitFields()
        {
            if (_cards.Count == 0 || _scroll.ClientSize.Width < 100) return;
            int width = Math.Max(160, (_scroll.ClientSize.Width - 48) / 2);
            for (int row = 0; row < (_cards.Count + 1) / 2; row++) {
                int height = 24;
                foreach (var card in _cards.Skip(row * 2).Take(2)) {
                    var label = card.GetControlFromPosition(0, 0);
                    height = Math.Max(height, TextRenderer.MeasureText(label.Text, Font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + 4);
                }
                foreach (var card in _cards.Skip(row * 2).Take(2)) card.RowStyles[0].Height = height;
                _fields.RowStyles[row].Height = height + 48;
            }
        }

        private void BindSelected()
        {
            var row = _view.GetFocusedDataRow(); if (row == null) return;
            _selectedRow = _view.FocusedRowHandle;
            Bind(row.Table.Columns.Cast<DataColumn>().ToDictionary(c => c.ColumnName, c => row[c]));
        }

        private void Bind(Dictionary<string, object> row)
        {
            if (_page == null) return;
            bool wasLoading = _loading; _loading = true;
            try {
                _original = row; _feedback.Visible = false;
                foreach (var field in _page.Fields) {
                    object value = row == null ? null : row[field.Name];
                    if (row == null && field.Name == "MANV" && _employee.HasValue) value = _employee.Value;
                    if (row == null && new[] { "NGAY", "TUNGAY", "DENNGAY" }.Contains(field.Name)) value = _day ?? new DateTime(_period / 100, _period % 100, 1);
                    if (row == null && field.Name == "TAX_YEAR") value = _period / 100;
                    var editor = _editors[field.Name]; var combo = editor as ComboBoxEdit;
                    if (combo != null) combo.SelectedItem = combo.Properties.Items.Cast<Option>().FirstOrDefault(o => value != null && value != DBNull.Value && Convert.ToString(o.Value, CultureInfo.InvariantCulture) == Convert.ToString(value, CultureInfo.InvariantCulture));
                    else editor.EditValue = value == DBNull.Value ? null : value;
                    editor.ReadOnly = _page.Section.IsAnomaly;
                }
                _reason.EditValue = null; _dirty = false;
                bool pending = row != null && _page.Section.IsRequest && Convert.ToString(row["TRANGTHAI"]) == "PENDING";
                _approve.Visible = _page.Section.IsRequest || _page.Section.IsAnomaly; _reject.Visible = _page.Section.IsRequest;
                _approve.Text = TranslationManager.Translate(_page.Section.IsAnomaly ? "Xác nhận không tính" : "Phê duyệt");
                _approve.Enabled = _reject.Enabled = pending && UserSession.CanEdit(_page.Section.Capability);
                _save.Enabled = !_page.Section.IsAnomaly && (row == null ? UserSession.CanAdd(_page.Section.Capability) : UserSession.CanEdit(_page.Section.Capability) && (!_page.Section.IsRequest || pending));
                if (_page.Section.IsAnomaly) _approve.Enabled = row != null && AttendancePreparationReviewService.CanExclude(Convert.ToString(row["MA_LOI"])) && UserSession.CanEdit("F_NV_PHEDUYET");
                foreach (var editor in _editors.Values) editor.ReadOnly = !_save.Enabled;
                _add.Enabled = !_page.Section.IsAnomaly && UserSession.CanAdd(_page.Section.Capability);
                _edit.Enabled = row != null; _preview.Enabled = _save.Enabled;
                _editorGroup.Text = TranslationManager.Translate(row == null ? "Thêm bản mới" : "Thông tin hồ sơ") + (row == null ? "" : " #" + Convert.ToString(row[PayrollPreparationDataService.Key(_page.Section)]));
            } finally { _loading = wasLoading; }
        }

        private Dictionary<string, object> ReadValues()
        {
            var values = new Dictionary<string, object>();
            foreach (var field in _page.Fields) {
                var editor = _editors[field.Name];
                object value = (editor as ComboBoxEdit)?.SelectedItem is Option option ? option.Value : editor.EditValue;
                if (value == null || string.IsNullOrWhiteSpace(Convert.ToString(value))) {
                    if (field.Required) throw new ArgumentException("Cần nhập " + Caption(field.Name) + ".", field.Name);
                    value = DBNull.Value;
                } else if (field.Type == "NUMBER" || field.Type == "FLOAT") {
                    if (!decimal.TryParse(Convert.ToString(value), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal number)) throw new ArgumentException(Caption(field.Name) + ": cần nhập số hợp lệ.", field.Name);
                    value = number;
                } else if (field.Type == "DATE") value = Convert.ToDateTime(value);
                else {
                    value = Convert.ToString(value).Trim();
                    if (field.Length > 0 && ((string)value).Length > field.Length) throw new ArgumentException(Caption(field.Name) + ": tối đa " + field.Length + " ký tự.", field.Name);
                }
                values[field.Name] = value;
            }
            return values;
        }

        private async Task Save(bool validateOnly = false)
        {
            if (_page == null) return;
            var values = ReadValues(); var original = _original; var table = _page.Section.Table; var reason = _reason.Text.Trim();
            // Validate locally before any database work so missing identity is focused immediately.
            PayrollPreparationDataService.Validate(_page.Section, values);
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Nhập căn cứ/lý do lưu dữ liệu.", "LY_DO_LUU");
            var result = await FormDataLoader.ReadAsync(this, () => new PayrollPreparationDataService().TrySave(table, values, original, _period, reason, validateOnly));
            if (!result.Success) {
                if (!string.IsNullOrEmpty(result.TargetField)) ShowValidation(new ArgumentException(result.Message, result.TargetField));
                else Feedback(result.Message, false);
                return;
            }
            if (validateOnly) { Feedback("Dữ liệu hợp lệ. Chưa lưu thay đổi; bấm Lưu dữ liệu để áp dụng.", true); return; }
            _dirty = false; await LoadSection();
            if(result.RecalculatePeriods.Length>0) Feedback(TranslationManager.Translate("Đã lưu hồ sơ dùng chung. Cần tính lại lương các kỳ:")+" "+string.Join(", ",result.RecalculatePeriods.Select(p=>(p%100).ToString("00")+"/"+(p/100)))+".", true);
            else Feedback("Đã lưu dữ liệu. Quay lại quy trình và bấm Kiểm tra lại.", true);
        }

        private async Task Decide(bool approve)
        {
            var row = _view.GetFocusedDataRow(); if (row == null) return;
            var table = _page.Section.Table; var id = Convert.ToDecimal(row[PayrollPreparationDataService.Key(_page.Section)]); var reason = _reason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Nhập căn cứ phê duyệt/từ chối vào ô lý do.", "LY_DO_LUU");
            if (XtraMessageBox.Show(this, TranslationManager.Translate(approve ? "Phê duyệt" : "Từ chối") + " #" + id + "?", TranslationManager.Translate("Xác nhận xử lý đơn"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            if (_page.Section.IsAnomaly) {
                if (XtraMessageBox.Show(this, TranslationManager.Translate("Bạn xác nhận khoảng ngoài ca này không được tính công/tăng ca, đã đối chiếu căn cứ và cần giữ nguyên dữ liệu quẹt thẻ? Quyết định được ghi lịch sử và chỉ áp dụng khi nguồn khớp."), TranslationManager.Translate("Xác minh khoảng không tính công"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                await FormDataLoader.RunAsync(this, () => new AttendancePreparationReviewService().ExcludeConfirmedInterval(id, _period, reason));
            } else await FormDataLoader.RunAsync(this, () => new PayrollPreparationDataService().DecideRequest(table, id, _period, approve, reason));
            _dirty = false; await LoadSection();
        }

        private static bool IsDateTime(string field)=>field.StartsWith("BATDAU_") || field.StartsWith("KETTHUC_") || field.StartsWith("THOIDIEM_");
        private static Option[] Options(string field,PayrollDataSection section)
        {
            if(PayrollPreparationDataService.IsFlag(field))return new[]{new Option {Value=1,Caption="Có"},new Option {Value=0,Caption="Không"}};
            if(field=="TRANG_THAI" && section.IsAttendanceConfiguration)return new[]{new Option{Value="DRAFT",Caption="Nháp"},new Option{Value="PUBLISHED",Caption="Công bố"},new Option{Value="RETIRED",Caption="Ngừng áp dụng"}};
            if(field=="LOAI_KHUNGGIO")return new[]{new Option{Value="LAM_VIEC",Caption="Làm việc"},new Option{Value="NGHI_HUONG_LUONG",Caption="Nghỉ hưởng lương"},new Option{Value="NGHI_KHONG_LUONG",Caption="Nghỉ không lương"}};
            if(field=="LOAICONG")return new[]{new Option{Value="THUONG",Caption="Ngày thường"},new Option{Value="NGHI_TUAN",Caption="Nghỉ tuần"},new Option{Value="LE",Caption="Ngày lễ"}};
            if(field=="KIEU_LAM_TRON")return new[]{new Option{Value="EXACT",Caption="Thời gian thực (không làm tròn)"}};
            if(field=="CACH_HUONG_DUNG_SAI")return new[]{new Option{Value="KHONG_BU",Caption="Không bù công"},new Option{Value="DU_CONG_NEU_TRONG_NGUONG",Caption="Đủ công trong ngưỡng"}};
            if(field=="CACH_DEM_MUON")return new[]{new Option{Value="TOAN_BO_NEU_VUOT",Caption="Tính toàn bộ nếu vượt ngưỡng"},new Option{Value="PHAN_VUOT_NGUONG",Caption="Chỉ phần vượt ngưỡng"}};
            if(field=="TRANG_THAI")return new[]{new Option{Value="ACTIVE",Caption="Đang áp dụng"},new Option{Value="INACTIVE",Caption="Ngừng áp dụng"}};
            if(field=="PERIOD_TYPE")return new[]{new Option{Value="MONTH",Caption="Tháng"},new Option{Value="YEAR",Caption="Năm"}};
            if(field=="TRANG_THAI_PHAN_CONG")return new[]{new Option{Value="LAM_VIEC",Caption="Làm việc"},new Option{Value="NGHI",Caption="Ngày nghỉ"}};
            if(field=="LOAI_NGAY")return new[]{new Option{Value="THUONG",Caption="Ngày thường"},new Option{Value="NGHI_TUAN",Caption="Nghỉ tuần"},new Option{Value="LE",Caption="Ngày lễ"}};
            if(field=="NGUON_CHI_TRA")return new[]{new Option{Value="CONG_TY",Caption="Công ty"},new Option{Value="BHXH",Caption="BHXH"},new Option{Value="KHONG_CHI_TRA",Caption="Không chi trả"}};
            if(field=="LOAI_HUONG_CONG")return new[]{new Option{Value="PHEP_NAM",Caption="Phép năm"},new Option{Value="NGHI_HUONG_LUONG",Caption="Nghỉ hưởng lương"},new Option{Value="NGHI_BHXH",Caption="Nghỉ hưởng BHXH"},new Option{Value="KHONG_LUONG",Caption="Nghỉ không lương"}};
            if(field=="LOAI_DINH_DANH")return new[]{new Option{Value="CCCD",Caption="Căn cước công dân"},new Option{Value="HO_CHIEU",Caption="Hộ chiếu"},new Option{Value="MST",Caption="Mã số thuế"},new Option{Value="KHAC",Caption="Khác"}};
            if(field=="CAN_CU_CU_TRU")return new[]{new Option{Value="CONG_DAN_VN_THUONG_TRU",Caption="Công dân VN thường trú"},new Option{Value="O_VIET_NAM_TREN_183_NGAY",Caption="Ở VN từ 183 ngày trở lên"},new Option{Value="HOP_DONG_THUE_NHA_TREN_183_NGAY",Caption="Hợp đồng thuê nhà từ 183 ngày"},new Option{Value="CHUA_XAC_MINH",Caption="Chưa xác minh"}};
            if(field=="TRANG_THAI_HO_SO")return new[]{new Option{Value="DA_XAC_MINH",Caption="Đã xác minh"},new Option{Value="CHO_XAC_MINH",Caption="Chờ xác minh"},new Option{Value="THIEU_DINH_DANH",Caption="Thiếu định danh"},new Option{Value="THIEU_CAN_CU",Caption="Thiếu căn cứ cư trú"}};
            if(field=="LOAI_GIAY_TO")return new[]{new Option{Value="GIAY_KHAI_SINH",Caption="Giấy khai sinh"},new Option{Value="CCCD",Caption="Căn cước công dân"},new Option{Value="HO_CHIEU",Caption="Hộ chiếu"},new Option{Value="DINH_DANH_CA_NHAN",Caption="Số định danh cá nhân"},new Option{Value="CHUA_XAC_MINH",Caption="Chưa xác minh"}};
            if(field=="CAN_CU_GIAM_TRU")return new[]{new Option{Value="CON_DUOI_18_TUOI",Caption="Con dưới 18 tuổi"},new Option{Value="CON_TREN_18_KHUYET_TAT",Caption="Con khuyết tật không nơi nương tựa"},new Option{Value="CHA_ME_HET_TUOI_LAO_DONG",Caption="Cha/mẹ hết tuổi lao động"},new Option{Value="NGUOI_PHU_THUOC_KHAC",Caption="Người phụ thuộc khác"}};
            if(field=="TRANG_THAI_DANG_KY")return new[]{new Option{Value="DA_DANG_KY_CO_QUAN_THUE",Caption="Đã đăng ký cơ quan thuế"},new Option{Value="CHO_XAC_MINH",Caption="Chờ xác minh"},new Option{Value="CHUA_NOP_HO_SO",Caption="Chưa nộp hồ sơ"},new Option{Value="NGUNG_GIAM_TRU",Caption="Ngừng giảm trừ"}};
            if(field=="TINH_TRANG_HO_SO_CHUNG_MINH")return new[]{new Option{Value="DA_NOP_DU",Caption="Đã nộp đủ hồ sơ"},new Option{Value="CHUA_NOP",Caption="Chưa nộp hồ sơ"},new Option{Value="HET_HAN_NOP",Caption="Quá hạn nộp hồ sơ"},new Option{Value="BO_SUNG",Caption="Cần bổ sung hồ sơ"}};
            return null;
        }
        private static readonly Dictionary<string,string> Labels=new Dictionary<string,string> {
            {"ID","Mã bản ghi"},{"IDLICH","Mã lịch"},{"MANV","Nhân viên"},{"MA_CHINH_SACH","Mã chính sách"},{"TEN_CHINH_SACH","Tên chính sách"},
            {"NGAY_HIEU_LUC","Ngày hiệu lực"},{"NGAY_HET_HIEU_LUC","Ngày hết hiệu lực"},{"NGAY_BAT_DAU","Ngày bắt đầu"},{"NGAY_KET_THUC","Ngày kết thúc"},{"NGAY_GIA_NHAP","Ngày bắt đầu hồ sơ"},
            {"SO_CONG_CHUAN_THANG","Ngày công chuẩn / tháng"},{"SO_GIO_CHUAN_NGAY","Giờ công chuẩn / ngày"},{"HE_SO_LAM_DEM","Tỷ lệ cộng thêm ca đêm"},{"TRANG_THAI","Trạng thái"},{"MUC_THAM_CHIEU","Mức tham chiếu (đ)"},
            {"TY_LE_BHXH_NLD","BHXH người lao động"},{"TY_LE_BHYT_NLD","BHYT người lao động"},{"TY_LE_BHTN_NLD","BHTN người lao động"},{"TY_LE_BHXH_NSDLD","BHXH doanh nghiệp"},{"TY_LE_BHYT_NSDLD","BHYT doanh nghiệp"},{"TY_LE_BHTN_NSDLD","BHTN doanh nghiệp"},
            {"TY_LE_TNLD_BNN_NSDLD","TNLĐ/BNN doanh nghiệp"},{"TY_LE_TNLD_BNN_UU_DAI","Tỷ lệ TNLĐ được giảm"},{"AP_DUNG_TRAN_BHXH_BHYT","Áp dụng trần BHXH/BHYT"},{"AP_DUNG_TRAN_BHTN","Áp dụng trần BHTN"},
            {"POLICY_BHXH_ID","Chính sách BHXH"},{"VUNG_LUONG","Vùng lương (1–4)"},{"LUONG_TOI_THIEU_THANG","Lương tối thiểu tháng (đ)"},{"LUONG_TOI_THIEU_GIO","Lương tối thiểu giờ (đ)"},{"HE_SO_SAN_DOANH_NGHIEP","Hệ số sàn doanh nghiệp"},
            {"TY_LE_DOAN_PHI_NLD","Tỷ lệ đoàn phí"},{"CAP_PERCENT_STATUTORY_BASE_SALARY","Trần đoàn phí / mức tham chiếu"},{"KINH_PHI_CONG_DOAN_NSDLD","Tỷ lệ kinh phí công đoàn"},
            {"TAX_YEAR","Năm thuế"},{"GIAM_TRU_BAN_THAN_THANG","Giảm trừ bản thân tháng (đ)"},{"GIAM_TRU_PHU_THUOC_THANG","Giảm trừ mỗi người phụ thuộc tháng (đ)"},{"GIAM_TRU_BAN_THAN_NAM","Giảm trừ bản thân năm (đ)"},{"GIAM_TRU_PHU_THUOC_NAM","Giảm trừ người phụ thuộc năm (đ)"},
            {"POLICY_THUE_ID","Chính sách thuế"},{"PERIOD_TYPE","Loại biểu thuế"},{"BAC_THUE","Bậc thuế"},{"CAN_DUOI","Cận dưới (đ)"},{"CAN_TREN","Cận trên (đ, bỏ trống bậc cuối)"},{"THUE_SUAT","Thuế suất (0–1)"},
            {"THAM_GIA_BHXH","Tham gia BHXH"},{"THAM_GIA_BHYT","Tham gia BHYT"},{"THAM_GIA_BHTN","Tham gia BHTN"},{"THAM_GIA_TNLD_BNN","Tham gia TNLĐ/BNN"},{"HUONG_TY_LE_TNLD_UU_DAI","Có căn cứ giảm TNLĐ"},{"LUONG_DONG_BHXH_RIENG","Lương đóng BHXH riêng (đ)"},
            {"LA_DOAN_VIEN","Là đoàn viên"},{"MA_SO_THUE","Mã số thuế / số định danh"},{"IS_CU_TRU","Cá nhân cư trú"},{"CO_UY_QUYEN_QUYET_TOAN","Ủy quyền quyết toán"},{"HO_TEN","Họ tên người phụ thuộc"},{"MOI_QUAN_HE","Mối quan hệ"},{"CCCD","Số định danh"},{"MA_SO_THUE_NPT","Mã thuế người phụ thuộc"},{"NGAY_SINH","Ngày sinh"},{"THANG_BAT_DAU_GIAM_TRU","Tháng bắt đầu giảm trừ (YYYYMM)"},{"THANG_KET_THUC_GIAM_TRU","Tháng kết thúc giảm trừ (YYYYMM)"},
            {"LOAI_DINH_DANH","Loại định danh"},{"SO_HO_CHIEU","Số hộ chiếu"},{"CAN_CU_CU_TRU","Căn cứ cư trú"},{"TRANG_THAI_HO_SO","Trạng thái hồ sơ"},{"NGAY_XAC_MINH","Ngày xác minh"},{"NGUOI_XAC_MINH","Người xác minh"},
            {"LOAI_GIAY_TO","Loại giấy tờ NPT"},{"SO_GIAY_TO","Số giấy tờ NPT"},{"MA_DINH_DANH_CA_NHAN","Mã định danh cá nhân"},{"NOI_CAP_GIAY_TO","Nơi cấp giấy tờ"},{"NGAY_CAP_GIAY_TO","Ngày cấp giấy tờ"},{"QUOC_TICH_NPT","Quốc tịch NPT"},{"CAN_CU_GIAM_TRU","Căn cứ giảm trừ"},{"TRANG_THAI_DANG_KY","Trạng thái đăng ký thuế"},{"TINH_TRANG_HO_SO_CHUNG_MINH","Hồ sơ chứng minh NPT"},{"NGAY_NOP_HO_SO","Ngày nộp hồ sơ"},{"NGAY_DANG_KY_THUE","Ngày đăng ký thuế"},{"GHI_CHU_XAC_MINH","Ghi chú xác minh"},
            {"ID_NPT","Người phụ thuộc"},{"THANG_BAT_DAU","Tháng bắt đầu (YYYYMM)"},{"THANG_KET_THUC","Tháng kết thúc (YYYYMM)"},{"LY_DO_CHUYEN","Lý do chuyển người khai"},
            {"NGAY","Ngày công"},{"IDCAPHIENBAN","Ca làm việc"},{"IDQUYDINH","Quy định chấm công"},{"BATDAU_KEHOACH","Bắt đầu ca"},{"KETTHUC_KEHOACH","Kết thúc ca"},{"TRANG_THAI_PHAN_CONG","Làm việc / nghỉ"},{"LOAI_NGAY","Loại ngày"},{"LY_DO","Lý do phân ca"},
            {"LOAIPHEP","Tên loại nghỉ"},{"TUNGAY","Từ ngày"},{"DENNGAY","Đến ngày"},{"SONGAY","Số ngày nghỉ theo lịch"},{"LYDO","Lý do / căn cứ"},{"BATDAU_NGHI","Bắt đầu nghỉ"},{"KETTHUC_NGHI","Kết thúc nghỉ"},{"GIAY_NGHI","Số giây nghỉ theo lịch (giờ × 3600)"},{"NGUON_CHI_TRA","Nguồn chi trả"},{"LOAI_HUONG_CONG","Loại hưởng công"},
            {"LOAIDIEUCHINH","Loại điều chỉnh"},{"THOIDIEM_VAO_MOI","Thời điểm vào xác nhận"},{"THOIDIEM_RA_MOI","Thời điểm ra xác nhận"},{"GIOTANGCA","Số giờ tăng ca"},{"IDCA","Loại ca tăng ca"},{"BATDAU_DANGKY","Bắt đầu tăng ca"},{"KETTHUC_DANGKY","Kết thúc tăng ca"},{"TRANGTHAI","Trạng thái đơn"}
        };
        static FrmPayrollDataEditor() {
            Labels["IDBATTHUONG"]="Mã bất thường";
            Labels["MA_LOI"]="Loại bất thường";
            Labels["BATDAU_LUC"]="Bắt đầu khoảng";
            Labels["KETTHUC_LUC"]="Kết thúc khoảng";
            Labels["MO_TA"]="Nội dung xác minh";
            Labels["IDLOAICA"]="Loại ca";
            Labels["SO_PHIENBAN"]="Phiên bản";
            Labels["TEN_PHIENBAN"]="Tên phiên bản ca";
            Labels["TU_NGAY"]="Hiệu lực từ ngày";
            Labels["DEN_NGAY"]="Kết thúc hiệu lực (không bao gồm ngày này)";
            Labels["TONG_GIAY_CHUAN"]="Giây công chuẩn ca";
            Labels["CONG_QUY_DOI"]="Công quy đổi ca";
            Labels["STT"]="Thứ tự khung";
            Labels["BATDAU_PHUT"]="Phút bắt đầu từ 00:00";
            Labels["KETTHUC_PHUT"]="Phút kết thúc từ 00:00";
            Labels["LOAI_KHUNGGIO"]="Loại khung giờ";
            Labels["BAT_BUOC_QUET_THE"]="Bắt buộc quẹt thẻ";
            Labels["MA_QUYDINH"]="Mã quy định";
            Labels["TEN_QUYDINH"]="Tên quy định";
            Labels["GIAY_MIEN_VIPHAM_MUON"]="Ngưỡng miễn đi muộn (giây)";
            Labels["GIAY_MIEN_VIPHAM_SOM"]="Ngưỡng miễn về sớm (giây)";
            Labels["GIAY_DUNG_SAI_HUONG_CONG"]="Dung sai hưởng công (giây)";
            Labels["CACH_HUONG_DUNG_SAI"]="Cách hưởng dung sai";
            Labels["CACH_DEM_MUON"]="Cách tính đi muộn";
            Labels["CUA_SO_VAO_TRUOC_GIAY"]="Khoảng ghép vào sớm (giây)";
            Labels["CUA_SO_RA_SAU_GIAY"]="Khoảng ghép ra muộn (giây)";
            Labels["GIOI_HAN_GHEP_GIAY"]="Giới hạn ghép lượt (giây)";
            Labels["MUI_GIO"]="Múi giờ";
            Labels["DEM_BATDAU_PHUT"]="Phút bắt đầu ca đêm";
            Labels["DEM_KETTHUC_PHUT"]="Phút kết thúc ca đêm";
            Labels["KIEU_LAM_TRON"]="Cách tính thời gian";
            Labels["BUOC_LAM_TRON_GIAY"]="Bước tính (giây)";
            Labels["LOAICONG"]="Loại ngày trả tăng ca";
            Labels["LOAICA"]="Tên loại ca trả tăng ca";
            Labels["GIO_BATDAU"]="Giờ bắt đầu (HH:mm)";
            Labels["GIO_KETTHUC"]="Giờ kết thúc (HH:mm)";
            Labels["HESO_CHINHTHUC"]="Hệ số lương chính thức";
            Labels["HESO_THUVIEC"]="Hệ số lương thử việc";
            Labels["LA_BAN_DEM"]="Làm ban đêm";
            Labels["CO_OT_BAN_NGAY_TRUOC"]="Có tăng ca ban ngày trước";
        }
        public static string Caption(string field)=>TranslationManager.Translate(Labels.TryGetValue(field,out string label)?label:field);
    }
}
