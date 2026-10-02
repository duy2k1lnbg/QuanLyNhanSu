using Bu.DTO;
using Bu.CLASS_SYSTEM;
using Bu.CLASS_PAYROLL;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmDieuChinhLuong : DevExpress.XtraEditors.XtraForm
    {
        private BANGLUONG_DTO _bangLuongItem;
        private BindingList<SalaryAdjustmentItemDto> _listDieuChinh;
        private readonly SalaryAdjustmentService _adjustmentService = new SalaryAdjustmentService();
        private bool _isDirty = false;
        private bool _isProcessing = false;

        public FrmDieuChinhLuong()
        {
            InitializeComponent();
            this.FormClosing += FrmDieuChinhLuong_FormClosing;
        }

        public FrmDieuChinhLuong(BANGLUONG_DTO item) : this()
        {
            _bangLuongItem = item;
        }

        private void FrmDieuChinhLuong_Load(object sender, EventArgs e)
        {
            if (_bangLuongItem != null)
            {
                _listDieuChinh = new BindingList<SalaryAdjustmentItemDto>
                {
                    new SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Lương công thực tế",
                        IsDeduction = false,
                        GiaTriCu = _bangLuongItem.LUONG_CONG_THUCTE ?? 0,
                        GiaTriMoi = _bangLuongItem.LUONG_CONG_THUCTE ?? 0,
                        GhiChu = "Điều chỉnh lương theo ngày công"
                    },
                    new SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Tiền làm thêm giờ (OT)",
                        IsDeduction = false,
                        GiaTriCu = _bangLuongItem.TIEN_TANGCA ?? 0,
                        GiaTriMoi = _bangLuongItem.TIEN_TANGCA ?? 0,
                        GhiChu = "Bổ sung hoặc giảm trừ giờ làm thêm ngoài giờ"
                    },
                    new SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Tiền thưởng chuyên cần",
                        IsDeduction = false,
                        GiaTriCu = _bangLuongItem.TIEN_CHUYENCAN ?? 0,
                        GiaTriMoi = _bangLuongItem.TIEN_CHUYENCAN ?? 0,
                        GhiChu = "Xét thưởng chuyên cần theo phê duyệt"
                    },
                    new SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Tiền ăn ca / Cơm trưa",
                        IsDeduction = false,
                        GiaTriCu = _bangLuongItem.TIEN_AN_CA ?? 0,
                        GiaTriMoi = _bangLuongItem.TIEN_AN_CA ?? 0,
                        GhiChu = "Bù trừ hỗ trợ tiền ăn giữa ca"
                    },
                    new SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Khoản cộng phát sinh khác",
                        IsDeduction = false,
                        GiaTriCu = _bangLuongItem.KHOAN_CONG_KHAC ?? 0,
                        GiaTriMoi = _bangLuongItem.KHOAN_CONG_KHAC ?? 0,
                        GhiChu = "Thưởng thành tích hoặc hỗ trợ phát sinh"
                    },
                    new SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Khoản trừ phát sinh khác",
                        IsDeduction = true,
                        GiaTriCu = _bangLuongItem.KHOAN_TRU_KHAC ?? 0,
                        GiaTriMoi = _bangLuongItem.KHOAN_TRU_KHAC ?? 0,
                        GhiChu = "Thu hồi hoặc các khoản giảm trừ phát sinh (tăng sẽ giảm thực lĩnh)"
                    }
                };

                gcDieuChinh.DataSource = _listDieuChinh;
                gvDieuChinh.CellValueChanged += GvDieuChinh_CellValueChanged;
                UpdatePreviewSummary();
                _isDirty = false;
            }
            Functions.TranslationManager.Translate(this);
        }

        private void GvDieuChinh_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column == colGiaTriMoi)
            {
                _isDirty = true;
                gvDieuChinh.RefreshRow(e.RowHandle);
                UpdatePreviewSummary();
            }
        }

        private SalaryAdjustmentPreviewDto _latestPreview;

        private void UpdatePreviewSummary()
        {
            if (_bangLuongItem == null || _listDieuChinh == null) return;

            try
            {
                _latestPreview = _adjustmentService.PreviewAdjustment(_bangLuongItem.IDBL, _listDieuChinh.ToList());
                string sign = _latestPreview.DeltaThucLinh >= 0 ? "+" : "";
                lblThongTin.Text = $"Mã NV: {_bangLuongItem.MANV} | Họ tên: {_bangLuongItem.HOTEN} | Kỳ: {_bangLuongItem.MAKYCONG} | Tổng cộng: {_latestPreview.NewTongCong:n0} đ | Khấu trừ: {_latestPreview.NewTongKhauTru:n0} đ (Thuế: {_latestPreview.NewThueTncn:n0} đ) | Thực lĩnh mới: {_latestPreview.NewThucLinh:n0} đ ({sign}{_latestPreview.DeltaThucLinh:n0} đ)";
            }
            catch (Exception ex)
            {
                _latestPreview = null;
                string friendly = ErrorHelper.ResolveUserFriendlyMessage(ex, "Xem trước điều chỉnh lương", out string correlationId);
                lblThongTin.Text = $"Lỗi xem trước: {friendly} (Mã đối chiếu: {correlationId})";
                System.Diagnostics.Debug.WriteLine($"[FrmDieuChinhLuong] Preview error: {ex}");
            }
        }

        private void btnLuuDieuChinh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_isProcessing) return;

            // Flush any active in-cell editor
            gvDieuChinh.PostEditor();
            gvDieuChinh.UpdateCurrentRow();

            if (string.IsNullOrWhiteSpace(txtLyDo.Text))
            {
                XtraMessageBox.Show("Vui lòng nhập lý do điều chỉnh lương bắt buộc!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLyDo.Focus();
                return;
            }

            bool coThayDoi = _listDieuChinh != null && _listDieuChinh.Any(x => x.ChenhLech != 0);
            if (!coThayDoi)
            {
                XtraMessageBox.Show("Không có khoản mục nào thay đổi giá trị!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
            {
                XtraMessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = (int)UserSession.CurrentUser.IDUSER;

            SalaryAdjustmentPreviewDto preview;
            try
            {
                preview = _adjustmentService.PreviewAdjustment(_bangLuongItem.IDBL, _listDieuChinh.ToList());
                _latestPreview = preview;
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Xem trước điều chỉnh lương");
                XtraMessageBox.Show(msg, "Lỗi xem trước", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (preview == null)
            {
                XtraMessageBox.Show("Dữ liệu xem trước không hợp lệ hoặc tính toán bị lỗi. Vui lòng kiểm tra lại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dialog = XtraMessageBox.Show(
                $"Xác nhận ghi nhận điều chỉnh lương cho nhân viên {_bangLuongItem?.HOTEN}?\n" +
                $"- Thay đổi tổng thu nhập: {(preview.DeltaTongCong >= 0 ? "+" : "")}{preview.DeltaTongCong:n0} đ\n" +
                $"- Thay đổi tổng khấu trừ: {(preview.DeltaTongKhauTru >= 0 ? "+" : "")}{preview.DeltaTongKhauTru:n0} đ (Thuế: {preview.NewThueTncn:n0} đ)\n" +
                $"- Ảnh hưởng thực lĩnh: {(preview.DeltaThucLinh >= 0 ? "+" : "")}{preview.DeltaThucLinh:n0} đ (Thực lĩnh mới: {preview.NewThucLinh:n0} đ)\n" +
                $"- Lý do: {txtLyDo.Text.Trim()}",
                "Xác nhận lưu điều chỉnh",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dialog != DialogResult.Yes) return;

            _isProcessing = true;
            btnLuuDieuChinh.Enabled = false;

            try
            {
                string soChungTu = txtChungTu?.Text?.Trim();
                var result = _adjustmentService.ApplyAdjustment(
                    _bangLuongItem.IDBL,
                    _listDieuChinh.ToList(),
                    txtLyDo.Text.Trim(),
                    soChungTu,
                    userId,
                    preview.DataVersionToken
                );

                if (result.Success)
                {
                    if (result.UpdatedBangLuong != null)
                    {
                        _bangLuongItem.LUONG_CONG_THUCTE = result.UpdatedBangLuong.LUONG_CONG_THUCTE;
                        _bangLuongItem.TIEN_TANGCA = result.UpdatedBangLuong.TIEN_TANGCA;
                        _bangLuongItem.TIEN_CHUYENCAN = result.UpdatedBangLuong.TIEN_CHUYENCAN;
                        _bangLuongItem.TIEN_AN_CA = result.UpdatedBangLuong.TIEN_AN_CA;
                        _bangLuongItem.PHUCAP_CONG_THUCTE = result.UpdatedBangLuong.PHUCAP_CONG_THUCTE;
                        _bangLuongItem.KHOAN_CONG_KHAC = result.UpdatedBangLuong.KHOAN_CONG_KHAC;
                        _bangLuongItem.KHOAN_TRU_KHAC = result.UpdatedBangLuong.KHOAN_TRU_KHAC;
                        _bangLuongItem.THUE_TNCN = result.UpdatedBangLuong.THUE_TNCN;
                        _bangLuongItem.TONG_CONG = result.UpdatedBangLuong.TONG_CONG;
                        _bangLuongItem.THUC_LINH = result.UpdatedBangLuong.THUC_LINH;
                    }
                    else
                    {
                        _bangLuongItem.TONG_CONG = result.NewTongCong;
                        _bangLuongItem.THUC_LINH = result.NewThucLinh;
                    }

                    _isDirty = false;
                    XtraMessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                // Giữ nguyên dữ liệu nhập và chế độ sửa, không đóng dialog, không reset form
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Lưu điều chỉnh lương");
                XtraMessageBox.Show(msg, "Lỗi lưu điều chỉnh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isProcessing = false;
                btnLuuDieuChinh.Enabled = true;
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void FrmDieuChinhLuong_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty && this.DialogResult != DialogResult.OK)
            {
                var ask = XtraMessageBox.Show("Có thay đổi điều chỉnh lương chưa được lưu. Bạn có chắc chắn muốn đóng form không?", "Cảnh báo dữ liệu chưa lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
