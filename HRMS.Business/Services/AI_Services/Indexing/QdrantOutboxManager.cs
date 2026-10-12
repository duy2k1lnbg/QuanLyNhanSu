using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Interfaces;
using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Bu.Services.AI_Services.Vector
{
    public enum OutboxActionType
    {
        SyncEmployee,
        RemoveEmployee,
        AddText
    }

    public class OutboxMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public OutboxActionType Action { get; set; }
        public int? EntityId { get; set; }
        public string Text { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
        public DateTime NextRetryTime { get; set; } = DateTime.UtcNow;
        public string LastError { get; set; }
        public long? OracleEventId { get; set; }
    }

    public class ReconciliationResult
    {
        public int TotalEmployeesScanned { get; set; }
        public int EnqueuedForSync { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
    }

    public class OutboxSyncStatus
    {
        public int LocalPendingCount { get; set; }
        public int LocalDeadLetterCount { get; set; }
        public int? OraclePendingCount { get; set; }
        public int? OracleLeasedCount { get; set; }
        public int? OracleProcessedCount { get; set; }
        public int? OracleDeadLetterCount { get; set; }
        public bool OracleOutboxAvailable { get; set; }
        public DateTime AsOf { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Quản trị hàng đợi Outbox Pattern và tiến trình đối soát định kỳ cho Qdrant Vector Service.
    /// Hỗ trợ cả Oracle PKG_AI_INDEX (V1_26) và hàng đợi cục bộ bền vững có Dead-Letter Storage.
    /// </summary>
    public class QdrantOutboxManager
    {
        private static readonly Lazy<QdrantOutboxManager> _lazy = new Lazy<QdrantOutboxManager>(() => new QdrantOutboxManager());
        public static QdrantOutboxManager Instance => _lazy.Value;

        private readonly ConcurrentQueue<OutboxMessage> _queue = new ConcurrentQueue<OutboxMessage>();
        private readonly List<OutboxMessage> _deadLetters = new List<OutboxMessage>();
        private readonly object _fileLock = new object();
        private readonly object _deadLetterLock = new object();
        private readonly string _storageFilePath;
        private readonly string _deadLetterFilePath;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const int MaxRetries = 5;

        public QdrantOutboxManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? AppDomain.CurrentDomain.RelativeSearchPath ?? ".";
            string dataDir = Path.Combine(baseDir, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                try { Directory.CreateDirectory(dataDir); } catch { }
            }
            _storageFilePath = Path.Combine(dataDir, "qdrant_outbox_queue.json");
            _deadLetterFilePath = Path.Combine(dataDir, "qdrant_outbox_deadletter.json");

            LoadQueueFromDisk();
            LoadDeadLettersFromDisk();

            // Khởi động background worker xử lý hàng đợi retry
            Task.Run(() => ProcessOutboxLoopAsync(_cts.Token));
        }

        public void EnqueueEmployeeSync(int manv)
        {
            long? dbEventId = TryEnqueueOracleEvent("EMPLOYEE", manv.ToString(), "SYNC");
            var msg = new OutboxMessage
            {
                Action = OutboxActionType.SyncEmployee,
                EntityId = manv,
                OracleEventId = dbEventId
            };
            _queue.Enqueue(msg);
            PersistQueueToDisk();
        }

        public void EnqueueEmployeeRemove(int manv)
        {
            long? dbEventId = TryEnqueueOracleEvent("EMPLOYEE", manv.ToString(), "REMOVE");
            var msg = new OutboxMessage
            {
                Action = OutboxActionType.RemoveEmployee,
                EntityId = manv,
                OracleEventId = dbEventId
            };
            _queue.Enqueue(msg);
            PersistQueueToDisk();
        }

        public void EnqueueTextAdd(string text, string tag)
        {
            var msg = new OutboxMessage
            {
                Action = OutboxActionType.AddText,
                Text = text,
                Tag = tag
            };
            _queue.Enqueue(msg);
            PersistQueueToDisk();
        }

        public int PendingCount => _queue.Count;
        public int DeadLetterCount
        {
            get
            {
                lock (_deadLetterLock) { return _deadLetters.Count; }
            }
        }

        private async Task ProcessOutboxLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 1. Kiểm tra claim các event từ Oracle DB nếu có
                    await TryProcessOracleLeasedEventsAsync(token);

                    // 2. Xử lý hàng đợi local
                    if (_queue.IsEmpty)
                    {
                        await Task.Delay(5000, token);
                        continue;
                    }

                    if (_queue.TryDequeue(out var msg))
                    {
                        if (msg.NextRetryTime > DateTime.UtcNow)
                        {
                            // Chưa tới thời gian retry theo Exponential Backoff -> cho lại vào queue
                            _queue.Enqueue(msg);
                            await Task.Delay(2000, token);
                            continue;
                        }

                        bool success = await ProcessMessageAsync(msg);
                        if (!success)
                        {
                            msg.RetryCount++;
                            if (msg.RetryCount < MaxRetries)
                            {
                                // Exponential backoff: 5s, 15s, 45s, 135s...
                                double delaySec = 5 * Math.Pow(3, msg.RetryCount - 1);
                                msg.NextRetryTime = DateTime.UtcNow.AddSeconds(delaySec);
                                _queue.Enqueue(msg);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[QDRANT OUTBOX] Tin nhắn {msg.Id} vượt quá {MaxRetries} lần retry -> Chuyển vào Dead-Letter. Lỗi: {msg.LastError}");
                                SaveToDeadLetter(msg);
                                if (msg.OracleEventId.HasValue)
                                {
                                    TryFailOracleEvent(msg.OracleEventId.Value, "MAX_RETRIES", msg.LastError ?? "Exceeded maximum retry attempts");
                                }
                            }
                        }
                        else
                        {
                            if (msg.OracleEventId.HasValue)
                            {
                                TryAckOracleEvent(msg.OracleEventId.Value, msg.EntityId?.ToString() ?? msg.Id, 2);
                            }
                        }

                        PersistQueueToDisk();
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QDRANT OUTBOX WORKER EXCEPTION]: {ex.Message}");
                    await Task.Delay(5000, token);
                }
            }
        }

        private async Task<bool> ProcessMessageAsync(OutboxMessage msg)
        {
            try
            {
                var vectorService = AiServiceLocator.GetService<IVectorService>() as QdrantService;
                if (vectorService == null) return false;

                switch (msg.Action)
                {
                    case OutboxActionType.SyncEmployee:
                        if (msg.EntityId.HasValue)
                        {
                            await vectorService.SyncEmployeeDataAsync(msg.EntityId.Value);
                        }
                        break;

                    case OutboxActionType.RemoveEmployee:
                        if (msg.EntityId.HasValue)
                        {
                            await vectorService.RemoveByEmployeeIdAsync(msg.EntityId.Value);
                        }
                        break;

                    case OutboxActionType.AddText:
                        if (!string.IsNullOrWhiteSpace(msg.Text))
                        {
                            await vectorService.AddAsync(msg.Text, msg.Tag ?? "GENERAL");
                        }
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                msg.LastError = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[QDRANT OUTBOX PROCESS FAILED]: {ex.Message}");
                return false;
            }
        }

        public async Task<OutboxSyncStatus> GetSyncStatusAsync()
        {
            var status = new OutboxSyncStatus
            {
                LocalPendingCount = _queue.Count,
                LocalDeadLetterCount = DeadLetterCount
            };

            try
            {
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 3;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_INDEX.READ_SYNC_STATUS";
                        var pCur = new OracleParameter("p_status_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pCur);
                        await cmd.ExecuteNonQueryAsync();

                        using (var reader = ((OracleRefCursor)pCur.Value).GetDataReader())
                        {
                            if (reader.Read())
                            {
                                status.OraclePendingCount = Convert.ToInt32(reader["PENDING_COUNT"]);
                                status.OracleLeasedCount = Convert.ToInt32(reader["LEASED_COUNT"]);
                                status.OracleProcessedCount = Convert.ToInt32(reader["PROCESSED_COUNT"]);
                                status.OracleDeadLetterCount = Convert.ToInt32(reader["DEAD_LETTER_COUNT"]);
                                status.OracleOutboxAvailable = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                status.OracleOutboxAvailable = false;
            }

            return status;
        }

        private long? TryEnqueueOracleEvent(string sourceCode, string entityKey, string action)
        {
            try
            {
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 3;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_INDEX.ENQUEUE_EVENT";
                        cmd.Parameters.Add(new OracleParameter("p_source_code", OracleDbType.Varchar2) { Value = sourceCode });
                        cmd.Parameters.Add(new OracleParameter("p_entity_key", OracleDbType.Varchar2) { Value = entityKey });
                        cmd.Parameters.Add(new OracleParameter("p_action", OracleDbType.Varchar2) { Value = action });
                        cmd.Parameters.Add(new OracleParameter("p_source_version", OracleDbType.Decimal) { Value = 1 });
                        cmd.ExecuteNonQuery();
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Oracle Outbox Enqueue Skip]: {ex.Message}");
                return null;
            }
        }

        private void TryAckOracleEvent(long eventId, string indexId, long version)
        {
            try
            {
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 3;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_INDEX.ACK_EVENT";
                        cmd.Parameters.Add(new OracleParameter("p_event_id", OracleDbType.Decimal) { Value = eventId });
                        cmd.Parameters.Add(new OracleParameter("p_index_id", OracleDbType.Varchar2) { Value = indexId ?? "DEFAULT" });
                        cmd.Parameters.Add(new OracleParameter("p_version", OracleDbType.Decimal) { Value = version });
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void TryFailOracleEvent(long eventId, string errorCode, string errorMsg)
        {
            try
            {
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 3;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_INDEX.FAIL_EVENT";
                        cmd.Parameters.Add(new OracleParameter("p_event_id", OracleDbType.Decimal) { Value = eventId });
                        cmd.Parameters.Add(new OracleParameter("p_error_code", OracleDbType.Varchar2) { Value = errorCode ?? "ERROR" });
                        cmd.Parameters.Add(new OracleParameter("p_error_msg", OracleDbType.Varchar2) { Value = errorMsg ?? "Unknown error" });
                        cmd.Parameters.Add(new OracleParameter("p_max_attempts", OracleDbType.Decimal) { Value = MaxRetries });
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private async Task TryProcessOracleLeasedEventsAsync(CancellationToken token)
        {
            try
            {
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync(token);
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 3;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_INDEX.CLAIM_EVENTS";
                        cmd.Parameters.Add(new OracleParameter("p_worker_id", OracleDbType.Varchar2) { Value = Environment.MachineName });
                        cmd.Parameters.Add(new OracleParameter("p_batch_size", OracleDbType.Decimal) { Value = 10 });
                        cmd.Parameters.Add(new OracleParameter("p_lease_seconds", OracleDbType.Decimal) { Value = 60 });
                        var pCur = new OracleParameter("p_events_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pCur);
                        await cmd.ExecuteNonQueryAsync(token);

                        using (var reader = ((OracleRefCursor)pCur.Value).GetDataReader())
                        {
                            var vectorService = AiServiceLocator.GetService<IVectorService>() as QdrantService;
                            while (reader.Read())
                            {
                                long eventId = Convert.ToInt64(reader["EVENT_ID"]);
                                string action = reader["ACTION"]?.ToString();
                                string key = reader["ENTITY_KEY"]?.ToString();
                                if (int.TryParse(key, out int manv) && vectorService != null)
                                {
                                    try
                                    {
                                        if (action == "SYNC") await vectorService.SyncEmployeeDataAsync(manv);
                                        else if (action == "REMOVE") await vectorService.RemoveByEmployeeIdAsync(manv);
                                        TryAckOracleEvent(eventId, key, 2);
                                    }
                                    catch (Exception ex)
                                    {
                                        TryFailOracleEvent(eventId, "SYNC_ERROR", ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // PKG_AI_INDEX có thể chưa được cấu hình hoặc quyền chưa cấp; fallback hoạt động bình thường
            }
        }

        private void SaveToDeadLetter(OutboxMessage msg)
        {
            lock (_deadLetterLock)
            {
                try
                {
                    _deadLetters.Add(msg);
                    string json = JsonConvert.SerializeObject(_deadLetters, Formatting.Indented);
                    File.WriteAllText(_deadLetterFilePath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QDRANT DEAD LETTER WRITE ERROR]: {ex.Message}");
                }
            }
        }

        private void LoadDeadLettersFromDisk()
        {
            lock (_deadLetterLock)
            {
                try
                {
                    if (File.Exists(_deadLetterFilePath))
                    {
                        string json = File.ReadAllText(_deadLetterFilePath);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            var list = JsonConvert.DeserializeObject<List<OutboxMessage>>(json);
                            if (list != null)
                            {
                                _deadLetters.Clear();
                                _deadLetters.AddRange(list);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QDRANT DEAD LETTER LOAD ERROR]: {ex.Message}");
                }
            }
        }

        public async Task<ReconciliationResult> ReconcileAllEmployeesAsync()
        {
            var result = new ReconciliationResult();
            try
            {
                List<int> employeeIds = new List<int>();
                using (var db = new AiEntities())
                {
                    employeeIds = db.V_AI_EMPLOYEE.Select(e => (int)e.MANV).Distinct().ToList();
                }

                result.TotalEmployeesScanned = employeeIds.Count;

                foreach (var manv in employeeIds)
                {
                    EnqueueEmployeeSync(manv);
                    result.EnqueuedForSync++;
                }

                result.Message = $"Đã lên lịch đối soát thành công {result.EnqueuedForSync}/{result.TotalEmployeesScanned} nhân viên.";
            }
            catch (Exception ex)
            {
                result.Message = $"Lỗi khi đối soát: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[QDRANT RECONCILIATION ERROR]: {ex.Message}");
            }

            return await Task.FromResult(result);
        }

        private void PersistQueueToDisk()
        {
            lock (_fileLock)
            {
                try
                {
                    var list = _queue.ToList();
                    string json = JsonConvert.SerializeObject(list, Formatting.Indented);
                    File.WriteAllText(_storageFilePath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QDRANT OUTBOX WRITE FILE ERROR]: {ex.Message}");
                }
            }
        }

        private void LoadQueueFromDisk()
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(_storageFilePath))
                    {
                        string json = File.ReadAllText(_storageFilePath);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            var list = JsonConvert.DeserializeObject<List<OutboxMessage>>(json);
                            if (list != null)
                            {
                                foreach (var item in list.Where(x => x.RetryCount < MaxRetries))
                                {
                                    _queue.Enqueue(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QDRANT OUTBOX LOAD FILE ERROR]: {ex.Message}");
                }
            }
        }
    }
}
