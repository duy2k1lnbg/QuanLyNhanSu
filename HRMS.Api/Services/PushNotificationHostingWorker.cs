using Bu.CLASS_SYSTEM;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace HRMS_API.Services
{
    public class PushNotificationHostingWorker : IRegisteredObject
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly object _lock = new object();
        private bool _isShuttingDown = false;
        private static PushNotificationHostingWorker _instance;

        public static PushNotificationHostingWorker Instance => _instance;

        public static void Start()
        {
            if (_instance == null)
            {
                _instance = new PushNotificationHostingWorker();
                HostingEnvironment.RegisterObject(_instance);
                Task.Run(() => _instance.RunLoopAsync(_instance._cts.Token));
                System.Diagnostics.Trace.TraceInformation("[PushNotificationHostingWorker] Worker started and registered with HostingEnvironment.");
            }
        }

        public static void Shutdown()
        {
            _instance?.Stop(true);
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                if (_isShuttingDown) return;
                _isShuttingDown = true;
            }

            try
            {
                _cts.Cancel();
            }
            catch { }

            HostingEnvironment.UnregisterObject(this);
            System.Diagnostics.Trace.TraceInformation("[PushNotificationHostingWorker] Worker stopped gracefully.");
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            DateTime lastReceiptCheck = DateTime.UtcNow;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 1. Xử lý hàng đợi Outbox gửi tới Expo
                    await PushNotificationOutboxProcessor.ProcessPendingOutboxAsync(100, token);

                    // 2. Định kỳ kiểm tra receipts (mỗi 5 phút)
                    if ((DateTime.UtcNow - lastReceiptCheck).TotalMinutes >= 5)
                    {
                        await PushNotificationOutboxProcessor.ProcessPendingReceiptsAsync(300, token);
                        lastReceiptCheck = DateTime.UtcNow;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("[PushNotificationHostingWorker Error]: " + ex.Message);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
