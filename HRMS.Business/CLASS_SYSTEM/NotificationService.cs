using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Bu.CLASS_SYSTEM
{
    public class NotificationItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Sender { get; set; }
        public string CreatedAt { get; set; }
        public string Type { get; set; }
        public bool IsPinned { get; set; }
        public string AttachedFile { get; set; }
        public bool IsUnread { get; set; }
        public string TargetType { get; set; }
        public long? TargetManv { get; set; }
        public string EventType { get; set; }
        public decimal? EntityId { get; set; }
    }

    public static class NotificationService
    {
        private static string GetConnectionString()
        {
            using (var db = new MyEntities())
            {
                return Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
            }
        }

        /// <summary>
        /// Tạo thông báo trong CSDL và tự động xếp hàng Remote Push Outbox trong cùng transaction
        /// </summary>
        public static long CreateNotification(
            OracleConnection conn,
            OracleTransaction tx,
            string title,
            string content,
            string type,
            string sender,
            string targetType = "PERSONAL",
            long? targetManv = null,
            string mapb = null,
            string macty = null,
            string eventType = null,
            decimal? entityId = null,
            string discretePushBody = null,
            bool isPinned = false,
            string attachedFile = null,
            DateTime? expiresAt = null,
            bool isActive = true)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề thông báo không được để trống.", nameof(title));

            string normTargetType = (targetType ?? (targetManv.HasValue ? "PERSONAL" : "ALL")).ToUpperInvariant();
            if (normTargetType != "PERSONAL" && normTargetType != "DEPARTMENT" && normTargetType != "COMPANY" && normTargetType != "ALL")
            {
                throw new ArgumentException("Phạm vi thông báo không hợp lệ.", nameof(targetType));
            }

            if (normTargetType == "PERSONAL" && (!targetManv.HasValue || targetManv.Value <= 0))
                throw new ArgumentException("Thông báo cá nhân cần nhân viên nhận.", nameof(targetManv));
            if (normTargetType == "DEPARTMENT" && string.IsNullOrWhiteSpace(mapb))
                throw new ArgumentException("Thông báo phòng ban cần phòng ban nhận.", nameof(mapb));
            if (normTargetType == "COMPANY" && string.IsNullOrWhiteSpace(macty))
                throw new ArgumentException("Thông báo công ty cần công ty nhận.", nameof(macty));

            // 1. Sinh ID an toàn bằng SEQUENCE SEQ_THONGBAO
            long newNotifId;
            using (var cmdSeq = new OracleCommand("SELECT SEQ_THONGBAO.NEXTVAL FROM DUAL", conn))
            {
                if (tx != null) cmdSeq.Transaction = tx;
                newNotifId = Convert.ToInt64(cmdSeq.ExecuteScalar());
            }

            // 2. Chèn vào TB_THONGBAO
            string insertSql = @"
                INSERT INTO TB_THONGBAO 
                (ID, TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, MACTY, MAPB, TARGET_TYPE, TARGET_MANV, EVENT_TYPE, ENTITY_ID, FILE_DINHKEM, NGAY_HETHAN)
                VALUES 
                (:p_id, :p_title, :p_content, :p_sender, SYSTIMESTAMP, :p_type, :p_pinned, :p_active, :p_cty, :p_pb, :p_ttype, :p_tmanv, :p_etype, :p_eid, :p_file, :p_expiry)";

            using (var cmd = new OracleCommand(insertSql, conn))
            {
                if (tx != null) cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_id", newNotifId);
                cmd.Parameters.Add("p_title", title);
                cmd.Parameters.Add("p_content", (object)content ?? DBNull.Value);
                cmd.Parameters.Add("p_sender", sender ?? "Hệ thống");
                cmd.Parameters.Add("p_type", type ?? "THONG_BAO_CHUNG");
                cmd.Parameters.Add("p_cty", string.IsNullOrWhiteSpace(macty) ? (object)DBNull.Value : macty.Trim());
                cmd.Parameters.Add("p_pb", string.IsNullOrWhiteSpace(mapb) ? (object)DBNull.Value : mapb.Trim());
                cmd.Parameters.Add("p_ttype", normTargetType);
                cmd.Parameters.Add("p_tmanv", (object)targetManv ?? DBNull.Value);
                cmd.Parameters.Add("p_etype", string.IsNullOrWhiteSpace(eventType) ? (object)DBNull.Value : eventType.Trim());
                cmd.Parameters.Add("p_eid", (object)entityId ?? DBNull.Value);

                cmd.Parameters.Add("p_pinned", isPinned ? 1 : 0);
                cmd.Parameters.Add("p_active", isActive ? 1 : 0);
                cmd.Parameters.Add("p_file", (object)attachedFile ?? DBNull.Value);
                cmd.Parameters.Add("p_expiry", (object)expiresAt ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            if (!isActive || (expiresAt.HasValue && expiresAt.Value < DateTime.Now)) return newNotifId;

            // 3. Tìm danh sách thiết bị nhận hợp lệ và chèn vào TB_PUSH_OUTBOX
            try
            {
                var targetDevices = DeviceRegistrationService.GetActiveDevicesForTarget(
                    normTargetType,
                    targetManv,
                    mapb,
                    macty,
                    conn,
                    tx
                );

                if (targetDevices != null && targetDevices.Count > 0)
                {
                    string safeDiscreteBody = discretePushBody ?? "HRMS: Yêu cầu của bạn đã có kết quả. Mở ứng dụng để xem.";

                    string outboxSql = @"
                        INSERT INTO TB_PUSH_OUTBOX
                        (ID, NOTIFICATION_ID, DEVICE_ID, IDUSER, INSTALLATION_ID, EXPO_PUSH_TOKEN,
                         TITLE, BODY, DATA_JSON, STATUS, RETRY_COUNT, MAX_RETRIES, NEXT_RETRY_AT,
                         IDEMPOTENCY_KEY, CREATED_AT)
                        VALUES
                        (SEQ_PUSH_OUTBOX.NEXTVAL, :p_nid, :p_did, :p_uid, :p_install, :p_token,
                         :p_title, :p_body, :p_data, 'PENDING', 0, 5, SYSDATE,
                         :p_idemp, SYSDATE)";

                    foreach (var dev in targetDevices)
                    {
                        var dataPayload = new
                        {
                            notificationId = newNotifId,
                            eventType = eventType ?? type ?? "ANNOUNCEMENT",
                            entityId = entityId.HasValue ? entityId.Value.ToString() : null,
                            type = type ?? "THONG_BAO_CHUNG",
                            title = title,
                            payloadVersion = "1.0"
                        };

                        string jsonPayload = JsonConvert.SerializeObject(dataPayload);
                        string idempotencyKey = $"{newNotifId}_{dev.DeviceId}";

                        using (var cmdOut = new OracleCommand(outboxSql, conn))
                        {
                            if (tx != null) cmdOut.Transaction = tx;
                            cmdOut.BindByName = true;
                            cmdOut.Parameters.Add("p_nid", newNotifId);
                            cmdOut.Parameters.Add("p_did", dev.DeviceId);
                            cmdOut.Parameters.Add("p_uid", dev.IdUser);
                            cmdOut.Parameters.Add("p_install", dev.InstallationId);
                            cmdOut.Parameters.Add("p_token", dev.ExpoPushToken);
                            cmdOut.Parameters.Add("p_title", "HRMS");
                            cmdOut.Parameters.Add("p_body", safeDiscreteBody);
                            cmdOut.Parameters.Add("p_data", jsonPayload);
                            cmdOut.Parameters.Add("p_idemp", idempotencyKey);

                            try
                            {
                                cmdOut.ExecuteNonQuery();
                            }
                            catch (OracleException oex) when (oex.Number == 1) // ORA-00001 unique constraint
                            {
                                // Bỏ qua trùng lặp idempotency key
                            }
                        }
                    }
                }
            }
            catch (Exception exOutbox)
            {
                System.Diagnostics.Trace.TraceWarning("[NotificationService.CreateNotification Outbox Warning]: " + exOutbox.Message);
                // Không hủy transaction của thông báo nội bộ nếu chỉ là lỗi tính toán outbox
            }

            return newNotifId;
        }

        // Keep the original CLR signature for existing Desktop callers.
        public static long CreateNotification(OracleConnection conn, OracleTransaction tx, string title,
            string content, string type, string sender, string targetType, long? targetManv,
            string mapb, string macty, string eventType, decimal? entityId, string discretePushBody)
        {
            return CreateNotification(conn, tx, title, content, type, sender, targetType, targetManv,
                mapb, macty, eventType, entityId, discretePushBody, false, null, null, true);
        }

        /// <summary>
        /// Overload tương thích ngược cho các lời gọi cũ
        /// </summary>
        public static void CreateNotification(
            OracleConnection conn,
            string title,
            string content,
            string type,
            string sender,
            long? targetManv = null)
        {
            CreateNotification(
                conn,
                null,
                title,
                content,
                type,
                sender,
                targetType: targetManv.HasValue ? "PERSONAL" : "ALL",
                targetManv: targetManv,
                mapb: null,
                macty: null,
                eventType: type,
                entityId: null
            );
        }

        private static string BuildTargetWhereClause()
        {
            return @"
                (t.TRANGTHAI IS NULL OR t.TRANGTHAI = 1)
                AND (t.NGAY_HETHAN IS NULL OR t.NGAY_HETHAN >= SYSDATE)
                AND (
                    (t.TARGET_TYPE = 'PERSONAL' AND :p_manv IS NOT NULL AND t.TARGET_MANV = :p_manv)
                    OR (t.TARGET_TYPE = 'DEPARTMENT' AND t.MAPB IS NOT NULL AND :p_pb IS NOT NULL AND t.MAPB = :p_pb AND (t.MACTY IS NULL OR (:p_cty IS NOT NULL AND t.MACTY = :p_cty)))
                    OR (t.TARGET_TYPE = 'COMPANY' AND t.MACTY IS NOT NULL AND :p_cty IS NOT NULL AND t.MACTY = :p_cty)
                    OR ((t.TARGET_TYPE = 'ALL' OR t.TARGET_TYPE IS NULL) AND (t.MACTY IS NULL OR (:p_cty IS NOT NULL AND t.MACTY = :p_cty)) AND (t.MAPB IS NULL OR (:p_pb IS NOT NULL AND t.MAPB = :p_pb)))
                )";
        }

        public static List<NotificationItemDto> GetNotificationsForUser(long userId, long? manv, string cty, string pb = null, int limit = 50)
        {
            return GetNotificationsForUser(userId, manv, cty, pb, limit, 0, false);
        }

        public static List<NotificationItemDto> GetNotificationsForUser(long userId, long? manv, string cty, string pb, int limit, int offset, bool unreadOnly)
        {
            limit = Math.Max(1, Math.Min(100, limit));
            offset = Math.Max(0, offset);
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                return ReadNotifications(conn, userId, manv, cty, pb, limit, offset, unreadOnly);
            }
        }

        private static List<NotificationItemDto> ReadNotifications(OracleConnection conn, long userId, long? manv, string cty, string pb, int limit, int offset, bool unreadOnly)
        {
            var list = new List<NotificationItemDto>();
            string sql = $@"
                SELECT * FROM (
                    SELECT 
                        t.ID, t.TIEUDE, t.NOIDUNG, t.NGUOIDANG, t.NGAYDANG, t.LOAI_TB, t.IS_PINNED, t.FILE_DINHKEM,
                        t.TARGET_TYPE, t.TARGET_MANV, t.EVENT_TYPE, t.ENTITY_ID,
                        NVL(nd.IS_READ, 0) AS IS_READ,
                        ROW_NUMBER() OVER (ORDER BY t.IS_PINNED DESC NULLS LAST, t.NGAYDANG DESC, t.ID DESC) AS PAGE_ROW
                    FROM TB_THONGBAO t
                    LEFT JOIN TB_THONGBAO_NGUOIDUNG nd ON t.ID = nd.ID_THONGBAO AND nd.IDUSER = :p_uid
                    WHERE {BuildTargetWhereClause()}
                      AND (:p_unread = 0 OR NVL(nd.IS_READ, 0) = 0)
                ) WHERE PAGE_ROW > :p_offset AND PAGE_ROW <= :p_end ORDER BY PAGE_ROW";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("p_uid", userId);
                cmd.Parameters.Add("p_manv", (object)manv ?? DBNull.Value);
                cmd.Parameters.Add("p_pb", string.IsNullOrWhiteSpace(pb) ? (object)DBNull.Value : pb.Trim());
                cmd.Parameters.Add("p_cty", string.IsNullOrWhiteSpace(cty) ? (object)DBNull.Value : cty.Trim());
                cmd.Parameters.Add("p_offset", offset);
                cmd.Parameters.Add("p_end", (long)offset + limit);
                cmd.Parameters.Add("p_unread", unreadOnly ? 1 : 0);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int isRead = Convert.ToInt32(r["IS_READ"]);
                        list.Add(new NotificationItemDto
                        {
                            Id = Convert.ToInt64(r["ID"]),
                            Title = r["TIEUDE"]?.ToString(),
                            Content = r["NOIDUNG"]?.ToString(),
                            Sender = r["NGUOIDANG"]?.ToString(),
                            CreatedAt = r["NGAYDANG"] != DBNull.Value ? Convert.ToDateTime(r["NGAYDANG"]).ToString("dd/MM/yyyy HH:mm") : "",
                            Type = r["LOAI_TB"]?.ToString() ?? "Thông báo chung",
                            IsPinned = r["IS_PINNED"] != DBNull.Value && Convert.ToInt32(r["IS_PINNED"]) == 1,
                            AttachedFile = r["FILE_DINHKEM"]?.ToString(),
                            IsUnread = isRead == 0,
                            TargetType = r["TARGET_TYPE"]?.ToString(),
                            TargetManv = r["TARGET_MANV"] != DBNull.Value ? (long?)Convert.ToInt64(r["TARGET_MANV"]) : null,
                            EventType = r["EVENT_TYPE"]?.ToString(),
                            EntityId = r["ENTITY_ID"] != DBNull.Value ? (decimal?)Convert.ToDecimal(r["ENTITY_ID"]) : null
                        });
                    }
                }
            }
            return list;
        }

        public static NotificationItemDto GetNotificationDetailForUser(long notifId, long userId, long? manv, string cty, string pb = null)
        {
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = $@"
                    SELECT 
                        t.ID, t.TIEUDE, t.NOIDUNG, t.NGUOIDANG, t.NGAYDANG, t.LOAI_TB, t.IS_PINNED, t.FILE_DINHKEM,
                        t.TARGET_TYPE, t.TARGET_MANV, t.EVENT_TYPE, t.ENTITY_ID,
                        NVL(nd.IS_READ, 0) AS IS_READ
                    FROM TB_THONGBAO t
                    LEFT JOIN TB_THONGBAO_NGUOIDUNG nd ON t.ID = nd.ID_THONGBAO AND nd.IDUSER = :p_uid
                    WHERE t.ID = :p_nid AND {BuildTargetWhereClause()}";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_nid", notifId);
                    cmd.Parameters.Add("p_uid", userId);
                    cmd.Parameters.Add("p_manv", (object)manv ?? DBNull.Value);
                    cmd.Parameters.Add("p_pb", string.IsNullOrWhiteSpace(pb) ? (object)DBNull.Value : pb.Trim());
                    cmd.Parameters.Add("p_cty", string.IsNullOrWhiteSpace(cty) ? (object)DBNull.Value : cty.Trim());

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            var dto = new NotificationItemDto
                            {
                                Id = Convert.ToInt64(r["ID"]),
                                Title = r["TIEUDE"]?.ToString(),
                                Content = r["NOIDUNG"]?.ToString(),
                                Sender = r["NGUOIDANG"]?.ToString(),
                                CreatedAt = r["NGAYDANG"] != DBNull.Value ? Convert.ToDateTime(r["NGAYDANG"]).ToString("dd/MM/yyyy HH:mm") : "",
                                Type = r["LOAI_TB"]?.ToString() ?? "Thông báo chung",
                                IsPinned = r["IS_PINNED"] != DBNull.Value && Convert.ToInt32(r["IS_PINNED"]) == 1,
                                AttachedFile = r["FILE_DINHKEM"]?.ToString(),
                                IsUnread = false,
                                TargetType = r["TARGET_TYPE"]?.ToString(),
                                TargetManv = r["TARGET_MANV"] != DBNull.Value ? (long?)Convert.ToInt64(r["TARGET_MANV"]) : null,
                                EventType = r["EVENT_TYPE"]?.ToString(),
                                EntityId = r["ENTITY_ID"] != DBNull.Value ? (decimal?)Convert.ToDecimal(r["ENTITY_ID"]) : null
                            };

                            // Tự động đánh dấu đã đọc
                            MarkAsReadInternal(conn, notifId, userId, manv);

                            return dto;
                        }
                    }
                }
            }
            return null;
        }

        public static int GetUnreadCount(long userId, long? manv, string cty, string pb = null)
        {
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = $@"
                    SELECT COUNT(*)
                    FROM TB_THONGBAO t
                    LEFT JOIN TB_THONGBAO_NGUOIDUNG nd ON t.ID = nd.ID_THONGBAO AND nd.IDUSER = :p_uid
                    WHERE {BuildTargetWhereClause()}
                      AND NVL(nd.IS_READ, 0) = 0";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_uid", userId);
                    cmd.Parameters.Add("p_manv", (object)manv ?? DBNull.Value);
                    cmd.Parameters.Add("p_pb", string.IsNullOrWhiteSpace(pb) ? (object)DBNull.Value : pb.Trim());
                    cmd.Parameters.Add("p_cty", string.IsNullOrWhiteSpace(cty) ? (object)DBNull.Value : cty.Trim());

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private static void MarkAsReadInternal(OracleConnection conn, long notifId, long userId, long? manv, OracleTransaction tx = null)
        {
            if (tx == null)
            {
                using (var ownTransaction = conn.BeginTransaction())
                {
                    MarkAsReadInternal(conn, notifId, userId, manv, ownTransaction);
                    ownTransaction.Commit();
                    return;
                }
            }
            using (var gate = new OracleCommand("SELECT IDUSER FROM TB_SYS_USER WHERE IDUSER=:p_userid FOR UPDATE",conn))
            {
                gate.Transaction=tx; gate.Parameters.Add("p_userid",userId); gate.ExecuteScalar();
            }
            string mergeSql = @"
                MERGE INTO TB_THONGBAO_NGUOIDUNG tgt
                USING (SELECT :p_nid AS NID, :p_uid AS USER_ID FROM DUAL) src
                ON (tgt.ID_THONGBAO = src.NID AND tgt.IDUSER = src.USER_ID)
                WHEN MATCHED THEN
                    UPDATE SET IS_READ = 1, READ_AT = SYSDATE
                WHEN NOT MATCHED THEN
                    INSERT (ID, ID_THONGBAO, IDUSER, MANV, IS_READ, READ_AT)
                    VALUES (SEQ_THONGBAO_NGUOIDUNG.NEXTVAL, src.NID, src.USER_ID, :p_manv, 1, SYSDATE)";

            using (var cmd = new OracleCommand(mergeSql, conn))
            {
                cmd.Transaction=tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_nid", notifId);
                cmd.Parameters.Add("p_uid", userId);
                cmd.Parameters.Add("p_manv", (object)manv ?? DBNull.Value);
                try { cmd.ExecuteNonQuery(); }
                catch (OracleException ex) when (ex.Number == 1)
                {
                    using (var retry = new OracleCommand("UPDATE TB_THONGBAO_NGUOIDUNG SET IS_READ=1, READ_AT=SYSDATE WHERE ID_THONGBAO=:p_nid AND IDUSER=:p_userid", conn))
                    {
                        retry.Transaction=tx; retry.BindByName=true; retry.Parameters.Add("p_nid",notifId); retry.Parameters.Add("p_userid",userId); retry.ExecuteNonQuery();
                    }
                }
            }
        }

        public static bool MarkAsRead(long notifId, long userId, long? manv, string cty, string pb = null)
        {
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                // Kiểm tra thông báo có thuộc phạm vi của người dùng hay không
                string checkSql = $@"
                    SELECT COUNT(*) FROM TB_THONGBAO t
                    WHERE t.ID = :p_nid AND {BuildTargetWhereClause()}";

                using (var cmdCheck = new OracleCommand(checkSql, conn))
                {
                    cmdCheck.BindByName = true;
                    cmdCheck.Parameters.Add("p_nid", notifId);
                    cmdCheck.Parameters.Add("p_manv", (object)manv ?? DBNull.Value);
                    cmdCheck.Parameters.Add("p_pb", string.IsNullOrWhiteSpace(pb) ? (object)DBNull.Value : pb.Trim());
                    cmdCheck.Parameters.Add("p_cty", string.IsNullOrWhiteSpace(cty) ? (object)DBNull.Value : cty.Trim());

                    int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (count == 0) return false;
                }

                MarkAsReadInternal(conn, notifId, userId, manv);
                return true;
            }
        }

        private static void MarkAllAsReadInternal(OracleConnection conn, OracleTransaction tx, long userId, long? manv, string cty, string pb)
        {
            // Serialize read-all requests from different installations of this account.
            using (var gate = new OracleCommand("SELECT IDUSER FROM TB_SYS_USER WHERE IDUSER=:p_userid FOR UPDATE",conn))
            {
                gate.Transaction=tx; gate.Parameters.Add("p_userid",userId); gate.ExecuteScalar();
            }
            string sql = $@"
                MERGE INTO TB_THONGBAO_NGUOIDUNG tgt
                USING (SELECT t.ID FROM TB_THONGBAO t WHERE {BuildTargetWhereClause()}) src
                ON (tgt.ID_THONGBAO=src.ID AND tgt.IDUSER=:p_uid)
                WHEN MATCHED THEN UPDATE SET IS_READ=1, READ_AT=SYSDATE WHERE NVL(tgt.IS_READ,0)=0
                WHEN NOT MATCHED THEN INSERT (ID,ID_THONGBAO,IDUSER,MANV,IS_READ,READ_AT)
                VALUES (SEQ_THONGBAO_NGUOIDUNG.NEXTVAL,src.ID,:p_uid,:p_manv,1,SYSDATE)";
            using (var cmd = new OracleCommand(sql,conn))
            {
                cmd.Transaction=tx; cmd.BindByName=true;
                cmd.Parameters.Add("p_uid",userId);
                cmd.Parameters.Add("p_manv",(object)manv??DBNull.Value);
                cmd.Parameters.Add("p_cty",string.IsNullOrWhiteSpace(cty)?(object)DBNull.Value:cty.Trim());
                cmd.Parameters.Add("p_pb",string.IsNullOrWhiteSpace(pb)?(object)DBNull.Value:pb.Trim());
                cmd.ExecuteNonQuery();
            }
        }

        public static void MarkAllAsRead(long userId, long? manv, string cty, string pb = null)
        {
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    MarkAllAsReadInternal(conn, tx, userId, manv, cty, pb);
                    tx.Commit();
                }
            }
        }
    }
}
