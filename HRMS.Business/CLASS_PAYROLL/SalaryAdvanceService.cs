using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public class SalaryAdvanceRequestDto
    {
        public long Id { get; set; }
        public long Manv { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public decimal Amount { get; set; }
        public string AdvanceDate { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string ApproverName { get; set; }
        public string ApprovedDate { get; set; }
        public string RejectReason { get; set; }
        public string DisbursedDate { get; set; }
        public string VoucherNo { get; set; }
        public string DisbursedByName { get; set; }
        public string DisburseNote { get; set; }
        public long? IdUlLink { get; set; }
        public string CreatedAt { get; set; }
    }

    public class SalaryAdvanceService
    {
        private readonly string _connectionString;

        public SalaryAdvanceService(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                using (var db = new MyEntities())
                {
                    _connectionString = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                }
            }
            else
            {
                _connectionString = connectionString;
            }
        }

        private OracleConnection GetConnection()
        {
            var conn = new OracleConnection(_connectionString);
            if (conn.State != ConnectionState.Open) conn.Open();
            return conn;
        }

        public SalaryAdvanceRequestDto CreateRequest(long manv, decimal amount, DateTime advanceDate, int thang, int nam, string reason, long userId)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền tạm ứng phải lớn hơn 0 đồng.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Vui lòng nêu rõ lý do tạm ứng lương.");
            if (thang < 1 || thang > 12)
                throw new ArgumentException("Tháng áp dụng không hợp lệ.");
            if (nam < 2000 || nam > 2100)
                throw new ArgumentException("Năm áp dụng không hợp lệ.");

            using (var conn = GetConnection())
            {
                // Lấy ID từ sequence
                long newId;
                using (var cmdSeq = new OracleCommand("SELECT HR.SEQ_YEUCAU_UNGLUONG.NEXTVAL FROM DUAL", conn))
                {
                    newId = Convert.ToInt64(cmdSeq.ExecuteScalar());
                }

                string sql = @"
                    INSERT INTO HR.TB_YEUCAU_UNGLUONG 
                    (ID, MANV, SOTIEN, NGAY_DENGHI, THANG, NAM, LYDO, TRANGTHAI, CREATED_AT, CREATED_BY)
                    VALUES (:p_id, :p_manv, :p_sotien, :p_ngay, :p_thang, :p_nam, :p_lydo, 'PENDING', SYSDATE, :p_uid)";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_id", newId);
                    cmd.Parameters.Add("p_manv", manv);
                    cmd.Parameters.Add("p_sotien", amount);
                    cmd.Parameters.Add("p_ngay", advanceDate.Date);
                    cmd.Parameters.Add("p_thang", thang);
                    cmd.Parameters.Add("p_nam", nam);
                    cmd.Parameters.Add("p_lydo", reason.Trim());
                    cmd.Parameters.Add("p_uid", userId);
                    cmd.ExecuteNonQuery();
                }

                // Gửi thông báo hệ thống
                CLASS_SYSTEM.NotificationService.CreateNotification(
                    conn,
                    "Yêu cầu tạm ứng lương mới",
                    $"Nhân viên #{manv} đã gửi yêu cầu tạm ứng số tiền {amount:N0} VNĐ cho kỳ {thang:D2}/{nam}.",
                    "YEUCAU_UNGLUONG",
                    "HR_SYSTEM"
                );

                return new SalaryAdvanceRequestDto
                {
                    Id = newId,
                    Manv = manv,
                    Amount = amount,
                    AdvanceDate = advanceDate.ToString("yyyy-MM-dd"),
                    Thang = thang,
                    Nam = nam,
                    Reason = reason,
                    Status = "PENDING",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                };
            }
        }

        public List<SalaryAdvanceRequestDto> GetList(int? thang, int? nam, string status, string search, decimal? selfManv, int page, int pageSize, out int totalCount)
        {
            using (var conn = GetConnection())
            {
                var where = new List<string> { "1=1" };
                var cmd = new OracleCommand { Connection = conn, BindByName = true };

                if (selfManv.HasValue)
                {
                    where.Add("y.MANV = :p_self_manv");
                    cmd.Parameters.Add("p_self_manv", selfManv.Value);
                }
                if (thang.HasValue && thang.Value > 0)
                {
                    where.Add("y.THANG = :p_thang");
                    cmd.Parameters.Add("p_thang", thang.Value);
                }
                if (nam.HasValue && nam.Value > 0)
                {
                    where.Add("y.NAM = :p_nam");
                    cmd.Parameters.Add("p_nam", nam.Value);
                }
                if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
                {
                    where.Add("y.TRANGTHAI = :p_status");
                    cmd.Parameters.Add("p_status", status.ToUpper());
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where.Add("(LOWER(nv.HOTEN) LIKE :p_search OR LOWER(nv.EMPLOYEE_CODE) LIKE :p_search OR LOWER(y.LYDO) LIKE :p_search)");
                    cmd.Parameters.Add("p_search", $"%{search.Trim().ToLower()}%");
                }

                string whereClause = string.Join(" AND ", where);

                // Count total
                cmd.CommandText = $@"
                    SELECT COUNT(*)
                    FROM HR.TB_YEUCAU_UNGLUONG y
                    LEFT JOIN HR.TB_NHANVIEN nv ON y.MANV = nv.MANV
                    WHERE {whereClause}";
                totalCount = Convert.ToInt32(cmd.ExecuteScalar());

                // Page query
                int startRow = (page - 1) * pageSize;
                cmd.CommandText = $@"
                    SELECT * FROM (
                        SELECT 
                            y.ID, y.MANV, nv.EMPLOYEE_CODE, nv.HOTEN AS EMPLOYEE_NAME, pb.TENPB AS DEPARTMENT_NAME,
                            y.SOTIEN, y.NGAY_DENGHI, y.THANG, y.NAM, y.LYDO, y.TRANGTHAI,
                            NVL(u_app.FULLNAME, u_app.USERNAME) AS APPROVER_NAME, y.NGAYDUYET, y.GHICHUDUYET,
                            y.NGAY_CHI, y.SO_CHUNG_TU, NVL(u_dis.FULLNAME, u_dis.USERNAME) AS DISBURSED_BY_NAME, y.GHI_CHU_CHI,
                            y.IDUL_LINK, y.CREATED_AT,
                            ROW_NUMBER() OVER (ORDER BY y.CREATED_AT DESC) AS RN
                        FROM HR.TB_YEUCAU_UNGLUONG y
                        LEFT JOIN HR.TB_NHANVIEN nv ON y.MANV = nv.MANV
                        LEFT JOIN HR.TB_PHONGBAN pb ON nv.IDPB = pb.IDPB
                        LEFT JOIN HR.TB_SYS_USER u_app ON y.NGUOIDUYET = u_app.IDUSER
                        LEFT JOIN HR.TB_SYS_USER u_dis ON y.NGUOI_CHI = u_dis.IDUSER
                        WHERE {whereClause}
                    ) WHERE RN > :p_startRow AND RN <= :p_endRow";

                cmd.Parameters.Add("p_startRow", startRow);
                cmd.Parameters.Add("p_endRow", startRow + pageSize);

                var list = new List<SalaryAdvanceRequestDto>();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new SalaryAdvanceRequestDto
                        {
                            Id = Convert.ToInt64(r["ID"]),
                            Manv = Convert.ToInt64(r["MANV"]),
                            EmployeeCode = r["EMPLOYEE_CODE"]?.ToString(),
                            EmployeeName = r["EMPLOYEE_NAME"]?.ToString(),
                            DepartmentName = r["DEPARTMENT_NAME"]?.ToString() ?? "Chưa phân bổ",
                            Amount = Convert.ToDecimal(r["SOTIEN"]),
                            AdvanceDate = r["NGAY_DENGHI"] != DBNull.Value ? Convert.ToDateTime(r["NGAY_DENGHI"]).ToString("yyyy-MM-dd") : "",
                            Thang = Convert.ToInt32(r["THANG"]),
                            Nam = Convert.ToInt32(r["NAM"]),
                            Reason = r["LYDO"]?.ToString(),
                            Status = r["TRANGTHAI"]?.ToString(),
                            ApproverName = r["APPROVER_NAME"]?.ToString(),
                            ApprovedDate = r["NGAYDUYET"] != DBNull.Value ? Convert.ToDateTime(r["NGAYDUYET"]).ToString("yyyy-MM-dd HH:mm") : null,
                            RejectReason = r["GHICHUDUYET"]?.ToString(),
                            DisbursedDate = r["NGAY_CHI"] != DBNull.Value ? Convert.ToDateTime(r["NGAY_CHI"]).ToString("yyyy-MM-dd") : null,
                            VoucherNo = r["SO_CHUNG_TU"]?.ToString(),
                            DisbursedByName = r["DISBURSED_BY_NAME"]?.ToString(),
                            DisburseNote = r["GHI_CHU_CHI"]?.ToString(),
                            IdUlLink = r["IDUL_LINK"] != DBNull.Value ? (long?)Convert.ToInt64(r["IDUL_LINK"]) : null,
                            CreatedAt = r["CREATED_AT"] != DBNull.Value ? Convert.ToDateTime(r["CREATED_AT"]).ToString("yyyy-MM-dd HH:mm") : ""
                        });
                    }
                }
                return list;
            }
        }

        public void ApproveRequest(long id, long approverUserId, string approverUsername, decimal? approverEmployeeId)
        {
            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    long manv;
                    string currentStatus;
                    using (var cmdLock = new OracleCommand(
                        "SELECT MANV, TRANGTHAI FROM HR.TB_YEUCAU_UNGLUONG WHERE ID = :p_id FOR UPDATE", conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", id);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy đơn tạm ứng lương.");
                            manv = Convert.ToInt64(r["MANV"]);
                            currentStatus = r["TRANGTHAI"]?.ToString();
                        }
                    }

                    if (approverEmployeeId.HasValue && approverEmployeeId.Value == manv)
                    {
                        throw new InvalidOperationException("Bạn không thể tự phê duyệt đơn tạm ứng lương của chính mình.");
                    }

                    if (currentStatus != "PENDING")
                    {
                        throw new InvalidOperationException($"Đơn tạm ứng không ở trạng thái chờ duyệt (Trạng thái hiện tại: {currentStatus}).");
                    }

                    using (var cmdUpd = new OracleCommand(
                        "UPDATE HR.TB_YEUCAU_UNGLUONG SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p_uid, NGAYDUYET = SYSDATE WHERE ID = :p_id", conn))
                    {
                        cmdUpd.Transaction = tx;
                        cmdUpd.BindByName = true;
                        cmdUpd.Parameters.Add("p_uid", approverUserId);
                        cmdUpd.Parameters.Add("p_id", id);
                        cmdUpd.ExecuteNonQuery();
                    }

                    // Thông báo tới nhân viên
                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Đơn tạm ứng lương đã được duyệt",
                        $"Đơn tạm ứng lương #{id} của bạn đã được quản lý phê duyệt và chuyển sang Kế toán để xác nhận chi tiền.",
                        "YEUCAU_UNGLUONG",
                        approverUsername,
                        targetType: "PERSONAL",
                        targetManv: manv,
                        eventType: "SALARY_ADVANCE_APPROVED",
                        entityId: id,
                        discretePushBody: "HRMS: Đơn tạm ứng lương của bạn đã được duyệt và chuyển sang Kế toán."
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void RejectRequest(long id, long approverUserId, string reason, decimal? approverEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Vui lòng cung cấp lý do từ chối.");

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    long manv;
                    string currentStatus;
                    using (var cmdLock = new OracleCommand(
                        "SELECT MANV, TRANGTHAI FROM HR.TB_YEUCAU_UNGLUONG WHERE ID = :p_id FOR UPDATE", conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", id);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy đơn tạm ứng lương.");
                            manv = Convert.ToInt64(r["MANV"]);
                            currentStatus = r["TRANGTHAI"]?.ToString();
                        }
                    }

                    if (approverEmployeeId.HasValue && approverEmployeeId.Value == manv)
                    {
                        throw new InvalidOperationException("Bạn không thể tự từ chối/phê duyệt đơn của chính mình.");
                    }

                    if (currentStatus != "PENDING" && currentStatus != "APPROVED")
                    {
                        throw new InvalidOperationException($"Không thể từ chối đơn ở trạng thái: {currentStatus}.");
                    }

                    using (var cmdUpd = new OracleCommand(
                        "UPDATE HR.TB_YEUCAU_UNGLUONG SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p_uid, NGAYDUYET = SYSDATE, GHICHUDUYET = :p_reason WHERE ID = :p_id", conn))
                    {
                        cmdUpd.Transaction = tx;
                        cmdUpd.BindByName = true;
                        cmdUpd.Parameters.Add("p_uid", approverUserId);
                        cmdUpd.Parameters.Add("p_reason", reason.Trim());
                        cmdUpd.Parameters.Add("p_id", id);
                        cmdUpd.ExecuteNonQuery();
                    }

                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Đơn tạm ứng lương bị từ chối",
                        $"Đơn tạm ứng lương #{id} của bạn đã bị từ chối với lý do: {reason.Trim()}.",
                        "YEUCAU_UNGLUONG",
                        "HR_SYSTEM",
                        targetType: "PERSONAL",
                        targetManv: manv,
                        eventType: "SALARY_ADVANCE_REJECTED",
                        entityId: id,
                        discretePushBody: "HRMS: Đơn tạm ứng lương của bạn đã bị từ chối."
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public long DisburseAdvance(long id, long accountantUserId, DateTime ngayChi, string soChungTu, string ghiChuChi)
        {
            if (string.IsNullOrWhiteSpace(soChungTu))
                throw new ArgumentException("Vui lòng nhập số chứng từ chi tiền (Phiếu chi).");

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    long manv;
                    decimal amount;
                    int thang, nam;
                    string currentStatus;
                    long? existingIdUl;

                    using (var cmdLock = new OracleCommand(
                        "SELECT MANV, SOTIEN, THANG, NAM, TRANGTHAI, IDUL_LINK FROM HR.TB_YEUCAU_UNGLUONG WHERE ID = :p_id FOR UPDATE", conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", id);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy đơn tạm ứng lương.");
                            manv = Convert.ToInt64(r["MANV"]);
                            amount = Convert.ToDecimal(r["SOTIEN"]);
                            thang = Convert.ToInt32(r["THANG"]);
                            nam = Convert.ToInt32(r["NAM"]);
                            currentStatus = r["TRANGTHAI"]?.ToString();
                            existingIdUl = r["IDUL_LINK"] != DBNull.Value ? (long?)Convert.ToInt64(r["IDUL_LINK"]) : null;
                        }
                    }

                    if (currentStatus != "APPROVED")
                    {
                        throw new InvalidOperationException($"Chỉ có thể giải ngân đơn đã được duyệt (APPROVED). Trạng thái hiện tại: {currentStatus}.");
                    }
                    if (existingIdUl.HasValue)
                    {
                        throw new InvalidOperationException($"Đơn tạm ứng này đã được chi trước đó (Mã phiếu ứng: #{existingIdUl.Value}).");
                    }

                    // 1. Ghi nhận khoản ứng vào TB_UNGLUONG (Khoản chi thực tế)
                    long newIdUl;
                    using (var cmdMax = new OracleCommand("SELECT NVL(MAX(IDUL), 0) + 1 FROM HR.TB_UNGLUONG", conn))
                    {
                        cmdMax.Transaction = tx;
                        newIdUl = Convert.ToInt64(cmdMax.ExecuteScalar());
                    }

                    string insertUlSql = @"
                        INSERT INTO HR.TB_UNGLUONG 
                        (IDUL, MANV, NAM, THANG, NGAY, SOTIENUNG, GHICHU, CREATED_DATE, CREATED_BY)
                        VALUES (:p_idul, :p_manv, :p_nam, :p_thang, :p_ngay, :p_sotien, :p_ghichu, SYSDATE, :p_uid)";

                    using (var cmdInsert = new OracleCommand(insertUlSql, conn))
                    {
                        cmdInsert.Transaction = tx;
                        cmdInsert.BindByName = true;
                        cmdInsert.Parameters.Add("p_idul", newIdUl);
                        cmdInsert.Parameters.Add("p_manv", manv);
                        cmdInsert.Parameters.Add("p_nam", (decimal)nam);
                        cmdInsert.Parameters.Add("p_thang", (decimal)thang);
                        cmdInsert.Parameters.Add("p_ngay", (decimal)ngayChi.Day);
                        cmdInsert.Parameters.Add("p_sotien", amount);
                        cmdInsert.Parameters.Add("p_ghichu", $"Chi ứng lương theo đơn #{id} (Chứng từ: {soChungTu.Trim()})");
                        cmdInsert.Parameters.Add("p_uid", accountantUserId);
                        cmdInsert.ExecuteNonQuery();
                    }

                    // 2. Cập nhật TB_YEUCAU_UNGLUONG sang DISBURSED và liên kết IDUL_LINK
                    string updateYcSql = @"
                        UPDATE HR.TB_YEUCAU_UNGLUONG 
                        SET TRANGTHAI = 'DISBURSED', 
                            IDUL_LINK = :p_idul, 
                            NGUOI_CHI = :p_uid, 
                            NGAY_CHI = :p_ngaychi, 
                            SO_CHUNG_TU = :p_soct, 
                            GHI_CHU_CHI = :p_ghichu
                        WHERE ID = :p_id";

                    using (var cmdUpdYc = new OracleCommand(updateYcSql, conn))
                    {
                        cmdUpdYc.Transaction = tx;
                        cmdUpdYc.BindByName = true;
                        cmdUpdYc.Parameters.Add("p_idul", newIdUl);
                        cmdUpdYc.Parameters.Add("p_uid", accountantUserId);
                        cmdUpdYc.Parameters.Add("p_ngaychi", ngayChi.Date);
                        cmdUpdYc.Parameters.Add("p_soct", soChungTu.Trim());
                        cmdUpdYc.Parameters.Add("p_ghichu", (object)ghiChuChi?.Trim() ?? DBNull.Value);
                        cmdUpdYc.Parameters.Add("p_id", id);
                        cmdUpdYc.ExecuteNonQuery();
                    }

                    // 3. Thông báo cho nhân viên
                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Đã giải ngân tạm ứng lương",
                        $"Khoản tạm ứng lương {amount:N0} VNĐ (Chứng từ: {soChungTu.Trim()}) đã được kế toán giải ngân thành công.",
                        "YEUCAU_UNGLUONG",
                        "KE_TOAN",
                        targetType: "PERSONAL",
                        targetManv: manv,
                        eventType: "SALARY_ADVANCE_DISBURSED",
                        entityId: id,
                        discretePushBody: "HRMS: Khoản tạm ứng lương của bạn đã được kế toán giải ngân."
                    );

                    tx.Commit();
                    return newIdUl;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
