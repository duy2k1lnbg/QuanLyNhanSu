using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmCauHinhLuong : DevExpress.XtraEditors.XtraForm
    {
        private bool _hasPolicySchema = false;

        private bool _isDirty = false;
        private bool _isProcessing = false;

        public FrmCauHinhLuong()
        {
            InitializeComponent();
            this.FormClosing += FrmCauHinhLuong_FormClosing;
        }

        private void FrmCauHinhLuong_Load(object sender, EventArgs e)
        {
            LoadData();
            gvKhoanLuong.CellValueChanged += (s, ev) => { _isDirty = true; };
            gvKhoanLuong.ShowingEditor += (s, ev) =>
            {
                var item = gvKhoanLuong.GetFocusedRow() as KhoanLuongConfigItem;
                if (item == null || gvKhoanLuong.FocusedColumn != colTenKhoan || string.IsNullOrEmpty(item.MaKhoan) || !item.MaKhoan.StartsWith("PC_"))
                {
                    ev.Cancel = true;
                }
            };
            Functions.TranslationManager.Translate(this);
        }

        private void FrmCauHinhLuong_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty && this.DialogResult != DialogResult.OK)
            {
                var ask = XtraMessageBox.Show("Có thay đổi cấu hình chưa được lưu. Bạn có chắc chắn muốn đóng form không?", "Cảnh báo dữ liệu chưa lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void LoadData()
        {
            try
            {
                // 1. Khoản Lương & Phụ cấp
                var listKhoan = new List<KhoanLuongConfigItem>
                {
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "LUONG_CONG_VIEC",
                        TenKhoan = "Lương thỏa thuận theo hợp đồng",
                        Nhom = "Thu nhập",
                        CachTinh = "Theo ngày công thực tế",
                        DonVi = "đồng/tháng",
                        TinhBHXH = "Có",
                        TinhThue = "Có",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "TIEN_AN_CA",
                        TenKhoan = "Tiền ăn ca / Trợ cấp cơm trưa",
                        Nhom = "Thu nhập",
                        CachTinh = "Theo ngày công thực tế",
                        DonVi = "đồng/tháng",
                        TinhBHXH = "Không",
                        TinhThue = "Có (miễn trừ tối đa 730k)",
                        MienThue = 730000,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "TIEN_CHUYENCAN",
                        TenKhoan = "Tiền thưởng chuyên cần",
                        Nhom = "Thu nhập",
                        CachTinh = "Cố định tháng đủ công",
                        DonVi = "đồng/tháng",
                        TinhBHXH = "Không",
                        TinhThue = "Có",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "TIEN_TANGCA",
                        TenKhoan = "Tiền làm thêm giờ (Tăng ca OT)",
                        Nhom = "Thu nhập",
                        CachTinh = "Theo giờ OT thực tế x Hệ số",
                        DonVi = "đồng/giờ",
                        TinhBHXH = "Không",
                        TinhThue = "Miễn thuế phần chênh lệch",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "LUONG_CA_DEM",
                        TenKhoan = "Lương làm thêm ca đêm (+30%)",
                        Nhom = "Thu nhập",
                        CachTinh = "Theo công làm đêm x 130%",
                        DonVi = "đồng/ngày",
                        TinhBHXH = "Không",
                        TinhThue = "Có (Miễn thuế phần phụ cấp ca đêm)",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "BHXH_NLD",
                        TenKhoan = "Bảo hiểm Xã hội (8%)",
                        Nhom = "Khấu trừ",
                        CachTinh = "8% Lương căn cứ đóng BH",
                        DonVi = "%",
                        TinhBHXH = "Không",
                        TinhThue = "Giảm trừ tính thuế",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "BHYT_NLD",
                        TenKhoan = "Bảo hiểm Y tế (1.5%)",
                        Nhom = "Khấu trừ",
                        CachTinh = "1.5% Lương căn cứ đóng BH",
                        DonVi = "%",
                        TinhBHXH = "Không",
                        TinhThue = "Giảm trừ tính thuế",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "BHTN_NLD",
                        TenKhoan = "Bảo hiểm Thất nghiệp (1%)",
                        Nhom = "Khấu trừ",
                        CachTinh = "1% Lương căn cứ đóng BH",
                        DonVi = "%",
                        TinhBHXH = "Không",
                        TinhThue = "Giảm trừ tính thuế",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "DOAN_PHI_NLD",
                        TenKhoan = "Đoàn phí Công đoàn (1%)",
                        Nhom = "Khấu trừ",
                        CachTinh = "1% Lương căn cứ đóng BH",
                        DonVi = "%",
                        TinhBHXH = "Không",
                        TinhThue = "Không",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "THUE_TNCN",
                        TenKhoan = "Thuế thu nhập cá nhân",
                        Nhom = "Khấu trừ",
                        CachTinh = "Biểu lũy tiến từng phần",
                        DonVi = "đồng/kỳ",
                        TinhBHXH = "Không",
                        TinhThue = "Không",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    },
                    new KhoanLuongConfigItem
                    {
                        MaKhoan = "TIEN_TAMUNG",
                        TenKhoan = "Khấu trừ tạm ứng lương",
                        Nhom = "Khấu trừ",
                        CachTinh = "Theo phiếu tạm ứng đã duyệt",
                        DonVi = "đồng/lần",
                        TinhBHXH = "Không",
                        TinhThue = "Không",
                        MienThue = 0,
                        TrangThai = "Mặc định hệ thống (Chỉ đọc)"
                    }
                };

                // Bổ sung các phụ cấp từ TB_PHUCAP (Cho phép sửa tên khoản)
                using (var db = new MyEntities())
                {
                    var dbPcs = db.TB_PHUCAP.ToList();
                    foreach (var pc in dbPcs)
                    {
                        if (!listKhoan.Any(k => k.MaKhoan == "PC_" + pc.IDPC))
                        {
                            listKhoan.Add(new KhoanLuongConfigItem
                            {
                                MaKhoan = "PC_" + pc.IDPC,
                                TenKhoan = pc.TENPC,
                                Nhom = "Thu nhập",
                                CachTinh = "Cố định / Theo ngày công",
                                DonVi = "đồng",
                                TinhBHXH = "Theo quy chế hợp đồng",
                                TinhThue = "Có",
                                MienThue = 0,
                                TrangThai = "Đang áp dụng (Có thể sửa tên)"
                            });
                        }
                    }
                }

                gcKhoanLuong.DataSource = listKhoan;

                // Cấu hình GridView: chỉ cho sửa cột TenKhoan đối với các dòng phụ cấp
                colMaKhoan.OptionsColumn.AllowEdit = false;
                colNhom.OptionsColumn.AllowEdit = false;
                colCachTinh.OptionsColumn.AllowEdit = false;
                colDonVi.OptionsColumn.AllowEdit = false;
                colTinhBHXH.OptionsColumn.AllowEdit = false;
                colTinhThue.OptionsColumn.AllowEdit = false;
                colMienThue.OptionsColumn.AllowEdit = false;
                colTrangThai.OptionsColumn.AllowEdit = false;

                // 2. Quy Tắc Căn Cứ
                var listQuyTac = new List<QuyTacConfigItem>
                {
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Tăng ca ngày làm việc bình thường",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Đơn giá giờ chuẩn x 150% (Hệ số: 1.5)",
                        HieuLuc = "Luật Lao động 2019"
                    },
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Tăng ca ngày nghỉ hằng tuần",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Đơn giá giờ chuẩn x 200% (Hệ số: 2.0)",
                        HieuLuc = "Luật Lao động 2019"
                    },
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Tăng ca ngày lễ, tết, nghỉ có hưởng lương",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Đơn giá giờ chuẩn x 300% (Hệ số: 3.0)",
                        HieuLuc = "Luật Lao động 2019"
                    },
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Làm việc vào ban đêm (22h - 06h)",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Hệ số lương ca đêm 130% (tăng thêm ít nhất 30% lương ngày)",
                        HieuLuc = "Điều 98 Bộ luật Lao động"
                    },
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Miễn thuế TNCN đối với tiền ăn ca",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Miễn thuế tối đa 730.000 đ/tháng/người (phần vượt tính thuế TNCN)",
                        HieuLuc = "Thông tư 26/2015/TT-BTC"
                    },
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Quy tắc không phạt tiền trừ lương",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Nghiêm cấm dùng hình thức phạt tiền, cắt lương thay xử lý kỷ luật lao động",
                        HieuLuc = "Điều 127 Bộ luật Lao động"
                    },
                    new QuyTacConfigItem
                    {
                        TenQuyTac = "Loại trừ phụ cấp phúc lợi khỏi căn cứ BHXH",
                        PhamVi = "Toàn công ty",
                        NoiDung = "Tiền ăn giữa ca, hỗ trợ xăng xe, đi lại, chuyên cần không tính vào lương đóng BHXH",
                        HieuLuc = "Thông tư 59/2015/TT-BLĐTBXH"
                    }
                };
                gcQuyTac.DataSource = listQuyTac;
                gvQuyTac.OptionsBehavior.Editable = false; // Tham số quy tắc luật định chỉ đọc

                // 3. Chính Sách Lương & Tham Số Pháp Lý
                var inspections = Bu.CLASS_PAYROLL.PolicyResolver.InspectAllPolicyGroups(DateTime.Today);
                var listChinhSach = new List<ChinhSachConfigItem>();

                foreach (var insp in inspections)
                {
                    listChinhSach.Add(new ChinhSachConfigItem
                    {
                        TenChinhSach = insp.GroupName,
                        GiaTri = insp.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.Ready ? insp.ValueSummary : insp.StatusDisplay,
                        HieuLuc = insp.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.Ready ? (insp.EffectivePeriod ?? "Đang hiệu lực") : "Chưa áp dụng",
                        VanBan = insp.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.Ready ? (insp.LegalReference ?? "Quy chế công ty") : insp.ActionRequired
                    });
                }

                _hasPolicySchema = inspections.All(i => i.Status == Bu.CLASS_PAYROLL.PolicyGroupStatus.Ready);
                gcChinhSach.DataSource = listChinhSach;
                gvChinhSach.OptionsBehavior.Editable = false;
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Tải cấu hình lương");
                XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_isProcessing) return;

            gvKhoanLuong.PostEditor();
            gvKhoanLuong.UpdateCurrentRow();
            gvQuyTac.PostEditor();
            gvQuyTac.UpdateCurrentRow();
            gvChinhSach.PostEditor();
            gvChinhSach.UpdateCurrentRow();

            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
            {
                XtraMessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGLUONG"))
            {
                XtraMessageBox.Show("Bạn không có quyền chỉnh sửa cấu hình danh mục lương. Vui lòng liên hệ quản trị viên!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = (int)UserSession.CurrentUser.IDUSER;

            var listKhoan = gcKhoanLuong.DataSource as List<KhoanLuongConfigItem>;
            if (listKhoan == null) return;

            // Xác định các khoản mục phụ cấp thực sự có thay đổi tên
            var changedItems = new List<(int IdPc, string OldName, string NewName)>();
            try
            {
                using (var db = new MyEntities())
                {
                    var dbPcs = db.TB_PHUCAP.ToList();
                    foreach (var pc in dbPcs)
                    {
                        var match = listKhoan.FirstOrDefault(k => k.MaKhoan == "PC_" + pc.IDPC);
                        if (match != null && !string.IsNullOrWhiteSpace(match.TenKhoan) && match.TenKhoan.Trim() != (pc.TENPC ?? "").Trim())
                        {
                            changedItems.Add(((int)pc.IDPC, pc.TENPC, match.TenKhoan.Trim()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Kiểm tra thay đổi danh mục phụ cấp");
                XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (changedItems.Count == 0)
            {
                XtraMessageBox.Show("Không có thay đổi để lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _isDirty = false;
                return;
            }

            _isProcessing = true;
            btnLuu.Enabled = false;

            try
            {
                var buPhuCap = new Bu.CLASS_CHAMCONG.PHUCAP();
                foreach (var item in changedItems)
                {
                    buPhuCap.UpdateCatalogItem(item.IdPc, item.NewName, userId);
                }

                // Cập nhật lại cache PolicyResolver nếu có
                try
                {
                    new Bu.CLASS_PAYROLL.PolicyResolver().RefreshCache();
                }
                catch { }

                _isDirty = false;

                // Thông báo rõ ràng nội dung các phụ cấp đã được cập nhật
                var changeLines = changedItems.Select(c => $"'{c.OldName}' -> '{c.NewName}'");
                string message = $"Đã cập nhật tên {changedItems.Count} phụ cấp lương thành công:\n- {string.Join("\n- ", changeLines)}";
                XtraMessageBox.Show(message, "Lưu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Sau commit thành công, thực hiện tải lại dữ liệu ở luồng riêng
                try
                {
                    LoadData();
                }
                catch (Exception reloadEx)
                {
                    string refId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    System.Diagnostics.Trace.TraceError($"[{refId}] LoadData reload failed after save: {reloadEx}");
                    XtraMessageBox.Show($"Dữ liệu đã được lưu thành công vào cơ sở dữ liệu, nhưng gặp lỗi khi làm mới hiển thị: {reloadEx.Message} (Mã đối chiếu: {refId}).", "Cảnh báo làm mới", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // Giữ nguyên dữ liệu nhập trên lưới khi gặp lỗi lưu để người dùng không bị mất dữ liệu
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Lưu cấu hình danh mục lương");
                XtraMessageBox.Show(msg, "Lỗi lưu cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isProcessing = false;
                btnLuu.Enabled = true;
            }
        }

        private void btnLamMoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private class KhoanLuongConfigItem
        {
            public string MaKhoan { get; set; }
            public string TenKhoan { get; set; }
            public string Nhom { get; set; }
            public string CachTinh { get; set; }
            public string DonVi { get; set; }
            public string TinhBHXH { get; set; }
            public string TinhThue { get; set; }
            public decimal MienThue { get; set; }
            public string TrangThai { get; set; }
        }

        private class QuyTacConfigItem
        {
            public string TenQuyTac { get; set; }
            public string PhamVi { get; set; }
            public string NoiDung { get; set; }
            public string HieuLuc { get; set; }
        }

        private class ChinhSachConfigItem
        {
            public string TenChinhSach { get; set; }
            public string GiaTri { get; set; }
            public string HieuLuc { get; set; }
            public string VanBan { get; set; }
        }
    }
}
