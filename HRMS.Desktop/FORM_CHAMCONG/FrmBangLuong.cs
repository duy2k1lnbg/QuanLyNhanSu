using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraSplashScreen;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmBangLuong : DevExpress.XtraEditors.XtraForm
    {
        private BANGLUONG _bangluong;
        private KYCONG _kycong;
        private bool _isCalculating;

        public FrmBangLuong()
        {
            InitializeComponent();
            Functions.DesktopTypography.Apply(this, barManager1);
            var preparation = new DevExpress.XtraBars.BarButtonItem(barManager1, Functions.TranslationManager.Translate("Quy trình tính lương"));
            preparation.ItemClick += async (s, e) => await OpenPreparation();
            bar1.AddItem(preparation);
            var publication=new DevExpress.XtraBars.BarButtonItem(barManager1,Functions.TranslationManager.Translate("Công bố lương"));
            publication.ItemClick+=async(s,e)=>await OpenPublication();
            bar1.AddItem(publication);
            Functions.DesktopTypography.Apply(this, barManager1);
        }

        private async void FrmBangLuong_Load(object sender, EventArgs e)
        {
            _bangluong = new BANGLUONG();
            _kycong = new KYCONG();
            ConfigureGrid();
            await LoadCombo();
            if (IsDisposed || Disposing) return;
            await LoadData();
            if (IsDisposed || Disposing) return;
            gvBangLuong.DoubleClick += gvBangLuong_DoubleClick;
        }

        private void ConfigureGrid()
        {
            // 1. Tắt tự động co ép cột vào khung hình (cho phép các cột mở rộng theo độ rộng thực tế)
            gvBangLuong.OptionsView.ColumnAutoWidth = false;

            // 2. Luôn hiển thị thanh cuộn ngang dưới đáy bảng
            gvBangLuong.HorzScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Always;

            // 3. Tối ưu tiêu đề cột (wrap text, căn giữa, tăng chiều cao header)
            gvBangLuong.Appearance.HeaderPanel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold);
            gvBangLuong.Appearance.HeaderPanel.Options.UseFont = true;
            gvBangLuong.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            gvBangLuong.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            gvBangLuong.Appearance.HeaderPanel.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            gvBangLuong.ColumnPanelRowHeight = 36;
            gvBangLuong.RowHeight = 26;

            var statusColumn=gvBangLuong.Columns.ColumnByFieldName("TRANG_THAI") ?? gvBangLuong.Columns.AddVisible("TRANG_THAI");
            statusColumn.Caption=Functions.TranslationManager.Translate("Kết quả lương");
            statusColumn.VisibleIndex=2; statusColumn.MinWidth=140; statusColumn.Width=150;
            gvBangLuong.CustomColumnDisplayText += (sender,args) => {
                if(args.Column.FieldName!="TRANG_THAI") return;
                var status=Convert.ToString(args.Value);
                if(status=="CAN_TINH_LAI") args.DisplayText=Functions.TranslationManager.Translate("Cần tính lại");
                else if(status=="CALCULATED") args.DisplayText=Functions.TranslationManager.Translate("Đã tính");
                else if(status=="PUBLISHED") args.DisplayText=Functions.TranslationManager.Translate("Đã công bố");
                else if(status=="LEGACY_READONLY") args.DisplayText=Functions.TranslationManager.Translate("Lương lịch sử");
            };

            Functions.DesktopTypography.Grid(gvBangLuong);

            // 4. Ghim cố định 2 cột đầu tiên (Mã NV và Họ Tên) bên trái khi cuộn ngang
            MANV.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            HOTEN.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            // Bỏ giới hạn MaxWidth để Họ tên hiển thị đầy đủ
            MANV.MaxWidth = 0;
            HOTEN.MaxWidth = 0;
            MANV.MinWidth = 80;
            HOTEN.MinWidth = 180;

            // 5. Căn lề phải cho tất cả cột số tiền và ngày công
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gvBangLuong.Columns)
            {
                if (col.DisplayFormat.FormatType == DevExpress.Utils.FormatType.Numeric)
                {
                    col.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                }
            }

            // 6. Hiển thị dòng Footer tổng kết cuối bảng
            gvBangLuong.OptionsView.ShowFooter = true;
            MANV.Summary.Clear();
            MANV.Summary.Add(DevExpress.Data.SummaryItemType.Count, "MANV", "Tổng: {0} NV");

            TONG_CONG.Summary.Clear();
            TONG_CONG.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "TONG_CONG", "{0:n0}");

            TIEN_BHXH_TRICH.Summary.Clear();
            TIEN_BHXH_TRICH.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "TIEN_BHXH_TRICH", "{0:n0}");

            THUE_TNCN.Summary.Clear();
            THUE_TNCN.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "THUE_TNCN", "{0:n0}");

            THUC_LINH.Summary.Clear();
            THUC_LINH.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "THUC_LINH", "{0:n0}");
        }

        private async Task LoadCombo()
        {
            try
            {
            cboKyCong.DataSource = await Functions.FormDataLoader.ReadAsync(this, () => _kycong.getList());
            cboKyCong.DisplayMember = "MAKYCONG";
            cboKyCong.ValueMember = "MAKYCONG";

            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private async Task LoadData()
        {
            try
            {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                var kc = await Functions.FormDataLoader.ReadAsync(this, () => _kycong.getItem(makycong));
                bool isKhoa = kc != null && (kc.KHOA ?? 0) == 1;
                bool isPayrollFrozen = await Functions.FormDataLoader.ReadAsync(this, () => _kycong.IsPayrollFrozen(makycong));
                bool canEdit = Bu.CLASS_SYSTEM.UserSession.CanEdit("F_CC_BANGLUONG");
                bool canCalculate = canEdit && isKhoa && !isPayrollFrozen;
                btnTinhLuong.Enabled = canCalculate;
                btnTinhLuongCong.Enabled = canCalculate;

                gcBangLuong.DataSource = await Functions.FormDataLoader.ReadAsync(this, () => _bangluong.getList(makycong));
                gvBangLuong.OptionsBehavior.Editable = false;

                // Tự động căn chỉnh độ rộng các cột vừa khít dữ liệu và tiêu đề
                gvBangLuong.BestFitColumns();

                // Đảm bảo các cột quan trọng không bị thu quá hẹp
                MANV.Width = Math.Max(MANV.Width, 80);
                HOTEN.Width = Math.Max(HOTEN.Width, 180);
                MAKYCONG.Width = Math.Max(MAKYCONG.Width, 90);
                CONG_CHUAN.Width = Math.Max(CONG_CHUAN.Width, 100);
                CONG_THUCTE.Width = Math.Max(CONG_THUCTE.Width, 105);
                DAILY_RATE.Width = Math.Max(DAILY_RATE.Width, 110);
                LUONG_CONG_THUCTE.Width = Math.Max(LUONG_CONG_THUCTE.Width, 130);
                TIEN_TANGCA.Width = Math.Max(TIEN_TANGCA.Width, 105);
                TIEN_CHUYENCAN.Width = Math.Max(TIEN_CHUYENCAN.Width, 105);
                TIEN_AN_CA.Width = Math.Max(TIEN_AN_CA.Width, 100);
                KHOAN_CONG_KHAC.Width = Math.Max(KHOAN_CONG_KHAC.Width, 105);
                TONG_CONG.Width = Math.Max(TONG_CONG.Width, 135);
                LUONG_BHXH.Width = Math.Max(LUONG_BHXH.Width, 120);
                TIEN_BHXH.Width = Math.Max(TIEN_BHXH.Width, 100);
                TIEN_BHYT.Width = Math.Max(TIEN_BHYT.Width, 100);
                TIEN_BHTN.Width = Math.Max(TIEN_BHTN.Width, 100);
                TIEN_BHXH_TRICH.Width = Math.Max(TIEN_BHXH_TRICH.Width, 135);
                TIEN_CONG_DOAN.Width = Math.Max(TIEN_CONG_DOAN.Width, 105);
                TIEN_TAMUNG.Width = Math.Max(TIEN_TAMUNG.Width, 100);
                THUE_TNCN.Width = Math.Max(THUE_TNCN.Width, 105);
                KHOAN_TRU_KHAC.Width = Math.Max(KHOAN_TRU_KHAC.Width, 100);
                THUC_LINH.Width = Math.Max(THUC_LINH.Width, 140);
            }

            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private void btnTinhLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TinhLuong();
        }

        private void btnTinhLuongCong_Click(object sender, EventArgs e)
        {
            TinhLuong();
        }

        private async Task OpenPublication()
        {
            if(_isCalculating || cboKyCong.SelectedValue==null || !int.TryParse(cboKyCong.SelectedValue.ToString(),out int period))return;
            try {
                using(var publication=new FrmPayrollPublication(period)) await Functions.ChildFormWindow.ShowAsync(this,publication);
                if(!IsDisposed)await LoadData();
            }catch(OperationCanceledException){}catch(Exception error){Functions.FormDataLoader.ShowLoadError(this,error);}
        }

        private async Task OpenPreparation()
        {
            if (_isCalculating) return;
            try
            {
                if (!Bu.CLASS_SYSTEM.UserSession.CanView("F_CC_BANGLUONG")) return;
                if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int period)
                    || !Bu.CLASS_PAYROLL.PayrollReadinessService.IsValidPeriod(period)) return;
                using (var workflow = new FrmPayrollWorkflow(period))
                {
                    await Functions.ChildFormWindow.ShowAsync(this, workflow);
                    if (workflow.CalculationRequested) TinhLuong(true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception error) { Functions.FormDataLoader.ShowLoadError(this, error); }
        }

        private static string BuildPolicyReadinessMessage(IEnumerable<Bu.CLASS_PAYROLL.PolicyGroupInspectionResult> inspections, int period)
        {
            var failures = inspections.Where(item => item.Status != Bu.CLASS_PAYROLL.PolicyGroupStatus.Ready).ToList();
            var message = new StringBuilder($"Chưa thể tính lương cho kỳ {period}.\n\n");
            foreach (var item in failures)
            {
                string group = item.GroupKey == "SALARY" ? "Lương" : item.GroupKey == "INSURANCE" ? "BHXH" : item.GroupKey == "UNION" ? "Công đoàn" : "Thuế TNCN";
                string reason;
                switch (item.Status)
                {
                    case Bu.CLASS_PAYROLL.PolicyGroupStatus.ObjectNotFound: reason = "Không nhìn thấy bảng hoặc thiếu quyền (ORA-00942)."; break;
                    case Bu.CLASS_PAYROLL.PolicyGroupStatus.ColumnMismatch: reason = "Cấu trúc bảng không tương thích (ORA-00904)."; break;
                    case Bu.CLASS_PAYROLL.PolicyGroupStatus.NoPolicyRecord: reason = "Chưa đủ dữ liệu chính sách."; break;
                    case Bu.CLASS_PAYROLL.PolicyGroupStatus.NoEffectivePolicy: reason = "Không có chính sách hiệu lực ở cuối kỳ đã chọn."; break;
                    case Bu.CLASS_PAYROLL.PolicyGroupStatus.ConnectionFailed: reason = "Không kết nối được Oracle."; break;
                    default: reason = "Không đọc được chính sách. Mã đối chiếu: " + item.CorrelationId; break;
                }
                message.AppendLine($"• {group}: {reason}");
            }
            message.AppendLine();
            if (failures.Any(item => item.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.ObjectNotFound))
                message.AppendLine("Kiểm tra đúng kết nối/schema và quyền SELECT trước. Chỉ chạy migration khi DBA xác nhận thiếu bảng.");
            if (failures.Any(item => item.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.NoPolicyRecord || item.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.NoEffectivePolicy))
                message.AppendLine("Mở Cấu hình lương để kiểm tra chính sách của kỳ đã chọn.");
            if (failures.Any(item => item.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.ColumnMismatch))
                message.AppendLine("Đối chiếu phiên bản cấu trúc database với mã nguồn.");
            if (failures.Any(item => item.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.ConnectionFailed))
                message.AppendLine("Kiểm tra kết nối và dịch vụ Oracle; migration không sửa được lỗi kết nối.");
            return message.ToString().TrimEnd();
        }

        private async void TinhLuong(bool skipPreparation = false)
        {
            if (_isCalculating) return;
            if (!Bu.CLASS_SYSTEM.UserSession.IsLoggedIn)
            {
                XtraMessageBox.Show("Vui lòng đăng nhập để tính lương.", "Thông báo");
                return;
            }
            if (cboKyCong.SelectedValue == null || !int.TryParse(cboKyCong.SelectedValue.ToString(), out int period)
                || period / 100 < 2000 || period / 100 > 2100 || period % 100 < 1 || period % 100 > 12)
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ!", "Thông báo");
                return;
            }
            if (!Bu.CLASS_SYSTEM.UserSession.CanEdit("F_CC_BANGLUONG"))
            {
                XtraMessageBox.Show("Cần được cấp quyền sửa Bảng lương để thực hiện tính lương.", "Phân quyền");
                return;
            }
            int actorId = (int)Bu.CLASS_SYSTEM.UserSession.CurrentUser.IDUSER;
            DateTime policyDate = new DateTime(period / 100, period % 100, DateTime.DaysInMonth(period / 100, period % 100));
            bool splashShown = false;
            _isCalculating = true;
            try
            {
                if (!skipPreparation)
                {
                    using (var workflow = new FrmPayrollWorkflow(period))
                    {
                        await Functions.ChildFormWindow.ShowAsync(this, workflow);
                        if (!workflow.CalculationRequested) return;
                    }
                }
                var readiness = await Functions.FormDataLoader.ReadAsync(this, () => new Bu.CLASS_PAYROLL.PayrollReadinessService().Inspect(period));
                if (!readiness.CanCalculate)
                {
                    XtraMessageBox.Show("Dữ liệu đầu vào chưa đủ hoặc vừa thay đổi. Mở Quy trình tính lương để xử lý các mục còn thiếu.", "Chưa đủ điều kiện tính lương", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    using (var workflow = new FrmPayrollWorkflow(period)) await Functions.ChildFormWindow.ShowAsync(this, workflow);
                    return;
                }
                SplashScreenManager.ShowForm(this, typeof(FrmWaiting), true, true, ParentFormState.Unlocked);
                splashShown = true;
                SplashScreenManager.Default.SetWaitFormCaption("Tính lương và phụ cấp");
                Action<int, int, string> progress = (current, total, text) =>
                {
                    int percent = Math.Max(0, Math.Min(100, total > 0 ? current * 100 / total : current));
                    if (!IsDisposed && IsHandleCreated)
                    {
                        try { BeginInvoke(new Action(() => {
                            if (IsDisposed || !splashShown) return;
                            try {
                                SplashScreenManager.Default.SetWaitFormDescription($"{text} ({percent}%)");
                                SplashScreenManager.Default.SendCommand(FrmWaiting.WaitFormCommand.SetProgress, percent);
                            } catch (InvalidOperationException) { }
                        })); } catch (InvalidOperationException) { }
                    }
                };
                await Functions.FormDataLoader.RunAsync(this, () => new BANGLUONG().TinhLuongKyCong(period, actorId, progress));
                SplashScreenManager.CloseForm();
                splashShown = false;
                await LoadData();
                if (IsDisposed || Disposing) return;
                XtraMessageBox.Show("Đã tính lương kỳ " + period + ". Mở Chi tiết lương để đối chiếu công, phụ cấp, bảo hiểm và thuế trước khi duyệt/chi trả. Kết quả tính chưa được coi là đã chi trả.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                if (splashShown) { try { SplashScreenManager.CloseForm(); } catch { } splashShown = false; }
                if (!IsDisposed && !Disposing)
                {
                    string text = Bu.CLASS_SYSTEM.ErrorHelper.ResolveUserFriendlyMessage(error, "tính lương", out string reference);
                    XtraMessageBox.Show(text, "Lỗi tính lương", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                _isCalculating = false;
                if (splashShown) { try { SplashScreenManager.CloseForm(); } catch { } }
            }
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gvBangLuong.RowCount > 0)
            {
                gcBangLuong.ShowPrintPreview();
            }
            else
            {
                XtraMessageBox.Show("Không có dữ liệu để in!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private async void cboKyCong_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                await LoadData();
                if (IsDisposed || Disposing) return;
            }
        }

        private async void btnChiTiet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await OpenChiTietLuong();
            if (IsDisposed || Disposing) return;
        }

        private async void btnPhatSinh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
            using (var frm = new FrmPhatSinhLuong())
            {
                await Functions.ChildFormWindow.ShowAsync(this, frm);
            }

            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }

        private async void gvBangLuong_DoubleClick(object sender, EventArgs e)
        {
            await OpenChiTietLuong();
            if (IsDisposed || Disposing) return;
        }

        private async Task OpenChiTietLuong()
        {
            try
            {
            var item = gvBangLuong.GetFocusedRow() as Bu.DTO.BANGLUONG_DTO;
            if (item != null)
            {
                using (var frm = new FrmChiTietLuong(item))
                {
                    await Functions.ChildFormWindow.ShowAsync(this, frm);
                }
            }
            else
            {
                XtraMessageBox.Show("Vui lòng chọn một nhân viên trong bảng lương để xem chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Functions.FormDataLoader.ShowLoadError(this, ex); }
        }
    }
}
