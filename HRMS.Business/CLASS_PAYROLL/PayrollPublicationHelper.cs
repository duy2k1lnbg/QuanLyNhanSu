using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    /// <summary>
    /// Nguồn sự thật duy nhất (Single Source of Truth) xác định trạng thái công bố và thanh toán bảng lương.
    /// Độc lập hoàn toàn với việc khóa kỳ công (KHOA = 1) và không suy diễn từ các trạng thái trung gian (DRAFT, CALCULATED).
    /// </summary>
    public static class PayrollPublicationHelper
    {
        private static bool? _hasTrangThaiInBangLuong = null;

        public static bool CheckTrangThaiColumnsInBangLuong(MyEntities db)
        {
            if (_hasTrangThaiInBangLuong.HasValue) return _hasTrangThaiInBangLuong.Value;
            try
            {
                int count = db.Database.SqlQuery<int>(@"
                    SELECT COUNT(*) FROM USER_TAB_COLS 
                    WHERE TABLE_NAME = 'TB_BANGLUONG' AND COLUMN_NAME = 'TRANG_THAI'
                ").FirstOrDefault();
                _hasTrangThaiInBangLuong = count > 0;
            }
            catch
            {
                _hasTrangThaiInBangLuong = false;
            }
            return _hasTrangThaiInBangLuong.Value;
        }

        public static void ResetSchemaCache()
        {
            _hasTrangThaiInBangLuong = null;
        }

        public class PayrollPublicationResult
        {
            public bool IsPublished { get; set; }
            public string Status { get; set; }
            public string PaymentStatusText { get; set; }
            public string RawPaymentStatus { get; set; }
            public bool StatusQueryFailed { get; set; }
            public string CorrelationId { get; set; }
            public string Message { get; set; }
        }

        public class PayrollStatusRow
        {
            public string TRANG_THAI { get; set; }
            public string TRANGTHAI_CHITRA { get; set; }
        }

        /// <summary>
        /// Đánh giá trạng thái công bố và chi trả của một bảng lương theo đúng quy tắc nghiêm ngặt:
        /// - Chỉ các trạng thái xác định được phép công bố: APPROVED, PUBLISHED, CONG_BO, DA_DUYET, PAID, DA_CHI_TRA.
        /// - DRAFT, CALCULATED, PENDING_APPROVAL, REVOKED, UNKNOWN KHÔNG được coi là đã công bố.
        /// - Khóa kỳ (KHOA = 1) KHÔNG tự động biến bảng lương DRAFT/CALCULATED thành công bố.
        /// - Lỗi truy vấn trạng thái KHÔNG được nuốt để suy ra APPROVED mà phải Fail-Closed và ghi nhận mã đối chiếu REF-...
        /// - Trạng thái chi trả chỉ là "Đã chi trả" khi có dữ liệu thanh toán xác định (DA_CHI_TRA hoặc PAID), không suy từ khóa kỳ.
        /// </summary>
        public static PayrollPublicationResult EvaluatePublication(MyEntities db, TB_BANGLUONG bl)
        {
            var res = new PayrollPublicationResult
            {
                IsPublished = false,
                Status = "DRAFT",
                PaymentStatusText = "Chưa chi trả",
                RawPaymentStatus = "CHUA_CHI_TRA",
                StatusQueryFailed = false
            };

            if (bl == null)
            {
                res.Message = "Bản ghi bảng lương không tồn tại.";
                return res;
            }

            bool hasTrangThai = CheckTrangThaiColumnsInBangLuong(db);

            if (hasTrangThai)
            {
                try
                {
                    var row = db.Database.SqlQuery<PayrollStatusRow>(@"
                        SELECT NVL(TRANG_THAI, 'DRAFT') AS TRANG_THAI,
                               NVL(TRANGTHAI_CHITRA, 'CHUA_CHI_TRA') AS TRANGTHAI_CHITRA
                        FROM TB_BANGLUONG
                        WHERE IDBL = :p0",
                        new OracleParameter("p0", bl.IDBL)
                    ).FirstOrDefault();

                    if (row != null)
                    {
                        res.Status = row.TRANG_THAI?.Trim()?.ToUpperInvariant() ?? "DRAFT";
                        res.RawPaymentStatus = row.TRANGTHAI_CHITRA?.Trim()?.ToUpperInvariant() ?? "CHUA_CHI_TRA";
                    }
                }
                catch (Exception ex)
                {
                    res.StatusQueryFailed = true;
                    res.CorrelationId = $"REF-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
                    res.Status = "UNKNOWN";
                    res.IsPublished = false;
                    res.Message = $"Không thể xác định trạng thái công bố bảng lương (Mã đối chiếu: {res.CorrelationId}).";
                    System.Diagnostics.Trace.TraceError($"[{res.CorrelationId}] Lỗi truy vấn trạng thái công bố TB_BANGLUONG (IDBL={bl.IDBL}): {ex}");
                    return res;
                }
            }
            else
            {
                // Schema cũ: Kiểm tra bằng chứng công bố trong TB_SYS_LOG
                try
                {
                    int pubLogCount = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM TB_SYS_LOG
                        WHERE TEN_BANG = 'TB_BANGLUONG'
                          AND HANHDONG IN ('CONG_BO_BANGLUONG', 'DUYET_BANGLUONG', 'XAC_NHAN_CONG_BO')
                          AND ID_BAN_GHI = :p0",
                        new OracleParameter("p0", bl.IDBL.ToString())
                    ).FirstOrDefault();

                    if (pubLogCount > 0)
                    {
                        res.Status = "APPROVED";
                    }
                    else
                    {
                        res.Status = "DRAFT";
                    }
                }
                catch (Exception ex)
                {
                    res.StatusQueryFailed = true;
                    res.CorrelationId = $"REF-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
                    res.Status = "UNKNOWN";
                    res.IsPublished = false;
                    res.Message = $"Không thể xác minh chứng từ công bố lịch sử (Mã đối chiếu: {res.CorrelationId}).";
                    System.Diagnostics.Trace.TraceError($"[{res.CorrelationId}] Lỗi kiểm tra TB_SYS_LOG công bố: {ex}");
                    return res;
                }
            }

            // Danh sách trắng các trạng thái công bố hợp lệ
            res.IsPublished = res.Status == "APPROVED" ||
                              res.Status == "PUBLISHED" ||
                              res.Status == "CONG_BO" ||
                              res.Status == "DA_DUYET" ||
                              res.Status == "PAID" ||
                              res.Status == "DA_CHI_TRA";

            // Xác định trạng thái thanh toán từ dữ liệu thanh toán xác định
            if (res.RawPaymentStatus == "DA_CHI_TRA" || res.Status == "PAID" || res.Status == "DA_CHI_TRA")
            {
                res.PaymentStatusText = "Đã chi trả";
            }
            else if (res.RawPaymentStatus == "DANG_CHI_TRA")
            {
                res.PaymentStatusText = "Đang chi trả";
            }
            else
            {
                res.PaymentStatusText = "Chưa chi trả";
            }

            return res;
        }

        /// <summary>
        /// Phương thức thuần (Pure method) đánh giá trạng thái công bố không phụ thuộc kết nối DB, phục vụ unit testing
        /// </summary>
        public static bool IsStatusPublished(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            string s = status.Trim().ToUpperInvariant();
            return s == "APPROVED" || s == "PUBLISHED" || s == "CONG_BO" || s == "DA_DUYET" || s == "PAID" || s == "DA_CHI_TRA";
        }
    }
}
