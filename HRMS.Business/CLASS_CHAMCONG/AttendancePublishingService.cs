using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bu.CLASS_CHAMCONG
{
    public class AttendancePublishRequest
    {
        public int MaKyCong { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public long NguoiThucHien { get; set; } = 1;
        public List<long> ManvList { get; set; }
        public string MaYeuCau { get; set; }
        public string GhiChu { get; set; }
        public bool ForceRecalculate { get; set; } = false;
    }

    public class AttendancePublishResult
    {
        public bool Success { get; set; }
        public long IdLanTinh { get; set; }
        public string MaYeuCau { get; set; }
        public string TrangThai { get; set; }
        public int SoNhanVien { get; set; }
        public int SoNgayCong { get; set; }
        public int SoNgaySanSang { get; set; }
        public int SoNgayChoXacMinh { get; set; }
        public int SoBatThuongPhatHien { get; set; }
        public string InputDataHash { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }

    /// <summary>
    /// Service công bố bảng chấm công (AttendancePublishingService) quản lý luồng tính toán ngoài transaction,
    /// khóa bi quan (pessimistic lock) ngắn hạn trên Oracle, đối soát revision và cập nhật con trỏ không phá hủy.
    /// Tuân thủ nghiêm ngặt V1_18 và các ràng buộc cơ sở dữ liệu Oracle 19c.
    /// </summary>
    public class AttendancePublishingService
    {
        private readonly TimeSegmentationEngine _engine;
        private readonly string _connectionString;

        public AttendancePublishingService(string connectionString = null)
        {
            _engine = new TimeSegmentationEngine();
            string cs = connectionString;
            if (string.IsNullOrEmpty(cs))
            {
                try
                {
                    using (var db = new MyEntities())
                    {
                        var objContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
                        var entityConn = objContext.Connection as System.Data.Entity.Core.EntityClient.EntityConnection;
                        if (entityConn != null && entityConn.StoreConnection != null)
                        {
                            cs = entityConn.StoreConnection.ConnectionString;
                        }
                        if (string.IsNullOrEmpty(cs))
                        {
                            cs = db.Database.Connection.ConnectionString;
                        }
                    }
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Missing Oracle connection configuration.");
            var builder = new OracleConnectionStringBuilder(cs);
            if (string.IsNullOrWhiteSpace(builder.UserID) || string.IsNullOrWhiteSpace(builder.DataSource))
                throw new InvalidOperationException("Oracle owner and data source must be explicit.");
            _connectionString = builder.ConnectionString;
        }

        private static OracleCommand CreateCmd(string sql, OracleConnection conn)
        {
            var cmd = new OracleCommand(sql, conn);
            cmd.BindByName = true;
            return cmd;
        }

        public AttendancePublishResult PublishAttendance(AttendancePublishRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.MaKyCong <= 0) throw new ArgumentException("Mã kỳ công không hợp lệ.");

            int nam = request.MaKyCong / 100;
            int thang = request.MaKyCong % 100;
            int daysInMonth = DateTime.DaysInMonth(nam, thang);

            DateTime tuNgay = request.TuNgay?.Date ?? new DateTime(nam, thang, 1);
            DateTime denNgay = request.DenNgay?.Date ?? new DateTime(nam, thang, daysInMonth).AddDays(1); // Ngày tiếp theo lúc 00:00:00
            DateTime monthStart = new DateTime(nam, thang, 1);
            if (tuNgay < monthStart || denNgay > monthStart.AddMonths(1) || denNgay <= tuNgay)
                throw new ArgumentException("Attendance range must be a nonempty [start,end) inside the period.");
            DateTime lastDayOfMonth = denNgay.AddDays(-1);
            string requestHash = ComputeSha256(JsonConvert.SerializeObject(new {
                request.MaKyCong, TuNgay = tuNgay, DenNgay = denNgay,
                Employees = request.ManvList?.Distinct().OrderBy(x => x).ToArray(),
                request.NguoiThucHien
            }));

            string maYeuCau = request.MaYeuCau;
            if (string.IsNullOrEmpty(maYeuCau))
            {
                string raw = $"PUB_{request.MaKyCong}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
                maYeuCau = raw.Length > 60 ? raw.Substring(0, 60) : raw;
            }
            else if (maYeuCau.Length > 60)
            {
                throw new ArgumentException("Request key exceeds 60 characters.");
            }

            var result = new AttendancePublishResult
            {
                MaYeuCau = maYeuCau
            };

            // BƯỚC 1: Nạp đầu vào nhất quán bên ngoài transaction công bố
            decimal currentKhoa = 0;
            long loadedInputRev = 0;
            long loadedPublishRev = 0;
            long validUserId = request.NguoiThucHien;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                var completedRequest = FindCompletedRun(conn, maYeuCau, requestHash);
                if (completedRequest != null) return completedRequest;

                // Đảm bảo NguoiThucHien trỏ đến một IDUSER hợp lệ trong TB_SYS_USER (tuân thủ FK18_RUN_USER)
                using (var chkUserCmd = CreateCmd("SELECT COUNT(*) FROM TB_SYS_USER WHERE IDUSER = :p_u", conn))
                {
                    chkUserCmd.Parameters.Add("p_u", validUserId);
                    int userExists = Convert.ToInt32(chkUserCmd.ExecuteScalar());
                    if (userExists == 0)
                    {
                        using (var getFirstCmd = CreateCmd("SELECT IDUSER FROM TB_SYS_USER WHERE ROWNUM = 1", conn))
                        {
                            var firstObj = getFirstCmd.ExecuteScalar();
                            if (firstObj != null && firstObj != DBNull.Value)
                            {
                                validUserId = Convert.ToInt64(firstObj);
                            }
                        }
                    }
                }

                // 1.1 Kiểm tra trạng thái kỳ công
                using (var cmd = CreateCmd(@"
                    SELECT NVL(KHOA, 0) AS KHOA, 
                           NVL(CONG_INPUT_REV, 0) AS CONG_INPUT_REV, 
                           NVL(CONG_PUBLISH_REV, 0) AS CONG_PUBLISH_REV 
                    FROM TB_KYCONG 
                    WHERE MAKYCONG = :p_mkc", conn))
                {
                    cmd.Parameters.Add("p_mkc", request.MaKyCong);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentKhoa = Convert.ToDecimal(reader["KHOA"]);
                            loadedInputRev = Convert.ToInt64(reader["CONG_INPUT_REV"]);
                            loadedPublishRev = Convert.ToInt64(reader["CONG_PUBLISH_REV"]);
                        }
                        else
                        {
                            result.Success = false;
                            result.TrangThai = "THAT_BAI";
                            result.ErrorMessage = $"Không tìm thấy kỳ công {request.MaKyCong} trong TB_KYCONG.";
                            return result;
                        }
                    }
                }

                if (currentKhoa != 0)
                {
                    result.Success = false;
                    result.TrangThai = "THAT_BAI";
                    result.ErrorMessage = $"Kỳ công {request.MaKyCong} đã bị khóa (KHOA = {currentKhoa}), không thể công bố dữ liệu.";
                    return result;
                }

                // 1.2 Nạp Quy định chấm công hiệu lực
                // Policy is resolved for the actual schedule and day, never once for the whole month.

                // 1.3 Nạp Danh sách nhân viên cần tính
                var targetManvList = new List<long>();
                if (request.ManvList != null && request.ManvList.Count > 0)
                {
                    targetManvList.AddRange(request.ManvList.Distinct());
                }
                else
                {
                    // Ưu tiên 1: Lấy nhân viên từ kỳ công chi tiết đã có của kỳ này
                    using (var cmd = CreateCmd("SELECT DISTINCT MANV FROM TB_KYCONGCHITIET WHERE MAKYCONG = :p_mkc ORDER BY MANV", conn))
                    {
                        cmd.Parameters.Add("p_mkc", request.MaKyCong);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                targetManvList.Add(Convert.ToInt64(reader["MANV"]));
                            }
                        }
                    }

                    // Ưu tiên 2: Nếu chưa có trong TB_KYCONGCHITIET, lấy từ TB_BANGCONG_CHITIET
                    if (targetManvList.Count == 0)
                    {
                        using (var cmd = CreateCmd("SELECT DISTINCT MANV FROM TB_BANGCONG_CHITIET WHERE MAKYCONG = :p_mkc ORDER BY MANV", conn))
                        {
                            cmd.Parameters.Add("p_mkc", request.MaKyCong);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    targetManvList.Add(Convert.ToInt64(reader["MANV"]));
                                }
                            }
                        }
                    }

                    // Ưu tiên 3: Nếu vẫn rỗng, fallback về toàn bộ nhân sự chưa thôi việc
                    if (targetManvList.Count == 0)
                    {
                        using (var cmd = CreateCmd("SELECT MANV FROM TB_NHANVIEN WHERE NVL(DATHOIVIEC, 0) = 0 ORDER BY MANV", conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                targetManvList.Add(Convert.ToInt64(reader["MANV"]));
                            }
                        }
                    }
                }

                if (targetManvList.Count == 0)
                {
                    result.Success = true;
                    result.TrangThai = "DA_CONG_BO";
                    result.ErrorMessage = "Không có nhân viên nào trong phạm vi xử lý.";
                    return result;
                }

                // 1.4 Nạp các phiên bản ca và khung giờ
                var shifts = LoadShiftsWithFrames(conn);

                // 1.4.1 Tự động bảo đảm lịch làm việc phân công chuẩn tồn tại cho tất cả nhân sự trong kỳ nếu chưa có
                EnsureSchedulesExist(conn, targetManvList, tuNgay, lastDayOfMonth, validUserId);

                // 1.5 Nạp lịch làm việc của nhân viên
                var schedules = LoadSchedules(conn, targetManvList, tuNgay, lastDayOfMonth);

                // 1.6 Nạp quẹt thẻ thô từ TB_BANGCONG
                var rawPunches = LoadRawPunches(conn, targetManvList, nam, thang);

                // 1.7 Nạp đơn tăng ca đã duyệt
                var approvedOts = LoadApprovedOvertimes(conn, targetManvList, tuNgay, denNgay);

                // 1.8 Nạp đơn nghỉ phép đã duyệt
                var approvedLeaves = LoadApprovedLeaves(conn, targetManvList, tuNgay, denNgay);

                // 1.9 Nạp đơn điều chỉnh công đã duyệt
                var approvedAdjustments = LoadApprovedAdjustments(conn, targetManvList, tuNgay, lastDayOfMonth);

                // 1.10 Nạp hoặc đảm bảo sự tồn tại của TB_BANGCONG_CHITIET
                var bcctMap = EnsureAndLoadBangCongChiTiet(conn, targetManvList, request.MaKyCong, tuNgay, lastDayOfMonth, validUserId);

                // Đồng bộ lại loadedInputRev và loadedPublishRev sau khi hoàn tất các bước chuẩn bị (đảm bảo không bị coi là drift)
                using (var revSyncCmd = CreateCmd(@"
                    SELECT NVL(CONG_INPUT_REV, 0) AS CONG_INPUT_REV, 
                           NVL(CONG_PUBLISH_REV, 0) AS CONG_PUBLISH_REV 
                    FROM TB_KYCONG 
                    WHERE MAKYCONG = :p_mkc", conn))
                {
                    revSyncCmd.Parameters.Add("p_mkc", request.MaKyCong);
                    using (var r = revSyncCmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            loadedInputRev = Convert.ToInt64(r["CONG_INPUT_REV"]);
                            loadedPublishRev = Convert.ToInt64(r["CONG_PUBLISH_REV"]);
                        }
                    }
                }

                // BƯỚC 2: Tính toán thuần túy bằng TimeSegmentationEngine (Hoàn toàn ngoài Transaction)
                var calculatedDays = new List<DayCalculationResult>();
                var dayHashes = new List<string>();
                var policyCache = new Dictionary<(long, DateTime), AttendancePolicyDto>();

                for (DateTime d = tuNgay; d <= lastDayOfMonth; d = d.AddDays(1))
                {
                    foreach (var manv in targetManvList)
                    {
                        var key = (manv, d);
                        if (!bcctMap.TryGetValue(key, out long bcctId))
                        {
                            continue;
                        }

                        schedules.TryGetValue(key, out var schedule);

                        ShiftVersionDto shift = null;
                        if (schedule?.IdCaPhienBan != null && shifts.TryGetValue(schedule.IdCaPhienBan.Value, out var matchedShift))
                        {
                            shift = matchedShift;
                        }

                        rawPunches.TryGetValue(key, out var punchesForDay);
                        approvedOts.TryGetValue(key, out var otsForDay);
                        approvedLeaves.TryGetValue(key, out var leavesForDay);
                        approvedAdjustments.TryGetValue(key, out var adjForDay);

                        AttendancePolicyDto policy = new AttendancePolicyDto();
                        if (schedule?.IdQuyDinh != null)
                        {
                            var policyKey = (schedule.IdQuyDinh.Value, d);
                            if (!policyCache.TryGetValue(policyKey, out policy))
                            {
                                policy = LoadActivePolicy(conn, d, d.AddDays(1), schedule.IdQuyDinh);
                                policyCache[policyKey] = policy;
                            }
                        }
                        var dayInput = new DayCalculationInput
                        {
                            IdLanTinh = 0, // Sẽ được cập nhật sau khi sinh IDLANTINH
                            IdBangCongCt = bcctId,
                            MaNV = manv,
                            MaKyCong = request.MaKyCong,
                            Ngay = d,
                            Schedule = schedule,
                            Shift = shift,
                            Policy = policy,
                            RawPunches = punchesForDay ?? new List<RawPunchDto>(),
                            ApprovedOvertimes = otsForDay ?? new List<ApprovedOvertimeDto>(),
                            ApprovedLeaves = leavesForDay ?? new List<ApprovedLeaveDto>(),
                            ApprovedAdjustment = adjForDay
                        };

                        var dayRes = _engine.ProcessDay(dayInput);
                        calculatedDays.Add(dayRes);
                        dayHashes.Add(dayRes.InputHash);
                    }
                }

                // Băm toàn bộ dữ liệu đầu vào của lượt chạy
                string overallDataHash = ComputeSha256(string.Join("|", dayHashes));
                result.InputDataHash = overallDataHash;
                result.SoNhanVien = targetManvList.Count;
                result.SoNgayCong = calculatedDays.Count;
                result.SoNgaySanSang = calculatedDays.Count(c => c.DuDieuKienChot == 1);
                result.SoNgayChoXacMinh = calculatedDays.Count(c => c.TrangThaiCong == "CHO_XAC_MINH");
                result.SoBatThuongPhatHien = calculatedDays.Sum(c => c.Anomalies.Count);

                // BƯỚC 3: Bắt đầu Transaction ngắn, thực hiện pessimistic locking và kiểm tra concurrency
                using (var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 3.1 Khóa dòng kỳ công bằng SELECT FOR UPDATE
                        decimal lockedKhoa = 0;
                        long currentInputRev = 0;
                        long currentPublishRev = 0;

                        using (var lockCmd = CreateCmd(@"
                            SELECT NVL(KHOA, 0) AS KHOA, 
                                   NVL(CONG_INPUT_REV, 0) AS CONG_INPUT_REV, 
                                   NVL(CONG_PUBLISH_REV, 0) AS CONG_PUBLISH_REV 
                            FROM TB_KYCONG 
                            WHERE MAKYCONG = :p_mkc 
                            FOR UPDATE WAIT 5", conn))
                        {
                            lockCmd.Parameters.Add("p_mkc", request.MaKyCong);
                            using (var reader = lockCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    lockedKhoa = Convert.ToDecimal(reader["KHOA"]);
                                    currentInputRev = Convert.ToInt64(reader["CONG_INPUT_REV"]);
                                    currentPublishRev = Convert.ToInt64(reader["CONG_PUBLISH_REV"]);
                                }
                            }
                        }

                        if (lockedKhoa != 0)
                        {
                            tx.Rollback();
                            result.Success = false;
                            result.TrangThai = "THAT_BAI";
                            result.ErrorMessage = $"Kỳ công {request.MaKyCong} đã bị khóa trong khi đang chuẩn bị công bố.";
                            return result;
                        }

                        // 3.2 Kiểm tra va chạm đồng thời: Input Revision đã bị thay đổi trong lúc tính toán
                        var alreadyPublished = FindCompletedRun(conn, maYeuCau, requestHash);
                        if (alreadyPublished != null) { tx.Rollback(); return alreadyPublished; }
                        if (!request.ForceRecalculate && (currentInputRev != loadedInputRev || currentPublishRev != loadedPublishRev))
                        {
                            tx.Rollback();

                            // Ghi nhận CAN_TINH_LAI ở một transaction ngắn độc lập
                            RecordCancelledRun(request.MaKyCong, maYeuCau, tuNgay, denNgay, currentInputRev, currentPublishRev, request.NguoiThucHien, "Dữ liệu nguồn đã thay đổi trong khi đang tính toán (Input revision drift).");

                            result.Success = false;
                            result.TrangThai = "CAN_TINH_LAI";
                            result.ErrorMessage = "Dữ liệu nguồn đã thay đổi trong lúc tính toán. Đã đánh dấu CAN_TINH_LAI.";
                            return result;
                        }

                        // 3.3 Chèn bản ghi chạy vào TB_CHAMCONG_LANTINH
                        long idLanTinh = 0;
                        string reqHash = requestHash;
                        long nextPublishRev = currentPublishRev + 1;

                        using (var runCmd = CreateCmd(@"
                            INSERT INTO TB_CHAMCONG_LANTINH (
                                MA_YEU_CAU, REQUEST_HASH, MAKYCONG, TU_NGAY, DEN_NGAY,
                                INPUT_REV, EXPECTED_PUBLISH_REV, INPUT_DATA_HASH,
                                PHIENBAN_ENGINE, MUI_GIO, TRANG_THAI, BATDAU_TINH,
                                KETTHUC_TINH, CONGBO_LUC, NGUOI_THUC_HIEN, GHI_CHU
                            ) VALUES (
                                :p_mayeucau, :p_reqhash, :p_makycong, :p_tungay, :p_denngay,
                                :p_input_rev, :p_exp_pub_rev, :p_data_hash,
                                '1.19', 'Asia/Ho_Chi_Minh', 'DA_CONG_BO', SYSTIMESTAMP,
                                SYSTIMESTAMP, SYSTIMESTAMP, :p_user, :p_ghichu
                            ) RETURNING IDLANTINH INTO :p_out_id", conn))
                        {
                            runCmd.Parameters.Add("p_mayeucau", maYeuCau);
                            runCmd.Parameters.Add("p_reqhash", reqHash);
                            runCmd.Parameters.Add("p_makycong", request.MaKyCong);
                            runCmd.Parameters.Add("p_tungay", tuNgay);
                            runCmd.Parameters.Add("p_denngay", denNgay);
                            runCmd.Parameters.Add("p_input_rev", currentInputRev);
                            runCmd.Parameters.Add("p_exp_pub_rev", nextPublishRev);
                            runCmd.Parameters.Add("p_data_hash", overallDataHash);
                            runCmd.Parameters.Add("p_user", validUserId);
                            runCmd.Parameters.Add("p_ghichu", request.GhiChu ?? "Công bố bảng chấm công thành công");

                            var pOutId = new OracleParameter("p_out_id", OracleDbType.Int64, ParameterDirection.Output);
                            runCmd.Parameters.Add(pOutId);

                            runCmd.ExecuteNonQuery();
                            idLanTinh = Convert.ToInt64(pOutId.Value.ToString());
                        }

                        result.IdLanTinh = idLanTinh;

                        // 3.4 Lưu kết quả ngày TB_CHAMCONG_KQ_NGAY
                        foreach (var day in calculatedDays)
                        {
                            day.IdLanTinh = idLanTinh;
                            using (var kqCmd = CreateCmd(@"
                                INSERT INTO TB_CHAMCONG_KQ_NGAY (
                                    IDLANTINH, IDBANGCONGCT, MANV, MAKYCONG, NGAY,
                                    INPUT_HASH, INPUT_SNAPSHOT, TRANG_THAI, DU_DIEUKIEN_CHOT,
                                    GIAY_THUC_TE, GIAY_HUONG_CONG_THUONG, GIAY_OT_XAC_NHAN,
                                    GIAY_DEM_TRONG_GIO_THUONG, GIAY_DEM_OT, GIAY_DI_MUON_THUC_TE,
                                    GIAY_VE_SOM_THUC_TE, GIAY_DI_MUON_VIPHAM, GIAY_VE_SOM_VIPHAM,
                                    CONG_THUONG_QUYDOI
                                ) VALUES (
                                    :p_idlt, :p_bcct, :p_manv, :p_makycong, :p_ngay,
                                    :p_hash, :p_snap, :p_tt, :p_ddk,
                                    :p_thucte, :p_huongcong, :p_ot,
                                    :p_dem_thuong, :p_dem_ot, :p_muon_tt,
                                    :p_som_tt, :p_muon_vp, :p_som_vp,
                                    :p_congquydoi
                                )", conn))
                            {
                                kqCmd.Parameters.Add("p_idlt", idLanTinh);
                                kqCmd.Parameters.Add("p_bcct", day.IdBangCongCt);
                                kqCmd.Parameters.Add("p_manv", day.MaNV);
                                kqCmd.Parameters.Add("p_makycong", day.MaKyCong);
                                kqCmd.Parameters.Add("p_ngay", day.Ngay);
                                kqCmd.Parameters.Add("p_hash", day.InputHash);
                                kqCmd.Parameters.Add("p_snap", day.InputSnapshotJson);
                                kqCmd.Parameters.Add("p_tt", day.TrangThaiCong);
                                kqCmd.Parameters.Add("p_ddk", day.DuDieuKienChot);
                                kqCmd.Parameters.Add("p_thucte", day.GiayThucTe);
                                kqCmd.Parameters.Add("p_huongcong", day.GiayHuongCongThuong);
                                kqCmd.Parameters.Add("p_ot", day.GiayOtXacNhan);
                                kqCmd.Parameters.Add("p_dem_thuong", day.GiayDemTrongGioThuong);
                                kqCmd.Parameters.Add("p_dem_ot", day.GiayDemOt);
                                kqCmd.Parameters.Add("p_muon_tt", day.GiayDiMuonThucTe);
                                kqCmd.Parameters.Add("p_som_tt", day.GiayVeSomThucTe);
                                kqCmd.Parameters.Add("p_muon_vp", day.GiayDiMuonViPham);
                                kqCmd.Parameters.Add("p_som_vp", day.GiayVeSomViPham);
                                kqCmd.Parameters.Add("p_congquydoi", day.CongThuongQuyDoi);

                                kqCmd.ExecuteNonQuery();
                            }

                            // 3.5 Lưu phân đoạn thời gian TB_CONG_PHANDOAN & nguồn TB_CONG_PD_NGUON
                            foreach (var seg in day.Segments)
                            {
                                seg.IdLanTinh = idLanTinh;
                                long idPhanDoan = 0;

                                using (var pdCmd = CreateCmd(@"
                                    INSERT INTO TB_CONG_PHANDOAN (
                                        IDLANTINH, IDBANGCONGCT, MANV, IDLICH, IDQUYDINH,
                                        BATDAU_LUC, KETTHUC_LUC, THOILUONG_GIAY,
                                        LOAI_THOIGIAN, LOAI_NGAY, LA_BAN_DEM, CO_OT_BAN_NGAY_TRUOC,
                                        TRANGTHAI_XACNHAN, GIAY_THUC_TE, GIAY_HUONG_CONG_THUONG,
                                        GIAY_OT_XAC_NHAN, GIAY_DEM_TRONG_GIO_THUONG, GIAY_DEM_OT,
                                        GIAY_DI_MUON_THUC_TE, GIAY_VE_SOM_THUC_TE,
                                        GIAY_DI_MUON_VIPHAM, GIAY_VE_SOM_VIPHAM,
                                        IDYEUCAU_TANGCA, IDYEUCAU_NGHIPHEP, IDYEUCAU_DIEUCHINH
                                    ) VALUES (
                                        :p_idlt, :p_bcct, :p_manv, :p_idlich, :p_idqd,
                                        :p_start, :p_end, :p_dur,
                                        :p_type, :p_daytype, :p_night, :p_otday,
                                        :p_status, :p_thucte, :p_huongcong,
                                        :p_ot, :p_dem_thuong, :p_dem_ot,
                                        :p_muon_tt, :p_som_tt,
                                        :p_muon_vp, :p_som_vp,
                                        :p_tc, :p_np, :p_dc
                                    ) RETURNING IDPHANDOAN INTO :p_out_pd", conn))
                                {
                                    pdCmd.Parameters.Add("p_idlt", idLanTinh);
                                    pdCmd.Parameters.Add("p_bcct", seg.IdBangCongCt);
                                    pdCmd.Parameters.Add("p_manv", seg.MaNV);
                                    pdCmd.Parameters.Add("p_idlich", (object)seg.IdLich ?? DBNull.Value);
                                    pdCmd.Parameters.Add("p_idqd", seg.IdQuyDinh);
                                    pdCmd.Parameters.Add("p_start", seg.BatDauLuc);
                                    pdCmd.Parameters.Add("p_end", seg.KetThucLuc);
                                    pdCmd.Parameters.Add("p_dur", seg.ThoiLuongGiay);
                                    pdCmd.Parameters.Add("p_type", seg.LoaiThoiGian);
                                    pdCmd.Parameters.Add("p_daytype", seg.LoaiNgay);
                                    pdCmd.Parameters.Add("p_night", seg.LaBanDem);
                                    pdCmd.Parameters.Add("p_otday", seg.CoOtBanNgayTruoc);
                                    pdCmd.Parameters.Add("p_status", seg.TrangThaiXacNhan);
                                    pdCmd.Parameters.Add("p_thucte", seg.GiayThucTe);
                                    pdCmd.Parameters.Add("p_huongcong", seg.GiayHuongCongThuong);
                                    pdCmd.Parameters.Add("p_ot", seg.GiayOtXacNhan);
                                    pdCmd.Parameters.Add("p_dem_thuong", seg.GiayDemTrongGioThuong);
                                    pdCmd.Parameters.Add("p_dem_ot", seg.GiayDemOt);
                                    pdCmd.Parameters.Add("p_muon_tt", seg.GiayDiMuonThucTe);
                                    pdCmd.Parameters.Add("p_som_tt", seg.GiayVeSomThucTe);
                                    pdCmd.Parameters.Add("p_muon_vp", seg.GiayDiMuonViPham);
                                    pdCmd.Parameters.Add("p_som_vp", seg.GiayVeSomViPham);
                                    pdCmd.Parameters.Add("p_tc", (object)seg.IdYeuCauTangCa ?? DBNull.Value);
                                    pdCmd.Parameters.Add("p_np", (object)seg.IdYeuCauNghiPhep ?? DBNull.Value);
                                    pdCmd.Parameters.Add("p_dc", (object)seg.IdYeuCauDieuChinh ?? DBNull.Value);

                                    var pOutPd = new OracleParameter("p_out_pd", OracleDbType.Int64, ParameterDirection.Output);
                                    pdCmd.Parameters.Add(pOutPd);

                                    pdCmd.ExecuteNonQuery();
                                    idPhanDoan = Convert.ToInt64(pOutPd.Value.ToString());
                                }

                                // Ghi nhận nguồn quẹt thẻ vào TB_CONG_PD_NGUON
                                if (seg.SourceMabcList != null && seg.SourceMabcList.Count > 0)
                                {
                                    foreach (var mabc in seg.SourceMabcList)
                                    {
                                        using (var srcCmd = CreateCmd(@"
                                            INSERT INTO TB_CONG_PD_NGUON (
                                                IDPHANDOAN, MANV, MABC, SOURCE_HASH, SOURCE_SNAPSHOT
                                            ) VALUES (
                                                :p_idpd, :p_manv, :p_mabc, :p_srchash, :p_srcsnap
                                            )", conn))
                                        {
                                            string srcSnapshot = $"{{\"Mabc\":{mabc},\"Manv\":{seg.MaNV}}}";
                                            srcCmd.Parameters.Add("p_idpd", idPhanDoan);
                                            srcCmd.Parameters.Add("p_manv", seg.MaNV);
                                            srcCmd.Parameters.Add("p_mabc", mabc);
                                            srcCmd.Parameters.Add("p_srchash", ComputeSha256(srcSnapshot));
                                            srcCmd.Parameters.Add("p_srcsnap", srcSnapshot);
                                            srcCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            // 3.6 Ghi nhận bất thường vào TB_CHAMCONG_BATTHUONG
                            foreach (var anom in day.Anomalies)
                            {
                                anom.IdLanTinhPhatHien = idLanTinh;
                                using (var anomCmd = CreateCmd(@"
                                    MERGE INTO TB_CHAMCONG_BATTHUONG t
                                    USING (SELECT :p_hash AS h FROM DUAL) s
                                    ON (t.MA_SU_VIEC_HASH = s.h)
                                    WHEN NOT MATCHED THEN
                                        INSERT (
                                            MA_SU_VIEC_HASH, IDLANTINH_PHATHIEN, IDBANGCONGCT, MANV, IDLICH, NGAY,
                                            MA_LOI, BATDAU_LUC, KETTHUC_LUC, MUC_DO, CHAN_CHOT, INPUT_HASH,
                                            INPUT_SNAPSHOT, TRANG_THAI, MO_TA
                                        ) VALUES (
                                            :p_hash, :p_idlt, :p_bcct, :p_manv, :p_idlich, :p_ngay,
                                            :p_maloi, :p_start, :p_end, :p_mucdo, :p_chan, :p_inhash,
                                            :p_insnap, :p_tt, :p_mota
                                        )", conn))
                                {
                                    anomCmd.Parameters.Add("p_hash", anom.MaSuViecHash);
                                    anomCmd.Parameters.Add("p_idlt", idLanTinh);
                                    anomCmd.Parameters.Add("p_inhash", anom.InputHash);
                                    anomCmd.Parameters.Add("p_insnap", anom.InputSnapshotJson);
                                    anomCmd.Parameters.Add("p_mota", anom.MoTa);
                                    anomCmd.Parameters.Add("p_bcct", anom.IdBangCongCt);
                                    anomCmd.Parameters.Add("p_manv", anom.MaNV);
                                    anomCmd.Parameters.Add("p_idlich", (object)anom.IdLich ?? DBNull.Value);
                                    anomCmd.Parameters.Add("p_ngay", anom.Ngay);
                                    anomCmd.Parameters.Add("p_maloi", anom.MaLoi);
                                    anomCmd.Parameters.Add("p_start", (object)anom.BatDauLuc ?? DBNull.Value);
                                    anomCmd.Parameters.Add("p_end", (object)anom.KetThucLuc ?? DBNull.Value);
                                    anomCmd.Parameters.Add("p_mucdo", anom.MucDo);
                                    anomCmd.Parameters.Add("p_chan", anom.ChanChot);
                                    anomCmd.Parameters.Add("p_tt", anom.TrangThai);

                                    anomCmd.ExecuteNonQuery();
                                }
                            }

                            // 3.7 Cập nhật con trỏ LANTINH_ID_HIENHANH trên TB_BANGCONG_CHITIET
                            using (var updBcct = CreateCmd(@"
                                UPDATE TB_BANGCONG_CHITIET SET
                                    LANTINH_ID_HIENHANH = :p_idlt,
                                    TRANGTHAI_CONG = :p_tt,
                                    DU_DIEUKIEN_CHOT = :p_ddk,
                                    GIAY_THUC_TE = :p_thucte,
                                    GIAY_HUONG_CONG_THUONG = :p_huongcong,
                                    GIAY_OT_XAC_NHAN = :p_ot,
                                    GIAY_DEM_TRONG_GIO_THUONG = :p_dem_thuong,
                                    GIAY_DEM_OT = :p_dem_ot,
                                    GIAY_DI_MUON_THUC_TE = :p_muon_tt,
                                    GIAY_VE_SOM_THUC_TE = :p_som_tt,
                                    GIAY_DI_MUON_VIPHAM = :p_muon_vp,
                                    GIAY_VE_SOM_VIPHAM = :p_som_vp,
                                    NGAYCONG = :p_ngaycong,
                                    KYHIEU = :p_symbol, NGAYPHEP = :p_leave, CONGNGAYLE = :p_holiday
                                WHERE IDBANGCONGCT = :p_bcct AND MANV = :p_manv", conn))
                            {
                                updBcct.Parameters.Add("p_idlt", idLanTinh);
                                updBcct.Parameters.Add("p_tt", day.TrangThaiCong);
                                updBcct.Parameters.Add("p_ddk", day.DuDieuKienChot);
                                updBcct.Parameters.Add("p_thucte", day.GiayThucTe);
                                updBcct.Parameters.Add("p_huongcong", day.GiayHuongCongThuong);
                                updBcct.Parameters.Add("p_ot", day.GiayOtXacNhan);
                                updBcct.Parameters.Add("p_dem_thuong", day.GiayDemTrongGioThuong);
                                updBcct.Parameters.Add("p_dem_ot", day.GiayDemOt);
                                updBcct.Parameters.Add("p_muon_tt", day.GiayDiMuonThucTe);
                                updBcct.Parameters.Add("p_som_tt", day.GiayVeSomThucTe);
                                updBcct.Parameters.Add("p_muon_vp", day.GiayDiMuonViPham);
                                updBcct.Parameters.Add("p_som_vp", day.GiayVeSomViPham);
                                updBcct.Parameters.Add("p_ngaycong", day.CongThuongQuyDoi);
                                long leaveSeconds=day.Segments.Where(x=>x.LoaiThoiGian=="PHEP_HUONG_LUONG" && x.LoaiNgay!="LE").Sum(x=>x.GiayHuongCongThuong);
                                decimal leaveDays=day.GiayHuongCongThuong>0 ? day.CongThuongQuyDoi*leaveSeconds/day.GiayHuongCongThuong : 0;
                                bool holiday=day.Segments.Any(x=>x.LoaiNgay=="LE" && x.GiayHuongCongThuong>0);
                                bool isSunday = (day.Ngay.DayOfWeek == DayOfWeek.Sunday);
                                if (!holiday)
                                {
                                    holiday = (day.Ngay.Month == 1 && day.Ngay.Day == 1) || (day.Ngay.Month == 4 && day.Ngay.Day == 30) || (day.Ngay.Month == 5 && day.Ngay.Day == 1) || (day.Ngay.Month == 9 && day.Ngay.Day == 2);
                                }
                                string symbol = holiday ? "L" :
                                    day.Segments.Any(x => x.LoaiThoiGian == "NGHI_BHXH") ? "BH" :
                                    leaveDays > 0 ? (leaveDays == day.CongThuongQuyDoi ? "P" : "P/X") :
                                    day.CongThuongQuyDoi > 0 ? (day.GiayDemTrongGioThuong > 14400 ? "CD" : "X") :
                                    isSunday ? "CN" :
                                    day.Segments.Any(x => x.LoaiThoiGian == "PHEP_KHONG_LUONG") ? "KL" : "N";
                                updBcct.Parameters.Add("p_symbol",symbol);
                                updBcct.Parameters.Add("p_leave",leaveDays);
                                updBcct.Parameters.Add("p_holiday",holiday?day.CongThuongQuyDoi:0);
                                updBcct.Parameters.Add("p_bcct", day.IdBangCongCt);
                                updBcct.Parameters.Add("p_manv", day.MaNV);

                                updBcct.ExecuteNonQuery();
                            }
                        }

                        // 3.8 Rollup tổng hợp vào TB_KYCONGCHITIET cho các nhân viên bị ảnh hưởng
                        RollupToKyCongChiTiet(conn, targetManvList, request.MaKyCong, nam, thang, daysInMonth);

                        // 3.9 Tăng revision công bố trên TB_KYCONG và bật cờ TRANGTHAI = 1 (chuẩn ACID: chỉ bật khi hoàn tất công bố)
                        using (var revCmd = CreateCmd(@"
                            UPDATE TB_KYCONG 
                            SET CONG_PUBLISH_REV = CONG_PUBLISH_REV + 1,
                                TRANGTHAI = 1,
                                NGAYTINHCONG = SYSDATE,
                                UPDATED_BY = :p_user,
                                UPDATED_DATE = SYSDATE
                            WHERE MAKYCONG = :p_mkc", conn))
                        {
                            revCmd.Parameters.Add("p_user", validUserId);
                            revCmd.Parameters.Add("p_mkc", request.MaKyCong);
                            revCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        result.Success = true;
                        result.TrangThai = "DA_CONG_BO";
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        result.Success = false;
                        result.TrangThai = "THAT_BAI";
                        result.ErrorMessage = "Lỗi trong transaction công bố: " + ex.Message;
                        return result;
                    }
                }
            }

            return result;
        }

        private void RollupToKyCongChiTiet(OracleConnection conn, List<long> manvList, int makycong, int nam, int thang, int daysInMonth)
        {
            foreach (var manv in manvList)
            {
                // Lấy danh sách ký hiệu và ngày công từ TB_BANGCONG_CHITIET
                var dayValues = new Dictionary<int, decimal>();
                var daySymbols = new Dictionary<int, string>();

                using (var cmd = CreateCmd(@"
                    SELECT EXTRACT(DAY FROM NGAY) AS NGAY_SO, KYHIEU, NVL(NGAYCONG, 0) AS CONG
                    FROM TB_BANGCONG_CHITIET
                    WHERE MAKYCONG = :p_mkc AND MANV = :p_manv", conn))
                {
                    cmd.Parameters.Add("p_mkc", makycong);
                    cmd.Parameters.Add("p_manv", manv);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int dayNum = Convert.ToInt32(reader["NGAY_SO"]);
                            string sym = reader["KYHIEU"]?.ToString();
                            decimal cong = Convert.ToDecimal(reader["CONG"]);
                            daySymbols[dayNum] = sym;
                            dayValues[dayNum] = cong;
                        }
                    }
                }

                decimal totalCong = dayValues.Values.Sum();

                // Tạo câu UPDATE động cho D1..D31
                var setClauses = new List<string>();
                for (int d = 1; d <= 31; d++)
                {
                    string col = $"D{d}";
                    if (d <= daysInMonth && daySymbols.TryGetValue(d, out var s) && !string.IsNullOrEmpty(s))
                    {
                        setClauses.Add($"{col} = '{s.Replace("'", "''")}'");
                    }
                    else if (d > daysInMonth)
                    {
                        setClauses.Add($"{col} = NULL");
                    }
                }

                setClauses.Add("TONGNGAYCONG = :p_total");
                setClauses.Add("NGAYPHEP = (SELECT NVL(SUM(NGAYPHEP),0) FROM TB_BANGCONG_CHITIET WHERE MAKYCONG=:p_mkc AND MANV=:p_manv)");
                setClauses.Add("CONGNGAYLE = (SELECT NVL(SUM(CONGNGAYLE),0) FROM TB_BANGCONG_CHITIET WHERE MAKYCONG=:p_mkc AND MANV=:p_manv)");

                string sql = $@"
                    UPDATE TB_KYCONGCHITIET 
                    SET {string.Join(", ", setClauses)}
                    WHERE MAKYCONG = :p_mkc AND MANV = :p_manv";

                using (var updateCmd = CreateCmd(sql, conn))
                {
                    updateCmd.Parameters.Add("p_total", totalCong);
                    updateCmd.Parameters.Add("p_mkc", makycong);
                    updateCmd.Parameters.Add("p_manv", manv);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        private void RecordCancelledRun(int makycong, string maYeuCau, DateTime tuNgay, DateTime denNgay, long inputRev, long expPubRev, long userId, string ghiChu)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = CreateCmd(@"
                        INSERT INTO TB_CHAMCONG_LANTINH (
                            MA_YEU_CAU, REQUEST_HASH, MAKYCONG, TU_NGAY, DEN_NGAY,
                            INPUT_REV, EXPECTED_PUBLISH_REV, PHIENBAN_ENGINE,
                            MUI_GIO, TRANG_THAI, BATDAU_TINH, KETTHUC_TINH,
                            NGUOI_THUC_HIEN, GHI_CHU
                        ) VALUES (
                            :p_mayeucau, :p_reqhash, :p_makycong, :p_tungay, :p_denngay,
                            :p_input_rev, :p_exp_pub_rev, '1.18',
                            'Asia/Ho_Chi_Minh', 'CAN_TINH_LAI', SYSTIMESTAMP, SYSTIMESTAMP,
                            :p_user, :p_ghichu
                        )", conn))
                    {
                        cmd.Parameters.Add("p_mayeucau", maYeuCau + "_CANCEL");
                        cmd.Parameters.Add("p_reqhash", ComputeSha256(maYeuCau));
                        cmd.Parameters.Add("p_makycong", makycong);
                        cmd.Parameters.Add("p_tungay", tuNgay);
                        cmd.Parameters.Add("p_denngay", denNgay);
                        cmd.Parameters.Add("p_input_rev", inputRev);
                        cmd.Parameters.Add("p_exp_pub_rev", expPubRev);
                        cmd.Parameters.Add("p_user", userId);
                        cmd.Parameters.Add("p_ghichu", ghiChu);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private AttendancePolicyDto LoadActivePolicy(OracleConnection conn, DateTime tuNgay, DateTime denNgay, long? policyId)
        {
            using (var cmd = CreateCmd(@"
                SELECT IDQUYDINH, MA_QUYDINH, SO_PHIENBAN, GIAY_DUNG_SAI_HUONG_CONG,
                       CACH_HUONG_DUNG_SAI, CACH_DEM_MUON, GIAY_MIEN_VIPHAM_MUON, GIAY_MIEN_VIPHAM_SOM,
                       KIEU_LAM_TRON, BUOC_LAM_TRON_GIAY, DEM_BATDAU_PHUT, DEM_KETTHUC_PHUT
                FROM TB_QUYDINH_CHAMCONG
                WHERE TU_NGAY <= :p_tungay AND (DEN_NGAY IS NULL OR DEN_NGAY > :p_tungay)
                  AND TRANG_THAI = 'PUBLISHED' AND IDQUYDINH = :p_policy
                ORDER BY SO_PHIENBAN DESC", conn))
            {
                cmd.Parameters.Add("p_policy", policyId);
                cmd.Parameters.Add("p_tungay", tuNgay);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AttendancePolicyDto
                        {
                            IdQuyDinh = Convert.ToInt64(reader["IDQUYDINH"]),
                            MaQuyDinh = reader["MA_QUYDINH"].ToString(),
                            SoPhienBan = Convert.ToInt32(reader["SO_PHIENBAN"]),
                            GiayDungSaiHuongCong = reader["GIAY_DUNG_SAI_HUONG_CONG"] != DBNull.Value ? Convert.ToInt32(reader["GIAY_DUNG_SAI_HUONG_CONG"]) : 300,
                            CachHuongDungSai = reader["CACH_HUONG_DUNG_SAI"] != DBNull.Value ? reader["CACH_HUONG_DUNG_SAI"].ToString() : "DU_CONG_NEU_TRONG_NGUONG",
                            CachDemMuon = reader["CACH_DEM_MUON"] != DBNull.Value ? reader["CACH_DEM_MUON"].ToString() : "TOAN_BO_NEU_VUOT",
                            GiayMienViPhamMuon = reader["GIAY_MIEN_VIPHAM_MUON"] != DBNull.Value ? Convert.ToInt32(reader["GIAY_MIEN_VIPHAM_MUON"]) : 0,
                            GiayMienViPhamSom = reader["GIAY_MIEN_VIPHAM_SOM"] != DBNull.Value ? Convert.ToInt32(reader["GIAY_MIEN_VIPHAM_SOM"]) : 0,
                            KieuLamTron = reader["KIEU_LAM_TRON"] != DBNull.Value ? reader["KIEU_LAM_TRON"].ToString() : "EXACT",
                            BuocLamTronGiay = reader["BUOC_LAM_TRON_GIAY"] != DBNull.Value ? Convert.ToInt32(reader["BUOC_LAM_TRON_GIAY"]) : 1,
                            DemBatDauPhut = reader["DEM_BATDAU_PHUT"] != DBNull.Value ? Convert.ToInt32(reader["DEM_BATDAU_PHUT"]) : 1320,
                            DemKetThucPhut = reader["DEM_KETTHUC_PHUT"] != DBNull.Value ? Convert.ToInt32(reader["DEM_KETTHUC_PHUT"]) : 360
                        };
                    }
                }
            }

            throw new InvalidOperationException("No published attendance policy valid for this schedule/day.");
        }

        private Dictionary<long, ShiftVersionDto> LoadShiftsWithFrames(OracleConnection conn)
        {
            var shifts = new Dictionary<long, ShiftVersionDto>();
            using (var cmd = CreateCmd(@"
                SELECT IDCAPHIENBAN, IDLOAICA, SO_PHIENBAN, TEN_PHIENBAN,
                       TONG_GIAY_CHUAN, CONG_QUY_DOI
                FROM TB_CA_PHIENBAN", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt64(reader["IDCAPHIENBAN"]);
                    shifts[id] = new ShiftVersionDto
                    {
                        IdCaPhienBan = id,
                        IdLoaiCa = Convert.ToInt64(reader["IDLOAICA"]),
                        TenPhienBan = reader["TEN_PHIENBAN"].ToString(),
                        TongGiayChuan = Convert.ToInt64(reader["TONG_GIAY_CHUAN"]),
                        CongQuyDoi = Convert.ToDecimal(reader["CONG_QUY_DOI"]),
                        KhungGios = new List<ShiftFrameDto>()
                    };
                }
            }

            using (var cmd = CreateCmd(@"
                SELECT IDCAPHIENBAN, STT, BATDAU_PHUT, KETTHUC_PHUT,
                       LOAI_KHUNGGIO, BAT_BUOC_QUET_THE
                FROM TB_CA_KHUNGGIO
                ORDER BY IDCAPHIENBAN, STT", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt64(reader["IDCAPHIENBAN"]);
                    if (shifts.TryGetValue(id, out var shift))
                    {
                        shift.KhungGios.Add(new ShiftFrameDto
                        {
                            Stt = Convert.ToInt32(reader["STT"]),
                            BatDauPhut = Convert.ToInt32(reader["BATDAU_PHUT"]),
                            KetThucPhut = Convert.ToInt32(reader["KETTHUC_PHUT"]),
                            LoaiKhungGio = reader["LOAI_KHUNGGIO"].ToString(),
                            BatBuocQuetThe = Convert.ToInt32(reader["BAT_BUOC_QUET_THE"])
                        });
                    }
                }
            }

            return shifts;
        }

        private Dictionary<(long Manv, DateTime Date), ScheduleDto> LoadSchedules(OracleConnection conn, List<long> manvList, DateTime tuNgay, DateTime denNgay)
        {
            var schedules = new Dictionary<(long Manv, DateTime Date), ScheduleDto>();
            using (var cmd = CreateCmd(@"
                SELECT IDLICH, MANV, NGAY, IDCAPHIENBAN, IDQUYDINH, BATDAU_KEHOACH,
                       KETTHUC_KEHOACH, TRANG_THAI_PHAN_CONG, LOAI_NGAY, TRANG_THAI
                FROM TB_LICH_LAMVIEC
                WHERE NGAY BETWEEN :p_tu AND :p_den
                  AND TRANG_THAI = 'PUBLISHED'", conn))
            {
                cmd.Parameters.Add("p_tu", tuNgay);
                cmd.Parameters.Add("p_den", denNgay);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long manv = Convert.ToInt64(reader["MANV"]);
                        if (!manvList.Contains(manv)) continue;

                        DateTime ngay = Convert.ToDateTime(reader["NGAY"]).Date;
                        if (schedules.ContainsKey((manv, ngay)))
                            throw new InvalidOperationException("Multiple published schedules require explicit consolidation; refusing to overwrite a schedule.");
                        schedules[(manv, ngay)] = new ScheduleDto
                        {
                            IdLich = Convert.ToInt64(reader["IDLICH"]),
                            IdQuyDinh = reader["IDQUYDINH"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["IDQUYDINH"]),
                            MaNV = manv,
                            Ngay = ngay,
                            IdCaPhienBan = reader["IDCAPHIENBAN"] != DBNull.Value ? Convert.ToInt64(reader["IDCAPHIENBAN"]) : (long?)null,
                            BatDauKeHoach = reader["BATDAU_KEHOACH"] != DBNull.Value ? Convert.ToDateTime(reader["BATDAU_KEHOACH"]) : (DateTime?)null,
                            KetThucKeHoach = reader["KETTHUC_KEHOACH"] != DBNull.Value ? Convert.ToDateTime(reader["KETTHUC_KEHOACH"]) : (DateTime?)null,
                            TrangThaiPhanCong = reader["TRANG_THAI_PHAN_CONG"].ToString(),
                            LoaiNgay = reader["LOAI_NGAY"].ToString(),
                            TrangThai = reader["TRANG_THAI"].ToString()
                        };
                    }
                }
            }
            return schedules;
        }

        private void EnsureSchedulesExist(OracleConnection conn, List<long> manvList, DateTime tuNgay, DateTime denNgay, long userId)
        {
            var existingSet = new HashSet<(long, DateTime)>();
            using (var chkCmd = CreateCmd(@"
                SELECT MANV, TRUNC(NGAY) 
                FROM TB_LICH_LAMVIEC 
                WHERE NGAY BETWEEN :p_tu AND :p_den AND TRANG_THAI = 'PUBLISHED'", conn))
            {
                chkCmd.Parameters.Add("p_tu", tuNgay);
                chkCmd.Parameters.Add("p_den", denNgay);
                using (var reader = chkCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingSet.Add((Convert.ToInt64(reader[0]), Convert.ToDateTime(reader[1]).Date));
                    }
                }
            }

            var missing = new List<(long Manv, DateTime Date)>();
            for (DateTime d = tuNgay; d <= denNgay; d = d.AddDays(1))
            {
                foreach (var manv in manvList)
                {
                    if (!existingSet.Contains((manv, d)))
                    {
                        missing.Add((manv, d));
                    }
                }
            }

            if (missing.Count == 0) return;

            foreach (var item in missing)
            {
                DateTime d = item.Date;
                long manv = item.Manv;
                bool isSun = (d.DayOfWeek == DayOfWeek.Sunday);
                bool isHol = (d.Month == 1 && d.Day == 1) || (d.Month == 4 && d.Day == 30) || (d.Month == 5 && d.Day == 1) || (d.Month == 9 && d.Day == 2);
                string maPhanCong = $"STD_{manv}_{d:yyyyMMdd}";

                using (var ins = CreateCmd(@"
                    INSERT INTO TB_LICH_LAMVIEC (
                        MANV, NGAY, MA_PHANCONG, SO_PHIENBAN, IDCAPHIENBAN, IDQUYDINH,
                        BATDAU_KEHOACH, KETTHUC_KEHOACH, TRANG_THAI_PHAN_CONG, LOAI_NGAY,
                        TRANG_THAI, NGUON_PHAN_CONG, TAO_BOI, LY_DO
                    ) VALUES (
                        :p_manv, :p_ngay, :p_mapc, 1, :p_ca, :p_qd,
                        :p_start, :p_end, :p_tt, :p_loai,
                        'PUBLISHED', 'HE_THONG', :p_user, 'Lịch phân công chuẩn tự động'
                    )", conn))
                {
                    ins.Parameters.Add("p_manv", manv);
                    ins.Parameters.Add("p_ngay", d);
                    ins.Parameters.Add("p_mapc", maPhanCong);

                    if (isSun)
                    {
                        ins.Parameters.Add("p_ca", DBNull.Value);
                        ins.Parameters.Add("p_qd", DBNull.Value);
                        ins.Parameters.Add("p_start", DBNull.Value);
                        ins.Parameters.Add("p_end", DBNull.Value);
                        ins.Parameters.Add("p_tt", "NGHI");
                        ins.Parameters.Add("p_loai", "NGHI_TUAN");
                    }
                    else
                    {
                        ins.Parameters.Add("p_ca", 1L);
                        ins.Parameters.Add("p_qd", 1L);
                        ins.Parameters.Add("p_start", d.AddHours(8));
                        ins.Parameters.Add("p_end", d.AddHours(17));
                        ins.Parameters.Add("p_tt", "LAM_VIEC");
                        ins.Parameters.Add("p_loai", isHol ? "LE" : "THUONG");
                    }
                    ins.Parameters.Add("p_user", userId);
                    try
                    {
                        ins.ExecuteNonQuery();
                    }
                    catch { }
                }
            }
        }

        private Dictionary<(long Manv, DateTime Date), List<RawPunchDto>> LoadRawPunches(OracleConnection conn, List<long> manvList, int nam, int thang)
        {
            var rawMap = new Dictionary<(long Manv, DateTime Date), List<RawPunchDto>>();
            using (var cmd = CreateCmd(@"
                SELECT MABC, MANV, NGAY, GIOVAO, PHUTVAO, GIORA, PHUTRA, THOIDIEM_VAO, THOIDIEM_RA, NGUON_CHAM
                FROM TB_BANGCONG
                WHERE NAM = :p_nam AND THANG = :p_thang", conn))
            {
                cmd.Parameters.Add("p_nam", nam);
                cmd.Parameters.Add("p_thang", thang);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long manv = Convert.ToInt64(reader["MANV"]);
                        if (!manvList.Contains(manv)) continue;

                        int dayNum = Convert.ToInt32(reader["NGAY"]);
                        DateTime date = new DateTime(nam, thang, dayNum);

                        DateTime? inTime = null;
                        if (reader["GIOVAO"] != DBNull.Value && reader["PHUTVAO"] != DBNull.Value)
                        {
                            int h = Convert.ToInt32(reader["GIOVAO"]);
                            int m = Convert.ToInt32(reader["PHUTVAO"]);
                            inTime = date.AddHours(h).AddMinutes(m);
                        }

                        DateTime? outTime = null;
                        if (reader["GIORA"] != DBNull.Value && reader["PHUTRA"] != DBNull.Value)
                        {
                            int h = Convert.ToInt32(reader["GIORA"]);
                            int m = Convert.ToInt32(reader["PHUTRA"]);
                            outTime = date.AddHours(h).AddMinutes(m);
                        }

                        if (reader["THOIDIEM_VAO"] != DBNull.Value) inTime = Convert.ToDateTime(reader["THOIDIEM_VAO"]);
                        if (reader["THOIDIEM_RA"] != DBNull.Value) outTime = Convert.ToDateTime(reader["THOIDIEM_RA"]);
                        var punch = new RawPunchDto
                        {
                            Mabc = Convert.ToInt64(reader["MABC"]),
                            MaNV = manv,
                            ThoiDiemVao = inTime,
                            ThoiDiemRa = outTime,
                            NguonCham = reader["NGUON_CHAM"] == DBNull.Value ? "LEGACY" : reader["NGUON_CHAM"].ToString()
                        };

                        var key = (manv, date);
                        if (!rawMap.ContainsKey(key)) rawMap[key] = new List<RawPunchDto>();
                        rawMap[key].Add(punch);
                    }
                }
            }
            return rawMap;
        }

        private Dictionary<(long Manv, DateTime Date), List<ApprovedOvertimeDto>> LoadApprovedOvertimes(OracleConnection conn, List<long> manvList, DateTime tuNgay, DateTime denNgay)
        {
            var map = new Dictionary<(long Manv, DateTime Date), List<ApprovedOvertimeDto>>();
            using (var cmd = CreateCmd(@"
                SELECT ID, MANV, BATDAU_DUYET, KETTHUC_DUYET, GIAY_OT_DUYET
                FROM TB_YEUCAU_TANGCA
                WHERE TRANGTHAI = 'APPROVED'
                  AND BATDAU_DUYET < :p_den AND KETTHUC_DUYET > :p_tu", conn))
            {
                cmd.Parameters.Add("p_den", denNgay);
                cmd.Parameters.Add("p_tu", tuNgay);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long manv = Convert.ToInt64(reader["MANV"]);
                        if (!manvList.Contains(manv)) continue;

                        DateTime start = Convert.ToDateTime(reader["BATDAU_DUYET"]);
                        DateTime end = Convert.ToDateTime(reader["KETTHUC_DUYET"]);
                        long seconds = reader["GIAY_OT_DUYET"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_OT_DUYET"]) : (long)Math.Round((end - start).TotalSeconds);

                        var ot = new ApprovedOvertimeDto
                        {
                            Id = Convert.ToInt64(reader["ID"]),
                            MaNV = manv,
                            BatDauDuyet = start,
                            KetThucDuyet = end,
                            GiayOtDuyet = seconds
                        };

                        var key = (manv, start.Date);
                        if (!map.ContainsKey(key)) map[key] = new List<ApprovedOvertimeDto>();
                        map[key].Add(ot);
                    }
                }
            }
            return map;
        }

        private Dictionary<(long Manv, DateTime Date), List<ApprovedLeaveDto>> LoadApprovedLeaves(OracleConnection conn, List<long> manvList, DateTime tuNgay, DateTime denNgay)
        {
            var map = new Dictionary<(long Manv, DateTime Date), List<ApprovedLeaveDto>>();
            using (var cmd = CreateCmd(@"
                SELECT ID, MANV, BATDAU_NGHI, KETTHUC_NGHI, GIAY_NGHI, NGUON_CHI_TRA, LOAI_HUONG_CONG
                FROM TB_YEUCAU_NGHIPHEP
                WHERE TRANGTHAI = 'APPROVED'
                  AND BATDAU_NGHI < :p_den AND KETTHUC_NGHI > :p_tu", conn))
            {
                cmd.Parameters.Add("p_den", denNgay);
                cmd.Parameters.Add("p_tu", tuNgay);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long manv = Convert.ToInt64(reader["MANV"]);
                        if (!manvList.Contains(manv)) continue;

                        DateTime start = Convert.ToDateTime(reader["BATDAU_NGHI"]);
                        DateTime end = Convert.ToDateTime(reader["KETTHUC_NGHI"]);
                        long seconds = reader["GIAY_NGHI"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_NGHI"]) : (long)Math.Round((end - start).TotalSeconds);

                        string loaiHuongCong = reader["LOAI_HUONG_CONG"]?.ToString() ?? "PHEP_NAM";
                        int coHuongLuong = (loaiHuongCong == "PHEP_NAM" || loaiHuongCong == "NGHI_HUONG_LUONG")
                            && reader["NGUON_CHI_TRA"].ToString() == "CONG_TY" ? 1 : 0;

                        var leave = new ApprovedLeaveDto
                        {
                            Id = Convert.ToInt64(reader["ID"]),
                            MaNV = manv,
                            BatDauNghi = start,
                            KetThucNghi = end,
                            GiayNghi = seconds,
                            CoHuongLuong = coHuongLuong,
                            LoaiHuongCong = loaiHuongCong,
                            NguonChiTra = reader["NGUON_CHI_TRA"]?.ToString() ?? "CONG_TY"
                        };

                        for (var day = start.Date; day < end; day = day.AddDays(1))
                        {
                            if (day < tuNgay || day >= denNgay) continue;
                            var key = (manv, day);
                            if (!map.ContainsKey(key)) map[key] = new List<ApprovedLeaveDto>();
                            map[key].Add(leave);
                        }
                    }
                }
            }
            return map;
        }

        private Dictionary<(long Manv, DateTime Date), ApprovedAdjustmentDto> LoadApprovedAdjustments(OracleConnection conn, List<long> manvList, DateTime tuNgay, DateTime denNgay)
        {
            var map = new Dictionary<(long Manv, DateTime Date), ApprovedAdjustmentDto>();
            using (var cmd = CreateCmd(@"
                SELECT ID, MANV, NGAY, THOIDIEM_VAO_MOI, THOIDIEM_RA_MOI
                FROM TB_YEUCAU_DIEUCHINHCONG
                WHERE TRANGTHAI = 'APPROVED'
                  AND NGAY BETWEEN :p_tu AND :p_den", conn))
            {
                cmd.Parameters.Add("p_tu", tuNgay);
                cmd.Parameters.Add("p_den", denNgay);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long manv = Convert.ToInt64(reader["MANV"]);
                        if (!manvList.Contains(manv)) continue;

                        DateTime ngay = Convert.ToDateTime(reader["NGAY"]).Date;
                        DateTime? inTime = reader["THOIDIEM_VAO_MOI"] != DBNull.Value ? Convert.ToDateTime(reader["THOIDIEM_VAO_MOI"]) : (DateTime?)null;
                        DateTime? outTime = reader["THOIDIEM_RA_MOI"] != DBNull.Value ? Convert.ToDateTime(reader["THOIDIEM_RA_MOI"]) : (DateTime?)null;

                        map[(manv, ngay)] = new ApprovedAdjustmentDto
                        {
                            Id = Convert.ToInt64(reader["ID"]),
                            MaNV = manv,
                            Ngay = ngay,
                            ThoiDiemVaoMoi = inTime,
                            ThoiDiemRaMoi = outTime
                        };
                    }
                }
            }
            return map;
        }

        private Dictionary<(long Manv, DateTime Date), long> EnsureAndLoadBangCongChiTiet(
            OracleConnection conn,
            List<long> manvList,
            int makycong,
            DateTime tuNgay,
            DateTime lastDayOfMonth,
            long nguoiThucHien)
        {
            var map = new Dictionary<(long Manv, DateTime Date), long>();

            // Nạp các dòng đã có
            using (var cmd = CreateCmd(@"
                SELECT IDBANGCONGCT, MANV, NGAY
                FROM TB_BANGCONG_CHITIET
                WHERE MAKYCONG = :p_mkc", conn))
            {
                cmd.Parameters.Add("p_mkc", makycong);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = Convert.ToInt64(reader["IDBANGCONGCT"]);
                        long manv = Convert.ToInt64(reader["MANV"]);
                        DateTime date = Convert.ToDateTime(reader["NGAY"]).Date;
                        map[(manv, date)] = id;
                    }
                }
            }

            // Kiểm tra các dòng còn thiếu trong TB_BANGCONG_CHITIET (Fail Closed: không tự sinh ngầm dữ liệu sai ý nghĩa)
            var missingList = new List<(long Manv, DateTime Date)>();
            for (DateTime d = tuNgay; d <= lastDayOfMonth; d = d.AddDays(1))
            {
                foreach (var manv in manvList)
                {
                    if (!map.ContainsKey((manv, d)))
                    {
                        missingList.Add((manv, d));
                    }
                }
            }

            if (missingList.Count > 0)
            {
                var sample = missingList.Take(5).Select(m => $"NV {m.Manv} ngày {m.Date:dd/MM/yyyy}");
                string detail = string.Join(", ", sample);
                if (missingList.Count > 5) detail += $" và {missingList.Count - 5} ngày khác";
                throw new InvalidOperationException($"Phát hiện {missingList.Count} ngày công thiếu anchor (TB_BANGCONG_CHITIET: {detail}). Cần thực hiện bước sinh/cập nhật bảng công trước khi công bố.");
            }

            return map;
        }

        private static string GetVietnameseDayOfWeek(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: return "Thứ hai";
                case DayOfWeek.Tuesday: return "Thứ ba";
                case DayOfWeek.Wednesday: return "Thứ tư";
                case DayOfWeek.Thursday: return "Thứ năm";
                case DayOfWeek.Friday: return "Thứ sáu";
                case DayOfWeek.Saturday: return "Thứ bảy";
                case DayOfWeek.Sunday: return "Chủ nhật";
                default: return "";
            }
        }

        private AttendancePublishResult FindCompletedRun(OracleConnection conn, string requestKey, string hash)
        {
            using (var cmd = CreateCmd(@"SELECT IDLANTINH, REQUEST_HASH, TRANG_THAI, INPUT_DATA_HASH
                FROM TB_CHAMCONG_LANTINH WHERE MA_YEU_CAU=:p_key", conn))
            {
                cmd.Parameters.Add("p_key", requestKey);
                long runId; string storedHash; string state; string inputHash;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    runId = Convert.ToInt64(reader["IDLANTINH"]);
                    storedHash = reader["REQUEST_HASH"].ToString();
                    state = reader["TRANG_THAI"].ToString();
                    inputHash = reader["INPUT_DATA_HASH"].ToString();
                }
                if (storedHash != hash) throw new InvalidOperationException("Request key was already used with another payload.");
                if (state != "DA_CONG_BO") throw new InvalidOperationException("Request exists but is not published; use an explicit new request after review.");
                using (var counts = CreateCmd(@"SELECT COUNT(*) N, COUNT(DISTINCT MANV) NV,
                    NVL(SUM(CASE WHEN DU_DIEUKIEN_CHOT=1 THEN 1 ELSE 0 END),0) READY
                    FROM TB_CHAMCONG_KQ_NGAY WHERE IDLANTINH=:p_run",conn))
                {
                    counts.Parameters.Add("p_run",runId);
                    using (var reader=counts.ExecuteReader())
                    {
                        reader.Read();
                        int total=Convert.ToInt32(reader["N"]), ready=Convert.ToInt32(reader["READY"]);
                        return new AttendancePublishResult {Success=true,IdLanTinh=runId,MaYeuCau=requestKey,
                            TrangThai=state,InputDataHash=inputHash,SoNgayCong=total,
                            SoNhanVien=Convert.ToInt32(reader["NV"]),SoNgaySanSang=ready,SoNgayChoXacMinh=total-ready};
                    }
                }
            }
        }

        private static string ComputeSha256(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
