using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DevExpress.XtraEditors;

namespace QLyNSu.FORM_CHAMCONG
{
    public sealed class FrmPayrollPublication : XtraForm
    {
        private readonly int period;
        private readonly decimal actor;
        private PayrollPublicationReview review;
        private bool busy;
        private readonly Label summary=new Label {Dock=DockStyle.Fill,AutoSize=false};
        private readonly TextBox guidance=new TextBox {Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.None,BackColor=SystemColors.Control};
        private readonly TextBox reason=new TextBox {Dock=DockStyle.Fill,Multiline=true,MaxLength=1000,ScrollBars=ScrollBars.Vertical};
        private readonly CheckBox legacy=new CheckBox {Dock=DockStyle.Fill,AutoSize=false};
        private readonly Button publish=new Button {AutoSize=true};
        private readonly Button refresh=new Button {AutoSize=true};
        private readonly Button close=new Button {AutoSize=true};
        private static string T(string value)=>Functions.TranslationManager.Translate(value);
        public FrmPayrollPublication(int selectedPeriod)
        {
            period=selectedPeriod;actor=UserSession.CurrentUser.IDUSER;
            Text=T("Công bố lương")+" - "+period;
            MinimumSize=new Size(740,540);Size=new Size(820,600);StartPosition=FormStartPosition.CenterParent;
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=7,Padding=new Padding(16)};
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,65));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,26));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,82));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
            layout.Controls.Add(new Label {Dock=DockStyle.Fill,Text=T("Công bố để nhân viên xem phiếu lương trên Mobile và Web. Không thay đổi số tiền và không xác nhận đã chi trả.")},0,0);
            layout.Controls.Add(summary,0,1);layout.Controls.Add(guidance,0,2);
            legacy.Text=T("Tôi đã đối chiếu hồ sơ lương lịch sử và xác nhận công bố đúng số liệu lưu trữ.");layout.Controls.Add(legacy,0,3);
            layout.Controls.Add(new Label {Dock=DockStyle.Fill,Text=T("Căn cứ công bố (5–1000 ký tự)")},0,4);layout.Controls.Add(reason,0,5);
            var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(0,8,0,0)};
            publish.Text=T("Công bố lương");refresh.Text=T("Kiểm tra lại");close.Text=T("Đóng");
            actions.Controls.AddRange(new Control[]{publish,refresh,close});layout.Controls.Add(actions,0,6);Controls.Add(layout);
            Functions.DesktopTypography.Apply(this);
            Shown+=async(s,e)=>await Reload();
            publish.Click+=async(s,e)=>await Save();refresh.Click+=async(s,e)=>await Reload();close.Click+=(s,e)=>Close();
            FormClosing+=(s,e)=>{if(busy)e.Cancel=true;};
            publish.Enabled=false;
        }
        public static string Guidance(string code)
        {
            switch(code){
                case "PAYROLL_FROZEN":return ErrorHelper.PayrollFrozenMessage;
                case "GLOBAL_PERIOD_SCOPE":return "Kỳ công dùng chung giữa các công ty. Cần người quản trị có phạm vi toàn kỳ để công bố và khóa bất biến.";
                case "EMPTY_PAYROLL":return "Chưa có bảng lương. Mở Bảng lương và tính lương cho kỳ đã chọn.";
                case "LOCK_PERIOD":return "Đối chiếu lương, sau đó mở Bảng công và khóa kỳ công trước khi công bố lương.";
                case "CALCULATION_REQUIRED":return "Cần tính lại lương để có kết quả và phiên bản công hợp lệ. Thực hiện tại PC: Chấm công > Bảng lương > Tính lương.";
                case "INPUT_CHANGED":return "Đầu vào công hoặc hồ sơ đã thay đổi. Công bố lại công và tính lại lương trước khi công bố phiếu.";
                case "INVALID_TOTALS":return "Phiếu thiếu số tiền hoặc tổng thu nhập/khấu trừ không khớp thực lĩnh. Đối chiếu Chi tiết lương và tính lại.";
                default:return "Trạng thái phiếu chưa phù hợp để công bố. Kiểm tra và xử lý tại Bảng lương.";
            }
        }
        private void Toggle(bool value){busy=value;refresh.Enabled=!value;close.Enabled=!value;reason.Enabled=!value && review?.Frozen!=true;legacy.Enabled=!value && review?.Frozen!=true;publish.Enabled=!value && review!=null && review.CanPublish;}
        private async Task Reload()
        {
            if(busy)return;Toggle(true);
            try {
                review=await Functions.FormDataLoader.ReadAsync(this,()=>new PayrollPublicationService().Review(period,actor,"DESKTOP"));
                summary.Text=T("Kỳ công")+": "+period+"\r\n"+T("Tổng phiếu")+": "+review.Total+"   "+T("Chưa công bố")+": "+review.Pending+"   "+T("Đã công bố")+": "+review.Published;
                guidance.Text=string.Join("\r\n\r\n",review.BlockerCodes.Select(code=>T(Guidance(code))));
                if(!review.HasPermission)guidance.Text+="\r\n"+T("Cần quyền Sửa của chức năng Công bố lương trên nền tảng đang sử dụng.");
                if(review.LegacyPending>0)guidance.Text+="\r\n\r\n"+T("Có phiếu lương lịch sử. Xác nhận đã đối chiếu hồ sơ lưu trữ trước khi công bố; không tính lại theo chính sách mới.");
                if(review.Frozen)guidance.Text+="\r\n\r\n"+T(ErrorHelper.PayrollFrozenMessage);
                legacy.Visible=review.LegacyPending>0;legacy.Checked=false;
            } catch(OperationCanceledException){} catch(Exception ex){review=null;Functions.FormDataLoader.ShowLoadError(this,ex);}
            finally{if(!IsDisposed)Toggle(false);}
        }
        private async Task Save()
        {
            if(busy || review==null)return;
            if(!UserSession.IsLoggedIn || UserSession.CurrentUser==null || UserSession.CurrentUser.IDUSER!=actor || !UserSession.CanEdit(PayrollPublicationService.Capability))
            {XtraMessageBox.Show(T("Cần quyền Sửa của chức năng Công bố lương trên nền tảng đang sử dụng."),T("Thông báo"));return;}
            if(reason.Text.Trim().Length<5){XtraMessageBox.Show(T("Nhập căn cứ công bố từ 5 đến 1000 ký tự."),T("Thông báo"));return;}
            if(review.LegacyPending>0 && !legacy.Checked){XtraMessageBox.Show(T("Cần xác nhận đã đối chiếu hồ sơ lương lịch sử."),T("Thông báo"));return;}
            if(XtraMessageBox.Show(T("Công bố các phiếu đã kiểm tra cho nhân viên xem? Thao tác này không xác nhận chi trả.")+"\r\n\r\n"+T("Công bố sẽ khóa bất biến lương và bảng công của kỳ này. Sau công bố không được sửa, xóa, tính lại, đổi trạng thái hay mở khóa, kể cả ADMIN."),T("Xác nhận"),MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            var command=new PayrollPublicationCommand {ExpectedStamp=review.Stamp,Reason=reason.Text.Trim(),ConfirmLegacy=legacy.Checked};Toggle(true);
            try {
                var result=await Functions.FormDataLoader.ReadAsync(this,()=>new PayrollPublicationService().Publish(period,actor,"DESKTOP",command));
                XtraMessageBox.Show(T("Đã cập nhật số phiếu")+": "+result.Changed,T("Thông báo"));reason.Clear();
            } catch(OperationCanceledException){} catch(Exception ex){Functions.FormDataLoader.ShowLoadError(this,ex);}
            finally{if(!IsDisposed)Toggle(false);}
            if(!IsDisposed)await Reload();
        }
    }
}
