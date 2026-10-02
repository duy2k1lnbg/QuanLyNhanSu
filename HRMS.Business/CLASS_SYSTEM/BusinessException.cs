using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bu.CLASS_SYSTEM
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public string TargetField { get; }

        public BusinessException(string errorCode, string message, string targetField = null, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode ?? "UNKNOWN_ERROR";
            TargetField = targetField;
        }

        public BusinessException(string errorCode, string message, Exception innerException)
            : this(errorCode, message, null, innerException)
        {
        }

        public static BusinessException FromErrorCode(string errorCode, string message, string targetField = null)
        {
            return new BusinessException(errorCode, message, targetField);
        }
    }

    public static class ErrorHelper
    {
        public static string ResolveUserFriendlyMessage(Exception ex, string defaultAction = "thao tác")
        {
            return ResolveUserFriendlyMessage(ex, defaultAction, out _);
        }

        public static string ResolveUserFriendlyMessage(Exception ex, string defaultAction, out string correlationId)
        {
            correlationId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            if (ex == null)
            {
                return "Đã xảy ra lỗi không xác định.";
            }

            // 1. Direct or nested BusinessException: Keep clear business message
            if (ex is Bu.CLASS_PAYROLL.PolicySchemaMissingException psEx)
            {
                return $"Cơ sở dữ liệu chưa sẵn sàng: Thiếu bảng chính sách tính lương '{psEx.MissingTableName}'.\n\n" +
                       "BẠN CẦN LÀM GÌ TRƯỚC:\n" +
                       "1. Quản trị viên (DBA) cần chạy script migration:\n" +
                       $"   'database/migrations/{psEx.MigrationScript}' (hoặc file 'apply_payroll_v1_16_objects.sql') để tạo các bảng chính sách và nạp cấu hình mặc định.\n" +
                       "2. Kiểm tra quyền truy cập (SELECT, INSERT, UPDATE) của user kết nối trên schema Oracle.\n" +
                       "3. Mở chức năng 'Cấu hình lương' để kiểm tra lại các tham số lương, BHXH và thuế TNCN trước khi tính lương.";
            }

            if (ex is BusinessException directBe)
            {
                return directBe.Message;
            }

            Exception searchBe = ex.InnerException;
            while (searchBe != null)
            {
                if (searchBe is Bu.CLASS_PAYROLL.PolicySchemaMissingException innerPs)
                {
                    return $"Cơ sở dữ liệu chưa sẵn sàng: Thiếu bảng chính sách tính lương '{innerPs.MissingTableName}'.\n\n" +
                           "BẠN CẦN LÀM GÌ TRƯỚC:\n" +
                           "1. Quản trị viên (DBA) cần chạy script migration:\n" +
                           $"   'database/migrations/{innerPs.MigrationScript}' (hoặc file 'apply_payroll_v1_16_objects.sql') để tạo các bảng chính sách và nạp cấu hình mặc định.\n" +
                           "2. Kiểm tra quyền truy cập (SELECT, INSERT, UPDATE) của user kết nối trên schema Oracle.\n" +
                           "3. Mở chức năng 'Cấu hình lương' để kiểm tra lại các tham số lương, BHXH và thuế TNCN trước khi tính lương.";
                }

                if (searchBe is BusinessException be)
                {
                    return be.Message;
                }
                searchBe = searchBe.InnerException;
            }

            // 2. Entity Framework Validation Exception
            if (ex is DbEntityValidationException valEx)
            {
                var sb = new StringBuilder("Dữ liệu nhập không hợp lệ:");
                foreach (var entityError in valEx.EntityValidationErrors)
                {
                    foreach (var propError in entityError.ValidationErrors)
                    {
                        sb.Append($"\n- {propError.ErrorMessage}");
                    }
                }
                return sb.ToString();
            }

            // 3. Concurrency conflict
            if (ex is DbUpdateConcurrencyException)
            {
                return "Dữ liệu đã bị thay đổi hoặc khóa bởi một người dùng khác trong lúc thao tác. Vui lòng làm mới và thử lại.";
            }

            // 4. Walk the entire exception chain to inspect Oracle error numbers and messages
            int? oraNumber = null;
            string fullExceptionText = ex.ToString();
            Exception current = ex;
            while (current != null)
            {
                var numProp = current.GetType().GetProperty("Number");
                if (numProp != null)
                {
                    try
                    {
                        oraNumber = Convert.ToInt32(numProp.GetValue(current, null));
                        break;
                    }
                    catch { }
                }
                current = current.InnerException;
            }

            // Map by Oracle error number
            if (oraNumber.HasValue)
            {
                switch (oraNumber.Value)
                {
                    case 1: // ORA-00001
                        return ResolveUniqueConstraintMessage(fullExceptionText);

                    case 2291: // ORA-02291
                        return "Không tìm thấy dữ liệu liên kết tham chiếu tương ứng trong hệ thống.";

                    case 2292: // ORA-02292
                        return "Dữ liệu đang được sử dụng ở các chứng từ / bảng liên quan, không thể xóa hoặc thay đổi.";

                    case 1400: // ORA-01400
                    case 1407: // ORA-01407
                        return "Thiếu thông tin bắt buộc. Vui lòng nhập đầy đủ các trường yêu cầu.";

                    case 12704: // ORA-12704
                        return "Lỗi định dạng bộ ký tự dữ liệu (character set). Vui lòng thử lại hoặc liên hệ quản trị viên.";

                    case 54:  // ORA-00054: resource busy
                    case 60:  // ORA-00060: deadlock
                        return "Dữ liệu đang được thao tác bởi phiên làm việc khác. Vui lòng thử lại sau ít phút.";

                    case 942: // ORA-00942
                    case 904: // ORA-00904
                        if (fullExceptionText.IndexOf("TB_CHINH_SACH_LUONG", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            fullExceptionText.IndexOf("TB_CHINH_SACH_BHXH", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            fullExceptionText.IndexOf("TB_CHINH_SACH_CONG_DOAN", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            fullExceptionText.IndexOf("TB_THUE_TNCN_BAC", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            fullExceptionText.IndexOf("TB_THUE_TNCN_CHINH_SACH", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return "Cơ sở dữ liệu chưa sẵn sàng: Thiếu bảng chính sách tính lương ('TB_CHINH_SACH_LUONG').\n\n" +
                                   "BẠN CẦN LÀM GÌ TRƯỚC:\n" +
                                   "1. Quản trị viên (DBA) cần chạy script migration:\n" +
                                   "   'database/migrations/V1_16__payroll_production_policies_and_itemized_details.sql' (hoặc file 'apply_payroll_v1_16_objects.sql') để tạo các bảng chính sách và nạp cấu hình mặc định.\n" +
                                   "2. Kiểm tra quyền truy cập (SELECT, INSERT, UPDATE) của user kết nối trên schema Oracle.\n" +
                                   "3. Mở chức năng 'Cấu hình lương' để kiểm tra lại các tham số lương, BHXH và thuế TNCN trước khi tính lương.";
                        }
                        return "Chức năng chưa sẵn sàng do thiếu cấu trúc dữ liệu hoặc phiên bản hệ thống chưa đồng bộ.";

                    case 50000:
                    case 12154:
                    case 12170:
                    case 12514:
                    case 12541:
                    case 3113:
                    case 3114:
                        return "Không thể kết nối đến cơ sở dữ liệu. Vui lòng kiểm tra đường truyền mạng hoặc máy chủ và thử lại.";

                    case 1017: // ORA-01017: invalid username/password
                        return "Thông tin xác thực cơ sở dữ liệu không hợp lệ. Vui lòng liên hệ quản trị hệ thống.";
                }
            }

            // String pattern detection across exception chain
            if (fullExceptionText.IndexOf("ORA-00001", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ResolveUniqueConstraintMessage(fullExceptionText);
            }

            if (fullExceptionText.IndexOf("ORA-02292", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Dữ liệu đang được sử dụng ở các phân hệ khác, không thể xóa hoặc thay đổi.";
            }

            if (fullExceptionText.IndexOf("ORA-02291", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Không tìm thấy dữ liệu liên kết tham chiếu cha trong hệ thống.";
            }

            if (fullExceptionText.IndexOf("ORA-12541", StringComparison.OrdinalIgnoreCase) >= 0 ||
                fullExceptionText.IndexOf("ORA-12154", StringComparison.OrdinalIgnoreCase) >= 0 ||
                fullExceptionText.IndexOf("ORA-12170", StringComparison.OrdinalIgnoreCase) >= 0 ||
                fullExceptionText.IndexOf("ORA-50000", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Không thể kết nối đến cơ sở dữ liệu Oracle. Vui lòng kiểm tra đường truyền và thử lại.";
            }

            // 5. Unknown technical error: Log details safely and return user-friendly message with correlation ID
            Trace.TraceError($"[{correlationId}] Lỗi trong quá trình {defaultAction}: {ex}");
            Debug.WriteLine($"[{correlationId}] Lỗi trong quá trình {defaultAction}: {ex}");

            return $"Đã xảy ra sự cố kỹ thuật trong quá trình {defaultAction}. Vui lòng thử lại hoặc thông báo quản trị viên (Mã đối chiếu: {correlationId}).";
        }

        private static string ResolveUniqueConstraintMessage(string fullText)
        {
            if (string.IsNullOrEmpty(fullText))
                return "Dữ liệu đã tồn tại trong hệ thống (trùng khóa chính hoặc thông tin duy nhất). Vui lòng kiểm tra lại.";

            string upper = fullText.ToUpper();
            if (upper.Contains("KYCONG") || upper.Contains("TB_KYCONG"))
            {
                return "Kỳ công tháng này đã tồn tại trong hệ thống. Vui lòng mở kỳ công hiện có.";
            }
            if (upper.Contains("HOPDONG") || upper.Contains("TB_HOPDONG"))
            {
                return "Số hợp đồng lao động đã tồn tại trong hệ thống. Vui lòng kiểm tra lại số hợp đồng.";
            }
            if (upper.Contains("NHANVIEN") || upper.Contains("CCCD"))
            {
                return "Mã nhân viên hoặc số căn cước/CMND đã tồn tại trong hệ thống.";
            }
            if (upper.Contains("SYS_USER") || upper.Contains("USERNAME"))
            {
                return "Tên đăng nhập đã tồn tại trong hệ thống. Vui lòng chọn tên đăng nhập khác.";
            }

            return "Dữ liệu đã tồn tại trong hệ thống (trùng khóa chính hoặc thông tin duy nhất). Vui lòng kiểm tra lại.";
        }
    }
}
