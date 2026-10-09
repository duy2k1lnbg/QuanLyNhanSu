using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Bu.Services.AI_Services.Memory;

namespace Bu.Services.AI_Services.Security
{
    /// <summary>
    /// Triển khai thực tế trên Oracle CSDL:
    /// Đảm bảo một Transaction nhất quán cho Grant changes + Revision increment + Audit logging
    /// Không bao giờ fallback sang JSON khi CSDL gặp sự cố
    /// </summary>
    public class OracleAiScopeGrantRepository : IAiScopeGrantRepository
    {
        public bool IsSchemaReady(out string notReadyReason)
        {
            notReadyReason = null;
            try
            {
                using (var db = new MyEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM AI_OWNER.TB_AI_SCOPE_GRANT WHERE ROWNUM = 1";
                        cmd.ExecuteScalar();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM AI_OWNER.TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL'";
                        cmd.ExecuteScalar();
                    }
                }
                return true;
            }
            catch (OracleException oex)
            {
                if (oex.Number == 942) // Table or view does not exist
                {
                    notReadyReason = "Hệ thống chính sách AI chưa sẵn sàng: Bảng AI_OWNER.TB_AI_SCOPE_GRANT hoặc TB_AI_REVISION chưa được khởi tạo. Vui lòng liên hệ Quản trị viên hệ thống để áp dụng migration CSDL.";
                }
                else if (oex.Number == 1031) // Insufficient privileges
                {
                    notReadyReason = "Không đủ quyền truy cập bảng phân quyền AI (ORA-01031). Vui lòng kiểm tra quyền GRANT trên schema Oracle.";
                }
                else
                {
                    notReadyReason = $"Không thể truy cập dữ liệu chính sách AI trên Oracle (Mã lỗi: ORA-{oex.Number}). Chi tiết: {oex.Message}";
                }
                return false;
            }
            catch (Exception ex)
            {
                notReadyReason = "Không thể kết nối đến cơ sở dữ liệu Oracle để xác thực chính sách AI: " + ex.Message;
                return false;
            }
        }

        public bool SubjectExists(string subjectType, int subjectId, out string code, out string name, out int? manv, out bool isDisabled)
        {
            code = "";
            name = "";
            manv = null;
            isDisabled = false;

            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var userObj = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == subjectId);
                    if (userObj == null) return false;

                    code = userObj.USERNAME ?? "";
                    name = userObj.FULLNAME ?? "";
                    isDisabled = (userObj.DISABLED ?? 0) == 1;
                    manv = userObj.MANV.HasValue ? (int?)Convert.ToInt32(userObj.MANV.Value) : null;

                    if (string.Equals(subjectType, "GROUP", StringComparison.OrdinalIgnoreCase))
                    {
                        // Đối với GROUP, trong mô hình TB_SYS_USER phải có ISGROUP == 1
                        return (userObj.ISGROUP ?? 0) == 1;
                    }
                    else
                    {
                        // Đối với USER, ISGROUP phải là 0 hoặc null
                        return (userObj.ISGROUP ?? 0) == 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public List<int> GetUserGroupIds(int userId)
        {
            var list = new List<int>();
            try
            {
                using (var db = new MyEntities())
                {
                    list = db.TB_SYS_GROUP.Where(g => g.MEMBER == userId).Select(g => (int)g.ID_GROUP).ToList();
                }
            }
            catch { }
            return list;
        }

        public string GetGroupName(int groupId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var g = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == groupId && u.ISGROUP == 1);
                    if (g != null && !string.IsNullOrEmpty(g.FULLNAME)) return g.FULLNAME;
                    if (g != null && !string.IsNullOrEmpty(g.USERNAME)) return g.USERNAME;
                }
            }
            catch { }
            return "Nhóm #" + groupId;
        }

        public HashSet<string> GetUserFunctionRights(int userId, IEnumerable<int> groupIds)
        {
            var rights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var db = new MyEntities())
                {
                    // Tiêu chí: CAN_VIEW = 1 OR USER_RIGHT = 1 đồng bộ hoàn toàn với PKG_AI_AUTH
                    var directRights = db.Database.SqlQuery<string>(
                        "SELECT DISTINCT FUNCTION_CODE FROM HR.TB_SYS_RIGHT WHERE IDUSER = :p_uid AND (CAN_VIEW = 1 OR USER_RIGHT = 1)",
                        new OracleParameter("p_uid", userId)
                    ).ToList();
                    foreach (var r in directRights) rights.Add(r);

                    if (groupIds != null)
                    {
                        foreach (var gid in groupIds)
                        {
                            var gRights = db.Database.SqlQuery<string>(
                                "SELECT DISTINCT FUNCTION_CODE FROM HR.TB_SYS_RIGHT WHERE IDUSER = :p_gid AND (CAN_VIEW = 1 OR USER_RIGHT = 1)",
                                new OracleParameter("p_gid", gid)
                            ).ToList();
                            foreach (var r in gRights) rights.Add(r);
                        }
                    }
                }
            }
            catch { }
            return rights;
        }

        public HashSet<string> GetGroupFunctionRights(int groupId)
        {
            var rights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var db = new MyEntities())
                {
                    var gRights = db.Database.SqlQuery<string>(
                        "SELECT DISTINCT FUNCTION_CODE FROM HR.TB_SYS_RIGHT WHERE IDUSER = :p_gid AND (CAN_VIEW = 1 OR USER_RIGHT = 1)",
                        new OracleParameter("p_gid", groupId)
                    ).ToList();
                    foreach (var r in gRights) rights.Add(r);
                }
            }
            catch { }
            return rights;
        }

        public List<AiScopeGrantRecord> GetGrants(string subjectType, int subjectId)
        {
            var results = new List<AiScopeGrantRecord>();
            using (var db = new MyEntities())
            {
                var conn = (OracleConnection)db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.BindByName = true;
                    cmd.CommandText = @"
                        SELECT GRANT_ID, SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, SCOPE_KEY, EFFECT, IS_ENABLED, VALID_FROM, VALID_TO, CREATED_AT
                        FROM AI_OWNER.TB_AI_SCOPE_GRANT
                        WHERE SUBJECT_TYPE = :p_stype AND SUBJECT_ID = :p_sid AND IS_ENABLED = 1";
                    cmd.Parameters.Add(new OracleParameter("p_stype", subjectType));
                    cmd.Parameters.Add(new OracleParameter("p_sid", subjectId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new AiScopeGrantRecord
                            {
                                GrantId = Convert.ToInt64(reader["GRANT_ID"]),
                                SubjectType = reader["SUBJECT_TYPE"].ToString(),
                                SubjectId = Convert.ToInt32(reader["SUBJECT_ID"]),
                                CapabilityCode = reader["CAPABILITY_CODE"].ToString(),
                                ScopeType = reader["SCOPE_TYPE"].ToString(),
                                ScopeKey = reader["SCOPE_KEY"] != DBNull.Value ? reader["SCOPE_KEY"].ToString() : null,
                                Effect = reader["EFFECT"].ToString(),
                                IsEnabled = Convert.ToInt32(reader["IS_ENABLED"]) == 1,
                                ValidFrom = reader["VALID_FROM"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["VALID_FROM"]) : null,
                                ValidTo = reader["VALID_TO"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["VALID_TO"]) : null,
                                CreatedAt = Convert.ToDateTime(reader["CREATED_AT"])
                            });
                        }
                    }
                }
            }
            return results;
        }

        public List<KeyValuePair<string, string>> GetDepartmentOptions()
        {
            var list = new List<KeyValuePair<string, string>>();
            try
            {
                using (var db = new MyEntities())
                {
                    var depts = db.TB_PHONGBAN.OrderBy(p => p.TENPB).Select(p => new { p.IDPB, p.TENPB }).ToList();
                    foreach (var d in depts)
                    {
                        list.Add(new KeyValuePair<string, string>(d.IDPB.ToString(), d.TENPB));
                    }
                }
            }
            catch { }
            return list;
        }

        public List<KeyValuePair<string, string>> GetCompanyOptions()
        {
            var list = new List<KeyValuePair<string, string>>();
            try
            {
                using (var db = new MyEntities())
                {
                    var comps = db.TB_CONGTY.OrderBy(c => c.TENCTY).Select(c => new { c.IDCTY, c.TENCTY }).ToList();
                    foreach (var c in comps)
                    {
                        list.Add(new KeyValuePair<string, string>(c.IDCTY.ToString(), c.TENCTY));
                    }
                }
            }
            catch { }
            return list;
        }

        public long GetCurrentPolicyRevision()
        {
            using (var db = new MyEntities())
            {
                var conn = (OracleConnection)db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT REVISION_NUMBER FROM AI_OWNER.TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL'";
                    var res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) return Convert.ToInt64(res);
                }
            }
            return 0;
        }

        public bool SaveGrantsTransactional(
            string subjectType,
            int subjectId,
            List<AiScopeGrantRecord> grantsToSave,
            long expectedRevision,
            int actorUserId,
            string actorUsername,
            string clientType,
            out long newRevision,
            out int affectedCount,
            out string errorMessage)
        {
            newRevision = expectedRevision;
            affectedCount = 0;
            errorMessage = null;

            if (!IsSchemaReady(out string readyErr))
            {
                errorMessage = readyErr;
                return false;
            }

            using (var db = new MyEntities())
            {
                var conn = (OracleConnection)db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Kiểm tra Optimistic Concurrency: Khóa dòng REVISION_KEY = 'POLICY_GLOBAL' FOR UPDATE
                        long currentRev = 0;
                        using (var cmdLock = conn.CreateCommand())
                        {
                            cmdLock.Transaction = trans;
                            cmdLock.CommandText = "SELECT REVISION_NUMBER FROM AI_OWNER.TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL' FOR UPDATE";
                            var revObj = cmdLock.ExecuteScalar();
                            if (revObj != null && revObj != DBNull.Value)
                            {
                                currentRev = Convert.ToInt64(revObj);
                            }
                        }

                        if (expectedRevision > 0 && currentRev != expectedRevision)
                        {
                            trans.Rollback();
                            errorMessage = $"Xung đột phiên bản chính sách: Dữ liệu đã bị thay đổi bởi quản trị viên khác (Revision hiện tại: {currentRev}, Revision của bạn: {expectedRevision}). Vui lòng tải lại trang trước khi lưu.";
                            return false;
                        }

                        // 2. Đọc các grant hiện có để kiểm tra No-Op (tránh ghi đè và tăng revision giả khi không có thay đổi)
                        var existingGrants = new List<AiScopeGrantRecord>();
                        using (var cmdRead = conn.CreateCommand())
                        {
                            cmdRead.Transaction = trans;
                            cmdRead.BindByName = true;
                            cmdRead.CommandText = @"
                                SELECT CAPABILITY_CODE, SCOPE_TYPE, SCOPE_KEY, EFFECT, VALID_FROM, VALID_TO
                                FROM AI_OWNER.TB_AI_SCOPE_GRANT
                                WHERE SUBJECT_TYPE = :p_stype AND SUBJECT_ID = :p_sid AND IS_ENABLED = 1";
                            cmdRead.Parameters.Add(new OracleParameter("p_stype", subjectType));
                            cmdRead.Parameters.Add(new OracleParameter("p_sid", subjectId));

                            using (var r = cmdRead.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    existingGrants.Add(new AiScopeGrantRecord
                                    {
                                        CapabilityCode = r["CAPABILITY_CODE"].ToString(),
                                        ScopeType = r["SCOPE_TYPE"].ToString(),
                                        ScopeKey = r["SCOPE_KEY"] != DBNull.Value ? r["SCOPE_KEY"].ToString() : null,
                                        Effect = r["EFFECT"].ToString(),
                                        ValidFrom = r["VALID_FROM"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["VALID_FROM"]) : null,
                                        ValidTo = r["VALID_TO"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["VALID_TO"]) : null
                                    });
                                }
                            }
                        }

                        bool AreGrantsIdentical(List<AiScopeGrantRecord> a, List<AiScopeGrantRecord> b)
                        {
                            if (a.Count != b.Count) return false;
                            string Key(AiScopeGrantRecord g) => $"{g.CapabilityCode}|{g.Effect}|{g.ScopeType}|{g.ScopeKey ?? ""}|{g.ValidFrom:yyyyMMddHHmmss}|{g.ValidTo:yyyyMMddHHmmss}";
                            var setA = new HashSet<string>(a.Select(Key));
                            return b.All(g => setA.Contains(Key(g)));
                        }

                        if (AreGrantsIdentical(existingGrants, grantsToSave ?? new List<AiScopeGrantRecord>()))
                        {
                            trans.Rollback();
                            newRevision = currentRev;
                            affectedCount = 0;
                            errorMessage = "NO_CHANGES";
                            return true;
                        }

                        // 3. Xóa các grant cũ của đối tượng này
                        using (var cmdDel = conn.CreateCommand())
                        {
                            cmdDel.Transaction = trans;
                            cmdDel.BindByName = true;
                            cmdDel.CommandText = "DELETE FROM AI_OWNER.TB_AI_SCOPE_GRANT WHERE SUBJECT_TYPE = :p_stype AND SUBJECT_ID = :p_sid";
                            cmdDel.Parameters.Add(new OracleParameter("p_stype", subjectType));
                            cmdDel.Parameters.Add(new OracleParameter("p_sid", subjectId));
                            cmdDel.ExecuteNonQuery();
                        }

                        // 4. Chèn danh sách grant mới
                        foreach (var g in grantsToSave ?? new List<AiScopeGrantRecord>())
                        {
                            using (var cmdIns = conn.CreateCommand())
                            {
                                cmdIns.Transaction = trans;
                                cmdIns.BindByName = true;
                                cmdIns.CommandText = @"
                                    INSERT INTO AI_OWNER.TB_AI_SCOPE_GRANT
                                    (SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, SCOPE_KEY, EFFECT, IS_ENABLED, VALID_FROM, VALID_TO, CREATED_AT)
                                    VALUES (:p_stype, :p_sid, :p_cap, :p_scope, :p_skey, :p_effect, 1, :p_from, :p_to, SYSDATE)";
                                cmdIns.Parameters.Add(new OracleParameter("p_stype", subjectType));
                                cmdIns.Parameters.Add(new OracleParameter("p_sid", subjectId));
                                cmdIns.Parameters.Add(new OracleParameter("p_cap", g.CapabilityCode));
                                cmdIns.Parameters.Add(new OracleParameter("p_scope", g.ScopeType));
                                cmdIns.Parameters.Add(new OracleParameter("p_skey", (object)g.ScopeKey ?? DBNull.Value));
                                cmdIns.Parameters.Add(new OracleParameter("p_effect", g.Effect));
                                cmdIns.Parameters.Add(new OracleParameter("p_from", (object)g.ValidFrom ?? DBNull.Value));
                                cmdIns.Parameters.Add(new OracleParameter("p_to", (object)g.ValidTo ?? DBNull.Value));
                                cmdIns.ExecuteNonQuery();
                            }
                        }

                        // 5. Tăng phiên bản chính sách thật trong TB_AI_REVISION
                        long updatedRev = currentRev + 1;
                        using (var cmdRev = conn.CreateCommand())
                        {
                            cmdRev.Transaction = trans;
                            cmdRev.CommandText = "UPDATE AI_OWNER.TB_AI_REVISION SET REVISION_NUMBER = REVISION_NUMBER + 1, UPDATED_AT = SYSDATE WHERE REVISION_KEY = 'POLICY_GLOBAL'";
                            cmdRev.ExecuteNonQuery();
                        }

                        // 6. Ghi Audit Log vào HR.TB_AUTH_AUDIT trong cùng transaction
                        using (var cmdAudit = conn.CreateCommand())
                        {
                            cmdAudit.Transaction = trans;
                            cmdAudit.BindByName = true;
                            cmdAudit.CommandText = @"
                                INSERT INTO HR.TB_AUTH_AUDIT
                                (ACTOR_USER_ID, USER_ID, EVENT_TYPE, RESULT, REASON, CLIENT_TYPE, OCCURRED_AT, METADATA_JSON)
                                VALUES
                                (:p_actor, :p_uid, 'AI_SCOPE_GRANT_UPDATE', 'SUCCESS', :p_reason, :p_client, SYSDATE, :p_meta)";
                            
                            cmdAudit.Parameters.Add(new OracleParameter("p_actor", actorUserId > 0 ? (object)actorUserId : DBNull.Value));
                            cmdAudit.Parameters.Add(new OracleParameter("p_uid", string.Equals(subjectType, "USER", StringComparison.OrdinalIgnoreCase) ? (object)subjectId : DBNull.Value));
                            cmdAudit.Parameters.Add(new OracleParameter("p_reason", $"Cập nhật phân quyền tra cứu AI ({grantsToSave?.Count ?? 0} bản ghi) cho {subjectType} #{subjectId}. Revision mới: {updatedRev}"));
                            cmdAudit.Parameters.Add(new OracleParameter("p_client", clientType ?? "DESKTOP"));
                            
                            string meta = JsonConvert.SerializeObject(new
                            {
                                Actor = actorUsername,
                                SubjectType = subjectType,
                                SubjectId = subjectId,
                                PreviousCount = existingGrants.Count,
                                NewCount = grantsToSave?.Count ?? 0,
                                Revision = updatedRev,
                                Timestamp = DateTime.UtcNow
                            });
                            cmdAudit.Parameters.Add(new OracleParameter("p_meta", meta));
                            cmdAudit.ExecuteNonQuery();
                        }

                        trans.Commit();

                        newRevision = updatedRev;
                        affectedCount = grantsToSave?.Count ?? 0;

                        // Invalidate cache
                        try
                        {
                            AiCacheCoordinator.Instance.ResultCache.InvalidateAll();
                            AiCacheCoordinator.Instance.EntityCache.InvalidateAll();
                            AiCacheCoordinator.Instance.PlanCache.InvalidateAll();
                        }
                        catch { }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); } catch { }
                        errorMessage = "Lỗi trong tiến trình giao dịch Oracle: " + ex.Message;
                        return false;
                    }
                }
            }
        }
    }
}
