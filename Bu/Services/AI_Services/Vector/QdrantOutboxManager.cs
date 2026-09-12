using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Interfaces;
using DA;
using Newtonsoft.Json;

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
    }

    public class ReconciliationResult
    {
        public int TotalEmployeesScanned { get; set; }
        public int EnqueuedForSync { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
    }

    /// <summary>
    /// Quản trị hàng đợi Outbox Pattern và tiến trình đối soát định kỳ cho Qdrant Vector Service.
    /// Giúp bảo đảm dữ liệu đồng bộ không bị mất khi Qdrant service bị treo hoặc gián đoạn mạng.
    /// </summary>
    public class QdrantOutboxManager
    {
        private static readonly Lazy<QdrantOutboxManager> _lazy = new Lazy<QdrantOutboxManager>(() => new QdrantOutboxManager());
        public static QdrantOutboxManager Instance => _lazy.Value;

        private readonly ConcurrentQueue<OutboxMessage> _queue = new ConcurrentQueue<OutboxMessage>();
        private readonly object _fileLock = new object();
        private readonly string _storageFilePath;
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

            LoadQueueFromDisk();

            // Khởi động background worker xử lý hàng đợi retry
            Task.Run(() => ProcessOutboxLoopAsync(_cts.Token));
        }

        public void EnqueueEmployeeSync(int manv)
        {
            var msg = new OutboxMessage
            {
                Action = OutboxActionType.SyncEmployee,
                EntityId = manv
            };
            _queue.Enqueue(msg);
            PersistQueueToDisk();
        }

        public void EnqueueEmployeeRemove(int manv)
        {
            var msg = new OutboxMessage
            {
                Action = OutboxActionType.RemoveEmployee,
                EntityId = manv
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

        private async Task ProcessOutboxLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
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
                                System.Diagnostics.Debug.WriteLine($"[QDRANT OUTBOX] Tin nhắn {msg.Id} vượt quá {MaxRetries} lần retry. Hủy bỏ. Lỗi: {msg.LastError}");
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

        /// <summary>
        /// Job đối soát toàn diện dữ liệu nhân viên từ Oracle sang Qdrant
        /// </summary>
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
