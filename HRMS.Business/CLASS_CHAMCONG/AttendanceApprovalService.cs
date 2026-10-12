using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Bu.CLASS_CHAMCONG
{
    public class AttendanceApprovalService
    {
        private readonly string _connectionString;

        public AttendanceApprovalService(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                using (var db = new MyEntities())
                {
                    _connectionString = AttendanceCalculationHelper.GetStoreConnectionString(db);
                }
            }
            else
            {
                _connectionString = connectionString;
            }
        }

        private static void InvalidateOpenPeriods(OracleConnection conn,OracleTransaction tx,DateTime start,DateTime end)
        {
            // Use the same period locks as the publisher; reject edits to frozen periods.
            for (var month=new DateTime(start.Year,start.Month,1); month<=end; month=month.AddMonths(1))
            {
                int period=month.Year*100+month.Month;
                using (var cmd=new OracleCommand("SELECT NVL(KHOA,0) FROM HR.TB_KYCONG WHERE MAKYCONG=:period FOR UPDATE",conn))
                {
                    cmd.Transaction=tx;cmd.BindByName=true;cmd.Parameters.Add("period",period);
                    var locked=cmd.ExecuteScalar();
                    if (locked==null) throw new InvalidOperationException("Kỳ công "+period+" chưa được tạo. Tạo kỳ công trước khi duyệt.");
                    if (Convert.ToDecimal(locked)!=0) throw new InvalidOperationException("Kỳ công "+period+" đã khóa. Mở khóa theo quy trình trước khi điều chỉnh.");
                }
                using (var cmd=new OracleCommand("UPDATE HR.TB_KYCONG SET CONG_INPUT_REV=NVL(CONG_INPUT_REV,0)+1 WHERE MAKYCONG=:period",conn))
                {
                    cmd.Transaction=tx;cmd.BindByName=true;cmd.Parameters.Add("period",period);cmd.ExecuteNonQuery();
                }
            }
        }

        private OracleConnection GetConnection()
        {
            var conn = new OracleConnection(_connectionString);
            if (conn.State != ConnectionState.Open) conn.Open();
            return conn;
        }

        /// <summary>
        /// Phê duyệt giải trình điều chỉnh công với tính toán công thực tế theo ca kíp (không hardcode 08:00/17:00, không nuốt lỗi)
        /// </summary>
        public void ApproveAttendanceCorrection(decimal requestId, decimal approverUserId, decimal? approverEmployeeId)
        {
            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    decimal manv;
                    DateTime ngayCong;
                    string gioVaoStr;
                    string gioRaStr;
                    string currentStatus;

                    string lockSql = @"
                        SELECT MANV, NGAY, GIO_VAO, GIO_RA, TRANGTHAI
                        FROM HR.TB_YEUCAU_DIEUCHINHCONG
                        WHERE ID = :p_id FOR UPDATE";

                    using (var cmdLock = new OracleCommand(lockSql, conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", requestId);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy yêu cầu điều chỉnh chấm công.");
                            manv = Convert.ToDecimal(r["MANV"]);
                            ngayCong = Convert.ToDateTime(r["NGAY"]);
                            gioVaoStr = r["GIO_VAO"]?.ToString();
                            gioRaStr = r["GIO_RA"]?.ToString();
                            currentStatus = r["TRANGTHAI"]?.ToString();
                        }
                    }

                    if (approverEmployeeId.HasValue && approverEmployeeId.Value == manv)
                    {
                        throw new InvalidOperationException("Bạn không được phép tự phê duyệt yêu cầu điều chỉnh công của chính mình.");
                    }

                    if (currentStatus != "PENDING")
                    {
                        throw new InvalidOperationException($"Yêu cầu không ở trạng thái chờ duyệt (Trạng thái hiện tại: {currentStatus}).");
                    }

                    InvalidateOpenPeriods(conn,tx,ngayCong.Date,ngayCong.Date);
                    // An unchanged punch comes from the original record, never a fabricated 08:00/17:00.
                    if (string.IsNullOrWhiteSpace(gioVaoStr) || string.IsNullOrWhiteSpace(gioRaStr))
                    {
                        using (var original=new OracleCommand("SELECT GIOVAO,GIORA FROM HR.TB_BANGCONG_CHITIET WHERE MANV=:manv AND TRUNC(NGAY)=:day FOR UPDATE",conn))
                        {
                            original.Transaction=tx;original.BindByName=true;original.Parameters.Add("manv",manv);original.Parameters.Add("day",ngayCong.Date);
                            using (var reader=original.ExecuteReader()) if (reader.Read())
                            {
                                if (string.IsNullOrWhiteSpace(gioVaoStr)) gioVaoStr=reader["GIOVAO"]==DBNull.Value ? null:reader["GIOVAO"].ToString();
                                if (string.IsNullOrWhiteSpace(gioRaStr)) gioRaStr=reader["GIORA"]==DBNull.Value ? null:reader["GIORA"].ToString();
                            }
                        }
                    }
                    // 1. Xác định ca làm việc thực tế của nhân viên
                    var allShifts = AttendanceCalculationHelper.LoadAllShifts();
                    var shift = AttendanceCalculationHelper.GetShiftForEmployee(allShifts, (int)manv, ngayCong);

                    // 2. Parse giờ vào/ra đề xuất
                    DateTime? dtVao = null;
                    DateTime? dtRa = null;

                    if (!string.IsNullOrWhiteSpace(gioVaoStr))
                    {
                        if (TimeSpan.TryParse(gioVaoStr.Trim(), out var tsVao))
                        {
                            dtVao = ngayCong.Date.Add(tsVao);
                        }
                        else if (DateTime.TryParse(gioVaoStr.Trim(), out var dtv))
                        {
                            dtVao = dtv;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(gioRaStr))
                    {
                        if (TimeSpan.TryParse(gioRaStr.Trim(), out var tsRa))
                        {
                            // Hỗ trợ ca đêm qua ngày: nếu ra < vào thì giờ ra thuộc ngày hôm sau
                            if (dtVao.HasValue && tsRa < dtVao.Value.TimeOfDay && shift.IsNightShift)
                            {
                                dtRa = ngayCong.Date.AddDays(1).Add(tsRa);
                            }
                            else
                            {
                                dtRa = ngayCong.Date.Add(tsRa);
                            }
                        }
                        else if (DateTime.TryParse(gioRaStr.Trim(), out var dtr))
                        {
                            dtRa = dtr;
                        }
                    }

                    if (!dtVao.HasValue || !dtRa.HasValue || dtRa.Value<=dtVao.Value)
                        throw new InvalidOperationException("Đơn điều chỉnh thiếu giờ hợp lệ. Khi dữ liệu gốc thiếu, cần khai báo cả giờ vào và giờ ra.");
                    if (dtVao.Value.Date!=ngayCong.Date || dtRa.Value.Date>ngayCong.Date.AddDays(1))
                        throw new InvalidOperationException("Khoảng giờ điều chỉnh không phù hợp với ngày công.");
                    // 3. Tính toán công nghiệp vụ chính xác
                    var calcResult = AttendanceCalculationHelper.Calculate(
                        shift,
                        isDiLam: true,
                        isCongTac: false,
                        phanNghi: null,
                        loaiNghi: null,
                        gioVao: dtVao,
                        gioRa: dtRa,
                        coLogQuetTheGoc: true,
                        date: ngayCong
                    );

                    // 4. Đồng bộ vào TB_BANGCONG_CHITIET
                    int makycong = ngayCong.Year * 100 + ngayCong.Month;
                    string updateBcSql = @"
                        UPDATE HR.TB_BANGCONG_CHITIET 
                        SET GIOVAO = :p_gv, 
                            GIORA = :p_gr, 
                            NGAYCONG = :p_nc, 
                            KYHIEU = :p_kh,
                            CONGCHUNHAT = :p_cn,
                            CONGNGAYLE = :p_le
                        WHERE MANV = :p_manv AND TRUNC(NGAY) = TRUNC(:p_ngay)";

                    using (var cmdBc = new OracleCommand(updateBcSql, conn))
                    {
                        cmdBc.Transaction = tx;
                        cmdBc.BindByName = true;
                        cmdBc.Parameters.Add("p_gv", dtVao.HasValue ? dtVao.Value.ToString("HH:mm") : (object)DBNull.Value);
                        cmdBc.Parameters.Add("p_gr", dtRa.HasValue ? dtRa.Value.ToString("HH:mm") : (object)DBNull.Value);
                        cmdBc.Parameters.Add("p_nc", calcResult.NgayCong);
                        cmdBc.Parameters.Add("p_kh", calcResult.KyHieu ?? "X");
                        cmdBc.Parameters.Add("p_cn", (ngayCong.DayOfWeek == DayOfWeek.Sunday) ? calcResult.NgayCong : 0m);
                        cmdBc.Parameters.Add("p_le", 0m);
                        cmdBc.Parameters.Add("p_manv", manv);
                        cmdBc.Parameters.Add("p_ngay", ngayCong.Date);
                        int affectedRows = cmdBc.ExecuteNonQuery();

                        // Nếu chưa có dòng công ngày đó thì tạo mới
                        if (affectedRows == 0)
                        {
                            string insertBcSql = @"
                                INSERT INTO HR.TB_BANGCONG_CHITIET 
                                (MAKYCONG, MANV, NGAY, THU, GIOVAO, GIORA, NGAYCONG, KYHIEU, CONGCHUNHAT, CONGNGAYLE)
                                VALUES (:p_mkc, :p_manv, :p_ngay, :p_thu, :p_gv, :p_gr, :p_nc, :p_kh, :p_cn, 0)";
                            using (var cmdInsBc = new OracleCommand(insertBcSql, conn))
                            {
                                cmdInsBc.Transaction = tx;
                                cmdInsBc.BindByName = true;
                                cmdInsBc.Parameters.Add("p_mkc", makycong);
                                cmdInsBc.Parameters.Add("p_manv", manv);
                                cmdInsBc.Parameters.Add("p_ngay", ngayCong.Date);
                                cmdInsBc.Parameters.Add("p_thu", ((int)ngayCong.DayOfWeek + 1).ToString());
                                cmdInsBc.Parameters.Add("p_gv", dtVao.HasValue ? dtVao.Value.ToString("HH:mm") : (object)DBNull.Value);
                                cmdInsBc.Parameters.Add("p_gr", dtRa.HasValue ? dtRa.Value.ToString("HH:mm") : (object)DBNull.Value);
                                cmdInsBc.Parameters.Add("p_nc", calcResult.NgayCong);
                                cmdInsBc.Parameters.Add("p_kh", calcResult.KyHieu ?? "X");
                                cmdInsBc.Parameters.Add("p_cn", (ngayCong.DayOfWeek == DayOfWeek.Sunday) ? calcResult.NgayCong : 0m);
                                cmdInsBc.ExecuteNonQuery();
                            }
                        }
                    }

                    // 5. Cập nhật lại tổng ngày công tháng trong TB_KYCONGCHITIET
                    string updateKcctSql = @"
                        UPDATE HR.TB_KYCONGCHITIET
                        SET TONGNGAYCONG = (
                            SELECT NVL(SUM(NGAYCONG), 0)
                            FROM HR.TB_BANGCONG_CHITIET
                            WHERE MAKYCONG = :p_mkc AND MANV = :p_manv
                        )
                        WHERE MAKYCONG = :p_mkc AND MANV = :p_manv";

                    using (var cmdKcct = new OracleCommand(updateKcctSql, conn))
                    {
                        cmdKcct.Transaction = tx;
                        cmdKcct.BindByName = true;
                        cmdKcct.Parameters.Add("p_mkc", makycong);
                        cmdKcct.Parameters.Add("p_manv", manv);
                        cmdKcct.ExecuteNonQuery();
                    }

                    // 6. Cập nhật trạng thái yêu cầu sang APPROVED
                    string updateReqSql = @"
                        UPDATE HR.TB_YEUCAU_DIEUCHINHCONG 
                        SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p_uid, NGAYDUYET = SYSDATE, GHICHUDUYET = :p_note, THOIDIEM_VAO_MOI=:input, THOIDIEM_RA_MOI=:output, GIO_VAO=:inText, GIO_RA=:outText
                        WHERE ID = :p_id";

                    using (var cmdReq = new OracleCommand(updateReqSql, conn))
                    {
                        cmdReq.Transaction = tx;
                        cmdReq.BindByName = true;
                        cmdReq.Parameters.Add("input",dtVao.Value);cmdReq.Parameters.Add("output",dtRa.Value);
                        cmdReq.Parameters.Add("inText",dtVao.Value.ToString("HH:mm"));cmdReq.Parameters.Add("outText",dtRa.Value.ToString("HH:mm"));
                        cmdReq.Parameters.Add("p_uid", approverUserId);
                        cmdReq.Parameters.Add("p_note", $"Phê duyệt công: {calcResult.NgayCong:N1} công ({calcResult.KyHieu})");
                        cmdReq.Parameters.Add("p_id", requestId);
                        cmdReq.ExecuteNonQuery();
                    }

                    // 7. Gửi thông báo đến nhân viên
                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Điều chỉnh công đã được phê duyệt",
                        $"Giải trình chấm công ngày {ngayCong:dd/MM/yyyy} của bạn đã được phê duyệt thành công ({calcResult.NgayCong:N1} công).",
                        "DIEUCHINH_CONG",
                        "HR_SYSTEM",
                        targetType: "PERSONAL",
                        targetManv: (long)manv,
                        eventType: "ATTENDANCE_CORRECTION",
                        entityId: requestId,
                        discretePushBody: "HRMS: Yêu cầu điều chỉnh công của bạn đã được phê duyệt."
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

        /// <summary>
        /// Phê duyệt đơn xin nghỉ phép có ghi sổ phép đầy đủ, rollback nếu có lỗi
        /// </summary>
        public void ApproveLeave(decimal requestId, decimal approverUserId, decimal? approverEmployeeId)
        {
            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    decimal manv;
                    DateTime tuNgay,denNgay,startTime,endTime;
                    string benefit,payer;
                    decimal soNgay;
                    long giayNghi;
                    string loaiPhep;
                    string lyDo;
                    string currentStatus;

                    string lockSql = @"
                        SELECT MANV, TUNGAY, DENNGAY, BATDAU_NGHI, KETTHUC_NGHI, LOAI_HUONG_CONG, NGUON_CHI_TRA, SONGAY, GIAY_NGHI, LOAIPHEP, LYDO, TRANGTHAI
                        FROM HR.TB_YEUCAU_NGHIPHEP
                        WHERE ID = :p_id FOR UPDATE";

                    using (var cmdLock = new OracleCommand(lockSql, conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", requestId);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy đơn xin nghỉ phép.");
                            manv = Convert.ToDecimal(r["MANV"]);
                            tuNgay = r["TUNGAY"] != DBNull.Value ? Convert.ToDateTime(r["TUNGAY"]) : DateTime.Today;
                            soNgay = r["SONGAY"] != DBNull.Value ? Convert.ToDecimal(r["SONGAY"]) : 1m;
                            giayNghi = r["GIAY_NGHI"] != DBNull.Value ? Convert.ToInt64(r["GIAY_NGHI"]) : (long)(soNgay * 28800);
                            loaiPhep = r["LOAIPHEP"]?.ToString();
                            lyDo = r["LYDO"]?.ToString();
                            currentStatus = r["TRANGTHAI"]?.ToString();
                            denNgay=r["DENNGAY"]==DBNull.Value ? tuNgay:Convert.ToDateTime(r["DENNGAY"]);
                            startTime=r["BATDAU_NGHI"]==DBNull.Value ? tuNgay.Date:Convert.ToDateTime(r["BATDAU_NGHI"]);
                            endTime=r["KETTHUC_NGHI"]==DBNull.Value ? denNgay.Date.AddDays(1):Convert.ToDateTime(r["KETTHUC_NGHI"]);
                            benefit=r["LOAI_HUONG_CONG"]==DBNull.Value ? (loaiPhep=="Nghỉ phép năm" ? "PHEP_NAM":loaiPhep=="Nghỉ không lương" ? "KHONG_LUONG":loaiPhep!=null && loaiPhep.IndexOf("BHXH",StringComparison.OrdinalIgnoreCase)>=0 ? "NGHI_BHXH":"LEGACY_UNCLASSIFIED"):r["LOAI_HUONG_CONG"].ToString();
                            payer=r["NGUON_CHI_TRA"]==DBNull.Value ? (benefit=="PHEP_NAM" ? "CONG_TY":benefit=="NGHI_BHXH" ? "BHXH":benefit=="KHONG_LUONG" ? "KHONG_CHI_TRA":"CHUA_XAC_DINH"):r["NGUON_CHI_TRA"].ToString();
                        }
                    }

                    if (approverEmployeeId.HasValue && approverEmployeeId.Value == manv)
                    {
                        throw new InvalidOperationException("Bạn không được phép tự phê duyệt đơn xin nghỉ phép của chính mình.");
                    }

                    if (currentStatus != "PENDING")
                    {
                        throw new InvalidOperationException($"Yêu cầu không ở trạng thái chờ duyệt (Trạng thái hiện tại: {currentStatus}).");
                    }

                    if (endTime<=startTime) throw new InvalidOperationException("Khoảng thời gian nghỉ không hợp lệ.");
                    if (benefit=="LEGACY_UNCLASSIFIED" || payer=="CHUA_XAC_DINH") throw new InvalidOperationException("Cần xác định loại hưởng công và nguồn chi trả của đơn trước khi duyệt.");
                    InvalidateOpenPeriods(conn,tx,startTime.Date,endTime.AddTicks(-1).Date);
                    // 1. Cập nhật trạng thái yêu cầu
                    using (var cmdUpd = new OracleCommand(
                        "UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p_uid, NGAYDUYET = SYSDATE, BATDAU_NGHI=:startTime, KETTHUC_NGHI=:endTime, GIAY_NGHI=:seconds, LOAI_HUONG_CONG=:benefit, NGUON_CHI_TRA=:payer WHERE ID = :p_id", conn))
                    {
                        cmdUpd.Transaction = tx;
                        cmdUpd.BindByName = true;
                        cmdUpd.Parameters.Add("startTime",startTime);cmdUpd.Parameters.Add("endTime",endTime);cmdUpd.Parameters.Add("seconds",giayNghi);cmdUpd.Parameters.Add("benefit",benefit);cmdUpd.Parameters.Add("payer",payer);
                        cmdUpd.Parameters.Add("p_uid", approverUserId);
                        cmdUpd.Parameters.Add("p_id", requestId);
                        cmdUpd.ExecuteNonQuery();
                    }

                    // 2. Ghi sổ phát sinh phép TB_PHEP_SOPHATSINH (bắt buộc trong transaction, không nuốt ngoại lệ!)
                    string insertPhepSql = @"
                        INSERT INTO HR.TB_PHEP_SOPHATSINH (MANV, NGAY, LOAI, GIAY_PHEP, IDDON, LY_DO)
                        VALUES (:p_manv, :p_ngay, 'SU_DUNG', :p_giay, :p_iddon, :p_lydo)";

                    if (benefit=="PHEP_NAM")
                    using (var cmdPhep = new OracleCommand(insertPhepSql, conn))
                    {
                        cmdPhep.Transaction = tx;
                        cmdPhep.BindByName = true;
                        cmdPhep.Parameters.Add("p_manv", manv);
                        cmdPhep.Parameters.Add("p_ngay", tuNgay.Date);
                        cmdPhep.Parameters.Add("p_giay", -Math.Abs(giayNghi));
                        cmdPhep.Parameters.Add("p_iddon", requestId);
                        cmdPhep.Parameters.Add("p_lydo", $"Phê duyệt nghỉ phép - {loaiPhep ?? lyDo ?? "Nghỉ phép"}");
                        cmdPhep.ExecuteNonQuery();
                    }

                    // 3. Thông báo cho nhân viên
                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Đơn xin nghỉ phép đã được phê duyệt",
                        $"Đơn xin nghỉ phép #{requestId} ({soNgay} ngày) của bạn đã được phê duyệt thành công.",
                        "NGHI_PHEP",
                        "HR_SYSTEM",
                        targetType: "PERSONAL",
                        targetManv: (long)manv,
                        eventType: "LEAVE_APPROVAL",
                        entityId: requestId,
                        discretePushBody: "HRMS: Đơn xin nghỉ phép của bạn đã được phê duyệt."
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
    }
}
