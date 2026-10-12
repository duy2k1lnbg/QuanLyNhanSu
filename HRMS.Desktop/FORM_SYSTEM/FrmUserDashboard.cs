using System;
using ComboBox = DevExpress.XtraEditors.ComboBoxEdit;
using Button = DevExpress.XtraEditors.SimpleButton;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.CLASS_SYSTEM;
using Bu.CLASS_SECURITY;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using QLyNSu.Functions;
namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmUserDashboard:XtraForm
    {
        private readonly TabControl tabs=new TabControl {Dock=DockStyle.Top,Height=38};
        private readonly TextBox search=new TextBox {Width=220};
        private readonly ComboBox channel=new ComboBox {Width=140};
        private readonly ComboBox result=new ComboBox {Width=155};
        private readonly DateTimePicker fromDate=new DateTimePicker {Width=140,Format=DateTimePickerFormat.Short,ShowCheckBox=true,Checked=false};
        private readonly DateTimePicker toDate=new DateTimePicker {Width=140,Format=DateTimePickerFormat.Short,ShowCheckBox=true,Checked=false};
        private readonly TextBox session=new TextBox {Width=190},correlation=new TextBox {Width=190};
        private readonly GridControl grid=new GridControl {Dock=DockStyle.Fill};
        private readonly GridView view; private readonly Label summary=new Label {AutoSize=true};
        private readonly Button previous=new Button(),next=new Button(),revoke=new Button();
        private readonly Label note=new Label {Dock=DockStyle.Top,Height=64,Padding=new Padding(12),BackColor=Color.FromArgb(239,246,255),ForeColor=Color.FromArgb(30,64,120)};
        private readonly string[] ResultCodes={"ALL","SUCCESS","FAILURE","DENIED","ACTIVE","EXPIRED","REVOKED","UNKNOWN"};
        private int page=1,total;private string Kind=>new[]{"activity","security","sessions"}[Math.Max(0,tabs.SelectedIndex)];
        private static string T(string text)=>TranslationManager.Translate(text);
        public FrmUserDashboard()
        {
            AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);
            Text=T("Giám sát đa nền tảng");Font=new Font("Segoe UI",9.5f);MinimumSize=new Size(900,620);Size=new Size(1240,760);
            view=new GridView(grid);grid.MainView=view;view.OptionsBehavior.Editable=false;view.OptionsView.ShowGroupPanel=false;view.OptionsView.ColumnAutoWidth=false;view.OptionsView.RowAutoHeight=true;
            DesktopTypography.Grid(view);
            view.CustomColumnDisplayText+=(s,e)=>{if(e.Column.FieldName=="result" || e.Column.FieldName=="channel")e.DisplayText=DisplayValue(Convert.ToString(e.Value));};
            var title=new Label {Text=T("Nhật ký thao tác, bảo mật và phiên truy cập"),Dock=DockStyle.Top,Height=60,TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(12,4,12,4),Font=new Font(Font.FontFamily,12f,FontStyle.Bold)};
            foreach(var text in new[]{"Nhật ký thao tác","Đăng nhập và bảo mật","Thiết bị và phiên truy cập"})tabs.TabPages.Add(T(text));
            channel.Properties.TextEditStyle=DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;result.Properties.TextEditStyle=DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            channel.Properties.Items.AddRange(new object[]{T("Tất cả"),T("Desktop"),T("Web"),T("Mobile"),T("Hệ thống"),T("Không xác định")});channel.SelectedIndex=0;
            result.Properties.Items.AddRange(ResultCodes.Select(code=>(object)DisplayValue(code)).ToArray());result.SelectedIndex=0;
            var filters=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,Padding=new Padding(10),WrapContents=true};
            foreach(var pair in new[]{Tuple.Create("Tìm kiếm",(Control)search),Tuple.Create("Kênh",(Control)channel),Tuple.Create("Kết quả",(Control)result),Tuple.Create("Từ ngày",(Control)fromDate),Tuple.Create("Đến ngày",(Control)toDate),Tuple.Create("Mã phiên",(Control)session),Tuple.Create("Mã liên kết",(Control)correlation)}) {
                var group=new FlowLayoutPanel {AutoSize=true,WrapContents=false,Margin=new Padding(4,2,6,2)};group.Controls.Add(LabelFor(pair.Item1));group.Controls.Add(pair.Item2);filters.Controls.Add(group);
            }
            filters.Controls.Add(ActionButton("Tìm kiếm",async()=>{page=1;await LoadData();}));
            filters.Controls.Add(ActionButton("Đặt lại",async()=>{search.Clear();session.Clear();correlation.Clear();channel.SelectedIndex=result.SelectedIndex=0;fromDate.Checked=toDate.Checked=false;page=1;await LoadData();}));
            var footer=new FlowLayoutPanel {Dock=DockStyle.Bottom,AutoSize=true,Padding=new Padding(10)};
            Setup(previous,"Trang trước",async()=>{if(page>1){page--;await LoadData();}});Setup(next,"Trang sau",async()=>{if(page*50<total){page++;await LoadData();}});
            Setup(revoke,"Thu hồi phiên",Revoke);footer.Controls.AddRange(new Control[]{previous,next,summary,ActionButton("Xem chi tiết",Details),revoke,ActionButton("Làm mới",LoadData)});
            Controls.Add(grid);Controls.Add(footer);Controls.Add(note);Controls.Add(filters);Controls.Add(tabs);Controls.Add(title);
            tabs.SelectedIndexChanged+=async(s,e)=>{result.SelectedIndex=0;page=1;await LoadData();};view.DoubleClick+=async(s,e)=>await Details();
            Shown+=async(s,e)=>await LoadData();
        }
        private static Label LabelFor(string text)=>new Label {Text=T(text),AutoSize=true,Margin=new Padding(5,7,3,4)};
        private Button ActionButton(string text,Func<Task> action){var button=new Button();Setup(button,text,action);return button;}
        private void Setup(Button button,string text,Func<Task> action){button.Text=T(text);button.AutoSize=false;button.Size=new Size(Math.Max(110,TextRenderer.MeasureText(button.Text,Font).Width+28),36);button.MinimumSize=new Size(110,36);button.Click+=async(s,e)=>{try{await action();}catch(OperationCanceledException){}catch(Exception ex){FormDataLoader.ShowLoadError(this,ex);}};}
        private async Task LoadData()
        {
            if(!UserSession.CanView("F_SYSTEM_GIAMSAT")) {note.Text=T("Chưa được cấp quyền giám sát.");return;}
            var kind=Kind;var filter=new MonitoringFilter {Kind=kind,Channel=channel.SelectedIndex==5?"UNKNOWN":channel.SelectedIndex==0?null:new[]{"ALL","DESKTOP","WEB","MOBILE","SYSTEM","UNKNOWN"}[channel.SelectedIndex],Result=result.SelectedIndex<=0?null:ResultCodes[result.SelectedIndex],Search=search.Text,SessionId=session.Text,CorrelationId=correlation.Text,
                FromDate=fromDate.Checked?(DateTime?)fromDate.Value.Date:null,ToDate=toDate.Checked?(DateTime?)toDate.Value.Date:null,Page=page,PageSize=50};
            try {
                var data=await FormDataLoader.ReadAsync(this,()=>MonitoringService.Get(filter));if(kind!=Kind)return;
                grid.DataSource=data.data;view.PopulateColumns();
                var columns=kind=="sessions"?new[]{"timestamp","username","channel","computerName","result","lastSeen","expiresAt"}:new[]{"timestamp","username","channel","computerName","action","result","tableName","recordId"};
                foreach(DevExpress.XtraGrid.Columns.GridColumn c in view.Columns){c.Visible=columns.Contains(c.FieldName);c.Caption=T(Caption(c.FieldName));c.Width=c.FieldName=="action"?210:c.FieldName=="computerName"?200:155;}
                for(int i=0;i<columns.Length;i++)view.Columns[columns[i]].VisibleIndex=i;
                total=data.total;summary.Text=T("Tổng số bản ghi")+": "+total+" · "+T("Trang")+" "+page;previous.Enabled=page>1;next.Enabled=page*50<total;
                revoke.Visible=kind=="sessions" && UserSession.CanEdit("F_SYSTEM_GIAMSAT");
                note.Text=T(kind=="sessions"?"Phiên chưa hết hạn không đồng nghĩa thiết bị đang online. Xem thời điểm hoạt động gần nhất.":"Nhật ký cũ thiếu nguồn truy cập được hiển thị Không xác định; không suy đoán kênh từ IP.");
            } catch(OperationCanceledException){}catch(Exception ex){grid.DataSource=null;note.Text=T("Không tải được nhật ký. Vui lòng thử lại.");FormDataLoader.ShowLoadError(this,ex);}
        }
        private static string Caption(string key)
        {
            switch(key){case "timestamp":return "Thời gian";case "username":return "Người thực hiện";case "channel":return "Kênh";case "computerName":return "Thiết bị";case "action":return "Hành động";case "result":return "Kết quả";case "tableName":return "Đối tượng";case "recordId":return "Mã bản ghi";case "lastSeen":return "Hoạt động gần nhất";case "expiresAt":return "Hết hạn";case "id":return "Mã nhật ký";case "userId":return "Mã tài khoản";case "deviceId":return "Mã thiết bị";case "ipAddress":return "Địa chỉ IP";case "userAgent":return "Trình duyệt / ứng dụng";case "module":return "Phân hệ";case "sessionId":return "Mã phiên";case "correlationId":return "Mã liên kết";case "changedFields":return "Chi tiết thay đổi";case "oldData":return "Dữ liệu trước";case "newData":return "Dữ liệu sau";case "appVersion":return "Phiên bản";case "executor":return "Dịch vụ thực thi";case "ipSource":return "Nguồn địa chỉ IP";default:return key;}
        }
        private static string DisplayValue(string code)
        {
            switch(code){case "ALL":return T("Tất cả");case "SUCCESS":return T("Thành công");case "FAILURE":return T("Thất bại");case "DENIED":return T("Bị từ chối");case "ACTIVE":return T("Chưa hết hạn");case "EXPIRED":return T("Đã hết hạn");case "REVOKED":return T("Đã thu hồi");case "SYSTEM":return T("Hệ thống");case "UNKNOWN":return T("Không xác định");default:return code;}
        }
        private Task Details()
        {
            var row=view.GetFocusedRow() as MonitoringRow;if(row==null)return Task.CompletedTask;
            var window=new XtraForm {Text=T("Chi tiết nhật ký"),Size=new Size(780,640),Font=Font};
            var text=new TextBox {Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,WordWrap=true,Font=new Font("Segoe UI",10f)};
            text.Text=string.Join(Environment.NewLine+Environment.NewLine,typeof(MonitoringRow).GetProperties().Where(p=>p.Name!="canRevoke").Select(p=>T(Caption(p.Name))+": "+((p.Name=="result" || p.Name=="channel"?DisplayValue(Convert.ToString(p.GetValue(row))):Convert.ToString(p.GetValue(row)))??T("Không xác định"))));window.Controls.Add(text);window.Show(this);return Task.CompletedTask;
        }
        private async Task Revoke()
        {
            var row=view.GetFocusedRow() as MonitoringRow;if(row==null || Kind!="sessions" || row.result!="ACTIVE" || !UserSession.CanEdit("F_SYSTEM_GIAMSAT"))return;
            if(XtraMessageBox.Show(this,T("Thu hồi phiên sẽ yêu cầu thiết bị đăng nhập lại. Tiếp tục?"),T("Thu hồi phiên"),MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            var actor=UserSession.CurrentUser.IDUSER;bool ok=await FormDataLoader.ReadAsync(this,()=>new AuthSecurityService().RevokeSessionAsync(row.sessionId,"ADMIN_MONITOR_REVOKE",actor,Guid.NewGuid().ToString("N")).GetAwaiter().GetResult());
            if(!ok)throw new InvalidOperationException(T("Không thu hồi được phiên."));await LoadData();
        }
        // Keep designer event references valid; the monitoring layout is built in code.
        private async void FrmUserDashboard_Load(object sender,EventArgs e){await LoadData();}
        private async void btnRefresh_ItemClick(object sender,DevExpress.XtraBars.ItemClickEventArgs e){await LoadData();}
    }
}
