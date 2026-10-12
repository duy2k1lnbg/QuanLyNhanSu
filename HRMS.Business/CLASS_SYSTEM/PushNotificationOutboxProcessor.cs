using DA;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bu.CLASS_SYSTEM
{
    public class OutboxJobItem
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long DeviceId { get; set; }
        public long IdUser { get; set; }
        public string InstallationId { get; set; }
        public string ExpoPushToken { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string DataJson { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; }
    }

    public static class PushNotificationOutboxProcessor
    {
        private static readonly HttpClient _httpClient;
        private const string EXPO_SEND_URL = "https://exp.host/--/api/v2/push/send";
        private const string EXPO_RECEIPTS_URL = "https://exp.host/--/api/v2/push/getReceipts";

        static PushNotificationOutboxProcessor()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                MaxConnectionsPerServer = 10
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string GetConnectionString()
        {
            using (var db = new MyEntities())
            {
                return Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
            }
        }

        /// <summary>
        /// Xử lý gửi các thông báo Outbox đang chờ (PENDING, ERROR)
        /// </summary>
        public static async Task<int> ProcessPendingOutboxAsync(int batchSize = 100, CancellationToken cancellationToken = default)
        {
            string workerId = "Worker_" + Environment.MachineName + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var jobs = ClaimPendingJobs(workerId, batchSize);
            if (jobs.Count == 0) return 0;

            int processedCount = 0;

            // 1. Kiểm tra lại tính hợp lệ của liên kết thiết bị trước khi gửi
            var validJobs = new List<OutboxJobItem>();
            string connStr = GetConnectionString();

            using (var conn = new OracleConnection(connStr))
            {
                await conn.OpenAsync(cancellationToken);

                foreach (var job in jobs)
                {
                    bool isStillValid = false;
                    string checkSql = "SELECT IDUSER, IS_ACTIVE, PUSH_ENABLED, EXPO_PUSH_TOKEN FROM TB_USER_DEVICE WHERE ID = :p_did";
                    using (var cmd = new OracleCommand(checkSql, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_did", job.DeviceId);
                        using (var r = await cmd.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await r.ReadAsync(cancellationToken))
                            {
                                long currentUid = Convert.ToInt64(r["IDUSER"]);
                                int isActive = Convert.ToInt32(r["IS_ACTIVE"]);
                                int pushEn = Convert.ToInt32(r["PUSH_ENABLED"]);
                                string currentToken = r["EXPO_PUSH_TOKEN"]?.ToString();

                                if (currentUid == job.IdUser && isActive == 1 && pushEn == 1 && !string.IsNullOrWhiteSpace(currentToken))
                                {
                                    job.ExpoPushToken = currentToken;
                                    isStillValid = true;
                                }
                            }
                        }
                    }

                    if (isStillValid)
                    {
                        validJobs.Add(job);
                    }
                    else
                    {
                        // Đánh dấu SKIPPED nếu thiết bị đã đổi tài khoản, bị vô hiệu hóa hoặc tắt push
                        string skipSql = @"
                            UPDATE TB_PUSH_OUTBOX 
                            SET STATUS = 'SKIPPED',
                                LOCKED_UNTIL = NULL,
                                LOCKED_BY = NULL,
                                LAST_ERROR = 'Thiết bị không còn thuộc tài khoản này hoặc đã tắt thông báo Push.'
                            WHERE ID = :p_id";

                        using (var cmdSkip = new OracleCommand(skipSql, conn))
                        {
                            cmdSkip.BindByName = true;
                            cmdSkip.Parameters.Add("p_id", job.Id);
                            await cmdSkip.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                }
            }

            if (validJobs.Count == 0) return jobs.Count;

            // 2. Chia nhóm tối đa 100 bản ghi mỗi request tới Expo
            for (int i = 0; i < validJobs.Count; i += 100)
            {
                if (cancellationToken.IsCancellationRequested) break;
                var chunk = validJobs.Skip(i).Take(100).ToList();
                await SendChunkToExpoAsync(chunk, cancellationToken);
                processedCount += chunk.Count;
            }

            return processedCount;
        }

        private static List<OutboxJobItem> ClaimPendingJobs(string workerId, int limit)
        {
            var jobs = new List<OutboxJobItem>();
            string connStr = GetConnectionString();

            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                // Khóa an toàn các bản ghi
                string lockSql = @"
                    UPDATE TB_PUSH_OUTBOX
                    SET STATUS = 'PROCESSING',
                        LOCKED_UNTIL = SYSDATE + (2 / 1440),
                        LOCKED_BY = :p_worker
                    WHERE ID IN (
                        SELECT ID FROM (
                            SELECT ID FROM TB_PUSH_OUTBOX
                            WHERE STATUS IN ('PENDING', 'ERROR')
                              AND NEXT_RETRY_AT <= SYSDATE
                              AND (LOCKED_UNTIL IS NULL OR LOCKED_UNTIL < SYSDATE)
                              AND RETRY_COUNT < MAX_RETRIES
                            ORDER BY CREATED_AT ASC
                        ) WHERE ROWNUM <= :p_limit
                    )";

                using (var cmdLock = new OracleCommand(lockSql, conn))
                {
                    cmdLock.BindByName = true;
                    cmdLock.Parameters.Add("p_worker", workerId);
                    cmdLock.Parameters.Add("p_limit", limit);
                    cmdLock.ExecuteNonQuery();
                }

                // Đọc danh sách các bản ghi vừa được khóa
                string selectSql = @"
                    SELECT ID, NOTIFICATION_ID, DEVICE_ID, IDUSER, INSTALLATION_ID, EXPO_PUSH_TOKEN,
                           TITLE, BODY, DATA_JSON, RETRY_COUNT, MAX_RETRIES
                    FROM TB_PUSH_OUTBOX
                    WHERE LOCKED_BY = :p_worker AND STATUS = 'PROCESSING'";

                using (var cmdSelect = new OracleCommand(selectSql, conn))
                {
                    cmdSelect.BindByName = true;
                    cmdSelect.Parameters.Add("p_worker", workerId);

                    using (var reader = cmdSelect.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jobs.Add(new OutboxJobItem
                            {
                                Id = Convert.ToInt64(reader["ID"]),
                                NotificationId = Convert.ToInt64(reader["NOTIFICATION_ID"]),
                                DeviceId = Convert.ToInt64(reader["DEVICE_ID"]),
                                IdUser = Convert.ToInt64(reader["IDUSER"]),
                                InstallationId = reader["INSTALLATION_ID"]?.ToString(),
                                ExpoPushToken = reader["EXPO_PUSH_TOKEN"]?.ToString(),
                                Title = reader["TITLE"]?.ToString(),
                                Body = reader["BODY"]?.ToString(),
                                DataJson = reader["DATA_JSON"]?.ToString(),
                                RetryCount = Convert.ToInt32(reader["RETRY_COUNT"]),
                                MaxRetries = Convert.ToInt32(reader["MAX_RETRIES"])
                            });
                        }
                    }
                }
            }

            return jobs;
        }

        private static async Task SendChunkToExpoAsync(List<OutboxJobItem> chunk, CancellationToken cancellationToken)
        {
            var payloadList = new List<object>();

            foreach (var job in chunk)
            {
                object customData = null;
                if (!string.IsNullOrWhiteSpace(job.DataJson))
                {
                    try { customData = JsonConvert.DeserializeObject(job.DataJson); } catch { }
                }

                payloadList.Add(new
                {
                    to = job.ExpoPushToken,
                    sound = "default",
                    title = job.Title,
                    body = job.Body,
                    data = customData,
                    channelId = "hrms_default_channel",
                    priority = "high"
                });
            }

            string jsonPayload = JsonConvert.SerializeObject(payloadList);
            using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = null;
                string responseBody = null;
                try
                {
                    response = await _httpClient.PostAsync(EXPO_SEND_URL, content, cancellationToken);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    // Lỗi mạng hoặc timeout
                    await MarkChunkAsNetworkErrorAsync(chunk, ex.Message);
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    int statusCode = (int)response.StatusCode;
                    bool isTransient = statusCode == 429 || statusCode >= 500;
                    await MarkChunkAsHttpErrorAsync(chunk, statusCode, responseBody, isTransient);
                    return;
                }

                // Phân tích kết quả từng ticket
                try
                {
                    var resultObj = JObject.Parse(responseBody);
                    var dataArray = resultObj["data"] as JArray;

                    if (dataArray != null)
                    {
                        for (int i = 0; i < chunk.Count && i < dataArray.Count; i++)
                        {
                            var job = chunk[i];
                            var ticket = dataArray[i];
                            string status = ticket["status"]?.ToString();

                            if (string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
                            {
                                string ticketId = ticket["id"]?.ToString();
                                await UpdateJobTicketOkAsync(job.Id, ticketId);
                            }
                            else
                            {
                                string message = ticket["message"]?.ToString();
                                string errorCode = ticket["details"]?["error"]?.ToString();
                                await HandleTicketErrorAsync(job, errorCode, message);
                            }
                        }
                    }
                }
                catch (Exception exParse)
                {
                    await MarkChunkAsNetworkErrorAsync(chunk, "Không thể phân tích phản hồi từ Expo: " + exParse.Message);
                }
            }
        }

        private static async Task UpdateJobTicketOkAsync(long outboxId, string ticketId)
        {
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = @"
                    UPDATE TB_PUSH_OUTBOX
                    SET STATUS = 'TICKET_OK',
                        TICKET_ID = :p_tid,
                        TICKET_STATUS = 'ok',
                        SENT_AT = SYSDATE,
                        LOCKED_UNTIL = NULL,
                        LOCKED_BY = NULL,
                        LAST_ERROR = NULL
                    WHERE ID = :p_id";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_tid", (object)ticketId ?? DBNull.Value);
                    cmd.Parameters.Add("p_id", outboxId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task HandleTicketErrorAsync(OutboxJobItem job, string errorCode, string message)
        {
            bool isFatal = false;
            if (string.Equals(errorCode, "DeviceNotRegistered", StringComparison.OrdinalIgnoreCase))
            {
                isFatal = true;
                // Vô hiệu hóa thiết bị ngay lập tức
                DeviceRegistrationService.DeactivateToken(job.ExpoPushToken, "Expo reported DeviceNotRegistered");
            }
            else if (string.Equals(errorCode, "MessageTooBig", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(errorCode, "InvalidCredentials", StringComparison.OrdinalIgnoreCase))
            {
                isFatal = true;
            }

            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                await conn.OpenAsync();

                if (isFatal)
                {
                    string sqlFatal = @"
                        UPDATE TB_PUSH_OUTBOX
                        SET STATUS = 'FAILED_FATAL',
                            TICKET_STATUS = 'error',
                            TICKET_ERROR = :p_err,
                            LAST_ERROR = :p_msg,
                            LOCKED_UNTIL = NULL,
                            LOCKED_BY = NULL
                        WHERE ID = :p_id";

                    using (var cmd = new OracleCommand(sqlFatal, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_err", (object)errorCode ?? "FATAL_ERROR");
                        cmd.Parameters.Add("p_msg", (object)message ?? "Lỗi nghiêm trọng từ Expo");
                        cmd.Parameters.Add("p_id", job.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    int nextRetryCount = job.RetryCount + 1;
                    double backoffMinutes = Math.Pow(2, Math.Min(nextRetryCount, 5)) * 0.5; // 1m, 2m, 4m, 8m, 16m

                    string sqlRetry = @"
                        UPDATE TB_PUSH_OUTBOX
                        SET STATUS = CASE WHEN :p_retries >= MAX_RETRIES THEN 'FAILED_FATAL' ELSE 'ERROR' END,
                            RETRY_COUNT = :p_retries,
                            NEXT_RETRY_AT = SYSDATE + (:p_backoff / 1440),
                            TICKET_STATUS = 'error',
                            TICKET_ERROR = :p_err,
                            LAST_ERROR = :p_msg,
                            LOCKED_UNTIL = NULL,
                            LOCKED_BY = NULL
                        WHERE ID = :p_id";

                    using (var cmd = new OracleCommand(sqlRetry, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_retries", nextRetryCount);
                        cmd.Parameters.Add("p_backoff", backoffMinutes);
                        cmd.Parameters.Add("p_err", (object)errorCode ?? "TRANSIENT_ERROR");
                        cmd.Parameters.Add("p_msg", (object)message ?? "Lỗi tạm thời khi gửi push");
                        cmd.Parameters.Add("p_id", job.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private static async Task MarkChunkAsNetworkErrorAsync(List<OutboxJobItem> chunk, string errorMsg)
        {
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                await conn.OpenAsync();

                foreach (var job in chunk)
                {
                    int nextRetryCount = job.RetryCount + 1;
                    double backoffMinutes = Math.Pow(2, Math.Min(nextRetryCount, 5)) * 0.5;

                    string sql = @"
                        UPDATE TB_PUSH_OUTBOX
                        SET STATUS = CASE WHEN :p_retries >= MAX_RETRIES THEN 'FAILED_FATAL' ELSE 'ERROR' END,
                            RETRY_COUNT = :p_retries,
                            NEXT_RETRY_AT = SYSDATE + (:p_backoff / 1440),
                            LAST_ERROR = :p_msg,
                            LOCKED_UNTIL = NULL,
                            LOCKED_BY = NULL
                        WHERE ID = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_retries", nextRetryCount);
                        cmd.Parameters.Add("p_backoff", backoffMinutes);
                        cmd.Parameters.Add("p_msg", errorMsg != null && errorMsg.Length > 900 ? errorMsg.Substring(0, 900) : errorMsg);
                        cmd.Parameters.Add("p_id", job.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private static async Task MarkChunkAsHttpErrorAsync(List<OutboxJobItem> chunk, int statusCode, string body, bool isTransient)
        {
            string errMsg = $"HTTP {statusCode}: {body}";
            if (errMsg.Length > 900) errMsg = errMsg.Substring(0, 900);

            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                await conn.OpenAsync();

                foreach (var job in chunk)
                {
                    if (isTransient)
                    {
                        int nextRetryCount = job.RetryCount + 1;
                        double backoffMinutes = Math.Pow(2, Math.Min(nextRetryCount, 5)) * 1.0;

                        string sql = @"
                            UPDATE TB_PUSH_OUTBOX
                            SET STATUS = CASE WHEN :p_retries >= MAX_RETRIES THEN 'FAILED_FATAL' ELSE 'ERROR' END,
                                RETRY_COUNT = :p_retries,
                                NEXT_RETRY_AT = SYSDATE + (:p_backoff / 1440),
                                LAST_ERROR = :p_msg,
                                LOCKED_UNTIL = NULL,
                                LOCKED_BY = NULL
                            WHERE ID = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_retries", nextRetryCount);
                            cmd.Parameters.Add("p_backoff", backoffMinutes);
                            cmd.Parameters.Add("p_msg", errMsg);
                            cmd.Parameters.Add("p_id", job.Id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        string sql = @"
                            UPDATE TB_PUSH_OUTBOX
                            SET STATUS = 'FAILED_FATAL',
                                LAST_ERROR = :p_msg,
                                LOCKED_UNTIL = NULL,
                                LOCKED_BY = NULL
                            WHERE ID = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_msg", errMsg);
                            cmd.Parameters.Add("p_id", job.Id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý lấy Push Receipts từ Expo để xác nhận điện thoại đã nhận thông báo
        /// </summary>
        public static async Task<int> ProcessPendingReceiptsAsync(int batchSize = 300, CancellationToken cancellationToken = default)
        {
            var ticketJobs = new Dictionary<string, long>();
            string connStr = GetConnectionString();

            using (var conn = new OracleConnection(connStr))
            {
                await conn.OpenAsync(cancellationToken);

                // Lấy các bản ghi TICKET_OK được gửi cách đây ít nhất 15 phút và chưa kiểm tra receipt
                string sql = @"
                    SELECT * FROM (
                        SELECT ID, TICKET_ID
                        FROM TB_PUSH_OUTBOX
                        WHERE STATUS = 'TICKET_OK'
                          AND TICKET_ID IS NOT NULL
                          AND RECEIPT_STATUS IS NULL
                          AND SENT_AT <= SYSDATE - (5 / 1440)
                        ORDER BY SENT_AT ASC
                    ) WHERE ROWNUM <= :p_limit";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_limit", batchSize);

                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            long id = Convert.ToInt64(reader["ID"]);
                            string tid = reader["TICKET_ID"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(tid) && !ticketJobs.ContainsKey(tid))
                            {
                                ticketJobs[tid] = id;
                            }
                        }
                    }
                }
            }

            if (ticketJobs.Count == 0) return 0;

            var ticketIds = ticketJobs.Keys.ToList();
            string jsonBody = JsonConvert.SerializeObject(new { ids = ticketIds });

            using (var content = new StringContent(jsonBody, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage resp;
                try
                {
                    resp = await _httpClient.PostAsync(EXPO_RECEIPTS_URL, content, cancellationToken);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("[ProcessPendingReceipts Warning]: " + ex.Message);
                    return 0;
                }

                if (!resp.IsSuccessStatusCode) return 0;

                string respText = await resp.Content.ReadAsStringAsync();
                try
                {
                    var resultObj = JObject.Parse(respText);
                    var dataObj = resultObj["data"] as JObject;
                    if (dataObj == null) return 0;

                    using (var conn = new OracleConnection(connStr))
                    {
                        await conn.OpenAsync(cancellationToken);

                        foreach (var prop in dataObj.Properties())
                        {
                            string tid = prop.Name;
                            if (!ticketJobs.TryGetValue(tid, out long outboxId)) continue;

                            var receipt = prop.Value as JObject;
                            if (receipt == null) continue;

                            string rStatus = receipt["status"]?.ToString();
                            if (string.Equals(rStatus, "ok", StringComparison.OrdinalIgnoreCase))
                            {
                                string updSql = @"
                                    UPDATE TB_PUSH_OUTBOX
                                    SET STATUS = 'RECEIPT_OK',
                                        RECEIPT_STATUS = 'ok',
                                        RECEIPT_CHECKED_AT = SYSDATE
                                    WHERE ID = :p_id";

                                using (var cmdUpd = new OracleCommand(updSql, conn))
                                {
                                    cmdUpd.BindByName = true;
                                    cmdUpd.Parameters.Add("p_id", outboxId);
                                    await cmdUpd.ExecuteNonQueryAsync(cancellationToken);
                                }
                            }
                            else
                            {
                                string rError = receipt["details"]?["error"]?.ToString();
                                string rMsg = receipt["message"]?.ToString();

                                if (string.Equals(rError, "DeviceNotRegistered", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Tìm token và vô hiệu hóa
                                    string tokSql = "SELECT EXPO_PUSH_TOKEN FROM TB_PUSH_OUTBOX WHERE ID = :p_id";
                                    string tok = null;
                                    using (var cmdTok = new OracleCommand(tokSql, conn))
                                    {
                                        cmdTok.BindByName = true;
                                        cmdTok.Parameters.Add("p_id", outboxId);
                                        tok = (await cmdTok.ExecuteScalarAsync(cancellationToken))?.ToString();
                                    }
                                    if (!string.IsNullOrWhiteSpace(tok))
                                    {
                                        DeviceRegistrationService.DeactivateToken(tok, "Receipt DeviceNotRegistered");
                                    }
                                }

                                string updSql = @"
                                    UPDATE TB_PUSH_OUTBOX
                                    SET RECEIPT_STATUS = 'error',
                                        RECEIPT_ERROR = :p_err,
                                        LAST_ERROR = :p_msg,
                                        RECEIPT_CHECKED_AT = SYSDATE
                                    WHERE ID = :p_id";

                                using (var cmdUpd = new OracleCommand(updSql, conn))
                                {
                                    cmdUpd.BindByName = true;
                                    cmdUpd.Parameters.Add("p_err", (object)rError ?? "RECEIPT_ERROR");
                                    cmdUpd.Parameters.Add("p_msg", (object)rMsg ?? DBNull.Value);
                                    cmdUpd.Parameters.Add("p_id", outboxId);
                                    await cmdUpd.ExecuteNonQueryAsync(cancellationToken);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("[ProcessPendingReceipts Parse Warning]: " + ex.Message);
                }
            }

            return ticketJobs.Count;
        }
    }
}
