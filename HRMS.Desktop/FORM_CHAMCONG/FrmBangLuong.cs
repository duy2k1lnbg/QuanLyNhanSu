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

        public FrmBangLuong()
        {
            InitializeComponent();
        }

        private void FrmBangLuong_Load(object sender, EventArgs e)
        {
            _bangluong = new BANGLUONG();
            _kycong = new KYCONG();
            ConfigureGrid();
            LoadCombo();
            LoadData();
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

        private void LoadCombo()
        {
            cboKyCong.DataSource = _kycong.getList();
            cboKyCong.DisplayMember = "MAKYCONG";
            cboKyCong.ValueMember = "MAKYCONG";
        }

        private void LoadData()
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                var kc = _kycong.getItem(makycong);
                bool isKhoa = kc != null && (kc.KHOA ?? 0) == 1;
                btnTinhLuong.Enabled = !isKhoa;
                btnTinhLuongCong.Enabled = !isKhoa;

                gcBangLuong.DataSource = _bangluong.getList(makycong);
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

        private void btnTinhLuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TinhLuong();
        }

        private void btnTinhLuongCong_Click(object sender, EventArgs e)
        {
            TinhLuong();
        }

        private async void TinhLuong()
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                var kc = _kycong.getItem(makycong);
                if (kc != null && (kc.KHOA ?? 0) == 1)
                {
                    XtraMessageBox.Show($"Kỳ công {makycong} đã bị khoá sổ. Khi đã khoá bảng công thì không cho phép tính lại lương!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra trước tính sẵn sàng của schema bảng chính sách tính lương (TB_CHINH_SACH_LUONG, ...)
                if (!Bu.CLASS_PAYROLL.PolicyResolver.IsPolicySchemaAvailable(out string missingDetails))
                {
                    XtraMessageBox.Show(
                        $"Không thể thực hiện tính lương do cơ sở dữ liệu chưa được khởi tạo bảng chính sách ({missingDetails ?? "TB_CHINH_SACH_LUONG"}).\n\n" +
                        "BẠN CẦN LÀM GÌ TRƯỚC:\n" +
                        "1. Quản trị viên (DBA) cần chạy script migration V1_16:\n" +
                        "   'database/migrations/V1_16__payroll_production_policies_and_itemized_details.sql'\n" +
                        "   (hoặc file 'apply_payroll_v1_16_objects.sql') để tạo các bảng chính sách và nạp cấu hình mặc định.\n" +
                        "2. Kiểm tra quyền truy cập (SELECT, INSERT, UPDATE) của tài khoản kết nối trên schema Oracle.\n" +
                        "3. Mở chức năng 'Cấu hình lương' để kiểm tra lại các tham số lương, BHXH và thuế TNCN trước khi tính lương.",
                        "Cơ sở dữ liệu chưa sẵn sàng",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                try
                {
                    SplashScreenManager.ShowForm(this, typeof(FrmWaiting), true, true, ParentFormState.Locked);
                    SplashScreenManager.Default.SetWaitFormCaption("Tính Lương & Phụ Cấp");
                    SplashScreenManager.Default.SetWaitFormDescription("Đang chuẩn bị dữ liệu... (0%)");
                    SplashScreenManager.Default.SendCommand(FrmWaiting.WaitFormCommand.SetProgress, 0);

                    Action<int, int, string> onProgress = (current, total, msg) =>
                    {
                        int percent = total > 0 ? (int)((double)current / total * 100) : current;
                        if (percent < 0) percent = 0;
                        if (percent > 100) percent = 100;
                        try
                        {
                            SplashScreenManager.Default.SetWaitFormDescription($"{msg} ({percent}%)");
                            SplashScreenManager.Default.SendCommand(FrmWaiting.WaitFormCommand.SetProgress, percent);
                        }
                        catch { }
                    };

                    await Task.Run(() =>
                    {
                        _bangluong.TinhLuongKyCong(makycong, 1, onProgress);
                    });

                    LoadData();
                    SplashScreenManager.CloseForm();
                    XtraMessageBox.Show("Tính lương thành công cho kỳ công " + makycong + "!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    SplashScreenManager.CloseForm();
                    string friendlyMsg = Bu.CLASS_SYSTEM.ErrorHelper.ResolveUserFriendlyMessage(ex, "tính lương", out string correlationId);
                    XtraMessageBox.Show(friendlyMsg, "Lỗi tính lương", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                XtraMessageBox.Show("Vui lòng chọn kỳ công hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void cboKyCong_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKyCong.SelectedValue != null && int.TryParse(cboKyCong.SelectedValue.ToString(), out int makycong))
            {
                LoadData();
            }
        }

        private void btnChiTiet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenChiTietLuong();
        }

        private void btnPhatSinh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var frm = new FrmPhatSinhLuong())
            {
                frm.ShowDialog(this);
            }
        }

        private void gvBangLuong_DoubleClick(object sender, EventArgs e)
        {
            OpenChiTietLuong();
        }

        private void OpenChiTietLuong()
        {
            var item = gvBangLuong.GetFocusedRow() as Bu.DTO.BANGLUONG_DTO;
            if (item != null)
            {
                using (var frm = new FrmChiTietLuong(item))
                {
                    frm.ShowDialog(this);
                }
            }
            else
            {
                XtraMessageBox.Show("Vui lòng chọn một nhân viên trong bảng lương để xem chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}