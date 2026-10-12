using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bu.CLASS_NHANSU
{
    public class ProfileEditQuotaDto
    {
        public string GroupCode { get; set; }
        public int DirectEditCount { get; set; }
        public bool IsFirstEditAvailable => DirectEditCount == 0;
        public string FirstEditDate { get; set; }
        public string LastEditDate { get; set; }
    }

    public class ProfileProposalResultDto
    {
        public bool IsDirectEdit { get; set; }
        public long? RequestId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class ProfileChangeRequestDto
    {
        public long Id { get; set; }
        public long Manv { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string GroupCode { get; set; }
        public string ReqType { get; set; }
        public string CurrentDataJson { get; set; }
        public string ProposedDataJson { get; set; }
        public bool HasProposedAvatarBlob { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string RejectReason { get; set; }
        public string ResolvedByName { get; set; }
        public string ResolvedAt { get; set; }
        public string AppliedAt { get; set; }
        public string CreatedAt { get; set; }
    }

    public class PersonalInfoPayload
    {
        public string Hoten { get; set; }
        public string Dienthoai { get; set; }
        public string Diachi { get; set; }
        public string Email { get; set; }
        public string Cccd { get; set; }
        public string Ngaysinh { get; set; }
        public string Gioitinh { get; set; }
    }

    public class InsuranceProposalPayload
    {
        public short? VungLuong { get; set; }
        public bool? ThamGiaBhxh { get; set; }
        public bool? ThamGiaBhyt { get; set; }
        public bool? ThamGiaBhtn { get; set; }
        public bool? ThamGiaTnldBnn { get; set; }
        public decimal? LuongDongBhxhRieng { get; set; }
        public string NgayBatDau { get; set; }
        public string GhiChu { get; set; }
    }

    public class DependentProposalItem
    {
        public long? Id { get; set; } // Null nếu thêm mới
        public string HoTen { get; set; }
        public string MoiQuanHe { get; set; }
        public string Cccd { get; set; }
        public string MaSoThueNpt { get; set; }
        public string NgaySinh { get; set; }
        public string LoaiGiayTo { get; set; } // CCCD, GIAY_KHAI_SINH, HO_CHIEU...
        public string SoGiayTo { get; set; }
        public int ThangBatDau { get; set; }
        public int? ThangKetThuc { get; set; }
        public string Action { get; set; } // ADD, UPDATE, TERMINATE
    }

    public class EmployeeProfileRequestService
    {
        private readonly string _connectionString;

        public EmployeeProfileRequestService(string connectionString = null)
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

        public ProfileEditQuotaDto GetQuota(long manv, string groupCode)
        {
            using (var conn = GetConnection())
            {
                string sql = "SELECT DIRECT_EDIT_COUNT, FIRST_EDIT_DATE, LAST_EDIT_DATE FROM HR.TB_EMPLOYEE_EDIT_QUOTA WHERE MANV = :p_manv AND GROUP_CODE = :p_grp";
                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_manv", manv);
                    cmd.Parameters.Add("p_grp", groupCode);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new ProfileEditQuotaDto
                            {
                                GroupCode = groupCode,
                                DirectEditCount = Convert.ToInt32(r["DIRECT_EDIT_COUNT"]),
                                FirstEditDate = r["FIRST_EDIT_DATE"] != DBNull.Value ? Convert.ToDateTime(r["FIRST_EDIT_DATE"]).ToString("dd/MM/yyyy HH:mm") : null,
                                LastEditDate = r["LAST_EDIT_DATE"] != DBNull.Value ? Convert.ToDateTime(r["LAST_EDIT_DATE"]).ToString("dd/MM/yyyy HH:mm") : null,
                            };
                        }
                    }
                }
            }
            return new ProfileEditQuotaDto
            {
                GroupCode = groupCode,
                DirectEditCount = 0
            };
        }

        public ProfileProposalResultDto SubmitPersonalInfo(long manv, PersonalInfoPayload payload, string reason, long userId)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (!string.IsNullOrWhiteSpace(payload.Ngaysinh)) ParseProfileDate(payload.Ngaysinh);
            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    bool direct = LockOrInitQuota(conn, tx, manv, "PERSONAL_INFO") == 0;
                    string snapshot = GetCurrentPersonalInfoJson(conn, tx, manv);
                    string proposed = JsonConvert.SerializeObject(payload);
                    long id = CreateProfileRequest(conn, tx, manv, "PERSONAL_INFO", "UPDATE_PERSONAL", snapshot, proposed, null, ComputeHash(snapshot), reason, userId);
                    if (direct)
                    {
                        DirectApplyPersonalInfo(conn, tx, manv, payload, userId);
                        ConsumeFirstQuota(conn, tx, manv, "PERSONAL_INFO");
                        using (var cmd = new OracleCommand("UPDATE HR.TB_YEUCAU_THAYDOI_HOSO SET STATUS='APPLIED', APPLIED_DATA=:data, APPLIED_AT=SYSDATE, RESOLVED_AT=SYSDATE WHERE ID=:id", conn))
                        {
                            cmd.Transaction = tx; cmd.BindByName = true;
                            cmd.Parameters.Add("data", proposed); cmd.Parameters.Add("id", id); cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return new ProfileProposalResultDto { IsDirectEdit=direct, RequestId=id, Status=direct ? "APPLIED":"PENDING",
                        Message=direct ? "Đã lưu thông tin cá nhân (lần thay đổi đầu tiên).":"Đã gửi yêu cầu. Thông tin hiện tại chỉ thay đổi sau khi được duyệt." };
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public ProfileProposalResultDto SubmitAvatar(long manv, byte[] imageBytes, string reason, long userId)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Dữ liệu ảnh không hợp lệ.");
            if (imageBytes.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Dung lượng ảnh vượt quá giới hạn cho phép (tối đa 5MB).");

            // Validate magic bytes
            bool isJpeg = imageBytes.Length > 3 && imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF;
            bool isPng = imageBytes.Length > 8 && imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47;
            if (!isJpeg && !isPng)
                throw new ArgumentException("Định dạng ảnh không được hỗ trợ. Vui lòng chọn ảnh định dạng JPEG hoặc PNG.");

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int directCount = LockOrInitQuota(conn, tx, manv, "AVATAR");

                    if (directCount == 0)
                    {
                        // LẦN ĐẦU: Lưu trực tiếp vào TB_NHANVIEN.HINHANH
                        using (var cmd = new OracleCommand(
                            "UPDATE HR.TB_NHANVIEN SET HINHANH = :p_img, UPDATED_BY = :p_uid, UPDATED_DATE = SYSDATE WHERE MANV = :p_manv", conn))
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_img", OracleDbType.Blob).Value = imageBytes;
                            cmd.Parameters.Add("p_uid", userId);
                            cmd.Parameters.Add("p_manv", manv);
                            cmd.ExecuteNonQuery();
                        }

                        ConsumeFirstQuota(conn, tx, manv, "AVATAR");
                        tx.Commit();
                        return new ProfileProposalResultDto
                        {
                            IsDirectEdit = true,
                            Status = "APPLIED",
                            Message = "Ảnh đại diện đã được cập nhật thành công (Lượt cập nhật trực tiếp lần đầu)."
                        };
                    }
                    else
                    {
                        // LẦN 2 TRỞ ĐI: Lưu vào PROPOSED_BLOB của TB_YEUCAU_THAYDOI_HOSO
                        long reqId;
                        using (var cmdSeq = new OracleCommand("SELECT HR.SEQ_YEUCAU_THAYDOI_HOSO.NEXTVAL FROM DUAL", conn))
                        {
                            cmdSeq.Transaction = tx;
                            reqId = Convert.ToInt64(cmdSeq.ExecuteScalar());
                        }

                        string sql = @"
                            INSERT INTO HR.TB_YEUCAU_THAYDOI_HOSO 
                            (ID, MANV, GROUP_CODE, REQ_TYPE, PROPOSED_BLOB, STATUS, REASON, CREATED_AT, CREATED_BY)
                            VALUES (:p_id, :p_manv, 'AVATAR', 'UPDATE_AVATAR', :p_blob, 'PENDING', :p_reason, SYSDATE, :p_uid)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_id", reqId);
                            cmd.Parameters.Add("p_manv", manv);
                            cmd.Parameters.Add("p_blob", OracleDbType.Blob).Value = imageBytes;
                            cmd.Parameters.Add("p_reason", (object)reason?.Trim() ?? "Đổi ảnh đại diện");
                            cmd.Parameters.Add("p_uid", userId);
                            cmd.ExecuteNonQuery();
                        }

                        CLASS_SYSTEM.NotificationService.CreateNotification(
                            conn, tx,
                            "Đề xuất thay đổi ảnh đại diện",
                            $"Nhân viên #{manv} đã gửi đơn #{reqId} đề xuất đổi ảnh đại diện chờ duyệt.",
                            "YEUCAU_HOSO",
                            "HR_SYSTEM", targetType: "PERSONAL", targetManv: manv, eventType: "PROFILE_REQUEST_CREATED", entityId: reqId
                        );
                        tx.Commit();

                        return new ProfileProposalResultDto
                        {
                            IsDirectEdit = false,
                            RequestId = reqId,
                            Status = "PENDING",
                            Message = "Ảnh đại diện mới đã được gửi lên hệ thống và đang chờ phê duyệt trước khi áp dụng."
                        };
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public ProfileProposalResultDto SubmitInsuranceProposal(long manv, InsuranceProposalPayload payload, string reason, long userId)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            ValidateInsuranceProposal(payload);

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int directCount = LockOrInitQuota(conn, tx, manv, "INSURANCE_INFO");

                    if (directCount == 0)
                    {
                        DirectApplyInsurance(conn, tx, manv, payload, userId);
                        ConsumeFirstQuota(conn, tx, manv, "INSURANCE_INFO");
                        tx.Commit();
                        return new ProfileProposalResultDto
                        {
                            IsDirectEdit = true,
                            Status = "APPLIED",
                            Message = "Thông tin bảo hiểm đã được khai báo trực tiếp thành công (Lần đầu)."
                        };
                    }
                    else
                    {
                        string currentSnapshot = GetCurrentInsuranceJson(conn, tx, manv);
                        string proposedJson = JsonConvert.SerializeObject(payload);
                        string baseVersion = ComputeHash(currentSnapshot);

                        long reqId = CreateProfileRequest(conn, tx, manv, "INSURANCE_INFO", "UPDATE_INSURANCE", currentSnapshot, proposedJson, null, baseVersion, reason, userId);
                        tx.Commit();

                        return new ProfileProposalResultDto
                        {
                            IsDirectEdit = false,
                            RequestId = reqId,
                            Status = "PENDING",
                            Message = "Yêu cầu điều chỉnh thông tin bảo hiểm đã được gửi đến phòng Nhân sự để phê duyệt."
                        };
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public ProfileProposalResultDto SubmitDependentsProposal(long manv, List<DependentProposalItem> dependents, string reason, long userId)
        {
            if (dependents == null || dependents.Count == 0)
                throw new ArgumentException("Danh sách người phụ thuộc không được rỗng.");
            foreach (var dependent in dependents) ValidateDependentProposal(dependent);

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int directCount = LockOrInitQuota(conn, tx, manv, "DEPENDENTS");
                    foreach (var item in dependents.Where(d => d.Id.HasValue))
                    {
                        using (var owner = new OracleCommand("SELECT ID FROM HR.TB_NGUOI_PHU_THUOC WHERE ID=:id AND MANV=:manv FOR UPDATE",conn))
                        {
                            owner.Transaction=tx; owner.BindByName=true;
                            owner.Parameters.Add("id",item.Id.Value); owner.Parameters.Add("manv",manv);
                            if (owner.ExecuteScalar()==null) throw new KeyNotFoundException("Không tìm thấy người phụ thuộc trong hồ sơ của bạn.");
                        }
                    }


                    if (directCount == 0)
                    {
                        DirectApplyDependents(conn, tx, manv, dependents, userId);
                        ConsumeFirstQuota(conn, tx, manv, "DEPENDENTS");
                        tx.Commit();
                        return new ProfileProposalResultDto
                        {
                            IsDirectEdit = true,
                            Status = "APPLIED",
                            Message = "Hồ sơ người phụ thuộc đã được lưu trực tiếp thành công (Lần đầu)."
                        };
                    }
                    else
                    {
                        string currentSnapshot = GetCurrentDependentsJson(conn, tx, manv);
                        string proposedJson = JsonConvert.SerializeObject(dependents);
                        string baseVersion = ComputeHash(currentSnapshot);

                        long reqId = CreateProfileRequest(conn, tx, manv, "DEPENDENTS", "UPDATE_DEPENDENTS", currentSnapshot, proposedJson, null, baseVersion, reason, userId);
                        tx.Commit();

                        return new ProfileProposalResultDto
                        {
                            IsDirectEdit = false,
                            RequestId = reqId,
                            Status = "PENDING",
                            Message = "Đề xuất thay đổi người phụ thuộc đã được gửi đến phòng Nhân sự để phê duyệt."
                        };
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void ApproveRequest(long id, long approverUserId, decimal? approverEmployeeId)
        {
            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    long manv;
                    string groupCode;
                    string reqType;
                    string currentData;
                    string proposedData;
                    byte[] proposedBlob = null;
                    string baseVersion;
                    string status;

                    using (var cmdLock = new OracleCommand(
                        "SELECT MANV, GROUP_CODE, REQ_TYPE, CURRENT_DATA, PROPOSED_DATA, PROPOSED_BLOB, BASE_VERSION, STATUS FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE ID = :p_id FOR UPDATE", conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", id);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy đơn yêu cầu thay đổi hồ sơ.");
                            manv = Convert.ToInt64(r["MANV"]);
                            groupCode = r["GROUP_CODE"]?.ToString();
                            reqType = r["REQ_TYPE"]?.ToString();
                            currentData = r["CURRENT_DATA"]?.ToString();
                            proposedData = r["PROPOSED_DATA"]?.ToString();
                            if (r["PROPOSED_BLOB"] != DBNull.Value)
                            {
                                proposedBlob = (byte[])r["PROPOSED_BLOB"];
                            }
                            baseVersion = r["BASE_VERSION"]?.ToString();
                            status = r["STATUS"]?.ToString();
                        }
                    }

                    if (approverEmployeeId.HasValue && approverEmployeeId.Value == manv)
                    {
                        throw new InvalidOperationException("Bạn không được phép tự phê duyệt đơn thay đổi hồ sơ của chính mình.");
                    }

                    if (status != "PENDING")
                    {
                        throw new InvalidOperationException($"Đơn yêu cầu không ở trạng thái chờ duyệt (Trạng thái hiện tại: {status}).");
                    }

                    LockEmployee(conn, tx, manv);

                    // Kiểm tra Optimistic Concurrency nếu có baseVersion
                    if (!string.IsNullOrEmpty(baseVersion))
                    {
                        string currentDataNow = "";
                        if (groupCode == "PERSONAL_INFO") currentDataNow = GetCurrentPersonalInfoJson(conn, tx, manv);
                        else if (groupCode == "INSURANCE_INFO") currentDataNow = GetCurrentInsuranceJson(conn, tx, manv);
                        else if (groupCode == "DEPENDENTS") currentDataNow = GetCurrentDependentsJson(conn, tx, manv);

                        string currentHashNow = ComputeHash(currentDataNow);
                        if (!string.Equals(baseVersion, currentHashNow, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Dữ liệu hồ sơ gốc đã bị thay đổi bởi quản trị viên sau khi nhân viên gửi đơn. Vui lòng đối chiếu lại trước khi xử lý.");
                        }
                    }

                    // ÁP DỤNG DỮ LIỆU ĐÍCH
                    if (groupCode == "PERSONAL_INFO" && !string.IsNullOrEmpty(proposedData))
                    {
                        var p = JsonConvert.DeserializeObject<PersonalInfoPayload>(proposedData);
                        DirectApplyPersonalInfo(conn, tx, manv, p, approverUserId);
                    }
                    else if (groupCode == "AVATAR" && proposedBlob != null)
                    {
                        using (var cmdImg = new OracleCommand(
                            "UPDATE HR.TB_NHANVIEN SET HINHANH = :p_img, UPDATED_BY = :p_uid, UPDATED_DATE = SYSDATE WHERE MANV = :p_manv", conn))
                        {
                            cmdImg.Transaction = tx;
                            cmdImg.BindByName = true;
                            cmdImg.Parameters.Add("p_img", OracleDbType.Blob).Value = proposedBlob;
                            cmdImg.Parameters.Add("p_uid", approverUserId);
                            cmdImg.Parameters.Add("p_manv", manv);
                            cmdImg.ExecuteNonQuery();
                        }
                    }
                    else if (groupCode == "INSURANCE_INFO" && !string.IsNullOrEmpty(proposedData))
                    {
                        var ins = JsonConvert.DeserializeObject<InsuranceProposalPayload>(proposedData);
                        DirectApplyInsurance(conn, tx, manv, ins, approverUserId);
                    }
                    else if (groupCode == "DEPENDENTS" && !string.IsNullOrEmpty(proposedData))
                    {
                        var deps = JsonConvert.DeserializeObject<List<DependentProposalItem>>(proposedData);
                        DirectApplyDependents(conn, tx, manv, deps, approverUserId);
                    }

                    // Cập nhật trạng thái đơn thành APPROVED và APPLIED
                    using (var cmdUpd = new OracleCommand(@"
                        UPDATE HR.TB_YEUCAU_THAYDOI_HOSO 
                        SET STATUS = 'APPROVED', RESOLVED_BY = :p_uid, RESOLVED_AT = SYSDATE, APPLIED_AT = SYSDATE, APPLIED_DATA = :p_app
                        WHERE ID = :p_id", conn))
                    {
                        cmdUpd.Transaction = tx;
                        cmdUpd.BindByName = true;
                        cmdUpd.Parameters.Add("p_uid", approverUserId);
                        cmdUpd.Parameters.Add("p_app", (object)proposedData ?? "BLOB_AVATAR");
                        cmdUpd.Parameters.Add("p_id", id);
                        cmdUpd.ExecuteNonQuery();
                    }

                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Yêu cầu thay đổi hồ sơ đã được phê duyệt",
                        $"Đơn đề xuất #{id} nhóm [{groupCode}] của bạn đã được phòng Nhân sự phê duyệt và áp dụng vào hồ sơ.",
                        "YEUCAU_HOSO",
                        "HR_SYSTEM",
                        targetType: "PERSONAL",
                        targetManv: manv,
                        eventType: "PROFILE_REQUEST_APPROVED",
                        entityId: id,
                        discretePushBody: "HRMS: Yêu cầu thay đổi hồ sơ của bạn đã được phê duyệt."
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

        public void RejectRequest(long id, long approverUserId, string rejectReason, decimal? approverEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(rejectReason))
                throw new ArgumentException("Vui lòng cung cấp lý do từ chối.");

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    long manv;
                    string currentStatus;

                    using (var cmdLock = new OracleCommand(
                        "SELECT MANV, STATUS FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE ID = :p_id FOR UPDATE", conn))
                    {
                        cmdLock.Transaction = tx;
                        cmdLock.BindByName = true;
                        cmdLock.Parameters.Add("p_id", id);
                        using (var r = cmdLock.ExecuteReader())
                        {
                            if (!r.Read()) throw new KeyNotFoundException("Không tìm thấy đơn yêu cầu thay đổi hồ sơ.");
                            manv = Convert.ToInt64(r["MANV"]);
                            currentStatus = r["STATUS"]?.ToString();
                        }
                    }

                    if (approverEmployeeId.HasValue && approverEmployeeId.Value == manv)
                    {
                        throw new InvalidOperationException("Bạn không được phép tự từ chối đơn của chính mình.");
                    }

                    if (currentStatus != "PENDING")
                    {
                        throw new InvalidOperationException($"Không thể từ chối đơn ở trạng thái: {currentStatus}.");
                    }

                    using (var cmdUpd = new OracleCommand(@"
                        UPDATE HR.TB_YEUCAU_THAYDOI_HOSO 
                        SET STATUS = 'REJECTED', RESOLVED_BY = :p_uid, RESOLVED_AT = SYSDATE, REJECT_REASON = :p_reason 
                        WHERE ID = :p_id", conn))
                    {
                        cmdUpd.Transaction = tx;
                        cmdUpd.BindByName = true;
                        cmdUpd.Parameters.Add("p_uid", approverUserId);
                        cmdUpd.Parameters.Add("p_reason", rejectReason.Trim());
                        cmdUpd.Parameters.Add("p_id", id);
                        cmdUpd.ExecuteNonQuery();
                    }

                    CLASS_SYSTEM.NotificationService.CreateNotification(
                        conn,
                        tx,
                        "Yêu cầu thay đổi hồ sơ bị từ chối",
                        $"Đơn đề xuất #{id} của bạn đã bị từ chối. Lý do: {rejectReason.Trim()}.",
                        "YEUCAU_HOSO",
                        "HR_SYSTEM",
                        targetType: "PERSONAL",
                        targetManv: manv,
                        eventType: "PROFILE_REQUEST_REJECTED",
                        entityId: id,
                        discretePushBody: "HRMS: Yêu cầu thay đổi hồ sơ của bạn đã bị từ chối."
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

        public List<ProfileChangeRequestDto> GetRequests(string groupCode, string status, string search, decimal? selfManv, int page, int pageSize, out int totalCount)
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
                if (!string.IsNullOrWhiteSpace(groupCode) && groupCode.ToUpper() != "ALL")
                {
                    where.Add("y.GROUP_CODE = :p_grp");
                    cmd.Parameters.Add("p_grp", groupCode.ToUpper());
                }
                if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
                {
                    where.Add("y.STATUS = :p_st");
                    cmd.Parameters.Add("p_st", status.ToUpper());
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where.Add("(LOWER(nv.HOTEN) LIKE :p_search OR LOWER(nv.EMPLOYEE_CODE) LIKE :p_search)");
                    cmd.Parameters.Add("p_search", $"%{search.Trim().ToLower()}%");
                }

                string whereClause = string.Join(" AND ", where);

                cmd.CommandText = $@"
                    SELECT COUNT(*)
                    FROM HR.TB_YEUCAU_THAYDOI_HOSO y
                    LEFT JOIN HR.TB_NHANVIEN nv ON y.MANV = nv.MANV
                    WHERE {whereClause}";
                totalCount = Convert.ToInt32(cmd.ExecuteScalar());

                int startRow = (page - 1) * pageSize;
                cmd.CommandText = $@"
                    SELECT * FROM (
                        SELECT 
                            y.ID, y.MANV, nv.EMPLOYEE_CODE, nv.HOTEN AS EMPLOYEE_NAME, pb.TENPB AS DEPARTMENT_NAME,
                            y.GROUP_CODE, y.REQ_TYPE, y.CURRENT_DATA, y.PROPOSED_DATA, 
                            CASE WHEN y.PROPOSED_BLOB IS NOT NULL THEN 1 ELSE 0 END AS HAS_BLOB,
                            y.STATUS, y.REASON, y.REJECT_REASON,
                            NVL(u_res.FULLNAME, u_res.USERNAME) AS RESOLVED_BY_NAME,
                            y.RESOLVED_AT, y.APPLIED_AT, y.CREATED_AT,
                            ROW_NUMBER() OVER (ORDER BY y.CREATED_AT DESC) AS RN
                        FROM HR.TB_YEUCAU_THAYDOI_HOSO y
                        LEFT JOIN HR.TB_NHANVIEN nv ON y.MANV = nv.MANV
                        LEFT JOIN HR.TB_PHONGBAN pb ON nv.IDPB = pb.IDPB
                        LEFT JOIN HR.TB_SYS_USER u_res ON y.RESOLVED_BY = u_res.IDUSER
                        WHERE {whereClause}
                    ) WHERE RN > :p_startRow AND RN <= :p_endRow";

                cmd.Parameters.Add("p_startRow", startRow);
                cmd.Parameters.Add("p_endRow", startRow + pageSize);

                var list = new List<ProfileChangeRequestDto>();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ProfileChangeRequestDto
                        {
                            Id = Convert.ToInt64(r["ID"]),
                            Manv = Convert.ToInt64(r["MANV"]),
                            EmployeeCode = r["EMPLOYEE_CODE"]?.ToString(),
                            EmployeeName = r["EMPLOYEE_NAME"]?.ToString(),
                            DepartmentName = r["DEPARTMENT_NAME"]?.ToString() ?? "Chưa phân bổ",
                            GroupCode = r["GROUP_CODE"]?.ToString(),
                            ReqType = r["REQ_TYPE"]?.ToString(),
                            CurrentDataJson = r["CURRENT_DATA"]?.ToString(),
                            ProposedDataJson = r["PROPOSED_DATA"]?.ToString(),
                            HasProposedAvatarBlob = Convert.ToInt32(r["HAS_BLOB"]) == 1,
                            Status = r["STATUS"]?.ToString(),
                            Reason = r["REASON"]?.ToString(),
                            RejectReason = r["REJECT_REASON"]?.ToString(),
                            ResolvedByName = r["RESOLVED_BY_NAME"]?.ToString(),
                            ResolvedAt = r["RESOLVED_AT"] != DBNull.Value ? Convert.ToDateTime(r["RESOLVED_AT"]).ToString("yyyy-MM-dd HH:mm") : null,
                            AppliedAt = r["APPLIED_AT"] != DBNull.Value ? Convert.ToDateTime(r["APPLIED_AT"]).ToString("yyyy-MM-dd HH:mm") : null,
                            CreatedAt = r["CREATED_AT"] != DBNull.Value ? Convert.ToDateTime(r["CREATED_AT"]).ToString("yyyy-MM-dd HH:mm") : ""
                        });
                    }
                }
                return list;
            }
        }

        public byte[] GetProposedAvatar(long reqId)
        {
            using (var conn = GetConnection())
            {
                using (var cmd = new OracleCommand("SELECT PROPOSED_BLOB FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE ID = :p_id", conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_id", reqId);
                    var val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                    {
                        return (byte[])val;
                    }
                }
            }
            return null;
        }

        #region Helper Methods (Quota, Snapshot, Hash, Direct Apply)

        private static DateTime ParseProfileDate(string value)
        {
            DateTime result;
            if (string.IsNullOrWhiteSpace(value) || !DateTime.TryParseExact(value.Trim(), new[] { "yyyy-MM-dd", "dd/MM/yyyy" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result))
                throw new ArgumentException("Ngày không hợp lệ. Nhập YYYY-MM-DD hoặc DD/MM/YYYY.");
            if (result.Year<1900 || result.Year>2100) throw new ArgumentException("Năm không hợp lệ.");
            return result.Date;
        }

        private static DateTime MonthEnd(int month)
        {
            int year=month/100, number=month%100;
            if (year<1900 || year>2100 || number<1 || number>12) throw new ArgumentException("Tháng phải có dạng YYYYMM hợp lệ.");
            return new DateTime(year,number,DateTime.DaysInMonth(year,number));
        }

        private static int PreviousMonth(int month)
        {
            var previous=MonthEnd(month).AddMonths(-1);
            return previous.Year*100+previous.Month;
        }

        private static void ValidateInsuranceProposal(InsuranceProposalPayload p)
        {
            if (p==null) throw new ArgumentNullException(nameof(p));
            ParseProfileDate(p.NgayBatDau);
            if (p.VungLuong.HasValue && (p.VungLuong.Value<1 || p.VungLuong.Value>4)) throw new ArgumentException("Vùng lương phải từ 1 đến 4.");
            if (p.LuongDongBhxhRieng.HasValue && p.LuongDongBhxhRieng.Value<0) throw new ArgumentException("Mức lương đóng bảo hiểm không được âm.");
        }

        private static void ValidateDependentProposal(DependentProposalItem p)
        {
            if (p==null) throw new ArgumentNullException(nameof(p));
            p.Action=(p.Action ?? (p.Id.HasValue ? "UPDATE":"ADD")).Trim().ToUpperInvariant();
            if (p.Action!="ADD" && p.Action!="UPDATE" && p.Action!="TERMINATE") throw new ArgumentException("Thao tác người phụ thuộc không hợp lệ.");
            if ((p.Action=="ADD" && p.Id.HasValue) || (p.Action!="ADD" && (!p.Id.HasValue || p.Id.Value<=0))) throw new ArgumentException("Mã hồ sơ không phù hợp với thao tác.");
            MonthEnd(p.ThangBatDau);
            if (p.ThangKetThuc.HasValue) { MonthEnd(p.ThangKetThuc.Value); if (p.ThangKetThuc.Value<p.ThangBatDau) throw new ArgumentException("Tháng kết thúc không được trước tháng bắt đầu."); }
            if (p.Action=="TERMINATE" && !p.ThangKetThuc.HasValue) throw new ArgumentException("Nhập tháng kết thúc giảm trừ.");
            if (p.Action!="TERMINATE" && (string.IsNullOrWhiteSpace(p.HoTen) || string.IsNullOrWhiteSpace(p.MoiQuanHe))) throw new ArgumentException("Nhập họ tên và mối quan hệ người phụ thuộc.");
            if (!string.IsNullOrWhiteSpace(p.NgaySinh)) ParseProfileDate(p.NgaySinh);
        }

        private static void LockEmployee(OracleConnection conn, OracleTransaction tx, long manv)
        {
            using (var cmd=new OracleCommand("SELECT MANV FROM HR.TB_NHANVIEN WHERE MANV=:manv FOR UPDATE",conn))
            {
                cmd.Transaction=tx;cmd.BindByName=true;cmd.Parameters.Add("manv",manv);
                if (cmd.ExecuteScalar()==null) throw new KeyNotFoundException("Không tìm thấy nhân viên.");
            }
        }

        private int LockOrInitQuota(OracleConnection conn, OracleTransaction tx, long manv, string groupCode)
        {
            LockEmployee(conn, tx, manv);
            using (var pending = new OracleCommand("SELECT COUNT(*) FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE MANV=:manv AND GROUP_CODE=:grp AND STATUS='PENDING'", conn))
            {
                pending.Transaction=tx; pending.BindByName=true;
                pending.Parameters.Add("manv", manv); pending.Parameters.Add("grp", groupCode);
                if (Convert.ToInt32(pending.ExecuteScalar()) > 0)
                    throw new InvalidOperationException("Nhóm hồ sơ này đang có đơn chờ duyệt. Vui lòng chờ xử lý đơn hiện tại trước khi gửi thay đổi mới.");
            }

            using (var cmd = new OracleCommand(
                "SELECT DIRECT_EDIT_COUNT FROM HR.TB_EMPLOYEE_EDIT_QUOTA WHERE MANV = :p_manv AND GROUP_CODE = :p_grp FOR UPDATE", conn))
            {
                cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_manv", manv);
                cmd.Parameters.Add("p_grp", groupCode);
                var val = cmd.ExecuteScalar();
                if (val != null && val != DBNull.Value)
                {
                    return Convert.ToInt32(val);
                }
            }

            // Chưa có bản ghi -> Thêm mới với count = 0
            using (var cmdIns = new OracleCommand(@"
                INSERT INTO HR.TB_EMPLOYEE_EDIT_QUOTA (ID, MANV, GROUP_CODE, DIRECT_EDIT_COUNT)
                VALUES (HR.SEQ_EMPLOYEE_EDIT_QUOTA.NEXTVAL, :p_manv, :p_grp, 0)", conn))
            {
                cmdIns.Transaction = tx;
                cmdIns.BindByName = true;
                cmdIns.Parameters.Add("p_manv", manv);
                cmdIns.Parameters.Add("p_grp", groupCode);
                cmdIns.ExecuteNonQuery();
            }

            // Khóa lại dòng vừa tạo
            using (var cmdLock = new OracleCommand(
                "SELECT DIRECT_EDIT_COUNT FROM HR.TB_EMPLOYEE_EDIT_QUOTA WHERE MANV = :p_manv AND GROUP_CODE = :p_grp FOR UPDATE", conn))
            {
                cmdLock.Transaction = tx;
                cmdLock.BindByName = true;
                cmdLock.Parameters.Add("p_manv", manv);
                cmdLock.Parameters.Add("p_grp", groupCode);
                return Convert.ToInt32(cmdLock.ExecuteScalar());
            }
        }

        private void ConsumeFirstQuota(OracleConnection conn, OracleTransaction tx, long manv, string groupCode)
        {
            using (var cmd = new OracleCommand(@"
                UPDATE HR.TB_EMPLOYEE_EDIT_QUOTA 
                SET DIRECT_EDIT_COUNT = 1, FIRST_EDIT_DATE = SYSDATE, LAST_EDIT_DATE = SYSDATE 
                WHERE MANV = :p_manv AND GROUP_CODE = :p_grp", conn))
            {
                cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_manv", manv);
                cmd.Parameters.Add("p_grp", groupCode);
                cmd.ExecuteNonQuery();
            }
        }

        private void DirectApplyPersonalInfo(OracleConnection conn, OracleTransaction tx, long manv, PersonalInfoPayload p, long userId)
        {
            var sets = new List<string> { "UPDATED_BY = :p_uid", "UPDATED_DATE = SYSDATE" };
            var cmd = new OracleCommand { Connection = conn, Transaction = tx, BindByName = true };
            cmd.Parameters.Add("p_uid", userId);
            cmd.Parameters.Add("p_manv", manv);

            if (!string.IsNullOrWhiteSpace(p.Hoten))
            {
                sets.Add("HOTEN = :p_ht");
                cmd.Parameters.Add("p_ht", p.Hoten.Trim());
            }
            if (!string.IsNullOrWhiteSpace(p.Dienthoai))
            {
                sets.Add("DIENTHOAI = :p_dt");
                cmd.Parameters.Add("p_dt", p.Dienthoai.Trim());
            }
            if (!string.IsNullOrWhiteSpace(p.Diachi))
            {
                sets.Add("DIACHI = :p_dc");
                cmd.Parameters.Add("p_dc", p.Diachi.Trim());
            }
            if (!string.IsNullOrWhiteSpace(p.Email))
            {
                sets.Add("EMAIL = :p_em");
                cmd.Parameters.Add("p_em", p.Email.Trim());
            }
            if (!string.IsNullOrWhiteSpace(p.Cccd))
            {
                sets.Add("CCCD = :p_cccd");
                cmd.Parameters.Add("p_cccd", p.Cccd.Trim());
            }
            if (!string.IsNullOrWhiteSpace(p.Ngaysinh))
            {
                if (DateTime.TryParse(p.Ngaysinh.Trim(), out var ns))
                {
                    sets.Add("NGAYSINH = :p_ns");
                    cmd.Parameters.Add("p_ns", ns.Date);
                }
            }
            if (!string.IsNullOrWhiteSpace(p.Gioitinh))
            {
                int idgt = p.Gioitinh.Trim().ToLower().Contains("nam") ? 1 : 0;
                sets.Add("IDGT = :p_gt");
                cmd.Parameters.Add("p_gt", idgt);
            }

            cmd.CommandText = $"UPDATE HR.TB_NHANVIEN SET {string.Join(", ", sets)} WHERE MANV = :p_manv";
            cmd.ExecuteNonQuery();
        }

        private void DirectApplyInsurance(OracleConnection conn, OracleTransaction tx, long manv, InsuranceProposalPayload ins, long userId)
        {
            ValidateInsuranceProposal(ins);
            DateTime effective = ParseProfileDate(ins.NgayBatDau);
            var rows = new DataTable();
            using (var cmd = new OracleCommand("SELECT * FROM HR.TB_NHANVIEN_BAOHIEM_THAM_GIA WHERE MANV=:manv AND TRANG_THAI='ACTIVE' ORDER BY NGAY_BAT_DAU DESC FOR UPDATE", conn))
            {
                cmd.Transaction=tx; cmd.BindByName=true; cmd.Parameters.Add("manv",manv);
                using (var reader=cmd.ExecuteReader()) rows.Load(reader);
            }
            var previous = rows.Rows.Cast<DataRow>().FirstOrDefault();
            long? previousId = previous == null ? (long?)null : Convert.ToInt64(previous["ID"]);
            DateTime? previousEnd = previous == null || previous["NGAY_KET_THUC"]==DBNull.Value ? (DateTime?)null : Convert.ToDateTime(previous["NGAY_KET_THUC"]).Date;
            int lastUsed=0;
            if (previousId.HasValue)
            {
                using (var cmd = new OracleCommand("SELECT NVL(MAX(MAKYCONG),0) FROM HR.TB_BANGLUONG WHERE PROFILE_BH_ID=:id AND MANV=:manv",conn))
                {
                    cmd.Transaction=tx; cmd.BindByName=true; cmd.Parameters.Add("id",previousId.Value); cmd.Parameters.Add("manv",manv);
                    lastUsed=Convert.ToInt32(cmd.ExecuteScalar());
                }
                if (lastUsed>0 && effective<=MonthEnd(lastUsed))
                    throw new InvalidOperationException("Hồ sơ bảo hiểm đã dùng để tính lương. Chọn ngày hiệu lực sau kỳ đã dùng; hệ thống sẽ giữ phiên bản cũ.");
                if (effective<Convert.ToDateTime(previous["NGAY_BAT_DAU"]).Date)
                    throw new ArgumentException("Ngày hiệu lực không được trước phiên bản bảo hiểm hiện tại.");
            }
            if (previous==null && (!ins.VungLuong.HasValue || !ins.ThamGiaBhxh.HasValue || !ins.ThamGiaBhyt.HasValue || !ins.ThamGiaBhtn.HasValue || !ins.ThamGiaTnldBnn.HasValue))
                throw new ArgumentException("Hồ sơ mới phải khai báo vùng lương và Có/Không cho từng loại bảo hiểm.");
            bool update = previous!=null && lastUsed==0 && effective==Convert.ToDateTime(previous["NGAY_BAT_DAU"]).Date;
            object salary=(object)ins.LuongDongBhxhRieng ?? (previous==null ? DBNull.Value:previous["LUONG_DONG_BHXH_RIENG"]);
            Func<string,bool?,int> flag=(column,value)=>value.HasValue ? (value.Value ? 1:0):Convert.ToInt32(previous[column]);
            if (previous!=null && !update && (!previousEnd.HasValue || previousEnd.Value>=effective))
            {
                using (var cmd = new OracleCommand("UPDATE HR.TB_NHANVIEN_BAOHIEM_THAM_GIA SET NGAY_KET_THUC=:endDate WHERE ID=:id AND MANV=:manv",conn))
                {
                    cmd.Transaction=tx; cmd.BindByName=true;
                    cmd.Parameters.Add("endDate",effective.AddDays(-1)); cmd.Parameters.Add("id",previousId.Value); cmd.Parameters.Add("manv",manv);
                    cmd.ExecuteNonQuery();
                }
            }
            string sql= update
                ? "UPDATE HR.TB_NHANVIEN_BAOHIEM_THAM_GIA SET VUNG_LUONG=:region, THAM_GIA_BHXH=:bhxh, THAM_GIA_BHYT=:bhyt, THAM_GIA_BHTN=:bhtn, THAM_GIA_TNLD_BNN=:tnld, LUONG_DONG_BHXH_RIENG=:salary WHERE ID=:id AND MANV=:manv"
                : @"INSERT INTO HR.TB_NHANVIEN_BAOHIEM_THAM_GIA
                   (ID,MANV,VUNG_LUONG,THAM_GIA_BHXH,THAM_GIA_BHYT,THAM_GIA_BHTN,THAM_GIA_TNLD_BNN,HUONG_TY_LE_TNLD_UU_DAI,LUONG_DONG_BHXH_RIENG,NGAY_BAT_DAU,TRANG_THAI,DATA_ORIGIN,SOURCE_NOTE)
                   VALUES(HR.SEQ_NV_BAOHIEM_TG.NEXTVAL,:manv,:region,:bhxh,:bhyt,:bhtn,:tnld,:preferential,:salary,:effective,'ACTIVE','SELF_PORTAL',:note)";
            using (var cmd=new OracleCommand(sql,conn))
            {
                cmd.Transaction=tx; cmd.BindByName=true; cmd.Parameters.Add("manv",manv);
                cmd.Parameters.Add("region",ins.VungLuong.HasValue ? (object)ins.VungLuong.Value:previous["VUNG_LUONG"]);
                cmd.Parameters.Add("bhxh",flag("THAM_GIA_BHXH",ins.ThamGiaBhxh)); cmd.Parameters.Add("bhyt",flag("THAM_GIA_BHYT",ins.ThamGiaBhyt));
                cmd.Parameters.Add("bhtn",flag("THAM_GIA_BHTN",ins.ThamGiaBhtn)); cmd.Parameters.Add("tnld",flag("THAM_GIA_TNLD_BNN",ins.ThamGiaTnldBnn));
                cmd.Parameters.Add("salary",salary);
                if (update) cmd.Parameters.Add("id",previousId.Value);
                else { cmd.Parameters.Add("effective",effective);cmd.Parameters.Add("preferential",previous==null ? 0:Convert.ToInt32(previous["HUONG_TY_LE_TNLD_UU_DAI"]));cmd.Parameters.Add("note",(object)ins.GhiChu ?? "Khai báo từ Mobile"); }
                if (cmd.ExecuteNonQuery()!=1) throw new KeyNotFoundException("Không tìm thấy hồ sơ bảo hiểm cần cập nhật.");
            }
        }

        private void DirectApplyDependents(OracleConnection conn, OracleTransaction tx, long manv, List<DependentProposalItem> dependents, long userId)
        {
            foreach (var d in dependents)
            {
                ValidateDependentProposal(d);
                DateTime? ns = string.IsNullOrWhiteSpace(d.NgaySinh) ? (DateTime?)null : ParseProfileDate(d.NgaySinh);
                int lastUsed;
                using (var used = new OracleCommand("SELECT NVL(MAX(MAKYCONG),0) FROM HR.TB_BANGLUONG WHERE MANV=:manv",conn))
                {
                    used.Transaction=tx; used.BindByName=true; used.Parameters.Add("manv",manv);
                    lastUsed=Convert.ToInt32(used.ExecuteScalar());
                }
                if (d.Id.HasValue)
                {
                    int startMonth;
                    using (var owner=new OracleCommand("SELECT THANG_BAT_DAU_GIAM_TRU FROM HR.TB_NGUOI_PHU_THUOC WHERE ID=:id AND MANV=:manv FOR UPDATE",conn))
                    {
                        owner.Transaction=tx; owner.BindByName=true; owner.Parameters.Add("id",d.Id.Value);owner.Parameters.Add("manv",manv);
                        var value=owner.ExecuteScalar();
                        if (value==null || value==DBNull.Value) throw new KeyNotFoundException("Không tìm thấy người phụ thuộc trong hồ sơ của bạn.");
                        startMonth=Convert.ToInt32(value);
                    }
                    if (d.Action=="TERMINATE" && d.ThangKetThuc.Value<Math.Max(startMonth,lastUsed))
                        throw new InvalidOperationException("Tháng kết thúc không được trước tháng bắt đầu hoặc kỳ đã dùng để tính lương.");
                    if (d.Action=="UPDATE" && startMonth<=lastUsed)
                    {
                        if (d.ThangBatDau<=lastUsed)
                            throw new InvalidOperationException("Hồ sơ người phụ thuộc đã dùng để tính lương. Nhập tháng bắt đầu mới sau kỳ đã dùng để tạo phiên bản mới.");
                        int previousMonth=PreviousMonth(d.ThangBatDau);
                        using (var close=new OracleCommand("UPDATE HR.TB_NGUOI_PHU_THUOC SET THANG_KET_THUC_GIAM_TRU=LEAST(NVL(THANG_KET_THUC_GIAM_TRU,:endMonth),:endMonth) WHERE ID=:id AND MANV=:manv",conn))
                        {
                            close.Transaction=tx; close.BindByName=true;close.Parameters.Add("endMonth",previousMonth);close.Parameters.Add("id",d.Id.Value);close.Parameters.Add("manv",manv);close.ExecuteNonQuery();
                        }
                        // The historical row stays active over its original effective months.
                        d.Id=null; d.Action="ADD";
                    }
                }

                if (d.Id.HasValue && d.Id.Value > 0)
                {
                    if (d.Action == "TERMINATE")
                    {
                        using (var cmd = new OracleCommand(
                            "UPDATE HR.TB_NGUOI_PHU_THUOC SET THANG_KET_THUC_GIAM_TRU = :p_tk WHERE ID = :p_id AND MANV = :p_manv", conn))
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_tk", d.ThangKetThuc ?? Convert.ToInt32(DateTime.Now.ToString("yyyyMM")));
                            cmd.Parameters.Add("p_id", d.Id.Value);
                            cmd.Parameters.Add("p_manv", manv);
                            if (cmd.ExecuteNonQuery()!=1) throw new KeyNotFoundException("Không tìm thấy hồ sơ người phụ thuộc cần cập nhật.");
                        }
                    }
                    else
                    {
                        using (var cmd = new OracleCommand(@"
                            UPDATE HR.TB_NGUOI_PHU_THUOC 
                            SET HO_TEN = :p_ten, MOI_QUAN_HE = :p_qh, CCCD = :p_cccd, MA_SO_THUE_NPT = :p_mst,
                                NGAY_SINH = :p_ns, LOAI_GIAY_TO = :p_loai, SO_GIAY_TO = :p_so,
                                THANG_BAT_DAU_GIAM_TRU = :p_tb, THANG_KET_THUC_GIAM_TRU = :p_tk,
                                TRANG_THAI_DANG_KY='CHO_XAC_MINH', TINH_TRANG_HO_SO_CHUNG_MINH='CHUA_NOP', NGAY_XAC_MINH=NULL, NGUOI_XAC_MINH=NULL
                            WHERE ID = :p_id AND MANV = :p_manv", conn))
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_ten", d.HoTen);
                            cmd.Parameters.Add("p_qh", d.MoiQuanHe);
                            cmd.Parameters.Add("p_cccd", (object)d.Cccd ?? DBNull.Value);
                            cmd.Parameters.Add("p_mst", (object)d.MaSoThueNpt ?? DBNull.Value);
                            cmd.Parameters.Add("p_ns", ns.HasValue ? (object)ns.Value.Date : DBNull.Value);
                            cmd.Parameters.Add("p_loai", (object)d.LoaiGiayTo ?? "CCCD");
                            cmd.Parameters.Add("p_so", (object)d.SoGiayTo ?? (object)d.Cccd ?? DBNull.Value);
                            cmd.Parameters.Add("p_tb", d.ThangBatDau);
                            cmd.Parameters.Add("p_tk", (object)d.ThangKetThuc ?? DBNull.Value);
                            cmd.Parameters.Add("p_id", d.Id.Value);
                            cmd.Parameters.Add("p_manv", manv);
                            if (cmd.ExecuteNonQuery()!=1) throw new KeyNotFoundException("Không tìm thấy hồ sơ người phụ thuộc cần cập nhật.");
                        }
                    }
                }
                else
                {
                    // Thêm mới NPT
                    long newId;
                    using (var cmdMax = new OracleCommand("SELECT HR.SEQ_NGUOI_PHU_THUOC.NEXTVAL FROM DUAL", conn))
                    {
                        cmdMax.Transaction = tx;
                        newId = Convert.ToInt64(cmdMax.ExecuteScalar());
                    }

                    string insSql = @"
                        INSERT INTO HR.TB_NGUOI_PHU_THUOC 
                        (ID, MANV, HO_TEN, MOI_QUAN_HE, CCCD, MA_SO_THUE_NPT, NGAY_SINH, 
                         THANG_BAT_DAU_GIAM_TRU, THANG_KET_THUC_GIAM_TRU, TRANG_THAI, CREATED_AT, DATA_ORIGIN,
                         LOAI_GIAY_TO, SO_GIAY_TO, TRANG_THAI_DANG_KY, TINH_TRANG_HO_SO_CHUNG_MINH)
                        VALUES (:p_id, :p_manv, :p_ten, :p_qh, :p_cccd, :p_mst, :p_ns,
                                :p_tb, :p_tk, 'ACTIVE', SYSTIMESTAMP, 'SELF_PORTAL',
                                :p_loai, :p_so, 'CHO_XAC_MINH', 'CHUA_NOP')";

                    using (var cmd = new OracleCommand(insSql, conn))
                    {
                        cmd.Transaction = tx;
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_id", newId);
                        cmd.Parameters.Add("p_manv", manv);
                        cmd.Parameters.Add("p_ten", d.HoTen);
                        cmd.Parameters.Add("p_qh", d.MoiQuanHe);
                        cmd.Parameters.Add("p_cccd", (object)d.Cccd ?? DBNull.Value);
                        cmd.Parameters.Add("p_mst", (object)d.MaSoThueNpt ?? DBNull.Value);
                        cmd.Parameters.Add("p_ns", ns.HasValue ? (object)ns.Value.Date : DBNull.Value);
                        cmd.Parameters.Add("p_tb", d.ThangBatDau);
                        cmd.Parameters.Add("p_tk", (object)d.ThangKetThuc ?? DBNull.Value);
                        cmd.Parameters.Add("p_loai", (object)d.LoaiGiayTo ?? "CCCD");
                        cmd.Parameters.Add("p_so", (object)d.SoGiayTo ?? (object)d.Cccd ?? DBNull.Value);
                        if (cmd.ExecuteNonQuery()!=1) throw new KeyNotFoundException("Không tìm thấy hồ sơ người phụ thuộc cần cập nhật.");
                    }
                }
            }
        }

        private long CreateProfileRequest(
            OracleConnection conn, OracleTransaction tx, long manv, string groupCode, string reqType,
            string currentData, string proposedData, byte[] proposedBlob, string baseVersion, string reason, long userId)
        {
            long newId;
            using (var cmdSeq = new OracleCommand("SELECT HR.SEQ_YEUCAU_THAYDOI_HOSO.NEXTVAL FROM DUAL", conn))
            {
                cmdSeq.Transaction = tx;
                newId = Convert.ToInt64(cmdSeq.ExecuteScalar());
            }

            string sql = @"
                INSERT INTO HR.TB_YEUCAU_THAYDOI_HOSO 
                (ID, MANV, GROUP_CODE, REQ_TYPE, CURRENT_DATA, PROPOSED_DATA, PROPOSED_BLOB, BASE_VERSION, STATUS, REASON, CREATED_AT, CREATED_BY)
                VALUES (:p_id, :p_manv, :p_grp, :p_type, :p_curr, :p_prop, :p_blob, :p_ver, 'PENDING', :p_reason, SYSDATE, :p_uid)";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_id", newId);
                cmd.Parameters.Add("p_manv", manv);
                cmd.Parameters.Add("p_grp", groupCode);
                cmd.Parameters.Add("p_type", reqType);
                cmd.Parameters.Add("p_curr", (object)currentData ?? DBNull.Value);
                cmd.Parameters.Add("p_prop", (object)proposedData ?? DBNull.Value);
                if (proposedBlob != null)
                {
                    cmd.Parameters.Add("p_blob", OracleDbType.Blob).Value = proposedBlob;
                }
                else
                {
                    cmd.Parameters.Add("p_blob", DBNull.Value);
                }
                cmd.Parameters.Add("p_ver", (object)baseVersion ?? DBNull.Value);
                cmd.Parameters.Add("p_reason", (object)reason?.Trim() ?? DBNull.Value);
                cmd.Parameters.Add("p_uid", userId);
                cmd.ExecuteNonQuery();
            }

            return newId;
        }

        private string GetCurrentPersonalInfoJson(OracleConnection conn, OracleTransaction tx, long manv)
        {
            using (var cmd = new OracleCommand("SELECT HOTEN, DIENTHOAI, DIACHI, CCCD, NGAYSINH, IDGT, EMAIL FROM HR.TB_NHANVIEN WHERE MANV = :p_manv", conn))
            {
                cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_manv", manv);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        var obj = new
                        {
                            Hoten = r["HOTEN"]?.ToString(),
                            Dienthoai = r["DIENTHOAI"]?.ToString(),
                            Diachi = r["DIACHI"]?.ToString(),
                            Email = r["EMAIL"]?.ToString(),
                            Cccd = r["CCCD"]?.ToString(),
                            Ngaysinh = r["NGAYSINH"] != DBNull.Value ? Convert.ToDateTime(r["NGAYSINH"]).ToString("yyyy-MM-dd") : null,
                            Gioitinh = r["IDGT"] != DBNull.Value && Convert.ToInt32(r["IDGT"]) == 1 ? "Nam" : "Nữ"
                        };
                        return JsonConvert.SerializeObject(obj);
                    }
                }
            }
            return "{}";
        }

        private string GetCurrentInsuranceJson(OracleConnection conn, OracleTransaction tx, long manv)
        {
            using (var cmd = new OracleCommand("SELECT VUNG_LUONG, THAM_GIA_BHXH, THAM_GIA_BHYT, THAM_GIA_BHTN, THAM_GIA_TNLD_BNN, LUONG_DONG_BHXH_RIENG, NGAY_BAT_DAU FROM HR.TB_NHANVIEN_BAOHIEM_THAM_GIA WHERE MANV = :p_manv ORDER BY NGAY_BAT_DAU DESC, ID DESC", conn))
            {
                cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_manv", manv);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        var obj = new
                        {
                            VungLuong = r["VUNG_LUONG"] != DBNull.Value ? (short?)Convert.ToInt16(r["VUNG_LUONG"]) : null,
                            ThamGiaBhxh = r["THAM_GIA_BHXH"] != DBNull.Value && Convert.ToInt32(r["THAM_GIA_BHXH"]) == 1,
                            ThamGiaBhyt = r["THAM_GIA_BHYT"] != DBNull.Value && Convert.ToInt32(r["THAM_GIA_BHYT"]) == 1,
                            ThamGiaBhtn = r["THAM_GIA_BHTN"] != DBNull.Value && Convert.ToInt32(r["THAM_GIA_BHTN"]) == 1,
                            ThamGiaTnldBnn = r["THAM_GIA_TNLD_BNN"] != DBNull.Value && Convert.ToInt32(r["THAM_GIA_TNLD_BNN"]) == 1,
                            LuongDongBhxhRieng = r["LUONG_DONG_BHXH_RIENG"] != DBNull.Value ? (decimal?)Convert.ToDecimal(r["LUONG_DONG_BHXH_RIENG"]) : null,
                            NgayBatDau = r["NGAY_BAT_DAU"] != DBNull.Value ? Convert.ToDateTime(r["NGAY_BAT_DAU"]).ToString("yyyy-MM-dd") : null
                        };
                        return JsonConvert.SerializeObject(obj);
                    }
                }
            }
            return "{}";
        }

        private string GetCurrentDependentsJson(OracleConnection conn, OracleTransaction tx, long manv)
        {
            var list = new List<object>();
            using (var cmd = new OracleCommand("SELECT ID, HO_TEN, MOI_QUAN_HE, CCCD, MA_SO_THUE_NPT, NGAY_SINH, LOAI_GIAY_TO, SO_GIAY_TO, THANG_BAT_DAU_GIAM_TRU, THANG_KET_THUC_GIAM_TRU, TRANG_THAI FROM HR.TB_NGUOI_PHU_THUOC WHERE MANV = :p_manv ORDER BY ID", conn))
            {
                cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_manv", manv);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new
                        {
                            Id = Convert.ToInt64(r["ID"]),
                            HoTen = r["HO_TEN"]?.ToString(),
                            MoiQuanHe = r["MOI_QUAN_HE"]?.ToString(),
                            Cccd = r["CCCD"]?.ToString(),
                            MaSoThueNpt = r["MA_SO_THUE_NPT"]?.ToString(),
                            NgaySinh = r["NGAY_SINH"] != DBNull.Value ? Convert.ToDateTime(r["NGAY_SINH"]).ToString("yyyy-MM-dd") : null,
                            LoaiGiayTo = r["LOAI_GIAY_TO"]?.ToString(),
                            SoGiayTo = r["SO_GIAY_TO"]?.ToString(),
                            ThangBatDau = Convert.ToInt32(r["THANG_BAT_DAU_GIAM_TRU"]),
                            ThangKetThuc = r["THANG_KET_THUC_GIAM_TRU"] != DBNull.Value ? (int?)Convert.ToInt32(r["THANG_KET_THUC_GIAM_TRU"]) : null,
                            TrangThai = r["TRANG_THAI"]?.ToString()
                        });
                    }
                }
            }
            return JsonConvert.SerializeObject(list);
        }

        private string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        #endregion
    }
}
