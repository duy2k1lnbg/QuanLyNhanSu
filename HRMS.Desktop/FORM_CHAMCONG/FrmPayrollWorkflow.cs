using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using QLyNSu.Functions;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace QLyNSu.FORM_CHAMCONG
{
    public sealed class FrmPayrollWorkflow : XtraForm
    {
        public int Period { get; }
        public bool CalculationRequested { get; private set; }
        private PayrollReadinessReport _report;
        private readonly LabelControl _summary=new LabelControl {Dock=DockStyle.Top,Height=54,AutoSizeMode=LabelAutoSizeMode.None,Padding=new Padding(12)};
        private readonly LabelControl _guidance=new LabelControl {Dock=DockStyle.Top,Height=65,AutoSizeMode=LabelAutoSizeMode.None,Padding=new Padding(12)};
        private readonly GridControl _steps=new GridControl {Dock=DockStyle.Fill};private readonly GridView _stepView;
        private readonly GridControl _issues=new GridControl {Dock=DockStyle.Fill};private readonly GridView _issueView;
        private readonly SimpleButton _calculate=new SimpleButton {Text="Tiếp tục tính lương",Width=165,Height=34,Enabled=false};
        private readonly DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit _guidanceEditor=new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
        private bool _busy;
        private readonly SimpleButton _open = new SimpleButton { Text = "Mở nơi xử lý", Width = 170, Height = 36 };
        private readonly LabelControl _empty = new LabelControl { Text = "Bước này không có hồ sơ cần xử lý.", Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None };
        private readonly GroupControl _detail = new GroupControl { Dock = DockStyle.Fill, Text = "Chi tiết bước đang chọn", Padding = new Padding(12) };
        public FrmPayrollWorkflow(int period)
        {
            if (!PayrollReadinessService.IsValidPeriod(period)) throw new ArgumentException("Kỳ công không hợp lệ.");
            Period = period; Text = "Chuẩn bị tính lương - " + period;
            Font = new Font("Segoe UI", 9.5f); Size = new Size(1130, 740); MinimumSize = new Size(850, 650); StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(16);
            _stepView = View(_steps); _issueView = View(_issues);
            _stepView.OptionsView.ColumnAutoWidth = true; _stepView.OptionsView.RowAutoHeight = true;
            _stepView.RowHeight = 42; _steps.RepositoryItems.Add(_guidanceEditor);
            _issueView.OptionsView.ColumnAutoWidth = true; _issueView.OptionsView.RowAutoHeight = true;
            _issueView.OptionsView.ShowAutoFilterRow = false;
            _issues.RepositoryItems.Add(_guidanceEditor);
            _issueView.FocusedRowChanged += (s,e) => _guidance.Text = TranslationManager.Translate(SelectedIssue?.Reason ?? SelectedStep?.Guidance ?? "");
            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0,12,0,12) };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42)); content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            var stepGroup = new GroupControl { Dock = DockStyle.Fill, Text = "Các bước chuẩn bị", Padding = new Padding(8), Margin = new Padding(0,0,12,0) };
            stepGroup.Controls.Add(_steps); content.Controls.Add(stepGroup,0,0); content.Controls.Add(_detail,1,0);
            _guidance.Dock = DockStyle.Top; _guidance.Height = 112; _guidance.Padding = new Padding(8);
            _guidance.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            _guidance.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            var actionPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
            actionPanel.Controls.Add(_open); _detail.Controls.Add(_issues); _detail.Controls.Add(_empty); _detail.Controls.Add(_guidance); _detail.Controls.Add(actionPanel);
            _empty.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            _empty.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0,8,0,0) };
            footer.Controls.Add(Button("Đóng", () => { Close(); return Task.CompletedTask; })); footer.Controls.Add(_calculate);
            footer.Controls.Add(Button("Kiểm tra lại", Reload));
            var more = new ContextMenuStrip { Font = Font };
            AddAction(more,"Phân ca / ngày nghỉ",()=>OpenData("TB_LICH_LAMVIEC"));
            AddAction(more,"Đơn nghỉ / vắng",()=>OpenData("TB_YEUCAU_NGHIPHEP"));
            AddAction(more,"Điều chỉnh giờ công",()=>OpenData("TB_YEUCAU_DIEUCHINHCONG"));
            AddAction(more,"Xác minh bất thường",()=>OpenData("TB_CHAMCONG_BATTHUONG"));
            AddAction(more,"Đơn tăng ca",()=>OpenData("TB_YEUCAU_TANGCA"));
            AddAction(more,"Công bố lại công",Publish); more.Items.Add(new ToolStripSeparator());
            AddAction(more,"Phụ cấp",()=>OpenForm(new FrmPhuCap(),"F_CC_PHUCAP"));
            AddAction(more,"Ứng lương",()=>OpenForm(new FrmUngLuong(),"F_CC_UNGLUONG"));
            AddAction(more,"Người phụ thuộc",()=>OpenData("TB_NGUOI_PHU_THUOC"));
            var other = new SimpleButton { Text = "Thao tác khác ▾", Width = 145, Height = 34 };
            other.Click += (sender,args) => more.Show(other,new Point(0,other.Height)); footer.Controls.Add(other);
            FormClosed += (sender,args) => more.Dispose();
            _summary.Height = 64; _summary.Padding = new Padding(8);
            _summary.Appearance.Font = new Font("Segoe UI", 11, FontStyle.Bold); _summary.Appearance.Options.UseFont = true;
            _summary.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            Controls.Add(content); Controls.Add(_summary); Controls.Add(footer);
            _stepView.FocusedRowChanged += (s,e) => SelectStep(); _stepView.DoubleClick += async(s,e) => await Guard(OpenSelected);
            _open.Click += async(s,e) => await Guard(OpenSelected);
            _calculate.Click += async(s,e) => await Guard(async()=> { await Reload(); if(_report?.CanCalculate != true || !UserSession.CanEdit("F_CC_BANGLUONG")) return; CalculationRequested = true; Close(); });
            Load += async(s,e) => await Guard(Reload); TranslationManager.Translate(this);
        }
        private void AddAction(ContextMenuStrip menu, string caption, Func<Task> action)
        {
            var item = menu.Items.Add(TranslationManager.Translate(caption)); item.Click += async(s,e) => await Guard(action);
        }
        private static GridView View(GridControl grid) {var view=new GridView(grid);grid.MainView=view;view.OptionsBehavior.Editable=false;view.OptionsView.ShowGroupPanel=false;view.OptionsView.ColumnAutoWidth=false;view.OptionsDetail.EnableMasterViewMode=false;view.OptionsSelection.EnableAppearanceFocusedCell=false;view.RowHeight=30;view.Appearance.HeaderPanel.TextOptions.HAlignment=DevExpress.Utils.HorzAlignment.Center;view.Appearance.HeaderPanel.TextOptions.VAlignment=DevExpress.Utils.VertAlignment.Center;return view;}
        private SimpleButton Button(string caption,Func<Task> action) {var b=new SimpleButton {Text=caption,Width=145,Height=34};b.Click+=async(s,e)=>await Guard(action);return b;}
        private async Task Guard(Func<Task> action) {if(_busy)return;_busy=true;try {await action();}catch(OperationCanceledException){}catch(Exception e){FormDataLoader.ShowLoadError(this,e);}finally{_busy=false;}}
        public void ApplyReport(PayrollReadinessReport report)
        {
            if(report.Period!=Period)throw new ArgumentException("Kết quả kiểm tra không thuộc kỳ đang mở.");_report=report;
            _steps.DataSource=report.Steps;_stepView.PopulateColumns();
            foreach(DevExpress.XtraGrid.Columns.GridColumn c in _stepView.Columns)c.Visible=false;
            SetColumn(_stepView,"Title","Bước thực hiện",300);SetColumn(_stepView,"Status","Kết quả",110);
            _stepView.OptionsView.RowAutoHeight=true;_stepView.Columns["Title"].ColumnEdit=_guidanceEditor;
            DesktopTypography.Grid(_stepView); _stepView.RowHeight=42;
            _calculate.Enabled=report.CanCalculate && UserSession.CanEdit("F_CC_BANGLUONG");
            int blocked = report.Steps.Count(step => step.Blocking);
            _summary.Text = "Tháng " + (Period % 100).ToString("00") + "/" + (Period / 100) + " · " + TranslationManager.Translate(report.CanCalculate ? "Đủ điều kiện tính lương" : "Còn " + blocked + " bước cần xử lý") + "\n" + (report.Steps.Count - blocked) + "/" + report.Steps.Count + " " + TranslationManager.Translate("bước đã hoàn tất");
            var first=report.Steps.FindIndex(s=>s.Blocking);if(first>=0)_stepView.FocusedRowHandle=first;SelectStep();
        }
        private async Task Reload() {if(!UserSession.CanView("F_CC_BANGLUONG"))throw new UnauthorizedAccessException("Cần quyền xem Bảng lương.");var report=await FormDataLoader.ReadAsync(this,()=>new PayrollReadinessService().Inspect(Period));ApplyReport(report);}
        private PayrollPreparationStep SelectedStep=>_stepView.GetFocusedRow() as PayrollPreparationStep;
        private PayrollPreparationIssue SelectedIssue=>_issueView.GetFocusedRow() as PayrollPreparationIssue;
        private void SelectStep() {var step=SelectedStep;_detail.Text=TranslationManager.Translate(step?.Title??"Chi tiết bước đang chọn"); _open.Enabled=step!=null; _open.Text=TranslationManager.Translate(step?.Target==PayrollPreparationTarget.PublishAttendance?"Công bố lại công":"Mở nơi xử lý"); _issues.Visible=step?.Issues.Count>0; _empty.Visible=!_issues.Visible; _guidance.Text=TranslationManager.Translate(step?.Guidance??"");_issues.DataSource=step?.Issues;_issueView.PopulateColumns();foreach(DevExpress.XtraGrid.Columns.GridColumn c in _issueView.Columns)c.Visible=false;
            SetColumn(_issueView,"EmployeeName","Họ tên",145);SetColumn(_issueView,"Day","Ngày công",95);SetColumn(_issueView,"Reason","Cần xử lý",300);
            if(_issueView.Columns["Reason"]!=null) _issueView.Columns["Reason"].ColumnEdit=_guidanceEditor; DesktopTypography.Grid(_issueView);
            if(_issueView.Columns["Day"]!=null){_issueView.Columns["Day"].DisplayFormat.FormatType=DevExpress.Utils.FormatType.DateTime;_issueView.Columns["Day"].DisplayFormat.FormatString="dd/MM/yyyy";}}
        private static void SetColumn(GridView view,string field,string title,int width) {var c=view.Columns[field];if(c==null)return;c.Visible=true;c.Caption=TranslationManager.Translate(title);c.MinWidth=Math.Min(width,120);c.Width=width;}
        private async Task OpenSelected()
        {
            switch(SelectedStep?.Target) {
                case PayrollPreparationTarget.Period:await OpenForm(new FrmBangCong {InitialPeriod=Period},"F_CC_BANGCONG");break;
                case PayrollPreparationTarget.Contracts:await OpenForm(new QLyNSu.FrmHopDongLaoDong(),"F_NV_HOPDONG");break;
                case PayrollPreparationTarget.Policies:await OpenData("TB_CHINH_SACH_LUONG");break;
                case PayrollPreparationTarget.Insurance:await OpenData("TB_NHANVIEN_BAOHIEM_THAM_GIA");break;
                case PayrollPreparationTarget.Union:await OpenData("TB_NHANVIEN_CONG_DOAN_THAM_GIA");break;
                case PayrollPreparationTarget.Tax:await OpenData("TB_NHANVIEN_THUE");break;
                case PayrollPreparationTarget.PublishAttendance:await Publish();break;
                case PayrollPreparationTarget.Occurrences:await OpenForm(new FrmPhatSinhLuong {InitialPeriod=Period},"F_CC_BANGLUONG");break;
                case PayrollPreparationTarget.Attendance:
                    var issue=SelectedIssue;
                    if(issue?.Day!=null) await OpenData(issue.ActionTable??"TB_LICH_LAMVIEC");
                    else await OpenForm(new FrmBangCong_ChiTiet {_MAKYCONG=Period,_nam=Period/100,_thang=Period%100},"F_CC_BANGCONG");break;
                case PayrollPreparationTarget.Database:await OpenForm(new QLyNSu.FORM_SYSTEM.FrmDatabaseConfig(),"F_SYSTEM_DB_CONFIG");break;
            }
        }
        private async Task OpenData(string table) {PayrollPreparationDataService.Demand(table);var issue=SelectedIssue;await OpenForm(new FrmPayrollDataEditor(Period,table,issue!=null && issue.EmployeeId>0 ? (decimal?)issue.EmployeeId:null,issue?.Day),PayrollPreparationDataService.Find(table).Capability);}
        private async Task OpenForm(Form child,string capability) {using(child) {if(!UserSession.CanView(capability))throw new UnauthorizedAccessException("Cần quyền "+capability+" để mở nơi xử lý. Hãy yêu cầu quản trị viên cấp quyền tương ứng.");await ChildFormWindow.ShowAsync(this,child);}if(!IsDisposed)await Reload();}
        private async Task Publish()
        {
            if(!UserSession.CanView("F_CC_BANGCONG") || !UserSession.CanEdit("F_CC_BANGCONG"))throw new UnauthorizedAccessException("Cần quyền sửa Bảng công để công bố công.");
            if(XtraMessageBox.Show(this,"Công bố lại công kỳ "+Period+" từ dữ liệu nguồn và đơn đã phê duyệt? Bất thường còn tồn tại sẽ tiếp tục chặn tính lương.","Công bố bảng công",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            long actor=(long)UserSession.CurrentUser.IDUSER;
            var result=await FormDataLoader.ReadAsync(this,()=>new AttendancePublishingService().PublishAttendance(new AttendancePublishRequest {MaKyCong=Period,NguoiThucHien=actor,ForceRecalculate=true,MaYeuCau="DESKTOP-PREPARATION-"+Guid.NewGuid().ToString("N"),GhiChu="Công bố công theo quy trình chuẩn bị tính lương"}));
            if(!result.Success)throw new InvalidOperationException(result.ErrorMessage);
            await Reload();XtraMessageBox.Show(this,$"Đã công bố {result.SoNgayCong} ngày công. {result.SoNgaySanSang} ngày đủ điều kiện; {result.SoNgayChoXacMinh} ngày chờ xác minh. Kiểm tra danh sách còn cần xử lý.","Công bố bảng công");
        }
    }
}
