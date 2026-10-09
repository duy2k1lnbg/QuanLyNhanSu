using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public class SalaryAdjustmentItemDto
    {
        public string KhoanMuc { get; set; }
        public bool IsDeduction { get; set; }
        public decimal GiaTriCu { get; set; }
        public decimal GiaTriMoi { get; set; }
        public decimal ChenhLech => GiaTriMoi - GiaTriCu;
        public decimal AnhHuongThucLinh => IsDeduction ? -ChenhLech : ChenhLech;
        public string GhiChu { get; set; }
    }

    public class SalaryAdjustmentPreviewDto
    {
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public decimal OldTongCong { get; set; }
        public decimal NewTongCong { get; set; }
        public decimal DeltaTongCong => NewTongCong - OldTongCong;
        public decimal OldTongKhauTru { get; set; }
        public decimal NewTongKhauTru { get; set; }
        public decimal DeltaTongKhauTru => NewTongKhauTru - OldTongKhauTru;
        public decimal OldThueTncn { get; set; }
        public decimal NewThueTncn { get; set; }
        public decimal DeltaThueTncn => NewThueTncn - OldThueTncn;
        public decimal OldThucLinh { get; set; }
        public decimal NewThucLinh { get; set; }
        public decimal DeltaThucLinh => NewThucLinh - OldThucLinh;
        public decimal GrossTaxableIncome { get; set; }
        public decimal TaxableAssessableIncome { get; set; }
        public decimal PersonalDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public string DataVersionToken { get; set; }
        public List<SalaryAdjustmentItemDto> Items { get; set; } = new List<SalaryAdjustmentItemDto>();
        public List<PayrollTaxTraceDto> TaxBracketTraces { get; set; } = new List<PayrollTaxTraceDto>();
    }

    public class SalaryAdjustmentResultDto
    {
        public bool Success { get; set; }
        public decimal IDBL { get; set; }
        public decimal NewThucLinh { get; set; }
        public decimal NewTongCong { get; set; }
        public string SoChungTu { get; set; }
        public string DataVersionToken { get; set; }
        public string Message { get; set; }
        public BANGLUONG_DTO UpdatedBangLuong { get; set; }
    }

    public class TaxSnapshotRow
    {
        public decimal TONG_THU_NHAP_CHIU_THUE { get; set; }
        public decimal GIAM_TRU_BAO_HIEM { get; set; }
        public decimal SO_NGUOI_PHU_THUOC { get; set; }
    }

    public class BangLuongCtRow
    {
        public decimal IDBLCT { get; set; }
        public decimal IDBL { get; set; }
        public string MA_KHOAN_MUC { get; set; }
        public string TEN_KHOAN_MUC { get; set; }
        public string NHOM_KHOAN_MUC { get; set; }
        public decimal? THANH_TIEN { get; set; }
        public decimal? DON_GIA { get; set; }
        public decimal? SO_LUONG { get; set; }
        public decimal? HE_SO { get; set; }
        public decimal? TINH_THUE_TNCN { get; set; }
        public decimal? SO_TIEN_CHIU_THUE { get; set; }
        public decimal? SO_TIEN_MIEN_THUE { get; set; }
    }

    public class PayrollStatusCheckRow
    {
        public string TRANG_THAI { get; set; }
        public string TRANGTHAI_CHITRA { get; set; }
    }

    public class SalaryAdjustmentService
    {
        private readonly IPolicyResolver _policyResolver;

        public SalaryAdjustmentService(IPolicyResolver policyResolver = null)
        {
            _policyResolver = policyResolver;
        }

        public static string ComputeDataVersionToken(TB_BANGLUONG bl)
        {
            if (bl == null) return string.Empty;
            return $"{bl.IDBL}_{bl.TONG_CONG ?? 0m}_{bl.THUC_LINH ?? 0m}_{bl.THUE_TNCN ?? 0m}_{bl.KHOAN_TRU_KHAC ?? 0m}_{bl.LUONG_CONG_THUCTE ?? 0m}_{bl.PHUCAP_CONG_THUCTE ?? 0m}_{bl.TIEN_TANGCA ?? 0m}_{bl.TIEN_AN_CA ?? 0m}_{bl.KHOAN_CONG_KHAC ?? 0m}";
        }

        public SalaryAdjustmentPreviewDto PreviewAdjustment(decimal idbl, List<SalaryAdjustmentItemDto> items)
        {
            if (idbl <= 0)
                throw new BusinessException("INVALID_ID", "Mã bảng lương không hợp lệ.");

            if (items == null || items.Count == 0)
                throw new BusinessException("EMPTY_ITEMS", "Danh sách khoản mục điều chỉnh không được để trống.");

            using (var db = new MyEntities())
            {
                var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.IDBL == idbl);
                if (bl == null)
                    throw new BusinessException("NOT_FOUND", $"Không tìm thấy bảng lương mã {idbl}.");

                // Lấy danh sách chi tiết hiện có của bảng lương để phân bổ chính xác theo từng khoản mục
                List<BangLuongCtRow> existingDetails;
                try
                {
                    existingDetails = db.Database.SqlQuery<BangLuongCtRow>(@"
                        SELECT IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                               SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                               SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI
                        FROM TB_BANGLUONG_CT
                        WHERE IDBL = :p0",
                        new OracleParameter("p0", bl.IDBL)
                    ).ToList();
                }
                catch
                {
                    existingDetails = new List<BangLuongCtRow>();
                }

                decimal oldLuong = bl.LUONG_CONG_THUCTE ?? 0m;
                decimal oldTangCa = bl.TIEN_TANGCA ?? 0m;
                decimal oldChuyenCan = bl.TIEN_CHUYENCAN ?? 0m;
                decimal oldAnCa = bl.TIEN_AN_CA ?? 0m;
                decimal oldPhuCap = bl.PHUCAP_CONG_THUCTE ?? 0m;
                decimal oldKhoanCongKhac = bl.KHOAN_CONG_KHAC ?? 0m;
                decimal oldKhoanTruKhac = bl.KHOAN_TRU_KHAC ?? 0m;
                decimal oldThue = bl.THUE_TNCN ?? 0m;
                decimal oldTongCong = bl.TONG_CONG ?? 0m;
                decimal oldKhauTru = (bl.TIEN_BHXH_TRICH ?? 0m) + (bl.TIEN_CONG_DOAN ?? 0m) + (bl.TIEN_TAMUNG ?? 0m) + oldThue + oldKhoanTruKhac;
                decimal oldThucLinh = bl.THUC_LINH ?? 0m;

                decimal newLuong = items.FirstOrDefault(x => x.KhoanMuc == "Lương công thực tế")?.GiaTriMoi ?? oldLuong;
                decimal newTangCa = items.FirstOrDefault(x => x.KhoanMuc == "Tiền làm thêm giờ (OT)")?.GiaTriMoi ?? oldTangCa;
                decimal newChuyenCan = items.FirstOrDefault(x => x.KhoanMuc == "Tiền thưởng chuyên cần")?.GiaTriMoi ?? oldChuyenCan;
                decimal newAnCa = items.FirstOrDefault(x => x.KhoanMuc == "Tiền ăn ca / Cơm trưa")?.GiaTriMoi ?? oldAnCa;
                decimal newKhoanCongKhac = items.FirstOrDefault(x => x.KhoanMuc == "Khoản cộng phát sinh khác")?.GiaTriMoi ?? oldKhoanCongKhac;
                decimal newKhoanTruKhac = items.FirstOrDefault(x => x.KhoanMuc == "Khoản trừ phát sinh khác")?.GiaTriMoi ?? oldKhoanTruKhac;

                // Cập nhật phụ cấp theo chênh lệch chuyên cần và tiền ăn (tránh cộng trùng lặp)
                decimal deltaChuyenCan = newChuyenCan - oldChuyenCan;
                decimal deltaAnCa = newAnCa - oldAnCa;
                decimal newPhuCap = Math.Max(0m, oldPhuCap + deltaChuyenCan + deltaAnCa);

                // Tổng cộng thu nhập sau điều chỉnh: Lương + Tăng ca + Phụ cấp (đã gồm CC + Ăn ca) + Khoản cộng khác
                decimal newTongCong = newLuong + newTangCa + newPhuCap + newKhoanCongKhac;

                // Xác định thời điểm và chính sách thuế áp dụng cho kỳ
                int makycong = (int)bl.MAKYCONG;
                int nam = (bl.NAM > 2000) ? (int)bl.NAM : ((int)bl.MAKYCONG / 100);
                int thang = (bl.THANG >= 1 && bl.THANG <= 12) ? (int)bl.THANG : ((int)bl.MAKYCONG % 100);
                if (thang < 1 || thang > 12) thang = 1;
                if (nam < 2000) nam = DateTime.Now.Year;
                DateTime effDate = new DateTime(nam, thang, 1);

                var policyResolver = _policyResolver;
                TaxPolicyDto taxPolicy = null;
                try
                {
                    if (policyResolver == null)
                    {
                        policyResolver = new PolicyResolver();
                    }
                    taxPolicy = policyResolver.GetTaxPolicy(nam, effDate);
                }
                catch (PolicySchemaMissingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    string refId = $"REF-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
                    System.Diagnostics.Trace.TraceError($"[{refId}] GetTaxPolicy failed: {ex}");
                    throw new BusinessException("TAX_POLICY_MISSING", $"Không thể xác định chính sách thuế TNCN năm {nam} do lỗi truy vấn (Mã đối chiếu: {refId}).");
                }

                // Tiền ăn ca: Mức miễn thuế căn cứ theo chính sách (hoặc hạn mức luật định 730k)
                decimal mealExemptCap = taxPolicy != null ? taxPolicy.MUC_MIEN_THUE_AN_CA : 730000m;
                decimal oldTaxableMeal = Math.Max(0m, oldAnCa - mealExemptCap);
                decimal newTaxableMeal = Math.Max(0m, newAnCa - mealExemptCap);
                decimal deltaMealTaxable = newTaxableMeal - oldTaxableMeal;

                // Tiền làm thêm giờ (OT): Không chia hệ số cố định 1.5!
                // Tính dựa trên tỷ lệ chịu thuế thực tế do engine chấm công ghi nhận hoặc căn cứ giờ OT
                decimal deltaTangCa = newTangCa - oldTangCa;
                decimal deltaOtTaxable = 0m;
                var otRow = existingDetails.FirstOrDefault(x => x.MA_KHOAN_MUC == "TIEN_TANG_CA" || x.NHOM_KHOAN_MUC == "TANG_CA");
                if (deltaTangCa != 0)
                {
                    if (otRow != null && otRow.THANH_TIEN.HasValue && otRow.THANH_TIEN.Value > 0)
                    {
                        decimal existingTaxable = otRow.SO_TIEN_CHIU_THUE ?? 0m;
                        decimal taxableRatio = existingTaxable / otRow.THANH_TIEN.Value;
                        decimal newOtTaxable = Math.Round(newTangCa * taxableRatio, 2);
                        deltaOtTaxable = newOtTaxable - existingTaxable;
                    }
                    else if (oldTangCa == 0m && newTangCa > 0m)
                    {
                        throw new BusinessException("INSUFFICIENT_OT_SOURCE", "Không thể điều chỉnh trực tiếp tiền làm thêm giờ (OT) khi chưa có căn cứ chấm công tăng ca gốc. Vui lòng điều chỉnh qua phân hệ Chấm công / Tăng ca để hệ thống phân bổ miễn thuế và kiểm tra trần giờ theo quy định.");
                    }
                    else
                    {
                        deltaOtTaxable = deltaTangCa;
                    }
                }

                // Khoản cộng phát sinh khác: Phân tách rõ trợ cấp làm đêm (miễn thuế theo NĐ 253/2026) và tiền thưởng (chịu thuế)
                var nightRow = existingDetails.FirstOrDefault(x => x.MA_KHOAN_MUC == "PHUCAP_LAM_DEM");
                decimal nightAllowance = nightRow?.THANH_TIEN ?? 0m;

                if (newKhoanCongKhac < nightAllowance)
                {
                    throw new BusinessException("INVALID_ADJUSTMENT", $"Giá trị khoản cộng phát sinh ({newKhoanCongKhac:N0} đ) không được nhỏ hơn phụ cấp làm đêm ({nightAllowance:N0} đ). Vui lòng điều chỉnh qua bảng chấm công nếu muốn thay đổi công làm đêm.");
                }

                decimal oldRewards = Math.Max(0m, oldKhoanCongKhac - nightAllowance);
                decimal newRewards = Math.Max(0m, newKhoanCongKhac - nightAllowance);
                decimal deltaRewardsTaxable = newRewards - oldRewards;

                decimal deltaLuong = newLuong - oldLuong;
                decimal deltaTaxable = deltaLuong + deltaChuyenCan + deltaRewardsTaxable + deltaMealTaxable + deltaOtTaxable;

                decimal newThue = oldThue;
                TaxCalculationResult taxResult = null;

                // Lấy snapshot căn cứ thuế lịch sử của bảng lương; fallback nếu schema chưa migrate các cột mới
                TaxSnapshotRow taxSnap = null;
                try
                {
                    taxSnap = db.Database.SqlQuery<TaxSnapshotRow>(@"
                        SELECT NVL(TONG_THU_NHAP_CHIU_THUE, 0) AS TONG_THU_NHAP_CHIU_THUE,
                               NVL(GIAM_TRU_BAO_HIEM, 0) AS GIAM_TRU_BAO_HIEM,
                               NVL(SO_NGUOI_PHU_THUOC, 0) AS SO_NGUOI_PHU_THUOC
                        FROM TB_BANGLUONG
                        WHERE IDBL = :p0",
                        new OracleParameter("p0", bl.IDBL)
                    ).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning($"Querying tax snapshot skipped or legacy schema: {ex.Message}");
                    taxSnap = null;
                }

                // Bảo toàn số người phụ thuộc của phiên bản lịch sử; không trộn tùy tiện với hồ sơ hiện tại
                int depCount = 0;
                if (taxSnap != null && taxSnap.SO_NGUOI_PHU_THUOC > 0)
                {
                    depCount = (int)taxSnap.SO_NGUOI_PHU_THUOC;
                }
                else
                {
                    try
                    {
                        var depResolver = new EmployeeProfileResolver();
                        var dependents = depResolver.GetActiveDependents(bl.MANV, makycong);
                        depCount = dependents != null ? dependents.Count : 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning($"GetActiveDependents fallback to 0: {ex.Message}");
                        depCount = 0;
                    }
                }

                if (deltaTaxable != 0)
                {
                    try
                    {
                        if (taxPolicy == null)
                        {
                            throw new BusinessException("TAX_POLICY_MISSING", $"Không thể tính lại thuế TNCN: Không tìm thấy chính sách thuế TNCN cho năm {nam}. Vui lòng cấu hình chính sách thuế trước khi điều chỉnh.");
                        }

                        var taxBrackets = policyResolver.GetTaxBrackets(taxPolicy.ID, "MONTH");
                        if (taxBrackets == null || taxBrackets.Count == 0)
                        {
                            throw new BusinessException("TAX_POLICY_MISSING", $"Không thể tính lại thuế TNCN: Biểu thuế lũy tiến từng phần của chính sách thuế năm {nam} chưa được cấu hình.");
                        }

                        decimal baseGrossTaxable;
                        if (taxSnap != null && taxSnap.TONG_THU_NHAP_CHIU_THUE > 0)
                        {
                            baseGrossTaxable = Math.Max(0m, taxSnap.TONG_THU_NHAP_CHIU_THUE + deltaTaxable);
                        }
                        else
                        {
                            decimal exemptOt = (otRow?.SO_TIEN_MIEN_THUE ?? 0m);
                            baseGrossTaxable = Math.Max(0m, newTongCong - Math.Min(newAnCa, mealExemptCap) - nightAllowance - exemptOt);
                        }

                        decimal insuranceDeduction = (taxSnap != null && taxSnap.GIAM_TRU_BAO_HIEM > 0)
                            ? taxSnap.GIAM_TRU_BAO_HIEM
                            : (bl.TIEN_BHXH_TRICH ?? 0m);

                        var taxEngine = new TaxEngine();
                        taxResult = taxEngine.CalculateMonthlyTax(
                            bl.IDBL, bl.MANV, bl.MAKYCONG, baseGrossTaxable, depCount, insuranceDeduction, taxPolicy, taxBrackets
                        );
                        newThue = taxResult.TotalTax;
                    }
                    catch (BusinessException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        string correlationId = $"REF-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
                        System.Diagnostics.Trace.TraceError($"[{correlationId}] Tax recalculation failed: {ex}");
                        throw new BusinessException("TAX_POLICY_MISSING", $"Không thể tính lại thuế TNCN do thiếu chính sách hoặc căn cứ thuế cho năm {nam} (Mã đối chiếu: {correlationId}). Vui lòng cấu hình chính sách thuế trước khi điều chỉnh.");
                    }
                }

                // Tổng khấu trừ sau điều chỉnh (gồm bảo hiểm, công đoàn, tạm ứng, thuế TNCN và khoản khấu trừ phát sinh)
                decimal newKhauTru = (bl.TIEN_BHXH_TRICH ?? 0m) +
                                    (bl.TIEN_CONG_DOAN ?? 0m) +
                                    (bl.TIEN_TAMUNG ?? 0m) +
                                    newThue +
                                    newKhoanTruKhac;

                // Thực lĩnh sau điều chỉnh
                decimal newThucLinh = newTongCong - newKhauTru + (bl.HOAN_THUE ?? 0m);

                decimal grossTaxable = taxResult != null
                    ? taxResult.GrossTaxableIncome
                    : (taxSnap != null && taxSnap.TONG_THU_NHAP_CHIU_THUE > 0 ? taxSnap.TONG_THU_NHAP_CHIU_THUE : Math.Max(0m, oldTongCong - Math.Min(oldAnCa, mealExemptCap)));
                decimal assessableIncome = taxResult != null
                    ? taxResult.TaxableAssessableIncome
                    : Math.Max(0m, grossTaxable - (taxSnap?.GIAM_TRU_BAO_HIEM ?? (bl.TIEN_BHXH_TRICH ?? 0m)) - (taxPolicy?.GIAM_TRU_BAN_THAN_THANG ?? 15500000m) - (depCount * (taxPolicy?.GIAM_TRU_PHU_THUOC_THANG ?? 6200000m)));

                return new SalaryAdjustmentPreviewDto
                {
                    IDBL = bl.IDBL,
                    MANV = bl.MANV,
                    MAKYCONG = bl.MAKYCONG,
                    OldTongCong = oldTongCong,
                    NewTongCong = newTongCong,
                    OldTongKhauTru = oldKhauTru,
                    NewTongKhauTru = newKhauTru,
                    OldThueTncn = oldThue,
                    NewThueTncn = newThue,
                    OldThucLinh = oldThucLinh,
                    NewThucLinh = newThucLinh,
                    GrossTaxableIncome = grossTaxable,
                    TaxableAssessableIncome = assessableIncome,
                    PersonalDeduction = taxResult?.PersonalDeduction ?? (taxPolicy?.GIAM_TRU_BAN_THAN_THANG ?? 15500000m),
                    DependentDeduction = taxResult?.DependentDeduction ?? (depCount * (taxPolicy?.GIAM_TRU_PHU_THUOC_THANG ?? 6200000m)),
                    InsuranceDeduction = taxResult?.InsuranceDeduction ?? (taxSnap?.GIAM_TRU_BAO_HIEM ?? (bl.TIEN_BHXH_TRICH ?? 0m)),
                    DataVersionToken = ComputeDataVersionToken(bl),
                    Items = items,
                    TaxBracketTraces = taxResult?.BracketTraces ?? new List<PayrollTaxTraceDto>()
                };
            }
        }

        public SalaryAdjustmentResultDto ApplyAdjustment(
            decimal idbl,
            List<SalaryAdjustmentItemDto> items,
            string lyDo,
            string soChungTu,
            int userId,
            string expectedDataVersionToken = null)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            int effectiveUserId = (int)UserSession.CurrentUser.IDUSER;

            if (!UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền thực hiện điều chỉnh bảng lương. Vui lòng liên hệ quản trị viên.");

            if (string.IsNullOrWhiteSpace(lyDo))
                throw new BusinessException("VALIDATION_ERROR", "Vui lòng nhập lý do điều chỉnh lương bắt buộc.", "LyDo");

            // Bắt buộc truyền expectedDataVersionToken để bảo vệ cập nhật đồng thời
            if (string.IsNullOrWhiteSpace(expectedDataVersionToken))
            {
                throw new BusinessException("VERSION_TOKEN_REQUIRED", "Mã kiểm soát phiên bản (DataVersionToken) là bắt buộc để thực hiện điều chỉnh nhằm bảo vệ tính toàn vẹn dữ liệu.");
            }

            var preview = PreviewAdjustment(idbl, items);

            BANGLUONG_DTO updated = null;
            string reloadWarning = null;
            string actualSoChungTu = null;
            string newVersionToken = null;
            decimal resultingThucLinh = 0m;
            decimal resultingTongCong = 0m;

            using (var db = new MyEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Khóa và lấy bản ghi TB_BANGLUONG
                    var bl = db.TB_BANGLUONG.SqlQuery(
                        "SELECT * FROM TB_BANGLUONG WHERE IDBL = :p0 FOR UPDATE",
                        new OracleParameter("p0", idbl)
                    ).FirstOrDefault();

                    if (bl == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy bảng lương mã {idbl}.");

                    // So sánh token phiên bản đồng thời (CAS)
                    string currentToken = ComputeDataVersionToken(bl);
                    if (!string.Equals(expectedDataVersionToken, currentToken, StringComparison.Ordinal))
                    {
                        throw new BusinessException("CONCURRENCY_CONFLICT", "Dữ liệu bảng lương đã được tính lại hoặc điều chỉnh bởi người khác kể từ khi bạn mở xem trước. Vui lòng tải lại dữ liệu để lấy số liệu mới nhất.");
                    }

                    // 2. Kiểm tra khóa kỳ công
                    var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == bl.MAKYCONG);
                    if (kc != null && (kc.KHOA ?? 0) == 1)
                    {
                        throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {bl.MAKYCONG} đã bị khóa sổ. Dữ liệu bảng lương kỳ khóa không được phép sửa đổi trực tiếp.");
                    }

                    // Kiểm tra trạng thái chi trả và công bố trong transaction; lỗi đọc trạng thái phải fail-closed
                    PayrollStatusCheckRow statusRow = null;
                    try
                    {
                        statusRow = db.Database.SqlQuery<PayrollStatusCheckRow>(@"
                            SELECT NVL(TRANG_THAI, 'DRAFT') AS TRANG_THAI,
                                   NVL(TRANGTHAI_CHITRA, 'CHUA_CHI_TRA') AS TRANGTHAI_CHITRA
                            FROM TB_BANGLUONG
                            WHERE IDBL = :p0",
                            new OracleParameter("p0", idbl)
                        ).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        string refId = $"REF-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
                        System.Diagnostics.Trace.TraceError($"[{refId}] Status query failed: {ex}");
                        throw new BusinessException("STATUS_CHECK_FAILED", $"Không thể xác thực trạng thái bảo vệ bảng lương do lỗi cơ sở dữ liệu (Mã đối chiếu: {refId}). Giao dịch điều chỉnh bị hủy bỏ để đảm bảo an toàn.");
                    }

                    if (statusRow != null && string.Equals(statusRow.TRANGTHAI_CHITRA, "DA_CHI_TRA", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BusinessException("PAID_PAYROLL_LOCKED", $"Bảng lương mã {idbl} đã được chi trả (DA_CHI_TRA). Không được phép chỉnh sửa trực tiếp kết quả lương đã thanh toán.");
                    }

                    actualSoChungTu = string.IsNullOrWhiteSpace(soChungTu)
                        ? $"ADJ-{bl.MAKYCONG}-{bl.MANV}-{DateTime.Now:yyyyMMddHHmmss}"
                        : soChungTu.Trim();

                    // 3. Serialize old state for audit
                    var oldValues = new
                    {
                        bl.IDBL,
                        bl.MANV,
                        bl.MAKYCONG,
                        bl.LUONG_CONG_THUCTE,
                        bl.TIEN_TANGCA,
                        bl.TIEN_CHUYENCAN,
                        bl.TIEN_AN_CA,
                        bl.PHUCAP_CONG_THUCTE,
                        bl.KHOAN_CONG_KHAC,
                        bl.KHOAN_TRU_KHAC,
                        bl.THUE_TNCN,
                        bl.TONG_CONG,
                        bl.THUC_LINH,
                        OldGrossTaxable = preview.GrossTaxableIncome,
                        OldTaxableAssessable = preview.TaxableAssessableIncome
                    };
                    string oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(oldValues);

                    // 4. Cập nhật các trường tổng hợp và căn cứ thuế
                    decimal newLuong = items.FirstOrDefault(x => x.KhoanMuc == "Lương công thực tế")?.GiaTriMoi ?? (bl.LUONG_CONG_THUCTE ?? 0m);
                    decimal newTangCa = items.FirstOrDefault(x => x.KhoanMuc == "Tiền làm thêm giờ (OT)")?.GiaTriMoi ?? (bl.TIEN_TANGCA ?? 0m);
                    decimal newChuyenCan = items.FirstOrDefault(x => x.KhoanMuc == "Tiền thưởng chuyên cần")?.GiaTriMoi ?? (bl.TIEN_CHUYENCAN ?? 0m);
                    decimal newAnCa = items.FirstOrDefault(x => x.KhoanMuc == "Tiền ăn ca / Cơm trưa")?.GiaTriMoi ?? (bl.TIEN_AN_CA ?? 0m);
                    decimal newKhoanCongKhac = items.FirstOrDefault(x => x.KhoanMuc == "Khoản cộng phát sinh khác")?.GiaTriMoi ?? (bl.KHOAN_CONG_KHAC ?? 0m);
                    decimal newKhoanTruKhac = items.FirstOrDefault(x => x.KhoanMuc == "Khoản trừ phát sinh khác")?.GiaTriMoi ?? (bl.KHOAN_TRU_KHAC ?? 0m);

                    decimal deltaChuyenCan = newChuyenCan - (bl.TIEN_CHUYENCAN ?? 0m);
                    decimal deltaAnCa = newAnCa - (bl.TIEN_AN_CA ?? 0m);
                    decimal newPhuCap = Math.Max(0m, (bl.PHUCAP_CONG_THUCTE ?? 0m) + deltaChuyenCan + deltaAnCa);

                    bl.LUONG_CONG_THUCTE = newLuong;
                    bl.TIEN_TANGCA = newTangCa;
                    bl.TIEN_CHUYENCAN = newChuyenCan;
                    bl.TIEN_AN_CA = newAnCa;
                    bl.PHUCAP_CONG_THUCTE = newPhuCap;
                    bl.KHOAN_CONG_KHAC = newKhoanCongKhac;
                    bl.KHOAN_TRU_KHAC = newKhoanTruKhac;
                    bl.THUE_TNCN = preview.NewThueTncn;
                    bl.TONG_CONG = preview.NewTongCong;
                    bl.THUC_LINH = preview.NewThucLinh;

                    resultingThucLinh = preview.NewThucLinh;
                    resultingTongCong = preview.NewTongCong;

                    try
                    {
                        db.Database.ExecuteSqlCommand(@"
                            UPDATE TB_BANGLUONG
                            SET LUONG_CONG_THUCTE = :p0,
                                TIEN_TANGCA = :p1,
                                TIEN_CHUYENCAN = :p2,
                                TIEN_AN_CA = :p3,
                                PHUCAP_CONG_THUCTE = :p4,
                                KHOAN_CONG_KHAC = :p5,
                                KHOAN_TRU_KHAC = :p6,
                                THUE_TNCN = :p7,
                                TONG_CONG = :p8,
                                THUC_LINH = :p9,
                                TONG_THU_NHAP_CHIU_THUE = :p10,
                                THU_NHAP_TINH_THUE = :p11,
                                GIAM_TRU_BAN_THAN = :p12,
                                GIAM_TRU_PHU_THUOC = :p13,
                                GIAM_TRU_BAO_HIEM = :p14
                            WHERE IDBL = :p15",
                            new OracleParameter("p0", bl.LUONG_CONG_THUCTE),
                            new OracleParameter("p1", bl.TIEN_TANGCA),
                            new OracleParameter("p2", bl.TIEN_CHUYENCAN),
                            new OracleParameter("p3", bl.TIEN_AN_CA),
                            new OracleParameter("p4", bl.PHUCAP_CONG_THUCTE),
                            new OracleParameter("p5", bl.KHOAN_CONG_KHAC),
                            new OracleParameter("p6", bl.KHOAN_TRU_KHAC),
                            new OracleParameter("p7", bl.THUE_TNCN),
                            new OracleParameter("p8", bl.TONG_CONG),
                            new OracleParameter("p9", bl.THUC_LINH),
                            new OracleParameter("p10", preview.GrossTaxableIncome),
                            new OracleParameter("p11", preview.TaxableAssessableIncome),
                            new OracleParameter("p12", preview.PersonalDeduction),
                            new OracleParameter("p13", preview.DependentDeduction),
                            new OracleParameter("p14", preview.InsuranceDeduction),
                            new OracleParameter("p15", bl.IDBL)
                        );
                    }
                    catch
                    {
                        // Fallback cho schema chưa migrate các cột thuế snapshot mới
                        db.Database.ExecuteSqlCommand(@"
                            UPDATE TB_BANGLUONG
                            SET LUONG_CONG_THUCTE = :p0,
                                TIEN_TANGCA = :p1,
                                TIEN_CHUYENCAN = :p2,
                                TIEN_AN_CA = :p3,
                                PHUCAP_CONG_THUCTE = :p4,
                                KHOAN_CONG_KHAC = :p5,
                                KHOAN_TRU_KHAC = :p6,
                                THUE_TNCN = :p7,
                                TONG_CONG = :p8,
                                THUC_LINH = :p9
                            WHERE IDBL = :p10",
                            new OracleParameter("p0", bl.LUONG_CONG_THUCTE),
                            new OracleParameter("p1", bl.TIEN_TANGCA),
                            new OracleParameter("p2", bl.TIEN_CHUYENCAN),
                            new OracleParameter("p3", bl.TIEN_AN_CA),
                            new OracleParameter("p4", bl.PHUCAP_CONG_THUCTE),
                            new OracleParameter("p5", bl.KHOAN_CONG_KHAC),
                            new OracleParameter("p6", bl.KHOAN_TRU_KHAC),
                            new OracleParameter("p7", bl.THUE_TNCN),
                            new OracleParameter("p8", bl.TONG_CONG),
                            new OracleParameter("p9", bl.THUC_LINH),
                            new OracleParameter("p10", bl.IDBL)
                        );
                    }

                    // 5. Đồng bộ chi tiết thuế bậc (TB_BANGLUONG_THUE_CT)
                    try
                    {
                        if (preview.TaxBracketTraces != null && preview.TaxBracketTraces.Count > 0)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_THUE_CT WHERE IDBL = :p0", new OracleParameter("p0", bl.IDBL));
                            foreach (var bt in preview.TaxBracketTraces)
                            {
                                decimal taxId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_THUE_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                                db.Database.ExecuteSqlCommand(@"
                                    INSERT INTO TB_BANGLUONG_THUE_CT (
                                        ID, IDBL, MANV, MAKYCONG, POLICY_THUE_ID, BAC_THUE, CAN_DUOI, CAN_TREN,
                                        THU_NHAP_CHIU_THUE_BAC, THUE_SUAT, TIEN_THUE_BAC, CREATED_AT
                                    ) VALUES (
                                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, SYSTIMESTAMP
                                    )",
                                    new OracleParameter("p0", taxId),
                                    new OracleParameter("p1", bl.IDBL),
                                    new OracleParameter("p2", bl.MANV),
                                    new OracleParameter("p3", bl.MAKYCONG),
                                    new OracleParameter("p4", bt.POLICY_THUE_ID),
                                    new OracleParameter("p5", bt.BAC_THUE),
                                    new OracleParameter("p6", bt.CAN_DUOI),
                                    new OracleParameter("p7", bt.CAN_TREN.HasValue ? (object)bt.CAN_TREN.Value : DBNull.Value),
                                    new OracleParameter("p8", bt.THU_NHAP_CHIU_THUE_BAC),
                                    new OracleParameter("p9", bt.THUE_SUAT),
                                    new OracleParameter("p10", bt.TIEN_THUE_BAC)
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning($"Sync TB_BANGLUONG_THUE_CT skipped or legacy schema: {ex.Message}");
                    }

                    // 6. Đồng bộ các khoản chi tiết (TB_BANGLUONG_CT) bảo đảm khớp tổng và công thức
                    SyncBangLuongChiTiet(db, bl, items, actualSoChungTu, preview);

                    newVersionToken = ComputeDataVersionToken(bl);

                    // 7. Ghi audit log vào TB_SYS_LOG
                    var newValues = new
                    {
                        bl.IDBL,
                        bl.MANV,
                        bl.MAKYCONG,
                        bl.LUONG_CONG_THUCTE,
                        bl.TIEN_TANGCA,
                        bl.TIEN_CHUYENCAN,
                        bl.TIEN_AN_CA,
                        bl.PHUCAP_CONG_THUCTE,
                        bl.KHOAN_CONG_KHAC,
                        bl.KHOAN_TRU_KHAC,
                        bl.THUE_TNCN,
                        bl.TONG_CONG,
                        bl.THUC_LINH,
                        LyDo = lyDo.Trim(),
                        SoChungTu = actualSoChungTu,
                        DataVersionToken = newVersionToken
                    };
                    string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(newValues);

                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + effectiveUserId);
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_CU, DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, :p6, CURRENT_TIMESTAMP, :p7, :p8
                        )",
                        new OracleParameter("p0", effectiveUserId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "DIEU_CHINH_LUONG"),
                        new OracleParameter("p3", "TB_BANGLUONG"),
                        new OracleParameter("p4", bl.IDBL.ToString()),
                        new OracleParameter("p5", oldJson),
                        new OracleParameter("p6", newJson),
                        new OracleParameter("p7", "TIENLUONG"),
                        new OracleParameter("p8", $"SoChungTu={actualSoChungTu}, DeltaThucLinh={preview.DeltaThucLinh:N0}, LyDo={lyDo.Trim()}")
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            // Post-commit: Reload record outside transaction.
            try
            {
                updated = new Bu.CLASS_CHAMCONG.BANGLUONG().getList((int)preview.MAKYCONG).FirstOrDefault(x => x.IDBL == idbl);
            }
            catch (Exception ex)
            {
                string reloadRef = $"REF-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
                System.Diagnostics.Trace.TraceWarning($"[{reloadRef}] Reload after salary adjustment commit failed: {ex}");
                reloadWarning = $"Dữ liệu điều chỉnh (Số chứng từ: {actualSoChungTu}) đã được lưu thành công vào cơ sở dữ liệu. Tải lại giao diện gặp lỗi (Mã đối chiếu: {reloadRef}). Vui lòng bấm 'Tải lại' trên danh sách bảng lương.";
            }

            return new SalaryAdjustmentResultDto
            {
                Success = true,
                IDBL = idbl,
                NewThucLinh = resultingThucLinh,
                NewTongCong = resultingTongCong,
                SoChungTu = actualSoChungTu,
                DataVersionToken = newVersionToken,
                Message = reloadWarning ?? $"Đã ghi nhận điều chỉnh bảng lương thành công (Chứng từ: {actualSoChungTu}, Thực lĩnh mới: {resultingThucLinh:N0} đ).",
                UpdatedBangLuong = updated
            };
        }

        private static void SyncBangLuongChiTiet(MyEntities db, TB_BANGLUONG bl, List<SalaryAdjustmentItemDto> items, string soChungTu, SalaryAdjustmentPreviewDto preview = null)
        {
            // Lấy danh sách các dòng chi tiết hiện tại của bảng lương
            var existingDetails = db.Database.SqlQuery<BangLuongCtRow>(@"
                SELECT IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                       SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                       SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI
                FROM TB_BANGLUONG_CT
                WHERE IDBL = :p0",
                new OracleParameter("p0", bl.IDBL)
            ).ToList();

            foreach (var item in items)
            {
                if (item.ChenhLech == 0) continue;

                string dienGiai = $"Điều chỉnh theo chứng từ {soChungTu}: Cũ={item.GiaTriCu:N0}, Mới={item.GiaTriMoi:N0}";

                switch (item.KhoanMuc)
                {
                    case "Lương công thực tế":
                    {
                        // Kiểm tra dòng lương phép (nếu có) để trừ ra khỏi tổng Lương công thực tế, tránh nhân đôi
                        var leaveRow = existingDetails.FirstOrDefault(x => x.MA_KHOAN_MUC == "LUONG_NGAY_PHEP" || x.NHOM_KHOAN_MUC == "NGHIPHEP");
                        decimal leaveWage = leaveRow?.THANH_TIEN ?? 0m;
                        decimal workWage = Math.Max(0m, item.GiaTriMoi - leaveWage);

                        var row = existingDetails.FirstOrDefault(x =>
                            x.MA_KHOAN_MUC == "LUONG_CONG_THUCTE" ||
                            x.MA_KHOAN_MUC == "LUONG_CONG_THUC_TE" ||
                            x.NHOM_KHOAN_MUC == "LUONG_CHINH");

                        decimal dailyRate = (bl.DAILY_RATE.HasValue && bl.DAILY_RATE.Value > 0) ? bl.DAILY_RATE.Value : (row?.DON_GIA ?? (workWage > 0 ? workWage : 1m));
                        decimal workDays = dailyRate > 0 ? Math.Round(workWage / dailyRate, 2) : 1m;
                        string formula = leaveWage > 0
                            ? $"{workDays} công x {dailyRate:N0} đ/ngày = {workWage:N0} đ (Tổng lương {item.GiaTriMoi:N0} đ trừ lương phép {leaveWage:N0} đ)"
                            : $"{workDays} công x {dailyRate:N0} đ/ngày = {workWage:N0} đ (Chứng từ {soChungTu})";

                        if (row != null)
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_BANGLUONG_CT
                                SET MA_KHOAN_MUC = 'LUONG_CONG_THUCTE',
                                    SO_LUONG = :p0,
                                    DON_GIA = :p1,
                                    HE_SO = 1.0,
                                    THANH_TIEN = :p2,
                                    SO_TIEN_CHIU_THUE = :p2,
                                    SO_TIEN_MIEN_THUE = 0,
                                    CONG_THUC_DIEN_GIAI = :p3
                                WHERE IDBLCT = :p4",
                                new OracleParameter("p0", workDays),
                                new OracleParameter("p1", dailyRate),
                                new OracleParameter("p2", workWage),
                                new OracleParameter("p3", formula),
                                new OracleParameter("p4", row.IDBLCT)
                            );
                        }
                        else
                        {
                            decimal nextId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO TB_BANGLUONG_CT (
                                    IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                                    SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                                    SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                                ) VALUES (
                                    :p0, :p1, :p2, :p3, 'LUONG_CHINH', 'LUONG_CONG_THUCTE', 'Lương công thực tế',
                                    :p4, :p5, 1.0, :p6, 1, 1,
                                    0, :p6, :p7, 0, SYSTIMESTAMP
                                )",
                                new OracleParameter("p0", nextId),
                                new OracleParameter("p1", bl.IDBL),
                                new OracleParameter("p2", bl.MANV),
                                new OracleParameter("p3", bl.MAKYCONG),
                                new OracleParameter("p4", workDays),
                                new OracleParameter("p5", dailyRate),
                                new OracleParameter("p6", workWage),
                                new OracleParameter("p7", formula)
                            );
                        }
                        break;
                    }

                    case "Tiền làm thêm giờ (OT)":
                    {
                        var row = existingDetails.FirstOrDefault(x =>
                            x.MA_KHOAN_MUC == "TIEN_TANG_CA" ||
                            x.NHOM_KHOAN_MUC == "TANG_CA");

                        decimal otTaxable = 0m;
                        decimal otExempt = 0m;
                        if (row != null && row.THANH_TIEN.HasValue && row.THANH_TIEN.Value > 0)
                        {
                            decimal ratio = (row.SO_TIEN_CHIU_THUE ?? 0m) / row.THANH_TIEN.Value;
                            otTaxable = Math.Round(item.GiaTriMoi * ratio, 2);
                            otExempt = item.GiaTriMoi - otTaxable;
                        }
                        else
                        {
                            otTaxable = item.GiaTriMoi;
                            otExempt = 0m;
                        }

                        decimal hourlyRate = (bl.DAILY_RATE.HasValue && bl.DAILY_RATE.Value > 0) ? Math.Round(bl.DAILY_RATE.Value / 8m, 2) : (row?.DON_GIA ?? 1m);
                        decimal otHours = (row != null && row.SO_LUONG.HasValue && row.SO_LUONG.Value > 0) ? row.SO_LUONG.Value : 1m;
                        decimal heSo = (otHours > 0 && hourlyRate > 0) ? Math.Round(item.GiaTriMoi / (otHours * hourlyRate), 2) : (row?.HE_SO ?? 1.5m);
                        string formula = $"Làm thêm giờ (OT): {item.GiaTriMoi:N0} đ (Miễn thuế: {otExempt:N0} đ, Chịu thuế: {otTaxable:N0} đ) - Chứng từ {soChungTu}";

                        if (row != null)
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_BANGLUONG_CT
                                SET MA_KHOAN_MUC = 'TIEN_TANG_CA',
                                    DON_GIA = :p0,
                                    HE_SO = :p1,
                                    THANH_TIEN = :p2,
                                    SO_TIEN_MIEN_THUE = :p3,
                                    SO_TIEN_CHIU_THUE = :p4,
                                    CONG_THUC_DIEN_GIAI = :p5
                                WHERE IDBLCT = :p6",
                                new OracleParameter("p0", hourlyRate),
                                new OracleParameter("p1", heSo),
                                new OracleParameter("p2", item.GiaTriMoi),
                                new OracleParameter("p3", otExempt),
                                new OracleParameter("p4", otTaxable),
                                new OracleParameter("p5", formula),
                                new OracleParameter("p6", row.IDBLCT)
                            );
                        }
                        else
                        {
                            decimal nextId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO TB_BANGLUONG_CT (
                                    IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                                    SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                                    SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                                ) VALUES (
                                    :p0, :p1, :p2, :p3, 'TANG_CA', 'TIEN_TANG_CA', 'Tiền làm thêm giờ (OT)',
                                    :p4, :p5, :p6, :p7, 0, 1,
                                    :p8, :p9, :p10, 0, SYSTIMESTAMP
                                )",
                                new OracleParameter("p0", nextId),
                                new OracleParameter("p1", bl.IDBL),
                                new OracleParameter("p2", bl.MANV),
                                new OracleParameter("p3", bl.MAKYCONG),
                                new OracleParameter("p4", otHours),
                                new OracleParameter("p5", hourlyRate),
                                new OracleParameter("p6", heSo),
                                new OracleParameter("p7", item.GiaTriMoi),
                                new OracleParameter("p8", otExempt),
                                new OracleParameter("p9", otTaxable),
                                new OracleParameter("p10", formula)
                            );
                        }
                        break;
                    }

                    case "Tiền thưởng chuyên cần":
                    {
                        var row = existingDetails.FirstOrDefault(x =>
                            x.MA_KHOAN_MUC == "PHUCAP_CHUYEN_CAN" ||
                            (x.NHOM_KHOAN_MUC == "PHU_CAP" && x.TEN_KHOAN_MUC != null &&
                             (x.TEN_KHOAN_MUC.IndexOf("chuyên cần", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              x.TEN_KHOAN_MUC.IndexOf("Chuyên cần", StringComparison.OrdinalIgnoreCase) >= 0)));

                        if (row != null)
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_BANGLUONG_CT
                                SET DON_GIA = :p0,
                                    THANH_TIEN = :p0,
                                    SO_TIEN_CHIU_THUE = :p0,
                                    CONG_THUC_DIEN_GIAI = :p1
                                WHERE IDBLCT = :p2",
                                new OracleParameter("p0", item.GiaTriMoi),
                                new OracleParameter("p1", dienGiai),
                                new OracleParameter("p2", row.IDBLCT)
                            );
                        }
                        else
                        {
                            decimal nextId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO TB_BANGLUONG_CT (
                                    IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                                    SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                                    SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                                ) VALUES (
                                    :p0, :p1, :p2, :p3, 'PHU_CAP', 'PHUCAP_CHUYEN_CAN', 'Tiền thưởng chuyên cần',
                                    1.0, :p4, 1.0, :p4, 0, 1,
                                    0, :p4, :p5, 0, SYSTIMESTAMP
                                )",
                                new OracleParameter("p0", nextId),
                                new OracleParameter("p1", bl.IDBL),
                                new OracleParameter("p2", bl.MANV),
                                new OracleParameter("p3", bl.MAKYCONG),
                                new OracleParameter("p4", item.GiaTriMoi),
                                new OracleParameter("p5", dienGiai)
                            );
                        }
                        break;
                    }

                    case "Tiền ăn ca / Cơm trưa":
                    {
                        decimal exemptCap = 730000m;
                        decimal exemptMeal = Math.Min(item.GiaTriMoi, exemptCap);
                        decimal taxableMeal = Math.Max(0m, item.GiaTriMoi - exemptMeal);

                        var row = existingDetails.FirstOrDefault(x =>
                            x.MA_KHOAN_MUC == "PHUCAP_AN_CA" ||
                            (x.NHOM_KHOAN_MUC == "PHU_CAP" && x.TEN_KHOAN_MUC != null &&
                             (x.TEN_KHOAN_MUC.IndexOf("ăn ca", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              x.TEN_KHOAN_MUC.IndexOf("cơm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              x.TEN_KHOAN_MUC.IndexOf("Ăn", StringComparison.OrdinalIgnoreCase) >= 0)));

                        if (row != null)
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_BANGLUONG_CT
                                SET DON_GIA = :p0,
                                    THANH_TIEN = :p0,
                                    SO_TIEN_MIEN_THUE = :p1,
                                    SO_TIEN_CHIU_THUE = :p2,
                                    CONG_THUC_DIEN_GIAI = :p3
                                WHERE IDBLCT = :p4",
                                new OracleParameter("p0", item.GiaTriMoi),
                                new OracleParameter("p1", exemptMeal),
                                new OracleParameter("p2", taxableMeal),
                                new OracleParameter("p3", dienGiai),
                                new OracleParameter("p4", row.IDBLCT)
                            );
                        }
                        else
                        {
                            decimal nextId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO TB_BANGLUONG_CT (
                                    IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                                    SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                                    SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                                ) VALUES (
                                    :p0, :p1, :p2, :p3, 'PHU_CAP', 'PHUCAP_AN_CA', 'Tiền ăn ca / Cơm trưa',
                                    1.0, :p4, 1.0, :p4, 0, :p5,
                                    :p6, :p7, :p8, 0, SYSTIMESTAMP
                                )",
                                new OracleParameter("p0", nextId),
                                new OracleParameter("p1", bl.IDBL),
                                new OracleParameter("p2", bl.MANV),
                                new OracleParameter("p3", bl.MAKYCONG),
                                new OracleParameter("p4", item.GiaTriMoi),
                                new OracleParameter("p5", taxableMeal > 0 ? 1 : 0),
                                new OracleParameter("p6", exemptMeal),
                                new OracleParameter("p7", taxableMeal),
                                new OracleParameter("p8", dienGiai)
                            );
                        }
                        break;
                    }

                    case "KhoanCongKhac":
                    case "Khoan cộng phát sinh khác":
                    {
                        var nightRow = existingDetails.FirstOrDefault(x => x.MA_KHOAN_MUC == "PHUCAP_LAM_DEM");
                        decimal nightWage = nightRow?.THANH_TIEN ?? 0m;
                        decimal rewardWage = Math.Max(0m, item.GiaTriMoi - nightWage);

                        var row = existingDetails.FirstOrDefault(x =>
                            x.MA_KHOAN_MUC == "TIEN_KHEN_THUONG" ||
                            x.MA_KHOAN_MUC == "KHOAN_CONG_KHAC" ||
                            x.NHOM_KHOAN_MUC == "KHEN_THUONG" ||
                            x.NHOM_KHOAN_MUC == "THU_NHAP_KHAC");

                        string formula = nightWage > 0
                            ? $"Thưởng / Thu nhập khác: {rewardWage:N0} đ (Tổng khoản cộng {item.GiaTriMoi:N0} đ giữ nguyên trợ cấp làm đêm {nightWage:N0} đ)"
                            : $"Khoản cộng phát sinh khác: {rewardWage:N0} đ - Chứng từ {soChungTu}";

                        if (row != null)
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_BANGLUONG_CT
                                SET MA_KHOAN_MUC = 'TIEN_KHEN_THUONG',
                                    SO_LUONG = 1.0,
                                    DON_GIA = :p0,
                                    HE_SO = 1.0,
                                    THANH_TIEN = :p0,
                                    SO_TIEN_CHIU_THUE = :p0,
                                    SO_TIEN_MIEN_THUE = 0,
                                    CONG_THUC_DIEN_GIAI = :p1
                                WHERE IDBLCT = :p2",
                                new OracleParameter("p0", rewardWage),
                                new OracleParameter("p1", formula),
                                new OracleParameter("p2", row.IDBLCT)
                            );
                        }
                        else
                        {
                            decimal nextId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO TB_BANGLUONG_CT (
                                    IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                                    SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                                    SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                                ) VALUES (
                                    :p0, :p1, :p2, :p3, 'THU_NHAP_KHAC', 'TIEN_KHEN_THUONG', 'Tiền khen thưởng / Thu nhập khác',
                                    1.0, :p4, 1.0, :p4, 0, 1,
                                    0, :p4, :p5, 0, SYSTIMESTAMP
                                )",
                                new OracleParameter("p0", nextId),
                                new OracleParameter("p1", bl.IDBL),
                                new OracleParameter("p2", bl.MANV),
                                new OracleParameter("p3", bl.MAKYCONG),
                                new OracleParameter("p4", rewardWage),
                                new OracleParameter("p5", formula)
                            );
                        }
                        break;
                    }

                    case "Khoan trừ phát sinh khác":
                    {
                        var row = existingDetails.FirstOrDefault(x =>
                            x.MA_KHOAN_MUC == "KHAU_TRU_PHAT_SINH" ||
                            (x.NHOM_KHOAN_MUC == "KHAU_TRU" &&
                             x.MA_KHOAN_MUC != "TIEN_BHXH_TRICH" &&
                             x.MA_KHOAN_MUC != "TIEN_CONG_DOAN" &&
                             x.MA_KHOAN_MUC != "TIEN_TAMUNG" &&
                             x.MA_KHOAN_MUC != "THUE_TNCN"));

                        if (row != null)
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_BANGLUONG_CT
                                SET MA_KHOAN_MUC = 'KHAU_TRU_PHAT_SINH',
                                    DON_GIA = :p0,
                                    THANH_TIEN = :p0,
                                    CONG_THUC_DIEN_GIAI = :p1
                                WHERE IDBLCT = :p2",
                                new OracleParameter("p0", item.GiaTriMoi),
                                new OracleParameter("p1", dienGiai),
                                new OracleParameter("p2", row.IDBLCT)
                            );
                        }
                        else
                        {
                            decimal nextId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").FirstOrDefault();
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO TB_BANGLUONG_CT (
                                    IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                                    SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                                    SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                                ) VALUES (
                                    :p0, :p1, :p2, :p3, 'KHAU_TRU', 'KHAU_TRU_PHAT_SINH', 'Khoản trừ phát sinh khác',
                                    1.0, :p4, 1.0, :p4, 0, 0,
                                    0, 0, :p5, 0, SYSTIMESTAMP
                                )",
                                new OracleParameter("p0", nextId),
                                new OracleParameter("p1", bl.IDBL),
                                new OracleParameter("p2", bl.MANV),
                                new OracleParameter("p3", bl.MAKYCONG),
                                new OracleParameter("p4", item.GiaTriMoi),
                                new OracleParameter("p5", dienGiai)
                            );
                        }
                        break;
                    }
                }
            }
        }
    }
}

